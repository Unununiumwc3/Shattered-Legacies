using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;

namespace WarcraftLegacies.Source.Ai
{
  // 0.44.2: CP INCOME FOR BOTS. WL pays control-point income only to HUMAN slots that are playing
  // (MacroTools ControlPointManager: Util.EnumeratePlayers(playerslotstate.Playing, mapcontrol.User)). Computer slots
  // never got any: the strength-vote bonus was the bots' whole income -- with Veteran = 0 (0.43.1) they had none,
  // spent their starting gold in ~40 min and trained nothing for the rest of the game (0.44.1: 9.5-hour game, 16
  // bots, almost no unit after 40:00; only the maintainer's autopilot slot -- a human slot, paid by WL -- kept
  // producing). Now the bot pays every slot WL does not pay exactly what WL would: PlayerData.TotalIncome (CPs +
  // bonus income), per second, from turn 1, with WL's fractional carry. Bot-side only, no change to WL.
  internal static class AiIncome
  {
    private static float[] _partial = new float[24];
    internal static int[] Paid = new int[24];

    internal static void Setup()
    {
      AiScheduler.Add("bot income", 4, 2, Pay, false); // every 1 s, like WL's ControlPointManager (Period = 1)
    }

    // WL pays it itself: a human slot that is still playing (also when the bot plays it as autopilot)
    private static bool PaidByWl(player p)
    {
      return GetPlayerController(p) == MAP_CONTROL_USER && GetPlayerSlotState(p) == PLAYER_SLOT_STATE_PLAYING;
    }

    private static void Pay()
    {
      if (GameTimeManager.Turn < 1)
      {
        return; // WL starts paying at turn 1
      }

      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var p = bots[b];
        var pid = GetPlayerId(p);
        if (pid < 0 || pid >= 24 || PaidByWl(p))
        {
          continue;
        }

        var pd = p.GetPlayerData();
        if (pd.Faction == null)
        {
          continue;
        }

        _partial[pid] += pd.TotalIncome / 60f;
        var whole = (int)_partial[pid];
        if (whole > 0)
        {
          _partial[pid] -= whole;
          SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + whole);
          Paid[pid] += whole;
        }
      }
    }

    internal static void Lines(List<string> o)
    {
      var s = "bot income paid since start (0.44.2, CP income WL pays only to human slots):";
      for (var pid = 0; pid < 24; pid++)
      {
        if (Paid[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(Paid[pid]) + ";";
        }
      }

      o.Add(s);
    }
  }
}
