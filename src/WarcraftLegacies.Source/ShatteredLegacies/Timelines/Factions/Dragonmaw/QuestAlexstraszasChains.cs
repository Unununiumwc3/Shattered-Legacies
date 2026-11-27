using System.Collections.Generic;
using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.GameTime;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives.ControlPointBased;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Dragonmaw
{
  // 0.88.0 (agreed): ALEXSTRASZA'S CHAINS. The Dragonmaw hold Grim Batol -> the chained red dragons hatch a Red Drake
  // for them every turn while they keep it. High reward, real risk: the day Nek'rosh is slain, Alexstrasza breaks her
  // chains -- no more drakes, and every red dragon of the clan turns on it (Neutral Hostile).
  internal sealed class QuestAlexstraszasChains : QuestData
  {
    private const float GrimBatolX = 17344f; // the Grim Batol CP (n03X)
    private const float GrimBatolY = -3456f;
    private static readonly int RedDrake = FourCC("nrdr");
    private readonly List<unit> _drakes = new();
    private bool _broken;

    internal QuestAlexstraszasChains() : base("Alexstrasza's Chains",
      "Deep in Grim Batol the Dragonqueen lies in chains, and her brood is the Dragonmaw's to command -- as long as "
      + "the clan holds the fortress, and as long as Nek'rosh holds the chains.",
      @"ReplaceableTextures\CommandButtons\BTNRedDragon.blp")
    {
      AddObjective(new ObjectiveControlPoint(UNIT_N03X_GRIM_BATOL));
    }

    protected override string RewardDescription => "A Red Drake every turn while you hold Grim Batol -- until "
                                                   + "Nek'rosh is slain";

    public override string RewardFlavour => "The chains of Grim Batol are the Dragonmaw's again. The red brood hatches "
                                            + "for its masters.";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      GameTimeManager.RegisterOnTurnRepeating(GameTimeManager.Turn + 1, () => AiApi.Safe("dragonmaw chains", () =>
      {
        var cp = AiApi.SafeCp(UNIT_N03X_GRIM_BATOL);
        if (!_broken && cp != null && cp.Unit != null && GetOwningPlayer(cp.Unit) == p)
        {
          _drakes.Add(CreateUnit(p, RedDrake, GrimBatolX, GrimBatolY - 256f, 270f));
        }
      }), 1, () => !_broken && p.GetPlayerData().Faction == whichFaction);

      var death = CreateTrigger();
      TriggerRegisterPlayerUnitEvent(death, p, EVENT_PLAYER_UNIT_DEATH, null);
      TriggerAddAction(death, () =>
      {
        if (_broken || GetUnitTypeId(GetTriggerUnit()) != DragonmawObjects.Nekrosh)
        {
          return;
        }

        _broken = true;
        AiApi.Safe("alexstrasza free", () => Break(p));
      });
    }

    private void Break(player p)
    {
      var g = CreateGroup();
      GroupEnumUnitsOfPlayer(g, p, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        var t = GetUnitTypeId(u);
        if (t == RedDrake || t == DragonmawObjects.AncientRedDragon)
        {
          SetUnitOwner(u, Player(PLAYER_NEUTRAL_AGGRESSIVE), true);
        }
      }

      DestroyGroup(g);
      for (var i = 0; i < 24; i++)
      {
        DisplayTimedTextToPlayer(Player(i), 0, 0, 20, "|cffff0000Nek'rosh is slain!|r Alexstrasza breaks her chains -- "
          + "and the red dragons turn on their tormentors.");
      }

      AiApi.LogPin("Dragonmaw: Alexstrasza breaks free");
    }
  }
}
