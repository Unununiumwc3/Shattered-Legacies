using System.Reflection;
using System.Text.RegularExpressions;
using MacroTools.Localization;
using MacroTools.Localization.Translations;
using MacroTools.TestSupport;

namespace MacroTools.Tests.Localization;

public sealed class ChineseTranslationTests : LocTestsBase
{
  [Fact]
  public void ChineseDictionaryIsRegisteredAndUnknownForkTextFallsBack()
  {
    Assert.Equal("部落", Loc.Get("Horde", "zh"));
    Assert.Equal("计分板", Loc.Get("Scoreboard", "zh"));
    Assert.Equal("第 15 回合", Loc.Format("Turn {turn}", "zh", ("{turn}", "15")));
    Assert.Equal("Twillight custom fallback text", Loc.Get("Twillight custom fallback text", "zh"));
    Assert.Equal("Horde", Loc.Get("Horde", "en"));
  }

  [Theory]
  [InlineData("zhCN", true)]
  [InlineData("zh_CN", true)]
  [InlineData("zh-Hans", true)]
  [InlineData("zhTW", true)]
  [InlineData("enUS", false)]
  public void ChineseLocaleVariantsResolve(string reported, bool expected)
  {
    var match = typeof(Loc).GetMethod("MatchesLocale", BindingFlags.NonPublic | BindingFlags.Static)!;
    Assert.Equal(expected, (bool)match.Invoke(null, new object[] { reported, "zhCN" })!);
  }

  [Fact]
  public void ChineseDictionaryRetainsFormattingTokens()
  {
    var token = new Regex(@"\{[A-Za-z][A-Za-z0-9_]*\}");
    foreach (var pair in new ChineseTranslation().Entries)
    {
      var english = token.Matches(pair.Key).Select(m => m.Value).Distinct().Order().ToArray();
      var chinese = token.Matches(pair.Value).Select(m => m.Value).Distinct().Order().ToArray();
      Assert.True(english.SequenceEqual(chinese), $"Placeholder mismatch: {pair.Key}");
    }
  }

  [Fact]
  public void LiteralPercentAndNestedTranslationSurviveFormatting()
  {
    Loc.SetTranslations(new Dictionary<string, IReadOnlyDictionary<string, string>>
    {
      ["zh"] = new Dictionary<string, string> { ["Effect: {value}"] = "效果：{value}" }
    });
    Assert.Equal("效果：35%", LocalizedText.Create("Effect: {value}", LocalizedTextArg.Create("{value}", "35%")).ToString("zh"));
  }
}
