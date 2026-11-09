"""Import objects of the 24p map into the main map: new ids (ID rule ?Za?), their dependencies, their model files.

Usage (repo root):  python3 tools/shattered-legacies/content/import_24p.py
Reads the import sets below (one per new faction), then writes
  mapdata/WarcraftLegacies/{UnitData,AbilityData,UpgradeData,ItemData,BuffData}/<new id>.json   the copies
  mapdata/WarcraftLegacies/Imports/...                 model / icon files the copies need that only the 24p map has
                                                       (Git LFS files: run `git lfs pull` after committing them)
  src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/<Set>ObjectInfo.cs   object limits (GENERATED)
  src/WarcraftLegacies.Source/ShatteredLegacies/Timelines/Factions/<Set>/<Set>Objects.g.cs  the new ids as constants
  src/WarcraftLegacies.Source/ShatteredLegacies/Content/SL_OBJECTS.md                  registry: new id | name | source
Rules: a referenced 24p object that is identical in the main map, or the same object with other data ("changed"),
is used under its main id; one that is "different" or "24p only" is copied with a new id. Stock objects stay.
Re-running gives the same ids (they are kept in tools/shattered-legacies/content/import_ids.json).
"force" (0.58) copies the roots also when "changed"; tooltip placeholders are remapped and renamed hero skills get a
name / learn title (0.60); objects dropped from a set lose their file, their id stays reserved (0.60).
HOW TO ADD A FACTION and WL's rules a copied faction must follow: tools/shattered-legacies/content/README.md.
"""
from deathwing import configure as configure_deathwing

import glob
import json
import os
import re
import shutil

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", "..", ".."))
MAIN = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
P24 = os.path.join(ROOT, "mapdata", "WarcraftLegacies24p")
KINDS = ["UnitData", "AbilityData", "UpgradeData", "ItemData", "BuffData"]
PREFIX = {"AbilityData": "A", "UpgradeData": "R", "ItemData": "I", "BuffData": "B"}
IDS_FILE = os.path.join(HERE, "import_ids.json")
SL_OBJECTS = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Content", "SL_OBJECTS.md")
SEQ = "0123456789abcdefghijklmnopqrstuvwxyz"  # one case only: Windows file names ignore case

# ------------------------------------------------------------------ import sets
# (24p id, C# name, categories, limit). Limit 0 = the object is copied but not available (the set's code unlocks it).
FORSAKEN = {
    "name": "Forsaken",
    "third": "f",  # third character of the new ids: ?Zf?
    "objects": [
        ("u010", "AltarOfTheQueen", ["Altar"], 200),
        ("u01K", "Acolyte", ["Builder"], 200),
        ("h089", "DarkMansion", ["TownHall"], 200),
        ("h08A", "GrimEstate", ["Keep"], 200),
        ("h08B", "HauntedCastle", ["Castle"], 200),
        ("h08C", "Grave", ["Farm"], 200),
        ("h08D", "ScoutTower", ["Tower"], 200),
        ("h08E", "PlagueTower", ["Tower"], 200),
        ("h08F", "ImprovedPlagueTower", ["Tower"], 200),
        ("u011", "Plaguetorium", ["Barracks"], 200),
        ("u01J", "Graveyard", ["Research"], 200),
        ("u014", "Laboratory", ["SiegeWorkshop"], 200),
        ("u016", "ForbiddenArchive", ["Magic"], 200),
        ("u017", "ForsakenDen", ["Shop"], 200),
        ("u01A", "HauntedHarbor", ["Shipyard"], 200),
        ("u01W", "RoyalSepulcrum", ["Specialist"], 1),
        ("h08S", "Undercity", ["Specialist"], 1),
        ("h08O", "Rotguard", ["Fighter"], 200),
        ("n07S", "Deadeye", ["Marksman"], 200),
        ("h08N", "Dreadguard", ["Tank"], 200),
        ("n0BY", "DarkRanger", ["Marksman"], 12),
        ("h08P", "Archivist", ["Support"], 200),
        ("u01R", "Apothecary", ["Support"], 200),
        ("n07W", "PlagueConstruct", ["Siege"], 6),
        ("u01P", "PlagueCatapult", ["Siege"], 8),
        ("o05H", "PlagueFlyer", ["Flyer"], 12),
        ("n07V", "ElderBanshee", ["Support"], 6),
        ("u01V", "Valkyr", ["Flyer"], 6),
        ("u02G", "EliteAbominations", ["Tank"], 6),
        ("U01O", "GrandApothecary", ["Support"], 1),
        ("U02I", "MasterApothecary", ["Support"], 1),
        # researches whose ids the code needs (the Forsaken quests grant them)
        ("R04X", "QuestFreeWill", None, 0),       # "Quest Completed: Forsaken Independance" -> Free Will
        ("R04Z", "QuestThroneOfLordaeron", None, 0),  # "Quest Completed: The Secret Buildup" -> Undercity
        # 0.56: Sylvanas' 24p skills, for her phase heroes (tools/shattered-legacies/content/forsaken_heroes.py)
        ("A03J", "BlackArrow", None, 0),
        ("A01O", "TrueshotAura", None, 0),
        ("A0DU", "BansheeQueen", None, 0),
    ],
    # not imported (24p heroes / mechanics the fork does differently): Sylvanas Usyl (our UZa1 + phases), Ranger-Lord,
    # Arch Dreadlord, Cultist; "Declare Forsaken Independence" (R050: breaking free is level 6 here)
    "exclude": ["Usyl", "H049", "Uvar", "u01U", "R050", "R02D"],
    # field values to drop from copies: (24p id, field, token)
    "drop": [("u010", "utra", "Usyl"), ("h08B", "ures", "R050")],
    # 0.55.1 (maintainer): lumber is "knowledge" (the tome) in this map -- won by quests. The copies cost no lumber,
    # except what is set here (24p id -> {field: value}): Tier 4 costs knowledge.
    "zero_lumber": True,
    # hand-made / main-map objects with limits in the set's ObjectInfo (main ids as written)
    "extra": [("UZa1", "SylvanasBanshee", ["Marksman", "Support"], 1),        # forsaken_heroes.py, 0.56
              ("UZa2", "SylvanasDarkRanger", ["Marksman", "Support"], 1),
              ("UZa3", "SylvanasQueen", ["Marksman", "Support"], 1),
              ("ubot", "HauntedTransportShip", [], 200)],  # 0.57.1: the main map's transport (needs Navigation)
    "set": {"R04R": {"glmb": 25, "glmm": 0, "greq": "R04X"},  # Tier 4 only after Free Will (maintainer, 0.57)
            "A0DU": {"arlv": 6, "alsk": 6},                     # her ultimate at level 6 (maintainer), not 10
            # 0.57 (maintainer): only these need Tier 4 -- Val'kyr, Plague Flyer (+ Flight), Elite Abominations,
            # Elder Banshee, the Royal Sepulcrum
            "u01V": {"ureq": "R04R"}, "o05H": {"ureq": "R04R,R09X"}, "u02G": {"ureq": "R04R"},
            "n07V": {"ureq": "R04R"}, "u01W": {"ureq": "R04R"},
            # the Haunted Harbor like every faction's: ships need Navigation (R04R of the MAIN map) -- researched at
            # the Graveyard together with Flight (R09X); no Tier 4 for the harbour itself
            "u01A": {"ureq": "RZs1"},
            "u01J": {"ures": "R02A,Rhme,Rhar,=R04R,R09X"},
            "uubs": {"ureq": "=R04R"}, "udes": {"ureq": "=R04R"},  # 0.57.1: their ships need Navigation
            "R02A": {"glmb": 10, "glmm": 0},  # 0.57.3 (maintainer): Chaos Infusion costs 10 knowledge per level  # "=" = the main map's id, not the 24p one
            # 0.57 (maintainer): the Plaguetorium is the only troop building before Sylvanas reaches level 6 (RZs1,
            # "The Banshee's Rise", forsaken_heroes.py / QuestForsakenRise)
            "u014": {"ureq": "h08A,RZs1"}, "u016": {"ureq": "h08A,RZs1"}},
}

