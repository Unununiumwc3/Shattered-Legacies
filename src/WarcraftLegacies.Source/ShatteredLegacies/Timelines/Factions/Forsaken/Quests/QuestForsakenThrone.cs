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
  // tree locks behind "The Secret Buildup" (the Grand Apothecary). 0.57.1 (maintainer): the Forsaken must hold
  // Lordaeron's Capital Palace too (capitals are taken over, not destroyed); then it falls into ruins by itself -- no
  // one else can destroy it first -- and the ruins become the entrance to the Undercity (ForsakenUndercity).
  public sealed class QuestForsakenThrone : QuestData
  {
    public QuestForsakenThrone(QuestData freeWill) : base("The Throne of Lordaeron",
      "Beneath the ruins of Lordaeron's capital lie the crypts of its kings. Take the city, and the Forsaken will "
      + "build their home in its depths.",
      @"ReplaceableTextures\CommandButtons\BTNForsakenCastle.blp")
    {
      AddObjective(new ObjectiveQuestComplete(freeWill));
      // 0.57.1: a capital is not destroyed -- the attacker takes it over; so: the Forsaken hold the Capital Palace
      AddObjective(new ObjectiveControlCapital(AllLegends.Lordaeron.CapitalPalace, false));
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
      var atPalace = palace != null && GetUnitTypeId(palace) != 0;
      var x = atPalace ? GetUnitX(palace) : (cp != null && cp.Unit != null ? GetUnitX(cp.Unit) + 300f : 9388f);
      var y = atPalace ? GetUnitY(palace) : (cp != null && cp.Unit != null ? GetUnitY(cp.Unit) : 8576f);
      if (atPalace)
      {
        // the palace falls into ruins: gone, rubble around its place, the gate in the middle
        DestroyEffect(AddSpecialEffect(@"Objects\Spawnmodels\Other\NeutralBuildingExplosion\NeutralBuildingExplosion.mdl", x, y));
        RemoveUnit(palace);
        for (var a = 0; a < 8; a++)
        {
          var rubble = CreateDestructable(1280602723, x + 320f * Cos(a * 0.785f), y + 320f * Sin(a * 0.785f),
            GetRandomReal(0f, 360f), 1f, GetRandomInt(0, 2)); // LTrc Rock Chunks
          SetDestructableInvulnerable(rubble, true);
        }
      }

      ForsakenUndercity.Open(p, x, y);
    }
  }
}
