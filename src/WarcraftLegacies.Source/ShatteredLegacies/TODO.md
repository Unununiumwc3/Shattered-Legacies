# TODO -- Shattered Legacies (fork content)

**Current handoff: the 0.87.x playtest items below** ("WL 5.0.4 alignment" holds the playtest lists of every
release; `PLAYTEST-0.69.md` is historical). The fallbacks as they are: `Timelines/TIMELINES.md` section 0.

`[you]` = needs a decision or in-game data from the maintainer. `[ai]` = implementation work. Finished items go to
`CHANGELOG.md`. **This file holds everything that exists only in Shattered Legacies**: timelines, new factions, map
content, the fork's win rule and game length, balance, known issues, the maintainer's ideas. Bot work (what would go
into the upstream PR: `Ai/` + `[BOT]` hooks) is in `../Ai/TODO.md`. Fork code uses the bot only through `Ai/AiApi.cs`.

Design: `Timelines/SCENARIO_CONCEPTS.md` (what) and `Timelines/TIMELINES.md` (how, incl. the faction template).

## WL 5.0.4 alignment (0.72.0; ledger: `UPSTREAM.md`)

- **Bundle 1 playtest (0.72.0)**: `-testkit`, `-testkit items`, `-testkit foes` near a free spot; cast every new button
  once (Tyrande, Naisha, Anasterian, Rivendare, Cenarius, Malfurion, Antonidas, Priestess / Warden / Heartwood /
  Keeper elites), equip the four artifacts, research both elite choices once per faction (the other must lock).
  Twilight's Hammer: The Hammer of Twilight drops at Suramar; Ironforge's Uldum expedition can still give Thunderfury.
- **Bundle 2 playtest (0.73.0)**: `-testkit dalaran` (as Dalaran or any player): Antonidas beside the Power
  Generator -> Dalaran Shield (dome, undead hurt inside, Cancel Shield ends it); far from it -> refused with a message.
  Build two Way Gates with the Farmers -> travel both ways; a bot ally's army should use them (dump: "Dalaran Way
  Gates open"). Geomancers / Mages: Teleport after Teleportation. Check AQ's Ouro (Shifting Sands), C'Thun's
  Progenesis, Skywall's Frost Bite, and the Orcish Horde's Frenzy / Improved Frenzy (Fortress) / Blademasters.
- **Draenei**: the Legion's fallback since 0.80 (the 2026-11-25 "keep them as they are" was overtaken; `UPSTREAM.md`).
- **Bundle 3 playtest (0.74.0)**: Immortality card lists the four regions (green = held); a unit outside them is not
  saved; a unit is saved once. Druids: destroy Orgrimmar -> Wrath of the Wild (Siege Ancients 8, +150 life). Sentinels:
  Return to the Fold -> 12 Redeemed Highborne. Cenarius dies -> his ghost comes back (bot and player).
- **Ashtongue**: Shattrath / Halaa stand on their spot (fallback and after purification), click circle = model; the
  blue channel from Shattrath to the site being purified.
- **Decide**: Cenarion Hold to Neutral Passive (WL 5) or kept as the Druids' outpost against Ahn'Qiraj (`UPSTREAM.md`).
- **Bundle 4 playtest (0.75.0)**: lobby description and loading screen (English and Chinese client), the start message,
  F9 "How Shattered Legacies works" and Credits, new hints; Boralus fails at turn 11; Kil'jaeden's Command with
  Nordrassil destroyed; Ahn'Qiraj's Spell Conduction; Old Gods tentacles.
- **WL 5.0.4 alignment**: the four bundles are in. Left: the open Cenarion Hold decision, the parked Earthmother's
  Cradle, and the object-data audit of the remaining factions (`UPSTREAM.md`).
- **0.76.0 playtest**: dump "quest runner" (who was sent where, done / failed); Illidan's Broken Isles once its area is
  clear. Skywall: take the Vortex Pinnacle CP with another team -> Skywall changes sides; its halls stay; it never
  dies. Stormwind Keep lost -> Gilneas (North Alliance, Gilneas CPs, Genn); Dalaran has no Gilneas choice and no
  Greymane Wall; nobody walks into Gilneas before.
- **0.79.0 playtest**: snipe a bot's HQ -- after 5 s its far troops and its allies' appear beside it (text + ping,
  dump "recall"); a bot Kezan builds rigs and tankers (log "kezan oil bot"); Twilight owns Dragonmaw Port on arrival;
  Sylvanas (Dark Ranger / Queen) learns Dark Lady's Command at 6 and the bot casts it.
