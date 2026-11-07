"""The Ashtongue Deathsworn's hand-made objects (0.63) -- what neither WL nor the 24p map has.

Usage (repo root):  python3 tools/shattered-legacies/content/ashtongue_objects.py   (after import_24p.py: it reads the
Ashtongue copies' ids from import_ids.json)
Writes:
  RZt0-RZt4  conditions: "Telredor purified" ... "Karabor restored" (granted by AshtonguePurification); the units /
             the hero that need them show the line greyed out until then
  AZt0       Purify (Ishanah's autocast; Faerie Fire based -- only the toggle, AshtonguePurification does the work)
  oZt0       Temple of the Deathsworn -- rises where the Black Temple stood (the Black Temple's model and size); an
             altar (trains, revives heroes), the Ashtongue's production centre; food 100
  oZt1       Temple of Karabor -- the same, light-tinted, after "Restoring Karabor to the Light"
Ids are fixed: never reorder the tables.
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
BTN = "ReplaceableTextures\\CommandButtons\\"

SITES = ["Shattrath", "Halaa", "Telredor", "Oshu'gun"]  # 0.64: this order (maintainer); then the temple
CONDITIONS = [(f"RZt{i}", f"{SITES[i]} purified") for i in range(4)] + [("RZt4", "Karabor restored")]


REAL = {"usca", "ussc"}


def v(fid, value, level=None):
    """a field with the type WL's JSON needs: text 3, real 2, integer none (check_json.py)"""
    m = {} if level is None else {"Level": level}
    m["Id"] = fid
    if isinstance(value, str):
        m["Type"] = 3
    elif isinstance(value, float):
        m["Type"] = 1 if fid in REAL else 2  # WL writes scales as real (1), the rest as unreal (2) -- check_json.py
    m["Value"] = value
    return m


