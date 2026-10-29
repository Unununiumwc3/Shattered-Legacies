# Changelog -- Shattered Legacies: Botified Realm (bot AI and fork content)

Versions are `0.MINOR.PATCH` (feature build = MINOR, fix-only build = PATCH). Git tags: `v0.39.9` and later (older
builds were tagged `4.8.2-ai.<build>` in the deleted fork). The `-version` command and every `-dump` header show the
same version.

## 0.44.1
Fixes after the first chain game (0.44.0: chains worked -- long orders 4482 -> 615, orders/min ~470-770, stalled units
2-5 % -- but planning them was expensive).
- **One backward route tree per destination**: every unit heading to the same point reads its way from one Dijkstra
  run (from the destination over reversed edges, waygates included). 0.44.0 planned from each unit's own area and
  kept only the last result -- 9261 plans in 25 min, up to 34860 ops in one slot (script work, not visible on screen).
- **Leftover enemy buildings are targets**: every enemy structure counts for the AREA of the node it stands in
  (region map) -- CP nodes included, not only within 2000 of an area node. A CP the bot or an ally already holds is a
  target again while enemy buildings stand in its area (after the 10-min lock; inside the own land with +2500 cost so
  it does not override everything). Stratholme's city survived next to its captured CP. The scan runs in four tasks
  (6 players each) spread over the 15-s cycle.
- Dump ROUTES: every waygate edge by name (from -> to, cost, exit), units found idle at a waygate exit (= the queued
  chain was lost in the teleport), chain trees computed. Waygate hop cost unchanged (300) until the data shows what
  the Dark Portal "in and out" is.
- Version v0.44.1.

