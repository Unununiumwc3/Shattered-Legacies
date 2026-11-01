using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.48.3: MEASUREMENT ONLY for the back-and-forth movement (TODO.md "Open problems" 1) -- nothing here changes what
  // the bots do. Two parts:
  // (1) TURNAROUNDS, all bots: every army unit is sampled on its bot's service (~8 s with 16 bots, SimpleBot.Process,
  //     before the theatre split). Two walks of at least MinStep in a row, the second within TurnWindow of the first,
  //     that differ by more than 120 degrees = one turnaround. Its cause = the last bot order the unit got after the
  //     first walk began (AiArmy.Issue: change / rest / idle-away / ...), else "engine" (no bot order: chases, creep
  //     leash, rally point, crowding). Teleports (more than Jump in one sample) break the chain.
  // (2) TRAIL, one bot (default P11 Druids; -track <pid> | -track off | -track = status + minimap ping): up to Slots
  //     units are followed from the moment they are trained (half the slots may adopt existing units, so the start of
  //     the game is covered too). Every bot order, turnaround, teleport and death is a line in the dump's TRAIL
  //     section, plus one position line per service. Order lines say how far the destination is from the unit's
  //     spawn point ("back to where it was produced?") and whether it points backwards from its last walk.
  // Deterministic counters only (game time, synced unit state); the trail is text for the dump (desync-safe).
  internal sealed class AiTrackUnit
  {
    internal bool Seen;
    internal float X;
    internal float Y;
    internal float T;
    internal bool HasWalk;
    internal float Wx;      // last walk (displacement of one sample interval of at least MinStep)
    internal float Wy;
    internal float WStart;  // time the last walk began (sample before it)
    internal float WEnd;    // time the last walk ended
    internal int Kind = -1; // last bot order (AiTrack.K...)
    internal float KindAt;
    internal bool Dropped;  // left the attack group while walking (target change), not yet ordered again
    internal int Slot = -1; // trail slot, -1 = not followed
  }

  internal static class AiTrack
  {
    internal const int KChange = 0;
    internal const int KAdvance = 1;
    internal const int KRest = 2;
    internal const int KIdle = 3;
    internal const int KRefill = 4;
    internal const int KExit = 5;
    internal const int KDefend = 6;
    internal const int KWhole = 7;
    internal const int KDefRest = 8;
    internal const int KPortal = 9;
    internal const int KHero = 10;
    internal const int KEngine = 11;
    private const int Kinds = 12;
    private const float MinStep = 300f;
    private const float Jump = 4500f;
    private const float TurnWindow = 25f;
    private const int Slots = 8;
    private const int TrailCap = 700;
    private const int DoneCap = 40;
    private const int Phases = 8; // 5-min game phases, the last one open-ended (35m+)

    internal static int TrackPid = 11; // Druids (maintainer, 0.48.2: they do it most at the start)

    private static string[] _kn = new string[]
    {
      "change", "advance", "rest", "idle-away", "refill", "exit", "defend", "whole", "def-rest", "portal", "hero",
      "engine",
    };

    private static Dictionary<unit, AiTrackUnit> _u = new();
    private static int[] _turns = new int[24 * Kinds];
    private static int[] _phase = new int[Phases * Kinds];
    private static int[] _walks = new int[24];
    private static int[] _swapBusy = new int[24];
    private static int[] _swapBack = new int[24];
    private static int[] _restBack = new int[24];
    private static int _jumps;

    private static unit[] _slotUnit = new unit[Slots];
    private static int[] _slotId = new int[Slots];
    private static float[] _slotBorn = new float[Slots];
    private static float[] _slotSx = new float[Slots];
    private static float[] _slotSy = new float[Slots];
    private static float[] _slotWalked = new float[Slots];
    private static int[] _slotOrders = new int[Slots];
    private static int[] _slotTurnKind = new int[Slots * Kinds];
    private static string[] _slotName = new string[Slots];
    private static int _nextId;
    private static List<string> _trail = new();
    private static List<string> _done = new();

    private static AiTrackUnit Get(unit u)
    {
      if (_u.TryGetValue(u, out var s))
      {
        return s;
      }

      s = new AiTrackUnit();
      _u[u] = s;
      return s;
    }

    // ---- hooks ---------------------------------------------------------------------------------------------------

    // SimpleBot.Process, after the army pass (all orderable army units of the bot, before the theatre split)
    internal static void Sample(player p, int pid, List<unit> units)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      var now = SimpleBot.Clock;
      for (var i = 0; i < units.Count; i++)
      {
        Step(pid, units[i], Get(units[i]), now);
      }

      if (pid == TrackPid)
      {
        TrailSample(pid, units, now);
      }

      if (_u.Count > 6000)
      {
        Reset(); // dead units pile up -- start over now and then (followed units are kept)
      }
    }

    // AiArmy.Issue (and the other order sites): a bot order to an army unit
    internal static void Ordered(unit u, float x, float y, int kind)
    {
      var s = Get(u);
      var pid = GetPlayerId(GetOwningPlayer(u));
      if (kind == KRest && s.Dropped && pid >= 0 && pid < 24)
      {
        _swapBack[pid] = _swapBack[pid] + 1; // dropped from the group while walking, now sent to the staging point
      }

      var back = false;
      if (s.HasWalk && SimpleBot.Clock - s.WEnd <= TurnWindow) // walking right now: does the order point backwards?
      {
        var dx = x - GetUnitX(u);
        var dy = y - GetUnitY(u);
        back = Reverses(s.Wx, s.Wy, dx, dy);
        if (back && kind == KRest && pid >= 0 && pid < 24)
        {
          _restBack[pid] = _restBack[pid] + 1;
        }
      }

      s.Kind = kind;
      s.KindAt = SimpleBot.Clock;
      s.Dropped = false; // counted once: sent back (rest) or taken into a group again

      if (s.Slot >= 0)
      {
        TrailOrder(u, s, pid, x, y, kind, back);
      }
    }

    // AiArmy.Attack, new target: u was in the old attack group, is not in the new one, and was still walking
    internal static void Dropped(int pid, unit u)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      _swapBusy[pid] = _swapBusy[pid] + 1;
      var s = Get(u);
      s.Dropped = true;
      if (s.Slot >= 0)
      {
        Trail(Tag(s.Slot) + " dropped from the group while walking (new target) @" + Xy(GetUnitX(u), GetUnitY(u))
              + " order " + OrderName(u));
      }
    }

    // AiStats.OnTrained: a bot unit finished training in building b
    internal static void Trained(unit u, unit b)
    {
      if (u == null || GetPlayerId(GetOwningPlayer(u)) != TrackPid || IsUnitType(u, UNIT_TYPE_STRUCTURE)
          || IsUnitType(u, UNIT_TYPE_PEON) || GetUnitMoveSpeed(u) <= 1f)
      {
        return;
      }

      var k = FreeSlot();
      if (k >= 0)
      {
        Follow(k, u, "new", b != null ? GetUnitName(b) : "?");
      }
    }

    // ---- turnarounds -----------------------------------------------------------------------------------------------

    private static void Step(int pid, unit u, AiTrackUnit s, float now)
    {
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      if (!s.Seen)
      {
        s.Seen = true;
        s.X = x;
        s.Y = y;
        s.T = now;
        return;
      }

      if (s.T >= now)
      {
        return; // sampled already this service (followed unit)
      }

      var dx = x - s.X;
      var dy = y - s.Y;
      var d2 = dx * dx + dy * dy;
      if (d2 > Jump * Jump)
      {
        _jumps++;
        s.HasWalk = false;
        if (s.Slot >= 0)
        {
          Trail(Tag(s.Slot) + " jump @" + Xy(s.X, s.Y) + " -> @" + Xy(x, y) + " (teleport)");
        }
      }
      else if (d2 >= MinStep * MinStep)
      {
        _walks[pid] = _walks[pid] + 1;
        if (s.Slot >= 0)
        {
          _slotWalked[s.Slot] = _slotWalked[s.Slot] + SquareRoot(d2);
        }

        if (s.HasWalk && now - s.WEnd <= TurnWindow && Reverses(s.Wx, s.Wy, dx, dy))
        {
          var cause = KEngine;
          if (s.Kind >= 0 && s.KindAt > s.WStart)
          {
            cause = s.Kind;
          }

          var ti = pid * Kinds + cause;
          _turns[ti] = _turns[ti] + 1;
          var ph = R2I(now / 300f);
          if (ph >= Phases)
          {
            ph = Phases - 1;
          }

          var pi = ph * Kinds + cause;
          _phase[pi] = _phase[pi] + 1;
          if (s.Slot >= 0)
          {
            var si = s.Slot * Kinds + cause;
            _slotTurnKind[si] = _slotTurnKind[si] + 1;
            var why = cause == KEngine
              ? "engine (no bot order since " + I2S(R2I(now - s.WStart)) + "s" + (s.Kind >= 0 ? "; last " + _kn[s.Kind] + " "
                + I2S(R2I(now - s.KindAt)) + "s ago)" : ")")
              : _kn[cause] + " order " + I2S(R2I(now - s.KindAt)) + "s ago";
            Trail(Tag(s.Slot) + " TURN " + I2S(Angle(s.Wx, s.Wy, dx, dy)) + "deg @" + Xy(x, y) + " legs "
                  + I2S(R2I(SquareRoot(s.Wx * s.Wx + s.Wy * s.Wy))) + "/" + I2S(R2I(SquareRoot(d2))) + " cause " + why);
          }
        }

        s.HasWalk = true;
        s.Wx = dx;
        s.Wy = dy;
        s.WStart = s.T;
        s.WEnd = now;
      }

      s.X = x;
      s.Y = y;
      s.T = now;
    }

    // more than 120 degrees between the two vectors (cos < -0.5)
    private static bool Reverses(float ax, float ay, float bx, float by)
    {
      var la = SquareRoot(ax * ax + ay * ay);
      var lb = SquareRoot(bx * bx + by * by);
      if (la < 1f || lb < 1f)
      {
        return false;
      }

      return ax * bx + ay * by < -0.5f * la * lb;
    }

    private static int Angle(float ax, float ay, float bx, float by)
    {
      var la = SquareRoot(ax * ax + ay * ay);
      var lb = SquareRoot(bx * bx + by * by);
      if (la < 1f || lb < 1f)
      {
        return 0;
      }

      var c = (ax * bx + ay * by) / (la * lb);
      if (c > 1f)
      {
        c = 1f;
      }

      if (c < -1f)
      {
        c = -1f;
      }

      return R2I(Acos(c) * 57.29578f + 0.5f);
    }

    private static void Reset()
    {
      _u.Clear();
      for (var k = 0; k < Slots; k++)
      {
        if (_slotUnit[k] != null)
        {
          var s = new AiTrackUnit();
          s.Slot = k;
          _u[_slotUnit[k]] = s;
        }
      }
    }

    // ---- trail (one bot) -------------------------------------------------------------------------------------------

    private static int FreeSlot()
    {
      for (var k = 0; k < Slots; k++)
      {
        if (_slotUnit[k] == null)
        {
          return k;
        }
      }

      return -1;
    }

    private static int Followed()
    {
      var n = 0;
      for (var k = 0; k < Slots; k++)
      {
        if (_slotUnit[k] != null)
        {
          n++;
        }
      }

      return n;
    }

    private static void Follow(int k, unit u, string how, string where)
    {
      var s = Get(u);
      if (s.Slot >= 0)
      {
        return;
      }

      _nextId++;
      s.Slot = k;
      _slotUnit[k] = u;
      _slotId[k] = _nextId;
      _slotBorn[k] = SimpleBot.Clock;
      _slotSx[k] = GetUnitX(u);
      _slotSy[k] = GetUnitY(u);
      _slotWalked[k] = 0f;
      _slotOrders[k] = 0;
      _slotName[k] = GetUnitName(u);
      for (var c = 0; c < Kinds; c++)
      {
        _slotTurnKind[k * Kinds + c] = 0;
      }

      Trail(Tag(k) + " " + how + " " + _slotName[k] + " @" + Xy(_slotSx[k], _slotSy[k]) + " (" + where + ")"
            + " order " + OrderName(u));
    }

    private static void TrailSample(int pid, List<unit> units, float now)
    {
      for (var k = 0; k < Slots; k++)
      {
        var u = _slotUnit[k];
        if (u == null)
        {
          continue;
        }

        if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          Finish(k, "died");
        }
        else if (GetPlayerId(GetOwningPlayer(u)) != pid)
        {
          Finish(k, "changed owner");
        }
        else
        {
          Step(pid, u, Get(u), now); // not in the army list (mission, hero care): sample it anyway
        }
      }

      // half the slots may adopt units that already exist (start of the game); the rest waits for new units
      for (var i = 0; i < units.Count && Followed() < Slots / 2; i++)
      {
        if (Get(units[i]).Slot < 0)
        {
          Follow(FreeSlot(), units[i], "adopt", "already in the army");
        }
      }

      var line = "";
      var n = 0;
      for (var k = 0; k < Slots; k++)
      {
        var u = _slotUnit[k];
        if (u == null)
        {
          continue;
        }

        line = line + " " + Tag(k) + " " + Xy(GetUnitX(u), GetUnitY(u)) + " " + OrderName(u) + " " + AiArmy.RoleOf(pid, u);
        n++;
        if (n == 4)
        {
          Trail("pos" + line);
          line = "";
          n = 0;
        }
      }

      if (n > 0)
      {
        Trail("pos" + line);
      }
    }

    private static void TrailOrder(unit u, AiTrackUnit s, int pid, float x, float y, int kind, bool back)
    {
      var k = s.Slot;
      _slotOrders[k] = _slotOrders[k] + 1;
      var ux = GetUnitX(u);
      var uy = GetUnitY(u);
      var dx = x - ux;
      var dy = y - uy;
      var sx = x - _slotSx[k];
      var sy = y - _slotSy[k];
      var d = pid >= 0 && pid < 24 ? SimpleBot.GetDiag(pid) : null;
      var tgt = d != null && d.Target != null ? GetUnitName(d.Target) : "-";
      Trail(Tag(k) + " ord " + _kn[kind] + (back ? " BACK" : "") + " @" + Xy(ux, uy) + " -> @" + Xy(x, y) + " d="
            + I2S(R2I(SquareRoot(dx * dx + dy * dy))) + " spawn " + I2S(R2I(SquareRoot(sx * sx + sy * sy)))
            + (AiTheatres.Secondary ? " T" + I2S(AiTheatres.Offset / 24 + 1) : "")
            + " " + (d != null ? d.Reason : "-") + " tgt " + tgt);
    }

    private static void Finish(int k, string why)
    {
      var u = _slotUnit[k];
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      var nx = x - _slotSx[k];
      var ny = y - _slotSy[k];
      var line = Tag(k) + " " + _slotName[k] + " " + why + " @" + Xy(x, y) + " age "
                 + SimpleBot.TimeTag(SimpleBot.Clock - _slotBorn[k]) + Summary(k, nx, ny);
      Trail(line);
      _done.Add(SimpleBot.TimeTag(SimpleBot.Clock) + " " + line);
      if (_done.Count > DoneCap)
      {
        _done.RemoveAt(0);
      }

      if (_u.TryGetValue(u, out var s))
      {
        s.Slot = -1;
      }

      _slotUnit[k] = null;
    }

    private static string Summary(int k, float nx, float ny)
    {
      var turns = 0;
      var parts = "";
      for (var c = 0; c < Kinds; c++)
      {
        var n = _slotTurnKind[k * Kinds + c];
        if (n > 0)
        {
          turns += n;
          parts = parts + " " + _kn[c] + " " + I2S(n);
        }
      }

      return " walked " + I2S(R2I(_slotWalked[k])) + " net " + I2S(R2I(SquareRoot(nx * nx + ny * ny))) + " orders "
             + I2S(_slotOrders[k]) + " turns " + I2S(turns) + (parts != "" ? " (" + parts.Substring(1) + ")" : "");
    }

    private static void Trail(string msg)
    {
      _trail.Add(SimpleBot.TimeTag(SimpleBot.Clock) + " " + msg);
      if (_trail.Count > TrailCap)
      {
        _trail.RemoveAt(0);
      }
    }

    private static string Tag(int k) => "U" + I2S(_slotId[k]);

    private static string Xy(float x, float y) => I2S(R2I(x)) + "," + I2S(R2I(y));

    // a = attack, m = move, s = smart (rally / waygate click), - = idle, else the order's name
    private static string OrderName(unit u)
    {
      var id = GetUnitCurrentOrder(u);
      if (id == 0)
      {
        return "-";
      }

      var name = OrderId2String(id);
      if (name == null || name == "")
      {
        return I2S(id);
      }

      if (name == "attack")
      {
        return "a";
      }

      if (name == "move")
      {
        return "m";
      }

      return name == "smart" ? "s" : name;
    }

    // ---- command and dump ------------------------------------------------------------------------------------------

    // -track <pid> | -track off | -track (status, pings the followed units for the player who typed it)
    internal static void OnCommand()
    {
      var who = GetTriggerPlayer();
      var msg = GetEventPlayerChatString();
      var rest = msg.Length > 7 && msg.Substring(0, 7) == "-track " ? msg.Substring(7) : "";
      if (rest == "off")
      {
        SetPid(-1);
      }
      else if (rest != "")
      {
        var n = S2I(rest);
        if (n >= 0 && n < 24 && (rest == "0" || n > 0))
        {
          SetPid(n);
        }
      }

      var s = "trail: " + (TrackPid >= 0 ? "P" + I2S(TrackPid) + " " + SimpleBot.FacName(Player(TrackPid)) : "off")
              + ", followed:";
      for (var k = 0; k < Slots; k++)
      {
        var u = _slotUnit[k];
        if (u == null)
        {
          continue;
        }

        s = s + " " + Tag(k) + " " + _slotName[k] + " @" + Xy(GetUnitX(u), GetUnitY(u));
        if (GetLocalPlayer() == who)
        {
          PingMinimap(GetUnitX(u), GetUnitY(u), 8f); // display only, like -portals
        }
      }

      DisplayTimedTextToPlayer(who, 0, 0, 15, s + "  -- -track <pid>|off");
    }

    private static void SetPid(int pid)
    {
      if (pid == TrackPid)
      {
        return;
      }

      for (var k = 0; k < Slots; k++)
      {
        if (_slotUnit[k] != null)
        {
          Finish(k, "no longer followed (-track)");
        }
      }

      TrackPid = pid;
      Trail("trail now " + (pid >= 0 ? "P" + I2S(pid) : "off"));
    }

    internal static void Lines(List<string> o)
    {
      o.Add("## TURNAROUNDS (0.48.3; >120 deg between two walks of >=300 within 25 s; cause = last bot order since the");
      o.Add("  first walk began, else engine; walks = samples with >=300 movement)  teleports " + I2S(_jumps));
      for (var pid = 0; pid < 24; pid++)
      {
        if (_walks[pid] == 0)
        {
          continue;
        }

        var total = 0;
        var parts = "";
        for (var c = 0; c < Kinds; c++)
        {
          var n = _turns[pid * Kinds + c];
          if (n > 0)
          {
            total += n;
            parts = parts + " " + _kn[c] + " " + I2S(n);
          }
        }

        o.Add("P" + I2S(pid) + " turns " + I2S(total) + " of " + I2S(_walks[pid]) + " walks:" + parts
              + " | group swaps " + I2S(_swapBusy[pid]) + ", sent back " + I2S(_swapBack[pid]) + " | rest orders backwards "
              + I2S(_restBack[pid]));
      }

      for (var ph = 0; ph < Phases; ph++)
      {
        var parts = "";
        for (var c = 0; c < Kinds; c++)
        {
          var n = _phase[ph * Kinds + c];
          if (n > 0)
          {
            parts = parts + " " + _kn[c] + " " + I2S(n);
          }
        }

        if (parts != "")
        {
          o.Add("phase " + I2S(ph * 5) + (ph == Phases - 1 ? "m+:" : "-" + I2S(ph * 5 + 5) + "m:") + parts);
        }
      }

      o.Add("## TRAIL " + (TrackPid >= 0 ? "P" + I2S(TrackPid) + " " + SimpleBot.FacName(Player(TrackPid)) : "off")
            + " (0.48.3; pos: U@x,y order role -- a attack, m move, s smart, - idle; g group, r rest, w whole army)");
      for (var k = 0; k < Slots; k++)
      {
        var u = _slotUnit[k];
        if (u != null)
        {
          o.Add("now " + Tag(k) + " " + _slotName[k] + " age " + SimpleBot.TimeTag(SimpleBot.Clock - _slotBorn[k])
                + Summary(k, GetUnitX(u) - _slotSx[k], GetUnitY(u) - _slotSy[k]));
        }
      }

      for (var i = 0; i < _done.Count; i++)
      {
        o.Add("done " + _done[i]);
      }

      for (var i = 0; i < _trail.Count; i++)
      {
        o.Add(_trail[i]);
      }
    }
  }
}
