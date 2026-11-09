using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.LegendBased;
using WarcraftLegacies.Source.Shared;
using MacroTools.DummyCasters;
using WarcraftLegacies.Source.Objectives.UnitBased;
using WCSharp.Events;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

public sealed class TwilightHammerFaction : Faction
{
  internal const string FactionName = "Twillight";

  public TwilightHammerFaction()
    : base(FactionName, playercolor.Red, @"ReplaceableTextures\CommandButtons\BTNChogall.blp")
  {
    TraditionalTeam = TeamSetup.OldGods;
    ControlPointDefenderUnitTypeId = UNIT_N0DV_CONTROL_POINT_DEFENDER_NZOTH_TOWER;
    Nicknames = new List<string> { "twilight", "twilightshammer" };
    ProcessObjectInfo(TwilightHammerObjectInfo.GetAllObjectLimits());
  }

  public override void OnRegistered()
  {
    SharedFactionConfigSetup.AddSharedFactionConfig(this);
    PlayerUnitEvents.Register(UnitTypeEvent.Damaging, ThunderFists, TwilightHammerObjects.Chogall);
  }

  // Evaluate territorial objectives only after the old army, control points and alliances have been settled.
  internal void AddCampaignQuests()
  {
    StartingQuest = AddQuest(new QuestTwilightUnlock("Voices in the Quarry",
      "The miners at Dragonmaw Port hear whispers beneath their picks. Study Spiritual Infusion at the War Mill so Azil can interpret them.",
      "BTNChogall", TwilightHammerObjects.Whispers, "High Priestess Azil joins the Altar of Visions",
      new ObjectiveResearch(TwilightHammerObjects.SpiritualInfusion, TwilightHammerObjects.WarMill)));

    var fortress = AddQuest(new QuestCursedFortress());
    var blade = AddQuest(new QuestWindseeker());
    var fire = AddQuest(new QuestTwilightUnlock("Gift of the Firelord",
      "Extinguish Ironforge's Great Forge and claim Blackrock Depths to earn Ignacious's allegiance.",
      "BTNHeroAvatarOfFlame", TwilightHammerObjects.FlameRite, "Ignacious joins the Altar of Visions",
      new ObjectiveControlLevel(UNIT_N024_BLACKROCK_DEPTHS, 1), new ForgeDestroyed()));
    var rime = AddQuest(new QuestTwilightUnlock("Rite of Rime",
      "Train the heralds at the Grim Totem to bind the waters beneath the port. Feludius will rise from the depths.",
      "BTNFrostRevenant2", TwilightHammerObjects.FrostRite, "Feludius joins the Altar of Visions",
      new ObjectiveResearch(TwilightHammerObjects.HeraldTraining, TwilightHammerObjects.GrimTotem)));
    AddQuest(new QuestHourOfTwilight(fortress, blade, fire, rime));
  }

  private sealed class ForgeDestroyed : Objective
  {
    internal ForgeDestroyed()
    {
      SetDescription("The Great Forge is destroyed");
    }

    public override void OnAdd(Faction faction)
    {
      var forge = AllLegends.Ironforge.GreatForge.Unit;
      if (forge != null && forge.Alive)
      {
        var death = trigger.Create();
        death.RegisterUnitEvent(forge, unitevent.Death);
        death.AddAction(() => Progress = QuestProgress.Complete);
      }
      else
      {
        // The fallback may arrive long after Ironforge falls. Award after quest registration.
        timer.Create().Start(0, false, () =>
        {
          Progress = QuestProgress.Complete;
          DestroyTimer(GetExpiredTimer());
        });
      }
    }
  }

  private static void ThunderFists()
  {
    var caster = @event.DamageSource;
    var level = caster.GetAbilityLevel(TwilightHammerObjects.ThunderFists);
    if (@event.IsAttack && level > 0 && GetRandomReal(0f, 1f) < 0.15f)
    {
      DummyCasterManager.GetGlobalDummyCaster().CastUnit(caster, TwilightHammerObjects.ThunderFistsLightning,
        ORDER_FORKED_LIGHTNING, level, @event.Unit, DummyCastOriginType.Caster);
    }
  }
}
