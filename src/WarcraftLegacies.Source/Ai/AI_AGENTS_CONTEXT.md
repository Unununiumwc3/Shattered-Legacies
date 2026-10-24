# Context for AI assistants (vibecoding handover)

Read this first, then `TODO.md`. Humans: the normal docs are in `README.md`; "AI" in file and class names means
the **bots**, this file is about **AI assistants** working on the code.

## The project in one paragraph

Computer-player bots for **Warcraft Legacies 4.8.2** (a Warcraft III Reforged map written in C# with WCSharp and
transpiled to Lua by **CSharp.lua**). Kept on 4.8.2 because 5.x removed the Old Gods. Fork:
`https://github.com/lehmbein/WarcraftLegacies`, branch `4.8.2-ai`, tags `4.8.2-ai.0.MINOR.PATCH`. All bot code is
in `src/WarcraftLegacies.Source/Ai/`. Changes outside it (keep them minimal and listed here): one hook line in
`Setup/GameSetup.cs` (`WarcraftLegacies.Source.Ai.AiSetup.Setup();` after `Artifacts.Setup();`); `Loc.cs` literal
replace (0.34.3); the bot-pick hook `MacroTools/UserInterface/BotChoicePolicy.cs` + `ChoiceDialogPresenter.TryBotPick`
(0.37.1); the map title in `Warcraft.Cartographer/.../AdvancedMapBuilder.cs` + version in
`WarcraftLegacies.CLI/appsettings.json` (0.37.1); `Setup/ControlPointSetup.cs`: the 1-gold CP marker (0.38); `Setup/GameSetup.cs`: no game-mode vote, Closed
Alliance forced (0.39.1); `GameLogic/GameEnd/PlayerLeaves.cs`: the AI takes over leaving humans (0.39.2); map data `n0FB` / `AFB1` (fallback trigger building, 0.39.1); map data:
CPs `n0JS` Jaina's Sanctum (0.37.2), `n0SA`/`n0TD`/`n0RF` + marker `A0ZG`/`B0ZG` (0.38).

## Getting the code (sandbox)

```
GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none --sparse --branch 4.8.2-ai \
    https://github.com/lehmbein/WarcraftLegacies.git wl && cd wl
git sparse-checkout set src/WarcraftLegacies.Source/Ai tools/ai src/MacroTools \
    mapdata/WarcraftLegacies/Units mapdata/WarcraftLegacies/UnitData mapdata/WarcraftLegacies/AbilityData \
    src/WarcraftLegacies.CLI src/Warcraft.Cartographer
```
(Cone mode also brings the files directly in the parent folders: `mapdata/WarcraftLegacies/PathingMap.json` and
`src/WarcraftLegacies.Source/Regions.g.cs` -- what `tools/ai/map_query.py` needs. Add `src/WarcraftLegacies.Source/Factions`
to read quest classes.)
The repo is huge (map data, LFS models); never clone it fully. Upstream WL for comparison:
`https://github.com/AzerothWarsLR/WarcraftLegacies` (`main` = 5.x).

## Files

