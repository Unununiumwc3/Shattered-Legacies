using MacroTools.Factions.Choices;
using WarcraftLegacies.Source.Factions.Dalaran;
using WarcraftLegacies.Source.Factions.Draenei;
using WarcraftLegacies.Source.Factions.Frostwolf;
using WarcraftLegacies.Source.Factions.Illidari;
using WarcraftLegacies.Source.Factions.Sentinels;
using WarcraftLegacies.Source.Factions.Sunfury;
using WarcraftLegacies.Source.Factions.Warsong;

namespace WarcraftLegacies.Source.Setup;

public static class FactionChoiceDialogSetup
{
  public static void Setup()
  {
    var illidari = new FactionChoice
    {
      Faction = new IllidariFaction(),
      Difficulty = FactionLearningDifficulty.Basic,
      StartingArea = Regions.IllidanStartingPosition
    };
    var sunfury = new FactionChoice
    {
      Faction = new SunfuryFaction(),
      Difficulty = FactionLearningDifficulty.Advanced,
      StartingArea = Regions.SunfuryStartingPosition
    };
    // [SL] (0.53): Sunfury is Quel'thalas' fallback, not a pick -- the Illidari slot always plays the Illidari (one choice:
    // no dialog), and Sunfury's Netherstorm areas stay for the fallback (no OnNotPicked cleanup).
    new FactionChoiceDialogPresenter(illidari).Run(player.Create(15));

    var dalaran = new FactionChoice
    {
      Faction = new DalaranFaction(),
      Difficulty = FactionLearningDifficulty.Basic,
      StartingArea = Regions.DalaStartPos
    };
    // [SL] (0.76.0): Gilneas is Stormwind's fallback, not a pick (ShatteredLegacies/Timelines/Factions/Gilneas) -- the
    // Dalaran slot always plays Dalaran (one choice: no dialog)
    new FactionChoiceDialogPresenter(dalaran).Run(player.Create(7));

    var sentinels = new FactionChoice
    {
      Faction = new SentinelsFaction(),
      Difficulty = FactionLearningDifficulty.Basic,
      StartingArea = Regions.SentDraeSharedStartPos
    };
    var draenei = new FactionChoice

    {
      Faction = new DraeneiFaction(),
      Difficulty = FactionLearningDifficulty.Advanced,
      StartingArea = Regions.SentDraeSharedStartPos,
      RequiresCheats = false
    };
    new FactionChoiceDialogPresenter(sentinels, draenei).Run(player.Create(18));
  }
}
