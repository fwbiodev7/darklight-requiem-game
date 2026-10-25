"""Register and pack complete authored figures, retaining every nontransparent RGBA pixel."""
from pathlib import Path
import argparse
import hashlib
import json
import io
import math
import os
import shutil
import subprocess
import sys
import time

import numpy as np
from PIL import Image
from whole_figure_components import identify

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'ArtSources/WeaponRedesign20261023'
RES = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT = ROOT / 'outputs/redesign-armas-20261023'
PIPE = ROOT / 'Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py'
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
DURATIONS = [350, 250, 180, 350]
TIMED_ACTIONS = {'Idle', 'Walk', 'Run', 'IdleAlert', 'IdleImpatient', 'IdleInspect',
                 'IdleFatigued', 'IdleWounded', 'StartMove', 'Brake', 'Turn'}
LOOP_ACTIONS = {'Idle', 'Walk', 'Run'}


def frame_durations(job, spec, count):
    durations = spec.get('durationMs') or job.get('durationMs') or (DURATIONS if spec['action']=='Idle' and count==4 else [500]*count)
    if len(durations)!=count or any(type(value) not in (int, float) or not math.isfinite(value) or value<=0 for value in durations):
        raise ValueError('Expected one positive finite duration per complete figure: ' + job['name'])
    return durations


def page_durations(job, spec, count, phases):
    if spec['action'] not in TIMED_ACTIONS or spec.get('sourceSpriteNames'):
        return []
    durations = frame_durations(job, spec, count)
    selected = [durations[phase] for phase in phases]
    if any(value!=int(value) for value in selected):
        raise ValueError('Unity timed pages require integer milliseconds; use exact sourceSpriteNames for fractional legacy phases: ' + job['name'])
    return [int(value) for value in selected]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def write(path, value):
    atomic_bytes(path, json.dumps(value, ensure_ascii=False, indent=2).encode('utf-8'))


