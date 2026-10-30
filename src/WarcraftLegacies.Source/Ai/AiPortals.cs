using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.46: PSEUDO-PORTALS (design: CONCEPTS.md 7). Bots no longer click waygates. A chain that crosses a portal edge
  // ends in front of the waygate and the unit gets a TICKET (waygate, exit, final destination). One enter-region
  // trigger per waygate area fires when a unit actually walks in -- so a unit is never teleported from somewhere else.
  // It teleports only a living unit whose ticket names THIS waygate (passers-by, human units, stale tickets: ignored),
  // then gives it the rest of the way from the exit at once. A unit already inside the area when it gets the ticket
  // gets no enter event: it is teleported on issue (AiRoutes.Chain). Real waygates stay for human players.
  // Why: the real teleport dropped the queued chain (1089 units idle at exits, 0.45.0), and no bot crossed the Old Gods
  // rift in 510 min (0.45.0) -- every WL / AI waygate went through units clicking it.
  // "-pportal off" = 0.45 (chains click the waygate).
  internal static class AiPortals
  {
    internal static bool On = true;
    private const float Area = 450f; // half side of the square entry area around a waygate (approach point: 400)
    private const int OrderAttack = 851983;
    private const string FxIn = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportCaster.mdl";
    private const string FxOut = "Abilities\\Spells\\Human\\MassTeleport\\MassTeleportTarget.mdl";

    private static trigger _enter;
    private static Dictionary<region, unit> _gateOf = new();
    private static HashSet<unit> _watched = new();
    private static Dictionary<unit, unit> _ticket = new();     // unit -> waygate it should take
    private static Dictionary<unit, float> _exitX = new();
    private static Dictionary<unit, float> _exitY = new();
    private static Dictionary<unit, float> _destX = new();
    private static Dictionary<unit, float> _destY = new();
    private static Dictionary<unit, float> _fxAt = new();      // waygate -> last effect time (one per second)
    private static int _depth;
    internal static int Teleports;
    internal static int OnIssue;
    internal static int Ignored;
    internal static int Cleaned;

    // ---- entry areas -------------------------------------------------------------------------------------------

    // called for the waygate of every portal edge (AiRoutes.AddPortalEdge); once per waygate
    internal static void Watch(unit gate)
    {
      if (gate == null || _watched.Contains(gate))
      {
        return;
      }

      if (_enter == null)
      {
        _enter = CreateTrigger();
        TriggerAddAction(_enter, OnEnter);
      }

      _watched.Add(gate);
      float gx = GetUnitX(gate), gy = GetUnitY(gate);
      var box = Rect(gx - Area, gy - Area, gx + Area, gy + Area);
      var reg = CreateRegion();
      RegionAddRect(reg, box);
      RemoveRect(box);
      _gateOf[reg] = gate;
      TriggerRegisterEnterRegion(_enter, reg, null);
    }

    internal static bool Inside(unit u, unit gate)
    {
      var dx = GetUnitX(u) - GetUnitX(gate);
      var dy = GetUnitY(u) - GetUnitY(gate);
      return dx <= Area && dx >= -Area && dy <= Area && dy >= -Area;
    }

    // ---- tickets -----------------------------------------------------------------------------------------------

    internal static void SetTicket(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      _ticket[u] = gate;
      _exitX[u] = ex;
      _exitY[u] = ey;
      _destX[u] = dx;
      _destY[u] = dy;
    }

    private static void Drop(unit u)
    {
      _ticket.Remove(u);
      _exitX.Remove(u);
      _exitY.Remove(u);
      _destX.Remove(u);
      _destY.Remove(u);
    }

    // every 30 s (AiRoutes.CheckPortals): forget tickets of dead / removed units. Collect first, then remove -- the
    // result does not depend on the dictionary's iteration order (desync-safe).
    internal static void Clean()
    {
      var dead = new List<unit>();
      foreach (var kv in _ticket)
      {
        if (GetUnitTypeId(kv.Key) == 0 || GetUnitState(kv.Key, UNIT_STATE_LIFE) <= 0.405f)
        {
          dead.Add(kv.Key);
        }
      }

      for (var i = 0; i < dead.Count; i++)
      {
        Drop(dead[i]);
        Cleaned++;
      }
    }

    // ---- teleport ----------------------------------------------------------------------------------------------

    private static void OnEnter()
    {
      try
      {
        var u = GetTriggerUnit();
        if (!_ticket.ContainsKey(u))
        {
          return; // not a bot unit on its way through a portal (the common case)
        }

        var reg = GetTriggeringRegion();
        if (!_gateOf.ContainsKey(reg) || _gateOf[reg] != _ticket[u])
        {
          Ignored++; // walked into ANOTHER waygate's area: not its portal
          return;
        }

        Teleport(u, _ticket[u], _exitX[u], _exitY[u], _destX[u], _destY[u]);
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("TICK ERROR in pseudo-portal: " + e.Message);
      }
    }

    // from AiRoutes.Chain: the unit stands in the area already. False = not now (nested teleports): walk in instead.
    internal static bool TeleportNow(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      if (_depth > 1)
      {
        return false;
      }

      OnIssue++;
      Teleport(u, gate, ex, ey, dx, dy);
      return true;
    }

    private static void Teleport(unit u, unit gate, float ex, float ey, float dx, float dy)
    {
      Drop(u);
      if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || gate == null
          || GetUnitState(gate, UNIT_STATE_LIFE) <= 0.405f)
      {
        Ignored++;
        return;
      }

      var now = SimpleBot.Clock;
      var last = _fxAt.ContainsKey(gate) ? _fxAt[gate] : -10f;
      if (now - last >= 1f)
      {
        _fxAt[gate] = now;
        DestroyEffect(AddSpecialEffect(FxIn, GetUnitX(u), GetUnitY(u)));
        DestroyEffect(AddSpecialEffect(FxOut, ex, ey));
      }

      SetUnitPosition(u, ex, ey); // the engine picks the nearest free spot
      Teleports++;
      _depth++;
      if (!AiRoutes.Chain(GetOwningPlayer(u), u, dx, dy))
      {
        AiScheduler.CountOrder(u, dx, dy);
        IssuePointOrderById(u, OrderAttack, dx, dy);
      }

      _depth--;
    }

    // ---- the Dark Portal ---------------------------------------------------------------------------------------
    // WL's Fel Horde quest "The Dark Portal" (turn 10, or research R02C at Hellfire Citadel, never before turn 8) shows
    // and activates three preplaced waygate pairs (n036, Neutral Hostile). Active neutral waygates are portal edges
    // anyway (AiRoutes.CheckPortals) -- but the 0.45.0 dump at 540 min had none. If WL made them VISIBLE (= opened)
    // and still no live edge exists, one pair is added here, in both directions. Positions: WL's preplaced units
    // (Blasted Lands side / Outland side) and their destinations (WL's Regions.Dark_Portal_Exit_2 / _Entrance_2).
    private const float DpEkX = 16736f;
    private const float DpEkY = -19680f;
    private const float DpOutX = 4704f;
    private const float DpOutY = -25376f;
    private const float DpToOutX = 4352f;
    private const float DpToOutY = -25504f;
    private const float DpToEkX = 16752f;
    private const float DpToEkY = -19680f;
    private static unit _dpEk;
    private static unit _dpOut;
    private static bool _dpSearched;
    internal static string DarkPortalState = "not checked";

    internal static void AddStatic()
    {
      if (!_dpSearched)
      {
        _dpSearched = true;
        _dpEk = FindWaygate(DpEkX, DpEkY);
        _dpOut = FindWaygate(DpOutX, DpOutY);
      }

      if (_dpEk == null || _dpOut == null)
      {
        DarkPortalState = "waygates not found";
        return;
      }

      if (IsUnitHidden(_dpEk) && IsUnitHidden(_dpOut))
      {
        DarkPortalState = "closed (WL waygates hidden)";
        return;
      }

      if (AiRoutes.IsPortalEdgeGate(_dpEk) || AiRoutes.IsPortalEdgeGate(_dpOut))
      {
        DarkPortalState = "open (live waygate edges)";
        return;
      }

      var a = AiRoutes.AddPortalEdge(_dpEk, DpToOutX, DpToOutY);
      var b = AiRoutes.AddPortalEdge(_dpOut, DpToEkX, DpToEkY);
      DarkPortalState = "open (added by the bot: " + (a ? "EK->Outland " : "") + (b ? "Outland->EK" : "") + ")";
    }

    private static unit FindWaygate(float x, float y)
    {
      unit found = null;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, 300f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetUnitTypeId(u) == AiWorld.PortalUnitId)
        {
          found = u;
        }
      }

      DestroyGroup(g);
      return found;
    }

    // ---- commands and dump -------------------------------------------------------------------------------------

    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg == "-pportal on")
      {
        On = true;
      }
      else if (msg == "-pportal off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "portals: "
        + (On ? "PSEUDO-PORTALS (bots teleport by trigger, 0.46)" : "units click the waygates (0.45)")
        + "  -- -pportal on|off");
    }

    internal static void Lines(List<string> o)
    {
      o.Add("pseudo-portals " + (On ? "on" : "off") + ": teleports " + I2S(Teleports) + " (on issue " + I2S(OnIssue)
            + "), ignored " + I2S(Ignored) + ", open tickets " + I2S(_ticket.Count) + ", cleaned " + I2S(Cleaned)
            + ", watched waygates " + I2S(_watched.Count) + "; Dark Portal: " + DarkPortalState);

      // WL's own waygates (not the AI pairs of Neutral Extra): where, owner, active, hidden, destination
      var g = CreateGroup();
      for (var pl = 24; pl < 28; pl++)
      {
        if (pl == 26)
        {
          continue;
        }

        GroupEnumUnitsOfPlayer(g, Player(pl), null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (GetUnitTypeId(u) != AiWorld.PortalUnitId && !WaygateIsActive(u))
          {
            continue;
          }

          o.Add("  WL waygate " + GetUnitName(u) + " @" + I2S(R2I(GetUnitX(u))) + "," + I2S(R2I(GetUnitY(u)))
                + " P" + I2S(pl) + " active=" + (WaygateIsActive(u) ? "1" : "0") + " hidden="
                + (IsUnitHidden(u) ? "1" : "0") + " -> " + I2S(R2I(WaygateGetDestinationX(u))) + ","
                + I2S(R2I(WaygateGetDestinationY(u))) + (AiRoutes.IsPortalEdgeGate(u) ? " edge" : " no-edge"));
        }
      }

      DestroyGroup(g);
    }
  }
}
