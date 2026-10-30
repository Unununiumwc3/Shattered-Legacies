# What the bot needs outside `Ai/` -- hooks and workarounds

Goal: a pull request of the bot to the original Warcraft Legacies that works on the original map without
workarounds. This file lists every change to the original code or map the bot depends on, and every change the bot
makes to the map while the game runs. Keep it current in the same drop that adds or removes such a change.

Markers in the code (search for them): `// [BOT]` = hook the bot needs, `// [SL]` = Shattered Legacies change
(not needed by the bot; listed in `ShatteredLegacies/README.md`), `// [WORKAROUND]` = temporary, listed below.
Map data (JSON) cannot hold comments -- it is only listed here.

"Blocks PR" = the bot would not work on the original map without it, or the original maintainer would have to
accept a change to his map / game.

## 1. Hooks (good solution, part of the PR)

| What | Where | Why | Blocks PR |
|---|---|---|---|
| `AiSetup.Setup()` call | `Setup/GameSetup.cs` `[BOT]`, before `FactionChoiceDialogSetup` | starts the bot | no -- the one line the PR adds |
| Bot pick in click menus | `MacroTools/UserInterface/BotChoicePolicy.cs` (new) + `ChoiceDialogPresenter.TryBotPick` `[BOT]` | bots used to get option 1 after 24 s | no (null = WL's behaviour) |
| AI takes over leaving humans | `GameLogic/GameEnd/PlayerLeaves.cs` `[BOT]` | the game stays alive; falls back to WL's defeat when the AI declines | no, but a game-design change -- ask upstream (could be an option) |
| `%` in `Loc.Format` | `MacroTools/Localization/Loc.cs` `[BOT]` | WL 4.8.2 bug: `%` in a replacement crashes quest rewards in Lua | no -- plain bug fix, send upstream as its own PR (upstream 5.x already has it) |

## 2. Workarounds (no good solution yet)

| # | What | Where | Why | Proper solution | Blocks PR |
|---|---|---|---|---|---|
| W1 | 1-gold CPs Sunsail Anchorage `n0SA`, Terrordale `n0TD`, Runestone Falithas `n0RF`; marker `A0ZG`/`B0ZG` | map data; `Setup/ControlPointSetup.cs` `[WORKAROUND]` | bots had no target in enemy areas without a CP | **objective nodes** in the generated CP graph (preplaced hostile structures, capitals) -- planned for 0.42 | yes |
| W2 | Jaina's Sanctum CP `n0JS` (0.37.2) | map data | reason not recorded; most likely a bot target in Theramore | [you] decide: bot need (-> objective node, 0.42) or Shattered Legacies content (-> move to its list) | yes, if bot need |
| W3 | Sulfuron Spire CP `n0SP` (0.39.3) | map data | gives a bot Skywall a target at the Fireland Invasion | objective node at the Spire (0.42) | yes |
| W4 | The Nexus CP `n0NX` (0.39.3) | map data | replaced the hard-coded Nexus area clear | objective node (0.42) | yes |

0.42 status of W1-W4: the route planner has **area nodes** (generated from map data; a target while enemy structures
stand within 2000). Generated without these six CPs (`gen_routes.py --without ...`), every one of their spots has an
area node within 1800 (Sunsail 661, Terrordale 634, Runestone 1817, Jaina's Sanctum 1519, Sulfuron 1232, Nexus 510).
So they are expected to be unnecessary. Since 0.44.1 enemy buildings anywhere in a node's area are targets and since
0.44.3 creep camps too -- remove W1-W4 once a playtest shows bots attacking those areas without the extra CPs.
| W5 | Strength vote at 49 s | `Ai/AiGoldVote.cs` | uses the time slot of WL's game-mode vote, which Shattered Legacies removed (`[SL]` in `GameSetup.cs`). On the original map both dialogs would collide | start the vote when WL's own vote has ended (event, not a fixed time) | yes |

## 3. Changes the bot makes to the map at runtime

They change the game for humans too, so the original maintainer has to agree to each one.

| What | Where | Note | Blocks PR |
|---|---|---|---|
| Gate sweep: bots' gates are removed (also new gates entering the map) | `AiGates` | bots cannot use the gate mechanic (maintainer's decision) | ask upstream |
| AI waygates (always / prelegion / legion / oldgods), incl. the Broken Isles ship, Kezan, Tol Barad, Argus | `AiLegion`, `AiGraph.Portals` | real waygates -- humans use them too; owned by Neutral Extra (26) | ask upstream |
| Waygate `n036` "Anti Portal Blocking" shrunk to 250 for AI portals | `AiLegion` | the 1000-range aura killed units at the landing spot | ask upstream |
| Demon Portal fix (Argus <-> Sunwell link) | `AiLegion.FixDemonPortal` | WL's portal was missing at (0,0) | upstream bug? check on 5.x |
| Emerald Portals activated, Old Gods rift (Maw of Gor'ma <-> Twilight Highlands) | `AiGates` | the Old Gods rift does not exist on 5.x | rift: no (4.8.2 only) |
| CP handovers: Terrordale / Deathknell to the Scourge at "Plague of Undeath", Sulfuron Spire to Skywall, palace handover | `AiMissions`, `AiConfig` | bots would not get these areas otherwise | ask upstream |
| Bot bonuses: gold subsidy, free techs / upgrades / heroes / revives, starter production buildings, Kul Tiras repair, Continental Outpost buildings | `AiTech`, `AiMissions`, `SimpleBot.HasPerks` | difficulty; humans never get them | design -- discuss upstream |
| Old Gods rift east end moved 100 north of WL's preplaced `n07E` (0.45.0) | `AiConfig.RiftHighlandsY` | the end at `n07E` did not work in game (maintainer); unconfirmed fix | rift is 4.8.2 only |
| One-shots (hand-placed creep clears) replaced by creep camps as route targets (0.44.3) | `AiRoutes.CreepAt`, `AiMissions.RunOneShots` | general logic, no coordinates | no |

Also: **CP income for computer slots** (`AiIncome`, 0.44.2): WL pays CP income only to human slots
(`ControlPointManager`). The bot pays its own slots the same amount -- no WL change. Upstream question: should WL pay
computer slots itself (one condition in `ControlPointManager`)? Then `AiIncome` goes away. Blocks PR: no.

## 4. Bot data tied to this map version or to Shattered Legacies content

| What | Where | Proper solution | Blocks PR |
|---|---|---|---|
| Hand-written CP graph (edges, weights, gates) incl. Shattered Legacies CPs (`n1A0`-`n1B2`, 100-gold CPs) and the W1-W4 CPs | `AiConfig` / `AiGraph` | 0.42: generated graph (`AiRoutes.g.cs`, map data only) with runtime state (rocks, waygates) is the default; the hand graph is the fallback / `-route off`. Delete it in 0.43 | yes, until deleted |
| Coordinates: portal ends, Argus spots, hero visits, kill targets, area clears | `AiConfig`, `AiWorld` | generate where possible; everything else must skip silently when its id is missing (5.x has no Old Gods) | yes |
| Production data | `AiData.g.cs` (generated) | already generated from WL's data; Shattered Legacies factions come through `AiApi.AddProductionData` | no |

## Done

- 0.39.10: the fallback system (Shattered Legacies) no longer lives in `Ai/`; the bot has no reference to it
  (`check_refs.py` LAYER), and Shattered Legacies uses the bot only through `AiApi` (API check).
