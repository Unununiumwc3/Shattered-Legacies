# Worker construction — 0.65.0 first playable candidate (not yet verified)

This describes the implementation in `AiBuilder.cs` and `AiBuildSites.cs`. `TODO.md` is the live roadmap. Old
plans in `CONCEPTS.md` and historical changelogs remain useful context. The bot has no dependency on fork classes.

## Responsibilities and lifecycle

- `AiBuilder.Setup` registers WC3 construct-start, construct-finish and construct-cancel events and one
  deferrable `AiScheduler` task, every 8 scheduler slots (2 s). Each scheduled visit services **one bot** in
  rotation; with 16 bots, a project's periodic failure check happens roughly every 32 seconds. One outstanding
  construction project per bot avoids issuing overlapping build orders.
- On a planning visit (at most once per 60 s per bot), enumerate the faction's **Builder** category, actual
  workers and buildings. Try to train workers if fewer than two exist. For perked bots with no viable native
  worker-training order, the *emergency fallback* may place a single worker, never a completed building.
- If the hall is missing, try the faction's TownHall/Keep/Castle categories or a previously observed hall type.
  Recover missing useful troop-producing structures (`AiData.Trains` + faction categories), then an altar.
  One of each useful producer type is the target; already owned structures are counted even when building.
- `IssueBuildOrderById(worker, structureType, x, y)` is the only normal mechanism used to create structures.
  Order refusal tries another distinct site/worker; a type rejected this cycle is cooled down for three minutes.
  A started project stays reserved until WC3 emits a *construct-finish* callback (or cancel/failure/timeout).
  Death, cancellation, lost sites and stale orders trigger a retry elsewhere. On faction replacement any pending
  project is invalidated; the next plan uses the new faction's units and building categories.
- Existing `AiMissions` timed worker-consuming starter kit (0:20 / 3:00) and instantly completed production
  (0:35, successor `GiveStarterProduction`) are removed. The public API call now requests a builder evaluation.
  The bespoke Legion **Argus** base remains a scenario exception but does **not** consume Herald workers.

## HQ and allied control-point fallback

The home coordinate is captured from the faction's first observed town hall, else a worker, else player start.
For a *worker* to construct, `AiBuildSites` chooses:

1. **Original HQ area**, as long as it has an owned hall or supporting owned/allied CP, is safe, and lies on the
   worker's current open walking component.
2. Otherwise, the **nearest reachable safe owned or allied CP** by cost along open walking graph edges. This is
   *not* Euclidean distance, which can cross impassable terrain. The CP is an anchor: the bot still orders its
   own worker to construct a building on open ground around it.
3. With no eligible CP, use the **worker's own vicinity**, provided it is safe. This also covers zero owned CPs.

This policy does not move workers between continents by ferry or portal. If no safe reachable spot exists,
construction is deferred instead of spawning a building into an impassable location.

## Forward production

Owned CPs determine the desired total base count: 0–9 = 1, 10–19 = 2, 20+ = **3 maximum including HQ**.
These are ceilings, not mandatory builds. Perked bots only. `AiRoutes.StagePoint` identifies the main army's
friendly staging node; a BFS of live open walking edges considers owned/allied CPs within one to four hops,
preferably about two hops behind it. Candidates need a worker on the same walking component, a safe site,
and no nearby own hall/forward base. When a new town hall **finishes**, record that forward base and add up to
**two** military production buildings nearby in later construction projects (two total, not per type).

## Placement, costs, and important limitations

- `AiBuildSites.FindSpot` uses 16 directions on rings roughly 800–2400 WC3 world units from the anchor; it
  requires walkable terrain at the centre and conservative 300/420-unit surroundings, no other structure within
  620 units and no non-allied threat nearby. The worker and site must share an open walking component. This is
  **not** a proof that a 32-unit pathing-cell corridor remains after constructing an irregular structure.
- `AiData.Trains` describes *military production* and is not a worker build-menu table. The first implementation
  uses faction categories as candidates and native WC3 order acceptance to decide whether a worker can build
  each type. A higher-quality second pass should generate `ubui` worker->building and `utra` hall->worker tables
  (e.g. dedicated `AiBuildData.g.cs` plus fork-provided data through `AiApi`) if playtests show repeated refusals.
- Perked bots receive a temporary gold/lumber top-up at order time, restored immediately. If the order did
  **not** consume resources, `AiBuilder` refunds the building cost on native construction start. This attempts
  to cover both immediate-charge and delayed-charge versions of WC3. **It is not yet verified**. A delayed
  cost still requires the bot to have enough of its *own* resources on arrival for construction to begin;
  otherwise the project times out. Fair autopilot receives no subsidy. Playtest both timing and zero-gold cases
  before considering this a completed free-building implementation.
- An incomplete structure whose worker dies might need assistance/reassignment; the first version relies on a
  construction timeout and player-tech counts and may not recover every race-specific building behavior.
- Construction data and placement are in `Ai/`. No new hardcoded faction rawcodes or map-specific exception
  classes are introduced.

## Acceptance / playtest matrix

1. **First barracks:** choose a starting bot with a worker but without a military producer. Observe worker
   training where needed, `IssueBuildOrderById`, travel, construct start, finish, and a new army unit produced.
2. **Production destroyed:** destroy one of each producer type and the HQ; confirm recovery. With a surviving
   worker and **zero owned CPs**, compare (a) nearby allied CP, (b) no friendly CP, (c) unsafe location.
3. **No workers:** with an available town hall test worker training; with neither workers nor halls, compare
   perked bot and fair autopilot. Confirm no timed magic building grant returns.
4. **Late faction:** assign Sunfury, Forsaken, Kezan Enterprise and Ashtongue to bot slots, including a faction
   switch while a construction order is in progress. Confirm the new faction builds *its own* building types.
5. **Expansion:** 9→10 and 19→20 CP boundaries, ally CP closer than own, disconnected islands, closed rocks.
   Ensure at most three bases including HQ, and two producers total per new base. Destroy/rebuild a forward hall.
6. **Bad site/build-menu:** water, mountain, narrow road, crowded HQ, enemy defence, building worker that cannot
   construct a particular faction building. Confirm rejected orders back off rather than flooding the scheduler.
7. **Costs and load:** record gold/lumber before issuing, during travel, at actual start and finish; validate that
   perked builds are free while fair builds consume normal resources. Examine `-dump` for `TICK ERROR` and
   operation counts, then use a full multiplayer test with real CSharp.lua map build.

## Next milestones (not included)

Exact build-menu data generation, building-specific footprints/pathing masks and corridor checks, direct rescue
of abandoned incomplete structures, richer construction failure instrumentation, worker transport between islands,
and broader quest/objective-driven construction. Debug commands remain enabled for development and are a 1.0 gate.
