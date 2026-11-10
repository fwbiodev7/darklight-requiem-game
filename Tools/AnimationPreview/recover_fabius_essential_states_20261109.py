"""Recover reviewed essential catalog entries only; never import or edit artwork.

Dry run by default. --apply appends missing entries and writes an evidence receipt.
Existing contexts must match exactly; conflicting contexts abort before any write.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BASE = ROOT / 'ArtSources/ReviewedDeliveries/FabiusNormal20261103'
REPAIR = ROOT / 'ArtSources/ReviewedDeliveries/FabiusNormal20261103-layout-repair'
RESOURCES = ROOT / 'Assets/Darklight/Resources'
CATALOG = RESOURCES / 'Darklight/WeaponRedesign20261023/catalog.json'
RECEIPT = ROOT / 'outputs/retomada-20261109/essential-recovery.json'
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def relative(path):
    return path.relative_to(ROOT).as_posix()


def require(condition, message):
    if not condition:
        raise ValueError(message)


def context(page):
    return tuple(page.get(key, '') for key in
                 ('skin', 'weapon', 'form', 'direction', 'action', 'attackKey'))


def active(page):
    return page['skin'] == 0 and page['form'] == 0 and page['weapon'] in (0, 2, 3)


def counts(pages):
    selected = [p for p in pages if active(p)]
    return {'catalogPages': len(pages), 'activeSequences': len(selected),
            'activeFigures': sum(len(p['spriteNames']) for p in selected),
            'activeSheets': len({p['resourcePath'] for p in selected})}


def plan():
    catalog = read(CATALOG)
    prior = copy.deepcopy(catalog)
    tracked = {}

    def track(path):
        tracked[relative(path)] = sha(path)

    for path in (BASE / 'PAGES.json', BASE / 'manifest.json',
                 REPAIR / 'pages.json', REPAIR / 'preflight-receipt.json',
                 REPAIR / 'rgba-proofs.json'):
        track(path)
    manifest = {entry['path']: entry['sha256'] for entry in read(BASE / 'manifest.json')['files']}
    original = [p for p in read(BASE / 'PAGES.json')['pages']
                if active(p) and p['action'] in ('Parry', 'Hurt', 'Dead')]
    require(len(original) == 72, 'Expected exactly 72 reviewed essential pages')
    repaired = read(REPAIR / 'pages.json')['pages']
    require(len(repaired) == 12 and len({context(p) for p in repaired}) == 12,
            'Expected 12 unique Hurt layout replacements')
    replacements = {context(p): p for p in repaired}
    require(all(active(p) and p['action'] == 'Hurt' and p['direction'] in (4, 5, 6, 7)
                and p['resourcePath'].endswith('LayoutV2') for p in repaired),
            'Unexpected repair context')
    require(set(replacements) <= {context(p) for p in original}, 'Repair has unmatched contexts')
    selected = [copy.deepcopy(replacements.get(context(p), p)) for p in original]
    require(len({context(p) for p in selected}) == 72, 'Duplicate selected context')
    require(sum(len(p['spriteNames']) for p in selected) == 144, 'Expected 144 whole figures')
    protected = []
    for resource in sorted({p['resourcePath'] for p in selected}):
        pages = [p for p in selected if p['resourcePath'] == resource]
        name = resource.rsplit('/', 1)[-1]
        staging = REPAIR if name.endswith('LayoutV2') else BASE
        descriptor = staging / 'staging' / (name + '.json')
        image = descriptor.with_suffix('.png')
        sheet = read(descriptor)
        for path in (descriptor, image):
            track(path)
            if staging == BASE:
                require(manifest.get(relative(path)) == sha(path), 'Reviewed manifest hash mismatch: ' + str(path))
        require(sheet['status'] == 'visual-reviewed' and sheet['wholeFigures'] is True,
                'Unreviewed sheet: ' + name)
        require(sha(image) == sheet['imageSha256'], 'Sheet image hash mismatch: ' + name)
        source = ROOT / sheet['sourcePath']
        track(source)
        require(sha(source) == sheet['sourceSha256'], 'Original source hash mismatch: ' + name)
        runtime_json = RESOURCES / (resource + '.json')
        runtime_png = runtime_json.with_suffix('.png')
        for path, expected in ((runtime_json, descriptor), (runtime_png, image)):
            track(path)
            require(sha(path) == sha(expected), 'Runtime asset differs from reviewed artifact: ' + str(path))
            meta = Path(str(path) + '.meta')
            track(meta)
            protected.extend((path, meta))
        frames = {f['id']: f for f in sheet['frames']}
        require(len(frames) == len(sheet['frames']), 'Duplicate frame ID: ' + name)
        width, height = sheet['nativeSize']
        rectangles = []
        for frame in frames.values():
            x, y, w, h = frame['rect']
            require(w > 0 and h > 0 and x >= 0 and y >= 0 and x+w <= width and y+h <= height,
                    'Out-of-bounds frame: ' + frame['id'])
            for ax, ay, aw, ah in rectangles:
                require(not (x < ax+aw and ax < x+w and y < ay+ah and ay < y+h),
                        'Overlapping frame rectangles: ' + name)
            rectangles.append((x, y, w, h))
        for page in pages:
            require(page['status'] == 'visual-reviewed' and page['wholeFigures'] is True,
                    'Unreviewed page: ' + page['id'])
            require(page['imageSha256'] == sha(image) and page['sourceSha256'] == sha(source),
                    'Page provenance mismatch: ' + page['id'])
            require(page['loop'] is False and len(page['spriteNames']) == len(page['durationMs']),
                    'Invalid timing/loop: ' + page['id'])
            clip = next((c for c in sheet['clips'] if c['frames'] == page['spriteNames']), None)
            require(clip is not None and clip['loop'] is False, 'Missing matching descriptor clip: ' + page['id'])
            for sprite, duration in zip(page['spriteNames'], page['durationMs']):
                f = frames[sprite]
                require(type(duration) is int and duration > 0 and duration == f['durationMs'],
                        'Invalid frame timing: ' + sprite)
                require(f['direction'] == DIRECTIONS[page['direction']] and f['action'].lower() == page['action'].lower(),
                        'Frame context mismatch: ' + sprite)
    for weapon in (0, 2, 3):
        for action in ('Parry', 'Hurt', 'Dead'):
            require(sorted(p['direction'] for p in selected if p['weapon'] == weapon and p['action'] == action)
                    == list(range(8)), 'Incomplete essential direction coverage')
    contexts = [context(p) for p in catalog['pages']]
    require(len(contexts) == len(set(contexts)), 'Existing catalog has duplicate contexts')
    known = dict(zip(contexts, catalog['pages']))
    ids = {p['id'] for p in catalog['pages']}
    additions = []
    for page in selected:
        key = context(page)
        if key in known:
            require(known[key] == page, 'Existing context conflicts with reviewed selection: ' + page['id'])
        else:
            require(page['id'] not in ids, 'Duplicate page ID: ' + page['id'])
            additions.append(page)
            ids.add(page['id'])
    catalog['pages'].extend(additions)
    require(catalog['pages'][:len(prior['pages'])] == prior['pages'], 'Existing pages changed')
    require({k: v for k, v in catalog.items() if k != 'pages'} ==
            {k: v for k, v in prior.items() if k != 'pages'}, 'Catalog header changed')
    return catalog, {'scope': 'Fabius skin0 form0 weapons0/2/3 Parry/Hurt/Dead eight directions',
                     'before': counts(prior['pages']), 'after': counts(catalog['pages']),
                     'addedSequences': len(additions), 'selectedSequences': 72, 'selectedFigures': 144,
                     'selectedSheets': len({p['resourcePath'] for p in selected}),
                     'layoutV2Sequences': 12, 'catalogBeforeSha256': sha(CATALOG),
                     'preservedFiles': tracked, 'protectedRuntimeFiles': len(protected),
                     'validation': {'hashesAndDescriptors': True, 'rectanglesNonOverlapping': True,
                                    'contextsUnique': True, 'previousPagesUnchanged': True,
                                    'timingsAndDirections': True, 'sourceImagesModified': False,
                                    'unityExecuted': False, 'runtimeVerified': False,
                                    'artRegenerated': False}}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    catalog, receipt = plan()
    if args.apply and receipt['addedSequences']:
        CATALOG.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        _, repeated = plan()
        require(repeated['addedSequences'] == 0, 'Idempotence check failed')
        require(all(sha(ROOT / path) == digest for path, digest in receipt['preservedFiles'].items()),
                'Protected source/runtime artifact changed')
        receipt['catalogAfterSha256'] = sha(CATALOG)
        receipt['validation']['idempotentReplan'] = True
        RECEIPT.parent.mkdir(parents=True, exist_ok=True)
        RECEIPT.write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'applied': args.apply, 'addedSequences': receipt['addedSequences'],
                      'before': receipt['before'], 'after': receipt['after'],
                      'receipt': relative(RECEIPT) if RECEIPT.exists() else None}))


if __name__ == '__main__':
    main()
