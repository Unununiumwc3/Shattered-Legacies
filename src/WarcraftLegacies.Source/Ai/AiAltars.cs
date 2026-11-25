using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.UnitTraits;

namespace WarcraftLegacies.Source.Ai
{
  internal static class AiAltars
  {
    private sealed class AltarJob
    {
      internal unit Altar;
      internal int Type;
      internal float Issued;
      internal bool Started;
    }

    private static readonly List<AltarJob>[] HeroJobs = new List<AltarJob>[24];
    private static readonly bool[] RecruitNext = new bool[24];
    private static readonly int[] ReviveCursor = new int[24];
    internal static readonly int[] HeroAccepted = new int[24];
    internal static readonly int[] HeroStarted = new int[24];
    internal static readonly int[] HeroFinished = new int[24];
    internal static readonly int[] HeroCancelled = new int[24];
    internal static readonly int[] HeroExpired = new int[24];
    internal static readonly int[] HeroRefused = new int[24];
    internal static readonly int[] AltarCount = new int[24];
    internal static readonly int[] FoodNeeded = new int[24];
    internal static readonly int[] FreeRevives = new int[24];

    // 0.73.0: bots with perks revive their heroes for free a few seconds after death, at a living altar, without
    // the native altar revive (~100 s at high levels, needs the altar idle and free food). ReviveHero fires no
    // "finishes reviving" event, so the things WL hangs on it are done here (unit traits, hero colour);
    // RefundZeroLimitUnits' check is the object-limit test below.
    internal const float FreeReviveDelay = 8f;
    private static readonly Dictionary<unit, float> DeadSince = new();

    private static void FreeRevive(player p, unit altar)
    {
      var heroes = LegendaryHeroManager.GetAll();
      for (var i = 0; i < heroes.Count; i++)
      {
        var legend = heroes[i];
        var hero = legend.Unit;
        if (hero == null || GetUnitTypeId(hero) == 0 || GetOwningPlayer(hero) != p
            || GetUnitState(hero, UNIT_STATE_LIFE) > 0.405f || GetUnitAbilityLevel(hero, SimpleBot.PermaDiesMarker) > 0
            || SimpleBot.PermaDead.Contains(legend.UnitType) || p.GetPlayerData().GetObjectLimit(GetUnitTypeId(hero)) <= 0)
        {
          continue;
        }

        if (!DeadSince.TryGetValue(hero, out var since))
        {
          DeadSince[hero] = SimpleBot.Clock;
          continue;
        }

        if (SimpleBot.Clock - since < FreeReviveDelay)
        {
          continue;
        }

        if (!ReviveHero(hero, GetUnitX(altar), GetUnitY(altar) - 256f, true))
        {
          continue;
        }

        DeadSince.Remove(hero);
        UnitTypeTraitRegistry.ForceOnCreated(hero);
        SetUnitColor(hero, legend.HasCustomColor && legend.PlayerColor != null ? legend.PlayerColor : GetPlayerColor(p));
        var pid = GetPlayerId(p);
        FreeRevives[pid]++;
        AltarReason[pid] = "revived " + GetObjectName(GetUnitTypeId(hero)) + " (free)";
        SimpleBot.Log("altar: " + SimpleBot.FacName(p) + " revived " + GetUnitName(hero) + " for free after "
                      + I2S(R2I(SimpleBot.Clock - since)) + " s");
      }
    }

    private static void ReserveMissingFood(player p, int type)
    {
      var needed = GetFoodUsed(type);
      var free = GetPlayerState(p, PLAYER_STATE_RESOURCE_FOOD_CAP) - GetPlayerState(p, PLAYER_STATE_RESOURCE_FOOD_USED);
      if (free >= needed)
      {
        return;
      }
      var pid = GetPlayerId(p);
      if (needed > FoodNeeded[pid])
      {
        FoodNeeded[pid] = needed;
      }
      AltarReason[pid] = "waiting for food: " + GetObjectName(type);
    }

    private static readonly int[] UnstartedType = new int[24];
    internal static readonly string[] AltarReason = new string[24];

