using System.Collections.Generic;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.53: the trigger of a fallback as a WL quest objective (was TimelineSystem.Check, 0.39.4): complete when ALL HQ
  // units are lost (dead or owned by someone else) after the faction held at least one; incomplete again when one is
  // held again. Its ResearchId (the turning point's condition tech) unlocks the decision at the Hourglass.
  internal sealed class ObjectiveHqLost : Objective
  {
    private readonly int[] _types;
    private List<unit> _units;
    private bool _hadHq;

    public ObjectiveHqLost(int[] types, string text)
    {
      _types = types;
      SetDescription(text);
    }

    internal bool Armed => _units != null;

    // 0.39.4: one whole-map scan for the HQ units, once (at the Hourglass's creation, after the picks)
    internal void Arm()
    {
      _units = new List<unit>();
      if (_types.Length == 0)
      {
        return;
      }

      var g = CreateGroup();
      GroupEnumUnitsInRect(g, GetWorldBounds(), null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        for (var i = 0; i < _types.Length; i++)
        {
          if (_types[i] == t)
          {
            _units.Add(u);
          }
        }
      }

      DestroyGroup(g);
    }

    // returns +1 when the HQ was just lost, -1 when it was just held again, 0 otherwise
    internal int Poll()
    {
      if (_units == null || _units.Count == 0 || ProgressLocked || EligibleFactions.Count == 0)
      {
        return 0;
      }

      var p = EligibleFactions[0].Player;
      if (p == null)
      {
        return 0;
      }

      var held = 0;
      for (var i = 0; i < _units.Count; i++)
      {
        var u = _units[i];
        if (GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && GetOwningPlayer(u) == p)
        {
          held++;
        }
      }

      if (held > 0)
      {
        _hadHq = true;
        if (Progress == QuestProgress.Complete)
        {
          Progress = QuestProgress.Incomplete; // recaptured -> must fall again
          return -1;
        }

        return 0;
      }

      if (!_hadHq || Progress == QuestProgress.Complete)
      {
        return 0; // never held -> not "lost"; or already lost
      }

      Progress = QuestProgress.Complete;
      return 1;
    }
  }
}
