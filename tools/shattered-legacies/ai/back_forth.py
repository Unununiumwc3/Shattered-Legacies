"""Back-and-forth report from one game's dumps (0.48.3).

Usage: python3 tools/shattered-legacies/ai/back_forth.py WLAI_dump_*.txt            (overview)
       python3 tools/shattered-legacies/ai/back_forth.py WLAI_dump_*.txt --unit U3  (one followed unit's whole trail)
       python3 tools/shattered-legacies/ai/back_forth.py WLAI_dump_*.txt --bot 11   (one bot's target changes)

Every dump keeps only the last ~300 event-log lines and ~700 trail lines; this stitches the EVENT LOG and TRAIL
sections of all dumps of a game (with -autodump: every 5 min, nothing lost) into one timeline. Reports:
- target changes by reason; TURN-BACKS = a new target more than 120 deg away from the old one. 0.48.3 dumps carry the
  angle seen from the army ("at 160deg"); older dumps fall back to target-to-target geometry (CP targets only);
- defence episodes: duration and how soon the next one started (ping-pong);
- the unit trail (one bot, -track): summaries of the followed units; --unit prints one unit's lines.
"""
import collections
import math
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from dump_summary import decode  # noqa: E402

T = re.compile(r"^(\d+):(\d\d) ")
TARGET = re.compile(r"^(\d+:\d\d) P(\d+) ([^:\[]+?)( \[theatre (\d)\])?: target (.*?) -> (.*?) \[(.*?)\]( \| army @(-?\d+),(-?\d+)"
                    r"(?: new (\d+))?(?: old (\d+) at (\d+)deg)?)?$")
STATE = re.compile(r"^(\d+:\d\d) P(\d+) [^:]+: state (\w+) -> (\w+)")
NODE = re.compile(r"^\d+ \w{4} (.*) @(-?\d+),(-?\d+) lm=")


def secs(tag):
    m = T.match(tag + " ")
    return int(m.group(1)) * 60 + int(m.group(2)) if m else -1


def stitched(paths, section):
    seen, out = set(), []
    for p in sorted(paths, key=os.path.getmtime):
        on = False
        for ln in decode(p) or []:
            if ln.startswith("## "):
                on = ln.startswith("## " + section)
                continue
            if on and T.match(ln) and ln not in seen:
                seen.add(ln)
                out.append(ln)
    return sorted(out, key=lambda ln: secs(ln.split(" ")[0]))


def section_lines(path, section):
    out, on = [], False
    for ln in decode(path) or []:
        if ln.startswith("## "):
            on = ln.startswith("## " + section)
            continue
        if on:
            out.append(ln)
    return out


def cp_coords(paths):
    c = {}
    for p in paths:
        for ln in section_lines(p, "NODES"):
            m = NODE.match(ln)
            if m:
                c[m.group(1).strip()] = (int(m.group(2)), int(m.group(3)))
    return c


def strip_gold(name):
    return re.sub(r" \(\d+ gold/min\)", "", name).strip()


def angle(v1, v2):
    l1, l2 = math.hypot(*v1), math.hypot(*v2)
    if l1 < 1 or l2 < 1:
        return None
    c = max(-1.0, min(1.0, (v1[0] * v2[0] + v1[1] * v2[1]) / (l1 * l2)))
    return math.degrees(math.acos(c))


def cause(prev_reason, reason):
    if "defend" in (prev_reason, reason):
        return "defence in/out"
    if reason in ("hold-nothing-reachable", "wait-at-rocks", "hold-nothing-in-theatre"):
        return "hold/wait"
    if reason == "route-neutral" and prev_reason == "route-neutral":
        return "neutral hop"
    return "route re-pick"


