# Reuse candidates (GENERATED -- `tools/shattered-legacies/content/content_index.py`, do not edit)

Units, heroes and buildings that can be recycled for new content, by race. **model**: `stock` (every WC3
install has it), `main` (import already in the main map), `24p` (import only in the 24p map -- the copy needs
the model file too), `missing` (the import is not in the repo). **trains / builds**: what a building makes.
Full data and every other object (abilities, upgrades, items, used objects, stock WC3): the TSV files, see
`README.md`. Ask for content as `main:<id>`, `24p:<id>`, `stock:<id>`.

## Counts

| Kind | main used | main unused | 24p only | 24p changed | 24p different | stock used | stock unused |
|---|---|---|---|---|---|---|---|
| units | 1816 | 15 | 495 | 1045 | 385 | 56 | 287 |
| abilities | 1610 | 16 | 713 | 486 | 237 | 229 | 283 |
| upgrades | 462 | 0 | 148 | 133 | 71 | 30 | 16 |
| items | 89 | 13 | 81 | 28 | 2 | 4 | 223 |

## Unused in the main map

Objects of the main map that nothing uses (no faction limit, not preplaced, not in the code, not made or referenced by anything used). Free to reuse under their id.

### nightelf (9)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `HZy4` | High Priestess of the Aldor (Draenei) | hero | main |  |  | unused |
| `HZy3` | The Naaru (Draenei) | hero | main |  |  | unused |
| `oZy0` | Defender (Draenei) | unit | main |  |  | unused |
| `hZy1` | Elekk Knight (draenei) | unit | main |  |  | unused |
| `nZy0` | Lightforged Warframe (Draenei) | unit | main |  |  | unused |
| `oZy1` | Luminarch (Draenei) | unit | main |  |  | unused |
| `hZy0` | Rangari (draenei) | unit | main |  |  | unused |
| `even` | Spirit of Vengeance | unit | stock |  |  | unused |
| `hZy2` | Vindicator (draenei) | unit | main |  |  | unused |

### other (2)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `ncg1` | Clockwerk Goblin (level 2) | unit | stock |  |  | unused |
| `ncgb` | Clockwerk Goblin (level 1) | unit | stock |  |  | unused |

