# Bot AI – Shattered Legacies: Botified Realm

Computer-player bots for **Shattered Legacies: Botified Realm**, an unofficial fork built on **Warcraft Legacies
4.8.2** (the version that still has the Old Gods). The bots capture
control points, fight, produce, research, follow faction quests and lore events (Legion summon, Ragnaros, ...).
Current version: see `AiSetup.Build` in `AiSystem.cs`, the `-version` chat command and `CHANGELOG.md`.

## Installing into a WL fork

1. Copy this `Ai/` folder to `src/WarcraftLegacies.Source/Ai/`.
2. Add one line to `src/WarcraftLegacies.Source/Setup/GameSetup.cs`, directly after `Artifacts.Setup();`:
   ```csharp
   WarcraftLegacies.Source.Ai.AiSetup.Setup(); // [BOT]
   ```
3. The other hooks and what still ties the bot to this fork's map are listed in `WORKAROUNDS.md` (bot picks in click
   menus, leaver takeover, the `%` fix in `Loc.cs`).
4. Build as usual. Every computer slot in the lobby becomes a bot.
5. Fork code talks to the bot only through `AiApi.cs` (this fork: `../ShatteredLegacies/`).

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

- **Bot strength vote** 49 s into the game (0.43.1 scale): Recruit (bots lose ~50 gold/min), Soldier (lose ~25),
  Veteran (no bonus gold), Commander (+~50), Warlord (+~100, the old Veteran; may lag). Majority wins; ties go toward
  Veteran; no votes = Veteran. Bots still get free techs, heroes and revives.
- `-autopilot` hands your faction to the AI; type it again to take over. All players are told.
  - **Fair** (release build, `Access = Off`): the AI plays with your own resources — no bot bonuses, 60 s cooldown.
  - **Test** (`HostOnly`/`Everyone`): full bot treatment (gold subsidy, free techs, revives, missions, waves).
- Bots get bonuses humans don't: gold subsidy, free techs, upgrades and heroes, free hero revives, starter production
  buildings, scripted missions (Ragnaros, hero visits, kill targets, area clears).

## Chat commands

Levels: **read** (everyone, cheap), **heavy** (single-human games only), **game-changing** ⚠ (host only in `HostOnly`).

