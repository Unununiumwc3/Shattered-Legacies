# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Current build: **0.37.2** (map "Warcraft Legacies
Botified 4.8.2-ai.0.37.2"). Next code version: **0.38.0**. Playtest of 0.37.x still running -- no big changes until
it is done.

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

## General overhaul: performance, food, gold, balance (own project, 0.38+)

Goal: the game runs smoothly with many armies, bots stay a challenge without flooding the map. Note (maintainer):
24-player melee maps with melee bots run fine on big maps -- so map size alone is not the reason. Measure before
changing (bullet 1), then try the levers one at a time.
- **Measure first** [ai]: count units per player and in total, AI orders per minute (the dump's LOAD section +
  `-debug2`), at 10 / 20 / 30 / 45 min; compare with a 24-player melee map with melee bots in the same setup.
  Suspects to rule in/out: total unit count (WL starts with ~1,900 hidden preplaced units + creeps + every bot's
  army), how often the AI re-issues attack-move to whole armies (every 20 s today), cross-map orders (long paths).
- **Hard food caps** [you]: a cap for ALL players, e.g. 150 or 200 (WL's faction maximums are 160-612 today, seen in
  the 0.36 dump) -- or a lower cap only for bots, per difficulty (e.g. Recruit 100 ... Warlord 200).
- **Bonus gold model** [you] (designed in discussion, not built): per minute through WL's `BonusIncome` (shown on
  the scoreboard, independent of the bot count -- today's per-production-turn subsidy pays more the fewer bots
  there are). Start at 11:00 (when WL's starting income ends); Recruit 0.
  `bonus/min = D x (20 % of WL StartingGold.Income + 10 % of CP income, CP income counted up to 300)`, D: Recruit
  0, Soldier 0.5, Veteran 1, Commander 1.5, Warlord 2. Evil factions automatically get more (higher WL start income).
  **Food upkeep on the bonus only**: food used < 100 full, 100-149 half, 150-199 quarter, 200+ none.
- **Alternatives / additions to compare** [you]: (a) a flat amount per CP instead of a share of CP income;
  (b) diminishing returns (square root of CPs) instead of a hard cap; (c) the bonus only while the bot is behind its
  team's average CPs (catch-up); (d) bot-only upkeep that raises unit prices like melee (rejected for now --
  keep prices); (e) gold to buildings/upgrades instead of units (less army, same strength via tech).
- **Melee-like AI behaviour** [ai]: attack groups of limited size (30-40 food) instead of the whole army; orders only
  for new or idle units; short waypoint orders (next CP) instead of cross-map orders.
- **Balance** [you]: no catch-up; evil slightly favoured. Revisit after the gold model and food caps.

## 0.38 -- needs a decision first

- [you] **Old Gods rift, second trigger** (good side): the Maw of Gor'ma taken by a NON-Old-Gods faction AND "the big
  Old Gods HQ" fallen -> the rift opens for them. Which building or unit is that HQ -- N'Zoth (`U01Z`, Black Empire,
  Essential), the Pyramid of Madness (`n0DU`, -26432,-5952), or another? Then: lore text + pings like trigger 1.
- [you] "The other pair in the Eastern Kingdoms can also spawn for the good guys if they win -- maybe the Scourge
  losing the Frozen Throne?" -- which portal pair (both ends), and is the trigger "Frozen Throne destroyed/ruptured"?
- [you] **Archimonde's channel** (420 s in WL's quest "Twilight of the Gods"): 0.37.1 sends him into the channel
  spot once the Legion holds Nordrassil and stops ordering him there. Open: (a) the Legion army defends him while
  he channels (gathers at the spot), (b) reinforcements go there, (c) a shorter channel for bots (changes WL's
  quest -- outside the AI). Humans can interrupt him; he is PermaDies (died for good at 29:08 in the 0.36 test).
- [you] **Defend range**: the 0.36 dump showed the Legion "defending" a shrine 26 km from its army. Proposal: defend
  only CPs within ~8000 walking distance of the army; farther ones are left to the faction's other groups / allies.
- [you] **Other faction picks for bots**: Dalaran/Gilneas, Sentinels/Draenei, Illidari/Sunfury stay Basic (Draenei
  and Sunfury have no AI data). Random too, once the data exists?
- [you] **Legion summon, second trigger** (lore: "Under the Burning Sky" -- Book of Medivh in Dalaran): a bot Legion
  takes the Book once Dalaran's capital is dead (any Legion unit at the pedestal) -> summon. Keeps the Sunwell
  trigger. Yes / no?
