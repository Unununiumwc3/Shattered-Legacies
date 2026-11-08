using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Shared;

namespace WarcraftLegacies.Source.Ai
{
  // Regular bot construction. Never creates a building: the worker issues a real Warcraft III build order.
  // Projects reserve their workers, types and sites until completion or failure.
  // PlaceProduction / the old starter kit are historical and no longer run on the scheduler.
  internal sealed class AiBuildProject
  {
    internal unit Worker;
    internal int Type;
    internal float X;
    internal float Y;
    internal float OrderedAt;
    internal bool NewBase; // only the forward town hall creates a new base
    internal unit Building;
    internal float StartedAt;
    internal bool Subsidized;
    internal int TravelGold;
    internal int TravelLumber;
  }

  internal sealed class AiBuildBase
  {
    internal float X;
    internal float Y;
    internal float CreatedAt;
  }

  internal sealed class AiBuildCandidates
  {
    internal float AnchorX;
    internal float AnchorY;
    internal int Component;
    internal int HalfWidth;
    internal bool Shore;
    internal readonly List<float> X = new();
    internal readonly List<float> Y = new();
  }

  internal sealed class AiBuildState
  {
    internal Faction CurrentFaction;
    internal unit Upgrading;
    internal int MorphFrom;
    internal bool HasWorkers;
    internal int CancelGold;
    internal int CancelWood;
    internal player Owner;
    internal string Faction = "";
    internal bool HomeSet;
    internal int HallType; // observed native town-hall type; survives hall loss, reset at faction change
    internal float HomeX;
    internal float HomeY;
    internal float NextPlan;
    internal float BadX;
    internal float BadY;
    internal float BadUntil;
    internal readonly List<AiBuildProject> Projects = new();
    internal readonly Dictionary<unit, AiBuildCandidates> Anchors = new();
    internal readonly List<AiBuildCandidates> Sites = new();
    internal int SearchPhase;
    internal int OrdersThisVisit;
    internal int ProbesThisVisit;
    internal bool Established;
    internal bool Shore;
    internal string Blocked = "";
    internal readonly List<AiBuildBase> Forward = new();
    internal readonly Dictionary<int, float> UnbuildableUntil = new();
    internal float NextExpansion;
    internal string LastReason = "";
    internal float NextReasonLog;
    internal string LastBuildFailure = "";
  }

  internal static class AiBuilder
  {
    private const float PlanInterval = 12f; // per bot, not per scheduler tick
    private const float ProjectTimeout = 120f; // long journeys get a chance; cancelled idle orders fail early
    private const float ConstructionTimeout = 600f; // or to finish after start
    private const float RejectedCooldown = 30f;
    private const int WantedWorkers = 2;
    private const float ExpansionInterval = 180f;
    private const int MaxBases = 3;             // including the original HQ, NOT three extra bases
    private static readonly AiBuildState[] States = new AiBuildState[24];
    private static readonly UnitCategory[] ProducerCats =
    {
      UnitCategory.Barracks, UnitCategory.SiegeWorkshop, UnitCategory.Magic,
      UnitCategory.FlyingBuilding, UnitCategory.Specialist,
    };
    internal static int SiteProbes;
    internal static int BuildOrders;
    private static int _cursor;
    private static readonly List<player> Ready = new();
    private static bool _serveReady;

