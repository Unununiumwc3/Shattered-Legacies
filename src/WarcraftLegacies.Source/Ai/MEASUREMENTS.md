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

### v0.45.0 -- 17 dumps, 5:00 .. 540:00 -- 0.45.0, 16 bots, goldBonus 0, 540 min; maintainer watched only the end (very laggy); no stopwatch. Rift: no crossing in 510 min. TICK ERROR route creeps every run (creep camps never worked).

Skipped (other build or other game): WLAI_dump_14m12s.txt (v0.44.2, 14:12)

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 5221 | 1281 | 2705 / 253 | - | North Alliance 32, South Alliance 34, Burning Legion 19, Outland 25, Old Gods 33, Kalimdor 27 |
| 20:00 | 4060 | 1691 | 1936 / 310 | - | North Alliance 31, South Alliance 36, Burning Legion 22, Outland 22, Old Gods 50, Kalimdor 22 |
| 30:00 | 3669 | 910 | 1628 / 304 | - | North Alliance 30, South Alliance 39, Burning Legion 23, Outland 19, Old Gods 69, Kalimdor 4 |
| 45:00 | 3532 | 844 | 1643 / 259 | - | North Alliance 27, South Alliance 31, Burning Legion 22, Outland 31, Old Gods 73, Kalimdor 0 |
| 60:00 | 3467 | 800 | 1830 / 295 | - | North Alliance 17, South Alliance 26, Burning Legion 28, Outland 41, Old Gods 73, Kalimdor 0, Kezan Enterprise Pact 1 |
| 90:00 | 3281 | 592 | 1771 / 260 | - | North Alliance 14, South Alliance 5, Burning Legion 36, Outland 57, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |
| 540:00 | 3367 | 634 | 1939 / 294 | - | North Alliance 12, South Alliance 0, Burning Legion 35, Outland 66, Old Gods 74, Kalimdor 0, Kezan Enterprise Pact 0 |

LOAD at the checkpoints:
```
  10:00  combat 200/616 orders=0 | def-scan 0/142 | production peak 374 | wave-tick 686/4088 | last gate sweep 3832
  10:00  army smart orders: change=8122 idle-away=356 rest=3349 refill=615 advance=347 exit=19 | chains on issued=12267 points=38206 fallback=140 | stalled 3% of 31565
  10:00  groups (food group/need of army @staging): P0 all 12; P1 50/50 of 147 @Blasted Lands (15 gold/min); P2 50/50 of 148 @Deathknell (10 gold/min); P3 50/50 of 254 @Drak'Tharon Keep (30 gold/min); P4 50/50 of 117 @Dragonmaw Gates (10 gold/min); P6 50/50 of 225 @Blackrock Depths (20 gold/min); P7 50/50 of 173 @Silverpine Forest (15 gold/min); P8 50/50 of 200 @Gadgetzan (20 gold/min); P9 50/50 of 201 @Tirisfal Glades (15 gold/min); P11 50/50 of 176 @Southern Barrens (10 gold/min); P12 50/50 of 196 @Dire Maul (15 gold/min); P15 31/50 of 54 @Eldarath (10 gold/min); P16 50/50 of 189 @Stonemaul (10 gold/min); P18 50/50 of 161 @Windshear Crossing (10 gold/min); P22 38/50 of 144 @Southern Highlands (10 gold/min); P23 50/50 of 121 @Howling Fjords (20 gold/min);
  10:00  switches per bot (target/state, since start): P0 14/5; P1 9/0; P2 8/0; P3 12/2; P4 10/0; P6 12/2; P7 11/0; P8 14/2; P9 7/0; P11 12/2; P12 12/2; P15 10/0; P16 16/0; P18 13/2; P22 8/0; P23 12/0;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 1730; P1 1558; P2 2381; P3 3350; P4 2027; P7 1948; P8 2478; P9 2900; P11 2236; P12 2469; P15 1957; P16 3544; P18 2153; P22 1703; P23 2877;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3951 over=634 would-defer=885 deferred=0
  10:00  orders total=12808 near(<2k)=2695 mid(2-8k)=9973 far(>8k)=140 idle-at-target(<300)=7
  20:00  combat 137/663 orders=0 | def-scan 0/304 | production peak 459 | wave-tick 3986/9115 | last gate sweep 4380
  20:00  army smart orders: change=19759 idle-away=1020 rest=7381 refill=1108 advance=347 exit=102 | chains on issued=26792 points=89050 fallback=802 | stalled 9% of 85917
  20:00  groups (food group/need of army @staging): P0 all 11; P1 20/50 of 20 @Redridge Mountains (10 gold/min); P2 107/120 of 131 @Strahnbrad (10 gold/min); P3 50/50 of 288 @Crystalsong Forest (15 gold/min); P4 41/50 of 41 @Fuselight (10 gold/min); P6 30/88 of 30 @Swamp of Sorrows (15 gold/min); P7 50/50 of 180 @Dalaran (30 gold/min); P8 92/119 of 198 @Sulfuron Spire (10 gold/min); P9 all 80; P11 54/50 of 57 @Val'sharah (10 gold/min); P12 49/50 of 188 @Astranaar (15 gold/min); P15 61/50 of 61 @Jaguero Isle (15 gold/min); P16 50/50 of 199 @Orgrimmar (25 gold/min); P18 3/71 of 3 @Grove of the Ancients (10 gold/min); P22 51/50 of 173 @Booty Bay (25 gold/min); P23 53/64 of 64 @Brill (15 gold/min);
  20:00  switches per bot (target/state, since start): P0 33/17; P1 24/10; P2 16/2; P3 24/6; P4 25/2; P6 26/8; P7 17/0; P8 25/4; P9 19/7; P11 25/6; P12 19/2; P15 22/0; P16 30/4; P18 28/12; P22 17/0; P23 18/2;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2742; P1 3210; P2 3918; P3 5875; P4 4228; P7 4037; P8 5473; P9 4605; P11 3796; P12 4765; P15 3028; P16 6658; P18 3445; P22 3551; P23 5062;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=5079 over=1473 would-defer=2021 deferred=0
  20:00  orders total=29717 near(<2k)=8093 mid(2-8k)=20822 far(>8k)=802 idle-at-target(<300)=58
  30:00  combat 165/702 orders=3 | def-scan 12/374 | production peak 459 | wave-tick 3363/9115 | last gate sweep 3362
  30:00  army smart orders: change=24867 idle-away=1682 rest=9797 refill=1789 advance=537 exit=150 | chains on issued=34346 points=117652 fallback=1222 | stalled 11% of 122097
  30:00  groups (food group/need of army @staging): P0 2/50 of 2 @Maelstrom (10 gold/min); P1 40/50 of 44 @Deadwind Pass (15 gold/min); P2 26/50 of 26 @Strahnbrad (10 gold/min); P3 92/92 of 298 @Lordaeron City (30 gold/min); P4 26/50 of 31 @Nethergarde Keep (15 gold/min); P6 26/50 of 34 @-; P7 50/50 of 146 @Dalaran (30 gold/min); P8 47/50 of 199 @Nordrassil (25 gold/min); P9 all 17; P11 all 15; P12 56/88 of 197 @Darkshore (10 gold/min); P15 14/50 of 14 @Blasted Lands (15 gold/min); P16 50/50 of 225 @Nazjatar (10 gold/min); P22 50/50 of 138 @-; P23 73/92 of 73 @Brill (15 gold/min);
  30:00  switches per bot (target/state, since start): P0 37/20; P1 32/12; P2 19/2; P3 31/6; P4 36/6; P6 32/12; P7 20/0; P8 33/8; P9 22/9; P11 37/15; P12 27/2; P15 36/8; P16 42/4; P18 37/23; P22 24/4; P23 24/2;
  30:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2927; P1 4652; P2 5438; P3 8432; P4 6600; P7 6137; P8 8770; P9 5803; P11 5029; P12 7635; P15 4122; P16 10437; P18 3825; P22 5192; P23 7018;
  30:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=7200 max-slot=5079 over=2247 would-defer=3065 deferred=0
  30:00  orders total=38822 near(<2k)=10378 mid(2-8k)=27225 far(>8k)=1219 idle-at-target(<300)=72
  45:00  combat 5/702 orders=0 | def-scan 0/374 | production peak 459 | wave-tick 2837/9115 | last gate sweep 3029
  45:00  army smart orders: change=32401 idle-away=3050 rest=12468 refill=2573 advance=675 exit=310 | chains on issued=45135 points=161355 fallback=1900 | stalled 11% of 173404
  45:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 all 4; P2 25/50 of 29 @Caer Darrow (15 gold/min); P3 65/70 of 257 @Tirisfal Glades (15 gold/min); P4 50/50 of 86 @Gnomeregan (20 gold/min); P6 50/50 of 164 @Duskwood (10 gold/min); P7 5/69 of 5 @Durnholde (15 gold/min); P8 48/50 of 199 @Teldrassil (20 gold/min); P9 51/50 of 51 @Brill (15 gold/min); P11 all 1; P12 50/50 of 197 @Exodar Regalis (15 gold/min); P15 100/145 of 103 @Western Stranglethorn (15 gold/min); P16 50/50 of 200 @Spinebark Grove (10 gold/min); P22 all 108; P23 38/50 of 71 @Silverpine Forest (15 gold/min);
  45:00  switches per bot (target/state, since start): P0 42/20; P1 42/19; P2 24/4; P3 37/11; P4 49/14; P6 51/20; P7 28/6; P8 43/8; P9 31/12; P11 46/21; P12 34/2; P15 52/14; P16 49/4; P18 42/24; P22 32/7; P23 34/6;
  45:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2930; P1 6261; P2 7697; P3 12556; P4 9979; P7 9085; P8 14660; P9 7280; P11 5364; P12 12510; P15 6316; P16 16960; P18 3825; P22 7252; P23 9995;
  45:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=10800 max-slot=5079 over=3387 would-defer=4603 deferred=0
  45:00  orders total=51477 near(<2k)=13224 mid(2-8k)=36366 far(>8k)=1887 idle-at-target(<300)=83
  60:00  combat 186/790 orders=0 | def-scan 10/418 | production peak 459 | wave-tick 29/9115 | last gate sweep 2973
  60:00  army smart orders: change=40392 idle-away=3947 rest=14635 refill=3145 advance=985 exit=379 | chains on issued=54594 points=204642 fallback=2620 | stalled 11% of 231704
  60:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 9/145 of 9 @Southshore (15 gold/min); P2 25/50 of 25 @Caer Darrow (15 gold/min); P3 27/50 of 294 @Brill (15 gold/min); P4 53/98 of 55 @Alterac Mountains (20 gold/min); P6 50/50 of 249 @Northern Badlands (10 gold/min); P7 3/50 of 3 @Tempest Reach (10 gold/min); P8 180/210 of 198 @Nazmir (10 gold/min); P9 all 7; P11 1/50 of 1 @-; P12 50/50 of 197 @Exodar Regalis (15 gold/min); P15 50/50 of 192 @Stormsong Valley (10 gold/min); P16 195/199 of 198 @Nazjatar (10 gold/min); P22 12/50 of 12 @Kezan (15 gold/min); P23 all 157;
  60:00  switches per bot (target/state, since start): P0 46/20; P1 65/38; P2 30/8; P3 44/15; P4 60/20; P6 71/24; P7 44/22; P8 49/8; P9 37/17; P11 51/22; P12 38/2; P15 76/18; P16 53/4; P18 46/24; P22 40/14; P23 50/9;
  60:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2930; P1 7018; P2 9779; P3 16318; P4 14101; P7 10894; P8 20925; P9 9158; P11 5364; P12 17385; P15 9904; P16 23485; P18 3825; P22 8995; P23 13326;
  60:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=14400 max-slot=5079 over=4434 would-defer=6081 deferred=0
  60:00  orders total=63483 near(<2k)=16983 mid(2-8k)=43899 far(>8k)=2601 idle-at-target(<300)=108
  90:00  combat 68/790 orders=4 | def-scan 19/430 | production peak 459 | wave-tick 34/9115 | last gate sweep 2798
  90:00  army smart orders: change=48268 idle-away=6250 rest=20519 refill=3974 advance=1726 exit=501 | chains on issued=70685 points=281937 fallback=3507 | stalled 13% of 347964
  90:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/71 of 2 @Southshore (15 gold/min); P2 28/50 of 37 @Caer Darrow (15 gold/min); P3 46/50 of 256 @Alterac Mountains (20 gold/min); P4 5/50 of 5 @Dun Morogh (15 gold/min); P6 93/113 of 249 @Stromgarde (25 gold/min); P7 1/50 of 1 @Keel Harbor (10 gold/min); P8 112/112 of 225 @Kezan (15 gold/min); P9 7/50 of 7 @Terrordale (1 gold/min); P11 1/50 of 1 @-; P12 49/50 of 200 @Twilight Landing (15 gold/min); P15 39/50 of 262 @Seradane (10 gold/min); P16 112/112 of 195 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 all 157;
  90:00  switches per bot (target/state, since start): P0 53/20; P1 75/44; P2 41/14; P3 53/15; P4 86/40; P6 94/28; P7 57/30; P8 58/8; P9 51/26; P11 58/22; P12 49/2; P15 89/18; P16 63/4; P18 54/24; P22 50/16; P23 69/25;
  90:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2930; P1 8318; P2 13962; P3 25101; P4 18228; P7 11369; P8 33916; P9 11903; P11 5364; P12 27543; P15 19506; P16 36535; P18 3825; P22 9770; P23 23617;
  90:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=21600 max-slot=5079 over=6515 would-defer=8971 deferred=0
  90:00  orders total=81238 near(<2k)=20845 mid(2-8k)=56905 far(>8k)=3488 idle-at-target(<300)=289
 540:00  combat 4/1020 orders=0 | def-scan 0/639 | production peak 473 | wave-tick 33/9115 | last gate sweep 2773
 540:00  army smart orders: change=177014 idle-away=40473 rest=124498 refill=16809 advance=6852 exit=1089 | chains on issued=300344 points=1334949 fallback=46914 | stalled 21% of 2111915
 540:00  groups (food group/need of army @staging): P0 2/50 of 2 @-; P1 2/50 of 2 @-; P2 43/50 of 218 @Terrordale (1 gold/min); P3 all 275; P6 50/50 of 248 @Sylvanaar (10 gold/min); P7 1/50 of 1 @-; P8 50/50 of 211 @Kezan (15 gold/min); P9 3/50 of 3 @-; P11 1/50 of 1 @-; P12 50/50 of 200 @Twilight Landing (15 gold/min); P15 142/147 of 256 @Durnholde (15 gold/min); P16 50/50 of 195 @Spinebark Grove (10 gold/min); P22 1/50 of 1 @-; P23 all 117;
 540:00  switches per bot (target/state, since start): P0 164/20; P1 180/46; P2 156/40; P3 268/118; P4 203/46; P6 608/144; P7 169/32; P8 168/8; P9 174/56; P11 169/22; P12 159/2; P15 288/90; P16 173/4; P18 164/24; P22 160/16; P23 333/195;
 540:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2930; P1 8958; P2 89425; P3 181672; P4 19068; P7 11775; P8 233371; P9 24562; P11 5364; P12 170214; P15 181660; P16 232308; P18 3825; P22 9770; P23 165903;
 540:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=129600 max-slot=5079 over=38535 would-defer=53273 deferred=0
 540:00  orders total=366735 near(<2k)=79317 mid(2-8k)=240523 far(>8k)=46895 idle-at-target(<300)=1326
```

Top tasks by total ops at 540:00 (of 43):
```
task combat every=0.500s runs=64800 last=4 max=3774 sum=49456351
task production every=1.000s runs=32399 last=5 max=473 sum=5362416
task route structures 3 every=15.000s runs=2159 last=1010 max=1046 sum=2081484
task route structures 2 every=15.000s runs=2159 last=732 max=1628 sum=1672292
task route structures 1 every=15.000s runs=2159 last=803 max=1362 sum=1480872
task route portals every=30.000s runs=1079 last=639 max=3278 sum=694551
task route structures 4 every=15.000s runs=2159 last=201 max=784 sum=490176
task hero visits every=15.000s runs=2159 last=0 max=4478 sum=453177
task gate sweep every=15.000s runs=2159 last=0 max=4782 sum=301097
task hero care every=15.000s runs=2159 last=33 max=45 sum=72919
```

Errors (TICK ERROR / FAILED / section errors): 1
- 0:27 TICK ERROR in route creeps: war3map.lua:29279: attempt to perform arithmetic on a boolean value

Bots at 540:00: P0 Warsong atk a4 f0; P1 Stormwind atk a89 f0; P2 Quel'thalas atk a223 f200; P3 Scourge def a294 f250; P4 Ironforge atk a3 f0; P7 Gilneas atk a4 f0; P8 Skywall atk a212 f200; P9 Lordaeron atk a3 f0; P11 Druids atk a3 f0; P12 Black Empire atk a209 f200; P15 Illidan atk a270 f250; P16 Ahn'qiraj atk a223 f200; P18 Sentinels atk a4 f0; P22 Kezan Enterprise atk a1 f20; P23 Legion def a137 f107; P6 Fel Horde atk a260 f250

### v0.46.1 -- 9 dumps, 5:00 .. 40:00 -- 0.46.1, 15 bots + maintainer as Lordaeron on autopilot (multiplayer lobby, 1 human), Veteran, 44 min. Stopwatch: turn 20 at 20:04. Order delay ~2 s felt at turn 10.

Skipped (other build or other game): WLAI_dump_05m00s_1.txt (v0.45.0, 5:00)

**Errors (TICK ERROR / FAILED / section errors): none**

Stopwatch (real time at game turn N; real s per game min between readings): t20 20:04

Bots: 15, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4679 | 1544 | 2255 / 226 | - | North Alliance 32, South Alliance 38, Burning Legion 18, Outland 25, Old Gods 30, Kalimdor 30 |
| 20:00 | 3382 | 1899 | 1530 / 240 | - | North Alliance 26, South Alliance 37, Burning Legion 25, Outland 21, Old Gods 41, Kalimdor 32 |
| 30:00 | 3203 | 1309 | 1383 / 226 | - | North Alliance 32, South Alliance 31, Burning Legion 21, Outland 26, Old Gods 42, Kalimdor 31 |
| 40:00 | 2940 | 1185 | 1230 / 236 | - | North Alliance 30, South Alliance 13, Burning Legion 24, Outland 39, Old Gods 50, Kalimdor 28 |

