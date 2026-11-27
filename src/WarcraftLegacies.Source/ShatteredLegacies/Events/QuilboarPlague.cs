using System.Collections.Generic;
using MacroTools.DummyCasters;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): THE PLAGUE GOES SOUTH -- a Scourge option (the Hourglass and the Frozen Throne: the same
  // research), open once WL's Plague of Undeath has run; bots take it at once.
  // - Amnennar the Coldbringer, a quilboar lich, rises for the Scourge at Razorfen Downs (southern Barrens).
  // - 30 plague boars (Neutral Hostile, they never attack, no bounty) march in six bands on the strongholds of Kalimdor
  //   (Orgrimmar, Thunder Bluff, Nordrassil, Auberdine, Feathermoon, Theramore). Every 5 s each boar infects up to 3
  //   units within 500 (not the Scourge's team, no buildings): the ZOMBIE PLAGUE (a debuff that never ends by itself --
  //   only a dispel or purge cures it) eats 2 life a second (never below 1), and an infected unit that dies rises as a
  //   Scourge zombie (heroes do not). The boars die easily -- killing them is what stops the plague.
  internal static class QuilboarPlague
  {
    private const float DownsX = -12700f; // Razorfen Downs (map_query: open ground by the Razormane camps)
    private const float DownsY = -8200f;
    private const int Bands = 6;
    private const int PerBand = 5;
    private const float Range = 500f;
    private const int PerCast = 3;
    private static readonly int Zombie = FourCC("nzom");

    private static readonly float[] TargetX = { -7424f, -15552f, -10496f, -16640f, -20288f, -7488f };
    private static readonly float[] TargetY = { -448f, -4096f, 10944f, 10048f, -8768f, -8768f };

    private static readonly List<unit> _boars = new();
    private static readonly HashSet<unit> _infected = new();
    private static readonly Dictionary<unit, int> _age = new(); // seconds since infected (the missile needs a moment)
    private static player _scourge;
    private static bool _started;

    internal static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, 5f, true, () => AiApi.Safe("plague goes south", Watch));
      var done = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(done, Player(i), EVENT_PLAYER_UNIT_RESEARCH_FINISH, null);
      }

      TriggerAddAction(done, () =>
      {
        if (GetResearched() == Objects088.PlagueGoesSouth && !_started)
        {
          var p = GetTriggerPlayer();
          AiApi.Safe("plague start", () => Start(p));
        }
      });
    }

    // the condition once the Plague of Undeath has run; bots research it at once
    private static void Watch()
    {
      if (_started)
      {
        return;
      }

      for (var i = 0; i < 24; i++)
      {
        var p = Player(i);
        if (p.GetPlayerData().Faction?.Name != "Scourge"
            || GetPlayerTechCount(p, UPGRADE_R009_QUEST_COMPLETED_PLAGUE_OF_UNDEATH, true) == 0)
        {
          continue;
        }

        SetPlayerTechResearched(p, Objects088.PlagueGoesSouthCondition, 1);
        var glass = Hourglass.Get(i);
        if (AiApi.IsBot(p) && glass != null)
        {
          AiApi.FreeOrder(p, glass, Objects088.PlagueGoesSouth);
        }
      }
    }

    private static void Start(player p)
    {
      _started = true;
      _scourge = p;
      var lich = CreateUnit(p, Objects088.Amnennar, DownsX, DownsY, 270f);
      SetHeroLevel(lich, 6, false);
      var hostile = Player(PLAYER_NEUTRAL_AGGRESSIVE);
      for (var b = 0; b < Bands; b++)
      {
        for (var i = 0; i < PerBand; i++)
        {
          var boar = CreateUnit(hostile, Objects088.PlagueBoar, DownsX + 64f * i, DownsY + 96f * b, 270f);
          IssuePointOrder(boar, "move", TargetX[b] + GetRandomReal(-200f, 200f), TargetY[b] + GetRandomReal(-200f, 200f));
          _boars.Add(boar);
        }
      }

      var spread = CreateTimer();
      TimerStart(spread, 5f, true, () => AiApi.Safe("plague spread", Spread));
      var eat = CreateTimer();
      TimerStart(eat, 1f, true, () => AiApi.Safe("plague eats", Eat));
      var death = CreateTrigger();
      for (var i = 0; i < 28; i++)
      {
        TriggerRegisterPlayerUnitEvent(death, Player(i), EVENT_PLAYER_UNIT_DEATH, null);
      }

      TriggerAddAction(death, () =>
      {
        var u = GetTriggerUnit();
        _age.Remove(u);
        if (!_infected.Remove(u) || IsUnitType(u, UNIT_TYPE_HERO))
        {
          return;
        }

        CreateUnit(_scourge, Zombie, GetUnitX(u), GetUnitY(u), GetUnitFacing(u));
      });
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 25, "|cff00ff00The plague goes south!|r Amnennar the Coldbringer "
          + "rises in Razorfen Downs, and plague boars swarm out towards the strongholds of Kalimdor. Kill the boars -- "
          + "and dispel what they leave behind.");
      }

      PingMinimap(DownsX, DownsY, 6f);
      AiApi.LogPin("The plague goes south (P" + I2S(GetPlayerId(p)) + ")");
    }

    private static void Spread()
    {
      var caster = DummyCasterManager.GetGlobalDummyCaster();
      for (var i = _boars.Count - 1; i >= 0; i--)
      {
        var boar = _boars[i];
        if (GetUnitTypeId(boar) == 0 || GetUnitState(boar, UNIT_STATE_LIFE) <= 0.405f)
        {
          _boars.RemoveAt(i);
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsInRange(g, GetUnitX(boar), GetUnitY(boar), Range, null);
        var cast = 0;
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (cast >= PerCast || !Victim(u))
          {
            continue;
          }

          caster.CastUnit(boar, Objects088.ZombiePlague, ORDER_FAERIE_FIRE, 1, u, DummyCastOriginType.Caster);
          _infected.Add(u);
          _age[u] = 0;
          cast++;
        }

        DestroyGroup(g);
      }
    }

    private static bool Victim(unit u)
    {
      var owner = GetOwningPlayer(u);
      return GetPlayerId(owner) < 24 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && !IsUnitType(u, UNIT_TYPE_STRUCTURE)
             && !IsUnitType(u, UNIT_TYPE_MECHANICAL) && GetUnitAbilityLevel(u, Objects088.ZombiePlagueBuff) == 0
             && !_infected.Contains(u)
             && owner != _scourge && !IsPlayerAlly(owner, _scourge);
    }

    // 2 life a second; a cured unit (no buff any more, 3 s after the cast) is no longer infected
    private static void Eat()
    {
      var gone = new List<unit>();
      foreach (var u in _infected)
      {
        if (GetUnitTypeId(u) == 0)
        {
          gone.Add(u);
          continue;
        }

        if (GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var age = _age.TryGetValue(u, out var a) ? a + 1 : 1;
        _age[u] = age;
        if (GetUnitAbilityLevel(u, Objects088.ZombiePlagueBuff) == 0)
        {
          if (age >= 3)
          {
            gone.Add(u);
          }

          continue;
        }

        var life = GetUnitState(u, UNIT_STATE_LIFE);
        if (life > 3f)
        {
          SetUnitState(u, UNIT_STATE_LIFE, life - 2f);
        }
      }

      for (var i = 0; i < gone.Count; i++)
      {
        _infected.Remove(gone[i]);
        _age.Remove(gone[i]);
      }
    }
  }
}
