using System.Collections.Generic;
using MacroTools.Instances;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.57, building block "instance": a base of its own, reachable only through a waygate.
  // 0.82 (void-area dungeons): the instance is a dungeon copied from the 24p map into a void part of our map
  // (tools/shattered-legacies/terrain/copy_area.py, the layout in Timelines/VOID_DUNGEONS.md) -- the void around it
  // is its wall, so the ring of rocks and sight blockers of the 0.57 squares is gone. Prepared at map setup, visible
  // and invulnerable:
  //   - the inner waygate where the 24p map had its entrance (closed)
  //   - its buildings at their 24p spots (Neutral Passive)
  //   - registered as a WL instance (MacroTools.Instances): no world-tree immortality, Summon Legion or slipstream
  //     across its border -- like Outland and Argus
  // Open(): a second waygate where the entrance should be (e.g. a ruined capital), both gates open, the buildings go
  // to the owner (they stay invulnerable). First user: the Forsaken's Undercity.
  internal sealed class Instance
  {
    private const int Waygate = 1853319028; // nwgt (stock Waygate)

    internal readonly string Name;
    private readonly Rectangle _area;
    private readonly float _gateX, _gateY;     // the inner waygate
    private readonly float _arriveX, _arriveY; // where the entrance puts you (a step from the inner gate)
    private readonly List<unit> _buildings = new();
    private unit _gate;
    private unit _entrance;
    private float _exitX, _exitY;

    internal Instance(string name, Rectangle area, float gateX, float gateY, float arriveX, float arriveY)
    {
      Name = name;
      _area = area;
      _gateX = gateX;
      _gateY = gateY;
      _arriveX = arriveX;
      _arriveY = arriveY;
      _exitX = gateX;
      _exitY = gateY;
    }

    internal bool IsOpen => _entrance != null;

    internal bool Contains(float x, float y) => _area.Contains(x, y);

    // buildings: unit types and their spots (x, y), facing 270
    internal void Prepare(int[] types, float[] x, float[] y)
    {
      var passive = Player(PLAYER_NEUTRAL_PASSIVE);
      _gate = CreateUnit(passive, Waygate, _gateX, _gateY, 270f);
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

    // the entrance at (x, y) -- both waygates open; the buildings to the owner (they stay invulnerable)
    internal void Open(player owner, float x, float y)
    {
      if (_entrance == null)
      {
        _entrance = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), Waygate, x, y, 270f);
        SetUnitInvulnerable(_entrance, true);
      }

      _exitX = x;
      _exitY = y - 250f;
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
