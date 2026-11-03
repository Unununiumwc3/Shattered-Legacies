"""Condenses the bot's -dump / -autodump files of ONE game into a short markdown block for
src/WarcraftLegacies.Source/Ai/MEASUREMENTS.md, so measurements stay in the repo and a new chat can read them.

Usage (from the repo root, on the copied CustomMapData files):
  python3 tools/shattered-legacies/ai/dump_summary.py ~/Downloads/game1/WLAI_dump_*.txt                 # print the block
  python3 tools/shattered-legacies/ai/dump_summary.py ~/Downloads/game1/WLAI_*.txt --append "1 human, 12 bots, Veteran"
  python3 tools/shattered-legacies/ai/dump_summary.py ... --real "t5=5:02 t10=10:30 t20=21:40"
     --real (0.46): the maintainer's stopwatch -- real time (m:ss) when the turn counter showed turn N. Gives the
     real seconds per game minute between the readings (the game falling behind real time).
     --append writes the block (with your note) to the end of MEASUREMENTS.md. The heartbeat file
     (WLAI_heartbeat.txt) may be passed too; it is shown separately (last game time, last tasks).

What it keeps: build, players, checkpoints at 10/20/30/45/60/90/120... min (the nearest dump), units on the map,
orders per minute, armies, team CPs, the LOAD section, every TICK ERROR / FAILED line once. Real seconds per game
minute come from the file times -- only meaningful when the copy kept the modification times.
"""
import os
import re
import sys

PRELOAD = re.compile(r'\s*call Preload\( "(.*)" \)')
HEADER = re.compile(r"=== WLAI DUMP build=(\S+) t=(\S+)")
CHECKPOINTS = [10, 20, 30, 45, 60, 90, 120, 180, 240, 300, 400, 500]
MEASUREMENTS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", "src", "WarcraftLegacies.Source",
                            "Ai", "MEASUREMENTS.md")


def decode(path):
    """Preload lines -> text lines; '  ~' continuation lines are joined to the line before."""
    out = []
    for raw in open(path, encoding="utf-8", errors="replace"):
        m = PRELOAD.match(raw)
        if not m:
            continue
        s = m.group(1)
        if s.startswith("  ~") and out:
            out[-1] += s[3:]
        else:
            out.append(s)
    return out


def minutes(tag):
    """'12:34' / '1:02:03' / '12m34s' -> minutes as float."""
    m = re.match(r"(\d+)m(\d+)s$", tag)
    if m:
        return int(m.group(1)) + int(m.group(2)) / 60.0
    parts = [int(x) for x in re.findall(r"\d+", tag)]
    total = 0
    for p in parts:
        total = total * 60 + p
    return total / 60.0 if len(parts) >= 2 else float(total)


def sections(lines):
    sec, cur = {}, "HEADER"
    for ln in lines:
        if ln.startswith("## "):
            cur = ln[3:].split(" ")[0].strip()
            sec.setdefault(cur, [])
            sec[cur].append(ln)
        else:
            sec.setdefault(cur, []).append(ln)
    return sec


def parse(path):
    lines = decode(path)
    if not lines:
        return None
    m = HEADER.search(lines[0])
    if not m:
        return None
    d = {"path": path, "build": m.group(1), "t": minutes(m.group(2)), "tag": m.group(2), "lines": lines,
         "mtime": os.path.getmtime(path)}
    d["sec"] = sections(lines)
    units = d["sec"].get("UNITS", [])
    mu = re.search(r"total (\d+) \| orders total (\d+), mode (\w+)", units[0]) if units else None
    d["units"] = int(mu.group(1)) if mu else None
    d["orders"] = int(mu.group(2)) if mu else None
    d["mode"] = mu.group(3) if mu else "?"
    armies, states = [], []
    for ln in d["sec"].get("BOTS", []):
        mb = re.match(r"P(\d+) (.+?) st=(\S*) .*?army=(\d+).*?food=(\d+)/(\d+)", ln)
        if mb:
            armies.append(int(mb.group(4)))
            states.append(f"P{mb.group(1)} {mb.group(2)} {mb.group(3)} a{mb.group(4)} f{mb.group(5)}")
    d["armies"], d["states"] = armies, states
    d["teams"] = [ln for ln in d["sec"].get("TEAMS", [])[1:] if ":" in ln]
    d["load"] = d["sec"].get("LOAD", [])[1:]
    d["ab"] = ab_values(lines)  # 0.46.2
    d["errors"] = [ln for ln in d["sec"].get("PINNED", []) if "TICK ERROR" in ln or "FAILED" in ln]
    d["errors"] += [ln for ln in lines if ln.startswith("## ERROR in")]
    return d


def heartbeat(path):
    lines = decode(path)
    return lines if lines and lines[0].startswith("WLAI heartbeat") else None


def team_short(line):
    name, _, rest = line.partition(":")
    cps = rest.split("|")[0].strip()
    return f"{name.strip()} {cps}"