### undead (4)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `ucs1` | Carrion Beetle (Red - Anub'Arak)(Level 1) | unit | stock |  |  | unused |
| `ucs2` | Carrion Beetle (Red - Anub'Arak)(Level 2) | unit | stock |  |  | unused |
| `ucs3` | Dire Carrion Beetle (Red - Anub'Arak)(Level 3) | unit | stock |  |  | unused |
| `uplg` | Disease Cloud | unit | stock |  |  | unused |

## 24p map: not in the main map

Status `24p only` = the id is free in the main map; `different` = the id is taken by ANOTHER object in the main map. Copies get a new `?Za?` id. (`changed` = the same object with other data: only in units.tsv.)

### commoner (7)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `h02B` | Arcane Phantom (Khadgar) | unit | 24p |  |  | 24p only |
| `h05M` | Can not be cast while moving | unit | missing |  |  | 24p only |
| `h06L` | Dummy Nuke Warning | unit | 24p |  |  | 24p only |
| `h050` | Dummy Nuke leftover | unit | missing |  |  | 24p only |
| `h05B` | ForestSpirit Dummy | unit | stock |  |  | 24p only |
| `h03C` | Lightning Bolt (illidan cine) | unit | 24p |  |  | different |
| `h043` | WaveCrash | unit | 24p |  |  | 24p only |

### creeps (71)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `Nal2` | Alchemist (Morph level 2) | hero | stock |  |  | 24p only |
| `Nal3` | Alchemist (Morph level 3) | hero | stock |  |  | 24p only |
| `Nalm` | Alchemist (Morph level 1) | hero | stock |  |  | 24p only |
| `Npbm` | Brewmaster | hero | stock |  |  | 24p only |
| `H00E` | Centaur Khan | hero | stock |  |  | 24p only |
| `O00O` | Chieftan of the Amani Tribe (Creep-Zul'Aman) | hero | main |  |  | 24p only |
| `O024` | Chieftan of the Sandfury Tribe (Creep-Zul'Farrak) | hero | stock |  |  | 24p only |
| `O025` | Guardian of the Caverns (Creep-Caverns) | hero | stock |  |  | 24p only |
| `O02C` | High Prophet of Akali (Creep-Zul'Drak) | hero | stock |  |  | 24p only |
| `N089` | Murloc Sorcerer | hero | stock |  |  | 24p only |
| `Nmsr` | Murloc Sorcerer | hero | stock |  |  | 24p only |
| `Nngs` | Sea Witch | hero | missing |  |  | 24p only |
| `O02L` | Sea Witch (Creep) | hero | stock |  |  | 24p only |
| `O023` | Witch Doctor of the Gurubian Tribe (Creep-Zul'Gurub) | hero | main |  |  | 24p only |
| `nfgo` | Forgotten One (Creep) | building | stock |  |  | 24p only |
| `ncop` | Tavern Selector | building | stock |  |  | 24p only |
| `nsqa` | Ancient Sasquatch | unit | stock |  |  | 24p only |
| `nane` | Arachnathid Earth-borer | unit | stock |  |  | 24p only |
| `nanm` | Barbed Arachnathid | unit | stock |  |  | 24p only |
| `nadk` | Blue Drake | unit | stock |  |  | 24p only |
| `n0A5` | Bronze Dragon Whelp | unit | stock |  |  | 24p only |
| `nbzk` | Bronze Drake | unit | stock |  |  | 24p only |
| `ncat` | Draenei Demolisher | unit | stock | ndh3 |  | 24p only |
| `ndrp` | Draenei Protector | unit | stock |  |  | 24p only |
| `nbds` | Dragonspawn Sorcerer | unit | stock |  |  | 24p only |
| `nsqe` | Elder Sasquatch | unit | stock |  |  | 24p only |
| `nerw` | Eredar Warlock | unit | stock |  |  | 24p only |
| `nfot` | Faceless One Terror | unit | stock |  |  | 24p only |
| `nfor` | Faceless One Trickster | unit | stock |  |  | 24p only |
| `nrvs` | Frost Revenant | unit | stock |  |  | 24p only |
| `ngsp` | Goblin Sapper (Goblin Shop) | unit | stock |  |  | 24p only |
| `nsw3` | Greater Spirit Beast (Level 3) | unit | stock |  |  | 24p only |
| `ngrw` | Green Dragon Whelp | unit | stock |  |  | 24p only |
| `ngdk` | Green Drake | unit | stock |  |  | 24p only |
| `nmit` | Icetusk Mammoth | unit | stock |  |  | 24p only |
| `n04R` | Immol'thar | unit | 24p |  |  | different |
| `ninm` | Infernal Machine | unit | stock |  |  | 24p only |
| `nsw1` | Lesser Spirit Beast (Level 1) | unit | stock |  |  | 24p only |
| `nmgw` | Magnataur Warrior | unit | stock |  |  | 24p only |
| `nndr` | Nether Dragon | unit | stock |  |  | 24p only |
| `nnht` | Nether Dragon Hatchling | unit | stock |  |  | 24p only |
| `nndk` | Nether Drake | unit | stock |  |  | 24p only |
| `n0AB` | Occulus | unit | stock |  |  | 24p only |
| `nbzd` | Occulus | unit | stock |  |  | 24p only |
| `nogr` | Ogre Warrior | unit | stock |  |  | 24p only |
| `nfpc` | Polar Furbolg Champion | unit | stock |  |  | 24p only |
| `nfpt` | Polar Furbolg Tracker | unit | stock |  |  | 24p only |
| `nqbh` | Quillboar Hunter | unit | stock |  |  | 24p only |
| `nrzg` | Razormane Chieftain | unit | stock |  |  | 24p only |
| `nrdk` | Red Dragon Whelp | unit | stock |  |  | 24p only |
| `n08Y` | SG Dummy | unit |  |  |  | 24p only |
| `nsqt` | Sasquatch | unit | stock |  |  | 24p only |
| `nsqo` | Sasquatch Oracle | unit | stock |  |  | 24p only |
| `ngir` | Shredder (Goblin Shop) | unit | stock | ngad |  | 24p only |
| `nsca` | Skeleton Archer | unit | stock |  |  | 24p only |
| `ngz3` | Spirit Bear (Level 3) | unit | stock |  |  | 24p only |
| `nsw2` | Spirit Beast (Level 2) | unit | stock |  |  | 24p only |
| `nspp` | Spirit Pig | unit | stock |  |  | 24p only |
| `nsra` | Stormreaver Apprentice | unit | stock |  |  | 24p only |
| `nsrh` | Stormreaver Hermit | unit | stock |  |  | 24p only |
| `nsrw` | Stormreaver Warlock | unit | stock |  |  | 24p only |
| `nlps` | Summoned Prawn | unit | stock |  |  | 24p only |
| `nbot` | Transport Ship | unit | stock | nshp |  | different |
| `nubk` | Unbroken Darkhunter | unit | stock |  |  | 24p only |
| `nubw` | Unbroken Darkweaver | unit | stock |  |  | 24p only |
| `nubr` | Unbroken Rager | unit | stock |  |  | 24p only |
| `h00V` | Water Elemental (Level 3 Custom) | unit | stock |  |  | 24p only |
| `h00W` | Water Elemental (Level 2 Custom) | unit | stock |  |  | 24p only |
| `nsns` | Watery Minion (Level 3) | unit | stock |  |  | 24p only |
| `ntws` | Watery Minion (Level 2) | unit | stock |  |  | 24p only |
| `nwns` | Wendigo Shaman | unit | stock |  |  | 24p only |

### critters (12)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `n0D2` | Banana - Tier 4 Patron Patrons Tier 4 1 | unit | stock |  |  | 24p only |
| `n0CS` | Mr. Cash - Tier 2 Patron | unit | stock |  |  | 24p only |
| `ndog` | Patrons Crusader (Tier 3 Patron) | unit | stock |  |  | 24p only |
| `nhmc` | Patrons Footman (Tier 1 Patron) | unit | stock |  |  | 24p only |
| `nfro` | Patrons Knight (Tier 2 Patron) | unit | stock |  |  | 24p only |
| `npig` | Patrons Tier 3 1 | unit | main |  |  | 24p only |
| `n04D` | Skyguy the Murloc King - Tier 4 Patron Patrons Tier 4 1 | unit | stock |  |  | 24p only |
| `n0CR` | Spore - Tier 3 Patron Patrons Tier 3 2 | unit | stock |  |  | 24p only |
| `n0CT` | Talinn - Tier 4 Patron Patrons Tier 4 1 | unit | stock |  |  | 24p only |
| `n0D5` | hllh (Tier 1 Patron) | unit | 24p |  |  | 24p only |
| `n0BR` | 彼岸归航 - Tier 3 Patron Patrons Tier 3 2 | unit | stock |  |  | 24p only |
| `n0BS` | 神已堕落 - Tier 4 Patron Patrons Tier 4 1 | unit | stock |  |  | 24p only |

### demon (4)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `nba2` | Doom Guard (Summoned) | unit | stock |  |  | 24p only |
| `n0CI` | Zelemar the Wrathful (2) | unit | stock |  |  | different |
| `n0CJ` | Zelemar the Wrathful (1) | unit | stock |  |  | different |
| `n0CK` | Zelemar the Wrathful (3) | unit | stock |  |  | different |

### human (161)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `Hamg` | Archmage | hero | stock |  |  | 24p only |
| `H00Y` | Commander of the Scarlet Crusade (Rise from the ashes) | hero | main | h080 |  | different |
| `E01H` | Defias Kingpin (Mercenary) | hero | 24p | n0D8 |  | 24p only |
| `E004` | Dragon form (Dragon Form) | hero | stock |  |  | 24p only |
| `E00O` | High Commander (garithos) | hero | 24p |  |  | 24p only |
| `H04Q` | Leader of the Explorers' League (Ironforge Vassal) | hero | stock |  |  | 24p only |
| `H04A` | Lord of the Nexus (Nexus) | hero | 24p |  |  | 24p only |
| `U00W` | Majordomo of Ragnaros (Ironforge) | hero | 24p | h07B |  | 24p only |
| `H0A2` | Monastery Commander (Scarlet) | hero | main | h080 |  | different |
| `Ewar` | Princess of Gilneas (gilneas) | hero | main | h02X |  | 24p only |
| `H08G` | Scarlet Crusader Lord (Scarlet) | hero | main | h080 |  | different |
| `H09Z` | Supreme Commander (Argent) | hero | 24p | h080 |  | 24p only |
| `H04K` | The Dragonheart (Dalaran Vassal) | hero | main |  |  | 24p only |
| `O038` | Worgen Blood Shaman (worgen Hero) | hero | 24p |  |  | 24p only |
| `h076` | Alliance Shipyard (dalaran) | building | main | h022,h09C | h060,hbot,hbsh,hdes | different |
| `h077` | Alliance Shipyard (high elves) | building | 24p | nbee | h060,hbot,hbsh,hdes | different |
| `hshy` | Alliance Shipyard (Lordaeron) | building | stock | hpea | h060,hbot,hbsh,hdes | different |
| `h080` | Altar of Harvest (Scarlet) | building | 24p | hpea | H00Y,H08G,H09Z,H0A2,h05W,h09H | 24p only |
| `haro` | Arcane Observatory | building | stock |  |  | 24p only |
| `h06C` | Aviary (Lordaeron) | building | stock | hpea | h02Q,nwe2 | different |
| `h07C` | Barracks (Ironforge) | building | main | h019 | h00A,h00B,h014,h018,h01B,h01C,h029,h02O,h032,h033,h03K,h03U,+14 | different |
| `h081` | Barracks (Scarlet) | building | 24p | hpea | h08I,h08J,h08L,h09I,h09J,h0A3 | 24p only |
| `h005` | Barricades (Tinker Barricades) | building | stock | h06D,h07D |  | 24p only |
| `h0AL` | Black Rook Hold (Neutral Hostile) | building | 24p |  | ewsp,n06E | different |
| `h06G` | Blacksmith (Scarlet) | building | 24p | hpea |  | different |
| `h09B` | Blue Dragon Roost (Dalaran) | building | stock | h09C | n09T,n0AC,u025 | 24p only |
| `h07G` | Castle (Ironforge) | building | main |  | h00F,h019,h01L,h022,h03S,h05F,h06B,h06D,h0A8,hpea,n007,n00A,+1 | different |
| `h07Z` | Castle (Scarlet) | building | 24p |  | h06B,hpea,o00X | 24p only |
| `h035` | Chapel (Lordaeron) | building | 24p | hpea | hcth,n03K,nchp | different |
| `h083` | Church (Scarlet) | building | 24p | hpea | h08K,n068,n09N | 24p only |
| `ndt1` | Cold Tower | building | stock |  |  | 24p only |
| `h09O` | Crimson Castle (Scarlet) | building | 24p |  | h08M,h095,h096 | different |
| `ntt1` | Death Tower | building | stock |  |  | 24p only |
| `h09A` | Draconian Spire (Dalaran) | building | main | h09C | h099,n0A4,n0AK | 24p only |
| `net1` | Energy Tower | building | stock |  |  | 24p only |
| `h02F` | Farm (Gilneas) | building | main | hpea |  | different |
| `h072` | Farm (stormwind) | building | 24p | hpea |  | different |
| `h07N` | Farm (Kul tiras) | building | main | h01E |  | different |
| `h088` | Farm (Scarlet) | building | 24p | hpea |  | 24p only |
| `nft1` | Flame Tower | building | stock |  |  | 24p only |
| `h09D` | Fleetmaster's Table | building | stock |  |  | 24p only |
| `nfrt` | Fruit Stand | building | stock |  |  | 24p only |
| `h03Q` | Garrison (Gilneas) | building | main | hpea | h03L,h04E,o04U | different |
| `h04G` | Gilneas City Building (Creep - Gilneas Big) | building | stock |  |  | 24p only |
| `h04H` | Gilneas City Building (Creep - Gilneas Big) | building | stock |  |  | 24p only |
| `h059` | Gilneas City Building (Creep - Gilneas) | building | stock |  |  | 24p only |
| `h03P` | Gnomish House | building | stock |  |  | different |
| `ngwr` | Grain Warehouse | building | stock |  |  | 24p only |
| `h020` | Greymane Manor | building | stock |  | h00H,h019,h022,hpea,nbee,uaco | 24p only |
| `h097` | Guard Post (Scarlet) | building | 24p | h08M,hpea | h08M | 24p only |
| `h085` | Guard Tower (Scarlet) | building | 24p |  |  | 24p only |
| `h048` | Guild (Ironforge) | building | main | h019 |  | different |
| `nef3` | High Elven Farm 4 | building | stock |  |  | 24p only |
| `ndt2` | Improved Cold Tower | building | stock |  |  | 24p only |
| `ntx2` | Improved Death Tower | building | stock |  |  | 24p only |
| `net2` | Improved Energy Tower | building | stock |  |  | 24p only |
| `nft2` | Improved Flame Tower | building | stock |  |  | 24p only |
| `h087` | Improved Guard Tower (Scarlet) | building | 24p |  |  | 24p only |
| `h07F` | Keep (Ironforge) | building | main |  | h00F,h019,h01L,h022,h03S,h05F,h06B,h06D,h0A8,hpea,n007,n00A,+1 | different |
| `h07Y` | Keep (Scarlet) | building | 24p |  | h06B,hpea,o00X | 24p only |
| `h067` | Laboratory (Dalaran) | building | main | h022 | n096,n0AC,n0AD,o02U | different |
| `h057` | Light's Dawn Cathedral (Gilneas) | building | stock | h019 | hpea,o01V | different |
| `h09N` | Palisade Fort (Scarlet) | building | 24p |  | h08M,h095,h096 | 24p only |
| `o003` | Portable Turret (engineer) | building | main |  |  | different |
| `h09Q` | Royal Fortress (Scarlet) | building | 24p |  | h08M,h095,h096 | different |
| `n07T` | Royal Treasury (Stormwind) | building | 24p | hpea |  | different |
| `h08R` | Scarlet Halls (Scarlet) | building | stock |  | h06B,hpea,o00X | 24p only |
| `h09P` | Scarlet Keep (Scarlet) | building | 24p |  | h08M,h095,h096 | different |
| `h084` | Scout Tower (Scarlet) | building | 24p | h08M,hpea |  | 24p only |
| `h094` | Siege Camp (lord) | building | 24p | hpea | e017,o02F | different |
| `h06A` | Smitty (stormwind) | building | 24p | hpea | h014,h03K,h03U,h03Z,n06N | different |
| `h07E` | Town Hall (Ironforge) | building | main | h019 | h00F,h019,h01L,h022,h03S,h05F,h06B,h06D,h0A8,hpea,n007,n00A,+1 | different |
| `h07X` | Town Hall (Scarlet) | building | 24p | h08M,hpea | h06B,hpea,o00X | different |
| `h02N` | Trade Quarter (Dalaran) | building | 24p | h022,h09C |  | different |
| `n008` | Tradehouse (Gilneas) | building | main | hpea |  | different |
| `n07H` | Tradehouse (Kul Tiras) | building | main | h01E |  | different |
| `h09M` | Training Camp (Scarlet) | building | 24p |  | h08M,h095 | 24p only |
| `h03E` | Training Hall (Gilneas) | building | main | hpea | n06L,n06Q,n06Z | different |
| `n07U` | Treasure Vault (Khaz Modan) | building | main | h019 |  | different |
| `n0AO` | Way Gate (Dalaran) | building | stock | h022,h09C |  | 24p only |
| `h082` | Workshop (Scarlet) | building | 24p | hpea | h09X,n07N,o04C | 24p only |
| `n07N` | Airship (scarlet) | unit | 24p | h082 |  | different |
| `hbot` | Alliance Transport Ship (Alliance) | unit | stock | h03H,h06D,h076,h077,h07D,h07W,hshy |  | different |
| `h0A6` | Anvilrage Guardsman (Ironforge) | unit | 24p | h07C |  | 24p only |
| `h08J` | Arbalest (scarlet) | unit | 24p | h081 |  | 24p only |
| `h01M` | Baelgun Flamebeard (Khaz Modan-Demi) | unit | 24p | h07B |  | 24p only |
| `hbsh` | Battleship (Alliance) | unit | stock | h03H,h06D,h076,h077,h07D,hshy |  | 24p only |
| `h0A7` | Bear Rider (Ironforge) | unit | 24p | h07C |  | 24p only |
| `n09S` | Blood Elven Ranger (Quel'Thalas) | unit | 24p | h033,h03S,h03T,n05X |  | different |
| `o04C` | Bombard (scarlet) | unit | 24p | h082 |  | different |
| `h095` | Bowman (scarlet) | unit | 24p | h09M,h09N,h09O,h09P,h09Q |  | 24p only |
| `h012` | Captain Falric (Lordaeron-Demi) | unit | stock | halt |  | 24p only |
| `h0A8` | Chieftain (Ironforge-Elite) | unit | 24p | h01O,h07E,h07F,h07G |  | 24p only |
| `h09J` | Crossbowman (scarlet) | unit | 24p | h081 |  | different |
| `h03W` | Danath Trollbane (Arathor-Demi) | unit | stock | h06T |  | different |
| `h009` | Dark Knight (Garithos) | unit | 24p | hcas,hkee,htow |  | 24p only |
| `hprt` | Dark Portal | unit | stock |  |  | 24p only |
| `h018` | Defender (Ironforge) | unit | main | h07C |  | different |
| `hdes` | Destroyer (Alliance) | unit | stock | h03H,h06D,h076,h077,h07D,h07W,hshy |  | 24p only |
| `nccd` | Diablo Cart | unit | stock |  |  | 24p only |
| `nws1` | Dragon Hawk | unit | stock |  |  | 24p only |
| `nccr` | Dwarf Cart | unit | stock |  |  | 24p only |
| `h05R` | Effect (Thunder clap) | unit | stock |  |  | 24p only |
| `h06O` | Effect (Consecration) | unit | missing |  |  | 24p only |
| `h06P` | Effect (Unholy Defense) | unit | stock |  |  | 24p only |
| `n02F` | Felblood Warlock (Quel'thalas) | unit | main | n0A2 |  | different |
| `n0CH` | Flame Keeper (Ironforge) | unit | 24p | h042 |  | 24p only |
| `h014` | Forward-Marshal (Stormwind-Offensive) | unit | stock | h06A,h07C |  | different |
| `h05W` | Grand Inquisitor Isilien (scarlet) | unit | 24p | h080 |  | 24p only |
| `n06Z` | Gunship (Gilneas) | unit | main | h03E |  | different |
| `hgyr` | Gyrocopter (Ironforge) | unit | stock | harm |  | 24p only |
| `h09I` | Halbardier (Argent) | unit | 24p | h081 |  | different |
| `n049` | Hawk (Alliance) | unit | stock |  |  | different |
| `nhef` | High Elf (Female) | unit | stock |  |  | 24p only |
| `nhem` | High Elf (Male) | unit | stock |  |  | 24p only |
| `h05X` | High Sorcerer Andromath (Arathor-Demi) | unit | stock | h06T |  | different |
| `h00B` | Irondrake (Ironforge) | unit | 24p | h07C |  | 24p only |
| `njks` | Jailor Kassan (Dalaran-Demi) | unit | 24p | h044 |  | 24p only |
| `n06F` | Katrana Prestor (Arathor-Demi) | unit | stock |  |  | different |
| `h0A3` | Lancer (argent) | unit | 24p | h081 |  | different |
| `n05P` | Land Mine | unit | stock |  |  | 24p only |
| `h096` | Light Cavalry (scarlet) | unit | 24p | h09N,h09O,h09P,h09Q |  | 24p only |
| `h0AA` | Magma Tank (Ironforge) | unit | 24p | harm |  | 24p only |
| `h08M` | Man-at-arms (scarlet) | unit | 24p | h097,h09M,h09N,h09O,h09P,h09Q | h07X,h084,h097 | 24p only |
| `nmdm` | Medivh (Raven Form) | unit | stock |  |  | 24p only |
| `nmed` | Medivh, The Last Guardian (Dalaran) | unit | stock |  |  | 24p only |
| `h09X` | Mounted Archer (Scarlet) | unit | 24p | h082 |  | different |
| `n07G` | Musketeers (Kul tiras) | unit | 24p | h010,h046,h062,h064,h06I |  | different |
| `h06J` | Navy Snipers (Kul Tiras) | unit | 24p | h06R |  | different |
| `ncco` | Orc Cart | unit | stock |  |  | 24p only |
| `h092` | Order Inquisitor (Lordaeron) | unit | 24p | h093 |  | 24p only |
| `h04C` | Pikeman (Stormwind) | unit | 24p | h05E,h05Z,h06E,h07C |  | different |
| `n09P` | Portal (Stormwind) | unit | stock |  |  | 24p only |
| `h05V` | Proudmoore Flagship (Kul'tiras) | unit | 24p | h07C |  | 24p only |
| `h08T` | Proudmoore Flagship (landed) | unit | 24p |  |  | 24p only |
| `h03F` | Reginald Windsor (Arathor-Demi) | unit | stock | h06T |  | different |
| `hhdl` | Riderless Horse | unit | stock |  |  | 24p only |
| `n06Q` | Royal Guard (Gilneas) | unit | 24p | h03E |  | different |
| `hrrh` | Runner High Elf | unit | stock |  | halt,harm,hars,hbar,hbla,hgra,hhou,hlum,htow,hvlt,hwtw | 24p only |
| `h03Z` | Saboteur (Arathor) | unit | stock | h06A |  | 24p only |
| `h09H` | Scarlet Champion Herod (Scarlet) | unit | 24p | h080 |  | different |
| `nser` | Searinox | unit | stock |  |  | 24p only |
| `h0AB` | Shadowcaster (Ironforge) | unit | 24p | h042 |  | 24p only |
| `n0CC` | Shadowforge Gatebreaker (Ironforge) | unit | 24p | harm |  | different |
| `n0CM` | Shadowspawn | unit | stock |  |  | 24p only |
| `hmtt` | Siege Engine | unit | stock |  |  | 24p only |
| `hrtt` | Siege Engine (Barrage) | unit | stock |  |  | 24p only |
| `o036` | Stun Mine (Dwarf) | unit | stock |  |  | 24p only |
| `e00B` | Thornspeaker (Bear Form) | unit | 24p | h07Q |  | different |
| `n009` | Tide Revenant (Kul'tiras) | unit | stock |  |  | different |
| `h03R` | Tinkerer (Ironforge) | unit | stock |  |  | 24p only |
| `nccu` | Undead Cart | unit | stock |  |  | 24p only |
| `n075` | Vereesa Windrunner (Quel'Thalas-Demi) | unit | stock | h01H |  | 24p only |
| `hbew` | Wagon | unit | stock |  |  | 24p only |
| `nwar` | War Wagon | unit | stock |  |  | 24p only |
| `h04J` | Warship (Kul'tiras) | unit | 24p | h07W |  | 24p only |
| `hwat` | Water Elemental (Level 1) | unit | stock |  |  | 24p only |
| `hwt2` | Water Elemental (Dalaran-Jaina) | unit | stock |  |  | 24p only |
| `n057` | Water Elemental Sentry | unit | stock |  |  | different |
| `h09C` | Whelp (Dalaran) | unit | 24p | h04P,h065,h066,h068 | h02N,h036,h044,h063,h065,h076,h078,h09A,h09B,hvlt,n0AO | 24p only |
| `nhew` | Worker (Blood Elf) | unit | stock |  | halt,hars,hbar,hbla,hhou,hlum,htow,hvlt,hwtw,nnsa,nnsg | 24p only |

### naga (85)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `Eevi` | Betrayer (Naga) | hero | stock |  |  | different |
| `U00P` | C'thrax Aberration | hero | main | n0AV |  | different |
| `E015` | Champion of the Illidari (Naga) | hero | 24p | nnad |  | 24p only |
| `H08U` | Empress of Nazjatar (Nzoth) | hero | 24p | n0C0 |  | 24p only |
| `U02M` | Grimscale Chieftain (Naga) | hero | 24p | n0D8 |  | 24p only |
| `U02A` | N'raqi Aberration (yogg) | hero | main | u01F |  | 24p only |
| `E01D` | N'raqi Arcanist (yogg) | hero | main | n0AV |  | different |
| `H0A5` | Sea Witch (Nzoth) | hero | 24p | n0C0 |  | 24p only |
| `Eill` | The Betrayer (Naga) | hero | 24p |  |  | 24p only |
| `U02N` | The Leader of Bloodcursed Naga (Naga) | hero | 24p | n0C0 |  | 24p only |
| `U02B` | Yogg-Saron Champion | hero | main | n0AV |  | different |
| `n0C0` | Altar of the Depths (Nzoth) | building | stock | nmpe | H08U,H0A5,U02N | 24p only |
| `n0A3` | Coilfang Archive (Illidari) | building | 24p | nmpe | nmsc,nnsu,nnsw | different |
| `n0C1` | Coral Bed (Nzoth) | building | stock | nmpe |  | 24p only |
| `h0A4` | Coral Forge (Nzoth) | building | 24p | nmpe |  | 24p only |
| `n0AW` | Creation Hive (Yogg) | building | main | n0B5 | o01G,o04V,u029 | different |
| `n0BZ` | Deep Sea Vault (nzoth) | building | 24p | nmpe |  | 24p only |
| `n08V` | Depth Void Portal (Nzoth) | building | 24p | nmpe | h01Q,n0CO | 24p only |
| `ndh4` | Draenei Haven (Green Draenei) | building | stock | ndrl | ndrn,ndrs,ndrt,ndsa | 24p only |
| `nico` | Hero's Obelisk | building | stock |  |  | 24p only |
| `o01I` | Illidan Path Selector | building | stock |  |  | different |
| `n08H` | Illidari Proving Grounds (Illidari) | building | 24p | nmpe | e00S,h08W | 24p only |
| `n0C6` | Improved Tidal Guardian (Nzoth) | building | stock |  |  | 24p only |
| `n0B3` | Library of Forbidden Knowledges (Yogg) | building | 24p | n0B5 | n077,o04Y,u02F | different |
| `n08W` | Lost One Den (Illidari) | building | main | nmpe | ndrn,ndrs | different |
| `n0C8` | Monument of Currents (Nzoth) | building | 24p |  | h02P,nmpe | 24p only |
| `n0C2` | Pillar of Waves (Nzoth) | building | 24p | nmpe | h02P,nmpe | 24p only |
| `n0AX` | Prison of Unspeakable Horrors (Yogg) | building | main | n0B5 | h09F,n0AH,o04Z | different |
| `n0B6` | Pyramid of Madness (black Empire) | building | main |  | n0B4,n0B5,u02E | different |
| `n0C7` | Royal Pyramid (Nzoth) | building | 24p | nmpe | n0CB,n0CD | 24p only |
| `nnsa` | Serpentshrine (Illidari) | building | 24p | nhew,nmpe | e00Y,nwgs | different |
| `n0C3` | Shrine of Azshara (Nzoth) | building | stock | nmpe | nwgs,u02J | 24p only |
| `n0C4` | Spawning Grounds (Nzoth) | building | stock | nmpe | n0CA,n0CE,n0CL | 24p only |
| `nnsg` | Spawning Pit (Illidari) | building | 24p | nhew,nmpe | nhyc,nmyr,nsnp | different |
| `h06S` | Steamforge (Illidari) | building | 24p | nmpe |  | different |
| `n055` | Steamwork Citadel (Illidari) | building | 24p |  | h0AC,nmpe,nnrg | different |
| `nntt` | Steamwork Reservoir (Illidari) | building | 24p | nmpe | h0AC,nmpe,nnrg | different |
| `n04T` | Steamwork Spire (Illidari) | building | 24p |  | h0AC,nmpe,nnrg | different |
| `n045` | Temple of Azshara (Nzoth) | building | stock |  | h02P,nmpe | 24p only |
| `n0C9` | Temple of Tides (Nzoth) | building | 24p |  | h02P,nmpe | 24p only |
| `n0BQ` | The First Guardian (Nzoth) | building | stock |  |  | 24p only |
| `n0C5` | Tidal Guardian (Nzoth) | building | stock | nmpe |  | 24p only |
| `n0AR` | Twisted Halls (Yogg) | building | main | n0B5 | n0B4,n0B5 | different |
| `n05A` | Vault of the Eternal Palace (Illidari) | building | 24p |  | h0AC,nmpe,nnrg | different |
| `n0AS` | Whispering Chambers (Yogg) | building | main |  | n0B4,n0B5 | different |
| `n0CD` | Bloodkin (Nzoth) | unit | stock | n0C7 |  | 24p only |
| `u01E` | Burrowed Giant Scarab (C'thun Giant Scarab) | unit | stock |  |  | 24p only |
| `n073` | CthunTentacle | unit | main | u019 |  | different |
| `n0CB` | Deepseer (Nzoth) | unit | 24p | n0C7 |  | 24p only |
| `ndrs` | Draenei Seer (Illidari) | unit | stock | n08W,ndh4 |  | different |
| `ndrn` | Draenei Vindicator (Illidari) | unit | stock | n08W,ndh3,ndh4 |  | different |
| `h01N` | Faceless Corrupter (C'thun Faceless Corruptor) | unit | stock | u01H |  | different |
| `o01G` | Faceless Macemen (yogg) | unit | stock | n0AW,o02W |  | different |
| `u02F` | Forgotten one (yogg) | unit | stock | n0B3 |  | different |
| `n0CL` | Gargantuan Sea Turtle (Nzoth) | unit | stock | n0C4 |  | 24p only |
| `u013` | Giant Scarab (C'thun Giant Scarab) | unit | stock | o00R |  | different |
| `h09F` | Gladiator (yogg) | unit | main | n0AX |  | different |
| `e00S` | Glaive Warrior (Sentinels) | unit | 24p | n08H |  | 24p only |
| `n0CE` | Hatchling (Nzoth) | unit | stock | n0C4 |  | 24p only |
| `h01Q` | Immortal Guardian (Nzoth) | unit | 24p | n08V |  | 24p only |
| `nmsc` | Mur'gul Shadowcaster (Illidari) | unit | stock | n0A3 |  | different |
| `nmpe` | Mur'gul Slave (Illidari, Nzoth, Worker) | unit | stock | n045,n04T,n055,n05A,n0C2,n0C8,n0C9,nntt | h06S,h0A4,n08H,n08V,n08W,n0A3,n0BZ,n0C0,n0C1,n0C2,n0C3,n0C4,+9 | different |
| `n0CA` | Naga Incursor (nzoth) | unit | main | n0C4 |  | 24p only |
| `nmyr` | Naga Myrmidon (Illidari) | unit | stock | nnsg |  | different |
| `nnrg` | Naga Royal Guard (Illidari) | unit | stock | n04T,n055,n05A,nntt |  | different |
| `h02P` | Naga Sea Witch (Nzoth Elite) | unit | stock | n045,n0C2,n0C8,n0C9 |  | different |
| `h0AC` | Naga Sea Witch (illidan) | unit | stock | n04T,n055,n05A,nntt |  | different |
| `nnsw` | Naga Siren (Illidari) | unit | stock | n0A3 |  | different |
| `h01K` | Scorpid Overlord (C'thun Overlord) | unit | stock | u01H |  | different |
| `o000` | Silithid Colossus (C'thun Elites) | unit | main | ncte,nctl,u020,u021,u022 |  | different |
| `o00L` | Silithid Reaver (C'thun Silithid Reaver) | unit | 24p | o00R |  | different |
| `n060` | Silithid Tunneler (C'thun Silithid Tunneler) | unit | stock | o00D |  | different |
| `n06I` | Silithid Warrior (C'thun Silithid Warrior) | unit | 24p | o00R |  | different |
| `o02N` | Silithid Wasp (cthun) | unit | 24p | u01H |  | different |
| `n077` | Sorcerer (yogg) | unit | main | n0B3 |  | different |
| `o04V` | Soul Hunter (yogg) | unit | main | n0AW |  | different |
| `nmys` | Submerged Naga Myrmidon (Green - Naga) | unit | stock |  |  | 24p only |
| `nnrs` | Submerged Naga Royal Guard (Green - Naga) | unit | stock |  |  | 24p only |
| `nsbs` | Submerged Snap Dragon (Green - Naga) | unit | stock |  |  | 24p only |
| `nnsu` | Summoner (Illidari) | unit | stock | n0A3 |  | different |
| `nfgt` | Tentacle | unit | stock |  |  | 24p only |
| `u02J` | Tidal Terror (Nzoth) | unit | 24p | n0C3 |  | 24p only |
| `n0CO` | Trench Hydra (nzoth) | unit | stock | n08V |  | 24p only |
| `n0B7` | YoggTentacle | unit | 24p |  |  | 24p only |
| `n0B8` | YoggTentacleMace | unit | 24p |  |  | 24p only |

### nightelf (77)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `E001` | Archdruid of Darnassus (Druids-Bear Form) | hero | stock |  |  | 24p only |
| `E00K` | Archdruid of Darnassus (Druids) | hero | stock |  |  | 24p only |
| `E00D` | Demon Hunter (Morphed Level 3) | hero | 24p |  |  | 24p only |
| `E00E` | Demon Hunter (Morphed Level 2) | hero | 24p |  |  | 24p only |
| `E00F` | Demon Hunter (Evil Morphed Level 2) | hero | stock |  |  | different |
| `E00G` | Demon Hunter (Evil Morphed Level 3) | hero | stock |  |  | 24p only |
| `Edmm` | Demon Hunter (Demon Form) | hero | stock |  |  | 24p only |
| `Eidm` | Demon Hunter (Demon Form) | hero | stock |  |  | 24p only |
| `Eilm` | Demon Hunter (Morphed) | hero | 24p |  |  | 24p only |
| `O02E` | Guard Captain (Sentinel Vassal) | hero | 24p | e00R |  | 24p only |
| `Ekgg` | Keeper of the Grove (Creep) | hero | stock |  |  | 24p only |
| `H0AK` | The Naaru (Draenei) | hero | main | o058 |  | 24p only |
| `ncte` | Corrupted Tree of Eternity | building | stock |  | n067,o000 | 24p only |
| `h04X` | Darkshore Runestone (Sentinels) | building | stock | h06D,h07D |  | different |
| `o05U` | Defias Hideout (defias) | building | 24p |  | h0AI,n0DC,o05W,o05X | different |
| `ndh3` | Draenei Barracks | building | stock |  | ncat,ndrn,ndrt,ndsa | 24p only |
| `egol` | Entangled Gold Mine | building | stock |  |  | 24p only |
| `o05E` | Exodar Regalis (Draenei) | building | 24p |  | h09R,o05A | different |
| `h0AF` | Farahlon (Draenei) | building | 24p |  | o05A,o05D | different |
| `o05T` | Gnoll Kennel (gnoll) | building | 24p |  | h0AJ,o05V,o05Y,o05Z | different |
| `h03N` | Improved Runestone (Sentinels) | building | stock |  |  | different |
| `o05R` | Murloc Gathering (murloc) | building | 24p |  | n0DD,n0DE,o061,o062 | 24p only |
| `nvr0` | Night Elf Fishing Village (ruined, two story) | building | stock |  |  | 24p only |
| `nvr1` | Night Elf Fishing Village (ruined, one story) | building | stock |  |  | 24p only |
| `nvr2` | Night Elf Fishing Village (ruined, crested) | building | stock |  |  | 24p only |
| `n07I` | Residence (Sentinels-Druids) | building | stock |  |  | 24p only |
| `nfv0` | Residence (Two Story-Druids-Sentinels) | building | stock |  |  | 24p only |
| `nfv1` | Residence (Crested-Druids-Sentinels) | building | stock |  |  | 24p only |
| `nfv2` | Residence (Sentinels-Druids) | building | stock |  |  | 24p only |
| `nfv3` | Residence (Two Story-Druids-Sentinels) | building | stock |  |  | 24p only |
| `nfv4` | Residence (Crested-Druids-Sentinels) | building | stock |  |  | 24p only |
| `h03M` | Runestone (Sentinels) | building | stock | ewsp |  | different |
| `h0AG` | Telredor (Draenei) | building | main |  | o05A,o05D | different |
| `h09W` | The Exodar | building | stock |  |  | different |
| `h0AD` | Tuurem (Draenei) | building | 24p |  | h09T,o05B | 24p only |
| `ngob` | World Tree Guardian Runestone | building | stock |  |  | 24p only |
| `o05W` | Axeman (bandit) | unit | stock | o05U |  | 24p only |
| `n0DC` | Bandit Mage (bandit) | unit | stock | o05U |  | 24p only |
| `o05X` | Bandit Spearman (bandit) | unit | stock | o05U |  | 24p only |
| `e00W` | Broll Bearmantle (Brown)(Bear Form) | unit | stock |  |  | different |
| `n006` | Charred Ancient (Brown - Fandral Summon) | unit | 24p |  |  | different |
| `o061` | Cliffrunner (murloc) | unit | stock | o05R |  | 24p only |
| `edes` | Corvette (Druids-Sentinels) | unit | stock | e011,eshy,o059 |  | 24p only |
| `ebsh` | Cruiser (Druids-Sentinels) | unit | stock | e011,eshy,o059 |  | 24p only |
| `h0AI` | Defias Assassin (Arathor-Elite) | unit | main | o05U |  | 24p only |
| `h08W` | Demon Hunter (light Blue) | unit | stock | n08H |  | 24p only |
| `ndrm` | Draenei Disciple | unit | stock |  |  | 24p only |
| `ndrl` | Draenei Laborer | unit | stock |  | eshy,ndh4 | 24p only |
| `edcm` | Druid of the Claw (Bear Form) | unit | stock | eaoe |  | different |
| `e00X` | Elemental Guardian (Druids - Demi) | unit | stock | eate |  | 24p only |
| `n09W` | Elven Trap (Sentinels-Moonhunter) | unit | stock |  |  | 24p only |
| `n0CY` | Enraged Ancient of Lore (event) | unit | stock |  |  | different |
| `n0CX` | Enraged Ancient of War (event) | unit | stock |  |  | different |
| `n0CZ` | Enraged Ancient of Wind (event) | unit | stock |  |  | different |
| `n0D0` | Enraged Ancient of Wonder (event) | unit | stock |  |  | different |
| `n0D1` | Enraged Tree of Life (event) | unit | stock |  |  | different |
| `o05V` | Gnoll Brawler (gnoll) | unit | stock | o05T |  | 24p only |
| `o05Y` | Gnoll Crossbowman (bandit) | unit | stock | o05T |  | 24p only |
| `h0AJ` | Gnoll Overseer (gnoll) | unit | stock | o05T |  | 24p only |
| `o05Z` | Gnoll Warden (Gnoll) | unit | stock | o05T |  | 24p only |
| `n0D6` | Lightforged Dragoon (Draenei) | unit | main | o064 |  | different |
| `n0D7` | Lightforged Juggernaut (nexus) | unit | 24p | o064 |  | 24p only |
| `n0D9` | Lightforged Sharpshooter (Draenei) | unit | 24p | o064 |  | different |
| `n0DA` | Lightforged Warframe (Draenei) | unit | main |  |  | 24p only |
| `n04A` | Mangrove Ent (Cenarius) | unit | stock |  |  | 24p only |
| `h03I` | Moon Priestess Amara (sentinels - demi) | unit | stock | e00R |  | different |
| `o062` | Murloc Hunter (murloc) | unit | stock | o05R |  | 24p only |
| `n0DE` | Murloc Mutant (murloc) | unit | stock | o05R |  | 24p only |
| `n0DD` | Murloc Snarecaster (murloc) | unit | stock | o05R |  | 24p only |
| `e009` | Naisha (Sentinels-Demi) | unit | stock | e00R |  | 24p only |
| `ensh` | Naisha (UNUSED BECAUSE SCALING VALUE) | unit | stock |  |  | 24p only |
| `h08V` | Nightsaber Knight (sentinel) | unit | 24p | e00T |  | different |
| `now2` | Owl Scout (Sentinels-Tyrande) | unit | stock |  |  | 24p only |
| `e00U` | Poison Treant (Brown - Malfurion Summon) | unit | stock |  |  | 24p only |
| `e00I` | Spirit of Retribution (Sentinels-Nightblade on death) | unit | stock |  |  | 24p only |
| `e01F` | Treant (Druids - Event) | unit | stock | h09G |  | 24p only |
| `nssn` | Warden (Creep) | unit | stock |  |  | 24p only |

### orc (130)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `Nsjs` | Brewmaster (Warsong) | hero | stock | o020 |  | 24p only |
| `H06Q` | Demigod (Warsong) | hero | stock | o040 |  | 24p only |
| `O00Y` | Dragonmaw Chieftain (Fel Horde) | hero | 24p | o03C |  | 24p only |
| `O01Q` | Dragonmaw Chieftain (Fel Horde) | hero | 24p | o03C |  | 24p only |
| `Odrt` | Far Seer | hero | stock |  |  | 24p only |
| `O04H` | Fire Ascendant (twilight) | hero | main | o03C |  | different |
| `O026` | God King of the Zandalari Tribe (troll) | hero | 24p | o040 |  | 24p only |
| `E01G` | Gronn Overlord (Gilneas) | hero | 24p |  |  | 24p only |
| `Obla` | High Overlord of the Kor'kron (vassal) | hero | main | o020 |  | 24p only |
| `O01P` | Leader of the Twilight's Hammer (Twilight Hammer) | hero | main | o03C |  | 24p only |
| `O05S` | Overseer of Fenris Keep (gnoll) | hero | 24p | n0D8 |  | 24p only |
| `O05O` | Ragefire Warlock (Warsong) | hero | 24p | o020 |  | 24p only |
| `Ocb2` | Tauren Chieftain (Unused) | hero | stock |  |  | 24p only |
| `Otcc` | Tauren Chieftain (Cinematic Unused) | hero | stock |  |  | 24p only |
| `O01J` | The Dark Prophet (Troll) | hero | main | o040 |  | 24p only |
| `Nman` | The Destroyer (Warsong-Bloodpact) | hero | stock |  |  | different |
| `U023` | The Soulflayer (Legion) | hero | 24p |  |  | 24p only |
| `Osam` | Veteran of the Horde (Frostwolf-True Horde) | hero | stock |  |  | 24p only |
| `Ogld` | Warlock | hero | stock |  |  | 24p only |
| `o02A` | Alterac Watch Tower (real tower) | building | stock |  |  | different |
| `o065` | Beastiary (Frostwolf Specialist) | building | stock | opeo | h0AM,h0AN,otbr | 24p only |
| `o03T` | Burrow (goblin) | building | 24p | o02I |  | different |
| `o045` | Burrow (Zandalar) | building | 24p | o04A |  | different |
| `n05S` | Covenant of Witch Demon Gate (T2 Sister) | building | stock |  |  | 24p only |
| `o059` | Draenei Shipyard (Draenei) | building | main | o05A | ebsh,edes,etrs | different |
| `o02O` | Dragonmaw Fortification | building | stock |  |  | 24p only |
| `o00M` | Drakkari Tower | building | stock |  |  | 24p only |
| `o022` | Elder Rise | building | stock |  |  | 24p only |
| `n04I` | Eredar Witch Demon Gate (T1 Succubus) | building | stock |  |  | 24p only |
| `n0AJ` | Goblin Boulder Tower | building | stock |  |  | 24p only |
| `o03V` | Goblin Dock (Goblin) | building | stock | o02I | obot,odes,ojgn | different |
| `o02B` | Horde Boulder Tower | building | stock |  |  | 24p only |
| `o02T` | Horde Pier (warsong) | building | stock | o04L | obot,odes,ojgn | different |
| `o032` | Horde Pier (fel horde) | building | 24p | ncpn | obot,odes,ojgn | different |
| `o03W` | Improved Missile Tower (Goblin) | building | 24p |  |  | different |
| `o064` | Lightforged Gateway (Draenei Elite) | building | main | o05A | n0D6,n0D7,n0D9 | 24p only |
| `o03Q` | Lumber Mill (Goblin) | building | 24p | o02I |  | different |
| `n07D` | Maiden of Dominance Demon Gate (T3 Maiden) | building | stock |  |  | 24p only |
| `o03U` | Missile Tower (Goblin) | building | 24p | o02I |  | different |
| `o04R` | Oil Platform (Goblin) | building | main |  |  | 24p only |
| `o01T` | Orc Shop (warsong) | building | 24p | o00E,o04L |  | different |
| `n07B` | Queen of Suffering Demon Gate (T3 Queen Suffering) | building | stock |  |  | 24p only |
| `o05J` | Red Dragon Hatchery (Dragonmaw) | building | stock | o05P | n0CP,n0CQ,o05M | 24p only |
| `o053` | Smithery (Draenei) | building | main | o05A |  | different |
| `o033` | Spirit Lodge (fel horde) | building | 24p | ncpn | n08E,nchw,nw2w,odkt,odoc,oshm,ospw,u00V,u018 | different |
| `o01C` | Throne of Kil'jaeden | building | stock |  |  | 24p only |
| `h004` | Troll House | building | stock |  |  | different |
| `o03K` | Twilight Burrow (twilight) | building | 24p | o04B,o05P |  | different |
| `h05U` | Twilight Citadel (twilight) | building | 24p | h05E | o04B,o04I | 24p only |
| `o03I` | Twilight Dock (twilight) | building | 24p | o04B,o05P | obot,odes,ojgn | different |
| `o03G` | Twilight Lookout (twilight) | building | 24p | o04B,o05P |  | different |
| `n06G` | Voidcrusher Demon Gate (T2 Voidcrusher) | building | stock |  |  | different |
| `o03X` | Voodoo Lounge (Goblin) | building | 24p | o02I |  | different |
| `o047` | Voodoo Lounge (Zandalar) | building | 24p | o04A |  | different |
| `u00Q` | Voodoo Lounge (Fel Horde) | building | 24p | ncpn |  | different |
| `u00Y` | Voodoo Lounge (twilight) | building | 24p | o04B,o05P |  | different |
| `o02W` | War Camp (fel horde) | building | 24p | ncpn | n03R,n058,nchg,nftk,o005,o00G,o00I,o00P,o01F,o01G,o01O,ocat,+4 | different |
| `o03D` | War Camp (twilight) | building | 24p | o04B,o05P | n087,o01H,o04K,o05K,o05L | different |
| `o03P` | War Camp (Goblin) | building | main | o02I | h08X,h08Y,n099 | different |
| `o041` | War Camp (Zandalar) | building | 24p | o04A | h021,o04D | different |
| `o03J` | War Mill (twilight) | building | 24p | o04B,o05P |  | different |
| `n0CP` | Ancient Red Dragon (Dragonmaw) | unit | stock | o05J |  | 24p only |
| `obai` | Baine | unit | stock |  |  | 24p only |
| `u00O` | Banshee Spirits | unit | stock |  |  | 24p only |
| `o02K` | Bear Rider (warsong) | unit | 24p | o04X |  | 24p only |
| `nbdm` | Blackrock Dragonkin (Fel Horde) | unit | stock | o02X |  | 24p only |
| `n0A4` | Blue Dragonspawn (nexus) | unit | stock | h09A |  | 24p only |
| `o005` | Chaos Blademaster (Warsong - Bloodpact) | unit | stock | o02W |  | 24p only |
| `o00P` | Chaos Grunt (Warsong-Bloodpact) | unit | stock | o02W |  | different |
| `osw2` | Dire Wolf (Frostwolf-Thrall) | unit | stock |  |  | 24p only |
| `e00Z` | Direhorn (troll) | unit | 24p | o044 |  | 24p only |
| `o05M` | Dragon Rider (dragonmaw) | unit | 24p | o05J |  | 24p only |
| `n0CQ` | Dragon Whelp (Dragonmaw) | unit | stock | o05J |  | 24p only |
| `o05K` | Dragonmaw Warrior (dragonmaw) | unit | main | o03D |  | different |
| `h00C` | Drek'Thar (Frostwolf-Demi) | unit | stock | oalt |  | 24p only |
| `negz` | Engineer Gazlowe | unit | stock |  |  | 24p only |
| `o01E` | Enhanced Healing Ward (Frostwolf-Witch Doctor) | unit | stock |  |  | 24p only |
| `n0CN` | Foreman Glibbs (Warsong-Demi) | unit | stock | o020 |  | 24p only |
| `u028` | Fuel Transport Vehicle (Goblin) | unit | main | h04Z |  | different |
| `ngbl` | Goblin Blaster | unit | stock |  |  | 24p only |
| `o04W` | Golden Vessel (zandalar) | unit | 24p | o049 |  | 24p only |
| `h08Y` | Gunner (goblin) | unit | main | o03P |  | different |
| `o007` | Healing Totem (Level 1) | unit | stock |  |  | 24p only |
| `o00T` | Healing Totem (Level 2) | unit | stock |  |  | 24p only |
| `o00U` | Healing Totem (Level 3) | unit | stock |  |  | 24p only |
| `o01N` | Improved Healing Ward (Frostwolf-Witch Doctor) | unit | stock |  |  | 24p only |
| `ojgn` | Juggernaut (Warsong-Frostwolf-Fel Horde) | unit | stock | o02T,o032,o03I,o03V,o049,oshy |  | 24p only |
| `nckb` | Kodo War Beast (Warsong - Bloodpact) | unit | stock |  |  | 24p only |
| `h0AN` | Mammoth Wrangler (frostwolf) | unit | 24p | o065 |  | different |
| `omtg` | Mathog | unit | stock |  |  | 24p only |
| `nmsh` | Misha | unit | stock |  |  | 24p only |
| `o01R` | Nazgrel (Warsong-Demi) | unit | stock |  |  | 24p only |
| `onzg` | Nazgrel | unit | stock |  |  | 24p only |
| `nbwm` | Nefarian (Fel Horde-Tier-Demi) | unit | stock | o02V |  | 24p only |
| `n08A` | Neltharaku (Fel Horde-Demi) | unit | stock | o02V |  | different |
| `n0D3` | Ogron (fel horde) | unit | main | o02X |  | 24p only |
| `n0AQ` | Oil Rig (Warsong) | unit | missing | h04Z |  | 24p only |
| `nw2w` | Orc Warlock | unit | stock | o033,osld |  | 24p only |
| `oosc` | Pack Kodo (Frostwolf) | unit | stock | oalt,obar,ofor,ofrt,ogre,oshy,osld,ostr,+2 |  | 24p only |
| `h0AM` | Packleader (Frostwolf) | unit | main | o065 |  | different |
| `o05P` | Peon (Dragonmaw, Worker) | unit | 24p | h01Z,o039,o03A,o03B | o039,o03C,o03D,o03E,o03G,o03I,o03J,o03K,o05J,u00Y | 24p only |
| `o021` | Ravager (Warsong) | unit | main | o04X |  | 24p only |
| `n061` | Sapper (Warsong) | unit | stock |  |  | 24p only |
| `osp1` | Serpent Ward (Level 1) | unit | stock |  |  | 24p only |
| `o012` | Skeletal Grunt (Fel Horde- Fel Death Knight) | unit | stock |  |  | 24p only |
| `owad` | Slave Master | unit | stock |  |  | 24p only |
| `h051` | SoulSacrificeDummy | unit | stock |  |  | 24p only |
| `o05L` | Spearthrower (dragonmaw) | unit | main | o03D |  | different |
| `nwe3` | Spirit Hawk (Level 3)(Warson-Rexxar) | unit | stock |  |  | different |
| `oswy` | Spirit Wind Rider | unit | stock |  |  | 24p only |
| `osw1` | Spirit Wolf (Level 1) | unit | stock |  |  | 24p only |
| `n09E` | Storm Wyrm (troll) | unit | stock | o044 |  | 24p only |
| `ndqn` | Succubus (Fel Horde-Portal) | unit | stock |  |  | 24p only |
| `nspc` | Support Column | unit | stock |  |  | 24p only |
| `o063` | Tauren Gladiator (Frostwolf) | unit | main | otto |  | 24p only |
| `u005` | Totemic Spirits | unit | stock |  |  | different |
| `o04S` | Trader (goblin) | unit | main | o04M |  | 24p only |
| `nzep` | Trading Zeppelin (Warsong) | unit | stock | o04M |  | 24p only |
| `h038` | UnholyArmorDummy | unit | stock |  |  | 24p only |
| `n08C` | Voidcrusher (Fel Horde-Portal) | unit | stock |  |  | 24p only |
| `ovlj` | Vol'jin | unit | stock |  |  | 24p only |
| `n0DH` | Wanderer (Frostwolf) | unit | main | otto |  | 24p only |
| `n064` | War Master Voone (Fel Horde-Demi) | unit | stock | o02V |  | different |
| `e01L` | War Station (Warsong) | unit | 24p | o05G |  | different |
| `n07R` | War Turtle (Druid) | unit | stock |  |  | 24p only |
| `ogrk` | Warbringer (Fel Horde) | unit | stock | o02W |  | 24p only |
| `n03R` | Warsong Blood-Rider (Warsong-Elite) | unit | stock | o00J,o02W,o06U |  | different |
| `nwad` | Watcher Ward | unit | stock |  |  | 24p only |
| `ownr` | Wind Rider (Riderless) | unit | stock |  |  | 24p only |
| `o04P` | Wizard (goblin) | unit | main | o03S |  | different |

### other (240)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `n03W` | Aerie Peak (15gold/min) | building | stock |  |  | different |
| `n00K` | Aetheneum Catacombs (20gold/min) | building | stock |  |  | different |
| `n053` | Ahn'Kahet (10gold/min) | building | stock |  |  | different |
| `n079` | Ahn'Qiraj | building | stock |  |  | different |
| `n09C` | Alcaz Isle (10gold/min) | building | stock |  |  | different |
| `n019` | Alterac Mountains (20gold/min) | building | stock |  |  | different |
| `n01H` | Andorhal (15gold/min) | building | stock |  |  | different |
| `n0BF` | Antoran Wastes (10gold/min) | building | stock |  |  | different |
| `n01Z` | Arathi Highlands (15gold/min) | building | stock |  |  | different |
| `n07Q` | Area 52 (10gold/min) | building | stock |  |  | different |
| `n01R` | Astranaar (15gold/min) | building | stock |  |  | different |
| `n09D` | Auberdine (25gold/min) | building | stock |  |  | different |
| `n08Z` | Auchindoun (AoS) | building | stock |  |  | different |
| `n04S` | Azjol-Nerub | building | stock |  |  | different |
| `n00I` | Azjol-Nerub (15gold/min) | building | stock |  |  | different |
| `n01S` | Azshara Coast (10gold/min) | building | stock |  |  | different |
| `n0D4` | Azsuna (15gold/min) | building | stock |  |  | 24p only |
| `n02Z` | Azuremyst Isle (15gold/min) | building | stock |  |  | different |
| `n07L` | Balor (15gold/min) | building | stock |  |  | different |
| `n02P` | Barrow Deeps (10gold/min) | building | stock |  |  | 24p only |
| `n00R` | Black Temple (30gold/min) | building | stock |  |  | 24p only |
| `n0AA` | Blackrock Depths (10gold/min) | building | stock |  |  | different |
| `n033` | Blackrock Entrance | building | stock |  |  | different |
| `n00O` | Blackrock Spire (25gold/min) | building | stock |  |  | different |
| `n06V` | Blackwald (10gold/min) | building | stock |  |  | different |
| `n02N` | Blade's Edge Mountains (15gold/min) | building | stock |  |  | different |
| `n0A9` | Blasted Lands (10gold/min) | building | stock |  |  | different |
| `ndru` | Blue Dragon Roost | building | stock |  |  | 24p only |
| `n00L` | Booty Bay (10gold/min) | building | stock |  |  | different |
| `n01W` | Boralus (25gold/min) | building | stock |  |  | different |
| `n00G` | Borean Tundra (15gold/min) | building | stock |  |  | different |
| `n012` | Burning Steppes (20gold/min) | building | stock |  |  | different |
| `n01I` | Caer Darrow (15gold/min) | building | stock |  |  | different |
| `n09J` | Centaur Tent (2) | building | stock |  |  | 24p only |
| `ncp2` | Circle of Power (medium) | building | stock |  |  | 24p only |
| `ncp3` | Circle of Power (large) | building | stock |  |  | 24p only |
| `n03P` | Corin's Crossing (10gold/min) | building | stock |  |  | different |
| `ngnh` | Creep Hut | building | stock |  |  | 24p only |
| `n09K` | Crestfall (10gold/min) | building | stock |  |  | 24p only |
| `n02R` | Crystalsong Forest (15gold/min) | building | stock |  |  | different |
| `n01B` | Dalaran (30gold/min) | building | stock |  |  | different |
| `n03A` | Dalaran Dungeons | building | 24p |  |  | different |
| `n03H` | Dalaran Dungeons (Inside) | building | stock |  |  | different |
| `n05Z` | Dalaran Underbelly (10gold/min) | building | stock |  |  | different |
| `n02U` | Darkshore (10gold/min) | building | stock |  |  | different |
| `n04Y` | Darkspear Isles (10gold/min) | building | stock |  |  | different |
| `n00Y` | Deadwind Pass (10gold/min) | building | stock |  |  | different |
| `n01Y` | Desolace (15gold/min) | building | stock |  |  | different |
| `ndch` | Draenei Chieftain's Hut | building | stock |  |  | 24p only |
| `ndh1` | Draenei Hut 2 | building | stock |  |  | 24p only |
| `n02Q` | Dragonblight (20gold/min) | building | stock |  |  | different |
| `n08T` | Dragonmaw Port (10gold/min) | building | stock |  |  | different |
| `n030` | Drak'Tharon Keep (30gold/min) | building | stock |  |  | different |
| `n0BV` | Drustvar (10gold/min) | building | stock |  |  | different |
| `n017` | Dun Modr (10gold/min) | building | stock |  |  | different |
| `n014` | Dun Morogh (15gold/min) | building | stock |  |  | different |
| `n018` | Durnholde (15gold/min) | building | stock |  |  | different |
| `n03Z` | Durotar (15gold/min) | building | stock |  |  | different |
| `n031` | Duskhaven (20gold/min) | building | stock |  |  | different |
| `n00V` | Duskwood (10gold/min) | building | stock |  |  | different |
| `n0AF` | Dustwallow Marsh (10gold/min) | building | stock |  |  | different |
| `n02B` | Eastern Rockland (15gold/min) | building | stock |  |  | different |
| `n02V` | Echo Isles (30gold/min) | building | stock |  |  | different |
| `n09R` | Eldarath (15gold/min) | building | stock |  |  | different |
| `n03J` | Eldre'thalas (10gold/min) | building | stock |  |  | different |
| `n00Z` | Elwynn Forest (20gold/min) | building | stock |  |  | different |
| `n09H` | En'Kilah (10gold/min) | building | stock |  |  | different |
| `n0BT` | Entrance Only (Green - twilight highlands) | building | stock |  |  | 24p only |
| `n0BH` | Eredath (25gold/min) | building | stock |  |  | different |
| `n01L` | Eversong Woods (20gold/min) | building | stock |  |  | different |
| `n0CW` | Farahlon (10gold/min) | building | stock |  |  | different |
| `n05U` | Feathermoon Stronghold (20gold/min) | building | stock |  |  | different |
| `n07C` | Felwood (10gold/min) | building | stock |  |  | different |
| `n027` | Feralas (15gold/min) | building | stock |  |  | different |
| `nmoo` | Fountain of Mana | building | stock |  |  | 24p only |
| `n0BJ` | Frostwhisper Gorge (10gold/min) | building | stock |  |  | different |
| `n07J` | Frozen Throne (20gold/min) | building | stock |  |  | different |
| `n01E` | Fuselight (10gold/min) | building | stock |  |  | different |
| `n05C` | Gadgetzan (10gold/min) | building | stock |  |  | different |
| `n046` | Gilneas City (25gold/min) | building | stock |  |  | different |
| `n04E` | Gishan Cavern (10gold/min) | building | stock |  |  | different |
| `n06X` | Gnomeregan (20gold/min) | building | stock |  |  | different |
| `ngol` | Gold Mine | building | stock |  |  | 24p only |
| `n03X` | Grim Batol (10gold/min) | building | stock |  |  | different |
| `n03U` | Grizzly Hills (15gold/min) | building | stock |  |  | different |
| `n0CV` | Halaar (10gold/min) | building | stock |  |  | different |
| `n04W` | Havenshire (10gold/min) | building | stock |  |  | different |
| `n0A0` | Heart of Azeroth (30gold/min) | building | stock |  |  | different |
| `n044` | Hearthglen (10gold/min) | building | stock |  |  | different |
| `n00S` | Hellfire Peninsula (30gold/min) | building | stock |  |  | different |
| `n0DG` | Highmountain (15gold/min) | building | stock |  |  | 24p only |
| `n01A` | Hinterlands (20gold/min) | building | stock |  |  | different |
| `n02J` | Howling Fjords (15gold/min) | building | stock |  |  | different |
| `nmg0` | Hut 0 | building | stock |  |  | 24p only |
| `nmg1` | Hut 1 | building | stock |  |  | 24p only |
| `nth1` | Ice Troll Hut | building | stock |  |  | 24p only |
| `nth0` | Ice Troll Hut Main 2 | building | stock |  |  | 24p only |
| `n00E` | Icecrown Citadel (30gold/min) | building | stock |  |  | different |
| `n08D` | Icecrown Glacier (20gold/min) | building | stock |  |  | different |
| `n015` | Ironforge (30gold/min) | building | stock |  |  | different |
| `n05G` | Isle of Dread (10gold/min) | building | stock |  |  | different |
| `n08X` | Keel Harbor (10gold/min) | building | stock |  |  | different |
| `n04Z` | Kezan (15gold/min) | building | stock |  |  | different |
| `n09Y` | Kil'sorrow Fortress (10gold/min) | building | stock |  |  | different |
| `n0BG` | Krokuun (10gold/min) | building | stock |  |  | different |
| `n024` | Kul Tiras (10gold/min) | building | stock |  |  | different |
| `n05W` | Lightless Cavern (15gold/min) | building | stock |  |  | different |
| `n013` | Loch Modan (10gold/min) | building | stock |  |  | different |
| `n01G` | Lordaeron City (30gold/min) | building | stock |  |  | different |
| `n08L` | Lordamere Lake (10gold/min) | building | stock |  |  | different |
| `n0BK` | Lost City of the Tol'vir (25gold/min) | building | stock |  |  | different |
| `h01A` | Maelstrom Pass (Portal) | building | stock |  |  | 24p only |
| `n09Z` | Maelstrom's Rift (10gold/min) | building | stock |  |  | 24p only |
| `n06W` | Menethil Harbour (10gold/min) | building | stock |  |  | different |
| `n0D8` | Mercenaries Tavern 1 (Vassals) | building | stock |  |  | different |
| `nmr8` | Mercenary Camp (Cityscape) | building | stock |  |  | 24p only |
| `n00P` | Molten Core (10gold/min) | building | stock |  |  | different |
| `n02E` | Moonglade (20gold/min) | building | stock |  |  | different |
| `n05O` | Mount Hyjal (20gold/min) | building | stock |  |  | 24p only |
| `n09G` | Mulgore (20gold/min) | building | stock |  |  | different |
| `h01D` | Naga Entrance (Portal) | building | stock |  |  | 24p only |
| `n00B` | Nagrand (15gold/min) | building | stock |  |  | different |
| `n05Q` | Nazjatar (25gold/min) | building | stock |  |  | different |
| `n0A8` | Nazmir (10gold/min) | building | stock |  |  | different |
| `nnzg` | Nerubian Ziggurat | building | stock |  |  | 24p only |
| `n00T` | Nethergarde Keep (10gold/min) | building | stock |  |  | different |
| `n02O` | Netherstorm (10gold/min) | building | stock |  |  | different |
| `n01P` | Nordrassil (30gold/min) | building | stock |  |  | different |
| `n01Q` | Northern Ashenvale (10gold/min) | building | stock |  |  | different |
| `n02M` | Northern Badlands (10gold/min) | building | stock |  |  | different |
| `n01T` | Northern Barrens (15gold/min) | building | stock |  |  | different |
| `n04V` | Northern Highlands (10gold/min) | building | stock |  |  | different |
| `n08S` | Northern Silverpine Forest (20gold/min) | building | stock |  |  | different |
| `n06R` | Northgate Woods (10gold/min) | building | stock |  |  | different |
| `n0BB` | Ny'alotha (30gold/min) | building | stock |  |  | different |
| `n07F` | Ny'alotha Portal (Green - twilight highlands) | building | stock |  |  | different |
| `n07P` | Orgrimmar (30gold/min) | building | stock |  |  | different |
| `n090` | Oshu'gun (AoS) | building | stock |  |  | 24p only |
| `n04X` | Plunder Isle/Balor (15gold/min) | building | stock |  |  | different |
| `n03N` | Quel'Danas (30gold/min) | building | stock |  |  | different |
| `n03O` | Ranazjar Isle (10gold/min) | building | stock |  |  | different |
| `n0A6` | Ratchet (10gold/min) | building | stock |  |  | different |
| `n0BM` | Ravencrest Monument (10gold/min) | building | stock |  |  | different |
| `n0BO` | Razorfen Downs (15gold/min) | building | stock |  |  | different |
| `n011` | Redridge Mountains (10gold/min) | building | stock |  |  | different |
| `n02K` | Ruins of Ahn'Qiraj (15 gold/min) | building | stock |  |  | different |
| `n03I` | Ruins of Eldre'thalas | building | stock |  |  | 24p only |
| `n035` | Scholomance | building | stock |  |  | different |
| `n028` | Scholomance (20gold/min) | building | stock |  |  | different |
| `n04B` | Scholomance Inner | building | stock |  |  | 24p only |
| `n02X` | Searing Gorge (10gold/min) | building | stock |  |  | different |
| `n054` | Shadowforge Exterior (15gold/min) | building | stock |  |  | 24p only |
| `n09V` | Shadowmoon Valley (10gold/min) | building | stock |  |  | different |
| `n09X` | Shattrath City (15 gold/min) | building | stock |  |  | different |
| `n0BN` | Shimmering Flats (10gold/min) | building | stock |  |  | different |
| `n00F` | Sholazar Basin (15gold/min) | building | stock |  |  | different |
| `n0BI` | Shrine of Goldrinn (10gold/min) | building | stock |  |  | different |
| `n047` | Silithus (10gold/min) | building | stock |  |  | different |
| `n01O` | Silvermoon (30gold/min) | building | stock |  |  | different |
| `n01D` | Silverpine Forest (15gold/min) | building | stock |  |  | different |
| `n08U` | Southern Ashenvale (10gold/min) | building | stock |  |  | different |
| `n02A` | Southern Barrens (15gold/min) | building | stock |  |  | different |
| `n09F` | Southern Highlands (20gold/min) | building | stock |  |  | different |
| `n08M` | Southshore (15gold/min) | building | stock |  |  | different |
| `n08N` | Steamwheedle Port (20gold/min) | building | stock |  |  | different |
| `n022` | Stonemaul (20gold/min) | building | stock |  |  | different |
| `n01U` | Stonetalon Peak (15gold/min) | building | stock |  |  | different |
| `n02S` | Storm Peaks (15gold/min) | building | stock |  |  | different |
| `n0DF` | Stormheim (10gold/min) | building | stock |  |  | 24p only |
| `n0BW` | Stormsong Valley (10gold/min) | building | stock |  |  | different |
| `n010` | Stormwind City (30gold/min) | building | stock |  |  | different |
| `n01C` | Strahnbrad (10gold/min) | building | stock |  |  | different |
| `n00X` | Stranglethorn (10gold/min) | building | stock |  |  | different |
| `n01M` | Stratholme (30gold/min) | building | stock |  |  | different |
| `n01K` | Stromgarde (25gold/min) | building | stock |  |  | different |
| `n05Y` | Suramar (20gold/min) | building | stock |  |  | different |
| `n00U` | Swamp of Sorrows (10gold/min) | building | stock |  |  | different |
| `n020` | Tanaris (10gold/min) | building | stock |  |  | different |
| `n052` | Target | building | main |  |  | 24p only |
| `n07K` | Teldrassil | building | stock |  |  | 24p only |
| `n02Y` | Teldrassil (30gold/min) | building | stock |  |  | different |
| `n0BU` | Teleporter | building | stock |  |  | 24p only |
| `n08B` | Tempest Keep (30gold/min) | building | stock |  |  | different |
| `n084` | Tempest Reach (10gold/min) | building | stock |  |  | different |
| `n07A` | Temple of Ahn'Qiraj | building | stock |  |  | different |
| `n078` | Temple of Ahn'Qiraj (30 gold/min) | building | stock |  |  | different |
| `n02T` | Terokkar Forest (10 gold/min) | building | stock |  |  | different |
| `n09I` | The Dread Chain (10gold/min) | building | stock |  |  | 24p only |
| `n0BL` | The Exodar (25gold/min) | building | stock |  |  | different |
| `n00N` | The Violet Hold (15gold/min) | building | stock |  |  | different |
| `n00M` | Theramore (30gold/min) | building | stock |  |  | different |
| `n026` | Thousand Needles (15gold/min) | building | stock |  |  | different |
| `n03M` | Thunderbluff (30gold/min) | building | stock |  |  | different |
| `n0BX` | Tiragarde Sound (10gold/min) | building | stock |  |  | different |
| `n01F` | Tirisfal Glades (10gold/min) | building | stock |  |  | different |
| `n08Q` | Tol Barad (20gold/min) | building | stock |  |  | different |
| `n032` | Tomb of Sargeras | building | stock |  |  | different |
| `n00J` | Tomb of Sargeras (20gold/min) | building | stock |  |  | different |
| `n01N` | Tranquillien (15gold/min) | building | stock |  |  | different |
| `n0CU` | Tuurem (10gold/min) | building | stock |  |  | 24p only |
| `n00Q` | Twisting Nether (50gold/min) | building | stock |  |  | different |
| `n02W` | Tyr's Hand (30gold/min) | building | stock |  |  | different |
| `n0BD` | Uldum (10gold/min) | building | stock |  |  | different |
| `n025` | Un'Goro Crater (10gold/min) | building | stock |  |  | different |
| `n0BP` | Un'gol Ruins (25gold/min) | building | stock |  |  | different |
| `n0BC` | Undercity (20gold/min) | building | stock |  |  | different |
| `n0DB` | Val'sharah (15gold/min) | building | stock |  |  | 24p only |
| `n07Y` | Venture Bay (10gold/min) | building | stock |  |  | different |
| `n0A7` | Vol'dun (10gold/min) | building | stock |  |  | 24p only |
| `n095` | Voodoo Shop (Unique) | building | stock |  |  | 24p only |
| `n05K` | Warsong Lumber Camp (15gold/min) | building | stock |  |  | different |
| `nwgt` | Way Gate | building | stock |  |  | 24p only |
| `n08O` | Western Hillsbrad (15gold/min) | building | stock |  |  | different |
| `n02I` | Western Stranglethorn (10gold/min) | building | stock |  |  | different |
| `n02L` | Westfall (10gold/min) | building | stock |  |  | different |
| `n016` | Wetlands (10gold/min) | building | stock |  |  | different |
| `n05N` | Windshear Crossing (10gold/min) | building | stock |  |  | different |
| `n01X` | Winterspring (15gold/min) | building | stock |  |  | different |
| `n01J` | Zangarmarsh (15gold/min) | building | stock |  |  | different |
| `n01V` | Zul'Aman (20gold/min) | building | stock |  |  | different |
| `n00H` | Zul'Drak (20gold/min) | building | stock |  |  | different |
| `n092` | Zul'Farrak (10gold/min) | building | stock |  |  | different |
| `n00W` | Zul'Gurub (10gold/min) | building | stock |  |  | different |
| `n050` | Zuldazar (20gold/min) | building | stock |  |  | different |
| `n038` | Azjol-Nerub | unit | stock |  |  | 24p only |
| `nqb2` | Dire Quilbeast (Level 2) | unit | stock |  |  | 24p only |
| `ndr3` | Greater Dark Minion | unit | stock |  |  | 24p only |
| `n069` | Hurricane (Level 1) | unit | stock |  |  | different |
| `n06A` | Hurricane (Level 2) | unit | stock |  |  | different |
| `n06B` | Hurricane (Level 3) | unit | stock |  |  | different |
| `nlv2` | Lava Spawn (Level 2) | unit | stock |  |  | 24p only |
| `ndr1` | Lesser Dark Minion | unit | stock |  |  | 24p only |
| `n09U` | Maelstrom Pass (New) | unit | stock |  |  | 24p only |
| `zmar` | Marine | unit | stock |  |  | 24p only |
| `ngza` | Misha (Level 3) | unit | stock |  |  | 24p only |
| `ngzc` | Misha (Level 1) | unit | stock |  |  | 24p only |
| `ngzd` | Misha (Level 2) | unit | stock |  |  | 24p only |
| `nqb3` | Raging Quilbeast (Level 3) | unit | stock |  |  | 24p only |
| `n09Q` | Tunnel (Dalaran Prison Secret Room) | unit | stock |  |  | 24p only |
| `nfac` | Void Portal (Level 1) | unit | stock |  |  | 24p only |

### undead (87)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `Uvar` | Arch Dreadlord (forsaken) | hero | stock |  |  | 24p only |
| `U026` | Aspect of Magic (nexus) | hero | 24p | h044 |  | different |
| `E01O` | Champion of the Uncrowned (gilneas) | hero | 24p |  |  | 24p only |
| `Usyl` | Dark Ranger (forsaken) | hero | stock | u010 |  | 24p only |
| `Udth` | Dreadlord | hero | stock |  |  | 24p only |
| `Uvng` | Dreadlord | hero | stock |  |  | 24p only |
| `Ubal` | Envoy of Archimonde (Legion) | hero | stock |  |  | 24p only |
| `U01O` | Grand Apothecary (Forsaken) | hero | main | u010 |  | 24p only |
| `Ulic` | Lich | hero | stock |  |  | 24p only |
| `H047` | Lord of the Satyrs (Legion Vassal) | hero | 24p |  |  | 24p only |
| `U02I` | Master of the Royal Apothecary Society (Forsaken) | hero | main | u010 |  | 24p only |
| `H049` | Ranger-Lord (Forsaken) | hero | 24p |  |  | 24p only |
| `U027` | Steward of Magic (nexus) | hero | 24p | h044 |  | 24p only |
| `u010` | Altar of the Queen (Forsaken) | building | 24p | u01K | U01O,U02I,Usyl | 24p only |
| `h089` | Dark Mansion (Forsaken) | building | 24p | u01K | n0BY,u01K | 24p only |
| `n043` | Deactivated Demon Gate (Fel Horde) | building | stock |  |  | 24p only |
| `n04C` | Defiled Fountain of Life (Fel Horde) | building | stock |  |  | 24p only |
| `u016` | Forbidden Archive (Forsaken) | building | 24p | u01K | h08P,u01R | 24p only |
| `u017` | Forsaken Den (Forsaken) | building | 24p | u01K |  | 24p only |
| `nfrm` | Frostmourne Pedestal | building | stock |  |  | 24p only |
| `h08C` | Grave (Forsaken) | building | 24p | u01K |  | 24p only |
| `u01J` | Graveyard (Forsaken) | building | 24p | u01K |  | 24p only |
| `h08A` | Grim Estate (Forsaken) | building | 24p |  | u01K | 24p only |
| `h08B` | Haunted Castle (Forsaken) | building | 24p |  | n0BY,u01K | 24p only |
| `ugol` | Haunted Gold Mine | building | stock |  |  | 24p only |
| `u009` | Haunted Harbor (Legion) | building | 24p | u00D | ubot,udes,uubs | different |
| `u01A` | Haunted Harbor (Forsaken) | building | 24p | u01K | ubot,udes,uubs | 24p only |
| `u01X` | Heart of Naxxramas | building | stock |  | h00H,uaco | 24p only |
| `n040` | Hell Forge (Legion) | building | 24p | u00D |  | different |
| `h08F` | Improved Plague Tower (Forsaken) | building | 24p |  |  | 24p only |
| `ngni` | Infected Granary | building | stock |  |  | 24p only |
| `n04N` | Infernal Machine Factory (Legion) | building | 24p | u00D | n03L,n04L | different |
| `u014` | Laboratory (Forsaken) | building | 24p | u01K | n07W,o05H,u01P | 24p only |
| `n0BE` | Legion Nexus (Legion) | building | main |  |  | different |
| `e013` | Naxxramas (scourge) | building | stock |  |  | 24p only |
| `u00E` | Ossuary (Legion) | building | 24p | u00D |  | different |
| `h02W` | Plague Cauldron | building | stock |  |  | different |
| `h08E` | Plague Tower (Forsaken) | building | 24p |  |  | 24p only |
| `u011` | Plaguetorium (Forsaken) | building | 24p | u01K | h08N,h08O,n07S | 24p only |
| `u01W` | Royal Sepulcrum (forsaken) | building | main | u01K | n07V,u01V,u02G | 24p only |
| `u012` | Scholomance | building | stock |  | u01K | 24p only |
| `h08D` | Scout Tower (Forsaken) | building | 24p | u01K |  | 24p only |
| `nshr` | Shrine | building | stock |  |  | 24p only |
| `h015` | The Legion's Portal (Legion) | building | stock |  |  | 24p only |
| `h08S` | Undercity (Forsaken) | building | stock |  | n0BY,u01K | 24p only |
| `n041` | Abyssal (Legion-Archimond-Level 1) | unit | stock |  |  | different |
| `u01K` | Acolyte (Forsaken, Cult, Worker) | unit | 24p | h089,h08A,h08B,h08S,u012,u01M,u02L | h089,h08C,h08D,u010,u011,u014,u016,u017,u01A,u01J,u01W | 24p only |
| `o060` | Annihilan Guard (Frostwolf) | unit | stock | n04Q |  | 24p only |
| `u01R` | Apothecary (Forsaken, Cult) | unit | stock | u016 |  | 24p only |
| `h08P` | Archivist (Forsaken, Cult) | unit | main | u016 |  | 24p only |
| `ucsB` | Burrowed Carrion Beetle (Red - Anub'Arak)(Level 2) | unit | stock |  |  | 24p only |
| `ucsC` | Burrowed Carrion Beetle (Red - Anub'Arak)(Level 3) | unit | stock |  |  | 24p only |
| `u00K` | Channel Dummy Caster | unit | stock |  |  | 24p only |
| `udes` | Corsair (Scourge-Legion) | unit | stock | u009,u01A,ushp |  | 24p only |
| `n0A1` | Crystal Lord (nexus) | unit | 24p | h065,h066,h068 |  | 24p only |
| `u01U` | Cultist of the Damned (Forsaken, Cult) | unit | stock |  |  | different |
| `n0BY` | Dark Ranger (forsaken) | unit | main | h089,h08B,h08S |  | 24p only |
| `n07S` | Deadeye (Forsaken, Cult) | unit | main | u011 |  | 24p only |
| `o066` | Death Domain | unit | 24p |  |  | 24p only |
| `u01Y` | Deathwing (twilight) | unit | main |  |  | 24p only |
| `n091` | Doomfire Abyssal (Legion-Archimond-Level 2) | unit | stock |  |  | different |
| `h08N` | Dreadguard (Forsaken, Cult) | unit | 24p | u011 |  | 24p only |
| `uubs` | Dreadnought (Scourge-Legion) | unit | stock | u009,u01A,ushp |  | 24p only |
| `u019` | Drone (C'thun, Worker) | unit | main | u020,u021,u022 | n071,n073,o00D,o00R,u01F,u01G,u01H,u01I,u020 | different |
| `n07V` | Elder Banshee (Forsaken) | unit | 24p | u01W |  | 24p only |
| `u025` | Elementalist (nexus) | unit | 24p | h09B |  | 24p only |
| `u02G` | Elite Abominations (Scourge, Forsaken, Cult) | unit | main | u01W |  | 24p only |
| `n04J` | Fel Creeper (Legion) | unit | stock |  |  | different |
| `n042` | Gargantuan Abyssal (Legion-Archimond-Level 3) | unit | stock |  |  | different |
| `n04O` | Greater Doom Guard (Legion) | unit | 24p | ndmg |  | different |
| `u02E` | Herald of Ny'alotha (Black Emprie) | unit | 24p | n0B6 |  | 24p only |
| `n09T` | Judicator (nexus) | unit | 24p | h09B |  | 24p only |
| `uktn` | Kel'Thuzad ( - - - Unused - - - ) | unit | stock |  |  | 24p only |
| `nzlc` | Lich King (cinematic campaign unit) | unit | stock |  |  | 24p only |
| `h09G` | MonolithSpawn | unit | stock |  | e01F | 24p only |
| `u008` | Peasant Converter | unit | stock |  |  | different |
| `u01P` | Plague Catapult (Forsaken) | unit | main | u014 |  | 24p only |
| `n07W` | Plague Construct (Forsaken) | unit | main | u014 |  | 24p only |
| `o05H` | Plague Flyer (forsaken) | unit | main | u014 |  | 24p only |
| `h08O` | Rotguard (Forsaken) | unit | main | u011 |  | 24p only |
| `nska` | Skeleton Archer (Scourge) | unit | stock |  | R094 | 24p only |
| `uarb` | Sky Barge | unit | stock |  |  | 24p only |
| `u01D` | Soul of Frostmourne | unit | stock |  |  | 24p only |
| `uswb` | Sylvanas Windrunner (Banshee) | unit | stock |  |  | 24p only |
| `u01V` | Val'kyr (forsaken) | unit | main | u01W |  | 24p only |
| `h099` | Zealot (Nexus) | unit | 24p | h09A |  | 24p only |
| `nzof` | Zombie Female | unit | stock |  |  | 24p only |

### unknown (6)

| Id | Name | Kind | Model | Made at | Trains / builds | Status |
|---|---|---|---|---|---|---|
| `h01W` | Gate of Silvermoon (Closed) | building | stock |  |  | different |
| `h01X` | Gate of Silvermoon (Open) | building | stock |  |  | different |
| `h01Y` | Gate of Silvermoon (Dead) | building | stock |  |  | different |
| `h04R` | Uldum Gate (Open) | building | stock |  |  | different |
| `h04S` | Uldum Gate (Closed) | building | stock |  |  | different |
| `h04T` | Uldum Gate (Dead) | building | stock |  |  | different |
