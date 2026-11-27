# Void-area dungeons (0.82.0 - 0.87.0)

Maintainer's idea (2026-11-26): the black, unwalkable void of the map holds dungeons that can be entered only through
gates. The void stays as the wall around and between them. Pack them tightly, so that most of the void gets used:
existing things may move a little towards the edges, and a small dungeon can take a small pocket. Leave room for
dungeons that have no concept yet.

Source of the terrain: Warcraft Legacies 24p 1.18a (`mapdata/WarcraftLegacies24p`). It has the same size and origin
as our map (417 x 449 corners from -28672, -32256), and all 16 of its ground textures are among our 27. The 5 cliff
types are the same.

## Tool

`tools/shattered-legacies/terrain/copy_area.py NAME X0 Y0 X1 Y1 DX DY [--types A=B,..] [--check]` copies these:

- the terrain corners (with the texture ids remapped)
- the pathing and shadow cells
- the doodads, with new CreationNumbers, into `Doodads/Dungeon_NAME.json`

It refuses if the destination is not void (walkable cells, our doodads or our preplaced units). It also refuses if a
custom doodad type is unknown here: `--types` copies such a type under a fork id (`?Z<lower>?`; `DZv*` = void
dungeons).

Units, regions and the minimap are not copied. The code places the buildings, gates and CPs. Run it again after a
change: a destination that already holds the same copy counts as free.

Each dungeon also needs these, so the bot finds it:

- a fixed node in `tools/shattered-legacies/ai/route_exceptions.txt` (`node @x,y`), then run `gen_routes.py`
- its own landmass letter in `Geography.Grid` (`Ai/AiSystem.cs`)

The code registers it as a WL instance (`Core/Effects/Instance`). Inside, world-tree immortality, Summon Legion and
slipstreams across its border do not work.

## Done

| Dungeon | 24p source (copied with its wall) | Offset | Our rectangle | Landmass | Since |
|---|---|---|---|---|---|
| Undercity | -28672,14208 .. -25728,17920 | 0, +6912 | -28672,21120 .. -25728,24832 | 20 `U` | 0.82 |
| Nazjatar | 7936,-31488 .. 15744,-26112 | -31104, +512 | -23168,-30976 .. -15360,-25600 | 21 `V` | 0.83 |
| Shadowforge (24p Blackrock Depths + Molten Core) | 16256,-32000 .. 22784,-28288 | -40704, +53120 | -24448,21120 .. -17920,24832 | 22 `W` | 0.83 |
| Deadmines (24p Temple of Ahn'Qiraj) | -28416,-16640 .. -24192,-8064 | -256, -14848 | -28672,-31488 .. -24448,-22912 | 23 `X` | 0.83 |
| Timbermaw Hold (OUR Felwood glade by the furbolg camp, `--src mapdata/WarcraftLegacies`) | -12416,7680 .. -10368,9600 | -3968, +13824 | -16384,21504 .. -14336,23424 | `.` | 0.87 |
| The Exodar (OUR Azuremyst base, MOVED: `terrain/exodar_move.py` -- the old place is void now) | -24704,5632 .. -21248,8960 | -3840, +7168 | -28544,12800 .. -25088,16128 | `.` | 0.87 |

**What counts as void**: unflyable cells only (pathing flag 0x04, the map boundary). Deep sea is unwalkable too, but
ships and flyers cross it. The 0.82 measurement mixed the two: much of the "northern band" and "western column" is
sea. Each placement leaves at least 1536 (12 tiles) of void between its ground and any other flyable ground (land or
sea), so no attack reaches across and nothing flies over. The places were found by search: walkable bounding box + 12
tiles, all void, no doodads or units. Measured after the copies: 1568 for all four.

**Room made**: WL's seven unused squares in the south-west corner (walkable boxes in the void, x -23040 .. -9984,
y -30208 .. -24960; the 0.57 Undercity used one; landmasses 8, 9, 11-17) are void now (`copy_area.py --clear
-23296 -30464 -9728 -24704`); Nazjatar stands there. `Regions.HideUnitBottomLeft` still covers it. That region only
hides Neutral Passive units at setup, so the CPs (Neutral Hostile) and the gate (created 1 s later) are not
affected.

The commands (rerunnable; after a copy, run the generators again):