- **0.89.0 (no playtest yet)**: P20 in the lobby as Twilight's Hammer (Old Gods); its base one second in (Grim Batol,
  the Citadel camp at the Southern Highlands -- buildings placed well? the ogres gone?); the Elven Gate of the pass (opens
  and closes? the bot keeps it open?) and the two watchtowers; "The Old Pact" riders; the Dragonmaw fallback at the port
  (Grim Batol lost + Cho'gall dead); an empty P20 (defeated at 2 s, nothing placed); Forsaken split into P11 / P14.
- **0.88.0 (no playtest yet)**: Theramore taken at Jaina's Sanctum, Jaina a dreadlord under an evil holder and back;
  "The Nexus" (a minute's channel) and Kalecgos' Dragon Form (skills kept? attribute?); Edwin after 1,500 pillaged;
  Benediction from Majordomo; Mmmrrrggglll and Snarlmane with their artifacts; the Claw of Ursoc (free furbolgs, the
  first loss, hostile satyrs); the Broodmother taken at 0 life; Acherus rises, flies, carries, burns and mends (bot
  movement?); the harbours (70 % / the north fallen); the plague boars (do they walk? zombies? dispel?); Maraad after six
  turns; Alexstrasza's Chains (drakes, Nek'rosh); the elite locks (only own researches visible on the Hourglass? bots
  pick?). Models to check: Amnennar (RazormaneChief), the Broodmother (Meat Wagon).
- **0.87.1 (no playtest yet)**: the Hourglass shows no second fallback for Gilneas or the Draenei (rows gone); Skywall's
  info row "Conquered, not defeated" still shows; the Exodar has no preplaced shipyards (the Draenei can still build
  them).
- **0.87.0 (no playtest yet; 0.86.1 builds and runs -- maintainer)**:
  - The Exodar: the Legion beaten -> the ship lands near the conqueror's home; walk in through it; after "The
    Dimensional Ship" use Set Course on the Dimensional Generator -- 90 s flight, gates shut. Take the bridge with an
    enemy and try to fly (refused), retake it.
  - Furbolgs: the lodge by Nordrassil leads into Timbermaw Hold (shrine, CP).
  - Dark Iron: the 24p troops and models (`git lfs pull`), the Throne of Thaurissan, a Gatebreaker every 5 turns at
    Shadowforge City, the statue's aura for the Siege Foundry's holder, the Black Market.
  - Dungeons: only the seat on arrival, creep camps on the other CPs.
  - Horde -> Zandalari always; Fel Horde -> Dragonmaw always; Sunfury / Gilneas / Draenei conquerable (Gilneas' mercy
    text); Stromgarde passes to Lordaeron.
  - Turn 30: the Kul Tiras <-> Kezan ferry and its announcement.
  - Still open:
    - Maiev as a sub-path (on hold, maintainer)
    - Twilight's Hammer (on hold since 0.87)
    - satyrs as the Druids' alternative
    - the Horde dies too early -- needs a game's dumps
    - the 24p quests: the 24p repository is scanned (0.87.1) -- what to take is in "6. Faction by faction"
- **0.86.0 (no playtest yet)**:
  - Dark Iron: the Great Forge lost -> Shadowforge opens, town inside, Mercenary Charter (tribute and golems for the
    Siege Foundry's holder), "The Great Anvil".
  - Furbolgs: Nordrassil and Cenarion Hold lost -> the Totem Lodge, levy and strength by world trees held.
  - Still open then: all built in 0.87.0 except Maiev (on hold) and the satyrs -- see 0.87.0.
- **0.85.0 (no playtest yet, maintainer: build ahead, test in bulk)**:
  - Defias: Stormwind Keep lost -> the Deadmines open; Pillage on every unit; recruits each turn; quests.
  - Centaur: Thunder Bluff lost -> Kolkar camp; levy each turn; the khan quests and "The Five Khans".
  - Harpies: N'Zoth lost -> the nests at Stonetalon become theirs, the Roost trains the heroes.
  - Skywall from C'Thun's loss; Gilneas from Lordaeron's Capital Palace.
  - Still open then: all built in 0.86 / 0.87 except Maiev (on hold) -- see 0.87.0.
- **0.84.0 playtest** (`git lfs checkout` first: naga models):
  - Before Auberdine and Feathermoon fall, the Nazjatar, Shadowforge and Deadmines gates stand but stay shut.
  - Sentinels beaten -> the Naga of Nazjatar: their base in Nazjatar becomes yours and the circle by the Maelstrom
    opens. You get the four Nazjatar CPs, join the conqueror's team, and the temples cannot fall.
  - The quests unlock Azshara, Atoph and the Immortal Guardians.
  - Look at the gate models (circles, gold mine in Westfall, Shadowforge City building, Undercity iron gate) and at
    the Deadmines' new look.
  - Next: Maiev's "Chase Illidan to Outland" as a sub-path (engine support for SubPath), the Dark Iron in
    Shadowforge, the Defias in the Deadmines, naga flavour (amphibious exit at the Maelstrom).
- **0.83.0 playtest**: walk through the three open gates and back: Nazjatar (by the Nazjatar CP at the Maelstrom),
  Shadowforge (east of Shadowforge City), the Deadmines (Westfall, above the bandit camp). Capture a dungeon CP. Check
  edges for holes or odd cliffs, and missing doodads (a type the 24p map only modifies is stock here). WL's unused
  squares in the south-west corner are gone (void). The Undercity
  CP goes to the Forsaken with "The Throne of Lordaeron". Bots: the dump shows the dungeon gates as "edge".
  Next: redecorate the Deadmines; the fallbacks that live there (naga of Azshara, Dark Iron, Defias); maybe 24p
  creeps; the Forsaken join the Undercity's holder.
- **0.82.0 playtest** (void-area dungeons, `Timelines/VOID_DUNGEONS.md`): the north-west corner of the map (-27136,23100): the 24p
  Undercity in the north-west corner (terrain, doodads, no holes or cliffs at its edge); the Forsaken's buildings stand
  there, Neutral Passive and invulnerable. "The Throne of Lordaeron" -> gate in the palace ruins -> you arrive at the
  southern entrance; the inner gate leads back. A bot's Forsaken army leaves and enters through it (dump "WL waygate
  ... edge"). Next dungeons: Deadmines (confirm: the 24p Temple of Ahn'Qiraj terrain, redecorated as a mine?),
  Shadowforge City (Dark Iron), Nazjatar (naga) -- places proposed in VOID_DUNGEONS.md; the Undercity CP (Forsaken
  join its holder).
- **0.81.0 playtest**: dump "ferries ... boarded N" > 0, "waygates (~30)"; the Legion leaves Argus; no "WC3 refused"
  hundreds of times, dead fork heroes come back; no "ResearchId should be named" at the start; Horde beaten by a
  non-Old-God -> Zandalari.
- **Discussed 2026-11-26**: Twilight's place (Dragonmaw Port between the alliances and the Fel Horde); Undercity from
  the 24p 1.18a (rect -28672,14176 .. -25728,17888 -- empty and unwalkable in this map) -- plan in the 0.81.0 reply.
- **0.80.0 playtest**: Orgrimmar taken by a non-Old-God -> the Dragonmaw at Dragonmaw Port (by an Old God -> Twilight);
  Twilight with Cho'gall dead and Grim Batol not theirs -> the Dragonmaw (cult units hostile); C'Thun lost -> the
  Zandalari at Zuldazar; all three serve whoever holds their seat and cannot be defeated. Models: `git lfs checkout`.
  Frozen Throne lost + Lich King dead -> the Ebon Blade (4 heroes, Acherus); Legion Stronghold lost -> the Draenei on
  Azuremyst, joining the conqueror; the Sentinels' slot no longer offers the Draenei.
- Next: Dragonmaw quests (Grim Batol, the red dragons), the Zandalari's Hakkar endgame (decide), the 24p dungeons
  (coordinates from the maintainer), a third Dragonmaw hero (Zaela? no model).
- **0.78.0 playtest**: a bot's gates open within 15 s (dump "gates ... opened by bots N"); Ahn'Qiraj owns its gate
  from the start; a bot's Forsaken units say "Forsaken" on hover; the pinned "ferries: first voyage" line appears.
- **Discussed 2026-11-25 (not built)**: HQ emergency recall, Kezan oil for bots, Dragonmaw Port as Twilight's CP,
  Twilight's Old Gods support (portal weight), conquerable fallbacks, Inspire Madness for Sylvanas, the 24p trolls
  (Zuldazar), Dalaran / Gilneas shared quests, 24p dungeons into this terrain.
- **0.77.0 playtest**: gates stand everywhere again; a bot's gate opens within 30 s and stays open (dump "gates"); the
  Greymane Wall is shut (gate invulnerable) until Stormwind falls, then Gilneas holds it. Illusions keep their
  caster's name. Chinese client: books, commands, quest rewards translated.
- **Decide**: Earthmother's Cradle as the Tauren's reward for beating the Old Gods (discussed 2026-11-25: needs WL 5's
  Tauren buildings and the Un'Goro camp placed into this map's terrain).
- [ai] Gates, next: a player's closed gate on a bot's route (ask an ally? attack an enemy's?).
- [ai] Quest runner, next: objectives "hostiles in an area dead" as army targets, so quests like Illidan's Broken
  Isles get cleared too; bots using closable gates (discussed 2026-11-25).
- [ai] Bots and Dalaran: they neither build Way Gates nor use the Dalaran Shield on purpose (the engine may cast it
  when Antonidas happens to stand beside the generator in a fight).

## Left from the western port (0.69) and the Twillight pilot (0.68)

The western port (WL 5.0.4's Orcish Horde, Tauren Tribes and their Kalimdor openings) and the shared Twillight claim
are history: the Horde falls to the Zandalari, the Fel Horde to the Dragonmaw, Skywall is Ahn'qiraj's (0.80 - 0.87).
Left from them:
- [you] The Tauren Earthmother's Cradle reward and Skywall's overlapping Shimmering Flats / joint Firelands invasion
  quests -- parked for design review (see "Decide" above).
- [you] The Orcish Horde's upstream Kezan reward vs the Kezan Enterprise (does it still clash?).
- Dark Trolls / Hakkar (the Zandalari's endgame), Sunfury balance, Ironforge artifact rewards -- separate follow-ups.

## Order of the work

The structure comes first, so that every later faction is the same template filled with different content.

0. ~~Structure (0.52)~~: folders, rename to timelines, separate docs for bot and fork, tools in `tools/shattered-legacies/`.
1. ~~Content index~~ (after 0.52.0): `Content/` -- left: the import tool (below)
2. ~~Turning-point core~~ (0.53): quests + Hourglass researches + effects + defeat rule, pilot Quel'thalas -> Sunfury
   -- left: the playtest checks (below)
3. **Kul Tiras -> Kezan Enterprise rebuilt** (~~0.58: own `Faction`, the 24p goblin tree, quests, Oil Rigs; 0.59:
   the 24p Trading Center and trade, Oil Refining, the end goal on the scoreboard~~): playtest 0.59, the items below
4. **Forsaken -- polish** (~~0.54-0.57: betrayal, split, 24p tree, Sylvanas' phases, gates, Undercity~~): the
   small issues below, the five 24p quests, Quel'thalas' late game
5. **Light turning point**, pilot: Illidari -> draenei remnant
6. **Lordaeron pick** Arthas / Garithos
7. **Faction by faction** (concepts)
8. **Shared mechanics**

### 1. Content index -- left over

Done: `Content/` (`Content/README.md`), generated by `tools/shattered-legacies/content/content_index.py`.
- Done (0.55): `tools/shattered-legacies/content/import_24p.py` (import sets per faction; `Content/SL_OBJECTS.md`).
- [ai] (optional) The index's "unused" is a heuristic (stock ability data and anything outside the C# code are not
  followed): before deleting an "unused" object, check it in the editor.

### 2. Turning-point core -- playtest and leftovers (0.53)

- [you] **Playtest the Hourglass of Fate**: every player sees only his own research (others hidden by object limits);
  several hidden researches share button (0, 0) -- does the visible one show? Greyed out with "Requires: ..." until
  the HQ falls, clickable after, greyed again when the HQ is retaken. Lordaeron sees the Scarlet Crusade research
  (WL's requirements). Fallback: one Hourglass copy per slot (`gen_hourglass.py`).
- [you] **Playtest Quel'thalas -> Sunfury** in the middle of a game (cheat the HQ away): units removed / CPs neutral
  hostile, Sunfury's start units, Kael'thas, the Sunfury quests (Tempest Keep, biodomes ...), team Outland, colour kept.
- [you] **Playtest the defeat rule**: a human with 0 CPs gets 5 warnings, then his fallback (if unused) or defeat.
- [you] Should the rule also apply to bots? WL's elimination only checks humans; bots with 0 CPs live on (as before).
- Decided (maintainer): Sunfury's Netherstorm areas and its Well of Eternity (`n0DZ`) stay all game, even when
  Quel'thalas never falls -- nothing is removed.
- [ai] Texts of the Hourglass researches and quests are drafts (`gen_hourglass.py`, `TimelineCatalog`) -- `[you]` final
  wording.

### 4. Forsaken -- polish

- [you] **Playtest 0.57.2**: in the lobby P11 / P14 / P20 show "Computer" and cannot be taken; bots complete their
  starting / deadline quests in the last 30 s of the turn before (the dump pins "bot quest COMPLETED"), also the
  successor factions on a bot slot (Sunfury after Quel'thalas' fallback).

**0.82.0**: the Undercity left the squares (24p terrain in the north-west void, `Timelines/VOID_DUNGEONS.md`); the
"real terrain, by script" way below is built (`tools/shattered-legacies/terrain/copy_area.py`).

**The south-west squares (the Undercity until 0.81; void since 0.83, Nazjatar stands there)**: WL uses `Regions.HideUnitBottomLeft` only once, at
setup, to hide Neutral Passive units placed there (none are); no other WL or bot code uses those squares (checked
0.57.2). They are safe as long as everything placed there is created after that setup (`ForsakenUndercity` waits 1 s).

**Terrain polish (discussed 0.57.2, not built)**: the Undercity square is plain terrain. Two ways:
- Without terrain changes: ground textures by code (`CreateImage` / `SetImageRenderAlways` -- a texture drawn on the
  ground, e.g. green slime "rivers"), stock ubersplats (`CreateUbersplat`), effects that lie on the ground
  (`AddSpecialEffect` + `BlzSetSpecialEffectZ` / scale -- water planes, slime pools), destructables as decoration.
  Cheap, no editor; looks like decoration on top, not like real terrain (no real water, no cliffs).
- Real terrain, by script: the repository already holds the terrain as JSON -- `mapdata/WarcraftLegacies/
  Environment.json` (tiles, heights, water, cliffs), `PathingMap.json`, `Doodads/` -- and the CLI builds the map from
  them. A tool (like `import_24p.py`) can paint tiles, lower the ground for a river, add water and doodads in a
  rectangle, and regenerate the pathing there. No WC3MapTranslator needed (the Cartographer does the conversion).
  Risk: the pathing map must match; first test on one square. Recommended for the instances (Undercity, the dwarves'
  mountain) when they are polished.

Done in 0.60 (maintainer, 0.59 playtest): the Altar of the Queen attackable from the arrival; the autopilot / bots no
longer train Acolytes as army; Plague Bomb shows its damage (import tool). Check in the 0.60 playtest.

Small issues (maintainer, 0.57 playtest), in the order I would take them:
- [you] ~~**Sunfury on a bot slot builds no troops**~~ (0.59: cause confirmed in the code -- bots never construct
  and the starter production ran only at the game start; now `AiApi.GiveStarterProduction` 3 s after a turning point
  on a bot): check in the playtest that Sunfury / the Enterprise on a bot slot get their production buildings (pin
  "arrival production -- placed: ...") and train.
- [ai] ~~**The Undercity in the bot's node system**~~ (0.82.0: a fixed route node, the gates are portal edges; the CP
  is still open) (maintainer, 0.57.3): the bot does not know the square or the
  gates. Plan without touching the bot's generated graph: an extension point in the bot API --
  `AiApi.AddNode(name, x, y)` + `AiApi.AddPortalEdge(fromNode, toNode, gate)` -- that the fork calls from
  `Timelines/Factions/Forsaken/` (like `RegisterHeroes`): Lordaeron City <-> gate <-> the Undercity. Then the
  Undercity can also get a real control point (a CP unit created there, registered with WL's `ControlPointManager`).
- [ai] The Undercity's gates: a ruin model instead of the waygate (maintainer).
- Explained (0.57.4): Quel'thalas on P11 fought only the Forsaken because it was still allied with the Scourge and
  the Legion (lobby force). Earlier note: after the betrayal, did Quel'thalas on P11 really lose Silvermoon to the Forsaken (the 0.57.2 dump
  shows Silvermoon owned by the Forsaken at 27:25)? The teams were right (P11 in North Alliance); the bots chose
  their targets by their routes -- Scourge / Legion did not go for Quel'thalas. If the Forsaken never took it, the
  split did not move all units.
- [ai] Hourglass of Fate: they show the CP defenders (0.57.1 playtest); the maintainer wants the CP tower upgrades'
  models (e.g. `n02R`'s) -- find the per-faction tower upgrade unit types, map faction -> model.
- [ai] The Undercity's buildings: invulnerable for good, or normal once the Forsaken own them?
- [ai] "Arthas must also exist, so the Forsaken can even die" -- unclear yet (betrayal or Free Will condition).
- [ai] Campaign sounds by game path (no files in the repository): needs the Reforged paths.
- [ai] The Haunted Harbor: check that the Transport Ship (main `ubot`), Corsair and Dreadnought need Navigation
  and that the Graveyard offers it (0.57.1).
- Done (0.57.2): bot quest timing for starts and turning points (last 30 s of the turn before the deadline).


- [you] **Playtest 0.55**: the Altar of the Queen stands invulnerable in Northrend (-1340, 20171) from the start.
  After the betrayal: you get it, Sylvanas "The Banshee", 3 Forsaken Acolytes and 500 gold; Quel'thalas (P11, bot)
  keeps all gold and lumber and has its own Hourglass. The Acolytes build the 24p Forsaken tree (Dark Mansion,
  Grave, Plaguetorium, Graveyard, ...). Sylvanas level 6 -> "The Dark Ranger" (Dark Ranger model), the Forsaken leave
  the Scourge's team, Dreadguard / Dark Ranger / Elder Banshee / Royal Sepulcrum unlock. Then Lordaeron City ->
  the Undercity appears there, "Queen of the Forsaken", the Grand Apothecary unlocks.
- [you] **Models**: after `git lfs pull`, run `python3 tools/shattered-legacies/content/import_24p.py` once more --
  with the real model files it also copies the textures they use (the sandbox only sees LFS pointers). Commit what
  it adds. A model that still shows white or green lacks a texture.
- [you] **Playtest 0.56**: Sylvanas' skills per phase (Banshee: Possession, Anti-magic Shell, Banshee Wail, Ethereal
  Walk; level 6 -> Dark Ranger, skills reset; Lordaeron City -> Queen), her items stay; "Plague Research" (+5 knowledge
  per 100 kills); 150 gold per turn for 10 turns after the betrayal. The freeze at turn 14 (0.55.0): if it comes
  back, a `-dump` shortly before.
- [you] (refinement) An income mechanic of their own: the Forsaken as corpse looters (a Raise Dead variant that
  turns corpses into gold). Kezan's oil: step 3.
- [you] **Playtest 0.57.1**: the NPC slots in the lobby (Computer), after the betrayal Quel'thalas on P11 builds
  troops and, after its own fallback, Sunfury too; holding the Capital Palace -> ruins + gate; the Undercity square
  shows its buildings and gate. Then the 0.57 list: Domination, Ghostform (20 s, buff icon), the Banshee's intelligence; level 6 -> the
  Laboratory / Forbidden Archive / Haunted Harbor; Free Will needs level 10 + "The Culling" + the Scourge below 10
  CPs; Tier 4 only after it; Navigation and Flight at the Graveyard; the Capital Palace's quest marker; the Undercity
  square (visible from the start), its gate opens with "The Throne of Lordaeron"; each faction's Hourglass model.
- [you] Your note "Arthas must also exist, so the Forsaken can even die": which condition did you mean -- for the
  betrayal (Sylvanas slain by the Scourge while Arthas leads it?) or for Free Will? Not built yet.
- [ai] **More instances** (the building block exists: `Core/Effects/Instance`): the dwarves inside a mountain,
  tunnels between squares. The Undercity's buildings stay invulnerable for now -- decide after the playtest.
- [ai] **Campaign sounds without files**: WC3's own sounds can be played by their game path (`CreateSound` with
  e.g. a `Sound\Dialogue\...` path) -- nothing in the repository. Needs the exact paths of the Reforged files
  (a public list or your game's CASC); then quest completions can play campaign lines.
- [ai] **Quel'thalas' late game** (maintainer), so Quel'thalas stays worth playing (not only Sunfury / Forsaken):
  win Alleria Windrunner (exists in 24p) over and remove the Silver Covenant; the final goal "self-sufficient magic"
  -- channel at three places (light, arcane, void), then strong late tech.
- [ai] **The 24p Forsaken quests** (jass/Quests/Forsaken in the 24p repository) -- to rebuild with WL objectives:
  "Secret Buildup" (build 2 Plaguetoriums, 20 Graves, a Laboratory, 2 Graveyards, Haunted Castle -> Putress), "The
  Plaguelands" (Undercity + 5 Lordaeron CPs -> 4 Val'kyr, 500 gold), "The Nine" (Lich King dead + 2 Northrend CPs
  -> 9 Val'kyr), "Retaking the Sunwell" (Sunwell + Undercity -> +500 hit points for Sylvanas and the banshees),
  "Cold-Hearted Revenge" (Lich King dead, Icecrown -> +20 strength / intelligence). "The First Banshee" is our
  betrayal; "Forsaken Independance" is our Free Will + The Throne of Lordaeron.
- [ai] (polish) The Haunted Harbor trains the main map's ships (`ubot`, `udes`, `uubs`) -- not in the Forsaken's
  limits yet; the stay branch: the Scourge gets Sylvanas as a banshee (concepts D3); Sylvanas' abilities per phase;
  texts; a model for "Queen of the Forsaken" (24p `Hero_DarkRangerBoss2` is copied with its unit -- a candidate).
- [ai] (polish) The Forsaken bot: production data exist (`SL_BUILDINGS`); check in a bot game that it builds.

### 3. Kul Tiras -> Kezan Enterprise -- playtest 0.62 and polish

- [you] **Playtest 0.62**: oil starts at 0; an Oil Rig -> a platform whose tank fills (its mana bar); Oil Tankers
  (Trading Center) sail to it, load, come back ("+300 oil" at home), the Oil Supply's mana grows; Refine Oil cast on
  the Oil Supply (any distance) or by autocast pays 100 gold per minute; the Trading Center / Oil Supply cannot be
  hurt; the refinery is smaller; slave camps train slowly and refund when empty; the Venture Bay camp stays;
  druid refugees once Nordrassil AND Cenarion Hold are lost, Skywall's once the Vortex Pinnacle and the Sulfuron Spire
  are; Kul Tiras' fallback no longer goes away.
- [you] **Balance of the carried oil**: 625 per platform per minute, tank 3,000, field 40,000 (64 min at full pump),
  300 per tanker trip (`KezanEnterpriseConfig`). With the platforms 1,200-5,000 from the Oil Supply a tanker makes
  roughly 600-1,000 oil per minute -- 5-8 tankers for 8 platforms.
- [you] **Druids' HQ**: today Nordrassil AND Cenarion Hold must be lost (the catalog's rule: all of them). Nordrassil
  alone? (`TimelineCatalog.cs`, `Hq`).


- [you] **Playtest 0.61**: Gazlowe's Engineering Upgrade (no more empty icons / crash); Alchemist and Wizard trainings
  give their spells; the Oil Supply fills up (scoreboard "Kezan oil: <oil> (+<rate>/min)", +100 per platform), the
  Oil Refinery's Refine Oil is on (autocast border) and pays 100 gold per minute ("+100" over it), Oil Refining 150 /
  200 (the second behind Tier 4); Fuel Transports blow up near enemies (Detonate on); traders need a platform; the
  Trading Center and the platforms cannot be hurt; the camps at your spots, refugees at half price, Sentinels' archers
  at Everlook also in a Draenei game, Black Empire at Gadgetzan; Master of the Slave Trade after one sale per camp.
- [ai] **Kezan's Missile Battery** (the main map's `o05I`, preplaced): 300 attack damage, very slow attack (maintainer).
  It is the main map's object -- copy it for the Enterprise (hand-made, `kezan_objects.py`) and map it in
  `StartArea.Take` (an extra equivalent), or change the main map's creep version (used anywhere else? check first).


- [you] **Playtest 0.60**: the Lumber Mill researches Flight -> Oil Rigs, Trading Zeppelins, War Zeppelins can be
  trained; the Builders appear on open ground in the base; six **Refugee Slave Camps** rise next to Booty Bay,
  Ratchet, Gadgetzan, Venture Bay, Fuselight and Everlook when the Enterprise arrives -- each belongs to whoever holds
  that town's CP (neutral: nobody); its refugees are greyed out ("No refugees from X yet") until X falls (defeated or
  its fallback; Kul Tiras' Navy Snipers at Booty Bay are there at once); a purchase by another player pays the
  Enterprise ("Contract sold", the scoreboard's "Kezan trade"); stock 3, one back every 2 min. Quest "Master of the
  Slave Trade" counts every kind sold once. Plus the 0.59 points (trade, Oil Refining, scoreboard lines).
- [you] **Refugee Slave Camps built one by one** (maintainer: the first quest builds them; for now they are prebuilt).
  Pitch, pick one:
  (a) **Contracts**: a "Camp Contract" item (Trading Center, 1 per town) given to a Builder; at the town it becomes the
  camp. The Builder has to sail there -- the camp appears where the Enterprise actually goes.
  (b) **Trade routes open camps**: a camp opens at a town once the Trading Zeppelins / Traders made N trips there
  (KezanEnterpriseTrade counts trips per town) -- no fighting, just business, and it rewards the trade fleet.
  (c) **Pay the ruler**: a "Bribe <town>" research at the Trading Center per camp (cost rising per camp); the CP's
  ruler must not be at war with ... -- simple, but it is only a button.
  My default: (b) -- it ties the three mechanics (trade, camps, goal) together and needs no new unit.
- [you] **Master of the Slave Trade** reward: today the title on Gallywix and the Intercontinental Artillery. Ideas:
  the camps' stock refills twice as fast, or the Enterprise gets a share of every refugee's upkeep.
- [you] **The end goal reached** -- what does it do? Today: announced to everyone, the scoreboard says "reached". Win
  for the Enterprise, a big reward, or a point in the fork's win rule (below)? Also: "hold 20,000 at once" (my
  default) or "earned 20,000 in total"?
- [you] Balance: refugee prices (the source unit's), limit 12 each, stock 3 / 2 min, which unit per faction and
  which town (`tools/shattered-legacies/content/kezan_objects.py`, tables `CAMPS` / `REFUGEES`); trade pay
  (`KezanEnterpriseConfig.ZeppelinGoldPer1000` / `TraderGoldPer1000`, a full fleet ~+400 gold per minute) and costs
  (60 / 150 gold); Oil Refining; Lumber Market Crash (Nordrassil) is a big ask for an island faction; Gadgetzan's 15
  turns; the Tinker needs Tier 4 (24p data).
- [ai] (polish) The 24p fuel system comes with the data (mechanical units carry fuel as mana, the Oil Supply's Fuel
  Refill aura, the Fuel Transport) -- check in game that it does not leave tanks without mana for good.
- [ai] (polish) Business Expansion (24p: train 16 zeppelins + 10 traders -> the Goblin Dock) is not rebuilt -- the dock
  is there from the start; a quest with another reward could come back now that the traders exist.
- [ai] (polish) Traders stuck at sea: a ship that cannot reach a harbour counts as arrived after 240 s
  (`TradeLegMax`); watch for ships that idle in a corner.

**Kezan concept -- needs more thought** (maintainer, 0.59 playtest):
- [you] **A turtle / tower-defence strategy**: the Enterprise bombs and mines its own coast to defend. Needs more
  tower buildings and mines (goblin land mines exist in the stock data: Goblin Land Mine `nglm`; 24p towers: Missile
  Tower, Improved Missile Tower). Waves keep coming, more and more, while it holds.
- [you] **Too weak when it appears**: Kalimdor has mass armies by then, the Enterprise starts alone with almost
  nothing. How does it buy time? Ideas: Kezan's shore protected for N turns (a storm / reef no ship can cross, a
  bought truce), stronger preplaced defences (towers, mines already laid), a mercenary fleet that guards the island for
  the first turns, or bigger start gold that only towers can use.
- [you] **The goblin towns as trade areas instead of conquest**: holding these CPs is hardly possible (they appear
  too late). Instead the Enterprise takes the town's shops, improves them (better items) and gets the profit for each
  item sold to other players -- like the Refugee Slave Camps, for items.

### 3b. Illidari -> Ashtongue Deathsworn -- playtest 0.64

- [you] **Playtest 0.64**: Halaa is named Halaa and is taken over (not destroyed) when beaten before the Ashtongue
  come; at the fallback: Shattrath, Halaa, Telredor, Oshu'gun as "(fel-tainted)" buildings of yours (indestructible,
  no buttons; quest markers on all four and the temple); Ishanah next to Shattrath cannot move or be hurt; Shattrath's
  mana bar is the purification (no regeneration); every 8 s mana-burn beams from Shattrath to your / your allies'
  units within 1500; at the site being purified: the bubble (not at Shattrath), replenish effects, a ring of light
  pillars -- only while mana flows; each step done: the site becomes its purified building (trains / researches);
  the order Shattrath, Halaa, Telredor, Oshu'gun, temple; at the end: Temple of Karabor, Xi'ri, Ishanah moves (no
  Purify), the scoreboard says Karabor. The temple trains Defenders, Deathsworn, Seers; the Draenei Hut works. Food:
  30 + 6 per Outland CP of your team.
- [you] **Balance**: 12,000 purification (2000 / 2400 / 2400 / 2600 / 2600), every 8 s up to 50 per unit and 160 in
  all, 2 per second by itself (`AshtongueConfig`); 10 gold and 6 food per Outland CP.
- ~~**The faction's name "Karabor"**~~ (0.64: `Faction.Name` -- WL re-keys its tables; the bot knows both names).
- ~~**Evil factions conquered instead of defeated**~~ (0.76 - 0.87: most fallbacks use `Core/Conquest`;
  `TIMELINES.md` section 0).
- [ai] **A "Hold <HQ>" quest for every faction** (maintainer): generated from the catalog's `Hq`, with map markers --
  says which buildings the faction must not lose and what losing them unlocks.
- [ai] (check) The WL quests that use the Black Temple -- Ironforge's "destroy it", the Fel Horde's rebuild and HQ list
  -- after the Ashtongue's temple stands on its spot; Shattrath / Halaar are WL Exodar capitals (in a Draenei game the
  Ashtongue take them).

### 5. Lordaeron pick Arthas / Garithos (D1)

Still open (maintainer, 0.87.1: keep). Design: `Timelines/SCENARIO_CONCEPTS.md` section 8 P10 and section 7 (the
Great Alliance belongs to the Arthas games).
- [ai] A `FactionChoice` for the Lordaeron slot (WL's pick dialog is still used, with one choice, by the Dalaran,
  Sentinels and Illidari slots -- `Setup/FactionChoiceDialogSetup.cs`; `Timelines/Factions/Lordaeron/`).
  `[you]` Garithos as its own `Faction` class or Lordaeron with a game flag -- decide when we build it.
- [ai] **A stronger Garithos** (maintainer, 0.87.1): the main map's Garithos (`Hlgr`: Storm Bolt, Rally, Cavalry
  Onslaught, Mighty Guard) with the 24p **Dark Knights** (`h009`: Dark Hammer, Unholy Plating) as his elite -- the 24p
  "High Commander (garithos)" `E00O` is really Michael Goodchilde and is not him. [you] which kit to keep.
