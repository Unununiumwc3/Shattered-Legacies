using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.58: a COUNTER objective (a building block): the faction's own code counts something WL has no objective for (the
  // Kezan Enterprise's Oil Rigs) and calls Add; complete once `target` is reached. Like WL's ObjectiveBuild, but the
  // count comes from code, not from an event.
  internal sealed class ObjectiveCounter : Objective
  {
    private readonly string _what;
    private readonly int _target;
    private int _count;

    public ObjectiveCounter(string what, int target)
    {
      _what = what;
      _target = target;
      Refresh();
    }

    // 0.64: with a place -- a minimap icon there (the Ashtongue's purification sites)
    public ObjectiveCounter(string what, int target, float x, float y)
    {
      _what = what;
      _target = target;
      Position = new WCSharp.Shared.Data.Point(x, y);
      DisplaysPosition = true;
      Refresh();
    }

    internal void Add(int n)
    {
      if (Progress == QuestProgress.Complete)
      {
        return;
      }

      _count += n;
      Refresh();
      if (_count >= _target)
      {
        Progress = QuestProgress.Complete;
      }
    }

    private void Refresh() => SetDescription(_what + " (" + I2S(_count < _target ? _count : _target) + "/" + I2S(_target)
                                             + ")");
  }
}
