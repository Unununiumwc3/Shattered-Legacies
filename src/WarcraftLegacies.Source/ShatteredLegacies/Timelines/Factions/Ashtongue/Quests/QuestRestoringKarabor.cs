using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue.Quests
{
  // 0.63, the Ashtongue's final goal (maintainer): Outland cleansed of fel energy. 0.64: the fifth purification step
  // is the temple itself (AshtonguePurification replaces it by the Temple of Karabor -- WL's Black Temple is never
  // renamed); then the faction is called "Karabor" (Faction.Name: WL re-keys its tables; the fork's lookups by name
  // know both names), Xi'ri can be trained, Ishanah is free.
  public sealed class QuestRestoringKarabor : QuestData
  {
    public QuestRestoringKarabor(QuestData[] sites) : base("Restoring Karabor to the Light",
      "Before Illidan, before the orcs, the Black Temple was Karabor, the draenei's holiest place. Cleanse Outland of "
      + "the fel and it will be again.",
      @"ReplaceableTextures\CommandButtons\BTNDranaiAkama.blp")
    {
      for (var i = 0; i < sites.Length; i++)
      {
        AddObjective(new ObjectiveQuestComplete(sites[i]));
      }

      var temple = new ObjectiveCounter("Purify the temple (Shattrath's pool)", AshtongueConfig.StepCost[4],
        AshtongueConfig.TempleX, AshtongueConfig.TempleY);
      AddObjective(temple);
      AshtonguePurification.Counters.Add(temple);
      ResearchId = AshtongueConfig.StepTech[4];
    }

    protected override string RewardDescription =>
      "The temple becomes the Temple of Karabor and trains Xi'ri the naaru; Ishanah is free; the Ashtongue become "
      + "Karabor";

    protected override void OnComplete(Faction whichFaction)
    {
      whichFaction.Name = AshtongueConfig.FinalName;
      if (whichFaction.Player != null)
      {
        SwapFaction.NameSlot(whichFaction.Player, whichFaction);
      }

      for (var j = 0; j < 24; j++)
      {
        DisplayTimedTextToPlayer(Player(j), 0, 0, 20, "|cffffcc00Karabor is restored to the Light.|r The fel is "
                                                      + "burned out of Outland; the Ashtongue are Karabor's again.");
      }

      AiApi.LogPin("Ashtongue: Karabor restored -> " + AshtongueConfig.FinalName);
    }
  }
}
