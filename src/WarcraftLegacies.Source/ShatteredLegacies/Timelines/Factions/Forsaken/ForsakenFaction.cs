using System.Collections.Generic;
using MacroTools.Factions;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.54: the first faction of the fork's own (template: WL's ScarletCrusadeFaction). Arrives by Quel'thalas'
  // betrayal (ForsakenBetrayal). Object limits: WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/.
  // 0.55: the 24p Forsaken tech tree (import_24p.py) and two quests -- "Free Will" (Sylvanas level 6: the Dark Ranger,
  // off the Scourge's team) and "The Throne of Lordaeron" (Lordaeron City: the Undercity, Queen of the Forsaken).
  public sealed class ForsakenFaction : Faction
  {
    public ForsakenFaction()
      : base(ForsakenConfig.Name, playercolor.Lavender, @"ReplaceableTextures\CommandButtons\BTNBanshee.blp")
    {
      TraditionalTeam = TeamSetup.Legion; // the Lich King's team until they break free (D4)
      ControlPointDefenderUnitTypeId = UNIT_U028_CONTROL_POINT_DEFENDER_SCOURGE;
      Nicknames = new List<string> { "forsaken", "fs" };
      ProcessObjectInfo(ForsakenObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      var freeWill = new QuestForsakenFreeWill();
      AddQuest(freeWill);
      StartingQuest = freeWill;
      AddQuest(new QuestForsakenThrone(freeWill));
      AddQuest(new QuestPlagueResearch());
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
    }
  }
}
