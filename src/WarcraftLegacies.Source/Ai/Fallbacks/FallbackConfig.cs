using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.39.1: fallback scenarios, first slice (design: Ai/FALLBACKS.md). One definition per faction that has a fallback.
  // Mode "wl"    = WL already does it (Theramore, Scarlet Crusade): only the trigger building shows it, no ability.
  // Mode "spawn" = this system does it: when the faction loses its HQ CP (after having held it), a bot falls back at
  //                once; a human gets the "Begin the Fallback" ability on its trigger building and decides when.
  internal sealed class FallbackDef
  {
    public readonly string Faction;     // WL faction name
    public readonly string Name;        // shown on the trigger building
    public readonly string Mode;        // "wl" or "spawn"
    public int HqCpId;                  // spawn: the CP whose loss enables the fallback
    public float X;                     // spawn: where the fallback starts
    public float Y;
    public int HeroType;                // spawn: hero created at the start spot
    public string Announcement = "";    // spawn: shown to everyone when the fallback happens

    public FallbackDef(string faction, string name, string mode)
    {
      Faction = faction;
      Name = name;
      Mode = mode;
    }
  }

  internal static class FallbackConfig
  {
    internal const int TriggerBuildingType = 1848657474; // n0FB "Fallback" (copy of a CP model, invulnerable, no attack)
    internal const int AbilityId = 1095123505;           // AFB1 "Begin the Fallback" (channel, copy of LLLN)

    // Trigger buildings: the sealed top-right corner (Regions.ArtifactDummyInstance -- WL only creates artifact
    // items there). One per player slot: row = slot / 12, column = slot % 12.
    internal const float GridX = 19800f;
    internal const float GridStep = 250f;
    internal const float Row1Y = 23500f;
    internal const float Row2Y = 23950f;
    internal const float CreateAt = 62f;  // after the faction picks (and where the mode vote used to end)
    internal const float CheckEvery = 5f;

    internal static readonly List<FallbackDef> All = new()
    {
      new FallbackDef("Dalaran", "Theramore (automatic when the Violet Citadel falls)", "wl"),
      new FallbackDef("Lordaeron", "Scarlet Crusade (WL event)", "wl"),
      // PLACEHOLDER (first test of the system): Ironforge loses its Ironforge CP -> Marin Noggenfogger in the
      // Storm Peaks. The real fallback is the Explorers' League (FALLBACKS.md).
      new FallbackDef("Ironforge", "Explorers' League (placeholder: Marin Noggenfogger)", "spawn")
      {
        HqCpId = 1848652085, // Ironforge CP
        X = 3468f,           // Storm Peaks, next to the CP (walkable, checked with map_query)
        Y = 22662f,
        HeroType = 1447571505, // VH01 Marin Noggenfogger, Baron of Gadgetzan
        Announcement = "|cffffcc00Ironforge has fallen.|r Marin Noggenfogger, Baron of Gadgetzan, offers the dwarves "
                       + "his services in the frozen north -- the fight goes on in the Storm Peaks.",
      },
    };
  }
}
