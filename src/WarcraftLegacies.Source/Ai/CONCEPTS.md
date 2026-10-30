# Concepts (bot behaviour)

Designs and ideas. They interact -- movement, gold spending, production and its buildings belong together -- so decide
them together, one version per step, each measured (`MEASUREMENTS.md`). Agreed items are scheduled in `TODO.md`
(phases and versions); their design stays here. What was already tried and why it changed: `README.md`, "Already
tried". `[you]` = maintainer decision, `[ai]` = implementation.

Guiding rules (maintainer): natural movement first; stay close to the original Warcraft Legacies (no changes to WL's
CP income); simple and not engine-heavy. Engine limits come first: WC3 slows down with many units and many orders
(order delay of 1-2 s seen in 0.43.1 and 0.44.2) -- a movement idea that needs more orders has to pay for them.

## 0. Movement rework -- steps

| Step | Version | Content |
|---|---|---|
| 1 | 0.44 (done, tested) | **Movement chains**: queued attack-moves along route nodes; waygate legs; a new chain only when the target changes. |
| 2 | 0.45 (done, test running) | **Gather points**: routes longer than 5 nodes wait at the last own node (80 % of the group's food within 1500, or 60 s). |
| L | 0.46 | **Load**: unit budget per CP (8), order-latency probe, guard positions, activity / flapping counters. |
| P | 0.46 | **Pseudo-portals** (7): bots teleport by trigger instead of clicking waygates. |
| T | 0.47 | **Theatres** (6): up to 3 groups per bot, one per walking component of the graph. Replaces the old "step 3". |
| F | 0.47 | **Ferries** (7): sea links as harbour-to-harbour trips with a visible boat and a delay. |
| 2b | 0.48+ | **Extend or re-order** a chain when the target changes during it (<= ~1.2 x the direct route: extend). |
| G | alongside | **Graph quality**: maintainer's edge list (0.46), exceptions file, pass nodes; phase 3: graph without CPs. |

Decisions behind it:
- Attack-move never attacks own / allied units: own CPs on the route are passed, enemy CPs and hostile neutrals fought.
- "A falls behind us" is the defence rule's business (0.43.2: reach + minimum times; 0.47: per theatre).
- Group stretching matters only for small groups -- hence gather points by chain length, not always.
- Portal edges need no hand-placed nodes: every live waygate gives its entry and exit. Ferries need a harbour list.
- WC3 has no group movement: every unit walks on its own. "Group" is only the bot's bookkeeping; cohesion comes from
  waiting (gather points, ferry boarding), never from the engine.

Graph notes:
- **Graph independent of CPs** (phase 3, also good for upstream): nodes generated from terrain only; CPs are TARGETS
  inside a node's area (region map). Shattered Legacies CPs (`n1A0`-`n1B2`) and the workaround CPs (W1-W4) then do not
  shape the graph; W1-W4 become non-CP nodes at the same spots.
- **The map image** (`render_map.py`) is NOT committed (~1-2 MB per graph change, for good). Regenerate locally; attach
  a version worth keeping to its GitHub release.

## 1. Waves to the front -- done as step 2 (0.45)

Front = nodes bordering the own land; staging = own node next to the target; wave = the group goes once enough food
has gathered. Built as gather points (0.45). The 0.45.0 game: 149 started in 540 min, 12 assembled, 121 timeouts --
barely used and mostly 60-s waits; diagnostics and per-bot A/B in 0.46. Left open: the threshold for neutral targets
(same rule today), and what waiting units do (they stand -- no orders is the cheapest). The unit budget (8) adds the
economic side of waves: capped bots save gold and rebuild in bursts after losses -- but in 0.45.0 the strong bots
were already food-capped with 95-227k gold, so the budget changes little for them.

## 2. Gold in bursts / gold depot

Problem (maintainer): late-game fights are boring -- a few troops against a few, or a few against a death stack.
Units trickle to the front one by one.

Options:
- **Bonus gold in bursts** (difficulty vote): bot-only code (`AiTech` subsidy / later WL's `BonusIncome`), so it does
  not touch WL's CP income. Bursts fit waves: lump -> production spike -> wave.
- **Gold depot**: a bot saves until a threshold, then spends everything, then saves again. Tried before without
  success -- unclear whether the depot or something else was the cause. Numbers from the maintainer: the most
  expensive units cost ~50 gold; 10 units = 500; doubled -> a depot of **~1000 gold**. Needs a dynamic factor
  (smaller depot when income is low). Bursts of 5000 were spent very fast -- spending speed is not the problem.
- **Spend in intervals** (all gold every N minutes): hard to tune.
- [you] Not wanted: bursts in the CP income itself (a step away from WL).

**0.44.2 correction**: bots never received CP income (WL pays human slots only) -- every gold observation before
0.44.2 is void; the bonus WAS their whole income. Re-measure before deciding depot / bursts / scale.

Data (0.43.1, new Veteran = 0 bonus): at 20:00 armies of 2-50 food, gold ~0 for every bot, the map felt empty.
Without any bonus a bot cannot hold an army -- the free techs / heroes do not make up for it.

Open: [you] depot vs. bursts of the bonus only; [ai] measure first how the gold of a bot flows today (income per
minute, spent per minute, gold on hand) -- a dump line per bot would show whether the trickle comes from income or
from spending.

## 3. Where bots produce

Today bots train in every production building they own, also buildings they got far from their start (starter kit,
captured areas) -- units appear all over the map.

Options:
- Produce only in the starting region: simple, predictable; new units walk far to the front.
- Produce near the front: short ways, waves assemble faster; but production follows the front and can be lost.
- Mixed: production where it is, but new units go to the staging node of the current wave (0.41 already sends the
  "rest" there) -- may already be enough. [you] decide after waves.

Note: "home region" needs no configuration -- the territory (own + allied nodes) is known at runtime (0.43).

## 4. Quest objectives beyond "capture a CP"

0.42.1/0.43: a CP where an open objective of an open quest lies counts 0.3 of its cost when it is on the front
(`AiRoutes.QuestNodes`: reads WL's `Objective.Position` -- no ids, every faction).

Next level (feasible, not now): WL's objective classes keep their requirements private (required hero level, area,
building type, research). Proposal:
- [ai] a small `[BOT]` hook in WL: objectives expose their requirements read-only (good upstream candidate -- no
  gameplay change);
- [ai] one bot handler per objective type: CP -> route target; hero + level + place -> hero visits (today configured by
  hand in `AiConfig.HeroVisits`, then derived); build in an area -> the bot places the buildings (it can already find
  build spots); research -> `AiTech`.
- This would replace the Kul Tiras workaround (buildings placed around, quest given) with the real quest.

## 5. Defence

"Natural" defence (units that react where the enemy attacks) is not feasible cheaply. Today: `AiDefense` (0.43.2) --
the whole army defends a CP on its landmass within 9000, at least 30 s, 45-s cooldown; still ~1 switch per minute for
weak factions under pressure. With theatres (6) the decision is per theatre: a defence in one theatre never pulls the
group of another. A cheaper trigger than scanning: `EVENT_PLAYER_UNIT_ATTACKED` on CPs / bases (events are synced).

## 6. Theatres (0.47, agreed)

Problem: one attack group and one staging CP per bot for an empire on several continents. Every decision is global:
attack <-> defence flapping, rest units sent across the map, new units from far production buildings funnelled to one
target.

Design:
- **Theatre** = a walking component of the route graph (walking edges with the live rock state, no portals): Kalimdor,
  Eastern Kingdoms, Northrend, Outland, each island group. Computed at runtime from the generated graph -- NOT from
  `Geography.Landmass`, which returns -1 for islands, isolated realms and 9 Kalimdor CPs.
- A bot has up to **3 active theatres**: the ones where it holds nodes and has at least ~30 food of army; ranked by
  army food. Units in other theatres join the biggest one (by portal / ferry).
- Per theatre: own target (Pick restricted to that component; territory = own nodes in it), own attack group, own
  staging CP, own gather point, own defence decision (only its units defend).
- **Big fights stay**: one front = one theatre = one group (plus its rest). Splitting is by geography, never by size
  on the same front. (The old "step 3" -- several groups when the army is bigger than one target needs -- is dropped
  unless the data asks for it.)
- Cost: up to 3 x the target planning per bot (the route trees are shared per destination).
- Counters: theatres per bot, food per theatre, switches per theatre.

## 7. Pseudo-portals (0.46) and ferries (0.47) (agreed)

Problem: waygates were the most unstable part (0.42-0.45): units click the waygate, the teleport drops their queued
chain (WC3), they stand at the exit; fixes made it better, the next change worse. Ships as real transports are too
complex.

**Pseudo-portals (0.46, instant)** -- bots stop using waygates:
- A chain that crosses a portal edge **ends at the portal entry** (the spot in front of the waygate). The chain holds
  no teleport.
- The unit gets a **ticket**: portal edge + its final destination. Set when the chain is issued; a new chain
  overwrites it.
- One **enter-region trigger** per entry (radius ~250). It fires only for a unit that actually walks in -- so a
  "teleport from a distant location" cannot happen. It teleports only a living bot unit whose ticket names this
  portal; passers-by, human units, stale tickets and dead portals are ignored.
- Teleport: effect at the entry, `SetUnitPosition` beside the exit (the engine picks the nearest free spot), effect at
  the exit (at most one effect per portal per second), then **at once the rest of the chain** from the exit. No
  waiting at exits, no polling.
- Edge case: a unit already INSIDE the entry area when it gets the ticket gets no enter event -> teleport on issue.
- Each unit is handled on its own (WC3 has no group movement): units arrive one by one, like human players' units.
  Cohesion comes from the gather point on the far side, not from the portal.
- Applies to every portal edge: Old Gods rift (existing assets stay where they are -- the east-end position does not
  matter for bots; 0.45.0: no bot crossed it in 510 min), Argus / Legion portals, Emerald Portals, the AI ship
  waygates, and the **Dark Portal** (Outland <-> Blasted Lands -- missing from the live graph in 0.45.0). Portal
  edges then come from a data list (with an "open" condition), not only from live waygates. Real waygates stay for
  humans.
  Bot-only by construction: no human can use the trigger.
- `-pportal off` = 0.45 (click the waygate). Counters: teleports, stale tickets ignored, teleports on issue.

**Ferries (0.47)** -- sea links with a delay and a visible boat:
- **Harbours** (data: a list, later maybe generated): landmass, pier point (entry), landing point (exit). Sea lanes
  between harbour pairs as polylines over water, computed offline from `PathingMap.json` (water cells).
- Graph: **one-way edges** harbour A -> harbour B per lane (the planner already handles one-way portal edges); cost =
  sail time as walking distance + boarding wait. Usable as ENTRY only when the bot or an ally holds the harbour's
  area (taking a harbour matters); landing anywhere (an invasion lands in enemy land).
- Every landmass the bots can reach needs at least one harbour that can also be an entry -- else units are stranded
  (Broken Isles, 0.43.2). Hence harbours are both entry and exit. A harbour is a data point, not a building: islands
  without a shipyard get a **beach landing** (the boat anchors offshore, units are rowed over -- the same boarding and
  landing, only the spot differs). Exit-only beaches are an option for invasion flavour. Bot-built shipyards as
  entries: dropped (overkill).
- **Boarding**: units with a ticket entering the pier area are hidden (`ShowUnit` false -- removed from the game world:
  no target, no collision, no true-sight reveal; not the invisibility mechanic) AND paused (`PauseUnit` -- no order
  runs, even if some bot code orders them by mistake) and listed as passengers. Sleep / invisibility effects are not
  used: sleep wakes on damage and needs dummy casters, invisibility is revealed by true sight. The boat
  leaves when the group's share has boarded or after N s (gather logic).
- **Boat**: a dummy ship owned by the bot (team colour), Locust (not clickable, not targetable), moved with
  `SetUnitX/Y` along the lane every scheduler slot -- no pathfinder, fixed travel time, visible to everyone.
- **Landing**: passengers shown beside the landing point (`SetUnitPosition`), re-chained at once; boat removed.
- Hidden passengers keep HP, mana, items and food. They must be kept OUT of the army logic and the samplers
  (in-transit set): player enumerations still return hidden units.
- [you] Boat invulnerable (simple) or killable (passengers die with it)? Warning ping at departure? Do humans keep the
  AI ship waygates (today the Broken Isles are reached only through one)?
- [ai] Check WL's defeat handling for hidden units (a bot defeated during a trip): land them or remove them.
- Later idea, not planned: shipyards a bot builds as extra entries. Bots build no ships today and every landmass
  would still need a harbour.

## 8. Unit budget per CP (0.46, agreed)

Problem: 5227 units on the map at 14:12 (0.44.2, first game with bot income); 4000+ units in 0.43.1 with ~2-s order
delay. Every faction may train up to its food maximum (250 for some); bots spend instantly, so they reach it.

Design:
- A bot trains units only while its army food (heroes not counted) is below **budget = base + perCp x held CPs**,
  capped at the faction's food maximum. Default **40 + 10** (maintainer): ~150 food for an average faction once the
  neutral CPs are taken (~187 CPs / ~17 factions = ~11 each); the faction maximum (250) is reached at 21 CPs; early
  (~4 CPs) ~80. Caution: 16 bots x 150 = ~2400 food, only ~20 % below 0.44.2 at 14 min (2986, already lagging) -- the
  0.46 data decides. `-cap off|<base> <perCp>` to try other values in game.
- **Base per faction**: the bot owns the mechanism; the values per faction / team (good vs evil balance) come from
  Shattered Legacies through an `AiApi` hook (`../ShatteredLegacies/TODO.md`). Default: the same base for all.
- A production gate in the bot, not WL's food cap: WL's food state stays untouched (no fight with `FoodMaximum` and the
  bots' food floor of 100).
- The total is bounded by the number of CPs -- the property that matters for the engine. Few factions with mass
  armies are fine: CPs are limited, so not everyone can have one.
- Humans keep WL's normal cap: they pace themselves; bots spend every coin at once.
- Side effects to watch: snowball (more CPs -> more army -> more CPs, on top of the CP income); weak bots cannot rebuild
  to recover (games end faster -- fine for the game-length goal). Capped bots bank gold and rebuild in a burst after
  losses -> waves (section 2's goal, without a depot).
- Numbers: from the 0.46 games -- order latency and stopwatch drift against units on the map; set the budget below the
  point where they start to climb.

## 9. Measuring engine load (0.46)

- **Stopwatch** (maintainer, no code): real time at turns 5, 10, 20, 30, 40. Shows whether the whole simulation falls
  behind (single player: the game slows; multiplayer: lag screens).
- **Order-latency probe** (code): the delay a player feels (orders start 1-2 s late) is WC3's pathfinder queue -- every
  unit's path requests wait in it, the bots' and the player's. The game clock can run on time while orders wait, so
  the stopwatch does not show it. The probe samples orders the bot gives anyway (idle units, <= 8 tracked) and records
  the game time until the unit has moved 32. Pathing is part of the synced simulation, so the result is the same on
  every client -- safe to show, and later safe to use (e.g. a load governor).
- **What the bot costs the engine**: Lua runs in the same frame as the simulation, so a Lua spike is a hitch for
  everything (78k ops in one slot, 0.44.1 -- fixed). Average Lua work is small next to thousands of units. The bot's
  main engine cost is **orders**: each order, and each queued chain point when it becomes active, is a path request
  (~2000 orders/min at 14:12 in 0.44.2). Fewer units AND fewer orders both shorten the queue.
- Load counters in ops (0.40) stay as the spike detector; they do not measure engine load.
