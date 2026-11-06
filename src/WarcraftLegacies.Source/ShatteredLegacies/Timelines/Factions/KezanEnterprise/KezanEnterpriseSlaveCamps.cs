using System.Collections.Generic;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.60, the Refugee Slave Camps (maintainer's design; replace the Goblin Empire quest's coast). Lore: when a kingdom
  // falls, its survivors flee to the goblin ports -- and sign Kezan contracts to get off the docks. The Enterprise
  // sells them as soldiers to whoever rules the port, and takes its cut of every contract.
  // - One camp per goblin town (Booty Bay, Ratchet, Gadgetzan, Venture Bay, Fuselight, Everlook), placed next to the
  //   town's CP when the Enterprise arrives (for now prebuilt -- later built one by one, TODO.md). Invulnerable; it
  //   belongs to whoever holds the town's CP (that ruler trades with Kezan; a neutral town: nobody), so taking the CP
  //   takes the camp. The Enterprise holding the CP uses the refugees itself.
  // - Each camp sells the refugees of its region: one unit per WL faction, only once that faction has fallen
  //   (defeated, or its fallback -- TimelineSystem.FactionFell); until then greyed out ("No refugees from X yet").
  // - Stock: 3 per unit, one back every 2 min (the owner's max-allowed = alive + stock).
  // - Every sale to another player pays the Enterprise the unit's price; every unit sold at least once completes
  //   "Master of the Slave Trade" (QuestMasterOfTheSlaveTrade).
  // Every faction gets an object limit for the refugees: WL refunds any trained unit whose limit is 0 in the buyer's
  // faction (GameLogic/RefundZeroLimitUnits -- why the 0.39.8 creep market trained nothing).
  // Data: KezanEnterpriseSlaveCampsData (generated, tools/shattered-legacies/content/kezan_objects.py).
  internal static class KezanEnterpriseSlaveCamps
  {
    internal static player Kezan;
    internal static List<unit> Camps = new();
    internal static List<int> Stock = new();
    internal static List<bool> Sold = new();
    internal static List<string> Fallen = new();
    internal static ObjectiveCounter SoldCounter; // QuestMasterOfTheSlaveTrade
    internal static bool GallywixTitled;          // its reward: new Gallywix units get the title too

    // map setup: limits for every faction, and fallen factions are recorded from the start (the Enterprise arrives late)
    internal static void Setup()
    {
      var units = KezanEnterpriseSlaveCampsData.Units;
      for (var i = 0; i < units.Length; i++)
      {
        Stock.Add(KezanEnterpriseConfig.CampStock);
        Sold.Add(false);
      }

      foreach (var f in FactionManager.GetAllFactions())
      {
        Register(f);
      }

      FactionManager.FactionRegistered += Register;
      TimelineSystem.FactionFell.Add(Fall);
    }

    private static void Register(Faction f)
    {
      var units = KezanEnterpriseSlaveCampsData.Units;
      for (var i = 0; i < units.Length; i++)
      {
        f.ModObjectLimit(units[i], KezanEnterpriseConfig.RefugeeLimit);
      }

      f.ScoreStatusChanged += x =>
      {
        if (x.ScoreStatus == ScoreStatus.Defeated)
        {
          AiApi.Safe("slave camps fell", () => Fall(x.Name));
        }
      };
    }

    // the refugees of that faction become available -- to everyone (whoever rules a camp can buy them)
    private static void Fall(string name)
    {
      if (Fallen.Contains(name))
      {
        return;
      }

      Fallen.Add(name);
      var names = KezanEnterpriseSlaveCampsData.Factions;
      for (var i = 0; i < names.Length; i++)
      {
        if (names[i] != name)
        {
          continue;
        }

        for (var j = 0; j < 24; j++)
        {
          SetPlayerTechResearched(Player(j), KezanEnterpriseSlaveCampsData.Conditions[i], 1);
        }

        if (Kezan != null)
        {
          DisplayTimedTextToPlayer(Kezan, 0, 0, 12, "|cffffcc00Refugees from " + name + "|r arrive at the camp of "
            + KezanEnterpriseSlaveCampsData.CampTowns[KezanEnterpriseSlaveCampsData.UnitCamp[i]] + ".");
        }
      }
    }

    // the Enterprise arrives: the camps rise next to the goblin towns' CPs
    internal static void Start(player p)
    {
      if (Kezan != null)
      {
        return;
      }

      Kezan = p;
      var types = KezanEnterpriseSlaveCampsData.Camps;
      var xy = KezanEnterpriseSlaveCampsData.CampXY;
      for (var k = 0; k < types.Length; k++)
      {
        unit camp = null;
        if (AiApi.FindBuildSpot(xy[k * 2], xy[k * 2 + 1], new List<float>(), new List<float>(), out var x, out var y))
        {
          camp = CreateUnit(Player(PLAYER_NEUTRAL_PASSIVE), types[k], x, y, 270f);
          SetUnitInvulnerable(camp, true);
        }
        else
        {
          AiApi.Log("slave camps: no room next to " + KezanEnterpriseSlaveCampsData.CampTowns[k]);
        }

        Camps.Add(camp);
      }

      for (var j = 0; j < 24; j++)
      {
        var trained = CreateTrigger();
        TriggerRegisterPlayerUnitEvent(trained, Player(j), EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
        TriggerAddAction(trained, () => AiApi.Safe("slave camps sale", OnTrained));
      }

      var sync = CreateTimer();
      TimerStart(sync, 5f, true, () => AiApi.Safe("slave camps owners", SyncOwners));
      var restock = CreateTimer();
      TimerStart(restock, KezanEnterpriseConfig.CampRestockEvery, true, () => AiApi.Safe("slave camps stock", Restock));
      SyncOwners();
    }

    // each camp belongs to the ruler of its town's CP (neutral -> nobody)
    private static void SyncOwners()
    {
      for (var k = 0; k < Camps.Count; k++)
      {
        var camp = Camps[k];
        if (camp == null || GetUnitTypeId(camp) == 0)
        {
          continue;
        }

        var cp = AiApi.SafeCp(KezanEnterpriseSlaveCampsData.CampCps[k]);
        var ruler = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : Player(PLAYER_NEUTRAL_PASSIVE);
        if (ruler == Player(PLAYER_NEUTRAL_AGGRESSIVE))
        {
          ruler = Player(PLAYER_NEUTRAL_PASSIVE);
        }

        var old = GetOwningPlayer(camp);
        if (old != ruler)
        {
          SetUnitOwner(camp, ruler, true);
          if (ruler != Player(PLAYER_NEUTRAL_PASSIVE) && Kezan != null && ruler != Kezan)
          {
            DisplayTimedTextToPlayer(ruler, 0, 0, 12, "|cffffcc00Refugee Slave Camp of "
              + KezanEnterpriseSlaveCampsData.CampTowns[k] + "|r: you rule the town -- you may buy its refugees "
              + "(the Kezan Enterprise takes its cut).");
          }
        }

        UpdateCamp(k, old);
        if (old != ruler)
        {
          UpdateCamp(k, ruler);
        }
      }
    }

    // the max-allowed of this camp's refugees for that player: alive + stock (owner), alive (anyone else)
    private static void UpdateCamp(int k, player who)
    {
      if (who == Player(PLAYER_NEUTRAL_PASSIVE) || GetPlayerId(who) >= 24)
      {
        return;
      }

      var owner = Camps[k] != null ? GetOwningPlayer(Camps[k]) : null;
      var units = KezanEnterpriseSlaveCampsData.Units;
      for (var i = 0; i < units.Length; i++)
      {
        if (KezanEnterpriseSlaveCampsData.UnitCamp[i] == k)
        {
          var alive = GetPlayerUnitTypeCount(who, units[i]);
          SetPlayerTechMaxAllowed(who, units[i], who == owner ? alive + Stock[i] : alive);
        }
      }
    }

    private static void Restock()
    {
      for (var i = 0; i < Stock.Count; i++)
      {
        if (Stock[i] < KezanEnterpriseConfig.CampStock)
        {
          var more = Stock[i] + 1;
          Stock[i] = more;
        }
      }

      for (var k = 0; k < Camps.Count; k++)
      {
        if (Camps[k] != null)
        {
          UpdateCamp(k, GetOwningPlayer(Camps[k]));
        }
      }
    }

    private static void OnTrained()
    {
      var u = GetTrainedUnit();
      var t = GetUnitTypeId(u);
      if (t == KezanEnterpriseObjects.Gallywix && GallywixTitled)
      {
        BlzSetUnitName(u, KezanEnterpriseConfig.GallywixTitle); // the title stays with a new Gallywix
        return;
      }

      var units = KezanEnterpriseSlaveCampsData.Units;
      var i = 0;
      while (i < units.Length && units[i] != t)
      {
        i++;
      }

      if (i >= units.Length)
      {
        return;
      }

      var buyer = GetOwningPlayer(u);
      var left = Stock[i] > 0 ? Stock[i] - 1 : 0;
      Stock[i] = left;
      UpdateCamp(KezanEnterpriseSlaveCampsData.UnitCamp[i], buyer);
      if (Kezan != null && buyer != Kezan)
      {
        var price = GetUnitGoldCost(t);
        SetPlayerState(Kezan, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Kezan, PLAYER_STATE_RESOURCE_GOLD) + price);
        KezanEnterpriseTrade.Earned += price; // the scoreboard's "Kezan trade"
        DisplayTimedTextToPlayer(Kezan, 0, 0, 8, "|cffffcc00Contract sold:|r " + GetUnitName(u) + " to "
          + GetPlayerName(buyer) + " (+" + I2S(price) + " gold).");
      }

      if (!Sold[i])
      {
        Sold[i] = true;
        SoldCounter?.Add(1);
      }
    }
  }
}
