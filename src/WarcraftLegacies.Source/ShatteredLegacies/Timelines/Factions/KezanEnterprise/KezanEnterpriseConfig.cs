namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.KezanEnterprise
{
  // Data of the Kezan Enterprise (Kul Tiras' fallback; 0.39.5-0.39.8 by hand, 0.58 rebuilt on the Forsaken's building
  // blocks). Its objects are the 24p map's goblin faction ("Bilgewater"), copied by
  // tools/shattered-legacies/content/import_24p.py (KezanEnterpriseObjects.g.cs).
  internal static class KezanEnterpriseConfig
  {
    internal const string Name = "Kezan Enterprise"; // 0.39.7: "The ..." was cut off on the scoreboard

    // the start (maintainer: the island is right): Kezan's CP, the base preplaced in Regions.KezanUnlock (Neutral
    // Passive in the map data: Fortress, War Camp, Lumber Mill, Laboratory, Burrows, Missile Towers, the Bilgewater
    // Harbor, an Oil Supply, the Kezan Trading Center, a few mercenaries) -- hidden until the fallback
    internal const float StartX = -662f;
    internal const float StartY = -7410f;

    // the start kit on top of the preplaced base (Spawn): the Altar of Industry and two more Oil Supplies
    internal static int[] StartBuildings =
      { KezanEnterpriseObjects.AltarOfIndustry, KezanEnterpriseObjects.OilSupply, KezanEnterpriseObjects.OilSupply };

    internal static int[] StartUnits =
    {
      KezanEnterpriseObjects.Builder, KezanEnterpriseObjects.Builder, KezanEnterpriseObjects.Builder,
      KezanEnterpriseObjects.Builder,
    };

    // Kul Tiras' lore heir (inheritance rule step 2): its holdings go to Stormwind if Stormwind still plays on its team
    internal const string Heir = "Stormwind";

    // oil (maintainer: an income mechanic of its own). Gold per minute while the building stands (WL's BonusIncome).
    internal const int GoldPerOilSupply = 50;
    internal const int GoldPerOilPlatform = 25;
    internal const int MaxOilPlatforms = 8;           // the 24p limit ("Can only build 8")
    internal const float RigDelayMin = 40f;           // the 24p expedition: an Oil Rig appears after 40-60 s
    internal const float RigDelayMax = 60f;
    internal const float RigRangeMin = 1200f;         // ... at sea, this far from the Oil Supply that sent it
    internal const float RigRangeMax = 5000f;
    internal const float RigSpacing = 700f;           // ... and not next to another platform

    // quests
    internal const int GadgetzanTurns = 15;           // 24p: 1522 s; here counted from the arrival
    internal const int OilRigsForGazlowe = 4;         // 24p "Explosive Engineering"
    internal const int LumberMarketKnowledge = 30;    // 24p gave 30,000 lumber; lumber is knowledge in this map
  }
}
