using MacroTools.Quests;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.63, a building block: an objective at a PLACE (minimap icon there) that the faction's own code completes
  // (Complete()) -- e.g. "Sell a refugee at Booty Bay" (the Kezan Enterprise's slave camps; maintainer: the quest
  // needed map markers).
  internal sealed class ObjectiveAt : Objective
  {
    public ObjectiveAt(string text, float x, float y)
    {
      SetDescription(text);
      Position = new Point(x, y);
      DisplaysPosition = true;
    }

    internal void Complete()
    {
      if (Progress != QuestProgress.Complete)
      {
        Progress = QuestProgress.Complete;
      }
    }
  }
}
