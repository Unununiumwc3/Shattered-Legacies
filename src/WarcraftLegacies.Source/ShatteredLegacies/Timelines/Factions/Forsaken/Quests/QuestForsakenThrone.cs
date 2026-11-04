using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.Objectives.QuestBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests
{
  // 0.55 (maintainer): free, then Lordaeron City taken -> Sylvanas becomes Queen of the Forsaken. Unlocks what the 24p
  // tree locks behind "The Secret Buildup" (the Grand Apothecary). 0.57 (maintainer): Lordaeron's Capital Palace must
  // fall too (an objective with its quest marker on the minimap); its ruins become the entrance to the Undercity, a
  // base of its own in the south-west corner (ForsakenUndercity).
  public sealed class QuestForsakenThrone : QuestData
  {
    public QuestForsakenThrone(QuestData freeWill) : base("The Throne of Lordaeron",
      "Beneath the ruins of Lordaeron's capital lie the crypts of its kings. Take the city, and the Forsaken will "
      + "build their home in its depths.",
      @"ReplaceableTextures\CommandButtons\BTNForsakenCastle.blp")
    {
      AddObjective(new ObjectiveQuestComplete(freeWill));
      AddObjective(new ObjectiveCapitalDead(AllLegends.Lordaeron.CapitalPalace));
      AddObjective(new ObjectiveControlPoint(UNIT_N01G_LORDAERON_CITY));
      ResearchId = ForsakenObjects.QuestThroneOfLordaeron;
    }

    public override string RewardFlavour =>
      "The Undercity is the Forsaken's now -- and Sylvanas their queen.";

    protected override string RewardDescription =>
      "A gate in the palace's ruins leads to the Undercity (its buildings become yours), Sylvanas becomes Queen of the "
      + "Forsaken, the Grand Apothecary joins";

    protected override void OnComplete(Faction whichFaction)
    {
      ForsakenSylvanas.SetPhase(3);
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      var palace = AllLegends.Lordaeron.CapitalPalace.Unit;
      var cp = AiApi.SafeCp(UNIT_N01G_LORDAERON_CITY);
      var atPalace = palace != null && GetUnitTypeId(palace) != 0; // the ruins, if the dead palace is still there
      var x = atPalace ? GetUnitX(palace) : (cp != null && cp.Unit != null ? GetUnitX(cp.Unit) + 300f : 9388f);
      var y = atPalace ? GetUnitY(palace) : (cp != null && cp.Unit != null ? GetUnitY(cp.Unit) : 8576f);
      ForsakenUndercity.Open(p, x, y);
    }
  }
}
