namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // Who runs a turning point (was the Mode string of FallbackDef: "wl" / "spawn" / "none").
  internal enum TurningPointSource
  {
    Wl,       // WL already does it (Theramore, Scarlet Crusade): the Hourglass only names it
    Timeline, // the timeline system runs it: HQ lost -> bots at once, humans through the Hourglass
    NotYet,   // planned: the Hourglass names it, nothing runs yet
  }
}
