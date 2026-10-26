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

> Note (added in 0.41): this game ran **0.39.10**, not 0.40 (dump headers, heartbeat). The 25:00 row is an old
> 0.39.9 dump from another game (units 5614 -> 2359, negative orders/min) -- ignore it. Valid: 10:00 and 20:00.
> The bots had far too much gold (set by accident) -- a stress test, not a baseline. 15 bots, 1 human.

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

### v0.41.0 -- 4 dumps, 5:00 .. 20:00 -- B smart, Veteran, 20 min

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4618 | 1496 | 2068 / 203 | - | North Alliance 32, South Alliance 33, Burning Legion 18, Outland 17, Old Gods 32, Kalimdor 30 |
| 20:00 | 3622 | 2198 | 1502 / 259 | 61 | North Alliance 23, South Alliance 39, Burning Legion 30, Outland 19, Old Gods 41, Kalimdor 31 |

LOAD at the checkpoints:
```
  10:00  combat 317/575 orders=0 | def-scan 7/189 | production peak 309 | wave-tick 544/3820 | last gate sweep 3611
  10:00  army smart orders: change=8727 idle-away=318 rest=5454 refill=465
  10:00  groups (food group/need of army @staging): P0 all 183; P1 50/50 of 122 @Deadwind Pass (15 gold/min); P2 50/50 of 87 @Terrordale (1 gold/min); P3 49/50 of 186 @Storm Peaks (15 gold/min); P4 49/50 of 79 @-; P6 50/50 of 161 @Blackrock Depths (20 gold/min); P7 50/50 of 145 @Alterac Mountains (20 gold/min); P8 49/50 of 120 @Kezan (15 gold/min); P9 50/50 of 130 @Brill (15 gold/min); P11 50/50 of 106 @Nazjatar (10 gold/min); P12 39/50 of 118 @Feralas (10 gold/min); P15 all 57; P16 50/50 of 74 @Gadgetzan (20 gold/min); P18 49/50 of 128 @Windshear Crossing (10 gold/min); P22 50/50 of 118 @Stranglethorn (15 gold/min); P23 44/50 of 115 @Grizzly Hills (20 gold/min);
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3774 over=41 would-defer=40 deferred=0
  10:00  orders total=14964 near(<2k)=1365 mid(2-8k)=9586 far(>8k)=4013 idle-at-target(<300)=8
  20:00  combat 399/874 orders=197 | def-scan 80/372 | production peak 367 | wave-tick 3367/8263 | last gate sweep 4069
  20:00  army smart orders: change=21543 idle-away=993 rest=12886 refill=1518
  20:00  groups (food group/need of army @staging): P0 all 20; P1 120/140 of 125 @-; P2 8/276 of 8 @Terrordale (1 gold/min); P3 197/160 of 197 @Hearthglen (15 gold/min); P4 50/50 of 109 @Dun Modr (10 gold/min); P6 13/50 of 13 @Blackrock Depths (20 gold/min); P7 all 5; P8 50/50 of 69 @Ratchet (10 gold/min); P9 all 10; P11 50/50 of 171 @Felwood (10 gold/min); P12 32/50 of 41 @Feralas (10 gold/min); P15 15/50 of 15 @-; P16 50/50 of 91 @Durotar (10 gold/min); P18 37/50 of 61 @Desolace (10 gold/min); P22 117/139 of 131 @-; P23 50/50 of 213 @Alterac Mountains (20 gold/min);
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4412 over=256 would-defer=294 deferred=0
  20:00  orders total=36940 near(<2k)=4386 mid(2-8k)=22639 far(>8k)=9915 idle-at-target(<300)=36
```

Top tasks by total ops at 20:00 (of 35):
```
task combat every=0.500s runs=2400 last=596 max=1078 sum=529243
task production every=1.000s runs=1199 last=144 max=367 sum=206869
task hero visits every=15.000s runs=79 last=3213 max=4010 sum=82726
task gate sweep every=15.000s runs=79 last=0 max=4198 sum=14732
task ragnaros every=15.000s runs=79 last=117 max=319 sum=9342
task hero care every=15.000s runs=79 last=37 max=46 sum=2572
task starter kit every=15.000s runs=79 last=0 max=1493 sum=1716
task kul tiras repair every=15.000s runs=79 last=0 max=1369 sum=1369
task kill targets every=15.000s runs=79 last=0 max=15 sum=17
task demon portal every=15.000s runs=79 last=0 max=2 sum=2
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 20:00: P1 Stormwind atk a129 f124; P2 Quel'thalas atk a14 f10; P3 Scourge atk a201 f161; P4 Ironforge atk a111 f112; P6 Fel Horde atk a21 f16; P7 Gilneas def a12 f8; P8 Skywall atk a72 f64; P9 Lordaeron atk a15 f7; P11 Druids atk a176 f164; P12 Black Empire atk a60 f31; P15 Illidan atk a18 f15; P16 Ahn'qiraj atk a156 f90; P18 Sentinels atk a97 f60; P22 Kul'tiras atk a150 f126; P23 Legion atk a259 f173; P0 Frostwolf def a11 f15

Heartbeat: WLAI heartbeat build=0.41.0 t=22m09s units-on-map=3014 orders/min=1029 orders=smart
  task 22:01 combat
  task 22:02 production
  task 22:02 named gates
  ...
