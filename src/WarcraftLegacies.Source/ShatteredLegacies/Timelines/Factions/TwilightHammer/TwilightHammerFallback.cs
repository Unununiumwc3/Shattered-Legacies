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

  // 0.80.0 - 0.87.0: one path, two successors (Twilight or the Dragonmaw at Dragonmaw Port) for the Orcish / Fel Horde.
  // 0.89.0 (maintainer): TWILIGHT IS A START FACTION (P20, TwilightStart) and the Dragonmaw are ITS fallback: Grim Batol
  // lost while Cho'gall is dead -> the Dragonmaw Clan rises at Dragonmaw Port (their base waits there, invulnerable and
  // hidden, from the map start -- Twilight cannot use it), sworn to whoever holds the port. The Fel Horde has no
  // fallback for now (the Consortium comes later).
  internal static readonly ExclusivePath Path = new("Dragonmaw", Prepare, Run, StartX, StartY);
  internal const string OfferDescription = "Cho'gall is dead and Grim Batol is lost: the Dragonmaw throw off the cult. "
                                           + "You play the Dragonmaw Clan at Dragonmaw Port with its red dragons -- "
                                           + "sworn to whoever holds the port. Everything else of the cult is lost.";
  internal static List<unit> Port => _port!;

  // 0.81.0 (maintainer): the ORCISH HORDE has its own route -- beaten by an Old God: Twilight (this path); by anyone
  // else: the Zandalari Empire (Factions/Zandalari, "Rastakhan swears allegiance to the destroyers of the Horde").
  // No Dragonmaw for the Horde any more (the Fel Horde keeps Twilight / Dragonmaw). Each destination is claimed
  // (one faction each); when the wanted one is taken the other one is used.
  // 0.85.0: Ahn'Qiraj has Skywall now -- the Zandalari base waits for the Horde alone (its Prepare runs here)
  // 0.87.0 (maintainer): TROLLS ONLY -- whoever beat the Horde (an Old God too) gets the Zandalari; no Twilight
  internal static readonly ExclusivePath HordeRoute = new("Zandalari",
    () => Zandalari.ZandalariFallback.Path.Prepare(), RunHorde, StartX, StartY);
  internal const string HordeOfferDescription = "King Rastakhan swears the Zandalari trolls to your conqueror -- you "
                                                + "play them at Zuldazar.";

  private static void RunHorde(player p, TurningPointDef def)
  {
    if (Zandalari.ZandalariFallback.Path.TryClaim())
    {
      Zandalari.ZandalariFallback.FromHorde(p, Dragonmaw.Conquerors.Of(def.Faction));
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
    // who takes Orgrimmar (the Zandalari's master); 0.89.0: who kills Cho'gall (the Dragonmaw's, if Grim Batol is free)
    Dragonmaw.Conquerors.Watch(AllPreplacedWidgets.Units.Get(UNIT_O01B_ORGRIMMAR_WARSONG), "Orcish Horde");
    WatchChogall();
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

  // Twilight -> the Dragonmaw: the conqueror is Grim Batol's holder, else Cho'gall's killer
  internal static void Run(player p, TurningPointDef def)
  {
    var batol = AllPreplacedWidgets.Units.Get(UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT);
    var holder = batol != null && GetUnitTypeId(batol) != 0 && GetUnitState(batol, UNIT_STATE_LIFE) > 0.405f
      ? GetOwningPlayer(batol)
      : null;
    var conqueror = holder != null && GetPlayerId(holder) < 24 && holder != p ? holder : ChogallKiller;
    Dragonmaw.DragonmawFallback.FromTwilight(p, conqueror);
  }

  // who killed Cho'gall last (a player)
  internal static player? ChogallKiller { get; private set; }

  private static player? _lastChogallDeathOwner;

  private static void WatchChogall()
  {
    var death = CreateTrigger();
    for (var i = 0; i < 24; i++)
    {
      TriggerRegisterPlayerUnitEvent(death, Player(i), EVENT_PLAYER_UNIT_DEATH, null);
    }

    TriggerAddAction(death, () =>
    {
      var killer = GetKillingUnit();
      if (GetUnitTypeId(GetTriggerUnit()) != TwilightHammerObjects.Chogall)
      {
        return;
      }

      _lastChogallDeathOwner = GetOwningPlayer(GetTriggerUnit());
      if (killer != null && GetPlayerId(GetOwningPlayer(killer)) < 24)
      {
        ChogallKiller = GetOwningPlayer(killer);
      }
    });
  }

  // the turning point's prerequisite: Cho'gall is dead (Grim Batol lost is its HQ condition)
  internal static bool ChogallDead(player p)
  {
    if (_lastChogallDeathOwner != p)
    {
      return false; // An untrained hero has never died.
    }

    var g = CreateGroup();
    GroupEnumUnitsOfPlayer(g, p, null);
    var alive = false;
    unit u;
    while ((u = FirstOfGroup(g)) != null)
    {
      GroupRemoveUnit(g, u);
      if (!alive && GetUnitTypeId(u) == TwilightHammerObjects.Chogall && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
      {
        alive = true;
      }
    }

    DestroyGroup(g);
    return !alive;
  }
}
