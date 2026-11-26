namespace WarcraftLegacies.Source.Factions.Dalaran.Mechanics;

/// <summary>
/// One of Dalran's two gates.
/// </summary>
public sealed class Waygate
{
  public Waygate(unit whichUnit)
  {
    Unit = whichUnit;
  }

  public unit Unit { get; }

  public bool IsConstructed { get; private set; }

  public Waygate? Sister { get; set; }

  // [SL] (0.81.0): no property pattern ("Sister is { IsConstructed: true }") -- CSharp.lua indexed the missing sister
  // ("attempt to index a nil value (local 'ref')") as soon as ONE Way Gate stood: the bots' portal rebuild died there
  // every 30 s and took the ferries, the Legion's teleporters and the Dark Portal edges with it.
  public bool IsOperational =>
    IsConstructed &&
    Unit.Alive &&
    Sister != null &&
    Sister.IsConstructed &&
    Sister.Unit.Alive;

  public void MarkConstructed()
  {
    IsConstructed = true;
  }
}
