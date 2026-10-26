"""Condenses the bot's -dump / -autodump files of ONE game into a short markdown block for
src/WarcraftLegacies.Source/Ai/MEASUREMENTS.md, so measurements stay in the repo and a new chat can read them.

Usage (from the repo root, on the copied CustomMapData files):
  python3 tools/ai/dump_summary.py ~/Downloads/game1/WLAI_dump_*.txt                 # print the block
  python3 tools/ai/dump_summary.py ~/Downloads/game1/WLAI_*.txt --append "1 human, 12 bots, Veteran"
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
MEASUREMENTS = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "src", "WarcraftLegacies.Source",
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


def summary(dumps, beats, note):
    dumps, skipped = one_game(dumps)
    first, last = dumps[0], dumps[-1]
    out = [f"### v{first['build']} -- {len(dumps)} dumps, {first['tag']} .. {last['tag']}"
           + (f" -- {note}" if note else ""), ""]
    if skipped:
        out += ["Skipped (other build or other game): " + ", ".join(
            f"{os.path.basename(d['path'])} (v{d['build']}, {d['tag']})" for d in skipped), ""]
    beats = [b for b in beats if f"build={first['build']} " in b[0]]
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
            if not ln.startswith("task "):  # 0.40: per-task lines only for the last dump (below)
                out.append(f"{d['tag']:>7}  {ln}")
    out.append("```")
    tasks = [ln for ln in last["load"] if ln.startswith("task ")]
    if tasks:
        out += ["", f"Top tasks by total ops at {last['tag']} (of {len(tasks)}):", "```"] + tasks[:10] + ["```"]
    errs = []
    for d in dumps:
        for e in d["errors"]:
            key = re.sub(r"^\S+\s", "", e)
            if key not in [re.sub(r"^\S+\s", "", x) for x in errs]:
                errs.append(e)
    out += ["", "Errors (TICK ERROR / FAILED / section errors): " + ("none" if not errs else str(len(errs)))]
    out += [f"- {e}" for e in errs[:15]]
    out += ["", f"Bots at {last['tag']}: " + "; ".join(last["states"])]
    for b in beats:
        out += ["", f"Heartbeat: {b[0]}"] + [f"  {x}" for x in b[1:4]] + (["  ..."] if len(b) > 4 else [])
    return "\n".join(out) + "\n"


def main():
    args = sys.argv[1:]
    note = None
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
    text = summary(dumps, beats, note)
    if note is not None:
        with open(MEASUREMENTS, "a", encoding="utf-8") as f:
            f.write("\n" + text)
        print(f"appended to {os.path.normpath(MEASUREMENTS)}")
    print(text)


if __name__ == "__main__":
    main()
