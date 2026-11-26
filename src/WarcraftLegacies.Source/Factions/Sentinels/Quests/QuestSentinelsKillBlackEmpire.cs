using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.Factions.Sentinels.Quests;

/// <summary>
/// Destroy Nzoth to defeat the black empire.
/// </summary>
public sealed class QuestSentinelsKillBlackEmpire : QuestData
{
  /// <summary>
  /// Initializes a new instance of the <see cref="QuestSentinelsKillBlackEmpire"/> class.
  /// </summary>
  public QuestSentinelsKillBlackEmpire() : base("Otherwordly Invaders",
    "The Black Empire has poured out near Feathermoon and begun threatening the wilds. These invaders must be repelled.",
    @"ReplaceableTextures\CommandButtons\BTNNzothIcon.blp")
  {
    AddObjective(new ObjectiveLegendDead(AllLegends.BlackE.Nzoth));
    // [SL] (0.81.0): its own research (RZu*, WL 4.8.2's data) -- WL 5.0.4 gave this id to "Raze Orgrimmar", and the fork keeps
    // both quests: completing either one also grants the shared research the reward units require (OnComplete)
    ResearchId = 1381659952;

  }

  /// <inheritdoc/>
  protected override string RewardDescription => Loc.Format(
    "Allows {guildRanger}s to be trained from {bastion}s",
    ("{guildRanger}", GetObjectName(UNIT_N034_GUILD_RANGER_SENTINELS)),
    ("{bastion}", GetObjectName(UNIT_E00T_WATCHER_S_BASTION_SENTINELS_SIEGE)));

  /// <inheritdoc/>
  protected override void OnComplete(Faction completingFaction)
  {
    completingFaction.Player.SetTechResearched(ResearchId, 1);
    completingFaction.Player.SetTechResearched(UPGRADE_R007_QUEST_COMPLETED_OTHERWORDLY_INVADERS_SENTINELS, 1); // [SL] (0.81.0)
  }

  /// <inheritdoc/>
  protected override void OnAdd(Faction whichFaction)
  {
    whichFaction.ModObjectLimit(ResearchId, Faction.Unlimited);
    whichFaction.ModObjectLimit(UPGRADE_R007_QUEST_COMPLETED_OTHERWORDLY_INVADERS_SENTINELS, Faction.Unlimited); // [SL] (0.81.0)
  }
}
