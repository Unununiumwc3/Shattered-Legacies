using System;
using System.Collections.Generic;

namespace MacroTools.UserInterface;

/// <summary>
/// Hook for the bot AI (WarcraftLegacies.Source/Ai): lets computer-controlled players pick an option of a
/// <see cref="ChoiceDialogPresenter{TChoice}"/> instead of always getting the default (option 1 after 24 s).
/// Never used for human players. Null (the default) keeps WL's behaviour for everyone.
/// </summary>
public static class BotChoicePolicy
{
  /// <summary>
  /// Called with the computer player and the ACTIVE choices; returns an index into that list, or -1 for the
  /// default behaviour. Must decide from the choice objects (types, factions, rectangles), never from
  /// <see cref="IChoice.Name"/>: names are localized per client, so comparing them could desync.
  /// Use synced randomness only (GetRandomInt).
  /// </summary>
  public static Func<player, IReadOnlyList<IChoice>, int>? Pick { get; set; }
}
