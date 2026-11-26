using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Dungeons
{
  // 0.83.0, void-area dungeons (Timelines/VOID_DUNGEONS.md): three 24p dungeons were copied into the void
  // (tools/shattered-legacies/terrain/copy_area.py), each reached through one waygate pair, open from the start:
  //   - Nazjatar     (24p Nazjatar)                         entrance: by WL's Nazjatar CP, beside the Maelstrom
  //   - Shadowforge  (24p Blackrock Depths / Molten Core)   entrance: WL's Shadowforge City (Ironforge's Dark Iron
  //                  base)
  //   - Deadmines    (24p Temple of Ahn'Qiraj, terrain kept for now, to be redecorated)   entrance: Westfall's
  //                  bandit camp
  // Their CPs are preplaced (DungeonObjects, Neutral Hostile, tools/shattered-legacies/content/void_dungeons.py).
  // The Undercity is the fourth, but it stays shut until the Forsaken open it (Factions/Forsaken/ForsakenUndercity).
  // Later: the fallbacks that live there (Sentinels -> naga of Azshara, Ironforge -> Dark Iron, Stormwind -> Defias).
  internal static class VoidDungeons
  {
    internal static Instance Nazjatar;
    internal static Instance Shadowforge;
    internal static Instance Deadmines;

    internal static void Setup()
    {
      // a moment after WL's setup (like the Undercity: nothing it hides or hands out at setup may catch the gates)
      var t = CreateTimer();
      TimerStart(t, 1f, false, () =>
      {
        DestroyTimer(t);
        Build();
      });
    }

    private static void Build()
    {
      // copy rectangles (with their wall); inner gate, arrival; entrance in the world, where the inner gate puts you
      Nazjatar = Make("Nazjatar", new Rectangle(-23168f, -30976f, -15360f, -25600f), -18496f, -29056f, -18496f,
        -29312f, -3744f, -3552f, -3744f, -3808f);
      Shadowforge = Make("Shadowforge", new Rectangle(-24448f, 21120f, -17920f, 24832f), -21472f, 21408f, -21472f,
        21664f, 12480f, -9152f, 12480f, -9408f);
      Deadmines = Make("Deadmines", new Rectangle(-28672f, -31488f, -24448f, -22912f), -27808f, -23904f, -27808f,
        -24160f, 7584f, -16896f, 7584f, -16640f);
    }

    private static Instance Make(string name, Rectangle area, float gateX, float gateY, float arriveX, float arriveY,
      float entranceX, float entranceY, float exitX, float exitY)
    {
      var dungeon = new Instance(name, area, gateX, gateY, arriveX, arriveY);
      dungeon.Prepare(new int[0], new float[0], new float[0]);
      dungeon.Open(Player(PLAYER_NEUTRAL_PASSIVE), entranceX, entranceY, exitX, exitY);
      return dungeon;
    }
  }
}
