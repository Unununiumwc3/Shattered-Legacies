using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.ShatteredLegacies.Events;
using WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Factions.Defias
{
  // 0.88.0 (maintainer): EDWIN VANCLEEF for 1,500 gold pillaged. Stormwind's nobles never paid the stonemasons who
  // rebuilt their city; every coin the Brotherhood pillages is a coin of the old debt. Counted from the Defias' attacks
  // on buildings (Pillage: half the damage dealt to a building becomes gold -- WC3's stock value).
  internal sealed class QuestStonemasonsDue : QuestData
  {
    private const int Gold = 1500;
    private const float PillageShare = 0.5f;
    private readonly ObjectiveCounter _pillaged;
    private float _sum;

    internal QuestStonemasonsDue() : base("The Stonemasons' Due",
      "The House of Nobles never paid the stonemasons who rebuilt Stormwind. Edwin VanCleef has not forgotten -- "
      + "pillage enough of what the nobles owe and the Defias Kingpin comes to collect the rest himself.",
      @"ReplaceableTextures\CommandButtons\BTNDarkSoldier2.blp")
    {
      _pillaged = new ObjectiveCounter("Gold pillaged", Gold);
      AddObjective(_pillaged);
      var hit = CreateTrigger();
      for (var i = 0; i < 28; i++)
      {
        TriggerRegisterPlayerUnitEvent(hit, Player(i), EVENT_PLAYER_UNIT_DAMAGED, null);
      }

      TriggerAddAction(hit, OnDamage);
    }

    private void OnDamage()
    {
      if (Progress == QuestProgress.Complete)
      {
        return;
      }

      var src = GetEventDamageSource();
      var target = BlzGetEventDamageTarget();
      if (src == null || target == null || !IsUnitType(target, UNIT_TYPE_STRUCTURE)
          || GetUnitAbilityLevel(src, FourCC("Asal")) <= 0
          || GetOwningPlayer(src).GetPlayerData().Faction?.Name != DefiasFaction.FactionName)
      {
        return;
      }

      _sum += GetEventDamage() * PillageShare;
      var whole = R2I(_sum);
      if (whole > 0)
      {
        _sum -= whole;
        _pillaged.Add(whole);
      }
    }

    protected override string RewardDescription => "Edwin VanCleef, the Defias Kingpin, joins you";

    public override string RewardFlavour =>
      "The debt is paid in Stormwind's own gold. Edwin VanCleef steps out of the Deadmines' shadows to lead his "
      + "Brotherhood himself.";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      var x = DefiasFallback.StartX;
      var y = DefiasFallback.StartY;
      var edwin = CreateUnit(p, Objects088.VanCleef, x, y, 270f);
      SetHeroLevel(edwin, 6, false);
      AiApi.LogPin("Defias: Edwin VanCleef joins P" + I2S(GetPlayerId(p)));
    }
  }
}
