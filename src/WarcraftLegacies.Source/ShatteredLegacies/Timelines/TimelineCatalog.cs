using System.Collections.Generic;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Kultiras;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Quelthalas;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines
{
  // One turning point per WL faction (was FallbackConfig.All, 0.39.1-0.39.7). Real content: Quel'thalas -> Sunfury
  // (0.53), Quel'thalas -> Forsaken (0.54) and Kul Tiras -> Kezan Enterprise (0.39.5, rebuilt 0.58); the others are PLACEHOLDERS of the first slice, most of them
  // superseded by Timelines/SCENARIO_CONCEPTS.md (noted per entry), replaced faction by faction (TODO.md). HQs: WL's
  // essential legends (0.39.4; ALL of them must be lost; a recaptured HQ makes the turning point unavailable again).
  // Good factions: placeholder fallback (Marin Noggenfogger at the planned place); evil factions: shown on the
  // Hourglass only (NotYet). 0.53: Decision / Condition = the Hourglass researches (Core/HourglassData.g.cs).
  internal static class TimelineCatalog
  {
    internal static List<TurningPointDef> All = new()
    {
      // WL runs it (automatic when the Violet Citadel falls) -- no Hourglass
      new TurningPointDef("Dalaran", "Theramore", TurningPointKind.Fallback, TurningPointSource.Wl),
      // WL runs it (research at Tyr's Hand); the Hourglass lists WL's research too
      new TurningPointDef("Lordaeron", "Scarlet Crusade", TurningPointKind.Fallback, TurningPointSource.Wl)
        { WlResearch = 1378900058 },          // R0XZ The Scarlet Crusade
      // 0.53: WL's Sunfury faction as Quel'thalas' fallback (no longer an Illidari pick); Forsaken = the betrayal (D3)
      new TurningPointDef("Quel'thalas", "Sunfury", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1747988531 },             // h003 Silvermoon Palace (0.53, maintainer: enough on its own)
        Trigger = "Silvermoon Palace is lost",
        Description = "If Quel'thalas falls, Prince Kael'thas leads the last of his people to Netherstorm. The survivors "
                      + "start again as the Sunfury.",
        Decision = HourglassData.DecisionQuelthalas, Condition = HourglassData.ConditionQuelthalas,
        X = 2752f, Y = -22736f,                // Regions.SunfuryStartingPosition (centre)
        HeroType = 0,                          // Kael'thas leads them (QuelthalasFallback)
        Run = (p, def) => QuelthalasFallback.Run(p, def),
        Prepare = () => QuelthalasFallback.Prepare(),
        Announcement = "|cffffcc00Quel'thalas is lost.|r Prince Kael'thas gathers the last of his people in Outland -- "
                       + "the Sunfury rise in Netherstorm.",
      },
      // 0.54, the pilot of a BETRAYAL with a split (concepts D3): Sylvanas slain by the Scourge -> the player may become the
      // Forsaken; Quel'thalas goes on in a free slot, played by a bot. Bots always choose it.
      new TurningPointDef("Quel'thalas", "Forsaken", TurningPointKind.Betrayal, TurningPointSource.Timeline)
      {
        MakeTrigger = () => new ObjectiveLegendKilledBy(AllLegends.Quel.Sylvanas, "Scourge",
          "Sylvanas Windrunner is slain by the Scourge") { ResearchId = HourglassData.ConditionForsaken },
        Description = "If the Scourge slays Sylvanas, she is lost to Quel'thalas for good. Her spirit may rise as a "
                      + "banshee and break away: you play the Forsaken, enslaved to the Scourge, while Quel'thalas fights "
                      + "on under another commander.",
        Decision = HourglassData.DecisionForsaken, Condition = HourglassData.ConditionForsaken,
        X = ForsakenConfig.StartX, Y = ForsakenConfig.StartY,
        HeroType = 0,                          // Sylvanas the banshee leads them (ForsakenBetrayal)
        OnTrigger = (p, def) => ForsakenBetrayal.OnSylvanasSlain(p),
        Prepare = () => ForsakenBetrayal.Prepare(),
        Run = (p, def) => ForsakenBetrayal.Run(p, def),
        Announcement = "|cffffcc00Sylvanas has risen as a banshee.|r In the frozen north the Forsaken gather -- slaves of "
                       + "the Lich King, for now.",
      },
      // superseded: Gilneas takes Dalaran's Theramore / Jaina's Sanctum path (concepts, P8)
      new TurningPointDef("Gilneas", "Gilneans on Darkshore (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Trigger = "Gilneas Castle is lost",
        Description = "Placeholder: a hero appears where the Gilneans would start again.",
        Decision = HourglassData.DecisionGilneas, Condition = HourglassData.ConditionGilneas,
        Hq = new[] { 1747989577 },             // h04I Gilneas Castle
        X = -17238f, Y = 6446f,                // Darkshore
        Announcement = "|cffffcc00Gilneas has fallen.|r The night elves' ships carry the survivors to the shores of Darkshore.",
      },
      // concepts: Valiance Expedition, Bolvar -> Lich King (P2)
      new TurningPointDef("Stormwind", "Valiance Expedition (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Trigger = "Stormwind Keep is lost",
        Description = "Placeholder: a hero appears where the Valiance Expedition would land in Northrend.",
        Decision = HourglassData.DecisionStormwind, Condition = HourglassData.ConditionStormwind,
        Hq = new[] { 1747988568 },             // h00X Stormwind Keep
        X = -7382f, Y = 15342f,                // Borean Tundra
        Announcement = "|cffffcc00Stormwind has fallen.|r Bolvar Fordragon leads the Valiance Expedition north, to Northrend.",
      },
      // superseded: Dwarven Expedition (name open) -- base near the Storm Peaks, frost dwarves, a quest at Ulduar (P5)
      new TurningPointDef("Ironforge", "Explorers' League (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Trigger = "The Great Forge is lost",
        Description = "Placeholder: a hero appears where the dwarves would start again in the Storm Peaks.",
        Decision = HourglassData.DecisionIronforge, Condition = HourglassData.ConditionIronforge,
        Hq = new[] { 1747988529 },             // h001 the Great Forge (was the Ironforge CP in 0.39.1)
        X = 3468f, Y = 22662f,                 // Storm Peaks
        Announcement = "|cffffcc00Ironforge has fallen.|r The Explorers' League digs for hope in the Storm Peaks.",
      },
      // 0.39.5 (maintainer): Kul Tiras keeps Katherine and becomes a mercenary faction on Kezan. 0.58: rebuilt -- a
      // faction of its own (KezanEnterpriseFaction, the 24p goblin tree), the base on Kezan hidden until then
      // (Timelines/Factions/Kultiras/KultirasFallback.cs).
      new TurningPointDef("Kul'tiras", "Kezan Enterprise", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "Boralus Keep is lost",
        Description = "If Boralus falls, Katherine Proudmoore rallies the survivors on Kezan and the goblins join her: "
                      + "the Kezan Enterprise, on its own team.",
        Decision = HourglassData.DecisionKultiras, Condition = HourglassData.ConditionKultiras,
        Hq = new[] { 1747989558 },             // h046 Boralus Keep
        X = KezanEnterpriseConfig.StartX, Y = KezanEnterpriseConfig.StartY, // Kezan (maintainer: the right spot)
        HeroType = 0,                          // no placeholder hero: Katherine leads them
        Run = (p, def) => KultirasFallback.Run(p, def),
        Prepare = () => KultirasFallback.Prepare(),
        Announcement = "|cffffcc00The Proudmoore dynasty is shattered.|r Driven to piracy by tragedy, Katherine rallies the "
                       + "survivors under the skull and crossbones -- while the opportunistic Goblins of Kezan join the global "
                       + "conflict for gold and glory!",
      },
      // concepts: Dream trees -> South Alliance, trigger = the World Tree consumed / Nordrassil falls (P12)
      new TurningPointDef("Druids", "Dream trees (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "Nordrassil and Cenarion Hold are lost",
        Description = "Placeholder: a hero appears where the druids would start again in the Eastern Kingdoms.",
        Decision = HourglassData.DecisionDruids, Condition = HourglassData.ConditionDruids,
        Hq = new[] { 1848651826, 1848653380 }, // n002 Nordrassil, n06D Cenarion Hold
        X = 12522f, Y = -16722f,               // Duskwood (Twilight Grove)
        Announcement = "|cffffcc00Nordrassil is lost.|r The druids walk the Emerald Dream to its great trees in the Eastern Kingdoms.",
      },
      // concepts: Maiev's Watchers for both picks of the slot (P19)
      new TurningPointDef("Sentinels", "Maiev's Watchers (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Trigger = "Auberdine and Feathermoon Stronghold are lost",
        Description = "Placeholder: a hero appears where Maiev's Watchers would start in Outland.",
        Decision = HourglassData.DecisionSentinels, Condition = HourglassData.ConditionSentinels,
        Hq = new[] { 1697656906, 1697656909 }, // e00J Auberdine, e00M Feathermoon Stronghold
        X = 5802f, Y = -31282f,                // Warden's Cage (Outland)
        Announcement = "|cffffcc00The Sentinels are broken.|r Maiev Shadowsong leads her Watchers into Outland.",
      },
      // superseded: the Draenei pick takes Maiev's Watchers too (P19)
      new TurningPointDef("The Exodar", "Draenei of Shattrath (placeholder)", TurningPointKind.Fallback,
        TurningPointSource.Timeline)
      {
        Trigger = "Exodar Regalis is lost",
        Description = "Placeholder: a hero appears where the draenei would start again.",
        Decision = HourglassData.DecisionExodar, Condition = HourglassData.ConditionExodar,
        Hq = new[] { 1697657176 },             // e01X Exodar Regalis
        X = -982f, Y = -29010f,                // Shattrath
        Announcement = "|cffffcc00The Exodar has fallen.|r Its people return to Outland -- the Naaru welcome them in Shattrath.",
      },
      // superseded: Twilight's Hammer (Grim Batol, reusing the Dragonmaw assets and the 24p tech trees) (P1)
      new TurningPointDef("Frostwolf", "Dragonmaw (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "Thunder Bluff and Darkspear Hold are lost",
        Description = "Placeholder: a hero appears where the orcs would start again.",
        Decision = HourglassData.DecisionFrostwolf, Condition = HourglassData.ConditionFrostwolf,
        Hq = new[] { 1865429066, 1865429572 }, // o00J Thunder Bluff, o02D Darkspear Hold
        X = 14442f, Y = -2578f,                // Wetlands, next to Grim Batol
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      new TurningPointDef("Warsong", "Dragonmaw (placeholder)", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "Stonemaul Keep and Orgrimmar are lost",
        Description = "Placeholder: a hero appears where the orcs would start again.",
        Decision = HourglassData.DecisionWarsong, Condition = HourglassData.ConditionWarsong,
        Hq = new[] { 1865429044, 1865429314 }, // o004 Stonemaul Keep, o01B Orgrimmar
        X = 14442f, Y = -2578f,
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r The Dragonmaw rise in Grim Batol.",
      },
      // evil factions: shown on the Hourglass only (greyed out, "Not available yet"), names from the concepts
      new TurningPointDef("Scourge", "Knights of the Ebon Blade (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionScourge, Condition = HourglassData.ConditionScourge,
        Hq = new[] { 1966092336 },             // u000 Frozen Throne
      },
      new TurningPointDef("Legion", "Dreadlord Insurgents (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionLegion, Condition = HourglassData.ConditionLegion,
        Hq = new[] { 1966092359 },             // u00G Legion Stronghold
      },
      new TurningPointDef("Fel Horde", "Mag'har (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionFelHorde, Condition = HourglassData.ConditionFelHorde,
        Hq = new[] { 1865429303, 1865429062, 1865429048 }, // o017, o00F, o008
      },
      new TurningPointDef("Illidan", "Akama's Ashtongue (not yet)", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionIllidari, Condition = HourglassData.ConditionIllidari,
      },
      new TurningPointDef("Skywall", "not decided yet", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionSkywall, Condition = HourglassData.ConditionSkywall,
        // 0.62: was 1852140643 ("nelc", lowercase -- no such unit: Skywall's trigger never armed); the Vortex Pinnacle
        // is nELC, and the Sulfuron Spire (swSS) counts too (maintainer): both lost or not Skywall's
        Hq = new[] { UNIT_NELC_VORTEX_PINNACLE_SKYWALL_T3, UNIT_SWSS_SULFURON_SPIRE_SCOURGE_T3 },
      },
      new TurningPointDef("Black Empire", "not decided yet", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionBlackEmpire, Condition = HourglassData.ConditionBlackEmpire,
        Hq = new[] { 1429221722 },             // U01Z N'Zoth
      },
      new TurningPointDef("Ahn'qiraj", "Brood of Nozdormu (not yet)", TurningPointKind.Fallback,
        TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionAhnqiraj, Condition = HourglassData.ConditionAhnqiraj,
        Hq = new[] { 1429221458 },             // U00R C'Thun
      },
    };

    // 0.54: all definitions of a WL faction name (a faction can have several turning points), in catalog order
    internal static List<TurningPointDef> FindAll(string factionName)
    {
      var found = new List<TurningPointDef>();
      for (var i = 0; i < All.Count; i++)
      {
        if (All[i].Faction == factionName)
        {
          found.Add(All[i]);
        }
      }

      return found;
    }
  }
}