LOAD at the checkpoints:
```
  10:00  combat 245/401 orders=1 | def-scan 0/178 | production peak 325 | wave-tick 575/3825 | last gate sweep 3617
  10:00  army smart orders: change=9881 idle-away=421 rest=3711 refill=543 advance=0 exit=154 | chains on issued=13651 points=35144 fallback=0 | stalled 3% of 26170
  10:00  groups (food group/need of army @staging): P0 50/50 of 155 @Thunderbluff (20 gold/min); P1 77/77 of 137 @Deadwind Pass (15 gold/min); P2 50/50 of 132 @Terrordale (1 gold/min); P3 50/50 of 139 @Borean Tundra (15 gold/min); P4 50/50 of 123 @Fuselight (10 gold/min); P6 50/50 of 224 @West-Zangarmarsh (15 gold/min); P7 51/50 of 51 @Jintha'Alor  (15 gold/min); P8 50/50 of 183 @Nazmir (10 gold/min); P9 50/50 of 147 @Corin's Crossing (15 gold/min); P11 37/50 of 169 @Northern Ashenvale (10 gold/min); P12 all 120; P15 50/50 of 97 @Eldarath (10 gold/min); P16 37/50 of 143 @Feralas (10 gold/min); P18 all 41; P22 77/77 of 161 @Stranglethorn (15 gold/min); P23 50/50 of 136 @Grizzly Hills (20 gold/min);
  10:00  switches per bot (target/state, since start): P0 20/2; P1 12/0; P2 12/0; P3 13/2; P4 13/0; P6 16/2; P7 15/0; P8 17/2; P9 8/0; P11 15/2; P12 10/1; P15 17/0; P16 13/2; P18 13/2; P22 14/0; P23 15/0;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2075; P1 1420; P2 2265; P3 3365; P4 2009; P6 2918; P7 1830; P8 2543; P11 2339; P12 2420; P15 2010; P16 3440; P18 1997; P22 1928; P23 2779;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 68 <1s 55 <2s 42 <5s 41 <10s 19 stuck 7
  10:00  unit budget 40 + 10/CP (army/budget, capped turns): P0 153/170 c0; P1 140/130 c3; P2 134/130 c8; P3 125/120 c20; P4 124/170 c0; P6 223/190 c6; P7 45/170 c0; P8 183/180 c8; P11 170/140 c11; P12 121/110 c17; P15 100/140 c0; P16 124/130 c11; P18 39/120 c0; P22 165/190 c0; P23 115/130 c3; P9 148/140 c12;
  10:00  guard positions removed: ab [slots P0 P2 P4 P6 P8 P12 P16 P18 P22], on 2019 units; idle-away orders on 9 bots avg 30 | off 7 bots avg 20; rest orders on 9 bots avg 281 | off 7 bots avg 168
  10:00  attack groups: units left out (no way to the target) 0
  10:00  caster cap 35%, caster orders skipped per bot: P0 226; P1 68; P2 295; P3 57; P4 6; P6 2; P7 538; P8 227; P11 72; P12 4; P15 166; P16 62; P18 64; P22 322; P23 24;
  10:00  activity (0.46, sampled army units): walking 85% fighting 7% idle 4% stalled 2% of 31280
  10:00  target returns within 5 min (A->B->A) per bot: P0 1; P1 0; P2 1; P3 0; P4 0; P6 0; P7 0; P8 1; P11 0; P12 1; P15 0; P16 0; P18 1; P22 0; P23 0; P9 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx19 ownx1; P1: Nx12; P2: Nx12; P3: Nx13; P4: Nx13; P6: Nx16; P7: Nx15; P8: Nx17; P11: Nx15; P12: Nx9 ownx1; P15: Nx17; P16: Nx13; P18: Nx12 ownx1; P22: Nx14; P23: Nx15; P9: Nx8;
  10:00  style check (0.46): OK (300 instance fields, separate objects)
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3798 over=577 would-defer=786 deferred=0
  10:00  orders total=15442 near(<2k)=4462 mid(2-8k)=11170 far(>8k)=0 idle-at-target(<300)=32
  20:00  combat 331/835 orders=10 | def-scan 0/466 | production peak 364 | wave-tick 3437/8203 | last gate sweep 3919
  20:00  army smart orders: change=21339 idle-away=1628 rest=7743 refill=1418 advance=49 exit=273 | chains on issued=30179 points=85716 fallback=129 | stalled 5% of 66300
  20:00  groups (food group/need of army @staging): P0 50/50 of 204 @Northern Barrens (10 gold/min); P1 26/140 of 26 @-; P2 39/50 of 64 @Strahnbrad (10 gold/min); P3 50/50 of 182 @Lordaeron City (30 gold/min); P4 48/50 of 72 @Nethergarde Keep (15 gold/min); P6 all 17; P7 34/50 of 34 @Alterac Mountains (20 gold/min); P8 all 202; P9 56/50 of 56 @Corin's Crossing (15 gold/min); P11 all 13; P12 41/50 of 47 @Twilight Landing (15 gold/min); P15 all 61; P16 50/50 of 65 @Desolace (10 gold/min); P18 24/50 of 24 @Grove of the Ancients (10 gold/min); P22 50/50 of 116 @Western Stranglethorn (15 gold/min); P23 38/50 of 115 @Hearthglen (15 gold/min);
  20:00  switches per bot (target/state, since start): P0 32/2; P1 27/8; P2 19/2; P3 30/6; P4 26/0; P6 29/7; P7 26/2; P8 30/7; P9 24/10; P11 34/15; P12 15/2; P15 26/1; P16 25/4; P18 20/7; P22 29/4; P23 30/4;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 4318; P1 3132; P2 3733; P3 6409; P4 3911; P6 5082; P7 4023; P8 5869; P11 3602; P12 4366; P15 3247; P16 5675; P18 3068; P22 4149; P23 4501;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 115 <1s 104 <2s 86 <5s 112 <10s 53 stuck 28
  20:00  unit budget 40 + 10/CP (army/budget, capped turns): P0 203/200 c22; P1 25/120 c9; P2 65/140 c13; P3 193/180 c52; P4 68/190 c0; P6 18/180 c22; P7 23/170 c0; P8 192/200 c22; P11 14/80 c19; P12 46/130 c20; P15 70/110 c14; P16 67/140 c20; P18 17/100 c0; P22 120/180 c11; P23 108/150 c9; P9 52/60 c27;
  20:00  guard positions removed: ab [slots P0 P2 P4 P6 P8 P12 P16 P18 P22], on 4183 units; idle-away orders on 9 bots avg 136 | off 7 bots avg 57; rest orders on 9 bots avg 606 | off 7 bots avg 326
  20:00  attack groups: units left out (no way to the target) 183
  20:00  caster cap 35%, caster orders skipped per bot: P0 274; P1 242; P2 904; P3 81; P4 6; P6 46; P7 1830; P8 727; P11 235; P12 4; P15 405; P16 62; P18 112; P22 713; P23 76; P9 71;
  20:00  activity (0.46, sampled army units): walking 74% fighting 13% idle 7% stalled 4% of 79913
  20:00  target returns within 5 min (A->B->A) per bot: P0 1; P1 7; P2 2; P3 3; P4 3; P6 3; P7 1; P8 3; P11 5; P12 2; P15 1; P16 1; P18 1; P22 5; P23 1; P9 6;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx31 ownx1; P1: P6x10 P15x1 Nx12 ownx4; P2: P3x3 P23x3 Nx13; P3: P7x1 P9x12 Nx15 ownx2; P4: P6x7 P15x3 Nx16; P6: P1x6 Nx20 ownx3; P7: P3x3 P23x4 Nx18 ownx1; P8: P11x7 P18x2 Nx18 ownx3; P11: P8x4 Nx22 ownx8; P12: P11x1 P18x2 P22x1 Nx10 ownx1; P15: P1x3 P22x3 Nx19 ownx1; P16: P0x2 P11x5 P18x4 Nx13 ownx1; P18: P8x2 P12x1 P16x1 Nx14 ownx2; P22: P6x4 P15x7 Nx16 ownx2; P23: P9x10 Nx18 ownx2; P9: P3x5 P23x3 Nx10 ownx6;
  20:00  style check (0.46): OK (300 instance fields, separate objects)
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4591 over=1381 would-defer=1898 deferred=0
  20:00  orders total=34433 near(<2k)=10823 mid(2-8k)=24149 far(>8k)=129 idle-at-target(<300)=161
  30:00  combat 361/835 orders=28 | def-scan 7/466 | production peak 380 | wave-tick 2685/8203 | last gate sweep 2940
  30:00  army smart orders: change=28322 idle-away=3029 rest=10416 refill=2275 advance=49 exit=396 | chains on issued=41996 points=122187 fallback=129 | stalled 5% of 92173
  30:00  groups (food group/need of army @staging): P0 55/81 of 203 @Stonetalon Peak (10 gold/min); P1 6/50 of 6 @Deadwind Pass (15 gold/min); P2 60/70 of 60 @Brill (15 gold/min); P3 all 126; P4 55/50 of 55 @-; P6 43/50 of 88 @Redridge Mountains (10 gold/min); P7 37/62 of 37 @Dalaran (30 gold/min); P8 45/50 of 199 @Shrine to Malorne (10 gold/min); P9 51/75 of 63 @Lordaeron City (30 gold/min); P11 all 15; P12 13/128 of 13 @Twilight Landing (15 gold/min); P15 57/62 of 57 @Duskwood (10 gold/min); P16 50/50 of 81 @Grove of the Ancients (10 gold/min); P22 32/50 of 32 @Drustvar (10 gold/min); P23 23/59 of 23 @Howling Fjords (20 gold/min);
  30:00  switches per bot (target/state, since start): P0 42/6; P1 39/18; P2 25/2; P3 34/11; P4 41/6; P6 46/16; P7 32/6; P8 45/14; P9 28/12; P11 43/21; P12 25/10; P15 36/4; P16 32/4; P18 27/14; P22 35/4; P23 36/8;
  30:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 6900; P1 4252; P2 5569; P3 9436; P4 5991; P6 7248; P7 6235; P8 9863; P11 4126; P12 6478; P15 4547; P16 7967; P18 3457; P22 5863; P23 5911;
  30:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 195 <1s 140 <2s 123 <5s 176 <10s 72 stuck 63
  30:00  unit budget 40 + 10/CP (army/budget, capped turns): P0 196/200 c45; P1 6/80 c9; P2 62/170 c13; P3 88/160 c73; P4 51/190 c0; P6 93/210 c22; P7 29/180 c0; P8 199/200 c39; P11 14/100 c19; P12 18/110 c20; P15 37/130 c14; P16 69/160 c20; P18 0/50 c0; P22 38/160 c11; P23 18/130 c9; P9 67/90 c38;
  30:00  guard positions removed: ab [slots P0 P2 P4 P6 P8 P12 P16 P18 P22], on 6026 units; idle-away orders on 9 bots avg 217 | off 7 bots avg 153; rest orders on 9 bots avg 855 | off 7 bots avg 388
  30:00  attack groups: units left out (no way to the target) 187
  30:00  caster cap 35%, caster orders skipped per bot: P0 274; P1 242; P2 1429; P3 93; P4 6; P6 101; P7 2835; P8 1102; P11 235; P12 4; P15 789; P16 62; P18 112; P22 1108; P23 126; P9 151;
  30:00  activity (0.46, sampled army units): walking 73% fighting 13% idle 7% stalled 4% of 111394
  30:00  target returns within 5 min (A->B->A) per bot: P0 4; P1 15; P2 2; P3 4; P4 9; P6 12; P7 2; P8 6; P11 11; P12 7; P15 2; P16 1; P18 3; P22 6; P23 4; P9 6;
  30:00  targets by owner (since start; N = neutral, own = defending itself): P0: P12x3 P16x2 Nx34 ownx3; P1: P6x17 P15x1 Nx12 ownx9; P2: P3x7 P23x4 Nx14; P3: P7x1 P9x13 Nx15 ownx5; P4: P6x10 P12x4 P15x8 Nx16 ownx3; P6: P1x15 P4x4 Nx20 ownx7; P7: P3x6 P23x5 Nx19 ownx2; P8: P11x10 P18x10 Nx20 ownx5; P11: P8x10 Nx22 ownx11; P12: P11x1 P18x2 P22x7 Nx10 ownx5; P15: P1x5 P4x3 P22x8 Nx19 ownx1; P16: P0x3 P11x6 P18x6 Nx16 ownx1; P18: P8x4 P12x1 P16x1 Nx14 ownx7; P22: P6x4 P12x6 P15x7 Nx16 ownx2; P23: P2x1 P7x3 P9x10 Nx18 ownx4; P9: P3x9 P23x3 Nx10 ownx6;
  30:00  style check (0.46): OK (300 instance fields, separate objects)
  30:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=7200 max-slot=4591 over=2164 would-defer=2992 deferred=0
  30:00  orders total=47525 near(<2k)=14529 mid(2-8k)=33763 far(>8k)=129 idle-at-target(<300)=200
  40:00  combat 194/835 orders=2 | def-scan 32/466 | production peak 380 | wave-tick 2367/8203 | last gate sweep 2602
  40:00  army smart orders: change=34779 idle-away=4044 rest=13102 refill=3087 advance=49 exit=432 | chains on issued=52721 points=153940 fallback=129 | stalled 5% of 113534
  40:00  groups (food group/need of army @staging): P0 55/69 of 204 @Grove of the Ancients (10 gold/min); P1 2/50 of 2 @Zul'Gurub (25 gold/min); P2 49/50 of 52 @Stromgarde (25 gold/min); P3 48/50 of 157 @Tirisfal Glades (15 gold/min); P4 6/65 of 6 @Menethil Harbour (10 gold/min); P6 all 136; P7 35/50 of 35 @Caer Darrow (15 gold/min); P8 50/50 of 198 @Northern Ashenvale (10 gold/min); P9 17/50 of 17 @Lordaeron City (30 gold/min); P11 6/50 of 6 @Auberdine (15 gold/min); P12 41/50 of 44 @Dragonmaw Gates (10 gold/min); P15 50/50 of 105 @Deadwind Pass (15 gold/min); P16 all 41; P22 11/50 of 11 @Southern Highlands (10 gold/min); P23 all 49;
  40:00  switches per bot (target/state, since start): P0 49/12; P1 55/32; P2 34/6; P3 40/13; P4 57/18; P6 62/19; P7 36/10; P8 51/14; P9 31/14; P11 50/26; P12 34/12; P15 54/10; P16 46/11; P18 32/17; P22 48/14; P23 43/9;
  40:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 9798; P1 4899; P2 7576; P3 12173; P4 7551; P6 9726; P7 8454; P8 14075; P11 4515; P12 8677; P15 6999; P16 10335; P18 3491; P22 6867; P23 7273;
  40:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 277 <1s 207 <2s 160 <5s 204 <10s 84 stuck 103
  40:00    0-5m <0.5s 13 <1s 27 <2s 28 <5s 17 <10s 10 stuck 5
  40:00    5-10m <0.5s 55 <1s 28 <2s 14 <5s 24 <10s 9 stuck 2
  40:00    10-15m <0.5s 10 <1s 27 <2s 27 <5s 29 <10s 23 stuck 10
  40:00    15-20m <0.5s 37 <1s 22 <2s 17 <5s 42 <10s 11 stuck 11
  40:00    20-25m <0.5s 41 <1s 17 <2s 21 <5s 42 <10s 6 stuck 12
  40:00    25-30m <0.5s 38 <1s 19 <2s 16 <5s 22 <10s 13 stuck 23
  40:00    30-35m <0.5s 46 <1s 32 <2s 15 <5s 17 <10s 8 stuck 19
  40:00    35-40m <0.5s 37 <1s 35 <2s 22 <5s 11 <10s 4 stuck 21
  40:00  unit budget 40 + 10/CP (army/budget, capped turns): P0 200/200 c56; P1 0/50 c9; P2 54/170 c13; P3 145/180 c73; P4 5/90 c0; P6 136/250 c22; P7 32/170 c0; P8 197/200 c47; P11 7/70 c19; P12 47/160 c20; P15 108/200 c14; P16 39/160 c20; P18 0/40 c0; P22 10/110 c11; P23 39/140 c9;
  40:00  guard positions removed: ab [slots P0 P2 P4 P6 P8 P12 P16 P18 P22], on 7915 units; idle-away orders on 9 bots avg 277 | off 6 bots avg 210; rest orders on 9 bots avg 1099 | off 6 bots avg 460
  40:00  attack groups: units left out (no way to the target) 187
  40:00  caster cap 35%, caster orders skipped per bot: P0 274; P1 242; P2 1849; P3 465; P4 43; P6 101; P7 3850; P8 1727; P11 235; P12 4; P15 1109; P16 62; P18 112; P22 1168; P23 182;
  40:00  activity (0.46, sampled army units): walking 73% fighting 14% idle 7% stalled 4% of 136911
  40:00  target returns within 5 min (A->B->A) per bot: P0 5; P1 23; P2 3; P3 5; P4 14; P6 16; P7 2; P8 6; P11 12; P12 10; P15 4; P16 7; P18 6; P22 14; P23 5;
  40:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x3 P12x3 P16x3 Nx34 ownx6; P1: P6x19 P15x8 Nx12 ownx16; P2: P1x3 P3x9 P4x1 P6x1 P23x4 Nx14 ownx2; P3: P2x3 P7x2 P9x14 Nx16 ownx5; P4: P6x13 P7x5 P12x4 P15x8 Nx16 ownx11; P6: P1x17 P4x16 Nx20 ownx9; P7: P1x3 P3x6 P23x6 Nx19 ownx2; P8: P0x2 P11x10 P18x14 Nx20 ownx5; P11: P8x14 P16x1 Nx22 ownx13; P12: P4x3 P6x1 P11x1 P18x2 P22x11 Nx10 ownx6; P15: P1x19 P4x5 P22x8 Nx19 ownx3; P16: P0x7 P11x12 P18x6 Nx16 ownx5; P18: P8x7 P12x1 P16x1 Nx14 ownx9; P22: P6x4 P12x13 P15x7 Nx16 ownx8; P23: P2x4 P7x5 P9x11 Nx18 ownx5;
  40:00  style check (0.46): OK (300 instance fields, separate objects)
  40:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=9600 max-slot=4591 over=2917 would-defer=4033 deferred=0
  40:00  orders total=59377 near(<2k)=17494 mid(2-8k)=42694 far(>8k)=129 idle-at-target(<300)=214
```

Top tasks by total ops at 40:00 (of 44):
```
task combat every=0.500s runs=4800 last=930 max=3468 sum=2954296
task production every=1.000s runs=2399 last=29 max=380 sum=404480
task hero visits every=15.000s runs=159 last=2330 max=4055 sum=299206
task route structures 1 every=15.000s runs=159 last=882 max=1446 sum=160192
task route structures 2 every=15.000s runs=159 last=825 max=1414 sum=141541
task route portals every=30.000s runs=79 last=560 max=3256 sum=77586
task route structures 3 every=15.000s runs=159 last=518 max=689 sum=73768
task route structures 4 every=15.000s runs=159 last=154 max=693 sum=56892
task route creeps every=15.000s runs=159 last=67 max=1469 sum=45929
task gate sweep every=15.000s runs=159 last=0 max=4265 sum=25927
```

