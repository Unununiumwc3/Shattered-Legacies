# 0.69.0 — mainland Horde and restored Kezan

This handoff supersedes the western pilot's island opening and disabled Kezan notes.
The original 0.68 Twillight playtest passed at `15484d14de1ac0db9ed07ec30839473abe2f44a3`.
The western pilot was subsequently playtested by the maintainer. **0.69 is a new,
unplaytested follow-up**, not a declaration that its gameplay has passed.
Apply after the western 5.0.4 integration, retaining subsequent unrelated changes.

## Implemented

- Orcish Horde keeps the imported faction roster and quests, but starts in Durotar.
  The island survival/evacuation opening is no longer registered. Existing start
  buildings and five workers have been moved onto checked mainland pathing.
- `Build Orgrimmar` (`R05O`) is available in Great Hall, Stronghold and Fortress.
  Completing it resolves `To Tame a Land`, reveals/transfers Orgrimmar including
  its capital, and unlocks Grom. The three-construction objective and turn-15 wait
  are removed. Existing bot event-research support includes all three halls.
- Orgrimmar's hidden `h00L` gates are removed when the city is revealed; removed
  gates are excluded from the quest's rescue list. One Durotar CP remains.
- `Break the Blood Pact`: after founding Orgrimmar, bring Grom within 900 range
  of the Fountain of Blood to reveal/unprotect the existing Mannoroth. An allied
  kill with living Grom within 1200 completes the quest and renames the fountain.
  Mannoroth's death makes Southern Ashenvale capturable, even if another team
  kills him; an enemy kill fails the Horde quest. Grom is not gated by this fight.
  The old drink-blood dialogue trigger is removed; Grom's repentance dialogue
  follows successful quest completion. No combat-stat reward is added.
- Restored original Kezan terrain, pathing, units and doodads inside
  `(-5120,-10752)` to `(1024,-4864)` from the pre-western fork. Terrain outside
  that rectangle is retained. Restored Kul'tiras → Kezan Enterprise after Boralus
  falls and the Nazmir–Kezan ferry link. Original imported asset files are retained;
  this patch changes no imported assets and requires no asset installer.
- Tauren alone retains Skywall. On claiming it, the player joins Thunder Bluff's
  recorded conqueror's current team; a neutral kill without a conqueror retains
  the previous team. This is a one-time arrival decision, not perpetual following.
  The protected Lost City starter camp and reserved Vortex base transfer on arrival.
  **Lost City and Vortex CP ownership is forcibly transferred**, including from
  another player, per the maintainer's explicit Lost City safety requirement.
  The Vortex quest still unlocks Al'Akir; it no longer promises the initial base
  and does not reclaim player-owned buildings after a later conquest.
- Bots can register a moving escort objective. Tauren's active caravan supplies
  that target; the army accompanies the surviving kodo rather than being ordered
  to a fixed Thunder Bluff coordinate. Completion releases normal planning.
- Worker recovery checks all owned production buildings, including capitals,
  rather than just ordinary halls. Hall upgrades wait until the worker target is
  met; ownership changes request a fresh construction pass and home location.
- Flying creep/quest targets require at least three air-capable army units and
  six food of air-capable attackers. This is a conservative admission check,
  not a full damage simulation; spell-only anti-air is not counted.
- Flight-path routing rejects inactive/hidden gates. Private portal ownership is
  read live. Argus quest completion explicitly activates both Legion teleporters;
  bot edges use their actual destinations rather than separate fixed coordinates.
- Restored `R05A` to the retained Druid `The War of the Shifting Sands` quest,
  resolving its collision with the donor's `Wrath of the Wild` completion marker.
- Regenerated bot construction/route/lane data and the Skywall Hourglass tooltip.
  Map build version is `v0.69.0`.

## Configuration and pairing

Edit `Timelines/TimelineCatalog.cs`, not a copy of the destination faction:

| Source | Destination / execution | Availability |
| --- | --- | --- |
| Tauren Tribes | `SkywallFallback.Path` | Thunder Bluff loss; exclusive Skywall claim |
| Orcish Horde | `TwilightHammerFallback.Path` | Orgrimmar loss; shared exclusive Twillight claim |
| Fel Horde | `TwilightHammerFallback.Path` | Kil'sorrow Fortress loss; same shared claim |
| Kul'tiras | existing Kezan Enterprise timeline effects | Boralus loss |

For Skywall, source, source-capital trigger and tooltip are in the catalog;
conqueror tracking currently watches Thunder Bluff in `SkywallFallback.Prepare`.
Reassigning this path therefore also requires changing that tracked capital.
Update `tools/shattered-legacies/timelines/gen_hourglass.py` and regenerate the
Hourglass when changing visible offer text. Kezan still uses its existing effect
list; converting it to `ExclusivePath` is a separate refactor, not part of 0.69.

## Playtest checklist

1. **Opening and Orgrimmar:** one human Orc and one bot Orc test. Start on mainland
   with usable workers/hall; no island assault or evacuation. Research at T1,
   then separately T2/T3. City/HQ become yours, both gates disappear, Grom trains,
   only one Durotar CP exists. Destroy the owned HQ and check the Twillight offer.
2. **Tauren:** bot escorts the moving caravan, does not divert the full army into
   the flying dragons, gains Thunder Bluff, trains workers and builds promptly.
   Also test caravan loss and late recovery; escort orders must cease afterward.
3. **Skywall:** destroy Thunder Bluff with an enemy-team player. Claim Skywall
   and check scoreboard, alliances, vision and bot orders against that conqueror's
   team. Pre-capture Lost City with someone else: Skywall must receive it anyway.
   Verify Vortex buildings, workers and CP arrive immediately; no spawn camping
   from a missing starter base. Repeat with a neutral killer. Complete/fail the
   Vortex quest later and ensure no player-owned base is stolen back.
4. **Kezan:** visit the restored island/coast, test its ferry in both directions,
   then lose Boralus and take Kezan. Check Katherine, workers, roster, research,
   bot production and arrival. No Orc start or murloc assault may remain there.
5. **Mannoroth:** before Orgrimmar completion he stays hidden/protected. Grom's
   visit reveals him afterward; allied victory with nearby Grom completes the
   quest and frees Southern Ashenvale. Test enemy last hit and distant Grom too.
6. **Travel/regressions:** both Thunder Bluff–Orgrimmar flight directions; Legion
   captures Eredath and exits Argus after unlock. Check at turns 5/10/15, saving
   `-dump` output if it stalls. Test both Orc/Fel Twillight claim orders and the
   retained Old Gods openings. Check startup for both reported warnings.

## Remaining work

- **Argus is not proven fixed in-game.** The portal activation/destination issues
  are addressed, but the reported later army stall and missed Eredath capture
  still need a runtime retest and a fresh dump if reproducible.
- The shore warning at `(-2502,-7759)` was on the overwritten Kezan footprint;
  the restored pathing is walkable there. The screenshot does not establish that
  a separate region is missing. Retest startup and inspect any remaining warning.
- Validate caravan escort priorities and worker ramp-up in-game. No forced
  destination attack-move, instant city grant, or new army-strength buff is used.
- Mannoroth has a normal human quest; there is no special new bot hero-visit
  mission for it yet. Existing autoquest behavior needs observation.
- Horde balance/buffs follow correctness and remaining upstream alignment.
- This is still a selective 5.0.4 integration, not the complete 4.82→5.0.4 update.
  Dark Troll/Hakkar port and the Quel'Thalas split remain separate roadmap items.
