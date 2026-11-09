using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal static class TwilightHammerFallback
{
  internal const float StartX = 22080f;
  internal const float StartY = -3904f;

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
    StartCps.Give(p, UNIT_N016_WETLANDS, UNIT_N08T_DRAGONMAW_PORT);
    CreateUnit(p, TwilightHammerObjects.GreatHall, 22464f, -3264f, 270f);
    CreateUnit(p, TwilightHammerObjects.AltarOfVisions, 22016f, -3584f, 270f);
    var worker = TwilightHammerObjects.Cultist;
    Spawn.Units(p, StartX, StartY, new[] { worker, worker, worker, worker, TwilightHammerObjects.Chogall });
    twilight.AddCampaignQuests();
    p.RepositionCamera(new Point(StartX, StartY));
  }
}
