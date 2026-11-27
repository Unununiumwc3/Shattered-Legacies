-- Run with Lua 5.3 outside Warcraft III: lua startup_check.lua artifacts/war3map.lua
-- Exercise the published type metadata and the handoff to the first in-game timer.
local scheduled = false
local startupTimer = {}
function CreateTimer()
  return startupTimer
end
function TimerStart(timer, delay, periodic, callback)
  assert(timer == startupTimer and delay == 0.01 and not periodic and type(callback) == "function")
  scheduled = true
end

dofile(assert(arg[1], "expected the path to a published war3map.lua"))
-- main also initializes terrain, units and other engine state. Call its real
-- C# initializer directly so the test only needs the two timer natives.
for index = 1, math.huge do
  local name, value = debug.getupvalue(main, index)
  assert(name, "map entry point has no InitCSharp")
  if name == "InitCSharp" then
    value()
    break
  end
end
assert(scheduled, "Program.Main did not schedule game setup")
-- Conquest initializes lazily when a seat first comes into play.
assert(WarcraftLegacies.Source.ShatteredLegacies.Timelines.Core.Conquest.SeatHolders)
print("Map script initialized and scheduled game setup")
