"""Extension methods without their using: the CSharp.lua compile at publish (json-to-w3x) fails on them, while
`dotnet build src/WarcraftLegacies.CLI` does not compile WarcraftLegacies.Source at all (0.53.0: TimelineSystem called
GetPlayerData() without `using MacroTools.Extensions;`). Heuristic: an instance method with the same name (List.Contains,
ToString) is reported too -- check those by hand.
Usage (repo root): python3 tools/shattered-legacies/ai/using_check.py <file.cs> ...   (default: the Ai and ShatteredLegacies folders)
"""
import re, glob, os, sys
ROOT=os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', 'src')
# extension methods: name -> set of namespaces that define them
ext={}
for f in glob.glob(ROOT+'/**/*.cs', recursive=True):
    if '/bin/' in f or '/obj/' in f: continue
    s=open(f,encoding='utf-8-sig',errors='replace').read()
    m=re.search(r'^\s*namespace\s+([\w.]+)', s, re.M)
    ns=m.group(1) if m else ''
    for name in re.findall(r'public\s+static\s+[\w<>\[\],.? ]+?\s+(\w+)\s*(?:<[^>]*>)?\s*\(\s*this\s', s):
        ext.setdefault(name,set()).add(ns)
COMMON = {'Contains', 'ToString', 'Add', 'Remove', 'Equals', 'GetHashCode'}  # instance methods of common types
targets=sys.argv[1:] or [f for d in ('Ai', 'ShatteredLegacies') for f in glob.glob(os.path.join(ROOT, 'WarcraftLegacies.Source', d, '**', '*.cs'), recursive=True)]
bad=0
for f in targets:
    s=open(f,encoding='utf-8-sig').read()
    usings=set(re.findall(r'^\s*using\s+([\w.]+)\s*;', s, re.M))
    m=re.search(r'^\s*namespace\s+([\w.]+)', s, re.M); ns=m.group(1) if m else ''
    # namespaces visible: usings + own namespace and its parents
    vis=set(usings)|{'.'.join(ns.split('.')[:i]) for i in range(1,len(ns.split('.'))+1)}
    code=re.sub(r'//.*','',s)
    for name in sorted(set(re.findall(r'\.(\w+)\s*\(', code)) - COMMON):
        if name in ext and not (ext[name] & vis):
            # a same-named instance method may exist; report for manual check
            print(f'{os.path.relpath(f,ROOT)}: .{name}() needs one of {sorted(ext[name])}'); bad+=1
print('using check:', len(targets), 'files,', bad, 'findings')
