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
BUTTONS = {"Forsaken": (1, 0), "LordaeronGilneas": (1, 0),
           # 0.88.0: WL's own elite choices are listed too (their buttons: Ironforge (0, 0) / (0, 1), Ahn'qiraj (0, 0) /
           # (0, 1)) -- the decision moves aside
           "Ironforge": (2, 0), "Ahnqiraj": (2, 0)}

# 0.88.0 (maintainer): ELITE CHOICES. WL's own (one of two elite types per faction, ResearchManager.RegisterIncompatibleSet)
# are also researched at the Hourglass -- the same research, so the lock is the same:
WL_ELITE = ["RV01", "RV02", "RK01", "RK02", "RT01", "RT02", "RZ03", "RZo1", "ZB24", "ZB74", "ZBHS", "ZBML", "TP28",
            "TP29"]
# ... and a new one for every faction with two elite-tier units: a faction can only have ONE elite unit, the other is
# locked (Events/EliteChoices.cs holds the units). key, choice A (title, icon), choice B (title, icon). Buttons (2, 2) and
# (3, 2); open from turn 10 (the condition RZe0, granted by code).
ELITE_CONDITION = "RZe0"
# 0.88.0: other researches on the Hourglass (content_088.py): The Plague Goes South (the Scourge; also at the Throne)
EXTRA = ["RZq2"]
ELITE = [
    ("Stormwind", ("Stormwind Champions", "BTNTheCaptain"), ("Conjurers", "BTNSorceress")),
    ("Lordaeron", ("Paladins", "BTNHeroPaladin"), ("Pegasus Knights", "BTNGryphonRider")),
    ("Dalaran", ("Kirin Tor", "BTNKirinTor"), ("Blue Dragons", "BTNAzureDragon")),
    ("Quelthalas", ("Farstriders", "BTNHighElvenArcher"), ("Blood Mages", "BTNBloodElfSupplyWagon")),
    ("Kultiras", ("Musketeers", "BTNRifleman"), ("Sea Giants", "BTNSeaGiant")),
    ("Gilneas", ("Greyguards", "BTNDireWolf"), ("Worgen Shamans", "BTNTimberWolf")),
    ("Scarlet", ("Templars", "BTNArthas"), ("Bishops of the Light", "BTNPriest")),
    ("Sunfury", ("Bloodwarders", "BTNBloodElfPriest"), ("Warlocks", "BTNBloodElfSorceress")),
    ("Draenei", ("Vindicators", "BTNDranaiAkama"), ("Elekk Knights", "BTNDranaiMage")),
    ("Legion", ("Dreadlords", "BTNHeroDreadLord"), ("Doom Lords", "BTNDoomGuard")),
    ("FelHorde", ("Executioners", "BTNChaosGrunt"), ("Fel Death Knights", "BTNChaosWarlord")),
    ("Illidari", ("Royal Guards", "BTNNagaMyrmidonRoyalGuard"), ("Sea Witches", "BTNNagaSeaWitch")),
    ("BlackEmpire", ("Reapers", "BTNFacelessOne"), ("Heralds", "BTNFacelessOneTrickster")),
    ("Skywall", ("Elemental Lords", "BTNStormEarth&Fire"), ("Tidal Lords", "BTNSeaElemental")),
    ("Nazjatar", ("Immortal Guardians", "BTNNagaMyrmidonRoyalGuard"), ("Naga Sea Witches", "BTNNagaSeaWitch")),
    ("Ashtongue", ("Vindicators", "BTNDranaiAkama"), ("Seers", "BTNDraeneiSeer")),
    ("Zandalari", ("Boneseers", "BTNShadowHunter"), ("Direhorns", "BTNKotoBeast")),
    ("Forsaken", ("Elder Banshees", "BTNBanshee"), ("Val'kyr", "BTNValkyr")),
    ("Kezan", ("Tinkers", "BTNHeroTinker"), ("War Zeppelins", "BTNGoblinZeppelin")),
]


