using MacroTools.GameTime;
using WarcraftLegacies.Source.ShatteredLegacies.Rules;

namespace WarcraftLegacies.Source.GameLogic.GameEnd;

public static class PlayerElimination
{
  public static void Setup()
  {
    // [SL] Evaluate the team standings once per turn, before any fallback transfers ownership.
    GameTimeManager.RegisterOnTurnRepeating(20, EliminationRule.ProcessTeams);
  }
}
