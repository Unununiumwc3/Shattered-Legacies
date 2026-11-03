using System.Collections.Generic;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Kultiras;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines
{
  // One turning point per WL faction (was FallbackConfig.All, 0.39.1-0.39.7). These are the PLACEHOLDERS of the
  // first slice: most of them are superseded by the design in Timelines/SCENARIO_CONCEPTS.md (noted per entry) and
  // get replaced faction by faction (TODO.md). HQs: WL's essential legends (0.39.4; where WL has several, ALL of them
  // must be lost; a recaptured HQ makes the turning point unavailable again). Good factions: placeholder fallback
  // (Marin Noggenfogger at the planned place); evil factions: Hourglass only (NotYet).
  internal static class TimelineCatalog
  {
    internal static List<TurningPointDef> All = new()
    {
      new TurningPointDef("Dalaran", "Theramore (automatic when the Violet Citadel falls)", TurningPointKind.Fallback,
        TurningPointSource.Wl),
      new TurningPointDef("Lordaeron", "Scarlet Crusade (story event)", TurningPointKind.Fallback, TurningPointSource.Wl),
      // concepts: Sunfury = WL's Sunfury faction (no longer an Illidari pick), Forsaken = Quel'thalas' betrayal (D3)
      new TurningPointDef("Quel'thalas", "Sunfury (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1747988531, 1848651825 }, // h003 Silvermoon Palace, n001 the Sunwell
        X = 3114f, Y = -23074f,                // Netherstorm (Sunfury's start)
        Announcement = "|cffffcc00Quel'thalas is lost.|r Prince Kael'thas gathers the last of his people in Outland.",
      },
      // superseded: Gilneas takes Dalaran's Theramore / Jaina's Sanctum path (concepts, P8)
      new TurningPointDef("Gilneas", "Gilneans on Darkshore (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Hq = new[] { 1747989577 },             // h04I Gilneas Castle
        X = -17238f, Y = 6446f,                // Darkshore
        Announcement = "|cffffcc00Gilneas has fallen.|r The night elves' ships carry the survivors to the shores of Darkshore.",
      },
      // concepts: Valiance Expedition, Bolvar -> Lich King (P2)
      new TurningPointDef("Stormwind", "Valiance Expedition (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Hq = new[] { 1747988568 },             // h00X Stormwind Keep
        X = -7382f, Y = 15342f,                // Borean Tundra
        Announcement = "|cffffcc00Stormwind has fallen.|r Bolvar Fordragon leads the Valiance Expedition north, to Northrend.",
      },
      // superseded: Dwarven Expedition (name open) -- base near the Storm Peaks, frost dwarves, a quest at Ulduar (P5)
      new TurningPointDef("Ironforge", "Explorers' League (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Hq = new[] { 1747988529 },             // h001 the Great Forge (was the Ironforge CP in 0.39.1)
        X = 3468f, Y = 22662f,                 // Storm Peaks
        Announcement = "|cffffcc00Ironforge has fallen.|r The Explorers' League digs for hope in the Storm Peaks.",
      },
      // 0.39.5 (maintainer): Kul Tiras keeps Katherine and becomes a mercenary faction on Kezan
      // (Timelines/Factions/Kultiras/KultirasFallback.cs). To be rebuilt on WL's mechanics (TODO.md, step 3).
      new TurningPointDef("Kul'tiras", "Kezan Enterprise", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1747989558 },             // h046 Boralus Keep
        X = -662f, Y = -7410f,                 // Kezan
        HeroType = 0,                          // no placeholder hero: Katherine is moved there
        Run = (p, def) => KultirasFallback.Run(p, def),
        Announcement = "|cffffcc00The Proudmoore dynasty is shattered.|r Driven to piracy by tragedy, Katherine rallies the "
                       + "survivors under the skull and crossbones -- while the opportunistic Goblins of Kezan join the global "
                       + "conflict for gold and glory!",
      },
      // concepts: Dream trees -> South Alliance, trigger = the World Tree consumed / Nordrassil falls (P12)
      new TurningPointDef("Druids", "Dream trees (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1848651826, 1848653380 }, // n002 Nordrassil, n06D Cenarion Hold
        X = 12522f, Y = -16722f,               // Duskwood (Twilight Grove)
        Announcement = "|cffffcc00Nordrassil is lost.|r The druids walk the Emerald Dream to its great trees in the Eastern Kingdoms.",
      },
      // concepts: Maiev's Watchers for both picks of the slot (P19)
      new TurningPointDef("Sentinels", "Maiev's Watchers (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Hq = new[] { 1697656906, 1697656909 }, // e00J Auberdine, e00M Feathermoon Stronghold
        X = 5802f, Y = -31282f,                // Warden's Cage (Outland)
        Announcement = "|cffffcc00The Sentinels are broken.|r Maiev Shadowsong leads her Watchers into Outland.",
      },
      // superseded: the Draenei pick takes Maiev's Watchers too (P19)
      new TurningPointDef("The Exodar", "Draenei of Shattrath (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Hq = new[] { 1697657176 },             // e01X Exodar Regalis
        X = -982f, Y = -29010f,                // Shattrath
        Announcement = "|cffffcc00The Exodar has fallen.|r Its people return to Outland -- the Naaru welcome them in Shattrath.",
      },
      // superseded: Twilight's Hammer (Grim Batol, reusing the Dragonmaw assets and the 24p tech trees) (P1)
      new TurningPointDef("Frostwolf", "Dragonmaw (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1865429066, 1865429572 }, // o00J Thunder Bluff, o02D Darkspear Hold
        X = 14442f, Y = -2578f,                // Wetlands, next to Grim Batol
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      new TurningPointDef("Warsong", "Dragonmaw (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1865429044, 1865429314 }, // o004 Stonemaul Keep, o01B Orgrimmar
        X = 14442f, Y = -2578f,
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      // evil factions: Hourglass only for now. The names are the old plan; concepts: Scourge -> Knights of the Ebon
      // Blade, Legion -> Dreadlord Insurgents, Illidari -> Akama's Ashtongue (draenei remnant), Skywall / Black
      // Empire -> open, Ahn'qiraj -> Brood of Nozdormu.
      new TurningPointDef("Scourge", "Forsaken (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet)
        { Hq = new[] { 1966092336 } },                                   // u000 Frozen Throne
      new TurningPointDef("Legion", "Knights of the Ebon Blade (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet) { Hq = new[] { 1966092359 } },        // u00G Legion Stronghold
      new TurningPointDef("Fel Horde", "Mag'har (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet)
        { Hq = new[] { 1865429303, 1865429062, 1865429048 } },           // o017, o00F, o008
      new TurningPointDef("Illidan", "Demon hunters (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet),
      new TurningPointDef("Skywall", "Ramkahen (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet)
        { Hq = new[] { 1852140643 } },                                   // nelc Vortex Pinnacle
      new TurningPointDef("Black Empire", "Titan Watchers (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet) { Hq = new[] { 1429221722 } },        // U01Z N'Zoth
      new TurningPointDef("Ahn'qiraj", "Brood of Nozdormu (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet) { Hq = new[] { 1429221458 } },        // U00R C'Thun
    };

    // the definition for a WL faction name, null if it has none
    internal static TurningPointDef Find(string factionName)
    {
      TurningPointDef found = null;
      for (var i = 0; i < All.Count; i++)
      {
        if (All[i].Faction == factionName)
        {
          found = All[i];
        }
      }

      return found;
    }
  }
}
