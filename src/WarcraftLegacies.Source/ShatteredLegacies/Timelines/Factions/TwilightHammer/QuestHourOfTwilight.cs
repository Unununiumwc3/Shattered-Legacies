using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestHourOfTwilight : QuestData
{
  internal QuestHourOfTwilight(QuestData fortress) : base("The Hour of Twilight",
    "Gather the cult at Grim Batol. When the highlands answer to the Old Gods, Deathwing will return.",
    @"ReplaceableTextures\CommandButtons\BTNDeathwing.blp")
  {
    AddObjective(new ObjectiveControlCapital(AllLegends.Neutral.GrimBatol, false));
    AddObjective(new ObjectiveResearch(TwilightHammerObjects.TierFour, TwilightHammerObjects.Fortress));
    var holdHighlands = new ObjectiveQuestComplete(fortress);
    if (fortress.Progress == QuestProgress.Complete)
    {
      holdHighlands.Progress = QuestProgress.Complete;
    }
    AddObjective(holdHighlands);
  }

  protected override string RewardDescription => "Deathwing emerges over Grim Batol and joins your army";

  protected override void OnComplete(Faction whichFaction)
  {
    CreateUnit(whichFaction.Player, TwilightHammerObjects.Deathwing,
      17344f, -3456f, 270f);
  }
}