Bots at 40:00: P0 Frostwolf atk a234 f200; P1 Stormwind atk a5 f0; P2 Quel'thalas atk a56 f54; P3 Scourge atk a160 f148; P4 Ironforge atk a13 f4; P6 Fel Horde def a149 f137; P7 Dalaran atk a35 f32; P8 Skywall atk a236 f200; P11 Druids atk a13 f8; P12 Black Empire atk a56 f51; P15 Illidan atk a108 f112; P16 Ahn'qiraj def a96 f39; P18 The Exodar atk a4 f0; P22 Kul'tiras atk a13 f8; P23 Legion def a52 f39

Heartbeat: WLAI heartbeat build=0.46.1 t=43m49s units-on-map=2554 orders/min=1637 orders=smart
  task 43:45 bot income
  task 43:45 latency probe
  task 43:45 destructible gates
  ...

### v0.46.2 -- 16 dumps, 5:00 .. 80:00 -- 0.46.2, 16 bots (maintainer watching), Veteran, 81 min. A/B per phase: gather ab, spread ab, guard ab2, regroup ab. Evil factions won without the 100-gold CPs; first Eye of Sargeras by bots.

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 2 \| 3 | 8 \| 1 | 25 \| 44 | 2 \| 4 | 1 \| 0 | 211 \| 144 | 2 \| 2 |
| 5:00-10:00 | 5 \| 3 | 13 \| 6 | 148 \| 123 | 4 \| 3 | 0 \| 0 | 1240 \| 650 | 46 \| 10 |
| 10:00-15:00 | 3 \| 5 | 76 \| 11 | 197 \| 135 | 4 \| 5 | 0 \| 0 | 1434 \| 1303 | 108 \| 59 |
| 15:00-20:00 | 2 \| 2 | 66 \| 16 | 89 \| 95 | 2 \| 1 | 0 \| 0 | 831 \| 939 | 47 \| 53 |
| 20:00-25:00 | 2 \| 2 | 27 \| 9 | 131 \| 90 | 2 \| 3 | 0 \| 0 | 765 \| 1019 | 45 \| 44 |
| 25:00-30:00 | 3 \| 1 | 28 \| 6 | 146 \| 77 | 1 \| 3 | 0 \| 0 | 890 \| 1357 | 35 \| 118 |
| 30:00-35:00 | 0 \| 1 | 29 \| 8 | 137 \| 97 | 1 \| 1 | 0 \| 0 | 887 \| 1119 | 75 \| 69 |
| 35:00-40:00 | 1 \| 1 | 41 \| 10 | 101 \| 56 | 0 \| 0 | 0 \| 0 | 879 \| 1005 | 86 \| 45 |
| 40:00-45:00 | 0 \| 0 | 27 \| 11 | 122 \| 83 | 1 \| 1 | 0 \| 0 | 829 \| 979 | 110 \| 75 |
| 45:00-50:00 | 0 \| 1 | 35 \| 11 | 96 \| 76 | 0 \| 0 | 0 \| 0 | 884 \| 1234 | 133 \| 136 |
| 50:00-55:00 | 1 \| 0 | 73 \| 14 | 93 \| 51 | 1 \| 1 | 0 \| 0 | 1002 \| 1127 | 175 \| 148 |
| 55:00-60:00 | 1 \| 1 | 26 \| 7 | 96 \| 36 | 0 \| 1 | 1 \| 0 | 992 \| 770 | 198 \| 68 |
| 60:00-65:00 | 0 \| 1 | 27 \| 4 | 116 \| 42 | 1 \| 1 | 0 \| 0 | 783 \| 893 | 123 \| 79 |
| 65:00-70:00 | 0 \| 1 | 35 \| 3 | 70 \| 43 | 1 \| 1 | 0 \| 0 | 716 \| 1022 | 130 \| 78 |
| 70:00-75:00 | 1 \| 1 | 40 \| 3 | 46 \| 42 | 0 \| 0 | 0 \| 0 | 717 \| 815 | 170 \| 27 |
| 75:00-80:00 | 1 \| 0 | 16 \| 2 | 69 \| 25 | 1 \| 1 | 0 \| 0 | 686 \| 901 | 106 \| 40 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4455 | 1156 | 1823 / 160 | - | North Alliance 18, South Alliance 27, Burning Legion 14, Outland 13, Old Gods 27, Kalimdor 29 |
| 20:00 | 3447 | 1809 | 1428 / 208 | - | North Alliance 22, South Alliance 30, Burning Legion 25, Outland 23, Old Gods 35, Kalimdor 30 |
| 30:00 | 3218 | 1312 | 1546 / 247 | - | North Alliance 14, South Alliance 9, Burning Legion 34, Outland 36, Old Gods 62, Kalimdor 11, Kezan Enterprise Pact 1 |
| 45:00 | 3078 | 894 | 1550 / 252 | - | North Alliance 8, South Alliance 2, Burning Legion 44, Outland 42, Old Gods 62, Kalimdor 7, Kezan Enterprise Pact 2 |
| 60:00 | 2767 | 885 | 1484 / 286 | - | North Alliance 2, South Alliance 1, Burning Legion 6, Outland 51, Old Gods 68, Kalimdor 2, Kezan Enterprise Pact 0, Scourge Pact 37 |
| 80:00 | 2290 | 653 | 1119 / 259 | - | North Alliance 2, South Alliance 0, Burning Legion 1, Outland 60, Old Gods 78, Kalimdor 1, Kezan Enterprise Pact 0, Scourge Pact 25 |

