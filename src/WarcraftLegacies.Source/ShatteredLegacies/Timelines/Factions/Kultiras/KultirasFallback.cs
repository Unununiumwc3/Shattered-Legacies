using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Kultiras
{
  // 0.39.5 (maintainer's design): Kul Tiras -> the Kezan Enterprise (was FallbackSystem.KulTirasFallback).
  // Everything Kul Tiras owns goes to Stormwind (if Stormwind still plays) or is removed -- except Katherine (moved to
  // Kezan) and the Hourglass. Kezan and the Neutral Hostile creeps around it become Kul Tiras'. Kul Tiras loses all
  // its allies. To be rebuilt on WL's mechanics (own Faction class and ObjectInfo, like the Scarlet Crusade):
  // TODO.md, step 3.
  internal static class KultirasFallback
  {
    internal static void Run(player p, TurningPointDef def)
    {
      player stormwind = null;
      foreach (var f in FactionManager.GetAllFactions())
      {
        if (f.Name == "Stormwind" && f.Player != null && AiApi.IsActive(f.Player))
        {
          stormwind = f.Player;
        }
      }

      unit katherine = null;
      var legends = LegendaryHeroManager.GetAll();
      for (var i = 0; i < legends.Count; i++)
      {
        if (legends[i].Name == "Katherine Proudmoore")
        {
          katherine = legends[i].Unit;
        }
      }

      var pid = GetPlayerId(p);
      var hourglass = Hourglass.Get(pid);
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (u == katherine || u == hourglass)
        {
          continue;
        }

        if (stormwind != null)
        {
          SetUnitOwner(u, stormwind, true);
        }
        else
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
      if (katherine != null && GetUnitState(katherine, UNIT_STATE_LIFE) > 0.405f)
      {
        SetUnitPosition(katherine, def.X, def.Y);
      }

      var kezan = AiApi.SafeCp(KezanEnterpriseConfig.KezanCp);
      unit kezanUnit = null;
      if (kezan != null && kezan.Unit != null)
      {
        kezanUnit = kezan.Unit;
        AiApi.GiveUnit(kezanUnit, p);
      }

      // 0.39.7: everything neutral on Kezan (Regions.KezanAmbient) joins: the goblin buildings there (Scrapyard,
      // Bunker, Laboratory, Rocket Tower, Oil Supply ...) and the units -- passive AND hostile
      var area = Rect(KezanEnterpriseConfig.AreaMinX, KezanEnterpriseConfig.AreaMinY, KezanEnterpriseConfig.AreaMaxX,
        KezanEnterpriseConfig.AreaMaxY);
      var g2 = CreateGroup();
      GroupEnumUnitsInRect(g2, area, null);
      var taken = 0;
      while ((u = FirstOfGroup(g2)) != null)
      {
        GroupRemoveUnit(g2, u);
        var o = GetPlayerId(GetOwningPlayer(u));
        if ((o == PLAYER_NEUTRAL_AGGRESSIVE || o == PLAYER_NEUTRAL_PASSIVE) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
            && u != kezanUnit)
        {
          SetUnitOwner(u, p, true);
          taken++;
        }
      }

      DestroyGroup(g2);
      RemoveRect(area);
      AiApi.LogPin("Kezan: " + I2S(taken) + " neutral buildings / units joined");

      // 0.39.6: the faction becomes "The Kezan Enterprise" -- renamed, goblin start buildings, its units allowed
      var fac = p.GetPlayerData().Faction;
      if (fac != null)
      {
        fac.Name = KezanEnterpriseConfig.Name;
      }

      for (var i = 0; i < KezanEnterpriseConfig.UnitTypes.Length; i++)
      {
        SetPlayerTechMaxAllowed(p, KezanEnterpriseConfig.UnitTypes[i], 100);
      }

      for (var i = 0; i < KezanEnterpriseConfig.HeroTypes.Length; i++)
      {
        SetPlayerTechMaxAllowed(p, KezanEnterpriseConfig.HeroTypes[i], 1); // 0.39.7: all three Altar of Industry heroes
      }

      fac?.Unally(); // 0.39.7: WL's own unally -- new team "<n> Pact", the scoreboard follows
      KezanEnterpriseMechanics.Start(p); // 0.39.8: oil income, creep market
      var usedX = new List<float>();
      var usedY = new List<float>();
      var placed = 0;
      for (var i = 0; i < KezanEnterpriseConfig.BuildingTypes.Length; i++)
      {
        if (AiApi.FindBuildSpot(def.X, def.Y, usedX, usedY, out var sx, out var sy))
        {
          CreateUnit(p, KezanEnterpriseConfig.BuildingTypes[i], sx, sy, 270f);
          usedX.Add(sx);
          usedY.Add(sy);
          placed++;
        }
      }

      AiApi.LogPin("Kul Tiras fallback -> The Kezan Enterprise: old army to " + (stormwind != null ? "Stormwind" : "removed")
                   + ", " + I2S(placed) + " goblin buildings, own team (Pact)");
    }
  }
}
