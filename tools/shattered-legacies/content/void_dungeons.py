"""Control points of the void-area dungeons (0.83.0, Timelines/VOID_DUNGEONS.md).

  python3 tools/shattered-legacies/content/void_dungeons.py          writes the files below
  python3 tools/shattered-legacies/content/void_dungeons.py --check  reports only

Writes
  mapdata/WarcraftLegacies/UnitData/nZv*.json          one CP type per CP (WL's CP: stock ncop, the name is the type's
                                                       name, the income comes from its marker ability A0LO .. A0NT)
  mapdata/WarcraftLegacies/UnitData.zhCN/nZv*.json     the Chinese name
  mapdata/WarcraftLegacies/UnitData/nZvb..nZvd.json    0.84: the dungeons' waygates (circle, gold mine, Undercity gate)
  mapdata/WarcraftLegacies/Units/Dungeon_ControlPoints.json   the CPs, preplaced, Neutral Hostile (WL's
                                                       ControlPointSetup registers every preplaced CP; the bot's
                                                       gen_routes.py makes them route nodes)
  src/WarcraftLegacies.Source/ShatteredLegacies/Dungeons/DungeonObjects.g.cs   the type ids for the code
Rerun gen_routes.py after a change (the CPs are route nodes).
"""
import json
import os
import struct
import sys
import glob

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
OUT_CS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Dungeons", "DungeonObjects.g.cs")
UNITS = os.path.join(MAP, "Units", "Dungeon_ControlPoints.json")

MARKER = {10: "A0LO", 15: "A0MW", 20: "A0NR", 25: "A0NU", 30: "A0NS", 50: "A0NT"}  # income -> marker ability

# 0.84: the dungeons' waygates (stock Waygate with another model; the code places them, Core/Effects/Instance.cs):
# id, constant, name, Chinese name, model, model scale
GATES = [
    ("nZvb", "GatewayCircle", "Gateway", "传送门", "war3mapImported\\Effect_Gateway_White.mdl", 2.0),
    ("nZvc", "DeadminesEntrance", "The Deadmines", "死亡矿井", "buildings\\other\\GoldMine\\GoldMine.mdl", 1.0),
    ("nZvd", "UndercityGate", "Gate of the Undercity", "幽暗城大门", "Doodads\\LordaeronSummer\\Terrain\\IronGate\\IronGate.mdl",
     0.9),
    # 0.87: the Exodar itself -- the ship is the waygate into its own interior (Factions/Draenei/ExodarShip.cs)
    ("nZvf", "ExodarShip", "The Exodar", "埃索达", "war3mapImported\\Unit_Exodar.mdl", 0.32),
]

# id, constant, name, Chinese name, income, x, y (the 24p map's CP spots where it had one, moved with its dungeon)
CPS = [
    ("nZv0", "Undercity", "Undercity", "幽暗城", 20, -27139, 22825),            # maintainer's spot
    ("nZv1", "EternalPalace", "Eternal Palace", "永恒王宫", 25, -22208, -30016),  # 24p n05Q "Nazjatar"
    ("nZv2", "HeartOfAzeroth", "Heart of Azeroth", "艾泽拉斯之心", 30, -19008, -27904),  # 24p n0A0
    ("nZv3", "LightlessCavern", "Lightless Cavern", "无光洞穴", 15, -16192, -28352),  # 24p n05W
    ("nZv4", "GishanCavern", "Gishan Cavern", "吉山洞穴", 10, -20288, -26368),     # 24p n04E
    # 0.84 (maintainer): the dungeon IS Shadowforge City (its surface building is the entrance) -- its districts
    # 0.86 (maintainer): the Dark Iron's Mercenary Charter -- their Treasury and Siege Foundry (the seat)
    ("nZv5", "DarkIronTreasury", "Dark Iron Treasury", "黑铁宝库", 20, -22144, 22784),  # 24p n0AA "Blackrock Depths"
    ("nZv6", "SiegeFoundry", "Siege Foundry", "攻城铸造厂", 15, -23040, 23936),      # 24p n00P "Molten Core"
    # 0.87 (maintainer): the throne by it is the Dark Iron HQ -- the Throne Room is their seat (was Tavern District)
    ("nZv7", "ThroneRoom", "Throne Room", "王座厅", 10, -18944, 23360),             # 24p n00O "Blackrock Spire"
    ("nZv8", "Deadmines", "The Deadmines", "死亡矿井", 20, -26048, -26880),       # 24p n078 "Temple of Ahn'Qiraj"
    ("nZv9", "GoblinFoundry", "Goblin Foundry", "地精铸造厂", 10, -27952, -28256),
    ("nZva", "IroncladCove", "Ironclad Cove", "铁甲湾", 15, -25232, -30176),
    # 0.87 (maintainer): the Furbolgs' den behind the Totem Lodge (a Felwood glade copied into the void)
    ("nZve", "TimbermawHold", "Timbermaw Hold", "木喉要塞", 10, -14892, 22008),
]


