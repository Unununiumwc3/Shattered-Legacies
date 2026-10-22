# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Current build: **0.39.0** (map "Warcraft Legacies
Botified 4.8.2-ai.0.39.0"). Next: **0.39.0** = refactor phase 1 + 2 (below).

## Freeze check (still open -- now also WITHOUT alt-tabbing)

- 0.36.0 minute 1 and 0.37.1 turn 17 froze while alt-tabbing (sound kept playing). **0.38.1 froze at ~34 min without
  alt-tabbing.** The dumps up to 30:00 show nothing unusual: no errors, units fewer than at 10:00 (armies 1753 -> 914),
  load flat, last event 18:59. So it is not simply unit growth.
- 0.39 adds the **heartbeat file** `CustomMapData\WLAI_heartbeat.txt` (with `-autodump` on): after a freeze, send it
  together with the dumps -- its time says when the game stopped, its task list what ran last.
- Also helpful: the folder listing with times (`ls -l --time-style=full-iso` in `CustomMapData`) -> real seconds per
  5 game-minutes (does the engine fall behind before the freeze?).
- Still open: does a freeze also happen without computer players? On Windows?

## Refactor (agreed concept, 0.39+)

Vision: bots replace human players and challenge them -- mostly through **many troops on the map** (that is the
fun; a food cap is the last resort). So the engine budget must go to units, not to AI overhead. Each phase is
playable on its own; changes stay inside `Ai/` (outside: only the hooks listed in `AI_AGENTS_CONTEXT.md`).
0. **Step 0 (measure) and lever 1 (orders only for idle units) are in 0.39.0** -- compare `-orders idle` vs
   `-orders all` with the UNITS section / heartbeat file.
1. [ai] **Scheduler + load budget** (0.39): one timer, one small task per tick, op counters visible in `-debug2` /
   the dump. No behaviour change. Spread the 15-s wave-tick work (hero care, standings, gates, missions) over ticks.
2. [ai] **Army orders** (0.39): orders only for idle, new and straggling units; re-order the whole army only when
   the target changes. **Attack group** of limited size (start 40-60 food, tunable per faction) goes to the target,
   the rest holds at the nearest owned node (garrison = defense). Waypoint orders: next node on the route instead
   of one cross-map order.
3. [ai] **Route planner on one CP graph** (0.40): one edge list (generated walking cost + optional gate + hand
   weight); from the army's own node, Dijkstra over OPEN edges only -> the cheapest unowned node is the target;
   retarget only on events (taken / lost / stuck / attacked). Replaces: Combined/Kalimdor sets and the merge,
   portal-aware distance, stickiness, most stuck special cases, goals, skip nodes. Fixes "frontier behind a
   barrier" (Ironforge -> Arathi through an ally node) by construction. Defense = nodes within a few hops of the
   army. Data: `tools/ai/graph_weights.py` (pathing data + rocks/bridges + portals; feasibility: 176 of 187 edges,
   median 1.08x the straight line). Names: **CP graph**, **route planner**.
4. [you/ai] **Economy** (after 2-3 are measured): bonus gold per minute via WL's `BonusIncome` (independent of the
   bot count; today's per-turn subsidy pays more the fewer bots there are), start 11:00, Recruit 0,
   `D x (20 % of WL StartingGold.Income + 10 % of CP income, counted up to 300)`, D 0 / 0.5 / 1 / 1.5 / 2.
   Alternatives to compare: flat amount per CP; square root of CPs; quality instead of quantity (earlier tech, hero
   XP, elite share) for the same food. Food caps only as the last resort (maintainer: many troops = fun).
5. [ai] **Cleanup**: wave runner, doctrine steps, `OpenThandolIfDue`, goals; version tags in comments ->
   CHANGELOG; split `AiSystem.cs` (Army, Economy, Graph, World, Missions).

Measure before and after each phase: units per player and total, orders per minute (LOAD), at 10/20/30/45 min.

## 0.39 -- after the production rework (0.38)

- [ai] **Units of buildings a bot doesn't have**: bots never construct buildings, so units from buildings they lack
  stay "never" (0.38 test: Quel'thalas' Academy and Consortium units, Kul Tiras' and Stormwind's special units).
  Proposal: the starter kit (or the building credits) also gives each bot its "Specialist" building(s).
