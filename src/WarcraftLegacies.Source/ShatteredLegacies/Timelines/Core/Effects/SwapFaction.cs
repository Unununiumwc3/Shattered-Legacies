using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Objectives.TurnBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "swap faction" (full turning point): the player gets a new Faction object -- WL's Scarlet Crusade pattern
  // (QuestScarletCrusade.AssignScarletCrusadeFaction): object levels copied, faction assigned, registered (its
  // OnRegistered adds its quests).
  // - Ground rule: the successor keeps the player's colour -- on the units and on the scoreboard (Faction.SetPlayerColor,
  //   [SL] in MacroTools; 0.53 fix: the scoreboard showed the successor's own colour).
  // - Its quests' WL deadlines ("Turn 10 hasn't started") count from its arrival, not from the game start
  //   (ObjectiveExpire.ExpirationOffset, [SL]; 0.53 fix: they would have failed at turn 10).
  internal static class SwapFaction
  {
    internal static void Run(player p, Faction successor)
    {
      var data = p.GetPlayerData();
      successor.SetPlayerColor(GetPlayerColor(p));
      var old = data.Faction;
      if (old != null)
      {
        successor.CopyObjectLevelsFrom(old);
      }

      data.Faction = successor;
      RegisterArriving(successor);
    }

    // registers a faction that arrives in the middle of the game: its quests' WL deadlines count from now
    internal static void RegisterArriving(Faction faction)
    {
      ObjectiveExpire.ExpirationOffset = GameTimeManager.Turn;
      try
      {
        FactionManager.Register(faction);
      }
      finally
      {
        ObjectiveExpire.ExpirationOffset = 0;
      }
    }
  }
}
