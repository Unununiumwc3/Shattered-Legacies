"""Adapt the donor dragon and its three spells to this map's hero progression."""
from copy import deepcopy


def set_field(obj, key, value, level=0):
    mods = obj["Modifications"]
    for mod in mods:
        if mod["Id"] == key and mod.get("Level", 0) == level:
            mod["Value"] = value
            return
    mod = {"Id": key, "Value": value}
    if level:
        mod["Level"] = level
    if isinstance(value, str):
        mod["Type"] = 3
    elif isinstance(value, float):
        mod["Type"] = 2
    mods.append(mod)


def configure(obj):
    raw = obj["NewId"]
    if raw == "UZwx":
        # Use a native hero parent and explicitly restore the dragon's flying combat profile.
        obj["OldId"] = "Udre"
        fields = {
            "uabi": "AInv,ACrk", "uhab": "AZwe,AZwd,AZwp,AZwf,Aamk", "upro": "Deathwing",
            "upor": r"war3mapImported\Deathwing.mdl", "upru": 1, "upra": "STR", "ustr": 32, "ustp": 3.5, "uagi": 15, "uagp": 1.4,
            "uint": 24, "uinp": 2.5, "umpm": 450, "umpi": 450, "uhpm": 1750,
            "umvt": "fly", "umvh": 325.0, "umvf": 150.0, "utar": "air", "uaen": 3,
            "ua1w": "missile", "ua2w": "missile", "ua1r": 500, "ua2r": 500,
            "ua1g": "ground,structure,debris,item,ward", "ua2g": "air", "ua1c": 3.0,
            "ua2c": 3.0, "ua2b": 98, "ua2d": 2, "ua2s": 6, "ua2z": 900, "ua1z": 900,
            "ua2m": r"Abilities\Weapons\RedDragonBreath\RedDragonMissile.mdl",
            "utip": "Deathwing", "utub": "Flying strength hero. Learns Breath of Fire, Flame Strike, Command Aura and Searing Breath.",
        }
        for key, value in fields.items():
            set_field(obj, key, value)
        return
    spells = {
        "AZwe": ("Breath of Fire", "Q", 0, {"Ucs1": [150., 225., 300., 375.], "Nbf5": [10., 15., 20., 25.]}),
        "AZwd": ("Flame Strike", "W", 1, {"Hfs1": [15., 25., 35., 45.], "Hfs3": [10., 15., 20., 25.]}),
        "AZwp": ("Command Aura", "E", 2, {"Cac1": [0.10, 0.15, 0.20, 0.25]}),
        "AZwf": ("Searing Breath", "R", 3, {"Hfs1": [10., 20., 30.], "Hfs3": [2., 4., 6.]}),
    }
    if raw not in spells:
        return
    name, hotkey, column, data = spells[raw]
    count = len(next(iter(data.values())))
    for key, value in {"aher": 1, "alev": count, "arlv": 6 if column == 3 else 1,
                       "alsk": 5 if column == 3 else 2, "arhk": hotkey, "ahky": hotkey,
                       "arpx": column, "arpy": 0, "abpx": column, "abpy": 2,
                       "aret": "Learn " + name + " - [Level %d]", "anam": name}.items():
        set_field(obj, key, value)
    # Duplicate explicit donor overrides for every rank before applying rank-specific numbers.
    for mod in list(obj["Modifications"]):
        if mod.get("Level") == 1:
            for rank in range(2, count + 1):
                if not any(other["Id"] == mod["Id"] and other.get("Level") == rank for other in obj["Modifications"]):
                    obj["Modifications"].append(dict(deepcopy(mod), Level=rank))
    icon = {0: "BTNFireForTheCannon", 1: "BTNWallOfFire", 2: "BTNCommand", 3: "PASMoltenBoulder"}[column]
    set_field(obj, "arar", "ReplaceableTextures\\CommandButtons\\" + icon + ".blp")
    descriptions = []
    for rank in range(1, count + 1):
        for key, values in data.items():
            set_field(obj, key, values[rank - 1], rank)
        first, *rest = [values[rank - 1] for values in data.values()]
        if column == 0:
            text = f"Deals {first:g} cone damage and {rest[0]:g} burn damage per second."
        elif column == 1:
            text = f"Burns the target area for {first:g} damage per interval, then {rest[0]:g} lingering damage per interval."
        elif column == 2:
            text = f"Nearby friendly units gain {round(first * 100)}% attack damage."
        else:
            text = f"Attacks ignite the ground for {first:g} initial and {rest[0]:g} lingering damage per interval."
        set_field(obj, "atp1", f"{name} - Level {rank}", rank)
        set_field(obj, "aub1", text, rank)
        descriptions.append(f"Level {rank}: {text}")
    set_field(obj, "arut", "|n".join(descriptions))
