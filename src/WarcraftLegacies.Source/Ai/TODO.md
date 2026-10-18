# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Next code version: **0.36.0**.

## 0.36 -- gameplay (playtest after)

- [ai] **Finishing off losers**: winners struggle to take the last CPs of beaten factions. 85-min dump: winning
  armies are split over several landmasses (Sentinels 98 on an island + 84 on the mainland, Druids over 5
  landmasses), the army centroid lands in the sea, targets switch every few seconds. Ideas: anchor on the biggest
  same-landmass group instead of the centroid, stick to a target until taken or blacklisted, send stragglers
  through portals to the main group.
- [ai] **Path-based targeting**: precomputed walking distances between connected CPs as edge weights, shortest
  path from the army (Dijkstra). 0.35.2 (portal-aware distance) and 0.35.3 (`Edge.Weight`) are first steps.
  Fixes "Ironforge too far" for Fel Horde/Illidan and long cross-map marches.
- [ai] **Clustered armies**: gather at a staging point before a long march, or let the front wait for the rest.
- [ai] **Stranded heroes**: Illidan's heroes stood in the sea in the middle of the map (85 min) -- probably a
  quest event moved them. Rule: a hero far from its army that hasn't moved for ~2 min walks to the army; if it
  still can't, teleport it to its altar. A `-heroes` command (ping + label per hero) helps find such cases.
- [ai] **Economy curve** ("constant steam"): late game starves (Recruit: 0-12 gold, 1-5 units from 30 min). The
  vote's strength applies from minute 10-15 and the bonus grows over time instead of a flat amount.
- [ai] **Bot food floor >= 100** (humans kill farms -> production stops). Re-applied every production turn.
- [you] **Balance** (no catch-up mechanics; evil slightly favoured): (a) flat bot-only buff for the evil teams,
  or (b) "melee upkeep" for the good teams past ~60 food (bot-only). Your thought "make the extra gold ..." was cut
  off -- finish it.
- [ai] **Legion summon never happened** in 40-85-min tests (needs the Scourge to take the Sunwell). Add a fallback
  (time-based or Legion strength) -- plus the civil war: start a bit later, lore announcement ~30 s before, and
  fix the incomplete alliance shift with a human Scourge/Legion ("ally under attack" warnings, shared resources;
  check which alliance flags WL's own betrayal sets).
- [ai] **Arthas**: died permanently at 24:41 as Lordaeron's hero, so the Scourge never got him. Protect or route
  Arthas for bots.
- [ai] **Quest choices**: orcs pick "Subjugate" (Tauren/Troll/Ogre), Scourge's plague picks an invasion option
  (never "no invasion"). Bots take the first/default option today -- find WL's choice mechanic.
- [ai] Fel Horde: unfocused Demon Gates -> free upgrade at minute 15, then the further tiers.
- [ai] Heroes: learn passives/auras first, autocast on for autocast-capable abilities; casting later.
- [ai] Kul Tiras Highbank backup start: add spice (e.g. starting gold).
- [ai] Portal "burning" area (unit n036, abilities A01M/A023) too big -- shrink on the AI portals.
- [ai] Legion's Argus start places only 3 of 8 buildings (cramped) -- fixed spots.
- [ai] Old Gods late-game portal to the Eastern Kingdoms (replaces the invasion wave and `-ogwave`). Asset `n07E`
  (unused preplaced one at 22545, -4784). [you] trigger and both ends.
- [ai] Tomb of Sargeras (`n00J`): gated edge Val'sharah -> Tomb, opened by a hero visit (WL needs level 10).
- [ai] More hero visits: Soul Gem, Old Hatreds, Lone Wanderer, Jailor of the Damned ... [you] Ironforge quest title.

## 0.36 -- polish / small

- [you] Map name "Warcraft Legacies Botified": `src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs`
  ~line 121 and `src/WarcraftLegacies.CLI/appsettings.json` (`MapSettings.Version`). Loading screen: title/text in
  `mapdata/WarcraftLegacies/Info.json`; a red "BOTIFIED" needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`
  (the loading screen shows before any game code runs).
- [you/ai] Jaina's Sanctum (-7555, -9237) needs a CP (map data, not AI code), then graph edges.
- [ai] Losing conditions for Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [ai] `map_audit.py` into `tools/ai` (read the map's source files instead of the compiled .w3x).
- [ai] Debug: team standings + winner record in the dump, `-heroes`, `-scout`, `-tp x y`.

## Check in the next playtest

- Felwood portal takes ground units after `-legion` (0.35.3). Legion walks to Northrend through the portal early.
- Tol Barad ship. Dwarves stay south while the Thandol bridge is closed (`Edge.Weight` 3).
- Hero revive only at a living altar. Ragnaros goes only with a real level-12 hero.
- Freezes: so far only with alt-tabbing on Linux/Proton (with and without autopilot) -- one long game without
  alt-tabbing would settle it.

## Open questions

- [you] Stromgarde backup castle feels flat for Stormwind. Make Stormwind an ally of the North Alliance too?
- [you] Thandol bridge opening sooner: WL's own timed bridge (AI can't change WL timing) or the AI edge time?
- [you] Broken Isles: a more lore-fitting connection than the Azsuna ship.
- [you] Kul Tiras pathway while Stormwind City is still locked: extra landing, or path through locked CPs?

## Later

- Switch paths: Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the betrayal, Dalaran -> Kalimdor,
  Sentinels -> Maiev, Warsong/Frostwolf -> Dragonmaw (Old Gods side), Stormwind -> Honor Hold, mobile Ragnaros
  for Ironforge.
- Performance: re-order only idle/retargeted units, wave-tick rotation, one unit scan per bot turn.
- Rebuilding destroyed production buildings + HQ safety net (no barracks -> HQ trains basics).
- 24-player map (`mapdata/WarcraftLegacies24p`): old 1.18a layout, 20 graph nodes missing, 65 moved -- would need
  its own graph and coordinates; not planned.
