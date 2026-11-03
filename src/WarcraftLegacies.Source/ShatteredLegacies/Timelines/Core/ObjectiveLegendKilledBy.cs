using MacroTools.Extensions;
using MacroTools.Legends;
using MacroTools.Quests;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core
{
  // 0.54: a trigger "<legend> is slain by <faction>": complete (for good) when the legend, while it belongs to the
  // quest's faction, is killed by a unit of the named WL faction. Built on WL's LegendaryHero.Died event, like WL's
  // ObjectiveLegendDead.
  internal sealed class ObjectiveLegendKilledBy : Objective
  {
    private readonly string _killerFaction;

    public ObjectiveLegendKilledBy(LegendaryHero legend, string killerFaction, string text)
    {
      _killerFaction = killerFaction;
      SetDescription(text);
      legend.Died += OnDeath;
    }

    private void OnDeath(LegendDiedEventArgs args)
    {
      if (Progress == QuestProgress.Complete || EligibleFactions.Count == 0 || EligibleFactions[0].Player == null)
      {
        return;
      }

      var dying = GetDyingUnit(); // the Died event runs inside WC3's death event (WL's ObjectiveLegendDead does the same)
      var killer = GetKillingUnit();
      if (dying == null || killer == null || dying != args.LegendaryHero.Unit
          || GetOwningPlayer(dying) != EligibleFactions[0].Player)
      {
        return;
      }

      var fac = GetOwningPlayer(killer).GetPlayerData().Faction;
      if (fac != null && fac.Name == _killerFaction)
      {
        Progress = QuestProgress.Complete;
      }
    }
  }
}
