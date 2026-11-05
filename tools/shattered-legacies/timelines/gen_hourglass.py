"""Hourglass of Fate: writes its map data and the matching C# constants.

Usage (repo root):  python3 tools/shattered-legacies/timelines/gen_hourglass.py
Writes
  mapdata/WarcraftLegacies/UnitData/nZh0.json        the Hourglass of Fate (building, one per player slot, by code)
  mapdata/WarcraftLegacies/UpgradeData/RZd1..RZdH    one DECISION research per turning point (researched at the Hourglass)
  mapdata/WarcraftLegacies/UpgradeData/RZc1..RZcH    one CONDITION tech per turning point (a dummy, granted by code when
                                                      the trigger fired; the decision requires it -> greyed out until then)
  src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/Core/HourglassData.g.cs   the ids as C# constants
Edit TURNING_POINTS (and the building below), then re-run. Ids follow the fork's ID rule (second character Z, third
a lowercase letter -- ShatteredLegacies/README.md).
"""
import json
import os
import struct

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
CS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Core", "HourglassData.g.cs")

BUILDING = "nZh0"
WL_RESEARCHES = ["R0XZ"]  # WL's own turning-point researches, also listed on the Hourglass (Scarlet Crusade)
ICONS = "ReplaceableTextures\\CommandButtons\\"
# command-card button per decision (x, y); default (0, 0). A faction with two turning points needs two positions.
BUTTONS = {"Forsaken": (1, 0)}

# key (C# name), WL faction name, decision id, condition id, button title, extended tooltip, condition text, icon
TURNING_POINTS = [
    ("Quelthalas", "Quel'thalas", "RZd1", "RZc1", "Fallback: Sunfury",
     "Kael'thas leads the survivors of Quel'thalas to Netherstorm and the Sunfury rise: a second start as WL's Sunfury "
     "faction. Everything left in Quel'thalas is lost.",
     "Silvermoon Palace lost", "BTNHeroBloodElfPrince"),
    ("Forsaken", "Quel'thalas", "RZdI", "RZcI", "Betrayal: Forsaken",
     "Sylvanas' spirit rises as a banshee and breaks away: you play the Forsaken in Northrend, enslaved to the Scourge, "
     "while Quel'thalas fights on in another slot under the AI.",
     "Sylvanas Windrunner slain by the Scourge", "BTNBanshee"),
    ("Gilneas", "Gilneas", "RZd2", "RZc2", "Fallback: Gilneans on Darkshore (placeholder)",
     "Placeholder until Gilneas takes Dalaran's Theramore path.", "Gilneas Castle lost", "BTNJaina"),
    ("Stormwind", "Stormwind", "RZd3", "RZc3", "Fallback: Valiance Expedition (placeholder)",
     "Placeholder: Bolvar Fordragon leads the Valiance Expedition to Northrend.", "Stormwind Keep lost",
     "BTNHeroPaladin"),
    ("Ironforge", "Ironforge", "RZd4", "RZc4", "Fallback: Dwarven Expedition (placeholder)",
     "Placeholder: the dwarves dig for hope in the Storm Peaks.", "The Great Forge lost", "BTNHeroMountainKing"),
    ("Kultiras", "Kul'tiras", "RZd5", "RZc5", "Fallback: Kezan Enterprise",
     "Katherine Proudmoore rallies the survivors on Kezan, joined by the goblins: a mercenary faction on its own team. "
     "Kul Tiras' old holdings go to Stormwind if it still fights on your team.", "Boralus Keep lost", "BTNHeroTinker"),
    ("Druids", "Druids", "RZd6", "RZc6", "Fallback: Dream trees (placeholder)",
     "Placeholder: the druids walk the Emerald Dream to its trees in the Eastern Kingdoms.",
     "Nordrassil and Cenarion Hold lost", "BTNKeeperOfTheGrove"),
    ("Sentinels", "Sentinels", "RZd7", "RZc7", "Fallback: Maiev's Watchers (placeholder)",
     "Placeholder: Maiev Shadowsong leads her Watchers into Outland.", "Auberdine and Feathermoon Stronghold lost",
     "BTNHeroWarden"),
    ("Exodar", "The Exodar", "RZd8", "RZc8", "Fallback: Draenei of Shattrath (placeholder)",
     "Placeholder until the draenei take Maiev's Watchers path.", "Exodar Regalis lost", "BTNHeroArchMage"),
    ("Frostwolf", "Frostwolf", "RZd9", "RZc9", "Fallback: Dragonmaw (placeholder)",
     "Placeholder until the orcs' fallback becomes the Twilight's Hammer.", "Thunder Bluff and Darkspear Hold lost",
     "BTNChaosWarlord"),
    ("Warsong", "Warsong", "RZdA", "RZcA", "Fallback: Dragonmaw (placeholder)",
     "Placeholder until the orcs' fallback becomes the Twilight's Hammer.", "Stonemaul Keep and Orgrimmar lost",
     "BTNChaosWarlord"),
    ("Scourge", "Scourge", "RZdB", "RZcB", "Fallback: Knights of the Ebon Blade (not yet)",
     "Planned: when Lich King Arthas dies, his death knights break free. Not in this version.",
     "Not available yet", "BTNHeroDeathKnight"),
    ("Legion", "Legion", "RZdC", "RZcC", "Fallback: Dreadlord Insurgents (not yet)",
     "Planned: the dreadlords fight on from a hidden lair. Not in this version.", "Not available yet",
     "BTNHeroDreadLord"),
    ("FelHorde", "Fel Horde", "RZdD", "RZcD", "Fallback: Mag'har (not yet)",
     "Planned: the orcs of Nagrand who never drank the blood. Not in this version.", "Not available yet",
     "BTNHeroBlademaster"),
    ("Illidari", "Illidan", "RZdE", "RZcE", "Fallback: Akama's Ashtongue (not yet)",
     "Planned: Akama opens the Black Temple to Illidan's enemies. Not in this version.", "Not available yet",
     "BTNEvilIllidan"),
    ("Skywall", "Skywall", "RZdF", "RZcF", "Fallback (not decided yet)",
     "Planned: the Earthen Ring or Ramkahen. Not in this version.", "Not available yet", "BTNHeroAvatarOfFlame"),
    ("BlackEmpire", "Black Empire", "RZdG", "RZcG", "Fallback (not decided yet)",
     "Planned: Xal'atath or the Titan Watchers. Not in this version.", "Not available yet", "BTNFacelessOne"),
    ("Ahnqiraj", "Ahn'qiraj", "RZdH", "RZcH", "Fallback: Brood of Nozdormu (not yet)",
     "Planned: the bronze dragonflight takes over the Caverns of Time. Not in this version.", "Not available yet",
     "BTNBronzeDragon"),
]


