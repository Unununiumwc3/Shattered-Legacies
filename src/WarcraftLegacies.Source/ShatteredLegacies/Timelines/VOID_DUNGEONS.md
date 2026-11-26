# Void-area dungeons (0.82.0, 0.83.0)

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

The commands (rerunnable):

```
T=tools/shattered-legacies/terrain/copy_area.py
python3 $T Undercity -28672 14208 -25728 17920 0 6912 --types D00O=DZv0,D00K=DZv1
python3 $T --clear -23296 -30464 -9728 -24704
python3 $T Nazjatar 7936 -31488 15744 -26112 -31104 512 --types D03U=DZv2,D020=DZv3,D03V=DZv4,D03Q=DZv5
python3 $T Shadowforge 16256 -32000 22784 -28288 -40704 53120 --types D06C=DZv6,D00F=DZv7,D030=D030
python3 $T Deadmines -28416 -16640 -24192 -8064 -256 -14848 --types B00U=BZv0
python3 tools/shattered-legacies/content/void_dungeons.py
python3 tools/shattered-legacies/ai/gen_routes.py
```

Gates (stock waygates, invulnerable, Neutral Passive; code: `Dungeons/VoidDungeons.cs`, the Undercity in
`Factions/Forsaken/ForsakenUndercity.cs`):

| Dungeon | Inner gate (24p entrance) | World entrance | Open |
|---|---|---|---|
| Undercity | -26528,21344 | Lordaeron's palace ruins | "The Throne of Lordaeron" (Forsaken) |
| Nazjatar | -18496,-29056 (24p "NazjatarExit2") | -3744,-3552, by WL's Nazjatar CP beside the Maelstrom (shallow water) | from the start |
| Shadowforge | -21472,21408 (24p entrance portal) | 12480,-9152, east side of WL's Shadowforge City | from the start |
| Deadmines | -27808,-23904 (24p north portal) | 7584,-16896, Westfall, above the bandit camp | from the start |

Control points (`tools/shattered-legacies/content/void_dungeons.py`: types `nZv*`, preplaced Neutral Hostile, with
Chinese names). The 24p names are kept except where our map already has the name or the dungeon changed:

| CP | Gold | Spot | 24p |
|---|---|---|---|
| Undercity | 20 | -27139,22825 | n0BC "Undercity"; goes to the Forsaken with the buildings |
| Eternal Palace | 25 | -22208,-30016 | n05Q "Nazjatar" (WL has a Nazjatar CP at the Maelstrom) |
| Heart of Azeroth | 30 | -19008,-27904 | n0A0 |
| Lightless Cavern | 15 | -16192,-28352 | n05W |
| Gishan Cavern | 10 | -20288,-26368 | n04E |
| Shadowforge City | 20 | -22144,22784 | n0AA "Blackrock Depths" (WL has that CP in the world) |
| Molten Core | 15 | -23040,23936 | n00P |
| The Grim Guzzler | 10 | -18944,23360 | n00O "Blackrock Spire" (WL has an orc building of that name) |
| The Deadmines | 20 | -26048,-26880 | n078 "Temple of Ahn'Qiraj" |
| Goblin Foundry | 10 | -27952,-28256 | new |
| Ironclad Cove | 15 | -25232,-30176 | new |

The Deadmines still look like the 24p temple (sand, titan rock, temple doodads): it gets redecorated later.

The 24p creeps are not copied. A CP is Neutral Hostile, with WL's 5000 life, and it defends itself.

New doodad types (stock models or imports we already have): `DZv0` Gargoyle Hub, `DZv1` Iron Gate (Undercity),
`DZv2`-`DZv5` naga tower / desolace plant / naga walls (Nazjatar), `DZv6` Shadowforge Barricades, `DZv7` Teleporter
Circle Indicator (Shadowforge), destructable `BZv0` Outland Tree 2 (Deadmines). For Shadowforge, our own `D030`
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
