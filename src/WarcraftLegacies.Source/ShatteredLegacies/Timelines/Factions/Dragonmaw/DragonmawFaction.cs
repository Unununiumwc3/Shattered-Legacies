using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw
{
  // 0.80.0 (maintainer): the Dragonmaw Clan -- the orcs of the 24p's Twilight tree without the cult (import_24p.py, set
  // DRAGONMAW): grunts, spearthrowers, skull catapults, red dragons and their riders; heroes Nek'rosh Skullcrusher and
  // Zuluhed the Whacked. A VASSAL (Core/Conquest.cs): sworn to whoever holds Dragonmaw Port, never defeated, its town
  // halls indestructible (DragonmawFallback). It arrives either as the Orcish Horde's / Fel Horde's fallback (not
  // beaten by an Old God) or as Twilight's successor (Grim Batol lost while Cho'gall is dead).
  public sealed class DragonmawFaction : Faction
  {
    internal const string FactionName = "Dragonmaw";

    public DragonmawFaction()
      : base(FactionName, playercolor.Maroon, @"ReplaceableTextures\CommandButtons\BTNRedDragon.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0AA_CONTROL_POINT_DEFENDER_FEL; // the Fel Horde's orc tower
      Nicknames = new List<string> { "dragonmaw", "dm", "nekros", "zuluhed" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe red dragons of Grim Batol are yours to command again. The clan serves "
        + "whoever holds Dragonmaw Port -- lose the port and you fight for its new master. Your halls cannot fall.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Dragonmaw Clan")}|r"));
      ProcessObjectInfo(DragonmawObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      AddQuest(new QuestAlexstraszasChains()); // 0.88.0: hold Grim Batol -> a Red Drake every turn, until Nek'rosh falls
    }
  }
}
