using System;
using System.Collections.Generic;
using System.Linq;
using MacroTools.Localization;
using MacroTools.Save;

namespace MacroTools.UserInterface;

/// <summary>Allows a player to choose between one of two factions at the start of the game.</summary>
public abstract class ChoiceDialogPresenter<TChoice> where TChoice : IChoice
{
  private readonly dialog? _pickDialog = dialog.Create();
  protected readonly List<TChoice> Choices;
  protected bool HasChoiceBeenPicked = false;

  private readonly List<trigger> _triggers = new();
  private readonly string _dialogText;

  /// <summary>Initializes a new instance of the <see cref="ChoiceDialogPresenter{T}"/> class.</summary>
  protected ChoiceDialogPresenter(TChoice[] choices, string dialogText)
  {
    Choices = choices.ToList();
    _dialogText = dialogText;
  }

  protected abstract void OnChoicePicked(player whichPlayer, TChoice choice);
  protected abstract TChoice GetDefaultChoice(player whichPlayer);
  protected abstract bool IsChoiceActive(player whichPlayer, TChoice choice);

  /// <summary>Displays the faction choice to a player.</summary>
  public void Run(player whichPlayer)
  {
    var activeChoices = Choices.Where(x => IsChoiceActive(whichPlayer, x));

    if (activeChoices.Count() == 1)
    {
      OnChoicePicked(whichPlayer, GetDefaultChoice(whichPlayer));
      return;
    }

    // [BOT] a computer player picks without a dialog (see BotChoicePolicy). Humans never get here.
    if (TryBotPick(whichPlayer, activeChoices.ToList()))
    {
      return;
    }

    var choicePicksByButton = new Dictionary<button, TChoice>();
    foreach (var choice in Choices.Where(x => IsChoiceActive(whichPlayer, x)))
    {
      var factionButton = _pickDialog.AddButton(Loc.Get(choice.Name), 0);
      choicePicksByButton[factionButton] = choice;
    }

    timer timer = timer.Create();
    timer.Start(4, false, () =>
    {
      StartChoicePick(whichPlayer, choicePicksByButton);
      @event.ExpiredTimer.Dispose();
    });

    var concludeTimer = timer.Create();
    concludeTimer.Start(24, false, () =>
    {
      ChoiceExpired(whichPlayer);
      @event.ExpiredTimer.Dispose();
    });
  }

  // [BOT] Own method on purpose: Run() declares a local named "timer", which hides the timer TYPE in the whole of Run.
  // The pick happens on the same 4 s delay as the human dialog, so the order of events stays the same.
  private bool TryBotPick(player whichPlayer, List<TChoice> active)
  {
    if (BotChoicePolicy.Pick == null || whichPlayer.Controller != mapcontrol.Computer)
    {
      return false;
    }

    var index = BotChoicePolicy.Pick(whichPlayer, active.Cast<IChoice>().ToList());
    if (index < 0 || index >= active.Count)
    {
      return false;
    }

    var botChoice = active[index];
    _pickDialog?.Dispose();
    timer.Create().Start(4, false, () =>
    {
      OnChoicePicked(whichPlayer, botChoice);
      @event.ExpiredTimer.Dispose();
    });
    return true;
  }

  private void StartChoicePick(player whichPlayer, Dictionary<button, TChoice> choicePicksByButton)
  {
    if (player.LocalPlayer == whichPlayer)
    {
      SaveManager.RunWhenLocalPlayerSettingsReady(() =>
      {
        _pickDialog?.SetMessage(Loc.Get(_dialogText));
        _pickDialog?.SetVisibility(player.LocalPlayer, true);
      });
    }

    foreach (var (button, choice) in choicePicksByButton)
    {
      var pickTrigger = trigger.Create();
      pickTrigger.RegisterButtonEvent(button);
      pickTrigger.AddAction(() =>
      {
        try
        {
          OnChoicePicked(whichPlayer, choice);
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
      });

      _triggers.Add(pickTrigger);
    }
  }

  private void ChoiceExpired(player whichPlayer)
  {
    if (player.LocalPlayer == whichPlayer)
    {
      _pickDialog.SetVisibility(player.LocalPlayer, false);
    }

    if (!HasChoiceBeenPicked)
    {
      OnChoicePicked(whichPlayer, GetDefaultChoice(whichPlayer));
    }

    _pickDialog.Clear();
    _pickDialog.Dispose();

    foreach (var trigger in _triggers)
    {
      trigger.Dispose();
    }
  }
}
