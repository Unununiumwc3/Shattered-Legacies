"""Map data JSON check: every modification's value must match its type, or the CLI's publish step stops with
"The JSON value could not be converted ... requires an element of type 'Number'" (0.57.0: a requirement written
without "Type": 3). Types in WL's JSON: none / 0 = integer, 1 = real, 2 = unreal, 3 = text.
Usage (repo root): python3 tools/shattered-legacies/content/check_json.py   -- run after every map data change
"""
import glob
import json
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", ".."))
bad = 0
files = glob.glob(os.path.join(ROOT, "mapdata", "WarcraftLegacies", "*Data", "*.json"))
for f in files:
    d = json.load(open(f, encoding="utf-8-sig"))
    for m in d.get("Modifications", []):
        t, v = m.get("Type"), m.get("Value")
        ok = (isinstance(v, int) and not isinstance(v, bool)) if t in (None, 0) else \
            isinstance(v, str) if t == 3 else isinstance(v, (int, float))
        if not ok:
            bad += 1
            print(f"{os.path.relpath(f, ROOT)}: {m['Id']} Type={t} Value={v!r}")
# 0.63: a field's Type must be the one the map uses for that field everywhere else (a scale written as unreal, Type 2,
# where WL writes real, Type 1 -- 0.61-0.62's Oil Refinery). Fields with one dominant type (>= 95 %, >= 20 uses).
from collections import Counter, defaultdict
seen = defaultdict(Counter)
where = []
for f in files:
    for m in json.load(open(f, encoding="utf-8-sig")).get("Modifications", []):
        t = m.get("Type") or 0
        seen[m["Id"]][t] += 1
        where.append((f, m["Id"], t))
odd = 0
for f, fid, t in where:
    c = seen[fid]
    top, n = c.most_common(1)[0]
    if t != top and n >= 20 and n >= 0.95 * sum(c.values()):
        odd += 1
        bad += 1
        print(f"{os.path.relpath(f, ROOT)}: {fid} Type={t}, the map uses Type={top} ({n} of {sum(c.values())})")
print(f"json check: {len(files)} files, {bad} type mismatches ({odd} unusual field types)")
sys.exit(1 if bad else 0)
