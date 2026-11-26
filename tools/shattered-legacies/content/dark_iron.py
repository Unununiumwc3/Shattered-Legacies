"""The Dark Iron dwarves of Shadowforge: Ironforge's tech tree as Dark Iron copies (0.86.0, Ironforge's fallback).

Usage (repo root):  python3 tools/shattered-legacies/content/dark_iron.py
Reads WL's Ironforge object limits (src/WarcraftLegacies.Shared/FactionObjectLimits/IronforgeObjectInfo.cs) and copies
every Ironforge unit and building under a fork id (?Zi?): "Dark Iron " + its name, the suffix "(Dark Iron)", a dark
tint; train / build / upgrade / requirement lists point at the copies. Researches are shared (WL's ids).
Left out (maintainer: Ragnaros-free Dark Irons, no Bronzebeard heroes, no Wildhammer gryphons):
  Magni, Muradin, Falstad (their places at the altar: Golem Lord Argelmach and Fineous Darkvire, copies of Muradin /
  Magni, renamed); the Gryphon Aviary and Gryphon Riders; the Flame / Artillery Tanks (WL's researches unlock the
  original ids only) and the Deeprun Tram research. Dagran Thaurissan stays WL's legend (H03G).
Writes
  mapdata/WarcraftLegacies/UnitData/?Zi?.json
  src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/DarkIronObjectInfo.cs
  src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/Factions/DarkIron/DarkIronObjects.g.cs
Re-running gives the same ids (sorted by the Ironforge list's order; new entries go to the end of the id sequence).
"""
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
SHARED = os.path.join(ROOT, "src", "WarcraftLegacies.Shared")
IRONFORGE = os.path.join(SHARED, "FactionObjectLimits", "IronforgeObjectInfo.cs")
INFO = os.path.join(SHARED, "ShatteredLegacies", "FactionObjectLimits", "DarkIronObjectInfo.cs")
CONSTS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Factions", "DarkIron",
                      "DarkIronObjects.g.cs")
UNITS_TSV = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Content", "units.tsv")
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"
TINT = (170, 140, 130)  # soot and ember
EXCLUDE = {"H00S", "Hmbr", "H028", "H03G", "hgra", "hgry", "tp01", "tp02", "hbot", "h0AR", "h0AX", "h0B3", "h0B0",
           "h0B6", "h0AN", "h0B7"}
SHARED_UNITS = {"H03G", "hbot", "h0AR", "h0AX", "h0B3", "h0B0", "h0B6", "h0AN", "h0B7"}  # kept under WL's ids
DROP_RESEARCH = {"TP28", "TP29", "R014"}
# the two new heroes: copied from WL's hero, renamed (title, proper name, story)
HEROES = {
    "Hmbr": ("Golem Lord of Shadowforge", "Golem Lord Argelmach",
             "Master of Shadowforge's golem foundries. Without Ragnaros his constructs serve gold, not fire."),
    "H00S": ("Chief Architect of Shadowforge", "Fineous Darkvire",
             "The architect who raised Shadowforge City -- now he wants to rebuild Ironforge in its image."),
}
REF_FIELDS = ("utra", "ubui", "uupt", "ureq", "ures")
RENAME = {"n07U": "Dark Iron Black Market"}  # the lore: a black-market arsenal


def code(v):
    return v.to_bytes(4, "big").decode("latin1")


def constants():
    out = {}
    for f in ("Units.g.cs", "Upgrades.g.cs"):
        for name, value in re.findall(r"const int (\w+) = (-?\d+);", open(os.path.join(SHARED, f), encoding="utf-8-sig").read()):
            out[name] = int(value)
    return out


def ironforge_limits(consts):
    rows = []
    for line in open(IRONFORGE, encoding="utf-8-sig"):
        m = re.match(r"\s*yield return new\((\w+), (\w+)(?:, (.*?))?\)\s*;?\s*(\{)?", line)
        if not m:
            continue
        ident, limit, cats = m.group(1), m.group(2), m.group(3)
        value = int(ident) if ident.isdigit() else consts[ident]
        limit = 200 if limit == "Unlimited" else int(limit)
        rows.append((code(value), limit, cats, ident.startswith("UPGRADE_")))
    return rows


def stock_names():
    names = {}
    for line in open(UNITS_TSV, encoding="utf-8"):
        p = line.rstrip("\n").split("\t")
        if len(p) > 4 and p[1] in ("main", "stock"):
            names.setdefault(p[0], p[4])
    return names


def load(oid):
    f = os.path.join(MAP, "UnitData", oid + ".json")
    if os.path.exists(f):
        d = json.load(open(f, encoding="utf-8"))
        custom = d.get("NewId") is not None
        return (d["OldId"] if custom else oid), d.get("Modifications", [])
    return oid, []


def fourcc(c):
    return int.from_bytes(c.encode("latin1"), "big")


