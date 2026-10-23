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
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- gate removal (gate units + gate destructables), Tempest Reach edge, Thandol bridge.
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiGates
  {
    public static int LastSweepOps; // named graph gates (e.g. "greymane")
    private static bool _thandolOpened;      // v25: force-open Thandol Span once at 20 min
    private static bool _tempestWired;

    private static float _lastGateSweep = -1f;
    private static HashSet<int> _gateSet;
    private static bool _destGatesSwept;
    private static HashSet<int> _destGateSet;

    // One-time: kill every destructible GATE doodad so no chokepoint gate blocks anyone. Gates are preplaced,
    // so the single whole-map destructible enum runs once. Only AiGraph.GateDestructibles types (gates) are
    // touched -- bridges and tree/stone walls are excluded, so this cannot delete a bridge deck.
    internal static void SweepDestructibleGates()
    {
      if (_destGatesSwept)
      {
        return;
      }

      _destGatesSwept = true;
      if (_destGateSet == null)
      {
        _destGateSet = new HashSet<int>();
        for (var i = 0; i < AiGraph.GateDestructibles.Count; i++)
        {
          _destGateSet.Add(AiGraph.GateDestructibles[i]);
        }
      }

      EnumDestructablesInRect(GetWorldBounds(), null, KillGateDestructable);
    }

    private static void KillGateDestructable()
    {
      var d = GetEnumDestructable();
      if (_destGateSet.Contains(GetDestructableTypeId(d)))
      {
        SetDestructableInvulnerable(d, false);
        KillDestructable(d);
      }
    }

    // Remove every bot-owned wall-gate so a bot can never wall its own army in. Runs once immediately, then
    // ~every 5 min (gates can be built later). Strict type match against AiGraph.GateTypes -> only real gates.
    // 0.38: gates that appear DURING the game (quest rewards, rescues that create units, upgrades) are removed the
    // moment they enter the map, not up to 5 min later. Units that were already on the map (hidden rescue units
    // that are only shown or handed over) do not "enter" -- the 5-min whole-map sweep below stays for those.
    // Cost: one HashSet lookup per created unit.
    private static trigger _gateWatch;

    private static void WatchNewGates()
    {
      if (_gateWatch != null)
      {
        return;
      }

      var world = CreateRegion();
      RegionAddRect(world, GetWorldBounds());
      _gateWatch = CreateTrigger();
      TriggerRegisterEnterRegion(_gateWatch, world, null);
      TriggerAddAction(_gateWatch, OnUnitEntersMap);
    }

    private static void OnUnitEntersMap()
    {
      var u = GetTriggerUnit();
      if (_gateSet != null && _gateSet.Contains(GetUnitTypeId(u)))
      {
        SimpleBot.Log("gate watch: new gate " + GetUnitName(u) + " removed at " + I2S(R2I(GetUnitX(u))) + ","
                      + I2S(R2I(GetUnitY(u))));
        RemoveUnit(u);
      }
    }

    internal static void SweepBotGates()
    {
      if (_lastGateSweep >= 0f && SimpleBot._clock - _lastGateSweep < 300f)
      {
        return;
      }

      _lastGateSweep = SimpleBot._clock;
      if (_gateSet == null)
      {
        _gateSet = new HashSet<int>();
        for (var i = 0; i < AiGraph.GateTypes.Count; i++)
        {
          _gateSet.Add(AiGraph.GateTypes[i]);
        }

        WatchNewGates();
      }

      // v27.2: whole-map unit enum (not per-player) -- catches gate units no matter who owns them or how they
      // were placed. Strict gate-type match -> only real gates are removed. Use -id on a surviving gate to read
      // its true type id; if it isn't one of these, add it to AiGraph.GateTypes.
      var g = CreateGroup();
      GroupEnumUnitsInRect(g, GetWorldBounds(), null);
      unit u;
      var touched = 0;
      var removed = 0;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        touched++;
        if (_gateSet.Contains(GetUnitTypeId(u)))
        {
          RemoveUnit(u);
          removed++;
        }
      }

      DestroyGroup(g);
      SimpleBot.WaveOps += touched;
      LastSweepOps = touched;
      if (removed > 0)
      {
        SimpleBot.Log("gate sweep: removed " + I2S(removed) + " gate units (" + I2S(touched) + " units scanned)");
      }
    }

    // Tempest Reach has no named CP constant in WL source (CPs are auto-detected by buff), so we resolve it at
    // runtime from its coord (8960,-1152): find the nearest CP and add the Blackwald edge. Niche Gilneas spur --
    // if nothing sits there, it just stays a leaf.
    internal static void WireTempestReach()
    {
      if (_tempestWired)
      {
        return;
      }

      _tempestWired = true;
      const int blackwald = 1848653398;
      var bestType = 0;
      var bestD = 1500f * 1500f;
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      for (var i = 0; i < cps.Count; i++)
      {
        var ex = GetUnitX(cps[i].Unit) - 8960f;
        var ey = GetUnitY(cps[i].Unit) - (-1152f);
        var d = ex * ex + ey * ey;
        if (d < bestD)
        {
          bestD = d;
          bestType = cps[i].UnitType;
        }
      }

      if (bestType != 0 && bestType != blackwald)
      {
        AiGraph.Combined.Add(new Edge(blackwald, bestType));
      }
    }

    // 0.36: named graph gates (Edge.Gate). "legion"/"prelegion" are set by AiLegion; these two are opened here.
    //  * "greymane" (Silverpine <-> Gilneas City): was NEVER opened since v25, so no bot could attack Gilneas City
    //    from Silverpine. The gate unit itself is removed by the sweep and a quest-locked (Neutral-Passive) Gilneas
    //    City is skipped by IsUncapturable anyway -> open the edge once a real player owns Gilneas City.
    //  * "tomb" (Val'sharah <-> Tomb of Sargeras): WL keeps everyone out of the Tomb until its shared quest
    //    "Tomb of Sargeras" (turn 15 + a level-10 hero at the entrance; AiConfig.HeroVisits sends one) is complete.
    private static bool _greymaneOpen;
    private static QuestData _tombQuest;

    // 0.39.3: without the Legion summon the two graphs (Eastern Kingdoms / Kalimdor) never merged -- in a 520-min test
    // Illidan and the Fel Horde won the Eastern Kingdoms but never saw Kalimdor as a target. Merge when the rift
    // opens, and at the latest at 60:00. Home first still applies (own continent before the other one).
    private const float MergeAt = 3600f;

    internal static void MergeGraphs(string why)
    {
      if (SimpleBot._graphMerged)
      {
        return;
      }

      SimpleBot._graphMerged = true;
      SimpleBot.LogPin("graphs MERGED (" + why + ") -- every faction may target the whole world, home first");
    }

    internal static void OpenNamedGates()
    {
      if (SimpleBot._clock >= MergeAt)
      {
        MergeGraphs("60:00 without a Legion summon");
      }

      if (!_greymaneOpen)
      {
        var cp = SimpleBot.SafeCp(AiConfig.GilneasCityCpId);
        if (cp != null && cp.Unit != null && GetPlayerId(GetOwningPlayer(cp.Unit)) < 24)
        {
          _greymaneOpen = true;
          SimpleBot._gateOpen["greymane"] = true;
          SimpleBot.LogPin("gate 'greymane' open: Gilneas City held by " + SimpleBot.FacName(GetOwningPlayer(cp.Unit)));
        }
      }

      OpenThandolIfClear();
      if (SimpleBot.GateOpen("tomb"))
      {
        return;
      }

      if (_tombQuest == null)
      {
        var all = FactionManager.GetAllFactions();
        for (var i = 0; i < all.Count && _tombQuest == null; i++)
        {
          _tombQuest = QuestAutoComplete.FindQuest(all[i], AiConfig.TombQuestTitle); // shared: one instance
        }

        if (_tombQuest == null)
        {
          return;
        }
      }

      if (_tombQuest.Progress == QuestProgress.Complete)
      {
        SimpleBot._gateOpen["tomb"] = true;
        SimpleBot.LogPin("gate 'tomb' open: the Tomb of Sargeras was opened");
      }
    }

    // 0.37.2: gate "thandol" (Arathi Highlands <-> Dun Modr) follows the real bridge: WL blocks Thandol Span with
    // invulnerable rocks (LTrc, "Bridge Rocks" in Regions.BridgeAmbient) and removes them at turn 30. The old fixed
    // 20:00 opening sent dwarves to the rocks for ten minutes. Checked every wave tick until the rocks are gone.
    private static rect _bridgeRect;
    private static int _bridgeRocks;

    private static void OpenThandolIfClear()
    {
      if (SimpleBot.GateOpen("thandol"))
      {
        return;
      }

      if (_bridgeRect == null)
      {
        _bridgeRect = Rect(15232f, -512f, 17856f, 896f); // Regions.BridgeAmbient
      }

      _bridgeRocks = 0;
      EnumDestructablesInRect(_bridgeRect, null, CountBridgeRock);
      if (_bridgeRocks == 0)
      {
        SimpleBot._gateOpen["thandol"] = true;
        SimpleBot.LogPin("gate 'thandol' open: the Thandol Span rocks are gone");
      }
    }

    private static void CountBridgeRock()
    {
      var d = GetEnumDestructable();
      if (GetDestructableTypeId(d) == RockChunkId && GetDestructableLife(d) > 0f)
      {
        _bridgeRocks++;
      }
    }

    private const int RockChunkId = 1280602723; // 'LTrc'

    // 0.37: the OLD GODS RIFT (replaces the Old Gods wave). When an Old Gods faction (AiConfig.OldGodsFactions) takes
    // Nordrassil, a real waygate pair opens between the Maw of Gor'ma and the Twilight Highlands (portal phase and
    // graph gate "oldgods"), usable by every player. The unused preplaced portal n07E in the Twilight Highlands is
    // replaced by the real end. Announced to everyone with both ends pinged. The second trigger (non-Old-Gods take
    // the Maw + the Old Gods' main base falls) waits for a decision (TODO.md).
    private static bool _riftOpen;
    private static bool _riftForced;

    internal static void ForceOldGodsRift() // -ogrift
    {
      _riftForced = true;
      OpenOldGodsRift();
    }

    internal static void OpenOldGodsRift()
    {
      if (_riftOpen)
      {
        return;
      }

      var who = "-ogrift";
      var byVictors = false;
      if (!_riftForced)
      {
        var oldGods = HolderIsOldGods(AiConfig.NordrassilCpId);
        if (oldGods != "")
        {
          who = oldGods; // trigger 1: an Old Gods faction took Nordrassil
        }
        else
        {
          // 0.38.1 trigger 2: the Maw of Gor'ma held by a faction outside the Old Gods AND N'Zoth (Black Empire's
          // immobile Old God) dead -> the rift opens for the victors
          var victor = HolderOutsideOldGods(AiConfig.MawCpId);
          if (victor == "" || OldGodAlive(AiConfig.NZothType))
          {
            return;
          }

          who = victor;
          byVictors = true;
        }
      }

      _riftOpen = true;
      MergeGraphs("Old Gods rift open"); // 0.39.3: every faction may now target the other continent
      RemovePreplacedRiftPortal();
      AiLegion.OpenPortalPhase("oldgods");
      SimpleBot._gateOpen["oldgods"] = true;
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 25, byVictors
          ? "|cffaa00ffN'Zoth is slain and the Maw of Gor'ma is in the hands of his foes. The rift the Old God tore "
            + "through the world now carries them to the Twilight Highlands.|r"
          : "|cffaa00ffNordrassil has fallen to the servants of the Old Gods.|r "
            + "|cffaa00ffIn the Maw of Gor'ma, N'Zoth's whispers tear the veil of the world -- a rift now opens onto the "
            + "Twilight Highlands.|r");
      }

      PingMinimapEx(AiConfig.RiftMawX, AiConfig.RiftMawY, 6f, 170, 0, 255, true);
      PingMinimapEx(AiConfig.RiftHighlandsX, AiConfig.RiftHighlandsY, 6f, 170, 0, 255, true);
      SimpleBot.LogPin("OLD GODS RIFT open: " + (byVictors ? "Maw of Gor'ma taken by " : "Nordrassil taken by ") + who
                       + " -- Maw of Gor'ma <-> Twilight Highlands");
    }

    // faction name of the CP's holder if it is an Old Gods faction, else ""
    private static string HolderIsOldGods(int cpId)
    {
      var f = HolderFaction(cpId);
      return f != null && AiConfig.OldGodsFactions.Contains(f.Name) ? f.Name : "";
    }

    // faction name of the CP's holder if it is a real player outside the Old Gods, else ""
    private static string HolderOutsideOldGods(int cpId)
    {
      var f = HolderFaction(cpId);
      return f != null && !AiConfig.OldGodsFactions.Contains(f.Name) ? f.Name : "";
    }

    private static Faction HolderFaction(int cpId)
    {
      var cp = SimpleBot.SafeCp(cpId);
      if (cp == null || cp.Unit == null || GetPlayerId(GetOwningPlayer(cp.Unit)) >= 24)
      {
        return null;
      }

      return GetOwningPlayer(cp.Unit).GetPlayerData().Faction;
    }

    // is the immobile Old God (a legend, e.g. N'Zoth) still alive? Not in the game -> counts as dead.
    private static bool OldGodAlive(int unitType)
    {
      var all = LegendaryHeroManager.GetAll();
      for (var i = 0; i < all.Count; i++)
      {
        var u = all[i].Unit;
        if (u != null && GetUnitTypeId(u) == unitType)
        {
          return GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;
        }
      }

      return false;
    }

    private static void RemovePreplacedRiftPortal()
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, AiConfig.RiftHighlandsX, AiConfig.RiftHighlandsY, 300f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == AiConfig.RiftPreplacedPortalType)
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
    }

    // WL sets Thandol Span (destructible LT08 ~15695,457) invulnerable and never opens it, so bots can never
    // cross Arathi <-> Dun Modr. Force it open at 20 min, matching that edge's OpenTime.
    // 0.36 note: NOT called anywhere (since 0.35 at least) -- WL's own timing applies. See TODO open questions.
    private static void OpenThandolIfDue()
    {
      if (_thandolOpened || SimpleBot._clock < 1200f)
      {
        return;
      }

      _thandolOpened = true;
      var best = FindDestructableNear(1280585784, 15695f, 457f, 2000f);
      if (best != null)
      {
        SetDestructableInvulnerable(best, false);
        KillDestructable(best);
        PingMinimap(15695f, 457f, 4f);
      }
    }

    private static destructable _destFound;
    private static int _destWant;
    private static float _destBx, _destBy, _destBest;
    private static destructable FindDestructableNear(int typeId, float x, float y, float r)
    {
      _destFound = null;
      _destWant = typeId;
      _destBx = x;
      _destBy = y;
      _destBest = r * r;
      if (SimpleBot._itemRect == null)
      {
        SimpleBot._itemRect = Rect(0f, 0f, 1f, 1f);
      }

      SetRect(SimpleBot._itemRect, x - r, y - r, x + r, y + r);
      EnumDestructablesInRect(SimpleBot._itemRect, null, PickDestructable);
      return _destFound;
    }

    private static void PickDestructable()
    {
      var d = GetEnumDestructable();
      if (GetDestructableTypeId(d) != _destWant)
      {
        return;
      }

      var ex = GetDestructableX(d) - _destBx;
      var ey = GetDestructableY(d) - _destBy;
      var dist = ex * ex + ey * ey;
      if (dist <= _destBest)
      {
        _destBest = dist;
        _destFound = d;
      }
    }
  }
}
