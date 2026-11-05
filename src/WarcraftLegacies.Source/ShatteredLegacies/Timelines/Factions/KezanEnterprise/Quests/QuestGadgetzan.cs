using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.FactionBased;
using WarcraftLegacies.Source.Objectives.TurnBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.58, the 24p goblins' "Gadgetzan" (CP Gadgetzan before the deadline -> Noggenfogger). The deadline counts from the
  // Enterprise's arrival (SwapFaction.RegisterArriving). The 24p reward of Gadgetzan's buildings is left out: the main
  // map's Neutral Passive units in Tanaris belong to Skywall.
  public sealed class QuestGadgetzan : QuestData
  {
    public QuestGadgetzan() : base("Gadgetzan",
      "The city of Gadgetzan is the perfect foothold for the Enterprise in Kalimdor.",
      @"ReplaceableTextures\CommandButtons\BTNHeroAlchemist.blp")
    {
      AddObjective(new ObjectiveControlPoint(UNIT_N05C_GADGETZAN));
      AddObjective(new ObjectiveExpire(KezanEnterpriseConfig.GadgetzanTurns, "Gadgetzan"));
      AddObjective(new ObjectiveSelfExists());
      ResearchId = KezanEnterpriseObjects.QuestGadgetzan;
    }

    protected override string RewardDescription =>
      "Noggenfogger, the Baron of Gadgetzan, can be trained at the Altar of Industry";
  }
}
