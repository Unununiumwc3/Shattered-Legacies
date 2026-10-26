using System.Collections.Generic;

namespace WarcraftLegacies.CLI.Settings;

public sealed class MapSettings
{
  /// <summary>
  /// The current version of the map that players can see, e.g. "2.4.0".
  /// </summary>
  public string Version { get; init; }

  /// <summary>
  /// Optional file names for published maps, keyed by map name (e.g. "WarcraftLegacies").
  /// When a map has an entry, the published file is called "{file name}-{version}.w3x" instead of
  /// "{map name}{version}.w3x". File names must not contain characters Windows forbids, such as ':'.
  /// </summary>
  public Dictionary<string, string> PublishedFileNames { get; init; } = new();
}
