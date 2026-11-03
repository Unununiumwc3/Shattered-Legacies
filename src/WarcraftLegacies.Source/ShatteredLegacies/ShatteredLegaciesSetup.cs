using WarcraftLegacies.Source.ShatteredLegacies.Bot;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines;

namespace WarcraftLegacies.Source.ShatteredLegacies
{
  // 0.39.10: entry point of the Shattered Legacies code. Called from WL's Setup/GameSetup.cs right after the bot's
  // AiSetup.Setup() ([SL] line). Layers (README.md): this code may use the bot, but only through Ai/AiApi.cs; the bot
  // never uses this code (tools/shattered-legacies/ai/check_refs.py, LAYER / API).
  public static class ShatteredLegaciesSetup
  {
    public static void Setup()
    {
      SlBotSetup.Setup();    // the fork's data for the bot (Bot/)
      TimelineSetup.Setup(); // turning points: fallbacks, sub paths, betrayals, picks (Timelines/)
    }
  }
}
