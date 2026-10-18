# Context for AI assistants (vibecoding handover)

Read this first, then `TODO.md`. Humans: the normal docs are in `README.md`; "AI" in file and class names means
the **bots**, this file is about **AI assistants** working on the code.

## The project in one paragraph

Computer-player bots for **Warcraft Legacies 4.8.2** (a Warcraft III Reforged map written in C# with WCSharp and
transpiled to Lua by **CSharp.lua**). Kept on 4.8.2 because 5.x removed the Old Gods. Fork:
`https://github.com/lehmbein/WarcraftLegacies`, branch `4.8.2-ai`, tags `4.8.2-ai.0.MINOR.PATCH`. All bot code is
in `src/WarcraftLegacies.Source/Ai/`; the only hook into WL is one line in `Setup/GameSetup.cs`
(`WarcraftLegacies.Source.Ai.AiSetup.Setup();` after `Artifacts.Setup();`). One fork-wide WL fix: `Loc.cs`
literal replace (0.34.3).

## Getting the code (sandbox)

```
GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none --sparse --branch 4.8.2-ai \
    https://github.com/lehmbein/WarcraftLegacies.git wl && cd wl
git sparse-checkout set src/WarcraftLegacies.Source/Ai tools/ai src/MacroTools
```
The repo is huge (map data, LFS models); never clone it fully. Upstream WL for comparison:
`https://github.com/AzerothWarsLR/WarcraftLegacies` (`main` = 5.x).

## Files

| File | Class(es) | Contents |
|---|---|---|
| `AiSystem.cs` | SimpleBot, QuestAutoComplete, Geography, AiState, AiSetup | core loop (combat round-robin 0.5 s, production 15 s, defense, heroes, graph targeting, logging), quest automation, landmass grid, shared runtime collections, setup + version |
| `AiTech.cs` | AiTech | tech pools, building credits, free/event/quest upgrades, CP-capture counter |
| `AiMissions.cs` | AiMissions | starter kit, Ragnaros, kill targets, area clears, one-shots, CP grants, Kul Tiras repair, palace handover, banned forms |
| `AiLegion.cs` | AiLegion | Legion summon/betrayal, Argus start base, node events (Sunwell), demon portal fix, all AI portals |
| `AiWaveRunner.cs` | AiWaveRunner | runs the scripted invasion waves (wave data: class `AiWaves` in `AiConfig.cs`) |
| `AiGates.cs` | AiGates | gate removal, Tempest Reach edge, Thandol bridge |
| `AiAutopilot.cs` | AiAutopilot | `-autopilot` |
| `AiGoldVote.cs` | AiGoldVote | bot strength vote at 70 s (sets `SimpleBot.BotGoldBonus`) |
| `AiHeroVisits.cs` | AiHeroVisits | quests that need a hero at a place (Yogg-Saron, Fel Horde Shadowmoon); config `AiConfig.HeroVisits` |
| `AiConfig.cs` | AiConfig, AiGraph, AiWorld, ... | data: CP graph edges, portals, quest rules, kill targets, coordinates |
| `AiUpgrades.cs` | AiUpgrades | data: tech pools per faction, building upgrades, event researches |
| `AiDebug.cs` | AiDebug | chat commands, `-dump` writer, readouts, **the command switch `Access`** |
| `tools/ai/check_refs.py` | | cross-class refs + CSharp.lua limits (run before every build) |
| `tools/ai/lua_check.py` | | compile-check a built `war3map.lua` with Lua 5.3 |

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

## WL facts that bit us

- `ControlPointManager.Instance.GetFromUnitType()` **throws** for non-CP ids -> always `SimpleBot.SafeCp()`.
- The Sunwell (`n001`) and Sunwell Corrupted (`n079`) are **capitals**, not CPs.
- Player slots 24+ (neutrals) also report `PLAYER_SLOT_STATE_PLAYING`; only 0-23 are real players.
- `Geography.Landmass()` returns -1 for islands (and 9 Kalimdor CPs).
- `LegendaryHero.PermaDies` is set-only; detect death via `LegendaryHero.Died` or ability `LEgo`.
- Gates are lowercase-h custom units; gate destructables use big-endian FourCC.
- WL removes Neutral-Passive units from whole areas at runtime -> AI-created portals are owned by Neutral Extra (26).
- Win threshold 90 CPs per team (warning at 80).
- Map coordinates given by the maintainer have contained sign typos 3 times -- check walkability/landmass first.
- Waygates: a landing spot on/near a building or preplaced unit lets flyers through but not ground units. Keep
  portal ends on open ground (`LandBeside` keeps 250 from buildings).
