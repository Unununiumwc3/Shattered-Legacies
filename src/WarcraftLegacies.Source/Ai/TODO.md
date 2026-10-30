# TODO -- bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Finished items go to
`CHANGELOG.md`; ideas still under discussion live in `CONCEPTS.md`; fork content (fallbacks, Kezan, map design) in
`../ShatteredLegacies/TODO.md`.

Current build: **0.45.0** (not playtested). **Focus: MOVEMENT.** Balance observations are collected below and decided
later (other changes will shift balance anyway).

## Playtest status

| Version | Played? | What it showed |
|---|---|---|
| 0.39.10 | yes (20 min, too much gold by mistake) | baseline; old 0.39.9 dumps mixed in |
| 0.40.0 | no (only inside later builds) | -- |
| 0.41.0 | yes (game B, 20 min) | attack group + staging work; "hold" order flood found |
| 0.42.0 | yes (game C) | routes from the straight-line nearest node, re-plans every service: back and forth |
| 0.42.1 | no (shipped with 0.43.0) | -- |
| 0.43.0 | yes | whole teams marched to one front (allied land counted as territory) |
| 0.43.1 | yes (Veteran = 0) | attack/defence flapping across continents; map "empty" |
| 0.43.2 | yes (Commander) | Thandol fixed; orcs along the Broken Isles |
| 0.44.0 | yes | chains work (long orders 4482 -> 615, stalled 2-5 %); planning spikes |
| 0.44.1 | yes (chain off 40 min; chains 9.5 h) | 78k-op spikes; 1048 units idle at waygate exits; stalemate |
| 0.44.2 | yes (14 min) | **first build with CP income for bots**; spikes gone (max-slot 5207); rest churn |
| 0.44.3 | **no** | creep targets, rest churn fix, exit re-chain |
| 0.45.0 | **no** | gather points, rift east end moved 100 north |

**Every game before 0.44.2 ran without CP income for bots** (WL pays human slots only): gold, army size and balance
observations from those games are void. Movement observations stay valid.

## Next playtest (0.45.0)

Veteran, chains on, autopilot allowed (say which faction), `-autodump`, 30-40 min, empty `CustomMapData` first.
- **Rift**: `-ogrift`, then a unit of yours through the EAST end (Twilight Highlands, now 22545,-4684) and back. Does
  it work now? The west end (Maw) worked.
- **Gather points**: long routes wait at the last own node (ROUTES line "gather ... started / assembled / timeout")?
  Do waves look better than single units arriving? `-gather off` for comparison.
- **Creeps**: bots clear camps in their own areas (ROUTES "creep camps N")? Not before neutral CPs?
- **Rest churn**: army line "rest=" much lower than "change="?
- **Waygate exits**: army line "exit=" counts units continuing after a teleport; no crowds at exits.
- Flapping: "switches per bot" (target / state) -- weak factions switched ~1/min in 0.44.0.

## Movement (next steps -- `CONCEPTS.md` section 0)

- [ai] **Step 2b, extend or re-order**: when the target changes during a chain (an ally took it), continue from the
  chain's end if that costs <= ~1.2 x the direct route, else a new chain. Today: a new chain at once. Decide after
  watching 0.45.0.
- [ai] **Step 3, several groups per bot** (2-3), only when the army is clearly bigger than one target needs;
  newcomers chain to the gather point and merge there.
- [you/ai] **Waygate hop cost** (300 = one short walk): keep, or raise so ship trips to dead-end island chains are a
  commitment (Warsong along the Broken Isles, 0.43.2). Maintainer unsure; decide with ROUTES waygate data.
- [ai] Queued chains are lost in the teleport (WC3) -- 0.44.3 re-chains at the exit; if units still crowd there, give
  the post-portal part as a separate chain right after the click.
- [ai] Defence still flaps for weak factions under pressure (~1 attack <-> defence switch per minute, 0.44.0).
  Options: longer hold, defence only for CPs worth more than the current target, or "defend" = the rest at staging.

## Graph (generated from map data -- `tools/ai/gen_routes.py`, check with `tools/ai/render_map.py`)

- [ai] **Graph independent of CPs** (for the upstream PR): nodes from terrain only, CPs as targets inside node areas;
  Shattered Legacies CPs and the workaround CPs then do not shape the graph.
- [ai] **Pass nodes**: generate nodes at narrow passages (mountain passes) and on long edges.
- [ai] **Exceptions file** for what map data cannot know (invisible blockers, gate units, quest-only passages), listed
  in `WORKAROUNDS.md`. Maintainer can mark nodes / edges on `route_map.png` in a fixed colour (pixel -> map:
  x = -28672 + 16 px, y = -32256 + 16 (3584 - py)); read by program, not by eye.
- [you] **Gnomeregan / Ironforge / Blackrock Depths** sit inside "no walk, no fly" rings (black on the map image) with
  one gap: dwarves walked into Gnomeregan repeatedly (0.43.1). Check in game whether the gap is real.
- [you] The 7 detours in `ROUTES.md` ("detour Nx"): blocked in game or a real way around?
- [ai] Remove the workaround CPs W1-W4 (`WORKAROUNDS.md`) once area nodes + creep / leftover targets show the bots
  attack those areas anyway (every W1-W4 spot has an area node within 1800).
