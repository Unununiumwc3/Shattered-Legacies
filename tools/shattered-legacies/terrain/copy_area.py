"""Copies a rectangle of terrain (+ pathing, shadows, doodads) from one map-data folder into another (0.82, void-area
dungeons). Made for importing the gated dungeons of Warcraft Legacies 24p 1.18a (mapdata/WarcraftLegacies24p) into
the void parts of our map; both maps have the same size and origin (417 x 449 corners from -28672, -32256).

  python3 tools/shattered-legacies/terrain/copy_area.py NAME X0 Y0 X1 Y1 DX DY [--src DIR] [--types A=B,..] [--check]

  NAME          the dungeon's name: its doodads go to Doodads/Dungeon_NAME.json (the builder reads every file there)
  X0 Y0 X1 Y1   the source rectangle, multiples of 128 (tile corners X0..X1 / Y0..Y1 are copied, both included)
  DX DY         where it goes: offset, multiples of 128
  --src DIR     source map-data folder (default mapdata/WarcraftLegacies24p)
  --types A=B   a custom doodad type of the source we do not have: copied into our DoodadData / DestructableData
                under the new id B (fork rule ?Z<lower>?, e.g. DZv0), the copied doodads use B
  --check       report only, write nothing

What is copied
  Environment.json  every corner (height, water, cliffs, flags, variations); the ground texture is remapped by its
                    fourcc into our TerrainTypes list, the cliff texture into our CliffTypes list (an unknown one stops
                    the copy)
  PathingMap.json   the 32-unit cells (they include the doodads' pathing -- the editor bakes it in)
  ShadowMap.json    the 32-unit cells
  Doodads           every doodad / destructable standing in the rectangle, moved by DX, DY, new CreationNumbers after
                    our highest; a custom type we do not define stops the copy
NOT copied: units (the dungeon's buildings and gates are placed by code), regions, the minimap.

The destination must be void (every pathing cell unwalkable, no doodads, no units) -- or already hold this very copy,
so the tool can be run again after a change.
"""
import glob
import json
import os
import struct
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".."))
DST = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
MINX, MINY, TILE, CELL = -28672, -32256, 128, 32


def load(path):
    with open(path, encoding="utf-8") as f:
        return json.load(f)


def save(path, data):
    # the exact layout of the map-data files (round trip checked: indent 2, no trailing newline)
    with open(path, "w", encoding="utf-8") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def code(v):
    return struct.pack("<I", v).decode("latin1")


def doodads(folder):
    out = []
    for f in sorted(glob.glob(os.path.join(folder, "Doodads", "*.json"))):
        out.extend((os.path.basename(f), d) for d in load(f))
    return out


def custom_types(folder):
    ids = set()
    for sub in ("DoodadData", "DestructableData"):
        for f in glob.glob(os.path.join(folder, sub, "*.json")):
            ids.add(os.path.splitext(os.path.basename(f))[0])
    return ids


def model_of(folder, t):
    for sub in ("DoodadData", "DestructableData"):
        f = os.path.join(folder, sub, t + ".json")
        if os.path.exists(f):
            obj = load(f)
            return obj["OldId"], [m["Value"] for m in obj["Modifications"] if m["Id"] in ("dfil", "bfil")]
    return None


def inside(p, x0, y0, x1, y1):
    return x0 <= p["X"] < x1 and y0 <= p["Y"] < y1


