using System;
using MacroTools.UserInterface;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Rules
{
  // 0.58: the fork's lines on WL's scoreboard (under the teams; [SL] hook FactionMultiboard.ExtraLines) -- the end
  // goals of factions (maintainer: the Kezan Enterprise's 20,000 gold, while it plays). A line is a function that
  // returns its text, or "" while it has nothing to show; refreshed every 2 s.
  public static class ScoreboardLines
  {
    internal const float RefreshEvery = 2f;

    internal static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, RefreshEvery, true, () => AiApi.Safe("scoreboard lines", FactionMultiboard.RefreshExtraLines));
    }

    internal static void Add(Func<string> line)
    {
      FactionMultiboard.ExtraLines.Add(line);
    }
  }
}
