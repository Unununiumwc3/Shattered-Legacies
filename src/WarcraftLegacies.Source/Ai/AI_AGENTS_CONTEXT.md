# Context for AI assistants (vibecoding handover)

Read this first, then `TODO.md`. Humans: the normal docs are in `README.md`; "AI" in file and class names means
the **bots**, this file is about **AI assistants** working on the code.

## The project in one paragraph

Computer-player bots for **Shattered Legacies: Botified Realm**, an unofficial fork of **Warcraft Legacies 4.8.2** (a
Warcraft III Reforged map written in C# with WCSharp and transpiled to Lua by **CSharp.lua**). Kept on 4.8.2 because
5.x removed the Old Gods. Repo: `https://github.com/Unununiumwc3/Shattered-Legacies`, branch `botified-dev`, tags
`v0.MINOR.PATCH`. Never present it as "Warcraft Legacies" or "WL" in anything players see (lobby, loading screen,
chat): the original maintainer asked for that, and players mistook the fork for an official release. Attribution and
the fork notice ("based on Warcraft Legacies", "not the official version") stay -- he asked for those too.

**Two layers** (0.39.10):
- `src/WarcraftLegacies.Source/Ai/` = the bot. Long-term goal: a pull request to the original Warcraft Legacies that
  works on the original map without workarounds. The bot never uses fork code.
- `src/WarcraftLegacies.Source/ShatteredLegacies/` = the fork's own content (fallbacks, Kezan Enterprise). It may use
  the bot, but only through `Ai/AiApi.cs`. `tools/ai/check_refs.py` enforces both (LAYER, API).

Changes outside these folders are marked in the code: `// [BOT]` (hook the bot needs), `// [SL]` (fork content),
`// [WORKAROUND]` (temporary). Lists: `WORKAROUNDS.md` (what the bot needs outside `Ai/`, incl. map data and runtime
map changes; "blocks PR" yes/no) and `../ShatteredLegacies/README.md` (the fork's changes). Find all markers with
`grep -rn "\[BOT\]\|\[SL\]\|\[WORKAROUND\]" src`.

## State of the work (0.45.0 build, 0.45.1 docs) -- read this, then `TODO.md`

- **Plan in three phases** (`TODO.md`): 1 movement "good enough" (0.46, 0.47, 0.48/0.49 as needed; ends at the exit
  criteria in `TODO.md`, not at a version number) -> 2 restructure (~0.50: layout for the upstream PR, state in
  objects, old code deleted) -> 3 fine-tuning (graph without the fork's CPs, balance). Do not refactor during phase 1,
  but write NEW subsystems in the phase-2 style (own class, state in an object) once the 0.46 CSharp.lua check passes.
- **Upstream PR method**: not this code base -- clone upstream WL, PR branch, copy `Ai/` in plus its `[BOT]` hooks.
- **Next build 0.46** (`TODO.md`), built once all parts are in. Part 1 (creep fix, graph list review) and part 2
  (pseudo-portals + Dark Portal, planner keeps its target, reachable groups, unit budget, latency probe, guard A/B)
  are done; part 3 (activity / flapping / targets-by-owner counters, spread points, caster cap, gather A/B, `-edges`
  on the generated graph, dump_summary, CSharp.lua check class) is open. 0.47: theatres, ferries. Designs:
  `CONCEPTS.md` 6-9.
- **Engine first**: the order delay players feel (1-2 s) is WC3's pathfinder queue; 5227 units at 14 min in 0.44.2.
  The bot's op counters measure Lua spikes, not engine load (section "Engine (WC3) limits" below).
- **Refactor done so far** (details: `CHANGELOG.md`; overview and history: `README.md` "How the bots move" and
  "Already tried"): 0.39.10 layers -- 0.40 one scheduler -- 0.41 army orders -- 0.42 route planner on a generated
  graph -- 0.43 targets from the bot's own territory, defence with reach + hysteresis -- 0.44 movement chains, leftover
  buildings and creep camps as targets, **CP income for bots** (`AiIncome`; every game before 0.44.2 had bots WITHOUT
  income) -- 0.45 gather points, rift east end moved 100 north.
  - Switches for A/B tests: `-orders smart|idle|all`, `-route on|off`, `-chain on|off`, `-creeps on|off`,
    `-gather on|off`, `-budget on|off`.
- **Playtested**: up to 0.45.0 (table and the 0.45.0 results in `TODO.md`): the rift was never crossed (dead map
  after ~45 min), creep camps never ran (a TICK ERROR nobody noticed), Fel Horde flapped between the route planner and
  the old picker. **Read PINNED for `TICK ERROR` in every dump you get** -- `dump_summary.py` lists them.
- **More changes per playtest** (maintainer): every change gets its own switch and counter; behaviour switches get a
  per-bot A/B mode (0.46). Playtest protocol (stopwatch at turns) in `TODO.md`.
- **West <-> east**: Kalimdor + its islands (ships) vs Eastern Kingdoms + Kul Tiras are linked ONLY by the Old Gods
  rift (and the rare Legion portal). Bots will use it through a pseudo-portal (0.46); for humans the east end's fix
  (0.45.0) is unconfirmed.
- Measurements per game: `MEASUREMENTS.md` (`tools/ai/dump_summary.py --append`). The maintainer often plays with
  `-autopilot` on one faction (a human slot -- say which in the note).

## Getting the code (sandbox)

```
GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none --sparse --branch botified-dev \
    https://github.com/Unununiumwc3/Shattered-Legacies.git wl && cd wl
git sparse-checkout set src/WarcraftLegacies.Source/Ai src/WarcraftLegacies.Source/ShatteredLegacies tools/ai \
    src/MacroTools src/WarcraftLegacies.Source/Setup src/WarcraftLegacies.Shared \
    mapdata/WarcraftLegacies/Units mapdata/WarcraftLegacies/UnitData mapdata/WarcraftLegacies/AbilityData \
    src/WarcraftLegacies.CLI src/Warcraft.Cartographer
```
(`WarcraftLegacies.Shared` is needed by `gen_ai_data.py`; `git sparse-checkout add src/WarcraftLegacies.Source/GameLogic`
for the other marked files. `git mv` into a folder outside the sparse set fails -- add the folder first.)
(Cone mode also brings the files directly in the parent folders: `mapdata/WarcraftLegacies/PathingMap.json` and
`src/WarcraftLegacies.Source/Regions.g.cs` -- what `tools/ai/map_query.py` needs. Add `src/WarcraftLegacies.Source/Factions`
to read quest classes.)
The repo is huge (map data, LFS models); never clone it fully. Upstream WL for comparison:
`https://github.com/AzerothWarsLR/WarcraftLegacies` (`main` = 5.x).

## Files

| File | Class(es) | Contents |
|---|---|---|
| `AiSystem.cs` | SimpleBot, QuestAutoComplete, Geography, AiState, AiSetup | core loop (combat round-robin 0.5 s, production tick 1 s -- one bot per tick, defense, heroes, graph targeting, logging), quest automation, landmass grid, shared runtime collections, setup + version |
| `AiTech.cs` | AiTech | tech pools, building credits, free/event/quest upgrades, CP-capture counter |
| `AiMissions.cs` | AiMissions | starter kit, Ragnaros, kill targets, area clears, one-shots, CP grants, Kul Tiras repair, palace handover, banned forms |
| `AiLegion.cs` | AiLegion | Legion summon/betrayal, Argus start base, node events (Sunwell), demon portal fix, all AI portals |
| `AiWaveRunner.cs` | AiWaveRunner | invasion-wave runner; no waves configured since 0.37.1 (`AiWaves.All` empty) |
| `AiGates.cs` | AiGates | gate removal (+ new gates on entering the map), named graph gates (greymane, tomb, thandol, oldgods), Old Gods rift, Emerald Portals, graph merge |
| `AiAutopilot.cs` | AiAutopilot | `-autopilot` |
| `AiGoldVote.cs` | AiGoldVote | bot strength vote at 49 s (the old game-mode vote slot) (sets `SimpleBot.BotGoldBonus`) |
| `AiHeroVisits.cs` | AiHeroVisits | quests that need a hero at a place (Yogg-Saron, Tomb, Soul Gem, ...); config `AiConfig.HeroVisits` |
| `AiChoices.cs` | AiChoices | bot picks in WL's click menus (orc faction random, Scourge always invades); hook `MacroTools/UserInterface/BotChoicePolicy.cs` |
| `AiData.g.cs` | AiData | GENERATED (tools/ai/gen_ai_data.py): train lists, altar heroes, requirements, hero skill order of WL's factions -- re-run after WL updates |
| `AiRoutes.cs` | AiRoutes | route planner (0.42): live edge state (rocks every 15 s, waygates every 30 s, enemy structures / creeps per node area), territory Dijkstra -> target, route from the group's area, commit + 30-s re-plan; movement chains (0.44, backward route trees, 4 kept); leftover / creep targets (0.44.1/0.44.3); gather points (0.45) |
| `AiDefense.cs` | AiDefense | 0.43.2: which CP the army defends -- same landmass, within 9000, min 30 s, 45-s cooldown |
| `AiIncome.cs` | AiIncome | 0.44.2: pays computer slots the CP income WL pays only to human slots (`PlayerData.TotalIncome`, per second) |
| `AiPortals.cs` | AiPortals | 0.46: pseudo-portals (tickets, enter-region trigger per waygate, teleport + rest of the chain), Dark Portal edge, WL waygate dump |
| `AiLoad.cs` | AiAb, AiGuard, AiBudget, AiProbe, AiLoad | 0.46: per-bot A/B modes, guard positions, unit budget per CP, order-latency probe |
| `AiRoutes.g.cs` | AiRouteData | GENERATED (`tools/ai/gen_routes.py`, map data only): nodes (CPs + area nodes), walking edges, rock groups. Report: `ROUTES.md` |
| `AiArmy.cs` | AiArmy | 0.41: army orders -- attack group sized from the target, rest at the staging CP, orders only for idle units away from their point (`-orders smart`) |
| `AiScheduler.cs` | AiScheduler, AiTask | 0.40: the one timer for all periodic bot work (0.25-s slots, period + fixed phase per task), load counters and budget |
| `AiApi.cs` | AiApi | **the bot's public surface** for code outside `Ai/` (players, logging, `Safe`, build spots, data hooks `RegisterHeroes` / `AddProductionData`) |
| `AiHeroSkills.cs` | AiHeroSkills | bot heroes learn skills (level-up event + wave tick) |
| `AiStats.cs` | AiStats | what bots train (train-finish event) -> dump section TRAINED (0.38) |
| `AiHeroCare.cs` | AiHeroCare, AiStandings | stranded bot heroes (walk back / to the altar); team CP standings (dump TEAMS, 80/90 pins) |
| `AiConfig.cs` | AiConfig, AiGraph, AiWorld, ... | data: CP graph edges, portals, quest rules, kill targets, coordinates |
| `AiUpgrades.cs` | AiUpgrades | data: tech pools per faction, building upgrades, event researches |
| `AiDebug.cs` | AiDebug | chat commands, `-dump` writer, readouts, **the command switch `Access`** |
| `tools/ai/check_refs.py` | | cross-class refs + CSharp.lua limits (run before every build) |
| `CONCEPTS.md` | | designs: movement steps, theatres, pseudo-portals / ferries, unit budget, load measuring; gold, production places, quest requirements |
| `WORKAROUNDS.md` | | everything the bot needs outside `Ai/` (hooks, workarounds, runtime map changes, version-bound data) |
| `MEASUREMENTS.md` | | load numbers per version (from `dump_summary.py`) |
| `../ShatteredLegacies/` | ShatteredLegaciesSetup, FallbackConfig, FallbackSystem, KezanEnterprise, SlAiData | fork content: fallbacks, Kezan Enterprise; own `README.md`, `TODO.md`, `FALLBACKS.md`, `FACTION_UNITS.md` |
| `tools/ai/gen_routes.py` | | generates `AiRoutes.g.cs` (nodes, edges, rock groups, region map) + report `ROUTES.md` from map data only; `--without <ids>` |
| `tools/ai/render_map.py` | | draws terrain, CPs, area nodes and the route graph to `tools/ai/out/route_map.png` (never committed); `--regions` |
| `ROUTES.md` | | generated report: every edge with its "via" point, detours, edges behind rocks |
| `tools/ai/dump_summary.py` | | condenses one game's dumps into a block for `MEASUREMENTS.md` (`--append "note"`) |
| `tools/ai/lua_check.py` | | compile-check a built `war3map.lua` with Lua 5.3 |
| `tools/ai/scope_check.py` | | C# scope errors tree-sitter misses (local hides a type, CS0841; nested re-declaration, CS0136) |
| `tools/ai/gen_ai_data.py` | | writes `AiData.g.cs` (WL factions) and `../ShatteredLegacies/SlAiData.g.cs` (fork buildings, `SL_BUILDINGS`) from WL's ObjectInfo + map data (run after every WL update) |
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
6. Every periodic task is registered in `AiScheduler` (0.40; no new periodic timers) and runs through
   `SimpleBot.Safe(name, task)`; errors go to the pinned log once
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
11. **Unqualified calls across classes don't compile** (0.39.2 break, CS0103): a helper added to `SimpleBot` and
    called as `IsActive(...)` from `QuestAutoComplete` (same file, other class). `check_refs.py` reports these as
    UNQUALIFIED since 0.39.3.
12. **Keep file encodings**: WL's C#/JSON files start with a UTF-8 BOM (files in `Ai/` and `ShatteredLegacies/` don't).
    Keep whatever the file had. Python: read `utf-8-sig`, write the BOM back
    if it was there; `/bin/sh` is dash -- `printf '\xef'` writes the text `\xef`, not the byte. Compare BOMs with
    `git show HEAD:<file> | head -c 3 | od -An -tx1` before delivering.
13. **Layers** (0.39.10): nothing in `Ai/` may reference a class outside `Ai/` except WL's own code; fork code uses
    the bot only through `AiApi` (add a method there instead of reaching into `SimpleBot`). `check_refs.py`: LAYER, API.
14. **Markers**: every change in an original file gets `// [BOT]`, `// [SL]` or `// [WORKAROUND]` and an entry in
    `WORKAROUNDS.md` or `../ShatteredLegacies/README.md` in the same drop.
15. **Desync**: never base a decision on wall-clock time, local files or per-client text. Counters for budgets must
    be deterministic (units enumerated, orders issued).
16. **CSharp.lua: no compound assignment to an ARRAY element with a ternary / comparison / logic on the right**
    (0.44.3-0.45.0, silent TICK ERROR on every run): `a[i] += c ? x : y` became `a:get(i) + (c) and x or y` --
    arithmetic on a boolean. A plain variable (`food += c ? x : y`) was translated correctly. Compute the value into a
    local first. `check_refs.py` flags it ("INDEXER COMPOUND").

## How we work

- Session start: the maintainer pastes the repo/branch and a task; clone sparsely (command above), read this file
  and `TODO.md`. He compiles in a Windows VM (Winboat) and runs git on Ubuntu; no GitHub Actions.
- "Discuss only" = do not code. When coding: leave out what is unclear, put it in `TODO.md`, and say so.
- Two TODO files: `Ai/TODO.md` = bot work only (what would go into the upstream PR); `../ShatteredLegacies/TODO.md` =
  everything fork-only (fallbacks, Kezan, Thandol Treaty, map content, the fork's win rule, faction balance, the
  maintainer's brainstorming). Put new items in the right one.
- Before proposing a movement change, read `README.md` "Already tried"; add a row there when an approach is dropped
  or replaced (same drop).
- At most one question per reply; propose defaults instead of asking many.
- Push only named tags (`git push origin v<build>`), never `--tags` (old tags drag Git LFS into history the clone
  never downloaded).
- Deliver one patch per version (`git diff --binary -M` against the pushed branch, or against the previous patch
  of the same delivery), verified with `git apply --check` on a clean worktree of `origin/botified-dev`, plus a zip
  of the whole files with repo paths. The maintainer downloads them to `~/Downloads/` and runs the commands given in
  the reply (apply, commit, tag); he cannot run `.ps1` scripts in Windows, only paste PowerShell commands.
- Version on every code drop: `AiSetup.Build` (bottom of `AiSystem.cs`) AND `MapSettings.Version` in
  `src/WarcraftLegacies.CLI/appsettings.json` (`v<build>`; also in the published file name), `CHANGELOG.md`.
  Map title: "Shattered Legacies: Botified Realm v<build>" (name in `Info.json`, version appended on publish).
  Docs-only drop: CHANGELOG only. Release = commit + push + tag `v<build>` (commands in `README.md`, "Releasing").
- Keep `TODO.md` and `CHANGELOG.md` current in the same drop; TODO only holds open work.
- Measurements live in `MEASUREMENTS.md` (written by `tools/ai/dump_summary.py --append`), so a new chat sees them.
- Testing data comes as `-dump` files (`CustomMapData/WLAI_dump_<time>.txt`). `dump_summary.py` condenses a whole
  game; to read one dump in full, decode:
  ```
  python3 -c "import re,sys;[print(m.group(1)) for l in open(sys.argv[1],errors='replace') for m in [re.match(r'\s*call Preload\( \"(.*)\" \)',l)] if m]" dump.txt
  ```
  The header shows the build -- ignore dumps of another build. Read **PINNED** first (one-off events,
  `TICK ERROR`/`FAILED`), then **BOTS** (state, target, army), **TEAMS**, **PORTALS**, **EVENT LOG**.

## Map data (offline, no game needed)

- `mapdata/WarcraftLegacies/PathingMap.json`: 32-unit cells, rows bottom-up from (-28672, -32256), flag 0x02 not
  walkable / 0x04 not flyable / 0x08 not buildable / 0x40 NOT water. Destructables (trees, WL's `LTrc` rocks) and
  units are NOT in it -- `gen_routes.py` adds trees (64 x 64) and rock doodads; `LTrc` marks removable edges (+-192:
  +-96 left a gap at the Thandol bridge railing). `tools/ai/map_query.py` wraps it (walk / area / near / find / around / rect).
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

## Engine (WC3) limits -- the constraints behind the movement work

- **Order delay = the pathfinder queue.** Every path request (each order; each queued chain point when it becomes
  active) waits in one queue per game -- bots' and players' alike. Many units + many orders -> players' orders start
  1-2 s late (0.43.1 with 4000+ units, 0.44.2 with 5227). The game clock can run on time meanwhile.
- **Pathing is part of the synced simulation**: delays measured in GAME time are the same on every client
  (desync-safe), unlike wall-clock time.
- **Lua runs in the same frame as the simulation**: a Lua spike is a hitch for everything (0.44.1: 78k ops in one
  slot). Average Lua work is small next to thousands of units; the bot's main engine cost is the orders it issues.
  Op counters (0.40) = spike detector, not an engine-load measure.
- **No group movement**: every unit paths on its own. Units sent to the same point compete for it (crowds at nodes,
  bridges) -- 0.46 tests spreading the points.
- **Waygate teleports drop the queued orders** (0.44.1: 1048 units idle at exits) -> pseudo-portals (0.46).
- **Guard positions** (hypothesis, tested in 0.46): computer players' units walk back to a guard position after chasing
  an enemy; nothing in the code removes it (`RemoveAllGuardPositions` / `RemoveGuardPosition`).
- **Enter-region events fire only on entering**: a unit already inside the region when the trigger matters gets no
  event -- handle that case when the decision is made.
- **Hidden** (`ShowUnit` false) = out of the game world: no target, no collision, not revealed by true sight (not the
  invisibility mechanic). Range searches skip hidden units, player enumerations do not. Pair it with `PauseUnit` when
  units must not act at all (ferries).
- **Game speed**: when the simulation cannot keep up, single player slows down (turn counter behind a stopwatch);
  multiplayer shows lag screens.

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
- (OLD frontier picker only, `-route off` / fallback:) **Edge list membership defines a node's continent**: `ResolveEdgeSet` counts a faction's CPs per list (Combined /
  Kalimdor). An edge in the WRONG list turns its end node into a node of that continent (0.37.1: the rift edge in
  Combined made the Maw an EK node -> Black Empire walked the EK graph all game). Cross-continent edges belong in
  ONE list (the home side of the factions that use them) or are gated and handled by the edge-set logic.
- Graph gate flags (`Edge.Gate`) must be opened by code: `legion`/`prelegion` (AiLegion), `greymane`/`tomb`/`thandol`/
  `oldgods` (AiGates). `greymane` was never opened from v25 to 0.35.4 -- a new flag needs its opener.
- (OLD frontier picker only; the route planner walks the generated graph with closed rock edges:) A frontier can come
  from an ALLY's node on the far side of a barrier (Ironforge got Arathi through an ally node
  north of the closed bridge): the graph says reachable, the army can't get there. The stuck blacklist (4 min, then
  5/10/20 min skip since 0.37.2) limits the damage; real fix = walking costs (TODO, CP graph weights).
- Waygates: a landing spot on/near a building or preplaced unit lets flyers through but not ground units. Keep portal
  ends on open ground (`LandBeside` keeps 250 from buildings). Waygate n036 carries A01M/A023 "Anti Portal Blocking"
  (1000-range immolation); AI portals shrink it to 250.
- **Two graphs** (until phase 2): the route planner uses the GENERATED graph (`AiRoutes.g.cs`, drawn by
  `tools/ai/render_map.py`); the OLD hand-written graph (`AiConfig` edges) is only the fallback picker's -- but it is
  what `-edges` draws in game and what the dump's EDGES sections list. The maintainer's 0.46 edge list was made from
  the old graph; the generated one already matched 12 of its 13 items. Always say which graph you mean.
- **Dark Portal** (Outland <-> Blasted Lands): WL's Fel Horde quest "The Dark Portal" (`QuestDarkPortal`, global)
  completes at turn 10, or earlier by research R02C at Hellfire Citadel, never before turn 8. It then shows and
  activates three preplaced waygate pairs (n036, owner Neutral Hostile, hidden until then). The route graph reads
  active neutral waygates every 30 s -- yet the 0.45.0 dump at 540 min had no Dark Portal edge (cause open, 0.46
  lists WL's own waygates in the dump).
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
- Income/min = CP values (10-50 each) + `PlayerData.BonusIncome`, paid every second by `ControlPointManager` -- **to
  HUMAN slots only** (`EnumeratePlayers(Playing, User)`). Computer slots got nothing until 0.44.2 (`AiIncome` now pays
  them the same). Every faction's `StartingGold` adds `Income` to `BonusIncome` for 10 turns -> WL's early bonus ends
  at 11:00.
- The strength-vote bonus is a per-production-turn `SetPlayerState` (pays more the fewer bots there are). Scale since
  0.43.1: Recruit -13, Soldier -6, Veteran 0, Commander +13, Warlord +25. Before 0.44.2 it was the bots' ONLY income.

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
  (-17908,-6308). Old Gods rift ends: Maw of Gor'ma (`AiConfig.RiftMawX/Y`, -27768,-7864) and the Twilight Highlands
  end 100 north of the preplaced `n07E` (22545,-4684 since 0.45.0; `n07E` at -4784 is removed at opening).
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

- **Targeting** (default, `-route on`): `AiRoutes.Pick` -- Dijkstra from the bot's own nodes (territory) over the
  generated graph; candidates = CPs (neutral half cost, quest CPs on the front 0.3), enemy buildings in node areas,
  leftovers in own areas (+2500), creep camps (+3000); 10-min lock on player-owned targets, stuck blacklist, stickiness.
  Fallback / `-route off`: the old frontier picker `SimpleBot.PickGraphTarget` on the hand-written graph
  (`AiGraph.Combined` / `Kalimdor`, merged at the Legion summon) -- to be deleted once the planner is proven.
- **Movement** (`-chain on`): `AiArmy.Issue` -> `AiRoutes.Chain` (queued attack-moves along the route, waygate legs);
  the attack group / rest / staging logic in `AiArmy.Attack`; gather points `AiRoutes.UpdateGather`; defence
  `AiDefense.Pick` before targeting.
- **Bot bonuses**: `SimpleBot.HasPerks(p)` -- real bots and test-mode autopilot only.
- **Command switch**: `AiDebug.Access` (`Off` release / `HostOnly` / `Everyone`); levels Read / Heavy / Control.
- **Portals**: `AiGraph.Portals` (phases always / prelegion / legion / oldgods), created by `AiLegion`, watchdog
  rebuilds them. Target distance through portals: `SimpleBot.TravelDist2`. Edge weights: `Edge.Weight`.
- **Missions**: Ragnaros (`AiMissions`, real level-12 hero), hero visits (`AiHeroVisits`, `AiConfig.HeroVisits`),
  kill targets / area clears (`AiConfig.KillTargets`, `ClearAreas`, `RequireType`). Heroes on a mission are in
  `AiState.MissionUnits`. Stranded heroes: `AiHeroCare`. Bot menu picks: `AiChoices`.
- **Bot strength vote** (`AiGoldVote`, 49 s) sets `SimpleBot.BotGoldBonus`; free hero revive needs a living altar.
