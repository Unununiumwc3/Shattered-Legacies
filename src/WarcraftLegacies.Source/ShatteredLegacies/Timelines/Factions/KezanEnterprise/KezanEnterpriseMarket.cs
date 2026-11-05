using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.39.8: the Kezan Trading Center (o04M, preplaced on Kezan, the main map's) is a creep market (maintainer's
  // design). Each creep type has a stock of 3; one comes back every 2 min (6 min for a full stock). Done by code: the
  // type's max-allowed = units alive + stock. 0.58: the oil income moved to KezanEnterpriseOil (was
  // KezanEnterpriseMechanics).
  internal static class KezanEnterpriseMarket
  {
    private const int StockMax = 3;
    private const float RestockEvery = 120f;

    internal static int[] MarketTypes =
    {
      FourCC("nogr"), FourCC("nomg"), FourCC("nftr"), FourCC("nfsp"), FourCC("nmrr"), FourCC("ncen"),
    };

    internal static player Owner;
    internal static Dictionary<int, int> Stock = new();

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      for (var i = 0; i < MarketTypes.Length; i++)
      {
        Stock[MarketTypes[i]] = StockMax;
        UpdateAllowed(MarketTypes[i]);
      }

      var restock = CreateTimer();
      TimerStart(restock, RestockEvery, true, () => AiApi.Safe("kezan market", Restock));
      var trained = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(trained, p, EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      TriggerAddAction(trained, OnTrained);
    }

    private static void OnTrained()
    {
      var t = GetUnitTypeId(GetTrainedUnit());
      if (Stock.ContainsKey(t))
      {
        var left = Stock[t] > 0 ? Stock[t] - 1 : 0;
        Stock[t] = left;
        UpdateAllowed(t);
      }
    }

    private static void Restock()
    {
      for (var i = 0; i < MarketTypes.Length; i++)
      {
        var t = MarketTypes[i];
        if (Stock[t] < StockMax)
        {
          var more = Stock[t] + 1;
          Stock[t] = more;
        }

        UpdateAllowed(t);
      }
    }

    // the player may own (units alive + stock) of this type -> training stops when the stock is empty
    private static void UpdateAllowed(int t)
    {
      SetPlayerTechMaxAllowed(Owner, t, GetPlayerUnitTypeCount(Owner, t) + Stock[t]);
    }
  }
}
