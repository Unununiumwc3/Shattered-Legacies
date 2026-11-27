using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer
{
  // 0.89.0 (maintainer): TWILIGHT'S HAMMER IS A START FACTION -- player slot P20 (player id 19: one of the fork's three
  // free NPC slots, the one Twilight had in the 24p; splits keep P11 / P14), on the Old Gods' team. One second into the
  // game, if the slot plays (an empty slot is defeated by WL at 2 s):
  // - Grim Batol: its capital, the cult's buildings around it (TwilightHammerFallback.ClaimHighlands) and its CP;
  // - the Highlands camp of the 24p at the Southern Highlands CP: the Twilight Citadel (24p h05U: Cultists and
  //   Battlemasters), a Great Hall, the Altar of Visions, Burrows and Lookouts (spots checked with map_query -- the 24p's
  //   own Citadel spot is a slope here); the ogres guarding the CP leave;
  // - Cho'gall, 5 Cultists and a small army;
  // - the pass north of Grim Batol is THEIR GATE: the wooden gate becomes the big Elven Gate (WL's gate unit, its owner
  //   opens and closes it) between two watchtowers.
  // Dragonmaw Port's base is not theirs: it waits, hidden and invulnerable, for the Dragonmaw (their fallback); the
  // port's CP is free to take ("The Old Pact").
  internal static class TwilightStart
  {
    internal const int Slot = 19; // P20
    private const float CitadelX = 19768f;
    private const float CitadelY = -4920f;
    private const float GateX = 18048f; // the wooden gate of the pass (h00L)
    private const float GateY = -2176f;
    private static readonly int Watchtower = FourCC("B01W"); // the 24p's big watchtowers (scale 1.94 there)

    private static readonly (int Type, float X, float Y)[] Camp =
    {
      (TwilightHammerObjects.TwilightCitadel, CitadelX, CitadelY),
      (TwilightHammerObjects.GreatHall, 20088f, -5888f),
      (TwilightHammerObjects.AltarOfVisions, 20472f, -5504f),
      (TwilightHammerObjects.Burrow, 20352f, -5248f),
      (TwilightHammerObjects.Burrow, 20224f, -5568f),
      (TwilightHammerObjects.Burrow, 19776f, -6016f),
      (TwilightHammerObjects.Burrow, 20160f, -4864f),
      (TwilightHammerObjects.Lookout, 19712f, -5824f),
      (TwilightHammerObjects.Lookout, 20096f, -5056f),
      (TwilightHammerObjects.Lookout, 20096f, -5504f),
    };

    private static readonly int[] Army =
    {
      TwilightHammerObjects.Enforcer, TwilightHammerObjects.Enforcer, TwilightHammerObjects.Enforcer,
      TwilightHammerObjects.Enforcer, TwilightHammerObjects.Enforcer, TwilightHammerObjects.Enforcer,
      TwilightHammerObjects.PhaseGrenadier, TwilightHammerObjects.PhaseGrenadier, TwilightHammerObjects.PhaseGrenadier,
      TwilightHammerObjects.PhaseGrenadier, TwilightHammerObjects.OgreMagi, TwilightHammerObjects.OgreMagi,
      TwilightHammerObjects.Herald, TwilightHammerObjects.Herald
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
          || p.GetPlayerData().Faction is not TwilightHammerFaction faction)
      {
        return;
      }

      TwilightHammerFallback.Prepare(); // the port and the Highlands buildings (also the turning point's preparation)
      ClearCreeps(CitadelX, CitadelY, 900f);
      var capital = AllLegends.Neutral.GrimBatol.Unit;
      if (capital != null && GetUnitTypeId(capital) != 0)
      {
        SetUnitOwner(capital, p, true);
        SetUnitInvulnerable(capital, false);
      }

      TwilightHammerFallback.ClaimHighlands(p, faction);
      StartCps.Give(p, UNIT_N03X_GRIM_BATOL, UNIT_N09F_SOUTHERN_HIGHLANDS);
      for (var i = 0; i < Camp.Length; i++)
      {
        CreateUnit(p, Camp[i].Type, Camp[i].X, Camp[i].Y, 270f);
      }

      var worker = TwilightHammerObjects.Cultist;
      Spawn.Units(p, 19950f, -5300f, new[] { TwilightHammerObjects.Chogall, worker, worker, worker, worker, worker });
      Spawn.Units(p, 19600f, -5450f, Army);
      Gate(p);
      AiApi.LogPin("Twilight's Hammer starts (P20)");
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
      CreateUnit(p, UNIT_H01X_ELVEN_GATE_GATE_OPEN, GateX, GateY, 270f);
      // on the cliffs either side of the pass (map_query: the pass is open from about 17756 to 18332 here)
      CreateDestructable(Watchtower, GateX - 440f, GateY + 20f, 270f, 1.9f, 0);
      CreateDestructable(Watchtower, GateX + 390f, GateY + 20f, 270f, 1.9f, 0);
    }
  }
}
