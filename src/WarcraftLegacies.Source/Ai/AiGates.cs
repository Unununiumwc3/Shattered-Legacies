using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- gate removal (gate units + gate destructables), Tempest Reach edge, Thandol bridge.
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiGates
  {
    public static int LastSweepOps; // named graph gates (e.g. "greymane")
    private static bool _thandolOpened;      // v25: force-open Thandol Span once at 20 min
    private static bool _tempestWired;

    private static float _lastGateSweep = -1f;
    private static HashSet<int> _gateSet;
    private static bool _destGatesSwept;
    private static HashSet<int> _destGateSet;

    // One-time: kill every destructible GATE doodad so no chokepoint gate blocks anyone. Gates are preplaced,
    // so the single whole-map destructible enum runs once. Only AiGraph.GateDestructibles types (gates) are
    // touched -- bridges and tree/stone walls are excluded, so this cannot delete a bridge deck.
    internal static void SweepDestructibleGates()
    {
      if (_destGatesSwept)
      {
        return;
      }

      _destGatesSwept = true;
      if (_destGateSet == null)
      {
        _destGateSet = new HashSet<int>();
        for (var i = 0; i < AiGraph.GateDestructibles.Count; i++)
        {
          _destGateSet.Add(AiGraph.GateDestructibles[i]);
        }
      }

      EnumDestructablesInRect(GetWorldBounds(), null, KillGateDestructable);
    }

    private static void KillGateDestructable()
    {
      var d = GetEnumDestructable();
      if (_destGateSet.Contains(GetDestructableTypeId(d)))
      {
        SetDestructableInvulnerable(d, false);
        KillDestructable(d);
      }
    }

    // Remove every bot-owned wall-gate so a bot can never wall its own army in. Runs once immediately, then
    // ~every 5 min (gates can be built later). Strict type match against AiGraph.GateTypes -> only real gates.
    internal static void SweepBotGates()
    {
      if (_lastGateSweep >= 0f && SimpleBot._clock - _lastGateSweep < 300f)
      {
        return;
      }

      _lastGateSweep = SimpleBot._clock;
      if (_gateSet == null)
      {
        _gateSet = new HashSet<int>();
        for (var i = 0; i < AiGraph.GateTypes.Count; i++)
        {
          _gateSet.Add(AiGraph.GateTypes[i]);
        }
      }

      // v27.2: whole-map unit enum (not per-player) -- catches gate units no matter who owns them or how they
      // were placed. Strict gate-type match -> only real gates are removed. Use -id on a surviving gate to read
      // its true type id; if it isn't one of these, add it to AiGraph.GateTypes.
      var g = CreateGroup();
      GroupEnumUnitsInRect(g, GetWorldBounds(), null);
      unit u;
      var touched = 0;
      var removed = 0;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        touched++;
        if (_gateSet.Contains(GetUnitTypeId(u)))
        {
          RemoveUnit(u);
          removed++;
        }
      }

      DestroyGroup(g);
      SimpleBot.WaveOps += touched;
      LastSweepOps = touched;
      if (removed > 0)
      {
        SimpleBot.Log("gate sweep: removed " + I2S(removed) + " gate units (" + I2S(touched) + " units scanned)");
      }
    }

    // Tempest Reach has no named CP constant in WL source (CPs are auto-detected by buff), so we resolve it at
    // runtime from its coord (8960,-1152): find the nearest CP and add the Blackwald edge. Niche Gilneas spur --
    // if nothing sits there, it just stays a leaf.
    internal static void WireTempestReach()
    {
      if (_tempestWired)
      {
        return;
      }

      _tempestWired = true;
      const int blackwald = 1848653398;
      var bestType = 0;
      var bestD = 1500f * 1500f;
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      for (var i = 0; i < cps.Count; i++)
      {
        var ex = GetUnitX(cps[i].Unit) - 8960f;
        var ey = GetUnitY(cps[i].Unit) - (-1152f);
        var d = ex * ex + ey * ey;
        if (d < bestD)
        {
          bestD = d;
          bestType = cps[i].UnitType;
        }
      }

      if (bestType != 0 && bestType != blackwald)
      {
        AiGraph.Combined.Add(new Edge(blackwald, bestType));
      }
    }

    // WL sets Thandol Span (destructible LT08 ~15695,457) invulnerable and never opens it, so bots can never
    // cross Arathi <-> Dun Modr. Force it open at 20 min, matching that edge's OpenTime.
    private static void OpenThandolIfDue()
    {
      if (_thandolOpened || SimpleBot._clock < 1200f)
      {
        return;
      }

      _thandolOpened = true;
      var best = FindDestructableNear(1280585784, 15695f, 457f, 2000f);
      if (best != null)
      {
        SetDestructableInvulnerable(best, false);
        KillDestructable(best);
        PingMinimap(15695f, 457f, 4f);
      }
    }

    private static destructable _destFound;
    private static int _destWant;
    private static float _destBx, _destBy, _destBest;
    private static destructable FindDestructableNear(int typeId, float x, float y, float r)
    {
      _destFound = null;
      _destWant = typeId;
      _destBx = x;
      _destBy = y;
      _destBest = r * r;
      if (SimpleBot._itemRect == null)
      {
        SimpleBot._itemRect = Rect(0f, 0f, 1f, 1f);
      }

      SetRect(SimpleBot._itemRect, x - r, y - r, x + r, y + r);
      EnumDestructablesInRect(SimpleBot._itemRect, null, PickDestructable);
      return _destFound;
    }

    private static void PickDestructable()
    {
      var d = GetEnumDestructable();
      if (GetDestructableTypeId(d) != _destWant)
      {
        return;
      }

      var ex = GetDestructableX(d) - _destBx;
      var ey = GetDestructableY(d) - _destBy;
      var dist = ex * ex + ey * ey;
      if (dist <= _destBest)
      {
        _destBest = dist;
        _destFound = d;
      }
    }
  }
}