# 0.58: Kul Tiras' fallback (TODO step 3) -- the 24p map's goblin faction ("Bilgewater", jass/Setup/FactionSetup/
# GoblinSetup.j in the 24p repository). The main map keeps WL's CREEP versions under the same ids (Area 52, Kezan's
# decoration), so the roots are copied even where the 24p object only has other data ("force").
KEZAN = {
    "name": "KezanEnterprise",
    "third": "k",
    "force": True,  # copy every root, also when the main map has an object of the same name under that id
    "objects": [
        ("o03L", "GreatHall", ["TownHall"], 200),
        ("o03M", "Stronghold", ["Keep"], 200),
        ("o03N", "Fortress", ["Castle"], 200),
        ("o03O", "AltarOfIndustry", ["Altar"], 200),
        ("o03P", "WarCamp", ["Barracks"], 200),
        ("o03Q", "LumberMill", ["Research"], 200),
        ("o03S", "Laboratory", ["Magic"], 200),
        ("o01M", "EngineersGuild", ["SiegeWorkshop"], 200),
        ("o03T", "Burrow", ["Farm"], 200),
        ("o03U", "MissileTower", ["Tower"], 200),
        ("o03W", "ImprovedMissileTower", ["Tower"], 200),
        ("o03X", "VoodooLounge", ["Shop"], 200),
        ("o03V", "GoblinDock", ["Shipyard"], 200),
        ("h011", "IntercontinentalArtillery", ["Specialist"], 1),
        ("h04Z", "OilSupply", [], 200),      # not buildable: the preplaced ones on Kezan (oil income, Oil Rigs)
        ("o04R", "OilPlatform", [], 8),      # created by code (KezanEnterpriseOil), never built
        ("o02I", "Builder", ["Builder"], 200),
        ("h08Y", "Gunner", ["Marksman"], 200),
        ("n099", "OgreMercenary", ["Fighter"], 200),
        ("h08X", "Sappers", ["Siege"], 8),
        ("o04P", "Wizard", ["Support"], 200),
        ("o04O", "Alchemist", ["Support"], 200),
        ("o04Q", "Tinker", ["Elite"], 6),
        ("n062", "Shredder", ["Tank"], 12),
        ("h08Z", "AssaultTank", ["Siege"], 5),
        ("h091", "WarZeppelin", ["Flyer"], 6),
        ("u028", "FuelTransport", ["Support"], 2),
        ("n0AQ", "OilRig", [], 8),           # the 24p expedition: trained at the Oil Supply, a platform at sea
        # 0.59 (maintainer): the Trading Center from the 24p too -- its Trading Zeppelins and Traders trade on their
        # own between it and the goblin towns (KezanEnterpriseTrade); not buildable: the preplaced one on Kezan
        ("o04M", "TradingCenter", [], 200),
        ("nzep", "TradingZeppelin", [], 16),
        ("o04S", "OilTanker", [], 10),  # 0.62: the 24p "Trader" ship, an Oil Tanker now
        ("O04N", "Gallywix", ["Support"], 1),
        ("Ntin", "Gazlowe", ["Siege"], 1),
        ("Nalc", "Noggenfogger", ["Support"], 1),
        ("R07L", "WizardTraining", None, 0),
        ("R07M", "AlchemistTraining", None, 0),
        # the 24p goblin quests' rewards (Timelines/Factions/KezanEnterprise/Quests/)
        ("R01F", "QuestExplosiveEngineering", None, 0),  # Gazlowe
        ("R07E", "QuestGadgetzan", None, 0),             # Noggenfogger
        ("R07Z", "QuestLumberMarket", None, 0),          # Shredders +500 hit points
        ("R07F", "QuestMasterOfTheSlaveTrade", None, 0), # Intercontinental Artillery (0.60: was Goblin Empire)
    ],
    # not imported: the 24p ships (the dock trains WL's Horde ships, below), Orgrimmar (o01B, the Fortress' upgrade in
    # the 24p data), Business Expansion (R07G: the dock is needed from the start -- Kezan is an island)
    # 0.60: Western Expansion (R07Y) removed (maintainer: the Sentinels are probably dead by then) -- War Zeppelins
    # need only the Fortress and the Engineer's Guild
    "exclude": ["obot", "odes", "ojgn", "o01B", "R07G", "R07Y"],
    # 0.61 (maintainer): the Oil Platforms keep the 24p's invulnerability again (0.58 dropped it)
    "drop": [("o03N", "uupt", "o01B"), ("h091", "ureq", "R07Y")],
    "zero_lumber": True,
    # main-map objects the Enterprise uses (limits): Katherine (moved to Kezan, revived at the altar), WL's Horde ships
    # (trained at the Goblin Dock)
    "extra": [("H05L", "KatherineProudmoore", ["Support"], 1),
              ("hZq0", "OilRefinery", [], 200),  # 0.61, hand-made (kezan_objects.py); its research: RZko
              ("RZko", "OilRefining", [], 200),
              ("obot", "TransportShip", [], 200), ("h0AS", "ScoutShip", [], 200), ("h0AP", "Frigate", [], 200),
              ("h0B2", "Fireship", [], 200), ("h0AY", "Galley", [], 200), ("h0B5", "BoardingVessel", [], 200),
              ("h0BC", "Juggernaut", [], 200), ("h0AO", "Bombard", [], 6)],
    "set": {"R04R": {"glmb": 25, "glmm": 0},           # the 24p Tier 4 costs knowledge, like the Forsaken's
            # the dock from the start (no Business Expansion, no Tier 4), WL's Horde ships (need Navigation)
            "o03V": {"ureq": "", "utra": "=obot,=h0AS,=h0AP,=h0B2,=h0AY,=h0B5,=h0BC,=h0AO"},
            # WL's universal researches at the Lumber Mill, like every faction's research building: Navigation (main
            # R04R) and Flight (R09X -- WL's CLI FlightMigration makes EVERY flying unit require it: the Oil Rig,
            # the Trading Zeppelin and the War Zeppelin were locked, 0.59 playtest)
            "o03Q": {"ures": "R023,Rhme,Rhar,Rosp,Rorb,=R04R,=R09X"},
            # 0.61: Oil Refining (hand-made, kezan_objects.py) is researched at the Oil Refinery now; the Oil Supply
            # only holds the oil
            # 0.59: the Trading Center's tooltip (24p: the shipyard's); the traders cost gold only (no lumber: more
            # gold instead -- 24p 15 + 80 lumber / 125 + 250 lumber)
            "o04M": {"uabi": "Avul",  # 0.61: indestructible (maintainer)
                     "utub": "The Enterprise's trading house.|n|cfff5962dTrains:|R Trading Zeppelins and Traders, "
                             "which trade on their own between it and the goblin towns and bring back gold."},
            "R07F": {"gnam": "Master of the Slave Trade"},  # 0.60: the quest that unlocks the Artillery now
            # 0.61 (maintainer): the traders need an Oil Platform (like the Engineer's Guild); the Trading Center is
            # indestructible; the Fuel Transport gets Detonate (hand-made AZq0, KezanEnterpriseBombers)
            "nzep": {"ugol": 60, "ureq": "o04R"},
            # 0.62 (maintainer): the ship is an OIL TANKER -- it carries oil from the Oil Platforms to the Oil Supply
            # (KezanEnterpriseTrade); the zeppelins trade gold with the goblin towns
            "o04S": {"ugol": 150, "ureq": "o04R", "unam": "Oil Tanker",
                     "utub": "Carries the oil of the Oil Platforms home to the Kezan Oil Supply, on its own -- it "
                             "cannot be controlled. Without tankers the platforms' oil stays at sea."},
            # 0.62: the platforms store what they pump (mana = oil; KezanEnterpriseOil) until a tanker takes it
            "o04R": {"umpm": 3000, "umpi": 0, "umpr": 0.0,
                     "utub": "Pumps oil from the sea floor into its tank until the field runs dry -- then it "
                             "collapses. Oil Tankers take the oil home to the Oil Supply."},
            "u028": {"uabi": "A0OJ,A0NY,A0MN,A0HG,=AZq0"}},
}

