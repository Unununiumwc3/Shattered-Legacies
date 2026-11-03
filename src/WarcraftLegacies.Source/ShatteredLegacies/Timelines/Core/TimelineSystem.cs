using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // The timeline engine (was FallbackSystem, 0.39.1): watches each registered faction's HQ and runs its turning point
  // -- bots at once, humans through the Hourglass. Faction-specific logic lives in Timelines/Factions/<X>/ and is
  // hooked in through TurningPointDef.Run. Not yet (TODO.md): turning points as WL quests with Hourglass researches,
  // the Defeat hook, team change, hidden start bases, the light / full effect library.
  internal static class TimelineSystem
  {
    internal const float CheckEvery = 5f;

    private static readonly Dictionary<int, TurningPointDef> _def = new(); // pid -> its turning point
    private static readonly Dictionary<int, List<unit>> _hqUnits = new();  // pid -> its HQ units (found at creation)
    private static readonly HashSet<int> _hadHq = new();                   // pid held its HQ at least once
    private static readonly HashSet<int> _ready = new();                   // HQ lost -> turning point available
    private static readonly HashSet<int> _done = new();

    internal static void Setup()
    {
      Hourglass.Setup();
      var check = CreateTimer();
      TimerStart(check, CheckEvery, true, () => AiApi.Safe("timelines", Check));
    }

    // called by the Hourglass for every slot that gets a building (0.39.4: one whole-map scan for the HQ units)
    internal static void Register(int pid, TurningPointDef def)
    {
      _hqUnits[pid] = FindHqUnits(def.Hq);
      _def[pid] = def;
    }

    internal static void OnHourglassUsed(int pid)
    {
      if (_ready.Contains(pid) && !_done.Contains(pid))
      {
        Execute(pid);
      }
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

    // 0.39.4: ALL HQ units lost (dead or owned by someone else) -> turning point available; any HQ held again ->
    // unavailable again (it has to fall again). Bots run it at once; a human gets / loses the Hourglass ability.
    private static void Check()
    {
      foreach (var kv in _def)
      {
        var pid = kv.Key;
        var def = kv.Value;
        if (def.Source != TurningPointSource.Timeline || _done.Contains(pid) || !_hqUnits.ContainsKey(pid)
            || _hqUnits[pid].Count == 0)
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
            Hourglass.Withdraw(pid);
            DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00Fallback no longer available:|r your HQ is held again.");
          }

          continue;
        }

        if (!_hadHq.Contains(pid) || _ready.Contains(pid))
        {
          continue; // never held -> not "lost"; or already announced
        }

        _ready.Add(pid);
        if (AiApi.IsBot(p) && AiApi.HasPerks(p))
        {
          Execute(pid); // bots run it at once
        }
        else if (Hourglass.Has(pid))
        {
          Hourglass.Offer(pid);
          DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00Your fallback is ready:|r " + def.Name + ". Use the Fallback "
            + "building in the north-east corner of the map when you want to start it.");
          AiApi.LogPin("turning point ready for P" + I2S(pid) + " " + def.Faction + " (human decides)");
        }
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
      if (def.Run != null)
      {
        def.Run(p, def);
      }

      if (def.HeroType != 0)
      {
        CreateUnit(p, def.HeroType, def.X, def.Y, 270f);
      }

      Hourglass.MarkUsed(pid, def);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, def.Announcement);
      }

      PingMinimap(def.X, def.Y, 5f);
      AiApi.LogPin("TURNING POINT P" + I2S(pid) + " " + def.Faction + " -> " + def.Name);
    }
  }
}