| File | Class(es) | Contents |
|---|---|---|
| `AiSystem.cs` | SimpleBot, QuestAutoComplete, Geography, AiState, AiSetup | core loop (combat round-robin 0.5 s, production 15 s, defense, heroes, graph targeting, logging), quest automation, landmass grid, shared runtime collections, setup + version |
| `AiTech.cs` | AiTech | tech pools, building credits, free/event/quest upgrades, CP-capture counter |
| `AiMissions.cs` | AiMissions | starter kit, Ragnaros, kill targets, area clears, one-shots, CP grants, Kul Tiras repair, palace handover, banned forms |
| `AiLegion.cs` | AiLegion | Legion summon/betrayal, Argus start base, node events (Sunwell), demon portal fix, all AI portals |
| `AiWaveRunner.cs` | AiWaveRunner | invasion-wave runner; no waves configured since 0.37.1 (`AiWaves.All` empty) |
| `AiGates.cs` | AiGates | gate removal (+ new gates on entering the map), named graph gates (greymane, tomb, thandol, oldgods), Old Gods rift, Emerald Portals, graph merge |
| `AiAutopilot.cs` | AiAutopilot | `-autopilot` |
| `AiGoldVote.cs` | AiGoldVote | bot strength vote at 49 s (the old game-mode vote slot) (sets `SimpleBot.BotGoldBonus`) |
| `AiHeroVisits.cs` | AiHeroVisits | quests that need a hero at a place (Yogg-Saron, Tomb, Soul Gem, ...); config `AiConfig.HeroVisits` |
| `AiChoices.cs` | AiChoices | bot picks in WL's click menus (orc faction random, Scourge always invades); hook `MacroTools/UserInterface/BotChoicePolicy.cs` |
| `AiData.g.cs` | AiData | GENERATED (tools/ai/gen_ai_data.py): train lists, altar heroes, requirements, hero skill order -- re-run after WL updates |
| `AiHeroSkills.cs` | AiHeroSkills | bot heroes learn skills (level-up event + wave tick) |
| `AiStats.cs` | AiStats | what bots train (train-finish event) -> dump section TRAINED (0.38) |
| `AiHeroCare.cs` | AiHeroCare, AiStandings | stranded bot heroes (walk back / to the altar); team CP standings (dump TEAMS, 80/90 pins) |
| `AiConfig.cs` | AiConfig, AiGraph, AiWorld, ... | data: CP graph edges, portals, quest rules, kill targets, coordinates |
| `AiUpgrades.cs` | AiUpgrades | data: tech pools per faction, building upgrades, event researches |
| `AiDebug.cs` | AiDebug | chat commands, `-dump` writer, readouts, **the command switch `Access`** |
| `tools/ai/check_refs.py` | | cross-class refs + CSharp.lua limits (run before every build) |
| `Fallbacks/` | FallbackConfig, FallbackSystem, KezanEnterprise | fallback scenarios: trigger buildings, HQ watch, placeholder fallbacks, Kul Tiras -> Kezan Enterprise (oil income, creep market) |
| `FACTION_UNITS.md` | | GENERATED (`unit_scan.py --md`): units / heroes per faction, for fallback keep/lose decisions |
| `FALLBACKS.md` | | fallback scenarios: design, teams, per-faction work list (big project, after the load rework) |
| `tools/ai/lua_check.py` | | compile-check a built `war3map.lua` with Lua 5.3 |
| `tools/ai/scope_check.py` | | C# scope errors tree-sitter misses (local hides a type, CS0841; nested re-declaration, CS0136) |
| `tools/ai/gen_ai_data.py` | | writes `AiData.g.cs` from WL's ObjectInfo + map data (run after every WL update) |
| `tools/ai/unit_scan.py` | | per faction: every unit/hero, can the AI train it, why not, which quest unlocks it |
| `tools/ai/map_query.py` | | offline map checks from `mapdata/` (walkable, nearest walkable, preplaced units, rects) |

## Hard rules (each one cost a broken build)

1. **CSharp.lua: all static members of a class become Lua locals of ONE function (Lua limit 200 incl. 1 param).**
   It counts methods, non-auto property getters and private static fields up to ~195 (overflow goes to a table
   named `const`), but **every non-private static `readonly` field is added on top, uncounted** -> 201 = map won't
   load ("map unavailable or corrupted", no error text). Keep classes small; shared state = `internal static`
   **without** `readonly`.
2. **CSharp.lua: static fields with a non-literal initializer (`= new()`, a call) are built in one static
   constructor limited to ~55 upvalues; fields past that are silently `nil` at runtime.** Keep each class under ~45.
3. `dotnet build` passing proves nothing about the Lua; wrong-class references only fail at the transpile step.
   Run `tools/ai/check_refs.py` (refs + both limits) and, after building, `tools/ai/lua_check.py artifacts/war3map.lua`.
4. Never `string.Replace` with user/game text that may contain `%` (Lua pattern). WL's `Loc.Format` is fixed.
5. Avoid `GetHandleId` (key dictionaries by unit/player objects instead).
6. Every periodic subtask runs through `SimpleBot.Safe(name, task)`; errors go to the pinned log once
   (`TICK ERROR in ...`), never abort the tick. Commands and dump sections are isolated the same way.