def main(argv):
    check = "--check" in argv
    argv = [a for a in argv if a != "--check"]
    src = os.path.join(ROOT, "mapdata", "WarcraftLegacies24p")
    if "--src" in argv:
        i = argv.index("--src")
        src = os.path.abspath(argv[i + 1])
        del argv[i:i + 2]
    remap = {}
    if "--types" in argv:
        i = argv.index("--types")
        remap = dict(p.split("=") for p in argv[i + 1].split(","))
        del argv[i:i + 2]
    if len(argv) != 7:
        print(__doc__)
        return 2
    name = argv[0]
    x0, y0, x1, y1, dx, dy = (int(a) for a in argv[1:])
    if any(v % TILE for v in (x0, y0, x1, y1, dx, dy)) or x1 <= x0 or y1 <= y0:
        print("rectangle and offset must be multiples of 128, X0 < X1, Y0 < Y1")
        return 2
    own_file = "Dungeon_" + name + ".json"
    problems = []

    # --- terrain corners
    se, de = load(os.path.join(src, "Environment.json")), load(os.path.join(DST, "Environment.json"))
    w = de["Width"]
    if (se["Width"], se["Height"], se["Left"], se["Bottom"]) != (de["Width"], de["Height"], de["Left"], de["Bottom"]):
        problems.append("the maps differ in size or origin")
    tex = {}
    for i, fcc in enumerate(se["TerrainTypes"]):
        tex[i] = de["TerrainTypes"].index(fcc) if fcc in de["TerrainTypes"] else None
    cliff = {}
    for i, fcc in enumerate(se["CliffTypes"]):
        cliff[i] = de["CliffTypes"].index(fcc) if fcc in de["CliffTypes"] else None
    c0, c1 = (x0 - MINX) // TILE, (x1 - MINX) // TILE
    r0, r1 = (y0 - MINY) // TILE, (y1 - MINY) // TILE
    oc, orow = dx // TILE, dy // TILE
    if c0 + oc < 0 or c1 + oc >= w or r0 + orow < 0 or r1 + orow >= de["Height"]:
        problems.append("the destination leaves the map")
    corners = []
    for r in range(r0, r1 + 1):
        for c in range(c0, c1 + 1):
            t = dict(se["TerrainTiles"][r * w + c])
            if tex.get(t["Texture"]) is None:
                problems.append("ground texture %s is not in our TerrainTypes" % code(se["TerrainTypes"][t["Texture"]]))
                break
            if t["CliffTexture"] in cliff and cliff[t["CliffTexture"]] is None:
                problems.append("cliff texture %s is not in our CliffTypes" % code(se["CliffTypes"][t["CliffTexture"]]))
                break
            t["Texture"] = tex[t["Texture"]]
            if t["CliffTexture"] in cliff:
                t["CliffTexture"] = cliff[t["CliffTexture"]]
            corners.append(((r + orow) * w + c + oc, t))

    # --- pathing / shadow cells
    sp, dp = load(os.path.join(src, "PathingMap.json")), load(os.path.join(DST, "PathingMap.json"))
    ss, ds = load(os.path.join(src, "ShadowMap.json")), load(os.path.join(DST, "ShadowMap.json"))
    pw = dp["Width"]
    cx0, cx1, cy0, cy1 = (x0 - MINX) // CELL, (x1 - MINX) // CELL, (y0 - MINY) // CELL, (y1 - MINY) // CELL
    ocx, ocy = dx // CELL, dy // CELL
    cells = [(cy * pw + cx, (cy + ocy) * pw + cx + ocx) for cy in range(cy0, cy1) for cx in range(cx0, cx1)]
    already = all(dp["Cells"][d] == sp["Cells"][s] for s, d in cells)
    void = all(dp["Cells"][d] & 0x02 for _, d in cells)

    # --- doodads
    known, theirs = custom_types(DST), custom_types(src)
    moved = []
    for _, d in doodads(src):
        if inside(d["Position"], x0, y0, x1, y1):
            if code(d["TypeId"]) in theirs and code(d["TypeId"]) not in known and code(d["TypeId"]) not in remap:
                problems.append("doodad type %s is custom in the source and unknown here" % code(d["TypeId"]))
            elif code(d["TypeId"]) in theirs and code(d["TypeId"]) in known and code(d["TypeId"]) not in remap \
                    and model_of(src, code(d["TypeId"])) != model_of(DST, code(d["TypeId"])):
                problems.append("doodad type %s is custom in both maps with different models" % code(d["TypeId"]))
            moved.append(d)
    ours = [(f, d) for f, d in doodads(DST) if f != own_file]
    blocking = [d for f, d in ours if inside(d["Position"], x0 + dx, y0 + dy, x1 + dx, y1 + dy)]
    units = []
    for f in glob.glob(os.path.join(DST, "Units", "*.json")):
        units.extend(u for u in load(f) if inside(u["Position"], x0 + dx, y0 + dy, x1 + dx, y1 + dy))
    if blocking:
        problems.append("%d doodads of ours stand in the destination" % len(blocking))
    if units:
        problems.append("%d preplaced units of ours stand in the destination" % len(units))
    if not void and not already:
        problems.append("the destination is not void (walkable cells) and not this copy")

    print("%s: %d x %d tiles, corners %d, cells %d, doodads %d -> offset %d, %d (%s)" % (
        name, c1 - c0, r1 - r0, len(corners), len(cells), len(moved), dx, dy,
        "already copied" if already else "void" if void else "NOT void"))
    if problems:
        for p in dict.fromkeys(problems):
            print("  PROBLEM: " + p)
        return 1
    if check:
        return 0

    for i, t in corners:
        de["TerrainTiles"][i] = t
    for s, d in cells:
        dp["Cells"][d] = sp["Cells"][s]
        ds["Cells"][d] = ss["Cells"][s]
    nxt = max(d["CreationNumber"] for _, d in ours) + 1
    out = []
    for old, new in remap.items():
        for sub in ("DoodadData", "DestructableData"):
            f = os.path.join(src, sub, old + ".json")
            if os.path.exists(f):
                obj = load(f)
                obj["NewId"] = new
                save(os.path.join(DST, sub, new + ".json"), obj)
                print("  type %s -> %s/%s.json" % (old, sub, new))
    for d in moved:
        n = json.loads(json.dumps(d))
        t = code(n["TypeId"])
        if t in remap:
            n["TypeId"] = struct.unpack("<I", remap[t].encode("latin1"))[0]
            if code(n["SkinId"]) == t:
                n["SkinId"] = n["TypeId"]
        n["Position"]["X"] += dx
        n["Position"]["Y"] += dy
        n["CreationNumber"] = nxt
        nxt += 1
        out.append(n)
    save(os.path.join(DST, "Environment.json"), de)
    save(os.path.join(DST, "PathingMap.json"), dp)
    save(os.path.join(DST, "ShadowMap.json"), ds)
    save(os.path.join(DST, "Doodads", own_file), out)
    print("  written: Environment.json, PathingMap.json, ShadowMap.json, Doodads/" + own_file)
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
