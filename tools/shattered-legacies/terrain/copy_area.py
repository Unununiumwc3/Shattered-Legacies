"""Copies a rectangle of terrain (+ pathing, shadows, doodads) from one map-data folder into another (0.82, void-area
dungeons). Made for importing the gated dungeons of Warcraft Legacies 24p 1.18a (mapdata/WarcraftLegacies24p) into
the void parts of our map; both maps have the same size and origin (417 x 449 corners from -28672, -32256).

  python3 tools/shattered-legacies/terrain/copy_area.py NAME X0 Y0 X1 Y1 DX DY [--src DIR] [--types A=B,..]
          [--retype A=B,..] [--tex A=B,..] [--drop A,A@X,Y,..] [--check]

  NAME          the dungeon's name: its doodads go to Doodads/Dungeon_NAME.json (the builder reads every file there)
  X0 Y0 X1 Y1   the source rectangle, multiples of 128 (tile corners X0..X1 / Y0..Y1 are copied, both included)
  DX DY         where it goes: offset, multiples of 128
  --src DIR     source map-data folder (default mapdata/WarcraftLegacies24p)
  --types A=B   a custom doodad type of the source we do not have (or define differently): copied into our
                DoodadData / DestructableData under the new id B (fork rule ?Z<lower>?, e.g. DZv0), the copied doodads
                use B. A=A: ours is defined differently but fits -- keep ours (e.g. D030 "Dark Iron City")
  --retype A=B  0.84 (redecoration): the copied doodads of type A become type B -- a stock type or one we define; no
                object data is copied. Variation 0, scale kept within 0.7 .. 1.3 (the old scale was made for another
                model). The pathing map keeps the old doodad's baked pathing.
  --tex A=B     0.84: ground texture A (fourcc) becomes B inside the copy (both must be in our TerrainTypes)
  --drop A      0.84: leave out every doodad of type A (its source or its new id); A@X,Y only the one within 96 of X,Y (OUR coordinates) --
                e.g. a gate doodad where the code places a waygate with the same model
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

  python3 tools/shattered-legacies/terrain/copy_area.py --clear X0 Y0 X1 Y1 [--check]

turns a rectangle of OUR map into void (0.83: WL's unused squares in the south-west corner made room for Nazjatar):
every corner becomes the void corner (boundary, cliff level 2, height 0.875, texture 1 -- the most common void corner
around), every cell unwalkable / unflyable / unbuildable (206, as the void), no shadow, and its doodads are removed.
It refuses if preplaced units stand there.

The destination must be void (every pathing cell unflyable -- deep sea is unwalkable too, but ships and flyers cross
it; no doodads, no units) -- or already hold this very copy,
so the tool can be run again after a change.
"""
import glob
import json
import os
import re
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
    """new doodad / destructable types (a file with a NewId). A file without one only modifies a stock type (24p:
    scale limits, tints) -- such types count as stock: ours keeps the stock values."""
    ids = set()
    for sub in ("DoodadData", "DestructableData"):
        for f in glob.glob(os.path.join(folder, sub, "*.json")):
            if load(f).get("NewId"):
                ids.add(os.path.splitext(os.path.basename(f))[0])
    return ids


def imported_models(folder):
    out = set()
    for f in glob.glob(os.path.join(folder, "Imports", "war3mapImported", "*")):
        out.add(os.path.splitext(os.path.basename(f))[0].lower())
    return out


def model_of(folder, t):
    for sub in ("DoodadData", "DestructableData"):
        f = os.path.join(folder, sub, t + ".json")
        if os.path.exists(f):
            obj = load(f)
            return obj["OldId"], [m["Value"] for m in obj["Modifications"] if m["Id"] in ("dfil", "bfil")]
    return None


def inside(p, x0, y0, x1, y1):
    return x0 <= p["X"] < x1 and y0 <= p["Y"] < y1


VOID_CORNER = {"Height": 0.875, "WaterHeight": 1, "IsEdgeTile": False, "Texture": 1, "IsRamp": False,
               "IsBlighted": False, "IsWater": False, "IsBoundary": True, "CliffLevel": 2, "CliffTexture": 1}
VOID_CELL = 206