```
T=tools/shattered-legacies/terrain/copy_area.py
python3 $T Undercity -28672 14208 -25728 17920 0 6912 --types D00O=DZv0,D00K=DZv1 --drop DZv1@-26537,21323
python3 $T --clear -23296 -30464 -9728 -24704
python3 $T Nazjatar 7936 -31488 15744 -26112 -31104 512 --types D03U=DZv2,D020=DZv3,D03V=DZv4,D03Q=DZv5
python3 $T Shadowforge 16256 -32000 22784 -28288 -40704 53120 --types D06C=DZv6,D00F=DZv7,D030=D030 \
  --drop DZv7@-20640,23230
python3 $T Deadmines -28416 -16640 -24192 -8064 -256 -14848 \
  --retype D05I=NOfl,D05K=LOlp,D05M=DSar,D05D=DOmc,D05J=LOth,D05O=DRst,D05F=DRrk,D05H=LOwb,D05L=DOjp,D05N=DOtp,\
D05G=DOjp,D05U=DRrk,B01R=LTcr,B00U=DTsh,JOgr=LOtz,ZPms=GPsh,BRgs=GRrk,OOal=DOtp \
  --drop LPcw,LOfl,OPop --tex Zsan=Adrd,Gsqd=Ndrt
python3 tools/shattered-legacies/content/void_dungeons.py
python3 tools/shattered-legacies/ai/gen_routes.py
```

**Gates** (0.84, maintainer). Each gate is a stock Waygate with another model (`void_dungeons.py`: `nZvb`
"Gateway", the circle effect of the 24p's teleporter indicator; `nZvc` a gold mine; `nZvd` the Undercity's iron
gate). They are invulnerable and Neutral Passive. They stand from the start, visible and SHUT, so everyone sees
that something will happen there. A dungeon opens when the fallback that lives there arrives (the extra CPs' gold
would upset the balance otherwise). Code: `Dungeons/VoidDungeons.cs`; the Undercity is in
`Factions/Forsaken/ForsakenUndercity.cs`.

