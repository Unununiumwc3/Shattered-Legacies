using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Zandalari
{
  // 0.80.0 (maintainer): Ahn'Qiraj -> the Zandalari Empire at Zuldazar. Trigger: C'Thun lost (the catalog's Hq).
  // - map setup: the Zandalari base preplaced at Zuldazar (WL's creep trolls, Neutral Passive) waits: buildings visible
  //   and invulnerable, units hidden (as Kezan's island); C'Thun's killer is recorded (the first overlord)
  // - run: Ahn'Qiraj's units are left behind, the base becomes the Empire's (MainEquivalents), Zuldazar's CP is theirs
  //   whoever holds it, Nazmir's if it is neutral; Rastakhan, 5 Gatherers and an army; a VASSAL of whoever holds
  //   Zuldazar (at the start: C'Thun's killer), never defeated, its halls and Dazar'alor indestructible.
  internal static class ZandalariFallback
  {
    internal const float StartX = -3776f;
    internal const float StartY = -15104f;
    private static readonly Rectangle Base = new(-6400f, -17920f, -2400f, -14656f);
    internal static readonly ExclusivePath Path = new("Zandalari Empire", Prepare, Run, StartX, StartY);

    private static readonly int[] HallTypes =
    {
      ZandalariObjects.GreatHall, ZandalariObjects.Stronghold, ZandalariObjects.Fortress, UNIT_O00V_DAZAR_ALOR_CREEP
    };

    private static readonly int[] ArmyTypes =
    {
      ZandalariObjects.Watcher, ZandalariObjects.Scout, ZandalariObjects.RaptorRider, ZandalariObjects.HexDoctor,
      ZandalariObjects.Haruspex
    };

    private static readonly int[] ArmyCounts = { 12, 10, 8, 4, 4 };
    private static List<unit> _base = new();
    private static player _cthunKiller;

    private static void Prepare()
    {
      _base = StartArea.Prepare(Base, RescuePreparationMode.HideNonStructures);
      var death = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(death, Player(i), EVENT_PLAYER_UNIT_DEATH, null);
      }

      TriggerAddAction(death, () =>
      {
        var killer = GetKillingUnit();
        if (GetUnitTypeId(GetTriggerUnit()) == UNIT_U00R_OLD_GOD_CTHUN && killer != null
            && GetPlayerId(GetOwningPlayer(killer)) < 24 && GetOwningPlayer(killer) != GetOwningPlayer(GetTriggerUnit()))
        {
          _cthunKiller = GetOwningPlayer(killer);
        }
      });
    }

    internal const string FromHordeText =
      "|cffffcc00The Horde is broken.|r King Rastakhan never bowed to Thrall's \"new\" Horde. Now he swears the "
      + "Zandalari Empire to those who destroyed it -- and calls every troll tribe to Zuldazar.";

    internal const string FromAhnqirajText =
      "|cffffcc00C'Thun has fallen.|r From Zuldazar King Rastakhan raises the Zandalari Empire -- it will serve "
      + "whoever promises the trolls a united empire.";

    private static void Run(player p, TurningPointDef def) => Arrive(p, _cthunKiller, FromAhnqirajText);

    // 0.81.0 (maintainer): the Orcish Horde's fallback too (not beaten by an Old God): first sworn to its conqueror
    internal static void FromHorde(player p, player conqueror) => Arrive(p, conqueror, FromHordeText);

    private static void Arrive(player p, player overlord, string text)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var empire = new ZandalariFaction();
      SwapFaction.Run(p, empire);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      StartArea.Take(p, _base, empire, ZandalariObjects.MainEquivalents);
      StartCps.Force(p, UNIT_N050_ZULDAZAR);
      StartCps.Give(p, UNIT_N0A8_NAZMIR);
      var g = ZandalariObjects.Gatherer;
      Spawn.Units(p, StartX, StartY, new[] { g, g, g, g, g, ZandalariObjects.Rastakhan });
      var army = new List<int>();
      for (var k = 0; k < ArmyTypes.Length; k++)
      {
        for (var i = 0; i < ArmyCounts[k]; i++)
        {
          army.Add(ArmyTypes[k]);
        }
      }

      Spawn.Units(p, StartX + 192f, StartY - 256f, army.ToArray());
      Conquest.Register(p, UNIT_N050_ZULDAZAR, overlord, true);
      ProtectHalls(p);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, text);
      }

      p.RepositionCamera(new Point(StartX, StartY));
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
