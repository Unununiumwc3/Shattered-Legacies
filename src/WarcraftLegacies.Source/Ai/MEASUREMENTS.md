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

### v0.39.10 -- 3 dumps, 10:00 .. 20:00 -- A idle+route off, Veteran

> Note (0.42.1): NOT a new game -- the same numbers as the first 0.39.10 entry (the too-much-gold game; probably the
> old dumps folder again). Game A is missing; B (0.41) vs C (0.42) is the valid comparison.

Skipped (other build or other game): WLAI_dump_25m00s.txt (v0.39.9, 25:00)

Bots: 16, orders mode: idle.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4258 | 1618 | 1623 / 182 | - | North Alliance 30, South Alliance 39, Burning Legion 17, Outland 18, Old Gods 31, Kalimdor 32 |
| 20:00 | 5614 | 3139 | 3355 / 269 | 61 | North Alliance 29, South Alliance 37, Burning Legion 24, Outland 21, Old Gods 40, Kalimdor 27 |

LOAD at the checkpoints:
```
  10:00  combat 153/841 orders=0 | def-scan 0/241 | production peak 293 | wave-tick 528/3680 | last gate sweep 3458
  20:00  combat 285/1264 orders=0 | def-scan 0/473 | production peak 427 | wave-tick 5595/6892 | last gate sweep 3367
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 20:00: P0 Frostwolf def a162 f121; P1 Stormwind def a231 f200; P2 Quel'thalas atk a202 f200; P3 Scourge atk a269 f250; P4 Ironforge atk a199 f200; P6 Fel Horde atk a261 f241; P7 Dalaran atk a251 f200; P8 Skywall def a233 f200; P9 Lordaeron atk a130 f131; P11 Druids def a227 f200; P12 Black Empire atk a222 f200; P15 Illidan atk a205 f221; P16 Ahn'qiraj def a262 f200; P18 The Exodar atk a81 f159; P23 Legion atk a213 f175; P22 Kul'tiras atk a207 f200

Heartbeat: WLAI heartbeat build=0.39.10 t=22m49s units-on-map=5447 orders/min=4353 orders=idle
  task 22:45 hero care
  task 22:45 hero skills
  task 22:45 standings
  ...

### v0.41.0 -- 4 dumps, 5:00 .. 20:00 -- B route off, Veteran

> Note (0.42.1): the same game as "B smart" above (identical numbers) -- appended twice.

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

### v0.42.0 -- 4 dumps, 5:00 .. 20:00 -- C default, Veteran

> Note (0.42.1): vs. B at 20 min -- orders/min 2327 vs 2198, of them "advance" (waypoints) 11483 in 20 min; slots over
> budget 2114 vs 256 (route planning on every service, counted in combat). Fixed in 0.42.1 (committed routes).

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4475 | 1986 | 1846 / 174 | - | North Alliance 31, South Alliance 37, Burning Legion 14, Outland 26, Old Gods 29, Kalimdor 27 |
| 20:00 | 3398 | 2327 | 1243 / 234 | 61 | North Alliance 33, South Alliance 28, Burning Legion 17, Outland 29, Old Gods 29, Kalimdor 37 |

LOAD at the checkpoints:
```
  10:00  combat 162/421 orders=0 | def-scan 0/276 | production peak 294 | wave-tick 434/3545 | last gate sweep 3514
  10:00  army smart orders: change=8888 idle-away=162 rest=3526 refill=453 advance=6833
  10:00  groups (food group/need of army @staging): P0 50/50 of 88 @Ranazjar Isle (10 gold/min); P1 45/50 of 121 @Burning Steppes (20 gold/min); P2 50/50 of 81 @Seradane (10 gold/min); P3 50/50 of 170 @Icecrown Citadel (100 gold/min); P4 50/50 of 77 @Loch Modan (10 gold/min); P6 50/50 of 159 @-; P7 38/50 of 109 @Deathknell (10 gold/min); P8 50/50 of 119 @The Vortex Pinnacle (100 gold/min); P9 50/50 of 109 @Tirisfal Glades (15 gold/min); P11 50/50 of 134 @Sulfuron Spire (10 gold/min); P12 9/50 of 9 @Maw of Gor'ma (20 gold/min); P15 50/50 of 85 @-; P16 50/50 of 91 @Spinebark Grove (10 gold/min); P18 50/50 of 112 @Exodar Regalis (15 gold/min); P22 50/50 of 128 @Northern Badlands (10 gold/min); P23 50/50 of 103 @Storm Peaks (15 gold/min);
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3645 over=1055 would-defer=1399 deferred=0
  10:00  orders total=19862 near(<2k)=2276 mid(2-8k)=15032 far(>8k)=2554 idle-at-target(<300)=18
  20:00  combat 199/620 orders=50 | def-scan 0/317 | production peak 367 | wave-tick 2977/6904 | last gate sweep 3459
  20:00  army smart orders: change=21060 idle-away=685 rest=9194 refill=706 advance=11483
  20:00  groups (food group/need of army @staging): P0 46/50 of 46 @-; P1 all 3; P2 40/67 of 48 @Durnholde (15 gold/min); P3 62/62 of 222 @Terrordale (1 gold/min); P4 50/50 of 113 @Dun Modr (10 gold/min); P6 47/50 of 182 @Duskwood (10 gold/min); P7 50/50 of 154 @Howling Fjords (20 gold/min); P8 21/50 of 21 @Dire Maul (15 gold/min); P9 all 10; P11 50/50 of 73 @-; P12 49/50 of 78 @Gadgetzan (20 gold/min); P15 14/50 of 14 @Booty Bay (25 gold/min); P16 36/50 of 43 @Alcaz Isle (10 gold/min); P18 19/50 of 19 @Exodar Regalis (15 gold/min); P22 36/54 of 49 @Zul'Gurub (25 gold/min); P23 26/50 of 26 @Icecrown Glacier (20 gold/min);
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4205 over=2114 would-defer=2794 deferred=0
  20:00  orders total=43128 near(<2k)=5817 mid(2-8k)=29785 far(>8k)=7526 idle-at-target(<300)=53
