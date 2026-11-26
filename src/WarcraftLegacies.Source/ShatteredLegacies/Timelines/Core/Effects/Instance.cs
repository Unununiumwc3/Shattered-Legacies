using System.Collections.Generic;
using MacroTools.Instances;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.57, building block "instance": a base of its own, reachable only through a waygate.
  // 0.82 (void-area dungeons): the instance is a dungeon copied from the 24p map into a void part of our map
  // (tools/shattered-legacies/terrain/copy_area.py, the layout in Timelines/VOID_DUNGEONS.md) -- the void around it
  // is its wall. Prepared at map setup, visible and invulnerable:
  //   - the inner waygate where the 24p map had its entrance (closed)
  //   - its buildings at their 24p spots (Neutral Passive)
  //   - registered as a WL instance (MacroTools.Instances): no world-tree immortality, Summon Legion or slipstream
  //     across its border -- like Outland and Argus
  // 0.84 (maintainer): the gates have their own models (Dungeons/DungeonObjects: a circle, a gold mine, the
  // Undercity's iron gate) and the world entrance stands from the start too, shut -- everyone sees that something
  // will happen there. The entrance can also be an existing building that has the waygate ability (Shadowforge
  // City). Open(): both gates open, the buildings go to the owner (they stay invulnerable).
  internal sealed class Instance
  {
    internal const int StockWaygate = 1853319028; // nwgt

    internal readonly string Name;
    private readonly Rectangle _area;
    private readonly float _gateX, _gateY;     // the inner waygate
    private readonly float _arriveX, _arriveY; // where the entrance puts you (a step from the inner gate)
    private readonly int _gateType;
    private readonly List<unit> _buildings = new();
    private unit _gate;
    private unit _entrance;
    private bool _open;
    private float _exitX, _exitY;

    internal Instance(string name, Rectangle area, int gateType, float gateX, float gateY, float arriveX,
      float arriveY)
    {
      Name = name;
      _area = area;
      _gateType = gateType;
      _gateX = gateX;
      _gateY = gateY;
      _arriveX = arriveX;
      _arriveY = arriveY;
      _exitX = gateX;
      _exitY = gateY;
    }

    internal bool IsOpen => _open;

    internal bool Contains(float x, float y) => _area.Contains(x, y);

    // buildings: unit types and their spots (x, y), facing 270
    internal void Prepare(int[] types, float[] x, float[] y)
    {
      var passive = Player(PLAYER_NEUTRAL_PASSIVE);
      _gate = CreateUnit(passive, _gateType, _gateX, _gateY, 270f);
      SetUnitInvulnerable(_gate, true);
      WaygateActivate(_gate, false);
      for (var i = 0; i < types.Length; i++)
      {
        var u = CreateUnit(passive, types[i], x[i], y[i], 270f);
        SetUnitInvulnerable(u, true);
        _buildings.Add(u);
      }

      var wl = new MacroTools.Instances.Instance(_area);
      wl.AddGate(new Gate(() => new Point(_gateX, _gateY), () => new Point(_exitX, _exitY)));
      InstanceSystem.Register(wl);
    }

    // 0.84: the world entrance, shut until Open; (exitX, exitY) = where the inner gate puts you
    internal void PlaceEntrance(int type, float x, float y, float exitX, float exitY)
    {
      if (_entrance != null)
      {
        return;
      }

      var u = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), type, x, y, 270f);
      SetUnitInvulnerable(u, true);
      UseEntrance(u, exitX, exitY);
    }

    // 0.84: an existing building with the waygate ability is the entrance (its owner and life are left alone)
    internal void UseEntrance(unit entrance, float exitX, float exitY)
    {
      _entrance = entrance;
      _exitX = exitX;
      _exitY = exitY;
      WaygateActivate(_entrance, false);
    }

    // the entrance at (x, y), a stock waygate (the Undercity: Lordaeron's palace ruins) -- then Open(owner)
    internal void Open(player owner, float x, float y)
    {
      PlaceEntrance(StockWaygate, x, y, x, y - 250f);
      Open(owner);
    }

    // both waygates open; the buildings to the owner (they stay invulnerable)
    internal void Open(player owner)
    {
      if (_entrance == null || _gate == null)
      {
        return;
      }

      _open = true;
      WaygateSetDestination(_entrance, _arriveX, _arriveY);
      WaygateActivate(_entrance, true);
      WaygateSetDestination(_gate, _exitX, _exitY);
      WaygateActivate(_gate, true);
      for (var i = 0; i < _buildings.Count; i++)
      {
        SetUnitOwner(_buildings[i], owner, true);
      }
    }
  }
}
