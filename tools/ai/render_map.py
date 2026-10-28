"""Draws the map as the bot sees it: terrain (water, cliffs / mountains, the map edge), trees, WL's removable rocks,
CPs, area nodes, and the generated route graph. For discussing whether the graph fits the terrain.

  python3 tools/ai/render_map.py                 (repo root; after gen_routes.py)
    -> tools/ai/out/route_map.png        full size, 2 px per 32-unit cell, CP names (zoom in)
    -> tools/ai/out/route_map_small.png  half size overview
  python3 tools/ai/render_map.py --regions       also tint every node's area (region map)

Legend (also drawn on the image): beige = walkable land, darker beige = walkable but not buildable, grey-brown =
cliffs / mountains / doodads you cannot walk, light blue = shallow water (walkable), dark blue = deep water, black = map
edge. Green dots = trees. Red squares = WL's removable rocks (LTrc). Gold circles = CPs (size by income), white rings =
area nodes. White lines = walking edges, red dashed = edges closed while rocks stand, purple dashed = hand-written
links the generator cannot walk (waygates, ships, portals -- added at runtime from live waygates).
"""
import glob
import math
import os
import re
import struct
import sys

from PIL import Image, ImageDraw, ImageFont

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import gen_routes as g  # noqa: E402

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")
PX = 2  # pixels per 32-unit cell
FONT = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
FONT_BOLD = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"

LAND = (222, 206, 170)
LAND_NOBUILD = (196, 180, 146)
BLOCKED = (110, 96, 84)
SHALLOW = (150, 196, 230)
DEEP = (40, 70, 130)
EDGE_OF_MAP = (15, 15, 15)
TREE = (40, 110, 50)
ROCK = (210, 30, 30)
CP = (240, 190, 30)
EDGE = (255, 255, 255)
ROCK_EDGE = (220, 30, 30)
PORTAL = (170, 60, 220)


def font(size, bold=False):
    try:
        return ImageFont.truetype(FONT_BOLD if bold else FONT, size)
    except OSError:
        return ImageFont.load_default()


def terrain():
    d = g.load(os.path.join(g.MAP, "PathingMap.json"))
    w, h, cells = d["Width"], d["Height"], d["Cells"]
    img = Image.new("RGB", (w, h))
    px = img.load()
    for r in range(h):
        row = r * w
        y = h - 1 - r  # map y grows upwards, image y downwards
        for c in range(w):
            f = cells[row + c]
            if f & 0x04 and f & 0x02:
                col = EDGE_OF_MAP
            elif not f & 0x40:  # water
                col = DEEP if f & 0x02 else SHALLOW
            elif f & 0x02:
                col = BLOCKED
            else:
                col = LAND_NOBUILD if f & 0x08 else LAND
            px[c, y] = col
    return img.resize((w * PX, h * PX), Image.NEAREST), w, h


def to_px(x, y, h):
    return (x - g.MINX) / 32.0 * PX, (h - (y - g.MINY) / 32.0) * PX


