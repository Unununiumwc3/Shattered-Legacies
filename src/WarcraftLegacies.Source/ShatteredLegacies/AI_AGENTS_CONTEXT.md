# Context for AI assistants -- Shattered Legacies (start here)

Read this first. Then, depending on the task:
- **fork content** (timelines, new factions, map content, fork rules): `TODO.md` here, then `Timelines/TIMELINES.md`
  (how) and `Timelines/SCENARIO_CONCEPTS.md` (what);
- **the bot**: `../Ai/AI_AGENTS_CONTEXT.md` (the bot's architecture, hard-learned facts) and `../Ai/TODO.md`.

"AI" in file and class names means the **bots**; this file is about **AI assistants** working on the code.

## Handover (end of the 0.52-0.57 session) -- where things stand

- **Released state**: `v0.64.0` on `botified-dev` (0.64.0: the Ashtongue's purification reworked -- Shattrath as the node, sites as tech buildings, Karabor rename; 0.63.2: the Hourglass survives WL's quests; 0.63.1: Hourglass / trigger for late picks, slot names; 0.63.0: Illidari -> Ashtongue Deathsworn -- the third full
  faction: conquest rule, purification, Karabor; 0.62.0: Kezan's carried oil). Open from the last playtest: Sunfury on a bot slot builds no troops (`TODO.md`). Everything since 0.52 is in `CHANGELOG.md` (fork) and
  `../Ai/CHANGELOG.md` (bot). The maintainer playtests 0.57.1/0.57.2 next and brings his findings to the new chat.
- **0.63.0**: Illidari -> Ashtongue Deathsworn (full faction; `TODO.md` step 3b). **0.58.0-0.62.0**: Kul Tiras -> Kezan Enterprise rebuilt (own faction, the 24p goblin tree and Trading Center,
  Oil Rigs, Oil Refining, trade, the Refugee Slave Camps, the end goal on the scoreboard). **Next work**: the 0.62
  playtest, the "Known issues" of the 0.60 playtest (Kul Tiras' fallback flicker, Katherine's ultimate level, quest
  buildings visible, detect invisible), the maintainer's answers (`TODO.md` step 3: how camps are built, what the end goal does, the Kezan
  concept -- turtle / tower defence, buying time), then step 4 the Forsaken's small issues. Bot: `Ai/TODO.md`
  "Production follow-ups" (bots that build with workers, food cap under siege).
- **Reusing the 24p map**: read `tools/shattered-legacies/content/README.md` FIRST -- the steps of an import set and
  WL's rules a copied faction must follow (object limits or WL refunds the unit, Flight / Navigation, tooltip
  placeholders, workers, command card, LFS).
- **How we work**: the maintainer edits nothing by hand. The assistant clones the repo in the sandbox, makes the
  change, runs every check, and hands over ONE `git apply` patch (plus a full patch when the base is unclear); the
  maintainer applies, commits, builds on Windows (`dotnet build src\WarcraftLegacies.Source` first, then the CLI and
  `json-to-w3x publish`), playtests, and sends screenshots / `-dump` files. Version bump in every patch
  (`AiSetup.Build` in `Ai/AiSystem.cs`, `appsettings.json`), both changelogs.
- **Checks before every patch** (no compiler in the sandbox): `tools/shattered-legacies/ai/check_refs.py`,
  `using_check.py`, `scope_check.py`, a tree-sitter C# syntax pass, `tools/shattered-legacies/content/check_json.py`
  after map data changes, `git apply --check` on a clean copy of the base.
- **Generators** (never edit their output): `gen_hourglass.py` (Hourglass), `import_24p.py` (24p objects, object
  limits, id constants, main-map equivalents; sets FORSAKEN, KEZAN), `kezan_objects.py` (Oil Refining), `forsaken_heroes.py` (Sylvanas), `gen_ai_data.py` (bot data), `content_index.py` (`Content/`).

## The project in one paragraph

