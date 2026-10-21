using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Shared;

namespace WarcraftLegacies.Source.Ai
{
  // 0.38: what the bots really train. Counts every unit/hero a bot finishes training (event, no scans) and lists in
  // the dump, per bot: the trained types with counts, and the faction's categorised unit/hero types that were NEVER
  // trained. Compare with the offline scan `python3 tools/ai/unit_scan.py <faction>`, which explains WHY a type
  // can't be trained (wrong building pool, quest requirement without an AI rule, not in AiConfig.Heroes ...).
  internal static class AiStats
  {
    private static readonly Dictionary<int, Dictionary<int, int>> _trained = new(); // pid -> type -> count
    private static readonly Dictionary<int, Dictionary<int, int>> _ordered = new(); // pid -> type -> orders given
    private static trigger _trig;

    // Unit categories only (WL's buildings have their own); Builder is left out on purpose.
    private static readonly UnitCategory[] UnitCats =
    {
      UnitCategory.Elite, UnitCategory.Support, UnitCategory.Marksman, UnitCategory.Fighter, UnitCategory.Flyer,
      UnitCategory.Siege, UnitCategory.Destroyer, UnitCategory.Tank, UnitCategory.Assassin, UnitCategory.Summoner,
      UnitCategory.AntiMage, UnitCategory.AntiAir, UnitCategory.Unique, UnitCategory.Scout,
    };

    internal static void Setup()
    {
      _trig = CreateTrigger();
      for (var i = 0; i < 24; i++)
      {
        TriggerRegisterPlayerUnitEvent(_trig, Player(i), EVENT_PLAYER_UNIT_TRAIN_FINISH, null);
      }

      TriggerAddAction(_trig, OnTrained);
    }

    private static void OnTrained()
    {
      var u = GetTrainedUnit();
      var p = GetOwningPlayer(u);
      if (!SimpleBot.IsBot(p))
      {
        return;
      }

      var pid = GetPlayerId(p);
      if (!_trained.ContainsKey(pid))
      {
        _trained[pid] = new Dictionary<int, int>();
      }

      var t = GetUnitTypeId(u);
      var d = _trained[pid];
      d[t] = (d.ContainsKey(t) ? d[t] : 0) + 1;
    }

    // 0.38: orders the production gave per type (for "least trained first"; finished counts lag behind)
    internal static int Count(int pid, int typeId)
    {
      return _ordered.ContainsKey(pid) && _ordered[pid].ContainsKey(typeId) ? _ordered[pid][typeId] : 0;
    }

    internal static void Planned(int pid, int typeId)
    {
      if (!_ordered.ContainsKey(pid))
      {
        _ordered[pid] = new Dictionary<int, int>();
      }

      var d = _ordered[pid];
      d[typeId] = (d.ContainsKey(typeId) ? d[typeId] : 0) + 1;
    }

    internal static void Lines(List<string> o)
    {
      o.Add("## TRAINED (per bot since game start; never = the faction's categorised unit/hero types not trained yet)");
      for (var b = 0; b < SimpleBot.Bots.Count; b++)
      {
        var p = SimpleBot.Bots[b];
        var pid = GetPlayerId(p);
        var fac = p.GetPlayerData().Faction;
        if (fac == null)
        {
          continue;
        }

        var d = _trained.ContainsKey(pid) ? _trained[pid] : new Dictionary<int, int>();
        var line = "P" + I2S(pid) + " " + fac.Name + ":";
        var total = 0;
        foreach (var kv in d)
        {
          line = line + " " + Code(kv.Key) + "x" + I2S(kv.Value);
          total += kv.Value;
        }

        o.Add(line + " | total " + I2S(total) + ", " + I2S(d.Count) + " types"
              + (SimpleBot.GetDiag(pid).Saving ? " | SAVING for a hero" : ""));

        var seen = new List<int>();
        var never = "";
        var nCount = 0;
        for (var c = 0; c < UnitCats.Length; c++)
        {
          if (!fac.TryGetObjectByCategory(UnitCats[c], out var ids) || ids == null)
          {
            continue;
          }

          for (var i = 0; i < ids.Count; i++)
          {
            var id = ids[i];
            if (SimpleBot.ListHas(seen, id) || d.ContainsKey(id))
            {
              continue;
            }

            seen.Add(id);
            nCount++;
            never = never + " " + Code(id);
          }
        }

        o.Add("  never (" + I2S(nCount) + "):" + never);
      }
    }

    // 4-char raw code (heroes start with a capital letter)
    private static string Code(int id)
    {
      var s = "";
      for (var shift = 24; shift >= 0; shift -= 8)
      {
        var ch = (id >> shift) & 0xFF;
        s = s + (ch >= 32 && ch <= 126 ? "" + (char)ch : "?");
      }

      return s;
    }
  }
}
