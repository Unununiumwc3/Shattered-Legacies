using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.Ai
{
  // Regular bot construction. Never creates a building: the worker issues a real Warcraft III build order.
  // One outstanding project per player prevents repeat orders while the worker is walking to its site.
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
    internal bool RefundOnStart;
    internal int RefundGold;
    internal int RefundLumber;
  }

  internal sealed class AiBuildBase
  {
    internal float X;
    internal float Y;
    internal float CreatedAt;
  }

  internal sealed class AiBuildState
  {
    internal string Faction = "";
    internal bool HomeSet;
    internal int HallType; // observed native town-hall type; survives hall loss, reset at faction change
    internal float HomeX;
    internal float HomeY;
    internal float NextPlan;
    internal float BadX;
    internal float BadY;
    internal float BadUntil;
    internal AiBuildProject Project;
    internal readonly List<AiBuildBase> Forward = new();
    internal readonly Dictionary<int, float> UnbuildableUntil = new();
    internal float NextExpansion;
  }

  internal static class AiBuilder
  {
    private const float PlanInterval = 60f;
    private const float ProjectTimeout = 180f; // worker has this long to start construction
    private const float ConstructionTimeout = 600f; // or to finish after start
    private const float RejectedCooldown = 180f;
    private const int WantedWorkers = 2;
    private const float ExpansionInterval = 180f;
    private const int MaxBases = 3;             // including the original HQ, NOT three extra bases
    private static readonly AiBuildState[] States = new AiBuildState[24];
    private static readonly UnitCategory[] ProducerCats =
    {
      UnitCategory.Barracks, UnitCategory.SiegeWorkshop, UnitCategory.Magic,
      UnitCategory.FlyingBuilding, UnitCategory.Specialist,
    };
    private static int _cursor;

    internal static void Setup()
    {
      // Native construction events are authoritative: an accepted order is not a finished building.
      // Register once, for all slots, including later faction takeovers.
      var begun = CreateTrigger();
      var finished = CreateTrigger();
      var cancelled = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        var p = Player(i);
        TriggerRegisterPlayerUnitEvent(begun, p, EVENT_PLAYER_UNIT_CONSTRUCT_START, null);
        TriggerRegisterPlayerUnitEvent(finished, p, EVENT_PLAYER_UNIT_CONSTRUCT_FINISH, null);
        TriggerRegisterPlayerUnitEvent(cancelled, p, EVENT_PLAYER_UNIT_CONSTRUCT_CANCEL, null);
      }
      TriggerAddAction(begun, OnConstructionStart);
      TriggerAddAction(finished, OnConstructionFinish);
      TriggerAddAction(cancelled, OnConstructionCancel);

      // One bot every 2 seconds; a 16-bot game revisits each pending order about every 32 seconds.
      AiScheduler.Add("builder", 8, 5, Tick, true);
    }

    private static AiBuildState StateForEvent(unit building)
    {
      if (building == null)
      {
        return null;
      }
      var id = GetPlayerId(GetOwningPlayer(building));
      if (id < 0 || id >= States.Length)
      {
        return null;
      }
      var s = States[id];
      var pr = s != null ? s.Project : null;
      if (pr == null)
      {
        return null;
      }
      if (pr.Building == building)
      {
        return s; // cancellation can invalidate the unit's rawcode before the event callback
      }
      if (pr.Type != GetUnitTypeId(building))
      {
        return null;
      }
      var dx = pr.X - GetUnitX(building);
      var dy = pr.Y - GetUnitY(building);
      return dx * dx + dy * dy <= 450f * 450f ? s : null;
    }

    private static void OnConstructionStart()
    {
      var building = GetConstructingStructure();
      var s = StateForEvent(building);
      if (s != null && s.Project.Building == null)
      {
        Started(s.Project, building);
        SimpleBot.Log("builder: started " + GetObjectName(s.Project.Type));
      }
    }

    private static void Started(AiBuildProject pr, unit building)
    {
      pr.Building = building;
      pr.StartedAt = SimpleBot.Clock;
      if (pr.RefundOnStart)
      {
        // Some WC3 builds charge at the construction site rather than at order acceptance.
        // Refund only if the temporary order subsidy was NOT consumed when the order was issued.
        var p = GetOwningPlayer(building);
        SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD,
          GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + pr.RefundGold);
        SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER,
          GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) + pr.RefundLumber);
        pr.RefundOnStart = false;
      }
    }

    private static void OnConstructionFinish()
    {
      var building = GetConstructedStructure();
      var s = StateForEvent(building);
      if (s == null)
      {
        return;
      }
      var pr = s.Project;
      if (pr.NewBase)
      {
        var b = new AiBuildBase();
        b.X = GetUnitX(building);
        b.Y = GetUnitY(building);
        b.CreatedAt = SimpleBot.Clock;
        s.Forward.Add(b);
      }
      SimpleBot.LogPin("builder: completed " + GetObjectName(pr.Type));
      s.Project = null;
      s.NextPlan = SimpleBot.Clock + 15f;
    }

    private static void OnConstructionCancel()
    {
      var building = GetCancelledStructure();
      var s = StateForEvent(building);
      if (s != null)
      {
        Failed(s, "cancelled");
      }
    }

    // A late faction requests an early evaluation, instead of receiving a magically completed barracks.
    internal static void Request(player p)
    {
      var id = GetPlayerId(p);
      if (id >= 0 && id < States.Length && States[id] != null)
      {
        States[id].NextPlan = 0f;
      }
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
      var p = SimpleBot.Bots[_cursor++];
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
        States[id] = s;
      }
      if (s.Faction != fac.Name)
      {
        // A fallback/split can reuse a player slot. Cancel the old faction's outstanding order.
        if (s.Project != null && Alive(s.Project.Worker, p))
        {
          IssueImmediateOrder(s.Project.Worker, "stop");
        }
        s.Faction = fac.Name;
        s.Project = null;
        s.Forward.Clear();
        s.UnbuildableUntil.Clear();
        s.HomeSet = false;
        s.HallType = 0;
        s.BadUntil = 0f;
        s.NextPlan = 0f;
        s.NextExpansion = 0f;
      }
      if (s.Project != null)
      {
        CheckProject(p, s);
        return;
      }
      if (SimpleBot.Clock < s.NextPlan)
      {
        return;
      }
      s.NextPlan = SimpleBot.Clock + PlanInterval;
      Evaluate(p, fac, s);
    }

    private static void CheckProject(player p, AiBuildState s)
    {
      var pr = s.Project;
      if (pr.Building == null)
      {
        // Fallback for a build that started before its trigger callback was observed.
        var building = AiBuildSites.StructureNear(p, pr.Type, pr.X, pr.Y, 450f);
        if (building != null)
        {
          Started(pr, building);
        }
        else if (!Alive(pr.Worker, p) || SimpleBot.Clock - pr.OrderedAt > ProjectTimeout)
        {
          Failed(s, "worker missing / never started");
        }
        return;
      }
      if (!Alive(pr.Building, p) || SimpleBot.Clock - pr.StartedAt > ConstructionTimeout)
      {
        Failed(s, "building destroyed / construction timed out");
      }
      // Otherwise wait for EVENT_PLAYER_UNIT_CONSTRUCT_FINISH; do not re-order a busy worker.
    }

    private static void Failed(AiBuildState s, string why)
    {
      var pr = s.Project;
      if (pr == null)
      {
        return;
      }
      s.BadX = pr.X;
      s.BadY = pr.Y;
      s.BadUntil = SimpleBot.Clock + RejectedCooldown;
      SimpleBot.Log("builder: retry " + GetObjectName(pr.Type) + " (" + why + ")");
      s.Project = null;
      s.NextPlan = SimpleBot.Clock + 20f;
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
      if (builderTypes.Count == 0)
      {
        return; // special factions with no build-capable units retain their scripted map mechanics
      }
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
      Scan(p, builderTypes, hallTypes, workers, halls);
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
        var initial = halls.Count > 0 ? halls[0] : (workers.Count > 0 ? workers[0] : null);
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
      var trained = false;
      if (workers.Count < WantedWorkers)
      {
        trained = TrainWorker(p, builderTypes, halls);
      }
      if (workers.Count == 0)
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
          s.NextPlan = SimpleBot.Clock + 120f;
        }
        return;
      }
      // No buildable hall is present in some factions. Do not invent one.
      if (halls.Count == 0 && hallTypes.Count > 0)
      {
        for (var i = 0; i < hallTypes.Count; i++)
        {
          if (Build(p, s, workers, hallTypes, hallTypes[i], false, false, 0f, 0f))
          {
            return;
          }
        }
      }
      // Restore minimum troop production at the HQ before trying to expand. Includes altars after military sites.
      var all = ProductionTypes(fac);
      for (var i = 0; i < all.Count; i++)
      {
        if (GetPlayerTechCount(p, all[i], true) == 0)
        {
          if (Build(p, s, workers, hallTypes, all[i], false, false, 0f, 0f))
          {
            return;
          }
        }
      }
      var altars = new List<int>();
      SimpleBot.AddCategory(fac, UnitCategory.Altar, altars);
      var haveAltar = false;
      for (var i = 0; i < altars.Count; i++)
      {
        haveAltar = haveAltar || GetPlayerTechCount(p, altars[i], true) > 0;
      }
      if (!haveAltar && altars.Count > 0 && Build(p, s, workers, hallTypes, Cheapest(altars), false, false, 0f, 0f))
      {
        return;
      }
      if (!SimpleBot.HasPerks(p) || hallTypes.Count == 0 || all.Count == 0)
      {
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
      if (halls.Count >= wantedBases)
      {
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
        Build(p, s, workers, hallTypes, Cheapest(hallTypes), true, true, fx, fy);
      }
    }

    private static void Scan(player p, List<int> builderTypes, List<int> hallTypes, List<unit> workers, List<unit> halls)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!Alive(u, p))
        {
          continue;
        }
        if (!IsUnitType(u, UNIT_TYPE_STRUCTURE) && SimpleBot.ListHas(builderTypes, GetUnitTypeId(u)))
        {
          workers.Add(u);
        }
        else if (IsUnitType(u, UNIT_TYPE_TOWNHALL) || SimpleBot.ListHas(hallTypes, GetUnitTypeId(u)))
        {
          halls.Add(u);
        }
      }
      DestroyGroup(g);
    }

    private static bool TrainWorker(player p, List<int> types, List<unit> halls)
    {
      for (var h = 0; h < halls.Count; h++)
      {
        // Never overwrite an active training / upgrade order.
        if (GetUnitCurrentOrder(halls[h]) != 0)
        {
          return true; // assume it may already be training; do not spawn an emergency worker
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
      return false;
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
      if (type == 0 || (s.UnbuildableUntil.ContainsKey(type) && SimpleBot.Clock < s.UnbuildableUntil[type]))
      {
        return false;
      }
      // Try each worker; a worker may have been carried across a sea or may have a different build menu.
      for (var w = 0; w < workers.Count; w++)
      {
        var worker = workers[w];
        float ax = fixedX, ay = fixedY;
        if (fixedAnchor)
        {
          if (AiRoutes.CompAt(GetUnitX(worker), GetUnitY(worker)) != AiRoutes.CompAt(ax, ay))
          {
            continue;
          }
        }
        else if (!AiBuildSites.HomeAnchor(p, hallTypes, s.HomeX, s.HomeY, worker, out ax, out ay))
        {
          continue;
        }
        // Retry distinct sites immediately for a rejected worker order; do not require a second worker.
        var skipX = 999999f;
        var skipY = 999999f;
        for (var attempt = 0; attempt < 4; attempt++)
        {
          if (!AiBuildSites.FindSpot(p, worker, ax, ay, s.BadX, s.BadY, s.BadUntil,
                skipX, skipY, out var x, out var y))
          {
            break;
          }
          // Free construction for perked bots only. This copies the AiTech.FreeOrder top-up/restore
          // model. Validate in a WC3 playtest whether the charge happens on order or build start.
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
          var accepted = IssueBuildOrderById(worker, type, x, y);
          // If the engine charges at the construction site rather than now, refund it on
          // EVENT_PLAYER_UNIT_CONSTRUCT_START. Do not keep bonus resources during worker travel.
          var delayedCost = bonus && GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) == gold + costGold
                                   && GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) == lumber + costLumber;
          if (bonus)
          {
            SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, gold);
            SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, lumber);
          }
          if (!accepted)
          {
            skipX = x;
            skipY = y;
            s.BadX = x;
            s.BadY = y;
            s.BadUntil = SimpleBot.Clock + RejectedCooldown;
            continue;
          }
          var pr = new AiBuildProject();
          pr.Worker = worker;
          pr.Type = type;
          pr.X = x;
          pr.Y = y;
          pr.OrderedAt = SimpleBot.Clock;
          pr.NewBase = expansion;
          pr.RefundOnStart = delayedCost;
          pr.RefundGold = delayedCost ? costGold : 0;
          pr.RefundLumber = delayedCost ? costLumber : 0;
          s.Project = pr;
          SimpleBot.Log("builder: " + SimpleBot.FacName(p) + " ordered " + GetObjectName(type)
                           + (expansion ? " forward" : " HQ") + " at " + I2S(R2I(x)) + "," + I2S(R2I(y)));
          return true;
        }
      }
      // An invalid faction/build-menu combination shouldn't be reissued dozens of times per minute.
      s.UnbuildableUntil[type] = SimpleBot.Clock + RejectedCooldown;
      return false;
    }
  }
}
