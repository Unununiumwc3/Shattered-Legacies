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

# Check both edges of each declared three-tier faction hall family.
chains = 0
for faction, entries in infos.items():
    tiers = {name[-1]: us.code(value) for name, value, cats in entries
             if re.search(r"_T[123]$", name) and set(cats) & {"TownHall", "Keep", "Castle"}}
    if set(tiers) == {"1", "2", "3"}:
        for source, target in (("1", "2"), ("2", "3")):
            assert tiers[target] in tables["Upgrades"].get(tiers[source], []), (faction, tiers)
        chains += 1
assert chains >= 21
assert set(tables["Upgrades"]["n081"]) == {"n000", "n05F"}
for gate in tables["Upgrades"]["n081"]:
    assert len(tables["Upgrades"][gate]) == 3
    assert "o02Z" in required[gate]
    for target in tables["Upgrades"][gate]:
        assert "o030" in required[target]
print(f"{chains} complete hall chains and both Demon Gate branches verified.")

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
