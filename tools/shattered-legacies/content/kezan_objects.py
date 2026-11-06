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

GOLD = [100, 150, 200]               # KezanEnterpriseConfig.RefineGold: gold per 1000 oil at Oil Refining 0 / 1 / 2
OIL = 1000                           # KezanEnterpriseConfig.RefineOil
NAMES = ["Improved Oil Refining", "Advanced Oil Refining"]
TIER4 = "RZk7"                       # the Enterprise's Tier 4 (import_24p.py, KEZAN: 24p R04R)
def s(fid, value, level=None):
    m = {} if level is None else {"Level": level}
    m.update({"Id": fid, "Type": 3, "Value": value})
    return m


def i(fid, value):
    return {"Id": fid, "Value": value}


def v(fid, value, level=None):
    """a field with the type WL's JSON needs: text 3, real 2, integer none (check_json.py)"""
    m = {} if level is None else {"Level": level}
    m["Id"] = fid
    if isinstance(value, str):
        m["Type"] = 3
    elif isinstance(value, float):
        m["Type"] = 2
    m["Value"] = value
    return m


def main():
    # 0.61 (maintainer): Oil Refining improves the Oil Refinery's exchange rate; 2 levels, the second behind Tier 4
    # (0.59's base Rhar asked for a human Castle -- "Burg" -- at levels 2 / 3, which the Enterprise cannot build)
    mods = [i("gglb", 200), i("gglm", 100), i("glmb", 0), i("glmm", 0), i("glvl", 2), i("gtim", 45), i("gtib", 15),
            i("gbpx", 0), i("gbpy", 0), s("gef1", ""), s("gcls", ""), s("grac", "orc")]
    for lvl in range(1, 3):
        mods += [s("gnam", NAMES[lvl - 1], lvl), s("gtp1", "Research " + NAMES[lvl - 1], lvl),
                 s("gnsf", "(Kezan Enterprise)", lvl), s("ghk1", "R", lvl), s("gar1", BTN + "BTNOilStation.blp", lvl),
                 s("greq", "" if lvl == 1 else TIER4, lvl),
                 s("gub1", f"The Oil Refinery turns {OIL} oil into {GOLD[lvl]} gold every 60 seconds "
                           f"(was {GOLD[lvl - 1]}).", lvl)]
    data = {"OldId": "Rhar", "NewId": "RZko", "Unk": [0], "Modifications": mods}
    path = os.path.join(MAP, "UpgradeData", "RZko.json")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))
    print("wrote", os.path.relpath(path, ROOT))
    refinery()
    detonate()


def refinery():
    """0.61: the Oil Refinery (hZq0) -- the Oil Supply's model; its autocast "Refine Oil" (AZq1, a Moon Well's
    Replenish without effect: the refinery has no mana) only shows the toggle; KezanEnterpriseOil does the exchange."""
    write("AbilityData", "AZq1", {"OldId": "Ambt", "NewId": "AZq1", "Unk": [0], "Modifications": [
        s("anam", "Refine Oil"), s("atp1", "Refine Oil", 1), s("aub1", f"While on (autocast), every 60 seconds turns {OIL} oil of the Kezan Oil Supply into {GOLD[0]} gold "
                  f"({GOLD[1]} / {GOLD[2]} with Oil Refining).", 1), s("aart", BTN + "BTNOilStation.blp"),
        v("aran", 0.0, 1), i("abpx", 1), i("abpy", 0), s("ahky", "F")]})
    sup = unit_json("hZk1")
    mods = [m for m in sup["Modifications"] if m["Id"] not in ("unam", "utra", "ures", "uabi", "utip", "utub", "uhot",
                                                                 "ubpx", "ubpy", "umpm", "umpi", "umpr")]
    mods += [s("unam", "Kezan Oil Refinery"), s("utra", ""), s("ures", "RZko"), s("uabi", "AZq1,Avul"),
             s("utub", "Turns the Oil Supply's oil into gold (Refine Oil, autocast). Oil Refining improves the rate."),
             i("umpm", 0), i("umpi", 0)]
    write("UnitData", "hZq0", {"OldId": sup["OldId"], "NewId": "hZq0", "Unk": [0], "Modifications": mods})


