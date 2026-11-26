using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives;

namespace WarcraftLegacies.Source.Ai
{
  // 0.76.0: the QUEST RUNNER -- one general way for bots to finish "bring a unit to a place" quests (Illidan to the
  // Tomb of Sargeras, a hero of level 10 at a shrine, any unit at an outpost) without one-off code (maintainer,
  // 2026-11-25). Runs on the wave tick (15 s), bots only.
  // 1. READ: every incomplete quest of the bot's faction. An objective that is IObjectiveVisit (Objectives/) says where
  //    and who: a legend, a unit type, a hero of a level, any hero, any unit.
  // 2. ONLY WHEN THE REST IS DONE (maintainer): the unit is sent only while every OTHER objective of the quest is
  //    complete (turn, capital, research, kills ...). "Expire" and "your faction exists" start complete, so they never
  //    block. Is something reopened, the unit comes back.
  // 3. RESERVE: the unit joins AiState.MissionUnits (army, defence and spell code leave it alone) and walks the route
  //    graph (AiRoutes.MoveVia: ferries, portals). At most 2 jobs per bot; one unit per job.
  // 4. GUARDS: released (back to the army) when the objective or quest is done / failed, the unit died or changed
  //    owner, it fell below 35 % life on the way, or it took longer than its time limit (2.5x the walking time + 90 s).
  //    Each failure pauses the job (60 s, doubling); after 4 failures the bot gives up on it for this game. A dead
  //    legend is simply waited for (the altar revives it).
  // 5. NEVER TWICE: quests that have a hand-made hero visit (AiConfig.HeroVisits) or are listed in AiConfig.
  //    QuestRunnerSkip are left alone. Unknown objective types are not touched at all.
  // Dump: section "quest runner".
  internal static class AiQuestRunner
  {
    private sealed class Job
    {
      internal player P;
      internal unit U;
      internal string Quest;
      internal float Since;
      internal float Limit;
      internal int Tries;
      internal float PauseUntil;
      internal bool GivenUp;
      internal string Last = "";
    }

    private const int MaxPerBot = 2;
    private const int MaxTries = 4;
    private const float LowLife = 0.35f;
    private static readonly Dictionary<Objective, Job> _jobs = new();
    private static readonly List<Objective> _order = new(); // deterministic iteration (dump, cleanup)
    internal static int Sent;
    internal static int Done;
    internal static int Failed;
    internal static int Waiting;
    internal static int NoUnit;

    internal static void Run()
    {
      Waiting = 0;
      NoUnit = 0;
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || !SimpleBot.IsBot(p))
        {
          continue;
        }

        var quests = fac.GetAllQuests();
        for (var qi = 0; qi < quests.Count; qi++)
        {
          var q = quests[qi];
          if (q.Progress != QuestProgress.Incomplete || Skipped(q.Title))
          {
            continue;
          }

          Objective visit = null;
          var others = 0;
          for (var oi = 0; oi < q.Objectives.Count; oi++)
          {
            var o = q.Objectives[oi];
            if (o.Progress == QuestProgress.Complete)
            {
              continue;
            }

            if (visit == null && o is IObjectiveVisit)
            {
              visit = o;
            }
            else
            {
              others++;
            }
          }

          if (visit == null)
          {
            continue;
          }

          _jobs.TryGetValue(visit, out var job);
          if (job != null && job.U != null)
          {
            Tend(job, visit, others);
            continue;
          }

          if (others > 0)
          {
            Waiting++;
            continue; // the maintainer's rule: the other requirements first
          }

          if (job != null && (job.GivenUp || SimpleBot.Clock < job.PauseUntil))
          {
            continue;
          }

          if (Active(p) >= MaxPerBot)
          {
            continue;
          }

          var v = (IObjectiveVisit)visit;
          var r = v.VisitRect;
          float cx = GetRectCenterX(r), cy = GetRectCenterY(r);
          var u = Pick(p, v, cx, cy);
          if (u == null)
          {
            NoUnit++;
            continue;
          }

          if (job == null)
          {
            job = new Job { P = p, Quest = q.Title };
            _jobs[visit] = job;
            _order.Add(visit);
          }

          job.U = u;
          job.Since = SimpleBot.Clock;
          var speed = GetUnitMoveSpeed(u) > 150f ? GetUnitMoveSpeed(u) : 150f;
          var dx = GetUnitX(u) - cx;
          var dy = GetUnitY(u) - cy;
          job.Limit = 90f + 2.5f * SquareRoot(dx * dx + dy * dy) / speed;
          AiState.MissionUnits.Add(u);
          AiRoutes.MoveVia(u, cx, cy);
          Sent++;
          job.Last = "sent " + GetUnitName(u);
          SimpleBot.LogPin(SimpleBot.FacName(p) + ": quest '" + q.Title + "' -- sends " + GetUnitName(u)
                           + " (limit " + I2S(R2I(job.Limit)) + " s)");
        }
      }

