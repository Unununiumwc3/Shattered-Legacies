using MacroTools.Extensions;
using MacroTools.Factions;
using MacroTools.Legends;
using MacroTools.Quests;
using WarcraftLegacies.Source.Ai;
using WarcraftLegacies.Source.Objectives;
using WarcraftLegacies.Source.Shared;
using WCSharp.Shared.Data;

namespace WarcraftLegacies.Source.ShatteredLegacies.Events
{
  // 0.88.0 (maintainer): Dalaran's "The Nexus" -- Jaina CHANNELS at the Nexus for one minute (she has to stay there,
  // alive); Jaina and Kalecgos get to know each other a little too well, and Kalecgos joins Dalaran with blue
  // dragonspawn. Kalecgos (Objects088): human form a caster, Dragon Form a great azure dragon that fights in melee.
  internal sealed class QuestTheNexus : QuestData
  {
    internal const float NexusX = -9984f; // the Nexus (h04P) in Coldarra
    internal const float NexusY = 17856f;

    private static readonly int[] Dragonspawn = { FourCC("nbdw"), FourCC("nbdw"), FourCC("nbdw"), FourCC("nbdm"),
      FourCC("nbdm") };

    internal QuestTheNexus(LegendaryHero jaina) : base("The Nexus",
      "Kalecgos of the blue dragonflight does not share Malygos' hatred of mortal magic. If Jaina Proudmoore came to "
      + "the Nexus and spent some time with him -- a whole minute, say -- he might be persuaded to stand with Dalaran.",
      @"ReplaceableTextures\CommandButtons\BTNKalecgos.blp")
    {
      AddObjective(new ObjectiveChannelAt(jaina, NexusX, NexusY, 500f, 60, "the Nexus"));
    }

    public override string RewardFlavour =>
      "For one long minute the Nexus is very quiet. Whatever Jaina and Kalecgos discussed in there, nobody asks -- the "
      + "blushing blue dragon has decided that Dalaran is worth defending after all.";

    protected override string RewardDescription => "Kalecgos (a hero who can take Dragon Form) and 5 blue dragonspawn "
                                                   + "join you at the Nexus";

    protected override void OnComplete(Faction whichFaction)
    {
      var p = whichFaction.Player;
      if (p == null)
      {
        return;
      }

      var k = CreateUnit(p, Objects088.KalecgosHuman, NexusX, NexusY - 300f, 270f);
      SetHeroLevel(k, 5, false);
      for (var i = 0; i < Dragonspawn.Length; i++)
      {
        CreateUnit(p, Dragonspawn[i], NexusX - 256f + 128f * i, NexusY - 450f, 270f);
      }

      AiApi.LogPin("The Nexus: Kalecgos joins P" + I2S(GetPlayerId(p)));
    }
  }

  // a hero stays near a place for a number of seconds without a break (alive, in the quest's faction) -- the bot's
  // quest runner brings it there (IObjectiveVisit)
  internal sealed class ObjectiveChannelAt : Objective, IObjectiveVisit
  {
    private readonly LegendaryHero _hero;
    private readonly float _x;
    private readonly float _y;
    private readonly float _radius;
    private readonly int _seconds;
    private readonly rect _rect;
    private int _held;

    public rect VisitRect => _rect;
    public Legend VisitLegend => _hero;
    public int VisitUnitType => 0;
    public int VisitMinLevel => 0;
    public bool VisitHeroOnly => true;

    internal ObjectiveChannelAt(LegendaryHero hero, float x, float y, float radius, int seconds, string place)
    {
      _hero = hero;
      _x = x;
      _y = y;
      _radius = radius;
      _seconds = seconds;
      _rect = Rect(x - radius / 2f, y - radius / 2f, x + radius / 2f, y + radius / 2f);
      SetDescription(hero.Name + " channels at " + place + " for " + I2S(seconds) + " seconds");
      Position = new Point(x, y);
      DisplaysPosition = true;
      var t = CreateTimer();
      TimerStart(t, 1f, true, () => AiApi.Safe("channel objective", Tick));
    }

    private void Tick()
    {
      if (Progress == QuestProgress.Complete)
      {
        return;
      }

      var u = _hero.Unit;
      var ok = u != null && GetUnitTypeId(u) != 0 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f
               && EligibleFactions.Contains(GetOwningPlayer(u))
               && SquareRoot((GetUnitX(u) - _x) * (GetUnitX(u) - _x) + (GetUnitY(u) - _y) * (GetUnitY(u) - _y))
               <= _radius;
      _held = ok ? _held + 1 : 0;
      if (_held >= _seconds)
      {
        Progress = QuestProgress.Complete;
      }
    }
  }
}
