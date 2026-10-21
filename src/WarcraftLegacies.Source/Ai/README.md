# Warcraft Legacies – Bot AI

Computer-player bots for **Warcraft Legacies 4.8.2** (the version that still has the Old Gods). The bots capture
control points, fight, produce, research, follow faction quests and lore events (Legion summon, Ragnaros, ...).
Current version: see `AiSetup.Build` in `AiSystem.cs`, the in-game banner ("WL AI 0.37.1") and `CHANGELOG.md`.

## Installing into a WL fork

1. Copy this `Ai/` folder to `src/WarcraftLegacies.Source/Ai/`.
2. Add one line to `src/WarcraftLegacies.Source/Setup/GameSetup.cs`, directly after `Artifacts.Setup();`:
   ```csharp
   WarcraftLegacies.Source.Ai.AiSetup.Setup();
   ```
3. Recommended: the `%` fix in `src/MacroTools/Localization/Loc.cs` (see "Known WL bug" below).
4. Build as usual. Every computer slot in the lobby becomes a bot.

## Before you publish a build: the command switch

At the top of `AiDebug.cs`:

```csharp
public const CommandAccess Access = CommandAccess.Everyone;
```

| Setting | Effect |
|---|---|
| `Off` | **Release.** No AI chat command at all. `-autopilot` still works, but fair (see below). |
| `HostOnly` | Read-only commands for everyone; game-changing commands only for the host (lowest human slot). |
| `Everyone` | Every command for every player. **Testing only.** |

Heavy commands (`-debug1..4`, `-edges`, `-ids`, `-autodump`, `-tp`) only work when a single human is in the game.

## Playing with bots

- **Bot strength vote** 70 s into the game: Recruit (no bonus gold), Soldier (half), Veteran (normal), Commander
  (1.5x), Warlord (2x, may lag). Majority wins; ties go toward Veteran; no votes = Veteran.
- `-autopilot` hands your faction to the AI; type it again to take over. All players are told.
  - **Fair** (release build, `Access = Off`): the AI plays with your own resources — no bot bonuses, 60 s cooldown.
  - **Test** (`HostOnly`/`Everyone`): full bot treatment (gold subsidy, free techs, revives, missions, waves).
- Bots get bonuses humans don't: gold subsidy, free techs and upgrades, free hero revives, a starter barracks +
  altar, scripted missions (Ragnaros, Sapphiron, Murmur, area clears), invasion waves.

## Chat commands

Levels: **read** (everyone, cheap), **heavy** (single-human games only), **game-changing** ⚠ (host only in `HostOnly`).