# 0.87: the Exodar's helm -- "Set Course" on WL's Dimensional Generator (n00E, inside the ship): a channel aimed at any
# point; the ship flies to the landing spot nearest to it (ExodarShip.cs). Needs "The Dimensional Ship" (R09A, like the
# generator's portals); 5 minutes cooldown.
SET_COURSE = "AZv0"
GENERATOR = "n00E"


def set_course():
    mods = [
        {"Id": "anam", "Type": 3, "Value": "Set Course"},
        {"Id": "ansf", "Type": 3, "Value": "(Exodar)"},
        {"Id": "aart", "Type": 3, "Value": "ReplaceableTextures\\CommandButtons\\BTNStaffOfTeleportation.blp"},
        {"Id": "ahky", "Type": 3, "Value": "S"},
        {"Id": "aher", "Value": 0}, {"Id": "alev", "Value": 1}, {"Id": "areq", "Type": 3, "Value": "R09A"},
        {"Id": "arac", "Type": 3, "Value": "orc"},
        {"Id": "abpx", "Value": 0}, {"Id": "abpy", "Value": 2},
        {"Id": "aran", "Level": 1, "Type": 2, "Value": 99999},
        {"Id": "acdn", "Level": 1, "Type": 2, "Value": 300},
        {"Id": "Ncl1", "Level": 1, "Type": 2, "Value": 0},
        {"Id": "Ncl2", "Level": 1, "Value": 2},
        {"Id": "Ncl3", "Level": 1, "Value": 1},
        {"Id": "Ncl5", "Level": 1, "Value": 0},
        {"Id": "Ncl6", "Level": 1, "Type": 3, "Value": "blizzard"},
        {"Id": "atp1", "Level": 1, "Type": 3, "Value": "Set Course"},
        {"Id": "aub1", "Level": 1, "Type": 3,
         "Value": "The Exodar flies to the landing spot nearest the target point. 90 seconds in flight -- nobody "
                  "gets in or out meanwhile. Only while the Exodar's bridge is held by the draenei or their master."},
    ]
    return {"OldId": "ANcl", "NewId": SET_COURSE, "Unk": [0], "Modifications": mods}


def add_to_generator():
    """WL's Dimensional Generator gets Set Course (a [SL] change of WL's object, idempotent)"""
    path = os.path.join(MAP, "UnitData", GENERATOR + ".json")
    d = json.load(open(path, encoding="utf-8-sig"))
    for m in d["Modifications"]:
        if m["Id"] == "uabi":
            parts = [t for t in str(m["Value"]).split(",") if t]
            if SET_COURSE not in parts:
                m["Value"] = ",".join(parts + [SET_COURSE])
    with open(path, "w", encoding="utf-8") as f:
        f.write(json.dumps(d, indent=2, ensure_ascii=False))


def save(path, data):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def le(code):
    return struct.unpack("<I", code.encode("latin1"))[0]


def be(code):
    return struct.unpack(">I", code.encode("latin1"))[0]


def unit_type(code, name, income):
    # WL's neutral CP (e.g. n024 "Blackrock Depths"), field for field
    return {
        "OldId": "ncop", "NewId": code, "Unk": [0],
        "Modifications": [
            {"Id": "uabi", "Type": 3, "Value": MARKER[income]},
            {"Id": "ucol", "Type": 2, "Value": 0},
            {"Id": "uhos", "Value": 1},
            {"Id": "uhpm", "Value": 5000},
            {"Id": "uhrt", "Type": 3, "Value": "always"},
            {"Id": "utyp", "Type": 3, "Value": "ancient,sapper"},
            {"Id": "upat", "Type": 3, "Value": "PathTextures\\4x4SimpleSolid.tga"},
            {"Id": "ua1b", "Value": -1},
            {"Id": "ua1d", "Value": 1},
            {"Id": "ua1s", "Value": 1},
            {"Id": "uaen", "Value": 1},
            {"Id": "ua1r", "Value": 900},
            {"Id": "uacq", "Type": 2, "Value": 900},
            {"Id": "ua1g", "Type": 3, "Value": "bridge"},
            {"Id": "urac", "Type": 3, "Value": "creeps"},
            {"Id": "unam", "Type": 3, "Value": name},
            {"Id": "ussc", "Type": 1, "Value": 1.5},
        ],
    }


