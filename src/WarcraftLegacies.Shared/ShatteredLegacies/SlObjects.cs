namespace WarcraftLegacies.Shared.ShatteredLegacies
{
  // 0.73.0: ids of fork objects that WL code refers to (fork ID rule `?Z<lowercase>?`, ShatteredLegacies/README.md).
  // WL's generated constants (Abilities.g.cs, Upgrades.g.cs, ...) are not edited for fork ids.
  // The four below are WL 5.0.4 objects of the Orcish Horde that the 0.69 port had copied under WL 5's ids, which in
  // this map (WL 4.8.2) already belonged to Ahn'Qiraj and Skywall objects (A000 Shifting Sands, A00Y Frost Bite,
  // R003 Progenesis, RZ02 Shadow Weaver Adept Training) -- those got their 4.8.2 data back (UPSTREAM.md).
  public static class SlObjects
  {
    public const int ABILITY_AZO0_FRENZY_ORCISH_HORDE = 1096445744; // AZo0 (WL 5.0.4 A000)
    public const int ABILITY_AZO1_TELEPORT_SEA_WITCH = 1096445745; // AZo1 (WL 5.0.4 A00Y)
    public const int UPGRADE_RZO0_IMPROVED_FRENZY_ORCISH_HORDE = 1381658416; // RZo0 (WL 5.0.4 R003; needs ofrt)
    public const int UPGRADE_RZO1_BLADEMASTERS_ORCISH_HORDE = 1381658417; // RZo1 (WL 5.0.4 RZ02)
    // 0.74.0: WL 5.0.4's R05A ("Quest Completed: Wrath of the Wild", Siege Ancients +150 life / +10 damage); R05A here
    // is "Quest Completed: The War of the Shifting Sands" (the Druids' C'Thun quest)
    public const int UPGRADE_RZN0_QUEST_COMPLETED_WRATH_OF_THE_WILD_DRUIDS = 1381658160; // RZn0
  }
}