LOAD at the checkpoints:
```
  10:00  combat 296/473 orders=66 | def-scan 0/117 | production peak 289 | wave-tick 571/3665 | last gate sweep 3637
  10:00  army smart orders: change=7294 idle-away=215 rest=2719 refill=551 advance=50 exit=117 | chains on issued=9939 points=21884 fallback=34 | stalled 2% of 18624 | staging on route 75, kept 62
  10:00  groups (food group/need of army @staging): P0 50/50 of 140 @Dustwallow Marsh (10 gold/min); P1 all 106; P2 57/57 of 95 @Hinterlands (10 gold/min); P3 56/56 of 160 @En'Kilah (10 gold/min); P4 49/50 of 97 @route node; P6 68/68 of 99 @route node; P7 0/50 of 21 @route node; P8 61/61 of 153 @route node; P9 50/50 of 115 @route node; P11 50/50 of 106 @Northern Ashenvale (10 gold/min); P12 30/50 of 93 @Feathermoon (20 gold/min); P15 50/50 of 93 @route node; P16 67/67 of 113 @Desolace (10 gold/min); P18 all 94; P22 47/50 of 132 @route node; P23 50/50 of 112 @Dragonblight (20 gold/min);
  10:00  switches per bot (target/state, since start): P0 15/0; P1 9/1; P2 9/0; P3 11/2; P4 6/0; P6 6/2; P7 6/0; P8 21/2; P9 7/0; P11 16/2; P12 13/0; P15 13/0; P16 19/4; P18 10/1; P22 10/0; P23 9/0;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 1743; P1 1272; P2 2199; P3 2588; P4 1579; P6 2026; P7 1095; P8 1975; P11 2299; P12 2033; P15 1979; P16 2821; P18 1744; P22 1611; P23 2438;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 57 <1s 43 <2s 41 <5s 35 <10s 18 stuck 9
  10:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 137/136 c13; P1 108/104 c11; P2 97/104 c13; P3 146/112 c20; P4 99/96 c4; P6 97/72 c22; P7 23/48 c0; P8 153/152 c13; P11 117/112 c12; P12 91/88 c20; P15 96/104 c0; P16 111/96 c20; P18 95/104 c1; P22 133/128 c6; P23 83/80 c15; P9 116/112 c16;
  10:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 985 units; idle-away orders on 7 bots avg 21 | off 9 bots avg 7; rest orders on 7 bots avg 173 | off 9 bots avg 167
  10:00  attack groups: units left out (no way to the target) 34
  10:00  caster cap 35%, caster orders skipped per bot: P0 80; P1 44; P2 248; P3 53; P4 2; P7 261; P8 178; P11 75; P12 4; P15 164; P16 2; P18 76; P22 186; P23 22;
  10:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 1 | off 5 bots avg 0; CP captures per bot on 11 bots avg 6 | off 5 bots avg 7
  10:00  activity (0.46, sampled army units): walking 75% fighting 9% idle 13% stalled 2% of 24391
  10:00  target returns within 5 min (A->B->A) per bot: P0 0; P1 0; P2 0; P3 0; P4 0; P6 1; P7 1; P8 0; P11 0; P12 0; P15 0; P16 1; P18 1; P22 0; P23 0; P9 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx15; P1: Nx8 ownx1; P2: Nx9; P3: Nx11; P4: Nx6; P6: Nx6; P7: Nx6; P8: Nx21; P11: P12x1 Nx15; P12: Nx13; P15: Nx13; P16: P0x1 Nx18; P18: Nx9 ownx1; P22: Nx10; P23: Nx9; P9: Nx7;
  10:00  style check (0.46): OK (300 instance fields, separate objects)
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=3734 over=492 would-defer=703 deferred=0
  10:00  orders total=11557 near(<2k)=3189 mid(2-8k)=8509 far(>8k)=34 idle-at-target(<300)=19
  20:00  combat 283/697 orders=0 | def-scan 13/346 | production peak 342 | wave-tick 485/7635 | last gate sweep 3776
  20:00  army smart orders: change=17618 idle-away=1448 rest=6788 refill=1323 advance=50 exit=225 | chains on issued=26043 points=73640 fallback=139 | stalled 4% of 54700 | staging on route 145, kept 115
  20:00  groups (food group/need of army @staging): P0 35/50 of 72 @route node; P1 0/50 of 11 @route node; P2 0/50 of 20 @route node; P3 50/50 of 199 @Tirisfal Glades (15 gold/min); P4 54/81 of 55 @route node; P6 48/50 of 160 @route node; P7 28/81 of 28 @route node; P8 50/50 of 177 @route node; P9 8/50 of 8 @Corin's Crossing (15 gold/min); P11 9/50 of 9 @Moonglade (15 gold/min); P12 23/50 of 23 @route node; P15 50/50 of 124 @route node; P16 65/74 of 104 @route node; P18 0/50 of 30 @Grove of the Ancients (10 gold/min); P22 all 18; P23 68/68 of 171 @route node;
  20:00  switches per bot (target/state, since start): P0 26/0; P1 18/8; P2 14/2; P3 23/10; P4 21/2; P6 22/10; P7 16/4; P8 29/2; P9 21/10; P11 33/10; P12 27/8; P15 26/2; P16 28/4; P18 18/6; P22 19/7; P23 19/4;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 3720; P1 2598; P2 4039; P3 4725; P4 3263; P6 3691; P7 2592; P8 4314; P11 3819; P12 3640; P15 3371; P16 4528; P18 2528; P22 3558; P23 4026;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 132 <1s 144 <2s 83 <5s 69 <10s 36 stuck 22
  20:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 77/192 c17; P1 12/80 c20; P2 21/112 c17; P3 179/144 c53; P4 55/136 c23; P6 160/152 c49; P7 33/104 c0; P8 169/184 c30; P11 6/88 c22; P12 27/88 c30; P15 122/112 c31; P16 105/128 c42; P18 25/80 c1; P22 20/144 c24; P23 130/136 c28; P9 7/80 c31;
  20:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 3459 units; idle-away orders on 7 bots avg 163 | off 9 bots avg 34; rest orders on 7 bots avg 459 | off 9 bots avg 397
  20:00  attack groups: units left out (no way to the target) 34
  20:00  caster cap 35%, caster orders skipped per bot: P0 346; P1 86; P2 794; P3 53; P4 74; P6 33; P7 1436; P8 578; P11 195; P12 4; P15 216; P16 2; P18 96; P22 416; P23 90; P9 75;
  20:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 1 | off 5 bots avg 0; CP captures per bot on 11 bots avg 12 | off 5 bots avg 13
  20:00  activity (0.46, sampled army units): walking 74% fighting 14% idle 7% stalled 4% of 65182
  20:00  target returns within 5 min (A->B->A) per bot: P0 0; P1 3; P2 0; P3 3; P4 4; P6 4; P7 5; P8 0; P11 5; P12 6; P15 1; P16 1; P18 3; P22 0; P23 2; P9 4;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x1 P12x5 P15x1 Nx19; P1: P6x2 Nx11 ownx5; P2: P3x1 P23x1 Nx11 ownx1; P3: P9x2 Nx17 ownx4; P4: P6x7 Nx13 ownx1; P6: Nx18 ownx4; P7: P3x4 Nx10 ownx2; P8: P11x8 Nx21; P11: P8x1 P12x1 P15x2 P16x4 Nx21 ownx4; P12: P11x2 P18x6 P22x1 Nx14 ownx4; P15: P0x1 P22x1 Nx23 ownx1; P16: P0x1 P11x7 P18x2 Nx18; P18: P12x4 P16x1 Nx10 ownx3; P22: Nx15 ownx4; P23: P9x3 Nx14 ownx2; P9: P3x2 P23x3 Nx11 ownx5;
  20:00  style check (0.46): OK (300 instance fields, separate objects)
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4144 over=1284 would-defer=1782 deferred=0
  20:00  orders total=29648 near(<2k)=7745 mid(2-8k)=22228 far(>8k)=139 idle-at-target(<300)=32
  30:00  combat 317/697 orders=0 | def-scan 0/346 | production peak 369 | wave-tick 435/7635 | last gate sweep 2975
  30:00  army smart orders: change=25711 idle-away=1974 rest=10233 refill=1500 advance=97 exit=279 | chains on issued=36591 points=103676 fallback=167 | stalled 5% of 86234 | staging on route 213, kept 167
  30:00  groups (food group/need of army @staging): P0 all 1; P1 all 1; P2 0/50 of 30 @Tranquillien (15 gold/min); P3 57/57 of 197 @route node; P4 all 19; P6 50/50 of 221 @route node; P7 all 26; P8 64/73 of 208 @route node; P9 4/213 of 4 @route node; P11 12/50 of 12 @route node; P12 50/50 of 72 @route node; P15 50/50 of 161 @route node; P16 50/50 of 206 @route node; P18 0/50 of 21 @Silvermyst Island (10 gold/min); P22 0/50 of 13 @Kezan (15 gold/min); P23 47/50 of 226 @route node;
  30:00  switches per bot (target/state, since start): P0 43/15; P1 30/17; P2 20/6; P3 37/10; P4 28/7; P6 38/12; P7 25/13; P8 41/6; P9 28/18; P11 40/12; P12 39/12; P15 46/2; P16 41/4; P18 23/10; P22 32/20; P23 36/6;
  30:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 5245; P1 2950; P2 5314; P3 7330; P4 4949; P6 6203; P7 3648; P8 7431; P11 4357; P12 5351; P15 5756; P16 7111; P18 2915; P22 4420; P23 6720;
  30:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 202 <1s 203 <2s 126 <5s 110 <10s 46 stuck 60
  30:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 0/72 c17; P1 0/40 c20; P2 32/80 c17; P3 173/160 c89; P4 17/112 c23; P6 220/200 c82; P7 21/88 c0; P8 208/200 c39; P11 11/72 c22; P12 74/152 c30; P15 163/160 c62; P16 200/200 c49; P18 21/64 c1; P22 14/48 c24; P23 188/192 c32; P9 4/64 c31;
  30:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 4853 units; idle-away orders on 7 bots avg 218 | off 9 bots avg 49; rest orders on 7 bots avg 736 | off 9 bots avg 564
  30:00  attack groups: units left out (no way to the target) 55
  30:00  caster cap 35%, caster orders skipped per bot: P0 385; P1 86; P2 1223; P3 71; P4 85; P6 63; P7 2420; P8 1103; P11 195; P12 4; P15 311; P16 2; P18 96; P22 477; P23 170; P9 75;
  30:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 1 | off 5 bots avg 0; CP captures per bot on 11 bots avg 15 | off 5 bots avg 19
  30:00  activity (0.46, sampled army units): walking 74% fighting 13% idle 6% stalled 4% of 101365
  30:00  target returns within 5 min (A->B->A) per bot: P0 7; P1 6; P2 3; P3 5; P4 6; P6 6; P7 10; P8 4; P11 6; P12 10; P15 3; P16 2; P18 6; P22 5; P23 3; P9 7;
  30:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x9 P12x5 P15x1 Nx19 ownx9; P1: P6x4 P12x1 Nx13 ownx12; P2: P3x1 P23x5 Nx11 ownx3; P3: P7x2 P9x13 Nx18 ownx4; P4: P6x10 Nx13 ownx5; P6: P1x11 P4x2 Nx20 ownx5; P7: P3x5 P23x3 Nx10 ownx7; P8: P0x7 P11x9 Nx23 ownx2; P11: P8x4 P12x1 P15x2 P16x6 Nx22 ownx5; P12: P4x5 P11x2 P18x6 P22x6 Nx14 ownx6; P15: P0x1 P1x7 P4x3 P22x8 Nx26 ownx1; P16: P0x10 P11x8 P18x5 Nx18; P18: P12x4 P16x4 Nx10 ownx5; P22: P8x1 P12x1 P15x5 Nx15 ownx10; P23: P1x2 P2x5 P7x9 P9x3 Nx14 ownx3; P9: P3x3 P23x5 Nx11 ownx9;
  30:00  style check (0.46): OK (300 instance fields, separate objects)
  30:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=7200 max-slot=4144 over=1992 would-defer=2766 deferred=0
  30:00  orders total=42769 near(<2k)=12295 mid(2-8k)=31076 far(>8k)=167 idle-at-target(<300)=81
  45:00  combat 3/697 orders=0 | def-scan 0/346 | production peak 403 | wave-tick 33/7635 | last gate sweep 2684
  45:00  army smart orders: change=31260 idle-away=2921 rest=14878 refill=2164 advance=343 exit=286 | chains on issued=48718 points=145273 fallback=181 | stalled 6% of 131307 | staging on route 260, kept 209
  45:00  groups (food group/need of army @staging): P0 all 2; P1 0/64 of 1 @-; P2 all 22; P3 all 219; P4 0/137 of 3 @Ironforge (30 gold/min); P6 50/50 of 238 @route node; P7 all 3; P8 50/50 of 204 @route node; P11 3/50 of 3 @Teldrassil (20 gold/min); P12 12/50 of 12 @route node; P15 72/72 of 199 @route node; P16 49/50 of 204 @route node; P18 0/50 of 33 @Silvermyst Island (10 gold/min); P22 0/50 of 29 @route node; P23 96/96 of 226 @route node;
  45:00  switches per bot (target/state, since start): P0 50/19; P1 35/18; P2 25/9; P3 48/15; P4 45/22; P6 49/14; P7 32/19; P8 48/8; P9 37/24; P11 52/20; P12 45/16; P15 57/2; P16 50/4; P18 27/10; P22 40/24; P23 51/6;
  45:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 5708; P1 2950; P2 6920; P3 12232; P4 6222; P6 10830; P7 4968; P8 12975; P11 5266; P12 8234; P15 10437; P16 11624; P18 3440; P22 7019; P23 12032;
  45:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 259 <1s 239 <2s 254 <5s 205 <10s 67 stuck 121
  45:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 0/64 c17; P1 0/40 c20; P2 22/72 c17; P3 191/192 c141; P4 1/56 c23; P6 237/232 c135; P7 0/72 c0; P8 202/200 c88; P11 1/48 c22; P12 17/128 c30; P15 192/184 c111; P16 200/200 c86; P18 33/64 c1; P22 31/56 c28; P23 174/232 c49; P9 0/40 c31;
  45:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 7054 units; idle-away orders on 7 bots avg 315 | off 9 bots avg 78; rest orders on 7 bots avg 1096 | off 9 bots avg 800
  45:00  attack groups: units left out (no way to the target) 55
  45:00  caster cap 35%, caster orders skipped per bot: P0 385; P1 86; P2 1709; P3 89; P4 120; P6 99; P7 2893; P8 1247; P11 269; P12 4; P15 425; P16 2; P18 124; P22 477; P23 238; P9 75;
  45:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 1 | off 5 bots avg 0; CP captures per bot on 11 bots avg 17 | off 5 bots avg 21
  45:00  activity (0.46, sampled army units): walking 75% fighting 11% idle 7% stalled 5% of 154980
  45:00  target returns within 5 min (A->B->A) per bot: P0 8; P1 6; P2 3; P3 6; P4 15; P6 7; P7 13; P8 5; P11 11; P12 11; P15 3; P16 2; P18 6; P22 8; P23 5; P9 10;
  45:00  targets by owner (since start; N = neutral, own = defending itself): P0: P7x2 P8x9 P12x5 P15x1 P16x3 Nx19 ownx11; P1: P6x4 P7x5 P12x1 Nx13 ownx12; P2: P3x4 P23x5 Nx11 ownx5; P3: P2x2 P7x2 P9x19 Nx19 ownx6; P4: P6x15 P15x3 Nx13 ownx14; P6: P1x11 P4x9 P12x1 P23x1 Nx21 ownx6; P7: P3x5 P23x6 Nx10 ownx11; P8: P0x7 P11x12 P22x2 Nx24 ownx3; P11: P8x5 P12x1 P15x2 P16x13 Nx22 ownx9; P12: P4x5 P6x2 P11x2 P18x6 P22x6 P23x2 Nx14 ownx8; P15: P0x1 P1x8 P4x12 P22x8 P23x1 Nx26 ownx1; P16: P0x11 P7x1 P11x14 P18x6 Nx18; P18: P12x4 P16x8 Nx10 ownx5; P22: P8x7 P12x1 P15x5 Nx15 ownx12; P23: P1x2 P2x5 P4x1 P7x15 P9x3 P12x6 Nx16 ownx3; P9: P3x7 P23x5 Nx11 ownx14;
  45:00  style check (0.46): OK (300 instance fields, separate objects)
  45:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=10800 max-slot=4193 over=3059 would-defer=4269 deferred=0
  45:00  orders total=56184 near(<2k)=15676 mid(2-8k)=41435 far(>8k)=181 idle-at-target(<300)=139
  60:00  combat 321/804 orders=0 | def-scan 0/433 | production peak 430 | wave-tick 31/7635 | last gate sweep 2392
  60:00  army smart orders: change=37483 idle-away=4143 rest=18345 refill=2663 advance=453 exit=439 | chains on issued=60408 points=185097 fallback=181 | stalled 8% of 179124 | staging on route 305, kept 245
  60:00  groups (food group/need of army @staging): P0 0/50 of 2 @Maelstrom (10 gold/min); P1 0/50 of 1 @-; P2 0/50 of 3 @-; P3 50/50 of 272 @route node; P4 0/50 of 16 @Gnomeregan (20 gold/min); P6 102/102 of 249 @route node; P7 0/50 of 2 @route node; P8 67/67 of 209 @Kezan (15 gold/min); P11 4/50 of 4 @Teldrassil (20 gold/min); P12 21/50 of 21 @route node; P15 50/50 of 252 @route node; P16 50/50 of 213 @route node; P23 35/64 of 102 @-;
  60:00  switches per bot (target/state, since start): P0 54/20; P1 39/18; P2 34/18; P3 67/18; P4 50/24; P6 58/14; P7 38/24; P8 59/12; P9 42/24; P11 56/20; P12 48/16; P15 63/4; P16 63/4; P18 35/16; P22 50/32; P23 67/18;
  60:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 6011; P1 2950; P2 7731; P3 18380; P4 6593; P6 15914; P7 5396; P8 18554; P11 5603; P12 10859; P15 16122; P16 17055; P18 3662; P22 9474; P23 17662;
  60:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 311 <1s 268 <2s 326 <5s 380 <10s 84 stuck 179
  60:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 0/48 c17; P1 0/40 c20; P2 0/40 c17; P3 247/250 c194; P4 15/48 c23; P6 248/248 c186; P7 0/56 c0; P8 200/200 c135; P11 2/48 c22; P12 29/128 c30; P15 230/240 c160; P16 199/200 c130; P18 0/40 c1; P22 0/40 c29; P23 88/88 c52; P9 0/40 c31;
  60:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 8786 units; idle-away orders on 7 bots avg 449 | off 9 bots avg 110; rest orders on 7 bots avg 1381 | off 9 bots avg 963
  60:00  attack groups: units left out (no way to the target) 55
  60:00  caster cap 35%, caster orders skipped per bot: P0 385; P1 86; P2 1858; P3 89; P4 120; P6 138; P7 2893; P8 1443; P11 269; P12 4; P15 520; P16 2; P18 132; P22 477; P23 395; P9 75;
  60:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 2 | off 5 bots avg 0; CP captures per bot on 11 bots avg 18 | off 5 bots avg 23
  60:00  activity (0.46, sampled army units): walking 73% fighting 10% idle 7% stalled 7% of 211104
  60:00  target returns within 5 min (A->B->A) per bot: P0 9; P1 6; P2 7; P3 8; P4 16; P6 7; P7 15; P8 8; P11 11; P12 11; P15 3; P16 3; P18 7; P22 11; P23 10; P9 10;
  60:00  targets by owner (since start; N = neutral, own = defending itself): P0: P7x2 P8x10 P12x5 P15x1 P16x6 Nx19 ownx11; P1: P6x4 P7x6 P12x1 P23x3 Nx13 ownx12; P2: P3x8 P23x5 Nx11 ownx10; P3: P2x17 P7x2 P9x19 P15x1 P23x2 Nx19 ownx7; P4: P6x16 P15x6 Nx13 ownx15; P6: P1x11 P3x1 P4x10 P12x2 P23x7 Nx21 ownx6; P7: P3x6 P23x9 Nx10 ownx13; P8: P0x8 P11x12 P22x8 Nx26 ownx5; P11: P8x5 P12x1 P15x2 P16x17 Nx22 ownx9; P12: P4x5 P6x4 P11x2 P18x6 P22x6 P23x3 Nx14 ownx8; P15: P0x1 P1x8 P3x2 P4x12 P22x8 P23x5 Nx26 ownx1; P16: P0x14 P7x1 P11x15 P18x14 P23x1 Nx18; P18: P12x4 P16x12 Nx10 ownx9; P22: P8x11 P12x1 P15x5 P16x2 Nx15 ownx16; P23: P1x2 P2x5 P3x1 P4x1 P7x20 P9x3 P12x6 P15x4 Nx16 ownx9; P9: P3x11 P23x6 Nx11 ownx14;
  60:00  style check (0.46): OK (300 instance fields, separate objects)
  60:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=14400 max-slot=4193 over=4151 would-defer=5737 deferred=0
  60:00  orders total=69465 near(<2k)=19880 mid(2-8k)=50815 far(>8k)=181 idle-at-target(<300)=278
  80:00  combat 327/804 orders=0 | def-scan 0/433 | production peak 539 | wave-tick 30/7635 | last gate sweep 1916
  80:00  army smart orders: change=43084 idle-away=5072 rest=21817 refill=3102 advance=502 exit=728 | chains on issued=72650 points=235286 fallback=181 | stalled 9% of 230662 | staging on route 363, kept 271
  80:00  groups (food group/need of army @staging): P0 0/50 of 2 @-; P1 0/50 of 1 @-; P2 0/50 of 3 @-; P3 0/50 of 62 @route node; P4 0/50 of 1 @-; P6 48/50 of 257 @route node; P7 0/50 of 2 @route node; P8 50/50 of 216 @route node; P11 3/50 of 3 @route node; P12 19/50 of 19 @route node; P15 27/50 of 235 @Andorhal (15 gold/min); P16 50/50 of 207 @route node; P23 0/81 of 4 @Nath'raxas Hold (10 gold/min);
  80:00  switches per bot (target/state, since start): P0 60/20; P1 45/18; P2 39/18; P3 86/34; P4 58/26; P6 66/14; P7 45/24; P8 75/12; P9 43/25; P11 59/22; P12 61/26; P15 81/10; P16 80/4; P18 41/16; P22 55/32; P23 92/34;
  80:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 6098; P1 2950; P2 7731; P3 29053; P4 6650; P6 23672; P7 5796; P8 26897; P9 1; P11 6003; P12 14044; P15 26658; P16 25829; P18 3662; P22 9482; P23 19548;
  80:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 357 <1s 316 <2s 463 <5s 594 <10s 118 stuck 247
  80:00    20-25m <0.5s 40 <1s 35 <2s 19 <5s 22 <10s 4 stuck 13
  80:00    25-30m <0.5s 31 <1s 24 <2s 24 <5s 19 <10s 6 stuck 25
  80:00    30-35m <0.5s 17 <1s 11 <2s 39 <5s 38 <10s 2 stuck 23
  80:00    35-40m <0.5s 18 <1s 13 <2s 44 <5s 37 <10s 12 stuck 14
  80:00    40-45m <0.5s 21 <1s 12 <2s 45 <5s 20 <10s 7 stuck 24
  80:00    45-50m <0.5s 25 <1s 6 <2s 39 <5s 48 <10s 6 stuck 15
  80:00    50-55m <0.5s 15 <1s 16 <2s 26 <5s 42 <10s 2 stuck 27
  80:00    55-60m <0.5s 12 <1s 7 <2s 7 <5s 85 <10s 9 stuck 16
  80:00    60-65m <0.5s 15 <1s 10 <2s 22 <5s 50 <10s 12 stuck 28
  80:00    65-70m <0.5s 12 <1s 18 <2s 58 <5s 35 <10s 2 stuck 14
  80:00    70-75m <0.5s 13 <1s 17 <2s 34 <5s 47 <10s 8 stuck 15
  80:00    75-80m <0.5s 7 <1s 3 <2s 23 <5s 81 <10s 12 stuck 11
  80:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 0/40 c17; P1 0/40 c20; P2 0/40 c17; P3 57/240 c195; P4 0/40 c23; P6 257/250 c243; P7 0/56 c0; P8 205/200 c206; P11 1/48 c22; P12 24/112 c30; P15 241/250 c186; P16 200/200 c193; P18 0/40 c1; P22 0/40 c29; P23 1/48 c63; P9 0/40 c31;
  80:00  guard positions removed: ab2 [slots P1 P3 P7 P11 P15 P23 P9], on 11673 units; idle-away orders on 7 bots avg 567 | off 9 bots avg 122; rest orders on 7 bots avg 1682 | off 9 bots avg 1115
  80:00  attack groups: units left out (no way to the target) 55
  80:00  caster cap 35%, caster orders skipped per bot: P0 385; P1 86; P2 1858; P3 1043; P4 120; P6 180; P7 2893; P8 1511; P11 269; P12 4; P15 1185; P16 2; P18 132; P22 477; P23 833; P9 75;
  80:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 2 | off 5 bots avg 0; CP captures per bot on 11 bots avg 21 | off 5 bots avg 26
  80:00  activity (0.46, sampled army units): walking 74% fighting 10% idle 7% stalled 8% of 267794
  80:00  target returns within 5 min (A->B->A) per bot: P0 9; P1 6; P2 7; P3 18; P4 17; P6 9; P7 17; P8 8; P11 12; P12 16; P15 9; P16 4; P18 7; P22 12; P23 17; P9 10;
  80:00  targets by owner (since start; N = neutral, own = defending itself): P0: P7x2 P8x14 P12x5 P15x1 P16x8 Nx19 ownx11; P1: P3x4 P6x4 P7x8 P12x1 P23x3 Nx13 ownx12; P2: P3x8 P8x1 P23x9 Nx11 ownx10; P3: P2x17 P7x9 P9x19 P15x1 P23x6 Nx19 ownx15; P4: P6x18 P7x1 P15x10 Nx13 ownx16; P6: P1x11 P3x1 P4x11 P12x9 P23x7 Nx21 ownx6; P7: P3x13 P23x9 Nx10 ownx13; P8: P0x9 P3x2 P11x13 P22x8 P23x12 Nx26 ownx5; P11: P8x5 P12x1 P15x2 P16x19 Nx22 ownx10; P12: P4x5 P6x9 P11x2 P15x2 P18x6 P22x6 P23x4 Nx14 ownx13; P15: P0x1 P1x8 P3x15 P4x12 P22x8 P23x7 Nx26 ownx4; P16: P0x14 P3x1 P7x1 P11x16 P18x14 P23x15 Nx19; P18: P12x4 P16x18 Nx10 ownx9; P22: P8x16 P12x1 P15x5 P16x2 Nx15 ownx16; P23: P1x2 P2x5 P3x10 P4x1 P7x20 P8x1 P9x3 P12x6 P15x4 P16x7 Nx16 ownx17; P9: P3x11 P23x6 Nx11 ownx14;
  80:00  style check (0.46): OK (300 instance fields, separate objects)
  80:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=19200 max-slot=4235 over=5457 would-defer=7569 deferred=0
  80:00  orders total=82529 near(<2k)=23282 mid(2-8k)=60631 far(>8k)=181 idle-at-target(<300)=312
```

Top tasks by total ops at 80:00 (of 44):
```
task combat every=0.500s runs=9600 last=327 max=4047 sum=6746463
task production every=1.000s runs=4799 last=7 max=539 sum=717067
task route structures 2 every=15.000s runs=319 last=747 max=1214 sum=258721
task route structures 3 every=15.000s runs=319 last=857 max=872 sum=222486
task route structures 1 every=15.000s runs=319 last=212 max=1331 sum=178062
task route portals every=30.000s runs=159 last=447 max=3241 sum=123755
task route structures 4 every=15.000s runs=319 last=24 max=600 sum=107699
task hero visits every=15.000s runs=319 last=0 max=3671 sum=86824
task route creeps every=15.000s runs=319 last=36 max=1448 sum=65041
task gate sweep every=15.000s runs=319 last=0 max=4002 sum=44538
```

Bots at 80:00: P0 Frostwolf atk a5 f0; P1 Stormwind atk a6 f0; P2 Quel'thalas atk a3 f0; P3 Scourge atk a83 f57; P4 Ironforge atk a4 f0; P6 Fel Horde atk a259 f250; P7 Dalaran atk a3 f0; P8 Skywall atk a218 f192; P11 Druids atk a6 f0; P12 Black Empire atk a30 f27; P15 Illidan atk a249 f246; P16 Ahn'qiraj atk a244 f199; P18 The Exodar atk a4 f0; P22 Kezan Enterprise atk a1 f20; P23 Legion atk a4 f2; P9 Lordaeron idle a0 f0

Heartbeat: WLAI heartbeat build=0.46.2 t=81m29s units-on-map=1913 orders/min=325 orders=smart
  task 81:25 latency probe
  task 81:25 early quests
  task 81:25 palace handover
  ...

### v0.48.1 -- 4 dumps, 5:00 .. 20:00 -- 0.48.1, 16 bots, Veteran, 25 min. Tram worked; Legion did not leave Argus (cast never happened -- line cut in the dump); North Alliance patrolled at Tirisfal while waiting; back-and-forth still seen (Sentinels, Druids early; Dalaran, Scourge later). Slightly laggy.

Skipped (other build or other game): WLAI_dump_25m00s_57284c.txt (v0.48.0, 25:00)

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 0 \| 5 | 0 \| 8 | 0 \| 34 | 6 \| 4 | 0 \| 0 | 0 \| 456 | 0 \| 10 |
| 5:00-10:00 | 0 \| 4 | 0 \| 14 | 0 \| 104 | 3 \| 4 | 0 \| 0 | 0 \| 1428 | 0 \| 44 |
| 10:00-15:00 | 0 \| 1 | 0 \| 22 | 0 \| 101 | 1 \| 2 | 0 \| 0 | 0 \| 1440 | 0 \| 75 |
| 15:00-20:00 | 0 \| 1 | 0 \| 17 | 0 \| 116 | 1 \| 2 | 0 \| 0 | 0 \| 988 | 0 \| 72 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4475 | 1718 | 2076 / 180 | - | North Alliance 26, South Alliance 35, Burning Legion 15, Outland 16, Old Gods 24, Kalimdor 38 |
| 20:00 | 3446 | 1826 | 1631 / 232 | - | North Alliance 27, South Alliance 36, Burning Legion 14, Outland 17, Old Gods 32, Kalimdor 33 |

