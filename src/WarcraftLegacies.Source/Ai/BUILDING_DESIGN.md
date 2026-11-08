# Worker construction and altar production — 0.66.0 playtest

## Planning

Starting workers establish a local base. If its supporting hall and friendly CP are lost after construction has begun, a safe owned strategic HQ on the worker's walking component is a recovery option. The timeline catalog supplies HQ types through AiApi. For factions without an override, the bot checks owned essential capitals. The existing friendly CP and local-worker fallbacks still apply. Legion Heralds build on Argus through the same system.

Each pass first tries to restore a hall, military producers, altars, research buildings and the remaining legal worker menu. Ordinary building upgrade paths are considered for missing upgraded types. After first copies, buildings with a unit train list are eligible for a second copy, subject to the faction limit. This includes altars, shipyards and halls which train units. Hall tiers count as one family. A temporarily unavailable order is retried on a later visit. Forward bases still use the CP thresholds and get two local military producers.

A hall with a worker available attempts its next legal tier immediately and for free. A faction with no worker first tries to train one. Regular troops cannot occupy a hall queue needed for recovery or a legal upgrade. Check the current worker's actual build menu, the player's object limits and prerequisites before assigning construction.

Construction, worker/hero training, hall upgrades, research, quest progress and hero revival events wake individual factions. The scheduler alternates queued requests with round-robin visits, servicing one faction per half-second. Each visit can issue up to eight native worker build attempts and probe 384 sites. It can accept one new project. Two projects may be active per faction. A single surviving worker chains projects as construction finishes.

## Placement and costs

The search rotates near and farther rings within 2368 units. Square pathing textures supply footprint widths. Unknown or irregular masks use a 96-unit half-width estimate. Placement reserves a 64-unit lane between estimated footprints and samples an outward walkable exit toward the staging point or anchor. Shipyards look for both water and a reachable shore. The native order validates the building mask. Inspect worker access and newly produced unit movement in the map.

Computer bots and test-mode autopilot receive the normal resource subsidy for workers, structures, upgrades and altar orders. For a worker order, the bot leaves the uncharged portion of the grant available during travel. Regular army training checks its costs against the unreserved balance. Arrival consumes the grant. Failure releases outstanding funds. Native construction cancellation refunds are recovered using the map's 100% refund setting. Fair human autopilot uses its own resources.

Dead eligible legends have priority at idle altars. The bot targets the existing dead hero with the native revival order. Once a hero is available, new recruitment uses the altar's train order. Both use the altar's normal queue and time. Permanent death, ownership and quest restrictions apply.

## Combined playtest

- Start Stormwind and Legion with other bots. Look for a worker-built Nether Pit on Argus and Stormwind's local hall upgrade, Blacksmith and Smithy. At zero gold/wood, watch a free project start while regular troops still require their gold.
- Use one worker, then five. Record the order of first copies, upgrades, and second producers. Cancel one of two concurrent projects during travel and after construction starts. Compare gold/wood and the other worker's orders.
- Kill a revivable hero while an altar is busy. Watch the existing hero enter the next available altar's queue. Unlock a second hero through a quest. Check a permanent death, two altars, an altar destroyed during revival and a faction switch.
- Compare faction-tagged builder and altar messages, the two builder counters and scheduler load in dumps at 3 and 7 minutes. The offline menu test is `python3 tools/shattered-legacies/ai/check_build_data.py`.

## 0.65.0 observed failures and 0.65.1 diagnostics

The first 16-bot playtest produced only four normal builder completions through 12:22. Frostwolf, Illidan and
Skywall built; accepted Archive and Pavilion orders stalled for ~3 minutes before the first version retried.
A key code problem: the original `Safe()` treated neutral passive map units as enemy threats. In the 5-minute
dump, P27 alone owned 587 units and 1234 structures. This could reject HQ anchors and construction spots without
any log. The 0.65.1 fix narrows those exclusions and logs one `builder: <faction> idle:` reason whenever it changes,
or every three minutes, to distinguish missing categories from actual no-space/no-build-menu failures.

The new native idle-order check is a heuristic: in the next test, verify that builders with valid long travel orders
are not incorrectly cancelled. One-project-per-faction remains a throughput bottleneck, and the heuristic cannot
replace generated worker build-menu data. A log with `production satisfied` means the builder believes the faction
needs no HQ recovery; it is not a promise that the faction's unit production itself is functioning.

## Earlier milestones listed for 0.65.1

Exact build-menu data generation, building-specific footprints/pathing masks and corridor checks, direct rescue
of abandoned incomplete structures, richer construction failure instrumentation, worker transport between islands,
and broader quest/objective-driven construction. Debug commands remain enabled for development and are a 1.0 gate.