def detonate():
    """0.61 (maintainer): the Fuel Transport as a suicide bomber -- Detonate (AZq0, Faerie Fire based: autocast toggle,
    manual cast on an enemy). KezanEnterpriseBombers: on cast, or while autocast is on and an enemy comes close,
    500 + its mana (oil) damage to everything within 1000 -- allies too."""
    write("AbilityData", "AZq0", {"OldId": "Afae", "NewId": "AZq0", "Unk": [0], "Modifications": [
        s("anam", "Detonate"), s("atp1", "Detonate", 1), s("aub1", "Blows the vehicle up: 500 damage plus its fuel (mana) to every unit and building within 1000 -- "
                  "allies too. Autocast: detonates when an enemy comes close.", 1), s("aart", BTN + "BTNGoblinSapper.blp"),
        v("amcs", 0, 1), v("aran", 300.0, 1), v("acdn", 1.0, 1), v("adur", 0.01, 1), v("ahdu", 0.01, 1),
        i("abpx", 3), i("abpy", 2), s("ahky", "D")]})


# ---------------------------------------------------------------------------------------------------------------- camps
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"
SRC = os.path.join(ROOT, "src")
MERC_MODEL = "buildings\\other\\Mercenary\\Mercenary.mdl"   # stock (WL's Stonemaul Keep uses it)
MERC_ICON = BTN + "BTNMercenaryCamp.blp"