# 0.63: the Illidari's fallback (TODO step 3b): Akama's Ashtongue Deathsworn -- the 24p draenei ("The Exodar" in
# WL) as THEIR OWN copies ("force": the main map has WL's Exodar versions under the same ids; separate copies keep the
# two factions apart). Hand-made objects (the Temple of the Deathsworn / of Karabor, the purification techs, Purify):
# ashtongue_objects.py.
ASHTONGUE = {
    "name": "Ashtongue",
    "third": "y",
    "force": True,
    "objects": [
        ("o05B", "Defender", ["Fighter"], 200),     # the Temple of the Deathsworn
        ("h09T", "Rangari", ["Marksman"], 200),     # Telredor (purified)
        ("h09U", "ElekkKnight", ["Tank"], 12),      # Halaa (purified)
        ("o05C", "Luminarch", ["Support"], 200),    # Shattrath (purified)
        ("h09R", "Vindicator", ["Elite"], 8),       # Shattrath (purified)
        ("o05D", "Elementalist", ["Support"], 200), # Oshu'gun (purified)
        ("H0AK", "Xiri", ["Support"], 1),           # the Temple of Karabor (the final goal)
        ("H09S", "Ishanah", ["Support"], 1),        # the purifier (placed by code, never trained)
    ],
    # 0.64: the units come from the purified sites (ashtongue_objects.py -- each site a fel-tainted and a purified type),
    # so they need no requirement; Telredor's 24p building is no longer copied (only its model is used)
    "exclude": ["o053", "o051", "R04R", "R08L", "R082", "h0AG"],
    "drop": [],
    "zero_lumber": True,
    # main-map objects the Ashtongue use: the Illidari's draenei (Akama, Deathsworn, Seer, the Draenei Hut and its two
    # researches), WL's trainings of the copied Luminarch / Elementalist (their spells ask for them), the hand-made
    # temples, sites and food (ashtongue_objects.py)
    "extra": [("Naka", "Akama", ["Assassin"], 1),
              ("ndrn", "Deathsworn", ["Assassin"], 12),
              ("ndrs", "Seer", ["Support"], 6),
              ("n08W", "DraeneiHut", ["Specialist"], 200),
              ("YBPH", "DraeneiHutResearch1", [], 200), ("YBAA", "DraeneiHutResearch2", [], 200),
              ("R07C", "LuminarchTraining", [], 200), ("R078", "ElementalistTraining", [], 200),
              ("oZt0", "TempleOfTheDeathsworn", ["TownHall"], 1),
              ("oZt1", "TempleOfKarabor", ["TownHall"], 1),
              # no production categories: a bot's arrival production must not place a purified site somewhere
              ("oZt2", "ShattrathTainted", [], 1), ("oZt3", "Shattrath", [], 1),
              ("oZt4", "HalaaTainted", [], 1), ("oZt5", "Halaa", [], 1),
              ("oZt6", "TelredorTainted", [], 1), ("oZt7", "Telredor", [], 1),
              ("oZt8", "OshugunTainted", [], 1), ("oZt9", "Oshugun", [], 1),
              ("oZta", "OutlandSupply", [], 200)],
    "set": {
        "o05B": {"ureq": ""}, "h09T": {"ureq": ""}, "h09U": {"ureq": ""}, "o05C": {"ureq": ""},
        "h09R": {"ureq": ""}, "o05D": {"ureq": ""},
        "H0AK": {"upro": "Xi'ri", "ureq": ""},
        # Ishanah, High Priestess of the Aldor: the 24p's Maraad (the Exodar's legend) made a priestess -- the
        # Luminarch's model, the paladin's light skills; Purify (AZt0, autocast: the donations' on / off)
        "H09S": {"ureq": "", "upro": "Ishanah", "unam": "High Priestess of the Aldor",
                 "umdl": "war3mapImported\\DraeneiLuminary_NoGlow.mdl", "uhab": "=AHhb,=AHds,=AHad,=AHre",
                 "uabi": "=AInv,=AZt0"},
    },
}
# The Old Gods branch of the 24p Twilight player (TwilightSetup.j). The alternative
# Dragonmaw player shares some command cards in 24p, but has its own worker and heroes.
# Its preplaced buildings here provide the models and icons for the port.
TWILIGHT = {
    "name": "TwilightHammer",
    "third": "w",
    "force": True,
    "objects": [
        ("o039", "GreatHall", ["TownHall"], 200),
        ("o03A", "Stronghold", ["Keep"], 200),
        ("o03B", "Fortress", ["Castle"], 200),
        ("o03C", "AltarOfVisions", ["Altar"], 200),
        ("o03D", "WarCamp", ["Barracks"], 200),
        ("o03J", "WarMill", ["Research"], 200),
        ("o03E", "GrimTotem", ["Magic"], 200),
        ("o03F", "DragonHatchery", ["Specialist"], 200),
        ("o03I", "TwilightDock", ["Shipyard"], 200),
        ("o03G", "Lookout", ["Tower"], 200),
        ("o03H", "ImprovedLookout", ["Tower"], 200),
        ("u00Y", "VoodooLounge", ["Shop"], 200),
        ("o03K", "Burrow", ["Farm"], 200),
        ("n051", "NetherDragon", ["Flyer"], 4),
        ("o04J", "VoidRider", ["Flyer"], 8),
        ("n07X", "OgreMagi", ["Support"], 200),
        ("o01H", "Enforcer", ["Fighter"], 200),
        ("o04B", "Cultist", ["Builder"], 200),
        ("n083", "Dragonspawn", ["Tank"], 200),
        ("o04I", "Battlemaster", ["Elite"], 6),
        ("o04K", "SkullCatapult", ["Siege"], 6),
        ("n09O", "DeathKnight", ["Support"], 6),
        ("u01T", "Herald", ["Support"], 200),
        ("n087", "PhaseGrenadier", ["Marksman"], 200),
        ("O01P", "Chogall", ["Support"], 1),
        ("H08Q", "Azil", ["Support"], 1),
        ("U01S", "Feludius", ["Support"], 1),
        ("O04H", "Ignacious", ["Tank"], 1),
        ("u01Y", "Deathwing", ["Flyer"], 1),
        ("A0LN", "ThunderFists", None, 0),
        ("A024", "ThunderFistsLightning", None, 0),
        ("R05F", "Whispers", None, 0),
        ("R06Y", "CursedFortress", None, 0),
        ("R07Q", "FlameRite", None, 0),
        ("R07T", "FrostRite", None, 0),
        ("R04R", "TierFour", None, 0),
        ("R06X", "OgreTraining", None, 0),
        ("Aams", "AntiMagicShell", None, 0),  # donor version has no Banshee Training prerequisite
        ("R023", "SpiritualInfusion", None, 0),
        ("R06Z", "HeraldTraining", None, 0),
    ],
    "zero_lumber": True,
    "exclude": ["o05P", "O01Q", "O00Y", "o05K", "o05L", "odes", "obot", "ojgn", "R006", "R00C",
                "R08S"],
    "drop": [],
    "extra": [('obot', 'TransportShip', [], 12), ('h0AS', 'ScoutShip', [], 200), ('h0AP', 'Frigate', [], 12),
              ('h0B2', 'Fireship', [], 200), ('h0AY', 'Galley', [], 200), ('h0B5', 'BoardingVessel', [], 200),
              ('h0BC', 'Juggernaut', [], 6), ('h0AO', 'Bombard', [], 6)],
    "set": {
        "o03D": {"umdl": "war3mapImported\\DragonDungeon.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonDungeon.blp", "utra": "o01H,n087,o04K"},
        "o03J": {"umdl": "war3mapImported\\DragonQuarry.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonQuarry.blp", "ures": "Rosp,R023,Rhme,Rhar,=R04R,=R09X"},
        "o03E": {"umdl": "war3mapImported\\DragonArcaneTemple.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonArcaneTemple.blp"},
        "o03H": {"umdl": "war3mapImported\\DragonTower.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonTower.blp"},
        "o03G": {"umdl": "war3mapImported\\DragonTower.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonTower.blp"},
        "o03K": {"umdl": "war3mapImported\\DragonNest.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNDragonNest.blp"},
        "o03I": {"umdl": "war3mapImported\\OrcBarrackAlphaV2_01.mdl", "uico": "ReplaceableTextures\\CommandButtons\\BTNIronHordeSummoningCircle.blp", "utra": "=obot,=h0AS,=h0AP,=h0B2,=h0AY,=h0B5,=h0BC,=h0AO", "ures": "=R006,=R00C", "ureq": ""},
        "o03F": {"ures": "=Rovs"},
        "R07Q": {"gnam": "Quest Completed: Gift of the Firelord"},
        "R07T": {"gnam": "Quest Completed: Rite of Rime"},
        "R05F": {"gnam": "Quest Completed: Voices in the Quarry"},
        "U01S": {"unam": "Water Ascendant", "upro": "Feludius",
                 "umdl": r"Units\Creeps\RevenantOfTheWaves\RevenantOfTheWaves.mdl",
                 "utip": "Summon Feludius", "uawt": "Revive Feludius", "utpr": "Revive Feludius",
                 "utub": "A water ascendant of the Twilight cult. Learns Blizzard, Frost Bite, Chilling Aura and Living Tornado."},
        "o039": {"utra": "o04B,o04I"},
        "o03A": {"utra": "o04B,o04I"},
        "o03B": {"utra": "o04B,o04I"},
        "o03C": {"utra": "O01P,H08Q,O04H,U01S"},
        "o04B": {"ureq": ""},
    },
}

