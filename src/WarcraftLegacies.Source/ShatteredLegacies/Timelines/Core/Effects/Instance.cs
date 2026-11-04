using System.Collections.Generic;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.57, building block "instance": a base of its own in one of the unused squares of the south-west corner (about
  // 1900 x 1900 each, over void, inside Regions.HideUnitBottomLeft). Prepared at map setup, visible and invulnerable:
  //   - a ring of rocks along the square's edge, each with a line-of-sight blocker -- units in neighbouring squares
  //     cannot see, and so not shoot, across the void
  //   - a waygate in the middle (closed)
  //   - its buildings, symmetric around the middle (Neutral Passive)
  // Open(): a second waygate where the entrance should be (e.g. a ruined capital), both gates open, the buildings go
  // to the owner. First user: the Forsaken's Undercity.
  internal sealed class Instance
  {
    private const int Waygate = 1853319028;      // nwgt (stock Waygate)
    private const int Rocks = 1280602723;        // LTrc (stock Rock Chunks)
    private const int SightBlocker = 1498704994; // YTlb (stock Line of Sight Blocker)
    private const float RingInset = 70f;         // from the walkable edge
    private const float RingStep = 160f;

    internal readonly string Name;
    internal readonly float CenterX;
    internal readonly float CenterY;
    private readonly float _half;
    private readonly List<unit> _buildings = new();
    private unit _gate;
    private unit _entrance;

    internal Instance(string name, float minX, float minY, float maxX, float maxY)
    {
      Name = name;
      CenterX = (minX + maxX) / 2f;
      CenterY = (minY + maxY) / 2f;
      _half = (maxX - minX) / 2f;
    }

    // buildings: unit types and their offsets from the middle (keep it symmetric)
    internal void Prepare(int[] types, float[] dx, float[] dy)
    {
      var r = _half - RingInset;
      for (var d = -r; d <= r; d += RingStep)
      {
        Edge(CenterX + d, CenterY - r);
        Edge(CenterX + d, CenterY + r);
        Edge(CenterX - r, CenterY + d);
        Edge(CenterX + r, CenterY + d);
      }

      var passive = Player(PLAYER_NEUTRAL_PASSIVE);
      _gate = CreateUnit(passive, Waygate, CenterX, CenterY, 270f);
      SetUnitInvulnerable(_gate, true);
      WaygateActivate(_gate, false);
      for (var i = 0; i < types.Length; i++)
      {
        var u = CreateUnit(passive, types[i], CenterX + dx[i], CenterY + dy[i], 270f);
        SetUnitInvulnerable(u, true);
        _buildings.Add(u);
      }
    }

    private static void Edge(float x, float y)
    {
      var rock = CreateDestructable(Rocks, x, y, GetRandomReal(0f, 360f), 1f, GetRandomInt(0, 2));
      SetDestructableInvulnerable(rock, true);
      CreateDestructable(SightBlocker, x, y, 0f, 1f, 0);
    }

    // the entrance at (x, y) -- both waygates open; the buildings to the owner (they stay invulnerable)
    internal void Open(player owner, float x, float y)
    {
      if (_entrance == null)
      {
        _entrance = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), Waygate, x, y, 270f);
        SetUnitInvulnerable(_entrance, true);
      }

      WaygateSetDestination(_entrance, CenterX, CenterY - 250f);
      WaygateActivate(_entrance, true);
      WaygateSetDestination(_gate, x, y - 250f);
      WaygateActivate(_gate, true);
      for (var i = 0; i < _buildings.Count; i++)
      {
        SetUnitOwner(_buildings[i], owner, true);
      }
    }
  }
}
