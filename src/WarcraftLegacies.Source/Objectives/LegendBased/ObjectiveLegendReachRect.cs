using MacroTools.Legends;
using MacroTools.Localization;
using MacroTools.Quests;
using MacroTools.Wrappers;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.Objectives.LegendBased;

public sealed class ObjectiveLegendReachRect : Objective, IObjectiveVisit
{

  // [SL] (0.76.0): read by the bot's quest runner
  public rect VisitRect => _targetRect;
  public Legend? VisitLegend => _legend;
  public int VisitUnitType => 0;
  public int VisitMinLevel => 0;
  public bool VisitHeroOnly => true;
  private readonly TriggerWrapper _entersRect = new();

  private readonly Legend _legend;
  private readonly region _target;
  private readonly rect _targetRect;

  public ObjectiveLegendReachRect(LegendaryHero legendaryHero, Rectangle targetRect, string rectName)
  {
    _targetRect = targetRect.Rect;
    _target = RectToRegion(_targetRect);
    _legend = legendaryHero;
    SetDescription("{legend} reaches {rect}", ("{legend}", legendaryHero.Name), ("{rect}", Loc.Get(rectName)));
    _entersRect.Trigger.RegisterEnterRegion(_target);
    _entersRect.Trigger.AddAction(OnRegionEnter);
    PingPath = "MinimapQuestTurnIn";
    DisplaysPosition = true;
    Position = new(_targetRect.CenterX, _targetRect.CenterY);
  }

  private static region RectToRegion(rect whichRect)
  {
    var rectRegion = region.Create();
    rectRegion.AddRect(whichRect);
    return rectRegion;
  }

  private void OnRegionEnter()
  {
    if (@event.Region == _target && _legend.Unit.Alive && @event.Unit == _legend.Unit)
    {
      Progress = QuestProgress.Complete;
    }
  }
}
