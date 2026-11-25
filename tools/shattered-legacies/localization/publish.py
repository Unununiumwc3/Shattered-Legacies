#!/usr/bin/env python3
"""Build one English/Chinese map using upstream's shared-object/WTS packaging."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[3]

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--dotnet', default='dotnet', help='Path to the .NET 10 dotnet executable')
    args = parser.parse_args()
    project = ROOT / 'src/WarcraftLegacies.CLI'
    config = json.loads((project / 'appsettings.json').read_text(encoding='utf-8-sig'))
    version = config['MapSettings']['Version']
    basename = config['MapSettings'].get('PublishedFileNames', {}).get('WarcraftLegacies')
    basename = f'{basename}-{version}' if basename else f'WarcraftLegacies{version}'
    outputs = ROOT / 'maps/published'
    base = outputs / (basename + '.w3x')
    chinese = outputs / (basename + '-zhCN.w3x')
    combined = outputs / (basename + '-multilingual.w3x')
    def run(command, cwd=ROOT):
        print('+ ' + ' '.join(map(str, command)), flush=True)
        subprocess.run(list(map(str, command)), cwd=cwd, check=True)
    run([args.dotnet, 'build', project / 'WarcraftLegacies.CLI.csproj', '-c', 'Release', '--disable-build-servers', '-m:1'])
    working = project / 'bin/Release/net10.0'
    cli = [args.dotnet, working / 'WarcraftLegacies.CLI.dll']
    run(cli + ['json-to-w3x', 'publish', 'WarcraftLegacies'], working)
    run(cli + ['json-to-w3x', 'publish', 'WarcraftLegacies', '--locale', 'zhCN'], working)
    run(cli + ['merge-locales', base, chinese, combined, 'zhcn.w3mod,zhtw.w3mod',
               '--locale-folder', '--string-table'], working)
    print(f'Play/distribute this map: {combined}\nBoth Chinese client regions use Simplified Chinese text.')

if __name__ == '__main__':
    main()
