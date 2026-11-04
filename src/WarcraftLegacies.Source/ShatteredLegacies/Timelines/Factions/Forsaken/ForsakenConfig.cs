namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Forsaken
{
  // Data of the Forsaken (0.54 / 0.55, Quel'thalas' betrayal, Timelines/SCENARIO_CONCEPTS.md P3 / D3 / D4). Their
  // objects are copied from the 24p map (ForsakenObjects.g.cs, tools/shattered-legacies/content/import_24p.py).
  internal static class ForsakenConfig
  {
    internal const string Name = "Forsaken";

    // 0.55 (maintainer): the Altar of the Queen waits preplaced and invulnerable in Northrend; the Forsaken start there
    internal const float StartX = -1340f;
    internal const float StartY = 20171f;

    internal const int SylvanasBanshee = 1431986481; // UZa1 The Banshee (tools/shattered-legacies/content/forsaken_heroes.py)
    internal const int SylvanasDarkRanger = 1431986482; // UZa2 The Dark Ranger
    internal const int SylvanasQueen = 1431986483;   // UZa3 Queen of the Forsaken
    internal const int EtherealWalk = 1096446772;    // AZs4 (Berserk's speed; the code adds Ethereal and phasing)
    internal const int Ethereal = 1097167980;        // Aetl (stock "Ethereal")
    internal const float EtherealWalkDuration = 20f; // Ghostform (0.57, maintainer)
    internal const int BansheeRise = 1381659441;     // RZs1 condition "Sylvanas reaches level 6" (forsaken_heroes.py)
    internal const int RiseLevel = 6;                // 0.57: the other troop buildings (Laboratory, Archive, Harbor)

    // 0.57 (maintainer), Free Will: Sylvanas level 10, the Scourge's "The Culling" done, and the Lich King's grip
    // broken -- the Scourge holds fewer than 10 CPs (while it holds 10, it keeps the Forsaken)
    internal const int ScourgeGripControlPoints = 10;
    internal const string CullingQuest = "The Culling";

    // the Undercity: the eastern unused square of the south-west corner (measured with map_query.py)
    internal const float UndercityMinX = -11896f;
    internal const float UndercityMinY = -30056f;
    internal const float UndercityMaxX = -10008f;
    internal const float UndercityMaxY = -28168f;
    internal const int SylvanasRanger = UNIT_HVWD_RANGER_GENERAL_OF_SILVERMOON_QUELTHALAS; // Quel'thalas' Sylvanas

    // 0.55 (maintainer): the old faction keeps its gold and lumber; the Forsaken start with this (no abuse by dying)
    internal const int StartGold = 500;
    internal const int StartLumber = 0;

    internal static int[] StartUnits = { ForsakenObjects.Acolyte, ForsakenObjects.Acolyte, ForsakenObjects.Acolyte };

    // Sylvanas' phases (maintainer): 1 the Banshee (start) -> level 6: 2 the Dark Ranger, the Forsaken leave the
    // Scourge -> Lordaeron City taken: 3 Queen of the Forsaken. (Phase 0 is Quel'thalas' Ranger-General.) 0.56: one
    // hero type per phase (WC3 cannot change which skills a hero learns).
    internal static int[] PhaseHeroes = { SylvanasRanger, SylvanasBanshee, SylvanasDarkRanger, SylvanasQueen };
    internal const int FreeWillLevel = 10; // 0.57 (maintainer): was 6
  }
}
