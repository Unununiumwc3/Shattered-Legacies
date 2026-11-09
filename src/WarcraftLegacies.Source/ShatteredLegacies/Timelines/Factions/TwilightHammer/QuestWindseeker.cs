using MacroTools.Artifacts;
using MacroTools.ControlPoints;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestWindseeker : QuestData
{
  private readonly ObjectiveAnyUnitInRect _expedition = new(Regions.BrokenIslesA, "the Broken Isles", true, TwilightHammerObjects.Chogall);

  internal QuestWindseeker() : base("Blessed Blade of the Windseeker",
    "Cho'gall has seen Thunderfury in a vision. Take Cho'gall to the Broken Isles and secure Suramar.",
    @"ReplaceableTextures\CommandButtons\BTNThoriumMelee.blp")
  {
    AddObjective(_expedition);
    AddObjective(new ObjectiveControlLevel(UNIT_N032_SURAMAR, 1));
  }

  protected override string RewardDescription => "Discover Thunderfury at Suramar, or reveal its location if already discovered";

  protected override void OnComplete(Faction faction)
  {
    if (faction.Player == null)
    {
      return;
    }
    var existing = ArtifactManager.GetFromTypeId(ITEM_I00Z_THUNDERFURY);
    if (existing != null)
    {
      existing.Ping(faction.Player);
      return;
    }
    var suramar = ControlPointManager.Instance.GetFromUnitType(UNIT_N032_SURAMAR).Unit;
    var blade = item.Create(ITEM_I00Z_THUNDERFURY, suramar.X, suramar.Y);
    ArtifactManager.Register(new Artifact(blade));

  }
}
