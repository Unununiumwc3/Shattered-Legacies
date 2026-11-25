using MacroTools.Artifacts;
using MacroTools.ControlPoints;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.UnitBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

// 0.72.0: the reward was Thunderfury -- also one of the four artifacts Ironforge's "Secrets of Uldum" can dig up
// (WL). Ironforge keeps it; the Twilight's Hammer gets its own artifact, the Hammer of Twilight (IZh1, fork item,
// hand-made: ../README.md "Changes to map data"). Same objectives (playtested in 0.68); class name kept so the faction
// setup and the bot need no change.
internal sealed class QuestWindseeker : QuestData
{
  internal const int HammerOfTwilight = 1230661681; // IZh1

  private readonly ObjectiveAnyUnitInRect _expedition = new(Regions.BrokenIslesA, "the Broken Isles", true, TwilightHammerObjects.Chogall);

  internal QuestWindseeker() : base("The Hammer of Twilight",
    "Cho'gall has seen the relic his cult is named for, buried beneath the ruins of Suramar. Take Cho'gall to the Broken Isles and secure Suramar.",
    @"ReplaceableTextures\CommandButtons\BTNStormHammer.blp")
  {
    AddObjective(_expedition);
    AddObjective(new ObjectiveControlLevel(UNIT_N032_SURAMAR, 1));
  }

  protected override string RewardDescription => "Discover the Hammer of Twilight at Suramar";

  protected override void OnComplete(Faction faction)
  {
    if (faction.Player == null)
    {
      return;
    }
    var existing = ArtifactManager.GetFromTypeId(HammerOfTwilight);
    if (existing != null)
    {
      existing.Ping(faction.Player);
      return;
    }
    var suramar = ControlPointManager.Instance.GetFromUnitType(UNIT_N032_SURAMAR).Unit;
    var hammer = item.Create(HammerOfTwilight, suramar.X, suramar.Y);
    ArtifactManager.Register(new Artifact(hammer));

  }
}
