using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Events;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.EbonBlade
{
  // 0.88.0 (maintainer): ACHERUS FLIES. Once all four death knights are level 10, Acherus can research "The Ebon Hold
  // Rises" (a minute: the four channel) -> the building becomes a flying transport (Objects088.AcherusFlying) with
  // Remorseless Winter, Army of the Dead and Death's Advance; its death knights are revived at it (60 s), it gives the
  // food. No landing spots: it goes where its master sends it (bots: the bot's own movement, to be playtested).
  // Brought to 0 life it never falls: it burns, hangs still and invulnerable for 60 seconds and mends meanwhile (its
  // life is the timer). Attacks never change its owner -- only its team follows Icecrown Citadel's holder (Conquest).
  internal static class AcherusFlight
  {
    private const int Level = 10;
    private const float Mend = 60f;
    private const float Revive = 60f;
    private static unit _flyer;
    private static bool _burning;

    internal static void Start(player p, unit hold)
    {
      var t = CreateTimer();
      TimerStart(t, 5f, true, () => AiApi.Safe("acherus rises", () =>
      {
        if (_flyer != null || GetUnitTypeId(hold) == 0)
        {
          return;
        }

        if (GetPlayerTechCount(p, Objects088.EbonHoldRisesCondition, true) == 0 && AllKnightsReady(p))
        {
          SetPlayerTechResearched(p, Objects088.EbonHoldRisesCondition, 1);
          DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00The four death knights are ready.|r Acherus can rise: "
            + "research The Ebon Hold Rises at Acherus.");
        }

        if (AiApi.IsBot(p) && GetPlayerTechCount(p, Objects088.EbonHoldRisesCondition, true) > 0)
        {
          AiApi.FreeOrder(p, hold, Objects088.EbonHoldRises);
        }
      }));

      var done = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(done, p, EVENT_PLAYER_UNIT_RESEARCH_FINISH, null);
      TriggerAddAction(done, () =>
      {
        if (GetResearched() == Objects088.EbonHoldRises && _flyer == null)
        {
          AiApi.Safe("acherus lift", () => Rise(p, GetTriggerUnit()));
        }
      });
    }

    private static bool AllKnightsReady(player p)
    {
      var ready = 0;
      for (var i = 0; i < EbonBladeFallback.Heroes.Length; i++)
      {
        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        var found = false;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (!found && GetUnitTypeId(u) == EbonBladeFallback.Heroes[i] && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
              && GetHeroLevel(u) >= Level)
          {
            found = true;
          }
        }

        DestroyGroup(g);
        if (found)
        {
          ready++;
        }
      }

      return ready == EbonBladeFallback.Heroes.Length;
    }

    private static void Rise(player p, unit hold)
    {
      var x = GetUnitX(hold);
      var y = GetUnitY(hold);
      var share = GetUnitState(hold, UNIT_STATE_LIFE) / GetUnitState(hold, UNIT_STATE_MAX_LIFE);
      RemoveUnit(hold);
      _flyer = CreateUnit(p, Objects088.AcherusFlying, x, y, 270f);
      UnitAddAbility(_flyer, Objects088.RemorselessWinter);
      UnitAddAbility(_flyer, Objects088.ArmyOfTheDead);
      UnitAddAbility(_flyer, Objects088.DeathsAdvance);
      SetUnitState(_flyer, UNIT_STATE_LIFE, GetUnitState(_flyer, UNIT_STATE_MAX_LIFE) * (share > 0.05f ? share : 1f));
      Undying(_flyer);
      Revival(p);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, "|cffffcc00Acherus rises!|r Four death knights channel their "
          + "will into the necropolis, and the Ebon Hold lifts into the sky.");
      }

      PingMinimap(x, y, 5f);
      AiApi.LogPin("Ebon Blade: Acherus flies");
    }

    // 0 life: it burns, hangs still and invulnerable, mends to full in 60 s
    private static void Undying(unit flyer)
    {
      var hit = trigger.Create();
      hit.RegisterUnitEvent(flyer, unitevent.Damaged);
      hit.AddAction(() =>
      {
        if (_burning || @event.Damage + 1 < flyer.Life)
        {
          if (_burning)
          {
            @event.Damage = 0;
          }

          return;
        }

        @event.Damage = 0;
        _burning = true;
        flyer.Life = 1;
        SetUnitInvulnerable(flyer, true);
        PauseUnit(flyer, true);
        var fire = AddSpecialEffectTarget("Environment\\LargeBuildingFire\\LargeBuildingFire1.mdl", flyer, "origin");
        var step = GetUnitState(flyer, UNIT_STATE_MAX_LIFE) / Mend;
        var ticks = 0;
        var t = CreateTimer();
        TimerStart(t, 1f, true, () =>
        {
          ticks++;
          SetUnitState(flyer, UNIT_STATE_LIFE, GetUnitState(flyer, UNIT_STATE_LIFE) + step);
          if (ticks < Mend)
          {
            return;
          }

          DestroyTimer(t);
          DestroyEffect(fire);
          PauseUnit(flyer, false);
          SetUnitInvulnerable(flyer, false);
          SetUnitState(flyer, UNIT_STATE_LIFE, GetUnitState(flyer, UNIT_STATE_MAX_LIFE));
          _burning = false;
        });
      });
    }

    // its death knights come back at Acherus, wherever it is
    private static void Revival(player p)
    {
      var t = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_DEATH, null);
      TriggerAddAction(t, () =>
      {
        var hero = GetTriggerUnit();
        if (!IsUnitType(hero, UNIT_TYPE_HERO))
        {
          return;
        }

        var wait = CreateTimer();
        TimerStart(wait, Revive, false, () =>
        {
          DestroyTimer(wait);
          if (GetUnitTypeId(hero) != 0 && GetUnitState(hero, UNIT_STATE_LIFE) <= 0.405f && GetOwningPlayer(hero) == p
              && _flyer != null)
          {
            ReviveHero(hero, GetUnitX(_flyer), GetUnitY(_flyer) - 200f, true);
          }
        });
      });
    }
  }
}
