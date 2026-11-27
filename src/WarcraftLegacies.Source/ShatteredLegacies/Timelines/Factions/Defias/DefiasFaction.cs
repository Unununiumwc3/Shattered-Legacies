using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Localization;
using MacroTools.Quests;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.QuestBased;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Defias
{
  // 0.85.0 (maintainer): the Defias Brotherhood -- Stormwind's fallback in the Deadmines (DefiasFallback). WL's bandit
  // creeps as Defias units, every one with Pillage (tools/shattered-legacies/content/minor_factions.py), no buildings:
  // recruits come every turn (Core/Effects/TurnLevy). Two leaders with two goals:
  //   Dunco Ratsbon wants the loot  -> "Ratsbon's Haul": hold 1500 gold at once
  //   Count Clessington wants land  -> "The Clessington Estates": Westfall, Duskwood and Elwynn Forest
  // Both -> "The Brotherhood Rises". Every quest raises the recruits per turn. A VASSAL of whoever holds the Deadmines.
  public sealed class DefiasFaction : Faction
  {
    internal const string FactionName = "Defias";

    public DefiasFaction()
      : base(FactionName, playercolor.Peanut, @"ReplaceableTextures\CommandButtons\BTNBanditLord.blp")
    {
      ControlPointDefenderUnitTypeId = UNIT_H05X_CONTROL_POINT_DEFENDER_STORMWIND; // Stormwind's towers, turned
      Nicknames = new List<string> { "defias", "brotherhood", "bandits", "ratsbon", "clessington" };
      IntroText = () => Loc.Format(
        "You are playing as {faction}.\n\nStormwind has fallen and its gutters belong to the Brotherhood. Dunco "
        + "Ratsbon wants the kingdom's gold, Count Clessington wants its lands -- you serve whoever holds the Deadmines, "
        + "and every one of your cutthroats pillages what he strikes. Recruits come every turn.",
        ("{faction}", $"{PrefixCol}{Loc.Get("the Defias Brotherhood")}|r"));
      ProcessObjectInfo(DefiasObjectInfo.GetAllObjectLimits());
    }

    public override void OnRegistered()
    {
      SharedFactionConfigSetup.AddSharedFactionConfig(this);
      var haul = AddQuest(new QuestDefiasLevy("Ratsbon's Haul",
        "\"Gold talks, and every cutthroat in Westfall listens.\" Fill the Brotherhood's chests and Dunco Ratsbon's "
        + "name draws the desperate to the Deadmines.", "BTNChestOfGold", "2 more recruits every turn (a Rogue and an "
        + "Assassin)", new[] { DefiasObjects.Rogue, DefiasObjects.Assassin }, 0, new ObjectiveHoldGold(1500)));
      var estates = AddQuest(new QuestDefiasLevy("The Clessington Estates",
        "The House of Nobles stripped Count Clessington of his lands. Take Westfall, Duskwood and Elwynn Forest and "
        + "the count holds court again -- with the Brotherhood as his guard.", "BTNBanditMage", "500 gold and 2 more "
        + "recruits every turn (an Enforcer and a Brigand)", new[] { DefiasObjects.Enforcer, DefiasObjects.Brigand },
        500, new ObjectiveControlPoint(UNIT_N02L_WESTFALL), new ObjectiveControlPoint(UNIT_N00V_DUSKWOOD),
        new ObjectiveControlPoint(UNIT_N00Z_ELWYNN_FOREST)));
      StartingQuest = haul;
      AddQuest(new QuestDefiasLevy("The Brotherhood Rises",
        "Gold in Ratsbon's chests, land under Clessington's seal: the Brotherhood is no gang any more. It is the new "
        + "power of the south.", "BTNBanditLord", "3 more recruits every turn (a Bandit, an Assassin and an Enforcer)",
        new[] { DefiasObjects.Bandit, DefiasObjects.Assassin, DefiasObjects.Enforcer }, 0,
        new ObjectiveQuestComplete(haul), new ObjectiveQuestComplete(estates)));
      AddQuest(new QuestStonemasonsDue()); // 0.88.0: Edwin VanCleef for 1,500 gold pillaged
    }
  }

  // a Defias quest: raises the recruits per turn (DefiasFallback.Levy), maybe gold
  internal sealed class QuestDefiasLevy : QuestData
  {
    private readonly string _reward;
    private readonly int[] _recruits;
    private readonly int _gold;

    internal QuestDefiasLevy(string title, string text, string icon, string reward, int[] recruits, int gold,
      params Objective[] objectives) : base(title, text, @"ReplaceableTextures\CommandButtons\" + icon + ".blp")
    {
      _reward = reward;
      _recruits = recruits;
      _gold = gold;
      foreach (var o in objectives)
      {
        AddObjective(o);
      }
    }

    protected override string RewardDescription => _reward;

    protected override void OnComplete(Faction whichFaction)
    {
      DefiasFallback.Levy?.Add(_recruits);
      var p = whichFaction.Player;
      if (p != null && _gold > 0)
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + _gold);
      }
    }
  }
}
