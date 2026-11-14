using System.Collections.Generic;
using System.Linq;
using MacroTools.Dialogues;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;
using MacroTools.Localization;
using MacroTools.PreplacedWidgets;
using MacroTools.Quests;
using MacroTools.Researches;
using MacroTools.Utils;
using WarcraftLegacies.Shared.FactionObjectLimits;
using WarcraftLegacies.Source.Factions.OrcishHorde.Mechanics;
using WarcraftLegacies.Source.Factions.OrcishHorde.Quests;
using WarcraftLegacies.Source.Factions.TaurenTribes;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.Shared.Researches;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.Factions.OrcishHorde;

public sealed class OrcishHordeFaction : Faction
{
  private QuestData _bloodPact;

  /// <inheritdoc />
  public OrcishHordeFaction() : base("Orcish Horde", playercolor.Red, @"ReplaceableTextures\CommandButtons\BTNThrall.blp")
  {
    TraditionalTeam = TeamSetup.Horde;
    ControlPointDefenderUnitTypeId = UNIT_N0B6_CONTROL_POINT_DEFENDER_FROSTWOLF;
    StartingGold = new StartingGold
    {
      Instant = 200,
      Income = 200,
      Turns = 10
    };
    CinematicMusic = "SadMystery";
    IntroText = () => Loc.Format(
      "You are playing as the battle-hardened {faction}.\n\nYou begin in Durotar. Build your army and research Build Orgrimmar at your main building to claim the city and unlock Grom Hellscream.\n\nOrgrimmar will be the new home of the Horde.\n\nThe Night Elves will not welcome you. Stand with your Tauren allies, drive into Ashenvale, and burn the World Tree if you must.",
      ("{faction}", $"{PrefixCol}{Loc.Get("Orcish Horde")}|r"));
    Nicknames = new List<string>
    {
      "horde",
      "oh",
      "orc",
      "orcs"
    };
    ProcessObjectInfo(OrcishHordeObjectInfo.GetAllObjectLimits());
    RegisterFactionDependentInitializer<TaurenTribesFaction>(RegisterTaurenDialogue);
  }

  /// <inheritdoc />
  public override void OnRegistered()
  {
    RegisterResearches();
    OrcishHordeSpells.Setup();
    OrcishHordeTraits.Setup();
    SharedFactionConfigSetup.AddSharedFactionConfig(this);
    RegisterQuests();
    RegisterDialogue();
  }

  /// <inheritdoc />
  public override void OnNotPicked()
  {
    Regions.DurotarUnlock.CleanupNeutralPassiveUnits();
    base.OnNotPicked();
  }

