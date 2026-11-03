# TODO -- Shattered Legacies (fork content)

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Finished items go to
`CHANGELOG.md`. **This file holds everything that exists only in Shattered Legacies**: timelines, new factions, map
content, the fork's win rule and game length, balance, known issues, the maintainer's ideas. Bot work (what would go
into the upstream PR: `Ai/` + `[BOT]` hooks) is in `../Ai/TODO.md`. Fork code uses the bot only through `Ai/AiApi.cs`.

Design: `Timelines/SCENARIO_CONCEPTS.md` (what) and `Timelines/TIMELINES.md` (how, incl. the faction template).

## Order of the work

The structure comes first, so that every later faction is the same template filled with different content.

0. ~~Structure (0.52)~~: folders, rename to timelines, separate docs for bot and fork, tools in `tools/shattered-legacies/`.
1. **Content index** (own patch, no game code)
2. **Turning-point core** + Defeat rules, pilot: Quel'thalas -> Sunfury
3. **Kul Tiras -> Kezan Enterprise rebuilt** on the core; Scarlet Crusade shown on the Hourglass
4. **Light turning point**, pilot: Illidari -> draenei remnant
5. **Lordaeron pick** Arthas / Garithos
6. **Faction by faction** (concepts)
7. **Shared mechanics**

### 1. Content index (own patch)

A search index of everything that can be recycled, so no assistant has to scan the map data again.
- [ai] `tools/shattered-legacies/content/content_index.py` reads `mapdata/WarcraftLegacies/` (main map) and
  `mapdata/WarcraftLegacies24p/` (24p map, already in the repo since WL 4.8.2 -- the 24p GitHub repo has vJASS only,
  no object data) and writes greppable TSV files to `ShatteredLegacies/Content/`, one per kind (units, abilities,
  upgrades, items). Committed, regenerated on demand.
- Columns: map, id, name, editor suffix, base id, race, faction (main: from `ObjectInfo`; 24p: from the building
  that trains it), trains / builds / researches, requirements, model, icon, **model source** (stock WC3 / import, and
  whether the import exists in the main map), **id status** ("identical in main" / "different object in main" /
  "24p only" -- ~385 unit ids exist in both maps as different objects).
- **Stock WC3 objects** (`hfoo`, `ogru`, ... and their models / icons): only referenced, never copied. Their data is
  already in the repo: `src/War3Api.Object/Generated/1.36.0.20257/` (WC3 1.36: ~865 units, ~283 items, 90 upgrades,
  ~837 abilities -- names in `Enums/*Type.cs`, model, icon, race, train lists in `*Loader.cs`). The index reads them
  from there as map "stock".
- [ai] Later, with the first content that needs it: `import.py <map>:<id> ...` copies objects with a new `?Z??` id and
  their dependencies (abilities, upgrades, trained units, model import as LFS pointer) and records them in a generated
  `Content/SL_OBJECTS.md` (new id | name | source | used by). LFS: the copied pointer refers to an object the repo
  already has; check with `git lfs ls-files` that it is downloaded / pushed (see "New Kezan units are invisible").

### 2. Turning-point core (pilot: Quel'thalas -> Sunfury)

- [ai] Turning point = `QuestData` (objectives: trigger + Hourglass research; `OnComplete`: effect list) and the
  effect library (swap faction, modify faction, change team, relocate, leave behind, heroes, announce --
  `Timelines/TIMELINES.md` section 1). Definitions move from `TimelineCatalog` into `Timelines/Factions/<X>/` and are
  attached to WL's factions through `FactionManager.FactionRegistered` (no WL faction file changes).
- [ai] **Hourglass of Fate** with researches: one per turning point, a dummy "condition" tech per trigger (greyed out
  with readable text until it fires), incompatible sets for alternatives, hidden for other factions by object limits.
  New object with a `?Z??` id (replaces `n0FB` / `AFB1`). `[you]` names, icon, tooltip texts; check the command card
  in game (several hidden researches on one button position).
