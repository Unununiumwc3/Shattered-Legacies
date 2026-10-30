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

### v0.43.2 -- 6 dumps, 5:00 .. 25:00 -- 0.43.2 default, Commander (+13), maintainer -- no Thandol problems seen; orcs (Warsong) expanded along the Broken Isles

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3653 | 966 | 1006 / 132 | - | North Alliance 31, South Alliance 30, Burning Legion 15, Outland 20, Old Gods 33, Kalimdor 28 |
| 20:00 | 2635 | 1064 | 512 / 121 | - | North Alliance 34, South Alliance 24, Burning Legion 17, Outland 33, Old Gods 41, Kalimdor 31 |
| 25:00 | 2519 | 802 | 523 / 144 | - | North Alliance 30, South Alliance 21, Burning Legion 22, Outland 39, Old Gods 36, Kalimdor 37 |

LOAD at the checkpoints:
```
  10:00  combat 218/288 orders=1 | def-scan 0/179 | production peak 217 | wave-tick 456/3281 | last gate sweep 3118
  10:00  army smart orders: change=4752 idle-away=224 rest=806 refill=498 advance=3375
  10:00  groups (food group/need of army @staging): P0 83/77 of 83 @Ratchet (10 gold/min); P1 36/50 of 36 @Deadwind Pass (15 gold/min); P2 38/50 of 38 @Terrordale (1 gold/min); P3 50/50 of 120 @Storm Peaks (15 gold/min); P4 42/50 of 44 @Northern Badlands (10 gold/min); P6 49/50 of 122 @-; P7 50/50 of 70 @Deathknell (10 gold/min); P8 53/53 of 76 @Shimmering Flats (10 gold/min); P9 44/50 of 44 @Brill (15 gold/min); P11 43/50 of 114 @Sulfuron Spire (10 gold/min); P12 48/50 of 69 @Feralas (10 gold/min); P15 1/50 of 1 @Suramar (10 gold/min); P16 43/60 of 48 @Thunderbluff (20 gold/min); P18 all 7; P22 50/50 of 68 @Booty Bay (25 gold/min); P23 25/50 of 25 @-;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3275 over=364 would-defer=481 deferred=0
  10:00  orders total=9655 near(<2k)=1089 mid(2-8k)=7527 far(>8k)=1039 idle-at-target(<300)=19
  20:00  combat 239/596 orders=2 | def-scan 0/331 | production peak 312 | wave-tick 2213/5257 | last gate sweep 2676
  20:00  army smart orders: change=9249 idle-away=581 rest=2562 refill=685 advance=7219
  20:00  groups (food group/need of army @staging): P0 50/50 of 78 @Nazjatar (10 gold/min); P1 1/50 of 1 @Zul'Gurub (25 gold/min); P2 all 35; P3 43/50 of 128 @Sholazar Basin (15 gold/min); P4 12/50 of 12 @Gnomeregan (20 gold/min); P6 all 112; P7 1/50 of 1 @Alterac Mountains (20 gold/min); P8 6/50 of 6 @Eldarath (10 gold/min); P9 4/205 of 4 @Hearthglen (15 gold/min); P11 1/50 of 1 @Felwood (10 gold/min); P12 45/50 of 45 @Twilight Landing (15 gold/min); P15 7/50 of 7 @Stranglethorn (15 gold/min); P18 4/50 of 4 @Dire Maul (15 gold/min); P22 8/50 of 8 @Drustvar (10 gold/min); P23 3/104 of 3 @Tirisfal Glades (15 gold/min);
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=3429 over=755 would-defer=962 deferred=0
  20:00  orders total=20296 near(<2k)=2491 mid(2-8k)=14492 far(>8k)=3313 idle-at-target(<300)=58
  25:00  combat 101/596 orders=0 | def-scan 29/331 | production peak 312 | wave-tick 2314/5257 | last gate sweep 2209
  25:00  army smart orders: change=10368 idle-away=707 rest=3691 refill=743 advance=8797
  25:00  groups (food group/need of army @staging): P0 all 104; P1 4/50 of 4 @Arathi Highlands (15 gold/min); P2 2/50 of 2 @Storm Peaks (15 gold/min); P3 50/50 of 139 @Deathknell (10 gold/min); P4 all 1; P6 50/50 of 139 @Balor (15 gold/min); P7 3/50 of 3 @Alterac Mountains (20 gold/min); P8 5/50 of 5 @Sulfuron Spire (10 gold/min); P9 4/96 of 4 @Tirisfal Glades (15 gold/min); P11 2/50 of 2 @Shrine to Malorne (10 gold/min); P12 24/50 of 24 @Windshear Crossing (10 gold/min); P15 8/50 of 8 @Deadwind Pass (15 gold/min); P16 3/50 of 3 @Thunderbluff (20 gold/min); P18 3/50 of 3 @Feralas (10 gold/min); P22 4/50 of 4 @Highbank (10 gold/min); P23 3/50 of 3 @Alterac Mountains (20 gold/min);
  25:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=6000 max-slot=3429 over=951 would-defer=1189 deferred=0
  25:00  orders total=24306 near(<2k)=2763 mid(2-8k)=17061 far(>8k)=4482 idle-at-target(<300)=59
```

