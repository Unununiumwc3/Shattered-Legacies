# TODO -- bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Finished items go to
`CHANGELOG.md`; designs and ideas live in `CONCEPTS.md`; how the movement works today and what was already tried is
in `README.md` ("How the bots move", "Already tried").

**This file is bot work only.** Everything that exists only in Shattered Legacies -- fallbacks, Kezan, the Thandol
Treaty, map content, the fork's win rule and game length, balance of the fork's factions, brainstorming -- is in
`../ShatteredLegacies/TODO.md`. Rule of thumb: would it go into the upstream PR (`Ai/` + `[BOT]` hooks)? Then here.

Current build: **0.45.0** (played, 540 min). Docs: 0.45.1 (no map change). Next playable build: **0.46**.

## The plan: three phases

| Phase | Versions (planned) | Goal |
|---|---|---|
| 1 Movement "good enough" | 0.46, 0.47, 0.48/0.49 as needed | engine load under control, graph right, armies fight |
| 2 Restructure | ~0.50 | layout for the upstream PR, state in objects, old code deleted |
| 3 Fine-tuning | after | movement details, graph without the fork's CPs, balance |

A phase ends when its goal is met, not when a version number is reached: phase 1 at the exit criteria below, phase 2
when `Ai/` can be copied into a fresh upstream branch (with its `[BOT]` hooks), phase 3 at the maintainer's call.

Rules for phase 1:
- **More changes per playtest**, but every change has its own switch and its own counter (dump line), so one game
  shows what each did. Behaviour switches get a per-bot A/B mode (even slots on, odd off; swapped the next game).
- New subsystems (pseudo-portals, ferries, theatres) are written in the phase-2 style from the start (own class,
  state in an object), so phase 2 only migrates old code -- once the CSharp.lua check (0.46) confirms the style.
- Before proposing a movement change: read "Already tried" in `README.md`.

### Phase 1 exit criteria (draft -- `[you]` set the numbers after the 0.46 baseline)

1. **Load**: no stutter. Stopwatch: game turn 40 within ~42 real minutes (<= 5 % drift); order latency (0.46 probe)
   median < 0.5 s, 90th percentile < 1.5 s at 30-40 min. A few spikes in a long game are fine.
2. **Graph**: the maintainer's list of wrong edges is fixed; no new wrong edge seen in a game.
3. **Activity**: little walking-only / idle time -- the activity breakdown (0.46) shows a low idle share, compared
   at the same game minute (late games have longer ways by nature).
4. **Flapping**: few RETURNS (A -> B -> A within 5 min) per bot. Plain target switches are only informative: they are
   expected to be frequent early (many factions, allies capture targets) and to drop later.
5. **No dead map**: CP owners still change in the last 20 min of a long game; a team reaches WL's win (90 CPs per
   TEAM) eventually. Game length and the fork's own win rule: `../ShatteredLegacies/TODO.md` (not a bot criterion).
6. **Final call**: the maintainer's judgement after a game he played (with other humans if possible). The numbers
   only guard against regressions.

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
| 0.44.2 | yes (14 min) | **first build with bot CP income**; max-slot 5207; rest churn; 5227 units at 14 min |
| 0.44.3 | only inside 0.45.0 | creep targets, rest churn fix, exit re-chain |
| 0.45.0 | yes (540 min, 16 bots) | rift: no crossing in 510 min -> dead map; creep camps never ran (TICK ERROR); laggy end |

**Every game before 0.44.2 ran without CP income for bots** (WL pays human slots only): gold, army size and balance
observations from those games are void. Movement observations stay valid.

## Playtest protocol (from 0.46)

- Empty `CustomMapData` first; Veteran; `-autodump`; 40 min (longer when possible). Autopilot allowed -- say which
  faction in the note.
- **Stopwatch**: write down the real time when the turn counter shows turn 5, 10, 20, 30, 40 (e.g. "t10 = 10:30").
  It goes into the note (0.46: `dump_summary.py --real`). It shows whether the game falls behind real time.
- Note which A/B assignment ran (`ab` or `ab2`); the next game uses the other one.
- If you take control of units: does an order start at once or after 1-2 s? (Compare with the latency probe.)

## 0.45.0 results (540-min game, `MEASUREMENTS.md`)

- **Rift: no crossing in 510 min.** Opened at 29:02. Kalimdor team at 0 CPs from 45 min (Old Gods took Kalimdor);
  from then on the three Old Gods bots (~650 units, 160-227k gold each) only targeted Eastern Kingdoms CPs through the
  rift: STUCK 240 s -> skip 1200 s -> the next EK target, all game; 163 units of Black Empire beside the Maw at 540.
  No Old Gods unit ever reached EK, no Fel Horde unit Kalimdor. The late game was a dead map because of the rift --
  pseudo-portals (0.46) are the top item.
