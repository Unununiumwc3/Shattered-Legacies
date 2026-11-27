# Changelog -- Shattered Legacies (fork content)

One version number for the whole map (`README.md`, "Map name and versions"). Bot changes are in
`../Ai/CHANGELOG.md`; up to 0.51.1 that file held the fork's changes too (fallbacks 0.39.1-0.39.10, Kezan Enterprise
0.39.5-0.39.8, map name 0.39.9, the extra CPs 0.39.4-0.46.2).

## 0.89.2 - Twilight opening and invasion routes

- Move Twilight's opening camp and P20's camera to the Southern Highlands at (19015, -6411). Cho'gall and five Cultists start with the camp. Its units, buildings and gate share the opening pause. Capture the Grim Batol control point to earn the fortress and its workshops.
- Neutral northern capitals no longer open the Northrend–Kalimdor crossing while starting bases are being assigned.
- Move the clickable Statue of Thaurissan to the neighbouring dwarf statue inside Shadowforge and restore the decorative statue at the old spot.
- Handle the preplaced Majordomo's death once. Register Benediction after the death event and record both steps in the dump.

## 0.89.1 - restore map startup

- Store locked elite choices in a named type so the Lua runtime can initialize the map before registering commands and starting the turn clock.
- Pass the vassal to the seat-holder callback to avoid a CSharp.lua delegate type collision during initialization.

## 0.89.0 — Twilight's Hammer returns as a start faction

No bot changes (version only, `../Ai/CHANGELOG.md`). No playtest yet.
- **Twilight's Hammer on P20** (`Timelines/Factions/TwilightHammer/TwilightStart`): the lobby's NPC slot P20 (the one
  Twilight had in the 24p) is a playable slot on the Old Gods' force; the fork's free slots for splits are P11 and P14.
  One second in (if the slot plays): Grim Batol -- its capital, the cult's buildings around it and its CP --, the
  Southern Highlands CP and the 24p's camp there: the **Twilight Citadel** (24p `h05U`, import set TWILIGHT: Cultists
  and Battlemasters -- the Great Halls train Cultists only now), a Great Hall, the Altar of Visions, 4 Burrows and 3
  Lookouts; Cho'gall, 5 Cultists and 14 soldiers. The ogres of the Southern Highlands make room.
- **The gate of the pass** north of Grim Batol: the wooden gate becomes the big Elven Gate (WL's gate unit, Twilight's),
  between two of the 24p's watchtowers.
- **"The Old Pact"**: Twilight takes Dragonmaw Port's CP -> 2 Dragonmaw Dragon Riders every 3 turns while it holds the
  port and the clan is not in play. The Dragonmaw's base at the port stays hidden and invulnerable -- it is theirs.
