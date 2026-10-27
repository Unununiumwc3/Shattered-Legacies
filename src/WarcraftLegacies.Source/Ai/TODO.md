# TODO -- bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Fork content (fallbacks, Kezan, balance, map design) is
in `../ShatteredLegacies/TODO.md`. Current build: **0.43.1**. Next: one game with 0.43.1 (new Veteran = no bonus gold); then waves (`CONCEPTS.md` 1).

## Where we are

- **0.39.10 (step 0 of the refactor)**: the fallback code moved to `ShatteredLegacies/`; the bot has no reference to
  it and offers `AiApi` instead. Markers `[BOT]` / `[SL]` / `[WORKAROUND]` in original files, `WORKAROUNDS.md` lists
  what blocks a pull request to the original map, `MEASUREMENTS.md` keeps load numbers per version
  (`tools/ai/dump_summary.py`).
- **Engine load**: UNITS dump section, heartbeat file and `-orders idle` are in (0.39.0). A 520-min game ran without a
  freeze. The refactor below is the next lever.
- **Production** trains from real train lists (0.38); bot heroes are free and learn skills; starter production
  buildings; bots take over leaving humans.

## Refactor (agreed, one version per phase, each playable on its own)

Vision: bots replace human players and challenge them -- mostly through **many troops on the map**. So the engine
budget must go to units, not to AI overhead. Long-term goal: a pull request to the original Warcraft Legacies that
works on the original map without workarounds (`WORKAROUNDS.md`).

Measure every phase with one game and `-autodump`, summary into `MEASUREMENTS.md` (units, orders/min, LOAD at
10/20/30/45 min). Compare with the phase before.

