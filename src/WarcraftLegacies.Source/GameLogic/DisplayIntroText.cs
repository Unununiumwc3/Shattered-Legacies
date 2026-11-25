using System;
using MacroTools.Extensions;
using MacroTools.Localization;
using MacroTools.Save;
using WCSharp.Shared;

namespace WarcraftLegacies.Source.GameLogic;

/// <summary>
/// Display intro text to all players after some period of time has elapsed.
/// </summary>
public static class DisplayIntroText
{
  /// <summary>
  /// Displays intro text to all players after some period of time has elapsed.
  /// </summary>
  /// <param name="displayTime">The time after which to display intro text, in seconds.</param>
  public static void Setup(float displayTime)
  {
    timer.Create().Start(displayTime, false, () =>
    {
      try
      {
        SaveManager.RunWhenLocalPlayerSettingsReady(() =>
        {
          foreach (var player in Util.EnumeratePlayers(playerslotstate.Playing, mapcontrol.User))
          {
            player.DisplayTextTo(player.GetPlayerData().Faction?.IntroText?.Invoke() ?? "");
          }
        });

        @event.ExpiredTimer.Dispose();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error displaying intro text {ex}");
      }
    });

    SaveManager.RunWhenLocalPlayerSettingsReady(() =>
    {
      foreach (var player in Util.EnumeratePlayers())
      {
        // [SL] (0.39.9): fork title and notice instead of WL's intro (requested by the original maintainer);
        // 0.75.0: the maintainer's text, translated (SlText)
        player.DisplayTimedTextTo(displayTime - 1, $"|cffffcc00{Loc.Get(SlText.Title)}|r\n"
                                                   + $"|cffaaaaaa{Loc.Get(SlText.IntroNotice)}|r\n\n"
                                                   + Loc.Get(SlText.IntroTurningPoints) + "\n"
                                                   + Loc.Get(SlText.IntroNewPlayer));
      }
    });
  }
}
