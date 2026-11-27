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

0.87.0 (maintainer: "replace the Dark Iron with the 24p's"): their TROOPS are the 24p's Dark Iron units now (import set
DARK_IRON of import_24p.py, ids ?Zz? -- run import_24p.py first): REPLACED below maps an Ironforge unit to its 24p
successor in every train list; the replaced copies keep their reserved id but are not written. The Mason, the Rifleman,
the Mortar Team, the War Golem and the Dreadnaught stay tinted copies. New:
  - the heroes: Argelmach with the Dark Iron caster model and "Forge Golem" (AZi0, summons War Golems) for Storm Bolt,
    Darkvire with the Chieftain-king model; no tint on either
  - Shadowforge (VOID_DUNGEONS.md): the Throne of Thaurissan (hZiy, their HQ, by the Throne Room CP), the Statue of
    Thaurissan (hZix, the Siege Foundry holder's aura), the Shadowforge Black Market (nZiy, a neutral shop anyone may
    use) and its Titanforged trinkets (IZi0 .. IZi2) -- expensive
Shadowforge's own throne / statue doodads are replaced by these units (dungeon_content.py places them).
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
RENAME = {"n07U": "Dark Iron Arms Vault"}  # 0.87.0: "Black Market" is Shadowforge's neutral shop now
IMPORT_IDS = os.path.join(HERE, "import_ids.json")
# 0.87.0: Ironforge unit -> its 24p Dark Iron successor (import set DARK_IRON)
REPLACED = {"h018": "h0A6",   # Defender -> Anvilrage Guardsman
            "h01L": "h0A7",   # Thane -> Bear Rider
            "h01P": "h0AA",   # Steam Tank -> Magma Tank
            "n00C": "h0AB",   # Rune Priest -> Shadowcaster
            "tp05": "n0CH"}   # Rune Smith -> Flame Keeper
ADD_TRAIN = {"h07C": ["h0A8"], "harm": ["hgyr"]}  # barracks: the Chieftain (elite); workshop: the Gyrocopter
HERO_LOOK = {"Hmbr": ("war3mapImported\\Unit_DarkIronCaster.mdl",
                      "ReplaceableTextures\\CommandButtons\\BTNDarkIronCaster.blp", 1.25),
             "H00S": ("war3mapImported\\KingBloodstoneRagnar.mdl", "ReplaceableTextures\\CommandButtons\\BTNKingRagnar.blp",
                      1.2)}
FORGE_GOLEM = "AZi0"
ARGELMACH_SWAP = ("TP49", FORGE_GOLEM)  # Muradin's Storm Bolt -> Forge Golem
THRONE, STATUE, MARKET = "hZiy", "hZix", "nZiy"
TRINKETS = [  # id, name, ability, price, icon, text
    ("IZi0", "Titanforged Signet", "A0VJ", 1000, "ReplaceableTextures\\CommandButtons\\BTNRingJadeFalcon.blp",
     "A ring from the Titan forges beneath Shadowforge. +3 to all attributes."),
    ("IZi1", "Titanforged Gauntlets", "A0VM", 1200, "ReplaceableTextures\\CommandButtons\\BTNGauntletsOfOgrePower.blp",
     "Gauntlets of Titan steel. +9 Strength."),
    ("IZi2", "Titanforged Core", "A0VN", 1800, "ReplaceableTextures\\CommandButtons\\BTNOrbOfFire.blp",
     "A Titan engine's burning heart. Burns nearby enemy units for 40 damage per second."),
]


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


