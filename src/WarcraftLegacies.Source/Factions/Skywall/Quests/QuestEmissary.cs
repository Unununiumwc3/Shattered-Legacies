using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.FactionBased;

namespace WarcraftLegacies.Source.Factions.Skywall.Quests;

/// <summary>
/// Kill some creeps to gain an outpost and 1 forgotten one.
/// </summary>
public sealed class QuestEmissary : QuestData
{

  /// <summary>
  /// Initializes a new instance of the <see cref="QuestEmissary"/> class.
  /// </summary>
  public QuestEmissary() : base("Emissary of the Firelands",
    "Take control of Un'Goro Crater to make contact with Ragnaros and the Firelands.",
    @"ReplaceableTextures\CommandButtons\BTNHeroAvatarOfFlame.blp")
  {
    AddObjective(new ObjectiveControlPoint(UNIT_N025_UN_GORO_CRATER));
    AddObjective(new ObjectiveSelfExists());
    ResearchId = UPGRADE_RSW2_QUEST_COMPLETED_EMISSARY_OF_THE_FIRELANDS;

  }

  /// <inheritdoc />
  protected override string RewardDescription => Loc.Format(
    "Learn to train Ragnaros from the {altar} and learn to build the Magma Complex",
    ("{altar}", GetObjectName(UNIT_N078_ALTAR_OF_ELEMENTS_SKYWALL_ALTAR)));

}
