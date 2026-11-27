# Timelines -- how they are built

**State: 0.89.0** (updated 0.89.0). What changed version by version: `../CHANGELOG.md`.

**What** is planned lives in `SCENARIO_CONCEPTS.md` (vocabulary, ground rules, decision register D1-D17, every slot)
and its flow in `timeline-overview.drawio`. This file is the **how**: the turning-point model, the WL mechanics it
reuses, the template for adding content to a faction, the data the engine needs, and the status per faction.
Open work and its order: `../TODO.md`. (Until 0.51.1 this file was `FALLBACKS.md`; its superseded design -- the
"Enslaved" rule, the team wrappers, the old per-faction tables -- is in git history: `git show
v0.51.1:src/WarcraftLegacies.Source/ShatteredLegacies/FALLBACKS.md`.)

## 0. The fallback map today (0.87.1)

Every successor is **conquered, not defeated** unless the table says otherwise: it serves the TEAM of its SEAT's
holder (`Core/Conquest`); at first that is the conqueror of the old faction (`Core/LastConqueror`). "Undefeatable" =
its halls cannot be destroyed and the defeat rule passes it by. Fallbacks get no second fallback.

| Slot | Turning point | Base | Seat (whoever holds it is the master) | Team at the start | Undefeatable | Code |
|---|---|---|---|---|---|---|
| Lordaeron | Scarlet Crusade (WL) | Tyr's Hand | -- | own | no | WL |
| Lordaeron | Gilneas | Gilneas | Gilneas City (mercy text) | North Alliance | no | `Factions/Gilneas` |
| Dalaran | Theramore (WL) | Theramore | Jaina's Sanctum (0.88: taken at 0 life; Jaina a dreadlord under an evil holder) | the holder's | no | WL + `Events/TheramoreJaina` |
| Quel'thalas | Sunfury | Netherstorm | Netherstorm CP | Outland / own | no | `Factions/Quelthalas` |
| Quel'thalas | Forsaken (betrayal, a split) | Northrend, later the Undercity | -- | the Scourge's, free later | no | `Factions/Forsaken` |
| Stormwind | Defias Brotherhood | the Deadmines (void dungeon) | Deadmines | the conqueror's | yes | `Factions/Defias` |
| Ironforge | Dark Iron of Shadowforge | Shadowforge (void dungeon) | Throne Room | the conqueror's | yes | `Factions/DarkIron` |
| Kul Tiras | Kezan Enterprise | Kezan | Kezan | own | no | `Factions/KezanEnterprise` |
| Druids | Furbolgs | Timbermaw Hold (void den) | the Claw of Ursoc (0.88: free while Xavius lives or the Claw is theirs) | their own, then the Claw holder's | yes | `Factions/Furbolg` |
| Sentinels | Naga of Azshara | Nazjatar (void dungeon) | Eternal Palace | the conqueror's | yes | `Factions/Nazjatar` |
| Orcish Horde | Zandalari | Zuldazar | Zuldazar | the conqueror's | yes | `Factions/Zandalari` |
| Tauren Tribes | Centaur Khans | Thunder Bluff and the khans' huts | Thunder Bluff | the conqueror's | yes | `Factions/Centaur` |
| Scourge | Knights of the Ebon Blade | Acherus (0.88: flies once the four are level 10) | Icecrown Citadel | the conqueror's | yes | `Factions/EbonBlade` |
| Legion | Draenei | the Exodar (a ship that flies) | the Exodar's bridge | the conqueror's | no | `Factions/Draenei` |
| Fel Horde | -- (the Consortium, later; 0.89: an info row) | -- | -- | -- | -- | -- |
| Twilight's Hammer (P20, a start faction since 0.89) | Dragonmaw Clan | Dragonmaw Port (its base waits there from the start) | Dragonmaw Port | Grim Batol's holder / Cho'gall's killer | yes | `Factions/Dragonmaw` |
| Illidari | Ashtongue Deathsworn | the Black Temple | Black Temple CP | the destroyer's | no | `Factions/Ashtongue` |
| Black Empire | Harpies | Stonetalon | the caged Broodmother (0.88) | the slayer's | yes | `Factions/Harpy` |
| Ahn'qiraj | Skywall | the Lost City, the Vortex Pinnacle | the Vortex Pinnacle | the slayer's | yes | `Factions/Skywall` |
| Skywall (itself a fallback) | -- (Hourglass info row "Conquered, not defeated") | -- | -- | -- | -- | `TimelineCatalog` |

