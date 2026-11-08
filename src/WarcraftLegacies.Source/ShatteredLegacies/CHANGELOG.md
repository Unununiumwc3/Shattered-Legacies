# Changelog -- Shattered Legacies (fork content)

One version number for the whole map (`README.md`, "Map name and versions"). Bot changes are in
`../Ai/CHANGELOG.md`; up to 0.51.1 that file held the fork's changes too (fallbacks 0.39.1-0.39.10, Kezan Enterprise
0.39.5-0.39.8, map name 0.39.9, the extra CPs 0.39.4-0.46.2).

## 0.65.2 — bot construction playtest candidate

Worker placement and scheduling changes are documented in `../Ai/CHANGELOG.md`.

## 0.65.0 — first worker-construction candidate

Version v0.65.0. Bot changes are detailed in `../Ai/CHANGELOG.md`: native worker construction, no regular
instant starter grants, original-HQ and allied-CP recovery, and a cap of three total production bases.
The Argus scenario's special base remains, but no longer consumes Herald workers. Build and playtest required.
Debug commands intentionally unchanged until the 1.0 release gate.

## 0.64.0
The Ashtongue's purification reworked (maintainer, 0.63 playtest). Version v0.64.0.
- **Shattrath is the node**: every 8 s it draws up to 50 mana from each own / allied unit within 1500 (160 in all), a
  mana-burn beam to each (heights from the units -- 0.63's beams missed their targets); +2 per second by itself. Its
  mana pool (no regeneration) is the purification of the current step.
- **Order**: Shattrath, Halaa, Telredor, Oshu'gun, then the temple itself (= Karabor); 12,000 in all (0.63: 6,000)
  -> 10 minutes with perfect mana.
- **Effects** at the site being purified, only while mana flows: the Obsidian Statue's replenish (life + mana) at the
  bubble's size, a shield bubble (not at Shattrath; gone when done), a ring of 12 healing-light pillars.
- **Sites as tech buildings**: each site is a fel-tainted building (no buttons) until purified, then its purified
  building trains its units and researches its techs (Shattrath: Luminarchs, Vindicators, weapons, armour, the
  Luminarchs' training; Halaa: Elekk Knights; Telredor: Rangari; Oshu'gun: Elementalists + training). Quest markers on
  every site and the temple.
- **Halaa**: the original map's "Halaar" is renamed Halaa (and Shattrah -> Shattrath) in game; Halaa and Shattrath are
  conquered (WL's capital capture) instead of destroyed before the Ashtongue come; a site that is gone is built again.
- **Ishanah** cannot move (no move ability -- 0.63's leash let her slide) and cannot be hurt until Karabor is
  restored; then a free Ishanah (level, skills, items) without Purify.
- **Seers again**: the temple's card held 9 trains and an altar's buttons -- some were hidden. The temple trains
  Defenders, Deathsworn, Seers; the sites train the rest. The Draenei Hut works (its requirement, "Quest Completed:
  The Draenei", is granted).
- **Food**: 30 from the temple, +6 per Outland CP of the team (invisible supplies).
- **Karabor**: the faction is renamed when the final goal is reached (WL re-keys by `Faction.Name`; the bot knows both
  names).

## 0.63.2
The Illidari's Hourglass, found (maintainer: the whole building was missing). Version v0.63.2.
- **Cause**: WL's Illidari quest "Return to Outland" abandons the Broken Isles -- every building of the player
  outside the Black Temple area is removed (refunded), the Hourglass with them (`QuestBlackTemple.AbandonBrokenIsles`).
  The building existed until that quest completed. (0.63.1's late-pick fix stays: it was a real gap too.)
- **Fix**: the Hourglass type is registered `NeverDelete` (WL's code then leaves it, or hands it to Neutral Hostile
  instead), and a watch every 5 s gives a Hourglass that changed owner back to its player and places one that is gone
  again (`Hourglass.Watch`). The only other WL code that clears a player's units this way: none found (Domination kills
  mindless undead, Progenesis replaces workers).

## 0.63.1
Fixes from the 0.63 playtest (maintainer). Version v0.63.1.
- **The Illidari had no Hourglass** -- and their fallback trigger was never armed: the Hourglass and the arming of the
  HQ triggers ran once, 62 s into the game; a faction picked later (a slow pick in its dialog -- the Illidari slot has
  one) got neither. Now a faction registered after that moment is armed at once, and every later faction change gives
  the player an Hourglass if his faction has a turning point (`TimelineSystem.Attach`, `Hourglass.Late`). Only the
  Hourglass and the arming ran on a fixed timer -- the turning-point quests attach on registration, as before.
- **Units still said "Illidari"** when hovered: a slot that is not a human's carries the lobby's name (the faction,
  "Illidari", "Kul Tiras"); after a swap it now carries the new faction's (`SwapFaction.NameSlot`). Humans keep theirs.

## 0.63.0
The Illidari's fallback: the Ashtongue Deathsworn (maintainer's design). Version v0.63.0.
- **Trigger**: the Black Temple (`o00F`) destroyed. **Effects** (`Timelines/Factions/Illidari/IllidariFallback`): a
  new `AshtongueFaction`; Illidan gone for good; everything but the draenei removed / hostile; the **Temple of the
  Deathsworn** (the fork's type, the Black Temple's model, indestructible, the production centre; food 100) on the
  Black Temple's spot; Shattrath, Halaar, Oshu'gun and the 24p's Telredor theirs and indestructible; Akama kept
  (levels), revived or created; an announcement.
- **Conquest rule** (`Core/Conquest`, reusable -- for evil factions later): the Ashtongue belong to the team of whoever
  destroyed the Black Temple, then of whoever holds its CP; the old team's alliances go; the Ashtongue taking the CP
  themselves changes nothing.
- **Purification** (`AshtonguePurification`): Ishanah, High Priestess of the Aldor, rooted at Shattrath; Purify
  (autocast, on) draws up to 10 mana per second from each own / allied unit within 1500 (20 in all) + 2 by herself;
  Telredor 1000, Halaar 1250, Shattrath 1750, Oshu'gun 2000 -> 5 minutes with perfect mana. Each site: a quest and its
  units at the temple. **Restoring Karabor to the Light**: the Temple of Karabor, Xi'ri the naaru, Ishanah free.
- **Gold**: 10 per minute per Outland CP of their team (`AshtongueIncome`).
- **Data**: import set `ASHTONGUE` (23 objects, `?Zy?`: Defender, Rangari, Elekk Knight, Luminarch, Vindicator,
  Elementalist, Xi'ri, Ishanah, Telredor + dependencies; 3 model / icon files); hand-made `ashtongue_objects.py`
  (`oZt0` / `oZt1` temples, `AZt0` Purify, `RZt0-4` conditions).
- **Kezan**: Master of the Slave Trade has one objective per camp with its map marker (`Core/ObjectiveAt`); Kul Tiras'
  heroes are removed at the fallback instead of going to Stormwind (`LeaveBehind.RemoveHeroes`).
- **Tools**: `check_json.py` also flags a field written with another type than the map uses for it everywhere else
  -- it found the Oil Refinery's scale written as unreal (0.61-0.62; fixed, real now); `import_24p.py` creates a new
  faction's folder.

## 0.62.0
0.61 playtest (maintainer): the oil has to be carried, the refinery works from any distance, slave camps and
fallen factions fixed. Version v0.62.0.
- **Oil, carried** (lore: rigs alone deliver nothing): every Oil Platform pumps 625 oil per minute into its own tank
  (3,000) from a field of 40,000 -- dry and emptied, it collapses and a new rig is needed. **Oil Tankers** (the
  Trading Center's ships, was "Trader") carry 300 per trip from the platform with the most unclaimed oil to the
  Trading Center and into the Oil Supply; they never go to the goblin towns. **Trading Zeppelins** trade the gold.
  No oil without rigs and tankers (0.61 had 200 per minute from nowhere and 1,000 at the start). Maxed out (8
  platforms) 5,000 oil per minute: the refinery takes 1,000, the rest is fuel. Scoreboard: "Kezan oil: <oil>
  (rigs +<pumped>/min)".
- **Refine Oil works**: now Faerie Fire based (the Moon Well's Replenish needed the target in reach and mana) --
  cast it on the Oil Supply from any distance, or leave autocast on (on from the start): every 60 s (its cooldown)
  1,000 oil -> 100 / 150 / 200 gold. The refinery is half the Oil Supply's size.
- **The Trading Center and the Oil Supply are indestructible** (their data said so, WL's rescue took it away).
- **Slave camps**: 4x the regular training time; a unit trained when the stock is empty is refunded and removed (the
  limit alone let them be spammed); an unruled camp belongs to Neutral Extra, not Neutral Passive (WL removes / rescues
  Neutral Passive units -- the Venture Bay camp vanished), and a camp that is gone is placed again.
- **A faction falls when its HQ is lost** (the fallback trigger), not only when its fallback runs -- refugees come for
  every faction with an HQ (Druids, Skywall ...). The Druids need both Nordrassil and Cenarion Hold lost.
- **Skywall's HQ** was a wrong id (`nelc`, no such unit: its trigger never armed) -- now the Vortex Pinnacle (`nELC`)
  and the Sulfuron Spire (maintainer): both lost or not Skywall's.
- **A lost HQ stays lost** for its turning point (maintainer): retaking it no longer takes the fallback away (Kul
  Tiras' fallback came and went).

## 0.61.0
0.60 playtest (maintainer): the Kezan oil like the 24p, the Fuel Transport as a bomber, a crash and two 24p data bugs
found by the import tool, the slave camps reworked. Version v0.61.0.
- **Crash fixed: Gazlowe's Engineering Upgrade** swapped his skills for 24p-only abilities (`A0PT` ...) because the
  upgrade itself was used under its main-map id (same name) -> buttons without icons, the game crashed. `import_24p.py`
  now copies an ability whose 24p data names abilities the main map lacks (13 new copies for the Enterprise).
- **Alchemist / Wizard trainings** gave no spells: the Alchemist's Acid Bomb / Dispel (main ids) waited for the main
  map's `R07M`, another object. The tool now also copies an ability that names an object the set copies.
- **Oil like the 24p**: the preplaced Oil Supply fills up with oil (its mana): 200 per minute from its own wells, +100
  per Oil Platform (at most 8); 1,000 at the start. The **Oil Refinery** (new, the Oil Supply's model, replaces the 2
  extra Oil Supplies of the start kit) turns 1,000 oil into 100 gold every 60 s -- **Refine Oil**, an autocast that is
  on from the start (bots too). **Oil Refining** (2 levels, at the refinery): 150, then 200 gold; the second level
  behind Tier 4 (it asked for a human Castle, "Burg"). No more gold per Oil Supply / platform. Scoreboard: "Kezan oil:
  <oil> (+<rate>/min)".
- **Fuel Transport = suicide bomber**: Detonate (autocast, on when trained): an enemy within 300, or a manual cast ->
  500 + its fuel (mana) damage to everything within 1,000, allies too, a pillar of flame (`KezanEnterpriseBombers`).
- **Trading Zeppelins and Traders need an Oil Platform**; the **Trading Center and the Oil Platforms are
  indestructible**.
- **Refugee Slave Camps**: at the maintainer's spots (Gadgetzan, Booty Bay, Everlook, Fuselight, Venture Bay; Ratchet
  next to its CP); refugees at **half price**; **one entry per slot** -- Sentinels / The Exodar (Sentinels' archers),
  Frostwolf / Warsong, Dalaran / Gilneas: available when whichever faction plays the slot falls; **Black Empire at
  Gadgetzan**. **Master of the Slave Trade**: one sale at every camp (not every kind); +25 knowledge.
- Tools / docs: `content/README.md` -- the two new import rules, autocast toggles tracked by their orders.

## 0.60.0
0.59 playtest fixes, the Refugee Slave Camps, and the tools / docs for reusing the 24p map. Version v0.60.0.
- **Refugee Slave Camps** (maintainer's design; `KezanEnterpriseSlaveCamps`, data `kezan_objects.py`): six camps,
  one per goblin town (Booty Bay, Ratchet, Gadgetzan, Venture Bay, Fuselight, Everlook), placed next to the town's CP
  when the Enterprise arrives (prebuilt for now; how they get built one by one: `TODO.md`). A camp belongs to whoever
  holds the town's CP (that ruler trades with Kezan; neutral: nobody), so taking the CP takes the camp; the Enterprise
  holding it uses the refugees itself. Each camp sells the refugees of its region -- one unit per WL faction (21, a
  copy of a basic unit), greyed out "No refugees from X yet" until X falls (defeated, or its fallback). Stock 3, one
  back every 2 min. Every sale to another player pays the Enterprise the price. Stock Mercenary Camp model.
- **Quest "Master of the Slave Trade"** replaces Goblin Empire: every kind of refugee sold once -> Gallywix's title
  and the Intercontinental Artillery. **Western Expansion removed** (maintainer); War Zeppelins need only the Fortress
  and the Engineer's Guild.
- **Flight at the Kezan Lumber Mill** (WL's build makes every flying unit require Flight: the Oil Rigs and zeppelins
  were locked, 0.59 playtest).
- **Kezan's Builders** appear on open ground in the base (they were in a cliff pocket).
- **Sunfury** starts with the CPs Area 52, Farahlon and Netherstorm (maintainer; if neutral).
- **Forsaken**: the Altar of the Queen can be attacked from the Forsaken's arrival on (maintainer).
- **"Plague Bomb" showed no damage** (German: "Saeurebombe") -- not a translation problem: its 24p tooltips read
  their numbers from another 24p ability (`A03Z`) that the main map lacks. `import_24p.py` now remaps tooltip
  placeholders (also 5 other tooltips of the Forsaken and the Enterprise) and gives skills renamed only in their
  tooltips their own name and learn title.
- **Autopilot / bots trained Forsaken Acolytes as army** (`gen_ai_data.py` knew workers only by a few stock base ids;
  now also by `peon` in their type).
- Building blocks: `StartCps.Give` (CPs at an arrival), `TimelineSystem.FactionFell` (a faction falls: its fallback).
- Tools / docs: `tools/shattered-legacies/content/README.md` -- how to add a 24p faction and WL's rules a copied
  faction must follow (object limits or WL refunds the unit, Flight / Navigation, tooltips, workers, command card,
  LFS); `import_24p.py` deletes the files of objects dropped from a set (the id stays reserved). The TODOs are split:
  the fork's (`TODO.md`: the Kezan concept -- turtle / tower defence, buying time, trade towns) and the bot's
  (`../Ai/TODO.md`: bots that build with workers, the food cap under siege).

## 0.59.0
Kezan Enterprise, the rest (maintainer after the 0.58 playtest), and arriving factions on bot slots. Version v0.59.0.
- **The Trading Center from the 24p** (maintainer: needed for the oil part to work right): the preplaced one becomes
  the 24p's (`oZkj`). It trains **Trading Zeppelins** (at most 16) and **Traders** (ships, at most 10), which
  cannot be controlled and trade on their own (`KezanEnterpriseTrade`, the 24p's GUI triggers rebuilt): to a random
  goblin town -- Booty Bay, Ratchet, Gadgetzan, Venture Bay, Fuselight, Winterspring; ships to the town's harbour
  (found offline on the pathing map, all reachable by sea from Kezan) -- and back, paid by the distance (1 / 2 gold
  per 1,000 of round trip, a full fleet about +400 gold per minute). Gold only: 60 / 150 (the 24p's lumber dropped).
- **The creep market is gone** (`KezanEnterpriseMarket` removed): maintainer -- the pseudo mercenary centre comes
  another way (`TODO.md`).
- **Oil Refining** (the upgradeable refinery): 3 levels at the Oil Supply (150 / 250 / 350 gold), each +10 gold per
  minute per Oil Supply and +5 per Oil Platform. Hand-made (`tools/shattered-legacies/content/kezan_objects.py`,
  `RZko`).
- **The end goal on the scoreboard** (maintainer): "The Trade Prince's Fortune" -- hold 20,000 gold at once
  (`Core/ObjectiveHoldGold`). From the Enterprise's arrival the scoreboard shows, under the teams: the gold held /
  20,000 (then "reached"), the oil income per minute, the gold earned by trade. Reaching it is announced to everyone;
  what it does beyond that is open (the fork's win rule, `TODO.md`).
- **The scoreboard takes extra lines** (`[SL]` hook in WL's `FactionMultiboard.ExtraLines`; the fork's lines:
  `Rules/ScoreboardLines`, refreshed every 2 s).
- **A faction that arrives on a bot slot gets its production buildings** (Sunfury / the Enterprise on a bot trained
  nothing -- bots never build): 3 s after the turning point, one of each production building it lacks around its base
  and an altar if it has none, as every bot got at the game start (`AiApi.GiveStarterProduction`, bot).

## 0.58.0
Kul Tiras -> Kezan Enterprise rebuilt on the Forsaken's building blocks (`TODO.md` step 3). Version v0.58.0.
- **A faction of its own**: `Timelines/Factions/KezanEnterprise/KezanEnterpriseFaction` + generated object limits
  (`WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/KezanEnterpriseObjectInfo.cs`, one `[SL]` line in
  `ObjectInfoRepository`) instead of Kul Tiras renamed with limits set by hand (0.39.6).
- **The 24p goblin tree** (24p "Bilgewater"): a second import set in `import_24p.py` -- 67 objects with `?Zk?` ids, 7
  model / icon files. Great Hall / Stronghold / Fortress, Altar of Industry, War Camp, Lumber Mill, Laboratory,
  Engineer's Guild, Burrow, Missile Towers, Voodoo Lounge, Goblin Dock, Intercontinental Artillery; Builder, Gunner,
  Ogre Mercenary, Sappers, Wizard, Alchemist, Tinker, Shredder, Assault Tank, War Zeppelin, Fuel Transport; heroes
  Gallywix, Gazlowe, Noggenfogger. Changes to the 24p data: the Goblin Dock from the start (no "Business Expansion"
  -- Kezan is an island) with WL's Horde ships, Navigation at the Lumber Mill, no lumber costs (Tier 4: 25
  knowledge), the Fortress no longer upgrades into "Orgrimmar" (`o01B` in the 24p data), Oil Platforms can be
  attacked. The import tool: `force` (copy roots although the main map has WL's creep version under the same id),
  `extra` (main-map / hand-made objects in the limits, per set), `MainEquivalents` (creep id -> copy).
- **The fallback** (`Kultiras/KultirasFallback`): the base preplaced on Kezan (`Regions.KezanUnlock`) waits from the
  start (buildings visible and invulnerable, units hidden) and is taken over, WL's creep goblins replaced by the
  copies; Kezan's CP joins; start kit Altar of Industry, 2 Oil Supplies, 4 Builders; Katherine moved / revived;
  Kul Tiras' holdings to Stormwind only if Stormwind still plays on its team (inheritance rule step 2), otherwise
  removed / Neutral Hostile; own team with an explicit unally. The neutral shops outside the base (Goblin Merchant,
  Laboratory, Shipyard) are no longer taken (0.39.7 took everything in `KezanAmbient`).
- **Oil** (`KezanEnterpriseOil`, the 24p Oil Rig expedition): Oil Supplies are not buildable (+50 gold per minute
  each); an Oil Supply trains an "Oil Rig" -- 40-60 s later an Oil Platform rises at sea near it (pinged), +25 gold
  per minute, at most 8. Platforms unlock the Engineer's Guild and the Fuel Transport (24p data). The creep market
  stays (`KezanEnterpriseMarket`, was `KezanEnterpriseMechanics`).
- **Quests** (the 24p goblin quests with WL objectives): Explosive Engineering (starting quest; 4 Oil Rigs ->
  Gazlowe), Gadgetzan (its CP within 15 turns of the arrival -> Noggenfogger), Western Expansion (Feathermoon and
  Auberdine -> War Zeppelins), Lumber Market Crash (Nordrassil -> Shredders +500 hit points, +30 knowledge), Goblin
  Empire (7 goblin CPs -> the Intercontinental Artillery).
- New building blocks: `Core/ObjectiveCounter` (a count from code), `StartArea.Prepare(area, mode)` and
  `StartArea.Take(..., equivalents)`, `LeaveBehind.ToHeir`.

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
