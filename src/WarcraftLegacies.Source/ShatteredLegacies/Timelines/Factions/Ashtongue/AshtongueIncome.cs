using MacroTools.ControlPoints;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // 0.63 (maintainer): the Ashtongue's gold -- every Outland CP (Regions.InstanceOutland) held by their TEAM pays them
  // 10 gold per minute. Their allies keep their own CP income; the Ashtongue profit from defending them. Through WL's
  // BonusIncome (only the difference is applied), recounted every 5 s.
  internal static class AshtongueIncome
  {
    internal static player Owner;
    internal static int Income;

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      var t = CreateTimer();
      TimerStart(t, 5f, true, () => AiApi.Safe("ashtongue income", Update));
      Update();
    }

    private static void Update()
    {
      var team = Owner.GetPlayerData().Team;
      var held = 0;
      foreach (var cp in ControlPointManager.Instance.GetAllControlPoints())
      {
        var u = cp.Unit;
        if (u == null || !Regions.InstanceOutland.Contains(GetUnitX(u), GetUnitY(u)))
        {
          continue;
        }

        var owner = GetOwningPlayer(u);
        if (owner == Owner || (team != null && GetPlayerId(owner) < 24 && owner.GetPlayerData().Team == team))
        {
          held++;
        }
      }

      var want = held * AshtongueConfig.GoldPerOutlandCp;
      if (want != Income)
      {
        Owner.GetPlayerData().BonusIncome += want - Income;
        Income = want;
      }
    }
  }
}
