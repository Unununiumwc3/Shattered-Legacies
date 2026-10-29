using System.Collections.Generic;
using MacroTools.Extensions;

namespace WarcraftLegacies.Source.Ai
{
  // 0.41 (refactor phase 2): army orders. Mode "smart" (default; -orders smart|idle|all switches for A/B tests):
  // - An ATTACK GROUP goes to the target: the units nearest to it until their food covers the need (enemy food near
  //   the target x 1.5 + structures there x 8, at least 50). If the need is 80 % of the army or more, the whole army
  //   goes. The REST waits at the staging point: the bot's own CP nearest to the target (same landmass).
  // - Orders only when needed: on a target change the group (all of it) and the rest (those away from the staging
  //   point); on the 20-s heartbeat only IDLE units that are AWAY from their point (group: 600 from the target,
  //   rest: 1500 from the staging point). Units standing at their point are never re-ordered (0.39's lever 1
  //   re-ordered them every heartbeat). A group that lost half its food is refilled with the nearest rest units.
  // - Defence, kill targets and "hold" use the whole army (with the same idle-and-away rule).
  // Units = the orderable army of the bot being serviced, filled by SimpleBot.Process (one unit pass per service).
  internal static class AiArmy
  {
    internal const float AtTarget = 600f;
    internal const float AtStage = 1500f;
    internal const float ScanRadius = 1200f;
    internal const int FloorFood = 50;
    internal const int StructureFood = 8;
    internal const float EnemyFactor = 1.5f;

    internal static bool Smart = true;
    internal static List<unit> Units = new();

    private static HashSet<unit>[] _group = new HashSet<unit>[24];
    private static bool[] _whole = new bool[24];
    private static bool[] _hasStage = new bool[24];
    private static float[] _stageX = new float[24];
    private static float[] _stageY = new float[24];
    private static string[] _stageName = new string[24];
    private static int[] _need = new int[24];
    private static int[] _groupFood = new int[24];
    private static int[] _armyFood = new int[24];
    private static bool[] _hasFinal = new bool[24]; // 0.42: final target (group size, staging) vs. waypoint (orders)
    private static float[] _fx = new float[24];
    private static float[] _fy = new float[24];

    internal static int OrdChange;  // target changed: group / whole army
    internal static int OrdRestMove; // target changed or heartbeat: rest to the staging point
    internal static int OrdIdleFar; // heartbeat: idle unit away from its point
    internal static int OrdRefill;  // heartbeat: new group members after losses
    internal static int OrdAdvance; // 0.42: same target, next waypoint: the group moves on

    // whole = defence / kill target / hold: every unit goes, no group. (x, y) = where to go now (waypoint or target),
    // (fx, fy) = the final target: a new final target picks a new group; a new waypoint only moves the group on.
    internal static void Attack(player p, int pid, float x, float y, bool changed, bool whole, float fx, float fy)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      SampleStall(pid); // 0.44

      if (whole)
      {
        _hasFinal[pid] = false;
        _whole[pid] = true;
        _hasStage[pid] = false;
        _group[pid]?.Clear();
        _need[pid] = 0;
        var food = 0;
        for (var i = 0; i < Units.Count; i++)
        {
          var u = Units[i];
          food += Food(u);
          if (changed)
          {
            Issue(u, x, y);
            OrdChange++;
          }
          else if (IsIdleAway(u, x, y, AtTarget))
          {
            Issue(u, x, y);
            OrdIdleFar++;
          }
        }

        _armyFood[pid] = food;
        _groupFood[pid] = food;
        return;
      }

      var grp = _group[pid];
      if (grp == null)
      {
        grp = new HashSet<unit>();
        _group[pid] = grp;
      }

      var newTarget = !_hasFinal[pid] || _whole[pid]
                      || (fx - _fx[pid]) * (fx - _fx[pid]) + (fy - _fy[pid]) * (fy - _fy[pid]) > 300f * 300f;
      _hasFinal[pid] = true;
      _fx[pid] = fx;
      _fy[pid] = fy;
      if (changed && !newTarget)
      {
        for (var i = 0; i < Units.Count; i++)
        {
          if (grp.Contains(Units[i]))
          {
            Issue(Units[i], x, y);
            OrdAdvance++;
          }
        }

        return;
      }

