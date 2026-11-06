"""The Kezan Enterprise's hand-made objects (0.58) -- what the 24p goblins did not have.

Usage (repo root):  python3 tools/shattered-legacies/content/kezan_objects.py   (before or after import_24p.py)
Writes:
  RZko Oil Refining (3 levels, researched at the Kezan Oil Supply -- import_24p.py's KEZAN set adds it to the Oil
       Supply's research list). No data effect: KezanEnterpriseOil raises the gold per Oil Supply / Oil Platform by
       KezanEnterpriseConfig.RefiningPerSupply / RefiningPerPlatform per level.
  0.60, the Refugee Slave Camps (maintainer's design; code: KezanEnterpriseSlaveCamps):
  oZq0-5 one camp per goblin town (CAMPS): stock Mercenary Camp model, trains the refugees of its region
  ?Zr*   one refugee unit per WL faction (REFUGEES): a copy of a basic unit of that faction, ureq = its condition
  RZr*   "No refugees from <faction> yet" -- condition researches (granted to everyone when the faction falls:
         defeated, or its fallback); until then the unit shows greyed out with that line
  + src/WarcraftLegacies.Source/.../KezanEnterprise/KezanEnterpriseSlaveCamps.g.cs (ids, towns, names)
  + src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/KezanSlaveCampsObjectInfo.cs (limits)
Keep the numbers in the tooltips in step with KezanEnterpriseConfig. Ids are fixed by the order of the tables:
append new rows at the end, never reorder (a row's index is its id).
"""
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
BTN = "ReplaceableTextures\\CommandButtons\\"

SUPPLY, PLATFORM = 50, 25            # KezanEnterpriseConfig.GoldPerOilSupply / GoldPerOilPlatform
PER_SUPPLY, PER_PLATFORM = 10, 5     # KezanEnterpriseConfig.RefiningPerSupply / RefiningPerPlatform
NAMES = ["Oil Refining", "Improved Oil Refining", "Advanced Oil Refining"]


def s(fid, value, level=None):
    m = {} if level is None else {"Level": level}
    m.update({"Id": fid, "Type": 3, "Value": value})
    return m


def i(fid, value):
    return {"Id": fid, "Value": value}


def main():
    mods = [i("gglb", 150), i("gglm", 100), i("glmb", 0), i("glmm", 0), i("glvl", 3), i("gtim", 45), i("gtib", 15),
            i("gbpx", 0), i("gbpy", 1), s("gef1", ""), s("gcls", ""), s("grac", "orc")]
    for lvl in range(1, 4):
        sup, pla = SUPPLY + PER_SUPPLY * lvl, PLATFORM + PER_PLATFORM * lvl
        mods += [s("gnam", NAMES[lvl - 1], lvl), s("gtp1", "Research " + NAMES[lvl - 1], lvl),
                 s("gnsf", "(Kezan Enterprise)", lvl), s("ghk1", "R", lvl), s("gar1", BTN + "BTNOilStation.blp", lvl),
                 s("gub1", f"Refines more oil: every Kezan Oil Supply gives {sup} gold per minute, every Oil Platform "
                           f"{pla} (was {sup - PER_SUPPLY} / {pla - PER_PLATFORM}).", lvl)]
    data = {"OldId": "Rhar", "NewId": "RZko", "Unk": [0], "Modifications": mods}
    path = os.path.join(MAP, "UpgradeData", "RZko.json")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))
    print("wrote", os.path.relpath(path, ROOT))


# ---------------------------------------------------------------------------------------------------------------- camps
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"
SRC = os.path.join(ROOT, "src")
MERC_MODEL = "buildings\\other\\Mercenary\\Mercenary.mdl"   # stock (WL's Stonemaul Keep uses it)
MERC_ICON = BTN + "BTNMercenaryCamp.blp"

