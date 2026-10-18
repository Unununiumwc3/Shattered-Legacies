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
  // 0.35.0: -autopilot -- a human player hands their faction to the AI and takes it back with the same command.
  // While on, the slot is in SimpleBot.Bots, so it is played exactly like a bot (targets, defense, production,
  // quests it can see). Bot BONUSES depend on the command switch (AiDebug.Access):
  //   Off (release build) -> FAIR: no gold subsidy, no free techs/upgrades/revives, no starter kit, no scripted
  //                          missions or waves; 60 s cooldown between toggles. Usable in real multiplayer.
  //   HostOnly/Everyone   -> TEST: full bot treatment, no cooldown -- a test game shows how real bots perform.
  // No extra vision. Always available, also when AiDebug.Access is Off.
  internal static class AiAutopilot
  {
    private const float Cooldown = 60f;

    public static void Setup()
    {
      var tr = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerChatEvent(tr, Player(i), "-autopilot", true);
      }

      TriggerAddAction(tr, OnCommand);
    }

    private static bool Fair => AiDebug.Access == AiDebug.CommandAccess.Off;

    private static void OnCommand()
    {
      var p = GetTriggerPlayer();
      var pid = GetPlayerId(p);
      if (GetPlayerController(p) != MAP_CONTROL_USER)
      {
        return;
      }

      if (Fair && AiState.AutopilotToggled.ContainsKey(pid) && SimpleBot.Clock - AiState.AutopilotToggled[pid] < Cooldown)
      {
        var wait = R2I(Cooldown - (SimpleBot.Clock - AiState.AutopilotToggled[pid])) + 1;
        DisplayTimedTextToPlayer(p, 0, 0, 6, "|cffffcc00-autopilot|r: wait " + I2S(wait) + " s before switching again.");
        return;
      }

      AiState.AutopilotToggled[pid] = SimpleBot.Clock;
      var on = !AiState.Autopilot.Contains(pid);
      if (on)
      {
        AiState.Autopilot.Add(pid);
        SimpleBot.AddBot(p);
      }
      else
      {
        AiState.Autopilot.Remove(pid);
        SimpleBot.RemoveBot(p);
      }

      var mode = Fair ? "fair: own resources only" : "test: full bot bonuses";
      for (var i = 0; i < 24; i++)
      {
        var msg = i == pid
          ? (on ? "|cff00ffffAutopilot ON|r (" + mode + ") -- the AI plays your faction. Type -autopilot to take over."
                : "|cff00ffffAutopilot OFF|r -- you are in control again.")
          : GetPlayerName(p) + (on ? " switched autopilot on (" + mode + ")." : " switched autopilot off.");
        DisplayTimedTextToPlayer(Player(i), 0, 0, 10, msg);
      }

      SimpleBot.LogPin("P" + I2S(pid) + " " + SimpleBot.FacName(p) + ": autopilot " + (on ? "ON (" + mode + ")" : "OFF"));
    }
  }
}