def clear(argv, check):
    x0, y0, x1, y1 = (int(a) for a in argv)
    if any(v % TILE for v in (x0, y0, x1, y1)) or x1 <= x0 or y1 <= y0:
        print("rectangle must be multiples of 128, X0 < X1, Y0 < Y1")
        return 2
    units = []
    for f in glob.glob(os.path.join(DST, "Units", "*.json")):
        units.extend(u for u in load(f) if inside(u["Position"], x0, y0, x1, y1))
    de, dp, ds = (load(os.path.join(DST, n)) for n in ("Environment.json", "PathingMap.json", "ShadowMap.json"))
    w, pw = de["Width"], dp["Width"]
    corners = [r * w + c for r in range((y0 - MINY) // TILE, (y1 - MINY) // TILE + 1)
               for c in range((x0 - MINX) // TILE, (x1 - MINX) // TILE + 1)]
    cells = [cy * pw + cx for cy in range((y0 - MINY) // CELL, (y1 - MINY) // CELL)
             for cx in range((x0 - MINX) // CELL, (x1 - MINX) // CELL)]
    files = {}
    gone = 0
    for f in sorted(glob.glob(os.path.join(DST, "Doodads", "*.json"))):
        ds_ = load(f)
        keep = [d for d in ds_ if not inside(d["Position"], x0, y0, x1, y1)]
        if len(keep) != len(ds_):
            files[f] = keep
            gone += len(ds_) - len(keep)
    walk = sum(1 for i in cells if not dp["Cells"][i] & 0x02)
    print("clear %d,%d .. %d,%d: corners %d, cells %d (%d walkable), doodads %d, units %d" % (
        x0, y0, x1, y1, len(corners), len(cells), walk, gone, len(units)))
    if units:
        print("  PROBLEM: preplaced units stand there")
        return 1
    if check:
        return 0
    for i in corners:
        t = de["TerrainTiles"][i]
        t.update(VOID_CORNER)
    for i in cells:
        dp["Cells"][i] = VOID_CELL
        ds["Cells"][i] = 0
    save(os.path.join(DST, "Environment.json"), de)
    save(os.path.join(DST, "PathingMap.json"), dp)
    save(os.path.join(DST, "ShadowMap.json"), ds)
    for f, keep in files.items():
        if keep:
            save(f, keep)
        else:
            os.remove(f)
    print("  written: Environment.json, PathingMap.json, ShadowMap.json, %d doodad files" % len(files))
    return 0


def main(argv):
    check = "--check" in argv
    if "--clear" in argv:
        rest = [a for a in argv if a not in ("--clear", "--check")]
        return clear(rest, check)
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
    retype, texmap, drops = {}, {}, []
    for flag in ("--retype", "--tex", "--drop"):
        if flag in argv:
            i = argv.index(flag)
            items = argv[i + 1].split(",") if flag != "--drop" else re.split(r",(?![-\d])", argv[i + 1])
            del argv[i:i + 2]
            if flag == "--retype":
                retype = dict(p.split("=") for p in items)
            elif flag == "--tex":
                texmap = dict(p.split("=") for p in items)
            else:
                for item in items:
                    t, _, at = item.partition("@")
                    drops.append((t, tuple(float(v) for v in at.split(",")) if at else None))
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
            fcc = code(de["TerrainTypes"][t["Texture"]])
            if fcc in texmap:
                if struct.unpack("<I", texmap[fcc].encode("latin1"))[0] not in de["TerrainTypes"]:
                    problems.append("texture %s is not in our TerrainTypes" % texmap[fcc])
                    break
                t["Texture"] = de["TerrainTypes"].index(struct.unpack("<I", texmap[fcc].encode("latin1"))[0])
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
    void = all(dp["Cells"][d] & 0x04 for _, d in cells)  # 0.83: the void is UNFLYABLE (deep sea is not void)

    # --- doodads
    known, theirs = custom_types(DST), custom_types(src)
    moved = []
    for _, d in doodads(src):
        if inside(d["Position"], x0, y0, x1, y1):
            t = code(d["TypeId"])
            if any(dt in (t, remap.get(t), retype.get(t)) and (at is None or abs(d["Position"]["X"] + dx - at[0]) <= 96
                                and abs(d["Position"]["Y"] + dy - at[1]) <= 96) for dt, at in drops):
                continue
            if t in retype:
                moved.append(d)
                continue
            if code(d["TypeId"]) in theirs and code(d["TypeId"]) not in known and code(d["TypeId"]) not in remap:
                problems.append("doodad type %s is custom in the source and unknown here" % code(d["TypeId"]))
            elif code(d["TypeId"]) in theirs and code(d["TypeId"]) in known and code(d["TypeId"]) not in remap \
                    and model_of(src, code(d["TypeId"])) != model_of(DST, code(d["TypeId"])):
                problems.append("doodad type %s is custom in both maps with different models" % code(d["TypeId"]))
            moved.append(d)
    have = imported_models(DST)
    for old, new in remap.items():
        if old == new:
            continue
        mod = model_of(src, old)
        for path in (mod[1] if mod else []):
            if str(path).lower().startswith("war3mapimported\\"):
                base = os.path.splitext(str(path)[16:])[0].lower()
                if base not in have:
                    problems.append("type %s uses the import %s -- not in our Imports" % (old, path))
        for sub in ("DoodadData", "DestructableData"):
            mine, theirs_f = os.path.join(DST, sub, new + ".json"), os.path.join(src, sub, old + ".json")
            if os.path.exists(mine):
                want = dict(load(theirs_f), NewId=new) if os.path.exists(theirs_f) else None
                if load(mine) != want:
                    problems.append("new id %s is taken (%s)" % (new, sub))
    ours = [(f, d) for f, d in doodads(DST) if f != own_file]
    blocking = [d for f, d in ours if inside(d["Position"], x0 + dx, y0 + dy, x1 + dx, y1 + dy)]
    units = []
    for f in glob.glob(os.path.join(DST, "Units", "*.json")):
        if os.path.basename(f).startswith("Dungeon_"):
            continue  # the dungeons' own CPs (content/void_dungeons.py)
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
        if old == new:
            continue
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
        if t in remap and remap[t] != t:
            n["TypeId"] = struct.unpack("<I", remap[t].encode("latin1"))[0]
            if code(n["SkinId"]) == t:
                n["SkinId"] = n["TypeId"]
        elif t in retype:
            n["TypeId"] = n["SkinId"] = struct.unpack("<I", retype[t].encode("latin1"))[0]
            n["Variation"] = 0
            for k in ("X", "Y", "Z"):
                n["Scale"][k] = min(1.3, max(0.7, n["Scale"][k]))
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
