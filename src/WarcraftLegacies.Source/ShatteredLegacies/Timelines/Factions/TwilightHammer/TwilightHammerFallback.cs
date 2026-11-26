using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal static class TwilightHammerFallback
{
  internal const float StartX = 22080f;
  internal const float StartY = -3904f;

  // 0.80.0 (maintainer): one path, two successors -- beaten by an Old God: Twilight (Old Gods' team); by anyone else:
  // the Dragonmaw Clan (Factions/Dragonmaw/DragonmawFallback.cs). Both start at Dragonmaw Port, so only one ever comes.
  internal static readonly ExclusivePath Path = new("Twillight or Dragonmaw", Prepare, Run, StartX, StartY);
  internal const string OfferDescription = "Beaten by an Old God: join Cho'gall at Dragonmaw Port as Twillight. Beaten by "
                                           + "anyone else: the Dragonmaw Clan rises there, sworn to whoever holds the port.";
  internal static List<unit> Port => _port!;

  // 0.81.0 (maintainer): the ORCISH HORDE has its own route -- beaten by an Old God: Twilight (this path); by anyone
  // else: the Zandalari Empire (Factions/Zandalari, "Rastakhan swears allegiance to the destroyers of the Horde").
  // No Dragonmaw for the Horde any more (the Fel Horde keeps Twilight / Dragonmaw). Each destination is claimed
  // (one faction each); when the wanted one is taken the other one is used.
  internal static readonly ExclusivePath HordeRoute = new("Twillight or Zandalari", () => { }, RunHorde, StartX, StartY);
  internal const string HordeOfferDescription = "Beaten by an Old God: join Cho'gall at Dragonmaw Port as Twillight. "
                                                + "Beaten by anyone else: King Rastakhan swears the Zandalari trolls "
                                                + "to your conqueror -- you play them at Zuldazar.";

  private static void RunHorde(player p, TurningPointDef def)
  {
    var conqueror = Dragonmaw.Conquerors.Of(def.Faction);
    var oldGods = Dragonmaw.Conquerors.ByOldGods(def.Faction);
    if (oldGods && Path.TryClaim())
    {
      RunTwilight(p);
    }
    else if (Zandalari.ZandalariFallback.Path.TryClaim())
    {
      Zandalari.ZandalariFallback.FromHorde(p, conqueror);
    }
    else if (Path.TryClaim())
    {
      RunTwilight(p); // the trolls are taken (Ahn'Qiraj): Twilight after all
    }
  }

  // Only these neutral buildings belong to the start. The portal, the capital and player holdings stay in play.
  private static readonly int[] PortBuildings =
  {
    UNIT_O067_DUNGEON_CREEP_BARRACKS, TwilightHammerObjects.WarCamp,
    UNIT_O068_QUARRY_CREEP_RESEARCH, TwilightHammerObjects.WarMill,
    UNIT_O069_DARK_CONCLAVE_CREEP_MAGIC, TwilightHammerObjects.GrimTotem,
    UNIT_O06A_IMPROVED_TERRITORIAL_DRAKE_CREEP_TOWER_2, TwilightHammerObjects.ImprovedLookout,
    UNIT_O06B_TERRITORIAL_DRAKE_CREEP_TOWER, TwilightHammerObjects.Lookout,
    UNIT_O06C_NEST_CREEP_FARM, TwilightHammerObjects.Burrow,
    UNIT_O06D_SHIPYARD_CREEP_SHIPYARD, TwilightHammerObjects.TwilightDock
  };
  private static List<unit>? _port;

  // 0.74.0 (maintainer): a starting army like the Scarlet Crusade's (Tyr's Hand: ~45 units and 5 workers) --
  // Cho'gall and 5 Cultists plus 38 of the Twilight's Hammer's own units, beside the Great Hall
  private static readonly int[] ArmyTypes =
  {
    TwilightHammerObjects.Enforcer, TwilightHammerObjects.PhaseGrenadier, TwilightHammerObjects.Dragonspawn,
    TwilightHammerObjects.OgreMagi, TwilightHammerObjects.Herald, TwilightHammerObjects.DeathKnight,
    TwilightHammerObjects.Battlemaster, TwilightHammerObjects.VoidRider
  };
  private static readonly int[] ArmyCounts = { 10, 8, 6, 4, 4, 2, 2, 2 };

  private static int[] StartArmy()
  {
    var list = new List<int>();
    for (var k = 0; k < ArmyTypes.Length; k++)
    {
      for (var i = 0; i < ArmyCounts[k]; i++)
      {
        list.Add(ArmyTypes[k]);
      }
    }

    return list.ToArray();
  }

  private static List<unit>? _highlandBuildings;

  internal static void Prepare()
  {
    if (_port != null)
    {
      return;
    }

    _port = PrepareBuildings(Regions.DragonmawUnlock);
    _highlandBuildings = PrepareBuildings(new Rectangle(16384, -3904, 18240, -2176));

    var mouths = AllPreplacedWidgets.Units;
    var capital = mouths.Get(UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT);
    if (capital.Owner == player.NeutralPassive)
    {
      capital.IsInvulnerable = true;
    }
    var hill = mouths.GetClosest(UNIT_N08R_GRIM_BATOL_TUNNELS, 16760, -2484);
    var foot = mouths.GetClosest(UNIT_N08R_GRIM_BATOL_TUNNELS, 16568, -2772);
    OpenTunnel(hill, 16569, -2836);
    OpenTunnel(foot, 16761, -2420);
    // 0.80.0: who takes Orgrimmar / Kil'sorrow Fortress decides between Twilight and the Dragonmaw
    Dragonmaw.Conquerors.Watch(AllPreplacedWidgets.Units.Get(UNIT_O01B_ORGRIMMAR_WARSONG), "Orcish Horde");
    Dragonmaw.Conquerors.Watch(AllLegends.FelHorde.KilsorrowFortress.Unit, "Fel Horde");
  }

  private static void OpenTunnel(unit mouth, float exitX, float exitY)
  {
    SetUnitScale(mouth, 1, 1, 1);
    WaygateSetDestination(mouth, exitX, exitY);
    WaygateActivate(mouth, true);
  }

  private static List<unit> PrepareBuildings(Rectangle region) =>
    region.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures, u =>
    {
      for (var i = 0; i < PortBuildings.Length; i += 2)
      {
        if (u.UnitType == PortBuildings[i])
        {
          // The replacement is a fresh playable object and loses this protection on rescue.
          UnitAddAbility(u, FourCC("Avul"));
          return true;
        }
      }
      return false;
    });

  internal static void ClaimHighlands(player owner, Faction faction) =>
    StartArea.Take(owner, _highlandBuildings!, faction, PortBuildings);

  internal static void Run(player p, TurningPointDef def)
  {
    if (!Dragonmaw.Conquerors.ByOldGods(def.Faction))
    {
      Dragonmaw.DragonmawFallback.FromHorde(p, Dragonmaw.Conquerors.Of(def.Faction));
      return;
    }

    RunTwilight(p);
  }

  private static void RunTwilight(player p)
  {
    var keep = new List<unit>();
    var hourglass = Hourglass.Get(GetPlayerId(p));
    if (hourglass != null)
    {
      keep.Add(hourglass);
    }

    var twilight = new TwilightHammerFaction();
    SwapFaction.Run(p, twilight);
    LeaveBehind.RemoveHeroes(p, keep);
    LeaveBehind.ToNeutralHostile(p, keep);
    ChangeTeam.JoinOrAlone(p, TeamSetup.OldGods);
    SplitFaction.UnallyOutsideTeam(p);

    StartArea.Take(p, _port!, twilight, PortBuildings);
    StartCps.Give(p, UNIT_N016_WETLANDS);
    StartCps.Force(p, UNIT_N08T_DRAGONMAW_PORT); // 0.79.0: always theirs on arrival (maintainer)
    CreateUnit(p, TwilightHammerObjects.GreatHall, 22464f, -3264f, 270f);
    CreateUnit(p, TwilightHammerObjects.AltarOfVisions, 22016f, -3584f, 270f);
    var worker = TwilightHammerObjects.Cultist;
    Spawn.Units(p, StartX, StartY, new[] { worker, worker, worker, worker, worker, TwilightHammerObjects.Chogall });
    Spawn.Units(p, StartX + 176f, StartY - 304f, StartArmy()); // open ground south-east of the start (PathingMap)
    twilight.AddCampaignQuests();
    p.RepositionCamera(new Point(StartX, StartY));
    for (var i = 0; i < 24; i++)
    {
      DisplayTimedTextToPlayer(Player(i), 0, 0, 20, "|cffffcc00An Old God claims the Horde's survivors.|r Cho'gall "
                                                   + "rallies Twilight's Hammer at Dragonmaw Port.");
    }

    Dragonmaw.DragonmawFallback.WatchTwilight(p); // 0.80.0: Grim Batol lost while Cho'gall is dead -> the Dragonmaw
  }
}