- [ai] Delete the old frontier picker, edge sets, merge, goals, doctrine steps once the route planner has run a few
  games without falling back (`-route off` keeps it for A/B until then).

## Missions -> general logic

- [ai] **Area clears and kill targets** (`AiConfig.ClearAreas`, `KillTargets`): check each in a game with creep
  targets on (0.44.3), then remove the ones the general logic covers. Sapphiron / Murmur timing = design: keep until
  the maintainer decides. One-shots are already skipped (`-creeps off` brings them back).
- Hero visits stay (quests).
- [ai] **Quest requirements** (`CONCEPTS.md` 4): a small `[BOT]` hook so WL objectives expose their requirements
  read-only; one bot handler per objective type (CP, hero + level + place, build in area, research). Would replace the
  Kul Tiras workaround (buildings placed, quest given).

## Bugs and checks

- [ai] **SYSTEM ERROR at the plague**: "Scourge failed to execute OnQuestProgressChanged: Tried to call DropAllItems
  on a summoned hero" (0.43.1). Check whether a bot-made hero counts as summoned.
- [ai] **Lordaeron had a ship** (0.43.1) -- bots should not build ships. Find the source.
- [ai] Bots without an altar never revive heroes (same rule as players). With income bases should hold; otherwise
  rebuild an altar at a held CP (see production follow-ups).
- [ai] Orcs never take the Warsong path (0.39.9): check the orc choice in `AiChoices`.
- [ai] Legion summoned while the plague support had moved its army to Alterac (0.38); Argus not taken in 520 min;
  Eye of Sargeras never picked up. Re-check after the movement work.
- [you] Free heroes for a human? (0.38: the human Lordaeron got heroes for free.)

## Production follow-ups

- [ai] Units of buildings a bot doesn't have stay "never" (Quel'thalas' Academy / Consortium, Kul Tiras', Stormwind's
  special units). Proposal: the starter kit also gives each bot its "Specialist" building(s).
- [ai] Quest-locked heroes and units: `unit_scan.py` lists which quest unlocks what.
- [ai] Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- [ai] Hero care teleports often (~40 in 65 min; Warsong's Chieftain 6x from -9952,1944).
- [ai] Autocast for bot units / heroes.

## Balance and economy (collected -- decide later, `CONCEPTS.md` 2-3)

- [you] **Strength scale**: re-decide with bots that have income (0.44.2+). Veteran 0 may be right now.
- [you/ai] Bonus gold in bursts / gold depot (~1000 gold, dynamic) / load governor on the bonus once the stalled-units
  counter shows an overload threshold (0.44.1: stalled 2-10 % at ~2500 units -- no overload yet).
- [you] Where bots produce (starting region vs near the front).
- Observations from games WITHOUT bot income (void for balance, kept for reference): Old Gods took Kalimdor (0.44.0),
  most bots had armies of 1-10 food (0.43.x-0.44.1).
- [you/ai] CP towers vs bots: fully upgraded CP towers beat small bot armies.
- [you] The South Alliance / Naga balance, Old Gods strength -> `../ShatteredLegacies/TODO.md`.

## Freeze check (still open)

- Freezes so far only with alt-tabbing on Linux/Proton (0.36.0, 0.37.1); 0.38.1 froze at ~34 min without alt-tabbing.
  After a freeze send `CustomMapData\WLAI_heartbeat.txt` with the dumps.

## Decisions needed

- [ai] Guardian summons (decided, not built): a channelling quest hero that gets attacked pulls the 24 nearest army
  units (once per minute); HQ last stand the same (once per 5 min, HQ invulnerable during the teleport).
- [you] Kul Tiras Highbank last stand?
- [you] Argus with mixed owners: the pre-summon portal stays open for everyone without a summon. Keep it?
- [you] Broken Isles reached only by the AI ship Azshara Coast <-> Azsuna -- a real waygate humans use too.

## Smaller work

- [ai] `map_query.py` -> `map_audit.py`: check every coordinate in `AiConfig.cs` in one run.
- [ai] Debug: `-scout`, winner record when WL declares victory.
- [ai] Gate sweep stays; check in playtests which gates survive (`-id`, then `AiGraph.GateTypes`).
- [ai] Cleanup / PR preparation: wave runner, doctrine steps, `OpenThandolIfDue`, goals; version tags in comments ->
  CHANGELOG; split `AiSystem.cs`; every hand-coded id must skip silently when missing (5.x has no Old Gods).

## Repository

- Push only named tags (`git push origin v0.45.0`), never `--tags`: old tags drag Git LFS into history the local
  clone never downloaded ("Git LFS upload failed"). Missing tags on GitHub so far: v0.42.1, v0.43.0, v0.43.1 (push if
  they exist locally).
- [you] `git lfs ls-files | awk '$2=="-"'` lists current files that are only LFS pointers -- if any, the built map
  lacks those assets (possible cause of the invisible Kezan units). Fix: `git lfs pull`, else fetch from the original
  repository (`../ShatteredLegacies/TODO.md`).
- The route map image (`tools/ai/out/`) is never committed; attach it to a GitHub release when worth keeping.
