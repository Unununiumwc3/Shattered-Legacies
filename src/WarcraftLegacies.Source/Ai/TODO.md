# TODO -- Warcraft Legacies bot AI

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work.
Move finished items to `CHANGELOG.md`. Keep this file short.

## Next (0.35.x / 0.36)

- [you] Freezes: a third freeze came at 65 min with alt-tabbing and WITHOUT autopilot, so autopilot is not the
  cause. Likely Proton/Steam + alt-tab. Still worth: one long game without alt-tabbing.
- [ai] Balance (decided: no catch-up mechanics): the evil side should be slightly favoured. Options discussed:
  (a) a flat bot-only buff for the evil teams (more gold, faster tech, cheaper units ...), (b) a "melee upkeep"
  for the good teams: past ~60 food their units cost more or their income drops (bot-only), evil teams exempt.
  [you] pick one.
- [ai] Economy curve ("constant steam"): late game starves (Recruit game: most bots had 0-12 gold and 1-5 units
  from 30 min on, food 0/200). Idea: the vote's strength applies from minute 10-15, and the bonus grows over time
  (low early, higher late) instead of a flat amount, so early waves don't overwhelm and late game isn't empty.
- [ai] Bot food floor of at least 100 (farm-sniping exploit: humans kill a bot's farms and its production stops).
  Re-applied every production turn (WC3 only recalculates food when buildings change).
- [ai] Armies move in clusters instead of streams (cheap: gather at a staging point before a long march, or let
  the front units wait for the rest) -- watch the tick cost.
- [ai] Fel Horde: unfocused Demon Gates never get upgraded -- free upgrade at minute 15, then the further tiers.
- [ai] `-legion` / Legion summon: epic "Civil War" message explaining the alliance shift (Scourge <-> Legion).
  Bug: with a human Scourge (or Legion) the shift is incomplete -- "ally under attack" warnings still play and
  the human still sees the other's resources. Check which alliance flags WL's own betrayal sets.
- [ai] Ironforge too "far" for Fel Horde/Illidan (they skip it and march north to Dalaran) -- path weights, see
  path-based targeting below.
- [you/ai] Jaina's Sanctum (-7555, -9237) needs a CP (neutral, invulnerable until Jaina/Dalaran gets the event,
  then capturable) to connect it. A CP is map data (a unit + WL ControlPoint setup), not AI code.
- [ai] Tol Barad ship portal (11356, -2009) <-> (11765, -3397) to Menethil Harbour + graph edges.
- [ai] Legion pre-summon: check it now walks to Northrend through the portal (0.35.2 portal-aware distance).
- [ai] Orc factions should pick "Subjugate" in the Tauren / Troll / Ogre quest choices (bots currently take the
  first/default option, probably "Pillage"). Find WL's choice mechanic and make bots choose.
- [ai] Scourge plague: choose one of the invasion options, never "no invasion" (same choice mechanic).
- [ai] Legion/Scourge civil war: start a bit later, with a lore announcement ~30 s before it begins.
- [ai] Kul Tiras Highbank backup feels flat -- add spice (e.g. starting gold for the new start).
- [ai] Scourge never gets Arthas: in the 65-min test Arthas died permanently at 24:41 while still Lordaeron's hero
  (Uther at 25:18), so the Culling / Lich King path never happened. Protect or route Arthas for bots.
- [ai] Ragnaros/Yogg visits: confirm in a playtest (dump PINNED shows "hero visit ..." / "Ragnaros mission ...").

- [ai] Hero revive only with an HQ or altar standing. [you] define "HQ" per faction (can change, can be several).
- [ai] Old Gods late-game portal to the Eastern Kingdoms, replacing the invasion wave and `-ogwave`.
  Asset: unit `n07E` (Shimmering Portal model; an unused preplaced one stands in the Southern Highlands at
  22545, -4784). [you] trigger (suggest: Old Gods team holds ~50 % of Kalimdor CPs, or a minimum time) and both ends.
- [ai] Tomb of Sargeras (`n00J`, 1848651850): gated edge Val'sharah -> Tomb, opened by a hero mission (bot holding
  Val'sharah brings its best hero to the entrance). [you] raise the hero to 12 like Ragnaros, or require a real 12?
