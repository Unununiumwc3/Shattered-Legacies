using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.47: THEATRES (design: CONCEPTS.md 6). A bot's army can stand on several separate fronts -- a theatre is a
  // walking component of the route graph (with the live rock state, without portals): Kalimdor, the Eastern Kingdoms
  // (+ Northrend once its rocks fall), Outland, island groups. Up to MaxTheatres per bot, each with its own target,
  // route, staging point, attack group and defence decision -- the same planner and army code, with its own STATE
  // SLOT: slot = player id + 24 x theatre (theatre 0 = the main one = the bot's old slot). AiRoutes, AiArmy,
  // AiDefense and the order throttle index their state by S(pid) = pid + Offset; Offset is set only while a bot's
  // secondary theatre is serviced (SimpleBot.Process -> ProcessTheatres), 0 everywhere else.
  // Before: one target and one group per bot -- a defence at home pulled the army back across the sea, units on
  // another continent walked to the one target, fronts were abandoned (Druids, Fel Horde, Kul Tiras, 0.45-0.46).
  // - A component becomes a theatre with at least OpenFood of army food; it stays one while it has KeepFood.
  // - The main theatre (slot 0) follows the biggest army; it switches only to a component with 2x its food.
  // - Units in components without a theatre belong to the main theatre (they route to it, through portals).
  // - The main theatre does not target components that have their own theatre; a secondary theatre targets only its
  //   own component and does not use portals.
  internal static class AiTheatres
  {
    internal const int MaxTheatres = 3;
    internal const int Agents = 24 * MaxTheatres;
    // 0.48: relative to the bot's whole army (0.47.0: 30 / 15 food split strong armies early -- Scourge, Legion and
    // the Old Gods were weaker): open at max(40, 25 %), keep down to max(20, 10 %)
    private const int OpenFood = 40;
    private const int KeepFood = 20;
    private const int OpenShare = 4;   // 1/4 of the army
    private const int KeepShare = 10;  // 1/10 of the army
    internal static bool On = true;

    internal static int Offset;          // 24 x theatre while a secondary theatre is serviced, else 0
    private static int _curPid = -1;     // the bot being serviced (SimpleBot.Process), -1 = none
    private static int _restrictComp = -1;
    private static int[] _slotComp = new int[Agents];       // [pid + 24 t] -> component, -1 = none
    private static int[] _slotFood = new int[Agents];
    private static float[] _slotX = new float[Agents];
    private static float[] _slotY = new float[Agents];
    private static string[] _slotTarget = new string[Agents];
    private static List<unit>[] _slotUnits = new List<unit>[Agents];
    private static BotDiag[] _diag = new BotDiag[Agents];
    internal static int Opened;
    internal static int Closed;
    internal static int MainSwitches;
    private static bool _init;
    private static bool[] _mainIdle = new bool[24];
    private static int[] _doneComp = new int[24];     // land whose targets are all taken (main moved away from it)
    private static float[] _doneUntil = new float[24]; // ...not a theatre again before this game time
    private const float DoneFor = 180f;

    // the main theatre's land has no target left (all taken): next service it becomes the biggest other theatre --
    // its units follow (they are "without a theatre" then and route there, through portals)
    // 0.48.1: only after NothingStreak services in a row (~30 s) -- one empty plan moved the whole front (0.48.0:
    // "main moved" 36 times in 25 min, armies walking between fronts)
    private const int NothingStreak = 4;
    private static int[] _nothing = new int[24];
    private const float SwitchAfter = 60f;
    private static int[] _switchTo = new int[24];       // the component the main theatre may switch to
    private static float[] _switchSince = new float[24];

    internal static void MainFoundTarget(int pid)
    {
      if (pid >= 0 && pid < 24)
      {
        _nothing[pid] = 0;
      }
    }

    internal static void MainFoundNothing(int pid)
    {
      if (pid < 0 || pid >= 24)
      {
        return;
      }

      _nothing[pid]++;
      if (_nothing[pid] < NothingStreak)
      {
        return;
      }

      _nothing[pid] = 0;

      for (var t = 1; t < MaxTheatres; t++)
      {
        if (_slotComp[pid + 24 * t] >= 0)
        {
          _mainIdle[pid] = true;
          return;
        }
      }
    }

    internal static int S(int pid) => pid + Offset;

    private static void Init()
    {
      if (_init)
      {
        return;
      }

      _init = true;
      for (var i = 0; i < Agents; i++)
      {
        _slotComp[i] = -1;
        _slotTarget[i] = "-";
      }

      for (var i = 0; i < 24; i++)
      {
        _switchTo[i] = -1;
      }
    }

    // SimpleBot.Process, after the army pass: split AiArmy.Units into theatres. Afterwards AiArmy.Units holds the
    // main theatre's units (+ units without a theatre); true = (mx, my) is the main theatre's centre.
    internal static bool Split(int pid, out float mx, out float my)
    {
      Init();
      mx = 0f;
      my = 0f;
      _curPid = pid;
      Offset = 0;
      _restrictComp = -1;
      if (!On || !AiRoutes.On || pid < 0 || pid >= 24)
      {
        for (var t = 1; t < MaxTheatres; t++)
        {
          Release(pid, t);
        }

        return false;
      }

      // food per component
      var all = AiArmy.Units;
      var n = all.Count;
      var comp = new int[n];
      var food = new Dictionary<int, int>();
      for (var i = 0; i < n; i++)
      {
        comp[i] = AiRoutes.CompAt(GetUnitX(all[i]), GetUnitY(all[i]));
        if (comp[i] < 0)
        {
          continue;
        }

        var f = GetUnitFoodUsed(all[i]);
        if (f < 1)
        {
          f = 1;
        }

        var prev = 0;
        if (food.ContainsKey(comp[i]))
        {
          prev = food[comp[i]];
        }

        food[comp[i]] = prev + f; // no ternary in an indexer assignment (hard rule 16)
      }

      var total = 0;
      foreach (var kv in food)
      {
        total += kv.Value;
      }

      var openAt = total / OpenShare > OpenFood ? total / OpenShare : OpenFood;
      var keepAt = total / KeepShare > KeepFood ? total / KeepShare : KeepFood;

      // main theatre: keep its component while it holds KeepFood; follow a component with twice the food
      var main = _slotComp[pid];
      var mainFood = main >= 0 && food.ContainsKey(main) ? food[main] : 0;
      var biggest = -1;
      var biggestFood = 0;
      foreach (var kv in food)
      {
        if (kv.Value > biggestFood || (kv.Value == biggestFood && kv.Key < biggest))
        {
          biggest = kv.Key;
          biggestFood = kv.Value;
        }
      }

      if (_mainIdle[pid])
      {
        _mainIdle[pid] = false;
        var next = -1;
        var nextFood = -1;
        for (var t = 1; t < MaxTheatres; t++)
        {
          var c = _slotComp[pid + 24 * t];
          var cf = c >= 0 && food.ContainsKey(c) ? food[c] : 0;
          if (c >= 0 && cf > nextFood)
          {
            next = c;
            nextFood = cf;
          }
        }

        if (next >= 0)
        {
          SimpleBot.Log("P" + I2S(pid) + ": main theatre done in " + AiRoutes.CompName(main) + " -- moves to "
                        + AiRoutes.CompName(next));
          _doneComp[pid] = main;
          _doneUntil[pid] = SimpleBot.Clock + DoneFor;
          main = next;
          mainFood = nextFood;
          MainSwitches++;
        }
      }

      // a "done" land does not pull the main theatre back (its units walk to the new main front)
      if (_doneUntil[pid] > SimpleBot.Clock && biggest == _doneComp[pid])
      {
        biggest = main;
        biggestFood = mainFood;
      }

      // 0.48.2: the "twice the food" switch only after it held SwitchAfter seconds -- armies walking through a portal
      // shift the food for a moment, and every switch changed the front's target (0.48.1: "main moved" 47 times in
      // 25 min -- a source of the remaining back and forth)
      var twice = biggest >= 0 && biggest != main && biggestFood >= 2 * mainFood;
      if (twice && _switchTo[pid] != biggest)
      {
        _switchTo[pid] = biggest;
        _switchSince[pid] = SimpleBot.Clock;
      }
      else if (!twice)
      {
        _switchTo[pid] = -1;
      }

      var held = twice && SimpleBot.Clock - _switchSince[pid] >= SwitchAfter;
      if (main < 0 || mainFood < KeepFood || held)
      {
        if (main >= 0 && biggest != main)
        {
          MainSwitches++;
        }

        main = biggest;
        _switchTo[pid] = -1;
      }

      _slotComp[pid] = main;

      // secondary theatres: keep while KeepFood; open new ones from OpenFood (biggest first)
      for (var t = 1; t < MaxTheatres; t++)
      {
        var s = pid + 24 * t;
        var c = _slotComp[s];
        if (c >= 0 && (c == main || !food.ContainsKey(c) || food[c] < keepAt))
        {
          Release(pid, t);
        }
      }

      for (var t = 1; t < MaxTheatres; t++)
      {
        var s = pid + 24 * t;
        if (_slotComp[s] >= 0)
        {
          continue;
        }

        var pick = -1;
        var pickFood = 0;
        foreach (var kv in food)
        {
          if (kv.Key == main || kv.Value < openAt || kv.Value < pickFood || Taken(pid, kv.Key)
              || (_doneUntil[pid] > SimpleBot.Clock && kv.Key == _doneComp[pid]))
          {
            continue;
          }

          if (kv.Value > pickFood || kv.Key < pick)
          {
            pick = kv.Key;
            pickFood = kv.Value;
          }
        }

        if (pick >= 0)
        {
          _slotComp[s] = pick;
          Opened++;
          SimpleBot.Log("P" + I2S(pid) + ": theatre " + I2S(t + 1) + " opened -- " + AiRoutes.CompName(pick) + " ("
                        + I2S(pickFood) + " food)");
        }
      }

      // distribute the units
      var mainUnits = new List<unit>();
      float sx = 0f, sy = 0f;
      var sn = 0;
      for (var t = 1; t < MaxTheatres; t++)
      {
        var s = pid + 24 * t;
        if (_slotUnits[s] == null)
        {
          _slotUnits[s] = new List<unit>();
        }

        _slotUnits[s].Clear();
        _slotX[s] = 0f;
        _slotY[s] = 0f;
        _slotFood[s] = 0;
      }

      for (var i = 0; i < n; i++)
      {
        var u = all[i];
        var target = -1;
        for (var t = 1; t < MaxTheatres && comp[i] >= 0; t++)
        {
          if (_slotComp[pid + 24 * t] == comp[i])
          {
            target = pid + 24 * t;
          }
        }

        if (target < 0)
        {
          mainUnits.Add(u);
          if (comp[i] == main)
          {
            sx += GetUnitX(u);
            sy += GetUnitY(u);
            sn++;
          }

          continue;
        }

        _slotUnits[target].Add(u);
        _slotX[target] += GetUnitX(u);
        _slotY[target] += GetUnitY(u);
        _slotFood[target] += GetUnitFoodUsed(u);
      }

      for (var t = 1; t < MaxTheatres; t++)
      {
        var s = pid + 24 * t;
        var cnt = _slotUnits[s].Count;
        if (cnt > 0)
        {
          _slotX[s] /= cnt;
          _slotY[s] /= cnt;
        }
      }

      all.Clear();
      all.AddRange(mainUnits);
      _slotFood[pid] = main >= 0 && food.ContainsKey(main) ? food[main] : 0;
      if (sn > 0)
      {
        mx = sx / sn;
        my = sy / sn;
        _slotX[pid] = mx;
        _slotY[pid] = my;
        return true;
      }

      return false;
    }

    private static bool Taken(int pid, int comp)
    {
      for (var t = 0; t < MaxTheatres; t++)
      {
        if (_slotComp[pid + 24 * t] == comp)
        {
          return true;
        }
      }

      return false;
    }

    private static void Release(int pid, int t)
    {
      var s = pid + 24 * t;
      if (_slotComp[s] < 0)
      {
        return;
      }

      SimpleBot.Log("P" + I2S(pid) + ": theatre " + I2S(t + 1) + " closed -- " + AiRoutes.CompName(_slotComp[s]));
      _slotComp[s] = -1;
      _slotTarget[s] = "-";
      if (_slotUnits[s] != null)
      {
        _slotUnits[s].Clear();
      }

      Closed++;
    }

    // ---- servicing a secondary theatre (SimpleBot.ProcessTheatres) --------------------------------------------

    internal static bool Has(int pid, int t) => _init && pid >= 0 && pid < 24 && _slotComp[pid + 24 * t] >= 0
                                                && _slotUnits[pid + 24 * t] != null
                                                && _slotUnits[pid + 24 * t].Count > 0;

    internal static float X(int pid, int t) => _slotX[pid + 24 * t];
    internal static float Y(int pid, int t) => _slotY[pid + 24 * t];

    internal static void Begin(int pid, int t)
    {
      Offset = 24 * t;
      _restrictComp = _slotComp[pid + Offset];
      AiArmy.Units.Clear();
      AiArmy.Units.AddRange(_slotUnits[pid + Offset]);
    }

    internal static void End()
    {
      Offset = 0;
      _restrictComp = -1;
    }

    // after the whole Process: no restriction for code outside a bot's service (chains from triggers, dumps)
    internal static void Done()
    {
      Offset = 0;
      _restrictComp = -1;
      _curPid = -1;
    }

    internal static bool Secondary => Offset > 0;

    // AiRoutes.Plan / AiDefense: may the theatre being serviced target this component?
    internal static bool Allowed(int comp)
    {
      if (comp < 0)
      {
        return true;
      }

      if (Offset > 0)
      {
        return comp == _restrictComp;
      }

      if (_curPid < 0 || !_init)
      {
        return true;
      }

      for (var t = 1; t < MaxTheatres; t++)
      {
        if (_slotComp[_curPid + 24 * t] == comp && _slotUnits[_curPid + 24 * t] != null
            && _slotUnits[_curPid + 24 * t].Count > 0)
        {
          return false; // another theatre of this bot owns that front
        }
      }

      return true;
    }

    internal static BotDiag Diag(int pid, int t)
    {
      var s = pid + 24 * t;
      if (_diag[s] == null)
      {
        _diag[s] = new BotDiag();
      }

      return _diag[s];
    }

    internal static void Note(player p, int pid, int t, unit tgt, string why)
    {
      var s = pid + 24 * t;
      var name = tgt != null ? GetUnitName(tgt) : "-";
      if (_slotTarget[s] != name)
      {
        SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(p) + " [theatre " + I2S(t + 1) + "]: target "
                      + _slotTarget[s] + " -> " + name + " [" + why + "]");
        _slotTarget[s] = name;
      }
    }

    internal static void Lines(List<string> o)
    {
      o.Add("theatres (0.47; opened " + I2S(Opened) + ", closed " + I2S(Closed) + ", main moved " + I2S(MainSwitches)
            + "; one line per bot):");
      for (var pid = 0; pid < 24; pid++)
      {
        if (!_init || _slotComp[pid] < 0)
        {
          continue;
        }

        var s = "  P" + I2S(pid) + " main " + AiRoutes.CompName(_slotComp[pid]) + " " + I2S(_slotFood[pid]) + "f";
        for (var t = 1; t < MaxTheatres; t++)
        {
          var k = pid + 24 * t;
          if (_slotComp[k] >= 0)
          {
            s = s + " | T" + I2S(t + 1) + " " + AiRoutes.CompName(_slotComp[k]) + " " + I2S(_slotFood[k]) + "f -> "
                + _slotTarget[k];
          }
        }

        o.Add(s);
      }
    }
  }
}
