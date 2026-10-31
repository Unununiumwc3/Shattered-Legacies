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

    private static HashSet<unit>[] _group = new HashSet<unit>[AiTheatres.Agents];
    private static bool[] _whole = new bool[AiTheatres.Agents];
    private static bool[] _hasStage = new bool[AiTheatres.Agents];
    private static float[] _stageX = new float[AiTheatres.Agents];
    private static float[] _stageY = new float[AiTheatres.Agents];
    private static string[] _stageName = new string[AiTheatres.Agents];
    private static int[] _need = new int[AiTheatres.Agents];
    private static int[] _groupFood = new int[AiTheatres.Agents];
    private static int[] _armyFood = new int[AiTheatres.Agents];
    private static bool[] _hasFinal = new bool[AiTheatres.Agents]; // 0.42: final target (group size, staging) vs. waypoint (orders)
    private static float[] _fx = new float[AiTheatres.Agents];
    private static float[] _fy = new float[AiTheatres.Agents];

    internal static int OrdChange;  // target changed: group / whole army
    internal static int OrdRestMove; // target changed or heartbeat: rest to the staging point
    internal static int OrdIdleFar; // heartbeat: idle unit away from its point
    internal static int OrdRefill;  // heartbeat: new group members after losses
    internal static int OrdAdvance; // 0.42: same target, next waypoint: the group moves on
    internal static int OrdExit;    // 0.44.3: idle at a waygate exit, re-chained on the next service
    internal static int[] IdleBy = new int[24]; // 0.46: idle-away orders per bot (guard-position A/B test)
    internal static int[] RestBy = new int[24]; // 0.46: rest orders per bot
    internal static int Unreachable; // 0.46: units left out of an attack group -- no way to the target
    internal static int[] StallSeenBy = new int[24]; // 0.46: per bot, for the spread A/B test
    internal static int[] StallBy = new int[24];
    internal static float LastShare; // 0.46: share of the group's food at the gather point (GroupAssembled)
    internal static bool DefendCall;   // 0.46.4: set around a defence order (AiSystem)
    internal static int DefendKept;    // 0.46.4: unit orders skipped -- other landmass than the defended CP
    private const float StageKeep = 2500f; // 0.46.2: keep the staging point if the new one is this close
    internal static int StageKept;         // 0.46.2: staging kept (new one close), set on the route
    internal static int StageRoute;

    internal static int ArmyFood(int pid) => pid >= 0 && pid < 24 ? _armyFood[AiTheatres.S(pid)] : 0;

    // 0.45.0: has share of the attack group's food arrived within r of (x, y)? (gather points)
    internal static bool GroupAssembled(int pid, float x, float y, float r, float share)
    {
      var grp = pid >= 0 && pid < 24 ? _group[AiTheatres.S(pid)] : null;
      if (grp == null || grp.Count == 0)
      {
        return true;
      }

      var total = 0;
      var near = 0;
      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        if (!grp.Contains(u))
        {
          continue;
        }

        var f = Food(u);
        total += f;
        if (IsNear(u, x, y, r))
        {
          near += f;
        }
      }

      LastShare = total > 0 ? (float)near / total : 1f;
      return total == 0 || near >= total * share;
    }

    // whole = defence / kill target / hold: every unit goes, no group. (x, y) = where to go now (waypoint or target),
    // (fx, fy) = the final target: a new final target picks a new group; a new waypoint only moves the group on.
    internal static void Attack(player p, int pid, float x, float y, bool changed, bool whole, float fx, float fy)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      SampleStall(pid); // 0.44

      if (whole && DefendCall && GroupFarFrom(pid, x, y))
      {
        DefendWithRest(pid, x, y, changed); // 0.47: the attack group keeps its front
        return;
      }

      if (whole)
      {
        _hasFinal[AiTheatres.S(pid)] = false;
        _whole[AiTheatres.S(pid)] = true;
        _hasStage[AiTheatres.S(pid)] = false;
        _group[AiTheatres.S(pid)]?.Clear();
        _need[AiTheatres.S(pid)] = 0;
        var food = 0;
        var defLm = DefendCall ? Geography.Landmass(x, y) : -1;
        for (var i = 0; i < Units.Count; i++)
        {
          var u = Units[i];
          food += Food(u);
          if (defLm >= 0)
          {
            var ulm = Geography.Landmass(GetUnitX(u), GetUnitY(u));
            if (ulm >= 0 && ulm != defLm)
            {
              DefendKept++; // 0.46.4: fighting on another landmass -- the defence does not pull it across the sea
              continue;
            }
          }

          if (changed)
          {
            Issue(u, x, y);
            OrdChange++;
          }
          else if (IsIdleAway(u, x, y, AtTarget))
          {
            Issue(u, x, y);
            OrdIdleFar++;
            IdleBy[pid]++;
          }
        }

        _armyFood[AiTheatres.S(pid)] = food;
        _groupFood[AiTheatres.S(pid)] = food;
        return;
      }

      var grp = _group[AiTheatres.S(pid)];
      if (grp == null)
      {
        grp = new HashSet<unit>();
        _group[AiTheatres.S(pid)] = grp;
      }

      var newTarget = !_hasFinal[AiTheatres.S(pid)] || _whole[AiTheatres.S(pid)]
                      || (fx - _fx[AiTheatres.S(pid)]) * (fx - _fx[AiTheatres.S(pid)]) + (fy - _fy[AiTheatres.S(pid)]) * (fy - _fy[AiTheatres.S(pid)]) > 300f * 300f;
      _hasFinal[AiTheatres.S(pid)] = true;
      _fx[AiTheatres.S(pid)] = fx;
      _fy[AiTheatres.S(pid)] = fy;
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
        _whole[AiTheatres.S(pid)] = false;
        Select(p, pid, fx, fy, grp, true);
        for (var i = 0; i < Units.Count; i++)
        {
          var u = Units[i];
          if (grp.Contains(u))
          {
            Issue(u, x, y);
            OrdChange++;
          }
          else if (_hasStage[AiTheatres.S(pid)] && IsIdleAway(u, _stageX[AiTheatres.S(pid)], _stageY[AiTheatres.S(pid)], AtStage))
          {
            // 0.44.3: only IDLE rest units -- the busy ones keep walking (to the old staging CP or a fight) and get
            // the new one when they stand idle. Before, every target change re-sent all rest units away from the new
            // staging CP: 12,913 "rest" orders in 14 min (0.44.2 game), units walking back and forth.
            Issue(u, _stageX[AiTheatres.S(pid)], _stageY[AiTheatres.S(pid)]);
            OrdRestMove++;
            RestBy[pid]++;
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

      _armyFood[AiTheatres.S(pid)] = armyFood;
      _groupFood[AiTheatres.S(pid)] = groupFood;
      if (groupFood * 2 < _need[AiTheatres.S(pid)] && groupFood < armyFood)
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
            Issue(u, x, y);
            OrdIdleFar++;
            IdleBy[pid]++;
          }
        }
        else if (_hasStage[AiTheatres.S(pid)] && IsIdleAway(u, _stageX[AiTheatres.S(pid)], _stageY[AiTheatres.S(pid)], AtStage))
        {
          Issue(u, _stageX[AiTheatres.S(pid)], _stageY[AiTheatres.S(pid)]);
          OrdRestMove++;
          RestBy[pid]++;
        }
      }
    }

    // 0.44.3: units standing idle at a waygate exit lost their queued chain in the teleport -- give them the rest of
    // the way NOW (every service), not after the 20-s heartbeat (0.44.1 long game: 1048 found at exits).
    internal static void ExitPass(int pid)
    {
      if (pid < 0 || pid >= 24 || !_hasFinal[AiTheatres.S(pid)])
      {
        return;
      }

      var grp = _group[AiTheatres.S(pid)];
      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        if (GetUnitCurrentOrder(u) != 0 || !AiRoutes.NearWaygateExit(GetUnitX(u), GetUnitY(u)))
        {
          continue;
        }

        if (grp != null && grp.Contains(u) || _whole[AiTheatres.S(pid)])
        {
          Issue(u, _fx[AiTheatres.S(pid)], _fy[AiTheatres.S(pid)]);
        }
        else if (_hasStage[AiTheatres.S(pid)])
        {
          Issue(u, _stageX[AiTheatres.S(pid)], _stageY[AiTheatres.S(pid)]);
        }
        else
        {
          continue;
        }

        AiRoutes.ExitIdle++;
        OrdExit++;
      }
    }

    // 0.42: centre of the attack group (route start); unchanged when the bot has no group (whole army / none).
    // 0.42.1: centre of the group's biggest part on ONE landmass (like the army anchor, 0.36) -- a group half through
    // a waygate had its centre between the two worlds and was sent back and forth (Dark Portal).
    private static int[] _lmCount = new int[64];
    // 0.47: ONLY FORWARD in defence -- a CP more than DefendFar behind the attack group is defended by the units NOT in
    // the group (the rest at the staging point, new units); the group keeps its front. Before, every defence pulled
    // the whole army back (Druids: Broken Isles -> Ashenvale; North Alliance between its own CPs).
    private const float DefendFar = 6000f;
    internal static int DefendByRest;

    private static bool GroupFarFrom(int pid, float x, float y)
    {
      var s = AiTheatres.S(pid);
      var grp = pid >= 0 && pid < 24 ? _group[s] : null;
      if (grp == null || grp.Count == 0 || _whole[s])
      {
        return false;
      }

      float cx = x, cy = y;
      GroupCenter(pid, ref cx, ref cy);
      return (cx - x) * (cx - x) + (cy - y) * (cy - y) > DefendFar * DefendFar;
    }

    private static void DefendWithRest(int pid, float x, float y, bool changed)
    {
      var grp = _group[AiTheatres.S(pid)];
      DefendByRest++;
      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        if (grp.Contains(u))
        {
          continue;
        }

        if (changed)
        {
          Issue(u, x, y);
          OrdChange++;
        }
        else if (IsIdleAway(u, x, y, AtTarget))
        {
          Issue(u, x, y);
          OrdIdleFar++;
          IdleBy[pid]++;
        }
      }
    }

    internal static void GroupCenter(int pid, ref float x, ref float y)
    {
      var grp = pid >= 0 && pid < 24 ? _group[AiTheatres.S(pid)] : null;
      if (grp == null || grp.Count == 0 || _whole[AiTheatres.S(pid)])
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
        _need[AiTheatres.S(pid)] = Need(p, x, y);
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

      _armyFood[AiTheatres.S(pid)] = armyFood;

      // 0.46: only units that have a way to the target (walking or a portal edge), unless none has
      var reach = new bool[n];
      var reachN = 0;
      for (var i = 0; i < n; i++)
      {
        reach[i] = AiRoutes.Reachable(p, GetUnitX(Units[i]), GetUnitY(Units[i]), x, y);
        if (reach[i])
        {
          reachN++;
        }
      }

      if (reachN > 0 && reachN < n)
      {
        for (var i = 0; i < n; i++)
        {
          if (!reach[i])
          {
            taken[i] = true;
            Unreachable++;
          }
        }
      }

      if (!AiTheatres.Secondary && AiRegroup.Holding(p, pid, n)) // 0.47: regroup is the main theatre's
      {
        grp.Clear(); // 0.46.2: regroup -- nobody attacks, every unit rests at the staging CP
        _groupFood[AiTheatres.S(pid)] = 0;
        return;
      }

      var all = _need[AiTheatres.S(pid)] * 5 >= armyFood * 4; // need >= 80 % of the army -> everybody goes
      var food = 0;
      while (food < _need[AiTheatres.S(pid)] || all)
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

      _groupFood[AiTheatres.S(pid)] = food;
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
        else if (oid == 24)
        {
          var f = Food(u);
          var lv = GetUnitLevel(u);
          food += lv > f ? lv : f; // 0.44.3: creeps / bosses often use 0 food -- their level counts
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
      var had = _hasStage[AiTheatres.S(pid)];
      var oldX = _stageX[AiTheatres.S(pid)];
      var oldY = _stageY[AiTheatres.S(pid)];
      var oldName = _stageName[AiTheatres.S(pid)];

      // 0.46.2: on the route (AiRoutes.StagePoint), and kept when the new one is within StageKeep of the old --
      // the rest of the army stops walking between own CPs at every target change
      if (AiRoutes.StagePoint(p, pid, out var rx, out var ry))
      {
        if (had && (rx - oldX) * (rx - oldX) + (ry - oldY) * (ry - oldY) < StageKeep * StageKeep)
        {
          StageKept++;
          return;
        }

        _hasStage[AiTheatres.S(pid)] = true;
        _stageX[AiTheatres.S(pid)] = rx;
        _stageY[AiTheatres.S(pid)] = ry;
        _stageName[AiTheatres.S(pid)] = "route node";
        StageRoute++;
        return;
      }

      _hasStage[AiTheatres.S(pid)] = false;
      _stageName[AiTheatres.S(pid)] = "-";
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
          _hasStage[AiTheatres.S(pid)] = true;
          _stageX[AiTheatres.S(pid)] = cx;
          _stageY[AiTheatres.S(pid)] = cy;
          _stageName[AiTheatres.S(pid)] = GetUnitName(cu);
        }
      }

      if (had && _hasStage[AiTheatres.S(pid)]
          && (_stageX[AiTheatres.S(pid)] - oldX) * (_stageX[AiTheatres.S(pid)] - oldX) + (_stageY[AiTheatres.S(pid)] - oldY) * (_stageY[AiTheatres.S(pid)] - oldY)
             < StageKeep * StageKeep)
      {
        _stageX[AiTheatres.S(pid)] = oldX; // 0.46.2: fallback (no route) -- keep the old one too when close
        _stageY[AiTheatres.S(pid)] = oldY;
        _stageName[AiTheatres.S(pid)] = oldName;
        StageKept++;
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
    private static Dictionary<unit, float>[] _sx = new Dictionary<unit, float>[AiTheatres.Agents];
    private static Dictionary<unit, float>[] _sy = new Dictionary<unit, float>[AiTheatres.Agents];
    private static Dictionary<unit, float>[] _sl = new Dictionary<unit, float>[AiTheatres.Agents];
    internal static int StallSeen;
    internal static int StallStalled;

    private static void SampleStall(int pid)
    {
      if (_sx[AiTheatres.S(pid)] == null)
      {
        _sx[AiTheatres.S(pid)] = new Dictionary<unit, float>();
        _sy[AiTheatres.S(pid)] = new Dictionary<unit, float>();
        _sl[AiTheatres.S(pid)] = new Dictionary<unit, float>();
      }

      var sx = _sx[AiTheatres.S(pid)];
      var sy = _sy[AiTheatres.S(pid)];
      var sl = _sl[AiTheatres.S(pid)];
      for (var i = 0; i < Units.Count; i++)
      {
        var u = Units[i];
        float x = GetUnitX(u), y = GetUnitY(u), life = GetUnitState(u, UNIT_STATE_LIFE);
        var ordered = GetUnitCurrentOrder(u) != 0;
        if (sx.ContainsKey(u))
        {
          var dx = x - sx[u];
          var dy = y - sy[u];
          var moved = dx * dx + dy * dy >= 100f * 100f;
          var hurt = life < sl[u];
          if (ordered)
          {
            StallSeen++;
            StallSeenBy[pid]++;
            if (!moved && !hurt)
            {
              StallStalled++;
              StallBy[pid]++;
            }
          }

          // 0.46: activity breakdown (AiObserve)
          if (hurt)
          {
            AiObserve.ActHurt++;
          }
          else if (moved)
          {
            AiObserve.ActWalk++;
          }
          else if (!ordered)
          {
            AiObserve.ActIdle++;
          }
          else
          {
            AiObserve.ActStall++;
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
             + I2S(OrdAdvance) + " exit=" + I2S(OrdExit) + " | chains " + (AiRoutes.Chains ? "on" : "off") + " issued=" + I2S(AiRoutes.ChainsIssued)
             + " points=" + I2S(AiRoutes.ChainPoints) + " fallback=" + I2S(AiRoutes.ChainFallback) + " | stalled "
             + I2S(StallSeen > 0 ? StallStalled * 100 / StallSeen : 0) + "% of " + I2S(StallSeen)
             + " | staging on route " + I2S(StageRoute) + ", kept " + I2S(StageKept) // 0.46.2
             + " | defence kept away (other landmass) " + I2S(DefendKept) // 0.46.4
             + " | defended by the rest (group kept its front) " + I2S(DefendByRest); // 0.47
    }

    internal static void Lines(List<string> o)
    {
      o.Add(ShortLine());
      var s = "groups (food group/need of army @staging):";
      for (var pid = 0; pid < 24; pid++)
      {
        if (_armyFood[AiTheatres.S(pid)] == 0)
        {
          continue;
        }

        s = s + " P" + I2S(pid) + " " + (_whole[AiTheatres.S(pid)] ? "all " + I2S(_armyFood[AiTheatres.S(pid)])
          : I2S(_groupFood[AiTheatres.S(pid)]) + "/" + I2S(_need[AiTheatres.S(pid)]) + " of " + I2S(_armyFood[AiTheatres.S(pid)]) + " @"
            + (_hasStage[AiTheatres.S(pid)] ? _stageName[AiTheatres.S(pid)] : "-")) + ";";
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
