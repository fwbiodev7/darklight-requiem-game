"""Read-only structural audit; never certifies generated art or runtime import."""
import hashlib
import json
import re
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
FOLDERS = ['FabiusForms20261111', 'LuciusNormal20261109', 'LuciusArconte20261111']
REPORT = ROOT / 'work/session-20261114-catalog-audit.json'


def main():
    report = dict(runtimeVerified=False, newSessionImagesReferenced=0, catalogs=[])
    for folder in FOLDERS:
        base = ROOT / 'Assets/Darklight/Resources/Darklight' / folder
        cat = json.loads((base / 'catalog.json').read_text(encoding='utf-8-sig'))
        entry = dict(name=folder, pages=len(cat['pages']), frames=0, errors=[], warnings=[], coverage={})
        images = {}
        contexts = set()
        for page in cat['pages']:
            name = page.get('name') or Path(page.get('resourcePath', '')).name
            png = base / (page.get('image') or name + '.png')
            if not png.is_file():
                entry['errors'].append(dict(sheet=name, issue='missing-image'))
                continue
            if png not in images:
                with Image.open(png) as im:
                    images[png] = dict(size=list(im.size), mode=im.mode,
                                       alpha=im.getchannel('A').getextrema() if im.mode == 'RGBA' else None,
                                       sha=hashlib.sha256(png.read_bytes()).hexdigest())
            info = images[png]
            if page.get('imageSha256') != info['sha']:
                entry['errors'].append(dict(sheet=name, issue='hash-mismatch'))
            if page.get('nativeSize') != info['size']:
                entry['errors'].append(dict(sheet=name, issue='size-mismatch'))
            meta = png.with_suffix('.png.meta')
            text = meta.read_text(encoding='utf-8-sig') if meta.exists() else ''
            if not re.search(r'^guid: [a-f0-9]{32}$', text, re.M):
                entry['errors'].append(dict(sheet=name, issue='missing-texture-guid'))
            if info['alpha'] is None or info['alpha'][0] != 0:
                entry['warnings'].append(dict(sheet=name, issue='no-transparent-background'))
            if 'frames' in page:
                for frame in page['frames']:
                    entry['frames'] += 1
                    rect = frame.get('rect', [])
                    pivot = frame.get('pivot', [])
                    if len(rect) != 4 or min(rect) < 0 or rect[2] <= 0 or rect[3] <= 0 or rect[0]+rect[2] > info['size'][0] or rect[1]+rect[3] > info['size'][1]:
                        entry['errors'].append(dict(sheet=name, frame=frame.get('id'), issue='out-of-bounds-rect'))
                    if len(pivot) != 2 or any(not 0 <= n <= 1 for n in pivot):
                        entry['errors'].append(dict(sheet=name, frame=frame.get('id'), issue='invalid-pivot'))
                    if frame.get('id') and not re.search(r'^\s+name: '+re.escape(frame['id'])+r'\s*$', text, re.M):
                        entry['errors'].append(dict(sheet=name, frame=frame['id'], issue='sprite-name-missing-meta'))
                    action = frame.get('action', page.get('action', ''))
                    direction = frame.get('direction', '?')
                    entry['coverage'].setdefault(action, set()).add(str(direction))
            else:
                names = page.get('spriteNames', [])
                entry['frames'] += len(names)
                if len(names) != len(page.get('durationMs', [])) or any(n <= 0 for n in page.get('durationMs', [])):
                    entry['errors'].append(dict(sheet=name, issue='invalid-durations'))
                for sprite in names:
                    if not re.search(r'^\s+name: '+re.escape(sprite)+r'\s*$', text, re.M):
                        entry['errors'].append(dict(sheet=name, frame=sprite, issue='sprite-name-missing-meta'))
                context = (page.get('action'), page.get('direction'), page.get('attackKey', ''))
                if context in contexts:
                    entry['errors'].append(dict(sheet=name, issue='duplicate-context', context=context))
                contexts.add(context)
                entry['coverage'].setdefault(page.get('action', ''), set()).add(str(page.get('direction')))
        entry['coverage'] = {k: sorted(v) for k, v in entry['coverage'].items()}
        entry['images'] = len(images)
        report['newSessionImagesReferenced'] += sum(p.stem.startswith('Session14') or p.stem.endswith('Session20261114') for p in images)
        entry['structuralPass'] = not entry['errors']
        report['catalogs'].append(entry)
    report['errors'] = sum(len(x['errors']) for x in report['catalogs'])
    report['warnings'] = sum(len(x['warnings']) for x in report['catalogs'])
    REPORT.parent.mkdir(parents=True, exist_ok=True)
    REPORT.write_text(json.dumps(report, indent=2, ensure_ascii=False)+'\n', encoding='utf-8')
    print(json.dumps({**{k:v for k,v in report.items() if k != 'catalogs'},
                      'catalogs': [{k:v for k,v in x.items() if k not in ('errors','warnings','coverage')} for x in report['catalogs']],
                      'firstErrors': [e for x in report['catalogs'] for e in x['errors']][:8]}, ensure_ascii=True))
    return 1 if report['errors'] else 0


if __name__ == '__main__':
    raise SystemExit(main())
