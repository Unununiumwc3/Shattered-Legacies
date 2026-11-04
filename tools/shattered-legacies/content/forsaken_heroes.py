"""Sylvanas of the Forsaken: her three phase heroes and the hero skills made for them (0.56).

Usage (repo root):  python3 tools/shattered-legacies/content/forsaken_heroes.py   (after import_24p.py)
Writes mapdata/WarcraftLegacies/UnitData/UZa1.json, UZa2.json, UZa3.json and AbilityData/AZs1..AZs5.json.
  UZa1 The Banshee            (caster)  Possession, Anti-magic Shell, Banshee Wail; Ethereal Walk (no skill point)
  UZa2 The Dark Ranger        (attack)  Black Arrow, Silence, Aimed Shot; ultimate Multi Shot; Trueshot Aura
  UZa3 Queen of the Forsaken  (mix)     Black Arrow, Possession, Banshee Wail; ultimate Banshee Queen; Trueshot Aura,
                                        Ethereal Walk
All three start from Quel'thalas' Sylvanas (Hvwd: stats, attack). Every learnable skill has 4 levels (ultimates 3,
from level 6), plus the Attribute Bonus. The new skills are hero versions of existing spells; level 1 = the
original, each level one even step (maintainer: no new spells -- reuse, scale carefully).
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
IDS = json.load(open(os.path.join(HERE, "import_ids.json")))
BTN = "ReplaceableTextures\\CommandButtons\\"
REAL = {"aare", "acdn", "adur", "ahdu", "aran", "bsk1", "bsk2", "bsk3", "Roa1"}  # "unreal" fields (Type 2) in WL's JSON


def imp(oid):
    return IDS["Forsaken:" + oid]


def load(kind, oid):
    return json.load(open(os.path.join(MAP, kind, oid + ".json"), encoding="utf-8-sig"))


def write(kind, oid, data):
    with open(os.path.join(MAP, kind, oid + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def s(fid, value, level=None):
    m = {} if level is None else {"Level": level}
    m.update({"Id": fid, "Type": 3, "Value": value})
    return m


def n(fid, value, level=None, real=False):
    m = {} if level is None else {"Level": level}
    m.update({"Id": fid})
    if real:
        m["Type"] = 2  # unreal (0..1 style values)
    m["Value"] = value
    return m


def hero_skill(new_id, base, name, hotkey, x, levels, per_level, text, research_text, icon, targets=None, buffs=None):
    """A learnable hero skill with `levels` levels. per_level: {field: [values], ...} (real values with a float)."""
    # 0.56.1: no tech requirement -- the stock unit spells require Banshee Adept / Master Training, so the learned
    # skill stayed hidden ("I lose the skill point but the ability does not show up")
    mods = [s("areq", ""), n("aher", 1), n("alev", levels), n("arlv", 1), n("alsk", 2), s("anam", name),
            s("ansf", "(Forsaken - Sylvanas)"),
            s("aart", BTN + icon), s("arar", BTN + icon), n("abpx", x), n("abpy", 2), n("arpx", x), n("arpy", 0),
            s("ahky", hotkey), s("arhk", hotkey), s("aret", "Learn " + name + " - [|cffffcc00Level %d|r]"),
            s("arut", research_text)]
    for lv in range(1, levels + 1):
        mods.append(s("atp1", name + " - [|cffffcc00Level " + str(lv) + "|r]", lv))
        mods.append(s("aub1", text(lv), lv))
        if targets:
            mods.append(s("atar", targets, lv))
        if buffs:
            mods.append(s("abuf", buffs, lv))
        for field, values in per_level.items():
            mods.append(n(field, values[lv - 1], lv, real=field in REAL))
    return {"OldId": base, "NewId": new_id, "Unk": [0], "Modifications": mods}


def abilities():
    pos = [3, 4, 5, 6]
    write("AbilityData", "AZs1", hero_skill(
        "AZs1", "Apos", "Possession", "Q", 0, 4,
        {"Pos1": pos, "amcs": [150, 140, 130, 120], "acdn": [90, 80, 70, 60], "aran": [500, 500, 500, 500]},
        lambda lv: f"Sylvanas takes over an enemy unit of level {pos[lv - 1]} or lower for good. Her body stays "
                   "behind while she channels.",
        "Sylvanas takes over an enemy unit for good.|n|n|cffffcc00Level 1|r - up to level 3.|n|cffffcc00Level 2|r - "
        "up to level 4.|n|cffffcc00Level 3|r - up to level 5.|n|cffffcc00Level 4|r - up to level 6.",
        "BTNPossession.blp", targets="enemies,ground,neutral,nonhero,organic"))
    shield = [300, 450, 600, 750]
    write("AbilityData", "AZs2", hero_skill(
        "AZs2", "Aam2", "Anti-magic Shell", "W", 1, 4,
        {"Ams3": shield, "adur": [30, 30, 30, 30], "ahdu": [30, 30, 30, 30], "amcs": [75, 70, 65, 60],
         "acdn": [20, 18, 16, 14], "aran": [500, 500, 500, 500]},
        lambda lv: f"A barrier around a friendly unit that absorbs {shield[lv - 1]} magic damage. Lasts 30 seconds.",
        "A barrier that absorbs magic damage.|n|n|cffffcc00Level 1|r - 300.|n|cffffcc00Level 2|r - 450.|n"
        "|cffffcc00Level 3|r - 600.|n|cffffcc00Level 4|r - 750.",
        "BTNAntiMagicShell.blp", targets="air,friend,ground,invulnerable,self,vulnerable", buffs="Bam2,Bams"))
    # Banshee Wail: WL's 4-level hero Howl of Terror (Magtheridon's A07M) under the banshee's name
    wail = load("AbilityData", "A07M")
    wail["NewId"] = "AZs3"
    reduce = [0.25, 0.4, 0.55, 0.7]  # less damage dealt by the enemies (Howl of Terror's Roa1)
    keep = [m for m in wail["Modifications"] if m["Id"] not in ("ansf", "ahky", "arhk", "atp1", "aub1", "anam", "aret",
                                                                 "arut", "abpx", "abpy", "arpx", "arpy", "Roa1", "areq")]
    keep += [s("areq", "")] + [n("Roa1", reduce[lv - 1], lv, real=True) for lv in range(1, 5)]
    keep += [s("anam", "Banshee Wail"), s("ansf", "(Forsaken - Sylvanas)"), s("ahky", "E"), s("arhk", "E"),
             n("abpx", 2), n("abpy", 2), n("arpx", 2), n("arpy", 0),
             s("aret", "Learn Banshee Wail - [|cffffcc00Level %d|r]"),
             s("arut", "Sylvanas' wail weakens nearby enemies: they deal less damage for a while.|n|n"
                       "|cffffcc00Level 1|r - 25% less damage.|n|cffffcc00Level 2|r - 40% less damage.|n"
                       "|cffffcc00Level 3|r - 55% less damage.|n|cffffcc00Level 4|r - 70% less damage.")]
    for lv in range(1, 5):
        keep += [s("atp1", "Banshee Wail - [|cffffcc00Level " + str(lv) + "|r]", lv),
                 s("aub1", "Sylvanas' wail weakens nearby enemies: they deal " + str(int(reduce[lv - 1] * 100))
                   + "% less damage for a while.", lv)]
    wail["Modifications"] = keep
    write("AbilityData", "AZs3", wail)
    # Ethereal Walk (no skill point): Berserk's speed, the code makes her ethereal and lets her pass through units
    write("AbilityData", "AZs4", {"OldId": "Absk", "NewId": "AZs4", "Unk": [0], "Modifications": [
        s("areq", ""), n("aher", 0), n("alev", 1), s("anam", "Ethereal Walk"), s("ansf", "(Forsaken - Sylvanas)"),
        s("aart", BTN + "BTNEtherealFormOn.blp"), n("abpx", 1), n("abpy", 1), s("ahky", "D"),
        n("bsk1", 0.5, 1, real=True), n("bsk2", 0.0, 1, real=True), n("bsk3", 0.0, 1, real=True),
        n("adur", 6, 1, real=True), n("ahdu", 6, 1, real=True), n("acdn", 30, 1, real=True), n("amcs", 50, 1),
        s("atp1", "Ethereal Walk", 1),
        s("aub1", "For 6 seconds Sylvanas becomes ethereal: 50% faster, she walks through units and physical attacks "
                  "pass through her -- magic hurts her more. She cannot attack meanwhile.", 1)]})
    # Multi Shot as the Dark Ranger's ultimate at level 6 (WL's Sylvanas has it at level 10)
    multi = load("AbilityData", "A047")
    multi["NewId"] = "AZs5"
    for m in multi["Modifications"]:
        if m["Id"] == "arlv":
            m["Value"] = 6
        if m["Id"] == "alsk":
            m["Value"] = 6
        if m["Id"] == "ansf":
            m["Value"] = "(Forsaken - Sylvanas)"
    write("AbilityData", "AZs5", multi)


def heroes():
    base = load("UnitData", "Hvwd")
    common = [m for m in base["Modifications"] if m["Id"] not in ("uico", "unsf", "unam", "uawt", "utpr", "utip", "utub",
                                                                   "umdl", "urac", "uhot", "uhab", "uabi", "upro")]
    phases = [
        ("UZa1", "The Banshee", "units\\undead\\Banshee\\Banshee", "BTNBanshee.blp",
         "AZs1,AZs2,AZs3,Aamk", "AZs4,AInv,Ault",
         "Sylvanas Windrunner, slain by the Scourge and risen as a banshee. A caster: she possesses her enemies."),
        ("UZa2", "The Dark Ranger", "Units\\Undead\\EvilSylvanas\\EvilSylvanas", "BTNBansheeRanger.blp",
         f"{imp('A03J')},A083,A0PK,AZs5,Aamk", f"{imp('A01O')},A0KV,AInv,Ault",
         "Free of the Lich King, Sylvanas is a ranger again -- a deadly archer."),
        ("UZa3", "Queen of the Forsaken", "war3mapImported\\Hero_DarkRangerBoss2.mdl", "BTNBansheeRanger.blp",
         f"{imp('A03J')},AZs1,AZs3,{imp('A0DU')},Aamk", f"{imp('A01O')},AZs4,AInv,Ault",
         "The Banshee Queen, ruler of the Undercity: archer and spellcaster both."),
    ]
    for oid, title, model, icon, uhab, uabi, story in phases:
        mods = list(common) + [s("umdl", model), s("urac", "undead"), s("uico", BTN + icon), s("unam", title),
                               s("unsf", "(Forsaken)"), s("upro", "Sylvanas Windrunner"), s("uawt", "Revive Sylvanas"),
                               s("utpr", "Revive Sylvanas"), s("utip", "Summon Sylvanas"), s("utub", story),
                               s("uhab", uhab), s("uabi", uabi)]
        write("UnitData", oid, {"OldId": "Hvwd", "NewId": oid, "Unk": [0], "Modifications": mods})


if __name__ == "__main__":
    abilities()
    heroes()
    print("wrote UZa1-UZa3, AZs1-AZs5")
