using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "split" (0.54): one player becomes two. The player's faction moves to a FREE slot with its units (except
  // `keep`), its team, its quests and its colour, and the bot plays it there; the player himself gets the successor
  // (a new faction, registered like any arriving faction). Gold and lumber are shared half and half.
  // Free slots, in this order: P11, P14, P20 (Timelines/SCENARIO_CONCEPTS.md section 3; P6, P15, P18 hold preplaced
  // units). A slot is free while it has no faction and nobody plays it.
  internal static class SplitFaction
  {
    internal static int[] FreeSlotOrder = { 10, 13, 19 }; // player ids of P11, P14, P20

    internal static player FindFreeSlot()
    {
      for (var i = 0; i < FreeSlotOrder.Length; i++)
      {
        var s = Player(FreeSlotOrder[i]);
        if (s.GetPlayerData().Faction == null && GetPlayerSlotState(s) != PLAYER_SLOT_STATE_PLAYING)
        {
          return s;
        }
      }

      return null;
    }

    internal static void Run(player p, player slot, Faction successor, List<unit> keep)
    {
      var data = p.GetPlayerData();
      var old = data.Faction;
      var team = data.Team;

      // factions first (so every unit that changes owner below lands with a faction): the successor on the player --
      // which takes the old faction off him (WL unapplies its limits / levels) -- then the old one on the free slot
      data.Faction = successor;
      if (team != null)
      {
        slot.GetPlayerData().SetTeam(team);
      }

      if (old != null)
      {
        slot.GetPlayerData().Faction = old; // the old faction (and its colour) on the free slot
      }

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
        if (!keep.Contains(all[i]))
        {
          SetUnitOwner(all[i], slot, true);
        }
      }

      ShareResource(p, slot, PLAYER_STATE_RESOURCE_GOLD);
      ShareResource(p, slot, PLAYER_STATE_RESOURCE_LUMBER);

      SwapFaction.RegisterArriving(successor);
      if (old != null && !AiApi.TakeOver(slot))
      {
        AiApi.Log("timelines: the bot could not take over P" + I2S(GetPlayerId(slot) + 1));
      }
    }

    private static void ShareResource(player from, player to, playerstate state)
    {
      var amount = GetPlayerState(from, state);
      var half = amount / 2;
      SetPlayerState(from, state, amount - half);
      SetPlayerState(to, state, GetPlayerState(to, state) + half);
    }
  }
}
