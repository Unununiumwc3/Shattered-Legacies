using System.Collections.Generic;
using MacroTools.Factions;
using MacroTools.DummyCasters;
using WarcraftLegacies.Source.Objectives.ControlPointBased;
using WarcraftLegacies.Source.Objectives.UnitBased;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;
using WCSharp.Events;
using WarcraftLegacies.Shared.ShatteredLegacies.FactionObjectLimits;
using WarcraftLegacies.Source.Setup;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.TwilightHammer;

public sealed class TwilightHammerFaction : Faction
{
  internal const string FactionName = "Twilight's Hammer";

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
    var transports = new ObjectiveCounter("Train three transports at a Twilight Dock", 3);
    PlayerUnitEvents.Register(UnitTypeEvent.FinishesTraining, () =>
    {
      if (@event.Unit.Owner == Player && @event.TrainedUnitType == UNIT_OBOT_HORDE_TRANSPORT_SHIP_WARSONG_FROSTWOLF_FEL_HORDE)
      {
        transports.Add(1);
      }
    }, TwilightHammerObjects.TwilightDock);

    StartingQuest = AddQuest(new QuestTwilightUnlock("Spread the Whispers of the Old God",
      "Send the cult's emissaries across the sea from Dragonmaw Port.", "BTNChogall",
      TwilightHammerObjects.Whispers, "High Priestess Azil joins the Altar of Visions", transports));

    var fortress = AddQuest(new QuestTwilightUnlock("The Cursed Fortress",
      "Secure Grim Batol and the approaches through the Twilight Highlands.", "BTNDragonDungeon",
      TwilightHammerObjects.CursedFortress, "Orcish Death Knights can be trained at the Grim Totem",
      new ObjectiveControlLevel(UNIT_N03X_GRIM_BATOL, 1), new ObjectiveControlLevel(UNIT_N08T_DRAGONMAW_PORT, 1),
      new ObjectiveControlLevel(UNIT_N04V_NORTHERN_HIGHLANDS, 1), new ObjectiveControlLevel(UNIT_N09F_SOUTHERN_HIGHLANDS, 1)));
    AddQuest(new QuestTwilightUnlock("Gift of the Firelord",
      "Take the forges of Ironforge and Blackrock Depths for the Firelord.", "BTNHeroAvatarOfFlame",
      TwilightHammerObjects.Firelord, "Ignacious joins the Altar of Visions",
      new ObjectiveControlLevel(UNIT_N015_IRONFORGE, 1), new ObjectiveControlLevel(UNIT_N024_BLACKROCK_DEPTHS, 1)));
    AddQuest(new QuestTwilightUnlock("Gift of the Windlord",
      "Lead a pilgrimage from the Broken Isles to the seat of the Windlord in Uldum.", "BTNChogall",
      TwilightHammerObjects.Windlord, "Feludius joins the Altar of Visions",
      new ObjectiveControlLevel(UNIT_N032_SURAMAR, 1), new ObjectiveControlLevel(UNIT_N0BD_ULDUM, 1),
      new ObjectiveHeroWithLevelInRect(6, Regions.UldumAmbiance, "Uldum")));
    AddQuest(new QuestHourOfTwilight(fortress));
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