    internal static void Setup()
    {
      // Native construction events are authoritative: an accepted order is not a finished building.
      // Register once, for all slots, including later faction takeovers.
      var begun = CreateTrigger();
      var finished = CreateTrigger();
      var cancelled = CreateTrigger();
      var changed = CreateTrigger();
      var trained = CreateTrigger();
      var unqueued = CreateTrigger();
      var revived = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        var p = Player(i);
        TriggerRegisterPlayerUnitEvent(unqueued, p, EVENT_PLAYER_UNIT_TRAIN_CANCEL, null);
        TriggerRegisterPlayerUnitEvent(revived, p, EVENT_PLAYER_HERO_REVIVE_FINISH, null);
        TriggerRegisterPlayerUnitEvent(revived, p, EVENT_PLAYER_HERO_REVIVE_CANCEL, null);
        TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
        TriggerRegisterPlayerUnitEvent(changed, p, EVENT_PLAYER_UNIT_UPGRADE_CANCEL, null);
        TriggerRegisterPlayerUnitEvent(changed, p, EVENT_PLAYER_UNIT_UPGRADE_FINISH, null);
        TriggerRegisterPlayerUnitEvent(changed, p, EVENT_PLAYER_UNIT_RESEARCH_FINISH, null);
        TriggerRegisterPlayerUnitEvent(changed, p, EVENT_PLAYER_HERO_REVIVABLE, null);
        TriggerRegisterPlayerUnitEvent(begun, p, EVENT_PLAYER_UNIT_CONSTRUCT_START, null);
        TriggerRegisterPlayerUnitEvent(finished, p, EVENT_PLAYER_UNIT_CONSTRUCT_FINISH, null);
        TriggerRegisterPlayerUnitEvent(cancelled, p, EVENT_PLAYER_UNIT_CONSTRUCT_CANCEL, null);
      }
      TriggerAddAction(unqueued, () => SimpleBot.EndHeroOrder(GetTriggerPlayer(), GetTrainedUnitType()));
      TriggerAddAction(revived, () =>
      {
        var hero = GetRevivingUnit();
        SimpleBot.EndHeroOrder(GetOwningPlayer(hero), GetUnitTypeId(hero));
        Request(GetOwningPlayer(hero));
      });
      TriggerAddAction(trained, () =>
      {
        var u = GetTrainedUnit();
        if (IsUnitType(u, UNIT_TYPE_HERO))
        {
          SimpleBot.EndHeroOrder(GetOwningPlayer(u), GetUnitTypeId(u));
          Request(GetOwningPlayer(u));
        }
        else if (AiBuildData.Menus.ContainsKey(GetUnitTypeId(u)))
        {
          Request(GetOwningPlayer(u));
        }
      });
      TriggerAddAction(changed, () => Request(GetTriggerPlayer()));
      TriggerAddAction(begun, OnConstructionStart);
      TriggerAddAction(finished, OnConstructionFinish);
      TriggerAddAction(cancelled, OnConstructionCancel);

      // One service per half-second. Alternate pending event work with the fair round-robin.
      // Kept separate from the faster production/combat slot where possible.
      AiScheduler.Add("builder", 2, 1, Tick, true);
    }

    private static AiBuildProject ProjectForEvent(unit building, out AiBuildState state)
    {
      state = null;
      if (building == null)
      {
        return null;
      }
      var id = GetPlayerId(GetOwningPlayer(building));
      if (id < 0 || id >= States.Length || States[id] == null)
      {
        return null;
      }
      state = States[id];
      for (var i = 0; i < state.Projects.Count; i++)
      {
        var pr = state.Projects[i];
        if (pr.Building == building)
        {
          return pr;
        }
        if (pr.Building == null && pr.Type == GetUnitTypeId(building))
        {
          var dx = pr.X - GetUnitX(building);
          var dy = pr.Y - GetUnitY(building);
          if (dx * dx + dy * dy <= 128f * 128f)
          {
            return pr;
          }
        }
      }
      return null;
    }

    private static void OnConstructionStart()
    {
      var building = GetConstructingStructure();
      var pr = ProjectForEvent(building, out var state);
      if (pr != null && pr.Building == null)
      {
        Started(pr, building);
        Request(state.Owner);
        SimpleBot.Log("builder: " + state.Faction + " started " + GetObjectName(pr.Type));
      }
    }

    private static void Started(AiBuildProject pr, unit building)
    {
      pr.Building = building;
      pr.StartedAt = SimpleBot.Clock;
      // WC3 has now charged any resources left for arrival.
      pr.TravelGold = 0;
      pr.TravelLumber = 0;
    }

    internal static int Reserved(player p, bool gold)
    {
      var s = States[GetPlayerId(p)];
      if (s == null)
      {
        return 0;
      }
      var amount = gold ? s.CancelGold : s.CancelWood;
      for (var i = 0; i < s.Projects.Count; i++)
      {
        amount += gold ? s.Projects[i].TravelGold : s.Projects[i].TravelLumber;
      }
      return amount;
    }

