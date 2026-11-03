namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // What a turning point is (vocabulary: Timelines/SCENARIO_CONCEPTS.md section 1). Only Fallback is used so far;
  // SubPath, Betrayal and Pick get their engine support in the turning-point core (TODO.md, step 2).
  internal enum TurningPointKind
  {
    Fallback, // second start when the HQ falls
    SubPath,  // voluntary commitment to another direction while the faction still stands
    Betrayal, // the faction (or one of its leaders) turns on its team or allies
    Pick,     // a start choice in WL's faction pick dialog
  }
}
