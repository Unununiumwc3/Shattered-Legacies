"""The Kezan Enterprise's hand-made objects (0.58) -- what the 24p goblins did not have.

Usage (repo root):  python3 tools/shattered-legacies/content/kezan_objects.py   (before or after import_24p.py)
Writes mapdata/WarcraftLegacies/UpgradeData/RZko.json:
  RZko Oil Refining (3 levels, researched at the Kezan Oil Supply -- import_24p.py's KEZAN set adds it to the Oil
       Supply's research list). No data effect: KezanEnterpriseOil raises the gold per Oil Supply / Oil Platform by
       KezanEnterpriseConfig.RefiningPerSupply / RefiningPerPlatform per level.
Keep the numbers in the tooltips in step with KezanEnterpriseConfig.
"""
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
BTN = "ReplaceableTextures\\CommandButtons\\"

SUPPLY, PLATFORM = 50, 25            # KezanEnterpriseConfig.GoldPerOilSupply / GoldPerOilPlatform
PER_SUPPLY, PER_PLATFORM = 10, 5     # KezanEnterpriseConfig.RefiningPerSupply / RefiningPerPlatform
NAMES = ["Oil Refining", "Improved Oil Refining", "Advanced Oil Refining"]


def s(fid, value, level=None):
    m = {} if level is None else {"Level": level}
    m.update({"Id": fid, "Type": 3, "Value": value})
    return m


def i(fid, value):
    return {"Id": fid, "Value": value}


def main():
    mods = [i("gglb", 150), i("gglm", 100), i("glmb", 0), i("glmm", 0), i("glvl", 3), i("gtim", 45), i("gtib", 15),
            i("gbpx", 0), i("gbpy", 1), s("gef1", ""), s("gcls", ""), s("grac", "orc")]
    for lvl in range(1, 4):
        sup, pla = SUPPLY + PER_SUPPLY * lvl, PLATFORM + PER_PLATFORM * lvl
        mods += [s("gnam", NAMES[lvl - 1], lvl), s("gtp1", "Research " + NAMES[lvl - 1], lvl),
                 s("gnsf", "(Kezan Enterprise)", lvl), s("ghk1", "R", lvl), s("gar1", BTN + "BTNOilStation.blp", lvl),
                 s("gub1", f"Refines more oil: every Kezan Oil Supply gives {sup} gold per minute, every Oil Platform "
                           f"{pla} (was {sup - PER_SUPPLY} / {pla - PER_PLATFORM}).", lvl)]
    data = {"OldId": "Rhar", "NewId": "RZko", "Unk": [0], "Modifications": mods}
    path = os.path.join(MAP, "UpgradeData", "RZko.json")
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))
    print("wrote", os.path.relpath(path, ROOT))


if __name__ == "__main__":
    main()