def elite_id(i, b):
    return ("RZe", "RZg")[b] + "123456789abcdefghijklmnopqrstuvwxyz"[i]  # A: RZe1.., B: RZg1..


def elite_research(i, b):
    key, a, bb = ELITE[i]
    title, icon = (a, bb)[b]
    other = (bb, a)[b][0]
    text = (f"Your faction can field only one elite unit: {title} (6). {other} can no longer be trained -- the "
            f"choice cannot be undone.")
    mods = [mod("gglb", 0), mod("glmb", 0), mod("gtib", 30), mod("gbpx", 2 + b), mod("gbpy", 2),
            mod("greq", ELITE_CONDITION, 1, 3), mod("gar1", ICONS + icon + ".blp", 1, 3),
            mod("gnsf", "(elite choice)", 1, 3), mod("gnam", "Elite: " + title, 1, 3),
            mod("gtp1", "Elite: " + title, 1, 3), mod("gub1", text, 1, 3)]
    return {"OldId": "Roch", "NewId": elite_id(i, b), "Unk": [0], "Modifications": mods}

# key (C# name), WL faction name, decision id, condition id, button title, extended tooltip, condition text, icon
TURNING_POINTS = [
    ("Quelthalas", "Quel'thalas", "RZd1", "RZc1", "Fallback: Sunfury",
     "Kael'thas leads the survivors of Quel'thalas to Netherstorm and the Sunfury rise: a second start as WL's Sunfury "
     "faction, serving whoever takes the Netherstorm CP. Everything left in Quel'thalas is lost.",
     "Silvermoon Palace lost", "BTNHeroBloodElfPrince"),
    ("Forsaken", "Quel'thalas", "RZdI", "RZcI", "Betrayal: Forsaken",
     "Sylvanas' spirit rises as a banshee and breaks away: you play the Forsaken in Northrend, enslaved to the Scourge, "
     "while Quel'thalas fights on in another slot under the AI.",
     "Sylvanas Windrunner slain by the Scourge", "BTNBanshee"),
    # 0.87.1: no "Gilneas" row any more (RZd2 / RZc2 free) -- Gilneas is itself a fallback (Lordaeron's), conquered
    # instead of falling back again
    ("Stormwind", "Stormwind", "RZd3", "RZc3", "Fallback: Defias Brotherhood",
     "The red masks come out of the Deadmines: you play the Defias Brotherhood -- Dunco Ratsbon and Count "
     "Clessington, bandits that pillage what they strike, recruits every turn -- sworn to whoever broke Stormwind, then "
     "to whoever holds the Deadmines. Everything left in Stormwind is lost.", "Stormwind Keep lost",
     "BTNBanditLord"),
    ("LordaeronGilneas", "Lordaeron", "RZdJ", "RZcJ", "Fallback: Gilneas",
     "Lordaeron's survivors flee to Gilneas: you play the Kingdom of Gilneas behind the Greymane Wall, with Genn "
     "Greymane, in the North Alliance -- whoever takes Gilneas City makes Gilneas bend the knee. Everything left in "
     "Lordaeron is lost.", "Capital Palace lost",
     "BTNGreymane"),
    ("Ironforge", "Ironforge", "RZd4", "RZc4", "Fallback: Dark Iron of Shadowforge",
     "A Dark Iron force tunnels up to claim the Great Anvil: you play the Dark Iron -- Anvilrage guards, bear riders, "
     "magma tanks and golems, Dagran Thaurissan on his throne, Shadowforge opens -- sworn to whoever holds the Throne "
     "Room; the Siege Foundry's holder gets the Mercenary Charter. They cannot be defeated.", "The Great Forge lost",
     "BTNHeroMountainKing"),
    ("Kultiras", "Kul'tiras", "RZd5", "RZc5", "Fallback: Kezan Enterprise",
     "Katherine Proudmoore rallies the survivors on Kezan, joined by the goblins: a mercenary faction on its own team. "
     "Kul Tiras' old holdings go to Stormwind if it still fights on your team.", "Boralus Keep lost", "BTNHeroTinker"),
    ("Druids", "Druids", "RZd6", "RZc6", "Fallback: Furbolgs",
     "The furbolg tribes follow whoever holds their Sacred Ancestral Totem: you play the Furbolgs -- Nafien and their "
     "den, Timbermaw Hold, warriors every turn, more and stronger for every world tree your side holds -- with "
     "Xavius and Peroth'arn. Your own masters while Xavius lives or the Claw of Ursoc is yours; whoever takes the "
     "Claw takes the furbolgs. They cannot be defeated.", "Nordrassil and Cenarion Hold lost", "BTNFurbolgElder"),
    ("Sentinels", "Sentinels", "RZd7", "RZc7", "Fallback: Naga of Nazjatar",
     "Queen Azshara's naga rise in Nazjatar beneath the Maelstrom: you play the Naga of Nazjatar, sworn to whoever "
     "broke the Sentinels -- then to whoever holds the Eternal Palace. Everything else of the Sentinels is lost.",
     "Auberdine and Feathermoon Stronghold lost", "BTNNagaSummoner"),
    # 0.87.1: no "The Exodar" row any more (RZd8 / RZc8 free) -- the Draenei are themselves a fallback (the Legion's),
    # conquered through the Exodar's bridge
    ("OrcishHorde", "Orcish Horde", "RZd9", "RZc9", "Fallback: Zandalari",
     "King Rastakhan swears the Zandalari trolls to your conqueror -- you play them at Zuldazar, sworn to whoever holds "
     "it.", "Orgrimmar lost", "BTNHeadHunterBerserker"),
    ("Tauren", "Tauren Tribes", "RZdA", "RZcA", "Fallback: Centaur Khans",
     "Whoever broke the Tauren is the Apex Predator of the plains: the centaur swear the Blood-Oath of the Hunt. You "
     "play the Centaur Khans -- warriors every turn at the camp by Thunder Bluff, four more khans to win -- sworn to "
     "whoever holds Thunder Bluff. They cannot be defeated.",
     "Thunder Bluff lost",
     "BTNCentaurKhan"),
    ("Scourge", "Scourge", "RZdB", "RZcB", "Fallback: Knights of the Ebon Blade",
     "The Lich King dead (if he rose) and the Frozen Throne broken: his death knights are free. You play the Knights of "
     "the Ebon Blade -- Thassarian, Koltira, Nazgrim and Thoras Trollbane, no army -- with Acherus near Icecrown, "
     "sworn to whoever holds Icecrown Citadel. The rest of the Scourge is lost.",
     "Frozen Throne lost, Lich King dead", "BTNHeroDeathKnight"),
    ("Legion", "Legion", "RZdC", "RZcC", "Fallback: Draenei",
     "The Legion beaten on Azeroth: the Exodar wakes. You play the Draenei with Prophet Velen, sworn to whoever beat "
     "the Legion -- even an old enemy -- then to whoever holds the Exodar's bridge. The ship lands near your conqueror; "
     "\"The Dimensional Ship\" makes it fly. The rest of the Legion is lost.", "Legion Stronghold lost",
     "BTNBOSSVelen"),
    # 0.89.0 (maintainer): the Dragonmaw are Twilight's fallback now; the Fel Horde gets the Consortium (later)
    ("FelHorde", "Fel Horde", "RZdD", "RZcD", "Not yet: the Consortium",
     "Rogue ethereal raiders of the Consortium will be the Fel Horde's fallback -- not built yet.",
     "Kil'sorrow Fortress destroyed", "BTNEtherealFormOn"),
    ("Twilight", "Twillight", "RZdK", "RZcK", "Fallback: Dragonmaw",
     "Cho'gall is dead and Grim Batol is lost: the Dragonmaw throw off the cult. You play the Dragonmaw Clan at "
     "Dragonmaw Port with Nek'rosh Skullcrusher and the red dragons, sworn to whoever holds the port. Everything else "
     "of the cult is lost.", "Grim Batol lost and Cho'gall dead", "BTNRedDragon"),
    ("Illidari", "Illidan", "RZdE", "RZcE", "Fallback: Ashtongue Deathsworn",
     "The Black Temple has fallen and Illidan is gone. Akama and the Ashtongue Deathsworn rise from its ruins, sworn "
     "to whoever holds the Black Temple's CP: purify Telredor, Halaar, Shattrath and Oshu'gun and restore Karabor to "
     "the Light.", "Black Temple destroyed", "BTNDranaiAkama"),
    ("Skywall", "Skywall", "RZdF", "RZcF", "Conquered, not defeated",
     "Skywall cannot lose: whoever takes the Vortex Pinnacle gains its service. Its halls cannot be destroyed.",
     "Not available", "BTNHeroAvatarOfFlame"),
    ("BlackEmpire", "Black Empire", "RZdG", "RZcG", "Fallback: Harpies",
     "With N'Zoth's tether severed the harpies of Stonetalon imprint on the banner of his slayer: you play the Harpies "
     "-- their nests, a few harpy kinds and two matriarchs -- sworn to whoever holds the caged Broodmother by the "
     "nests. They cannot be defeated.", "N'Zoth lost", "BTNHarpyQueen"),
    ("Ahnqiraj", "Ahn'qiraj", "RZdH", "RZcH", "Fallback: Skywall",
     "With C'Thun fallen the storm lords claim the Lost City and the Vortex Pinnacle: you play Skywall, sworn to "
     "C'Thun's slayer, then to whoever holds the Vortex Pinnacle. Skywall cannot be defeated. Ahn'Qiraj is lost.",
     "C'Thun lost", "BTNHeroAvatarOfFlame"),
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
    ures = ",".join([t[2] for t in TURNING_POINTS] + WL_RESEARCHES + WL_ELITE + EXTRA
                    + [elite_id(i, b) for i in range(len(ELITE)) for b in (0, 1)])
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
    lines.append("")
    lines.append("    // 0.88.0: the elite choices (choice A, choice B per faction key) and their condition (turn 10)")
    lines.append(f"    internal const int EliteCondition = {be(ELITE_CONDITION)}; // {ELITE_CONDITION}")
    for i, (key, a, b) in enumerate(ELITE):
        lines.append(f"    internal const int Elite{key}A = {be(elite_id(i, 0))}; // {elite_id(i, 0)} {a[0]}")
        lines.append(f"    internal const int Elite{key}B = {be(elite_id(i, 1))}; // {elite_id(i, 1)} {b[0]}")
    lines += ["  }", "}", ""]
    with open(CS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def main():
    write(os.path.join(MAP, "UnitData", BUILDING + ".json"), building())
    for t in TURNING_POINTS:
        write(os.path.join(MAP, "UpgradeData", t[2] + ".json"), decision(t))
        write(os.path.join(MAP, "UpgradeData", t[3] + ".json"), condition(t))
    for i in range(len(ELITE)):
        for b in (0, 1):
            write(os.path.join(MAP, "UpgradeData", elite_id(i, b) + ".json"), elite_research(i, b))
    write(os.path.join(MAP, "UpgradeData", ELITE_CONDITION + ".json"),
          {"OldId": "Roch", "NewId": ELITE_CONDITION, "Unk": [0],
           "Modifications": [mod("gnsf", "(Hourglass condition)", 1, 3), mod("gnam", "Turn 10", 1, 3)]})
    constants()
    print(f"{2 * len(ELITE)} elite choices; ", end="")
    print(f"wrote {BUILDING}, {len(TURNING_POINTS)} decisions, {len(TURNING_POINTS)} conditions, HourglassData.g.cs")


if __name__ == "__main__":
    main()
