using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "leave behind", step 3 of the inheritance rule (Timelines/SCENARIO_CONCEPTS.md section 6) the way WL does it
  // (PlayerExtensions.RemoveAllUnits, used by the Scarlet Crusade): units that can be removed are removed, the rest
  // (control points, capitals, gates ...) turns Neutral Hostile. Units in `keep` stay with the player.
  // 0.58: step 2 too -- ToHeir: everything goes to a lore heir that still plays on the player's team.
  internal static class LeaveBehind
  {
    // step 2 -> 3: the heir (a WL faction's name) gets everything but `keep` if it is played (a human or an active
    // bot) and on the player's team right now; otherwise step 3. Returns the heir's player, or null.
    internal static player ToHeir(player p, string heirName, List<unit> keep)
    {
      var team = p.GetPlayerData().Team;
      player heir = null;
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == heirName && f.Player != null && f.Player != p && AiApi.IsActive(f.Player) && team != null
            && f.Player.GetPlayerData().Team == team)
        {
          heir = f.Player;
        }
      }

      if (heir == null)
      {
        ToNeutralHostile(p, keep);
        return null;
      }

      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!keep.Contains(u))
        {
          SetUnitOwner(u, heir, true);
        }
      }

      DestroyGroup(g);
      return heir;
    }

    internal static void ToNeutralHostile(player p, List<unit> keep)
    {
      var hostile = Player(PLAYER_NEUTRAL_AGGRESSIVE);
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      var all = new List<unit>();
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        all.Add(u);
      }

      DestroyGroup(g);
      for (var i = 0; i < all.Count; i++)
      {
        u = all[i];
        if (keep.Contains(u))
        {
          continue;
        }

        if (u.IsRemovable())
        {
          u.SafelyRemove();
        }
        else
        {
          SetUnitOwner(u, hostile, true);
        }
      }
    }
  }
}
