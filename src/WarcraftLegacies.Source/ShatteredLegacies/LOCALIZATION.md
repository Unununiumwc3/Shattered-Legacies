# Chinese localization

Source: Warcraft Legacies **5.0.4**, commit
`a0c73082c268f65142781c287bc746e36c7d94c9`. This is a selective port, not a merge
of the remaining upstream gameplay. In the map since **0.75.0** (bundle 4): the maintainer's add-on (made on 0.68),
applied, with the object-text overlays rebuilt for the current map.

## Since 0.75.0

- **Overlays rebuilt** by `tools/shattered-legacies/localization/build_overlays.py --wl <WL 5.0.4 mapdata>`: a WL
  translation is taken for a field only when the object exists here with the same base and WL's English of that field
  equals ours; everything else stays English. WL 5 objects under fork ids (`AZo0`, `AZo1`, `RZo0`, `RZo1`, `RZn0`)
  take WL's text from WL's id; this map's `A000`, `A00Y`, `R003`, `RZ02`, `n00K` (Ahn'Qiraj / Skywall, restored in
  0.73.0) no longer get WL 5's Frenzy / Teleport / ... text. Never an overlay without a base file (it would CREATE a
  text-only object in the Chinese build). Re-run it after every change of English object text or a port.
  0.75.0: 3,607 files, 13,536 fields (bundles 1-3 objects included: Dalaran Shield, Power Generator, ...).
- **The fork's own texts**: `MacroTools/Localization/SlText.cs` (English, the lookup keys) and
  `Translations/ChineseTranslation.ShatteredLegacies.cs` (Chinese). In there: the start message, the F9 guide "How
  Shattered Legacies works", the credits notice, the fork's hints. The lobby description and loading screen are map
  data: `mapdata/WarcraftLegacies/Info.json` and `zhCN/Info.json`.
- Still English: the fork's quests, turning points / Hourglass, fallback announcements, bot messages, the 24p imports'
  object text. New fork text: put the English in `SlText`, show it with `Loc.Get`, add the Chinese line.

## Included

- 2,584 Simplified Chinese runtime dictionary entries, including two new language
  help/error entries. Chinese clients are detected automatically; `-language zh`
  also selects Chinese. `-language en` and `-language es` remain available.
- Locale spelling variants, early-start language lookup before settings load,
  literal `%` substitutions, localized turn titles, faction/team scoreboard names,
  objective initialization and command replies/help.
- Two donor messages now preserve `{controlWord}` instead of always saying control
  was taken. English faction/team identifiers remain unchanged for bot and path logic.
- 3,602 sparse object-text files: 1,554 unit, 1,354 ability, 318 upgrade, 226 buff,
  95 item and 55 destructable records. Only display-text fields are included.
  The donor English value must match the fork's value before accepting a translation.
  45 object/field candidates were omitted for absent/different records or fork quest
  markers. R05A/R05O/R05R are reserved for the fork's existing quest behavior/text.
- Upstream locale-aware tooltip generation and shared-string-table packaging.
  Imported assets, terrain, placements, rosters, costs, combat values, fallback
  pairings and opening slot choices are not edited by this patch.
- A sparse Chinese `Info.json` changes only display titles/description. The converter
  explicitly rejects gameplay/lobby properties in that overlay, so it cannot replace
  the fork's player slots or start positions with the donor's lobby.

## Build one map for English and Chinese clients

From the repository root, with .NET 10 and the existing build dependencies/assets:

```bash
python3 tools/shattered-legacies/localization/publish.py
```

Use the resulting `maps/published/*-multilingual.w3x`. The script publishes the
normal English map, a separate `*-zhCN.w3x`, then combines them using upstream's
`--locale-folder --string-table` options. It reads your current configured version
and keeps the Shattered Legacies filename. It does not overwrite the English map
with the Chinese-only build. Optional: `--dotnet /path/to/dotnet`.

Only language strings live in `_Locales/zhcn.w3mod` and `_Locales/zhtw.w3mod`;
object data and the Lua script remain shared at the map root. Both Chinese regions
currently receive **Simplified** Chinese, not a separate Traditional translation.
A normal single-language `publish` still builds normally: the Chinese runtime
quest/UI dictionary is present, but object text needs the multilingual packaging.

The locale includes map-name/loading-title strings. The MPQ pre-archive header is
kept from the English map. Lobby/browser presentation can therefore vary by client
and screen; verify it with the target Warcraft III clients rather than assuming
that every map-list label changes. An in-game language command cannot rename an
already-created lobby or change the client's object-text locale.

## Coverage limits and validation

Fork-only fallback quests, bot messages, new 0.69 text and changed English strings
without a matching translation stay English. Some older interpolated source text
still needs conversion to localization templates. This is not a claim of complete
Chinese coverage. Changing the language mid-game does not guarantee that all
already-created UI labels immediately refresh; client auto-detection works at start.

Verified: 28 runtime-localization tests, 9 overlay/path/lobby tests; English and
Chinese map publishing; combined WTS packaging (10,444 strings in the available
western test snapshot). The source snapshot used for this independent add-on is
from the western integration; the patch does not reconstruct or replace the 0.69
terrain/gameplay follow-up. Apply checks must still pass on the maintainer's checkout.
No Warcraft III or mixed-client multiplayer playtest has been performed here.

Playtest:

1. Chinese client: opening picks, quest objectives, turn timer and scoreboard.
2. English client: English text and unchanged fork identity/teams.
3. Host with English and join with Chinese, then reverse host/client languages.
   Check map verification, synchronization, picks, research, CP capture and fallback.
4. Inspect unit/ability tooltips and generated training/research lists in both clients.
5. Check the lobby title, map-browser title and loading title separately.
6. Run `-language zh`, `-language en`, then load saved preferences in a new match.
7. Repeat the 0.69 Skywall/Kezan/Twillight tests; fork-only text may remain English.

Future updates: re-check overlay text against the new English fields. A matching
rawcode alone does not mean a donor tooltip still describes the fork's unit or quest.
