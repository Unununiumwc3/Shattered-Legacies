using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;

namespace WarcraftLegacies.Source.Ai
{
  // 0.71.0: OPENING HOME-FIRST. During the first Turns turns, while the army stands on its home landmass (0.71.1:
  // the landmass where the bot holds the most CPs -- 0.71.0 took the capital's, BotDiag.HomeLm, and Illidan's capital
  // stands in Outland: the filter never ran for him and he left the Broken Isles at 6:50 again) and the planner has a target there (neutral CP, creep camp, enemy), targets on other
  // landmasses are dropped from that plan -- the bot creeps and takes the neutral CPs at home before it sails or
  // walks off (Illidan bots left their island for Kalimdor at ~6:00, 0.70.3 games). Only a filter on the CANDIDATES
  // of AiRoutes.Plan step 1: routes, waypoints, chains, gathering, defence and missions are untouched; nothing pulls
  // an army back that already left. Exempt: quest CPs and quest creeps (AiQuestCreeps), unknown landmass (-1).
  // No home candidate left -> nothing is filtered. "-home on|off|ab|ab2" (slot bit 6).
  internal static class AiHomeFirst
  {
    internal static int Mode = AiAb.OnAll;
    internal const int Turns = 8;
    private static int[] _nodeLm;
    private static int[] _lmCount = new int[64];
    internal static int Plans;      // plans the filter looked at (opening, army at home)
    internal static int Filtered;   // plans that dropped away targets
    internal static int Dropped;    // away candidates dropped
    internal static int NoHome;     // plans with candidates away only (left alone)
    internal static int[] FilteredBy = new int[24];

    private static int NodeLm(int i)
    {
      if (_nodeLm == null)
      {
        _nodeLm = new int[AiRouteData.Nodes];
        for (var k = 0; k < AiRouteData.Nodes; k++)
        {
          _nodeLm[k] = Geography.Landmass(AiRouteData.NodeX[k], AiRouteData.NodeY[k]);
        }
      }

      return i >= 0 && i < _nodeLm.Length ? _nodeLm[i] : -1;
    }

    private static bool Home(int i, int homeLm, HashSet<int> questNodes)
    {
      var lm = NodeLm(i);
      return lm < 0 || lm == homeLm || questNodes.Contains(i) || AiQuestCreeps.Has(i);
    }

    // AiRoutes.Plan, after step 1: score[i] >= 0 = candidate
    internal static void Filter(int pid, BotDiag d, float[] score, HashSet<int> questNodes, float armyX, float armyY,
      bool armyKnown)
    {
      if (pid < 0 || pid >= 24 || !AiAb.For(Mode, pid, AiAb.BitHome) || GameTimeManager.Turn > Turns
          || AiTheatres.Secondary || d == null || !armyKnown)
      {
        return;
      }

      var homeLm = OpeningLm(Player(pid));
      if (homeLm < 0 || Geography.Landmass(armyX, armyY) != homeLm)
      {
        return; // the army is away already (or between landmasses): never pulled back
      }

      Plans++;
      var home = 0;
      var away = 0;
      for (var i = 0; i < score.Length; i++)
      {
        if (score[i] < 0f)
        {
          continue;
        }

        if (Home(i, homeLm, questNodes))
        {
          home++;
        }
        else
        {
          away++;
        }
      }

      if (away == 0)
      {
        return;
      }

      if (home == 0)
      {
        NoHome++;
        return;
      }

      for (var i = 0; i < score.Length; i++)
      {
        if (score[i] >= 0f && !Home(i, homeLm, questNodes))
        {
          score[i] = -1f;
        }
      }

      Filtered++;
      Dropped += away;
      FilteredBy[pid]++;
    }

    // 0.71.1: the landmass with the most CPs of the bot (ties: the first found); -1 = none
    private static int OpeningLm(player p)
    {
      var cps = p.GetPlayerData().ControlPoints;
      var best = -1;
      for (var k = 0; k < _lmCount.Length; k++)
      {
        _lmCount[k] = 0;
      }

      for (var i = 0; i < cps.Count; i++)
      {
        var cu = cps[i].Unit;
        if (cu == null)
        {
          continue;
        }

        var lm = Geography.Landmass(GetUnitX(cu), GetUnitY(cu));
        if (lm < 0 || lm >= _lmCount.Length)
        {
          continue;
        }

        _lmCount[lm]++;
        if (best < 0 || _lmCount[lm] > _lmCount[best])
        {
          best = lm;
        }
      }

      return best;
    }

    internal static void OnCommand()
    {
      var m = AiAb.Parse(GetEventPlayerChatString(), "-home");
      if (m >= 0)
      {
        Mode = m;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8,
        "opening home-first (turns 1-" + I2S(Turns) + ": targets at home before other landmasses): " + AiAb.Name(Mode)
        + " [slots" + AiAb.Slots(Mode, AiAb.BitHome) + "]  -- -home on|off|ab|ab2");
    }

    internal static string Line()
    {
      var s = "home-first (0.71) -home " + AiAb.Name(Mode) + " [slots" + AiAb.Slots(Mode, AiAb.BitHome) + "] turn "
              + I2S(GameTimeManager.Turn) + "/" + I2S(Turns) + ": plans at home " + I2S(Plans) + ", filtered "
              + I2S(Filtered) + " (away targets dropped " + I2S(Dropped) + "), away-only left alone " + I2S(NoHome)
              + "; per bot:";
      for (var pid = 0; pid < 24; pid++)
      {
        if (FilteredBy[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(FilteredBy[pid]) + ";";
        }
      }

      if (Mode == AiAb.Ab || Mode == AiAb.Ab2)
      {
        s = s + " A/B filtered plans: " + AiAb.Compare(Mode, AiAb.BitHome, FilteredBy);
      }

      return s;
    }
  }
}
