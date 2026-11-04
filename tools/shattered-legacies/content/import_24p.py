"""Import objects of the 24p map into the main map: new ids (ID rule ?Za?), their dependencies, their model files.

Usage (repo root):  python3 tools/shattered-legacies/content/import_24p.py
Reads the import sets below (one per new faction), then writes
  mapdata/WarcraftLegacies/{UnitData,AbilityData,UpgradeData,ItemData,BuffData}/<new id>.json   the copies
  mapdata/WarcraftLegacies/Imports/...                 model / icon files the copies need that only the 24p map has
                                                       (Git LFS files: run `git lfs pull` after committing them)
  src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/<Set>ObjectInfo.cs   object limits (GENERATED)
  src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/Factions/<Set>/<Set>Objects.g.cs  the new ids as constants
  src/WarcraftLegacies.Source/ShatteredLegacies/Content/SL_OBJECTS.md                  registry: new id | name | source
Rules: a referenced 24p object that is identical in the main map, or the same object with other data ("changed"),
is used under its main id; one that is "different" or "24p only" is copied with a new id. Stock objects stay.
Re-running gives the same ids (they are kept in tools/shattered-legacies/content/import_ids.json).
"""
import glob
import json
import os
import re
import shutil

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAIN = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
P24 = os.path.join(ROOT, "mapdata", "WarcraftLegacies24p")
KINDS = ["UnitData", "AbilityData", "UpgradeData", "ItemData", "BuffData"]
PREFIX = {"AbilityData": "A", "UpgradeData": "R", "ItemData": "I", "BuffData": "B"}
IDS_FILE = os.path.join(HERE, "import_ids.json")
SL_OBJECTS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Content", "SL_OBJECTS.md")
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"  # one case only: Windows file names ignore case

# ------------------------------------------------------------------ import sets
# (24p id, C# name, categories, limit). Limit 0 = the object is copied but not available (the set's code unlocks it).
FORSAKEN = {
    "name": "Forsaken",
    "third": "f",  # third character of the new ids: ?Zf?
    "objects": [
        ("u010", "AltarOfTheQueen", ["Altar"], 200),
        ("u01K", "Acolyte", ["Builder"], 200),
        ("h089", "DarkMansion", ["TownHall"], 200),
        ("h08A", "GrimEstate", ["Keep"], 200),
        ("h08B", "HauntedCastle", ["Castle"], 200),
        ("h08C", "Grave", ["Farm"], 200),
        ("h08D", "ScoutTower", ["Tower"], 200),
        ("h08E", "PlagueTower", ["Tower"], 200),
        ("h08F", "ImprovedPlagueTower", ["Tower"], 200),
        ("u011", "Plaguetorium", ["Barracks"], 200),
        ("u01J", "Graveyard", ["Research"], 200),
        ("u014", "Laboratory", ["SiegeWorkshop"], 200),
        ("u016", "ForbiddenArchive", ["Magic"], 200),
        ("u017", "ForsakenDen", ["Shop"], 200),
        ("u01A", "HauntedHarbor", ["Shipyard"], 200),
        ("u01W", "RoyalSepulcrum", ["Specialist"], 1),
        ("h08S", "Undercity", ["Specialist"], 1),
        ("h08O", "Rotguard", ["Fighter"], 200),
        ("n07S", "Deadeye", ["Marksman"], 200),
        ("h08N", "Dreadguard", ["Tank"], 200),
        ("n0BY", "DarkRanger", ["Marksman"], 12),
        ("h08P", "Archivist", ["Support"], 200),
        ("u01R", "Apothecary", ["Support"], 200),
        ("n07W", "PlagueConstruct", ["Siege"], 6),
        ("u01P", "PlagueCatapult", ["Siege"], 8),
        ("o05H", "PlagueFlyer", ["Flyer"], 12),
        ("n07V", "ElderBanshee", ["Support"], 6),
        ("u01V", "Valkyr", ["Flyer"], 6),
        ("u02G", "EliteAbominations", ["Tank"], 6),
        ("U01O", "GrandApothecary", ["Support"], 1),
        ("U02I", "MasterApothecary", ["Support"], 1),
        # researches whose ids the code needs (the Forsaken quests grant them)
        ("R04X", "QuestFreeWill", None, 0),       # "Quest Completed: Forsaken Independance" -> Free Will
        ("R04Z", "QuestThroneOfLordaeron", None, 0),  # "Quest Completed: The Secret Buildup" -> Undercity
        # 0.56: Sylvanas' 24p skills, for her phase heroes (tools/shattered-legacies/content/forsaken_heroes.py)
        ("A03J", "BlackArrow", None, 0),
        ("A01O", "TrueshotAura", None, 0),
        ("A0DU", "BansheeQueen", None, 0),
    ],
    # not imported (24p heroes / mechanics the fork does differently): Sylvanas Usyl (our UZa1 + phases), Ranger-Lord,
    # Arch Dreadlord, Cultist; "Declare Forsaken Independence" (R050: breaking free is level 6 here)
    "exclude": ["Usyl", "H049", "Uvar", "u01U", "R050", "R02D"],
    # field values to drop from copies: (24p id, field, token)
    "drop": [("u010", "utra", "Usyl"), ("h08B", "ures", "R050")],
    # 0.55.1 (maintainer): lumber is "knowledge" (the tome) in this map -- won by quests. The copies cost no lumber,
    # except what is set here (24p id -> {field: value}): Tier 4 costs knowledge.
    "zero_lumber": True,
    "set": {"R04R": {"glmb": 25, "glmm": 0, "greq": "R04X"},  # Tier 4 only after Free Will (maintainer, 0.57)
            "A0DU": {"arlv": 6, "alsk": 6},                     # her ultimate at level 6 (maintainer), not 10
            # 0.57 (maintainer): only these need Tier 4 -- Val'kyr, Plague Flyer (+ Flight), Elite Abominations,
            # Elder Banshee, the Royal Sepulcrum
            "u01V": {"ureq": "R04R"}, "o05H": {"ureq": "R04R,R09X"}, "u02G": {"ureq": "R04R"},
            "n07V": {"ureq": "R04R"}, "u01W": {"ureq": "R04R"},
            # the Haunted Harbor like every faction's: ships need Navigation (R04R of the MAIN map) -- researched at
            # the Graveyard together with Flight (R09X); no Tier 4 for the harbour itself
            "u01A": {"ureq": "RZs1"},
            "u01J": {"ures": "R02A,Rhme,Rhar,=R04R,R09X"},  # "=" = the main map's id, not the 24p one
            # 0.57 (maintainer): the Plaguetorium is the only troop building before Sylvanas reaches level 6 (RZs1,
            # "The Banshee's Rise", forsaken_heroes.py / QuestForsakenRise)
            "u014": {"ureq": "h08A,RZs1"}, "u016": {"ureq": "h08A,RZs1"}},
}
SETS = [FORSAKEN]


