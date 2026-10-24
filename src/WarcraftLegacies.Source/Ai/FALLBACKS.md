# Fallback scenarios -- design and work list

Big project of its own; comes AFTER the engine-load rework (see `TODO.md`). Everything fallback-related lives here;
`TODO.md` only points to this file.

## Idea

A race between the evils. Every faction -- good and evil -- has exactly ONE fallback, triggered when its **HQ** falls
(a CP, a building or an Old God, defined per faction below).
- **Good faction beaten** -> a small new start somewhere else that is a **roadblock for the OTHER evils** (never for
  its destroyer). Usually a relocation with a changed, reduced unit list -- sometimes a complete race change.
  Some fallbacks go west, so the Old Gods meet roadblocks too.
- **Evil faction beaten** -> its fallback faction **joins the team of whoever dealt the blow** -- the good side grows.
  Beaten by another evil -> the same faction, named "**Enslaved** <name>", joins the evil victor's team.
- Evils are slightly favoured; the finale is evil vs evil, and whoever is further ahead (more CPs) has the edge.

## Rules

- One fallback per faction; **its team changes at most once** (start team -> team after the fallback).
- Small start: 1 CP + a few units and/or gold; a reduced unit list, no full tech tree.
- **Bots** trigger their fallback at once. **Humans** may keep their old faction and trigger it later through an
  indestructible **trigger building in the unplayable area** (like grand melee maps), even with nothing else left.
- **Leaving humans are taken over by bots** (autopilot), so every fallback always has an owner.
- **One lore announcement per fallback** (row type "announcement" below), shown to everyone.
- Subjugation (beaten good faction becomes a vassal of its destroyer) is an exception tool for cases where a
  roadblock would feel forced. No faction uses it yet.
- **Abuse protection -- BACKLOG**, not for the next versions: a fallback must not be a free bonus (e.g. Lordaeron
  triggering the Scarlet Crusade early). Ideas: only when the HQ fell to an enemy; a minimum game time; a smaller
  start while the old base is still strong; nothing drastic like removing all old units.

## Teams

Teams are fixed **wrappers** (North Alliance, South Alliance, Kalimdor, Burning Legion, Outland, Old Gods). Factions
move between them; the wrappers stay (names may change later). Example: Ironforge falls -> the Explorers' League joins
the **North Alliance**. If every original North Alliance faction is gone, the dwarves may be the only member left.

## Good factions -> roadblocks

