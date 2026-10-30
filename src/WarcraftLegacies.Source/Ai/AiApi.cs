using System.Collections.Generic;
using MacroTools.ControlPoints;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.10: the bot's public surface for code OUTSIDE Ai/ (Shattered Legacies, later other forks). Rule: other code
  // may use the bot only through this class; the bot never uses code outside Ai/ except WL's own (checked by
  // tools/ai/check_refs.py, LAYER). Add a method here instead of reaching into SimpleBot's internals.
  public static class AiApi
  {
    // --- players ---------------------------------------------------------------------------------------------

    // True for a computer slot the bot plays (also a taken-over leaver, also an autopilot human).
    public static bool IsBot(player p) => SimpleBot.IsBot(p);

    // True when the bot gives this player its bonuses (real bots, test-mode autopilot; false for fair autopilot).
    public static bool HasPerks(player p) => SimpleBot.HasPerks(p);

    // True for a slot that still plays (human or bot, incl. taken-over leavers).
    public static bool IsActive(player p) => SimpleBot.IsActive(p);

    // --- logging and safety ----------------------------------------------------------------------------------

    public static void Log(string msg) => SimpleBot.Log(msg);

    // Pinned log: one-off events, shown first in the -dump file (max 150 lines).
    public static void LogPin(string msg) => SimpleBot.LogPin(msg);

    // Runs a periodic task isolated: an error is written once to the pinned log ("TICK ERROR in <name>") and never
    // stops the caller. Also records the task for the freeze heartbeat.
    public static void Safe(string name, System.Action task) => SimpleBot.Safe(name, task);

    // --- world -----------------------------------------------------------------------------------------------

    // ControlPointManager.GetFromUnitType THROWS for non-CP ids; this returns null instead.
    public static ControlPoint SafeCp(int unitTypeId) => SimpleBot.SafeCp(unitTypeId);

    // Changes the owner without counting it as a capture in the bot's statistics.
    public static void GiveUnit(unit u, player to) => SimpleBot.GiveUnit(u, to);

    // Open ground for a ~512-wide building 500-2000 around (cx, cy), at least 600 from the spots in usedX/usedY.
    public static bool FindBuildSpot(float cx, float cy, List<float> usedX, List<float> usedY, out float x, out float y)
    {
      var found = AiLegion.FindBuildSpot(cx, cy, usedX, usedY);
      x = SimpleBot._spotX;
      y = SimpleBot._spotY;
      return found;
    }

    // --- data hooks (call once at setup, after AiSetup.Setup) ------------------------------------------------

    // Hero types a faction's bot trains, best first. Replaces the list for that faction name (e.g. a faction
    // renamed by a fork).
    public static void RegisterHeroes(string factionName, List<int> heroTypes)
    {
      AiConfig.Heroes[factionName] = heroTypes;
    }

    // Extra production data (same format as the generated AiData tables). Entries replace the bot's own.
    public static void AddProductionData(Dictionary<int, int[]> trains, Dictionary<int, int[]> altarHeroes,
      Dictionary<int, int[]> requires, Dictionary<int, int[]> heroSkills)
    {
      Merge(AiData.Trains, trains);
      Merge(AiData.AltarHeroes, altarHeroes);
      Merge(AiData.Requires, requires);
      Merge(AiData.HeroSkills, heroSkills);
    }

    // 0.46: the bots' unit budget = base + perCp x held CPs (AiBudget). A different base per faction, e.g. to balance
    // teams; default for all factions: AiBudget.Base (40).
    public static void SetBudgetBase(string factionName, int baseFood)
    {
      AiBudget.SetBase(factionName, baseFood);
    }

    private static void Merge(Dictionary<int, int[]> into, Dictionary<int, int[]> from)
    {
      foreach (var kv in from)
      {
        into[kv.Key] = kv.Value;
      }
    }
  }
}
