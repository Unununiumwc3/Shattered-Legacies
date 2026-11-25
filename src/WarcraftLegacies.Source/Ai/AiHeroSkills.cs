using MacroTools.Legends;

namespace WarcraftLegacies.Source.Ai
{
  // 0.38: bot heroes learn their skills. Order per hero type from AiData.HeroSkills (generated): auras/passives
  // first, then the other skills in map order, the attribute bonus last. 0.73: each free point goes to the
  // learnable skill with the lowest level (round robin, see LearnOne); WC3 refuses maxed skills and ultimates
  // below level 6 by itself.
  // Runs on every hero level-up of a bot (event) and every 15 s for heroes with unspent points (heroes given by
  // quests, revived heroes, new forms). No unit scans.
  internal static class AiHeroSkills
  {
    private static trigger _trig;

    internal static void Setup()
    {
      _trig = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_trig, Player(i), EVENT_PLAYER_HERO_LEVEL, null);
      }

      TriggerAddAction(_trig, OnLevel);
    }

    private static void OnLevel()
    {
      var u = GetLevelingUnit();
      if (SimpleBot.IsBot(GetOwningPlayer(u)))
      {
        Learn(u);
      }
    }

    // wave tick: legends of bots with unspent points
    internal static void Run()
    {
      var all = LegendaryHeroManager.GetAll();
      for (var i = 0; i < all.Count; i++)
      {
        var u = all[i].Unit;
        if (u != null && GetUnitTypeId(u) != 0 && SimpleBot.IsBot(GetOwningPlayer(u)) && GetHeroSkillPoints(u) > 0)
        {
          Learn(u);
        }
      }
    }

    internal static void Learn(unit u)
    {
      var t = GetUnitTypeId(u);
      if (!AiData.HeroSkills.ContainsKey(t))
      {
        return;
      }

      var order = AiData.HeroSkills[t];
      var guard = 0;
      while (GetHeroSkillPoints(u) > 0 && guard < 10)
      {
        guard++;
        if (!LearnOne(u, order))
        {
          return; // nothing learnable right now (all maxed / level too low)
        }
      }
    }

    // 0.73: round robin. The point goes to the learnable skill with the LOWEST current level (ties: list order),
    // so every active spell is learned by hero level 3-4 instead of maxing the aura first. The attribute bonus
    // (Aamk) only when nothing else can be learned.
    private static bool LearnOne(unit u, int[] order)
    {
      var tried = 0;
      for (var pass = 0; pass < order.Length; pass++)
      {
        var best = -1;
        var bestLevel = 99;
        for (var k = 0; k < order.Length; k++)
        {
          if (order[k] == Attribute || (tried & (1 << k)) != 0)
          {
            continue;
          }

          var lvl = GetUnitAbilityLevel(u, order[k]);
          if (lvl < bestLevel)
          {
            best = k;
            bestLevel = lvl;
          }
        }

        if (best < 0)
        {
          break;
        }

        tried |= 1 << best;
        var before = GetHeroSkillPoints(u);
        SelectHeroSkill(u, order[best]);
        if (GetHeroSkillPoints(u) < before)
        {
          return true;
        }
      }

      for (var k = 0; k < order.Length; k++)
      {
        if (order[k] == Attribute)
        {
          var before = GetHeroSkillPoints(u);
          SelectHeroSkill(u, order[k]);
          return GetHeroSkillPoints(u) < before;
        }
      }

      return false;
    }

    private const int Attribute = 1096904043; // Aamk
  }
}
