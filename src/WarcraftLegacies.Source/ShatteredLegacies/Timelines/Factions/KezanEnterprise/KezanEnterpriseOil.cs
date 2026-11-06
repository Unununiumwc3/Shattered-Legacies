using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // The Enterprise's oil -- its income mechanic of its own (0.58; 0.61 like the 24p; 0.62, maintainer: carried):
  // - an Oil Supply sends an "Oil Rig" expedition (the 24p's n0AQ): 40-60 s later an Oil Platform rises at sea near it
  //   (pinged; invulnerable, the 24p's). It pumps 625 oil per minute into its own tank (its mana, 3000) from a field
  //   of 40,000 -- when the field runs dry the platform collapses: a new rig is needed. At most 8.
  // - Oil Tankers (the Trading Center's ships, KezanEnterpriseTrade) carry it home into the Kezan Oil Supply (mana);
  // - the Oil Refinery turns it into gold: Refine Oil (cast on the Oil Supply from any distance, or autocast -- on
  //   from the start, bots too) takes 1000 oil -> 100 gold, 150 / 200 with Oil Refining; cooldown 60 s.
  // - the Oil Supply's oil also fuels the mechanical units (24p Fuel Refill) and the Fuel Transports' blast.
  // Platforms unlock the Engineer's Guild, the Fuel Transport and the traders (24p data / 0.61). No oil without rigs
  // and tankers (0.61 had 200 per minute from nowhere).
  internal static class KezanEnterpriseOil
  {
    private const int RefineOn = 852150;  // faeriefireon (Refine Oil, Faerie Fire based since 0.62)
    private const int RefineOff = 852151; // faeriefireoff

    internal static player Owner;
    internal static int OilRate;                  // oil per minute the platforms pump (the scoreboard)
    internal static List<unit> Platforms = new();
    internal static List<int> Field = new();      // per platform: oil left in its field
    internal static List<unit> Refineries = new();
    internal static List<bool> AutoOn = new();    // per refinery: Refine Oil's autocast (tracked by its orders)
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
      TimerStart(refine, 5f, true, () => AiApi.Safe("kezan refine", AutoRefine));
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, () => AiApi.Safe("kezan oil rig", OnTrained));
      var ordered = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(ordered, p, EVENT_PLAYER_UNIT_ISSUED_ORDER, null);
      TriggerAddAction(ordered, () => AiApi.Safe("kezan refine toggle", OnOrder));
      var cast = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(cast, p, EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      TriggerAddAction(cast, () => AiApi.Safe("kezan refine cast", OnCast));
      FindRefineries();
      Update();
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

    // the refineries the player has, a new one with Refine Oil's autocast switched on
    private static void FindRefineries()
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Owner, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == KezanEnterpriseConfig.OilRefinery && Alive(u) && !Refineries.Contains(u))
        {
          Refineries.Add(u);
          AutoOn.Add(true);
          IssueImmediateOrderById(u, RefineOn); // autocast on (bots never touch it)
        }
      }

      DestroyGroup(g);
    }

    private static void OnOrder()
    {
      var i = Refineries.IndexOf(GetTriggerUnit());
      if (i < 0)
      {
        return;
      }

      var order = GetIssuedOrderId();
      if (order == RefineOn)
      {
        AutoOn[i] = true;
      }
      else if (order == RefineOff)
      {
        AutoOn[i] = false;
      }
    }

    // a manual cast (on the Oil Supply -- any target of the player's does: the oil is always the Oil Supply's)
    private static void OnCast()
    {
      if (GetSpellAbilityId() == KezanEnterpriseConfig.RefineAbility)
      {
        Refine(GetTriggerUnit(), true);
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

    // every 10 s: the platforms pump into their tanks; a dry field -> the platform collapses; the Oil Rig limit (8
    // minus the platforms -- a running expedition counts by itself: its hidden unit lives until the platform appears)
    private static void Update()
    {
      var pumped = 0;
      for (var i = Platforms.Count - 1; i >= 0; i--)
      {
        var pl = Platforms[i];
        if (!Alive(pl))
        {
          Platforms.RemoveAt(i);
          Field.RemoveAt(i);
          continue;
        }

        var room = GetUnitState(pl, UNIT_STATE_MAX_MANA) - GetUnitState(pl, UNIT_STATE_MANA);
        var pump = KezanEnterpriseConfig.OilPerPlatform / 6;
        pump = pump < Field[i] ? pump : Field[i];
        pump = pump < R2I(room) ? pump : R2I(room);
        if (pump > 0)
        {
          SetUnitState(pl, UNIT_STATE_MANA, GetUnitState(pl, UNIT_STATE_MANA) + pump);
          var left = Field[i] - pump;
          Field[i] = left;
        }

        if (Field[i] <= 0 && GetUnitState(pl, UNIT_STATE_MANA) < 1f)
        {
          // dry and emptied: the platform collapses (a rig slot is free again)
          DisplayTimedTextToPlayer(Owner, 0, 0, 10, "|cffff6600An Oil Platform ran dry|r and collapses -- send a new "
                                                    + "Oil Rig expedition.");
          SetUnitInvulnerable(pl, false);
          KillUnit(pl);
          Platforms.RemoveAt(i);
          Field.RemoveAt(i);
          continue;
        }

        if (Field[i] > 0)
        {
          pumped++;
        }
      }

      OilRate = pumped * KezanEnterpriseConfig.OilPerPlatform;
      var cap = KezanEnterpriseConfig.MaxOilPlatforms - Platforms.Count;
      SetPlayerTechMaxAllowed(Owner, KezanEnterpriseObjects.OilRig, cap > 0 ? cap : 0);
    }

    // the oil in store now (the scoreboard)
    internal static int Oil()
    {
      var supply = Supply();
      return supply != null ? R2I(GetUnitState(supply, UNIT_STATE_MANA)) : 0;
    }

    // every 5 s: each refinery with autocast on whose Refine Oil is ready refines (and goes on cooldown)
    private static void AutoRefine()
    {
      FindRefineries(); // one that came later
      for (var i = Refineries.Count - 1; i >= 0; i--)
      {
        var r = Refineries[i];
        if (!Alive(r) || GetOwningPlayer(r) != Owner)
        {
          Refineries.RemoveAt(i);
          AutoOn.RemoveAt(i);
          continue;
        }

        if (AutoOn[i] && BlzGetUnitAbilityCooldownRemaining(r, KezanEnterpriseConfig.RefineAbility) <= 0f)
        {
          Refine(r, false);
        }
      }
    }

    // 1000 oil of the Oil Supply -> gold by Oil Refining (a manual cast already started the cooldown)
    private static void Refine(unit r, bool manual)
    {
      var supply = Supply();
      if (supply == null || GetUnitState(supply, UNIT_STATE_MANA) < KezanEnterpriseConfig.RefineOil)
      {
        if (manual)
        {
          DisplayTimedTextToPlayer(Owner, 0, 0, 8, "|cffff6600Refine Oil:|r the Oil Supply holds less than "
                                                   + I2S(KezanEnterpriseConfig.RefineOil) + " oil.");
          BlzEndUnitAbilityCooldown(r, KezanEnterpriseConfig.RefineAbility);
        }

        return;
      }

      var level = GetPlayerTechCount(Owner, KezanEnterpriseConfig.OilRefining, true);
      var rates = KezanEnterpriseConfig.RefineGold;
      var gold = rates[level < rates.Length ? level : rates.Length - 1];
      SetUnitState(supply, UNIT_STATE_MANA, GetUnitState(supply, UNIT_STATE_MANA) - KezanEnterpriseConfig.RefineOil);
      SetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD) + gold);
      if (!manual)
      {
        BlzStartUnitAbilityCooldown(r, KezanEnterpriseConfig.RefineAbility, KezanEnterpriseConfig.RefineEvery);
      }

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
      Field.Add(KezanEnterpriseConfig.PlatformField);
      DestroyEffect(AddSpecialEffect(@"Objects\Spawnmodels\Naga\NagaDeath\NagaDeath.mdl", x, y));
      if (GetLocalPlayer() == Owner)
      {
        PingMinimap(x, y, 4f); // only on the owner's screen (no game state changes)
      }

      DisplayTimedTextToPlayer(Owner, 0, 0, 10, "|cffffcc00Oil Rig established|r (+"
        + I2S(KezanEnterpriseConfig.OilPerPlatform) + " oil per minute into its tank -- Oil Tankers carry it home).");
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
