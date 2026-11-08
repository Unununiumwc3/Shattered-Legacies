using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Shared;

namespace WarcraftLegacies.Source.Ai
{
  internal static class AiBuildPlan
  {
    internal static bool OnMenu(unit worker, int type)
    {
      if (AiBuildData.Menus.TryGetValue(GetUnitTypeId(worker), out var menu))
      {
        for (var i = 0; i < menu.Length; i++)
        {
          if (menu[i] == type)
          {
            return true;
          }
        }
      }
      return false;
    }

    // Specific-only=false includes WC3's dependency equivalents and upgraded halls.
    internal static int Missing(player p, int type)
    {
      if (AiBuildData.Required.TryGetValue(type, out var requirements))
      {
        for (var i = 0; i < requirements.Length; i++)
        {
          var required = requirements[i];
          var count = 1;
          while (i + 1 < requirements.Length && requirements[i + 1] == required)
          {
            i++;
            count++;
          }
          if (GetPlayerTechCount(p, required, false) < count)
          {
            return required;
          }
        }
      }
      return 0;
    }

    internal static List<int> WorkerBuildings(player p, List<unit> workers, List<int> military, Faction faction)
    {
      // Establish the first fighting producer and an altar early. The remaining
      // menu entries include prerequisite, supply, shop, tower and naval buildings.
      var types = new List<int>();
      for (var i = 0; i < military.Count; i++)
      {
        types.Add(military[i]);
      }
      SimpleBot.AddCategory(faction, UnitCategory.Altar, types);
      SimpleBot.AddCategory(faction, UnitCategory.Research, types);
      for (var w = 0; w < workers.Count; w++)
      {
        if (!AiBuildData.Menus.TryGetValue(GetUnitTypeId(workers[w]), out var menu))
        {
          continue;
        }
        for (var i = 0; i < menu.Length; i++)
        {
          if (p.GetPlayerData().GetObjectLimit(menu[i]) > 0 && !types.Contains(menu[i]))
          {
            types.Add(menu[i]);
          }
        }
      }
      for (var i = types.Count - 1; i >= 0; i--)
      {
        var available = false;
        for (var w = 0; w < workers.Count && !available; w++)
        {
          available = OnMenu(workers[w], types[i]);
        }
        if (!available || p.GetPlayerData().GetObjectLimit(types[i]) <= 0)
        {
          types.RemoveAt(i);
        }
      }
      return types;
    }

    internal static int NextHallTier(player p, unit hall)
    {
      if (!AiBuildData.Upgrades.TryGetValue(GetUnitTypeId(hall), out var options))
      {
        return 0;
      }
      for (var i = 0; i < options.Length; i++)
      {
        var type = options[i];
        if (p.GetPlayerData().GetObjectLimit(type) > 0 && Missing(p, type) == 0)
        {
          return type;
        }
      }
      return 0;
    }

    internal static bool UpgradeHall(player p, unit hall)
    {
      var target = NextHallTier(p, hall);
      if (target == 0 || GetUnitCurrentOrder(hall) != 0 || !AiTech.FreeOrder(p, hall, target))
      {
        return false;
      }
      SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " upgrading " + GetUnitName(hall) + " to " + GetObjectName(target));
      return true;
    }

    internal static bool UpgradeForCoverage(player p, AiBuildState s, List<unit> buildings, List<int> halls, int wanted)
    {
      if (s.Upgrading != null && GetUnitTypeId(s.Upgrading) == s.MorphFrom &&
          GetUnitState(s.Upgrading, UNIT_STATE_LIFE) > 0.405f && GetUnitCurrentOrder(s.Upgrading) != 0)
      {
        return true; // wait for the morph before making a second copy
      }
      s.Upgrading = null;
      for (var i = 0; i < buildings.Count; i++)
      {
        var source = buildings[i];
        var from = GetUnitTypeId(source);
        // Only ordinary upgrade paths. Scripted, timed and quest transformations have their own handlers.
        if (halls.Contains(from) || IsUnitType(source, UNIT_TYPE_TOWNHALL) || GetUnitCurrentOrder(source) != 0
            || !AiUpgrades.BuildingUpgrades.TryGetValue(from, out var options))
        {
          continue;
        }
        for (var k = 0; k < options.Count; k++)
        {
          var target = options[k];
          if (p.GetPlayerData().GetObjectLimit(target) < wanted || GetPlayerTechCount(p, target, true) >= wanted
              || (wanted == 2 && !AiBuildData.Products.ContainsKey(target)) || Missing(p, target) != 0)
          {
            continue;
          }
          if (AiTech.FreeOrder(p, source, target))
          {
            s.Upgrading = source;
            s.MorphFrom = from;
            SimpleBot.Log("builder: " + s.Faction + " upgrading " + GetUnitName(source) + " to " + GetObjectName(target));
            return true;
          }
        }
      }
      return false;
    }
  }
}
