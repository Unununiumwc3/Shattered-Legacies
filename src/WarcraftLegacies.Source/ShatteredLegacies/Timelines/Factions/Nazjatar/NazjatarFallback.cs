using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Nazjatar
{
  // 0.84.0 (maintainer): Sentinels -> the NAGA OF AZSHARA in the Nazjatar dungeon (Dungeons/VoidDungeons). Trigger:
  // Auberdine and Feathermoon Stronghold lost (the catalog's Hq). Replaces Maiev's "Chase Illidan to Outland" as the
  // Sentinels' fallback (Maiev's return becomes a sub-path later, TODO.md).
  // - map setup (+1 s): the naga's base stands in Nazjatar where the 24p map preplaced it -- Neutral Extra, visible and
  //   invulnerable; the dungeon's gates stay shut. Whoever destroys or takes a Sentinels HQ last is recorded.
  // - run: the Sentinels' units are left behind, the base is theirs, the gates open, the Eternal Palace is theirs
  //   (0.87.0: the other three CPs are guarded by creeps)
  //   (the Eternal Palace whoever holds it), Lady Naz'jar, 5 Mur'gul Slaves and an army; a VASSAL of whoever holds the
  //   Eternal Palace (at the start: the Sentinels' conqueror), never defeated, its temples indestructible.
  internal static class NazjatarFallback
  {
    internal const float StartX = -21500f;
    internal const float StartY = -29200f;
    internal static readonly ExclusivePath Path = new("Naga of Azshara", Prepare, Run, StartX, StartY);

    private static readonly int[] HallTypes =
    {
      NazjatarObjects.TempleOfAzshara, NazjatarObjects.PillarOfWaves, NazjatarObjects.MonumentOfCurrents,
      NazjatarObjects.TempleOfTides
    };

    // the 24p map's naga base in Nazjatar (its spots moved with the dungeon: x - 31104, y + 512)
    private static readonly int[] BaseTypes =
    {
      NazjatarObjects.TempleOfAzshara, NazjatarObjects.PillarOfWaves, NazjatarObjects.AltarOfTheDepths,
      NazjatarObjects.ShrineOfAzshara, NazjatarObjects.RoyalPyramid, NazjatarObjects.DepthVoidPortal,
      NazjatarObjects.DeepSeaVault, NazjatarObjects.FirstGuardian,
      NazjatarObjects.SpawningGrounds, NazjatarObjects.SpawningGrounds,
      NazjatarObjects.CoralBed, NazjatarObjects.CoralBed, NazjatarObjects.CoralBed, NazjatarObjects.CoralBed,
      NazjatarObjects.CoralBed, NazjatarObjects.CoralBed, NazjatarObjects.CoralBed, NazjatarObjects.CoralBed,
      NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian,
      NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian,
      NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian,
      NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian, NazjatarObjects.TidalGuardian,
    };

    private static readonly float[] BaseX =
    {
      -22528f, -19008f, -20320f, -21088f, -22752f, -21856f, -21888f, -21812f,
      -22688f, -21344f,
      -22912f, -22720f, -22592f, -22336f, -22080f, -21120f, -21056f, -20288f,
      -22848f, -22656f, -22208f, -22080f, -21696f, -21696f,
      -21440f, -21440f, -20992f, -20736f, -20672f, -22784f,
    };

    private static readonly float[] BaseY =
    {
      -30272f, -30528f, -29600f, -29600f, -28064f, -28064f, -28928f, -29537f,
      -27040f, -27168f,
      -28800f, -27584f, -27776f, -27456f, -27008f, -30720f, -27200f, -30720f,
      -30080f, -29504f, -30528f, -29504f, -30464f, -29888f,
      -30592f, -27584f, -28544f, -28800f, -27712f, -29184f,
    };

    private static readonly int[] ArmyTypes =
    {
      NazjatarObjects.NagaIncursor, NazjatarObjects.Deepseer, NazjatarObjects.Bloodkin, NazjatarObjects.NagaSeaWitch
    };

    private static readonly int[] ArmyCounts = { 12, 4, 4, 2 };
    private static readonly List<unit> _base = new();
    private static player _conqueror;

    internal const string ArrivalText =
      "|cffffcc00The Sentinels are broken.|r For ten thousand years Queen Azshara waited beneath the Maelstrom for "
      + "Tyrande's kin to fall. Now the gates of Nazjatar open -- and the naga swear themselves to those who broke "
      + "the Sentinels, for whoever holds the Eternal Palace holds the Empress's favour.";

    private static void Prepare()
    {
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        var waiting = Player(26); // Neutral Extra: WL cleans up Neutral Passive units in its regions (README)
        for (var i = 0; i < BaseTypes.Length; i++)
        {
          var u = CreateUnit(waiting, BaseTypes[i], BaseX[i], BaseY[i], 270f);
          SetUnitInvulnerable(u, true);
          _base.Add(u);
        }
      });
      Watch(UNIT_E00J_AUBERDINE_SENTINELS_OTHER);
      Watch(UNIT_E00M_FEATHERMOON_STRONGHOLD_SENTINELS_OTHER);
    }

    // the LAST player to destroy or take a Sentinels HQ is the conqueror
    private static void Watch(int hqType)
    {
      if (!AllPreplacedWidgets.Units.TryGetAll(hqType, out var hqs))
      {
        return;
      }

      for (var i = 0; i < hqs.Count; i++)
      {
        var hq = hqs[i];
        var death = CreateTrigger();
        TriggerRegisterUnitEvent(death, hq, EVENT_UNIT_DEATH);
        TriggerAddAction(death, () =>
        {
          var killer = GetKillingUnit();
          if (killer != null && GetPlayerId(GetOwningPlayer(killer)) < 24
              && GetOwningPlayer(hq).GetPlayerData().Faction?.Name == "Sentinels")
          {
            _conqueror = GetOwningPlayer(killer);
          }
        });
        var capture = CreateTrigger();
        TriggerRegisterUnitEvent(capture, hq, EVENT_UNIT_CHANGE_OWNER);
        TriggerAddAction(capture, () =>
        {
          var owner = GetOwningPlayer(hq);
          if (GetPlayerId(owner) < 24 && GetChangingUnitPrevOwner().GetPlayerData().Faction?.Name == "Sentinels")
          {
            _conqueror = owner;
          }
        });
      }
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var naga = new NazjatarFaction();
      SwapFaction.Run(p, naga);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      for (var i = 0; i < _base.Count; i++)
      {
        var u = _base[i];
        if (GetUnitTypeId(u) != 0)
        {
          SetUnitOwner(u, p, true);
          SetUnitInvulnerable(u, false);
        }
      }

      VoidDungeons.Open(VoidDungeons.Nazjatar);
      StartCps.Force(p, DungeonObjects.EternalPalace); // 0.87.0: the seat only -- the other three are guarded by creeps
      var s = NazjatarObjects.MurgulSlave;
      Spawn.Units(p, -19200f, -30100f, new[] { s, s, s, s, s, NazjatarObjects.LadyNazjar });
      var army = new List<int>();
      for (var k = 0; k < ArmyTypes.Length; k++)
      {
        for (var i = 0; i < ArmyCounts[k]; i++)
        {
          army.Add(ArmyTypes[k]);
        }
      }

      Spawn.Units(p, StartX, StartY, army.ToArray());
      var overlord = _conqueror != null && _conqueror != p ? _conqueror : null;
      Conquest.Register(p, DungeonObjects.EternalPalace, overlord, true);
      ProtectHalls(p);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(StartX, StartY));
      AiApi.LogPin("Sentinels fallback -> Nazjatar" + (overlord != null ? ", conqueror P" + I2S(GetPlayerId(overlord))
        : ", no conqueror"));
    }

    private static bool IsHall(unit u)
    {
      var t = GetUnitTypeId(u);
      for (var i = 0; i < HallTypes.Length; i++)
      {
        if (HallTypes[i] == t)
        {
          return true;
        }
      }

      return false;
    }

    private static void ProtectHalls(player p)
    {
      var grp = CreateGroup();
      GroupEnumUnitsOfPlayer(grp, p, null);
      unit u;
      while ((u = FirstOfGroup(grp)) != null)
      {
        GroupRemoveUnit(grp, u);
        if (IsHall(u))
        {
          SetUnitInvulnerable(u, true);
        }
      }

      DestroyGroup(grp);
      var t = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_CONSTRUCT_FINISH, null);
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_UPGRADE_FINISH, null);
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_CHANGE_OWNER, null);
      TriggerAddAction(t, () =>
      {
        var unit = GetTriggerEventId() == EVENT_PLAYER_UNIT_CONSTRUCT_FINISH ? GetConstructedStructure() : GetTriggerUnit();
        if (unit != null && GetOwningPlayer(unit) == p && IsHall(unit))
        {
          SetUnitInvulnerable(unit, true);
        }
      });
    }
  }
}
