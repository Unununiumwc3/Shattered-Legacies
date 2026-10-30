using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.Ai
{
  // 0.46: ENGINE LOAD (design: CONCEPTS.md 8-9) -- per-bot A/B switches, guard positions, the unit budget per CP and
  // the order-latency probe. All counters are deterministic (game time, synced unit state).

  // Per-bot A/B for behaviour switches: one game compares two halves of the bots. Swap the assignment next game.
  // Every switch splits by its OWN bit of the player slot (guard: bit 0, gather: bit 1, spread: bit 2), so the halves
  // of different tests are independent -- with one split for all, a difference could come from any of them.
  internal static class AiAb
  {
    internal const int Off = 0;
    internal const int OnAll = 1;
    internal const int Ab = 2;  // slots whose bit is 0 on
    internal const int Ab2 = 3; // slots whose bit is 1 on
    internal const int BitGuard = 0;
    internal const int BitGather = 1;
    internal const int BitSpread = 2;

    internal static int Half(int pid, int bit)
    {
      var d = 1;
      for (var k = 0; k < bit; k++)
      {
        d = d * 2;
      }

      return (pid / d) % 2;
    }

    internal static bool For(int mode, int pid, int bit)
    {
      return mode == OnAll || (mode == Ab && Half(pid, bit) == 0) || (mode == Ab2 && Half(pid, bit) == 1);
    }

    internal static string Name(int mode)
    {
      if (mode == OnAll)
      {
        return "on";
      }

      if (mode == Ab)
      {
        return "ab";
      }

      if (mode == Ab2)
      {
        return "ab2";
      }

      return "off";
    }

    // the slots a mode switches on, e.g. "P0 P1 P4 P5" -- for the dump
    internal static string Slots(int mode, int bit)
    {
      var s = "";
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (pid >= 0 && pid < 24 && For(mode, pid, bit))
        {
          s = s + " P" + I2S(pid);
        }
      }

      return s == "" ? " none" : s;
    }

    // a per-bot value summed over each half: "on-half N bots avg X | off-half M bots avg Y"
    internal static string Compare(int mode, int bit, int[] perBot)
    {
      int nOn = 0, nOff = 0, sumOn = 0, sumOff = 0;
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (pid < 0 || pid >= 24)
        {
          continue;
        }

        if (For(mode, pid, bit))
        {
          nOn++;
          sumOn += perBot[pid];
        }
        else
        {
          nOff++;
          sumOff += perBot[pid];
        }
      }

      return "on " + I2S(nOn) + " bots avg " + I2S(nOn > 0 ? sumOn / nOn : 0) + " | off " + I2S(nOff) + " bots avg "
             + I2S(nOff > 0 ? sumOff / nOff : 0);
    }

    // "-cmd on|off|ab|ab2" -> mode, -1 = not understood
    internal static int Parse(string msg, string cmd)
    {
      if (msg == cmd + " on")
      {
        return OnAll;
      }

      if (msg == cmd + " off")
      {
        return Off;
      }

      if (msg == cmd + " ab")
      {
        return Ab;
      }

      if (msg == cmd + " ab2")
      {
        return Ab2;
      }

      return -1;
    }
  }

  // Guard positions (hypothesis, TODO 0.46): units of computer players walk back to a "guard position" after chasing
  // an enemy. RemoveAllGuardPositions per bot + RemoveGuardPosition on every unit a bot gets. Default ab: half of the
  // bots, compared by their idle-away and rest orders (AiArmy.IdleBy / RestBy). Turning it off later cannot give
  // removed positions back -- it only stops removing.
  internal static class AiGuard
  {
    internal static int Mode = AiAb.Ab;
    internal static int Removed;
    private static trigger _enter;

    internal static void Setup()
    {
      _enter = CreateTrigger();
      var world = CreateRegion();
      RegionAddRect(world, GetWorldBounds());
      TriggerRegisterEnterRegion(_enter, world, null);
      TriggerAddAction(_enter, OnEnter);
      ApplyAll();
    }

    private static void ApplyAll()
    {
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (AiAb.For(Mode, pid, AiAb.BitGuard))
        {
          RemoveAllGuardPositions(bots[b]);
        }
      }
    }

    private static void OnEnter()
    {
      try
      {
        var u = GetTriggerUnit();
        var p = GetOwningPlayer(u);
        var pid = GetPlayerId(p);
        if (pid < 24 && AiAb.For(Mode, pid, AiAb.BitGuard) && SimpleBot.IsBot(p) && !IsUnitType(u, UNIT_TYPE_STRUCTURE))
        {
          RemoveGuardPosition(u);
          Removed++;
        }
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("TICK ERROR in guard positions: " + e.Message);
      }
    }

    internal static void OnCommand()
    {
      var m = AiAb.Parse(GetEventPlayerChatString(), "-guard");
      if (m >= 0)
      {
        Mode = m;
        ApplyAll();
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "guard positions removed for bots: " + AiAb.Name(Mode)
        + " [slots" + AiAb.Slots(Mode, AiAb.BitGuard) + "]  -- -guard on|off|ab|ab2");
    }

    // idle-away and rest orders per bot of each half (the A/B comparison)
    internal static string Line()
    {
      return "guard positions removed: " + AiAb.Name(Mode) + " [slots" + AiAb.Slots(Mode, AiAb.BitGuard) + "], on "
             + I2S(Removed) + " units; idle-away orders " + AiAb.Compare(Mode, AiAb.BitGuard, AiArmy.IdleBy)
             + "; rest orders " + AiAb.Compare(Mode, AiAb.BitGuard, AiArmy.RestBy);
    }
  }

  // Unit budget per CP (CONCEPTS.md 8): a bot trains units only while its army food (heroes not counted) is below
  // Base + PerCp x held CPs, never above the faction's food maximum. A production gate in the bot -- WL's food state
  // is not touched. Base per faction can be set by Shattered Legacies through AiApi.SetBudgetBase.
  internal static class AiBudget
  {
    internal static bool On = true;
    internal static int Base = 40;
    internal static int PerCp = 10;
    internal static int[] Budget = new int[24];
    internal static int[] Army = new int[24];
    internal static int[] CappedTurns = new int[24];
    private static Dictionary<string, int> _baseOf = new();

    internal static void SetBase(string factionName, int value)
    {
      _baseOf[factionName] = value;
    }

    // from the production turn: false = the bot trains no units this turn (heroes still)
    internal static bool Allows(player p, int pid, Faction fac, int armyFood)
    {
      if (!On || pid < 0 || pid >= 24 || fac == null)
      {
        return true;
      }

      var b = _baseOf.ContainsKey(fac.Name) ? _baseOf[fac.Name] : Base;
      b = b + PerCp * p.GetPlayerData().ControlPoints.Count;
      if (fac.FoodMaximum > 0 && b > fac.FoodMaximum)
      {
        b = fac.FoodMaximum;
      }

      Budget[pid] = b;
      Army[pid] = armyFood;
      if (armyFood < b)
      {
        return true;
      }

      CappedTurns[pid]++;
      return false;
    }

    // "-cap on" / "-cap off" / "-cap <base> <perCp>"
    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      var rest = msg.Length > 5 && msg.Substring(0, 5) == "-cap " ? msg.Substring(5) : "";
      if (rest == "on")
      {
        On = true;
      }
      else if (rest == "off")
      {
        On = false;
      }
      else
      {
        var sp = -1;
        for (var i = 1; i < rest.Length && sp < 0; i++) // only Substring/Length -- safest in CSharp.lua
        {
          if (rest.Substring(i, 1) == " ")
          {
            sp = i;
          }
        }

        if (sp > 0)
        {
          Base = S2I(rest.Substring(0, sp));
          PerCp = S2I(rest.Substring(sp + 1));
          On = true;
        }
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "unit budget: " + (On ? "ON, " + I2S(Base) + " + "
        + I2S(PerCp) + " food per held CP (max = faction food maximum)" : "OFF") + "  -- -cap on|off|<base> <perCp>");
    }

    internal static string Line()
    {
      var s = "unit budget " + (On ? I2S(Base) + " + " + I2S(PerCp) + "/CP" : "off") + " (army/budget, capped turns):";
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (pid >= 0 && pid < 24 && Budget[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(Army[pid]) + "/" + I2S(Budget[pid]) + " c" + I2S(CappedTurns[pid]) + ";";
        }
      }

      return s;
    }
  }

  // Order-latency probe (CONCEPTS.md 9): the delay players feel (orders start 1-2 s late) is WC3's pathfinder queue.
  // Samples orders the bot gives anyway: a unit that is IDLE when its first chain point is ordered, at most one new
  // sample per 2 s, at most MaxTracked at once. Latency = game time until it has moved 32. Pathing is part of the
  // synced simulation, so this is the same on every client. Buckets: <0.5 s, <1, <2, <5, <10, stuck (>= 10 s).
  internal static class AiProbe
  {
    private const int MaxTracked = 8;
    private const float GiveUp = 10f;
    private const float Window = 300f;
    private static timer _clockT;
    private static unit[] _u = new unit[MaxTracked];
    private static float[] _x = new float[MaxTracked];
    private static float[] _y = new float[MaxTracked];
    private static float[] _t = new float[MaxTracked];
    private static float _lastSample = -100f;
    private static float _winStart;
    internal static int[] All = new int[6];
    private static int[] _win = new int[6];
    private static List<string> _windows = new();

    internal static void Setup()
    {
      _clockT = CreateTimer();
      TimerStart(_clockT, 100000000f, false, () => { });
      AiScheduler.Add("latency probe", 1, 0, Check, false); // every slot (0.25 s)
    }

    private static float Now() => _clockT != null ? TimerGetElapsed(_clockT) : 0f;

    // from AiRoutes.ChainPoint, BEFORE the order is issued
    internal static void Sample(unit u, float x, float y)
    {
      if (_clockT == null || GetUnitCurrentOrder(u) != 0)
      {
        return;
      }

      var now = Now();
      if (now - _lastSample < 2f)
      {
        return;
      }

      float ux = GetUnitX(u), uy = GetUnitY(u);
      if ((x - ux) * (x - ux) + (y - uy) * (y - uy) < 600f * 600f)
      {
        return;
      }

      for (var i = 0; i < MaxTracked; i++)
      {
        if (_u[i] == null)
        {
          _u[i] = u;
          _x[i] = ux;
          _y[i] = uy;
          _t[i] = now;
          _lastSample = now;
          return;
        }
      }
    }

    private static void Check()
    {
      var now = Now();
      for (var i = 0; i < MaxTracked; i++)
      {
        var u = _u[i];
        if (u == null)
        {
          continue;
        }

        if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          _u[i] = null;
          continue;
        }

        var dx = GetUnitX(u) - _x[i];
        var dy = GetUnitY(u) - _y[i];
        var waited = now - _t[i];
        if (dx * dx + dy * dy >= 32f * 32f)
        {
          Count(Bucket(waited));
          _u[i] = null;
        }
        else if (waited >= GiveUp)
        {
          Count(5);
          _u[i] = null;
        }
      }

      if (now - _winStart >= Window)
      {
        _windows.Add(I2S(R2I(_winStart / 60f)) + "-" + I2S(R2I(now / 60f)) + "m " + Hist(_win));
        if (_windows.Count > 12)
        {
          _windows.RemoveAt(0);
        }

        for (var k = 0; k < 6; k++)
        {
          _win[k] = 0;
        }

        _winStart = now;
      }
    }

    private static int Bucket(float s)
    {
      if (s < 0.5f)
      {
        return 0;
      }

      if (s < 1f)
      {
        return 1;
      }

      if (s < 2f)
      {
        return 2;
      }

      if (s < 5f)
      {
        return 3;
      }

      return s < 10f ? 4 : 5;
    }

    private static void Count(int k)
    {
      All[k] = All[k] + 1;
      _win[k] = _win[k] + 1;
    }

    private static string Hist(int[] h)
    {
      return "<0.5s " + I2S(h[0]) + " <1s " + I2S(h[1]) + " <2s " + I2S(h[2]) + " <5s " + I2S(h[3]) + " <10s "
             + I2S(h[4]) + " stuck " + I2S(h[5]);
    }

    internal static void Lines(List<string> o)
    {
      o.Add("order latency (0.46, game time from order to moving, idle units): all " + Hist(All));
      for (var i = 0; i < _windows.Count; i++)
      {
        o.Add("  " + _windows[i]);
      }
    }
  }

  // 0.46: what the armies do and whom the bots attack (TODO 0.46, exit criteria 3-4).
  // Activity: every sampled army unit (AiArmy.SampleStall, once per service) is walking (moved >= 100), hurt (lost
  // life: in a fight), idle (no order) or stalled (an order, not moving, not hurt -- ranged units shooting unhurt count
  // here).
  // Returns: a bot takes a target again that it left less than 5 min ago (A -> B -> A) -- the flapping that matters;
  // plain switches are expected early. Targets by owner: who attacks whom (0.45.0: who attacked Quel'thalas?).
  internal static class AiObserve
  {
    internal static int ActWalk;
    internal static int ActHurt;
    internal static int ActIdle;
    internal static int ActStall;
    internal static int[] Returns = new int[24];
    private const int Memory = 3;
    private const float ReturnWindow = 300f;
    private static unit[] _left = new unit[24 * Memory];
    private static float[] _leftAt = new float[24 * Memory];
    private static int[] _leftNext = new int[24];
    private static int[] _byOwner = new int[24 * 26]; // [bot][owner 0..23, 24 = neutral, 25 = itself]

    internal static void Target(int pid, unit from, unit to)
    {
      if (pid < 0 || pid >= 24 || to == null)
      {
        return;
      }

      var now = SimpleBot.Clock;
      for (var k = 0; k < Memory; k++)
      {
        var i = pid * Memory + k;
        if (_left[i] == to && now - _leftAt[i] < ReturnWindow)
        {
          Returns[pid]++;
          _left[i] = null;
          break;
        }
      }

      if (from != null)
      {
        var slot = pid * Memory + _leftNext[pid];
        _left[slot] = from;
        _leftAt[slot] = now;
        _leftNext[pid] = (_leftNext[pid] + 1) % Memory;
      }

      var oid = GetPlayerId(GetOwningPlayer(to));
      var col = oid == pid ? 25 : (oid >= 24 ? 24 : oid);
      _byOwner[pid * 26 + col] = _byOwner[pid * 26 + col] + 1;
    }

    internal static void Lines(List<string> o)
    {
      var total = ActWalk + ActHurt + ActIdle + ActStall;
      o.Add("activity (0.46, sampled army units): walking " + Pct(ActWalk, total) + " fighting " + Pct(ActHurt, total)
            + " idle " + Pct(ActIdle, total) + " stalled " + Pct(ActStall, total) + " of " + I2S(total));
      var r = "target returns within 5 min (A->B->A) per bot:";
      var t = "targets by owner (since start; N = neutral, own = defending itself):";
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (pid < 0 || pid >= 24)
        {
          continue;
        }

        r = r + " P" + I2S(pid) + " " + I2S(Returns[pid]) + ";";
        var row = "";
        for (var col = 0; col < 26; col++)
        {
          var n = _byOwner[pid * 26 + col];
          if (n > 0)
          {
            row = row + " " + (col == 25 ? "own" : (col == 24 ? "N" : "P" + I2S(col))) + "x" + I2S(n);
          }
        }

        if (row != "")
        {
          t = t + " P" + I2S(pid) + ":" + row + ";";
        }
      }

      o.Add(r);
      o.Add(t);
    }

    private static string Pct(int a, int total)
    {
      return total > 0 ? I2S(R2I(100f * a / total)) + "%" : "-";
    }
  }

  // 0.46: caster share cap. 0.45.0: 94 % of Quel'thalas' 5102 units were casters (Sorceress, Priest, Spell Breaker) --
  // without autocast they are weak. 0.46.1: 35 % (was 25 %) -- Quel'thalas is a caster faction (maintainer). A type is a caster when a trained unit of it has mana (learned from the units the
  // production pass sees; heroes excluded). While casters are MaxShare or more of a bot's units, no caster is queued.
  internal static class AiCaster
  {
    internal static bool On = true;
    private const float MaxShare = 0.35f;
    private static Dictionary<int, bool> _isCaster = new();
    internal static int[] Skipped = new int[24];

    // production pass, for every living non-hero unit of the bot; true = a caster
    internal static bool Learn(unit u)
    {
      var t = GetUnitTypeId(u);
      if (!_isCaster.ContainsKey(t))
      {
        _isCaster[t] = BlzGetUnitMaxMana(u) > 0;
      }

      return _isCaster[t];
    }

    internal static bool IsCaster(int typeId) => _isCaster.ContainsKey(typeId) && _isCaster[typeId];

    internal static bool TooMany(int casters, int units)
    {
      return On && units >= 20 && casters >= units * MaxShare;
    }

    internal static string Line()
    {
      var s = "caster cap " + (On ? I2S(R2I(MaxShare * 100f)) + "%" : "off") + ", caster orders skipped per bot:";
      var bots = SimpleBot.Bots;
      for (var b = 0; b < bots.Count; b++)
      {
        var pid = GetPlayerId(bots[b]);
        if (pid >= 0 && pid < 24 && Skipped[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(Skipped[pid]) + ";";
        }
      }

      return s;
    }
  }

  // dump: one block in LOAD
  internal static class AiLoad
  {
    internal static void Setup()
    {
      AiGuard.Setup();
      AiProbe.Setup();
      AiStyleCheck.Run(); // 0.46: CSharp.lua check for the phase-2 style (removed in 0.47)
    }

    internal static void Lines(List<string> o)
    {
      AiProbe.Lines(o);
      o.Add(AiBudget.Line());
      o.Add(AiGuard.Line());
      o.Add("attack groups: units left out (no way to the target) " + I2S(AiArmy.Unreachable));
      o.Add(AiCaster.Line());
      AiObserve.Lines(o);
      o.Add("style check (0.46): " + AiStyleCheck.Result);
    }
  }
}
