"""Configure reviewed whole-figure stances without Unity or pixel modifications.

Input: schemaVersion1/sheets[]; each sheet supplies name, sourcePath/Sha256,
status=visual-reviewed, wholeFigures, representative measured bodyHeightPx PER
SHEET/TextureImporter and frames[]. Directions sharing a PNG share that one PPU.
The generator never copies a paired PNG twice to assign per-direction densities.
Each
frame supplies direction (S,SW,W,NW,N,NE,E,SE), action (Idle/Walk), phase0..N-1,
rect[x,y,width,height] top-left, ground[x,y] GLOBAL top-left coordinates and
durationMs. ground is a reviewed projected floor anchor, never an alpha bound.
Optional sheet clips[] supplies direction/action/sequence (phase indices) and
durationMs per playback step, e.g. Walk [0,1,2,1] reuses the passing figure.
Idle requires >=2 unique figures, Walk >=3; all16contexts must exist to activate.
The original approved S source must be referenced directly, without repacking.

Default is read-only planning; --apply configures only the new Resources folder.
Partial packets are allowed but cannot activate the all-or-nothing runtime bank.
"""
import argparse
import copy
import hashlib
import json
import math
import re
import uuid
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/FistsCombatStance20261118'
RESOURCE = 'Darklight/FistsCombatStance20261118/'
RECIPE = ROOT / 'ArtSources/FistsCombatStance20261118/recipe.json'
TEMPLATE = ROOT / 'Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalIdle8V1.png.meta'
RECEIPT = ROOT / 'outputs/retomada-20261118/fists-combat-stance/import-configuration.json'
S_SOURCE = 'ArtSources/FistsCombatStance20261117/selected/FabiusFistsCombatS-selected-v1.png'
S_HASH = '202f33f42d937ab55cc5cc2162b41c8d1881ea3ea329e8d18e04c895de70740e'
BODY_WORLD = 302 / 157.5757576
DIRS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def encoded(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def identity(value):
    return uuid.uuid5(uuid.NAMESPACE_URL, 'darklight-requiem/fists-combat-stance-20261118/' + value).hex


def name(value):
    return isinstance(value, str) and re.fullmatch(r'[A-Za-z0-9_-]+', value) is not None


def asset_guid(path):
    if path.exists():
        match = re.search(r'^guid: ([a-f0-9]{32})$', path.read_text(encoding='utf-8-sig'), re.MULTILINE)
        require(match, 'Existing asset GUID is malformed: ' + str(path))
        return match.group(1)
    return identity(path.relative_to(ROOT).as_posix())


def text_meta(path):
    return ('fileFormatVersion: 2\nguid: ' + asset_guid(path) + '\nTextScriptImporter:\n'
            '  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n').encode()


def texture_meta(path, size, ppu, frames):
    # Reuse a genuine Multiple-sprite TextureImporter template. Rebuild only the
    # new asset's sprite tables; names, GUIDs and internal IDs survive reruns.
    text = TEMPLATE.read_text(encoding='utf-8-sig').replace('\r\n', '\n')
    previous = {}
    if path.exists():
        old = path.read_text(encoding='utf-8-sig')
        for block in re.findall(r'(?ms)^    - serializedVersion:.*?(?=^    - serializedVersion:|^    outline:)', old):
            n = re.search(r'^      name: (\S+)$', block, re.MULTILINE)
            sprite_id = re.search(r'^      spriteID: ([a-f0-9]{32})$', block, re.MULTILINE)
            file_id = re.search(r'^      internalID: (-?\d+)$', block, re.MULTILINE)
            if n and sprite_id and file_id:
                previous[n.group(1)] = (sprite_id.group(1), int(file_id.group(1)))
    entries, internal_ids, table = [], set(), []
    for frame in frames:
        key = path.relative_to(ROOT).as_posix() + '/' + frame['id']
        sid, fid = previous.get(frame['id'], (identity(key), int(identity(key + '/file')[:8], 16) & 0x7fffffff))
        require(fid and fid not in internal_ids, 'Sprite internal ID collision')
        internal_ids.add(fid)
        x, y, w, h = frame['rect']
        px, py = frame['pivot']
        entries.append(f'''    - serializedVersion: 2
      name: {frame['id']}
      rect:
        serializedVersion: 2
        x: {x}
        y: {size[1]-y-h}
        width: {w}
        height: {h}
      alignment: 9
      pivot: {{x: {px:.12g}, y: {py:.12g}}}
      border: {{x: 0, y: 0, z: 0, w: 0}}
      customData:
      outline: []
      physicsShape: []
      tessellationDetail: 0
      bones: []
      spriteID: {sid}
      internalID: {fid}
      vertices: []
      indices:
      edges: []
      weights: []''')
        table.append((frame['id'], fid))
    text = re.sub(r'^guid: .*$', 'guid: ' + asset_guid(path), text, flags=re.MULTILINE)
    importer_table = '  internalIDToNameTable:\n' + ''.join(
        f'  - first:\n      213: {fid}\n    second: {sprite}\n' for sprite, fid in table)
    text = re.sub(r'(?ms)^  internalIDToNameTable:.*?(?=^  [A-Za-z])', importer_table, text, count=1)
    for field, value in {'spritePixelsToUnits': format(ppu, '.12g'), 'spriteMode': 2, 'textureType': 8,
                         'spriteMeshType': 0, 'nPOTScale': 0, 'maxTextureSize': 8192,
                         'isReadable': 0, 'alphaIsTransparency': 1, 'spriteGenerateFallbackPhysicsShape': 0}.items():
        text = re.sub(rf'^  {field}:.*$', f'  {field}: {value}', text, flags=re.MULTILINE)
    for field, value in {'enableMipMap': 0, 'filterMode': 0, 'maxTextureSize': 8192,
                         'textureFormat': 4, 'textureCompression': 0, 'crunchedCompression': 0,
                         'wrapU': 1, 'wrapV': 1, 'wrapW': 1}.items():
        text = re.sub(rf'^    {field}:.*$', f'    {field}: {value}', text, flags=re.MULTILINE)
    prefix = text.split('  spriteSheet:\n', 1)[0]
    suffix_match = re.search(r'^  mipmapLimitGroupName:', text, re.MULTILINE)
    require(suffix_match is not None, 'Genuine importer template footer missing')
    suffix = text[suffix_match.start():]
    sprite_sheet = '  spriteSheet:\n    serializedVersion: 2\n    sprites:\n' + '\n'.join(entries)
    sprite_sheet += '\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: ' + identity(path.relative_to(ROOT).as_posix() + '/atlas')
    sprite_sheet += '\n    internalID: 0\n    vertices: []\n    indices:\n    edges: []\n    weights: []\n    secondaryTextures: []\n    spriteCustomMetadata:\n      entries: []\n    nameFileIdTable:\n'
    sprite_sheet += ''.join(f'      {sprite}: {fid}\n' for sprite, fid in table)
    return (prefix + sprite_sheet + suffix).encode('utf-8')


def plan(recipe_path):
    recipe = read(recipe_path)
    require(recipe.get('schemaVersion') == 1 and isinstance(recipe.get('sheets'), list), 'Invalid recipe schema')
    if 'bodyWorldPreScale' in recipe:
        require(abs(recipe['bodyWorldPreScale'] - BODY_WORLD) < .000001, 'Reference body-world calibration changed')
    require(sha(ROOT / S_SOURCE) == S_HASH, 'Approved original south source changed; abort without editing it')
    files, pages, contexts, names, frame_ids, records = {}, [], set(), set(), set(), []
    source_hashes = set()
    for sheet in recipe['sheets']:
        sheet_name = sheet['name']
        require(name(sheet_name) and sheet_name not in names, 'Invalid or repeated sheet name')
        names.add(sheet_name)
        source_rel = Path(sheet['sourcePath']).as_posix()
        require(source_rel.startswith('ArtSources/') and '..' not in Path(source_rel).parts, 'Source must remain under ArtSources')
        source = ROOT / source_rel
        require(sheet.get('status') == 'visual-reviewed' and sheet.get('wholeFigures') is True, 'Only reviewed whole sheets may be configured')
        require(sha(source) == sheet['sourceSha256'], 'Reviewed source hash changed: ' + source_rel)
        require(sheet['sourceSha256'] not in source_hashes, 'A shared PNG must use one sheet/PPU, not duplicate full texture assets per direction')
        source_hashes.add(sheet['sourceSha256'])
        with Image.open(source) as image:
            require(image.mode == 'RGBA', 'Source must already be RGBA; automatic conversion is forbidden')
            size = list(image.size)
        body = sheet['bodyHeightPx']
        require(type(body) in (float, int) and math.isfinite(body) and body > 0, 'Measured ready-body height required')
        ppu = body / BODY_WORLD
        require(isinstance(sheet.get('bodyMeasurement'), str) and sheet['bodyMeasurement'], 'Body measurement authority is required')
        frames, groups, rectangles = [], {}, []
        playback = {(c['direction'], c['action']): c for c in sheet.get('clips', [])}
        require(len(playback) == len(sheet.get('clips', [])), 'Repeated playback override')
        for original in sheet['frames']:
            item = copy.deepcopy(original)
            require(not any(k in item for k in ('pixelsPerUnit', 'framePixelsPerUnit', 'bodyHeightPx')),
                    'PPU/bodyHeightPx is per sheet: use a representative measured ready body, not per-frame/direction calibration')
            direction, action, phase = item['direction'], item['action'], item['phase']
            require(direction in DIRS and action in ('Idle', 'Walk') and type(phase) is int and phase >= 0, 'Invalid stance frame context')
            if direction == 'S':
                require(source_rel == S_SOURCE, 'Reference the preserved approved S directly; do not duplicate or redraw it')
            require(len(item['rect']) == 4 and all(type(v) is int for v in item['rect']), 'Real integer rect[x,y,w,h] required')
            x, y, w, h = item['rect']
            require(x >= 0 and y >= 0 and w > 0 and h > 0 and x+w <= size[0] and y+h <= size[1], 'Frame exceeds real source dimensions')
            require(all(not(x < ax+aw and ax < x+w and y < ay+ah and ay < y+h) for ax, ay, aw, ah in rectangles), 'Whole figure rects overlap')
            rectangles.append((x, y, w, h))
            require(len(item['ground']) == 2 and all(type(v) in (float, int) and math.isfinite(v) for v in item['ground']), 'Explicit global ground anchor required')
            gx, gy = item['ground']
            require(x <= gx <= x+w and y <= gy <= y+h, 'Projected ground anchor outside frame')
            require(type(item['durationMs']) is int and item['durationMs'] > 0, 'Positive native frame timing required')
            sprite_id = item.get('id', f'Redesign_FabiusFistsCombat_{direction}_{action}_{phase:02}')
            require(name(sprite_id) and sprite_id.startswith('Redesign_FabiusFistsCombat') and sprite_id not in frame_ids, 'Invalid or duplicate frame ID')
            frame_ids.add(sprite_id)
            frame = dict(id=sprite_id, direction=direction, action=action, phase=phase, rect=[x, y, w, h],
                         ground=[gx, gy], pivot=[(gx-x)/w, 1-(gy-y)/h], durationMs=item['durationMs'])
            frames.append(frame)
            groups.setdefault((direction, action), []).append(frame)
        sheet_clips = []
        require(set(playback) <= set(groups), 'Playback references a missing action/direction')
        for (direction, action), group in groups.items():
            group.sort(key=lambda f: f['phase'])
            require(len(group) >= (2 if action == 'Idle' else 3) and
                    [f['phase'] for f in group] == list(range(len(group))), 'Each directional action needs consecutive unique phases (Idle>=2, Walk>=3)')
            override = playback.get((direction, action), {})
            sequence = override.get('sequence', list(range(len(group))))
            require(isinstance(sequence, list) and sequence and all(type(i) is int and 0 <= i < len(group) for i in sequence) and
                    set(sequence) == set(range(len(group))), 'Playback must reference all unique figures and may repeat their indices')
            timing = override.get('durationMs', [group[i]['durationMs'] for i in sequence])
            require(len(timing) == len(sequence) and all(type(ms) is int and ms > 0 for ms in timing), 'Positive duration required for every playback step')
            sheet_clips.append(dict(name=action+'_'+direction, frames=[group[i]['id'] for i in sequence], durationMs=timing, loop=True))
            key = (direction, action)
            require(key not in contexts, 'Duplicate directional stance context')
            contexts.add(key)
            pages.append(dict(id=f'FabiusFistsCombat_{direction}_{action}', direction=DIRS.index(direction), action=action,
                              resourcePath=RESOURCE+sheet_name, imageSha256=sheet['sourceSha256'], sourcePath=source_rel,
                              sourceSha256=sheet['sourceSha256'], nativeSize=size, pixelsPerUnit=ppu,
                              wholeFigures=True, loop=True, status='visual-reviewed', frames=group, sequence=sequence, durationMs=timing))
        require(frames, 'Empty sheet is not a prepared asset')
        png = DEST / (sheet_name + '.png')
        if png.exists():
            require(sha(png) == sheet['sourceSha256'], 'Existing asset PNG differs; preserve it and choose a new versioned sheet name')
        files[png] = source.read_bytes()
        files[Path(str(png)+'.meta')] = texture_meta(Path(str(png)+'.meta'), size, ppu, frames)
        descriptor = dict(schemaVersion=1, image=png.name, imageSha256=sheet['sourceSha256'], sourcePath=source_rel,
                          sourceSha256=sheet['sourceSha256'], sourceSize=size, nativeSize=size, pixelsPerUnit=ppu,
                          status='visual-reviewed', wholeFigures=True, frames=frames,
                          clips=sheet_clips,
                          calibration=dict(bodyHeightPx=body, bodyMeasurement=sheet['bodyMeasurement'],
                                           bodyWorldPreScale=BODY_WORLD, visualScale=1.325, densityScope='one PPU per source PNG/TextureImporter'),
                          groundMethod='Reviewed projected support/ground; never alpha recentering or airborne sole reanchoring',
                          states=dict(importConfigured=True, imported=False, runtimeVerified=False))
        path = png.with_suffix('.json')
        files[path] = encoded(descriptor);files[Path(str(path)+'.meta')] = text_meta(Path(str(path)+'.meta'))
        records.append(dict(name=sheet_name, sourcePath=source_rel, sourceSha256=sheet['sourceSha256'], nativeSize=size,
                            bodyHeightPx=body, pixelsPerUnit=ppu, frames=len(frames),
                            resourcePath=RESOURCE+sheet_name, textureGuid=asset_guid(Path(str(png)+'.meta')),
                            textureMetaSha256=hashlib.sha256(files[Path(str(png)+'.meta')]).hexdigest(),
                            descriptorSha256=hashlib.sha256(files[path]).hexdigest(),
                            sourcePixelsModified=False, importConfigured=True, imported=False, runtimeVerified=False))
    complete = contexts == {(d, a) for d in DIRS for a in ('Idle', 'Walk')}
    catalog = dict(schemaVersion=1, skin=0, weapon=2, form=0, complete=complete, importConfigured=bool(pages),
                   pages=sorted(pages, key=lambda p: (p['direction'], p['action'])))
    catalog_path = DEST / 'catalog.json'
    if catalog_path.exists():
        require(not read(catalog_path).get('complete') or complete, 'Refusing to demote a complete new stance packet with a partial recipe')
    files[catalog_path] = encoded(catalog)
    files[Path(str(catalog_path)+'.meta')] = text_meta(Path(str(catalog_path)+'.meta'))
    folder_meta = Path(str(DEST)+'.meta')
    files[folder_meta] = ('fileFormatVersion: 2\nguid: ' + asset_guid(folder_meta) +
                         '\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n').encode()
    receipt = dict(recipe=recipe_path.relative_to(ROOT).as_posix(), recipeSha256=sha(recipe_path),
                   selectedSequences=len(pages), configuredFrames=len(frame_ids), configuredUniqueFigures=len(frame_ids),
                   playbackSteps=sum(len(p['sequence']) for p in pages), configuredSheets=len(records),
                   complete=complete, missingContexts=[f'{a}_{d}' for d in DIRS for a in ('Idle', 'Walk') if (d, a) not in contexts],
                   sourceSProtectedSha256=S_HASH, sheets=records, states=dict(importConfigured=bool(pages), imported=False,
                   runtimeVerified=False, unityExecuted=False, testsExecuted=False),
                   runtimeActivation='Requires complete reviewed16contexts (Idle>=2/Walk>=3 unique figures each) plus matching native imported sprites; repeated playback indices reuse cached sprites; otherwise all legacy locomotion remains active')
    return files, receipt


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--recipe', type=Path, default=RECIPE)
    parser.add_argument('--apply', action='store_true')
    args = parser.parse_args()
    recipe = args.recipe.resolve()
    files, receipt = plan(recipe)
    if args.apply:
        # Prepare every byte before the first write; no source/editor/test calls.
        for path, content in files.items():
            path.parent.mkdir(parents=True, exist_ok=True)
            if not path.exists() or path.read_bytes() != content:
                path.write_bytes(content)
        RECEIPT.parent.mkdir(parents=True, exist_ok=True)
        content = encoded(receipt)
        if not RECEIPT.exists() or RECEIPT.read_bytes() != content:
            RECEIPT.write_bytes(content)
    print(json.dumps({k: receipt[k] for k in ('selectedSequences', 'configuredFrames', 'configuredSheets', 'complete', 'missingContexts', 'states')}))


if __name__ == '__main__':
    main()
