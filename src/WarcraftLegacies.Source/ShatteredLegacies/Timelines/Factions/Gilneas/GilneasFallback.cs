using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Factions.Gilneas;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Gilneas
{
  // 0.76.0 (maintainer): Stormwind -> GILNEAS (replaces the Valiance Expedition placeholder). Gilneas is no longer a
  // pick of the Dalaran slot (Setup/FactionChoiceDialogSetup.cs) -- that also takes Gilneas away from the North
  // Alliance until Stormwind falls (Dalaran's "The Greymane Wall" is gone), a nerf of the north against the Scourge.
  //   1. map setup: everything in Regions.Gilneas -- the five CPs (Gilneas City, Keel Harbor, Duskhaven, Blackwald,
  //      Tempest Reach) included -- waits hidden, paused and invulnerable (Neutral Passive), as it did for Dalaran's
  //      Greymane Wall. Bots never go there: hidden / neutral CPs are no targets, and the route gate "greymane"
  //      (Silverpine <-> Gilneas City, AiGates) opens only once a player holds Gilneas City.
  //   2. Stormwind Keep lost -> swap faction (GilneasFaction in fallback mode), heroes and the rest of Stormwind are
  //      left behind (Neutral Hostile), the Gilneas region and its CPs are taken over, Liberation of Gilneas is
  //      researched (Genn Greymane and the worgen), Genn leads them, the team becomes the NORTH ALLIANCE.
  internal static class GilneasFallback
  {
    private static List<unit> _units = new();
    private static bool _prepared;

    internal static void Prepare()
    {
      if (_prepared)
      {
        return;
      }

      _prepared = true;
      _units = StartArea.Prepare(Regions.Gilneas);
    }

    internal static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var hourglass = Hourglass.Get(GetPlayerId(p));
      if (hourglass != null)
      {
        keep.Add(hourglass);
      }

      var gilneas = new GilneasFaction(true);
      SwapFaction.Run(p, gilneas); // first: if it fails, the player keeps his units
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      var taken = StartArea.Take(p, _units, gilneas);
      var cps = StartCps.Give(p, UNIT_N046_GILNEAS_CITY, UNIT_N08X_KEEL_HARBOR, UNIT_N031_DUSKHAVEN, UNIT_N06V_BLACKWALD,
        UNIT_N084_TEMPEST_REACH);
      SetPlayerTechResearched(p, UPGRADE_R02R_QUEST_COMPLETED_LIBERATION_OF_GILNEAS, 1);
      var castle = AllLegends.Gilneas.GilneasCastle.Unit;
      var at = castle != null ? castle.GetPosition() : Regions.Gilneas.Center;
      AllLegends.Gilneas.Genn.ForceCreate(p, at, 270f);
      ChangeTeam.JoinOrAlone(p, TeamSetup.NorthAlliance);
      p.RepositionCamera(at);
      AiApi.LogPin("Stormwind fallback -> Gilneas: " + I2S(taken) + " units, " + I2S(cps) + " more CPs, team "
                   + (p.GetPlayerData().Team != null ? p.GetPlayerData().Team.Name : "-"));
    }
  }
}
