using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;
using MacroTools.Utils;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Factions.Stormwind.Quests;
using WarcraftLegacies.Source.Objectives.FactionBased;
using WarcraftLegacies.Source.Objectives.UnitBased;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Defias
{
  // 0.87.0 (maintainer, agreed 0.81): Stormwind falls back to the Defias before it won Stromgarde (WL's quest
  // "Stromgarde" still open) -> the quest passes to LORDAERON ("Stromgarde's Oath": a Lordaeron hero at Stromgarde ->
  // Stromgarde's units and Galen Trollbane). Is Lordaeron gone too (no player, defeated, or fallen back to Gilneas),
  // Stromgarde becomes a capturable neutral (its units hostile, like WL's failed quest).
  // Stormwind's own quest object stays open and harmless: its eligible faction has no player any more.
  internal static class StromgardeHandover
  {
    internal const string ToLordaeronText =
      "|cffffcc00Stormwind is gone.|r Galen Trollbane will not bend to bandits -- Stromgarde looks north and offers its "
      + "swords to Lordaeron. A Lordaeron hero must ride to Stromgarde.";

    internal const string ToNobodyText =
      "|cffffcc00Stormwind and Lordaeron are gone.|r Stromgarde stands alone, bars its gates and trusts no one -- "
      + "whoever wants Arathi's last fortress must take it.";

    internal static void Run(Faction stormwind)
    {
      var open = stormwind?.GetQuestByType<QuestStromgarde>();
      if (open == null || open.Progress == QuestProgress.Complete || open.Progress == QuestProgress.Failed)
      {
        return;
      }

      var units = new List<unit>();
      foreach (var u in GlobalGroup.EnumUnitsInRect(Regions.Stromgarde))
      {
        if (u.Owner == player.NeutralPassive)
        {
          units.Add(u);
        }
      }

      var lordaeron = FindLordaeron();
      if (lordaeron != null)
      {
        lordaeron.AddQuest(new QuestStromgardeOath(units));
        Announce(ToLordaeronText);
        AiApi.LogPin("Stromgarde -> Lordaeron (" + I2S(units.Count) + " units)");
        return;
      }

      foreach (var u in units)
      {
        u.IsInvulnerable = false;
        u.Rescue(player.NeutralAggressive);
      }

      Announce(ToNobodyText);
      AiApi.LogPin("Stromgarde -> neutral hostile (" + I2S(units.Count) + " units)");
    }

    private static Faction FindLordaeron()
    {
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == "Lordaeron" && f.Player != null && f.ScoreStatus != ScoreStatus.Defeated)
        {
          return f;
        }
      }

      return null;
    }

    private static void Announce(string text)
    {
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, text);
      }
    }
  }

  // Lordaeron's copy of WL's "Stromgarde" (own title: a faction cannot hold two quests of one name)
  internal sealed class QuestStromgardeOath : QuestData
  {
    private readonly List<unit> _units;

    internal QuestStromgardeOath(List<unit> units) : base("Stromgarde's Oath",
      "With Stormwind gone, Galen Trollbane looks north. Stromgarde will march for Lordaeron -- if Lordaeron shows it "
      + "still has a king's will.",
      @"ReplaceableTextures\CommandButtons\BTNTheCaptain.blp")
    {
      _units = units;
      AddObjective(new ObjectiveAnyUnitInRect(Regions.Stromgarde, "Stromgarde", true));
      AddObjective(new ObjectiveSelfExists());
    }

    public override string RewardFlavour => "Galen Trollbane has pledged Stromgarde's forces to Lordaeron.";

    protected override string RewardDescription => "Control of all units at Stromgarde, and Galen Trollbane joins you";

    protected override void OnComplete(Faction completingFaction)
    {
      var p = completingFaction.Player;
      if (p == null)
      {
        return;
      }

      foreach (var u in _units)
      {
        u.IsInvulnerable = false;
        u.Rescue(p);
      }

      var galen = AllLegends.Stormwind.Galen;
      if (galen.Unit == null || GetUnitState(galen.Unit, UNIT_STATE_LIFE) <= 0.405f)
      {
        galen.ForceCreate(p, Regions.Stromgarde.Center, 270f);
      }
    }

    protected override void OnFail(Faction completingFaction)
    {
      foreach (var u in _units)
      {
        u.IsInvulnerable = false;
        u.Rescue(player.NeutralAggressive);
      }
    }
  }
}