  private void RegisterDialogue()
  {
    TriggeredDialogueManager.Add(
      new TriggeredDialogue(new DialogueSequence(new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc01\O01Grunt01",
            "Warchief, our ship sustained heavy damage when we passed through the raging maelstrom. It's unsalvageable.",
            "Grunt"),
          new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc01\O01Thrall02",
            "I knew it. Can we confirm our location? Is this Kalimdor?",
            "Thrall"),
          new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc01\O01Grunt03",
            "We traveled due west, as you instructed. This should be it.",
            "Grunt")),
        new[] { this },
        new[]
        {
          new ObjectiveControlLegend(AllLegends.Orc.Thrall, false)
          {
            EligibleFactions = new List<Faction> { this }
          }
        }));

    TriggeredDialogueManager.Add(
      new TriggeredDialogue(new DialogueSequence(new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc08\O08Grom33",
            "Thrall... I see clearly now.  I'm... sorry.  I am so sorry..",
            "Grom Hellscream")),
        new[] { this },
        new[]
        {
          new ObjectiveQuestComplete(_bloodPact)
        }));
  }

  private void RegisterTaurenDialogue(TaurenTribesFaction tauren)
  {
    TriggeredDialogueManager.Add(
      new TriggeredDialogue(new DialogueSequence(new Dialogue(
            @"Sound\Dialogue\OrcExpCamp\OrcQuest00x\D00Thrall25",
            "Who are you, warrior?",
            "Thrall"),
          new Dialogue(
            @"Sound\Dialogue\OrcExpCamp\OrcQuest00x\D00Rexxar26",
            "I am Rexxar, last son of the Mok'Nathal.",
            "Rexxar")),
        new Faction[] { this, tauren },
        new[]
        {
          new ObjectiveLegendMeetsLegend(AllLegends.Orc.Thrall, AllLegends.Tauren.Rexxar)
        }));

    TriggeredDialogueManager.Add(
      new TriggeredDialogue(new DialogueSequence(new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc01\O01Cairne23",
            "I am Cairne, chief of the Bloodhoof tauren. You greenskins fight with both savagery and valor. I am intrigued.",
            "Cairne Bloodhoof"),
          new Dialogue(
            @"Sound\Dialogue\OrcCampaign\Orc01\O01Thrall24",
            "I am Thrall, and these are my brethren, the orcs. We've come seeking the destiny promised to us.",
            "Thrall")),
        new Faction[] { this, tauren },
        new[]
        {
          new ObjectiveLegendMeetsLegend(AllLegends.Tauren.CairneBloodhoof, AllLegends.Orc.Thrall)
        }));
  }

  private void RegisterQuests()
  {
    Player.RescueGroup(Regions.DurotarUnlock.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures));
    var questOrgrimmar = AddQuest(new QuestOrgrimmar(Regions.Orgrimmar));
    StartingQuest = questOrgrimmar;

    var questCrossroads = new QuestCrossroads(Regions.Crossroads);
    questCrossroads.AddObjective(new ObjectiveQuestResolved(questOrgrimmar)
    {
      Progress = QuestProgress.Undiscovered,
      ShowsInQuestLog = false,
      ShowsInPopups = false
    });
    AddQuest(questCrossroads);

    var questSenjinIsles = new QuestSenjinIsles(AllLegends.Orc.Voljin);
    questSenjinIsles.AddObjective(new ObjectiveQuestResolved(questOrgrimmar)
    {
      Progress = QuestProgress.Undiscovered,
      ShowsInQuestLog = false,
      ShowsInPopups = false
    });
    AddQuest(questSenjinIsles);

    _bloodPact = AddQuest(new QuestBreakBloodPact(questOrgrimmar));

    var questSlayCenarius = AddQuest(new QuestSlayCenarius(questOrgrimmar));
    var questDemolishElfCities = AddQuest(new QuestDemolishElfCities(questSlayCenarius));
    var questThrallMaelstrom = AddQuest(new QuestThrallMaelstrom(AllLegends.Orc.Thrall, questOrgrimmar));
    var questWarsongHold = AddQuest(new QuestWarsongHold(questDemolishElfCities));
    AddQuest(new QuestFreeNerzhul(AllLegends.Scourge.TheFrozenThrone, AllLegends.Orc.Thrall, questWarsongHold, questDemolishElfCities));

  }

  private void RegisterResearches()
  {
    ResearchManager.RegisterIncompatibleSet(
      new CustomResearch(UPGRADE_RZ02_BLADEMASTERS_ORCISH_HORDE, 0)
      {
        ResearchFunc = researchingPlayer =>
        {
          var faction = researchingPlayer.GetPlayerData().Faction;
          faction?.ModObjectLimit(UNIT_O00G_BLADEMASTER_ORCISH_HORDE, 6);
        }
      },
      new CustomResearch(UPGRADE_RZ03_KOR_KRON_ELITES_ORCISH_HORDE, 0)
      {
        ResearchFunc = researchingPlayer =>
        {
          var faction = researchingPlayer.GetPlayerData().Faction;
          faction?.ModObjectLimit(UNIT_N03F_KOR_KRON_ELITE_ORCISH_HORDE_ELITE, 6);
        }
      });
  }
}
