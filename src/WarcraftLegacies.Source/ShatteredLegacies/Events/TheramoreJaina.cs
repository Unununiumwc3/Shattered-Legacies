using MacroTools.Extensions;
using MacroTools.PreplacedWidgets;
using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Setup;
using WarcraftLegacies.Source.Shared;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): THERAMORE IS CONQUERABLE. Once Dalaran holds Theramore (WL's "Theramore" quest), Jaina's
  // Sanctum is its seat: whoever brings the Sanctum to 0 life takes it (Core/Effects/CaptureSeat) and Theramore serves
  // that player's team (never neutral -- the snowball is wanted). And JAINA follows her master:
  // - the Sanctum held by an EVIL faction -> Jaina is a dreadlord (Objects088.JainaDreadlord: Mal'Ganis' kit) -- the
  //   evil ones: the Burning Legion's and the Old Gods' teams (the Scourge, the Legion, the Black Empire, Ahn'qiraj and
  //   whoever serves them), the Fel Horde and the Dragonmaw (their team is shared with the Illidari);
  // - held by anyone else -> Jaina again. As often as the Sanctum changes hands (WL's own unit replacement: life, mana,
  //   XP and items carry over; skill points are given back to spend again). A dead Jaina changes when she is revived.
  internal static class TheramoreJaina
  {
    private static CaptureSeat _seat;
    private static bool _dreadlord;

    internal const string ConquestText =
      "|cffffcc00Jaina's Sanctum has fallen.|r Theramore lays down its arms -- Jaina Proudmoore serves its new masters.";

    internal static void Setup()
    {
      var t = CreateTimer();
      TimerStart(t, 2f, true, () => AiApi.Safe("theramore", Check));
    }

    private static void Check()
    {
      if (_seat == null)
      {
        TryRegister();
        return;
      }

      var evil = IsEvil(_seat.Holder);
      if (evil != _dreadlord)
      {
        Morph(evil);
      }
    }

    // Dalaran in Theramore: its Sanctum becomes the seat
    private static void TryRegister()
    {
      var sanctum = AllPreplacedWidgets.Units.Get(UNIT_H010_JAINA_S_SANCTUM_DALARAN_OTHER);
      if (sanctum == null || GetUnitTypeId(sanctum) == 0)
      {
        return;
      }

      var owner = GetOwningPlayer(sanctum);
      if (GetPlayerId(owner) >= 24 || owner.GetPlayerData().Faction?.Name != "Dalaran")
      {
        return;
      }

      SetUnitInvulnerable(sanctum, false);
      _seat = new CaptureSeat(sanctum, owner);
      Conquest.Register(owner, _ => _seat.Holder, null, false, true, ConquestText);
      AiApi.LogPin("Theramore conquerable (Jaina's Sanctum, P" + I2S(GetPlayerId(owner)) + ")");
    }

    internal static bool IsEvil(player p)
    {
      if (p == null || GetPlayerId(p) >= 24)
      {
        return false;
      }

      var data = p.GetPlayerData();
      var team = data.Team;
      if (team != null && (team == TeamSetup.Legion || team == TeamSetup.OldGods))
      {
        return true;
      }

      var name = data.Faction?.Name;
      return name == "Fel Horde" || name == "Dragonmaw" || name == "Scourge" || name == "Legion"
             || name == "Black Empire" || name == "Ahn'qiraj";
    }

    private static void Morph(bool evil)
    {
      var jaina = AllLegends.Dalaran.Jaina;
      var u = jaina.Unit;
      if (u == null || GetUnitTypeId(u) == 0 || GetUnitState(u, UNIT_STATE_LIFE) <= 0.405f)
      {
        return; // not there or dead: once she is back, the next check changes her
      }

      _dreadlord = evil;
      var p = GetOwningPlayer(u);
      var selected = IsUnitSelected(u, p);
      jaina.UnitType = evil ? Objects088.JainaDreadlord : UNIT_HJAI_ARCHMAGE_OF_DALARAN_DALARAN;
      var hero = jaina.Unit;
      if (hero != null && selected && GetLocalPlayer() == p)
      {
        SelectUnit(hero, true);
      }

      var text = evil
        ? "|cffff0000Theramore has a dark master now.|r Jaina Proudmoore's mask slips -- beneath it waits a dreadlord. "
          + "Garithos always said so."
        : "|cffffcc00Theramore is free of its dark master.|r The dreadlord's shape falls away: Jaina Proudmoore is herself "
          + "again.";
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 15, text);
      }

      AiApi.LogPin("Jaina -> " + (evil ? "dreadlord" : "Jaina"));
    }
  }
}
