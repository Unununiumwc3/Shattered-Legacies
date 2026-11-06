# Content tools -- reusing the 24p map (and making objects by hand)

How a faction of the 24p map (`mapdata/WarcraftLegacies24p`, the vJASS repository
`AzerothWarsLR/WarcraftLegacies24p` for its quests) becomes a faction of this map. Done twice: the Forsaken (0.55,
set `FORSAKEN`) and the Kezan Enterprise (0.58, set `KEZAN`, the 24p goblins). Every rule below cost a playtest once.

## The tools

| tool | writes | when |
| --- | --- | --- |
| `import_24p.py` | the copies (`?Z<third>?` ids, `import_ids.json` keeps them stable), their model / icon files, `<Faction>Objects.g.cs` (constants + `MainEquivalents`), `WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/<Faction>ObjectInfo.cs`, `Content/SL_OBJECTS.md` | after every change of a set; again after `git lfs pull` (textures) |
| `forsaken_heroes.py`, `kezan_objects.py` | hand-made objects a 24p faction did not have (Sylvanas' phases; Oil Refining, the Refugee Slave Camps) | after changing their tables; rows are appended, never reordered (a row's index is its id) |
| `check_json.py` | nothing -- checks the type of every value in the map's JSON (a string without `"Type": 3` broke the build, 0.57) | before every delivery |
| `content_index.py` | `ShatteredLegacies/Content/` (what exists in the main map, the 24p map, stock WC3) | after map data changes |
| `../ai/gen_ai_data.py` | the bot's production data (`SL_BUILDINGS`: the fork's buildings) | after a set's buildings change |

## A new import set, step by step

1. **Find the faction** in `Content/units.tsv` (`grep -i goblin`; column `faction`), its quests in the vJASS repo
   (`jass/Quests/<Faction>/`, `jass/Setup/FactionSetup/<Faction>Setup.j` for its limits).
2. **Roots** (`"objects"`): every building, unit, hero and research the faction trains / builds -- name, categories
   (the first decides `ReplaceWithFactionEquivalent` and the bot), limit. Researches with `None` categories: quest
   rewards (granted by a quest's `ResearchId`).
3. **`"third"`**: a lowercase letter no set uses (Forsaken `f`, Kezan `k`; hand-made: `a`, `s`, `q`, `r`; the tool falls
   back to `g`-`p` when a letter is full -- Kezan ran out of `AZk?` in 0.61 and went on with `AZg?`).
4. **`"force": True`** when the main map keeps WL's CREEP version under the same 24p ids (the goblins: Area 52,
   Kezan's decoration) -- otherwise "same name" objects are not copied and the faction would get WL's creep data.
   `MainEquivalents` then maps creep id -> copy, so `StartArea.Take(..., <Faction>Objects.MainEquivalents)` turns a
   preplaced creep base into the faction's.
5. **`"exclude"` / `"drop"` / `"set"`**: leave out what the map has otherwise (ships, a 24p "Orgrimmar" upgrade),
   drop references (`(id, field, token)`), set fields (`=XXXX` = a main-map id, kept as it is). The closure is computed
   from the ORIGINAL 24p data: a replaced list still pulls its old entries in -- exclude them.
6. **`"zero_lumber": True`** (lumber is knowledge here; costs that were lumber become gold where it matters).
7. **`"extra"`**: main-map or hand-made objects the faction uses (a WL hero it keeps, WL's ships) -- they need a
   limit in the faction's ObjectInfo too.
8. **Run**, then `check_json.py`, `content_index.py`, `gen_ai_data.py`. Code: `<Faction>Faction` (template
   `ForsakenFaction`), `ProcessObjectInfo`, one `[SL]` line in `WarcraftLegacies.Shared/ObjectInfoRepository.cs`.

## WL's rules a copied faction must follow (each found in a playtest)

- **Every unit a faction trains needs an object limit in THAT faction** -- WL refunds and removes any trained unit
  whose limit is 0 (`GameLogic/RefundZeroLimitUnits`). The 0.39.8 creep market "trained nothing" because of it; units
  any player can buy (the Refugee Slave Camps) get a limit in every faction (`FactionManager.FactionRegistered`).
- **Flight and Navigation**: WL's build (`WarcraftLegacies.CLI/Migrations/FlightMigration`) makes EVERY flying unit
  require Flight (`R09X`); ships need Navigation (`R04R`, main map). Every faction's research building offers both
  (`"ures": "...,=R04R,=R09X"`). Missing Flight locked Kezan's Oil Rigs and zeppelins (0.59 playtest).
- **Tooltip placeholders** (`<A03Z,DataC1>`) name 24p ids. The tool remaps them (0.60): copied -> new id, the copy's
  stock base -> the copy, other 24p objects of the same kind -> the copy itself; stock ids / other kinds stay. A 24p
  ":base" form (`<A03Z:ANab,...>`) is dropped -- the game does not read it.
- **Names**: a 24p skill renamed only in its tooltips (no `anam` / `aret`) shows the stock name in the learn button,
  localised ("Saeurebombe"). The tool adds name and learn title from `atp1` (0.60).
- **Tooltips are rewritten by WL's build** (`UnitTooltipBasicMigration` / `UnitTooltipExtendedMigration`): a unit's
  `utip` / `utub` set by a tool may not survive -- requirements still show.
- **Workers** are excluded from the bot's train lists by base id or `peon` in `utyp` (`gen_ai_data.py`,
  `WORKER_BASES`) -- the Forsaken Acolyte was trained as an army unit before 0.60.
- **Command card**: about 9-11 train buttons per building (3 columns x 3 rows, the 4th column free for rally /
  cancel). More kinds -> more buildings (the Refugee Slave Camps: one per town).
- **Models**: the sandbox / a fresh clone has LFS pointers only. Commit the patch, `git lfs pull`, run `import_24p.py`
  again -- only then does it see which textures the models use (and copies them).
- **Abilities that name abilities** (0.61): an ability the main map has under the same id and name is used as it is --
  but if its 24p data names 24p-only abilities (Engineering Upgrade swapping Pocket Factory for `A0PT` ...), the hero
  gets abilities that do not exist: empty icons, then a crash. The tool copies such an ability ("dangling").
- **Abilities that name a copied object** (0.61): the Alchemist's Acid Bomb, used under its main id, required the
  main map's `R07M` -- another object than the 24p's Alchemist Training (copied as `RZk1`): the training gave no
  spells. The tool copies such an ability too, until nothing changes.
- **Autocast switched on by code** (0.61): bots never touch autocast. Give the ability an on / off order
  (Moon Well's Replenish `Ambt`: replenishon / -off; Faerie Fire `Afae`: 852150 / 852151), issue the "on" order when
  the unit appears, and track the player's toggles through `EVENT_PLAYER_UNIT_ISSUED_ORDER` -- there is no native
  that reads an autocast state (`KezanEnterpriseOil`, `KezanEnterpriseBombers`). Two abilities of the same base on
  one unit clash (the Oil Supply's Transfer Fuel is `Ambt` -- the refinery's copy leaves it out).
- **Dropped objects**: a root removed from a set -> the tool deletes its file, the id stays reserved in
  `import_ids.json` (0.60).
- **Preplaced bases**: `StartArea.Prepare(area, RescuePreparationMode.HideNonStructures)` keeps buildings visible and
  invulnerable (nobody builds there), units hidden -- only Neutral Passive units are prepared; CPs (Neutral Hostile)
  are given with `StartCps.Give`.
- **Spawn points**: check the spot on the pathing map (`../ai/map_query.py`, a flood fill from the base) -- Kezan's
  Builders appeared in a cliff pocket (0.59).
