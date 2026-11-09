using War3Api.Object;
using War3Net.Build;
using War3Net.CodeAnalysis.Jass.Extensions;
using Warcraft.Cartographer.Migrations;

namespace WarcraftLegacies.CLI.Migrations;

/// <summary>
/// Sets the Art Portrait Model File to an empty string except Grim Batol, which uses a separate stock portrait.
/// </summary>
public sealed class PortraitModelFileMapMigration : IMapMigration
{
  public void Migrate(Map map, ObjectDatabase objectDatabase)
  {
    foreach (var unit in objectDatabase.GetUnits())
    {
      if (unit.Key != UNIT_H01Z_GRIM_BATOL_CREEP_TWILIGHT.InvertEndianness())
      {
        unit.ArtPortraitModelFile = "";
      }
    }

    var unitData = objectDatabase.GetAllData().UnitData;
    map.UnitObjectData = unitData;
    map.UnitSkinObjectData = unitData;
  }
}
