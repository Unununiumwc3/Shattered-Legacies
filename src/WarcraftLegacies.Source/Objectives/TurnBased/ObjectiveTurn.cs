using MacroTools.GameTime;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.Objectives.TurnBased;

public sealed class ObjectiveTurn : Objective
{
  /// <param name="targetTurn">The turn number on which this objective should complete</param>
  public ObjectiveTurn(int targetTurn)
  {
    targetTurn += ObjectiveExpire.ExpirationOffset; // [SL] (0.57.3): counted from a mid-game arrival, like the deadlines
    SetDescription("Turn {turn} has started", ("{turn}", targetTurn.ToString()));
    GameTimeManager.RegisterOnTurn(targetTurn, OnComplete);
  }

  private void OnComplete()
  {
    Progress = QuestProgress.Complete;
  }
}
