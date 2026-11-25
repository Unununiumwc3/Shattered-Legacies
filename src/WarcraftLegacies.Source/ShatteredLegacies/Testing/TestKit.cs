using System.Collections.Generic;
using MacroTools.Legends;
using MacroTools.PreplacedWidgets;
using WCSharp.Shared.Data;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Testing
{
  // 0.72.0: "-testkit [kits|items|foes]" -- a debug command (AiApi.RegisterDebugCommand: only where the bot's
  // control commands work, never in a release build). Spawns, beside the typing player's camera, what a content port
  // changed, so one short session checks it all instead of a full game:
  //   kits  : the heroes / units whose abilities were ported from WL 5.0.4 (level 10, full mana) -- default
  //   items : the new / changed artifacts
  //   foes  : 8 Neutral Hostile footmen to cast on
  //   dalaran (0.73.0, bundle 2): the Power Generator is handed to the typing player with Antonidas beside it
  //           (Dalaran Shield), two Farmers + 1500 gold / 600 lumber to build the two Way Gates, Teleportation
  //           researched and three Geomancers (caster teleport)
  // Update the lists with every port bundle (ShatteredLegacies/UPSTREAM.md names the bundle).
  public static class TestKit
  {
    private const int HeroLevel = 10;

    public static void Setup()
    {
      AiApi.RegisterDebugCommand("-testkit", OnCommand);
    }

    // bundle 1 (0.72.0): hero kits whose buttons did nothing in 0.69-0.71, plus Malfurion's WL 5.0.4 kit
    private static List<LegendaryHero> Heroes()
    {
      return new List<LegendaryHero>
      {
        AllLegends.Sentinels.Tyrande,   // Lunar Sanctuary; Bow of Elune's Grace
        AllLegends.Sentinels.Naisha,    // Glaive Trap, Watcher's Focus
        AllLegends.Quel.Anasterian,     // Sunfire Barrage
        AllLegends.Scourge.Rivendare,   // Corruption Mine
        AllLegends.Scourge.Kelthuzad,   // Soulseeker
        AllLegends.Druids.Cenarius,     // Tree of Renewal, Lord of the Forest, Master of Nature progression
        AllLegends.Druids.Malfurion,    // Emerald Dream, Seed of Rebirth
        AllLegends.Dalaran.Antonidas,   // Arcane Cataclysm
        AllLegends.FelHorde.Teron       // Gorefiend's Truncheon
      };
    }

    private static readonly int[] Units =
    {
      UNIT_H04L_PRIESTESS_OF_THE_MOON_SENTINELS_ELITE, // Light of the Goddess, Mass Wind Walk, Elune's Protection, Moonlit Arrows
      UNIT_H045_WARDEN_SENTINELS_ELITE,                // Spirit of Vengeance
      UNIT_E03H_HEARTWOOD_ANCIENT_DRUIDS_ELITE,        // Undying Grove, Spirits of the Forest, Root Tap
      UNIT_E00N_KEEPER_OF_THE_GROVE_DRUIDS_ELITE       // Living Wall
    };

    private static readonly int[] Items =
    {
      ITEM_I026_SOULSEEKER, ITEM_I027_GOREFIEND_S_TRUNCHEON, ITEM_I028_BOW_OF_ELUNE_S_GRACE, Timelines.Factions.TwilightHammer.QuestWindseeker.HammerOfTwilight
    };

    private static void OnCommand()
    {
      var p = GetTriggerPlayer();
      var msg = GetEventPlayerChatString();
      var what = msg.Length > 9 ? msg.Substring(9) : "kits";
      var x = GetCameraTargetPositionX();
      var y = GetCameraTargetPositionY();
      if (what == "dalaran")
      {
        Dalaran(p, x, y);
      }
      else if (what == "items")
      {
        for (var i = 0; i < Items.Length; i++)
        {
          CreateItem(Items[i], x + 120f * i, y - 200f);
        }
      }
      else if (what == "foes")
      {
        for (var i = 0; i < 8; i++)
        {
          CreateUnit(Player(PLAYER_NEUTRAL_AGGRESSIVE), FourCC("hfoo"), x + 600f + 80f * (i % 4), y + 80f * (i / 4), 180f);
        }
      }
      else
      {
        var heroes = Heroes();
        for (var i = 0; i < heroes.Count; i++)
        {
          var type = heroes[i]?.UnitType ?? 0;
          if (type == 0)
          {
            continue;
          }

          var u = CreateUnit(p, type, x + 150f * (i % 5), y + 150f * (i / 5), 270f);
          if (IsUnitType(u, UNIT_TYPE_HERO))
          {
            SetHeroLevel(u, HeroLevel, false);
          }

          SetUnitState(u, UNIT_STATE_MANA, GetUnitState(u, UNIT_STATE_MAX_MANA));
        }

        for (var i = 0; i < Units.Length; i++)
        {
          var u = CreateUnit(p, Units[i], x + 150f * i, y - 300f, 270f);
          SetUnitState(u, UNIT_STATE_MANA, GetUnitState(u, UNIT_STATE_MAX_MANA));
        }
      }

      DisplayTimedTextToPlayer(p, 0, 0, 10, "-testkit " + what + ": spawned (kits = WL 5.0.4 hero kits, items = artifacts, foes = 8 hostile footmen, dalaran = shield, Way Gates, caster teleport)");
    }

    private static void Dalaran(player p, float x, float y)
    {
      if (AllPreplacedWidgets.Units.TryGetAll(UNIT_N0DS_POWER_GENERATOR, out var gens) && gens.Count > 0)
      {
        var gen = gens[0];
        ShowUnit(gen, true);
        SetUnitOwner(gen, p, true);
        AllLegends.Dalaran.Antonidas.ForceCreate(p, new Point(GetUnitX(gen) + 120f, GetUnitY(gen) - 120f), 90f);
        SetUnitState(AllLegends.Dalaran.Antonidas.Unit, UNIT_STATE_MANA, GetUnitState(AllLegends.Dalaran.Antonidas.Unit, UNIT_STATE_MAX_MANA));
      }

      for (var i = 0; i < 2; i++)
      {
        CreateUnit(p, UNIT_H022_FARMER_DALARAN_WORKER, x + 150f * i, y, 270f);
      }

      SetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD, GetPlayerState(p, PLAYER_STATE_RESOURCE_GOLD) + 1500);
      SetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER, GetPlayerState(p, PLAYER_STATE_RESOURCE_LUMBER) + 600);
      SetPlayerTechResearched(p, UPGRADE_RDTP_TELEPORTATION_DALARAN, 1);
      for (var i = 0; i < 3; i++)
      {
        var u = CreateUnit(p, UNIT_H02D_GEOMANCER_DALARAN, x + 150f * i, y - 300f, 270f);
        SetUnitState(u, UNIT_STATE_MANA, GetUnitState(u, UNIT_STATE_MAX_MANA));
      }
    }
  }
}
