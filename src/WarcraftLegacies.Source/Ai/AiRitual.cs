using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Factions.Legion.Quests;
using WarcraftLegacies.Source.Objectives.UnitBased;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.Ai
{
  // 0.49.0: the Legion is summoned ONLY the way WL does it -- a hero holding the Book of Medivh casts WL's ritual
  // "Summon the Burning Legion" (A00J, in the Book's spellbook; a 180-s channel at the hero's feet). WL's quest
  // "Under the Burning Sky" completes when the cast finishes and opens the Demon Portal WHERE THE CASTER STANDS.
  // Before (0.38-0.48): the bot completed the quest itself (Sunwell event, Book quest done, -legion), the portal came
  // up at (0,0) and was moved, and the Legion's army was teleported to Felwood.
  // - Who: a bot hero (real bot or test autopilot) of the Legion or the Scourge that carries the Book (item I006).
  //   It gets the Book through WL's "Book of Medivh" quest (AiConfig.HeroVisits sends a hero to the pedestal).
  // - The hero is kept out of army orders (AiState.MissionUnits) until the summon is done -- an order breaks the
  //   channel. Not enough mana for the ritual (410): the bot hero gets it (a bot perk, logged once).
  // - While it CHANNELS the ritual the hero is invulnerable (maintainer: only then). WL itself gives the caster spell
  //   immunity for the channel; the invulnerability is the bot's, for bot casters of the Legion / the Scourge only.
  internal static class AiRitual
  {
    private static int _ritualId;
    private static int _bookId;
    private static int _orderId;
    private static int _altOrderId;    // 0.50.1: "deathanddecay" -- A00J is based on Death and Decay
    private static int _refused;
    private static float _pauseUntil;
    private const float ManaCost = 410f;
    private const float RetryEvery = 10f;

    private static trigger _start;
    private static trigger _end;
    private static float _lastSearch = -100f;
    private static unit _caster;       // the hero we made invulnerable (channel running)
    private static bool _wasInvulnerable;
    private static unit _hero;         // the bot hero that holds the Book and is to cast
    private static float _lastOrder = -100f;
    private static bool _manaGiven;
    private static bool _done;
    private static QuestData _summonQuest;
    internal static int Casts;         // ritual orders given
    internal static int Channels;      // channels started by bot casters (invulnerable)
    internal static string State = "waiting for the Book";

    internal static void Setup()
    {
      _ritualId = FourCC("A00J");
      _bookId = FourCC("I006");
      _orderId = OrderId("volcano"); // A00J's order string in the object data (aord)
      // 0.50.1: WC3 uses the order string of the object data only for Channel-based abilities; A00J is a Death and Decay
      // (AUdd), so its real order is very likely "deathanddecay". 0.50.0 game: "volcano" refused 4 times, Arthas stood.
      _altOrderId = OrderId("deathanddecay");
      _start = CreateTrigger();
      _end = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_start, Player(i), EVENT_PLAYER_UNIT_SPELL_CHANNEL, null);
        TriggerRegisterPlayerUnitEvent(_end, Player(i), EVENT_PLAYER_UNIT_SPELL_ENDCAST, null);
      }

      TriggerAddAction(_start, () => OnSpell(true));
      TriggerAddAction(_end, () => OnSpell(false));
    }

    internal static bool Owns(unit u) => u != null && (u == _hero || u == _caster);

    // channel start: invulnerable (bot caster of the Legion / the Scourge); end / interrupt: back to how it was
    private static void OnSpell(bool start)
    {
      try
      {
        if (GetSpellAbilityId() != _ritualId)
        {
          return;
        }

        var u = GetTriggerUnit();
        if (start)
        {
          if (!Eligible(GetOwningPlayer(u)) || _caster != null)
          {
            return;
          }

          _caster = u;
          _wasInvulnerable = BlzIsUnitInvulnerable(u);
          SetUnitInvulnerable(u, true);
          Channels++;
          State = "channelling";
          SimpleBot.LogPin("P" + I2S(GetPlayerId(GetOwningPlayer(u))) + " " + SimpleBot.FacName(GetOwningPlayer(u))
                           + ": " + GetUnitName(u) + " channels the Legion ritual @" + I2S(R2I(GetUnitX(u))) + ","
                           + I2S(R2I(GetUnitY(u))) + " -- invulnerable while channelling");
        }
        else if (u == _caster)
        {
          EndInvulnerable("channel ended");
        }
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("TICK ERROR in legion ritual: " + e.Message);
      }
    }

    private static void EndInvulnerable(string why)
    {
      if (_caster == null)
      {
        return;
      }

      SetUnitInvulnerable(_caster, _wasInvulnerable);
      SimpleBot.Log(GetUnitName(_caster) + ": ritual " + why + " -- invulnerability removed");
      _caster = null;
    }

    private static bool Eligible(player p)
    {
      if (p == null || !SimpleBot.HasPerks(p))
      {
        return false;
      }

      var f = p.GetPlayerData().Faction;
      return f != null && (f.Name == "Legion" || f.Name == "Scourge");
    }

    // every 2 s (AiLegion.OnEventCheck): find the bot hero with the Book and let it cast
    internal static void Tick()
    {
      if (_done)
      {
        return;
      }

      if (Summoned())
      {
        _done = true;
        EndInvulnerable("done (Legion summoned)");
        Release();
        State = "done";
        return;
      }

      // safety: the channel stopped without an end event (unit removed, order replaced)
      if (_caster != null && (GetUnitTypeId(_caster) == 0 || !IsRitualOrder(GetUnitCurrentOrder(_caster))))
      {
        EndInvulnerable("stopped");
      }

      if (_hero != null && (GetUnitTypeId(_hero) == 0 || GetUnitState(_hero, UNIT_STATE_LIFE) <= 0.405f
                            || !HasBook(_hero) || !Eligible(GetOwningPlayer(_hero))))
      {
        Release();
      }

      if (_hero == null)
      {
        if (SimpleBot.Clock - _lastSearch < 6f || SimpleBot.Clock < _pauseUntil)
        {
          return;
        }

        _lastSearch = SimpleBot.Clock;
        BookForBots(); // 0.50: no level 12 for bot heroes at the pedestal
        _hero = FindBookHero();
        if (_hero == null)
        {
          State = "waiting for the Book";
          return;
        }

        AiState.MissionUnits.Add(_hero);
        SimpleBot.LogPin("P" + I2S(GetPlayerId(GetOwningPlayer(_hero))) + " " + SimpleBot.FacName(GetOwningPlayer(_hero))
                         + ": " + GetUnitName(_hero) + " holds the Book of Medivh -> Legion ritual");
      }

      AiState.MissionUnits.Add(_hero); // kept every tick (hero visits release their own heroes)
      if (_caster == _hero || IsRitualOrder(GetUnitCurrentOrder(_hero)) || SimpleBot.Clock - _lastOrder < RetryEvery)
      {
        return;
      }

      if (GetUnitState(_hero, UNIT_STATE_MANA) < ManaCost)
      {
        SetUnitState(_hero, UNIT_STATE_MANA, ManaCost);
        if (!_manaGiven)
        {
          _manaGiven = true;
          SimpleBot.Log(GetUnitName(_hero) + ": mana raised to " + I2S(R2I(ManaCost)) + " for the ritual (bot)");
        }
      }

      _lastOrder = SimpleBot.Clock;
      Casts++;
      State = "casting";
      float x = GetUnitX(_hero), y = GetUnitY(_hero);
      // "deathanddecay" first -- but never for a hero with a real Death and Decay (Kel'Thuzad): it would cast that
      var alt = GetUnitAbilityLevel(_hero, FourCC("AUdd")) == 0;
      var ok = (alt && IssuePointOrderById(_hero, _altOrderId, x, y)) || IssuePointOrderById(_hero, _orderId, x, y);
      if (ok)
      {
        _refused = 0;
        return;
      }

      _refused++;
      State = "order refused x" + I2S(_refused) + " (cooldown, Argus, or the ritual is not available yet)";
      if (_refused >= 3)
      {
        // 0.50.1: do not keep the hero frozen out of the army -- back to it for a minute, then try again
        SimpleBot.LogPin(GetUnitName(_hero) + ": ritual order refused 3 times -- back to the army, retry in 60 s");
        _refused = 0;
        _pauseUntil = SimpleBot.Clock + 60f;
        Release();
        State = "paused after refused orders";
      }
    }

    private static bool IsRitualOrder(int id) => id != 0 && (id == _orderId || id == _altOrderId);

    private static void Release()
    {
      if (_hero != null)
      {
        AiState.MissionUnits.Remove(_hero);
      }

      _hero = null;
    }

    private static unit FindBookHero()
    {
      for (var pl = 0; pl < 24; pl++)
      {
        var p = Player(pl);
        if (!Eligible(p))
        {
          continue;
        }

        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, p, null);
        unit found = null;
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (found == null && IsUnitType(u, UNIT_TYPE_HERO) && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && HasBook(u))
          {
            found = u;
          }
        }

        DestroyGroup(g);
        if (found != null)
        {
          return found;
        }
      }

      return null;
    }

    private static bool HasBook(unit u)
    {
      for (var i = 0; i < 6; i++)
      {
        var it = UnitItemInSlot(u, i);
        if (it != null && GetItemTypeId(it) == _bookId)
        {
          return true;
        }
      }

      return false;
    }

    // ---- 0.50: the Book of Medivh without level 12 for bots (maintainer) -------------------------------------------
    // WL's Book quest wants a level-12 hero from every faction but the Legion. A bot Legion / Scourge hero of ANY level
    // that stands at the pedestal when the level objective is the ONLY open one (Dalaran's / Gilneas' capital dead, no
    // one else holds the Book) completes the bot's own Book quest; the bot then puts the Book in the hero's inventory
    // (WL hands it only to the unit that completed the objective). WL's objective itself is not changed: humans still
    // need level 12. AiHeroVisits sends the hero (AtBookPedestal).
    private const float PedestalReach = 400f;
    private static bool _pedestalKnown;
    private static float _pedX;
    private static float _pedY;
    internal static int BookGiven;

    // where WL created the pedestal (Dalaran or Gilneas, whichever faction registered its quest first)
    internal static void Pedestal(ref float x, ref float y)
    {
      if (!_pedestalKnown)
      {
        var type = FourCC("nbsm");
        var g = CreateGroup();
        GroupEnumUnitsOfPlayer(g, Player(27), null);
        unit u;
        while ((u = FirstOfGroup(g)) != null)
        {
          GroupRemoveUnit(g, u);
          if (!_pedestalKnown && GetUnitTypeId(u) == type)
          {
            _pedestalKnown = true;
            _pedX = GetUnitX(u);
            _pedY = GetUnitY(u);
          }
        }

        DestroyGroup(g);
      }

      if (_pedestalKnown)
      {
        x = _pedX;
        y = _pedY;
      }
    }

    private static void BookForBots()
    {
      float px = 0f, py = 0f;
      Pedestal(ref px, ref py);
      if (!_pedestalKnown)
      {
        return;
      }

      for (var pl = 0; pl < 24; pl++)
      {
        var p = Player(pl);
        if (!Eligible(p))
        {
          continue;
        }

        var q = QuestAutoComplete.FindQuest(p.GetPlayerData().Faction, AiConfig.BookQuestTitle);
        if (q == null || q.Progress != QuestProgress.Incomplete || !OnlyLevelObjectiveOpen(q))
        {
          continue;
        }

        var hero = HeroNear(p, px, py);
        if (hero == null)
        {
          continue;
        }

        q.Progress = QuestProgress.Complete;
        if (!HasBook(hero))
        {
          UnitAddItem(hero, Artifacts.BookOfMedivh.Item);
        }

        BookGiven++;
        SimpleBot.LogPin("P" + I2S(pl) + " " + SimpleBot.FacName(p) + ": " + GetUnitName(hero) + " (lvl "
                         + I2S(GetHeroLevel(hero)) + ") took the Book of Medivh at the pedestal (bot: no level 12)"
                         + (HasBook(hero) ? "" : " -- Book NOT in its inventory"));
        return;
      }
    }

    private static bool OnlyLevelObjectiveOpen(QuestData q)
    {
      var open = 0;
      var level = false;
      var objs = q.Objectives;
      for (var i = 0; i < objs.Count; i++)
      {
        if (objs[i].Progress != QuestProgress.Complete)
        {
          open++;
          level = objs[i] is ObjectiveHeroWithLevelInRect;
        }
      }

      return open == 1 && level;
    }

    private static unit HeroNear(player p, float x, float y)
    {
      unit found = null;
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, PedestalReach, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (found == null && GetOwningPlayer(u) == p && IsUnitType(u, UNIT_TYPE_HERO)
            && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && UnitInventorySize(u) > 0)
        {
          found = u;
        }
      }

      DestroyGroup(g);
      return found;
    }

    // WL's "Under the Burning Sky" (QuestSummonLegion, global) is complete
    internal static bool Summoned()
    {
      if (_summonQuest == null)
      {
        var all = FactionManager.GetAllFactions();
        for (var i = 0; i < all.Count && _summonQuest == null; i++)
        {
          foreach (var q in all[i].GetAllQuests())
          {
            if (q is QuestSummonLegion)
            {
              _summonQuest = q;
            }
          }
        }
      }

      return _summonQuest != null && _summonQuest.Progress == QuestProgress.Complete;
    }

    internal static void Lines(List<string> o)
    {
      o.Add("legion ritual (0.49): " + State + (_hero != null ? " -- " + GetUnitName(_hero) + " of " + SimpleBot.FacName(GetOwningPlayer(_hero)) : "")
            + "; orders " + I2S(Casts) + ", channels " + I2S(Channels) + ", Book given at the pedestal (bot, no level 12) "
            + I2S(BookGiven) + (_pedestalKnown ? " -- pedestal @" + I2S(R2I(_pedX)) + "," + I2S(R2I(_pedY)) : ""));
    }
  }
}