| Faction | Start team | Fallback | Where | Team after | Blocks | Lore |
|---|---|---|---|---|---|---|
| Dalaran | North Alliance | Theramore / Jaina's Sanctum | Kalimdor (Dustwallow) | Kalimdor | Old Gods; Legion in Kalimdor | Jaina leads the survivors west |
| Gilneas | North Alliance | Gilneans on Teldrassil / Darkshore | Kalimdor | Kalimdor | Old Gods; Legion in Kalimdor | Cataclysm: the night elves evacuate Gilneas |
| Quel'thalas | North Alliance | Sunfury | Outland (Netherstorm) | North Alliance (no change) | Illidan, Fel Horde | Kael leads the blood elves to Outland |
| Lordaeron | North Alliance | Scarlet Crusade (as today) | Lordaeron | North Alliance (no change) | as today | after the Third War |
| Stormwind | South Alliance | Valiance Expedition (hero fallback) | Northrend (Borean Tundra) | North Alliance | Scourge; Legion's Northrend portal | WotLK: Bolvar leads the Alliance in Northrend |
| Ironforge | South Alliance | Explorers' League | Northrend (Storm Peaks) | North Alliance | Scourge | WotLK: dwarves at Ulduar |
| Kul Tiras | South Alliance | Kezan Enterprise (pirates + goblins; built 0.39.6-0.39.8) | island (Kezan, west) | own team ("Kezan Enterprise Pact") | Old Gods in southern Kalimdor | sea powers |
| Druids | Kalimdor | Dream trees (Twilight Grove in Duskwood, Seradane in the Hinterlands) | Eastern Kingdoms | South Alliance | Fel Horde / Illidan; Scourge | the Emerald Dream's great trees |
| Sentinels | Kalimdor | Maiev's Watchers | Outland (Warden's Cage) | Kalimdor (no change) | Illidan | WC3 TFT: Maiev hunts Illidan |
| The Exodar | Kalimdor | Draenei of Shattrath (own base, NOT Illidan's Ashtongue) | Outland | Kalimdor (no change) | Fel Horde, Illidan | TBC: draenei in Outland |
| Orc slot (Frostwolf / Warsong) | Kalimdor | Dragonmaw | Eastern Kingdoms (Wetlands / Grim Batol) | South Alliance | Scourge (north), Fel Horde (south) | Day of the Dragon |

## Evil factions -> join the victor

| Faction | HQ (trigger) | Fallback | Where it starts | Team after | Lore |
|---|---|---|---|---|---|
| Scourge | Lich King / Frozen Throne | Forsaken | Tirisfal / Undercity | victor's team | Sylvanas breaks free and fights the Scourge |
| Legion | Archimonde / Argus hub | Knights of the Ebon Blade | Eastern Plaguelands (Acherus) | victor's team | the death knights fight the Legion beside every faction (thin link, accepted: least work) |
| Fel Horde | Hellfire / Magtheridon's line | Mag'har | Nagrand (Outland) | victor's team | uncorrupted orcs side with whoever freed them |
| Illidan | Black Temple / Illidan | Demon hunters | Shadowmoon (Outland) | victor's team | after Illidan's fall his demon hunters fight beside everyone |
| Skywall | Al'Akir / Vortex Pinnacle | Ramkahen Tol'vir | Uldum | victor's team | fought Al'Akir's servants beside both factions |
| Black Empire | N'Zoth | Titan Watchers (hero-only) | Ny'alotha / Uldum | victor's team | the keepers fight the Old Gods with any mortal ally |
| Ahn'qiraj | C'Thun | Brood of Nozdormu (hero-only) | Tanaris | victor's team | stood with everyone against the Qiraji |

Victor = owner of the unit that killed the HQ legend, or the new owner of the HQ CP / building (first event wins).

## "Sylvanas falls" (needed before Scourge -> Forsaken)

Quel'thalas' Ranger-General and the Forsaken's Banshee Queen must never exist at the same time, and she must become
undead even when Quel'thalas is winning. One event, first trigger wins: (1) the Scourge's Arthas meets her -- WL
already has this meeting as a dialogue (`ObjectiveLegendMeetsLegend`, Scourge); (2) the Scourge's HQ falls first;
(3) optional time limit, with a compensation for Quel'thalas. Quel'thalas loses her for good; a lore message for all.

## Generic remnant first, lore version later

Fallbacks with little reusable data start as a **generic remnant**: the SAME faction (its own unit tables), a small
base at the RIGHT spot (the place in the tables), the team change and the announcement. The lore version replaces it
later. Start generic: Mag'har (Fel Horde), Ramkahen (Skywall), Titan Watchers (Black Empire), Brood of Nozdormu
(Ahn'qiraj), Dream trees (Druids). The rest start with their lore version (enough exists).

## How a fallback base exists before it triggers

- **Preplaced, not swept**: the base is preplaced in the map data (Neutral Passive) inside a start rectangle and
  prepared with WL's rescue mechanism (`PrepareUnitsForRescue`), like Theramore: **structures visible and
  invulnerable** (indestructible; nobody can build on the spot), **units hidden**. The fallback hands everything over
  (`RescueGroup`). [ai] can preplace through the map-data JSON; [you] check the look in the editor.
- **Reused neutral buildings** get the same treatment (inside the rectangle -> invulnerable until the fallback).
- Check per rectangle: no other WL rescue / cleanup covers the same area (WL removes Neutral Passive units in some
  areas at runtime -- `CleanupNeutralPassiveUnits`). [ai]

## Trigger buildings for humans

- One per player slot, created at game start by code, owned by the slot's player -- the slot's CURRENT faction decides
  which fallback it starts. Bots never need it (they trigger by code). [ai]
- **Place: the sealed top-right corner** that WL already uses for dummy objects (`Regions.ArtifactDummyInstance`,
  19584..22688 x 23232..24800): no preplaced units, no path out (checked), inside the camera area up to y 24576.
  Proposed grid, 2 rows x 12: x = 19800 + 250 * column (0..11), y = 23500 (row 1) and 23950 (row 2); slot n -> row
  n / 12, column n % 12. [ai] proposed, [you] check in the editor.
- Unit type: a copy of an existing neutral building with one channel ability "Begin the fallback" (tooltip names the
  fallback, e.g. "Abandon Dalaran -- lead the survivors to Theramore"). Names and icon [you]; data [ai].

## Placement data (for the start rectangles)

CP ids: 4-char raw code (big-endian int in AiConfig). Owner 24 = Neutral Hostile, 27 = Neutral Passive (often already
part of a WL rescue -> conflict check), other numbers = a player slot at start.

| Fallback | Start spot | CPs it gets / quest CPs | Existing assets to reuse | Conflicts / checks | Who |
|---|---|---|---|---|---|
| Theramore | `Regions.Theramore` (exists) | Jaina's Sanctum `n0JS` (-7616,-9152), Alcaz Isle | WL's Theramore base | none | -- |
| Gilneans | Darkshore / Auberdine | Auberdine `n09D` (-16640,10048, owner 27), Darkshore `n02U` (-17600,6784); Teldrassil `n02Y` (-18432,15168, owner 27) | Worgen Manor `h03E` (copy; the original stands in Gilneas) | Auberdine / Teldrassil are owner 27 -> probably a Sentinels / Druids rescue | [ai] check, [both] base |
| Sunfury | `Regions.SunfuryStartingPosition` (1888..3616, -23424..-22048) | Netherstorm `n02O` (-64,-19840) | the whole Sunfury start (WL faction) | only possible once Sunfury is no longer an Illidari pick | [ai] |
| Scarlet Crusade | as today | as today | everything | none | -- |
| Valiance Expedition | Borean Tundra | Borean Tundra `n00G` (-7744,15680); quest: Dragonblight `n02Q` (-640,17152) | Stormwind buildings (copies) | the Nexus area clear runs there (fine) | [both] |
| Explorers' League | Storm Peaks | Storm Peaks `n02S` (3456,22976) | Explorer's Vault `n07U` (copy; two exist in Khaz Modan) | Yogg-Saron's prison is next door -- the League will hold the CP the Yogg quest needs (fine, lore) | [both] |
| Goblins of Kezan | `Regions.KezanUnlock` (-2400..608, -8000..-5184) -- **exists and is unused** | Kezan `n04Z` (-960,-7168); quest: Gadgetzan `n05C` (-8832,-13184) | Bilgewater Harbor `o03V` (-1632,-5408, owner 27) -- already inside the rectangle | none found | [ai] (almost ready) |
| Dream trees | Twilight Grove, Duskwood | Duskwood `n00V` (12160,-16384, owner 20 at start); quest: Hinterlands `n01A` (16256,4352) | druid buildings (copies) | Duskwood starts owned by slot 20 -- which faction? | [ai] check, [both] base |
| Watchers | Warden's Cage, Outland | Warden's Cage `md03` (5440,-31040, owner 27) | `Regions.MaievStartUnlock` (-6016..-4896, -28416..-27104) and Sentinels' Maiev Outland quest | MaievStartUnlock belongs to an existing Sentinels quest -> reuse or second rectangle | [ai] check |
| Shattrath draenei | Shattrath City | Shattrath `n09X` (-1344,-28672) | A'dal, vindicators (Draenei faction data) | the Draenei's own quests may already use Shattrath | [ai] check |
| Dragonmaw | Wetlands, next to Grim Batol | Wetlands `n016` (14080,-2240), Dragonmaw Port `n08T` (21312,-3200); quest target: Grim Batol `n03X` (17344,-3456) | Grim Batol is a **neutral WL capital** (`h01Z`, Twilight's Hammer creeps) + tunnels `n08R` -> quest "take Grim Batol", not the start | the capital must stay WL's | [both] |
| Forsaken | Undercity (under Lordaeron City) | Lordaeron City `n01G` (9088,8576, owner 27), Tirisfal `n01F` (6784,10112, owner 9 = Lordaeron) | 24p Forsaken buildings | Tirisfal / Brill belong to Lordaeron at start -> the Forsaken base must be its own small rectangle | [you] spot, [ai] data |
| Ebon Blade | Eastern Plaguelands (Acherus) | Havenshire `n04W` (20608,9344, owner 27), Corin's Crossing `n03P` (17536,9088, owner 9) | Scourge buildings (copies) | Havenshire is owner 27 (rescue?) | [you] spot, [ai] check |
| Mag'har | Nagrand (Garadar) | Nagrand `n00B` (-2944,-27904), Halaar `n0CV` (-4352,-29312) | orc buildings (copies) | none found | [both] |
| Demon hunters | Shadowmoon Valley | Shadowmoon `n09V` (2560,-31040, owner 27), Black Temple `n00I` (7168,-30144, owner 27) | Illidari buildings (copies) | both owner 27 -> probably Illidan's own rescue | [ai] check |
| Ramkahen | Lost City of the Tol'vir | Lost City `n0BK` (-12288,-19840, owner 8 = Skywall), Uldum `n0BD` (-14592,-17472) | -- | the Lost City is Skywall's own -> fine, the fallback IS Skywall's player | [both] |
| Titan Watchers | Uldum (hero-only) | Uldum `n0BD` | golem / construct models | -- | [both] |
| Brood of Nozdormu | Tanaris (Caverns of Time) | Tanaris `n020` (-10624,-15360) | dragon models (24p) | -- | [both] |

## HQ per faction (shared by fallbacks AND guardian summons)

One HQ definition per faction, in ONE code file (planned: `Ai/Fallbacks/HqConfig.cs`) that both the fallback logic and
the guardian summons read -- so they can never disagree. Candidates come from WL: legends marked **Essential** (WL
eliminates a faction when ALL of them are gone). Proposal: HQ = the first one listed; "HQ falls" = that one dies /
changes owner. **Input needed** per faction: confirm or pick another (several factions have more than one).

| Faction | WL essential legends (elimination when all are gone) | Proposed HQ | Input needed |
|---|---|---|---|
| Lordaeron | Capital Palace `h000`, Stratholme Castle `h01G`, Tyr's Hand Citadel `h030` | Capital Palace | which one / all? |
| Dalaran | The Violet Citadel `h002` | Violet Citadel (WL's Theramore trigger) | -- |
| Quel'thalas | Silvermoon Palace `h003`, The Sunwell `n001` | Silvermoon Palace | Sunwell instead? |
| Gilneas | Gilneas Castle `h04I` | Gilneas Castle | -- |
| Stormwind | Stormwind Keep `h00X` | Stormwind Keep | -- |
| Ironforge | The Great Forge `h001` | The Great Forge (0.39.1 placeholder uses the Ironforge CP) | switch to the Great Forge? |
| Kul Tiras | Boralus Keep `h046` | Boralus Keep | -- |
| Druids | Nordrassil `n002`, Cenarion Hold `n06D` (+1 more) | Nordrassil | which one? |
| Sentinels | Auberdine `e00J`, Feathermoon Stronghold `e00M` | Auberdine | which one? |
| The Exodar | Exodar Regalis `e01X` | Exodar Regalis | -- |
| Frostwolf | Thunder Bluff `o00J`, Darkspear Hold `o02D` | Thunder Bluff | which one? |
| Warsong | Stonemaul Keep `o004`, Orgrimmar `o01B` | Orgrimmar | which one? |
| Scourge | Frozen Throne `u000` | Frozen Throne | -- |
| Legion | Legion Stronghold `u00G` | Legion Stronghold | Archimonde instead? |
| Fel Horde | Kil'sorrow Fortress `o017`, Black Temple `o00F`, Hellfire Citadel `o008` | Hellfire Citadel | which one? |
| Illidan | (none) | ? | **pick one** |
| Skywall | Vortex Pinnacle `nelc` | Vortex Pinnacle | -- |
| Black Empire | N'Zoth `U01Z` (Old God) | N'Zoth | -- |
| Ahn'qiraj | C'Thun `U00R` (Old God) | C'Thun | -- |
| Sunfury | The Well of Eternity `n0DZ` | (Sunfury is itself a fallback -- no second one) | -- |
| Scarlet Crusade | (none) | (is itself a fallback) | -- |

## Lighter fallbacks: same faction, changed units (instead of a full race change)

Answers to the maintainer's questions -- this keeps the scope small:
- **Removing units / buildings: easy.** WL's object limits per player (`SetPlayerTechMaxAllowed` = 0) -> e.g. the
  Druids lose Mountain Giants. No data copies.
- **Adding units: possible when a building of the player trains them.** Train lists belong to the building TYPE, so a
  new unit needs a building that lists it (give the player such a building, or a copy with a changed list).
- **Changing existing units without copying the unit tree: yes, per trained unit.** On "training finished" the code
  can set name, HP, damage, armour, attack speed of that unit (`BlzSetUnitName`, `BlzSetUnitMaxHP`,
  `BlzSetUnitBaseDamage`, ...): "Archer" -> "Watcher Archer" with +20 % damage, or a general multiplier for all units
  of a fallback faction. Limit: the build menu (button name, tooltip, cost) still shows the original -- WC3 has no
  per-player text for unit types. Upgrades / researches can also be given or taken per player.
- **Renaming the whole faction: WL's own way** -- the player gets a new `Faction` object (that is how Lordaeron
  becomes the Scarlet Crusade: `GetPlayerData().Faction = scarlet`). A "light" fallback faction = a small faction
  class (name, colour, icon, object limits); its units come from the old faction's buildings. The AI's data is keyed
  by faction name, so each new faction needs its entries (the generated tables cover units).
- **Decision table**: `FACTION_UNITS.md` (generated by `python3 tools/ai/unit_scan.py --md`) lists every unit and hero
  per faction with a "Fallback" column -- mark keep / lose there.

## Details per fallback faction

Types: **hq** (trigger), **base**, **announcement**, **hero**, **unit**, **building**, **quest**, **event**.
Support: **WL logic** = code exists; **main** / **24p** = object data exists in that map (can be renamed /
rebalanced); **new** = needs new data (reuse models where possible). Quests are kept simple.
Status: **agreed** (maintainer decided) / **proposal**.
Who: **[ai]** = code, map-data JSON (copies, renames, preplacement), lookups -- the AI can do it;
**[you]** = needs the maintainer (final texts, models, visual check in the editor, balance);
**[both]** = AI prepares (data / coordinates), maintainer checks and tunes. Default per type: hq, quest, event [ai];
base, hero, unit, building [both]; announcement [you] (AI drafts).

| Faction | Type | Description | Support | Status | Who |
|---|---|---|---|---|---|
| **Theramore** (Dalaran) | hq | the Violet Citadel falls | WL logic (QuestTheramore) | agreed | -- |
|  | base | Theramore base, hidden until the fallback | WL logic (`Regions.Theramore` rescue) | agreed | -- |
| | hero | Jaina Proudmoore stays with the faction | main (Dalaran legend) | agreed | [both] |
|  | announcement | Theramore's reward / penalty texts ("Jaina Proudmoore abandons the once mighty city of Dalaran ...") | WL logic (QuestTheramore) | agreed | -- |
| | quest | "Jaina's Sanctum": hold Jaina's Sanctum and Alcaz Isle -> unlock Kul Tiran marines from Theramore's harbour. Today Dalaran gets NO quest after the move -- this would be the first | new (CPs exist) | proposal | [ai] |
| **Gilneans** (Gilneas) | hq | Gilneas Castle / Gilneas City falls | main (Gilneas Castle, Gilneas City CP) | proposal | [ai] |
| | base | refugee camp on Teldrassil / Darkshore (Worgen Manor, Greymane Manor) | main + 24p buildings | proposal | [both] |
| | hero | Genn Greymane, Darius Crowley, Tess Greymane | main | proposal | [both] |
| | unit | Worgen, Worgen Blood Shaman (Acwulf) | 24p | proposal | [both] |
| | quest | "A New Home": Genn reaches Darnassus -> night elf priestesses join (Priestess, Priestess of the Moon) | new (units exist in main) | proposal | [ai] |
| | announcement | "Gilneas has fallen. The night elves' ships carry the survivors to the shores of Teldrassil." | new | proposal | [you] |
| **Sunfury** (Quel'thalas) | hq | Silvermoon / the Sunwell falls | WL capitals exist | proposal | [ai] |
|  | base | Sunfury's own start in Netherstorm | WL logic (Sunfury faction) | agreed | -- |
| | hero | Kael'thas, Pathaleon the Calculator | main | agreed | [both] |
|  | quest | Sunfury's own quests: "Area 52", "The High Astromancer", "Eco-domes", "Upper Netherstorm" | WL logic | agreed | -- |
| | announcement | "Quel'thalas is lost. Prince Kael'thas gathers the last of his people and leads them through the Dark Portal." | new | proposal | [you] |
| | pick | Sunfury is removed as the Illidari slot's alternative pick -- it only comes as Quel'thalas' fallback (WL change in `Setup/FactionChoiceDialogSetup.cs`, ships with the fallback system) | WL change | agreed | [ai] |
| **Scarlet Crusade** (Lordaeron) | everything | faction, trigger, quests, units, heroes (the Mograines) and texts already exist -- nothing to build | WL logic | agreed | -- |
| **Valiance Expedition** (Stormwind) | hq | Stormwind Keep falls | WL capital exists | proposal | [ai] |
| | base | Valiance Keep, Borean Tundra (small: keep + barracks, reused Stormwind buildings) | main (reuse) | proposal | [both] |
| | hero | Bolvar Fordragon; Grand Marshal Garithos as a second hero | main | agreed (Bolvar) | [both] |
| | unit | Veteran Footman, Marshal, Rear-Marshal | main | proposal | [both] |
| | quest | "The Wrathgate": take Dragonblight -> unlock knights | new (CP exists) | proposal | [ai] |
| | quest | "The Frozen Throne": once the Scourge's Lich King is beaten, Bolvar takes up the Helm of Domination -> he becomes the Lich King hero (reuse the Scourge's Lich King / Arthas hero data) | main (copy) | proposal (maintainer idea) | [ai] |
| | announcement | "Stormwind has fallen. Bolvar Fordragon leads the Valiance Expedition north, to strike at the Lich King himself." | new | proposal | [you] |
| **Explorers' League** (Ironforge) | hq | Ironforge (the Great Forge) falls | WL capital exists | proposal | [ai] |
| | base | dig camp in the Storm Peaks (Frosthold), Explorer's Vault | main (Explorer's Vault) + reuse dwarf buildings | proposal | [both] |
| | hero | the League's leader | 24p | proposal | [both] |
| | unit | riflemen, mortar teams, gyrocopters (Ironforge units, reduced list) | main | proposal | [both] |
| | quest | "Secrets of Ulduar": a hero reaches Ulduar -> an artifact / titan relic. Template: Ironforge's "Explorer's League Foundation" / "Kalimdor Expedition" | WL logic (templates) | proposal | [ai] |
| | announcement | "Ironforge has fallen. The Explorers' League turns its back on the ruins and digs for hope in the frozen north." | new | proposal | [you] |
| **Goblins of Kezan** (Kul Tiras) | hq | Boralus Keep falls | WL capital exists | proposal | [ai] |
| | base | Kezan: Bilgewater Harbor, Venture Bay | main | agreed | [both] |
| | hero | Trade Prince Gallywix | main | agreed | [both] |
| | unit | Sappers, Trade Prince Guard, War Zeppelin, Goblin Privateer (+ 24p: sapper, trading zeppelin) | main + 24p | proposal | [both] |
| | building | Goblin Boulder Tower | 24p | proposal | [both] |
| | quest | "Kaja'mite": hold Kezan and Gadgetzan -> extra gold income | new (CPs exist) | proposal | [ai] |
| | announcement | "Kul Tiras has fallen. Its surviving captains sell their fleet to the Bilgewater Cartel -- Kezan opens for business." | new | proposal | [you] |
| **Dream trees** (Druids) | hq | Nordrassil falls | WL capital exists | proposal | [ai] |
| | base | Twilight Grove (Duskwood); Seradane (Hinterlands) through the quest | new base, reuse druid buildings | agreed (dream trees) | [both] |
| | hero | Malfurion Stormrage; Fandral Staghelm as a second hero | main + 24p | proposal | [both] |
| | unit | reduced list: Keeper of the Grove, Druid of the Scythe, dryads, treants | main | proposal | [both] |
| | quest | "The Dream Trees": hold Twilight Grove and Seradane -> unlock the green dragons of the Dream | new | proposal | [ai] |
| | announcement | "Nordrassil is lost. The druids walk the Emerald Dream to its great trees in the Eastern Kingdoms." | new | proposal | [you] |
| **Watchers** (Sentinels) | hq | the Sentinels' capital falls | WL capital exists | proposal | [ai] |
| | base | Warden's Cage, Vault of the Wardens (Outland) | main | agreed | [both] |
| | hero | Maiev Shadowsong, Naisha | main | agreed | [both] |
| | unit / building | Warden, Watcher, Watcher's Bastion, Altar of Watchers | main | agreed | [both] |
| | quest | reuse Maiev's Outland / Vault of the Wardens quests (the Vault channel area is at -2272,4000; 0.39 sends Maiev there as a hero visit) | WL logic (Sentinels quests) | proposal | [ai] |
| | hero requirement | Maiev comes from "Shores of Feathermoon" (3 buildings built in Feathermoon + the CP). If the Sentinels fall before it is done, the fallback completes it, so the Watchers always have Maiev | WL logic (quest) + [ai] | proposal | [ai] |
| | announcement | "Maiev Shadowsong leads her Watchers into Outland -- the Betrayer will not escape." If Illidan is already beaten: "Time to hunt Illidan -- wait, where is he?" | new | proposal | [you] |
| **Shattrath draenei** (Exodar) | hq | the Exodar falls | WL capital exists | proposal | [ai] |
| | base | Shattrath City | main (CP) | agreed | [both] |
| | hero | A'dal (the Naaru), Vindicator Maraad | main | agreed | [both] |
| | unit | Vindicator + reduced draenei list | main | proposal | [both] |
| | quest | reuse "Reconquering Tempest Keep" / "Crown of the Triumvirate" | WL logic (Draenei quests) | proposal | [ai] |
| | announcement | "The Exodar has fallen. Its people return to the world they fled -- the Naaru welcome them in Shattrath." | new | proposal | [you] |
| **Dragonmaw** (orc slot) | hq | Orgrimmar falls | WL capital exists | proposal | [ai] |
| | base | Grim Batol, Grim Batol Tunnels, Dragonmaw Port | main (CPs) | agreed | [both] |
| | hero | Dragonmaw chieftain; Zuluhed, Nek'rosh | main + 24p | agreed | [both] |
| | unit | Dragon Whelp, Blackrock Dragonkin, Ancient Red Dragon | 24p | proposal | [both] |
| | building | Dragon Hatchery, Red Dragon Hatchery, Dragonmaw Fortification | main + 24p | proposal | [both] |
| | quest | "The Demon Soul": hold Grim Batol -> unlock the Ancient Red Dragon | new | proposal | [ai] |
| | announcement | "The Horde of Kalimdor is broken. The Dragonmaw rise in Grim Batol, with red dragons in their chains." | new | proposal | [you] |
| **Forsaken** (Scourge) | hq | the Lich King / Frozen Throne falls | WL logic (Frozen Throne) | agreed | -- |
| | event | "Sylvanas falls" (see above) | new (meeting dialogue exists) | agreed | [ai] |
| | hero | Sylvanas, the Banshee Queen (needs another model), Grand Apothecary Putress, Master of the Royal Apothecary Society | 24p | agreed | [both] |
| | unit | Dark Ranger, Elder Banshee, Rotguard, Dreadguard, Plague Construct | 24p | proposal | [both] |
| | building | Forsaken Den, Plaguetorium, Plague Tower | 24p | proposal | [both] |
| | quest | "The Throne of Lordaeron": take Lordaeron City -> Undercity opens below it | new (CP exists) | proposal | [ai] |
| | announcement | "The Lich King's grip fails. Sylvanas Windrunner and her Forsaken claim the ruins of Lordaeron for themselves." | new | proposal | [you] |
| **Ebon Blade** (Legion) | hq | Archimonde / the Argus hub falls | WL legend / CP exist | proposal | [ai] |
| | hero | Darion Mograine = a copied death knight hero | main (copy) | agreed | [both] |
| | unit | Death Knight, Orcish / Fel Death Knight (copies, no new assets) | main (copies) | agreed | [both] |
| | building | Acherus = copy of an existing Scourge building | main (copy) | proposal | [both] |
| | quest | "The Light of Dawn": take Havenshire / Light's Hope -> unlock ghoul copies | new (CPs exist) | proposal | [ai] |
| | announcement | "With the Legion broken, the Knights of the Ebon Blade ride out of Acherus -- they serve whoever holds the line." | new | proposal | [you] |
| **Mag'har** (Fel Horde) | hq | Hellfire Citadel / Magtheridon's line falls | WL capital exists | proposal | [ai] |
| | base | Garadar, Nagrand | main (Nagrand CP) | proposal | [both] |
| | hero | Greatmother Geyah (copy of an orc caster hero) | new (copy) | proposal | [both] |
| | unit | Warsong Grunt copies (un-corrupted), Kodo War Beast | main + 24p | proposal | [both] |
| | quest | "The Ring of Blood": hold Nagrand and Halaa -> unlock Nagrand riders | new (CPs exist) | proposal | [ai] |
| | announcement | "The Fel Horde is broken. In Nagrand, the Mag'har -- untouched by the blood -- offer their axes to the victors." | new | proposal | [you] |
| **Demon hunters** (Illidan) | hq | the Black Temple / Illidan falls | WL legend / capital exist | proposal | [ai] |
| | hero | Altruis; Kayn Sunfury (copy) | 24p | agreed | [both] |
| | unit | Demon Hunter | 24p | agreed | [both] |
| | quest | "The Fel Hammer": kill one demon lord (any Legion hero) -> unlock a second hero | new | proposal | [ai] |
| | announcement | "Illidan has fallen. His demon hunters walk out of the Black Temple -- the Legion is still their enemy." | new | proposal | [you] |
| **Ramkahen** (Skywall) | hq | Al'Akir / the Vortex Pinnacle falls | WL legend / CP exist | proposal | [ai] |
| | base | Lost City of the Tol'vir, Tol'vir Statue | main | proposal | [both] |
| | hero | King Phaoris (copy) | new (copy) | proposal | [both] |
| | unit | Tol'vir warriors (copies of existing units) | new (copy) | proposal | [both] |
| | quest | "Uldum's Keepers": hold Uldum and the Lost City | new (CPs exist) | proposal | [ai] |
| | announcement | "Al'Akir's storm is broken. The Ramkahen of Uldum stand with those who freed the skies." | new | proposal | [you] |
| **Titan Watchers** (Black Empire) | hq | N'Zoth dies | WL legend exists | proposal | [ai] |
| | hero (hero-only) | two keepers -- copies of golem / construct models | main (Earth Golem, Titan Construct, Crystal Golem models) | proposal | [both] |
| | quest | "The Halls of Origination": a keeper reaches Uldum -> a third keeper | new | proposal | [ai] |
| | announcement | "N'Zoth is slain. The titans' keepers awaken to guard what remains." | new | proposal | [you] |
| **Brood of Nozdormu** (Ahn'qiraj) | hq | C'Thun dies | WL legend exists | proposal | [ai] |
| | hero (hero-only) | Anachronos + one bronze dragon (copies of dragon models, recoloured) | 24p dragon models | proposal | [both] |
| | quest | "The Scepter of the Shifting Sands": hold Silithus and Tanaris -> a second bronze dragon | new (template: Sentinels' "Scepter of the Queen") | proposal | [ai] |
| | announcement | "C'Thun is dead. The bronze dragonflight steps out of time to aid the victors." | new | proposal | [you] |

## Scope and direction

Why: in maps like this, players who lose early leave, and the game dies for the rest. Fallbacks give every beaten
player a second start; bots fill the slots of leavers -- so the game stays alive for the remaining humans. The bots
become a mechanic of the map, not just opponents. Balance is not the goal (it wasn't in the original map either).

Risks and how to keep them small:
- **Scope**: 18 fallbacks x (base, heroes, units, quests, texts) is bigger than all AI work so far, and the bottleneck
  is map data, not code. -> Build the system once, data-driven (one definition per fallback: HQ, start rect, team
  after, unit list, announcement); start with a **generic remnant** for every faction (same faction, small base at its
  fallback place, team change) and replace it with the rich lore version faction by faction.
- **Distance from upstream WL**: every WL update has to be merged. -> Keep fallbacks a separate layer like `Ai/`
  (own folder, minimal hooks), reuse WL's rescue and quest mechanisms instead of changing them.
- **Game length and load**: fallbacks keep factions alive and add CPs -> longer games, more units late. -> After the
  engine-load rework; small starts; later maybe adjust the victory condition.
- **Understanding**: team changes confuse players. -> One announcement per fallback, the team wrappers on the
  scoreboard, a ping at the new start.
- **Bots**: cheap, thanks to the table-driven production (0.38) -- a new faction mostly needs its data generated and
  a home-continent switch.

## Implemented so far (up to 0.39.9)

- Folder `Ai/Fallbacks/`: `FallbackConfig.cs` (one definition per faction), `FallbackSystem.cs` (trigger buildings,
  HQ watch, run). Map data: `n0FB` trigger building, `AFB1` "Begin the Fallback".
- Trigger buildings in the corner grid (row = slot / 12, column = slot % 12), created at 62 s for every slot whose
  faction has a definition; bots run the fallback by code.
- Dalaran and Lordaeron: listed as WL-driven (no ability). Ironforge: **placeholder** -- Ironforge CP lost (after
  having held it) -> Marin Noggenfogger (`VH01`) in the Storm Peaks + announcement.
- 0.39.2: bots take over leaving humans (so every fallback keeps an owner).
- 0.39.4: **every faction has a trigger building**; every good faction has a **placeholder** fallback (Marin
  Noggenfogger at the planned place); evil factions: building only. HQs chosen by the AI from WL's essential legends
  (table above): where several are listed, ALL must be lost; holding one again makes the fallback unavailable again.
  Ironforge's HQ is the Great Forge. Illidan has no essential legend -> no HQ yet.
- 0.39.5: symmetric trigger-building layout (team columns); **Kul Tiras fallback** implemented as designed by the
  maintainer (Katherine to Kezan, old army to Stormwind or removed, Kezan + local creeps, no allies, own text).
- 0.39.6: **Kul Tiras -> The Kezan Enterprise** (pirates + goblins). Data mined from both maps:

  | Kind | Name | Code | Source | Used |
  |---|---|---|---|---|
  | hero | Katherine Proudmoore (Lady of House Proudmoore) | `H05L` | main (Kul Tiras) | kept |
  | hero | Trade Prince of the Bilgewater Cartel (Gallywix) | `O04N` | main, no faction; Altar of Industry | yes |
  | hero | Tinker / Marin Noggenfogger | `Ntin` / `VH01` | main; Altar of Industry | trainable |
  | building | goblin hall (trains Tinker, Builder) | `o03L` | main, no faction | start |
  | building | Altar of Industry | `o03O` | main, no faction | start |
  | building | Tank Factory (Grenadier, Personal Tank, War Zeppelin) | `o05T` | main, no faction | start |
  | building | Kezan Oil Supply (farm) | `h04Z` | main, no faction | start x2 |
  | building | Garrison (Marines), Workshop (Cannons) | `h06R`, `h07P` | main (Kul Tiras) | start (human share) |
  | building | Bilgewater Harbor (ships, oil rig) | `o03V` | main, preplaced on Kezan | via Kezan |
  | unit | Sappers, Trade Prince Guard, Ogre Mercenary | `h08X`, `h090`, `n099` | main, no trainer | later (needs a building) |
  | building / unit | Mercenary Den, Kezan Trading Center, Goblin Boulder Tower, Trading Zeppelin | `o03P`, `o04M`, `n0AJ`, `nzep` | main / 24p | later |
  | unit / hero | Defias Assassin, Defias Kingpin, Corsair | `h0AI`, `E01H`, `udes` | 24p | later (pirate flavour) |

  Open [you]: buildings / tooltips in the editor; whether the Mercenary Den should train Sappers / Ogre Mercenaries /
  Defias (pirates); Bilgewater Harbor ships.
- TODO (not now): **fallbacks as quests** -- one quest per fallback in the quest log; its minimap marker only appears
  when the fallback is ready. TODO: Kul Tiras mercenary units from the 24p data (pirate / goblin units).
- Missing: team change, hidden start bases (rescue rectangles), unit packages, the real factions, "Enslaved".
- **Must-have (found in the 520-min test)**: WL's defeat makes the player an **observer** (`Faction.Defeat()` ->
  `SetState(Observer)`) -- the player loses everything and could never use a fallback. The fallback must run BEFORE
  WL's defeat: a hook in `Faction.Defeat` (or where WL eliminates a faction) that runs an available fallback instead.
  Same for bots: an autopilot / bot that is defeated becomes an observer too.
- Related concept in TODO.md: "Guardian summons" / HQ last stand.

## Work order

1. **System** [ai]: HQ per faction, trigger buildings, hidden start bases (WL's rescue mechanism), the team switch (once),
   "Enslaved" naming, announcements, bots taking over leaving humans, bot support (home continent / edge set).
   Plus the WL change: Sunfury no longer an Illidari alternative pick (only Quel'thalas' fallback).
2. **Prototypes** [both]: one good and one evil fallback -- orc slot -> Dragonmaw (roadblock) and Scourge -> Forsaken (team
   switch, needs "Sylvanas falls").
3. [both] Fallbacks that mostly reuse what exists: Theramore, Scarlet Crusade, Sunfury, Watchers, Shattrath draenei,
   Goblins, Explorers' League, Valiance Expedition, Ebon Blade, demon hunters, Gilneans.
4. [both] Generic remnants first, lore versions later: Mag'har, Ramkahen, Titan Watchers, Brood of Nozdormu, the Dream trees' base.