def imported():
    """24p id -> its id in the DARK_IRON import set (import_24p.py)"""
    table = json.load(open(IMPORT_IDS))
    return {k.split(":", 1)[1]: v for k, v in table.items() if k.startswith("DarkIronForces:")}


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
    forces = imported()
    successor = {oid: forces[new] for oid, new in REPLACED.items()}
    for oid in successor:  # 0.87.0: replaced -- the id stays reserved, the file goes
        stale = os.path.join(MAP, "UnitData", ids[oid] + ".json")
        if os.path.exists(stale):
            os.remove(stale)
    written = [oid for oid in copied if oid not in successor]

    def remap(value, field):
        out = []
        for t in str(value).split(","):
            t = t.strip()
            if not t:
                continue
            if field == "ures" and t in DROP_RESEARCH:
                continue
            if t in successor:
                out.append(successor[t])
            elif t in ids:
                out.append(ids[t])
            elif t in EXCLUDE and t not in SHARED_UNITS:
                continue
            else:
                out.append(t)
        return ",".join(out)

    for oid in written:
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
            model, icon, scale = HERO_LOOK[oid]
            keep = [m for m in keep if m["Id"] not in ("ureq", "utub", "uawt", "utpr", "utip", "umdl", "uico", "usca")]
            keep += [{"Id": "umdl", "Type": 3, "Value": model}, {"Id": "uico", "Type": 3, "Value": icon},
                     {"Id": "usca", "Type": 1, "Value": scale}]
            if oid == "Hmbr":
                for m in keep:
                    if m["Id"] == "uhab":
                        m["Value"] = ",".join(ARGELMACH_SWAP[1] if t == ARGELMACH_SWAP[0] else t
                                              for t in str(m["Value"]).split(","))
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
        if oid in ADD_TRAIN:
            extra = [forces[t] for t in ADD_TRAIN[oid]]
            hit = [m for m in keep if m["Id"] == "utra"]
            if hit:
                hit[0]["Value"] = ",".join([t for t in str(hit[0]["Value"]).split(",") if t] + extra)
            else:
                keep.append({"Id": "utra", "Type": 3, "Value": ",".join(extra)})
        keep.append({"Id": "unsf", "Type": 3, "Value": "(Dark Iron)"})
        if oid not in HEROES:
            keep += [{"Id": "uclr", "Value": TINT[0]}, {"Id": "uclg", "Value": TINT[1]}, {"Id": "uclb", "Value": TINT[2]}]
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
        if oid in DROP_RESEARCH or (oid in EXCLUDE and oid not in SHARED_UNITS) or oid in successor:
            continue  # 0.87.0: the 24p successors' limits are DarkIronForcesObjectInfo's
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
    for oid in written:
        cname = special.get(oid)
        if not cname:
            const = inv.get(fourcc(oid), oid)
            words = re.sub(r"^UNIT_\w{4}_", "", const).replace("_IRONFORGE", "").split("_")
            cname = "".join(w.capitalize() for w in words if w and w not in ("OTHER",)) or oid
        out.append("    internal const int %s = %d; // %s (Ironforge %s)" % (cname, fourcc(ids[oid]), ids[oid], oid))
    out += ["",
            "    // 0.87.0: Shadowforge's own objects",
            "    internal const int ThroneOfThaurissan = %d; // %s, their HQ" % (fourcc(THRONE), THRONE),
            "    internal const int StatueOfThaurissan = %d; // %s, the Siege Foundry holder's aura" % (fourcc(STATUE),
                                                                                                    STATUE),
            "    internal const int ShadowforgeBlackMarket = %d; // %s, neutral" % (fourcc(MARKET), MARKET),
            "    internal const int ForgeGolem = %d; // %s, Argelmach's summon" % (fourcc(FORGE_GOLEM), FORGE_GOLEM),
            "  }", "}", ""]
    with open(CONSTS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(out))
    shadowforge(ids, forces)
    print("Dark Iron:", len(written), "copies,", len(successor), "replaced by the 24p's")


