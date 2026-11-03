"""Content index: every object of the main map, the 24p map and stock WC3, with what it is and whether it is used.

Usage (repo root):  python3 tools/shattered-legacies/content/content_index.py
Writes src/WarcraftLegacies.Source/ShatteredLegacies/Content/{units,abilities,upgrades,items}.tsv and REUSE.md.
Re-run after map data changes (a few seconds). Read Content/README.md for the columns.

Sources (all in the repo, nothing else needed):
  main   mapdata/WarcraftLegacies/        object data JSON, preplaced units, Imports/
  24p    mapdata/WarcraftLegacies24p/     the same for the 24-player map (part of WL 4.8.2)
  stock  src/War3Api.Object/Generated/1.36.0.20257/   WC3's own objects (names, models, icons, train lists ...)
         -- referenced only; stock models and icons are in every WC3 install, never in the repo.

Status of a row:
  main   used / unused -- "used" = reachable from WL's faction object limits, the preplaced units or a reference in
         the C# code, through the objects' own references (trains, builds, researches, sells, abilities, upgrades,
         requirements, summoned units in changed ability data ...). Heuristic: an object only used through a stock
         ability's built-in data (e.g. a summon nobody changed) shows "unused".
  24p    24p only (id not in the main map) / changed (same id and name, other data) / different (same id, a
         DIFFERENT object) -- identical copies of main objects are left out. A copy into the main map always gets a
         new ?Za? id (Z second, lowercase third; ID rule, ShatteredLegacies/README.md).
  stock  used / unused in the main map (same rule as main).
"""
import glob
import json
import os
import re
import struct
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
sys.path.insert(0, os.path.join(HERE, "..", "ai"))
import unit_scan as us  # noqa: E402  (WL's object limits per faction, the generated constants)

STOCK = os.path.join(ROOT, "src", "War3Api.Object", "Generated", "1.36.0.20257")
MAPS = {"main": os.path.join(ROOT, "mapdata", "WarcraftLegacies"),
        "24p": os.path.join(ROOT, "mapdata", "WarcraftLegacies24p")}
OUT = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Content")
CODE_DIRS = [os.path.join(ROOT, "src", d) for d in ("WarcraftLegacies.Source", "MacroTools", "WarcraftLegacies.Shared")]

# kind -> (map data folder, War3Api class, name field, suffix field, icon field, race field)
KINDS = {
    "units": ("UnitData", "Unit", "unam", "unsf", "uico", "urac"),
    "abilities": ("AbilityData", "Ability", "anam", "ansf", "aart", "arac"),
    "upgrades": ("UpgradeData", "Upgrade", "gnam", "gnsf", "gar1", "grac"),
    "items": ("ItemData", "Item", "unam", "unsf", "iico", None),
}


def le(n):
    return struct.pack("<i", int(n)).decode("latin1")


def be(n):
    return struct.pack(">i", int(n)).decode("latin1")


def clean(v):
    return str(v).replace("\t", " ").replace("\r", " ").replace("\n", " ").strip()


# ---------------------------------------------------------------- stock WC3 objects (War3Api.Object)
def stock_props(cls):
    """property name -> 4-char field id, from War3Api's generated class (plain and per-level properties)."""
    s = open(os.path.join(STOCK, cls + ".cs"), encoding="utf-8-sig").read()
    m = {}
    for name, n in re.findall(r"public \S+ (\w+)\s*\{\s*get => _modifications\.GetModification\((-?\d+)\)", s):
        m[name] = le(n)
    for name, n in re.findall(r"private \S+ Get(\w+)\(int level\)\s*\{\s*return _modifications\.GetModification\((-?\d+), level\)", s):
        m.setdefault(name, le(n))
    return m


def stock_enum(cls):
    s = open(os.path.join(STOCK, "Enums", cls + "Type.cs"), encoding="utf-8-sig").read()
    return {name: code for code, name in re.findall(r"<summary>[^<]*\('(.{4})'\)\.</summary>\s*(\w+) = -?\d+", s)}


def cs_value(raw):
    raw = raw.strip()
    if raw.startswith('"'):
        try:
            return json.loads(raw)
        except ValueError:
            return raw.strip('"')
    return raw.rstrip("f")


