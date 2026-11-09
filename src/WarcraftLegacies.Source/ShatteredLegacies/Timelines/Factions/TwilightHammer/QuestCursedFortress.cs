using MacroTools.Factions;
using MacroTools.PreplacedWidgets;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

internal sealed class QuestCursedFortress : QuestData
{
  internal QuestCursedFortress() : base("The Cursed Fortress",
    "Secure the highland roads. From Grim Batol, Cho'gall can send his Death Knights through the mountain to Dragonmaw Port.",
    @"ReplaceableTextures\CommandButtons\BTNDragonDungeon.blp")
  {
    ResearchId = TwilightHammerObjects.CursedFortress;
    AddObjective(new ObjectiveControlLevel(UNIT_N03X_GRIM_BATOL, 1));
    AddObjective(new ObjectiveControlLevel(UNIT_N08T_DRAGONMAW_PORT, 1));
    AddObjective(new ObjectiveControlLevel(UNIT_N04V_NORTHERN_HIGHLANDS, 1));
    AddObjective(new ObjectiveControlLevel(UNIT_N09F_SOUTHERN_HIGHLANDS, 1));
    AddObjective(new ClaimableFortress());
  }

  protected override string RewardDescription =>
    "Claim Grim Batol, open its tunnels to Dragonmaw Port, and train Orcish Death Knights at the Grim Totem";

  protected override void OnComplete(Faction faction)
  {
    var p = faction.Player;
    var capital = AllLegends.Neutral.GrimBatol;
    if (p == null || capital.Unit == null || !CanClaim(capital.Unit.Owner, p))
    {
      return;
    }
    if (capital.Unit.Owner != p)
    {
      capital.Unit.SetOwner(p);
    }

    // Both existing mouths are at Grim Batol in the base map. Place the lower mouth at the port.
    var tunnels = AllPreplacedWidgets.Units;
    var mountain = tunnels.GetClosest(UNIT_N08R_GRIM_BATOL_TUNNELS, 16761f, -2484f);
    var port = tunnels.GetClosest(UNIT_N08R_GRIM_BATOL_TUNNELS, 16569f, -2772f);
    SetUnitPosition(mountain, 16744f, -2404f);
    SetUnitPosition(port, 22208f, -4032f);
    SetUnitScale(mountain, 1f, 1f, 1f);
    SetUnitScale(port, 1f, 1f, 1f);
    WaygateSetDestination(mountain, 22400f, -4160f);
    WaygateSetDestination(port, 16900f, -2260f);
    WaygateActivate(mountain, true);
    WaygateActivate(port, true);
    capital.ChangedOwner += _ =>
    {
      var open = capital.Unit != null && capital.Unit.Owner == p;
      WaygateActivate(mountain, open);
      WaygateActivate(port, open);
    };
  }

  private static bool CanClaim(player owner, player claimant) =>
    owner == claimant || owner == player.NeutralPassive || owner == player.NeutralAggressive;

  private sealed class ClaimableFortress : Objective
  {
    internal ClaimableFortress()
    {
      SetDescription("Secure the capital of Grim Batol if another player holds it");
    }

    public override void OnAdd(Faction faction)
    {
      var capital = AllLegends.Neutral.GrimBatol;
      void Refresh()
      {
        var p = faction.Player;
        Progress = p != null && capital.Unit != null && capital.Unit.Alive && CanClaim(capital.Unit.Owner, p)
          ? QuestProgress.Complete : QuestProgress.Incomplete;
      }
      capital.ChangedOwner += _ => Refresh();
      // QuestData.Add evaluates initial objectives before Faction subscribes to completion rewards.
      var firstCheck = CreateTimer();
      TimerStart(firstCheck, 0f, false, () =>
      {
        DestroyTimer(firstCheck);
        Refresh();
      });
    }
  }
}
