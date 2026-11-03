using MacroTools.Shared;
using static MacroTools.Shared.UnitCategory;

namespace WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits
{
  // 0.54: object limits of the Forsaken (Shattered Legacies; the pattern of WL's FactionObjectLimits). PLACEHOLDER
  // tech: the Scourge's basics, until 0.55 brings the 24p Forsaken tree. Registered in ObjectInfoRepository ([SL]).
  public static class ForsakenObjectInfo
  {
    private const int Unlimited = 200;
    private const int SylvanasBanshee = 1431986481; // UZa1 (no generated constant: the fork does not regenerate Units.g.cs)

    public static IEnumerable<ObjectInfo> GetAllObjectLimits()
    {
      yield return new(UNIT_UNPL_NECROPOLIS_SCOURGE_T1, Unlimited, TownHall);
      yield return new(UNIT_UZIG_ZIGGURAT_SCOURGE_FARM, Unlimited, Farm);
      yield return new(UNIT_UAOD_ALTAR_OF_DARKNESS_SCOURGE_ALTAR, Unlimited, Altar);
      yield return new(UNIT_USEP_CRYPT_SCOURGE_BARRACKS, Unlimited, Barracks);
      yield return new(UNIT_UACO_ACOLYTE_SCOURGE_WORKER, Unlimited, Builder);
      yield return new(UNIT_UGHO_GHOUL_SCOURGE, Unlimited, new List<UnitCategory> { Fighter, Builder });
      yield return new(UNIT_UBAN_BANSHEE_SCOURGE, Unlimited, Support);
      yield return new(UNIT_UABO_ABOMINATION_SCOURGE, Unlimited, Tank);
      yield return new(SylvanasBanshee, 1, new List<UnitCategory> { Marksman, Support });
    }
  }
}