SETS = [FORSAKEN, KEZAN, ASHTONGUE, TWILIGHT]


def load(map_dir):
    data = {}
    for kind in KINDS:
        for f in glob.glob(os.path.join(map_dir, kind, "*.json")):
            d = json.load(open(f, encoding="utf-8-sig"))
            oid = d.get("NewId") or d.get("OldId")
            data[oid] = (kind, d)
    return data


def name_of(d):
    for m in d.get("Modifications", []):
        if m["Id"] in ("unam", "anam", "gnam", "fnam") and m.get("Level") in (None, 0, 1):
            return str(m["Value"])
    return ""


def imports(map_dir):
    base = os.path.join(map_dir, "Imports")
    out = {}
    for f in glob.glob(os.path.join(base, "**", "*"), recursive=True):
        if os.path.isfile(f):
            rel = os.path.relpath(f, base).replace("\\", "/")
            out.setdefault(os.path.splitext(rel.lower())[0], []).append(rel)
    return out


def main():
    main_data, p24 = load(MAIN), load(P24)
    imp_main, imp_24 = imports(MAIN), imports(P24)
    ids = json.load(open(IDS_FILE)) if os.path.exists(IDS_FILE) else {}
    taken = {k.lower() for k in main_data} | {v.lower() for v in ids.values()}
    registry = []

    def needs_copy(oid):
        if oid not in p24:
            return False
        if oid in main_data:
            a, b = main_data[oid][1], p24[oid][1]
            same = a.get("OldId") == b.get("OldId") and a.get("Modifications") == b.get("Modifications")
            if same or name_of(a) == name_of(b):
                return False  # identical, or the same object with other data: use the main one
        return True

    def extra_abilities(closure, exclude, todo, forced):
        """0.61: an ability used under its main id that names an object this set COPIES (the Alchemist's Acid Bomb /
        Dispel: areq R07M, the 24p's Alchemist Training -- copied as RZk1; under the main id they waited for the
        main map's R07M, another object: the training gave no spells) is copied too. Adds them to todo; False = none."""
        for c in list(closure):
            for m in p24[c][1].get("Modifications", []):
                for t in tokens(m.get("Value")):
                    if (t not in closure and t not in exclude and t not in todo and t in p24 and t in main_data
                            and p24[t][0] == "AbilityData"
                            and any(r in closure for mm in p24[t][1].get("Modifications", [])
                                    for r in tokens(mm.get("Value")))):
                        todo.append(t)
                        forced.add(t)
        return bool(todo)

    def dangling(oid):
        """0.61: an object the main map has under the same id and name (so it would be used as it is) whose 24p data
        names 24p objects the main map LACKS -- the main version cannot be what the 24p meant. Gazlowe's Engineering
        Upgrade (ANeg) swaps his skills for 24p-only abilities (A0PT ...): under its main id the hero got abilities
        that do not exist -> empty icons, the game crashed. Such an object is copied, its references with it."""
        # abilities only, and only references to other abilities (an ability that swaps / adds abilities); a wider rule
        # pulled hundreds of objects into the Forsaken set (units naming 24p-only items, sounds ...)
        if oid not in p24 or oid not in main_data or p24[oid][0] != "AbilityData":
            return False
        return any(t in p24 and p24[t][0] == "AbilityData" and t not in main_data and t not in STOCK_SAFE
                   for m in p24[oid][1].get("Modifications", []) for t in tokens(m.get("Value")))

    def tokens(value):
        if isinstance(value, str):
            for t in value.split(","):
                if len(t.strip()) == 4:
                    yield t.strip()

    for s in SETS:
        roots = [o[0] for o in s["objects"]]
        exclude = set(s["exclude"])
        todo, closure = list(roots), []
        forced = set(roots) if s.get("force") else set()
        while todo or extra_abilities(closure, exclude, todo, forced):
            oid = todo.pop(0)
            if oid in closure or oid in exclude or not (needs_copy(oid) or (oid in forced and oid in p24)
                                                        or dangling(oid)):
                continue
            closure.append(oid)
            for m in p24[oid][1].get("Modifications", []):
                todo.extend(t for t in tokens(m.get("Value")) if t not in closure)

        # new ids, stable across runs
        for oid in closure:
            key = s["name"] + ":" + oid
            if key in ids:
                continue
            kind = p24[oid][0]
            first = oid[0] if kind == "UnitData" else PREFIX[kind]
            for third in s["third"] + "ghijklmnop":
                new = next((first + "Z" + third + c for c in SEQ if (first + "Z" + third + c).lower() not in taken), None)
                if new:
                    break
            ids[key] = new
            taken.add(new.lower())
        mapping = {oid: ids[s["name"] + ":" + oid] for oid in closure}
        drops = {(a, b, c) for a, b, c in s["drop"]}

        copied_files = set()
        written = {}
        for oid in closure:
            kind, d = p24[oid]
            d = json.loads(json.dumps(d))
            d["NewId"] = mapping[oid]
            if s.get("zero_lumber"):
                for m in d.get("Modifications", []):
                    if m["Id"] in ("ulum", "glmb", "glmm"):
                        m["Value"] = 0
                if kind == "UnitData" and not any(m["Id"] == "ulum" for m in d.get("Modifications", [])):
                    d.setdefault("Modifications", []).append({"Id": "ulum", "Value": 0})
                if kind == "UpgradeData" and not any(m["Id"] == "glmb" for m in d.get("Modifications", [])):
                    d.setdefault("Modifications", []).append({"Id": "glmb", "Value": 0})
            for field, value in s.get("set", {}).get(oid, {}).items():
                hit = [m for m in d.get("Modifications", []) if m["Id"] == field]
                for m in hit:
                    m["Value"] = value
                if not hit:  # 0.57: with the type WL's JSON needs (3 = text; a string without it broke the build)
                    mod = {"Id": field, "Value": value}
                    if isinstance(value, str):
                        mod = {"Id": field, "Type": 3, "Value": value}
                    elif isinstance(value, float):
                        mod = {"Id": field, "Type": 2, "Value": value}
                    d.setdefault("Modifications", []).append(mod)
            for m in d.get("Modifications", []):
                v = m.get("Value")
                if isinstance(v, str):
                    parts = v.split(",")
                    ok = lambda p: len(p.strip()) == 4 or (p.strip().startswith("=") and len(p.strip()) == 5)
                    if all(ok(p) or p.strip() == "" for p in parts) and any(ok(p) for p in parts):
                        parts = [p for p in parts if (oid, m["Id"], p.strip()) not in drops]
                        m["Value"] = ",".join(p.strip()[1:] if p.strip().startswith("=") else mapping.get(p.strip(), p.strip())
                                              for p in parts)
                    else:
                        v = remap_placeholders(v, oid, mapping, main_data, p24)
                        files, new_value = copy_import(v, imp_main, imp_24)
                        copied_files |= files
                        m["Value"] = new_value
            if kind == "AbilityData":
                learn_title(d)
            if s is TWILIGHT:
                configure_deathwing(d)
            written[oid] = d
            out = os.path.join(MAIN, kind, mapping[oid] + ".json")
            with open(out, "w", encoding="utf-8", newline="\n") as f:
                f.write(json.dumps(d, indent=2, ensure_ascii=False))
            registry.append((mapping[oid], name_of(d), "24p:" + oid, s["name"]))

        # 0.60: objects dropped from the set (an id in import_ids.json, no longer in the closure): their files go, the id
        # stays reserved (never reused -- a saved game or an old note may name it)
        for key, new in ids.items():
            st, old = key.split(":", 1)
            if st == s["name"] and old not in closure:
                for kind in KINDS:
                    gone = os.path.join(MAIN, kind, new + ".json")
                    if os.path.exists(gone):
                        os.remove(gone)
                        print(f"  {s['name']}: removed {kind}/{new}.json ({old} is no longer in the set)")

        # 0.55.1: every research the set's buildings offer needs an object limit (WL records research levels in the
        # faction and refuses levels above its limit: "level (1) cannot be higher than the object limit for Tier 4")
        researches = []  # from the copies as written (ids already the main map's)
        for oid in closure:
            if p24[oid][0] != "UnitData":
                continue
            for m in written[oid].get("Modifications", []):
                if m["Id"] == "ures" and isinstance(m.get("Value"), str):
                    for t in m["Value"].split(","):
                        t = t.strip()
                        if len(t) == 4 and t not in researches:
                            researches.append(t)
        s["researches"] = researches
        write_object_info(s, mapping)
        # 0.58: force-copied roots whose id also exists in the main map (WL's creep version): main id -> copy, so
        # preplaced main-map units can be replaced by the faction's copies (StartArea.Take)
        # (only WL's creep versions -- "(Creep ...)" -- not other objects under the same id, e.g. u028 / o04P)
        s["equivalents"] = [(o, mapping[o]) for o in roots if s.get("force") and o in mapping and o in main_data
                            and main_data[o][0] == "UnitData" and "creep" in suffix_of(main_data[o][1]).lower()]
        write_constants(s, mapping)
        print(f"{s['name']}: {len(closure)} objects copied, {len(copied_files)} import files copied")

    with open(IDS_FILE, "w") as f:
        json.dump(ids, f, indent=1, sort_keys=True)
    lines = ["# Objects of the fork (GENERATED -- `tools/shattered-legacies/content/import_24p.py`, do not edit)", "",
             "Every object the fork copied into the main map. Hand-made objects (`nZh0`, `UZa1`, the Hourglass",
             "researches ...) are listed in `../README.md` \"Changes to map data\".", "",
             "| New id | Name | Source | Used by |", "|---|---|---|---|"]
    lines += [f"| `{a}` | {b.replace('|', '/')} | `{c}` | {d} |" for a, b, c, d in sorted(registry)]
    with open(SL_OBJECTS, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines) + "\n")