- Some heroes can't walk (the Old Gods' level-20 god heroes). Anything that sends "the best hero" must filter
  `GetUnitMoveSpeed(u) > 1` (`SimpleBot.BestHero` does).
- Quests a hero must visit are `ObjectiveHeroWithLevelInRect` / `...ReachRect` in WL; read the level and the
  `Regions.X` rectangle from the quest class (Ragnaros 10 in WL, our rule 12; Yogg-Saron 14; Tomb of Sargeras 10).
- WL's map name is hard-coded in `src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs` (~line 121);
  the version comes from `src/WarcraftLegacies.CLI/appsettings.json` (`MapSettings.Version`).
- The repo has a second map, `mapdata/WarcraftLegacies24p` ("Warcraft Legacies 1.18a", 24 slots). It is an OLD,
  unmaintained layout: 20 of our 167 graph nodes are missing there and 65 sit elsewhere. Not supported.
- Bot economy depends on the bonus gold: with the vote on "Recruit" (0) most bots had 0-12 gold and 1-5 units
  from minute 30 on. The Legion summon (Scourge takes the Sunwell) did not happen in 40-85-min tests.

## How we work

- Session start: the maintainer pastes the repo/branch and a task; clone sparsely (command above), read this file
  and `TODO.md`. He compiles in a Windows VM (Winboat) and runs git on Ubuntu; he does NOT want GitHub Actions.
- Decode dumps quickly (each line is `call Preload( "..." )`):
  ```
  python3 -c "import re,sys;[print(m.group(1)) for l in open(sys.argv[1],errors='replace') for m in [re.match(r'\s*call Preload\( \"(.*)\" \)',l)] if m]" dump.txt
  ```
  The header line shows the build -- old dumps get mixed in; ignore those of another build.
- Freezes on the maintainer's Linux/Proton setup came with alt-tabbing (with and without autopilot): not an AI bug
  until proven otherwise.

- The maintainer often says "discuss only" -- then do not code. When coding: leave out what is unclear and say so.
- Ask at most one question per reply; propose defaults instead of asking many.
- Version bump on every code drop: `AiSetup.Build` (bottom of `AiSystem.cs`), `CHANGELOG.md`, git tag
  `4.8.2-ai.<version>` (tag only after the playtest). A docs-only drop bumps only `CHANGELOG.md`/tag
  (0.35.4: the in-game banner stays 0.35.3).
- Before delivering code: `check_refs.py` must print OK (refs, duplicate classes, Lua locals, static fields);
  tree-sitter syntax check; list exactly which files changed.
- Deliver whole files; the maintainer copies them into the repo and builds in a Windows VM.
- Testing data comes as `-dump` files (`CustomMapData/WLAI_dump_<time>.txt`, lines wrapped in `call Preload("...")`).
  Read the **PINNED** section first (one-off events and `TICK ERROR`/`FAILED` lines), then **BOTS** (state, target,
  army per bot), **PORTALS**, **EVENT LOG**.
- Keep `TODO.md` and `CHANGELOG.md` current in the same drop.

## Key mechanisms (where to look)

- **Targeting**: CP graph (`AiGraph.Combined`, `AiGraph.Kalimdor`, merged at the Legion summon), frontiers, home
  graph first, neutral bias 0.5, 10-min lock on player-owned CPs, stuck blacklist (4 min). `SimpleBot.PickGraphTarget`.
- **Bot bonuses**: `SimpleBot.HasPerks(p)` -- real bots and test-mode autopilot only.
- **Command switch**: `AiDebug.Access` (`Off` release / `HostOnly` / `Everyone`); levels Read / Heavy / Control.
- **Portals**: `AiGraph.Portals` (phase always / prelegion / legion), created by `AiLegion`, watchdog rebuilds them.
  Target distance through portals: `SimpleBot.TravelDist2` (portal-aware since 0.35.2). Edge weights: `Edge.Weight`.
- **Missions**: Ragnaros (`AiMissions`, real level-12 hero), hero visits (`AiHeroVisits`, config
  `AiConfig.HeroVisits`), kill targets / area clears (`AiConfig.KillTargets`, `ClearAreas`).
- **Bot strength vote** (`AiGoldVote`, 70 s) sets `SimpleBot.BotGoldBonus`; free hero revive needs a living altar.
