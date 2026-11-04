using MacroTools.Legends;
using WarcraftLegacies.Source.Ai;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // 0.55: Sylvanas of the Forsaken as a WL legendary hero (WL's death / revive rules, quest objectives on her level).
  // 0.56: one hero type per phase (WC3 cannot change which skills a hero learns): at a phase change WL's own
  // Legend.UnitType replaces her in place -- life, mana, XP (so her skill points come back to be spent anew) and her
  // items carried over; still selected. If she is dead, the change happens when she is revived. Ethereal Walk (AZs4): Berserk's speed + the code makes her ethereal and lets her walk through units.
  internal static class ForsakenSylvanas
  {
    internal static LegendaryHero Legend;
    internal static int Phase;
    private static int _pending;

    // map setup: registered like WL's legends (QuelthalasLegends)
    internal static void Setup()
    {
      Legend = new LegendaryHero("Sylvanas the Forsaken") { UnitType = ForsakenConfig.SylvanasBanshee };
      LegendaryHeroManager.Register(Legend);
      Legend.Revived += () => AiApi.Safe("sylvanas revived", () =>
      {
        if (_pending > Phase)
        {
          SetPhase(_pending);
        }
      });
      var cast = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(cast, Player(i), EVENT_PLAYER_UNIT_SPELL_EFFECT, null);
      }

      TriggerAddAction(cast, () =>
      {
        if (GetSpellAbilityId() == ForsakenConfig.EtherealWalk)
        {
          AiApi.Safe("ethereal walk", () => EtherealWalk(GetTriggerUnit()));
        }
      });
    }

    internal static void Spawn(player p, float x, float y)
    {
      Legend.ForceCreate(p, new Point(x, y), 270f);
      Phase = 1;
    }

    internal static void SetPhase(int phase)
    {
      var old = Legend.Unit;
      if (old == null)
      {
        return;
      }

      if (GetUnitState(old, UNIT_STATE_LIFE) <= 0.405f)
      {
        _pending = phase; // dead: changes when she is revived
        return;
      }

      _pending = 0;
      Phase = phase;
      var p = GetOwningPlayer(old);
      var selected = IsUnitSelected(old, p);
      Legend.UnitType = ForsakenConfig.PhaseHeroes[phase]; // WL's own replacement: life, mana, XP, items carried over
      var hero = Legend.Unit;
      if (hero == null)
      {
        return;
      }

      if (selected && GetLocalPlayer() == p)
      {
        SelectUnit(hero, true);
      }

      AiApi.LogPin("Sylvanas phase " + I2S(phase) + " (" + GetUnitName(hero) + ")");
    }

    private static void EtherealWalk(unit u)
    {
      UnitAddAbility(u, ForsakenConfig.Ethereal);
      SetUnitPathing(u, false);
      var t = CreateTimer();
      TimerStart(t, ForsakenConfig.EtherealWalkDuration, false, () =>
      {
        DestroyTimer(t);
        UnitRemoveAbility(u, ForsakenConfig.Ethereal);
        SetUnitPathing(u, true);
      });
    }
  }
}
