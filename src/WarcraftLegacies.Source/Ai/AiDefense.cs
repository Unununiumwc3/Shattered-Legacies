namespace WarcraftLegacies.Source.Ai
{
  // 0.43.2: which CP (or capital) the army defends -- with REACH and HYSTERESIS. Before, Rule 0 took any damaged own
  // CP anywhere on the map, every service, without minimum times: the army flipped attack <-> defence every 15-90 s
  // and walked back and forth ("stop go stop go") -- the Scourge between Northrend and Lordaeron, the Druids across
  // Kalimdor (0.43.1 game: up to 8 attack -> defence switches per bot in the last minutes of the log).
  // - Reach: only on the army's landmass, and within DefendReach of it (an own capital on that landmass: any distance).
  // - Keep a defence at least DefendMinHold, and as long as enemies are still there.
  // - After a defence ended, no new one for DefendCooldown -- unless the threat is within CloseThreat of the army.
  internal static class AiDefense
  {
    internal const float DefendReach = 9000f;
    private const float DefendMinHold = 30f;
    private const float DefendCooldown = 45f;
    private const float CloseThreat = 4000f;

    private static unit[] _current = new unit[AiTheatres.Agents];
    private static float[] _since = new float[AiTheatres.Agents];
    private static float[] _ended = new float[AiTheatres.Agents];
    internal static int Kept;
    internal static int Skipped;

    // (ax, ay) = the army anchor. Null = no defence: the caller attacks.
    internal static unit Pick(player p, int pid, float ax, float ay)
    {
      if (pid < 0 || pid >= 24)
      {
        return SimpleBot.FindDefenseTarget(p, ax, ay);
      }

      var now = SimpleBot.Clock;
      var cur = _current[AiTheatres.S(pid)];
      if (cur != null)
      {
        var alive = GetUnitTypeId(cur) != 0 && GetUnitState(cur, UNIT_STATE_LIFE) > 0.405f && GetOwningPlayer(cur) == p;
        var enemies = alive ? SimpleBot.CountEnemiesNear(p, GetUnitX(cur), GetUnitY(cur)) : 0;
        if (alive && (now - _since[AiTheatres.S(pid)] < DefendMinHold || enemies > SimpleBot.DefendThreshold))
        {
          SimpleBot.NoteDefCount(enemies); // 0.46: for the log (was "0 enemy units" while kept)
          Kept++;
          return cur;
        }

        _current[AiTheatres.S(pid)] = null;
        _ended[AiTheatres.S(pid)] = now;
      }

      var cand = SimpleBot.FindDefenseTarget(p, ax, ay);
      if (cand == null)
      {
        return null;
      }

      if (!AiTheatres.Allowed(AiRoutes.CompAt(GetUnitX(cand), GetUnitY(cand))))
      {
        Skipped++;
        return null; // 0.47: another theatre's land -- that theatre defends it
      }

      if (_ended[AiTheatres.S(pid)] > 0f && now - _ended[AiTheatres.S(pid)] < DefendCooldown && !Near(cand, ax, ay, CloseThreat))
      {
        Skipped++;
        return null; // just left a defence: finish the attack first unless the threat is close
      }

      _current[AiTheatres.S(pid)] = cand;
      _since[AiTheatres.S(pid)] = now;
      return cand;
    }

    private static bool Near(unit u, float x, float y, float r)
    {
      var dx = GetUnitX(u) - x;
      var dy = GetUnitY(u) - y;
      return dx * dx + dy * dy <= r * r;
    }
  }
}
