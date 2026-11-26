using MacroTools.Extensions;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.85.0, a building block: heroes of a faction without an altar (the Centaur khans, the Defias' two heroes) come back
  // by themselves -- a dead hero of the player is revived at a spot after a delay, while the player plays that faction.
  internal static class HeroRevival
  {
    internal static void Register(player p, Faction faction, float x, float y, float delay)
    {
      var t = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_DEATH, null);
      TriggerAddAction(t, () =>
      {
        var hero = GetTriggerUnit();
        if (!IsUnitType(hero, UNIT_TYPE_HERO) || p.GetPlayerData().Faction != faction)
        {
          return;
        }

        var wait = CreateTimer();
        TimerStart(wait, delay, false, () =>
        {
          DestroyTimer(wait);
          if (GetUnitTypeId(hero) != 0 && GetUnitState(hero, UNIT_STATE_LIFE) <= 0.405f && GetOwningPlayer(hero) == p
              && p.GetPlayerData().Faction == faction)
          {
            ReviveHero(hero, x, y, true);
          }
        });
      });
    }
  }
}