## 0.44.0
Movement rework, step 1 (`CONCEPTS.md` 1): movement chains. `-chain off` = 0.43 for A/B tests.
- **Every army order is a chain**: queued attack-moves along the route from the unit's own area, like a player
  shift-clicking attack-move B, C, D (`BlzQueuePointOrderById`, patch 1.32.10). Units follow the graph node by node,
  each on its own (nobody waits for the group's centre), and fight what they meet. Before, the order went straight
  to the farthest node within 8000 -- ground units used WC3's pathing in between, flyers flew straight over water.
- **Waygates on the route**: attack-move to a point 400 in front of the waygate, a queued right-click ON the waygate
  (walk in, like a player), then onward from the exit. Never an attack-move onto the waygate itself (a unit teleported
  by it would walk all the way back to click it).
- At most 8 points per chain; a unit that used them up gets the next chain at the heartbeat. A chain is given only
  when the target changes (plus to units standing idle away from their point) -- no "advance" orders any more.
- Applies to attack groups, the rest moving to the staging CP, refills, defence, kill targets and "hold".
- **Counters** (dump LOAD, `-debug2`): chains issued / points / fallbacks; **stalled units** (have an order, did not
  move 100 since the bot's last army round, did not lose health -- the overload signature; ranged units shooting
  unhurt count too, so compare between games); **switches per bot** (target / attack-defence state since the start --
  the flapping signature).
- Version v0.44.0.

## 0.43.2
Fixes from the first 0.43.1 game: armies walked back and forth ("stop go stop go"), the Scourge shuttled between
Lordaeron and Northrend, dwarves stood at the closed Thandol bridge.
- **Defence with reach and hysteresis** (`AiDefense.cs`). The main cause of the back and forth: Rule 0 took any
  damaged own CP on the WHOLE map, every service, without minimum times -- the army flipped attack <-> defence every
  15-90 s and turned around each time (Scourge: attack Borean Tundra <-> defend Tirisfal / Deathknell, across the
  sea; Druids: attack Feralas <-> defend Felwood / Nordrassil; up to 8 switches per bot in the log window). Now: only
  CPs on the army's landmass within 9000 of it (an own capital there at any distance); a defence lasts at least 30 s
  and as long as enemies stay; after it, no new defence for 45 s unless the threat is within 4000 of the army.
- **Waypoints without detours**: the farthest route node within 8000 must also be walkable without a big detour (route
  cost <= 1.35 x the straight line + 2000). 0.43.1 picked Arathi Highlands from the other side of the closed Thandol
  bridge (near in a line, far on foot).
- **Thandol bridge in the graph**: the generator marked only +-96 around each rock; the walk slipped past the first
  rock at the west railing (Dun Modr <-> Stromgarde "via 15376,560" was not behind rocks). Now +-192: both bridge
  edges are closed while the rocks stand -- Arathi is no target on foot from the Wetlands side until turn 30.
- **Waygate ends by area** (region map), not by the nearest node in a straight line (the Emerald Portal at the Thandol
  bridge was joined to the node across the gorge).
- New tool `tools/ai/render_map.py`: draws the map as the bot sees it -- terrain (water, cliffs, map edge), trees,
  WL's rocks, CPs (size = gold), area nodes, the route graph (closed and waygate links dashed); `--regions` tints each
  node's area. Writes `tools/ai/out/route_map.png` (+ `_small`), never committed.
- Version v0.43.2.

## 0.43.1
Movement fixes from the first 0.43.0 game, and a weaker strength scale (maintainer).
- **Territory = the bot's OWN nodes only.** With the allies' nodes every bot of a team computed the same front -- the
  team's cheapest -- and the whole team marched there as one ball: at 20:00 three bots on Mulgore and three on
  Drustvar, at 25:00 Druids, Black Empire and Sentinels from Kalimdor to Arathi Highlands (9 hops). This was also why
  the Druids did not take their own neighbourhood.
- **Far waypoints**: the order goes to the farthest node on the route within 8000 of the group (a CP rather than an
  area node, if at most 2 nodes back), never to one behind. 0.42.1/0.43.0 ordered to the next node: units stopped
  there and waited until the group's centre came within 1200 ("stop go stop go", ~950 waypoint orders/min).
- **Re-plan only when the target is gone or every 30 s.** 0.42.1/0.43.0 also re-planned whenever the group stood in an
  area that is not on its route -- all the time while walking past other nodes (1778 plans in 25 min, ~1 per bot every
  13 s): new routes, back and forth.
- **Strength vote shifted down**: Recruit -13, Soldier -6, Veteran 0 (default), Commander +13, Warlord +25 (the old
  Veteran) per production turn. Bots already get free techs, heroes and revives; the old Veteran made 250-food armies
  and 4000+ units, orders took ~2 s to start (engine overloaded) -- which also spoiled the movement tests. Negative
  bonus = bots lose gold (never below 0).
- Version v0.43.1.

## 0.43.0
Natural expansion: the route planner chooses the TARGET by distance from the faction's territory, not from its attack
group. Movement only -- waves, gold and production are in the new `CONCEPTS.md`.
- **Target = nearest to the territory**: one Dijkstra from all own and allied nodes at once (CPs, and area nodes where
  an own or allied structure stands -- no home region to configure). Neutral CPs in the own area come first, then the
  front. 0.42 measured from the group: once it had marched off, the neutral CPs at home were "far" and the enemy
  ahead "near" -- Druids went north and skipped their neighbourhood, new troops too. Offline check on the generated
  graph (Druids own Nordrassil + Moonglade, group in Winterspring): from the group -> Winterspring, Ascendant's Rise,
  Azshara Coast; from the territory -> Shrine to Malorne, Felwood, Auberdine.
- The **route** is still planned from where the group stands (second Dijkstra); the best-scored target the group can
  reach wins. Same rules: neutral half cost, enemy nodes on the way cost extra, stickiness, 30-s re-plan.
- **Quest CPs count 0.3 only on the front** (bordering the territory): a far quest CP waits until the territory
  reaches it (late-game quests no longer pull armies across the map).
- Dump ROUTES per bot: "score", "front" / "inside", "quest", hops left, next node.
- New `CONCEPTS.md`: waves to the front, gold depot / bursts, where bots produce, quest objectives beyond CPs,
  defence -- open questions to decide together.
- Version v0.43.0.

## 0.42.1
Fixes for the route planner after the first game (0.42.0: groups ran back and forth, fought across the closed
Thandol bridge and at the Outland portals, dwarves ran over a mountain, Druids skipped their quest CPs).
- **Route start = the area the group stands in.** The generator now also writes a region map (which node's area each
  256-unit cell belongs to, `AiRouteData.Regions`). 0.42.0 started at the node nearest in a STRAIGHT line -- across the
  Thandol gorge or a mountain (a group behind Fuselight "stood at" Twilight Shore, 3267 away in a line, 10861 on
  foot) -- and planned from the wrong side.
- **Group centre on one landmass** (the biggest part, like the army anchor since 0.36). A group half through a waygate
  had its centre between Azeroth and Outland and was sent back and forth at the portal.
- **Committed routes**: a bot plans, then follows its route node by node and never goes back. It plans again only
  when the target is no longer valid, the group stands in an area that is not on the route, or every 30 s (and then
  switches only to a target that costs less than half). 0.42.0 planned on every service: back and forth to dead ends
  (Gnomeregan, Terrordale), ~570 waypoint orders/min, and the planning ran in every combat slot.
- **Quest CPs first**: a CP where an open objective of one of the faction's open quests lies counts 0.3 of its
  walking cost (read from WL's `Objective.Position` -- works for every faction and quest, no names).
- **Defence ignores enemies that cannot walk to the CP** (other area and no open edge, e.g. across the closed
  Thandol bridge). Both armies stood at the banks and shot across.
- Area targets must be visible and vulnerable (hidden / invulnerable structures were targets that could never fall).
- `ROUTES.md` lists every edge with the point where the two areas touch ("via") -- check suspicious edges there.
- Version v0.42.1.

## 0.42.0
Refactor phase 3: route planner on one generated graph. New default `-route on`; `-route off` = 0.41's targeting
(the hand-written frontier graph) for A/B tests.
- **Graph generated from map data only** (`tools/ai/gen_routes.py` -> `AiRoutes.g.cs`, report `ROUTES.md`), so it
  can run on other WL versions: CPs = units with an income marker ability (the same rule WL uses); walkable grid =
  WC3's 32-unit pathing cells + trees + rock doodads; two nodes are linked when their areas touch, cost = walking
  distance. 187 CPs + 62 area nodes, 356 edges. 178 of the 220 hand-written edges come out directly; the rest walk
  through another node, or are waygate / ship links (13), or detours to check (7, listed in `ROUTES.md`).
- **Area nodes**: walkable ground farther than 3000 (walking) from every CP gets a node. It is a target only while an
  enemy structure stands within 2000 (checked every 15 s) -- harbours, camps, bases without a CP.
- **Live edge state**: edges through WL's removable rocks (`LTrc`, 9 groups, e.g. the Thandol Span) are closed while
  the rocks stand (every 15 s); every active waygate of the neutral players (WL's and the AI's portals) is an edge
  (every 30 s). The "frontier behind a barrier" problem (Ironforge -> Arathi through an ally node) is gone by
  construction: with the rocks standing the route goes around through Stromgarde.
- **Target**: Dijkstra from the node nearest to the attack group (or the army), over open edges; lowest walking cost
  wins (neutral CPs count half, walking through an enemy-held node costs 2000 extra), current target kept unless the
  new one costs less than half. Same rules as before: 10-min lock, uncapturable, Argus Legion-only before the summon,
  skip nodes, stuck blacklist. Nothing reachable -> the old frontier picker.
- **Waypoint orders**: the group goes to the first node on the route more than 2000 away (or the node behind a
  waygate -- the engine walks through it), not across the map in one order. A new waypoint only moves the group on
  ("advance" in the army orders line); a new target picks a new group.
- New dump section ROUTES (graph state, per bot target / cost / hops / next node). `-route on|off` (Control).
- Not replaced yet (only bypassed while the planner finds a target): the edge sets, graph merge, goals and doctrine
  steps.
- Version v0.42.0.

## 0.41.0
Refactor phase 2: army orders (`AiArmy.cs`). New default `-orders smart`; the old modes stay for A/B tests
(`-orders idle` = 0.39-0.40, `-orders all` = before 0.39).
- **Attack group**: the units nearest to the target until their food covers the need -- enemy food within 1200 of
  the target x 1.5 + enemy structures there x 8 (towers, the CP's defenders), at least 50 food. Enemy = an enemy
  player's unit or Neutral Hostile (creeps guarding neutral CPs). If the need is 80 % of the army or more, the whole
  army goes. A group that lost half its food is refilled with the nearest other units.
- **The rest waits at the staging point**: the bot's own CP nearest to the target (on the target's landmass). No
  owned CP there -> the rest stays where it is. Defence, kill targets and "hold" still use the whole army.
- **Orders only when needed** (fixes lever 1's hole): on a target change the group and the rest that is away from
  the staging point; on the 20-s heartbeat only IDLE units AWAY from their point (group 600 from the target, rest
  1500 from the staging point). Before, idle units standing at the target were re-ordered every heartbeat
  (~3700 orders/min in a 538-min game).
- **"Hold" is throttled** like any target. Before, a holding bot re-ordered every unit on every service (~every
  7 s with 15 bots) -- a second, unlisted order source.
- Units that cannot move (move speed <= 1, the Old Gods' god heroes), dead units, Ragnaros' hero and units on a
  hero visit get no army orders (also in the old modes).
- **One unit pass per service**: the order code uses the army that `Process` already collected (before, it
  enumerated every unit of the bot a second time). The combat "units touched" numbers drop accordingly.
- Readouts: `-debug2` third line and dump LOAD: orders by reason (change / idle-away / rest / refill) and per bot
  "group food / need of army @staging CP".
- `dump_summary.py` keeps one game only (the build most dumps share, the order counter never going back) and says
  which dumps it skipped; the 0.39.10 measurement had an old 0.39.9 dump mixed in. File times that cannot be right
  show "-".
- Version v0.41.0.

## 0.40.0
Refactor phase 1: scheduler + load budget. Same tasks, same periods, same order -- no gameplay change.
- **One scheduler** (`AiScheduler.cs`) instead of seven periodic timers: a 0.25-s slot timer; every task has a period
  and a fixed phase (deterministic, the same on every client). Combat every 0.5 s and production every 1 s as before;
  the 15-s wave tick is split into its 29 subtasks, one per odd slot of the 15-s cycle, in the old order (gates first,
  Legion check before the portal watchdog). Also in the scheduler: Legion events (2 s), early quests (3 s), timed
  quests (30 s), hero revive (180 s). Before, every 30 s about 31 tasks ran in one frame; now at most 3 per slot.
  Within a 15-s cycle a subtask may run up to 14 s later than before.
- Debug output (`-debug`, `-autodump`, heartbeat) keeps its own timers: the heartbeat keeps writing even if the
  scheduler breaks. The heartbeat's task list is longer (48 entries; combat and production are in it now).
- **Load counters** (deterministic only): per task runs and ops (last / max / sum; ops = units enumerated +
  production checks + wave work + orders), ops per slot, **orders by distance** (<2k / 2-8k / >8k) and **idle
  re-orders of units already at their target** -- the two proxies for engine pathing cost (and the baseline for 0.41).
  Shown in `-debug2` (second line) and in the dump's LOAD section (one line per task); `dump_summary.py` picks them up.
- **Load budget** 400 ops per slot, report-only ("over", "would-defer"). `-budget on|off` (Control) lets deferrable
  tasks (not combat / production) slide to the next slot after a busy slot -- a test switch, default off.
- "wave cycle" in `-debug2` / LOAD now counts all work of the wave subtasks (before: only their unit loops).
- Version v0.40.0.

## 0.39.10
Refactor step 0: the bot and the fork's own content are separate layers. No gameplay change except the two fixes.
- **Fallbacks moved** from `Ai/Fallbacks/` to `src/WarcraftLegacies.Source/ShatteredLegacies/` (namespace
  `WarcraftLegacies.Source.ShatteredLegacies`), with `FALLBACKS.md` and `FACTION_UNITS.md`. New entry point
  `ShatteredLegaciesSetup.Setup()`, called from `Setup/GameSetup.cs` right after the bot (same timing as before).
- **`Ai/AiApi.cs`**: the bot's public surface for code outside `Ai/` (players, logging, `Safe`, build spots) and data
  hooks (`RegisterHeroes`, `AddProductionData`). The fork code no longer reads `SimpleBot` internals.
- **The bot no longer knows fork data**: the Kezan Enterprise hero list left `AiConfig`; `gen_ai_data.py` writes the
  Kezan buildings' train lists to `ShatteredLegacies/SlAiData.g.cs` (merged at setup -- same tables as before).
- `check_refs.py` checks both folders and the layer rule: LAYER (bot code uses fork code), API (fork code bypasses
  `AiApi`).
- **Markers** in original files: `// [BOT]` (hooks the bot needs), `// [SL]` (fork content), `// [WORKAROUND]`.
- New docs: `Ai/WORKAROUNDS.md` (everything the bot needs outside `Ai/`, with "blocks PR"), `Ai/MEASUREMENTS.md`
  + `tools/ai/dump_summary.py` (a game's dumps -> summary in the repo), `ShatteredLegacies/README.md` (all fork
  changes to the original map / code, incl. the map data lists the context file had lost track of),
  `ShatteredLegacies/TODO.md` (fork work and the known issues from the root README). `Ai/TODO.md` rewritten: refactor
  phases 0.40 scheduler, 0.41 army orders, 0.42 route planner (+ waypoint orders, objective nodes); outdated facts
  fixed (vote at 49 s, no invasion waves).
- Fix: the Kezan Enterprise bot now trains Gallywix first -- the hero list was keyed "The Kezan Enterprise", but the
  faction is named "Kezan Enterprise" since 0.39.7.
- Fix: the Scarlet Crusade trigger building no longer says "WL event" ("story event").
- Version v0.39.10.

## 0.39.9
- **Book of Medivh** gets a third ability: **Guardian's Abjuration** -- WC3's Big Bad Voodoo (`AOvd`) as an item
  ability, 5 seconds (map data: ability `AGAB`, item `I006`).
- Docs: all md files reviewed (outdated versions / numbers fixed, playtest list consolidated, a "Where we are" handover
  section at the top of TODO.md).
- **Rebrand** to **Shattered Legacies: Botified Realm** at the original maintainer's request: map name, loading
  screen, lobby author/description, credits quest and hints say it is an unofficial fork; Discord links removed;
  published file `ShatteredLegacies-BotifiedRealm-v0.39.9.w3x`. The map title now comes from `Info.json`.
- The "WL AI" chat banner at game start is gone. New command `-version` (works in every build, also `Access = Off`)
  shows the map name and bot build; `-help` says "Bot AI".
- Build fix: duplicate `AiGraph.ArgusNodes` removed.
- Version v0.39.9 (new tag scheme; first release in the Shattered-Legacies repo).

## 0.39.8
- **Kezan Enterprise buildings** (maintainer's design; map data): Laboratory trains Sappers + Alchemist (function
  units / casters); Mercenary Den trains Merc Gunner (renamed Gunner), Ogre Mercenary, Trade Prince Guard; Tank Factory
  also trains the Goblin Shredder; Scrapyard is the tech building (Cluster Rockets; Personal Tank and War Zeppelin need
  it); Bunker already is a food tower (orc Burrow base). **Kezan Oil Supply: +50 gold per minute each** while it stands
  (`Fallbacks/KezanEnterprise.cs`, WL's BonusIncome; cannot be captured). **Kezan Trading Center: creep market** --
  Ogre Warrior, Ogre Magi, Forest Troll, Troll Shadow Priest, Murloc Huntsman, Centaur Outrunner; stock 3 each, +1 every
  2 min (by code: max allowed = alive + stock). Bots train Laboratory / Mercenary Den units (generated tables).
- **Scoreboard after the Legion betrayal**: the Scourge leaves through WL's `Faction.Unally()` ("Scourge Pact"), so the
  scoreboard follows (like the Kezan Enterprise).
- Version 4.8.2-ai.0.39.8.

## 0.39.7
- **Kezan Enterprise** (Kul Tiras fallback, from the maintainer's test): name without "The" (cut off on the
  scoreboard); everything neutral on Kezan (Regions.KezanAmbient: Scrapyard, Bunker, Laboratory, Rocket Tower, Oil
  Supply ..., passive AND hostile units) joins; WL's own `Faction.Unally()` gives it its own team ("... Pact") so the
  scoreboard follows; all three Altar of Industry heroes allowed (Gallywix, Tinker, Marin).
- **Argus is Legion-only before the summon**: other factions no longer pick Antoran Wastes, Eredath, Krokuun or
  Nath'raxas Hold as targets until the Legion is summoned (the Scourge took Antoran early; weight 15 was not enough).
- **Fallback buildings**: 3 columns around x 22208 from y 23500 northwards, 150 apart (maintainer's spot; 100
  overlapped the models): middle column first, then left, then right; order = team, then slot.
- Version 4.8.2-ai.0.39.7.

## 0.39.6
- **Kul Tiras fallback -> "The Kezan Enterprise"**: the faction is renamed, Katherine stays, the maintainer's new
  announcement. Start buildings at Kezan from unused main-map goblin data: goblin hall `o03L` (Tinker), Altar of
  Industry `o03O` (Gallywix, Tinker hero, Marin), Tank Factory `o05T` (Grenadier, Personal Tank, War Zeppelin), Kezan Oil
  Supply x2; kept from Kul Tiras: Garrison (Marines), Workshop (Cannons). Their units are allowed per player
  (`SetPlayerTechMaxAllowed`); `gen_ai_data.py` adds these buildings to the bots' train tables; Gallywix heads the
  Enterprise's hero list.
- **Hero visit "Gul'dans Legacy"** (Fel Horde, Gul'dan's corpse in the Tomb of Sargeras): level-12 hero, only when the
  Tomb is open (gate "tomb"), the Fel Horde holds Broken Shore and its army is within 3500; idle army units near the
  target escort the hero. New per-visit options `WithArmy`, `RequireGate`.
- Version 4.8.2-ai.0.39.6.

## 0.39.5
- **Fallback buildings in a symmetric layout**: one column per team (scoreboard order), one row per member.
- **Kul Tiras fallback** (maintainer's design): Katherine is moved to Kezan; everything else Kul Tiras owns goes to
  Stormwind (if it still plays) or is removed; Kezan and the Neutral Hostile creeps within 3000 become Kul Tiras';
  Kul Tiras loses all allies; announcement "Boralus lies in ruin ...". No placeholder hero.
- **Graph**: removed the "wall" edges Seradane-Corin's Crossing, Hive'Regal-Silithus, Hive'Zora-Uldum,
  Uldum-Hive'Ashi; added Ramkahen-Throne of the Four Winds and Scarab Wall-Hive'Zora (weight 0.5).
- **New CPs**: Twilight Shore (Twilight Highlands, the corrected spot) and **Deathknell** (Tirisfal) -- Deathknell
  goes to the Scourge when "Plague of Undeath" lands, like Terrordale.
- **100-gold CPs** (evil buff, new marker `A1HG`/`B1HG` + one line in WL's `ControlPointSetup`): Ny'alotha (the Waking
  City), The Vortex Pinnacle, Temple of Ahn'Qiraj, Icecrown Citadel.
- **Kul Tiras "Continental Outpost"**: a bot Kul Tiras gets the 5 required buildings at Western Stranglethorn once it
  or an ally (Stormwind) holds the CP; the quest is completed (WL counts only BUILT buildings).
- **Bugs**: Emerald Portals duplicated -- the preplaced pair was hidden when the AI pair replaced it; now the
  preplaced pair itself is activated once both are visible. Rift Highlands -> Maw did not work: the Maw end moves to
  (-27768,-7864), where the whole landing ring is free of doodads.
- Version 4.8.2-ai.0.39.5.

## 0.39.4
- **11 new CPs** (10 gold, maintainer's spots, map data `n1A0`-`n1B1`), each connected to its two nearest graph nodes
  (check with `-edges`): Scarab Wall, Hive'Zora, Hive'Ashi, Ramkahen, Hive'Regal (the way to Ahn'Qiraj), Seradane
  (Hinterlands), Broken Shore (Broken Isles), Dragonmaw Gates, Thundermar (Twilight Highlands), Sporeggar, Sylvanaar
  (Outland). (-16640,16829) was taken as (-16640,-16829) (the first is sea). (19065,-8289) left out: no walkable
  ground within 600.
- **Sulfuron Spire CP moved** to (-11160,4520), next to the Spire (the given -1169,4552 is on the Broken Isles).
- **Fallbacks for all factions**: a trigger building for every faction in the corner. Good factions get a
  placeholder fallback (Marin Noggenfogger at the planned place: Netherstorm, Darkshore, Borean Tundra, Storm Peaks,
  Kezan, Duskwood, Warden's Cage, Shattrath, Wetlands). Evil factions: building only. HQs = WL's essential legends;
  where there are several, ALL must be lost, and a recaptured HQ makes the fallback unavailable again. Ironforge's
  HQ is now the Great Forge (was the Ironforge CP).
- Version 4.8.2-ai.0.39.4.

## 0.39.3
- **Build fix** (0.39.2 did not compile): `IsActive(...)` was called without `SimpleBot.` from another class
  (`QuestAutoComplete`). `check_refs.py` now reports unqualified calls to methods of other classes (UNQUALIFIED) --
  it reproduces this error.
- From the 520-min test (0.39.0, no freeze, `-orders idle`): **graphs merge** when the Old Gods rift opens and at the
  latest at 60:00 -- without a Legion summon the two continents' graphs never merged, so Illidan / Fel Horde won the
  Eastern Kingdoms and never targeted Kalimdor. Home first still applies.
- **Sulfuron Spire CP** (new map data `n0SP`, 10 gold, in front of the Spire, edge to Northern Ashenvale): at the
  Fireland Invasion it goes to Skywall (an early Druids owner's tower killed the new buildings), and a bot Skywall
  moves its army there.
- **The Nexus CP** (new map data `n0NX`, 10 gold, edge to Borean Tundra) -- replaces the Nexus area clear.
- Version 4.8.2-ai.0.39.3.

## 0.39.2
- **Bots take over leaving humans**: WL used to defeat a leaving player's faction; now the AI plays the slot on as a
  full bot (bot bonuses) and everyone sees "<faction> is now played by the AI". WL change: one hook in
  `GameLogic/GameEnd/PlayerLeaves.cs` (falls back to WL's defeat if the AI declines). Taken-over slots count as active
  opponents (`SimpleBot.IsActive`).
- **Starter production**: at 0:35 every bot gets each production building of its faction it does not own yet (one of
  each, only types that train units), placed around its workers with the Argus ring search. Pin per bot: "starter
  production -- placed: ... | NO ROOM: ..." (maintainer checks where it worked). Fixes "never trained" units of
  buildings bots never had (Academy, Consortium, ...).
- Docs: `FALLBACKS.md` -- HQ list per faction (from WL's essential legends, input needed), "lighter fallbacks"
  (remove / add / re-tune units per player instead of full race changes); new generated `FACTION_UNITS.md`
  (`tools/ai/unit_scan.py --md`) for keep / lose decisions; TODO: guardian summons decided (bots only, 24 units).
- Version 4.8.2-ai.0.39.2.

## 0.39.1
- **No game-mode vote**: Closed Alliance is forced (WL change in `Setup/GameSetup.cs`: `ClosedAlliance.OnChoose()` at
  59 s, when the vote used to end). The bot difficulty vote moves into the freed slot: 49 s instead of 70 s.
- **Fallbacks, first slice** (new folder `Ai/Fallbacks/`, design in `FALLBACKS.md`): one **trigger building** per
  player slot with a fallback, in the sealed north-east corner (WL only creates artifact items there). New map data:
  unit `n0FB` "Fallback" (invulnerable, no attack) and ability `AFB1` "Begin the Fallback" (channel). Dalaran
  (Theramore) and Lordaeron (Scarlet Crusade) are listed as WL-driven. **Placeholder**: Ironforge loses its
  Ironforge CP -> a bot falls back at once, a human gets the ability -> Marin Noggenfogger (`VH01`) appears in the
  Storm Peaks with an announcement. No team change, base or unit package yet.
- Ship to Tol Barad: mainland end moved to 13007,-2039 (maintainer).
- AI portal minimap icon: `MinimapIconGoldmine.blp` (the square from `-icontest`; NeutralBuilding.blp showed nothing).
- Tools: `check_refs.py` and `scope_check.py` read subfolders (Fallbacks/).
- Version 4.8.2-ai.0.39.1.

## 0.39.0
Engine-load refactor started (step 0 + lever 1), fixes from the 0.38.1 playtest (froze at ~34 min WITHOUT alt-tabbing).

Refactor
- **Step 0 -- measure**: new dump section UNITS (units / structures per player, total, orders since the start, order
  mode). New **heartbeat file** `WLAI_heartbeat.txt` while `-autodump` is on: every 10 s game time, units on the
  map (counted every 60 s), orders per minute and the last 12 started periodic tasks -- after a freeze it shows the
  last state before it.
- **Lever 1 -- order discipline**: with an unchanged target the 20-s heartbeat only orders IDLE army units (new units
  after their rally point, units done fighting); a new target still orders the whole army. Switch `-orders idle|all`
  (default idle) for A/B tests in one build.

Fixes / gameplay
- Scourge no longer marches into Argus at the start: Argus <-> Crystalsong (pre-summon portal) weight 6 -> 15.
- **Bot Legion's Argus start**: Nether Pit x2, Summoning Circle, Infernal Siegeworks, Demon Gate instead of 5 Nether
  Pits (which only produced Felguards).
- **Legion supports the plague**: when "Plague of Undeath" lands, a bot Legion's army is moved next to Alterac
  Mountains (it could not reach Lordaeron, the invasion fizzled).
- **Emerald Portals** (Shrine of Malorne <-> Feralas): WL's preplaced pair had no destination; replaced by a working
  AI portal pair + graph edge (Ysondre / Taerar area).
- **Feathermoon**: a bot Sentinels holding Feathermoon gets 3 different buildings there and the quest "Shores of
  Feathermoon" (Maiev) is completed -- WL counts only buildings that were BUILT, which bots never do.
- Hero visits: **"Vault of the Wardens"** (Maiev into the channel area) and **"A Feast for Our Kin"** (Cairne into
  Highmountain, while the Frostwolf or an ally holds Stormheim -- new per-visit option `AllyCp`).
- `-icontest N`: one minimap icon variant at a time on the selected unit.
- Version 4.8.2-ai.0.39.0. Docs: `FALLBACKS.md` (new) holds the fallback project; `TODO.md` points to it.

## 0.38.1
From the 0.38.0 playtest (45 min; 0.38.0 was played but never pushed -- this release contains it).
Production check: every bot trained 7-14 unit types, quest heroes appeared (Quel'thalas: Rommath, Lor'themar),
LOAD about as in 0.37.2.
- **Terrordale** goes to the Scourge the moment WL's "Plague of Undeath" rescues the Stratholme Scourge base around
  it (checked every 2 s). Before, an early North Alliance owner's CP tower destroyed the new Scourge base.
- **Yogg-Saron**: the hero visit only for a bot that holds Storm Peaks; afterwards the bot that owns the prison
  casts "Free Yogg-Saron" (WL's spell; bots never did), and the Storm Peaks holder clears Yogg (Val'anyr).
- **Area clears** no longer give up when their target is not found: Ysondre was hidden at 2:00 and the clear was
  wrongly marked "already dead"; now it is retried later (also needed for Yogg, who only appears when released).
- **Argus weights** (0.38 test: the summoned Legion still went straight to the Sunwell): Argus <-> Quel'Danas and
  Argus <-> Crystalsong 6 (were 3), Argus <-> Felwood 1 (was 2).
- **Old Gods rift, second trigger**: a faction outside the Old Gods holds the Maw of Gor'ma and N'Zoth (Black
  Empire's immobile Old God) is dead -> the rift opens for the victors, with its own lore text and pings.
- **Faction picks**: the Sentinels slot is random for bots too (Sentinels / The Exodar). Illidari stays Basic.
- **Minimap icons** did not show in 0.38: new `-icontest` (heavy) shows six icon variants to find the working one.
- Version 4.8.2-ai.0.38.1.

## 0.38.0
From the 0.37.2 playtest (65 min, 15 bots).

Fixes
- **Black Empire marched east from Kezan into nothing** -- my 0.37.1 bug: the rift edge Maw of Gor'ma <-> Dragonmaw
  Port was also in the Eastern Kingdoms edge list, which made the Maw an EK node; Black Empire starts owning it and
  resolved to the EK graph at 0:05. The edge is now in the Kalimdor list only, and the Old Gods' default edge set is
  Kalimdor. Once the rift is open they walk the whole world (home Kalimdor first).
- **Old Gods rift only worked one way:** the Maw end stood on a Silithid archway + obelisk (doodads block ground
  units, and the terrain pathing data doesn't show them). Moved 530 south to -28049,-7701.
- **Gates that appear during the game** are removed the moment they enter the map (trigger on the whole map,
  one type lookup per new unit); the 5-min whole-map sweep stays for gates that were already on the map.

Gameplay
- **Argus weights** (maintainer): inside Argus 0.5, Argus <-> Felwood 2, Argus <-> Quel'Danas / Crystalsong 3 --
  in and out of Argus is expensive, Felwood cheaper than the Sunwell side.
- **Ragnaros** only for a bot that controls Blackrock Depths (no more level-12 race across the map).
- **Area clears** for the green dragons, only while they live: Ysondre by whoever holds Shrine to Malorne, Taerar
  by whoever holds Feralas.
- **Three 1-gold CPs** (new map data) so bots attack areas they used to skip: Sunsail Anchorage (Quel'thalas
  harbour, edge to Silvermoon), Terrordale (Scourge camp, edge to Stratholme), Runestone Falithas (edge to Eversong
  Woods). New income marker `A0ZG` / buff `B0ZG` (copies of the 10-gold marker) + one line in WL's
  `Setup/ControlPointSetup.cs` (`new(FourCC("B0ZG"), 1, true)`).
- Version 4.8.2-ai.0.38.0.

Production (from the unit scan)
- **Units come from the buildings' real train lists** (new generated `AiData.g.cs`, `tools/ai/gen_ai_data.py`).
  Before, production sent category pools (Fighter -> barracks, Tank/Siege -> siege workshop, ...) to building kinds
  and could train only **105 of 448** unit types (Quel'thalas: 4 of 22 -- no Swordsman, Farstrider, Spellbreaker,
  Dragonhawk ...). Now **248** (the rest are ships, builders and quest/summon units). Each building trains the type
  this bot has ordered least (random among equals) -> an even mix; only types whose requirements are met (researches
  / buildings via `GetPlayerTechCount`). Town halls and "Specialist" buildings now train too. Unmodified vanilla
  buildings (Crypt, Ancients) use WC3's default lists. Old category rule kept as fallback.
- **New heroes are free for bots** (like researches and upgrades -- before, only the revive was free): heroes are
  trained first, at no cost. Quel'thalas never had more than ~110 gold in the 0.37.2 test, so Rommath / Lor'themar
  never came although "The Blood Elves" was done. Candidates: AiConfig.Heroes (priority) + every hero of the altar.
  A fair autopilot (no perks) pays and saves gold for an available hero instead.
- **Heroes learn their skills** (`AiHeroSkills.cs`): auras/passives first, then the rest, attribute bonus last; on
  level-up and every 15 s for unspent points.

Gameplay
- **Dalaran slot random for bots** (Dalaran / Gilneas), like the orc slot since 0.37.1. Humans keep their dialog.
- **Legion summon only forced for a bot Legion**; a human Legion summons through WL's ritual. New second trigger
  (lore: Book of Medivh in Dalaran): once Dalaran's capital is dead a bot Legion sends a unit to the pedestal
  (hero visit "Book of Medivh"); the Legion holding the Book -> summon.
- **Small minimap icons on all AI portals** (ships blue, Legion/rift purple), destroyed with the portal.

Analysis
- **New dump section TRAINED** (`AiStats.cs`): per bot, every unit/hero type it finished training with counts, the
  faction's categorised types it NEVER trained, and "SAVING for a hero". Event-based, no scans.
- **New `tools/ai/unit_scan.py`**: per faction, every unit and hero, whether the AI trains it and where, and the
  quest a requirement depends on.

Docs: TODO.md restructured around the agreed refactor (scheduler, idle-only orders + attack groups, route planner,
economy, cleanup) and the 0.39 unit statistics; AI_AGENTS_CONTEXT.md: map-data facts corrected and extended.

## 0.37.2
Small fixes on top of 0.37.1 (playtest still running); docs reworked.
- **Thandol bridge:** the Arathi Highlands <-> Dun Modr edge opened at a guessed 20:00, but the bridge is blocked by
  WL's invulnerable "Bridge Rocks" (`LTrc` in `Regions.BridgeAmbient`, removed by WL at turn 30). Dwarves stood at
  the rocks for 10 minutes. The edge now has the gate "thandol", opened by `AiGates` when the rocks are gone.
- **Stuck back-off:** a target the bot gets stuck on again is skipped longer each time (5, 10, 20 min) -- the
  dwarves also came back to the rocks every 9 min through an ally-held node north of the bridge.
- **Jaina's Sanctum CP** (new map data): unit type `n0JS` (copy of Alcaz Isle's CP, 10 gold/min) placed at
  -7616,-9152 in Theramore (`mapdata/WarcraftLegacies/UnitData/n0JS.json`, `Units/-7168_-8704.json`); graph edge to
  Alcaz Isle.
- Version 4.8.2-ai.0.37.2 (`AiSetup.Build`, `appsettings.json`).
- Docs: `AI_AGENTS_CONTEXT.md` reorganised (hard rules 9-11 from the 0.37.1 build break: a local hiding a type,
  no compiler in the sandbox, file encodings; new "Map data" section incl. how to add a CP; WL facts grouped);
  `README.md` "Releasing" with the git commands; `TODO.md` refreshed.

## 0.37.1
(0.37.0 was never released -- its changes are part of this build.)

Build fix (first 0.37.1 drop did not compile): the bot hook in `ChoiceDialogPresenter.Run` called `timer.Create()`,
but `Run` declares a local named `timer` further down -- in C# that local hides the `timer` TYPE in the whole method
(CS0841). The bot pick is now its own method `TryBotPick`. New `tools/ai/scope_check.py` catches this kind of error
(a local hiding a type; the same local name in a nested block) -- run it before every drop.

Map
- **Own map name:** "Warcraft Legacies |cffff0000Botified|r 4.8.2-ai.0.37.1" (lobby and loading screen, "Botified"
  in red) -- `SetMapTitles` in `src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs`. The version comes
  from `MapSettings.Version` in `src/WarcraftLegacies.CLI/appsettings.json` (now `4.8.2-ai.0.37.1`); it is also part
  of the published file name, so every AI build is its own `.w3x`.

Gameplay
- **Bots pick in WL's click menus** (new `AiChoices.cs` + a small hook in
  `MacroTools/UserInterface/ChoiceDialogPresenter.cs` / `BotChoicePolicy.cs`, only for computer players). Before,
  every menu timed out for bots and took option 1: orcs were always Frostwolf and the Scourge's plague always chose
  "No invasion". Now the orc slot is Frostwolf or Warsong at random, and the Scourge invades Scholomance,
  Stratholme or Deathknell at random. Other menus keep WL's default. Decisions use the choice objects, never the
  (localized) names -- comparing names could desync.
- **Old Gods rift replaces the Old Gods wave:** when an Old Gods faction takes Nordrassil, a real waygate pair opens
  between the Maw of Gor'ma and the Twilight Highlands (the unused preplaced portal `n07E` there becomes the real
  end), usable by every player; lore text for everyone and pings on both ends; graph edge Maw of Gor'ma <->
  Dragonmaw Port behind the gate "oldgods". `-ogwave` is replaced by `-ogrift`.
- **Sulfuron Spire:** after Skywall's "Fireland Invasion" each Old Gods bot gets one barracks beside the Spire, so
  its troops also spawn there.
- **Ramzes the Horror** (Desolace): area clear for whoever holds the Desolace CP -- only while Ramzes is alive.
- **Archimonde** walks into WL's channel spot next to the World Tree once the Legion holds Nordrassil (hero visit
  "Twilight of the Gods"). Hero visits no longer re-order a hero standing at its target (that broke the channel)
  and never give up there.
- **Arthas:** the Scourge trains Arthas first (he was last in the list); his later form (Lord of the Scourge) counts
  as alive, so he is not trained twice. The AI no longer forces Lordaeron's "Line of Succession" at 30:00 -- that
  made the Scourge's "The Culling" impossible.

Fixes (from the 0.36 playtest dumps)
- **Old Hatreds** looped all game: Daelin was sent at level 1 from 1:00 and died on the way 11 times. Now level 10
  first, and every hero visit pauses 5 min after "hero lost".
- **Stranded heroes:** the teleport to the altar did nothing when the hero already stood there (Black Empire's
  isolated realm, Scourge heroes at the Northrend base). Then the hero now goes to the bot's own CP nearest its army.
- Hero visits "Lone Wanderer" / "Jailor of the Damned" removed (Frostwolf never reaches Northrend).

## 0.36.1 (docs only -- the in-game banner stays 0.36.0)
- TODO.md: 0.37 plan agreed in discussion -- economy via WL's `BonusIncome` from 11:00 with food upkeep on the bonus
  (100 / 150 / 200), Arthas priority + no forced "Line of Succession", a bot hook for WL's click menus (random orc
  pick, Scourge always invades), Warsong AI data, CP graph weights from the pathing data, Old Gods portal instead of
  the wave; 0.38+: melee-like performance work. New "Freeze check" steps (0.36.0 froze in minute 1 with heavy
  alt-tabbing on Proton) -- no 0.37 code before that is settled.
- AI_AGENTS_CONTEXT.md: WL facts found in the source (economy and `StartingGold`, click-menu defaults for bots,
  Arthas hand-over, Book of Medivh, Highbank, Warsong-only subdue quests).

## 0.36.0
Gameplay
- **Army anchor = biggest group on real ground.** The target anchor was the centroid of the whole army; a winner split
  over two continents (85-min test: Sentinels 98 on an island + 84 on the mainland) had it in the sea and changed
  targets every few seconds. Now the biggest same-landmass group decides (dump: `anchor=army@x,y(Nu)`).
- **Target stickiness:** a bot keeps the frontier it is marching on unless another one is in a better class
  (home graph / goal route) or clearly closer (< 0.5x the squared score, about 30 % closer). Dump: `kept=1`.
- **Food floor 100** for bots (perks): food cap and ceiling are raised to 100 every production turn, never lowered.
  Humans killing a bot's farms no longer stops its production for good.
- **Stranded heroes** (new `AiHeroCare.cs`): a bot hero > 4000 from its army that hasn't moved for 2 min gets a move
  order back; still stuck 2 min later -> put beside its nearest living altar (perks only). Mission heroes and heroes
  that can't walk are left alone.
- **Fel Horde demon gates:** Unfocused Demon Gates (n081) are upgraded for free to Fel Hound / Voidwalker gates from
  15:00, their second tier from 20:00 (WC3 still enforces the hall tiers). `AiUpgrades.TimedUpgrades`.
- **Argus start:** the bot Legion's 8 buildings go to fixed spots computed from the map's pathing data (a flood fill
  confirmed the portal and both Argus CPs stay reachable). The ring search's +-220 footprint test rejected 7 of these 8
  spots -- the reason only 3 buildings appeared. Ring search kept as fallback; the pin says how many used fixed spots.
- **AI portals burn less:** n036's two "Anti Portal Blocking" immolations (A01M units, A023 structures) have a 1000
  range; on AI portals they are set to 250 (landing spots are 300+ away).
- **Hero visits** (`AiConfig.HeroVisits`) can name a legend (`Hero`), wait for the quest's other objectives
  (`LastObjective`) and require the hero to be on the target's landmass already (`SameLandmass`). New visits:
  Tomb of Sargeras (any level-10 hero, turn 15), The Soul Gem (Jaina, Caer Darrow held), Old Hatreds (Daelin to
  Hellfire), Lone Wanderer (Rexxar) and Jailor of the Damned (Thrall) -- the last two only once the hero is in
  Northrend. Shared quests send one hero at a time; a hero on one mission is never picked for another (visits and
  Ragnaros could both take the same hero before).
- **Tomb of Sargeras on the graph:** edge Val'sharah <-> Tomb (n00J) behind the new gate "tomb", opened when WL's
  shared quest has opened the Tomb.

Fixes
- **Gate "greymane" was never opened** (since v25): no bot could attack Gilneas City from Silverpine. It opens now
  once a real player owns Gilneas City (pinned).
- Removed dead code: `SimpleBot.PlaceFirstOf`, `LastAttack`, three unused constants, `AiLegion.SpawnPortalPair`.
  `AiGates.OpenThandolIfDue` is also never called -- left in place and marked (open question, TODO.md).

Debug / tools
- `-heroes` (read): label + ping every living bot hero for 30 s (level, owner, `mission` / `still Ns` /
  `walking back`) and list them in chat.
- `-tp x y` (heavy = single-human games only): move your selected units there.
- Dump: new TEAMS section (CPs per team and member); pins when a team first reaches 80 and 90 CPs; bot lines show the
  anchor group size and `kept=`; hero list shows `mission` and the stranded state.
- New `tools/ai/map_query.py`: walkability, nearest walkable point, preplaced units, `Regions.X` rectangles -- straight
  from the map's source files.
- `tools/ai/check_refs.py`: nested enums are members (the unchanged 0.35.4 tree reported 2x `MISSING
  AiDebug.CommandAccess`).

## 0.35.4 (docs only -- the in-game banner stays 0.35.3)
- `AI_AGENTS_CONTEXT.md`: new hard-won facts (waygate landing spots, immobile heroes, hero-visit quest data, map
  name/version location, the 24p map is an old unsupported layout, economy depends on bonus gold), session workflow
  (sparse clone, dump decode one-liner, check before delivery, docs-only drops).
- `TODO.md` restructured for 0.36 (gameplay / polish / next playtest / open questions / later), with the 85-min
  test findings: winners can't finish off losers (split armies), stranded Illidan heroes, Legion never summoned.

## 0.35.3
- **Edge weights**: an edge can make targets behind it count as farther (`Edge.Weight`). Arathi Highlands <-> Dun Modr
  has weight 3 -- the dwarves marched north over the (often still closed) Thandol bridge while their allies died.
- **Legion's Felwood portal** moved onto open ground (-13580, 8448): the old spot was exactly on a preplaced unit,
  180 from the Felwood CP, and ground units could not use the portal (flyers could). Portal landing spots now
  also keep 250 away from buildings.
- **Free hero revives need a living altar**, and the hero appears at that altar (nearest to the start location).
- **Tol Barad** ship portal to Menethil Harbour + graph edge.
- **Hero visits / Ragnaros skip heroes that can't walk** (the Old Gods' level-20 god heroes were sent to Yogg-Saron
  every 13 minutes and never arrived).

## 0.35.2
- **Illidan and Fel Horde pinned to the Outland/Eastern Kingdoms graph.** Owning Broken Isles nodes (Kalimdor graph
  since v34) flipped Illidan to Kalimdor: "nothing reachable", army parked at the Outland edge.
- **Portal-aware travel distance**: a target on another landmass is measured through the best AI portal, not as
  the crow flies. Fixes the Legion sitting in Argus (it aimed at Blasted Lands, which only looked close).
- **Ragnaros**: only a bot whose best hero has really reached level 12 goes (any bot, no trigger CP); no level raising.
- **Hero visits** (`AiHeroVisits.cs`, config `AiConfig.HeroVisits`): Yogg-Saron (hero level 14 in the prison) and
  Fel Horde "Ash and Smoke" (a hero in Shadowmoon Valley once Terokkar Forest is held). The hero leaves the army
  until the quest is done; gives up after 8 min, retries 5 min later.
- Autopilot off removes the slot by player id (safety; see TODO: freezes with autopilot under investigation).

## 0.35.1
- **Bot strength vote**: 70 s into the game every human player chooses Recruit (no bonus gold) / Soldier (half) /
  Veteran (normal, default) / Commander (1.5x) / Warlord (2x, may lag). Majority wins, ties go toward Veteran.
  New file `AiGoldVote.cs`. `-gold*` debug commands still override it.
- Kul Tiras invasion wave removed (Kul Tiras uses its two ship portals).
- The wave runner class is `AiWaveRunner` (0.35.0 named it `AiWaves`, which already existed in `AiConfig.cs` --
  the map build failed). `check_refs.py` reports duplicate class names.

## 0.35.0
- **Code split into separate classes** (not `partial`): `AiSystem.cs` (SimpleBot core loop, QuestAutoComplete,
  Geography, AiState, AiSetup) plus `AiTech`, `AiMissions`, `AiLegion`, `AiWaveRunner`, `AiGates`, `AiAutopilot` (the wave runner is not
  called `AiWaves` because that name is the wave data class in `AiConfig.cs`).
  SimpleBot went from ~194 to ~88 estimated Lua locals. Behaviour unchanged.
- **One command switch** `AiDebug.Access` = `Off` / `HostOnly` / `Everyone` now covers *every* AI chat command,
  including `-botbreak`, `-prod`, `-atk` (moved from SimpleBot into AiDebug). Commands have levels: read-only
  (everyone), heavy displays (`-debug1..4`, `-edges`, `-ids`, `-autodump`: single-human games only) and
  game-changing (host only in `HostOnly`).
- **`-autopilot`**: a human hands their faction to the AI and takes it back with the same command. Fair in a
  release build (`Access = Off`): no bot bonuses, 60 s toggle cooldown. With commands on: full bot treatment.
  Everyone is told when someone switches. No extra vision.
- New `SimpleBot.HasPerks(p)`: bot bonuses (gold subsidy, free techs/upgrades/revives, starter kit, Ragnaros, CP
  grants, Legion summon gold/teleport/handover, waves) only for real bots and test-mode autopilot.
- Version history moved from the top of `AiSystem.cs` into this file. New `AI_AGENTS_CONTEXT.md` and `TODO.md`.
- `tools/ai/check_refs.py` understands nested enums.

## 0.34.4
- 0.34.3 did not load ("map unavailable or corrupted"): SimpleBot's Lua function had 201 locals. The 18
  collections moved into `AiState`; SimpleBot's 11 public collections are no longer `readonly` (CSharp.lua adds
  every non-private static readonly field as an uncounted local). Verified by compiling the real script with Lua 5.3.
- New `tools/ai/lua_check.py` (compile-check a built `war3map.lua`); `check_refs.py` estimates Lua locals per class.

## 0.34.3
- Root cause of "bots produce but never attack" (v34-v34.2) found: CSharp.lua leaves static fields with
  initializers past ~55 per class `nil`. Portals, area clears and every bot's attack logic crashed on them.
- `Loc.cs` fix (own commit): `%` in quest/power text no longer crashes rewards (Kul Tiras base, Stormwind).
- Legion exit back beside Felwood; Legion's Argus buildings placed on searched open ground.
- `check_refs.py`: limit check for initialized static fields per class.

## Earlier builds (v8 - v34.2, as kept in the code header until 0.35)

```
Warcraft Legacies AI -- build 0.34.4 (hotfix). main / 4.8.x.
 * 0.34.4: 0.34.3 failed to load ("map corrupted"): SimpleBot's Lua function had more than 200 locals. CSharp.lua
   caps the members it counts at ~195 but adds every non-private static READONLY field on top. Fix: the 18
   collections moved into the new class AiState, and SimpleBot's 11 public readonly collections are no longer
   'readonly' (mutable public statics are class-table members, not locals). Verified by compiling the 0.34.3
   war3map.lua with Lua 5.3; tools/ai/check_refs.py now estimates the locals per class.
Warcraft Legacies AI -- build 0.34.3 (hotfix).
 * 0.34.3: 18 static collections created in InitState -- CSharp.lua left static fields with initializers past
   ~55 per class nil, which broke portals, area clears and every bot's attack logic (v34-v34.2). Legion exit
   back beside Felwood; Legion's Argus buildings placed on searched open ground. Versions now 0.MINOR.PATCH.
Warcraft Legacies AI -- build v34.2 (robustness: isolated tick/commands/dump; area clears; links).
 * v34.2: every wave-tick subtask, summon step, chat command and dump section runs isolated (one error used
   to stop the gate sweep, -dump and -portals); gates swept first. Area clears (Oculus, Nexus). Highbank <->
   Southern Highlands, Nazmir <-> Kezan ship. Neutral bias 0.5. -help shortened (README.md has the list).
Warcraft Legacies AI -- build v34.1 (portal + summon fixes, Kul Tiras repair, Balor).
 * v34.1: portals owned by Neutral Extra (WL removes Neutral-Passive units from areas) + watchdog + per-pair
   try/catch; summon handling runs whatever completed the quest (polled), once, merge guaranteed; Kul Tiras'
   crashed Admiralty reward repaired (base rescued); Balor <-> W. Stranglethorn ship + Balor on the island
   graph; pinned log + PORTALS section in -dump; -portals pings every AI portal.
Warcraft Legacies AI -- build v34 (node system: islands, Kul Tiras, Argus hub, merge event; bug fixes).
 * v34: ship portals (real waygates, named, landing beside the other end) for the Kalimdor islands and Kul
   Tiras; Argus open from the start and linked to Northrend until the summon, then to Felwood and (WL's demon
   portal) Quel'Danas; the summon MERGES the graphs, home graph first. Bot Legion starts in Argus (workers ->
   buildings). The Sunwell capital now triggers the summon (once). Construction Sites upgraded (Stormwind
   City). Fallback events (Theramore, Scarlet Crusade) no longer auto-ordered. Fallback target respects the
   10-min lock. No naval techs. Illidan's Broken Isles grant removed.
Warcraft Legacies AI -- build v33 (tech pools, Ragnaros summon+kill, ship-portal test).
 * v33: two tech pools besides weapon/armor (unchanged): UNIT/ABILITY (casters + abilities, incl. stock
   abilities the auto-list missed) = 1 + 1 per 5 min + 1 per 2 CPs, stepped by level; BUILDING/MISC (free
   building upgrades, town halls first; then naval/flight) = 1 per 2 CPs. v32 caster schedule removed.
   Ragnaros: pedestal captured -> summoned -> killed for free by the bot's level-12 hero standing by.
   Legion exit back beside Felwood. -ship asset test for the Kul Tiras ship portals.
Warcraft Legacies AI -- build v32 (early quests by turn, starter buildings, tech split, lore rules).
 * v32: early quests force-completed for bots during turn N-1 of their expiry (WL turn counter), one per 3 s.
   Starter barracks + altar for every bot (workers become buildings). Upgrades split: techs (5-min timer +1 per
   2 CP captures), caster trainings (all, up to max level at 4/10/18 min), event researches (ordered when WC3
   allows, free; Fireland Invasion not before 15:00; plague left to WL). Stromgarde rule fixed. Cenarius stays
   dead for bots. Late-game quest rules (World-Shaman, Guardian, Line of Succession, Ascension). Ragnaros
   pedestal hero trip. Murmur kill target. Legion's Lordaeron capital + northern CPs handed to the Scourge.
Warcraft Legacies AI -- build v31 (target ranking, stuck detection, quest rules, Legion portal).
 * v31: frontier ranking -- neutral-only before 10 min, then nearest-to-army with a neutral bias; per-faction
   goal routing (Legion -> Nordrassil); stuck detection blacklists a target after 4 min without progress.
   Kill targets by unit type (Scourge -> Sapphiron 'ubdr'). Quest rules with conditions (capital lost, CP
   held by ally). WL's caster-less Demon Portal rebuilt off (0,0) + Antoran portal re-pointed; +3000 gold
   for a bot Legion on summon. Per-faction gold subsidy. CP grants (Illidan's Broken Isles).
Warcraft Legacies AI -- build v30 (neutral-CP fix, army anchor, bot-only waves, graph edits).
 * v30: IsEnemyCp ignores neutral players (their slot state reads PLAYING -> every creep CP was "enemy").
   Target selection anchors on the army centroid. Waves only run when a BOT is in the group (a human
   Kul Tiras got the fleet messages). Timed quest auto-complete for bots (AiConfig.TimedQuests).
Warcraft Legacies AI -- build v29 (defense rewrite).
 * v29: Rule 0 defense only fires for a DAMAGED owned anchor with >5 ENEMY-PLAYER ground units near it.
   Neutrals (creeps + ~1900 preplaced Neutral-Passive rescue units), allies and units on unwalkable terrain
   (ships) are ignored. Undamaged anchors are not scanned at all (removes most of the combat-tick load).
 * v28: graph walker skips invulnerable / Neutral-Passive CPs (quest-locked; can never be captured -> bots parked
   on them forever). Clash window + staging REMOVED: enemy frontiers are attacked directly (the 30s release
   window yo-yo'd armies back to the muster CP mid-march). Perma-death guard: heroes that died permanently
   (LegendaryHero.Died, Permanent=true) or carry WL's 'LEgo' perma-dies marker are never re-trained/revived.
   Diagnostics: per-bot BotDiag + per-wave WaveDiag + decision event log + per-subsystem op counters, read by
   AiDebug (-dump writes them to a file). Logic only WRITES diagnostics; it never branches on them.
 * v14: per-faction ordered attack march from AiConfig.Doctrine (coordinate waypoints via -xy, time-gated,
   ownership-aware, advances when a step is clear+held). Falls back to the generic picker when unfilled.
 * v13: bots get a flat gold subsidy each production service (they were gold-starved, not food-capped);
   mass to MinArmy=20 before attacking (bigger pushes); free hero revival for bots every 3 min.
 * v12: production now services ONE bot per tick (was all bots every 15s) -> the big load spike is gone.
   Attack-move is re-issued only when the target changes or every ~20s -> no more stop-and-go stutter.
 * v11: inspection moved to AiDebug.cs (own file, master const, per-player -debug1/2/3, default OFF).
   Deterministic op counters live in the logic here (near-free, always on). Control commands
   -botbreak/-prod/-atk stay here so they work even in a no-debug build.
 * v8-main: dropped the dev-only H00B vertical-gate constant (absent on main -> would not compile).
 * v9: area doctrine -- attack nearest reachable enemy/neutral CONTROL POINT first, then CAPITAL;
   Rule 0 defense (recall if >5 enemies near an owned anchor); home landmass taken from an owned
   capital/CP (fixes idle-at-portal, snaking, and the Druids l-1 hug). Replaces FindTarget's scan.
 * v10: bots train/revive heroes at owned Altars using faction-correct ids from AiConfig.cs
   (source-derived, no id spray). Gold-gated; WC3 hero-limit self-caps; faction absent -> skip.
   NEW FILE: AiConfig.cs must sit beside this file in the Ai/ folder -- copy BOTH into the build.
 * v8: one bot serviced per combat tick (round-robin); ~13x less enum/orders per second.
 * v8: chat toggles -botbreak (stop), -debug (readout mute), -prod, -atk.  Diagnostics kept.
 * QuestAutoComplete: at 2:00, completes every in-play faction's designated StartingQuest (humans + bots for
   now; flip computerOnly=true later to exclude humans -- that is the split point).
 * Capital-targeting for SCOURGE only: attack-moves toward the nearest reachable enemy/unowned CAPITAL
   (clears the yard; capital becomes takeable), instead of snaking through nearest hostile. Others unchanged.
 * everything from v6 (gate removal, gather-to-5, attack-move, hold-if-unknown-landmass, faction production, -xy, -fac).
```
