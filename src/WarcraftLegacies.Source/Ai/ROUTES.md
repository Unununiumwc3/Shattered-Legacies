# Route graph (generated)

Written by `python3 tools/ai/gen_routes.py` together with `AiRoutes.g.cs`. Review this after every run.

- Nodes: 187 CPs, 62 area nodes
- Edges: 356, of them 8 behind rock groups (9 groups of `LTrc`)
- Hand-written graph (`AiConfig`): 220 edges, 162 of them also generated.

## Hand-written edges NOT generated

A missing direct edge is fine when the generated graph still walks from A to B (through another
node) at a similar cost. "unreachable" = different landmass: waygate / ship / portal (added at
runtime from live waygates) -- or a wall the generator does not know. "detour Nx" = the generated
walk is N times the straight line: check whether something blocks it that does not block in game.

- West-Zangarmarsh <-> Halaar: ok, 1.5x
- East-Zangarmarsh <-> Blade's Edge Mountains: ok, 1.6x
- ?n001 <-> Quel'Danas: not a CP in the map data
- Borean Tundra <-> Dragonblight: ok, 1.1x
- Tomb of Sargeras <-> Val'sharah: ok, 1.3x
- Tunnels of Ahn'Qiraj <-> Ruins of Ahn'Qiraj: ok, 1.4x
- The Abyss <-> Maelstrom: ok, 1.2x
- The Abyss <-> Eldarath: unreachable on foot
- Spinebark Grove <-> Southern Barrens: ok, 1.4x
- Spinebark Grove <-> Ratchet: ok, 1.5x
- Hellfire Peninsula <-> Nethergarde Keep: unreachable on foot
- Duskwood <-> Westfall: ok, 1.3x
- Elwynn Forest <-> Redridge Mountains: ok, 1.6x
- Stormwind City <-> Tiragarde Sound: unreachable on foot
- Burning Steppes <-> Fuselight: detour 2.2x
- Burning Steppes <-> Searing Gorge: ok, 2.0x
- Loch Modan <-> Dun Morogh: ok, 1.6x
- Dun Morogh <-> Wetlands: ok, 1.9x
- Dun Modr <-> Grim Batol: ok, 1.9x
- Durnholde <-> Arathi Highlands: ok, 1.7x
- Alterac Mountains <-> Southshore: ok, 1.5x
- Silverpine Forest <-> Tirisfal Glades: ok, 1.2x
- Fuselight <-> Twilight Shore: detour 3.3x
- Andorhal <-> Stratholme: ok, 1.2x
- Tranquillien <-> Corin's Crossing: ok, 1.6x
- Astranaar <-> Southern Ashenvale: ok, 1.4x
- Azshara Coast <-> Azsuna: unreachable on foot
- Northern Barrens <-> Southern Ashenvale: ok, 1.7x
- Stonetalon Peak <-> Desolace: ok, 1.6x
- Arathi Highlands <-> Jintha'Alor : ok, 2.0x
- Tanaris <-> Uldum: ok, 1.6x
- Tanaris <-> Lost City of the Tol'vir: ok, 1.4x
- Feralas <-> Shrine to Malorne: ok, 1.2x
- Western Stranglethorn <-> Balor: unreachable on foot
- Howling Fjords <-> Venture Bay: ok, 1.4x
- Netherstorm <-> Sylvanaar: ok, 1.6x
- Crystalsong Forest <-> Antoran Wastes: unreachable on foot
- Echo Isles <-> Nazjatar: unreachable on foot
- Echo Isles <-> Alcaz Isle: detour 2.8x
- Teldrassil <-> Auberdine: ok, 1.8x
- Nath'raxas Hold <-> Antoran Wastes: detour 4.2x
- Brill <-> Deathknell: ok, 1.1x
- Quel'Danas <-> ?n079: not a CP in the map data
- Quel'Danas <-> Antoran Wastes: unreachable on foot
- Northern Highlands <-> Dragonmaw Gates: detour 3.4x
- Kezan <-> Nazmir: unreachable on foot
- Gadgetzan <-> Nazmir: unreachable on foot
- Feathermoon <-> Dire Maul: ok, 2.0x
- Dire Maul <-> Ratchet: ok, 1.6x
- Menethil Harbour <-> Tol Barad: unreachable on foot
- Felwood <-> Antoran Wastes: unreachable on foot
- Dragonmaw Port <-> Southern Highlands: detour 2.7x
- Dragonmaw Port <-> Thundermar: ok, 1.1x
- Dragonmaw Port <-> Maw of Gor'ma: unreachable on foot
- Zul'Farrak <-> Shimmering Flats: detour 2.6x
- Ratchet <-> Dustwallow Marsh: ok, 1.9x
- Uldum <-> Throne of the Four Winds: ok, 1.4x
- Antoran Wastes <-> Krokuun: ok, 1.5x

