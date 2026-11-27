namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.89.1: a Conquest seat that is not a CP -- whoever holds it now (null = nobody). An interface on purpose: a
  // Func of player here broke the map's setup (CSharp.lua delegate collision with WL's Action of player).
  internal interface ISeatHolder
  {
    player Holder { get; }
  }
}