**Shattered Legacies: Botified Realm** is an unofficial fork of **Warcraft Legacies 4.8.2** (a Warcraft III Reforged
map written in C# with WCSharp and transpiled to Lua by **CSharp.lua**). It stays on 4.8.2 because 5.x removed the
Old Gods; changes from WL 5.x come in by cherry-pick only. Repo: `https://github.com/Unununiumwc3/Shattered-Legacies`,
branch `botified-dev`, tags `v0.MINOR.PATCH`. Never present it as "Warcraft Legacies" or "WL" in anything players see
(lobby, loading screen, chat): the original maintainer asked for that, and players mistook the fork for an official
release. Attribution and the fork notice ("based on Warcraft Legacies", "not the official version") stay -- he asked
for those too.

**Three layers** (`README.md`):
- WL 4.8.2 -- touched as little as possible; every change marked (`[BOT]`, `[SL]`, `[WORKAROUND]`).
- `src/WarcraftLegacies.Source/Ai/` = the bot. Goal: a pull request to the original Warcraft Legacies (`Ai/*.cs` + the
  `[BOT]` hooks). It contains **no fork content** and never uses fork code. Its docs describe the bot only.
- `src/WarcraftLegacies.Source/ShatteredLegacies/` = the fork's own content. It may use the bot, but only through
  `Ai/AiApi.cs`. `tools/shattered-legacies/ai/check_refs.py` enforces both directions (LAYER, API).

Markers in WL files: `// [BOT]` = hook the bot needs (list: `../Ai/WORKAROUNDS.md`), `// [SL]` = fork change (list:
`README.md`), `// [WORKAROUND]` = temporary. Find them with `grep -rn "\[BOT\]\|\[SL\]\|\[WORKAROUND\]" src`.
Every change to a WL file gets a marker and a row in the matching list in the same drop. Map data (JSON) cannot hold
comments -- it is only listed.

## State of the work -- read this, then `TODO.md` ("Order of the work")

- **0.52.0 (released, map works)**: the structure is in place -- `Timelines/` (`Core/` engine, `TimelineCatalog`,
  `Factions/<X>/`), `Bot/`, separate docs for bot and fork, tools in `tools/shattered-legacies/`. Behaviour as in
  0.51.1: one turning point per faction, mostly placeholders; Kul Tiras -> Kezan Enterprise is the only real one.
- **Content index (after 0.52.0, tools / docs only)**: `Content/` lists what the main map, the 24p map and stock WC3
  offer, used or unused. Use it for every content question instead of scanning the map data:
  `grep -i <name> Content/units.tsv`, browse `Content/REUSE.md`. Content is named `<map>:<id>` (`24p:u011`).
- **0.53.0**: the turning-point core -- turning points are WL quests (`Core/TurningPointQuest`), the Hourglass of
  Fate has one research per turning point (generated by `tools/shattered-legacies/timelines/gen_hourglass.py`), the
  effect library (`Core/Effects/`), the fork's defeat rule (`Rules/EliminationRule`: no CP for more than 5 turns; an
  unused fallback starts instead), pilot Quel'thalas -> WL's Sunfury (no Sunfury pick any more). Waits for the
  playtest (`TODO.md`, step 2).
- **0.54.0**: the betrayal structure -- any objective as trigger, effects on the trigger, several turning points per
  faction, the split ("one player becomes two", `AiApi.TakeOver`) and spawn effects, the fork's first own faction
  (`Timelines/Factions/Forsaken/`, object limits in `src/WarcraftLegacies.Shared/ShatteredLegacies/`); pilot:
  Quel'thalas' betrayal -> Forsaken (placeholder tech).
- **0.55.0**: the import tool (`tools/shattered-legacies/content/import_24p.py`, import sets per faction) and the
  Forsaken tree from the 24p map, Sylvanas' phases, the quests "Free Will" and "The Throne of Lordaeron".
- **0.56.x**: Sylvanas' three phase heroes (`forsaken_heroes.py`), "Plague Research" (`ObjectiveRepeatingKills`),
  income for new starts. 0.56.1: her new skills had the stock tech requirement (invisible after learning).
- **0.57.0**: the Forsaken reworked (Domination, Ghostform, level-6 / Free Will / Tier 4 gates, ships), the Undercity
  as the first instance (`Core/Effects/Instance`), Hourglass models per faction.