Top tasks by total ops at 25:00 (of 38):
```
task combat every=0.500s runs=3000 last=101 max=1691 sum=1001123
task production every=1.000s runs=1499 last=72 max=312 sum=169647
task hero visits every=15.000s runs=99 last=2001 max=2481 sum=91492
task route areas every=15.000s runs=99 last=619 max=978 sum=72710
task route portals every=30.000s runs=49 last=766 max=3275 sum=69613
task gate sweep every=15.000s runs=99 last=0 max=3269 sum=14135
task ragnaros every=15.000s runs=99 last=271 max=271 sum=13828
task hero care every=15.000s runs=99 last=42 max=46 sum=3262
task kul tiras repair every=15.000s runs=99 last=0 max=1418 sum=1418
task starter kit every=15.000s runs=99 last=0 max=1077 sum=1300
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 25:00: P1 Stormwind atk a8 f2; P2 Quel'thalas atk a2 f4; P3 Scourge atk a144 f95; P4 Ironforge def a3 f4; P6 Fel Horde atk a144 f142; P7 Gilneas atk a3 f3; P8 Skywall atk a5 f6; P9 Lordaeron atk a4 f1; P11 Druids atk a10 f1; P12 Black Empire atk a40 f20; P15 Illidan atk a12 f11; P16 Ahn'qiraj atk a26 f8; P18 The Exodar atk a3 f3; P22 Kul'tiras atk a5 f5; P23 Legion atk a3 f1; P0 Warsong def a111 f102

### v0.44.0 -- 6 dumps, 5:00 .. 25:00 -- 0.44.0 default, Commander

> Note (0.44.1): chains work -- long orders (>8k) 615 (0.43.2: 4482), orders/min ~470-770, stalled units 2-5 % (no
> overload at ~2500 units). But 9261 route plans and up to 34860 ops in one slot (per-unit planning; fixed in 0.44.1).
> Switches (target/state): weak factions ~1 attack<->defence switch per minute (Frostwolf 35/22, Lordaeron 29/19,
> Stormwind 28/18, Druids 32/18), strong ones almost none. Old Gods 66 CPs, Kalimdor 8; most bots 1-4 food at 25:00.
> Maintainer: dwarves helped Stormwind; Outland portals a mess (in and out through 3 pairs); Stratholme's buildings
> survived (fixed in 0.44.1: leftover buildings are targets).

Skipped (other build or other game): WLAI_dump_21m25s.txt (v0.43.2, 21:25)

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3595 | 771 | 1047 / 127 | - | North Alliance 32, South Alliance 31, Burning Legion 19, Outland 25, Old Gods 33, Kalimdor 23 |
| 20:00 | 2480 | 723 | 443 / 97 | 73 | North Alliance 25, South Alliance 28, Burning Legion 27, Outland 32, Old Gods 58, Kalimdor 10 |
| 25:00 | 2342 | 471 | 431 / 94 | 76 | North Alliance 22, South Alliance 18, Burning Legion 30, Outland 41, Old Gods 66, Kalimdor 8 |

LOAD at the checkpoints:
```
  10:00  combat 216/486 orders=0 | def-scan 0/136 | production peak 228 | wave-tick 454/3308 | last gate sweep 3143
  10:00  army smart orders: change=5611 idle-away=387 rest=1228 refill=483 advance=0 | chains on issued=7155 points=21676 fallback=307 | stalled 2% of 13126
  10:00  groups (food group/need of army @staging): P0 2/50 of 2 @Durotar (10 gold/min); P1 all 28; P2 35/50 of 35 @Seradane (10 gold/min); P3 47/50 of 131 @Drak'Tharon Keep (30 gold/min); P4 46/50 of 47 @Northern Badlands (10 gold/min); P6 50/50 of 114 @Blasted Lands (15 gold/min); P7 50/50 of 70 @Jintha'Alor  (15 gold/min); P8 50/50 of 77 @Mulgore (10 gold/min); P9 57/50 of 57 @Strahnbrad (10 gold/min); P11 54/50 of 125 @Northern Ashenvale (10 gold/min); P12 all 63; P15 9/50 of 9 @-; P16 41/50 of 41 @Southern Barrens (10 gold/min); P18 15/50 of 15 @Stonetalon Peak (10 gold/min); P22 50/50 of 75 @Southern Highlands (10 gold/min); P23 50/50 of 74 @Howling Fjords (20 gold/min);
  10:00  switches per bot (target/state, since start): P0 13/4; P1 5/1; P2 7/0; P3 11/2; P4 10/0; P6 16/2; P7 10/0; P8 14/2; P9 6/0; P11 13/2; P12 10/3; P15 7/0; P16 13/0; P18 10/0; P22 11/0; P23 12/0;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=25710 over=557 would-defer=763 deferred=0
  10:00  orders total=7709 near(<2k)=1674 mid(2-8k)=5730 far(>8k)=305 idle-at-target(<300)=9
  20:00  combat 155/507 orders=0 | def-scan 8/245 | production peak 284 | wave-tick 2083/5338 | last gate sweep 2666
  20:00  army smart orders: change=11002 idle-away=840 rest=2364 refill=731 advance=0 | chains on issued=13687 points=43524 fallback=508 | stalled 5% of 28307
  20:00  groups (food group/need of army @staging): P0 3/50 of 3 @-; P1 5/50 of 5 @Westfall (10 gold/min); P2 1/50 of 1 @Seradane (10 gold/min); P3 all 76; P4 1/50 of 1 @Fuselight (10 gold/min); P6 41/76 of 41 @Elwynn Forest (20 gold/min); P7 2/50 of 2 @Strahnbrad (10 gold/min); P8 50/50 of 76 @Kezan (15 gold/min); P9 13/96 of 13 @Brill (15 gold/min); P11 1/50 of 1 @-; P12 37/50 of 37 @Twilight Landing (15 gold/min); P15 13/50 of 13 @-; P16 41/50 of 41 @Orgrimmar (25 gold/min); P22 1/50 of 1 @Twilight Shore (10 gold/min); P23 7/105 of 7 @Tirisfal Glades (15 gold/min);
  20:00  switches per bot (target/state, since start): P0 27/16; P1 22/14; P2 12/2; P3 22/7; P4 21/6; P6 31/12; P7 17/4; P8 24/4; P9 21/12; P11 28/16; P12 18/5; P15 19/0; P16 23/0; P18 18/10; P22 20/6; P23 16/2;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=34860 over=1141 would-defer=1515 deferred=0
  20:00  orders total=14937 near(<2k)=3375 mid(2-8k)=11064 far(>8k)=498 idle-at-target(<300)=18
  25:00  combat 86/507 orders=0 | def-scan 26/245 | production peak 284 | wave-tick 2044/5338 | last gate sweep 2074
  25:00  army smart orders: change=12714 idle-away=1017 rest=2794 refill=769 advance=0 | chains on issued=15799 points=50765 fallback=625 | stalled 5% of 32933
  25:00  groups (food group/need of army @staging): P0 3/50 of 3 @-; P1 2/50 of 2 @Stromgarde (25 gold/min); P2 1/136 of 1 @Arathi Highlands (15 gold/min); P3 76/136 of 76 @Durnholde (15 gold/min); P4 2/50 of 2 @Grim Batol (10 gold/min); P6 56/50 of 68 @Fuselight (10 gold/min); P7 3/50 of 3 @Hinterlands (10 gold/min); P8 54/56 of 91 @Northern Ashenvale (10 gold/min); P9 all 4; P11 2/50 of 2 @Moonglade (15 gold/min); P12 32/50 of 33 @Dragonmaw Port (10 gold/min); P15 18/50 of 18 @Boralus (25 gold/min); P16 50/50 of 52 @Spinebark Grove (10 gold/min); P22 3/50 of 3 @Northern Highlands (10 gold/min); P23 3/65 of 3 @Alterac Mountains (20 gold/min);
  25:00  switches per bot (target/state, since start): P0 35/22; P1 28/18; P2 13/2; P3 29/12; P4 28/12; P6 37/12; P7 23/8; P8 31/4; P9 29/19; P11 32/18; P12 19/5; P15 25/2; P16 30/0; P18 23/14; P22 25/10; P23 20/4;
  25:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=6000 max-slot=34860 over=1427 would-defer=1859 deferred=0
  25:00  orders total=17294 near(<2k)=3968 mid(2-8k)=12711 far(>8k)=615 idle-at-target(<300)=27