Shared paths: `ExclusivePath` (`Core/`) -- one destination, claimed once (`TwilightHammerFallback.Path` = the
Dragonmaw, `HordeRoute` = the Zandalari). Twilight's Hammer is a START faction since 0.89.0 (P20, `TwilightStart`):
The clan starts at the Twilight Citadel in the Southern Highlands with Cho'gall, five Cultists and the gate of the pass.
Capturing the Grim Batol control point for its team awards the fortress. Its fallback is the Dragonmaw.

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
faction object and modifies it (0.63: no case left -- the Illidari's Ashtongue became a full one). Rule (0.63): a
successor with its own name, tech tree, quests or income gets its own `Faction` class; light is only for "the same
faction, a few objects more or less" and never brings a mechanic of its own. Both run
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

## 4. The engine today (0.87)

| Part | Today |
|---|---|
| `TimelineCatalog` + `Core/TurningPointDef` | one definition per turning point: source faction, HQ unit types, trigger text, Hourglass ids, `Prepare` (map setup), `Run` (the effects); `ExclusivePath` for a destination several sources share |
| `Core/TurningPointQuest` | a turning point = a WL quest of its faction, attached through `FactionManager.FactionRegistered`. Objectives: the trigger (`ObjectiveHqLost` by default, any WL objective through `MakeTrigger`) + the Hourglass decision (`ObjectiveResearch`). `OnTrigger` effects run when the trigger fires |
| `Core/TimelineSystem` | every 5 s: the HQ objectives; bots decide at once, humans at the Hourglass; `ForceFallback` for the defeat rule; arrival income and grace |
| `Core/Hourglass` | the Hourglass of Fate (`nZh0`), one per slot, per-player limits hide other factions' researches |
| `Core/Conquest` (0.63, 0.80+) | vassals: the team of the seat's holder, optional undefeatable, optional own team until conquered (`keepTeam`), optional conquest text (0.87) |
| `Core/LastConqueror` (0.85) | who took / destroyed a building last -- the first master of a successor |
| `Core/Effects/` | `SwapFaction`, `SplitFaction`, `LeaveBehind` (incl. `ToHeir`, `RemoveHeroes`, `ToNeutralHostile`), `StartArea`, `StartCps` (`Force` / `Give`), `Spawn` (`Units` / `Buildings`), `ChangeTeam`, `TurnLevy` and `HeroRevival` (0.85), `Instance` (a void-area base behind waygates; 0.87: the entrance can move and both gates can shut) |
| `Dungeons/VoidDungeons` (0.82+) | the void-area bases: Nazjatar, Shadowforge, the Deadmines (0.83), Timbermaw Hold and the Exodar (0.87); shut until their fallback (`VOID_DUNGEONS.md`) |
| Defeat (`../Rules/EliminationRule`) | the only way to lose: no CP for more than 5 turns (humans, from turn 20); an unused fallback starts instead of the defeat |

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

**HQ per faction** (the trigger: ALL of them lost after having held one).

| Faction | HQ in the catalog |
|---|---|
| Lordaeron | Capital Palace `h000` (Gilneas); WL's Scarlet Crusade has its own research |
| Dalaran | WL's Theramore quest (the Violet Citadel) |
| Quel'thalas | Silvermoon Palace `h003` |
| Stormwind | Stormwind Keep `h00X` |
| Ironforge | the Great Forge `h001` |
| Kul Tiras | Boralus Keep `h046` |
| Druids | Nordrassil `n002`, Cenarion Hold `n06D` |
| Sentinels | Auberdine `e00J`, Feathermoon `e00M` |
| Orcish Horde | Orgrimmar `o01B` (also hidden preplaced) |
| Tauren Tribes | Thunder Bluff `o00J` |
| Scourge | the Frozen Throne `u000` |
| Legion | the Legion Stronghold `u00G` |
| Fel Horde | Kil'sorrow Fortress `o017` |
| Illidari | the Black Temple `o00F` destroyed |
| Black Empire / Ahn'qiraj | N'Zoth `U01Z` / C'Thun `U00R` |

**Placement data.** Each successor's spots, CPs and reused assets are in its `Timelines/Factions/<X>/` code (start
spots as constants at the top) and, for the void-area bases, in `VOID_DUNGEONS.md`. (0.87.1: the old placement table
of planned successors -- Valiance Expedition, Dwarven Expedition, Dream trees, Maiev's Watchers, Mag'har, Brood of
Nozdormu, Twilight's Hammer for the Horde -- is gone: none of them was built that way.)

**Light successors -- what the engine can do without copying units.** Removing units / buildings: object limits = 0.
Adding units: only when a building of the player trains them (train lists belong to the building TYPE -- give the
player such a building, or a copy with a changed list). Changing trained units without copies: on "training finished"
name, HP, damage, armour, attack speed can be set per unit (`BlzSetUnitName`, `BlzSetUnitMaxHP`, ...); the build menu
still shows the original (WC3 has no per-player text for unit types). Upgrades / researches can be given or taken per
player. The bot's data is keyed by faction name: a renamed faction needs its own entries (`AiApi.RegisterHeroes`).
`FACTION_UNITS.md` (generated) lists every unit / hero per WL faction with a "Timeline" column (keep / lose).

## 6. Status per slot

Section 0 is the status (0.87.1). Open work per slot: `../TODO.md`; designs still open (the Garithos path for
Lordaeron, the Great Alliance and its tax, Jaina's choice, the civil war ...): `SCENARIO_CONCEPTS.md`.

## 7. History (implemented so far)

- 0.64-0.87 (details: `../CHANGELOG.md`): the WL 5.0.4 alignment (0.69-0.77), the conquest rule for most successors
  (0.76+), the fallback pairings (0.80-0.81: Dragonmaw, Zandalari, Ebon Blade, Draenei), the void-area dungeons (0.82-0.84:
  the Undercity, Nazjatar, Shadowforge, the Deadmines; naga), Defias / Centaur / Harpies (0.85), Dark Iron / Furbolgs
  (0.86), the flying Exodar, Timbermaw Hold, the 24p's Dark Iron, dungeon creeps (0.87), no second fallbacks (0.87.1).

- 0.39.1: first slice in `Ai/Fallbacks/`: one definition per faction, trigger buildings (`n0FB`, `AFB1`), bots run by
  code. 0.39.2: bots take over leaving humans. 0.39.4: every faction gets a trigger building, every good faction a
  placeholder, HQs from WL's essential legends. 0.39.5-0.39.8: Kul Tiras -> Kezan Enterprise (Katherine to Kezan, old
  army to Stormwind or removed, Kezan + local creeps, no allies, goblin buildings, oil income, creep market).
- 0.39.10: moved to `ShatteredLegacies/`, uses the bot only through `AiApi`.
- 0.52: renamed to timelines (`Timelines/`, turning points, the Hourglass), split into `Core/`, the catalog and
  `Factions/<X>/`; same behaviour.
- 0.54: any objective as trigger, effects on the trigger, several turning points per faction, the split and spawn
  effects, the fork's first own faction (Forsaken), `AiApi.TakeOver`; pilot: Quel'thalas' betrayal.
- 0.63: Illidari -> Ashtongue Deathsworn (full): the conquest rule (Core/Conquest), purification, the Temple of the
  Deathsworn / of Karabor.
- 0.60: Kezan: the Refugee Slave Camps at the goblin towns (Master of the Slave Trade); Sunfury starts with Area 52,
  Farahlon, Netherstorm.
- 0.59: Kezan: the 24p Trading Center (traders), Oil Refining, the end goal on the scoreboard; arriving factions on
  a bot get production buildings.
- 0.58: Kul Tiras -> Kezan Enterprise rebuilt: own faction (24p goblin tree), base on Kezan via WL's rescue mechanism,
  lore heir Stormwind (`LeaveBehind.ToHeir`), Oil Rigs, the 24p goblin quests.
- 0.53: turning points as WL quests, the Hourglass of Fate with researches (`nZh0`, `RZd*` / `RZc*`, replaces `n0FB`
  / `AFB1`), the effect library, the fork's defeat rule, Quel'thalas -> Sunfury (no Sunfury pick any more).
