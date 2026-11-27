using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Events;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Harpy
{
  // 0.85.0 (maintainer): Black Empire -> the HARPIES of Stonetalon. Trigger: N'Zoth lost (the catalog's Hq).
  // Lore: capturing the Harpy Roost means defeating the Void-Touched Matriarch and claiming her Resonance Totem. Once
  // the Old God's psychic tether is severed, the harpies enter a state of Magical Thralldom and imprint on the magic
  // and banner of whichever commander shattered their old master.
  // - map setup: WL's harpy nests at Stonetalon (Neutral Passive) stay visible and become indestructible; N'Zoth's
  //   slayer is recorded.
  // - run: the Black Empire's units are left behind; the nests become theirs -- Harpy Nests (harpies, food) and, the
  //   one at the heart of the colony, the Harpy Roost (Storm-hags, the two heroes); Stonetalon Peak is theirs (the
  //   Roost's CP: whoever holds it); Serena Bloodfeather and a flock. A VASSAL of whoever holds Stonetalon Peak (at the
  //   start: N'Zoth's slayer), never defeated, its nests indestructible.
  internal static class HarpyFallback
  {
    internal const float RoostX = -18505f; // the heart of the colony
    internal const float RoostY = 987f;
    internal static readonly ExclusivePath Path = new("Harpies", Prepare, Run, RoostX, RoostY);
    private const float BroodX = -18300f; // 0.88.0: the Broodmother's cage, by the nests (map_query: open ground)
    private const float BroodY = 1200f;
    private const string BroodText = "|cffffcc00The Broodmother's cage is taken!|r The harpies of Stonetalon follow "
                                     + "her new keeper: {0}.";
    private static CaptureSeat _broodmother;
    private static readonly List<unit> _nests = new();

    private static readonly int[] Flock =
    {
      HarpyObjects.Scout, HarpyObjects.Scout, HarpyObjects.Scout, HarpyObjects.Scout, HarpyObjects.Scout,
      HarpyObjects.Scout, HarpyObjects.Rogue, HarpyObjects.Rogue, HarpyObjects.Rogue, HarpyObjects.Rogue,
      HarpyObjects.Windwitch, HarpyObjects.Windwitch
    };

    internal const string ArrivalText =
      "|cffffcc00N'Zoth has fallen.|r At Stonetalon the Void-Touched Matriarch shrieks her last and her Resonance Totem "
      + "cracks. With the Old God's tether severed the harpies fall into Magical Thralldom -- they imprint on the banner "
      + "of the one who shattered their old master.";

    private static void Prepare()
    {
      LastConqueror.Watch(UNIT_U01Z_OLD_GOD_NZOTH, "Black Empire");
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, RoostX, RoostY, 2000f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_PASSIVE) && GetUnitTypeId(u) == FourCC("nhns"))
        {
          SetUnitInvulnerable(u, true);
          _nests.Add(u);
        }
      }

      DestroyGroup(g);
    }

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      var harpies = new HarpyFaction();
      SwapFaction.Run(p, harpies);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      TakeNests(p);
      StartCps.Force(p, UNIT_N01U_STONETALON_PEAK);
      Spawn.Units(p, RoostX, RoostY - 300f, new[] { HarpyObjects.Serena });
      Spawn.Units(p, RoostX + 256f, RoostY - 300f, Flock);
      var overlord = LastConqueror.Of("Black Empire");
      // 0.88.0 (maintainer): the seat is THE BROODMOTHER -- a caged hostage by the nests that cannot move; whoever
      // brings her to 0 life takes her (CaptureSeat), and the harpies serve that player's team. At the start she is
      // held by N'Zoth's slayer.
      var brood = CreateUnit(p, Objects088.Broodmother, BroodX, BroodY, 270f);
      _broodmother = new CaptureSeat(brood, overlord != null && overlord != p ? overlord : p, BroodText);
      Conquest.Register(p, _ => _broodmother.Holder, overlord != p ? overlord : null, true);
      ProtectNests(p);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(RoostX, RoostY));
      AiApi.LogPin("Black Empire fallback -> Harpies" + (overlord != null ? ", conqueror P"
        + I2S(GetPlayerId(overlord)) : ", no conqueror"));
    }

    // WL's nests become the faction's: the one nearest the colony's heart is the Roost
    private static void TakeNests(player p)
    {
      unit roost = null;
      var best = float.MaxValue;
      for (var i = 0; i < _nests.Count; i++)
      {
        var n = _nests[i];
        if (GetUnitTypeId(n) == 0)
        {
          continue;
        }

        var dx = GetUnitX(n) - RoostX;
        var dy = GetUnitY(n) - RoostY;
        if (dx * dx + dy * dy < best)
        {
          best = dx * dx + dy * dy;
          roost = n;
        }
      }

      for (var i = 0; i < _nests.Count; i++)
      {
        var n = _nests[i];
        if (GetUnitTypeId(n) == 0)
        {
          continue;
        }

        float x = GetUnitX(n), y = GetUnitY(n);
        RemoveUnit(n);
        var made = CreateUnit(p, n == roost ? HarpyObjects.Roost : HarpyObjects.Nest, x, y, 270f);
        SetUnitInvulnerable(made, true);
      }
    }

    private static void ProtectNests(player p)
    {
      var t = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(t, p, EVENT_PLAYER_UNIT_CHANGE_OWNER, null);
      TriggerAddAction(t, () =>
      {
        var u = GetTriggerUnit();
        var type = GetUnitTypeId(u);
        if (GetOwningPlayer(u) == p && (type == HarpyObjects.Nest || type == HarpyObjects.Roost))
        {
          SetUnitInvulnerable(u, true);
        }
      });
    }
  }
}
