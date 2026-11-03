# Changelog -- Shattered Legacies (fork content)

One version number for the whole map (`README.md`, "Map name and versions"). Bot changes are in
`../Ai/CHANGELOG.md`; up to 0.51.1 that file held the fork's changes too (fallbacks 0.39.1-0.39.10, Kezan Enterprise
0.39.5-0.39.8, map name 0.39.9, the extra CPs 0.39.4-0.46.2).

## 0.52.0
Structure for the timelines; no gameplay change. Version v0.52.0.
- **Fallbacks renamed to timelines** -- turning points of four kinds: fallback, sub path, betrayal, pick. The code is
  split the way new content will be added:
  - `Timelines/Core/`: `TurningPointDef` (was `FallbackDef`; the mode strings "wl" / "spawn" / "none" became
    `TurningPointSource`, the special-case string "kultiras" became the `Run` delegate), `TurningPointKind`,
    `TimelineSystem` (was `FallbackSystem`: HQ watch, run), `Hourglass` (the trigger buildings).
  - `Timelines/TimelineCatalog.cs` (was `FallbackConfig.All`), with each placeholder marked where the concepts
    superseded it.
  - `Timelines/Factions/Kultiras/KultirasFallback.cs` and `Timelines/Factions/KezanEnterprise/` (config + mechanics,
    was `KezanEnterprise.cs`).
  - `Bot/`: `SlBotSetup` (everything handed to the bot) and `SlAiData.g.cs`.
  Same behaviour; in-game texts unchanged (the building is still called "Fallback: ..."). Debug log labels:
  "fallbacks" -> "timelines", "FALLBACK" -> "TURNING POINT".
- **The bot holds no fork content any more**: the `-version` title and the rule "bots never pick Sunfury" are set
  from `Bot/SlBotSetup.cs` through `AiApi` (`../Ai/CHANGELOG.md` 0.52.0 -- with it, a bot Illidari slot is decided
  after 4 s instead of WL's 24-s timeout; same faction).
- **Tools**: `tools/ai/` moved to `tools/shattered-legacies/ai/`; the drawio generator added as
  `tools/shattered-legacies/timelines/timeline_overview_gen.py` (rewrites `Timelines/timeline-overview.drawio` in place,
  keeps the old draft page).
- **Docs**, separate for bot and fork: new `AI_AGENTS_CONTEXT.md` (start here: repository, way of working, delivery)
  and this `CHANGELOG.md`; `README.md` rewritten (layers, folder map, every `[SL]` change, files deleted on purpose,
  map data, ID rule `?Z??`, tools, releasing); `TODO.md` rewritten in the agreed order (content index, turning-point
  core with the Defeat rules, Kezan rebuilt, light turning point, Lordaeron pick, faction by faction, shared
  mechanics; the Thandol Treaty draft dropped -- the Great Alliance replaces it); `FALLBACKS.md` replaced by
  `Timelines/TIMELINES.md` (turning-point model, reused WL mechanics, faction template, status per slot);
  `Timelines/SCENARIO_CONCEPTS.md` and the drawio added and corrected (Gilneas -> Theramore, Exodar -> Maiev,
  Twilight's Hammer with Dragonmaw / 24p assets, Dwarven Expedition, Skywall / Black Empire fallbacks open, Forsaken =
  Quel'thalas' betrayal, Ebon Blade = Scourge's fallback, the defeat rule "no CP for more than 5 turns").
  `FACTION_UNITS.md` moved to `Timelines/` (column "Timeline").
- **`[SL]` markers** added where they were missing: `WarcraftLegacies.CLI/Settings/MapSettings.cs`,
  `DefaultOptionsFactory.cs`.