LOAD at the checkpoints:
```
  10:00  combat 216/432 orders=0 | def-scan 5/171 | production peak 296 | wave-tick 305/3708 | last gate sweep 3680
  10:00  army smart orders: change=13400 idle-away=352 rest=2208 refill=534 advance=0 exit=58 | chains on issued=14074 points=34171 fallback=10 | stalled 2% of 30155 | staging on route 96, kept 85 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 0
  10:00  groups (food group/need of army @staging): P0 50/50 of 120 @route node; P1 all 52; P2 all 78; P3 39/55 of 155 @Grizzly Hills (20 gold/min); P4 50/50 of 135 @route node; P6 50/50 of 180 @route node; P7 all 130; P8 50/50 of 146 @Kezan (15 gold/min); P9 all 127; P11 50/50 of 124 @Azsuna (10 gold/min); P12 50/50 of 105 @Twilight Landing (15 gold/min); P15 all 75; P16 50/50 of 97 @route node; P18 50/50 of 129 @-; P22 65/70 of 77 @route node; P23 all 78;
  10:00  switches per bot (target/state, since start): P0 17/0; P1 8/1; P2 6/1; P3 12/2; P4 16/0; P6 17/2; P7 15/3; P8 20/2; P9 13/1; P11 18/4; P12 15/4; P15 14/1; P16 18/0; P18 19/2; P22 15/0; P23 7/1;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2133; P2 2201; P3 2886; P4 1977; P6 2805; P7 1917; P8 1951; P9 3042; P11 2230; P12 2116; P15 1992; P16 2827; P18 2100; P22 1938; P23 2511;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 48 <1s 36 <2s 75 <5s 40 <10s 20 stuck 11
  10:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 165/160 c7; P2 80/80 c21; P3 135/120 c18; P4 135/136 c1; P6 180/136 c14; P7 130/128 c12; P8 145/136 c13; P9 127/120 c19; P11 125/120 c19; P12 108/88 c20; P15 80/72 c10; P16 106/88 c24; P18 124/136 c14; P22 166/152 c3; P23 75/72 c21; P1 95/88 c7;
  10:00  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 22; rest orders on 0 bots avg 0 | off 16 bots avg 138
  10:00  attack groups: units left out (no way to the target) 0
  10:00  caster cap 35%, caster orders skipped per bot: P0 151; P2 146; P3 75; P4 40; P6 4; P7 71; P8 173; P9 22; P11 1; P12 7; P15 101; P16 43; P18 8; P22 229; P23 14; P1 8;
  10:00  regroup ab [slots P0 P2 P3 P4 P6 P7 P16 P18 P22 P23 P1]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 9 | off 5 bots avg 8
  10:00  activity (0.46, sampled army units): walking 88% fighting 7% idle 1% stalled 2% of 32169
  10:00  target returns within 5 min (A->B->A) per bot: P0 0; P2 0; P3 0; P4 0; P6 0; P7 0; P8 1; P9 0; P11 1; P12 0; P15 0; P16 0; P18 0; P22 0; P23 0; P1 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx17; P2: Nx5; P3: Nx12; P4: Nx16; P6: Nx17; P7: Nx13; P8: Nx20; P9: Nx12; P11: Nx17; P12: Nx14 ownx1; P15: Nx13; P16: Nx18; P18: Nx19; P22: Nx15; P23: Nx6; P1: Nx7;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=4437 over=545 would-defer=779 deferred=0
  10:00  orders total=17183 near(<2k)=6110 mid(2-8k)=11297 far(>8k)=10 idle-at-target(<300)=15
  20:00  combat 169/817 orders=0 | def-scan 0/406 | production peak 376 | wave-tick 2999/7558 | last gate sweep 3762
  20:00  army smart orders: change=24314 idle-away=978 rest=5695 refill=2319 advance=30 exit=118 | chains on issued=29521 points=76700 fallback=267 | stalled 4% of 69005 | staging on route 144, kept 138 | defence kept away (other landmass) 119 | defended by the rest (group kept its front) 4
  20:00  groups (food group/need of army @staging): P0 8/50 of 8 @Orgrimmar (25 gold/min); P1 40/50 of 40 @-; P2 50/50 of 76 @route node; P3 50/50 of 136 @Drak'Tharon Keep (30 gold/min); P4 46/50 of 54 @route node; P6 12/50 of 14 @route node; P7 50/50 of 155 @route node; P8 50/50 of 185 @Dustwallow Marsh (10 gold/min); P9 50/50 of 64 @route node; P11 84/102 of 151 @route node; P12 8/50 of 8 @Feathermoon (20 gold/min); P15 5/197 of 5 @route node; P16 55/55 of 69 @route node; P18 30/50 of 86 @Windshear Crossing (10 gold/min); P22 50/50 of 84 @route node; P23 all 77;
  20:00  switches per bot (target/state, since start): P0 34/14; P1 24/2; P2 13/2; P3 27/6; P4 26/0; P6 27/6; P7 26/4; P8 31/4; P9 29/11; P11 25/4; P12 22/8; P15 16/2; P16 24/0; P18 26/4; P22 26/5; P23 11/5;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 3633; P2 3696; P3 5219; P4 3952; P6 4683; P7 3778; P8 4399; P9 4911; P11 3869; P12 3562; P15 3209; P16 4382; P18 3591; P22 4477; P23 3346;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 93 <1s 126 <2s 154 <5s 74 <10s 37 stuck 19
  20:00    0-5m <0.5s 10 <1s 17 <2s 39 <5s 12 <10s 10 stuck 6
  20:00    5-10m <0.5s 37 <1s 19 <2s 36 <5s 28 <10s 10 stuck 5
  20:00    10-15m <0.5s 28 <1s 54 <2s 36 <5s 12 <10s 6 stuck 5
  20:00    15-20m <0.5s 18 <1s 36 <2s 43 <5s 21 <10s 11 stuck 3
  20:00  unit budget 40 + 8/CP (army/budget, capped turns): P0 8/104 c7; P2 81/112 c41; P3 119/120 c48; P4 54/160 c14; P6 17/136 c28; P7 152/144 c44; P8 184/192 c37; P9 63/64 c42; P11 149/136 c54; P12 13/80 c25; P15 11/80 c30; P16 46/104 c44; P18 88/144 c38; P22 170/160 c35; P23 74/72 c46; P1 38/88 c15;
  20:00  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 61; rest orders on 0 bots avg 0 | off 16 bots avg 355
  20:00  attack groups: units left out (no way to the target) 662
  20:00  caster cap 35%, caster orders skipped per bot: P0 200; P2 482; P3 75; P4 63; P6 70; P7 71; P8 448; P9 110; P11 1; P12 7; P15 226; P16 67; P18 24; P22 298; P23 22; P1 164;
  20:00  regroup ab [slots P0 P2 P3 P4 P6 P7 P16 P18 P22 P23 P1]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 11 | off 5 bots avg 12
  20:00  activity (0.46, sampled army units): walking 79% fighting 13% idle 2% stalled 4% of 74873
  20:00  target returns within 5 min (A->B->A) per bot: P0 10; P2 0; P3 2; P4 1; P6 0; P7 0; P8 2; P9 3; P11 1; P12 3; P15 0; P16 0; P18 1; P22 3; P23 1; P1 1;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x4 P12x2 P16x4 Nx17 ownx7; P2: P3x2 P23x2 Nx8; P3: P9x10 Nx15 ownx2; P4: P6x5 P15x4 Nx17; P6: P1x1 P4x1 P22x1 Nx21 ownx2; P7: P3x3 P23x2 Nx19; P8: P0x10 Nx20 ownx1; P9: P3x4 P23x1 Nx16 ownx6; P11: P12x4 P16x1 Nx19; P12: P18x5 Nx14 ownx3; P15: P22x2 Nx13; P16: P0x5 P18x1 Nx18; P18: P12x4 P16x1 Nx20 ownx1; P22: P6x6 P15x2 Nx17; P23: P9x2 Nx6 ownx1; P1: P6x8 P15x6 Nx9;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4588 over=1285 would-defer=1825 deferred=0
  20:00  orders total=35447 near(<2k)=11819 mid(2-8k)=23762 far(>8k)=267 idle-at-target(<300)=55
```

Top tasks by total ops at 20:00 (of 45):
```
task combat every=0.500s runs=2400 last=697 max=1897 sum=1022006
task production every=1.000s runs=1199 last=76 max=376 sum=207469
task route portals every=30.000s runs=39 last=3491 max=4588 sum=162469
task route structures 2 every=15.000s runs=79 last=1070 max=1413 sum=82921
task route structures 1 every=15.000s runs=79 last=893 max=1244 sum=67726
task hero visits every=15.000s runs=79 last=2893 max=3538 sum=64013
task route creeps every=15.000s runs=79 last=113 max=1438 sum=38414
task route structures 4 every=15.000s runs=79 last=543 max=682 sum=37315
task route structures 3 every=15.000s runs=79 last=387 max=623 sum=33246
task gate sweep every=15.000s runs=79 last=0 max=4006 sum=14281
```

Bots at 20:00: P0 Frostwolf atk a9 f9; P2 Quel'thalas atk a122 f87; P3 Scourge atk a182 f123; P4 Ironforge atk a62 f55; P6 Fel Horde atk a22 f17; P7 Gilneas atk a182 f152; P8 Skywall atk a232 f189; P9 Scarlet Crusade atk a71 f74; P11 Druids atk a160 f139; P12 Black Empire atk a23 f13; P15 Illidan atk a18 f12; P16 Ahn'qiraj atk a136 f44; P18 Sentinels atk a109 f90; P22 Kul'tiras atk a181 f161; P23 Legion hold a77 f74; P1 Stormwind atk a45 f39

Heartbeat: WLAI heartbeat build=0.48.1 t=22m59s units-on-map=2947 orders/min=1135 orders=smart
  task 22:55 latency probe
  task 22:55 early quests
  task 22:55 palace handover
  ...

### v0.48.2 -- 5 dumps, 5:00 .. 25:00 -- 0.48.2, 16 bots (P4 Ironforge on autopilot from 1:42), Veteran, ~26 min (heartbeat 25:39); no stopwatch. Back-and-forth analysis (tools/ai/back_forth.py, 0.48.3): 501 target changes; turn-backs > 120 deg (target-to-target, CP targets only) = neutral hops in the first 10 min (Sentinels, Druids, Legion, Ironforge), defence in/out after 10 min (42 defence episodes, 24 ended at the 30-s minimum, 15 of 31 re-entries within 60 s), route re-picks between enemy targets. Unit-level movement not in these dumps -> 0.48.3 TURNAROUNDS + TRAIL.

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 0 \| 5 | 0 \| 7 | 0 \| 31 | 5 \| 4 | 0 \| 0 | 0 \| 411 | 0 \| 9 |
| 5:00-10:00 | 0 \| 4 | 0 \| 14 | 0 \| 87 | 4 \| 3 | 0 \| 0 | 0 \| 1214 | 0 \| 30 |
| 10:00-15:00 | 0 \| 1 | 0 \| 26 | 0 \| 58 | 2 \| 2 | 0 \| 0 | 0 \| 1272 | 0 \| 75 |
| 15:00-20:00 | 0 \| 2 | 0 \| 23 | 0 \| 100 | 1 \| 2 | 0 \| 0 | 0 \| 1022 | 0 \| 51 |
| 20:00-25:00 | 0 \| 1 | 0 \| 14 | 0 \| 95 | 1 \| 2 | 0 \| 0 | 0 \| 818 | 0 \| 30 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4242 | 1463 | 1891 / 180 | - | North Alliance 26, South Alliance 33, Burning Legion 18, Outland 16, Old Gods 26, Kalimdor 36 |
| 20:00 | 3463 | 1857 | 1658 / 171 | - | North Alliance 28, South Alliance 35, Burning Legion 19, Outland 18, Old Gods 32, Kalimdor 33 |
| 25:00 | 3056 | 1345 | 1304 / 150 | - | North Alliance 30, South Alliance 28, Burning Legion 17, Outland 25, Old Gods 33, Kalimdor 32 |

LOAD at the checkpoints:
```
  10:00  combat 165/374 orders=107 | def-scan 0/193 | production peak 282 | wave-tick 415/3659 | last gate sweep 3626
  10:00  army smart orders: change=11148 idle-away=347 rest=1892 refill=586 advance=0 exit=38 | chains on issued=12713 points=29939 fallback=40 | stalled 2% of 26011 | staging on route 97, kept 86 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 2
  10:00  groups (food group/need of army @staging): P0 50/50 of 82 @-; P1 50/50 of 111 @route node; P2 all 67; P3 50/50 of 117 @route node; P4 50/50 of 108 @Northern Badlands (10 gold/min); P6 all 151; P7 all 117; P8 50/50 of 107 @Kezan (15 gold/min); P9 all 98; P11 50/50 of 121 @route node; P12 50/50 of 82 @Twilight Landing (15 gold/min); P15 all 56; P16 all 81; P18 59/50 of 59 @Desolace (10 gold/min); P22 53/50 of 54 @Southern Highlands (10 gold/min); P23 all 51;
  10:00  switches per bot (target/state, since start): P0 18/2; P1 13/0; P2 8/1; P3 11/2; P4 12/0; P6 18/3; P7 12/1; P8 17/2; P9 13/1; P11 17/3; P12 13/2; P15 13/1; P16 16/1; P18 20/2; P22 16/0; P23 13/1;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2167; P1 1372; P2 2199; P3 2746; P6 2825; P7 1978; P8 1942; P9 2961; P11 2208; P12 2106; P15 1923; P16 2879; P18 2100; P22 2020; P23 2909;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 42 <1s 53 <2s 60 <5s 40 <10s 18 stuck 12
  10:00  unit budget 30 + 7/CP (army/budget, capped turns): P0 133/135 c16; P1 115/93 c15; P2 69/65 c21; P3 102/93 c23; P6 150/114 c20; P7 117/114 c13; P8 107/107 c19; P9 98/93 c23; P11 123/93 c23; P12 86/79 c24; P15 61/58 c11; P16 111/86 c25; P18 108/107 c19; P22 140/135 c13; P23 97/93 c16; P4 111/93 c9;
  10:00  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 21; rest orders on 0 bots avg 0 | off 16 bots avg 118
  10:00  attack groups: units left out (no way to the target) 10
  10:00  caster cap 35%, caster orders skipped per bot: P0 57; P1 32; P2 148; P3 27; P7 116; P8 96; P11 16; P12 4; P15 93; P18 2; P22 153; P23 16;
  10:00  regroup ab [slots P0 P1 P2 P3 P6 P7 P16 P18 P22 P23 P4]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 9 | off 5 bots avg 7
  10:00  activity (0.46, sampled army units): walking 86% fighting 9% idle 2% stalled 2% of 28100
  10:00  target returns within 5 min (A->B->A) per bot: P0 0; P1 0; P2 0; P3 0; P6 0; P7 0; P8 1; P9 0; P11 1; P12 0; P15 0; P16 0; P18 0; P22 0; P23 0; P4 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx17 ownx1; P1: Nx13; P2: Nx7; P3: Nx11; P6: Nx17; P7: Nx11; P8: Nx17; P9: Nx12; P11: Nx16 ownx1; P12: Nx13; P15: Nx12; P16: Nx15 ownx1; P18: P12x1 Nx19; P22: Nx16; P23: Nx12; P4: Nx12;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=4194 over=516 would-defer=684 deferred=0
  10:00  orders total=14634 near(<2k)=4100 mid(2-8k)=10771 far(>8k)=40 idle-at-target(<300)=20
  20:00  combat 182/796 orders=0 | def-scan 0/358 | production peak 441 | wave-tick 3293/6576 | last gate sweep 3205
  20:00  army smart orders: change=23988 idle-away=1124 rest=4426 refill=2073 advance=1 exit=139 | chains on issued=27498 points=72161 fallback=734 | stalled 4% of 62714 | staging on route 168, kept 128 | defence kept away (other landmass) 37 | defended by the rest (group kept its front) 19
  20:00  groups (food group/need of army @staging): P0 all 5; P1 21/50 of 21 @route node; P2 54/54 of 76 @route node; P3 25/50 of 93 @route node; P4 98/92 of 98 @Northern Badlands (10 gold/min); P6 50/50 of 126 @route node; P7 54/54 of 151 @Silverpine Forest (15 gold/min); P8 45/50 of 120 @route node; P9 53/50 of 53 @Hearthglen (15 gold/min); P11 45/50 of 127 @route node; P12 all 10; P15 27/50 of 27 @route node; P16 66/67 of 107 @route node; P18 33/50 of 120 @route node; P22 50/50 of 135 @route node; P23 29/50 of 29 @route node;
  20:00  switches per bot (target/state, since start): P0 29/11; P1 26/6; P2 17/6; P3 25/8; P4 27/4; P6 25/10; P7 20/2; P8 25/6; P9 31/11; P11 26/6; P12 22/7; P15 19/6; P16 24/6; P18 26/4; P22 29/6; P23 20/2;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 3829; P1 2843; P2 3785; P3 4630; P6 4764; P7 4294; P8 4017; P9 4238; P11 3702; P12 3562; P15 3168; P16 4739; P18 3630; P22 4502; P23 4785;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 91 <1s 123 <2s 114 <5s 82 <10s 36 stuck 29
  20:00  unit budget 30 + 7/CP (army/budget, capped turns): P0 9/100 c19; P1 25/79 c22; P2 76/86 c32; P3 115/93 c55; P6 129/107 c54; P7 145/142 c44; P8 119/142 c30; P9 61/58 c53; P11 125/100 c55; P12 16/72 c30; P15 81/72 c41; P16 106/100 c60; P18 121/121 c51; P22 131/142 c44; P23 33/100 c24; P4 100/114 c41;
  20:00  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 70; rest orders on 0 bots avg 0 | off 16 bots avg 276
  20:00  attack groups: units left out (no way to the target) 447
  20:00  caster cap 35%, caster orders skipped per bot: P0 125; P1 127; P2 673; P3 27; P6 25; P7 162; P8 696; P9 12; P11 49; P12 4; P15 158; P18 10; P22 268; P23 54; P4 11;
  20:00  regroup ab [slots P0 P1 P2 P3 P6 P7 P16 P18 P22 P23 P4]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 12 | off 5 bots avg 11
  20:00  activity (0.46, sampled army units): walking 80% fighting 14% idle 1% stalled 3% of 67409
  20:00  target returns within 5 min (A->B->A) per bot: P0 4; P1 2; P2 3; P3 3; P6 1; P7 1; P8 4; P9 4; P11 3; P12 4; P15 1; P16 3; P18 2; P22 0; P23 1; P4 0;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x2 P16x3 Nx17 ownx7; P1: P6x6 P15x4 Nx13 ownx3; P2: P3x4 P23x1 Nx8 ownx3; P3: P9x7 Nx14 ownx4; P6: P1x1 P22x3 Nx17 ownx1; P7: P3x4 P23x2 Nx13; P8: P0x6 Nx17 ownx2; P9: P3x7 P23x3 Nx15 ownx4; P11: P8x1 P12x6 P16x1 Nx16 ownx2; P12: P11x2 P18x4 Nx13 ownx3; P15: P1x4 Nx12 ownx1; P16: P0x6 Nx15 ownx3; P18: P12x5 P16x1 Nx19 ownx1; P22: P6x4 P15x3 Nx20; P23: P2x1 P9x3 Nx15; P4: P6x6 P15x4 Nx15;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4312 over=1291 would-defer=1744 deferred=0
  20:00  orders total=33207 near(<2k)=10330 mid(2-8k)=22628 far(>8k)=733 idle-at-target(<300)=31
  25:00  combat 257/796 orders=2 | def-scan 29/358 | production peak 441 | wave-tick 2740/6576 | last gate sweep 3101
  25:00  army smart orders: change=27381 idle-away=1348 rest=5950 refill=2993 advance=1 exit=207 | chains on issued=33578 points=91033 fallback=736 | stalled 4% of 75805 | staging on route 184, kept 158 | defence kept away (other landmass) 278 | defended by the rest (group kept its front) 20
  25:00  groups (food group/need of army @staging): P0 0/50 of 12 @Eldarath (10 gold/min); P1 all 13; P2 50/50 of 77 @route node; P3 11/50 of 11 @Zul'Drak (20 gold/min); P4 19/50 of 19 @Blackrock Depths (20 gold/min); P6 48/50 of 69 @route node; P7 50/50 of 149 @Andorhal (15 gold/min); P8 68/75 of 95 @Ratchet (10 gold/min); P9 49/50 of 75 @route node; P11 116/105 of 116 @route node; P12 12/50 of 12 @route node; P15 55/54 of 100 @Deadwind Pass (15 gold/min); P16 40/51 of 73 @Thunderbluff (20 gold/min); P18 all 74; P22 all 108; P23 37/85 of 37 @route node;
  25:00  switches per bot (target/state, since start): P0 35/16; P1 33/13; P2 22/6; P3 27/8; P4 29/4; P6 28/10; P7 28/2; P8 31/8; P9 38/11; P11 29/6; P12 25/8; P15 29/10; P16 28/8; P18 29/5; P22 32/9; P23 21/4;
  25:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 4209; P1 3273; P2 4660; P3 5351; P6 5611; P7 5543; P8 5227; P9 4707; P11 4450; P12 4139; P15 3960; P16 5680; P18 4399; P22 5580; P23 5621;
  25:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 136 <1s 169 <2s 130 <5s 100 <10s 43 stuck 33
  25:00    0-5m <0.5s 4 <1s 20 <2s 40 <5s 18 <10s 5 stuck 6
  25:00    5-10m <0.5s 38 <1s 33 <2s 20 <5s 22 <10s 13 stuck 6
  25:00    10-15m <0.5s 21 <1s 33 <2s 26 <5s 20 <10s 9 stuck 7
  25:00    15-20m <0.5s 28 <1s 37 <2s 28 <5s 22 <10s 9 stuck 10
  25:00    20-25m <0.5s 44 <1s 46 <2s 16 <5s 18 <10s 7 stuck 4
  25:00  unit budget 30 + 7/CP (army/budget, capped turns): P0 11/72 c19; P1 16/51 c22; P2 76/86 c32; P3 13/86 c58; P6 128/135 c67; P7 149/142 c62; P8 94/142 c30; P9 76/72 c63; P11 115/107 c70; P12 16/65 c30; P15 101/100 c45; P16 67/114 c65; P18 75/135 c56; P22 106/128 c48; P23 39/93 c24; P4 16/107 c41;
  25:00  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 84; rest orders on 0 bots avg 0 | off 16 bots avg 371
  25:00  attack groups: units left out (no way to the target) 451
  25:00  caster cap 35%, caster orders skipped per bot: P0 125; P1 140; P2 988; P3 81; P6 69; P7 185; P8 1080; P9 28; P11 69; P12 4; P15 356; P18 42; P22 498; P23 66; P4 11;
  25:00  regroup ab [slots P0 P1 P2 P3 P6 P7 P16 P18 P22 P23 P4]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 13 | off 5 bots avg 13
  25:00  activity (0.46, sampled army units): walking 79% fighting 15% idle 1% stalled 3% of 81665
  25:00  target returns within 5 min (A->B->A) per bot: P0 7; P1 5; P2 4; P3 3; P6 1; P7 3; P8 6; P9 5; P11 3; P12 6; P15 3; P16 3; P18 3; P22 0; P23 1; P4 0;
  25:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x4 P16x4 Nx17 ownx10; P1: P6x6 P15x7 Nx13 ownx7; P2: P3x5 P23x5 Nx8 ownx3; P3: P2x1 P9x7 Nx15 ownx4; P6: P1x1 P22x6 Nx17 ownx1; P7: P3x7 P23x7 Nx13; P8: P0x8 P18x3 Nx17 ownx3; P9: P3x10 P23x7 Nx15 ownx4; P11: P8x1 P12x7 P16x3 Nx16 ownx2; P12: P11x3 P18x6 Nx13 ownx3; P15: P1x9 P22x3 Nx12 ownx3; P16: P0x8 P11x1 Nx15 ownx4; P18: P8x1 P12x5 P16x2 Nx19 ownx2; P22: P6x4 P15x3 Nx20 ownx2; P23: P2x1 P9x3 Nx15 ownx1; P4: P6x6 P15x6 Nx15;
  25:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=6000 max-slot=4312 over=1633 would-defer=2192 deferred=0
  25:00  orders total=39933 near(<2k)=11863 mid(2-8k)=27850 far(>8k)=733 idle-at-target(<300)=42
```

