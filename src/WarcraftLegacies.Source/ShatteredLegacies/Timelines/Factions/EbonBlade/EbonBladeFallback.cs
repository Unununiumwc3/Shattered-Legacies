using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Legends;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Rules;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.EbonBlade
{
  // 0.80.0 (maintainer): Scourge -> the Knights of the Ebon Blade.
  // - Trigger: the Frozen Throne lost (WL ruptures it when it is taken: "CAPITAL DAMAGED") AND, if Arthas became the
  //   Lich King, the Lich King permanently dead (LichKingGone, the turning point's Prerequisite).
  // - The rest of the Scourge turns Neutral Hostile, its heroes leave.
  // - Acherus: The Ebon Hold (uZe0, indestructible) rises at AcherusX/Y near Icecrown, the four death knights beside it
  //   (level 6). Hero-only: Acherus revives them and gives the food.
  // - A vassal of whoever holds Icecrown Citadel's CP (n04R) -- at the start its holder, else the one who broke the
  //   throne (the last enemy to damage it) --, never defeated.
  internal static class EbonBladeFallback
  {
    internal const float AcherusX = -2081f; // open ground south-east of Icecrown Citadel (map_query: walkable, buildable)
    internal const float AcherusY = 18418f;
    private const float HeroX = -1629f;
    private const float HeroY = 18419f;
    internal const int Acherus = 1968858416;                   // uZe0
    internal static readonly int[] Heroes = { 1431987505, 1431987506, 1431987507, 1431987508 }; // UZe1..UZe4

    internal static readonly ExclusivePath Path = new("Knights of the Ebon Blade", Prepare, Run, AcherusX, AcherusY);
    private static bool _lichKingDead;
    private static player _breaker;

    // the Lich King, if he ever rose, must be dead for good
    internal static bool LichKingGone(player p)
    {
      var arthas = AllLegends.Scourge.Arthas;
      return arthas.UnitType != UNIT_N023_LORD_OF_THE_SCOURGE_SCOURGE || _lichKingDead;
    }

    private static void Prepare()
    {
      AllLegends.Scourge.Arthas.Died += e =>
      {
        if (e.Permanent && e.LegendaryHero.UnitType == UNIT_N023_LORD_OF_THE_SCOURGE_SCOURGE)
        {
          _lichKingDead = true;
        }
      };
      var throne = AllLegends.Scourge.TheFrozenThrone.Unit;
      if (throne == null)
      {
        return;
      }

      var hit = CreateTrigger();
      TriggerRegisterUnitEvent(hit, throne, EVENT_UNIT_DAMAGED);
      TriggerAddAction(hit, () =>
      {
        var src = GetEventDamageSource();
        if (src == null)
        {
          return;
        }

        var who = GetOwningPlayer(src);
        if (GetPlayerId(who) < 24 && who != GetOwningPlayer(throne) && IsPlayerEnemy(who, GetOwningPlayer(throne)))
        {
          _breaker = who;
        }
      });
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var blade = new EbonBladeFaction();
      SwapFaction.Run(p, blade);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      var hold = CreateUnit(p, Acherus, AcherusX, AcherusY, 270f);
      SetUnitInvulnerable(hold, true);
      AcherusFlight.Start(p, hold); // 0.88.0: the four at level 10 -> "The Ebon Hold Rises", Acherus flies
      for (var i = 0; i < Heroes.Length; i++)
      {
        var h = CreateUnit(p, Heroes[i], HeroX + 128f * (i % 2), HeroY - 128f * (i / 2), 270f);
        SetHeroLevel(h, 6, false);
      }

      var cp = AiApi.SafeCp(UNIT_N04R_ICECROWN_CITADEL);
      var holder = cp != null && cp.Unit != null ? GetOwningPlayer(cp.Unit) : null;
      var overlord = holder != null && GetPlayerId(holder) < 24 && holder != p ? holder : _breaker;
      Conquest.Register(p, UNIT_N04R_ICECROWN_CITADEL, overlord, true);
      p.RepositionCamera(new Point(AcherusX, AcherusY));
      AiApi.LogPin("Scourge fallback -> Ebon Blade, overlord "
                   + (overlord != null ? "P" + I2S(GetPlayerId(overlord)) : "none (own team)"));
    }
  }
}
