using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Nazjatar
{
  // 0.84.0 (maintainer): the naga of Azshara -- the Sentinels' fallback in the Nazjatar dungeon (NazjatarFallback).
  // The 24p's Nazjatar naga (import_24p.py, set NAZJATAR: its N'Zoth tree without the Old God), separate from
  // Illidan's naga. Lady Naz'jar leads them; Queen Azshara and Atoph the Bloodcursed come with quests. A VASSAL
  // (Core/Conquest.cs): sworn to whoever holds the Eternal Palace -- at the start: whoever broke the Sentinels.
  public sealed class NazjatarFaction : Faction
  {
    internal const string FactionName = "Nazjatar";

    public NazjatarFaction()
      : base(FactionName, playercolor.Turquoise, @"ReplaceableTextures\CommandButtons\BTNNagaSummoner.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0BB_CONTROL_POINT_DEFENDER_ILLIDARI_TOWER; // a naga tower
      Nicknames = new List<string> { "nazjatar", "naga", "azshara", "nazjar" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe Sentinels are broken and Queen Azshara's naga rise from the deep. You "
        + "serve whoever holds the Eternal Palace -- lose it and you swim for its new master. Retake Azshara's old "
        + "shores to call the Empress herself. Your temples cannot fall.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Naga of Nazjatar")}|r"));
      ProcessObjectInfo(NazjatarObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      StartingQuest = AddQuest(new QuestNagaReward("The Empress Awakens",
        "Azshara's palace sank beneath the Maelstrom ten thousand years ago; her old shores still remember her. Take "
        + "the Maelstrom and the Azshara Coast, and the Empress of Nazjatar rises to lead her people herself.",
        "BTNNagaSummoner", "Queen Azshara can be trained at the Altar of the Depths, and Trench Hydras at the Depth "
        + "Void Portal", NazjatarObjects.QuestEmpress, UNIT_N028_MAELSTROM, UNIT_N01S_AZSHARA_COAST));
      AddQuest(new QuestNagaReward("The Bloodcursed",
        "Atoph's bloodcursed naga still haunt the isles of the western sea. Take Ranazjar Isle and they answer "
        + "Nazjatar's call.", "BTNRoyalGuardHeroV2", "Atoph the Bloodcursed can be trained at the Altar of the "
        + "Depths", NazjatarObjects.QuestBloodcursed, UNIT_N03O_RANAZJAR_ISLE));
      AddQuest(new QuestNagaReward("Reel Them Back",
        "The Heart of Azeroth pulses beneath the Eternal Palace, and the old outpost of Nazjatar beside the "
        + "Maelstrom guards the way down. Hold both and the deep sends its immortal guardians.",
        "BTNFacelessMadness", "Immortal Guardians can be trained at the Depth Void Portal",
        NazjatarObjects.QuestGuardians, UNIT_N04Y_NAZJATAR, DungeonObjects.HeartOfAzeroth));
    }
  }

  // hold the CPs -> a research (a hero or a unit unlocks)
  internal sealed class QuestNagaReward : QuestData
  {
    private readonly string _reward;

    internal QuestNagaReward(string title, string text, string icon, string reward, int researchId,
      params int[] cpTypes) : base(title, text, @"ReplaceableTextures\CommandButtons\" + icon + ".blp")
    {
      _reward = reward;
      foreach (var cp in cpTypes)
      {
        AddObjective(new ObjectiveControlPoint(cp));
      }

      ResearchId = researchId;
    }

    protected override string RewardDescription => _reward;
  }
}