- [ai] **Defeat rules** (maintainer): leavers -> bots (exists); lose only with no CP at all for more than 5 turns (WL's
  `PlayerElimination` adjusted); no defeat when the essential legends are gone; a faction with an unused fallback is
  never defeated. One `[SL]` hook at the top of `MacroTools/Factions/Faction.Defeat()`; check every caller (`PlayerElimination`,
  essential legends, `CleanupUnoccupiedPlayerSlots`, `PlayerLeaves`).
  Threshold (maintainer): **0 CPs** for more than 5 turns -- 1 CP is enough to live.
- [ai] Pilot **Quel'thalas -> Sunfury** (full turning point, WL's `SunfuryFaction` and its quests, start in
  `Regions.SunfuryStartingPosition`): remove the Sunfury pick from the Illidari slot (one `[SL]` line in
  `Setup/FactionChoiceDialogSetup.cs`; with one choice left WL picks it without a dialog), then drop
  `AiApi.ExcludeFromBotPicks("Sunfury")` in `Bot/SlBotSetup.cs`. Must run before WL's defeat (needs the Defeat hook).
- [you] HQ of Quel'thalas: Silvermoon Palace AND the Sunwell (today) or the Sunwell only (concepts: "Sunwell destroyed").

### 3. Kul Tiras -> Kezan Enterprise rebuilt

- [ai] `KezanEnterpriseFaction` (`Faction` subclass) + `KezanEnterpriseObjectInfo` in
  `src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/` (+ one `[SL]` line in `ObjectInfoRepository`)
  instead of renaming WL's Kul Tiras object and setting limits by hand; the hidden base via WL's rescue mechanism
  (`Regions.KezanUnlock`); leave behind by the inheritance rule. Oil income and creep market stay
  (`Factions/KezanEnterprise/`).
- [ai] The Scarlet Crusade's research (`R0XZ`, Tyr's Hand) listed on the Hourglass too -- no WL change.
- [you] Kezan Oil Supply and Trading Center should not be buildable -- which builder offers them? Trading Center
  placement, the creep list, stock refresh, oil income on the scoreboard; Bilgewater Harbor ships.
- [you] End goal: acquire 20,000 gold (confirm the number); oil upgrades that raise the income?

### 4. Light turning point (pilot: Illidari -> draenei remnant)

- [ai] Modify faction: remove the non-draenei objects, add a few (content index), Akama. HQ: the Black Citadel.

### 5. Lordaeron pick Arthas / Garithos (D1)

- [ai] A `FactionChoice` for the Lordaeron slot like Dalaran / Gilneas (`Timelines/Factions/Lordaeron/`).
  `[you]` Garithos as its own `Faction` class or Lordaeron with a game flag -- decide when we build it.

### 6. Faction by faction (`Timelines/SCENARIO_CONCEPTS.md` section 8)

