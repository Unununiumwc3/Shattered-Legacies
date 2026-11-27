"""0.87.0 (maintainer): the Exodar leaves Kalimdor -- its base becomes the INSIDE OF THE SHIP, a void-area instance
reached only through the ship (Timelines/Factions/Draenei/ExodarShip.cs). One-shot: moves the map data once.

  python3 tools/shattered-legacies/terrain/exodar_move.py           do it (refuses when already done)
  python3 tools/shattered-legacies/terrain/exodar_move.py --check   report only

Steps
  1. copy_area.py: the Exodar base (SRC below, Regions.ExodarBaseUnlock with its coast) into the void of the western
     column (offset DX, DY) -- terrain, pathing, shadows, doodads (Doodads/Dungeon_Exodar.json)
  2. its preplaced units are moved by the same offset into Units/Exodar_Interior.json (out of their chunk files);
     the wooden gate on the land bridge (h00L) is dropped -- there is no bridge any more
  3. copy_area.py --clear: the old place becomes void -- Azuremyst's Exodar is carved out of Kalimdor
  4. the regions inside (ExodarBaseUnlock, the two slipstream origins) move: Regions/*.json and Regions.g.cs
Then: gen_routes.py (the interior is a landmass of its own; its CP a route node).
"""
import glob
import json
import os
import subprocess
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
COPY = os.path.join(ROOT, "tools", "shattered-legacies", "terrain", "copy_area.py")
REGIONS_CS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Regions.g.cs")
OUT = os.path.join(MAP, "Units", "Exodar_Interior.json")
SRC = (-24704, 5632, -21248, 8960)
DX, DY = -3840, 7168
DROP = {"h00L"}  # Horizontal Wooden Gate (the land bridge to Bloodmyst)
REGIONS = ["ExodarBaseUnlock", "SlipstreamTempestOrigin", "SlipstreamArgusOrigin"]


def code(tid):
    return tid.to_bytes(4, "little").decode("latin1")


def inside(x, y):
    return SRC[0] <= x < SRC[2] and SRC[1] <= y < SRC[3]


def run(args):
    r = subprocess.run([sys.executable, COPY] + args, capture_output=True, text=True)
    print(r.stdout.strip())
    if r.returncode != 0 or "PROBLEM" in r.stdout:
        sys.exit("copy_area.py failed: " + r.stderr.strip())


def main(check):
    if os.path.exists(OUT):
        print("exodar_move: already done (%s exists)" % os.path.relpath(OUT, ROOT))
        return 0
    moved, dropped = [], 0
    files = {}
    for f in glob.glob(os.path.join(MAP, "Units", "*.json")):
        units = json.load(open(f, encoding="utf-8"))
        keep = []
        for u in units:
            if inside(u["Position"]["X"], u["Position"]["Y"]):
                if code(u["TypeId"]) in DROP:
                    dropped += 1
                    continue
                u["Position"]["X"] += DX
                u["Position"]["Y"] += DY
                moved.append(u)
            else:
                keep.append(u)
        if len(keep) != len(units):
            files[f] = keep
    print("exodar_move: %d units to move, %d dropped, %d unit files touched" % (len(moved), dropped, len(files)))
    if check:
        run(["Exodar"] + [str(v) for v in SRC] + [str(DX), str(DY), "--src", MAP, "--check"])
        return 0
    run(["Exodar"] + [str(v) for v in SRC] + [str(DX), str(DY), "--src", MAP])
    for f, keep in files.items():
        with open(f, "w", encoding="utf-8") as fh:
            fh.write(json.dumps(keep, indent=2, ensure_ascii=False))
    with open(OUT, "w", encoding="utf-8") as fh:
        fh.write(json.dumps(moved, indent=2, ensure_ascii=False))
    run(["--clear"] + [str(v) for v in SRC])
    cs = open(REGIONS_CS, encoding="utf-8-sig").read()
    for name in REGIONS:
        path = os.path.join(MAP, "Regions", name + ".json")
        d = json.load(open(path, encoding="utf-8"))
        old = "new Rectangle(%gf, %gf, %gf, %gf)" % (d["Left"], d["Bottom"], d["Right"], d["Top"])
        d["Left"] += DX
        d["Right"] += DX
        d["Bottom"] += DY
        d["Top"] += DY
        new = "new Rectangle(%gf, %gf, %gf, %gf)" % (d["Left"], d["Bottom"], d["Right"], d["Top"])
        with open(path, "w", encoding="utf-8") as fh:
            fh.write(json.dumps(d, indent=2, ensure_ascii=False))
        line = "public static Rectangle %s { get; set; } = " % name
        if line + old not in cs:
            sys.exit("Regions.g.cs: %s not found as %s" % (name, old))
        cs = cs.replace(line + old, line + new)
        print("  region %s -> %s" % (name, new))
    with open(REGIONS_CS, "w", encoding="utf-8-sig") as fh:
        fh.write(cs)
    print("exodar_move: done -- rerun tools/shattered-legacies/ai/gen_routes.py")
    return 0


if __name__ == "__main__":
    sys.exit(main("--check" in sys.argv))