Top tasks by total ops at 25:00 (of 44):
```
task combat every=0.500s runs=3000 last=777 max=1688 sum=1252955
task production every=1.000s runs=1499 last=43 max=441 sum=245638
task route portals every=30.000s runs=49 last=3052 max=4312 sum=187459
task hero visits every=15.000s runs=99 last=2587 max=3317 sum=125984
task route structures 2 every=15.000s runs=99 last=1031 max=1325 sum=95726
task route structures 1 every=15.000s runs=99 last=540 max=1151 sum=75272
task route structures 4 every=15.000s runs=99 last=455 max=670 sum=48625
task route structures 3 every=15.000s runs=99 last=471 max=619 sum=43411
task route creeps every=15.000s runs=99 last=67 max=1438 sum=37295
task gate sweep every=15.000s runs=99 last=0 max=3749 sum=16514
```

Bots at 25:00: P0 Frostwolf atk a13 f12; P1 Stormwind def a16 f10; P2 Quel'thalas atk a81 f79; P3 Scourge atk a23 f10; P6 Fel Horde atk a150 f140; P7 Gilneas atk a149 f149; P8 Skywall atk a116 f93; P9 Scarlet Crusade atk a76 f76; P11 Druids atk a135 f112; P12 Black Empire atk a26 f16; P15 Illidan atk a96 f85; P16 Ahn'qiraj atk a129 f68; P18 Sentinels def a86 f75; P22 Kul'tiras def a133 f95; P23 Legion atk a54 f41; P4 Ironforge atk a21 f18

Heartbeat: WLAI heartbeat build=0.48.2 t=25m39s units-on-map=2711 orders/min=1310 orders=smart
  task 25:35 one-shots
  task 25:35 latency probe
  task 25:35 combat
  ...

### v0.48.3 -- 3 dumps, 5:00 .. 8:25 -- 0.48.3 (measurement build), 16 bots (P12 Black Empire on autopilot from 1:04), Veteran, 8.5 min (two manual dumps at 8:04 / 8:25 while watching the Druids, -track). TRAIL: every followed Druid unit walked 28,000-52,000 and stood within 80 of the spot where it was TRAINED at 8:25 -- the engine's guard position (units out of orders walk back ~15-25 s later with an attack order; the bot sees them as busy). TURNAROUNDS (all bots): change 2168, engine 1747, rest 412, whole 141 of ~38,000 walks. Druids' target changes: re-plan switches to targets behind the group (Felwood <-> Northern Ashenvale), creep camps at home. -> 0.48.4: guard positions removed for all bots, commit on re-plan.

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 0 \| 5 | 0 \| 7 | 0 \| 23 | 5 \| 4 | 0 \| 0 | 0 \| 357 | 0 \| 10 |
| 5:00-8:04 | 0 \| 2 | 0 \| 3 | 0 \| 60 | 3 \| 2 | 0 \| 0 | 0 \| 665 | 0 \| 12 |
| 8:04-8:25 | 0 \| 1 | 0 \| 1 | 0 \| 8 | 0 \| 0 | 0 \| 0 | 0 \| 93 | 0 \| 1 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 8:25 | 4114 | 1247 | 1608 / 141 | - | North Alliance 25, South Alliance 28, Burning Legion 16, Outland 18, Old Gods 22, Kalimdor 32 |

LOAD at the checkpoints:
```
   8:25  combat 199/356 orders=0 | def-scan 0/264 | production peak 266 | wave-tick 217/3491 | last gate sweep 3461
   8:25  army smart orders: change=8020 idle-away=189 rest=1468 refill=477 advance=0 exit=28 | chains on issued=9135 points=20225 fallback=6 | stalled 2% of 17850 | staging on route 82, kept 68 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 2
   8:25  groups (food group/need of army @staging): P0 50/50 of 135 @route node; P1 50/50 of 62 @route node; P2 all 71; P3 48/55 of 117 @Storm Peaks (15 gold/min); P4 50/50 of 98 @Loch Modan (10 gold/min); P6 50/50 of 125 @route node; P7 all 36; P8 50/50 of 127 @route node; P9 50/50 of 98 @route node; P11 50/50 of 104 @-; P12 50/50 of 75 @route node; P15 47/50 of 57 @route node; P16 50/50 of 90 @route node; P18 all 95; P22 49/61 of 140 @route node; P23 52/52 of 110 @route node;
   8:25  switches per bot (target/state, since start): P0 15/2; P1 9/0; P2 6/1; P3 10/2; P4 11/0; P6 14/2; P7 14/3; P8 14/2; P9 12/0; P11 13/2; P12 9/0; P15 11/0; P16 14/2; P18 16/3; P22 15/0; P23 12/2;
   8:25  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 1626; P1 959; P2 1782; P3 2236; P4 1495; P6 2242; P7 1402; P8 1486; P9 2397; P11 1848; P15 1539; P16 2356; P18 1604; P22 1544; P23 2181;
   8:25  order latency (0.46, game time from order to moving, idle units): all <0.5s 30 <1s 47 <2s 35 <5s 34 <10s 19 stuck 9
   8:25    0-5m <0.5s 10 <1s 25 <2s 25 <5s 21 <10s 5 stuck 5
   8:25  unit budget 30 + 7/CP (army/budget, capped turns): P0 136/121 c14; P1 65/58 c12; P2 73/65 c14; P3 96/93 c17; P4 95/100 c0; P6 126/114 c13; P7 36/107 c0; P8 126/107 c13; P9 99/93 c17; P11 106/93 c18; P15 65/72 c0; P16 80/79 c20; P18 103/100 c7; P22 136/128 c4; P23 74/72 c14; P12 80/58 c14;
   8:25  guard positions removed: off [slots none], on 0 units; idle-away orders on 0 bots avg 0 | off 16 bots avg 11; rest orders on 0 bots avg 0 | off 16 bots avg 91
   8:25  attack groups: units left out (no way to the target) 0
   8:25  caster cap 35%, caster orders skipped per bot: P0 47; P1 24; P2 130; P3 36; P6 2; P7 416; P8 108; P9 4; P11 16; P15 126; P18 36; P22 120; P23 16;
   8:25  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P22 P23]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 8 | off 5 bots avg 6
   8:25  activity (0.46, sampled army units): walking 86% fighting 9% idle 1% stalled 1% of 19129
   8:25  target returns within 5 min (A->B->A) per bot: P0 0; P1 0; P2 0; P3 0; P4 0; P6 0; P7 0; P8 1; P9 0; P11 1; P15 0; P16 0; P18 0; P22 0; P23 0; P12 0;
   8:25  targets by owner (since start; N = neutral, own = defending itself): P0: Nx14 ownx1; P1: Nx9; P2: Nx5; P3: Nx10; P4: Nx11; P6: Nx14; P7: Nx12; P8: Nx14; P9: Nx12; P11: Nx13; P15: Nx11; P16: Nx14; P18: Nx15 ownx1; P22: Nx15; P23: Nx11; P12: Nx9;
   8:25  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2021 max-slot=4123 over=392 would-defer=553 deferred=0
   8:25  orders total=10495 near(<2k)=3125 mid(2-8k)=7565 far(>8k)=6 idle-at-target(<300)=19
```

Top tasks by total ops at 8:25 (of 44):
```
task combat every=0.500s runs=1010 last=199 max=1391 sum=326794
task route portals every=30.000s runs=16 last=4104 max=4123 sum=63753
task production every=1.000s runs=505 last=141 max=266 sum=52297
task route creeps every=15.000s runs=32 last=365 max=1450 sum=31367
task route structures 2 every=15.000s runs=33 last=998 max=998 sum=21045
task route structures 1 every=15.000s runs=33 last=875 max=875 sum=17839
task route structures 4 every=15.000s runs=32 last=503 max=503 sum=8658
task route structures 3 every=15.000s runs=32 last=475 max=475 sum=8511
task gate sweep every=15.000s runs=33 last=0 max=3461 sum=6271
task starter kit every=15.000s runs=33 last=0 max=1398 sum=1621
```

Turnarounds at 8:25 (0.48.3; per cause, all bots; P lines: turns / walks):
```
total: change 2168 engine 1747 rest 412 whole 141 portal 76 idle-away 50 def-rest 28 defend 6 exit 4 refill 3
phase 0-5m: change 723 rest 38 idle-away 19 refill 3 whole 23 portal 43 engine 462
phase 5-10m: change 1445 rest 374 idle-away 31 exit 4 defend 6 whole 118 def-rest 28 portal 33 engine 1285
P0 456/3616 swaps 103>51 rest-back 55; P1 117/1689 swaps 23>8 rest-back 6; P2 186/2246 swaps 0>0 rest-back 0; P3 378/3408 swaps 65>38 rest-back 65
P4 250/1920 swaps 66>0 rest-back 2; P6 414/3318 swaps 58>12 rest-back 34; P7 146/1237 swaps 0>0 rest-back 0; P8 327/3048 swaps 64>8 rest-back 44
P9 413/3193 swaps 136>40 rest-back 11; P11 346/2686 swaps 47>8 rest-back 46; P12 216/1737 swaps 44>1 rest-back 1; P15 237/1452 swaps 0>0 rest-back 1
P16 324/2549 swaps 58>3 rest-back 14; P18 182/1394 swaps 8>4 rest-back 3; P22 246/2461 swaps 71>29 rest-back 40; P23 397/2858 swaps 48>0 rest-back 11
```

Bots at 8:25: P0 Frostwolf atk a141 f135; P1 Stormwind atk a63 f64; P2 Quel'thalas wait a71 f73; P3 Scourge atk a122 f96; P4 Ironforge atk a99 f99; P6 Fel Horde atk a125 f126; P7 Dalaran wait a36 f36; P8 Skywall atk a127 f128; P9 Lordaeron atk a98 f99; P11 Druids atk a113 f100; P15 Illidan atk a63 f68; P16 Ahn'qiraj atk a136 f80; P18 The Exodar def a65 f97; P22 Kul'tiras atk a133 f123; P23 Legion atk a122 f74; P12 Black Empire atk a94 f79

Heartbeat: WLAI heartbeat build=0.48.3 t=08m09s units-on-map=3477 orders/min=1989 orders=smart
  task 8:05 one-shots
  task 8:05 latency probe
  task 8:05 combat
  ...

### v0.48.4 -- 3 dumps, 5:00 .. 15:00 -- 0.48.4, 16 bots (P16 Ahn'Qiraj on autopilot from 0:53), Veteran, ~17 min (heartbeat 16:49); no stopwatch. Maintainer: improved a lot, no back and forth seen; some stutter (dwarves at turn 15). Engine walks 4.5 % -> 1.0 % of walks (0-10 min; 0.48.3 at 8:25 vs this game at 10:00); Druid trail units 11-19k from spawn. Scourge army 6:06-10:06 at a sleeping Mammut camp (Borean Tundra). Units 3852 at 15 min (0.48.2: 3536), bot orders lower (22,965 vs 25,178). Latency windows not comparable (probe samples idle units; without guard positions those stand in crowds).

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 0 \| 4 | 11 \| 0 | 23 \| 0 | 4 \| 5 | 0 \| 0 | 0 \| 291 | 0 \| 8 |
| 5:00-10:00 | 0 \| 4 | 21 \| 0 | 84 \| 0 | 4 \| 3 | 0 \| 0 | 0 \| 689 | 0 \| 28 |
| 10:00-15:00 | 0 \| 3 | 43 \| 0 | 113 \| 0 | 2 \| 4 | 0 \| 0 | 0 \| 998 | 0 \| 54 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 4170 | 1138 | 1599 / 146 | - | North Alliance 25, South Alliance 31, Burning Legion 17, Outland 15, Old Gods 26, Kalimdor 33 |
| 15:00 | 3852 | 2316 | 1762 / 174 | - | North Alliance 28, South Alliance 33, Burning Legion 18, Outland 19, Old Gods 40, Kalimdor 22 |

