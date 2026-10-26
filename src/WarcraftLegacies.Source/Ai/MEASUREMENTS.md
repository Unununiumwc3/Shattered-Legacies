# Measurements

Load and game-state numbers per version, so they survive between chats. Every refactor phase is compared with the
game before it (TODO.md, "Refactor"): units on the map, orders per minute, LOAD, at 10/20/30/45 min.

How to add a game (Ubuntu, repo root):

1. In the game: `-autodump` at the start (single human only). Play at least 45 min.
2. Copy `Documents\Warcraft III\CustomMapData\WLAI_dump_*.txt` (and `WLAI_heartbeat.txt`) of THIS game into an
   empty folder, e.g. `~/Downloads/game1/` -- delete the old dumps in CustomMapData first, the names repeat.
3. `python3 tools/ai/dump_summary.py ~/Downloads/game1/WLAI_*.txt --append "<players, strength vote, notes>"`
   appends a block below. Commit this file with the next drop (or on its own: `git commit -m "measurements"`).

Notes: "real s per game min" needs the file times of the copy (60 = real time; higher = the engine falls behind).
The LOAD format changes with 0.40 (scheduler). Compare like with like: same players, same vote, same length.

## Games

(none yet -- the next game is the baseline. 0.40 adds per-task counters to LOAD; its game is the baseline for 0.41.)

### v0.39.10/0.39.9 -- 4 dumps, 10:00 .. 25:00 -- 1 human, N bots, Veteran

**Warning: dumps of different builds mixed.**

Bots: 15, orders mode: idle.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4258 | 1618 | 1623 / 182 | - | North Alliance 30, South Alliance 39, Burning Legion 17, Outland 18, Old Gods 31, Kalimdor 32 |
| 20:00 | 5614 | 3139 | 3355 / 269 | 61 | North Alliance 29, South Alliance 37, Burning Legion 24, Outland 21, Old Gods 40, Kalimdor 27 |
| 25:00 | 2359 | -3412 | 592 / 165 | - | North Alliance 9, South Alliance 9, Burning Legion 45, Outland 49, Old Gods 23, Kalimdor 46, Kezan Enterprise Pact 0 |

LOAD at the checkpoints:
```
  10:00  combat 153/841 orders=0 | def-scan 0/241 | production peak 293 | wave-tick 528/3680 | last gate sweep 3458
  20:00  combat 285/1264 orders=0 | def-scan 0/473 | production peak 427 | wave-tick 5595/6892 | last gate sweep 3367
  25:00  combat 145/774 orders=0 | def-scan 0/360 | production peak 312 | wave-tick 2208/5030 | last gate sweep 2154
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 25:00: P0 Frostwolf def a83 f73; P1 Stormwind def a3 f0; P2 Quel'thalas atk a2 f1; P3 Scourge atk a139 f94; P4 Ironforge def a3 f0; P6 Fel Horde atk a165 f164; P7 Gilneas def a4 f0; P8 Skywall atk a8 f3; P9 Lordaeron def a4 f0; P11 Druids atk a6 f4; P12 Black Empire atk a12 f6; P15 Illidan atk a46 f46; P16 Ahn'qiraj def a22 f9; P18 The Exodar atk a19 f27; P23 Legion atk a76 f54

Heartbeat: WLAI heartbeat build=0.39.10 t=22m49s units-on-map=5447 orders/min=4353 orders=idle
  task 22:45 hero care
  task 22:45 hero skills
  task 22:45 standings
  ...
