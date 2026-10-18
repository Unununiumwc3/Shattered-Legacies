# Changelog -- Warcraft Legacies bot AI

Versions are `0.MINOR.PATCH` (feature build = MINOR, fix-only build = PATCH). Git tags carry the WL base in front:
`4.8.2-ai.0.35.0`. The in-game banner and every `-dump` header show the same version.

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
