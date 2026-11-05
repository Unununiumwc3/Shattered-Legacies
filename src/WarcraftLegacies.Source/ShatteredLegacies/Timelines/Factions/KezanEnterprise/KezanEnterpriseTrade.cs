using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.59 (maintainer: the Trading Center from the 24p): its Trading Zeppelins (fly, at most 16) and Traders (ships, at
  // most 10) cannot be controlled (Locust, invulnerable -- 24p data) and trade on their own, as in the 24p: from the
  // Trading Center to a random goblin town and back; every return pays gold by the length of the trip (KezanEnterprise
  // Config.TradeGoldPer1000). Ships sail to the town's harbour (KezanEnterpriseConfig.TradeTowns, a deep-water spot
  // next to it, found offline on the map's pathing), zeppelins fly to the town itself. A trader that stops (arrived,
  // or as far as it gets) or is under way too long counts as arrived. They return to the Trading Center the player
  // owns that is nearest; with none they wait.
  internal static class KezanEnterpriseTrade
  {
    internal static player Owner;
    internal static List<unit> Traders = new();
    internal static List<int> Leg = new();        // per trader: the town index it heads for, or -1 = home
    internal static List<float> LegStart = new(); // per trader: when the leg began
    internal static List<float> FromX = new();    // per trader: where the trip began (the Trading Center)
    internal static List<float> FromY = new();
    internal static List<float> OneWay = new();   // per trader: the length of the town leg just made (0 = none)
    internal static float Clock;
    internal static int Earned;                   // gold earned by trade (the scoreboard)

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, () => AiApi.Safe("kezan trade", OnTrained));
      var tick = CreateTimer();
      TimerStart(tick, KezanEnterpriseConfig.TradeCheckEvery, true, () => AiApi.Safe("kezan trade", Tick));
    }

    private static bool IsTraderType(int t) =>
      t == KezanEnterpriseObjects.TradingZeppelin || t == KezanEnterpriseObjects.Trader;

    private static void OnTrained()
    {
      var u = GetTrainedUnit();
      if (!IsTraderType(GetUnitTypeId(u)))
      {
        return;
      }

      Traders.Add(u);
      Leg.Add(-1);
      LegStart.Add(Clock);
      FromX.Add(GetUnitX(u));
      FromY.Add(GetUnitY(u));
      OneWay.Add(0f);
      SendOut(Traders.Count - 1);
    }

    private static void Tick()
    {
      Clock += KezanEnterpriseConfig.TradeCheckEvery;
      for (var i = Traders.Count - 1; i >= 0; i--)
      {
        var u = Traders[i];
        if (GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f || GetOwningPlayer(u) != Owner)
        {
          Traders.RemoveAt(i);
          Leg.RemoveAt(i);
          LegStart.RemoveAt(i);
          FromX.RemoveAt(i);
          FromY.RemoveAt(i);
          OneWay.RemoveAt(i);
          continue;
        }

        var stopped = GetUnitCurrentOrder(u) == 0;
        var tooLong = Clock - LegStart[i] > KezanEnterpriseConfig.TradeLegMax;
        if (!stopped && !tooLong)
        {
          continue;
        }

        if (Leg[i] >= 0)
        {
          GoHome(i); // at the town: back to the Trading Center
        }
        else
        {
          Pay(i);     // home again (or never left): paid by the trip just made, then out again
          SendOut(i);
        }
      }
    }

    // to a random goblin town (not Kezan); the trip starts here
    private static void SendOut(int i)
    {
      var u = Traders[i];
      var towns = KezanEnterpriseConfig.TradeTowns;
      var n = towns.Length / 4;
      var k = GetRandomInt(0, n - 1);
      var ship = GetUnitTypeId(u) == KezanEnterpriseObjects.Trader;
      var x = ship ? towns[k * 4 + 2] : towns[k * 4];
      var y = ship ? towns[k * 4 + 3] : towns[k * 4 + 1];
      Leg[i] = k;
      LegStart[i] = Clock;
      FromX[i] = GetUnitX(u);
      FromY[i] = GetUnitY(u);
      IssuePointOrderById(u, ORDER_MOVE, x, y);
    }

    // back to the nearest Trading Center the player owns; none -> wait where it is (it tries again next check)
    private static void GoHome(int i)
    {
      var u = Traders[i];
      if (!NearestCenter(GetUnitX(u), GetUnitY(u), out var hx, out var hy))
      {
        return;
      }

      // the pay counts the town leg twice (there and back)
      var dx = GetUnitX(u) - FromX[i];
      var dy = GetUnitY(u) - FromY[i];
      OneWay[i] = SquareRoot(dx * dx + dy * dy);
      Leg[i] = -1;
      LegStart[i] = Clock;
      IssuePointOrderById(u, ORDER_MOVE, hx, hy);
    }

    private static void Pay(int i)
    {
      var oneWay = OneWay[i];
      OneWay[i] = 0f;
      if (oneWay <= 0f)
      {
        return; // never went out (just trained)
      }

      var per = GetUnitTypeId(Traders[i]) == KezanEnterpriseObjects.Trader
        ? KezanEnterpriseConfig.TraderGoldPer1000
        : KezanEnterpriseConfig.ZeppelinGoldPer1000;
      var gold = R2I(2f * oneWay / 1000f * per);
      if (gold < 1)
      {
        return;
      }

      SetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD) + gold);
      Earned += gold;
      var u = Traders[i];
      var tag = CreateTextTag();
      SetTextTagText(tag, "+" + I2S(gold), 0.023f);
      SetTextTagPos(tag, GetUnitX(u), GetUnitY(u), 64f);
      SetTextTagColor(tag, 255, 220, 0, 255);
      SetTextTagVelocity(tag, 0f, 0.03f);
      SetTextTagPermanent(tag, false);
      SetTextTagLifespan(tag, 2.5f);
      SetTextTagFadepoint(tag, 1.5f);
      SetTextTagVisibility(tag, GetLocalPlayer() == Owner); // only the owner sees it (no game state)
    }

    private static bool NearestCenter(float x, float y, out float hx, out float hy)
    {
      hx = 0f;
      hy = 0f;
      var best = -1f;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, Owner, null);
      unit c;
      while ((c = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, c);
        if (GetUnitTypeId(c) != KezanEnterpriseObjects.TradingCenter || GetUnitState(c, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        var dx = GetUnitX(c) - x;
        var dy = GetUnitY(c) - y;
        var d = dx * dx + dy * dy;
        if (best < 0f || d < best)
        {
          best = d;
          hx = GetUnitX(c);
          hy = GetUnitY(c);
        }
      }

      DestroyGroup(g);
      return best >= 0f;
    }
  }
}
