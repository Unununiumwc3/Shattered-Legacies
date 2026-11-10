using System;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

// Offers that share this destination compete for the same faction and arrival site.
internal sealed class ExclusivePath
{
  internal readonly string Name;
  internal readonly Action Prepare;
  internal readonly Action<player, TurningPointDef> Run;
  internal readonly float X;
  internal readonly float Y;
  internal bool Claimed { get; private set; }

  internal ExclusivePath(string name, Action prepare, Action<player, TurningPointDef> run, float x, float y)
  {
    Name = name;
    Prepare = prepare;
    Run = run;
    X = x;
    Y = y;
  }

  internal bool TryClaim()
  {
    if (Claimed)
    {
      return false;
    }

    Claimed = true;
    return true;
  }
}
