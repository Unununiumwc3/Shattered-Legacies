"""Check the shipped construction tables against starting workers and known unlocks.
Run after gen_build_data.py from the repo root. No game client required.
"""
import re
from pathlib import Path
import unit_scan as us

source = Path(us.SRC) / "WarcraftLegacies.Source"
tables = {}
for file in [source / "Ai/AiBuildData.g.cs", source / "ShatteredLegacies/Bot/SlBuildData.g.cs"]:
    for name, body in re.findall(r"Dictionary<int, int\[\]> (\w+) = new\(\)\s*\{(.*?)\n    \};", file.read_text(), re.S):
        entries = tables.setdefault(name, {})
        for key, values in re.findall(r"\{ (\d+), new\[\] \{ ([\d, ]+) \} \}", body):
            entries[us.code(int(key))] = [us.code(int(v)) for v in values.split(",")]
menus, required = tables["Menus"], tables["Required"]
infos = us.object_infos(us.load_consts())
assert len(infos) >= 20, "Include WarcraftLegacies.Shared/FactionObjectLimits in the checkout"
tested = 0
for faction, objects in infos.items():
    workers = [us.code(v) for _, v, cats in objects if "Builder" in cats]
    if not workers:
        continue
    options = {b for w in workers for b in menus.get(w, [])}
    for category in ("TownHall", "Barracks", "Research", "Altar"):
        candidates = {us.code(v) for _, v, cats in objects if category in cats}
        assert candidates & options, (faction, category)
    tested += 1

# Shared peasant menu; Stormwind's Smithy requires BOTH Keep and Blacksmith.
assert {"h06F", "h06A", "h06K", "h06T", "hars"} <= set(menus["hpea"])
assert set(required["h06A"]) == {"h06M", "h06F"}
assert tables["Upgrades"]["h06K"] == ["h06M"]
assert tables["Upgrades"]["h06M"] == ["h06N"]
assert "h06T" in required["h06N"]
assert "h06F" not in tables["Products"] and "h06A" in tables["Products"]
assert "n04Q" in menus["u00D"] and "u00F" in menus["u00D"]
assert "ugho" not in menus and "uaco" in menus
assert "oZk3" in menus["oZke"] and "uZf3" in menus["uZf1"]
print(f"{tested} faction worker menus cover hall, barracks, research and altar. Unlock, producer and fork checks passed.")