STOCK_SAFE = set()  # ids never treated as dangling (none yet)
PLACEHOLDER = re.compile(r"<([A-Za-z0-9]{4})((?::[A-Za-z0-9]{4})?,)")


def remap_placeholders(text, oid, mapping, main_data, p24):
    """0.60: tooltip placeholders "<A03Z,DataC1>" name an object id -- the 24p's. A copied object -> its new id;
    a stock id (in neither map's data) or a main-map object of the same name stays; anything else (a 24p object that is
    not copied, or a different main-map object under that id, of the same kind) -> the object's own new id, as does
    the copy's own stock base: such a tooltip showed nothing (Plague Bomb's damage, 0.59 playtest) or another
    object's numbers. Stock ids and other kinds (a unit in an ability's tooltip) stay. A ":base" part ("<A03Z:ANab,...>",
    24p only, not a format the game reads -- WL never uses it) is dropped."""
    kind, own = p24[oid][0], p24[oid][1]
    base = own.get("OldId")

    def fix(mt):
        ref = mt.group(1)
        if ref in mapping:
            return "<" + mapping[ref] + ","
        if ref == base and ref != oid:
            return "<" + mapping[oid] + ","  # the stock base of this copy: the copy's own values
        if ref not in p24 or p24[ref][0] != kind:
            return "<" + ref + ","  # stock, or another kind of object (a unit's hit points in a summon's tooltip)
        if ref in main_data and name_of(main_data[ref][1]) == name_of(p24[ref][1]):
            return "<" + ref + ","  # the same object in the main map
        return "<" + mapping[oid] + ","
    return PLACEHOLDER.sub(fix, text)