def main():
    consts = constants()
    rows = ironforge_limits(consts)
    names = stock_names()
    copied = list(dict.fromkeys(r[0] for r in rows if not r[3] and r[0] not in EXCLUDE)) + list(HEROES)
    ids, used = {}, {}
    for oid in copied:
        first = oid[0]
        k = used.get(first, 0)
        ids[oid] = first + "Zi" + SEQ[k]
        used[first] = k + 1

    def remap(value, field):
        out = []
        for t in str(value).split(","):
            t = t.strip()
            if not t:
                continue
            if field == "ures" and t in DROP_RESEARCH:
                continue
            if t in ids:
                out.append(ids[t])
            elif t in EXCLUDE and t not in SHARED_UNITS:
                continue
            else:
                out.append(t)
        return ",".join(out)

    for oid in copied:
        base, mods = load(oid)
        mods = json.loads(json.dumps(mods))
        name = next((m["Value"] for m in mods if m["Id"] == "unam"), names.get(oid, oid))
        keep = []
        for m in mods:
            if m["Id"] in REF_FIELDS and isinstance(m.get("Value"), str):
                m["Value"] = remap(m["Value"], m["Id"])
            if m["Id"] in ("unam", "unsf", "uclr", "uclg", "uclb", "upro"):
                continue
            keep.append(m)
        if oid in HEROES:
            title, proper, story = HEROES[oid]
            keep = [m for m in keep if m["Id"] not in ("ureq", "utub", "uawt", "utpr", "utip")]
            keep += [{"Id": "unam", "Type": 3, "Value": title}, {"Id": "upro", "Type": 3, "Value": proper},
                     {"Id": "ureq", "Type": 3, "Value": ""}, {"Id": "utub", "Type": 3, "Value": story},
                     {"Id": "uawt", "Type": 3, "Value": "Revive " + proper},
                     {"Id": "utpr", "Type": 3, "Value": "Revive " + proper},
                     {"Id": "utip", "Type": 3, "Value": "Summon " + proper}]
        else:
            new_name = RENAME.get(oid) or (name if name.startswith("Dark Iron") else "Dark Iron " + name)
            keep.append({"Id": "unam", "Type": 3, "Value": new_name})
        if oid == "h07B":  # the altar: Dagran (WL's legend) and the two new heroes
            keep = [m for m in keep if m["Id"] != "utra"]
            keep.append({"Id": "utra", "Type": 3, "Value": "H03G," + ids["Hmbr"] + "," + ids["H00S"]})
        keep += [{"Id": "unsf", "Type": 3, "Value": "(Dark Iron)"}, {"Id": "uclr", "Value": TINT[0]},
                 {"Id": "uclg", "Value": TINT[1]}, {"Id": "uclb", "Value": TINT[2]}]
        with open(os.path.join(MAP, "UnitData", ids[oid] + ".json"), "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps({"OldId": base, "NewId": ids[oid], "Unk": [0], "Modifications": keep}, indent=2,
                               ensure_ascii=False))

    # object limits: the copies with Ironforge's limits, the shared units / researches as they are
    lines = ["// <auto-generated> tools/shattered-legacies/content/dark_iron.py -- object limits of the Dark Iron",
             "// (Ironforge's, on the Dark Iron copies). Do not edit. </auto-generated>", "using MacroTools.Shared;",
             "using static MacroTools.Shared.UnitCategory;", "",
             "namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits", "{",
             "  public static class DarkIronObjectInfo", "  {",
             "    public static IEnumerable<ObjectInfo> GetAllObjectLimits()", "    {"]
    for oid, limit, cats, _ in rows:
        if oid in DROP_RESEARCH or (oid in EXCLUDE and oid not in SHARED_UNITS):
            continue
        new = ids.get(oid, oid)
        cat = (", " + cats) if cats else ""
        lines.append("      yield return new(%d, %d%s); // %s%s" % (fourcc(new), limit, cat, new,
                                                                 " (" + oid + ")" if new != oid else ""))
    for oid in HEROES:
        lines.append("      yield return new(%d, 1, Support); // %s %s" % (fourcc(ids[oid]), ids[oid], HEROES[oid][1]))
    lines += ["    }", "  }", "}", ""]
    with open(INFO, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))

    # constants
    inv = {v: k for k, v in consts.items()}
    os.makedirs(os.path.dirname(CONSTS), exist_ok=True)
    out = ["// <auto-generated> tools/shattered-legacies/content/dark_iron.py -- the Dark Iron copies of Ironforge's",
           "// objects. Do not edit. </auto-generated>",
           "namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIron", "{",
           "  internal static class DarkIronObjects", "  {"]
    special = {"Hmbr": "Argelmach", "H00S": "Darkvire"}
    for oid in copied:
        cname = special.get(oid)
        if not cname:
            const = inv.get(fourcc(oid), oid)
            words = re.sub(r"^UNIT_\w{4}_", "", const).replace("_IRONFORGE", "").split("_")
            cname = "".join(w.capitalize() for w in words if w and w not in ("OTHER",)) or oid
        out.append("    internal const int %s = %d; // %s (Ironforge %s)" % (cname, fourcc(ids[oid]), ids[oid], oid))
    out += ["  }", "}", ""]
    with open(CONSTS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out))
    print("Dark Iron:", len(copied), "copies")


if __name__ == "__main__":
    main()
