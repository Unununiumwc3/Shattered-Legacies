using MacroTools.Factions;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Shared;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0: events and additions that belong to no single fallback (agreed 2026-11-27): Theramore and Jaina, Kalecgos,
  // the creep bosses, the elite choices, the Kalimdor <-> Northrend crossing, the quilboar plague. The objects:
  // tools/shattered-legacies/content/content_088.py (Objects088.g.cs).
  internal static class EventsSetup
  {
    internal static void Setup()
    {
      EliteChoices.Setup();
      TheramoreJaina.Setup();
      BossDrops.Setup();
      KalimdorCrossing.Setup();
      QuilboarPlague.Setup();
      AiApi.RegisterHeroFamily(Objects088.KalecgosHuman, Objects088.KalecgosDragon); // one hero in two forms
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        AiApi.Safe("the nexus", () =>
        {
          if (FactionManager.TryGetFactionByName("Dalaran", out var dalaran))
          {
            dalaran.AddQuest(new QuestTheNexus(AllLegends.Dalaran.Jaina));
          }
        });
      });
    }
  }
}