Order proposal (reuse first): Theramore for Gilneas (WL path exists) -> Maiev's Watchers (Sentinels and Draenei) ->
Valiance Expedition (Bolvar -> Lich King) -> Dwarven Expedition -> Twilight's Hammer -> Knights of the Ebon Blade ->
Forsaken (Quel'thalas' betrayal, D3) -> Dream trees -> Mag'har -> Dreadlord Insurgents -> Brood of Nozdormu.
- `[you]` **Dwarven Expedition**: name (must differ from Stormwind's Valiance Expedition); base near the Storm Peaks,
  frost dwarves, "Secrets of Ulduar" -- a NEW quest (Ironforge's Explorers' League quests in WL go to Kalimdor / Uldum).
- `[you]` Fallbacks of **Skywall** and the **Black Empire**: Earthen Ring / Xal'atath or Ramkahen / Titan Watchers.
- Twilight's Hammer: Dragonmaw assets + the 24p tech / unit / building trees (content index).
- End goals of the remaining factions (`SCENARIO_CONCEPTS.md` section 10).

### 7. Shared mechanics (concepts sections 5, 7, 9, 11)

Great Alliance + Imperial Tax + tax vote (D2; replaces the retired Thandol Treaty draft), Defias, Legion's Tithe,
shared boxes (World Tree, Helm of Domination), defection (free slots P11 -> P14 -> P20), barks, sudden death "The End
Time". `[you]` the numbers (tax rate, sudden death X / N / Y).

## Win rule and game length

- `[you]` Goal: a game ends with ONE winner after **100-140 min**, and the winner has to capture everything. WL's rule
  today: a TEAM wins at 90 CPs (`ControlPointVictory.CpsVictory`). Needs the fork's own rule (an `[SL]` change).
- Caution: "everything" must mean every CAPTURABLE CP -- some CPs are invulnerable / quest-locked and can never fall.
  The last few CPs (islands) make long tails; a share (e.g. 95 %) or "all capitals + N %" ends cleaner. Bots must be
  able to reach them all (ferries).
- Measurement: the dump pins when a team first reaches 80 / 90 CPs; the actual winner / victory is not recorded yet
  (bot debug item "winner record", `../Ai/TODO.md`).

## Balance

- The bots' unit budget per CP (`../Ai/CONCEPTS.md` 8). `[you]` a different base per faction / team (good vs evil) --
  `AiApi.SetBudgetBase` from `Bot/SlBotSetup.cs`. `[you]` a CP-based food cap for humans too (a fork rule)?
- `[you]` **Old Gods balance -- re-check**: "too weak" was measured WITHOUT bot income. In 0.45.0 (bots with income)
  the Old Gods took all of Kalimdor; the Kalimdor team had 0 CPs at 45 min.
- `[you]` **Quel'thalas survives very long** (0.45.0: alive at 540 min with 12 CPs). Map side: is the area unusually
  fortified (towers, walls, the Elfgate)? Bot side: `../Ai/TODO.md`.
- `[you]` **Stormwind Castle CP needs a buff**; **South Alliance vs Naga / Fel Horde**: the South needs a buff (or
  Illidan / Fel Horde a debuff).

## Known issues

- [ai] **Fallback messages are wrong**: when Kul Tiras loses its HQ, the game says the HQ was regained, although it
  was destroyed for good (v0.39.9 playtest; check with step 3).
- [ai] **Dalaran / Gilneas fallback does not swap teams on the scoreboard.** Gilneas should use Dalaran's path (same
  slot), with Gilneas City as its HQ (step 6).
- [ai] **New Kezan units are invisible**: the unit count goes up during production, but nothing can be seen. The
  model files exist in `Imports/` as Git LFS pointers -- first check that the LFS objects are really downloaded and
  pushed (`git lfs ls-files | awk '$2=="-"'` lists missing ones; `git lfs pull`, else fetch them from the original
  repository and `git lfs push origin`). Then textures / paths of the models.
- [ai] Hint "When a player leaves, their units are refunded ..." (`Setup/HintSetup.cs`) is wrong since bots take over
  leavers (0.39.2).
- [ai] Lobby slot names in `Info.json` are still WL's old 24p layout -> the Shattered Legacies layout
  (`SCENARIO_CONCEPTS.md` section 3).
- [you] Remove the preplaced units of the unused slots P6, P15, P18 at start? (WL only defeats empty slots that have a
  faction.)

## Map content, design and ideas (brainstorm)

The maintainer's own ideas go here too -- half-baked is fine. Timeline ideas go to `SCENARIO_CONCEPTS.md` (backlog,
section 12).
- [you] **Pseudo-flyers**: air units lose real flying and become pseudo-flyers, because bots handle flying pathing
  badly (orc air units fly over the ocean). Affects the bot too -- see `../Ai/TODO.md`.
- [you] **Book of Medivh redesign**: the Book gets its own spell; if the Scourge or the Legion gets it, it can trigger
  the summon on that spot.
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`.
- [you] Islands still free for neutral roadblocks (Zandalar trolls -- 24p: God King, Gurubashi, Amani, Sandfury).
- [ai] (low) Chinese localization from upstream WL (`Loc` system) -- a cherry-pick candidate from 5.x.
