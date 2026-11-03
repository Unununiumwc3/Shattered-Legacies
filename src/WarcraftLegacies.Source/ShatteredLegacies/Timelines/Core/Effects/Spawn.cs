using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "spawn start" (0.54): a start kit created by code at a spot -- buildings on free build spots around it
  // (AiApi.FindBuildSpot, as for the Kezan Enterprise), units next to it. For successors without a preplaced base.
  internal static class Spawn
  {
    internal static int Buildings(player p, float x, float y, int[] types)
    {
      var usedX = new List<float>();
      var usedY = new List<float>();
      var placed = 0;
      for (var i = 0; i < types.Length; i++)
      {
        if (AiApi.FindBuildSpot(x, y, usedX, usedY, out var sx, out var sy))
        {
          CreateUnit(p, types[i], sx, sy, 270f);
          usedX.Add(sx);
          usedY.Add(sy);
          placed++;
        }
      }

      return placed;
    }

    internal static void Units(player p, float x, float y, int[] types)
    {
      for (var i = 0; i < types.Length; i++)
      {
        CreateUnit(p, types[i], x + 64f * (i % 4), y - 64f * (i / 4), 270f);
      }
    }
  }
}