- [you] In Garithos games Lordaeron has no fallback ("the gamble") -- still true now that Gilneas is its fallback in
  Arthas games?

### 6. Faction by faction (`Timelines/SCENARIO_CONCEPTS.md` section 8)

Every slot has its fallback now (`TIMELINES.md` section 0). The old order (Theramore for Gilneas -> Maiev's Watchers ->
Valiance -> Dwarven Expedition -> Twilight's Hammer -> Ebon Blade -> Forsaken -> Dream trees -> Mag'har -> Dreadlord
Insurgents -> Brood of Nozdormu) is superseded; those concepts are listed in `SCENARIO_CONCEPTS.md` section 14.
- On hold: Maiev as a Sentinels sub path; Twilight's Hammer as a faction (its code stays; see Black Empire below).
- End goals of the remaining factions (`SCENARIO_CONCEPTS.md` section 10).
- [ai] `timeline-overview.drawio` still shows the old fallbacks -- update `timelines/timeline_overview_gen.py`.

**0.88.0 -- agreed (maintainer, 2026-11-27) -- built in 0.88.0, playtest open (`CHANGELOG.md`):**

- **Theramore conquerable**: Jaina's Sanctum is the seat; Theramore always serves the holder's team (never neutral).
  Held by an evil faction (Scourge, Legion, Fel Horde, Old Gods -- by the holder's FACTION, the teams are shared) ->
  Jaina becomes a dreadlord; held by any other -> Jaina again. As often as the Sanctum changes hands.
- **Kalecgos** (Dalaran): a transform hero -- human form (24p model, caster), dragon form (the blue dragon, scaled up,
  more attack damage, its own spells); same primary attribute. Quest "The Nexus": a hero CHANNELS at the Nexus for one
  minute (Jaina and Kalecgos, a little hanky-panky) -> Kalecgos and the blue dragonspawn join Dalaran.
- **Defias**: Edwin VanCleef for 1,500 gold pillaged ("The Stonemasons' Due").
- **Majordomo**: his kill gives **Benediction** (the Eye of Divinity's holy form; no Anathema for now).
- **Furbolgs**: when the Druids fall the satyr dungeon opens. **Xavius** (demi-hero, holds the **Claw of Ursoc**) and
  **Peroth'arn** (demi-hero). The Furbolgs are their own faction while Xavius lives OR while they hold the Claw. The
  first time they lose the Claw they join the conqueror's team (the Claw holder's), and Xavius and Peroth'arn turn
  Neutral Hostile (the satyrs no longer rule the furbolgs -- also stops a Claw hand-off to another hero). An
  announcement about the Claw when the Furbolgs join the game. The Furbolgs get a simple furbolg hero. No new bot hook.
- **Harpies**: the **broodmother** -- an immovable, conquerable caged unit (cage on wheels) next to the harpy huts.
  Later (not 0.88): corpses, digesting, eggs.
- **Ebon Blade**: four knights at level 10 channel -> **Acherus flies** (a transport, ultimate-like abilities). At 0 HP
  it burns, stops in the air for 60 s, invulnerable, and regenerates to full (the HP bar is the timer). Attacks never
  change its owner, only the team (the seat). Movement: the bots' current system, playtest first.
- **Kalimdor <-> Northrend**: harbour docks open (waygates) when one team holds 70 % of Kalimdor's CPs (from turn 30)
  or Lordaeron, Dalaran and Quel'thalas have all fallen.
- **Quilboar plague**: a Scourge option (Hourglass and Frozen Throne) -> Amnennar the Coldbringer and about 30 plague
  boars from Razorfen Downs heading to the Kalimdor HQs; infection only purge / dispel cures; infected units that die
  rise as Scourge zombies.
- **Smaller**: same research on the Hourglass and at the faction's own building where it fits; mini-bosses
  Mmmrrrggglll and a Blackmane chief with an artifact each; Draenei quests "The Broken One" (Nobundo) and "Survivors of
  Shattrath" (Maraad); Dragonmaw "Alexstrasza's Chains" (hold Grim Batol -> the chained reds hatch drakes every turn;
  Nek'rosh slain -> Alexstrasza breaks free and the dragons turn on the Dragonmaw).
- **Elite choices on the Hourglass**: WL's seven mirrored (same research), plus a unit lock for every faction with
  two elite-tier units -- a faction can only have ONE elite unit (the other is locked):
  Stormwind Champion / Conjurer, Lordaeron Paladin / Pegasus Knight, Dalaran Kirin Tor / Blue Dragon, Quel'thalas
  Farstrider / Blood Mage, Kul Tiras Musketeer / Sea Giant, Gilneas Greyguard / Worgen Shaman, Scarlet Templar /
  Bishop, Sunfury Bloodwarder / Warlock, Draenei Vindicator / Elekk Knight, Legion Dreadlord / Doom Lord, Fel Horde
  Executioner / Fel Death Knight, Illidari Royal Guard / Sea Witch, Black Empire Reaper / Herald, Skywall Elemental
  Lord / Tidal Lord, Nazjatar Immortal Guardian / Naga Sea Witch, Ashtongue Vindicator / Seer, Zandalari Boneseer /
  Direhorn, Forsaken Elder Banshee / Val'kyr, Kezan Tinker / War Zeppelin.

**Later:**
- ~~**Twilight**: its own player slot~~ (0.89.0, P20; the sub path plan of 0.87.1 is dropped). Next (discussed
  0.89.0): a Twilight dungeon with more CPs -- **Deepholm** proposed (Temple of Earth, the Stonecore, Twilight Precipice,
  the World Pillar; a void dungeon, terrain to find); **Deathwing harder** for a start faction -- proposal: not before
  turn 40, Grim Batol + Dragonmaw Port + both Highlands held, then Cho'gall channels 2 min at Grim Batol (announced,
  interruptible); **the Maw of Madness** (the tentacle at the Northern Highlands) -- proposals: a portal to Ny'alotha
  while an Old God holds the Northern Highlands, later the way into Deepholm, or sacrifices for N'Zoth's gifts.
  `[you]` decide.