- **0.57.1**: free slots are lobby computer slots "NPC" (P11, P14, P20; reserved for the bot until a split), late
  factions' starting quests, the Throne via holding the Capital Palace, the Undercity built after WL's corner hiding.
- **0.57.2**: the free slots are fixed computer slots (the map's "fixed player settings" keep humans out); the bot
  completes a bot faction's starting / deadline quests in the last 30 s of the turn before (deadline read from WL's
  `ObjectiveExpire`, no list), arriving factions get the same time from their arrival.
- **0.58.0**: Kul Tiras -> Kezan Enterprise rebuilt -- the second full faction of the fork, the same template:
  import set `KEZAN` (`force`: copied although WL keeps creep versions under the 24p ids), `KezanEnterpriseFaction`,
  `KultirasFallback` (hidden base -> `StartArea.Take` with `MainEquivalents`, `LeaveBehind.ToHeir`), oil
  (`KezanEnterpriseOil`), five quests. Next: the playtest, then the Forsaken's small issues (step 4). Every new turning
  point follows `Timelines/TIMELINES.md` sections 3 and 4; new content comes through an import set.
- **Bot side**: phase 2 (restructure) is the bot's next step (`../Ai/TODO.md`); fork work does not wait for it --
  anything the fork needs from the bot is one `AiApi` method.
- The maintainer plays test games in the Windows VM and reports what he sees; he recycles content, he does not create
  assets.

## Building blocks so far -- reuse them, do not rebuild

Everything a new faction's content needs exists once; `Timelines/TIMELINES.md` sections 1-4 explain each.

