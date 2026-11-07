using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai
{
  internal static class AiBuilder
  {
    private static readonly UnitCategory[] ProductionCats =
    {
      UnitCategory.Barracks, UnitCategory.SiegeWorkshop, UnitCategory.Magic, UnitCategory.FlyingBuilding,
      UnitCategory.Specialist,
    };

    internal static void Setup()
    {
      AiScheduler.Add("builder", 720, 360, Tick, true);
    }

    private static void Tick()
    {
      for (var i = 0; i < SimpleBot.Bots.Count; i++)
      {
        var p = SimpleBot.Bots[i];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || !SimpleBot.HasPerks(p))
        {
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        var workers = 0;
        var halls = 0;
        var ownedTypes = new List<int>();
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }

          SimpleBot.OpUnits++;
          var tid = GetUnitTypeId(u);
          if (IsUnitType(u, UNIT_TYPE_PEON))
          {
            workers++;
          }
          if (IsUnitType(u, UNIT_TYPE_TOWNHALL))
          {
            halls++;
          }
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE) && !SimpleBot.ListHas(ownedTypes, tid))
          {
            ownedTypes.Add(tid);
          }
        }
        DestroyGroup(g);

        var prodTypes = new List<int>();
        for (var c = 0; c < ProductionCats.Length; c++)
        {
          if (!fac.TryGetObjectByCategory(ProductionCats[c], out var ids) || ids == null)
          {
            continue;
          }
          for (var k = 0; k < ids.Count; k++)
          {
            var id = ids[k];
            if (AiData.Trains.ContainsKey(id))
            {
              if (!SimpleBot.ListHas(prodTypes, id))
              {
                prodTypes.Add(id);
              }
            }
          }
        }

        var prodKinds = 0;
        for (var t = 0; t < ownedTypes.Count; t++)
        {
          if (AiData.Trains.ContainsKey(ownedTypes[t]))
          {
            prodKinds++;
          }
        }

        var missing = new List<int>();
        for (var pidx = 0; pidx < prodTypes.Count; pidx++)
        {
          var found = false;
          for (var oidx = 0; oidx < ownedTypes.Count; oidx++)
          {
            if (ownedTypes[oidx] == prodTypes[pidx])
            {
              found = true;
              break;
            }
          }
          if (!found)
          {
            missing.Add(prodTypes[pidx]);
          }
        }

        var missingNames = "";
        for (var m = 0; m < missing.Count; m++)
        {
          if (m > 0)
          {
            missingNames = missingNames + ",";
          }
          missingNames = missingNames + " " + GetObjectName(missing[m]);
        }

        SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " workers=" + workers + " halls=" + halls + " prodKinds=" + prodKinds + (workers == 0 ? " RULE1" : "") + (prodKinds < 2 ? " RULE2" : "") + " missing:" + missingNames);
      }
    }
  }
}