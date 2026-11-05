# Changelog -- Shattered Legacies (fork content)

One version number for the whole map (`README.md`, "Map name and versions"). Bot changes are in
`../Ai/CHANGELOG.md`; up to 0.51.1 that file held the fork's changes too (fallbacks 0.39.1-0.39.10, Kezan Enterprise
0.39.5-0.39.8, map name 0.39.9, the extra CPs 0.39.4-0.46.2).

## 0.57.4
Fix from the 0.57.3 playtest. Version v0.57.4.
- **Alliances of the free slots**: WL's lobby setup (`Info.json`) put every slot outside the lobby -- also P11, P14,
  P20 -- into the "Scourge, Legion" force (allied, shared vision). WL's teams only add alliances, never remove
  them, so Quel'thalas on P11 stayed allied with the Scourge and the Legion (their units healed it, it only fought
  the Forsaken) although the scoreboard showed the right teams. The three slots are now only in the "NPC" force, and
  a split makes everyone outside the slot's team an enemy, both ways (`SplitFaction.UnallyOutsideTeam`).
- Not solved yet (`TODO.md`): Sunfury on a bot slot builds no troops.

## 0.57.3
Fixes from the 0.57.2 playtest. Version v0.57.3.
- **Sunfury on a bot slot did not happen** (scoreboard changed, the units stayed Quel'thalas'): registering Sunfury
  at turn 27 failed -- one of its quests waits for a fixed turn (18) that had passed ("Cannot register callback for
  turn 18 when current turn is 27"), which stopped the swap halfway. While a faction arrives, WL turn callbacks for
  past turns now run on the next turn (`GameTimeManager.ClampPastTurns`, `[SL]` in `TurnCallback`), and
  `ObjectiveTurn` counts from the arrival like the deadlines.
- **The free slot showed "NPC"** when its units were hovered: the slot takes its faction's name (split, swaps).
- **Scarlet Crusade's "A Once Great People" was completed for the bot at once** (bot): starting quests without a
  deadline are completed only for factions of the game start; deadline quests only if the deadline lies after the
  faction's arrival (WL's Scarlet Crusade arrives with deadlines already past).
- **Chaos Infusion** (Graveyard) costs 10 knowledge per level besides its gold (maintainer).
- **The Altar of the Queen** becomes destructible when the Forsaken are free ("Free Will").