      Cleanup();
    }

    private static void Tend(Job job, Objective visit, int others)
    {
      var u = job.U;
      var p = job.P;
      if (visit.Progress == QuestProgress.Complete)
      {
        Release(job, "done", false);
        Done++;
        return;
      }

      if (GetUnitTypeId(u) == 0 || GetOwningPlayer(u) != p)
      {
        Release(job, "unit gone", true);
        return;
      }

      if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        Release(job, "died on the way", true);
        return;
      }

      if (others > 0)
      {
        Release(job, "another objective reopened", false);
        return;
      }

      var r = ((IObjectiveVisit)visit).VisitRect;
      float x = GetUnitX(u), y = GetUnitY(u);
      var there = x >= GetRectMinX(r) && x <= GetRectMaxX(r) && y >= GetRectMinY(r) && y <= GetRectMaxY(r);
      if (there)
      {
        job.Last = "there";
        return; // no orders: a channel or a "stay inside" objective must not be broken
      }

      if (GetUnitState(u, UNIT_STATE_LIFE) < LowLife * GetUnitState(u, UNIT_STATE_MAX_LIFE))
      {
        Release(job, "low life", true);
        return;
      }

      if (SimpleBot.Clock - job.Since > job.Limit)
      {
        Release(job, "too slow", true);
        return;
      }

      AiRoutes.MoveVia(u, GetRectCenterX(r), GetRectCenterY(r));
    }

    private static void Release(Job job, string why, bool failure)
    {
      if (job.U != null && !AiRitual.Owns(job.U))
      {
        AiState.MissionUnits.Remove(job.U);
      }

      job.U = null;
      job.Last = why;
      if (failure)
      {
        Failed++;
        job.Tries++;
        var pause = 60f;
        for (var i = 1; i < job.Tries; i++)
        {
          pause *= 2f;
        }

        job.PauseUntil = SimpleBot.Clock + pause;
        job.GivenUp = job.Tries >= MaxTries;
      }

      SimpleBot.LogPin(SimpleBot.FacName(job.P) + ": quest '" + job.Quest + "' -- " + why
                       + (job.GivenUp ? " (given up)" : ""));
    }

    // jobs whose quest ended while nobody looked (quest complete / failed: no longer listed as incomplete)
    private static void Cleanup()
    {
      for (var i = 0; i < _order.Count; i++)
      {
        var o = _order[i];
        var job = _jobs[o];
        if (job.U != null && (o.Progress == QuestProgress.Complete || o.Progress == QuestProgress.Failed
                              || !SimpleBot.IsBot(job.P)))
        {
          Release(job, o.Progress == QuestProgress.Complete ? "done" : "quest over", false);
        }
      }
    }

    private static int Active(player p)
    {
      var n = 0;
      for (var i = 0; i < _order.Count; i++)
      {
        var job = _jobs[_order[i]];
        if (job.P == p && job.U != null)
        {
          n++;
        }
      }

      return n;
    }

    private static bool Skipped(string title)
    {
      var visits = AiConfig.HeroVisits;
      for (var i = 0; i < visits.Count; i++)
      {
        if (visits[i].Quest == title)
        {
          return true;
        }
      }

      return AiConfig.QuestRunnerSkip.Contains(title);
    }

    // who goes: the named legend; else a unit of the named type; else the best free hero (of the level); else, when
    // any unit will do, the free army unit nearest to the place
    private static unit Pick(player p, IObjectiveVisit v, float cx, float cy)
    {
      var legend = v.VisitLegend;
      if (legend != null)
      {
        var lu = legend.Unit;
        return Free(lu, p) ? lu : null; // dead / elsewhere: wait for it
      }

      unit best = null;
      var bestScore = -1f;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!Free(u, p) || IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON))
        {
          continue;
        }

        var hero = IsUnitType(u, UNIT_TYPE_HERO);
        if ((v.VisitUnitType != 0 && GetUnitTypeId(u) != v.VisitUnitType) || (v.VisitHeroOnly && !hero)
            || (v.VisitMinLevel > 0 && (!hero || GetHeroLevel(u) < v.VisitMinLevel)))
        {
          continue;
        }

        // heroes: the highest level; units: the nearest (any unit: a hero only if it is the nearest)
        var dx = GetUnitX(u) - cx;
        var dy = GetUnitY(u) - cy;
        var score = v.VisitHeroOnly || v.VisitMinLevel > 0 ? GetHeroLevel(u) : 100000000f - (dx * dx + dy * dy) / 100f;
        if (score > bestScore)
        {
          best = u;
          bestScore = score;
        }
      }

      DestroyGroup(g);
      return best;
    }

    private static bool Free(unit u, player p)
    {
      return u != null && GetUnitTypeId(u) != 0 && GetOwningPlayer(u) == p && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
             && !IsUnitHidden(u) && !IsUnitLoaded(u) && GetUnitMoveSpeed(u) > 1f && !AiState.MissionUnits.Contains(u) && !AiRecall.Holds(u)
             && u != SimpleBot._ragHero && !AiFerries.InTransit(u);
    }

    internal static void Lines(List<string> o)
    {
      o.Add("## QUEST RUNNER (0.76.0: bots bring units to quest places once the other objectives are done)");
      o.Add("sent " + I2S(Sent) + ", done " + I2S(Done) + ", failed " + I2S(Failed) + "; last pass: waiting for other "
            + "objectives " + I2S(Waiting) + ", no fitting unit " + I2S(NoUnit));
      for (var i = 0; i < _order.Count; i++)
      {
        var job = _jobs[_order[i]];
        o.Add("  P" + I2S(GetPlayerId(job.P)) + " '" + job.Quest + "': " + (job.U != null ? "under way, " + GetUnitName(job.U)
              + " " + I2S(R2I(SimpleBot.Clock - job.Since)) + "/" + I2S(R2I(job.Limit)) + " s" : job.Last)
              + (job.Tries > 0 ? ", tries " + I2S(job.Tries) : "") + (job.GivenUp ? ", GIVEN UP" : ""));
      }
    }
  }
}
