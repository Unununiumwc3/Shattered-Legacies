using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.PreplacedWidgets;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.FactionBased;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.Factions.OrcishHorde.Quests;

public sealed class QuestBreakBloodPact : QuestData
{
  public QuestBreakBloodPact(QuestData founding) : base("Break the Blood Pact",
    "Bring Grom to the Fountain of Blood after founding Orgrimmar. Defeat Mannoroth with Grom nearby to cleanse the fountain and open Southern Ashenvale.",
    @"ReplaceableTextures\CommandButtons\BTNPitLord.blp")
  {
    AddObjective(new ObjectiveQuestResolved(founding));
    AddObjective(new BloodPactChallenge(founding));
    AddObjective(new ObjectiveSelfExists());
  }

  protected override string RewardDescription =>
    "Cleanse the Fountain of Blood and open Southern Ashenvale to capture.";

  private sealed class BloodPactChallenge : Objective
  {
    private readonly unit _mannoroth;
    private readonly QuestData _founding;
    private bool _revealed;

    internal BloodPactChallenge(QuestData founding)
    {
      _founding = founding;
      _mannoroth = AllPreplacedWidgets.Units.Get(UNIT_NMAN_MANNOROTH_THE_DESTROYER_WARSONG_BLOODPACT);
      var fountain = AllLegends.Neutral.FountainOfBlood.Unit;
      Position = new Point(fountain.X, fountain.Y);
      DisplaysPosition = true;
      Description = "Bring Grom to the Fountain of Blood, then defeat Mannoroth with Grom within 1200 range";
      _mannoroth.IsVisible = false;
      _mannoroth.IsInvulnerable = true;
      _mannoroth.SetPausedEx(true);

      var watching = true;
      var watch = timer.Create();
      watch.Start(1f, true, () =>
      {
        var grom = AllLegends.Orc.GromHellscream.Unit;
        if (_revealed || Progress == QuestProgress.Failed || _founding.Progress == QuestProgress.Failed)
        {
          watching = false;
          watch.Dispose();
          return;
        }
        if (_founding.Progress != QuestProgress.Complete || grom == null || !grom.Alive ||
            !Near(grom, fountain, 900f))
        {
          return;
        }
        _revealed = true;
        _mannoroth.IsVisible = true;
        _mannoroth.IsInvulnerable = false;
        _mannoroth.SetPausedEx(false);
        _mannoroth.IssueOrder(ORDER_ATTACK, grom);
      });

      var death = trigger.Create();
      death.RegisterUnitEvent(_mannoroth, unitevent.Death);
      death.AddAction(() =>
      {
        var cp = AiApi.SafeCp(FourCC("n08U"));
        if (cp != null && cp.Owner == player.NeutralPassive)
        {
          AiApi.GiveUnit(cp.Unit, player.NeutralAggressive);
          cp.Unit.IsInvulnerable = false;
        }
        var grom = AllLegends.Orc.GromHellscream.Unit;
        var killer = @event.KillingUnit;
        Progress = grom != null && grom.Alive && Near(grom, _mannoroth, 1200f) &&
                   killer != null && IsPlayerOnSameTeamAsAnyEligibleFaction(killer.Owner)
          ? QuestProgress.Complete
          : QuestProgress.Failed;
        if (Progress == QuestProgress.Complete)
        {
          fountain.Name = "Cleansed Fountain";
        }
        death.Dispose();
        if (watching)
        {
          watching = false;
          watch.Dispose();
        }
      });
    }

    private static bool Near(unit a, unit b, float distance)
    {
      var x = a.X - b.X;
      var y = a.Y - b.Y;
      return x * x + y * y <= distance * distance;
    }
  }
}
