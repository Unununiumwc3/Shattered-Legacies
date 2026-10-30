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
    private const float PortalCost = 300f;

    private static bool _ready;
    private static List<int>[] _adj;      // node -> walking edge indices
    private static List<int>[] _padj;     // node -> portal edge indices (one-way)
    private static List<int> _pFrom = new();
    private static List<int> _pTo = new();
    private static List<float> _pCost = new();
    private static List<unit> _pUnit = new();  // 0.44: the waygate of each portal edge (chains click it)
    private static List<float> _pExitX = new();
    private static List<float> _pExitY = new();
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
      // 0.44.1: enemy structures per node AREA (region map), 6 players per task, spread over the cycle
      AiScheduler.Add("route structures 1", 60, 34, () => CheckStructures(0), true);
      AiScheduler.Add("route structures 2", 60, 38, () => CheckStructures(6), true);
      AiScheduler.Add("route structures 3", 60, 42, () => CheckStructures(12), true);
      AiScheduler.Add("route structures 4", 60, 46, () => CheckStructures(18), true);
      AiScheduler.Add("route creeps", 60, 50, CheckCreeps, true); // 0.44.3: Neutral Hostile per node area
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

    // 0.44.1: for players first .. first+5: one alive structure per node AREA and owner (region map). Before (0.42):
    // only within 2000 of the 62 area nodes -- buildings elsewhere were never targets (Stratholme's city survived
    // next to its captured CP).
    private static void CheckStructures(int first)
    {
      Ready();
      var g = CreateGroup();
      for (var k = first; k < first + 6 && k < 24; k++)
      {
        for (var i = 0; i < AiRouteData.Nodes; i++)
        {
          _areaUnit[i * 24 + k] = null;
        }

        GroupEnumUnitsOfPlayer(g, Player(k), null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (!IsUnitType(u, UNIT_TYPE_STRUCTURE) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }

          var node = Region(GetUnitX(u), GetUnitY(u));
          if (node >= 0 && _areaUnit[node * 24 + k] == null)
          {
            _areaUnit[node * 24 + k] = u;
          }
        }
      }

      DestroyGroup(g);
    }

    // ---- 0.44.3: creep targets ---------------------------------------------------------------------------------
    // Neutral Hostile (slot 24) units per node AREA (region map), like the structures: one creep to attack and the
    // camp's strength (sum of levels -- bosses often use 0 food). A camp is a target for a bot that holds the CP the
    // area belongs to (an area node's patch: the nearest CP on foot), when the bot's army is strong enough
    // (CreepFoodPerLevel x levels), at any time; invulnerable / hidden creeps are skipped (a boss that becomes
    // attackable through a quest appears by itself). Lower priority than CPs (+CreepCost). Replaces the hand-placed
    // one-shots (AiConfig.OneShots, skipped while Creeps is on). "-creeps off" = 0.44.2.
    internal static bool Creeps = true;
    private const float CreepCost = 3000f;
    private const float CreepFoodPerLevel = 1.5f;
    private static unit[] _creep;
    private static int[] _creepLevel;
    private static int[] _ownerCp;    // node -> the CP node its area belongs to (itself for CP nodes), -1 = none
    internal static int CreepCamps;

    private static void CheckCreeps()
    {
      Ready();
      var n = AiRouteData.Nodes;
      if (_creep == null)
      {
        _creep = new unit[n];
        _creepLevel = new int[n];
        OwnerCps();
      }

      for (var i = 0; i < n; i++)
      {
        _creep[i] = null;
        _creepLevel[i] = 0;
      }

      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Player(24), null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.WaveOps++;
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || IsUnitHidden(u)
            || BlzIsUnitInvulnerable(u) || SimpleBot.SafeCp(GetUnitTypeId(u)) != null)
        {
          continue; // not a CP itself (neutral CPs can belong to Neutral Hostile)
        }

        var node = Region(GetUnitX(u), GetUnitY(u));
        if (node < 0)
        {
          continue;
        }

        // 0.46: no ternary in a compound assignment to an array element -- CSharp.lua emitted
        // "a:get(i) + (lv > 0) and lv or 1" (arithmetic on a boolean, TICK ERROR on every run in 0.44.3-0.45.0)
        var lvl = GetUnitLevel(u);
        if (lvl < 1)
        {
          lvl = 1;
        }

        _creepLevel[node] = _creepLevel[node] + lvl;
        if (_creep[node] == null)
        {
          _creep[node] = u;
        }
      }

      DestroyGroup(g);
      CreepCamps = 0;
      for (var i = 0; i < n; i++)
      {
        if (_creep[i] != null)
        {
          CreepCamps++;
        }
      }
    }

    // once: every node's CP -- CP nodes themselves, area nodes the nearest CP node on foot (walking edges, O(n^2))
    private static void OwnerCps()
    {
      var n = AiRouteData.Nodes;
      _ownerCp = new int[n];
      var dist = new float[n];
      var done = new bool[n];
      for (var i = 0; i < n; i++)
      {
        _ownerCp[i] = AiRouteData.NodeType[i] != 0 ? i : -1;
        dist[i] = AiRouteData.NodeType[i] != 0 ? 0f : -1f;
      }

      for (var round = 0; round < n; round++)
      {
        var u = -1;
        for (var i = 0; i < n; i++)
        {
          if (!done[i] && dist[i] >= 0f && (u < 0 || dist[i] < dist[u]))
          {
            u = i;
          }
        }

        if (u < 0)
        {
          break;
        }

        done[u] = true;
        var adj = _adj[u];
        for (var k = 0; k < adj.Count; k++)
        {
          var e = adj[k];
          var v = AiRouteData.EdgeA[e] == u ? AiRouteData.EdgeB[e] : AiRouteData.EdgeA[e];
          var nd = dist[u] + AiRouteData.EdgeCost[e];
          if (!done[v] && (dist[v] < 0f || nd < dist[v]))
          {
            dist[v] = nd;
            _ownerCp[v] = _ownerCp[u];
          }
        }
      }
    }

    // the creep to attack at node i for bot p, or null
    private static unit CreepAt(player p, int pid, int i, BotDiag d)
    {
      if (!Creeps || _creep == null || _creep[i] == null || _ownerCp[i] < 0)
      {
        return null;
      }

      var cu = _creep[i];
      if (GetUnitState(cu, UNIT_STATE_LIFE) <= 0.405f || GetOwningPlayer(cu) != Player(24) || IsUnitHidden(cu)
          || BlzIsUnitInvulnerable(cu))
      {
        return null;
      }

      var cp = SimpleBot.SafeCp(AiRouteData.NodeType[_ownerCp[i]]);
      if (cp == null || cp.Unit == null || GetOwningPlayer(cp.Unit) != p)
      {
        return null; // only in the area of a CP the bot holds
      }

      if (AiArmy.ArmyFood(pid) < _creepLevel[i] * CreepFoodPerLevel || SimpleBot.IsBlacklisted(pid, cu))
      {
        return null;
      }

      d.Neutral++;
      return cu;
    }

    // a target at node i: a CP / enemy building / leftover (Candidate), else a creep camp
    private static unit NodeTarget(player p, int pid, Faction fac, int i, BotDiag d)
    {
      var c = Candidate(p, pid, fac, i, d);
      return c ?? CreepAt(p, pid, i, d);
    }

    private static bool IsCreep(unit u) => GetOwningPlayer(u) == Player(24) && !IsUnitType(u, UNIT_TYPE_STRUCTURE)
                                           && SimpleBot.SafeCp(GetUnitTypeId(u)) == null;

    // "-creeps on" / "-creeps off"
    internal static void OnCreepsCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg.Length >= 10 && msg.Substring(0, 10) == "-creeps on")
      {
        Creeps = true;
      }
      else if (msg.Length >= 11 && msg.Substring(0, 11) == "-creeps off")
      {
        Creeps = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "creep camps as targets: "
        + (Creeps ? "ON (one-shots off)" : "OFF (one-shots on, 0.44.2)") + "  -- -creeps on|off");
    }

    private static void CheckPortals()
    {
      Ready();
      if (_padjIn == null)
      {
        _padjIn = new List<int>[AiRouteData.Nodes];
        for (var i = 0; i < AiRouteData.Nodes; i++)
        {
          _padjIn[i] = new List<int>();
        }
      }

      for (var i = 0; i < AiRouteData.Nodes; i++)
      {
        _padj[i].Clear();
        _padjIn[i].Clear();
      }

      ClearTrees();

      _pFrom.Clear();
      _pTo.Clear();
      _pCost.Clear();
      _pUnit.Clear();
      _pExitX.Clear();
      _pExitY.Clear();
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
          _padjIn[b].Add(_pFrom.Count);
          _pFrom.Add(a);
          _pTo.Add(b);
          _pCost.Add(cost);
          _pUnit.Add(u);
          _pExitX.Add(dx);
          _pExitY.Add(dy);
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
    private const float LeftoverCost = 2500f; // 0.44.1: score of an enemy building left in an own node's area
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
      var tu = t >= 0 ? NodeTarget(p, pid, fac, t, d) : null;
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

      UpdateGather(p, pid, path);
      return tu;
    }

    // ---- 0.45.0: gather points (waves) -- CONCEPTS.md section 0, step 2 -----------------------------------------
    // A route longer than GatherHops nodes ends first at the LAST OWN NODE before the frontier: the attack group
    // waits there until GatherShare of its food has arrived (within GatherRadius) or GatherMax seconds passed, then
    // it gets the chain to the target. Short routes go without waiting. Decided once per target. "-gather off" = 0.44.
    internal static bool Gather = true;
    private const int GatherHops = 5;
    private const float GatherMax = 60f;
    private const float GatherRadius = 1500f;
    private const float GatherShare = 0.8f;
    private static int[] _gatherFor = new int[24];     // target node the decision belongs to (-1 = none yet)
    private static int[] _gatherNode = new int[24];    // -1 = no gathering for this target
    private static float[] _gatherStart = new float[24];
    private static bool[] _gatherInit = new bool[24];
    internal static int GatherStarted;
    internal static int GatherAssembled;
    internal static int GatherTimeout;

    private static void UpdateGather(player p, int pid, List<int> path)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      if (!_gatherInit[pid])
      {
        _gatherInit[pid] = true;
        _gatherFor[pid] = -1;
        _gatherNode[pid] = -1;
      }

      var target = path[path.Count - 1];
      if (_gatherFor[pid] != target)
      {
        _gatherFor[pid] = target;
        _gatherNode[pid] = Gather && path.Count - 1 > GatherHops ? GatherNodeOn(p, pid, path) : -1;
        _gatherStart[pid] = SimpleBot.Clock;
        if (_gatherNode[pid] >= 0)
        {
          GatherStarted++;
        }

        return;
      }

      var gn = _gatherNode[pid];
      if (gn < 0)
      {
        return;
      }

      if (!Gather)
      {
        _gatherNode[pid] = -1;
        return;
      }

      if (AiArmy.GroupAssembled(pid, AiRouteData.NodeX[gn], AiRouteData.NodeY[gn], GatherRadius, GatherShare))
      {
        _gatherNode[pid] = -1;
        GatherAssembled++;
      }
      else if (SimpleBot.Clock - _gatherStart[pid] >= GatherMax)
      {
        _gatherNode[pid] = -1;
        GatherTimeout++;
      }
    }

    // the last own node on the path before the first node that is not the bot's -- not the group's own start node
    private static int GatherNodeOn(player p, int pid, List<int> path)
    {
      for (var j = 1; j < path.Count; j++)
      {
        if (!IsOwnNode(p, pid, path[j]))
        {
          return j - 1 >= 1 ? path[j - 1] : -1;
        }
      }

      return -1;
    }

    // true while the bot's attack group should go to its gather point (x, y) instead of the target
    internal static bool Gathering(int pid, out float x, out float y)
    {
      x = 0f;
      y = 0f;
      if (pid < 0 || pid >= 24 || !_gatherInit[pid] || _gatherNode[pid] < 0)
      {
        return false;
      }

      x = AiRouteData.NodeX[_gatherNode[pid]];
      y = AiRouteData.NodeY[_gatherNode[pid]];
      return true;
    }

    // "-gather on" / "-gather off"
    internal static void OnGatherCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg.Length >= 10 && msg.Substring(0, 10) == "-gather on")
      {
        Gather = true;
      }
      else if (msg.Length >= 11 && msg.Substring(0, 11) == "-gather off")
      {
        Gather = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "gather points: "
        + (Gather ? "ON (routes > 5 nodes wait at the last own node: 80 % of the group or 60 s)" : "OFF (0.44)")
        + "  -- -gather on|off");
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
        if (!_done[i])
        {
          continue; // 0.44.1: own nodes too -- leftover enemy buildings in their area (Leftover)
        }

        var cand = i == cur ? curUnit : NodeTarget(p, pid, fac, i, d);
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
      if (IsCreep(cand))
      {
        return s + CreepCost; // 0.44.3: creeps after CPs (neutral CP 4000 away = 2000, a camp at home = 3000)
      }

      if (_source[i])
      {
        s += LeftoverCost; // 0.44.1: clearing leftovers inside the own land must not override everything else
      }

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
        return Leftover(p, pid, i, d); // 0.44.1: our CP -- enemy buildings left in its area?
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

    // an enemy structure left in the area of a node the bot or an ally already holds (after the 10-min lock)
    private static unit Leftover(player p, int pid, int i, BotDiag d)
    {
      if (SimpleBot.Clock < 600f)
      {
        return null;
      }

      var s = EnemyStructure(p, i);
      if (s == null || SimpleBot.IsBlacklisted(pid, s))
      {
        return null;
      }

      d.Enemy++;
      return s;
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

    // ---- 0.44: movement chains --------------------------------------------------------------------------------
    // Every army order becomes a CHAIN: queued attack-moves along the route from the unit's own area -- like a player
    // shift-clicking attack-move B, C, D. Each unit follows the graph node by node on its own (no waiting for the
    // group, no "advance" orders) and fights what it meets. A waygate on the route: attack-move to a point in front of
    // it, then a queued right-click ON the waygate (walk in), then onward from the exit. Never an attack-move onto the
    // waygate itself: a unit teleported by it would then walk all the way back to "click" it.
    // At most ChainMax points; a unit that used them up stands idle away from its target and gets the next chain at
    // the heartbeat. "-chain off" = 0.43 (one order to the target, the engine finds the way).

    internal static bool Chains = true;
    private const int ChainMax = 8;
    private const float GateApproach = 400f;
    private const int OrderAttack = 851983;
    private const int OrderSmart = 851971;
    internal static int ChainsIssued;
    internal static int ChainPoints;
    internal static int ChainFallback;

    // 0.44.1: ONE backward route tree per destination (Dijkstra from the destination over reversed edges): every unit
    // heading there reads its way from it. 0.44.0 planned from each unit's own area and kept only the last result --
    // a group spread over several areas recomputed again and again (9261 plans in 25 min, 34860 ops in one slot).
    // 0.44.2: TreeSlots trees kept at once (flat arrays, slot s at offset s * Nodes). One tree only (0.44.1) was
    // recomputed whenever the loop alternated between destinations -- the attack group's target and the rest's
    // staging CP: up to 78318 ops in one slot in the 9.5-hour game.
    private const int TreeSlots = 4;
    private static float[] _rdist;
    private static int[] _rnext;      // next node toward the destination
    private static int[] _rnextEdge;  // edge used for that step: >= 0 walking edge, < 0 portal edge -(i + 1)
    private static bool[] _rdone;
    private static List<int>[] _padjIn; // node -> portal edges ENDING there (for the backward search)
    private static int[] _treeTo = new int[TreeSlots];
    private static player[] _treePlayer = new player[TreeSlots];
    private static float[] _treeAt = new float[TreeSlots];
    private static int _treeNext;
    private static int _tb;           // offset of the slot in use
    internal static int Trees;

    private static void ClearTrees()
    {
      for (var k = 0; k < TreeSlots; k++)
      {
        _treeTo[k] = -1;
      }
    }

    // the slot holding the tree for (p, to) at this moment -- computed if needed (round-robin replacement)
    private static int TreeFor(player p, int to)
    {
      for (var k = 0; k < TreeSlots; k++)
      {
        if (_treeTo[k] == to && _treePlayer[k] == p && _treeAt[k] == SimpleBot.Clock)
        {
          return k;
        }
      }

      var s = _treeNext;
      _treeNext = (_treeNext + 1) % TreeSlots;
      BackwardTree(p, to, s);
      return s;
    }

    private static void BackwardTree(player p, int to, int slot)
    {
      Trees++;
      _treeTo[slot] = to;
      _treePlayer[slot] = p;
      _treeAt[slot] = SimpleBot.Clock;
      var n = AiRouteData.Nodes;
      if (_rdist == null)
      {
        _rdist = new float[n * TreeSlots];
        _rnext = new int[n * TreeSlots];
        _rnextEdge = new int[n * TreeSlots];
        _rdone = new bool[n * TreeSlots];
      }

      _tb = slot * n;
      for (var i = 0; i < n; i++)
      {
        _rdist[_tb + i] = -1f;
        _rnext[_tb + i] = -1;
        _rnextEdge[_tb + i] = 0;
        _rdone[_tb + i] = false;
      }

      var relax = 0;
      _heapN = 0;
      _rdist[_tb + to] = 0f;
      Push(to, 0f);
      while (_heapN > 0)
      {
        var v = Pop();
        if (_rdone[_tb + v])
        {
          continue;
        }

        _rdone[_tb + v] = true;
        var dv = _rdist[_tb + v];
        var adj = _adj[v];
        for (var k = 0; k < adj.Count; k++)
        {
          var e = adj[k];
          var rock = AiRouteData.EdgeRocks[e];
          if (rock >= 0 && !_rockOpen[rock])
          {
            continue;
          }

          var u = AiRouteData.EdgeA[e] == v ? AiRouteData.EdgeB[e] : AiRouteData.EdgeA[e];
          relax++;
          BackRelax(p, u, v, dv + AiRouteData.EdgeCost[e], e);
        }

        var pin = _padjIn != null ? _padjIn[v] : null;
        if (pin != null)
        {
          for (var k = 0; k < pin.Count; k++)
          {
            var pe = pin[k];
            relax++;
            BackRelax(p, _pFrom[pe], v, dv + _pCost[pe], -(pe + 1));
          }
        }
      }

      LastRelax = relax;
      SimpleBot.WaveOps += relax;
    }

    // u -> v is the forward step; walking through v costs extra when v is enemy-held (as in the forward search)
    private static void BackRelax(player p, int u, int v, float nd, int edge)
    {
      if (_rdone[_tb + u])
      {
        return;
      }

      nd += EnterCost(p, v);
      if (_rdist[_tb + u] < 0f || nd < _rdist[_tb + u])
      {
        _rdist[_tb + u] = nd;
        _rnext[_tb + u] = v;
        _rnextEdge[_tb + u] = edge;
        Push(u, nd);
      }
    }

    // Orders unit u (owner p) to (x, y) as a chain. False = no chain possible (same area, unknown ground, unreachable):
    // the caller gives the plain order.
    internal static bool Chain(player p, unit u, float x, float y)
    {
      if (!On || !Chains)
      {
        return false;
      }

      Ready();
      var from = StartNode(GetUnitX(u), GetUnitY(u));
      var to = StartNode(x, y);
      if (from < 0 || to < 0 || from == to)
      {
        return false;
      }

      _tb = TreeFor(p, to) * AiRouteData.Nodes;
      if (!_rdone[_tb + from])
      {
        ChainFallback++;
        return false;
      }

      var points = 0;
      var node = from;
      var guard = 0;
      while (node != to && node >= 0 && points < ChainMax && guard++ <= AiRouteData.Nodes)
      {
        var next = _rnext[_tb + node];
        var e = _rnextEdge[_tb + node];
        if (next < 0)
        {
          break;
        }

        if (e < 0)
        {
          var pe = -e - 1;
          var gate = _pUnit[pe];
          if (gate == null || GetUnitState(gate, UNIT_STATE_LIFE) <= 0.405f)
          {
            break; // the waygate is gone: the chain ends here, the heartbeat re-plans
          }

          // c_01 in front of the waygate, c_02 = the click on it, c_03 = its exit
          float gx = GetUnitX(gate), gy = GetUnitY(gate);
          var bx = AiRouteData.NodeX[node] - gx;
          var by = AiRouteData.NodeY[node] - gy;
          var bl = SquareRoot(bx * bx + by * by);
          if (bl > 1f)
          {
            ChainPoint(u, gx + bx / bl * GateApproach, gy + by / bl * GateApproach, points == 0);
            points++;
          }

          if (points == 0)
          {
            IssueTargetOrderById(u, OrderSmart, gate);
          }
          else
          {
            BlzQueueTargetOrderById(u, OrderSmart, gate);
          }

          points++;
          ChainPoint(u, _pExitX[pe], _pExitY[pe], false);
          points++;
        }

        if (next == to)
        {
          ChainPoint(u, x, y, points == 0);
        }
        else
        {
          ChainPoint(u, AiRouteData.NodeX[next], AiRouteData.NodeY[next], points == 0);
        }

        points++;
        node = next;
      }

      if (points == 0)
      {
        return false;
      }

      ChainsIssued++;
      ChainPoints += points;
      SimpleBot.OpOrders++;
      SimpleBot.OrdersIssued++;
      return true;
    }

    // first point replaces the unit's orders (and old queue), the others are queued (shift-click)
    private static void ChainPoint(unit u, float x, float y, bool first)
    {
      if (first)
      {
        AiScheduler.CountOrder(u, x, y);
        IssuePointOrderById(u, OrderAttack, x, y);
      }
      else
      {
        BlzQueuePointOrderById(u, OrderAttack, x, y);
      }
    }

    // "-chain on" / "-chain off"
    internal static void OnChainCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg.Length >= 9 && msg.Substring(0, 9) == "-chain on")
      {
        Chains = true;
      }
      else if (msg.Length >= 10 && msg.Substring(0, 10) == "-chain off")
      {
        Chains = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "army movement: "
        + (Chains ? "CHAINS (queued attack-moves along the graph, waygates clicked)" : "ONE ORDER to the target (0.43)")
        + "  -- -chain on|off");
    }

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
            + I2S(closed) + "/" + I2S(AiRouteData.RockX.Length) + "; nodes with enemy-able structures " + I2S(areas)
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

      // 0.44.1: the waygate edges (to see e.g. the Dark Portal pairs) and units found idle at a waygate exit
      o.Add("waygates (" + I2S(_pFrom.Count) + "), units idle at an exit: " + I2S(ExitIdle) + ", chain trees "
            + I2S(Trees) + "; creep camps " + I2S(CreepCamps) + " (creeps " + (Creeps ? "on" : "off") + "); gather "
            + (Gather ? "on" : "off") + " started=" + I2S(GatherStarted) + " assembled=" + I2S(GatherAssembled)
            + " timeout=" + I2S(GatherTimeout));
      for (var k = 0; k < _pFrom.Count; k++)
      {
        o.Add("  " + NodeName(_pFrom[k]) + " -> " + NodeName(_pTo[k]) + " cost " + I2S(R2I(_pCost[k])) + " exit "
              + I2S(R2I(_pExitX[k])) + "," + I2S(R2I(_pExitY[k])));
      }
    }

    internal static int ExitIdle;

    // 0.44.1: is (x, y) within 800 of a waygate exit? (a unit idle there lost its queued chain in the teleport)
    internal static bool NearWaygateExit(float x, float y)
    {
      for (var k = 0; k < _pExitX.Count; k++)
      {
        var dx = _pExitX[k] - x;
        var dy = _pExitY[k] - y;
        if (dx * dx + dy * dy < 800f * 800f)
        {
          return true;
        }
      }

      return false;
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
