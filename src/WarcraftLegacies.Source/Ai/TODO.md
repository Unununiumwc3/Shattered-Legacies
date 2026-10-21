# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Current build: **0.38.1** (map "Warcraft Legacies
Botified 4.8.2-ai.0.38.1"). Next: **0.39.0** = refactor phase 1 + 2 (below).

## Freeze check (still open)

Freezes so far: 0.36.0 minute 1 and 0.37.1 turn 17, both while alt-tabbing (Ubuntu, Battle.net via a Proton
prefix); other games ran 30+ min. **0.37.1: picture frozen, sound kept playing** -> points to the graphics layer.
Music streams on its own thread, so this is a hint, not proof. Next steps, cheapest first:
1. ~~Picture or game?~~ Done: sound kept playing. Next time also check: do unit/ambient sounds or chat sounds still
   come (game logic alive), or only the music?
2. **Alt-tab only?** 3 starts without alt-tabbing, 10 min each.
3. **AI or not?** Same lobby without computer players (the AI then does almost nothing), alt-tab heavily.
4. **Window mode:** windowed / windowed fullscreen instead of fullscreen.
5. **Proton log:** launch option `PROTON_LOG=1 %command%` -> `~/steam-<appid>.log`, last lines before the freeze.
6. **Windows 11:** one tester, same build and lobby, 10+ alt-tabs in minute 1.
A map hang also happens without alt-tabbing and, in multiplayer, freezes every player.

## Refactor (agreed concept, 0.39+)

Vision: bots replace human players and challenge them -- mostly through **many troops on the map** (that is the
fun; a food cap is the last resort). So the engine budget must go to units, not to AI overhead. Each phase is
playable on its own; changes stay inside `Ai/` (outside: only the hooks listed in `AI_AGENTS_CONTEXT.md`).
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

## Fallbacks and alternative paths (concept, maintainer ideas)

- [you] **Stromgarde fallback -> alliance change**: when Stromgarde becomes Stormwind's fallback (the AI's quest rule
  "Stromgarde"), Stormwind joins the North Alliance -- only if it was triggered as the fallback.
- [you] **Aerie Peak fallback for the dwarves** (like Stromgarde): Ironforge falls -> Aerie Peak -> alliance change,
  again only when triggered as the fallback.
- [you] **Sunfury as Quel'thalas' fallback** (instead of a random Illidari/Sunfury pick, which stays Basic).
- [you] **CPs and food**: tie the food cap to the CP count, not only the income.
- The 24-player map has no extra content to transplant (checked 0.38.1: 224 unit types differ, all older versions;
  no separate Dragonmaw camp -- only the Dragonmaw Port CP, in both maps). Dropped from the plans.

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

## Check in the next playtest (0.38.1)

- **Minimap icons**: still not visible on the AI portals in 0.38. Run `-icontest` (single-player, select a unit):
  six icons eastwards, numbered -- tell me which numbers show; the portals then use that variant.
- Terrordale: right after "Plague of Undeath" the pin "Terrordale handed to the Scourge"; the Scourge base near
  Stratholme survives.
- Yogg-Saron: the hero visit only for a bot holding Storm Peaks; after it, "releases Yogg-Saron from his prison",
  then "AREA CLEAR Yogg-Saron" by the Storm Peaks holder.
- Ysondre: "AREA CLEAR Ysondre started" once a bot holds Shrine to Malorne and she is visible (0.38: skipped at 2:00
  as "already dead" -- she was hidden).
- Old Gods rift, second trigger: a non-Old-Gods faction holds the Maw of Gor'ma and N'Zoth is dead -> "OLD GODS RIFT
  open: Maw of Gor'ma taken by ..." with its own text.
- Legion after the summon goes to Kalimdor through Felwood (weights: Felwood 1, Sunwell side / Northrend 6).
- Faction picks: Sentinels slot now also random (Sentinels / The Exodar).
- From 0.38 (seen working: TRAINED variety, quest heroes like Rommath/Lor'themar, LOAD as before, Sulfuron barracks,
  Taerar, Ramzes, Dalaran pick, Black Empire on Kalimdor, Book/Legion pins not yet seen).

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
