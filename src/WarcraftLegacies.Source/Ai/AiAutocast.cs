using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.71.0: autocast ON for bot (and autopilot) units. A computer player without a melee AI script never switches
  // autocast on, so Heal, Inner Fire ("Powered Up Ammunition"), Slow, Frost Armor, searing / cold / black arrows,
  // Bloodlust, Raise Dead, ... stayed idle. The table (AiAutocastData, gen_spells.py) lists every autocast ability a
  // unit type carries -- custom ones by their base -- with the order that turns it on. Once switched on, WC3 casts
  // it like for a human player: no decision or scan of ours. Cost: the army pass hands units over (type lookup); the
  // "autocast" task looks at a few units per second, each unit until all its rows are on (heroes: once learned;
  // abilities that need a research: once researched -- retried every Retry seconds). Follows -spells (Mode, bit).
  internal static class AiAutocast
  {
    private const float Retry = 20f;
    private const int LookPerTick = 30;
    private static Dictionary<int, List<int>> _rows = new(); // unit type -> rows
    private static List<int> _order = new();                // row -> OrderId (0 = unknown order, skipped)
    private static List<unit> _units = new();
    private static Dictionary<unit, int> _mask = new();      // unit -> rows (bits of its RowsOf list) switched on
    private static Dictionary<unit, float> _next = new();
    private static Dictionary<unit, int> _tries = new();    // 0.71.1: passes with refused orders
    private const int TryLimit = 3;
    private static int _cursor;
    private static bool _loaded;
    internal static int Switched;
    internal static int Refused;
    internal static int GaveUp; // 0.71.1: rows refused TryLimit times -- most likely ON already (computer players)
    internal static int UnknownOrders;
    internal static int Units;
    internal static int[] SwitchedBy = new int[24];

    internal static void Setup()
    {
      AiScheduler.Add("autocast", 4, 3, Tick, true); // every 1 s
    }

    private static void Ensure()
    {
      if (_loaded)
      {
        return;
      }

      _loaded = true;
      for (var i = 0; i < AiAutocastData.Id.Length; i++)
      {
        var o = OrderId(AiAutocastData.Order[i]);
        if (o == 0)
        {
          UnknownOrders++;
        }

        _order.Add(o);
      }

      for (var k = 0; k < AiAutocastData.CarrierType.Length && k < AiAutocastData.CarrierRow.Length; k++)
      {
        var type = AiAutocastData.CarrierType[k];
        if (!_rows.TryGetValue(type, out var list))
        {
          list = new List<int>();
          _rows[type] = list;
        }

        if (list.Count < 30 && !list.Contains(AiAutocastData.CarrierRow[k]))
        {
          list.Add(AiAutocastData.CarrierRow[k]);
        }
      }
    }

    // army pass (SimpleBot.Process): every living non-worker army unit of a bot
    internal static void Note(unit u)
    {
      if (AiSpells.Mode == AiAb.Off || _mask.ContainsKey(u))
      {
        return;
      }

      Ensure();
      if (!_rows.ContainsKey(GetUnitTypeId(u)))
      {
        return;
      }

      _mask[u] = 0;
      _units.Add(u);
      Units++;
    }

    private static void Tick()
    {
      if (AiSpells.Mode == AiAb.Off || _units.Count == 0)
      {
        return;
      }

      var looked = 0;
      while (looked < LookPerTick && _units.Count > 0)
      {
        looked++;
        if (_cursor >= _units.Count)
        {
          _cursor = 0;
        }

        var u = _units[_cursor];
        var type = GetUnitTypeId(u);
        // removed from the game, or a dead non-hero (heroes are revived as the same unit)
        if (type == 0 || (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f && !IsUnitType(u, UNIT_TYPE_HERO)))
        {
          _mask.Remove(u);
          _next.Remove(u);
          _tries.Remove(u);
          _units.RemoveAt(_cursor);
          continue;
        }

        _cursor++;
        if (_next.TryGetValue(u, out var due) && SimpleBot.Clock < due)
        {
          continue;
        }

        _next[u] = SimpleBot.Clock + Retry;
        var p = GetOwningPlayer(u);
        var pid = GetPlayerId(p);
        if (pid < 0 || pid >= 24 || !SimpleBot.IsBot(p) || !AiAb.For(AiSpells.Mode, pid, AiAb.BitSpells)
            || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        if (!_rows.TryGetValue(type, out var rows))
        {
          continue; // changed type (morph): kept, nothing to switch for this form
        }

        var mask = _mask[u];
        var all = true;
        var refusedBits = 0;
        for (var i = 0; i < rows.Count; i++)
        {
          var bit = 1 << i;
          if ((mask & bit) != 0)
          {
            continue;
          }

          var row = rows[i];
          var order = _order[row];
          if (order == 0 || GetUnitAbilityLevel(u, AiAutocastData.Id[row]) <= 0)
          {
            all = false; // not learned / not on this unit (yet)
            continue;
          }

          if (IssueImmediateOrderById(u, order))
          {
            mask |= bit;
            Switched++;
            SwitchedBy[pid]++;
          }
          else
          {
            Refused++; // a research the ability still needs, or autocast is ON already
            refusedBits |= bit;
            all = false;
          }
        }

        if (refusedBits != 0)
        {
          // 0.71.1: 0.71.0 retried every 20 s for ever (4998 refusals by 13:08, 26 switched -- 12 of them on the
          // autopilot's HUMAN slot): WC3 seems to switch autocast on for computer players itself, so "on" is
          // refused. After TryLimit passes (~1 min) the row counts as done.
          var t = _tries.TryGetValue(u, out var nt) ? nt + 1 : 1;
          _tries[u] = t;
          if (t >= TryLimit)
          {
            mask |= refusedBits;
            for (var b = refusedBits; b != 0; b = b & (b - 1))
            {
              GaveUp++;
            }

            // done when nothing switchable is left (unlearned hero skills keep the unit in the 20-s retry)
            all = true;
            for (var i = 0; i < rows.Count; i++)
            {
              if ((mask & (1 << i)) == 0)
              {
                all = false;
              }
            }
          }
        }

        _mask[u] = mask;
        if (all)
        {
          _next[u] = SimpleBot.Clock + 100000f; // all on: never looked at again (kept to stay known)
        }
      }
    }

    internal static void Lines(List<string> o)
    {
      Ensure();
      var s = "autocast (0.71): table " + I2S(AiAutocastData.Id.Length) + " abilities on " + I2S(_rows.Count)
              + " unit types (unknown orders " + I2S(UnknownOrders) + "); units seen " + I2S(Units) + ", tracked "
              + I2S(_units.Count) + "; switched on " + I2S(Switched) + ", refused " + I2S(Refused)
              + " (0.71.1: given up after " + I2S(TryLimit) + " tries -- ON already? -- " + I2S(GaveUp) + "); per bot:";
      for (var pid = 0; pid < 24; pid++)
      {
        if (SwitchedBy[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(SwitchedBy[pid]) + ";";
        }
      }

      o.Add(s);
    }
  }
}