def atomic_bytes(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    if path.exists() and path.read_bytes() == value:
        return
    pending = path.with_name('.' + path.name + '.staged')
    pending.write_bytes(value)
    for attempt in range(6):
        try:
            os.replace(pending, path)
            break
        except PermissionError:
            if attempt == 5:
                raise
            time.sleep(.05 * 2 ** attempt)


def preserved_copy(source, destination):
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists():
        if sha(source) != sha(destination):
            raise ValueError('Preserved source differs; use a new version: ' + str(destination))
    else:
        shutil.copy2(source, destination)


def prepare(job):
    name = job['name']
    skin = job.get('skin', 0)
    if type(skin) is not int or skin not in (0, 1):
        raise ValueError('Unknown redesign skin; expected 0 Requiem or 1 Rock: ' + name)
    original = Path(job['source'])
    source_name = job.get('preservedSourceName', name + '-source.png')
    if Path(source_name).name != source_name or not source_name.endswith('.png'):
        raise ValueError('Expected a versioned source PNG filename: ' + str(source_name))
    source = ART / source_name
    preserved_copy(original, source)
    image = Image.open(source).convert('RGBA')
    rgba = np.array(image)
    columns = job.get('columns', 4)
    rows = job.get('rows') or [dict(sourceRow=i, direction=direction, action='Idle')
                               for i, direction in enumerate(job['directions'])]
    source_rows = job.get('sourceRows', len(rows))
    all_figures, owner = identify(image, rows=source_rows, columns=columns,
                              minimum_area=job.get('minimumArea',15000), alpha_threshold=128)
    groups = [row.get('sourceIndices') or [row['sourceRow'] * columns + phase for phase in range(columns)] for row in rows]
    indices = [index for group in groups for index in group]
    frame_specs = [(spec, phase, len(group)) for spec, group in zip(rows, groups) for phase in range(len(group))]
    figures = [all_figures[index] for index in indices]
    if any(figure['touchesExtractionEdge'] for figure in figures):
        raise ValueError('Complete opaque figure touches source edge: ' + name)
    # Ownership follows complete connected figures. Original low-alpha edge pixels
    # are assigned to the nearest complete silhouette, never erased or recolored.
    yy, xx = np.where((rgba[:, :, 3] > 0) & (owner == 0))
    best = np.full(xx.shape, np.inf)
    distance_only = np.full(xx.shape, np.inf)
    assigned = np.zeros(xx.shape, dtype=np.uint8)
    for i, figure in enumerate(all_figures):
        x0, y0, x1, y1 = figure['grid']
        dx = np.maximum(np.maximum(x0 - xx, xx - x1), 0)
        dy = np.maximum(np.maximum(y0 - yy, yy - y1), 0)
        distance = dx * dx + dy * dy
        distance_only = np.minimum(distance_only, distance)
        score = distance + ((xx - (x0 + x1) / 2) ** 2 + (yy - (y0 + y1) / 2) ** 2) * .00001
        closer = score < best
        assigned[closer] = i + 1
        best[closer] = score[closer]
    outside = distance_only > 16 ** 2
    far_alpha = rgba[yy[outside], xx[outside], 3]
    if np.any(far_alpha >= 32):
        raise ValueError('Visible background beyond whole silhouettes; reject source: ' + name)
    owner[yy, xx] = assigned
    rectangles = []
    extents = []
    for source_index, figure in zip(indices, figures):
        fy, fx = np.where(owner == source_index + 1)
        x0, y0, x1, y1 = int(fx.min()), int(fy.min()), int(fx.max() + 1), int(fy.max() + 1)
        anchor = [round(figure['anchor'][0]), figure['anchor'][1]]
        rectangles.append((x0, y0, x1, y1, anchor))
        extents.append((anchor[0] - x0, anchor[1] - y0, x1 - anchor[0], y1 - anchor[1]))
    left, top, right, bottom = [max(extent[k] for extent in extents) + 8 for k in range(4)]
    cw, ch = left + right, top + bottom
    atlas = Image.new('RGBA', (cw * columns, ch * ((len(indices) + columns - 1) // columns)))
    if max(atlas.size) > 8192:
        raise ValueError('Native atlas exceeds importer limit')
    frames = []
    reconstructed = np.zeros_like(rgba)
    for i, (x0, y0, x1, y1, anchor) in enumerate(rectangles):
        selected = owner[y0:y1, x0:x1] == indices[i] + 1
        pixels = rgba[y0:y1, x0:x1].copy()
        pixels[~selected] = 0
        cx, cy = (i % columns) * cw, (i // columns) * ch
        dx, dy = left - (anchor[0] - x0), top - (anchor[1] - y0)
        atlas.paste(Image.fromarray(pixels), (cx + dx, cy + dy))
        # Exact source-to-atlas equality includes soft alpha and detached flecks.
        actual = np.array(atlas.crop((cx + dx, cy + dy, cx + dx + x1 - x0, cy + dy + y1 - y0)))
        if not np.array_equal(actual[selected], rgba[y0:y1, x0:x1][selected]):
            raise ValueError('Changed RGBA during complete-figure registration')
        reconstructed[y0:y1, x0:x1][selected] = actual[selected]
        spec, phase, phase_count = frame_specs[i]
        group_durations = frame_durations(job, spec, phase_count) if spec['action'] in TIMED_ACTIONS else None
        support = job.get('supportAnchors', {}).get(str(indices[i]))
        render_anchor = support['anchor'] if support else anchor
        pivot = [(dx + render_anchor[0] - x0) / cw,
                 (ch - (dy + render_anchor[1] - y0)) / ch]
        if not all(0 <= value <= 1 for value in pivot):
            raise ValueError('Foot support pivot outside complete figure frame: ' + name)
        frames.append(dict(id=f'Redesign_{name}_{i:02}', rect=[cx, cy, cw, ch],
                           pivot=pivot, durationMs=group_durations[phase] if spec['action'] in TIMED_ACTIONS else 120,
                           action=spec['action'].lower(), direction=DIRECTIONS[spec['direction']], skin=['Requiem', 'Rock'][skin],
                           form=['Normal', 'Ruptura', 'Eclipse', 'Absoluto'][job['form']],
                           phase=['neutral', 'inhale', 'blink', 'return'][phase] if spec['action']=='Idle' else spec['action'].lower()+'_'+str(phase),
                           sourceFrameIndex=indices[i],
                           sourceRect=[x0, y0, x1 - x0, y1 - y0], sourceAnchor=render_anchor,
                           sourcePackingAnchor=anchor, supportGuide=support,
                           sourceOffsetInFrame=[dx, dy], sourceScale=1))
    nonzero = (rgba[:, :, 3] > 0) & np.isin(owner, np.array(indices) + 1)
    if not np.array_equal(reconstructed[nonzero], rgba[nonzero]):
        raise ValueError('Missing, duplicated, moved or recolored source RGBA')
    source_pixels = int(np.count_nonzero(nonzero))
    if int(np.count_nonzero(np.array(atlas)[:, :, 3])) != source_pixels:
        raise ValueError('Source alpha pixel count changed')
    ppu = float(np.median([figure['bodyHeight'] for figure in figures[:columns]])) / 1.98
    target = RES / (name + '.png')
    target.parent.mkdir(parents=True, exist_ok=True)
    png_bytes = io.BytesIO()
    atlas.save(png_bytes, format='PNG')
    atomic_bytes(target, png_bytes.getvalue())
    clips = []
    start = 0
    for spec, group in zip(rows, groups):
        clips.append(dict(name=spec['action'].lower() + '_' + DIRECTIONS[spec['direction']],
                          frames=[frames[start + phase]['id'] for phase in range(len(group))],
                          loop=spec.get('loop', spec['action'] in LOOP_ACTIONS), allowStatic=False))
        start += len(group)
    manifest = dict(schemaVersion=1, image=target.name, imageSha256=sha(target),
                    nativeSize=list(atlas.size), sourceSize=list(image.size),
                    pixelsPerUnit=ppu, status='visual-reviewed', wholeFigures=True,
                    sourcePath=source.relative_to(ROOT).as_posix(), sourceSha256=sha(source),
                    frames=frames, clips=clips, requiredClips=[clip['name'] for clip in clips],
                    quality=dict(alphaThreshold=0, maxColors=256, maxHeightRatio=1.12),
                    sourcePixels=source_pixels, packedPixels=source_pixels,
                    originalSourcePixels=int(np.count_nonzero(rgba[:, :, 3])),
                    selectedSourceFrameIndices=indices,
                    excludedSourceFrameIndices=[i for i in range(len(all_figures)) if i not in indices],
                    excludedSourceReason=job.get('excludedReason', ''),
                    sourceRgbaExactReconstruction=True, anatomyEdits=False, sourceScale=1,
                    logicalResolution='Native generated raster; indexed pixel grid unverified',
                    alphaReview=dict(transparentPixels=int((rgba[:, :, 3] == 0).sum()),
                                     farBeyond16pxPixels=int(outside.sum()),
                                     farBeyond16pxMaxAlpha=int(far_alpha.max(initial=0)),
                                     farBeyond16pxOpaqueEquivalent=float(far_alpha.sum() / 255),
                                     backgroundRemoved=False, everySelectedNonzeroAlphaPixelRetained=True),
                    notes=['Complete figures only; no anatomical component assembly or resizing.',
                           'Partial coverage: only declared directions/actions for this form/weapon.',
                           'Third authored idle pose is a blink, not a breathing in-between.' if any(row['action']=='Idle' for row in rows) else 'Action phases are complete authored figures; legacy contact timing is preserved by exact source-name mappings.',
                           'Source RGB beneath zero alpha is preserved in the original source file.',
                           'Point/uncompressed import required. Unity import/runtime still pending.'])
    write(RES / (name + '.json'), manifest)
    pages = []
    start = 0
    for spec, group in zip(rows, groups):
        direction, action = spec['direction'], spec['action']
        phases = spec.get('phaseIndices', list(range(len(group))))
        pages.append(dict(id=name + '_' + action + '_' + DIRECTIONS[direction], skin=skin, weapon=job.get('weapon', 1),
                          form=job['form'], direction=direction, action=action, attackKey=spec.get('attackKey', ''),
                          resourcePath='Darklight/WeaponRedesign20261023/' + name,
                          imageSha256=manifest['imageSha256'], sourcePath=manifest['sourcePath'],
                          sourceSha256=manifest['sourceSha256'], sourceSize=manifest['sourceSize'],
                          nativeSize=manifest['nativeSize'], pixelsPerUnit=ppu,
                          status=manifest['status'], wholeFigures=True, loop=spec.get('loop', action in LOOP_ACTIONS),
                          spriteNames=[frames[start + phase]['id'] for phase in phases],
                          sourceSpriteNames=spec.get('sourceSpriteNames', []),
                          durationMs=page_durations(job, spec, len(group), phases)))
        start += len(group)
    return manifest, pages


def qa(name):
    folder = OUT / name
    folder.mkdir(parents=True, exist_ok=True)
    for mode in ['audit', 'palette', 'preview']:
        source = RES / (name + ('.png' if mode == 'palette' else '.json'))
        output = folder / ('palette.json' if mode == 'palette' else mode)
        existing = output / 'preview.html' if mode == 'preview' else output / 'report.json' if mode == 'audit' else output
        fingerprint = sha(source)
        marker = folder / (mode + '-input.sha256')
        if existing.exists() and marker.exists() and marker.read_text(encoding='utf-8').strip() == fingerprint:
            continue
        if output.exists():
            # The skill's audit/preview commands require a new or empty output.
            # Preserve previous QA instead of reusing stale evidence or deleting it.
            archive_root = folder / 'qa-history'
            archive_root.mkdir(exist_ok=True)
            old = marker.read_text(encoding='utf-8').strip()[:12] if marker.exists() else 'legacy'
            archived = archive_root / (mode + '-' + old + '-' + str(time.time_ns()))
            if not output.resolve().is_relative_to(ROOT.resolve()) or not archived.resolve().is_relative_to(ROOT.resolve()):
                raise ValueError('QA archive escaped workspace')
            output.rename(archived)
        result = subprocess.run([sys.executable, str(PIPE), mode,
                                 '--image' if mode == 'palette' else '--manifest', str(source),
                                 '--out', str(output)], capture_output=True, text=True,
                                encoding='utf-8', errors='replace')
        (folder / (mode + '.log')).write_text(result.stdout + result.stderr, encoding='utf-8')
        if result.returncode:
            raise RuntimeError(f'{name} {mode} failed: {result.returncode}; see {folder}')
        marker.write_text(fingerprint + '\n', encoding='utf-8')
    report = json.loads((folder / 'audit/report.json').read_text(encoding='utf-8'))
    return dict(name=name, errors=len(report['errors']), warnings=len(report['warnings']))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--jobs', type=Path, default=ART / 'import-jobs.json')
    args = parser.parse_args()
    jobs = json.loads(args.jobs.read_text(encoding='utf-8'))['jobs']
    catalog_path = RES / 'catalog.json'
    catalog = json.loads(catalog_path.read_text(encoding='utf-8')) if catalog_path.exists() else dict(
        schemaVersion=1, complete=False, fallbackPolicy='approved-existing-same-context', pages=[])
    summaries = []
    for job in jobs:
        manifest, pages = prepare(job)
        contexts = {(page['skin'],page['weapon'],page['form'],page['direction'],page['action'],page['attackKey']) for page in pages}
        catalog['pages'] = [page for page in catalog['pages'] if
                            (page['skin'],page['weapon'],page['form'],page['direction'],page['action'],page.get('attackKey','')) not in contexts] + pages
        summaries.append(dict(name=job['name'], frames=len(manifest['frames']),
                              nativeSize=manifest['nativeSize'], pixelsPerUnit=manifest['pixelsPerUnit'],
                              exactRgba=True, alphaReview=manifest['alphaReview']))
    catalog['complete'] = False
    write(catalog_path, catalog)
    for job in jobs:
        summaries[[summary['name'] for summary in summaries].index(job['name'])]['qa'] = qa(job['name'])
    write(OUT / 'redesign-asset-qa.json', dict(sheets=summaries, unityImport=False,
                                             runtimeVerified=False, audio=False))
    print(json.dumps(dict(sheets=len(jobs), clips=sum(len(job.get('rows') or job.get('directions',[])) for job in jobs),
                         frames=sum(summary['frames'] for summary in summaries),
                         errors=sum(summary['qa']['errors'] for summary in summaries),
                         warnings=sum(summary['qa']['warnings'] for summary in summaries))))


if __name__ == '__main__':
    main()
