"""Three small conquerable fallback factions made from WL's creeps (0.85.0, hand-made, stock skills only).

Usage (repo root):  python3 tools/shattered-legacies/content/minor_factions.py
Writes per faction
  mapdata/WarcraftLegacies/UnitData/<id>.json                                   its units, buildings and heroes
  src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/<Name>ObjectInfo.cs   object limits (GENERATED)
  src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/Factions/<Name>/<Name>Objects.g.cs   the ids as constants

The Defias Brotherhood (Stormwind's fallback, ids ?Zp?): WL's bandit creeps as Defias units -- every one with
  Pillage -- and two heroes: Dunco Ratsbon (the cutthroat who wants the loot) and Count Clessington (the noble who
  wants Stormwind's estates). No buildings: recruits come every turn (code).
The Centaur Khans (the Tauren's fallback, ids ?Zl?): five khan heroes, one per clan; the centaur units are WL's creep
  centaurs, given every turn by code (no buildings train them).
The Furbolgs (the Druids' fallback, ids ?Zu?, 0.86): one hero, Nafien, and the Totem Lodge of Ursoc (his altar, the
  HQ); WL's creep furbolgs every turn, more and stronger for each world tree their side holds (code).
The Harpies (the Black Empire's fallback, ids ?Zj?): harpy units with a price, the Harpy Nest and the Harpy Roost
  (WL's preplaced nests at Stonetalon become these), two heroes. No tech.
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
SHARED = os.path.join(ROOT, "src", "WarcraftLegacies.Shared", "ShatteredLegacies", "FactionObjectLimits")
FACTIONS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Factions")
BTN = "ReplaceableTextures\\CommandButtons\\"


def s(fid, value):
    return {"Id": fid, "Type": 3, "Value": value}


def n(fid, value):
    return {"Id": fid, "Value": value}


def r(fid, value):
    return {"Id": fid, "Type": 1, "Value": value}


def main_mods(base):
    """the main map's changes of a stock unit (WL's creep stats), without the fields we set ourselves"""
    f = os.path.join(MAP, "UnitData", base + ".json")
    return json.load(open(f, encoding="utf-8"))["Modifications"] if os.path.exists(f) else []


def merged(base, mods):
    own = {m["Id"] for m in mods}
    return [m for m in main_mods(base) if m["Id"] not in own] + mods


def write(oid, base, mods):
    with open(os.path.join(MAP, "UnitData", oid + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps({"OldId": base, "NewId": oid, "Unk": [0], "Modifications": mods}, indent=2,
                           ensure_ascii=False))


def fourcc(code):
    return int.from_bytes(code.encode("latin1"), "big")


def hero(oid, base, title, proper, skills, model, icon, attr, suffix, story, extra=None, scale=1.0):
    mods = [s("unam", title), s("upro", proper), s("uhab", skills + ",Aamk"), s("uabi", "AInv"), s("unsf", suffix),
            n("ugol", 100), n("ulum", 0), s("ureq", ""), s("uico", BTN + icon), s("umdl", model),
            s("uawt", "Revive " + proper), s("utpr", "Revive " + proper), s("utip", "Summon " + proper),
            s("utub", story), s("upra", attr), r("usca", scale)]
    extra = extra or []
    own = {m["Id"] for m in extra}
    write(oid, base, [m for m in mods if m["Id"] not in own] + extra)


# ---------------------------------------------------------------- the Defias Brotherhood
DEFIAS_UNITS = [  # id, stock base, name, category, abilities of the main map's creep (+ Pillage)
    ("nZp0", "nban", "Defias Bandit", "Fighter", "Ashm"),
    ("nZp1", "nbrg", "Defias Brigand", "Marksman", "A0P0,Ashm"),
    ("nZp2", "nrog", "Defias Rogue", "Fighter", "ACct,Ashm"),
    ("nZp3", "nass", "Defias Assassin", "Marksman", "ACvs,Ashm"),
    ("nZp4", "nenf", "Defias Enforcer", "Tank", "A020,ACct,ACev"),
]
DEFIAS_HEROES = [
    ("OZp5", "Obla", "Kingpin of the Defias", "Dunco Ratsbon", "AEfk,AOwk,AOcr,AOmi",
     "units\\creeps\\BanditLord\\BanditLord", "BTNBanditLord.blp", "AGI", "Support",
     "A cutthroat who learned his trade in Westfall's ditches. He wants the loot -- every coin Stormwind's nobles ever "
     "kept from honest men. Fan of Knives, Wind Walk, Critical Strike; ultimate Mirror Image."),
    ("HZp6", "Hamg", "Patron of the Brotherhood", "Count Clessington", "ANsi,AHad,AOhx,ANch",
     "units\\creeps\\HumanMage\\HumanMage", "BTNBanditMage.blp", "INT", "Support",
     "A count without a county. The House of Nobles took his lands; the Brotherhood will take them back -- and every "
     "estate of the men who voted against him. Silence, Devotion Aura, Hex; ultimate Charm."),
]


def defias():
    for oid, base, name, _, abil in DEFIAS_UNITS:
        write(oid, base, merged(base, [s("unam", name), s("unsf", "(Defias)"), s("uabi", abil + ",Asal")]))
    for oid, base, title, proper, skills, model, icon, attr, _, story in DEFIAS_HEROES:
        hero(oid, base, title, proper, skills, model, icon, attr, "(Defias)", story,
             [s("uabi", "AInv,Asal")])
    limits = [(oid, 200, cat, name) for oid, _, name, cat, _ in DEFIAS_UNITS]
    limits += [(h[0], 1, h[8], h[3]) for h in DEFIAS_HEROES]
    consts = [(c, oid) for c, oid in zip(["Bandit", "Brigand", "Rogue", "Assassin", "Enforcer"],
                                         [u[0] for u in DEFIAS_UNITS])]
    consts += [("Ratsbon", "OZp5"), ("Clessington", "HZp6")]
    return "Defias", limits, consts


# ---------------------------------------------------------------- the Centaur Khans
# the five khans (warcraft.wiki.gg "khan": the clans' current khans); the clan's camp is in CentaurFallback.cs
KHANS = [
    ("OZl0", "Khan of the Kolkar", "Khan Leh'Prah", "AOsh,AOws,AOae,AOre", "STR",
     "The Kolkar ride the Barrens around Thunder Bluff -- the first to swear the Blood-Oath of the Hunt. Shockwave, "
     "War Stomp, Endurance Aura; ultimate Reincarnation."),
    ("OZl1", "Khan of the Galak", "Khan Ablinh", "AEar,AEev,ANba,ANst", "AGI",
     "The Galak archers of the southern Barrens and the Needles. Trueshot Aura, Evasion, Black Arrow; ultimate "
     "Stampede."),
    ("OZl2", "Khan of the Maraudine", "Khan Hratha", "AOcl,AOhx,ANht,AOvd", "INT",
     "The Maraudine, sons of Theradras, raid the coast of Durotar. Chain Lightning, Hex, Howl of Terror; ultimate Big "
     "Bad Voodoo."),
    ("OZl3", "Khan of the Magram", "Khan Kammah", "AHbh,AHtc,ANca,AHav", "STR",
     "The Magram rule the wastes of Desolace -- the strongest clan, by their own word. Bash, Thunder Clap, Cleaving "
     "Attack; ultimate Avatar."),
    ("OZl4", "Khan of the Gelkis", "Khan Shodo", "AEer,AOhw,AEah,AOeq", "INT",
     "The Gelkis earthcallers keep Theradras' old ways at the edge of Feralas. Entangling Roots, Healing Wave, Thorns "
     "Aura; ultimate Earthquake."),
]
CENTAUR_UNITS = [("ncen", "Fighter"), ("ncea", "Marksman"), ("ncim", "Marksman"), ("ncks", "Support")]


def centaur():
    for oid, title, proper, skills, attr, story in KHANS:
        hero(oid, "Otch", title, proper, skills, "units\\creeps\\CentaurKhan\\CentaurKhan", "BTNCentaurKhan.blp",
             attr, "(Centaur)", story, scale=1.15)
    limits = [(k[0], 1, "Support" if k[4] == "INT" else "Tank", k[2]) for k in KHANS]
    limits += [(u, 200, cat, u) for u, cat in CENTAUR_UNITS]
    consts = list(zip(["LehPrah", "Ablinh", "Hratha", "Kammah", "Shodo"], [k[0] for k in KHANS]))
    consts += list(zip(["Outrunner", "Archer", "Impaler", "Sorcerer"], [u for u, _ in CENTAUR_UNITS]))  # WL's creeps
    return "Centaur", limits, consts


# ---------------------------------------------------------------- the Harpies
HARPY_UNITS = [  # id, stock base, name, category, gold, food, build time
    ("nZj0", "nhar", "Harpy Scout", "Fighter", 120, 2, 20),
    ("nZj1", "nhrr", "Harpy Rogue", "Fighter", 160, 2, 24),
    ("nZj2", "nhrw", "Harpy Windwitch", "Support", 180, 2, 26),
    ("nZj3", "nhrh", "Harpy Storm-hag", "Support", 260, 3, 32),
]
HARPY_HEROES = [
    ("OZj6", "Obla", "Matriarch of the Bloodfeathers", "Serena Bloodfeather", "AEsh,AEev,AOcr,AOww",
     "units\\creeps\\HarpyQueen\\HarpyQueen", "BTNHarpyQueen.blp", "AGI", "Support",
     "Freed from the whispers of the deep, she follows the banner of whoever broke them. Shadow Strike, Evasion, "
     "Critical Strike; ultimate Bladestorm."),
    ("OZj7", "Ofar", "Storm Witch of the Bloodfury", "Sister Hatelash", "ANfl,ANsi,AUsl,ANto",
     "units\\creeps\\HarpyWitch\\HarpyWitch", "BTNHarpyWitch.blp", "INT", "Support",
     "The winds of Stonetalon answer her shriek. Forked Lightning, Silence, Sleep; ultimate Tornado."),
]


def harpies():
    for oid, base, name, _, gold, food, bt in HARPY_UNITS:
        write(oid, base, merged(base, [s("unam", name), s("unsf", "(Harpies)"), n("ugol", gold), n("ulum", 0),
                                       n("ufoo", food), n("ubld", bt)]))
    for oid, base, title, proper, skills, model, icon, attr, _, story in HARPY_HEROES:
        hero(oid, base, title, proper, skills, model, icon, attr, "(Harpies)", story,
             [s("umvt", "fly"), {"Id": "umvh", "Type": 2, "Value": 240}])
    write("nZj4", "nhns", merged("nhns", [s("unam", "Harpy Nest"), s("unsf", "(Harpies)"), s("utra", "nZj0,nZj1,nZj2"),
                                          n("ufma", 10), s("uico", BTN + "BTNHarpyQueen.blp"),
                                          s("utub", "A nest of the Stonetalon harpies. Trains Harpy Scouts, Rogues and "
                                                    "Windwitches; provides food.")]))
    write("nZj5", "nhns", merged("nhns", [s("unam", "Harpy Roost"), s("unsf", "(Harpies)"),
                                          s("utra", "nZj3,OZj6,OZj7"), n("ufma", 20), r("usca", 1.35),
                                          s("uico", BTN + "BTNHarpyQueen.blp"),
                                          s("utub", "The matriarchs' roost. Trains Storm-hags and the harpy heroes, "
                                                    "revives them; provides food.")]))
    limits = [(u[0], 200 if u[0] != "nZj3" else 6, u[3], u[2]) for u in HARPY_UNITS]
    limits += [("nZj4", 200, "Barracks", "Harpy Nest"), ("nZj5", 1, "Altar", "Harpy Roost")]
    limits += [(h[0], 1, h[8], h[3]) for h in HARPY_HEROES]
    consts = list(zip(["Scout", "Rogue", "Windwitch", "StormHag"], [u[0] for u in HARPY_UNITS]))
    consts += [("Nest", "nZj4"), ("Roost", "nZj5"), ("Serena", "OZj6"), ("Hatelash", "OZj7")]
    return "Harpy", limits, consts


# ---------------------------------------------------------------- the Furbolgs (0.86.0, the Druids' fallback, ?Zu?)
FURBOLG_UNITS = [("nfrl", "Fighter"), ("nfrs", "Support"), ("nfrb", "Marksman"), ("nfrg", "Tank"),
                 ("nfra", "Tank")]


def furbolgs():
    hero("OZu0", "Otch", "Totem-Keeper of the Timbermaw", "Nafien", "ANsg,AEer,AOhw,AEtq",
         "units\\creeps\\FurbolgElder\\FurbolgElder", "BTNFurbolgElder.blp", "STR", "(Furbolg)",
         "Keeper of the Sacred Ancestral Totem. The tribes follow whoever proves worthy of holding it. Summon Bear, "
         "Entangling Roots, Healing Wave; ultimate Tranquility.", scale=1.3)
    write("oZu1", "oalt", [s("unam", "Totem Lodge of Ursoc"), s("unsf", "(Furbolg)"), s("utra", "OZu0"),
                           n("ufma", 30), n("ugol", 0), n("ulum", 0), r("usca", 1.6),
                           s("umdl", "buildings\\other\\FurbolgHut2\\FurbolgHut2"),
                           s("upat", "PathTextures\\6x6SimpleSolid.tga"), s("uico", BTN + "BTNFurbolgElder.blp"),
                           s("utub", "The lodge of the Sacred Ancestral Totem. Revives the Totem-Keeper; it cannot "
                                     "be destroyed.")])
    limits = [("OZu0", 1, "Support", "Nafien"), ("oZu1", 1, "Altar", "Totem Lodge of Ursoc")]
    limits += [(u, 200, cat, u) for u, cat in FURBOLG_UNITS]
    consts = [("Nafien", "OZu0"), ("Lodge", "oZu1")]
    consts += list(zip(["Furbolg", "Shaman", "Tracker", "Champion", "UrsaWarrior"], [u for u, _ in FURBOLG_UNITS]))
    return "Furbolg", limits, consts


def write_info(name, limits):
    lines = ["// <auto-generated> tools/shattered-legacies/content/minor_factions.py -- object limits of the " + name
             + ".", "// Do not edit. </auto-generated>", "using MacroTools.Shared;",
             "using static MacroTools.Shared.UnitCategory;", "",
             "namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits", "{",
             "  public static class " + name + "ObjectInfo", "  {",
             "    public static IEnumerable<ObjectInfo> GetAllObjectLimits()", "    {"]
    for oid, limit, cat, label in limits:
        lines.append("      yield return new(%d, %d, %s); // %s %s" % (fourcc(oid), limit, cat, oid, label))
    lines += ["    }", "  }", "}", ""]
    with open(os.path.join(SHARED, name + "ObjectInfo.cs"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def write_consts(name, consts):
    folder = os.path.join(FACTIONS, name)
    os.makedirs(folder, exist_ok=True)
    lines = ["// <auto-generated> tools/shattered-legacies/content/minor_factions.py -- the " + name
             + " objects. Do not edit. </auto-generated>",
             "namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions." + name, "{",
             "  internal static class " + name + "Objects", "  {"]
    for const, oid in consts:
        lines.append("    internal const int %s = %d; // %s" % (const, fourcc(oid), oid))
    lines += ["  }", "}", ""]
    with open(os.path.join(folder, name + "Objects.g.cs"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    for make in (defias, centaur, harpies, furbolgs):
        name, limits, consts = make()
        write_info(name, limits)
        write_consts(name, consts)
        print("wrote", name, len(limits), "objects")
