using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.Quests;
using MacroTools.Legends;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai;

// All inspection / measurement lives here, separate from the logic in AiSystem.cs.
//  * Master switch (0.35): Access below -- Off / HostOnly / Everyone. Off is a release build with NO AI chat command
//    (except -autopilot, which then runs in fair mode) and NO display work at
//    all. The op counters in AiSystem stay (they're near-free and always on); only the display is gated.
//  * Per-player, default OFF -> zero screen spam until someone opts in. Toggles are independent:
//      -debug1  per-bot state     (army / landmass / candidates / has-target)
//      -debug2  load profiler      (combat ops per tick + peak, orders, production peak; 0.40: scheduler line)
//      -debug3  targeting decision (what each bot is doing: atk/def/gather/hold)
//      -xy      selected unit's X/Y + landmass id (one-shot)
//      -fac     each bot's faction + buildable category counts (one-shot)
//      -help    lists every command
//  * v28 AI-development tools (all display-only / local file output -> cannot desync):
//      -dump        write a full snapshot (load, bots, waves, event log, node + edge audit) to
//                   Documents\Warcraft III\CustomMapData\WLAI_dump_<time>.txt  (paste that file to the assistant)
//      -autodump    toggle: write a dump every 5 min for you (WLAI_dump_05m00s.txt, WLAI_dump_10m00s.txt, ...)
//      -why [pid]   decision trace on screen: one bot in detail, or every bot in one line each
//      -edges       toggle: draw every graph edge as a lightning line, coloured by state (only you see it)
//  * IMPORTANT: the op counters measure the BOT LOGIC only, never this display. So an on-screen number is a
//    pessimistic reading -- the real (debug-off) game always has more headroom than what you see here.
//  * Op counts are DETERMINISTIC (identical on every machine), so displaying them can't desync. They are a
//    load *proxy* (units/candidates touched, orders issued), not wall-clock ms -- the right metric for
//    lockstep, and the one to watch: MaxOpUnits in the heaviest single tick, full-bots lategame.
public static class AiDebug
{
  // ============================================================================================================
  // 0.35: THE ONE SWITCH FOR ALL AI CHAT COMMANDS -- check it before publishing a map (see README).
  //   Off      = release: no AI command at all; -autopilot still works but FAIR (no bot bonuses).
  //   HostOnly = read-only commands for everyone; game-changing ones only for the host (lowest human slot).
  //   Everyone = every command for every player (testing).
  // Heavy display commands (-debug1..4, -edges, -ids, -autodump) only work when a single human is in the game.
  // ============================================================================================================
  public enum CommandAccess
  {
    Off,
    HostOnly,
    Everyone,
  }

  public const CommandAccess Access = CommandAccess.Everyone;

  private enum Level
  {
    Read,
    Heavy,
    Control,
  }

  private const float ShowEvery = 3.0f;
  private const float AutoDumpEvery = 300f;
  private const int PreloadChunk = 180; // keep every Preload() line short; longer lines are wrapped with "  ~"

  private static readonly Dictionary<int, bool> On1 = new();
  private static readonly Dictionary<int, bool> On2 = new();
  private static readonly Dictionary<int, bool> On3 = new();
  private static readonly Dictionary<int, bool> On4 = new();

  public static void Setup()
  {
    // 0.39.9: -version works in every build (also Access = Off): read-only, shows which map and bot build is running.
    Register("-version", OnVersion, Level.Read);

    if (Access == CommandAccess.Off)
    {
      return;
    }

    Register("-debug1", () => Toggle(On1, "state"), Level.Heavy);
    Register("-debug2", () => Toggle(On2, "load"), Level.Heavy);
    Register("-debug3", () => Toggle(On3, "decisions"), Level.Heavy);
    Register("-debug4", () => Toggle(On4, "economy"), Level.Heavy);
    Register("-xy", OnXy, Level.Read);
    Register("-fac", OnFac, Level.Read);
    Register("-legion", OnLegion, Level.Control);
    Register("-ids", OnIds, Level.Heavy);
    Register("-id", OnId, Level.Read);
    Register("-portals", OnPortals, Level.Read);
    Register("-heroes", OnHeroes, Level.Read);    // 0.36
    RegisterLoose("-tp", OnTp, Level.Heavy);      // 0.36: single-human games only (selection is not synced)
    RegisterLoose("-icontest", OnIconTest, Level.Heavy); // 0.39: "-icontest N" = one variant on your unit
    RegisterLoose("-orders", OnOrders, Level.Control);   // 0.39: -orders idle|all (refactor lever 1)
    RegisterLoose("-budget", AiScheduler.OnBudgetCommand, Level.Control); // 0.40: -budget on|off (load budget)
    RegisterLoose("-route", AiRoutes.OnRouteCommand, Level.Control);      // 0.42: -route on|off (route planner)
    RegisterLoose("-chain", AiRoutes.OnChainCommand, Level.Control);      // 0.44: -chain on|off (movement chains)
    RegisterLoose("-creeps", AiRoutes.OnCreepsCommand, Level.Control);    // 0.44.3: -creeps on|off (creep camps)
    RegisterLoose("-gather", AiRoutes.OnGatherCommand, Level.Control);    // 0.45.0: -gather on|off (gather points)
    RegisterLoose("-pportal", AiPortals.OnCommand, Level.Control);        // 0.46: -pportal on|off (pseudo-portals)
    RegisterLoose("-guard", AiGuard.OnCommand, Level.Control);            // 0.46: -guard on|off|ab|ab2
    RegisterLoose("-cap", AiBudget.OnCommand, Level.Control);             // 0.46: -cap on|off|<base> <perCp>
    Register("-ogrift", OnOgRift, Level.Control);   // 0.37 (replaces -ogwave)
    Register("-gold0", () => SetGold(0), Level.Control);
    Register("-gold50", () => SetGold(50), Level.Control);
    Register("-gold100", () => SetGold(100), Level.Control);
    Register("-gold150", () => SetGold(150), Level.Control);
    Register("-gold200", () => SetGold(200), Level.Control);
    Register("-help", OnHelp, Level.Read);
    Register("-dump", () => WriteDump(GetTriggerPlayer(), "manual"), Level.Read);
    Register("-autodump", OnAutoDump, Level.Heavy);
    Register("-edges", OnEdges, Level.Heavy);
    RegisterLoose("-why", OnWhy, Level.Read);
    Register("-ship", OnShipTest, Level.Control);
    Register("-botbreak", SimpleBot.OnBotBreak, Level.Control); // 0.35: moved here from SimpleBot (now switched)
    Register("-prod", SimpleBot.OnProd, Level.Control);
    Register("-atk", SimpleBot.OnAtk, Level.Control);

    var t = CreateTimer();
    TimerStart(t, ShowEvery, true, Show);
    var ad = CreateTimer();
    TimerStart(ad, AutoDumpEvery, true, OnAutoDumpTick);
    var hb = CreateTimer();
    TimerStart(hb, HeartbeatEvery, true, OnHeartbeat); // 0.39: freeze diagnosis, only while -autodump is on
  }

