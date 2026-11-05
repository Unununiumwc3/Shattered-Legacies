using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the 24p goblins' "Lumber Market Crash" (Nordrassil -> Shredders +500 hit points, 30,000 lumber). Lumber is
  // knowledge in this map (won by quests): +30 knowledge instead.
  public sealed class QuestLumberMarket : QuestData
  {
    public QuestLumberMarket() : base("Lumber Market Crash",
      "The World Tree would crash the lumber market for good -- and free the Shredders for war.",
      @"ReplaceableTextures\CommandButtons\BTNJunkGolem.blp")
    {
      AddObjective(new ObjectiveControlCapital(AllLegends.Druids.Nordrassil, false));
      ResearchId = KezanEnterpriseObjects.QuestLumberMarket;
    }

    protected override string RewardDescription =>
      "Shredders gain 500 hit points; +" + I2S(KezanEnterpriseConfig.LumberMarketKnowledge) + " knowledge";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p != null)
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER,
          GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) + KezanEnterpriseConfig.LumberMarketKnowledge);
      }
    }
  }
}
