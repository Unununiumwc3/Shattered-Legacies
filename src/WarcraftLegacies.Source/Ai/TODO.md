# TODO -- bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Finished items go to
`CHANGELOG.md`; designs and ideas live in `CONCEPTS.md`; how the movement works today and what was already tried is
in `README.md` ("How the bots move", "Already tried").

**This file is bot work only.** Rule of thumb: would it go into the upstream PR (`Ai/` + `[BOT]` hooks)? Then here.
Everything that exists only in the fork that hosts the bot (its timelines, new factions, map content, win rule, game
length, balance) is in the fork's own TODO -- in this repository `../ShatteredLegacies/TODO.md`.

Working patch: **0.68.0**. See [BUILDING_DESIGN.md](BUILDING_DESIGN.md) for the combined construction/hero playtest.

## 0.68.0 playtest and follow-up

- Confirm a Keep becomes a Castle after the altar prerequisite. Check a different faction with two successive upgrades.
- Check three surviving Demon Gates after Stronghold and Fortress. Record branch, gold before and after the paid order and whether a lost gate stays lost.
- Investigate repeated refusals per hero and altar. The 25-minute 0.67.0 dump records 144 starts, 123 finishes and 2,972 refusals. See `MEASUREMENTS.md`.

- Follow a dead hero and a newly unlocked hero through the altar section of consecutive dumps. Distinguish a missing altar, queued order, native finish, cancellation, expiry and food pressure.
- Compare direction changes after neutral captures with `back_forth.py` and the new expansion node/hop/cost lines. Evaluate kill quests separately before tuning the graph.
- Custom ability work for units, heroes and buildings: catalog map-specific active spells and their native orders. Test valid ally/enemy targets already in range, hostile clusters for point AoE, combat-only no-target casts and enabling supported autocasts. Stagger evaluations across a 5–10 second rotation and measure scheduler peaks before widening coverage.

## Current priorities (0.66.0)

1. **[you/ai] Production coverage.** Record the first research building, first higher-tier producer and second producer per type. Verify Legion constructs on Argus, and check late faction workers after a switch.
2. **[you/ai] Hall and altar queues.** Verify immediate tier upgrades when legal, native revives with the original hero and prompt quest-unlocked recruitment.
3. **[you/ai] Economy.** Test two overlapping zero-balance builds, cancellations, fair autopilot and regular troop costs.
4. **[you/ai] Recovery and exits.** Check tight city lots, shoreline yards, loss of the opening base and strategic HQ/CP recovery. Check 10/20-CP forward bases.
5. **[you/ai] Engine load.** Compare builder probes, orders, scheduler peaks and actual multiplayer responsiveness with 0.65.2.

6. **[ai] Economy and siege.** Ignore the food budget temporarily while a defended HQ is under siege; refine
   worker numbers, production priority, autocast and the mix of scarce-unit spending.
7. **[ai] Dynamic instances.** Runtime graph nodes/edges for Undercity and successor-faction content. Keep
   generated routing immutable and add through `AiApi` rather than referencing the fork from `Ai/`.
8. **[ai] Maintenance in separate, behavior-preserving patches.** Delete inactive frontier/wave fallback logic;
   then split `AiSystem` and migrate to `BotBrain`. Keep movement regressions and load counters measurable.
9. **[ai] Upstream contribution.** Test `Ai/` plus explicit `[BOT]` hooks against the unmodified map; review
   scenario assists and generated data separately from behavior.
10. **[ai] Release 1.0 gate, not now:** restrict or disable AI debug commands; check multiplayer release flags.
   **Debug permissions are intentionally unchanged in this patch.**

## Historical movement and phase notes (0.52 documentation; retain for reference)

## Open problems -- historical observations

Phase 1 (movement "good enough") is done (maintainer, 0.51). Next: **phase 2, restructure** (section below).

