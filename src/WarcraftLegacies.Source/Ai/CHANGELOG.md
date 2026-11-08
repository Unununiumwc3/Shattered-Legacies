# Changelog -- bot AI

Versions are `0.MINOR.PATCH` (feature build = MINOR, fix-only build = PATCH). Git tags: `v0.39.9` and later (older
builds were tagged `4.8.2-ai.<build>` in the deleted fork). The `-version` command and every `-dump` header show the
same version. Up to 0.51.1 this file also held the changes of the fork that hosts the bot (Shattered Legacies); from
0.52.0 on those are in `../ShatteredLegacies/CHANGELOG.md`.

## 0.65.2 — construction in crowded bases (playtest candidate)

- Search smaller lots starting at 256 units from the anchor. Prefer buildable terrain and let the native worker order test custom placements. Reuse each anchor's candidate list across worker menus in a planning pass.
- Two different workers can recover different HQ buildings concurrently. Reserve their sites and keep expansion planning serialized. Visit one bot per half-second and retry idle orders and rejected types sooner.
- Include the faction in start, completion and retry messages for comparison with the 0.65.1 dump.

## 0.65.1 — construction coverage/speed diagnostic fix (needs playtest)

- Do not treat neutral passive map units/structures (P25–P27) as enemies when deciding whether construction
  ground is safe. Neutral hostile creep camps now exclude only a 650-unit vicinity, versus the former blanket
  1500-unit radius; player-owned enemies retain the broader safety distance.
- Native WC3 peon-flagged units can act as builders even without a faction Builder category.
- Visit a bot every ~16 instead of ~32 seconds in a 16-bot game; plan every 25 instead of 60 seconds.
  Pending orders are checked for abandoned idle workers and time out in 120 instead of 180 seconds; failed
  projects retry faster. Still only one project per bot.
- Throttled per-faction `builder: <name> idle:` lines explain no workers, missing producers, rejected orders,
  unsuitable anchors/sites, satisfied production and blocked forward expansion.
- Based on the 0.65.0 5-, 10- and 12-minute dumps. No compilation/in-game verification of this fix yet.

## 0.65.0 — worker construction candidate (requires map build and playtest)

- Added `AiBuilder` / `AiBuildSites` in `Ai/`: per-bot worker orders, native construct-start/finish/cancel
  lifecycle, retries, worker training, producer/altar/HQ recovery, and original-HQ / owned-or-allied CP / worker
  fallback. A real worker builds; routine magic starter structures are no longer generated.
- 10 owned CPs allow two total bases; 20+ allow three total (cap three including HQ). Allied CPs may anchor a
  safe forward base about two walking edges behind the front; each gets two troop producers total.
- `AiApi.GiveStarterProduction` signals the builder rather than spawning buildings. The scripted Legion Argus
  base remains temporarily, without deleting Herald workers. `AiScheduler` runs the worker construction task.
- Documentation roadmap updated. Debug access **unchanged** (1.0 release gate). Native worker build menus,
  delayed resource charging and map/pathing edge cases require testing: `BUILDING_DESIGN.md`.

## 0.64.0
No bot logic change. Fork data: the Ashtongue's purified sites train (`SL_BUILDINGS`); their heroes are also registered
under "Karabor" (the faction's name after its final goal).

## 0.63.2
No bot change.

## 0.63.1
No bot change.

## 0.63.0
No bot logic change. Fork data: the Ashtongue's temples in `SL_BUILDINGS`, their heroes (Akama, Xi'ri) registered.
The Ashtongue's mechanics (Purify, the conquest rule, the income) run by code -- bots need nothing.

## 0.62.0
No bot logic change. Fork data regenerated (`gen_ai_data.py`: the Oil Tanker renamed).

## 0.61.0
No bot logic change. Fork data regenerated (`gen_ai_data.py`): the Kezan copies of 0.61, the refugee units of the
slave camps (one per slot). The Enterprise's autocasts (Refine Oil, Detonate) are switched on by the fork's code, so
bots use them without knowing them.

