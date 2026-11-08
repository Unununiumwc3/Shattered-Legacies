# Shattered Legacies: Botified Realm

> **This is not Warcraft Legacies.**
> Shattered Legacies: Botified Realm is an unofficial exploration/prototype based on
> [Warcraft Legacies](https://github.com/AzerothWarsLR/WarcraftLegacies). It is not the official
> version and is not affiliated with or endorsed by the Warcraft Legacies team.
>
> Please do not report bugs or problems from this project to the Warcraft Legacies team.

## What is this?

An experiment in making a graph-based bot system for Warcraft 3 work with the help of LLMs,
built on top of the Warcraft Legacies codebase.

A more detailed README will follow.

## Where things are

Based on Warcraft Legacies **4.8.2** (the last version with the Old Gods); changes from later versions come in by
cherry-pick only.

- `src/WarcraftLegacies.Source/Ai/` -- the bot AI (README, TODO, CHANGELOG, measurements, `WORKAROUNDS.md`). It
  contains no Shattered Legacies content, so it can be offered to the original Warcraft Legacies as it is.
- `src/WarcraftLegacies.Source/ShatteredLegacies/` -- this fork's own content: timelines (fallbacks, sub paths,
  betrayals, picks), the Kezan Enterprise. Its README lists every change to the original code and map; its TODO holds
  the open work and the roadmap; `Timelines/` holds the scenario design.
- `tools/shattered-legacies/` -- development tools (checks, generators) for both.

Known issues and plans are tracked in the two TODO files (`Ai/TODO.md` for the bots,
`ShatteredLegacies/TODO.md` for everything else).

## Restoring models and textures on Windows

Run these commands in PowerShell at the repository root on the branch used to publish the map. `fsck` verifies the local LFS objects. Fetching downloads the objects for the checked-out commit, and checkout fills pointer files in the working tree. Empty include and exclude settings override local download filters for the fetch.

```powershell
git branch --show-current
git lfs install --local
git -c lfs.fetchexclude= lfs fsck --objects HEAD
git -c lfs.fetchinclude= -c lfs.fetchexclude= lfs fetch origin HEAD
git lfs checkout
git -c lfs.fetchexclude= lfs fsck --objects HEAD
```

For Elate (`o02N`), the MDX names `SilithidFlyerSkin.BLP` at the archive root. Verify the two working files against the hashes at `9794d98`:

```powershell
$imports = 'mapdata/WarcraftLegacies/Imports'
Get-FileHash "$imports/war3mapImported/SilithidWasp.mdx", "$imports/SilithidFlyerSkin.BLP"
```

| File | Bytes | SHA-256 |
|---|---:|---|
| SilithidWasp.mdx | 260058 | 252199ce8dbcf3522c12862305586f8f54cb9866951885bf1cfa711b6a2a0825 |
| SilithidFlyerSkin.BLP | 85186 | f0fae36bf93c9a926e8e9a999ffe221ca1d078cfa9a2df22ed53cc78141e7d12 |

LFS checkout preserves modified files. Back up local asset edits before restoring any individual file. If both hashes match, publish a fresh map and inspect the exact playable archive for `war3mapImported/SilithidWasp.mdx` and root-level `SilithidFlyerSkin.BLP`.

## Credits

All of the original map, game design and code comes from
[Warcraft Legacies](https://github.com/AzerothWarsLR/WarcraftLegacies), created by its founders
Lordsebas and YakaryBovine, its developers Technopig, Madsen and Vampirika, programmer alrun3,
and all contributors and artists listed in the commit history and in the in-game credits.
Thank you for building and open-sourcing it.

The full commit history of the original project is preserved in this repository.

## License

MIT. The original copyright notice in [LICENSE](LICENSE) is kept unchanged.

## Code structure (inherited from Warcraft Legacies)

### MacroTools
Library containing tools intended for consumption by Warcraft 3 maps in the Macro Strategy genre.

### MacroTools.Shared
Library code intended to be consumed by both Warcraft.Cartographer-based CLIs and Warcraft 3 Macro maps, to support data accessible at both map generation time and runtime.

### War3Api.Object
"Fork" of [War3Api.Object](https://github.com/Drake53/War3Api) modified for internal consumption.

### Warcraft.Cartographer
Library built on [War3Net](https://github.com/Drake53/War3Net) providing advanced tools for serializing, deserializing, compiling, and postprocessing Warcraft 3 maps.

### Warcraft.Integrity
Library built on [War3Api.Object](https://github.com/Drake53/War3Api/tree/master/src/War3Api.Object) providing tools for assessing the data integrity of Warcraft 3 maps.

### WarcraftLegacies.CLI
Command line executable that leverages Warcraft.Cartographer to support the editing, testing, and publishing of Warcraft Legacies.

### WarcraftLegacies.Shared
Game code shared between WarcraftLegacies.CLI and WarcraftLegacies.Source, to support data accessible at both map generation time and runtime.

### WarcraftLegacies.Source
Code specific to Warcraft: Legacies, which gets compiled into Lua before being executed during the map's runtime.

### WarcraftLegacies.Map.Tests
Tests that ensure the Warcraft Legacies map contains valid data, and that it is in accordance with our game principles.
