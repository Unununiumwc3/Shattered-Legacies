using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Harpy
{
  // 0.85.0 (maintainer): the Harpies of Stonetalon -- the Black Empire's fallback (HarpyFallback). With N'Zoth's psychic
  // tether severed the harpies fall into Magical Thralldom: they imprint on the banner of whoever shattered their old
  // master. Only WL's harpy nests (now Harpy Nests and the Harpy Roost), a few harpy kinds and two heroes; no tech.
  public sealed class HarpyFaction : Faction
  {
    internal const string FactionName = "Harpies";

    public HarpyFaction()
      : base(FactionName, playercolor.Lavender, @"ReplaceableTextures\CommandButtons\BTNHarpyQueen.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0DV_CONTROL_POINT_DEFENDER_NZOTH_TOWER; // the Black Empire's, left over
      Nicknames = new List<string> { "harpy", "harpies", "stonetalon" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nN'Zoth's whispers are gone, and the harpies follow the banner of whoever "
        + "silenced them -- and of whoever holds Stonetalon Peak after him. Your nests train harpies, the Roost your "
        + "two matriarchs. Your nests cannot fall.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Harpies of Stonetalon")}|r"));
      ProcessObjectInfo(HarpyObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered() => SharedFactionConfigSetup.AddSharedFactionConfig(this);
  }
}
