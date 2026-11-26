using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Rules;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw
{
  // 0.80.0 (maintainer): the Dragonmaw Clan comes in two ways -- both start at Dragonmaw Port (the shared path of
  // TwilightHammerFallback.Path: one of the two, once):
  // 1. the Orcish Horde's / Fel Horde's fallback when the conqueror of Orgrimmar / Kil'sorrow Fortress is NOT on the
  //    Old Gods' team (an Old God's conqueror -> Twilight, as before). Conquerors: Conquerors (death / capture).
  // 2. Twilight's successor: Cho'gall dead (not revived) while Grim Batol is not Twilight's -- checked every 2 s
  //    (WatchTwilight). Cho'gall stays revivable until then; after it he is gone for good (the Dragonmaw have no
  //    altar slot for him). The cult's own units (heralds, death knights, ogres, dragonspawn, void riders, nether
  //    dragons, grenadiers, enforcers, battlemasters) turn Neutral Hostile, the heroes leave, buildings / workers /
  //    catapults become the clan's (WL's categories); Grim Batol's holder becomes the overlord.
  // A vassal (Core/Conquest.cs): sworn to whoever holds Dragonmaw Port (its seat; at the start: the conqueror), never
  // defeated, its Great Hall / Stronghold / Fortress indestructible while it owns them.
  internal static class DragonmawFallback
  {
    private static readonly int[] HallTypes =
    {
      DragonmawObjects.GreatHall, DragonmawObjects.Stronghold, DragonmawObjects.Fortress
    };

    // the port's neutral creep buildings -> the clan's (the same buildings Twilight takes, TwilightHammerFallback)
    internal static readonly int[] PortPairs =
    {
      UNIT_O067_DUNGEON_CREEP_BARRACKS, DragonmawObjects.Barracks,
      UNIT_O068_QUARRY_CREEP_RESEARCH, DragonmawObjects.WarMill,
      UNIT_O06A_IMPROVED_TERRITORIAL_DRAKE_CREEP_TOWER_2, DragonmawObjects.ImprovedWatchTower,
      UNIT_O06B_TERRITORIAL_DRAKE_CREEP_TOWER, DragonmawObjects.WatchTower,
      UNIT_O06C_NEST_CREEP_FARM, DragonmawObjects.Burrow,
      UNIT_O06D_SHIPYARD_CREEP_SHIPYARD, DragonmawObjects.Dock
    };

    private static readonly int[] CultTypes =
    {
      TwilightHammerObjects.Herald, TwilightHammerObjects.DeathKnight, TwilightHammerObjects.OgreMagi,
      TwilightHammerObjects.Dragonspawn, TwilightHammerObjects.VoidRider, TwilightHammerObjects.NetherDragon,
      TwilightHammerObjects.PhaseGrenadier, TwilightHammerObjects.Enforcer, TwilightHammerObjects.Battlemaster
    };

    private static readonly int[] ArmyTypes =
    {
      DragonmawObjects.Warrior, DragonmawObjects.Spearthrower, DragonmawObjects.DragonRider,
      DragonmawObjects.DragonWhelp, DragonmawObjects.SkullCatapult
    };

    private static readonly int[] ArmyCounts = { 14, 10, 6, 8, 2 };

    internal const string FromHordeText =
      "|cffffcc00The Dragonmaw Clan rises.|r No Old God claimed the Horde's survivors: Nek'rosh Skullcrusher gathers the "
      + "clan and its red dragons at Dragonmaw Port -- sworn to whoever holds the port.";

    internal const string FromTwilightText =
      "|cffffcc00Cho'gall is dead and Grim Batol is lost.|r The whispers fall silent. Free of the Old Gods' grip, the "
      + "Dragonmaw throw off the Twilight's Hammer -- its fanatics turn on them -- and Nek'rosh Skullcrusher leads the "
      + "clan, sworn to whoever holds Dragonmaw Port.";

    // ---- 1. from the Orcish Horde / the Fel Horde ---------------------------------------------------------------
    internal static void FromHorde(player p, player conqueror)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var clan = new DragonmawFaction();
      SwapFaction.Run(p, clan);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      StartArea.Take(p, TwilightHammerFallback.Port, clan, PortPairs);
      StartCps.Give(p, UNIT_N016_WETLANDS);
      CreateUnit(p, DragonmawObjects.GreatHall, 22464f, -3264f, 270f);
      CreateUnit(p, DragonmawObjects.AltarOfStorms, 22016f, -3584f, 270f);
      var worker = DragonmawObjects.Peon;
      Spawn.Units(p, TwilightHammerFallback.StartX, TwilightHammerFallback.StartY,
        new[] { worker, worker, worker, worker, worker, DragonmawObjects.Nekrosh });
      Spawn.Units(p, TwilightHammerFallback.StartX + 176f, TwilightHammerFallback.StartY - 304f, StartArmy());
      Establish(p, conqueror);
      Announce(FromHordeText);
      p.RepositionCamera(new Point(TwilightHammerFallback.StartX, TwilightHammerFallback.StartY));
    }

    private static int[] StartArmy()
    {
      var list = new List<int>();
      for (var k = 0; k < ArmyTypes.Length; k++)
      {
        for (var i = 0; i < ArmyCounts[k]; i++)
        {
          list.Add(ArmyTypes[k]);
        }
      }

      return list.ToArray();
    }

    // the port is theirs, they serve its conqueror's team (or their own), their halls stand
    private static void Establish(player p, player overlord)
    {
      StartCps.Force(p, UNIT_N08T_DRAGONMAW_PORT);
      Conquest.Register(p, UNIT_N08T_DRAGONMAW_PORT, overlord != null && GetPlayerId(overlord) < 24 ? overlord : null,
        true);
      ProtectHalls(p);
    }

    private static void Announce(string text)
    {
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, text);
      }

      PingMinimap(TwilightHammerFallback.StartX, TwilightHammerFallback.StartY, 5f);
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
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (IsHall(u))
        {
          SetUnitInvulnerable(u, true);
        }
      }

      DestroyGroup(g);
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

    // ---- 2. from Twilight -------------------------------------------------------------------------------------
    private static player _twilight;
    private static player _chogallKiller;
    private static bool _done;

    // TwilightHammerFallback.RunTwilight: watch the Twilight player from its arrival on
    internal static void WatchTwilight(player p)
    {
      if (_twilight != null)
      {
        return;
      }

      _twilight = p;
      var death = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(death, p, EVENT_PLAYER_UNIT_DEATH, null);
      TriggerAddAction(death, () =>
      {
        var killer = GetKillingUnit();
        if (GetUnitTypeId(GetTriggerUnit()) == TwilightHammerObjects.Chogall && killer != null
            && GetPlayerId(GetOwningPlayer(killer)) < 24)
        {
          _chogallKiller = GetOwningPlayer(killer);
        }
      });
      var t = CreateTimer();
      TimerStart(t, 2f, true, () => AiApi.Safe("twilight -> dragonmaw", Check));
    }

    private static void Check()
    {
      if (_done || _twilight.GetPlayerData().Faction?.Name != TwilightHammerFaction.FactionName)
      {
        return;
      }

      if (ChogallAlive(_twilight))
      {
        return;
      }

      var batol = AllPreplacedWidgets.Units.Get(UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT);
      var holder = batol != null && GetUnitTypeId(batol) != 0 && GetUnitState(batol, UNIT_STATE_LIFE) > 0.405f
        ? GetOwningPlayer(batol)
        : null;
      if (holder == _twilight)
      {
        return; // Grim Batol still stands for the cult: Cho'gall can be revived
      }

      _done = true;
      FromTwilight(_twilight, holder != null && GetPlayerId(holder) < 24 ? holder : _chogallKiller);
    }

    private static bool ChogallAlive(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      var alive = false;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!alive && GetUnitTypeId(u) == TwilightHammerObjects.Chogall && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          alive = true;
        }
      }

      DestroyGroup(g);
      return alive;
    }

    private static bool IsCult(int type)
    {
      for (var i = 0; i < CultTypes.Length; i++)
      {
        if (CultTypes[i] == type)
        {
          return true;
        }
      }

      return false;
    }

    private static void FromTwilight(player p, player overlord)
    {
      var glass = Hourglass.Get(GetPlayerId(p));
      var clan = new DragonmawFaction();
      SwapFaction.Run(p, clan);
      var all = new List<unit>();
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        all.Add(u);
      }

      DestroyGroup(g);
      var turned = 0;
      for (var i = 0; i < all.Count; i++)
      {
        u = all[i];
        if (u == glass || GetUnitTypeId(u) == 0)
        {
          continue;
        }

        if (IsUnitType(u, UNIT_TYPE_HERO))
        {
          RemoveUnit(u); // Cho'gall and his ascendants are gone for good
          continue;
        }

        if (IsCult(GetUnitTypeId(u)))
        {
          SetUnitOwner(u, Player(PLAYER_NEUTRAL_AGGRESSIVE), true); // the fanatics turn on the clan
          turned++;
          continue;
        }

        try
        {
          u.ReplaceWithFactionEquivalent(clan);
        }
        catch (System.Exception)
        {
          if (IsUnitType(u, UNIT_TYPE_STRUCTURE))
          {
            SetUnitOwner(u, Player(PLAYER_NEUTRAL_AGGRESSIVE), true); // the Grim Totem: a cult temple
          }
        }
      }

      CreateUnit(p, DragonmawObjects.Nekrosh, TwilightHammerFallback.StartX, TwilightHammerFallback.StartY, 270f);
      Establish(p, overlord);
      EliminationRule.GiveGrace(p, 2);
      if (AiApi.IsBot(p))
      {
        var bot = p;
        var t = CreateTimer();
        TimerStart(t, 3f, false, () =>
        {
          DestroyTimer(t);
          AiApi.Safe("arrival production", () => AiApi.GiveStarterProduction(bot));
        });
      }

      Announce(FromTwilightText);
      AiApi.LogPin("TURNING POINT P" + I2S(GetPlayerId(p)) + " Twilight -> Dragonmaw (" + I2S(turned)
                   + " cult units turned hostile)");
    }
  }

  // who took Orgrimmar / Kil'sorrow Fortress (decides Twilight or Dragonmaw): the killer of the HQ, or its new owner
  internal static class Conquerors
  {
    private static readonly Dictionary<string, player> _by = new();

    internal static player Of(string factionName) => _by.TryGetValue(factionName, out var p) ? p : null;

    internal static void Watch(unit hq, string factionName)
    {
      if (hq == null)
      {
        return;
      }

      var death = CreateTrigger();
      TriggerRegisterUnitEvent(death, hq, EVENT_UNIT_DEATH);
      TriggerAddAction(death, () =>
      {
        var killer = GetKillingUnit();
        if (!_by.ContainsKey(factionName) && killer != null && GetPlayerId(GetOwningPlayer(killer)) < 24
            && GetOwningPlayer(hq).GetPlayerData().Faction?.Name == factionName)
        {
          _by[factionName] = GetOwningPlayer(killer);
        }
      });
      var capture = CreateTrigger();
      TriggerRegisterUnitEvent(capture, hq, EVENT_UNIT_CHANGE_OWNER);
      TriggerAddAction(capture, () =>
      {
        var owner = GetOwningPlayer(hq);
        if (!_by.ContainsKey(factionName) && GetPlayerId(owner) < 24
            && GetChangingUnitPrevOwner().GetPlayerData().Faction?.Name == factionName)
        {
          _by[factionName] = owner;
        }
      });
    }

    // an Old God's conqueror: on the Old Gods' team
    internal static bool ByOldGods(string factionName)
    {
      var c = Of(factionName);
      return c != null && c.GetPlayerData().Team == TeamSetup.OldGods;
    }
  }
}
