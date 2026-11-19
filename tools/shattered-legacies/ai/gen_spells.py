"""Generates the bot's spell tables (0.70.0, AiSpells):
  src/WarcraftLegacies.Source/Ai/AiSpells.g.cs                      WL's Channel abilities (class AiSpellData)
  src/WarcraftLegacies.Source/ShatteredLegacies/Bot/SlAiSpells.g.cs  abilities only fork units carry (SlAiSpells;
                                                                    handed to the bot by SlBotSetup -> AiApi.AddSpellData)
Why only Channel (ANcl): WC3's computer AI casts abilities by their BASE ability -- a custom spell built on a stock
ability (Storm Bolt, Blizzard, ...) is already cast by the engine. Channel is a dummy the engine never casts; the bot
orders it with the order string from the ability's data (Ncl6, the only order string WC3 lets a map change).

Sources: mapdata AbilityData (Channel data, defaults from WC3 1.36), UnitData (uabi / uhab: who carries what),
WL's FactionObjectLimits (WL units) and the fork's (fork units), the C# source (is a Spell registered for the id?),
War3Api's AbilityLoader (stock order strings: two abilities of one unit with the same order clash), and the reviewed
rules in tools/shattered-legacies/ai/spell_rules.txt (kind, enemy / ally, area -- every carried Channel ability
needs a row; unlisted ones are never cast).

  python3 tools/shattered-legacies/ai/gen_spells.py           (from the repo root)
  python3 tools/shattered-legacies/ai/gen_spells.py --check   only report, write nothing (exit 1 on a problem)"""
import glob, json, os, re, struct, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
SRC = os.path.join(ROOT, "src")
MAPD = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
RULES = os.path.join(HERE, "spell_rules.txt")
OUT = os.path.join(SRC, "WarcraftLegacies.Source", "Ai", "AiSpells.g.cs")
SL_OUT = os.path.join(SRC, "WarcraftLegacies.Source", "ShatteredLegacies", "Bot", "SlAiSpells.g.cs")
LOADER = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "AbilityLoader.cs")
ENUM = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "Enums", "AbilityType.cs")

KINDS = {"unit": 1, "point": 2, "instant": 3}
WHO = {"enemy": 1, "ally": 2, "self": 3}
FLAGS = {"mana": 1, "hurt": 2}
GROUND, ORGANIC = 4, 8  # from the ability's targets allowed (atar, level 1): no "air" -> ground only; "organic"
# Channel defaults (WC3 1.36, War3Api LoadIllidanChannel): target type 0, order "channel", range 500, area 0
CHANNEL_DEFAULT = {"Ncl2": 0, "Ncl6": "channel", "aran": 500.0, "aare": 0.0}
# target type (Ncl2) each kind accepts: 0 no target, 1 unit, 2 point, 3 unit or point
NCL2_OK = {"unit": (1, 3), "point": (2, 3), "instant": (0,)}


def be(c):
    return struct.unpack(">I", c.encode("latin1"))[0]


def load_json(path):
    return json.load(open(path, encoding="utf-8-sig"))


def abilities():
    out = {}
    for f in glob.glob(os.path.join(MAPD, "AbilityData", "*.json")):
        out[os.path.basename(f)[:-5]] = load_json(f)
    return out


def field(d, key, level=1):
    """value of a (levelled) field: the entry for `level`, else a level-less entry, else None"""
    plain = None
    for m in d.get("Modifications", []):
        if m["Id"] != key:
            continue
        lv = m.get("Level", 0)
        if lv == level:
            return m["Value"]
        if lv == 0:
            plain = m["Value"]
    return plain


def levels(d):
    v = field(d, "alev")
    return int(v) if v else 3


HERO_SKILLS = set()  # abilities some hero learns (uhab): every level counts; unit abilities only use level 1