def load(map_dir):
    data = {}
    for kind in KINDS:
        for f in glob.glob(os.path.join(map_dir, kind, "*.json")):
            d = json.load(open(f, encoding="utf-8-sig"))
            oid = d.get("NewId") or d.get("OldId")
            data[oid] = (kind, d)
    return data


def name_of(d):
    for m in d.get("Modifications", []):
        if m["Id"] in ("unam", "anam", "gnam", "fnam") and m.get("Level") in (None, 0, 1):
            return str(m["Value"])
    return ""


def imports(map_dir):
    base = os.path.join(map_dir, "Imports")
    out = {}
    for f in glob.glob(os.path.join(base, "**", "*"), recursive=True):
        if os.path.isfile(f):
            rel = os.path.relpath(f, base).replace("\\", "/")
            out.setdefault(os.path.splitext(rel.lower())[0], []).append(rel)
    return out


def main():
    main_data, p24 = load(MAIN), load(P24)
    imp_main, imp_24 = imports(MAIN), imports(P24)
    ids = json.load(open(IDS_FILE)) if os.path.exists(IDS_FILE) else {}
    taken = {k.lower() for k in main_data} | {v.lower() for v in ids.values()}
    registry = []

    def needs_copy(oid):
        if oid not in p24:
            return False
        if oid in main_data:
            a, b = main_data[oid][1], p24[oid][1]
            same = a.get("OldId") == b.get("OldId") and a.get("Modifications") == b.get("Modifications")
            if same or name_of(a) == name_of(b):
                return False  # identical, or the same object with other data: use the main one
        return True

    def tokens(value):
        if isinstance(value, str):
            for t in value.split(","):
                if len(t.strip()) == 4:
                    yield t.strip()

    for s in SETS:
        roots = [o[0] for o in s["objects"]]
        exclude = set(s["exclude"])
        todo, closure = list(roots), []
        while todo:
            oid = todo.pop(0)
            if oid in closure or oid in exclude or not needs_copy(oid):
                continue
            closure.append(oid)
            for m in p24[oid][1].get("Modifications", []):
                todo.extend(t for t in tokens(m.get("Value")) if t not in closure)

        # new ids, stable across runs
        for oid in closure:
            key = s["name"] + ":" + oid
            if key in ids:
                continue
            kind = p24[oid][0]
            first = oid[0] if kind == "UnitData" else PREFIX[kind]
            for third in s["third"] + "ghijklmnop":
                new = next((first + "Z" + third + c for c in SEQ if (first + "Z" + third + c).lower() not in taken), None)
                if new:
                    break
            ids[key] = new
            taken.add(new.lower())
        mapping = {oid: ids[s["name"] + ":" + oid] for oid in closure}
        drops = {(a, b, c) for a, b, c in s["drop"]}

        copied_files = set()
        written = {}
        for oid in closure:
            kind, d = p24[oid]
            d = json.loads(json.dumps(d))
            d["NewId"] = mapping[oid]
            if s.get("zero_lumber"):
                for m in d.get("Modifications", []):
                    if m["Id"] in ("ulum", "glmb", "glmm"):
                        m["Value"] = 0
                if kind == "UnitData" and not any(m["Id"] == "ulum" for m in d.get("Modifications", [])):
                    d.setdefault("Modifications", []).append({"Id": "ulum", "Value": 0})
                if kind == "UpgradeData" and not any(m["Id"] == "glmb" for m in d.get("Modifications", [])):
                    d.setdefault("Modifications", []).append({"Id": "glmb", "Value": 0})
            for field, value in s.get("set", {}).get(oid, {}).items():
                hit = [m for m in d.get("Modifications", []) if m["Id"] == field]
                for m in hit:
                    m["Value"] = value
                if not hit:
                    d.setdefault("Modifications", []).append({"Id": field, "Value": value})
            for m in d.get("Modifications", []):
                v = m.get("Value")
                if isinstance(v, str):
                    parts = v.split(",")
                    ok = lambda p: len(p.strip()) == 4 or (p.strip().startswith("=") and len(p.strip()) == 5)
                    if all(ok(p) or p.strip() == "" for p in parts) and any(ok(p) for p in parts):
                        parts = [p for p in parts if (oid, m["Id"], p.strip()) not in drops]
                        m["Value"] = ",".join(p.strip()[1:] if p.strip().startswith("=") else mapping.get(p.strip(), p.strip())
                                              for p in parts)
                    else:
                        files, new_value = copy_import(v, imp_main, imp_24)
                        copied_files |= files
                        m["Value"] = new_value
            written[oid] = d
            out = os.path.join(MAIN, kind, mapping[oid] + ".json")
            with open(out, "w", encoding="utf-8", newline="\n") as f:
                f.write(json.dumps(d, indent=2, ensure_ascii=False))
            registry.append((mapping[oid], name_of(d), "24p:" + oid, s["name"]))

        # 0.55.1: every research the set's buildings offer needs an object limit (WL records research levels in the
        # faction and refuses levels above its limit: "level (1) cannot be higher than the object limit for Tier 4")
        researches = []  # from the copies as written (ids already the main map's)
        for oid in closure:
            if p24[oid][0] != "UnitData":
                continue
            for m in written[oid].get("Modifications", []):
                if m["Id"] == "ures" and isinstance(m.get("Value"), str):
                    for t in m["Value"].split(","):
                        t = t.strip()
                        if len(t) == 4 and t not in researches:
                            researches.append(t)
        s["researches"] = researches
        write_object_info(s, mapping)
        write_constants(s, mapping)
        print(f"{s['name']}: {len(closure)} objects copied, {len(copied_files)} import files copied")

    with open(IDS_FILE, "w") as f:
        json.dump(ids, f, indent=1, sort_keys=True)
    lines = ["# Objects of the fork (GENERATED -- `tools/shattered-legacies/content/import_24p.py`, do not edit)", "",
             "Every object the fork copied into the main map. Hand-made objects (`nZh0`, `UZa1`, the Hourglass",
             "researches ...) are listed in `../README.md` \"Changes to map data\".", "",
             "| New id | Name | Source | Used by |", "|---|---|---|---|"]
    lines += [f"| `{a}` | {b.replace('|', '/')} | `{c}` | {d} |" for a, b, c, d in sorted(registry)]
    with open(SL_OBJECTS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")


def safe_name(rel):
    """WL #4190: imports with more than one dot in the name do not show in game -> dots of the stem become _."""
    folder, name = os.path.split(rel)
    stem, ext = os.path.splitext(name)
    return os.path.join(folder, stem.replace(".", "_") + ext).replace("\\", "/")


def copy_import(value, imp_main, imp_24):
    """Copies the import a path value points to (and files named like it: portrait, disabled icon) if only 24p has it.
    Returns (copied files, the value pointing to the copy). Models that are real files (after `git lfs pull`) also
    bring the textures they use."""
    copied = set()
    p = value.replace("\\", "/").lower()
    if not re.search(r"\.(mdl|mdx|blp|tga)$", p) and "/" not in p:
        return copied, value
    stem = os.path.splitext(p)[0]
    if stem in imp_main or stem not in imp_24:
        return copied, value
    folder, base = os.path.split(stem)
    candidates = [k for k in imp_24 if k == stem or (os.path.split(k)[0] == folder and os.path.split(k)[1].startswith(base + "_"))]
    if "commandbuttons/btn" in stem:
        dis = stem.replace("commandbuttons/btn", "commandbuttonsdisabled/disbtn")
        candidates += [k for k in imp_24 if k == dis]
    for k in candidates:
        for rel in imp_24[k]:
            copied |= copy_file(rel, imp_main, imp_24)
    # the value points to the renamed copy (same extension as written in the data)
    v_folder, v_name = os.path.split(value.replace("\\", "/"))
    v_stem, v_ext = os.path.splitext(v_name)
    new_value = (v_folder + "/" if v_folder else "") + v_stem.replace(".", "_") + v_ext
    return copied, new_value.replace("/", "\\")


def copy_file(rel, imp_main, imp_24):
    copied = set()
    dst_rel = safe_name(rel)
    if os.path.splitext(dst_rel.lower())[0] in imp_main:
        return copied
    src = os.path.join(P24, "Imports", rel)
    dst = os.path.join(MAIN, "Imports", dst_rel)
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    shutil.copyfile(src, dst)
    copied.add(dst_rel)
    for tex in textures(src):
        key = os.path.splitext(tex.replace("\\", "/").lower())[0]
        if key in imp_24 and key not in imp_main:
            for trel in imp_24[key]:
                tdst = os.path.join(MAIN, "Imports", trel)  # textures keep their names (the model names them)
                if not os.path.exists(tdst):
                    os.makedirs(os.path.dirname(tdst), exist_ok=True)
                    shutil.copyfile(os.path.join(P24, "Imports", trel), tdst)
                    copied.add(trel)
    return copied


def textures(path):
    """Texture paths a model uses -- only for real files (a Git LFS pointer has no content to read)."""
    data = open(path, "rb").read()
    if data.startswith(b"version https://git-lfs"):
        return []
    if data[:4] == b"MDLX":
        out, pos = [], 4
        while pos + 8 <= len(data):
            tag, size = data[pos:pos + 4], int.from_bytes(data[pos + 4:pos + 8], "little")
            if tag == b"TEXS":
                for e in range(pos + 8, pos + 8 + size, 268):
                    name = data[e + 4:e + 264].split(b"\0")[0].decode("latin1")
                    if name:
                        out.append(name)
            pos += 8 + size
        return out
    return re.findall(r'Image\s+"([^"]+)"', data.decode("latin1", "replace"))


def fourcc(code):
    return int.from_bytes(code.encode("latin1"), "big", signed=True)


def write_object_info(s, mapping):
    path = os.path.join(ROOT, "src", "WarcraftLegacies.Shared", "ShatteredLegacies", "FactionObjectLimits",
                        s["name"] + "ObjectInfo.cs")
    extra = getattr_extra(s)
    lines = [f"// <auto-generated> tools/shattered-legacies/content/import_24p.py -- object limits of the {s['name']}",
             "// (from its import set + the hand-made objects listed there). Do not edit. </auto-generated>",
             "using MacroTools.Shared;", "using static MacroTools.Shared.UnitCategory;", "",
             "namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits", "{",
             f"  public static class {s['name']}ObjectInfo", "  {",
             f"    public static IEnumerable<ObjectInfo> GetAllObjectLimits()", "    {"]
    for code, name, cats, limit in extra + [(mapping.get(o, o), n, c, l) for o, n, c, l in s["objects"]]:
        if cats is None:
            continue
        cat = cats[0] if len(cats) == 1 else "new List<UnitCategory> { " + ", ".join(cats) + " }"
        lines.append(f"      yield return new({fourcc(code)}, {limit}, {cat}); // {code} {name}")
    for code in s.get("researches", []):
        lines.append(f"      yield return new({fourcc(code)}, 200); // {code} research")
    lines += ["    }", "  }", "}", ""]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def getattr_extra(s):
    # hand-made objects of the set that are not imported (Sylvanas the banshee, 0.54)
    if s["name"] == "Forsaken":
        return [("UZa1", "SylvanasBanshee", ["Marksman", "Support"], 1),        # forsaken_heroes.py, 0.56
                ("UZa2", "SylvanasDarkRanger", ["Marksman", "Support"], 1),
                ("UZa3", "SylvanasQueen", ["Marksman", "Support"], 1)]
    return []


def write_constants(s, mapping):
    path = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Factions", s["name"],
                        s["name"] + "Objects.g.cs")
    lines = [f"// <auto-generated> tools/shattered-legacies/content/import_24p.py -- ids of the {s['name']}' objects",
             "// copied from the 24p map. Do not edit. </auto-generated>",
             f"namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.{s['name']}", "{",
             f"  internal static class {s['name']}Objects", "  {"]
    for o, n, _, _ in s["objects"]:
        code = mapping.get(o, o)
        lines.append(f"    internal const int {n} = {fourcc(code)}; // {code} (24p {o})")
    lines += ["  }", "}", ""]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    main()
