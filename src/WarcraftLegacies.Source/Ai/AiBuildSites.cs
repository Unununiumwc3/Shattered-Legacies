using System.Collections.Generic;
using MacroTools.ControlPoints;
using MacroTools.Legends;

namespace WarcraftLegacies.Source.Ai
{
  // Location policy lives separately from AiBuilder's project state to keep both classes small for CSharp.lua.
  internal static class AiBuildSites
  {
    internal static Dictionary<string, int[]> Headquarters = new();
    private const float Safety = 1500f;
    private const int Lane = 64;

    internal static int HalfWidth(int type)
    {
      return AiBuildData.HalfWidth.TryGetValue(type, out var width) ? width : 96;
    }

    private static bool Overlaps(float x, float y, float otherX, float otherY, int space)
    {
      return x > otherX - space && x < otherX + space && y > otherY - space && y < otherY + space;
    }

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
    internal static bool HomeAnchor(player p, bool established, List<int> hallTypes, float hx, float hy, unit worker, out float x, out float y)
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
      // Opening workers can establish themselves before capturing a CP. Recovery
      // after losing a supported base may instead use a living, owned capital.
      if (!established && Safe(p, x, y))
      {
        return true;
      }
      if (established)
      {
        Headquarters.TryGetValue(SimpleBot.FacName(p), out var ids);
        var capitals = CapitalManager.GetAll();
        for (var k = 0; k < capitals.Count; k++)
        {
          var capital = capitals[k];
          var home = capital.Unit;
          if (home == null || GetOwningPlayer(home) != p || GetUnitState(home, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }
          var match = ids == null && capital.Essential;
          for (var h = 0; ids != null && h < ids.Length; h++)
          {
            match = match || ids[h] == capital.UnitType;
          }
          if (match && comp >= 0 && AiRoutes.CompAt(GetUnitX(home), GetUnitY(home)) == comp && Safe(p, GetUnitX(home), GetUnitY(home)))
          {
            x = GetUnitX(home);
            y = GetUnitY(home);
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
      if (sites.X.Count == 0 && !sites.Shore)
      {
        // Some custom structures permit normally unbuildable ground. Let the
        // worker try walkable sites when the standard buildable-core search is empty.
        CollectSites(p, state, sites, false);
      }
    }

    private static void CollectSites(player p, AiBuildState state, AiBuildCandidates sites, bool standard)
    {
      if (!AiRoutes.StagePoint(p, GetPlayerId(p), out var rallyX, out var rallyY))
      {
        rallyX = sites.AnchorX;
        rallyY = sites.AnchorY;
      }
      for (var sample = 0; sample < 4; sample++)
      {
        // Always check the closest ring; rotate the remaining rings over visits.
        var ring = sample == 0 ? 0 : 1 + ((state.SearchPhase * 3 + sample - 1) % 11);
        var radius = 256f + 192f * ring;
        for (var a = 0; a < 16; a++)
        {
          if (state.ProbesThisVisit >= 384)
          {
            return;
          }
          state.ProbesThisVisit++;
          SimpleBot.ProdOps++;
          AiBuilder.SiteProbes++;
          var angle = ((a * 5 + state.SearchPhase) % 16) * 0.392699f +
                      ((ring + state.SearchPhase) % 2) * 0.196350f;
          var x = sites.AnchorX + radius * Cos(angle);
          var y = sites.AnchorY + radius * Sin(angle);
          if ((SimpleBot.Clock < state.BadUntil && Dist2(x, y, state.BadX, state.BadY) < 192f * 192f)
              || !(sites.Shore ? ShoreOpen(x, y, sites.Component) : FootprintOpen(x, y, standard, sites.HalfWidth)
                  && AiRoutes.CompAt(x, y) == sites.Component && ExitOpen(x, y, rallyX, rallyY, sites.HalfWidth))
              || !StructureClear(x, y, sites.HalfWidth) || !Safe(p, x, y))
          {
            continue;
          }
          var reserved = false;
          for (var k = 0; k < state.Projects.Count; k++)
          {
            var project = state.Projects[k];
            reserved = reserved || Overlaps(x, y, project.X, project.Y, sites.HalfWidth + HalfWidth(project.Type) + Lane);
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

    private static bool ShoreOpen(float x, float y, int component)
    {
      var land = false;
      var water = false;
      for (var i = 0; i < 8; i++)
      {
        var angle = i * 0.785398f;
        var px = x + 160f * Cos(angle);
        var py = y + 160f * Sin(angle);
        land = land || (!IsTerrainPathable(px, py, PATHING_TYPE_WALKABILITY) && AiRoutes.CompAt(px, py) == component);
        water = water || !IsTerrainPathable(px, py, PATHING_TYPE_FLOATABILITY);
      }
      return land && water;
    }

    private static bool ExitOpen(float x, float y, float rallyX, float rallyY, int halfWidth)
    {
      var heading = Atan2(rallyY - y, rallyX - x);
      for (var side = -1; side <= 1; side++)
      {
        var angle = heading + side * 0.785398f;
        var dx = Cos(angle);
        var dy = Sin(angle);
        var clear = true;
        for (var distance = halfWidth + 32; clear && distance <= halfWidth + 160; distance += 32)
        {
          var px = x + distance * dx;
          var py = y + distance * dy;
          clear = !IsTerrainPathable(px, py, PATHING_TYPE_WALKABILITY)
                  && !IsTerrainPathable(px - 32f * dy, py + 32f * dx, PATHING_TYPE_WALKABILITY)
                  && !IsTerrainPathable(px + 32f * dy, py - 32f * dx, PATHING_TYPE_WALKABILITY);
        }
        if (clear)
        {
          return true;
        }
      }
      return false;
    }

    private static bool FootprintOpen(float x, float y, bool standard, int halfWidth)
    {
      var pathing = standard ? PATHING_TYPE_BUILDABILITY : PATHING_TYPE_WALKABILITY;
      var extent = halfWidth - 16;
      for (var dx = -1; dx <= 1; dx++)
      {
        for (var dy = -1; dy <= 1; dy++)
        {
          if (IsTerrainPathable(x + dx * extent, y + dy * extent, pathing))
          {
            return false;
          }
        }
      }
      return true;
    }

    private static bool StructureClear(float x, float y, int halfWidth)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, (halfWidth + 256 + Lane) * 1.42f, null);
      var clear = true;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
            && Overlaps(x, y, GetUnitX(u), GetUnitY(u), halfWidth + HalfWidth(GetUnitTypeId(u)) + Lane))
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