def stock_objects(cls):
    props, enum = stock_props(cls), stock_enum(cls)
    s = open(os.path.join(STOCK, cls + "Loader.cs"), encoding="utf-8-sig").read()
    out = {}
    for chunk in s.split("protected virtual ")[1:]:
        m = re.search(r"var (\w+) = new (\w+)\((?:\w+Type\.(\w+), )?db\);", chunk)
        if not m:
            continue
        var, klass, member = m.groups()
        code = enum.get(member or klass)
        if not code:
            continue
        fields = {}
        for name, lvl, val in re.findall(rf"^\s*{var}\.(\w+)(?:\[(\d+)\])? = (.+?);\s*$", chunk, re.M):
            fid = props.get(name)
            if fid and (lvl in ("", "1") or fid not in fields):
                fields[fid] = cs_value(val)
        out[code] = fields
    return out


# ---------------------------------------------------------------- map objects (JSON)
def map_objects(folder):
    out = {}
    for f in sorted(glob.glob(os.path.join(folder, "*.json"))):
        d = json.load(open(f, encoding="utf-8-sig"))
        oid = d.get("NewId") or d.get("OldId")
        first, rank, every = {}, {}, []
        for x in d.get("Modifications", []):
            v = x.get("Value")
            every.append(v)
            r = 0 if x.get("Level") in (None, 0, 1) else 1  # level 1 (or no level) wins over higher levels
            if x["Id"] not in first or r < rank[x["Id"]]:
                first[x["Id"]], rank[x["Id"]] = v, r
        out[oid] = {"base": d.get("OldId"), "mods": first, "all": every, "custom": bool(d.get("NewId"))}
    return out


def imports(map_dir):
    base = os.path.join(map_dir, "Imports")
    s = set()
    for f in glob.glob(os.path.join(base, "**", "*"), recursive=True):
        if os.path.isfile(f):
            s.add(os.path.splitext(os.path.relpath(f, base).replace("\\", "/").lower())[0])
    return s


def model_source(path, imp):
    if not path or str(path).strip() in ("", "_"):
        return ""
    p = os.path.splitext(str(path).replace("\\", "/").lower())[0]
    if p in imp["main"]:
        return "main"
    if p in imp["24p"]:
        return "24p"
    if p.startswith("war3mapimported/"):
        return "missing"
    return "stock"


def preplaced(map_dir):
    ids = {}
    for f in glob.glob(os.path.join(map_dir, "Units", "*.json")):
        for u in json.load(open(f, encoding="utf-8-sig")):
            for t in [u.get("TypeId")] + [i.get("ItemId") for i in u.get("InventoryData") or []]:
                if t:
                    c = le(t)  # preplaced units store their type id little-endian (tools/.../map_query.py)
                    ids[c] = ids.get(c, 0) + 1
    return ids


def code_refs(consts):
    text = []
    for d in CODE_DIRS:
        for f in glob.glob(os.path.join(d, "**", "*.cs"), recursive=True):
            if not f.endswith(".g.cs") or "ShatteredLegacies" in f:
                text.append(open(f, encoding="utf-8-sig", errors="replace").read())
    text = "\n".join(text)
    refs = set()
    names = set(re.findall(r"\b[A-Z][A-Z0-9_]{3,}\b", text))
    for n in names & set(consts):
        refs.add(be(consts[n]))
    for n in re.findall(r"\b\d{9,10}\b", text):
        if int(n) < 2 ** 31:
            refs.add(be(int(n)))
    refs |= set(re.findall(r"[\"'](\w{4})[\"']", text))
    return refs


def tokens(values):
    for v in values:
        if isinstance(v, str):
            for t in v.split(","):
                t = t.strip()
                if len(t) == 4:
                    yield t


def split_list(v):
    return [t.strip() for t in str(v or "").split(",") if len(t.strip()) == 4]


