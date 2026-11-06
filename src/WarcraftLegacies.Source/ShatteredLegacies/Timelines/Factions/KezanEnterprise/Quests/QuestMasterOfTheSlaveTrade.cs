using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests
{
  // 0.60 (maintainer): replaces the 24p "Goblin Empire" (hold 7 goblin CPs -- hardly possible for an island faction
  // that arrives late). The goblin towns' rulers trade with Kezan instead: every kind of refugee sold at least once at
  // the Refugee Slave Camps (KezanEnterpriseSlaveCamps; the Enterprise's own purchases count too) -- 0.61: one sale at
  // every camp. Reward: the title, the Intercontinental Artillery (the Goblin Empire's reward), 25 knowledge (0.61). Lore: whoever owns the contracts of a whole
  // continent's dispossessed owns its wars -- Gallywix, Master of the Slave Trade.
  public sealed class QuestMasterOfTheSlaveTrade : QuestData
  {
    public QuestMasterOfTheSlaveTrade() : base("Master of the Slave Trade",
      "Every fallen kingdom leaves survivors, and every survivor needs passage. Sell the contracts of each people at "
      + "the Refugee Slave Camps of the goblin ports -- whoever rules the port pays, the Enterprise profits.",
      @"ReplaceableTextures\CommandButtons\BTNGoblinPrince.blp")
    {
      Sold = new ObjectiveCounter("Sell a refugee at every Refugee Slave Camp",
        KezanEnterpriseSlaveCampsData.Camps.Length); // 0.61 (maintainer): one per camp, not every kind
      AddObjective(Sold);
      KezanEnterpriseSlaveCamps.SoldCounter = Sold;
      ResearchId = KezanEnterpriseObjects.QuestMasterOfTheSlaveTrade;
    }

    internal ObjectiveCounter Sold { get; }

    protected override string RewardDescription =>
      "Gallywix becomes the Master of the Slave Trade; the Intercontinental Artillery can be built (Tier 4); +"
      + I2S(KezanEnterpriseConfig.SlaveTradeKnowledge) + " knowledge";

    protected override void OnComplete(Faction whichFaction)
    {
      KezanEnterpriseSlaveCamps.GallywixTitled = true;
      var p = whichFaction.Player;
      if (p != null)
      {
        TitleGallywix(p);
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER,
          GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) + KezanEnterpriseConfig.SlaveTradeKnowledge); // 0.61
      }

      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 15, "|cffffcc00Gallywix|r is now the Master of the Slave Trade.");
      }

      AiApi.LogPin("Kezan: Master of the Slave Trade");
    }

    // the title on every Gallywix the player has (a newly trained / revived one: KezanEnterpriseSlaveCamps)
    internal static void TitleGallywix(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == KezanEnterpriseObjects.Gallywix)
        {
          BlzSetUnitName(u, KezanEnterpriseConfig.GallywixTitle);
        }
      }

      DestroyGroup(g);
    }
  }
}
