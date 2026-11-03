namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // Data of the Forsaken (0.54, Quel'thalas' betrayal, Timelines/SCENARIO_CONCEPTS.md P3 / D3 / D4).
  internal static class ForsakenConfig
  {
    internal const string Name = "Forsaken";

    // start in Northrend near Drak'Tharon (maintainer, 0.54: safe, away from Lordaeron; not lore)
    internal const float StartX = 2968f;
    internal const float StartY = 17655f;

    internal const int SylvanasBanshee = 1431986481; // UZa1 Sylvanas the banshee (hero; copy of Hvwd, banshee model)
    internal const int SylvanasRanger = UNIT_HVWD_RANGER_GENERAL_OF_SILVERMOON_QUELTHALAS; // Quel'thalas' Sylvanas

    // start kit: an altar to rebuild from, builders. Placeholder tech (Scourge basics) until 0.55 brings the 24p
    // Forsaken tree (Content/REUSE.md: Altar of the Queen u010, Acolyte u01K, Plaguetorium u011 ...).
    internal static int[] StartBuildings = { UNIT_UAOD_ALTAR_OF_DARKNESS_SCOURGE_ALTAR };
    internal static int[] StartUnits =
      { UNIT_UACO_ACOLYTE_SCOURGE_WORKER, UNIT_UACO_ACOLYTE_SCOURGE_WORKER, UNIT_UACO_ACOLYTE_SCOURGE_WORKER };
  }
}
