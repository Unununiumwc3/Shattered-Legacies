using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // The timeline engine (0.39.1 FallbackSystem; 0.53 quest-based). Every WL faction that has a quest-run turning point
  // (TurningPointDef.IsQuest) gets a TurningPointQuest when it registers (FactionManager.FactionRegistered -- no WL
  // faction file changes). Every 5 s the HQ objectives are checked: a bot decides at once, a human gets the decision
  // at the Hourglass of Fate. Completing the quest runs the effects: the faction's own (TurningPointDef.Run,
  // Timelines/Factions/<X>/) and the placeholder hero, the announcement and a ping.
  internal static class TimelineSystem
  {
    internal const float CheckEvery = 5f;

    private static readonly List<TurningPointQuest> _quests = new();

    internal static void Setup()
    {
      FactionManager.FactionRegistered += Attach;
      foreach (var f in FactionManager.GetAllFactions())
      {
        Attach(f); // factions registered before this setup (none in 4.8.2's order, but safe)
      }

      for (var i = 0; i < TimelineCatalog.All.Count; i++)
      {
        var def = TimelineCatalog.All[i];
        if (def.Prepare != null)
        {
          AiApi.Safe("timelines prepare", def.Prepare);
        }
      }

      Hourglass.Setup();
      var check = CreateTimer();
      TimerStart(check, CheckEvery, true, () => AiApi.Safe("timelines", Check));
    }

    private static void Attach(Faction faction)
    {
      var def = TimelineCatalog.Find(faction.Name);
      if (def == null || !def.IsQuest)
      {
        return;
      }

      for (var i = 0; i < _quests.Count; i++)
      {
        if (_quests[i].Def == def)
        {
          return; // a definition belongs to one faction
        }
      }

      var quest = new TurningPointQuest(def, faction.Icon);
      faction.AddQuest(quest);
      _quests.Add(quest);
    }

    // called by the Hourglass once it exists (62 s, after the picks): the HQ units are found now
    internal static void ArmAll()
    {
      for (var i = 0; i < _quests.Count; i++)
      {
        _quests[i].HqLost.Arm();
      }
    }

    private static void Check()
    {
      for (var i = 0; i < _quests.Count; i++)
      {
        var q = _quests[i];
        if (!q.HqLost.Armed || q.Progress == QuestProgress.Complete)
        {
          continue;
        }

        var change = q.HqLost.Poll();
        var p = q.HqLost.EligibleFactions.Count > 0 ? q.HqLost.EligibleFactions[0].Player : null;
        if (change == 0 || p == null)
        {
          continue;
        }

        if (change < 0)
        {
          DisplayTimedTextToPlayer(p, 0, 0, 15, "|cffffcc00Fallback no longer available:|r your HQ is held again.");
          continue;
        }

        if (AiApi.IsBot(p) && AiApi.HasPerks(p))
        {
          q.Decided.Progress = QuestProgress.Complete; // bots decide at once -> the quest completes
        }
        else
        {
          DisplayTimedTextToPlayer(p, 0, 0, 20, "|cffffcc00Your fallback is ready:|r " + q.Def.Name + ". Research it at "
            + "the Hourglass of Fate in the north-east corner of the map when you want to start it.");
          AiApi.LogPin("turning point ready for P" + I2S(GetPlayerId(p)) + " " + q.Def.Faction + " (human decides)");
        }
      }
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
            || q.HqLost.EligibleFactions.Count == 0 || q.HqLost.EligibleFactions[0] != fac)
        {
          continue;
        }

        q.HqLost.Progress = QuestProgress.Complete;
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

      var t = CreateTimer();
      TimerStart(t, 0f, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("turning point", () => ApplyEffects(quest.Def, p));
      });
    }

    private static void ApplyEffects(TurningPointDef def, player p)
    {
      // 0.53 fix: WL's FactionManager records every finished research in the faction's object levels, and a faction
      // swap resets them to 0 -- which fails for the decision research ("Failed to set the object level of Fallback:
      // Sunfury to 0; it is 1 instead") and stopped the swap. Taking the research back first makes that reset a no-op;
      // the limit 0 keeps the button from coming back.
      var level = GetPlayerTechCount(p, def.Decision, true);
      if (level > 0)
      {
        BlzDecPlayerTechResearched(p, def.Decision, level);
      }

      p.GetPlayerData().SetObjectLimit(def.Decision, 0);
      if (def.Run != null)
      {
        def.Run(p, def);
      }

      if (def.HeroType != 0)
      {
        CreateUnit(p, def.HeroType, def.X, def.Y, 270f);
      }

      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, def.Announcement);
      }

      PingMinimap(def.X, def.Y, 5f);
      AiApi.LogPin("TURNING POINT P" + I2S(GetPlayerId(p)) + " " + def.Faction + " -> " + def.Name);
    }
  }
}
