using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
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
  private static List<unit> _vortex = new();
  private static player _conqueror;
  private static bool _lossRecorded;

  private static void Prepare()
  {
    var camp = new Rectangle(-12544f, -19712f, -11904f, -18752f);
    _camp = camp.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures,
      u => u.UnitType == UNIT_N05Q_HOLDFAST_SKYWALL_T1 || u.UnitType == UNIT_ELFA_BAYT_SKYWALL_FARM
        || u.UnitType == UNIT_LS05_SHAPER_SKYWALL_WORKER);
    _vortex = Regions.Tempest_Rain.PrepareUnitsForRescue(RescuePreparationMode.HideAll,
      u => u.UnitType != UNIT_NGME_GOBLIN_MERCHANT);
    Regions.ThroneoftheFourWind.PrepareUnitsForRescue(RescuePreparationMode.HideAll,
      u => u.UnitType != UNIT_NGME_GOBLIN_MERCHANT);

    var hq = AllPreplacedWidgets.Units.Get(UNIT_O00J_THUNDER_BLUFF_FROSTWOLF_OTHER);
    var death = CreateTrigger();
    TriggerRegisterUnitEvent(death, hq, EVENT_UNIT_DEATH);
    TriggerAddAction(death, () =>
    {
      if (_lossRecorded || hq.Owner.GetPlayerData().Faction?.Name != "Tauren Tribes")
      {
        return;
      }
      _lossRecorded = true;
      var killer = GetKillingUnit();
      if (killer != null && GetPlayerId(GetOwningPlayer(killer)) < 24)
      {
        _conqueror = GetOwningPlayer(killer);
      }
    });
    var capture = CreateTrigger();
    TriggerRegisterUnitEvent(capture, hq, EVENT_UNIT_CHANGE_OWNER);
    TriggerAddAction(capture, () =>
    {
      var previous = GetChangingUnitPrevOwner();
      var owner = GetOwningPlayer(hq);
      if (!_lossRecorded && previous.GetPlayerData().Faction?.Name == "Tauren Tribes" && GetPlayerId(owner) < 24)
      {
        _lossRecorded = true;
        _conqueror = owner;
      }
    });
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
    if (_conqueror != null && _conqueror != p)
    {
      ChangeTeam.JoinOrAlone(p, _conqueror.GetPlayerData().Team);
      SplitFaction.UnallyOutsideTeam(p);
    }
    foreach (var u in _camp)
    {
      if (u.Alive && u.Owner == player.NeutralPassive)
      {
        u.Rescue(p);
      }
    }
    foreach (var u in _vortex)
    {
      if (u.Alive && u.Owner == player.NeutralPassive)
      {
        u.Rescue(p);
      }
    }

    // Lost City is the maintainer's explicit exception to preserving already captured CPs.
    foreach (var type in new[] { UNIT_N0BK_LOST_CITY_OF_THE_TOL_VIR, FourCC("nVXP") })
    {
      var cp = AiApi.SafeCp(type);
      if (cp != null)
      {
        AiApi.GiveUnit(cp.Unit, p);
        cp.Unit.IsVisible = true;
        cp.Unit.IsInvulnerable = false;
        cp.Unit.SetPausedEx(false);
      }
    }
    p.RepositionCamera(Start);
  }
}