    internal static bool CanBuy(player p, int type)
    {
      return GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) - Reserved(p, true) >= GetUnitGoldCost(type)
          && GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) - Reserved(p, false) >= GetUnitWoodCost(type);
    }

    internal static bool NeedsHallQueue(player p, unit hall)
    {
      var s = States[GetPlayerId(p)];
      return s == null || !s.HasWorkers || AiBuildPlan.NextHallTier(p, hall) != 0;
    }

    private static void CollectCancelRefund(AiBuildState s)
    {
      // The map refunds 100% on native construction cancellation. Collect on the
      // next visit, after the engine has credited it, independently of event ordering.
      if (s.CancelGold != 0 || s.CancelWood != 0)
      {
        var p = s.Owner;
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) - s.CancelGold);
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) - s.CancelWood);
        s.CancelGold = 0;
        s.CancelWood = 0;
      }
    }

    private static void ReleaseTravelFunds(player p, AiBuildProject pr)
    {
      if (pr.TravelGold != 0 || pr.TravelLumber != 0)
      {
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) - pr.TravelGold);
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) - pr.TravelLumber);
        pr.TravelGold = 0;
        pr.TravelLumber = 0;
      }
    }

    private static void OnConstructionFinish()
    {
      var building = GetConstructedStructure();
      Request(GetOwningPlayer(building));
      var pr = ProjectForEvent(building, out var s);
      if (pr == null)
      {
        return;
      }
      s.Established = true;
      if (pr.NewBase)
      {
        var b = new AiBuildBase();
        b.X = GetUnitX(building);
        b.Y = GetUnitY(building);
        b.CreatedAt = SimpleBot.Clock;
        s.Forward.Add(b);
      }
      SimpleBot.LogPin("builder: " + s.Faction + " completed " + GetObjectName(pr.Type));
      s.Projects.Remove(pr);
      s.NextPlan = 0f;
    }

    private static void OnConstructionCancel()
    {
      var building = GetCancelledStructure();
      var pr = ProjectForEvent(building, out var s);
      if (pr != null)
      {
        if (pr.Subsidized && pr.Building != null)
        {
          s.CancelGold += GetUnitGoldCost(pr.Type);
          s.CancelWood += GetUnitWoodCost(pr.Type);
        }
        Failed(s, pr, "cancelled");
      }
    }

    // A late faction requests an early evaluation, instead of receiving a magically completed barracks.
    internal static void Request(player p)
    {
      var id = GetPlayerId(p);
      if (id >= 0 && id < States.Length && States[id] != null && SimpleBot.IsBot(p))
      {
        States[id].NextPlan = 0f;
        if (!Ready.Contains(p))
        {
          Ready.Add(p);
        }
      }
    }

    private static void OnQuestChanged(FactionQuestProgressChangedEventArgs change)
    {
      if (change.Faction.Player != null)
      {
        Request(change.Faction.Player);
      }
    }

    private static void ResetProjects(AiBuildState s)
    {
      while (s.Projects.Count > 0)
      {
        var pr = s.Projects[0];
        s.Projects.RemoveAt(0);
        if (pr.Building == null)
        {
          if (Alive(pr.Worker, s.Owner))
          {
            IssueImmediateOrder(pr.Worker, "stop");
          }
          ReleaseTravelFunds(s.Owner, pr);
        }
      }
    }

    internal static void Release(player p)
    {
      var id = GetPlayerId(p);
      var s = States[id];
      if (s != null)
      {
        ResetProjects(s);
        CollectCancelRefund(s);
        if (s.CurrentFaction != null)
        {
          s.CurrentFaction.QuestProgressChanged -= OnQuestChanged;
        }
        States[id] = null;
      }
      Ready.Remove(p);
    }

    private static void Tick()
    {
      if (SimpleBot.BotBreak || !SimpleBot.ProdOn || SimpleBot.Bots.Count == 0)
      {
        return;
      }
      if (_cursor >= SimpleBot.Bots.Count)
      {
        _cursor = 0;
      }
      player p;
      _serveReady = !_serveReady;
      if (_serveReady && Ready.Count > 0)
      {
        p = Ready[0];
        Ready.RemoveAt(0);
        if (!SimpleBot.IsBot(p))
        {
          return;
        }
      }
      else
      {
        p = SimpleBot.Bots[_cursor++];
        Ready.Remove(p);
      }
      var id = GetPlayerId(p);
      var fac = p.GetPlayerData().Faction;
      if (id < 0 || id >= 24 || fac == null)
      {
        return;
      }
      var s = States[id];
      if (s == null)
      {
        s = new AiBuildState();
        s.Owner = p;
        States[id] = s;
      }
      CollectCancelRefund(s);
      if (s.CurrentFaction != fac || s.Faction != fac.Name)
      {
        // A fallback/split can reuse a player slot. Cancel the old faction's outstanding order.
        ResetProjects(s);
        if (s.CurrentFaction != null)
        {
          s.CurrentFaction.QuestProgressChanged -= OnQuestChanged;
        }
        s.CurrentFaction = fac;
        fac.QuestProgressChanged += OnQuestChanged;
        s.Upgrading = null;
        s.Faction = fac.Name;
        s.Forward.Clear();
        s.UnbuildableUntil.Clear();
        s.HomeSet = false;
        s.Established = false;
        s.HallType = 0;
        s.BadUntil = 0f;
        s.NextPlan = 0f;
        s.NextExpansion = 0f;
        s.LastReason = "";
        s.NextReasonLog = 0f;
        s.LastBuildFailure = "";
      }
      for (var i = s.Projects.Count - 1; i >= 0; i--)
      {
        CheckProject(p, s, s.Projects[i]);
      }
      if (SimpleBot.Clock < s.NextPlan)
      {
        return;
      }
      s.NextPlan = SimpleBot.Clock + PlanInterval;
      s.Sites.Clear();
      s.Anchors.Clear();
      s.ProbesThisVisit = 0;
      s.OrdersThisVisit = 0;
      s.Blocked = "";
      s.SearchPhase = (s.SearchPhase + 1) % 16;
      Evaluate(p, fac, s);
    }

    private static void CheckProject(player p, AiBuildState s, AiBuildProject pr)
    {
      if (pr.Building == null)
      {
        // Fallback for a build that started before its trigger callback was observed.
        var building = AiBuildSites.StructureNear(p, pr.Type, pr.X, pr.Y, 128f);
        if (building != null)
        {
          Started(pr, building);
        }
        else if (!Alive(pr.Worker, p))
        {
          Failed(s, pr, "worker missing");
        }
        else if (SimpleBot.Clock - pr.OrderedAt >= 8f && !IsUnitPaused(pr.Worker) && GetUnitCurrentOrder(pr.Worker) == 0)
        {
          // Warcraft III can return 'accepted' and drop an impossible build order on the next frame.
          Failed(s, pr, "worker idle before construction");
        }
        else if (SimpleBot.Clock - pr.OrderedAt > ProjectTimeout)
        {
          Failed(s, pr, "never started / unreachable site");
        }
        return;
      }
      if (!Alive(pr.Building, p) || SimpleBot.Clock - pr.StartedAt > ConstructionTimeout)
      {
        Failed(s, pr, "building destroyed / construction timed out");
      }
      // Otherwise wait for EVENT_PLAYER_UNIT_CONSTRUCT_FINISH; do not re-order a busy worker.
    }

    private static void Failed(AiBuildState s, AiBuildProject pr, string why)
    {
      if (pr.Building == null && Alive(pr.Worker, s.Owner) && why == "never started / unreachable site")
      {
        IssueImmediateOrder(pr.Worker, "stop");
      }
      s.BadX = pr.X;
      s.BadY = pr.Y;
      s.BadUntil = SimpleBot.Clock + RejectedCooldown;
      if (why == "worker idle before construction")
      {
        // The native call may have accepted an unavailable build-menu order.
        // Try another producer type on the next planning pass.
        s.UnbuildableUntil[pr.Type] = SimpleBot.Clock + RejectedCooldown;
      }
      var p = s.Owner;
      ReleaseTravelFunds(p, pr);
      SimpleBot.Log("builder: " + s.Faction + " hall=" + GetObjectName(s.HallType) + " gold=" +
        I2S(GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD)) + " wood=" + I2S(GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER)));
      Request(p);
      SimpleBot.Log("builder: " + s.Faction + " retry " + GetObjectName(pr.Type) + " (" + why + ")");
      s.Projects.Remove(pr);
      s.NextPlan = 0f;
    }

    private static void Explain(player p, AiBuildState s, string reason)
    {
      if (reason == s.LastReason && SimpleBot.Clock < s.NextReasonLog)
      {
        return;
      }
      s.LastReason = reason;
      s.NextReasonLog = SimpleBot.Clock + 180f;
      SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " idle: " + reason);
    }

    private static bool Alive(unit u, player p)
    {
      return u != null && GetUnitTypeId(u) != 0 && GetOwningPlayer(u) == p
             && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;
    }

    private static void Evaluate(player p, Faction fac, AiBuildState s)
    {
      var builderTypes = new List<int>();
      SimpleBot.AddCategory(fac, UnitCategory.Builder, builderTypes);
      // Scourge also categorizes lumber ghouls as builders; they cannot construct.
      for (var i = builderTypes.Count - 1; i >= 0; i--)
      {
        if (!AiBuildData.Menus.ContainsKey(builderTypes[i]) || p.GetPlayerData().GetObjectLimit(builderTypes[i]) <= 0)
        {
          builderTypes.RemoveAt(i);
        }
      }
      // Do not return here: some custom factions have native peon-flagged workers but no
      // corresponding Builder category. Scan recognizes those workers as a fallback.
      var workers = new List<unit>();
      var halls = new List<unit>();
      var hallTypes = new List<int>();
      // Faction categories are the fallback if the starting HQ was already destroyed.
      SimpleBot.AddCategory(fac, UnitCategory.TownHall, hallTypes);
      SimpleBot.AddCategory(fac, UnitCategory.Keep, hallTypes);
      SimpleBot.AddCategory(fac, UnitCategory.Castle, hallTypes);
      if (s.HallType != 0 && !SimpleBot.ListHas(hallTypes, s.HallType))
      {
        hallTypes.Add(s.HallType);
      }
      var altars = new List<unit>();
      var structures = new List<unit>();
      Scan(p, builderTypes, hallTypes, workers, halls, altars, structures);
      s.HasWorkers = workers.Count > 0;
      // WC3's town-hall flag is authoritative. Remember its actual rawcode even after it is lost;
      // a faction with no observed hall cannot safely guess a buildable hall from upgrade categories.
      if (halls.Count > 0)
      {
        s.HallType = GetUnitTypeId(halls[0]);
        if (!SimpleBot.ListHas(hallTypes, s.HallType))
        {
          hallTypes.Add(s.HallType);
        }
      }
      if (!s.HomeSet)
      {
        var initial = workers.Count > 0 ? workers[0] : (halls.Count > 0 ? halls[0] : null);
        if (initial != null)
        {
          s.HomeX = GetUnitX(initial);
          s.HomeY = GetUnitY(initial);
        }
        else
        {
          var start = GetPlayerStartLocation(p);
          s.HomeX = GetStartLocationX(start);
          s.HomeY = GetStartLocationY(start);
        }
        s.HomeSet = true;
      }
      if (SimpleBot.HasPerks(p))
      {
        if (workers.Count > 0)
        {
          var local = 0;
          var nearest = -1f;
          for (var h = 0; h < halls.Count; h++)
          {
            var dx = GetUnitX(halls[h]) - s.HomeX;
            var dy = GetUnitY(halls[h]) - s.HomeY;
            var dist = dx * dx + dy * dy;
            if (nearest < 0f || dist < nearest)
            {
              nearest = dist;
              local = h;
            }
          }
          if (halls.Count > 0)
          {
            s.HallType = GetUnitTypeId(halls[local]);
          }
          for (var i = 0; i < halls.Count; i++)
          {
            if (AiBuildPlan.UpgradeHall(p, halls[(local + i) % halls.Count]))
            {
              break;
            }
          }
        }
        for (var i = 0; i < altars.Count; i++)
        {
          SimpleBot.TrainHeroes(altars[i], fac, p);
        }
      }
      if (workers.Count == 0 && builderTypes.Count == 0)
      {
        Explain(p, s, "no Builder category and no native worker found");
        return;
      }
      if (s.Projects.Count >= WantedWorkers)
      {
        return;
      }
      s.Shore = false;
      var trained = false;
      if (workers.Count < WantedWorkers)
      {
        trained = TrainWorker(p, builderTypes, halls);
      }
      if (workers.Count == 0 && s.Projects.Count == 0)
      {
        // Perked bot with no viable training order: one emergency worker, not a free building.
        // Never do this while a town hall has a worker queued.
        if (!trained && SimpleBot.HasPerks(p))
        {
          var x = s.HomeX;
          var y = s.HomeY;
          if (AiBuildSites.EmergencyAnchor(p, x, y, out var ax, out var ay))
          {
            x = ax;
            y = ay;
          }
          var w = CreateUnit(p, builderTypes[0], x, y, 270f);
          if (w != null)
          {
            SimpleBot.LogPin("builder: " + SimpleBot.FacName(p) + " stranded -> one emergency worker");
          }
          s.NextPlan = w != null ? 0f : SimpleBot.Clock + 30f;
        }
        Explain(p, s, trained ? "no worker; town hall training/queued" : "no worker; no accepted train order");
        return;
      }
      for (var i = 0; i < s.Projects.Count; i++)
      {
        workers.Remove(s.Projects[i].Worker);
      }
      if (workers.Count == 0)
      {
        return;
      }
      // Enumeration order can put a distant worker ahead of one standing at the HQ.
      // Give the local worker the first attempt, but retain workers with different build menus.
      var closest = 0;
      var bestDistance = -1f;
      for (var i = 0; i < workers.Count; i++)
      {
        var dx = GetUnitX(workers[i]) - s.HomeX;
        var dy = GetUnitY(workers[i]) - s.HomeY;
        var distance = dx * dx + dy * dy;
        if (bestDistance < 0f || distance < bestDistance)
        {
          closest = i;
          bestDistance = distance;
        }
      }
      var first = workers[0];
      workers[0] = workers[closest];
      workers[closest] = first;
      // No buildable hall is present in some factions. Do not invent one.
      var localHallMissing = !s.Established && AiBuildSites.AnyHallNear(p, hallTypes, s.HomeX, s.HomeY, 1900f) == null;
      if ((halls.Count == 0 || localHallMissing) && hallTypes.Count > 0)
      {
        for (var i = 0; i < hallTypes.Count; i++)
        {
          if (Build(p, s, workers, hallTypes, hallTypes[i], false, false, 0f, 0f))
          {
            return;
          }
        }
        Explain(p, s, "no town hall; " + s.LastBuildFailure);
        // Other producers may still be recoverable if this faction cannot reconstruct a hall.
      }
      // Restore minimum troop production at the HQ before trying to expand. Includes altars after military sites.
      var all = ProductionTypes(fac);
      var types = AiBuildPlan.WorkerBuildings(p, workers, all, fac);
      var shipyards = new List<int>();
      SimpleBot.AddCategory(fac, UnitCategory.Shipyard, shipyards);
      var productionHalls = 0;
      for (var h = 0; h < halls.Count; h++)
      {
        if (AiBuildData.Products.ContainsKey(GetUnitTypeId(halls[h])))
        {
          productionHalls++;
        }
      }
      var missing = 0;
      for (var wanted = 1; wanted <= 2; wanted++)
      {
        if (wanted == 2 && s.Projects.Count > 0)
        {
          return; // finish first copies before committing another worker to duplicates
        }
        for (var i = 0; i < types.Count; i++)
        {
          var type = types[i];
          var hall = SimpleBot.ListHas(hallTypes, type);
          var count = hall && AiBuildData.Products.ContainsKey(type) ? productionHalls : GetPlayerTechCount(p, type, true);
          if (count >= wanted || (wanted == 2 && !AiBuildData.Products.ContainsKey(type))
              || p.GetPlayerData().GetObjectLimit(type) < wanted)
          {
            continue;
          }
          s.Shore = shipyards.Contains(type);
          if (Build(p, s, workers, hallTypes, type, false, false, 0f, 0f))
          {
            return;
          }
          if (wanted == 1)
          {
            missing++;
          }
          if (s.OrdersThisVisit >= 8)
          {
            return;
          }
        }
        if (SimpleBot.HasPerks(p) && AiBuildPlan.UpgradeForCoverage(p, s, structures, hallTypes, wanted))
        {
          return;
        }
      }
      s.Shore = false;
      if (missing > 0)
      {
        Explain(p, s, "hall " + GetObjectName(s.HallType) + "; " +
          (s.Blocked != "" ? s.Blocked : s.LastBuildFailure));
      }
      if (s.Projects.Count > 0)
      {
        return; // serialize expansion planning so hall and local producer caps include completions
      }
      if (!SimpleBot.HasPerks(p) || hallTypes.Count == 0 || all.Count == 0)
      {
        if (missing == 0)
        {
          Explain(p, s, all.Count == 0 ? "no production types in faction+AiData" :
            "production satisfied; no perk-based expansion");
        }
        return; // fair autopilot performs recovery, but not free, CP-triggered bonus expansion
      }
      for (var i = s.Forward.Count - 1; i >= 0; i--)
      {
        var b = s.Forward[i];
        if (AiBuildSites.AnyHallNear(p, hallTypes, b.X, b.Y, 750f) == null)
        {
          s.Forward.RemoveAt(i);
        }
      }
      // Finish existing forward bases before opening another. Two producers TOTAL per base, not per type.
      for (var i = 0; i < s.Forward.Count; i++)
      {
        var b = s.Forward[i];
        if (SimpleBot.Clock - b.CreatedAt < 45f)
        {
          continue; // let the town hall begin construction first
        }
        var amount = AiBuildSites.ProducerCountNear(p, all, b.X, b.Y, 2600f);
        if (amount < 2)
        {
          // Try all types, not just one which this faction's workers may be unable to construct.
          for (var k = 0; k < all.Count; k++)
          {
            if (Build(p, s, workers, hallTypes, all[(amount + k) % all.Count], false, true, b.X, b.Y))
            {
              return;
            }
          }
          return;
        }
      }
      var ownedCps = p.GetPlayerData().ControlPoints.Count;
      var wantedBases = ownedCps >= 20 ? 3 : (ownedCps >= 10 ? 2 : 1);
      if (wantedBases > MaxBases)
      {
        wantedBases = MaxBases;
      }
      if (1 + s.Forward.Count >= wantedBases)
      {
        if (missing == 0)
        {
          Explain(p, s, "production satisfied; hall count " + I2S(halls.Count) +
            "/" + I2S(wantedBases) + " at " + I2S(ownedCps) + " CPs");
        }
        return;
      }
      if (SimpleBot.Clock < s.NextExpansion)
      {
        return;
      }
      s.NextExpansion = SimpleBot.Clock + ExpansionInterval;
      // AiRoutes.StagePoint: last friendly node on the attack route. Own/allied CP ~two walk edges back.
      if (AiBuildSites.ForwardAnchor(p, hallTypes, workers, s.Forward, out var fx, out var fy))
      {
        if (!Build(p, s, workers, hallTypes, Cheapest(hallTypes), true, true, fx, fy))
        {
          Explain(p, s, "forward hall blocked; " + s.LastBuildFailure);
        }
      }
      else
      {
        Explain(p, s, "no reachable safe forward CP");
      }
    }

    private static void Scan(player p, List<int> builderTypes, List<int> hallTypes, List<unit> workers, List<unit> halls, List<unit> altars, List<unit> structures)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.ProdOps++;
        if (!Alive(u, p))
        {
          continue;
        }
        if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
        {
          structures.Add(u);
        }
        if (!IsUnitType(u, UNIT_TYPE_STRUCTURE) && AiBuildData.Menus.ContainsKey(GetUnitTypeId(u)) &&
            (SimpleBot.ListHas(builderTypes, GetUnitTypeId(u)) || IsUnitType(u, UNIT_TYPE_PEON)))
        {
          workers.Add(u);
        }
        else if (IsUnitType(u, UNIT_TYPE_TOWNHALL) || SimpleBot.ListHas(hallTypes, GetUnitTypeId(u)))
        {
          halls.Add(u);
        }
        else if (AiData.AltarHeroes.ContainsKey(GetUnitTypeId(u)) || GetUnitAbilityLevel(u, FourCC("Arev")) > 0)
        {
          altars.Add(u);
        }
      }
      DestroyGroup(g);
    }

    private static bool TrainWorker(player p, List<int> types, List<unit> halls)
    {
      var busyHall = false;
      for (var h = 0; h < halls.Count; h++)
      {
        // Never overwrite an active training / upgrade order.
        if (GetUnitCurrentOrder(halls[h]) != 0)
        {
          busyHall = true;
          continue; // another hall may be idle
        }
        for (var i = 0; i < types.Count; i++)
        {
          var accepted = SimpleBot.HasPerks(p) ? AiTech.FreeOrder(p, halls[h], types[i])
                        : IssueImmediateOrderById(halls[h], types[i]);
          if (accepted)
          {
            SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " training " + GetObjectName(types[i]));
            return true;
          }
        }
      }
      return busyHall;
    }

    private static List<int> ProductionTypes(Faction f)
    {
      var types = new List<int>();
      for (var c = 0; c < ProducerCats.Length; c++)
      {
        var cat = new List<int>();
        SimpleBot.AddCategory(f, ProducerCats[c], cat);
        for (var i = 0; i < cat.Count; i++)
        {
          if (AiData.Trains.ContainsKey(cat[i]) && !SimpleBot.ListHas(types, cat[i]))
          {
            types.Add(cat[i]);
          }
        }
      }
      return types;
    }

    private static int Cheapest(List<int> types)
    {
      var best = 0;
      for (var i = 0; i < types.Count; i++)
      {
        if (best == 0 || GetUnitGoldCost(types[i]) < GetUnitGoldCost(best))
        {
          best = types[i];
        }
      }
      return best;
    }

    private static bool Build(player p, AiBuildState s, List<unit> workers, List<int> hallTypes, int type, bool expansion,
      bool fixedAnchor, float fixedX, float fixedY)
    {
      s.LastBuildFailure = "";
      var need = AiBuildPlan.Missing(p, type);
      if (need != 0)
      {
        s.Blocked = GetObjectName(type) + " needs " + GetObjectName(need);
        s.LastBuildFailure = s.Blocked;
        return false;
      }
      if (type == 0 || p.GetPlayerData().GetObjectLimit(type) <= 0)
      {
        s.LastBuildFailure = "unknown building type";
        return false;
      }
      if (s.UnbuildableUntil.ContainsKey(type) && SimpleBot.Clock < s.UnbuildableUntil[type])
      {
        s.LastBuildFailure = GetObjectName(type) + " on cooldown";
        return false;
      }
      for (var i = 0; i < s.Projects.Count; i++)
      {
        if (s.Projects[i].Type == type
            || (SimpleBot.ListHas(hallTypes, type) && SimpleBot.ListHas(hallTypes, s.Projects[i].Type)))
        {
          s.LastBuildFailure = GetObjectName(type) + " already ordered";
          return false;
        }
      }
      var sawWorker = false;
      var sawAnchor = false;
      var sawSite = false;
      var rejectedOrders = 0;
      // Try each worker; a worker may have been carried across a sea or may have a different build menu.
      for (var w = 0; w < workers.Count; w++)
      {
        var worker = workers[w];
        if (!AiBuildPlan.OnMenu(worker, type))
        {
          continue;
        }
        sawWorker = true;
        float ax = fixedX, ay = fixedY;
        if (fixedAnchor)
        {
          if (AiRoutes.CompAt(GetUnitX(worker), GetUnitY(worker)) != AiRoutes.CompAt(ax, ay))
          {
            continue;
          }
        }
        else
        {
          if (!s.Anchors.TryGetValue(worker, out var anchor))
          {
            if (AiBuildSites.HomeAnchor(p, s.Established, hallTypes, s.HomeX, s.HomeY, worker, out ax, out ay))
            {
              anchor = new AiBuildCandidates();
              anchor.AnchorX = ax;
              anchor.AnchorY = ay;
            }
            s.Anchors[worker] = anchor;
          }
          if (anchor == null)
          {
            continue;
          }
          ax = anchor.AnchorX;
          ay = anchor.AnchorY;
        }
        sawAnchor = true;
        // Retry distinct sites immediately for a rejected worker order; do not require a second worker.
        var comp = AiRoutes.CompAt(GetUnitX(worker), GetUnitY(worker));
        var halfWidth = AiBuildSites.HalfWidth(type);
        AiBuildCandidates sites = null;
        for (var k = 0; k < s.Sites.Count; k++)
        {
          var cached = s.Sites[k];
          if (cached.AnchorX == ax && cached.AnchorY == ay && cached.Component == comp && cached.Shore == s.Shore
              && cached.HalfWidth == halfWidth)
          {
            sites = cached;
            break;
          }
        }
        if (sites == null)
        {
          sites = new AiBuildCandidates();
          sites.AnchorX = ax;
          sites.AnchorY = ay;
          sites.Component = comp;
          sites.Shore = s.Shore;
          sites.HalfWidth = halfWidth;
          AiBuildSites.FindSites(p, s, sites);
          s.Sites.Add(sites);
        }
        for (var attempt = 0; attempt < sites.X.Count && s.OrdersThisVisit < 8; attempt++)
        {
          var x = sites.X[attempt];
          var y = sites.Y[attempt];
          sawSite = true;
          // The grant covers the order and any outstanding cost at the site.
          var gold = GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD);
          var lumber = GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER);
          var bonus = SimpleBot.HasPerks(p);
          var costGold = GetUnitGoldCost(type);
          var costLumber = GetUnitWoodCost(type);
          if (bonus)
          {
            SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, gold + costGold);
            SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, lumber + costLumber);
          }
          s.OrdersThisVisit++;
          BuildOrders++;
          SimpleBot.ProdOps++;
          var pr = new AiBuildProject();
          pr.Worker = worker;
          pr.Type = type;
          pr.X = x;
          pr.Y = y;
          pr.OrderedAt = SimpleBot.Clock;
          pr.NewBase = expansion;
          pr.Subsidized = bonus;
          s.Projects.Add(pr);
          var accepted = IssueBuildOrderById(worker, type, x, y);
          // For accepted orders keep exactly the uncharged subsidy until start.
          // Observe gold and lumber independently (either cost can be zero).
          var heldGold = bonus ? GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) - gold : 0;
          var heldWood = bonus ? GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) - lumber : 0;
          if (bonus && !accepted)
          {
            SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, gold);
            SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, lumber);
          }
          if (!accepted)
          {
            s.Projects.Remove(pr);
            rejectedOrders++;
            continue;
          }
          if (pr.Building == null)
          {
            pr.TravelGold = heldGold;
            pr.TravelLumber = heldWood;
          }
          Request(p);
          SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " ordered " + GetObjectName(type)
                           + (fixedAnchor ? " forward" : " local") + " at " + I2S(R2I(x)) + "," + I2S(R2I(y)));
          return true;
        }
      }
      // A map-owned neutral structure near the HQ used to make Safe() reject every site.
      // Do not blacklist a building type when the underlying problem was simply the location.
      if (rejectedOrders > 0)
      {
        s.LastBuildFailure = GetObjectName(type) + " order rejected by WC3 (" + I2S(rejectedOrders) + ")";
        s.UnbuildableUntil[type] = SimpleBot.Clock + RejectedCooldown;
      }
      else if (!sawWorker)
      {
        s.LastBuildFailure = "no current worker menu for " + GetObjectName(type);
      }
      else if (!sawAnchor)
      {
        s.LastBuildFailure = "no reachable safe HQ/CP anchor for " + GetObjectName(type);
      }
      else if (!sawSite)
      {
        s.LastBuildFailure = "no open placement for " + GetObjectName(type);
      }
      return false;
    }
  }
}
