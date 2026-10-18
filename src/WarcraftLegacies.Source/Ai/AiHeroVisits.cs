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

          if (hv.RequireCpId != 0)
          {
            var cp = SimpleBot.SafeCp(hv.RequireCpId);
            if (cp == null || cp.Unit == null || GetOwningPlayer(cp.Unit) != p)
            {
              continue;
            }
          }

          var best = SimpleBot.BestHero(p);
          if (best == null || GetHeroLevel(best) < hv.MinLevel || best == SimpleBot._ragHero)
          {
            continue;
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
  }
}
