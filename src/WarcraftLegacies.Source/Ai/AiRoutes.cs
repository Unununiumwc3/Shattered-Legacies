using System.Collections.Generic;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.Ai
{
  // 0.42 (refactor phase 3): route planner on ONE generated graph (AiRouteData, tools/ai/gen_routes.py -- map data
  // only). "-route on" (default) / "-route off" (0.41: the hand-written frontier graph) for A/B tests.
  // - Edges: walking edges from the generator; closed while the LTrc rock group on them stands (checked every 15 s).
  //   Portal edges: every ACTIVE waygate of the neutral players (WL's own and the AI's portals), every 30 s.
  // - Targets: CPs not held by the bot or an ally (same rules as before: 10-min lock on enemy CPs, uncapturable,
  //   Argus Legion-only before the summon, skip nodes, stuck blacklist) and AREA nodes where an enemy structure stands
  //   (every 15 s) -- harbours, camps, bases without a CP.
  // - Dijkstra from the node nearest to the attack group (or the army) over open edges; target = lowest walking cost
  //   (neutral CPs count half, entering an enemy-held node on the way costs extra); keep the current target unless
  //   the new one costs less than half.
  // - Waypoint: the first node on the route more than 2000 away from the group (or the node behind a waygate -- the
  //   engine walks through it) instead of one cross-map order.
  internal static class AiRoutes
  {
    internal static bool On = true;
    private const float WaypointMin = 2000f;
    private const float EnemyNodePenalty = 2000f;
    private const float AreaRadius = 2000f;
    private const float PortalCost = 300f;

    private static bool _ready;
    private static List<int>[] _adj;      // node -> walking edge indices
    private static List<int>[] _padj;     // node -> portal edge indices (one-way)
    private static List<int> _pFrom = new();
    private static List<int> _pTo = new();
    private static List<float> _pCost = new();
    private static bool[] _rockOpen;
    private static unit[] _areaUnit;      // node * 24 + owner -> one alive structure of that owner near an area node
    private static float[] _dist;
    private static int[] _prev;
    private static int[] _prevEdge;       // >= 0 walking edge, < 0 portal edge -(i + 1)
    private static bool[] _done;
    private static int[] _heapNode;
    private static float[] _heapKey;
    private static int _heapN;
    private static rect _rockRect;
    private static int _rockCount;

    // last result per bot (diagnostics + orders)
    internal static float[] WpX = new float[24];
    internal static float[] WpY = new float[24];
    private static int[] _lastTarget = new int[24];
    private static int[] _lastWp = new int[24];
    private static int[] _lastHops = new int[24];
    private static float[] _lastCost = new float[24];
    internal static int Runs;
    internal static int LastRelax;

    private static void Ready()
    {
      if (_ready)
      {
        return;
      }

      _ready = true;
      var n = AiRouteData.Nodes;
      _adj = new List<int>[n];
      _padj = new List<int>[n];
      for (var i = 0; i < n; i++)
      {
        _adj[i] = new List<int>();
        _padj[i] = new List<int>();
      }

      for (var e = 0; e < AiRouteData.Edges; e++)
      {
        _adj[AiRouteData.EdgeA[e]].Add(e);
        _adj[AiRouteData.EdgeB[e]].Add(e);
      }

      _rockOpen = new bool[AiRouteData.RockX.Length + 1]; // false = rocks stand (closed) until the first check
      _areaUnit = new unit[n * 24];
      _dist = new float[n];
      _prev = new int[n];
      _prevEdge = new int[n];
      _done = new bool[n];
      _heapNode = new int[AiRouteData.Edges * 2 + 512];
      _heapKey = new float[AiRouteData.Edges * 2 + 512];
      for (var i = 0; i < 24; i++)
      {
        _lastTarget[i] = -1;
        _lastWp[i] = -1;
      }
    }

    internal static void Setup()
    {
      AiScheduler.Add("route rocks", 60, 30, CheckRocks, true);   // every 15 s
      AiScheduler.Add("route areas", 60, 34, CheckAreas, true);   // every 15 s
      AiScheduler.Add("route portals", 120, 19, CheckPortals, true); // every 30 s
    }

    // ---- live edge state ----------------------------------------------------------------------------------

    private static void CheckRocks()
    {
      Ready();
      for (var gidx = 0; gidx < AiRouteData.RockX.Length; gidx++)
      {
        float x = AiRouteData.RockX[gidx], y = AiRouteData.RockY[gidx], r = AiRouteData.RockR[gidx];
        if (_rockRect == null)
        {
          _rockRect = Rect(x - r, y - r, x + r, y + r);
        }
        else
        {
          SetRect(_rockRect, x - r, y - r, x + r, y + r);
        }

        _rockCount = 0;
        EnumDestructablesInRect(_rockRect, null, CountRock);
        _rockOpen[gidx] = _rockCount == 0;
        SimpleBot.WaveOps++;
      }
    }

    private static void CountRock()
    {
      var d = GetEnumDestructable();
      if (GetDestructableTypeId(d) == 1280602723 && GetDestructableLife(d) > 0f) // 'LTrc'
      {
        _rockCount++;
      }
    }

    private static void CheckAreas()
    {
      Ready();
      var g = CreateGroup();
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        if (AiRouteData.NodeType[i] != 0)
        {
          continue;
        }

        for (var k = 0; k < 24; k++)
        {
          _areaUnit[i * 24 + k] = null;
        }

        GroupEnumUnitsInRange(g, AiRouteData.NodeX[i], AiRouteData.NodeY[i], AreaRadius, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          var oid = GetPlayerId(GetOwningPlayer(u));
          if (oid < 24 && _areaUnit[i * 24 + oid] == null && IsUnitType(u, UNIT_TYPE_STRUCTURE)
              && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
          {
            _areaUnit[i * 24 + oid] = u;
          }
        }
      }

      DestroyGroup(g);
    }

    private static void CheckPortals()
    {
      Ready();
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _padj[i].Clear();
      }

      _pFrom.Clear();
      _pTo.Clear();
      _pCost.Clear();
      var g = CreateGroup();
      for (var pl = 24; pl < 28; pl++)
      {
        GroupEnumUnitsOfPlayer(g, Player(pl), null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (!WaygateIsActive(u) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }

          float ux = GetUnitX(u), uy = GetUnitY(u);
          float dx = WaygateGetDestinationX(u), dy = WaygateGetDestinationY(u);
          var a = Nearest(ux, uy);
          var b = Nearest(dx, dy);
          if (a < 0 || b < 0 || a == b)
          {
            continue;
          }

          var cost = Dist(AiRouteData.NodeX[a], AiRouteData.NodeY[a], ux, uy) + PortalCost
                     + Dist(dx, dy, AiRouteData.NodeX[b], AiRouteData.NodeY[b]);
          _padj[a].Add(_pFrom.Count);
          _pFrom.Add(a);
          _pTo.Add(b);
          _pCost.Add(cost);
        }
      }

      DestroyGroup(g);
    }

    // ---- planning -----------------------------------------------------------------------------------------

    // Target for bot p from (sx, sy) -- the attack group's centre (or the army's). Null = nothing reachable (the
    // caller falls back to the old frontier picker). Sets WpX/WpY[pid].
    internal static unit Pick(player p, int pid, Faction fac, float sx, float sy, BotDiag d)
    {
      Ready();
      d.EdgeSet = "route";
      var start = NearestSameLand(sx, sy);
      if (start < 0)
      {
        return null;
      }

      Dijkstra(p, start);
      var current = SimpleBot.Committed.ContainsKey(pid) ? SimpleBot.Committed[pid] : null;
      var best = -1;
      unit bestUnit = null;
      var bestScore = 0f;
      var curScore = -1f;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        if (!_done[i])
        {
          continue;
        }

        var tu = Candidate(p, pid, fac, i, d);
        if (tu == null)
        {
          continue;
        }

        var score = _dist[i] * (IsNeutral(tu) ? SimpleBot.NeutralBias : 1f);
        if (tu == current)
        {
          curScore = score;
        }

        if (best < 0 || score < bestScore)
        {
          best = i;
          bestUnit = tu;
          bestScore = score;
        }
      }

      if (best < 0)
      {
        return null;
      }

      if (curScore >= 0f && bestUnit != current && bestScore >= curScore * SimpleBot.StickySwitch)
      {
        d.Sticky++;
        for (var i = 0; i < AiRouteData.Nodes; i++)
        {
          if (_done[i] && NodeUnitFor(p, i) == current)
          {
            best = i;
            bestUnit = current;
            break;
          }
        }
      }

      SetWaypoint(pid, best, sx, sy, start);
      _lastTarget[pid] = best;
      _lastCost[pid] = _dist[best];
      return bestUnit;
    }

    private static unit NodeUnitFor(player p, int i)
    {
      var t = AiRouteData.NodeType[i];
      if (t != 0)
      {
        var cp = SimpleBot.SafeCp(t);
        return cp != null ? cp.Unit : null;
      }

      return EnemyStructure(p, i);
    }

    // the unit to attack at node i, or null when node i is no target for p
    private static unit Candidate(player p, int pid, Faction fac, int i, BotDiag d)
    {
      var t = AiRouteData.NodeType[i];
      if (t == 0)
      {
        var s = EnemyStructure(p, i);
        if (s == null)
        {
          return null;
        }

        if (SimpleBot.Clock < 600f)
        {
          d.Locked++;
          return null;
        }

        if (SimpleBot.IsBlacklisted(pid, s))
        {
          d.Stuck++;
          return null;
        }

        d.Enemy++;
        return s;
      }

      var cp = SimpleBot.SafeCp(t);
      if (cp == null || cp.Unit == null)
      {
        d.NoCp++;
        return null;
      }

      var cu = cp.Unit;
      if (GetOwningPlayer(cu) == p || IsUnitAlly(cu, p))
      {
        return null;
      }

      if (fac.Name != "Legion" && !SimpleBot.GateOpen("legion") && AiGraph.ArgusNodes.Contains(t))
      {
        d.Skipped++;
        return null;
      }

      if (AiGraph.SkipNodes.Contains(t))
      {
        d.Skipped++;
        return null;
      }

      if (SimpleBot.IsUncapturable(cu))
      {
        d.Invuln++;
        return null;
      }

      var enemy = SimpleBot.IsEnemyCp(cu, p);
      if (SimpleBot.Clock < 600f && enemy)
      {
        d.Locked++;
        return null;
      }

      if (SimpleBot.IsBlacklisted(pid, cu))
      {
        d.Stuck++;
        return null;
      }

      if (enemy)
      {
        d.Enemy++;
      }
      else
      {
        d.Neutral++;
      }

      return cu;
    }

    private static bool IsNeutral(unit tu)
    {
      return GetPlayerId(GetOwningPlayer(tu)) >= 24;
    }

    private static unit EnemyStructure(player p, int i)
    {
      for (var k = 0; k < 24; k++)
      {
        var s = _areaUnit[i * 24 + k];
        if (s != null && IsPlayerEnemy(Player(k), p) && GetUnitState(s, UNIT_STATE_LIFE) > 0.405f
            && GetOwningPlayer(s) == Player(k))
        {
          return s;
        }
      }

      return null;
    }

    // a node held by an enemy of p costs extra to walk through (fights on the way)
    private static float EnterCost(player p, int i)
    {
      var t = AiRouteData.NodeType[i];
      if (t == 0)
      {
        return EnemyStructure(p, i) != null ? EnemyNodePenalty : 0f;
      }

      var cp = SimpleBot.SafeCp(t);
      return cp != null && cp.Unit != null && SimpleBot.IsEnemyCp(cp.Unit, p) ? EnemyNodePenalty : 0f;
    }

    private static void Dijkstra(player p, int start)
    {
      Runs++;
      var relax = 0;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _dist[i] = -1f;
        _prev[i] = -1;
        _prevEdge[i] = 0;
        _done[i] = false;
      }

      _heapN = 0;
      _dist[start] = 0f;
      Push(start, 0f);
      while (_heapN > 0)
      {
        var u = Pop();
        if (_done[u])
        {
          continue;
        }

        _done[u] = true;
        var du = _dist[u];
        var adj = _adj[u];
        for (var k = 0; k < adj.Count; k++)
        {
          var e = adj[k];
          var rock = AiRouteData.EdgeRocks[e];
          if (rock >= 0 && !_rockOpen[rock])
          {
            continue;
          }

          var v = AiRouteData.EdgeA[e] == u ? AiRouteData.EdgeB[e] : AiRouteData.EdgeA[e];
          relax++;
          Relax(p, u, v, du + AiRouteData.EdgeCost[e], e);
        }

        var padj = _padj[u];
        for (var k = 0; k < padj.Count; k++)
        {
          var pe = padj[k];
          relax++;
          Relax(p, u, _pTo[pe], du + _pCost[pe], -(pe + 1));
        }
      }

      LastRelax = relax;
      SimpleBot.WaveOps += relax;
    }

    private static void Relax(player p, int u, int v, float nd, int edge)
    {
      if (_done[v])
      {
        return;
      }

      nd += EnterCost(p, v);
      if (_dist[v] < 0f || nd < _dist[v])
      {
        _dist[v] = nd;
        _prev[v] = u;
        _prevEdge[v] = edge;
        Push(v, nd);
      }
    }

    private static void SetWaypoint(int pid, int target, float sx, float sy, int start)
    {
      // path target -> start, then walk it from the start side
      var path = new List<int>();
      var n = target;
      while (n >= 0 && path.Count <= AiRouteData.Nodes)
      {
        path.Add(n);
        if (n == start)
        {
          break;
        }

        n = _prev[n];
      }

      path.Reverse();
      var wp = target;
      for (var i = 1; i < path.Count; i++)
      {
        var node = path[i];
        if (_prevEdge[node] < 0 || Dist(AiRouteData.NodeX[node], AiRouteData.NodeY[node], sx, sy) > WaypointMin)
        {
          wp = node; // first far node, or the node behind a waygate (the engine walks through the waygate)
          break;
        }
      }

      _lastWp[pid] = wp;
      _lastHops[pid] = path.Count - 1;
      if (wp == target)
      {
        WpX[pid] = -1f; // caller orders straight to the target unit
        WpY[pid] = -1f;
      }
      else
      {
        WpX[pid] = AiRouteData.NodeX[wp];
        WpY[pid] = AiRouteData.NodeY[wp];
      }
    }

    internal static bool HasWaypoint(int pid) => WpX[pid] != -1f || WpY[pid] != -1f;

    // ---- helpers ------------------------------------------------------------------------------------------

    private static float Dist(float ax, float ay, float bx, float by)
    {
      var dx = ax - bx;
      var dy = ay - by;
      return SquareRoot(dx * dx + dy * dy);
    }

    private static int Nearest(float x, float y)
    {
      var best = -1;
      var bd = 0f;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        var dx = AiRouteData.NodeX[i] - x;
        var dy = AiRouteData.NodeY[i] - y;
        var dd = dx * dx + dy * dy;
        if (best < 0 || dd < bd)
        {
          best = i;
          bd = dd;
        }
      }

      return best;
    }

    // nearest node on the same landmass (Geography grid) when the point's landmass is known
    private static int NearestSameLand(float x, float y)
    {
      var lm = Geography.Landmass(x, y);
      var best = -1;
      var bd = 0f;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        var dx = AiRouteData.NodeX[i] - x;
        var dy = AiRouteData.NodeY[i] - y;
        var dd = dx * dx + dy * dy;
        if (best >= 0 && dd >= bd)
        {
          continue;
        }

        if (lm >= 0)
        {
          var nl = Geography.Landmass(AiRouteData.NodeX[i], AiRouteData.NodeY[i]);
          if (nl >= 0 && nl != lm)
          {
            continue;
          }
        }

        best = i;
        bd = dd;
      }

      return best;
    }

    private static void Push(int node, float key)
    {
      if (_heapN >= _heapNode.Length)
      {
        return; // cannot happen with the sized arrays; never grow inside the loop
      }

      var i = _heapN++;
      _heapNode[i] = node;
      _heapKey[i] = key;
      while (i > 0)
      {
        var parent = (i - 1) / 2;
        if (_heapKey[parent] <= _heapKey[i])
        {
          break;
        }

        Swap(i, parent);
        i = parent;
      }
    }

    private static int Pop()
    {
      var top = _heapNode[0];
      _heapN--;
      _heapNode[0] = _heapNode[_heapN];
      _heapKey[0] = _heapKey[_heapN];
      var i = 0;
      while (true)
      {
        var l = 2 * i + 1;
        var r = l + 1;
        var m = i;
        if (l < _heapN && _heapKey[l] < _heapKey[m])
        {
          m = l;
        }

        if (r < _heapN && _heapKey[r] < _heapKey[m])
        {
          m = r;
        }

        if (m == i)
        {
          break;
        }

        Swap(i, m);
        i = m;
      }

      return top;
    }

    private static void Swap(int a, int b)
    {
      var n = _heapNode[a];
      var k = _heapKey[a];
      _heapNode[a] = _heapNode[b];
      _heapKey[a] = _heapKey[b];
      _heapNode[b] = n;
      _heapKey[b] = k;
    }

    // ---- readouts -----------------------------------------------------------------------------------------

    private static string NodeName(int i)
    {
      if (i < 0)
      {
        return "-";
      }

      var t = AiRouteData.NodeType[i];
      if (t != 0)
      {
        var cp = SimpleBot.SafeCp(t);
        if (cp != null && cp.Unit != null)
        {
          return GetUnitName(cp.Unit);
        }
      }

      return "area #" + I2S(i);
    }

    internal static void Lines(List<string> o)
    {
      Ready();
      var closed = 0;
      for (var gidx = 0; gidx < AiRouteData.RockX.Length; gidx++)
      {
        if (!_rockOpen[gidx])
        {
          closed++;
        }
      }

      var areas = 0;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        if (AiRouteData.NodeType[i] != 0)
        {
          continue;
        }

        for (var k = 0; k < 24; k++)
        {
          if (_areaUnit[i * 24 + k] != null)
          {
            areas++;
            break;
          }
        }
      }

      o.Add("## ROUTES (0.42: generated graph, -route " + (On ? "on" : "off") + ")  nodes " + I2S(AiRouteData.Nodes)
            + ", edges " + I2S(AiRouteData.Edges) + " + " + I2S(_pFrom.Count) + " waygate edges; rock groups closed "
            + I2S(closed) + "/" + I2S(AiRouteData.RockX.Length) + "; area nodes with structures " + I2S(areas)
            + "; plans " + I2S(Runs) + ", last relaxations " + I2S(LastRelax));
      for (var pid = 0; pid < 24; pid++)
      {
        if (_lastTarget[pid] < 0)
        {
          continue;
        }

        o.Add("P" + I2S(pid) + " -> " + NodeName(_lastTarget[pid]) + " cost " + I2S(R2I(_lastCost[pid])) + ", "
              + I2S(_lastHops[pid]) + " hops, next " + NodeName(_lastWp[pid]));
      }
    }

    // "-route on" / "-route off"
    internal static void OnRouteCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg.Length >= 9 && msg.Substring(0, 9) == "-route on")
      {
        On = true;
      }
      else if (msg.Length >= 10 && msg.Substring(0, 10) == "-route off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "targeting: "
        + (On ? "ROUTE PLANNER (generated graph, waypoints)" : "OLD frontier graph (0.41)") + "  -- -route on|off");
    }
  }
}