LOAD at the checkpoints:
```
  10:00  combat 156/417 orders=2 | def-scan 11/281 | production peak 269 | wave-tick 387/3441 | last gate sweep 3412
  10:00  army smart orders: change=8420 idle-away=519 rest=1726 refill=534 advance=0 exit=49 | chains on issued=9526 points=19518 fallback=68 | stalled 3% of 15694 | staging on route 75, kept 84 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 0
  10:00  groups (food group/need of army @staging): P0 45/50 of 51 @route node; P1 50/50 of 115 @Blasted Lands (15 gold/min); P2 all 65; P3 38/50 of 83 @route node; P4 50/50 of 106 @route node; P6 50/50 of 125 @route node; P7 36/50 of 38 @route node; P8 49/50 of 115 @Southern Barrens (10 gold/min); P9 50/50 of 100 @route node; P11 50/50 of 73 @Winterspring (10 gold/min); P12 50/50 of 87 @route node; P15 all 65; P16 all 95; P18 26/50 of 68 @Desolace (10 gold/min); P22 44/50 of 107 @Stranglethorn (15 gold/min); P23 52/50 of 52 @route node;
  10:00  switches per bot (target/state, since start): P0 20/0; P1 10/0; P2 8/1; P3 8/2; P4 10/0; P6 15/2; P7 9/0; P8 12/4; P9 13/0; P11 13/2; P12 10/4; P15 12/1; P16 11/1; P18 16/2; P22 11/0; P23 10/0;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 1768; P1 1436; P2 2199; P3 2672; P4 1982; P6 2701; P7 1764; P8 1983; P9 3024; P11 2358; P12 2099; P15 2090; P18 1897; P22 1721; P23 2893;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 21 <1s 50 <2s 53 <5s 34 <10s 25 stuck 22
  10:00  unit budget 30 + 7/CP (army/budget, capped turns): P0 103/114 c8; P1 116/93 c16; P2 67/65 c21; P3 83/79 c24; P4 107/107 c6; P6 125/100 c21; P7 38/100 c0; P8 115/107 c19; P9 100/100 c23; P11 135/121 c18; P12 91/79 c19; P15 70/65 c13; P18 71/86 c2; P22 111/107 c13; P23 100/100 c15; P16 103/86 c20;
  10:00  guard positions removed: on [slots P0 P1 P2 P3 P4 P6 P7 P8 P9 P11 P12 P15 P18 P22 P23 P16], on 2242 units; idle-away orders on 16 bots avg 32 | off 0 bots avg 0; rest orders on 16 bots avg 107 | off 0 bots avg 0
  10:00  attack groups: units left out (no way to the target) 11
  10:00  caster cap 35%, caster orders skipped per bot: P0 8; P1 24; P2 127; P3 33; P6 2; P7 544; P8 98; P11 25; P12 4; P15 77; P18 60; P22 113; P23 18;
  10:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P18 P22 P23 P16]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 8 | off 5 bots avg 8
  10:00  activity (0.46, sampled army units): walking 69% fighting 8% idle 19% stalled 2% of 23909
  10:00  target returns within 5 min (A->B->A) per bot: P0 0; P1 0; P2 0; P3 0; P4 0; P6 0; P7 0; P8 0; P9 0; P11 0; P12 1; P15 0; P18 0; P22 0; P23 0; P16 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx20; P1: Nx10; P2: Nx7; P3: Nx8; P4: Nx10; P6: Nx15; P7: Nx9; P8: Nx12; P9: Nx13; P11: Nx13; P12: Nx9 ownx1; P15: Nx11; P18: Nx16; P22: Nx11; P23: Nx10; P16: Nx11;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=4160 over=451 would-defer=615 deferred=0
  10:00  orders total=11385 near(<2k)=3621 mid(2-8k)=7972 far(>8k)=68 idle-at-target(<300)=23
  15:00  combat 212/703 orders=0 | def-scan 0/462 | production peak 300 | wave-tick 431/4129 | last gate sweep 3731
  15:00  army smart orders: change=16905 idle-away=1203 rest=3530 refill=950 advance=18 exit=85 | chains on issued=18704 points=39259 fallback=125 | stalled 4% of 31653 | staging on route 114, kept 145 | defence kept away (other landmass) 115 | defended by the rest (group kept its front) 6
  15:00  groups (food group/need of army @staging): P0 0/50 of 1 @Southern Ashenvale (10 gold/min); P1 13/107 of 13 @route node; P2 all 56; P3 all 123; P4 50/50 of 139 @route node; P6 50/50 of 123 @Swamp of Sorrows (15 gold/min); P7 40/75 of 40 @Dalaran (30 gold/min); P8 50/50 of 144 @Northern Barrens (10 gold/min); P9 50/50 of 108 @route node; P11 all 81; P12 50/50 of 104 @Astranaar (15 gold/min); P15 48/50 of 84 @route node; P16 50/50 of 98 @route node; P18 0/68 of 4 @Grove of the Ancients (10 gold/min); P22 55/104 of 106 @Zul'Gurub (25 gold/min); P23 98/131 of 130 @Alterac Mountains (20 gold/min);
  15:00  switches per bot (target/state, since start): P0 32/10; P1 17/2; P2 12/3; P3 18/5; P4 21/0; P6 22/5; P7 16/0; P8 24/6; P9 25/6; P11 18/5; P12 19/8; P15 18/2; P16 23/2; P18 25/8; P22 19/0; P23 12/2;
  15:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2420; P1 2191; P2 2939; P3 3547; P4 2976; P6 3669; P7 2788; P8 3095; P9 4014; P11 3378; P12 3050; P15 2870; P18 2382; P22 2782; P23 4029;
  15:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 29 <1s 72 <2s 72 <5s 64 <10s 42 stuck 37
  15:00    0-5m <0.5s 12 <1s 29 <2s 32 <5s 10 <10s 3 stuck 7
  15:00    5-10m <0.5s 8 <1s 21 <2s 21 <5s 24 <10s 22 stuck 15
  15:00    10-15m <0.5s 9 <1s 22 <2s 19 <5s 30 <10s 17 stuck 15
  15:00  unit budget 30 + 7/CP (army/budget, capped turns): P0 3/51 c8; P1 14/72 c23; P2 59/72 c32; P3 97/79 c42; P4 140/128 c24; P6 123/114 c37; P7 33/114 c0; P8 134/135 c32; P9 99/100 c38; P11 141/135 c35; P12 110/107 c35; P15 90/79 c30; P18 7/65 c2; P22 117/121 c29; P23 94/107 c27; P16 131/121 c37;
  15:00  guard positions removed: on [slots P0 P1 P2 P3 P4 P6 P7 P8 P9 P11 P12 P15 P18 P22 P23 P16], on 4236 units; idle-away orders on 16 bots avg 75 | off 0 bots avg 0; rest orders on 16 bots avg 220 | off 0 bots avg 0
  15:00  attack groups: units left out (no way to the target) 79
  15:00  caster cap 35%, caster orders skipped per bot: P0 8; P1 59; P2 274; P3 33; P6 30; P7 1110; P8 222; P11 39; P12 40; P15 111; P18 72; P22 159; P23 30;
  15:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P18 P22 P23 P16]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 10 | off 5 bots avg 12
  15:00  activity (0.46, sampled army units): walking 68% fighting 12% idle 15% stalled 3% of 45469
  15:00  target returns within 5 min (A->B->A) per bot: P0 3; P1 1; P2 0; P3 1; P4 1; P6 0; P7 0; P8 2; P9 3; P11 0; P12 1; P15 0; P18 3; P22 0; P23 0; P16 1;
  15:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x3 Nx24 ownx5; P1: P6x3 P15x2 Nx11 ownx1; P2: P3x3 P23x1 Nx7; P3: P9x5 Nx11 ownx2; P4: P6x1 Nx20; P6: P1x3 Nx17 ownx1; P7: P3x2 P23x3 Nx11; P8: P0x7 Nx16 ownx1; P9: P3x2 Nx19 ownx4; P11: P8x2 P12x1 Nx14 ownx1; P12: P0x3 P11x2 P18x3 Nx9 ownx2; P15: P1x3 P22x1 Nx13; P18: P12x5 Nx17 ownx3; P22: P6x2 P15x3 Nx14; P23: P9x1 Nx11; P16: P0x9 Nx14;
  15:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=3600 max-slot=4211 over=825 would-defer=1135 deferred=0
  15:00  orders total=22965 near(<2k)=7941 mid(2-8k)=15431 far(>8k)=125 idle-at-target(<300)=31
```

Top tasks by total ops at 15:00 (of 44):
```
task combat every=0.500s runs=1800 last=212 max=1782 sum=682179
task production every=1.000s runs=899 last=187 max=300 sum=126672
task route portals every=30.000s runs=29 last=3844 max=4211 sum=116859
task route structures 2 every=15.000s runs=59 last=1134 max=1179 sum=49193
task route structures 1 every=15.000s runs=59 last=854 max=1078 sum=42142
task route creeps every=15.000s runs=59 last=141 max=1463 sum=40661
task route structures 3 every=15.000s runs=59 last=637 max=637 sum=23195
task route structures 4 every=15.000s runs=59 last=514 max=539 sum=21270
task gate sweep every=15.000s runs=59 last=0 max=3731 sum=9969
task hero visits every=15.000s runs=59 last=212 max=242 sum=6580
```

Turnarounds at 15:00 (0.48.3; per cause, all bots; P lines: turns / walks):
```
total: change 3340 engine 978 rest 805 whole 142 defend 101 idle-away 43 refill 40 portal 34 def-rest 21 exit 12 advance 1
phase 0-5m: change 861 rest 27 refill 3 whole 3 portal 20 engine 56
phase 5-10m: change 1341 rest 315 idle-away 23 refill 11 exit 12 defend 14 whole 62 portal 10 engine 311
phase 10-15m: change 1138 advance 1 rest 463 idle-away 20 refill 26 defend 87 whole 77 def-rest 21 portal 4 engine 607
phase 15-20m: engine 4
P0 293/3622 swaps 50>16 rest-back 17; P1 271/3201 swaps 30>0 rest-back 1; P2 240/2488 swaps 24>0 rest-back 1; P3 352/3677 swaps 193>28 rest-back 55
P4 435/5576 swaps 184>100 rest-back 123; P6 429/6068 swaps 156>51 rest-back 102; P7 139/1580 swaps 0>0 rest-back 0; P8 437/6092 swaps 212>84 rest-back 121
P9 590/5645 swaps 259>138 rest-back 177; P11 547/6176 swaps 60>31 rest-back 55; P12 421/4738 swaps 49>33 rest-back 47; P15 164/2333 swaps 40>9 rest-back 14
P16 409/5443 swaps 170>18 rest-back 29; P18 152/1613 swaps 30>6 rest-back 6; P22 281/4516 swaps 179>107 rest-back 109; P23 357/4289 swaps 33>21 rest-back 23
```

Bots at 15:00: P0 Warsong atk a5 f4; P1 Stormwind atk a36 f15; P2 Quel'thalas def a85 f65; P3 Scourge def a143 f96; P4 Ironforge atk a140 f140; P6 Fel Horde atk a123 f125; P7 Dalaran atk a50 f29; P8 Skywall atk a154 f141; P9 Lordaeron atk a140 f101; P11 Druids def a154 f141; P12 Black Empire atk a131 f107; P15 Illidan atk a96 f90; P18 The Exodar atk a8 f7; P22 Kul'tiras atk a154 f100; P23 Legion atk a169 f94; P16 Ahn'qiraj atk a174 f131

Heartbeat: WLAI heartbeat build=0.48.4 t=16m49s units-on-map=3418 orders/min=1414 orders=smart
  task 16:45 bot income
  task 16:45 latency probe
  task 16:45 destructible gates
  ...

### v0.48.5 -- 4 dumps, 5:00 .. 20:00 -- 0.48.5, multiplayer (stability test), maintainer as Kul Tiras (P22; autopilot 0:18-16:09), 15 bots, Veteran, ~22.5 min (heartbeat 22:29). Stopwatch: ~5 s behind real time at 20 min; game felt okay. Dark Portal open from ~8 min but unused: its waygates left Neutral Hostile ownership between 5 and 10 min, and the 0.48 owner rule blocked both sides -- Fel Horde / Illidan held in Outland ~10 min, Stormwind / Ironforge waited at the Thandol rocks from 13:12 (fixed in 0.50).

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-5:00 | 0 \| 5 | 8 \| 0 | 21 \| 0 | 5 \| 4 | 0 \| 0 | 0 \| 314 | 0 \| 7 |
| 5:00-10:00 | 0 \| 4 | 13 \| 0 | 102 \| 0 | 4 \| 4 | 0 \| 0 | 0 \| 870 | 0 \| 15 |
| 10:00-15:00 | 0 \| 2 | 31 \| 0 | 84 \| 0 | 2 \| 3 | 0 \| 0 | 0 \| 871 | 0 \| 73 |
| 15:00-20:00 | 0 \| 1 | 66 \| 0 | 70 \| 0 | 1 \| 1 | 0 \| 0 | 0 \| 462 | 0 \| 53 |

Bots: 15, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3904 | 1505 | 1510 / 121 | - | North Alliance 26, South Alliance 32, Burning Legion 17, Outland 15, Old Gods 26, Kalimdor 37 |
| 20:00 | 3513 | 1540 | 1346 / 156 | - | North Alliance 27, South Alliance 36, Burning Legion 19, Outland 16, Old Gods 36, Kalimdor 29 |

LOAD at the checkpoints:
```
  10:00  combat 240/444 orders=0 | def-scan 3/182 | production peak 259 | wave-tick 419/3471 | last gate sweep 3441
  10:00  army smart orders: change=11719 idle-away=341 rest=1972 refill=565 advance=0 exit=41 | chains on issued=12750 points=30550 fallback=27 | stalled 1% of 18944 | staging on route 113, kept 83 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 0
  10:00  groups (food group/need of army @staging): P0 32/50 of 32 @route node; P1 77/77 of 96 @route node; P2 all 62; P3 50/50 of 107 @Borean Tundra (15 gold/min); P4 50/50 of 87 @route node; P6 all 119; P7 all 88; P8 50/50 of 101 @Kezan (15 gold/min); P9 all 90; P11 50/50 of 89 @route node; P12 50/50 of 66 @Twilight Landing (15 gold/min); P15 all 51; P16 68/68 of 87 @route node; P18 50/50 of 99 @route node; P22 89/77 of 89 @route node; P23 all 84;
  10:00  switches per bot (target/state, since start): P0 19/2; P1 12/0; P2 8/1; P3 13/2; P4 14/0; P6 19/3; P7 14/1; P8 19/2; P9 17/3; P11 18/2; P12 16/2; P15 13/5; P16 18/2; P18 18/2; P22 12/0; P23 16/1;
  10:00  wake (0.48.5) on: sleeping creeps woken 107 in 440 checks; per bot: P0 21; P1 6; P3 12; P4 5; P6 1; P8 2; P9 13; P11 1; P12 3; P15 8; P16 16; P18 8; P22 3; P23 8;
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2073; P1 1497; P2 2194; P3 2730; P4 1979; P6 2788; P7 1899; P8 1962; P9 3046; P11 2216; P12 2098; P15 2062; P16 2879; P18 2098; P23 2721;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 43 <1s 49 <2s 48 <5s 39 <10s 25 stuck 19
  10:00  unit budget 30 + 5/CP (army/budget, capped turns): P0 109/100 c19; P1 97/80 c16; P2 64/55 c23; P3 100/75 c25; P4 88/85 c14; P6 119/85 c23; P7 88/85 c20; P8 101/85 c22; P9 89/80 c24; P11 90/85 c20; P12 67/65 c26; P15 56/50 c16; P16 91/70 c26; P18 85/90 c22; P23 65/70 c20; P22 94/85 c20;
  10:00  guard positions removed: on [slots P0 P1 P2 P3 P4 P6 P7 P8 P9 P11 P12 P15 P16 P18 P23 P22], on 1930 units; idle-away orders on 16 bots avg 21 | off 0 bots avg 0; rest orders on 16 bots avg 123 | off 0 bots avg 0
  10:00  attack groups: units left out (no way to the target) 21
  10:00  caster cap 35%, caster orders skipped per bot: P0 51; P1 12; P2 115; P3 18; P6 2; P7 73; P8 73; P11 32; P12 4; P15 60; P18 14; P23 16;
  10:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P23 P22]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 9 | off 5 bots avg 8
  10:00  activity (0.46, sampled army units): walking 82% fighting 8% idle 6% stalled 1% of 24584
  10:00  target returns within 5 min (A->B->A) per bot: P0 1; P1 0; P2 0; P3 0; P4 0; P6 0; P7 0; P8 1; P9 0; P11 1; P12 0; P15 0; P16 0; P18 1; P23 0; P22 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx18 ownx1; P1: Nx12; P2: Nx7; P3: Nx13; P4: Nx14; P6: Nx18; P7: Nx13; P8: Nx19; P9: Nx15; P11: Nx18; P12: Nx16; P15: Nx10; P16: Nx17 ownx1; P18: Nx18; P23: Nx15; P22: Nx12;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=4115 over=508 would-defer=667 deferred=0
  10:00  orders total=15050 near(<2k)=4429 mid(2-8k)=10884 far(>8k)=27 idle-at-target(<300)=40
  20:00  combat 81/793 orders=7 | def-scan 20/327 | production peak 311 | wave-tick 3085/6864 | last gate sweep 3340
  20:00  army smart orders: change=21017 idle-away=1779 rest=4322 refill=1661 advance=259 exit=541 | chains on issued=25059 points=63849 fallback=52 | stalled 5% of 40135 | staging on route 169, kept 123 | defence kept away (other landmass) 19 | defended by the rest (group kept its front) 8
  20:00  groups (food group/need of army @staging): P0 all 7; P1 all 84; P2 50/50 of 69 @route node; P3 55/79 of 144 @route node; P4 all 117; P6 all 116; P7 13/50 of 13 @Dalaran (30 gold/min); P8 70/66 of 78 @Thunderbluff (20 gold/min); P9 14/50 of 14 @route node; P11 all 112; P12 16/81 of 16 @Feathermoon (20 gold/min); P15 all 63; P16 46/50 of 58 @route node; P18 all 65; P22 all 118; P23 75/96 of 94 @route node;
  20:00  switches per bot (target/state, since start): P0 29/9; P1 19/3; P2 18/6; P3 26/10; P4 22/1; P6 19/3; P7 20/6; P8 29/4; P9 26/13; P11 30/3; P12 31/10; P15 13/5; P16 29/6; P18 29/15; P22 21/2; P23 30/8;
  20:00  wake (0.48.5) on: sleeping creeps woken 116 in 695 checks; per bot: P0 21; P1 6; P3 12; P4 5; P6 1; P8 2; P9 13; P11 9; P12 3; P15 8; P16 16; P18 8; P22 4; P23 8;
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 3210; P1 3185; P2 3814; P3 4826; P4 3949; P6 4610; P7 3542; P8 4185; P9 4841; P11 3911; P12 3747; P15 3279; P16 4926; P18 3436; P23 4328;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 89 <1s 101 <2s 85 <5s 69 <10s 42 stuck 67
  20:00    0-5m <0.5s 9 <1s 25 <2s 29 <5s 16 <10s 6 stuck 8
  20:00    5-10m <0.5s 34 <1s 24 <2s 19 <5s 23 <10s 19 stuck 11
  20:00    10-15m <0.5s 28 <1s 28 <2s 24 <5s 17 <10s 11 stuck 9
  20:00    15-20m <0.5s 18 <1s 24 <2s 13 <5s 13 <10s 6 stuck 39
  20:00  unit budget 30 + 5/CP (army/budget, capped turns): P0 9/50 c24; P1 85/80 c54; P2 73/80 c46; P3 121/95 c60; P4 118/100 c51; P6 116/85 c61; P7 10/75 c30; P8 74/95 c41; P9 15/70 c42; P11 114/105 c52; P12 21/60 c38; P15 66/55 c55; P16 110/110 c61; P18 62/85 c49; P23 63/60 c36;
  20:00  guard positions removed: on [slots P0 P1 P2 P3 P4 P6 P7 P8 P9 P11 P12 P15 P16 P18 P23], on 6685 units; idle-away orders on 15 bots avg 118 | off 0 bots avg 0; rest orders on 15 bots avg 277 | off 0 bots avg 0
  20:00  attack groups: units left out (no way to the target) 54
  20:00  caster cap 35%, caster orders skipped per bot: P0 166; P1 12; P2 388; P3 18; P6 2; P7 280; P8 448; P9 109; P11 107; P12 4; P15 60; P16 24; P18 54; P23 64;
  20:00  regroup ab [slots P0 P1 P2 P3 P4 P6 P7 P16 P18 P23]: times entered on 10 bots avg 0 | off 5 bots avg 0; CP captures per bot on 10 bots avg 12 | off 5 bots avg 12
  20:00  activity (0.46, sampled army units): walking 65% fighting 12% idle 17% stalled 3% of 58348
  20:00  target returns within 5 min (A->B->A) per bot: P0 4; P1 1; P2 0; P3 2; P4 0; P6 0; P7 2; P8 1; P9 2; P11 3; P12 6; P15 0; P16 1; P18 7; P23 2;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x1 P16x5 Nx18 ownx5; P1: P6x2 P15x1 Nx14 ownx1; P2: P3x3 P23x6 Nx7 ownx1; P3: P9x9 Nx14 ownx3; P4: P6x3 P15x1 Nx17; P6: Nx18; P7: P3x1 P23x3 Nx13 ownx2; P8: P0x7 P18x3 Nx19; P9: P3x3 Nx16 ownx4; P11: P8x1 P12x4 Nx24 ownx1; P12: P0x2 P11x3 P18x6 Nx16 ownx4; P15: Nx10; P16: P0x9 Nx17 ownx3; P18: P12x3 P16x2 Nx18 ownx6; P23: P7x5 P9x6 Nx15 ownx3;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4115 over=1362 would-defer=1802 deferred=0
  20:00  orders total=30449 near(<2k)=9934 mid(2-8k)=20933 far(>8k)=52 idle-at-target(<300)=593
```

