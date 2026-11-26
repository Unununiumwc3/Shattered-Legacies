using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Legends;

namespace WarcraftLegacies.Source.Ai
{
  // 0.79.0 (maintainer): EMERGENCY HQ TELEPORT. A bot whose headquarters is under attack while its army is far away (or
  // a human sniping its HQ / HQ CP) calls its troops home -- and its allied bots send help.
  // - HQ: the faction's registered headquarters (AiBuildSites.Headquarters, the timelines' Hq types) it owns; without
  //   one its own essential capitals; without those its biggest town hall (Twilight's Great Hall before Grim Batol).
  // - Under attack: the HQ is damaged and more than SimpleBot.DefendThreshold enemy-player units stand near it.
  // - Need: 1.5 x the attackers' food, minus the defenders' food already there. Own troops first, then each allied bot
  //   gives at most half of what it could send. Only units at least FarFrom away, not fighting (no enemy within
  //   FightRadius), not at sea, not on a mission; heroes come too.
  // - A visible 5-s warning (effects on every unit and at the HQ, a text to EVERY player, a ping): then the units stand
  //   beside the HQ, attack-move onto it, and stay out of the army logic for Hold seconds.
  // - Once per Cooldown per bot -- for its own HQ and, separately, as a helper. Bots with perks only (no autopilot
  //   in fair mode). "-recall on|off".
  internal sealed class AiRecallCall
  {
    internal int Pid;
    internal unit Hq;
    internal float At;
    internal List<unit> Units = new();
  }

  internal static class AiRecall
  {
    internal static bool On = true;
    private const float Cooldown = 180f;
    private const float Warning = 5f;
    private const float Hold = 45f;
    private const float Radius = 1500f;       // around the HQ: attackers / defenders
    private const float FarFrom = 3000f;      // nearer units walk (the normal defence)
    private const float FightRadius = 800f;   // an enemy this close = the unit is fighting, stays
    private const float NeedFactor = 1.5f;
    private const int MaxCandidates = 80;     // per player: units looked at (ops)
    private const string FxGo = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportTarget.mdl";
    private const string FxHq = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportCaster.mdl";
    private const string FxArrive = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportTo.mdl";

    private static readonly float[] _ownNext = new float[24];
    private static readonly float[] _helpNext = new float[24];
    private static readonly Dictionary<unit, float> _hold = new();
    private static readonly List<AiRecallCall> _calls = new();
    internal static int Calls;
    internal static int Moved;
    internal static int Helped;
    internal static string Last = "none";

    internal static void Setup()
    {
      AiScheduler.Add("recall", 8, 5, Tick, false); // every 2 s
    }

    internal static bool Holds(unit u)
    {
      if (!_hold.TryGetValue(u, out var until))
      {
        return false;
      }

      if (SimpleBot.Clock < until)
      {
        return true;
      }

      _hold.Remove(u);
      return false;
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

    private static void Tick()
    {
      var now = SimpleBot.Clock;
      for (var i = _calls.Count - 1; i >= 0; i--)
      {
        if (now >= _calls[i].At)
        {
          Arrive(_calls[i]);
          _calls.RemoveAt(i);
        }
      }

      if (!On)
      {
        return;
      }

      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var pid = GetPlayerId(p);
        if (pid < 0 || pid >= 24 || now < _ownNext[pid] || !SimpleBot.HasPerks(p) || p.GetPlayerData().Faction == null)
        {
          continue;
        }

        var hq = HqUnderAttack(p, out var need);
        if (hq != null)
        {
          Call(p, pid, hq, need);
        }
      }
    }

    // the faction's HQ (see the head comment), or null
    internal static List<unit> HqOf(player p)
    {
      var list = new List<unit>();
      var fac = SimpleBot.FacName(p);
      AiBuildSites.Headquarters.TryGetValue(fac, out var ids);
      if (ids != null)
      {
        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          var t = GetUnitTypeId(u);
          for (var k = 0; k < ids.Length; k++)
          {
            if (ids[k] == t && Alive(u))
            {
              list.Add(u);
            }
          }
        }

        DestroyGroup(g);
        if (list.Count > 0)
        {
          return list;
        }
      }

      var caps = CapitalManager.GetAll();
      for (var i = 0; i < caps.Count; i++)
      {
        var cu = caps[i].Unit;
        if (caps[i].Essential && Alive(cu) && GetOwningPlayer(cu) == p)
        {
          list.Add(cu);
        }
      }

