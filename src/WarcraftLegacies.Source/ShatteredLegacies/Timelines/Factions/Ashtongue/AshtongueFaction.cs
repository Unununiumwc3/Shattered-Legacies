using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // 0.63: the Ashtongue Deathsworn -- the Illidari's fallback (IllidariFallback), a faction of its own (template:
  // KezanEnterpriseFaction). Akama's redemption: sworn to whoever holds the Black Temple's CP (Conquest), they purify
  // Outland (AshtonguePurification) and restore Karabor. Units: the 24p draenei as their own copies (import_24p.py,
  // set ASHTONGUE) and the Illidari's draenei (Akama, Deathsworn, Seers, the Draenei Hut).
  public sealed class AshtongueFaction : Faction
  {
    public AshtongueFaction()
      : base(AshtongueConfig.Name, playercolor.Violet, @"ReplaceableTextures\CommandButtons\BTNDranaiAkama.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0BB_CONTROL_POINT_DEFENDER_ILLIDARI_TOWER; // the Illidari's
      Nicknames = new List<string> { "ashtongue", "akama", "ash", "karabor" };
      ProcessObjectInfo(AshtongueObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      // 0.64: the order (maintainer) -- Shattrath, Halaa, Telredor, Oshu'gun; the temple itself is the final quest
      var sites = new QuestData[]
      {
        new QuestPurifySite(0, "Shattrath trains Luminarchs and Vindicators and researches weapons, armour and the "
                               + "Luminarchs' training"),
        new QuestPurifySite(1, "Halaa trains Elekk Knights"),
        new QuestPurifySite(2, "Telredor trains Rangari"),
        new QuestPurifySite(3, "Oshu'gun trains Elementalists and researches their training"),
      };
      for (var i = 0; i < sites.Length; i++)
      {
        AddQuest(sites[i]);
      }

      StartingQuest = sites[0];
      AddQuest(new QuestRestoringKarabor(sites));
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
    }
  }
}