- [ai] Generic hero-visit missions (quest + hero + place): Ironforge quest, Yogg-Saron (level-14 hero in the
  prison), Soul Gem, Old Hatreds, Lone Wanderer, Jailor of the Damned ... [you] Ironforge quest title.
- [ai] Portal routing: when a target lies across a portal edge, march to the portal first (WC3 pathing through
  waygates works but clumps).
- [ai] Arthas falls to the Scourge when Lordaeron is weak (forcing rule).
- [ai] Lobby menu for bot gold: -100 / -50 / 0 / +50 / +100 ("Insane, may cause lag").
- [ai] Losing conditions for the fallback events Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [ai] Ragnaros mission: minimum game time (~20 min) before a low-level hero is raised to 12.
- [ai] Neutral CPs pull harder than player CPs (bias 0.5 since v34.2) -- re-check in a playtest.
- [ai] Frostwolf held only 1 CP for a whole 80-minute game (v33) -- investigate its start and frontiers.
- [ai] `map_audit.py` into `tools/ai` (needs reading the map's source files instead of the compiled .w3x).
- [ai] Portal "burning" area (the portals use unit type n036 Dark Portal, abilities A01M/A023): good against
  turtling, but the area is too big -- shrink it on the AI portals (BlzSetAbilityRealLevelField) or use a
  smaller custom portal type for ships.
- [ai] Heroes: bots (and autopilot) learn active skills they never cast. Learn passives/auras first, switch on
  autocast for autocast-capable abilities; real casting later (watch the tick cost).
- [ai] Legion's Argus start placed only 3 of 8 buildings (cramped terrain) -- widen the search or use fixed spots.
- [you] Balance thought was cut off: "another option would be to make the extra gold ..." -- finish the idea.
- [you] Map name "Warcraft Legacies Botified": `src/Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs`
  line ~121 (`map.Info.MapName = $"Warcraft Legacies {version}"`) and `src/WarcraftLegacies.CLI/appsettings.json`
  (`MapSettings.Version`, also used in the published file name). Loading screen: `mapdata/WarcraftLegacies/Info.json`
  (title/text) and the four tiles `Imports/LoadingScreenTL/TR/BL/BR.blp` (source art: `war3mapImported/LoadingScreen.webp`).

## Open questions

- [you] Stromgarde backup castle feels flat for Stormwind (it only gets the castle). Make Stormwind an ally of
  the North Alliance too? Or give the backup more (units, CPs)?
- [you] Bridge near Stromgarde opening sooner: WL's own timed bridge (AI can't change WL timing) or the AI edge time?
- [you] Tol Barad (`n08Q`): who should own it, where does its ship come from?
- [you] Broken Isles: a more lore-fitting connection than the Azsuna ship.
- [you] Kul Tiras pathway: Stormwind City stays locked until rebuilt, so Kul Tiras' ship edge leads nowhere
  early. Extra landing at an always-capturable CP, or let bots path through locked CPs?

## Later (v36+)

- Loading screen: big red diagonal "BOTIFIED" -- via code (a frame/texture over the loading screen is not
  possible from game code; it runs only after loading). Needs an image edit of the four BLP tiles or a new
  loading screen model. Map name "Warcraft Legacies Botified": see the Next section above.

- Switch paths: Quel'thalas -> Sunfury (when the Sunwell falls), Scarlet Crusade -> Legion at the betrayal,
  Dalaran -> Kalimdor, Sentinels -> Maiev, Warsong/Frostwolf -> Dragonmaw at Grim Batol (Old Gods side),
  Stormwind -> Honor Hold, toned-down mobile Ragnaros for Ironforge.
- Performance: re-order only idle/retargeted units, wave-tick rotation, one unit scan per bot turn, gate cache.
- Rebuilding destroyed production buildings (2 min, +1 per repeat, cap 4, gold) + HQ safety net.
- Debug: team standings and winner record in the dump, `-heroes`, `-scout`, `-tp x y`.
- Balance: Old Gods stronger (more islands), Legion/Scourge vs Alliance, Illidan/Fel Horde vs Stormwind/Ironforge.
