using MacroTools.Legends;
using MacroTools.Extensions;
using MacroTools.Localization;
using MacroTools.Quests;
using MacroTools.Wrappers;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.Objectives.UnitBased;

/// <summary>
/// Defines an <see cref="Objective"/> that completes when ANY hero of a certain level or higher enters a specific region.
/// </summary>
public sealed class ObjectiveAnyHeroWithLevelReachRect : Objective, IHasCompletingUnit, IObjectiveVisit
{

  // [SL] (0.76.0): read by the bot's quest runner
  public rect VisitRect => _targetRect;
  public Legend? VisitLegend => null;
  public int VisitUnitType => 0;
  public int VisitMinLevel => _targetLevel;
  public bool VisitHeroOnly => true;
  private readonly TriggerWrapper _entersRect = new();

  private readonly rect _targetRect;
  private readonly int _targetLevel;

  /// <summary>
  /// Initializes a new instance of the <see cref="ObjectiveHeroWithLevelInRect"/> class.
  /// </summary>
  /// <param name="targetLevel">Minimum required level to complete the objective</param>
  /// <param name="targetRect">Region that the unit has to enter in order to complete the objective</param>
  /// <param name="rectName">Display name of the region the unit has to enter. Used for flavor only.</param>
  public ObjectiveAnyHeroWithLevelReachRect(int targetLevel, Rectangle targetRect, string rectName)
  {
    _targetRect = targetRect.Rect;
    _targetLevel = targetLevel;
    var target = RectToRegion(_targetRect);
    SetDescription("Any level {level}+ hero reaches {rect}",
      ("{level}", targetLevel.ToString()), ("{rect}", Loc.Get(rectName)));
    _entersRect.Trigger.RegisterEnterRegion(target);
    _entersRect.Trigger.AddAction(OnRegionEnter);
    PingPath = "MinimapQuestTurnIn";
    DisplaysPosition = true;
    Position = new(_targetRect.CenterX, _targetRect.CenterY);
  }

  /// <inheritdoc />
  public unit? CompletingUnit { get; private set; }

  /// <inheritdoc />
  public string CompletingUnitName => CompletingUnit != null ? CompletingUnit.GetProperName() : Loc.Get("an unknown hero");

  private static region RectToRegion(rect whichRect)
  {
    var rectRegion = region.Create();
    rectRegion.AddRect(whichRect);
    return rectRegion;
  }

  private void OnRegionEnter()
  {
    var triggerUnit = @event.Unit;
    if (!triggerUnit.Alive || triggerUnit.HeroLevel < _targetLevel)
    {
      return;
    }

    CompletingUnit = triggerUnit;
    Progress = QuestProgress.Complete;
  }
}