      if (list.Count > 0)
      {
        return list;
      }

      unit hall = null;
      var g2 = CreateGroup();
      GroupEnumUnitsOfPlayer(g2, p, null);
      unit h;
      while ((h = FirstOfGroup(g2)) != null)
      {
        GroupRemoveUnit(g2, h);
        if (IsUnitType(h, UNIT_TYPE_TOWNHALL) && Alive(h)
            && (hall == null || GetUnitState(h, UNIT_STATE_MAX_LIFE) > GetUnitState(hall, UNIT_STATE_MAX_LIFE)))
        {
          hall = h;
        }
      }

      DestroyGroup(g2);
      if (hall != null)
      {
        list.Add(hall);
      }

      return list;
    }

    private static unit HqUnderAttack(player p, out int need)
    {
      need = 0;
      var hqs = HqOf(p);
      unit worst = null;
      for (var i = 0; i < hqs.Count; i++)
      {
        var hq = hqs[i];
        var max = GetUnitState(hq, UNIT_STATE_MAX_LIFE);
        if (max <= 0f || GetUnitState(hq, UNIT_STATE_LIFE) >= max * 0.97f)
        {
          continue; // not being hit
        }

        Count(p, GetUnitX(hq), GetUnitY(hq), out var enemies, out var enemyFood, out var ownFood);
        var n = R2I(enemyFood * NeedFactor) - ownFood;
        if (enemies > SimpleBot.DefendThreshold && n > need)
        {
          need = n;
          worst = hq;
        }
      }

      return worst;
    }

