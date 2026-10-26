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

## Known issues (v0.39.9 playtesting)

- **Rift to Maw of Gorma does not work.** The target is around x 22545, y -4784; moving it about 100 units further north might fix it, or the cause is something else.
- **Fallback messages are wrong.** When Kul Tiras loses its HQ, the game says the HQ was regained, although it was destroyed for good.
- **Dalaran/Gilneas fallback does not swap teams on the scoreboard.** Gilneas should use the same fallback as Dalaran (same player slot), but with Gilneas City as its HQ.
- **Auto-surrender interferes with fallbacks.** The rule that makes players surrender with too few control points can stay, but controlling 1 CP should be enough to survive.
- **The Old Gods are still too weak.**
- **New Kezan units are invisible.** The unit count goes up during production, but the units can't be seen. Probably an asset problem: models and textures reused from the 24-player map may need adjusted paths or need to be copied over.
- **Orcs never seem to take the Warsong path** instead of Frostwolf. Either bad luck or the random selection doesn't work for them (it does work for the Sentinels and the Exodar).
- **Stormwind Castle CP needs a buff** (the 100-gold version).

## Planned

- **Pseudo-flyers:** air units lose real flying and become pseudo-flyers, because bots handle flying pathing badly.
- **Kezan alternative win condition:** Kezan wins by owning a large amount of gold (around 20,000). Unlikely, since Kezan is alone and needs gold to defend itself. The oil buildings should get upgrades that increase production, and with it the gold income.

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
