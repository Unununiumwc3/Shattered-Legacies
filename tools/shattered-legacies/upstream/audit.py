"""Three-way audit of this map against WL 4.8.2 (the fork's base) and WL 5.0.4 (0.77.0).

For every object file (Ability/Unit/Upgrade/Buff/Item/Destructable data) and every C# file it compares
  B = WL 4.8.2, W = WL 5.0.4, F = this repository
and sorts it:
  aligned            F == W
  take               F == B, W != B -- WL 5 changed it, the fork never touched it: a candidate to port
  fork-only change   F != B, W == B -- the fork's own change
  both changed       F != B, W != B, F != W -- look at it by hand
  collision?         same id, different object (base or name) in B and W -- never port blindly (UPSTREAM.md)
  wl5-deleted        gone in WL 5, still here (removed factions / features the fork keeps)
  missing            new in WL 5, not here
Fork ids (?Z??) are skipped. Get the trees with
  git -C <WL repo> archive 4.8.2 mapdata/WarcraftLegacies src | tar -x -C /tmp/wl482
  git -C <WL repo> archive a0c73082 mapdata/WarcraftLegacies src | tar -x -C /tmp/wl504
and run  python3 tools/shattered-legacies/upstream/audit.py --base /tmp/wl482 --wl /tmp/wl504 [--list take]
"""
import argparse
import collections
import hashlib
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
KINDS = ["AbilityData", "UnitData", "UpgradeData", "BuffData", "ItemData", "DestructableData"]
NAME_FIELDS = ("unam", "anam", "gnam", "fnam", "inam")


def load(p):
    try:
        with open(p, encoding="utf-8-sig") as f:
            return json.load(f)
    except FileNotFoundError:
        return None


def name(j):
    for m in (j or {}).get("Modifications", []):
        if m["Id"] in NAME_FIELDS:
            return m["Value"]
    return None


def norm(j):
    if j is None:
        return None
    return (j.get("OldId"), tuple(sorted((m["Id"], m.get("Level", 0) or 0, m.get("Pointer", 0) or 0,
                                          json.dumps(m.get("Value"))) for m in j["Modifications"])))


def objects(base, wl):
    res = collections.defaultdict(list)
    dirs = [os.path.join(base, "mapdata", "WarcraftLegacies"), os.path.join(wl, "mapdata", "WarcraftLegacies"),
            os.path.join(ROOT, "mapdata", "WarcraftLegacies")]
    for k in KINDS:
        ids = set()
        for d in dirs:
            p = os.path.join(d, k)
            if os.path.isdir(p):
                ids |= {f[:-5] for f in os.listdir(p) if f.endswith(".json")}
        for i in sorted(ids):
            if len(i) == 4 and i[1] == "Z":
                continue
            b, w, f = (load(os.path.join(d, k, i + ".json")) for d in dirs)
            nb, nw, nf = norm(b), norm(w), norm(f)
            key = k + "/" + i
            if nw == nf:
                res["aligned"].append(key)
            elif w is None:
                res["wl5-deleted" if f is not None else "gone"].append(key)
            elif f is None:
                res["missing" if b is None else "fork-deleted"].append(key)
            elif b is not None and (b.get("OldId") != w.get("OldId") or (name(b) != name(w) and name(f) == name(b))):
                res["collision?"].append(key)
            elif nb == nf:
                res["take"].append(key)
            elif nb == nw:
                res["fork-only change"].append(key)
            else:
                res["both changed"].append(key)
    return res


def digest(p):
    try:
        with open(p, "rb") as f:
            return hashlib.md5(f.read().replace(b"\r\n", b"\n")).hexdigest()
    except FileNotFoundError:
        return None


def code(base, wl):
    res = collections.defaultdict(list)
    roots = [os.path.join(base, "src"), os.path.join(wl, "src"), os.path.join(ROOT, "src")]
    files = set()
    for r in roots[:2]:
        for dp, _, fn in os.walk(r):
            files |= {os.path.relpath(os.path.join(dp, f), r) for f in fn if f.endswith(".cs")}
    for f in sorted(files):
        b, w, k = (digest(os.path.join(r, f)) for r in roots)
        if w == k:
            continue
        if w is None:
            res["wl5-deleted" if k else "gone"].append(f)
        elif k is None:
            res["missing" if b is None else "fork-deleted"].append(f)
        elif b == k:
            res["take"].append(f)
        elif b == w:
            res["fork-only change"].append(f)
        else:
            res["both changed"].append(f)
    return res


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", required=True, help="WL 4.8.2 tree (mapdata/ + src/)")
    ap.add_argument("--wl", required=True, help="WL 5.0.4 tree (mapdata/ + src/)")
    ap.add_argument("--list", default="", help="print the entries of this category")
    a = ap.parse_args()
    for title, res in (("object data", objects(a.base, a.wl)), ("C# code", code(a.base, a.wl))):
        print(title + ": " + ", ".join(f"{k} {len(v)}" for k, v in sorted(res.items())))
        if a.list:
            for e in res.get(a.list, []):
                print("  " + e)


if __name__ == "__main__":
    main()
