"""Read catalog selection and whole preview figures; never alter source artwork.

This is a static descriptor audit, not Unity execution or visual approval.
"""
from pathlib import Path
import json
import re
import struct
import hashlib
import math
from collections import deque
from PIL import Image
import numpy as np
from whole_figure_components import identify
from inventory_animation_coverage import scalar

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/fidelidade-original-20261018'


def catalog(name):
    source = (ROOT / 'Assets/Darklight/Resources/Darklight' / name).read_text(encoding='utf-8-sig')
    # Reuse the existing catalog audit's fixed-format Unity text reader.
    key = 'name' if name == 'TodayCharacterCatalog.asset' else 'baseName'
    pages = []
    for block in re.split(r'^  - ' + key + ': ', source, flags=re.M)[1:]:
        page = {key: block.splitlines()[0]}
        for field in ['kind', 'resourcePath', 'paletteResourcePath', 'attackKey', 'directions']:
            page[field] = scalar(block, field)
        for field in ['skin', 'age', 'weapon', 'phases', 'form']:
            page[field] = int(scalar(block, field, '0'))
        reference_section = block.split('    references:', 1)[-1] if '    references:' in block else ''
        page['references'] = [{'resourcePath': path.strip()} for path in re.findall(r'^    - resourcePath: (.*)$', reference_section, re.M)]
        pages.append(page)
    return pages