LEVEL_SUFFIX = re.compile(r"\s*-\s*\[.*$")


def learn_title(d):
    """0.60: a hero skill the 24p renamed only in its tooltips (atp1 "Plague Bomb - [Level 1]", no anam / aret)
    showed the stock name in the learn button -- in a German client "Saeurebombe". Name and learn title from atp1."""
    mods = d.get("Modifications", [])
    have = {m["Id"] for m in mods}
    if "arut" not in have:
        return  # not a learnable hero skill
    tp = next((m["Value"] for m in mods if m["Id"] == "atp1" and m.get("Level") in (None, 0, 1)), None)
    if not isinstance(tp, str) or not tp.strip():
        return
    name = LEVEL_SUFFIX.sub("", tp).strip()
    if not name:
        return
    if "anam" not in have:
        mods.append({"Id": "anam", "Type": 3, "Value": name})
    if "aret" not in have:
        mods.append({"Id": "aret", "Type": 3, "Value": "Learn " + name + " - [|cffffcc00Level %d|r]"})


def suffix_of(d):
    for m in d.get("Modifications", []):
        if m["Id"] == "unsf":
            return str(m["Value"])
    return ""


def safe_name(rel):
    """WL #4190: imports with more than one dot in the name do not show in game -> dots of the stem become _."""
    folder, name = os.path.split(rel)
    stem, ext = os.path.splitext(name)
    return os.path.join(folder, stem.replace(".", "_") + ext).replace("\\", "/")