def gate_type(code, name, model, scale):
    return {
        "OldId": "nwgt", "NewId": code, "Unk": [0],
        "Modifications": [
            {"Id": "umdl", "Type": 3, "Value": model},
            {"Id": "unam", "Type": 3, "Value": name},
            {"Id": "usca", "Type": 1, "Value": scale},
        ],
    }


def zh(code, name):
    return {"OldId": "ncop", "Unk": [0], "Modifications": [{"Id": "unam", "Type": 3, "Value": name}], "NewId": code}


def placed(code, x, y, number):
    return {
        "TypeId": le(code), "Variation": 0, "Position": {"X": x, "Y": y, "Z": 0}, "Rotation": 4.712389,
        "Scale": {"X": 1, "Y": 1, "Z": 1}, "SkinId": le(code), "Flags": 2, "OwnerId": 24, "Unk1": 0, "Unk2": 0,
        "HP": -1, "MP": -1, "MapItemTableId": -1, "ItemTableSets": [], "GoldAmount": 0, "TargetAcquisition": -1,
        "HeroLevel": 1, "HeroStrength": 0, "HeroAgility": 0, "HeroIntelligence": 0, "InventoryData": [],
        "AbilityData": [], "RandomDataMode": -1, "CustomPlayerColorId": -1, "WaygateDestinationRegionId": -1,
        "CreationNumber": number,
    }


def main(check):
    sys.path.insert(0, os.path.join(ROOT, "tools", "shattered-legacies", "ai"))
    import map_query
    problems = []
    for code, _, name, _, income, x, y in CPS:
        if not map_query.area_walkable(x, y, 96):
            problems.append("%s %s: no open ground at %d, %d" % (code, name, x, y))
        if income not in MARKER:
            problems.append("%s: no marker for income %d" % (code, income))
    ours = []
    for f in glob.glob(os.path.join(MAP, "Units", "*.json")):
        if os.path.abspath(f) != os.path.abspath(UNITS):
            ours.extend(json.load(open(f, encoding="utf-8")))
    taken = {u["TypeId"] for u in ours}
    for code, *_ in CPS:
        if le(code) in taken:
            problems.append("%s is preplaced elsewhere" % code)
    print("void-dungeon CPs: %d" % len(CPS))
    for p in problems:
        print("  PROBLEM: " + p)
    if problems or check:
        return 1 if problems else 0

    start = max(u["CreationNumber"] for u in ours) + 1
    units = []
    for i, (code, const, name, cn, income, x, y) in enumerate(CPS):
        save(os.path.join(MAP, "UnitData", code + ".json"), unit_type(code, name, income))
        save(os.path.join(MAP, "UnitData.zhCN", code + ".json"), zh(code, cn))
        units.append(placed(code, x, y, start + i))
    save(UNITS, units)
    for code, _, name, cn, model, scale in GATES:
        save(os.path.join(MAP, "UnitData", code + ".json"), gate_type(code, name, model, scale))
        zg = zh(code, cn)
        zg["OldId"] = "nwgt"
        save(os.path.join(MAP, "UnitData.zhCN", code + ".json"), zg)
    save(os.path.join(MAP, "AbilityData", SET_COURSE + ".json"), set_course())
    add_to_generator()
    lines = [
        "// <auto-generated>",
        "// Generated by tools/shattered-legacies/content/void_dungeons.py -- DO NOT EDIT, re-run the generator.",
        "// </auto-generated>",
        "namespace WarcraftLegacies.Source.ShatteredLegacies.Dungeons",
        "{",
        "  // the control points of the void-area dungeons (Timelines/VOID_DUNGEONS.md): preplaced, Neutral Hostile",
        "  internal static class DungeonObjects",
        "  {",
    ]
    for code, const, name, _, income, x, y in CPS:
        lines.append("    internal const int %s = %d; // %s \"%s\", %d gold, %d, %d" % (const, be(code), code, name, income,
                                                                                    x, y))
    lines.append("")
    lines.append("    // 0.87: the Exodar's Set Course (on the Dimensional Generator)")
    lines.append("    internal const int SetCourse = %d; // %s" % (be(SET_COURSE), SET_COURSE))
    lines.append("")
    lines.append("    // the waygates (stock Waygate, another model)")
    for code, const, name, _, model, scale in GATES:
        lines.append("    internal const int %s = %d; // %s \"%s\"" % (const, be(code), code, name))
    lines += ["  }", "}", ""]
    os.makedirs(os.path.dirname(OUT_CS), exist_ok=True)
    with open(OUT_CS, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print("  written: UnitData/nZv*.json (+ zhCN), Units/Dungeon_ControlPoints.json, Dungeons/DungeonObjects.g.cs")
    return 0


if __name__ == "__main__":
    sys.exit(main("--check" in sys.argv))
