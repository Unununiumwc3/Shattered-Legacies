using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;

namespace WarcraftLegacies.Source.ShatteredLegacies.Bot
{
  // Everything Shattered Legacies hands to the bot -- only through AiApi (was part of ShatteredLegaciesSetup).
  internal static class SlBotSetup
  {
    internal static void Setup()
    {
      AiApi.SetMapTitle("Shattered Legacies: Botified Realm"); // -version (0.52; was a fixed text in the bot)
      // Production data of the Kezan Enterprise buildings (generated, tools/shattered-legacies/ai/gen_ai_data.py) and
      // the Enterprise's hero list (Kul Tiras is renamed by its fallback, so it needs its own entry) -- Gallywix first.
      // 0.39.10: keyed by the real name; the old key "The Kezan Enterprise" (0.39.6) never matched after 0.39.7.
      AiApi.AddProductionData(SlAiData.Trains, SlAiData.AltarHeroes, SlAiData.Requires, SlAiData.HeroSkills);
      AiApi.RegisterHeroes(KezanEnterpriseConfig.Name, new List<int> { KezanEnterpriseConfig.GallywixType });
      // Sunfury is planned as Quel'thalas' fallback, not as the Illidari slot's pick (Timelines/SCENARIO_CONCEPTS.md):
      // the bot keeps the Illidari (was a fixed rule in the bot's AiChoices until 0.51).
      AiApi.ExcludeFromBotPicks("Sunfury");
    }
  }
}
