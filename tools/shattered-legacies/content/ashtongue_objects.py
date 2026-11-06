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

SITES = ["Telredor", "Halaar", "Shattrath", "Oshu'gun"]
CONDITIONS = [(f"RZt{i}", f"{SITES[i]} purified") for i in range(4)] + [("RZt4", "Karabor restored")]


REAL = {"usca"}


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


def main():
    for code, name in CONDITIONS:
        write("UpgradeData", code, {"OldId": "Roch", "NewId": code, "Unk": [0], "Modifications": [
            v("gnam", name, 1), v("gnsf", "(Condition)", 1), v("glvl", 1), v("gglb", 0), v("glmb", 0)]})

    write("AbilityData", "AZt0", {"OldId": "Afae", "NewId": "AZt0", "Unk": [0], "Modifications": [
        v("anam", "Purify"), v("atp1", "Purify", 1),
        v("aub1", "Draws mana from your and your allies' units within 1500 and turns it into purification of "
                  "the fel-tainted draenei sites -- Telredor, Halaar, Shattrath, Oshu'gun, in this order. Keep "
                  "autocast on. Ishanah cannot leave Shattrath until Outland is cleansed.", 1),
        v("aart", BTN + "BTNSpell_Holy_Renew.blp"), v("amcs", 0, 1), v("aran", 1500.0, 1), v("acdn", 1.0, 1),
        v("adur", 0.01, 1), v("ahdu", 0.01, 1), v("atar", "self", 1), v("abpx", 0), v("abpy", 2),
        v("ahky", "P")]})

    bt = black_temple()
    trains = ",".join([copy_id("o05B"), "ndrn", "ndrs", copy_id("h09T"), copy_id("h09U"), copy_id("o05C"),
                       copy_id("h09R"), copy_id("o05D"), copy_id("H0AK")])
    for code, name, tint, text in [
            ("oZt0", "Temple of the Deathsworn", None,
             "Akama's temple, raised on the ruins of the Black Temple. Trains the Ashtongue; the purified draenei "
             "sites unlock more."),
            ("oZt1", "Temple of Karabor", (255, 240, 190),
             "Karabor, restored to the Light. Trains the Ashtongue and Xi'ri, the naaru.")]:
        mods = [v("unam", name), v("umdl", bt.get("umdl", "")), v("usca", float(bt.get("usca", 1.0))),
                v("upat", bt.get("upat", "")), v("uhpm", 7000), v("ufma", 100), v("ugol", 0), v("ulum", 0),
                v("utra", trains), v("ures", ""), v("uupt", ""), v("ubui", ""), v("uabi", "Arev,Avul"),
                v("ureq", ""), v("utub", text), v("uico", BTN + "BTNTempleOfTheDamned.blp"), v("urac", "other")]
        if tint:
            mods += [v("uclr", tint[0]), v("uclg", tint[1]), v("uclb", tint[2])]
        write("UnitData", code, {"OldId": "oalt", "NewId": code, "Unk": [0], "Modifications": mods})
    print("wrote 5 conditions, Purify, 2 temples")


if __name__ == "__main__":
    main()
