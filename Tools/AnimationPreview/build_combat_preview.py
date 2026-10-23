"""Build descriptors for intact generated figures. Never redraw source pixels."""
from pathlib import Path
import json
import hashlib
import math
import sys
import numpy as np
from PIL import Image
from whole_figure_components import identify
from audit_visual_selection import largest_component

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/fidelidade-original-20261018'
PACK = OUT / 'pacote-v6'
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
FORMS = ['normal', 'vermelha', 'roxa', 'branca']


def measure(asset):
    path = PACK / asset['file']
    anatomical_height = positive(asset.get('calibration', {}).get('bodyHeightPx'), 'calibration.bodyHeightPx')
    source_hash=hashlib.sha256(path.read_bytes()).hexdigest()
    cache_path=path.with_name(path.stem+'-medidas.json')
    if cache_path.exists():
        cached=json.loads(cache_path.read_text(encoding='utf-8'))
        if (cached.get('revision') == 3 and cached.get('sha256') == source_hash
                and cached.get('anatomicalBodyHeightPx') == anatomical_height
                and cached.get('rows') == asset['rows'] and cached.get('columns') == asset['columns']
                and (PACK / cached['ownerFile']).exists()):
            return cached['sheet'], cached['figures']
    im = Image.open(path).convert('RGBA')
    figures, owner = identify(im, rows=asset['rows'], columns=asset['columns'], minimum_area=650)
    pixels = np.asarray(im)
    r, g, b = [pixels[:, :, i].astype(np.int16) for i in range(3)]
    dark = (r < 100) & (g < 100) & (b < 110)
    # Both red and violet weapon surfaces; skin and white hair are excluded.
    weapon = ((r > 135) & (g < 115) & (r-g > 55) & (r-b > 45)) | ((b > 120) & (g < 105) & (b-g > 55))
    if asset.get('form') == 'branca':
        weapon[:] = False  # Red eyes must never become white-form weapon anchors.
    for f in figures:
        oid = f['index'] + 1
        owned = owner == oid
        x0, y0, x1, y1 = f['rect']
        yy, xx = np.where(owned & dark)
        if not len(xx):
            raise ValueError((path.name, f['index'], 'No dark body pixels'))
        # Dark-pixel maximum is only an approximate foot Y; a low weapon or coat
        # can bias it. calibration.pivotOverrides supplies manual support points.
        foot_y = int(yy.max()+1)
        # Fixed anatomical band/chest offsets for the whole sheet. A raised
        # weapon or long hair must not move the inferred pelvis/chest band.
        height = anatomical_height
        band = owned & dark
        band[:max(0, int(foot_y-height*.42))] = False
        band[int(foot_y-height*.31):] = False
        _, hx = np.where(band)
        pivot_x = float(np.median(hx)) if len(hx) else float(np.median(xx))
        blade = largest_component(owned & weapon)
        by, bx = np.where(blade)
        chest = np.array([pivot_x, foot_y-height*.52])
        if len(bx) >= 15:
            points = np.column_stack((bx, by))
            distance = ((points-chest)**2).sum(axis=1)
            f['bladeRoot'] = points[np.argmin(distance)].tolist()
            f['bladeTip'] = points[np.argmax(distance)].tolist()
        else:
            # Explicit absence is preferable to anchoring effects to a red eye.
            f['bladeRoot'] = None
            f['bladeTip'] = None
        f['weaponAnchors'] = []
        remaining = owned & weapon
        for _ in range(2):
            component = largest_component(remaining)
            ay, ax = np.where(component)
            if len(ax) < 35:
                break
            points = np.column_stack((ax, ay)); distance = ((points-chest)**2).sum(axis=1)
            f['weaponAnchors'].append(dict(root=points[np.argmin(distance)].tolist(),tip=points[np.argmax(distance)].tolist(),center=points.mean(axis=0).tolist()))
            remaining &= ~component
        f.update(ownerId=oid, sheet='pacote-v6/'+path.name,
                 walkPivot=[pivot_x, foot_y], previewPivot=[pivot_x, foot_y],
                 pixelHash=hashlib.sha256(pixels[owned].tobytes()).hexdigest())
    owner_file = path.stem+'-ownership.png'
    Image.fromarray(owner).save(PACK / owner_file)
    sheet=dict(file='pacote-v6/'+path.name, ownerFile='pacote-v6/'+owner_file,
                size=list(im.size), alphaRange=list(im.getchannel('A').getextrema()),
                sha256=source_hash)
    cache_path.write_text(json.dumps(dict(revision=3,sha256=source_hash,ownerFile=owner_file,
        anatomicalBodyHeightPx=anatomical_height,rows=asset['rows'],columns=asset['columns'],
        sheet=sheet,figures=figures),indent=2),encoding='utf-8')
    return sheet, figures


