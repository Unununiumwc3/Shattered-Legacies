using System.Collections.Generic;
using MacroTools.Factions.Choices;
using MacroTools.UserInterface;
using WarcraftLegacies.Source.Factions.Scourge.Mechanics;

namespace WarcraftLegacies.Source.Ai
{
  // 0.37: picks for WL's click menus (ChoiceDialogPresenter) when the player is a COMPUTER. Without this every menu
  // timed out after 24 s and took option 1 for bots: orcs were always Frostwolf, and the Scourge's plague always
  // chose "No invasion". Humans never reach this code (the hook in MacroTools/UserInterface/ChoiceDialogPresenter.cs
  // only calls it for computer players). Decides from the choice OBJECTS, never from their names -- names are
  // localized per client (comparing them could desync). Randomness: GetRandomInt (synced).
  // -1 = keep WL's default (option 1). Registered in AiSetup.Setup, which runs before WL's faction dialogs.
  internal static class AiChoices
  {
    internal static void Setup()
    {
      BotChoicePolicy.Pick = Pick;
    }

    private static int Pick(player p, IReadOnlyList<IChoice> choices)
    {
      // Faction pick at game start, random for bots: the orc slot (Frostwolf / Warsong, 0.37.1), the Dalaran slot
      // (Dalaran / Gilneas, 0.38) and the Sentinels slot (Sentinels / Draenei, 0.38.1). Illidari / Sunfury stays
      // Basic on purpose (Sunfury is meant as Quel'thalas' fallback, TODO.md).
      if (choices.Count > 0 && choices[0] is FactionChoice)
      {
        for (var i = 0; i < choices.Count; i++)
        {
          if (choices[i] is FactionChoice fc && fc.Faction != null
              && (fc.Faction.Name == "Warsong" || fc.Faction.Name == "Gilneas" || fc.Faction.Name == "The Exodar"))
          {
            var pick = GetRandomInt(0, choices.Count - 1);
            SimpleBot.LogPin("bot pick P" + I2S(GetPlayerId(p)) + ": faction choice " + I2S(pick + 1) + " of "
                             + I2S(choices.Count) + " (random)");
            return pick;
          }
        }

        return -1;
      }

      // Scourge plague: one of the real invasions at random, never "No invasion" (the only choice without a place).
      if (choices.Count > 0 && choices[0] is ScourgeInvasionChoice)
      {
        var real = new List<int>();
        for (var i = 0; i < choices.Count; i++)
        {
          if (choices[i] is ScourgeInvasionChoice sc && sc.Location != null)
          {
            real.Add(i);
          }
        }

        if (real.Count == 0)
        {
          return -1;
        }

        var pick = real[GetRandomInt(0, real.Count - 1)];
        SimpleBot.LogPin("bot pick P" + I2S(GetPlayerId(p)) + ": Scourge invasion option " + I2S(pick + 1) + " (random)");
        return pick;
      }

      return -1; // Warsong pillage (Subdue), Kul Tiras ship (Sail to Westfall): WL's default is what we want
    }
  }
}
