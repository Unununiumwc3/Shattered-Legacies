using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Rules
{
  public static class EliminationRule
  {
    internal const int GraceTurns = 5;

    private static readonly Dictionary<int, int> _graceUntil = new(); // pid -> last turn without the check

    // 0.54: a faction that just started anew (a turning point) is not checked for the given number of turns
    internal static void GiveGrace(player p, int turns)
    {
      _graceUntil[GetPlayerId(p)] = GameTimeManager.Turn + turns;
      p.GetPlayerData().EliminationTurns = 0;
    }

    private static Dictionary<Team, int> EmptyTurns = new();

    public static void ProcessTeams()
    {
      var totals = new Dictionary<Team, int>();
      var eligible = new List<Faction>();
      for (var id = 0; id < 24; id++)
      {
        var p = Player(id);
        var data = p.GetPlayerData();
        if (data.Team == null || data.Faction == null || !AiApi.IsActive(p)
            || data.Faction.ScoreStatus == ScoreStatus.Defeated)
        {
          continue;
        }
        eligible.Add(data.Faction);
        if (!totals.ContainsKey(data.Team))
        {
          totals[data.Team] = 0;
        }
        totals[data.Team] += data.ControlPoints.Count;
      }
      var next = new Dictionary<Team, int>();
      foreach (var entry in totals)
      {
        next[entry.Key] = entry.Value > 0 ? 0 : (EmptyTurns.ContainsKey(entry.Key) ? EmptyTurns[entry.Key] : 0) + 1;
      }
      EmptyTurns = next;

      var doomed = new List<Faction>();
      foreach (var faction in eligible)
      {
        var p = faction.Player;
        var data = p.GetPlayerData();
        var turns = next[data.Team];
        var id = GetPlayerId(p);
        data.EliminationTurns = turns;
        if (turns == 0 || (_graceUntil.ContainsKey(id) && GameTimeManager.Turn <= _graceUntil[id]))
        {
          continue;
        }
        if (turns >= GraceTurns)
        {
          doomed.Add(faction);
        }
        else
        {
          DisplayTimedTextToPlayer(p, 0, 0, 20, "Your team holds no control points. Turns until fallback or defeat: "
            + I2S(GraceTurns - turns));
        }
      }
      foreach (var faction in doomed)
      {
        var p = faction.Player;
        if (p != null && p.GetPlayerData().Faction == faction && faction.ScoreStatus != ScoreStatus.Defeated
            && !TimelineSystem.ForceFallback(p))
        {
          AiApi.StopBot(p);
          faction.Defeat();
        }
      }
    }
  }
}
