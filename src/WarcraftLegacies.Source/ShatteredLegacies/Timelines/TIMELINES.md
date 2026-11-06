# Timelines -- how they are built

**What** is planned lives in `SCENARIO_CONCEPTS.md` (vocabulary, ground rules, decision register D1-D17, every slot)
and its flow in `timeline-overview.drawio`. This file is the **how**: the turning-point model, the WL mechanics it
reuses, the template for adding content to a faction, the data the engine needs, and the status per faction.
Open work and its order: `../TODO.md`. (Until 0.51.1 this file was `FALLBACKS.md`; its superseded design -- the
"Enslaved" rule, the team wrappers, the old per-faction tables -- is in git history: `git show
v0.51.1:src/WarcraftLegacies.Source/ShatteredLegacies/FALLBACKS.md`.)

## 1. The model: turning points

A **turning point** is any deviation from WL's default history: a fallback, a sub path, a betrayal or a pick
(`SCENARIO_CONCEPTS.md` section 1). Every turning point has the same frame:

- **Trigger** -- when it becomes available (the HQ falls, a legend dies, a turn, another turning point ...).
- **Decision** -- humans decide at the Hourglass of Fate (or in a pick / pop-up dialog), bots by code (the bot rules
  in `SCENARIO_CONCEPTS.md` section 2).
- **Effects** -- what it does, as a list of small reusable steps:

