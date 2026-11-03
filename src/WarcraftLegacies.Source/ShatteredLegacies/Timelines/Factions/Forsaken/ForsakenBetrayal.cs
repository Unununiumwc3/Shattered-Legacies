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
  //              bot plays it; the player becomes the Forsaken: Sylvanas as a banshee hero, an altar and builders in
  //              Northrend near Drak'Tharon, on the Scourge's team (living-target rule), 10 turns safe from the
  //              defeat rule
  internal static class ForsakenBetrayal
  {
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
      SplitFaction.Run(p, slot, new ForsakenFaction(), keep);
      var placed = Spawn.Buildings(p, ForsakenConfig.StartX, ForsakenConfig.StartY, ForsakenConfig.StartBuildings);
      Spawn.Units(p, ForsakenConfig.StartX + 200f, ForsakenConfig.StartY - 200f, ForsakenConfig.StartUnits);
      CreateUnit(p, ForsakenConfig.SylvanasBanshee, ForsakenConfig.StartX + 100f, ForsakenConfig.StartY - 100f, 270f);
      ChangeTeam.JoinOrAlone(p, scourgeTeam);
      p.RepositionCamera(ForsakenConfig.StartX, ForsakenConfig.StartY);
      AiApi.LogPin("Quel'thalas betrayal -> Forsaken P" + I2S(GetPlayerId(p)) + ", Quel'thalas -> P"
                   + I2S(GetPlayerId(slot)) + " (bot), " + I2S(placed) + " start buildings");
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