- **The Dragonmaw are Twilight's fallback** (Hourglass, `RZdK`): Grim Batol lost and Cho'gall dead -> the clan rises
  at Dragonmaw Port, sworn to whoever holds the port (first master: Grim Batol's holder, else Cho'gall's killer); the
  cult is left behind. The old in-place takeover (`WatchTwilight`) is gone.
- **The Fel Horde has no fallback for now** (the Consortium comes later): an info row on the Hourglass.
- Twilight's quests run from the start; their texts name Grim Batol instead of the port.

## 0.88.0 — Theramore and Jaina, Kalecgos, the Claw of Ursoc, Acherus flies, the plague goes south

Bot changes: `../Ai/CHANGELOG.md` 0.88.0 (`AiApi.Landmass`). Agreed 2026-11-27; the objects come from
`tools/shattered-legacies/content/content_088.py` (after `import_24p.py`, set `HEROES_088`). No playtest yet.
- **Theramore is conquerable** (`Events/TheramoreJaina`): once Dalaran holds Theramore, Jaina's Sanctum is its seat --
  brought to 0 life it is TAKEN (it never dies; `Core/Effects/CaptureSeat`), and Theramore serves the taker's team.
  Held by an evil faction (the Legion's and the Old Gods' teams, the Fel Horde, the Dragonmaw) Jaina becomes a
  **dreadlord** (Mal'Ganis' kit, the dreadlord model); held by anyone else she is Jaina again -- as often as it changes.
- **Kalecgos** (`Events/QuestTheNexus`): Dalaran's quest "The Nexus" -- Jaina channels at the Nexus for a minute (a
  little hanky-panky) -> Kalecgos (the 24p's, human form a caster) and 5 blue dragonspawn. "Dragon Form" (30 s) turns
  him into a great azure dragon: more attack damage, melee, Frost Breath and Devour Magic; the same hero skills and
  primary attribute (the 24p's timed Metamorphosis replaced by Blizzard).
- **Defias**: "The Stonemasons' Due" -- 1,500 gold pillaged -> Edwin VanCleef (the 24p's Kingpin, without the 24p's
  guild building and permanent invisibility).
- **Majordomo** drops **Benediction** (the Eye of Divinity, holy form only). **Mini-bosses** with an artifact
  (`Events/BossDrops`): Mmmrrrggglll (the Pearl of the Grimscale, the murloc camp at 11164, 2104) and Snarlmane of the
  Blackmane gnolls (the Blackmane Warbanner, the gnoll camp at 16744, -9216).
- **Furbolgs** (`Factions/Furbolg/FurbolgClaw`): Xavius (demi-hero, holds the **Claw of Ursoc**) and Peroth'arn
  (demi-hero) come with them in Timbermaw Hold. Their own faction while Xavius lives or the Claw is theirs; the first
  time the Claw leaves them they serve the Claw holder's team, and Xavius and Peroth'arn turn Neutral Hostile. No
  Nordrassil seat any more; the arrival text announces the Claw. No bot hook.
- **Harpies**: the seat is **the Broodmother** -- a caged hostage (the Meat Wagon model) by the nests that cannot move;
  taken at 0 life (CaptureSeat). No eggs yet.
- **Ebon Blade** (`Factions/EbonBlade/AcherusFlight`): all four death knights at level 10 -> Acherus researches "The Ebon
  Hold Rises" (60 s) and flies: a transport with Remorseless Winter, Army of the Dead and Death's Advance; its death
  knights are revived at it. At 0 life it burns, hangs still and invulnerable for 60 s and mends (its life is the
  timer); attacks never change its owner. Movement: the bots' own (playtest).
- **Kalimdor <-> Northrend** (`Events/KalimdorCrossing`): two harbours (a waygate pair, for everyone) open when one team
  holds 70 % of Kalimdor's CPs (from turn 30) or Lordaeron, Dalaran and Quel'thalas have all lost their capitals.
- **The plague goes south** (`Events/QuilboarPlague`): a Scourge research at the Hourglass and the Frozen Throne, open
  after the Plague of Undeath (bots at once) -> Amnennar the Coldbringer at Razorfen Downs and 30 plague boars marching
  on six Kalimdor strongholds; the zombie plague (only dispel / purge cures it) eats 2 life a second, an infected unit
  that dies rises as a Scourge zombie.
- **Draenei**: "The Survivors of Shattrath" -- hold the Exodar's bridge six turns -> Maraad trainable. **Dragonmaw**:
  "Alexstrasza's Chains" -- hold Grim Batol -> a Red Drake every turn, until Nek'rosh is slain: then the red dragons
  turn Neutral Hostile.
- **Elite choices** (`Events/EliteChoices`, `gen_hourglass.py`): WL's seven choices are also on the Hourglass; every
  faction with two elite-tier units gets a lock (one of the two, from turn 10; bots pick at random): Stormwind,
  Lordaeron, Dalaran, Quel'thalas, Kul Tiras, Gilneas, Scarlet Crusade, Sunfury, Draenei, Legion, Fel Horde, Illidari,
  Black Empire, Skywall, Nazjatar, Ashtongue, Zandalari, Forsaken, Kezan. Ironforge's and Ahn'qiraj's decision buttons
  moved to (2, 0).
- `Core/Conquest`: seats that are not CPs (`Register(vassal, () => holder, ...)`), `Release`, `Join` public.
- Not in 0.88 (TODO): Twilight (its own patch, its own slot), Garithos, the broodmother's eggs, Anathema, ability
  choices.

## 0.87.1 — no second fallbacks; the docs brought up to date

Bot changes: `../Ai/CHANGELOG.md` 0.87.1 (version only).
- **No second fallback for Gilneas or the Draenei** (maintainer): both are themselves fallbacks and are conquered.
  The placeholder rows "Gilneans on Darkshore" and "Draenei of Shattrath" are gone from `TimelineCatalog` and the
  Hourglass (`RZd2` / `RZc2`, `RZd8` / `RZc8` deleted, `gen_hourglass.py` rerun). Skywall's info row "Conquered, not
  defeated" stays.
- **The Exodar has no preplaced shipyards** (maintainer): the two `o059` in `Units/Exodar_Interior.json` are removed;
  the Draenei can still build shipyards within their limits.
- **Docs**: `Timelines/TIMELINES.md` gets "0. The fallback map today" (slot, base, seat, team, code), the engine and
  HQ tables of 0.87; `SCENARIO_CONCEPTS.md` follows it -- the replaced concepts are listed in its new section 14
  (Garithos path and Great Alliance stay open); `TODO.md` drops the stale order of the work and the 0.68 / 0.69
  handoffs and holds the plan discussed for 0.87.1 ("6. Faction by faction"); `UPSTREAM.md` (the Draenei: done),
  `AI_AGENTS_CONTEXT.md` (state, checks, generators, void dungeons), `README.md`, `VOID_DUNGEONS.md`, the content
  tools' README, `../Ai/AI_AGENTS_CONTEXT.md` (current build); `PLAYTEST-0.69.md` marked historical. Comments in
  `TimelineCatalog` and `Setup/FactionChoiceDialogSetup.cs` (Gilneas is Lordaeron's fallback) corrected.

## 0.87.0 — the Exodar flies, Timbermaw Hold, the 24p's Dark Iron, dungeon creeps

Bot changes: `../Ai/CHANGELOG.md` 0.87.0. Map moves: `Timelines/VOID_DUNGEONS.md`.
- **Orcish Horde -> trolls only** (maintainer): whoever beats the Horde, an Old God too, gets the Zandalari. No Twilight
  route for the Horde any more.
- **Fel Horde -> the Dragonmaw** (maintainer): whoever beats it. Twilight's Hammer is on hold (its code stays).
- **Conquerable** (maintainer) -- each stays in its team until someone takes its seat; it can still be defeated:
  - **Quel'thalas' Sunfury**: the Netherstorm CP (Tempest Keep's ground).
  - **Gilneas** (Lordaeron's): Gilneas City. The mercy text when it is taken: "Greymane's walls have fallen. Gilneas begs
    for mercy and bends the knee to its conqueror."
  - **The Draenei** (the Legion's): the Exodar's bridge (its CP).
- **Stromgarde** (agreed 0.81): Stormwind falls back to the Defias before it won Stromgarde -> the quest passes to
  Lordaeron as "Stromgarde's Oath" (a Lordaeron hero at Stromgarde: its units and Galen Trollbane). Lordaeron gone
  too -> Stromgarde stands alone, a capturable neutral. Both with an announcement.
- **The Exodar flies** (maintainer):
  - Its base is the inside of the ship now: moved into the western void (`tools/shattered-legacies/terrain/
    exodar_move.py`); Azuremyst's old spot is void -- carved out of Kalimdor.
  - The only way in is the ship (a waygate with the Exodar model). It lands at the spot nearest the home of whoever beat
    the Legion. 16 landing spots on all continents and in Outland.
  - "The Dimensional Ship" (WL's quest, rewritten) unlocks **Set Course** on the Dimensional Generator: aim it anywhere,
    the ship flies to the nearest landing spot -- 90 s in flight, both gates shut, nobody gets in or out. Only while
    the bridge is held by the draenei or one of their allies; 5 minutes cooldown. Bots never fly it.
- **Furbolgs: Timbermaw Hold** (maintainer): their den, a Felwood glade copied into the void. The Totem Lodge (moved to
  -11154,10970 -- the maintainer's -11138,11210 is a cliff edge, this is the nearest open ground) is its entrance; the
  **Shrine of Ursoc** inside is their HQ and altar; the Timbermaw Hold CP (10 gold) is theirs on arrival.
- **The Dark Iron are the 24p's** (maintainer):
  - Troops: Anvilrage Guardsman, Bear Rider, Chieftain (elite), Magma Tank, Shadowcaster, Flame Keeper, Gyrocopter --
    with their Dark Iron models. Kept from 0.86: the tinted buildings, Mason, Rifleman, Mortar Team, War Golem,
    Dreadnaught.
  - Heroes: Dagran Thaurissan; Golem Lord Argelmach now has the Dark Iron caster model and **Forge Golem** (summons War
    Golems) instead of Storm Bolt; Fineous Darkvire the Chieftain-king model.
  - **Seat: the Throne Room** (was Tavern District). Its throne is their HQ, the Throne of Thaurissan.
  - **Gatebreaker**: every 5 turns their controller (the Throne Room's holder, at first their conqueror) gets a Dark Iron
    Gatebreaker at Shadowforge City -- about 200 chaos damage, a 4.5 s swing.
  - **Mercenary Charter** (the Siege Foundry's holder, allied): 50 gold tribute -> a Mortar Team each turn (no golem
    any more); the first tribute a weapons level. The **Statue of Thaurissan** by the Foundry stands with its holder:
    +4 armour around it.
  - **Shadowforge Black Market**: neutral, by the Treasury, anyone may shop -- expensive: Titanforged Signet (+3 all,
    1000), Titanforged Gauntlets (+9 Strength, 1200), Titanforged Core (immolation, 1800), Book of Experience (600),
    Mercenary Staff of Town Portal (800), Staff of Teleportation (1500), Granite Token (400).
  - The faction's own shop is the Dark Iron Arms Vault now (it was "Dark Iron Black Market").
- **Dungeon creeps** (maintainer): a fallback starts with its dungeon's seat only. Nazjatar's three other CPs, the
  Treasury and the Siege Foundry, the Goblin Foundry and Ironclad Cove are guarded by creep camps; Majordomo Executus
  holds Shadowforge's west throne.
- Models of the 24p Dark Iron (Git LFS): `git lfs pull` after committing.

## 0.86.1 — build fix: Azshara's skills

- **The map build stopped** in the CLI's object migrations: "Could not find object with key 'A0PD'" (Empress of
  Nazjatar, 0.84). The 24p's own data lists her skills as "A0ZN,A0PD,A0ZOAEsf,AEsf,Aamk" -- two ids run together --
  and that one bad token left the whole list un-remapped: her copy named the 24p's ids (A0PD is absent here; A0ZN is
  another ability in this map).
  - `import_24p.py` repairs it before importing (`REPAIR_24P`). It now also stops with an error on a malformed id list
    instead of copying it un-remapped.
  - Azshara now has her 24p kit: Water Wave, Water Images, **Tidal Aura** (newly copied with its buff and its
    model -- a Git LFS file: `git lfs pull` after committing) and Starfall.
  - `content/check_json.py` now also fails on any ability, unit, research or item id that a unit or item names and
    that exists neither in stock data nor in the map: the lists the CLI's migrations read.

## 0.86.0 — the Dark Iron of Shadowforge and the Furbolgs

Bot changes: `../Ai/CHANGELOG.md` 0.86.0.
- **Ironforge -> the Dark Iron of Shadowforge** (maintainer; replaces the Explorers' League placeholder). Lore: the
  Bronzebeard throne is empty; a Dark Iron expeditionary force tunnels up to claim the Great Anvil -- not for Ragnaros
  but for a ruthless industrial empire. Cold-blooded pragmatists: gold, ore and survival.
  - **Objects:** Ironforge's tech tree as Dark Iron copies (`tools/shattered-legacies/content/dark_iron.py`, ids
    `?Zi?`): "Dark Iron <name>", the suffix "(Dark Iron)", a soot tint. The Explorer's Vault is the Black Market.
    Researches are shared.
  - **Heroes:** Dagran Thaurissan (WL's legend), plus Golem Lord Argelmach and Fineous Darkvire (copies of
    Muradin / Magni).
  - **Left out:** the Bronzebeard / Wildhammer heroes, gryphons, Flame / Artillery Tanks and the Deeprun Tram
    research.
  - **Arrival (the Great Forge lost):** Shadowforge opens; its entrance is the Shadowforge City building. WL's Dark
    Iron outpost in Shadowforge City is theirs, and so are the dungeon's three CPs. They get a Dark Iron town inside
    (hall, altar, barracks, workshop, forge, 2 taverns), 5 Masons and an army.
  - **Mercenary Charter:** vassal of whoever holds the **Siege Foundry** (renamed from Forge District; Imperial
    District is now the **Dark Iron Treasury**). Every turn the charter holder, if allied, pays 50 gold tribute and
    gets a Dark Iron War Golem and Mortar Team at the Foundry. The first tribute also gives him one weapons level.
    Halls indestructible, never defeated.
  - **Quests:** "The Great Anvil" (the Great Forge + the Ironforge CP: a War Golem and Mortar Team every turn there,
    plus a weapons and an armour level) and WL's artifact hunt "Secrets of Uldum".
- **Druids -> the Furbolgs** (maintainer; replaces the Dream trees placeholder). Lore: the tribes follow whoever proves
  worthy of holding their Sacred Ancestral Totem (the Claw of Ursoc). Seat: the Nordrassil CP.
  - **Units:** one hero, Nafien (Summon Bear, Entangling Roots, Healing Wave, Tranquility), and the Totem Lodge of
    Ursoc by Nordrassil (HQ and altar, indestructible, preplaced and visible).
  - **Levy:** WL's creep furbolgs every turn: a Furbolg plus one per **world tree** their side holds (Nordrassil,
    Vordrassil, Shaladrassil, Seradane -- theirs or an ally's).
  - **Strength:** every furbolg gets +3 damage, +1 armour and +60 life per tree, kept up to date each turn.

## 0.85.0 — Defias, Centaur Khans and Harpies; Skywall to Ahn'Qiraj, Gilneas to Lordaeron

Bot changes: `../Ai/CHANGELOG.md` 0.85.0. New objects: `tools/shattered-legacies/content/minor_factions.py` (stock
skills only, WL's creep models).

Fallbacks now (maintainer's table): Stormwind -> **Defias**, Lordaeron -> **Gilneas** (besides WL's Scarlet
Crusade), Tauren -> **Centaur Khans**, Black Empire -> **Harpies**, Ahn'Qiraj -> **Skywall**. The Zandalari stay the
Orcish Horde's alone. All three new factions are VASSALS (Core/Conquest.cs): first sworn to whoever beat the old
faction (`Core/LastConqueror`: the last to raze or take its HQ), then to whoever holds their seat; never defeated.

- **The Defias Brotherhood** (Stormwind Keep lost).
  - The Deadmines open; they get the Deadmines' three CPs (the seat, the Deadmines CP, whoever holds it) and
    Westfall's bandit camp.
  - Units: WL's bandits as Defias Bandits, Brigands, Rogues, Assassins and Enforcers. **Every unit has Pillage**,
    the heroes too.
  - Heroes: **Dunco Ratsbon** (wants the loot: Fan of Knives, Wind Walk, Critical Strike, Mirror Image) and **Count
    Clessington** (wants Stormwind's estates: Silence, Devotion Aura, Hex, Charm).
  - No buildings: recruits every turn in the Deadmines (3 Bandits and a Brigand).
  - Quests: "Ratsbon's Haul" (hold 1500 gold: +Rogue +Assassin per turn), "The Clessington Estates" (Westfall,
    Duskwood, Elwynn Forest: 500 gold, +Enforcer +Brigand), "The Brotherhood Rises" (both: +3 per turn).
  - Heroes come back by themselves after 60 s.
- **The Centaur Khans** (Thunder Bluff lost).
  - Lore: the Tauren's conqueror is the Apex Predator of the plains; the centaur swear him the Blood-Oath of the
    Hunt. Their seat is Thunder Bluff's CP.
  - Five khans, one per clan: Leh'Prah (Kolkar, at the start), Ablinh (Galak), Hratha (Maraudine), Kammah (Magram),
    Shodo (Gelkis).
  - WL's centaur tents are the five camps: by Thunder Bluff, Southern Barrens, Durotar, Desolace, near Silithus.
    They are visible and indestructible until their clan joins.
  - Nothing is trainable: WL's creep centaurs arrive every turn at the camp by Thunder Bluff (2 Outrunners, an
    Archer).
  - Quests: each other khan joins when you hold his land's CP, with his camp and one more warrior per turn. "The Five
    Khans" adds 3 more per turn and 300 gold. Khans return by themselves.
- **The Harpies of Stonetalon** (N'Zoth lost).
  - Lore: the Void-Touched Matriarch's Resonance Totem breaks; in Magical Thralldom they imprint on N'Zoth's slayer.
    Their seat is Stonetalon Peak.
  - WL's 8 harpy nests become 7 Harpy Nests (Scouts, Rogues, Windwitches; food) and the Harpy Roost (Storm-hags and
    the heroes **Serena Bloodfeather** and **Sister Hatelash**). All are indestructible; no tech.
- **Skywall** is Ahn'Qiraj's fallback (C'Thun's slayer is the first overlord). **Gilneas** is Lordaeron's (Capital
  Palace lost; new Hourglass decision `RZdJ`).
- New helpers: `Core/Effects/TurnLevy` (units every turn), `Core/Effects/HeroRevival` (heroes without an altar),
  `Core/LastConqueror`.

## 0.84.0 — the naga of Azshara; the dungeons shut until their fallback; new gate models; the Deadmines as a mine

Bot changes: `../Ai/CHANGELOG.md` 0.84.0. Layout, gates, CPs: `Timelines/VOID_DUNGEONS.md`.
- **Sentinels -> the Naga of Nazjatar** (maintainer). The 24p's Nazjatar naga are imported (`import_24p.py`, set
  `NAZJATAR`, ids `?Zb?`: its N'Zoth tree without the Old God). The tree:
  - Temple of Azshara (HQ), and Pillar of Waves -> Monument of Currents -> Temple of Tides
  - Altar of the Depths, Coral Bed, Spawning Grounds, Coral Forge, Shrine of Azshara, Royal Pyramid, Depth Void
    Portal, Tidal Guardians, Deep Sea Vault, Mur'gul Slaves
  - units: Naga Incursors, Hatchlings, Deepseers, Bloodkin, Sea Turtles, Couatl, Tidal Terrors, Naga Sea Witches,
    Immortal Guardians, Trench Hydras
  - heroes: Lady Naz'jar, Queen Azshara and Atoph the Bloodcursed

  Trigger: Auberdine and Feathermoon Stronghold lost.
  - **Before:** their base stands in Nazjatar where the 24p map preplaced it. It is Neutral Extra, visible and
    invulnerable, behind the shut gates.
  - **Arrival:** the base is theirs, Nazjatar's gates open, and they get its four CPs (the Eternal Palace whoever holds
    it). Lady Naz'jar, 5 Mur'gul Slaves and an army arrive.
  - **Vassal:** sworn to whoever holds the Eternal Palace. At the start that is whoever destroyed or took the last
    Sentinels HQ. Never defeated; its temples are indestructible.

  Quests: "The Empress Awakens" (Maelstrom + Azshara Coast -> Queen Azshara, Trench Hydras), "The Bloodcursed"
  (Ranazjar Isle -> Atoph), "Reel Them Back" (Nazjatar + Heart of Azeroth -> Immortal Guardians). Maiev's "Chase
  Illidan to Outland" is no longer the Sentinels' fallback (it becomes a sub-path, TODO.md).
- **The dungeons stay shut until their fallback arrives** (maintainer: the extra CPs' gold would upset the balance).
  Their gates stand from the start, visible: Nazjatar opens for the naga, Shadowforge for the Dark Iron and the
  Deadmines for the Defias (both later).
- **Gate models** (maintainer):
  - circles (the 24p's teleporter circle effect) inside every dungeon and at Nazjatar's world entrance
  - a gold mine for the Deadmines, now at 7721,-17503 in Westfall
  - WL's Shadowforge City building is Shadowforge's entrance (it got the waygate ability); the inner circle stands
    at -20640,23230
  - the Undercity's inner gate is its 24p iron gate
- **Shadowforge's CPs are districts** of Shadowforge City: Imperial District, Forge District, Tavern District.
- **The Deadmines look like a mine**. Dirt instead of sand. Instead of silithid doodads: wooden fences, lantern
  posts, archways, mine carts, torches, stalagmites, crates, cave walls, junk and treasure. No plants.
- `copy_area.py`: `--retype`, `--tex`, `--drop` (redecoration, and doodads where the code places a gate with the
  same model).
- Models: `git lfs checkout` after applying (22 naga models / icons).

## 0.83.0 — Nazjatar, Shadowforge and the Deadmines in the void; the Undercity's control point

Bot changes: `../Ai/CHANGELOG.md` 0.83.0. Layout, gates, CPs: `Timelines/VOID_DUNGEONS.md`.
- **Undercity control point** (maintainer's spot -27139,22825, 20 gold): preplaced Neutral Hostile, out of reach
  while the gates are shut. "The Throne of Lordaeron" gives it to the Forsaken with the buildings.
- **Three more 24p dungeons in the void**, each with its own place found by search. "Void" now means the unflyable
  map boundary: deep sea is not void (the 0.82 plan mixed the two). Every dungeon has at least 12 tiles of void to any
  other land or sea.
  - **Nazjatar** (south-west, -23168,-30976 .. -15360,-25600). Gate by WL's Nazjatar CP beside the Maelstrom. CPs
    Eternal Palace 25, Heart of Azeroth 30, Lightless Cavern 15, Gishan Cavern 10. To make room, **WL's seven unused
    squares in the south-west corner are void now** (they held nothing since the Undercity moved; `copy_area.py
    --clear`).
  - **Shadowforge** (24p Blackrock Depths + Molten Core; north-west, east of the Undercity,
    -24448,21120 .. -17920,24832). Gate at the east side of WL's Shadowforge City. CPs Shadowforge City 20, Molten
    Core 15, The Grim Guzzler 10.
  - **The Deadmines** (the 24p Temple of Ahn'Qiraj instance; south-west corner, -28672,-31488 .. -24448,-22912). Gate
    in Westfall above the bandit camp. CPs The Deadmines 20, Goblin Foundry 10, Ironclad Cove 15. The terrain is still
    the temple's; the redecoration comes later.
  The three gates are open from the start (stock waygates, invulnerable). The CPs are WL's neutral CPs (new types
  `nZv0`-`nZva`, Chinese names too, `tools/shattered-legacies/content/void_dungeons.py`). No 24p creeps. Each
  dungeon is a WL instance (no world-tree immortality, Summon Legion or slipstream across its border).
- `copy_area.py`: the destination must be unflyable void. A type that the 24p map only modifies counts as stock.
  `--types A=A` keeps our definition, and the tool checks that imported models exist and that new ids are free.
  `--clear` turns a rectangle into void. New doodad types `DZv2`-`DZv7`, destructable
  `BZv0`.

## 0.82.0 — void-area dungeons: the 24p Undercity

Bot changes: `../Ai/CHANGELOG.md` 0.82.0. Plan and layout: `Timelines/VOID_DUNGEONS.md`.
- **Void-area dungeons** (maintainer, 2026-11-26): the map's black void is to hold gated dungeons, packed tightly and
  walled by the void. New tool `tools/shattered-legacies/terrain/copy_area.py`: copies a rectangle of the 24p 1.18a
  map (terrain corners with remapped textures, pathing, shadows, doodads) into a void part of ours. It refuses if the
  destination is not void or a custom doodad type is missing (`--types` copies it under a fork id, `DZv*`).
- **The Undercity is the 24p map's**: terrain and 206 doodads copied into the north-west corner of the void
  (-28672,21120 .. -25728,24832; 24p -28672,14208 .. -25728,17920 moved 6912 north). New doodad types `DZv0`
  (Gargoyle Hub, 24p `D00O`) and `DZv1` (Iron Gate, 24p `D00K`), stock models. The Forsaken's buildings stand at the
  24p spots, visible and invulnerable until "The Throne of Lordaeron", which opens the gate in the palace's ruins as
  before. The buildings: Undercity, Royal Sepulcrum, Plaguetorium, Forbidden Archive, Graveyard, Laboratory, 4
  Graves and 5 Plague Towers. Before, it was 4 buildings and 4 towers. The inner gate stands at the 24p southern
  entrance. No rock ring any more (the void is the wall).
  The Undercity is a WL instance now (like Outland): no world-tree immortality, Summon Legion or slipstream across its
  border. The 0.57 square in the south-west corner is free again.

## 0.81.0 — fallback pairings, research ids, lore texts

Bot changes: `../Ai/CHANGELOG.md` 0.81.0 (ferries, Argus, hero revives).
- **Orcish Horde -> Zandalari** (maintainer): beaten by an Old God -> Twilight as before; by anyone else -> the
  Zandalari Empire at Zuldazar, first sworn to the conqueror ("King Rastakhan never bowed to Thrall's 'new' Horde ...
  he swears the Zandalari to those who destroyed it"). No Dragonmaw for the Horde any more (the Fel Horde keeps
  Twilight / Dragonmaw). Ahn'Qiraj keeps the Zandalari as its fallback too -- whoever falls first gets them; a
  Horde that finds them taken becomes Twilight.
- **Draenei get Exodar Regalis** (the CP, whoever holds it) on arrival.
- **Kezan joins its conqueror** -- not on arrival (it stays in Kul Tiras' team), but when its CP falls to another team
  later (`Conquest.Register(..., keepTeam)`).
- **Stormwind -> Gilneas** announcement: "The last of its people seek shelter at the Greymane Wall. Moved by their
  plight, King Genn Greymane is finally persuaded to open the gates -- Gilneas joins the war effort ..."
- **"ResearchId should be named ..." warnings at the start** (WL 5's integrity check): WL 5 gave the research ids of
  the Sentinels' "Gates of Ahn'Qiraj" (`R052`) and "Otherwordly Invaders" (`R007`) to its new quests "Break the Bluff"
  and "Raze Orgrimmar"; the fork has all four. The two old quests have their own research again (`RZu1`, `RZu0`, WL
  4.8.2's data) and also grant the shared one, so either quest still unlocks Moon Riders / Guild Rangers.

## 0.80.0 — Dragonmaw, Zandalari, Ebon Blade, Draenei fallbacks; Gilneas / Dalaran cleaned up

Bot changes: `../Ai/CHANGELOG.md` 0.80.0. One patch on 0.77.0 carries 0.78.0-0.80.0.
- **The Dragonmaw Clan** (new faction, maintainer): the orcs of the 24p's Twilight tree without the cult
  (`import_24p.py` set DRAGONMAW, ids `?Zm?`): Great Hall line, Altar of Storms, Barracks, War Mill, Red Dragon
  Hatchery, Docks, Watch Towers, Burrows, Voodoo Lounge (stock orc models); Peons, Dragonmaw Warriors, Spearthrowers,
  Skull Catapults, Dragon Riders, Dragon Whelps, Ancient Red Dragons (4); heroes Nek'rosh Skullcrusher and Zuluhed the
  Whacked. A vassal: sworn to whoever holds Dragonmaw Port (its seat), never defeated, its halls indestructible.
  It comes two ways (both start at Dragonmaw Port -- one of them, once):
  - **Orcish Horde / Fel Horde fallback**: Orgrimmar / Kil'sorrow Fortress taken by a player of the Old Gods' team ->
    Twilight, as before; by anyone else -> the Dragonmaw Clan (start army ~40), first serving that conqueror's team.
    The Hourglass decision ("Fallback: Twillight or Dragonmaw") explains both.
  - **Twilight's successor**: Cho'gall dead (not revived) while Grim Batol is not Twilight's. Until then Cho'gall is
    revived as before; then he is gone for good. The cult's own units turn Neutral Hostile ("its fanatics turn on
    them"), Twilight's heroes leave, its buildings / workers / catapults become the clan's, the Grim Totem goes
    hostile; Nek'rosh arrives; Grim Batol's holder (or Cho'gall's slayer) is the first overlord. Lore text for all.
- **The Zandalari Empire** (new faction, maintainer): Ahn'Qiraj's fallback (was "Brood of Nozdormu (not yet)"),
  trigger C'Thun lost. The 24p's Zandalar trolls (set ZANDALAR, ids `?Zx?`) with King Rastakhan and Zul; the
  preplaced Zandalari base at Zuldazar (WL's creep trolls) waits hidden / invulnerable and becomes theirs, with
  Zuldazar's CP (whoever holds it) and Nazmir (if neutral), 5 Gatherers and an army of 38. A vassal of whoever holds
  Zuldazar -- first C'Thun's slayer --, never defeated, its halls and Dazar'alor indestructible. Quests: the Amani
  (Zul'Aman), the Drakkari (Gundrak), the Sandfury (Zul'Farrak), "The Soulflayer's Prison" (Zul'Gurub, Hakkar's
  priests) -- 300 gold each --, then "The Empire Reborn" (all four): Direhorns at the Dinosaur Pen.
- **The Knights of the Ebon Blade** (Scourge's fallback, was "not yet"; maintainer): trigger = the Frozen Throne lost
  (WL ruptures it) AND, if Arthas rose as the Lich King, the Lich King permanently dead. A HERO-ONLY faction: four
  death knights with their own skills (`tools/shattered-legacies/content/ebon_blade.py`, `UZe1`-`UZe4`, level 6 on
  arrival) -- Thassarian (frost: Frost Nova, Frost Armor, Death Coil; Death and Decay), Koltira Deathweaver (unholy:
  Carrion Beetles, Animate Dead, Death Pact; Locust Swarm), Nazgrim (blood: Critical Strike, Shockwave, Vampiric Aura;
  Bladestorm), Thoras Trollbane (plate: Storm Bolt, Thunder Clap, Endurance Aura; Avatar) -- and Acherus: The Ebon Hold
  (`uZe0`, south-east of Icecrown Citadel, indestructible: revives them, 40 food). No Mograine (Lordaeron's name), no
  Rivendare (WL's Scourge). A vassal of whoever holds Icecrown Citadel's CP (first its holder, else the last enemy who
  hit the Frozen Throne), never defeated. The rest of the Scourge turns Neutral Hostile. No quest yet.
- **The Draenei are the Legion's fallback** (maintainer; was "Dreadlord Insurgents (not yet)"): the Sentinels' slot
  always plays the Sentinels (no Draenei pick). The Exodar base waits on Azuremyst (buildings invulnerable, units
  hidden); Darkshore's draenei leftovers go as in every Sentinels game. Legion Stronghold lost -> WL's Draenei faction
  without its Darkshore quest: the Exodar base theirs at once ("A New Home" completed), Velen, 5 Gemcrafters and a
  guard; they JOIN the conqueror's team. Lore by conqueror: a good team (Velen swears his people to the Legion's
  breakers), an evil one (a bitter oath: only that hand can keep the Legion broken), the Fel Horde (Velen's oldest
  vision -- orcs free of the blood pact, redeemable).
- **Gilneas / Dalaran** (maintainer): Dalaran no longer builds the Gilneas Manor; Gilneas no longer has Dalaran's three
  houses (farms) or the Violet Tower. Their quests were already separate (Dalaran: Shadowfang and Southshore kept;
  Gilneas as Stormwind's fallback: Crowley, the Sunwell Vial, Goldrinn).
- `import_24p.py`: `"unset"` (fields dropped from a copy -- its stock value again).
- **Model files**: the Dragonmaw and Zandalari copies need 9 imported models / icons (Nek'rosh, Zuluhed, the Dragon
  Rider, 2 Zandalari files) -- the patch carries them as Git LFS pointers: run `git lfs checkout` (or `git lfs pull`)
  after applying, as for every 24p import.

## 0.79.0 — HQ teleport, Kezan oil for bots, Twilight's port, Sylvanas' second ultimate

Bot changes: `../Ai/CHANGELOG.md` 0.79.0 (the emergency HQ teleport). Includes 0.78.0 (not applied separately).
- **Kezan oil for bots** (maintainer: "tech it at the speed of the caster research, free for bots";
  `KezanEnterpriseBot`): a bot Enterprise gets two oil steps on arrival, one more every 2 minutes and one per CP it
  captures -- Oil Rig, Oil Tanker, Oil Rig, Oil Tanker, Oil Refining 1, ... up to 8 rigs, 10 tankers and Oil
  Refining 2. Rigs and tankers are trained for free (one of each under way at a time), Oil Refining is granted; a
  free rig that finds no oil field refunds nothing.
- **Twilight keeps Dragonmaw Port** (maintainer): on arrival the CP is theirs whoever holds it (`StartCps.Force`;
  before, only a neutral one was handed over). Wetlands as before (only if neutral).
- **Sylvanas: two ultimates** (maintainer): the Dark Ranger and the Queen of the Forsaken lose the Attribute Bonus
  and get "Dark Lady's Command" (`AZs6`, one level, from level 6): up to 6 non-summoned enemy units (no heroes, no
  mechanical units) in the target area fight for her for 30 seconds, then die -- C'Thun's Inspire Madness and WL's
  own spell code. The Banshee (phase 1, until level 6) keeps hers. `forsaken_heroes.py`.

## 0.78.0 — Ahn'Qiraj holds its gate; gates open for bots; slot names

Bot changes: `../Ai/CHANGELOG.md` 0.78.0.
- **"Rebuilding of Ahn'Qiraj" removed** (maintainer): the Gates of Ahn'Qiraj belong to Ahn'Qiraj from the start
  (closed, not invulnerable any more; a bot opens them).
- **Bots open their gates** (the 0.77.0 orders were wrong, `../Ai/CHANGELOG.md`).
- **The slot's name follows the faction**: after Quel'thalas' split, a bot's Forsaken units still said "Quel'thalas"
  on hover (the scoreboard was right) -- `SplitFaction` now renames the slot like `SwapFaction` does (human players
  keep their own name). The same for Lordaeron -> Scarlet Crusade on a bot slot (WL code, `[SL]` line).

## 0.77.0 — WL 5.0.4: the UI / localisation fixes and an alignment audit; gates stay

Bot changes: `../Ai/CHANGELOG.md` 0.77.0 (gates).
- **Gates stay in the game** (maintainer): the bot's map-wide gate removal is switched off (kept in the code). Bots keep
  their own gates open. **Greymane's Gate** stays visible, closed and invulnerable until Gilneas joins (Stormwind's
  fallback) -- then it is Gilneas' and can be attacked; the announcement says "Gilneas joins the war effort".
- **WL 5.0.4 UI fixes** (#4146 / #4199 / #4189): illusions keep their caster's name (no new name from the name pool);
  the faction pick dialog only takes the picker's and neutral units into account; the strings of the artifact and power
  books, pager, commands, cheats, settings, floating texts, quest rewards and objectives (Build, Cast from, Start
  casting, Don't control), the "BIG" income, the refund message, the Titanforged label and the unrevivable tooltip go
  through the translation table -- Chinese clients now see WL's Chinese there (the table had them, the code did not
  ask). Not taken: WL's localised LEGEND names (the bots compare legends by name; a name that differs between an
  English and a Chinese client would desync a mixed game).
- **WL 5.0.4 balance / data fixes**: 74 objects that WL 5 changed and the fork never touched, taken as they are --
  gate icons, leftover "Breaking Bad Blood" (Warsong) upgrades removed from ~60 buildings, Seradane 6000 life, Old
  God / Exodar scale, the Control Point Defender's model path (renamed import), the Temple of the Moon's trainees and
  researches, Windfury without the Shadowpriest requirement, the Sulfuron Spire's pathing; leveled values of Reap,
  Apocalypse, Devour, Mass spells, Gryphon Orbit, Second Wind, Summoning Mastery, Permanent Metamorphosis and the
  Legion's health-on-hit computed per level as in WL 5 (the last two scaled wrongly before).
- **Audit tool** `tools/shattered-legacies/upstream/audit.py`: three-way comparison against WL 4.8.2 and 5.0.4; the
  result and what is left on purpose are in `UPSTREAM.md` ("Audit 0.77.0").
- Chinese overlays rebuilt for the changed objects.

## 0.76.0 — Skywall conquered, not defeated; Gilneas becomes Stormwind's fallback

Bot changes: `../Ai/CHANGELOG.md` 0.76.0 (quest runner).
- **Skywall** (maintainer): a conquered vassal like the Ashtongue (`Core/Conquest.cs`), with the **Vortex Pinnacle CP**
  (`nVXP`) as its seat. At the fallback it serves Thunder Bluff's conqueror; whoever takes the Vortex Pinnacle later
  gets its service -- Skywall joins that player's whole team and leaves the old one. It **cannot lose the game**
  (`EliminationRule`: an undefeatable vassal is never doomed) and its HQ buildings (Holdfast / Fortified Burg / Great
  Alcazar, the Vortex Pinnacle, the Sulfuron Spire) are indestructible while it owns them, also ones built or upgraded
  later. Hourglass texts updated.
- **Gilneas -> Stormwind's fallback** (maintainer): no longer a pick of the Dalaran slot (Dalaran always plays
  Dalaran). Stormwind Keep lost -> the player becomes the Kingdom of Gilneas behind the Greymane Wall: the region with
  its buildings, defenders and five CPs (Gilneas City, Keel Harbor, Duskhaven, Blackwald, Tempest Reach), Genn Greymane,
  Liberation of Gilneas researched (worgen), team **North Alliance**. Stormwind's heroes and everything left there are
  lost. Replaces the Valiance Expedition placeholder. Until then Gilneas waits hidden and neutral -- Dalaran's "The
  Greymane Wall" (which handed Gilneas to Dalaran) is gone, so the North Alliance no longer gets Gilneas early (a nerf
  against the Scourge). Movement: nothing new -- the region was closed this way before the Greymane Wall too; hidden
  neutral CPs are no bot targets and the route gate "greymane" opens only once a player holds Gilneas City.

## 0.75.0 — WL 5.0.4 alignment, bundle 4: hotfixes and Chinese; new lobby and loading texts

No bot changes (`../Ai/CHANGELOG.md` 0.75.0: version only).
- **Chinese** (WL 5.0.4 #4146 / #4147 / #4157, from the maintainer's add-on made on 0.68): Chinese clients get WL's
  Chinese runtime text (2,584 entries) and object text (overlays `*Data.zhCN/`), `-language zh`; one map for English
  and Chinese clients with `tools/shattered-legacies/localization/publish.py`. The add-on's overlays were made before
  0.73.0 -- they would have given this map's Shifting Sands, Frost Bite, Progenesis, ... WL 5's Frenzy / Teleport
  text; rebuilt with the new `localization/build_overlays.py` (`LOCALIZATION.md`). The add-on's separate
  `UPSTREAM-5.0.4-STATUS.md` was not taken (`UPSTREAM.md` is the ledger).
- **New texts** (maintainer), English and Chinese: lobby description, loading screen (`Info.json`, `zhCN/Info.json`),
  the start message, a new F9 entry "How Shattered Legacies works" (fallbacks, betrayals, the Hourglass, defeat, bots),
  five hints. WL's hint "a leaver's units are refunded" is gone (a bot takes over). The credits quest has WL 5.0.4's
  longer artist list and is translated too.
- **Hotfixes 5.0.1-5.0.4** (#4198, #4199, #4203): Boralus expires at turn 11 (was 8); lobby language bug
  (`FactionManager`); Kil'jaeden's Command counts a destroyed Nordrassil as done; Redeemed Highborne Adept Training
  in the Sentinels' limits. Not taken: `AhnQirajSetup`, Feathermoon at full life (both not wanted). The rest was in.
- **Ahn'Qiraj / Old Gods fixes** (#4080, #4097): Spell Conduction checks at damage time instead of buffs; the
  tentacle attack no longer hits its primary target twice.

## 0.74.0 — WL 5.0.4 alignment, bundle 3: Night Elves; Outland sites, Twilight's Hammer army

Bot changes: `../Ai/CHANGELOG.md` 0.74.0 (revive fixed, Cenarius' ghost for bots).
- **Halaa / Shattrath in the wrong place** (Ashtongue fallback): the site buildings (`oZt2`-`oZt9`, temples `oZt0` /
  `oZt1`) are built on the barracks, whose placement forbids unbuildable ground -- CreateUnit put Shattrath and Halaa
  hundreds away. 0.71.0 then moved them back with SetUnitX/Y, which on a structure moves the click circle but not
  the model; the purified building came on the displaced spot again. Now the site buildings copy the preplaced
  buildings' placement and collision (`ashtongue_objects.py`: "upap", "ucol"), so they stand on their spot; no more
  moving (a displacement is only logged).
- **Shattrath, the hub**: while Halaa, Telredor, Oshu'gun or the temple is being purified, a blue drain-mana channel
  runs from Shattrath to it (`AshtonguePurification.Channel`).
- **Twilight's Hammer starting army** (like the Scarlet Crusade's at Tyr's Hand): Cho'gall, 5 Cultists and 10
  Enforcers, 8 Phase Grenadiers, 6 Dragonspawn, 4 Ogre Magi, 4 Heralds, 2 Orcish Death Knights, 2 Battlemasters and
  2 Void Riders at Dragonmaw Port.
- **Bundle 3 -- Night Elves** (WL 5.0.4):
  - **Immortality by region** (#4184): a unit is saved (once) only inside the region of a World Tree its team holds --
    Kalimdor (Nordrassil), Broken Isles (Shaladrassil), Northrend (Vordrassil), Eastern Kingdoms (Seradane, new in
    the list); everywhere when the team holds all four. Druids 25 % / 45 % (were 20 / 40). The "Immortality is
    active" research (`YB01`) is gone, as in WL 5 (`A0FP` Haste, unused, no longer requires it).
  - **Wrath of the Wild** (Druids, #4172): Orgrimmar destroyed -> Malfurion +2000 XP and +5 attributes, 500 gold,
    Siege Ancient limit 4 -> 8, Siege Ancients +150 life / +10 damage. Its research is the fork's `RZn0` (WL 5 uses
    `R05A`, which is the Druids' "War of the Shifting Sands" here -- the seventh collision, `UPSTREAM.md`).
  - **Return to the Fold** (Sentinels, #4189): Redeemed Highborne limit 6, +6 from the quest; texts as in WL 5.
  - **Income and intro texts** (#4189): Druids 190, Sentinels 195 (were 155 / 145); WL 5's intros, with the Old Gods
    and this map's Feathermoon (rebuilt, no southern guard) added.
  - **Not taken**: Cenarion Hold to Neutral Passive (WL 5 empties southern Kalimdor; here it is the Druids' outpost
    against Ahn'Qiraj -- your call), Feathermoon's "southern passes open at turn 15" (the South Kalimdor guard is not
    wanted; the fork's rebuild objectives stay).
- **Cenarius** returns as his ghost for bots too (bot change; players always had WL's mechanic).

## 0.73.0 — WL 5.0.4 alignment, bundle 2: Dalaran; repairs of the 0.69 port

Bot changes: `../Ai/CHANGELOG.md` 0.73.0. Playtest of 0.72.0 (maintainer, 2026-11-25).
- **ID collisions of the 0.69 port repaired.** WL 5 reused ids of objects it had removed; the 0.69 port copied WL 5's
  Orcish Horde objects under those ids and so overwrote this map's (WL 4.8.2) objects of Ahn'Qiraj and Skywall:
  `A000` Shifting Sands (Ouro) had become Frenzy, `A00Y` Frost Bite (Neptulon, Skywall heroes) the Sea Witch's
  Teleport, `R003` Progenesis (C'Thun) Improved Frenzy, `RZ02` Shadow Weaver Adept Training Blademasters, `n00K`
  Tunnels of Ahn'Qiraj the Darkspear Isles. The five have their 4.8.2 data back; WL 5's objects moved to fork ids:
  `AZo0` Frenzy, `AZo1` Teleport (Sea Witch), `RZo0` Improved Frenzy, `RZo1` Blademasters (unit references
  updated; constants in `WarcraftLegacies.Shared/ShatteredLegacies/SlObjects.cs`). Side effects that are gone:
  researching Improved Frenzy no longer ran C'Thun's Progenesis code, and the AQ / Skywall heroes have their spells.
- **Improved Frenzy researchable**: it required `o02S` (WL 5's Warsong Fortress); the Orcish Horde's Fortress here is
  `ofrt` (like Blademasters) -- `RZo0` requires `ofrt`.
- **Darkspear Isles CP removed** from the Durotar start (the 0.69 port had copied WL 5's island-start CP with its
  coordinates swapped; it stood hidden under the orc base). The razormane camp on the new orc start (four creeps
  around -9000,-3100: `nrzb` x2, `nrzm`, `nrzt`) removed -- WL 5.0.4 has none there; the thunder lizards stay.
- **Orcish Horde fallback (Twilight's Hammer)**: the "Orgrimmar is lost" trigger looked for Orgrimmar with a map
  scan, which skips hidden units -- Orgrimmar is hidden until the Horde builds it, so the trigger never armed. It now
  takes hidden preplaced units too (`ObjectiveHqLost.Arm`). In the playtest the Fel Horde then took the shared
  Twilight path at 33:01 (both fallbacks lead to the Twilight's Hammer; first come, first served).
- **Bundle 2 -- Dalaran** (WL 5.0.4 as it is): the **Dalaran Shield** (Power Generator `n0DS` in Dalaran, placed at
  WL 5's spot 9216,4736 -- the city keeps its 4.8.2 layout; Antonidas must stand beside it: a 90 s dome that hurts
  undead inside the city and holds Antonidas channelling; Cancel Shield), the **clickable Way Gates** (`n0AO`, two
  at most, built by Farmers; the travel ability moves nearby allies to the other gate) and the **caster teleport**
  (research Teleportation `RDTP` at the Hall of the Arcane, Teleport on Geomancers -- Mages and Hydromancers already
  had it). Data that the 0.69 port had brought without code (Way Gates, `RDTP`, models, icons, the region) is now
  used; the missing objects (`A0E0`, `A0E1`, `A0E3`, `B0DS`, `n0DS`) copied from WL 5.0.4.
- **`-testkit dalaran`**: hands the Power Generator to the typing player with Antonidas beside it, two Farmers and
  resources for the Way Gates, Teleportation researched and three Geomancers.
- **Draenei**: unchanged for now (maintainer, 2026-11-25).

## 0.72.0 — WL 5.0.4 alignment, bundle 1: hero kits and artifacts (includes 0.71.1)

0.71.1 was never applied on its own: this build contains it (bot fixes, `../Ai/CHANGELOG.md`). New ledger of the WL
5.x alignment: `UPSTREAM.md` (ported, wanted bundles, not wanted -- the maintainer's decisions of 2026-11-24 -- and the
Draenei: terrain stays, the faction becomes a fallback, owner open).
- **13 dead buttons fixed**: the 0.69 port brought WL 5.0.4's hero DATA but not the code. Ported from WL 5.0.4 as
  they are: Tyrande's Lunar Sanctuary; Naisha's Glaive Trap and Watcher's Focus; the Priestess of the Moon elite's
  Light of the Goddess, Mass Wind Walk, Elune's Protection and Moonlit Arrows; Anasterian's Sunfire Barrage;
  Rivendare's Corruption Mine; Cenarius' Tree of Renewal, Lord of the Forest and Master of Nature progression; the
  Heartwood Ancient's Undying Grove, Spirits of the Forest and Root Tap; plus the Keeper's Living Wall, Antonidas'
  Arcane Cataclysm, the Warden elite's Spirit of Vengeance and Malfurion's new kit (Emerald Dream, Seed of Rebirth).
  Missing data files (dummy abilities, buffs, the treant / tree / wall units) copied from WL 5.0.4.
- **Elite choices** (WL 5.0.4): Druids research Keepers of the Grove or Heartwood Ancients, Sentinels Priestesses of
  the Moon or Wardens (elites start at limit 0); Vault of the Wardens empowers the Wardens or Maiev.
- **Artifacts** (WL 5.0.4): Soulseeker for Kel'Thuzad (kept on his ghost), Gorefiend's Truncheon for Teron, Bow of
  Elune's Grace for Tyrande; the Horn of Cenarius restores mana and goes to Cenarius when Malfurion awakens.
- **Sentinels**: WL 5.0.4 hero levels (Tyrande, Naisha) and the Tyrande / Maiev / Naisha dialogue.
- **Twilight's Hammer**: "Blessed Blade of the Windseeker" becomes "The Hammer of Twilight" -- same objectives
  (Cho'gall at the Broken Isles, Suramar), the reward is the new artifact Hammer of Twilight (`IZh1`) instead of
  Thunderfury, which stays in Ironforge's "Secrets of Uldum" pool.
- **Kept as the fork has them** (WL 5.0.4 ties them to quests not ported yet): Siege Ancient limit 6, Redeemed Highborne
  12. Not ported yet: Dalaran Shield and waygates (bundle 2), Immortality regions, Wrath of the Wild, Sentinels quest
  changes, Druids / Sentinels income (bundle 3), hotfix sync (bundle 4).
- **`-testkit`** (debug only, the bot's control-command gate): spawns the bundle's heroes at level 10, `-testkit items`
  the new artifacts, `-testkit foes` eight hostile footmen.
- Content index regenerated (`content_index.py`; it was stale since the 0.69 port).
- New tool: `tools/shattered-legacies/compile_check.sh` -- a real C# compile of the map code without NuGet (WCSharp
  from GitHub source). 0.72.0 compiles cleanly with it.

## 0.71.1 — bot fixes only

Map unchanged; see `../Ai/CHANGELOG.md` 0.71.1 (Legion bot leaves Argus again -- a 0.69.0 regression in the bot's
teleporter edges, home-first for Illidan, lighter spell checks). Halaa / Shattrath (0.71.0): the log showed
CreateUnit had pushed them 951 / 454 away; they are moved back.

## 0.71.0 — Long March creeps like WL 5.0.4, spell order strings, Halaa / Shattrath placement

- **Long March creeps**: the rest of the WL 5.0.4 layout along the route. Thousand Needles camp: the extra `ncks` and
  `ncen` removed (cells `-11776_-10240`, `-12288_-10752`); leg two: the five centaurs on the valley floor at about
  -13500,-9950 (cell `-13312_-9728`) replaced by WL 5.0.4's three on the hill (`ncea` -13309,-8989; `ncer`
  -13382,-9036 and -13494,-8942; cells `-12800_-8704`, `-13312_-8704`).
- **Order strings (Ncl6) of nine Channel spells** (every level): the stock UNIT-target orders made WC3 refuse the bot's
  point orders. Muradin's Stormbolt (TP49), Wild Throw (A14Q) `thunderbolt` -> `shockwave`; Solar Judgement (A01F),
  Apocalypse (A10N) `creepthunderbolt` -> `carrionswarm`; Ancestral Legion (A0YX) `creepthunderbolt` ->
  `breathoffire`; Spawn Tentacle (ZBST) `slow` -> `clusterrockets`; Inspire Madness (A10M, ZBIM), Voodoo Hex (MD28)
  `darkconversion` -> `stampede`. No C# listens to these orders; no carrier has another ability with the new order
  (`gen_spells.py` checks). Players cast them as before.
- **Halaa and Shattrath stay on their spot** when the Ashtongue take them over (`IllidariFallback.TakeSites`): the old
  building is hidden before the fel-tainted one is created, removed after it; a site that CreateUnit pushed to a free
  spot (Shattrath has no pathing texture: the units around it pushed it away) is moved back and logged.
- `ObjectiveHostilesInAreaAreDead` keeps the creeps it counts (`Targets`, `[SL]` marker) -- read by the bot.

## 0.70.3 — Long March: hill archers at the camp exit removed

The ported Long March (WL 5.0.4) starts at a camp whose exit, in the fork's 4.8.2 creep layout, is overlooked by
three centaur archers on a hill (Neutral Hostile `ncer` -11057,-10743, `ncea` -11204,-10694 and -11318,-10940).
WL 5.0.4 removed them when it added the march; the fork kept them, so the Tauren started by fighting ranged creeps
they could hardly reach and the caravan took ~7.5 minutes (0.70.2 playtest: escort 1:26 -> 9:09). Removed like in
WL 5.0.4 (their two map cells: `mapdata/WarcraftLegacies/Units/-10752_-10240.json`, `-11264_-10752.json`).
Not changed (still differs from WL 5.0.4, decide after the next playtest): the Thousand Needles camp has two more
centaurs (`ncks`, `ncen`), and leg two passes five centaurs on the valley floor at about -13500,-9950 where WL 5.0.4
has three on a hill at about -13400,-9000. Bot side: `../Ai/CHANGELOG.md` 0.70.3.

## 0.70.2 — fix: factions after Tauren Tribes missing (map init crash since 0.69.0)

0.69.0 gave the Orcish Horde back its Durotar start and removed "Countdown to Extinction", but Tauren Tribes'
"The Darkspear Champion" (`TaurenTribesFaction.RegisterOrcishHordeQuests`) still asked the Horde for that quest:
"Orcish Horde does not have a QuestData of type ...QuestCountdownToExtinction" while P5 registered, so player setup
stopped there -- every faction after it (P7 and up) was never created and missing from the scoreboard. The quest
was only a hidden gate ("the island phase is over"); it is gone, Rokhan still unlocks when the Horde completes
Orgrimmar or at turn 8. Checked: every other cross-faction `GetQuestByType` asks for a quest that faction creates.

## 0.70.1 — fix: map init crash since 0.69.0

The caravan escort hook (`AiApi.RegisterEscort`, used by the Tauren Long March) crashed CSharp.lua's type metadata
at map init -- no control points, scoreboard or building menus. Details and the new `delegate_check.py` in
`../Ai/CHANGELOG.md`.

## 0.70.0 — bot spellcasting (playtest candidate)

Bots cast WL's Channel-based hero and unit spells in big fights (`../Ai/CHANGELOG.md`). `Bot/SlBotSetup` hands the
fork's spell table (`Bot/SlAiSpells.g.cs`, empty today: every cast spell belongs to a WL faction) to the bot.

## 0.68.0 — hall and gate upgrades (playtest candidate)

The bot hall and Demon Gate changes are in `../Ai/CHANGELOG.md`. The project README includes a Windows asset recovery procedure and hashes for the Elate model and texture.

## 0.67.0 — bot playtest candidate

- From turn 20, five consecutive turns with zero team CPs trigger unused fallbacks or defeat for human and bot factions. Evaluate the team snapshot before applying outcomes. Successor grace still applies.

## 0.66.0 — bot economy playtest

Pass timeline HQs and fork worker menus to the bot. The Argus opening, worker construction and altar queues are described in `../Ai/BUILDING_DESIGN.md`.

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
