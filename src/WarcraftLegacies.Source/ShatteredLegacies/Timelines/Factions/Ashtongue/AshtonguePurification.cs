using System.Collections.Generic;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Ashtongue
{
  // 0.63 (maintainer): Ishanah, High Priestess of the Aldor, purifies Outland from Shattrath. She cannot move until
  // the work is done (a leash: she is put back when she leaves her spot). Purify (AZt0) is an autocast, on from the
  // start -- bots too: while it is on she draws mana from own and ALLIED units within 1500 (up to 10 per second from
  // each, at most 20 in all; healing-light pillars and drain beams) and turns it into purification; 2 per second she
  // makes by herself. The sites in their order: Telredor 1000, Halaar 1250, Shattrath 1750, Oshu'gun 2000 -> 5
  // minutes with perfect mana. Each purified site completes its quest (AshtongueFaction): its tech unlocks units at the
  // temple. All four: Restoring Karabor to the Light (QuestRestoringKarabor). If she dies, she is back at her spot
  // after 60 s.
  internal static class AshtonguePurification
  {
    private const int OrderOn = 852150;  // faeriefireon (Purify, Faerie Fire based)
    private const int OrderOff = 852151; // faeriefireoff
    private const string Pillar = @"Abilities\Spells\Human\Resurrect\ResurrectTarget.mdl";

    internal static player Owner;
    internal static unit Ishanah;
    internal static bool AutoOn = true;
    internal static bool Done;
    internal static int Site;                         // the site being purified (0-3), 4 = all done
    internal static float Progress;                   // of the current site
    internal static List<ObjectiveCounter> Counters = new(); // per site (the quests)
    internal static float Clock;

    internal static void Start(player p)
    {
      if (Owner != null)
      {
        return;
      }

      Owner = p;
      Ishanah = CreateUnit(p, AshtongueObjects.Ishanah, AshtongueConfig.IshanahX, AshtongueConfig.IshanahY, 270f);
      SetUnitMoveSpeed(Ishanah, 0f);
      IssueImmediateOrderById(Ishanah, OrderOn);
      var ordered = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(ordered, p, EVENT_PLAYER_UNIT_ISSUED_ORDER, null);
      TriggerAddAction(ordered, () => AiApi.Safe("ashtongue purify toggle", OnOrder));
      var died = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(died, p, EVENT_PLAYER_UNIT_DEATH, null);
      TriggerAddAction(died, () => AiApi.Safe("ashtongue ishanah", OnDeath));
      var tick = CreateTimer();
      TimerStart(tick, 1f, true, () => AiApi.Safe("ashtongue purify", Tick));
      var leash = CreateTimer();
      TimerStart(leash, 0.5f, true, () => AiApi.Safe("ashtongue leash", Leash));
    }

    private static bool Alive(unit u) => u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;

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

    private static void OnDeath()
    {
      if (GetTriggerUnit() != Ishanah || Done)
      {
        return;
      }

      var t = CreateTimer();
      TimerStart(t, AshtongueConfig.IshanahReviveAfter, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("ashtongue ishanah revive", () =>
        {
          if (Alive(Ishanah) || GetUnitTypeId(Ishanah) == 0)
          {
            return;
          }

          ReviveHero(Ishanah, AshtongueConfig.IshanahX, AshtongueConfig.IshanahY, true);
          SetUnitMoveSpeed(Ishanah, 0f);
          IssueImmediateOrderById(Ishanah, AutoOn ? OrderOn : OrderOff);
        });
      });
    }

    // rooted until the work is done: put back on her spot when she wanders off (move speed 0 may not hold everywhere)
    private static void Leash()
    {
      if (Done || !Alive(Ishanah))
      {
        return;
      }

      var dx = GetUnitX(Ishanah) - AshtongueConfig.IshanahX;
      var dy = GetUnitY(Ishanah) - AshtongueConfig.IshanahY;
      if (dx * dx + dy * dy > 64f * 64f)
      {
        SetUnitPosition(Ishanah, AshtongueConfig.IshanahX, AshtongueConfig.IshanahY);
        IssueImmediateOrderById(Ishanah, ORDER_STOP);
      }
    }

    private static void Tick()
    {
      Clock += 1f;
      if (Done || Site >= AshtongueConfig.Sites.Length || !Alive(Ishanah))
      {
        return;
      }

      var gained = AshtongueConfig.PurifyTrickle;
      if (AutoOn)
      {
        gained += Drain();
      }

      Progress += gained;
      if (Site < Counters.Count)
      {
        Counters[Site].Add(R2I(gained));
      }

      if (Progress >= AshtongueConfig.SiteCost[Site])
      {
        Purified(Site);
        Progress = 0f;
        Site++;
      }
    }

    // mana of own and allied units within reach -> purification (at most DrainCap per second)
    private static float Drain()
    {
      var x = GetUnitX(Ishanah);
      var y = GetUnitY(Ishanah);
      var left = AshtongueConfig.DrainCap;
      var beams = 0;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, AshtongueConfig.PurifyRange, null);
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

        var take = mana < AshtongueConfig.DrainPerUnit ? mana : AshtongueConfig.DrainPerUnit;
        take = take < left ? take : left;
        SetUnitState(u, UNIT_STATE_MANA, mana - take);
        left -= take;
        if (beams < 4)
        {
          beams++;
          Beam(u);
        }
      }

      DestroyGroup(g);
      var drained = AshtongueConfig.DrainCap - left;
      if (drained > 0f && R2I(Clock) % 2 == 0)
      {
        // healing-light pillars around her, several at once
        for (var i = 0; i < 3; i++)
        {
          var a = GetRandomReal(0f, 6.2832f);
          var r = GetRandomReal(80f, 260f);
          DestroyEffect(AddSpecialEffect(Pillar, x + r * Cos(a), y + r * Sin(a)));
        }
      }

      return drained;
    }

    private static void Beam(unit from)
    {
      var l = AddLightningEx("DRAM", true, GetUnitX(from), GetUnitY(from), 60f, GetUnitX(Ishanah), GetUnitY(Ishanah),
        80f);
      var t = CreateTimer();
      TimerStart(t, 0.6f, false, () =>
      {
        DestroyTimer(t);
        DestroyLightning(l);
      });
    }

    private static void Purified(int site)
    {
      var name = AshtongueConfig.Sites[site];
      if (site < Counters.Count)
      {
        Counters[site].Add(AshtongueConfig.SiteCost[site]); // the quest completes (its counter rounds down)
      }

      for (var j = 0; j < 24; j++)
      {
        DisplayTimedTextToPlayer(Player(j), 0, 0, 12, "|cffffcc00" + name + " is purified|r -- the Light returns to Outland.");
      }

      AiApi.LogPin("Ashtongue: " + name + " purified");
    }

    // Karabor restored: Ishanah is free
    internal static void Release()
    {
      Done = true;
      if (Alive(Ishanah))
      {
        SetUnitMoveSpeed(Ishanah, GetUnitDefaultMoveSpeed(Ishanah));
      }
    }
  }
}