def one_game(dumps):
    """0.41: keep only ONE game -- the build most dumps share, then a monotonic order counter (it never goes back
    within a game). Old dumps left in CustomMapData repeat their names and were mixed in once (0.39.10 test)."""
    counts = {}
    for d in dumps:
        counts[d["build"]] = counts.get(d["build"], 0) + 1
    build = max(sorted(counts), key=lambda b: counts[b])
    keep, skipped = [], [d for d in dumps if d["build"] != build]
    for d in sorted((d for d in dumps if d["build"] == build), key=lambda d: d["t"]):
        if keep and d["orders"] is not None and keep[-1]["orders"] is not None and d["orders"] < keep[-1]["orders"]:
            skipped.append(d)
        else:
            keep.append(d)
    return keep, skipped


LATENCY_WINDOW = re.compile(r"^\s*\d+-\d+m <0\.5s")


def stopwatch(real):
    """'t5=5:02 t10=10:30' -> 't5 5:02, t10 10:30 (68 s/min)' -- turn N = game minute N (WL turns are 1 min)."""
    pts = []
    for part in real.split():
        m = re.match(r"t(\d+)=(\d+):(\d{1,2})$", part)
        if m:
            pts.append((int(m.group(1)), int(m.group(2)) * 60 + int(m.group(3)), part))
    pts.sort()
    out, prev = [], None
    for turn, secs, raw in pts:
        txt = f"t{turn} {raw.split('=')[1]}"
        if prev and turn > prev[0]:
            txt += f" ({(secs - prev[1]) / (turn - prev[0]):.0f} s/min)"
        out.append(txt)
        prev = (turn, secs)
    return ", ".join(out) if out else "(could not read " + real + ")"


AB_LINE = re.compile(r"^(A/B \w+|guard positions removed|regroup)")
AB_PAIR = re.compile(r"([A-Za-z][A-Za-z %/-]*?) on (\d+) bots avg (-?\d+) \| off (\d+) bots avg (-?\d+)")


def ab_values(lines):
    """0.46.2: {(test, metric): (on_avg, off_avg)} from the per-bot A/B lines (cumulative since game start)."""
    out = {}
    for ln in lines:
        m = AB_LINE.match(ln.strip())
        if not m:
            continue
        test = m.group(1).replace("A/B ", "").replace("guard positions removed", "guard")
        for mm in AB_PAIR.finditer(ln):
            metric = mm.group(1).strip().rstrip(":;").split(":")[-1].split(";")[-1].strip()
            out[(test, metric)] = (int(mm.group(3)), int(mm.group(5)))
    return out


def ab_phases(dumps):
    """0.46.2: A/B per game phase -- the difference of the cumulative per-bot averages between two dumps, so early, mid
    and late game can be told apart (maintainer: a feature may help in one phase only). Spread: stalled % per phase."""
    pts = [(d["tag"], d["ab"]) for d in dumps if d.get("ab")]
    if len(pts) < 2:
        return []
    out = ["A/B per game phase (per bot of each half; on | off). Phases between the dumps listed:", ""]
    keys = sorted({k for _, ab in pts for k in ab})
    out.append("| phase | " + " | ".join(f"{t}: {m}" for t, m in keys) + " |")
    out.append("|---|" + "---|" * len(keys))
    prev_tag, prev = "0", {}
    for tag, ab in pts:
        cells = []
        for k in keys:
            if k not in ab:
                cells.append("-")
                continue
            a0, b0 = prev.get(k, (0, 0))
            cells.append(f"{ab[k][0] - a0} \\| {ab[k][1] - b0}")
        out.append(f"| {prev_tag}-{tag} | " + " | ".join(cells) + " |")
        prev_tag, prev = tag, ab
    return out + [""]


