using System;
using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Utils;
using WarcraftLegacies.Source.Ai;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "relocate": a start area waits hidden until its turning point, then the player takes it over the way WL's
  // faction pick does (FactionChoiceDialogPresenter.ReplaceStartingUnitsWithFactionEquivalents) -- units replaced with
  // the faction's equivalents -- and WL's rescue (UnitExtensions.Rescue: owner, visible, unpaused, vulnerable).
  internal static class StartArea
  {
    // at map setup: the Neutral Passive units in the area are hidden, paused and invulnerable (WL's HideAll)
    internal static List<unit> Prepare(Rectangle area) => area.PrepareUnitsForRescue(RescuePreparationMode.HideAll);

    internal static int Take(player p, List<unit> units, Faction faction)
    {
      var taken = 0;
      for (var i = 0; i < units.Count; i++)
      {
        var u = units[i];
        if (GetUnitTypeId(u) == 0 || GetOwningPlayer(u) != Player(PLAYER_NEUTRAL_PASSIVE))
        {
          continue; // removed, or taken by someone else meanwhile
        }

        var replaced = u;
        try
        {
          replaced = u.ReplaceWithFactionEquivalent(faction);
        }
        catch (Exception ex)
        {
          AiApi.Log("timelines: start area unit kept as it is (" + ex.Message + ")");
        }

        replaced.Rescue(p);
        taken++;
      }

      return taken;
    }
  }
}
