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

    // where the Builders appear (0.60: was StartX + 200 / StartY - 200 -- a cliff pocket they could not leave; this
    // spot is open ground inside the base, found offline on the pathing map: walkable, joined to the base)
    internal const float UnitsX = -646f;
    internal const float UnitsY = -6784f;

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
    // the upgradeable refinery (maintainer): Oil Refining (RZko, 3 levels at the Oil Supply; data written by
    // tools/shattered-legacies/content/kezan_objects.py -- keep its tooltip numbers in step) adds per level
    internal const int OilRefining = 1381657455;      // RZko
    internal const int RefiningPerSupply = 10;
    internal const int RefiningPerPlatform = 5;
    internal const float RigDelayMin = 40f;           // the 24p expedition: an Oil Rig appears after 40-60 s
    internal const float RigDelayMax = 60f;
    internal const float RigRangeMin = 1200f;         // ... at sea, this far from the Oil Supply that sent it
    internal const float RigRangeMax = 5000f;
    internal const float RigSpacing = 700f;           // ... and not next to another platform

    // trade (0.59, the 24p Trading Center): Trading Zeppelins / Traders pay this much gold per 1,000 of distance
    // of straight-line distance on a round trip (Kezan -> Booty Bay and back ~41,000: a zeppelin ~41 gold in ~4 min,
    // a ship ~82). A full fleet (16 + 10) brings roughly 400 gold per minute.
    internal const int ZeppelinGoldPer1000 = 1;
    internal const int TraderGoldPer1000 = 2;
    internal const float TradeCheckEvery = 2f;
    internal const float TradeLegMax = 240f;          // a leg longer than this counts as arrived (stuck)

    // the goblin towns they trade with: x, y of the town's CP (zeppelins), x, y of its harbour (ships) -- harbours: the
    // nearest deep-water spot reachable by sea from Kezan (found offline on mapdata/.../PathingMap, 0.59)
    internal static float[] TradeTowns =
    {
      10368f, -22976f, 10060f, -23822f, // Booty Bay (n00L)
      -10624f, -3968f, -9781f, -4675f,  // Ratchet (n0A6)
      -8832f, -13184f, -7986f, -12876f, // Gadgetzan (n05C)
      2688f, 16448f, 1936f, 16174f,     // Venture Bay (n07Y)
      19200f, -9984f, 19650f, -10520f,  // Fuselight (n01E)
      -9856f, 7296f, -7394f, 7730f,     // Winterspring (n01X)
    };

    // the Refugee Slave Camps (0.60, KezanEnterpriseSlaveCamps): stock per refugee unit, one back every ..., and the
    // object limit every faction gets for each refugee unit
    internal const int CampStock = 3;
    internal const float CampRestockEvery = 120f;
    internal const int RefugeeLimit = 12;
    internal const string GallywixTitle = "Master of the Slave Trade"; // the reward (QuestMasterOfTheSlaveTrade)

    // quests
    internal const int GadgetzanTurns = 15;           // 24p: 1522 s; here counted from the arrival
    internal const int OilRigsForGazlowe = 4;         // 24p "Explosive Engineering"
    internal const int LumberMarketKnowledge = 30;    // 24p gave 30,000 lumber; lumber is knowledge in this map

    // the end goal (concepts section 10, maintainer: on the scoreboard while the Enterprise plays): hold this much gold
    // at once -- the Trade Prince's fortune (KezanEnterpriseGoal)
    internal const int GoalGold = 20000;
  }
}
