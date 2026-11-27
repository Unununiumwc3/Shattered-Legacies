using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Dungeons;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Defias
{
  // 0.85.0 (maintainer): Stormwind -> the DEFIAS BROTHERHOOD in the Deadmines (Dungeons/VoidDungeons). Trigger: Stormwind
  // Keep lost (the catalog's Hq; replaces the Gilneas fallback -- Gilneas is Lordaeron's now).
  // - run: Stormwind's units are left behind; the Deadmines' gates open and its seat is theirs (0.87.0: the others are
  //   guarded by creeps); Dunco Ratsbon, Count Clessington and a band arrive; Westfall's bandit camp joins; recruits
  //   every turn (TurnLevy: 3 Bandits and a Brigand, more with the quests); the heroes return by themselves in the
  //   Deadmines (HeroRevival). A VASSAL of whoever holds the Deadmines (at the start: whoever took or razed Stormwind
  //   Keep), never defeated.
  internal static class DefiasFallback
  {
    internal const float StartX = -27808f; // the Deadmines, by the inner gate
    internal const float StartY = -24400f;
    private const float CampX = 7787f;     // Westfall's bandit camp (WL's creeps)
    private const float CampY = -17311f;
    internal static readonly ExclusivePath Path = new("Defias Brotherhood", Prepare, Run, StartX, StartY);
    internal static TurnLevy Levy;

    internal const string ArrivalText =
      "|cffffcc00Stormwind has fallen.|r In the ruins of Moonbrook the red masks come out. Dunco Ratsbon promises gold, "
      + "Count Clessington promises land -- and the Defias Brotherhood bends the knee to the one who broke the "
      + "kingdom that cheated them.";

    private static readonly int[] Band =
    {
      DefiasObjects.Bandit, DefiasObjects.Bandit, DefiasObjects.Bandit, DefiasObjects.Bandit, DefiasObjects.Bandit,
      DefiasObjects.Bandit, DefiasObjects.Bandit, DefiasObjects.Bandit, DefiasObjects.Brigand, DefiasObjects.Brigand,
      DefiasObjects.Brigand, DefiasObjects.Brigand, DefiasObjects.Brigand, DefiasObjects.Brigand, DefiasObjects.Rogue,
      DefiasObjects.Rogue, DefiasObjects.Rogue, DefiasObjects.Rogue, DefiasObjects.Assassin, DefiasObjects.Assassin,
      DefiasObjects.Enforcer, DefiasObjects.Enforcer
    };

    private static void Prepare() => LastConqueror.Watch(UNIT_H00X_STORMWIND_KEEP_STORMWIND_OTHER, "Stormwind");

    private static void Run(player p, TurningPointDef def)
    {
      var keep = new List<unit>();
      var glass = Hourglass.Get(GetPlayerId(p));
      if (glass != null)
      {
        keep.Add(glass);
      }

      StromgardeHandover.Run(p.GetPlayerData().Faction); // 0.87.0: before the swap (Stormwind's quests)
      var defias = new DefiasFaction();
      SwapFaction.Run(p, defias);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      VoidDungeons.Open(VoidDungeons.Deadmines);
      StartCps.Force(p, DungeonObjects.Deadmines); // 0.87.0: the seat only -- the other two are guarded by creeps
      Spawn.Units(p, StartX, StartY, new[] { DefiasObjects.Ratsbon, DefiasObjects.Clessington });
      Spawn.Units(p, StartX + 256f, StartY - 256f, Band);
      JoinCamp(p);
      Levy = new TurnLevy(p, defias, StartX + 128f, StartY - 512f, DefiasObjects.Bandit, DefiasObjects.Bandit,
        DefiasObjects.Bandit, DefiasObjects.Brigand);
      HeroRevival.Register(p, defias, StartX, StartY, 60f);
      var overlord = LastConqueror.Of("Stormwind");
      Conquest.Register(p, DungeonObjects.Deadmines, overlord != p ? overlord : null, true);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(StartX, StartY));
      AiApi.LogPin("Stormwind fallback -> Defias" + (overlord != null ? ", conqueror P" + I2S(GetPlayerId(overlord))
        : ", no conqueror"));
    }

    // WL's bandits at the Deadmines' entrance in Westfall are the Brotherhood's men
    private static void JoinCamp(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, CampX, CampY, 700f, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_AGGRESSIVE) && UnitAliveCheck(u)
            && (t == FourCC("nban") || t == FourCC("nbrg") || t == FourCC("nrog") || t == FourCC("nass")
                || t == FourCC("nenf") || t == FourCC("nbld")))
        {
          SetUnitOwner(u, p, true);
        }
      }

      DestroyGroup(g);
    }

    private static bool UnitAliveCheck(unit u) => GetUnitState(u, UNIT_STATE_LIFE) > 0.405f;
  }
}
