using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Zandalari
{
  // 0.80.0 (maintainer): the Zandalari Empire -- Ahn'Qiraj's fallback at Zuldazar (ZandalariFallback). The 24p's
  // Zandalar trolls (import_24p.py, set ZANDALAR), King Rastakhan and Zul the Prophet. A VASSAL (Core/Conquest.cs):
  // sworn to whoever holds Zuldazar -- "they side with whoever promises a united troll empire". Its quests unite the
  // troll cities; Zul'Gurub is Hakkar's prison: the Zandalari took it from the Blood God's priests once before.
  public sealed class ZandalariFaction : Faction
  {
    internal const string FactionName = "Zandalari";

    public ZandalariFaction()
      : base(FactionName, playercolor.Yellow, @"ReplaceableTextures\CommandButtons\BTNHeadHunterBerserker.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_N0B6_CONTROL_POINT_DEFENDER_FROSTWOLF; // an orcish / troll tower
      Nicknames = new List<string> { "zandalari", "zandalar", "trolls", "troll", "zul", "rastakhan" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nWith C'Thun fallen, the Zandalari reclaim the south seas. The empire serves "
        + "whoever holds Zuldazar -- lose it and you march for its new master. Unite Zul'Aman, Gundrak, Zul'Farrak and "
        + "Zul'Gurub to restore the empire. Your halls cannot fall.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Zandalari Empire")}|r"));
      ProcessObjectInfo(ZandalariObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      var amani = AddQuest(new QuestTrollCity("The Amani", "The Amani of Zul'Aman have fought the elves alone for "
        + "thousands of years. Show them the empire's might and they will kneel to Rastakhan.", "BTNForestTrollShadowPriest",
        UNIT_N01V_ZUL_AMAN, 300));
      var drakkari = AddQuest(new QuestTrollCity("The Drakkari", "Gundrak's ice trolls bargain with their own gods. "
        + "Take Zul'Drak and the Drakkari answer Zuldazar's call again.", "BTNIceTrollShadowPriest", UNIT_N00H_ZUL_DRAK, 300));
      var sandfury = AddQuest(new QuestTrollCity("The Sandfury", "Zul'Farrak's sand trolls guard their pyramid "
        + "jealously. Seize the city and the Sandfury join the empire.", "BTNDarkTrollShadowPriest", UNIT_N092_ZUL_FARRAK, 300));
      var gurubashi = AddQuest(new QuestTrollCity("The Soulflayer's Prison", "Hakkar's priests stir again in Zul'Gurub. "
        + "Once the Zandalari broke the Blood God's grip on the Gurubashi -- take the city before he is reborn.",
        "BTNTrollBurrow", UNIT_N00W_ZUL_GURUB, 300));
      StartingQuest = amani;
      AddQuest(new QuestEmpireReborn(amani, drakkari, sandfury, gurubashi));
    }
  }

  // one troll city: hold its CP -> gold, the tribe joins
  internal sealed class QuestTrollCity : QuestData
  {
    private readonly int _gold;

    internal QuestTrollCity(string title, string text, string icon, int cpType, int gold)
      : base(title, text, @"ReplaceableTextures\CommandButtons\" + icon + ".blp")
    {
      _gold = gold;
      AddObjective(new ObjectiveControlPoint(cpType));
    }

    protected override string RewardDescription => "The tribe joins the empire: " + I2S(_gold) + " gold";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p != null)
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + _gold);
      }
    }
  }

  // all four cities -> the City of Gold research: the Direhorns at the Dinosaur Pen
  internal sealed class QuestEmpireReborn : QuestData
  {
    internal QuestEmpireReborn(params QuestData[] cities) : base("The Empire Reborn",
      "Amani, Drakkari, Sandfury and Gurubashi bow to Zuldazar once more. The Zandalari Empire stands united.",
      @"ReplaceableTextures\CommandButtons\BTNHeadHunterBerserker.blp")
    {
      foreach (var city in cities)
      {
        AddObjective(new ObjectiveQuestComplete(city));
      }

      ResearchId = ZandalariObjects.QuestCityOfGold;
    }

    protected override string RewardDescription => "Direhorns can be trained at the Dinosaur Pen";
  }
}
