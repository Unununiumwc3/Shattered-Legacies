# Changelog -- Warcraft Legacies bot AI

Versions are `0.MINOR.PATCH` (feature build = MINOR, fix-only build = PATCH). Git tags carry the WL base in front:
`4.8.2-ai.0.35.0`. The in-game banner and every `-dump` header show the same version.

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
