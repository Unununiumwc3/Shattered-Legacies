using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.EbonBlade
{
  // 0.80.0 (maintainer): the Knights of the Ebon Blade -- the Scourge's fallback, a HERO-ONLY faction: four death
  // knights (Thassarian, Koltira Deathweaver, Nazgrim, Thoras Trollbane; tools/shattered-legacies/content/ebon_blade.py)
  // and Acherus: The Ebon Hold near Icecrown (revives them; indestructible). A vassal of whoever holds Icecrown Citadel's
  // CP (Core/Conquest.cs), never defeated. No quests yet (maintainer).
  public sealed class EbonBladeFaction : Faction
  {
    internal const string FactionName = "Ebon Blade";

    public EbonBladeFaction()
      : base(FactionName, playercolor.Navy, @"ReplaceableTextures\CommandButtons\BTNHeroDeathKnight.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_U028_CONTROL_POINT_DEFENDER_SCOURGE;
      Nicknames = new List<string> { "ebonblade", "ebon", "eb", "kotebb", "deathknights", "dk" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe Lich King is dead and the Frozen Throne broken -- his death knights are "
        + "free. Four champions, no army: Acherus revives them and cannot fall. You serve whoever holds Icecrown "
        + "Citadel.", ("{faction}", $"{PrefixCol}{Loc.Get("the Knights of the Ebon Blade")}|r"));
      ProcessObjectInfo(EbonBladeObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
    }
  }
}
