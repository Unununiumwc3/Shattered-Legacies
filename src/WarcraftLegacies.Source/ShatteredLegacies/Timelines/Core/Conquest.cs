using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.63, a building block (maintainer): a VASSAL faction belongs to the team of whoever holds its SEAT (a CP). First
  // user: the Ashtongue (seat: the Black Temple's CP). Later: evil factions that are conquered instead of defeated.
  // - whenever the seat passes to a player P (not the vassal itself, not a neutral) whose team is not the vassal's, the
  //   vassal joins P's WHOLE team; the old team's alliances are removed (WL's teams only add alliances --
  //   SplitFaction.UnallyOutsideTeam), the scoreboard follows the team;
  // - anti-abuse: the vassal taking its seat itself changes nothing (it stays with its team); a later conquest by
  //   another player switches it again;
  // - the start: Register(vassal, seat, overlord) -- the overlord's team (the Ashtongue: who destroyed the Black
  //   Temple), or a team of its own when there is none.
  // Checked every 2 s (a CP has no event of its own here).
  internal static class Conquest
  {
    internal static List<player> Vassals = new();
    internal static List<int> Seats = new();
    internal static List<player> Holders = new(); // per vassal: who held the seat at the last check
    internal static bool Running;

    internal static void Register(player vassal, int seatCp, player overlord)
    {
      Vassals.Add(vassal);
      Seats.Add(seatCp);
      var cp = AiApi.SafeCp(seatCp);
      Holders.Add(cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null);
      if (overlord != null && GetPlayerId(overlord) < 24 && overlord != vassal)
      {
        Join(vassal, overlord);
      }
      else
      {
        vassal.GetPlayerData().Faction?.Unally(); // a team of its own
        SplitFaction.UnallyOutsideTeam(vassal);
      }

      if (!Running)
      {
        Running = true;
        var t = CreateTimer();
        TimerStart(t, 2f, true, () => AiApi.Safe("conquest", Check));
      }
    }

    private static void Check()
    {
      for (var i = 0; i < Vassals.Count; i++)
      {
        var cp = AiApi.SafeCp(Seats[i]);
        var holder = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
        if (holder == Holders[i])
        {
          continue;
        }

        Holders[i] = holder;
        var vassal = Vassals[i];
        if (holder == null || holder == vassal || GetPlayerId(holder) >= 24)
        {
          continue; // its own conquest (anti-abuse), or a neutral: nothing changes
        }

        if (holder.GetPlayerData().Team != vassal.GetPlayerData().Team)
        {
          Join(vassal, holder);
        }
      }
    }

    // the vassal joins the overlord's whole team, and only that
    private static void Join(player vassal, player overlord)
    {
      var team = overlord.GetPlayerData().Team;
      if (team == null)
      {
        return;
      }

      vassal.GetPlayerData().SetTeam(team);
      SplitFaction.UnallyOutsideTeam(vassal);
      var name = vassal.GetPlayerData().Faction?.Name ?? GetPlayerName(vassal);
      for (var j = 0; j < 24; j++)
      {
        DisplayTimedTextToPlayer(Player(j), 0, 0, 15, "|cffffcc00" + name + "|r now serve " + team.Name + ".");
      }

      AiApi.LogPin("conquest: " + name + " -> " + team.Name);
    }
  }
}
