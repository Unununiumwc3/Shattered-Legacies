namespace MacroTools.Localization;

/// <summary>
/// [SL] (0.75.0) Shattered Legacies' own player-facing texts. The English text is the lookup key of the translations
/// (<c>Translations/ChineseTranslation.ShatteredLegacies.cs</c>), so the code that shows a text and its translation
/// use the same constant and can never drift apart. Show them through <see cref="Loc.Get(string)"/>.
/// </summary>
public static class SlText
{
  // game start (DisplayIntroText)
  public const string Title = "Shattered Legacies";
  public const string IntroNotice = "Unofficial map based on Warcraft Legacies. Not the official version.";
  public const string IntroTurningPoints =
    "Losing your capital is not the end. When a turning point appears, choose your path in the Hourglass of Fate (top right).";
  public const string IntroNewPlayer =
    "New player? Open the Quest log (F9) to see your objectives and how Shattered Legacies works.";

  // F9: the guide quest (InfoQuests)
  public const string GuideTitle = "How Shattered Legacies works";
  public const string GuideHistory = "History in Shattered Legacies can change.";
  public const string GuideFallbacks =
    "Fallbacks: when a faction loses its capital, it can fall back and return as a new faction.";
  public const string GuideBetrayals = "Betrayals: some factions can turn on their allies or leave their team.";
  public const string GuideHourglass =
    "Hourglass of Fate: turning points appear in the Hourglass (top right). When one becomes available, research it to take that path. Bots decide on their own.";
  public const string GuideDefeat =
    "Defeat: you only lose if you hold no Control Point for more than 5 turns. Losing your heroes does not defeat you.";
  public const string GuideBots = "Bots: bots take over empty slots and players who leave.";
  public const string GuideInDevelopment = "Many turning points are still in development.";

  // F9: credits (InfoQuests)
  public const string CreditsTitle = "Credits";
  public const string CreditsNotice =
    "Shattered Legacies: Botified Realm is an unofficial exploration/prototype based on Warcraft Legacies. It is not the official version and is not affiliated with the Warcraft Legacies team. It experiments with a graph-based bot system built with the help of LLMs.";
  public const string CreditsThanks = "The Warcraft Legacies team sends an enormous thank you to...";

  // hints (HintSetup)
  public const string HintFallbacks =
    "Losing your capital is not the end. Many factions fall back and return as a new faction.";
  public const string HintHourglass =
    "Turning points appear in the Hourglass of Fate (top right). Once one is available, research it to choose that path.";
  public const string HintDefeat =
    "You are only defeated if you hold no Control Point for more than 5 turns. One Control Point is enough to survive.";
  public const string HintLeavers = "When a player leaves, a bot takes over their faction.";
  public const string HintBotChoices = "Bots choose their turning points at random, so you can't predict what they will do.";
  public const string HintFork = "This map is an unofficial fork of Warcraft Legacies. You can find the original at {url}";
  public const string HintPatreon = "You can support the original Warcraft Legacies team on Patreon at {url}";
}