def positive(value, label):
    if isinstance(value, bool) or not isinstance(value, (int, float)) or not math.isfinite(value) or value <= 0:
        raise ValueError(f'{label} must be a positive finite number')
    return float(value)


def calibration(asset):
    """Read manually marked anatomy; silhouette/weapon extents never set scale."""
    data = asset.get('calibration')
    if not isinstance(data, dict):
        raise ValueError(f"{asset['file']}: missing calibration")
    body = positive(data.get('bodyHeightPx'), 'calibration.bodyHeightPx')
    target = positive(data.get('targetBodyHeightPx'), 'calibration.targetBodyHeightPx')
    scale = target / body
    anatomy = dict(file=asset['file'], bodyHeightPx=body, targetBodyHeightPx=target,
                   uniformRenderScale=scale, measurementMethod='Manually marked guard anatomy; excludes weapon and effects.',
                   visualApproval=False)
    for field in ('guardFrame', 'notes', 'referenceFile'):
        if field in data:
            anatomy[field] = data[field]
    if 'headHeightPx' in data:
        head = positive(data['headHeightPx'], 'calibration.headHeightPx')
        if head > body:
            raise ValueError('calibration.headHeightPx cannot exceed bodyHeightPx')
        anatomy.update(headHeightPx=head, headBodyRatio=head / body)
    if 'referenceHeadRatio' in data:
        ratio = positive(data['referenceHeadRatio'], 'calibration.referenceHeadRatio')
        if ratio > 1:
            raise ValueError('calibration.referenceHeadRatio cannot exceed 1')
        anatomy['referenceHeadRatio'] = ratio
        if 'headBodyRatio' in anatomy:
            anatomy['headRatioDelta'] = anatomy['headBodyRatio'] - ratio
    return scale, anatomy


def frame_indices(indices, count, label):
    if not isinstance(indices, list) or not indices:
        raise ValueError(f'{label} must be a nonempty list')
    if any(type(i) is not int or i < 0 or i >= count for i in indices):
        raise ValueError(f'{label} contains an invalid frame index (available: {count})')
    return list(indices)


def timings(values, count, label):
    if not isinstance(values, list) or len(values) != count:
        raise ValueError(f'{label} requires exactly {count} explicit durations')
    return [positive(value, label) for value in values]


def anchor_frame_pairs(asset, current_count, template_count):
    if 'anchorFrameMap' not in asset:
        if current_count != template_count:
            raise ValueError(f"{asset['file']}: anchor template frame count differs; provide anchorFrameMap")
        return list(enumerate(range(template_count)))
    mapping = asset['anchorFrameMap']
    if not isinstance(mapping, dict):
        raise ValueError('anchorFrameMap must map current frame indices to template frame indices')
    pairs, seen = [], set()
    for current, template in mapping.items():
        if type(current) is not int:
            if not isinstance(current, str) or not current.isdecimal():
                raise ValueError('anchorFrameMap current indices must be integers')
            current = int(current)
        if current < 0 or current >= current_count or current in seen:
            raise ValueError('anchorFrameMap contains an invalid or duplicate current frame index')
        if type(template) is not int or template < 0 or template >= template_count:
            raise ValueError('anchorFrameMap contains an invalid template frame index')
        seen.add(current)
        pairs.append((current, template))
    return pairs


def walk_directions(asset, count):
    if 'frameOrder' in asset or 'direction' in asset:
        direction = asset.get('direction', 'S')
        result = {direction: frame_indices(asset.get('frameOrder', list(range(count))), count, 'frameOrder')}
    elif 'directions' in asset:
        if not isinstance(asset['directions'], dict):
            raise ValueError('directions must map direction names to frame index lists')
        result = {d: frame_indices(indices, count, f'directions.{d}') for d, indices in asset['directions'].items()}
    else:
        columns = asset['columns']
        rows = asset.get('directionRows')
        if rows is None:
            if asset['rows'] != len(DIRECTIONS):
                raise ValueError(f"{asset['file']}: provide direction/frameOrder, directions or directionRows")
            rows = dict(enumerate(DIRECTIONS))
        if not isinstance(rows, dict):
            raise ValueError('directionRows must map row indices to direction names')
        result = {}
        for row, direction in rows.items():
            row = int(row)
            if row < 0 or row >= asset['rows']:
                raise ValueError('directionRows contains an invalid row')
            if direction in result:
                raise ValueError('directionRows contains a duplicate direction')
            result[direction] = frame_indices(list(range(row * columns, (row + 1) * columns)), count, 'directionRows')
    if not result or any(d not in DIRECTIONS for d in result):
        raise ValueError('Walking directions must use S, SW, W, NW, N, NE, E, SE')
    for direction, indices in result.items():
        timings(asset.get('durationsMs'), len(indices), f'{asset["file"]} durationsMs ({direction})')
    return result


