"""Catches two C# compile errors that tree-sitter (syntax only) misses -- found the hard way in 0.37.1:
  CS0841/CS0844  a local variable has the same name as a TYPE used in the same method ("timer timer = ...", then
                 "timer.Create()" anywhere in that method refers to the not-yet-declared local)
  CS0136         a local declared in a nested block has the same name as a local of an enclosing block
Usage: python3 tools/ai/scope_check.py FILE_OR_DIR [...]   (exit code 1 on findings)"""
import sys, os, glob
import tree_sitter_c_sharp as tscs, tree_sitter as ts

P = ts.Parser(ts.Language(tscs.language()))
SCOPES = {"block", "for_statement", "foreach_statement", "using_statement", "catch_clause", "switch_section",
          "lambda_expression", "local_function_statement"}

def text(n, src): return src[n.start_byte:n.end_byte].decode("utf-8", "replace")

def method_findings(m, src, path):
    out = []
    decls = []  # (name, scope_node, line)
    def scope_of(n):
        p = n.parent
        while p is not None and p.type not in SCOPES and p != m:
            p = p.parent
        return p
    def walk(n):
        if n.type == "variable_declarator":
            nm = n.child_by_field_name("name") or (n.children[0] if n.children else None)
            if nm is not None and nm.type == "identifier":
                decls.append((text(nm, src), scope_of(n), n.start_point[0] + 1))
        if n.type in ("declaration_pattern", "foreach_statement"):
            for c in n.children:
                if c.type == "identifier" and n.type == "foreach_statement" and c == n.child_by_field_name("left"):
                    decls.append((text(c, src), n, c.start_point[0] + 1))
            if n.type == "declaration_pattern":
                d = n.child_by_field_name("designation")
                if d is not None: decls.append((text(d, src), scope_of(n), d.start_point[0] + 1))
        for c in n.children: walk(c)
    walk(m)
    # CS0136: same name, one scope encloses the other
    def encloses(a, b):
        p = b
        while p is not None:
            if p == a: return True
            p = p.parent
        return False
    for i, (n1, s1, l1) in enumerate(decls):
        for n2, s2, l2 in decls[i + 1:]:
            if n1 == n2 and s1 is not None and s2 is not None and s1 != s2 and (encloses(s1, s2) or encloses(s2, s1)):
                out.append(f"{path}:{l2}: '{n2}' declared again in a nested scope (first at line {l1}) -- CS0136")
    # CS0841-like: local named like a type that is used as Type.Member or as a declaration type in this method
    local_names = {n for n, _, _ in decls}
    used_types = set()
    def walk2(n):
        if n.type == "member_access_expression":
            e = n.child_by_field_name("expression")
            if e is not None and e.type == "identifier": used_types.add((text(e, src), e.start_point[0] + 1))
        if n.type == "variable_declaration":
            t = n.child_by_field_name("type")
            if t is not None and t.type == "identifier": used_types.add((text(t, src), t.start_point[0] + 1))
        for c in n.children: walk2(c)
    walk2(m)
    for nm, line in sorted(used_types):
        if nm in local_names:
            decl_line = min(l for n, _, l in decls if n == nm)
            if line < decl_line:
                out.append(f"{path}:{line}: '{nm}' used before the local '{nm}' (line {decl_line}) is declared -- "
                           "the local hides the type/name in the whole block (CS0841)")
    return out

def check(path):
    src = open(path, "rb").read()
    tree = P.parse(src)
    out = []
    def walk(n):
        if n.type in ("method_declaration", "constructor_declaration", "local_function_statement", "accessor_declaration"):
            out.extend(method_findings(n, src, path))
        for c in n.children: walk(c)
    walk(tree.root_node)
    return out

if __name__ == "__main__":
    files = []
    for a in sys.argv[1:] or ["src/WarcraftLegacies.Source/Ai"]:
        files += sorted(glob.glob(os.path.join(a, "**", "*.cs"), recursive=True)) if os.path.isdir(a) else [a]
    found = [f for p in files for f in check(p)]
    print("\n".join(found) if found else f"scope check: {len(files)} files OK")
    sys.exit(1 if found else 0)