| Dungeon | Inner gate | World entrance | Opens with |
|---|---|---|---|
| Undercity | the 24p iron gate at -26537,21323 (its doodad is dropped) | stock waygate in Lordaeron's palace ruins | "The Throne of Lordaeron" (Forsaken) |
| Nazjatar | circle at -18496,-29056 (24p "NazjatarExit2") | circle at -3744,-3552, by WL's Nazjatar CP beside the Maelstrom | the naga of Azshara (Sentinels' fallback, 0.84) |
| Shadowforge | circle at -20640,23230 (the 24p's teleporter circle; its doodad is dropped) | WL's Shadowforge City building (h01O, Awrp added) | the Dark Iron (Ironforge's fallback, 0.86) |
| Deadmines | circle at -27808,-23904 (24p north portal) | gold mine at 7721,-17503, Westfall | the Defias (Stormwind's fallback, 0.85) |
| Timbermaw Hold | circle at -14692,22708 | the Totem Lodge of Ursoc at -11154,10970 by Nordrassil (Awrp) | the Furbolgs (the Druids' fallback, 0.87) |
| The Exodar | circle at -26200,13400 | the ship itself (`nZvf`), at one of 16 landing spots; it flies (`Factions/Draenei/ExodarShip.cs`) | the Draenei (the Legion's fallback, 0.87; 0.87.1: its two preplaced shipyards removed -- the Draenei can still build them) |

**Control points** (`tools/shattered-legacies/content/void_dungeons.py`: types `nZv*`, preplaced Neutral Hostile,
with Chinese names). The 24p names are kept except where our map already has the name or the dungeon changed.
Shadowforge is Shadowforge City itself (0.84: districts; 0.86: the Dark Iron's Treasury and Siege Foundry; 0.87: the
Throne Room is their seat):

| CP | Gold | Spot | 24p |
|---|---|---|---|
| Undercity | 20 | -27139,22825 | n0BC "Undercity"; goes to the Forsaken with the buildings |
| Eternal Palace | 25 | -22208,-30016 | n05Q "Nazjatar" (WL has a Nazjatar CP at the Maelstrom); the naga's seat |
| Heart of Azeroth | 30 | -19008,-27904 | n0A0 |
| Lightless Cavern | 15 | -16192,-28352 | n05W |
| Gishan Cavern | 10 | -20288,-26368 | n04E |
| Dark Iron Treasury | 20 | -22144,22784 | n0AA "Blackrock Depths" (0.84: Imperial District) |
| Siege Foundry | 15 | -23040,23936 | n00P "Molten Core" (0.84: Forge District); their Mercenary Charter, the Statue of Thaurissan |
| Throne Room | 10 | -18944,23360 | n00O "Blackrock Spire" (0.84 - 0.86: Tavern District); the Dark Iron's seat (0.87), the Throne of Thaurissan beside it |
| The Deadmines | 20 | -26048,-26880 | n078 "Temple of Ahn'Qiraj" |
| Goblin Foundry | 10 | -27952,-28256 | new |
| Ironclad Cove | 15 | -25232,-30176 | new |
| Timbermaw Hold | 10 | -14892,22008 | new (0.87); the Furbolgs' |

**The Deadmines look like a mine** (0.84). The copy swaps the temple's look:
- Ground: sand becomes rough dirt, tiles become dirt (`--tex`).
- Silithid walls become wooden fences, wall ends become lantern posts, arches become dungeon archways.
- Obelisks become mine carts, pillars become torches, columns become stalagmites or rocks, stakes become wheelbarrows.
- Trash becomes piles of junk; the circle and the altar become piles of treasure.
- Egg sacks become crates, Outland trees become the dungeon's cave walls, magic orbs become glowing torches.
- Plants are removed (`--retype` / `--drop`).

The baked pathing keeps the old doodads' footprints. Fine-tune in the editor if something looks off.

The 24p creeps are not copied. A CP is Neutral Hostile, with WL's 5000 life, and it defends itself.

**Creeps (0.87, maintainer)**: a fallback starts with its dungeon's SEAT only; every other CP of a dungeon is guarded
by a Neutral Hostile camp (`tools/shattered-legacies/content/dungeon_content.py`, `Units/Dungeon_Content.json`):

| Dungeon | Guarded CPs | Camp |
|---|---|---|
| Nazjatar | Heart of Azeroth, Lightless Cavern, Gishan Cavern | mur'gul, hydras, a giant sea turtle |
| Shadowforge | Dark Iron Treasury, Siege Foundry | Ragnaros' loyalists: Core Hounds, Fire Wyrms, a Shadowforge Titan; Majordomo Executus (24p, level 8) and two Core Hounds at the west throne (-24313,21273) |
| Deadmines | Goblin Foundry, Ironclad Cove | kobolds, goblin shredders |

**Shadowforge's props (0.87)**: the throne doodad by the Throne Room is the Throne of Thaurissan (`hZiy`, the Dark Iron
HQ), the big dwarf statue by the Siege Foundry the Statue of Thaurissan (`hZix`: the Foundry's holder owns it, the
24p's Will of the Emperor at level 4, +4 armour), and the Shadowforge Black Market (`nZiy`, neutral, a Goblin Merchant
copy) stands by the Treasury: expensive Titanforged trinkets and the 24p's Book of Experience, staves and Granite
Token. All Neutral Passive and invulnerable until the Dark Iron arrive (the market stays neutral).

New doodad types (stock models or imports we already have): `DZv0` Gargoyle Hub, `DZv1` Iron Gate (Undercity),
`DZv2`-`DZv5` naga tower / desolace plant / naga walls (Nazjatar), `DZv6` Shadowforge Barricades, `DZv7` Teleporter
Circle Indicator (Shadowforge). `BZv0` (0.83) is gone: the Deadmines' Outland trees became stock cave walls. For Shadowforge, our own `D030`
"Dark Iron City" (dwarven barracks import) is used instead of the 24p's human barracks. When the 24p map only
changes a stock type (scale limits, tints), the copy keeps the stock type.

## The void that is left (measured 0.83, true void)

Searched like the placements above:

| Area | Room for (walkable box) |
|---|---|
| Western column | 4096 x 4096 at x -28672 .., y 12300 .. 16400 (below the Undercity) |
| South-west, east of Nazjatar | 4096 x 4096 at x -16400 .. -12300, y -32256 .. -28700 |
| South-east, east of the Black Temple | 4096 x 4096 at x 8200 .. 16300, y -32256 .. -28700 |
| South-west strip | 4096 x 4096 at x -28672 .., y -4100 .. 4100, north of our Ny'alotha |
| Small pockets (2048) | x -16400 .. -12300, y 20500 .. 24500 (east of Shadowforge); along the west edge |

No 6144 x 4096 box is left. A bigger dungeon needs a smaller gap or something moved.
