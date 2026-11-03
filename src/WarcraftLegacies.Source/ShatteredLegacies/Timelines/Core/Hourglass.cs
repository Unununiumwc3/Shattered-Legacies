using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // The Hourglass of Fate (was the "Fallback" trigger building, 0.39.1): one building per player slot whose faction has
  // a turning point. Humans start their turning point with its ability; bots never need it (TimelineSystem runs
  // theirs by code). Planned (TODO.md, step 2): one research per turning point, greyed out until its trigger fired.
  // The map data still calls the building "Fallback" (n0FB) and the ability "Begin the Fallback" (AFB1).
  internal static class Hourglass
  {
    internal const int BuildingType = 1848657474; // n0FB (copy of a CP model, invulnerable, no attack)
    internal const int AbilityId = 1095123505;    // AFB1 "Begin the Fallback" (channel, copy of LLLN)

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

    internal static void Setup()
    {
      var create = CreateTimer();
      TimerStart(create, CreateAt, false, Create);
      var cast = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(cast, Player(i), EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      }

      TriggerAddAction(cast, OnCast);
    }

    internal static bool Has(int pid) => _building.ContainsKey(pid);

    internal static unit Get(int pid) => _building.ContainsKey(pid) ? _building[pid] : null;

    // the turning point is available: a human may start it now
    internal static void Offer(int pid)
    {
      if (_building.ContainsKey(pid))
      {
        UnitAddAbility(_building[pid], AbilityId);
      }
    }

    // no longer available (HQ held again)
    internal static void Withdraw(int pid)
    {
      if (_building.ContainsKey(pid))
      {
        UnitRemoveAbility(_building[pid], AbilityId);
      }
    }

    internal static void MarkUsed(int pid, TurningPointDef def)
    {
      if (_building.ContainsKey(pid))
      {
        UnitRemoveAbility(_building[pid], AbilityId);
        BlzSetUnitName(_building[pid], "Fallback: " + def.Name + " (used)");
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

      var placedCount = 0;
      for (var oi = 0; oi < order.Count; oi++)
      {
        var pid = order[oi];
        var p = Player(pid);
        var fac = p.GetPlayerData().Faction;
        if (fac == null)
        {
          continue;
        }

        var def = TimelineCatalog.Find(fac.Name);
        if (def == null)
        {
          continue;
        }

        TimelineSystem.Register(pid, def);
        // 0.39.7: middle column first, then left, then right (each RowsPerColumn high, going north)
        var colIndex = placedCount / RowsPerColumn; // 0 middle, 1 left, 2 right, 3 far left ...
        var row = placedCount % RowsPerColumn;
        placedCount++;
        var side = colIndex == 0 ? 0 : (colIndex % 2 == 1 ? -((colIndex + 1) / 2) : colIndex / 2);
        var x = CenterX + ColumnStep * side;
        var y = FirstRowY + RowStep * row;
        var u = CreateUnit(p, BuildingType, x, y, 270f);
        BlzSetUnitName(u, "Fallback: " + def.Name);
        _building[pid] = u;
      }

      AiApi.Log("timelines: " + I2S(_building.Count) + " Hourglass building(s) created");
    }

    private static void OnCast()
    {
      if (GetSpellAbilityId() != AbilityId)
      {
        return;
      }

      TimelineSystem.OnHourglassUsed(GetPlayerId(GetOwningPlayer(GetTriggerUnit())));
    }
  }
}
