using MacroTools.Artifacts;
using MacroTools.Extensions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Events;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Furbolg
{
  // 0.88.0 (maintainer): XAVIUS, PEROTH'ARN AND THE CLAW OF URSOC. When the Druids fall, the satyrs come out of the
  // Nightmare with the furbolgs: Xavius (demi-hero, holds the Claw of Ursoc) and Peroth'arn (demi-hero), both the
  // Furbolgs' units in Timbermaw Hold.
  // - The Furbolgs are their OWN faction (a team of their own) while Xavius lives or while the Claw is theirs.
  // - The first time the Claw leaves them (picked up by another player's unit -- Xavius slain, or the Claw handed
  //   over), they serve the Claw's holder's team, and Xavius and Peroth'arn turn Neutral Hostile for good: the satyrs
  //   no longer rule the furbolgs (that also ends a hand-over trick). From then on the Claw is their seat: whoever
  //   carries it is their master; the Furbolgs carrying it themselves change nothing (they keep their team).
  // - Never defeated. No bot hook: bots fight the satyrs as any enemy once they are hostile.
  internal static class FurbolgClaw
  {
    private static unit _xavius;
    private static unit _perotharn;
    private static Artifact _claw;
    private static bool _lost;

    internal static void Start(player p, player conqueror)
    {
      Conquest.Undefeatable.Add(p);
      p.GetPlayerData().Faction?.Unally(); // a team of its own
      SplitFaction.UnallyOutsideTeam(p);
      var x = FurbolgFallback.ShrineX;
      var y = FurbolgFallback.ShrineY;
      _xavius = CreateUnit(p, Objects088.Xavius, x - 256f, y - 256f, 270f);
      _perotharn = CreateUnit(p, Objects088.Perotharn, x + 256f, y - 256f, 270f);
      var it = CreateItem(Objects088.ClawOfUrsoc, x, y);
      _claw = new Artifact(it);
      ArtifactManager.Register(_claw);
      UnitAddItem(_xavius, it);
      var t = CreateTimer();
      TimerStart(t, 1f, true, () => AiApi.Safe("claw of ursoc", () => Check(p)));
      AiApi.LogPin("Furbolgs: Xavius holds the Claw of Ursoc (conqueror "
                   + (conqueror != null ? "P" + I2S(GetPlayerId(conqueror)) : "none") + ")");
    }

    // whoever carries the Claw (a living unit), or null
    private static player Holder()
    {
      var u = _claw?.OwningUnit;
      return u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f ? GetOwningPlayer(u) : null;
    }

    private static void Check(player p)
    {
      if (_lost)
      {
        return;
      }

      var holder = Holder();
      if (holder == null || holder == p || GetPlayerId(holder) >= 24)
      {
        return; // Xavius (or another furbolg) has it, or it lies on the ground
      }

      _lost = true;
      Hostile(_xavius);
      Hostile(_perotharn);
      Conquest.Register(p, _ => Holder(), holder, true);
      var name = holder.GetPlayerData().Faction?.Name ?? GetPlayerName(holder);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, "|cffffcc00The Claw of Ursoc is taken!|r " + name
          + " holds the furbolgs' sacred totem now -- the tribes bow to it, and the satyrs who ruled them are cast out.");
      }

      AiApi.LogPin("Furbolgs: the Claw is lost to " + name);
    }

    private static void Hostile(unit u)
    {
      if (u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f)
      {
        SetUnitOwner(u, Player(PLAYER_NEUTRAL_AGGRESSIVE), true);
      }
    }
  }
}