# the goblin towns (the Goblin Empire quest's coast, 0.58; maintainer 0.60: the camps replace that quest): name, CP
# constant (Units.g.cs), CP position, camp position (0.61: the maintainer's spots; None = next to the CP, found in
# game by AiApi.FindBuildSpot)
CAMPS = [
    ("Booty Bay", "UNIT_N00L_BOOTY_BAY", 10368, -22976, (10852, -22728)),
    ("Ratchet", "UNIT_N0A6_RATCHET", -10624, -3968, None),
    ("Gadgetzan", "UNIT_N05C_GADGETZAN", -8832, -13184, (-8672, -12960)),
    ("Venture Bay", "UNIT_N07Y_VENTURE_BAY", 2688, 16448, (1081, 15949)),
    ("Fuselight", "UNIT_N01E_FUSELIGHT", 19200, -9984, (18210, -10567)),
    ("Everlook", "UNIT_N01X_WINTERSPRING", -9856, 7296, (-8960, 6912)),  # maintainer wrote y -6912: Winterspring is +
]
# one refugee unit per WL SLOT (0.61: a slot with a pick -- Sentinels / The Exodar, Frostwolf / Warsong, Dalaran /
# Gilneas -- is one entry, available when whichever faction plays it falls): faction names ("A|B"), its unit, the camp
# (index into CAMPS). None = a retired row (its id stays reserved, nothing is written). Never reorder rows.
REFUGEES = [
    ("Stormwind", "h00A", 0), ("Kul'tiras", "h06J", 0), ("Fel Horde", "nchg", 0), ("Illidan", "nmyr", 0),
    ("Sunfury", "h0CF", 0), None,                                   # 5: The Exodar -> row 20 (Sentinels / The Exodar)
    ("Black Empire", "o04V", 2), ("Frostwolf|Warsong", "ogru", 1), None,  # 6: to Gadgetzan (maintainer); 8: Warsong -> 7
    ("Skywall", "n0CG", 2), ("Ahn'qiraj", "o00L", 2),
    ("Lordaeron", "h01C", 3), ("Scourge", "h02G", 3), ("Dalaran|Gilneas", "h032", 3), None,  # 14: Gilneas -> 13
    ("Scarlet Crusade", "h09P", 3),
    ("Ironforge", "hrif", 4), ("Quel'thalas", "nhea", 4), ("Legion", "n04H", 4),
    ("Druids", "edry", 5), ("Sentinels|The Exodar", "earc", 5),
]
RETIRED_UNITS = {5: "h09T", 8: "otbk", 14: "h04E"}  # the source of a retired row (its old file is removed)
PRICE = 0.5  # 0.61 (maintainer): refugees cost half of the regular unit -- an incentive to buy at the camps
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
    for i, row in enumerate(REFUGEES):
        if row is None:
            for kind, code in (("UnitData", RETIRED_UNITS[i][0] + "Zr" + SEQ[i]), ("UpgradeData", "RZr" + SEQ[i])):
                gone = os.path.join(MAP, kind, code + ".json")
                if os.path.exists(gone):
                    os.remove(gone)
            continue
        fac, src, camp = row
        shown = " / ".join(fac.split("|"))
        code = src[0] + "Zr" + SEQ[i]
        res = "RZr" + SEQ[i]
        d = unit_json(src)
        base = d.get("OldId") or src
        mods = [m for m in d["Modifications"] if m["Id"] not in DROP]
        town = CAMPS[camp][0]
        uname = name_of(d, src)
        slot = sum(1 for r in REFUGEES[:i] if r and r[2] == camp)  # its button: 3 columns, x = 3 left for the rally
        gold = next((m["Value"] for m in mods if m["Id"] == "ugol"), None)
        if isinstance(gold, int):
            mods = [m for m in mods if m["Id"] != "ugol"] + [{"Id": "ugol", "Value": max(1, int(round(gold * PRICE)))}]
        mods += [{"Id": "ureq", "Type": 3, "Value": res}, {"Id": "ubpx", "Value": slot % 3},
                 {"Id": "ubpy", "Value": slot // 3}, {"Id": "unsf", "Type": 3, "Value": "(Refugee, " + shown + ")"},
                 {"Id": "utub", "Type": 3,
                  "Value": "|cffffcc00Refugees from " + shown + "|r, sold at the Refugee Slave Camp of " + town
                           + " at half the price. Only once " + shown + " has fallen (defeated, or its fallback); the "
                           "camp's stock refills over time. Every sale pays the Kezan Enterprise."}]
        write("UnitData", code, {"OldId": base, "NewId": code, "Unk": [0], "Modifications": mods})
        write("UpgradeData", res, {"OldId": "Roch", "NewId": res, "Unk": [0], "Modifications": [
            {"Level": 1, "Id": "gnam", "Type": 3, "Value": "No refugees from " + shown + " yet"},
            {"Level": 1, "Id": "gnsf", "Type": 3, "Value": "(Condition)"},
            {"Id": "glvl", "Value": 1}, {"Id": "gglb", "Value": 0}, {"Id": "glmb", "Value": 0}]})
        units.append((fac, code, res, camp, uname))
        info.append((code, categories(src), uname + " (refugee)"))
    camp_codes = []
    for k, (town, _, _, _, _) in enumerate(CAMPS):
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
             "    // per camp: its building, the town's CP"]
    lines.append("    internal static int[] Camps = { " + ", ".join(str(fourcc(c)) for c in camp_codes) + " };")
    lines.append("    internal static int[] CampCps = { " + ", ".join(c[1] for c in CAMPS) + " };")
    lines.append("    // where the camp stands; CampExact false = next to the CP at x, y (AiApi.FindBuildSpot)")
    lines.append("    internal static float[] CampXY = { " + ", ".join(
        f"{(c[4] or (c[2], c[3]))[0]}f, {(c[4] or (c[2], c[3]))[1]}f" for c in CAMPS) + " };")
    lines.append("    internal static bool[] CampExact = { " + ", ".join("true" if c[4] else "false" for c in CAMPS)
                 + " };")
    lines.append("    internal static string[] CampTowns = { " + ", ".join('"' + c[0] + '"' for c in CAMPS) + " };")
    lines.append("")
    lines.append("    // per refugee unit: the factions of its slot (Faction.Name, \"A|B\"), the unit, its condition research, its camp")
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
