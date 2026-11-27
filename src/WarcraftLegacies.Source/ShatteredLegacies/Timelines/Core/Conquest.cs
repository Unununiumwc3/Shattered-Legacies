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
  // 0.76.0: second user, Skywall (seat: the Vortex Pinnacle CP) -- registered UNDEFEATABLE: it is never defeated
  // (EliminationRule), only conquered; its halls are indestructible (SkywallFallback).
  internal static class Conquest
  {
    internal static List<player> Vassals = new();
    internal static List<int> Seats = new();
    // 0.88.0: a seat that is not a CP -- who holds it, asked every check (Jaina's Sanctum, the Broodmother, the Claw of
    // Ursoc: Core/Effects/CaptureSeat); null for a CP seat
    // Include the vassal in the callback signature. CSharp.lua caches Func<player> at the same key prefix as Action<player>.
    internal static List<System.Func<player, player>> SeatHolders = new();
    internal static List<player> Holders = new(); // per vassal: who held the seat at the last check
    internal static HashSet<player> Undefeatable = new();
    // 0.87.0: a vassal's own line when its seat is taken (Gilneas' mercy text); none = the plain "now serve" line
    internal static Dictionary<player, string> ConquestTexts = new();
    internal static bool Running;

    // 0.81.0: keepTeam -- the vassal stays in its team now; only a LATER conquest of its seat moves it (Kezan)
    internal static void Register(player vassal, int seatCp, player overlord, bool undefeatable = false,
      bool keepTeam = false, string conquestText = null)
    {
      if (conquestText != null)
      {
        ConquestTexts[vassal] = conquestText;
      }

      if (undefeatable)
      {
        Undefeatable.Add(vassal);
      }

      Vassals.Add(vassal);
      Seats.Add(seatCp);
      SeatHolders.Add(null);
      var cp = AiApi.SafeCp(seatCp);
      Holders.Add(cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null);
      Start(vassal, overlord, keepTeam);
    }

    // 0.88.0: the seat is whatever holder(vassal) says (a captured unit, an artifact's carrier); null = nobody
    internal static void Register(player vassal, System.Func<player, player> holder, player overlord, bool undefeatable = false,
      bool keepTeam = false, string conquestText = null)
    {
      if (conquestText != null)
      {
        ConquestTexts[vassal] = conquestText;
      }

      if (undefeatable)
      {
        Undefeatable.Add(vassal);
      }

      Vassals.Add(vassal);
      Seats.Add(0);
      SeatHolders.Add(holder);
      Holders.Add(holder(vassal));
      Start(vassal, overlord, keepTeam);
    }

    // 0.88.0: a vassal that is its own master again (the Furbolgs while the Claw is theirs): no seat any more
    internal static void Release(player vassal)
    {
      var i = Vassals.IndexOf(vassal);
      if (i < 0)
      {
        return;
      }

      Vassals.RemoveAt(i);
      Seats.RemoveAt(i);
      SeatHolders.RemoveAt(i);
      Holders.RemoveAt(i);
    }

    private static void Start(player vassal, player overlord, bool keepTeam)
    {
      if (keepTeam)
      {
        // stays where it is
      }
      else if (overlord != null && GetPlayerId(overlord) < 24 && overlord != vassal)
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
        player holder;
        if (SeatHolders[i] != null)
        {
          holder = SeatHolders[i](Vassals[i]);
        }
        else
        {
          var cp = AiApi.SafeCp(Seats[i]);
          holder = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
        }

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
          if (ConquestTexts.TryGetValue(vassal, out var text))
          {
            for (var j = 0; j < 24; j++)
            {
              DisplayTimedTextToPlayer(Player(j), 0, 0, 20, text);
            }
          }

          Join(vassal, holder);
        }
      }
    }

    // the vassal joins the overlord's whole team, and only that
    internal static void Join(player vassal, player overlord)
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
