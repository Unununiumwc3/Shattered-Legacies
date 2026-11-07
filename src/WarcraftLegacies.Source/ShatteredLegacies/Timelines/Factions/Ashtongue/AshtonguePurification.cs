using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // The purification of Outland (0.63; 0.64 reworked, maintainer). SHATTRATH is the node:
  // - donations: every 8 s Shattrath draws up to 50 mana from each own / ALLIED unit within 1500 (at most 160 in all --
  //   many units at once, slower; bots running past give too): a mana-burn beam from Shattrath to each donor. Plus 2
  //   per second by itself. Only while Ishanah's Purify (autocast, on from the start) is on.
  // - progress: Shattrath's mana pool IS the purification of the current step (no regeneration; its maximum = the
  //   step's cost). Full -> the step is done, the pool starts again for the next one.
  // - the steps, in this order: Shattrath, Halaa, Telredor, Oshu'gun, then the temple itself (Karabor). A finished
  //   step replaces the site's fel-tainted building by its purified one (it trains and researches from then on).
  // - while mana flows, the current site shows: the Obsidian Statue's replenish (life + mana) at the size of the
  //   bubble, a shield bubble (not at Shattrath: it has its own; gone when the step is done) and a ring of healing-light
  //   pillars. No mana flowing, no effects.
  // - Ishanah stands next to Shattrath: she cannot move (no move ability) and cannot be hurt until Karabor is
  //   restored; then she is replaced by a free Ishanah (level, skills, items kept) without Purify.
  internal static class AshtonguePurification
  {
    private const int OrderOn = 852150;  // faeriefireon (Purify, Faerie Fire based)
    private const int OrderOff = 852151; // faeriefireoff
    private const string Pillar = @"Abilities\Spells\Human\Resurrect\ResurrectTarget.mdl";
    private const string Bubble = @"Abilities\Spells\Human\DivineShield\DivineShieldTarget.mdl";
    private const string StatueLife = @"Abilities\Spells\Undead\ReplenishHealth\ReplenishHealthCaster.mdl";
    private const string StatueMana = @"Abilities\Spells\Undead\ReplenishMana\ReplenishManaCaster.mdl";
    private const string BurnTarget = @"Abilities\Spells\NightElf\ManaBurn\ManaBurnTarget.mdl";

    internal static player Owner;
    internal static unit Ishanah;
    internal static bool AutoOn = true;
    internal static bool Done;
    internal static int Step;                         // 0-4, 5 = all done
    internal static float Clock;
    internal static float LastFlow = -100f;           // when mana last came in (effects only while it flows)
    internal static List<unit> Sites = new();         // per step: its building now (tainted, then purified)
    internal static List<ObjectiveCounter> Counters = new(); // per step (the quests)
    internal static effect BubbleFx;

    internal static void Start(player p, List<unit> sites)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      Sites = sites;
      Ishanah = CreateUnit(p, AshtongueObjects.Ishanah, AshtongueConfig.IshanahX, AshtongueConfig.IshanahY, 270f);
      UnitRemoveAbility(Ishanah, FourCC("Amov")); // rooted (the 0.63 leash let her slide)
      SetUnitInvulnerable(Ishanah, true);
      IssueImmediateOrderById(Ishanah, OrderOn);
      SetPool();
      var ordered = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(ordered, p, EVENT_PLAYER_UNIT_ISSUED_ORDER, null);
      TriggerAddAction(ordered, () => AiApi.Safe("ashtongue purify toggle", OnOrder));
      var tick = CreateTimer();
      TimerStart(tick, 1f, true, () => AiApi.Safe("ashtongue purify", Tick));
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

    private static unit Shattrath => Sites.Count > 0 ? Sites[0] : null;

    private static void OnOrder()
    {
      if (GetTriggerUnit() != Ishanah)
      {
        return;
      }

      var order = GetIssuedOrderId();
      if (order == OrderOn)
      {
        AutoOn = true;
      }
      else if (order == OrderOff)
      {
        AutoOn = false;
      }
    }

    // Shattrath's pool: maximum = the current step's cost
    private static void SetPool()
    {
      if (Alive(Shattrath) && Step < AshtongueConfig.StepCost.Length)
      {
        BlzSetUnitMaxMana(Shattrath, AshtongueConfig.StepCost[Step]);
        SetUnitState(Shattrath, UNIT_STATE_MANA, 0f);
      }
    }

    private static void Tick()
    {
      Clock += 1f;
      if (Done || Step >= AshtongueConfig.StepCost.Length || !Alive(Shattrath))
      {
        return;
      }

      var gained = AshtongueConfig.Trickle;
      if (AutoOn && R2I(Clock) % R2I(AshtongueConfig.DonationEvery) == 0)
      {
        var donated = Donations();
        if (donated > 0f)
        {
          LastFlow = Clock;
        }

        gained += donated;
      }

      var pool = GetUnitState(Shattrath, UNIT_STATE_MANA) + gained;
      SetUnitState(Shattrath, UNIT_STATE_MANA, pool);
      if (Step < Counters.Count)
      {
        Counters[Step].Add(R2I(gained));
      }

      Effects();
      if (pool >= AshtongueConfig.StepCost[Step] - 0.5f)
      {
        Finish(Step);
        Step++;
        SetPool();
        if (Step >= AshtongueConfig.StepCost.Length)
        {
          Release();
        }
      }
    }

    // own and allied units within reach give up to 50 each, 160 in all -- mana-burn beams from Shattrath
    private static float Donations()
    {
      var sx = GetUnitX(Shattrath);
      var sy = GetUnitY(Shattrath);
      var left = AshtongueConfig.DonationCap;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, sx, sy, AshtongueConfig.DonationRange, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (left <= 0f || u == Ishanah || IsUnitType(u, UNIT_TYPE_STRUCTURE) || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f
            || !(GetOwningPlayer(u) == Owner || IsUnitAlly(u, Owner)))
        {
          continue;
        }

        var mana = GetUnitState(u, UNIT_STATE_MANA);
        if (mana < 1f)
        {
          continue;
        }

        var take = mana < AshtongueConfig.DonationPerUnit ? mana : AshtongueConfig.DonationPerUnit;
        take = take < left ? take : left;
        SetUnitState(u, UNIT_STATE_MANA, mana - take);
        left -= take;
        Beam(Shattrath, u);
      }

      DestroyGroup(g);
      return AshtongueConfig.DonationCap - left;
    }

    // a mana-burn beam from Shattrath to the donor -- heights from the units themselves (0.63's beams missed: fixed
    // heights ignored the terrain)
    private static void Beam(unit from, unit to)
    {
      var l = AddLightningEx("MBUR", true, GetUnitX(from), GetUnitY(from), BlzGetUnitZ(from) + 200f, GetUnitX(to),
        GetUnitY(to), BlzGetUnitZ(to) + 60f);
      DestroyEffect(AddSpecialEffectTarget(BurnTarget, to, "origin"));
      var t = CreateTimer();
      TimerStart(t, 1.2f, false, () =>
      {
        DestroyTimer(t);
        DestroyLightning(l);
      });
    }

    // while mana flows (a donation in the last 9 s): the current site lights up
    private static void Effects()
    {
      var site = Step < Sites.Count ? Sites[Step] : null;
      var flowing = Clock - LastFlow <= AshtongueConfig.DonationEvery + 1f;
      if (!flowing || !Alive(site))
      {
        DropBubble();
        return;
      }

      var x = GetUnitX(site);
      var y = GetUnitY(site);
      var scale = Step == 0 ? 4f : AshtongueConfig.BubbleScale[Step];
      if (Step > 0 && BubbleFx == null)
      {
        BubbleFx = AddSpecialEffect(Bubble, x, y);
        BlzSetSpecialEffectScale(BubbleFx, AshtongueConfig.BubbleScale[Step]);
      }

      if (R2I(Clock) % 2 == 0)
      {
        // a ring of healing-light pillars at the bubble's size (at least 10)
        var radius = 75f * scale;
        for (var i = 0; i < 12; i++)
        {
          var a = 6.2832f * i / 12f + GetRandomReal(0f, 0.3f);
          DestroyEffect(AddSpecialEffect(Pillar, x + radius * Cos(a), y + radius * Sin(a)));
        }
      }

      if (R2I(Clock) % 4 == 0)
      {
        var life = AddSpecialEffect(StatueLife, x, y);
        BlzSetSpecialEffectScale(life, scale);
        DestroyEffect(life);
        var mana = AddSpecialEffect(StatueMana, x, y);
        BlzSetSpecialEffectScale(mana, scale);
        DestroyEffect(mana);
      }
    }

    private static void DropBubble()
    {
      if (BubbleFx != null)
      {
        DestroyEffect(BubbleFx);
        BubbleFx = null;
      }
    }

    // a step done: the site's tainted building -> its purified one (Shattrath keeps being the pool)
    private static void Finish(int step)
    {
      DropBubble();
      var name = AshtongueConfig.Steps[step];
      if (step < Counters.Count)
      {
        Counters[step].Add(AshtongueConfig.StepCost[step]); // the quest completes (its counter rounds down)
      }

      var old = step < Sites.Count ? Sites[step] : null;
      if (old != null && GetUnitTypeId(old) != 0)
      {
        var x = GetUnitX(old);
        var y = GetUnitY(old);
        var facing = GetUnitFacing(old);
        RemoveUnit(old);
        var n = CreateUnit(Owner, AshtongueConfig.Purified[step], x, y, facing);
        SetUnitInvulnerable(n, true);
        Sites[step] = n;
        if (step == 0 && Step + 1 < AshtongueConfig.StepCost.Length)
        {
          BlzSetUnitMaxMana(n, AshtongueConfig.StepCost[step + 1]); // Shattrath stays the pool
        }

        for (var i = 0; i < 12; i++)
        {
          var a = 6.2832f * i / 12f;
          DestroyEffect(AddSpecialEffect(Pillar, x + 300f * Cos(a), y + 300f * Sin(a)));
        }
      }

      for (var j = 0; j < 24; j++)
      {
        DisplayTimedTextToPlayer(Player(j), 0, 0, 12, "|cffffcc00" + name + " is purified|r -- the Light returns to "
                                                      + "Outland.");
      }

      AiApi.LogPin("Ashtongue: " + name + " purified");
    }

    // Karabor restored: Ishanah is free -- a new Ishanah (move ability back), with her level, skills and items
    private static void Release()
    {
      Done = true;
      DropBubble();
      if (Ishanah == null || GetUnitTypeId(Ishanah) == 0)
      {
        return;
      }

      var old = Ishanah;
      var n = CreateUnit(Owner, AshtongueObjects.Ishanah, GetUnitX(old), GetUnitY(old), GetUnitFacing(old));
      SetHeroXP(n, GetHeroXP(old), false);
      int[] skills = { FourCC("AHhb"), FourCC("AHds"), FourCC("AHad"), FourCC("AHre") };
      for (var s = 0; s < skills.Length; s++)
      {
        var level = GetUnitAbilityLevel(old, skills[s]);
        for (var l = 0; l < level; l++)
        {
          SelectHeroSkill(n, skills[s]);
        }
      }

      for (var slot = 0; slot < 6; slot++)
      {
        var item = UnitItemInSlot(old, slot);
        if (item != null)
        {
          UnitAddItem(n, item);
        }
      }

      UnitRemoveAbility(n, AshtongueConfig.PurifyAbility); // its work is done
      RemoveUnit(old);
      Ishanah = n;
    }
  }
}
