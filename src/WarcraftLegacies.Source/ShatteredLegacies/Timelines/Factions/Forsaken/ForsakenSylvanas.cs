using MacroTools.Legends;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.55: Sylvanas of the Forsaken as a WL legendary hero (WL's death / revive rules, quest objectives on her level)
  // and her phases: a new title (unit name) and, for the Dark Ranger, the stock Dark Ranger model (BlzSetUnitSkin --
  // the same unit, so level, items and abilities stay). Phase 3 keeps the Dark Ranger model.
  internal static class ForsakenSylvanas
  {
    internal static LegendaryHero Legend;
    internal static int Phase;

    // map setup: registered like WL's legends (QuelthalasLegends)
    internal static void Setup()
    {
      Legend = new LegendaryHero("Sylvanas the Forsaken") { UnitType = ForsakenConfig.SylvanasBanshee };
      LegendaryHeroManager.Register(Legend);
    }

    internal static void Spawn(player p, float x, float y)
    {
      Legend.ForceCreate(p, new Point(x, y), 270f);
      SetPhase(1);
    }

    internal static void SetPhase(int phase)
    {
      Phase = phase;
      var u = Legend.Unit;
      if (u == null)
      {
        return;
      }

      BlzSetUnitName(u, ForsakenConfig.PhaseTitles[phase]);
      if (phase >= 2)
      {
        BlzSetUnitSkin(u, ForsakenConfig.DarkRangerSkin);
      }
    }
  }
}
