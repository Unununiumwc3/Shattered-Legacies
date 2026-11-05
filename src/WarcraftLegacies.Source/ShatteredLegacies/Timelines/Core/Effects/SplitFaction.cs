using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "split" (0.54): one player becomes two. The player's faction moves to a FREE slot with its units (except
  // `keep`), its team, its quests and its colour, and the bot plays it there; the player himself gets the successor
  // (a new faction, registered like any arriving faction). 0.55 (maintainer): the old faction keeps all gold and
  // lumber, the successor starts with a fixed amount (no abuse: a human cannot let himself be killed for a rich
  // start); the old faction's Hourglass of Fate comes along (a new one on the slot -- its turning points stay).
  // Free slots, in this order: P11, P14, P20 (Timelines/SCENARIO_CONCEPTS.md section 3; P6, P15, P18 hold preplaced
  // units). 0.57.1 (maintainer): they are computer slots "NPC" in the lobby (Info.json) -- a computer player gets the
  // bot's full treatment (production, its starting quests); they stay without a bot until a split hands them a
  // faction (AiApi.ReserveSlot / TakeOver). A slot is free while it has no faction and no human plays it.
  internal static class SplitFaction
  {
    internal static int[] FreeSlotOrder = { 10, 13, 19 }; // player ids of P11, P14, P20

    internal static player FindFreeSlot()
    {
      for (var i = 0; i < FreeSlotOrder.Length; i++)
      {
        var s = Player(FreeSlotOrder[i]);
        if (s.GetPlayerData().Faction == null
            && (GetPlayerSlotState(s) != PLAYER_SLOT_STATE_PLAYING || GetPlayerController(s) == MAP_CONTROL_COMPUTER))
        {
          return s;
        }
      }

      return null;
    }

    internal static void Run(player p, player slot, Faction successor, List<unit> keep, int startGold, int startLumber)
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

      UnallyOutsideTeam(slot); // 0.57.4

      if (old != null)
      {
        slot.GetPlayerData().Faction = old; // the old faction (and its colour) on the free slot
        SwapFaction.NameSlot(slot, old);    // 0.57.3: not "NPC"
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

      MoveResource(p, slot, PLAYER_STATE_RESOURCE_GOLD, startGold);
      MoveResource(p, slot, PLAYER_STATE_RESOURCE_LUMBER, startLumber);
      if (Hourglass.Get(GetPlayerId(p)) != null)
      {
        Hourglass.Give(slot);
      }

      SwapFaction.RegisterArriving(successor);
      if (old != null && !AiApi.TakeOver(slot))
      {
        AiApi.Log("timelines: the bot could not take over P" + I2S(GetPlayerId(slot) + 1));
      }
    }

    // 0.57.4: WL's teams only ally a player with his team; they never take alliances away. A free slot started with the
    // alliances of its lobby force -- until 0.57.4 the Scourge / Legion force (Info.json), so Quel'thalas on P11 and the
    // Scourge / Legion were allies (Legion units healed it, it only fought the Forsaken). Everyone outside the slot's
    // team becomes an enemy, both ways.
    private static readonly alliancetype[] Alliances =
    {
      ALLIANCE_PASSIVE, ALLIANCE_HELP_REQUEST, ALLIANCE_HELP_RESPONSE, ALLIANCE_SHARED_XP, ALLIANCE_SHARED_SPELLS,
      ALLIANCE_SHARED_VISION, ALLIANCE_SHARED_CONTROL, ALLIANCE_SHARED_ADVANCED_CONTROL,
    };

    internal static void UnallyOutsideTeam(player slot)
    {
      var team = slot.GetPlayerData().Team;
      for (var i = 0; i < 24; i++)
      {
        var other = Player(i);
        if (other == slot || (team != null && other.GetPlayerData().Team == team))
        {
          continue;
        }

        for (var a = 0; a < Alliances.Length; a++)
        {
          SetPlayerAlliance(slot, other, Alliances[a], false);
          SetPlayerAlliance(other, slot, Alliances[a], false);
        }
      }
    }

    // everything goes to the old faction; the player keeps `start`
    private static void MoveResource(player from, player to, playerstate state, int start)
    {
      SetPlayerState(to, state, GetPlayerState(to, state) + GetPlayerState(from, state));
      SetPlayerState(from, state, start);
    }
  }
}
