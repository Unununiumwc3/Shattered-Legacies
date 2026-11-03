using MacroTools.Extensions;
using MacroTools.Factions;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // Effect "change team". Ground rule "living target": a team is only joined while it has an undefeated member;
  // otherwise the faction stands alone (WL's Unally: a new team "<faction> Pact").
  internal static class ChangeTeam
  {
    internal static void JoinOrAlone(player p, Team team)
    {
      var data = p.GetPlayerData();
      if (team != null && team.UndefeatedPlayerCount > 0)
      {
        if (data.Team != team)
        {
          data.SetTeam(team);
        }

        return;
      }

      data.Faction?.Unally();
    }
  }
}