## Area nodes (targets only while enemy structures stand near them)

- #187 area 187 @ -9904,-17840
- #188 area 188 @ -4784,24144
- #189 area 189 @ 17680,-22064
- #190 area 190 @ 19344,-23440
- #191 area 191 @ 6448,6096
- #192 area 192 @ 5680,-24784
- #193 area 193 @ -1200,1200
- #194 area 194 @ 464,2640
- #195 area 195 @ -20528,12624
- #196 area 196 @ -22992,-5872
- #197 area 197 @ -14128,560
- #198 area 198 @ -48,24112
- #199 area 199 @ 3504,4944
- #200 area 200 @ 19504,-18384
- #201 area 201 @ -208,-23504
- #202 area 202 @ 2480,11184
- #203 area 203 @ -3664,15568
- #204 area 204 @ 12496,-4944
- #205 area 205 @ -5424,-11984
- #206 area 206 @ -80,-32048
- #207 area 207 @ 19504,2640
- #208 area 208 @ -14928,-15408
- #209 area 209 @ 14608,-20944
- #210 area 210 @ 20272,-25904
- #211 area 211 @ 18032,-12976
- #212 area 212 @ 3408,-14640
- #213 area 213 @ -1840,-9904
- #214 area 214 @ 16720,-5680
- #215 area 215 @ -8144,9520
- #216 area 216 @ 4016,-27568
- #217 area 217 @ -2608,-1616
- #218 area 218 @ 20912,-29520
- #219 area 219 @ 19760,10704
- #220 area 220 @ -10960,-17520
- #221 area 221 @ -7376,-15920
- #222 area 222 @ 20656,-26384
- #223 area 223 @ -19184,-10352
- #224 area 224 @ -12016,-6832
- #225 area 225 @ 15024,10192
- #226 area 226 @ -14288,-8272
- #227 area 227 @ 11856,-17360
- #228 area 228 @ 9712,-15184
- #229 area 229 @ 8528,-20688
- #230 area 230 @ -4816,20176
- #231 area 231 @ 21168,14640
- #232 area 232 @ -16464,14160
- #233 area 233 @ 21680,10960
- #234 area 234 @ 12496,15280
- #235 area 235 @ -17968,-11184
- #236 area 236 @ -18512,3664
- #237 area 237 @ 19792,-7984
- #238 area 238 @ 12464,-12752
- #239 area 239 @ -13744,-336
- #240 area 240 @ 7472,6608
- #241 area 241 @ 18448,-23568
- #242 area 242 @ -2736,19120
- #243 area 243 @ -13968,-7792
- #244 area 244 @ 19408,-20080
- #245 area 245 @ 10320,-23760
- #246 area 246 @ -5808,-30032
- #247 area 247 @ 4176,14864
- #248 area 248 @ -13040,2448

## Edges behind rocks

- Grizzly Hills <-> Venture Bay (cost 4011, rock group 0)
- Zul'Drak <-> Drak'Tharon Keep (cost 4209, rock group 8)
- Crystalsong Forest <-> area 198 (cost 4220, rock group 7)
- Stonemaul <-> area 243 (cost 3943, rock group 1)
- Dun Modr <-> Arathi Highlands (cost 3134, rock group 3)
- Orgrimmar <-> Durotar (cost 4247, rock group 2)
- Venture Bay <-> area 247 (cost 3131, rock group 0)
- Scarab Wall <-> Hive'Zora (cost 3601, rock group 6)
