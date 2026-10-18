using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using MacroTools.Shared;
using MacroTools.ControlPoints;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Factions.Legion.Quests;

namespace WarcraftLegacies.Source.Ai
{
  // 0.36: stranded heroes. In an 85-min test Illidan's heroes stood in the sea in the middle of the map for a long
  // time (probably moved there by a quest event); army orders reached them but they could not move. Runs on the
  // wave tick (15 s) over the registered legends owned by bots:
  //   1. a hero more than FarDist from its army's anchor (SimpleBot's biggest same-landmass group) that has not
  //      moved for StillSec gets an explicit move order to the anchor;
  //   2. if it still has not moved StillSec after that order, it is put next to its nearest living altar
  //      (bots with perks only -- a fair autopilot keeps its heroes where they are, like a human would).
  // Heroes on a mission (hero visits, Ragnaros) and heroes that can't walk are never touched.
  // Own class because of the CSharp.lua limits (see AI_AGENTS_CONTEXT.md).
  internal static class AiHeroCare
  {
    private const float FarDist = 4000f;
    private const float StillSec = 120f;
    private const float MoveEps = 150f;
    private const int MinAnchorUnits = 3; // a smaller "army" is no reference point (could be the hero himself)

    private static readonly Dictionary<unit, float> _lastX = new();
    private static readonly Dictionary<unit, float> _lastY = new();
    private static readonly Dictionary<unit, float> _stillSince = new();
    private static readonly Dictionary<unit, float> _walkSince = new(); // only present after the move order

    public static void Run()
    {
      var all = LegendaryHeroManager.GetAll();
      for (var i = 0; i < all.Count; i++)
      {
        var u = all[i].Unit;
        if (u == null || GetUnitTypeId(u) == 0)
        {
          continue;
        }

        var owner = GetOwningPlayer(u);
        if (!SimpleBot.IsBot(owner) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || GetUnitMoveSpeed(u) <= 1f
            || AiState.MissionUnits.Contains(u) || u == SimpleBot._ragHero)
        {
          Forget(u);
          continue;
        }

        var pid = GetPlayerId(owner);
        var d = SimpleBot.GetDiag(pid);
        if (d.AnchorIsBase || d.AnchorUnits < MinAnchorUnits)
        {
          Forget(u);
          continue;
        }

        SimpleBot.WaveOps++;
        var x = GetUnitX(u);
        var y = GetUnitY(u);
        var ax = d.AnchorX - x;
        var ay = d.AnchorY - y;
        if (ax * ax + ay * ay <= FarDist * FarDist)
        {
          Forget(u); // with the army (again)
          continue;
        }

        if (!_lastX.ContainsKey(u))
        {
          _lastX[u] = x;
          _lastY[u] = y;
          _stillSince[u] = SimpleBot.Clock;
          continue;
        }

        var mx = x - _lastX[u];
        var my = y - _lastY[u];
        _lastX[u] = x;
        _lastY[u] = y;
        if (mx * mx + my * my > MoveEps * MoveEps)
        {
          _stillSince[u] = SimpleBot.Clock; // moving -- fine
          continue;
        }

        if (SimpleBot.Clock - _stillSince[u] < StillSec)
        {
          continue;
        }

        if (!_walkSince.ContainsKey(u))
        {
          _walkSince[u] = SimpleBot.Clock;
          _stillSince[u] = SimpleBot.Clock;
          IssuePointOrder(u, "move", d.AnchorX, d.AnchorY);
          SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(owner) + ": hero " + GetUnitName(u) + " stuck at "
                        + I2S(R2I(x)) + "," + I2S(R2I(y)) + " far from the army -> walks back");
          continue;
        }

        // the move order did not help either
        if (SimpleBot.HasPerks(owner))
        {
          var loc = GetPlayerStartLocation(owner);
          var altar = SimpleBot.FindAltar(owner, GetStartLocationX(loc), GetStartLocationY(loc));
          if (altar != null)
          {
            SetUnitPosition(u, GetUnitX(altar), GetUnitY(altar) - 200f);
            SimpleBot.LogPin("P" + I2S(pid) + " " + SimpleBot.FacName(owner) + ": stranded hero " + GetUnitName(u)
                             + " (" + I2S(R2I(x)) + "," + I2S(R2I(y)) + ") moved to " + GetUnitName(altar));
          }
        }

        Forget(u);
      }
    }

    // for -heroes: "" (fine / not watched), "still Ns" or "walking back"
    internal static string Status(unit u)
    {
      if (u == null || !_stillSince.ContainsKey(u))
      {
        return "";
      }

      if (_walkSince.ContainsKey(u))
      {
        return "walking back";
      }

      return "still " + I2S(R2I(SimpleBot.Clock - _stillSince[u])) + "s";
    }

    private static void Forget(unit u)
    {
      if (!_lastX.ContainsKey(u))
      {
        return;
      }

      _lastX.Remove(u);
      _lastY.Remove(u);
      _stillSince.Remove(u);
      _walkSince.Remove(u);
    }
  }

  // 0.36: team standings -- control points per team (WL: 90 wins, warning at 80). Pinned once per team when it first
  // reaches 80 and 90 (the closest the dump gets to a "winner record"), listed in the dump's TEAMS section.
  internal static class AiStandings
  {
    private static readonly HashSet<string> _pinned = new();

    internal static void Check()
    {
      var teams = FactionManager.GetAllTeams();
      for (var i = 0; i < teams.Count; i++)
      {
        var n = TeamCps(teams[i]);
        PinOnce(teams[i], n, 80, "warning threshold");
        PinOnce(teams[i], n, 90, "victory threshold");
      }
    }

    private static void PinOnce(Team t, int cps, int at, string what)
    {
      var key = t.Name + "|" + I2S(at);
      if (cps < at || _pinned.Contains(key))
      {
        return;
      }

      _pinned.Add(key);
      SimpleBot.LogPin("STANDINGS: team " + t.Name + " reached " + I2S(cps) + " CPs (" + what + " " + I2S(at) + ") -- "
                       + Summary());
    }

    internal static int TeamCps(Team t)
    {
      var n = 0;
      for (var i = 0; i < 24; i++)
      {
        var pd = Player(i).GetPlayerData();
        if (pd.Team == t)
        {
          n += pd.ControlPoints.Count;
        }
      }

      return n;
    }

    // one line: "Team A 45, Team B 30, ..."
    internal static string Summary()
    {
      var s = "";
      var teams = FactionManager.GetAllTeams();
      for (var i = 0; i < teams.Count; i++)
      {
        var n = TeamCps(teams[i]);
        if (n > 0 || teams[i].Size > 0)
        {
          s = s + (s == "" ? "" : ", ") + teams[i].Name + " " + I2S(n);
        }
      }

      return s;
    }

    // dump section: one line per team with its members
    internal static void Lines(List<string> o)
    {
      o.Add("## TEAMS (control points; 90 wins)  " + Summary());
      var teams = FactionManager.GetAllTeams();
      for (var i = 0; i < teams.Count; i++)
      {
        var t = teams[i];
        var members = "";
        for (var k = 0; k < 24; k++)
        {
          var p = Player(k);
          var pd = p.GetPlayerData();
          if (pd.Team != t)
          {
            continue;
          }

          members = members + " P" + I2S(k) + " " + SimpleBot.FacName(p) + "=" + I2S(pd.ControlPoints.Count)
                    + (SimpleBot.IsBot(p) ? "(bot)" : "");
        }

        if (members != "")
        {
          o.Add(t.Name + ": " + I2S(TeamCps(t)) + " |" + members);
        }
      }
    }
  }
}
