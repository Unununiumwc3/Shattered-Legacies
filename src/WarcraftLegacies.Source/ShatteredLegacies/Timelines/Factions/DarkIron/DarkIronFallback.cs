using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIronForces;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.DarkIron
{
  // 0.86.0 (maintainer): Ironforge -> the DARK IRON of Shadowforge (replaces the Explorers' League placeholder). Trigger:
  // the Great Forge lost (the catalog's Hq).
  // - run: Ironforge's units are left behind -- but what stands in Shadowforge City on the surface (WL's Dark Iron
  //   outpost, hidden until Ironforge's "The Dark Iron" quest) is theirs; the Shadowforge dungeon opens (its entrance
  //   is that city's building), its CPs are theirs (0.87.0: the seat only); a Dark Iron town inside
  //   the dungeon, 5 Masons, an army, Dagran Thaurissan.
  // - MERCENARY CHARTER: a VASSAL of whoever holds the Siege Foundry (at the start: whoever took or razed the Great
  //   Forge), never defeated, its halls indestructible. Every turn the charter holder (an ally, not the Dark Iron
  //   themselves) pays 50 gold tribute -- and gets a Dark Iron War Golem and a Dark Iron Mortar Team at the Foundry; the
  //   first tribute also brings master-forged weapons (one level of Copper-Forged Weaponry).
  // - "The Great Anvil" (DarkIronFaction): the arsenal -- a War Golem and a Mortar Team every turn at the Great Forge.
  // 0.87.0 (maintainer):
  // - the 24p's Dark Iron troops (DarkIronForces); the SEAT is the Throne Room (was the Siege Foundry), its throne the
  //   Dark Iron HQ (the Throne of Thaurissan, indestructible). Only the seat is theirs on arrival -- the Treasury and
  //   the Siege Foundry are guarded by Ragnaros' loyalists (dungeon_content.py), Majordomo Executus holds the west throne
  // - their CONTROLLER (whoever they serve: the seat's holder, at first the conqueror) gets a Dark Iron Gatebreaker
  //   every 5 turns at Shadowforge City (200 chaos damage, a very slow swing)
  // - the Mercenary Charter stays with the Siege Foundry: its holder pays tribute for a Mortar Team each turn (the
  //   first tribute: a weapons level); the Statue of Thaurissan beside it gives the Foundry's holder +4 armour around it
  // - the Shadowforge Black Market (neutral, expensive) sells to anyone who reaches it
  internal static class DarkIronFallback
  {
    internal const float StartX = -19200f; // Shadowforge, by the Throne Room
    internal const float StartY = 22900f;
    private const float SurfaceX = 11400f;  // Shadowforge City on the surface (the dungeon's world entrance)
    private const float SurfaceY = -9336f;
    private const int GatebreakerTurns = 5;
    private static player _controller;
    private static unit _throne;
    private static unit _statue;
    private const float FoundryX = -23040f; // the Siege Foundry CP (DungeonObjects.SiegeFoundry)
    private const float FoundryY = 23700f;
    private const int Tribute = 50;
    internal static readonly ExclusivePath Path = new("Dark Iron", Prepare, Run, StartX, StartY);
    private static readonly HashSet<player> _forged = new();

    private static readonly int[] HallTypes =
    {
      DarkIronObjects.MiningColonyT1, DarkIronObjects.DwarfHoldT2, DarkIronObjects.GreatHoldT3,
      UNIT_H01O_SHADOWFORGE_CITY_IRONFORGE_OTHER, DarkIronObjects.ThroneOfThaurissan
    };

    private static readonly int[] Town =
    {
      DarkIronObjects.MiningColonyT1, DarkIronObjects.AltarOfFortitudeAltar, DarkIronObjects.MusteringHallBarracks,
      DarkIronObjects.WorkshopSpecialist, DarkIronObjects.ForgeworksResearch, DarkIronObjects.TavernFarm,
      DarkIronObjects.TavernFarm
    };

    private static readonly int[] Army =
    {
      DarkIronObjects.MasonWorker, DarkIronObjects.MasonWorker, DarkIronObjects.MasonWorker, DarkIronObjects.MasonWorker,
      DarkIronObjects.MasonWorker, DarkIronForcesObjects.AnvilrageGuardsman,
      DarkIronForcesObjects.AnvilrageGuardsman, DarkIronForcesObjects.AnvilrageGuardsman,
      DarkIronForcesObjects.AnvilrageGuardsman, DarkIronForcesObjects.AnvilrageGuardsman,
      DarkIronForcesObjects.AnvilrageGuardsman, DarkIronForcesObjects.BearRider, DarkIronForcesObjects.BearRider,
      DarkIronObjects.Rifleman, DarkIronObjects.Rifleman, DarkIronObjects.Rifleman, DarkIronObjects.Rifleman,
      DarkIronObjects.Rifleman, DarkIronObjects.Rifleman, DarkIronForcesObjects.Shadowcaster,
      DarkIronForcesObjects.Shadowcaster, DarkIronForcesObjects.FlameKeeper, DarkIronObjects.MortarTeam,
      DarkIronObjects.MortarTeam, DarkIronObjects.WarGolem, DarkIronObjects.WarGolem
    };

    internal const string ArrivalText =
      "|cffffcc00The Bronzebeard throne is empty.|r A Dark Iron expeditionary force tunnels up through the shattered "
      + "foundations -- not to burn the world for Ragnaros, but to claim the Great Anvil. Gold, ore and survival are "
      + "all they serve: whoever holds their Throne Room is their master, whoever holds their Siege Foundry holds their "
      + "Mercenary Charter.";

    private static void Prepare()
    {
      LastConqueror.Watch(UNIT_H001_THE_GREAT_FORGE_IRONFORGE_OTHER, "Ironforge");
      // Shadowforge's preplaced props (dungeon_content.py): visible, invulnerable, neutral until the Dark Iron come
      _throne = Protect(DarkIronObjects.ThroneOfThaurissan);
      _statue = Protect(DarkIronObjects.StatueOfThaurissan);
      if (_statue != null)
      {
        // CreateUnit may displace a building from the unwalkable hill. Restore the editor anchor.
        SetUnitX(_statue, -22380.629f);
        SetUnitY(_statue, 23577.186f);
      }
      Protect(DarkIronObjects.ShadowforgeBlackMarket);
    }

    private static unit Protect(int type)
    {
      if (!AllPreplacedWidgets.Units.TryGetAll(type, out var all) || all.Count == 0)
      {
        return null;
      }

      SetUnitInvulnerable(all[0], true);
      return all[0];
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      KeepShadowforgeCity(p, keep);
      var darkIron = new DarkIronFaction();
      SwapFaction.Run(p, darkIron);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      TakeShadowforgeCity(p);
      VoidDungeons.Open(VoidDungeons.Shadowforge);
      StartCps.Force(p, DungeonObjects.ThroneRoom); // 0.87.0: the seat only -- the rest is guarded by creeps
      if (_throne != null)
      {
        SetUnitOwner(_throne, p, true);
      }

      Spawn.Buildings(p, StartX, StartY, Town);
      Spawn.Units(p, StartX + 256f, StartY - 256f, Army);
      AllLegends.Ironforge.Dagran.ForceCreate(p, new Point(StartX, StartY), 270f);
      var overlord = LastConqueror.Of("Ironforge");
      _controller = overlord != p ? overlord : null;
      Conquest.Register(p, DungeonObjects.ThroneRoom, _controller, true);
      ProtectHalls(p);
      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, () => AiApi.Safe("dark iron charter",
        () => Charter(p)), 1, () => p.GetPlayerData().Faction == darkIron);
      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + GatebreakerTurns, () =>
        AiApi.Safe("dark iron gatebreaker", () => Gatebreaker(p)), GatebreakerTurns,
        () => p.GetPlayerData().Faction == darkIron);
      if (_statue != null)
      {
        UnitAddAbility(_statue, DarkIronForcesObjects.WillOfTheEmperor);
        SetUnitAbilityLevel(_statue, DarkIronForcesObjects.WillOfTheEmperor, 4);
        var t = CreateTimer();
        TimerStart(t, 2f, true, () => AiApi.Safe("statue of thaurissan", StatueOwner));
      }
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(StartX, StartY));
      AiApi.LogPin("Ironforge fallback -> Dark Iron" + (overlord != null ? ", conqueror P" + I2S(GetPlayerId(overlord))
        : ", no conqueror"));
    }

    // what Ironforge already took in Shadowforge City (its "The Dark Iron" quest) stays with the player
    private static void KeepShadowforgeCity(player p, List<unit> keep)
    {
      foreach (var area in new[] { Regions.Shadowforge_City, Regions.Shadowforge_City_Base })
      {
        var g = CreateGroup();
        GroupEnumUnitsInRect(g, area.Rect, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (GetOwningPlayer(u) == p && !IsUnitType(u, UNIT_TYPE_HERO) && !keep.Contains(u))
          {
            keep.Add(u);
          }
        }

        DestroyGroup(g);
      }
    }

    // WL's Dark Iron outpost in Shadowforge City (Neutral Passive, hidden or not) is theirs
    private static void TakeShadowforgeCity(player p)
    {
      foreach (var area in new[] { Regions.Shadowforge_City, Regions.Shadowforge_City_Base })
      {
        var g = CreateGroup();
        GroupEnumUnitsInRect(g, area.Rect, null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_PASSIVE) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
              && GetUnitTypeId(u) != UNIT_N03B_DEEPRUN_TRAM)
          {
            u.Rescue(p);
          }
        }

        DestroyGroup(g);
      }
    }

    // the Statue of Thaurissan stands with whoever holds the Siege Foundry (its aura is the owner's)
    private static void StatueOwner()
    {
      var cp = AiApi.SafeCp(DungeonObjects.SiegeFoundry);
      var holder = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
      var owner = holder != null && GetPlayerId(holder) < 24 ? holder : Player(PLAYER_NEUTRAL_PASSIVE);
      if (GetOwningPlayer(_statue) != owner)
      {
        SetUnitOwner(_statue, owner, true);
      }
    }

    // every 5 turns: a Gatebreaker for the Dark Iron's controller -- the Throne Room's holder, else their conqueror
    private static void Gatebreaker(player p)
    {
      var cp = AiApi.SafeCp(DungeonObjects.ThroneRoom);
      var holder = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
      if (holder != null && holder != p && GetPlayerId(holder) < 24)
      {
        _controller = holder;
      }

      if (_controller == null || GetPlayerSlotState(_controller) != PLAYER_SLOT_STATE_PLAYING
          || !IsPlayerAlly(_controller, p))
      {
        return;
      }

      CreateUnit(_controller, DarkIronForcesObjects.Gatebreaker, SurfaceX, SurfaceY, 270f);
      DisplayTimedTextToPlayer(_controller, 0, 0, 15, "|cffffcc00Shadowforge pays its due:|r a Dark Iron Gatebreaker "
        + "waits for you at Shadowforge City.");
      if (GetLocalPlayer() == _controller)
      {
        PingMinimap(SurfaceX, SurfaceY, 4f);
      }
    }

    // the Mercenary Charter: the Siege Foundry's holder pays tribute and gets a mortar team (the first time: weapons)
    private static void Charter(player p)
    {
      var cp = AiApi.SafeCp(DungeonObjects.SiegeFoundry);
      if (cp == null || cp.Unit == null)
      {
        return;
      }

      var holder = GetOwningPlayer(cp.Unit);
      if (holder == p || GetPlayerId(holder) >= 24 || !IsPlayerAlly(holder, p)
          || GetPlayerState(holder, PLAYER_STATE_RESOURCE_GOLD) < Tribute)
      {
        return;
      }

      SetPlayerState(holder, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(holder, PLAYER_STATE_RESOURCE_GOLD) - Tribute);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + Tribute);
      CreateUnit(holder, DarkIronObjects.MortarTeam, FoundryX + 128f, FoundryY - 256f, 270f);
      if (_forged.Add(holder))
      {
        var weapons = UPGRADE_RHME_COPPER_FORGED_WEAPONRY_UNIVERSAL_UPGRADE;
        SetPlayerTechResearched(holder, weapons, GetPlayerTechCount(holder, weapons, true) + 1);
        DisplayTimedTextToPlayer(holder, 0, 0, 15, "|cffffcc00Mercenary Charter:|r the Dark Iron forge your weapons "
          + "and send a mortar team every turn -- for " + I2S(Tribute) + " gold.");
      }
    }

    // "The Great Anvil": the arsenal at the Great Forge, the forges' best work
    internal static void Arsenal(player p)
    {
      if (p == null)
      {
        return;
      }

      foreach (var tech in new[]
               {
                 UPGRADE_RHME_COPPER_FORGED_WEAPONRY_UNIVERSAL_UPGRADE,
                 UPGRADE_RHAR_COPPER_ARMOR_PLATING_UNIVERSAL_UPGRADE
               })
      {
        SetPlayerTechResearched(p, tech, GetPlayerTechCount(p, tech, true) + 1);
      }

      var forge = AllLegends.Ironforge.GreatForge.Unit;
      var fx = forge != null ? GetUnitX(forge) : 10624f;
      var fy = forge != null ? GetUnitY(forge) - 400f : -5000f;
      var faction = p.GetPlayerData().Faction;
      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, () =>
      {
        CreateUnit(p, DarkIronObjects.WarGolem, fx, fy, 270f);
        CreateUnit(p, DarkIronObjects.MortarTeam, fx + 128f, fy, 270f);
      }, 1, () => p.GetPlayerData().Faction == faction);
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