def write(kind, code, data):
    with open(os.path.join(MAP, kind, code + ".json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def copy_id(old):
    with open(os.path.join(HERE, "import_ids.json"), encoding="utf-8") as f:
        return json.load(f)["Ashtongue:" + old]


def black_temple():
    with open(os.path.join(MAP, "UnitData", "o00F.json"), encoding="utf-8-sig") as f:
        d = json.load(f)
    return {m["Id"]: m["Value"] for m in d["Modifications"]}


def source(mapname, code):
    """model / scale / pathing / selection size of a building the site copies"""
    with open(os.path.join(ROOT, "mapdata", mapname, "UnitData", code + ".json"), encoding="utf-8-sig") as f:
        d = json.load(f)
    return {m["Id"]: m["Value"] for m in d["Modifications"] if m.get("Level") in (None, 0, 1)}


def building(code, name, look, trains="", researches="", food=0, mana=0, extra=None, text=""):
    mods = [v("unam", name), v("umdl", look.get("umdl", "")), v("upat", look.get("upat", "")),
            v("uico", look.get("uico", BTN + "BTNDraeneiDivineCitadel.blp")), v("ugol", 0), v("ulum", 0),
            v("utra", trains), v("ures", researches), v("uupt", ""), v("ubui", ""), v("ureq", ""), v("utub", text),
            v("urac", "other"), v("ufma", food)]
    if "usca" in look:
        mods.append(v("usca", float(look["usca"])))
    if "ussc" in look:
        mods.append(v("ussc", float(look["ussc"])))
    if mana:
        mods += [v("umpm", mana), v("umpi", 0), v("umpr", 0.0)]  # a pool (the purification), no regeneration
    mods += extra or []
    write("UnitData", code, {"OldId": "obar", "NewId": code, "Unk": [0],
                             "Modifications": mods + [v("uabi", "Avul")] if not any(m["Id"] == "uabi" for m in mods)
                             else mods})


def main():
    for code, name in CONDITIONS:
        write("UpgradeData", code, {"OldId": "Roch", "NewId": code, "Unk": [0], "Modifications": [
            v("gnam", name, 1), v("gnsf", "(Condition)", 1), v("glvl", 1), v("gglb", 0), v("glmb", 0)]})

    write("AbilityData", "AZt0", {"OldId": "Afae", "NewId": "AZt0", "Unk": [0], "Modifications": [
        v("anam", "Purify"), v("atp1", "Purify", 1),
        v("aub1", "While on (autocast), Shattrath draws mana from your and your allies' units within 1500 and "
                  "spends it on purifying the fel-tainted draenei sites: Shattrath, Halaa, Telredor, Oshu'gun, then "
                  "the temple itself -- Karabor. Ishanah cannot leave Shattrath until the work is done.", 1),
        v("aart", BTN + "BTNSpell_Holy_Renew.blp"), v("amcs", 0, 1), v("aran", 1500.0, 1), v("acdn", 1.0, 1),
        v("adur", 0.01, 1), v("ahdu", 0.01, 1), v("atar", "self", 1), v("abpx", 0), v("abpy", 2),
        v("ahky", "P")]})

    bt = black_temple()
    start = ",".join([copy_id("o05B"), "ndrn", "ndrs"])
    temple = {"umdl": bt.get("umdl", ""), "usca": bt.get("usca", 1.0), "upat": bt.get("upat", ""),
              "uico": BTN + "BTNTempleOfTheDamned.blp"}
    tint = [v("uclr", 255), v("uclg", 240), v("uclb", 190)]
    building("oZt0", "Temple of the Deathsworn", temple, trains=start, food=30,
             extra=[v("uabi", "Arev,Avul"), v("uhpm", 7000)],
             text="Akama's temple, raised on the ruins of the Black Temple. Trains the Ashtongue; the purified draenei "
                  "sites train more. Purified last of all, it becomes the Temple of Karabor.")
    building("oZt1", "Temple of Karabor", temple, trains=start + "," + copy_id("H0AK"), food=30,
             extra=[v("uabi", "Arev,Avul"), v("uhpm", 7000)] + tint,
             text="Karabor, restored to the Light. Trains the Ashtongue and Xi'ri, the naaru.")

    shattrath = source("WarcraftLegacies", "h0AH")
    halaa = source("WarcraftLegacies", "h0AE")
    telredor = source("WarcraftLegacies24p", "h0AG")
    oshugun = source("WarcraftLegacies", "h02Z")
    tainted = "Fel-tainted: purified by Ishanah's work at Shattrath (in its order), then it trains and researches."
    # Shattrath's mana pool shows the purification of the current site (its maximum set by code to that site's cost)
    building("oZt2", "Shattrath (fel-tainted)", shattrath, mana=2000, text=tainted)
    building("oZt3", "Shattrath", shattrath, mana=2000,
             trains=",".join([copy_id("o05C"), copy_id("h09R")]),
             researches=",".join(["Rhme", "Rhar", "R07C", copy_id("R023")]),
             text="The city of light. Its pool of mana is the purification under way. Trains Luminarchs and "
                  "Vindicators; researches weapons, armour and the Luminarchs' training.")
    building("oZt4", "Halaa (fel-tainted)", halaa, text=tainted)
    building("oZt5", "Halaa", halaa, trains=copy_id("h09U"), text="Nagrand's draenei town. Trains Elekk Knights.")
    building("oZt6", "Telredor (fel-tainted)", telredor, text=tainted)
    building("oZt7", "Telredor", telredor, trains=copy_id("h09T"), text="The refuge of Zangarmarsh. Trains Rangari.")
    building("oZt8", "Oshu'gun (fel-tainted)", oshugun, text=tainted)
    building("oZt9", "Oshu'gun", oshugun, trains=copy_id("o05D"), researches="R078",
             text="The naaru's fallen vessel. Trains Elementalists; researches their training.")

    # food for the Outland CPs of the Ashtongue's team (AshtongueIncome places / removes one per CP): invisible
    write("UnitData", "oZta", {"OldId": "otrb", "NewId": "oZta", "Unk": [0], "Modifications": [
        v("unam", "Outland Supply"), v("umdl", "war3mapImported\\DUMMY.mdl"), v("upat", ""), v("ufma", 6),
        v("uabi", "Aloc,Avul"), v("ugol", 0), v("ulum", 0), v("ubui", ""), v("ureq", ""), v("utra", ""),
        v("ures", ""), v("ushu", ""), v("ucol", 0.0)]})
    print("wrote 5 conditions, Purify, 2 temples, 8 site buildings, the food dummy")


if __name__ == "__main__":
    main()