- **Fel Horde**: the Consortium (rogue ethereal raiders) as its fallback -- later (maintainer).
- **Garithos** (D1 and his phases): too big for now.
- Extra abilities as elite choices (instead of locks); the broodmother's eggs; Anathema; Horde reunification.
- **Thandol / the Great Alliance**: human-only when built (bots never take them); a desperation opening for both
  sides. Not built.
- Not changed: the lumber crash (maintainer).

### 7. Shared mechanics (concepts sections 5, 7, 9, 11)

Great Alliance + Imperial Tax + tax vote (D2; replaces the retired Thandol Treaty draft), Defias, Legion's Tithe,
shared boxes (World Tree, Helm of Domination), defection (free slots P11 -> P14 -> P20), barks, sudden death "The End
Time". `[you]` the numbers (tax rate, sudden death X / N / Y).

## Win rule and game length

- `[you]` Goal: a game ends with ONE winner after **100-140 min**, and the winner has to capture everything. WL's rule
  today: a TEAM wins at 90 CPs (`ControlPointVictory.CpsVictory`). Needs the fork's own rule (an `[SL]` change).
- Caution: "everything" must mean every CAPTURABLE CP -- some CPs are invulnerable / quest-locked and can never fall.
  The last few CPs (islands) make long tails; a share (e.g. 95 %) or "all capitals + N %" ends cleaner. Bots must be
  able to reach them all (ferries).
