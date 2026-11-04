using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.57 (maintainer): until Sylvanas reaches level 6 the Plaguetorium is the Forsaken's only troop building; then the
  // Laboratory, the Forbidden Archive and the Haunted Harbor unlock (they require RZs1, granted here).
  public sealed class QuestForsakenRise : QuestData
  {
    public QuestForsakenRise() : base("The Banshee's Rise",
      "The more of her power Sylvanas regains, the more of the Forsaken answer her call.",
      @"ReplaceableTextures\CommandButtons\BTNBanshee.blp")
    {
      AddObjective(new ObjectiveLegendLevel(ForsakenSylvanas.Legend, ForsakenConfig.RiseLevel));
      ResearchId = ForsakenConfig.BansheeRise;
    }

    protected override string RewardDescription =>
      "The Laboratory, the Forbidden Archive and the Haunted Harbor can be built";
  }
}
