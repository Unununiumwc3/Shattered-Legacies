# Upstream alignment -- Warcraft Legacies 5.x into Shattered Legacies

The fork stays on Warcraft Legacies **4.8.2** (`baff9d5c`). Changes from WL 5.x come in **by cherry-pick only**.
This file is the ledger: what WL changed after 4.8.2, what is ported, what is wanted next, and what the fork does
**not** want (decided by the maintainer). Update it with every port.

- WL source compared: `main` **5.0.4** (`a0c73082`, 2026-10-07) and `dev` (`ed63e89a`). The 4.8.2..dev range has 94
  commits; 5.0 came to `main` as one squashed merge (#4196), so compare against `dev` for the single PRs.
- How to compare: for each file a WL PR touched, compare the blob hashes of WL 4.8.2, WL 5.0.4 and the fork:
  fork == 5.0.4 -> ported; fork == 4.8.2 -> not ported (safe to take WL's file); fork differs from both -> merged by
  hand / fork changes, read the diff; missing -> not ported.
- Compile check without NuGet: `tools/shattered-legacies/compile_check.sh` (0.72.0).

## Decisions (maintainer, 2026-11-24)

| Topic | Decision |
|---|---|
| WL 5 removes Black Empire, Ahn'Qiraj, Skywall | **Not wanted.** The fork keeps the Old Gods and Skywall. |
| Great War 7v7, forfeit at 5 votes (#4188) | **Not wanted** (follows from the above; the fork stays 8v8 / its own teams). |
| South Kalimdor guard (#4126 `SouthKalimdorGuardSystem`: pushes everyone out of south Kalimdor until turn 15) | **Not wanted** -- the Old Gods live there. |
| Sulfuron Spire hidden in Ashenvale (#4109) | **Not wanted** -- the fork uses it for Skywall's Fireland Invasion. |
| `AhnQirajSetup` (5.0.4 hotfix: Gates of Ahn'Qiraj and C'Thun invulnerable) | **Not wanted** -- Ahn'qiraj is a faction here. |
| Orc island opening (Countdown to Extinction, Sea Witch assault, troll rescue, ship repair peons) | **Not wanted** -- 0.69 chose the Durotar mainland start ("Build Orgrimmar"). |
| WL 5 Kezan rework | **Not wanted** -- the fork restored its own Kezan (0.69). |
| Sentinels-only lobby choice (#4178) | **Partly**: Sentinels become the only START. The Draenei (The Exodar) **keep their terrain** and become a **fallback** of another faction (which one: open, see below). Until then the lobby pick stays. |
| Ironforge "Secrets of Uldum" artifact pool (Demon Soul, Gloves of Ahn'Qiraj, Thunderfury, Fandral's Flamescythe) | **Unchanged (WL).** The Twilight's Hammer gets a replacement instead (0.72.0: Hammer of Twilight). |

## Ported

| What (WL PR) | Fork version | Notes |
|---|---|---|
| Orcish Horde + Tauren Tribes rosters, quests, art, unit abilities, Thrall (Elemental Convergence), Magatha, Rexxar, Roots of the World, rebuilding Orgrimmar, the Long March (#4119-#4194) | 0.69 western port (`c1293066`) | the orc start is the fork's mainland one |
| 5.0 balance data (#4100 Vampirika, most of #4189) | 0.69 | ability / unit files identical to 5.0.4 |
| Choice dialog desync (#4192) | own fix | the fork's `ChoiceDialogPresenter` creates the buttons on every client |
| Long March creep layout | 0.70.3, 0.71.0 | hill archers removed, Thousand Needles / leg-two camps as in 5.0.4 |
| **Bundle 1 -- hero kits and artifacts** | **0.72.0** | 0.69 had ported the hero DATA without the code: 13 buttons did nothing |
| -- Tyrande: Lunar Sanctuary (#4149, crash fix #4182) | 0.72.0 | |
| -- Naisha: Glaive Trap, Watcher's Focus (#4151) | 0.72.0 | replaces Quick Knives in the hero-trait list |
| -- Priestess of the Moon elite: Light of the Goddess, Mass Wind Walk, Elune's Protection, Moonlit Arrows (#4191, hotfix 5.0.4) | 0.72.0 | Wind Walk moved from Archers / Huntresses to her (unit data) |
| -- Warden elite: Spirit of Vengeance trait; Vault of the Wardens rework (#4176) | 0.72.0 | |
| -- Sentinels elite choice Priestesses (RV01) / Wardens (RV02), hero levels (#4181), Tyrande / Maiev / Naisha dialogue (#4179) | 0.72.0 | bots research RV01 |
| -- Anasterian: Sunfire Barrage (#4152) | 0.72.0 | old Phoenix data files left in place (unused) |
| -- Rivendare: Corruption Mine (#4155) | 0.72.0 | replaces Death's Advance in the hero-trait list |
| -- Cenarius: Tree of Renewal, Lord of the Forest, Master of Nature progression (#4163) | 0.72.0 | |
| -- Heartwood Ancient: Undying Grove, Spirits of the Forest, Root Tap; Keeper: Living Wall; Druids elite choice Keepers (RK01) / Heartwood (RK02) (#4174, #4158) | 0.72.0 | bots research RK01 |
| -- Malfurion: Emerald Dream, Seed of Rebirth (#4150, #4165) | 0.72.0 | |
| -- Antonidas: Arcane Cataclysm (#4156) | 0.72.0 | Dalaran Shield: bundle 2 (0.73.0) |
| -- Artifacts: Soulseeker (Kel'Thuzad), Gorefiend's Truncheon (Teron), Bow of Elune's Grace (Tyrande), Horn of Cenarius restores mana and goes to Cenarius (#4183) | 0.72.0 | |
| **Bundle 2 -- Dalaran** | **0.73.0** | |
| -- Dalaran Shield + Power Generator, tidy-up (#4103, #4129) | 0.73.0 | 5.0.4 versions (dev's later cooldown / cancel changes not taken); the generator stands at WL 5's spot (9216,4736) in the fork's 4.8.2 city |
| -- Clickable Dalaran Way Gates (#4087) | 0.73.0 | data was already in (0.69); bot support: `AiPortals.AddDalaranGates` (owner + allies use an operational pair; bots do not build gates) |
| -- Dalaran caster teleport (#4185) | 0.73.0 | `RDTP` at the Hall of the Arcane, Teleport on Geomancers |

Fork deviations inside bundle 1 (deliberate):
- Bot order strings (Ncl6) changed so the bot's script orders are not refused: Sunfire Barrage `shockwave`,
  Lunar Sanctuary `blizzard`, Lord of the Forest `earthquake`, Undying Grove `monsoon`, Elune's Protection
  `thunderclap` (players cast them as before).
- Siege Ancient limit stays 6 and Redeemed Highborne 12: WL 5.0.4 ties 4 / 6 to "Wrath of the Wild" and the reworked
  "Return to the Fold" (not ported, bundle 3).
- Not ported with the kits: WL's `InfoQuests` texts, Druids / Sentinels income (190 / 195) and intro texts, Cenarion
  Hold given to Neutral Passive (5.0.4 `DruidsFaction`), the Immortality regions (bundle 3).

## Wanted, not ported yet

| Bundle | What (WL PR) | Interacts with | Notes |
|---|---|---|---|
| 3 Night Elves | Immortality regions, `WorldTreeProtection` (#4184) | Druids, Sentinels powers | replaces the `Capital` list of the Immortality power |
| 3 Night Elves | Druid quest Wrath of the Wild (#4172), Siege Ancient limit 4 | Druids | |
| 3 Night Elves | Sentinels quest changes: Feathermoon, Scepter of the Queen / Return to the Fold, Redeemed Highborne 6 (#4172, #4189) | Sentinels, Illidari (Scepter) | |
| 3 Night Elves | Druids / Sentinels income and intro texts (#4189), Cenarion Hold to Neutral Passive | Druids | balance |
| 4 Sync | Hotfixes 5.0.1-5.0.4: Boralus quest, Kil'jaeden's Command, `Team`, `FactionManager` lobby language (#4198, #4199, #4203) | small, separate | no own playtest -- check in any game |
| 4 Sync | Spell Conduction without buffs (#4080, Ahn'qiraj), tentacle hits not the primary target twice (#4097, Old Gods) | Old Gods | the fork keeps these factions, so the fixes apply |
| 4 Sync | `InfoQuests` texts of the new kits | -- | text only |
| low | Chinese runtime localization + overlay build (#4146, #4147, #4157), locale CLI commands | build | large, no gameplay |
| parked | Earthmother's Cradle (#4168, Tauren reward) | Skywall (Shimmering Flats, Firelands) | design review first |

Not from WL 5 but often named with it (fork roadmap): Dark Trolls / Hakkar (24p), the Quel'Thalas split, Sunfury
balance.

## ID collisions of the 0.69 port (repaired in 0.73.0)

WL 5 reused the ids of objects it removed (Ahn'Qiraj, Skywall). This map still has those objects, so a WL 5 object
under such an id overwrites one of ours. The 0.69 port did that five times; 0.73.0 restored the 4.8.2 data and gave
WL 5's objects fork ids. **When porting from WL 5: map these ids, and check every other `M` (modified) object file
for a changed base / name before copying** (the remaining ones -- `A01D`, `A09Q`, `n049`, `o006`, `ocat`, `owyv` --
are harmless: same role or a dead faction).

| WL 5 id | WL 5 object | This map's object (4.8.2, restored) | WL 5 object here |
|---|---|---|---|
| `A000` | Frenzy (Orcish Horde) | Shifting Sands (Ouro, Ahn'Qiraj) | `AZo0` |
| `A00Y` | Teleport (Sea Witch) | Frost Bite (Neptulon, Skywall) | `AZo1` |
| `R003` | Improved Frenzy | Progenesis (C'Thun) | `RZo0` (requires `ofrt`, not WL 5's `o02S`) |
| `RZ02` | Blademasters | Shadow Weaver Adept Training (Ahn'Qiraj) | `RZo1` |
| `n00K` | Darkspear Isles (CP of the island start) | Tunnels of Ahn'Qiraj | not used (no island start) |

## The Draenei (The Exodar) -- from start to fallback

**On hold (maintainer, 2026-11-25): "we keep draenei as they are for now"** -- lobby start unchanged; the plan below
stays for later.

Decided: terrain stays (Azuremyst Isle, Exodar Regalis, Silvermyst Island); the faction stops being a lobby start and
becomes a fallback. **Which faction falls back to it is open.** Things to separate from the Sentinels first:
- The lobby pick (`FactionChoiceDialogSetup`: Sentinels / Draenei share slot P19) and the bot's pick (`AiChoices`).
- Maiev's Watchers is the fallback "for both" (`TimelineCatalog`, P19) -- Sentinels only after the change.
- The Draenei-only turning points: The Triumvirate (D15) and the placeholder "The Exodar" fallback ("Draenei of
  Shattrath", Hq Exodar Regalis `e01X`).
- Shared object data: the Exodar's hero types carry Druids suffixes (`E01I` Ageless One, `E01J` High Shaman); the
  Sentinels' Maiev quests / outpost were tested "with both Sentinel and Draenei openings".
- Bot data: `AiConfig` heroes for "The Exodar", the Silvermyst <-> Exodar Regalis edge.

Candidates (0.72.0 proposal, for discussion): **Druids** (same team; the Exodar keeps the Night Elves at two players
when Hyjal falls; replaces the "Dream trees" placeholder) or **Gilneas** (placeholder fallback; the Gilneans fled to
the night elves in lore).

## Test plan for a port bundle

One build per playtest: one content bundle + bot changes (bot changes have switches and are read from the dumps).
A bundle is checked in a short session with the debug command **`-testkit`** (0.72.0; only where the bot's control
commands work, never in a release build): `-testkit` spawns the bundle's heroes (level 10, full mana) and elite
units, `-testkit items` the new artifacts, `-testkit foes` eight hostile footmen to cast on. `ShatteredLegacies/Testing/TestKit.cs`
holds the lists -- update them with each bundle.
