using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Factions.Sunfury;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Quelthalas
{
  // 0.53, the pilot of a FULL turning point: Quel'thalas -> WL's Sunfury faction (Timelines/SCENARIO_CONCEPTS.md P3).
  // Reuses what WL does when the Illidari slot picks Sunfury, now in the middle of the game:
  //   1. swap faction: a new SunfuryFaction (its quests come with it; the player keeps his colour)
  //   2. leave behind: Quel'thalas' units are removed, its CPs and capitals turn Neutral Hostile (WL's Scarlet pattern);
  //      Kael'thas and the Hourglass stay
  //   3. relocate: Sunfury's start area (Regions.SunfuryStartingPosition) is taken over like at a pick; 0.60: the CPs
  //      Area 52, Farahlon and Netherstorm join (if neutral)
  //   4. heroes: Kael'thas leads them (moved, revived or created)
  //   5. team: Outland while it has an undefeated member, otherwise own team (concepts: later the Horde)
  // Requires that Sunfury is no longer the Illidari slot's pick ([SL] in Setup/FactionChoiceDialogSetup.cs): its
  // Netherstorm areas must still be there. 0.53 fix: they wait hidden / invulnerable from the start (Prepare).
  internal static class QuelthalasFallback
  {
    private static List<unit> _startUnits = new();

    // map setup: Sunfury's start area hidden (taken over by the fallback), the areas of its quests (Tempest Keep, Area
    // 52, Upper Netherstorm) invulnerable and paused until Sunfury's quests take them over (they hide them, like WL).
    internal static void Prepare()
    {
      _startUnits = StartArea.Prepare(Regions.SunfuryStartingPosition);
      Regions.TempestKeep.PrepareUnitsForRescue(RescuePreparationMode.Invulnerable);
      Regions.Area52Unlock.PrepareUnitsForRescue(RescuePreparationMode.Invulnerable);
      Regions.UpperNetherstorm.PrepareUnitsForRescue(RescuePreparationMode.Invulnerable);
    }

    internal static void Run(player p, TurningPointDef def)
    {
      var kael = AllLegends.Sunfury.Kael;
      var keep = new List<unit>();
      var hourglass = Hourglass.Get(GetPlayerId(p));
      if (hourglass != null)
      {
        keep.Add(hourglass);
      }

      var kaelUnit = kael.Unit;
      var kaelIsOurs = kaelUnit != null && GetOwningPlayer(kaelUnit) == p;
      if (kaelIsOurs)
      {
        keep.Add(kaelUnit);
      }

      var sunfury = new SunfuryFaction();
      SwapFaction.Run(p, sunfury); // first: if it fails, the player keeps his units
      LeaveBehind.ToNeutralHostile(p, keep);
      var area = Regions.SunfuryStartingPosition;
      var taken = StartArea.Take(p, _startUnits, sunfury);
      // 0.60 (maintainer): Sunfury starts with Area 52, Farahlon and Netherstorm (if neutral -- StartCps)
      var cps = StartCps.Give(p, UNIT_N07Q_AREA_52, UNIT_N0CW_FARAHLON, UNIT_N02O_NETHERSTORM);
      // Kael'thas: ours (alive or dead) or nobody's -> he leads the survivors; another player's living Kael stays his
      if (kaelUnit == null || kaelIsOurs || GetUnitState(kaelUnit, UNIT_STATE_LIFE) <= 0.405f)
      {
        kael.ForceCreate(p, area.Center, 270f);
      }

      ChangeTeam.JoinOrAlone(p, TeamSetup.Outland);
      p.RepositionCamera(area.Center);
      AiApi.LogPin("Quel'thalas fallback -> Sunfury: " + I2S(taken) + " start units, " + I2S(cps) + " CPs, team "
                   + (p.GetPlayerData().Team != null ? p.GetPlayerData().Team.Name : "-"));
    }
  }
}