# the goblin towns (the Goblin Empire quest's coast, 0.58; maintainer 0.60: the camps replace that quest): name, CP
# constant (Units.g.cs), CP position (preplaced) -- the camp is placed next to the CP (code: AiApi.FindBuildSpot)
CAMPS = [
    ("Booty Bay", "UNIT_N00L_BOOTY_BAY", 10368, -22976),
    ("Ratchet", "UNIT_N0A6_RATCHET", -10624, -3968),
    ("Gadgetzan", "UNIT_N05C_GADGETZAN", -8832, -13184),
    ("Venture Bay", "UNIT_N07Y_VENTURE_BAY", 2688, 16448),
    ("Fuselight", "UNIT_N01E_FUSELIGHT", 19200, -9984),
    ("Everlook", "UNIT_N01X_WINTERSPRING", -9856, 7296),
]
# one refugee unit per WL faction: faction name (Faction.Name), its unit (a basic one), the camp (index into CAMPS).
# Region = the town nearest the faction's home on this map, evened out (computed 0.60 from the preplaced units).
REFUGEES = [
    ("Stormwind", "h00A", 0), ("Kul'tiras", "h06J", 0), ("Fel Horde", "nchg", 0), ("Illidan", "nmyr", 0),
    ("Sunfury", "h0CF", 0), ("The Exodar", "h09T", 0),
    ("Black Empire", "o04V", 1), ("Frostwolf", "ogru", 1), ("Warsong", "otbk", 1),
    ("Skywall", "n0CG", 2), ("Ahn'qiraj", "o00L", 2),
    ("Lordaeron", "h01C", 3), ("Scourge", "h02G", 3), ("Dalaran", "h032", 3), ("Gilneas", "h04E", 3),
    ("Scarlet Crusade", "h09P", 3),
    ("Ironforge", "hrif", 4), ("Quel'thalas", "nhea", 4), ("Legion", "n04H", 4),
    ("Druids", "edry", 5), ("Sentinels", "earc", 5),
]
REFUGEE_LIMIT = 12
DROP = {"ureq", "utra", "ubui", "ures", "uupt", "ubpx", "ubpy", "uhot", "unsf", "utip", "utub", "ucat"}


def fourcc(code):
    return int.from_bytes(code.encode("latin1"), "big", signed=True)


def unit_json(code):
    path = os.path.join(MAP, "UnitData", code + ".json")
    if os.path.exists(path):
        with open(path, encoding="utf-8-sig") as f:
            return json.load(f)
    return {"OldId": code, "Unk": [0], "Modifications": []}


def name_of(d, fallback):
    for m in d["Modifications"]:
        if m["Id"] == "unam":
            return m["Value"]
    return fallback


def categories(code):
    """the unit's first WL category (Fighter / Marksman) from WL's object limits, for the bot"""
    consts = {}
    for line in open(os.path.join(SRC, "WarcraftLegacies.Shared", "Units.g.cs"), encoding="utf-8-sig"):
        m = re.search(r"const int (\w+) = (-?\d+);", line)
        if m:
            consts[m.group(1)] = int(m.group(2))
    want = fourcc(code)
    names = [n for n, v in consts.items() if v == want]
    for path in sorted(os.listdir(os.path.join(SRC, "WarcraftLegacies.Shared", "FactionObjectLimits"))):
        for line in open(os.path.join(SRC, "WarcraftLegacies.Shared", "FactionObjectLimits", path), encoding="utf-8-sig"):
            for n in names:
                if "(" + n + "," in line.replace("new(", "("):
                    for cat in ("Fighter", "Marksman", "Tank", "Support", "Siege"):
                        if "UnitCategory." + cat in line or ", " + cat + ")" in line:
                            return cat
    return "Fighter"