- **Creep camps never ran**: `TICK ERROR in route creeps: war3map.lua:29279: attempt to perform arithmetic on a
  boolean value` on every run since 0:27 (pinned once, then silent). With `-creeps on` (default) the one-shots are
  skipped too, so 0.44.3+ bots cleared no camps on purpose. The playtest check "PINNED has no TICK ERROR" missed it.
- **Fel Horde flapping (608 target switches)**: every ~40 s `route-enemy` Maw of Gor'ma -> `graph-enemy` (OLD picker)
  Southshore / Strahnbrad / ... -> 8 s later back to the Maw. Its biggest group stood in Outland; the live graph had
  no edge out of Outland (no Dark Portal waygate edge; `ROUTES.md`: Hellfire <-> Nethergarde "unreachable on foot"),
  so `Pick` from Outland returned null and the old picker took over. Every switch re-orders the group.
- **Gather points barely work**: 149 started in 540 min, 12 assembled, 121 timeouts (81 %).
- **Lag at the end with FEWER units than early**: 5221 units on the map at 10 min, 2800-3400 from 30 min to the end.
  Late: ~100 direct far orders / min (chain fallback, 46,914 in total -- cross-continent path requests), stalled share
  ~24 % in 200-540 min (13 % up to 90 min), 1089 units idle at waygate exits, flapping. Without stopwatch numbers
  unit count vs. orders vs. long-game degradation cannot be separated -- 0.46 measures it.
- **Food-capped, not gold-capped**: the strong bots sat at their food maximum with 95-227k gold. The unit budget
  (40 + 10 per CP) would have cut the 10-min peak by only ~15-20 % and the late game not at all.
- **Quel'thalas survived** (12 CPs at 540, 90-106 structures all game, army often 19-60): 5102 units trained, ~4800
  of them casters (Sorceress 1897, Priest 1898, Spell Breaker 1025) -- no autocast, weak. Its neighbours Scourge
  (~260-300 units) and Legion spent the game on Lordaeron targets and defence (state switches 118 / 195). The dumps
  cannot tell who attacked Quel'thalas how often (the event log keeps 300 lines) -- 0.46 counts targets by owner.
- Not answered by this game: rift east end for humans, rest churn (rest orders 124,498 vs change 177,014 -- still
  high), waygate exits in general.

## Phase 1 -- 0.46 (fixes from 0.45.0, load, guard positions, graph, pseudo-portals)

Each item: switch + counter. Designs: `CONCEPTS.md` sections 6-9. **Priority: pseudo-portals** (0.45.0: the rift
decided the whole late game).

Fixes from 0.45.0:
- [ai] **Creep camps TICK ERROR** (`AiRoutes.CheckCreeps`, war3map.lua:29279). Find the line in the built
  `war3map.lua`; suspect: CSharp.lua's translation of `_creepLevel[node] += cond ? a : b` (rewrite with a local). Then
  the creep targets of 0.44.3 run for the first time -- check them in the 0.46 game.
- [ai] **No fallback to the old picker while the route target is valid**: if `Pick` returns null and the committed
  target came from the route planner and is still alive / not owned / reachable from the territory, keep it. Log WHY
  `Pick` returned null (no start node, no route, nothing scored) as a counter.
- [ai] **Dark Portal / Outland**: a portal edge Outland <-> Blasted Lands (pseudo-portal list), so groups in Outland
  can plan. `[you]` is WL's Dark Portal open all game, or opened by a quest?
- [ai] **Groups only from units that can reach the target** (same walking component, or through a portal edge): the
  Fel Horde's group for the Maw was its Outland units.
- [ai] **Caster share cap** in production: at most ~25 % of a bot's trained units are casters (Quel'thalas: 94 %).
- [ai] **Targets by owner** counter (per bot: how often it targeted each owner's CPs), so "who attacks whom" is
  visible without the event log.
- [ai] **Gather diagnostics**: at a timeout, the share of the group that had arrived; `-gather ab` (per-bot A/B).
- [ai] `dump_summary.py`: TICK ERRORs at the top of the summary, not at the end; the defence log shows the real enemy
  count while a defence is kept (now "0 enemy units").
- [you] **Graph edits**: the maintainer's list of edges to remove / add -> `gen_routes.py` (exceptions list in the
  generator, so a regeneration keeps them), `ROUTES.md`, `render_map.py` check. Needs the list.
- [ai] **Unit budget per CP** (`CONCEPTS.md` 8): production gate, `-cap off|<base> <perCp>`. Default 40 + 10 food per
  held CP (~150 for an average faction), never above the faction's food maximum; heroes not counted. Bots only.
  `AiApi` hook for a base per faction (set by Shattered Legacies; same for all until then).
