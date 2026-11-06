namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // Data of the Ashtongue Deathsworn (the Illidari's fallback, 0.63 -- concepts P16). Copies of the 24p draenei:
  // import_24p.py (AshtongueObjects.g.cs); hand-made objects: ashtongue_objects.py (ids below).
  internal static class AshtongueConfig
  {
    internal const string Name = "Ashtongue"; // "Ashtongue Deathsworn" in texts (the scoreboard cuts long names)

    // the Black Temple (o00F, WL's Fel Horde capital, the Illidari's HQ) and its CP (n00I) -- the seat (Conquest)
    internal const float TempleX = 7808f;
    internal const float TempleY = -30144f;
    internal const float UnitsX = 7653f;                // open ground next to it (found offline, pathing map)
    internal const float UnitsY = -29564f;
    internal const int TempleOfTheDeathsworn = 1868198960; // oZt0 (hand-made)
    internal const int TempleOfKarabor = 1868198961;       // oZt1 (hand-made)
    internal const int PurifyAbility = 1096447024;         // AZt0 (hand-made)

    // the purification sites, in their order (Telredor is placed by code: the 24p's, at Zangarmarsh)
    internal static string[] Sites = { "Telredor", "Halaar", "Shattrath", "Oshu'gun" };
    internal static int[] SiteCost = { 1000, 1250, 1750, 2000 }; // 6000 in all
    internal static int[] SiteTech = { 1381659696, 1381659697, 1381659698, 1381659699 }; // RZt0-RZt3 (hand-made)
    internal const int KaraborTech = 1381659700;                                         // RZt4
    internal const float TelredorX = -2512f;            // Regions.TelredorUnlock's centre
    internal const float TelredorY = -25920f;

    // Ishanah at Shattrath (rooted until Karabor is restored), Purify: up to 10 mana per second from each own / allied
    // unit within 1500, at most 20 per second in all -> 6000 in 5 minutes when mana is fed perfectly (maintainer);
    // plus 2 per second by herself, so a game without help ends too (about 50 minutes)
    internal const float IshanahX = -1132f;             // next to Shattrath (found offline, pathing map)
    internal const float IshanahY = -28640f;
    internal const float PurifyRange = 1500f;
    internal const float DrainPerUnit = 10f;
    internal const float DrainCap = 20f;
    internal const float PurifyTrickle = 2f;
    internal const float IshanahReviveAfter = 60f;

    // income: gold per minute for every Outland CP (Regions.InstanceOutland) held by the Ashtongue's team
    internal const int GoldPerOutlandCp = 10;

    // the start kit next to the temple (Akama is kept, revived or created)
    internal static int[] StartUnits = { AshtongueObjects.Defender, AshtongueObjects.Defender,
                                         AshtongueObjects.Defender, AshtongueObjects.Defender };
  }
}
