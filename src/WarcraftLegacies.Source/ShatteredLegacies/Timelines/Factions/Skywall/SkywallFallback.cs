using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Factions.Skywall;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Skywall;

internal static class SkywallFallback
{
  private static readonly Point Start = new(-12352f, -19520f);
  internal static readonly ExclusivePath Path = new("Skywall", Prepare, Run, Start.X, Start.Y);
  private static List<unit> _camp = new();

  private static void Prepare()
  {
    var camp = new Rectangle(-12544f, -19712f, -11904f, -18752f);
    _camp = camp.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures,
      u => u.UnitType == UNIT_N05Q_HOLDFAST_SKYWALL_T1 || u.UnitType == UNIT_ELFA_BAYT_SKYWALL_FARM
        || u.UnitType == UNIT_LS05_SHAPER_SKYWALL_WORKER);
    // These neutral units were reserved by Skywall's quests at registration in 0.68.
    Regions.Tempest_Rain.PrepareUnitsForRescue(RescuePreparationMode.HideAll, u => u.UnitType != UNIT_NGME_GOBLIN_MERCHANT);
    Regions.ThroneoftheFourWind.PrepareUnitsForRescue(RescuePreparationMode.HideAll, u => u.UnitType != UNIT_NGME_GOBLIN_MERCHANT);
  }

  private static void Run(player p, TurningPointDef offer)
  {
    var keep = new List<unit>();
    var glass = Hourglass.Get(GetPlayerId(p));
    if (glass != null)
    {
      keep.Add(glass);
    }

    LeaveBehind.RemoveHeroes(p, keep);
    LeaveBehind.ToNeutralHostile(p, keep);
    SwapFaction.Run(p, new SkywallFaction());
    foreach (var unit in _camp)
    {
      if (unit.Alive && unit.Owner == player.NeutralPassive)
      {
        unit.Rescue(p);
      }
    }
    StartCps.Give(p, UNIT_N0BK_LOST_CITY_OF_THE_TOL_VIR);
    p.RepositionCamera(Start);
  }
}
