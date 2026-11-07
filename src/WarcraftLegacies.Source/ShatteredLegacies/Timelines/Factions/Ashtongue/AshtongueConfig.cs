namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // Data of the Ashtongue Deathsworn (the Illidari's fallback, 0.63; 0.64 the purification reworked -- concepts P16).
  // Copies of the 24p draenei: import_24p.py (AshtongueObjects.g.cs); hand-made objects: ashtongue_objects.py (ids
  // below).
  internal static class AshtongueConfig
  {
    internal const string Name = "Ashtongue";       // "Ashtongue Deathsworn" in texts (the scoreboard cuts long names)
    internal const string FinalName = "Karabor";    // the faction's name once Karabor is restored (0.64)

    // the Black Temple (o00F, WL's Fel Horde capital, the Illidari's HQ) and its CP (n00I) -- the seat (Conquest)
    internal const float TempleX = 7808f;
    internal const float TempleY = -30144f;
    internal const float UnitsX = 7653f;                // open ground next to it (found offline, pathing map)
    internal const float UnitsY = -29564f;
    internal const int TempleOfTheDeathsworn = 1868198960; // oZt0 (hand-made)
    internal const int TempleOfKarabor = 1868198961;       // oZt1
    internal const int PurifyAbility = 1096447024;         // AZt0
    internal const int OutlandSupply = 1868199009;         // oZta: food, one per Outland CP of the team
    internal const int QuestDraenei = 1378891080;          // R05H "Quest Completed: The Draenei" (Akama, the hut)

    // the purification, in its order (maintainer): Shattrath, Halaa, Telredor, Oshu'gun, then the temple itself.
    // Each site: a fel-tainted building (no buttons) replaced by its purified one (trains, researches).
    internal static string[] Steps = { "Shattrath", "Halaa", "Telredor", "Oshu'gun", "Karabor" };
    internal static int[] StepCost = { 2000, 2400, 2400, 2600, 2600 }; // 12,000 (0.63: 6,000 -- "a bit quick")
    internal static int[] Tainted = { 1868198962, 1868198964, 1868198966, 1868198968, TempleOfTheDeathsworn };
    internal static int[] Purified = { 1868198963, 1868198965, 1868198967, 1868198969, TempleOfKarabor };
    internal static int[] StepTech = { 1381659696, 1381659697, 1381659698, 1381659699, 1381659700 }; // RZt0-RZt4
    // where they stand: the preplaced Shattrah (h0AH), Halaar (h0AE), Oshu'gun (h02Z); Telredor at Zangarmarsh
    internal static float[] SiteXY = { -1632f, -28640f, -4576f, -29472f, -2512f, -25920f, -5120f, -30528f };
    internal static float[] BubbleScale = { 0f, 4f, 3.5f, 4.5f, 6f }; // Shattrath has its own shield bubble

    // the donations: Shattrath draws mana from own and allied units within 1500 -- bigger steals, slower, many units
    // at once (maintainer): every 8 s up to 50 from each unit, at most 160 in all -> 20 per second -> 10 minutes with
    // perfect mana; 2 per second by itself (about 100 minutes without help). Shattrath's mana pool shows the step.
    internal const float DonationRange = 1500f;
    internal const float DonationEvery = 8f;
    internal const float DonationPerUnit = 50f;
    internal const float DonationCap = 160f;
    internal const float Trickle = 2f;

    // Ishanah next to Shattrath: cannot move and cannot be hurt until Karabor is restored
    internal const float IshanahX = -1132f;             // found offline, pathing map
    internal const float IshanahY = -28640f;

    // income: gold per minute and food for every Outland CP (Regions.InstanceOutland) held by the Ashtongue's team
    internal const int GoldPerOutlandCp = 10;
    internal const float SupplyX = 21600f;              // the food dummies: the sealed corner (the Hourglasses' area)
    internal const float SupplyY = 23500f;

    internal static int[] StartUnits = { AshtongueObjects.Defender, AshtongueObjects.Defender,
                                         AshtongueObjects.Defender, AshtongueObjects.Defender };
  }
}
