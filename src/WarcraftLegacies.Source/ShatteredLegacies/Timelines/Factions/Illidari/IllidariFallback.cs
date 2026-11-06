using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Illidari
{
  // Illidari -> the Ashtongue Deathsworn (0.63, maintainer's design; concepts P16). Trigger: the Black Temple (o00F)
  // destroyed. Effects, in this order:
  //   1. swap faction: a new AshtongueFaction (keeps the player's colour)
  //   2. hero fate: Illidan is gone for good (Black Temple, TBC)
  //   3. leave behind: everything but the draenei (Akama, Deathsworn, Seers, Draenei Huts) and the Hourglass is removed
  //      or turns Neutral Hostile -- no lore heir
  //   4. relocate: the Temple of the Deathsworn rises where the Black Temple stood (indestructible, the production
  //      centre); 4 Defenders next to it
  //   5. the sites: Shattrath, Halaar, Oshu'gun (preplaced, whoever holds them) and the 24p's Telredor (placed at
  //      Zangarmarsh) become the Ashtongue's and indestructible -- fel-tainted until purified
  //   6. Akama: kept (the same unit -- level and skills), revived at the temple if dead, created if he never was
  //   7. team: who destroyed the Black Temple -- then whoever holds its CP (Conquest; seat n00I)
  //   8. mechanics: Ishanah and the purification (AshtonguePurification), 10 gold per Outland CP of the team
  //      (AshtongueIncome); an announcement to everyone
  internal static class IllidariFallback
  {
    internal static player Destroyer; // who dealt the Black Temple its death blow

    // map setup: remember who destroys the Black Temple (the fallback runs later, after the decision)
    internal static void Prepare()
    {
      var temple = AllLegends.FelHorde.BlackTemple.Unit;
      if (temple == null)
      {
        return;
      }

      var death = CreateTrigger();
      TriggerRegisterUnitEvent(death, temple, EVENT_UNIT_DEATH);
      TriggerAddAction(death, () =>
      {
        var killer = GetKillingUnit();
        if (killer != null)
        {
          Destroyer = GetOwningPlayer(killer);
        }
      });
    }

    internal static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var hourglass = Hourglass.Get(GetPlayerId(p));
      if (hourglass != null)
      {
        keep.Add(hourglass);
      }

      var akama = AllLegends.Naga.Akama;
      var akamaUnit = akama.Unit;
      var akamaOurs = akamaUnit != null && GetOwningPlayer(akamaUnit) == p;
      KeepDraenei(p, keep);

      SwapFaction.Run(p, new AshtongueFaction()); // first: if it fails, the player keeps his units
      var illidan = AllLegends.Naga.Illidan;
      if (illidan.Unit != null && GetOwningPlayer(illidan.Unit) == p)
      {
        illidan.PermanentlyKill();
      }

      LeaveBehind.ToNeutralHostile(p, keep);
      var temple = CreateUnit(p, AshtongueConfig.TempleOfTheDeathsworn, AshtongueConfig.TempleX,
        AshtongueConfig.TempleY, 270f);
      SetUnitInvulnerable(temple, true);
      Spawn.Units(p, AshtongueConfig.UnitsX, AshtongueConfig.UnitsY, AshtongueConfig.StartUnits);
      var sites = TakeSites(p);

      var start = new Point(AshtongueConfig.UnitsX, AshtongueConfig.UnitsY);
      if (akamaOurs && GetUnitState(akamaUnit, UNIT_STATE_LIFE) > 0.405f)
      {
        SetUnitPosition(akamaUnit, AshtongueConfig.UnitsX, AshtongueConfig.UnitsY);
      }
      else if (akamaOurs)
      {
        ReviveHero(akamaUnit, AshtongueConfig.UnitsX, AshtongueConfig.UnitsY, true); // dead: his level stays
      }
      else if (akamaUnit == null)
      {
        akama.ForceCreate(p, start, 270f); // never trained
      }

      Conquest.Register(p, UNIT_N00I_BLACK_TEMPLE, Destroyer);
      AshtonguePurification.Start(p);
      AshtongueIncome.Start(p);
      p.RepositionCamera(start);
      AiApi.LogPin("Illidari fallback -> Ashtongue: " + I2S(sites) + " sites, destroyer "
                   + (Destroyer != null ? GetPlayerName(Destroyer) : "none"));
    }

    // the Illidari's draenei stay with the player: Akama, Deathsworn, Seers, Draenei Huts
    private static void KeepDraenei(player p, List<unit> keep)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if (t == UNIT_NAKA_ELDER_SAGE_ILLIDARI || t == UNIT_NDRN_DEATHSWORN_ILLIDARI || t == UNIT_NDRS_SEER_ILLIDARI
            || t == UNIT_N08W_DRAENEI_HUT_ILLIDARI_SPECIALIST)
        {
          keep.Add(u);
        }
      }

      DestroyGroup(g);
    }

    // Shattrath, Halaar, Oshu'gun (whoever holds them) and the 24p Telredor: the Ashtongue's, indestructible
    private static int TakeSites(player p)
    {
      var taken = 0;
      var types = new[] { UNIT_H0AH_SHATTRAH_DRAENEI_OTHER, UNIT_H0AE_HALAAR_DRAENEI_OTHER, UNIT_H02Z_OSHU_GUN_CREEP };
      for (var i = 0; i < types.Length; i++)
      {
        unit u = null;
        try
        {
          u = AllPreplacedWidgets.Units.GetClosest(types[i], AshtongueConfig.IshanahX, AshtongueConfig.IshanahY);
        }
        catch (System.Exception)
        {
          AiApi.Log("Illidari fallback: no preplaced site of type " + I2S(types[i]));
        }

        if (u == null || GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
        {
          continue;
        }

        SetUnitOwner(u, p, true);
        SetUnitInvulnerable(u, true);
        taken++;
      }

      if (AiApi.FindBuildSpot(AshtongueConfig.TelredorX, AshtongueConfig.TelredorY, new List<float>(),
            new List<float>(), out var x, out var y))
      {
        SetUnitInvulnerable(CreateUnit(p, AshtongueObjects.Telredor, x, y, 270f), true);
        taken++;
      }

      return taken;
    }
  }
}
