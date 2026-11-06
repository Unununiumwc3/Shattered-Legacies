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

    // the start kit on top of the preplaced base (Spawn): the Altar of Industry and (0.61) the Oil Refinery -- was two
    // more Oil Supplies; the preplaced Oil Supply (-1056, -6816) holds the oil now
    internal static int[] StartBuildings = { KezanEnterpriseObjects.AltarOfIndustry, OilRefinery };
    internal const int OilRefinery = 1750757680;      // hZq0 (hand-made, kezan_objects.py)

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

    // oil (0.61 like the 24p; 0.62, maintainer: the oil has to be carried). Every Oil Platform pumps into its own tank
    // (its mana) from a field that runs dry -- then it collapses and a new rig is needed. Oil Tankers carry it home
    // into the Kezan Oil Supply (its mana); the Oil Refinery turns it into gold. Nothing without rigs and tankers.
    // Maxed out (8 platforms, enough tankers): 8 x 625 = 5000 oil per minute -- the refinery takes 1000 of it, the rest
    // is for fuel (mechanical units, Fuel Transports).
    internal const int OilPerPlatform = 625;          // per minute into the platform's tank (3000, data)
    internal const int PlatformField = 40000;         // oil in a platform's field (64 min at full pump)
    internal const int MaxOilPlatforms = 8;           // the 24p limit ("Can only build 8")
    internal const int TankerLoad = 300;              // oil an Oil Tanker carries per trip
    internal const float TankerReach = 700f;          // this close to a platform / the Trading Center: loads / unloads
    // Refine Oil (the refinery's autocast, AZq1): every 60 s RefineOil oil -> RefineGold[Oil Refining level] gold
    internal const int RefineOil = 1000;
    internal static int[] RefineGold = { 100, 150, 200 }; // keep in step with kezan_objects.py (GOLD)
    internal const float RefineEvery = 60f;
    internal const int RefineAbility = 1096446257;    // AZq1
    internal const int OilRefining = 1381657455;      // RZko, 2 levels at the refinery (the second behind Tier 4)

    // the Fuel Transport as a suicide bomber (0.61, maintainer; KezanEnterpriseBombers): Detonate (AZq0, autocast on)
    internal const int DetonateAbility = 1096446256;  // AZq0
    internal const float DetonateDamage = 500f;       // + the vehicle's mana (fuel)
    internal const float DetonateRadius = 1000f;
    internal const float DetonateTrigger = 300f;      // autocast: an enemy this close sets it off
    internal const float RigDelayMin = 40f;           // the 24p expedition: an Oil Rig appears after 40-60 s
    internal const float RigDelayMax = 60f;
    internal const float RigRangeMin = 1200f;         // ... at sea, this far from the Oil Supply that sent it
    internal const float RigRangeMax = 5000f;
    internal const float RigSpacing = 700f;           // ... and not next to another platform

    // trade (0.59, the 24p Trading Center; 0.62: only the Trading Zeppelins trade gold -- the ships are Oil Tankers):
    // Trading Zeppelins pay this much gold per 1,000 of distance
    // of straight-line distance on a round trip (Kezan -> Booty Bay and back ~41,000: a zeppelin ~41 gold in ~4 min,
    // a ship ~82). A full fleet (16 + 10) brings roughly 400 gold per minute.
    internal const int ZeppelinGoldPer1000 = 1;
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
    internal const int SlaveTradeKnowledge = 25;      // ... and knowledge (lumber, the tome; 0.61, maintainer)

    // quests
    internal const int GadgetzanTurns = 15;           // 24p: 1522 s; here counted from the arrival
    internal const int OilRigsForGazlowe = 4;         // 24p "Explosive Engineering"
    internal const int LumberMarketKnowledge = 30;    // 24p gave 30,000 lumber; lumber is knowledge in this map

    // the end goal (concepts section 10, maintainer: on the scoreboard while the Enterprise plays): hold this much gold
    // at once -- the Trade Prince's fortune (KezanEnterpriseGoal)
    internal const int GoalGold = 20000;
  }
}