def selected_indices(page):
    selected = set()
    if not page['directions']:
        raise ValueError('Page selects no directions')
    for direction, indices in page['directions'].items():
        selected.update(frame_indices(indices, len(page['frames']), f'directions.{direction}'))
        timings(page['durationsMs'], len(indices), 'durationsMs')
    for name, variant in page.get('variants', {}).items():
        indices = frame_indices(variant['indices'], len(page['frames']), f'variants.{name}.indices')
        selected.update(indices)
        timings(variant.get('durationsMs'), len(indices), f'variants.{name}.durationsMs')
        if any(type(i) is not int or i < 0 or i >= len(indices) for i in variant.get('activePhases', [])):
            raise ValueError(f'variants.{name}.activePhases contains an invalid phase')
    phase_count = len(page['durationsMs'])
    if any(type(i) is not int or i < 0 or i >= phase_count for i in page.get('activePhases', [])):
        raise ValueError('activePhases contains an invalid phase')
    if not selected:
        raise ValueError('Page selects no figures')
    return sorted(selected)


def build():
    sheets, pages, measured, anatomy_checks, scale_checks = [], [], {}, [], []
    source_counts, problems, selected_pages, selected_sources = {}, [], [], set()
    report = dict(revision=7, success=False, productionImported=False, visualApproval=False,
                  problems=problems, anatomyChecks=anatomy_checks, scaleChecks=scale_checks,
                  selectedPages=selected_pages,
                  limits=['Source pixels and anatomical proportions are not repaired by rendering scale.',
                          'Connected-component bodyHeight is a whole-figure extent, not an anatomical measurement.',
                          'Scale, anatomy and framing are separate checks; none establishes visual fidelity or approval.',
                          'Preview coordinates are canvas pixels, independent of CSS size and Unity PPU/camera.'])
    try:
        plan = json.loads((PACK / 'fontes.json').read_text(encoding='utf-8'))
        palette_path=PACK/'paleta-pele.json'
        palette_data=json.loads(palette_path.read_text(encoding='utf-8')) if palette_path.exists() else None
        palette={e['file']:e for e in palette_data['entries']} if palette_data else {}
        if plan.get('includeLegacyWalk', False):
            legacy_calibration = plan.get('legacyCalibration', {})
            legacy_scale = positive(legacy_calibration.get('uniformRenderScale'), 'legacyCalibration.uniformRenderScale')
            if not isinstance(legacy_calibration.get('notes'), str) or not legacy_calibration['notes'].strip():
                raise ValueError('legacyCalibration.notes must document the global anatomical reference adjustment')
            report['legacyCalibration'] = legacy_calibration
            legacy = json.loads((OUT / 'manifesto-estudos.json').read_text(encoding='utf-8'))
            for preview in legacy['previews']:
                sheets.extend(preview['sheets'])
                for f in preview['frames']:
                    source_counts.setdefault(f['sheet'], set()).add(f['index'])
                # Retain every pre-existing per-sheet scale; only the explicit,
                # documented anatomical reference factor is applied globally.
                copied = [dict(f, renderScale=f['renderScale'] * legacy_scale) for f in preview['frames']]
                pages.append(dict(skin=preview['skin'], weapon='katana', form='normal', kind='walk',
                                  frames=copied, directions=preview['directionalWalk'],
                                  durationsMs=preview['durationsMs']['walk'], legacy=True,
                                  legacyGlobalRenderScale=legacy_scale))
        for asset in plan['assets']:
            if not asset.get('selected', True):
                continue
            if 'anchorFrameMap' in asset and not asset.get('anchorSource'):
                raise ValueError('anchorFrameMap requires anchorSource')
            scale, anatomy = calibration(asset)
            anatomy_checks.append(anatomy)
            sheet, figures = measure(asset)
            sheet = dict(sheet, uniformRenderScale=scale)
            if palette_data:
                correction=palette.get(sheet['file'])
                if correction is None or correction['sha256']!=sheet['sha256']:
                    raise ValueError('Skin palette metadata does not match this source: '+sheet['file'])
                sheet['skinPalette']=dict(referenceMedian=palette_data['referenceMedian'],shiftRgb=correction['shiftRgb'])
            source_counts.setdefault(sheet['file'], set()).update(f['index'] for f in figures)
            overrides = asset['calibration'].get('pivotOverrides', {})
            if not isinstance(overrides, dict):
                raise ValueError('calibration.pivotOverrides must map frame indices to [x,y]')
            for index, pivot in overrides.items():
                index = int(index)
                if index < 0 or index >= len(figures) or not isinstance(pivot, list) or len(pivot) != 2:
                    raise ValueError('calibration.pivotOverrides requires valid frame indices and [x,y]')
                if any(isinstance(v, bool) or not isinstance(v, (int, float)) or not math.isfinite(v) for v in pivot):
                    raise ValueError('calibration.pivotOverrides coordinates must be finite numbers')
                figures[index]['walkPivot'] = list(pivot)
                figures[index]['previewPivot'] = list(pivot)
                figures[index]['pivotMethod'] = 'Manual whole-figure foot/pelvis registration'
            if asset.get('anchorSource'):
                template, template_body = measured[asset['anchorSource']]
                pairs = anchor_frame_pairs(asset, len(figures), len(template))
                # An explicit partial map transfers only registered whole-pose
                # pairs. Unmapped/merged figures receive no inferred FX anchors.
                for f in figures:
                    f['weaponAnchors'] = []
                    f['bladeRoot'] = None
                    f['bladeTip'] = None
                    f['anchorMethod'] = 'No registered template pair; weapon effects disabled'
                # Effects can follow manually marked body resolution; raised
                # weapons/whole-component bounds must not control their scale.
                ratio = anatomy['bodyHeightPx'] / template_body
                for current_index, template_index in pairs:
                    f, normal = figures[current_index], template[template_index]
                    def transfer(point):
                        return [f['walkPivot'][i] + (point[i] - normal['walkPivot'][i]) * ratio for i in range(2)]
                    f['weaponAnchors'] = [{k: transfer(p) for k, p in a.items()} for a in normal['weaponAnchors']]
                    f['anchorMethod'] = 'Normal whole-pose anchors transferred using manual anatomical calibration; preview only'
                    f['anchorTemplateFrame'] = template_index
            for f in figures:
                f['renderScale'] = scale
                f['wholeFigureExtentHeightPx'] = f['bodyHeight']
                if f['touchesExtractionEdge']:
                    problems.append(dict(file=asset['file'], frame=f['index'], issue='source-edge'))
            # Explicitly exclude a misclassified visual landmark (for example a
            # red eye in a pose where the second dagger is hidden). No source
            # pixels or hidden weapons are manufactured by this metadata edit.
            for frame_index, anchors in asset.get('anchorExclusions', {}).items():
                index = int(frame_index)
                if index < 0 or index >= len(figures) or not isinstance(anchors, list):
                    raise ValueError('anchorExclusions requires valid frame indices and lists')
                current = figures[index]['weaponAnchors']
                if any(type(i) is not int or i < 0 or i >= len(current) for i in anchors):
                    raise ValueError('anchorExclusions contains an invalid anchor index')
                figures[index]['weaponAnchors'] = [a for i, a in enumerate(current) if i not in anchors]
                figures[index]['anchorExclusionReason'] = asset.get('anchorExclusionReason', 'Manual visual landmark audit')
            measured[asset['file']] = figures, anatomy['bodyHeightPx']
            sheets.append(sheet)
            scale_checks.append(dict(file=asset['file'], method='targetBodyHeightPx / bodyHeightPx',
                                     uniformRenderScale=scale, wholeSheetUniform=True,
                                     anatomicalHeightAfterScalePx=anatomy['bodyHeightPx'] * scale))
            common = dict(skin=asset['skin'], weapon=asset.get('weapon', 'katana'),
                          uniformRenderScale=scale, calibration=anatomy)
            if asset['kind'] == 'walk':
                directions = walk_directions(asset, len(figures))
                pages.append(dict(common, form=asset.get('form', 'normal'), kind='walk', frames=figures,
                                  directions=directions, durationsMs=asset['durationsMs']))
                if 'idleFrame' in asset:
                    idle = frame_indices([asset['idleFrame']], len(figures), 'idleFrame')
                    pages.append(dict(common, form=asset.get('form', 'normal'), kind='idle',
                                      frames=figures, directions={'S': idle}, durationsMs=[2600]))
            elif asset['kind'] == 'attack':
                if 'form' not in asset and (asset['rows'], asset['columns']) != (6, 8):
                    raise ValueError('Attack source without form must be the explicit 6x8 four-form layout')
                for form_index, form in enumerate([asset['form']] if 'form' in asset else FORMS):
                    if 'form' in asset:
                        indices = frame_indices(asset.get('frameOrder', list(range(len(figures)))), len(figures), 'frameOrder')
                    else:
                        block_row, block_col = divmod(form_index, 2)
                        indices = [(block_row * 3 + row) * 8 + block_col * 4 + col for row in range(3) for col in range(4)]
                    selected = [dict(figures[i]) for i in indices]
                    duration = timings(asset.get('durationsMs'), len(selected), f'{asset["file"]} durationsMs')
                    direction = asset.get('direction', 'S')
                    if direction not in DIRECTIONS:
                        raise ValueError('Invalid attack direction')
                    rest = asset.get('restMs', 470)
                    if isinstance(rest, bool) or not isinstance(rest, (int, float)) or not math.isfinite(rest) or rest < 0:
                        raise ValueError('restMs must be a nonnegative finite number')
                    page = dict(common, form=form, kind='attack', frames=selected,
                                directions={direction: list(range(len(selected)))}, durationsMs=duration,
                                activePhases=asset.get('activePhases', []), restMs=rest,
                                events=asset.get('events', []))
                    if asset.get('variants'):
                        page['variants'] = asset['variants']
                    pages.append(page)
            else:
                raise ValueError(f"Unknown asset kind: {asset['kind']}")
        for page in pages:
            indices = selected_indices(page)
            selected_pages.append(dict(skin=page['skin'], weapon=page['weapon'], form=page['form'],
                                       kind=page['kind'], selectedFigures=len(indices),
                                       storedFigures=len(page['frames']),
                                       directions={d: len(v) for d, v in page['directions'].items()},
                                       sourceFrames=[dict(sheet=page['frames'][i]['sheet'], index=page['frames'][i]['index']) for i in indices]))
            for index in indices:
                f = page['frames'][index]
                selected_sources.add((f['sheet'], f['index']))
                x0, y0, x1, y1 = f['rect']
                px, py = f['walkPivot']
                scale = positive(f['renderScale'], 'frame.renderScale')
                bounds = [260 + (x0 - px) * scale, 450 + (y0 - py) * scale,
                          260 + (x1 - px) * scale, 450 + (y1 - py) * scale]
                context = dict(skin=page['skin'], weapon=page['weapon'], form=page['form'],
                               kind=page['kind'], sheet=f['sheet'], frame=f['index'])
                if f.get('touchesExtractionEdge') and page.get('legacy'):
                    problems.append(dict(context, issue='source-edge'))
                if (not all(math.isfinite(v) for v in bounds) or bounds[0] < 0 or bounds[1] < 0
                        or bounds[2] > 640 or bounds[3] > 480):
                    problems.append(dict(context, issue='preview-bounds', bounds=bounds))
    except (OSError, ValueError, KeyError, IndexError, TypeError) as error:
        problems.append(dict(issue='invalid-plan-or-source', message=str(error)))
    report.update(sourceFigures=sum(len(v) for v in source_counts.values()),
                  sources=[dict(file=k, figures=len(v)) for k, v in source_counts.items()],
                  selectedFigures=sum(p['selectedFigures'] for p in selected_pages),
                  selectedUniqueSourceFigures=len(selected_sources),
                  pages=len(pages), walkPages=sum(p['kind'] == 'walk' for p in pages),
                  attackPages=sum(p['kind'] == 'attack' for p in pages),
                  framingCheck=dict(canvasSize=[640, 480], footPivot=[260, 450], automaticShrink=False),
                  success=not problems)
    (PACK / 'verificacao.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    if problems:
        print(json.dumps(report, ensure_ascii=False))
        sys.exit(1)
    # Deduplicate sheets shared by legacy pages while preserving page ordering.
    sheets = list({sheet['file']: sheet for sheet in sheets}.values())
    (PACK / 'manifesto.json').write_text(json.dumps(dict(revision=7, sheets=sheets, pages=pages,
            productionImported=False), ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({key:report[key] for key in ['success','sourceFigures','selectedUniqueSourceFigures','pages','walkPages','attackPages','framingCheck']}, ensure_ascii=False))


if __name__ == '__main__':
    build()
