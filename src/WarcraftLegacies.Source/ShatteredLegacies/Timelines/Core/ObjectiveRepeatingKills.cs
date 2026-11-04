using System;
using MacroTools.Extensions;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.56: a REPEATING objective (a building block for repeatable quests, e.g. the Forsaken's "Plague Research"): every
  // `count` enemy units the faction kills give `reward`, and the count starts again. It never completes, so its quest
  // stays in the log as the running offer. Structures and summoned units do not count.
  internal sealed class ObjectiveRepeatingKills : Objective
  {
    private readonly int _count;
    private readonly string _what;
    private readonly Action<player> _reward;
    private int _kills;

    public ObjectiveRepeatingKills(int count, string what, Action<player> reward)
    {
      _count = count;
      _what = what;
      _reward = reward;
      Refresh();
      var t = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(t, Player(i), EVENT_PLAYER_UNIT_DEATH, null);
      }

      TriggerAddAction(t, OnDeath);
    }

    private void Refresh() => SetDescription("Kill " + I2S(_count) + " enemy units (" + I2S(_kills) + "/" + I2S(_count)
                                             + ") -- " + _what + ", again and again");

    private void OnDeath()
    {
      if (EligibleFactions.Count == 0 || EligibleFactions[0].Player == null)
      {
        return;
      }

      var p = EligibleFactions[0].Player;
      var killer = GetKillingUnit();
      var dying = GetDyingUnit();
      if (killer == null || GetOwningPlayer(killer) != p || !IsUnitEnemy(dying, p)
          || IsUnitType(dying, UNIT_TYPE_STRUCTURE) || IsUnitType(dying, UNIT_TYPE_SUMMONED))
      {
        return;
      }

      _kills++;
      if (_kills >= _count)
      {
        _kills = 0;
        _reward(p);
      }

      Refresh();
    }
  }
}
