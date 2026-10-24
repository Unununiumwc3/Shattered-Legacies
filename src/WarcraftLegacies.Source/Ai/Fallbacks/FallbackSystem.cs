using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.1: fallback scenarios, first slice. Creates one trigger building per player slot whose faction has a fallback
  // (FallbackConfig), watches the HQ CPs, and runs the fallback: bots at once, humans through the building's
  // "Begin the Fallback" ability. Not yet: team change, hidden start bases, unit packages (FALLBACKS.md).
  internal static class FallbackSystem
  {
    private static readonly Dictionary<int, unit> _building = new();      // pid -> trigger building
    private static readonly Dictionary<int, FallbackDef> _def = new();    // pid -> its fallback
    private static readonly Dictionary<int, List<unit>> _hqUnits = new(); // pid -> its HQ units (found at creation)
    private static readonly HashSet<int> _hadHq = new();                  // pid held its HQ CP at least once
    private static readonly HashSet<int> _ready = new();                  // HQ lost -> fallback available
    private static readonly HashSet<int> _done = new();

    internal static void Setup()
    {
      var create = CreateTimer();
      TimerStart(create, FallbackConfig.CreateAt, false, CreateBuildings);
      var check = CreateTimer();
      TimerStart(check, FallbackConfig.CheckEvery, true, () => SimpleBot.Safe("fallbacks", Check));
      var cast = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(cast, Player(i), EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      }

      TriggerAddAction(cast, OnCast);
    }

    private static void CreateBuildings()
    {
      var teams = FactionManager.GetAllTeams();
      var order = new List<int>(); // 0.39.7: pids sorted by team (scoreboard order), then slot
      for (var t = 0; t <= teams.Count; t++)
      {
        for (var q = 0; q < 24; q++)
        {
          var team = Player(q).GetPlayerData().Team;
          var ti = teams.Count;
          for (var k = 0; k < teams.Count; k++)
          {
            if (team == teams[k])
            {
              ti = k;
            }
          }

          if (ti == t)
          {
            order.Add(q);
          }
        }
      }

      var placedCount = 0;
      for (var oi = 0; oi < order.Count; oi++)
      {
        var pid = order[oi];
        var p = Player(pid);
        var fac = p.GetPlayerData().Faction;
        if (fac == null)
        {
          continue;
        }

        FallbackDef def = null;
        for (var i = 0; i < FallbackConfig.All.Count; i++)
        {
          if (FallbackConfig.All[i].Faction == fac.Name)
          {
            def = FallbackConfig.All[i];
          }
        }

        if (def == null)
        {
          continue;
        }

        _hqUnits[pid] = FindHqUnits(def.Hq);
        // 0.39.7: middle column first, then left, then right (each RowsPerColumn high, going north)
        var colIndex = placedCount / FallbackConfig.RowsPerColumn; // 0 middle, 1 left, 2 right, 3 far left ...
        var row = placedCount % FallbackConfig.RowsPerColumn;
        placedCount++;
        var side = colIndex == 0 ? 0 : (colIndex % 2 == 1 ? -((colIndex + 1) / 2) : colIndex / 2);
        var x = FallbackConfig.CenterX + FallbackConfig.ColumnStep * side;
        var y = FallbackConfig.FirstRowY + FallbackConfig.RowStep * row;
        var u = CreateUnit(p, FallbackConfig.TriggerBuildingType, x, y, 270f);
        BlzSetUnitName(u, "Fallback: " + def.Name);
        _building[pid] = u;
        _def[pid] = def;
      }

      SimpleBot.Log("fallbacks: " + I2S(_building.Count) + " trigger building(s) created");
    }

    // 0.39.4: the HQ units (buildings / CPs) of the given types -- one whole-map scan, once at creation
    private static List<unit> FindHqUnits(int[] types)
    {
      var found = new List<unit>();
      if (types.Length == 0)
      {
        return found;
      }

      var g = CreateGroup();
      GroupEnumUnitsInRect(g, GetWorldBounds(), null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        for (var i = 0; i < types.Length; i++)
        {
          if (types[i] == t)
          {
            found.Add(u);
          }
        }
      }

      DestroyGroup(g);
      return found;
    }

    // 0.39.4: ALL HQ units lost (dead or owned by someone else) -> fallback available; any HQ held again -> unavailable
    // again (it has to fall again). Bots run it at once; a human gets / loses the ability on the trigger building.
    private static void Check()
    {
      foreach (var kv in _def)
      {
        var pid = kv.Key;
        var def = kv.Value;
        if (def.Mode != "spawn" || _done.Contains(pid) || !_hqUnits.ContainsKey(pid) || _hqUnits[pid].Count == 0)
        {
          continue;
        }

        var p = Player(pid);
        var held = 0;
        var hqs = _hqUnits[pid];
        for (var i = 0; i < hqs.Count; i++)
        {
          var u = hqs[i];
          if (GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && GetOwningPlayer(u) == p)
          {
            held++;
          }
        }

        if (held > 0)
        {
          _hadHq.Add(pid);
          if (_ready.Contains(pid))
          {
            _ready.Remove(pid); // recaptured -> must fall again
            if (_building.ContainsKey(pid))
            {
              UnitRemoveAbility(_building[pid], FallbackConfig.AbilityId);
            }

            DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00Fallback no longer available:|r your HQ is held again.");
          }

          continue;
        }

        if (!_hadHq.Contains(pid) || _ready.Contains(pid))
        {
          continue; // never held -> not "lost"; or already announced
        }

        _ready.Add(pid);
        if (SimpleBot.IsBot(p) && SimpleBot.HasPerks(p))
        {
          Execute(pid); // bots fall back at once
        }
        else if (_building.ContainsKey(pid))
        {
          UnitAddAbility(_building[pid], FallbackConfig.AbilityId);
          DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00Your fallback is ready:|r " + def.Name + ". Use the Fallback "
            + "building in the north-east corner of the map when you want to start it.");
          SimpleBot.LogPin("fallback ready for P" + I2S(pid) + " " + def.Faction + " (human decides)");
        }
      }
    }

    private static void OnCast()
    {
      if (GetSpellAbilityId() != FallbackConfig.AbilityId)
      {
        return;
      }

      var pid = GetPlayerId(GetOwningPlayer(GetTriggerUnit()));
      if (_ready.Contains(pid) && !_done.Contains(pid))
      {
        Execute(pid);
      }
    }

    // 0.39.5 (maintainer's design): everything Kul Tiras owns goes to Stormwind (if Stormwind still plays) or is removed
    // -- except Katherine (moved to Kezan) and the trigger building. Kezan and the Neutral Hostile creeps around it
    // become Kul Tiras'. Kul Tiras loses all its allies. Mercenary units from the 24p data come later (FALLBACKS.md).
    private static void KulTirasFallback(player p, FallbackDef def)
    {
      player stormwind = null;
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == "Stormwind" && f.Player != null && SimpleBot.IsActive(f.Player))
        {
          stormwind = f.Player;
        }
      }

      unit katherine = null;
      var legends = LegendaryHeroManager.GetAll();
      for (var i = 0; i < legends.Count; i++)
      {
        if (legends[i].Name == "Katherine Proudmoore")
        {
          katherine = legends[i].Unit;
        }
      }

      var pid = GetPlayerId(p);
      var trigger = _building.ContainsKey(pid) ? _building[pid] : null;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (u == katherine || u == trigger)
        {
          continue;
        }

        if (stormwind != null)
        {
          SetUnitOwner(u, stormwind, true);
        }
        else
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
      if (katherine != null && GetUnitState(katherine, UNIT_STATE_LIFE) > 0.405f)
      {
        SetUnitPosition(katherine, def.X, def.Y);
      }

      var kezan = SimpleBot.SafeCp(1848652890); // n04Z Kezan
      unit kezanUnit = null;
      if (kezan != null && kezan.Unit != null)
      {
        kezanUnit = kezan.Unit;
        SimpleBot.GiveUnit(kezanUnit, p);
      }

      // 0.39.7: everything neutral on Kezan (Regions.KezanAmbient) joins: the goblin buildings there (Scrapyard,
      // Bunker, Laboratory, Rocket Tower, Oil Supply ...) and the units -- passive AND hostile
      var area = Rect(-3200f, -10368f, 736f, -5184f);
      var g2 = CreateGroup();
      GroupEnumUnitsInRect(g2, area, null);
      var taken = 0;
      while ((u = FirstOfGroup(g2)) != null)
      {
        GroupRemoveUnit(g2, u);
        var o = GetPlayerId(GetOwningPlayer(u));
        if ((o == PLAYER_NEUTRAL_AGGRESSIVE || o == PLAYER_NEUTRAL_PASSIVE) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
            && u != kezanUnit)
        {
          SetUnitOwner(u, p, true);
          taken++;
        }
      }

      DestroyGroup(g2);
      RemoveRect(area);
      SimpleBot.LogPin("Kezan: " + I2S(taken) + " neutral buildings / units joined");

      // 0.39.6: the faction becomes "The Kezan Enterprise" -- renamed, goblin start buildings, its units allowed
      var fac = p.GetPlayerData().Faction;
      if (fac != null)
      {
        fac.Name = FallbackConfig.KezanName;
      }

      for (var i = 0; i < FallbackConfig.KezanUnits.Length; i++)
      {
        SetPlayerTechMaxAllowed(p, FallbackConfig.KezanUnits[i], 100);
      }

      for (var i = 0; i < FallbackConfig.KezanHeroes.Length; i++)
      {
        SetPlayerTechMaxAllowed(p, FallbackConfig.KezanHeroes[i], 1); // 0.39.7: all three Altar of Industry heroes
      }

      fac?.Unally(); // 0.39.7: WL's own unally -- new team "<name> Pact", the scoreboard follows
      KezanEnterprise.Start(p); // 0.39.8: oil income, creep market
      var usedX = new List<float>();
      var usedY = new List<float>();
      var placed = 0;
      for (var i = 0; i < FallbackConfig.KezanBuildings.Length; i++)
      {
        if (AiLegion.FindBuildSpot(def.X, def.Y, usedX, usedY))
        {
          CreateUnit(p, FallbackConfig.KezanBuildings[i], SimpleBot._spotX, SimpleBot._spotY, 270f);
          usedX.Add(SimpleBot._spotX);
          usedY.Add(SimpleBot._spotY);
          placed++;
        }
      }

      SimpleBot.LogPin("Kul Tiras fallback -> The Kezan Enterprise: old army to " + (stormwind != null ? "Stormwind" : "removed")
                       + ", " + I2S(placed) + " goblin buildings, own team (Pact)");
    }

    private static void Execute(int pid)
    {
      if (!_def.ContainsKey(pid))
      {
        return;
      }

      var def = _def[pid];
      var p = Player(pid);
      _done.Add(pid);
      if (def.Special == "kultiras")
      {
        KulTirasFallback(p, def);
      }

      if (def.HeroType != 0)
      {
        CreateUnit(p, def.HeroType, def.X, def.Y, 270f);
      }
      if (_building.ContainsKey(pid))
      {
        UnitRemoveAbility(_building[pid], FallbackConfig.AbilityId);
        BlzSetUnitName(_building[pid], "Fallback: " + def.Name + " (used)");
      }

      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, def.Announcement);
      }

      PingMinimap(def.X, def.Y, 5f);
      SimpleBot.LogPin("FALLBACK P" + I2S(pid) + " " + def.Faction + " -> " + def.Name);
    }
  }
}
