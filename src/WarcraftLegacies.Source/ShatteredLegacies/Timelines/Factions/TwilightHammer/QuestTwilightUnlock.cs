using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestTwilightUnlock : QuestData
{
  protected override string RewardDescription { get; }

  internal QuestTwilightUnlock(string title, string description, string icon, int researchId, string reward,
    params Objective[] objectives) : base(title, description, @"ReplaceableTextures\CommandButtons\" + icon + ".blp")
  {
    ResearchId = researchId;
    RewardDescription = reward;
    foreach (var objective in objectives)
    {
      AddObjective(objective);
    }
  }
}
