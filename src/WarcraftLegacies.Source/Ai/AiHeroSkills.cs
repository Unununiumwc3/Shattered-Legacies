using MacroTools.Legends;

namespace WarcraftLegacies.Source.Ai
{
  // 0.38: bot heroes learn their skills. Order per hero type from AiData.HeroSkills (generated): auras/passives
  // first, then the other skills in map order, the attribute bonus last. Each free point goes to the FIRST skill in
  // that order that can still be learned (WC3 refuses maxed skills and ultimates below level 6 by itself).
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
        var learned = false;
        for (var k = 0; k < order.Length && !learned; k++)
        {
          var before = GetHeroSkillPoints(u);
          SelectHeroSkill(u, order[k]);
          learned = GetHeroSkillPoints(u) < before;
        }

        if (!learned)
        {
          return; // nothing learnable right now (all maxed / level too low)
        }
      }
    }
  }
}
