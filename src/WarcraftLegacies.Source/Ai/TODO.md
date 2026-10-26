# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Current build: **0.39.9** (map "Shattered Legacies:
Botified Realm v0.39.9"). Next: **0.40** = the refactor (below).

## Where we are (handover, end of the 0.36-0.39 chat)

- **Engine load**: refactor step 0 (UNITS dump, heartbeat file) and lever 1 (`-orders idle`) are in. A 520-min game ran
  without a freeze. Next: one comparison game with `-orders all`, then decide lever 2 (scheduler / waypoints).
- **Production** trains from real train lists (0.38); bot heroes are free and learn skills; starter production
  buildings; bots take over leaving humans.
- **Fallbacks** (`FALLBACKS.md`): trigger buildings for every faction (corner, 3 columns), placeholder fallbacks for
  the good factions (Marin Noggenfogger), Kul Tiras -> **Kezan Enterprise** fully playable (own team, oil income,
  creep market, goblin buildings). Open: HQ choices per faction, a hook before WL's defeat (observer), the real
  fallback factions.
- **Map**: many new CPs (1-gold, 10-gold, 100-gold evil CPs), Old Gods rift, Emerald Portals, graph merge at the rift /
  60:00, Argus Legion-only before the summon.
- **Next playtest**: the list "Check in the next playtest" below.

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

## Production follow-ups

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
- **Archimonde's channel: ON HOLD** (maintainer). With stronger evils and a Legion focused on Kalimdor the fights
  happen around the channel area anyway; the remaining risk (humans sneaking past to snipe him) is covered by the
  guardian summons below.
- [ai] **Guardian summons (decided, not built)**: bots only. A channelling quest hero that gets attacked pulls the
  **24 nearest army units** of its owner to it (fewer if it has fewer; none -> nothing happens), with WC3's Mass
  Teleport look (delay + effects), a ping and a **custom announcement per channel** (Archimonde: "You fool -- you
  thought you could catch me off guard?", Maiev at the Vault ...). At most once per minute. **HQ last stand**: the
  same with a **general announcement** when a bot's HQ is attacked, at most once per 5 min, HQ invulnerable during the
  teleport (~5 s). HQs come from the one HQ list in `FALLBACKS.md` (shared with the fallbacks). Engine: 24 position
  changes, no pathfinding -- negligible.
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
- [you/ai] **Ahn'Qiraj is too far**: the Kalimdor team won but did not finish C'Thun in 520 min. The graph path is
  Spinebark Grove -> Silithus -> Ruins of Ahn'Qiraj -> Tunnels of Ahn'Qiraj -> Temple of Ahn'Qiraj; Silithus ->
  Tunnels alone is ~8 km. More regular CPs in between (map data) would help -- and also give the Old Gods more income.
  [ai] can propose spots from the pathing data.
- [ai/you] **Legion still on the Sunwell side** (0.39.3 test): it WAS summoned (17:39, graphs merged), but the 0.39
  plague support had already teleported its army to Alterac -- from there the Eastern Kingdoms targets are simply
  nearest. The Argus weights only matter for frontiers reached over those edges; they cannot move an army that is
  already in the east. Proposal: at the summon, a bot Legion's army is moved through to Felwood (like the plague
  support), or the plague support stops once the Legion is summoned. Decide.
- [you] **Book of Medivh redesign** (not now): picking it up after the summon is boring. Idea: the Book gets its own
  spell; if the Scourge or the Legion gets it, it can trigger the summon ON THAT SPOT -- tempts good players to take
  it early. The hero would need to be indestructible while channelling (bots cannot defend a channel) -- risk: bad
  portal placement.
- [ai] **Argus not taken by the winners**: after 520 min Fel Horde / Illidan had not captured the Argus CPs. Probably
  the pre-summon edge weight (15) keeps Argus last in their list, and before 0.39.3 the graphs never merged. Re-check.
- [you] **Argus with mixed owners**: without a Legion summon the pre-summon portal (Crystalsong <-> Argus) stays open
  for everyone -- Lordaeron took Argus CPs. Keep it, or only for the Legion's team (needs a per-team portal)?
- [ai] **Eye of Sargeras**: nobody picked it up in 520 min (WL has `EyeOfSargerasPickup`). Find where it lies and
  whether bots can reach it (kill target / hero visit).
- [ai] **Orc air units fly over the ocean**: probably flyers taking the straight line to a target that ground units
  reach by portal. Idea: flyers follow the army anchor instead of the target.
- [ai] **Orders/min in long games**: the heartbeat at 538 min showed ~3700 orders/min with `-orders idle` (all units
  idle at their targets get re-ordered every heartbeat). Compare with `-orders all` before deciding lever 2.
- [you] **South Alliance vs Naga / Fel Horde**: the South Alliance needs a buff (or Illidan / Fel Horde a debuff).
- [you/ai] **CP towers vs bots**: fully upgraded CP towers (strong split-arrow attack) were made against human hit-and-
  run CP stealing; bots handle them badly -> the map felt empty after the start, and winners grew weak later. Ideas:
  weaker towers on bot-owned CPs, or bots avoiding / focusing towers. Also check against the refactor (lever 1).
