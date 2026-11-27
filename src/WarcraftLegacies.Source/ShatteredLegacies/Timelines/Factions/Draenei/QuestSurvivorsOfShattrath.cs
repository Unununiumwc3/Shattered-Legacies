using MacroTools.Factions;
using MacroTools.GameTime;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.TurnBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Draenei
{
  // 0.88.0 (maintainer, from the 24p's "The Survivors of Shattrah"): the Draenei hold the Exodar's bridge for six turns
  // after their arrival -> Vindicator Maraad and the survivors of Shattrath reach them (Maraad trainable: WL's "The Way
  // Forward" research, which the fork's Draenei never get -- no Darkshore quest). Nobundo comes with the arrival already
  // ("A New Home").
  internal sealed class QuestSurvivorsOfShattrath : QuestData
  {
    internal QuestSurvivorsOfShattrath() : base("The Survivors of Shattrath",
      "The Shattrath massacre was swift and brutal. If the draenei hold the Exodar long enough, the last survivors may "
      + "find their way to the ship -- Vindicator Maraad among them.",
      @"ReplaceableTextures\CommandButtons\BTNDranaiAkama.blp")
    {
      AddObjective(new ObjectiveControlPoint(UNIT_N0BL_EXODAR_REGALIS));
      AddObjective(new ObjectiveTurn(GameTimeManager.Turn + 6));
    }

    protected override string RewardDescription => "Learn to train Vindicator Maraad";

    public override string RewardFlavour =>
      "Battered and few, the survivors of Shattrath reach the Exodar. Vindicator Maraad takes up the hammer again.";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p != null)
      {
        SetPlayerTechResearched(p, UPGRADE_R082_QUEST_COMPLETED_THE_WAY_FORWARD, 1);
        AiApi.LogPin("Draenei: Maraad trainable (P" + I2S(GetPlayerId(p)) + ")");
      }
    }
  }
}
