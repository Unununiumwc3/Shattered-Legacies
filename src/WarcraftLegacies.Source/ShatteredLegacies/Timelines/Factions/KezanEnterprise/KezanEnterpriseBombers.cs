using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.61 (maintainer): the Fuel Transport Vehicle as a suicide bomber (it was of little use, and none for bots). Its
  // Detonate (AZq0, Faerie Fire based) is an autocast, switched on when the vehicle is trained -- so bots use it too:
  // while on, the vehicle blows up as soon as an enemy comes within 300; a manual cast on an enemy blows it up at
  // once. The blast: 500 + the vehicle's mana (its fuel -- the Oil Supply's Fuel Refill tops it up) to every unit and
  // building within 1000, allies and its own side included; a pillar of flame. The 24p skills stay.
  internal static class KezanEnterpriseBombers
  {
    private const int OrderOn = 852150;  // faeriefireon (Detonate's autocast)
    private const int OrderOff = 852151; // faeriefireoff
    private const string Effect = @"Abilities\Spells\Human\FlameStrike\FlameStrike1.mdl";

    internal static player Owner;
    internal static List<unit> Trucks = new();
    internal static List<bool> AutoOn = new();

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, () => AiApi.Safe("kezan bomber", () => Add(GetTrainedUnit())));
      var ordered = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(ordered, p, EVENT_PLAYER_UNIT_ISSUED_ORDER, null);
      TriggerAddAction(ordered, () => AiApi.Safe("kezan bomber toggle", OnOrder));
      var cast = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(cast, p, EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      TriggerAddAction(cast, () => AiApi.Safe("kezan bomber cast", OnCast));
      var tick = CreateTimer();
      TimerStart(tick, 0.5f, true, () => AiApi.Safe("kezan bomber watch", Watch));
    }

    private static void Add(unit u)
    {
      if (GetUnitAbilityLevel(u, KezanEnterpriseConfig.DetonateAbility) <= 0 || Trucks.Contains(u))
      {
        return;
      }

      Trucks.Add(u);
      AutoOn.Add(true);
      IssueImmediateOrderById(u, OrderOn);
    }

    private static void OnOrder()
    {
      var i = Trucks.IndexOf(GetTriggerUnit());
      if (i < 0)
      {
        return;
      }

      var order = GetIssuedOrderId();
      if (order == OrderOn)
      {
        AutoOn[i] = true;
      }
      else if (order == OrderOff)
      {
        AutoOn[i] = false;
      }
    }

    private static void OnCast()
    {
      if (GetSpellAbilityId() == KezanEnterpriseConfig.DetonateAbility)
      {
        Explode(GetTriggerUnit());
      }
    }

    // autocast on + an enemy close -> boom
    private static void Watch()
    {
      for (var i = Trucks.Count - 1; i >= 0; i--)
      {
        var u = Trucks[i];
        if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          Trucks.RemoveAt(i);
          AutoOn.RemoveAt(i);
          continue;
        }

        if (AutoOn[i] && EnemyNear(u))
        {
          Explode(u);
        }
      }
    }

    private static bool EnemyNear(unit u)
    {
      var owner = GetOwningPlayer(u);
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, GetUnitX(u), GetUnitY(u), KezanEnterpriseConfig.DetonateTrigger, null);
      var found = false;
      unit t;
      while ((t = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, t);
        if (!found && GetUnitState(t, UNIT_STATE_LIFE) > 0.405f && IsUnitEnemy(t, owner) && !IsUnitHidden(t)
            && GetOwningPlayer(t) != Player(PLAYER_NEUTRAL_PASSIVE) && IsUnitVisible(t, owner))
        {
          found = true;
        }
      }

      DestroyGroup(g);
      return found;
    }

    private static void Explode(unit u)
    {
      if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        return;
      }

      var x = GetUnitX(u);
      var y = GetUnitY(u);
      var damage = KezanEnterpriseConfig.DetonateDamage + GetUnitState(u, UNIT_STATE_MANA);
      var i = Trucks.IndexOf(u);
      if (i >= 0)
      {
        Trucks.RemoveAt(i);
        AutoOn.RemoveAt(i);
      }

      DestroyEffect(AddSpecialEffect(Effect, x, y));
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, KezanEnterpriseConfig.DetonateRadius, null);
      unit t;
      while ((t = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, t);
        if (t != u && GetUnitState(t, UNIT_STATE_LIFE) > 0.405f)
        {
          UnitDamageTarget(u, t, damage, true, false, ATTACK_TYPE_SIEGE, DAMAGE_TYPE_FIRE, WEAPON_TYPE_WHOKNOWS);
        }
      }

      DestroyGroup(g);
      KillUnit(u);
    }
  }
}
