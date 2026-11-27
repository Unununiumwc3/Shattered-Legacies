using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.GameTime;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Furbolg
{
  // 0.86.0 (maintainer): Druids -> the FURBOLGS. Trigger: Nordrassil and Cenarion Hold lost (the catalog's Hq).
  // Lore: the furbolg tribes follow whoever proves worthy of holding their Sacred Ancestral Totem (the Claw of Ursoc).
  // - map setup (+1 s): the Totem Lodge of Ursoc stands by Nordrassil -- Neutral Extra, visible, indestructible; whoever
  //   razes or takes Nordrassil / Cenarion Hold last is recorded.
  // - run: the Druids' units are left behind; the lodge is theirs (HQ, altar, indestructible), Nafien and a few
  //   furbolgs. Every turn at the lodge: a Furbolg plus one more per WORLD TREE their side holds (Nordrassil,
  //   Vordrassil, Shaladrassil, Seradane -- held by them or an ally); every furbolg of theirs is stronger by the same
  //   count (+3 damage, +1 armour, +60 life per tree, kept up to date each turn). A VASSAL of whoever holds the Nordrassil
  //   CP (the Totem: at the start the Druids' conqueror), never defeated.
  // 0.87.0 (maintainer): TIMBERMAW HOLD, their den (Dungeons/VoidDungeons): the lodge (moved to -11154,10970 -- the
  // maintainer's -11138,11210 is a cliff edge; this is the nearest open ground) is its entrance, shut until they come;
  // inside the Shrine of Ursoc (HQ, altar) and the Timbermaw Hold CP, theirs on arrival.
  internal static class FurbolgFallback
  {
    internal const float LodgeX = -11154f; // by Nordrassil (maintainer)
    internal const float LodgeY = 10970f;
    internal const float ShrineX = -15492f; // inside Timbermaw Hold
    internal const float ShrineY = 22308f;
    internal static readonly ExclusivePath Path = new("Furbolg Tribes", Prepare, Run, LodgeX, LodgeY);
    private static readonly Dictionary<unit, int> _applied = new();

    private static readonly int[] Kinds =
    {
      FurbolgObjects.Shaman, FurbolgObjects.Tracker, FurbolgObjects.UrsaWarrior, FurbolgObjects.Champion
    };

    private static readonly int[] Band =
    {
      FurbolgObjects.Furbolg, FurbolgObjects.Furbolg, FurbolgObjects.Furbolg, FurbolgObjects.Furbolg,
      FurbolgObjects.Furbolg, FurbolgObjects.Furbolg, FurbolgObjects.Shaman, FurbolgObjects.Shaman,
      FurbolgObjects.Tracker, FurbolgObjects.Tracker, FurbolgObjects.UrsaWarrior
    };

    internal const string ArrivalText =
      "|cffffcc00Nordrassil is lost to the druids.|r From the deep woods the furbolg tribes gather at the Totem Lodge "
      + "of Ursoc -- and with them Xavius, Lord of the Nightmare, who holds their sacred |cffffcc00Claw of Ursoc|r. "
      + "While he lives or the Claw stays with them, the furbolgs are their own masters. Whoever takes the Claw from "
      + "them takes the furbolgs.";

    private static void Prepare()
    {
      LastConqueror.Watch(UNIT_N002_NORDRASSIL_DRUIDS_OTHER, "Druids");
      LastConqueror.Watch(UNIT_N06D_CENARION_HOLD_SENTINELS_OTHER, "Druids");
      // 0.87.0: the lodge (Timbermaw Hold's entrance) and the shrine are placed by VoidDungeons
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var furbolgs = new FurbolgFaction();
      SwapFaction.Run(p, furbolgs);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      var lodge = VoidDungeons.Timbermaw?.Entrance;
      if (lodge != null && GetUnitTypeId(lodge) != 0)
      {
        SetUnitOwner(lodge, p, true);
        SetUnitInvulnerable(lodge, true);
      }

      VoidDungeons.Timbermaw?.Open(p); // the den opens, the Shrine of Ursoc is theirs
      StartCps.Force(p, DungeonObjects.TimbermawHold);

      Spawn.Units(p, LodgeX, LodgeY - 320f, new[] { FurbolgObjects.Nafien });
      Spawn.Units(p, LodgeX + 256f, LodgeY - 320f, Band);
      var overlord = LastConqueror.Of("Druids");
      // 0.88.0 (maintainer): no Nordrassil seat any more -- the CLAW OF URSOC decides (FurbolgClaw): their own masters
      // while Xavius lives or the Claw is theirs; never defeated
      FurbolgClaw.Start(p, overlord != p ? overlord : null);
      Empower(p);
      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, () => AiApi.Safe("furbolg turn", () =>
      {
        Levy(p);
        Empower(p);
      }), 1, () => p.GetPlayerData().Faction == furbolgs);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(LodgeX, LodgeY));
      AiApi.LogPin("Druids fallback -> Furbolgs" + (overlord != null ? ", conqueror P" + I2S(GetPlayerId(overlord))
        : ", no conqueror"));
    }

    // the world trees held by the furbolgs or an ally
    internal static int Trees(player p)
    {
      var trees = new[]
      {
        AllLegends.Druids.Nordrassil.Unit, AllLegends.Druids.Vordrassil.Unit, AllLegends.Neutral.Shaladrassil.Unit,
        AllLegends.Neutral.Seradane.Unit
      };
      var count = 0;
      for (var i = 0; i < trees.Length; i++)
      {
        var u = trees[i];
        if (u == null || GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var owner = GetOwningPlayer(u);
        if (owner == p || (GetPlayerId(owner) < 24 && IsPlayerAlly(owner, p)))
        {
          count++;
        }
      }

      return count;
    }

    private static void Levy(player p)
    {
      var trees = Trees(p);
      CreateUnit(p, FurbolgObjects.Furbolg, LodgeX + 128f, LodgeY - 448f, 270f);
      for (var i = 0; i < trees; i++)
      {
        CreateUnit(p, Kinds[i % Kinds.Length], LodgeX + 192f + 64f * i, LodgeY - 448f, 270f);
      }
    }

    private static bool IsFurbolg(int type) =>
      type == FurbolgObjects.Furbolg || type == FurbolgObjects.Shaman || type == FurbolgObjects.Tracker
      || type == FurbolgObjects.Champion || type == FurbolgObjects.UrsaWarrior || type == FurbolgObjects.Nafien;

    // +3 damage, +1 armour, +60 life per tree on every furbolg of theirs (the difference to what it has)
    private static void Empower(player p)
    {
      var level = Trees(p);
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (!IsFurbolg(GetUnitTypeId(u)) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var had = _applied.TryGetValue(u, out var a) ? a : 0;
        var delta = level - had;
        if (delta == 0)
        {
          continue;
        }

        BlzSetUnitBaseDamage(u, BlzGetUnitBaseDamage(u, 0) + 3 * delta, 0);
        BlzSetUnitArmor(u, BlzGetUnitArmor(u) + delta);
        var life = GetUnitState(u, UNIT_STATE_LIFE);
        BlzSetUnitMaxHP(u, BlzGetUnitMaxHP(u) + 60 * delta);
        SetUnitState(u, UNIT_STATE_LIFE, life + (delta > 0 ? 60 * delta : 0));
        _applied[u] = level;
      }

      DestroyGroup(g);
    }
  }
}
