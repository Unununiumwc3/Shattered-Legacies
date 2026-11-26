using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.85.0: who beat a faction -- the LAST player (slots 0-23) to kill or take one of its HQ units, by unit type: a
  // preplaced building (Stormwind Keep, Thunder Bluff) or a hero that is created later (N'Zoth). The new fallbacks swear
  // their first oath to him (Core/Conquest.cs).
  internal static class LastConqueror
  {
    private static readonly Dictionary<string, player> _by = new();

    internal static player Of(string factionName) => _by.TryGetValue(factionName, out var p) ? p : null;

    internal static void Watch(int unitType, string factionName)
    {
      var death = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(death, Player(i), EVENT_PLAYER_UNIT_DEATH, null);
      }

      TriggerAddAction(death, () =>
      {
        var dead = GetTriggerUnit();
        var killer = GetKillingUnit();
        if (GetUnitTypeId(dead) == unitType && killer != null && GetPlayerId(GetOwningPlayer(killer)) < 24
            && GetOwningPlayer(killer) != GetOwningPlayer(dead)
            && GetOwningPlayer(dead).GetPlayerData().Faction?.Name == factionName)
        {
          _by[factionName] = GetOwningPlayer(killer);
        }
      });
      if (!AllPreplacedWidgets.Units.TryGetAll(unitType, out var hqs))
      {
        return;
      }

      for (var i = 0; i < hqs.Count; i++)
      {
        var hq = hqs[i];
        var capture = CreateTrigger();
        TriggerRegisterUnitEvent(capture, hq, EVENT_UNIT_CHANGE_OWNER);
        TriggerAddAction(capture, () =>
        {
          var owner = GetOwningPlayer(hq);
          if (GetPlayerId(owner) < 24 && GetChangingUnitPrevOwner().GetPlayerData().Faction?.Name == factionName)
          {
            _by[factionName] = owner;
          }
        });
      }
    }
  }
}
