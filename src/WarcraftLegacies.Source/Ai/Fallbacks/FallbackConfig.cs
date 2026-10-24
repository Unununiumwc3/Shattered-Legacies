using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.1: fallback scenarios, first slice (design: Ai/FALLBACKS.md). One definition per faction that has a fallback.
  // Mode "wl"    = WL already does it (Theramore, Scarlet Crusade): only the trigger building shows it, no ability.
  // Mode "spawn" = this system does it: when the faction loses its HQ CP (after having held it), a bot falls back at
  //                once; a human gets the "Begin the Fallback" ability on its trigger building and decides when.
  internal sealed class FallbackDef
  {
    public readonly string Faction;     // WL faction name
    public readonly string Name;        // shown on the trigger building
    public readonly string Mode;        // "wl" (WL does it), "spawn" (this system), "none" (building only, not yet)
    public int[] Hq = new int[0];       // HQ building / CP unit types -- ALL must be lost (recaptured -> again)
    public float X;                     // spawn: where the fallback starts
    public float Y;
    public int HeroType = 1447571505;   // spawn: placeholder hero (VH01 Marin Noggenfogger, Baron of Gadgetzan)
    public string Announcement = "";    // spawn: shown to everyone when the fallback happens
    public string Special = "";         // 0.39.5: extra logic ("kultiras")

    public FallbackDef(string faction, string name, string mode)
    {
      Faction = faction;
      Name = name;
      Mode = mode;
    }
  }

  internal static class FallbackConfig
  {
    internal const int TriggerBuildingType = 1848657474; // n0FB "Fallback" (copy of a CP model, invulnerable, no attack)
    internal const int AbilityId = 1095123505;           // AFB1 "Begin the Fallback" (channel, copy of LLLN)

    // Trigger buildings: the sealed top-right corner (Regions.ArtifactDummyInstance -- WL only creates artifact
    // items there). One per player slot: row = slot / 12, column = slot % 12.
    // 0.39.7 (maintainer): 3 columns around x 22208 starting at y 23500 going north; the middle column fills first,
    // then left, then right. Order: team (scoreboard), then member. 150 apart (100 overlapped the building models).
    internal const float CenterX = 22208f;
    internal const float ColumnStep = 150f;
    internal const float FirstRowY = 23500f;
    internal const float RowStep = 150f;
    internal const int RowsPerColumn = 7;
    internal const float CreateAt = 62f;  // after the faction picks (and where the mode vote used to end)
    internal const float CheckEvery = 5f;

    // 0.39.4: every faction gets a trigger building. Good factions: placeholder fallback (Marin Noggenfogger at the
    // planned place, FALLBACKS.md). Evil factions: building only (Mode "none"). HQs: WL's essential legends (AI's pick
    // where WL has several -- ALL of them must be lost; a recaptured HQ makes the fallback unavailable again).
    // 0.39.6: "The Kezan Enterprise" (Kul Tiras' fallback). Start buildings placed at Kezan; all exist in the main map
    // and belonged to no faction: goblin hall o03L (Tinker), Altar of Industry o03O (Gallywix, Tinker hero),
    // Tank Factory o05T (Grenadier, Personal Tank, War Zeppelin), Kezan Oil Supply h04Z (farm) x2; kept from Kul Tiras:
    // Garrison h06R (Marines) and Workshop h07P (Cannons).
    internal const string KezanName = "Kezan Enterprise"; // 0.39.7: "The ..." was cut off on the scoreboard
    internal static readonly int[] KezanBuildings =
      { 1865429836, 1865429839, 1865430356, 1747990098, 1747990352, 1747989594, 1747989594 };
    // units / heroes the Enterprise may train (WL's limits would block types of no faction)
    internal static readonly int[] KezanUnits =
    {
      1865430097, 1865429577, 1747990858, 1747990857, 1747990833, // Tinker, Builder, Grenadier, Personal Tank, War Zeppelin
      1747989553, 1865429313, 1747989583, 1747989591,             // Marine, Cannon, + the Garrison / Workshop extras
    };
    internal const int GallywixType = 1328559182;                   // O04N
    internal static readonly int[] KezanHeroes = { 1328559182, 1316252014, 1447571505 }; // O04N Gallywix, Ntin, VH01 Marin

    internal static readonly List<FallbackDef> All = new()
    {
      new FallbackDef("Dalaran", "Theramore (automatic when the Violet Citadel falls)", "wl"),
      new FallbackDef("Lordaeron", "Scarlet Crusade (WL event)", "wl"),
      new FallbackDef("Quel'thalas", "Sunfury (placeholder)", "spawn")
      {
        Hq = new[] { 1747988531, 1848651825 }, // h003 Silvermoon Palace, n001 the Sunwell
        X = 3114f, Y = -23074f,                // Netherstorm (Sunfury's start)
        Announcement = "|cffffcc00Quel'thalas is lost.|r Prince Kael'thas gathers the last of his people in Outland.",
      },
      new FallbackDef("Gilneas", "Gilneans on Darkshore (placeholder)", "spawn")
      {
        Hq = new[] { 1747989577 },             // h04I Gilneas Castle
        X = -17238f, Y = 6446f,                // Darkshore
        Announcement = "|cffffcc00Gilneas has fallen.|r The night elves' ships carry the survivors to the shores of Darkshore.",
      },
      new FallbackDef("Stormwind", "Valiance Expedition (placeholder)", "spawn")
      {
        Hq = new[] { 1747988568 },             // h00X Stormwind Keep
        X = -7382f, Y = 15342f,                // Borean Tundra
        Announcement = "|cffffcc00Stormwind has fallen.|r Bolvar Fordragon leads the Valiance Expedition north, to Northrend.",
      },
      new FallbackDef("Ironforge", "Explorers' League (placeholder)", "spawn")
      {
        Hq = new[] { 1747988529 },             // h001 the Great Forge (was the Ironforge CP in 0.39.1)
        X = 3468f, Y = 22662f,                 // Storm Peaks
        Announcement = "|cffffcc00Ironforge has fallen.|r The Explorers' League digs for hope in the Storm Peaks.",
      },
      // 0.39.5 (maintainer): Kul Tiras keeps Katherine and becomes a mercenary faction on Kezan (special logic in
      // FallbackSystem.KulTirasFallback: old units -> Stormwind, Kezan + the creeps there -> Kul Tiras, no allies).
      new FallbackDef("Kul'tiras", "Kezan Enterprise", "spawn")
      {
        Hq = new[] { 1747989558 },             // h046 Boralus Keep
        X = -662f, Y = -7410f,                 // Kezan
        HeroType = 0,                          // no placeholder hero: Katherine is moved there
        Special = "kultiras",
        Announcement = "|cffffcc00The Proudmoore dynasty is shattered.|r Driven to piracy by tragedy, Katherine rallies the "
                       + "survivors under the skull and crossbones -- while the opportunistic Goblins of Kezan join the global "
                       + "conflict for gold and glory!",
      },
      new FallbackDef("Druids", "Dream trees (placeholder)", "spawn")
      {
        Hq = new[] { 1848651826, 1848653380 }, // n002 Nordrassil, n06D Cenarion Hold
        X = 12522f, Y = -16722f,               // Duskwood (Twilight Grove)
        Announcement = "|cffffcc00Nordrassil is lost.|r The druids walk the Emerald Dream to its great trees in the Eastern Kingdoms.",
      },
      new FallbackDef("Sentinels", "Maiev's Watchers (placeholder)", "spawn")
      {
        Hq = new[] { 1697656906, 1697656909 }, // e00J Auberdine, e00M Feathermoon Stronghold
        X = 5802f, Y = -31282f,                // Warden's Cage (Outland)
        Announcement = "|cffffcc00The Sentinels are broken.|r Maiev Shadowsong leads her Watchers into Outland.",
      },
      new FallbackDef("The Exodar", "Draenei of Shattrath (placeholder)", "spawn")
      {
        Hq = new[] { 1697657176 },             // e01X Exodar Regalis
        X = -982f, Y = -29010f,                // Shattrath
        Announcement = "|cffffcc00The Exodar has fallen.|r Its people return to Outland -- the Naaru welcome them in Shattrath.",
      },
      new FallbackDef("Frostwolf", "Dragonmaw (placeholder)", "spawn")
      {
        Hq = new[] { 1865429066, 1865429572 }, // o00J Thunder Bluff, o02D Darkspear Hold
        X = 14442f, Y = -2578f,                // Wetlands, next to Grim Batol
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      new FallbackDef("Warsong", "Dragonmaw (placeholder)", "spawn")
      {
        Hq = new[] { 1865429044, 1865429314 }, // o004 Stonemaul Keep, o01B Orgrimmar
        X = 14442f, Y = -2578f,
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      // evil factions: trigger building only for now (no fallback ability yet)
      new FallbackDef("Scourge", "Forsaken (not yet)", "none") { Hq = new[] { 1966092336 } },                       // u000 Frozen Throne
      new FallbackDef("Legion", "Knights of the Ebon Blade (not yet)", "none") { Hq = new[] { 1966092359 } },       // u00G Legion Stronghold
      new FallbackDef("Fel Horde", "Mag'har (not yet)", "none") { Hq = new[] { 1865429303, 1865429062, 1865429048 } }, // o017, o00F, o008
      new FallbackDef("Illidan", "Demon hunters (not yet)", "none"),                                                // no essential legend in WL
      new FallbackDef("Skywall", "Ramkahen (not yet)", "none") { Hq = new[] { 1852140643 } },                       // nelc Vortex Pinnacle
      new FallbackDef("Black Empire", "Titan Watchers (not yet)", "none") { Hq = new[] { 1429221722 } },            // U01Z N'Zoth
      new FallbackDef("Ahn'qiraj", "Brood of Nozdormu (not yet)", "none") { Hq = new[] { 1429221458 } },            // U00R C'Thun
    };
  }
}
