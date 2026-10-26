using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.40: one periodic task of the scheduler, with its load statistics.
  internal sealed class AiTask
  {
    public string Name = "";
    public int Period;          // in slots (0.25 s)
    public int Phase;           // runs in the slots where (slot - Phase) % Period == 0, from slot Period on
    public System.Action Run = () => { };
    public bool Deferrable;     // may slide to a later slot when the budget is enforced (never combat / production)
    public bool Wave;           // part of the 15-s wave cycle: skipped while -botbreak is on (as the old wave tick)
    public int Runs;
    public int LastOps;
    public int MaxOps;
    public int SumOps;
  }

  // 0.40 (refactor phase 1): ONE timer for all periodic bot work. Each task has a period and a fixed phase, so the
  // work is spread over the slots deterministically (same on every client) instead of several timers firing in the
  // same frame (every 30 s: combat, production, Legion check, quests and the whole 15-s wave tick at once).
  // Load = deterministic ops only (units enumerated + production checks + wave work + orders issued) -- never
  // wall-clock time, that would desync. Work done in event triggers (train finish, level-up ...) is not counted.
  // Budget: report-only by default; "-budget on" lets deferrable tasks slide to the next slot after an over-budget
  // slot (test switch). Debug output (-debug, -autodump, heartbeat) keeps its own timers on purpose: the heartbeat
  // must keep writing even if this scheduler breaks.
  internal static class AiScheduler
  {
    internal const float SlotSeconds = 0.25f;
    internal const int CycleSlots = 60;  // 15 s: the old wave tick
    internal const int Budget = 400;     // ops per slot -- first guess, calibrate from the LOAD numbers

    internal static List<AiTask> Tasks = new();
    private static List<AiTask> _carry = new();
    private static bool _started;
    internal static int SlotNo;
    internal static bool Enforce;
    internal static int LastSlotOps;
    internal static int MaxSlotOps;
    internal static int SlotsOver;
    internal static int WouldDefer;
    internal static int DeferredRuns;
    internal static int WaveCycleOps;

    // orders by distance unit -> target point (a proxy for engine pathing cost) and idle re-orders of units that
    // already stand at their target (lever 1's hole, fixed in 0.41)
    internal static int OrdNear;
    internal static int OrdMid;
    internal static int OrdFar;
    internal static int OrdIdleAtTarget;

    internal static AiTask Add(string name, int periodSlots, int phase, System.Action run, bool deferrable)
    {
      var t = new AiTask();
      t.Name = name;
      t.Period = periodSlots < 1 ? 1 : periodSlots;
      t.Phase = phase % t.Period;
      t.Run = run;
      t.Deferrable = deferrable;
      Tasks.Add(t);
      return t;
    }

    // A subtask of the 15-s wave cycle; index k runs in odd slot 2k+1 of the cycle (k = 0..29), in this order.
    internal static void AddWave(int k, string name, System.Action run)
    {
      var t = Add(name, CycleSlots, 2 * k + 1, run, true);
      t.Wave = true;
    }

    internal static void Start()
    {
      if (_started)
      {
        return;
      }

      _started = true;
      var tm = CreateTimer();
      TimerStart(tm, SlotSeconds, true, OnSlot);
    }

    private static void OnSlot()
    {
      SlotNo++;
      var afterOver = LastSlotOps > Budget;
      var slotOps = 0;
      if (Enforce && !afterOver && _carry.Count > 0)
      {
        var carried = _carry;
        _carry = new List<AiTask>();
        for (var i = 0; i < carried.Count; i++)
        {
          slotOps += RunTask(carried[i]);
        }
      }

      for (var i = 0; i < Tasks.Count; i++)
      {
        var t = Tasks[i];
        if (SlotNo < t.Period || (SlotNo - t.Phase) % t.Period != 0)
        {
          continue;
        }

        if (t.Deferrable && afterOver)
        {
          WouldDefer++;
          if (Enforce)
          {
            if (!_carry.Contains(t))
            {
              _carry.Add(t);
              DeferredRuns++;
            }

            continue;
          }
        }

        slotOps += RunTask(t);
      }

      LastSlotOps = slotOps;
      if (slotOps > MaxSlotOps)
      {
        MaxSlotOps = slotOps;
      }

      if (slotOps > Budget)
      {
        SlotsOver++;
      }
    }

    private static int RunTask(AiTask t)
    {
      if (t.Wave && SimpleBot.BotBreak)
      {
        return 0;
      }

      SimpleBot.OpUnits = 0;
      SimpleBot.WaveOps = 0;
      SimpleBot.ProdOps = 0;
      var orders0 = SimpleBot.OrdersIssued;
      SimpleBot.Safe(t.Name, t.Run);
      var ops = SimpleBot.OpUnits + SimpleBot.WaveOps + SimpleBot.ProdOps + (SimpleBot.OrdersIssued - orders0);
      t.Runs++;
      t.LastOps = ops;
      t.SumOps += ops;
      if (ops > t.MaxOps)
      {
        t.MaxOps = ops;
      }

      if (t.Wave)
      {
        WaveCycleOps += ops;
      }

      return ops;
    }

    // first wave subtask of a cycle (SimpleBot.BeginWaveCycle) and the last one (EndWaveCycle) use these
    internal static int TakeWaveCycleOps()
    {
      var n = WaveCycleOps;
      WaveCycleOps = 0;
      return n;
    }

    // called right before an attack order: distance bucket + "idle and already at the target"
    internal static void CountOrder(unit u, float tx, float ty)
    {
      var dx = GetUnitX(u) - tx;
      var dy = GetUnitY(u) - ty;
      var d2 = dx * dx + dy * dy;
      if (d2 < 2000f * 2000f)
      {
        OrdNear++;
        if (d2 < 300f * 300f && GetUnitCurrentOrder(u) == 0)
        {
          OrdIdleAtTarget++;
        }
      }
      else if (d2 < 8000f * 8000f)
      {
        OrdMid++;
      }
      else
      {
        OrdFar++;
      }
    }

    // ---- readouts (-debug2, dump section LOAD, -budget) --------------------------------------------------------

    internal static string ShortLine()
    {
      return "slot max=" + I2S(MaxSlotOps) + " over=" + I2S(SlotsOver) + " defer=" + I2S(WouldDefer)
             + (Enforce ? "(ON)" : "(off)") + " | orders far=" + I2S(OrdFar) + " idle@target=" + I2S(OrdIdleAtTarget)
             + " | top " + TopTasks(3);
    }

    internal static void Lines(List<string> o)
    {
      o.Add("scheduler slot=0.25s cycle=15s budget=" + I2S(Budget) + " enforce=" + (Enforce ? "on" : "off")
            + " slots=" + I2S(SlotNo) + " max-slot=" + I2S(MaxSlotOps) + " over=" + I2S(SlotsOver)
            + " would-defer=" + I2S(WouldDefer) + " deferred=" + I2S(DeferredRuns));
      o.Add("orders total=" + I2S(SimpleBot.OrdersIssued) + " near(<2k)=" + I2S(OrdNear) + " mid(2-8k)=" + I2S(OrdMid)
            + " far(>8k)=" + I2S(OrdFar) + " idle-at-target(<300)=" + I2S(OrdIdleAtTarget));
      var sorted = Sorted();
      for (var i = 0; i < sorted.Count; i++)
      {
        var t = sorted[i];
        o.Add("task " + t.Name + " every=" + R2S(t.Period * SlotSeconds) + "s runs=" + I2S(t.Runs) + " last="
              + I2S(t.LastOps) + " max=" + I2S(t.MaxOps) + " sum=" + I2S(t.SumOps));
      }
    }

    private static string TopTasks(int n)
    {
      var sorted = Sorted();
      var s = "";
      for (var i = 0; i < sorted.Count && i < n; i++)
      {
        s = s + (i > 0 ? ", " : "") + sorted[i].Name + " " + I2S(sorted[i].MaxOps);
      }

      return s;
    }

    // by total ops, highest first (display only)
    private static List<AiTask> Sorted()
    {
      var rest = new List<AiTask>(Tasks);
      var res = new List<AiTask>();
      while (rest.Count > 0)
      {
        var best = 0;
        for (var i = 1; i < rest.Count; i++)
        {
          if (rest[i].SumOps > rest[best].SumOps)
          {
            best = i;
          }
        }

        res.Add(rest[best]);
        rest.RemoveAt(best);
      }

      return res;
    }

    // "-budget on" / "-budget off" (test switch, Control level)
    internal static void OnBudgetCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg.Length >= 10 && msg.Substring(0, 10) == "-budget on")
      {
        Enforce = true;
      }
      else if (msg.Length >= 11 && msg.Substring(0, 11) == "-budget off")
      {
        Enforce = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "load budget " + I2S(Budget) + " ops per slot: "
        + (Enforce ? "ENFORCED (tasks slide after a busy slot)" : "report only") + "  -- -budget on|off");
    }
  }
}