7. `partial` classes do NOT help with the limits (still one Lua function). Split into separate classes.
8. Class names must be unique in the namespace -- `dotnet build` of the CLI does not compile the AI code, only the
   map build does ("namespace already contains a definition for ..."). `check_refs.py` reports DUPLICATE CLASS.
9. **A local variable hides a type of the same name in its WHOLE block** (0.37.1 break, CS0841): WL code uses
   lowercase API types (`timer`, `unit`, `player`, `trigger`, `dialog`, `effect`) and sometimes names locals the same
   (`timer timer = timer.Create();` in `ChoiceDialogPresenter.Run`). Any `timer.Create()` ABOVE that line in the same
   block then fails. Never add code to a method that declares such a local -- put it in an own method.
   `tools/ai/scope_check.py` catches this and nested re-declarations (CS0136).
10. **No compiler in the sandbox** (no dotnet, no nuget). Before every drop: `check_refs.py`, a tree-sitter syntax
    pass and `scope_check.py` on EVERY changed .cs file, including files outside `Ai/`; JSON files through `json.load`.
12. **Unqualified calls across classes don't compile** (0.39.2 break, CS0103): a helper added to `SimpleBot` and
    called as `IsActive(...)` from `QuestAutoComplete` (same file, other class). `check_refs.py` reports these as
    UNQUALIFIED since 0.39.3.
11. **Keep file encodings**: WL's C#/JSON files start with a UTF-8 BOM. Python: read `utf-8-sig`, write the BOM back
    if it was there; `/bin/sh` is dash -- `printf '\xef'` writes the text `\xef`, not the byte. Compare BOMs with
    `git show HEAD:<file> | head -c 3 | od -An -tx1` before delivering.

## How we work

- Session start: the maintainer pastes the repo/branch and a task; clone sparsely (command above), read this file
  and `TODO.md`. He compiles in a Windows VM (Winboat) and runs git on Ubuntu; no GitHub Actions.
- "Discuss only" = do not code. When coding: leave out what is unclear, put it in `TODO.md`, and say so.
- At most one question per reply; propose defaults instead of asking many.
- Deliver whole files (zip with repo paths) plus one `git diff` patch against the pushed branch, verified with
  `git apply --check` on a clean worktree of `origin/4.8.2-ai`.
- Version on every code drop: `AiSetup.Build` (bottom of `AiSystem.cs`) AND `MapSettings.Version` in
  `src/WarcraftLegacies.CLI/appsettings.json` (`4.8.2-ai.<build>`; also the published file name), `CHANGELOG.md`.
  Map title: "Warcraft Legacies Botified <version>" (`AdvancedMapBuilder.SetMapTitles`). Docs-only drop: CHANGELOG +
  tag only. Release = commit + tag `4.8.2-ai.<build>` + push (commands in `README.md`, "Releasing").
- Keep `TODO.md` and `CHANGELOG.md` current in the same drop; TODO only holds open work.
- Testing data comes as `-dump` files (`CustomMapData/WLAI_dump_<time>.txt`). Decode:
  ```
  python3 -c "import re,sys;[print(m.group(1)) for l in open(sys.argv[1],errors='replace') for m in [re.match(r'\s*call Preload\( \"(.*)\" \)',l)] if m]" dump.txt
  ```
  The header shows the build -- ignore dumps of another build. Read **PINNED** first (one-off events,
  `TICK ERROR`/`FAILED`), then **BOTS** (state, target, army), **TEAMS**, **PORTALS**, **EVENT LOG**.

## Map data (offline, no game needed)

- `mapdata/WarcraftLegacies/PathingMap.json`: 32-unit cells, rows bottom-up from (-28672, -32256), flag 0x02 not
  walkable / 0x08 not buildable. **Terrain only** -- doodads, destructables (rocks, bridges, trees) and units are NOT
  in it. `tools/ai/map_query.py` wraps it (walk / area / near / find / around / rect).
