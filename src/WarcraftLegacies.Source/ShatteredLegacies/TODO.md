# TODO -- Shattered Legacies (fork content)

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Move finished items to
`../Ai/CHANGELOG.md` (one changelog for the whole fork).

**This file holds everything that exists only in Shattered Legacies**: fallbacks, Kezan, the Thandol Treaty, map
content, the fork's win rule and game length, balance of the fork's factions, and the maintainer's brainstorming.
Bot work (what would go into the upstream PR: `Ai/` + `[BOT]` hooks) is in `../Ai/TODO.md`. Fork code uses the bot
only through `Ai/AiApi.cs` -- a feature here that needs bot data gets an `AiApi` method, never a reach into `Ai/`.

## Thandol Treaty (design draft -- `[you]` open points at the end)

Belongs to the timeline (fallbacks, betrayals, sub-paths) -- `FALLBACKS.md` is renamed to a general timeline document
in the phase-2 restructure; this section moves there then.

A start decision for the **North Alliance** (Lordaeron, Quel'thalas, Dalaran, Gilneas) and the **South Alliance**
(Stormwind, Ironforge, Kul Tiras), today two separate teams kept apart by WL's rocks on the Thandol Span (removed at
turn 30).

- **Treaty kept** (= WL today): the rocks stay until turn 30. Then the announcement:
  > The Treaty of the Thandol Span is shattered! Starving for land, gold, and reinforcements, mutual trust between
  > the Northern and Southern Commands has collapsed. Driven by paranoia and desperation, both Alliances lower their
  > bridges -- marching to claim total control over the Eastern Kingdoms!

  (The text describes North vs South at war -- it fits this path only; the vote dialog needs its own text.)
- **Great Alliance**: the rocks go at once; North and South become ONE team. WL already has an unused team
  `TeamSetup.Alliance` (its "Great War" mode uses it) and `PlayerData.SetTeam` (scoreboard and alliances follow).
  The price: the South pays the **Imperial Tax**, and the Defias rise in the south.
- **Imperial Tax**: every second, X % (start: 20 %) of each South member's income -- CP income + WL's bonus income
  (`PlayerData.TotalIncome`) + the bots' strength-vote bonus (an `AiApi` method that reports what the bot paid) --
  is taken from the South member (never below 0 gold) and split equally among the living North members. One 1-s
  task in this folder; WL's payout code and `AiIncome` stay untouched.
- **Announcement when the tax starts** (draft):
  > The Great Alliance is sealed -- at a price. Every fifth coin of the South now flows north as the Imperial Tax.
  > Stormwind's treasury runs dry, its garrisons march north... and in the shadows of Westfall, the Defias
  > Brotherhood sharpens its knives.
- **Defias events** (design open): raids in Westfall / Elwynn / Redridge against the South's CPs while the tax runs.
  Units: the 24p map has Defias Assassin / Kingpin (`h0AI`, `E01H`, `FACTION_UNITS.md`) -- the same asset risk as the
  invisible Kezan units.
- **Vote**: voters = all North and South players. If one of them is human, only the human votes count; otherwise
  the bots vote at random (`GetRandomInt` -- synced). Tie = treaty kept. Not at 49 s (the bot strength vote uses that
  slot); proposal ~1:30.
- **Bots** need nothing special: the route graph follows the real rocks (edge open within 15 s), allies are never
  targets, teams come from WL.
- `[you]` open points:
  - One human on ONE side decides for both (a North human can make the South pay). Accept, or require a yes from
    each side?
  - Tax recipients: equal split among the North (default), or the holder of the North's capital?
  - The merged team (7 factions) still wins at 90 CPs like every team -- likely fast. Raise its threshold, or accept?
  - The vote dialog's own text (the treaty text above describes the war path).

## Win rule and game length (fork only)

- `[you]` Goal: a game ends with ONE winner after **100-140 min**, and the winner has to capture everything. WL's rule
  today: a TEAM wins at 90 CPs (`ControlPointVictory.CpsVictory`). Needs the fork's own rule (an `[SL]` change).
- Caution: "everything" must mean every CAPTURABLE CP -- some CPs are invulnerable / quest-locked and can never fall.
  The last few CPs (islands) make long tails, the 9.5-h stalemate kind; a share (e.g. 95 %) or "all capitals + N %"
  ends cleaner. Bots must be able to reach them all (ferries, `../Ai/TODO.md` 0.47).
- Measurement: the dump pins when a team first reaches 80 / 90 CPs; the actual winner / victory is not recorded yet
  (bot debug item "winner record", `../Ai/TODO.md`). Then `dump_summary.py` reports the game length.

## Balance of the fork's factions and the bots' unit budget