| Command | Level | What it does |
|---|---|---|
| `-help` | read | Version, command access and the most used commands |
| `-dump` | read | Full snapshot to `Documents\Warcraft III\CustomMapData\WLAI_dump_<time>.txt` |
| `-why` / `-why <pid>` | read | One line per bot (state, target, reason) / detailed trace for one bot |
| `-portals` | read | Labels and pings every AI portal for 30 s; says whether the graphs are merged |
| `-heroes` | read | Labels and pings every living bot hero for 30 s (level, owner, mission / stranded state) |
| `-xy` | read | Position and landmass of your selected unit |
| `-id` | read | Type id, name, owner, position of your selected unit |
| `-fac` | read | Each bot's faction and its unit/building types per category |
| `-autodump` | heavy | Toggle: a dump every 5 minutes |
| `-edges` | heavy | Toggle: draws every graph edge (green held, yellow neutral frontier, orange uncapturable, red contested, blue neutral↔neutral, magenta crosses landmass, grey closed) |
| `-ids` | heavy | Every control point's id above it for 30 s |
| `-debug1` … `-debug4` | heavy | Repeating readouts: state / load / decisions / economy |
| `-tp x y` | heavy | Moves your selected units to x, y (testing) |
| `-icontest` | heavy | Six numbered minimap icon variants east of your selected unit (which one shows?) |
| `-botbreak` `-prod` `-atk` | ⚠ | Toggle all bot logic / production / attacking |
| `-gold0` … `-gold200` | ⚠ | Gold every bot gets per production turn (default 25 ≈ 100 gold/min; Scourge/Legion 150 %) |
| `-legion` | ⚠ | Force the Legion summon (Argus hub, graph merge, betrayal) |
| `-ogrift` | ⚠ | Force the Old Gods rift (Maw of Gor'ma <-> Twilight Highlands) open |
| `-ship` | ⚠ | Ship-portal asset test |
| `-autopilot` | always | See "Playing with bots" |

## The dump file

`call Preload( "…" )` wraps every line — send the file as it is. Sections:
**PINNED** (one-off events, `TICK ERROR`/`FAILED` lines — read first), **TEAMS** (CPs per team), **TRAINED** (what each bot trained / never trained), **PORTALS**, **LOAD** (units touched per tick,
the multiplayer lag indicator), **BOTS** (target, reason, frontiers, army by landmass, heroes), **WAVES**,
**PERMADEAD**, **EVENT LOG** (last 300 changes), **NODES**, **EDGES**.

## Testing a build

1. Lobby shows "Warcraft Legacies Botified <version>", the banner the same AI build. 2. No `SYSTEM ERROR` text in
the first turns. 3. `-autodump` at the start; afterwards PINNED has no `TICK ERROR`/`FAILED` and "portals: 8
pair(s) created (always)". 4. BOTS: states like `atk`/`cap`/`def`, real army sizes. 5. `-portals` shows all pairs;
`-legion` swaps Northrend for Felwood. The release-specific checks are in `TODO.md` ("Check in the next playtest").

## Map name
The AI branch builds as **"Warcraft Legacies Botified <version>"** ("Botified" in red, lobby and loading screen), so
it is never mistaken for the official map. Version = `MapSettings.Version` in `src/WarcraftLegacies.CLI/appsettings.json`
(e.g. `4.8.2-ai.0.38.1`), also used in the published file name. Bump it together with `AiSetup.Build`.

## Checking which units the bots build

1. Offline, what is possible: `python3 tools/ai/unit_scan.py <faction>` -- every unit/hero of the faction, where it
   is trained, and why the AI leaves it out. Classification is exact: buildings by WL's category, ships by their WC3
   naval base type, builders by WL's "Builder" category, researches by `UpgradeData`; "no building of this faction
   trains it" = quest/summon unit. 0.38: 248 trainable, 161 ships, 21 builders, 9 quest/summon units (439 total).
2. In the game: `-dump` -> section TRAINED, per bot "trained types x count" and "never (N): ...". A type in "never"
   that the scan marks OK means: requirement not met yet (often a quest -- the scan names it) or not reached yet.

## If the game freezes
So far freezes came only with alt-tabbing on Linux/Proton, picture frozen while sound kept playing -- most likely
the graphics layer, not the map. The steps to tell the two apart are in `TODO.md` ("Freeze check"). A map/script hang
also happens without alt-tabbing and, in multiplayer, freezes every player.

## Developer checks

Python, `pip install tree-sitter tree-sitter-c-sharp lupa --break-system-packages`:

- `python3 tools/ai/check_refs.py src/WarcraftLegacies.Source/Ai` — wrong-class/private references and the two
  CSharp.lua limits (Lua locals per class, initialized static fields per class). **Run before every build.**
- `python3 tools/ai/lua_check.py artifacts/war3map.lua` — compiles the built script with Lua 5.3; catches what
  makes Warcraft III call the map "unavailable or corrupted".
- `python3 tools/ai/scope_check.py src/WarcraftLegacies.Source/Ai <other changed .cs files>` — catches C# scope
  errors a syntax check misses (a local hiding a type name, e.g. `timer timer = ...`; a local re-declared in a nested
  block). Not a compiler, but it caught the 0.37.1 build break.
- `python3 tools/ai/gen_ai_data.py` — regenerates `AiData.g.cs` (train lists, hero skills ...) from WL's data; run
  after every WL update, then rebuild.
- `python3 tools/ai/unit_scan.py [faction|--summary]` — which units/heroes each faction has and whether the AI can
  train them (with the reason / the unlocking quest). Compare with the dump's TRAINED section.
- `python3 tools/ai/map_query.py walk X Y` (also `near`, `area`, `find`, `around`, `rect`) — checks coordinates
  against the map's source data (terrain pathing, preplaced units, `Regions.X`) before they go into `AiConfig.cs`.

**CSharp.lua limits to remember:** all static members of a class become locals of one Lua function (limit 200),
and non-private static `readonly` fields are added uncounted; initialized static fields past ~55 per class stay
`nil`. Keep classes small; shared state as `internal static` without `readonly`.

## Working on this with an AI assistant

The files `AI_AGENTS_CONTEXT.md` (architecture, hard rules, workflow) and `TODO.md` let a fresh chat pick up the
project. Example prompt:

> I'm working on the Warcraft Legacies bot AI. Repo: https://github.com/lehmbein/WarcraftLegacies, branch
> `4.8.2-ai` (latest tag: see `git tag`). First read `src/WarcraftLegacies.Source/Ai/AI_AGENTS_CONTEXT.md` and
> `TODO.md`. Task: [what you want]. Attached: the latest dump files and screenshots. Discuss first, don't code yet.

## Versions

`0.MINOR.PATCH` — a feature build raises MINOR, a fix/small build raises PATCH. Git tags carry the WL base:
`4.8.2-ai.0.38.1`. History: `CHANGELOG.md`.

## Releasing

On Ubuntu, in the repo, branch `4.8.2-ai`, after the map compiled (version = `AiSetup.Build` and
`MapSettings.Version` in `src/WarcraftLegacies.CLI/appsettings.json`, both already bumped in the delivered files):

```
git checkout 4.8.2-ai && git pull
unzip -o ~/Downloads/wl-ai-<build>.zip        # or: git apply ~/Downloads/ai-<build>.patch
git status                                    # only the files listed in the drop should show up
git add src/WarcraftLegacies.Source/Ai tools/ai src/MacroTools/UserInterface \
        src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs src/WarcraftLegacies.CLI/appsettings.json \
        src/WarcraftLegacies.Source/Setup/ControlPointSetup.cs \
        <the mapdata files listed in the drop>       # new files too: git status shows them as ??
git commit -m "AI <build>: <one line>"
git push origin 4.8.2-ai
git tag -a 4.8.2-ai.<build> -m "WL AI <build>"   # after the playtest
git push origin 4.8.2-ai.<build>
```
Never `git add -A` blindly: the build writes published maps and artifacts into the working tree.

## Known WL bug fixed in this fork: `%` in quest/power text

`SYSTEM ERROR: … invalid use of '%' in replacement string` (Kul Tiras, Stormwind). WL's `Loc.Format` used
`string.Replace`, which in the Lua build treats `%` in the replacement as a pattern character, so a power text
with "35%" crashed the quest reward before it handed over units. Upstream: #4061 added a safe replacement, #4083
removed it (4.8.2 ships that), #4189 (5.0 beta, 253 files) restored it — not cherry-pickable alone, so this fork
carries the same small change in `Loc.cs` as its own commit.