  // "-orders smart" (default since 0.41: attack group + staging, AiArmy) / "-orders idle" (0.39-0.40: whole army,
  // heartbeat re-orders idle units) / "-orders all" (before 0.39) -- switchable for A/B tests
  private static void OnOrders()
  {
    var msg = GetEventPlayerChatString();
    if (msg.Length >= 13 && msg.Substring(0, 13) == "-orders smart")
    {
      AiArmy.Smart = true;
    }
    else if (msg.Length >= 11 && msg.Substring(0, 11) == "-orders all")
    {
      AiArmy.Smart = false;
      SimpleBot.OrdersIdleOnly = false;
    }
    else if (msg.Length >= 12 && msg.Substring(0, 12) == "-orders idle")
    {
      AiArmy.Smart = false;
      SimpleBot.OrdersIdleOnly = true;
    }

    var mode = SimpleBot.OrdersMode();
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "army orders: "
      + (mode == "smart" ? "SMART (attack group, rest at the staging CP, only idle units away from their point)"
        : mode == "idle" ? "IDLE (0.40: whole army, idle units re-ordered)" : "ALL (old: whole army, every unit)")
      + "  -- -orders smart|idle|all");
  }

  // 0.39: heartbeat file for freeze diagnosis. While -autodump is on, every 10 s: game time, units on the map
  // (counted every 60 s), orders per minute, the last started periodic tasks. After a freeze, its time and content
  // show what ran last. Written only on the autodump player's machine (Preload, desync-safe like the dump).
  private const float HeartbeatEvery = 10f;
  private static int _hbMapUnits;
  private static float _hbCountedAt = -999f;
  private static int _hbLastOrders;
  private static float _hbLastOrdersAt;
  private static int _hbOrdersPerMin;

  private static void OnHeartbeat()
  {
    if (_autoDumpFor == null)
    {
      return;
    }

    if (SimpleBot.Clock - _hbCountedAt >= 60f)
    {
      _hbCountedAt = SimpleBot.Clock;
      _hbMapUnits = CountMapUnits();
      var dt = SimpleBot.Clock - _hbLastOrdersAt;
      _hbOrdersPerMin = dt > 0f ? R2I((SimpleBot.OrdersIssued - _hbLastOrders) * 60f / dt) : 0;
      _hbLastOrders = SimpleBot.OrdersIssued;
      _hbLastOrdersAt = SimpleBot.Clock;
    }

    if (GetLocalPlayer() != _autoDumpFor)
    {
      return;
    }

    PreloadGenClear();
    PreloadGenStart();
    Preload("WLAI heartbeat build=" + AiSetup.Build + " t=" + FileTime(SimpleBot.Clock) + " units-on-map=" + I2S(_hbMapUnits)
            + " orders/min=" + I2S(_hbOrdersPerMin) + " orders=" + SimpleBot.OrdersMode());
    for (var i = 0; i < SimpleBot.TaskRing.Length; i++)
    {
      var e = SimpleBot.TaskRing[(SimpleBot.TaskRingPos + i) % SimpleBot.TaskRing.Length];
      if (e != null)
      {
        Preload("task " + e);
      }
    }

    PreloadGenEnd("WLAI_heartbeat.txt");
  }

  private static int CountMapUnits()
  {
    var n = 0;
    var g = CreateGroup();
    GroupEnumUnitsInRect(g, GetWorldBounds(), null);
    unit u;
    while ((u = FirstOfGroup(g)) != null)
    {
      GroupRemoveUnit(g, u);
      n++;
    }

    DestroyGroup(g);
    return n;
  }

  // 0.39: dump section UNITS -- units per player (army / structures), total on the map, orders per minute
  private static void UnitLines(List<string> o)
  {
    var total = 0;
    var line = "";
    for (var i = 0; i < 28; i++)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Player(i), null);
      var army = 0;
      var buildings = 0;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
        {
          buildings++;
        }
        else
        {
          army++;
        }
      }

      DestroyGroup(g);
      total += army + buildings;
      if (army + buildings > 0)
      {
        line = line + " P" + I2S(i) + "=" + I2S(army) + "/" + I2S(buildings);
      }
    }

    o.Add("## UNITS (per player: units/structures; 24 NeutralHostile, 25-27 neutral)  total " + I2S(total)
          + " | orders total " + I2S(SimpleBot.OrdersIssued) + ", mode " + SimpleBot.OrdersMode());
    o.Add(line);
  }

  // Like Register, but matches any chat line CONTAINING cmd (so "-why 3" works).
  private static void RegisterLoose(string cmd, System.Action act, Level level)
  {
    var tr = CreateTrigger();
    for (var i = 0; i < 24; i++)
    {
      TriggerRegisterPlayerChatEvent(tr, Player(i), cmd, false);
    }

    TriggerAddAction(tr, () => RunCommand(cmd, act, level));
  }

  private static void Register(string cmd, System.Action act, Level level)
  {
    var tr = CreateTrigger();
    for (var i = 0; i < 24; i++)
    {
      TriggerRegisterPlayerChatEvent(tr, Player(i), cmd, true);
    }

    TriggerAddAction(tr, () => RunCommand(cmd, act, level));
  }

  // humans in the game (playing, user-controlled) and the "host" = lowest such slot (WC3 can't see the real host)
  public static int HumanCount()
  {
    var n = 0;
    for (var i = 0; i < 24; i++)
    {
      if (GetPlayerSlotState(Player(i)) == PLAYER_SLOT_STATE_PLAYING && GetPlayerController(Player(i)) == MAP_CONTROL_USER)
      {
        n++;
      }
    }

    return n;
  }

  public static bool IsHost(player p)
  {
    for (var i = 0; i < 24; i++)
    {
      if (GetPlayerSlotState(Player(i)) == PLAYER_SLOT_STATE_PLAYING && GetPlayerController(Player(i)) == MAP_CONTROL_USER)
      {
        return Player(i) == p;
      }
    }

    return false;
  }

  private static string Denied(Level level, player p)
  {
    if (level == Level.Heavy && HumanCount() > 1)
    {
      return "only in single-player games (it costs every player performance)";
    }

    if (level == Level.Control && Access == CommandAccess.HostOnly && !IsHost(p))
    {
      return "only the host may use it in this build";
    }

    if (level == Level.Heavy && Access == CommandAccess.HostOnly && !IsHost(p))
    {
      return "only the host may use it in this build";
    }

    return null;
  }

  // v34.2: a failing command reports its error to the player instead of silently doing nothing.
  // 0.35: and a command the access switch doesn't allow says so.
  private static void RunCommand(string cmd, System.Action act, Level level)
  {
    var why = Denied(level, GetTriggerPlayer());
    if (why != null)
    {
      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "|cffffcc00" + cmd + "|r: " + why);
      return;
    }

    try
    {
      act();
    }
    catch (System.Exception e)
    {
      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 15, "|cffff4444AI command " + cmd + " failed:|r " + e.Message);
    }
  }

  private static void Toggle(Dictionary<int, bool> set, string label)
  {
    var pid = GetPlayerId(GetTriggerPlayer());
    var on = !(set.ContainsKey(pid) && set[pid]);
    set[pid] = on;
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 6, "debug " + label + (on ? " ON" : " OFF"));
  }

  private static bool IsOn(Dictionary<int, bool> set, int pid)
  {
    return set.ContainsKey(pid) && set[pid];
  }

  private static void Show()
  {
    if (Bolts.Count > 0)
    {
      RecolorBolts(); // edge states change as CPs flip; recolor only (no new handles)
    }

    for (var i = 0; i < 24; i++)
    {
      var pl = Player(i);
      if (GetPlayerSlotState(pl) != PLAYER_SLOT_STATE_PLAYING || GetPlayerController(pl) != MAP_CONTROL_USER)
      {
        continue;
      }

      if (IsOn(On1, i))
      {
        DisplayTimedTextToPlayer(pl, 0, 0, ShowEvery + 0.5f, StateLine());
      }

      if (IsOn(On2, i))
      {
        DisplayTimedTextToPlayer(pl, 0, 0, ShowEvery + 0.5f, LoadLine());
      }

      if (IsOn(On3, i))
      {
        DisplayTimedTextToPlayer(pl, 0, 0, ShowEvery + 0.5f, DecisionLine());
      }

      if (IsOn(On4, i))
      {
        DisplayTimedTextToPlayer(pl, 0, 0, ShowEvery + 0.5f, EconLine());
      }
    }
  }

  private static string StateLine()
  {
    var s = "|cff00ffffAI|r bots=" + I2S(SimpleBot.BotCount);
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      var pid = GetPlayerId(SimpleBot.BotAt(b));
      var hasT = SimpleBot.Committed.ContainsKey(pid) && SimpleBot.Committed[pid] != null ? 1 : 0;
      s = s + "  P" + I2S(pid)
          + " a" + I2S(GetI(SimpleBot.BotArmy, pid))
          + " l" + I2S(GetI(SimpleBot.BotLm, pid))
          + " c" + I2S(GetI(SimpleBot.Cand, pid))
          + " t" + I2S(hasT);
    }

    return s;
  }

  private static string LoadLine()
  {
    return "|cffffcc00LOAD|r combat units last=" + I2S(SimpleBot.LastOpUnits)
           + " max=" + I2S(SimpleBot.MaxOpUnits)
           + " orders=" + I2S(SimpleBot.LastOpOrders)
           + " def=" + I2S(SimpleBot.LastDefOps) + "/" + I2S(SimpleBot.MaxDefOps)
           + "  |  prod peak=" + I2S(SimpleBot.MaxProdOps)
           + "  |  wave cycle=" + I2S(SimpleBot.LastWaveOps) + "/" + I2S(SimpleBot.MaxWaveOps)
           + "\n" + AiScheduler.ShortLine() // 0.40
           + "\n" + AiArmy.ShortLine();     // 0.41
  }

  private static string EconLine()
  {
    var s = "|cff44ff44ECON|r";
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      var pl = SimpleBot.BotAt(b);
      s = s + "  P" + I2S(GetPlayerId(pl))
          + " g" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_GOLD))
          + " f" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_FOOD_USED))
          + "/" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_FOOD_CAP));
    }

    return s;
  }

  private static string DecisionLine()
  {
    var s = "|cffff88ffDECIDE|r";
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      var pid = GetPlayerId(SimpleBot.BotAt(b));
      s = s + "  P" + I2S(pid) + ":" + GetS(SimpleBot.BotState, pid) + " " + GetS(SimpleBot.BotTargetName, pid);
    }

    return s;
  }

  private static int GetI(Dictionary<int, int> d, int k)
  {
    return d.ContainsKey(k) ? d[k] : 0;
  }

  private static string GetS(Dictionary<int, string> d, int k)
  {
    return d.ContainsKey(k) ? d[k] : "-";
  }

  // Force-summon the Legion for testing (normally a late milestone). Completes its SummonLegion quest,
  // which unlocks the Nether Pit + Summoning Circle, rescues its units, and opens the portals.
  private static void OnLegion()
  {
    AiLegion.SummonLegion(); // completes the quest AND spawns a fresh portal pair at the configured endpoints
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8,
      "|cffff5555Legion force-summoned. Use -portals to find the exit.|r");
  }

  // Float a label + ping at the Legion portal endpoints so they can be located even if the model is unclear.
  // v34.1: label + ping every AI portal that currently exists (ship and Argus portals).
  private static void OnPortals()
  {
    var n = 0;
    for (var i = 0; i < AiLegion.PortalPairCount; i++)
    {
      n += MarkUnit(AiLegion.PortalA(i)) + MarkUnit(AiLegion.PortalB(i));
    }

    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 10, "|cffffcc00-portals|r: " + I2S(n)
      + " AI portal ends labelled + pinged for 30s (" + I2S(AiLegion.PortalPairCount) + " pairs; graphs merged="
      + B(SimpleBot.GraphMerged) + ")");
  }

  // 0.36: every bot hero -- label (name, level, owner, mission / stuck state) + ping for 30 s, and a chat list.
  // 0.44.2: pings only heroes you can see on the map -- WL parks some heroes HIDDEN off the playable map ("lm-1"
  // lines at -23657,-15923 etc.); they were listed and pinged as if alive. One line per bot (the one-line-per-hero
  // list did not fit on screen with 16 bots); dead heroes are counted, not pinged.
  private static void OnHeroes()
  {
    var p = GetTriggerPlayer();
    var all = LegendaryHeroManager.GetAll();
    var lines = new string[24];
    var dead = new int[24];
    var n = 0;
    for (var i = 0; i < all.Count; i++)
    {
      var hu = all[i].Unit;
      if (hu == null || GetUnitTypeId(hu) == 0 || !SimpleBot.IsBot(GetOwningPlayer(hu)))
      {
        continue;
      }

      var pid = GetPlayerId(GetOwningPlayer(hu));
      if (pid < 0 || pid >= 24)
      {
        continue;
      }

      if (GetUnitState(hu, UNIT_STATE_LIFE) <= 0.405f)
      {
        dead[pid]++;
        continue;
      }

      if (IsUnitHidden(hu))
      {
        continue;
      }

      var st = AiState.MissionUnits.Contains(hu) ? "mission" : AiHeroCare.Status(hu);
      var x = GetUnitX(hu);
      var y = GetUnitY(hu);
      Mark(GetUnitName(hu) + " L" + I2S(GetHeroLevel(hu)) + " P" + I2S(pid) + (st != "" ? " [" + st + "]" : ""), x, y);
      lines[pid] = (lines[pid] == null ? "" : lines[pid] + ", ") + GetUnitName(hu) + " L" + I2S(GetHeroLevel(hu))
                   + (st != "" ? " |cffffcc00" + st + "|r" : "");
      n++;
    }

    for (var pid = 0; pid < 24; pid++)
    {
      if (lines[pid] == null && dead[pid] == 0)
      {
        continue;
      }

      DisplayTimedTextToPlayer(p, 0, 0, 30, "P" + I2S(pid) + " " + FacOf(Player(pid)) + ": "
        + (lines[pid] ?? "-") + (dead[pid] > 0 ? "  |cffff4040dead " + I2S(dead[pid]) + "|r" : ""));
    }

    DisplayTimedTextToPlayer(p, 0, 0, 10, "|cffffcc00-heroes|r: " + I2S(n) + " living bot heroes marked for 30 s");
  }

  // 0.38.1: the AI portals' minimap icons did not show in the 0.38 test. Six variants in a row eastwards from your
  // selected unit (2500 apart, number labels in the world) -- report which numbers appear on the minimap.
  private static void OnIconTest()
  {
    var p = GetTriggerPlayer();
    var g = CreateGroup();
    GroupEnumUnitsSelected(g, p, null);
    var u = FirstOfGroup(g);
    DestroyGroup(g);
    if (u == null)
    {
      DisplayTimedTextToPlayer(p, 0, 0, 8, "|cffffcc00-icontest|r: select a unit first");
      return;
    }

    var paths = new[]
    {
      "UI\\Minimap\\MinimapIcon\\MinimapIconNeutralBuilding.blp", "UI\\Minimap\\MinimapIcon\\MinimapIconNeutralBuilding.blp",
      "UI\\Minimap\\MiniMap-ControlPoint.mdl", "UI\\Minimap\\Minimap-QuestObjectivePrimary.mdl",
      "UI\\Minimap\\MiniMap-Hero.mdl", "UI\\Minimap\\MinimapIcon\\MinimapIconGoldmine.blp",
    };
    // 0.39: one variant per call, ON the selected unit: "-icontest 1" ... "-icontest 6"
    var msg = GetEventPlayerChatString();
    var n = msg.Length >= 11 ? S2I(msg.Substring(10, 1)) : 0;
    if (n < 1 || n > paths.Length)
    {
      DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00-icontest N|r (N = 1..6): one minimap icon on your selected unit. "
        + "1 current portal icon, 2 same with fog VISIBLE, 3 ControlPoint.mdl, 4 QuestObjectivePrimary.mdl, 5 Hero.mdl, "
        + "6 Goldmine.blp. Tell which numbers show.");
      return;
    }

    CreateMinimapIcon(GetUnitX(u), GetUnitY(u), 170, 0, 255, paths[n - 1], n == 2 ? FOG_OF_WAR_VISIBLE : FOG_OF_WAR_MASKED);
    DisplayTimedTextToPlayer(p, 0, 0, 10, "|cffffcc00-icontest|r: variant " + I2S(n) + " placed on your selected unit -- "
      + "look at the minimap.");
  }

  // 0.36: "-tp x y" -- move your selected units there (testing; Heavy = single-human games, selection isn't synced)
  private static void OnTp()
  {
    var p = GetTriggerPlayer();
    var msg = GetEventPlayerChatString();
    var rest = msg.Length > 4 && msg.Substring(0, 4) == "-tp " ? msg.Substring(4) : "";
    var sp = -1;
    for (var i = 1; i < rest.Length && sp < 0; i++) // only Substring/Length -- safest in CSharp.lua
    {
      if (rest.Substring(i, 1) == " ")
      {
        sp = i;
      }
    }

    if (sp <= 0)
    {
      DisplayTimedTextToPlayer(p, 0, 0, 8, "|cffffcc00-tp|r: usage -tp <x> <y>, e.g. -tp -13580 8448");
      return;
    }

    var x = S2R(rest.Substring(0, sp));
    var y = S2R(rest.Substring(sp + 1));
    var g = CreateGroup();
    GroupEnumUnitsSelected(g, p, null);
    var n = 0;
    unit u;
    while ((u = FirstOfGroup(g)) != null)
    {
      GroupRemoveUnit(g, u);
      if (!IsUnitType(u, UNIT_TYPE_STRUCTURE))
      {
        SetUnitPosition(u, x, y);
        n++;
      }
    }

    DestroyGroup(g);
    DisplayTimedTextToPlayer(p, 0, 0, 8, "|cffffcc00-tp|r: " + I2S(n) + " unit(s) -> " + I2S(R2I(x)) + "," + I2S(R2I(y))
      + " landmass " + I2S(Geography.Landmass(x, y)) + (IsTerrainPathable(x, y, PATHING_TYPE_WALKABILITY) ? " (NOT walkable)" : ""));
  }

  private static int MarkUnit(unit u)
  {
    if (u == null || GetUnitTypeId(u) == 0)
    {
      return 0;
    }

    Mark(GetUnitName(u), GetUnitX(u), GetUnitY(u));
    return 1;
  }

  private static void Mark(string label, float x, float y)
  {
    var tt = CreateTextTag();
    SetTextTagText(tt, label, 0.022f);
    SetTextTagPos(tt, x, y, 96f);
    SetTextTagColor(tt, 255, 120, 120, 255);
    SetTextTagVisibility(tt, true);
    SetTextTagPermanent(tt, false);
    SetTextTagLifespan(tt, 30f);
    SetTextTagFadepoint(tt, 27f);
    PingMinimap(x, y, 30f);
  }

  // Float every control point's id + name above it for 30s, so you can read ids off the map.
  private static void OnIds()
  {
    var cps = ControlPointManager.Instance.GetAllControlPoints();
    for (var i = 0; i < cps.Count; i++)
    {
      var cp = cps[i];
      var tt = CreateTextTag();
      SetTextTagText(tt, I2S(cp.UnitType) + " " + cp.Name, 0.020f);
      SetTextTagPos(tt, GetUnitX(cp.Unit), GetUnitY(cp.Unit), 64f);
      SetTextTagColor(tt, 255, 255, 0, 255);
      SetTextTagVisibility(tt, true);
      SetTextTagPermanent(tt, false);
      SetTextTagLifespan(tt, 30f);
      SetTextTagFadepoint(tt, 27f);
    }

    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "|cffffcc00-ids|r: control-point ids shown for 30s.");
  }

  private static void SetGold(int amount)
  {
    SimpleBot.BotGoldBonus = amount;
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 6, "bot gold bonus = " + I2S(amount));
  }

  private static void OnOgRift()
  {
    AiGates.ForceOldGodsRift();
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "|cffaa00ffOld Gods rift forced open.|r");
  }

  // Report the identity of the selected unit: type id as decimal AND 4-char rawcode (so FourCC byte-order
  // mistakes are obvious), plus name, owner and position. Read-only -> lockstep-safe, fine to leave in.
  private static void OnId()
  {
    var p = GetTriggerPlayer();
    var g = CreateGroup();
    GroupEnumUnitsSelected(g, p, null);
    var u = FirstOfGroup(g);
    DestroyGroup(g);
    if (u == null)
    {
      DisplayTimedTextToPlayer(p, 0, 0, 10, "|cffffcc00-id|r: select a unit first (destructibles aren't reported).");
      return;
    }

    var tid = GetUnitTypeId(u);
    var owner = GetOwningPlayer(u);
    DisplayTimedTextToPlayer(p, 0, 0, 30,
      "|cffffcc00-id|r " + RawCode(tid) + " = " + I2S(tid)
      + "  '" + GetUnitName(u) + "'"
      + "  owner=P" + I2S(GetPlayerId(owner))
      + "  @ " + I2S(R2I(GetUnitX(u))) + "," + I2S(R2I(GetUnitY(u))));
  }

  // Decode a big-endian FourCC int back to its 4 characters (non-printable bytes shown as '?').
  private static string RawCode(int id)
  {
    var s = "";
    for (var shift = 24; shift >= 0; shift -= 8)
    {
      var b = (id >> shift) & 0xFF;
      s = s + (b >= 32 && b <= 126 ? "" + (char)b : "?");
    }

    return s;
  }

  private static void OnXy()
  {
    var p = GetTriggerPlayer();
    var g = CreateGroup();
    GroupEnumUnitsSelected(g, p, null);
    var u = FirstOfGroup(g);
    DestroyGroup(g);
    if (u == null)
    {
      DisplayTimedTextToPlayer(p, 0, 0, 8, "|cffffcc00-xy|r: select a unit first.");
      return;
    }

    var x = GetUnitX(u);
    var y = GetUnitY(u);
    DisplayTimedTextToPlayer(p, 0, 0, 30,
      "|cffffcc00X=" + I2S(R2I(x)) + " Y=" + I2S(R2I(y)) + "|r landmass=" + I2S(Geography.Landmass(x, y)));
  }

  private static int CatCount(Faction f, UnitCategory cat)
  {
    if (f.TryGetObjectByCategory(cat, out var ids) && ids != null)
    {
      return ids.Count;
    }

    return 0;
  }

  private static string CatTok(Faction f, string label, UnitCategory cat)
  {
    var n = CatCount(f, cat);
    return n > 0 ? " " + label + I2S(n) : "";
  }

  private static void OnFac()
  {
    var p = GetTriggerPlayer();
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      var bot = SimpleBot.BotAt(b);
      var pid = GetPlayerId(bot);
      var faction = bot.GetPlayerData().Faction;
      if (faction == null)
      {
        continue;
      }

      var line = "|cff00ffffP" + I2S(pid) + " " + faction.Name + "|r u:"
                 + CatTok(faction, "Fi", UnitCategory.Fighter)
                 + CatTok(faction, "Ma", UnitCategory.Marksman)
                 + CatTok(faction, "Si", UnitCategory.Siege)
                 + CatTok(faction, "Ta", UnitCategory.Tank)
                 + CatTok(faction, "De", UnitCategory.Destroyer)
                 + CatTok(faction, "Fl", UnitCategory.Flyer)
                 + CatTok(faction, "Su", UnitCategory.Support)
                 + CatTok(faction, "El", UnitCategory.Elite)
                 + "  b:"
                 + CatTok(faction, "Ba", UnitCategory.Barracks)
                 + CatTok(faction, "Sw", UnitCategory.SiegeWorkshop)
                 + CatTok(faction, "Mg", UnitCategory.Magic)
                 + CatTok(faction, "Fb", UnitCategory.FlyingBuilding)
                 + CatTok(faction, "Al", UnitCategory.Altar)
                 + CatTok(faction, "Sh", UnitCategory.Shipyard);
      DisplayTimedTextToPlayer(p, 0, 0, 60, line);
    }
  }

  // =========================================================================================================
  //  v28 AI-DEVELOPMENT TOOLS
  // =========================================================================================================

  // v34.2: -help is short now -- the full command list lives in README.md (AI section).
  private static void OnHelp()
  {
    var p = GetTriggerPlayer();
    DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00Bot AI|r " + AiSetup.Build + " (commands: " + (Access == CommandAccess.HostOnly ? "host only" : "everyone")
      + ") -- full list: README.md."
      + " Most used: -dump  -autodump  -why  -portals  -heroes  -edges  -xy  -id  -autopilot  -version");
  }

  // 0.39.9: replaces the chat banner at game start.
  private static void OnVersion()
  {
    DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 15,
      "|cffffcc00Shattered Legacies: Botified Realm|r -- Bot AI " + AiSetup.Build);
  }


  // ---- -why -------------------------------------------------------------------------------------------
  private static void OnWhy()
  {
    var p = GetTriggerPlayer();
    var msg = GetEventPlayerChatString();
    var arg = msg.Length > 5 ? msg.Substring(5) : "";
    if (arg == "")
    {
      for (var b = 0; b < SimpleBot.BotCount; b++)
      {
        var pid = GetPlayerId(SimpleBot.BotAt(b));
        var d = SimpleBot.GetDiag(pid);
        DisplayTimedTextToPlayer(p, 0, 0, 20, "P" + I2S(pid) + " " + FacOf(SimpleBot.BotAt(b))
          + " " + GetS(SimpleBot.BotState, pid) + " -> " + TargetLabel(d) + " [" + d.Reason + "]"
          + " n" + I2S(d.Neutral) + " e" + I2S(d.Enemy) + " inv" + I2S(d.Invuln) + " cl" + I2S(d.Closed));
      }

      return;
    }

    var want = S2I(arg);
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      if (GetPlayerId(SimpleBot.BotAt(b)) == want)
      {
        var lines = new List<string>();
        BotLines(want, lines, false);
        for (var i = 0; i < lines.Count; i++)
        {
          DisplayTimedTextToPlayer(p, 0, 0, 30, lines[i]);
        }

        return;
      }
    }

    DisplayTimedTextToPlayer(p, 0, 0, 10, "-why: P" + I2S(want) + " is not a bot. Use -why alone to list bots.");
  }

  // ---- -dump / -autodump ------------------------------------------------------------------------------
  private static player _autoDumpFor;

  private static void OnAutoDump()
  {
    var p = GetTriggerPlayer();
    _autoDumpFor = _autoDumpFor == p ? null : p;
    DisplayTimedTextToPlayer(p, 0, 0, 8, "autodump " + (_autoDumpFor != null ? "ON (every 5 min)" : "OFF"));
  }

  private static void OnAutoDumpTick()
  {
    if (_autoDumpFor != null)
    {
      WriteDump(_autoDumpFor, "auto");
    }
  }

  private static void WriteDump(player who, string tag)
  {
    var lines = new List<string>();
    BuildDump(lines, tag);
    var path = "WLAI_dump_" + FileTime(SimpleBot.Clock) + ".txt"; // flat name: no subfolder needed

    // The file is written ONLY on the requesting player's machine. Preload* natives create no handles and
    // change no game state, so running them inside a GetLocalPlayer() block is desync-safe (same technique
    // save/load systems use). Everything above (building the lines) runs identically on every client.
    if (GetLocalPlayer() == who)
    {
      PreloadGenClear();
      PreloadGenStart();
      for (var i = 0; i < lines.Count; i++)
      {
        var line = lines[i];
        var first = true;
        while (line.Length > PreloadChunk)
        {
          Preload((first ? "" : "  ~") + line.Substring(0, PreloadChunk));
          line = line.Substring(PreloadChunk);
          first = false;
        }

        Preload((first ? "" : "  ~") + line);
      }

      PreloadGenEnd(path);
    }

    DisplayTimedTextToPlayer(who, 0, 0, 12, "|cff88ff88dump written|r (" + I2S(lines.Count)
      + " lines) -> Documents\\Warcraft III\\CustomMapData\\" + path);
  }

  private static string FileTime(float t)
  {
    var s = R2I(t);
    var m = s / 60;
    var r = s - m * 60;
    return (m < 10 ? "0" : "") + I2S(m) + "m" + (r < 10 ? "0" : "") + I2S(r) + "s";
  }

  // v34.2: every section is isolated -- a failing section writes "## ERROR in <section>" and the file is still
  // written (a single error used to make the whole -dump silently do nothing). Pinned events + portals first.
  private static void BuildDump(List<string> o, string tag)
  {
    o.Add("=== WLAI DUMP build=" + AiSetup.Build + " t=" + SimpleBot.TimeTag(SimpleBot.Clock) + " tag=" + tag
          + " bots=" + I2S(SimpleBot.BotCount) + " goldBonus=" + I2S(SimpleBot.BotGoldBonus)
          + " botbreak=" + B(SimpleBot.BotBreak) + " prod=" + B(SimpleBot.ProdOn) + " atk=" + B(SimpleBot.AtkOn) + " ===");

    Section(o, "pinned", () =>
    {
      o.Add("## PINNED (one-off events, never pushed out)");
      for (var i = 0; i < SimpleBot.PinnedLog.Count; i++)
      {
        o.Add(SimpleBot.PinnedLog[i]);
      }
    });

    Section(o, "portals", () =>
    {
      o.Add("## PORTALS (AI waygates: name @x,y alive -> destination)  merged=" + B(SimpleBot.GraphMerged));
      for (var i = 0; i < AiLegion.PortalPairCount; i++)
      {
        o.Add(PortalLine(AiLegion.PortalA(i)) + "   <->   " + PortalLine(AiLegion.PortalB(i)));
      }
    });

    Section(o, "units", () => UnitLines(o));          // 0.39: refactor step 0 (measure)
    Section(o, "teams", () => AiStandings.Lines(o)); // 0.36
    Section(o, "trained", () => AiStats.Lines(o));   // 0.38
    Section(o, "routes", () => AiRoutes.Lines(o));   // 0.42: route planner state, target and next waypoint per bot

    Section(o, "load", () =>
    {
      o.Add("## LOAD  (units touched; last/max. combat = one bot's service; def is part of combat)");
      o.Add("combat " + I2S(SimpleBot.LastOpUnits) + "/" + I2S(SimpleBot.MaxOpUnits)
            + " orders=" + I2S(SimpleBot.LastOpOrders)
            + " | def-scan " + I2S(SimpleBot.LastDefOps) + "/" + I2S(SimpleBot.MaxDefOps)
            + " | production peak " + I2S(SimpleBot.MaxProdOps)
            + " | wave-tick " + I2S(SimpleBot.LastWaveOps) + "/" + I2S(SimpleBot.MaxWaveOps)
            + " | last gate sweep " + I2S(AiGates.LastSweepOps));
      AiArmy.Lines(o);      // 0.41: orders by reason, attack group per bot
      AiIncome.Lines(o);    // 0.44.2: CP income paid to computer slots
      AiLoad.Lines(o);      // 0.46: order latency, unit budget, guard positions A/B
      AiScheduler.Lines(o); // 0.40: scheduler, orders by distance, one line per task (by total ops)
    });

    o.Add("## BOTS");
    for (var b = 0; b < SimpleBot.BotCount; b++)
    {
      var pid = GetPlayerId(SimpleBot.BotAt(b));
      Section(o, "bot P" + I2S(pid), () => BotLines(pid, o, true));
    }

    Section(o, "waves", () =>
    {
      o.Add("## WAVES");
      for (var w = 0; w < AiWaves.All.Count; w++)
      {
        WaveLine(w, o);
      }
    });

    Section(o, "permadead", () =>
    {
      o.Add("## PERMADEAD hero types (never re-trained/revived)");
      var all = LegendaryHeroManager.GetAll();
      var pd = "";
      for (var i = 0; i < all.Count; i++)
      {
        if (SimpleBot.PermaDead.Contains(all[i].UnitType))
        {
          pd = pd + RawCode(all[i].UnitType) + " ";
        }
      }

      o.Add(pd == "" ? "(none)" : pd);
    });

    Section(o, "event log", () =>
    {
      o.Add("## EVENT LOG (oldest first, state changes only)");
      for (var i = 0; i < SimpleBot.EventLog.Count; i++)
      {
        o.Add(SimpleBot.EventLog[i]);
      }
    });

    Section(o, "nodes", () => NodeLines(o));
    Section(o, "edges C", () => EdgeLines(o, "C", AiGraph.Combined));
    Section(o, "edges K", () => EdgeLines(o, "K", AiGraph.Kalimdor));
    o.Add("=== END ===");
  }

  private static void Section(List<string> o, string name, System.Action build)
  {
    try
    {
      build();
    }
    catch (System.Exception e)
    {
      o.Add("## ERROR in " + name + ": " + e.Message);
    }
  }


  private static string B(bool v) => v ? "1" : "0";

  private static string PortalLine(unit u)
  {
    if (u == null || GetUnitTypeId(u) == 0)
    {
      return "(gone)";
    }

    return GetUnitName(u) + " @" + I2S(R2I(GetUnitX(u))) + "," + I2S(R2I(GetUnitY(u)))
           + (GetUnitState(u, UNIT_STATE_LIFE) > 0.405f ? "" : " DEAD")
           + " -> " + I2S(R2I(WaygateGetDestinationX(u))) + "," + I2S(R2I(WaygateGetDestinationY(u)));
  }

  // ---- per-bot block (shared by -dump and -why) -------------------------------------------------------
  private static void BotLines(int pid, List<string> o, bool withHeroes)
  {
    var pl = Player(pid);
    var d = SimpleBot.GetDiag(pid);
    o.Add("P" + I2S(pid) + " " + FacOf(pl)
          + " st=" + GetS(SimpleBot.BotState, pid)
          + " set=" + d.EdgeSet
          + " army=" + I2S(GetI(SimpleBot.BotArmy, pid))
          + " gold=" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_GOLD))
          + " food=" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_FOOD_USED))
          + "/" + I2S(GetPlayerState(pl, PLAYER_STATE_RESOURCE_FOOD_CAP))
          + " anchor=" + (d.AnchorIsBase ? "base" : "army") + "@" + I2S(R2I(d.AnchorX)) + "," + I2S(R2I(d.AnchorY))
          + "(" + I2S(d.AnchorUnits) + "u)"
          + " lm=" + I2S(GetI(SimpleBot.BotLm, pid)) + " homeLm=" + I2S(d.HomeLm));

    // army by landmass (non-structure, non-worker, alive) -> exposes units stranded on another continent
    var lms = new List<int>();
    var cnt = new List<int>();
    var sx = new List<float>();
    var sy = new List<float>();
    float tx = 0f, ty = 0f;
    var tn = 0;
    var g = CreateGroup();
    GroupEnumUnitsOfPlayer(g, pl, null);
    unit u;
    while ((u = FirstOfGroup(g)) != null)
    {
      GroupRemoveUnit(g, u);
      if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON)
          || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        continue;
      }

      var x = GetUnitX(u);
      var y = GetUnitY(u);
      tx += x;
      ty += y;
      tn++;
      var lm = Geography.Landmass(x, y);
      var k = lms.IndexOf(lm);
      if (k < 0)
      {
        lms.Add(lm);
        cnt.Add(0);
        sx.Add(0f);
        sy.Add(0f);
        k = lms.Count - 1;
      }

      cnt[k] = cnt[k] + 1;
      sx[k] = sx[k] + x;
      sy[k] = sy[k] + y;
    }

    DestroyGroup(g);

    var t = d.Target;
    var tline = "  tgt=" + TargetLabel(d) + " why=" + d.Reason;
    if (t != null)
    {
      var maxLife = GetUnitState(t, UNIT_STATE_MAX_LIFE);
      var hp = maxLife > 0f ? R2I(100f * GetUnitState(t, UNIT_STATE_LIFE) / maxLife) : 0;
      tline = tline + " own=" + OwnerTag(GetOwningPlayer(t))
              + " inv=" + B(BlzIsUnitInvulnerable(t))
              + " hp=" + I2S(hp) + "%"
              + " lm=" + I2S(Geography.Landmass(GetUnitX(t), GetUnitY(t)));
      if (tn > 0)
      {
        var dx = GetUnitX(t) - tx / tn;
        var dy = GetUnitY(t) - ty / tn;
        tline = tline + " armyDist=" + I2S(R2I(SquareRoot(dx * dx + dy * dy)));
      }
    }

    tline = tline + " for=" + I2S(R2I(SimpleBot.Clock - d.TargetSince)) + "s";
    o.Add(tline);

    o.Add("  frontiers: neutral=" + I2S(d.Neutral) + " enemy=" + I2S(d.Enemy) + " locked<10m=" + I2S(d.Locked)
          + " uncapturable=" + I2S(d.Invuln) + " closed=" + I2S(d.Closed) + " skipnode=" + I2S(d.Skipped)
          + " stuck-skip=" + I2S(d.Stuck)
          + " kept=" + I2S(d.Sticky)
          + " non-cp-edges=" + I2S(d.NoCp));

    var ul = "  army by landmass:";
    for (var i = 0; i < lms.Count; i++)
    {
      ul = ul + " lm" + I2S(lms[i]) + "=" + I2S(cnt[i])
           + "@" + I2S(R2I(sx[i] / cnt[i])) + "," + I2S(R2I(sy[i] / cnt[i]));
    }

    o.Add(lms.Count == 0 ? "  army by landmass: (none)" : ul);

    if (!withHeroes)
    {
      return;
    }

    var hl = "  heroes:";
    var any = false;
    var all = LegendaryHeroManager.GetAll();
    for (var i = 0; i < all.Count; i++)
    {
      var hu = all[i].Unit;
      if (hu == null || GetUnitTypeId(hu) == 0 || GetOwningPlayer(hu) != pl)
      {
        continue;
      }

      any = true;
      hl = hl + " " + GetUnitName(hu) + "(" + RawCode(GetUnitTypeId(hu)) + ","
           + (GetUnitState(hu, UNIT_STATE_LIFE) > 0.405f ? "alive" : "DEAD")
           + (GetUnitAbilityLevel(hu, FourCC("LEgo")) > 0 ? ",perma" : "")
           + (AiState.MissionUnits.Contains(hu) ? ",mission" : "")
           + (AiHeroCare.Status(hu) != "" ? "," + AiHeroCare.Status(hu) : "") + ")";
    }

    o.Add(any ? hl : "  heroes: (none)");
  }

  private static string TargetLabel(BotDiag d)
  {
    return d.Target != null ? GetUnitName(d.Target) + "{" + RawCode(GetUnitTypeId(d.Target)) + "}" : "-";
  }

  private static string FacOf(player p)
  {
    var f = p.GetPlayerData().Faction;
    return f != null ? f.Name : "?";
  }

  private static string OwnerTag(player o)
  {
    if (o == Player(PLAYER_NEUTRAL_AGGRESSIVE))
    {
      return "NA";
    }

    if (o == Player(PLAYER_NEUTRAL_PASSIVE))
    {
      return "NP";
    }

    var id = GetPlayerId(o);
    return id < 24 ? "P" + I2S(id) + "(" + FacOf(o) + ")" : "N" + I2S(id);
  }

  // ---- waves ------------------------------------------------------------------------------------------
  private static void WaveLine(int idx, List<string> o)
  {
    var cfg = AiWaves.All[idx];
    var w = AiWaveRunner.GetWaveDiag(idx);
    var names = "";
    for (var i = 0; i < cfg.Factions.Count; i++)
    {
      names = names + (i > 0 ? "," : "") + cfg.Factions[i];
    }

    if (!w.HomeOk)
    {
      o.Add("W" + I2S(idx) + " [" + names + "] home CP " + I2S(cfg.HomeCpId) + " NOT a control point -> wave can never run");
      return;
    }

    var phase = w.Phase == 0 ? "idle" : w.Phase == 1 ? "GATHER" : "CHANNEL";
    var thresholdMet = w.Total > 0 && w.Owned >= w.Need;
    var blocked = "";
    if (!thresholdMet && !w.Monuments)
    {
      blocked = blocked + " needCPs(" + I2S(w.Owned) + "/" + I2S(w.Need) + ")";
    }

    if (w.Enemies >= AiWorld.EnemyLeaveThreshold)
    {
      blocked = blocked + " enemiesOnHome(" + I2S(w.Enemies) + ">=" + I2S(AiWorld.EnemyLeaveThreshold) + ")";
    }

    if (!w.TriggerOk)
    {
      blocked = blocked + " triggerCpNotOwned";
    }

    if (!w.HasBot)
    {
      blocked = blocked + " NO-BOT-IN-GROUP(disabled)";
    }

    if (w.Active && w.Phase == 0 && w.CooldownLeft > 0f)
    {
      blocked = blocked + " cooldown(" + I2S(R2I(w.CooldownLeft)) + "s)";
    }

    o.Add("W" + I2S(idx) + " [" + names + "] phase=" + phase + " age=" + I2S(R2I(w.PhaseAge)) + "s"
          + " active=" + B(w.Active) + " homeLm=" + I2S(w.HomeLm)
          + " homeCPs=" + I2S(w.Owned) + "/" + I2S(w.Total) + " need=" + I2S(w.Need)
          + " monuments=" + B(w.Monuments) + " enemiesHome=" + I2S(w.Enemies)
          + " trigger=" + B(w.TriggerOk) + " cooldown=" + I2S(R2I(w.CooldownLeft)) + "s"
          + (blocked != "" ? " BLOCKED:" + blocked : ""));
  }

  // ---- graph audit ------------------------------------------------------------------------------------
  // Every control point on the map: where it is, which landmass the grid gives it, who owns it, whether it
  // can be captured, and which authored graph (C=Combined, K=Kalimdor, -=none) contains it.
  private static void NodeLines(List<string> o)
  {
    var inC = new HashSet<int>();
    var inK = new HashSet<int>();
    for (var i = 0; i < AiGraph.Combined.Count; i++)
    {
      inC.Add(AiGraph.Combined[i].A);
      inC.Add(AiGraph.Combined[i].B);
    }

    for (var i = 0; i < AiGraph.Kalimdor.Count; i++)
    {
      inK.Add(AiGraph.Kalimdor[i].A);
      inK.Add(AiGraph.Kalimdor[i].B);
    }

    var cps = ControlPointManager.Instance.GetAllControlPoints();
    o.Add("## NODES (" + I2S(cps.Count) + " control points)  id raw name @x,y lm owner uncapturable graph");
    for (var i = 0; i < cps.Count; i++)
    {
      var cp = cps[i];
      var cu = cp.Unit;
      if (cu == null)
      {
        o.Add(I2S(cp.UnitType) + " " + RawCode(cp.UnitType) + " " + cp.Name + " (no unit)");
        continue;
      }

      var x = GetUnitX(cu);
      var y = GetUnitY(cu);
      o.Add(I2S(cp.UnitType) + " " + RawCode(cp.UnitType) + " " + cp.Name
            + " @" + I2S(R2I(x)) + "," + I2S(R2I(y))
            + " lm=" + I2S(Geography.Landmass(x, y))
            + " own=" + OwnerTag(GetOwningPlayer(cu))
            + " unc=" + B(SimpleBot.IsUncapturable(cu))
            + " g=" + (inC.Contains(cp.UnitType) ? "C" : "") + (inK.Contains(cp.UnitType) ? "K" : "")
            + (inC.Contains(cp.UnitType) || inK.Contains(cp.UnitType) ? "" : "-"));
    }
  }

  // Every edge with the facts that decide whether a bot can actually walk it. Flags:
  //   XLM   endpoints on different landmasses (probably needs water/portal -> bots park at the shore)
  //   LM?   an endpoint reads landmass -1 (island / unresolved grid cell)
  //   NOCP  an id isn't a registered control point (SafeCp null) -> edge ignored
  //   UNC   an endpoint is uncapturable (Neutral Passive / invulnerable)
  //   T<s>  time-gated edge (open/closed now)    G:<name>  named gate (open/closed now)
  private static void EdgeLines(List<string> o, string tag, List<Edge> edges)
  {
    o.Add("## EDGES " + tag + " (" + I2S(edges.Count) + ")  A <-> B | lm a/b | own a/b | flags");
    var suspect = 0;
    for (var i = 0; i < edges.Count; i++)
    {
      var e = edges[i];
      var ca = SimpleBot.SafeCp(e.A);
      var cb = SimpleBot.SafeCp(e.B);
      var flags = "";
      if (e.OpenTime > 0f)
      {
        flags = flags + " T" + I2S(R2I(e.OpenTime)) + (e.OpenTime > SimpleBot.Clock ? "(closed)" : "(open)");
      }

      if (e.Gate != "")
      {
        flags = flags + " G:" + e.Gate + (SimpleBot.GateOpen(e.Gate) ? "(open)" : "(closed)");
      }

      if (ca == null || cb == null || ca.Unit == null || cb.Unit == null)
      {
        o.Add(tag + " " + I2S(e.A) + " <-> " + I2S(e.B) + " | NOCP" + flags);
        continue;
      }

      var la = Geography.Landmass(GetUnitX(ca.Unit), GetUnitY(ca.Unit));
      var lb = Geography.Landmass(GetUnitX(cb.Unit), GetUnitY(cb.Unit));
      if (la != lb)
      {
        flags = flags + " XLM";
      }

      if (la < 0 || lb < 0)
      {
        flags = flags + " LM?";
      }

      if (SimpleBot.IsUncapturable(ca.Unit) || SimpleBot.IsUncapturable(cb.Unit))
      {
        flags = flags + " UNC";
      }

      if (la != lb || la < 0 || lb < 0)
      {
        suspect++;
      }

      o.Add(tag + " " + ca.Name + " <-> " + cb.Name
            + " | lm " + I2S(la) + "/" + I2S(lb)
            + " | own " + OwnerTag(GetOwningPlayer(ca.Unit)) + "/" + OwnerTag(GetOwningPlayer(cb.Unit))
            + " |" + (flags == "" ? " -" : flags));
    }

    o.Add("## EDGES " + tag + ": " + I2S(suspect) + " edge(s) cross landmasses or touch landmass -1");
  }

  // ---- -edges (lightning overlay) ---------------------------------------------------------------------
  private static readonly List<lightning> Bolts = new();
  private static readonly List<Edge> BoltEdges = new();
  private static player _edgesViewer;
  private static location _zLoc;

  private static void OnEdges()
  {
    var p = GetTriggerPlayer();
    if (Bolts.Count > 0)
    {
      for (var i = 0; i < Bolts.Count; i++)
      {
        DestroyLightning(Bolts[i]);
      }

      Bolts.Clear();
      BoltEdges.Clear();
      _edgesViewer = null;
      DisplayTimedTextToPlayer(p, 0, 0, 6, "edges OFF");
      return;
    }

    _edgesViewer = p;
    DrawEdges(AiGraph.Combined);
    DrawEdges(AiGraph.Kalimdor);
    RecolorBolts();
    DisplayTimedTextToPlayer(p, 0, 0, 20, "edges ON (" + I2S(Bolts.Count) + "). green=held yellow=neutral frontier "
      + "orange=uncapturable red=contested blue=neutral magenta=crosses landmass grey=closed");
  }

  private static void DrawEdges(List<Edge> edges)
  {
    if (_zLoc == null)
    {
      _zLoc = Location(0f, 0f);
    }

    for (var i = 0; i < edges.Count; i++)
    {
      var ca = SimpleBot.SafeCp(edges[i].A);
      var cb = SimpleBot.SafeCp(edges[i].B);
      if (ca == null || cb == null || ca.Unit == null || cb.Unit == null)
      {
        continue;
      }

      var x1 = GetUnitX(ca.Unit);
      var y1 = GetUnitY(ca.Unit);
      var x2 = GetUnitX(cb.Unit);
      var y2 = GetUnitY(cb.Unit);
      // GetLocationZ is client-local terrain height -> only ever used for visuals, never logic.
      MoveLocation(_zLoc, x1, y1);
      var z1 = GetLocationZ(_zLoc) + 90f;
      MoveLocation(_zLoc, x2, y2);
      var z2 = GetLocationZ(_zLoc) + 90f;
      // v30: "CLSB" uses the near-white Lightning.blp, so the tint shows true colours. ("DRAL" was a green
      // texture: tinted yellow looked green, red looked dark -> the "different greens" on the overlay.)
      Bolts.Add(AddLightningEx("CLSB", false, x1, y1, z1, x2, y2, z2));
      BoltEdges.Add(edges[i]);
    }
  }

  private static void RecolorBolts()
  {
    // Only the player who toggled -edges sees them (alpha computed locally; no handle work inside).
    var a = GetLocalPlayer() == _edgesViewer ? 1f : 0f;
    for (var i = 0; i < Bolts.Count; i++)
    {
      var c = EdgeColor(BoltEdges[i]);
      float r = 0.5f, gr = 0.5f, b = 0.5f;               // 0 grey: closed
      if (c == 1) { r = 1f; gr = 0f; b = 1f; }           // magenta: crosses landmass
      else if (c == 2) { r = 0.2f; gr = 1f; b = 0.2f; }  // green: held (same owner / allies)
      else if (c == 3) { r = 1f; gr = 0.15f; b = 0.15f; } // red: contested between players
      else if (c == 4) { r = 1f; gr = 1f; b = 0.1f; }    // yellow: player -> capturable neutral
      else if (c == 5) { r = 1f; gr = 0.55f; b = 0f; }   // orange: player -> uncapturable neutral
      else if (c == 6) { r = 0.3f; gr = 0.45f; b = 1f; } // blue: neutral <-> neutral (faded: background)
      SetLightningColor(Bolts[i], r, gr, b, c == 6 ? a * 0.35f : a);
    }
  }

  private static int EdgeColor(Edge e)
  {
    if (e.OpenTime > SimpleBot.Clock || (e.Gate != "" && !SimpleBot.GateOpen(e.Gate)))
    {
      return 0;
    }

    var ca = SimpleBot.SafeCp(e.A);
    var cb = SimpleBot.SafeCp(e.B);
    if (ca == null || cb == null || ca.Unit == null || cb.Unit == null)
    {
      return 0;
    }

    var la = Geography.Landmass(GetUnitX(ca.Unit), GetUnitY(ca.Unit));
    var lb = Geography.Landmass(GetUnitX(cb.Unit), GetUnitY(cb.Unit));
    if (la != lb || la < 0 || lb < 0)
    {
      return 1;
    }

    var oa = GetOwningPlayer(ca.Unit);
    var ob = GetOwningPlayer(cb.Unit);
    var pa = GetPlayerId(oa) < 24;
    var pb = GetPlayerId(ob) < 24;
    if (pa && pb)
    {
      return oa == ob || IsPlayerAlly(oa, ob) ? 2 : 3;
    }

    if (pa || pb)
    {
      var neutralSide = pa ? cb.Unit : ca.Unit;
      return SimpleBot.IsUncapturable(neutralSide) ? 5 : 4;
    }

    return 6;
  }

  // ---- v33 -ship : ship-portal asset test (Kul Tiras landfall) ---------------------------------------------
  // Every call removes the previous test and shows the NEXT ship model, two ways side by side:
  //   A "skin":   the waygate unit itself re-skinned to a ship unit type (BlzSetUnitSkin)
  //   B "effect": an invisible waygate with the ship model as a special effect on top (works with any model)
  // A and B are created at (0,0) and linked to each other. With a unit selected, a second linked pair is made
  // 600/2000 units beside it on land, so walking into one ship can be tested (should land at the other).
  private static readonly string[] ShipCodes = { "hbot", "h0AN", "h0B6", "h0AR", "h060", "hdes" };
  private static readonly string[] ShipNames =
    { "Transport Ship", "Juggernaut", "Boarding Vessel", "Scout Ship", "Carrier Flagship", "Frigate (stock)" };
  private static readonly string[] ShipModels =
  {
    "war3mapImported\\Unit_HumanTransportShip.mdl",
    "war3mapImported\\Unit_HumanJuggernaut.mdl",
    "war3mapImported\\Unit_HumanBoardingShip.mdl",
    "war3mapImported\\Unit_HumanScout.mdl",
    "StormhawkShip.mdl",
    "units\\human\\HumanDestroyerShip\\HumanDestroyerShip.mdl",
  };

  private static int _shipIdx = -1;
  private static readonly List<unit> ShipUnits = new();
  private static readonly List<effect> ShipFx = new();

  private static void OnShipTest()
  {
    var p = GetTriggerPlayer();
    for (var i = 0; i < ShipUnits.Count; i++)
    {
      RemoveUnit(ShipUnits[i]);
    }

    for (var i = 0; i < ShipFx.Count; i++)
    {
      DestroyEffect(ShipFx[i]);
    }

    ShipUnits.Clear();
    ShipFx.Clear();
    _shipIdx = (_shipIdx + 1) % ShipCodes.Length;

    ShipPair(0f, 0f, 900f, 0f);

    var g = CreateGroup();
    GroupEnumUnitsSelected(g, p, null);
    var sel = FirstOfGroup(g);
    DestroyGroup(g);
    if (sel != null)
    {
      var sx = GetUnitX(sel);
      var sy = GetUnitY(sel);
      ShipPair(sx + 600f, sy, sx + 2000f, sy);
    }

    DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00-ship|r " + I2S(_shipIdx + 1) + "/" + I2S(ShipCodes.Length) + ": "
      + ShipNames[_shipIdx] + " (" + ShipCodes[_shipIdx] + ")  -- A = re-skinned waygate, B = effect on hidden waygate"
      + (sel != null ? "; land pair placed beside your selected unit" : "; select a unit first for a walkable pair"));
  }

  private static void ShipPair(float ax, float ay, float bx, float by)
  {
    var a = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), AiWorld.PortalUnitId, ax, ay, 0f);
    BlzSetUnitSkin(a, FourCC(ShipCodes[_shipIdx]));

    var b = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), AiWorld.PortalUnitId, bx, by, 0f);
    SetUnitVertexColor(b, 255, 255, 255, 0);
    var fx = AddSpecialEffect(ShipModels[_shipIdx], bx, by);
    BlzSetSpecialEffectYaw(fx, 0f);

    // v34: land BESIDE the other end -- landing exactly on a waygate can bounce units straight back, which is
    // the likely cause of the "works only one way" result.
    WaygateSetDestination(a, bx + 300f, by - 300f);
    WaygateSetDestination(b, ax + 300f, ay - 300f);
    BlzSetUnitName(a, "Ship test A (skin)");
    BlzSetUnitName(b, "Ship test B (effect)");
    WaygateActivate(a, true);
    WaygateActivate(b, true);
    ShipUnits.Add(a);
    ShipUnits.Add(b);
    ShipFx.Add(fx);
    ShipLabel("A skin", ax, ay);
    ShipLabel("B effect", bx, by);
  }

  private static void ShipLabel(string text, float x, float y)
  {
    var tt = CreateTextTag();
    SetTextTagText(tt, text, 0.022f);
    SetTextTagPos(tt, x, y, 160f);
    SetTextTagColor(tt, 120, 200, 255, 255);
    SetTextTagVisibility(tt, true);
    SetTextTagPermanent(tt, false);
    SetTextTagLifespan(tt, 60f);
    SetTextTagFadepoint(tt, 55f);
  }
}
