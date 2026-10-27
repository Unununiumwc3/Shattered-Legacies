"""Generates the bot's route graph from MAP DATA ONLY (0.42, refactor phase 3) -- so it can run on any WL version.

  python3 tools/ai/gen_routes.py        (repo root; a few minutes)  -> src/WarcraftLegacies.Source/Ai/AiRoutes.g.cs
                                                              + src/WarcraftLegacies.Source/Ai/ROUTES.md (report)
Nodes
  CP         every preplaced unit whose type carries an income marker ability (an ability whose buff is one of the
             CP buffs listed in Setup/ControlPointSetup.cs) -- the same rule WL uses at runtime.
  area       walkable ground farther than AREA_GAP (walking) from every other node gets a node (farthest point
             first, until no such ground is left): harbours, camps, bases without a CP. At runtime an area node is
             a target only while enemy STRUCTURES stand near it (replaces the bot-only CPs, WORKAROUNDS W1-W4).
             (0.42 first tried clusters of preplaced player units: those bases belong to neutral players in the map
             data and are handed over by WL at runtime -- only 1 cluster was found.)
Edges
  Walkable grid (WC3's 32-unit pathing cells) from PathingMap.json (terrain), minus trees ('?T??' destructables and custom ones
  based on them) and rock doodads ('?Rrk'). Multi-source Dijkstra from all nodes; two nodes are connected when their
  areas touch; cost = walking distance through the cheapest touching point. Water separates landmasses (portals are
  added at runtime from the live waygates).
  WL's removable rock blockers ('LTrc', Setup/RockSetup.cs) count as walkable, but every edge whose path crosses them
  remembers the rock group -> closed at runtime while those rocks stand.
Region map (0.42.1): which node's area every 256 x 256 cell belongs to (majority of its walkable 32-unit cells),
  two letters per cell -- the bot starts a route in the area its group STANDS in (not the nearest node in a straight
  line, which can be across a chasm or a mountain) and ignores enemies it cannot walk to.
Known gaps (v1): other doodads (archways, walls) and units (gates) are not in the grid.

  python3 tools/ai/gen_routes.py --without n0SA,n0TD,n0RF   report only (nothing written): the graph as if these CPs
                                                            did not exist -- which objective nodes would replace them
"""
import glob
import heapq
import json
import math
import os
import re
import struct
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
OUT = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "AiRoutes.g.cs")
REPORT = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "ROUTES.md")
CP_SETUP = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Setup", "ControlPointSetup.cs")
AICONFIG = os.path.join(ROOT, "src", "WarcraftLegacies.Source", "Ai", "AiConfig.cs")
MINX, MINY = -28672.0, -32256.0
C = 32                      # grid cell size = WC3's pathing cell (64 closed city entrances: trees fill whole cells)
TREE_HALF = 32              # a tree blocks 64 x 64 (its pathing texture)
ROCK_HALF = 64              # a rock doodad blocks 128 x 128
GATE_ROCK_HALF = 96         # LTrc: marked as a removable blocker in this box
GATE_GROUP = 700            # LTrc within this distance form one rock group
AREA_GAP = 3000             # walking distance from the nearest node that gets an area node
RC = 256                    # region map cell
LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"


def le(v):
    return struct.pack("<I", v).decode("latin1")


def be(c):
    return struct.unpack(">I", c.encode("latin1"))[0]


def load(path):
    return json.load(open(path, encoding="utf-8-sig"))


def mods(d):
    return {m.get("Id"): m.get("Value") for m in d.get("Modifications", []) if isinstance(m, dict)}


def cp_types():
    src = open(CP_SETUP, encoding="utf-8-sig").read()
    buffs = set(re.findall(r"BUFF_(B\w{3})_", src)) | set(re.findall(r'FourCC\("(B\w{3})"\)', src))
    markers = set()
    for f in glob.glob(os.path.join(MAP, "AbilityData", "*.json")):
        if mods(load(f)).get("abuf") in buffs:
            markers.add(os.path.basename(f)[:4])
    types = set()
    for f in glob.glob(os.path.join(MAP, "UnitData", "*.json")):
        abil = mods(load(f)).get("uabi") or ""
        if any(a.strip() in markers for a in str(abil).split(",")):
            types.add(os.path.basename(f)[:4])
    return types, buffs, markers


