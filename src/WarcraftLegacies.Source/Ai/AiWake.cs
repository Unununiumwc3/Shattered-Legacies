using System.Collections.Generic;

namespace WarcraftLegacies.Source.Ai
{
  // 0.48.5: WAKE SLEEPING CREEPS at the bot's target. At night WC3 creeps sleep, and units never auto-acquire a
  // sleeping unit: an army that attack-moved to a creep camp stood next to it until day came (0.48.4 game: the
  // Scourge's 85 units at a Mammut camp in Borean Tundra from 6:06 to 10:06). Once MinUnits of the bot's army (the
  // theatre being serviced) stand within Radius of the target, every sleeping Neutral Hostile unit within Radius of it
  // is woken (UnitWakeUp) -- the creeps attack, the army fights back. No orders are given: movement is unchanged.
  // "-wake on|off". Deterministic (synced unit state only).
  internal static class AiWake
  {
    internal static bool On = true;
    private const float Radius = 900f;
    private const int MinUnits = 3;
    internal static int Woken;
    internal static int Checks;
    internal static int[] WokenBy = new int[24];

    // after a target decision; (x, y) = the target unit's position
    internal static void Near(player p, int pid, float x, float y)
    {
      if (!On || pid < 0 || pid >= 24)
      {
        return;
      }

      var units = AiArmy.Units;
      var n = 0;
      for (var i = 0; i < units.Count && n < MinUnits; i++)
      {
        var dx = GetUnitX(units[i]) - x;
        var dy = GetUnitY(units[i]) - y;
        if (dx * dx + dy * dy <= Radius * Radius)
        {
          n++;
        }
      }

      if (n < MinUnits)
      {
        return;
      }

      Checks++;
      var woke = 0;
      var name = "";
      var g = CreateGroup();
      GroupEnumUnitsInRange(g, x, y, Radius, null);
      unit u;
      while ((u = FirstOfGroup(g)) != null)
      {
        GroupRemoveUnit(g, u);
        SimpleBot.OpUnits++;
        if (GetPlayerId(GetOwningPlayer(u)) == 24 && GetUnitState(u, UNIT_STATE_LIFE) > 0.405f && UnitIsSleeping(u))
        {
          UnitWakeUp(u);
          woke++;
          name = GetUnitName(u);
        }
      }

      DestroyGroup(g);
      if (woke > 0)
      {
        Woken += woke;
        WokenBy[pid] = WokenBy[pid] + woke;
        SimpleBot.Log("P" + I2S(pid) + " " + SimpleBot.FacName(p) + ": woke " + I2S(woke) + " sleeping creep(s) ("
                      + name + ") @" + I2S(R2I(x)) + "," + I2S(R2I(y)));
      }
    }

    internal static void OnCommand()
    {
      var msg = GetEventPlayerChatString();
      if (msg == "-wake on")
      {
        On = true;
      }
      else if (msg == "-wake off")
      {
        On = false;
      }

      DisplayTimedTextToPlayer(GetTriggerPlayer(), 0, 0, 8, "wake sleeping creeps at the target (army within "
        + I2S(R2I(Radius)) + "): " + (On ? "on" : "off") + "  -- -wake on|off");
    }

    internal static void Lines(List<string> o)
    {
      var s = "wake (0.48.5) " + (On ? "on" : "off") + ": sleeping creeps woken " + I2S(Woken) + " in " + I2S(Checks)
              + " checks; per bot:";
      for (var pid = 0; pid < 24; pid++)
      {
        if (WokenBy[pid] > 0)
        {
          s = s + " P" + I2S(pid) + " " + I2S(WokenBy[pid]) + ";";
        }
      }

      o.Add(s);
    }
  }
}
