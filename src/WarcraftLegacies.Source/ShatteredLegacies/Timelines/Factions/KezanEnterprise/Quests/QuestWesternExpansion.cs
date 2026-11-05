using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the 24p goblins' "Western Expansion" (Feathermoon and Auberdine destroyed -> War Zeppelins). Capitals are
  // taken over, not destroyed, in WL: the Enterprise must hold both.
  public sealed class QuestWesternExpansion : QuestData
  {
    public QuestWesternExpansion() : base("Western Expansion",
      "Feathermoon Stronghold and Auberdine give the elves a grip on the western shore of Kalimdor. Take them, and the "
      + "Enterprise's zeppelins fly safe.",
      @"ReplaceableTextures\CommandButtons\BTNNightElfShipyard.blp")
    {
      AddObjective(new ObjectiveControlCapital(AllLegends.Sentinels.Feathermoon, false));
      AddObjective(new ObjectiveControlCapital(AllLegends.Sentinels.Auberdine, false));
      ResearchId = KezanEnterpriseObjects.QuestWesternExpansion;
    }

    protected override string RewardDescription => "War Zeppelins can be trained at the Engineer's Guild";
  }
}
