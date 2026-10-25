"""Copy only named reviewed whole-figure atlases and merge their exact contexts."""
import argparse
import hashlib
import json
import math
import shutil
import subprocess
import sys
from pathlib import Path
from prepare_weapon_redesign import ROOT, RES, write

OUT = ROOT / 'outputs/redesign-armas-20261025-retomada1350'


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--fragment', type=Path, action='append', required=True)
    args = parser.parse_args()
    copied, fragments = [], []
    for fragment_path in args.fragment:
        fragment = json.loads(fragment_path.read_text(encoding='utf-8'))
        for page in fragment['pages']:
            name = Path(page['resourcePath']).name
            if page['status'] != 'visual-reviewed' or not page['wholeFigures']:
                raise ValueError('Unreviewed context: ' + page['id'])
            candidates = [fragment_path.parent / (name + '.json'), fragment_path.parent / 'packed' / (name + '.json')]
            descriptor = next((path for path in candidates if path.is_file()), None)
            if descriptor is None:
                raise FileNotFoundError('Missing staged descriptor: ' + name)
            sheet = json.loads(descriptor.read_text(encoding='utf-8'))
            image = descriptor.with_suffix('.png')
            source = ROOT / sheet['sourcePath']
            if sheet['image'] != image.name or digest(image) != sheet['imageSha256'] or digest(source) != sheet['sourceSha256']:
                raise ValueError('Changed source or atlas: ' + name)
            if not math.isfinite(sheet['pixelsPerUnit']) or sheet['pixelsPerUnit'] <= 0:
                raise ValueError('Invalid anatomical scale: ' + name)
            for field in ('imageSha256', 'sourceSha256', 'sourcePath', 'pixelsPerUnit'):
                if sheet[field] != page[field]:
                    raise ValueError('Page/manifest mismatch: ' + name + '/' + field)
            if not set(page['spriteNames']).issubset({frame['id'] for frame in sheet['frames']}):
                raise ValueError('Missing authored phases: ' + name)
            for original in (descriptor, image):
                target = RES / original.name
                if target.exists() and digest(target) != digest(original):
                    backup = OUT / 'preserved-runtime-before-update' / (digest(target)[:16] + '-' + target.name)
                    backup.parent.mkdir(parents=True, exist_ok=True)
                    if not backup.exists():
                        shutil.copy2(target, backup)
                if not target.exists() or digest(target) != digest(original):
                    shutil.copy2(original, target)
                    copied.append(target.relative_to(ROOT).as_posix())
        fragments.append(str(fragment_path))
    result = subprocess.run([sys.executable, str(ROOT / 'Tools/AnimationPreview/consolidate_redesign_20261025.py'),
        *[arg for path in fragments for arg in ('--fragment', path)]], capture_output=True, text=True, encoding='utf-8')
    if result.returncode:
        raise RuntimeError(result.stderr[-1600:])
    receipt = dict(fragments=fragments, copiedFiles=len(set(copied)), copiedPaths=sorted(set(copied)),
        consolidation=json.loads(result.stdout), imported=False, gameplayVerified=False, audio=False)
    write(OUT / 'integration-preparation.json', receipt)
    print(json.dumps({key:value for key,value in receipt.items() if key!='copiedPaths'}))


if __name__ == '__main__':
    main()
