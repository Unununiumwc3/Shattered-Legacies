using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the 24p goblins' "Goblin Empire": every goblin town under one banner -> the Intercontinental Artillery.
  public sealed class QuestGoblinEmpire : QuestData
  {
    public QuestGoblinEmpire() : base("Goblin Empire",
      "All the goblin syndicates' towns must be united under one banner.",
      @"ReplaceableTextures\CommandButtons\BTNGoblinWarZeppelin.blp")
    {
      AddObjective(new ObjectiveControlPoint(UNIT_N04Z_KEZAN));
      AddObjective(new ObjectiveControlPoint(UNIT_N05C_GADGETZAN));
      AddObjective(new ObjectiveControlPoint(UNIT_N0A6_RATCHET));
      AddObjective(new ObjectiveControlPoint(UNIT_N00L_BOOTY_BAY));
      AddObjective(new ObjectiveControlPoint(UNIT_N07Y_VENTURE_BAY));
      AddObjective(new ObjectiveControlPoint(UNIT_N01E_FUSELIGHT));
      AddObjective(new ObjectiveControlPoint(UNIT_N01X_WINTERSPRING));
      ResearchId = KezanEnterpriseObjects.QuestGoblinEmpire;
    }

    protected override string RewardDescription => "The Intercontinental Artillery can be built (Tier 4)";
  }
}
