# TODO -- Shattered Legacies (fork content)

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Bot work is in
`../Ai/TODO.md`. Move finished items to `../Ai/CHANGELOG.md` (one changelog for the whole fork).

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
- [you] **The Old Gods are still too weak.**
- [you] **Stormwind Castle CP needs a buff** (the 100-gold version).
- [you] **South Alliance vs Naga / Fel Horde**: the South Alliance needs a buff (or Illidan / Fel Horde a debuff).
- [ai] Hint "When a player leaves, their units are refunded ..." (`Setup/HintSetup.cs`) is wrong since bots take over
  leavers (0.39.2).

## Fallbacks -> `FALLBACKS.md`

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

## Map content and design

- [you] **Pseudo-flyers**: air units lose real flying and become pseudo-flyers, because bots handle flying pathing
  badly (orc air units fly over the ocean). Affects the bot too -- see `../Ai/TODO.md`.
- [you] **Book of Medivh redesign**: the Book gets its own spell; if the Scourge or the Legion gets it, it can trigger
  the summon on that spot. The hero would need to be indestructible while channelling (bots cannot defend it).
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`.
- [you] Islands still free for neutral roadblocks (Zandalar trolls -- 24p: God King, Gurubashi, Amani, Sandfury).
- [you] Idea: tie the food cap to the CP count.
- [ai] (low) Chinese localization from upstream WL (`Loc` system).
- Later, switch paths (mostly covered by `FALLBACKS.md`): Quel'thalas -> Sunfury, Scarlet Crusade -> Legion at the
  betrayal, Dalaran -> Kalimdor, Sentinels -> Maiev, Warsong / Frostwolf -> Dragonmaw, Stormwind -> Honor Hold.