def carriers():
    """ability id -> [(unit id, unit name)] from uabi / uhab"""
    out = {}
    for f in sorted(glob.glob(os.path.join(MAPD, "UnitData", "*.json"))):
        d = load_json(f)
        uid = os.path.basename(f)[:-5]
        name = ""
        for m in d.get("Modifications", []):
            if m["Id"] == "unam":
                name = m["Value"]
        for m in d.get("Modifications", []):
            if m["Id"] in ("uabi", "uhab"):
                for a in str(m["Value"]).split(","):
                    a = a.strip()
                    if len(a) == 4:
                        if m["Id"] == "uhab":
                            HERO_SKILLS.add(a)
                        out.setdefault(a, [])
                        if (uid, name) not in out[a]:
                            out[a].append((uid, name))
    return out


def unit_lists():
    """unit ids per order string: {unit: [ability ids]} for every carried ability"""
    out = {}
    for f in glob.glob(os.path.join(MAPD, "UnitData", "*.json")):
        d = load_json(f)
        uid = os.path.basename(f)[:-5]
        for m in d.get("Modifications", []):
            if m["Id"] in ("uabi", "uhab"):
                out.setdefault(uid, [])
                out[uid] += [a.strip() for a in str(m["Value"]).split(",") if len(a.strip()) == 4]
    return out


def stock_orders():
    """stock ability code -> its order string (War3Api's AbilityLoader: TextOrderStringUseTurnOnRaw)"""
    names = {name: code for code, name in re.findall(r"\('(\w{4})'\)\.</summary>\s*(\w+) =",
                                                     open(ENUM, encoding="utf-8-sig").read())}
    out = {}
    text = open(LOADER, encoding="utf-8-sig").read()
    for m in re.finditer(r"protected virtual \w+ Load(\w+)\(ObjectDatabaseBase db\)\s*\{(.*?)\n        \}", text, re.S):
        code = names.get(m.group(1))
        o = re.search(r'TextOrderStringUseTurnOnRaw = "([^"]*)"', m.group(2))
        if code and o and o.group(1):
            out[code] = o.group(1)
    return out


def object_units(folder):
    """unit ids listed in the FactionObjectLimits of a folder (big-endian constants -> ids)"""
    consts = {}
    for f in glob.glob(os.path.join(SRC, "WarcraftLegacies.Shared", "**", "*.g.cs"), recursive=True):
        for name, val in re.findall(r"public const int (\w+) = (-?\d+);", open(f, encoding="utf-8-sig").read()):
            consts[name] = int(val)
    units = set()
    for f in glob.glob(os.path.join(folder, "*.cs")):
        s = open(f, encoding="utf-8-sig").read()
        for name in re.findall(r"yield return new\(\s*(-?\w+)", s):
            val = int(name) if re.fullmatch(r"-?\d+", name) else consts.get(name)
            if val is not None:
                units.add(struct.pack(">I", val & 0xFFFFFFFF).decode("latin1"))
    return units


def code_refs():
    """ability ids that some C# outside Ai/ references (a registered Spell or another use)"""
    text = []
    for f in glob.glob(os.path.join(SRC, "**", "*.cs"), recursive=True):
        n = f.replace("\\", "/")
        if "/Ai/" in n or n.endswith(".g.cs") or "/War3Api.Object/" in n:
            continue
        text.append(open(f, encoding="utf-8-sig", errors="replace").read())
    return "\n".join(text)


def read_rules():
    rows = {}
    for ln, line in enumerate(open(RULES, encoding="utf-8"), 1):
        line = line.rstrip("\n")
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        p = line.split(None, 6)
        if len(p) < 6:
            raise SystemExit(f"spell_rules.txt:{ln}: needs id kind who min radius flags [note]")
        aid, kind, who, mn, rad, flags = p[:6]
        note = p[6] if len(p) > 6 else ""
        if kind not in KINDS and kind != "skip":
            raise SystemExit(f"spell_rules.txt:{ln}: unknown kind {kind}")
        if kind != "skip" and who not in WHO:
            raise SystemExit(f"spell_rules.txt:{ln}: unknown who {who}")
        if aid in rows:
            raise SystemExit(f"spell_rules.txt:{ln}: {aid} listed twice")
        fl = 0
        if flags != "-":
            for x in flags.split(","):
                if x not in FLAGS:
                    raise SystemExit(f"spell_rules.txt:{ln}: unknown flag {x}")
                fl |= FLAGS[x]
        rows[aid] = dict(kind=kind, who=who, min=0 if mn == "-" else int(mn), radius=0 if rad == "-" else int(rad),
                         flags=fl, note=note)
    return rows


