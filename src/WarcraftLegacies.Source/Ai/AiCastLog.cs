using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.73.0: every spell a bot's unit casts, whoever ordered it (the engine's own unit AI for spells built on stock
  // abilities, the bot's AiSpells for Channel spells, autocast). Dump only: answers "do bot heroes use their
  // spells at all?" -- the SPELLS section counts the bot's own Channel casts only. One event per cast, no scans.
  internal static class AiCastLog
  {
    private static trigger _trig;
    private static readonly Dictionary<int, int> _byAbility = new();
    private static readonly List<int> _abilities = new(); // first-cast order: deterministic listing
    private static readonly int[] _byBot = new int[24];
    private static int _heroCasts;
    private static int _unitCasts;

    internal static void Setup()
    {
      _trig = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_trig, Player(i), EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      }

      TriggerAddAction(_trig, OnCast);
    }

    private static void OnCast()
    {
      var u = GetTriggerUnit();
      var p = GetOwningPlayer(u);
      if (!SimpleBot.IsBot(p))
      {
        return;
      }

      var id = GetSpellAbilityId();
      if (_byAbility.TryGetValue(id, out var n))
      {
        _byAbility[id] = n + 1;
      }
      else
      {
        _byAbility[id] = 1;
        _abilities.Add(id);
      }

      _byBot[GetPlayerId(p)]++;
      if (IsUnitType(u, UNIT_TYPE_HERO))
      {
        _heroCasts++;
      }
      else
      {
        _unitCasts++;
      }
    }

    internal static void Lines(List<string> o)
    {
      o.Add("all casts by bot units (engine unit AI + bot + autocast, 0.73.0): heroes " + I2S(_heroCasts) + ", other units "
            + I2S(_unitCasts) + ", different abilities " + I2S(_abilities.Count));
      var s = "all casts per bot:";
      for (var pid = 0; pid < 24; pid++)
      {
        if (_byBot[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(_byBot[pid]) + ";";
        }
      }

      o.Add(s);
      // top 40 by count (ties: first cast first)
      var shown = new HashSet<int>();
      for (var k = 0; k < 40 && k < _abilities.Count; k++)
      {
        var best = 0;
        var bestN = -1;
        for (var i = 0; i < _abilities.Count; i++)
        {
          var id = _abilities[i];
          if (!shown.Contains(id) && _byAbility[id] > bestN)
          {
            best = id;
            bestN = _byAbility[id];
          }
        }

        shown.Add(best);
        o.Add("cast " + GetObjectName(best) + " (" + AiDebug.RawCode(best) + "): " + I2S(bestN));
      }
    }
  }
}