# ---------------------------------------------------------------- build
def main():
    stock = {k: stock_objects(cls) for k, (_, cls, *_r) in KINDS.items()}
    imp = {m: imports(d) for m, d in MAPS.items()}
    maps = {m: {k: map_objects(os.path.join(d, folder)) for k, (folder, *_r) in KINDS.items()} for m, d in MAPS.items()}

    def fields(m, kind, oid):
        o = maps[m][kind].get(oid)
        if o is None:
            return dict(stock[kind].get(oid, {}))
        f = dict(stock[kind].get(o["base"], {}))
        f.update(o["mods"])
        return f

    def known(m, oid):
        return any(oid in maps[m][k] or oid in stock[k] for k in KINDS)

    def refs_of(m, oid):
        out = set()
        for kind in KINDS:
            o = maps[m][kind].get(oid)
            if o is not None:
                vals = list(fields(m, kind, oid).values()) + o["all"]
            elif oid in stock[kind]:
                vals = list(stock[kind][oid].values())
            else:
                continue
            out |= {t for t in tokens(vals) if t != oid and known(m, t)}
        return out

    # usage in the main map
    consts = us.load_consts()
    infos = us.object_infos(consts)
    faction_of = {}
    for fac, items in infos.items():
        for _, val, _ in items:
            faction_of.setdefault(be(val), []).append(fac)
    placed = preplaced(MAPS["main"])
    roots = set(faction_of) | set(placed) | code_refs(consts)
    used, todo = set(), [r for r in roots if known("main", r)]
    while todo:
        o = todo.pop()
        if o in used:
            continue
        used.add(o)
        todo.extend(refs_of("main", o) - used)

    # reverse links per map: child -> parents (trained / built / researched / sold / made at)
    def reverse(m, field_ids):
        rev = {}
        for oid in set(maps[m]["units"]) | (set(stock["units"]) if m == "main" else set()):
            if m == "main" and oid not in maps[m]["units"] and oid not in used:
                continue
            f = fields(m, "units", oid)
            for fid in field_ids:
                for c in split_list(f.get(fid)):
                    rev.setdefault(c, []).append(oid)
        return rev

    rev_unit = {m: reverse(m, ("utra", "ubui", "useu")) for m in MAPS}
    rev_up = {m: reverse(m, ("ures",)) for m in MAPS}
    rev_item = {m: reverse(m, ("usei", "umki")) for m in MAPS}
    rev_abil = {}
    for m in MAPS:
        r = {}
        for kind, fids in (("units", ("uabi", "uhab")), ("items", ("iabi",))):
            for oid in maps[m][kind]:
                for c in split_list(fields(m, kind, oid).get(fids[0])) + (
                        split_list(fields(m, kind, oid).get(fids[1])) if len(fids) > 1 else []):
                    r.setdefault(c, []).append(oid)
        rev_abil[m] = r

    def name(m, kind, oid):
        return clean(fields(m, kind, oid).get(KINDS[kind][2]) or "")

    def status(m, kind, oid):
        if m == "main" or m == "stock":
            return "used" if oid in used else "unused"
        main = maps["main"][kind].get(oid)
        if main is None:
            return "24p only"
        if main["mods"] == maps["24p"][kind][oid]["mods"] and main["base"] == maps["24p"][kind][oid]["base"]:
            return None  # identical copy: left out
        return "changed" if name("main", kind, oid) == name("24p", kind, oid) else "different"

    def rows(kind):
        for m in ("main", "24p"):
            for oid in sorted(maps[m][kind]):
                st = status(m, kind, oid)
                if st:
                    yield m, oid, st
        for oid in sorted(stock[kind]):
            if oid not in maps["main"][kind]:
                yield "stock", oid, status("stock", kind, oid)

    def short(lst, n=8):
        lst = sorted(set(lst))
        return ",".join(lst[:n]) + (f",+{len(lst) - n}" if len(lst) > n else "")

    os.makedirs(OUT, exist_ok=True)
    head = {
        "units": ["id", "map", "status", "kind", "name", "suffix", "base", "race", "model", "model_src", "icon",
                  "faction", "made_at", "trains", "builds", "researches", "sells", "abilities", "requires"],
        "abilities": ["id", "map", "status", "name", "suffix", "base", "hero", "item", "race", "icon", "on"],
        "upgrades": ["id", "map", "status", "name", "suffix", "base", "race", "icon", "researched_at"],
        "items": ["id", "map", "status", "name", "base", "class", "icon", "abilities", "sold_at"],
    }
    summary = {}
    reuse = {"main": [], "24p": []}
    for kind in KINDS:
        _, _, nf, sf, icf, rf = KINDS[kind]
        lines = ["\t".join(head[kind])]
        counts = {}
        for m, oid, st in rows(kind):
            src = "main" if m == "stock" else m
            f = fields(src, kind, oid) if m != "stock" else dict(stock[kind][oid])
            base = (maps[m][kind][oid]["base"] if m != "stock" else oid) or oid
            counts[(m, st)] = counts.get((m, st), 0) + 1
            nm, sfx = clean(f.get(nf) or ""), clean(f.get(sf) or "")
            race = clean(f.get(rf) or "") if rf else ""
            icon = clean(f.get(icf) or "")
            if kind == "units":
                bld = str(f.get("ubdg", "0")).lower() in ("1", "true")
                k = "hero" if oid[0].isupper() else ("building" if bld else "unit")
                model = clean(f.get("umdl") or "")
                row = [oid, m, st, k, nm, sfx, base, race, model, model_source(model, imp), icon,
                       ",".join(sorted(set(faction_of.get(oid, [])))) if src == "main" else "",
                       short(rev_unit[src].get(oid, [])), short(split_list(f.get("utra")), 12),
                       short(split_list(f.get("ubui")), 12), short(split_list(f.get("ures")), 12),
                       short(split_list(f.get("useu")) + split_list(f.get("usei")), 12),
                       short(split_list(f.get("uabi")) + split_list(f.get("uhab")), 12), clean(f.get("ureq") or "")]
                if m in reuse and st in ("unused", "24p only", "different") and nm:
                    reuse[m].append((race or "-", k, oid, nm, sfx, row[9], row[12], row[13] or row[14], st))
            elif kind == "abilities":
                row = [oid, m, st, nm, sfx, base, "yes" if str(f.get("aher", "0")) in ("1", "True") else "",
                       "yes" if str(f.get("aite", "0")) in ("1", "True") else "", race, icon,
                       short(rev_abil[src].get(oid, []), 6)]
            elif kind == "upgrades":
                row = [oid, m, st, nm, sfx, base, race, icon, short(rev_up[src].get(oid, []))]
            else:
                row = [oid, m, st, nm, base, clean(f.get("icla") or ""), icon, short(split_list(f.get("iabi"))),
                       short(rev_item[src].get(oid, []))]
            lines.append("\t".join(clean(x) or "-" for x in row))  # "-" = none (no empty cells, no trailing tabs)
        open(os.path.join(OUT, kind + ".tsv"), "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")
        summary[kind] = counts
        print(f"{kind}: {len(lines) - 1} rows")

    # REUSE.md -- the browsable part: units, heroes, buildings that are free to recycle
    md = ["# Reuse candidates (GENERATED -- `tools/shattered-legacies/content/content_index.py`, do not edit)", "",
          "Units, heroes and buildings that can be recycled for new content, by race. **model**: `stock` (every WC3",
          "install has it), `main` (import already in the main map), `24p` (import only in the 24p map -- the copy needs",
          "the model file too), `missing` (the import is not in the repo). **trains / builds**: what a building makes.",
          "Full data and every other object (abilities, upgrades, items, used objects, stock WC3): the TSV files, see",
          "`README.md`. Ask for content as `main:<id>`, `24p:<id>`, `stock:<id>`.", "", "## Counts", "",
          "| Kind | main used | main unused | 24p only | 24p changed | 24p different | stock used | stock unused |",
          "|---|---|---|---|---|---|---|---|"]
    for kind, c in summary.items():
        md.append(f"| {kind} | {c.get(('main', 'used'), 0)} | {c.get(('main', 'unused'), 0)} | "
                  f"{c.get(('24p', '24p only'), 0)} | {c.get(('24p', 'changed'), 0)} | {c.get(('24p', 'different'), 0)} | "
                  f"{c.get(('stock', 'used'), 0)} | {c.get(('stock', 'unused'), 0)} |")
    sections = (("main", "Unused in the main map",
                 "Objects of the main map that nothing uses (no faction limit, not preplaced, not in the code, not made "
                 "or referenced by anything used). Free to reuse under their id."),
                ("24p", "24p map: not in the main map",
                 "Status `24p only` = the id is free in the main map; `different` = the id is taken by ANOTHER object in "
                 "the main map. Copies get a new `?Za?` id. (`changed` = the same object with other data: only in "
                 "units.tsv.)"))
    for m, title, text in sections:
        md += ["", f"## {title}", "", text]
        by_race = {}
        for r in reuse[m]:
            by_race.setdefault(r[0], []).append(r)
        for race in sorted(by_race):
            md += ["", f"### {race} ({len(by_race[race])})", "",
                   "| Id | Name | Kind | Model | Made at | Trains / builds | Status |", "|---|---|---|---|---|---|---|"]
            for _, k, oid, nm, sfx, src, at, tb, st in sorted(by_race[race], key=lambda r: ("hbu".index(r[1][0]), r[3])):
                label = (nm + (f" {sfx}" if sfx else "")).replace("|", "/")
                md.append(f"| `{oid}` | {label} | {k} | {src} | {at} | {tb} | {st} |")
    open(os.path.join(OUT, "REUSE.md"), "w", encoding="utf-8", newline="\n").write("\n".join(md) + "\n")
    print("wrote", os.path.relpath(OUT, ROOT))


if __name__ == "__main__":
    main()
