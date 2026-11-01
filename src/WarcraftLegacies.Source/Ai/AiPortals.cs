using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.46: PSEUDO-PORTALS (design: CONCEPTS.md 7). Bots no longer click waygates. A chain that crosses a portal edge
  // ends in front of the waygate and the unit gets a TICKET (waygate, exit, final destination). One enter-region
  // trigger per waygate area fires when a unit actually walks in -- so a unit is never teleported from somewhere else.
  // It teleports only a living unit whose ticket names THIS waygate (passers-by, human units, stale tickets: ignored),
  // then gives it the rest of the way from the exit at once. A unit already inside the area when it gets the ticket
  // gets no enter event: it is teleported on issue (AiRoutes.Chain). Real waygates stay for human players.
  // Why: the real teleport dropped the queued chain (1089 units idle at exits, 0.45.0), and no bot crossed the Old Gods
  // rift in 510 min (0.45.0) -- every WL / AI waygate went through units clicking it.
  // "-pportal off" = 0.45 (chains click the waygate).
  internal static class AiPortals
  {
    internal static bool On = true;
    private const float Area = 220f;        // half side of the square entry area around a waygate (trigger)
    private const float AreaIssue = 500f;   // on issue: a unit this close is teleported at once (stragglers, crowds)
    internal const float Approach = 150f;   // the chain's last point, in front of the waygate (0.46 part 2: 400 --
                                            // units vanished too far from the asset, maintainer's smoke test)
    private const int OrderAttack = 851983;
    private const string FxIn = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportCaster.mdl";
    private const string FxOut = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportTarget.mdl";

    private static trigger _enter;
    private static Dictionary<region, unit> _gateOf = new();
    private static HashSet<unit> _watched = new();
    private static Dictionary<unit, unit> _ticket = new();     // unit -> waygate it should take
    private static Dictionary<unit, float> _exitX = new();
    private static Dictionary<unit, float> _exitY = new();
    private static Dictionary<unit, float> _destX = new();
    private static Dictionary<unit, float> _destY = new();
    private static Dictionary<unit, float> _fxAt = new();      // waygate -> last effect time (one per second)
    private static int _depth;
    internal static int Teleports;
    internal static int OnIssue;
    internal static int Ignored;
    internal static int Cleaned;

    // ---- entry areas -------------------------------------------------------------------------------------------

    // called for the waygate of every portal edge (AiRoutes.AddPortalEdge); once per waygate
    internal static void Watch(unit gate)
    {
      if (gate == null || _watched.Contains(gate))
      {
        return;
      }

      if (_enter == null)
      {
        _enter = CreateTrigger();
        TriggerAddAction(_enter, OnEnter);
      }

      _watched.Add(gate);
      float gx = GetUnitX(gate), gy = GetUnitY(gate);
      var box = Rect(gx - Area, gy - Area, gx + Area, gy + Area);
      var reg = CreateRegion();
      RegionAddRect(reg, box);
      RemoveRect(box);
      _gateOf[reg] = gate;
      TriggerRegisterEnterRegion(_enter, reg, null);
    }

    // on issue (AiRoutes.Chain): close enough to teleport now -- the enter event will not come
    internal static bool Inside(unit u, unit gate)
    {
      var dx = GetUnitX(u) - GetUnitX(gate);
      var dy = GetUnitY(u) - GetUnitY(gate);
      return dx <= AreaIssue && dx >= -AreaIssue && dy <= AreaIssue && dy >= -AreaIssue;
    }

    // ---- tickets -----------------------------------------------------------------------------------------------

    internal static void SetTicket(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      _ticket[u] = gate;
      _exitX[u] = ex;
      _exitY[u] = ey;
      _destX[u] = dx;
      _destY[u] = dy;
    }

    private static void Drop(unit u)
    {
      _ticket.Remove(u);
      _exitX.Remove(u);
      _exitY.Remove(u);
      _destX.Remove(u);
      _destY.Remove(u);
    }

    // every 30 s (AiRoutes.CheckPortals): forget tickets of dead / removed units. Collect first, then remove -- the
    // result does not depend on the dictionary's iteration order (desync-safe).
    internal static void Clean()
    {
      var dead = new List<unit>();
      foreach (var kv in _ticket)
      {
        if (GetUnitTypeId(kv.Key) == 0 || GetUnitState(kv.Key, UNIT_STATE_LIFE) <= 0.405f)
        {
          dead.Add(kv.Key);
        }
      }

      for (var i = 0; i < dead.Count; i++)
      {
        Drop(dead[i]);
        Cleaned++;
      }
    }

    // ---- teleport ----------------------------------------------------------------------------------------------

    private static void OnEnter()
    {
      try
      {
        var u = GetTriggerUnit();
        if (!_ticket.ContainsKey(u))
        {
          return; // not a bot unit on its way through a portal (the common case)
        }

        var reg = GetTriggeringRegion();
        if (!_gateOf.ContainsKey(reg) || _gateOf[reg] != _ticket[u])
        {
          Ignored++; // walked into ANOTHER waygate's area: not its portal
          return;
        }

        Teleport(u, _ticket[u], _exitX[u], _exitY[u], _destX[u], _destY[u]);
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("TICK ERROR in pseudo-portal: " + e.Message);
      }
    }

    // from AiRoutes.Chain: the unit stands in the area already. False = not now (nested teleports): walk in instead.
    internal static bool TeleportNow(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      if (_depth > 1)
      {
        return false;
      }

      OnIssue++;
      Teleport(u, gate, ex, ey, dx, dy);
      return true;
    }

    private static void Teleport(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      Drop(u);
      if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || gate == null
          || GetUnitState(gate, UNIT_STATE_LIFE) <= 0.405f)
      {
        Ignored++;
        return;
      }

      var now = SimpleBot.Clock;
      var last = _fxAt.ContainsKey(gate) ? _fxAt[gate] : -10f;
      if (now - last >= 1f)
      {
        _fxAt[gate] = now;
        DestroyEffect(AddSpecialEffect(FxIn, GetUnitX(u), GetUnitY(u)));
        DestroyEffect(AddSpecialEffect(FxOut, ex, ey));
      }

      SetUnitPosition(u, ex, ey); // the engine picks the nearest free spot
      Teleports++;
      _depth++;
      if (!AiRoutes.Chain(GetOwningPlayer(u), u, dx, dy))
      {
        AiScheduler.CountOrder(u, dx, dy);
        IssuePointOrderById(u, OrderAttack, dx, dy);
      }

      _depth--;
    }

    // ---- the Dark Portal ---------------------------------------------------------------------------------------
    // WL's Fel Horde quest "The Dark Portal" (turn 10, or research R02C at Hellfire Citadel, never before turn 8) shows
    // and activates three preplaced waygate pairs (n036, Neutral Hostile). Active neutral waygates are portal edges
    // anyway (AiRoutes.CheckPortals) -- but the 0.45.0 dump at 540 min had none. If WL made them VISIBLE (= opened)
    // and still no live edge exists, one pair is added here, in both directions. Positions: WL's preplaced units
    // (Blasted Lands side / Outland side) and their destinations (WL's Regions.Dark_Portal_Exit_2 / _Entrance_2).
    private const float DpEkX = 16736f;
    private const float DpEkY = -19680f;
    private const float DpOutX = 4704f;
    private const float DpOutY = -25376f;
    private const float DpToOutX = 4352f;
    private const float DpToOutY = -25504f;
    private const float DpToEkX = 16752f;
    private const float DpToEkY = -19680f;
    private static unit _dpEk;
    private static unit _dpOut;
    private static bool _dpSearched;
    internal static string DarkPortalState = "not checked";

    // ---- 0.48: bot-only portals at WL's spots (no visible waygates; maintainer's spec) ---------------------------
    // A HIDDEN, inactive marker (n036 without its blocking aura) is the "gate" of each one: the pseudo-portal trigger
    // works on its area like on any waygate; no human can use it, nothing shows. Open conditions follow WL's gates.
    //  - Tomb of Sargeras -> Argus: ONE-WAY, closed until the Tomb is open (shared quest, gate "tomb").
    //  - Old Gods rift, both ways: Maw of Gor'ma <-> Twilight Highlands (gate "oldgods", AiGates). Replaces the bot's
    //    visible rift waygates; WL's preplaced portal in the Twilight Highlands stays.
    private const float TombX = -736f;   // Regions.Sargeras_Entrance (the Tomb visit spot)
    private const float TombY = 8336f;
    private const float ArgusX = AiWorld.LegionArgusPortalX; // Argus landing (open ground)
    private const float ArgusY = AiWorld.LegionArgusPortalY;
    private static unit _tombMarker;
    private static unit _riftMaw;
    private static unit _riftHigh;
    internal static string BotPortals = "-";

    private static unit Marker(unit m, float x, float y)
    {
      if (m != null && GetUnitTypeId(m) != 0 && GetUnitState(m, UNIT_STATE_LIFE) > 0.405f)
      {
        return m;
      }

      m = CreateUnit(Player(27), AiWorld.PortalUnitId, x, y, 0f);
      UnitRemoveAbility(m, 1093677389); // A01M "Anti Portal Blocking" (1000-range immolation)
      UnitRemoveAbility(m, 1093677619); // A023
      WaygateActivate(m, false);          // not a live waygate: CheckPortals skips it
      SetUnitInvulnerable(m, true);
      ShowUnit(m, false);
      return m;
    }

    private static void AddBotPortals()
    {
      BotPortals = "";
      if (SimpleBot.GateOpen("tomb"))
      {
        _tombMarker = Marker(_tombMarker, TombX, TombY);
        if (AiRoutes.AddPortalEdge(_tombMarker, ArgusX, ArgusY))
        {
          BotPortals = BotPortals + " tomb->Argus";
        }
      }

      if (SimpleBot.GateOpen("oldgods"))
      {
        _riftMaw = Marker(_riftMaw, AiConfig.RiftMawX, AiConfig.RiftMawY);
        _riftHigh = Marker(_riftHigh, AiConfig.RiftHighlandsX, AiConfig.RiftHighlandsY);
        if (AiRoutes.AddPortalEdge(_riftMaw, AiConfig.RiftHighlandsX, AiConfig.RiftHighlandsY))
        {
          BotPortals = BotPortals + " rift->Highlands";
        }

        if (AiRoutes.AddPortalEdge(_riftHigh, AiConfig.RiftMawX, AiConfig.RiftMawY))
        {
          BotPortals = BotPortals + " rift->Maw";
        }
      }

      if (BotPortals == "")
      {
        BotPortals = " none open";
      }
    }

    internal static void AddStatic()
    {
      AddBotPortals();
      AddLegionTeleporters();
      if (!_dpSearched)
      {
        _dpSearched = true;
        _dpEk = FindWaygate(DpEkX, DpEkY);
        _dpOut = FindWaygate(DpOutX, DpOutY);
      }

      if (_dpEk == null || _dpOut == null)
      {
        DarkPortalState = "waygates not found";
        return;
      }

      if (IsUnitHidden(_dpEk) && IsUnitHidden(_dpOut))
      {
        DarkPortalState = "closed (WL waygates hidden)";
        return;
      }

      if (AiRoutes.IsPortalEdgeGate(_dpEk) || AiRoutes.IsPortalEdgeGate(_dpOut))
      {
        DarkPortalState = "open (live waygate edges)";
        return;
      }

      var a = AiRoutes.AddPortalEdge(_dpEk, DpToOutX, DpToOutY);
      var b = AiRoutes.AddPortalEdge(_dpOut, DpToEkX, DpToEkY);
      DarkPortalState = "open (added by the bot: " + (a ? "EK->Outland " : "") + (b ? "Outland->EK" : "") + ")";
    }

    // by owner, not by range: WL keeps the Dark Portal waygates HIDDEN until it opens them, and range searches skip
    // hidden units (0.46 part 2 smoke test: "waygates not found")
    private static unit FindWaygate(float x, float y)
    {
      unit found = null;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Player(24), null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var dx = GetUnitX(u) - x;
        var dy = GetUnitY(u) - y;
        if (found == null && GetUnitTypeId(u) == AiWorld.PortalUnitId && dx * dx + dy * dy < 300f * 300f)
        {
          found = u;
        }
      }

      DestroyGroup(g);
      return found;
    }

    // ---- 0.48.2: WL's Legion Teleporters as PSEUDO-PORTALS (maintainer: no spell casts, like the other portals) --
    // WL's Argus exit: the teleporter buildings (n0BE, Legion-owned) -- for humans a spell after the Legion's "Argus"
    // quest. For bots: once that quest is complete, each teleporter is a one-way pseudo-portal to WL's landing
    // (the one near 22939,-29345 -> Northrend 3587,20680; the other -> Alterac 11000,6424). Owner + allies only
    // (player-owned gate). 0.48.1 cast the spells -- the Legion never left Argus that way.
    private const int TeleporterType = 1848656453; // n0BE
    internal static string LegionTeleporterState = "-";
    private static QuestData _argusQuest;

    private static void AddLegionTeleporters()
    {
      player legion = null;
      for (var i = 0; i < 24 && legion == null; i++)
      {
        var f = Player(i).GetPlayerData().Faction;
        if (f != null && f.Name == "Legion")
        {
          legion = Player(i);
          if (_argusQuest == null)
          {
            _argusQuest = QuestAutoComplete.FindQuest(f, "Argus");
          }
        }
      }

      if (legion == null)
      {
        LegionTeleporterState = "no Legion";
        return;
      }

      if (_argusQuest == null || _argusQuest.Progress != QuestProgress.Complete)
      {
        LegionTeleporterState = "closed (quest Argus not complete)";
        return;
      }

      var n = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, legion, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) != TeleporterType || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var dx = GetUnitX(u) - 22939f;
        var dy = GetUnitY(u) - (-29345f);
        var north = dx * dx + dy * dy < 300f * 300f;
        if (AiRoutes.AddPortalEdge(u, north ? 3587f : 11000f, north ? 20680f : 6424f))
        {
          n++;
        }
      }

      DestroyGroup(g);
      LegionTeleporterState = "open (" + I2S(n) + " edges)";
    }

    // ---- commands and dump -------------------------------------------------------------------------------------

    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg == "-pportal on")
      {
        On = true;
      }
      else if (msg == "-pportal off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "portals: "
        + (On ? "PSEUDO-PORTALS (bots teleport by trigger, 0.46)" : "units click the waygates (0.45)")
        + "  -- -pportal on|off");
    }

    internal static void Lines(List<string> o)
    {
      o.Add("pseudo-portals " + (On ? "on" : "off") + ": teleports " + I2S(Teleports) + " (on issue " + I2S(OnIssue)
            + "), ignored " + I2S(Ignored) + ", open tickets " + I2S(_ticket.Count) + ", cleaned " + I2S(Cleaned)
            + ", watched waygates " + I2S(_watched.Count));
      // 0.48.2: own line -- the dump cuts long lines (these parts were lost in 0.48.1)
      o.Add("portal states: Dark Portal " + DarkPortalState + "; bot portals:" + BotPortals + "; Legion teleporters "
            + LegionTeleporterState);

      // WL's own waygates (not the AI pairs of Neutral Extra): where, owner, active, hidden, destination
      var g = CreateGroup();
      for (var pl = 24; pl < 28; pl++)
      {
        if (pl == 26)
        {
          continue;
        }

        GroupEnumUnitsOfPlayer(g, Player(pl), null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (GetUnitTypeId(u) != AiWorld.PortalUnitId && !WaygateIsActive(u))
          {
            continue;
          }

          o.Add("  WL waygate " + GetUnitName(u) + " @" + I2S(R2I(GetUnitX(u))) + "," + I2S(R2I(GetUnitY(u)))
                + " P" + I2S(pl) + " active=" + (WaygateIsActive(u) ? "1" : "0") + " hidden="
                + (IsUnitHidden(u) ? "1" : "0") + " -> " + I2S(R2I(WaygateGetDestinationX(u))) + ","
                + I2S(R2I(WaygateGetDestinationY(u))) + (AiRoutes.IsPortalEdgeGate(u) ? " edge" : " no-edge"));
        }
      }

      DestroyGroup(g);
    }
  }
}
