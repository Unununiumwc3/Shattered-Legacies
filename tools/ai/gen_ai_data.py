"""Generates src/WarcraftLegacies.Source/Ai/AiData.g.cs -- hard-coded tables the bot AI reads at runtime:
  Trains       building type -> the unit types it trains (faction units with a category; no heroes, builders, ships)
  AltarHeroes  altar type    -> the hero types it trains
  Requires     unit/hero type -> its requirements ("ureq": researches like "Quest completed: X" or building types)
  HeroSkills   hero type     -> its skills in learning order: auras/passives first, then the rest in map order,
                                the attribute bonus (Aamk) last
Sources: WL's FactionObjectLimits/*ObjectInfo.cs (who owns what, categories) and mapdata UnitData/AbilityData.
At runtime the AI only checks what is possible NOW (requirements met via GetPlayerTechCount, gold).
0.39.10: the bot's file holds WL's factions only. Buildings of Shattered Legacies factions (SL_BUILDINGS, the Kezan
Enterprise) go to src/WarcraftLegacies.Source/ShatteredLegacies/SlAiData.g.cs -- only the entries they add or change
-- and ShatteredLegaciesSetup hands them to the bot (AiApi.AddProductionData). Together: the same tables as before.
Re-run after every WL update:   python3 tools/ai/gen_ai_data.py      (from the repo root)"""
import os, re, struct, sys, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import unit_scan as us

OUT = os.path.join(us.SRC, "WarcraftLegacies.Source", "Ai", "AiData.g.cs")
SL_OUT = os.path.join(us.SRC, "WarcraftLegacies.Source", "ShatteredLegacies", "SlAiData.g.cs")
# Shattered Legacies: buildings of fallback factions that belong to no WL faction (The Kezan Enterprise, 0.39.6/0.39.8)
SL_BUILDINGS = ["o03L", "o05T", "o03O", "o03S", "o03P"]
ABIL = os.path.join(us.ROOT, "mapdata", "WarcraftLegacies", "AbilityData")
BUILDING_CATS = {"TownHall", "Keep", "Castle", "GilneasManor", "Shipyard", "Lighthouse", "Farm", "Altar", "Specialist",
                 "SiegeWorkshop", "Barracks", "Research", "Magic", "Tower", "Shop", "Teleport", "FlyingBuilding"}
# auras and passives (base ability ids, incl. creep versions) -> learned first
FIRST = {"AHab", "AHad", "AOae", "AEah", "AEar", "AUau", "AUav", "AEev", "AOcr", "AHbh", "AUts", "ACac", "ACav",
         "ACba", "ACat", "ACua", "ACah", "ACae", "ACev", "ACct", "ACbh", "ACvs", "AOr2", "ANic"}
LAST = {"Aamk"}


def be(c):
    return struct.unpack(">I", c.encode("latin1"))[0]


def base_of(a):
    p = os.path.join(ABIL, a + ".json")
    if os.path.exists(p):
        return json.load(open(p, encoding="utf-8-sig")).get("OldId", a)
    return a


def ids(codes):
    return "new[] { " + ", ".join(str(be(c)) for c in codes) + " }"