def be(code):
    return struct.unpack(">i", code.encode("latin1"))[0]


def mod(fid, value, level=None, typ=None):
    m = {}
    if level is not None:
        m["Level"] = level
    m["Id"] = fid
    if typ is not None:
        m["Type"] = typ
    m["Value"] = value
    return m


def write(path, data):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=2, ensure_ascii=False))


def building():
    ures = ",".join([t[2] for t in TURNING_POINTS] + WL_RESEARCHES)
    mods = [mod("uabi", "Avul", typ=3), mod("ucol", 0, typ=2), mod("uhos", 1), mod("uhpm", 5000),
            mod("uhrt", "always", typ=3), mod("utyp", "ancient,sapper", typ=3), mod("ua1b", -1), mod("ua1d", 1),
            mod("ua1s", 1), mod("uaen", 0), mod("ua1r", 900), mod("uacq", 900, typ=2), mod("ua1g", "bridge", typ=3),
            mod("urac", "creeps", typ=3), mod("upat", "PathTextures\\4x4SimpleSolid.tga", typ=3),
            mod("unam", "Hourglass of Fate", typ=3), mod("unsf", "(Shattered Legacies)", typ=3),
            mod("ures", ures, typ=3), mod("ussc", 1.5, typ=1)]
    return {"OldId": "ncop", "NewId": BUILDING, "Unk": [0], "Modifications": mods}


def decision(t):
    key, _, rid, cid, title, text, cond, icon = t
    bx, by = BUTTONS.get(key, (0, 0))
    mods = [mod("gglb", 0), mod("glmb", 0), mod("gtib", 5), mod("gbpx", bx), mod("gbpy", by),
            mod("greq", cid, 1, 3), mod("gar1", ICONS + icon + ".blp", 1, 3), mod("gnsf", "(Hourglass of Fate)", 1, 3),
            mod("gnam", title, 1, 3), mod("gtp1", title, 1, 3), mod("gub1", text, 1, 3)]
    return {"OldId": "Roch", "NewId": rid, "Unk": [0], "Modifications": mods}


def condition(t):
    cid, cond = t[3], t[6]
    return {"OldId": "Roch", "NewId": cid, "Unk": [0],
            "Modifications": [mod("gnsf", "(Hourglass condition)", 1, 3), mod("gnam", cond, 1, 3)]}


def constants():
    lines = ["// <auto-generated> tools/shattered-legacies/timelines/gen_hourglass.py -- the Hourglass of Fate's object ids",
             "// (map data written by the same script). Do not edit. </auto-generated>",
             "namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core",
             "{",
             "  internal static class HourglassData",
             "  {",
             f"    internal const int BuildingType = {be(BUILDING)}; // {BUILDING} Hourglass of Fate",
             "",
             "    // decision research (researched at the Hourglass) and condition tech (granted when the trigger fired)"]
    for key, fac, rid, cid, title, *_ in TURNING_POINTS:
        lines.append(f"    internal const int Decision{key} = {be(rid)}; // {rid} {title}")
        lines.append(f"    internal const int Condition{key} = {be(cid)}; // {cid}")
    lines.append("")
    lines.append("    // every research the Hourglass lists (decisions + WL's own), for the per-player limits")
    ids = ", ".join(str(be(t[2])) for t in TURNING_POINTS)
    wl = ", ".join(str(be(w)) for w in WL_RESEARCHES)
    lines.append(f"    internal static int[] Decisions = {{ {ids} }};")
    lines.append(f"    internal static int[] WlResearches = {{ {wl} }}; // {', '.join(WL_RESEARCHES)}")
    lines += ["  }", "}", ""]
    with open(CS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def main():
    write(os.path.join(MAP, "UnitData", BUILDING + ".json"), building())
    for t in TURNING_POINTS:
        write(os.path.join(MAP, "UpgradeData", t[2] + ".json"), decision(t))
        write(os.path.join(MAP, "UpgradeData", t[3] + ".json"), condition(t))
    constants()
    print(f"wrote {BUILDING}, {len(TURNING_POINTS)} decisions, {len(TURNING_POINTS)} conditions, HourglassData.g.cs")


if __name__ == "__main__":
    main()