```

Top tasks by total ops at 25:00 (of 38):
```
task combat every=0.500s runs=3000 last=988 max=34860 sum=3790994
task production every=1.000s runs=1499 last=60 max=284 sum=167309
task hero visits every=15.000s runs=99 last=1826 max=2476 sum=89320
task route areas every=15.000s runs=99 last=513 max=1143 sum=73992
task route portals every=30.000s runs=49 last=712 max=3255 sum=67159
task gate sweep every=15.000s runs=99 last=0 max=3190 sum=13920
task ragnaros every=15.000s runs=99 last=182 max=225 sum=11245
task hero care every=15.000s runs=99 last=36 max=45 sum=3088
task kul tiras repair every=15.000s runs=99 last=0 max=1407 sum=1407
task starter kit every=15.000s runs=99 last=0 max=1105 sum=1328
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 25:00: P1 Stormwind atk a4 f0; P2 Quel'thalas atk a2 f4; P3 Scourge atk a77 f77; P4 Ironforge atk a2 f2; P6 Fel Horde atk a69 f67; P7 Gilneas atk a3 f3; P8 Skywall atk a94 f87; P9 Lordaeron def a4 f2; P11 Druids atk a6 f1; P12 Black Empire atk a46 f35; P15 Illidan atk a16 f12; P16 Ahn'qiraj atk a94 f62; P18 The Exodar atk a3 f3; P22 Kul'tiras atk a3 f4; P23 Legion atk a3 f1; P0 Frostwolf atk a5 f2

Heartbeat: WLAI heartbeat build=0.44.0 t=26m19s units-on-map=1973 orders/min=295 orders=smart
  task 26:11 demon portal
  task 26:11 combat
  task 26:12 production
  ...

### v0.44.1 -- 8 dumps, 5:00 .. 40:00 -- 0.44.1 chain off, Commander

> Note (0.44.2): bots had NO CP income in every game before 0.44.2 (WL pays human slots only); header goldBonus=0
> (Veteran). Fel Horde = the maintainer's autopilot -- a human slot, the only player paid CP income: its 250 food is
> not a balance result.

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3756 | 1162 | 1126 / 255 | - | North Alliance 32, South Alliance 28, Burning Legion 13, Outland 26, Old Gods 32, Kalimdor 33 |
| 20:00 | 2742 | 1244 | 576 / 249 | 61 | North Alliance 37, South Alliance 28, Burning Legion 14, Outland 28, Old Gods 48, Kalimdor 25 |
| 30:00 | 2404 | 614 | 468 / 251 | 61 | North Alliance 46, South Alliance 23, Burning Legion 7, Outland 31, Old Gods 53, Kalimdor 22 |
| 40:00 | 2268 | 497 | 406 / 251 | 60 | North Alliance 50, South Alliance 22, Burning Legion 4, Outland 34, Old Gods 49, Kalimdor 25 |

LOAD at the checkpoints:
```
  10:00  combat 215/365 orders=58 | def-scan 0/162 | production peak 349 | wave-tick 567/3224 | last gate sweep 3055
  10:00  army smart orders: change=5127 idle-away=317 rest=1381 refill=506 advance=4288 | chains off issued=0 points=0 fallback=0 | stalled 4% of 16108
  10:00  groups (food group/need of army @staging): P0 50/50 of 72 @Thunderbluff (20 gold/min); P1 37/50 of 37 @Duskwood (10 gold/min); P2 39/50 of 39 @Terrordale (1 gold/min); P3 42/50 of 102 @Dragonblight (20 gold/min); P4 41/50 of 41 @Northern Badlands (10 gold/min); P6 44/50 of 249 @Burning Steppes (20 gold/min); P7 62/50 of 65 @Deathknell (10 gold/min); P8 50/50 of 113 @Spinebark Grove (10 gold/min); P9 57/50 of 57 @Brill (15 gold/min); P11 50/50 of 65 @Ascendant's Rise (10 gold/min); P12 44/50 of 54 @Feralas (10 gold/min); P15 1/50 of 1 @Azshara Coast (10 gold/min); P16 28/50 of 31 @Desolace (10 gold/min); P18 all 6; P22 50/50 of 63 @Southern Highlands (10 gold/min); P23 24/50 of 24 @-;
  10:00  switches per bot (target/state, since start): P0 14/0; P1 5/0; P2 5/0; P3 9/2; P4 9/0; P6 13/2; P7 9/0; P8 14/2; P9 5/0; P11 9/2; P12 11/2; P15 9/0; P16 14/3; P18 11/1; P22 6/0; P23 6/0;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3256 over=402 would-defer=552 deferred=0
  10:00  orders total=11619 near(<2k)=1401 mid(2-8k)=8459 far(>8k)=1759 idle-at-target(<300)=16
  20:00  combat 198/511 orders=0 | def-scan 0/193 | production peak 386 | wave-tick 2177/5645 | last gate sweep 2748
  20:00  army smart orders: change=10002 idle-away=582 rest=3723 refill=667 advance=9082 | chains off issued=0 points=0 fallback=0 | stalled 7% of 37572
  20:00  groups (food group/need of army @staging): P0 all 39; P1 all 1; P2 2/80 of 2 @Tranquillien (15 gold/min); P3 7/50 of 7 @En'Kilah (10 gold/min); P4 1/60 of 1 @Searing Gorge (10 gold/min); P6 50/50 of 248 @Duskwood (10 gold/min); P7 31/88 of 31 @Grizzly Hills (20 gold/min); P8 40/50 of 40 @Moonglade (15 gold/min); P9 29/112 of 29 @Venture Bay (10 gold/min); P11 1/50 of 1 @Teldrassil (20 gold/min); P12 43/50 of 43 @Twilight Landing (15 gold/min); P15 19/50 of 19 @-; P16 21/65 of 21 @Ratchet (10 gold/min); P22 all 4; P23 1/50 of 1 @-;
  20:00  switches per bot (target/state, since start): P0 25/3; P1 15/7; P2 10/0; P3 21/12; P4 15/0; P6 21/4; P7 14/0; P8 29/2; P9 14/8; P11 22/14; P12 16/2; P15 18/2; P16 27/5; P18 24/15; P22 10/1; P23 16/6;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=3386 over=894 would-defer=1215 deferred=0
  20:00  orders total=24056 near(<2k)=2905 mid(2-8k)=17193 far(>8k)=3958 idle-at-target(<300)=32
  30:00  combat 184/511 orders=0 | def-scan 0/193 | production peak 386 | wave-tick 2282/5645 | last gate sweep 2100
  30:00  army smart orders: change=12042 idle-away=781 rest=4478 refill=827 advance=12069 | chains off issued=0 points=0 fallback=0 | stalled 10% of 50635
  30:00  groups (food group/need of army @staging): P0 5/50 of 5 @Northern Barrens (10 gold/min); P1 all 7; P2 2/50 of 2 @Seradane (10 gold/min); P3 1/50 of 1 @Icecrown Citadel (100 gold/min); P6 50/50 of 248 @Duskwood (10 gold/min); P7 18/72 of 18 @Mord’rethar (10 gold/min); P8 all 2; P9 58/50 of 58 @Borean Tundra (15 gold/min); P11 1/50 of 1 @Teldrassil (20 gold/min); P12 29/50 of 29 @Dragonmaw Port (10 gold/min); P15 2/56 of 2 @Western Stranglethorn (15 gold/min); P16 3/50 of 3 @Thunderbluff (20 gold/min); P22 3/50 of 3 @Southern Highlands (10 gold/min); P23 3/50 of 3 @-;
  30:00  switches per bot (target/state, since start): P0 34/8; P1 23/13; P2 18/0; P3 29/16; P4 20/4; P6 26/6; P7 23/0; P8 40/5; P9 26/8; P11 25/14; P12 21/2; P15 28/4; P16 33/7; P18 31/20; P22 16/2; P23 19/6;
  30:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=7200 max-slot=3386 over=1399 would-defer=1858 deferred=0
  30:00  orders total=30197 near(<2k)=3811 mid(2-8k)=21499 far(>8k)=4887 idle-at-target(<300)=72
  40:00  combat 212/572 orders=0 | def-scan 36/269 | production peak 386 | wave-tick 2184/5645 | last gate sweep 1887
  40:00  army smart orders: change=13606 idle-away=1030 rest=5782 refill=900 advance=13851 | chains off issued=0 points=0 fallback=0 | stalled 10% of 59469
  40:00  groups (food group/need of army @staging): P0 5/50 of 5 @Northern Ashenvale (10 gold/min); P1 2/50 of 2 @Stromgarde (25 gold/min); P2 4/72 of 4 @Seradane (10 gold/min); P3 1/50 of 1 @-; P4 1/50 of 1 @Ironforge (30 gold/min); P6 all 249; P7 12/126 of 12 @Arathi Highlands (15 gold/min); P8 3/50 of 3 @Azuremyst Isle (10 gold/min); P9 54/75 of 54 @Andorhal (15 gold/min); P11 1/50 of 1 @Teldrassil (20 gold/min); P12 3/50 of 3 @Dun Morogh (15 gold/min); P15 1/50 of 1 @Blasted Lands (15 gold/min); P22 7/185 of 7 @Twilight Shore (10 gold/min); P23 3/50 of 3 @-;
  40:00  switches per bot (target/state, since start): P0 43/10; P1 35/22; P2 20/0; P3 34/18; P4 22/4; P6 37/9; P7 25/0; P8 43/6; P9 32/8; P11 27/14; P12 24/2; P15 34/6; P16 42/15; P18 34/20; P22 21/6; P23 21/6;
  40:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=9600 max-slot=3386 over=1883 would-defer=2458 deferred=0
  40:00  orders total=35169 near(<2k)=4498 mid(2-8k)=25133 far(>8k)=5538 idle-at-target(<300)=88
```