```

Top tasks by total ops at 20:00 (of 38):
```
task combat every=0.500s runs=2400 last=914 max=1420 sum=1799940
task production every=1.000s runs=1199 last=136 max=367 sum=187456
task route areas every=15.000s runs=79 last=941 max=1311 sum=78448
task hero visits every=15.000s runs=79 last=2632 max=3186 sum=61264
task route portals every=30.000s runs=39 last=814 max=3257 sum=59753
task gate sweep every=15.000s runs=79 last=0 max=3940 sum=13751
task ragnaros every=15.000s runs=79 last=307 max=310 sum=11661
task hero care every=15.000s runs=79 last=38 max=45 sum=2570
task starter kit every=15.000s runs=79 last=0 max=1475 sum=1694
task kul tiras repair every=15.000s runs=79 last=0 max=1421 sum=1421
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 20:00: P0 Frostwolf atk a54 f53; P1 Stormwind atk a6 f6; P2 Quel'thalas atk a50 f49; P3 Scourge atk a234 f214; P4 Ironforge atk a116 f116; P6 Fel Horde atk a202 f187; P7 Dalaran atk a156 f151; P8 Skywall atk a26 f25; P9 Lordaeron def a17 f10; P12 Black Empire atk a89 f83; P15 Illidan atk a23 f19; P16 Ahn'qiraj atk a62 f47; P18 Sentinels atk a24 f23; P22 Kul'tiras atk a70 f26; P23 Legion atk a26 f24; P11 Druids atk a88 f82

Heartbeat: WLAI heartbeat build=0.42.0 t=21m09s units-on-map=2998 orders/min=1840 orders=smart
  task 21:02 timed quests
  task 21:02 legion events
  task 21:02 old gods rift
  ...

### v0.43.0 -- 5 dumps, 5:00 .. 25:00 -- 0.43.0 default, Veteran (old scale: +25 bonus), played by the maintainer

