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
  // 0.35.1: bot strength vote. 70 s into the game every human player gets a dialog; the choice sets the bonus gold
  // every bot receives per production turn (SimpleBot.BotGoldBonus; 25 = the old default, about 100 gold/min).
  // Majority wins; a tie goes to the option closest to "Veteran"; no vote = Veteran. Shown only if there are bots.
  // The debug commands -gold0 ... -gold200 still override it afterwards.
  internal static class AiGoldVote
  {
    private const float ShowAt = 49f; // 0.39.1: the slot of WL's game-mode vote, which is gone (Closed Alliance forced)
    private const float VoteTime = 20f;
    private const int DefaultOption = 2;

    private static readonly string[] Names =
    {
      "Recruit -- bots get no bonus gold",
      "Soldier -- bots get half bonus gold",
      "Veteran -- normal bonus gold (default)",
      "Commander -- 1.5x bonus gold",
      "Warlord -- 2x bonus gold, huge armies (may cause lag)",
    };

    private static readonly string[] Short = { "Recruit", "Soldier", "Veteran", "Commander", "Warlord" };
    private static readonly int[] Gold = { 0, 13, 25, 38, 50 };

    private static readonly int[] Votes = new int[5];
    private static readonly button[] Buttons = new button[5];
    private static dialog _dialog;
    private static int _voters;
    private static int _cast;
    private static bool _closed;

    public static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, ShowAt, false, Show);
    }

    private static void Show()
    {
      if (SimpleBot.BotCount == 0)
      {
        return;
      }

      _dialog = DialogCreate();
      DialogSetMessage(_dialog, "Bot strength -- bonus gold for computer players");
      for (var i = 0; i < Names.Length; i++)
      {
        Buttons[i] = DialogAddButton(_dialog, Names[i], 0);
      }

      var tr = CreateTrigger();
      TriggerRegisterDialogEvent(tr, _dialog);
      TriggerAddAction(tr, OnClick);
      for (var i = 0; i < 24; i++)
      {
        var p = Player(i);
        if (GetPlayerSlotState(p) == PLAYER_SLOT_STATE_PLAYING && GetPlayerController(p) == MAP_CONTROL_USER)
        {
          DialogDisplay(p, _dialog, true);
          _voters++;
        }
      }

      if (_voters == 0)
      {
        Close();
        return;
      }

      var t = CreateTimer();
      TimerStart(t, VoteTime, false, Close);
    }

    private static void OnClick()
    {
      if (_closed)
      {
        return;
      }

      var b = GetClickedButton();
      for (var i = 0; i < Buttons.Length; i++)
      {
        if (Buttons[i] == b)
        {
          Votes[i]++;
        }
      }

      DialogDisplay(GetTriggerPlayer(), _dialog, false);
      _cast++;
      if (_cast >= _voters)
      {
        Close();
      }
    }

    private static void Close()
    {
      if (_closed)
      {
        return;
      }

      _closed = true;
      var best = DefaultOption;
      for (var i = 0; i < Votes.Length; i++)
      {
        var di = i > DefaultOption ? i - DefaultOption : DefaultOption - i;
        var db = best > DefaultOption ? best - DefaultOption : DefaultOption - best;
        var closer = di < db;
        if (Votes[i] > Votes[best] || (Votes[i] == Votes[best] && Votes[i] > 0 && closer))
        {
          best = i;
        }
      }

      SimpleBot.BotGoldBonus = Gold[best];
      for (var i = 0; i < 24; i++)
      {
        if (_dialog != null)
        {
          DialogDisplay(Player(i), _dialog, false);
        }

        DisplayTimedTextToPlayer(Player(i), 0, 0, 12, "|cff00ffffBot strength: " + Short[best] + "|r (" + I2S(Votes[best])
          + " of " + I2S(_voters) + " votes) -- bots get " + I2S(Gold[best]) + " bonus gold per production turn.");
      }

      SimpleBot.LogPin("bot strength vote: " + Short[best] + " (" + I2S(Votes[best]) + "/" + I2S(_voters)
                       + " votes) -> bonus gold " + I2S(Gold[best]));
    }
  }
}
