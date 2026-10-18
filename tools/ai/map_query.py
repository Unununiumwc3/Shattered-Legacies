"""Offline map checks for the WL bot AI -- reads the map's SOURCE files (mapdata/WarcraftLegacies), no game needed.
Use it before putting any coordinate into AiConfig.cs (maintainer coordinates had sign typos 3 times).

  python3 tools/ai/map_query.py walk X Y            terrain walkable? (+ buildable, AI landmass id)
  python3 tools/ai/map_query.py area X Y HALF       is the whole square +-HALF around X,Y walkable?
  python3 tools/ai/map_query.py near X Y [R]        walkable point nearest to X,Y (search radius R, default 1500)
  python3 tools/ai/map_query.py find CODE           preplaced units of a type ('n00J' or 1848651850): x y owner
  python3 tools/ai/map_query.py around X Y [R]      preplaced units within R (default 800) of X,Y
  python3 tools/ai/map_query.py rect NAME           centre of a Regions.X rectangle (Regions.g.cs) + walkability

Data facts (0.36): PathingMap.json = 32-unit cells, rows bottom-up from (-28672, -32256), flag 0x02 = not walkable,
0x08 = not buildable; terrain only (doodads, destructables and units are NOT in it). Preplaced units in Units/*.json
store TypeId little-endian ('n00J' -> struct '<I'). Landmass ids come from the Geography grid in AiSystem.cs.
Run from the repo root (or set WL_ROOT)."""
import glob, json, os, re, struct, sys

ROOT = os.environ.get("WL_ROOT", os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
MINX, MINY = -28672.0, -32256.0
_pm = None
_grid = None
_units = None


def _pathing():
    global _pm
    if _pm is None:
        d = json.load(open(os.path.join(MAP, "PathingMap.json")))
        _pm = (d["Width"], d["Height"], d["Cells"])
    return _pm


def cell(x, y):
    w, h, c = _pathing()
    col, row = int((x - MINX) // 32), int((y - MINY) // 32)
    return 255 if col < 0 or col >= w or row < 0 or row >= h else c[row * w + col]


def walkable(x, y):
    return cell(x, y) & 0x02 == 0


def buildable(x, y):
    return cell(x, y) & 0x0A == 0


def area_walkable(x, y, half):
    return all(walkable(x + a, y + b) for a in range(-half, half + 1, 32) for b in range(-half, half + 1, 32))


def landmass(x, y):
    global _grid
    if _grid is None:
        src = open(os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "AiSystem.cs"), encoding="utf-8").read()
        _grid = re.search(r'Grid =\s*"([^"]*)"', src).group(1)
    col, row = int((x + 28672) / 128), int((y + 32256) / 128)
    if col < 0 or col >= 416 or row < 0 or row >= 448:
        return -1
    ch = _grid[row * 416 + col]
    return -1 if ch == "." else ord(ch) - 65


def units():
    global _units
    if _units is None:
        _units = []
        for f in glob.glob(os.path.join(MAP, "Units", "*.json")):
            _units.extend(json.load(open(f)))
    return _units


def type_id(code):
    if isinstance(code, int) or code.lstrip("-").isdigit():
        code = struct.pack(">I", int(code)).decode("latin1")
    return struct.unpack("<I", code.encode("latin1"))[0]


def code_of(tid):
    return struct.pack("<I", tid).decode("latin1")


def nearest_walkable(x, y, r=1500, half=64):
    best = None
    for dx in range(-r, r + 1, 32):
        for dy in range(-r, r + 1, 32):
            d = dx * dx + dy * dy
            if d <= r * r and (best is None or d < best[0]) and area_walkable(x + dx, y + dy, half):
                best = (d, x + dx, y + dy)
    return best


def main(a):
    if len(a) < 2:
        print(__doc__)
        return
    cmd = a[0]
    if cmd == "walk":
        x, y = float(a[1]), float(a[2])
        print(f"walkable={walkable(x, y)} buildable={buildable(x, y)} landmass={landmass(x, y)} flags=0x{cell(x, y):02x}")
    elif cmd == "area":
        x, y, h = float(a[1]), float(a[2]), int(a[3])
        print(f"square +-{h} walkable={area_walkable(x, y, h)} landmass={landmass(x, y)}")
    elif cmd == "near":
        x, y = float(a[1]), float(a[2])
        b = nearest_walkable(x, y, int(a[3]) if len(a) > 3 else 1500)
        print("none" if b is None else f"{b[1]:.0f} {b[2]:.0f}  (dist {b[0] ** 0.5:.0f}, landmass {landmass(b[1], b[2])})")
    elif cmd == "find":
        t = type_id(a[1])
        for u in units():
            if u["TypeId"] == t:
                p = u["Position"]
                print(f"{p['X']:.0f} {p['Y']:.0f}  owner={u.get('OwnerId')}  landmass={landmass(p['X'], p['Y'])}")
    elif cmd == "around":
        x, y = float(a[1]), float(a[2])
        r = float(a[3]) if len(a) > 3 else 800
        for u in units():
            p = u["Position"]
            if (p["X"] - x) ** 2 + (p["Y"] - y) ** 2 <= r * r:
                print(f"{code_of(u['TypeId'])} {p['X']:.0f} {p['Y']:.0f} owner={u.get('OwnerId')}")
    elif cmd == "rect":
        src = open(os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Regions.g.cs"), encoding="utf-8").read()
        m = re.search(r"Rectangle " + re.escape(a[1]) + r" \{[^=]*= new Rectangle\(([-\d.]+)f, ([-\d.]+)f, ([-\d.]+)f, ([-\d.]+)f\)", src)
        if not m:
            print("no such rectangle")
            return
        x0, y0, x1, y1 = map(float, m.groups())
        cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
        print(f"rect {x0:.0f},{y0:.0f} .. {x1:.0f},{y1:.0f}  centre {cx:.0f},{cy:.0f}  walkable={area_walkable(cx, cy, 96)}"
              f"  landmass={landmass(cx, cy)}")
    else:
        print(__doc__)


if __name__ == "__main__":
    main(sys.argv[1:])
