using System.Collections.Generic;
using System.Linq;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.PreplacedWidgets;
using MacroTools.Utils;
using WarcraftLegacies.Shared.FactionObjectLimits;
using WarcraftLegacies.Source.Factions.Draenei.Quests;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.Shared.Quests;

namespace WarcraftLegacies.Source.Factions.Draenei;

public sealed class DraeneiFaction : Faction
{
  // [SL] (0.80.0, maintainer): the Draenei are no pick of the Sentinels' slot any more but the LEGION's fallback
  // (ShatteredLegacies/Timelines/Factions/Draenei/DraeneiFallback.cs) -- in that mode without Darkshore ("Rebuild
  // Civilisation", the Sentinels' land) and with "A New Home" completed by the fallback (RepairHull).
  private readonly bool _asFallback;

  internal QuestRepairExodarHull? RepairHull { get; private set; }

  /// <inheritdoc />
  public DraeneiFaction(bool asFallback = false)
    : base("The Exodar", playercolor.Mint, @"ReplaceableTextures\CommandButtons\BTNBOSSVelen.blp")
  {
    _asFallback = asFallback;
    TraditionalTeam = TeamSetup.NightElves;
    StartingGold = new StartingGold
    {
      Instant = 200,
      Income = 150,
      Turns = 10
    };
    ControlPointDefenderUnitTypeId = UNIT_U008_CONTROL_POINT_DEFENDER_DRAENEI;
    IntroText = () => Loc.Format(
      "You are playing as the exiled {faction}.\n\nYou begin on Azuremyst Island, amid the wreckage of your flight from the Burning Legion.\n\nFurther inland your Night-elf allies will need your help against the Old Gods. Quickly build your base and gain entry to the Exodar.\n\nPower up your buildings with your Arcane Wells to unlock powerful global abilities.",
      ("{faction}", $"{PrefixCol}{Loc.Get("Draenei")}|r"));

    Nicknames = new List<string>
    {
      "draenei",
      "dranei",
      "exo",
      "exodar",
      "theexodar",
      "goats"
    };
    ProcessObjectInfo(DraeneiObjectInfo.GetAllObjectLimits());
  }

  /// <inheritdoc />
  public override void OnRegistered()
  {
    RegisterQuests();
    DraeneiSpells.Setup();
    DraeneiTraits.Setup();
    SharedFactionConfigSetup.AddSharedFactionConfig(this);
  }
  /// <inheritdoc />
  public override void OnNotPicked()
  {
    Regions.ExodarBaseUnlock.CleanupNeutralPassiveUnits();
    Regions.Darkshore.CleanupNeutralPassiveUnits();
    base.OnNotPicked();
  }

  private void RegisterQuests()
  {
    var questRepairHull = new QuestRepairExodarHull(Regions.ExodarBaseUnlock, AllLegends.Draenei.LegendExodar);
    StartingQuest = questRepairHull;
    AddQuest(questRepairHull);
    RepairHull = questRepairHull; // [SL] (0.80.0)
    if (!_asFallback) // [SL] (0.80.0): Darkshore is the Sentinels'
    {
      AddQuest(new QuestRebuildCivilisation(Regions.Darkshore));
    }
    AddQuest(new QuestShipArgus(
      AllPreplacedWidgets.Units.GetClosest(UNIT_H03V_ENTRANCE_PORTAL, Regions.OutlandToArgus.Center),
      AllPreplacedWidgets.Units.GetClosest(UNIT_H03V_ENTRANCE_PORTAL, Regions.TempestKeepSpawn.Center),
      AllLegends.Draenei.Velen
    ));
    var crystalProtectors = GlobalGroup
      .EnumUnitsInRect(Regions.ExodarBaseUnlock.Rect)
      .Where(x => x.UnitType == UNIT_U00U_CRYSTAL_PROTECTOR_DRAENEI_TOWER);
    var questRepairGenerator = new QuestRepairGenerator(AllLegends.Draenei.LegendExodarGenerator, questRepairHull, crystalProtectors);
    AddQuest(questRepairGenerator);
    AddQuest(new QuestTriumvirate(AllLegends.Draenei.Velen));
    var questDimensionalShip = new QuestDimensionalShip(Regions.ExodarBaseUnlock, questRepairGenerator, AllLegends.Draenei.LegendExodarGenerator);
    AddQuest(questDimensionalShip);
    AddQuest(new QuestExtractSunwellVial(AllLegends.Quel.Sunwell, Artifacts.SunwellVial));
  }
}
