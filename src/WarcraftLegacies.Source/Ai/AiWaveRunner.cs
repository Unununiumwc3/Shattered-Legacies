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
  // 0.35.0: split out of SimpleBot (AiSystem.cs) -- runs the scripted invasion waves (gather, teleport, abort,
  // force). The wave DATA is the class AiWaves in AiConfig.cs (hence the different name).
  // Its own class because CSharp.lua turns every static member of a class into a Lua local of one function
  // (limit 200); SimpleBot had outgrown that. Shared state lives in SimpleBot / AiState as internal (not readonly).
  internal static class AiWaveRunner
  { // v15: node events fired once
    private static readonly Dictionary<int, int> _wavePhase = new();      // v23: 0 idle, 1 gather, 2 channel
    private static readonly Dictionary<int, float> _wavePhaseAt = new();
    private static readonly Dictionary<int, float> _waveLastDone = new();
    private static readonly Dictionary<int, float> _waveDestX = new();
    private static readonly Dictionary<int, float> _waveDestY = new();
    private static readonly Dictionary<int, effect> _waveSrcFx = new();
    private static readonly Dictionary<int, effect> _waveDstFx = new();
    private static readonly Dictionary<int, bool> _waveWasActive = new();

    // v27: true while an invasion wave has this faction in its gather (1) or channel (2) phase -- the window
    // during which the wave is actively commanding/teleporting the army, so combat must stand down.
    internal static bool WaveOwnsFaction(string facName)
    {
      var waves = AiWaves.All;
      for (var i = 0; i < waves.Count; i++)
      {
        var phase = _wavePhase.ContainsKey(i) ? _wavePhase[i] : 0;
        if ((phase == 1 || phase == 2) && InGroup(waves[i], facName))
        {
          return true;
        }
      }

      return false;
    }

    public static bool GroupHasBot(WaveConfig cfg)
    {
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var f = SimpleBot.Bots[b].GetPlayerData().Faction;
        if (f != null && InGroup(cfg, f.Name) && SimpleBot.HasPerks(SimpleBot.Bots[b]))
        {
          return true;
        }
      }

      return false;
    }

    internal static bool InGroup(WaveConfig cfg, string facName)
    {
      for (var i = 0; i < cfg.Factions.Count; i++)
      {
        if (cfg.Factions[i] == facName)
        {
          return true;
        }
      }

      return false;
    }

    // v27.1: a position is "home" if it's on the home landmass -- or, when the home landmass is -1 (islands the
    // downsample can't resolve, e.g. Kul Tiras), within HomeWaveRadius of the home CP. Without this, island
    // waves count the whole map's -1 coastline and their %/enemy gate can never pass -> they never teleport.
    private static bool IsHomeArea(int homeLm, float hx, float hy, float x, float y)
    {
      if (homeLm != -1)
      {
        return Geography.Landmass(x, y) == homeLm;
      }

      var dx = x - hx;
      var dy = y - hy;
      return dx * dx + dy * dy <= AiWorld.HomeWaveRadius * AiWorld.HomeWaveRadius;
    }

    private static bool WaveActive(WaveConfig cfg, int homeLm, float hx, float hy)
    {
      // v30: cheapest test first -- an unowned trigger CP rules the wave out before any unit enumeration.
      if (cfg.TriggerCpId != 0 && !SimpleBot.OwnsCp(cfg, cfg.TriggerCpId))
      {
        return false;
      }

      HomeCpCounts(cfg, homeLm, hx, hy, out var owned, out var total);
      var thresholdMet = total > 0 && owned >= R2I(total * cfg.Threshold);

      // v24: launch when the group either holds the %-gate OR owns every monument on its home area,
      // AND has cleared home down to a handful of enemies. Optional TriggerCpId is still an extra AND.
      return (thresholdMet || AllMonumentsOwned(cfg, homeLm, hx, hy))
             && EnemiesOnLandmass(cfg, homeLm, hx, hy, AiWorld.EnemyLeaveThreshold) < AiWorld.EnemyLeaveThreshold;
    }

    // v28: home-area CP counts for a wave group (shared by WaveActive and the diagnostics snapshot).
    private static void HomeCpCounts(WaveConfig cfg, int homeLm, float hx, float hy, out int owned, out int total)
    {
      total = 0;
      owned = 0;
      var cps = ControlPointManager.Instance.GetAllControlPoints();
      for (var i = 0; i < cps.Count; i++)
      {
        var cu = cps[i].Unit;
        if (!IsHomeArea(homeLm, hx, hy, GetUnitX(cu), GetUnitY(cu)))
        {
          continue;
        }

        total++;
        var owner = GetOwningPlayer(cu);
        if (GetPlayerId(owner) < 24)
        {
          var of = owner.GetPlayerData().Faction;
          if (of != null && InGroup(cfg, of.Name))
          {
            owned++;
          }
        }
      }
    }

    // v28: diagnostics snapshot of one wave (on demand from AiDebug; never called by logic).
    public static WaveDiag GetWaveDiag(int idx)
    {
      var w = new WaveDiag();
      if (idx < 0 || idx >= AiWaves.All.Count)
      {
        return w;
      }

      var cfg = AiWaves.All[idx];
      w.HasBot = GroupHasBot(cfg);
      w.Phase = _wavePhase.ContainsKey(idx) ? _wavePhase[idx] : 0;
      w.PhaseAge = _wavePhaseAt.ContainsKey(idx) ? SimpleBot._clock - _wavePhaseAt[idx] : 0f;
      var lastDone = _waveLastDone.ContainsKey(idx) ? _waveLastDone[idx] : -99999f;
      w.CooldownLeft = cfg.IntervalSec - (SimpleBot._clock - lastDone);
      if (w.CooldownLeft < 0f)
      {
        w.CooldownLeft = 0f;
      }

      var homeCp = SimpleBot.SafeCp(cfg.HomeCpId);
      if (homeCp == null)
      {
        return w;
      }

      w.HomeOk = true;
      var hx = GetUnitX(homeCp.Unit);
      var hy = GetUnitY(homeCp.Unit);
      w.HomeLm = Geography.Landmass(hx, hy);
      HomeCpCounts(cfg, w.HomeLm, hx, hy, out var owned, out var total);
      w.Owned = owned;
      w.Total = total;
      w.Need = R2I(total * cfg.Threshold);
      w.Monuments = AllMonumentsOwned(cfg, w.HomeLm, hx, hy);
      w.Enemies = EnemiesOnLandmass(cfg, w.HomeLm, hx, hy, 500);
      w.TriggerOk = cfg.TriggerCpId == 0 || SimpleBot.OwnsCp(cfg, cfg.TriggerCpId);
      w.Active = WaveActive(cfg, w.HomeLm, hx, hy);
      return w;
    }

    // All monuments in the group's home area must be owned by the group. Monuments elsewhere are ignored,
    // so the same two lists serve every wave regardless of which theatre it launches from.
    private static bool AllMonumentsOwned(WaveConfig cfg, int homeLm, float hx, float hy)
    {
      var any = false;
      if (!MonumentsHeld(cfg, homeLm, hx, hy, AiWorld.CombinedMonuments, ref any))
      {
        return false;
      }

      if (!MonumentsHeld(cfg, homeLm, hx, hy, AiWorld.KalimdorMonuments, ref any))
      {
        return false;
      }

      return any; // true only if at least one monument was actually in this home area and all such were held
    }

    private static bool MonumentsHeld(WaveConfig cfg, int homeLm, float hx, float hy, List<int> monuments, ref bool any)
    {
      for (var i = 0; i < monuments.Count; i++)
      {
        var cp = SimpleBot.SafeCp(monuments[i]);
        if (cp == null || !IsHomeArea(homeLm, hx, hy, GetUnitX(cp.Unit), GetUnitY(cp.Unit)))
        {
          continue;
        }

        any = true;
        var owner = cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
        var of = owner != null && GetPlayerId(owner) < 24 ? owner.GetPlayerData().Faction : null;
        if (of == null || !InGroup(cfg, of.Name))
        {
          return false;
        }
      }

      return true;
    }

    // Count enemies (non-group, living, non-structure) on the home landmass, stopping at cap. Runs on the
    // 15s wave tick for a few active waves only -> cheap. "Cleared home enough to leave" gate.
    private static int EnemiesOnLandmass(WaveConfig cfg, int homeLm, float hx, float hy, int cap)
    {
      var count = 0;
      for (var i = 0; i < 24; i++)
      {
        var p = Player(i);
        var f = p.GetPlayerData().Faction;
        if (f != null && InGroup(cfg, f.Name))
        {
          continue; // own group isn't "enemy"
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
          {
            continue;
          }

          if (IsHomeArea(homeLm, hx, hy, GetUnitX(u), GetUnitY(u)))
          {
            count++;
          }
        }

        DestroyGroup(g);
        if (count >= cap)
        {
          return count;
        }
      }

      return count;
    }

    // idle -> gather (units walk to staging) -> channel (portals open) -> jump to ONE random destination.
    internal static void RunWave(int idx, WaveConfig cfg)
    {
      var homeCp = SimpleBot.SafeCp(cfg.HomeCpId);
      if (homeCp == null)
      {
        return;
      }

      // v30: waves are a BOT mechanic. With no bot in the group (e.g. Kul Tiras played by a human) the wave
      // never evaluates -- no fleet/portal messages, and no whole-map enemy count every 15 s.
      if (!GroupHasBot(cfg))
      {
        if (_wavePhase.ContainsKey(idx) && _wavePhase[idx] != 0)
        {
          SimpleBot.Log("wave " + I2S(idx) + ": no bot in group -> aborted");
          AbortWave(idx);
        }

        _waveWasActive[idx] = false;
        return;
      }

      var homeLm = Geography.Landmass(GetUnitX(homeCp.Unit), GetUnitY(homeCp.Unit));
      var active = WaveActive(cfg, homeLm, GetUnitX(homeCp.Unit), GetUnitY(homeCp.Unit));
      var was = _waveWasActive.ContainsKey(idx) && _waveWasActive[idx];
      if (!active && was)
      {
        SimpleBot.Announce(cfg.StopMsg);
        SimpleBot.Log("wave " + I2S(idx) + ": gate lost -> stopped");
        AbortWave(idx);
      }

      if (active && !was)
      {
        SimpleBot.Log("wave " + I2S(idx) + ": gate now OPEN (active)");
      }

      _waveWasActive[idx] = active;

      var gx = cfg.GatherX == 0f && cfg.GatherY == 0f ? GetUnitX(homeCp.Unit) : cfg.GatherX;
      var gy = cfg.GatherX == 0f && cfg.GatherY == 0f ? GetUnitY(homeCp.Unit) : cfg.GatherY;
      var phase = _wavePhase.ContainsKey(idx) ? _wavePhase[idx] : 0;

      if (phase == 0)
      {
        if (!active)
        {
          return;
        }

        var lastDone = _waveLastDone.ContainsKey(idx) ? _waveLastDone[idx] : -99999f;
        if (SimpleBot._clock - lastDone < cfg.IntervalSec || cfg.Dests.Count == 0)
        {
          return;
        }

        var d = cfg.Dests[GetRandomInt(0, cfg.Dests.Count - 1)];
        var dx = d.X;
        var dy = d.Y;
        if (d.CpId != 0)
        {
          var dc = SimpleBot.SafeCp(d.CpId);
          if (dc == null)
          {
            return;
          }

          dx = GetUnitX(dc.Unit);
          dy = GetUnitY(dc.Unit);
        }

        if (dx == 0f && dy == 0f)
        {
          return; // destination not set yet
        }

        _waveDestX[idx] = dx;
        _waveDestY[idx] = dy;
        SimpleBot.Announce(cfg.PrepMsg);
        SimpleBot.Log("wave " + I2S(idx) + ": gather -> dest " + I2S(R2I(dx)) + "," + I2S(R2I(dy)));
        SimpleBot.PingAt(gx, gy);
        _wavePhase[idx] = 1;
        _wavePhaseAt[idx] = SimpleBot._clock;
        return;
      }

      if (!active)
      {
        AbortWave(idx);
        return;
      }

      if (phase == 1)
      {
        GatherToPoint(cfg, homeLm, gx, gy);
        if (SimpleBot._clock - _wavePhaseAt[idx] >= cfg.GatherSec)
        {
          _waveSrcFx[idx] = AddSpecialEffect(cfg.PortalModel, gx, gy);
          _waveDstFx[idx] = AddSpecialEffect(cfg.PortalModel, _waveDestX[idx], _waveDestY[idx]);
          SimpleBot.PingAt(gx, gy);
          SimpleBot.Log("wave " + I2S(idx) + ": channel");
          _wavePhase[idx] = 2;
          _wavePhaseAt[idx] = SimpleBot._clock;
        }

        return;
      }

      if (SimpleBot._clock - _wavePhaseAt[idx] >= cfg.ChannelSec)
      {
        var moved = TeleportGathered(cfg, homeLm, gx, gy, _waveDestX[idx], _waveDestY[idx]);
        SimpleBot.Log("wave " + I2S(idx) + ": JUMP, " + I2S(moved) + " units teleported");
        SimpleBot.Announce(cfg.ArriveMsg);
        SimpleBot.PingAt(_waveDestX[idx], _waveDestY[idx]);
        AbortWave(idx);
        _waveLastDone[idx] = SimpleBot._clock;
      }
    }

    private static void GatherToPoint(WaveConfig cfg, int homeLm, float gx, float gy)
    {
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || !InGroup(cfg, fac.Name) || !SimpleBot.HasPerks(p))
        {
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || IsUnitType(u, UNIT_TYPE_PEON))
          {
            continue;
          }

          var ux = GetUnitX(u);
          var uy = GetUnitY(u);
          // In the home area AND not already sitting at a landing zone. The second test stops an away-team from
          // being dragged back home when home + landing share an ambiguous landmass id (islands).
          if (IsHomeArea(homeLm, gx, gy, ux, uy) && !NearAnyDest(cfg, ux, uy, 6000f) && u != SimpleBot._ragHero)
          {
            IssuePointOrder(u, "attack", gx, gy);
          }
        }

        DestroyGroup(g);
      }
    }

    private static int TeleportGathered(WaveConfig cfg, int homeLm, float gx, float gy, float dx, float dy)
    {
      var moved = 0;
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || !InGroup(cfg, fac.Name) || !SimpleBot.HasPerks(p))
        {
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          SimpleBot.WaveOps++;
          var ux = GetUnitX(u);
          var uy = GetUnitY(u);
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE) || !IsHomeArea(homeLm, gx, gy, ux, uy)
              || NearAnyDest(cfg, ux, uy, 6000f) || u == SimpleBot._ragHero)
          {
            continue; // structures, off-continent units, units at a landing, the Ragnaros hero: left alone
          }

          // v24: teleport ALL home-landmass units (gather is flavor now) -> nobody is stranded by a radius.
          SetUnitX(u, dx + GetRandomReal(-512f, 512f));
          SetUnitY(u, dy + GetRandomReal(-512f, 512f));
          moved++;
        }

        DestroyGroup(g);

        // v24: the arriving group adopts the destination continent's node system (no fallback to non-node).
        if (cfg.DestEdgeSet != "")
        {
          SimpleBot._factionEdgeSet[fac.Name] = cfg.DestEdgeSet;
          SimpleBot.Log(fac.Name + ": edge set -> " + cfg.DestEdgeSet + " (wave arrival)");
        }
      }

      return moved;
    }

    // True if (x,y) is within r of any of the wave's landing zones. Used to exclude an already-departed
    // away-team from the gather/teleport passes so it isn't pulled home again (islands share landmass ids).
    private static bool NearAnyDest(WaveConfig cfg, float x, float y, float r)
    {
      var r2 = r * r;
      for (var i = 0; i < cfg.Dests.Count; i++)
      {
        var d = cfg.Dests[i];
        var dx = d.X;
        var dy = d.Y;
        if (d.CpId != 0)
        {
          var dc = SimpleBot.SafeCp(d.CpId);
          if (dc == null)
          {
            continue;
          }

          dx = GetUnitX(dc.Unit);
          dy = GetUnitY(dc.Unit);
        }

        if (dx == 0f && dy == 0f)
        {
          continue;
        }

        var ex = x - dx;
        var ey = y - dy;
        if (ex * ex + ey * ey <= r2)
        {
          return true;
        }
      }

      return false;
    }

    private static void AbortWave(int idx)
    {
      if (_waveSrcFx.ContainsKey(idx) && _waveSrcFx[idx] != null)
      {
        DestroyEffect(_waveSrcFx[idx]);
        _waveSrcFx[idx] = null;
      }

      if (_waveDstFx.ContainsKey(idx) && _waveDstFx[idx] != null)
      {
        DestroyEffect(_waveDstFx[idx]);
        _waveDstFx[idx] = null;
      }

      _wavePhase[idx] = 0;
    }

    // Debug: instant jump of the group's home-landmass units to a random destination (skips prep/channel).
    public static void ForceWave(int idx)
    {
      if (idx < 0 || idx >= AiWaves.All.Count)
      {
        return;
      }

      var cfg = AiWaves.All[idx];
      if (cfg.Dests.Count == 0)
      {
        return;
      }

      var homeCp = SimpleBot.SafeCp(cfg.HomeCpId);
      if (homeCp == null)
      {
        return;
      }

      var homeLm = Geography.Landmass(GetUnitX(homeCp.Unit), GetUnitY(homeCp.Unit));
      var d = cfg.Dests[GetRandomInt(0, cfg.Dests.Count - 1)];
      var dx = d.X;
      var dy = d.Y;
      if (d.CpId != 0)
      {
        var dc = SimpleBot.SafeCp(d.CpId);
        if (dc == null)
        {
          return;
        }

        dx = GetUnitX(dc.Unit);
        dy = GetUnitY(dc.Unit);
      }

      var sfx = AddSpecialEffect(cfg.PortalModel, dx, dy);
      DestroyEffect(sfx);
      SimpleBot.PingAt(dx, dy);
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var fac = p.GetPlayerData().Faction;
        if (fac == null || !InGroup(cfg, fac.Name))
        {
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (!IsUnitType(u, UNIT_TYPE_STRUCTURE) && Geography.Landmass(GetUnitX(u), GetUnitY(u)) == homeLm)
          {
            SetUnitX(u, dx + GetRandomReal(-512f, 512f));
            SetUnitY(u, dy + GetRandomReal(-512f, 512f));
          }
        }

        DestroyGroup(g);
      }

      SimpleBot.Announce(cfg.ArriveMsg);
    }
  }
}
