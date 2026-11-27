using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Draenei
{
  // 0.87.0 (maintainer): THE EXODAR FLIES. The draenei's base is the inside of the ship now (moved into the western
  // void, tools/shattered-legacies/terrain/exodar_move.py; Dungeons/VoidDungeons.Exodar); the only way in or out is the
  // ship (DungeonObjects.ExodarShip, a waygate) at one of the fixed LANDING SPOTS below.
  // - arrival (DraeneiFallback): the ship lands at the spot nearest the home of whoever beat the Legion
  // - "Set Course" (DungeonObjects.SetCourse, on WL's Dimensional Generator inside; needs "The Dimensional Ship" -- the
  //   rewritten WL quest): aimed at any point, the ship flies to the landing spot nearest to it. Only while the
  //   Exodar's bridge (its CP) is held by the draenei or one of their allies -- reconquer the bridge to fly. Bots never
  //   cast it (it is in no bot spell list): the ship of a bot stays where it landed.
  // - in flight (FlightTime): both gates are shut, nobody gets in or out; the ship crosses the sky as an effect.
  // Bot routes follow by themselves: AiRoutes.CheckPortals reads the live waygates every 30 s.
  internal static class ExodarShip
  {
    private const float FlightTime = 90f;
    private const float Step = 0.1f;
    private const string Model = @"war3mapImported\Unit_Exodar.mdl";

    // name, x, y (open ground, 450 .. 1300 from a CP, nobody's base; the inner gate puts you out 280 south)
    private static readonly string[] Names =
    {
      "Azuremyst Isle", "Darkshore", "Ashenvale", "Durotar", "Dustwallow Marsh", "Feralas", "Tanaris", "Silithus",
      "Westfall", "the Wetlands", "Tirisfal Glades", "the Hinterlands", "Eversong Woods", "the Borean Tundra",
      "the Howling Fjord", "Hellfire Peninsula"
    };

    private static readonly float[] X =
    {
      -21314f, -17412f, -10110f, -8830f, -8709f, -19392f, -10624f, -17633f, 7585f, 14530f, 6559f, 15833f, 18023f,
      -7455f, 6630f, 2016f
    };

    private static readonly float[] Y =
    {
      10240f, 7301f, 2240f, -3328f, -7218f, -6206f, -14910f, -13894f, -15847f, -2240f, 10502f, 4506f, 16225f, 15335f,
      15207f, -26240f
    };

    private static int _at = -1;
    private static bool _flying;
    private static trigger _cast;

    internal static bool Flying => _flying;

    // the first landing: the spot nearest (x, y)
    internal static void LandNear(float x, float y)
    {
      var spot = Nearest(x, y, -1);
      var exodar = VoidDungeons.Exodar;
      if (exodar == null)
      {
        return;
      }

      exodar.PlaceEntrance(DungeonObjects.ExodarShip, X[spot], Y[spot], X[spot], Y[spot] - 280f);
      exodar.Open(Player(PLAYER_NEUTRAL_PASSIVE));
      _at = spot;
      Listen();
      AiApi.LogPin("Exodar landed at " + Names[spot]);
    }

    // the landing spot nearest a player's home (his start location)
    internal static void LandNear(player p)
    {
      if (p == null || GetPlayerId(p) >= 24)
      {
        LandNear(X[0], Y[0]);
        return;
      }

      var start = GetPlayerStartLocation(p);
      LandNear(GetStartLocationX(start), GetStartLocationY(start));
    }

    private static int Nearest(float x, float y, int except)
    {
      var best = 0;
      var bestD = -1f;
      for (var i = 0; i < X.Length; i++)
      {
        if (i == except)
        {
          continue;
        }

        var dx = X[i] - x;
        var dy = Y[i] - y;
        var d = dx * dx + dy * dy;
        if (bestD < 0f || d < bestD)
        {
          best = i;
          bestD = d;
        }
      }

      return best;
    }

    private static void Listen()
    {
      if (_cast != null)
      {
        return;
      }

      _cast = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_cast, Player(i), EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      }

      TriggerAddAction(_cast, () =>
      {
        if (GetSpellAbilityId() != DungeonObjects.SetCourse)
        {
          return;
        }

        var caster = GetTriggerUnit();
        var tx = GetSpellTargetX();
        var ty = GetSpellTargetY();
        AiApi.Safe("exodar set course", () => SetCourse(caster, tx, ty));
      });
    }

    private static void SetCourse(unit caster, float tx, float ty)
    {
      var p = GetOwningPlayer(caster);
      if (_flying || _at < 0)
      {
        DisplayTimedTextToPlayer(p, 0, 0, 8, "|cffffcc00The Exodar|r is already under way.");
        return;
      }

      var cp = AiApi.SafeCp(UNIT_N0BL_EXODAR_REGALIS);
      var bridge = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
      if (bridge == null || (bridge != p && !IsPlayerAlly(bridge, p)))
      {
        DisplayTimedTextToPlayer(p, 0, 0, 10, "|cffffcc00The Exodar's bridge is held by an enemy.|r Retake it to fly.");
        return;
      }

      var to = Nearest(tx, ty, _at);
      Fly(_at, to);
    }

    private static void Fly(int from, int to)
    {
      var exodar = VoidDungeons.Exodar;
      var ship = exodar?.Entrance;
      if (ship == null)
      {
        return;
      }

      _flying = true;
      exodar.Close();
      ShowUnit(ship, false);
      Announce("|cffffcc00The Exodar lifts off|r from " + Names[from] + " -- bound for " + Names[to] + ". Its gates "
               + "are shut until it lands.");
      var fx = AddSpecialEffect(Model, X[from], Y[from]);
      BlzSetSpecialEffectScale(fx, 0.32f);
      BlzSetSpecialEffectHeight(fx, 400f);
      var steps = (int)(FlightTime / Step);
      var n = 0;
      var t = CreateTimer();
      TimerStart(t, Step, true, () =>
      {
        n++;
        var k = (float)n / steps;
        BlzSetSpecialEffectPosition(fx, X[from] + (X[to] - X[from]) * k, Y[from] + (Y[to] - Y[from]) * k, 400f);
        if (n < steps)
        {
          return;
        }

        DestroyTimer(t);
        DestroyEffect(fx);
        Land(to);
      });
    }

    private static void Land(int to)
    {
      var exodar = VoidDungeons.Exodar;
      var ship = exodar?.Entrance;
      _flying = false;
      _at = to;
      if (ship == null)
      {
        return;
      }

      ShowUnit(ship, true);
      exodar.MoveEntrance(X[to], Y[to], X[to], Y[to] - 280f);
      Announce("|cffffcc00The Exodar has landed|r at " + Names[to] + ".");
      PingMinimap(X[to], Y[to], 5f);
      AiApi.LogPin("Exodar landed at " + Names[to]);
    }

    private static void Announce(string text)
    {
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 15, text);
      }
    }
  }
}
