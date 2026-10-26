using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies
{
  // 0.39.8: the Kezan Enterprise's special buildings (maintainer's design), started by the Kul Tiras fallback.
  // - Kezan Oil Supply (h04Z): +50 gold per minute each while it stands (WL's BonusIncome; cannot be captured).
  // - Kezan Trading Center (o04M): a creep market. Each creep type has a stock of 3; one comes back every 2 min
  //   (6 min for a full stock). Done by code: the type's max-allowed = units alive + stock.
  internal static class KezanEnterprise
  {
    private const int OilSupplyType = 1747989594;     // h04Z
    private const int GoldPerOilSupply = 50;
    private const int StockMax = 3;
    private const float RestockEvery = 120f;
    private static readonly int[] MarketTypes =
    {
      FourCC("nogr"), FourCC("nomg"), FourCC("nftr"), FourCC("nfsp"), FourCC("nmrr"), FourCC("ncen"),
    };

    private static player _p;
    private static int _oilIncome;
    private static readonly Dictionary<int, int> _stock = new();

    internal static void Start(player p)
    {
      if (_p != null)
      {
        return;
      }

      _p = p;
      for (var i = 0; i < MarketTypes.Length; i++)
      {
        _stock[MarketTypes[i]] = StockMax;
        UpdateAllowed(MarketTypes[i]);
      }

      var tick = CreateTimer();
      TimerStart(tick, 10f, true, () => AiApi.Safe("kezan oil", UpdateOilIncome));
      var restock = CreateTimer();
      TimerStart(restock, RestockEvery, true, () => AiApi.Safe("kezan market", Restock));
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, OnTrained);
      UpdateOilIncome();
    }

    // income = 50 per living Oil Supply; only the difference is applied to WL's BonusIncome
    private static void UpdateOilIncome()
    {
      var n = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, _p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == OilSupplyType && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          n++;
        }
      }

      DestroyGroup(g);
      var want = n * GoldPerOilSupply;
      if (want != _oilIncome)
      {
        _p.GetPlayerData().BonusIncome += want - _oilIncome;
        _oilIncome = want;
      }
    }

    private static void OnTrained()
    {
      var t = GetUnitTypeId(GetTrainedUnit());
      if (_stock.ContainsKey(t))
      {
        _stock[t] = _stock[t] > 0 ? _stock[t] - 1 : 0;
        UpdateAllowed(t);
      }
    }

    private static void Restock()
    {
      for (var i = 0; i < MarketTypes.Length; i++)
      {
        var t = MarketTypes[i];
        if (_stock[t] < StockMax)
        {
          _stock[t] = _stock[t] + 1;
        }

        UpdateAllowed(t);
      }
    }

    // the player may own (units alive + stock) of this type -> training stops when the stock is empty
    private static void UpdateAllowed(int t)
    {
      SetPlayerTechMaxAllowed(_p, t, GetPlayerUnitTypeCount(_p, t) + _stock[t]);
    }
  }
}
