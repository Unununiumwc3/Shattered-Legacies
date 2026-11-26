using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Centaur
{
  // 0.85.0 (maintainer): the Centaur Khans -- the Tauren's fallback (CentaurFallback). Whoever destroyed the Tauren has
  // proven himself the Apex Predator of the plains: the centaur swear the Blood-Oath of the Hunt to him. They train
  // nothing -- warriors ride in every turn at the camp by Thunder Bluff (TurnLevy). Khan Leh'Prah of the Kolkar leads
  // them; the other four khans join with a quest each (their camp's region), each raises the levy; all five ->
  // "The Five Khans". WL's centaur tents are the camps (visible and indestructible until they join).
  public sealed class CentaurFaction : Faction
  {
    internal const string FactionName = "Centaur";

    public CentaurFaction()
      : base(FactionName, playercolor.Brown, @"ReplaceableTextures\CommandButtons\BTNCentaurKhan.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0B6_CONTROL_POINT_DEFENDER_FROSTWOLF; // the Tauren's towers, taken
      Nicknames = new List<string> { "centaur", "centaurs", "khans", "kolkar" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nThe Tauren are broken, and the one who broke them is the Apex Predator of the "
        + "plains -- the centaur ride with him by the Blood-Oath of the Hunt, and with whoever holds Thunder Bluff "
        + "after him. You train nothing: warriors ride in every turn at your camp. Win the other four khans to raise "
        + "the horde.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Centaur Khans")}|r"));
      ProcessObjectInfo(CentaurObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      var galak = AddQuest(Khan(0));
      var maraudine = AddQuest(Khan(1));
      var magram = AddQuest(Khan(2));
      var gelkis = AddQuest(Khan(3));
      StartingQuest = galak;
      AddQuest(new QuestFiveKhans(galak, maraudine, magram, gelkis));
    }

    private static QuestKhan Khan(int i) => new(CentaurFallback.Clans[i]);
  }

  // one khan: hold the CP of his clan's land -> he joins with his camp, the levy grows
  internal sealed class QuestKhan : QuestData
  {
    private readonly CentaurFallback.Clan _clan;

    internal QuestKhan(CentaurFallback.Clan clan)
      : base(clan.Title, clan.Text, @"ReplaceableTextures\CommandButtons\BTNCentaurKhan.blp")
    {
      _clan = clan;
      AddObjective(new ObjectiveControlPoint(clan.Cp));
    }

    protected override string RewardDescription => _clan.Reward;

    protected override void OnComplete(Faction whichFaction) => CentaurFallback.JoinClan(whichFaction.Player, _clan);
  }

  internal sealed class QuestFiveKhans : QuestData
  {
    internal QuestFiveKhans(params QuestData[] khans) : base("The Five Khans",
      "Kolkar, Galak, Maraudine, Magram and Gelkis -- the children of Zaetar ride as one for the first time since they "
      + "slew their father. No army on the plains can outrun them.", @"ReplaceableTextures\CommandButtons\BTNCentaurKhan.blp")
    {
      foreach (var khan in khans)
      {
        AddObjective(new ObjectiveQuestComplete(khan));
      }
    }

    protected override string RewardDescription => "3 more warriors every turn (an Outrunner, an Impaler and a "
                                                   + "Sorcerer) and 300 gold";

    protected override void OnComplete(Faction whichFaction)
    {
      CentaurFallback.Levy?.Add(CentaurObjects.Outrunner, CentaurObjects.Impaler, CentaurObjects.Sorcerer);
      var p = whichFaction.Player;
      if (p != null)
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + 300);
      }
    }
  }
}