    // enemy-player units / their food and the defenders' food (p and its allies) near (x, y)
    private static void Count(player p, float x, float y, out int enemies, out int enemyFood, out int ownFood)
    {
      enemies = 0;
      enemyFood = 0;
      ownFood = 0;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, Radius, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!Alive(u) || IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON))
        {
          continue;
        }

        var owner = GetOwningPlayer(u);
        if (GetPlayerId(owner) >= 24)
        {
          continue;
        }

        var food = Food(u);
        if (owner == p || IsPlayerAlly(owner, p))
        {
          ownFood += food;
        }
        else if (IsPlayerEnemy(owner, p))
        {
          enemies++;
          enemyFood += food;
        }
      }

      DestroyGroup(g);
    }

    private static int Food(unit u)
    {
      var f = GetUnitFoodUsed(u);
      return f > 0 ? f : IsUnitType(u, UNIT_TYPE_HERO) ? 5 : 1;
    }

    private static void Call(player p, int pid, unit hq, int need)
    {
      var call = new AiRecallCall { Pid = pid, Hq = hq, At = SimpleBot.Clock + Warning };
      var hx = GetUnitX(hq);
      var hy = GetUnitY(hq);
      var got = Pick(p, hx, hy, need, call.Units);
      if (got <= 0 && need <= 0)
      {
        return;
      }

      var helpers = "";
      for (var b = 0; b < SimpleBot.Bots.Count && got < need; b++)
      {
        var q = SimpleBot.Bots[b];
        var qid = GetPlayerId(q);
        if (q == p || qid < 0 || qid >= 24 || SimpleBot.Clock < _helpNext[qid] || !SimpleBot.HasPerks(q)
            || q.GetPlayerData().Faction == null || !IsPlayerAlly(q, p) || !IsPlayerAlly(p, q))
        {
          continue;
        }

        var before = call.Units.Count;
        var gave = Pick(q, hx, hy, (need - got) < Spare(q, hx, hy) ? need - got : Spare(q, hx, hy), call.Units);
        if (gave > 0)
        {
          got += gave;
          _helpNext[qid] = SimpleBot.Clock + Cooldown;
          Helped += call.Units.Count - before;
          helpers += (helpers.Length > 0 ? ", " : "") + q.GetPlayerData().Faction.ColoredName;
        }
      }

      if (call.Units.Count == 0)
      {
        return; // nobody far away and free: the normal defence (walk) handles it
      }

      _ownNext[pid] = SimpleBot.Clock + Cooldown;
      Calls++;
      _calls.Add(call);
      for (var i = 0; i < call.Units.Count; i++)
      {
        _hold[call.Units[i]] = SimpleBot.Clock + Warning + Hold; // kept out of the army logic from now on
        DestroyEffect(AddSpecialEffectTarget(FxGo, call.Units[i], "origin"));
      }

      DestroyEffect(AddSpecialEffect(FxHq, hx, hy));
      var name = p.GetPlayerData().Faction.ColoredName;
      var text = name + " |cffffcc00calls its troops home|r to defend " + GetUnitName(hq)
                 + (helpers.Length > 0 ? " -- " + helpers + " send help" : "") + ".";
      for (var k = 0; k < 24; k++)
      {
        DisplayTimedTextToPlayer(Player(k), 0, 0, 10f, text); // every player (also the attackers: a fair warning)
      }

      PingMinimap(hx, hy, 3f);
      Last = SimpleBot.FacName(p) + " -> " + GetUnitName(hq) + ", need " + I2S(need) + " food, "
             + I2S(call.Units.Count) + " units" + (helpers.Length > 0 ? " (with allies)" : "") + " at "
             + I2S(R2I(SimpleBot.Clock)) + " s";
      SimpleBot.LogPin("recall: " + Last);
    }

    // what an allied bot can give: half of its free, far units' food
    private static int Spare(player q, float hx, float hy)
    {
      var tmp = new List<unit>();
      return Pick(q, hx, hy, 100000, tmp, true) / 2;
    }

    // adds free, far units of p until `need` food; returns the food added (dryRun: counts only)
    private static int Pick(player p, float hx, float hy, int need, List<unit> into, bool dryRun = false)
    {
      var got = 0;
      var looked = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (got >= need || looked >= MaxCandidates || !Alive(u) || IsUnitType(u, UNIT_TYPE_STRUCTURE)
            || IsUnitType(u, UNIT_TYPE_PEON) || IsUnitHidden(u) || IsUnitLoaded(u) || GetUnitMoveSpeed(u) <= 1f
            || AiFerries.InTransit(u) || AiFerries.IsBoat(u) || AiState.MissionUnits.Contains(u)
            || u == SimpleBot._ragHero || AiRitual.Owns(u) || _hold.ContainsKey(u) || into.Contains(u))
        {
          continue;
        }

        var dx = GetUnitX(u) - hx;
        var dy = GetUnitY(u) - hy;
        if (dx * dx + dy * dy < FarFrom * FarFrom)
        {
          continue;
        }

        looked++;
        if (Fighting(u))
        {
          continue;
        }

        got += Food(u);
        if (!dryRun)
        {
          into.Add(u);
        }
      }

      DestroyGroup(g);
      return got;
    }

    private static bool Fighting(unit u)
    {
      var p = GetOwningPlayer(u);
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, GetUnitX(u), GetUnitY(u), FightRadius, null);
      var fighting = false;
      unit e;
      while ((e = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, e);
        if (!fighting && Alive(e) && GetPlayerId(GetOwningPlayer(e)) < 24 && IsPlayerEnemy(GetOwningPlayer(e), p))
        {
          fighting = true;
        }
      }

      DestroyGroup(g);
      return fighting;
    }

    private static void Arrive(AiRecallCall call)
    {
      var hq = call.Hq;
      if (!Alive(hq))
      {
        for (var i = 0; i < call.Units.Count; i++)
        {
          _hold.Remove(call.Units[i]); // HQ gone: the army logic takes them back where they are
        }

        return;
      }

      var hx = GetUnitX(hq);
      var hy = GetUnitY(hq);
      for (var i = 0; i < call.Units.Count; i++)
      {
        var u = call.Units[i];
        if (!Alive(u) || IsUnitHidden(u) || IsUnitLoaded(u))
        {
          _hold.Remove(u);
          continue;
        }

        var a = GetRandomReal(0f, 6.2832f);
        var r = GetRandomReal(300f, 750f);
        SetUnitPosition(u, hx + r * Cos(a), hy + r * Sin(a)); // the engine picks the nearest free spot
        DestroyEffect(AddSpecialEffectTarget(FxArrive, u, "origin"));
        IssuePointOrderById(u, 851983, hx, hy); // attack-move onto the HQ
        _hold[u] = SimpleBot.Clock + Hold;
        Moved++;
      }
    }

    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg == "-recall on")
      {
        On = true;
      }
      else if (msg == "-recall off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "bot HQ recall: " + (On ? "ON" : "OFF")
        + "  -- -recall on|off");
    }

    internal static void Lines(List<string> o)
    {
      o.Add("recall (0.79.0) " + (On ? "on" : "off") + ": calls " + I2S(Calls) + ", units moved " + I2S(Moved)
            + " (allies' " + I2S(Helped) + "), held now " + I2S(_hold.Count) + "; last: " + Last);
    }
  }
}
