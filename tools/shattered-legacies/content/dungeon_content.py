"""0.87.0 (maintainer): what stands in the void dungeons besides their CPs (Timelines/VOID_DUNGEONS.md).

  python3 tools/shattered-legacies/content/dungeon_content.py          writes the file below
  python3 tools/shattered-legacies/content/dungeon_content.py --check  reports only

- CREEPS: a fallback starts with its dungeon's seat only; every other CP of a dungeon is guarded by a creep camp
  (Neutral Hostile): Nazjatar's three by mur'gul, hydras and sea turtles, Shadowforge's two by Ragnaros' loyalists
  (Core Hounds, Fire Wyrms, a Shadowforge Titan), the Deadmines' two by kobolds and goblin shredders. Majordomo
  Executus stands by the Siege Foundry. Two Core Hounds guard the west throne.
- SHADOWFORGE (Dark Iron): the Throne of Thaurissan (their HQ) replaces the throne doodad by the Throne Room, the
  Statue of Thaurissan replaces the big dwarf statue by the Siege Foundry, the Shadowforge Black Market stands by the
  Treasury -- Neutral Passive (the code makes them invulnerable and hands the throne to the Dark Iron).
Camp spots: rings around the CP (300 .. 650), open ground (map_query), 110 apart.
Writes
  mapdata/WarcraftLegacies/Units/Dungeon_Content.json
  mapdata/WarcraftLegacies/Doodads/Dungeon_Shadowforge.json    the two doodads the units replace are removed
Run after void_dungeons.py and dark_iron.py / import_24p.py (the unit types); then gen_routes.py.
"""
import json
import math
import os
import struct
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
OUT = os.path.join(MAP, "Units", "Dungeon_Content.json")
SHADOWFORGE_DOODADS = os.path.join(MAP, "Doodads", "Dungeon_Shadowforge.json")
IDS = os.path.join(ROOT, "tools", "shattered-legacies", "content", "import_ids.json")
HOSTILE, PASSIVE = 24, 27

# guarded CP (its spot) -> camp: (unit type, count)
CAMPS = [
    ("Heart of Azeroth", -19008, -27904, [("nmtw", 2), ("nmsn", 2), ("nhyd", 2), ("ntrt", 1)]),
    ("Lightless Cavern", -16192, -28352, [("nmrv", 3), ("nmsn", 1), ("nlsn", 2)]),
    ("Gishan Cavern", -20288, -26368, [("nmrv", 2), ("nmbg", 2), ("nhyh", 2)]),
    ("Dark Iron Treasury", -22144, 22784, [("n0CG", 3), ("n0CF", 1)]),
    ("Siege Foundry", -23040, 23936, [("n0CG", 2), ("n0CF", 1), ("n0CC", 1)]),
    ("Goblin Foundry", -27952, -28256, [("ngir", 2), ("nkol", 1), ("nkob", 3)]),
    ("Ironclad Cove", -25232, -30176, [("ngir", 1), ("nkot", 2), ("nkog", 2), ("nkol", 1), ("nkob", 2)]),
]
# doodads replaced by units: (type, x, y) within 64
REPLACE = [("BOth", -18635, 24065), ("D006", -22381, 23577)]


def le(code):
    return struct.unpack("<I", code.encode("latin1"))[0]


def placed(code, x, y, owner, number, facing=4.712389, level=1, scale=1.0):
    return {
        "TypeId": le(code), "Variation": 0, "Position": {"X": x, "Y": y, "Z": 0}, "Rotation": facing,
        "Scale": {"X": scale, "Y": scale, "Z": scale}, "SkinId": le(code), "Flags": 2, "OwnerId": owner, "Unk1": 0,
        "Unk2": 0, "HP": -1, "MP": -1, "MapItemTableId": -1, "ItemTableSets": [], "GoldAmount": 0,
        "TargetAcquisition": -1, "HeroLevel": level, "HeroStrength": 0, "HeroAgility": 0, "HeroIntelligence": 0,
        "InventoryData": [], "AbilityData": [], "RandomDataMode": -1, "CustomPlayerColorId": -1,
        "WaygateDestinationRegionId": -1, "CreationNumber": number,
    }


