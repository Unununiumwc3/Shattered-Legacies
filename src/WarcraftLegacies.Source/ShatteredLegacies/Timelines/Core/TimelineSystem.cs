using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Rules;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // The timeline engine (0.39.1 FallbackSystem; 0.53 quest-based). Every WL faction that has a quest-run turning point
  // (TurningPointDef.IsQuest) gets one TurningPointQuest per turning point when it registers
  // (FactionManager.FactionRegistered -- no WL faction file changes). When a trigger fires: its OnTrigger effects
  // (once), then a bot decides at once, a human gets the decision at the Hourglass of Fate. Completing the quest runs
  // the effects: the turning point's own (TurningPointDef.Run, Timelines/Factions/<X>/), the placeholder hero, the
  // announcement and a ping.
  internal static class TimelineSystem
  {
    // 0.60: called with the old faction's name when a real fallback runs (before its effects) -- a fallen faction
    // for whoever cares (the Kezan Enterprise's Refugee Slave Camps; a defeat they take from WL's ScoreStatus)
    internal static List<System.Action<string>> FactionFell = new();

    internal const float CheckEvery = 5f;
    internal const int GraceTurns = 10; // 0.54: a faction that just arrived cannot be eliminated for this long
    internal const int ArrivalIncome = 150; // 0.56 (maintainer): gold per turn for a new start without CPs ...
    internal const int ArrivalTurns = 10;   // ... for this many turns

    private static readonly List<TurningPointQuest> _quests = new();

    internal static void Setup()
    {
      for (var i = 0; i < TimelineCatalog.All.Count; i++)
      {
        var def = TimelineCatalog.All[i];
        if (def.Prepare != null)
        {
          AiApi.Safe("timelines prepare", def.Prepare);
        }
      }

      FactionManager.FactionRegistered += Attach;
      foreach (var f in FactionManager.GetAllFactions())
      {
        Attach(f); // factions registered before this setup (none in 4.8.2's order, but safe)
      }

      Hourglass.Setup();
      var check = CreateTimer();
      TimerStart(check, CheckEvery, true, () => AiApi.Safe("timelines", Check));
    }

    private static void Attach(Faction faction)
    {
      var defs = TimelineCatalog.FindAll(faction.Name);
      for (var d = 0; d < defs.Count; d++)
      {
        var def = defs[d];
        if (!def.IsQuest || HasQuest(def))
        {
          continue; // a definition belongs to one faction, once
        }

        var quest = new TurningPointQuest(def, faction.Icon);
        faction.AddQuest(quest);
        _quests.Add(quest);
        quest.Trigger.ProgressChanged += _ => AiApi.Safe("turning point trigger", () => OnTriggerChanged(quest));
      }
    }

    private static bool HasQuest(TurningPointDef def)
    {
      for (var i = 0; i < _quests.Count; i++)
      {
        if (_quests[i].Def == def)
        {
          return true;
        }
      }

      return false;
    }

    // called by the Hourglass once it exists (62 s, after the picks): the HQ units are found now
    internal static void ArmAll()
    {
      for (var i = 0; i < _quests.Count; i++)
      {
        if (_quests[i].HqLost != null)
        {
          _quests[i].HqLost.Arm();
        }
      }
    }

    private static void Check()
    {
      for (var i = 0; i < _quests.Count; i++)
      {
        var q = _quests[i];
        if (q.HqLost != null && q.HqLost.Armed && q.Progress != QuestProgress.Complete)
        {
          q.HqLost.Poll(); // sets the objective's progress -> OnTriggerChanged
        }
      }
    }

    private static player PlayerOf(TurningPointQuest q) =>
      q.Trigger.EligibleFactions.Count > 0 ? q.Trigger.EligibleFactions[0].Player : null;

    private static void OnTriggerChanged(TurningPointQuest q)
    {
      var p = PlayerOf(q);
      if (p == null || q.Progress == QuestProgress.Complete)
      {
        return;
      }

      if (q.Trigger.Progress != QuestProgress.Complete)
      {
        if (q.HqLost != null)
        {
          DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00" + q.Def.Title + " no longer available:|r your HQ is held again.");
        }

        return;
      }

      if (!q.TriggerRan && q.Def.OnTrigger != null)
      {
        q.TriggerRan = true;
        var def = q.Def;
        Later(() => def.OnTrigger(p, def)); // outside the event that fired the trigger (e.g. a hero's death)
      }

      if (AiApi.IsBot(p) && AiApi.HasPerks(p))
      {
        q.Decided.Progress = QuestProgress.Complete; // bots decide at once -> the quest completes
        return;
      }

      DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00" + q.Def.Title + " is possible now.|r Research it at the "
        + "Hourglass of Fate in the north-east corner of the map when you want to start it.");
      AiApi.LogPin("turning point ready for P" + I2S(GetPlayerId(p)) + " " + q.Def.Faction + ": " + q.Def.Title
                   + " (human decides)");
    }

    // 0.56: a new start lives from CPs it does not have yet -- 150 gold per turn for 10 turns, while the player keeps
    // the faction he got
    private static void GrantArrivalIncome(player p)
    {
      var fac = p.GetPlayerData().Faction;
      var now = MacroTools.GameTime.GameTimeManager.Turn;
      MacroTools.GameTime.GameTimeManager.RegisterOnTurnRange(now + 1, now + ArrivalTurns, () =>
      {
        if (p.GetPlayerData().Faction == fac)
        {
          SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + ArrivalIncome);
        }
      });
    }

    private static void Later(System.Action action)
    {
      var t = CreateTimer();
      TimerStart(t, 0f, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("turning point", action);
      });
    }

    // 0.53, defeat rule (Rules/EliminationRule): a faction that would be eliminated but still has an unused fallback of
    // its own gets the fallback instead -- also when its HQ still stands. Returns false if there is none.
    internal static bool ForceFallback(player p)
    {
      var fac = p.GetPlayerData().Faction;
      for (var i = 0; i < _quests.Count; i++)
      {
        var q = _quests[i];
        if (q.Def.Kind != TurningPointKind.Fallback || q.Progress == QuestProgress.Complete
            || q.Trigger.EligibleFactions.Count == 0 || q.Trigger.EligibleFactions[0] != fac)
        {
          continue;
        }

        q.Trigger.Progress = QuestProgress.Complete;
        q.Decided.Progress = QuestProgress.Complete;
        AiApi.LogPin("turning point forced for P" + I2S(GetPlayerId(p)) + " " + q.Def.Faction + " (no CP left)");
        return true;
      }

      return false;
    }

    // TurningPointQuest.OnComplete: the effects of the turning point. 0.53: one frame later, outside the quest / research
    // event (inside it WL is still updating the faction), after the decision research is taken back (below).
    internal static void RunEffects(TurningPointQuest quest, Faction faction)
    {
      var p = faction.Player;
      if (p == null)
      {
        return;
      }

      var def = quest.Def;
      Later(() => ApplyEffects(def, p));
    }

    private static void ApplyEffects(TurningPointDef def, player p)
    {
      // 0.53 fix: WL's FactionManager records every finished research in the faction's object levels, and a faction
      // change resets / re-applies them -- which failed for the decision research ("Failed to set the object level of
      // Fallback: Sunfury to 0; it is 1 instead"): a research level cannot be lowered while the research is allowed.
      // 0.54.1 fix (the Forsaken split did not happen): the ORDER matters -- first the limit 0 (hides the button; WL's
      // own work-around then sets levels with the limit raised for a moment), then the faction's record 0. A failure
      // there must not stop the turning point: the record is 0 before WL touches the player (Faction.SetObjectLevel).
      var level = GetPlayerTechCount(p, def.Decision, true);
      if (level > 0)
      {
        BlzDecPlayerTechResearched(p, def.Decision, level);
      }

      p.GetPlayerData().SetObjectLimit(def.Decision, 0);
      var fac = p.GetPlayerData().Faction;
      if (fac != null && fac.GetObjectLevel(def.Decision) > 0)
      {
        try
        {
          fac.SetObjectLevel(def.Decision, 0);
        }
        catch (System.Exception ex)
        {
          AiApi.Log("timelines: decision research level kept (" + ex.Message + ")");
        }
      }

      if (def.Run != null)
      {
        if (def.Kind == TurningPointKind.Fallback)
        {
          // 0.60: a fallback counts as the old faction falling (the Kezan Enterprise's Refugee Slave Camps)
          for (var i = 0; i < FactionFell.Count; i++)
          {
            var fell = FactionFell[i];
            AiApi.Safe("faction fell", () => fell(def.Faction));
          }
        }

        def.Run(p, def);
      }

      if (def.HeroType != 0)
      {
        CreateUnit(p, def.HeroType, def.X, def.Y, 270f);
      }

      Hourglass.Refresh(p);                     // the new faction's turning points (none of the old one's)
      if (def.Run != null)
      {
        GrantArrivalIncome(p);                  // real new starts (not the placeholders)
        if (AiApi.IsBot(p))
        {
          // 0.59: a bot never builds -- the new faction gets its production buildings like every bot at the game start
          // (Sunfury / the Kezan Enterprise on a bot slot trained nothing); a moment later, once its base is taken
          var bot = p;
          var t = CreateTimer();
          TimerStart(t, 3f, false, () =>
          {
            DestroyTimer(t);
            AiApi.Safe("arrival production", () => AiApi.GiveStarterProduction(bot));
          });
        }
      }

      EliminationRule.GiveGrace(p, GraceTurns); // a new start cannot be eliminated at once
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, def.Announcement);
      }

      PingMinimap(def.X, def.Y, 5f);
      AiApi.LogPin("TURNING POINT P" + I2S(GetPlayerId(p)) + " " + def.Faction + " -> " + def.Title);
    }
  }
}
