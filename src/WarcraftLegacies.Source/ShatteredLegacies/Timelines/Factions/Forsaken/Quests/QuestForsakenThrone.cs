using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.QuestBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.55 (maintainer): free, then Lordaeron City taken -> the Undercity rises beneath it and Sylvanas becomes Queen of
  // the Forsaken. Unlocks what the 24p tree locks behind "The Secret Buildup" (the Grand Apothecary).
  public sealed class QuestForsakenThrone : QuestData
  {
    public QuestForsakenThrone(QuestData freeWill) : base("The Throne of Lordaeron",
      "Beneath the ruins of Lordaeron's capital lie the crypts of its kings. Take the city, and the Forsaken will "
      + "build their home in its depths.",
      @"ReplaceableTextures\CommandButtons\BTNForsakenCastle.blp")
    {
      AddObjective(new ObjectiveQuestComplete(freeWill));
      AddObjective(new ObjectiveControlPoint(UNIT_N01G_LORDAERON_CITY));
      ResearchId = ForsakenObjects.QuestThroneOfLordaeron;
    }

    public override string RewardFlavour =>
      "The Undercity is the Forsaken's now -- and Sylvanas their queen.";

    protected override string RewardDescription =>
      "The Undercity appears near Lordaeron City, Sylvanas becomes Queen of the Forsaken, the Grand Apothecary joins";

    protected override void OnComplete(Faction whichFaction)
    {
      ForsakenSylvanas.SetPhase(3);
      var p = whichFaction.Player;
      var cp = AiApi.SafeCp(UNIT_N01G_LORDAERON_CITY);
      if (p == null || cp == null || cp.Unit == null)
      {
        return;
      }

      var usedX = new List<float>();
      var usedY = new List<float>();
      if (AiApi.FindBuildSpot(GetUnitX(cp.Unit), GetUnitY(cp.Unit), usedX, usedY, out var x, out var y))
      {
        CreateUnit(p, ForsakenObjects.Undercity, x, y, 270f);
      }
    }
  }
}