0. **0.39.10 -- step 0** (done): fork code out of `Ai/`, `AiApi`, markers, `WORKAROUNDS.md`.
1. **0.40 -- scheduler + load budget** (done; not measured yet -- the A/B test of 0.41 covers it: `-orders idle`
   in 0.41 = 0.40's army behaviour. Calibrate the budget from "max-slot" in LOAD). One 0.25-s timer; every periodic task has a period and a fixed phase
   (static, deterministic staggering): combat every 0.5 s and production every 1 s as today, the 15-s wave tick split
   into its subtasks spread over the cycle in today's order, plus revive, quest checks, Legion event check, debug
   output. One-shot delays stay as they are. Counters (deterministic only, never wall-clock time = desync): per task
   runs / ops (last, max, sum), ops per slot, orders/min, **orders by distance** and **re-orders of idle units
   already at their target** (proxies for engine pathing cost, which the script cannot see). Budget report-only;
   `-budget on` tests enforcement. Shown in `-debug2` and the dump's LOAD section. No behaviour change beyond the
   phase within a 15-s cycle. Note: this measures and smooths; it is not expected to cure a freeze by itself (the
   0.38.1 freeze had flat script load).
2. **0.41 -- army orders** (done, see CHANGELOG; tune after the A/B test: floor 50 food, factor 1.5, structures x 8,
   radii 600 / 1500). Original plan: Fix lever 1's hole: "idle" (order 0) is also true for units standing at the target,
   so they are re-ordered every 20-s heartbeat (~3700 orders/min at 538 min). New rule: re-order only idle units far
   from their assigned point, never units that cannot move (move speed <= 1), one shared unit pass with `Process`
   (`AttackMove` enumerates everything a second time). **Attack group**: size from the target (towers and enemies
   near the node), 40-60 food as the floor -- small groups fail against upgraded CP towers. The rest holds at the
   owned node nearest the frontier (not at every node), and is not re-ordered while it holds.
3. **0.42 -- route planner** (done, see CHANGELOG). Open: the 7 detours in `ROUTES.md`; doodads other than trees /
   rocks (archways, walls) and gate units are not in the grid. Original plan: Graph generated from map data only (so it can run on
   upstream 5.x): nodes = CPs + **objective nodes** (preplaced hostile structures, capitals like the Sunwell) +
   portal ends; edges with walking cost from the pathing data plus doodads / destructables (the pathing map has
   terrain only). Open / closed is read from the world at runtime (blocking rocks alive? portal exists?), no hand
   flags. Dijkstra from the army's own node over open edges -> cheapest target; retarget only on events.
   **Waypoint orders** (next node on the route instead of one cross-map order) move here -- they need routes.
   Replaces: Combined / Kalimdor sets and the merge, portal-aware distance, stickiness, most stuck special cases.
   Expected to make W1-W4 (`WORKAROUNDS.md`, the CPs added for bots) unnecessary -- through the objective nodes;
   Dijkstra over CPs alone would not. Data: `tools/ai/graph_weights.py` (feasibility: 176 of 187 edges, median 1.08x
   the straight line).
4. [ai] **0.43** done: target by distance from the territory (CHANGELOG). Still open from the plan --
   after a game with area targets: if area nodes find the harbour / camp targets, remove the workaround CPs W1-W4
   (map data; check with `gen_routes.py --without n0SA,n0TD,n0RF,n0JS,n0SP,n0NX` -- in 0.42 every one of these spots
   has an area node within 1800) and delete the old frontier picker, edge sets, merge and goals.
5. [you/ai] **Economy** (after 1-3 are measured): bonus gold per minute via WL's `BonusIncome` (independent of the
   bot count; today's per-turn subsidy pays more the fewer bots there are), start 11:00, Recruit 0,
   `D x (20 % of WL StartingGold.Income + 10 % of CP income, counted up to 300)`, D 0 / 0.5 / 1 / 1.5 / 2.
   Food caps only as the last resort.
6. [ai] **Cleanup / PR preparation**: wave runner, doctrine steps, `OpenThandolIfDue`, goals; version tags in
   comments -> CHANGELOG; split `AiSystem.cs`. Every hand-coded id and coordinate must skip silently when it is
   missing (5.x has no Old Gods); the strength vote must not depend on the removed game-mode vote (W5).

## Freeze check (still open -- also WITHOUT alt-tabbing)

- 0.36.0 minute 1 and 0.37.1 turn 17 froze while alt-tabbing (sound kept playing). **0.38.1 froze at ~34 min without
  alt-tabbing.** The dumps up to 30:00 showed nothing unusual (no errors, armies 1753 -> 914, load flat).
- After a freeze, send the heartbeat file `CustomMapData\WLAI_heartbeat.txt` with the dumps (time = when it stopped,
  task list = what ran last). `dump_summary.py` shows real seconds per game minute from the file times.
- Still open: does a freeze also happen without computer players? On Windows?

## Production follow-ups

- [ai] **Units of buildings a bot doesn't have** stay "never" (Quel'thalas' Academy and Consortium, Kul Tiras' and
  Stormwind's special units). Proposal: the starter kit also gives each bot its "Specialist" building(s).
- [ai] **Quest-locked heroes and units**: `unit_scan.py` lists which quest unlocks what; decide which quests bots
  should drive.
- [ai] Hero care teleports often (~40 in 65 min; Warsong's Chieftain 6x from -9952,1944). Check that spot first.
- [ai] Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- Ships and builders are left out of production on purpose; naval play is not planned.

## Decisions needed

- [ai] **Guardian summons (decided, not built)**: bots only. A channelling quest hero that gets attacked pulls the
  24 nearest army units of its owner to it (Mass Teleport look, ping, one announcement per channel), at most once per
  minute. **HQ last stand**: the same when a bot's HQ is attacked, at most once per 5 min, HQ invulnerable during the
  teleport. The HQ list must come from WL's essential legends inside the bot, or from a fork through `AiApi` -- not
  from `ShatteredLegacies/`.
- **Archimonde's channel: ON HOLD** (maintainer); the guardian summons cover the remaining risk.
- [you] **Defend range / flapping**: the Legion "defended" a shrine 26 km away and switched defend/attack every 15 s.
  Proposal: defend only within ~8000 of the army, keep a defend target at least 60 s. (Part of 0.42: defense = nodes
  within a few hops of the army.)
- [you] Kul Tiras Highbank last stand (`QuestRule` with `CapitalLost` = Boralus Keep)?
- [you] **Free heroes for a human?** In the 0.38 test the human Lordaeron got heroes for free. The bot only trains
  heroes for bots. Check whether gold goes down when you queue a hero.

## Smaller implementation work

- [ai] **Rift to the Maw of Gor'ma does not work** (0.39.9 playtest): the target end is around (22545,-4784), the
  preplaced `n07E`. Check doodads there offline (`map_query.py around`); maybe ~100 further north.
- [ai] **Orcs never take the Warsong path** (0.39.9 playtest): the random pick in `AiChoices` works for the Sentinels
  and the Exodar. Check the orc choice (bad luck vs a bug).
- [ai] **Warsong as a bot**: goals for its subdue quests -- Echo Isles, Thunder Bluff, Stonemaul; each needs Grom.
- [ai] **Legion still on the Sunwell side**: summoned at 17:39, but the plague support had already teleported its
  army to Alterac. Proposal: at the summon, move a bot Legion's army through to Felwood, or stop the plague support
  once the Legion is summoned. [you] decide.
- [ai] **Argus not taken by the winners** after 520 min: probably the pre-summon edge weight (15). Re-check.
- [you] **Argus with mixed owners**: without a summon the pre-summon portal stays open for everyone. Keep it, or only
  for the Legion's team?
- [ai] **Eye of Sargeras**: nobody picked it up in 520 min. Find it and whether bots can reach it.
- [ai] **Orc air units fly over the ocean**: flyers take the straight line to a target that ground units reach by
  portal. Idea: flyers follow the army anchor (see also pseudo-flyers in `../ShatteredLegacies/TODO.md`).
- [you/ai] **CP towers vs bots**: fully upgraded CP towers beat bot armies -> the map felt empty after the start.
  Partly 0.41 (group size from the target); ideas: weaker towers on bot-owned CPs, bots focusing towers.
- [ai] Autocast for bot units / heroes (heal, inner fire, ...).
- [ai] Gate sweep stays; the maintainer checks in playtests which gates survive (`-id`, then `AiGraph.GateTypes`).
- [ai] `map_query.py` -> `map_audit.py`: check every coordinate in `AiConfig.cs` in one run (incl. doodads near
  portal ends).
- [ai] Debug: `-scout` (define first), winner record when WL declares victory.
- [ai] (low) Bot heroes die early (train a hero, attack creeps without an army). Probably fixed by 0.41.
- Note: WL has no `-spectate`; the AI takes over a slot only when its human LEAVES.

## Check in the next playtests (0.42: games A / B / C)

- `-version` shows the new build; no `SYSTEM ERROR`, no `TICK ERROR` in PINNED.
- **A/B/C test with the 0.42 build, same settings every game** (strength vote Veteran = normal gold, same
  factions, 20-25 min, `-autodump` at the start, empty CustomMapData before each game):
  game A `-orders idle` and `-route off` (= 0.40), game B `-route off` (= 0.41), game C nothing (= 0.42).
  Summarize each with `dump_summary.py --append "A ..."` etc. Summarize each with `dump_summary.py --append "A idle ..."` / `"B smart ..."`. Compare
  orders/min, orders far / idle-at-target, units on the map, CPs per team, and the feel: do bots still take CPs?
  Do groups die at upgraded towers? Do units pile up at the staging CPs?
- 0.41: `-debug2` third line (army orders by reason); dump LOAD "groups" line per bot.
- **0.43.1** (Veteran = 0 bonus now -- not comparable with older games, start a new baseline): no team balls (ROUTES:
  different targets per bot of a team)? Units walk on without stopping at every node? Fewer "advance" orders?
  Druids take their neighbourhood (quest CPs)? Observations from the 0.43.0 game to re-check: Scourge had not cleared
  Northrend at 20:00 (its army was in Lordaeron); bots walk to dead ends like Thunder Bluff (an enemy capital -- a
  legal target, but check whether they fail there and come back: event log "stuck"); fights everywhere but few CPs
  change hands.
- **0.43.0**: Druids (and everybody) take the neutral CPs around their home first, then the front? ROUTES lines show
  "inside" / "front" / "quest". Plus the 0.42.1 checks below.
- **0.42.1 (game C')**: same settings as game B (Veteran, 20-25 min). Watch: dwarves around Fuselight / Twilight
  Shore, Thandol bridge while closed (no standoff at the banks?), Outland portals (groups go through?), Druids take
  their quest CPs first? Bots stop running to dead ends (Gnomeregan, Terrordale)? Send the last dump (ROUTES section).
- 0.42 (game C): dump ROUTES -- targets plausible? "next" nodes along the way? Rock groups closed early, open after
  the turns? Waygate edges > 0 after 1 min? Bots attack harbours / camps (area targets show as "E:" with a building
  name)? `-edges` still shows the OLD graph.
- 0.40: bots behave as in 0.39.9 (attack, produce, gates disappear, quests, portals). `-debug2` shows a second line
  (slot max, over, far orders, idle@target, top tasks); `-dump` LOAD has the scheduler, orders and task lines.
  `-budget on` for a few minutes: "deferred" grows, nothing breaks. Then `dump_summary.py` into MEASUREMENTS.md.
- Fallbacks work as before (trigger buildings at 62 s, Ironforge placeholder, Kul Tiras -> Kezan Enterprise).
  The Scarlet Crusade trigger building reads "Fallback: Scarlet Crusade (story event)".
- Kezan Enterprise bot: Gallywix is its first hero (fixed in 0.39.10); Laboratory / Mercenary Den / Tank Factory
  units are trained (dump TRAINED).
- Older checks still open: new CPs reachable (`-edges`), 100-gold CPs show "(100 gold/min)", Emerald Portals and the
  Old Gods rift both ways, hero visits (Gul'dans Legacy with army escort), Book of Medivh "Guardian's Abjuration",
  starter production pins at 0:35.

## Open questions

- [you] Thandol bridge: opening sooner is not needed as long as bots don't try to cross before it opens.
- [you] Broken Isles are reached only by the AI ship Azshara Coast <-> Azsuna -- a real waygate humans use too. Is a
  ship from Kalimdor the right connection?
