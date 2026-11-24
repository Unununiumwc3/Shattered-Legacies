using System.Collections.Generic;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.Ai
{
  // 0.71.0: QUEST CREEPS. The bot asks its faction which creeps its open quests want dead (every
  // ObjectiveHostilesInAreaAreDead of an open / undiscovered quest -- Gnomeregan for Ironforge, ...) and the route
  // planner treats their areas as priority targets: a camp of quest creeps is a candidate even outside the CPs the
  // bot holds, and scores its plain distance x Bias instead of distance + CreepCost (3000). The PLANNER picks it --
  // same routes, waypoints, ferries, strength check (army food vs camp levels) and stuck blacklist as any other
  // target; no kill state, no wake event. Refreshed by AiRoutes.Plan (QuestNodes) for the planning bot: a few
  // dozen alive checks. "-questcreeps on|off".
  internal static class AiQuestCreeps
  {
    internal static bool On = true;
    internal const float Bias = 0.6f;
    internal const float FoodPerLevel = 1f; // army food per creep level of the area (other camps: 1.5)
    private static HashSet<int> _nodes = new();
    private static Dictionary<int, unit> _unit = new(); // node -> a living quest creep there
    internal static int Plans;
    internal static int Nodes;
    internal static int Candidates;
    internal static int Picks;
    internal static string LastPick = "-";

    internal static void Clear()
    {
      _nodes.Clear();
      _unit.Clear();
    }

    // an objective of an open quest of the planning faction (AiRoutes.QuestNodes)
    internal static void Note(Objective o)
    {
      if (!On || o.Progress == QuestProgress.Complete)
      {
        return;
      }

      var hd = o as ObjectiveHostilesInAreaAreDead;
      if (hd == null)
      {
        return;
      }

      var list = hd.Targets;
      for (var i = 0; i < list.Count; i++)
      {
        var t = list[i];
        if (GetUnitTypeId(t) == 0 || GetUnitState(t, UNIT_STATE_LIFE) <= 0.405f
            || GetOwningPlayer(t) != Player(PLAYER_NEUTRAL_AGGRESSIVE) || IsUnitHidden(t) || BlzIsUnitInvulnerable(t)
            || !AiArmy.CanFight(t))
        {
          continue;
        }

        var node = AiRoutes.Region(GetUnitX(t), GetUnitY(t));
        if (node >= 0 && !_nodes.Contains(node))
        {
          _nodes.Add(node);
          _unit[node] = t;
        }
      }
    }

    internal static void Done()
    {
      Plans++;
      Nodes += _nodes.Count;
    }

    internal static bool Has(int node) => _nodes.Contains(node);

    internal static unit At(int node) => _unit.TryGetValue(node, out var u) ? u : null;

    internal static void Picked(int pid, unit u)
    {
      Picks++;
      LastPick = "P" + I2S(pid) + " " + GetUnitName(u);
    }

    internal static void OnCommand()
    {
      var s = GetEventPlayerChatString();
      if (s == "-questcreeps on")
      {
        On = true;
      }
      else if (s == "-questcreeps off")
      {
        On = false;
        Clear();
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8,
        "quest creeps (creeps an open quest wants dead are priority route targets): " + (On ? "ON" : "OFF")
        + "  -- -questcreeps on|off");
    }

    internal static string Line()
    {
      return "quest creeps (0.71) " + (On ? "ON" : "OFF") + ": plans " + I2S(Plans) + ", quest-creep areas seen "
             + I2S(Nodes) + ", candidates " + I2S(Candidates) + ", picked " + I2S(Picks) + " (last " + LastPick + ")";
    }
  }
}
