"""Finds CSharp.lua delegate-type collisions in a built war3map.lua (0.70.1).

CSharp.lua builds the type of a delegate (Func / Action / own delegates) for the reflection metadata from its type
arguments -- System.Delegate(a, b, ..., return type) -- and caches the result in ONE tree keyed by those arguments.
When the arguments of delegate A are a strict prefix of delegate B's (Func<unit> = (unit), Action<unit> =
(unit, Void)), whichever is built second can go wrong: if B came first, asking for A returns B's bare tree node, a
table without a type name. The next generic built around it (List<...>, Dictionary<..., ...>) dies with
"invalid value (nil) at index N in table for 'concat'" -- at map init this aborted the bot's setup and everything
after it in GameSetup (0.69.0 / 0.70.0: AiMissions' Func<unit> escort; no control points, no scoreboard).

Which of the two is built first depends on the order classes are first touched, so a pair can be harmless for
years and break when a class is used earlier. Fix a NEW pair by changing a delegate's signature (e.g. give it a
parameter). KNOWN pairs below are older than the bot and load in a safe order today.

  python3 tools/shattered-legacies/ai/delegate_check.py <path to war3map.lua>
  (the build writes a copy to artifacts/war3map.lua; a .w3x can be opened with any MPQ tool)
Exit code 1 when a pair outside KNOWN shows up."""
import re
import sys

KNOWN = {
    (("System.String",), ("System.String", "System.Void")),  # Func<string> / Action<string>: WL, loads safely
    (("System.Int32",), ("System.Int32", "System.Void")),    # Func<int> / Action<int>: WL, loads safely
}


def signatures(text):
    sigs = {}
    for m in re.finditer(r"System\.Delegate\(", text):
        i = j = m.end()
        depth = 1
        while depth and j < len(text):
            if text[j] == "(":
                depth += 1
            elif text[j] == ")":
                depth -= 1
            j += 1
        parts, depth, cur = [], 0, ""
        for c in text[i:j - 1]:
            if c == "(":
                depth += 1
            elif c == ")":
                depth -= 1
            if c == "," and depth == 0:
                parts.append(cur.strip())
                cur = ""
            else:
                cur += c
        parts.append(cur.strip())
        sigs.setdefault(tuple(parts), []).append(text.count("\n", 0, m.start()) + 1)
    return sigs


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    text = open(sys.argv[1], encoding="utf-8", errors="replace").read()
    sigs = signatures(text)
    bad = 0
    for a in sigs:
        for b in sigs:
            if len(a) < len(b) and b[:len(a)] == a:
                known = (a, b) in KNOWN
                bad += 0 if known else 1
                print(("known " if known else "COLLISION ") + f"({', '.join(a)}) lines {sigs[a][:3]}  is a prefix of  "
                      f"({', '.join(b)}) lines {sigs[b][:3]}")
    print(f"{len(sigs)} delegate signatures, {bad} new collision(s)")
    sys.exit(1 if bad else 0)


if __name__ == "__main__":
    main()