Top tasks by total ops at 20:00 (of 44):
```
task combat every=0.500s runs=2400 last=352 max=1849 sum=1017598
task production every=1.000s runs=1199 last=165 max=311 sum=181079
task route portals every=30.000s runs=39 last=3522 max=4115 sum=149895
task hero visits every=15.000s runs=79 last=2868 max=3326 sum=68919
task route structures 2 every=15.000s runs=79 last=893 max=1115 sum=68642
task route structures 1 every=15.000s runs=79 last=939 max=1104 sum=61586
task route creeps every=15.000s runs=79 last=84 max=1441 sum=38459
task route structures 3 every=15.000s runs=79 last=484 max=594 sum=32774
task route structures 4 every=15.000s runs=79 last=506 max=553 sum=31338
task gate sweep every=15.000s runs=79 last=0 max=3441 sum=13033
```

Turnarounds at 20:00 (0.48.3; per cause, all bots; P lines: turns / walks):
```
total: change 4204 engine 1071 rest 833 whole 595 defend 189 portal 112 idle-away 108 refill 85 advance 19 exit 7
phase 0-5m: change 642 rest 31 idle-away 3 refill 5 whole 3 portal 30 engine 94
phase 5-10m: change 2332 rest 518 idle-away 31 refill 8 exit 6 defend 48 whole 253 portal 73 engine 327
phase 10-15m: change 901 advance 18 rest 224 idle-away 48 refill 19 exit 1 defend 126 whole 305 portal 7 engine 457
phase 15-20m: change 329 advance 1 rest 60 idle-away 26 refill 53 defend 15 whole 34 portal 2 engine 193
P0 435/4334 swaps 71>22 rest-back 35; P1 375/5235 swaps 105>37 rest-back 22; P2 358/4184 swaps 26>22 rest-back 9; P3 375/6569 swaps 153>128 rest-back 144
P4 664/6507 swaps 232>116 rest-back 79; P6 306/4029 swaps 69>28 rest-back 55; P7 407/4485 swaps 85>48 rest-back 76; P8 738/7981 swaps 162>26 rest-back 118
P9 592/5249 swaps 109>67 rest-back 93; P11 523/6626 swaps 217>93 rest-back 89; P12 347/4463 swaps 43>6 rest-back 51; P15 141/1367 swaps 0>0 rest-back 0
P16 660/7431 swaps 189>98 rest-back 77; P18 449/6086 swaps 134>63 rest-back 66; P22 401/5167 swaps 136>88 rest-back 49; P23 452/6007 swaps 156>71 rest-back 62
```

Bots at 20:00: P0 Frostwolf def a10 f10; P1 Stormwind wait a84 f85; P2 Quel'thalas atk a101 f68; P3 Scourge atk a150 f119; P4 Ironforge wait a117 f118; P6 Fel Horde hold a119 f119; P7 Gilneas atk a39 f9; P8 Skywall atk a92 f72; P9 Lordaeron atk a39 f16; P11 Druids def a128 f114; P12 Black Empire atk a30 f22; P15 Illidan hold a63 f66; P16 Ahn'qiraj atk a156 f110; P18 Sentinels def a79 f61; P23 Legion atk a139 f62

Heartbeat: WLAI heartbeat build=0.48.5 t=22m29s units-on-map=2870 orders/min=1199 orders=smart
  task 22:25 latency probe
  task 22:25 early quests
  task 22:25 palace handover
  ...

### v0.50.1 -- 4 dumps, 8:27 .. 20:00 -- 0.50.1, maintainer as Scourge (P3; autopilot 6:48-7:08 and from 17:26), Veteran, ~20 min. Ferries look good (slower wanted). The Book: the Scourge took it at the Gilneas pedestal at 6:56 (bot, level 5), the Legion's Envoy and later Tichondrius (maintainer gave it) held it -- every ritual order refused ('volcano' / 'deathanddecay'), no channel -> 0.51 bot ritual.

**Errors (TICK ERROR / FAILED / section errors): none**

A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:

| phase | gather: CP captures per bot | guard: idle-away orders | guard: rest orders | regroup: CP captures per bot | regroup: times entered | spread: sampled per bot | spread: stalled samples per bot |
|---|---|---|---|---|---|---|---|
| 0-8:27 | 0 \| 8 | 24 \| 0 | 68 \| 0 | 8 \| 7 | 0 \| 0 | 0 \| 903 | 0 \| 33 |
| 8:27-10:00 | 0 \| 0 | 8 \| 0 | 32 \| 0 | 1 \| 0 | 0 \| 0 | 0 \| 228 | 0 \| 7 |
| 10:00-15:00 | 0 \| 3 | 39 \| 0 | 84 \| 0 | 2 \| 3 | 0 \| 0 | 0 \| 778 | 0 \| 51 |
| 15:00-20:00 | 0 \| 1 | 38 \| 0 | 49 \| 0 | 0 \| 4 | 0 \| 0 | 0 \| 491 | 0 \| 47 |

Bots: 16, orders mode: smart.

| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |
|---|---|---|---|---|---|
| 10:00 | 3772 | 1287 | 1222 / 142 | - | North Alliance 24, South Alliance 31, Burning Legion 19, Outland 16, Old Gods 24, Kalimdor 35 |
| 20:00 | 2875 | 1607 | 1113 / 195 | - | North Alliance 14, South Alliance 21, Burning Legion 34, Outland 30, Old Gods 38, Kalimdor 26 |

LOAD at the checkpoints:
```
  10:00  combat 149/494 orders=32 | def-scan 28/164 | production peak 259 | wave-tick 493/3369 | last gate sweep 3339
  10:00  army smart orders: change=10161 idle-away=483 rest=1502 refill=494 advance=0 exit=42 | chains on issued=10437 points=24721 fallback=233 | stalled 3% of 17013 | staging on route 83, kept 95 | defence kept away (other landmass) 0 | defended by the rest (group kept its front) 1
  10:00  groups (food group/need of army @staging): P0 50/50 of 77 @route node; P1 50/50 of 99 @route node; P2 all 57; P3 52/50 of 52 @route node; P4 23/50 of 23 @route node; P6 50/50 of 126 @route node; P7 all 12; P8 44/50 of 69 @route node; P9 all 93; P11 37/50 of 90 @Stonetalon Peak (10 gold/min); P12 40/50 of 70 @route node; P15 50/50 of 50 @-; P16 47/50 of 96 @Desolace (10 gold/min); P18 51/50 of 87 @route node; P22 48/50 of 74 @Deadwind Pass (15 gold/min); P23 all 59;
  10:00  switches per bot (target/state, since start): P0 17/0; P1 13/1; P2 7/1; P3 1/1; P4 14/0; P6 18/2; P7 12/3; P8 16/2; P9 13/3; P11 14/2; P12 12/2; P15 15/4; P16 23/2; P18 17/0; P22 15/0; P23 15/9;
  10:00  wake (0.48.5) on: sleeping creeps woken 65 in 401 checks; per bot: P0 7; P1 3; P4 4; P8 2; P11 5; P15 7; P16 19; P18 7; P22 11;
  10:00  legion ritual (0.49): order refused x1 (cooldown, Argus, or the ritual is not available yet) -- The Darkener of Legion; orders 7, channels 0, Book given at the pedestal (bot, no level 12) 1 -- pedestal @7936,512
  10:00  ferries (0.50) on: 8 links; boarded 363, landed 311, voyages 38, now at sea 13 / boarding 35; route checks refused (enemy pier) 1055
  10:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 2136; P1 1426; P2 2202; P4 1916; P6 2789; P7 1851; P8 1821; P9 2998; P11 2275; P12 2116; P15 2071; P16 2923; P18 1902; P22 1707; P23 2523;
  10:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 16 <1s 35 <2s 71 <5s 36 <10s 29 stuck 14
  10:00  unit budget 30 + 5/CP (army/budget, capped turns): P0 124/110 c20; P1 100/80 c17; P2 59/55 c25; P4 90/85 c15; P6 126/85 c24; P7 9/80 c6; P8 74/70 c24; P9 93/75 c26; P11 102/75 c21; P12 81/70 c28; P15 55/55 c18; P16 100/70 c27; P18 87/80 c20; P22 80/80 c20; P23 56/55 c25;
  10:00  guard positions removed: on [slots P0 P1 P2 P4 P6 P7 P8 P9 P11 P12 P15 P16 P18 P22 P23], on 1797 units; idle-away orders on 15 bots avg 32 | off 0 bots avg 0; rest orders on 15 bots avg 100 | off 0 bots avg 0
  10:00  attack groups: units left out (no way to the target) 110
  10:00  caster cap 35%, caster orders skipped per bot: P0 53; P1 26; P2 117; P6 2; P7 134; P8 79; P11 31; P12 7; P15 73; P16 4; P18 28; P22 75; P23 12;
  10:00  regroup ab [slots P0 P1 P2 P4 P6 P7 P16 P18 P22 P23]: times entered on 10 bots avg 0 | off 5 bots avg 0; CP captures per bot on 10 bots avg 9 | off 5 bots avg 7
  10:00  activity (0.46, sampled army units): walking 77% fighting 9% idle 10% stalled 2% of 22475
  10:00  target returns within 5 min (A->B->A) per bot: P0 0; P1 0; P2 0; P4 0; P6 1; P7 0; P8 1; P9 0; P11 1; P12 0; P15 0; P16 0; P18 0; P22 0; P23 0;
  10:00  targets by owner (since start; N = neutral, own = defending itself): P0: Nx17; P1: Nx12 ownx1; P2: Nx6; P4: Nx14; P6: Nx18; P7: Nx10; P8: Nx16; P9: Nx11; P11: Nx14; P12: Nx12; P15: Nx13; P16: Nx23; P18: Nx17; P22: Nx15; P23: Nx10;
  10:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=2400 max-slot=4044 over=502 would-defer=676 deferred=0
  10:00  orders total=12869 near(<2k)=4463 mid(2-8k)=8441 far(>8k)=198 idle-at-target(<300)=31
  20:00  combat 124/572 orders=0 | def-scan 0/267 | production peak 332 | wave-tick 595/5678 | last gate sweep 2996
  20:00  army smart orders: change=20400 idle-away=1749 rest=3742 refill=1465 advance=160 exit=255 | chains on issued=23978 points=60225 fallback=249 | stalled 5% of 38403 | staging on route 143, kept 155 | defence kept away (other landmass) 223 | defended by the rest (group kept its front) 20
  20:00  groups (food group/need of army @staging): P0 16/129 of 17 @Orgrimmar (25 gold/min); P1 all 2; P2 19/72 of 19 @Gilneas City (25 gold/min); P3 70/70 of 157 @route node; P4 16/183 of 16 @route node; P6 82/82 of 130 @route node; P7 0/72 of 2 @Keel Harbor (10 gold/min); P8 103/164 of 103 @route node; P9 all 47; P11 all 16; P12 50/50 of 91 @route node; P15 96/82 of 96 @Western Stranglethorn (15 gold/min); P16 43/50 of 43 @route node; P18 30/50 of 92 @Grove of the Ancients (10 gold/min); P22 0/159 of 6 @Highbank (10 gold/min); P23 70/70 of 108 @route node;
  20:00  switches per bot (target/state, since start): P0 30/12; P1 28/13; P2 14/4; P3 4/3; P4 21/0; P6 35/4; P7 25/14; P8 35/8; P9 20/9; P11 28/12; P12 26/7; P15 35/12; P16 38/8; P18 28/0; P22 31/12; P23 31/10;
  20:00  wake (0.48.5) on: sleeping creeps woken 65 in 723 checks; per bot: P0 7; P1 3; P4 4; P8 2; P11 5; P15 7; P16 19; P18 7; P22 11;
  20:00  legion ritual (0.49): paused after refused orders; orders 30, channels 0, Book given at the pedestal (bot, no level 12) 1 -- pedestal @7936,512
  20:00  ferries (0.50) on: 8 links; boarded 1023, landed 1015, voyages 110, now at sea 0 / boarding 0; route checks refused (enemy pier) 3995
  20:00  bot income paid since start (0.44.2, CP income WL pays only to human slots): P0 3671; P1 2539; P2 3542; P4 3972; P6 5282; P7 3032; P8 3844; P9 3755; P11 3904; P12 3946; P15 3853; P16 4751; P18 3089; P22 2798; P23 4232;
  20:00  order latency (0.46, game time from order to moving, idle units): all <0.5s 47 <1s 87 <2s 97 <5s 79 <10s 42 stuck 88
  20:00    0-5m <0.5s 4 <1s 15 <2s 42 <5s 16 <10s 7 stuck 6
  20:00    5-10m <0.5s 12 <1s 20 <2s 29 <5s 20 <10s 21 stuck 8
  20:00    10-15m <0.5s 15 <1s 28 <2s 15 <5s 25 <10s 9 stuck 26
  20:00    15-20m <0.5s 16 <1s 24 <2s 11 <5s 18 <10s 5 stuck 48
  20:00  unit budget 30 + 5/CP (army/budget, capped turns): P0 16/65 c34; P1 3/35 c25; P2 22/60 c31; P4 17/105 c34; P6 130/120 c59; P7 4/50 c6; P8 104/100 c54; P9 44/50 c41; P11 17/80 c44; P12 82/90 c61; P15 98/90 c52; P16 47/90 c45; P18 95/75 c57; P22 15/55 c27; P23 99/100 c58; P3 153/130 c8;
  20:00  guard positions removed: on [slots P0 P1 P2 P4 P6 P7 P8 P9 P11 P12 P15 P16 P18 P22 P23 P3], on 5278 units; idle-away orders on 16 bots avg 109 | off 0 bots avg 0; rest orders on 16 bots avg 233 | off 0 bots avg 0
  20:00  attack groups: units left out (no way to the target) 127
  20:00  caster cap 35%, caster orders skipped per bot: P0 114; P1 73; P2 441; P6 30; P7 134; P8 251; P9 22; P11 173; P12 36; P15 106; P16 28; P18 28; P22 202; P23 24;
  20:00  regroup ab [slots P0 P1 P2 P4 P6 P7 P16 P18 P22 P23 P3]: times entered on 11 bots avg 0 | off 5 bots avg 0; CP captures per bot on 11 bots avg 11 | off 5 bots avg 14
  20:00  activity (0.46, sampled army units): walking 73% fighting 12% idle 9% stalled 4% of 49612
  20:00  target returns within 5 min (A->B->A) per bot: P0 3; P1 7; P2 1; P4 0; P6 4; P7 7; P8 4; P9 1; P11 8; P12 3; P15 3; P16 5; P18 0; P22 7; P23 1; P3 0;
  20:00  targets by owner (since start; N = neutral, own = defending itself): P0: P8x2 P12x2 P16x1 Nx19 ownx6; P1: P6x4 P15x2 Nx14 ownx8; P2: P3x3 P23x3 Nx6 ownx1; P4: P6x2 P15x1 Nx18; P6: P1x8 P4x4 P22x3 Nx19 ownx1; P7: P3x5 P23x1 Nx10 ownx7; P8: P0x10 P11x2 Nx20 ownx3; P9: P3x5 Nx11 ownx2; P11: P8x2 P12x2 P16x3 Nx15 ownx6; P12: P0x10 Nx12 ownx4; P15: P1x12 P4x1 P22x4 Nx13 ownx3; P16: P0x6 P11x4 P18x1 Nx24 ownx3; P18: P8x2 P12x3 P16x1 Nx22; P22: P6x2 P15x8 Nx15 ownx6; P23: P2x2 P7x11 P9x2 Nx11; P3: P7x2 Nx1 ownx1;
  20:00  scheduler slot=0.25s cycle=15s budget=400 enforce=off slots=4800 max-slot=4044 over=1229 would-defer=1637 deferred=0
  20:00  orders total=28942 near(<2k)=9346 mid(2-8k)=19785 far(>8k)=209 idle-at-target(<300)=98
```

Top tasks by total ops at 20:00 (of 44):
```
task combat every=0.500s runs=2400 last=124 max=1583 sum=951419
task production every=1.000s runs=1199 last=117 max=332 sum=164815
task route portals every=30.000s runs=39 last=2927 max=4044 sum=141091
task route structures 2 every=15.000s runs=79 last=688 max=971 sum=58775
task route structures 1 every=15.000s runs=79 last=728 max=1088 sum=58479
task route creeps every=15.000s runs=79 last=71 max=1449 sum=39248
task route structures 3 every=15.000s runs=79 last=524 max=653 sum=34115
task route structures 4 every=15.000s runs=79 last=390 max=469 sum=26497
task ferries every=0.250s runs=4800 last=0 max=548 sum=21126
task hero visits every=15.000s runs=79 last=332 max=2487 sum=20667
```

Turnarounds at 20:00 (0.48.3; per cause, all bots; P lines: turns / walks):
```
total: change 4012 engine 1116 rest 681 whole 390 defend 168 idle-away 80 refill 58 exit 37 portal 32 def-rest 27 advance 1
phase 0-5m: change 937 rest 2 idle-away 3 refill 11 whole 37 engine 70
phase 5-10m: change 1597 rest 260 idle-away 8 refill 3 exit 13 whole 353 portal 1 engine 254
phase 10-15m: change 915 rest 258 idle-away 52 refill 34 defend 93 def-rest 5 engine 368
phase 15-20m: change 563 advance 1 rest 161 idle-away 17 refill 10 exit 24 defend 75 def-rest 22 portal 31 engine 424
P0 464/6239 swaps 152>119 rest-back 109; P1 339/3354 swaps 109>43 rest-back 43; P2 136/2940 swaps 0>0 rest-back 2; P3 95/1709 swaps 0>0 rest-back 0
P4 456/5198 swaps 33>21 rest-back 25; P6 719/8849 swaps 379>152 rest-back 144; P7 173/2223 swaps 19>16 rest-back 10; P8 541/7994 swaps 160>52 rest-back 65
P9 472/4483 swaps 175>46 rest-back 74; P11 517/5925 swaps 90>23 rest-back 30; P12 503/6435 swaps 86>38 rest-back 67; P15 483/5468 swaps 33>3 rest-back 63
P16 431/6309 swaps 224>43 rest-back 58; P18 305/3868 swaps 63>25 rest-back 67; P22 294/4237 swaps 36>10 rest-back 4; P23 674/5585 swaps 190>35 rest-back 78
```

Bots at 20:00: P0 Frostwolf atk a22 f10; P1 Stormwind def a3 f4; P2 Quel'thalas atk a21 f23; P4 Ironforge atk a60 f19; P6 Fel Horde atk a140 f132; P7 Gilneas atk a9 f0; P8 Skywall atk a105 f101; P9 Scarlet Crusade def a49 f48; P11 Druids atk a40 f19; P12 Black Empire def a97 f96; P15 Illidan atk a96 f98; P16 Ahn'qiraj atk a76 f49; P18 The Exodar atk a56 f85; P22 Kul'tiras atk a14 f16; P23 Legion atk a130 f111; P3 Scourge atk a195 f153

Heartbeat: WLAI heartbeat build=0.50.1 t=08m29s units-on-map=3253 orders/min=1512 orders=smart
  task 8:26 ferries
  task 8:26 legion events
  task 8:26 legion check
  ...
