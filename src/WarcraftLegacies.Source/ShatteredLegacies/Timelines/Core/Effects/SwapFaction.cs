using MacroTools.Extensions;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "swap faction" (full turning point): the player gets a new Faction object -- WL's Scarlet Crusade pattern
  // (QuestScarletCrusade.AssignScarletCrusadeFaction): object levels copied, faction assigned, registered (its
  // OnRegistered adds its quests). Ground rule: the successor keeps the player's colour (WL would switch to the new
  // faction's colour).
  internal static class SwapFaction
  {
    internal static void Run(player p, Faction successor)
    {
      var data = p.GetPlayerData();
      var colour = GetPlayerColor(p);
      var old = data.Faction;
      if (old != null)
      {
        successor.CopyObjectLevelsFrom(old);
      }

      data.Faction = successor;
      FactionManager.Register(successor);
      KeepColour(p, colour);
    }

    private static void KeepColour(player p, playercolor colour)
    {
      SetPlayerColor(p, colour);
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SetUnitColor(u, colour);
      }

      DestroyGroup(g);
    }
  }
}