- The bots get a unit budget per CP in 0.46 (`../Ai/CONCEPTS.md` 8: 40 + 10 food per held CP, ~150 on average).
  `[you]` a different base per faction / team (good vs evil) -- set from here through an `AiApi` hook.
- `[you]` Humans too? A CP-based food cap for humans would be a fork rule (the bot budget is bots only).

## Known issues (v0.39.9 playtest)

- [ai] **Fallback messages are wrong**: when Kul Tiras loses its HQ, the game says the HQ was regained, although it
  was destroyed for good.
- [ai] **Dalaran / Gilneas fallback does not swap teams on the scoreboard.** Gilneas should use the same fallback as
  Dalaran (same player slot), with Gilneas City as its HQ.
- [you/ai] **Auto-surrender interferes with fallbacks**: the rule that makes players surrender with too few CPs can
  stay, but 1 CP should be enough to survive.
- [ai] **New Kezan units are invisible**: the unit count goes up during production, but nothing can be seen.
  First check whether the models are only Git LFS pointers in the local checkout (`git lfs ls-files | awk '$2=="-"'`
  lists them; `git lfs pull`, else fetch the assets from the original repository and `git lfs push origin`).
  Probably assets: models / textures reused from the 24-player map need adjusted paths or must be copied over
  (also seen with the builders in 0.39.6: `-id` on the goblin hall while it trains).
- [you] **Old Gods balance -- re-check**: "too weak" was measured WITHOUT bot income. In 0.45.0 (bots with income)
  the Old Gods took all of Kalimdor; the Kalimdor team had 0 CPs at 45 min.
- [you] **Quel'thalas survives very long** (0.45.0: alive at 540 min with 12 CPs and 90-106 structures all game, a
  small army most of the time). Bot side: caster spam (94 % casters) and neighbours busy elsewhere (`../Ai/TODO.md`
  0.45.0 results). Map side: is the area unusually fortified (towers, walls, the Elfgate)?
- [you] **Stormwind Castle CP needs a buff** (the 100-gold version).
- [you] **South Alliance vs Naga / Fel Horde**: the South Alliance needs a buff (or Illidan / Fel Horde a debuff).
- [ai] Hint "When a player leaves, their units are refunded ..." (`Setup/HintSetup.cs`) is wrong since bots take over
  leavers (0.39.2).

## Fallbacks -> `FALLBACKS.md` (to be renamed to a general "timeline" document in the restructure)

- [ai] **Must-have**: WL's defeat makes the player an observer -- the fallback must run BEFORE it (a hook in
  `Faction.Defeat` or where WL eliminates a faction). Needs a `[SL]` hook in WL code.
- [you] HQ choices per faction (table in `FALLBACKS.md`); the real fallback factions instead of the placeholders.
- [ai] Losing conditions for Escape to Theramore / The Scarlet Crusade, then re-enable them.
- [you] **Arthas when Lordaeron turns Scarlet Crusade early**: "The Culling" needs Lordaeron's "Line of Succession";
  proposal: give the Scourge its Arthas unlock when Lordaeron becomes the Scarlet Crusade before it.
- The HQ list is shared with the bot's planned "guardian summons" (`../Ai/TODO.md`): it should come to the bot
  through `AiApi`, not from this folder.

## Kezan Enterprise

- [you] Check in the editor / game: Trading Center placement, the creep list, stock refresh, oil income on the
  scoreboard; buildings / tooltips; Bilgewater Harbor ships.
- [you] Kezan Oil Supply and Trading Center should not be buildable -- check which builder offers them.
- [you] **Alternative win condition**: Kezan wins by owning a lot of gold (~20,000). Oil buildings would need
  upgrades that raise the income.

## Map content, design and ideas (brainstorm)

The maintainer's own ideas go here too -- half-baked is fine.


- [you] **Pseudo-flyers**: air units lose real flying and become pseudo-flyers, because bots handle flying pathing
  badly (orc air units fly over the ocean). Affects the bot too -- see `../Ai/TODO.md`.
- [you] **Book of Medivh redesign**: the Book gets its own spell; if the Scourge or the Legion gets it, it can trigger
  the summon on that spot. The hero would need to be indestructible while channelling (bots cannot defend it).
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`.
- [you] Islands still free for neutral roadblocks (Zandalar trolls -- 24p: God King, Gurubashi, Amani, Sandfury).
- [ai] (low) Chinese localization from upstream WL (`Loc` system).
- Later, switch paths (mostly covered by `FALLBACKS.md`): Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the
  betrayal, Dalaran -> Kalimdor, Sentinels -> Maiev, Warsong / Frostwolf -> Dragonmaw, Stormwind -> Honor Hold.
