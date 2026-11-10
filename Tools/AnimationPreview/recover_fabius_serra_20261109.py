"""Append reviewed on-foot Serra de Entrada pages, preserving all existing data.

Default is read-only. --apply writes only the catalog and recovery receipt.
An invalid sheet/context is reported and withheld without blocking other sheets.
"""
import argparse
import copy
import json
from pathlib import Path

from recover_fabius_essential_states_20261109 import (
    ROOT, BASE, CATALOG, RESOURCES, DIRECTIONS, read, sha, relative,
    require, context, counts,
)

HISTORY = ROOT / 'outputs/retomada-20261102/normal-moto-completion'
RECEIPT = ROOT / 'outputs/retomada-20261109/resume/serra-recovery.json'
NAMES = ('ReqMotoNormalSerraEntradaS_SW20261102V1',
         'ReqMotoNormalSerraEntradaW_NW_N20261102Selected',
         'ReqMotoNormalSerraEntradaNE_E_SE20261102Selected')


def plan():
    prior = read(CATALOG)
    catalog = copy.deepcopy(prior)
    tracked = {}

    def track(path):
        tracked[relative(path)] = sha(path)

    for path in (BASE / 'PAGES.json', BASE / 'manifest.json',
                 HISTORY / 'SerraEntrada-eight-directions-pages.json', HISTORY / 'REPORT.md'):
        track(path)
    historical = read(HISTORY / 'SerraEntrada-eight-directions-pages.json')
    selected = [p for p in read(BASE / 'PAGES.json')['pages'] if p['skin'] == 0 and
                p['form'] == 0 and p['weapon'] == 3 and p['action'] == 'Attack' and
                p.get('attackKey') == 'Serra de Entrada']
    require(selected == historical['pages'], 'Reviewed and historical page selections differ')
    require(len(selected) == 8 and sorted(p['direction'] for p in selected) == list(range(8)),
            'Expected eight distinct reviewed directions')
    require({p['resourcePath'].rsplit('/', 1)[-1] for p in selected} == set(NAMES),
            'Unexpected sheet selection; Selected revisions required')
    manifest = {e['path']: e['sha256'] for e in read(BASE / 'manifest.json')['files']}
    existing = {context(p): p for p in prior['pages']}
    require(len(existing) == len(prior['pages']), 'Existing catalog contains duplicate contexts')
    ids = {p['id'] for p in prior['pages']}
    require(len(ids) == len(prior['pages']), 'Existing catalog contains duplicate IDs')
    accepted, blocked, additions = [], [], []
    for name in NAMES:
        pages = [p for p in selected if p['resourcePath'].endswith('/' + name)]
        try:
            descriptor = BASE / 'staging' / (name + '.json')
            image = descriptor.with_suffix('.png')
            sheet = read(descriptor)
            receipt = HISTORY / (name + '-receipt.json')
            track(receipt)
            history = read(receipt)
            require(history['status'] == 'visual-reviewed' and history['wholeSourcePixelsPreserved'],
                    'Historical receipt is not reviewed/pixel-preserved')
            for path in (descriptor, image):
                track(path)
                require(manifest.get(relative(path)) == sha(path), 'Reviewed archive hash mismatch: ' + relative(path))
            require(sha(descriptor) == history['manifestSha256'] and sha(image) == history['imageSha256'],
                    'Historical receipt hash mismatch')
            source = ROOT / sheet['sourcePath']
            track(source)
            require(sha(source) == sheet['sourceSha256'] == history['sourceSha256'], 'Original source hash mismatch')
            require(sheet['status'] == 'visual-reviewed' and sheet['wholeFigures'] is True and
                    sheet['presentation'] == 'OnFoot', 'Invalid descriptor review/context')
            require(sheet['attackKey'] == 'Serra de Entrada', 'Descriptor alias mismatch')
            runtime = RESOURCES / pages[0]['resourcePath']
            for extension, staged in (('.json', descriptor), ('.png', image)):
                asset = Path(str(runtime) + extension)
                track(asset)
                require(sha(asset) == sha(staged), 'Runtime differs from reviewed artifact: ' + relative(asset))
                track(Path(str(asset) + '.meta'))
            frames = {f['id']: f for f in sheet['frames']}
            require(len(frames) == len(sheet['frames']), 'Duplicate descriptor frame IDs')
            width, height = sheet['nativeSize']
            rectangles = []
            for frame in frames.values():
                x, y, w, h = frame['rect']
                require(w > 0 and h > 0 and x >= 0 and y >= 0 and x+w <= width and y+h <= height,
                        'Out-of-bounds sprite: ' + frame['id'])
                require(all(not(x < ax+aw and ax < x+w and y < ay+ah and ay < y+h)
                            for ax, ay, aw, ah in rectangles), 'Overlapping sprite rectangles')
                require(len(frame['pivot']) == 2 and all(0 <= v <= 1 for v in frame['pivot']), 'Invalid pivot')
                rectangles.append((x, y, w, h))
            for page in pages:
                require(page['status'] == 'visual-reviewed' and page['wholeFigures'] is True and
                        page['presentation'] == 'OnFoot' and page['loop'] is False, 'Invalid page status/context')
                require(page['imageSha256'] == sha(image) and page['sourceSha256'] == sha(source), 'Page provenance mismatch')
                require(page['durationMs'] == [125, 125, 240, 370] and page['startupFrames'] == 2 and
                        page['activeFrames'] == 1 and len(page['spriteNames']) == 4, 'Attack timing contract mismatch')
                require(any(c['frames'] == page['spriteNames'] and not c['loop'] for c in sheet['clips']), 'Missing descriptor clip')
                for i, sprite in enumerate(page['spriteNames']):
                    f = frames[sprite]
                    require(f['direction'] == DIRECTIONS[page['direction']] and f['durationMs'] == page['durationMs'][i],
                            'Frame direction/timing mismatch')
                    density = page.get('framePixelsPerUnit', [page['pixelsPerUnit']] * 4)[i]
                    require(density == f.get('pixelsPerUnit', sheet['pixelsPerUnit']), 'Frame density mismatch')
                if context(page) in existing:
                    require(existing[context(page)] == page, 'Existing context conflicts with reviewed page')
                else:
                    require(page['id'] not in ids, 'Page ID collides with existing page')
            accepted.extend(pages)
            additions.extend(p for p in pages if context(p) not in existing)
        except (ValueError, KeyError, OSError, TypeError) as error:
            blocked.append({'sheet': name, 'directions': [p['direction'] for p in pages], 'reason': str(error)})
    catalog['pages'].extend(copy.deepcopy(additions))
    require(catalog['pages'][:len(prior['pages'])] == prior['pages'], 'Existing pages changed')
    require({k: v for k, v in catalog.items() if k != 'pages'} ==
            {k: v for k, v in prior.items() if k != 'pages'}, 'Catalog header changed')
    return catalog, {'scope': 'Fabius Normal Moto Serra de Entrada OnFoot',
        'before': counts(prior['pages']), 'after': counts(catalog['pages']),
        'addedSequences': len(additions), 'addedFrames': sum(len(p['spriteNames']) for p in additions),
        'selectedSequences': len(accepted), 'selectedFrames': sum(len(p['spriteNames']) for p in accepted),
        'historicalUniqueWholeFigures': historical['uniqueWholeFigures'], 'selectedSheets': len(NAMES)-len(blocked),
        'blockedItems': blocked, 'catalogBeforeSha256': sha(CATALOG), 'preservedFiles': tracked,
        'validation': {'sourceAndRuntimeHashes': True, 'nonOverlappingRectangles': True,
            'pivotsAndDensitiesPreserved': True, 'aliasesAndTimingPreserved': True,
            'previousPagesUnchanged': True, 'artRegenerated': False, 'unityExecuted': False,
            'currentVisualReviewPerformed': False, 'runtimeVerified': False},
        'historicalDeliveryJsonMissing': not (HISTORY / 'delivery.json').exists(),
        'historicalAuthority': 'Reviewed PAGES/manifest, exact eight-direction fragment and per-sheet receipts; historical static review only.'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    catalog, receipt = plan()
    if args.apply:
        require(sha(CATALOG) == receipt['catalogBeforeSha256'], 'Catalog changed during planning; rerun recovery')
        if receipt['addedSequences']:
            CATALOG.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        _, repeated = plan()
        require(repeated['addedSequences'] == 0, 'Idempotence replan failed')
        require(all(sha(ROOT / path) == digest for path, digest in receipt['preservedFiles'].items()), 'Protected file changed')
        receipt['catalogAfterSha256'] = sha(CATALOG)
        receipt['validation']['idempotentReplan'] = True
        if receipt['addedSequences'] or not RECEIPT.exists():
            RECEIPT.parent.mkdir(parents=True, exist_ok=True)
            RECEIPT.write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: receipt[k] for k in ('addedSequences', 'addedFrames', 'before', 'after', 'blockedItems')}))


if __name__ == '__main__':
    main()
