using MacroTools.Quests;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the 24p goblins' "Explosive Engineering" (train 4 Oil Rigs -> Gazlowe): the Enterprise's starting quest.
  // KezanEnterpriseOil counts the platforms that rise (ObjectiveCounter).
  public sealed class QuestExplosiveEngineering : QuestData
  {
    public QuestExplosiveEngineering() : base("Explosive Engineering",
      "The Goblin chief engineer, Gazlowe, oversees the construction of the overseas oil platforms.",
      @"ReplaceableTextures\CommandButtons\BTNHeroTinker.blp")
    {
      var rigs = new ObjectiveCounter("Establish Oil Rigs (train them at a Kezan Oil Supply)",
        KezanEnterpriseConfig.OilRigsForGazlowe);
      AddObjective(rigs);
      KezanEnterpriseOil.RigCounter = rigs;
      ResearchId = KezanEnterpriseObjects.QuestExplosiveEngineering;
    }

    protected override string RewardDescription => "Gazlowe, the Chief Engineer, can be trained at the Altar of Industry";
  }
}