def write_obj(kind, oid, base, mods):
    with open(os.path.join(MAP, kind, oid + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps({"OldId": base, "NewId": oid, "Unk": [0], "Modifications": mods}, indent=2,
                           ensure_ascii=False))


def t3(fid, value, level=None):
    m = {"Id": fid, "Type": 3, "Value": value}
    if level is not None:
        m = {"Id": fid, "Level": level, "Type": 3, "Value": value}
    return m


def shadowforge(ids, forces):
    # Argelmach's Forge Golem: Summon Water Elemental with the Dark Iron War Golem (1 / 1 / 2 golems, 60 s)
    golem = ids["n02D"]
    mods = [t3("anam", "Forge Golem"), t3("aart", "ReplaceableTextures\\CommandButtons\\BTNArmorGolem.blp"),
            t3("arar", "ReplaceableTextures\\CommandButtons\\BTNArmorGolem.blp"), t3("arhk", "Q"), t3("ahky", "Q"),
            {"Id": "alev", "Value": 3}, {"Id": "aher", "Value": 1}, {"Id": "arlv", "Value": 1}, {"Id": "alsk", "Value": 2},
            t3("aret", "Learn Forge Golem - [|cffffcc00Level %d|r]"),
            t3("arut", "Argelmach wakes a Dark Iron War Golem from the foundries for 60 seconds.|n|n"
                       "|cffffcc00Level 1|r - 1 golem.|n|cffffcc00Level 2|r - 1 golem, cooldown 20 s.|n"
                       "|cffffcc00Level 3|r - 2 golems.")]
    for lv, count, cd in ((1, 1, 30), (2, 1, 20), (3, 2, 20)):
        mods += [{"Id": "Hwe1", "Level": lv, "Type": 3, "Value": golem}, {"Id": "Hwe2", "Level": lv, "Value": count},
                 {"Id": "adur", "Level": lv, "Type": 2, "Value": 60}, {"Id": "acdn", "Level": lv, "Type": 2, "Value": cd},
                 {"Id": "amcs", "Level": lv, "Value": 100},
                 t3("atp1", "Forge Golem - [|cffffcc00Level %d|r]" % lv, lv),
                 t3("aub1", "Wakes %d Dark Iron War Golem%s for 60 seconds." % (count, "s" if count > 1 else ""), lv)]
    write_obj("AbilityData", FORGE_GOLEM, "AHwe", mods)

    # the Throne of Thaurissan: the Dark Iron HQ by the Throne Room CP (a Great Hold with the throne's model)
    _, hold = load("h07G")
    keep = [m for m in json.loads(json.dumps(hold)) if m["Id"] not in
            ("unam", "unsf", "umdl", "upat", "usca", "ussc", "uupt", "ures", "utra", "ureq", "utub", "utip", "uico",
             "ubui", "uclr", "uclg", "uclb", "upro")]
    keep += [t3("unam", "Throne of Thaurissan"), t3("unsf", "(Dark Iron)"),
             t3("umdl", "Doodads\\Barrens\\Props\\HellscreamThrone\\HellscreamThrone.mdl"),
             t3("uico", "ReplaceableTextures\\CommandButtons\\BTNDwarvenTreasury.blp"),
             t3("upat", "PathTextures\\4x4SimpleSolid.tga"), {"Id": "usca", "Type": 1, "Value": 1.6},
             {"Id": "ussc", "Type": 1, "Value": 2.0}, t3("utra", ids["h019"]), t3("uupt", ""), t3("ures", ""),
             t3("ureq", ""), t3("utub", "The seat of the Dark Iron emperors in the heart of Shadowforge.")]
    write_obj("UnitData", THRONE, "hcas", keep)

    # the Statue of Thaurissan: no attack; its aura (the 24p's Will of the Emperor) is added by the code
    write_obj("UnitData", STATUE, "hwtw", [
        t3("unam", "Statue of Thaurissan"), t3("unsf", "(Shadowforge)"),
        t3("umdl", "war3mapImported\\Prop_DwarfStatue.mdl"),  # Shadowforge's dwarf statue doodad (D006)
        t3("uico", "ReplaceableTextures\\CommandButtons\\BTNStatUp.blp"), {"Id": "usca", "Type": 1, "Value": 2.95},
        {"Id": "ussc", "Type": 1, "Value": 3.0}, t3("upat", ""), t3("uabi", "Avul"),
        t3("utub", "Dagran Thaurissan in stone. Whoever holds the Siege Foundry stands under his gaze: +4 armour "
                   "to nearby units.")])

    # the Shadowforge Black Market (neutral, like a Goblin Merchant): expensive, one of each, slow restock
    goods = [forces["I00T"], forces["I00V"], forces["I01B"], forces["fgrg"]] + [t[0] for t in TRINKETS]
    write_obj("UnitData", MARKET, "ngme", [
        t3("unam", "Shadowforge Black Market"), t3("usei", ",".join(goods)),
        t3("umdl", "war3mapImported\\DwarvenTurret.mdl"), {"Id": "usca", "Type": 1, "Value": 1.4},
        t3("upat", "PathTextures\\4x4SimpleSolid.tga"),
        t3("uico", "ReplaceableTextures\\CommandButtons\\BTNDwarvenTreasury.blp"),
        t3("utub", "Dark Iron war-profiteers sell to anyone who pays.")])
    for oid, name, abil, price, icon, text in TRINKETS:
        abil = forces.get(abil, abil)  # the 24p's immolation is an import (A0VN -> its ?Zz? id)
        write_obj("ItemData", oid, "ratc", [
            t3("unam", name), t3("iabi", abil), {"Id": "igol", "Value": price}, t3("iico", icon), t3("ides", text),
            t3("utub", text), t3("utip", "Purchase " + name), {"Id": "isto", "Value": 1}, {"Id": "istr", "Value": 300},
            {"Id": "isst", "Value": 0}])


if __name__ == "__main__":
    main()
