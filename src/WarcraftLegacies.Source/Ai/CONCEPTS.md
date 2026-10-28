# Concepts to explore (bot behaviour)

Ideas that are discussed but not decided. They interact -- movement, gold spending, production and its buildings
belong together -- so decide them together, one version per step, each measured (`MEASUREMENTS.md`). Decided items
move to `TODO.md`. `[you]` = maintainer decision, `[ai]` = implementation.

Guiding rules (maintainer): natural movement first; stay close to the original Warcraft Legacies (no changes to WL's
CP income); simple and not engine-heavy.

## 0. Movement rework (agreed with the maintainer after 0.43.2) -- steps

| Step | Version | Content |
|---|---|---|
| 1 | 0.44 (done) | **Movement chains**: queued attack-moves along route nodes; waygate legs (attack-move in front, right-click on the waygate, onward from the exit); a new chain only when the target changes; units that used up their chain get the next one. Counters: switches per bot, stalled units. |
| 2 | 0.45 | **Gather points (waves)** for chains longer than ~5 nodes: the chain ends at the own node right behind the front; wait until the group has enough food (the 0.41 need) or ~60 s, then the final chain. Short chains go without waiting. |
| 2b | 0.45 | **Extend or re-order** when the target changes during a chain (an ally took D): continue from the chain's end (extend) if that costs at most ~1.2 x the direct route, else a new chain from where the group is. First version (0.44): units finish their leg, then get the new chain. |
| 3 | 0.46 | **Several groups per bot** (2-3), only when the army is clearly bigger than one target needs. Newcomers get their own chain to the gather point and merge there. |
| 4 | later | **Economy**: bonus gold in bursts, load governor (section 2) once the stalled-units counter shows the threshold. |
| alongside | as needed | **Graph quality**: generated pass nodes (narrow passages), nodes on long edges, an exceptions file for what map data cannot know (listed in `WORKAROUNDS.md`); the maintainer can mark nodes / edges on `route_map.png` in a fixed colour (pixel -> map: x = -28672 + 16 px, y = -32256 + 16 (3584 - py)). |

Decisions behind it:
- Attack-move never attacks own / allied units: own CPs on the route are passed, enemy CPs and hostile neutrals fought.
- "A falls behind us" is the defence rule's business (0.43.2: reach + minimum times), not the chain's.
- Group stretching matters only for small groups -- hence gather points by chain length, not always.
- Waygates need no hand-placed nodes: every live waygate gives its entry (the unit) and exit (its destination).

## 1. Waves to the front (now step 2 of section 0)

Model, building on 0.41 (attack group + staging) and 0.43 (target = distance from the territory):
- **Front** = every node that is not ours but borders our land (neutral or enemy). The target is the cheapest front
  node (0.43 already chooses this way).
- **Staging** = our own node next to that target. The army gathers there; what stays behind is the pseudo-defence,
  standing where the fighting is.
- **Wave** = the group attacks once enough food has gathered at the staging node (0.41's need: enemy food x 1.5 +
  structures x 8, at least 50). Then the next front node.

Open questions:
- [you] Threshold for neutral targets: low (they need small groups) or the same as for enemy targets?
- [ai] What do units do while they wait -- hold at staging (no orders = cheapest) or join a defence nearby?
- [ai] How long may a wave gather before it goes anyway (a bot with low income never reaches the need)?
- Risk: waiting slows the early expansion; bases far behind the front only have the recall (Rule 0).

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

"Natural" defence (units that react where the enemy attacks) is not feasible cheaply. Today: recall when a base or CP
is attacked (Rule 0), and since 0.41 the rest of the army waits at the staging CP. Waves (1) put that rest where the
fighting is. Revisit only if waves are not enough.
