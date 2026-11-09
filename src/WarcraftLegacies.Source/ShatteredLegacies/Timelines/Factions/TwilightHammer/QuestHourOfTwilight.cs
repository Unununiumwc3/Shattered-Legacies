using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestHourOfTwilight : QuestData
{
  internal QuestHourOfTwilight(params QuestData[] prerequisites) : base("The Hour of Twilight",
    "Unite the ascendants, follow the vision of Thunderfury, and hold Grim Batol with a tier-four cult to awaken Deathwing.",
    @"ReplaceableTextures\CommandButtons\BTNDeathwing.blp")
  {
    Global = true;
    AddObjective(new ObjectiveControlCapital(AllLegends.Neutral.GrimBatol, false));
    AddObjective(new ObjectiveResearch(TwilightHammerObjects.TierFour, TwilightHammerObjects.Fortress));
    foreach (var quest in prerequisites)
    {
      var objective = new ObjectiveQuestComplete(quest);
      if (quest.Progress == QuestProgress.Complete)
      {
        objective.Progress = QuestProgress.Complete;
      }
      AddObjective(objective);
    }
  }

  protected override string RewardDescription => "Reach Twillight's end goal. Deathwing emerges over Grim Batol as a hero.";

  protected override void OnComplete(Faction whichFaction)
  {
    CreateUnit(whichFaction.Player, TwilightHammerObjects.Deathwing,
      17344f, -3456f, 270f);
  }
}
