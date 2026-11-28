-- From repo root: lua tools/shattered-legacies/ai/portal_sweep_check.lua artifacts/war3map.lua
-- Exercise the published discovery callbacks with a stable world and with live gate changes.
dofile("tools/shattered-legacies/ai/startup_check.lua")

local ai = WarcraftLegacies.Source.Ai
local routes = ai.AiRoutes
local tasks, owners, gates = {}, {}, {}
local publications, cleanups = 0, 0
ai.SimpleBot = {WaveOps = 0}
ai.AiScheduler = {Add = function(name, period, phase, run)
  if name:match("^route portals") then
    assert(period == 120)
    tasks[phase] = run
  end
end}
ai.AiPortals = {
  MarkerOwner = function() return -1 end,
  Watch = function() end,
  AddStatic = function() publications = publications + 1 end,
  Clean = function() cleanups = cleanups + 1 end
}
I2S = tostring
Player = function(id) return id end
GetPlayerId = function(id) return id end
GetOwningPlayer = function(u) return u.owner end
GetUnitTypeId = function(u) return u.id end
GetUnitState = function(u) return u.life end
GetUnitX = function(u) return u.x end
GetUnitY = function(u) return u.y end
WaygateIsActive = function(u) return u.active end
WaygateGetDestinationX = function(u) return u.dx end
WaygateGetDestinationY = function(u) return u.dy end
IsTerrainPathable = function() return false end
FourCC = function(s) return string.unpack(">I4", s) end
CreateGroup = function() return {} end
DestroyGroup = function() end
GroupEnumUnitsOfPlayer = function(g, owner)
  owners[#owners + 1] = owner
  for _, u in ipairs(gates) do
    if u.owner == owner then g[#g + 1] = u end
  end
end
FirstOfGroup = function(g) return g[#g] end
GroupRemoveUnit = function(g, u) assert(table.remove(g) == u) end

local data = ai.AiRouteData
local function gate(owner)
  return {owner = owner, id = 1, life = 100, active = true,
          x = data.NodeX:get(0), y = data.NodeY:get(0),
          dx = data.NodeX:get(1), dy = data.NodeY:get(1)}
end
local moving, closing, dying, removed, neutral = gate(1), gate(2), gate(3), gate(4), gate(27)
gates = {moving, closing, dying, removed, neutral}
routes.Setup()
for phase = 16, 19 do tasks[phase]() end
assert(routes.getPortalEdges() == 5)
assert(#owners == 28)
for i, owner in ipairs(owners) do assert(owner == i - 1) end

tasks[16]()
moving.dx, moving.dy = data.NodeX:get(2), data.NodeY:get(2)
closing.active, dying.life, removed.id = false, 0, 0
for phase = 17, 18 do tasks[phase]() end
assert(routes.getPortalEdges() == 5 and publications == 1)
tasks[19]()
assert(routes.getPortalEdges() == 2)
assert(routes.PortalGate(0) == moving and routes.PortalExitX(0) == moving.dx and routes.PortalExitY(0) == moving.dy)
assert(routes.PortalGate(1) == neutral and publications == 2 and cleanups == 2)
print("Portal batches, live destinations, stale gates and complete graph publication passed")
