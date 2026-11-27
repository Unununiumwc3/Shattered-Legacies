using MacroTools.Extensions;

namespace WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Effects
{
  // 0.88.0: a SEAT that is a unit, not a CP (Jaina's Sanctum, the Broodmother). It never dies and never changes its
  // owner: brought to 0 life by an enemy of whoever holds it, it is "taken" -- the attacker's player is its holder now
  // and it is whole again. Core/Conquest asks Holder (Conquest.Register(vassal, _ => seat.Holder, ...)).
  internal sealed class CaptureSeat
  {
    internal unit Unit { get; }
    internal player Holder { get; private set; }
    private readonly string _takenText; // "{0}" = the new holder's faction

    internal CaptureSeat(unit seat, player holder, string takenText = null)
    {
      Unit = seat;
      Holder = holder;
      _takenText = takenText;
      var hit = trigger.Create();
      hit.RegisterUnitEvent(seat, unitevent.Damaged);
      hit.AddAction(() =>
      {
        if (@event.Damage + 1 < seat.Life)
        {
          return;
        }

        var src = @event.DamageSource;
        var who = src != null ? src.Owner : null;
        @event.Damage = 0;
        seat.Life = seat.MaxLife;
        if (who == null || GetPlayerId(who) >= 24 || who == Holder || who == seat.Owner
            || (Holder != null && !IsPlayerEnemy(who, Holder)))
        {
          return; // a neutral, the holder or its friends: nothing changes (and it does not die)
        }

        Holder = who;
        if (_takenText != null)
        {
          var name = who.GetPlayerData().Faction?.Name ?? GetPlayerName(who);
          for (var i = 0; i < 24; i++)
          {
            DisplayTimedTextToPlayer(Player(i), 0, 0, 15, _takenText.Replace("{0}", name));
          }
        }
      });
    }
  }
}
