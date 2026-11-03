# Context for AI assistants -- Shattered Legacies (start here)

Read this first. Then, depending on the task:
- **fork content** (timelines, new factions, map content, fork rules): `TODO.md` here, then `Timelines/TIMELINES.md`
  (how) and `Timelines/SCENARIO_CONCEPTS.md` (what);
- **the bot**: `../Ai/AI_AGENTS_CONTEXT.md` (the bot's architecture, hard-learned facts) and `../Ai/TODO.md`.

"AI" in file and class names means the **bots**; this file is about **AI assistants** working on the code.

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
- `git mv` into a folder outside the sparse set fails -- add the folder first.
- Cone mode also brings the files directly in the parent folders: `mapdata/WarcraftLegacies/PathingMap.json` and
  `src/WarcraftLegacies.Source/Regions.g.cs` -- what `tools/shattered-legacies/ai/map_query.py` needs.
- The repo is huge (map data, LFS models); never clone it fully. Upstream WL for comparison:
  `https://github.com/AzerothWarsLR/WarcraftLegacies` (`main` / `dev` = 5.x; tag `4.8.2` = this fork's base).

## Code rules

The hard rules in `../Ai/AI_AGENTS_CONTEXT.md` ("Hard rules") apply to ALL code of this repository, fork code
included -- above all the CSharp.lua limits (small classes; `internal static` without `readonly` for shared state),
no compiler in the sandbox (run `check_refs.py`, a tree-sitter syntax pass and `scope_check.py` on every changed
`.cs` file), file encodings (WL's C# / JSON files start with a UTF-8 BOM, files in `Ai/` and `ShatteredLegacies/`
don't -- keep whatever the file had) and desync safety. In addition, for fork code:

1. **Reuse WL's mechanics before writing new ones** -- `Timelines/TIMELINES.md` "Reused WL mechanics" lists them.
   New content for a faction follows the template there (folder, classes, data, docs).
2. **Extend WL by events, not edits** (`FactionManager.FactionRegistered`, `Faction` events). A WL file is changed only
   where no event exists, in one marked line where possible.
3. **New map objects: `?Z??` ids** (`README.md`, ID rule). Copies from the 24p map get a new id when they are truly
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
  map, the 24p map (`mapdata/WarcraftLegacies24p/`) and stock WC3 assets (referenced only, not in the repo). The
  planned content index (`TODO.md`, step 1) is the search index for this -- use it instead of re-scanning the map data.
