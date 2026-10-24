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
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- Legion summon and betrayal, Argus start base, node events (Sunwell), WL demon portal fix and all AI portals (ships + Argus hub).
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiLegion
  {
    private static readonly Dictionary<int, bool> FiredEvents = new();

    // At the summon/betrayal: every CP the bot Legion holds on the Combined graph (Lordaeron, Northrend, EK) goes
    // to the Scourge, so the Legion stops defending/reinforcing a continent it has left.
    private static void LegionNorthHandover(player legion)
    {
      var scourge = SimpleBot.PlayerOfFaction("Scourge");
      if (legion == null || scourge == null || !SimpleBot.HasPerks(legion))
      {
        return;
      }

      SimpleBot.EnsureNodeSets();
      var mine = legion.GetPlayerData().ControlPoints;
      var give = new List<unit>();
      for (var i = 0; i < mine.Count; i++)
      {
        var cu = mine[i].Unit;
        if (cu != null && SimpleBot._combinedNodes.Contains(GetUnitTypeId(cu)) && !AiGraph.ArgusNodes.Contains(GetUnitTypeId(cu)))
        {
          give.Add(cu); // collect first: transferring changes the list being read (v34: Argus stays Legion)
        }
      }

      for (var i = 0; i < give.Count; i++)
      {
        SimpleBot.GiveUnit(give[i], scourge);
      }

      SimpleBot.Log("Legion betrayal: " + I2S(give.Count) + " northern CPs handed to the Scourge");
    }

    // A minimal, functional Legion base beside the Crystalsong Forest CP: a town hall, an altar, a couple of
    // farms for food, and 5 producers -- all pulled from Legion's own faction categories. Bots only, so a
    // human Legion is never handed free structures. Ids come from the faction, so a WL rename can't wrong-spawn.
    internal static void SpawnLegionBase()
    {
      var legion = SimpleBot.PlayerOfFaction("Legion");
      if (legion == null || !SimpleBot.IsBot(legion))
      {
        return;
      }

      var fac = legion.GetPlayerData().Faction;
      if (fac == null)
      {
        return;
      }

      // remove the Legion Heralds (workers) at the Argus start -- they become the buildings below
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, AiWorld.LegionStartX, AiWorld.LegionStartY, 1500f, null);
      unit u;
      var removed = 0;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetOwningPlayer(u) == legion && GetUnitTypeId(u) == AiWorld.LegionHeraldId)
        {
          RemoveUnit(u);
          removed++;
        }
      }

      DestroyGroup(g);

      // 0.34.3: each building gets its own open spot (searched outward from the start location, footprint
      // checked, kept apart from the others) -- the old fixed ring of 8 found only 1 walkable spot in Argus.
      // 5 barracks, 1 altar, 2 farms.
      // 0.39: a mix of production buildings (AiConfig.LegionArgusProduction) -- 5 Nether Pits only trained Felguards
      var types = new List<int>();
      for (var i = 0; i < AiConfig.LegionArgusProduction.Count; i++)
      {
        types.Add(AiConfig.LegionArgusProduction[i]);
      }

      AddFirstOf(fac, UnitCategory.Altar, types);
      AddFirstOf(fac, UnitCategory.Farm, types);
      AddFirstOf(fac, UnitCategory.Farm, types);
      var placed = 0;
      var fixedUsed = 0;
      var usedX = new List<float>();
      var usedY = new List<float>();
      for (var i = 0; i < types.Count; i++)
      {
        // 0.36: fixed, offline-checked spots first (AiWorld.ArgusSpotX/Y); the ring search only as a fallback
        if (i < AiWorld.ArgusSpotX.Length && FootprintWalkable(AiWorld.ArgusSpotX[i], AiWorld.ArgusSpotY[i], 144f))
        {
          SimpleBot._spotX = AiWorld.ArgusSpotX[i];
          SimpleBot._spotY = AiWorld.ArgusSpotY[i];
          fixedUsed++;
        }
        else if (!FindBuildSpot(AiWorld.LegionStartX, AiWorld.LegionStartY, usedX, usedY))
        {
          continue;
        }

        CreateUnit(legion, types[i], SimpleBot._spotX, SimpleBot._spotY, 270f);
        usedX.Add(SimpleBot._spotX);
        usedY.Add(SimpleBot._spotY);
        placed++;
      }

      SimpleBot.LogPin("Legion: Argus start -- " + I2S(removed) + " workers replaced by " + I2S(placed) + " of "
                       + I2S(types.Count) + " buildings (" + I2S(fixedUsed) + " on fixed spots)");
    }

    // open ground for a ~512-wide building: rings 500..2000 out, 16 directions; centre and 4 corners walkable and
    // >= 600 from every spot already used. Result in _spotX/_spotY.
    internal static bool FindBuildSpot(float cx, float cy, List<float> usedX, List<float> usedY)
    {
      for (var r = 500f; r <= 2000f; r += 250f)
      {
        for (var i = 0; i < 16; i++)
        {
          var a = i * 0.392699f;
          var x = cx + r * Cos(a);
          var y = cy + r * Sin(a);
          if (!FootprintWalkable(x, y, 220f))
          {
            continue;
          }

          var free = true;
          for (var k = 0; k < usedX.Count && free; k++)
          {
            var dx = usedX[k] - x;
            var dy = usedY[k] - y;
            free = dx * dx + dy * dy >= 600f * 600f;
          }

          if (free)
          {
            SimpleBot._spotX = x;
            SimpleBot._spotY = y;
            return true;
          }
        }
      }

      return false;
    }

    // centre and 4 corners (+-h) walkable (IsTerrainPathable is TRUE when NOT walkable). The ring search uses 220
    // (a ~512-wide area); the fixed Argus spots were checked offline cell by cell over +-144 and use that.
    private static bool FootprintWalkable(float x, float y, float h)
    {
      return !IsTerrainPathable(x, y, PATHING_TYPE_WALKABILITY)
             && !IsTerrainPathable(x + h, y + h, PATHING_TYPE_WALKABILITY)
             && !IsTerrainPathable(x - h, y + h, PATHING_TYPE_WALKABILITY)
             && !IsTerrainPathable(x + h, y - h, PATHING_TYPE_WALKABILITY)
             && !IsTerrainPathable(x - h, y - h, PATHING_TYPE_WALKABILITY);
    }

    private static void AddFirstOf(Faction fac, UnitCategory cat, List<int> into)
    {
      var ids = new List<int>();
      SimpleBot.AddCategory(fac, cat, ids);
      if (ids.Count > 0)
      {
        into.Add(ids[0]);
      }
    }

    // v15: fire a node's event once, when a bot of its ByFaction owns that control point.
    private static unit CapitalUnitOfType(int typeId)
    {
      var caps = CapitalManager.GetAll();
      for (var i = 0; i < caps.Count; i++)
      {
        var cu = caps[i].Unit;
        if (cu != null && GetUnitTypeId(cu) == typeId && GetUnitState(cu, UNIT_STATE_LIFE) > 0.405f)
        {
          return cu;
        }
      }

      return null;
    }

    internal static void OnEventCheck()
    {
      if (SimpleBot.BotBreak)
      {
        return;
      }

      CheckBookSummon();
      AiMissions.RunTerrordale(); // 0.38.1: every 2 s, so the hand-over comes right after the plague

      var evs = AiGraph.Events;
      for (var i = 0; i < evs.Count; i++)
      {
        var ev = evs[i];
        if (ev.NodeId == 0 || FiredEvents.ContainsKey(ev.NodeId))
        {
          continue;
        }

        // v34: an event node may also be a CAPITAL (The Sunwell is not a control point -- that is why the
        // Scourge took it and nothing happened).
        var cp = SimpleBot.SafeCp(ev.NodeId);
        var nodeUnit = cp != null ? cp.Unit : CapitalUnitOfType(ev.NodeId);
        if (nodeUnit == null)
        {
          continue;
        }

        var owner = GetOwningPlayer(nodeUnit);
        if (GetPlayerId(owner) >= 24)
        {
          continue; // still neutral / unclaimed
        }

        var ownerFac = owner.GetPlayerData().Faction;
        if (ownerFac != null && ownerFac.Name == ev.ByFaction)
        {
          FiredEvents[ev.NodeId] = true;
          FireEvent(ev);
        }
      }
    }

    // 0.39: remove every preplaced unit of a type (e.g. the dead Emerald Portals) -- whole map, once
    private static void RemovePreplacedOfType(int typeId)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRect(g, GetWorldBounds(), null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == typeId)
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
    }

    // 0.39: the Scourge's plague lands and a bot Legion's army is stuck on Argus / in Northrend -> its whole army
    // (not heroes on a mission, not workers, not buildings) is moved next to Alterac Mountains to support the invasion.
    internal static void MoveLegionArmyToAlterac()
    {
      if (!LegionIsBot())
      {
        return;
      }

      var cp = SimpleBot.SafeCp(AiConfig.AlteracCpId);
      if (cp == null || cp.Unit == null)
      {
        return;
      }

      player legion = null;
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == "Legion")
        {
          legion = f.Player;
        }
      }

      if (legion == null)
      {
        return;
      }

      var x = GetUnitX(cp.Unit);
      var y = GetUnitY(cp.Unit) - 400f;
      var n = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, legion, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON) || AiState.MissionUnits.Contains(u)
            || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || GetUnitMoveSpeed(u) <= 1f)
        {
          continue;
        }

        SetUnitPosition(u, x + GetRandomReal(-500f, 500f), y + GetRandomReal(-500f, 500f));
        n++;
      }

      DestroyGroup(g);
      SimpleBot.LogPin("Legion: " + I2S(n) + " units moved to Alterac Mountains for the Scourge's invasion");
    }

    // 0.38: is the Legion a bot (with perks)? The AI only forces the summon for a bot Legion -- a human Legion summons
    // itself through WL's ritual.
    internal static bool LegionIsBot()
    {
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == "Legion" && f.Player != null)
        {
          return SimpleBot.HasPerks(f.Player);
        }
      }

      return false;
    }

    // 0.38: second summon trigger (lore: "Under the Burning Sky" -- Archimonde summoned with the Book of Medivh in
    // Dalaran). The Legion's own "Book of Medivh" quest completes when Dalaran's capital is dead and a Legion unit
    // reached the pedestal (AiConfig.HeroVisits sends one). For a bot Legion that quest done -> summon.
    private static QuestData _legionBookQuest;
    private static bool _bookChecked;

    private static void CheckBookSummon()
    {
      if (_legionSummonStarted || _bookChecked)
      {
        return;
      }

      if (_legionBookQuest == null)
      {
        foreach (var f in FactionManager.GetAllFactions())
        {
          if (f.Name == "Legion")
          {
            _legionBookQuest = QuestAutoComplete.FindQuest(f, AiConfig.BookQuestTitle);
          }
        }

        if (_legionBookQuest == null)
        {
          _bookChecked = true; // no Legion in this game / quest not found
          return;
        }
      }

      if (_legionBookQuest.Progress == QuestProgress.Complete && LegionIsBot())
      {
        _bookChecked = true;
        SimpleBot.LogPin("Legion holds the Book of Medivh -> summon");
        SummonLegion();
      }
    }

    private static void FireEvent(NodeEvent ev)
    {
      if (ev.Kind == "legion" && !LegionIsBot())
      {
        SimpleBot.LogPin("event: " + ev.ByFaction + " holds the Sunwell -- Legion is human, no forced summon");
        return; // 0.38: a human Legion summons through WL's own ritual
      }

      if (ev.Kind == "legion")
      {
        SimpleBot.Log("event: " + ev.ByFaction + " holds node " + I2S(ev.NodeId) + " -> Legion summon");
        SummonLegion();
      }
      else if (ev.Kind == "chat")
      {
        for (var i = 0; i < 24; i++)
        {
          DisplayTimedTextToPlayer(Player(i), 0, 0, 10, ev.Text);
        }
      }
    }

    public static void SummonLegion()
    {
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name != "Legion")
        {
          continue;
        }

        foreach (var q in f.GetAllQuests())
        {
          if (q is QuestSummonLegion)
          {
            if (_legionSummonStarted)
            {
              return; // v34: once only (Sunwell + corrupted Sunwell events, or a repeated -legion)
            }

            _legionSummonStarted = true;
            var t = CreateTimer();
            TimerStart(t, 1.0f, false, OnLegionSummoned); // let the quest's own CreatePortals run first
            try
            {
              q.Progress = QuestProgress.Complete;
            }
            catch (System.Exception e)
            {
              SimpleBot.LogPin("Legion summon quest: WL error during completion -- " + e.Message);
            }

            return;
          }
        }
      }
    }

    // v24: forget the quest's invisible exterior portal (it spawns at the caster's spot -> (0,0) for a bot, and
    // its model may not render). Instead spawn a clean, visible waygate pair, pull the Legion to the Kalimdor
    // exit so the Hyjal march begins at once, switch its node system to Kalimdor, and trigger the betrayal.
    // v34.1: the summon quest can complete by ANY path (bot ritual, WL, -legion, the Sunwell event). The wave
    // tick polls it, so the AI's summon handling (portals, merge, gold, betrayal) always runs -- exactly once.
    private static bool _legionHandled;
    private static QuestData _legionQuest;

    internal static void CheckLegionSummoned()
    {
      if (_legionHandled)
      {
        return;
      }

      if (_legionQuest == null)
      {
        var f = SimpleBot.PlayerOfFaction("Legion");
        var fac = f != null ? f.GetPlayerData().Faction : null;
        if (fac == null)
        {
          return;
        }

        foreach (var q in fac.GetAllQuests())
        {
          if (q is QuestSummonLegion)
          {
            _legionQuest = q;
          }
        }

        if (_legionQuest == null)
        {
          return;
        }
      }

      if (_legionQuest.Progress == QuestProgress.Complete)
      {
        SimpleBot.LogPin("Legion summon quest is complete -> running the summon handling");
        OnLegionSummoned();
      }
    }

    private static void OnLegionSummoned()
    {
      if (_legionHandled)
      {
        return;
      }

      _legionHandled = true;
      try
      {
        OnLegionSummonedSteps();
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("Legion summon handling ERROR: " + e.Message);
      }

      // the graph switches must happen even if a step above failed
      SimpleBot._factionEdgeSet["Legion"] = "Kalimdor";
      SimpleBot._gateOpen["legion"] = true;
      SimpleBot._gateOpen["prelegion"] = false;
      SimpleBot._graphMerged = true;
      SimpleBot.LogPin("LEGION SUMMONED: Legion home -> Kalimdor, Argus hub open, Northrend link closed, graphs MERGED");
    }

    private static void OnLegionSummonedSteps()
    {
      // 1) v31: WL's own portal pair. QuestSummonLegion creates the exterior Demon Portal (n037) at the casting
      //    hero's position -- (0,0) when the quest is completed without a caster (bot, -legion, Sunwell event) --
      //    and points the interior Antoran portal (n03C) at it. Recreate it at the real exit and re-point the
      //    interior portal. Only if WL's portal can't be found do we fall back to the old workaround pair.
      //    v34: that portal now goes beside Quel'Danas (Argus <-> Sunwell). The Argus <-> Northrend portal is
      //    removed and the Argus <-> Felwood portal opens (AiGraph.Portals, phase "legion").
      SimpleBot.Safe("summon: demon portal", () =>
      {
        if (!FixDemonPortal())
        {
          SimpleBot.LogPin("Legion: WL Demon Portal not found at (0,0) -- Argus <-> Sunwell link missing");
        }
      });
      SimpleBot.Safe("summon: portals", SwitchPortalsToLegionPhase);

      var legion = SimpleBot.PlayerOfFaction("Legion");

      // v31: one-time war chest for a bot Legion arriving on Kalimdor.
      if (legion != null && SimpleBot.HasPerks(legion) && !_legionGoldGiven)
      {
        _legionGoldGiven = true;
        SetPlayerState(legion, PLAYER_STATE_RESOURCE_GOLD,
          GetPlayerState(legion, PLAYER_STATE_RESOURCE_GOLD) + AiWorld.LegionSummonGold);
        SimpleBot.Log("Legion: +" + I2S(AiWorld.LegionSummonGold) + " gold (summon bonus)");
      }

      // v32: the north stays with the Scourge -- the Legion stops defending a continent it has left.
      SimpleBot.Safe("summon: north handover", () => LegionNorthHandover(legion));

      // 2) collect the Legion's host at the Kalimdor exit -- bots only, so a human Legion keeps control of theirs.
      if (legion != null && SimpleBot.HasPerks(legion))
      {
        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, legion, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
          {
            continue;
          }

          SetUnitX(u, AiWorld.LegionExitX + GetRandomReal(-384f, 384f));
          SetUnitY(u, AiWorld.LegionExitY + GetRandomReal(-384f, 384f));
        }

        DestroyGroup(g);
      }

      // 3) graph switches (Legion home -> Kalimdor, hub gates, MERGE) happen in OnLegionSummoned, after this.

      // 4) the betrayal: the Scourge turns on its demon masters. Scattered northern Legion units are left to die.
      var scourge = SimpleBot.PlayerOfFaction("Scourge");
      if (scourge != null && legion != null)
      {
        SetPlayerAlliance(scourge, legion, ALLIANCE_PASSIVE, false);
        SetPlayerAlliance(legion, scourge, ALLIANCE_PASSIVE, false);
      }

      SimpleBot.PingAt(AiWorld.LegionExitX, AiWorld.LegionExitY);
      SimpleBot.Announce("|cffff5555The Burning Legion pours into Kalimdor -- and the Scourge turns on its masters! (see the ping)|r");
    }

    private static bool _legionGoldGiven;
    private static bool _legionSummonStarted;

    // v31: find WL's exterior Demon Portal stranded near (0,0) and rebuild it at AiWorld.LegionExit, re-pointing
    // WL's interior Antoran portal at it. Returns true if a portal was (or already is) in place. Called after a
    // summon and from the 15 s wave tick (one small range scan at the map centre -- cheap, catches any summon path).
    internal static bool FixDemonPortal()
    {
      var portalType = FourCC("n037");
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, 0f, 0f, 900f, null);
      unit stranded = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.WaveOps++;
        if (stranded == null && GetUnitTypeId(u) == portalType)
        {
          stranded = u;
        }
      }

      DestroyGroup(g);
      if (stranded == null)
      {
        return _demonPortalFixed;
      }

      var owner = GetOwningPlayer(stranded);
      var x = AiWorld.LegionSunwellPortalX; // v34: beside Quel'Danas (was: beside Felwood)
      var y = AiWorld.LegionSunwellPortalY;
      var fresh = CreateUnit(owner, portalType, x, y, 0f);
      BlzSetUnitName(fresh, "Demon Portal to Argus");

      // interior portal: preplaced n03C in Antoran (WL points it at the exterior portal's old position)
      var interior = SimpleBot.FindUnitOfType(FourCC("n03C"), AiWorld.LegionInteriorX, AiWorld.LegionInteriorY, 1500f);
      if (interior != null)
      {
        LandBeside(GetUnitX(interior), GetUnitY(interior));
        WaygateSetDestination(fresh, SimpleBot._spotX, SimpleBot._spotY);
        LandBeside(x, y);
        WaygateSetDestination(interior, SimpleBot._spotX, SimpleBot._spotY);
        WaygateActivate(interior, true);
        BlzSetUnitName(interior, "Demon Portal to Quel'Danas");
      }
      else
      {
        WaygateSetDestination(fresh, WaygateGetDestinationX(stranded), WaygateGetDestinationY(stranded));
      }

      WaygateActivate(fresh, true);
      RemoveUnit(stranded);
      DestroyEffect(AddSpecialEffect(AiWorld.TeleportFx, x, y));
      SimpleBot.FloatLabel("Legion Portal", x, y);
      SimpleBot.PingAt(x, y);
      _demonPortalFixed = true;
      SimpleBot.LogPin("Legion: Demon Portal moved (0,0) -> " + I2S(R2I(x)) + "," + I2S(R2I(y))
          + (interior != null ? ", Antoran portal re-pointed" : ", Antoran portal NOT found"));
      return true;
    }

    private static bool _demonPortalFixed;

    // ---- v34 physical portals (AiGraph.Portals) ------------------------------------------------------------
    // Real waygates -- usable by every player. "always" + "prelegion" are created 2 s into the game, the
    // "prelegion" ones removed and the "legion" ones created at the summon. Travellers land on open ground
    // BESIDE the other end (landing exactly on a waygate can bounce them straight back).
    // v34.1: owned by Neutral EXTRA (player 26), not Neutral Passive -- WL removes Neutral-Passive units from
    // whole areas at runtime (CleanupNeutralPassiveUnits), which is the likely reason only one ship pair
    // survived. A watchdog on the wave tick rebuilds any pair whose unit disappeared, and every pair is created
    // inside try/catch so one failure can't stop the others.

    public static int PortalPairCount => AiState.PairLink.Count;
    public static unit PortalA(int i) => AiState.PairA[i];
    public static unit PortalB(int i) => AiState.PairB[i];

    internal static void SetupPortals()
    {
      SimpleBot._gateOpen["prelegion"] = true;
      CreatePortals("always");
      CreatePortals("prelegion");
    }

    private static void SwitchPortalsToLegionPhase()
    {
      for (var i = AiState.PairLink.Count - 1; i >= 0; i--)
      {
        if (AiGraph.Portals[AiState.PairLink[i]].Phase == "prelegion")
        {
          RemoveUnit(AiState.PairA[i]);
          RemoveUnit(AiState.PairB[i]);
          AiState.PairLink.RemoveAt(i);
          AiState.PairA.RemoveAt(i);
          AiState.PairB.RemoveAt(i);
        }
      }

      CreatePortals("legion");
    }

    // 0.37: create a later portal phase (e.g. "oldgods") -- AiGates.OpenOldGodsRift
    internal static void OpenPortalPhase(string phase)
    {
      CreatePortals(phase);
    }

    private static void CreatePortals(string phase)
    {

      var list = AiGraph.Portals;
      var n = 0;
      for (var i = 0; i < list.Count; i++)
      {
        if (list[i].Phase == phase && CreatePair(i, -1))
        {
          n++;
        }
      }

      SimpleBot.LogPin("portals: " + I2S(n) + " pair(s) created (" + phase + ")");
    }

    // build link i; slot >= 0 replaces an existing pair (watchdog), -1 appends
    private static bool CreatePair(int link, int slot)
    {
      var pl = AiGraph.Portals[link];
      try
      {
        var a = CreatePortalEnd(pl.AX, pl.AY, pl.Skin, pl.NameA);
        var b = CreatePortalEnd(pl.BX, pl.BY, pl.Skin, pl.NameB);
        LandBeside(pl.BX, pl.BY);
        WaygateSetDestination(a, SimpleBot._spotX, SimpleBot._spotY);
        LandBeside(pl.AX, pl.AY);
        WaygateSetDestination(b, SimpleBot._spotX, SimpleBot._spotY);
        WaygateActivate(a, true);
        WaygateActivate(b, true);
        if (slot < 0)
        {
          AiState.PairLink.Add(link);
          AiState.PairA.Add(a);
          AiState.PairB.Add(b);
        }
        else
        {
          AiState.PairA[slot] = a;
          AiState.PairB[slot] = b;
        }

        return true;
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("portal '" + pl.NameA + "' FAILED: " + e.Message);
        return false;
      }
    }

    // wave tick: rebuild any pair whose unit was removed or killed
    internal static void PortalWatchdog()
    {
      for (var i = 0; i < AiState.PairLink.Count; i++)
      {
        var aOk = GetUnitTypeId(AiState.PairA[i]) != 0 && GetUnitState(AiState.PairA[i], UNIT_STATE_LIFE) > 0.405f;
        var bOk = GetUnitTypeId(AiState.PairB[i]) != 0 && GetUnitState(AiState.PairB[i], UNIT_STATE_LIFE) > 0.405f;
        if (aOk && bOk)
        {
          continue;
        }

        if (aOk)
        {
          RemoveUnit(AiState.PairA[i]);
        }

        if (bOk)
        {
          RemoveUnit(AiState.PairB[i]);
        }

        if (CreatePair(AiState.PairLink[i], i))
        {
          SimpleBot.LogPin("portal '" + AiGraph.Portals[AiState.PairLink[i]].NameA + "' was gone -> rebuilt");
        }
      }
    }

    private static unit CreatePortalEnd(float x, float y, string skin, string name)
    {
      var u = CreateUnit(Player(26), AiWorld.PortalUnitId, x, y, 0f); // 26 = Neutral Extra (see above)
      if (skin != "")
      {
        BlzSetUnitSkin(u, FourCC(skin));
      }

      BlzSetUnitName(u, name);
      SetUnitInvulnerable(u, true);
      ShrinkBurn(u, AiWorld.PortalBurnUnitsAbility);      // 0.36: n036's 1000-range immolation -> 250
      ShrinkBurn(u, AiWorld.PortalBurnStructuresAbility);

      // 0.38: a small minimap icon so humans see the AI's portals (ships blue, Legion/rift portals purple). It is
      // destroyed together with the portal (orphan destroy) -- rebuilt portals get a new one.
      var ship = skin == "hbot";
      var icon = CreateMinimapIconOnUnit(u, ship ? 60 : 170, ship ? 140 : 0, 255, AiWorld.PortalIconPath,
        FOG_OF_WAR_MASKED);
      SetMinimapIconOrphanDestroy(icon, true);
      return u;
    }

    // 0.36: per-unit area of the portal's "Anti Portal Blocking" immolation. The disable/enable pair makes WC3
    // re-apply the passive with the new area (an aura keeps its old area until it is refreshed).
    private static void ShrinkBurn(unit u, int abilityId)
    {
      var ab = BlzGetUnitAbility(u, abilityId);
      if (ab == null)
      {
        return;
      }

      BlzSetAbilityRealLevelField(ab, ABILITY_RLF_AREA_OF_EFFECT, 0, AiWorld.PortalBurnRadius);
      BlzUnitDisableAbility(u, abilityId, true, false);
      BlzUnitDisableAbility(u, abilityId, false, false);
    }

    // open, walkable point ~300 from a portal (falls back to the portal itself) -> _spotX/_spotY
    // 0.35.3: also keeps 250 away from buildings (a landing spot inside a building's footprint made ground units
    // unable to use the Felwood portal) and tries a second ring at 450.
    private static void LandBeside(float x, float y)
    {
      for (var r = 300f; r <= 450f; r += 150f)
      {
        for (var i = 0; i < 8; i++)
        {
          var a = i * 0.785398f;
          var lx = x + r * Cos(a);
          var ly = y + r * Sin(a);
          if (!IsTerrainPathable(lx, ly, PATHING_TYPE_WALKABILITY) && !StructureNear(lx, ly, 250f))
          {
            SimpleBot._spotX = lx;
            SimpleBot._spotY = ly;
            return;
          }
        }
      }

      SimpleBot._spotX = x + 300f;
      SimpleBot._spotY = y;
    }

    private static bool StructureNear(float x, float y, float r)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, r, null);
      var found = false;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) && GetUnitAbilityLevel(u, FourCC("Awrp")) == 0)
        {
          found = true;
        }
      }

      DestroyGroup(g);
      return found;
    }
  }
}
