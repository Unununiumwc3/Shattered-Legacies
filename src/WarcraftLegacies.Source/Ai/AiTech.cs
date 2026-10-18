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
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- tech pools, building credits, free/event/quest upgrades and the CP-capture counter that feeds them.
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiTech
  {

    // ---- v32 tech pool / event researches / CP-capture counting ----------------------------------------------
    // v33: unit/ability tech steps per faction (built once): ByFaction + Extra, minus events / never-grant,
    // deduplicated, then expanded by level (all level 1 -> multi-level ones at 2 -> at 3).
    internal sealed class TechSteps
    {
      public readonly List<int> Ids = new();
      public readonly List<int> Levels = new();
    }

    private static readonly Dictionary<string, TechSteps> _techSteps = new();
    private static HashSet<int> _eventIds;

    internal static TechSteps TechStepsFor(string facName)
    {
      if (_techSteps.ContainsKey(facName))
      {
        return _techSteps[facName];
      }

      if (_eventIds == null)
      {
        _eventIds = new HashSet<int>();
        for (var i = 0; i < AiUpgrades.Events.Count; i++)
        {
          _eventIds.Add(AiUpgrades.Events[i].ResearchId);
        }
      }

      var ids = new List<int>();
      AddPoolIds(ids, AiUpgrades.ByFaction, facName);
      AddPoolIds(ids, AiUpgrades.Extra, facName);
      var st = new TechSteps();
      for (var lv = 1; lv <= 3; lv++)
      {
        for (var i = 0; i < ids.Count; i++)
        {
          var max = AiUpgrades.MaxLevel.ContainsKey(ids[i]) ? AiUpgrades.MaxLevel[ids[i]] : 1;
          if (lv <= max)
          {
            st.Ids.Add(ids[i]);
            st.Levels.Add(lv);
          }
        }
      }

      _techSteps[facName] = st;
      return st;
    }

    private static void AddPoolIds(List<int> into, Dictionary<string, List<int>> src, string facName)
    {
      if (!src.TryGetValue(facName, out var ups) || ups == null)
      {
        return;
      }

      for (var i = 0; i < ups.Count; i++)
      {
        var id = ups[i];
        if (!_eventIds.Contains(id) && !AiUpgrades.NeverGrant.Contains(id) && !into.Contains(id))
        {
          into.Add(id);
        }
      }
    }
    private static readonly Dictionary<int, int> _miscIdx = new();

    internal static void SpendBuildingCredit(player p, int pid, unit hall, unit other)
    {
      var spent = SimpleBot._bSpent.ContainsKey(pid) ? SimpleBot._bSpent[pid] : 0;
      if (TryFreeUpgrade(p, hall, spent) || TryFreeUpgrade(p, other, spent))
      {
        SimpleBot._bSpent[pid] = spent + 1;
        return;
      }

      var mi = _miscIdx.ContainsKey(pid) ? _miscIdx[pid] : 0;
      if (mi < AiUpgrades.MiscAll.Count)
      {
        SetPlayerTechResearched(p, AiUpgrades.MiscAll[mi], 1);
        _miscIdx[pid] = mi + 1;
        SimpleBot._bSpent[pid] = spent + 1;
        SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(p) + ": misc tech granted (" + I2S(AiUpgrades.MiscAll[mi]) + ")");
      }
    }

    private static bool TryFreeUpgrade(player p, unit b, int salt)
    {
      if (b == null)
      {
        return false;
      }

      var opts = AiUpgrades.BuildingUpgrades[GetUnitTypeId(b)];
      for (var k = 0; k < opts.Count; k++)
      {
        var target = opts[(salt + k) % opts.Count]; // vary the branch (e.g. which demon gate) between upgrades
        if (FreeOrder(p, b, target))
        {
          SimpleBot.Log("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": building upgrade " + GetUnitName(b) + " -> "
              + GetObjectName(target));
          return true;
        }
      }

      return false;
    }

    // Issue an order that costs resources for free: top up, order, restore. Returns WC3's accept/refuse.
    private static bool FreeOrder(player p, unit u, int orderId)
    {
      var gold = GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD);
      var wood = GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, gold + 10000);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, wood + 10000);
      var ok = IssueImmediateOrderById(u, orderId);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, gold);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, wood);
      return ok;
    }

    // Event research: if this building researches an event the bot doesn't have yet, order it. WC3 itself refuses
    // the order while the research's requirements aren't met ("only when they can"); once accepted it is free --
    // gold/lumber are topped up for the order and restored right after, so the research costs nothing.
    private static readonly Dictionary<int, float> _eventTried = new(); // key: pid*100 + event index

    internal static void TryEventResearch(player p, unit building, int typeId)
    {
      var evs = AiUpgrades.Events;
      for (var i = 0; i < evs.Count; i++)
      {
        var ev = evs[i];
        if (ev.BuildingId != typeId || SimpleBot._clock < ev.NotBefore || GetPlayerTechCount(p, ev.ResearchId, true) > 0)
        {
          continue;
        }

        var key = GetPlayerId(p) * 100 + i;
        if (_eventTried.ContainsKey(key) && SimpleBot._clock - _eventTried[key] < 60f)
        {
          continue; // accepted recently (research in progress) or refused recently
        }

        _eventTried[key] = SimpleBot._clock;
        if (FreeOrder(p, building, ev.ResearchId))
        {
          SimpleBot.Log("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": event research ordered (" + I2S(ev.ResearchId) + ")");
        }
      }
    }

    // v34: quest building upgrades -- free, as soon as WC3 accepts (requirements met), retried every 60 s.
    private static readonly Dictionary<unit, float> _questUpTried = new(); // per building (only looked up, never iterated)

    internal static void TryQuestUpgrade(player p, unit building, int typeId)
    {
      if (!AiUpgrades.QuestUpgrades.ContainsKey(typeId))
      {
        return;
      }

      if (_questUpTried.ContainsKey(building) && SimpleBot._clock - _questUpTried[building] < 60f)
      {
        return;
      }

      _questUpTried[building] = SimpleBot._clock;
      if (FreeOrder(p, building, AiUpgrades.QuestUpgrades[typeId]))
      {
        SimpleBot.Log("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": quest upgrade " + GetUnitName(building) + " ordered");
      }
    }
    private static unit _lastCapUnit;
    private static player _lastCapOwner;

    internal static void SetupCaptureCounting()
    {
      var t = CreateTrigger();
      for (var i = 0; i < 28; i++)
      {
        TriggerRegisterPlayerUnitEvent(t, Player(i), EVENT_PLAYER_UNIT_CHANGE_OWNER, null);
      }

      TriggerAddAction(t, OnUnitChangeOwner);
    }

    private static void OnUnitChangeOwner()
    {
      if (SimpleBot._suppressCaptureCount)
      {
        return;
      }

      var u = GetTriggerUnit();
      var owner = GetOwningPlayer(u);
      if (u == _lastCapUnit && owner == _lastCapOwner)
      {
        return;
      }

      if (!SimpleBot.IsBot(owner) || !ControlPointManager.Instance.UnitIsControlPoint(u))
      {
        return;
      }

      _lastCapUnit = u;
      _lastCapOwner = owner;
      var pid = GetPlayerId(owner);
      SimpleBot._cpCaptures[pid] = (SimpleBot._cpCaptures.ContainsKey(pid) ? SimpleBot._cpCaptures[pid] : 0) + 1;
    }
  }
}
