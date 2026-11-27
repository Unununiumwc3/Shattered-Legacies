"""The hand-made objects of 0.88.0 (agreed 2026-11-27). Run AFTER import_24p.py (it edits two of its copies).

Usage (repo root):  python3 tools/shattered-legacies/content/content_088.py
Writes
  mapdata/WarcraftLegacies/{UnitData,AbilityData,ItemData,BuffData,UpgradeData}/<id>.json   the objects below
  mapdata/WarcraftLegacies/UnitData/UZn0.json, EZn1.json   Kalecgos' two forms (the 24p copies, edited)
  src/WarcraftLegacies.Source/ShatteredLegacies/Events/Objects088.g.cs   the ids as constants

Ids: ?Zq? -- kept in content_088_ids.json, a new name gets the next free id (never one an object file already has).
- Jaina the Dreadlord (Theramore held by an evil faction: Mal'Ganis' kit and the stock dreadlord model)
- Kalecgos: human form = the 24p's Kalecgos (UZn0, caster), dragon form = the 24p's dragon form (EZn1: the azure
  dragon, bigger, more attack damage, Frost Breath and Devour Magic); "Dragon Form" (Bear Form base) swaps them, the
  same hero skills in both forms, the same primary attribute
- Xavius and Peroth'arn (demi-heroes of the Furbolgs), the Claw of Ursoc (artifact), Benediction (Majordomo's drop)
- Snarlmane (a Blackmane gnoll boss with an artifact; Mmmrrrggglll is the 24p's, imported)
- the Broodmother (the harpies' seat, a caged hostage that cannot move)
- Acherus in flight (a flying transport) with Remorseless Winter, Army of the Dead and Death's Advance; the research
  "The Ebon Hold Rises" and its condition
- the plague boars, the zombie plague (a dispellable debuff) and Amnennar the Coldbringer
- the two harbour docks (waygates) of the Kalimdor <-> Northrend crossing
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
CS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Events", "Objects088.g.cs")
IDS = os.path.join(HERE, "content_088_ids.json")
BTN = "ReplaceableTextures\\CommandButtons\\"
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"
KIND_DIR = {"unit": "UnitData", "ability": "AbilityData", "item": "ItemData", "buff": "BuffData",
            "upgrade": "UpgradeData"}


def s(fid, value, level=None, pointer=None):
    return mod(fid, value, 3, level, pointer)


def n(fid, value, level=None, pointer=None):
    return mod(fid, value, None, level, pointer)


def r(fid, value, level=None, pointer=None):
    return mod(fid, value, 2, level, pointer)


def real(fid, value):
    return mod(fid, value, 1, None, None)


def mod(fid, value, typ, level, pointer):
    m = {}
    if level is not None:
        m["Level"] = level
    if pointer is not None:
        m["Pointer"] = pointer
    m["Id"] = fid
    if typ is not None:
        m["Type"] = typ
    m["Value"] = value
    return m


def load(kind, oid):
    f = os.path.join(MAP, KIND_DIR[kind], oid + ".json")
    return json.load(open(f, encoding="utf-8-sig")) if os.path.exists(f) else None


def mods_of(kind, oid):
    d = load(kind, oid)
    return d["Modifications"] if d else []


def merged(base_mods, mods):
    own = {(m["Id"], m.get("Level")) for m in mods}
    return [m for m in base_mods if (m["Id"], m.get("Level")) not in own] + mods


def write(kind, oid, base, mods):
    with open(os.path.join(MAP, KIND_DIR[kind], oid + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps({"OldId": base, "NewId": oid, "Unk": [0], "Modifications": mods}, indent=2,
                           ensure_ascii=False))


class Ids:
    def __init__(self):
        self.ids = json.load(open(IDS, encoding="utf-8")) if os.path.exists(IDS) else {}
        self.used = []

    def get(self, name, first, kind):
        if name not in self.ids:
            taken = {f[:4].lower() for d in KIND_DIR.values() for f in os.listdir(os.path.join(MAP, d))}
            taken |= {v.lower() for v in self.ids.values()}
            self.ids[name] = next(first + "Zq" + c for c in SEQ if (first + "Zq" + c).lower() not in taken)
        self.used.append((name, self.ids[name], kind))
        return self.ids[name]

    def save(self):
        with open(IDS, "w", encoding="utf-8", newline="\n") as f:
            f.write(json.dumps(self.ids, indent=2, sort_keys=True) + "\n")


def fourcc(code):
    return int.from_bytes(code.encode("latin1"), "big")


def main():
    ids = Ids()
    I = lambda name, first, kind: ids.get(name, first, kind)

    # ------------------------------------------------------------ Jaina the Dreadlord (Theramore)
    jd = I("JainaDreadlord", "H", "unit")
    write("unit", jd, "Hjai", merged(mods_of("unit", "Hjai"), [
        s("unam", "Dreadlord of Theramore"), s("upro", "Jaina Proudmoore"), s("unsf", "(Theramore, evil)"),
        s("umdl", "units\\undead\\HeroDreadLord\\HeroDreadLord.mdl"), s("uico", BTN + "BTNHeroDreadLord.blp"),
        s("uhab", "Aamk,VP05,AUcs,AHab,SNin"), s("uabi", "A07B,VP02,AInv"), real("usca", 1.0),
        s("utub", "Theramore serves a dark master now -- and Jaina Proudmoore was never what she seemed. Garithos "
                  "always said so.|n|n|cffffcc00Jaina again when Jaina's Sanctum is held by a faction that is not "
                  "evil.|r")]))

    # ------------------------------------------------------------ Kalecgos (Dalaran, quest "The Nexus")
    human, dragon = "UZn0", "EZn1"
    form = I("KalecgosDragonForm", "A", "ability")
    write("ability", form, "Abrf", [
        r("acdn", 30, 1), n("amcs", 0, 1), s("Eme1", human, 1, 1), r("Eme4", 1.5, 1, 4), r("adur", 1.5, 1),
        s("Emeu", dragon, 1), s("areq", ""), s("arqa", ""), s("aart", BTN + "BTNAzureDragon.blp"),
        s("auar", BTN + "BTNKalecgos.blp"), s("ansf", "(Kalecgos)"), s("ahky", "D"), s("auhk", "D"),
        s("anam", "Dragon Form"), s("atp1", "Dragon Form", 1),
        s("aub1", "Kalecgos takes his true shape: a great blue dragon that fights in melee with much heavier blows, "
                  "Frost Breath and Devour Magic.", 1),
        s("aut1", "Mortal Form", 1),
        s("auu1", "Kalecgos takes his mortal shape again: a caster.", 1)])
    hum = load("unit", human)
    skills = next(m["Value"] for m in hum["Modifications"] if m["Id"] == "uhab")
    skills = ",".join(a for a in skills.split(",") if a not in ("AZn1",))  # the 24p's timed Metamorphosis goes
    if "AHbz" not in skills:
        skills = skills.replace("Aamk", "AHbz,Aamk")  # an ultimate-free fourth skill: Blizzard
    write("unit", human, hum["OldId"], merged(hum["Modifications"], [
        s("uhab", skills), s("uabi", "Alit," + form + ",AInv"), s("upra", "INT"), s("unam", "Aspect of Magic"),
        s("upro", "Kalecgos"), s("unsf", "(Dalaran)"), s("ureq", ""),
        s("utub", "The blue dragon who walks among mortals.|n|cfff5962dDragon Form:|r a great azure dragon that "
                  "fights in melee.")]))
    drg = load("unit", dragon)
    write("unit", dragon, drg["OldId"], merged(drg["Modifications"], [
        s("uhab", skills), s("uabi", "A09X,Advm," + form + ",AInv"), s("upra", "INT"), real("usca", 1.7),
        s("unam", "Aspect of Magic (Dragon Form)"), s("upro", "Kalecgos"), s("unsf", "(Dalaran)"),
        n("ua1b", 45), n("ua1d", 3), n("ua1s", 8), s("ua1w", "normal"), n("ua1r", 128), s("ureq", "")]))

    # ------------------------------------------------------------ Furbolgs: Xavius, Peroth'arn, the Claw of Ursoc
    xav = I("Xavius", "n", "unit")
    write("unit", xav, "nsth", [
        s("unam", "Xavius"), s("upro", "Xavius"), s("unsf", "(Lord of the Satyrs)"),
        s("umdl", "war3mapImported\\SatyrHeadhunter.mdl"), s("uico", BTN + "BTNHeroXaviusSatyr.blp"),
        real("usca", 1.5), n("uhpm", 2600), n("umpm", 600), n("udef", 6), n("ua1b", 62), n("ua1d", 2), n("ua1s", 12),
        r("ua1c", 1.4), n("ufoo", 0), n("ugol", 0), n("ulum", 0), n("ulev", 10), s("uabi", "AInv,ACrk,ACcr,ACsl"),
        n("ubba", 300), n("ubdi", 0), n("ubsi", 0),
        s("utub", "The first satyr, Lord of the Nightmare. He holds the Claw of Ursoc -- while he lives, or while the "
                  "Furbolgs keep the Claw, they are their own masters.")])
    per = I("Perotharn", "n", "unit")
    write("unit", per, "nsth", [
        s("unam", "Peroth'arn"), s("upro", "Peroth'arn"), s("unsf", "(Satyr lord)"), real("usca", 1.6),
        n("uhpm", 2100), n("umpm", 500), n("udef", 5), n("ua1b", 52), n("ua1d", 2), n("ua1s", 10), n("ufoo", 0),
        n("ugol", 0), n("ulum", 0), n("ulev", 9), s("uabi", "ACrk,ACsl"), n("ubba", 250), n("ubdi", 0), n("ubsi", 0),
        s("utub", "Xavius' first follower, a satyr lord of the Nightmare.")])
    claw = I("ClawOfUrsoc", "I", "item")
    write("item", claw, "ckng", merged(mods_of("item", "I00I"), [
        s("iabi", "A0AL,A0MC"), s("unam", "Claw of Ursoc"), s("iico", BTN + "BTNClawsOfAttack.blp"),
        s("ides", "The furbolgs' sacred totem. Increases Strength; attacks can bash."),
        s("utub", "|cffff8c00Artifact|r|nThe Sacred Ancestral Totem of the furbolgs. Increases Strength by 10; "
                  "attacks can bash.|n|n|cffffcc00Its holder is the master of the Furbolgs.|r")]))

    # ------------------------------------------------------------ Majordomo's drop: Benediction
    ben = I("Benediction", "I", "item")
    write("item", ben, "ckng", merged(mods_of("item", "I00I"), [
        s("iabi", "A0BF,A03Y"), s("unam", "Benediction"), s("iico", BTN + "BTNStaffOfSanctuary.blp"),
        s("ides", "The Eye of Divinity, from Majordomo's Cache of the Firelord. Increases Intelligence; heals."),
        s("utub", "|cffff8c00Artifact|r|nThe Eye of Divinity, kept in Majordomo Executus' Cache of the Firelord, "
                  "awakened in its holy form. Increases Intelligence by 8. Can cast a heal on all nearby friendly "
                  "units (life, mana and armour).")]))

    # ------------------------------------------------------------ Snarlmane (Blackmane gnolls)
    snarl = I("Snarlmane", "n", "unit")
    write("unit", snarl, "ngos", merged(mods_of("unit", "ngos"), [
        s("unam", "Snarlmane"), s("upro", "Snarlmane"), s("unsf", "(Blackmane chief)"), real("usca", 1.45),
        n("uhpm", 1500), n("ua1b", 40), n("udef", 4), n("ulev", 8), s("uabi", "AInv,ACrk,ACbh"),
        s("utub", "Chief of the Blackmane gnolls. He guards a stolen relic.")]))

    # ------------------------------------------------------------ the Broodmother (the harpies' seat)
    brood = I("Broodmother", "n", "unit")
    write("unit", brood, "nhns", merged(mods_of("unit", "nhns"), [
        s("unam", "The Broodmother"), s("upro", "The Broodmother"), s("unsf", "(Harpies' seat)"),
        s("umdl", "units\\undead\\MeatWagon\\MeatWagon.mdl"), s("uico", BTN + "BTNMeatWagon.blp"), real("usca", 1.3),
        n("uhpm", 3000), n("udef", 5), n("ufoo", 0), n("ugol", 0), n("ulum", 0), s("utra", ""),
        s("utub", "The harpies' broodmother, kept caged by whoever conquered her. The harpies serve whoever holds "
                  "her: bring her to 0 life to take her.")]))

    # ------------------------------------------------------------ Acherus in flight
    winter = I("RemorselessWinter", "A", "ability")
    write("ability", winter, "AUdd", merged(mods_of("ability", "AUdd"), [
        n("aher", 0), n("alev", 1), n("arlv", 0), s("anam", "Remorseless Winter"), s("ansf", "(Acherus)"),
        s("atp1", "Remorseless Winter", 1), s("aub1", "A storm of frost and death falls on the ground below "
                                                     "Acherus: units and buildings there take damage.", 1),
        r("acdn", 90, 1), n("amcs", 0, 1), r("aare", 450, 1), r("adur", 20, 1), s("aart", BTN + "BTNDeathAndDecay.blp")]))
    army = I("ArmyOfTheDead", "A", "ability")
    write("ability", army, "AUan", merged(mods_of("ability", "AUan"), [
        n("aher", 0), n("alev", 1), n("arlv", 0), s("anam", "Army of the Dead"), s("ansf", "(Acherus)"),
        s("atp1", "Army of the Dead", 1), s("aub1", "Raises the dead around Acherus to fight for the Ebon Blade.", 1),
        r("acdn", 150, 1), n("amcs", 0, 1), s("aart", BTN + "BTNAnimateDead.blp")]))
    advance = I("DeathsAdvance", "A", "ability")
    write("ability", advance, "AUau", merged(mods_of("ability", "AUau"), [
        n("aher", 0), n("alev", 1), n("arlv", 0), s("anam", "Death's Advance"), s("ansf", "(Acherus)"),
        s("atp1", "Death's Advance", 1), s("aub1", "Nearby friendly units move and regenerate faster.", 1),
        s("aart", BTN + "BTNUnholyAura.blp")]))
    flyer = I("AcherusFlying", "u", "unit")
    write("unit", flyer, "nzep", [
        s("unam", "Acherus: The Ebon Hold"), s("unsf", "(Ebon Blade, in flight)"),
        s("umdl", "buildings\\undead\\AltarOfDarkness\\AltarOfDarkness.mdl"), s("uico", BTN + "BTNAltarOfDarkness.blp"),
        real("usca", 1.1), n("uhpm", 4000), n("udef", 8), n("umvs", 180), r("umvh", 360), n("ufma", 40),
        n("ufoo", 0), n("ugol", 0), n("ulum", 0), s("urac", "undead"),
        s("utub", "The floating necropolis of the Knights of the Ebon Blade carries their army anywhere. Brought to "
                  "0 life it does not fall: it burns, hangs still for 60 seconds and mends -- its life is the "
                  "timer.")])
    rises = I("EbonHoldRises", "R", "upgrade")
    rises_cond = I("EbonHoldRisesCondition", "R", "upgrade")
    write("upgrade", rises, "Roch", [
        n("gglb", 0), n("glmb", 0), n("gtib", 60), n("gbpx", 0), n("gbpy", 2), s("greq", rises_cond, 1),
        s("gar1", BTN + "BTNAltarOfDarkness.blp", 1), s("gnsf", "(Ebon Blade)", 1),
        s("gnam", "The Ebon Hold Rises", 1), s("gtp1", "The Ebon Hold Rises", 1),
        s("gub1", "The four death knights channel for a minute and Acherus lifts into the sky -- a flying transport "
                  "with Remorseless Winter, Army of the Dead and Death's Advance.|n|n|cffffcc00Requires all four "
                  "death knights at level 10.|r", 1)])
    write("upgrade", rises_cond, "Roch", [s("gnsf", "(condition)", 1),
                                           s("gnam", "All four death knights at level 10", 1)])

    # ------------------------------------------------------------ the quilboar plague
    plague_buff = I("ZombiePlagueBuff", "B", "buff")
    write("buff", plague_buff, "Bfae", [
        s("fnam", "Zombie Plague"), s("ftip", "Zombie Plague"),
        s("fube", "This unit has the zombie plague; it slowly takes damage and rises as a zombie when it dies. Only "
                  "a dispel or purge cures it."),
        s("fart", BTN + "BTNPlagueCloud.blp"), s("ftat", "Abilities\\Spells\\Undead\\PlagueCloud\\PlagueCloudtarget.mdl")])
    plague = I("ZombiePlague", "A", "ability")
    write("ability", plague, "ACff", [
        s("anam", "Zombie Plague"), s("ansf", "(plague boar)"), n("Fae1", 0, 1, 1), r("adur", 3600, 1),
        r("ahdu", 3600, 1), r("acdn", 0, 1), n("amcs", 0, 1), r("aran", 600, 1), s("abuf", plague_buff, 1),
        s("atar", "air,ground,organic,enemy,neutral", 1), s("aart", BTN + "BTNPlagueCloud.blp")])
    boar = I("PlagueBoar", "n", "unit")
    write("unit", boar, "nqb1", merged(mods_of("unit", "nqb1"), [
        s("unam", "Plague Boar"), s("unsf", "(Razorfen plague)"), n("uhpm", 120), n("udef", 0), n("uaen", 0),
        n("ua1b", -1), n("umvs", 230), n("ubba", 0), n("ubdi", 0), n("ubsi", 0), n("ulev", 1), s("uabi", ""),
        s("utub", "A boar of Razorfen Downs, swollen with the plague. It does not fight -- it spreads.")]))
    amnennar = I("Amnennar", "U", "unit")
    write("unit", amnennar, "Ulic", merged(mods_of("unit", "Ulic"), [
        s("unam", "The Coldbringer"), s("upro", "Amnennar"), s("unsf", "(Scourge)"),
        s("umdl", "units\\creeps\\RazormaneChief\\RazormaneChief.mdl"), s("uico", BTN + "BTNRazormaneChief.blp"),
        real("usca", 1.6), n("uclr", 150), n("uclg", 200), n("uclb", 255), s("ureq", ""),
        s("utub", "A quilboar lich of Razorfen Downs, the Scourge's herald in Kalimdor.")]))

    plague_go = I("PlagueGoesSouth", "R", "upgrade")
    plague_cond = I("PlagueGoesSouthCondition", "R", "upgrade")
    write("upgrade", plague_go, "Roch", [
        n("gglb", 0), n("glmb", 0), n("gtib", 20), n("gbpx", 2), n("gbpy", 1), s("greq", plague_cond, 1),
        s("gar1", BTN + "BTNPlagueCloud.blp", 1), s("gnsf", "(Scourge)", 1),
        s("gnam", "The Plague Goes South", 1), s("gtp1", "The Plague Goes South", 1),
        s("gub1", "Amnennar the Coldbringer rises in Razorfen Downs and about 30 plague boars march on the "
                  "strongholds of Kalimdor. They do not fight -- they infect: the zombie plague eats at its victims "
                  "until a dispel or purge cures it, and whoever dies with it rises as a zombie of the Scourge.|n|n"
                  "|cffffcc00Requires the Plague of Undeath.|r", 1)])
    write("upgrade", plague_cond, "Roch", [s("gnsf", "(condition)", 1), s("gnam", "The Plague of Undeath", 1)])
    throne = load("unit", "u000")
    ures = next((m["Value"] for m in throne["Modifications"] if m["Id"] == "ures"), "") if throne else ""
    if throne and plague_go not in ures.split(","):
        ures = (ures + "," + plague_go).strip(",")
        write("unit", "u000", throne["OldId"], merged(throne["Modifications"], [s("ures", ures)]))

    # ------------------------------------------------------------ the mini-bosses' artifacts
    pearl = I("PearlOfTheGrimscale", "I", "item")
    write("item", pearl, "ckng", merged(mods_of("item", "I00I"), [
        s("iabi", "A03X,AIdb"), s("unam", "Pearl of the Grimscale"), s("iico", BTN + "BTNWaterOrb.blp"),
        s("ides", "Mmmrrrggglll's treasure. Increases all attributes; heals."),
        s("utub", "|cffff8c00Artifact|r|nThe Grimscale chieftain's hoarded pearl. Increases all attributes. Can "
                  "cast Crown Heal.")]))
    banner = I("BlackmaneWarbanner", "I", "item")
    write("item", banner, "ckng", merged(mods_of("item", "I00I"), [
        s("iabi", "A0AL,AIcd"), s("unam", "Blackmane Warbanner"), s("iico", BTN + "BTNInv_bannerpvp_02.blp"),
        s("ides", "Snarlmane's banner. Increases Strength; nearby allies deal more damage."),
        s("utub", "|cffff8c00Artifact|r|nThe warbanner of the Blackmane gnolls. Increases Strength by 10; nearby "
                  "friendly units deal more damage.")]))

    # ------------------------------------------------------------ the harbour docks (Kalimdor <-> Northrend)
    dock_n = I("DockNorthrend", "n", "unit")
    dock_k = I("DockKalimdor", "n", "unit")
    for oid, name in ((dock_n, "Harbour: ship to Kalimdor"), (dock_k, "Harbour: ship to Northrend")):
        write("unit", oid, "nwgt", merged(mods_of("unit", "nwgt"), [
            s("unam", name), s("unsf", "(the late crossing)"),
            s("utub", "Ships sail between Northrend and Kalimdor -- open to everyone since the crossing opened.")]))

    ids.save()
    constants(ids.used)
    print(f"wrote {len(ids.used)} objects, Kalecgos' forms edited, Objects088.g.cs")


def constants(used):
    lines = ["// <auto-generated> tools/shattered-legacies/content/content_088.py -- the hand-made objects of 0.88.0.",
             "// Do not edit. </auto-generated>",
             "namespace WarcraftLegacies.Source.ShatteredLegacies.Events",
             "{",
             "  internal static class Objects088",
             "  {",
             f"    internal const int KalecgosHuman = {fourcc('UZn0')}; // UZn0 (24p U027)",
             f"    internal const int KalecgosDragon = {fourcc('EZn1')}; // EZn1 (24p E004)",
             f"    internal const int VanCleef = {fourcc('EZn0')}; // EZn0 (24p E01H)",
             f"    internal const int Mmmrrrggglll = {fourcc('UZn1')}; // UZn1 (24p U02M)"]
    for name, oid, kind in used:
        lines.append(f"    internal const int {name} = {fourcc(oid)}; // {oid}")
    lines += ["  }", "}", ""]
    os.makedirs(os.path.dirname(CS), exist_ok=True)
    with open(CS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    main()
