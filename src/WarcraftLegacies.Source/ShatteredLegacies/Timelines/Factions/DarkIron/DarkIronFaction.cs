using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Factions.Ironforge.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIron
{
  // 0.86.0 (maintainer): the Dark Iron of Shadowforge -- Ironforge's fallback (DarkIronFallback). With the Bronzebeard
  // throne empty a Dark Iron expeditionary force tunnels up through the shattered foundations -- not to burn the world
  // for Ragnaros, but to claim the Great Anvil and build a ruthless, industrial Shadowforge empire over the ruins.
  // Master-smiths, siege engineers, runecrafters; cold-blooded pragmatists and war-profiteers. Ironforge's tech tree as
  // Dark Iron copies (tools/shattered-legacies/content/dark_iron.py): renamed, tinted; Dagran Thaurissan, Golem Lord
  // Argelmach and Fineous Darkvire at the altar. A VASSAL bound by the Mercenary Charter: whoever holds the Siege
  // Foundry is their master (DarkIronFallback.Charter).
  // 0.87.0 (maintainer): their troops are the 24p's Dark Iron (Anvilrage Guardsmen, Bear Riders, Chieftains, Magma
  // Tanks, Shadowcasters, Flame Keepers, Gyrocopters -- import_24p.py DARK_IRON); the seat is the Throne Room.
  public sealed class DarkIronFaction : Faction
  {
    internal const string FactionName = "Dark Iron";

    public DarkIronFaction()
      : base(FactionName, playercolor.Coal, @"ReplaceableTextures\CommandButtons\BTNHeroMountainKing.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_H0AL_CONTROL_POINT_DEFENDER_IRONFORGE;
      Nicknames = new List<string> { "darkiron", "dark iron", "shadowforge", "dagran", "thaurissan" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe Bronzebeard throne is empty. Your only goal: seize the Great Anvil and "
        + "turn the mountain into a black-market arsenal that out-produces every empire on the map. You care for gold, "
        + "rare ore and survival. Whoever holds your Throne Room is your master and gets a Gatebreaker every five "
        + "turns; whoever holds your Siege Foundry holds your Mercenary Charter: he pays tribute for your mortar teams.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Dark Iron of Shadowforge")}|r"));
      ProcessObjectInfo(DarkIronObjectInfo.GetAllObjectLimits());
      ProcessObjectInfo(DarkIronForcesObjectInfo.GetAllObjectLimits()); // 0.87.0: the 24p's Dark Iron units
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      StartingQuest = AddQuest(new QuestGreatAnvil());
      var artifacts = new[]
      {
        ITEM_I01A_DEMON_SOUL, ITEM_I00F_GLOVES_OF_AHN_QIRAJ, ITEM_I00Z_THUNDERFURY, ITEM_I01T_FANDRAL_S_FLAMESCYTHE
      };
      AddQuest(new QuestExpedition(artifacts[GetRandomInt(0, artifacts.Length - 1)])); // WL's artifact hunt
    }
  }

  // seize the Great Anvil -> the arsenal: a War Golem and a Mortar Team every turn, the forges' best work
  internal sealed class QuestGreatAnvil : QuestData
  {
    internal QuestGreatAnvil() : base("The Great Anvil",
      "The most powerful forge in Azeroth stands cold under Ironforge. Take it, and Shadowforge's smiths will turn the "
      + "mountain into an arsenal that out-produces every empire on the map.",
      @"ReplaceableTextures\CommandButtons\BTNSteelMelee.blp")
    {
      AddObjective(new ObjectiveControlCapital(AllLegends.Ironforge.GreatForge, false));
      AddObjective(new ObjectiveControlPoint(UNIT_N015_IRONFORGE));
    }

    protected override string RewardDescription =>
      "A War Golem and a Mortar Team every turn at the Great Forge; the next level of weapons and armour";

    protected override void OnComplete(Faction whichFaction) => DarkIronFallback.Arsenal(whichFaction.Player);
  }
}
