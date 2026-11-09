using MacroTools.Factions;
using MacroTools.Quests;
using MacroTools.ControlPoints;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestCursedFortress : QuestData
{
  internal QuestCursedFortress() : base("The Cursed Fortress",
    "Take Grim Batol to bring its workshops and drake towers into the cult.",
    @"ReplaceableTextures\CommandButtons\BTNDragonDungeon.blp")
  {
    ResearchId = TwilightHammerObjects.CursedFortress;
    AddObjective(new HoldGrimBatol());
  }

  protected override string RewardDescription =>
    "Claim Grim Batol, its workshops and towers. Train Orcish Death Knights at the Grim Totem and tier-four Battlemasters at Grim Batol.";

  protected override void OnComplete(Faction faction)
  {
    var p = faction.Player;
    var capital = AllLegends.Neutral.GrimBatol;
    if (p == null)
    {
      return;
    }
    if (capital.Unit != null && capital.Unit.Alive &&
        (capital.Unit.Owner == player.NeutralPassive || capital.Unit.Owner == player.NeutralAggressive))
    {
      capital.Unit.SetOwner(p);
      capital.Unit.IsInvulnerable = false;
    }

    TwilightHammerFallback.ClaimHighlands(p, faction);
  }

  private sealed class HoldGrimBatol : Objective
  {
    internal HoldGrimBatol()
    {
      SetDescription("Your team controls Grim Batol");
      var point = ControlPointManager.Instance.GetFromUnitType(UNIT_N03X_GRIM_BATOL);
      TargetWidget = point.Unit;
      Position = new(point.Unit.X, point.Unit.Y);
      DisplaysPosition = true;
    }

    public override void OnAdd(Faction faction)
    {
      var point = ControlPointManager.Instance.GetFromUnitType(UNIT_N03X_GRIM_BATOL);
      void Refresh()
      {
        Progress = IsPlayerOnSameTeamAsAnyEligibleFaction(point.Owner)
          ? QuestProgress.Complete : QuestProgress.Incomplete;
      }

      point.OwnerAllianceChanged += _ => Refresh();
      // QuestData.Add evaluates initial objectives before Faction subscribes to completion rewards.
      var firstCheck = CreateTimer();
      TimerStart(firstCheck, 0f, false, () =>
      {
        DestroyTimer(firstCheck);
        Refresh();
      });
    }
  }
}