| Need | Building block | Where |
|---|---|---|
| a turning point (fallback, betrayal, sub path, pick) | `TurningPointDef` in `TimelineCatalog` -> a WL quest (`TurningPointQuest`) with a trigger + the Hourglass decision | `Timelines/Core/` |
| a 24p faction whose ids the main map uses for something else (WL's creep versions) | import set with `"force": True`; preplaced creep units become the copies with `StartArea.Take(..., <X>Objects.MainEquivalents)` | `import_24p.py`, `KultirasFallback` |
| a count only code knows (rigs built, ...) | `ObjectiveCounter` (`Add(n)`) | `Timelines/Core/` |
| an end goal "hold N gold" | `ObjectiveHoldGold(n)` | `Timelines/Core/` |
| a line on the scoreboard (end goals, a faction's numbers) | `ScoreboardLines.Add(() => text or "")` | `Rules/` |
| control points a faction gets when it arrives | `StartCps.Give(p, cpTypes...)` (neutral ones only) | `Timelines/Core/Effects/` |
| something that happens when a faction falls (defeat or its fallback) | `TimelineSystem.FactionFell.Add(name => ...)` + `Faction.ScoreStatusChanged` | `Timelines/Core/` |
| an autocast bots should use | switch it on by code (its "on" order) and track toggles by `EVENT_PLAYER_UNIT_ISSUED_ORDER` | `KezanEnterpriseOil` / `Bombers` |
| a vassal on the team of whoever holds its seat CP (evil factions conquered, not defeated) | `Conquest.Register(vassal, seatCp, overlord)` | `Timelines/Core/` |
| an objective at a place (minimap icon) that code completes | `ObjectiveAt(text, x, y).Complete()` | `Timelines/Core/` |
| a successor's heroes removed instead of inherited | `LeaveBehind.RemoveHeroes(p, keep)` | `Timelines/Core/Effects/` |
| units any player can buy | a limit in EVERY faction (`FactionManager.FactionRegistered` + `ModObjectLimit`), or WL refunds them | `KezanEnterpriseSlaveCamps` |
| a hand-made object of a fork faction | a small generator next to `import_24p.py` (`forsaken_heroes.py`, `kezan_objects.py`) | `tools/shattered-legacies/content/` |
| leave behind to a lore heir | `LeaveBehind.ToHeir(p, "<faction>", keep)` (heir on the same team, else neutral hostile) | `Timelines/Core/Effects/` |
| its trigger | any WL objective (`MakeTrigger`); `ObjectiveHqLost` (default), `ObjectiveLegendKilledBy` | `Timelines/Core/` |
| the decision button | `tools/shattered-legacies/timelines/gen_hourglass.py` (decision + condition research, button position) | generator |
| what happens | effects: `SwapFaction`, `SplitFaction` (one player becomes two, bot on a free slot), `LeaveBehind`, `StartArea` (hidden base), `Spawn` (start kit), `ChangeTeam`; `OnTrigger` (e.g. a hero lost for good); `Prepare` (map setup) | `Timelines/Core/Effects/` |
| a new faction | `Faction` subclass + object limits generated into `src/WarcraftLegacies.Shared/ShatteredLegacies/FactionObjectLimits/` (+ one `[SL]` line in `ObjectInfoRepository`) | `Timelines/Factions/Forsaken/` |
| its units / buildings | an import set in `tools/shattered-legacies/content/import_24p.py` (24p objects with new ids, dependencies, models; knowledge costs) | generator |
| a hero that changes with the story | one hero type per phase, `Legend.UnitType = ...` (WL carries over life, mana, XP, items); generated by `forsaken_heroes.py` | `ForsakenSylvanas` |
| its quests | WL `QuestData` + WL objectives (`ObjectiveLegendLevel`, `ObjectiveControlPoint`, `ObjectiveCapitalDead`, `ObjectiveQuestComplete` ...); a repeatable reward: `ObjectiveRepeatingKills`; another faction's state: `ObjectiveFactionState` | `Timelines/Factions/Forsaken/Quests/` |
| a base of its own (an "instance") | one of the unused squares in the south-west corner: rocks + sight blockers along the edge, waygate in the middle, buildings symmetric; opened later with a second waygate | `Timelines/Core/Effects/Instance.cs`, `ForsakenUndercity` |
| gates in a tech tree | condition techs (granted by a quest's `ResearchId`) as requirements set in the import set (`"set": {id: {"ureq": ...}}`, `=XXXX` = a main-map id) | `import_24p.py` |
| money for a new start | 150 gold per turn for 10 turns (`TimelineSystem.GrantArrivalIncome`), 10 turns of grace from the defeat rule | `Timelines/Core/TimelineSystem.cs` |
| the bot | `AiApi.TakeOver`, `RegisterHeroes`, production data (`SL_BUILDINGS` in `gen_ai_data.py`) | `Bot/SlBotSetup.cs` |

## Pitfalls learned the hard way (each cost a playtest)

- **A building's command card holds about 11 buttons** -- an altar also shows revive / hero buttons: the 0.63 temple
  trained 9 kinds and some (the Seer) were simply hidden. Spread production over buildings.
- **Lightning between units**: give the heights from `BlzGetUnitZ` (+ an offset), not fixed numbers -- on uneven
  terrain the beam misses (0.63).
- **Rooting a hero**: `UnitRemoveAbility(u, 'Amov')` (it cannot be given back properly -- replace the unit with a
  fresh one when it must move again); a leash that puts it back still lets it slide.

- **WL quests clear a player's buildings** (0.63.2): the Illidari's "Return to Outland" removes every building outside
  the Black Temple area -- the Hourglass was gone. A fork building a player must keep: `UnitType.Register(new
  UnitType(id) { NeverDelete = true })` (WL then hands it to Neutral Hostile instead) plus a watch that gives it back.

- **Nothing per faction on a fixed timer** (0.63.1): a faction can be registered late (a slow pick, a swap, a split).
  The Hourglass and the arming of the HQ triggers ran once at 62 s -- the Illidari, picked later, had neither. Hook
  per-faction setup on `FactionManager.FactionRegistered` / `PlayerData.FactionChange`, or handle "after the moment".
- **Non-human slots carry the lobby's faction name** (0.63.1): shown when their units are hovered -- a swap renames
  them (`SwapFaction.NameSlot`).

- `dotnet build src\WarcraftLegacies.CLI` does not compile the map code -- a missing `using` shows up only at
  publish. Run `using_check.py`; the maintainer builds `src\WarcraftLegacies.Source` first.
- WL records every finished research in the faction (FactionManager) and re-applies it on a faction change. A
  research level cannot be lowered while its limit is above 0: hide it (limit 0) FIRST, then reset the level. A
  research without an object limit throws "level (1) cannot be higher than the object limit" -- every research a
  faction's buildings offer needs one (the import tool adds them).
- Run effects one frame later (`TimelineSystem.Later`), not inside the event that fired them (a death, a research,
  a quest completion): WL is still updating the faction there.
- Stock UNIT spells made into hero spells keep their tech requirement (Banshee / Berserker training): the learned
  skill stays invisible. Set `areq` to "" (`forsaken_heroes.py`).
- Imports with more than one dot in the name do not show in game (WL #4190): the import tool renames them. Model
  files are Git LFS pointers in the sandbox: textures can only be found after `git lfs pull` (re-run the import tool).
- In WL's JSON, cooldown, duration, area, range and similar are "unreal" values (`Type 2`); levels, mana, positions
  are plain integers; text is `Type 3`. A value of the wrong type stops the publish step ("requires an element of
  type 'Number'"). Run `tools/shattered-legacies/content/check_json.py` after every map data change.
- Object ids of the fork: `?Z` + a lowercase letter (`nZh0`, `RZd1`, `uZf0`); one case only in the last character --
  Windows file names ignore case.
- A slot that should be played by the bot must be a COMPUTER player (lobby, `Info.json`): many bot features (quest
  auto-complete, production checks) are computer-only; a taken-over empty slot is only half a bot.
- WL hides every Neutral Passive unit in `Regions.HideUnitBottomLeft` at setup -- anything placed in the south-west
  squares must be created a moment later.
- Capitals are never destroyed (the attacker takes them over): quest objectives use `ObjectiveControlCapital`.
- Alliances start from the lobby forces (`Info.json`); WL's teams only ADD alliances, never remove them. A player
  who changes sides needs an explicit unally (`SplitFaction.UnallyOutsideTeam`). WL puts every non-lobby slot into the
  "Scourge, Legion" force.
- A faction that arrives mid-game registers its quests then: WL code that schedules fixed turns failed for past
  turns -- always register arriving factions through `SwapFaction.RegisterArriving` (deadline shift + turn clamp).
- Successors keep the player's colour (`Faction.SetPlayerColor`, set before the swap); WL deadlines of arriving
  factions count from the arrival (`ObjectiveExpire.ExpirationOffset`).

## Getting the code (sandbox)

```
GIT_LFS_SKIP_SMUDGE=1 git clone --depth 1 --filter=blob:none --sparse --branch botified-dev \
    https://github.com/Unununiumwc3/Shattered-Legacies.git wl && cd wl
git sparse-checkout set src/WarcraftLegacies.Source/Ai src/WarcraftLegacies.Source/ShatteredLegacies tools/shattered-legacies \
    src/MacroTools src/WarcraftLegacies.Source/Setup src/WarcraftLegacies.Shared \
    mapdata/WarcraftLegacies/Units mapdata/WarcraftLegacies/UnitData mapdata/WarcraftLegacies/AbilityData \
    src/WarcraftLegacies.CLI src/Warcraft.Cartographer
```
- `WarcraftLegacies.Shared` is needed by `gen_ai_data.py`; `git sparse-checkout add src/WarcraftLegacies.Source/GameLogic`
  for the other marked files, `src/WarcraftLegacies.Source/Factions` to read WL's faction and quest classes (the
  templates for timeline content), `mapdata/WarcraftLegacies24p` for the 24p map's object data.
- Re-running the content index needs the whole `src/WarcraftLegacies.Source` (code references), `src/War3Api.Object`
  (stock data), `mapdata/WarcraftLegacies` and `mapdata/WarcraftLegacies24p` (object data, `Units/`, `Imports/` --
  LFS pointers are enough). Reading it needs only `ShatteredLegacies/Content/`.
- `git mv` into a folder outside the sparse set fails -- add the folder first.
- Cone mode also brings the files directly in the parent folders: `mapdata/WarcraftLegacies/PathingMap.json` and
  `src/WarcraftLegacies.Source/Regions.g.cs` -- what `tools/shattered-legacies/ai/map_query.py` needs.
- The repo is huge (map data, LFS models); never clone it fully. Upstream WL for comparison:
  `https://github.com/AzerothWarsLR/WarcraftLegacies` (`main` / `dev` = 5.x; tag `4.8.2` = this fork's base).

## Code rules

The hard rules in `../Ai/AI_AGENTS_CONTEXT.md` ("Hard rules") apply to ALL code of this repository, fork code
included -- above all the CSharp.lua limits (small classes; `internal static` without `readonly` for shared state),
no compiler in the sandbox (run `check_refs.py`, `using_check.py`, a tree-sitter syntax pass and `scope_check.py` on
every changed `.cs` file -- `dotnet build src/WarcraftLegacies.CLI` does NOT compile WarcraftLegacies.Source, only
the publish step does, so a missing `using` shows up there first; the maintainer can add
`dotnet build src\WarcraftLegacies.Source` to his build commands to see all C# errors at once), file encodings (WL's C# / JSON files start with a UTF-8 BOM, files in `Ai/` and `ShatteredLegacies/`
don't -- keep whatever the file had) and desync safety. In addition, for fork code:

1. **Reuse WL's mechanics before writing new ones** -- `Timelines/TIMELINES.md` "Reused WL mechanics" lists them.
   New content for a faction follows the template there (folder, classes, data, docs).
2. **Extend WL by events, not edits** (`FactionManager.FactionRegistered`, `Faction` events). A WL file is changed only
   where no event exists, in one marked line where possible.
3. **New map objects: `?Za?` ids** (`Z` second, a lowercase letter third -- `README.md`, ID rule). Copies from the 24p map get a new id when they are truly
   new (same id in both maps is often a DIFFERENT object).
4. Fork code reaches the bot only through `AiApi`; a feature that needs bot data gets an `AiApi` method in the same drop
   (and a line in `../Ai/CHANGELOG.md`, since it is a bot change).

## How we work

- Session start: the maintainer pastes the repo / branch and a task; clone sparsely (above), read this file and the
  matching TODO. He compiles in a Windows VM (Winboat) and runs git on Ubuntu; no GitHub Actions.
- "Discuss only" = do not code. When coding: leave out what is unclear, put it in the TODO, and say so.
- At most one question per reply; propose defaults instead of asking many.
- Two TODO files: `../Ai/TODO.md` = bot work only (what would go into the upstream PR); `TODO.md` here = everything
  fork-only (timelines, new factions, map content, the fork's win rule, faction balance, the maintainer's ideas). Put
  new items in the right one. TODO files hold open work only.
- Two CHANGELOG files with ONE version number: `../Ai/CHANGELOG.md` for bot changes, `CHANGELOG.md` here for fork
  changes. A drop that changes both gets an entry in both.
- Version on every code drop: `AiSetup.Build` (bottom of `../Ai/AiSystem.cs`) AND `MapSettings.Version` in
  `src/WarcraftLegacies.CLI/appsettings.json` (`v<build>`; also in the published file name). Docs-only drop: CHANGELOG
  only. Release = commit + push + tag `v<build>` (commands in `README.md`, "Releasing").
- Push only named tags (`git push origin v<build>`), never `--tags` (old tags drag Git LFS into history the clone
  never downloaded).
- Deliver one patch per version (`git diff --binary -M` against the pushed branch, or against the previous patch of
  the same delivery), verified with `git apply --check` on a clean worktree of `origin/botified-dev`, plus a zip of the
  whole files with repo paths. The maintainer downloads them to `~/Downloads/` and runs the commands given in the reply
  (apply, commit, tag); he cannot run `.ps1` scripts in Windows, only paste PowerShell commands.
- Content for new factions: recycle, then adjust -- the maintainer does not create assets. Reuse objects of the main
  map, the 24p map (`mapdata/WarcraftLegacies24p/`) and stock WC3 (referenced only, its data is in
  `src/War3Api.Object/Generated/`). The content index `Content/` is the search index for this -- use it instead of
  re-scanning the map data; re-run `tools/shattered-legacies/content/content_index.py` after map data changes.
- Generated files (`*.g.cs`, `Content/`, `FACTION_UNITS.md`, `ROUTES.md`, the drawio): change the generator, re-run
  it, never edit the output. Generated images (`tools/shattered-legacies/ai/out/`) are never committed.
