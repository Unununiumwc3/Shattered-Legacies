using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Instances;
using MacroTools.Quests;
using WarcraftLegacies.Source.Factions.Legion.Quests;
using WarcraftLegacies.Source.Objectives.UnitBased;
using WarcraftLegacies.Source.Setup;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.Ai
{
  // 0.49.0 / 0.51.0: the Legion is summoned ONLY through the Book of Medivh (maintainer). WL's way for humans: a hero
  // carrying the Book casts "Summon the Burning Legion" (A00J, in the Book's spellbook; 180-s channel), the quest
  // "Under the Burning Sky" completes and WL opens the Demon Portal where the caster stands.
  // 0.51.0: the BOT PERFORMS THE RITUAL ITSELF instead of ordering the spell (maintainer's choice): 0.50.0 / 0.50.1 --
  // every cast order was refused ("volcano" and "deathanddecay", heroes of both factions), the heroes stood.
  // Same requirements as the spell, same result:
  // - a bot hero (real bot or test autopilot) of the Legion or the Scourge with the Book (item I006) in its inventory;
  //   the Legion still exists (WL's research R04T, the spell's requirement); not inside an instance (Argus, Outland --
  //   WL's spell stops there too); 410 mana (spent; a bot hero that has less gets it -- logged once);
  // - it channels on the spot where it stands: hold position, channel animation, invulnerable (maintainer: only while
  //   channelling), a rain of fire that deals no damage, a 180-s timer for everyone ("Legion Summon (bot channelling)"),
  //   a minimap ping and a message. 0.51.1 (maintainer: bots must not be stopped): invulnerability is set again every
  //   second, a hero pushed off the spot is put back; only losing the Book, changing owner or a removed hero ends it.
  // - at the end the bot completes WL's quest (rescued units, Archimonde, music, limits -- WL's own reward) and puts
  //   WL's Demon Portal on the ritual spot: WL makes it at the caster, but a bot completion has no caster, so WL leaves
  //   it at (0,0); the bot moves it and points Antoran's portal (n03C) at it.
  // A human hero (any faction) uses WL's real spell as always; a bot that really casts A00J is made invulnerable too.
  internal static class AiRitual
  {
    private const float Duration = 180f;     // A00J adur
    private const float ManaCost = 410f;     // A00J amcs
    private const float Area = 750f;         // A00J aare -- the rain of fire
    private const float SpotLeave = 150f;    // 0.51.1: moved further than this (knockback, a stray order): put back
    private const float CancelPause = 30f;
    private const string FxRain = "Abilities\\Spells\\Demon\\RainOfFire\\RainOfFireTarget.mdl";
    private const string FxStart = "Abilities\\Spells\\Undead\\AntiMagicShell\\AntiMagicShell.mdl";

    private static int _ritualId;
    private static int _bookId;
    private static int _techId;
    private static int _holdId;
    private static trigger _start;
    private static trigger _end;
    private static float _lastSearch = -100f;
    private static float _pauseUntil;
    private static unit _caster;       // a bot hero that REALLY casts A00J (made invulnerable)
    private static bool _wasInvulnerable;
    private static unit _hero;         // the bot hero with the Book
    private static bool _channel;      // the bot's ritual runs
    private static float _chanStart;
    private static float _chanX;
    private static float _chanY;
    private static bool _heroWasInvulnerable;
    private static timer _timer;
    private static timerdialog _dialog;
    private static bool _manaGiven;
    private static bool _done;
    private static QuestData _summonQuest;
    internal static int Channels;      // bot rituals started
    internal static int Cancelled;
    internal static string State = "waiting for the Book";

    internal static void Setup()
    {
      _ritualId = FourCC("A00J");
      _bookId = FourCC("I006");
      _techId = FourCC("R04T"); // "Legion exists" -- A00J's requirement
      _holdId = OrderId("holdposition");
      _start = CreateTrigger();
      _end = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_start, Player(i), EVENT_PLAYER_UNIT_SPELL_CHANNEL, null);
        TriggerRegisterPlayerUnitEvent(_end, Player(i), EVENT_PLAYER_UNIT_SPELL_ENDCAST, null);
      }

      TriggerAddAction(_start, () => OnSpell(true));
      TriggerAddAction(_end, () => OnSpell(false));
      AiScheduler.Add("legion ritual", 4, 2, Tick, false); // every second
    }

    internal static bool Owns(unit u) => u != null && (u == _hero || u == _caster);

    // a bot hero that really casts A00J (should a cast ever go through): invulnerable while it channels
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
          SimpleBot.LogPin(GetUnitName(u) + " casts WL's Legion ritual -- invulnerable while channelling");
        }
        else if (u == _caster)
        {
          SetUnitInvulnerable(_caster, _wasInvulnerable);
          _caster = null;
        }
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("TICK ERROR in legion ritual: " + e.Message);
      }
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

    // every second (scheduler)
    private static void Tick()
    {
      if (_done || SimpleBot.BotBreak)
      {
        return;
      }

      if (Summoned())
      {
        _done = true;
        if (_channel)
        {
          EndChannel("the Legion was summoned (another way)");
        }

        Release();
        State = "done";
        return;
      }

      if (_channel)
      {
        Channel();
        return;
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

        SimpleBot.LogPin("P" + I2S(GetPlayerId(GetOwningPlayer(_hero))) + " " + SimpleBot.FacName(GetOwningPlayer(_hero))
                         + ": " + GetUnitName(_hero) + " holds the Book of Medivh -> Legion ritual");
      }

      AiState.MissionUnits.Add(_hero); // kept every tick (hero visits release their own heroes)
      TryStart();
    }

    private static void TryStart()
    {
      var p = GetOwningPlayer(_hero);
      float x = GetUnitX(_hero), y = GetUnitY(_hero);
      if (GetPlayerTechCount(p, _techId, true) < 1)
      {
        State = "waiting: the Legion does not exist (R04T)";
        return;
      }

      if (InstanceSystem.GetPointInstance(new Point(x, y)) != null)
      {
        // WL's spell is stopped inside an instance (Argus, Outland): let the hero go with the army until it is out
        State = "waiting: the hero is inside an instance";
        Release();
        _pauseUntil = SimpleBot.Clock + CancelPause;
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

      SetUnitState(_hero, UNIT_STATE_MANA, GetUnitState(_hero, UNIT_STATE_MANA) - ManaCost);
      _channel = true;
      _chanStart = SimpleBot.Clock;
      _chanX = x;
      _chanY = y;
      _heroWasInvulnerable = BlzIsUnitInvulnerable(_hero);
      SetUnitInvulnerable(_hero, true);
      IssueImmediateOrderById(_hero, _holdId);
      SetUnitAnimation(_hero, "spell channel");
      DestroyEffect(AddSpecialEffectTarget(FxStart, _hero, "overhead"));
      _timer = CreateTimer();
      TimerStart(_timer, Duration, false, null);
      _dialog = CreateTimerDialog(_timer);
      TimerDialogSetTitle(_dialog, "Legion Summon (bot channelling)");
      TimerDialogDisplay(_dialog, true);
      PingMinimap(x, y, 8f);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 10, "|cffff5555The Burning Legion is being summoned! ("
                                 + SimpleBot.FacName(p) + ", bot)|r");
      }

      Channels++;
      State = "channelling";
      SimpleBot.LogPin("P" + I2S(GetPlayerId(p)) + " " + SimpleBot.FacName(p) + ": " + GetUnitName(_hero)
                       + " channels the Legion ritual @" + I2S(R2I(x)) + "," + I2S(R2I(y))
                       + " -- invulnerable while channelling (bot ritual, 180 s)");
    }

    // every second while the bot ritual runs
    private static void Channel()
    {
      var gone = _hero == null || GetUnitTypeId(_hero) == 0 || GetUnitState(_hero, UNIT_STATE_LIFE) <= 0.405f;
      if (gone || !HasBook(_hero) || !Eligible(GetOwningPlayer(_hero)))
      {
        Cancel(gone ? "hero gone" : (!HasBook(_hero) ? "Book lost" : "owner changed"));
        return;
      }

      var dx = GetUnitX(_hero) - _chanX;
      var dy = GetUnitY(_hero) - _chanY;
      if (dx * dx + dy * dy > SpotLeave * SpotLeave)
      {
        SetUnitX(_hero, _chanX); // 0.51.1: back on the spot -- nothing stops a bot's ritual
        SetUnitY(_hero, _chanY);
      }

      SetUnitInvulnerable(_hero, true); // 0.51.1: every second -- other code may reset it
      AiState.MissionUnits.Add(_hero);
      if (GetUnitCurrentOrder(_hero) != _holdId)
      {
        IssueImmediateOrderById(_hero, _holdId);
      }

      SetUnitAnimation(_hero, "spell channel");
      for (var k = 0; k < 4; k++)
      {
        var a = GetRandomReal(0f, 6.2832f);
        var r = GetRandomReal(100f, Area);
        DestroyEffect(AddSpecialEffect(FxRain, _chanX + r * Cos(a), _chanY + r * Sin(a)));
      }

      if (SimpleBot.Clock - _chanStart < Duration)
      {
        State = "channelling, " + I2S(R2I(Duration - (SimpleBot.Clock - _chanStart))) + " s left";
        return;
      }

      Complete();
    }

    private static void Complete()
    {
      if (_summonQuest == null)
      {
        Summoned(); // finds the quest
      }

      if (_summonQuest == null)
      {
        Cancel("WL's summon quest not found");
        return;
      }

      try
      {
        _summonQuest.Progress = QuestProgress.Complete; // WL's reward; the portal comes up at (0,0) -- no caster
      }
      catch (System.Exception e)
      {
        SimpleBot.LogPin("Legion ritual: WL error while completing the summon quest -- " + e.Message);
      }

      SimpleBot.Safe("legion ritual: portal", () => PlacePortal(_chanX, _chanY));
      EndChannel("summoned");
      _done = true;
      Release();
      State = "done";
    }

    // WL's exterior Demon Portal (n037) from (0,0) to the ritual spot, Antoran's n03C pointed at it
    private static void PlacePortal(float x, float y)
    {
      var type = FourCC("n037");
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, 0f, 0f, 900f, null);
      unit stranded = null;
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (stranded == null && GetUnitTypeId(u) == type)
        {
          stranded = u;
        }
      }

      DestroyGroup(g);
      if (stranded == null)
      {
        SimpleBot.LogPin("Legion ritual: no Demon Portal at (0,0) after the summon -- left as WL made it");
        return;
      }

      var fresh = CreateUnit(GetOwningPlayer(stranded), type, x, y, 0f);
      var interior = SimpleBot.FindUnitOfType(FourCC("n03C"), AiWorld.LegionInteriorX, AiWorld.LegionInteriorY, 1500f);
      if (interior != null)
      {
        // land BESIDE the other end (landing on a waygate can bounce the unit straight back)
        WaygateSetDestination(fresh, GetUnitX(interior) - 400f, GetUnitY(interior));
        Beside(x, y);
        WaygateSetDestination(interior, _bx, _by);
        WaygateActivate(interior, true);
      }
      else
      {
        WaygateSetDestination(fresh, WaygateGetDestinationX(stranded), WaygateGetDestinationY(stranded));
      }

      WaygateActivate(fresh, true);
      RemoveUnit(stranded);
      PingMinimap(x, y, 8f);
      SimpleBot.LogPin("Legion ritual: Demon Portal placed at the ritual spot @" + I2S(R2I(x)) + "," + I2S(R2I(y))
                       + (interior != null ? ", Antoran portal pointed at it" : ", Antoran portal NOT found"));
    }

    private static float _bx;
    private static float _by;

    // a walkable point ~350 from (x, y) -> _bx/_by (the spot itself when none is)
    private static void Beside(float x, float y)
    {
      _bx = x;
      _by = y;
      for (var i = 0; i < 8; i++)
      {
        var a = i * 0.785398f;
        var px = x + 350f * Cos(a);
        var py = y + 350f * Sin(a);
        if (!IsTerrainPathable(px, py, PATHING_TYPE_WALKABILITY)) // TRUE = not walkable (WC3's inverted API)
        {
          _bx = px;
          _by = py;
          return;
        }
      }
    }

    private static void Cancel(string why)
    {
      Cancelled++;
      SimpleBot.LogPin("Legion ritual cancelled: " + why);
      EndChannel(why);
      Release();
      _pauseUntil = SimpleBot.Clock + CancelPause;
      State = "cancelled (" + why + "), retry in " + I2S(R2I(CancelPause)) + " s";
    }

    private static void EndChannel(string why)
    {
      _channel = false;
      if (_hero != null && GetUnitTypeId(_hero) != 0)
      {
        SetUnitInvulnerable(_hero, _heroWasInvulnerable);
        SetUnitAnimation(_hero, "stand");
      }

      if (_dialog != null)
      {
        TimerDialogDisplay(_dialog, false);
        DestroyTimerDialog(_dialog);
        _dialog = null;
      }

      if (_timer != null)
      {
        DestroyTimer(_timer);
        _timer = null;
      }

      SimpleBot.Log("Legion ritual: channel ended (" + why + ")");
    }

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
      o.Add("legion ritual (0.51, bot ritual): " + State + (_hero != null ? " -- " + GetUnitName(_hero) + " of "
            + SimpleBot.FacName(GetOwningPlayer(_hero)) : "") + "; started " + I2S(Channels) + ", cancelled "
            + I2S(Cancelled) + ", Book given at the pedestal (bot, no level 12) " + I2S(BookGiven)
            + (_pedestalKnown ? " -- pedestal @" + I2S(R2I(_pedX)) + "," + I2S(R2I(_pedY)) : ""));
    }
  }
}
