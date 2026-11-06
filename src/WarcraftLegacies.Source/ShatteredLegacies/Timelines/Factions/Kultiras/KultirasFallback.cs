using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Kultiras
{
  // Kul Tiras -> the Kezan Enterprise (0.39.5, maintainer's design; 0.58 rebuilt on the Forsaken's / Sunfury's building
  // blocks, TODO step 3). Effects, in this order:
  //   1. swap faction: a new KezanEnterpriseFaction (its tree, limits and quests; the player keeps his colour)
  //   2. leave behind (inheritance rule): everything Kul Tiras owns goes to its lore heir Stormwind if Stormwind still
  //      plays on Kul Tiras' team, otherwise removed / Neutral Hostile -- except Katherine and the Hourglass
  //   3. relocate: the base preplaced on Kezan (Regions.KezanUnlock, hidden until now) is taken over, WL's creep
  //      goblins replaced by the Enterprise's copies (the Trading Center too, 0.59); Kezan's CP joins; start kit (Altar of Industry, 2 Oil Supplies,
  //      4 Builders)
  //   4. heroes: Katherine Proudmoore leads the survivors (moved, revived or created)
  //   5. team: its own ("Kezan Enterprise Pact"), allied with no one
  //   6. mechanics: oil (KezanEnterpriseOil), trade (KezanEnterpriseTrade, 0.59: the 24p Trading Center), the
  //      Refugee Slave Camps at the goblin towns (KezanEnterpriseSlaveCamps, 0.60); the end
  //      goal (20,000 gold), oil and trade on the scoreboard (KezanEnterpriseGoal). 0.59: the creep market of the main
  //      map's Trading Center is gone (maintainer: the pseudo mercenary centre comes another way).
  internal static class KultirasFallback
  {
    private static List<unit> _startUnits = new();

    // map setup: the base on Kezan waits -- buildings visible and invulnerable, units hidden (WL's rescue mechanism)
    internal static void Prepare()
    {
      _startUnits = StartArea.Prepare(Regions.KezanUnlock, RescuePreparationMode.HideNonStructures);
      KezanEnterpriseGoal.Setup(); // its lines on the scoreboard (empty until the Enterprise plays)
      KezanEnterpriseSlaveCamps.Setup(); // 0.60: refugee limits for every faction; fallen factions from the start
    }

    internal static void Run(player p, TurningPointDef def)
    {
      var katherine = AllLegends.Kultiras.LegendKatherine;
      var keep = new List<unit>();
      var hourglass = Hourglass.Get(GetPlayerId(p));
      if (hourglass != null)
      {
        keep.Add(hourglass);
      }

      var kathUnit = katherine.Unit;
      var kathIsOurs = kathUnit != null && GetOwningPlayer(kathUnit) == p;
      if (kathIsOurs)
      {
        keep.Add(kathUnit);
      }

      var enterprise = new KezanEnterpriseFaction();
      SwapFaction.Run(p, enterprise); // first: if it fails, the player keeps his units
      var heir = LeaveBehind.ToHeir(p, KezanEnterpriseConfig.Heir, keep);
      var taken = StartArea.Take(p, _startUnits, enterprise, KezanEnterpriseObjects.MainEquivalents);
      StartCps.Give(p, UNIT_N04Z_KEZAN);

      var placed = Spawn.Buildings(p, KezanEnterpriseConfig.StartX, KezanEnterpriseConfig.StartY,
        KezanEnterpriseConfig.StartBuildings);
      Spawn.Units(p, KezanEnterpriseConfig.UnitsX, KezanEnterpriseConfig.UnitsY, KezanEnterpriseConfig.StartUnits);
      var start = new Point(KezanEnterpriseConfig.StartX, KezanEnterpriseConfig.StartY);
      if (kathIsOurs && GetUnitState(kathUnit, UNIT_STATE_LIFE) > 0.405f)
      {
        SetUnitPosition(kathUnit, KezanEnterpriseConfig.StartX, KezanEnterpriseConfig.StartY);
      }
      else if (kathUnit == null || kathIsOurs || GetUnitState(kathUnit, UNIT_STATE_LIFE) <= 0.405f)
      {
        katherine.ForceCreate(p, start, 270f); // ours but dead, or nobody's; another player's living Katherine stays his
      }

      p.GetPlayerData().Faction?.Unally(); // WL's own unally: a new team "<name> Pact", the scoreboard follows
      SplitFaction.UnallyOutsideTeam(p);   // WL's teams only add alliances: the lobby's South Alliance ends here
      KezanEnterpriseOil.Start(p);
      KezanEnterpriseTrade.Start(p);
      KezanEnterpriseSlaveCamps.Start(p); // 0.60: the Refugee Slave Camps rise at the goblin towns
      p.RepositionCamera(start);
      AiApi.LogPin("Kul Tiras fallback -> Kezan Enterprise: old holdings to "
                   + (heir != null ? "Stormwind" : "neutral hostile") + ", " + I2S(taken) + " start units, "
                   + I2S(placed) + " start buildings, own team");
    }
  }
}
