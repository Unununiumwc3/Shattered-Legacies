using MacroTools.Legends;

namespace WarcraftLegacies.Source.Objectives;

/// <summary>
/// [SL] (0.76.0) An objective that is completed by bringing a unit to a place. Lets the bot's quest runner
/// (Ai/AiQuestRunner.cs) read what the objective wants without one-off code per quest.
/// </summary>
public interface IObjectiveVisit
{
  /// <summary>Where the unit has to be.</summary>
  rect VisitRect { get; }

  /// <summary>The legend that has to go there, or null for any fitting unit.</summary>
  Legend? VisitLegend { get; }

  /// <summary>The unit type that has to go there, or 0.</summary>
  int VisitUnitType { get; }

  /// <summary>The hero level the unit needs, or 0.</summary>
  int VisitMinLevel { get; }

  /// <summary>Only a hero counts.</summary>
  bool VisitHeroOnly { get; }
}
