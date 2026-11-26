using System.Collections.Generic;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Centaur
{
  // 0.85.0 (maintainer): Tauren Tribes -> the CENTAUR KHANS (Skywall moved to Ahn'Qiraj). Trigger: Thunder Bluff lost.
  // - map setup: WL's centaur tents (Neutral Passive) at the five camps stay visible and become indestructible; whoever
  //   razes or takes Thunder Bluff last is recorded.
  // - run: the Tauren's units are left behind; the Kolkar camp by Thunder Bluff is theirs, Khan Leh'Prah and a war band
  //   ride in; every turn warriors arrive there (TurnLevy: 2 Outrunners, an Archer); khans return there by themselves
  //   (HeroRevival). A VASSAL of whoever holds Thunder Bluff's CP (at the start: Thunder Bluff's conqueror -- "the Apex
  //   Predator of the plains"), never defeated, its camps indestructible.
  // - the other four khans: a quest each (hold their land's CP) -> the khan, his camp and more warriors per turn.
  internal static class CentaurFallback
  {
    internal sealed class Clan
    {
      internal string Title, Text, Reward;
      internal int Cp, Khan;
      internal float X, Y;
      internal int[] Levy;
      internal List<unit> Tents = new();
    }

    private const float CampRadius = 1200f;
    internal const float StartX = -14453f; // the Kolkar camp by Thunder Bluff
    internal const float StartY = -2485f;
    internal static readonly ExclusivePath Path = new("Centaur Khans", Prepare, Run, StartX, StartY);
    internal static TurnLevy Levy;
    private static readonly List<unit> _kolkar = new();

    internal static readonly Clan[] Clans =
    {
      new()
      {
        Title = "The Galak", Cp = UNIT_N02A_SOUTHERN_BARRENS, Khan = CentaurObjects.Ablinh, X = -13200f, Y = -4736f,
        Text = "Khan Ablinh's archers hunt the southern Barrens. Take the land and he rides to the oath.",
        Reward = "Khan Ablinh and the Galak camp join; an Archer more every turn",
        Levy = new[] { CentaurObjects.Archer },
      },
      new()
      {
        Title = "The Maraudine", Cp = UNIT_N03Z_DUROTAR, Khan = CentaurObjects.Hratha, X = -9536f, Y = -3296f,
        Text = "Khan Hratha's raiders prowl the coast of Durotar. Take Durotar and the sons of Theradras follow.",
        Reward = "Khan Hratha and the Maraudine camp join; an Impaler more every turn",
        Levy = new[] { CentaurObjects.Impaler },
      },
      new()
      {
        Title = "The Magram", Cp = UNIT_N01Y_DESOLACE, Khan = CentaurObjects.Kammah, X = -20416f, Y = -4448f,
        Text = "The Magram bow to strength alone. Take Desolace and Khan Kammah swears the oath.",
        Reward = "Khan Kammah and the Magram camp join; an Outrunner more every turn",
        Levy = new[] { CentaurObjects.Outrunner },
      },
      new()
      {
        Title = "The Gelkis", Cp = UNIT_N047_SILITHUS, Khan = CentaurObjects.Shodo, X = -18240f, Y = -11840f,
        Text = "Khan Shodo's earthcallers wait at the edge of the southern desert. Take Silithus and they join the hunt.",
        Reward = "Khan Shodo and the Gelkis camp join; a Sorcerer more every turn",
        Levy = new[] { CentaurObjects.Sorcerer },
      },
    };

    private static readonly int[] Band =
    {
      CentaurObjects.Outrunner, CentaurObjects.Outrunner, CentaurObjects.Outrunner, CentaurObjects.Outrunner,
      CentaurObjects.Outrunner, CentaurObjects.Outrunner, CentaurObjects.Archer, CentaurObjects.Archer,
      CentaurObjects.Archer, CentaurObjects.Archer, CentaurObjects.Impaler, CentaurObjects.Impaler,
      CentaurObjects.Sorcerer, CentaurObjects.Sorcerer
    };

    internal const string ArrivalText =
      "|cffffcc00Thunder Bluff has fallen.|r The one who broke the Tauren has proven himself the Apex Predator of the "
      + "plains. The centaur do not resist: Khan Leh'Prah swears the Blood-Oath of the Hunt, eager to ride beside the "
      + "warlord strong enough to break their ancient enemies.";

    private static void Prepare()
    {
      LastConqueror.Watch(UNIT_O00J_THUNDER_BLUFF_FROSTWOLF_OTHER, "Tauren Tribes");
      Collect(StartX, StartY, _kolkar);
      for (var i = 0; i < Clans.Length; i++)
      {
        Collect(Clans[i].X, Clans[i].Y, Clans[i].Tents);
      }
    }

    // WL's centaur tents around a camp: visible, indestructible until their clan joins
    private static void Collect(float x, float y, List<unit> into)
    {
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, CampRadius, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if (GetOwningPlayer(u) == Player(PLAYER_NEUTRAL_PASSIVE)
            && (t == FourCC("ncnt") || t == FourCC("nct1") || t == FourCC("nct2")))
        {
          SetUnitInvulnerable(u, true);
          into.Add(u);
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

      var centaur = new CentaurFaction();
      SwapFaction.Run(p, centaur);
      LeaveBehind.RemoveHeroes(p, keep);
      LeaveBehind.ToNeutralHostile(p, keep);
      Take(p, _kolkar);
      Spawn.Units(p, StartX, StartY, new[] { CentaurObjects.LehPrah });
      Spawn.Units(p, StartX + 256f, StartY - 256f, Band);
      Levy = new TurnLevy(p, centaur, StartX + 128f, StartY - 384f, CentaurObjects.Outrunner,
        CentaurObjects.Outrunner, CentaurObjects.Archer);
      HeroRevival.Register(p, centaur, StartX, StartY, 60f);
      var overlord = LastConqueror.Of("Tauren Tribes");
      Conquest.Register(p, UNIT_N03M_THUNDERBLUFF, overlord != p ? overlord : null, true);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, ArrivalText);
      }

      p.RepositionCamera(new Point(StartX, StartY));
      AiApi.LogPin("Tauren fallback -> Centaur" + (overlord != null ? ", conqueror P" + I2S(GetPlayerId(overlord))
        : ", no conqueror"));
    }

    // a khan's quest is done: he rides in at his camp, its tents are theirs, the levy grows
    internal static void JoinClan(player p, Clan clan)
    {
      if (p == null)
      {
        return;
      }

      Take(p, clan.Tents);
      Spawn.Units(p, clan.X, clan.Y, new[] { clan.Khan });
      Levy?.Add(clan.Levy);
    }

    private static void Take(player p, List<unit> tents)
    {
      for (var i = 0; i < tents.Count; i++)
      {
        var u = tents[i];
        if (GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
        {
          SetUnitOwner(u, p, true);
          SetUnitInvulnerable(u, true);
        }
      }
    }
  }
}
