using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines
{
  // Entry point of the timelines (fallbacks, sub paths, betrayals, picks -- Timelines/TIMELINES.md).
  internal static class TimelineSetup
  {
    internal static void Setup()
    {
      TimelineSystem.Setup();
      Factions.TwilightHammer.TwilightStart.Setup(); // 0.89.0: Twilight's Hammer, a start faction on P20
    }
  }
}