      if (newTarget)
      {
        _whole[pid] = false;
        Select(p, pid, fx, fy, grp, true);
        for (var i = 0; i < Units.Count; i++)
        {
          var u = Units[i];
          if (grp.Contains(u))
          {
            Issue(u, x, y);
            OrdChange++;
          }
          else if (_hasStage[pid] && !IsNear(u, _stageX[pid], _stageY[pid], AtStage))
          {
            Issue(u, _stageX[pid], _stageY[pid]);
            OrdRestMove++;
          }
        }

        return;
      }

      // heartbeat: refill after losses, then only idle units away from their point
      var groupFood = 0;
      var armyFood = 0;
      for (var i = 0; i < Units.Count; i++)
      {
        var f = Food(Units[i]);
        armyFood += f;
        if (grp.Contains(Units[i]))
        {
          groupFood += f;
        }
      }

      _armyFood[pid] = armyFood;
      _groupFood[pid] = groupFood;
      if (groupFood * 2 < _need[pid] && groupFood < armyFood)
      {
        var before = new HashSet<unit>(grp);
        Select(p, pid, fx, fy, grp, false);
        for (var i = 0; i < Units.Count; i++)
        {
          var u = Units[i];
          if (grp.Contains(u) && !before.Contains(u))
          {
            Issue(u, x, y);
            OrdRefill++;
          }
        }
      }

      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        if (grp.Contains(u))
        {
          if (IsIdleAway(u, x, y, AtTarget))
          {
            if (AiRoutes.NearWaygateExit(GetUnitX(u), GetUnitY(u)))
            {
              AiRoutes.ExitIdle++; // 0.44.1: queue lost in the teleport?
            }

            Issue(u, x, y);
            OrdIdleFar++;
          }
        }
        else if (_hasStage[pid] && IsIdleAway(u, _stageX[pid], _stageY[pid], AtStage))
        {
          Issue(u, _stageX[pid], _stageY[pid]);
          OrdRestMove++;
        }
      }
    }

    // 0.42: centre of the attack group (route start); unchanged when the bot has no group (whole army / none).
    // 0.42.1: centre of the group's biggest part on ONE landmass (like the army anchor, 0.36) -- a group half through
    // a waygate had its centre between the two worlds and was sent back and forth (Dark Portal).
    private static int[] _lmCount = new int[64];
    internal static void GroupCenter(int pid, ref float x, ref float y)
    {
      var grp = pid >= 0 && pid < 24 ? _group[pid] : null;
      if (grp == null || grp.Count == 0 || _whole[pid])
      {
        return;
      }

      for (var k = 0; k < 64; k++)
      {
        _lmCount[k] = 0;
      }

      var best = -1;
      for (var i = 0; i < Units.Count; i++)
      {
        if (grp.Contains(Units[i]))
        {
          var lm = Geography.Landmass(GetUnitX(Units[i]), GetUnitY(Units[i])) + 1;
          if (lm < 0 || lm >= 64)
          {
            lm = 0;
          }

          _lmCount[lm]++;
          if (best < 0 || _lmCount[lm] > _lmCount[best] || (best == 0 && lm > 0))
          {
            best = lm;
          }
        }
      }

      float sx = 0f, sy = 0f;
      var n = 0;
      for (var i = 0; i < Units.Count; i++)
      {
        if (!grp.Contains(Units[i]))
        {
          continue;
        }

        var lm = Geography.Landmass(GetUnitX(Units[i]), GetUnitY(Units[i])) + 1;
        if (lm < 0 || lm >= 64)
        {
          lm = 0;
        }

        if (lm == best)
        {
          sx += GetUnitX(Units[i]);
          sy += GetUnitY(Units[i]);
          n++;
        }
      }

      if (n > 0)
      {
        x = sx / n;
        y = sy / n;
      }
    }

    // Group = nearest units until their food covers the need; fresh = also recompute need and staging point.
    private static void Select(player p, int pid, float x, float y, HashSet<unit> grp, bool fresh)
    {
      if (fresh)
      {
        _need[pid] = Need(p, x, y);
        PickStage(p, pid, x, y);
      }

      grp.Clear();
      var n = Units.Count;
      var armyFood = 0;
      var d2 = new float[n];
      var taken = new bool[n];
      for (var i = 0; i < n; i++)
      {
        var dx = GetUnitX(Units[i]) - x;
        var dy = GetUnitY(Units[i]) - y;
        d2[i] = dx * dx + dy * dy;
        armyFood += Food(Units[i]);
      }

      _armyFood[pid] = armyFood;
      var all = _need[pid] * 5 >= armyFood * 4; // need >= 80 % of the army -> everybody goes
      var food = 0;
      while (food < _need[pid] || all)
      {
        var best = -1;
        for (var i = 0; i < n; i++)
        {
          if (!taken[i] && (best < 0 || d2[i] < d2[best]))
          {
            best = i;
          }
        }

        if (best < 0)
        {
          break;
        }

        taken[best] = true;
        grp.Add(Units[best]);
        food += Food(Units[best]);
      }

      _groupFood[pid] = food;
    }

    // enemy food near the target x 1.5 + enemy structures there x 8 (towers, the CP's defenders), at least FloorFood.
    // Enemy = an enemy player's unit (IsPlayerEnemy) or Neutral Hostile (24: creeps guarding neutral CPs).
    private static int Need(player p, float x, float y)
    {
      var food = 0;
      var structures = 0;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, ScanRadius, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.OpUnits++;
        if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var owner = GetOwningPlayer(u);
        var oid = GetPlayerId(owner);
        var enemy = oid == 24 || (oid < 24 && owner != p && !IsUnitAlly(u, p) && IsPlayerEnemy(owner, p));
        if (!enemy)
        {
          continue;
        }

        if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
        {
          structures++;
        }
        else
        {
          food += Food(u);
        }
      }

      DestroyGroup(g);
      var need = R2I(food * EnemyFactor) + structures * StructureFood;
      return need < FloorFood ? FloorFood : need;
    }

    // the bot's own CP nearest to the target (not the target itself), on the target's landmass when both are known
    private static void PickStage(player p, int pid, float x, float y)
    {
      _hasStage[pid] = false;
      _stageName[pid] = "-";
      var targetLm = Geography.Landmass(x, y);
      var best = -1f;
      var mine = p.GetPlayerData().ControlPoints;
      for (var i = 0; i < mine.Count; i++)
      {
        var cu = mine[i].Unit;
        if (cu == null)
        {
          continue;
        }

        var cx = GetUnitX(cu);
        var cy = GetUnitY(cu);
        var dx = cx - x;
        var dy = cy - y;
        var dd = dx * dx + dy * dy;
        if (dd < AtTarget * AtTarget)
        {
          continue; // the target itself (or right next to it)
        }

        var lm = Geography.Landmass(cx, cy);
        if (targetLm >= 0 && lm >= 0 && lm != targetLm)
        {
          continue;
        }

        if (best < 0f || dd < best)
        {
          best = dd;
          _hasStage[pid] = true;
          _stageX[pid] = cx;
          _stageY[pid] = cy;
          _stageName[pid] = GetUnitName(cu);
        }
      }
    }

    private static int Food(unit u)
    {
      var f = GetUnitFoodUsed(u);
      return f < 1 ? 1 : f;
    }

    private static bool IsNear(unit u, float x, float y, float r)
    {
      var dx = GetUnitX(u) - x;
      var dy = GetUnitY(u) - y;
      return dx * dx + dy * dy <= r * r;
    }

    private static bool IsIdleAway(unit u, float x, float y, float r)
    {
      return GetUnitCurrentOrder(u) == 0 && !IsNear(u, x, y, r);
    }

    // 0.44: a chain along the graph when possible (AiRoutes.Chain), else one attack-move
    internal static void Issue(unit u, float x, float y)
    {
      if (AiRoutes.Chain(GetOwningPlayer(u), u, x, y))
      {
        return;
      }

      SimpleBot.OpOrders++;
      SimpleBot.OrdersIssued++;
      AiScheduler.CountOrder(u, x, y);
      IssuePointOrder(u, "attack", x, y);
    }

    // ---- 0.44: stalled units (engine overload vs. target flapping) ------------------------------------------
    // A unit that has an order, has not moved 100 since the bot's last army order round, and has not lost health is
    // "stalled" -- the signature of an overloaded engine (orders wait in the pathing queue). Flapping instead shows as
    // switches (AiScheduler.TargetSwitch / StateSwitch). Ranged units shooting without being hit count as stalled
    // too, so compare the share between games, not as an absolute truth.
    private static Dictionary<unit, float>[] _sx = new Dictionary<unit, float>[24];
    private static Dictionary<unit, float>[] _sy = new Dictionary<unit, float>[24];
    private static Dictionary<unit, float>[] _sl = new Dictionary<unit, float>[24];
    internal static int StallSeen;
    internal static int StallStalled;

    private static void SampleStall(int pid)
    {
      if (_sx[pid] == null)
      {
        _sx[pid] = new Dictionary<unit, float>();
        _sy[pid] = new Dictionary<unit, float>();
        _sl[pid] = new Dictionary<unit, float>();
      }

      var sx = _sx[pid];
      var sy = _sy[pid];
      var sl = _sl[pid];
      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        float x = GetUnitX(u), y = GetUnitY(u), life = GetUnitState(u, UNIT_STATE_LIFE);
        if (GetUnitCurrentOrder(u) != 0 && sx.ContainsKey(u))
        {
          StallSeen++;
          var dx = x - sx[u];
          var dy = y - sy[u];
          if (dx * dx + dy * dy < 100f * 100f && life >= sl[u])
          {
            StallStalled++;
          }
        }

        sx[u] = x;
        sy[u] = y;
        sl[u] = life;
      }

      if (sx.Count > Units.Count * 2 + 64)
      {
        sx.Clear(); // dead units pile up -- start over now and then (one round without samples)
        sy.Clear();
        sl.Clear();
      }
    }

    // ---- readouts -------------------------------------------------------------------------------------------

    internal static string ShortLine()
    {
      return "army " + (Smart ? "smart" : "old") + " orders: change=" + I2S(OrdChange) + " idle-away="
             + I2S(OrdIdleFar) + " rest=" + I2S(OrdRestMove) + " refill=" + I2S(OrdRefill) + " advance="
             + I2S(OrdAdvance) + " | chains " + (AiRoutes.Chains ? "on" : "off") + " issued=" + I2S(AiRoutes.ChainsIssued)
             + " points=" + I2S(AiRoutes.ChainPoints) + " fallback=" + I2S(AiRoutes.ChainFallback) + " | stalled "
             + I2S(StallSeen > 0 ? StallStalled * 100 / StallSeen : 0) + "% of " + I2S(StallSeen);
    }

    internal static void Lines(List<string> o)
    {
      o.Add(ShortLine());
      var s = "groups (food group/need of army @staging):";
      for (var pid = 0; pid < 24; pid++)
      {
        if (_armyFood[pid] == 0)
        {
          continue;
        }

        s = s + " P" + I2S(pid) + " " + (_whole[pid] ? "all " + I2S(_armyFood[pid])
          : I2S(_groupFood[pid]) + "/" + I2S(_need[pid]) + " of " + I2S(_armyFood[pid]) + " @"
            + (_hasStage[pid] ? _stageName[pid] : "-")) + ";";
      }

      o.Add(s);
      var sw = "switches per bot (target/state, since start):";
      for (var pid = 0; pid < 24; pid++)
      {
        if (AiScheduler.TargetSwitch[pid] + AiScheduler.StateSwitch[pid] > 0)
        {
          sw = sw + " P" + I2S(pid) + " " + I2S(AiScheduler.TargetSwitch[pid]) + "/" + I2S(AiScheduler.StateSwitch[pid]) + ";";
        }
      }

      o.Add(sw);
    }
  }
}