def write(kind, code, data):
    with open(os.path.join(MAP, kind, code + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def camps():
    units, info = [], []
    for i, (fac, src, camp) in enumerate(REFUGEES):
        code = src[0] + "Zr" + SEQ[i]
        res = "RZr" + SEQ[i]
        d = unit_json(src)
        base = d.get("OldId") or src
        mods = [m for m in d["Modifications"] if m["Id"] not in DROP]
        town = CAMPS[camp][0]
        uname = name_of(d, src)
        slot = sum(1 for _, _, c in REFUGEES[:i] if c == camp)   # its button: 3 columns, x = 3 left for the rally
        mods += [{"Id": "ureq", "Type": 3, "Value": res}, {"Id": "ubpx", "Value": slot % 3},
                 {"Id": "ubpy", "Value": slot // 3}, {"Id": "unsf", "Type": 3, "Value": "(Refugee, " + fac + ")"},
                 {"Id": "utub", "Type": 3,
                  "Value": "|cffffcc00Refugees from " + fac + "|r, sold at the Refugee Slave Camp of " + town
                           + ". Only once " + fac + " has fallen (defeated, or its fallback); the camp's stock refills "
                           "over time. Every sale pays the Kezan Enterprise."}]
        write("UnitData", code, {"OldId": base, "NewId": code, "Unk": [0], "Modifications": mods})
        write("UpgradeData", res, {"OldId": "Roch", "NewId": res, "Unk": [0], "Modifications": [
            {"Level": 1, "Id": "gnam", "Type": 3, "Value": "No refugees from " + fac + " yet"},
            {"Level": 1, "Id": "gnsf", "Type": 3, "Value": "(Condition)"},
            {"Id": "glvl", "Value": 1}, {"Id": "gglb", "Value": 0}, {"Id": "glmb", "Value": 0}]})
        units.append((fac, code, res, camp, uname))
        info.append((code, categories(src), uname + " (refugee)"))
    camp_codes = []
    for k, (town, _, _, _) in enumerate(CAMPS):
        code = "oZq" + SEQ[k]
        trains = ",".join(c for _, c, _, cp, _ in units if cp == k)
        write("UnitData", code, {"OldId": "obar", "NewId": code, "Unk": [0], "Modifications": [
            {"Id": "unam", "Type": 3, "Value": "Refugee Slave Camp"},
            {"Id": "unsf", "Type": 3, "Value": "(" + town + ")"},
            {"Id": "umdl", "Type": 3, "Value": MERC_MODEL}, {"Id": "uico", "Type": 3, "Value": MERC_ICON},
            {"Id": "utra", "Type": 3, "Value": trains}, {"Id": "ures", "Type": 3, "Value": ""},
            {"Id": "uupt", "Type": 3, "Value": ""}, {"Id": "ureq", "Type": 3, "Value": ""},
            {"Id": "ugol", "Value": 0}, {"Id": "ulum", "Value": 0}, {"Id": "ufma", "Value": 0},
            {"Id": "urac", "Type": 3, "Value": "other"},
            {"Id": "utub", "Type": 3, "Value": "Survivors of fallen kingdoms, bound by Kezan contracts. Whoever "
             "rules " + town + " buys them here; every sale pays the Kezan Enterprise."}]})
        camp_codes.append(code)
    write_cs(units, camp_codes)
    write_object_info(info, camp_codes)
    print("wrote", len(camp_codes), "camps,", len(units), "refugee units and condition researches")


def write_cs(units, camp_codes):
    lines = ["// <auto-generated> tools/shattered-legacies/content/kezan_objects.py -- the Refugee Slave Camps (0.60).",
             "// Do not edit. </auto-generated>",
             "namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise", "{",
             "  internal static class KezanEnterpriseSlaveCampsData", "  {",
             "    // per camp: its building, the town's CP, the CP's position (the camp goes next to it)"]
    lines.append("    internal static int[] Camps = { " + ", ".join(str(fourcc(c)) for c in camp_codes) + " };")
    lines.append("    internal static int[] CampCps = { " + ", ".join(cp for _, cp, _, _ in CAMPS) + " };")
    lines.append("    internal static float[] CampXY = { " + ", ".join(f"{x}f, {y}f" for _, _, x, y in CAMPS)
                 + " };")
    lines.append("    internal static string[] CampTowns = { " + ", ".join('"' + t + '"' for t, _, _, _ in CAMPS)
                 + " };")
    lines.append("")
    lines.append("    // per refugee unit: the faction it comes from (Faction.Name), the unit, its condition research, its camp")
    lines.append("    internal static string[] Factions = { " + ", ".join('"' + f + '"' for f, _, _, _, _ in units) + " };")
    lines.append("    internal static int[] Units = { " + ", ".join(str(fourcc(c)) for _, c, _, _, _ in units) + " };")
    lines.append("    internal static int[] Conditions = { " + ", ".join(str(fourcc(r)) for _, _, r, _, _ in units)
                 + " };")
    lines.append("    internal static int[] UnitCamp = { " + ", ".join(str(k) for _, _, _, k, _ in units) + " };")
    lines += ["  }", "}", ""]
    out = os.path.join(SRC, "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Factions", "KezanEnterprise",
                       "KezanEnterpriseSlaveCamps.g.cs")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(wrap(l) for l in lines))


def wrap(line, width=120):
    if len(line) <= width or "{" not in line:
        return line
    head, body = line.split("{", 1)
    body = body.rstrip().rstrip(";").rstrip().rstrip("}")
    items = [x.strip() for x in body.split(",") if x.strip()]
    out, cur = [head + "{"], "      "
    for it in items:
        if len(cur) + len(it) + 2 > width:
            out.append(cur.rstrip())
            cur = "      "
        cur += it + ", "
    out.append(cur.rstrip())
    out.append("    };")
    return "\n".join(out)


def write_object_info(info, camp_codes):
    lines = ["// <auto-generated> tools/shattered-legacies/content/kezan_objects.py -- object limits of the Refugee",
             "// Slave Camps' units (0.60). Do not edit. </auto-generated>",
             "using MacroTools.Shared;", "using static MacroTools.Shared.UnitCategory;", "",
             "namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits", "{",
             "  public static class KezanSlaveCampsObjectInfo", "  {",
             "    public static IEnumerable<ObjectInfo> GetAllObjectLimits()", "    {"]
    for code in camp_codes:
        lines.append(f"      yield return new({fourcc(code)}, 1); // {code} Refugee Slave Camp")
    for code, cat, name in info:
        lines.append(f"      yield return new({fourcc(code)}, {REFUGEE_LIMIT}, {cat}); // {code} {name}")
    lines += ["    }", "  }", "}", ""]
    out = os.path.join(SRC, "WarcraftLegacies.Shared", "ShatteredLegacies", "FactionObjectLimits",
                       "KezanSlaveCampsObjectInfo.cs")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    main()
    camps()
