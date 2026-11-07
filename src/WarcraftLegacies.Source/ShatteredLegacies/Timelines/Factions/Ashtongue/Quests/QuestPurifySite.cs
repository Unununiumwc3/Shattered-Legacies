using MacroTools.Quests;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue.Quests
{
  // 0.63: one purification step (0.64: Shattrath, Halaa, Telredor, Oshu'gun -- in this order; the temple itself is
  // QuestRestoringKarabor). Shattrath's donations fill its counter (AshtonguePurification); a minimap icon at the site.
  public sealed class QuestPurifySite : QuestData
  {
    private readonly string _reward;

    public QuestPurifySite(int step, string reward) : base("Purify " + AshtongueConfig.Steps[step],
      "The fel taint lies heavy on " + AshtongueConfig.Steps[step] + ". Shattrath draws on the mana of every ally "
      + "near it to burn it out -- keep Ishanah's Purify on and send your casters past the city.",
      @"ReplaceableTextures\CommandButtons\BTNSpell_Holy_Renew.blp")
    {
      _reward = reward;
      var counter = new ObjectiveCounter("Purification (Shattrath's pool)", AshtongueConfig.StepCost[step],
        AshtongueConfig.SiteXY[step * 2], AshtongueConfig.SiteXY[step * 2 + 1]);
      AddObjective(counter);
      AshtonguePurification.Counters.Add(counter);
      ResearchId = AshtongueConfig.StepTech[step];
    }

    protected override string RewardDescription => _reward;
  }
}
