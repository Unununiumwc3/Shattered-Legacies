using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // The Hourglass of Fate (0.39.1 the "Fallback" trigger building; 0.53 with researches): one building per player slot
  // whose faction has a turning point on it. Each turning point is one DECISION research (HourglassData, written by
  // tools/shattered-legacies/timelines/gen_hourglass.py); it requires the turning point's CONDITION tech, so it is
  // shown greyed out ("Requires: Silvermoon Palace lost") until its trigger fired. WL's own
  // turning-point researches (the Scarlet Crusade, R0XZ) are listed too and stay where WL has them. Researches of
  // other factions are hidden by object limits (0). Bots never need the building (TimelineSystem decides for them).
  internal static class Hourglass
  {
    // Place: the sealed top-right corner (Regions.ArtifactDummyInstance -- WL only creates artifact items there).
    // 0.39.7 (maintainer): 3 columns around x 22208 starting at y 23500 going north; the middle column fills first,
    // then left, then right. Order: team (scoreboard), then member. 150 apart (100 overlapped the building models).
    internal const float CenterX = 22208f;
    internal const float ColumnStep = 150f;
    internal const float FirstRowY = 23500f;
    internal const float RowStep = 150f;
    internal const int RowsPerColumn = 7;
    internal const float CreateAt = 62f; // after the faction picks (and where the mode vote used to end)

    private static readonly Dictionary<int, unit> _building = new(); // pid -> Hourglass
    private static int _placed;                                        // buildings placed in the grid so far

    internal static void Setup()
    {
      var create = CreateTimer();
      TimerStart(create, CreateAt, false, () => AiApi.Safe("hourglass", Create));
    }

    internal static unit Get(int pid) => _building.ContainsKey(pid) ? _building[pid] : null;

    // 0.39.7: middle column first, then left, then right (each RowsPerColumn high, going north)
    private static void Place(player p)
    {
      var colIndex = _placed / RowsPerColumn; // 0 middle, 1 left, 2 right, 3 far left ...
      var row = _placed % RowsPerColumn;
      _placed++;
      var side = colIndex == 0 ? 0 : (colIndex % 2 == 1 ? -((colIndex + 1) / 2) : colIndex / 2);
      var x = CenterX + ColumnStep * side;
      var y = FirstRowY + RowStep * row;
      var u = CreateUnit(p, HourglassData.BuildingType, x, y, 270f);
      // 0.57 (maintainer): every faction's Hourglass looks like its own control point tower
      var defender = p.GetPlayerData().Faction?.ControlPointDefenderUnitTypeId;
      if (defender != null && defender.Value != 0)
      {
        BlzSetUnitSkin(u, defender.Value);
      }

      _building[GetPlayerId(p)] = u;
    }

    // 0.55: a player who gets a faction mid-game (the free slot of a split) gets an Hourglass of his own, showing that
    // faction's turning points
    internal static void Give(player p)
    {
      if (!_building.ContainsKey(GetPlayerId(p)))
      {
        Place(p);
      }

      Refresh(p);
    }

    // 0.54: after a turning point changed the player's faction, his Hourglass shows the new faction's turning points
    internal static void Refresh(player p)
    {
      var fac = p.GetPlayerData().Faction;
      if (_building.ContainsKey(GetPlayerId(p)))
      {
        SetLimits(p, fac == null ? new List<TurningPointDef>() : TimelineCatalog.FindAll(fac.Name));
      }
    }

    private static void Create()
    {
      var teams = FactionManager.GetAllTeams();
      var order = new List<int>(); // 0.39.7: pids sorted by team (scoreboard order), then slot
      for (var t = 0; t <= teams.Count; t++)
      {
        for (var q = 0; q < 24; q++)
        {
          var team = Player(q).GetPlayerData().Team;
          var ti = teams.Count;
          for (var k = 0; k < teams.Count; k++)
          {
            if (team == teams[k])
            {
              ti = k;
            }
          }

          if (ti == t)
          {
            order.Add(q);
          }
        }
      }

      for (var oi = 0; oi < order.Count; oi++)
      {
        var pid = order[oi];
        var p = Player(pid);
        var fac = p.GetPlayerData().Faction;
        var defs = fac == null ? new List<TurningPointDef>() : TimelineCatalog.FindAll(fac.Name);
        var any = false;
        for (var d = 0; d < defs.Count; d++)
        {
          any = any || defs[d].HasHourglass;
        }

        if (!any)
        {
          continue;
        }

        SetLimits(p, defs);
        Place(p);
      }

      AiApi.Log("timelines: " + I2S(_building.Count) + " Hourglass building(s) created");
      TimelineSystem.ArmAll();
    }

    // the player sees only his own faction's researches; WL's own (R0XZ) stays as WL limits it for its faction
    private static void SetLimits(player p, List<TurningPointDef> defs)
    {
      var data = p.GetPlayerData();
      for (var i = 0; i < HourglassData.Decisions.Length; i++)
      {
        data.SetObjectLimit(HourglassData.Decisions[i], Owns(defs, HourglassData.Decisions[i], false) ? 1 : 0);
      }

      for (var i = 0; i < HourglassData.WlResearches.Length; i++)
      {
        if (!Owns(defs, HourglassData.WlResearches[i], true))
        {
          data.SetObjectLimit(HourglassData.WlResearches[i], 0);
        }
      }
    }

    private static bool Owns(List<TurningPointDef> defs, int research, bool wl)
    {
      for (var d = 0; d < defs.Count; d++)
      {
        if ((wl ? defs[d].WlResearch : defs[d].Decision) == research)
        {
          return true;
        }
      }

      return false;
    }
  }
}
