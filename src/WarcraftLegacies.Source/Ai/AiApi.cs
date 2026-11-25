using System.Collections.Generic;
using MacroTools.ControlPoints;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.10: the bot's public surface for code OUTSIDE Ai/ (a fork's own content). Rule: other code
  // may use the bot only through this class; the bot never uses code outside Ai/ except WL's own (checked by
  // tools/shattered-legacies/ai/check_refs.py, LAYER). Add a method here instead of reaching into SimpleBot's internals.
  public static class AiApi
  {
    // --- players ---------------------------------------------------------------------------------------------

    // True for a computer slot the bot plays (also a taken-over leaver, also an autopilot human).
    // target(owner) = the unit to escort now, or null when there is none (0.70.1: Func<player, unit> -- see AiMissions)
    public static void RegisterEscort(player p, System.Func<player, unit> target) => AiMissions.RegisterEscort(p, target);

    // 0.71.0: the escort's next stop (call it whenever it changes); the army walks ahead of the escort towards it
    public static void SetEscortGoal(player p, float x, float y) => AiMissions.SetEscortGoal(p, x, y);

    public static bool IsBot(player p) => SimpleBot.IsBot(p);

    public static void StopBot(player p)
    {
      AiState.TakenOver.Remove(GetPlayerId(p));
      AiState.Autopilot.Remove(GetPlayerId(p));
      if (SimpleBot.IsBot(p))
      {
        SimpleBot.RemoveBot(p);
      }
    }

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

    public static void AddBuildData(Dictionary<int, int[]> menus, Dictionary<int, int[]> products,
      Dictionary<int, int[]> upgrades, Dictionary<int, int[]> required, Dictionary<int, int> halfWidth)
    {
      Merge(AiBuildData.Menus, menus);
      Merge(AiBuildData.Products, products);
      Merge(AiBuildData.Upgrades, upgrades);
      Merge(AiBuildData.Required, required);
      foreach (var entry in halfWidth)
      {
        AiBuildData.HalfWidth[entry.Key] = entry.Value;
      }
    }

    // 0.70.0: Channel spells of a fork's units for the bot's spellcasting (AiSpells); same columns as AiSpellData
    // (tools/shattered-legacies/ai/gen_spells.py writes both). A row for an ability already in the table replaces it.
    public static void AddSpellData(int[] id, int[] kind, int[] who, int[] min, int[] radius, int[] flags,
      string[] order, int[] carrierType, int[] carrierRow)
    {
      AiSpellBook.Add(id, kind, who, min, radius, flags, order, carrierType, carrierRow);
    }

    // 0.72.0: a debug chat command of the fork ("-testkit ..."), gated like the bot's control commands (AiDebug.Access:
    // off in a release build). The command text is read with GetEventPlayerChatString inside the action.
    public static void RegisterDebugCommand(string cmd, System.Action act) => AiDebug.RegisterControl(cmd, act);

    public static void RegisterHeadquarters(string faction, int[] types)
    {
      AiBuildSites.Headquarters[faction] = types;
    }

    // 0.54: the bot takes over a player and plays it from now on, with the full bot treatment -- the same as for a
    // leaving human (AiAutopilot.TakeOverLeaver). For a fork that hands a faction to a free slot mid-game. False if
    // the player has no faction (or it is defeated).
    public static bool TakeOver(player p)
    {
      AiState.Reserved.Remove(GetPlayerId(p));
      return AiAutopilot.TakeOverLeaver(p);
    }

    // 0.65: after a fallback/split, request construction for the arriving faction. No instant building grants.
    public static void GiveStarterProduction(player p)
    {
      if (SimpleBot.IsBot(p))
      {
        AiBuilder.Request(p);
      }
    }

    // 0.57.1: a computer slot of the lobby that stays without a bot until TakeOver (a fork's free slot for a faction
    // that arrives later). Call before the bot starts (map setup).
    public static void ReserveSlot(int playerId)
    {
      AiState.Reserved.Add(playerId);
      SimpleBot.Bots.Remove(Player(playerId)); // the bot may already have listed it (it sets up before the fork)
    }

    // 0.52: the map title shown by -version (default "Warcraft Legacies").
    public static void SetMapTitle(string title)
    {
      AiDebug.MapTitle = title;
    }

    // 0.52: a faction a bot never picks in a faction dialog at game start (by WL faction name). A dialog whose other
    // factions are all excluded keeps WL's default.
    public static void ExcludeFromBotPicks(string factionName)
    {
      AiChoices.Excluded.Add(factionName);
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
