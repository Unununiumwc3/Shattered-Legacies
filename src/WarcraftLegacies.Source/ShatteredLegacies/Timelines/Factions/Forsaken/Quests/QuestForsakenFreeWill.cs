using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.55 (maintainer): Sylvanas reaches level 6 -> she becomes the Dark Ranger and the Forsaken leave the Scourge
  // (D4, breaking free). Unlocks what the 24p tree locks behind "Forsaken Independance" (Dreadguard, Dark Ranger,
  // Elder Banshee, Royal Sepulcrum ...).
  public sealed class QuestForsakenFreeWill : QuestData
  {
    public QuestForsakenFreeWill() : base("Free Will",
      "The Lich King's hold on the Forsaken weakens as their queen grows in power. When Sylvanas is strong enough, she "
      + "will tear her people free.",
      @"ReplaceableTextures\CommandButtons\BTNBanshee.blp")
    {
      AddObjective(new ObjectiveLegendLevel(ForsakenSylvanas.Legend, ForsakenConfig.FreeWillLevel));
      ResearchId = ForsakenObjects.QuestFreeWill;
    }

    public override string RewardFlavour =>
      "The Lich King's voice falls silent. Sylvanas, the Dark Ranger, leads the Forsaken on their own path.";

    protected override string RewardDescription =>
      "Sylvanas becomes the Dark Ranger, the Forsaken leave the Scourge's team and unlock the rest of their army";

    protected override void OnComplete(Faction whichFaction)
    {
      ForsakenSylvanas.SetPhase(2);
      whichFaction.Unally(); // own team ("<faction> Pact"): no longer the Lich King's
    }
  }
}
