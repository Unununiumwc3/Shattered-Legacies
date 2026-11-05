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

    // 0.58: another mode -- e.g. HideNonStructures: the buildings stay visible (and invulnerable), so the place still
    // looks inhabited and nobody builds on it (the Kezan Enterprise's island)
    internal static List<unit> Prepare(Rectangle area, RescuePreparationMode mode) => area.PrepareUnitsForRescue(mode);

    internal static int Take(player p, List<unit> units, Faction faction) => Take(p, units, faction, null);

    // 0.58: `equivalents` (pairs: old type, new type) first -- preplaced objects of another map's versions (WL's creep
    // goblins) become the faction's copies (import_24p.py: <Faction>Objects.MainEquivalents); then WL's categories
    internal static int Take(player p, List<unit> units, Faction faction, int[] equivalents)
    {
      var taken = 0;
      for (var i = 0; i < units.Count; i++)
      {
        var u = units[i];
        if (GetUnitTypeId(u) == 0 || GetOwningPlayer(u) != Player(PLAYER_NEUTRAL_PASSIVE))
        {
          continue; // removed, or taken by someone else meanwhile
        }

        var replaced = Replace(u, equivalents);
        try
        {
          if (replaced == u)
          {
            replaced = u.ReplaceWithFactionEquivalent(faction);
          }
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

    // the unit replaced by its listed equivalent (same place, facing and share of hit points), or the unit itself
    private static unit Replace(unit u, int[] equivalents)
    {
      if (equivalents == null)
      {
        return u;
      }

      var t = GetUnitTypeId(u);
      for (var i = 0; i + 1 < equivalents.Length; i += 2)
      {
        if (equivalents[i] != t)
        {
          continue;
        }

        var life = GetUnitState(u, UNIT_STATE_LIFE) / GetUnitState(u, UNIT_STATE_MAX_LIFE);
        var x = GetUnitX(u);
        var y = GetUnitY(u);
        var facing = GetUnitFacing(u);
        var owner = GetOwningPlayer(u);
        RemoveUnit(u);
        var n = CreateUnit(owner, equivalents[i + 1], x, y, facing);
        SetUnitState(n, UNIT_STATE_LIFE, life * GetUnitState(n, UNIT_STATE_MAX_LIFE)); // Rescue follows at once
        return n;
      }

      return u;
    }
  }
}