- Measurement: the dump pins when a team first reaches 80 / 90 CPs; the actual winner / victory is not recorded yet
  (bot debug item "winner record", `../Ai/TODO.md`).

## Balance

- The bots' unit budget per CP (`../Ai/CONCEPTS.md` 8). `[you]` a different base per faction / team (good vs evil) --
  `AiApi.SetBudgetBase` from `Bot/SlBotSetup.cs`. `[you]` a CP-based food cap for humans too (a fork rule)?
- `[you]` **Old Gods balance -- re-check**: "too weak" was measured WITHOUT bot income. In 0.45.0 (bots with income)
  the Old Gods took all of Kalimdor; the Kalimdor team had 0 CPs at 45 min.
- `[you]` **Quel'thalas survives very long** (0.45.0: alive at 540 min with 12 CPs). Map side: is the area unusually
  fortified (towers, walls, the Elfgate)? Bot side: `../Ai/TODO.md`.
- `[you]` **Stormwind Castle CP needs a buff**; **South Alliance vs Naga / Fel Horde**: the South needs a buff (or
  Illidan / Fel Horde a debuff).

## Known issues

From the 0.60 playtest (maintainer):
- [you] ~~**Kul Tiras' fallback flickers**~~ (0.62: a lost HQ stays lost for its turning point) -- check in the
  playtest; if it still flickers, the cause is elsewhere (the HQ changing owners back and forth?).