> Note: whole teams chose the same front (allies' nodes counted as territory) and marched as one ball; "advance"
> 23822 in 25 min (~950/min) = units stopping at every node; 1778 plans (a re-plan whenever the group left its route).
> Maintainer: orders took ~2 s to start at 4000+ units (Veteran too strong). All fixed / changed in 0.43.1.

Bots: 15, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4633 | 1746 | 1933 / 242 | - | North Alliance 29, South Alliance 26, Burning Legion 13, Outland 19, Old Gods 27, Kalimdor 27 |
| 20:00 | 4126 | 2425 | 1937 / 260 | - | North Alliance 32, South Alliance 31, Burning Legion 18, Outland 20, Old Gods 40, Kalimdor 30 |
| 25:00 | 4044 | 2060 | 1651 / 223 | - | North Alliance 29, South Alliance 34, Burning Legion 23, Outland 20, Old Gods 43, Kalimdor 30 |

LOAD at the checkpoints:
```
  10:00  combat 199/553 orders=0 | def-scan 19/131 | production peak 287 | wave-tick 364/3549 | last gate sweep 3521
  10:00  army smart orders: change=5384 idle-away=161 rest=2888 refill=493 advance=8539
  10:00  groups (food group/need of army @staging): P0 50/50 of 119 @-; P1 50/50 of 114 @Searing Gorge (10 gold/min); P2 50/50 of 74 @Tranquillien (15 gold/min); P3 50/50 of 155 @Grizzly Hills (20 gold/min); P4 50/50 of 77 @Dun Modr (10 gold/min); P6 57/57 of 107 @Blackrock Depths (20 gold/min); P7 50/50 of 134 @Dalaran (30 gold/min); P8 50/50 of 106 @Shimmering Flats (10 gold/min); P9 50/50 of 116 @Brill (15 gold/min); P11 50/50 of 114 @-; P12 50/50 of 126 @Thousand Needles (10 gold/min); P15 57/57 of 234 @-; P16 50/50 of 94 @Zul'Farrak (15 gold/min); P18 50/50 of 91 @Exodar Regalis (15 gold/min); P22 50/50 of 99 @Highbank (10 gold/min); P23 50/50 of 103 @Storm Peaks (15 gold/min);
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3664 over=446 would-defer=582 deferred=0
  10:00  orders total=17465 near(<2k)=2077 mid(2-8k)=13512 far(>8k)=1876 idle-at-target(<300)=21
  20:00  combat 201/632 orders=0 | def-scan 0/337 | production peak 388 | wave-tick 3720/7480 | last gate sweep 3840
  20:00  army smart orders: change=13280 idle-away=550 rest=8323 refill=698 advance=18860
  20:00  groups (food group/need of army @staging): P0 12/50 of 12 @Thunderbluff (20 gold/min); P1 all 94; P2 26/112 of 26 @Sunsail Anchorage (1 gold/min); P3 50/50 of 199 @En'Kilah (10 gold/min); P4 50/50 of 89 @-; P6 14/50 of 14 @-; P7 96/96 of 153 @Silverpine Forest (15 gold/min); P8 all 148; P9 24/112 of 24 @Venture Bay (10 gold/min); P11 50/50 of 177 @Northern Ashenvale (10 gold/min); P12 45/50 of 197 @Desolace (10 gold/min); P15 49/50 of 245 @Nethergarde Keep (15 gold/min); P16 50/50 of 118 @Scarab Wall (10 gold/min); P18 49/50 of 148 @Astranaar (15 gold/min); P22 50/50 of 120 @Boralus (25 gold/min); P23 49/50 of 111 @Sholazar Basin (15 gold/min);
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4335 over=941 would-defer=1220 deferred=0
  20:00  orders total=41711 near(<2k)=4704 mid(2-8k)=28589 far(>8k)=8418 idle-at-target(<300)=77
  25:00  combat 304/632 orders=0 | def-scan 0/337 | production peak 388 | wave-tick 3320/7631 | last gate sweep 3814
  25:00  army smart orders: change=15258 idle-away=821 rest=11238 refill=870 advance=23822
  25:00  groups (food group/need of army @staging): P0 all 7; P1 all 16; P2 13/84 of 13 @Seradane (10 gold/min); P3 25/50 of 163 @Hearthglen (15 gold/min); P4 50/50 of 84 @Dragonmaw Gates (10 gold/min); P6 25/50 of 34 @-; P7 50/50 of 130 @Arathi Highlands (15 gold/min); P8 50/50 of 158 @Southern Barrens (10 gold/min); P9 all 1; P11 50/50 of 259 @-; P12 50/50 of 185 @Spinebark Grove (10 gold/min); P15 74/80 of 243 @Nethergarde Keep (15 gold/min); P16 50/50 of 180 @Mulgore (10 gold/min); P18 50/50 of 98 @Exodar Regalis (15 gold/min); P22 50/50 of 167 @Southern Highlands (10 gold/min); P23 121/104 of 153 @Zul'Drak (20 gold/min);
  25:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=6000 max-slot=4335 over=1166 would-defer=1505 deferred=0
  25:00  orders total=52009 near(<2k)=5853 mid(2-8k)=34431 far(>8k)=11725 idle-at-target(<300)=80
```

Top tasks by total ops at 25:00 (of 38):
```
task combat every=0.500s runs=3000 last=304 max=1992 sum=1781562
task production every=1.000s runs=1499 last=298 max=388 sum=259551
task hero visits every=15.000s runs=99 last=3143 max=3594 sum=140972
task route areas every=15.000s runs=99 last=1078 max=1641 sum=107645
task route portals every=30.000s runs=49 last=751 max=3233 sum=69202
task gate sweep every=15.000s runs=99 last=0 max=4206 sum=18162
task ragnaros every=15.000s runs=99 last=138 max=243 sum=11798
task hero care every=15.000s runs=99 last=39 max=44 sum=3315
task starter kit every=15.000s runs=99 last=0 max=1418 sum=1648
task kul tiras repair every=15.000s runs=99 last=0 max=1319 sum=1319
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 25:00: P0 Frostwolf def a7 f11; P1 Stormwind def a20 f16; P2 Quel'thalas atk a22 f14; P3 Scourge atk a153 f123; P4 Ironforge atk a87 f87; P6 Fel Horde atk a35 f26; P7 Gilneas atk a130 f119; P8 Skywall atk a161 f161; P9 Lordaeron def a8 f2; P11 Druids atk a223 f200; P12 Black Empire atk a207 f185; P16 Ahn'qiraj atk a208 f185; P18 The Exodar atk a63 f89; P22 Kul'tiras atk a174 f130; P23 Legion atk a153 f145

Heartbeat: WLAI heartbeat build=0.43.0 t=25m29s units-on-map=3625 orders/min=1932 orders=smart
  task 25:22 production
  task 25:22 starter kit
  task 25:22 combat
  ...

### v0.43.1 -- 4 dumps, 5:00 .. 20:00 -- 0.43.1 default, Veteran (new scale: 0 bonus)

> Note (0.43.2): the 25:00 dump was the old 0.43.0 game (same file name) -- skipped. Armies of 2-50 food at 20:00 and
> gold ~0 for every bot: the new Veteran (0 bonus) is too weak. Main movement fault: attack <-> defence flapping (Rule 0
> took any damaged own CP on the map) -- Scourge between Northrend and Lordaeron, Druids across Kalimdor. Fixed in 0.43.2.

Skipped (other build or other game): WLAI_dump_25m00s.txt (v0.43.0, 25:00)

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3711 | 1008 | 1144 / 181 | - | North Alliance 31, South Alliance 32, Burning Legion 15, Outland 19, Old Gods 32, Kalimdor 34 |
| 20:00 | 2769 | 1087 | 789 / 193 | 82 | North Alliance 32, South Alliance 25, Burning Legion 9, Outland 33, Old Gods 44, Kalimdor 28, Scourge Pact 14 |

LOAD at the checkpoints:
```
  10:00  combat 254/297 orders=49 | def-scan 91/102 | production peak 227 | wave-tick 443/3310 | last gate sweep 3127
  10:00  army smart orders: change=5768 idle-away=278 rest=1538 refill=485 advance=2011
  10:00  groups (food group/need of army @staging): P0 50/50 of 79 @Ratchet (10 gold/min); P1 51/50 of 51 @Nethergarde Keep (15 gold/min); P2 33/50 of 33 @Terrordale (1 gold/min); P3 50/50 of 130 @Storm Peaks (15 gold/min); P4 45/50 of 48 @Northern Badlands (10 gold/min); P6 50/50 of 108 @-; P7 50/50 of 63 @Deathknell (10 gold/min); P8 50/50 of 86 @Thousand Needles (10 gold/min); P9 53/50 of 53 @Brill (15 gold/min); P11 55/50 of 55 @The Abyss (10 gold/min); P12 29/50 of 29 @Feathermoon (20 gold/min); P15 50/50 of 172 @-; P16 30/50 of 30 @Dire Maul (15 gold/min); P18 12/50 of 12 @Windshear Crossing (10 gold/min); P22 50/50 of 81 @Southern Highlands (10 gold/min); P23 28/50 of 28 @Venture Bay (10 gold/min);
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3254 over=364 would-defer=482 deferred=0
  10:00  orders total=10080 near(<2k)=668 mid(2-8k)=7506 far(>8k)=1906 idle-at-target(<300)=19
  20:00  combat 122/492 orders=0 | def-scan 46/264 | production peak 322 | wave-tick 2549/5919 | last gate sweep 2912
  20:00  army smart orders: change=11184 idle-away=991 rest=3797 refill=655 advance=4320
  20:00  groups (food group/need of army @staging): P0 2/50 of 2 @Jaina's Sanctum (10 gold/min); P1 5/50 of 5 @Stromgarde (25 gold/min); P2 3/50 of 3 @Sunsail Anchorage (1 gold/min); P3 33/50 of 74 @Grizzly Hills (20 gold/min); P4 all 41; P6 26/50 of 106 @-; P7 31/50 of 37 @-; P8 20/50 of 20 @Southern Barrens (10 gold/min); P9 17/110 of 17 @Stratholme (30 gold/min); P11 all 2; P12 6/50 of 6 @Ranazjar Isle (10 gold/min); P15 50/50 of 189 @Booty Bay (25 gold/min); P16 10/50 of 10 @Windshear Crossing (10 gold/min); P18 4/50 of 4 @Exodar Regalis (15 gold/min); P23 50/50 of 172 @Shrine to Malorne (10 gold/min);
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=3282 over=772 would-defer=1004 deferred=0
  20:00  orders total=20947 near(<2k)=1533 mid(2-8k)=12689 far(>8k)=6725 idle-at-target(<300)=30
```

Top tasks by total ops at 20:00 (of 38):
```
task combat every=0.500s runs=2400 last=122 max=1932 sum=1149623
task production every=1.000s runs=1199 last=220 max=322 sum=150809
task route areas every=15.000s runs=79 last=800 max=1163 sum=67358
task hero visits every=15.000s runs=79 last=2290 max=2843 sum=58666
task route portals every=30.000s runs=39 last=655 max=3235 sum=56659
task gate sweep every=15.000s runs=79 last=0 max=3233 sum=12044
task ragnaros every=15.000s runs=79 last=220 max=229 sum=9213
task hero care every=15.000s runs=79 last=39 max=44 sum=2512
task starter kit every=15.000s runs=79 last=0 max=1200 sum=1430
task kul tiras repair every=15.000s runs=79 last=0 max=1336 sum=1336
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 20:00: P0 Frostwolf atk a4 f6; P1 Stormwind atk a13 f4; P2 Quel'thalas atk a3 f1; P3 Scourge atk a78 f78; P4 Ironforge def a45 f39; P6 Fel Horde atk a110 f107; P7 Dalaran atk a37 f32; P8 Skywall atk a27 f21; P9 Lordaeron def a23 f14; P11 Druids def a9 f1; P12 Black Empire atk a15 f9; P16 Ahn'qiraj atk a33 f16; P18 Sentinels atk a4 f3; P22 Kul'tiras def a3 f4; P23 Legion atk a192 f145; P15 Sunfury atk a193 f196

Heartbeat: WLAI heartbeat build=0.43.1 t=20m39s units-on-map=2379 orders/min=1285 orders=smart
  task 20:32 combat
  task 20:32 timed quests
  task 20:32 legion events
  ...