def build(infos, extra_buildings):
    trains, altars, requires, skills = {}, {}, {}, {}
    owned_units = set()
    for fac, items in infos.items():
        faction_units, faction_heroes, buildings = set(), set(), []
        for const, v, cats in items:
            c = us.code(v)
            if us.is_research(c):
                continue
            if (cats and cats[0] in BUILDING_CATS) or c in us.VANILLA_TRAINS \
                    or (us.udata(c).get("_base") or "") in us.VANILLA_TRAINS:
                buildings.append(c)
            elif us.is_hero(c):
                faction_heroes.add(c)
            elif cats and cats[0] != "Builder":
                faction_units.add(c)
        owned_units |= faction_units | faction_heroes
        for b in buildings:
            t = us.trains(b) or []
            units = [u for u in t if u in faction_units]
            heroes = [u for u in t if u in faction_heroes]
            if units:
                trains.setdefault(b, [])
                trains[b] += [u for u in units if u not in trains[b]]
            if heroes:
                altars.setdefault(b, [])
                altars[b] += [h for h in heroes if h not in altars[b]]
    # extra buildings (Shattered Legacies factions): their full train lists (no builders), heroes into AltarHeroes
    for b in extra_buildings:
        for u in (us.trains(b) or []):
            base = us.udata(u).get("_base") or u
            if base in ("opeo", "hpea", "upeo", "ewsp"):
                continue
            target = altars if (us.is_hero(u) or us.is_hero(base)) else trains
            target.setdefault(b, [])
            if u not in target[b]:
                target[b].append(u)
            owned_units.add(u)
    for c in sorted(owned_units):
        r = us.udata(c).get("ureq")
        if isinstance(r, str) and r.strip():
            reqs = [x.strip() for x in r.split(",") if len(x.strip()) == 4]
            if reqs:
                requires[c] = reqs
        if us.is_hero(c):
            h = us.udata(c).get("uhab")
            if isinstance(h, str) and h.strip():
                abil = [a.strip() for a in h.split(",") if len(a.strip()) == 4]
                first = [a for a in abil if base_of(a) in FIRST]
                last = [a for a in abil if base_of(a) in LAST]
                mid = [a for a in abil if a not in first and a not in last]
                skills[c] = first + mid + last

    return trains, altars, requires, skills


def block(name, comment, d):
    lines = [f"    // {comment}", f"    internal static readonly Dictionary<int, int[]> {name} = new()", "    {"]
    for k in sorted(d):
        lines.append(f"      {{ {be(k)}, {ids(d[k])} }}, // {k}: {' '.join(d[k])}")
    lines.append("    };")
    return "\n".join(lines)


def render(namespace, cls, header, tables):
    t, a, r, s = tables
    return f"""// <auto-generated>
{header}
// DO NOT EDIT -- change tools/ai/gen_ai_data.py and re-run it.
// </auto-generated>
using System.Collections.Generic;

namespace {namespace}
{{
  internal static class {cls}
  {{
""" + "\n\n".join([
        block("Trains", "building type -> unit types it trains (faction units; no heroes, builders, ships)", t),
        block("AltarHeroes", "altar type -> hero types it trains", a),
        block("Requires", "unit/hero type -> requirements (research or unit type; met = GetPlayerTechCount >= 1)", r),
        block("HeroSkills", "hero type -> skills in learning order (auras/passives first, attribute bonus last)", s),
    ]) + "\n  }\n}\n"


def main():
    consts = us.load_consts()
    infos = us.object_infos(consts)
    wl = build(infos, [])
    full = build(infos, SL_BUILDINGS)
    # SL part = what the extra buildings add or change; merged over the bot's tables at runtime it gives `full`
    sl = tuple({k: v for k, v in f.items() if w.get(k) != v} for w, f in zip(wl, full))
    for w, f, d in zip(wl, full, sl):
        merged = dict(w)
        merged.update(d)
        assert merged == f
    out = render("WarcraftLegacies.Source.Ai", "AiData",
                 "// Generated by tools/ai/gen_ai_data.py from WL's FactionObjectLimits/*ObjectInfo.cs and the map data\n"
                 "// (UnitData utra / ureq / uhab, AbilityData). WL factions only.", wl)
    sl_out = render("WarcraftLegacies.Source.ShatteredLegacies", "SlAiData",
                    "// Generated by tools/ai/gen_ai_data.py: production data of Shattered Legacies factions (buildings in\n"
                    "// SL_BUILDINGS). Only the entries they add to or change in Ai/AiData.g.cs; merged at setup\n"
                    "// (ShatteredLegaciesSetup -> AiApi.AddProductionData).", sl)
    for path, text in ((OUT, out), (SL_OUT, sl_out)):
        open(path, "w", encoding="utf-8", newline="\n").write("\ufeff" + text)
        print(f"wrote {os.path.relpath(path, us.ROOT)}")
    t, a, r, s = full
    print(f"together: {len(t)} buildings, {len(a)} altars, {len(r)} requirement entries, {len(s)} heroes with skills")


if __name__ == "__main__":
    main()
