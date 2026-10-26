# Shattered Legacies -- the fork's own content

**Shattered Legacies: Botified Realm** = Warcraft Legacies 4.8.2 + the bot AI (`../Ai/`) + this fork's own content.
This folder holds the fork's code; this file lists every change the fork makes to the original code or map that is
NOT needed by the bot. What the bot needs is in `../Ai/WORKAROUNDS.md`.

## Rules

- This code may use the bot, but only through `WarcraftLegacies.Source.Ai.AiApi`. The bot never uses this code.
  `python3 tools/ai/check_refs.py` enforces both (LAYER, API).
- Changes in original files carry `// [SL]`. Map data cannot hold comments -- list it below.
- Players never see the map presented as "Warcraft Legacies" or "WL". Attribution and the fork notice ("based on
  Warcraft Legacies", "not the official version") stay -- the original maintainer asked for them.

## Files

| File | Contents |
|---|---|
| `ShatteredLegaciesSetup.cs` | entry point, called from `Setup/GameSetup.cs` (`[SL]`) right after the bot; hands the Kezan data to the bot |
| `FallbackConfig.cs` | one fallback definition per faction (HQ, place, mode), Kezan Enterprise data |
| `FallbackSystem.cs` | trigger buildings, HQ watch, placeholder fallbacks, Kul Tiras -> Kezan Enterprise |
| `KezanEnterprise.cs` | Kezan Oil Supply income, Kezan Trading Center creep market |
| `SlAiData.g.cs` | GENERATED (`tools/ai/gen_ai_data.py`): train lists etc. of the Kezan buildings for the bot |
| `FALLBACKS.md` | fallback design, teams, per-faction work list |
| `FACTION_UNITS.md` | GENERATED (`tools/ai/unit_scan.py --md`): units / heroes per faction, for fallback decisions |
| `TODO.md` | open work of the fork (fallbacks, balance, map content, known issues) |

## Changes to original code (marked `[SL]`)

| Where | What | Since |
|---|---|---|
| `Setup/GameSetup.cs` | `ShatteredLegaciesSetup.Setup()` call | 0.39.10 |
| `Setup/GameSetup.cs` | no game-mode vote: Closed Alliance forced at 59 s (the bot's strength vote uses the freed slot, see W5 in `../Ai/WORKAROUNDS.md`) | 0.39.1 |
| `Setup/ControlPointSetup.cs` | marker `B1HG` = 100-gold CPs (evil buff) | 0.39.5 |
| `Warcraft.Cartographer/Deserialization/AdvancedMapBuilder.cs` | map title from `Info.json` + version | 0.39.9 |
| `GameLogic/DisplayIntroText.cs`, `GameLogic/InfoQuests.cs`, `Setup/HintSetup.cs` | fork title and notice | 0.39.9 |
| `WarcraftLegacies.CLI/appsettings.json` (no comments possible) | version, published file name `ShatteredLegacies-BotifiedRealm` | 0.39.9 |

## Changes to map data

| Ids | What | Since |
|---|---|---|
| `Info.json` | map name, author, description, loading screen texts | 0.39.9 |
| `n0FB`, `AFB1` | fallback trigger building, "Begin the Fallback" | 0.39.1 |
| `n1A0`-`n1B1` | 11 new 10-gold CPs (Scarab Wall, Hive'Zora, Hive'Ashi, Ramkahen, Hive'Regal, Seradane, Broken Shore, Dragonmaw Gates, Thundermar, Sporeggar, Sylvanaar) | 0.39.4 |
| `n1A8`, `n1B2` | Twilight Shore, Deathknell CPs | 0.39.5 |
| `A1HG`, `B1HG` + 4 CPs | 100 gold/min: Ny'alotha, Vortex Pinnacle, Temple of Ahn'Qiraj, Icecrown Citadel | 0.39.5 |
| goblin buildings and units (`o03L`, `o03O`, `o03P`, `o03S`, `o05T`, `o04M`, `h04Z`, ...) | Kezan Enterprise buildings: train lists, oil supply, trading center | 0.39.6-0.39.8 |
| `AGAB`, `I006` | Book of Medivh: third ability "Guardian's Abjuration" | 0.39.9 |

CPs the bot needs (`n0SA`, `n0TD`, `n0RF`, `n0JS`, `n0SP`, `n0NX`, marker `A0ZG`/`B0ZG`) are listed in
`../Ai/WORKAROUNDS.md`.
