using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer
{
  // 0.89.0 (maintainer): THE OLD PACT -- Twilight and the Dragonmaw while the clan is no player yet. Both were clans of
  // the old Horde, and the Dragonmaw served the Twilight's Hammer in Grim Batol. Twilight takes the Dragonmaw Port CP ->
  // Zuluhed honours the pact: 2 Dragonmaw Dragon Riders join Twilight every 3 turns while Twilight holds the port and
  // the clan is not in play. The clan's own base stays theirs: hidden and invulnerable until the Dragonmaw rise
  // (Twilight's fallback).
  internal sealed class QuestOldPact : QuestData
  {
    private const float PortX = 21312f; // the Dragonmaw Port CP (n08T)
    private const float PortY = -3200f;

    internal QuestOldPact() : base("The Old Pact",
      "The Dragonmaw and the Twilight's Hammer marched together in the old Horde, and Zuluhed's clan once served the "
      + "cult in Grim Batol. Take Dragonmaw Port and the old pact holds again.",
      @"ReplaceableTextures\CommandButtons\BTNRedDragonDevour.blp")
    {
      AddObjective(new ObjectiveControlPoint(UNIT_N08T_DRAGONMAW_PORT));
    }

    protected override string RewardDescription => "2 Dragonmaw Dragon Riders every 3 turns while you hold Dragonmaw Port";

    public override string RewardFlavour => "Zuluhed honours the old pact: the Dragonmaw's riders fly for the cult again.";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, () => AiApi.Safe("old pact", () =>
      {
        var cp = AiApi.SafeCp(UNIT_N08T_DRAGONMAW_PORT);
        if (cp == null || cp.Unit == null || GetOwningPlayer(cp.Unit) != p || ClanInPlay())
        {
          return;
        }

        CreateUnit(p, DragonmawObjects.DragonRider, PortX, PortY - 256f, 270f);
        CreateUnit(p, DragonmawObjects.DragonRider, PortX + 128f, PortY - 256f, 270f);
      }), 3, () => p.GetPlayerData().Faction == whichFaction);
    }

    private static bool ClanInPlay()
    {
      for (var i = 0; i < 24; i++)
      {
        if (Player(i).GetPlayerData().Faction?.Name == DragonmawFaction.FactionName)
        {
          return true;
        }
      }

      return false;
    }
  }
}
