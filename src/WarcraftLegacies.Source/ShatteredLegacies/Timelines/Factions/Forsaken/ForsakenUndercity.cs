using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.57: the Undercity as an instance (Core/Effects/Instance): prepared at map setup -- visible and invulnerable,
  // symmetric: the waygate in the middle, the Undercity north of it, Plaguetorium west, Graveyard east, Forbidden
  // Archive south, a Plague Tower in each corner, rocks and sight blockers along the edge. "The Throne of Lordaeron"
  // opens it: a second waygate in the Capital Palace's ruins.
  internal static class ForsakenUndercity
  {
    private const float Side = 560f;   // the buildings at the sides
    private const float Corner = 640f; // the towers in the corners

    internal static Instance Square;

    internal static void Prepare()
    {
      // 0.57.1: a moment after the map setup -- WL's NeutralVictimAndPassiveSetup hides every Neutral Passive unit in
      // Regions.HideUnitBottomLeft (the Undercity's square lies in it), which hid the buildings and the gate
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        Build();
      });
    }

    private static void Build()
    {
      Square = new Instance("Undercity", ForsakenConfig.UndercityMinX, ForsakenConfig.UndercityMinY,
        ForsakenConfig.UndercityMaxX, ForsakenConfig.UndercityMaxY);
      Square.Prepare(
        new[]
        {
          ForsakenObjects.Undercity, ForsakenObjects.Plaguetorium, ForsakenObjects.Graveyard,
          ForsakenObjects.ForbiddenArchive, ForsakenObjects.PlagueTower, ForsakenObjects.PlagueTower,
          ForsakenObjects.PlagueTower, ForsakenObjects.PlagueTower,
        },
        new[] { 0f, -Side, Side, 0f, -Corner, Corner, -Corner, Corner },
        new[] { Side, 0f, 0f, -Side, Corner, Corner, -Corner, -Corner });
    }

    internal static void Open(player p, float x, float y)
    {
      Square?.Open(p, x, y);
    }
  }
}
