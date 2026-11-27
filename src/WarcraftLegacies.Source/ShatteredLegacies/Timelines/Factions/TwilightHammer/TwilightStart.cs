using System.Collections.Generic;
using MacroTools.Cinematics;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer
{
  // Place the Highlands camp once faction assignment has finished. The fortress and
  // its workshops are awarded by The Cursed Fortress when the team captures its CP.
  internal static class TwilightStart
  {
    internal const int Slot = 19; // P20
    private const float CitadelX = 19015f;
    private const float CitadelY = -6411f;
    private const float GateX = 18048f; // the wooden gate of the pass (h00L)
    private const float GateY = -2176f;
    private static readonly int Watchtower = FourCC("B01W"); // the 24p's big watchtowers (scale 1.94 there)

    private static readonly (int Type, float X, float Y)[] Camp =
    {
      (TwilightHammerObjects.TwilightCitadel, CitadelX, CitadelY),
      (TwilightHammerObjects.GreatHall, 19136f, -6912f),
      (TwilightHammerObjects.AltarOfVisions, 19072f, -5952f),
      (TwilightHammerObjects.Burrow, 18688f, -6016f),
      (TwilightHammerObjects.Burrow, 18816f, -6784f),
      (TwilightHammerObjects.Burrow, 19456f, -6784f),
      (TwilightHammerObjects.Burrow, 19456f, -6144f),
      (TwilightHammerObjects.Lookout, 18688f, -5824f),
      (TwilightHammerObjects.Lookout, 19584f, -7040f),
      (TwilightHammerObjects.Lookout, 19392f, -5952f),
    };

    internal static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("twilight start", Start);
      });
    }

    private static void Start()
    {
      var p = Player(Slot);
      if (GetPlayerSlotState(p) != PLAYER_SLOT_STATE_PLAYING
          || p.GetPlayerData().Faction is not TwilightHammerFaction)
      {
        return;
      }

      TwilightHammerFallback.Prepare(); // the port and the Highlands buildings (also the turning point's preparation)
      ClearCreeps(CitadelX, CitadelY, 900f);
      StartCps.Give(p, UNIT_N09F_SOUTHERN_HIGHLANDS);
      for (var i = 0; i < Camp.Length; i++)
      {
        Place(p, Camp[i].Type, Camp[i].X, Camp[i].Y);
      }

      Place(p, TwilightHammerObjects.Chogall, 19584f, -6848f);
      for (var i = 0; i < 5; i++)
      {
        Place(p, TwilightHammerObjects.Cultist, 19648f + 64f * (i % 2), -6784f - 64f * (i / 2));
      }
      Gate(p);
      AiApi.LogPin("Twilight's Hammer starts (P20)");
    }

    private static void Place(player p, int type, float x, float y)
    {
      CinematicMode.AddPausedUnit(CreateUnit(p, type, x, y, 270f));
    }

    // the neutral creeps on the camp's ground (the ogres of the Southern Highlands) make room
    private static void ClearCreeps(float x, float y, float radius)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, radius, null);
      var gone = new List<unit>();
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_AGGRESSIVE) && !IsUnitType(u, UNIT_TYPE_STRUCTURE))
        {
          gone.Add(u);
        }
      }

      DestroyGroup(g);
      for (var i = 0; i < gone.Count; i++)
      {
        RemoveUnit(gone[i]);
      }
    }

    // the wooden gate of the pass -> the big Elven Gate (open), Twilight's; two watchtowers beside it
    private static void Gate(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, GateX, GateY, 160f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if (t == UNIT_H00L_HORIZONTAL_WOODEN_GATE_GATE_OPEN || t == UNIT_H00K_HORIZONTAL_WOODEN_GATE_GATE_CLOSED)
        {
          RemoveUnit(u);
        }
      }

      DestroyGroup(g);
      Place(p, UNIT_H01X_ELVEN_GATE_GATE_OPEN, GateX, GateY);
      // on the cliffs either side of the pass (map_query: the pass is open from about 17756 to 18332 here)
      CreateDestructable(Watchtower, GateX - 440f, GateY + 20f, 270f, 1.9f, 0);
      CreateDestructable(Watchtower, GateX + 390f, GateY + 20f, 270f, 1.9f, 0);
    }
  }
}
