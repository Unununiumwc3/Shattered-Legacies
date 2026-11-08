using System.Collections.Generic;
using MacroTools.ControlPoints;

namespace WarcraftLegacies.Source.Ai
{
  // Location policy lives separately from AiBuilder's project state to keep both classes small for CSharp.lua.
  internal static class AiBuildSites
  {
    private const float Safety = 1500f;
    private const float Clearance = 256f;

    internal static unit StructureNear(player p, int type, float x, float y, float r)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, r, null);
      unit found = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetOwningPlayer(u) == p && GetUnitTypeId(u) == type
            && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          found = u;
        }
      }
      DestroyGroup(g);
      return found;
    }

    internal static unit AnyHallNear(player p, List<int> hallTypes, float x, float y, float r)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, r, null);
      unit found = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetOwningPlayer(u) == p && (IsUnitType(u, UNIT_TYPE_TOWNHALL) || SimpleBot.ListHas(hallTypes, GetUnitTypeId(u)))
            && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          found = u;
        }
      }
      DestroyGroup(g);
      return found;
    }

    internal static int ProducerCountNear(player p, List<int> types, float x, float y, float r)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, r, null);
      var count = 0;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetOwningPlayer(u) == p && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
            && SimpleBot.ListHas(types, GetUnitTypeId(u)))
        {
          count++;
        }
      }
      DestroyGroup(g);
      return count;
    }

    private static float Dist2(float x, float y, float a, float b)
    {
      var dx = x - a;
      var dy = y - b;
      return dx * dx + dy * dy;
    }

    // Shortest walking-graph cost from a worker, ignoring portals and ferries. This is used
    // only when its original HQ area is unavailable; Euclidean proximity across a ravine
    // is not a useful measure of "quickest" recovery. O(nodes * (nodes + edges)).
    private static float[] WalkingCosts(float x, float y)
    {
      var dist = new float[AiRouteData.Nodes];
      var done = new bool[AiRouteData.Nodes];
      for (var i = 0; i < dist.Length; i++)
      {
        dist[i] = -1f;
      }
      var source = AiRoutes.NodeAt(x, y);
      if (source < 0)
      {
        return dist;
      }
      dist[source] = 0f;
      for (var round = 0; round < dist.Length; round++)
      {
        var u = -1;
        for (var i = 0; i < dist.Length; i++)
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
        for (var e = 0; e < AiRouteData.Edges; e++)
        {
          if (!AiRoutes.WalkEdgeOpen(e))
          {
            continue;
          }
          var a = AiRouteData.EdgeA[e];
          var b = AiRouteData.EdgeB[e];
          var v = a == u ? b : (b == u ? a : -1);
          if (v < 0 || done[v])
          {
            continue;
          }
          var nd = dist[u] + AiRouteData.EdgeCost[e];
          if (dist[v] < 0f || nd < dist[v])
          {
            dist[v] = nd;
          }
        }
      }
      return dist;
    }

    private static bool FriendlyCp(unit u, player p)
    {
      if (u == null || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        return false;
      }
      var o = GetOwningPlayer(u);
      return o == p || (GetPlayerId(o) < 24 && IsPlayerAlly(p, o));
    }

    private static bool Safe(player p, float x, float y)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, Safety, null);
      var safe = true;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || IsUnitAlly(u, p))
        {
          continue;
        }
        var ownerId = GetPlayerId(GetOwningPlayer(u));
        if (ownerId >= 25)
        {
          continue; // neutral/passive map-owned units, buildings, CP scenery are NOT enemy armies
        }
        if (ownerId == 24)
        {
          // Neutral hostile creeps are a local danger, not a 1500-unit exclusion zone.
          // A remote camp must not make an entire faction HQ unbuildable.
          if (Dist2(x, y, GetUnitX(u), GetUnitY(u)) > 650f * 650f)
          {
            continue;
          }
        }
        safe = false;
        break;
      }
      DestroyGroup(g);
      return safe;
    }

    // Recover in the old HQ area if it is still supported by a friendly CP or hall.
    // If the HQ was lost, choose the NEAREST reachable own or allied CP; if there are none, use the worker.
    internal static bool HomeAnchor(player p, List<int> hallTypes, float hx, float hy, unit worker, out float x, out float y)
    {
      x = GetUnitX(worker);
      y = GetUnitY(worker);
      var comp = AiRoutes.CompAt(x, y);
      var homeComp = AiRoutes.CompAt(hx, hy);
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      if (comp == homeComp && AnyHallNear(p, hallTypes, hx, hy, 1900f) != null && Safe(p, hx, hy))
      {
        x = hx;
        y = hy;
        return true;
      }
      if (comp == homeComp)
      {
        for (var i = 0; i < cps.Count; i++)
        {
          var u = cps[i].Unit;
          if (FriendlyCp(u, p) && Dist2(hx, hy, GetUnitX(u), GetUnitY(u)) < 2000f * 2000f
              && Safe(p, hx, hy))
          {
            x = hx;
            y = hy;
            return true;
          }
        }
      }
      var walk = WalkingCosts(x, y);
      var best = -1;
      var bestD = 0f;
      for (var i = 0; i < cps.Count; i++)
      {
        var u = cps[i].Unit;
        if (!FriendlyCp(u, p) || AiRoutes.CompAt(GetUnitX(u), GetUnitY(u)) != comp)
        {
          continue;
        }
        var n = AiRoutes.NodeAt(GetUnitX(u), GetUnitY(u));
        if (n < 0 || walk[n] < 0f)
        {
          continue;
        }
        var d = walk[n];
        if ((best < 0 || d < bestD) && Safe(p, GetUnitX(u), GetUnitY(u)))
        {
          best = i;
          bestD = d;
        }
      }
      if (best >= 0)
      {
        x = GetUnitX(cps[best].Unit);
        y = GetUnitY(cps[best].Unit);
      }
      return Safe(p, x, y);
    }

    internal static bool EmergencyAnchor(player p, float hx, float hy, out float x, out float y)
    {
      x = hx;
      y = hy;
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      var best = -1;
      var bestD = 0f;
      for (var i = 0; i < cps.Count; i++)
      {
        var u = cps[i].Unit;
        if (!FriendlyCp(u, p))
        {
          continue;
        }
        var d = Dist2(hx, hy, GetUnitX(u), GetUnitY(u));
        if ((best < 0 || d < bestD) && Safe(p, GetUnitX(u), GetUnitY(u)))
        {
          best = i;
          bestD = d;
        }
      }
      if (best >= 0)
      {
        x = GetUnitX(cps[best].Unit);
        y = GetUnitY(cps[best].Unit);
      }
      return !IsTerrainPathable(x, y, PATHING_TYPE_WALKABILITY);
    }

    // BFS limited to four OPEN walking edges (never portal/ferry edges). All search nodes are static map nodes.
    private static int[] WalkingHops(int from)
    {
      var hops = new int[AiRouteData.Nodes];
      for (var n = 0; n < hops.Length; n++)
      {
        hops[n] = -1;
      }
      if (from < 0 || from >= hops.Length)
      {
        return hops;
      }
      hops[from] = 0;
      for (var depth = 0; depth < 4; depth++)
      {
        for (var e = 0; e < AiRouteData.Edges; e++)
        {
          if (!AiRoutes.WalkEdgeOpen(e))
          {
            continue;
          }
          var a = AiRouteData.EdgeA[e];
          var b = AiRouteData.EdgeB[e];
          if (hops[a] == depth && hops[b] < 0)
          {
            hops[b] = depth + 1;
          }
          if (hops[b] == depth && hops[a] < 0)
          {
            hops[a] = depth + 1;
          }
        }
      }
      return hops;
    }

    // Preferred site: an allied or owned CP two route edges behind the current army staging point.
    // No valid front or worker on the right continent -> no expansion (never teleport a worker across seas).
    internal static bool ForwardAnchor(player p, List<int> hallTypes, List<unit> workers, List<AiBuildBase> bases,
      out float x, out float y)
    {
      x = 0f;
      y = 0f;
      if (!AiRoutes.StagePoint(p, GetPlayerId(p), out var sx, out var sy))
      {
        return false;
      }
      var stage = AiRoutes.NodeAt(sx, sy);
      var hops = WalkingHops(stage);
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      var best = -1;
      var score = 0f;
      for (var i = 0; i < cps.Count; i++)
      {
        var cp = cps[i].Unit;
        if (!FriendlyCp(cp, p))
        {
          continue;
        }
        var cx = GetUnitX(cp);
        var cy = GetUnitY(cp);
        var n = AiRoutes.NodeAt(cx, cy);
        if (n < 0 || hops[n] < 1 || hops[n] > 4 || !Safe(p, cx, cy))
        {
          continue;
        }
        var nearestWorker = -1f;
        for (var w = 0; w < workers.Count; w++)
        {
          if (AiRoutes.CompAt(GetUnitX(workers[w]), GetUnitY(workers[w])) != AiRoutes.CompOfNode(n))
          {
            continue;
          }
          var d = Dist2(GetUnitX(workers[w]), GetUnitY(workers[w]), cx, cy);
          if (nearestWorker < 0f || d < nearestWorker)
          {
            nearestWorker = d;
          }
        }
        if (nearestWorker < 0f)
        {
          continue;
        }
        if (AnyHallNear(p, hallTypes, cx, cy, 2600f) != null)
        {
          continue;
        }
        var occupied = false;
        for (var b = 0; b < bases.Count; b++)
        {
          occupied = occupied || Dist2(cx, cy, bases[b].X, bases[b].Y) < 3200f * 3200f;
        }
        if (occupied)
        {
          continue;
        }
        var depth = hops[n] - 2;
        // Prefer two graph edges over Euclidean proximity; only then prefer the closest worker.
        var value = depth * depth * 100000000f + nearestWorker;
        if (best < 0 || value < score)
        {
          best = i;
          score = value;
        }
      }
      if (best < 0)
      {
        return false;
      }
      x = GetUnitX(cps[best].Unit);
      y = GetUnitY(cps[best].Unit);
      return true;
    }

    // Collect a few distinct sites once per anchor and planning pass. WC3 checks the
    // actual building mask, race-specific terrain requirements and obstructions.
    internal static void FindSites(player p, AiBuildState state, AiBuildCandidates sites)
    {
      CollectSites(p, state, sites, true);
      if (sites.X.Count == 0)
      {
        // Some custom structures permit normally unbuildable ground. Let the
        // worker try walkable sites when the standard buildable-core search is empty.
        CollectSites(p, state, sites, false);
      }
    }

    private static void CollectSites(player p, AiBuildState state, AiBuildCandidates sites, bool standard)
    {
      for (var ring = 0; ring < 12; ring++)
      {
        var radius = 256f + 192f * ring;
        for (var a = 0; a < 16; a++)
        {
          var angle = ((a * 5 + state.SearchPhase) % 16) * 0.392699f +
                      ((ring + state.SearchPhase) % 2) * 0.196350f;
          var x = sites.AnchorX + radius * Cos(angle);
          var y = sites.AnchorY + radius * Sin(angle);
          if ((SimpleBot.Clock < state.BadUntil && Dist2(x, y, state.BadX, state.BadY) < 192f * 192f)
              || !FootprintOpen(x, y, standard) || AiRoutes.CompAt(x, y) != sites.Component
              || !StructureClear(x, y) || !Safe(p, x, y))
          {
            continue;
          }
          var reserved = false;
          for (var k = 0; k < state.Projects.Count; k++)
          {
            var project = state.Projects[k];
            reserved = reserved || Dist2(x, y, project.X, project.Y) < Clearance * Clearance;
          }
          if (!reserved)
          {
            sites.X.Add(x);
            sites.Y.Add(y);
            if (sites.X.Count == 4)
            {
              return;
            }
          }
        }
      }
    }

    private static bool FootprintOpen(float x, float y, bool standard)
    {
      // Test a small buildable core. Requiring an entire 840-unit clearing
      // excluded usable lots between structures inside preplaced cities.
      var pathing = standard ? PATHING_TYPE_BUILDABILITY : PATHING_TYPE_WALKABILITY;
      for (var dx = -64; dx <= 64; dx += 64)
      {
        for (var dy = -64; dy <= 64; dy += 64)
        {
          if (IsTerrainPathable(x + dx, y + dy, pathing))
          {
            return false;
          }
        }
      }
      return (!IsTerrainPathable(x - 192f, y, PATHING_TYPE_WALKABILITY)
              && !IsTerrainPathable(x + 192f, y, PATHING_TYPE_WALKABILITY))
             || (!IsTerrainPathable(x, y - 192f, PATHING_TYPE_WALKABILITY)
                 && !IsTerrainPathable(x, y + 192f, PATHING_TYPE_WALKABILITY));
    }

    private static bool StructureClear(float x, float y)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, Clearance, null);
      var clear = true;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          clear = false;
          break;
        }
      }
      DestroyGroup(g);
      return clear;
    }
  }
}
