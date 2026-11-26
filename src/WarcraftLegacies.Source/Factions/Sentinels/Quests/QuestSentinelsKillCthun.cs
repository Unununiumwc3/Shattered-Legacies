using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.Factions.Sentinels.Quests;

/// <summary>
/// Destroy Thunderbluff to unlock a hero and a demi hero.
/// </summary>
public sealed class QuestSentinelsKillCthun : QuestData
{

  /// <summary>
  /// Initializes a new instance of the <see cref="QuestSentinelsKillCthun"/> class.
  /// </summary>
  public QuestSentinelsKillCthun() : base("Gates of Ahn'Qiraj",
    "The Qiraji have bursted out of their underground city. They need to be ridden from the surface of Kalimdor.",
    @"ReplaceableTextures\CommandButtons\BTNCthunIcon.blp")
  {
    AddObjective(new ObjectiveLegendDead(AllLegends.Ahnqiraj.Cthun));
    // [SL] (0.81.0): its own research (RZu*, WL 4.8.2's data) -- WL 5.0.4 gave this id to "Break the Bluff", and the fork keeps
    // both quests: completing either one also grants the shared research the reward units require (OnComplete)
    ResearchId = 1381659953;

  }

  /// <inheritdoc/>
  protected override string RewardDescription => Loc.Format(
    "Learn to train {moonRider}s from the {roost}s and research {upgrade}'s second level from the {academy}",
    ("{moonRider}", GetObjectName(UNIT_E022_MOON_RIDER_SENTINELS)),
    ("{roost}", GetObjectName(UNIT_EDOS_ROOST_SENTINELS_SPECIALIST)),
    ("{upgrade}", GetObjectName(UPGRADE_REMG_UPGRADE_MOON_GLAIVE_LIGHT_BLUE_RESEARCH)),
    ("{academy}", GetObjectName(UNIT_E00L_WAR_ACADEMY_SENTINELS_BARRACKS)));

  /// <inheritdoc/>
  protected override void OnComplete(Faction completingFaction)
  {
    completingFaction.Player.SetTechResearched(ResearchId, 1);
    completingFaction.Player.SetTechResearched(UPGRADE_R052_QUEST_COMPLETED_GATES_OF_AHN_QIRAJ_SENTINELS, 1); // [SL] (0.81.0)

  }
  /// <inheritdoc/>
  protected override void OnAdd(Faction whichFaction)
  {
    whichFaction.ModObjectLimit(ResearchId, Faction.Unlimited);
    whichFaction.ModObjectLimit(UPGRADE_R052_QUEST_COMPLETED_GATES_OF_AHN_QIRAJ_SENTINELS, Faction.Unlimited); // [SL] (0.81.0)
  }
}