- [ai] **Katherine's ultimate without its level** after the fallback to Kezan: probably every hero that stays with its
  faction through a fallback (the level requirement of hero skills -- `SwapFaction`, the faction's object levels?).
  Investigate with one hero, then fix for all fallbacks.
- [content] The Zandalari's endgame: Hakkar (the 24p Dark Trolls and Hakkar; `mapdata/WarcraftLegacies24p/`,
  `import_24p.py`) -- `[you]` decide.
- [map, next] Apply the same policy to additional named sites after checking their original rewards. Existing hidden
  buildings may stay hidden. Visible future buildings should resist attacks until claimed. Keep active creeps and
  public landmarks separate from reserved units. Verify the Sunfury garrison before and after Quel'Thalas takes
  its fallback. Review the Quel'Thalas split separately.
- [reference] The donor Twilight Citadel is 24p unit `h05U`, based on `hars`, model
  `war3mapImported\DarkShrineX2.mdl`. Its training list contains the Battlemaster and Cultist -- the production
  building of the planned Twilight sub path (Black Empire, "6. Faction by faction").
- [ai] **Detect invisible** is possible with the Forsaken's towers: needs a rework -- a research for it, and / or the
  same for other factions.
- [ai] (polish) **Kezan's trade ships** sometimes sail through islands -- why do they ignore land at times? (an order
  to a point on land? the "counts as arrived" fallback after 240 s? their pathing type?)
