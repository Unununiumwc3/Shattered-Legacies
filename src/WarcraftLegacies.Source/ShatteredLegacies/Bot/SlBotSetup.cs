using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;

namespace WarcraftLegacies.Source.ShatteredLegacies.Bot
{
  // Everything Shattered Legacies hands to the bot -- only through AiApi (was part of ShatteredLegaciesSetup).
  internal static class SlBotSetup
  {
    internal static void Setup()
    {
      AiApi.SetMapTitle("Shattered Legacies: Botified Realm"); // -version (0.52; was a fixed text in the bot)
      // Production data of the fork's buildings (generated, tools/shattered-legacies/ai/gen_ai_data.py) and the Kezan
      // Enterprise's heroes -- Gallywix first. 0.58: the Enterprise is a faction of its own with the 24p goblin tree
      // (KezanEnterpriseObjects); Katherine comes along by the fallback.
      AiApi.AddProductionData(SlAiData.Trains, SlAiData.AltarHeroes, SlAiData.Requires, SlAiData.HeroSkills);
      AiApi.RegisterHeroes(KezanEnterpriseConfig.Name, new List<int>
        { KezanEnterpriseObjects.Gallywix, KezanEnterpriseObjects.Gazlowe, KezanEnterpriseObjects.Noggenfogger });
      // 0.63: the Ashtongue -- Akama, and Xi'ri once Karabor is restored (Ishanah is placed by code, never trained)
      AiApi.RegisterHeroes(AshtongueConfig.Name, new List<int> { UNIT_NAKA_ELDER_SAGE_ILLIDARI, AshtongueObjects.Xiri });
      AiApi.RegisterHeroes(AshtongueConfig.FinalName, new List<int> { UNIT_NAKA_ELDER_SAGE_ILLIDARI, AshtongueObjects.Xiri });
      // 0.57.1: the free slots (computer "NPC" in the lobby) get no bot until a split hands them a faction
      for (var i = 0; i < SplitFaction.FreeSlotOrder.Length; i++)
      {
        AiApi.ReserveSlot(SplitFaction.FreeSlotOrder[i]);
      }

      // 0.54 / 0.55: the Forsaken (Quel'thalas' betrayal) -- Sylvanas first, then the 24p tree's apothecaries
      AiApi.RegisterHeroes(ForsakenConfig.Name, new List<int>
        { ForsakenConfig.SylvanasBanshee, ForsakenConfig.SylvanasDarkRanger, ForsakenConfig.SylvanasQueen,
          ForsakenObjects.MasterApothecary, ForsakenObjects.GrandApothecary });
      // 0.53: no ExcludeFromBotPicks("Sunfury") any more -- Sunfury left the Illidari pick ([SL] in
      // Setup/FactionChoiceDialogSetup.cs) and became Quel'thalas' fallback.
    }
  }
}
