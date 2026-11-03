namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // Data of the Kezan Enterprise (Kul Tiras' fallback, 0.39.6-0.39.8; was part of FallbackConfig).
  // Start buildings placed at Kezan; all exist in the main map and belonged to no faction: goblin hall o03L (Tinker),
  // Altar of Industry o03O (Gallywix, Tinker hero), Tank Factory o05T (Grenadier, Personal Tank, War Zeppelin), Kezan
  // Oil Supply h04Z (farm) x2; kept from Kul Tiras: Garrison h06R (Marines) and Workshop h07P (Cannons).
  internal static class KezanEnterpriseConfig
  {
    internal const string Name = "Kezan Enterprise"; // 0.39.7: "The ..." was cut off on the scoreboard
    internal const int GallywixType = 1328559182;    // O04N
    internal const int KezanCp = 1848652890;         // n04Z Kezan

    // Regions.KezanAmbient: everything neutral in it joins the Enterprise (0.39.7)
    internal const float AreaMinX = -3200f;
    internal const float AreaMinY = -10368f;
    internal const float AreaMaxX = 736f;
    internal const float AreaMaxY = -5184f;

    internal static int[] BuildingTypes =
      { 1865429836, 1865429839, 1865430356, 1747990098, 1747990352, 1747989594, 1747989594 };

    // units / heroes the Enterprise may train (WL's limits would block types of no faction)
    internal static int[] UnitTypes =
    {
      1865430097, 1865429577, 1747990858, 1747990857, 1747990833, // Tinker, Builder, Grenadier, Personal Tank, War Zeppelin
      1747989553, 1865429313, 1747989583, 1747989591,             // Marine, Cannon, + the Garrison / Workshop extras
    };

    internal static int[] HeroTypes = { 1328559182, 1316252014, 1447571505 }; // O04N Gallywix, Ntin, VH01 Marin
  }
}
