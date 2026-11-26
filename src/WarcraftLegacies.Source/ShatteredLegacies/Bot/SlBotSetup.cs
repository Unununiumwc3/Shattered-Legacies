using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Zandalari;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.EbonBlade;

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
      AiApi.AddBuildData(SlBuildData.Menus, SlBuildData.Products, SlBuildData.Upgrades, SlBuildData.Required, SlBuildData.HalfWidth);
      AiApi.AddSpellData(SlAiSpells.Id, SlAiSpells.Kind, SlAiSpells.Who, SlAiSpells.Min, SlAiSpells.Radius, SlAiSpells.Flags,
        SlAiSpells.Order, SlAiSpells.CarrierType, SlAiSpells.CarrierRow); // 0.70.0: fork Channel spells (gen_spells.py)
      foreach (var def in TimelineCatalog.All)
      {
        if (def.Hq.Length > 0)
        {
          AiApi.RegisterHeadquarters(def.Faction, def.Hq);
        }
      }
      // 0.79.0: Twilight's HQ (the emergency teleport's anchor, AiRecall): Grim Batol once it is theirs, until then the
      // Great Hall they arrive with (maintainer: they die before they reach Grim Batol)
      AiApi.RegisterHeadquarters(TwilightHammerFaction.FactionName,
        new[] { UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT, TwilightHammerObjects.GreatHall });
      // 0.80.0: the Dragonmaw Clan's halls (indestructible; the HQ teleport's anchor) and its heroes
      AiApi.RegisterHeadquarters(DragonmawFaction.FactionName,
        new[] { DragonmawObjects.GreatHall, DragonmawObjects.Stronghold, DragonmawObjects.Fortress });
      AiApi.RegisterHeadquarters(ZandalariFaction.FactionName, new[] { ZandalariObjects.GreatHall,
        ZandalariObjects.Stronghold, ZandalariObjects.Fortress, UNIT_O00V_DAZAR_ALOR_CREEP });
      AiApi.RegisterHeroes(ZandalariFaction.FactionName, new List<int> { ZandalariObjects.Rastakhan, ZandalariObjects.Zul });
      AiApi.RegisterHeadquarters(EbonBladeFaction.FactionName, new[] { EbonBladeFallback.Acherus });
      AiApi.RegisterHeroes(EbonBladeFaction.FactionName, new List<int>(EbonBladeFallback.Heroes));
      AiApi.RegisterHeroes(DragonmawFaction.FactionName, new List<int> { DragonmawObjects.Nekrosh, DragonmawObjects.Zuluhed });
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
