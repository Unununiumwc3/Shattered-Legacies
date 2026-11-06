namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // One turning point of one WL faction (0.39.1 FallbackDef). The catalog of all definitions is TimelineCatalog.
  // 0.53: a turning point run by the timelines is a WL quest (TurningPointQuest): objectives = the trigger (HQ lost)
  // + its decision research at the Hourglass of Fate; completing it runs the effects.
  internal sealed class TurningPointDef
  {
    internal const int PlaceholderHeroType = 1447571505; // VH01 Marin Noggenfogger, Baron of Gadgetzan

    public readonly string Faction;                // WL faction name
    public readonly string Name;                   // the successor, e.g. "Sunfury" (quest title "Fallback: <Name>")
    public readonly TurningPointKind Kind;
    public readonly TurningPointSource Source;
    public int[] Hq = new int[0];                  // HQ building / CP unit types -- ALL must be lost (0.62: then for good)
    public string Trigger = "";                    // the trigger as quest objective text ("Lose Silvermoon Palace ...")
    public string Description = "";                // quest text
    public int Decision;                           // decision research at the Hourglass (HourglassData); 0 = none
    public int Condition;                          // condition tech the decision requires (granted by the trigger)
    public int WlResearch;                         // WL's own research for this turning point, listed on the Hourglass
    public float X;                                // where the new start is (ping, placeholder hero)
    public float Y;
    public int HeroType = PlaceholderHeroType;     // placeholder hero created at (X, Y); 0 = none
    public string Announcement = "";               // shown to everyone when the turning point happens
    public System.Action<player, TurningPointDef> Run; // faction-specific effects (Timelines/Factions/<X>/); null = none
    public System.Action Prepare;                  // run once at map setup (e.g. hide the new start area); null = none
    // 0.54: the trigger as any WL quest objective (with ResearchId = Condition); null = ObjectiveHqLost from Hq
    public System.Func<MacroTools.Quests.Objective> MakeTrigger;
    // 0.54: effects that happen when the trigger fires, whatever is decided (e.g. Sylvanas is lost); null = none
    public System.Action<player, TurningPointDef> OnTrigger;

    public TurningPointDef(string faction, string name, TurningPointKind kind, TurningPointSource source)
    {
      Faction = faction;
      Name = name;
      Kind = kind;
      Source = source;
    }

    // run by the timelines (quest + Hourglass decision)
    internal bool IsQuest => Source == TurningPointSource.Timeline && Decision != 0;

    internal bool HasHourglass => Decision != 0 || WlResearch != 0;

    // quest title: "Fallback: Sunfury", "Betrayal: Forsaken" ...
    internal string Title => (Kind == TurningPointKind.Betrayal ? "Betrayal: "
      : Kind == TurningPointKind.SubPath ? "Path: " : Kind == TurningPointKind.Pick ? "Pick: " : "Fallback: ") + Name;
  }
}