def copy_import(value, imp_main, imp_24):
    """Copies the import a path value points to (and files named like it: portrait, disabled icon) if only 24p has it.
    Returns (copied files, the value pointing to the copy). Models that are real files (after `git lfs pull`) also
    bring the textures they use."""
    copied = set()
    p = value.replace("\\", "/").lower()
    if not re.search(r"\.(mdl|mdx|blp|tga)$", p) and "/" not in p:
        return copied, value
    stem = os.path.splitext(p)[0]
    if stem in imp_main or stem not in imp_24:
        return copied, value
    folder, base = os.path.split(stem)
    candidates = [k for k in imp_24 if k == stem or (os.path.split(k)[0] == folder and os.path.split(k)[1].startswith(base + "_"))]
    if "commandbuttons/btn" in stem:
        dis = stem.replace("commandbuttons/btn", "commandbuttonsdisabled/disbtn")
        candidates += [k for k in imp_24 if k == dis]
    for k in candidates:
        for rel in imp_24[k]:
            copied |= copy_file(rel, imp_main, imp_24)
    # the value points to the renamed copy (same extension as written in the data)
    v_folder, v_name = os.path.split(value.replace("\\", "/"))
    v_stem, v_ext = os.path.splitext(v_name)
    new_value = (v_folder + "/" if v_folder else "") + v_stem.replace(".", "_") + v_ext
    return copied, new_value.replace("/", "\\")


