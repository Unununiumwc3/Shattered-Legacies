using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // The Enterprise's oil -- its income mechanic of its own. 0.61 (maintainer), like the 24p:
  // - the Kezan Oil Supply (preplaced, -1056 / -6816) FILLS UP with oil -- its mana: its own wells
  //   (KezanEnterpriseConfig.OilPerMinute) and every Oil Platform at sea (OilPerPlatform, at most 8 platforms);
  // - the Oil Refinery (start kit) turns it into gold: Refine Oil (autocast, on from the start -- bots too) takes 1000
  //   oil from the Oil Supply every 60 s -> 100 gold, 150 / 200 with Oil Refining (the refinery's research);
  // - an Oil Supply sends an "Oil Rig" expedition (the 24p's n0AQ, trained there): 40-60 s later an Oil Platform
  //   rises at sea near it (pinged; invulnerable, the 24p's). Platforms unlock the Engineer's Guild, the Fuel
  //   Transport and the traders (24p data / 0.61). The Oil Supply's mana also fuels the mechanical units (24p Fuel
  //   Refill aura) and the Fuel Transports (KezanEnterpriseBombers: their fuel is their blast).
  // 0.58-0.60 paid gold per Oil Supply / platform straight through WL's BonusIncome -- gone.
  internal static class KezanEnterpriseOil
  {
    internal static player Owner;
    internal static int OilRate;                  // oil per minute into the Oil Supply (the scoreboard)
    internal static List<unit> Platforms = new();
    internal static List<unit> Refineries = new();
    internal static List<bool> RefineOn = new();  // per refinery: Refine Oil's autocast (tracked by its orders)
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
      var refine = CreateTimer();
      TimerStart(refine, KezanEnterpriseConfig.RefineEvery, true, () => AiApi.Safe("kezan refine", Refine));
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, () => AiApi.Safe("kezan oil rig", OnTrained));
      var ordered = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(ordered, p, EVENT_PLAYER_UNIT_ISSUED_ORDER, null);
      TriggerAddAction(ordered, () => AiApi.Safe("kezan refine toggle", OnOrder));
      FindRefineries();
      var supply = Supply();
      if (supply != null && GetUnitState(supply, UNIT_STATE_MANA) < KezanEnterpriseConfig.StartOil)
      {
        SetUnitState(supply, UNIT_STATE_MANA, KezanEnterpriseConfig.StartOil); // the first refining at once
      }

      Update();
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

    // the refineries the player has (the start kit), with Refine Oil switched on
    private static void FindRefineries()
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Owner, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == KezanEnterpriseConfig.OilRefinery && !Refineries.Contains(u))
        {
          Refineries.Add(u);
          RefineOn.Add(true);
          IssueImmediateOrderById(u, ORDER_REPLENISH_ON); // autocast on (bots never touch it)
        }
      }

      DestroyGroup(g);
    }

    private static void OnOrder()
    {
      var u = GetTriggerUnit();
      var i = Refineries.IndexOf(u);
      if (i < 0)
      {
        return;
      }

      var order = GetIssuedOrderId();
      if (order == ORDER_REPLENISH_ON)
      {
        RefineOn[i] = true;
      }
      else if (order == ORDER_REPLENISH_OFF)
      {
        RefineOn[i] = false;
      }
    }

    // the first Oil Supply the player has that stands (the oil store)
    internal static unit Supply()
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Owner, null);
      unit found = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetUnitTypeId(u) == KezanEnterpriseObjects.OilSupply && Alive(u))
        {
          found = u;
        }
      }

      DestroyGroup(g);
      return found;
    }

    // every 10 s: oil into the Oil Supply; the Oil Rig limit (8 minus the platforms -- a running expedition counts by
    // itself: its hidden unit lives until the platform appears)
    private static void Update()
    {
      for (var i = Platforms.Count - 1; i >= 0; i--)
      {
        if (!Alive(Platforms[i]))
        {
          Platforms.RemoveAt(i);
        }
      }

      OilRate = KezanEnterpriseConfig.OilPerMinute + Platforms.Count * KezanEnterpriseConfig.OilPerPlatform;
      var supply = Supply();
      if (supply != null)
      {
        var mana = GetUnitState(supply, UNIT_STATE_MANA) + OilRate / 6f;
        var max = GetUnitState(supply, UNIT_STATE_MAX_MANA);
        SetUnitState(supply, UNIT_STATE_MANA, mana < max ? mana : max);
      }

      var left = KezanEnterpriseConfig.MaxOilPlatforms - Platforms.Count;
      SetPlayerTechMaxAllowed(Owner, KezanEnterpriseObjects.OilRig, left > 0 ? left : 0);
    }

    // the oil in store now (the scoreboard)
    internal static int Oil()
    {
      var supply = Supply();
      return supply != null ? R2I(GetUnitState(supply, UNIT_STATE_MANA)) : 0;
    }

    // every 60 s: each refinery with Refine Oil on takes 1000 oil from the Oil Supply -> gold by Oil Refining
    private static void Refine()
    {
      var level = GetPlayerTechCount(Owner, KezanEnterpriseConfig.OilRefining, true);
      var rates = KezanEnterpriseConfig.RefineGold;
      var gold = rates[level < rates.Length ? level : rates.Length - 1];
      for (var i = Refineries.Count - 1; i >= 0; i--)
      {
        var r = Refineries[i];
        if (!Alive(r) || GetOwningPlayer(r) != Owner)
        {
          Refineries.RemoveAt(i);
          RefineOn.RemoveAt(i);
          continue;
        }

        var supply = Supply();
        if (!RefineOn[i] || supply == null || GetUnitState(supply, UNIT_STATE_MANA) < KezanEnterpriseConfig.RefineOil)
        {
          continue;
        }

        SetUnitState(supply, UNIT_STATE_MANA, GetUnitState(supply, UNIT_STATE_MANA) - KezanEnterpriseConfig.RefineOil);
        SetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD) + gold);
        var tag = CreateTextTag();
        SetTextTagText(tag, "+" + I2S(gold), 0.023f);
        SetTextTagPos(tag, GetUnitX(r), GetUnitY(r), 96f);
        SetTextTagColor(tag, 255, 220, 0, 255);
        SetTextTagVelocity(tag, 0f, 0.03f);
        SetTextTagPermanent(tag, false);
        SetTextTagLifespan(tag, 3f);
        SetTextTagFadepoint(tag, 2f);
        SetTextTagVisibility(tag, GetLocalPlayer() == Owner); // only the owner sees it (no game state)
      }
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
        + I2S(KezanEnterpriseConfig.OilPerPlatform) + " oil per minute into the Oil Supply).");
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
