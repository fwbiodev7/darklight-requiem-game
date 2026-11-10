"""Recover nine reviewed Normal attack families without changing source/runtime art.

Dry-run by default; --apply appends missing catalog pages and records evidence.
Only the explicitly listed Punhos attack keys are normalized, on copied pages.
"""
import argparse
import copy
import json
import re

from recover_fabius_essential_states_20261109 import (
    ROOT, BASE, CATALOG, RESOURCES, DIRECTIONS, read, sha, relative,
    require, context, counts,
)

RECEIPT = ROOT / 'outputs/retomada-20261109/resume/attack-families-recovery.json'
KATANA = ('Pesado', 'Carregado', 'ContraAtaque', 'OndaDoVeu', 'KatanaAerial')
FISTS = {'Martelo da Ru\u00c3\u00adna': 'Martelo da Ruína',
         'Ju\u00c3\u00adzo da Ru\u00c3\u00adna': 'Juízo da Ruína',
         'Cataclismo da Ru\u00c3\u00adna': 'Cataclismo da Ruína',
         'Resposta Brutal': 'Resposta Brutal'}
FISTS.update({canonical: canonical for canonical in tuple(FISTS.values())})


def plan():
    original = read(CATALOG)
    catalog = copy.deepcopy(original)
    protected = {}

    def track(path):
        protected[relative(path)] = sha(path)

    for path in (BASE / 'PAGES.json', BASE / 'manifest.json'):
        track(path)
    pages = [copy.deepcopy(p) for p in read(BASE / 'PAGES.json')['pages']
             if p['skin'] == 0 and p['form'] == 0 and p['action'] == 'Attack' and
             (p['weapon'] == 0 and p.get('attackKey') in KATANA or
              p['weapon'] == 2 and p.get('attackKey') in FISTS)]
    require(len(pages) == 72 and sum(len(p['spriteNames']) for p in pages) == 256,
            'Expected 72 sequences / 256 frames from the reviewed archive')
    mappings = []
    profile = ROOT / 'Assets/Darklight/Runtime/Combat/DarklightRuinFistsProfile.cs'
    track(profile)
    profile_text = profile.read_text(encoding='utf-8-sig')
    for raw in dict.fromkeys(p['attackKey'] for p in pages if p['weapon'] == 2):
        canonical = FISTS[raw]
        require('Make("' + canonical + '"' in profile_text, 'Canonical name absent from current Punhos profile: ' + canonical)
        mappings.append({'raw': raw, 'canonical': canonical, 'changed': raw != canonical,
                         'sequences': sum(p['weapon'] == 2 and p['attackKey'] == raw for p in pages)})
    for page in pages:
        if page['weapon'] == 2:
            page['attackKey'] = FISTS[page['attackKey']]
    groups = {}
    for page in pages:
        groups.setdefault((page['weapon'], page['attackKey']), []).append(page)
    require(len(groups) == 9 and len({context(p) for p in pages}) == 72, 'Duplicate or missing selected families')
    for group in groups.values():
        require(sorted(p['direction'] for p in group) == list(range(8)), 'Incomplete family direction coverage')
    archive_manifest = {e['path']: e['sha256'] for e in read(BASE / 'manifest.json')['files']}
    sheets = []
    for resource in sorted({p['resourcePath'] for p in pages}):
        name = resource.rsplit('/', 1)[-1]
        descriptor = BASE / 'staging' / (name + '.json')
        image = descriptor.with_suffix('.png')
        sheet = read(descriptor)
        for path in (descriptor, image):
            track(path)
            require(archive_manifest.get(relative(path)) == sha(path), 'Reviewed manifest mismatch: ' + relative(path))
        require(sheet['status'] == 'visual-reviewed' and sheet['wholeFigures'] is True,
                'Sheet is not reviewed whole-figure art: ' + name)
        require(sha(image) == sheet['imageSha256'], 'Image descriptor mismatch: ' + name)
        source = ROOT / sheet['sourcePath']
        track(source)
        require(sha(source) == sheet['sourceSha256'], 'Original source hash mismatch: ' + name)
        runtime_descriptor = RESOURCES / (resource + '.json')
        runtime_image = runtime_descriptor.with_suffix('.png')
        for runtime, staged in ((runtime_descriptor, descriptor), (runtime_image, image)):
            track(runtime)
            require(sha(runtime) == sha(staged), 'Runtime artifact differs from reviewed archive: ' + relative(runtime))
            meta = runtime.with_name(runtime.name + '.meta')
            track(meta)
            require(re.search(r'^guid: [0-9a-f]{32}$', meta.read_text(encoding='utf-8-sig'), re.MULTILINE),
                    'Missing/invalid existing asset GUID: ' + relative(meta))
        frames = {f['id']: f for f in sheet['frames']}
        frame_indices = {f['id']: i for i, f in enumerate(sheet['frames'])}
        require(len(frames) == len(sheet['frames']), 'Duplicate descriptor frame ID: ' + name)
        width, height = sheet['nativeSize']
        rectangles = []
        texture_meta = runtime_image.with_name(runtime_image.name + '.meta').read_text(encoding='utf-8-sig')
        for frame in frames.values():
            x, y, w, h = frame['rect']
            require(w > 0 and h > 0 and x >= 0 and y >= 0 and x+w <= width and y+h <= height,
                    'Frame exceeds sheet bounds: ' + frame['id'])
            require(all(not(x < ax+aw and ax < x+w and y < ay+ah and ay < y+h)
                        for ax, ay, aw, ah in rectangles), 'Overlapping sprite rectangles: ' + name)
            rectangles.append((x, y, w, h))
            require(len(frame['pivot']) == 2 and all(0 <= v <= 1 for v in frame['pivot']), 'Invalid sprite pivot')
            require(frame['id'] in texture_meta, 'Frame missing in preserved Unity sprite metadata: ' + frame['id'])
        for page in (p for p in pages if p['resourcePath'] == resource):
            require(page['status'] == 'visual-reviewed' and page['wholeFigures'] is True and page['loop'] is False,
                    'Invalid page review/loop contract: ' + page['id'])
            require(page['imageSha256'] == sha(image) and page['sourceSha256'] == sha(source), 'Page provenance mismatch')
            require(page['nativeSize'] == sheet['nativeSize'] and page['pixelsPerUnit'] == sheet['pixelsPerUnit'],
                    'Page density/size mismatch')
            names, timing = page['spriteNames'], page['durationMs']
            require(len(names) == len(timing) and all(type(ms) is int and ms > 0 for ms in timing), 'Invalid page timing')
            require(0 < page['startupFrames'] < len(names) and
                    0 < page['activeFrames'] < len(names)-page['startupFrames'], 'Invalid attack phase boundaries')
            require(any(c['frames'] == names and c['loop'] is False for c in sheet['clips']), 'Matching clip missing')
            for index, sprite in enumerate(names):
                frame = frames[sprite]
                require(frame['direction'] == DIRECTIONS[page['direction']] and frame['durationMs'] == timing[index],
                        'Frame direction/timing mismatch')
                density = page.get('framePixelsPerUnit', [page['pixelsPerUnit']] * len(names))[index]
                descriptor_density = sheet['framePixelsPerUnit'][frame_indices[sprite]] if sheet.get('framePixelsPerUnit') else frame.get('pixelsPerUnit', sheet['pixelsPerUnit'])
                require(density == descriptor_density, 'Per-frame density mismatch: ' + sprite)
        sheets.append({'resourcePath': resource, 'frames': len(frames),
                       'reviewedDescriptorSha256': sha(descriptor), 'imageSha256': sha(image),
                       'sourcePath': sheet['sourcePath'], 'sourceSha256': sha(source)})
    contexts = {context(p): p for p in catalog['pages']}
    ids = {p['id'] for p in catalog['pages']}
    require(len(contexts) == len(ids) == len(catalog['pages']), 'Existing duplicate context/ID')
    additions = []
    for page in pages:
        if context(page) in contexts:
            require(contexts[context(page)] == page, 'Existing context conflicts with reviewed family: ' + page['id'])
        else:
            require(page['id'] not in ids, 'ID collides with an existing page: ' + page['id'])
            additions.append(page)
            ids.add(page['id'])
    catalog['pages'].extend(additions)
    require(catalog['pages'][:len(original['pages'])] == original['pages'], 'Existing pages changed')
    require({k: v for k, v in catalog.items() if k != 'pages'} ==
            {k: v for k, v in original.items() if k != 'pages'}, 'Catalog header changed')
    return catalog, {'scope': 'Fabius Requiem Normal five Katana/four Punhos reviewed attack families',
        'before': counts(original['pages']), 'after': counts(catalog['pages']),
        'addedSequences': len(additions), 'addedFrames': sum(len(p['spriteNames']) for p in additions),
        'families': [{'weapon': w, 'attackKey': key, 'directions': sorted(p['direction'] for p in group),
                      'frames': sum(len(p['spriteNames']) for p in group)} for (w, key), group in groups.items()],
        'directedKeyNormalization': mappings, 'sheets': sheets, 'preservedFiles': protected,
        'catalogBeforeSha256': sha(CATALOG),
        'validation': {'reviewedArchiveAndRuntimeHashes': True, 'metasAndGuidsPreserved': True,
            'directionsAndContextsUnique': True, 'rectanglesNonOverlapping': True,
            'pivotsDensitiesAndTimingsPreserved': True, 'previousPagesUnchanged': True,
            'onlyIncomingPunhosAttackKeysNormalized': True, 'sourcePixelsModified': False,
            'unityExecuted': False, 'runtimeVerified': False, 'currentVisualReviewPerformed': False}}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    catalog, receipt = plan()
    if args.apply:
        require(sha(CATALOG) == receipt['catalogBeforeSha256'], 'Catalog changed during planning; rerun')
        if receipt['addedSequences']:
            CATALOG.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        _, repeated = plan()
        require(repeated['addedSequences'] == 0, 'Idempotence replan failed')
        require(all(sha(ROOT / path) == digest for path, digest in receipt['preservedFiles'].items()), 'Protected artifact changed')
        receipt['catalogAfterSha256'] = sha(CATALOG)
        receipt['validation']['idempotentReplan'] = True
        if receipt['addedSequences'] or not RECEIPT.exists():
            RECEIPT.parent.mkdir(parents=True, exist_ok=True)
            RECEIPT.write_text(json.dumps(receipt, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: receipt[k] for k in ('addedSequences', 'addedFrames', 'before', 'after')}))


if __name__ == '__main__':
    main()