- Gul'dan's corpse (-673,9918): done in 0.39.6 as the Fel Horde hero visit "Gul'dans Legacy" with army escort --
  check that the army really goes along (pins "hero visit 'Gul'dans Legacy'").
- Kezan Enterprise buildings: done in 0.39.8 (check in the editor / game: Trading Center placement, the creep list,
  stock refresh, oil income on the scoreboard). Old proposal kept below for reference.
- [you/ai] **Kezan Enterprise -- functional buildings** (0.39.8 implemented the maintainer's version). The Kezan buildings exist but most train nothing.
  Proposal: Scrapyard -> siege (Personal Tank, Shredder); Laboratory -> research + Alchemist / Tinker; Mercenary Den
  -> Ogre Mercenary, Sappers, Gunner (pirates: Defias from 24p); Bunker -> Gunners / Marines; Auction House -> item
  shop; Missile Battery / Rocket Tower -> defence; Bilgewater Harbor -> ships (already works); Engineer's Guild
  already trains 3 units. Kezan Oil Supply and Kezan Trading Center should NOT be buildable (they are not in the goblin
  Builder's list -- check which builder offers them). Special purposes, ideas: **Oil Supply** = income building (+X gold
  per minute while you own it, like a small CP) or the requirement for zeppelins / tanks; **Trading Center** = the
  mercenary market (hire ogres / pirates for gold) or a gold-for-lumber trade. Maintainer decides.
- [ai] **Kezan builders invisible** (0.39.6 test: trained builders do not appear). Their model
  (`war3mapImported\DayLaborer.mdx`) is in the imports, so it is something else -- use `-id` on the goblin hall while
  it trains, and check whether the unit exists (`-dump` UNITS count) or is removed / hidden.
- [ai] (low) **Chinese language**: the latest regular WL build seems to have Chinese localization -- check upstream
  and port it (WL's `Loc` system).
- Note: WL has no `-spectate` command; the AI takes over a slot only when its human LEAVES (WL's leave event).
- [ai] (low) Bot heroes die early: a bot trains its hero and attacks creep camps without an army. Accepted for now
  (they rebuild); probably fixed by group movement (refactor lever 4).

## Check in the next playtest (0.39.x)

- Heartbeat file and UNITS dump section present; compare `-orders idle` vs `-orders all` (orders/min, feel).
- Fallback buildings in the corner: 3 tidy columns at x ~22208 from y 23500; Ironforge / other good factions: losing
  all HQs -> "fallback ready" (human) or an immediate placeholder fallback (bot); recapturing an HQ removes it again.
- Kul Tiras fallback: "Kezan Enterprise" on the scoreboard in its own team ("... Pact"), Katherine on Kezan, the Kezan
  buildings and creeps are hers, Oil Supplies +50 gold/min each, Trading Center stock 3 / +1 per 2 min, Laboratory /
  Mercenary Den / Tank Factory train, Altar of Industry heroes. Builders visible? (`-id` on the goblin hall).
- Legion summon: the Scourge leaves the team as "Scourge Pact" on the scoreboard.
- New CPs reachable (`-edges`): Ahn'Qiraj chain (Scarab Wall - Hive'Zora cheap), Deathknell / Terrordale to the Scourge
  at the plague, Sulfuron Spire to Skywall at the Fireland Invasion, Twilight Shore, the Nexus.
- 100-gold CPs show "(100 gold/min)": Ny'alotha, Vortex Pinnacle, Temple of Ahn'Qiraj, Icecrown Citadel.
- Emerald Portals (Shrine of Malorne <-> Feralas) and the Old Gods rift work both ways; no duplicates.
- Hero visits: Gul'dans Legacy (army escorts), Vault of the Wardens, A Feast for Our Kin; Yogg released by the bot.
- Book of Medivh: the new ability "Guardian's Abjuration" (5 s).
- Starter production pins at 0:35; Continental Outpost / Feathermoon buildings for bots.

## Open questions

- [you] Thandol bridge: opening sooner is NOT needed as long as bots don't try to cross before it opens (0.37.2 gate
  "thandol") -- maintainer re-checks in playtests.
- [you] Broken Isles: they are reached only by the AI ship Azshara Coast <-> Azsuna (added in v34 after Illidan's
  Broken Isles grant was removed). Like every AI portal it is a real waygate -- **humans use it too**, it changes the
  map for everyone. Question: is a ship from Kalimdor the right lore connection, or should the Isles connect
  elsewhere (e.g. Dalaran / the Eastern Kingdoms)?

## Later

- Switch paths: Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the betrayal, Dalaran -> Kalimdor,
  Sentinels -> Maiev, Warsong/Frostwolf -> Dragonmaw (Old Gods side), Stormwind -> Honor Hold (most of these are now
  covered by FALLBACKS.md).
- Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