def overview(paths, bot=None):
    ev = stitched(paths, "EVENT LOG")
    coords = cp_coords(paths)
    changes = collections.defaultdict(list)  # (pid, theatre) -> [(t, old, new, reason, army-angle)]
    for ln in ev:
        m = TARGET.match(ln)
        if m:
            full = re.sub(r"\(.*", "", m.group(8))
            reason, _, why = full.partition(":")  # 0.48.4: "route-neutral:switch" (gone / switch / first)
            ang = int(m.group(14)) if m.group(14) else None
            changes[(int(m.group(2)), m.group(5) or "1")].append((secs(m.group(1)), m.group(6), m.group(7), reason, ang, why))
    total = sum(len(v) for v in changes.values())
    print(f"stitched event log: {len(ev)} lines, {total} target changes")
    print("by reason:", dict(collections.Counter(c[3] for v in changes.values() for c in v).most_common()))
    whys = collections.Counter(c[5] for v in changes.values() for c in v if c[5])
    if whys:
        print("route changes by why (0.48.4: gone = target taken/lost, switch = re-plan found a better one):", dict(whys))

    turns, measured, how = collections.Counter(), 0, collections.Counter()
    for (pid, th), seq in sorted(changes.items()):
        if th != "1":
            continue
        for i in range(1, len(seq)):
            t, old, new, reason, ang, why = seq[i]
            if ang is None:  # pre-0.48.3: target-to-target geometry, CP targets only
                if i < 2:
                    continue
                a, b, c = (coords.get(strip_gold(seq[k][2])) for k in (i - 2, i - 1, i))
                if not (a and b and c):
                    continue
                v1, v2 = (b[0] - a[0], b[1] - a[1]), (c[0] - b[0], c[1] - b[1])
                if math.hypot(*v1) < 1500 or math.hypot(*v2) < 1500:
                    continue
                ang = angle(v1, v2)
                how["target-to-target"] += 1
            else:
                how["from the army"] += 1
            measured += 1
            if ang is not None and ang > 120:
                c = cause(seq[i - 1][3], reason)
                if why and c in ("neutral hop", "route re-pick"):
                    c = c + " (" + why + ")"
                key = (c, "0-10m" if t < 600 else "10m+")
                turns[key] += 1
                if bot is not None and pid == bot:
                    print(f"  P{pid} {t // 60}:{t % 60:02d} {old[:24]} -> {new[:24]} {ang:.0f}deg [{reason}]")
    print(f"turn-backs (>120 deg) of {measured} measured changes ({dict(how)}):")
    for k, n in sorted(turns.items()):
        print(f"  {k[0]:<26} {k[1]:<6} {n}")

    starts, durations, regaps, last_end = {}, [], [], {}
    for ln in ev:
        m = STATE.match(ln)
        if not m:
            continue
        pid, a, b, t = m.group(2), m.group(3), m.group(4), secs(m.group(1))
        if b == "def":
            starts[pid] = t
            if pid in last_end:
                regaps.append(t - last_end[pid])
        if a == "def" and pid in starts:
            durations.append(t - starts.pop(pid))
            last_end[pid] = t

    def bucket(xs, edges):
        out = collections.Counter()
        for x in xs:
            out[next((f"<={e}s" for e in edges if x <= e), f">{edges[-1]}s")] += 1
        return dict(out)

    print(f"defence episodes {len(durations)}: duration {bucket(durations, [40, 90])}; next defence after "
          f"{bucket(regaps, [60, 120])}")
    if bot is not None:
        for ln in ev:
            if re.match(rf"^\d+:\d\d P{bot} ", ln) and (" target " in ln or " state " in ln):
                print("   ", ln)


def trail(paths, unit=None):
    lines = stitched(paths, "TRAIL")
    if not lines:
        print("no TRAIL lines (dumps before 0.48.3, or -track off)")
        return
    last = max(paths, key=os.path.getmtime)
    if unit is None:
        print(f"trail: {len(lines)} lines stitched; followed units (last dump):")
        for ln in section_lines(last, "TRAIL"):
            if ln.startswith("now ") or ln.startswith("done "):
                print("  " + ln)
        orders = collections.Counter(m.group(1) + (" BACK" if m.group(2) else "")
                                     for m in (re.search(r" ord (\S+)( BACK)?", ln) for ln in lines) if m)
        print("trail orders:", dict(orders.most_common()))
        tc = collections.Counter(m.group(1) for m in (re.search(r" TURN \d+deg .* cause (\S+)", ln) for ln in lines) if m)
        print("trail turns by cause:", dict(tc.most_common()))
        return
    tag = re.compile(rf"(^|\s){re.escape(unit)}(\s|$)")
    for ln in lines:
        body = ln.split(" ", 1)[1] if " " in ln else ln
        if body.startswith("pos"):
            m = re.search(rf"{re.escape(unit)} (-?\d+,-?\d+) (\S+) (\S)", body)
            if m:
                print(f"{ln.split(' ')[0]}   {unit} @{m.group(1)} {m.group(2)} {m.group(3)}")
        elif tag.search(body):
            print(ln)


def main():
    args = sys.argv[1:]
    unit = bot = None
    if "--unit" in args:
        i = args.index("--unit")
        unit = args[i + 1]
        del args[i:i + 2]
    if "--bot" in args:
        i = args.index("--bot")
        bot = int(args[i + 1])
        del args[i:i + 2]
    paths = [p for p in args if os.path.isfile(p)]
    if not paths:
        print(__doc__)
        return
    if unit:
        trail(paths, unit)
        return
    overview(paths, bot)
    print()
    trail(paths)


if __name__ == "__main__":
    main()