## 0.60.0
No bot logic change. Fork data (`gen_ai_data.py`): workers are recognised by `peon` in their type too (the Forsaken
Acolyte was in the train lists); the Refugee Slave Camps in `SL_BUILDINGS` (a bot ruling a goblin town buys refugees).
TODO: bots that build with their workers, the food cap while the HQ is under siege (maintainer's proposals).

## 0.59.0
- **`AiApi.GiveStarterProduction(player)`**: a faction that arrives on a bot mid-game (a fork's fallback, a split)
  gets the starter production of the game start -- one of each production building of its faction it lacks, around
  its base (town hall, else any structure; not the slot's start location, which belonged to the faction it replaced),
  and an altar if it has none. `AiMissions.PlaceProduction(p, arriving)` is the game-start pass for one bot (0.39.2),
  split out. Bots never construct: a successor without a preplaced barracks trained nothing (Sunfury on P11, 0.57.3).
- Fork data: the Kezan Enterprise's Trading Center in `SL_BUILDINGS` (it trains the traders).

## 0.58.0
No bot logic change. Fork data only: the Kezan Enterprise's buildings in `SL_BUILDINGS` (`gen_ai_data.py`, now the
24p goblin copies) and its heroes (Gallywix, Gazlowe, Noggenfogger) through `AiApi.RegisterHeroes`.

## 0.57.4
No bot change.

## 0.57.3
- Quest timing: a starting quest without a deadline is completed only for factions of the game start; a deadline
  quest only if its deadline lies after the faction's arrival -- a later faction's starting quest is a real goal
  (WL's Scarlet Crusade got "A Once Great People" for free).

## 0.57.2
- **Quest timing for bots, one rule** (`QuestAutoComplete`): a bot faction's quest is completed for it in the last
  30 seconds of a turn, if the bot has not done it itself -- a quest with a deadline (WL's `ObjectiveExpire`, read
  from the objective, no title list) in the turn before the deadline; the faction's starting quest without a deadline
  in its first full turn. A faction that arrives mid-game counts from its arrival. Replaces the 120 s starting-quest
  timer and the title list `AiConfig.EarlyQuests` (kept as a fallback for deadlines that are not an
  `ObjectiveExpire`). Needs two read-only additions in WL: `GameTimeManager.TurnTimeRemaining`,
  `ObjectiveExpire.ExpirationTurn` -- part of the bot's WL pull request.

## 0.57.1
- **`AiApi.ReserveSlot(id)`**: a computer slot of the lobby stays without a bot until `AiApi.TakeOver` (a fork's
  free slot for a faction that arrives later).
- **Late factions**: when a faction registers mid-game on a computer slot, its starting quest is completed (as at
  the start) and the early-quest list is rebuilt to include its deadline quests.

## 0.57.0
No bot change.

## 0.56.1
No bot change.

## 0.56.0
No bot logic change (the Forsaken's three Sylvanas heroes registered through `AiApi.RegisterHeroes`).

## 0.55.1
No bot change.

## 0.55.0
No bot logic change. Generated data: the Forsaken buildings in `ShatteredLegacies/Bot/SlAiData.g.cs`
(`gen_ai_data.py` `SL_BUILDINGS`).

## 0.54.1
No bot change (the map's version moved for a fork fix).

## 0.54.0
- **`AiApi.TakeOver(player)`**: the bot takes over a player mid-game with the full bot treatment (the same as for a
  leaving human). For forks that hand a faction to a free slot (Shattered Legacies: the Forsaken betrayal).

## 0.53.0
No bot change (the map's version moved for the fork's turning-point core). `AiApi.ExcludeFromBotPicks` is no longer
used by this fork (Sunfury left the Illidari pick) but stays for other forks.

## After 0.52.0 (docs only)
- `AI_AGENTS_CONTEXT.md`: state of the work updated for 0.52.0 (no bot logic changed since 0.51.1).

## 0.52.0
Structure only: the bot holds no fork content any more. Version v0.52.0.
- **`AiApi.SetMapTitle`**: the title `-version` shows was a fixed fork text in `AiDebug`; default now "Warcraft
  Legacies", the fork sets its own.
- **Faction picks: one rule for every dialog** (`AiChoices`): a bot picks at random among the factions not excluded
  through the new **`AiApi.ExcludeFromBotPicks`**; a dialog with one allowed faction takes it without a roll. Before:
  random only when Warsong / Gilneas / The Exodar was among the choices, the Illidari / Sunfury dialog left to WL's
  24-s timeout (option 1). Same picks as before in this map (the fork excludes Sunfury); the one difference: a bot
  Illidari slot now gets the Illidari after 4 s like the other dialogs, not after 24 s.
- **Tools moved** to `tools/shattered-legacies/ai/` (development aids of this repository, not part of a WL pull
  request); paths in comments, generated headers and docs updated. `gen_ai_data.py` writes the fork's data to
  `ShatteredLegacies/Bot/SlAiData.g.cs` (same data).
- **Docs**: `README.md`, `AI_AGENTS_CONTEXT.md`, `TODO.md`, `CONCEPTS.md`, `WORKAROUNDS.md` describe the bot only;
  the repository-level way of working (clone, delivery, versions, releasing, map name) moved to
  `../ShatteredLegacies/AI_AGENTS_CONTEXT.md` and `../ShatteredLegacies/README.md`.

## 0.51.1
End of phase 1 (maintainer: ready for the restructure). Version v0.51.1.
- **The bot ritual cannot be stopped** (maintainer): the hero's invulnerability is set again every second (other code
  may reset it), and a hero pushed off its spot is put back instead of cancelling. Only a removed hero, losing the Book
  or changing owner ends it.
- **Docs for the handover**: `AI_AGENTS_CONTEXT.md` -- state rewritten (phase 1 done, phase 2 next) and a new section
  "Movement: hard-learned facts"; `README.md` "How the bots move" rewritten for 0.51 and "Already tried" extended;
  `TODO.md` -- open problems for phase 2/3, the superseded per-build playtest checklists and old result write-ups
  removed (history stays in `CHANGELOG.md` / `MEASUREMENTS.md`), still-open items kept ("Phase 3 candidates").

## 0.51.0
From the 0.50.1 game (maintainer gave a bot the Book; ferries look good). Version v0.51.0.
- **The bot performs the Legion ritual itself** (maintainer's choice): every cast order was refused in 0.50.0 / 0.50.1
  ("volcano" and "deathanddecay"; Scourge and Legion heroes, also Tichondrius with the maintainer's Book). Same
  requirements as WL's spell: a bot Legion / Scourge hero with the Book in its inventory, the Legion exists (R04T),
  not inside an instance, 410 mana (spent; raised for a bot hero that has less). The hero channels on its spot for
  180 s: hold position, channel animation, invulnerable, a rain of fire without damage (radius 750), a timer for
  everyone at the top ("Legion Summon (bot channelling)"), a minimap ping and a message. Moving off the spot, dying,
  losing the Book or changing owner cancels it (retry after 30 s). At the end the bot completes WL's "Under the
  Burning Sky" (WL's whole reward) and moves WL's Demon Portal from (0,0) -- a completion without a caster -- to the
  ritual spot, Antoran's portal pointed at it. Runs in its own 1-s task ("legion ritual"). Dump LOAD: "legion ritual
  (0.51, bot ritual)". A human hero uses WL's real spell as always.
- **Ferry boats at 150** (maintainer asked for half of 200; WC3 clamps move speeds to the map's minimum, 150 by
  default -- the map does not change it).

## 0.50.1
From the 0.50.0 game (maintainer: Dark Portal works, ferries work, "enter boat" good). Version v0.50.1.
- **Legion ritual order fixed**: the Book carrier (Arthas) stood without casting -- "volcano" (A00J's order string
  in the object data) was refused 4 times. A00J is based on Death and Decay, and WC3 uses a changed order string only
  for Channel-based abilities, so the bot now orders "deathanddecay" first (never for a hero with a real Death and
  Decay, i.e. Kel'Thuzad), then "volcano". After 3 refused orders the hero goes back to the army for 60 s instead of
  standing frozen.
- **Ferry boat**: owned by the bot (shows on the minimap; was Neutral Extra + Locust), sails with move orders from
  lane point to lane point (smooth; 0.50.0 jumped every 0.25 s and stuttered), half the speed (200), removed at the
  landing (timed life as a safety net), kept out of the bot's army logic. A stuck boat is put on its next point
  after 4 s.
- **Missions travel by route**: hero visits, stranded heroes and the Ragnaros hero walk along the route graph (chains)
  instead of one plain move order, so they take ferries and portals (`AiRoutes.MoveVia`). Maiev ("Vault of the
  Wardens") stood at the coast in 0.50.0 -- a plain move cannot cross the sea since the AI ship waygates are gone.
- Dump: "route checks refused (enemy pier)" (was "entries refused" -- it counts route checks, not units).
- Found (0.48.5 game, 38-min dump): the Dark Portal waygates belong to whoever holds WL's "Dark Portal Aura Control
  Nexus"; the Outland bots crossed at 31:35, right after Stormwind took it. Since 0.50 the owner does not matter.

## 0.50.0
Portal rework, part 2 (maintainer: playtest once all problem portals are done). Version v0.50.0.
- **Ferries replace the 8 AI ship waygates** (`AiFerries`, design `CONCEPTS.md` 7). No visible bot waygates any more.
  Each harbour pier is a hidden pseudo-portal gate with a one-way edge to the other pier (cost = walk + sailing +
  boarding time). A bot unit with a ticket that reaches the pier boards: hidden, paused, out of the army logic and
  hero care. The boat of that direction and player leaves 12 s after its first passenger: a transport-ship model in
  the bot's team colour (Neutral Extra, Locust, invulnerable) that sails a lane over water at 400/s; the passengers
  land beside the far pier and get the rest of their way at once. A pier is no entry while an enemy holds its CP.
  `-ferry on|off` (off = instant crossing). Dump LOAD: "ferries" line.
- **Sea lanes generated from the map** (`tools/ai/gen_lanes.py` -> `AiLanes.g.cs`): A* over 128-unit water cells,
  smoothed; all 8 lanes found (1,700-5,700 long; Tol Barad needed the narrow-channel fallback).
- **Dark Portal fixed** (0.48.5 game: Fel Horde 119 units and Illidan 63 held in Outland for 10 min, Stormwind /
  Ironforge waited at the rocks). Its six waygates (Neutral Hostile at 5:00) belonged to a player from ~10:00 on, and
  since 0.48 a player-owned waygate was an edge only for the owner and allies -- here neither side. Offline replay of
  the planner with the game's data: 36 EK targets for the Fel Horde and 16 Outland targets for Stormwind, all through
  the portal. Now only the Deeprun Tram, the Legion Teleporters and the exterior Demon Portal are owner-only; every
  other waygate is open to all bots. The dump lists every waygate with its owner (to find who takes them).
- **Book of Medivh without level 12 for bots** (maintainer): the Scourge's visit sends any hero once only the hero
  objective is open; a bot Legion / Scourge hero at the pedestal then completes the bot's own Book quest and gets the
  Book (`AiRitual`). WL's objective is unchanged -- humans still need level 12.
- **Book pedestal where WL put it**: Dalaran or Gilneas (whichever faction is in the game) -- the visits went to
  Dalaran only.
- `MEASUREMENTS.md`: the 0.48.5 multiplayer game.

## 0.49.0
Portal rework, part 1: the Legion (maintainer's spec). Version v0.49.0.
- **The Book of Medivh is the only way to summon the Legion**, as in WL: a hero carrying the Book casts WL's ritual
  ("Summon the Burning Legion", A00J, 180-s channel) and WL's quest "Under the Burning Sky" opens the Demon Portal
  where the hero stands. New `AiRitual`: a bot hero (real bot or test autopilot) of the Legion or the Scourge with
  the Book casts at once, kept out of army orders until the summon is done; mana raised to 410 if short (bot perk).
  The Scourge now also sends a level-12 hero to the Book's pedestal (`AiConfig.HeroVisits`); Kel'Thuzad's own copy
  of the ritual is not used.
- **Invulnerable only while channelling**: a bot Legion / Scourge hero channelling the ritual is invulnerable from the
  channel's start to its end (or interruption), then back to its previous state. WL's spell immunity stays as it is.
- **Removed**: the summon when the Scourge takes the Sunwell (Quel'thalas losing the Sunwell no longer summons the
  Legion); the forced quest completion when the Legion's Book quest was done; the `-legion` command; the bot Legion's
  army teleported to Felwood at the summon; WL's Demon Portal moved from (0,0) to Quel'Danas (FixDemonPortal and its
  15-s task); the unused Argus <-> Northrend / Felwood bot waygate links and their coordinates. After a summon the bot
  only checks WL's portal pair (logs it, activates it if needed).
- **Tomb of Sargeras <-> Argus, both ways** (bot pseudo-portal, hidden markers): open for good when the Legion is
  summoned or when any player holds the Tomb of Sargeras CP (n00J). The Argus end stands beside WL's interior Demon
  Portal (n03C, Antoran); landings 400 from each entry, all on open ground (`tools/ai/map_query.py`). With it open,
  every faction may target Argus (before: only after the summon).
- Dump LOAD: "legion ritual" line (state, hero, orders, channels).

## 0.48.5
From the 0.48.4 game (15 min, Ahn'Qiraj on autopilot; maintainer: "improved a lot", no back and forth seen). Version
v0.48.5.
- **Sleeping creeps are woken** (`AiWake`, `-wake on|off`, default on): at night creeps sleep and units never attack a
  sleeping unit on their own -- the Scourge's 85 units stood at a Mammut camp in Borean Tundra from 6:06 to 10:06
  (screenshot at En'Kilah). Once 3 units of the bot's army (the theatre being serviced) stand within 900 of its target
  (route target or kill target), every sleeping Neutral Hostile unit within 900 is woken (`UnitWakeUp`). No orders:
  movement is unchanged. Dump (LOAD): "wake ... sleeping creeps woken"; event log: "woke N sleeping creep(s)".
- **Unit budget 30 + 5 per CP** (was 30 + 7; maintainer: stutter, e.g. the dwarves at turn 15). 0.48.4 had 3852 units
  at 15 min (0.48.2: 3536) although the bots gave fewer orders (22,965 vs 25,178). Test other values in game with
  `-cap 30 <n>`.
- Guard positions: removed once per unit (0.48.4 called the native for every army unit on every service).
- `MEASUREMENTS.md`: the 0.48.4 game. Back and forth: engine walks 4.5 % -> 1.0 % of all walks (first 10 min), followed
  Druid units end 11,000-19,000 from their spawn (0.48.3: < 80); re-plan switches 14 of 252 route changes (commit
  blocked 4). Order-latency windows look worse, but the probe samples IDLE units -- without guard positions those are
  units standing in crowds at targets, so the windows are not comparable with 0.48.3 and older.

## 0.48.4
From the 0.48.3 measurement game (8.5 min, Druids followed). Version v0.48.4. Two behaviour changes, each with a switch.
- **Guard positions removed for all bots** (`-guard on`, was off since 0.46.3). The unit trail found the back and
  forth: a bot unit that runs out of orders (end of its chain, or dropped from the attack group at a target change)
  walks back to the spot where it was TRAINED 15-25 s later -- the engine's guard position. It walks with an attack
  order, so the bot sees it as busy and never re-orders it. Every followed Druid unit walked 28,000-52,000 and stood
  within 80 of its spawn point at 8:25; "engine" turnarounds were 38 % of all (1,747 of 4,600 in 8 min, all bots).
  0.46 judged the removal by order counts (3-10 x idle-away orders) -- those orders are the bot sending the units to
  the target / the staging point instead of the engine sending them home. Now also applied on every service to each
  army unit (revived heroes, units the enter trigger missed).
- **Commit on re-plan** (`-commit on|off|ab|ab2`, default on): the 30-s re-plan keeps the current, still valid target
  when the new best one is farther from the attack group on foot. Druids: 1500 before Northern Ashenvale the re-plan
  chose Felwood (cheaper from the territory), the group turned north, then walked back to Northern Ashenvale. Dump:
  "commit ... re-plan switches blocked" per bot.
- Event log: route target changes say why -- `[route-neutral:gone]` (the old target was taken / died / became
  unreachable), `:switch` (re-plan found a better one), `:first`. The angle (` | army @x,y ... at Ndeg`) is now
  measured from the attack group's centre (0.48.3: the whole army's, incl. units at home).
- `tools/ai/back_forth.py`: changes by why; turn-backs split by gone / switch.
- `MEASUREMENTS.md`: the 0.48.3 game.

## 0.48.3
Measurement build for the back-and-forth movement (`TODO.md` "Open problems" 1). **No behaviour change.** Version v0.48.3.
- **Turnarounds** (`AiTrack`, all bots): every army unit is sampled on its bot's service; two walks of >= 300 in a
  row that differ by more than 120 deg = a turnaround, attributed to the last bot order since the first walk began
  (change, rest, idle-away, refill, advance, exit, defend, whole, def-rest, portal, hero) or "engine" (no bot order:
  chases, creep leash, rally points, crowding). Dump section TURNAROUNDS: per bot and per 5-min phase; plus group
  swaps (units dropped from the attack group while still walking at a target change, and how many were later sent
  back to the staging point) and rest orders that point backwards.
- **Unit trail** (`-track <pid>|off`, default P11 Druids; `-track` alone lists and pings the followed units): up to 8
  units of one bot followed from the building that trained them (4 slots may adopt existing units at the start). Every
  bot order (kind, destination, distance of the destination from the unit's spawn point, BACK when it points against
  its current walk, the bot's reason and target), turnaround, teleport, drop from the group and death is a line; one
  position line per service. Dump section TRAIL, ~700 lines (with `-autodump` nothing is lost).
- Event log: every target change carries where the army stood and the angle between old and new target
  (` | army @x,y new 4100 old 3200 at 160deg`).
- `AiArmy.Issue` takes the order kind; hooks in `AiStats` (trained), `AiPortals` (rest of the chain after a
  teleport), `AiHeroCare` (stranded heroes).
- Tools: `tools/ai/back_forth.py` stitches one game's dumps (event log + trail): target turn-backs by cause and phase,
  defence ping-pong, followed units, `--unit U3` = one unit's whole trail, `--bot 11` = one bot's changes.
  `dump_summary.py` adds the turnaround totals.
- `MEASUREMENTS.md`: the 0.48.2 game (5 dumps), with the first back-and-forth numbers.

## 0.48.2
From the 0.48.1 game (25 min; "felt mostly good", tram worked). Version v0.48.2. Handover build for a new chat.
- **Legion Teleporters as pseudo-portals** (maintainer: no spell casts): once the Legion's "Argus" quest is complete,
  each WL teleporter building is a one-way pseudo-portal -- the one near 22939,-29345 to Northrend (3587,20680), the
  other to Alterac (11000,6424), for the Legion and its allies. The 0.48.1 spell casts are removed (the Legion never
  left Argus that way).
- **Main-front switches need 60 s**: "twice the food elsewhere" must hold 60 s before the main theatre moves (0.48.1:
  "main moved" 47 times in 25 min -- armies walking through portals shift the food for a moment).
- **Sticky wait point**: an army waiting at closed rocks keeps its wait point while it stays blocked (0.48.1: the North
  Alliance patrolled between two rock edges at Tirisfal).
- **Unit budget 30 + 7 per CP** (was 40 + 8; maintainer: a bit laggy).
- Dump: portal states and empty-plan reasons on their own lines -- the dump cuts long lines (both were lost in 0.48.1).
- Docs: `AI_AGENTS_CONTEXT.md` state rewritten for a new chat; `TODO.md` "Open problems" (back and forth first).

## 0.48.1
From the 0.48.0 game (25 min). Version v0.48.1.
- **Hold where the army stands**: when the planner finds nothing, the army holds at its own position. 0.48.0 held at
  the bot's BASE -- armies walked back to their production buildings every time a plan came back empty ("a->b->c->d,
  then back"; empty plans: 264 in 25 min).
- **The main theatre moves only after ~30 s of empty plans** (4 services in a row). 0.48.0 moved it on every single
  empty plan: "main moved" 36 times in 25 min, armies walking between fronts.
- **WL's Legion Teleporters are cast**: Argus is left through a SPELL of the teleporter buildings ("Portal to Alterac"
  / "Portal to Northrend", once each after the Legion's "Argus" quest), which opens a temporary Legion-owned
  slipstream portal. A bot Legion casts when it has 40 food of army on Argus -- Alterac first, Northrend next; the
  portal is read like every player-owned waygate. 0.48.0 had no caster: the Legion never left Argus. Dump: "Legion
  teleporters: ..." in the pseudo-portals line, PINNED on a cast.
- Dump: empty plans split by reason ("no candidate" / "none reachable from the group").

## 0.48.0
Portals aligned with WL (part 1 -- ferries and the Book of Medivh follow), "nothing reachable" fixed, theatre
tuning. From the 0.47.0 game (18 min: back and forth much reduced). Version v0.48.0.
- **Player-owned waygates are portal edges for their owner and allies**: the route graph reads the waygates of ALL
  players (was: neutral ones only). WL's Deeprun Tram (Ironforge / Stormwind after the tram research) and WL's
  Legion Teleporters (Argus -> Northrend landing / Alterac landing, after the Legion's "Argus" quest; one-way, like
  for humans) are used now -- found by the building, so a moved building keeps working, and the edge closes when it
  dies or is deactivated.
- **No visible bot waygates for Argus and the rift**: the bot's Argus <-> Northrend / Felwood waygates are gone.
  Tomb of Sargeras -> Argus is a ONE-WAY bot-only pseudo-portal, closed until the Tomb is open (gate "tomb"). The Old
  Gods rift is a bot-only pseudo-portal both ways (gate "oldgods"); WL's preplaced portal in the Twilight Highlands is
  no longer removed. Hidden inactive markers serve as their gates (no human can use them, nothing shows). The ship
  waygates stay until the ferries.
- **Nothing reachable -> wait in front of the rocks**: when the planner finds no target, the army waits at the own
  end of the nearest closed rock edge that separates its land from another (Northrend rocks, whirlpools, Thandol) --
  maintainer's rule; else it holds. The old pickers (old graph, nearest objective) no longer run while the route
  planner is on: they ignored rocks and sent the North Alliance to the Brill coast towards Northrend (0.47.0).
- **Theatres**: a second front opens at max(40 food, 25 % of the army) and stays down to max(20, 10 %) (0.47.0: 30
  / 15 split strong armies early -- Scourge, Legion and the Old Gods were weaker). The dump lists one line per bot.

## 0.47.0
**Theatres and "only forward"** (maintainer: "not going forward" is the main problem). Includes 0.46.4 (not played).
Version v0.47.0.
- **Theatres** (`AiTheatres.cs`, design `CONCEPTS.md` 6): a theatre is a walking component of the route graph (live
  rock state, no portals) -- Kalimdor, the Eastern Kingdoms (+ Northrend after its rocks), Outland, island groups.
  Up to 3 per bot, each with its own target, route, staging point, attack group and defence -- the same planner and
  army code with its own STATE SLOT (slot = player + 24 x theatre; `AiTheatres.S(pid)`). A land becomes a theatre at
  30 food of army and stays one down to 15; the main theatre follows the biggest army (switch at 2x); units on land
  without a theatre belong to the main one. The main theatre does not target lands with their own theatre; a
  secondary theatre targets only its own land and uses no portals. When the main theatre's land has no target left,
  the main theatre moves to the bot's next front (the old land is "done" for 3 min) and its units follow.
- **Only forward -- targets**: a target's score adds half the walk from where the army stands to the territory
  distance. Before, any point of a long border could win, so the army turned back whenever its target fell (North
  Alliance between its own CPs, Kul Tiras east-west, Druids back from the Broken Isles).
- **Only forward -- defence**: a CP more than 6000 behind the attack group is defended by the units not in the group
  (staging rest, new units); the group keeps its front. Defences are per theatre: a theatre defends only its land.
- Regroup counts only for the main theatre. Order throttle per theatre.
- `AiStyleCheck.cs` removed -- the in-game check passed ("OK, 300 instance fields"): state in objects works with
  CSharp.lua (phase 2 can use it).
- Dump: "theatres (0.47; opened, closed, main moved): P.. main <land> <food> | T2 <land> <food> -> <target>",
  "secondary-theatre plans", "defended by the rest (group kept its front)".

## 0.46.4
From the 0.46.3 game (15 min, maintainer). Version v0.46.4. Note: the 0.46.3 on GitHub is the FIRST 0.46.3 delivery
(without the regroup fix below) -- both deliveries had the same version number.
- **Regroup in units by game phase** (maintainer): resume at 10 units before 15 min, 15 until 45 min, 40 later;
  regroup below half of that, and only after the army had reached the resume size once. 0.46.3 (GitHub) still held 11
  bots at 0:00 -- they attacked between 3:21 and 7:03.
- **Defence does not pull units across the sea**: a defence order skips units on another landmass than the defended
  CP -- they keep fighting where they are. 0.46.3: the Druids' whole army came back from the Broken Isles twice for
  a few enemy units in Ashenvale. Dump: "defence kept away (other landmass)". A first step towards theatres (0.47).
- **Only forward -- creep camps and leftovers measured from the army**: their score is at least the straight line
  from where the army stands (was: the distance from the territory only, so a camp at home looked cheap while the
  army fought far ahead).

## 0.46.3
From the 0.46.2 game (81 min, A/B per game phase). Version v0.46.3.
- **A/B results applied** (`dump_summary.py` "A/B per game phase", 16 phases of 5 min):
  - guard positions: removal OFF -- the half without guard positions issued 3-10 x the idle-away orders and more rest
    orders in every phase, in both splits (0.46.1 ab, 0.46.2 ab2): units stay where a chase ended and the bot sends
    them back; the engine did that for free.
  - spread points: OFF -- more stalled units from 50 min on (17-24 % vs 3-13 %).
  - gather points: OFF -- no difference in any phase.
  - regroup: stays ab (entered rarely; no verdict yet).
- **Route graph corrections** (maintainer's map review, `route_exceptions.txt` + generator):
  - **Removable rocks are read from WL's code**: every destructable type WL puts into a `RockGroup` (LTrc; B013
    "Whirlpool Blockers", removed 2 turns after the Scourge's Plague) closes its edges until WL removes it; the bot's
    runtime check counts each group's own type. The whirlpools were trees before (OldId LTrc, 'T' as 2nd letter).
    Other LTrc-based blockades nobody removes (B008 Ashenvale tree blockade, B01L outside-Kalimdor/Northrend blockade)
    are walls; B000 has no pathing. 12 rock groups (was 9). B01L closes Loch Modan - Searing Gorge and Alterac -
    Andorhal (check in game); no CP lost its connection.
  - **Tirisfal - Howling Fjords channel**: a fixed node in the shallow water; Tirisfal <-> channel and channel <->
    Howling Fjords are rock edges (whirlpools) -- Lordaeron units gathered on the Brill coast while the channel was
    blocked (0.46.2).
  - Teldrassil only to the area node west of it (-> Auberdine); Ruins of Ahn'Qiraj no longer to the Tunnels' node
    (exit 216 -> 222 only); Barrens: a fixed hub at the corridor's south entrance (-13220,-8800), Southern Barrens at
    the other end; no hub link through the cliffs east (the "Dire Maul -> Ratchet" route).
  - `route_exceptions.txt` gains `node @x,y` (fixed area node) and `add A | B`.
- **Regroup fixed**: only an army that had once reached the "attack again" size can regroup. 0.46.2 regrouped every
  bot of its half at 0:03 (army 0 < 20 food): they stood until 40-50 food (Fel Horde attacked at 7:18, Gilneas not
  by 7:36; the maintainer's autopilot units were held back) -- the 0.46.2 regroup A/B was void.
- **Finish off beaten factions**: targets of a faction with at most 3 CPs count half their distance (dynamic, no
  lists). 0.46.2: Druids, Frostwolf, Exodar, Stormwind kept a few CPs for 50+ min. Dump: "finish-off scores".
- **The Scarlet Crusade**: condition is now "Arthas has joined the Scourge" (the Scourge's Arthas legend exists) --
  maintainer; Stratholme's fall comes before that anyway.

## 0.46.2
From the 0.46.1 game (44 min, maintainer as Lordaeron on autopilot). Version v0.46.2.
- **WL's control points restored** (maintainer): the 19 CPs the fork added are gone from the map data -- 13 Shattered
  Legacies CPs (`n1A0`-`n1B2`, 10 gold) and the bot's 6 workaround CPs W1-W4 (`n0SA`, `n0TD`, `n0RF`, `n0JS`,
  `n0SP`, `n0NX`, 1 gold) -- and the four 100-gold CPs (Ny'alotha, Vortex Pinnacle, Temple of Ahn'Qiraj, Icecrown
  Citadel) have WL's income markers again (`A0NS` / `A0NT`; `A1HG` / `B1HG` / `A0ZG` / `B0ZG` deleted). Preplaced
  units and `ControlPointSetup.cs` are byte-identical to upstream WL 4.8.2 again. The route graph regenerated: 168 CPs
  + 72 area nodes (the generator puts area nodes where the CPs stood -- bot data in `Ai/`, no effect on gameplay);
  no connection between the remaining CPs lost. Gul'dan's Legacy visit no longer needs Broken Shore (`n1A6`).
- **Route graph: pathing blockers and custom doodads** (`gen_routes.py`): WC3's invisible pathing blockers (YTpb /
  YTlb 64 x 64, YTpc / Ytlc 128 x 128 -- sizes from their 64 / 128 spacing on the map) were sized as trees (the
  large ones at half size: gaps in every wall) or ignored (Ytlc, 1568); air-only blockers wrongly blocked ground.
  Custom doodads with their own pathing texture (`dptx`, e.g. 128 x 128 snowy trees) were ignored entirely.
- **Route exceptions file** (`tools/ai/route_exceptions.txt`, `remove A | B`, CP names or @x,y): what the map files
  cannot tell. First entry (maintainer): Howling Fjords | Grizzly Hills -- Lordaeron is cut off from Northrend until
  the Northrend rocks fall.
- **Staging on the route**: the rest of the army waits at the own node closest to the target along the planned route,
  kept while the new one is within 2500. Was: the own CP nearest in a straight line -- often a dead end across a
  gorge (Terrordale), jumping with every target change: units walked back and forth between the bot's own CPs.
- `-xy` shows the route node under the selected unit (number = `route_map.png` / `ROUTES.md`, same build).
- **Regroup** (`AiRegroup`, `-regroup on|off|ab|ab2`, default ab = slot bit 3): an army below max(20, budget / 5)
  food stops attacking, gathers at the staging point (production goes on), attacks again as one wave at
  max(50, budget / 2).
- **A/B per game phase** (maintainer: a feature may help early, mid or late only): gather (ab), spread (ab), guard
  (ab2, swapped) and regroup (ab) all run in one long game; `dump_summary.py` prints a table per phase (difference
  between two dumps, per bot of each half). The spread line gives raw stalled / sampled counts for that.
- **Unit budget 40 + 8 per CP** (was + 10): order delay ~2 s at 10-15 min.
- **The Scarlet Crusade** is ordered by bots again -- only after Stratholme has fallen (`EventResearch.When`).

## 0.46.1
Fix from the first 0.46.0 game (maintainer: dwarves ran into a teleport at the Thandol Span and came out in a weird
spot).
- **Naval waygates are no army edges.** WL's "Water Passageway" pairs (n03Q / n03S, Neutral Passive) stand on deep
  water and exit onto water -- they let SHIPS pass the Thandol Span (15040,0 <-> 16384,128) and sail between Highbank
  and western Kalimdor (22720,-9664 <-> -20800,4864). The route graph took every active neutral waygate as an edge
  since 0.42, so the planner believed in a Dun Modr -> Dragonmaw Gates passage and a Highbank <-> Feralas coast
  shortcut. Units could not reach them (0.42-0.45: stuck at the shore); the 0.46 pseudo-portals teleported them, and
  the engine set them down at the nearest dry spot. Now a waygate whose position or exit is not walkable is skipped
  (all other portals checked: AI pairs, rift, Argus, Emerald Portals, Dark Portal stand and land on walkable ground).
  Dump: "naval waygates ignored".
- **Caster cap 35 %** (was 25 %): Quel'thalas is a caster faction.
- Version v0.46.1.

## 0.46.0
Fixes from the 0.45.0 game, pseudo-portals, engine-load measuring and the first per-bot A/B tests. Built in three
parts (smoke test of part 2: 397 teleports in 5 min, no errors). Version v0.46.0.

Part 1:
- **Creep camps finally run**: `AiRoutes.CheckCreeps` failed on every run since 0.44.3 (TICK ERROR "arithmetic on a
  boolean value", war3map.lua:29279): CSharp.lua translated `_creepLevel[node] += lv > 0 ? lv : 1` without the
  parentheses. With `-creeps on` (default) the one-shots were skipped too, so bots cleared no creep camps at all.
- `check_refs.py` flags compound assignments to array elements with a ternary / comparison / logic on the right
  (hard rule 16).
- Graph list review: the maintainer's add/remove list matches the OLD graph; the generated graph already had 12 of 13
  items as wanted (`TODO.md`). Docs: "Portals and passages -- how they open" (`README.md`), Dark Portal and "two
  graphs" facts (`AI_AGENTS_CONTEXT.md`).

Part 2:
- **Pseudo-portals** (new `AiPortals.cs`, `-pportal on|off`, default on): bots no longer click waygates. A chain
  that crosses a portal ends in front of the waygate and the unit gets a ticket; an enter-region trigger per waygate
  area teleports it (only a living unit whose ticket names THIS waygate) and gives it the rest of the way from the
  exit at once. A unit already inside the area is teleported on issue. Effects at both ends (at most one per waygate
  per second). Real waygates stay for humans. Dump: teleports, ignored, open tickets, "Dark Portal: ...".
- **Dark Portal**: if WL opened it (its n036 waygates visible) but no live edge exists, the bot adds one pair in both
  directions (0.45.0: no edge out of Outland at 540 min). The dump lists WL's own waygates (owner, active, hidden,
  destination, edge yes/no) -- the 0.45.0 cause is still open.
- **The planner keeps a valid target** when a periodic re-plan finds nothing reachable (was: old picker took over;
  0.45.0 Fel Horde: 608 target switches). Dump: "planner nulls".
- **Attack groups only from units with a way to the target** (walking or portal edge), unless none has one.
- **Unit budget per CP** (`AiBudget`, `-cap on|off|<base> <perCp>`): bots train units only while their army food
  (heroes not counted) is below 40 + 10 per held CP, at most the faction's food maximum. `AiApi.SetBudgetBase`
  sets a base per faction. Dump: army/budget and capped turns per bot.
- **Order-latency probe** (`AiProbe`): game time from an order to an idle unit until it moves; buckets per 5 min.
- **Guard positions** (`AiGuard`, `-guard on|off|ab|ab2`, default ab): removed for half of the bots (even slots);
  dump compares idle-away and rest orders per bot of both halves. Per-bot A/B helper `AiAb`.

Part 3:
- **Pseudo-portals closer to the asset** (maintainer's smoke test): the chain's last point is 150 in front of the
  waygate (was 400), the trigger area 220 (was 450); a unit within 500 when it gets its ticket is teleported at once
  (stragglers in a crowd).
- **Dark Portal finder fixed**: WL keeps those waygates hidden until it opens them, and range searches skip hidden
  units ("waygates not found" in the smoke test) -- now found by owner.
- **Per-bot A/B with independent halves**: every switch splits by its own bit of the player slot (guard bit 0,
  gather bit 1, spread bit 2), so the tests do not confound each other. The dump names the slots of each half.
- **Gather points A/B** (`-gather on|off|ab|ab2`, default ab): dump compares CP captures per bot of both halves and
  shows the share of the group that had arrived at timeouts.
- **Spread chain points** (`-spread on|off|ab|ab2`, default ab, a test): intermediate node points get a per-unit
  offset (8 directions from the unit's handle id, 250 or 125 if walkable). Dump compares the stalled share per bot.
- **Caster cap**: while casters (types whose units have mana; heroes excluded) are 25 % or more of a bot's units, no
  caster is queued (0.45.0: Quel'thalas 94 % casters). Dump: caster orders skipped per bot.
- **Observation counters**: activity of sampled army units (walking / fighting / idle / stalled), target RETURNS
  within 5 min (A -> B -> A) per bot, targets by owner per bot (who attacks whom).
- **`-edges` draws the generated route graph** (white walking, grey closed by rocks, purple portal edges); `-edges
  old` the old hand-written one.
- Defence log shows the real enemy count while a defence is kept (was "0 enemy units").
- **CSharp.lua style check** (`AiStyleCheck.cs`, removed in 0.47): a class with 300 instance fields and a static
  instance; dump line "style check (0.46)". The built 0.45.0 Lua already shows instance fields as table entries
  (`BotDiag`), so the phase-2 style should work.
- `dump_summary.py`: errors at the TOP of the summary; `--real "t5=5:02 t10=10:30"` (stopwatch -> real s per game
  min); the 5-min latency windows only once.

## 0.45.1 (docs only -- the in-game banner stays 0.45.0)
Plan and docs after the review of the movement approach (no map change).
- **Phases** (`TODO.md`): 1 movement "good enough" (0.46-0.49, ends at exit criteria: load, graph, activity,
  flapping returns, no dead map, maintainer's call) -> 2 restructure (~0.50) -> 3 fine-tuning. Upstream PR = `Ai/`
  copied into a fresh upstream branch. Playtest protocol (stopwatch at turns, per-bot A/B swap).
- **0.46 planned**: unit budget per CP (40 + 10 per CP, ~150 food on average), order-latency probe, guard
  positions (hypothesis), the maintainer's graph edits, instant pseudo-portals, per-bot A/B switches, activity /
  flapping-return counters, spread chain points (test), CSharp.lua instance-field check. **0.47**: theatres, ferries.
- `CONCEPTS.md`: new designs 6 theatres, 7 pseudo-portals + ferries, 8 unit budget per CP, 9 measuring engine load;
  step table updated; old "step 3" (groups by army size) replaced by theatres.
- `README.md`: "How the bots move" (the pipeline today, planned parts marked), "Already tried" (every movement /
  targeting / load approach since v6, what happened, what replaced it), list of the documents.
- **TODO split**: `TODO.md` = bot work only (what would go into the upstream PR); everything fork-only (fallbacks,
  Kezan, map content, the fork's win rule and game length, faction balance, brainstorming) in
  `../ShatteredLegacies/TODO.md`. New there: the Thandol Treaty (start vote, Great Alliance, Imperial Tax, Defias),
  the fork's win rule and game length (100-140 min), the per-faction base of the bots' unit budget.
- **0.45.0 played (540 min)**, results in `TODO.md` and `MEASUREMENTS.md`: no rift crossing in 510 min (dead map);
  creep camps never ran (TICK ERROR in route creeps every run); Fel Horde flapped between route planner and old
  picker (no graph edge out of Outland); gather points 12 of 149 assembled; lag at the end with fewer units than at
  10 min. 0.46 gets the fixes, pseudo-portals first.
- `AI_AGENTS_CONTEXT.md`: state of the work and plan, new section "Engine (WC3) limits", workflow rule (read / extend
  "Already tried").

## 0.45.0
Movement step 2 and handover docs (a new chat starts from here). NOT playtested.
- **Gather points (waves)** (`CONCEPTS.md` 0, step 2): a route longer than 5 nodes first goes to the last own node
  before the frontier; the attack group waits there until 80 % of its food is within 1500 or 60 s passed, then gets
  the chain to the target. Short routes go without waiting. Decided once per target. ROUTES line: "gather on
  started / assembled / timeout". `-gather off` = 0.44.
- **Old Gods rift east end moved 100 north** (22545,-4684), beside WL's preplaced `n07E` (-4784, still removed at
  opening). The maintainer found the east end not working in game; the west end (Maw) works. Map data around both
  spots is open ground -- in-game confirmation pending.
- Docs: `AI_AGENTS_CONTEXT.md` (state of the work, playtest status, targeting / movement / economy facts that were
  outdated: route planner is the default, bots had no CP income before 0.44.2, the rift and continents, PathingMap
  flags, push only named tags), `TODO.md` rewritten (playtest table per version, next test, movement / graph /
  mission / bug / balance lists with everything discussed), `CONCEPTS.md` step status, `WORKAROUNDS.md` (W1-W4
  status, one-shots replaced, rift move), `README.md` commands, Shattered Legacies TODO (LFS check for the invisible
  Kezan units).
- Version v0.45.0.

## 0.44.3
Movement focus (maintainer: balance later).
- **Creep camps are targets** (agreed spec): Neutral Hostile units per node AREA (region map, the same scan as the
  buildings, every 15 s). A camp is a target for a bot that holds the CP the area belongs to (an area node's patch:
  the nearest CP on foot, computed once), when its army has at least 1.5 food per creep level (bosses often use 0
  food -- level counts), at any time; invulnerable / hidden creeps are skipped; reachable on foot like every target.
  Score = walking distance + 3000: a neutral CP 4000 away (2000) comes first, a camp at home (3000) before an enemy
  CP 4000 away. The attack group's size counts creeps by level too. The hand-placed one-shots (`AiConfig.OneShots`)
  are skipped while this is on; `-creeps off` = 0.44.2 (one-shots back). Area clears and kill targets unchanged.
- **Rest churn**: on a target change only IDLE rest units get the new staging CP; the busy ones keep walking. Every
  target change used to re-send all rest units: 12,913 "rest" orders in 14 min (0.44.2 game).
- **Waygate exits**: units standing idle at an exit (their queued chain was lost in the teleport) continue on the
  next service (~7 s), not after the 20-s heartbeat ("exit" in the army orders line).
- Version v0.44.3.

## 0.44.2
- **CP income for bots** (`AiIncome.cs`) -- the root cause of the empty games, tiny armies and the 9.5-hour
  stalemate. WL pays control-point income only to HUMAN slots that are playing (`ControlPointManager`:
  `EnumeratePlayers(Playing, User)`); computer slots never got any. The strength-vote bonus was the bots' whole
  income -- since 0.43.1 Veteran pays 0, so they had none: in the 0.44.1 long game 15 bots trained almost nothing
  after 40:00 (Warsong 92 units by 40:00, still 92 at 570:00), lost their bases and altars (dead heroes never
  revived) and the map froze; only the maintainer's autopilot slot -- a human slot, paid by WL -- kept producing
  (718,000 gold from 108 CPs). Now the bot pays every slot WL does not pay (computer slots, taken-over leavers)
  exactly what WL would: `PlayerData.TotalIncome` per second from turn 1, with WL's fractional carry. Bot-side, no
  WL change. Dump LOAD: "bot income paid since start" per bot.
- **Chains: 4 route trees kept at once** (one destination each). With one tree (0.44.1) the loop alternated between
  the attack group's target and the rest's staging CP and recomputed every time: up to 78,318 ops in one slot.
- **`-heroes`**: pings only heroes you can see (WL parks some heroes hidden off the playable map -- they were pinged
  as if alive), one line per bot instead of one per hero (did not fit on screen), dead heroes counted.
- Version v0.44.2.

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
