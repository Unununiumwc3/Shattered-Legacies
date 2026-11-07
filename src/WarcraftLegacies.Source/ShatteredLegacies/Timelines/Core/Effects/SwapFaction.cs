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
      NameSlot(p, successor);
      RegisterArriving(successor);
    }

    // registers a faction that arrives in the middle of the game: its quests' WL deadlines count from now
    // 0.57.3: and WL turn callbacks for turns already past run on the next turn (GameTimeManager.ClampPastTurns)
    internal static void RegisterArriving(Faction faction)
    {
      ObjectiveExpire.ExpirationOffset = GameTimeManager.Turn;
      GameTimeManager.ClampPastTurns = true;
      try
      {
        FactionManager.Register(faction);
      }
      finally
      {
        ObjectiveExpire.ExpirationOffset = 0;
        GameTimeManager.ClampPastTurns = false;
      }
    }

    // 0.57.3: a free slot (computer "NPC" in the lobby) carries its faction's name -- shown when its units are hovered.
    // 0.63.1 (maintainer: the Ashtongue's units still said "Illidari"): every slot that is not a human's carries the
    // lobby's faction name ("Illidari", "Kul Tiras") -- after a swap it carries the new faction's. A human keeps his.
    internal static void NameSlot(player p, Faction faction)
    {
      var human = GetPlayerController(p) == MAP_CONTROL_USER && GetPlayerSlotState(p) == PLAYER_SLOT_STATE_PLAYING;
      if (!human)
      {
        SetPlayerName(p, faction.Name);
        return;
      }

      for (var i = 0; i < SplitFaction.FreeSlotOrder.Length; i++)
      {
        if (SplitFaction.FreeSlotOrder[i] == GetPlayerId(p))
        {
          SetPlayerName(p, faction.Name);
        }
      }
    }
  }
}
