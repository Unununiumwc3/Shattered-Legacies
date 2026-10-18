"""Compile-check a built war3map.lua with Lua 5.3 (the Lua version Warcraft III uses) -- catches errors that make
the game reject the map as "unavailable or corrupted" before you even start it (e.g. "too many local variables").
Usage:  python3 lua_check.py <path to war3map.lua>          (the build writes it, e.g. under artifacts/)
Needs:  pip install lupa --break-system-packages"""
import sys
import lupa.lua53 as lua53

path = sys.argv[1] if len(sys.argv) > 1 else "war3map.lua"
src = open(path, "rb").read()
lua = lua53.LuaRuntime()
check = lua.eval('function(s) local f, err = load(s, "war3map.lua", "t") return f ~= nil, err end')
ok, err = check(src)
if ok:
    print(f"OK  {path} compiles with Lua 5.3 ({len(src) // 1024} KB)")
    sys.exit(0)
print(f"LUA ERROR  {err}")
try:
    line = int(str(err).split("]:")[1].split(":")[0])
    lines = src.decode("utf-8", "replace").split("\n")
    for i in range(max(0, line - 3), min(len(lines), line + 2)):
        print(f"{i + 1:7d}  {lines[i].rstrip()[:160]}")
except (IndexError, ValueError):
    pass
sys.exit(1)
