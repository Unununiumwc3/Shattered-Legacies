using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.53: a turning point as a WL quest (the Scarlet Crusade pattern): it shows in the faction's quest log (F9), its
  // objectives are the trigger (which unlocks the decision through its ResearchId, the turning point's condition
  // tech) and the decision itself (WL's ObjectiveResearch at the Hourglass of Fate); completing it runs the effects
  // (TimelineSystem.RunEffects). Bots decide by code (TimelineSystem sets the decision objective complete).
  // 0.54: the trigger is any objective (TurningPointDef.MakeTrigger), "all HQ units lost" by default.
  internal sealed class TurningPointQuest : QuestData
  {
    internal readonly TurningPointDef Def;
    internal readonly Objective Trigger;
    internal readonly ObjectiveHqLost HqLost; // null when the trigger is another objective
    internal readonly ObjectiveResearch Decided;
    internal bool TriggerRan;                 // OnTrigger effects done (once)

    public TurningPointQuest(TurningPointDef def, string icon)
      : base(def.Title, def.Description, icon)
    {
      Def = def;
      if (def.MakeTrigger != null)
      {
        Trigger = def.MakeTrigger();
      }
      else
      {
        HqLost = new ObjectiveHqLost(def.Hq, def.Trigger) { ResearchId = def.Condition };
        Trigger = HqLost;
      }

      Decided = new ObjectiveResearch(def.Decision, HourglassData.BuildingType);
      AddObjective(Trigger);
      AddObjective(Decided);
    }

    protected override string RewardDescription => Def != null ? Def.Announcement : ""; // null while the base constructor runs

    protected override void OnComplete(Faction whichFaction)
    {
      TimelineSystem.RunEffects(this, whichFaction);
    }
  }
}
