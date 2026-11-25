#!/usr/bin/env bash
# 0.72.0: real C# compile of MacroTools + WarcraftLegacies.Shared + WarcraftLegacies.Source WITHOUT NuGet.
#
# The map build needs NuGet (WCSharp.*, War3Net.*). Where NuGet is blocked (agent sandboxes), this script builds the
# same sources against WCSharp compiled from its GitHub source (github.com/Orden4/WCSharp, version 3.3.9 in
# Directory.Build.props) in a throw-away harness. It proves the C# compiles (types, members, overloads, scopes);
# it does NOT run CSharp.lua (Lua limits, delegate caching: tools/shattered-legacies/ai/check_refs.py,
# delegate_check.py on a built war3map.lua).
#
#   tools/shattered-legacies/compile_check.sh [harness dir, default /tmp/sl-harness]
#
# Needs: dotnet SDK 10, git, network access to github.com. Prints "Build succeeded." or the compiler errors.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
H="${1:-/tmp/sl-harness}"
W="$H/WCSharp"
R="$ROOT/src"
mkdir -p "$H"
if [ ! -d "$W" ]; then
  git clone -q --depth 50 https://github.com/Orden4/WCSharp.git "$W"
fi

inc=""
for l in Api Shared Events Timers Dummies Buffs Effects Json Lightnings Missiles SaveLoad Sync W3MMD; do
  [ -d "$W/WCSharp.$l" ] && inc="$inc<Compile Include=\"$W/WCSharp.$l/**/*.cs\" Exclude=\"$W/WCSharp.$l/obj/**;$W/WCSharp.$l/bin/**\" />"
done
mkdir -p "$H/WCSharpAll"
cat > "$H/WCSharpAll/WCSharpAll.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>11.0</LangVersion><EnableDefaultCompileItems>false</EnableDefaultCompileItems><NoWarn>\$(NoWarn);CS0626;CS1591;CS8632;CS0108;CS0114</NoWarn><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup><ItemGroup>$inc</ItemGroup></Project>
EOF

NOWARN='$(NoWarn);CS1591;CS8600;CS8601;CS8602;CS8603;CS8604;CS8618;CS8625;CS8619;CS8620;CS8622;CS8629;CS8765;CS8767;CS0626;CS0649;CS0169;CS0414;CS0168;CS0219'
mk() { # name, extra properties, references -- the same settings as the real .csproj files
  mkdir -p "$H/$1"
  cat > "$H/$1/$1.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><LangVersion>11.0</LangVersion><Nullable>enable</Nullable>$2<EnableDefaultCompileItems>false</EnableDefaultCompileItems><AssemblyName>$1</AssemblyName><NoWarn>$NOWARN</NoWarn></PropertyGroup><ItemGroup><Compile Include="$R/$1/**/*.cs" Exclude="$R/$1/obj/**;$R/$1/bin/**" /></ItemGroup><ItemGroup>$3</ItemGroup></Project>
EOF
}
mk MacroTools.Shared "<ImplicitUsings>enable</ImplicitUsings>" ""
mk MacroTools "<RootNamespace>MacroTools</RootNamespace>" '<ProjectReference Include="../MacroTools.Shared/MacroTools.Shared.csproj" /><ProjectReference Include="../WCSharpAll/WCSharpAll.csproj" />'
mk WarcraftLegacies.Shared "<ImplicitUsings>enable</ImplicitUsings>" '<ProjectReference Include="../MacroTools.Shared/MacroTools.Shared.csproj" />'
mk WarcraftLegacies.Source "<RootNamespace>WarcraftLegacies.Source</RootNamespace>" '<ProjectReference Include="../MacroTools.Shared/MacroTools.Shared.csproj" /><ProjectReference Include="../MacroTools/MacroTools.csproj" /><ProjectReference Include="../WarcraftLegacies.Shared/WarcraftLegacies.Shared.csproj" /><ProjectReference Include="../WCSharpAll/WCSharpAll.csproj" />'

cd "$H/WarcraftLegacies.Source"
dotnet build -nologo -v q 2>&1 | grep -E " error |Build succeeded|Build FAILED" | sed "s#$R/##; s# \[$H.*##" | sort -u
