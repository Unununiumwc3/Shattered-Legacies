# Shattered Legacies -- the fork's own content

**Shattered Legacies: Botified Realm** = Warcraft Legacies **4.8.2** (the last version with the Old Gods) + the bot AI
(`../Ai/`) + this fork's own content (this folder). The fork stays on 4.8.2: changes from WL 5.x come in by
**cherry-pick only**, never by merge (5.x deleted the Old Gods).

Documents of the fork (the bot has its own set in `../Ai/`):

| File | Contents |
|---|---|
| `README.md` | this file: layers, folder map, every change to WL's code and map, ID rule, tools, releasing |
| `TODO.md` | the fork's open work, in order |
| `AI_AGENTS_CONTEXT.md` | **start here** for AI assistants: repository, way of working, delivery |
| `CHANGELOG.md` | the fork's changes from 0.52 on (older entries: `../Ai/CHANGELOG.md`) |
| `Timelines/TIMELINES.md` | HOW timelines are built: the turning-point model, reused WL mechanics, the faction template, status |
| `Timelines/SCENARIO_CONCEPTS.md` | WHAT is planned: vocabulary, rules, decision register D1-D17, every slot |
| `Timelines/timeline-overview.drawio` | the flow of all timelines (GENERATED, `tools/shattered-legacies/timelines/`) |
| `Timelines/FACTION_UNITS.md` | units and heroes per WL faction (GENERATED, `unit_scan.py --md`) |
| `Content/` | the content index: what the main map, the 24p map and stock WC3 offer, used or not (GENERATED; `Content/README.md`) |

## Layers

```
Shattered Legacies (ShatteredLegacies/)  --AiApi only-->  bot (Ai/)  -->  WL 4.8.2
```

- This code may use the bot, but only through `WarcraftLegacies.Source.Ai.AiApi`. The bot never uses this code and
  contains no fork content, so `Ai/` can be copied into a WL branch as it is (a pull request = `Ai/*.cs` + the `[BOT]`
  hooks). `python3 tools/shattered-legacies/ai/check_refs.py` enforces both directions (LAYER, API).
- Shattered Legacies follows WL's own patterns wherever it can: `Faction` subclasses for new factions, `QuestData` for
  turning points, `FactionChoice` for picks, `ObjectInfo` for object limits, WL's rescue rectangles for hidden bases
  (`Timelines/TIMELINES.md`). Shattered Legacies code then reads like WL code, and cherry-picks work both ways.
- WL is extended by **events, not edits** (`FactionManager.FactionRegistered`, `Faction` events). Where no event
  exists, a one-line `// [SL]` hook -- preferably a generic one WL could take upstream.
- Every project that needs fork code gets a `ShatteredLegacies/` subfolder (planned:
  `src/WarcraftLegacies.Shared/ShatteredLegacies/` for the object limits of new factions); tools live in
  `tools/shattered-legacies/`. Map data cannot have subfolders: it is listed below.

## Folder map

```
ShatteredLegacies/
  ShatteredLegaciesSetup.cs     entry point, called from Setup/GameSetup.cs ([SL]) right after the bot
  Bot/
    SlBotSetup.cs               everything handed to the bot (AiApi): production data, heroes, map title, pick rules
    SlAiData.g.cs               GENERATED (tools/shattered-legacies/ai/gen_ai_data.py): the Kezan buildings' bot data
  Content/                      GENERATED content index: REUSE.md + units / abilities / upgrades / items .tsv
  Timelines/
    TimelineSetup.cs            entry point of the timelines
    TimelineCatalog.cs          one turning-point definition per WL faction (mostly placeholders, see TIMELINES.md)
    Core/                       the engine: TurningPointDef / Kind / Source, TimelineSystem (HQ watch, run), Hourglass
    Factions/<Faction>/         content per WL faction or new faction -- the template is in TIMELINES.md
      Kultiras/                 Kul Tiras -> Kezan Enterprise (KultirasFallback)
      KezanEnterprise/          the Kezan Enterprise: config, oil income, creep market
```

## Changes to WL's code (marked `[SL]`)

`grep -rn "\[SL\]" src` finds them. The bot's own hooks (`[BOT]`) are listed in `../Ai/WORKAROUNDS.md`.