| Effect | What it does | WL mechanic it reuses |
|---|---|---|
| Swap faction | the player gets a new `Faction` object (full successor) | `QuestScarletCrusade`: `new X()`, `CopyObjectLevelsFrom`, `GetPlayerData().Faction = x`, `FactionManager.Register` |
| Modify faction | rename, new icon, object limits added / removed (light successor) | `Faction.Name`, `Faction.Icon`, `Faction.ModObjectLimit`, an `ObjectInfo` delta |
| Change team | joins team X (once -- ground rules) | `PlayerData.SetTeam`, `Faction.Unally` |
| Relocate | a hidden base appears, units arrive | `Rectangle.PrepareUnitsForRescue` + `player.RescueGroup` (Theramore, Scarlet Crusade) |
| Leave behind | old holdings kept / to a lore heir / neutral hostile | the inheritance rule (`SCENARIO_CONCEPTS.md` section 6) |
| Heroes | legends come along or are created | `LegendaryHero.ForceCreate`, `AllLegends` |
| Announce | text for everyone, minimap ping | -- |
| Split (0.54) | one player becomes two: the old faction moves with army, quests and colour to a free slot (P11 / P14 / P20) and the bot plays it; the player gets the successor | `PlayerData.Faction` on both players, `AiApi.TakeOver` (the bot's leaver takeover) |
| Spawn start (0.54) | a start kit created by code (buildings on free spots, units) for a successor without a preplaced base | `AiApi.FindBuildSpot` |
| Hero fate (0.54) | a legend is lost to its faction for good, whatever is decided (runs when the trigger fires: `TurningPointDef.OnTrigger`) | `LegendaryHero.PermanentlyKill`, object limit 0 |

**Full vs light.** Not every faction gets a completely new faction. A **full** turning point swaps the faction (Quel'thalas
-> WL's Sunfury, Kul Tiras -> a Kezan Enterprise faction, like Lordaeron -> Scarlet Crusade). A **light** one keeps the
faction object and modifies it (Illidari -> draenei remnant: the non-draenei objects removed, a few added). Both run
through the same frame; only the effect list differs.

**Turning points are WL quests.** Each one is a `QuestData` (objectives = trigger + the Hourglass research,
`OnComplete` = the effect list), so it shows in the F9 quest log like WL's Scarlet Crusade and reuses WL's objectives
and the bot's quest handling. WL's own turning points (Scarlet Crusade, Theramore) stay WL's quests; the Hourglass only
lists them.

## 2. Reused WL mechanics (4.8.2) -- look here before writing new code

| Need | WL mechanic | Where |
|---|---|---|
| a new faction (full successor) | `Faction` subclass, registered on the quest's completion | `Factions/ScarletCrusade/ScarletCrusadeFaction.cs`, `Factions/Lordaeron/Quests/QuestScarletCrusade.cs` |
| a turning point | `QuestData` + objectives + `OnComplete` | `MacroTools/Quests/QuestData.cs`, `WarcraftLegacies.Source/Objectives/` (`ObjectiveControlPoint`, `ObjectiveLegendDead`, `ObjectiveCapitalDead`, `ObjectiveControlLegend`, ...) |
| a research as the decision | `ObjectiveResearch` -- completes when the research finishes in ANY building (checked: no building check) | `Objectives/UnitBased/ObjectiveResearch.cs` |
| stay / betray alternatives | `ResearchManager.RegisterIncompatibleSet` -- researching one removes the others | `MacroTools/Researches/ResearchManager.cs` (used by `ScourgeFaction`) |
| quests on a WL faction without editing its file | `FactionManager.FactionRegistered` event + `Faction.AddQuest` | `MacroTools/Factions/FactionManager.cs`, `Faction.cs` |
| an alternative start (pick) | `FactionChoice` + `FactionChoiceDialogPresenter` (one active choice = picked without a dialog) | `Setup/FactionChoiceDialogSetup.cs`, `MacroTools/UserInterface/ChoiceDialogPresenter.cs` |
| object limits of a faction | `ObjectInfo` list + `Faction.ProcessObjectInfo` | `WarcraftLegacies.Shared/FactionObjectLimits/*ObjectInfo.cs`, `ObjectInfoRepository.cs` |
| a hidden base until the turning point | `PrepareUnitsForRescue` (`RescuePreparationMode.HideAll` / `HideNonStructures` / `Invulnerable`) + `RescueGroup` | `MacroTools/Extensions/RectangleExtensions.cs`, `PlayerExtensions.cs` |
| a quest as a requirement | "Quest completed: X" researches as `ureq` of units / researches | WL's quests; `unit_scan.py` names them |
| bonus income | `PlayerData.BonusIncome` | the Kezan Oil Supply (`Factions/KezanEnterprise/`) |

## 3. Template: adding content for a faction

One folder per WL faction that gets timeline content, and one per new faction -- the same layout as WL's
`Factions/<Faction>/`:

```
Timelines/Factions/<Faction>/
  <Faction>Fallback.cs        a turning point's effects (TurningPointDef.Run; the definition is in TimelineCatalog,
                              the quest is attached by TimelineSystem through FactionManager.FactionRegistered)
  Quests/Quest<Name>.cs       the turning points and follow-up quests (QuestData, like WL)
  Mechanics/<Name>.cs         special mechanics (like Kezan's oil income and creep market)
  <New>Faction.cs             only for a full successor: Faction subclass (like ScarletCrusadeFaction)
src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/<New>ObjectInfo.cs
                              object limits of a new faction (+ one [SL] line in ObjectInfoRepository.cs)
```

Steps, in this order:

1. **Design** -- the slot's entry in `SCENARIO_CONCEPTS.md` (lore anchor, divergence, decision ID) and the drawio
   (`tools/shattered-legacies/timelines/timeline_overview_gen.py`).
2. **Content** -- choose what to recycle from the content index (main map, 24p map, stock WC3 assets) and note it in
   the slot's row of section 6. Never re-scan the map data for this.
3. **Map data** -- copies get `?Za?` ids (`Z` second, lowercase third) (`../README.md`, ID rule); list every new / changed object in
   `../README.md` "Changes to map data". Hidden bases: preplaced Neutral Passive inside a start rectangle (section 5).
4. **Object limits** -- an `ObjectInfo` file for a new faction; a light turning point uses `ModObjectLimit`.
5. **Code** -- the turning-point quest (trigger, Hourglass research, effects), the `Faction` subclass for a full one,
   mechanics. Announcement texts: drafts by the assistant, final text by the maintainer.
6. **Bot** -- production data of buildings no WL faction owns: add them to `SL_BUILDINGS` in
   `tools/shattered-legacies/ai/gen_ai_data.py` and re-run it (`../Bot/SlAiData.g.cs`); heroes and anything else
   through `AiApi` in `../Bot/SlBotSetup.cs`. A missing `AiApi` method is added in the same drop.
7. **Docs** -- section 6 status, `../README.md` lists, `../CHANGELOG.md`, `../TODO.md`.

## 4. The engine today (0.53)

| Part | Today | Next (`../TODO.md`) |
|---|---|---|
| `TimelineCatalog` + `Core/TurningPointDef` | one definition per WL faction: HQ unit types, trigger text, Hourglass ids, start spot, placeholder hero, announcement, `Run` (the faction's own effects) | definitions per faction folder |
| `Core/TurningPointQuest` | a turning point run by the timelines = a WL quest of its faction ("Fallback: ...", "Betrayal: ..."), attached through `FactionManager.FactionRegistered` (no WL faction file changes); a faction can have several. Objectives: the trigger -- `ObjectiveHqLost` by default, any WL objective through `TurningPointDef.MakeTrigger` (0.54: `ObjectiveLegendKilledBy`) -- which unlocks the decision through its condition tech, + WL's `ObjectiveResearch` (the decision at the Hourglass). `OnTrigger` effects run when the trigger fires, the effects when the quest completes | alternatives (stay / betray) as incompatible sets |
| `Core/TimelineSystem` | every 5 s: the HQ objectives (all HQ units lost after having held one; held again -> unavailable again). Bots decide at once (the decision objective is set complete), humans research at the Hourglass. `ForceFallback` for the defeat rule | alternatives (stay / betray) |
| `Core/Hourglass` | the Hourglass of Fate (`nZh0`, sealed top-right corner, columns by team), one per slot whose faction has a research on it; per-player limits hide the other factions' researches | -- |
| `Core/Effects/` | `SwapFaction` (+ keep colour), `LeaveBehind` (WL's removal: removable units removed, CPs / capitals Neutral Hostile), `StartArea` (take a start area like a pick), `ChangeTeam` (living-target rule) | modify faction (light), inheritance to a lore heir |
| Defeat (`../Rules/EliminationRule`) | the only way to lose: no CP for more than 5 turns (WL's `PlayerElimination` with one `[SL]` hook; humans, from turn 20, as in WL); an unused fallback starts instead of the defeat | -- |

**The Hourglass of Fate.** One DECISION research per turning point; it requires a CONDITION tech with a readable
name ("Requires: Silvermoon Palace lost"), which `ObjectiveHqLost` grants while the trigger holds --
until then WC3 shows the button greyed out, so players see what is possible. Turning points that are not built yet
show "Requires: Not available yet". WL's own researches stay where they are and are listed too (the Scarlet
Crusade's `R0XZ`: Tyr's Hand and the Hourglass -- `ObjectiveResearch` accepts it from any building). Researches of
other factions are hidden by their object limits (0). Data and ids: `tools/shattered-legacies/timelines/gen_hourglass.py`
(writes the map data and `Core/HourglassData.g.cs`). Alternatives (stay / betray) will be incompatible sets
(`ResearchManager.RegisterIncompatibleSet`).

**Placement of the Hourglass** (`Regions.ArtifactDummyInstance`, 19584..22688 x 23232..24800): no preplaced units, no
path out, inside the camera area up to y 24576. Three columns around x 22208 from y 23500 north, 150 apart, 7 per
column; the middle column fills first (0.39.7).

**Adding a turning point** (on top of section 3): a row in `gen_hourglass.py` (decision + condition, re-run), the
definition in `TimelineCatalog` (`Decision` / `Condition` from `HourglassData`, `Hq`, `Trigger`, `Run`), the effects
in `Timelines/Factions/<X>/`.

## 5. Data for the engine

**Hidden bases.** 0.53: a turning point's `Prepare` runs at map setup -- Sunfury's start area is hidden with WL's
`PrepareUnitsForRescue(HideAll)` and taken over by `StartArea.Take`; areas a successor's own quests take over later
(Tempest Keep, Area 52, Upper Netherstorm) are made invulnerable and paused (`Invulnerable`), because hidden units
cannot be found again by those quests. Preplaced in the map data (Neutral Passive) inside a start rectangle and prepared with WL's rescue
mechanism, like Theramore: structures visible and invulnerable (nobody can build on the spot), units hidden; the
turning point hands everything over (`RescueGroup`). Reused neutral buildings inside the rectangle get the same
treatment. Check per rectangle that no other WL rescue / cleanup covers the area (WL removes Neutral Passive units in
some areas at runtime -- `CleanupNeutralPassiveUnits`).

**HQ per faction.** Today's catalog uses WL's essential legends (ALL must be lost). The concepts change some of them.

| Faction | WL essential legends | Catalog today | Concepts / open |
|---|---|---|---|
| Lordaeron | Capital Palace `h000`, Stratholme Castle `h01G`, Tyr's Hand Citadel `h030` | (WL: Scarlet Crusade) | Arthas games: WL's research restart (D14); HQ fall breaks the Great Alliance; Garithos games: no fallback |
| Dalaran / Gilneas | Violet Citadel `h002` / Gilneas Castle `h04I` | (WL: Theramore) / placeholder | both picks: Theramore / Jaina's Sanctum, Jaina's choice D6 |
| Quel'thalas | Silvermoon Palace `h003`, the Sunwell `n001` | Silvermoon Palace only (0.53, maintainer) | -- |
| Stormwind | Stormwind Keep `h00X` | same | -- |
| Ironforge | the Great Forge `h001` | same | -- |
| Kul Tiras | Boralus Keep `h046` | same | -- |
| Druids | Nordrassil `n002`, Cenarion Hold `n06D` (+1) | both listed | the World Tree consumed / Nordrassil falls |
| Sentinels / Exodar | Auberdine `e00J`, Feathermoon `e00M` / Exodar Regalis `e01X` | same | -- |
| Frostwolf / Warsong | Thunder Bluff `o00J`, Darkspear Hold `o02D` / Stonemaul Keep `o004`, Orgrimmar `o01B` | same | orc HQ falls |
| Scourge | Frozen Throne `u000` | same | Lich King Arthas dies (Ebon Blade) |
| Legion | Legion Stronghold `u00G` | same | all Argus CPs; Archimonde while the Legion controls him |
| Fel Horde | Kil'sorrow Fortress `o017`, Black Temple `o00F`, Hellfire Citadel `o008` | all three | -- |
| Illidari | (none) | none | the Black Citadel |
| Skywall / Black Empire / Ahn'qiraj | Vortex Pinnacle `nelc` / N'Zoth `U01Z` / C'Thun `U00R` | same | -- |

**Placement data** (CP ids = raw codes; owner 24 = Neutral Hostile, 27 = Neutral Passive -- often part of a WL rescue,
check for conflicts; other numbers = a player slot at start). Rows for designs the concepts dropped are left out.

| Successor | Start spot | CPs / quest CPs | Assets to reuse | Checks |
|---|---|---|---|---|
| Theramore (Dalaran, Gilneas) | `Regions.Theramore` | Jaina's Sanctum `n0JS` (-7616,-9152), Alcaz Isle | WL's Theramore base | Gilneas uses the same path (no own fallback) |
| Sunfury (Quel'thalas) | `Regions.SunfuryStartingPosition` (1888..3616, -23424..-22048) | Netherstorm `n02O` (-64,-19840) | the whole WL Sunfury start | only once Sunfury is no longer an Illidari pick |
| Valiance Expedition (Stormwind) | Borean Tundra | Borean Tundra `n00G` (-7744,15680); quest: Dragonblight `n02Q` (-640,17152) | Stormwind buildings (copies) | the Nexus area clear runs there (fine) |
| Dwarven Expedition (Ironforge, name open) | near the Storm Peaks | Storm Peaks `n02S` (3456,22976); quest: Ulduar ("Secrets of Ulduar", new) | Explorer's Vault `n07U` (copy), dwarf buildings, frost dwarves (24p?) | Yogg-Saron's prison is next door (the Yogg quest needs that CP -- fine, lore) |
| Kezan Enterprise (Kul Tiras) | `Regions.KezanUnlock` (-2400..608, -8000..-5184) | Kezan `n04Z` (-960,-7168); quests: Gadgetzan `n05C` (-8832,-13184), the goblin towns (Goblin Empire) | the base preplaced in KezanUnlock (owner 27, WL's creep goblins -> the 24p copies by `MainEquivalents`) | 0.58: `HideNonStructures`; no other WL rescue / cleanup covers it |
| Dream trees (Druids) | Twilight Grove, Duskwood | Duskwood `n00V` (12160,-16384, owner 20 at start); quest: Hinterlands `n01A` (16256,4352) | druid buildings (copies) | Duskwood starts owned by slot 20 -- which faction? |
| Maiev's Watchers (Sentinels, Draenei) | Warden's Cage, Outland | Warden's Cage `md03` (5440,-31040, owner 27) | `Regions.MaievStartUnlock` (-6016..-4896, -28416..-27104), the Sentinels' Maiev quests | MaievStartUnlock belongs to a Sentinels quest -> reuse or second rectangle |
| Twilight's Hammer (orc slot) | Wetlands / Grim Batol | Wetlands `n016` (14080,-2240), Dragonmaw Port `n08T` (21312,-3200); Grim Batol `n03X` (17344,-3456) | Grim Batol is a neutral WL capital (`h01Z`, Twilight's Hammer creeps) + tunnels `n08R`; Dragonmaw assets; 24p tech / unit / building trees | the capital must stay WL's |
| Forsaken (Quel'thalas betrayal, D3) | Undercity (under Lordaeron City) | Lordaeron City `n01G` (9088,8576, owner 27), Tirisfal `n01F` (6784,10112, owner 9 = Lordaeron) | 24p Forsaken buildings (e.g. Plaguetorium `u011`) | Tirisfal / Brill are Lordaeron's at start -> own small rectangle |
| Knights of the Ebon Blade (Scourge) | Eastern Plaguelands (Acherus) | Havenshire `n04W` (20608,9344, owner 27), Corin's Crossing `n03P` (17536,9088, owner 9) | Scourge buildings (copies) | Havenshire is owner 27 (rescue?) |
| Mag'har (Fel Horde) | Nagrand (Garadar) | Nagrand `n00B` (-2944,-27904), Halaar `n0CV` (-4352,-29312) | orc buildings (copies) | none found |
| Draenei remnant / Akama's Ashtongue (Illidari, light) | Shadowmoon Valley | Shadowmoon `n09V` (2560,-31040, owner 27), Black Temple `n00I` (7168,-30144, owner 27) | the Illidari's own draenei objects | both owner 27 -> probably the Illidari's own rescue |
| Brood of Nozdormu (Ahn'qiraj) | Tanaris (Caverns of Time) | Tanaris `n020` (-10624,-15360) | dragon models (24p) | -- |

**Light successors -- what the engine can do without copying units.** Removing units / buildings: object limits = 0.
Adding units: only when a building of the player trains them (train lists belong to the building TYPE -- give the
player such a building, or a copy with a changed list). Changing trained units without copies: on "training finished"
name, HP, damage, armour, attack speed can be set per unit (`BlzSetUnitName`, `BlzSetUnitMaxHP`, ...); the build menu
still shows the original (WC3 has no per-player text for unit types). Upgrades / researches can be given or taken per
player. The bot's data is keyed by faction name: a renamed faction needs its own entries (`AiApi.RegisterHeroes`).
`FACTION_UNITS.md` (generated) lists every unit / hero per WL faction with a "Timeline" column (keep / lose).

## 6. Status per slot

Today = what the code does in 0.53. Placeholders (Marin Noggenfogger at the planned spot + announcement) are replaced
faction by faction.

| Slot | Turning points (concepts) | Today | Content plan |
|---|---|---|---|
| P10 Lordaeron | pick Arthas / Garithos (D1), Scarlet Crusade (WL, D14), Treason (D10), Helm (D11) | WL's Scarlet Crusade (also on the Hourglass, 0.53) | -- |
| P3 Quel'thalas | fallback Sunfury (WL faction), betrayal Forsaken (D3), rogue nation (D16) | **fallback Sunfury** (0.53, full); **betrayal Forsaken** (0.54, split; placeholder tech) | Forsaken: the 24p tree (0.55), breaking free + Undercity (0.56) |
| P8 Dalaran / Gilneas | fallback Theramore for both, Jaina's choice (D6), The Gates Close (D7) | Dalaran: WL's Theramore; Gilneas: placeholder (Darkshore -- superseded) | WL's Theramore |
| P2 Stormwind | fallback Valiance Expedition, D9, Bolvar -> Lich King | placeholder | -- |
| P5 Ironforge | fallback Dwarven Expedition (name open; must differ from Valiance), D9 | placeholder ("Explorers' League") | base near the Storm Peaks, frost dwarves, "Secrets of Ulduar" (new -- WL's Explorer quest is for Kalimdor) |
| P23 Kul Tiras | fallback Kezan Enterprise (own team) | **fallback Kezan Enterprise** (0.39.5; rebuilt 0.58: own `Faction`, the 24p goblin tree, base on Kezan via WL's rescue, Oil Rigs, 5 quests) | 0.59: 24p Trading Center + trade, Oil Refining, end goal on the scoreboard; 0.60: Refugee Slave Camps -- open: camps built one by one, what the end goal does, turtle concept |
| P12 Druids | fallback Dream trees | placeholder | -- |
| P19 Sentinels / Draenei | fallback Maiev's Watchers for both, The River (D8), The Triumvirate (D15) | placeholders (Exodar: Shattrath -- superseded) | -- |
| P1 Frostwolf / Warsong | fallback Twilight's Hammer, Found the Horde (D12) | placeholders ("Dragonmaw" -- superseded) | Dragonmaw assets + 24p tech / unit / building trees |
| P4 Scourge | fallback Ebon Blade, civil war (D5) | Hourglass only | -- |
| P24 Legion | fallback Dreadlord Insurgents, Grand Crusader (D13) | Hourglass only | -- |
| P16 Illidari | fallback draenei remnant (light; Akama's Ashtongue) | Hourglass only; no Sunfury pick any more (0.53) | non-draenei objects removed, a few added |
| P7 Fel Horde | fallback Mag'har | Hourglass only | -- |
| P9 Skywall / P13 Black Empire | fallbacks **open** (Earthen Ring / Xal'atath or Ramkahen / Titan Watchers) | Hourglass only | -- |
| P17 Ahn'qiraj | fallback Brood of Nozdormu | Hourglass only | dragon models (24p) |

## 7. History (implemented so far)

- 0.39.1: first slice in `Ai/Fallbacks/`: one definition per faction, trigger buildings (`n0FB`, `AFB1`), bots run by
  code. 0.39.2: bots take over leaving humans. 0.39.4: every faction gets a trigger building, every good faction a
  placeholder, HQs from WL's essential legends. 0.39.5-0.39.8: Kul Tiras -> Kezan Enterprise (Katherine to Kezan, old
  army to Stormwind or removed, Kezan + local creeps, no allies, goblin buildings, oil income, creep market).
- 0.39.10: moved to `ShatteredLegacies/`, uses the bot only through `AiApi`.
- 0.52: renamed to timelines (`Timelines/`, turning points, the Hourglass), split into `Core/`, the catalog and
  `Factions/<X>/`; same behaviour.
- 0.54: any objective as trigger, effects on the trigger, several turning points per faction, the split and spawn
  effects, the fork's first own faction (Forsaken), `AiApi.TakeOver`; pilot: Quel'thalas' betrayal.
- 0.60: Kezan: the Refugee Slave Camps at the goblin towns (Master of the Slave Trade); Sunfury starts with Area 52,
  Farahlon, Netherstorm.
- 0.59: Kezan: the 24p Trading Center (traders), Oil Refining, the end goal on the scoreboard; arriving factions on
  a bot get production buildings.
- 0.58: Kul Tiras -> Kezan Enterprise rebuilt: own faction (24p goblin tree), base on Kezan via WL's rescue mechanism,
  lore heir Stormwind (`LeaveBehind.ToHeir`), Oil Rigs, the 24p goblin quests.
- 0.53: turning points as WL quests, the Hourglass of Fate with researches (`nZh0`, `RZd*` / `RZc*`, replaces `n0FB`
  / `AFB1`), the effect library, the fork's defeat rule, Quel'thalas -> Sunfury (no Sunfury pick any more).
