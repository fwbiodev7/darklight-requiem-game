"""Non-destructive, capability-checked Aseprite/LibreSprite batch export."""
import argparse
import json
from pathlib import Path
import subprocess


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--editor', required=True)
    p.add_argument('--source', type=Path, required=True)
    p.add_argument('--out', type=Path, required=True)
    a = p.parse_args()
    if not a.source.is_file():
        p.error('Source file missing')
    help_result = subprocess.run([a.editor, '--help'], capture_output=True, text=True, errors='replace')
    help_text = help_result.stdout + help_result.stderr
    needed = ['--batch', '--sheet', '--data', '--format', '--sheet-type', '--list-tags']
    missing = [flag for flag in needed if flag not in help_text]
    if help_result.returncode or missing:
        p.error('Unsupported editor CLI; missing flags: ' + ', '.join(missing))
    if a.out.exists() and any(a.out.iterdir()):
        p.error('Output directory must be new or empty')
    a.out.mkdir(parents=True, exist_ok=True)
    out = a.out.resolve()
    command = [a.editor, '--batch', str(a.source.resolve()), '--sheet-type', 'rows',
               '--format', 'json-array', '--list-tags', '--sheet', str(out / 'sheet.png'),
               '--data', str(out / 'editor.json')]
    result = subprocess.run(command, capture_output=True, text=True, errors='replace')
    (out / 'editor.log').write_text(result.stdout + result.stderr, encoding='utf-8')
    ok = result.returncode == 0 and (out / 'sheet.png').is_file() and (out / 'editor.json').is_file()
    print(json.dumps({'ok': ok, 'exitCode': result.returncode, 'log': str(out / 'editor.log')}))
    return 0 if ok else 2


if __name__ == '__main__':
    raise SystemExit(main())