def preplaced():
    out = []
    for f in sorted(glob.glob(os.path.join(MAP, "Units", "*.json"))):
        for u in load(f):
            out.append((le(u["TypeId"]), u["Position"]["X"], u["Position"]["Y"], u.get("OwnerId", -1),
                        u.get("CreationNumber", 0)))
    out.sort(key=lambda t: t[4])
    return out


def tree_like():
    """destructable type codes that are trees: '?T??' or custom destructables based on one."""
    custom = set()
    for f in glob.glob(os.path.join(MAP, "DestructableData", "*.json")):
        d = load(f)
        old = d.get("OldId") or ""
        if len(old) == 4 and old[1] == "T":
            custom.add(os.path.basename(f)[:4])
    return custom


def grid():
    d = load(os.path.join(MAP, "PathingMap.json"))
    w, h, cells = d["Width"], d["Height"], d["Cells"]
    return w, h, bytearray(1 if (c & 2) == 0 else 0 for c in cells)


def mark(walk, gw, gh, x, y, half, val, tag=None, tags=None):
    """box of +-half around (x, y): set walkability to val, or (tags given) tag the walkable cells"""
    c0, c1 = int((x - half - MINX) // C), int((x + half - 1 - MINX) // C)
    r0, r1 = int((y - half - MINY) // C), int((y + half - 1 - MINY) // C)
    for r in range(max(r0, 0), min(r1, gh - 1) + 1):
        for c in range(max(c0, 0), min(c1, gw - 1) + 1):
            if tags is not None:
                if walk[r * gw + c]:
                    tags[r * gw + c] = tag
            else:
                walk[r * gw + c] = val


def nearest_walkable(walk, gw, gh, x, y, maxr=24):
    c0, r0 = int((x - MINX) // C), int((y - MINY) // C)
    for k in range(maxr + 1):
        best = None
        for r in range(r0 - k, r0 + k + 1):
            for c in range(c0 - k, c0 + k + 1):
                if max(abs(r - r0), abs(c - c0)) != k or not (0 <= c < gw and 0 <= r < gh):
                    continue
                if walk[r * gw + c]:
                    d = (c - c0) ** 2 + (r - r0) ** 2
                    if best is None or d < best[0]:
                        best = (d, r * gw + c)
        if best:
            return best[1]
    return None


def hand_edges():
    src = open(AICONFIG, encoding="utf-8").read()
    return {(min(a, b), max(a, b)) for a, b in
            ((int(m.group(1)), int(m.group(2))) for m in re.finditer(r"new Edge\((\d+),\s*(\d+)", src))}


def main():
    without = set()
    if "--without" in sys.argv:
        without = set(sys.argv[sys.argv.index("--without") + 1].split(","))
    cps, buffs, markers = cp_types()
    cps -= without
    units = preplaced()
    tree_custom = tree_like()
    gw, gh, walk = grid()
    print(f"grid {gw}x{gh} ({C}-unit cells), walkable {sum(walk)}; CP buffs {sorted(buffs)}, markers {len(markers)}, "
          f"CP types {len(cps)}")

    # blockers
    gate_rocks = []
    ntrees = nrocks = 0
    for f in glob.glob(os.path.join(MAP, "Doodads", "*.json")):
        for d in load(f):
            code = le(d["TypeId"])
            x, y = d["Position"]["X"], d["Position"]["Y"]
            if code == "LTrc":
                gate_rocks.append((x, y))
            elif (len(code) == 4 and code[1] == "T") or code in tree_custom:
                mark(walk, gw, gh, x, y, TREE_HALF, 0)
                ntrees += 1
            elif code.endswith("Rrk"):
                mark(walk, gw, gh, x, y, ROCK_HALF, 0)
                nrocks += 1
    # rock groups
    groups = []
    for x, y in gate_rocks:
        for gidx, g in enumerate(groups):
            if any((x - a) ** 2 + (y - b) ** 2 <= GATE_GROUP ** 2 for a, b in g):
                g.append((x, y))
                break
        else:
            groups.append([(x, y)])
    rocktag = [-1] * (gw * gh)
    for gidx, g in enumerate(groups):
        for x, y in g:
            mark(walk, gw, gh, x, y, GATE_ROCK_HALF, 1)  # the rocks themselves: walkable for generation
            mark(walk, gw, gh, x, y, GATE_ROCK_HALF, None, gidx, rocktag)
    print(f"blockers: {ntrees} trees, {nrocks} rock doodads, {len(gate_rocks)} LTrc in {len(groups)} groups")

    # nodes
    nodes = []  # (kind, code, x, y, name)
    for code, x, y, owner, _ in units:
        if code in cps:
            name = mods(load(os.path.join(MAP, "UnitData", code + ".json"))).get("unam") or code
            nodes.append((0, code, x, y, name))
    seen = {}
    nodes = [n for n in nodes if not (n[1] in seen or seen.setdefault(n[1], True) is None)]
    # multi-source Dijkstra from the CPs, then area nodes at the farthest walkable cell until all are within AREA_GAP
    INF = float("inf")
    dist = [INF] * (gw * gh)
    label = [-1] * (gw * gh)
    rk = [-1] * (gw * gh)  # rock group crossed on the way from the source (first one)
    steps = [(-1, 0, C), (1, 0, C), (0, -1, C), (0, 1, C),
             (-1, -1, C * 1.4142), (1, -1, C * 1.4142), (-1, 1, C * 1.4142), (1, 1, C * 1.4142)]

    def grow(seeds):
        pq = []
        for i, cidx in seeds:
            if dist[cidx] > 0:
                dist[cidx] = 0.0
                label[cidx] = i
                rk[cidx] = -1
                heapq.heappush(pq, (0.0, cidx))
        while pq:
            dcur, cidx = heapq.heappop(pq)
            if dcur > dist[cidx]:
                continue
            r, c = divmod(cidx, gw)
            for dc, dr, w in steps:
                nc, nr = c + dc, r + dr
                if not (0 <= nc < gw and 0 <= nr < gh):
                    continue
                n = nr * gw + nc
                if not walk[n]:
                    continue
                if dc and dr and not (walk[r * gw + nc] and walk[nr * gw + c]):
                    continue  # no corner cutting
                nd = dcur + w
                if nd < dist[n]:
                    dist[n] = nd
                    label[n] = label[cidx]
                    rk[n] = rk[cidx] if rk[cidx] >= 0 else rocktag[n]
                    heapq.heappush(pq, (nd, n))

    seeds = []
    for i, (_, _, x, y, _) in enumerate(nodes):
        cidx = nearest_walkable(walk, gw, gh, x, y)
        if cidx is not None:
            seeds.append((i, cidx))
    grow(seeds)
    while True:
        far, at = 0.0, -1
        for cidx in range(gw * gh):
            d = dist[cidx]
            if far < d < INF:
                far, at = d, cidx
        if far <= AREA_GAP:
            break
        r, c = divmod(at, gw)
        nodes.append((1, "", round(MINX + (c + 0.5) * C), round(MINY + (r + 0.5) * C), f"area {len(nodes)}"))
        grow([(len(nodes) - 1, at)])
    print(f"nodes: {sum(1 for n in nodes if n[0] == 0)} CPs, {sum(1 for n in nodes if n[0] == 1)} area nodes")

    # edges where areas touch
    best = {}
    for cidx in range(gw * gh):
        a = label[cidx]
        if a < 0:
            continue
        r, c = divmod(cidx, gw)
        for dc, dr, w in steps[:4] + steps[5:6] + steps[7:8]:
            nc, nr = c + dc, r + dr
            if not (0 <= nc < gw and 0 <= nr < gh):
                continue
            n = nr * gw + nc
            b = label[n]
            if b < 0 or b == a:
                continue
            cost = dist[cidx] + w + dist[n]
            key = (min(a, b), max(a, b))
            rock = rk[cidx] if rk[cidx] >= 0 else (rk[n] if rk[n] >= 0 else rocktag[n])
            if key not in best or cost < best[key][0]:
                best[key] = (cost, rock, cidx)
    edges = sorted((a, b, round(cost), rock) for (a, b), (cost, rock, _) in best.items())
    via = {(a, b): (round(MINX + (v % gw + 0.5) * C), round(MINY + (v // gw + 0.5) * C))
           for (a, b), (_, _, v) in best.items()}
    print(f"edges: {len(edges)} ({sum(1 for e in edges if e[3] >= 0)} behind rocks)")

    # compare with the hand-written graph
    hand = hand_edges()
    idx = {be(code): i for i, (k, code, _, _, _) in enumerate(nodes) if k == 0}
    gen_cp = {(min(be(nodes[a][1]), be(nodes[b][1])), max(be(nodes[a][1]), be(nodes[b][1])))
              for a, b, _, _ in edges if nodes[a][0] == 0 and nodes[b][0] == 0}
    both = hand & gen_cp
    missing = sorted(hand - gen_cp)
    reach = check_reach(nodes, edges, missing, idx)
    if without:
        print(f"--without {sorted(without)}: area nodes:")
        for n in nodes:
            if n[0] == 1:
                print(f"  {n[4]} @ {n[2]},{n[3]}")
        return
    write_cs(nodes, edges, groups, regions(label, gw, gh))
    write_report(nodes, edges, groups, hand, both, missing, idx, reach, via)
    globals()["LAST"] = (nodes, edges, via, walk, gw, gh)
    print(f"hand-written edges: {len(hand)}, also generated: {len(both)}, not generated: {len(missing)}")
    print(f"wrote {os.path.relpath(OUT, ROOT)} and {os.path.relpath(REPORT, ROOT)}")


def check_reach(nodes, edges, missing, idx):
    """for every hand-written edge that was not generated: is B reachable from A, and how far compared to the
    straight line? -> {(a, b): ratio or None}"""
    adj = {i: [] for i in range(len(nodes))}
    for a, b, cost, _ in edges:
        adj[a].append((b, cost))
        adj[b].append((a, cost))
    res = {}
    for a, b in missing:
        if a not in idx or b not in idx:
            res[(a, b)] = "no CP"
            continue
        i, j = idx[a], idx[b]
        dist = {i: 0}
        pq = [(0, i)]
        while pq:
            d, u = heapq.heappop(pq)
            if u == j:
                break
            if d > dist[u]:
                continue
            for v, c in adj[u]:
                if d + c < dist.get(v, float("inf")):
                    dist[v] = d + c
                    heapq.heappush(pq, (d + c, v))
        straight = math.hypot(nodes[i][2] - nodes[j][2], nodes[i][3] - nodes[j][3]) or 1
        res[(a, b)] = round(dist[j] / straight, 1) if j in dist else None
    ok = sum(1 for v in res.values() if isinstance(v, float) and v <= 2.0)
    far = sum(1 for v in res.values() if isinstance(v, float) and v > 2.0)
    print(f"not generated: {ok} reachable at <= 2x the straight line, {far} only by a detour > 2x, "
          f"{sum(1 for v in res.values() if v is None)} unreachable on foot (portal / ship / wall)")
    return res


def regions(label, gw, gh):
    k = RC // C
    rw, rh = gw // k, gh // k
    out = []
    for r in range(rh):
        for c in range(rw):
            cnt = {}
            for rr in range(r * k, r * k + k):
                base = rr * gw
                for cc in range(c * k, c * k + k):
                    lab = label[base + cc]
                    if lab >= 0:
                        cnt[lab] = cnt.get(lab, 0) + 1
            if cnt:
                lab = max(sorted(cnt), key=lambda v: cnt[v])
                out.append(LETTERS[lab // 52] + LETTERS[lab % 52])
            else:
                out.append("..")
    return rw, rh, "".join(out)


def chunk(vals, per=16):
    vals = [str(v) for v in vals]
    return "\n".join("      " + ", ".join(vals[i:i + per]) + "," for i in range(0, len(vals), per))


def write_cs(nodes, edges, groups, reg):
    gx = [round(sum(x for x, _ in g) / len(g)) for g in groups]
    gy = [round(sum(y for _, y in g) / len(g)) for g in groups]
    gr = [round(max(math.hypot(x - cx, y - cy) for x, y in g) + 200) for g, cx, cy in zip(groups, gx, gy)]
    arr = [
        ("NodeType", "CP unit type (big-endian id); 0 = area node", [be(n[1]) if n[0] == 0 else 0 for n in nodes]),
        ("NodeX", "", [round(n[2]) for n in nodes]),
        ("NodeY", "", [round(n[3]) for n in nodes]),
        ("EdgeA", "node index", [e[0] for e in edges]),
        ("EdgeB", "node index", [e[1] for e in edges]),
        ("EdgeCost", "walking distance through the cheapest touching point", [e[2] for e in edges]),
        ("EdgeRocks", "rock group that closes the edge while it stands, -1 = none", [e[3] for e in edges]),
        ("RockX", "rock group centre", gx),
        ("RockY", "", gy),
        ("RockR", "radius that holds all LTrc rocks of the group", gr),
    ]
    out = ["// <auto-generated>",
           "// Generated by tools/ai/gen_routes.py from map data only (PathingMap, Units, Doodads, UnitData,",
           "// AbilityData, the CP buffs in Setup/ControlPointSetup.cs). DO NOT EDIT -- re-run the generator.",
           "// </auto-generated>",
           "namespace WarcraftLegacies.Source.Ai",
           "{",
           "  internal static class AiRouteData",
           "  {",
           f"    internal const int Nodes = {len(nodes)};",
           f"    internal const int Edges = {len(edges)};",
           ""]
    for name, comment, vals in arr:
        if comment:
            out.append(f"    // {comment}")
        out.append(f"    internal static readonly int[] {name} =")
        out.append("    {")
        if vals:
            out.append(chunk(vals))
        out.append("    };")
        out.append("")
    rw, rh, text = reg
    out += [f"    // region map: {RC}-unit cells, {rw} x {rh}, rows bottom-up from ({int(MINX)}, {int(MINY)}); per cell two letters",
            "    // = node index (A-Z a-z, base 52), \"..\" = no walkable ground",
            f"    internal const float RegionMinX = {MINX}f;",
            f"    internal const float RegionMinY = {MINY}f;",
            f"    internal const int RegionCell = {RC};",
            f"    internal const int RegionW = {rw};",
            f"    internal const int RegionH = {rh};",
            "    internal const string Regions ="]
    out.append(f'      "{text}";')  # ONE literal, like Geography.Grid: a long "+" chain can exceed Lua's parser limits
    out += ["  }", "}", ""]
    open(OUT, "w", encoding="utf-8", newline="\n").write("\n".join(out))


def write_report(nodes, edges, groups, hand, both, missing, idx, reach, via):
    name = {be(n[1]): n[4] for n in nodes if n[0] == 0}
    lines = ["# Route graph (generated)", "",
             "Written by `python3 tools/ai/gen_routes.py` together with `AiRoutes.g.cs`. Review this after every run.", "",
             f"- Nodes: {sum(1 for n in nodes if n[0] == 0)} CPs, {sum(1 for n in nodes if n[0] == 1)} area nodes",
             f"- Edges: {len(edges)}, of them {sum(1 for e in edges if e[3] >= 0)} behind rock groups "
             f"({len(groups)} groups of `LTrc`)",
             f"- Hand-written graph (`AiConfig`): {len(hand)} edges, {len(both)} of them also generated.", "",
             "## Hand-written edges NOT generated", "",
             "A missing direct edge is fine when the generated graph still walks from A to B (through another",
             "node) at a similar cost. \"unreachable\" = different landmass: waygate / ship / portal (added at",
             "runtime from live waygates) -- or a wall the generator does not know. \"detour Nx\" = the generated",
             "walk is N times the straight line: check whether something blocks it that does not block in game.", ""]
    for a, b in missing:
        r = reach.get((a, b))
        tag = "unreachable on foot" if r is None else ("not a CP in the map data" if r == "no CP"
                                                        else ("ok, " if r <= 2.0 else "detour ") + f"{r}x")
        lines.append(f"- {name.get(a, '?' + le(a)[::-1])} <-> {name.get(b, '?' + le(b)[::-1])}: {tag}")
    lines += ["", "## Area nodes (targets only while enemy structures stand near them)", ""]
    for i, n in enumerate(nodes):
        if n[0] == 1:
            lines.append(f"- #{i} {n[4]} @ {n[2]},{n[3]}")
    lines += ["", "## All edges (A <-> B, walking cost, where the two areas touch)", "",
              "Check suspicious ones in the editor at the 'via' point: a wall, cliff or mountain the generator missed.", ""]
    for a, b, cost, rock in edges:
        vx, vy = via[(a, b)]
        lines.append(f"- {nodes[a][4]} <-> {nodes[b][4]}: {cost} via {vx},{vy}" + (f" (rocks {rock})" if rock >= 0 else ""))
    lines += ["", "## Edges behind rocks", ""]
    for a, b, cost, rock in edges:
        if rock >= 0:
            lines.append(f"- {nodes[a][4]} <-> {nodes[b][4]} (cost {cost}, rock group {rock})")
    open(REPORT, "w", encoding="utf-8", newline="\n").write("\n".join(lines) + "\n")


if __name__ == "__main__":
    main()
