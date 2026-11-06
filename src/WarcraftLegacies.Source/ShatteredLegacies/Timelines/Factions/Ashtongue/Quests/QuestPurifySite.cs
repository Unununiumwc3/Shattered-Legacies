using MacroTools.Quests;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue.Quests
{
  // 0.63: one of the four purifications (Telredor, Halaar, Shattrath, Oshu'gun -- in this order). Ishanah's Purify
  // fills its counter (AshtonguePurification); the quest's tech (RZt0-RZt3) unlocks units at the temple.
  public sealed class QuestPurifySite : QuestData
  {
    private readonly string _reward;

    public QuestPurifySite(int site, string reward) : base("Purify " + AshtongueConfig.Sites[site],
      "The fel taint lies heavy on " + AshtongueConfig.Sites[site] + ". Ishanah draws on the mana of every ally near "
      + "Shattrath to burn it out -- keep her Purify on and send your casters past her.",
      @"ReplaceableTextures\CommandButtons\BTNSpell_Holy_Renew.blp")
    {
      _reward = reward;
      var counter = new ObjectiveCounter("Purification (Ishanah's Purify)", AshtongueConfig.SiteCost[site]);
      AddObjective(counter);
      AshtonguePurification.Counters.Add(counter);
      ResearchId = AshtongueConfig.SiteTech[site];
    }

    protected override string RewardDescription => _reward;
  }
}
