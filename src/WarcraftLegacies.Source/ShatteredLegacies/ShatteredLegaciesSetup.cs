using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies
{
  // 0.39.10: entry point of the Shattered Legacies code (fallbacks, Kezan Enterprise). Called from WL's
  // Setup/GameSetup.cs right after the bot's AiSetup.Setup() ([SL] line). Rule: this code may use the bot, but only
  // through AiApi; the bot never uses this code (tools/ai/check_refs.py, LAYER).
  public static class ShatteredLegaciesSetup
  {
    public static void Setup()
    {
      // Production data of the Kezan Enterprise buildings (generated, tools/ai/gen_ai_data.py) and the Enterprise's
      // hero list (Kul Tiras is renamed by the fallback, so it needs its own entry) -- Gallywix first. 0.39.10: keyed
      // by the real name; the old key "The Kezan Enterprise" (0.39.6) never matched after the 0.39.7 rename.
      AiApi.AddProductionData(SlAiData.Trains, SlAiData.AltarHeroes, SlAiData.Requires, SlAiData.HeroSkills);
      AiApi.RegisterHeroes(FallbackConfig.KezanName, new List<int> { 1328559182 }); // O04N Gallywix
      FallbackSystem.Setup(); // 0.39.1: fallback scenarios (FALLBACKS.md)
    }
  }
}
