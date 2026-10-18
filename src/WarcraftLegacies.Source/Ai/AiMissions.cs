using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- one-off missions and scripted fixes: starter kit, Ragnaros, kill targets, area clears, one-shots, CP grants, Kul Tiras repair, palace handover, banned forms.
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiMissions
  {
    private static readonly HashSet<int> _oneShotDone = new();

    // ---- v32 starter kit: every bot owns at least one barracks and one altar --------------------------------
    // Bots never construct buildings. A faction that starts without them (Stormwind, Illidan, ...) would idle
    // until a quest rescues its base. At 0:20 and again at 3:00 (after the 2:00 starting-quest rescue) each
    // missing building is created -- the cheapest barracks / altar type of the faction -- on a worker's spot
    // (the worker is removed: "workers become buildings"), else next to the main building / home CP.
    private static readonly float[] StarterKitAt = { 20f, 180f };
    private static int _starterKitDone;

    internal static void RunStarterKit()
    {
      if (_starterKitDone >= StarterKitAt.Length || SimpleBot._clock < StarterKitAt[_starterKitDone])
      {
        return;
      }

      _starterKitDone++;
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        if (!SimpleBot.HasPerks(p))
        {
          continue; // 0.35: fair autopilot gets no starter kit
        }

        var f = p.GetPlayerData().Faction;
        if (f == null)
        {
          continue;
        }

        var barracks = new List<int>();
        SimpleBot.AddCategory(f, UnitCategory.Barracks, barracks);
        var altars = new List<int>();
        SimpleBot.AddCategory(f, UnitCategory.Altar, altars);

        var haveBarracks = false;
        var haveAltar = false;
        unit hall = null;
        var workers = new List<unit>();
        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }

          var t = GetUnitTypeId(u);
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
          {
            haveBarracks = haveBarracks || SimpleBot.ListHas(barracks, t);
            haveAltar = haveAltar || SimpleBot.ListHas(altars, t);
            if (hall == null && IsUnitType(u, UNIT_TYPE_TOWNHALL))
            {
              hall = u;
            }
          }
          else if (IsUnitType(u, UNIT_TYPE_PEON))
          {
            workers.Add(u);
          }
        }

        DestroyGroup(g);
        if (!haveBarracks)
        {
          PlaceStarter(p, Cheapest(barracks), workers, hall, "barracks");
        }

        if (!haveAltar)
        {
          PlaceStarter(p, Cheapest(altars), workers, hall, "altar");
        }
      }
    }

    private static int Cheapest(List<int> types)
    {
      var best = 0;
      var bestCost = 0;
      for (var i = 0; i < types.Count; i++)
      {
        var c = GetUnitGoldCost(types[i]);
        if (best == 0 || c < bestCost)
        {
          best = types[i];
          bestCost = c;
        }
      }

      return best;
    }

    private static void PlaceStarter(player p, int typeId, List<unit> workers, unit hall, string what)
    {
      if (typeId == 0)
      {
        return; // faction has no building of this category
      }

      // where: a worker's spot (worker removed) > ring around the main building > ring around the first CP
      var x = 0f;
      var y = 0f;
      var found = false;
      if (workers.Count > 0)
      {
        var w = workers[workers.Count - 1];
        workers.RemoveAt(workers.Count - 1);
        x = GetUnitX(w);
        y = GetUnitY(w);
        RemoveUnit(w);
        found = true;
      }

      if (!found && hall != null)
      {
        found = SimpleBot.FindOpenSpot(GetUnitX(hall), GetUnitY(hall));
        x = SimpleBot._spotX;
        y = SimpleBot._spotY;
      }

      if (!found)
      {
        var cps = p.GetPlayerData().ControlPoints;
        if (cps.Count > 0 && cps[0].Unit != null)
        {
          found = SimpleBot.FindOpenSpot(GetUnitX(cps[0].Unit), GetUnitY(cps[0].Unit));
          x = SimpleBot._spotX;
          y = SimpleBot._spotY;
        }
      }

      if (!found)
      {
        SimpleBot.Log("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": starter " + what + " -- no place found");
        return;
      }

      CreateUnit(p, typeId, x, y, 270f);
      SimpleBot.LogPin("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": starter " + what + " created ("
          + GetObjectName(typeId) + ")");
    }
    private static player _ragBot;
    private static float _ragSince;
    private static int _ragAttempts;
    private static bool _ragDone;
    private static unit _ragPedestal;
    private static bool _ragSummoned;
    private static float _ragSummonAt;
    private const float RagSummonX = 12332f; // where WL's QuestRagnaros spawns him
    private const float RagSummonY = -10597f;

    internal static void RunRagnarosMission()
    {
      if (_ragDone)
      {
        return;
      }

      if (_ragSummoned)
      {
        RagnarosKill();
        return;
      }

      if (_ragPedestal == null)
      {
        _ragPedestal = SimpleBot.FindUnitOfType(AiConfig.RagnarosPedestalId, AiConfig.RagnarosX, AiConfig.RagnarosY, 800f);
        if (_ragPedestal == null)
        {
          RagnarosEnd("pedestal not found -> mission off");
          return;
        }
      }

      if (GetUnitTypeId(_ragPedestal) == 0 || GetUnitState(_ragPedestal, UNIT_STATE_LIFE) <= 0.405f)
      {
        RagnarosEnd("pedestal already used");
        return;
      }

      var pedOwner = GetOwningPlayer(_ragPedestal);
      if (GetPlayerId(pedOwner) < 24)
      {
        if (pedOwner != _ragBot)
        {
          RagnarosEnd("pedestal taken by P" + I2S(GetPlayerId(pedOwner)) + " " + SimpleBot.FacName(pedOwner));
          return;
        }

        // phase 2: our bot owns the pedestal -- summon only with the level-12 hero standing by
        if (!RagHeroValid())
        {
          SimpleBot._ragHero = SimpleBot.BestHero(_ragBot);
          if (SimpleBot._ragHero == null || GetHeroLevel(SimpleBot._ragHero) < AiConfig.RagnarosHeroLevel)
          {
            SimpleBot._ragHero = null;
            return; // wait for a level-12 hero (revive / training / levelling)
          }
        }

        if (RagHeroReady() && IssueImmediateOrder(_ragPedestal, "selfdestruct"))
        {
          _ragSummoned = true;
          _ragSummonAt = SimpleBot._clock;
          SimpleBot.Log("Ragnaros mission: " + SimpleBot.FacName(_ragBot) + " summons Ragnaros");
          return;
        }

        RagHeroApproach();
        return;
      }

      // phase 1: travel
      if (SimpleBot._ragHero != null)
      {
        if (!RagHeroValid() || SimpleBot._clock - _ragSince > 360f)
        {
          SimpleBot.Log("Ragnaros mission: " + SimpleBot.FacName(_ragBot) + " hero " + (RagHeroValid() ? "too slow" : "lost") + " -> released");
          SimpleBot._ragHero = null;
          return;
        }

        RagHeroApproach();
        return;
      }

      if (_ragAttempts >= 3)
      {
        RagnarosEnd("3 attempts used");
        return;
      }

      // 0.35.2: any bot whose best hero has REALLY reached level 12 goes (no level raising any more, no trigger CP)
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var o = SimpleBot.Bots[b];
        if (!SimpleBot.HasPerks(o))
        {
          continue;
        }

        var hero = SimpleBot.BestHero(o);
        if (hero == null || GetHeroLevel(hero) < AiConfig.RagnarosHeroLevel)
        {
          continue;
        }

        SimpleBot._ragHero = hero;
        _ragBot = o;
        _ragSince = SimpleBot._clock;
        _ragAttempts++;
        SimpleBot.Log("Ragnaros mission: " + SimpleBot.FacName(o) + " sends " + GetUnitName(hero) + " (lvl " + I2S(GetHeroLevel(hero)) + ")");
        RagHeroApproach();
        return;
      }
    }

    private static bool RagHeroValid()
    {
      return SimpleBot._ragHero != null && GetUnitTypeId(SimpleBot._ragHero) != 0 && GetUnitState(SimpleBot._ragHero, UNIT_STATE_LIFE) > 0.405f
             && GetOwningPlayer(SimpleBot._ragHero) == _ragBot;
    }

    private static bool RagHeroNear(float x, float y)
    {
      var dx = GetUnitX(SimpleBot._ragHero) - x;
      var dy = GetUnitY(SimpleBot._ragHero) - y;
      return dx * dx + dy * dy < 1500f * 1500f;
    }

    private static bool RagHeroReady()
    {
      return RagHeroValid() && RagHeroNear(AiConfig.RagnarosX, AiConfig.RagnarosY)
             && GetHeroLevel(SimpleBot._ragHero) >= AiConfig.RagnarosHeroLevel;
    }

    // walk to the pedestal; raise to level 12 once within 1500 (BEFORE entering WL's rect)
    private static void RagHeroApproach()
    {
      if (!RagHeroValid())
      {
        return;
      }

      IssuePointOrder(SimpleBot._ragHero, "move", AiConfig.RagnarosX, AiConfig.RagnarosY);
    }

    // phase 3: find the summoned boss (hostile hero at WL's spawn point) and kill him with the standing-by hero
    private static void RagnarosKill()
    {
      unit rag = null;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, RagSummonX, RagSummonY, 1000f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (rag == null && IsUnitType(u, UNIT_TYPE_HERO) && GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_AGGRESSIVE)
            && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          rag = u;
        }
      }

      DestroyGroup(g);
      if (rag == null)
      {
        if (SimpleBot._clock - _ragSummonAt > 45f)
        {
          RagnarosEnd("Ragnaros not found after the summon (already dead?)");
        }

        return;
      }

      if (!RagHeroValid())
      {
        SimpleBot._ragHero = SimpleBot.BestHero(_ragBot);
      }

      if (!RagHeroValid())
      {
        return;
      }

      if (!RagHeroNear(RagSummonX, RagSummonY) || GetHeroLevel(SimpleBot._ragHero) < AiConfig.RagnarosHeroLevel)
      {
        RagHeroApproach(); // the level-12 hero must be present for the free kill
        return;
      }

      UnitDamageTarget(SimpleBot._ragHero, rag, 1000000f, true, false, ATTACK_TYPE_CHAOS, DAMAGE_TYPE_UNIVERSAL,
        WEAPON_TYPE_WHOKNOWS);
      if (GetUnitState(rag, UNIT_STATE_LIFE) <= 0.405f)
      {
        RagnarosEnd(SimpleBot.FacName(_ragBot) + "'s " + GetUnitName(SimpleBot._ragHero) + " slew Ragnaros");
      }
    }

    private static void RagnarosEnd(string why)
    {
      _ragDone = true;
      SimpleBot._ragHero = null;
      SimpleBot.Log("Ragnaros mission: " + why);
    }

    // ---- v32 Legion hand-overs to the Scourge --------------------------------------------------------------
    // Before the betrayal (Legion still allied to the Scourge): Capital Palace / Lordaeron City taken by a bot
    // Legion go to the Scourge -- the Scourge, not the demons, sacks Lordaeron.
    internal static void LegionPalaceHandover()
    {
      var legion = SimpleBot.PlayerOfFaction("Legion");
      var scourge = SimpleBot.PlayerOfFaction("Scourge");
      if (legion == null || scourge == null || !SimpleBot.HasPerks(legion) || !IsPlayerAlly(legion, scourge))
      {
        return;
      }

      var cp = SimpleBot.SafeCp(AiConfig.LordaeronCityCpId);
      if (cp != null && cp.Unit != null && GetOwningPlayer(cp.Unit) == legion)
      {
        SimpleBot.GiveUnit(cp.Unit, scourge);
        SimpleBot.Log("Legion took Lordaeron City before the betrayal -> handed to the Scourge");
      }

      var caps = CapitalManager.GetAll();
      for (var i = 0; i < caps.Count; i++)
      {
        var cu = caps[i].Unit;
        if (caps[i].UnitType == AiConfig.CapitalPalaceId && cu != null && GetOwningPlayer(cu) == legion
            && GetUnitState(cu, UNIT_STATE_LIFE) > 0.405f)
        {
          SimpleBot.GiveUnit(cu, scourge);
          SimpleBot.Log("Legion took Capital Palace before the betrayal -> handed to the Scourge");
        }
      }
    }

    // ---- v34.1 Kul Tiras repair -------------------------------------------------------------------------------
    // WL's Admiralty reward crashes on a '%' in its power text before it rescues the Kul Tiras base (seen as
    // "Kul'tiras failed to execute OnQuestProgressChanged: invalid use of '%' in replacement string"). Once the
    // quest is Complete, every Neutral-Passive unit still inside WL's Kul Tiras rescue area is rescued for Kul
    // Tiras with WL's own Rescue -- the step the crash skipped. Runs once; finds nothing if WL's reward worked.
    // (The City of Admirals power itself is still lost; that needs the text fix in WL's Loc.cs.)
    private static bool _ktRepairDone;

    internal static void KulTirasRepair()
    {
      if (_ktRepairDone)
      {
        return;
      }

      var kt = SimpleBot.PlayerOfFaction("Kul'tiras");
      var fac = kt != null ? kt.GetPlayerData().Faction : null;
      if (fac == null)
      {
        return;
      }

      QuestData adm = null;
      foreach (var q in fac.GetAllQuests())
      {
        if (q.Title == "The Admiralty of Kul Tiras")
        {
          adm = q;
        }
      }

      if (adm == null || adm.Progress == QuestProgress.Failed)
      {
        _ktRepairDone = true;
        return;
      }

      if (adm.Progress != QuestProgress.Complete)
      {
        return;
      }

      _ktRepairDone = true;
      var found = new List<unit>();
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Player(PLAYER_NEUTRAL_PASSIVE), null); // includes hidden (prepared) units
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.WaveOps++;
        var x = GetUnitX(u);
        var y = GetUnitY(u);
        if (x >= AiWorld.KulTirasRescueMinX && x <= AiWorld.KulTirasRescueMaxX
            && y >= AiWorld.KulTirasRescueMinY && y <= AiWorld.KulTirasRescueMaxY)
        {
          found.Add(u);
        }
      }

      DestroyGroup(g);
      for (var i = 0; i < found.Count; i++)
      {
        found[i].Rescue(kt);
      }

      SimpleBot.LogPin("Kul Tiras repair: " + I2S(found.Count) + " units/buildings rescued (Admiralty reward)");
    }

    // ---- v31 CP grants ----------------------------------------------------------------------------------------
    private static readonly HashSet<int> _grantDone = new();

    internal static void RunCpGrants()
    {
      var list = AiConfig.CpGrants;
      for (var i = 0; i < list.Count; i++)
      {
        if (_grantDone.Contains(i) || SimpleBot._clock < list[i].AtSec)
        {
          continue;
        }

        _grantDone.Add(i);
        var gr = list[i];
        var p = SimpleBot.PlayerOfFaction(gr.Faction);
        if (p == null || !SimpleBot.HasPerks(p))
        {
          continue;
        }

        for (var c = 0; c < gr.CpIds.Count; c++)
        {
          var cp = SimpleBot.SafeCp(gr.CpIds[c]);
          if (cp == null || cp.Unit == null || GetOwningPlayer(cp.Unit) != Player(PLAYER_NEUTRAL_AGGRESSIVE))
          {
            continue; // only free (creep-held) CPs; never take from a player or a quest lock
          }

          ClearCreeps(GetUnitX(cp.Unit), GetUnitY(cp.Unit), 1100f);
          SimpleBot.GiveUnit(cp.Unit, p);
          SimpleBot.Log(gr.Faction + ": granted " + GetUnitName(cp.Unit));
        }
      }
    }

    private static void ClearCreeps(float x, float y, float r)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, r, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.WaveOps++;
        if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_AGGRESSIVE) && !IsUnitType(u, UNIT_TYPE_STRUCTURE)
            && !ControlPointManager.Instance.UnitIsControlPoint(u))
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
    }

    // v32: a perma-dead legend that WL brought back as a BANNED new form (AiConfig.BannedNewForms, e.g. the
    // Cenarius ghost) is removed again when a bot owns it. Also scanned on the 15 s wave tick, because WL's
    // handler may run after ours.
    internal static void RemoveBannedForm(LegendaryHero lh)
    {
      var lu = lh.Unit;
      if (lu == null || GetUnitTypeId(lu) == 0 || !AiConfig.BannedNewForms.Contains(GetUnitTypeId(lu)))
      {
        return;
      }

      var owner = GetOwningPlayer(lu);
      if (!SimpleBot.IsBot(owner))
      {
        return; // humans keep WL's mechanic
      }

      SimpleBot.PermaDead.Add(GetUnitTypeId(lu));
      SimpleBot.Log("P" + I2S(GetPlayerId(owner)) + " " + SimpleBot.FacName(owner) + ": " + lh.Name
          + " returned as a banned new form -> removed (stays dead for bots)");
      RemoveUnit(lu);
    }

    internal static void ScanBannedForms()
    {
      for (var i = 0; i < SimpleBot._hookedLegends.Count; i++)
      {
        RemoveBannedForm(SimpleBot._hookedLegends[i]);
      }
    }

    // ---- v31 kill targets ------------------------------------------------------------------------------------
    // AiConfig.KillTargets: a faction's bot hunts one specific unit (found by TYPE id near X/Y) from AtSec until
    // it is dead. Assigned on the 15 s wave tick (one range scan, once); Process just reads AiState.KillTargetUnit.
    private const float KillGiveUpSec = 360f; // army can't finish it (too strong / unreachable) -> give up

    internal static void AssignKillTargets()
    {
      AssignClearAreas(); // v34.2
      var list = AiConfig.KillTargets;
      for (var i = 0; i < list.Count; i++)
      {
        if (AiState.KillDone.Contains(i) || SimpleBot._clock < list[i].AtSec)
        {
          continue;
        }

        var kt = list[i];
        var bot = SimpleBot.PlayerOfFaction(kt.Faction);
        if (bot == null || !SimpleBot.IsBot(bot))
        {
          AiState.KillDone.Add(i);
          continue;
        }

        AiState.KillDone.Add(i);
        var g = CreateGroup();
        GroupEnumUnitsInRange(g, kt.X, kt.Y, kt.Radius, null);
        unit found = null;
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (found == null && GetUnitTypeId(u) == kt.UnitType && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
          {
            found = u;
          }
        }

        DestroyGroup(g);
        if (found == null)
        {
          SimpleBot.Log("kill target " + kt.Label + ": not found near " + I2S(R2I(kt.X)) + "," + I2S(R2I(kt.Y)) + " -> skipped");
          continue;
        }

        AiState.KillTargetUnit[GetPlayerId(bot)] = found;
        AiState.KillSince[GetPlayerId(bot)] = SimpleBot._clock;
        SimpleBot.Log(kt.Faction + ": KILL TARGET " + kt.Label + " assigned");
      }
    }

    // ---- v34.2 area clears (AiConfig.ClearAreas) ----

    private static void AssignClearAreas()
    {
      var list = AiConfig.ClearAreas;
      for (var i = 0; i < list.Count; i++)
      {
        if (AiState.ClearDone.Contains(i))
        {
          continue;
        }

        var cp = SimpleBot.SafeCp(list[i].TriggerCpId);
        if (cp == null || cp.Unit == null)
        {
          continue;
        }

        var o = GetOwningPlayer(cp.Unit);
        var pid = GetPlayerId(o);
        if (!SimpleBot.IsBot(o) || (AiState.ClearActive.ContainsKey(pid) && AiState.ClearActive[pid] >= 0)
            || (AiState.KillTargetUnit.ContainsKey(pid) && AiState.KillTargetUnit[pid] != null))
        {
          continue; // owner is no bot, or already busy with a kill target / another clear
        }

        AiState.ClearDone.Add(i);
        AiState.ClearActive[pid] = i;
        AiState.ClearSince[pid] = SimpleBot._clock;
        SimpleBot.LogPin(SimpleBot.FacName(o) + ": AREA CLEAR " + list[i].Label + " started (holds trigger CP)");
      }
    }

    // nearest living Neutral Hostile unit in the bot's active clear area, or null (area done / given up)
    private static unit ActiveClearTarget(player p, int pid)
    {
      if (!AiState.ClearActive.ContainsKey(pid) || AiState.ClearActive[pid] < 0)
      {
        return null;
      }

      var ca = AiConfig.ClearAreas[AiState.ClearActive[pid]];
      if (SimpleBot._clock - AiState.ClearSince[pid] > 480f)
      {
        AiState.ClearActive[pid] = -1;
        SimpleBot.LogPin(SimpleBot.FacName(p) + ": area clear " + ca.Label + " given up after 8 min");
        return null;
      }

      unit best = null;
      var bestD = 0f;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, ca.X, ca.Y, ca.Radius, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.OpUnits++;
        if (GetOwningPlayer(u) != Player(PLAYER_NEUTRAL_AGGRESSIVE) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f
            || BlzIsUnitInvulnerable(u) || ControlPointManager.Instance.UnitIsControlPoint(u))
        {
          continue;
        }

        var dx = GetUnitX(u) - ca.X;
        var dy = GetUnitY(u) - ca.Y;
        var dd = dx * dx + dy * dy;
        if (best == null || dd < bestD)
        {
          best = u;
          bestD = dd;
        }
      }

      DestroyGroup(g);
      if (best == null)
      {
        AiState.ClearActive[pid] = -1;
        SimpleBot.LogPin(SimpleBot.FacName(p) + ": area clear " + ca.Label + " DONE");
      }

      return best;
    }

    internal static unit ActiveKillTarget(player p, int pid)
    {
      if (!AiState.KillTargetUnit.ContainsKey(pid) || AiState.KillTargetUnit[pid] == null)
      {
        return ActiveClearTarget(p, pid); // v34.2: area clears run when no single kill target is active
      }

      var u = AiState.KillTargetUnit[pid];
      if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        AiState.KillTargetUnit[pid] = null;
        SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(p) + ": kill target dead -> back to expansion");
        return null;
      }

      if (AiState.KillSince.ContainsKey(pid) && SimpleBot._clock - AiState.KillSince[pid] > KillGiveUpSec)
      {
        AiState.KillTargetUnit[pid] = null;
        SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(p) + ": kill target " + GetUnitName(u) + " still alive after "
            + I2S(R2I(KillGiveUpSec)) + "s -> giving up");
        return null;
      }

      return u;
    }

    // One-shots: when a non-excluded bot owns the trigger CP (or any CP within radius), send a small force to
    // the point once, then mark it consumed. Evaluated on the 15s wave tick -> no scanning loop.
    internal static void RunOneShots()
    {
      for (var i = 0; i < AiGraph.OneShots.Count; i++)
      {
        if (_oneShotDone.Contains(i))
        {
          continue;
        }

        var os = AiGraph.OneShots[i];
        var claimant = OneShotClaimant(os);
        if (claimant == null)
        {
          continue;
        }

        _oneShotDone.Add(i);
        SendForceTo(claimant, os.X, os.Y, 6);
        PingMinimap(os.X, os.Y, 4f);
      }
    }

    // The first bot that owns the trigger CP (or a CP within radius) and isn't excluded. Null if none yet.
    private static player OneShotClaimant(AiGraph.OneShot os)
    {
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || os.Exclude.Contains(fac.Name))
        {
          continue;
        }

        if (os.NearCpId != 0)
        {
          var cp = SimpleBot.SafeCp(os.NearCpId);
          if (cp != null && GetOwningPlayer(cp.Unit) == p)
          {
            return p;
          }
        }
        else
        {
          var cps = ControlPointManager.Instance.GetAllControlPoints();
          for (var c = 0; c < cps.Count; c++)
          {
            var cu = cps[c].Unit;
            if (GetOwningPlayer(cu) != p)
            {
              continue;
            }

            var ex = GetUnitX(cu) - os.X;
            var ey = GetUnitY(cu) - os.Y;
            if (ex * ex + ey * ey <= os.NearRadius * os.NearRadius)
            {
              return p;
            }
          }
        }
      }

      return null;
    }

    // Order up to n of a bot's nearest army units to attack-move a point (clears whatever is there).
    private static void SendForceTo(player p, float x, float y, int n)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      var sent = 0;
      unit u;
      while ((u = FirstOfGroup(g)) != null && sent < n)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON)
            || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        IssuePointOrder(u, "attack", x, y);
        sent++;
      }

      // drain the rest without ordering
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
      }

      DestroyGroup(g);
    }
  }
}
