using MacroTools.PreplacedWidgets;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Dungeons
{
  // 0.83.0, void-area dungeons (Timelines/VOID_DUNGEONS.md): three 24p dungeons copied into the void
  // (tools/shattered-legacies/terrain/copy_area.py), each reached through one waygate pair:
  //   - Nazjatar     (24p Nazjatar)                         circle by WL's Nazjatar CP beside the Maelstrom
  //   - Shadowforge  (24p Blackrock Depths / Molten Core)   WL's Shadowforge City building (Awrp since 0.84)
  //   - Deadmines    (24p Temple of Ahn'Qiraj, redecorated as a mine in 0.84)   a gold mine in Westfall
  // Their CPs are preplaced (DungeonObjects, Neutral Hostile, tools/shattered-legacies/content/void_dungeons.py).
  // 0.84 (maintainer): SHUT until the fallback that lives there arrives -- the extra CPs' gold would upset the balance
  // otherwise. The gates stand from the start (visible, invulnerable, closed): Nazjatar opens for the naga of Azshara
  // (Sentinels' fallback, Timelines/Factions/Nazjatar), Shadowforge for the Dark Iron (Ironforge's, 0.86), the
  // Deadmines for the Defias (Stormwind's, 0.85, Timelines/Factions/Defias). The Undercity is the fourth:
  // Factions/Forsaken/ForsakenUndercity.
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
      var circle = DungeonObjects.GatewayCircle;
      // copy rectangle (with its wall); inner gate, arrival
      Nazjatar = Make("Nazjatar", new Rectangle(-23168f, -30976f, -15360f, -25600f), circle, -18496f, -29056f,
        -18496f, -29312f);
      Nazjatar.PlaceEntrance(circle, -3744f, -3552f, -3744f, -3808f);

      Shadowforge = Make("Shadowforge", new Rectangle(-24448f, 21120f, -17920f, 24832f), circle, -20640f, 23230f,
        -20640f, 22980f);
      if (AllPreplacedWidgets.Units.TryGetAll(UNIT_H01O_SHADOWFORGE_CITY_IRONFORGE_OTHER, out var cities)
          && cities.Count > 0)
      {
        Shadowforge.UseEntrance(cities[0], 11400f, -9336f);
      }

      Deadmines = Make("Deadmines", new Rectangle(-28672f, -31488f, -24448f, -22912f), circle, -27808f, -23904f,
        -27808f, -24160f);
      Deadmines.PlaceEntrance(DungeonObjects.DeadminesEntrance, 7721f, -17503f, 7721f, -17253f);
    }

    private static Instance Make(string name, Rectangle area, int gateType, float gateX, float gateY, float arriveX,
      float arriveY)
    {
      var dungeon = new Instance(name, area, gateType, gateX, gateY, arriveX, arriveY);
      dungeon.Prepare(new int[0], new float[0], new float[0]);
      return dungeon;
    }

    // a fallback arrives: its dungeon's gates open (the gates stay Neutral Passive)
    internal static void Open(Instance dungeon)
    {
      if (dungeon != null && !dungeon.IsOpen)
      {
        dungeon.Open(Player(PLAYER_NEUTRAL_PASSIVE));
      }
    }
  }
}
