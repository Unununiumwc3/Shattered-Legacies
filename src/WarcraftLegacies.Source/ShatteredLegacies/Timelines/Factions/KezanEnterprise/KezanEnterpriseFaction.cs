using System.Collections.Generic;
using MacroTools.Factions;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.58: the Kezan Enterprise as a faction of its own (template: ForsakenFaction / WL's ScarletCrusadeFaction) -- was
  // Kul Tiras renamed with limits set by hand (0.39.6). Arrives by Kul Tiras' fallback (KultirasFallback). Its tree is
  // the 24p map's goblins (import_24p.py, object limits in WarcraftLegacies.Shared/ShatteredLegacies/
  // FactionObjectLimits/), its quests the 24p goblin quests rebuilt with WL objectives. Own team (Unally).
  public sealed class KezanEnterpriseFaction : Faction
  {
    public KezanEnterpriseFaction()
      : base(KezanEnterpriseConfig.Name, playercolor.Peanut, @"ReplaceableTextures\CommandButtons\BTNGoblinPrince.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_H09W_CONTROL_POINT_DEFENDER_KULTIRAS; // Kul Tiras' survivors hold them
      Nicknames = new List<string> { "kezan", "goblin", "goblins", "ke" };
      ProcessObjectInfo(KezanEnterpriseObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      var engineering = new QuestExplosiveEngineering();
      AddQuest(engineering);
      StartingQuest = engineering;
      AddQuest(new QuestGadgetzan());
      // capitals of a slot whose pick may have replaced them: only while they exist (WL's objective needs the unit)
      if (AllLegends.Sentinels.Feathermoon.Unit != null && AllLegends.Sentinels.Auberdine.Unit != null)
      {
        AddQuest(new QuestWesternExpansion());
      }

      if (AllLegends.Druids.Nordrassil.Unit != null)
      {
        AddQuest(new QuestLumberMarket());
      }

      AddQuest(new QuestGoblinEmpire());
      var fortune = new QuestTradePrincesFortune(); // the end goal (on the scoreboard: KezanEnterpriseGoal)
      AddQuest(fortune);
      KezanEnterpriseGoal.Quest = fortune;
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
    }
  }
}