def main(check):
    sys.path.insert(0, os.path.join(ROOT, "tools", "shattered-legacies", "ai"))
    import map_query
    import glob
    forces = {k.split(":", 1)[1]: v for k, v in json.load(open(IDS)).items() if k.startswith("DarkIronForces:")}
    others = []
    for f in glob.glob(os.path.join(MAP, "Units", "*.json")):
        if os.path.abspath(f) != os.path.abspath(OUT):
            others.extend(json.load(open(f, encoding="utf-8")))
    number = max(u["CreationNumber"] for u in others) + 1
    taken = [(u["Position"]["X"], u["Position"]["Y"]) for u in others]
    problems, units = [], []

    def free(x, y, gap=110):
        return all(math.hypot(x - a, y - b) >= gap for a, b in taken)

    def spot(cx, cy, rmin, rmax, half=48):
        for r in range(rmin, rmax + 1, 50):
            for k in range(24):
                a = 2 * math.pi * k / 24 + r * 0.01
                x, y = round(cx + r * math.cos(a)), round(cy + r * math.sin(a))
                if map_query.area_walkable(x, y, half) and free(x, y):
                    return x, y
        return None

    def add(code, x, y, owner, **kw):
        nonlocal number
        units.append(placed(code, x, y, owner, number, **kw))
        taken.append((x, y))
        number += 1

    for name, cx, cy, camp in CAMPS:
        for code, count in camp:
            for _ in range(count):
                p = spot(cx, cy, 300, 650)
                if p is None:
                    problems.append("%s: no room for %s" % (name, code))
                    continue
                add(code, p[0], p[1], HOSTILE, facing=math.atan2(cy - p[1], cx - p[0]))

    # Majordomo by the Siege Foundry; the hounds guard the west throne.
    tx, ty = -24313, 21273
    add(forces["U00W"], -23468, 24062, HOSTILE, level=8)
    for _ in range(2):
        q = spot(tx, ty, 250, 550)
        if q:
            add("n0CG", q[0], q[1], HOSTILE)


    # the Dark Iron props: the throne and the statue on their doodads' spots, the market by the Treasury
    doodads = json.load(open(SHADOWFORGE_DOODADS, encoding="utf-8"))
    rest = []
    found = {}
    for o in doodads:
        hit = next((r for r in REPLACE if o["TypeId"] == le(r[0]) and
                    math.hypot(o["Position"]["X"] - r[1], o["Position"]["Y"] - r[2]) < 64), None)
        if hit:
            found[hit[0]] = o
        else:
            rest.append(o)
    earlier = json.load(open(OUT, encoding="utf-8")) if os.path.exists(OUT) else []
    for code, unit in (("BOth", "hZiy"), ("D006", "hZix")):
        if code not in found:  # replaced by an earlier run: the unit keeps its spot and facing
            old = next((u for u in earlier if u["TypeId"] == le(unit)), None)
            if old:
                found[code] = old
    throne = found.get("BOth")
    statue = found.get("D006")
    if throne:
        add("hZiy", round(throne["Position"]["X"]), round(throne["Position"]["Y"]), PASSIVE,
            facing=throne.get("Rotation", 4.712389))
    if statue:
        add("hZix", -22380.629, 23577.186, PASSIVE,
            facing=statue.get("Rotation", 4.712389))
    m = spot(-22144, 22784, 400, 700, 64)
    if m:
        add("nZiy", m[0], m[1], PASSIVE)
    else:
        problems.append("no room for the Black Market by the Treasury")

    # Port garrison, separate from the Dragonmaw buildings reserved for the fallback.
    for unit_type in ("oZmc", "oZmc", "oZmc", "oZmd", "oZmd", "nZm0"):
        p = spot(21312, -3200, 240, 640)
        if p:
            add(unit_type, *p, HOSTILE)
        else:
            problems.append("no room for Dragonmaw Port guard " + unit_type)

    print("dungeon content: %d units (%d creeps), %d doodads replaced" %
          (len(units), sum(1 for u in units if u["OwnerId"] == HOSTILE), len(doodads) - len(rest)))
    for p in problems:
        print("  PROBLEM: " + p)
    if check or problems:
        return 1 if problems else 0
    with open(OUT, "w", encoding="utf-8") as f:
        f.write(json.dumps(units, indent=2, ensure_ascii=False))
    if len(rest) != len(doodads):
        with open(SHADOWFORGE_DOODADS, "w", encoding="utf-8") as f:
            f.write(json.dumps(rest, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    sys.exit(main("--check" in sys.argv))
