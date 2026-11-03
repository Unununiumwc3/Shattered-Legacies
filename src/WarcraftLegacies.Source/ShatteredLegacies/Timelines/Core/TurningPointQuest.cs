using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.53: a turning point as a WL quest (the Scarlet Crusade pattern): it shows in the faction's quest log (F9), its
  // objectives are the trigger (ObjectiveHqLost, which unlocks the decision) and the decision itself (WL's
  // ObjectiveResearch at the Hourglass of Fate); completing it runs the effects (TimelineSystem.RunEffects).
  // Bots decide by code (TimelineSystem sets the decision objective complete).
  internal sealed class TurningPointQuest : QuestData
  {
    internal readonly TurningPointDef Def;
    internal readonly ObjectiveHqLost HqLost;
    internal readonly ObjectiveResearch Decided;

    public TurningPointQuest(TurningPointDef def, string icon)
      : base("Fallback: " + def.Name, def.Description, icon)
    {
      Def = def;
      HqLost = new ObjectiveHqLost(def.Hq, def.Trigger) { ResearchId = def.Condition };
      Decided = new ObjectiveResearch(def.Decision, HourglassData.BuildingType);
      AddObjective(HqLost);
      AddObjective(Decided);
    }

    protected override string RewardDescription => Def != null ? Def.Announcement : ""; // null while the base constructor runs

    protected override void OnComplete(Faction whichFaction)
    {
      TimelineSystem.RunEffects(this, whichFaction);
    }
  }
}
