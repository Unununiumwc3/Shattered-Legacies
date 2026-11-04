using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.55 (maintainer): Sylvanas breaks free -> she becomes the Dark Ranger and the Forsaken leave the Scourge (D4).
  // Unlocks what the 24p tree locks behind "Forsaken Independance" (Dreadguard, Dark Ranger) and Tier 4.
  // 0.57 (maintainer): not too early -- Sylvanas level 10, the Scourge's "The Culling" done, and the Lich King's
  // grip broken: while the Scourge holds 10 CPs it keeps the Forsaken (losing a civil war loses them).
  public sealed class QuestForsakenFreeWill : QuestData
  {
    public QuestForsakenFreeWill() : base("Free Will",
      "The Lich King's hold on the Forsaken weakens as their queen grows in power. When Sylvanas is strong enough, she "
      + "will tear her people free.",
      @"ReplaceableTextures\CommandButtons\BTNBanshee.blp")
    {
      AddObjective(new ObjectiveLegendLevel(ForsakenSylvanas.Legend, ForsakenConfig.FreeWillLevel));
      AddObjective(ObjectiveFactionState.QuestOf("Scourge", ForsakenConfig.CullingQuest,
        "The Scourge has carried out \"The Culling\""));
      AddObjective(ObjectiveFactionState.ControlPointsBelow("Scourge", ForsakenConfig.ScourgeGripControlPoints,
        "The Lich King's grip weakens: the Scourge holds fewer than 10 control points"));
      ResearchId = ForsakenObjects.QuestFreeWill;
    }

    public override string RewardFlavour =>
      "The Lich King's voice falls silent. Sylvanas, the Dark Ranger, leads the Forsaken on their own path.";

    protected override string RewardDescription =>
      "Sylvanas becomes the Dark Ranger, the Forsaken leave the Scourge's team, Tier 4 and the rest of their army unlock";

    protected override void OnComplete(Faction whichFaction)
    {
      ForsakenSylvanas.SetPhase(2);
      whichFaction.Unally(); // own team ("<faction> Pact"): no longer the Lich King's
    }
  }
}