- [ai] **Order-latency probe**: on a sample of orders the bot gives anyway (idle units only, at most 8 tracked), the
  game time until the unit has moved 32. Dump: median / 90 % / max per 5 min. Game time -> desync-safe. No extra
  orders.
- [ai] **Guard positions** (hypothesis): units of computer players walk back to their "guard position" after
  chasing an enemy. Nothing in the code removes it. `RemoveAllGuardPositions` per bot at setup +
  `RemoveGuardPosition` on every unit a bot gets (train / enter map). `-guard on|off|ab`. Counter: idle-away and rest
  orders per bot (A/B halves compared).
- [ai] **Pseudo-portals, instant** (`CONCEPTS.md` 7): bots no longer click waygates; a chain ends at the portal
  entry, an enter-region trigger teleports units with a ticket and re-chains them at the exit. For every portal edge
  (rift, Argus / Legion, Emerald Portals, the AI ship waygates). `-pportal on|off` (off = 0.45 waygate click).
  Counters: teleports, stale tickets, units teleported on issue (already inside).
- [ai] **Per-bot A/B**: switches accept `ab` (even player slots on, odd off) and `ab2` (swapped). For: guard,
  gather, spread.
- [ai] **Activity breakdown** (from the stall sampler, no new scans): per sample walking / idle / hurt (in a fight) /
  stalled, shares per 5 min. Ranged units shooting unhurt count as stalled -- compare between games.
