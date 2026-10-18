# Warcraft Legacies – Bot AI

Computer-player bots for **Warcraft Legacies 4.8.2** (the version that still has the Old Gods). The bots capture
control points, fight, produce, research, follow faction quests and lore events (Legion summon, Ragnaros, ...).
Current version: see `AiSetup.Build` in `AiSystem.cs`, the in-game banner ("WL AI 0.36.0") and `CHANGELOG.md`.

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
| `-botbreak` `-prod` `-atk` | ⚠ | Toggle all bot logic / production / attacking |
| `-gold0` … `-gold200` | ⚠ | Gold every bot gets per production turn (default 25 ≈ 100 gold/min; Scourge/Legion 150 %) |
| `-legion` | ⚠ | Force the Legion summon (Argus hub, graph merge, betrayal) |
| `-ogwave` | ⚠ | Force the Old Gods invasion wave |
| `-ship` | ⚠ | Ship-portal asset test |
| `-autopilot` | always | See "Playing with bots" |

## The dump file

`call Preload( "…" )` wraps every line — send the file as it is. Sections:
**PINNED** (one-off events, `TICK ERROR`/`FAILED` lines — read first), **TEAMS** (CPs per team), **PORTALS**, **LOAD** (units touched per tick,
the multiplayer lag indicator), **BOTS** (target, reason, frontiers, army by landmass, heroes), **WAVES**,
**PERMADEAD**, **EVENT LOG** (last 300 changes), **NODES**, **EDGES**.

## Testing a build

1. Banner shows the new version. 2. No `SYSTEM ERROR` text in the first turns. 3. `-autodump` at the start;
afterwards PINNED has no `TICK ERROR`/`FAILED` and "portals: 7 pair(s) created (always)". 4. BOTS: states like
`atk`/`cap`/`def`, real army sizes. 5. `-portals` shows all pairs; `-legion` swaps Northrend for Felwood.

## Developer checks

Python, `pip install tree-sitter tree-sitter-c-sharp lupa --break-system-packages`:

- `python3 tools/ai/check_refs.py src/WarcraftLegacies.Source/Ai` — wrong-class/private references and the two
  CSharp.lua limits (Lua locals per class, initialized static fields per class). **Run before every build.**
- `python3 tools/ai/lua_check.py artifacts/war3map.lua` — compiles the built script with Lua 5.3; catches what
  makes Warcraft III call the map "unavailable or corrupted".
- `python3 tools/ai/map_query.py walk X Y` (also `near`, `area`, `find`, `around`, `rect`) — checks coordinates
  against the map's source data (terrain pathing, preplaced units, `Regions.X`) before they go into `AiConfig.cs`.

**CSharp.lua limits to remember:** all static members of a class become locals of one Lua function (limit 200),
and non-private static `readonly` fields are added uncounted; initialized static fields past ~55 per class stay
`nil`. Keep classes small; shared state as `internal static` without `readonly`.

## Working on this with an AI assistant

The files `AI_AGENTS_CONTEXT.md` (architecture, hard rules, workflow) and `TODO.md` let a fresh chat pick up the
project. Example prompt:

> I'm working on the Warcraft Legacies bot AI. Repo: https://github.com/lehmbein/WarcraftLegacies, branch
> `4.8.2-ai` (latest tag 4.8.2-ai.0.35.0). First read `src/WarcraftLegacies.Source/Ai/AI_AGENTS_CONTEXT.md` and
> `TODO.md`. Task: [what you want]. Attached: the latest dump files and screenshots. Discuss first, don't code yet.

## Versions

`0.MINOR.PATCH` — a feature build raises MINOR, a fix-only build raises PATCH. Git tags carry the WL base:
`4.8.2-ai.0.35.0`. Tag only after a successful playtest. History: `CHANGELOG.md`.

## Known WL bug fixed in this fork: `%` in quest/power text

`SYSTEM ERROR: … invalid use of '%' in replacement string` (Kul Tiras, Stormwind). WL's `Loc.Format` used
`string.Replace`, which in the Lua build treats `%` in the replacement as a pattern character, so a power text
with "35%" crashed the quest reward before it handed over units. Upstream: #4061 added a safe replacement, #4083
removed it (4.8.2 ships that), #4189 (5.0 beta, 253 files) restored it — not cherry-pickable alone, so this fork
carries the same small change in `Loc.cs` as its own commit.
