using MacroTools.Quests;
using WCSharp.Events;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

internal sealed class ObjectiveHourglassCast : Objective
{
  internal ObjectiveHourglassCast(int ability, TurningPointQuest quest)
  {
    SetDescription("Cast {spell} from the Hourglass of Fate", ("{spell}", GetObjectName(ability)));
    PlayerUnitEvents.Register(SpellEvent.Finish, () =>
    {
      var p = @event.Unit.Owner;
      if (@event.Unit == Hourglass.Get(p.Id) && quest.Trigger.Progress == QuestProgress.Complete
          && TimelineSystem.AbilityReady(p, ability))
      {
        Progress = QuestProgress.Complete;
      }
    }, ability);
  }
}
