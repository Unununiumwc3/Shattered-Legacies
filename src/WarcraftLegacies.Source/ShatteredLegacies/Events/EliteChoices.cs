using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;
using MacroTools.Researches;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Shared.Researches;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): ELITE CHOICES on the Hourglass of Fate. A faction can only have ONE elite unit:
  // - WL's own choices (Blademasters / Kor'kron, Liches / Death Knights ...) are also researched at the Hourglass --
  //   the same research, so the lock is WL's (tools/shattered-legacies/timelines/gen_hourglass.py, WL_ELITE);
  // - every faction with two elite-tier units gets a new choice: both are locked (limit 0) when the faction comes into
  //   play, the research of the one gives it back its limit, the other stays locked (ResearchManager's incompatible
  //   set). Open from turn 10 (the condition tech). Bots pick one at random once it is open (a free order at their
  //   Hourglass).
  // A faction is recognised by its units: the pair is the faction's if it may field both (limits > 0).
  internal static class EliteChoices
  {
    private const int OpenTurn = 10;

    // key (as in HourglassData), unit A, unit B -- the order of gen_hourglass.py's ELITE list
    private static readonly (int ResA, int ResB, string A, string B)[] Pairs =
    {
      (HourglassData.EliteStormwindA, HourglassData.EliteStormwindB, "h05F", "n05L"),
      (HourglassData.EliteLordaeronA, HourglassData.EliteLordaeronB, "h00F", "h02Q"),
      (HourglassData.EliteDalaranA, HourglassData.EliteDalaranB, "n007", "n0AC"),
      (HourglassData.EliteQuelthalasA, HourglassData.EliteQuelthalasB, "n00A", "n048"),
      (HourglassData.EliteKultirasA, HourglassData.EliteKultirasB, "n07G", "n029"),
      (HourglassData.EliteGilneasA, HourglassData.EliteGilneasB, "o01V", "o06P"),
      (HourglassData.EliteScarletA, HourglassData.EliteScarletB, "h06B", "n09N"),
      (HourglassData.EliteSunfuryA, HourglassData.EliteSunfuryB, "n0E7", "n0E3"),
      (HourglassData.EliteDraeneiA, HourglassData.EliteDraeneiB, "h09R", "h09U"),
      (HourglassData.EliteLegionA, HourglassData.EliteLegionB, "u007", "n04O"),
      (HourglassData.EliteFelHordeA, HourglassData.EliteFelHordeB, "o01L", "n086"),
      (HourglassData.EliteIllidariA, HourglassData.EliteIllidariB, "nnrg", "h0AC"),
      (HourglassData.EliteBlackEmpireA, HourglassData.EliteBlackEmpireB, "n0B4", "bHN1"),
      (HourglassData.EliteSkywallA, HourglassData.EliteSkywallB, "n08S", "oETL"),
      (HourglassData.EliteNazjatarA, HourglassData.EliteNazjatarB, "hZb2", "hZb1"),
      (HourglassData.EliteAshtongueA, HourglassData.EliteAshtongueB, "hZy8", "ndrs"),
      (HourglassData.EliteZandalariA, HourglassData.EliteZandalariB, "oZxh", "eZx0"),
      (HourglassData.EliteForsakenA, HourglassData.EliteForsakenB, "nZf3", "uZfb"),
      (HourglassData.EliteKezanA, HourglassData.EliteKezanB, "oZkh", "hZk5"),
    };

    // WL's own elite researches, listed on every Hourglass -- hidden where the faction has none of them
    private static readonly int[] WlElite =
    {
      UPGRADE_RV01_PRIESTESSES_OF_THE_MOON_SENTINELS, UPGRADE_RV02_WARDENS_SENTINELS,
      UPGRADE_RK01_KEEPERS_OF_THE_GROVE_DRUIDS, UPGRADE_RK02_HEARTWOOD_ANCIENTS_DRUIDS,
      UPGRADE_RT01_TAUREN_CHIEFTAINS_TAUREN_TRIBES, UPGRADE_RT02_OGRE_LORDS_TAUREN_TRIBES,
      UPGRADE_RZ03_KOR_KRON_ELITES_ORCISH_HORDE,
      WarcraftLegacies.Shared.ShatteredLegacies.SlObjects.UPGRADE_RZO1_BLADEMASTERS_ORCISH_HORDE,
      UPGRADE_ZB24_DEATH_KNIGHTS_SCOURGE, UPGRADE_ZB74_LICHES_SCOURGE, UPGRADE_ZBHS_SHAPED_OBSIDIAN_C_THUN,
      UPGRADE_ZBML_SPELL_CONDUCTION_C_THUN, UPGRADE_TP28_FLAMETHROWER_IRONFORGE, UPGRADE_TP29_ARTILLERY_IRONFORGE
    };

    // A named type avoids CSharp.lua's shared cache for three- and four-integer tuples.
    private sealed class LockedPair
    {
      internal readonly int Pair;
      internal readonly int LimitA;
      internal readonly int LimitB;

      internal LockedPair(int pair, int limitA, int limitB)
      {
        Pair = pair;
        LimitA = limitA;
        LimitB = limitB;
      }
    }

    // per faction: the pairs it has, with the limits the two units had before they were locked
    private static readonly Dictionary<Faction, List<LockedPair>> _locked = new();
    private static readonly Dictionary<int, Faction> _seen = new(); // pid -> the faction last handled
    private static readonly HashSet<int> _botPicked = new();         // pid * 100 + pair

    internal static void Setup()
    {
      for (var i = 0; i < Pairs.Length; i++)
      {
        var pair = i;
        ResearchManager.RegisterIncompatibleSet(
          new CustomResearch(Pairs[i].ResA, 0) { ResearchFunc = p => Unlock(p, pair, true) },
          new CustomResearch(Pairs[i].ResB, 0) { ResearchFunc = p => Unlock(p, pair, false) });
      }

      GameTimeManager.RegisterOnTurn(OpenTurn, () => AiApi.Safe("elite choices open", () =>
      {
        for (var i = 0; i < 24; i++)
        {
          SetPlayerTechResearched(Player(i), HourglassData.EliteCondition, 1);
        }
      }));
      var t = CreateTimer();
      TimerStart(t, 5f, true, () => AiApi.Safe("elite choices", Check));
    }

    private static void Check()
    {
      for (var pid = 0; pid < 24; pid++)
      {
        var p = Player(pid);
        var faction = p.GetPlayerData().Faction;
        if (faction == null)
        {
          continue;
        }

        if (!_seen.TryGetValue(pid, out var had) || had != faction)
        {
          _seen[pid] = faction;
          Apply(p, faction);
        }

        if (AiApi.IsBot(p) && GameTimeManager.Turn >= OpenTurn && _locked.TryGetValue(faction, out var pairs))
        {
          BotPick(p, pairs);
        }
      }
    }

    // a faction comes into play on this player: lock its pairs, show only its researches
    private static void Apply(player p, Faction faction)
    {
      if (!_locked.ContainsKey(faction))
      {
        var list = new List<LockedPair>();
        for (var i = 0; i < Pairs.Length; i++)
        {
          var a = FourCC(Pairs[i].A);
          var b = FourCC(Pairs[i].B);
          var la = faction.GetObjectLimit(a);
          var lb = faction.GetObjectLimit(b);
          if (la <= 0 || lb <= 0)
          {
            continue; // both must be the faction's (the Illidari field draenei Seers too, but no Vindicators)
          }

          faction.ModObjectLimit(a, -la);
          faction.ModObjectLimit(b, -lb);
          faction.ModObjectLimit(Pairs[i].ResA, 1);
          faction.ModObjectLimit(Pairs[i].ResB, 1);
          list.Add(new LockedPair(i, la, lb));
          AiApi.Log("elite choice " + faction.Name + ": " + Pairs[i].A + " / " + Pairs[i].B + " locked");
        }

        _locked[faction] = list;
      }

      var own = _locked[faction];
      for (var i = 0; i < Pairs.Length; i++)
      {
        var mine = own.Exists(x => x.Pair == i);
        if (!mine)
        {
          SetPlayerTechMaxAllowed(p, Pairs[i].ResA, 0);
          SetPlayerTechMaxAllowed(p, Pairs[i].ResB, 0);
        }
      }

      for (var i = 0; i < WlElite.Length; i++)
      {
        if (faction.GetObjectLimit(WlElite[i]) <= 0)
        {
          SetPlayerTechMaxAllowed(p, WlElite[i], 0);
        }
      }

      if (faction.Name != "Scourge")
      {
        SetPlayerTechMaxAllowed(p, Objects088.PlagueGoesSouth, 0);
      }
    }

    private static void Unlock(player p, int pair, bool first)
    {
      var faction = p.GetPlayerData().Faction;
      if (faction == null || !_locked.TryGetValue(faction, out var list))
      {
        return;
      }

      var i = list.FindIndex(x => x.Pair == pair);
      if (i < 0)
      {
        return;
      }

      var entry = list[i];
      var unitType = FourCC(first ? Pairs[pair].A : Pairs[pair].B);
      var limit = first ? entry.LimitA : entry.LimitB;
      faction.ModObjectLimit(unitType, limit);
      AiApi.LogPin("elite choice " + faction.Name + ": " + GetObjectName(unitType) + " (" + I2S(limit) + ")");
    }

    private static void BotPick(player p, List<LockedPair> pairs)
    {
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass == null)
      {
        return;
      }

      for (var i = 0; i < pairs.Count; i++)
      {
        var key = GetPlayerId(p) * 100 + pairs[i].Pair;
        if (_botPicked.Contains(key))
        {
          continue;
        }

        var pair = Pairs[pairs[i].Pair];
        if (GetPlayerTechCount(p, pair.ResA, true) > 0 || GetPlayerTechCount(p, pair.ResB, true) > 0)
        {
          _botPicked.Add(key);
          continue;
        }

        var research = GetRandomInt(0, 1) == 0 ? pair.ResA : pair.ResB;
        if (AiApi.FreeOrder(p, glass, research))
        {
          _botPicked.Add(key);
          AiApi.Log("bot elite choice P" + I2S(GetPlayerId(p)) + ": " + GetObjectName(research));
        }
      }
    }
  }
}
