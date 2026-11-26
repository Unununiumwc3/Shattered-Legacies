using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // 0.79.0 (maintainer: "the Kezan bot does not invest in its oil economy -- tech it at the speed of the caster
  // research, free for bots"). A bot Enterprise (with perks) gets its oil at the pace of the bot's caster research:
  // 2 steps on arrival, one more every 2 minutes and one per CP it captures after arriving. The steps, in order:
  // Oil Rig, Oil Tanker, Oil Rig, Oil Tanker, Oil Refining 1, ... (Plan) -- up to 8 rigs, 10 tankers, Oil Refining 2.
  // Everything is FREE for the bot (AiApi.FreeOrder: trained at the Oil Supply / the Trading Center; Oil Refining is
  // researched directly). One order per kind every 20 s; the rest (pumping, tankers, refining) is the oil's own code.
  internal static class KezanEnterpriseBot
  {
    private const float Every = 20f;
    private const int MaxTankers = 10;            // KezanEnterpriseTrade: at most 10 Oil Tankers
    private const int R = 0;                      // plan entries
    private const int T = 1;
    private const int F = 2;
    private static readonly int[] Plan = { R, T, R, T, F, R, T, R, T, R, T, F, R, T, R, T, R, T, T, T };

    private static player _p;
    private const float Pending = 90f;            // an order counts as under way this long (training time + margin)
    private static float _rigOrdered = -1000f;
    private static float _tankerOrdered = -1000f;
    private static int _capsAtStart;
    private static float _clock;
    internal static int Rigs;
    internal static int Tankers;

    internal static void Start(player p)
    {
      if (_p != null)
      {
        return;
      }

      _p = p;
      _capsAtStart = AiApi.CpCaptures(p);
      var t = CreateTimer();
      TimerStart(t, Every, true, () => AiApi.Safe("kezan oil bot", Tick));
    }

    private static void Tick()
    {
      _clock += Every;
      if (!AiApi.IsBot(_p) || !AiApi.HasPerks(_p) || _p.GetPlayerData().Faction == null
          || _p.GetPlayerData().Faction.Name != KezanEnterpriseConfig.Name)
      {
        return;
      }

      var steps = 2 + R2I(_clock / 120f) + AiApi.CpCaptures(_p) - _capsAtStart;
      var wantRigs = 0;
      var wantTankers = 0;
      var wantRefining = 0;
      for (var i = 0; i < steps && i < Plan.Length; i++)
      {
        if (Plan[i] == R)
        {
          wantRigs++;
        }
        else if (Plan[i] == T)
        {
          wantTankers++;
        }
        else
        {
          wantRefining++;
        }
      }

      if (wantRigs > KezanEnterpriseConfig.MaxOilPlatforms)
      {
        wantRigs = KezanEnterpriseConfig.MaxOilPlatforms;
      }

      if (wantTankers > MaxTankers)
      {
        wantTankers = MaxTankers;
      }

      if (wantRefining > 2)
      {
        wantRefining = 2;
      }

      if (GetPlayerTechCount(_p, KezanEnterpriseConfig.OilRefining, true) < wantRefining)
      {
        SetPlayerTechResearched(_p, KezanEnterpriseConfig.OilRefining, wantRefining);
      }

      // what stands / sails already: platforms + expeditions under way (their hidden rig) + rigs in training
      // (one order of each kind under way at a time: an order counts for Pending seconds)
      var rigs = KezanEnterpriseOil.Platforms.Count + Count(KezanEnterpriseObjects.OilRig);
      if (rigs < wantRigs && _clock - _rigOrdered >= Pending)
      {
        var supply = KezanEnterpriseOil.Supply();
        if (supply != null && AiApi.FreeOrder(_p, supply, KezanEnterpriseObjects.OilRig))
        {
          Rigs++;
          _rigOrdered = _clock;
          AiApi.Log("kezan oil bot: Oil Rig (free), step " + I2S(steps) + ", rigs " + I2S(rigs) + "/" + I2S(wantRigs));
        }
      }

      var tankers = Count(KezanEnterpriseObjects.OilTanker);
      if (tankers < wantTankers && _clock - _tankerOrdered >= Pending)
      {
        var center = First(KezanEnterpriseObjects.TradingCenter);
        if (center != null && AiApi.FreeOrder(_p, center, KezanEnterpriseObjects.OilTanker))
        {
          Tankers++;
          _tankerOrdered = _clock;
          AiApi.Log("kezan oil bot: Oil Tanker (free), tankers " + I2S(tankers) + "/" + I2S(wantTankers));
        }
      }
    }

    // units of this type the player has, alive (a rig under way is a hidden, paused unit -- counted too)
    private static int Count(int type)
    {
      var n = 0;
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, _p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetUnitTypeId(u) == type && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          n++;
        }
      }

      DestroyGroup(g);
      return n;
    }

    private static unit First(int type)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, _p, null);
      unit found = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetUnitTypeId(u) == type && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          found = u;
        }
      }

      DestroyGroup(g);
      return found;
    }

  }
}