- [you] An item was cut off in the 0.60 playtest notes ("Need mechanic that ...") -- what was it?

- [ai] **Fallback messages are wrong**: when Kul Tiras loses its HQ, the game says the HQ was regained, although it
  was destroyed for good (v0.39.9 playtest; check in the 0.58 playtest -- `TimelineSystem.OnTriggerChanged`).
- [ai] **New Kezan units are invisible** (0.39 Kezan units -- 0.58 uses the 24p copies; check again): the unit count goes up during production, but nothing can be seen. Checked
  (after 0.52.0): every model path of the Kezan units points to an import that exists in the main map, and all 4046
  main-map imports are on the fork's LFS server. Left: `[you]` `git lfs ls-files | awk '$2=="-"'` in your clone lists
  files that are only pointers locally (`git lfs pull` fixes them before building); then the models' TEXTURE paths
  (the sandbox cannot download LFS files -- open the models in a viewer or the editor).
- [ai] **Field Stockpile (`h086`) uses a model only the 24p map has** (`Building_SpawningGroundsWestfall`, found by
  the content index): probably invisible in game. Copy the import from `mapdata/WarcraftLegacies24p/Imports/` or
  give it another model.
- [ai] Hint "When a player leaves, their units are refunded ..." (`Setup/HintSetup.cs`) is wrong since bots take over
  leavers (0.39.2).
