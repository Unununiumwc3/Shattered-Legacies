using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // The Trading Center's ships and zeppelins (0.59 from the 24p; 0.62, maintainer: two jobs). Both cannot be
  // controlled (Locust, invulnerable -- 24p data) and work on their own:
  // - Trading Zeppelins (at most 16) trade GOLD: from the Trading Center to a random goblin town and back; every return
  //   pays by the length of the trip (KezanEnterpriseConfig.ZeppelinGoldPer1000).
  // - Oil Tankers (the 24p's "Trader" ship, at most 10) carry OIL: to the Oil Platform with the most oil not yet
  //   claimed by another tanker, load up to 300 (its tank), back to the Trading Center, unload into the Kezan Oil
  //   Supply. Without platforms they wait at home. They never go to the goblin towns.
  // A unit that stops (arrived, or as far as it gets) or is under way too long counts as arrived. Home = the nearest
  // Trading Center the player owns; with none they wait.
  internal static class KezanEnterpriseTrade
  {
    internal static player Owner;
    internal static float Clock;
    internal static int Earned;                   // gold earned by trade (the scoreboard)
    internal static int Delivered;                // oil the tankers brought home (the scoreboard)

    // zeppelins
    internal static List<unit> Zeppelins = new();
    internal static List<int> Leg = new();        // per zeppelin: the town it heads for, or -1 = home
    internal static List<float> LegStart = new(); // per zeppelin: when the leg began
    internal static List<float> FromX = new();    // per zeppelin: where the trip began (the Trading Center)
    internal static List<float> FromY = new();
    internal static List<float> OneWay = new();   // per zeppelin: the length of the town leg just made (0 = none)

    // tankers
    internal static List<unit> Tankers = new();
    internal static List<unit> Target = new();    // per tanker: the platform it heads for, or null = home / waiting
    internal static List<int> Cargo = new();      // per tanker: oil on board
    internal static List<float> TankerStart = new();

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

    private static bool Alive(unit u) =>
      u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && GetOwningPlayer(u) == Owner;

    private static void OnTrained()
    {
      var u = GetTrainedUnit();
      var t = GetUnitTypeId(u);
      if (t == KezanEnterpriseObjects.TradingZeppelin)
      {
        Zeppelins.Add(u);
        Leg.Add(-1);
        LegStart.Add(Clock);
        FromX.Add(GetUnitX(u));
        FromY.Add(GetUnitY(u));
        OneWay.Add(0f);
        SendOut(Zeppelins.Count - 1);
      }
      else if (t == KezanEnterpriseObjects.OilTanker)
      {
        Tankers.Add(u);
        Target.Add(null);
        Cargo.Add(0);
        TankerStart.Add(Clock);
      }
    }

    private static void Tick()
    {
      Clock += KezanEnterpriseConfig.TradeCheckEvery;
      TickZeppelins();
      TickTankers();
    }

    // ------------------------------------------------------------------------------------------------- zeppelins
    private static void TickZeppelins()
    {
      for (var i = Zeppelins.Count - 1; i >= 0; i--)
      {
        var u = Zeppelins[i];
        if (!Alive(u))
        {
          Zeppelins.RemoveAt(i);
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
      var u = Zeppelins[i];
      var towns = KezanEnterpriseConfig.TradeTowns;
      var k = GetRandomInt(0, towns.Length / 4 - 1);
      Leg[i] = k;
      LegStart[i] = Clock;
      FromX[i] = GetUnitX(u);
      FromY[i] = GetUnitY(u);
      IssuePointOrderById(u, ORDER_MOVE, towns[k * 4], towns[k * 4 + 1]);
    }

    private static void GoHome(int i)
    {
      var u = Zeppelins[i];
      if (!NearestCenter(GetUnitX(u), GetUnitY(u), out var hx, out var hy))
      {
        return;
      }

      var dx = GetUnitX(u) - FromX[i];
      var dy = GetUnitY(u) - FromY[i];
      OneWay[i] = SquareRoot(dx * dx + dy * dy); // the pay counts the town leg twice (there and back)
      Leg[i] = -1;
      LegStart[i] = Clock;
      IssuePointOrderById(u, ORDER_MOVE, hx, hy);
    }

    private static void Pay(int i)
    {
      var oneWay = OneWay[i];
      OneWay[i] = 0f;
      var gold = R2I(2f * oneWay / 1000f * KezanEnterpriseConfig.ZeppelinGoldPer1000);
      if (gold < 1)
      {
        return; // never went out (just trained)
      }

      SetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(Owner, PLAYER_STATE_RESOURCE_GOLD) + gold);
      Earned += gold;
      Tag(Zeppelins[i], "+" + I2S(gold), 255, 220, 0);
    }

    // ---------------------------------------------------------------------------------------------------- tankers
    private static void TickTankers()
    {
      for (var i = Tankers.Count - 1; i >= 0; i--)
      {
        var u = Tankers[i];
        if (!Alive(u))
        {
          Tankers.RemoveAt(i);
          Target.RemoveAt(i);
          Cargo.RemoveAt(i);
          TankerStart.RemoveAt(i);
          continue;
        }

        var stopped = GetUnitCurrentOrder(u) == 0;
        var tooLong = Clock - TankerStart[i] > KezanEnterpriseConfig.TradeLegMax;
        var target = Target[i];
        if (target != null)
        {
          if (!IsPlatform(target))
          {
            Target[i] = null; // the platform collapsed: home with what it has, or a new one
            GoHomeTanker(i);
            continue;
          }

          if (Near(u, target) || stopped || tooLong)
          {
            Load(i, target);
            GoHomeTanker(i);
          }

          continue;
        }

        // heading home or waiting there
        if (!stopped && !tooLong)
        {
          continue;
        }

        if (Cargo[i] > 0)
        {
          Unload(i);
          if (Cargo[i] > 0)
          {
            GoHomeTanker(i); // not home yet (stopped short), or the Oil Supply is full: try again
            continue;
          }
        }

        var next = BestPlatform(u);
        if (next != null)
        {
          Target[i] = next;
          TankerStart[i] = Clock;
          IssuePointOrderById(u, ORDER_MOVE, GetUnitX(next), GetUnitY(next));
        }
      }
    }

    private static bool IsPlatform(unit p) =>
      Alive(p) && GetUnitTypeId(p) == KezanEnterpriseObjects.OilPlatform;

    private static bool Near(unit a, unit b)
    {
      var dx = GetUnitX(a) - GetUnitX(b);
      var dy = GetUnitY(a) - GetUnitY(b);
      return dx * dx + dy * dy < KezanEnterpriseConfig.TankerReach * KezanEnterpriseConfig.TankerReach;
    }

    private static void Load(int i, unit platform)
    {
      var oil = R2I(GetUnitState(platform, UNIT_STATE_MANA));
      var room = KezanEnterpriseConfig.TankerLoad - Cargo[i];
      var take = oil < room ? oil : room;
      if (take <= 0)
      {
        return;
      }

      SetUnitState(platform, UNIT_STATE_MANA, GetUnitState(platform, UNIT_STATE_MANA) - take);
      var cargo = Cargo[i] + take;
      Cargo[i] = cargo;
    }

    private static void GoHomeTanker(int i)
    {
      var u = Tankers[i];
      Target[i] = null;
      TankerStart[i] = Clock;
      if (NearestCenter(GetUnitX(u), GetUnitY(u), out var hx, out var hy))
      {
        IssuePointOrderById(u, ORDER_MOVE, hx, hy);
      }
    }

    // at home: the cargo into the Oil Supply (what does not fit stays on board)
    private static void Unload(int i)
    {
      var u = Tankers[i];
      if (!NearestCenter(GetUnitX(u), GetUnitY(u), out var hx, out var hy))
      {
        return;
      }

      var dx = GetUnitX(u) - hx;
      var dy = GetUnitY(u) - hy;
      var reach = KezanEnterpriseConfig.TankerReach * 3f; // the Trading Center stands on the shore: ships stop short
      var supply = KezanEnterpriseOil.Supply();
      if (supply == null || dx * dx + dy * dy > reach * reach)
      {
        return;
      }

      var room = R2I(GetUnitState(supply, UNIT_STATE_MAX_MANA) - GetUnitState(supply, UNIT_STATE_MANA));
      var put = Cargo[i] < room ? Cargo[i] : room;
      if (put <= 0)
      {
        return;
      }

      SetUnitState(supply, UNIT_STATE_MANA, GetUnitState(supply, UNIT_STATE_MANA) + put);
      var left = Cargo[i] - put;
      Cargo[i] = left;
      Delivered += put;
      Tag(u, "+" + I2S(put) + " oil", 80, 80, 80);
    }

    // the platform with the most oil that other tankers have not claimed yet (null = none worth going for)
    private static unit BestPlatform(unit tanker)
    {
      unit best = null;
      var bestOil = 0f;
      var platforms = KezanEnterpriseOil.Platforms;
      for (var p = 0; p < platforms.Count; p++)
      {
        var pl = platforms[p];
        if (!IsPlatform(pl))
        {
          continue;
        }

        var oil = GetUnitState(pl, UNIT_STATE_MANA);
        for (var j = 0; j < Tankers.Count; j++)
        {
          if (Target[j] == pl)
          {
            oil -= KezanEnterpriseConfig.TankerLoad;
          }
        }

        if (oil > bestOil)
        {
          bestOil = oil;
          best = pl;
        }
      }

      return bestOil >= KezanEnterpriseConfig.TankerLoad / 2f ? best : null;
    }

    // ----------------------------------------------------------------------------------------------------- shared
    private static void Tag(unit u, string text, int r, int g, int b)
    {
      var tag = CreateTextTag();
      SetTextTagText(tag, text, 0.023f);
      SetTextTagPos(tag, GetUnitX(u), GetUnitY(u), 64f);
      SetTextTagColor(tag, r, g, b, 255);
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