def main():
    check_only = "--check" in sys.argv
    ab = abilities()
    carry = carriers()
    per_unit = unit_lists()
    stock = stock_orders()
    rules = read_rules()
    refs = code_refs()
    wl_units = object_units(os.path.join(SRC, "WarcraftLegacies.Shared", "FactionObjectLimits"))
    sl_units = object_units(os.path.join(SRC, "WarcraftLegacies.Shared", "ShatteredLegacies", "FactionObjectLimits"))
    problems, notes = [], []

    def order_of(a):
        d = ab.get(a)
        base = d.get("OldId", a) if d else a
        if base == "ANcl":
            return str(field(d, "Ncl6") or CHANNEL_DEFAULT["Ncl6"])
        return stock.get(base, "")

    channel = sorted(a for a, d in ab.items() if d.get("OldId") == "ANcl" and a in carry)
    for a in channel:
        if a not in rules:
            problems.append(f"{a} ({field(ab[a], 'anam')}) is carried by {', '.join(u for u, _ in carry[a][:3])} "
                            f"but has no row in spell_rules.txt -- never cast until reviewed")
    for a in sorted(rules):
        if a not in ab or ab[a].get("OldId") != "ANcl":
            problems.append(f"spell_rules.txt lists {a}, which is not a Channel ability of the map")
        elif a not in carry:
            notes.append(f"{a}: listed, but no unit carries it")

    wl, sl = [], []
    for a in channel:
        r = rules.get(a)
        if r is None or r["kind"] == "skip":
            continue
        d = ab[a]
        name = str(field(d, "anam") or a)
        orders = {str(field(d, "Ncl6", lv) or CHANNEL_DEFAULT["Ncl6"]) for lv in range(1, (levels(d) if a in HERO_SKILLS else 1) + 1)}
        if len(orders) > 1:
            problems.append(f"{a} {name}: the order string changes with the level ({sorted(orders)}) -- not cast")
            continue
        order = orders.pop()
        ncl2 = field(d, "Ncl2")
        ncl2 = CHANNEL_DEFAULT["Ncl2"] if ncl2 is None else int(ncl2)
        if ncl2 not in NCL2_OK[r["kind"]]:
            problems.append(f"{a} {name}: rule says {r['kind']} but the target type (Ncl2) is {ncl2} -- not cast")
            continue
        U = a.upper()
        if not re.search(r"ABILITY_" + U + r"_\w+", refs) and str(be(a)) not in refs:
            problems.append(f"{a} {name}: rule says {r['kind']} but no C# outside Ai/ uses it (no effect?) -- not cast")
            continue
        clash = []
        for uid, _ in carry[a]:
            for other in per_unit.get(uid, []):
                if other != a and order_of(other) == order:
                    clash.append(f"{uid}:{other}")
        if clash:
            problems.append(f"{a} {name}: order \"{order}\" also used by {', '.join(clash)} -- not cast")
            continue
        units = [u for u, _ in carry[a]]
        fork_only = all(u in sl_units and u not in wl_units for u in units)
        flags = r["flags"]
        atar = [x.strip() for x in str(field(d, "atar") or "").split(",") if x.strip()]
        if r["kind"] == "unit" and atar:
            if "air" not in atar:
                flags |= GROUND
            if "organic" in atar:
                flags |= ORGANIC
        row = (a, KINDS[r["kind"]], WHO[r["who"]], r["min"], r["radius"], flags, order,
               f"{name} -- {r['kind']} {r['who']}; on {' '.join(units[:4])}{' ...' if len(units) > 4 else ''}",
               units)
        (sl if fork_only else wl).append(row)

    for n in notes:
        print("note:", n)
    for p in problems:
        print("PROBLEM:", p)
    print(f"{len(channel)} carried Channel abilities: {len(wl)} cast by the bot (WL), {len(sl)} (fork), "
          f"{sum(1 for a in channel if rules.get(a, {}).get('kind') == 'skip')} skipped by rule")
    if check_only:
        sys.exit(1 if problems else 0)

    def render(namespace, cls, header, rows):
        def arr(t, vals):
            if not vals:
                return "new " + t + "[0]"
            per = 6 if t == "string" else 10
            chunks = [", ".join(vals[i:i + per]) for i in range(0, len(vals), per)]
            return "new " + t + "[]\n    {\n      " + ",\n      ".join(chunks) + ",\n    }"

        lines = [f"  //   {i:2d} {r[0]} {r[7]}" for i, r in enumerate(rows)] or ["  //   (none)"]
        return ("// <auto-generated>\n" + header + "\n"
                "// DO NOT EDIT -- change tools/shattered-legacies/ai/spell_rules.txt and re-run gen_spells.py.\n"
                "// </auto-generated>\n"
                f"namespace {namespace}\n{{\n"
                "  // One column per array, one row per ability:\n" + "\n".join(lines) + "\n"
                f"  internal static class {cls}\n  {{\n"
                "    // ability id\n"
                f"    internal static int[] Id = {arr('int', [str(be(r[0])) for r in rows])};\n\n"
                "    // 1 = unit target, 2 = point target, 3 = no target\n"
                f"    internal static int[] Kind = {arr('int', [str(r[1]) for r in rows])};\n\n"
                "    // 1 = enemy, 2 = ally, 3 = self\n"
                f"    internal static int[] Who = {arr('int', [str(r[2]) for r in rows])};\n\n"
                "    // least worth inside the area (unit 1, hero 3); 0 = default\n"
                f"    internal static int[] Min = {arr('int', [str(r[3]) for r in rows])};\n\n"
                "    // area used to count targets; 0 = the ability's own area of effect\n"
                f"    internal static int[] Radius = {arr('int', [str(r[4]) for r in rows])};\n\n"
                "    // 1 = prefer targets with mana, 2 = ally target only below 70 % life, 4 = ground targets only,\n"
                "    // 8 = organic targets only (4 / 8 from the ability's targets allowed)\n"
                f"    internal static int[] Flags = {arr('int', [str(r[5]) for r in rows])};\n\n"
                "    // order string (Channel: Ncl6)\n"
                f"    internal static string[] Order = {arr('string', [json.dumps(r[6]) for r in rows])};\n\n"
                "    // who carries what: unit type -> row (one entry per pair)\n"
                f"    internal static int[] CarrierType = {arr('int', [str(be(u)) for r in rows for u in r[8]])};\n\n"
                f"    internal static int[] CarrierRow = {arr('int', [str(i) for i, r in enumerate(rows) for u in r[8]])};\n"
                "  }\n}\n")

    out = render("WarcraftLegacies.Source.Ai", "AiSpellData",
                 "// Generated by tools/shattered-legacies/ai/gen_spells.py from the map data (Channel abilities units carry)\n"
                 "// and spell_rules.txt (reviewed). WL's abilities only; read by AiSpells.", wl)
    sl_out = render("WarcraftLegacies.Source.ShatteredLegacies.Bot", "SlAiSpells",
                    "// Generated by tools/shattered-legacies/ai/gen_spells.py: Channel abilities that only Shattered Legacies\n"
                    "// units carry. Handed to the bot at setup (SlBotSetup -> AiApi.AddSpellData).", sl)
    for path, text in ((OUT, out), (SL_OUT, sl_out)):
        open(path, "w", encoding="utf-8", newline="\n").write(text)
        print(f"wrote {os.path.relpath(path, ROOT)}")


if __name__ == "__main__":
    main()
