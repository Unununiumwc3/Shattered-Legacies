# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short. Next code version: **0.37.0**.
Each `[you]` item carries a proposed default -- answer "ok" and it gets built that way.

## 0.37 -- needs a decision first

- [you] **Economy curve** ("constant steam"; Recruit had 0-12 gold, 1-5 units from 30 min). Proposal: every bot
  gets a base subsidy that does not depend on the vote (15 gold/turn = ~60/min) so even Recruit keeps producing; the
  vote's amount is added on top, ramping in linearly between 10:00 and 15:00, then +10 % per 10 min (cap 2x at
  ~115 min). Recruit = base only. Open: is a base for Recruit acceptable, or must Recruit stay at 0?
- [you] **Balance** (no catch-up mechanics; evil slightly favoured): (a) flat bot-only buff for the evil teams,
  or (b) "melee upkeep" for the good teams past ~60 food (bot-only). Your thought "make the extra gold ..." was cut
  off -- finish it. Proposal: decide after a playtest with the economy curve, since it changes the baseline.
- [you] **Legion summon fallback** (summon never happened in 40-85-min tests). Proposal: if the Scourge hasn't
  taken the Sunwell by 45:00 and the Legion is a bot, summon anyway; announcement ~30 s before (lore text); then the
  civil war as today. Also: read which alliance flags WL's own betrayal sets and copy them (the human Scourge/Legion
  case had "ally under attack" warnings and shared resources left over). Alternative: trigger on Legion army size.
- [you] **Arthas** died permanently at 24:41 as Lordaeron's hero, so the Scourge never got him. Proposal: a bot
  Lordaeron keeps Arthas out of fights (with the base, not the army) until the Scourge's Arthas quest has run.
  Alternative: only skip the army orders while his HP < 50 %.
- [you] **Quest choices**: WL uses `ChoiceDialogPresenter` / `FactionChoiceDialogPresenter` (MacroTools). Orcs should
  pick "Subjugate" (Tauren/Troll/Ogre), the Scourge's plague an invasion option (never "no invasion"). Proposal:
  a config list (quest/choice title -> wanted option) applied for bots; needs a look at how the presenter resolves a
  pick without a click.
- [you] Old Gods late-game portal to the Eastern Kingdoms (replaces the invasion wave and `-ogwave`). Asset `n07E`
  (unused preplaced one at 22545, -4784). Trigger and both ends?
- [you] Kul Tiras Highbank backup start: "spice" = starting gold? How much, and how do we detect that the backup
  start was used (the Admiralty quest failing?)
- [you] More hero visits: Ironforge quest title. (Lone Wanderer / Jailor of the Damned need a Kalimdor -> Northrend
  route for Frostwolf: a ship portal, or let the visit teleport the hero? Proposal: ship portal Durotar <-> Borean
  Tundra, phase "always".)

## 0.37 -- implementation work

- [ai] **Finishing off losers**, part 2: send stragglers (units on a landmass away from the anchor group) through
  portals to the main group; merge targets of allied bots on the same continent.
- [ai] **Path-based targeting**: precomputed walking distances between connected CPs as edge weights, shortest
  path from the army (Dijkstra). 0.35.2 (portal-aware distance), 0.35.3 (`Edge.Weight`) and 0.36 (anchor on the
  biggest group) are steps. `tools/ai/map_query.py` + the pathing map could precompute the distances offline.
- [ai] **Clustered armies**: gather at a staging point before a long march, or let the front wait for the rest.
- [ai] Heroes: learn passives/auras first, autocast on for autocast-capable abilities; casting later. (Needs the
  skill ids per hero -- extract from mapdata UnitData `heroAbilList` like the upgrade lists.)
- [ai] Debug: `-scout` (what should it show?), winner record when WL declares victory.

## 0.37 -- polish / small

- [you] Map name "Warcraft Legacies Botified": `src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs`
  ~line 121 and `src/WarcraftLegacies.CLI/appsettings.json` (`MapSettings.Version`). Loading screen: title/text in
  `mapdata/WarcraftLegacies/Info.json`; a red "BOTIFIED" needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`
  (the loading screen shows before any game code runs).
- [you/ai] Jaina's Sanctum (-7555, -9237) needs a CP (map data, not AI code), then graph edges.
- [ai] Losing conditions for Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [ai] `tools/ai/map_query.py` (0.36) -> a full `map_audit.py`: check every coordinate in AiConfig.cs (portals,
  visits, kill targets) for walkability/landmass in one run.

## Check in the next playtest (0.36)

- Bots with a split army keep one target (dump: `anchor=army@x,y(Nu)`, `kept=1` now and then, fewer target lines
  in the EVENT LOG). Winners take the last CPs of beaten factions.
- Bot food never below 100 (`-debug4` / dump `food=`).
- Legion Argus pin: "8 of 8 buildings (8 on fixed spots)"; Legion units still walk to the portal.
- AI portals: units next to a ship portal no longer burn (if they still burn at 1000 range, the per-unit field
  change didn't take -- then remove A01M/A023 from AI portals instead).
- Fel Horde: "timed upgrade ... Demon Gate" lines after 15:00 / 20:00.
- Pins: "gate 'greymane' open", "gate 'tomb' open", hero visits for Tomb / Soul Gem / Old Hatreds, "stranded hero
  ... moved", "STANDINGS: team ... reached 80". `-heroes` and the TEAMS dump section.
- From 0.35.3, still open: Felwood portal takes ground units after `-legion`; Tol Barad ship; dwarves stay south
  while the Thandol bridge is closed; hero revive only at a living altar; Ragnaros only with a real level-12 hero.
- Freezes: so far only with alt-tabbing on Linux/Proton (with and without autopilot) -- one long game without
  alt-tabbing would settle it.

## Open questions

- [you] Stromgarde backup castle feels flat for Stormwind. Make Stormwind an ally of the North Alliance too?
- [you] Thandol bridge opening sooner: WL's own timed bridge (AI can't change WL timing) or the AI edge time?
  Note: `AiGates.OpenThandolIfDue` (force-open at 20:00) exists but has not been called since 0.35 -- re-enabling
  it is one line.
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