1. **Check via playtest when it happens (rare)**: a mission hero by ferry (Maiev's "Vault of the Wardens", 0.50.1),
   Tomb of Sargeras <-> Argus both ways (0.49.0), the bot Legion ritual end to end (0.51: 180-s channel with timer,
   then WL's summon and the Demon Portal on the ritual spot), the Legion leaving Argus (Demon Portal, Tomb portal,
   0.48.2 teleporters).
2. **Phase 3 (movement fine-tuning), measured in 0.48.x** -- only if the maintainer sees them again:
   - after a capture the next target can lie behind the army (neutral hops early: territory distance; 48 neutral +
     18 enemy turn-backs in 15 min, 0.48.4). Ideas: direction commitment (targets behind the front cost more), creep
     camps only near the route / front, keep the attack group across a target change.
   - defence ping-pong: `AiDefense.CloseThreat` skips the 45-s cooldown, and the army that just left is always within
     4000 (0.48.2: 24 of 42 defences ended at the 30-s minimum, 15 of 31 re-entries within 60 s).
   - group re-selection on every new target: dropped units still walking keep their OLD chain (TURNAROUNDS "group
     swaps"); hold = attack-move of the whole army to its own centre.
3. **Not WL, decide later**: `AiLegion.MoveLegionArmyToAlterac` teleports a bot Legion's army to Alterac when the
   Scourge's plague lands; the Legion's summon bonus (3000 gold); the betrayal (the Scourge leaves the Legion's team);
   the bot ritual itself (upstream: let a bot cast A00J, or a quest hook for the caster -- `WORKAROUNDS.md`).
4. **Kul Tiras: its islands first** (not strict): prefer the home land's targets early.
5. **Scourge creeps Northrend too slowly** (did better before; the Legion does not support it yet).
6. Load: budget 30 + 5 (0.48.5); multiplayer 0.48.5: ~5 s behind real time at 20 min (fine). Keep the stopwatch.
7. Ferry boats at 150 = the engine's minimum speed; slower only by lowering the map's MinUnitSpeed (all units).

## The plan: three phases

| Phase | Versions (planned) | Goal |
|---|---|---|
| 1 Movement "good enough" | 0.46-0.51 -- **done** (maintainer, 0.51) | engine load under control, graph right, armies fight |
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
   TEAM) eventually. Game length and a fork's own win rule are the fork's (not a bot criterion).
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
| 0.46 part 2 | smoke test, 5 min | 397 teleports, no errors; order latency already 1-2 s median at 5 min (3765 units) |
| 0.46.0 | started | dwarves teleported through WL's naval Water Passageway at the Thandol Span -> 0.46.1 |
| 0.46.1 | yes (44 min, maintainer as Lordaeron on autopilot) | rift crossed (Black Empire in Dun Modr); Dark Portal open turn 8-10; creeps cleared; latency median ~2 s at 10-15 min; stopwatch +4 s at 20 min |
| 0.46.2 | yes (81 min, A/B per phase) | evil won without the 100-gold CPs; first Eye of Sargeras by bots; guard / spread / gather decided; beaten factions not finished off |
| 0.46.3 | 15 min (GitHub build without the regroup fix) | regroup held 11 bots at the start; Druids' back and forth (defence across the sea, creeps at home); Kul Tiras via the dwarf mountains; Lordaeron no longer tries Northrend early |
| 0.46.4 | not played (merged into 0.47.0) |
| 0.47.0 | yes (18 min) | back and forth much reduced; North Alliance ran to the Brill coast once nothing was reachable (old pickers); Scourge / Legion / Old Gods weaker (fronts opened too early); Druids still seen heading to Exodar / Teldrassil |
| 0.47.0 | (second game) | froze ~3 min after the 18-min dump -- no data (send `WLAI_heartbeat.txt` after a freeze) |
| 0.48.0 | yes (25 min) | Legion stuck on Argus (teleporters are spells, not cast); armies walked back to their base on empty plans; fronts moved 36 times; South Alliance "nothing reachable" 15-20 min |
| 0.48.1 | yes (25 min) | "felt mostly good"; tram worked; Legion did not leave Argus; North Alliance patrolled at Tirisfal; back and forth still (Sentinels, Druids early; Dalaran, Scourge later); a bit laggy |
| 0.48.2 | yes (26 min, Ironforge on autopilot) | (built: finish-off bias; graph corrections (whirlpool rocks, Barrens hub, Teldrassil, Ahn'Qiraj); Lordaeron no longer gathers at the coast? Loch Modan - Searing Gorge and Alterac - Andorhal really blocked (B01L)?) turn-backs measured from the event log: neutral hops early, defence ping-pong (24 of 42 defences ended at the 30-s minimum) and enemy re-picks later; unit movement not in the dumps -> 0.48.3 |
| 0.48.3 | yes (8.5 min, Black Empire on autopilot, Druids watched) | measurement build: units out of orders walk back to the spot where they were TRAINED (guard position) -- the back and forth; plus re-plan switches backwards |
| 0.48.4 | yes (15 min, Ahn'Qiraj on autopilot) | "improved a lot", no back and forth seen; engine walks 4.5 % -> 1.0 % of walks; Scourge army 4 min at a sleeping Mammut camp; some stutter (dwarves, turn 15) |
| 0.48.5 | yes (22 min, multiplayer, maintainer as Kul Tiras / South Alliance) | sleeping creeps woken at the target; budget 30 + 5; guard removal once per unit. Stopwatch: ~5 s behind at 20 min (fine). Dark Portal unused: its waygates became player-owned (~10 min) -> 0.50 |
| 0.49.0 | no (portal work continues without playtests) | Legion: Book of Medivh ritual only (bot hero invulnerable while channelling), Tomb <-> Argus both ways |
| 0.51.1 | **next** | bot Legion ritual: invulnerable every second, put back on its spot (never stopped); 0.51.0: the bot performs the ritual itself (timer, rain of fire, portal on the spot), ferries at 150 |
| 0.51.0 | no (inside 0.51.1) | |
| 0.50.1 | yes (20 min) | ritual order fix; boat owned by the bot, smooth, half speed; missions travel by route (ferries). Ferries look good (slower wanted); the bot with the Book still did not cast (orders refused) |
| 0.50.0 | yes (24 min) | (built: ferries replace the AI ship waygates; Dark Portal open to all bots whoever owns its waygates; Book without level 12 for bots; Book pedestal Dalaran or Gilneas) Dark Portal works; ferries work ("enter boat" good, boat stuttered, not on the minimap, too fast); Arthas held the Book but did not cast; Maiev stood at the coast |

**Every game before 0.44.2 ran without CP income for bots** (WL pays human slots only): gold, army size and balance
observations from those games are void. Movement observations stay valid.

## Playtest protocol (from 0.46)

- Empty `CustomMapData` first; Veteran; `-autodump`; 40 min (longer when possible). Autopilot allowed -- say which
  faction in the note.
- **Stopwatch**: write down the real time when the turn counter shows turn 5, 10, 20, 30, 40 (e.g. "t10 = 10:30").
  It goes into the note (0.46: `dump_summary.py --real`). It shows whether the game falls behind real time.
- Note which A/B assignment ran (`ab` or `ab2`); the next game uses the other one.
- If you take control of units: does an order start at once or after 1-2 s? (Compare with the latency probe.)

## Phase 3 candidates (collected from the phase-1 lists -- decide with game data)

- [ai] Step 2b, extend or re-order: when the target changes during a chain, continue from the chain's end if that
  costs <= ~1.2 x the direct route, else a new chain.
- [you/ai] Hop cost of portals / ferries (300 + sail time today) -- decide with ROUTES data.
- [ai] Defence leftovers: longer hold, defence only for CPs worth more than the current target, or "defend" = the rest
  at staging; `CloseThreat` ping-pong (Open problems 2).
- [ai] Old Gods rift: 0.46.2 only one Old Gods faction used it; Fel Horde / Illidan barely -- check why (route cost,
  theatre).
- [ai] low priority (maintainer, 0.46.2): **attackable stones** -- between nodes 188 and 203 (0.46.3 map) and at
  Searing Gorge / Alterac (B01L, walls today): an edge "attackable" -- bots destroy the stones when their route needs
  the passage. Only if it disturbs the movement.
- `[you]` graph questions from 0.46-0.47 (need `-xy` on a unit there -- it shows the route node): "weird non-CP nodes";
  Windshear Crossing - Feralas (in neither graph); Kul Tiras around Dun Morogh (9417,-8499; its area has no link
  south); Druids heading to Exodar / Teldrassil after the area is cleared.

## Graph (generated from map data -- `tools/shattered-legacies/ai/gen_routes.py`, check with `tools/shattered-legacies/ai/render_map.py`)

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

## Phase 2 -- restructure (next chat)

Rules: pure refactors (no behaviour change in the same drop); compare a game's `dump_summary.py` numbers before / after;
keep every measurement (TURNAROUNDS, TRAIL, LOAD lines) working -- they are how phase 1 was won.

Method for the upstream PR: clone upstream WL, make a PR branch, copy `Ai/` in, add the `[BOT]` hooks, ask for review.
This code base is not the PR.
- [ai] State in objects (`BotBrain` per player, one object for global state) instead of static `[24]` arrays -- if
  the 0.46 check passes. Removes most of the CSharp.lua limits (hard rules 1, 2, 7) and the class splitting.
- [ai] Split `AiSystem.cs` (287 KB) by responsibility.
- [ai] Delete the old frontier picker, edge sets, graph merge, goals, doctrine steps (once the route planner ran a
  few games without falling back), wave runner, `OpenThandolIfDue`; `-route off` / `-orders idle|all` go with them.
- [ai] Everything fork-specific or a workaround moves out of `Ai/` behind `AiApi` (hand-placed coordinates, ids,
  missions that only exist because of fork content); every hand-coded id skips silently when missing (5.x has no Old
  Gods). 0.52: the map title and the faction-pick exclusions moved (`AiApi.SetMapTitle`, `ExcludeFromBotPicks`).
- [ai] Version tags in comments -> `CHANGELOG.md`.
- [ai] Remove the CSharp.lua check class (the 0.46 check passed); keep `tools/shattered-legacies/ai/check_refs.py` limits until the
  state-in-objects migration has removed the static arrays.
- [ai] Reuse buffers in the per-service code (`Process`, `Select` allocate arrays / sets every call): Lua garbage.
- [ai] `AiIncome` (bot pays computer slots WL's CP income) is a parallel payer for what is arguably a WL bug
  (`ControlPointManager` pays `User` slots only). For the PR: a one-line upstream fix instead; until then make sure
  both can never pay the same slot.

## Missions -> general logic

- [ai] **Ragnaros / Yogg-Saron instant death**: in 0.46.2 the Scourge fought Yogg-Saron as a creep after the first
  death and killed him -- the creep-camp logic handles bosses well. Remove the instant-death handling for Ragnaros too
  (and Yogg's, if it is one). Phase 2/3.
- [ai] **Fallback paths as one table** (Scarlet Crusade, Theramore; a fork's own turning points added through
  `AiApi`): research + building read from the WL quest's `ObjectiveResearch`; a small table per quest title for WHEN
  (capital lost, legend turned, CPs below N, not before turn N). Phase 2.

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

- [you/ai] **Worker construction, first implementation in 0.65.0, pending playtests.** The old proposal
  for 3-minute instant-production checks was superseded by persistent `AiBuilder` projects and real worker build
  orders; `AiBuildSites` handles HQ / owned-or-allied CP / worker-area anchors. See the top roadmap and
  `BUILDING_DESIGN.md` for the limits, open cases and actual tests. No longer use `PlaceProduction` or the old
  starter kit to grant missing buildings.
- [ai] **Ignore the food cap while the HQ is under siege** (maintainer, 0.59 playtest): a bot whose HQ (or the capital
  it defends) is under attack trains past `AiBudget`'s cap until the threat is gone.

- [ai] **Autocast** (maintainer, 0.46.3): turn autocast ON for bot units whose ability supports it but starts off
  (Blood Knight Squire); heroes with autocast abilities (Kael's magic spheres) should skill them first. Phase 3.

- [you/ai] Gold mainly for "limited" units (elite / capped units), without a hard focus -- armies must stay mixed
  (maintainer, 0.46.2). Phase 3.

- [ai] Missing specialist-production buildings: verify worker build menus for the Academy, Consortium and similar
  faction-specific structures. The new builder attempts them, but engine rejection must be checked in game.
- [ai] Quest-locked heroes and units: `unit_scan.py` lists which quest unlocks what.
- [ai] Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- [ai] Hero care teleports often (~40 in 65 min; Warsong's Chieftain 6x from -9952,1944).
- [ai] Autocast for bot units / heroes.

## Balance and economy (collected -- decide later, `CONCEPTS.md` 2-3)

- [you/ai] **Difficulty modes** (maintainer, 0.48): define them by the bots' mechanics -- food (floor / budget), free
  heroes, free tech, gold -- and how quickly / how much is given. Better descriptions in the vote, and a text
  announcement after the vote that tells the humans what the bots get (e.g. free heroes).

- [you] **Strength scale**: re-decide with bots that have income (0.44.2+) and the unit budget (0.46).
- [you/ai] Bonus gold in bursts / gold depot: the unit budget (0.46) already makes bots save gold while capped and
  rebuild fast after losses (waves) -- decide after seeing it.
- [you] Where bots produce (starting region vs near the front).
- Observations from games WITHOUT bot income (void for balance, kept for reference): Old Gods took Kalimdor (0.44.0),
  most bots had armies of 1-10 food (0.43.x-0.44.1).
- [you/ai] CP towers vs bots: fully upgraded CP towers beat small bot armies.
- [you] The South Alliance / Naga balance, Old Gods strength -> the fork's TODO (map balance, not bot work).

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
- The route map image (`tools/shattered-legacies/ai/out/`) is never committed; attach it to a GitHub release when worth keeping.