| Command | Level | What it does |
|---|---|---|
| `-version` | always | Map name and bot build (works in every build, also with `Access = Off`) |
| `-help` | read | Version, command access and the most used commands |
| `-dump` | read | Full snapshot to `Documents\Warcraft III\CustomMapData\WLAI_dump_<time>.txt` |
| `-why` / `-why <pid>` | read | One line per bot (state, target, reason) / detailed trace for one bot |
| `-portals` | read | Labels and pings every AI portal for 30 s; says whether the graphs are merged |
| `-heroes` | read | Labels and pings every living bot hero for 30 s (level, owner, mission / stranded state) |
| `-xy` | read | Position and landmass of your selected unit |
| `-id` | read | Type id, name, owner, position of your selected unit |
| `-fac` | read | Each bot's faction and its unit/building types per category |
| `-autodump` | heavy | Toggle: a dump every 5 minutes |
| `-edges` | heavy | Toggle: draws every edge of the OLD hand-written graph (the route planner's generated graph: `tools/ai/render_map.py`) (green held, yellow neutral frontier, orange uncapturable, red contested, blue neutral↔neutral, magenta crosses landmass, grey closed) |
| `-ids` | heavy | Every control point's id above it for 30 s |
| `-debug1` … `-debug4` | heavy | Repeating readouts: state / load / decisions / economy |
| `-tp x y` | heavy | Moves your selected units to x, y (testing) |
| `-icontest N` | heavy | Minimap icon variant N (1-6) on your selected unit (which one shows?) |
| `-orders smart\|idle\|all` | control | Army orders: smart = attack group + rest at the staging CP (default, 0.41); idle = 0.39-0.40 (whole army, idle units re-ordered); all = before 0.39 |
| `-creeps on\|off` | control | Creep camps in own areas as targets (default, 0.44.3; one-shots off) or the hand-placed one-shots (0.44.2) |
| `-gather on\|off` | control | Gather points: routes longer than 5 nodes wait at the last own node (80 % of the group or 60 s) (default, 0.45) |
| `-chain on\|off` | control | Army movement: chains of queued attack-moves along the graph, waygates clicked (default, 0.44) or one order to the target (0.43) |
| `-route on\|off` | control | Targeting: route planner on the generated graph with waypoints (default, 0.42) or the old frontier graph (0.41) |
| `-budget on\|off` | control | Load budget: report only (default) or let deferrable tasks slide after a busy slot (test) |
| `-botbreak` `-prod` `-atk` | ⚠ | Toggle all bot logic / production / attacking |
| `-gold0` … `-gold200` | ⚠ | Gold every bot gets per production turn (default 25 ≈ 100 gold/min; Scourge/Legion 150 %) |
| `-legion` | ⚠ | Force the Legion summon (Argus hub, graph merge, betrayal) |
| `-ogrift` | ⚠ | Force the Old Gods rift (Maw of Gor'ma <-> Twilight Highlands) open |
| `-ship` | ⚠ | Ship-portal asset test |
| `-autopilot` | always | See "Playing with bots" |

## The dump file

`call Preload( "…" )` wraps every line — send the file as it is. Sections:
**PINNED** (one-off events, `TICK ERROR`/`FAILED` lines — read first), **UNITS** (units per player, orders), **TEAMS** (CPs per team), **TRAINED** (what each bot trained / never trained), **PORTALS**, **LOAD** (units touched per tick,
the multiplayer lag indicator; since 0.40 also the scheduler, orders by distance and one line per periodic task; since
0.41-0.44 the army orders line (change / idle-away / rest / refill / exit, chains, stalled %), switches per bot, bot
income paid), **ROUTES** (0.42+: per bot target / score / front / next node; waygates, creep camps, gather counters),
**BOTS** (target, reason, frontiers, army by landmass, heroes), **WAVES**,
**PERMADEAD**, **EVENT LOG** (last 300 changes), **NODES**, **EDGES**.

## Measuring a game

`python3 tools/ai/dump_summary.py <the game's WLAI_*.txt files> --append "<note>"` writes a short summary (units,
orders/min, LOAD, errors at 10/20/30/45 min) to `MEASUREMENTS.md`, so the numbers stay in the repo. Details there.

## Testing a build

1. Lobby shows "Shattered Legacies: Botified Realm v<version>", `-version` the same bot build. 2. No `SYSTEM ERROR` text in
the first turns. 3. `-autodump` at the start; afterwards PINNED has no `TICK ERROR`/`FAILED` and "portals: 8
pair(s) created (always)". 4. BOTS: states like `atk`/`cap`/`def`, real army sizes. 5. `-portals` shows all pairs;
`-legion` swaps Northrend for Felwood. The release-specific checks are in `TODO.md` ("Check in the next playtest").

## Map name
The map builds as **"Shattered Legacies: Botified Realm v<version>"** (lobby and loading screen; name in
`mapdata/WarcraftLegacies/Info.json`, the version is appended on publish), so it is never mistaken for the official
Warcraft Legacies. Version = `MapSettings.Version` in `src/WarcraftLegacies.CLI/appsettings.json` (e.g. `v0.39.9`).
Published file: `maps/published/ShatteredLegacies-BotifiedRealm-v<version>.w3x`. Bump it together with `AiSetup.Build`.

## Checking which units the bots build

1. Offline, what is possible: `python3 tools/ai/unit_scan.py <faction>` -- every unit/hero of the faction, where it
   is trained, and why the AI leaves it out. Classification is exact: buildings by WL's category, ships by their WC3
   naval base type, builders by WL's "Builder" category, researches by `UpgradeData`; "no building of this faction
   trains it" = quest/summon unit. 0.38: 248 trainable, 161 ships, 21 builders, 9 quest/summon units (439 total).
2. In the game: `-dump` -> section TRAINED, per bot "trained types x count" and "never (N): ...". A type in "never"
   that the scan marks OK means: requirement not met yet (often a quest -- the scan names it) or not reached yet.

## If the game freezes
With `-autodump` on, `CustomMapData\WLAI_heartbeat.txt` is rewritten every 10 s (time, units on the map, orders
per minute, last tasks) -- send it with the dumps after a freeze.
So far freezes came only with alt-tabbing on Linux/Proton, picture frozen while sound kept playing -- most likely
the graphics layer, not the map. The steps to tell the two apart are in `TODO.md` ("Freeze check"). A map/script hang
also happens without alt-tabbing and, in multiplayer, freezes every player.

## Developer checks

Python, `pip install tree-sitter tree-sitter-c-sharp lupa --break-system-packages`:

- `python3 tools/ai/check_refs.py` — wrong-class/private references, the two CSharp.lua limits (Lua locals per
  class, initialized static fields per class) and the layer rule (LAYER: the bot uses fork code; API: fork code
  bypasses `AiApi`). Checks `Ai/` and `ShatteredLegacies/`. **Run before every build.**
- `python3 tools/ai/lua_check.py artifacts/war3map.lua` — compiles the built script with Lua 5.3; catches what
  makes Warcraft III call the map "unavailable or corrupted".
- `python3 tools/ai/scope_check.py src/WarcraftLegacies.Source/Ai <other changed .cs files>` — catches C# scope
  errors a syntax check misses (a local hiding a type name, e.g. `timer timer = ...`; a local re-declared in a nested
  block). Not a compiler, but it caught the 0.37.1 build break.
- `python3 tools/ai/gen_ai_data.py` — regenerates `AiData.g.cs` (train lists, hero skills ...) from WL's data, and
  `../ShatteredLegacies/SlAiData.g.cs` for the fork's own buildings; run after every WL update, then rebuild.
- `python3 tools/ai/render_map.py [--regions]` — draws terrain, CPs and the route graph to `tools/ai/out/route_map.png`
  (for discussing the graph; run after `gen_routes.py`).
- `python3 tools/ai/gen_routes.py` — regenerates the route graph `AiRoutes.g.cs` from map data (CPs, pathing,
  trees, rocks) and writes the report `ROUTES.md`; run after map changes. `--without <ids>` shows the graph as if
  those CPs did not exist.
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

> I'm working on the Shattered Legacies bot AI. Repo: https://github.com/Unununiumwc3/Shattered-Legacies, branch
> `botified-dev` (latest tag: see `git tag`). First read `src/WarcraftLegacies.Source/Ai/AI_AGENTS_CONTEXT.md` and
> `TODO.md`. Task: [what you want]. Attached: the latest dump files and screenshots. Discuss first, don't code yet.

## Versions

`0.MINOR.PATCH` — a feature build raises MINOR, a fix/small build raises PATCH. Each part is a whole number, not a
decimal: 0.39.9 -> 0.39.10 -> 0.39.11 (no leading zeros). `git tag --sort=v:refname` lists them in the right order. Git tags: `v0.39.9` (builds before
0.39.9 were tagged `4.8.2-ai.<build>` in the deleted fork). History: `CHANGELOG.md`.

## Releasing

Git runs on Ubuntu, the map is compiled in the Windows VM (Winboat). Repo:
`https://github.com/Unununiumwc3/Shattered-Legacies`, branch `botified-dev`.

1. Bump the version in **both** places: `AiSetup.Build` (bottom of `AiSystem.cs`) and `MapSettings.Version` in
   `src/WarcraftLegacies.CLI/appsettings.json` (with a leading `v`, e.g. `v0.39.9`). Add the `CHANGELOG.md` entry.
2. Compile in Windows (paste into PowerShell):
   ```powershell
   $repo  = "C:\Users\leif\Desktop\Shared\WarcraftLegacies"
   $start = Get-Date
   dotnet build "$repo\src\WarcraftLegacies.CLI" -c Debug
   cd "$repo\src\WarcraftLegacies.CLI\bin\Debug\net10.0"
   dotnet WarcraftLegacies.CLI.dll json-to-w3x publish WarcraftLegacies
   Get-ChildItem "$repo\maps\published\*.w3x" | Where-Object { $_.LastWriteTime -ge $start } | Copy-Item -Destination "C:\Users\leif\Desktop\Shared\" -Force -PassThru
   cd $repo
   ```
3. Playtest. `-version` in chat must show the new build.
4. Commit, push, tag (Ubuntu):
   ```
   git add <changed files>        # check with git status first
   git commit -m "<version>: <one line>"
   git push
   git tag -a v<version> -m "Shattered Legacies: Botified Realm v<version>"
   git push origin v<version>
   ```

## Known WL bug fixed in this fork: `%` in quest/power text (`[BOT]` in `Loc.cs`)

`SYSTEM ERROR: … invalid use of '%' in replacement string` (Kul Tiras, Stormwind). WL's `Loc.Format` used
`string.Replace`, which in the Lua build treats `%` in the replacement as a pattern character, so a power text
with "35%" crashed the quest reward before it handed over units. Upstream: #4061 added a safe replacement, #4083
removed it (4.8.2 ships that), #4189 (5.0 beta, 253 files) restored it — not cherry-pickable alone, so this fork
carries the same small change in `Loc.cs` as its own commit.
