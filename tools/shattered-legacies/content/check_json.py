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
print(f"json check: {len(files)} files, {bad} type mismatches")
sys.exit(1 if bad else 0)
