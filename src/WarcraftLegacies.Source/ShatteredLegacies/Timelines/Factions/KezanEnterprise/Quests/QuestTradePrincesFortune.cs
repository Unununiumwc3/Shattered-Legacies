using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the Enterprise's END GOAL (concepts section 10: acquire 20,000 gold -- here: hold it at once). Shown on the
  // scoreboard while the Enterprise plays (KezanEnterpriseGoal). What reaching it does beyond the announcement is
  // open (TODO.md: the fork's win rule).
  public sealed class QuestTradePrincesFortune : QuestData
  {
    public QuestTradePrincesFortune() : base("The Trade Prince's Fortune",
      "Every war ends, but gold endures. Amass a fortune so vast that no throne in Azeroth can ignore the Enterprise.",
      @"ReplaceableTextures\CommandButtons\BTNChestOfGold.blp")
    {
      Gold = new ObjectiveHoldGold(KezanEnterpriseConfig.GoalGold);
      AddObjective(Gold);
    }

    internal ObjectiveHoldGold Gold { get; }

    protected override string RewardDescription => "The Kezan Enterprise reaches its end goal";

    protected override void OnComplete(Faction whichFaction)
    {
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, "|cffffcc00The Kezan Enterprise has amassed "
          + I2S(KezanEnterpriseConfig.GoalGold) + " gold.|r The Trade Prince's fortune is made -- its end goal is reached.");
      }

      AiApi.LogPin("END GOAL: Kezan Enterprise holds " + I2S(KezanEnterpriseConfig.GoalGold) + " gold");
    }
  }
}
