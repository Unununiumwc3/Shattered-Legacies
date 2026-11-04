using System;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.57: an objective about ANOTHER faction's state, checked every 5 seconds -- complete while `test` holds for the
  // WL faction with that name (it can go back to incomplete). Two uses so far (the Forsaken's "Free Will"): another
  // faction's quest completed (QuestOf), another faction below N control points (ControlPointsBelow).
  internal sealed class ObjectiveFactionState : Objective
  {
    private readonly string _factionName;
    private readonly Func<Faction, bool> _test;

    private ObjectiveFactionState(string factionName, string text, Func<Faction, bool> test)
    {
      _factionName = factionName;
      _test = test;
      SetDescription(text);
      var t = CreateTimer();
      TimerStart(t, 5f, true, Check);
    }

    // "<quest title>" of the faction has been completed
    internal static ObjectiveFactionState QuestOf(string factionName, string questTitle, string text) =>
      new(factionName, text, f =>
      {
        foreach (var q in f.GetAllQuests())
        {
          if (q.Title == questTitle && q.Progress == QuestProgress.Complete)
          {
            return true;
          }
        }

        return false;
      });

    // the faction (played) holds fewer than `count` control points
    internal static ObjectiveFactionState ControlPointsBelow(string factionName, int count, string text) =>
      new(factionName, text, f => f.Player == null || f.Player.GetPlayerData().ControlPoints.Count < count);

    private void Check()
    {
      if (ProgressLocked)
      {
        return;
      }

      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == _factionName)
        {
          Progress = _test(f) ? QuestProgress.Complete : QuestProgress.Incomplete;
          return;
        }
      }
    }
  }
}
