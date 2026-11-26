using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.85.0, a building block (maintainer): PSEUDO-PRODUCTION -- a faction that trains nothing gets units every turn at a
  // spot (the Centaur at their camp by Thunder Bluff, the Defias in the Deadmines). Add() raises the levy (a khan
  // joins, a quest is done). It stops when the player plays another faction.
  internal sealed class TurnLevy
  {
    private readonly player _player;
    private readonly Faction _faction;
    private readonly float _x, _y;
    private readonly List<int> _types = new();

    internal TurnLevy(player p, Faction faction, float x, float y, params int[] types)
    {
      _player = p;
      _faction = faction;
      _x = x;
      _y = y;
      _types.AddRange(types);
      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, Levy, 1, () => Active);
    }

    internal int PerTurn => _types.Count;

    private bool Active => _player.GetPlayerData().Faction == _faction;

    internal void Add(params int[] types) => _types.AddRange(types);

    private void Levy()
    {
      if (!Active)
      {
        return;
      }

      for (var i = 0; i < _types.Count; i++)
      {
        CreateUnit(_player, _types[i], _x + 64f * (i % 5), _y - 64f * (i / 5), 270f);
      }
    }
  }
}
