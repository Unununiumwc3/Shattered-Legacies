using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.57: the Undercity as an instance (Core/Effects/Instance): prepared at map setup -- visible and invulnerable.
  // "The Throne of Lordaeron" opens it: a second waygate in the Capital Palace's ruins.
  // 0.82 (void-area dungeons): no longer a bare square in the south-west corner -- the 24p map's Undercity, terrain
  // and doodads, copied into the north-west corner of the void (ForsakenConfig). The buildings stand where the 24p
  // map preplaced them (24p y + UndercityShiftY); the 24p army and its three entrance portals are not taken over. The
  // inner gate stands at the 24p southern entrance ("Undercity Interior 1").
  // 0.83: its control point (DungeonObjects.Undercity, 20 gold, preplaced Neutral Hostile -- out of reach while the
  // gates are shut) goes to the Forsaken with the buildings.
  internal static class ForsakenUndercity
  {
    private const float Shift = ForsakenConfig.UndercityShiftY;

    // the 24p spots (x, 24p y)
    private static readonly int[] Types =
    {
      ForsakenObjects.Undercity, ForsakenObjects.RoyalSepulcrum, ForsakenObjects.Plaguetorium,
      ForsakenObjects.ForbiddenArchive, ForsakenObjects.Graveyard, ForsakenObjects.Laboratory,
      ForsakenObjects.Grave, ForsakenObjects.Grave, ForsakenObjects.Grave, ForsakenObjects.Grave,
      ForsakenObjects.PlagueTower, ForsakenObjects.PlagueTower, ForsakenObjects.PlagueTower,
      ForsakenObjects.PlagueTower, ForsakenObjects.PlagueTower,
    };

    private static readonly float[] X =
    {
      -27136f, -26016f, -27936f,
      -28256f, -26016f, -26784f,
      -26176f, -26048f, -28288f, -28416f,
      -27584f, -26688f, -27584f,
      -26688f, -27776f,
    };

    private static readonly float[] Y =
    {
      16192f, 16416f, 15392f,
      16544f, 16928f, 15328f,
      15616f, 15680f, 16896f, 16832f,
      15808f, 15808f, 16576f,
      16576f, 14656f,
    };

    // 0.84 (maintainer): the inner gate is the 24p's iron gate at the southern entrance (its doodad is left out of the
    // copy, the waygate carries its model: DungeonObjects.UndercityGate)
    private const float GateX = -26537f;
    private const float GateY = 21323f;
    private const float ArriveX = -26544f; // the middle of 24p "Undercity Interior 1"
    private const float ArriveY = 14656f + Shift;

    internal static Instance Square;

    internal static void Prepare()
    {
      // 0.57.1: a moment after the map setup -- WL's NeutralVictimAndPassiveSetup hides Neutral Passive units in
      // Regions.HideUnitBottomLeft (the 0.57 square lay in it; 0.82's Undercity does not -- kept as a safe order)
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        Build();
      });
    }

    private static void Build()
    {
      Square = new Instance("Undercity",
        new Rectangle(ForsakenConfig.UndercityMinX, ForsakenConfig.UndercityMinY, ForsakenConfig.UndercityMaxX,
          ForsakenConfig.UndercityMaxY), DungeonObjects.UndercityGate, GateX, GateY, ArriveX, ArriveY);
      var y = new float[Y.Length];
      for (var i = 0; i < Y.Length; i++)
      {
        y[i] = Y[i] + Shift;
      }

      Square.Prepare(Types, X, y);
    }

    internal static void Open(player p, float x, float y)
    {
      Square?.Open(p, x, y);
      var cp = AiApi.SafeCp(DungeonObjects.Undercity);
      if (cp != null && cp.Unit != null && GetOwningPlayer(cp.Unit) != p)
      {
        AiApi.GiveUnit(cp.Unit, p);
      }
    }
  }
}
