using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.70.0: the spell table at runtime -- AiSpellData (generated, WL) plus rows a fork adds (AiApi.AddSpellData).
  // A row = one Channel ability; RowsOf(unit type) = the rows a unit type carries (from the generated carrier list).
  internal static class AiSpellBook
  {
    internal static List<int> Id = new();
    internal static List<int> Kind = new();    // 1 unit target, 2 point target, 3 no target
    internal static List<int> Who = new();     // 1 enemy, 2 ally, 3 self
    internal static List<int> Min = new();     // least worth inside the area; 0 = default
    internal static List<int> Radius = new();  // area used to count targets; 0 = the ability's own area
    internal static List<int> Flags = new();   // 1 mana targets, 2 hurt allies only, 4 ground only, 8 organic only
    internal static List<int> OrderNo = new(); // OrderId(order string); 0 = unknown order, never cast
    private static Dictionary<int, List<int>> _rows = new();
    private static bool _loaded;

    internal static void Ensure()
    {
      if (_loaded)
      {
        return;
      }

      _loaded = true;
      AddRows(AiSpellData.Id, AiSpellData.Kind, AiSpellData.Who, AiSpellData.Min, AiSpellData.Radius,
        AiSpellData.Flags, AiSpellData.Order, AiSpellData.CarrierType, AiSpellData.CarrierRow);
    }

    // AiApi.AddSpellData: rows of a fork; an ability already in the table is replaced
    internal static void Add(int[] id, int[] kind, int[] who, int[] min, int[] radius, int[] flags, string[] order,
      int[] carrierType, int[] carrierRow)
    {
      Ensure();
      AddRows(id, kind, who, min, radius, flags, order, carrierType, carrierRow);
    }

    private static void AddRows(int[] id, int[] kind, int[] who, int[] min, int[] radius, int[] flags, string[] order,
      int[] carrierType, int[] carrierRow)
    {
      var global = new int[id.Length];
      for (var i = 0; i < id.Length; i++)
      {
        var row = Id.IndexOf(id[i]);
        if (row < 0)
        {
          row = Id.Count;
          Id.Add(id[i]);
          Kind.Add(0);
          Who.Add(0);
          Min.Add(0);
          Radius.Add(0);
          Flags.Add(0);
          OrderNo.Add(0);
        }

        Kind[row] = kind[i];
        Who[row] = who[i];
        Min[row] = min[i];
        Radius[row] = radius[i];
        Flags[row] = flags[i];
        OrderNo[row] = OrderId(order[i]);
        global[i] = row;
      }

      for (var k = 0; k < carrierType.Length && k < carrierRow.Length; k++)
      {
        var local = carrierRow[k];
        if (local < 0 || local >= global.Length)
        {
          continue;
        }

        if (!_rows.TryGetValue(carrierType[k], out var list))
        {
          list = new List<int>();
          _rows[carrierType[k]] = list;
        }

        if (!list.Contains(global[local]))
        {
          list.Add(global[local]);
        }
      }
    }

    internal static bool Has(int unitType) => _rows.ContainsKey(unitType);

    internal static List<int> RowsOf(int unitType) => _rows.TryGetValue(unitType, out var list) ? list : null;

    internal static int Count => Id.Count;
  }

  // 0.70.0: "BIG FIGHT" gate and the units around a caster. One unit search (radius Reach) per map cell, kept for
  // Keep seconds and shared by every caster standing in that cell; the gate result is kept per cell and bot.
  // Enemy = an enemy player's unit or Neutral Hostile (as AiArmy.Need), visible to the bot and not invulnerable;
  // friendly = own and allied units. Structures do not count; a hero counts 3.
  internal static class AiSpellFight
  {
    internal const float Reach = 2000f;
    internal const float Cell = 1000f;
    internal const float Keep = 3f;
    internal const int NeedEnemy = 5;
    internal const int NeedFriend = 5;
    private const float Shift = 40000f; // map coordinates are within +-33000
    private static Dictionary<int, List<unit>> _cell = new();
    private static Dictionary<int, float> _cellAt = new();
    private static Dictionary<int, bool> _gate = new(); // key cell * 24 + pid
    private static Dictionary<int, float> _gateAt = new();
    internal static int Scans;
    internal static int Reused;
    internal static int GateYes;
    internal static int GateNo;

    private static int CellX(float x) => R2I((x + Shift) / Cell);

    private static int CellKey(float x, float y) => CellX(x) * 100 + CellX(y);

    // every unit within Reach of the centre of (x, y)'s cell: alive, no structure, not hidden
    internal static List<unit> Around(float x, float y)
    {
      var key = CellKey(x, y);
      if (_cell.TryGetValue(key, out var list) && SimpleBot.Clock - _cellAt[key] < Keep)
      {
        Reused++;
        return list;
      }

      if (_cell.Count > 300)
      {
        _cell.Clear(); // old cells pile up -- start over now and then
        _cellAt.Clear();
        _gate.Clear();
        _gateAt.Clear();
      }

      list = new List<unit>();
      var cx = (CellX(x) + 0.5f) * Cell - Shift;
      var cy = (CellX(y) + 0.5f) * Cell - Shift;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, cx, cy, Reach, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.OpUnits++;
        if (Usable(u))
        {
          list.Add(u);
        }
      }

      DestroyGroup(g);
      _cell[key] = list;
      _cellAt[key] = SimpleBot.Clock;
      Scans++;
      return list;
    }

    internal static bool Usable(unit t)
    {
      return GetUnitTypeId(t) != 0 && GetUnitState(t, UNIT_STATE_LIFE) > 0.405f
             && !IsUnitType(t, UNIT_TYPE_STRUCTURE) && !IsUnitHidden(t);
    }

    internal static bool Enemy(unit t, player p)
    {
      var owner = GetOwningPlayer(t);
      var oid = GetPlayerId(owner);
      return oid == 24 || (oid < 24 && owner != p && !IsUnitAlly(t, p) && IsPlayerEnemy(owner, p));
    }

    // an enemy the bot may target: visible to it, not invulnerable
    internal static bool Foe(unit t, player p)
    {
      return Enemy(t, p) && IsUnitVisible(t, p) && !BlzIsUnitInvulnerable(t);
    }

    internal static bool Friend(unit t, player p)
    {
      var owner = GetOwningPlayer(t);
      var oid = GetPlayerId(owner);
      return oid < 24 && (owner == p || IsUnitAlly(t, p));
    }

    internal static int Worth(unit t) => IsUnitType(t, UNIT_TYPE_HERO) ? 3 : 1;

    internal static bool Big(unit caster, player p, int pid)
    {
      var x = GetUnitX(caster);
      var y = GetUnitY(caster);
      var gk = CellKey(x, y) * 24 + pid;
      if (_gate.TryGetValue(gk, out var ok) && SimpleBot.Clock - _gateAt[gk] < Keep)
      {
        return ok;
      }

      var list = Around(x, y);
      var enemy = 0;
      var friend = 0;
      for (var i = 0; i < list.Count; i++)
      {
        var t = list[i];
        if (!Usable(t))
        {
          continue;
        }

        if (Foe(t, p))
        {
          enemy += Worth(t);
        }
        else if (Friend(t, p))
        {
          friend += Worth(t);
        }
      }

      ok = enemy >= NeedEnemy && friend >= NeedFriend;
      _gate[gk] = ok;
      _gateAt[gk] = SimpleBot.Clock;
      if (ok)
      {
        GateYes++;
      }
      else
      {
        GateNo++;
      }

      return ok;
    }
  }

  // 0.70.0: BOT SPELLCASTING. WC3's computer AI already casts abilities built on stock abilities (by their base);
  // WL's Channel-based spells (AiSpellData, generated by tools/shattered-legacies/ai/gen_spells.py from the map data
  // and the reviewed spell_rules.txt) are cast here, by order:
  // - only in a BIG FIGHT (AiSpellFight: 5+ enemy and 5+ friendly worth within ~2000, heroes count 3) -- marching
  //   armies are never touched, the cast happens where the army already stopped to fight;
  // - unit target: an enemy (heroes first, then the weakest) or a hurt ally ALREADY inside cast range -- no chasing;
  // - point target: the point inside cast range that hits the most enemies (or allies) in the area, at least 3 worth
  //   (Blizzard-like); "self" = the caster's own spot;
  // - no target: enemies (or allies) in the area around the caster;
  // - always: skill learned, cooldown ready, enough mana; the caster not channelling, not on a mission, not at sea,
  //   not doing the ritual, and its current order an attack / smart / stop / hold or none (never interrupts a cast).
  // Movement side effects: a cast replaces the unit's queued chain -> marked, and AiArmy.ExitPass re-chains it once
  // idle (instead of waiting for the 20-s heartbeat); while it channels a bot spell, AiArmy.Issue leaves it alone.
  // Casters are registered by the army pass (SimpleBot.Process, cheap type lookup) and checked by one scheduler task
  // (every 0.25 s, at most EvalsPerSlot checks): heroes every 2 s, other units every 5 s. A spell whose order WC3
  // refuses RefuseLimit times in a row is off for RefuseOff seconds (pinned once). "-spells on|off|ab|ab2" (slot
  // bit 5). Deterministic: synced unit state, game time, lists in a fixed order.
  internal static class AiSpells
  {
    internal static int Mode = AiAb.OnAll;
    private const float HeroEvery = 2f;
    private const float UnitEvery = 5f;
    private const int LookPerSlot = 24;
    private const int EvalsPerSlot = 3;
    private const float MaxRange = 1200f;  // cast range cap (the Old Gods' spells reach the whole map)
    private const float Margin = 300f;     // units collected around the caster: MaxRange + Margin
    private const int Candidates = 12;     // point spells: centres tried
    private const float ChannelCap = 20f;  // longest channel protected from army orders
    private const float RechainFor = 60f;  // a cast unit is re-chained when idle within this time
    private const int RefuseLimit = 3;
    private const float RefuseOff = 120f;
    private const int OrderAttack = 851983;
    private const int OrderSmart = 851971;
    private const int OrderStop = 851972;
    private const int OrderHold = 851993;

    private static List<unit> _casters = new();
    private static HashSet<unit> _known = new();
    private static Dictionary<unit, float> _due = new();
    private static Dictionary<unit, float> _chanUntil = new();
    private static Dictionary<unit, int> _chanOrder = new();
    private static Dictionary<unit, float> _rechain = new();
    private static List<unit> _near = new();
    private static int _cursor;
    private static Dictionary<int, int> _castsOf = new();   // row -> casts
    private static Dictionary<int, int> _refusedOf = new(); // row -> refused orders
    private static Dictionary<int, int> _failRun = new();   // row -> refusals in a row
    private static Dictionary<int, float> _offUntil = new();
    private static HashSet<int> _pinned = new();
    internal static int[] CastsBy = new int[24];
    internal static int Evals;
    internal static int NotReady;
    internal static int Busy;
    internal static int Fights;
    internal static int Casts;
    internal static int Refused;
    internal static int Offs;
    internal static int Protected;
    internal static int Rechained;

    internal static void Setup()
    {
      AiScheduler.Add("spells", 1, 0, Tick, true); // every slot (0.25 s), a few casters per slot
    }

    // army pass (SimpleBot.Process): every living non-worker army unit of the bot
    internal static void Note(unit u)
    {
      if (Mode == AiAb.Off || _known.Contains(u))
      {
        return;
      }

      AiSpellBook.Ensure();
      if (!AiSpellBook.Has(GetUnitTypeId(u)))
      {
        return;
      }

      _known.Add(u);
      _casters.Add(u);
    }

    private static void Forget(unit u)
    {
      _known.Remove(u);
      _due.Remove(u);
      _chanUntil.Remove(u);
      _chanOrder.Remove(u);
      _rechain.Remove(u);
    }

    private static void Tick()
    {
      if (Mode == AiAb.Off || _casters.Count == 0)
      {
        return;
      }

      var looked = 0;
      var evals = 0;
      while (looked < LookPerSlot && evals < EvalsPerSlot && _casters.Count > 0)
      {
        looked++;
        if (_cursor >= _casters.Count)
        {
          _cursor = 0;
        }

        var u = _casters[_cursor];
        // removed from the game, or a dead non-hero (heroes are revived as the same unit)
        if (GetUnitTypeId(u) == 0 || (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f && !IsUnitType(u, UNIT_TYPE_HERO)))
        {
          Forget(u);
          _casters.RemoveAt(_cursor);
          continue;
        }

        _cursor++;
        if (_due.TryGetValue(u, out var due) && SimpleBot.Clock < due)
        {
          continue;
        }

        _due[u] = SimpleBot.Clock + (IsUnitType(u, UNIT_TYPE_HERO) ? HeroEvery : UnitEvery);
        if (Evaluate(u))
        {
          evals++;
        }
      }
    }

    // true = the expensive part ran (area check)
    private static bool Evaluate(unit u)
    {
      var p = GetOwningPlayer(u);
      var pid = GetPlayerId(p);
      if (pid < 0 || pid >= 24 || !AiAb.For(Mode, pid, AiAb.BitSpells) || !SimpleBot.IsBot(p))
      {
        return false;
      }

      if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || IsUnitHidden(u) || IsUnitPaused(u) || IsUnitLoaded(u)
          || AiState.MissionUnits.Contains(u) || AiFerries.InTransit(u) || AiFerries.IsBoat(u) || AiRitual.Owns(u)
          || u == SimpleBot._ragHero || Channelling(u))
      {
        return false;
      }

      var o = GetUnitCurrentOrder(u);
      if (o != 0 && o != OrderAttack && o != OrderSmart && o != OrderStop && o != OrderHold)
      {
        Busy++; // moving, or casting something else (the engine's own spells too) -- never interrupted
        return false;
      }

      var rows = AiSpellBook.RowsOf(GetUnitTypeId(u));
      if (rows == null)
      {
        return false;
      }

      var ready = false;
      for (var i = 0; i < rows.Count && !ready; i++)
      {
        ready = Ready(u, rows[i]);
      }

      if (!ready)
      {
        NotReady++;
        return false;
      }

      Evals++;
      if (!AiSpellFight.Big(u, p, pid))
      {
        return true;
      }

      Fights++;
      Collect(u);
      for (var i = 0; i < rows.Count; i++)
      {
        if (Ready(u, rows[i]) && TryCast(u, p, pid, rows[i]))
        {
          break;
        }
      }

      _near.Clear();
      return true;
    }

    private static bool Ready(unit u, int r)
    {
      var id = AiSpellBook.Id[r];
      var lvl = GetUnitAbilityLevel(u, id);
      if (lvl <= 0 || AiSpellBook.OrderNo[r] == 0)
      {
        return false;
      }

      if (_offUntil.TryGetValue(r, out var off) && SimpleBot.Clock < off)
      {
        return false;
      }

      return BlzGetUnitAbilityCooldownRemaining(u, id) <= 0f
             && GetUnitState(u, UNIT_STATE_MANA) >= BlzGetUnitAbilityManaCost(u, id, lvl - 1);
    }

    // units within MaxRange + Margin of the caster (from the cell's list)
    private static void Collect(unit u)
    {
      _near.Clear();
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      var list = AiSpellFight.Around(x, y);
      var reach = MaxRange + Margin;
      for (var i = 0; i < list.Count; i++)
      {
        var t = list[i];
        SimpleBot.OpUnits++;
        if (!AiSpellFight.Usable(t))
        {
          continue;
        }

        var dx = GetUnitX(t) - x;
        var dy = GetUnitY(t) - y;
        if (dx * dx + dy * dy <= reach * reach)
        {
          _near.Add(t);
        }
      }
    }

    private static bool Side(unit t, player p, int who)
    {
      return who == 2 ? AiSpellFight.Friend(t, p) : AiSpellFight.Foe(t, p);
    }

    private static bool Within(unit a, float x, float y, float r)
    {
      var dx = GetUnitX(a) - x;
      var dy = GetUnitY(a) - y;
      return dx * dx + dy * dy <= r * r;
    }

    // worth of the side's units within r of (x, y)
    private static int CountAt(player p, int who, float x, float y, float r)
    {
      var w = 0;
      for (var i = 0; i < _near.Count; i++)
      {
        var t = _near[i];
        if (Within(t, x, y, r) && Side(t, p, who))
        {
          w += AiSpellFight.Worth(t);
        }
      }

      return w;
    }

    private static bool TryCast(unit u, player p, int pid, int r)
    {
      var id = AiSpellBook.Id[r];
      var lvl = GetUnitAbilityLevel(u, id);
      var ab = BlzGetUnitAbility(u, id);
      if (ab == null)
      {
        return false;
      }

      var range = BlzGetAbilityRealLevelField(ab, ABILITY_RLF_CAST_RANGE, lvl - 1);
      if (range > MaxRange)
      {
        range = MaxRange;
      }

      if (range < 150f)
      {
        range = 150f;
      }

      var area = (float)AiSpellBook.Radius[r];
      if (area <= 0f)
      {
        area = BlzGetAbilityRealLevelField(ab, ABILITY_RLF_AREA_OF_EFFECT, lvl - 1);
      }

      if (area <= 0f)
      {
        area = 300f;
      }

      var kind = AiSpellBook.Kind[r];
      var who = AiSpellBook.Who[r];
      var min = AiSpellBook.Min[r];
      var order = AiSpellBook.OrderNo[r];
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      if (kind == 1)
      {
        var t = PickUnit(u, p, who, range, AiSpellBook.Flags[r]);
        if (t == null)
        {
          return false;
        }

        return Done(u, pid, r, IssueTargetOrderById(u, order, t), GetUnitX(t), GetUnitY(t), lvl, ab);
      }

      if (kind == 2)
      {
        var tx = x;
        var ty = y;
        if (who != 3)
        {
          var found = PickPoint(u, p, who, range, area, min > 0 ? min : 3, ref tx, ref ty); // own statement (Lua: ref)
          if (!found)
          {
            return false;
          }
        }

        return Done(u, pid, r, IssuePointOrderById(u, order, tx, ty), tx, ty, lvl, ab);
      }

      // no target: enemies (self buffs too) or allies around the caster
      var need = min > 0 ? min : (who == 3 ? 1 : 3);
      if (CountAt(p, who == 2 ? 2 : 1, x, y, area) < need)
      {
        return false;
      }

      return Done(u, pid, r, IssueImmediateOrderById(u, order), x, y, lvl, ab);
    }

    // heroes first, then the weakest; inside cast range only (no chasing)
    private static unit PickUnit(unit u, player p, int who, float range, int flags)
    {
      unit best = null;
      var bestScore = -1f;
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      for (var i = 0; i < _near.Count; i++)
      {
        var t = _near[i];
        if (!Within(t, x, y, range) || !Side(t, p, who))
        {
          continue;
        }

        if ((who != 2 && IsUnitType(t, UNIT_TYPE_MAGIC_IMMUNE))
            || ((flags & 4) != 0 && IsUnitType(t, UNIT_TYPE_FLYING))
            || ((flags & 8) != 0 && IsUnitType(t, UNIT_TYPE_MECHANICAL)))
        {
          continue;
        }

        var max = GetUnitState(t, UNIT_STATE_MAX_LIFE);
        var life = max > 1f ? GetUnitState(t, UNIT_STATE_LIFE) / max : 1f;
        if (who == 2 && (((flags & 2) != 0 && life >= 0.7f) || IsUnitType(t, UNIT_TYPE_MECHANICAL)))
        {
          continue;
        }

        var score = (IsUnitType(t, UNIT_TYPE_HERO) ? 2f : 0f) + (1f - life);
        if ((flags & 1) != 0)
        {
          if (GetUnitState(t, UNIT_STATE_MANA) <= 0f)
          {
            continue;
          }

          score += 1f;
        }

        if (score > bestScore)
        {
          best = t;
          bestScore = score;
        }
      }

      return best;
    }

    // the side's unit inside cast range whose area holds the most worth (at least need)
    private static bool PickPoint(unit u, player p, int who, float range, float area, int need, ref float tx,
      ref float ty)
    {
      var x = GetUnitX(u);
      var y = GetUnitY(u);
      var best = 0;
      var tried = 0;
      for (var i = 0; i < _near.Count && tried < Candidates; i++)
      {
        var c = _near[i];
        if (!Within(c, x, y, range) || !Side(c, p, who))
        {
          continue;
        }

        tried++;
        var cx = GetUnitX(c);
        var cy = GetUnitY(c);
        var w = CountAt(p, who, cx, cy, area);
        if (w > best)
        {
          best = w;
          tx = cx;
          ty = cy;
        }
      }

      return best >= need;
    }

    private static bool Done(unit u, int pid, int r, bool ok, float tx, float ty, int lvl, ability ab)
    {
      SimpleBot.OrdersIssued++;
      if (!ok)
      {
        Refused++;
        _refusedOf[r] = (_refusedOf.TryGetValue(r, out var nr) ? nr : 0) + 1;
        var run = (_failRun.TryGetValue(r, out var fr) ? fr : 0) + 1;
        if (run >= RefuseLimit)
        {
          run = 0;
          Offs++;
          _offUntil[r] = SimpleBot.Clock + RefuseOff;
          if (!_pinned.Contains(r))
          {
            _pinned.Add(r);
            SimpleBot.LogPin("spells: " + GetObjectName(AiSpellBook.Id[r]) + " refused " + I2S(RefuseLimit)
                             + "x in a row (P" + I2S(pid) + " " + GetUnitName(u) + ") -- off for "
                             + I2S(R2I(RefuseOff)) + " s each time");
          }
        }

        _failRun[r] = run;
        return false;
      }

      _failRun[r] = 0;
      AiTrack.Ordered(u, tx, ty, AiTrack.KCast);
      Casts++;
      CastsBy[pid] = CastsBy[pid] + 1;
      var n = (_castsOf.TryGetValue(r, out var nc) ? nc : 0) + 1;
      _castsOf[r] = n;
      if (n == 1)
      {
        SimpleBot.Log("P" + I2S(pid) + " first cast of " + GetObjectName(AiSpellBook.Id[r]) + " by "
                      + GetUnitName(u) + " @" + I2S(R2I(tx)) + "," + I2S(R2I(ty)));
      }

      var follow = BlzGetAbilityRealLevelField(ab, ABILITY_RLF_FOLLOW_THROUGH_TIME, lvl - 1);
      if (follow > ChannelCap)
      {
        follow = ChannelCap;
      }

      if (follow < 0f)
      {
        follow = 0f;
      }

      _chanUntil[u] = SimpleBot.Clock + follow + 2f; // + cast point
      _chanOrder[u] = AiSpellBook.OrderNo[r];
      _rechain[u] = SimpleBot.Clock + RechainFor;
      return true;
    }

    // AiArmy.Issue: the unit is casting / channelling a bot spell right now -- no army order
    internal static bool Channelling(unit u)
    {
      if (!_chanUntil.TryGetValue(u, out var until))
      {
        return false;
      }

      if (SimpleBot.Clock >= until || GetUnitCurrentOrder(u) != _chanOrder[u])
      {
        _chanUntil.Remove(u);
        _chanOrder.Remove(u);
        return false;
      }

      return true;
    }

    // AiArmy.ExitPass, for an IDLE unit: did it cast recently (its chain was replaced)? Consumes the mark.
    internal static bool Rechain(unit u)
    {
      if (!_rechain.TryGetValue(u, out var until))
      {
        return false;
      }

      _rechain.Remove(u);
      return SimpleBot.Clock < until;
    }

    // "-spells on|off|ab|ab2"
    internal static void OnCommand()
    {
      var m = AiAb.Parse(GetEventPlayerChatString(), "-spells");
      if (m >= 0)
      {
        Mode = m;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8,
        "bot spells (Channel spells, big fights only: " + I2S(AiSpellFight.NeedEnemy) + "+ enemy and "
        + I2S(AiSpellFight.NeedFriend) + "+ friendly within " + I2S(R2I(AiSpellFight.Reach)) + ", heroes count 3): "
        + AiAb.Name(Mode) + " [slots" + AiAb.Slots(Mode, AiAb.BitSpells) + "]  -- -spells on|off|ab|ab2");
    }

    internal static void Lines(List<string> o)
    {
      AiSpellBook.Ensure();
      o.Add("## SPELLS (0.70: bots cast Channel spells in big fights) -spells " + AiAb.Name(Mode) + " [slots"
            + AiAb.Slots(Mode, AiAb.BitSpells) + "]");
      o.Add("table " + I2S(AiSpellBook.Count) + " spells; casters known " + I2S(_casters.Count) + "; checks "
            + I2S(Evals) + " (+ not ready " + I2S(NotReady) + ", busy order " + I2S(Busy) + "); big fight yes/no "
            + I2S(AiSpellFight.GateYes) + "/" + I2S(AiSpellFight.GateNo) + "; area scans " + I2S(AiSpellFight.Scans)
            + " reused " + I2S(AiSpellFight.Reused));
      o.Add("casts " + I2S(Casts) + " in " + I2S(Fights) + " fight checks; refused " + I2S(Refused) + ", switched off "
            + I2S(Offs) + "x; army orders skipped while channelling " + I2S(Protected) + "; re-chained after a cast "
            + I2S(Rechained));
      var s = "casts per bot:";
      for (var pid = 0; pid < 24; pid++)
      {
        if (CastsBy[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(CastsBy[pid]) + ";";
        }
      }

      o.Add(s);
      if (Mode == AiAb.Ab || Mode == AiAb.Ab2)
      {
        o.Add("A/B casts: " + AiAb.Compare(Mode, AiAb.BitSpells, CastsBy));
      }

      for (var r = 0; r < AiSpellBook.Count; r++)
      {
        var c = _castsOf.TryGetValue(r, out var nc) ? nc : 0;
        var f = _refusedOf.TryGetValue(r, out var nf) ? nf : 0;
        if (c > 0 || f > 0)
        {
          o.Add("spell " + GetObjectName(AiSpellBook.Id[r]) + ": casts " + I2S(c) + " refused " + I2S(f));
        }
      }
    }
  }
}