    internal static void HeroEvent(player p, int type, bool started, bool finished)
    {
      var pid = GetPlayerId(p);
      if (pid < 0 || pid >= 24 || HeroJobs[pid] == null)
      {
        return;
      }
      var jobs = HeroJobs[pid];
      for (var i = 0; i < jobs.Count; i++)
      {
        if (jobs[i].Type != type)
        {
          continue;
        }
        if (started)
        {
          jobs[i].Started = true;
          HeroStarted[pid]++;
        }
        else
        {
          if (finished)
          {
            HeroFinished[pid]++;
          }
          else
          {
            HeroCancelled[pid]++;
          }
          AltarReason[pid] = (finished ? "finished " : "cancelled ") + GetObjectName(type);
          SimpleBot.Log("altar: " + SimpleBot.FacName(p) + " " + AltarReason[pid]);
          jobs.RemoveAt(i);
          AiBuilder.Request(p);
        }
        return;
      }
    }

    private static bool AltarReserved(player p, unit altar, int type)
    {
      var pid = GetPlayerId(p);
      var jobs = HeroJobs[pid];
      if (jobs == null)
      {
        return false;
      }
      var reserved = false;
      for (var i = jobs.Count - 1; i >= 0; i--)
      {
        var job = jobs[i];
        // Builder and production can visit the same altar before WC3 exposes a current order.
        // Native start owns the slot through finish/cancel, including high-level (180s) revivals.
        var age = SimpleBot.Clock - job.Issued;
        if (GetUnitTypeId(job.Altar) == 0 || GetOwningPlayer(job.Altar) != p
            || GetUnitState(job.Altar, UNIT_STATE_LIFE) <= 0.405f
            || age > 210f || (!job.Started && age > 8f && GetUnitCurrentOrder(job.Altar) == 0))
        {
          if (!job.Started)
          {
            UnstartedType[pid] = job.Type;
          }
          HeroExpired[pid]++;
          jobs.RemoveAt(i);
        }
        else if (job.Altar == altar || job.Type == type)
        {
          reserved = true;
        }
      }
      return reserved;
    }

    private static bool OrderHero(player p, unit altar, int type, unit hero)
    {
      var pid = GetPlayerId(p);
      if (HeroJobs[pid] == null)
      {
        HeroJobs[pid] = new List<AltarJob>();
      }
      var job = new AltarJob { Altar = altar, Type = type, Issued = SimpleBot.Clock };
      HeroJobs[pid].Add(job); // register before the native, which may emit START synchronously
      var order = hero == null ? type : 852039;
      var accepted = SimpleBot.HasPerks(p) ? AiTech.FreeOrder(p, altar, order, hero)
        : hero == null ? IssueImmediateOrderById(altar, order) : IssueTargetOrderById(altar, order, hero);
      if (!accepted)
      {
        HeroJobs[pid].Remove(job);
        HeroRefused[pid]++;
        AltarReason[pid] = "WC3 refused " + GetObjectName(type);
        return false;
      }
      HeroAccepted[pid]++;
      RecruitNext[pid] = hero != null;
      AltarReason[pid] = (hero == null ? "training " : "reviving ") + GetObjectName(type);
      SimpleBot.Log("altar: " + SimpleBot.FacName(p) + " " + AltarReason[pid]);
      return true;
    }

    internal static bool TrainHeroes(unit altar, Faction faction, player p)
    {
      if (SimpleBot.HasPerks(p) && GetUnitState(altar, UNIT_STATE_LIFE) > 0.405f)
      {
        FreeRevive(p, altar);
      }

      if (GetUnitState(altar, UNIT_STATE_LIFE) <= 0.405f || AltarReserved(p, altar, 0) || GetUnitCurrentOrder(altar) != 0)
      {
        return false;
      }
      var pid = GetPlayerId(p);
      var recruitFirst = RecruitNext[pid];
      if (!recruitFirst && TryReviveAtAltar(p, altar))
      {
        return false;
      }
      var cand = new List<int>();
      if (AiConfig.Heroes.ContainsKey(faction.Name))
      {
        var ids = AiConfig.Heroes[faction.Name];
        for (var i = 0; i < ids.Count; i++)
        {
          cand.Add(ids[i]);
        }
      }

      var atid = GetUnitTypeId(altar);
      if (AiData.AltarHeroes.ContainsKey(atid))
      {
        var more = AiData.AltarHeroes[atid];
        for (var i = 0; i < more.Length; i++)
        {
          if (!SimpleBot.ListHas(cand, more[i]))
          {
            cand.Add(more[i]);
          }
        }
      }

      // A repeatedly accepted order which never starts must leave room for another available hero.
      if (cand.Remove(UnstartedType[pid]))
      {
        cand.Add(UnstartedType[pid]);
      }
      for (var i = 0; i < cand.Count; i++)
      {
        var hid = cand[i];
        if (SimpleBot.PermaDead.Contains(hid))
        {
          continue; // v28: died permanently (e.g. Cenarius) -- WL would adopt a re-trained copy, so never train it
        }

        if (p.GetPlayerData().GetObjectLimit(hid) <= 0 || AltarReserved(p, null, hid) || HaveHero(p, hid) || !SimpleBot.RequirementsMet(p, hid))
        {
          continue; // already alive, or still locked (quest not done)
        }

        var perks = SimpleBot.HasPerks(p);
        if (OrderHero(p, altar, hid, null))
        {
          return false;
        }

        if (AiData.AltarHeroes.ContainsKey(atid))
        {
          var menu = AiData.AltarHeroes[atid];
          for (var k = 0; k < menu.Length; k++)
          {
            if (menu[k] == hid)
            {
              ReserveMissingFood(p, hid);
              break;
            }
          }
        }
        if (!perks && GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) < GetUnitGoldCost(hid))
        {
          return true; // available but too expensive right now -> save
        }

      }