## 0.57.2
Bot quest timing for starts and turning points; wrap-up of the 0.52-0.57 session. Version v0.57.2.
- The free slots P11 / P14 / P20 are computer players fixed by the map (the map's "fixed player settings"): humans
  cannot take them in the lobby.
- Bot quest timing (bot, see `../Ai/CHANGELOG.md`): starting quests and deadline quests of bot factions are completed
  for them in the last 30 seconds of the turn before -- also for factions that arrive with a turning point (their
  deadlines are shifted by the arrival turn, `ObjectiveExpire.ExpirationOffset`).
- `[SL]` in WL: `GameTimeManager.TurnTimeRemaining`, `ObjectiveExpire.ExpirationTurn` (read-only, for the bot).
- Docs: handover in `AI_AGENTS_CONTEXT.md`; the Undercity corner and terrain options in `TODO.md`.

## 0.57.1
Fixes from the 0.57.0 playtest. Version v0.57.1.
- **Free slots are computer slots** (maintainer): P11, P14, P20 are "NPC" computer players in the lobby
  (`Info.json`, force "NPC"). Quel'thalas on a free slot was not a computer player, so the bot only half played it
  (no troops after the Sunfury fallback, no starting quests). The bot leaves them alone until a split hands them a
  faction (`AiApi.ReserveSlot`, bot). Free = no faction and no human in it.
- **Starting quests of late factions** (bot): a computer faction that registers mid-game gets its starting quest
  completed like at the start, and its deadline quests join the bot's early-quest list.
- **The Throne of Lordaeron**: capitals are taken over, not destroyed -- the objective is now to hold the Capital
  Palace; then it falls into ruins by itself (removed, rubble, the gate in the middle), so no one can destroy it
  first.
- **The Undercity's buildings and gate were hidden**: WL hides all Neutral Passive units in that corner region at
  setup; the square is now built a second later.
- **Ships**: the main map's Transport Ship joins the Forsaken; their Corsair and Dreadnought need Navigation.
- `TODO.md` reordered: Kezan Enterprise next, the Forsaken's small issues listed.

## 0.57.0
The Forsaken reworked after the 0.56 playtest, the Undercity as a base of its own. Version v0.57.0.
- **Sylvanas** (`forsaken_heroes.py`): Domination instead of Possession (Charm-based: she keeps her body); Ghostform
  instead of Ethereal Walk (20 s, +25% speed, its own buff icon; the hidden "Ethereal" ability no longer shows as a
  button without an icon); the Banshee is an intelligence hero.
- **Gates of the tree** (`import_24p.py`):
  - "The Banshee's Rise" (new quest, Sylvanas level 6): until then the Plaguetorium is the only troop building; the
    Laboratory, the Forbidden Archive and the Haunted Harbor need it.
  - "Free Will" (the Dark Ranger, off the Scourge's team) later and harder: Sylvanas level 10, the Scourge's "The
    Culling" done, and the Scourge below 10 CPs -- while it holds 10 it keeps the Forsaken. New building block
    `ObjectiveFactionState` (another faction's quest done / CPs below N).
  - Tier 4 only after Free Will; only the Val'kyr, Plague Flyer, Elite Abominations, Elder Banshee and the Royal
    Sepulcrum need it.
  - Ships like every faction's: the Graveyard researches Navigation (and Flight -- the Plague Flyer needs it); the
    Haunted Harbor no longer needs Tier 4.
- **"The Throne of Lordaeron"**: Lordaeron's Capital Palace must fall too (its quest marker on the minimap). Its
  ruins get a waygate into **the Undercity** -- the eastern unused square of the south-west corner, prepared at map
  setup, visible and invulnerable: a waygate in the middle, the Undercity north of it, Plaguetorium west, Graveyard
  east, Forbidden Archive south, a Plague Tower in each corner, rocks and line-of-sight blockers along the edge (no
  shooting across the void). The buildings become the Forsaken's. New building block `Core/Effects/Instance`.
- **Hourglass of Fate**: every faction's looks like its own control point tower (its CP defender's model).
- Build fix (before release): the Val'kyr's new requirement was written without its text type -- the publish step
  stopped ("requires an element of type 'Number'"). `import_24p.py` writes typed values; new check
  `tools/shattered-legacies/content/check_json.py` (every value against its type).
- WL bug: the Plague's refund threw "Tried to call DropAllItems on a summoned hero" (`RefundSystem`, `[SL]`).

## 0.56.1
Fixes from the 0.56.0 playtest. Version v0.56.1.
- Possession, Anti-magic Shell and Ethereal Walk did not appear (the skill point was spent, no button): the stock unit
  spells they are made from require Banshee Adept / Master Training and Berserker training. `forsaken_heroes.py` sets
  no requirement (`areq` "").
- Banshee Wail showed no level progress: explicit values per level (25 / 40 / 55 / 70% less damage) and tooltips
  that name them.
- `AI_AGENTS_CONTEXT.md`: the building blocks so far and the pitfalls learned, so a new session reuses them.

## 0.56.0
Sylvanas' three heroes, "Plague Research", income for new starts. Version v0.56.0.
- **Sylvanas: one hero per phase** (generated by the new `tools/shattered-legacies/content/forsaken_heroes.py`; WC3
  cannot change which skills a hero learns). At a phase change WL's own `Legend.UnitType` replaces her in place --
  life, mana, XP and items carried over, her skill points come back to be spent anew; dead, she changes when revived.
  - `UZa1` The Banshee (caster): Possession, Anti-magic Shell, Banshee Wail; Ethereal Walk (no skill point).
  - `UZa2` The Dark Ranger (attacker): Black Arrow, Silence, Aimed Shot (the 24p Dark Ranger's set); ultimate Multi
    Shot at level 6; Trueshot Aura.
  - `UZa3` Queen of the Forsaken (both; the 24p boss model): Black Arrow, Possession, Banshee Wail; ultimate Banshee
    Queen (the 24p ultimate, now at level 6); Trueshot Aura, Ethereal Walk.
  - New hero versions of existing spells (4 levels, level 1 = the original): Possession `AZs1` (enemy units up to
    level 3/4/5/6), Anti-magic Shell `AZs2` (absorbs 300/450/600/750 magic damage), Banshee Wail `AZs3` (WL's
    4-level Howl of Terror), Ethereal Walk `AZs4` (6 s: +50% speed, ethereal, walks through units), Multi Shot
    `AZs5` (WL's, at level 6 instead of 10). Fixes the ultimate that could be learned before level 6.
- **Plague Research** (maintainer): the Forsaken's repeatable knowledge quest -- every 100 enemy units killed, +5
  knowledge. New building block `Timelines/Core/ObjectiveRepeatingKills` (a repeating objective for other factions).
- **Income for new starts** (maintainer): every real turning point (not the placeholders) gives 150 gold per turn for
  10 turns -- successors have no CPs at first.

## 0.55.1
Fixes from the 0.55.0 playtest. Version v0.55.1.
- "SYSTEM ERROR: level (1) cannot be higher than the object limit for Tier 4 (0)" (and Archivist Adept Training): the
  Forsaken's researches had no object limits -- WL records every finished research in the faction and refuses levels
  above the limit. `import_24p.py` now gives every research the set's buildings offer a limit (9 researches).
- Lumber is knowledge (the tome) in this map: the Forsaken's units, buildings and researches cost no lumber any more;
  Tier 4 (the Haunted Castle's upgrade) costs 25 knowledge (maintainer).

## 0.55.0
The Forsaken grow: the 24p tree, Sylvanas' phases, two quests; split fixes from the 0.54.1 playtest. Version v0.55.0.
- **Import tool** `tools/shattered-legacies/content/import_24p.py`: copies 24p objects into the main map with new
  `?Zf?` ids, follows their dependencies (abilities, upgrades, buffs, items; identical or same-named main objects are
  reused), copies their model files (multi-dot names renamed like WL #4190; textures when the models are real files),
  writes the faction's object limits and id constants and `Content/SL_OBJECTS.md`. First set: the **Forsaken** -- 91
  objects, 27 model files. Replaces the placeholder Scourge tech (it was locked behind the Scourge's quests).
- **Sylvanas' phases** (maintainer): 0 Ranger-General (Quel'thalas) -> 1 "The Banshee" -> 2 "The Dark Ranger" (level
  6; the Dark Ranger model) -> 3 "Queen of the Forsaken" (Lordaeron City taken). She is a WL legendary hero now.
- **Quests**: "Free Will" (Sylvanas level 6: the Dark Ranger, off the Scourge's team, unlocks the 24p tree's
  "Forsaken Independance" units) and "The Throne of Lordaeron" (Lordaeron City: the Undercity, Queen of the
  Forsaken, unlocks the Grand Apothecary).
- **Start** (maintainer): the Altar of the Queen preplaced and invulnerable in Northrend (-1340, 20171); 3 Acolytes,
  Sylvanas; **500 gold** -- the old faction keeps all its gold and lumber (no abuse by dying).
- **The free slot gets an Hourglass of Fate** (its faction's turning points; the bot uses them by code).
- Bot: production data for the Forsaken buildings (`SL_BUILDINGS`), their heroes.

## 0.54.1
Fix: the Forsaken betrayal did not happen (playtest of 0.54.0). Version v0.54.1.
- Researching "Betrayal: Forsaken" ended in "TICK ERROR in turning point: Failed to set the object level of Betrayal:
  Forsaken to 0; it is 1 instead" and the player stayed Quel'thalas. 0.54.0 cleared the faction's record of the
  decision research while the research was still allowed -- WL can only change a research level while its limit is
  0 (its own work-around raises it for a moment). Now: limit 0 first, then the record (and a failure there no longer
  stops the turning point). The same order as the Sunfury fallback that worked in 0.53.

## 0.54.0
The betrayal structure and its pilot: Quel'thalas -> the Forsaken. Version v0.54.0.
- **Triggers**: a turning point's trigger can be any WL quest objective (`TurningPointDef.MakeTrigger`); new
  `ObjectiveLegendKilledBy` ("Sylvanas Windrunner is slain by the Scourge"). `OnTrigger` effects run when the trigger
  fires, whatever is decided. A faction can have several turning points (Quel'thalas: Fallback: Sunfury and Betrayal:
  Forsaken, two buttons on its Hourglass). Quest titles by kind ("Fallback: ...", "Betrayal: ...").
- **New effects**: `SplitFaction` (one player becomes two: the old faction moves with army, quests, team and colour to
  the first free slot of P11 / P14 / P20 and the bot plays it -- new `AiApi.TakeOver`; gold and lumber shared half and
  half), `Spawn` (a start kit by code). After every turning point the player's Hourglass shows the new faction's
  turning points, and the defeat rule leaves him alone for 10 turns (`EliminationRule.GiveGrace`).
- **The fork's first own faction**: `ForsakenFaction` (lavender, the Scourge's team) with object limits in
  `src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/ForsakenObjectInfo.cs` (+ `[SL]` line in
  `ObjectInfoRepository`). Placeholder tech: Necropolis, Ziggurat, Altar of Darkness, Crypt, Acolyte, Ghoul, Banshee,
  Abomination.
- **Pilot: Quel'thalas' betrayal** (`Timelines/Factions/Forsaken/ForsakenBetrayal`): Sylvanas slain by the Scourge ->
  lost to Quel'thalas for good (WL's `PermanentlyKill`, no retraining); "Betrayal: Forsaken" at the Hourglass (bots
  always take it): Quel'thalas goes on in a free slot under the bot, the player becomes the Forsaken in Northrend
  near Drak'Tharon (2968, 17655) with Sylvanas the banshee (new hero `UZa1`), an Altar of Darkness and 3 Acolytes.
- Hourglass data: `RZdI` / `RZcI` (Forsaken), button positions per decision (`gen_hourglass.py`).

## 0.53.0
The turning-point core and its pilot. Version v0.53.0.
- **Turning points are WL quests** (`Timelines/Core/TurningPointQuest`, the Scarlet Crusade pattern): every faction
  with a turning point run by the timelines gets a quest "Fallback: <name>" in its quest log -- objectives: the
  trigger (`ObjectiveHqLost`: all HQ units lost) and the decision (WL's `ObjectiveResearch` at the Hourglass).
  Attached through `FactionManager.FactionRegistered`, no WL faction file changed. Bots decide at once as before.
- **Hourglass of Fate with researches** (`nZh0`, replaces the "Fallback" building `n0FB` and its ability `AFB1`):
  one decision research per turning point (`RZd1`-`RZdH`), each requiring a condition tech (`RZc1`-`RZcH`) that the
  trigger grants -- greyed out with "Requires: <trigger>" until then, "Not available yet" for turning points not
  built yet. Lordaeron's Hourglass also lists WL's Scarlet Crusade research. Every player sees only his own. All of it
  generated by the new `tools/shattered-legacies/timelines/gen_hourglass.py` (map data + `Core/HourglassData.g.cs`).
- **Effect library** `Timelines/Core/Effects/`: `SwapFaction` (keeps the player's colour), `LeaveBehind`,
  `StartArea`, `ChangeTeam`.
- **Defeat rule** (`Rules/EliminationRule`, `[SL]` hook in `PlayerElimination`): a player is eliminated only after
  more than 5 turns without any CP (1 CP is enough); food and essential legends no longer count. A faction with an
  unused fallback gets the fallback instead of the defeat. As in WL: humans only, from turn 20.
- **Pilot: Quel'thalas -> Sunfury** (`Timelines/Factions/Quelthalas/QuelthalasFallback`): when Silvermoon Palace
  is lost, Quel'thalas' units are removed and its CPs / capitals turn Neutral Hostile, the player becomes
  WL's Sunfury (its quests included), takes over Sunfury's start area, Kael'thas leads them, team Outland while it has
  a member. **The Illidari slot no longer offers Sunfury** (`[SL]` in `FactionChoiceDialogSetup`; no dialog, the bot's
  pick exclusion is gone).
- Kul Tiras -> Kezan Enterprise runs through the same quest (same effects as before).
- **ID rule corrected**: `Z` second AND a lowercase letter third (`nZh0`, `RZd1`) -- WL already has `RZ02`, `RZZL`,
  `AZ2A`, `AZ3A`.
- **Playtest fixes** (before release):
  - The fallback did not run: researching "Fallback: Sunfury" ended in "SYSTEM ERROR ... Failed to set the object
    level of Fallback: Sunfury to 0". WL records every finished research in the faction and resets those levels
    when the player changes faction; the decision research cannot go back to 0 inside the research event. The
    effects now run one frame later, after the decision research is taken back (`BlzDecPlayerTechResearched`) and
    hidden (limit 0). Quel'thalas' fallback swaps the faction first, so a failing swap can no longer cost the units.
  - Sunfury's start area was visible (and attackable) from the start: it is now hidden with WL's rescue preparation
    at map setup (`TurningPointDef.Prepare`, `StartArea.Prepare` / `Take`); the areas of Sunfury's quests (Tempest
    Keep, Area 52, Upper Netherstorm) are invulnerable and paused until those quests take them over.
  - Quel'thalas' HQ is Silvermoon Palace alone (maintainer) -- not also the Sunwell.
  - Second playtest: the scoreboard showed Sunfury in its own colour (the Illidari's) -- `Faction.SetPlayerColor`
    ([SL] in MacroTools), set before the swap. Sunfury's quests with a WL deadline ("Turn 10 hasn't started": Area 52,
    Eco-domes, Upper Netherstorm) only looked completed and would have failed at turn 10 -- their deadlines now count
    from the fallback (`ObjectiveExpire.ExpirationOffset`, [SL]). "Upper Netherstorm" could not be completed when
    another faction had killed the creeps before -- `ObjectiveHostilesInAreaAreDead` counted the corpses ([SL], a WL
    bug).
- **Build fix** (before release): `TimelineSystem` called `GetPlayerData()` without `using MacroTools.Extensions;`
  -- the publish step failed. New check `tools/shattered-legacies/ai/using_check.py` finds such calls.
- **Content index fixed**: preplaced units were read with the wrong byte order, so almost nothing counted as
  preplaced ("295 unused units" -- really 7, all summons of stock abilities). **Correction**: the Kezan Trading
  Center (`o04M`) is fine -- it is preplaced on Kezan and the Kul Tiras fallback takes it over.

## After 0.52.0 -- content index (tools and docs only, no new map build)
- **Content index** `Content/` (generated by the new `tools/shattered-legacies/content/content_index.py`): every
  object of the main map, the 24p map and stock WC3 (from `src/War3Api.Object/Generated/`, already in the repo) in
  four TSV files (units, abilities, upgrades, items) -- name, kind, race, model and where the model comes from,
  faction, what makes it, what it makes, abilities, requirements -- with a status: main / stock **used** or **unused**
  in the main map (reachable from WL's object limits, the preplaced units or the C# code), 24p **24p only** /
  **changed** / **different** (same id, another object). `Content/REUSE.md`: the recyclable units, heroes and
  buildings by race; `Content/README.md`: how to read and ask (`<map>:<id>`).
- Found with it: the **Kezan Trading Center (`o04M`) never appears** (nothing builds or places it); **Field Stockpile
  (`h086`) uses a model only the 24p map has**; checked: all 4046 main-map imports are on the fork's LFS server, so
  the invisible Kezan units are not a missing upload (`TODO.md`).
- **Removed** `tools/ai/out/route_map.png` and `route_map_small.png`: committed by accident in 0.52.0 (the folder's
  `.gitignore` had moved with the tools); generated images are never committed.
- Docs: `AI_AGENTS_CONTEXT.md` (both) with the current state; `README.md`, `TODO.md` (step 1 done, findings).

## 0.52.0
Structure for the timelines; no gameplay change. Version v0.52.0.
- **Fallbacks renamed to timelines** -- turning points of four kinds: fallback, sub path, betrayal, pick. The code is
  split the way new content will be added:
  - `Timelines/Core/`: `TurningPointDef` (was `FallbackDef`; the mode strings "wl" / "spawn" / "none" became
    `TurningPointSource`, the special-case string "kultiras" became the `Run` delegate), `TurningPointKind`,
    `TimelineSystem` (was `FallbackSystem`: HQ watch, run), `Hourglass` (the trigger buildings).
  - `Timelines/TimelineCatalog.cs` (was `FallbackConfig.All`), with each placeholder marked where the concepts
    superseded it.
  - `Timelines/Factions/Kultiras/KultirasFallback.cs` and `Timelines/Factions/KezanEnterprise/` (config + mechanics,
    was `KezanEnterprise.cs`).
  - `Bot/`: `SlBotSetup` (everything handed to the bot) and `SlAiData.g.cs`.
  Same behaviour; in-game texts unchanged (the building is still called "Fallback: ..."). Debug log labels:
  "fallbacks" -> "timelines", "FALLBACK" -> "TURNING POINT".
- **The bot holds no fork content any more**: the `-version` title and the rule "bots never pick Sunfury" are set
  from `Bot/SlBotSetup.cs` through `AiApi` (`../Ai/CHANGELOG.md` 0.52.0 -- with it, a bot Illidari slot is decided
  after 4 s instead of WL's 24-s timeout; same faction).
- **Tools**: `tools/ai/` moved to `tools/shattered-legacies/ai/`; the drawio generator added as
  `tools/shattered-legacies/timelines/timeline_overview_gen.py` (rewrites `Timelines/timeline-overview.drawio` in place,
  keeps the old draft page).
- **Docs**, separate for bot and fork: new `AI_AGENTS_CONTEXT.md` (start here: repository, way of working, delivery)
  and this `CHANGELOG.md`; `README.md` rewritten (layers, folder map, every `[SL]` change, files deleted on purpose,
  map data, ID rule `?Z??`, tools, releasing); `TODO.md` rewritten in the agreed order (content index, turning-point
  core with the Defeat rules, Kezan rebuilt, light turning point, Lordaeron pick, faction by faction, shared
  mechanics; the Thandol Treaty draft dropped -- the Great Alliance replaces it); `FALLBACKS.md` replaced by
  `Timelines/TIMELINES.md` (turning-point model, reused WL mechanics, faction template, status per slot);
  `Timelines/SCENARIO_CONCEPTS.md` and the drawio added and corrected (Gilneas -> Theramore, Exodar -> Maiev,
  Twilight's Hammer with Dragonmaw / 24p assets, Dwarven Expedition, Skywall / Black Empire fallbacks open, Forsaken =
  Quel'thalas' betrayal, Ebon Blade = Scourge's fallback, the defeat rule "no CP for more than 5 turns").
  `FACTION_UNITS.md` moved to `Timelines/` (column "Timeline").
- **`[SL]` markers** added where they were missing: `WarcraftLegacies.CLI/Settings/MapSettings.cs`,
  `DefaultOptionsFactory.cs`.
