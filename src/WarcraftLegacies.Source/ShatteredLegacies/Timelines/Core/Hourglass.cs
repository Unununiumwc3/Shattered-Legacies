using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.UnitTypes;
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
    private static readonly Dictionary<int, float> _x = new();       // pid -> its place (0.63.2: to put it back)
    private static readonly Dictionary<int, float> _y = new();
    private static int _placed;                                        // buildings placed in the grid so far

    internal static void Setup()
    {
      // 0.63.2: WL code that clears a player's buildings must leave it alone (the Illidari's "Return to Outland"
      // disposes every building outside the Black Temple -- the Hourglass too). NeverDelete keeps it from being
      // removed (such code hands it to Neutral Hostile instead) -- the watch below gives it back.
      UnitType.Register(new UnitType(HourglassData.BuildingType) { NeverDelete = true });
      var create = CreateTimer();
      TimerStart(create, CreateAt, false, () => AiApi.Safe("hourglass", Create));
    }

    // 0.63.2: every 5 s -- an Hourglass that changed owner goes back; one that is gone is placed again
    private static void Watch()
    {
      foreach (var pid in new List<int>(_building.Keys))
      {
        var p = Player(pid);
        if (GetPlayerSlotState(p) != PLAYER_SLOT_STATE_PLAYING && !AiApi.IsActive(p))
        {
          continue; // the player left
        }

        var u = _building[pid];
        if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          var nu = CreateUnit(p, HourglassData.BuildingType, _x[pid], _y[pid], 270f);
          Skin(nu, p);
          _building[pid] = nu;
          Refresh(p);
          AiApi.Log("hourglass: P" + I2S(pid) + "'s Hourglass was gone -- placed again");
        }
        else if (GetOwningPlayer(u) != p)
        {
          SetUnitOwner(u, p, true);
          AiApi.Log("hourglass: P" + I2S(pid) + "'s Hourglass had changed owner -- given back");
        }
      }
    }

    private static void Skin(unit u, player p)
    {
      // 0.57 (maintainer): every faction's Hourglass looks like its own control point tower
      var defender = p.GetPlayerData().Faction?.ControlPointDefenderUnitTypeId;
      if (defender != null && defender.Value != 0)
      {
        BlzSetUnitSkin(u, defender.Value);
      }
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
      Skin(u, p);
      _building[GetPlayerId(p)] = u;
      _x[GetPlayerId(p)] = x;
      _y[GetPlayerId(p)] = y;
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
        RefreshChase(p);
        SetLimits(p, fac == null ? new List<TurningPointDef>() : TimelineCatalog.FindAll(fac.Name));
      }
    }

    internal static void RefreshChase(player p)
    {
      var glass = Get(GetPlayerId(p));
      if (glass == null)
      {
        return;
      }
      var chase = ABILITY_A0J5_CHASE_ILLIDAN_TO_OUTLAND_SENTINEL;
      var ready = TimelineSystem.AbilityReady(p, chase);
      var shown = GetUnitAbilityLevel(glass, chase) > 0;
      if (ready && !shown)
      {
        UnitAddAbility(glass, chase);
      }
      else if (!ready && shown)
      {
        UnitRemoveAbility(glass, chase);
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
      // 0.63.1 (maintainer: the Illidari had no Hourglass): a faction picked AFTER this moment (a slow pick in its
      // dialog) got none -- every later faction change now gives the player one if his faction has a turning point
      PlayerData.FactionChange += e => AiApi.Safe("hourglass late", () => Late(e.Player));
      var watch = CreateTimer();
      TimerStart(watch, 5f, true, () => AiApi.Safe("hourglass watch", Watch));
    }

    private static void Late(player p)
    {
      var fac = p.GetPlayerData().Faction;
      if (fac == null)
      {
        return;
      }

      var defs = TimelineCatalog.FindAll(fac.Name);
      for (var d = 0; d < defs.Count; d++)
      {
        if (defs[d].HasHourglass)
        {
          Give(p);
          return;
        }
      }
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
        if ((wl || defs[d].HourglassAbility == 0) && (wl ? defs[d].WlResearch : defs[d].Decision) == research)
        {
          return true;
        }
      }

      return false;
    }
  }
}
