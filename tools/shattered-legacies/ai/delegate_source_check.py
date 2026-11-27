"""Source-level twin of delegate_check.py (0.89.1) -- no build needed. Lists every Func<...> / Action<...> type
written in the C# sources (comments skipped) and reports a pair where one signature's type arguments are a strict
prefix of another's (Func<player> = (player) vs Action<player> = (player, Void)): CSharp.lua may build the shorter
one as a bare table and the next generic around it aborts the map's setup (0.88.0: Conquest's List<Func<player>> --
no control points, no scoreboard, no commands). KNOWN holds the pairs that already loaded safely in a playtested
build (0.86.1); a NEW pair fails the check -- change one delegate's signature or use an interface instead.
Usage (repo root):  python3 tools/shattered-legacies/ai/delegate_source_check.py      (exit code 1 on a new pair)"""
import collections
import glob
import os
import re
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", "..", "src"))
KNOWN = {
    (("unit",), ("unit", "Void")), (("unit",), ("unit", "bool")), (("string",), ("string", "Void")),
    (("string",), ("string", "string")), (("int", "T"), ("int", "T", "Void")),
}


def split_args(s):
    out, depth, cur = [], 0, ""
    for ch in s:
        depth += ch == "<"
        depth -= ch == ">"
        if ch == "," and depth == 0:
            out.append(cur.strip())
            cur = ""
        else:
            cur += ch
    if cur.strip():
        out.append(cur.strip())
    return out


def main():
    sigs = collections.defaultdict(set)
    for f in glob.glob(os.path.join(ROOT, "**", "*.cs"), recursive=True):
        if "/obj/" in f or "/bin/" in f:
            continue
        text = open(f, encoding="utf-8-sig", errors="replace").read()
        text = re.sub(r"//[^\n]*|/\*.*?\*/", "", text, flags=re.S)
        for m in re.finditer(r"\b(?:System\.)?(Func|Action)<", text):
            i = j = m.end()
            depth = 1
            while depth and j < len(text):
                depth += text[j] == "<"
                depth -= text[j] == ">"
                j += 1
            args = tuple(a.replace("?", "").strip() for a in split_args(text[i:j - 1]))
            sigs[args if m.group(1) == "Func" else args + ("Void",)].add(os.path.relpath(f, ROOT))
    bad = 0
    for a in sigs:
        for b in sigs:
            if len(a) < len(b) and b[:len(a)] == a and (a, b) not in KNOWN:
                bad += 1
                print(f"COLLISION ({', '.join(a)}) {sorted(sigs[a])[:2]}  is a prefix of  ({', '.join(b)}) "
                      f"{sorted(sigs[b])[:2]}")
    print(f"{len(sigs)} delegate signatures in the sources, {bad} new collision(s)")
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()