def copy_file(rel, imp_main, imp_24):
    copied = set()
    dst_rel = safe_name(rel)
    if os.path.splitext(dst_rel.lower())[0] in imp_main:
        return copied
    src = os.path.join(P24, "Imports", rel)
    dst = os.path.join(MAIN, "Imports", dst_rel)
    os.makedirs(os.path.dirname(dst), exist_ok=True)
    shutil.copyfile(src, dst)
    copied.add(dst_rel)
    for tex in textures(src):
        key = os.path.splitext(tex.replace("\\", "/").lower())[0]
        if key in imp_24 and key not in imp_main:
            for trel in imp_24[key]:
                tdst = os.path.join(MAIN, "Imports", trel)  # textures keep their names (the model names them)
                if not os.path.exists(tdst):
                    os.makedirs(os.path.dirname(tdst), exist_ok=True)
                    shutil.copyfile(os.path.join(P24, "Imports", trel), tdst)
                    copied.add(trel)
    return copied


def textures(path):
    """Texture paths a model uses -- only for real files (a Git LFS pointer has no content to read)."""
    data = open(path, "rb").read()
    if data.startswith(b"version https://git-lfs"):
        return []
    if data[:4] == b"MDLX":
        out, pos = [], 4
        while pos + 8 <= len(data):
            tag, size = data[pos:pos + 4], int.from_bytes(data[pos + 4:pos + 8], "little")
            if tag == b"TEXS":
                for e in range(pos + 8, pos + 8 + size, 268):
                    name = data[e + 4:e + 264].split(b"\0")[0].decode("latin1")
                    if name:
                        out.append(name)
            pos += 8 + size
        return out
    return re.findall(r'Image\s+"([^"]+)"', data.decode("latin1", "replace"))


def fourcc(code):
    return int.from_bytes(code.encode("latin1"), "big", signed=True)


def write_object_info(s, mapping):
    path = os.path.join(ROOT, "src", "WarcraftLegacies.Shared", "ShatteredLegacies", "FactionObjectLimits",
                        s["name"] + "ObjectInfo.cs")
    extra = getattr_extra(s)
    lines = [f"// <auto-generated> tools/shattered-legacies/content/import_24p.py -- object limits of the {s['name']}",
             "// (from its import set + the hand-made objects listed there). Do not edit. </auto-generated>",
             "using MacroTools.Shared;", "using static MacroTools.Shared.UnitCategory;", "",
             "namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits", "{",
             f"  public static class {s['name']}ObjectInfo", "  {",
             f"    public static IEnumerable<ObjectInfo> GetAllObjectLimits()", "    {"]
    for code, name, cats, limit in extra + [(mapping.get(o, o), n, c, l) for o, n, c, l in s["objects"]]:
        if cats is None:
            continue
        if not cats:
            lines.append(f"      yield return new({fourcc(code)}, {limit}); // {code} {name}")
            continue
        cat = cats[0] if len(cats) == 1 else "new List<UnitCategory> { " + ", ".join(cats) + " }"
        lines.append(f"      yield return new({fourcc(code)}, {limit}, {cat}); // {code} {name}")
    for code in s.get("researches", []):
        lines.append(f"      yield return new({fourcc(code)}, 200); // {code} research")
    lines += ["    }", "  }", "}", ""]
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


def getattr_extra(s):
    # 0.58: objects of the set that are not imported (hand-made, or main-map objects the faction uses): "extra"
    return s.get("extra", [])


def write_constants(s, mapping):
    path = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "ShatteredLegacies", "Timelines", "Factions", s["name"],
                        s["name"] + "Objects.g.cs")
    lines = [f"// <auto-generated> tools/shattered-legacies/content/import_24p.py -- ids of the {s['name']}' objects",
             "// copied from the 24p map. Do not edit. </auto-generated>",
             f"namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.{s['name']}", "{",
             f"  internal static class {s['name']}Objects", "  {"]
    for o, n, _, _ in s["objects"]:
        code = mapping.get(o, o)
        lines.append(f"    internal const int {n} = {fourcc(code)}; // {code} (24p {o})")
    if s.get("equivalents"):
        lines += ["", "    // main-map id (WL's creep version under the 24p id) -> the faction's copy, in pairs",
                  "    internal static int[] MainEquivalents =", "    {"]
        lines += [f"      {fourcc(m)}, {fourcc(c)}, // {m} -> {c}" for m, c in s["equivalents"]]
        lines += ["    };"]
    lines += ["  }", "}", ""]
    os.makedirs(os.path.dirname(path), exist_ok=True)  # 0.63: a new faction's folder
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(lines))


if __name__ == "__main__":
    main()
