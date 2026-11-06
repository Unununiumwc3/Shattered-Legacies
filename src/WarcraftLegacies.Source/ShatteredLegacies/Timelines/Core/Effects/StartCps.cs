using WarcraftLegacies.Source.Ai;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.60, a building block: control points a faction gets when it arrives (Sunfury: Area 52, Farahlon, Netherstorm;
  // the Kezan Enterprise: Kezan). Taken only from the neutrals (hostile or passive) -- a CP another player holds stays
  // his (he took it). Returns how many were given.
  internal static class StartCps
  {
    internal static int Give(player p, params int[] cpTypes)
    {
      var given = 0;
      for (var i = 0; i < cpTypes.Length; i++)
      {
        var cp = AiApi.SafeCp(cpTypes[i]);
        if (cp == null || cp.Unit == null)
        {
          continue;
        }

        var owner = GetOwningPlayer(cp.Unit);
        if (owner == p)
        {
          continue;
        }

        if (owner == Player(PLAYER_NEUTRAL_AGGRESSIVE) || owner == Player(PLAYER_NEUTRAL_PASSIVE))
        {
          AiApi.GiveUnit(cp.Unit, p);
          given++;
        }
      }

      return given;
    }
  }
}