- [you] Kul Tiras Highbank last stand (`QuestRule` with `CapitalLost`, "the fleet falls back to its outpost")? Note:
  Highbank is the reward of "Eliminate Piracy" (Booty Bay + Katherine), not a backup start.
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`
  (the title text is already "Warcraft Legacies Botified ..." in red).

## 0.38 -- implementation work

- [ai] **Frontiers behind a barrier**: a frontier can come from an ALLY's node on the far side of a closed barrier
  (0.37.1: Ironforge targeted Arathi Highlands through an ally node north of the Thandol rocks; the army stood at the
  rocks). 0.37.2 limits it (stuck back-off 5/10/20 min). Real fix: only frontiers the army can walk to -- comes with
  the walking costs below (barriers = gates/rocks as closed edges).
- [ai] **CP graph weights**: `tools/ai/graph_weights.py` -- walking distance per edge from the pathing data
  (feasibility checked: 176 of 187 edges; median 1.08x the straight line; outliers Antoran Wastes <-> Nath'raxas
  Hold 3.05x, Burning Steppes <-> Fuselight 2.1x). Ship/portal edges = walk to the portal + from the exit + boarding
  penalty; bridge destructibles must be added as walkable. Generated base cost per edge; `Weight` stays the hand
  knob. Frontier picker: route cost from the army over the graph (Dijkstra, ~190 edges). Names: **CP graph**
  (`AiGraph`, `Edge`), **frontier picker** (`PickGraphTarget`).
- [ai] **Warsong as a bot** (it can now be picked): goals for its subdue quests -- Echo Isles (trolls), Thunder
  Bluff (tauren), Stonemaul (ogres, dead-end CP behind Dustwallow Marsh); each also needs Grom alive.
- [ai] Old Gods bots switch to the merged graph when the rift opens (until the Legion summon merges the graphs, a
  Kalimdor-set bot only sees Dragonmaw Port behind the rift).
- [ai] Heroes: learn passives/auras first, autocast on (skill ids from mapdata UnitData `heroAbilList`).
- [ai] Losing conditions for Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [ai] `tools/ai/map_query.py` -> `map_audit.py`: check every coordinate in AiConfig.cs in one run.
- [ai] Finishing off losers part 2 (stragglers through portals, allied bots merge targets).
- [ai] Debug: `-scout` (define first), winner record when WL declares victory.

## Check in the next playtest (0.37.2)

New in 0.37.2:
- Dwarves no longer walk to the Thandol bridge before WL's rocks are gone (pin "gate 'thandol' open: ..." at about
  30:00). A repeated "STUCK on X" shows growing skip times (300 / 600 / 1200 s).
- Jaina's Sanctum is a CP (10 gold/min, Theramore, next to Jaina's Sanctum building); bots reach it from Alcaz Isle.
From 0.37.1 (not all seen yet):
- Pins "bot pick P0: faction choice ..." (orc slot Frostwolf or Warsong; seen) and "Scourge invasion option ..." --
  the bot Scourge invades. Humans still get their dialogs.
- Old Gods take Nordrassil -> "OLD GODS RIFT open", purple text + pings; the Twilight Highlands portal works both
  ways (`-ogrift` forces it).
- "Sulfuron Spire: ... barracks placed" after Skywall's Fireland Invasion; troops come out there.
- "AREA CLEAR Ramzes the Horror started" by whoever holds Desolace (or "skipped: target already dead").
- Archimonde walks into the channel spot after the Legion takes Nordrassil and keeps channelling (timer dialog).
- Old Hatreds only from Daelin level 10; no "hero lost" loop.
- Stranded heroes in the Black Empire realm / Icecrown -> "moved to its CP ... near the army".
- Scourge trains Arthas right after "The Culling"; no second Arthas after "The Ascension".
- Still from 0.36: AI portals no longer burn at 1000 range; Felwood portal after `-legion`; Tol Barad ship;
  hero revive only at a living altar; Ragnaros only with a real level-12 hero.

## Open questions

- [you] Stromgarde backup castle feels flat for Stormwind. Make Stormwind an ally of the North Alliance too?
- [you] Thandol bridge sooner? WL removes its "Bridge Rocks" at turn 30 (`Setup/RockSetup.cs`, expiry 30); the AI
  now follows that. Opening it earlier = a WL change (expiry), e.g. 20 -- for everyone, not just bots.
- [you] Broken Isles: a more lore-fitting connection than the Azsuna ship.
- [you] Kul Tiras pathway while Stormwind City is still locked: extra landing, or path through locked CPs?

## Later

- Switch paths: Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the betrayal, Dalaran -> Kalimdor,
  Sentinels -> Maiev, Warsong/Frostwolf -> Dragonmaw (Old Gods side), Stormwind -> Honor Hold, mobile Ragnaros
  for Ironforge.
- Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- 24-player map (`mapdata/WarcraftLegacies24p`): old 1.18a layout, 20 graph nodes missing, 65 moved -- not planned.
