using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.54, the pilot of a BETRAYAL with a SPLIT (Timelines/SCENARIO_CONCEPTS.md P3, D3; maintainer's design):
  //   trigger:   Sylvanas (Quel'thalas' legend) is slain by the Scourge
  //   on trigger (whatever is decided): she is lost to Quel'thalas for good (WL's PermanentlyKill, no retraining)
  //   decision:  "Betrayal: Forsaken" at the Hourglass; bots always take it (one player becomes two)
  //   effects:   split -- Quel'thalas moves with its army, quests and colour to a free slot (P11 / P14 / P20) and the
  //              bot plays it, keeping all gold and lumber; the player becomes the Forsaken: 500 gold, Sylvanas the
  //              Banshee, the preplaced Altar of the Queen and 3 Acolytes in Northrend (-1340, 20171), on the Scourge's
  //              team (living-target rule), 10 turns safe from the defeat rule
  internal static class ForsakenBetrayal
  {
    private static unit _altar;

    // 0.57.3: the Altar of the Queen was invulnerable until the Forsaken are free (QuestForsakenFreeWill); 0.60
    // (maintainer): it is invulnerable only while it waits -- attackable as soon as the Forsaken enter the game
    internal static void AltarVulnerable()
    {
      if (_altar != null && GetUnitTypeId(_altar) != 0)
      {
        SetUnitInvulnerable(_altar, false);
      }
    }

    // map setup (TurningPointDef.Prepare): Sylvanas' legend, and the Altar of the Queen preplaced in Northrend --
    // Neutral Passive and invulnerable until the Forsaken take it (and still invulnerable then: their safe start)
    internal static void Prepare()
    {
      ForsakenSylvanas.Setup();
      ForsakenUndercity.Prepare();
      _altar = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), ForsakenObjects.AltarOfTheQueen, ForsakenConfig.StartX,
        ForsakenConfig.StartY, 270f);
      SetUnitInvulnerable(_altar, true);
    }

    internal static void OnSylvanasSlain(player p)
    {
      var sylvanas = AllLegends.Quel.Sylvanas;
      sylvanas.PermanentlyKill();
      var fac = p.GetPlayerData().Faction;
      fac?.SetObjectLimit(ForsakenConfig.SylvanasRanger, 0);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 15, "|cffffcc00Sylvanas Windrunner has fallen to the Scourge.|r "
          + "Quel'thalas has lost its Ranger-General.");
      }
    }

    internal static void Run(player p, TurningPointDef def)
    {
      var slot = SplitFaction.FindFreeSlot();
      if (slot == null)
      {
        DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffff6600No free player slot is left:|r the Forsaken cannot break away.");
        AiApi.LogPin("Forsaken betrayal for P" + I2S(GetPlayerId(p)) + " skipped: no free slot");
        return;
      }

      var keep = new List<unit>();
      var hourglass = Hourglass.Get(GetPlayerId(p));
      if (hourglass != null)
      {
        keep.Add(hourglass);
      }

      var scourgeTeam = TeamOf("Scourge");
      SplitFaction.Run(p, slot, new ForsakenFaction(), keep, ForsakenConfig.StartGold, ForsakenConfig.StartLumber);
      if (_altar != null && GetUnitTypeId(_altar) != 0)
      {
        SetUnitOwner(_altar, p, true);
      }

      AltarVulnerable(); // 0.60: from the arrival on

      Spawn.Units(p, ForsakenConfig.StartX + 250f, ForsakenConfig.StartY - 250f, ForsakenConfig.StartUnits);
      ForsakenSylvanas.Spawn(p, ForsakenConfig.StartX + 150f, ForsakenConfig.StartY - 300f);
      ChangeTeam.JoinOrAlone(p, scourgeTeam);
      p.RepositionCamera(ForsakenConfig.StartX, ForsakenConfig.StartY);
      AiApi.LogPin("Quel'thalas betrayal -> Forsaken P" + I2S(GetPlayerId(p)) + ", Quel'thalas -> P"
                   + I2S(GetPlayerId(slot)) + " (bot)");
    }

    private static Team TeamOf(string factionName)
    {
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == factionName && f.Player != null)
        {
          return f.Player.GetPlayerData().Team;
        }
      }

      return null;
    }
  }
}