| Where | What | Since |
|---|---|---|
| `Setup/GameSetup.cs` | `ShatteredLegaciesSetup.Setup()` call | 0.39.10 |
| `Setup/GameSetup.cs` | no game-mode vote: Closed Alliance forced at 59 s (the bot's strength vote uses the freed slot, W5 in `../Ai/WORKAROUNDS.md`) | 0.39.1 |
| `Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs` | map title from `Info.json` + version | 0.39.9 |
| `WarcraftLegacies.CLI/Settings/MapSettings.cs`, `DefaultOptionsFactory.cs` | optional published file name per map | 0.39.9 (marked 0.52) |
| `WarcraftLegacies.CLI/appsettings.json` (no comments possible) | version, published file name `ShatteredLegacies-BotifiedRealm` | 0.39.9 |
| `GameLogic/DisplayIntroText.cs`, `GameLogic/InfoQuests.cs`, `Setup/HintSetup.cs` | fork title and notice | 0.39.9 |
| `README.md` (repository root) | the fork's README | 0.39.9 |

Planned (`TODO.md`): the Defeat hook in `MacroTools/Factions/Faction.cs`, the Illidari pick in
`Setup/FactionChoiceDialogSetup.cs`, one line per new faction in `WarcraftLegacies.Shared/ObjectInfoRepository.cs`.

**Deleted on purpose** (do not restore when cherry-picking): `.github/workflows/dotnet.yml`,
`github-beta-releases-to-discord.yml`, `github-releases-to-discord.yml` -- the fork uses no GitHub Actions and no
Discord release posts.

Also in the history, not fork changes: WL commits after 4.8.2 that were cherry-picked as they are (#4099 dummy casters,
#4100 balance, #4190 import file names).

## Changes to map data

JSON cannot hold comments, so every change is listed here.

| Ids | What | Since |
|---|---|---|
| `Info.json` | map name, author, description, loading screen texts (the lobby slot names are still WL's, `TODO.md`) | 0.39.9 |
| `n0FB`, `AFB1` | the Hourglass (map name still "Fallback") and its ability "Begin the Fallback" | 0.39.1 |
| goblin buildings and units (`o03L`, `o03O`, `o03P`, `o03Q`, `o03S`, `o04M`, `o05T`, `h04Z`, `h08Y`, `h091`, `h09I`, ...) | Kezan Enterprise: train lists, oil supply, trading center | 0.39.6-0.39.8 |
| `AGAB`, `I006` | Book of Medivh: third ability "Guardian's Abjuration" | 0.39.9 |
| ~~`n1A0`-`n1B2`, `A1HG` / `B1HG`~~ | the fork's 13 extra CPs and the four 100-gold CPs -- **removed in 0.46.2** (WL's CPs again) | 0.39.4-0.46.2 |

**ID rule (0.52):** new objects of the fork get an id with **`Z` as the second character** (`nZ01`, `hZ01`, `AZ01`,
`RZ01`, heroes `HZ01` / `OZ01` / `UZ01` / `EZ01`). The World Editor practically never generates those, so a WL object cherry-picked
later can never collide. Objects taken from the 24p map (`mapdata/WarcraftLegacies24p/`, part of WL 4.8.2) get such an
id too when they are truly new: ~385 unit ids exist in both maps as DIFFERENT objects. `n0FB` / `AFB1` are older
than the rule (`n0FB` is inside the editor's range; re-id planned with the Hourglass rework, `TODO.md`).

## Tools (`tools/shattered-legacies/`, not part of any WL pull request)

| Folder | What |
|---|---|
| `ai/` | the bot's checks and generators: `check_refs.py`, `scope_check.py`, `lua_check.py`, `gen_ai_data.py`, `gen_routes.py`, `gen_lanes.py`, `unit_scan.py`, `map_query.py`, `render_map.py`, `dump_summary.py`, `back_forth.py` -- details in `../Ai/README.md` "Developer checks" |
| `timelines/` | `timeline_overview_gen.py` -- writes `Timelines/timeline-overview.drawio` in place (keeps its old draft page) |
| `content/` | `content_index.py` -- writes the content index `Content/` (main map, 24p map, stock WC3; used / unused) |

Generated images (`ai/out/`) are never committed.

## Map name and versions

- The map builds as **"Shattered Legacies: Botified Realm v<version>"** (lobby and loading screen; name in
  `mapdata/WarcraftLegacies/Info.json`, the version is appended on publish), so it is never mistaken for the official
  Warcraft Legacies. Players never see the map presented as "Warcraft Legacies" or "WL"; attribution and the fork
  notice ("based on Warcraft Legacies", "not the official version") stay -- the original maintainer asked for both.
- One version for the whole map: `0.MINOR.PATCH` (feature build = MINOR, fix / small build = PATCH; whole numbers,
  0.39.9 -> 0.39.10). It lives in **both** `AiSetup.Build` (bottom of `../Ai/AiSystem.cs`) and `MapSettings.Version`
  in `src/WarcraftLegacies.CLI/appsettings.json` (with a leading `v`). `-version` in game shows it.
- Published file: `maps/published/ShatteredLegacies-BotifiedRealm-v<version>.w3x`. Git tags: `v<version>` (builds
  before 0.39.9 were tagged `4.8.2-ai.<build>` in the deleted fork).

## Releasing

Git runs on Ubuntu, the map is compiled in the Windows VM (Winboat). Repo:
`https://github.com/Unununiumwc3/Shattered-Legacies`, branch `botified-dev`.

1. Bump the version in both places (above). Add the entry to `CHANGELOG.md` (fork) and / or `../Ai/CHANGELOG.md`
   (bot).
2. Compile in Windows (paste into PowerShell):
   ```powershell
   $repo  = "C:\Users\leif\Desktop\Shared\WarcraftLegacies"
   $start = Get-Date
   dotnet build "$repo\src\WarcraftLegacies.CLI" -c Debug
   cd "$repo\src\WarcraftLegacies.CLI\bin\Debug\net10.0"
   dotnet WarcraftLegacies.CLI.dll json-to-w3x publish WarcraftLegacies
   Get-ChildItem "$repo\maps\published\*.w3x" | Where-Object { $_.LastWriteTime -ge $start } | Copy-Item -Destination "C:\Users\leif\Desktop\Shared\" -Force -PassThru
   cd $repo
   ```
3. Playtest: the lobby shows "Shattered Legacies: Botified Realm v<version>", `-version` the same build.
4. Commit, push, tag (Ubuntu):
   ```
   git add <changed files>        # check with git status first
   git commit -m "<version>: <one line>"
   git push
   git tag -a v<version> -m "Shattered Legacies: Botified Realm v<version>"
   git push origin v<version>
   ```