def summary(dumps, beats, note, real=None):
    dumps, skipped = one_game(dumps)
    first, last = dumps[0], dumps[-1]
    out = [f"### v{first['build']} -- {len(dumps)} dumps, {first['tag']} .. {last['tag']}"
           + (f" -- {note}" if note else ""), ""]
    if skipped:
        out += ["Skipped (other build or other game): " + ", ".join(
            f"{os.path.basename(d['path'])} (v{d['build']}, {d['tag']})" for d in skipped), ""]
    beats = [b for b in beats if f"build={first['build']} " in b[0]]
    # 0.46: errors FIRST -- the 0.44.3-0.45.0 creep TICK ERROR went unnoticed at the end of the summary
    errs = []
    for d in dumps:
        for e in d["errors"]:
            key = re.sub(r"^\S+\s", "", e)
            if key not in [re.sub(r"^\S+\s", "", x) for x in errs]:
                errs.append(e)
    out += ["**Errors (TICK ERROR / FAILED / section errors): " + ("none" if not errs else str(len(errs))) + "**"]
    out += [f"- {e}" for e in errs[:15]]
    out.append("")
    if real:
        out += ["Stopwatch (real time at game turn N; real s per game min between readings): " + stopwatch(real), ""]
    out += ab_phases(dumps)
    out.append(f"Bots: {len(last['states'])}, orders mode: {last['mode']}.")
    out.append("")
    picks = []
    for c in CHECKPOINTS:
        if c > last["t"] + 2.5:
            break
        best = min(dumps, key=lambda d: abs(d["t"] - c))
        if abs(best["t"] - c) <= 3 and best not in picks:
            picks.append(best)
    if last not in picks:
        picks.append(last)
    out += ["| game min | units on map | orders/min | bot armies (sum / max) | real s per game min | teams (CPs) |",
            "|---|---|---|---|---|---|"]
    prev = None
    for d in picks:
        opm, real = "-", "-"
        if prev is None and d["orders"] is not None and d["t"] > 0:
            opm = str(round(d["orders"] / d["t"]))  # since game start
        if prev is not None and d["orders"] is not None and prev["orders"] is not None and d["t"] > prev["t"]:
            opm = str(round((d["orders"] - prev["orders"]) / (d["t"] - prev["t"])))
            r = (d["mtime"] - prev["mtime"]) / ((d["t"] - prev["t"]) * 60) * 60
            if 20 <= r <= 600:  # outside: file times lost or changed by the copy
                real = f"{r:.0f}"
        arm = f"{sum(d['armies'])} / {max(d['armies']) if d['armies'] else 0}"
        teams = ", ".join(team_short(t) for t in d["teams"])
        out.append(f"| {d['tag']} | {d['units'] if d['units'] is not None else '-'} | {opm} | {arm} | {real} | {teams} |")
        prev = d
    out += ["", "LOAD at the checkpoints:", "```"]
    for d in picks:
        for ln in d["load"]:
            if ln.startswith("task "):  # 0.40: per-task lines only for the last dump (below)
                continue
            if LATENCY_WINDOW.match(ln) and d is not last:  # 0.46: 5-min latency windows only once (last dump)
                continue
            out.append(f"{d['tag']:>7}  {ln}")
    out.append("```")
    tasks = [ln for ln in last["load"] if ln.startswith("task ")]
    if tasks:
        out += ["", f"Top tasks by total ops at {last['tag']} (of {len(tasks)}):", "```"] + tasks[:10] + ["```"]
    turns = turnaround_lines(last)  # 0.48.3: back-and-forth measurement
    if turns:
        out += ["", f"Turnarounds at {last['tag']} (0.48.3; per cause, all bots; P lines: turns / walks):", "```"] + turns + ["```"]
    out += ["", f"Bots at {last['tag']}: " + "; ".join(last["states"])]
    for b in beats:
        out += ["", f"Heartbeat: {b[0]}"] + [f"  {x}" for x in b[1:4]] + (["  ..."] if len(b) > 4 else [])
    return "\n".join(out) + "\n"


def turnaround_lines(d):
    """0.48.3: TURNAROUNDS section -> totals per cause, the 5-min phases and one short line per bot."""
    sec = d["sec"].get("TURNAROUNDS", [])
    if not sec:
        return []
    totals, bots = {}, []
    for ln in sec:
        m = re.match(r"P(\d+) turns (\d+) of (\d+) walks:(.*?) \| group swaps (\d+), sent back (\d+) \| rest orders backwards (\d+)", ln)
        if not m:
            continue
        for kind, n in re.findall(r"([a-z-]+) (\d+)", m.group(4)):
            totals[kind] = totals.get(kind, 0) + int(n)
        bots.append(f"P{m.group(1)} {m.group(2)}/{m.group(3)} swaps {m.group(5)}>{m.group(6)} rest-back {m.group(7)}")
    out = ["total: " + " ".join(f"{k} {v}" for k, v in sorted(totals.items(), key=lambda kv: -kv[1]))]
    out += [ln for ln in sec if ln.startswith("phase ")]
    out += ["; ".join(bots[i:i + 4]) for i in range(0, len(bots), 4)]
    return out


def main():
    args = sys.argv[1:]
    note = None
    real = None
    if "--real" in args:
        i = args.index("--real")
        real = args[i + 1] if i + 1 < len(args) else ""
        args = args[:i] + args[i + 2:]
    if "--append" in args:
        i = args.index("--append")
        note = args[i + 1] if i + 1 < len(args) else ""
        args = args[:i] + args[i + 2:]
    if not args:
        print(__doc__)
        sys.exit(1)
    dumps, beats = [], []
    for p in args:
        d = parse(p)
        if d:
            dumps.append(d)
            continue
        b = heartbeat(p)
        if b:
            beats.append(b)
        else:
            print(f"skipped (not a dump or heartbeat): {p}", file=sys.stderr)
    if not dumps:
        print("no dump files found", file=sys.stderr)
        sys.exit(1)
    text = summary(dumps, beats, note, real)
    if note is not None:
        with open(MEASUREMENTS, "a", encoding="utf-8") as f:
            f.write("\n" + text)
        print(f"appended to {os.path.normpath(MEASUREMENTS)}")
    print(text)


if __name__ == "__main__":
    main()