def directions(page):
    raw = str(page['directions']).removesuffix('i')
    return list(struct.unpack('<' + 'i' * (len(raw) // 8), bytes.fromhex(raw)))


def ppu(resource):
    path = ROOT / ('Assets/Darklight/Resources/' + resource + '.png.meta')
    match = re.search(r'spritePixelsToUnits: ([\d.]+)', path.read_text()) if path.exists() else None
    return float(match[1]) if match else None


def audit_catalog():
    pages = catalog('TodayCharacterCatalog.asset')
    transformed = catalog('TransformedCatalog.asset')
    for page in pages:
        page['views'] = directions(page)
    rows = []
    for skin in range(2):
        for weapon in range(3):
            for direction in range(8):
                for state, wanted in [('Idle', 'Idle'), ('Walk', 'Walk'), ('Attack', 'Attack'),
                                      ('Hurt', 'Combat'), ('Dodge', 'GuardDodge'), ('StartMove', 'StartMove')]:
                    candidates = [p for p in pages if p['skin'] == skin and p['weapon'] == weapon
                                  and p['age'] == 0 and direction in p['views'] and not p.get('attackKey')]
                    page = next((p for p in candidates if p['kind'] == wanted), None)
                    if page is None and wanted in ('Idle', 'Attack'):
                        page = next((p for p in candidates if p['kind'] == 'Combat'), None)
                    if page is None and wanted == 'StartMove':
                        page = next((p for p in candidates if p['kind'] == 'Motion'), None)
                    for form in range(4):
                        resource = page['resourcePath'] if page else None
                        substitutions = []
                        if page and form:
                            tp = next((p for p in transformed if p['baseName'] == page['name'] and p['form'] == form), None)
                            if tp:
                                if tp.get('references'):
                                    start = page['views'].index(direction) * page['phases']
                                    substitutions = sorted({r['resourcePath'] for r in tp['references'][start:start + page['phases']] if r})
                                elif tp.get('resourcePath'):
                                    substitutions = [tp['resourcePath']]
                        rows.append(dict(skin=['Requiem', 'Rock'][skin], weapon=['Katana', 'Adagas', 'Corrente'][weapon],
                                         direction=direction, state=state, form=form, page=page['name'] if page else None,
                                         kind=page['kind'] if page else None, phases=page['phases'] if page else None,
                                         normalResource=resource, normalPPU=ppu(resource) if resource else None,
                                         transformedResources=substitutions))
    report = dict(method='Static generic descriptor selection matching Page/Resolve/LivingPose priority; no Unity execution.',
                  limits=['Does not simulate named attackKey overrides, LivingPresentation state timing, or failed texture loads.',
                          'Abyss rider uses a separate bank and is deliberately excluded.',
                          'Missing transformedResources can mean shader fallback; it is not proof of absent rendering.',
                          'Counts, PPU and source names do not establish visual fidelity.'], rows=rows)
    (OUT / 'selecao-catalogo.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    return rows


def audit_previews():
    previews = []
    for skin in ['Requiem', 'Rock']:
        sheets, all_frames = [], []
        for suffix in ['estudo', 'intermediarios']:
            sheet, figures = measure_sheet(skin, suffix, len(all_frames))
            sheets.append(sheet)
            all_frames.extend(figures)
        sheet, passing = measure_sheet(skin, 'passagens', len(all_frames), rows=2, columns=2)
        # One uniform scale per entire new sheet, never stretch individual limbs/frames.
        sheet_scale = all_frames[0]['bodyHeight'] / float(np.median([f['bodyHeight'] for f in passing]))
        for frame in passing:
            frame['renderScale'] = sheet_scale
        sheet['uniformRenderScale'] = sheet_scale
        sheets.append(sheet)
        all_frames.extend(passing)
        sheet, attack_figures = measure_sheet(skin, 'ataque-continuo', len(all_frames))
        # Calibrate a whole sheet using its neutral poses, not its raised sword.
        attack_scale = all_frames[0]['bodyHeight'] / float(np.mean([attack_figures[i]['bodyHeight'] for i in [0, 11]]))
        for frame in attack_figures:
            frame['renderScale'] = attack_scale
        sheet['uniformRenderScale'] = attack_scale
        sheets.append(sheet)
        all_frames.extend(attack_figures)
        attack = [f['index'] for f in attack_figures]
        directional = {'S': [12, 1, 24, 25, 3, 4, 26, 27]}
        for suffix, rows_of_directions in [('cardinais', {1:'W', 2:'N', 3:'E'}),
                                           ('diagonais', {0:'SW', 1:'NW', 2:'NE', 3:'SE'})]:
            sheet, figures = measure_sheet(skin, suffix, len(all_frames), rows=4, columns=8)
            scale = all_frames[0]['bodyHeight'] / float(np.median([f['bodyHeight'] for f in figures]))
            for frame in figures:
                frame['renderScale'] = scale
            sheet['uniformRenderScale'] = scale
            for row, direction in rows_of_directions.items():
                directional[direction] = [f['index'] for f in figures[row*8:(row+1)*8]]
            sheets.append(sheet)
            all_frames.extend(figures)
        previews.append(dict(skin=skin, sheets=sheets, frames=all_frames,
                             baseStudyProvisionallyApproved=True, productionImported=False,
                             cycles=dict(idle=[0, 22, 23, 22, 0], walk=[12, 1, 24, 25, 3, 4, 26, 27], attack=attack),
                             durationsMs=dict(idle=[1700, 100, 80, 100, 900], walk=[105, 110, 110, 115]*2,
                                              attack=[90, 75, 70, 65, 55, 30, 35, 40, 50, 85, 90, 100]),
                             attackRestMs=420, slashPhaseRange=[5, 8],
                             directionalWalk=directional,
                             previewAlignment='Hair median X and whole-body foot Y; comparison only, not a gameplay pivot.'))
    (OUT / 'manifesto-estudos.json').write_text(json.dumps(dict(origin='top-left', revision=5, previews=previews), indent=2), encoding='utf-8')
    return previews


def measure_sheet(skin, suffix, offset, rows=3, columns=4):
        path = OUT / (skin + '-' + suffix + '.png')
        im = Image.open(path).convert('RGBA')
        figures, owners = identify(im, rows=rows, columns=columns)
        alpha = np.array(im.getchannel('A'))
        rgb = np.array(im).astype(np.int16)
        r, g, b = rgb[:, :, 0], rgb[:, :, 1], rgb[:, :, 2]
        hair = (r > 60) & (r < 165) & (g > 35) & (b > 25) & (r-g > 10) & (r-g < 45) & (g-b > 4) & (g-b < 30)
        for frame in figures:
            owner_id = frame['index'] + 1
            x0, y0, x1, y1 = frame['grid']
            region = (owners == owner_id) & hair
            region[y0 + int((y1-y0)*.5):] = False
            _, xs = np.where(region)
            frame['previewPivot'] = [float(np.median(xs)), frame['anchor'][1]]
            core_y = int(frame['anchor'][1] - frame['bodyHeight']*.37)
            xx = np.indices(owners.shape)[1]
            core = (owners == owner_id) & (r < 95) & (g < 95) & (b < 100)
            core &= abs(xx - frame['previewPivot'][0]) < frame['bodyHeight']*.2
            core[:max(0,core_y-7)] = False
            core[core_y+7:] = False
            _, hip_xs = np.where(core)
            frame['walkPivot'] = [float(np.median(hip_xs)), frame['anchor'][1]]
            frame['renderScale'] = 1
            blade = (owners == owner_id) & (r > 130) & (g < 110) & (b < 110) & (r-g > 70) & (r-b > 60)
            # Eyes share the red palette. Track only the largest connected blade
            # region, so slash anchors cannot attach to the face.
            blade = largest_component(blade)
            ys, xs = np.where(blade)
            assert len(xs) > 100, (path.name, frame['index'], 'No blade samples')
            points = np.column_stack((xs, ys))
            chest = np.array([frame['previewPivot'][0], frame['anchor'][1] - frame['bodyHeight'] * .5])
            far = np.argmax(((points - chest)**2).sum(axis=1))
            tip = points[far]
            # Red blade's end nearest the hands; used only to anchor runtime slash ribbons.
            near = np.argmin(((points - chest)**2).sum(axis=1))
            frame['bladeTip'] = tip.tolist()
            frame['bladeRoot'] = points[near].tolist()
            frame['sourceIndex'] = frame['index']
            frame['index'] += offset
            frame['ownerId'] = owner_id
            frame['sheet'] = path.name
            frame['pixelHash'] = hashlib.sha256(np.array(im)[owners == owner_id].tobytes()).hexdigest()
        # Technical whole-figure owner labels, not a recolored/exported art asset.
        owner_path = skin + '-' + suffix + '-ownership.png'
        Image.fromarray(owners).save(OUT / owner_path)
        return dict(file=path.name, ownerFile=owner_path, size=list(im.size),
                    alphaRange=list(im.getchannel('A').getextrema()), transparentPixels=int((alpha == 0).sum())), figures


def largest_component(mask):
    visited = np.zeros(mask.shape, dtype=bool)
    largest = []
    height, width = mask.shape
    for y, x in np.argwhere(mask):
        if visited[y, x]:
            continue
        queue = deque([(int(y), int(x))])
        visited[y, x] = True
        component = []
        while queue:
            yy, xx = queue.popleft()
            component.append((yy, xx))
            for dy in [-1, 0, 1]:
                for dx in [-1, 0, 1]:
                    ny, nx = yy + dy, xx + dx
                    if 0 <= ny < height and 0 <= nx < width and mask[ny, nx] and not visited[ny, nx]:
                        visited[ny, nx] = True
                        queue.append((ny, nx))
        if len(component) > len(largest):
            largest = component
    selected = np.zeros(mask.shape, dtype=bool)
    if largest:
        yy, xx = np.array(largest).T
        selected[yy, xx] = True
    return selected


def validate_selected(previews):
    issues = []
    for page in previews:
        assert len(page['directionalWalk']) == 8
        cycles = dict(page['directionalWalk'], idle=page['cycles']['idle'], attack=page['cycles']['attack'])
        for name, indices in cycles.items():
            if name not in ('idle', 'attack'):
                assert len(indices) == len(set(indices)) == 8
            for index in indices:
                f = page['frames'][index]
                s = f['renderScale']
                px, py = f['walkPivot']
                x0, y0, x1, y1 = f['rect']
                bounds = [260+(x0-px)*s, 378+(y0-py)*s, 260+(x1-px)*s, 378+(y1-py)*s]
                if not all(math.isfinite(v) for v in bounds) or bounds[0] < 0 or bounds[1] < 0 or bounds[2] > 520 or bounds[3] > 400:
                    issues.append(dict(skin=page['skin'], cycle=name, frame=index, bounds=bounds))
                if f['touchesExtractionEdge']:
                    issues.append(dict(skin=page['skin'], frame=index, sourceEdge=True))
    result = dict(sourceFigures=sum(len(p['frames']) for p in previews), walkDirectionsPerSkin=8,
                  walkFramesPerDirection=8, attackFramesPerSkin=12, boundsIssues=issues,
                  productionImported=False, visualApproval=False)
    (OUT / 'verificacao-previa.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
    assert not issues, issues


if __name__ == '__main__':
    rows = audit_catalog()
    previews = audit_previews()
    validate_selected(previews)
    print(json.dumps(dict(staticSelections=len(rows), previewFigures=sum(len(p['frames']) for p in previews),
                          previewEdgeContacts=sum(f['touchesExtractionEdge'] for p in previews for f in p['frames']),
                          uniqueWholeFigureHashes=len({f['pixelHash'] for p in previews for f in p['frames']}),
                          walkFramesPerSkin=8, attackFramesPerSkin=12,
                          productionImported=False)))
