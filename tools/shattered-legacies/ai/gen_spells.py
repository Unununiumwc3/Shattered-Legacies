"""Generates the bot's spell tables (0.70.0, AiSpells):
  src/WarcraftLegacies.Source/Ai/AiSpells.g.cs                      WL's Channel abilities (class AiSpellData)
  src/WarcraftLegacies.Source/ShatteredLegacies/Bot/SlAiSpells.g.cs  abilities only fork units carry (SlAiSpells;
                                                                    handed to the bot by SlBotSetup -> AiApi.AddSpellData)
  src/WarcraftLegacies.Source/Ai/AiAutocast.g.cs                    0.71.0: every autocast ability units carry and the
                                                                    order that switches it on (AiAutocastData, AUTOCAST)
0.71.0: spell_rules.txt may also list item-based no-target abilities (base AI..., no order string): order "" -> the
bot orders them by their ability id. A point / no-target Channel spell with a stock unit-target order is a PROBLEM.
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
AC_OUT = os.path.join(SRC, "WarcraftLegacies.Source", "Ai", "AiAutocast.g.cs")
UNIT_LOADER = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "UnitLoader.cs")
UNIT_ENUM = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "Enums", "UnitType.cs")
LOADER = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "AbilityLoader.cs")
ENUM = os.path.join(SRC, "War3Api.Object", "Generated", "1.36.0.20257", "Enums", "AbilityType.cs")

# 0.71.0: autocast abilities by base -> the order that turns autocast ON (computer players without a melee AI
# script never switch it on). Written by hand: War3Api's data leaves several "on" orders empty (bloodlust, cold
# arrows, faerie fire). Not here on purpose: toggles and forms (immolation, defend, divine shield, burrow, root,
# bear / raven / sphinx form, ...), mana shield (a toggle, not autocast), repair (workers), and Unstable
# Concoction (the unit blows itself up).
AUTOCAST = {
    "Ahea": "healon", "Anhe": "healon", "Anh1": "healon", "Anh2": "healon",
    "Ainf": "innerfireon", "ACif": "innerfireon",
    "Aslo": "slowon", "ACsw": "slowon",
    "Acrs": "curseon", "ACcs": "curseon",
    "Aadm": "autodispelon", "Andm": "autodispelon", "ACdm": "autodispelon", "ACd2": "autodispelon",
    "Arai": "raisedeadon", "ACrd": "raisedeadon",
    "AUfu": "frostarmoron", "ACf2": "frostarmoron", "ACfu": "frostarmoron",
    "ANba": "blackarrowon", "ACbk": "blackarrowon",
    "ANpa": "parasiteon", "ACpa": "parasiteon",
    "Aren": "renewon",
    "Arpb": "replenishon", "Arpl": "replenishlifeon", "Arpm": "replenishmanaon",
    "Arst": "restorationon",
    "Asps": "spellstealon",
    "Aweb": "webon", "ACwb": "webon",
    "Apsh": "phaseshifton",
    "AUcb": "carrionscarabson",
    "Avng": "vengeanceon",
    "AEpa": "poisonarrows",
    "AHfa": "flamingarrows", "ACsa": "flamingarrows",
    "AHca": "coldarrows", "ACcw": "coldarrows", "ANfa": "coldarrows",
    "Ablo": "bloodluston", "ACbl": "bloodluston", "ACbb": "bloodluston",
    "Afae": "faeriefireon", "Afa2": "faeriefireon", "ACff": "faeriefireon",
}

KINDS = {"unit": 1, "point": 2, "instant": 3}
WHO = {"enemy": 1, "ally": 2, "self": 3}
FLAGS = {"mana": 1, "hurt": 2}
GROUND, ORGANIC = 4, 8  # from the ability's targets allowed (atar, level 1): no "air" -> ground only; "organic"
# Channel defaults (WC3 1.36, War3Api LoadIllidanChannel): target type 0, order "channel", range 500, area 0
CHANNEL_DEFAULT = {"Ncl2": 0, "Ncl6": "channel", "aran": 500.0, "aare": 0.0}
# 0.71.0: order strings of stock UNIT-target spells. WC3 checks a script order against the stock spell that owns
# the order string: a point / no-target Channel ability with one of these is refused for every order (Muradin's
# Stormbolt "thunderbolt": 0 casts / 9 refused in the 0.70.3 playtest). Such a Channel ability needs another Ncl6.
UNIT_ORDERS = {"thunderbolt", "creepthunderbolt", "slow", "darkconversion", "firebolt", "hex", "polymorph",
               "entanglingroots", "frostnova", "deathcoil", "manaburn", "holybolt", "drain", "charm",
               "chainlightning", "forkedlightning", "shadowstrike", "banish", "doom", "faeriefire", "cripple",
               "curse", "purge", "sleep", "frostarmor", "innerfire", "bloodlust", "heal", "rejuvenation"}
# 0.72.0: order strings of stock NO-target spells -- a point Channel spell with one is treated the same (not proven
# refused, but the unit-target case was; the 0.72.0 kits got point orders instead: blizzard, earthquake, monsoon)
NOTARGET_ORDERS = {"tranquility", "roar", "thunderclap", "stomp", "starfall", "howlofterror", "berserk", "windwalk",
                   "immolation", "divineshield", "avatar", "metamorphosis", "fanofknives", "mirrorimage", "bladestorm",
                   "battleroar", "manashieldon"}
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


def stock_unit_abilities():
    """stock unit code -> (normal abilities, hero skills) from War3Api's UnitLoader (the defaults a map keeps
    unless it changes uabi / uhab)"""
    names = {name: code for code, name in re.findall(r"\('(\w{4})'\)\.</summary>\s*(\w+) =",
                                                     open(UNIT_ENUM, encoding="utf-8-sig").read())}
    out = {}
    text = open(UNIT_LOADER, encoding="utf-8-sig").read()
    for m in re.finditer(r"protected virtual \w+ Load(\w+)\(ObjectDatabaseBase db\)\s*\{(.*?)\n        \}", text, re.S):
        code = names.get(m.group(1))
        if not code:
            continue
        lists = []
        for key in ("AbilitiesNormalRaw", "AbilitiesHeroRaw"):
            o = re.search(r'unit\.' + key + r' = "([^"]*)"', m.group(2))
            lists.append([a.strip() for a in (o.group(1) if o else "").split(",") if len(a.strip()) == 4])
        out[code] = tuple(lists)
    return out


def all_unit_abilities():
    """every unit type of the map (stock and custom) -> abilities it carries (uabi + uhab, stock defaults where
    the map leaves the field unchanged)"""
    stock = stock_unit_abilities()
    out = {u: n + h for u, (n, h) in stock.items()}
    for f in glob.glob(os.path.join(MAPD, "UnitData", "*.json")):
        d = load_json(f)
        uid = os.path.basename(f)[:-5]
        n, h = stock.get(d.get("OldId", uid), ([], []))
        for m in d.get("Modifications", []):
            vals = [a.strip() for a in str(m["Value"]).split(",") if len(a.strip()) == 4]
            if m["Id"] == "uabi":
                n = vals
            elif m["Id"] == "uhab":
                h = vals
        out[uid] = n + h
    return out


def autocast_table(ab):
    """rows (ability id, on-order, [unit types]) for every autocast ability some unit type carries"""
    per_unit = all_unit_abilities()
    order = {}
    for a, base in AUTOCAST.items():
        order[a] = base
    for a, d in ab.items():
        base = d.get("OldId", a)
        if base in AUTOCAST:
            order[a] = AUTOCAST[base]
    rows = {}
    for uid in sorted(per_unit):
        for a in per_unit[uid]:
            if a in order:
                rows.setdefault(a, []).append(uid)
    return [(a, order[a], rows[a]) for a in sorted(rows)]


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
    per_unit = all_unit_abilities()  # 0.71.0: with stock defaults (clash check)
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
    # 0.71.0: item-based unit abilities (base AI..., no order string: ordered by their id) may have a row too --
    # no-target ones only (Storming Brew, Emergency Repairs, ...)
    items = sorted(a for a in rules if a in ab and ab[a].get("OldId", "").startswith("AI") and a in carry)
    for a in sorted(rules):
        if a not in ab or (ab[a].get("OldId") != "ANcl" and not ab[a].get("OldId", "").startswith("AI")):
            problems.append(f"spell_rules.txt lists {a}, which is not a Channel or item-based ability of the map")
        elif a not in carry:
            notes.append(f"{a}: listed, but no unit carries it")

    wl, sl = [], []
    for a in channel + items:
        r = rules.get(a)
        if r is None or r["kind"] == "skip":
            continue
        d = ab[a]
        name = str(field(d, "anam") or a)
        if a in items:
            if r["kind"] != "instant":
                problems.append(f"{a} {name}: item-based abilities are no-target only (rule says {r['kind']}) -- not cast")
                continue
            units = [u for u, _ in carry[a]]
            fork_only = all(u in sl_units and u not in wl_units for u in units)
            row = (a, KINDS["instant"], WHO[r["who"]], r["min"], r["radius"], r["flags"], "",
                   f"{name} -- instant {r['who']} (item-based {d.get('OldId')}); on {' '.join(units[:4])}"
                   f"{' ...' if len(units) > 4 else ''}", units)
            (sl if fork_only else wl).append(row)
            continue
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
        if r["kind"] == "point" and order in NOTARGET_ORDERS:
            problems.append(f"{a} {name}: point spell with the no-target order \"{order}\" -- give Ncl6 a point order -- not cast")
            continue
        if r["kind"] != "unit" and order in UNIT_ORDERS:
            problems.append(f"{a} {name}: {r['kind']} spell with the unit-target order \"{order}\" -- WC3 refuses "
                            f"it; give Ncl6 a point / no-target order -- not cast")
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
    print(f"{len(channel)} carried Channel abilities + {len(items)} item-based: {len(wl)} cast by the bot (WL), {len(sl)} (fork), "
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
                "    // order string (Channel: Ncl6); \"\" = item-based, ordered by the ability id\n"
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
    ac = autocast_table(ab)

    def arr_ac(t, vals):
        per = 6 if t == "string" else 10
        chunks = [", ".join(vals[i:i + per]) for i in range(0, len(vals), per)]
        return "new " + t + "[]\n    {\n      " + ",\n      ".join(chunks) + ",\n    }"

    ac_lines = [f"  //   {i:3d} {a} {o} -- {str(field(ab[a], 'anam') or '') if a in ab else 'stock'}; "
                f"on {' '.join(us[:4])}{' ...' if len(us) > 4 else ''}" for i, (a, o, us) in enumerate(ac)]
    ac_out = ("// <auto-generated>\n"
              "// Generated by tools/shattered-legacies/ai/gen_spells.py (0.71.0) from the map data: every autocast\n"
              "// ability a unit type carries (custom ones by their base) and the order that turns autocast on.\n"
              "// Read by AiAutocast. DO NOT EDIT -- change AUTOCAST in gen_spells.py and re-run it.\n"
              "// </auto-generated>\n"
              "namespace WarcraftLegacies.Source.Ai\n{\n"
              "  // One column per array, one row per ability:\n" + "\n".join(ac_lines) + "\n"
              "  internal static class AiAutocastData\n  {\n"
              "    // ability id\n"
              f"    internal static int[] Id = {arr_ac('int', [str(be(a)) for a, _, _ in ac])};\n\n"
              "    // order string that turns autocast on\n"
              f"    internal static string[] Order = {arr_ac('string', [json.dumps(o) for _, o, _ in ac])};\n\n"
              "    // who carries what: unit type -> row (one entry per pair)\n"
              f"    internal static int[] CarrierType = {arr_ac('int', [str(be(u)) for _, _, us in ac for u in us])};\n\n"
              f"    internal static int[] CarrierRow = {arr_ac('int', [str(i) for i, (_, _, us) in enumerate(ac) for u in us])};\n"
              "  }\n}\n")
    print(f"{len(ac)} autocast abilities on {len({u for _, _, us in ac for u in us})} unit types")
    for path, text in ((OUT, out), (SL_OUT, sl_out), (AC_OUT, ac_out)):
        open(path, "w", encoding="utf-8", newline="\n").write(text)
        print(f"wrote {os.path.relpath(path, ROOT)}")


if __name__ == "__main__":
    main()
