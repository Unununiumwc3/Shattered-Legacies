using MacroTools.ControlPoints;
using MacroTools.Extensions;
using MacroTools.GameTime;
using MacroTools.Legends;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): KALIMDOR <-> NORTHREND, late game only. Two harbours open (a waygate pair, open to everyone --
  // the bots read live waygates by themselves, AiRoutes.CheckPortals) when EITHER
  // - one team holds 70 % of Kalimdor's CPs (checked every turn from turn 30) -- a winner on Kalimdor, or
  // - Lordaeron, Dalaran and Quel'thalas have all fallen (their capitals lost) -- the Scourge's way south.
  // Northrend: the Borean Tundra's coast (-8600, 15400); Kalimdor: Moonglade's north shore (-13600, 11900).
  internal static class KalimdorCrossing
  {
    private const int FromTurn = 30;
    private const float Share = 0.7f;
    private const float NorthX = -8600f;
    private const float NorthY = 15400f;
    private const float SouthX = -13600f;
    private const float SouthY = 11900f;
    private static bool _open;

    internal static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, 10f, true, () => AiApi.Safe("kalimdor crossing", Check));
    }

    private static void Check()
    {
      if (_open)
      {
        return;
      }

      if (NorthFallen())
      {
        Open("Lordaeron, Dalaran and Quel'thalas have fallen. The Scourge's ships set sail for Kalimdor.");
        return;
      }

      if (GameTimeManager.Turn >= FromTurn && KalimdorWinner(out var team))
      {
        Open(team + " rules Kalimdor. Its fleets sail north -- and Northrend's ships can reach Kalimdor in turn.");
      }
    }

    // The capitals start neutral while factions are being assigned their bases.
    // Count a live capital as conquered only when an enemy player actually holds it.
    private static bool Lost(Capital capital, player defender)
    {
      if (capital == null || capital.Unit == null)
      {
        return false;
      }

      var u = capital.Unit;
      if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        return true;
      }

      var owner = GetOwningPlayer(u);
      return GetPlayerId(owner) < 24 && IsPlayerEnemy(owner, defender);
    }

    private static bool NorthFallen() =>
      Lost(AllLegends.Lordaeron.CapitalPalace, Player(9)) && Lost(AllLegends.Dalaran.Dalaran, Player(7))
      && Lost(AllLegends.Quel.Silvermoon, Player(2));

    // one team holds at least 70 % of the CPs on Kalimdor
    private static bool KalimdorWinner(out string team)
    {
      team = null;
      var counts = new System.Collections.Generic.Dictionary<string, int>();
      var total = 0;
      foreach (var cp in ControlPointManager.Instance.GetAllControlPoints())
      {
        var u = cp.Unit;
        if (u == null || AiApi.Landmass(GetUnitX(u), GetUnitY(u)) != 1)
        {
          continue;
        }

        total++;
        var owner = GetOwningPlayer(u);
        var name = GetPlayerId(owner) < 24 ? owner.GetPlayerData().Team?.Name : null;
        if (name != null)
        {
          counts[name] = (counts.TryGetValue(name, out var c) ? c : 0) + 1;
        }
      }

      foreach (var kv in counts)
      {
        if (total > 0 && kv.Value >= Share * total)
        {
          team = kv.Key;
          return true;
        }
      }

      return false;
    }

    private static void Open(string why)
    {
      _open = true;
      var passive = Player(PLAYER_NEUTRAL_PASSIVE);
      var north = CreateUnit(passive, Objects088.DockNorthrend, NorthX, NorthY, 270f);
      var south = CreateUnit(passive, Objects088.DockKalimdor, SouthX, SouthY, 270f);
      SetUnitInvulnerable(north, true);
      SetUnitInvulnerable(south, true);
      WaygateSetDestination(north, SouthX, SouthY - 250f);
      WaygateSetDestination(south, NorthX, NorthY - 250f);
      WaygateActivate(north, true);
      WaygateActivate(south, true);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 25, "|cffffcc00The sea route between Northrend and Kalimdor is "
          + "open!|r " + why + " Harbours: the Borean Tundra's coast and Moonglade's north shore.");
      }

      PingMinimap(NorthX, NorthY, 6f);
      PingMinimap(SouthX, SouthY, 6f);
      AiApi.LogPin("Kalimdor <-> Northrend crossing open");
    }
  }
}
