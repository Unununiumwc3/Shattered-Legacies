using MacroTools.Artifacts;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIronForces;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): creep bosses and their artifacts.
  // - Majordomo Executus (Shadowforge's west throne, 0.87): his kill drops BENEDICTION -- the Eye of Divinity from his
  //   Cache of the Firelord in its holy form (no Anathema for now).
  // - Mini-bosses that HOLD an artifact (dropped when they die): Mmmrrrggglll, the Grimscale chieftain (the 24p's
  //   hero) with the Pearl of the Grimscale at the murloc camp by the coast (11164, 2104); Snarlmane, chief of the
  //   Blackmane gnolls, with the Blackmane Warbanner at the gnoll camp (16744, -9216). Both Neutral Hostile.
  internal static class BossDrops
  {
    internal static void Setup()
    {
      var boss = AllPreplacedWidgets.Units.Get(DarkIronForcesObjects.Majordomo);
      var death = CreateTrigger();
      TriggerRegisterUnitEvent(death, boss, EVENT_UNIT_DEATH);
      TriggerAddAction(death, () =>
      {
        var x = GetUnitX(boss);
        var y = GetUnitY(boss);
        DestroyTrigger(death);
        AiApi.LogPin("Majordomo Executus died");
        // Register the artifact's event handlers after the death event has finished dispatching.
        var dropTimer = CreateTimer();
        TimerStart(dropTimer, 0f, false, () =>
        {
          DestroyTimer(dropTimer);
          AiApi.Safe("Majordomo loot", () =>
          {
            Drop(Objects088.Benediction, x, y, "Majordomo Executus has fallen. From his Cache of the "
              + "Firelord rolls the Eye of Divinity -- |cffffcc00Benediction|r.");
            AiApi.LogPin("Majordomo loot complete");
          });
        });
      });

      var t = CreateTimer();
      TimerStart(t, 2f, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("mini-bosses", () =>
        {
          Boss(Objects088.Mmmrrrggglll, 11164f, 2104f, Objects088.PearlOfTheGrimscale, true);
          Boss(Objects088.Snarlmane, 16744f, -9216f, Objects088.BlackmaneWarbanner, false);
        });
      });
    }

    private static void Boss(int type, float x, float y, int artifact, bool hero)
    {
      var u = CreateUnit(Player(PLAYER_NEUTRAL_AGGRESSIVE), type, x, y, 270f);
      if (hero)
      {
        SetHeroLevel(u, 8, false);
      }

      var it = CreateItem(artifact, x, y);
      ArtifactManager.Register(new Artifact(it));
      UnitAddItem(u, it);
    }

    private static void Drop(int type, float x, float y, string text)
    {
      var it = CreateItem(type, x, y);
      ArtifactManager.Register(new Artifact(it));
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 15, text);
      }
    }
  }
}
