using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Gilneas;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Kultiras;
using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Nazjatar;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Harpy;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Centaur;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Defias;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIron;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Furbolg;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Zandalari;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.EbonBlade;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Draenei;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Illidari;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Skywall;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Quelthalas;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines
{
  // One turning point per WL faction (was FallbackConfig.All, 0.39.1-0.39.7). Since 0.87.1 every row is real content or
  // WL's own (Theramore, Scarlet Crusade); the placeholders of the first slice (Marin Noggenfogger at the planned place)
  // are all gone, the last ones replaced or removed per entry. The overview is TIMELINES.md section 0. HQs: WL's
  // essential legends (0.39.4; ALL of them must be lost; a recaptured HQ makes the turning point unavailable again).
  // Skywall is shown on the Hourglass only (NotYet: an info row -- it is itself a fallback and is conquered).
  // 0.53: Decision / Condition = the Hourglass researches (Core/HourglassData.g.cs).
  internal static class TimelineCatalog
  {
    internal static List<TurningPointDef> All = new()
    {
      // WL runs it (automatic when the Violet Citadel falls) -- no Hourglass
      new TurningPointDef("Dalaran", "Theramore", TurningPointKind.Fallback, TurningPointSource.Wl),
      // WL runs it (research at Tyr's Hand); the Hourglass lists WL's research too
      new TurningPointDef("Lordaeron", "Scarlet Crusade", TurningPointKind.Fallback, TurningPointSource.Wl)
        { WlResearch = 1378900058 },          // R0XZ The Scarlet Crusade
      // 0.85.0 (maintainer): the Capital Palace lost -> GILNEAS (was Stormwind's fallback, 0.76.0-0.84.0): the last
      // humans of the north seek shelter behind the Greymane Wall (Factions/Gilneas/GilneasFallback.cs)
      new TurningPointDef("Lordaeron", "Gilneas", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "The Capital Palace is lost",
        Description = "If Lordaeron's capital falls, its survivors flee to Gilneas: you play the Kingdom of Gilneas "
                      + "behind the Greymane Wall, led by Genn Greymane, in the North Alliance -- until someone takes "
                      + "Gilneas City. Everything left in Lordaeron is lost.",
        Decision = HourglassData.DecisionLordaeronGilneas, Condition = HourglassData.ConditionLordaeronGilneas,
        Hq = new[] { UNIT_H000_CAPITAL_PALACE_LORDAERON },
        X = 7168f, Y = 512f,                   // Gilneas City
        HeroType = 0,                          // Genn Greymane leads them (GilneasFallback)
        Prepare = () => GilneasFallback.Prepare(),
        Run = (p, def) => GilneasFallback.Run(p, def),
        Announcement = "|cffffcc00Lordaeron has fallen.|r The last humans of the north seek shelter at the Greymane "
                       + "Wall. Moved by their plight, King Genn Greymane is finally persuaded to open the gates -- "
                       + "|cffffcc00Gilneas joins the war effort|r of the North Alliance.",
      },
      // 0.53: WL's Sunfury faction as Quel'thalas' fallback (no longer an Illidari pick); Forsaken = the betrayal (D3)
      new TurningPointDef("Quel'thalas", "Sunfury", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Hq = new[] { 1747988531 },             // h003 Silvermoon Palace (0.53, maintainer: enough on its own)
        Trigger = "Silvermoon Palace is lost",
        Description = "If Quel'thalas falls, Prince Kael'thas leads the last of his people to Netherstorm. The survivors "
                      + "start again as the Sunfury -- serving whoever takes the Netherstorm CP.",
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
      // 0.87.1 (maintainer): no second fallback for Gilneas -- it is itself Lordaeron's fallback and is conquered (the
      // "Gilneans on Darkshore" placeholder is gone)
      // 0.76.0 (maintainer): Stormwind -> Gilneas, on the North Alliance (Timelines/Factions/Gilneas/GilneasFallback.cs).
      // Replaces the Valiance Expedition placeholder; Gilneas is no longer a pick of the Dalaran slot.
      // 0.85.0 (maintainer): Stormwind Keep lost -> the Defias Brotherhood in the Deadmines, sworn to whoever holds the
      // Deadmines (Factions/Defias). Gilneas (the Stormwind fallback until 0.84) is Lordaeron's now (above).
      new TurningPointDef("Stormwind", TurningPointKind.Fallback, DefiasFallback.Path)
      {
        Trigger = "Stormwind Keep is lost",
        Description = "With Stormwind fallen the red masks come out of the Deadmines: you play the Defias Brotherhood "
                      + "-- Dunco Ratsbon and Count Clessington, bandits that pillage what they strike and recruits "
                      + "every turn -- sworn to whoever broke Stormwind, then to whoever holds the Deadmines. "
                      + "Everything else of Stormwind is lost.",
        Decision = HourglassData.DecisionStormwind, Condition = HourglassData.ConditionStormwind,
        Hq = new[] { UNIT_H00X_STORMWIND_KEEP_STORMWIND_OTHER },
        RequireTriggerForForcedFallback = true,
      },
      // superseded: Dwarven Expedition (name open) -- base near the Storm Peaks, frost dwarves, a quest at Ulduar (P5)
      // 0.86.0 (maintainer): the Great Forge lost -> the Dark Iron of Shadowforge, bound by the Mercenary Charter to
      // whoever holds the Siege Foundry (Factions/DarkIron); was "Explorers' League (placeholder)"
      new TurningPointDef("Ironforge", TurningPointKind.Fallback, DarkIronFallback.Path)
      {
        Trigger = "The Great Forge is lost",
        Description = "The Bronzebeard throne is empty: a Dark Iron force tunnels up to claim the Great Anvil. You play "
                      + "the Dark Iron of Shadowforge -- Anvilrage guards, bear riders, magma tanks and golems, "
                      + "Dagran Thaurissan at their head -- sworn to whoever broke Ironforge, then to whoever holds "
                      + "their Throne Room. Everything else of Ironforge is lost.",
        Decision = HourglassData.DecisionIronforge, Condition = HourglassData.ConditionIronforge,
        Hq = new[] { UNIT_H001_THE_GREAT_FORGE_IRONFORGE_OTHER },
        RequireTriggerForForcedFallback = true,
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
      // 0.86.0 (maintainer): Nordrassil and Cenarion Hold lost -> the Furbolgs (Factions/Furbolg); was "Dream trees
      // (placeholder)". 0.88.0: the Claw of Ursoc decides their master, not Nordrassil (FurbolgClaw)
      new TurningPointDef("Druids", TurningPointKind.Fallback, FurbolgFallback.Path)
      {
        Trigger = "Nordrassil and Cenarion Hold are lost",
        Description = "The furbolg tribes follow whoever proves worthy of holding their Sacred Ancestral Totem: you "
                      + "play the Furbolgs -- Nafien and their den, Timbermaw Hold, warriors every turn, more and "
                      + "stronger for every world tree your side holds -- with Xavius and Peroth'arn: your own "
                      + "masters while Xavius lives or the Claw of Ursoc is yours, whoever takes the Claw takes you. "
                      + "Everything else of the druids is lost.",
        Decision = HourglassData.DecisionDruids, Condition = HourglassData.ConditionDruids,
        Hq = new[] { UNIT_N002_NORDRASSIL_DRUIDS_OTHER, UNIT_N06D_CENARION_HOLD_SENTINELS_OTHER },
        RequireTriggerForForcedFallback = true,
      },
      // 0.84.0 (maintainer): both Sentinels HQs lost -> the naga of Azshara in the Nazjatar dungeon, sworn to whoever
      // holds the Eternal Palace (Factions/Nazjatar). Maiev's "Chase Illidan to Outland" (the fallback until 0.83)
      // becomes a sub-path without losing the Sentinels (TODO.md).
      new TurningPointDef("Sentinels", TurningPointKind.Fallback, NazjatarFallback.Path)
      {
        Trigger = "Auberdine and Feathermoon Stronghold are lost",
        Description = "With the Sentinels broken Queen Azshara's naga rise in Nazjatar: you play the Naga of "
                      + "Nazjatar, sworn to whoever broke the Sentinels -- then to whoever holds the Eternal Palace. "
                      + "Everything else of the Sentinels is lost.",
        Decision = HourglassData.DecisionSentinels, Condition = HourglassData.ConditionSentinels,
        Hq = new[] { UNIT_E00J_AUBERDINE_SENTINELS_OTHER, UNIT_E00M_FEATHERMOON_STRONGHOLD_SENTINELS_OTHER },
        RequireTriggerForForcedFallback = true,
      },
      // superseded: the Draenei pick takes Maiev's Watchers too (P19)
      // 0.87.1 (maintainer): no second fallback for the Draenei -- they are themselves the Legion's fallback and are
      // conquered through the Exodar's bridge (the "Draenei of Shattrath" placeholder is gone)
      // 0.81.0 (maintainer): Old God conqueror -> Twilight, anyone else -> the Zandalari (no Dragonmaw for the Horde)
      // 0.87.0 (maintainer): trolls only -- always the Zandalari
      new TurningPointDef("Orcish Horde", TurningPointKind.Fallback, TwilightHammerFallback.HordeRoute)
      {
        Trigger = "Orgrimmar is lost",
        Description = TwilightHammerFallback.HordeOfferDescription,
        Decision = HourglassData.DecisionOrcishHorde, Condition = HourglassData.ConditionOrcishHorde,
        Hq = new[] { UNIT_O01B_ORGRIMMAR_WARSONG }, RequireTriggerForForcedFallback = true,
        Announcement = "|cffffcc00The Horde of Kalimdor is broken.|r",
      },
      // 0.85.0 (maintainer): Thunder Bluff lost -> the Centaur Khans (Factions/Centaur); Skywall moved to Ahn'Qiraj
      new TurningPointDef("Tauren Tribes", TurningPointKind.Fallback, CentaurFallback.Path)
      {
        Trigger = "Thunder Bluff is lost",
        Description = "Whoever broke the Tauren is the Apex Predator of the plains: the centaur swear him the "
                      + "Blood-Oath of the Hunt. You play the Centaur Khans -- warriors every turn at the camp by "
                      + "Thunder Bluff, four more khans to win -- sworn to whoever holds Thunder Bluff.",
        Hq = new[] { UNIT_O00J_THUNDER_BLUFF_FROSTWOLF_OTHER }, RequireTriggerForForcedFallback = true,
        Decision = HourglassData.DecisionTauren, Condition = HourglassData.ConditionTauren,
      },
      // evil factions: shown on the Hourglass only (greyed out, "Not available yet"), names from the concepts
      // 0.80.0 (maintainer): the Frozen Throne lost (WL ruptures it) and the Lich King, if he rose, dead for good ->
      // the Knights of the Ebon Blade, hero-only, sworn to whoever holds Icecrown Citadel (Factions/EbonBlade)
      new TurningPointDef("Scourge", TurningPointKind.Fallback, EbonBladeFallback.Path)
      {
        Trigger = "The Frozen Throne is lost and the Lich King is dead",
        Description = "With the Lich King dead and the Frozen Throne broken his death knights are free: you play the "
                      + "Knights of the Ebon Blade -- four death knights and Acherus, sworn to whoever holds Icecrown "
                      + "Citadel. Everything else of the Scourge is lost.",
        Decision = HourglassData.DecisionScourge, Condition = HourglassData.ConditionScourge,
        Hq = new[] { 1966092336 },             // u000 Frozen Throne
        Prerequisite = EbonBladeFallback.LichKingGone,
        RequireTriggerForForcedFallback = true,
        Announcement = "|cffffcc00The Lich King has fallen and the Frozen Throne is broken.|r His death knights wake "
                       + "free: the Knights of the Ebon Blade ride out of Acherus -- they serve whoever holds Icecrown.",
      },
      // 0.80.0 (maintainer): the Legion Stronghold lost -> the Draenei (no Sentinels pick any more), joining the
      // conqueror's team (Factions/Draenei/DraeneiFallback.cs; the announcement is the fallback's own, by conqueror)
      new TurningPointDef("Legion", TurningPointKind.Fallback, DraeneiFallback.Path)
      {
        Trigger = "The Legion Stronghold is lost",
        Description = "With the Legion beaten on Azeroth the Exodar wakes: you play the Draenei, sworn to whoever beat "
                      + "the Legion -- even an old enemy -- then to whoever holds the Exodar's bridge. The ship lands "
                      + "near your conqueror and can fly again. Everything else of the Legion is lost.",
        Decision = HourglassData.DecisionLegion, Condition = HourglassData.ConditionLegion,
        Hq = new[] { 1966092359 },             // u00G Legion Stronghold
        RequireTriggerForForcedFallback = true,
      },
      // 0.89.0 (maintainer): the Dragonmaw are Twilight's fallback now -- the Fel Horde gets the Consortium (rogue
      // ethereal raiders) later; until then an info row on the Hourglass
      new TurningPointDef("Fel Horde", "the Consortium (later)", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionFelHorde, Condition = HourglassData.ConditionFelHorde,
        MakeTrigger = () => new ObjectiveCapitalDead(AllLegends.FelHorde.KilsorrowFortress)
          { ResearchId = HourglassData.ConditionFelHorde },
      },
      // 0.89.0 (maintainer): Twilight's Hammer is a start faction (P20, Factions/TwilightHammer/TwilightStart) -- its
      // fallback is the Dragonmaw Clan at Dragonmaw Port: Grim Batol lost while Cho'gall is dead
      new TurningPointDef("Twillight", TurningPointKind.Fallback, TwilightHammerFallback.Path)
      {
        Trigger = "Grim Batol is lost and Cho'gall is dead",
        Description = TwilightHammerFallback.OfferDescription,
        Decision = HourglassData.DecisionTwilight, Condition = HourglassData.ConditionTwilight,
        Hq = new[] { UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT },
        Prerequisite = TwilightHammerFallback.ChogallDead,
        RequireTriggerForForcedFallback = true,
        Announcement = "|cffffcc00The Twilight's Hammer is broken.|r",
      },
      // 0.63 (maintainer): the Black Temple destroyed -> Akama's Ashtongue Deathsworn, sworn to whoever destroyed it, then
      // to whoever holds its CP (Timelines/Factions/Illidari/IllidariFallback.cs)
      new TurningPointDef("Illidan", "Ashtongue Deathsworn", TurningPointKind.Fallback, TurningPointSource.Timeline)
      {
        Trigger = "The Black Temple is destroyed",
        Description = "If the Black Temple falls, Illidan is gone for good. Akama and the Ashtongue Deathsworn rise from "
                      + "its ruins, sworn to whoever holds the Black Temple's CP. Ishanah purifies Telredor, Halaar, "
                      + "Shattrath and Oshu'gun from Shattrath with your allies' mana -- then Karabor is restored.",
        Decision = HourglassData.DecisionIllidari, Condition = HourglassData.ConditionIllidari,
        Hq = new[] { UNIT_O00F_BLACK_TEMPLE_ILLIDARI_FEL_HORDE },
        X = AshtongueConfig.TempleX, Y = AshtongueConfig.TempleY,
        HeroType = 0,                          // Akama leads them
        Prepare = () => IllidariFallback.Prepare(),
        Run = (p, def) => IllidariFallback.Run(p, def),
        Announcement = "|cffffcc00The Black Temple has fallen.|r From its ruins Akama and the Ashtongue Deathsworn "
                       + "rise -- sworn to those who hold the temple, bent on restoring Karabor to the Light.",
      },
      new TurningPointDef("Skywall", "not decided yet", TurningPointKind.Fallback, TurningPointSource.NotYet)
      {
        Decision = HourglassData.DecisionSkywall, Condition = HourglassData.ConditionSkywall,
        // 0.62: was 1852140643 ("nelc", lowercase -- no such unit: Skywall's trigger never armed); the Vortex Pinnacle
        // is nELC, and the Sulfuron Spire (swSS) counts too (maintainer): both lost or not Skywall's
        Hq = new[] { UNIT_NELC_VORTEX_PINNACLE_SKYWALL_T3, UNIT_SWSS_SULFURON_SPIRE_SCOURGE_T3 },
      },
      // 0.85.0 (maintainer): N'Zoth lost -> the Harpies of Stonetalon, sworn to his slayer (Factions/Harpy)
      new TurningPointDef("Black Empire", TurningPointKind.Fallback, HarpyFallback.Path)
      {
        Trigger = "N'Zoth is lost",
        Description = "With N'Zoth's tether severed the harpies of Stonetalon imprint on the banner of whoever slew "
                      + "him: you play the Harpies -- their nests, a few harpy kinds and two matriarchs -- sworn to "
                      + "whoever holds the caged Broodmother by the nests. Everything else of the Black Empire is "
                      + "lost.",
        Decision = HourglassData.DecisionBlackEmpire, Condition = HourglassData.ConditionBlackEmpire,
        Hq = new[] { UNIT_U01Z_OLD_GOD_NZOTH },
        RequireTriggerForForcedFallback = true,
      },
      // 0.85.0 (maintainer): C'Thun lost -> SKYWALL (was the Tauren's; the Zandalari stay the Orcish Horde's), sworn
      // to C'Thun's slayer, then to whoever holds the Vortex Pinnacle (Factions/Skywall)
      new TurningPointDef("Ahn'qiraj", TurningPointKind.Fallback, SkywallFallback.Path)
      {
        Trigger = "C'Thun is lost",
        Description = "With C'Thun fallen the storm lords of Skywall claim the Lost City and the Vortex Pinnacle: you "
                      + "play Skywall, sworn to C'Thun's slayer, then to whoever holds the Vortex Pinnacle. Everything "
                      + "left of Ahn'Qiraj is lost.",
        Decision = HourglassData.DecisionAhnqiraj, Condition = HourglassData.ConditionAhnqiraj,
        Hq = new[] { UNIT_U00R_OLD_GOD_CTHUN },
        RequireTriggerForForcedFallback = true,
        Announcement = "|cffffcc00C'Thun has fallen.|r Al'Akir's storms sweep over the desert -- Skywall claims the "
                       + "Lost City and serves the one who slew the Old God.",
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