Top tasks by total ops at 40:00 (of 41):
```
task combat every=0.500s runs=4800 last=212 max=2200 sum=1750319
task production every=1.000s runs=2399 last=26 max=386 sum=272566
task hero visits every=15.000s runs=159 last=1777 max=2703 sum=213775
task route structures 2 every=15.000s runs=159 last=876 max=1147 sum=135826
task route portals every=30.000s runs=79 last=667 max=3256 sum=87961
task route structures 1 every=15.000s runs=159 last=340 max=892 sum=81036
task route structures 3 every=15.000s runs=159 last=293 max=400 sum=50466
task ragnaros every=15.000s runs=159 last=370 max=383 sum=44460
task route structures 4 every=15.000s runs=159 last=90 max=292 sum=23490
task gate sweep every=15.000s runs=159 last=0 max=3296 sum=20233
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 40:00: P0 Frostwolf atk a6 f6; P1 Stormwind atk a5 f0; P2 Quel'thalas atk a4 f4; P3 Scourge atk a3 f1; P4 Ironforge atk a2 f2; P7 Dalaran atk a14 f8; P8 Skywall atk a3 f3; P9 Lordaeron atk a54 f53; P11 Druids atk a4 f1; P12 Black Empire atk a12 f6; P15 Illidan atk a1 f5; P16 Ahn'qiraj atk a30 f7; P18 Sentinels atk a4 f0; P22 Kul'tiras atk a10 f5; P23 Legion atk a3 f1; P6 Fel Horde def a251 f250

Heartbeat: WLAI heartbeat build=0.44.1 t=42m49s units-on-map=1836 orders/min=842 orders=smart
  task 42:41 demon portal
  task 42:41 combat
  task 42:42 production
  ...

### v0.44.1 -- 114 dumps, 5:00 .. 570:00 -- 0.44.1 chains, Commander

> Note (0.44.2): goldBonus=0 (Veteran), and bots had NO CP income -- they trained almost nothing after 40:00
> (Warsong 92 units at 40:00 and at 570:00), lost bases and altars; from 240:00 only Outland and Old Gods held CPs,
> both without armies except the maintainer's autopilot Fel Horde (human slot, paid by WL: 718,000 gold). The
> stalemate is the missing income, not portals. max-slot 78,318 (chain trees; fixed in 0.44.2). 1048 units idle at
> waygate exits (queued chains lost in the teleport). The Old Gods rift was open (both directions live).

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3699 | 848 | 1158 / 239 | - | North Alliance 32, South Alliance 35, Burning Legion 19, Outland 25, Old Gods 32, Kalimdor 28 |
| 20:00 | 2669 | 1004 | 506 / 256 | 61 | North Alliance 26, South Alliance 36, Burning Legion 27, Outland 22, Old Gods 34, Kalimdor 39 |
| 30:00 | 2509 | 509 | 460 / 249 | 61 | North Alliance 15, South Alliance 29, Burning Legion 38, Outland 28, Old Gods 26, Kalimdor 48 |
| 45:00 | 2234 | 516 | 426 / 262 | 60 | North Alliance 9, South Alliance 23, Burning Legion 43, Outland 36, Old Gods 34, Kalimdor 40 |
| 60:00 | 2094 | 439 | 407 / 249 | 61 | North Alliance 7, South Alliance 4, Burning Legion 56, Outland 44, Old Gods 34, Kalimdor 39, Kezan Enterprise Pact 1 |
| 90:00 | 1954 | 263 | 341 / 249 | 61 | North Alliance 7, South Alliance 1, Burning Legion 47, Outland 57, Old Gods 48, Kalimdor 24, Kezan Enterprise Pact 1 |
| 120:00 | 1870 | 319 | 331 / 250 | 61 | North Alliance 7, South Alliance 1, Burning Legion 30, Outland 74, Old Gods 70, Kalimdor 2, Kezan Enterprise Pact 1 |
| 180:00 | 1683 | 246 | 351 / 249 | 61 | North Alliance 7, South Alliance 0, Burning Legion 4, Outland 100, Old Gods 75, Kalimdor 0, Kezan Enterprise Pact 0 |
| 240:00 | 1583 | 189 | 346 / 249 | 61 | North Alliance 0, South Alliance 0, Burning Legion 4, Outland 108, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |
| 300:00 | 1582 | 169 | 345 / 249 | 61 | North Alliance 0, South Alliance 0, Burning Legion 4, Outland 108, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |
| 400:00 | 1581 | 186 | 344 / 249 | 61 | North Alliance 0, South Alliance 0, Burning Legion 4, Outland 108, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |
| 500:00 | 1581 | 222 | 344 / 249 | 61 | North Alliance 0, South Alliance 0, Burning Legion 4, Outland 108, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |
| 570:00 | 1581 | 234 | 344 / 249 | 61 | North Alliance 0, South Alliance 0, Burning Legion 4, Outland 108, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |

LOAD at the checkpoints:
```
  10:00  combat 114/513 orders=0 | def-scan 0/212 | production peak 325 | wave-tick 561/3301 | last gate sweep 3129
  10:00  army smart orders: change=6043 idle-away=312 rest=1624 refill=503 advance=0 | chains on issued=7594 points=24737 fallback=605 | stalled 3% of 14526
  10:00  groups (food group/need of army @staging): P0 46/50 of 46 @Dustwallow Marsh (10 gold/min); P1 50/50 of 51 @Burning Steppes (20 gold/min); P2 44/50 of 44 @Seradane (10 gold/min); P3 50/50 of 126 @Crystalsong Forest (15 gold/min); P4 37/50 of 37 @Northern Badlands (10 gold/min); P6 49/50 of 237 @Netherstorm (10 gold/min); P7 53/53 of 69 @Arathi Highlands (15 gold/min); P8 50/50 of 84 @Shimmering Flats (10 gold/min); P9 56/56 of 56 @Strahnbrad (10 gold/min); P11 50/50 of 83 @Sulfuron Spire (10 gold/min); P12 58/50 of 58 @Dire Maul (15 gold/min); P15 24/50 of 24 @Southern Ashenvale (10 gold/min); P16 31/50 of 31 @Mulgore (10 gold/min); P18 all 45; P22 51/51 of 70 @Drustvar (10 gold/min); P23 53/54 of 70 @Howling Fjords (20 gold/min);
  10:00  switches per bot (target/state, since start): P0 16/6; P1 8/0; P2 9/0; P3 13/2; P4 9/0; P6 11/2; P7 11/0; P8 16/2; P9 7/0; P11 12/4; P12 13/2; P15 9/0; P16 14/0; P18 12/1; P22 11/0; P23 13/0;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=17945 over=575 would-defer=805 deferred=0
  10:00  orders total=8482 near(<2k)=1893 mid(2-8k)=6024 far(>8k)=565 idle-at-target(<300)=12
  20:00  combat 78/748 orders=0 | def-scan 0/343 | production peak 367 | wave-tick 2230/5380 | last gate sweep 2698
  20:00  army smart orders: change=12529 idle-away=944 rest=4258 refill=794 advance=0 | chains on issued=15393 points=47733 fallback=1676 | stalled 6% of 34043
  20:00  groups (food group/need of army @staging): P0 49/50 of 49 @Stonemaul (10 gold/min); P1 4/54 of 4 @Deadwind Pass (15 gold/min); P2 2/50 of 2 @Seradane (10 gold/min); P3 50/50 of 95 @Caer Darrow (15 gold/min); P4 1/99 of 1 @Northern Badlands (10 gold/min); P6 49/50 of 246 @Redridge Mountains (10 gold/min); P7 3/72 of 3 @Strahnbrad (10 gold/min); P8 all 18; P11 2/50 of 2 @Felwood (10 gold/min); P12 3/50 of 3 @Twilight Landing (15 gold/min); P15 4/50 of 4 @Blasted Lands (15 gold/min); P16 3/50 of 3 @Silithus (15 gold/min); P18 6/50 of 6 @Grove of the Ancients (10 gold/min); P22 2/50 of 2 @Drustvar (10 gold/min); P23 10/50 of 10 @Hinterlands (10 gold/min);
  20:00  switches per bot (target/state, since start): P0 34/10; P1 21/8; P2 13/0; P3 26/4; P4 17/0; P6 24/8; P7 21/4; P8 32/5; P9 17/10; P11 30/18; P12 22/8; P15 21/4; P16 25/6; P18 21/3; P22 22/2; P23 25/2;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=40190 over=1228 would-defer=1698 deferred=0
  20:00  orders total=18525 near(<2k)=4853 mid(2-8k)=12039 far(>8k)=1633 idle-at-target(<300)=37
  30:00  combat 80/748 orders=0 | def-scan 0/343 | production peak 367 | wave-tick 2080/5380 | last gate sweep 2186
  30:00  army smart orders: change=15195 idle-away=1309 rest=6253 refill=856 advance=0 | chains on issued=19948 points=56483 fallback=1726 | stalled 7% of 43996
  30:00  groups (food group/need of army @staging): P0 14/50 of 14 @Gadgetzan (20 gold/min); P1 8/50 of 8 @Elwynn Forest (20 gold/min); P2 2/50 of 2 @Tranquillien (15 gold/min); P3 107/105 of 107 @Corin's Crossing (15 gold/min); P6 all 248; P7 all 1; P8 5/50 of 5 @Sulfuron Spire (10 gold/min); P9 1/65 of 1 @Tyr's Hand (30 gold/min); P11 2/50 of 2 @Nordrassil (25 gold/min); P12 1/50 of 1 @Dragonmaw Port (10 gold/min); P15 4/72 of 4 @-; P18 3/50 of 3 @Winterspring (10 gold/min); P22 8/50 of 8 @Zul'Gurub (25 gold/min); P23 10/80 of 10 @Dalaran (30 gold/min);
  30:00  switches per bot (target/state, since start): P0 53/18; P1 32/16; P2 19/4; P3 41/6; P4 25/4; P6 33/9; P7 32/13; P8 46/16; P9 28/18; P11 32/18; P12 25/10; P15 27/4; P16 36/12; P18 32/7; P22 26/4; P23 36/2;
  30:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=7200 max-slot=40190 over=1843 would-defer=2477 deferred=0
  30:00  orders total=23613 near(<2k)=6087 mid(2-8k)=15856 far(>8k)=1670 idle-at-target(<300)=50
  45:00  combat 117/748 orders=1 | def-scan 31/363 | production peak 378 | wave-tick 1787/5380 | last gate sweep 1901
  45:00  army smart orders: change=18356 idle-away=1746 rest=10215 refill=1041 advance=0 | chains on issued=25976 points=71372 fallback=2106 | stalled 9% of 58529
  45:00  groups (food group/need of army @staging): P0 2/50 of 2 @Thunderbluff (20 gold/min); P1 4/50 of 4 @Arathi Highlands (15 gold/min); P2 2/50 of 2 @Tranquillien (15 gold/min); P3 50/50 of 74 @Blackwald (10 gold/min); P4 1/50 of 1 @Northern Badlands (10 gold/min); P6 47/50 of 250 @Boralus (25 gold/min); P7 1/50 of 1 @Tempest Reach (10 gold/min); P8 4/50 of 4 @Feralas (10 gold/min); P11 3/50 of 3 @Azshara Coast (10 gold/min); P12 1/50 of 1 @Dragonmaw Port (10 gold/min); P15 4/50 of 4 @Blasted Lands (15 gold/min); P16 2/88 of 2 @Mulgore (10 gold/min); P18 7/50 of 7 @Desolace (10 gold/min); P22 2/101 of 2 @Drustvar (10 gold/min);
  45:00  switches per bot (target/state, since start): P0 73/32; P1 45/26; P2 23/4; P3 60/6; P4 36/10; P6 56/10; P7 45/28; P8 71/26; P9 39/24; P11 41/18; P12 27/12; P15 31/4; P16 49/14; P18 48/15; P22 34/6; P23 53/6;
  45:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=10800 max-slot=78860 over=2831 would-defer=3785 deferred=0
  45:00  orders total=31358 near(<2k)=9152 mid(2-8k)=20191 far(>8k)=2015 idle-at-target(<300)=90
  60:00  combat 41/748 orders=0 | def-scan 0/363 | production peak 421 | wave-tick 2038/5380 | last gate sweep 1784
  60:00  army smart orders: change=20757 idle-away=2303 rest=13650 refill=1230 advance=0 | chains on issued=31421 points=90595 fallback=2652 | stalled 10% of 72334
  60:00  groups (food group/need of army @staging): P0 all 2; P1 2/50 of 2 @Thundermar (10 gold/min); P2 2/50 of 2 @Tranquillien (15 gold/min); P3 26/50 of 69 @Menethil Harbour (10 gold/min); P6 47/50 of 247 @Northern Badlands (10 gold/min); P7 1/50 of 1 @-; P8 11/50 of 11 @Echo Isles (15 gold/min); P11 2/50 of 2 @Feralas (10 gold/min); P12 3/50 of 3 @Dragonmaw Port (10 gold/min); P15 1/83 of 1 @Wetlands (10 gold/min); P16 3/50 of 3 @Thunderbluff (20 gold/min); P22 1/50 of 1 @Kezan (15 gold/min);
  60:00  switches per bot (target/state, since start): P0 85/41; P1 55/30; P2 26/4; P3 75/10; P4 46/18; P6 71/12; P7 52/32; P8 88/32; P9 43/24; P11 42/18; P12 31/12; P15 41/6; P16 65/20; P18 58/19; P22 41/10; P23 71/6;
  60:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=14400 max-slot=78860 over=3829 would-defer=5088 deferred=0
  60:00  orders total=37940 near(<2k)=11252 mid(2-8k)=24127 far(>8k)=2561 idle-at-target(<300)=106
  90:00  combat 5/748 orders=0 | def-scan 0/363 | production peak 421 | wave-tick 1870/5380 | last gate sweep 1579
  90:00  army smart orders: change=22730 idle-away=3708 rest=17821 refill=1566 advance=0 | chains on issued=37551 points=112988 fallback=4072 | stalled 10% of 91882
  90:00  groups (food group/need of army @staging): P0 2/50 of 2 @Durotar (10 gold/min); P1 2/50 of 2 @-; P2 2/50 of 2 @Tranquillien (15 gold/min); P3 3/50 of 3 @Stratholme (30 gold/min); P6 50/50 of 249 @Sylvanaar (10 gold/min); P7 1/50 of 1 @-; P8 4/50 of 4 @Ranazjar Isle (10 gold/min); P11 2/50 of 2 @Shrine to Malorne (10 gold/min); P12 2/50 of 2 @Twilight Landing (15 gold/min); P15 4/112 of 4 @Fuselight (10 gold/min); P16 2/50 of 2 @Nordrassil (25 gold/min); P22 1/50 of 1 @Kezan (15 gold/min); P23 2/50 of 2 @Durnholde (15 gold/min);
  90:00  switches per bot (target/state, since start): P0 99/46; P1 58/30; P2 33/4; P3 94/14; P4 58/26; P6 91/12; P7 60/32; P8 132/44; P9 50/24; P11 51/18; P12 46/20; P15 55/6; P16 92/28; P18 96/43; P22 50/10; P23 87/12;
  90:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=21600 max-slot=78860 over=5984 would-defer=7828 deferred=0
  90:00  orders total=45825 near(<2k)=13333 mid(2-8k)=28511 far(>8k)=3981 idle-at-target(<300)=197
 120:00  combat 5/748 orders=0 | def-scan 0/363 | production peak 447 | wave-tick 1788/5380 | last gate sweep 1485
 120:00  army smart orders: change=25335 idle-away=5385 rest=22948 refill=1727 advance=0 | chains on issued=45675 points=142967 fallback=5191 | stalled 10% of 113193
 120:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @Tranquillien (15 gold/min); P3 all 3; P6 111/112 of 249 @Twilight Shore (10 gold/min); P7 1/50 of 1 @-; P8 4/50 of 4 @Ascendant's Rise (10 gold/min); P11 1/50 of 1 @Teldrassil (20 gold/min); P12 3/50 of 3 @Twilight Landing (15 gold/min); P15 4/112 of 4 @Fuselight (10 gold/min); P16 3/50 of 3 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @Kezan (15 gold/min); P23 2/50 of 2 @Dalaran (30 gold/min);
 120:00  switches per bot (target/state, since start): P0 115/54; P1 65/30; P2 40/4; P3 117/31; P4 67/26; P6 118/18; P7 68/32; P8 172/52; P9 57/24; P11 66/24; P12 54/20; P15 64/6; P16 107/36; P18 129/69; P22 57/10; P23 101/20;
 120:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=28800 max-slot=78860 over=8199 would-defer=10564 deferred=0
 120:00  orders total=55395 near(<2k)=15694 mid(2-8k)=34601 far(>8k)=5100 idle-at-target(<300)=226
 180:00  combat 5/756 orders=0 | def-scan 0/363 | production peak 504 | wave-tick 1021/5380 | last gate sweep 1330
 180:00  army smart orders: change=28275 idle-away=8890 rest=31136 refill=1862 advance=0 | chains on issued=57908 points=194352 fallback=7211 | stalled 10% of 154795
 180:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @Tranquillien (15 gold/min); P3 3/50 of 3 @-; P6 50/50 of 249 @Northern Highlands (10 gold/min); P7 1/50 of 1 @-; P8 22/50 of 22 @Dragonmaw Port (10 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 9/50 of 9 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 3/65 of 3 @-;
 180:00  switches per bot (target/state, since start): P0 131/54; P1 82/32; P2 53/4; P3 150/54; P4 81/26; P6 158/18; P7 82/32; P8 199/52; P9 72/24; P11 84/28; P12 69/20; P15 79/6; P16 123/36; P18 146/69; P22 74/12; P23 121/24;
 180:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=43200 max-slot=78860 over=12556 would-defer=15989 deferred=0
 180:00  orders total=70163 near(<2k)=18213 mid(2-8k)=44830 far(>8k)=7120 idle-at-target(<300)=238
 240:00  combat 5/768 orders=0 | def-scan 0/363 | production peak 522 | wave-tick 1059/5380 | last gate sweep 1212
 240:00  army smart orders: change=31063 idle-away=12448 rest=36119 refill=1878 advance=0 | chains on issued=67046 points=242005 fallback=8714 | stalled 9% of 192807
 240:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @-; P3 3/50 of 3 @-; P6 50/50 of 249 @-; P7 1/50 of 1 @-; P8 15/50 of 15 @Kezan (15 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 7/50 of 7 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 2/50 of 2 @-;
 240:00  switches per bot (target/state, since start): P0 146/54; P1 95/32; P2 79/18; P3 165/54; P4 96/26; P6 191/18; P7 97/32; P8 214/52; P9 87/24; P11 101/28; P12 84/20; P15 95/6; P16 137/36; P18 161/69; P22 89/12; P23 136/24;
 240:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=57600 max-slot=78860 over=16909 would-defer=21371 deferred=0
 240:00  orders total=81508 near(<2k)=20469 mid(2-8k)=52416 far(>8k)=8623 idle-at-target(<300)=256
 300:00  combat 5/768 orders=0 | def-scan 0/363 | production peak 522 | wave-tick 1056/5380 | last gate sweep 1211
 300:00  army smart orders: change=32665 idle-away=16138 rest=40986 refill=1883 advance=0 | chains on issued=72059 points=275746 fallback=13823 | stalled 9% of 217770
 300:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @-; P3 3/50 of 3 @-; P6 50/50 of 249 @-; P7 1/50 of 1 @-; P8 15/50 of 15 @Kezan (15 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 6/50 of 6 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 3/50 of 3 @-;
 300:00  switches per bot (target/state, since start): P0 161/54; P1 110/32; P2 94/18; P3 180/54; P4 111/26; P6 206/18; P7 112/32; P8 229/52; P9 101/24; P11 118/28; P12 99/20; P15 110/6; P16 151/36; P18 175/69; P22 104/12; P23 151/24;
 300:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=72000 max-slot=78860 over=21292 would-defer=26779 deferred=0
 300:00  orders total=91672 near(<2k)=21443 mid(2-8k)=56497 far(>8k)=13732 idle-at-target(<300)=320
 400:00  combat 5/768 orders=0 | def-scan 0/363 | production peak 522 | wave-tick 1059/5380 | last gate sweep 1210
 400:00  army smart orders: change=35455 idle-away=23398 rest=49558 refill=1888 advance=0 | chains on issued=80419 points=331923 fallback=24005 | stalled 11% of 254807
 400:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @-; P3 3/50 of 3 @-; P6 148/148 of 249 @Sylvanaar (10 gold/min); P7 1/50 of 1 @-; P8 14/50 of 14 @Kezan (15 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 6/50 of 6 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 2/50 of 2 @-;
 400:00  switches per bot (target/state, since start): P0 185/54; P1 134/32; P2 119/18; P3 205/54; P4 136/26; P6 231/18; P7 137/32; P8 253/52; P9 126/24; P11 147/28; P12 124/20; P15 134/6; P16 176/36; P18 200/69; P22 128/12; P23 176/24;
 400:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=96000 max-slot=78860 over=28557 would-defer=35740 deferred=0
 400:00  orders total=110299 near(<2k)=23556 mid(2-8k)=62829 far(>8k)=23914 idle-at-target(<300)=880
 500:00  combat 5/768 orders=0 | def-scan 0/363 | production peak 522 | wave-tick 1059/5380 | last gate sweep 1210
 500:00  army smart orders: change=38087 idle-away=32361 rest=60128 refill=1896 advance=0 | chains on issued=88512 points=385279 fallback=37998 | stalled 14% of 287626
 500:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @-; P3 3/50 of 3 @-; P6 148/148 of 249 @Sylvanaar (10 gold/min); P7 1/50 of 1 @-; P8 14/50 of 14 @Kezan (15 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 6/50 of 6 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 2/50 of 2 @-;
 500:00  switches per bot (target/state, since start): P0 210/54; P1 158/32; P2 143/18; P3 229/54; P4 160/26; P6 255/18; P7 161/32; P8 278/52; P9 150/24; P11 176/28; P12 148/20; P15 158/6; P16 200/36; P18 225/69; P22 153/12; P23 200/24;
 500:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=120000 max-slot=78860 over=35808 would-defer=44709 deferred=0
 500:00  orders total=132472 near(<2k)=25871 mid(2-8k)=68694 far(>8k)=37907 idle-at-target(<300)=1619
 570:00  combat 5/768 orders=0 | def-scan 0/363 | production peak 522 | wave-tick 1059/5380 | last gate sweep 1210
 570:00  army smart orders: change=39856 idle-away=39532 rest=67557 refill=1901 advance=0 | chains on issued=94083 points=420902 fallback=48753 | stalled 15% of 308166
 570:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 2/50 of 2 @-; P3 3/50 of 3 @-; P6 50/50 of 249 @-; P7 1/50 of 1 @-; P8 14/50 of 14 @Kezan (15 gold/min); P11 1/50 of 1 @-; P12 4/50 of 4 @Twilight Landing (15 gold/min); P15 4/50 of 4 @-; P16 6/50 of 6 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 2/50 of 2 @-;
 570:00  switches per bot (target/state, since start): P0 227/54; P1 175/32; P2 160/18; P3 246/54; P4 177/26; P6 272/18; P7 178/32; P8 295/52; P9 168/24; P11 196/28; P12 165/20; P15 175/6; P16 217/36; P18 242/69; P22 170/12; P23 217/24;
 570:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=136800 max-slot=78860 over=40907 would-defer=50983 deferred=0
 570:00  orders total=148846 near(<2k)=27595 mid(2-8k)=72589 far(>8k)=48662 idle-at-target(<300)=2256
```

Top tasks by total ops at 570:00 (of 41):
```
task combat every=0.500s runs=68400 last=5 max=78318 sum=47921851
task production every=1.000s runs=34199 last=4 max=522 sum=2421544
task hero visits every=15.000s runs=2279 last=521 max=2527 sum=1691825
task route structures 2 every=15.000s runs=2279 last=686 max=1170 sum=1520090
task ragnaros every=15.000s runs=2279 last=521 max=522 sum=1057298
task route portals every=30.000s runs=1139 last=570 max=3275 sum=716172
task route structures 3 every=15.000s runs=2279 last=267 max=411 sum=602444
task route structures 1 every=15.000s runs=2279 last=25 max=870 sum=303044
task gate sweep every=15.000s runs=2279 last=0 max=3319 sum=158331
task route structures 4 every=15.000s runs=2279 last=33 max=418 sum=157614
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 570:00: P0 Warsong atk a4 f0; P1 Stormwind atk a6 f0; P2 Quel'thalas atk a3 f0; P3 Scourge atk a3 f0; P4 Ironforge atk a4 f0; P7 Gilneas atk a4 f0; P8 Skywall atk a15 f13; P9 Lordaeron atk a2 f0; P11 Druids atk a3 f0; P12 Black Empire atk a13 f7; P15 Illidan atk a4 f7; P16 Ahn'qiraj atk a26 f9; P18 The Exodar atk a4 f0; P22 Kezan Enterprise atk a1 f20; P23 Legion atk a3 f0; P6 Fel Horde atk a249 f250

Heartbeat: WLAI heartbeat build=0.44.1 t=571m59s units-on-map=1210 orders/min=192 orders=smart
  task 571:52 route rocks
  task 571:52 early quests
  task 571:52 legion events
  ...

### v0.44.2 -- 1 dumps, 14:12 .. 14:12 -- 0.44.2 Veteran, chains, bots WITH CP income, autopilot Fel Horde (manual dump at 14:12)

> Note (0.44.3): first game with CP income for bots -- 4920 units on the map at 14 min (heartbeat), income paid
> 2400-4800 per bot. max-slot 5207 (0.44.1: 78,318 -- 4 route trees work), stalled 6 %. "rest" orders 12,913 vs
> "change" 13,322 -- staging churn, fixed in 0.44.3. Maintainer: no distant creep camps cleared (creep targets came
> only in 0.44.3). Corrections: west (Kalimdor + islands) and east (Eastern Kingdoms + Kul Tiras) are linked only by
> the Old Gods rift; the ships link Kalimdor to its islands.

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 14:12 | 5227 | 1956 | 2986 / 288 | - | North Alliance 29, South Alliance 38, Burning Legion 23, Outland 20, Old Gods 37, Kalimdor 34 |

LOAD at the checkpoints:
```
  14:12  combat 550/683 orders=5 | def-scan 268/340 | production peak 452 | wave-tick 701/5572 | last gate sweep 4878
  14:12  army smart orders: change=13322 idle-away=823 rest=12913 refill=718 advance=0 | chains on issued=25325 points=79633 fallback=1134 | stalled 6% of 62099
  14:12  groups (food group/need of army @staging): P0 50/50 of 83 @-; P1 50/50 of 154 @Elwynn Forest (20 gold/min); P2 87/104 of 169 @Seradane (10 gold/min); P3 75/89 of 269 @Terrordale (1 gold/min); P4 138/146 of 138 @Northern Badlands (10 gold/min); P6 all 181; P7 72/104 of 240 @Hinterlands (10 gold/min); P8 116/116 of 212 @Sulfuron Spire (10 gold/min); P9 all 140; P11 50/50 of 172 @Broken Shore (10 gold/min); P12 all 110; P15 53/50 of 56 @Nethergarde Keep (15 gold/min); P16 34/50 of 221 @Ratchet (10 gold/min); P18 76/147 of 85 @Desolace (10 gold/min); P22 50/50 of 190 @Blackrock Depths (20 gold/min); P23 all 171;
  14:12  switches per bot (target/state, since start): P0 20/8; P1 17/4; P2 10/2; P3 13/2; P4 17/0; P6 20/3; P7 10/0; P8 16/2; P9 12/3; P11 16/2; P12 12/3; P15 15/0; P16 17/0; P18 18/4; P22 17/2; P23 13/1;
  14:12  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2372; P1 2467; P2 3121; P3 4505; P4 2868; P7 2840; P8 3757; P9 3673; P11 2894; P12 3430; P15 2498; P16 4836; P18 2861; P22 2804; P23 3775;
  14:12  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=3411 max-slot=5207 over=971 would-defer=1340 deferred=0
  14:12  orders total=27776 near(<2k)=6378 mid(2-8k)=20264 far(>8k)=1134 idle-at-target(<300)=19
```

Top tasks by total ops at 14:12 (of 42):
```
task combat every=0.500s runs=1705 last=1004 max=2628 sum=991692
task production every=1.000s runs=852 last=340 max=452 sum=155116
task route structures 2 every=15.000s runs=56 last=1606 max=1626 sum=60362
task route structures 1 every=15.000s runs=56 last=1364 max=1445 sum=50438
task route portals every=30.000s runs=28 last=798 max=3277 sum=48349
task route structures 3 every=15.000s runs=56 last=750 max=773 sum=28357
task route structures 4 every=15.000s runs=56 last=756 max=828 sum=27830
task hero visits every=15.000s runs=56 last=395 max=518 sum=12422
task gate sweep every=15.000s runs=56 last=0 max=4878 sum=11569
task ragnaros every=15.000s runs=56 last=275 max=350 sum=6443
```

Errors (TICK ERROR / FAILED / section errors): none

Bots at 14:12: P0 Warsong atk a84 f77; P1 Stormwind atk a159 f142; P2 Quel'thalas atk a182 f164; P3 Scourge atk a288 f250; P4 Ironforge atk a141 f142; P7 Gilneas atk a282 f188; P8 Skywall atk a214 f200; P9 Lordaeron def a165 f141; P11 Druids atk a183 f176; P12 Black Empire def a204 f107; P15 Illidan atk a62 f60; P16 Ahn'qiraj atk a263 f200; P18 Sentinels atk a108 f57; P22 Kul'tiras atk a185 f172; P23 Legion def a273 f139; P6 Fel Horde def a193 f186
