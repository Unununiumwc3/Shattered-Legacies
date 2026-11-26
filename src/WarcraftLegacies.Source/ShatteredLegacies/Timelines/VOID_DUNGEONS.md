# Void-area dungeons (0.82.0)

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

| Dungeon | 24p source | Offset | Our rectangle (with wall) | Landmass | Code |
|---|---|---|---|---|---|
| Undercity | -28672,14208 .. -25728,17920 | 0, +6912 | -28672,21120 .. -25728,24832 | 20 `U` | `Factions/Forsaken/ForsakenUndercity.cs` |

The Undercity sits in the north-west corner, not in its 24p place. This keeps the western column and the northern
band in one piece for the bigger dungeons. Its buildings stand at their 24p spots. The inner gate stands at the 24p
southern entrance. The 24p map's control point (`n0BC`, at -27136,22848 here) is free for a later Undercity CP: the
Forsaken join whoever holds it.

## The void that is left (our map, measured 0.82)

| Area | About | Notes |
|---|---|---|
| Northern band | x -25728 .. -8700, y 18400 .. 24800 (down to 16900 west of -10700) | east of the Undercity; Scarlet Harbor's old region at -9728,21984 lies in it (void) |
| Western column | x -28672 .. -20500, y 5600 .. 21100 | west of Teldrassil; narrower (to ~-24600) next to the Exodar, y 5600 .. 9000 |
| Middle pocket | x -16400 .. -9700, y 12800 .. 16900 | east of Teldrassil; joins the band above |
| South-west strip | x -28672 .. -26000, y -3500 .. 5100 | north of our Ny'alotha (-28672,-11936 .. -22016,-3360) |
| South-west squares | 7 squares of ~1900 x 1900 around -11000,-29000 | landmasses `L`-`R`; the 0.57 Undercity square is free again |

These 24p dungeons would fit **in their 24p place**: the rectangle is void here, with no doodads and no units.

| 24p dungeon | 24p rectangle | Notes |
|---|---|---|
| Scholomance | -21216,17344 .. -19136,19808 | ours is in the world (Plaguelands) |
| Dire Maul | -18784,20544 .. -15904,24544 | |
| Dalaran Dungeon | -14432,20192 .. -10752,24096 | |
| Scarlet Monastery | -25152,15072 .. -22592,17696 | |
| Naxxramas | -24928,11520 .. -22016,14528 | |
| Twisting Nether | -28320,10560 .. -26272,13600 | |

These would need a **new place**:

| 24p dungeon | 24p rectangle | Blocked by | Candidate |
|---|---|---|---|
| Nazjatar (Sentinels' naga fallback, exit near the Maelstrom) | 7872,-31584 .. 15552,-26624 (7700 x 5000) | the east side of our Black Temple (walkable to x 9920); east of it only ~5900 of void | the western column (e.g. x -28672 .. -20992, y 11500 .. 16500 -- then the Twisting Nether / Naxxramas / Scarlet spots are gone) |
| Blackrock Depths / Shadowforge City (Dark Iron fallback) | 16416,-32032 .. 22592,-28544 (Shadowforge City 16704,-31744 .. 18272,-30560) | our Argus (MonolithNoBuild 18208,-31584 ..) | northern band between the Undercity and Dire Maul's spot (~7000 x 5000) |
| Temple of Ahn'Qiraj (24p: a gated instance) | -28480,-16416 .. -23872,-8000 | our Ny'alotha | for the Deadmines (maintainer: "reuse that one and rename the control point"). It is a sand / titan-rock temple (Zsan, Qrck), so it needs new doodads to look like a mine |
| Aetheneum Catacombs | -28160,17952 .. -23072,24544 | now the Undercity | only if wanted |