def dashed(draw, a, b, color, width, dash=14):
    (x1, y1), (x2, y2) = a, b
    length = math.hypot(x2 - x1, y2 - y1) or 1
    n = int(length // dash)
    for i in range(0, n, 2):
        s, e = i / n, min((i + 1) / n, 1)
        draw.line([(x1 + (x2 - x1) * s, y1 + (y2 - y1) * s), (x1 + (x2 - x1) * e, y1 + (y2 - y1) * e)],
                  fill=color, width=width)


def read_graph():
    src = open(g.OUT, encoding="utf-8").read()

    def arr(name):
        body = re.search(name + r" =\s*\{([^}]*)\}", src).group(1)
        return [int(v) for v in body.replace("\n", " ").split(",") if v.strip()]

    regions = re.search(r'Regions =\s*"([^"]*)"', src)
    return {n: arr(n) for n in ("NodeType", "NodeX", "NodeY", "EdgeA", "EdgeB", "EdgeCost", "EdgeRocks")}, \
        (regions.group(1) if regions else None)


def cp_info():
    """CP type code -> (name, gold per minute) from the marker ability's name ('Income 10 gold per minute')."""
    info = {}
    abil_gold = {}
    for f in glob.glob(os.path.join(g.MAP, "AbilityData", "*.json")):
        m = g.mods(g.load(f))
        name = str(m.get("anam") or "")
        mg = re.search(r"Income (\d+) gold", name)
        if mg:
            abil_gold[os.path.basename(f)[:4]] = int(mg.group(1))
    for f in glob.glob(os.path.join(g.MAP, "UnitData", "*.json")):
        m = g.mods(g.load(f))
        gold = 0
        for a in str(m.get("uabi") or "").split(","):
            gold = max(gold, abil_gold.get(a.strip(), 0))
        info[os.path.basename(f)[:4]] = (m.get("unam") or os.path.basename(f)[:4], gold)
    return info


def main():
    with_regions = "--regions" in sys.argv
    img, w, h = terrain()
    draw = ImageDraw.Draw(img, "RGBA")
    data, regions = read_graph()

    if with_regions and regions:
        rw, rh, rc = 208, 224, 256
        for r in range(rh):
            for c in range(rw):
                i = (r * rw + c) * 2
                if regions[i] == ".":
                    continue
                v = g.LETTERS.index(regions[i]) * 52 + g.LETTERS.index(regions[i + 1])
                rnd = (v * 2654435761) & 0xFFFFFF
                col = ((rnd >> 16) & 255, (rnd >> 8) & 255, rnd & 255, 55)
                x0, y0 = to_px(g.MINX + c * rc, g.MINY + (r + 1) * rc, h)
                draw.rectangle([x0, y0, x0 + rc / 32 * PX, y0 + rc / 32 * PX], fill=col)

    # trees and rocks
    tree_custom = g.tree_like()
    for f in glob.glob(os.path.join(g.MAP, "Doodads", "*.json")):
        for d in g.load(f):
            code = g.le(d["TypeId"])
            x, y = to_px(d["Position"]["X"], d["Position"]["Y"], h)
            if code == "LTrc":
                draw.rectangle([x - 5, y - 5, x + 5, y + 5], fill=ROCK, outline=(0, 0, 0))
            elif (len(code) == 4 and code[1] == "T") or code in tree_custom:
                draw.point((x, y), fill=TREE)
                draw.point((x + 1, y), fill=TREE)

    # edges
    T, X, Y = data["NodeType"], data["NodeX"], data["NodeY"]
    pos = [to_px(X[i], Y[i], h) for i in range(len(T))]
    for a, b, rock in zip(data["EdgeA"], data["EdgeB"], data["EdgeRocks"]):
        if rock >= 0:
            dashed(draw, pos[a], pos[b], ROCK_EDGE, 4)
        else:
            draw.line([pos[a], pos[b]], fill=EDGE + (210,), width=2)

    # hand-written links the generator cannot walk (portals, ships)
    idx = {t: i for i, t in enumerate(T) if t}
    gen = {(min(T[a], T[b]), max(T[a], T[b])) for a, b in zip(data["EdgeA"], data["EdgeB"]) if T[a] and T[b]}
    for a, b in sorted(g.hand_edges() - gen):
        if a in idx and b in idx:
            i, j = idx[a], idx[b]
            if math.hypot(X[i] - X[j], Y[i] - Y[j]) > 4500:  # long ones are the portal / ship links
                dashed(draw, pos[i], pos[j], PORTAL + (220,), 3, 18)

    # nodes
    info = cp_info()
    small, label = font(15), font(17, True)
    for i, t in enumerate(T):
        x, y = pos[i]
        if t == 0:
            draw.ellipse([x - 7, y - 7, x + 7, y + 7], outline=(255, 255, 255), width=3)
            draw.text((x + 9, y - 8), str(i), fill=(255, 255, 255), font=small, stroke_width=2,
                      stroke_fill=(0, 0, 0))
            continue
        code = struct.pack(">I", t).decode("latin1")
        name, gold = info.get(code, (code, 10))
        r = 7 + min(gold, 100) ** 0.5 * 1.3
        draw.ellipse([x - r, y - r, x + r, y + r], fill=CP, outline=(0, 0, 0), width=2)
        draw.text((x + r + 3, y - 10), f"{name} ({gold})", fill=(0, 0, 0), font=label, stroke_width=3,
                  stroke_fill=(255, 255, 255))

    # legend
    lg = [("walkable land", LAND), ("walkable, no building", LAND_NOBUILD), ("cliff / mountain / blocked", BLOCKED),
          ("shallow water (walkable)", SHALLOW), ("deep water", DEEP), ("map edge", EDGE_OF_MAP), ("trees", TREE),
          ("WL rocks (LTrc, removable)", ROCK), ("CP (size = gold)", CP), ("walking edge", EDGE),
          ("edge closed while rocks stand", ROCK_EDGE), ("waygate / ship link (runtime)", PORTAL)]
    bx, by = 20, 20
    big = font(26, True)
    draw.rectangle([bx, by, bx + 560, by + 60 + 40 * len(lg)], fill=(255, 255, 255, 230), outline=(0, 0, 0), width=2)
    draw.text((bx + 15, by + 12), "Bot route graph (generated from map data)", fill=(0, 0, 0), font=big)
    for k, (text, col) in enumerate(lg):
        yy = by + 60 + 40 * k
        draw.rectangle([bx + 15, yy, bx + 55, yy + 28], fill=col, outline=(0, 0, 0))
        draw.text((bx + 70, yy), text, fill=(0, 0, 0), font=font(22))

    os.makedirs(OUT_DIR, exist_ok=True)
    full = os.path.join(OUT_DIR, "route_map.png")
    img.save(full, optimize=True)
    img.resize((img.width // 2, img.height // 2), Image.LANCZOS).save(os.path.join(OUT_DIR, "route_map_small.png"),
                                                                      optimize=True)
    print(f"wrote {full} ({img.width}x{img.height}) and route_map_small.png")


if __name__ == "__main__":
    main()
