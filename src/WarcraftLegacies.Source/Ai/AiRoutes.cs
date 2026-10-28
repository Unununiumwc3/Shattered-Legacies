using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.42 (refactor phase 3): route planner on ONE generated graph (AiRouteData, tools/ai/gen_routes.py -- map data
  // only). "-route on" (default) / "-route off" (0.41: the hand-written frontier graph) for A/B tests.
  // - Edges: walking edges from the generator; closed while the LTrc rock group on them stands (checked every 15 s).
  //   Portal edges: every ACTIVE waygate of the neutral players (WL's own and the AI's portals), every 30 s.
  // - Targets: CPs not held by the bot or an ally (same rules as before: 10-min lock on enemy CPs, uncapturable,
  //   Argus Legion-only before the summon, skip nodes, stuck blacklist) and AREA nodes where an enemy structure stands
  //   (every 15 s) -- harbours, camps, bases without a CP.
  // - Target (0.43): lowest walking cost from the faction's TERRITORY (own + allied nodes) -- neutral CPs count half,
  //   a quest CP on the front 0.3, entering an enemy-held node costs extra. Route: Dijkstra from the AREA the attack
  //   group stands in (region map, 0.42.1). The bot commits to the route and follows it node by node ("planning").
  // - Waypoint: the next node on the route (the engine walks through a waygate on its own).
  internal static class AiRoutes
  {
    internal static bool On = true;
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
    private static bool[] _lastFront = new bool[24];
    private static bool[] _lastQuest = new bool[24];
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
          var a = StartNode(ux, uy); // 0.43.2: the AREA of each waygate end (was: nearest node in a straight line --
          var b = StartNode(dx, dy); // the Emerald Portal at the Thandol bridge got the node across the gorge)
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
    // 0.42.1: a bot COMMITS to a route and follows it node by node (never backwards). It plans again only when the
    // target is no longer valid, the group stands in an area that is not on the route, or every ReplanEvery seconds
    // (and then switches only to a target that costs less than half). 0.42 planned every service from the node
    // nearest in a straight line: groups ran back and forth, across the closed Thandol bridge and over mountains.

    private const float ReplanEvery = 30f;
    private const float QuestBias = 0.3f;   // a CP of an open quest objective counts this much of its cost
    private const float WaypointReach = 8000f; // 0.43.1: order to the farthest route node within this distance
    private const float DetourFactor = 1.35f;  // 0.43.2: ... reachable without a detour (route cost vs straight line)
    private const float DetourSlack = 2000f;
    private static List<int>[] _path = new List<int>[24];
    private static List<float>[] _pathCost = new List<float>[24]; // 0.43.2: walking cost from the group, per route node
    private static int[] _step = new int[24];
    private static float[] _planAt = new float[24];
    private static HashSet<int> _questNodes = new();

    // Target for bot p; (sx, sy) = where its attack group (or army) stands. Null = nothing reachable (the caller falls
    // back to the old frontier picker). Sets WpX/WpY[pid] (-1/-1 = go straight to the target).
    internal static unit Pick(player p, int pid, Faction fac, float sx, float sy, BotDiag d)
    {
      Ready();
      d.EdgeSet = "route";
      var here = StartNode(sx, sy);
      if (here < 0)
      {
        return null;
      }

      var t = _lastTarget[pid];
      var tu = t >= 0 ? Candidate(p, pid, fac, t, d) : null;
      // 0.43.1: plan again only when the target is gone or every ReplanEvery seconds. 0.42.1/0.43.0 also planned
      // whenever the group stood in an area that is not on the route -- which happens all the time while walking
      // past other nodes (~1 plan per bot every 13 s, new routes, back and forth).
      var path = _path[pid];
      if (tu == null || path == null || SimpleBot.Clock - _planAt[pid] >= ReplanEvery)
      {
        tu = Plan(p, pid, fac, here, t, tu, d);
        if (tu == null)
        {
          _lastTarget[pid] = -1;
          _path[pid] = null;
          return null;
        }

        path = _path[pid];
      }

      // 0.43.1: the order goes to the FARTHEST node on the route within WaypointReach of the group (a CP before an
      // area node), never to one behind the last. The point moves ahead while the units still walk -- 0.42.1/0.43.0
      // ordered to the next node, units stopped there and waited until the group's centre came within 1200
      // ("stop go stop go", ~950 waypoint orders/min).
      // 0.43.2: ... and only a node the units can walk to WITHOUT a big detour: its route cost from where the group
      // is may be at most DetourFactor x the straight line + DetourSlack. 0.43.1 chose Arathi Highlands from the other
      // side of the closed Thandol bridge (close in a line, far on foot): the units stood at the bridge.
      var last = path.Count - 1;
      var costs = _pathCost[pid];
      var h = path.IndexOf(here);
      var baseIdx = h > _step[pid] - 1 ? h : _step[pid] - 1;
      var baseCost = costs != null && baseIdx >= 0 && baseIdx < costs.Count ? costs[baseIdx] : 0f;
      var pick = -1;
      var pickCp = -1;
      for (var k = last; k >= _step[pid] && k >= 0; k--)
      {
        if (!Near(path[k], sx, sy, WaypointReach))
        {
          continue;
        }

        if (costs != null && k < costs.Count && k > _step[pid]
            && costs[k] - baseCost > DetourFactor * Dist(AiRouteData.NodeX[path[k]], AiRouteData.NodeY[path[k]], sx, sy)
               + DetourSlack)
        {
          continue;
        }

        if (pick < 0)
        {
          pick = k;
        }

        if (AiRouteData.NodeType[path[k]] != 0)
        {
          pickCp = k;
          break;
        }
      }

      if (pick != last && pickCp >= 0 && pick - pickCp <= 2)
      {
        pick = pickCp; // a CP is a better gathering point than an area node, if it is at most 2 nodes back
      }

      if (pick > _step[pid])
      {
        _step[pid] = pick;
      }

      if (_step[pid] > last)
      {
        _step[pid] = last;
      }

      var wp = path[_step[pid]];
      _lastWp[pid] = wp;
      _lastHops[pid] = last - _step[pid];
      if (wp == path[path.Count - 1])
      {
        WpX[pid] = -1f;
        WpY[pid] = -1f;
      }
      else
      {
        WpX[pid] = AiRouteData.NodeX[wp];
        WpY[pid] = AiRouteData.NodeY[wp];
      }

      return tu;
    }

    // 0.43: the TARGET is chosen by distance from the faction's TERRITORY (Dijkstra from all own and allied nodes at
    // once, plus area nodes where its own buildings stand) -- natural expansion: neutral CPs in the own area first,
    // then the front. 0.42 measured from the attack group: once a group had marched off, the neutral CPs at home were
    // "far" and the enemy ahead "near" (Druids). The ROUTE is still planned from where the group stands.
    private static float[] _score;
    private static bool[] _source;
    private static bool[] _front;

    private static unit Plan(player p, int pid, Faction fac, int here, int cur, unit curUnit, BotDiag d)
    {
      QuestNodes(fac);
      if (_score == null)
      {
        _score = new float[AiRouteData.Nodes];
        _source = new bool[AiRouteData.Nodes];
        _front = new bool[AiRouteData.Nodes];
      }

      // 1) distance from the territory: score every candidate
      var sources = 0;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _source[i] = IsOwnNode(p, pid, i);
        if (_source[i])
        {
          sources++;
        }
      }

      if (sources == 0)
      {
        _source[here] = true; // no territory left: from where the group stands
      }

      Dijkstra(p, -1);
      unit[] cands = null;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _score[i] = -1f;
        _front[i] = false;
        if (!_done[i] || _source[i])
        {
          continue;
        }

        var cand = i == cur ? curUnit : Candidate(p, pid, fac, i, d);
        if (cand == null)
        {
          continue;
        }

        if (cands == null)
        {
          cands = new unit[AiRouteData.Nodes];
        }

        cands[i] = cand;
        _front[i] = _prev[i] >= 0 && _source[_prev[i]];
        _score[i] = Score(i, cand);
      }

      if (cands == null)
      {
        return null;
      }

      // 2) the route from where the group stands: the best-scored candidate it can reach
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _source[i] = false;
      }

      _source[here] = true;
      Dijkstra(p, -1);
      var best = -1;
      var curOk = false;
      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        if (_score[i] < 0f || !_done[i])
        {
          continue;
        }

        if (i == cur)
        {
          curOk = true;
        }

        if (best < 0 || _score[i] < _score[best])
        {
          best = i;
        }
      }

      if (best < 0)
      {
        return null;
      }

      if (curOk && best != cur && _score[best] >= _score[cur] * SimpleBot.StickySwitch)
      {
        d.Sticky++;
        best = cur;
      }

      var path = new List<int>();
      var n = best;
      while (n >= 0 && path.Count <= AiRouteData.Nodes)
      {
        path.Add(n);
        if (n == here)
        {
          break;
        }

        n = _prev[n];
      }

      path.Reverse();
      var costs = new List<float>();
      for (var k = 0; k < path.Count; k++)
      {
        costs.Add(_dist[path[k]]);
      }

      _path[pid] = path;
      _pathCost[pid] = costs;
      _step[pid] = path.Count > 1 ? 1 : 0;
      _planAt[pid] = SimpleBot.Clock;
      _lastTarget[pid] = best;
      _lastCost[pid] = _score[best];
      _lastFront[pid] = _front[best];
      _lastQuest[pid] = _questNodes.Contains(best);
      return cands[best];
    }

    // own territory: CPs held by p, area nodes where p has a structure. 0.43.1: NOT the allies' -- with them every bot
    // of a team computed the same front (the team's cheapest) and the whole team marched there as one ball (Druids,
    // Ahn'Qiraj and the Sentinels from Kalimdor to Arathi Highlands, 9 hops).
    private static bool IsOwnNode(player p, int pid, int i)
    {
      var t = AiRouteData.NodeType[i];
      if (t != 0)
      {
        var cp = SimpleBot.SafeCp(t);
        return cp != null && cp.Unit != null && GetOwningPlayer(cp.Unit) == p;
      }

      var s = _areaUnit[i * 24 + pid];
      return s != null && GetUnitState(s, UNIT_STATE_LIFE) > 0.405f;
    }

    private static float Score(int i, unit cand)
    {
      var s = _dist[i];
      if (IsNeutral(cand))
      {
        s *= SimpleBot.NeutralBias;
      }

      if (_questNodes.Contains(i) && _front[i])
      {
        s *= QuestBias; // 0.43: only on the front -- a far quest CP waits until the territory reaches it
      }

      return s;
    }

    // CP nodes where an open objective of the faction's open quests lies (Objective.Position; any objective type)
    private static void QuestNodes(Faction fac)
    {
      _questNodes.Clear();
      var quests = fac.GetAllQuests();
      for (var q = 0; q < quests.Count; q++)
      {
        var qd = quests[q];
        if (qd.Progress != QuestProgress.Incomplete && qd.Progress != QuestProgress.Undiscovered)
        {
          continue;
        }

        var objs = qd.Objectives;
        for (var k = 0; k < objs.Count; k++)
        {
          var pos = objs[k].Position;
          if (objs[k].Progress == QuestProgress.Complete || pos == null)
          {
            continue;
          }

          var node = Region(pos.X, pos.Y);
          if (node >= 0 && AiRouteData.NodeType[node] != 0 && Near(node, pos.X, pos.Y, 800f))
          {
            _questNodes.Add(node);
          }
        }
      }
    }

    private static bool Near(int node, float x, float y, float r)
    {
      var dx = AiRouteData.NodeX[node] - x;
      var dy = AiRouteData.NodeY[node] - y;
      return dx * dx + dy * dy <= r * r;
    }

    // the area (node) the point stands in; the straight-line nearest node on the same landmass where the region map
    // has no walkable ground
    private static int StartNode(float x, float y)
    {
      var r = Region(x, y);
      return r >= 0 ? r : NearestSameLand(x, y);
    }

    internal static int Region(float x, float y)
    {
      var col = (int)((x - AiRouteData.RegionMinX) / AiRouteData.RegionCell);
      var row = (int)((y - AiRouteData.RegionMinY) / AiRouteData.RegionCell);
      if (col < 0 || col >= AiRouteData.RegionW || row < 0 || row >= AiRouteData.RegionH)
      {
        return -1;
      }

      var i = (row * AiRouteData.RegionW + col) * 2;
      var a = AiRouteData.Regions[i];
      if (a == '.')
      {
        return -1;
      }

      var v = Letter(a) * 52 + Letter(AiRouteData.Regions[i + 1]);
      return v < AiRouteData.Nodes ? v : -1;
    }

    private static int Letter(char c) => c <= 'Z' ? c - 'A' : c - 'a' + 26;

    // 0.42.1: can a unit at (ux, uy) walk to (x, y) without a closed edge -- same area, or neighbouring areas joined
    // by an open walking edge? Unknown ground counts as reachable (the old behaviour). Used by the defence scan: enemies
    // across the closed Thandol bridge no longer pull an army to the bank.
    internal static bool Reachable(float x, float y, float ux, float uy)
    {
      if (!On)
      {
        return true;
      }

      Ready();
      var a = Region(x, y);
      var b = Region(ux, uy);
      if (a < 0 || b < 0 || a == b)
      {
        return true;
      }

      var adj = _adj[a];
      for (var k = 0; k < adj.Count; k++)
      {
        var e = adj[k];
        if ((AiRouteData.EdgeA[e] == b || AiRouteData.EdgeB[e] == b)
            && (AiRouteData.EdgeRocks[e] < 0 || _rockOpen[AiRouteData.EdgeRocks[e]]))
        {
          return true;
        }
      }

      return false;
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

    // an enemy structure that can be attacked (alive, visible, not invulnerable) near area node i
    private static unit EnemyStructure(player p, int i)
    {
      for (var k = 0; k < 24; k++)
      {
        var s = _areaUnit[i * 24 + k];
        if (s != null && IsPlayerEnemy(Player(k), p) && GetUnitState(s, UNIT_STATE_LIFE) > 0.405f
            && GetOwningPlayer(s) == Player(k) && !IsUnitHidden(s) && !BlzIsUnitInvulnerable(s))
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

    // start < 0: from every node marked in _source (multi-source)
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
      if (start >= 0)
      {
        _dist[start] = 0f;
        Push(start, 0f);
      }
      else
      {
        for (var i = 0; i < AiRouteData.Nodes; i++)
        {
          if (_source[i])
          {
            _dist[i] = 0f;
            Push(i, 0f);
          }
        }
      }
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

    internal static bool HasWaypoint(int pid) => WpX[pid] != -1f || WpY[pid] != -1f;

    // ---- helpers ------------------------------------------------------------------------------------------

    private static float Dist(float ax, float ay, float bx, float by)
    {
      var dx = ax - bx;
      var dy = ay - by;
      return SquareRoot(dx * dx + dy * dy);
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

        o.Add("P" + I2S(pid) + " -> " + NodeName(_lastTarget[pid]) + " score " + I2S(R2I(_lastCost[pid]))
              + (_lastFront[pid] ? " front" : " inside") + (_lastQuest[pid] ? " quest" : "") + ", "
              + I2S(_lastHops[pid]) + " hops left, next " + NodeName(_lastWp[pid]));
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
