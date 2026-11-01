using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.50: FERRIES (design: CONCEPTS.md 7) -- replace the 8 visible AI ship waygates. Bot-only: no human sees or uses
  // anything but the boat.
  // - Harbours = the ship links in AiGraph.Portals (phase "ship"): pier A <-> pier B. Each pier is a hidden marker
  //   (AiPortals.Marker) and the gate of a one-way portal edge to the other pier. Cost = walk + sailing time (the
  //   lane of tools/ai/gen_lanes.py, AiLanes.g.cs) + boarding wait, as walking distance.
  // - Entry only while no enemy holds the pier's CP (EntryAllowed, via AiRoutes.PortalAllowed); landing anywhere.
  // - Boarding: the pseudo-portal trigger hands a ticketed unit to Board instead of teleporting it. The unit is hidden
  //   (ShowUnit false: out of the game world) and paused, and kept out of the army logic (InTransit). The boat of
  //   this direction and player leaves BoardWait seconds after its first passenger; later arrivals take the next one.
  // - Boat (0.50.1): a transport ship OWNED BY THE BOT (shows on the minimap -- maintainer), invulnerable, removed at
  //   the landing (timed life as a safety net), kept out of the bot's army logic (IsBoat). It sails with real move
  //   orders from lane point to lane point (smooth; 0.50.0 jumped every 0.25 s and stuttered), at half the old speed.
  // - Landing: passengers shown beside the far pier (fixed offsets), unpaused and given the rest of their way at once.
  // "-ferry on|off": off = instant crossing (the 0.46 pseudo-portal teleport).
  internal sealed class AiVoyage
  {
    internal int Dir;            // link k * 2 (A -> B) or k * 2 + 1 (B -> A)
    internal int Pid;
    internal bool Sailing;
    internal float DepartAt;
    internal unit Boat;
    internal int Point;          // next lane point index (0 .. count - 1, in sailing direction)
    internal float X;
    internal float Y;
    internal float Started;
    internal float LastX;        // boat progress check (stuck)
    internal float LastY;
    internal float LastMoveAt;
    internal List<unit> Passengers = new();
    internal List<float> DestX = new();
    internal List<float> DestY = new();
  }

  internal static class AiFerries
  {
    internal static bool On = true;
    private const float BoardWait = 12f;     // the boat leaves this long after its first passenger
    // per second. 0.50.1: 200 (half of 0.50.0's 400). 0.51: the maintainer asked for half again (100), but WC3 clamps
    // move speeds to the map's minimum (MinUnitSpeed, default 150 -- not set in war3mapMisc.txt): 150 is the slowest.
    private const float ShipSpeed = 150f;
    private const float PointReach = 200f;   // a lane point counts as reached within this
    private const float StuckAfter = 4f;     // no progress this long -> the boat is put on its next point
    private const float Slot = 0.25f;        // scheduler slot
    private const float WalkSpeed = 300f;    // to express sailing time as walking distance (edge cost)
    private const float MaxVoyage = 150f;    // safety: land after this long at sea, whatever happened
    private const string BoatType = "hbot";  // WC3's transport ship (model only)
    private const string FxBoard = "Objects\\Spawnmodels\\Naga\\NagaDeath\\NagaDeath.mdl";

    private static List<int> _link = new();         // ship link index (AiGraph.Portals) per lane k
    private static unit[] _pier;                    // marker per pier: k * 2 = pier A, k * 2 + 1 = pier B
    private static int[] _pierNode;
    private static Dictionary<unit, int> _dirOf = new(); // marker -> direction it starts
    private static HashSet<unit> _transit = new();
    private static HashSet<unit> _boats = new();
    private static List<AiVoyage> _voyages = new();
    private static bool _ready;
    internal static int Boarded;
    internal static int Landed;
    internal static int Voyages;
    internal static int Blocked;    // entry refused (enemy holds the pier)

    internal static void Setup()
    {
      AiScheduler.Add("ferries", 1, 0, Tick, false); // every slot (0.25 s): only active boats do work
    }

    private static void Ready()
    {
      if (_ready)
      {
        return;
      }

      _ready = true;
      var list = AiGraph.Portals;
      for (var i = 0; i < list.Count; i++)
      {
        if (list[i].Phase == "ship")
        {
          _link.Add(i);
        }
      }

      if (_link.Count != AiLaneData.Lanes)
      {
        SimpleBot.LogPin("ferries: " + I2S(_link.Count) + " ship links but " + I2S(AiLaneData.Lanes)
                         + " lanes -- run tools/ai/gen_lanes.py; ferries off");
        On = false;
      }

      _pier = new unit[_link.Count * 2];
      _pierNode = new int[_link.Count * 2];
      for (var k = 0; k < _pier.Length; k++)
      {
        _pierNode[k] = -1;
      }
    }

    internal static bool InTransit(unit u) => _transit.Contains(u);

    internal static bool IsBoat(unit u) => _boats.Contains(u);

    internal static bool IsPier(unit gate) => gate != null && _dirOf.ContainsKey(gate);

    private static float PierX(int pier)
    {
      var pl = AiGraph.Portals[_link[pier / 2]];
      return pier % 2 == 0 ? pl.AX : pl.BX;
    }

    private static float PierY(int pier)
    {
      var pl = AiGraph.Portals[_link[pier / 2]];
      return pier % 2 == 0 ? pl.AY : pl.BY;
    }

    // every 30 s (AiPortals.AddStatic, from AiRoutes.CheckPortals): the edges of all piers
    internal static void AddEdges()
    {
      Ready();
      if (_link.Count != AiLaneData.Lanes)
      {
        return; // lanes out of date (logged once in Ready): no ferry edges at all
      }

      for (var k = 0; k < _link.Count; k++)
      {
        var len = LaneLength(k);
        var extra = len * WalkSpeed / ShipSpeed + BoardWait * WalkSpeed * 0.5f;
        for (var side = 0; side < 2; side++)
        {
          var pier = k * 2 + side;
          var other = k * 2 + 1 - side;
          _pier[pier] = AiPortals.Marker(_pier[pier], PierX(pier), PierY(pier));
          _dirOf[_pier[pier]] = pier; // direction index = the pier it starts from
          if (_pierNode[pier] < 0)
          {
            _pierNode[pier] = AiRoutes.NodeAt(PierX(pier), PierY(pier));
          }

          AiRoutes.AddPortalEdgeCost(_pier[pier], PierX(other), PierY(other), extra);
        }
      }
    }

    private static float LaneLength(int k)
    {
      var s = AiLaneData.LaneStart[k];
      var n = AiLaneData.LaneCount[k];
      var len = 0f;
      for (var i = s; i < s + n - 1; i++)
      {
        var dx = AiLaneData.LaneX[i + 1] - AiLaneData.LaneX[i];
        var dy = AiLaneData.LaneY[i + 1] - AiLaneData.LaneY[i];
        len += SquareRoot(dx * dx + dy * dy);
      }

      return len;
    }

    // AiRoutes.PortalAllowed: a pier is no entry while an enemy of p holds its CP (landing is always possible)
    internal static bool EntryAllowed(player p, unit gate)
    {
      if (p == null || gate == null || !_dirOf.ContainsKey(gate))
      {
        return true;
      }

      var node = _pierNode[_dirOf[gate]];
      if (node < 0)
      {
        return true;
      }

      var t = AiRouteData.NodeType[node];
      if (t == 0)
      {
        return true;
      }

      var cp = SimpleBot.SafeCp(t);
      if (cp == null || cp.Unit == null || !SimpleBot.IsEnemyCp(cp.Unit, p))
      {
        return true;
      }

      Blocked++;
      return false;
    }

    // ---- boarding ----------------------------------------------------------------------------------------------

    // AiPortals.Teleport: a ticketed unit reached pier `gate`; (dx, dy) = its final destination
    internal static void Board(unit u, unit gate, float dx, float dy)
    {
      var dir = _dirOf[gate];
      var pid = GetPlayerId(GetOwningPlayer(u));
      AiVoyage v = null;
      for (var i = 0; i < _voyages.Count && v == null; i++)
      {
        if (!_voyages[i].Sailing && _voyages[i].Dir == dir && _voyages[i].Pid == pid)
        {
          v = _voyages[i];
        }
      }

      if (v == null)
      {
        v = new AiVoyage();
        v.Dir = dir;
        v.Pid = pid;
        v.DepartAt = SimpleBot.Clock + BoardWait;
        _voyages.Add(v);
      }

      DestroyEffect(AddSpecialEffect(FxBoard, GetUnitX(u), GetUnitY(u)));
      ShowUnit(u, false);
      PauseUnit(u, true);
      _transit.Add(u);
      v.Passengers.Add(u);
      v.DestX.Add(dx);
      v.DestY.Add(dy);
      Boarded++;
    }

    // ---- sailing (every scheduler slot) --------------------------------------------------------------------------

    private static void Tick()
    {
      if (_voyages.Count == 0)
      {
        return;
      }

      var now = SimpleBot.Clock;
      for (var i = _voyages.Count - 1; i >= 0; i--)
      {
        var v = _voyages[i];
        if (!v.Sailing)
        {
          if (now >= v.DepartAt)
          {
            Depart(v);
          }

          continue;
        }

        if (Sail(v) || now - v.Started > MaxVoyage)
        {
          Land(v);
          _voyages.RemoveAt(i);
        }
      }
    }

    private static int LanePoints(int dir) => AiLaneData.LaneCount[dir / 2];

    // lane point i in sailing direction (pier B -> A walks the lane backwards)
    private static float LaneX(int dir, int i)
    {
      var k = dir / 2;
      var idx = dir % 2 == 0 ? i : AiLaneData.LaneCount[k] - 1 - i;
      return AiLaneData.LaneX[AiLaneData.LaneStart[k] + idx];
    }

    private static float LaneY(int dir, int i)
    {
      var k = dir / 2;
      var idx = dir % 2 == 0 ? i : AiLaneData.LaneCount[k] - 1 - i;
      return AiLaneData.LaneY[AiLaneData.LaneStart[k] + idx];
    }

    private static void Depart(AiVoyage v)
    {
      v.Sailing = true;
      v.Started = SimpleBot.Clock;
      v.X = LaneX(v.Dir, 0);
      v.Y = LaneY(v.Dir, 0);
      v.Point = 1;
      Voyages++;
      var tx = LaneX(v.Dir, 1);
      var ty = LaneY(v.Dir, 1);
      v.Boat = CreateUnit(Player(v.Pid), FourCC(BoatType), v.X, v.Y, Atan2(ty - v.Y, tx - v.X) * 57.29578f);
      if (v.Boat != null)
      {
        _boats.Add(v.Boat);
        SetUnitInvulnerable(v.Boat, true);
        SetUnitMoveSpeed(v.Boat, ShipSpeed);
        BlzSetUnitName(v.Boat, "Ferry -- " + SimpleBot.FacName(Player(v.Pid)));
        UnitApplyTimedLife(v.Boat, FourCC("BTLF"), MaxVoyage + 30f); // never left behind
        v.LastX = v.X;
        v.LastY = v.Y;
        v.LastMoveAt = SimpleBot.Clock;
        IssuePointOrder(v.Boat, "move", tx, ty);
      }
    }

    // one slot of sailing; true = arrived
    private static bool Sail(AiVoyage v)
    {
      if (v.Boat != null && GetUnitTypeId(v.Boat) != 0)
      {
        return SailBoat(v);
      }

      var step = ShipSpeed * Slot;
      var n = LanePoints(v.Dir);
      while (step > 0f && v.Point < n)
      {
        var tx = LaneX(v.Dir, v.Point);
        var ty = LaneY(v.Dir, v.Point);
        var dx = tx - v.X;
        var dy = ty - v.Y;
        var d = SquareRoot(dx * dx + dy * dy);
        if (d <= step)
        {
          v.X = tx;
          v.Y = ty;
          step -= d;
          v.Point++;
        }
        else
        {
          v.X += dx / d * step;
          v.Y += dy / d * step;
          step = 0f;
          if (v.Boat != null)
          {
            SetUnitFacing(v.Boat, Atan2(dy, dx) * 57.29578f);
          }
        }
      }

      if (v.Boat != null)
      {
        SetUnitX(v.Boat, v.X);
        SetUnitY(v.Boat, v.Y);
      }

      return v.Point >= n;
    }

    // 0.50.1: the boat sails by itself (move order to the next lane point); this only checks progress
    private static bool SailBoat(AiVoyage v)
    {
      var n = LanePoints(v.Dir);
      var bx = GetUnitX(v.Boat);
      var by = GetUnitY(v.Boat);
      var tx = LaneX(v.Dir, v.Point);
      var ty = LaneY(v.Dir, v.Point);
      var now = SimpleBot.Clock;
      var reached = (tx - bx) * (tx - bx) + (ty - by) * (ty - by) <= PointReach * PointReach;
      var moved = (bx - v.LastX) * (bx - v.LastX) + (by - v.LastY) * (by - v.LastY) >= 50f * 50f;
      if (moved)
      {
        v.LastX = bx;
        v.LastY = by;
        v.LastMoveAt = now;
      }
      else if (now - v.LastMoveAt >= StuckAfter)
      {
        SetUnitX(v.Boat, tx); // stuck (shore, another boat): put it on the point and go on
        SetUnitY(v.Boat, ty);
        v.LastX = tx;
        v.LastY = ty;
        v.LastMoveAt = now;
        reached = true;
      }

      if (!reached)
      {
        if (GetUnitCurrentOrder(v.Boat) == 0)
        {
          IssuePointOrder(v.Boat, "move", tx, ty);
        }

        return false;
      }

      v.Point++;
      if (v.Point >= n)
      {
        return true;
      }

      IssuePointOrder(v.Boat, "move", LaneX(v.Dir, v.Point), LaneY(v.Dir, v.Point));
      return false;
    }

    private static void Land(AiVoyage v)
    {
      if (v.Boat != null)
      {
        _boats.Remove(v.Boat);
        RemoveUnit(v.Boat);
        v.Boat = null;
      }

      var far = v.Dir % 2 == 0 ? v.Dir + 1 : v.Dir - 1; // the other pier of the link
      var lx = PierX(far);
      var ly = PierY(far);
      for (var i = 0; i < v.Passengers.Count; i++)
      {
        var u = v.Passengers[i];
        _transit.Remove(u);
        if (GetUnitTypeId(u) == 0)
        {
          continue; // removed while at sea
        }

        // fixed offsets around the pier (8 directions, two rings) -- the engine moves each to a free spot
        var ring = i % 16 < 8 ? 120f : 240f;
        var a = (i % 8) * 0.785398f;
        SetUnitPosition(u, lx + ring * Cos(a), ly + ring * Sin(a));
        PauseUnit(u, false);
        ShowUnit(u, true);
        Landed++;
        if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        AiTrack.Ordered(u, v.DestX[i], v.DestY[i], AiTrack.KPortal);
        if (!AiRoutes.Chain(GetOwningPlayer(u), u, v.DestX[i], v.DestY[i]))
        {
          AiScheduler.CountOrder(u, v.DestX[i], v.DestY[i]);
          IssuePointOrder(u, "attack", v.DestX[i], v.DestY[i]);
        }
      }
    }

    // ---- command and dump ------------------------------------------------------------------------------------------

    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg == "-ferry on")
      {
        On = true;
      }
      else if (msg == "-ferry off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "bot ferries: " + (On ? "ON (board, sail, land)"
        : "OFF (instant crossing)") + "  -- -ferry on|off");
    }

    internal static void Lines(List<string> o)
    {
      var atSea = 0;
      var waiting = 0;
      for (var i = 0; i < _voyages.Count; i++)
      {
        if (_voyages[i].Sailing)
        {
          atSea += _voyages[i].Passengers.Count;
        }
        else
        {
          waiting += _voyages[i].Passengers.Count;
        }
      }

      o.Add("ferries (0.50) " + (On ? "on" : "off") + ": " + I2S(_link.Count) + " links; boarded " + I2S(Boarded)
            + ", landed " + I2S(Landed) + ", voyages " + I2S(Voyages) + ", now at sea " + I2S(atSea) + " / boarding "
            + I2S(waiting) + "; route checks refused (enemy pier) " + I2S(Blocked));
    }
  }
}