- Preplaced units: `Units/<x>_<y>.json`, grouped by area. The names follow NO simple rounding rule (checked in
  0.38) -- append a new unit to the file that already holds its nearest neighbours; never create new files. `TypeId`/`SkinId` **little-endian** (`struct.unpack('<I', b'n0JS')`), unique
  `CreationNumber` (max + 1). Doodads/destructables: `Doodads/*.json`. Object data: `UnitData/<id>.json` (`OldId`
  base, `NewId`, `Modifications`: `unam` name, `uabi` abilities, `uupt` upgrades to), `AbilityData`, `DestructableData`.
  Rectangles: `src/WarcraftLegacies.Source/Regions.g.cs`. The JSON files round-trip exactly with
  `json.dumps(obj, indent=2, ensure_ascii=False)` + the original trailing whitespace (check before editing).
- **A new control point** (0.37.2, Jaina's Sanctum): copy an existing CP's `UnitData` (e.g. `n09C` Alcaz Isle) to a
  free id, rename it, place one unit (owner 24, like every CP) on a walkable 64-aligned spot clear of units. WL's
  `ControlPointSetup` finds it at runtime by its income marker ability (`A0LO` = 10 gold/min, buff B025; markers exist
  for 10/15/20/25/30/50, and since 0.38 `A0ZG`/`B0ZG` = 1 gold). Then add the graph edges (big-endian id, e.g.
  `n0JS` = 1848658515) -- in the edge list of the CP's continent (see "edge list membership" below).
- Map coordinates from the maintainer had sign typos 3 times -- always check them offline first.
- **Doodads block pathing but are not in the pathing data**: the first Maw of Gor'ma rift end stood on a Silithid
  archway -> ground units could not enter the waygate (one-way rift). For portal ends and building spots also check
  `Doodads/*.json` within ~250 (decor like fog/glow is harmless; archways, obelisks, walls are not).
- Fixed building spots: the old Argus ring search tested corners at +-220 and rejected 7 of 8 good spots; Argus
  spots (`AiWorld.ArgusSpotX/Y`) were picked with a flood fill so the exits stay open. Same method for Sulfuron Spire.
- `mapdata/WarcraftLegacies24p` ("1.18a", 24 slots) is an OLD layout: 20 of our graph nodes missing, 65 moved. Not
  supported.

## WL facts that bit us

Engine / API
- `ControlPointManager.Instance.GetFromUnitType()` **throws** for non-CP ids -> always `SimpleBot.SafeCp()`.
- Player slots 24+ (neutrals) also report `PLAYER_SLOT_STATE_PLAYING`; only 0-23 are real players.
- `LegendaryHero.PermaDies` is set-only; detect death via `LegendaryHero.Died` or ability `LEgo`.
- Gates are lowercase-h custom units; gate destructables use big-endian FourCC.
- WL removes Neutral-Passive units from whole areas at runtime -> AI-created portals are owned by Neutral Extra (26).
- Some heroes can't walk (Old Gods' god heroes): anything sending "the best hero" must filter `GetUnitMoveSpeed > 1`.
- Choice names (`IChoice.Name`) and other `Loc.Get` text are localized PER CLIENT -- never base a synced decision on
  them (desync). Decide from objects/types.
- `Geography.Landmass()` returns -1 for islands, isolated realms (Black Empire / Maw of Gor'ma) and 9 Kalimdor CPs.

Map mechanics
- **Rocks**: WL blocks paths with invulnerable `LTrc` rock destructibles (`Setup/RockSetup.cs`, `RockSystem`), removed
  at a turn: Bridge Rocks (Thandol Span, `Regions.BridgeAmbient`) turn 30, Northrend rocks turn 6, Kalimdor south and
  AQ by quests. The Thandol bridge itself is only made invulnerable (walkable); the rocks are what blocks it. Not in
  the pathing data. The AI's "thandol" gate follows the real rocks since 0.37.2 (before: guessed 20:00 -> dwarves
  stood at the rocks for 10 min).
- **Edge list membership defines a node's continent**: `ResolveEdgeSet` counts a faction's CPs per list (Combined /
  Kalimdor). An edge in the WRONG list turns its end node into a node of that continent (0.37.1: the rift edge in
  Combined made the Maw an EK node -> Black Empire walked the EK graph all game). Cross-continent edges belong in
  ONE list (the home side of the factions that use them) or are gated and handled by the edge-set logic.
