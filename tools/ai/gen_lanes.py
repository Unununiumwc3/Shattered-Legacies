"""Sea lanes for the bot ferries (0.50) -- from map data only.

Usage (repo root): python3 tools/ai/gen_lanes.py            writes src/WarcraftLegacies.Source/Ai/AiLanes.g.cs
                   python3 tools/ai/gen_lanes.py --check    only prints the lanes

Reads the ferry harbour pairs from AiConfig.cs (the PortalLink entries with phase "ship": pier A x,y and pier B x,y)
and PathingMap.json (32-unit cells, rows bottom-up from (-28672, -32256); bit 0x40 = NOT water). For every pair:
A* over a coarse grid (Coarse x Coarse cells) whose cells are water in every 32-unit cell (keeps the boat off the
shore), from the water cell nearest to pier A to the one nearest to pier B; then line-of-sight smoothing. The lane
is a polyline in world coordinates: first point = where the boat appears near pier A, last = where it ends near pier
B. B -> A uses the same lane reversed. Re-run after map changes; the C# file is generated, never edit it by hand.
"""
import heapq
import json
import math
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies", "PathingMap.json")
CONFIG = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "AiConfig.cs")
OUT = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "AiLanes.g.cs")
MINX, MINY, CELL = -28672, -32256, 32
COARSE = 4            # 4 x 4 cells = 128 units
SEARCH = 2500         # max distance from a pier to its first water point


def load():
    d = json.load(open(MAP, encoding="utf-8-sig"))
    return d["Width"], d["Height"], d["Cells"]


W, H, CELLS = load()
CW, CH = W // COARSE, H // COARSE


def water_cell(cx, cy):
    if cx < 0 or cy < 0 or cx >= W or cy >= H:
        return False
    return not (CELLS[cy * W + cx] & 0x40)


_coarse = {}
NEED = [COARSE * COARSE]  # water cells a coarse cell needs: all of them, else (fallback) at least half


def coarse_water(gx, gy):
    k = (gx, gy, NEED[0])
    if k not in _coarse:
        n = 0
        if 0 <= gx < CW and 0 <= gy < CH:
            for dy in range(COARSE):
                for dx in range(COARSE):
                    if water_cell(gx * COARSE + dx, gy * COARSE + dy):
                        n += 1
        _coarse[k] = n >= NEED[0]
    return _coarse[k]


def to_grid(x, y):
    return int((x - MINX) // (CELL * COARSE)), int((y - MINY) // (CELL * COARSE))


def to_world(gx, gy):
    s = CELL * COARSE
    return MINX + gx * s + s / 2, MINY + gy * s + s / 2


def nearest_water(x, y):
    gx, gy = to_grid(x, y)
    r = SEARCH // (CELL * COARSE)
    best = None
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            if coarse_water(gx + dx, gy + dy):
                d = dx * dx + dy * dy
                if best is None or d < best[0]:
                    best = (d, gx + dx, gy + dy)
    return (best[1], best[2]) if best else None


def astar(a, b):
    openh = [(0.0, a)]
    g = {a: 0.0}
    prev = {}
    while openh:
        _, u = heapq.heappop(openh)
        if u == b:
            path = [u]
            while u in prev:
                u = prev[u]
                path.append(u)
            return path[::-1]
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                if dx == 0 and dy == 0:
                    continue
                v = (u[0] + dx, u[1] + dy)
                if not coarse_water(*v):
                    continue
                if dx and dy and not (coarse_water(u[0] + dx, u[1]) and coarse_water(u[0], u[1] + dy)):
                    continue  # no corner cutting past land
                nd = g[u] + (1.4142 if dx and dy else 1.0)
                if v not in g or nd < g[v]:
                    g[v] = nd
                    prev[v] = u
                    heapq.heappush(openh, (nd + math.hypot(v[0] - b[0], v[1] - b[1]), v))
    return None


def line_clear(a, b):
    steps = int(max(abs(b[0] - a[0]), abs(b[1] - a[1])) * 2) + 1
    for i in range(steps + 1):
        t = i / steps
        x = a[0] + (b[0] - a[0]) * t
        y = a[1] + (b[1] - a[1]) * t
        if not coarse_water(int(round(x)), int(round(y))):
            return False
    return True


def smooth(path):
    out = [path[0]]
    i = 0
    while i < len(path) - 1:
        j = len(path) - 1
        while j > i + 1 and not line_clear(path[i], path[j]):
            j -= 1
        out.append(path[j])
        i = j
    return out


def ship_links():
    src = open(CONFIG, encoding="utf-8-sig").read()
    links = []
    for m in re.finditer(r'new PortalLink\((-?[\d.]+)f, (-?[\d.]+)f, (-?[\d.]+)f, (-?[\d.]+)f, "hbot", "([^"]*)", "([^"]*)", "ship"\)',
                         src):
        links.append((float(m.group(1)), float(m.group(2)), float(m.group(3)), float(m.group(4)), m.group(5), m.group(6)))
    return links


def main():
    check = "--check" in sys.argv
    links = ship_links()
    if not links:
        sys.exit("no ship links (phase \"ship\") found in AiConfig.cs")
    starts, lens, xs, ys, report = [], [], [], [], []
    for (ax, ay, bx, by, na, nb) in links:
        path = None
        for need in (COARSE * COARSE, COARSE * COARSE // 2):  # strict (open sea) first, then narrow channels
            NEED[0] = need
            a, b = nearest_water(ax, ay), nearest_water(bx, by)
            if a is not None and b is not None:
                path = astar(a, b)
            if path is not None:
                break
        if path is None:
            sys.exit(f"no sea lane between {na} and {nb}")
        pts = [to_world(*p) for p in smooth(path)]
        length = sum(math.hypot(pts[i + 1][0] - pts[i][0], pts[i + 1][1] - pts[i][1]) for i in range(len(pts) - 1))
        starts.append(len(xs))
        lens.append(len(pts))
        for x, y in pts:
            xs.append(int(round(x)))
            ys.append(int(round(y)))
        report.append(f"{na} <-> {nb}: {len(pts)} points, {int(length)} long, from {pts[0][0]:.0f},{pts[0][1]:.0f} "
                      f"to {pts[-1][0]:.0f},{pts[-1][1]:.0f}")
    print("\n".join(report))
    if check:
        return

    def arr(v):
        rows = [", ".join(f"{x}f" for x in v[i:i + 16]) for i in range(0, len(v), 16)]
        return ",\n      ".join(rows)

    code = f"""// <auto-generated> tools/ai/gen_lanes.py -- sea lanes of the bot ferries (from map data). Do not edit. </auto-generated>
namespace WarcraftLegacies.Source.Ai
{{
  // One lane per ship link (AiConfig: PortalLink entries with phase "ship", in their order): points
  // LaneStart[k] .. LaneStart[k] + LaneCount[k] - 1 of LaneX / LaneY, from pier A's water to pier B's water.
{"".join(f"  //   {k}: {r}{chr(10)}" for k, r in enumerate(report))}  internal static class AiLaneData
  {{
    internal static int Lanes = {len(links)};
    internal static int[] LaneStart = {{ {", ".join(map(str, starts))} }};
    internal static int[] LaneCount = {{ {", ".join(map(str, lens))} }};
    internal static float[] LaneX =
    {{
      {arr(xs)}
    }};
    internal static float[] LaneY =
    {{
      {arr(ys)}
    }};
  }}
}}
"""
    open(OUT, "w", encoding="utf-8").write(code)
    print(f"wrote {os.path.relpath(OUT, ROOT)}")


if __name__ == "__main__":
    main()
