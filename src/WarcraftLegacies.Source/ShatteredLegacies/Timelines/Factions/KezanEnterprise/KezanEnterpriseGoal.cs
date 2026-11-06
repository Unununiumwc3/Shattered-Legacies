using MacroTools.Quests;
using WarcraftLegacies.Source.ShatteredLegacies.Rules;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.58 (maintainer): the Enterprise's end goal, oil and trade on the scoreboard, from the moment it appears -- lines
  // under the teams (Rules/ScoreboardLines), empty (= not shown) before the fallback and after its defeat:
  //   "Kezan goal: 12345 / 20000"   (gold held now / the goal; "reached" once done)
  //   "Kezan oil: 4200 (rigs +2500/min)" (the oil in the Oil Supply, and what the platforms pump -- 0.62)
  //   "Kezan trade: 1234 gold"      (earned by the zeppelins and the slave camps so far)
  internal static class KezanEnterpriseGoal
  {
    internal static QuestTradePrincesFortune Quest;

    // map setup: the lines are registered once; they stay empty until the Enterprise plays
    internal static void Setup()
    {
      ScoreboardLines.Add(GoalLine);
      ScoreboardLines.Add(OilLine);
      ScoreboardLines.Add(TradeLine);
    }

    private static bool Playing()
    {
      if (Quest == null || KezanEnterpriseOil.Owner == null)
      {
        return false;
      }

      var fac = Quest.Gold.EligibleFactions.Count > 0 ? Quest.Gold.EligibleFactions[0] : null;
      return fac != null && fac.Player != null && fac.ScoreStatus != MacroTools.Factions.ScoreStatus.Defeated;
    }

    private static string GoalLine()
    {
      if (!Playing())
      {
        return "";
      }

      if (Quest.Progress == QuestProgress.Complete)
      {
        return "|cffffcc00Kezan goal: reached|r";
      }

      return "Kezan goal: |cffffcc00" + I2S(Quest.Gold.Gold()) + "|r / " + I2S(Quest.Gold.Amount);
    }

    private static string TradeLine() =>
      Playing() ? "Kezan trade: |cffffcc00" + I2S(KezanEnterpriseTrade.Earned) + "|r gold" : "";

    private static string OilLine() =>
      Playing()
        ? "Kezan oil: |cffffcc00" + I2S(KezanEnterpriseOil.Oil()) + "|r (rigs +" + I2S(KezanEnterpriseOil.OilRate)
          + "/min)"
        : "";
  }
}
