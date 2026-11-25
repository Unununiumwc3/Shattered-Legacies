"""Rebuilds the Chinese object-text overlays (mapdata/WarcraftLegacies/<Kind>Data.zhCN/) from WL 5.0.4's (0.75.0).

The overlays are sparse: a field replaces the base field of the same object (MapDataToMapConverter.ReadObjectRecord).
A WL 5.0.4 translation is taken for a field only when it still describes THIS map's object:
  - the object exists here (an overlay without a base file would CREATE a text-only object in the Chinese build),
  - its base (OldId) is the same as WL 5.0.4's,
  - and WL 5.0.4's ENGLISH value of that field equals this map's English value (both absent counts: the text comes
    from the stock game and so does the translation).
Everything else stays English. WL 5 objects that live under a fork id here (ID collisions, UPSTREAM.md) take their
translation from WL 5's id, and every "<id," tooltip reference to such an id is rewritten to the fork id.

  python3 tools/shattered-legacies/localization/build_overlays.py --wl <WL 5.0.4 mapdata/WarcraftLegacies>
  python3 tools/shattered-legacies/localization/build_overlays.py --wl <...> --check   (report only)

Get the WL tree with: git -C <WL repo> archive a0c73082 mapdata/WarcraftLegacies | tar -x -C /tmp/wl5
"""
import argparse
import json
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", "..", ".."))
MAP = os.path.join(ROOT, "mapdata", "WarcraftLegacies")
KINDS = ["AbilityData", "UnitData", "UpgradeData", "BuffData", "ItemData", "DestructableData"]

# WL 5.0.4 id -> this map's id (the same WL 5 object). Keep in step with UPSTREAM.md "ID collisions".
MOVED = {"A000": "AZo0", "A00Y": "AZo1", "R003": "RZo0", "RZ02": "RZo1", "R05A": "RZn0"}
# ids whose WL 5 object is NOT this map's object under that id (restored 4.8.2 data): never take WL 5's text
COLLIDED = set(MOVED) | {"n00K"}


def load(path):
    with open(path, encoding="utf-8-sig") as f:
        return json.load(f)


def key(m):
    return (m.get("Id"), m.get("Level", 0) or 0, m.get("Pointer", 0) or 0)


def fields(record):
    return {key(m): m.get("Value") for m in record.get("Modifications", [])}


TOKEN = re.compile(r"<(" + "|".join(MOVED) + r"),")


def remap(value):
    return TOKEN.sub(lambda m: "<" + MOVED[m.group(1)] + ",", value) if isinstance(value, str) else value



def build(wl, check):
    report = {}
    for kind in KINDS:
        src_dir = os.path.join(wl, kind + ".zhCN")
        out_dir = os.path.join(MAP, kind + ".zhCN")
        written = {}
        kept_fields = dropped_fields = 0
        for name in sorted(os.listdir(src_dir)):
            if not name.endswith(".json"):
                continue
            wl_id = name[:-5]
            fork_id = MOVED.get(wl_id, wl_id)
            if wl_id in COLLIDED and wl_id not in MOVED:
                continue
            base_path = os.path.join(MAP, kind, fork_id + ".json")
            wl_base_path = os.path.join(wl, kind, wl_id + ".json")
            if not os.path.exists(base_path) or not os.path.exists(wl_base_path):
                continue
            base, wl_base, zh = load(base_path), load(wl_base_path), load(os.path.join(src_dir, name))
            if base.get("OldId") != wl_base.get("OldId"):
                continue
            here, there = fields(base), fields(wl_base)
            mods = []
            for m in zh.get("Modifications", []):
                k = key(m)
                if not isinstance(m.get("Value"), str) or remap(there.get(k)) != here.get(k):
                    dropped_fields += 1
                    continue
                m = dict(m)
                m["Value"] = remap(m["Value"])
                mods.append(m)
            if not mods:
                continue
            kept_fields += len(mods)
            out = dict(zh)  # WL's layout: custom objects carry NewId, stock overrides do not
            out["Modifications"] = mods
            if "NewId" in out:
                out["NewId"] = fork_id
            written[fork_id + ".json"] = out
        report[kind] = (len(written), kept_fields, dropped_fields)
        if check:
            continue
        old = set(os.listdir(out_dir)) if os.path.isdir(out_dir) else set()
        os.makedirs(out_dir, exist_ok=True)
        for name, rec in written.items():
            text = json.dumps(rec, indent=2, ensure_ascii=False) + "\n"
            path = os.path.join(out_dir, name)
            if os.path.exists(path) and open(path, encoding="utf-8-sig").read() == text:
                continue
            with open(path, "w", encoding="utf-8", newline="") as f:
                f.write(text)
        for name in old - set(written):
            os.remove(os.path.join(out_dir, name))
    for kind, (n, k, d) in report.items():
        print(f"{kind}.zhCN: {n} files, {k} fields translated, {d} fields left English (changed here / not text)")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--wl", required=True, help="WL 5.0.4 mapdata/WarcraftLegacies (extracted)")
    ap.add_argument("--check", action="store_true")
    a = ap.parse_args()
    build(a.wl, a.check)


if __name__ == "__main__":
    main()
