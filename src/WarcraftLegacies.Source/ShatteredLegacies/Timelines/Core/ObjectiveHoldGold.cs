using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.58: an objective "hold this much gold at once" (a building block for end goals: the Kezan Enterprise's 20,000).
  // Checked every 2 s for the faction's player; complete when reached (it stays complete -- spending afterwards does
  // not undo it). Progress for other displays (the scoreboard): Gold().
  internal sealed class ObjectiveHoldGold : Objective
  {
    private readonly int _amount;
    private int _shown = -1;

    public ObjectiveHoldGold(int amount)
    {
      _amount = amount;
      Refresh(0);
      var t = CreateTimer();
      TimerStart(t, 2f, true, () => AiApi.Safe("hold gold", Check));
    }

    internal int Amount => _amount;

    // the gold the faction's player holds now (0 if it has none)
    internal int Gold()
    {
      if (EligibleFactions.Count == 0 || EligibleFactions[0].Player == null)
      {
        return 0;
      }

      return GetPlayerState(EligibleFactions[0].Player, PLAYER_STATE_RESOURCE_GOLD);
    }

    private void Check()
    {
      if (Progress == QuestProgress.Complete || EligibleFactions.Count == 0)
      {
        return;
      }

      var gold = Gold();
      Refresh(gold);
      if (gold >= _amount)
      {
        Progress = QuestProgress.Complete;
      }
    }

    // the quest log shows the count in steps of 500 (no text update every 2 s)
    private void Refresh(int gold)
    {
      var step = (gold >= _amount ? _amount : gold) / 500 * 500;
      if (step == _shown)
      {
        return;
      }

      _shown = step;
      SetDescription("Hold " + I2S(_amount) + " gold at once (" + I2S(step) + "+)");
    }
  }
}
