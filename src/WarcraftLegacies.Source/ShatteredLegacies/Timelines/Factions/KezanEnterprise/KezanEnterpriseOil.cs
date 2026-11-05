using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.58: the Enterprise's oil -- its income mechanic of its own (TODO step 3), recycled from the 24p goblins:
  // - every Kezan Oil Supply (not buildable: the preplaced one + the start kit) gives 50 gold per minute;
  // - an Oil Supply sends an "Oil Rig" expedition (the 24p's n0AQ, trained there): 40-60 s later an Oil Platform rises
  //   at sea near that Oil Supply (pinged), +25 gold per minute while it stands, at most 8. The platforms can be
  //   attacked (the 24p's were invulnerable). They also unlock the Engineer's Guild and the Fuel Transport (24p data).
  // Income goes through WL's BonusIncome (only the difference is applied).
  internal static class KezanEnterpriseOil
  {
    internal static player Owner;
    internal static int Income;
    internal static List<unit> Platforms = new();
    internal static ObjectiveCounter RigCounter; // the quest "Explosive Engineering" (QuestExplosiveEngineering)

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      var tick = CreateTimer();
      TimerStart(tick, 10f, true, () => AiApi.Safe("kezan oil", Update));
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, () => AiApi.Safe("kezan oil rig", OnTrained));
      Update();
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

    // income from the oil buildings that stand, and the Oil Rig limit: 8 minus the platforms (a running expedition
    // counts by itself -- its hidden unit lives until the platform appears)
    private static void Update()
    {
      for (var i = Platforms.Count - 1; i >= 0; i--)
      {
        if (!Alive(Platforms[i]))
        {
          Platforms.RemoveAt(i);
        }
      }

      var supplies = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Owner, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if ((t == KezanEnterpriseObjects.OilSupply || t == UNIT_H04Z_KEZAN_OIL_SUPPLY_CREEP_OTHER) && Alive(u))
        {
          supplies++;
        }
      }

      DestroyGroup(g);
      var want = supplies * KezanEnterpriseConfig.GoldPerOilSupply
                 + Platforms.Count * KezanEnterpriseConfig.GoldPerOilPlatform;
      if (want != Income)
      {
        Owner.GetPlayerData().BonusIncome += want - Income;
        Income = want;
      }

      var left = KezanEnterpriseConfig.MaxOilPlatforms - Platforms.Count;
      SetPlayerTechMaxAllowed(Owner, KezanEnterpriseObjects.OilRig, left > 0 ? left : 0);
    }

    private static void OnTrained()
    {
      var rig = GetTrainedUnit();
      if (GetUnitTypeId(rig) != KezanEnterpriseObjects.OilRig)
      {
        return;
      }

      var from = GetTriggerUnit();
      var x = GetUnitX(from);
      var y = GetUnitY(from);
      ShowUnit(rig, false);
      PauseUnit(rig, true);
      DisplayTimedTextToPlayer(Owner, 0, 0, 10, "|cffffcc00Oil Rig:|r the expedition sets sail -- the rig rises in "
                                                + "under a minute.");
      var t = CreateTimer();
      TimerStart(t, GetRandomReal(KezanEnterpriseConfig.RigDelayMin, KezanEnterpriseConfig.RigDelayMax), false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("kezan oil rig", () => Land(rig, x, y));
      });
    }

    private static void Land(unit rig, float fromX, float fromY)
    {
      if (GetUnitTypeId(rig) != 0)
      {
        RemoveUnit(rig);
      }

      if (Owner.GetPlayerData().Faction == null || Owner.GetPlayerData().Faction.Name != KezanEnterpriseConfig.Name)
      {
        return; // the Enterprise is gone
      }

      if (!FindSeaSpot(fromX, fromY, out var x, out var y))
      {
        var cost = GetUnitGoldCost(KezanEnterpriseObjects.OilRig);
        SetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD) + cost);
        DisplayTimedTextToPlayer(Owner, 0, 0, 10, "|cffff6600Oil Rig:|r no free oil field near that Oil Supply -- "
                                                  + "the gold is refunded.");
        Update();
        return;
      }

      Platforms.Add(CreateUnit(Owner, KezanEnterpriseObjects.OilPlatform, x, y, 270f));
      DestroyEffect(AddSpecialEffect(@"Objects\Spawnmodels\Naga\NagaDeath\NagaDeath.mdl", x, y));
      if (GetLocalPlayer() == Owner)
      {
        PingMinimap(x, y, 4f); // only on the owner's screen (no game state changes)
      }

      DisplayTimedTextToPlayer(Owner, 0, 0, 10, "|cffffcc00Oil Rig established|r (+"
        + I2S(KezanEnterpriseConfig.GoldPerOilPlatform) + " gold per minute).");
      RigCounter?.Add(1);
      Update();
    }

    // a deep-water spot (floatable, not walkable, also around it) inside the camera bounds, away from other platforms
    private static bool FindSeaSpot(float cx, float cy, out float x, out float y)
    {
      for (var i = 0; i < 60; i++)
      {
        var a = GetRandomReal(0f, 6.2832f);
        var r = GetRandomReal(KezanEnterpriseConfig.RigRangeMin, KezanEnterpriseConfig.RigRangeMax);
        x = cx + r * Cos(a);
        y = cy + r * Sin(a);
        if (x < GetCameraBoundMinX() || x > GetCameraBoundMaxX() || y < GetCameraBoundMinY() || y > GetCameraBoundMaxY()
            || !DeepWater(x, y) || !DeepWater(x + 192f, y) || !DeepWater(x - 192f, y) || !DeepWater(x, y + 192f)
            || !DeepWater(x, y - 192f) || NearPlatform(x, y))
        {
          continue;
        }

        return true;
      }

      x = 0f;
      y = 0f;
      return false;
    }

    // IsTerrainPathable is true where the terrain is NOT pathable for that type
    private static bool DeepWater(float x, float y) =>
      !IsTerrainPathable(x, y, PATHING_TYPE_FLOATABILITY) && IsTerrainPathable(x, y, PATHING_TYPE_WALKABILITY);

    private static bool NearPlatform(float x, float y)
    {
      for (var i = 0; i < Platforms.Count; i++)
      {
        var dx = GetUnitX(Platforms[i]) - x;
        var dy = GetUnitY(Platforms[i]) - y;
        if (dx * dx + dy * dy < KezanEnterpriseConfig.RigSpacing * KezanEnterpriseConfig.RigSpacing)
        {
          return true;
        }
      }

      return false;
    }
  }
}
