using System.Collections.Generic;
using MacroTools.Extensions;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "leave behind", step 3 of the inheritance rule (Timelines/SCENARIO_CONCEPTS.md section 6) the way WL does it
  // (PlayerExtensions.RemoveAllUnits, used by the Scarlet Crusade): units that can be removed are removed, the rest
  // (control points, capitals, gates ...) turns Neutral Hostile. Units in `keep` stay with the player.
  internal static class LeaveBehind
  {
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
