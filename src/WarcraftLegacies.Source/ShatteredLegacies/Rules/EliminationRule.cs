using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Rules
{
  // 0.53, the fork's defeat rule (maintainer): a player is eliminated only when he holds NO control point for more
  // than 5 turns -- one CP is enough to live; WL's food and essential-legend conditions do not apply. A faction that
  // still has an unused fallback is not eliminated: the fallback starts instead (TimelineSystem.ForceFallback).
  // Called from WL's PlayerElimination ([SL] hook) for the players WL checks there (humans, from turn 20, every turn);
  // bots and leavers taken over by bots are not checked, as in WL.
  public static class EliminationRule
  {
    internal const int GraceTurns = 5; // defeat on the 6th turn in a row without a CP

    private static readonly Dictionary<int, int> _graceUntil = new(); // pid -> last turn without the check

    // 0.54: a faction that just started anew (a turning point) is not checked for the given number of turns
    internal static void GiveGrace(player p, int turns)
    {
      _graceUntil[GetPlayerId(p)] = GameTimeManager.Turn + turns;
      p.GetPlayerData().EliminationTurns = 0;
    }

    // replaces WL's ProcessPlayerElimination; always handles the player (returns true)
    public static bool Process(player p)
    {
      var data = p.GetPlayerData();
      var pid = GetPlayerId(p);
      if (_graceUntil.ContainsKey(pid) && GameTimeManager.Turn <= _graceUntil[pid])
      {
        data.EliminationTurns = 0;
        return true;
      }

      if (data.ControlPoints.Count > 0)
      {
        data.EliminationTurns = 0;
        return true;
      }

      if (data.EliminationTurns >= GraceTurns)
      {
        data.EliminationTurns = 0;
        if (!TimelineSystem.ForceFallback(p))
        {
          data.Faction?.Defeat();
        }

        return true;
      }

      var left = GraceTurns - data.EliminationTurns;
      DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffff6600You hold no control point.|r Capture one within " + I2S(left)
        + (left == 1 ? " turn" : " turns") + ", or you are defeated -- if your fallback is still unused, it starts instead.");
      data.EliminationTurns++;
      return true;
    }
  }
}