- [ai] **Quest-locked heroes and units**: `unit_scan.py` lists per faction which quest unlocks what; decide which
  quests bots should drive (0.38 test: Rommath/Lor'themar came once "The Blood Elves" was done).
- [ai] Hero care teleports often (0.37.2 test: ~40 in 65 min; Warsong's Chieftain 6x from the same spot
  -9952,1944). Find out what is at that spot before tuning.
- [ai] Ships and builders are left out of production on purpose; naval play is not planned.

## Decisions needed

- [you] **Arthas when Lordaeron turns Scarlet Crusade early** (0.38 test: the human Lordaeron used the Scarlet
  Crusade event, Arthas died for good at 19:09 and the Scourge never got him -- "The Culling" needs Lordaeron's
  "Line of Succession" still open). Proposal: when Lordaeron becomes the Scarlet Crusade before "The Culling", give
  the Scourge its Arthas unlock (the research "The Culling" grants) -- AI-side for a bot Scourge, or a WL change for
  everyone?
- [you] **Archimonde's channel** (420 s, "Twilight of the Gods"): options (a) the Legion army gathers at the spot
  while he channels, (b) reinforcements go there, (c) a shorter channel (WL quest change, outside the AI).
- [you] **Defend range / flapping**: the Legion "defended" a shrine 26 km away (0.36) and switched defend/attack every
  15 s (0.37.2). Proposal: defend only within ~8000 of the army, keep a defend target at least 60 s.
- [you] Kul Tiras Highbank last stand (`QuestRule` with `CapitalLost` = Boralus Keep)?
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`.
- [you] **Free heroes for a human?** In the 0.38 test the human Lordaeron got heroes for free. The AI only trains
  heroes for bots (perks), WL heroes cost 100 gold in the map data -- so it is not the AI. Check: does gold go down
  when you queue a hero? If not, find WL's mechanic (Scarlet Crusade event? test-mode cheat?).

## Fallback scenarios -> `FALLBACKS.md`

Big project of its own, after the engine-load rework: every faction gets one fallback (good factions -> roadblocks
for the other evils, evil factions -> join whoever beat them). Design, team changes, announcements and the per-faction
work list are in `FALLBACKS.md`. Not related to it: tie the food cap to the CP count (idea); islands still free for
neutral roadblocks (Zandalar trolls -- 24p: God King, Gurubashi, Amani, Sandfury).

## Smaller implementation work

- [ai] **Warsong as a bot** (it is picked now -- seen in the 0.37.2 test): goals for its subdue quests -- Echo Isles
  (trolls), Thunder Bluff (tauren), Stonemaul (ogres); each also needs Grom alive.
- [ai] Autocast for bot units/heroes (heal, inner fire, ...) -- not done; skills are learned since 0.38. Some
  abilities (e.g. Blink) are already used by WC3's own unit AI for computer players.
- [ai] Gate sweep stays (maintainer: bots must not deal with gates). 0.38 removes new gates when they enter the map;
  the maintainer checks in playtests which gates still survive (`-id` on it, then add the type to `AiGraph.GateTypes`).
- [ai] Losing conditions for Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [ai] `tools/ai/map_query.py` -> `map_audit.py`: check every coordinate in AiConfig.cs in one run (incl. doodads
  near portal ends -- the Maw end stood on an archway).
- [ai] Debug: `-scout` (define first), winner record when WL declares victory.

## Check in the next playtest (0.39.0)

- `-autodump` on: the heartbeat file appears and is rewritten every 10 s; dumps have the UNITS section.
- Armies still fight and reinforce with `-orders idle` (default); orders/min in the heartbeat lower than with
  `-orders all`. If units stand around at the rally points: report it (lever 1 too strict).
- Scourge does not go to Argus at the start. Legion's Argus start trains more than Felguards.
- Plague of Undeath: pin "Legion: N units moved to Alterac Mountains".
- Emerald Portals Shrine of Malorne <-> Feralas work both ways.
- Bot Sentinels at Feathermoon: pin "Feathermoon: 3 Sentinels buildings placed", "Shores of Feathermoon" completed,
  Maiev trainable; later "hero visit 'Vault of the Wardens'".
- Bot Frostwolf: "hero visit 'A Feast for Our Kin'" once Stormheim is held by it or an ally.
- `-icontest 1` ... `-icontest 6`: which numbers show on the minimap?

## Open questions

- [you] Thandol bridge sooner? WL removes its "Bridge Rocks" at turn 30 (`Setup/RockSetup.cs`, expiry 30); the AI
  now follows that. Opening it earlier = a WL change (expiry), e.g. 20 -- for everyone, not just bots.
- [you] Broken Isles: a more lore-fitting connection than the Azsuna ship.
- [you] Kul Tiras pathway while Stormwind City is still locked: extra landing, or path through locked CPs?

## Later

- Switch paths: Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the betrayal, Dalaran -> Kalimdor,
  Sentinels -> Maiev, Warsong/Frostwolf -> Dragonmaw (Old Gods side), Stormwind -> Honor Hold, mobile Ragnaros
  for Ironforge.
- Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
