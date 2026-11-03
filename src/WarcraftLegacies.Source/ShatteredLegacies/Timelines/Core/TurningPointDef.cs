namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // One turning point of one WL faction (was FallbackDef). The catalog of all definitions is TimelineCatalog.
  internal sealed class TurningPointDef
  {
    internal const int PlaceholderHeroType = 1447571505; // VH01 Marin Noggenfogger, Baron of Gadgetzan

    public readonly string Faction;                // WL faction name
    public readonly string Name;                   // shown on the Hourglass
    public readonly TurningPointKind Kind;
    public readonly TurningPointSource Source;
    public int[] Hq = new int[0];                  // HQ building / CP unit types -- ALL must be lost (recaptured -> again)
    public float X;                                // where the new start is
    public float Y;
    public int HeroType = PlaceholderHeroType;     // placeholder hero created at (X, Y); 0 = none
    public string Announcement = "";               // shown to everyone when the turning point happens
    public System.Action<player, TurningPointDef> Run; // faction-specific logic (Timelines/Factions/<X>/); null = none

    public TurningPointDef(string faction, string name, TurningPointKind kind, TurningPointSource source)
    {
      Faction = faction;
      Name = name;
      Kind = kind;
      Source = source;
    }
  }
}
