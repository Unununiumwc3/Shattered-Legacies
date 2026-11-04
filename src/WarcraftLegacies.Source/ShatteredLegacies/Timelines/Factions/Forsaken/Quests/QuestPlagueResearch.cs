using MacroTools.Quests;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.56 (maintainer): the Forsaken's repeatable knowledge quest -- every 100 enemy units killed: +5 knowledge (the
  // tome, lumber in WC3 terms). It never completes; the objective counts again.
  public sealed class QuestPlagueResearch : QuestData
  {
    internal const int Kills = 100;
    internal const int Knowledge = 5;

    public QuestPlagueResearch() : base("Plague Research",
      "The Royal Apothecary Society studies every corpse the Forsaken leave behind. The more they kill, the more their "
      + "plague learns.",
      @"ReplaceableTextures\CommandButtons\BTNPlagueCloud.blp")
    {
      AddObjective(new ObjectiveRepeatingKills(Kills, "+5 knowledge", p =>
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) + Knowledge);
        DisplayTimedTextToPlayer(p, 0, 0, 10, "|cffffcc00Plague Research:|r +5 knowledge.");
      }));
    }

    protected override string RewardDescription => "+5 knowledge for every 100 enemy units killed";
  }
}
