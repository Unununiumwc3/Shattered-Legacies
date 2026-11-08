using System.Collections.Generic;
using MacroTools.Extensions;
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

  internal static void Prepare()
  {
    _port ??= Regions.DragonmawUnlock.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures, u =>
    {
      for (var i = 0; i < PortBuildings.Length; i += 2)
      {
        if (GetUnitTypeId(u) == PortBuildings[i])
        {
          return true;
        }
      }
      return false;
    });
  }

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