- Graph gate flags (`Edge.Gate`) must be opened by code: `legion`/`prelegion` (AiLegion), `greymane`/`tomb`/`thandol`/
  `oldgods` (AiGates). `greymane` was never opened from v25 to 0.35.4 -- a new flag needs its opener.
- A frontier can come from an ALLY's node on the far side of a barrier (Ironforge got Arathi through an ally node
  north of the closed bridge): the graph says reachable, the army can't get there. The stuck blacklist (4 min, then
  5/10/20 min skip since 0.37.2) limits the damage; real fix = walking costs (TODO, CP graph weights).
- Waygates: a landing spot on/near a building or preplaced unit lets flyers through but not ground units. Keep portal
  ends on open ground (`LandBeside` keeps 250 from buildings). Waygate n036 carries A01M/A023 "Anti Portal Blocking"
  (1000-range immolation); AI portals shrink it to 250.
- The Sunwell (`n001`) and Sunwell Corrupted (`n079`) are **capitals**, not CPs. Win threshold: 90 CPs per team.
- Shared quests (`SharedQuestRepository.RegisterQuest`: Tomb of Sargeras, Yogg-Saron, Ragnaros, ...) are ONE instance
  added to every faction -- completing it for one completes it for all.

Units and production
- A faction's units, heroes, buildings, limits and categories come from `src/WarcraftLegacies.Shared/
  FactionObjectLimits/<Faction>ObjectInfo.cs` (`ProcessObjectInfo` in the faction class); only the FIRST category is
  used by `Faction.TryGetObjectByCategory`. Which building trains what: map data `UnitData/<building>.json` `utra`.
  Unit requirements: `ureq` (often a "QUEST_COMPLETED" research set by a quest's `ResearchId`).
- Hero ids start with a capital H/O/E/N/U; Z.../R... entries in ObjectInfo are researches.
- Production since 0.38: each building trains from its own list (`AiData.Trains`), least-ordered type first,
  only types with met requirements (`SimpleBot.RequirementsMet`); heroes first, the bot saves gold for an available
  hero. Before (category pools -> building kinds) only 105 of 448 unit types were trainable; now 248 (rest: ships,
  builders, quest/summon units). Vanilla buildings used unmodified have no `utra` in the map data -> their WC3
  default lists are in `unit_scan.VANILLA_TRAINS` (Crypt, Ancients) -- extend it when the scan shows such gaps.
- Gold was the hidden hero blocker: bots spent everything on units each turn (Quel'thalas ~18-110 gold), so
  available heroes (Rommath after "The Blood Elves") were never bought.

Economy
- Income/min = CP values (10-50 each) + `PlayerData.BonusIncome`, paid continuously. Every faction's `StartingGold`
  adds `Income` to `BonusIncome` for 10 turns (turn = 60 s) -> WL's early bonus ends at 11:00.
- The AI subsidy is a per-production-turn `SetPlayerState` -> it pays MORE the fewer bots there are (TODO: move to
  `BonusIncome`). With the vote on "Recruit" (0) bots had 0-12 gold and 1-5 units from minute 30 on.

Quests and story
- **Click menus**: every WL dialog is a `ChoiceDialogPresenter`. Since 0.37.1 computer players pick through
  `BotChoicePolicy.Pick` (set by `AiChoices`). Without a pick: option 1 after 24 s (bot orcs always Frostwolf, the
  Scourge "No invasion"). `AiSetup.Setup()` runs before `FactionChoiceDialogSetup.Setup()` in `GameSetup.cs`.
- **Arthas**: Scourge quest "The Culling" (Stratholme destroyed while Lordaeron's "Line of Succession" is open)
  permanently removes Lordaeron's Arthas (Hart) and lets the Scourge train its own (UEAR; N023 after "The Ascension"
  -- same legend, new unit type). Both legends are named "Arthas Menethil".
- **Legion summon**: "Under the Burning Sky" = the ritual with the Book of Medivh. Shared quest "Book of Medivh":
  Dalaran's capital dead + a unit at the pedestal (any Legion unit; others a level-12 hero). The AI summons on the
  Scourge taking the Sunwell (did not happen in 40-85-min tests before 0.36).
- "Twilight of the Gods" = Archimonde ENTERS `Regions.ArchimondeChannel` -> 420 s channel (any order breaks it).
- "Fireland Invasion" (Skywall) rescues the Sulfuron Spire base. Skywall's own Ragnaros hero ("Lord of the
  Firelands", U02K) comes from its quest "Emissary of the Firelands" -- not the neutral boss.
- **Highbank** = reward of Kul Tiras' "Eliminate Piracy" (Booty Bay + Katherine), not a backup start.
- **Subdue/Pillage** (trolls, tauren, ogres) is Warsong only (CP + Grom). Frostwolf gets trolls/tauren from plain
  quests and has no ogre quest.
- Hero-visit quests are `ObjectiveHeroWithLevelInRect` / `...ReachRect` / `ObjectiveLegendInRect`; read level and
  `Regions.X` rectangle from the quest class (Ragnaros 10 in WL, our rule 12; Yogg-Saron 14; Tomb of Sargeras 10).
- Ramzes the Horror = `h0BD` (-20185,-4704, carries an artifact); Ysondre `n04X` (-8240,9090); Taerar `n04S`
  (-17908,-6308). Old Gods rift ends: Maw of Gor'ma (-28049,-7701 since 0.38) and the preplaced `n07E`
  (22545,-4784, replaced at opening).
- Computer players' units use some abilities on their own (e.g. Blink) -- that is WC3's built-in unit AI, not ours.
- Yogg-Saron: after "The Beast With a Thousand Maws" the completing player owns the prison (`yLL4`, 4160,23424),
  which must cast "Free Yogg-Saron" (`LLLN`, channel, order `selfdestruct`); Yogg (`U02C`) then appears as a Neutral
  Aggressive boss. Storm Peaks CP = `n02S`.
- "Plague of Undeath" (Scourge, turn 8-11) rescues the Stratholme Scourge base (`Regions.StratholmeScourgeBase`) and
  refunds enemy structures there -- CPs are never refunded (`IsRemovable`), but their towers fight on.
- Hidden units (WL's rescue preparation) are NOT found by range searches -- "not found" is not "dead".
- WL heroes cost 100 gold (map data); bot heroes are free since 0.38 (`AiTech.FreeOrder`).
- Gate sweep is wanted (maintainer: bots must not use the gate mechanic; forcing bots to open gates failed). New
  gates are caught by an enter-map trigger since 0.38.

Your setup
- Freezes on the maintainer's Linux/Proton setup came only with alt-tabbing (also without autopilot). 0.37.1, turn
  17: picture frozen, sound kept playing. Points to the graphics layer -- but music streams on its own thread, so
  sound alone is no proof. Not an AI bug until a freeze without alt-tabbing (or on Windows) shows up.

## Key mechanisms (where to look)

- **Targeting** ("frontier picker"): CP graph (`AiGraph.Combined`, `AiGraph.Kalimdor`, merged at the Legion summon),
  frontiers, home graph first, neutral bias 0.5, 10-min lock on player-owned CPs, stuck blacklist (4 min, back-off
  5/10/20 min), stickiness `SimpleBot.StickySwitch`, army anchor = biggest same-landmass group. `SimpleBot.PickGraphTarget`.
- **Bot bonuses**: `SimpleBot.HasPerks(p)` -- real bots and test-mode autopilot only.
- **Command switch**: `AiDebug.Access` (`Off` release / `HostOnly` / `Everyone`); levels Read / Heavy / Control.
- **Portals**: `AiGraph.Portals` (phases always / prelegion / legion / oldgods), created by `AiLegion`, watchdog
  rebuilds them. Target distance through portals: `SimpleBot.TravelDist2`. Edge weights: `Edge.Weight`.
- **Missions**: Ragnaros (`AiMissions`, real level-12 hero), hero visits (`AiHeroVisits`, `AiConfig.HeroVisits`),
  kill targets / area clears (`AiConfig.KillTargets`, `ClearAreas`, `RequireType`). Heroes on a mission are in
  `AiState.MissionUnits`. Stranded heroes: `AiHeroCare`. Bot menu picks: `AiChoices`.
- **Bot strength vote** (`AiGoldVote`, 70 s) sets `SimpleBot.BotGoldBonus`; free hero revive needs a living altar.