      if (recruitFirst)
      {
        TryReviveAtAltar(p, altar);
      }
      return false;
    }

    internal static void Dump(List<string> lines, player p, Faction faction)
    {
      var pid = GetPlayerId(p);
      var jobs = HeroJobs[pid];
      if (jobs != null)
      {
        foreach (var job in jobs)
        {
          lines.Add("  altar job " + GetObjectName(job.Type) + " age=" + I2S(R2I(SimpleBot.Clock - job.Issued))
            + "s started=" + (job.Started ? "yes" : "no") + " building=" + GetUnitName(job.Altar));
        }
      }
      if (!AiConfig.Heroes.ContainsKey(faction.Name))
      {
        return;
      }
      foreach (var type in AiConfig.Heroes[faction.Name])
      {
        if (HaveHero(p, type) || SimpleBot.PermaDead.Contains(type))
        {
          continue;
        }
        var why = p.GetPlayerData().GetObjectLimit(type) <= 0 ? "disabled"
          : !SimpleBot.RequirementsMet(p, type) ? "missing prerequisite"
          : AltarCount[pid] == 0 ? "no altar" : "unlocked";
        lines.Add("  missing hero " + GetObjectName(type) + " " + why);
      }
    }

    private static bool HaveHero(player p, int heroType)
    {
      // 0.37: a later form of the same legend counts too (AiConfig.HeroForms: Arthas UEAR -> N023 after "The
      // Ascension"), otherwise the bot would train the first form again
      var later = AiConfig.HeroForms.ContainsKey(heroType) ? AiConfig.HeroForms[heroType] : 0;
      var all = LegendaryHeroManager.GetAll();
      for (var i = 0; i < all.Count; i++)
      {
        var hu = all[i].Unit;
        if (hu == null || GetOwningPlayer(hu) != p || GetUnitTypeId(hu) == 0)
        {
          continue;
        }
        var actual = GetUnitTypeId(hu);
        var registered = all[i].UnitType;
        if (actual == heroType || registered == heroType || (later != 0 && (actual == later || registered == later)))
        {
          return true;
        }
      }

      return false;
    }

    private static bool TryReviveAtAltar(player p, unit altar)
    {
      if (SimpleBot.HasPerks(p))
      {
        return false; // 0.73.0: FreeRevive
      }

      var heroes = LegendaryHeroManager.GetAll();
      var pid = GetPlayerId(p);
      for (var offset = 0; offset < heroes.Count; offset++)
      {
        var i = (ReviveCursor[pid] + offset) % heroes.Count;
        var hero = heroes[i].Unit;
        if (hero == null || GetUnitTypeId(hero) == 0 || GetOwningPlayer(hero) != p
            || GetUnitState(hero, UNIT_STATE_LIFE) > 0.405f || GetUnitAbilityLevel(hero, SimpleBot.PermaDiesMarker) > 0
            || SimpleBot.PermaDead.Contains(heroes[i].UnitType) || p.GetPlayerData().GetObjectLimit(GetUnitTypeId(hero)) <= 0
            || AltarReserved(p, null, GetUnitTypeId(hero)))
        {
          continue;
        }
        if (OrderHero(p, altar, GetUnitTypeId(hero), hero))
        {
          ReviveCursor[pid] = i + 1;
          return true;
        }
        ReserveMissingFood(p, GetUnitTypeId(hero));
      }
      return false;
    }
  }
}
