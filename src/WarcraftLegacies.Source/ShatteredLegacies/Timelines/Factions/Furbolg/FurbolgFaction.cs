using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Furbolg
{
  // 0.86.0 (maintainer): the Furbolgs -- the Druids' fallback (FurbolgFallback). The furbolg tribes follow whoever
  // proves worthy of holding their Sacred Ancestral Totem: whoever holds Nordrassil. One hero (Nafien, the
  // Totem-Keeper), the Totem Lodge of Ursoc by Nordrassil (HQ and altar), WL's creep furbolgs every turn -- more and
  // stronger for every world tree their side holds.
  public sealed class FurbolgFaction : Faction
  {
    internal const string FactionName = "Furbolgs";

    public FurbolgFaction()
      : base(FactionName, playercolor.Wheat, @"ReplaceableTextures\CommandButtons\BTNFurbolgElder.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_E01Y_CONTROL_POINT_DEFENDER_DRUIDS; // the druids' guardians, kept
      Nicknames = new List<string> { "furbolg", "furbolgs", "timbermaw", "ursoc" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe druids are gone and the tribes follow whoever proves worthy of holding "
        + "their Sacred Ancestral Totem -- whoever holds Nordrassil. You train nothing but your Totem-Keeper: warriors "
        + "come every turn to the Totem Lodge, more and stronger for every world tree your side holds.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Furbolg tribes")}|r"));
      ProcessObjectInfo(FurbolgObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered() => SharedFactionConfigSetup.AddSharedFactionConfig(this);
  }
}
