using System.Collections.Generic;
using MacroTools.Extensions;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.1: fallback scenarios, first slice. Creates one trigger building per player slot whose faction has a fallback
  // (FallbackConfig), watches the HQ CPs, and runs the fallback: bots at once, humans through the building's
  // "Begin the Fallback" ability. Not yet: team change, hidden start bases, unit packages (FALLBACKS.md).
  internal static class FallbackSystem
  {
    private static readonly Dictionary<int, unit> _building = new();      // pid -> trigger building
    private static readonly Dictionary<int, FallbackDef> _def = new();    // pid -> its fallback
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
      for (var pid = 0; pid < 24; pid++)
      {
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

        var x = FallbackConfig.GridX + FallbackConfig.GridStep * (pid % 12);
        var y = pid < 12 ? FallbackConfig.Row1Y : FallbackConfig.Row2Y;
        var u = CreateUnit(p, FallbackConfig.TriggerBuildingType, x, y, 270f);
        BlzSetUnitName(u, "Fallback: " + def.Name);
        _building[pid] = u;
        _def[pid] = def;
      }

      SimpleBot.Log("fallbacks: " + I2S(_building.Count) + " trigger building(s) created");
    }

    private static void Check()
    {
      foreach (var kv in _def)
      {
        var pid = kv.Key;
        var def = kv.Value;
        if (def.Mode != "spawn" || _done.Contains(pid) || _ready.Contains(pid))
        {
          continue;
        }

        var cp = SimpleBot.SafeCp(def.HqCpId);
        if (cp == null || cp.Unit == null)
        {
          continue;
        }

        var p = Player(pid);
        if (GetOwningPlayer(cp.Unit) == p)
        {
          _hadHq.Add(pid);
          continue;
        }

        if (!_hadHq.Contains(pid))
        {
          continue; // never held it -> not "lost"
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

    private static void Execute(int pid)
    {
      if (!_def.ContainsKey(pid))
      {
        return;
      }

      var def = _def[pid];
      var p = Player(pid);
      _done.Add(pid);
      CreateUnit(p, def.HeroType, def.X, def.Y, 270f);
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
