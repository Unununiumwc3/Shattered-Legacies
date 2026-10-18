using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.35.2: hero visits (AiConfig.HeroVisits) -- quests that need a hero (of some level) standing at a place, e.g.
  // Yogg-Saron (level 14) or the Fel Horde's "Ash and Smoke" (Shadowmoon Valley). Runs on the wave tick (15 s).
  // A bot sends its best hero when its faction has the quest incomplete, the hero has REALLY reached MinLevel and
  // RequireCpId (if any) is owned. The hero is kept out of army orders (AiState.MissionUnits) until the quest is no
  // longer incomplete, it dies, or 8 min pass (then 5 min pause before the next try).
  // 0.36: a visit can name a specific legend (Hero), wait for the quest's other objectives (LastObjective) and
  // require the hero to stand on the target's landmass already (SameLandmass). A hero already on a mission (another
  // visit, Ragnaros) is never picked. Shared quests (Faction "") send one hero at a time across all bots.
  internal static class AiHeroVisits
  {
    private const float GiveUp = 480f;
    private const float Pause = 300f;

    private static readonly Dictionary<string, unit> _hero = new();   // key: visit index + "|" + pid
    private static readonly Dictionary<string, float> _since = new();
    private static readonly Dictionary<string, float> _pauseUntil = new();

    public static void Run()
    {
      var list = AiConfig.HeroVisits;
      for (var v = 0; v < list.Count; v++)
      {
        var hv = list[v];
        for (var b = 0; b < SimpleBot.Bots.Count; b++)
        {
          var p = SimpleBot.Bots[b];
          var key = I2S(v) + "|" + I2S(GetPlayerId(p));
          var fac = p.GetPlayerData().Faction;
          var q = fac != null && SimpleBot.HasPerks(p) && (hv.Faction == "" || fac.Name == hv.Faction)
            ? QuestAutoComplete.FindQuest(fac, hv.Quest)
            : null;
          var open = q != null && q.Progress == QuestProgress.Incomplete;
          var hero = _hero.ContainsKey(key) ? _hero[key] : null;
          if (hero != null)
          {
            var alive = GetUnitTypeId(hero) != 0 && GetUnitState(hero, UNIT_STATE_LIFE) > 0.405f && GetOwningPlayer(hero) == p;
            var late = SimpleBot.Clock - _since[key] > GiveUp;
            if (!open || !alive || late)
            {
              Release(key, hero);
              if (late)
              {
                _pauseUntil[key] = SimpleBot.Clock + Pause;
              }

              SimpleBot.LogPin(SimpleBot.FacName(p) + ": hero visit '" + hv.Quest + "' "
                               + (!open ? "done" : (!alive ? "hero lost" : "gave up after 8 min")));
              continue;
            }

            IssuePointOrder(hero, "move", hv.X, hv.Y);
            continue;
          }

          if (!open || (_pauseUntil.ContainsKey(key) && SimpleBot.Clock < _pauseUntil[key]))
          {
            continue;
          }

          if (hv.Faction == "" && AnyUnderWay(v))
          {
            continue; // 0.36: shared quest -- another bot's hero is already on the way
          }

          if (hv.RequireCpId != 0 && !OwnsNode(p, hv.RequireCpId))
          {
            continue;
          }

          if (hv.LastObjective && OpenObjectives(q) > 1)
          {
            continue; // 0.36: other objectives (turn, capital, Frozen Throne ...) still open
          }

          var best = hv.Hero != "" ? LegendUnit(p, hv.Hero) : SimpleBot.BestHero(p);
          if (best == null || GetHeroLevel(best) < hv.MinLevel || best == SimpleBot._ragHero
              || AiState.MissionUnits.Contains(best))
          {
            continue; // 0.36: never a hero that is already on a mission (another visit, Ragnaros)
          }

          if (hv.SameLandmass)
          {
            var lmT = Geography.Landmass(hv.X, hv.Y);
            if (lmT < 0 || Geography.Landmass(GetUnitX(best), GetUnitY(best)) != lmT)
            {
              continue;
            }
          }

          _hero[key] = best;
          _since[key] = SimpleBot.Clock;
          AiState.MissionUnits.Add(best);

          IssuePointOrder(best, "move", hv.X, hv.Y);
          SimpleBot.LogPin(SimpleBot.FacName(p) + ": hero visit '" + hv.Quest + "' -- sends " + GetUnitName(best)
                           + " (lvl " + I2S(GetHeroLevel(best)) + ")");
        }
      }
    }

    private static void Release(string key, unit hero)
    {
      AiState.MissionUnits.Remove(hero);
      _hero.Remove(key);
    }

    // 0.36: a hero of ANY bot is on this visit (shared quests: one at a time). Checks all 24 slots, so a slot that
    // left the bot list (autopilot off) can't block the visit -- its stale entry is released here.
    private static bool AnyUnderWay(int v)
    {
      for (var i = 0; i < 24; i++)
      {
        var key = I2S(v) + "|" + I2S(i);
        if (!_hero.ContainsKey(key))
        {
          continue;
        }

        if (SimpleBot.IsBot(Player(i)))
        {
          return true;
        }

        Release(key, _hero[key]);
      }

      return false;
    }

    // 0.36: objectives of the quest that are not complete yet
    private static int OpenObjectives(QuestData q)
    {
      var n = 0;
      var objs = q.Objectives;
      for (var i = 0; i < objs.Count; i++)
      {
        if (objs[i].Progress != QuestProgress.Complete)
        {
          n++;
        }
      }

      return n;
    }

    // 0.36: the bot's living, walking unit of the named legend (whatever form it is in), or null
    private static unit LegendUnit(player p, string name)
    {
      var all = LegendaryHeroManager.GetAll();
      for (var i = 0; i < all.Count; i++)
      {
        if (all[i].Name != name)
        {
          continue;
        }

        var u = all[i].Unit;
        if (u != null && GetUnitTypeId(u) != 0 && GetOwningPlayer(u) == p && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
            && GetUnitMoveSpeed(u) > 1f)
        {
          return u;
        }
      }

      return null;
    }

    // 0.36: RequireCpId may name a control point or a capital
    private static bool OwnsNode(player p, int id)
    {
      var cp = SimpleBot.SafeCp(id);
      if (cp != null)
      {
        return cp.Unit != null && GetOwningPlayer(cp.Unit) == p;
      }

      var caps = CapitalManager.GetAll();
      for (var i = 0; i < caps.Count; i++)
      {
        var cu = caps[i].Unit;
        if (caps[i].UnitType == id && cu != null && GetOwningPlayer(cu) == p && GetUnitState(cu, UNIT_STATE_LIFE) > 0.405f)
        {
          return true;
        }
      }

      return false;
    }
  }
}
