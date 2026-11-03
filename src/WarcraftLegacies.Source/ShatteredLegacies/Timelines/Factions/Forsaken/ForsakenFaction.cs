using System.Collections.Generic;
using MacroTools.Factions;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.54: the first faction of the fork's own (template: WL's ScarletCrusadeFaction). Arrives by Quel'thalas'
  // betrayal (ForsakenBetrayal). Object limits: WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/.
  // Planned (TODO.md): the 24p Forsaken tech tree (0.55), breaking free from the Lich King and "The Throne of
  // Lordaeron" -> Undercity (0.56).
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
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
    }
  }
}
