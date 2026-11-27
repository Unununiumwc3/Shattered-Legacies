using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Factions.Draenei;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Draenei
{
  // 0.80.0 (maintainer): Legion -> the DRAENEI (WL's "The Exodar"), who join whoever beat the Legion. The Draenei are no
  // pick of the Sentinels' slot any more (Setup/FactionChoiceDialogSetup.cs) -- a clean separation:
  // - map setup: the Exodar base (Regions.ExodarBaseUnlock) waits -- buildings visible and invulnerable, units hidden;
  //   Darkshore's draenei leftovers are cleaned up as in every Sentinels game before (the Sentinels' land)
  // - trigger: the Legion Stronghold lost (the catalog's Hq); its conqueror is recorded (Conquerors: death / capture)
  // - run: the Legion's units are left behind, the Exodar and its base are theirs at once ("A New Home" completed:
  //   Nobundo), Velen leads them, 5 Gemcrafters and a small guard; no Darkshore quest; they JOIN the conqueror's team
  //   (none: a team of their own). Lore: the announcement differs for a good, an evil and a Fel Horde conqueror.
  // 0.87.0 (maintainer): the base is the INSIDE OF THE EXODAR, in the void (ExodarShip): the ship lands near the home of
  // whoever beat the Legion and is the only way in or out; "The Dimensional Ship" lets it fly. CONQUERABLE: whoever
  // takes the Exodar's bridge (its CP) gains the draenei's service. They can still be defeated.
  internal static class DraeneiFallback
  {
    private const float StartX = -26432f; // inside the ship (0.86: -22592, 7008 on Azuremyst)
    private const float StartY = 14176f;

    internal const string ConquestText =
      "|cffffcc00The Exodar's bridge has fallen.|r Velen bows to the ship's new masters -- the draenei serve them now.";
    internal static readonly ExclusivePath Path = new("Draenei", Prepare, Run, StartX, StartY);
    private static List<unit> _exodar = new();

    private static readonly int[] Guard =
    {
      UNIT_O05A_GEMCRAFTER_DRAENEI_WORKER, UNIT_O05A_GEMCRAFTER_DRAENEI_WORKER, UNIT_O05A_GEMCRAFTER_DRAENEI_WORKER,
      UNIT_O05A_GEMCRAFTER_DRAENEI_WORKER, UNIT_O05A_GEMCRAFTER_DRAENEI_WORKER,
      UNIT_O05B_DEFENDER_DRAENEI, UNIT_O05B_DEFENDER_DRAENEI, UNIT_O05B_DEFENDER_DRAENEI, UNIT_O05B_DEFENDER_DRAENEI,
      UNIT_O05B_DEFENDER_DRAENEI, UNIT_O05B_DEFENDER_DRAENEI, UNIT_H09T_RANGARI_DRAENEI, UNIT_H09T_RANGARI_DRAENEI,
      UNIT_H09T_RANGARI_DRAENEI, UNIT_H09T_RANGARI_DRAENEI, UNIT_O05C_LUMINARCH_DRAENEI, UNIT_O05C_LUMINARCH_DRAENEI
    };

    internal const string GoodText =
      "|cffffcc00The Burning Legion is broken.|r For twenty-five thousand years the draenei have fled it. Now the "
      + "Exodar's crystals blaze to life on Azuremyst, and Prophet Velen swears his people to those who broke the Legion.";

    internal const string EvilText =
      "|cffffcc00The Burning Legion is broken.|r Velen has seen it in the Light of the naaru: the hand that broke the "
      + "Legion is the only one that can keep it broken. The draenei swallow their grief and swear a bitter oath to "
      + "their old enemies -- for now.";

    internal const string FelHordeText =
      "|cffffcc00The Burning Legion is broken -- by the Fel Horde.|r Velen remembers Shattrath burning. But the orcs who "
      + "slew their demon masters may be the ones his oldest vision promised: free of the blood pact, redeemable. The "
      + "draenei kneel to the Horde that once hunted them -- and pray the vision was true.";

    private static void Prepare()
    {
      _exodar = Regions.ExodarBaseUnlock.PrepareUnitsForRescue(RescuePreparationMode.HideNonStructures);
      Regions.Darkshore.CleanupNeutralPassiveUnits();
      if (AllPreplacedWidgets.Units.TryGetAll(UNIT_U00G_LEGION_STRONGHOLD_LEGION_OTHER, out var hq))
      {
        for (var i = 0; i < hq.Count; i++)
        {
          Conquerors.Watch(hq[i], "Legion");
        }
      }
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var victor = Conquerors.Of("Legion");
      var draenei = new DraeneiFaction(true);
      SwapFaction.Run(p, draenei);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      for (var i = 0; i < _exodar.Count; i++)
      {
        var u = _exodar[i];
        if (GetUnitTypeId(u) != 0 && GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_PASSIVE))
        {
          u.Rescue(p);
        }
      }

      if (draenei.RepairHull != null)
      {
        draenei.RepairHull.Progress = QuestProgress.Complete; // "A New Home": the Exodar is theirs, Nobundo trainable
      }

      StartCps.Force(p, UNIT_N0BL_EXODAR_REGALIS); // 0.81.0 (maintainer): theirs whoever holds it
      draenei.AddQuest(new QuestSurvivorsOfShattrath()); // 0.88.0: hold the bridge six turns -> Maraad
      AllLegends.Draenei.Velen.ForceCreate(p, new Point(StartX, StartY), 270f);
      Spawn.Units(p, StartX + 128f, StartY - 192f, Guard);
      var team = victor != null && GetPlayerId(victor) < 24 ? victor.GetPlayerData().Team : null;
      // 0.87.0: a vassal of the bridge -- the conqueror's team (none: their own), then whoever takes the Exodar's CP
      Conquest.Register(p, UNIT_N0BL_EXODAR_REGALIS, team != null ? victor : null, false, false, ConquestText);
      ExodarShip.LandNear(victor);
      var text = team == null || team == TeamSetup.Alliance || team == TeamSetup.NorthAlliance
                 || team == TeamSetup.SouthAlliance || team == TeamSetup.NightElves || team == TeamSetup.Horde
                 || team == TeamSetup.Kalimdor
        ? GoodText
        : victor.GetPlayerData().Faction?.Name == "Fel Horde" ? FelHordeText : EvilText;
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, text);
      }

      p.RepositionCamera(new Point(StartX, StartY));
      AiApi.LogPin("Legion fallback -> Draenei, team " + (p.GetPlayerData().Team != null ? p.GetPlayerData().Team.Name
        : "-") + (victor != null ? ", conqueror P" + I2S(GetPlayerId(victor)) : ""));
    }
  }
}