- [ai] **Flapping returns**: per bot, targets left and taken again within 5 min (A -> B -> A).
- [ai] **Spread chain points** (test, not a certain gain): intermediate chain points get a per-unit offset (ring
  around the node, or the edge's "via" point) instead of every unit walking to the same spot. `-spread on|off|ab`.
- [ai] `dump_summary.py --real "t5=5:02 t10=10:30 ..."` fills the "real s per game min" column.
- [ai] **CSharp.lua check**: a throwaway class with ~300 instance fields (+ a static instance of it); the maintainer
  builds, `lua_check.py` must pass. Confirms that object state avoids hard rules 1, 2 and 7. Removed in 0.47.

## Phase 1 -- 0.47 (theatres, ferries)

- [ai] **Theatres** (`CONCEPTS.md` 6): up to 3 groups per bot, one per theatre (walking component of the route
  graph); own target, own staging, own defence decision. One front = one big group. Replaces "step 3" and the
  defence flapping work.
- [ai] **Ferries** (`CONCEPTS.md` 7): harbour-to-harbour edges, units hidden on board, visible boat, delay. Replaces
  the instant pseudo-portals on sea links. Open decisions in `CONCEPTS.md` 7.
- [ai] Remove the CSharp.lua check class.

## Phase 1 -- 0.48 / 0.49 (as needed)

- Problems found in 0.46 / 0.47 games.
- [ai] **Step 2b, extend or re-order**: when the target changes during a chain, continue from the chain's end if
  that costs <= ~1.2 x the direct route, else a new chain. Decide after the 0.46 data.
- [you/ai] **Hop cost** of portals / ferries (300 today = one short walk; ferries: sail time). Decide with ROUTES data.
- [ai] Defence leftovers after theatres (longer hold, defence only for CPs worth more than the current target, or
  "defend" = the rest at staging).

## Graph (generated from map data -- `tools/ai/gen_routes.py`, check with `tools/ai/render_map.py`)

- [ai] **Exceptions file** for what map data cannot know (invisible blockers, gate units, quest-only passages, the
  maintainer's edge list), listed in `WORKAROUNDS.md`. Maintainer can mark nodes / edges on `route_map.png` in a fixed
  colour (pixel -> map: x = -28672 + 16 px, y = -32256 + 16 (3584 - py)); read by program, not by eye.
- [ai] **Pass nodes**: generate nodes at narrow passages (mountain passes) and on long edges.
- [you] **Gnomeregan / Ironforge / Blackrock Depths** sit inside "no walk, no fly" rings (black on the map image) with
  one gap: dwarves walked into Gnomeregan repeatedly (0.43.1). Check in game whether the gap is real.
- [you] The 7 detours in `ROUTES.md` ("detour Nx"): blocked in game or a real way around? (Probably part of the
  maintainer's edge list.)
- Phase 3: **graph independent of CPs** (nodes from terrain only, CPs as targets inside node areas) and remove the
  workaround CPs W1-W4 (`WORKAROUNDS.md`), replacing them with non-CP nodes at the same spots.

## Phase 2 -- restructure (~0.50)

Method for the upstream PR: clone upstream WL, make a PR branch, copy `Ai/` in, add the `[BOT]` hooks, ask for review.
This code base is not the PR.
- [ai] State in objects (`BotBrain` per player, one object for global state) instead of static `[24]` arrays -- if
  the 0.46 check passes. Removes most of the CSharp.lua limits (hard rules 1, 2, 7) and the class splitting.
- [ai] Split `AiSystem.cs` (287 KB) by responsibility.
- [ai] Delete the old frontier picker, edge sets, graph merge, goals, doctrine steps (once the route planner ran a
  few games without falling back), wave runner, `OpenThandolIfDue`; `-route off` / `-orders idle|all` go with them.
- [ai] Everything fork-specific or a workaround moves to `ShatteredLegacies/` (hand-placed coordinates, ids, missions
  that only exist because of fork content); every hand-coded id skips silently when missing (5.x has no Old Gods).
- [ai] Version tags in comments -> `CHANGELOG.md`.
- [ai] Reuse buffers in the per-service code (`Process`, `Select` allocate arrays / sets every call): Lua garbage.
- [ai] `AiIncome` (bot pays computer slots WL's CP income) is a parallel payer for what is arguably a WL bug
  (`ControlPointManager` pays `User` slots only). For the PR: a one-line upstream fix instead; until then make sure
  both can never pay the same slot.

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
- [you] The rift's east end for HUMANS (bots use pseudo-portals from 0.46): does it work after the 0.45.0 move?
- [you] Free heroes for a human? (0.38: the human Lordaeron got heroes for free.)

## Production follow-ups

- [ai] Units of buildings a bot doesn't have stay "never" (Quel'thalas' Academy / Consortium, Kul Tiras', Stormwind's
  special units). Proposal: the starter kit also gives each bot its "Specialist" building(s).
- [ai] Quest-locked heroes and units: `unit_scan.py` lists which quest unlocks what.
- [ai] Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- [ai] Hero care teleports often (~40 in 65 min; Warsong's Chieftain 6x from -9952,1944).
- [ai] Autocast for bot units / heroes.

## Balance and economy (collected -- decide later, `CONCEPTS.md` 2-3)

- [you] **Strength scale**: re-decide with bots that have income (0.44.2+) and the unit budget (0.46).
- [you/ai] Bonus gold in bursts / gold depot: the unit budget (0.46) already makes bots save gold while capped and
  rebuild fast after losses (waves) -- decide after seeing it.
- [you] Where bots produce (starting region vs near the front).
- Observations from games WITHOUT bot income (void for balance, kept for reference): Old Gods took Kalimdor (0.44.0),
  most bots had armies of 1-10 food (0.43.x-0.44.1).
- [you/ai] CP towers vs bots: fully upgraded CP towers beat small bot armies.
- [you] The South Alliance / Naga balance, Old Gods strength -> `../ShatteredLegacies/TODO.md`.

## Freeze check (still open)

- Freezes so far only with alt-tabbing on Linux/Proton (0.36.0, 0.37.1); 0.38.1 froze at ~34 min without alt-tabbing.
  After a freeze send `CustomMapData\WLAI_heartbeat.txt` with the dumps.

## Decisions needed

- [you] Unit budget numbers (default 40 + 10 per CP) and the exit-criteria numbers -- after the 0.46 baseline.
- [you] Ferries: boat invulnerable or not; warning ping; do humans keep the AI ship waygates (`CONCEPTS.md` 7).
- [ai] Guardian summons (decided, not built): a channelling quest hero that gets attacked pulls the 24 nearest army
  units (once per minute); HQ last stand the same (once per 5 min, HQ invulnerable during the teleport).
- [you] Kul Tiras Highbank last stand?
- [you] Argus with mixed owners: the pre-summon portal stays open for everyone without a summon. Keep it?

## Smaller work

- [ai] `map_query.py` -> `map_audit.py`: check every coordinate in `AiConfig.cs` in one run.
- [ai] Debug: `-scout`; **winner record** when WL declares victory (pinned, with game time; `dump_summary.py` prints
  it and the 80 / 90-CP pins). Today the dump pins only when a team first reaches 80 and 90 CPs.
- [ai] Gate sweep stays; check in playtests which gates survive (`-id`, then `AiGraph.GateTypes`).

## Repository

- Push only named tags (`git push origin v0.46.0`), never `--tags`: old tags drag Git LFS into history the local
  clone never downloaded ("Git LFS upload failed"). Missing tags on GitHub so far: v0.42.1, v0.43.0, v0.43.1 (push if
  they exist locally).
- The route map image (`tools/ai/out/`) is never committed; attach it to a GitHub release when worth keeping.