- [ai] Lobby slot names in `Info.json` are still WL's old 24p layout -> the Shattered Legacies layout
  (`SCENARIO_CONCEPTS.md` section 3).
- [you] Remove the preplaced units of the unused slots P15, P18 at start? (WL only defeats empty slots that have a
  faction.)

## Map content, design and ideas (brainstorm)

The maintainer's own ideas go here too -- half-baked is fine. Timeline ideas go to `SCENARIO_CONCEPTS.md` (backlog,
section 12).
- [you] **Pseudo-flyers**: air units lose real flying and become pseudo-flyers, because bots handle flying pathing
  badly (orc air units fly over the ocean). Affects the bot too -- see `../Ai/TODO.md`.
- [you] **Book of Medivh redesign**: the Book gets its own spell; if the Scourge or the Legion gets it, it can trigger
  the summon on that spot.
- [you] Loading screen: a red "BOTIFIED" picture needs an image edit of `Imports/LoadingScreenTL/TR/BL/BR.blp`.
- [you] Islands still free for neutral roadblocks (Zandalar trolls -- 24p: God King, Gurubashi, Amani, Sandfury).
- [you] Chinese (0.75.0): build with `localization/publish.py`, test a mixed English / Chinese game (`LOCALIZATION.md`
  playtest list). The fork's own quests, fallbacks and bot messages are still English.
