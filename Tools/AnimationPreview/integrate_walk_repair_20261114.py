"""Pack reviewed WHOLE generated figures, retaining their native RGBA pixels.

Input is an explicit reviewed row map; this does not generate poses, mirror,
segment anatomy, remove backgrounds, or touch Fabius Normal.
"""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw
from prepare_katana_walk_candidate_20261024 import whole_owners

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'ArtSources/WalkRepair20261114'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/FabiusForms20261111'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def extract_overlapping_whole_figures(path, rows=2, columns=3):
    """Reuse whole-component ownership for interlocking approved silhouettes.

    This separates entire figures, including their original soft alpha, without
    assembling anatomy, recoloring, warping, or throwing away source pixels.
    """
    rgba, figures, owner = whole_owners(Image.open(path).convert('RGBA'), rows=rows, columns=columns)
    if any(f['touchesExtractionEdge'] for f in figures):
        raise ValueError('Approved source itself contains an edge-cut figure')
    cells, records = [], []
    for i, figure in enumerate(figures):
        yy, xx = np.where((owner == i + 1) & (rgba[:, :, 3] > 0))
        box = [int(xx.min()), int(yy.min()), int(xx.max()) + 1, int(yy.max()) + 1]
        x0, y0, x1, y1 = box
        mask = owner[y0:y1, x0:x1] == i + 1
        pixels = rgba[y0:y1, x0:x1].copy()
        original_hash = hashlib.sha256(pixels[mask].tobytes()).hexdigest()
        pixels[~mask] = 0
        if hashlib.sha256(pixels[mask].tobytes()).hexdigest() != original_hash:
            raise ValueError('Whole source figure pixels changed')
        cells.append(Image.fromarray(pixels))
        records.append({'index': i, 'sourceRectTopLeft': [x0, y0, x1-x0, y1-y0],
                        'rgbaSha256': original_hash, 'alphaPixels': len(xx), 'wholeFigure': True})
    cw, ch = max(c.width for c in cells) + 64, max(c.height for c in cells) + 64
    packed = Image.new('RGBA', (cw * columns, ch * rows))
    for i, cell in enumerate(cells):
        packed.paste(cell, ((i % columns) * cw + 32, (i // columns) * ch + 32))
    if sum(f['alphaPixels'] for f in records) != int((rgba[:, :, 3] > 0).sum()):
        raise ValueError('Source alpha pixels were dropped during whole-figure extraction')
    output = path.with_stem(path.stem + '-isolated')
    packed.save(output)
    output.with_suffix('.json').write_text(json.dumps({'source': path.name, 'sourceSha256': sha(path),
        'output': output.name, 'outputSha256': sha(output), 'sourcePixelsPreserved': True,
        'anatomicalAssembly': False, 'resized': False, 'frames': records}, indent=2) + '\n', encoding='utf-8')
    return output


def support(cell):
    a = np.asarray(cell)
    h, w = a.shape[:2]
    y, x = np.indices((h, w))
    # Inspect dark, opaque drawn body/boot outlines, not red aura or soft shadow.
    dark = ((a[:, :, 3] > 220) & (a[:, :, 0] < 145) &
            (a[:, :, 1] < 125) & (a[:, :, 2] < 135))
    torso = dark & (y > h * .37) & (y < h * .57) & (x > w * .22) & (x < w * .80)
    if torso.sum() < 100:
        raise ValueError('No unambiguous whole-body torso anchor')
    ax = float(np.median(x[torso]))
    boots = dark & (y > h * .65) & (x > w * .13) & (x < w * .90)
    if boots.sum() < 30:
        raise ValueError('No opaque intact boot support')
    ay = int(np.quantile(y[boots], .999)) + 1
    chroma = a[:, :, :3].max(axis=2).astype(int) - a[:, :, :3].min(axis=2).astype(int)
    head = dark & (chroma < 80) & (abs(x - ax) < w * .18) & (y < h * .40)
    if head.sum() < 50:
        raise ValueError('No intact head for scale inspection')
    top = int(np.quantile(y[head], .005))
    return ax, ay, ay - top


def main():
    config = json.loads((SOURCE / 'integration-map.json').read_text(encoding='utf-8'))
    catalog_file = DEST / 'catalog.json'
    catalog = json.loads(catalog_file.read_text(encoding='utf-8-sig'))
    pages, receipts = [], []
    review = SOURCE / 'review'
    review.mkdir(exist_ok=True)
    for spec in config['pages']:
        name = spec['name']
        if spec['form'] not in ('Ruptura', 'Serafim'):
            raise ValueError('Scope is transformed Fabius only')
        cells, refs = [], []
        for rowmap in spec['rows']:
            path = SOURCE / 'raw' / rowmap['source']
            im = Image.open(path).convert('RGBA')
            cols, rows = rowmap.get('grid', [3, 4])
            if cols != 3:
                raise ValueError('Exactly three authored poses required')
            r = rowmap['row']
            for c in range(3):
                rect = [round(c * im.width / cols), round(r * im.height / rows),
                        round((c + 1) * im.width / cols), round((r + 1) * im.height / rows)]
                cell = im.crop(rect)
                alpha = np.asarray(cell)[:, :, 3]
                # A visible contour at the extraction edge means the figure is cut.
                if any((edge > 128).any() for edge in (alpha[0], alpha[-1], alpha[:, 0], alpha[:, -1])):
                    raise ValueError(f'Visible figure crosses cell border: {path.name} row={r} col={c}')
                if np.count_nonzero(alpha > 128) < 1500:
                    raise ValueError('Empty or synthetic frame')
                cells.append((cell, rowmap, c, rect, support(cell)))
            refs.append({'path': path.relative_to(ROOT).as_posix(), 'sha256': sha(path)})
        cw = max(x[0].width for x in cells) + 64
        ch = max(x[0].height for x in cells) + 64
        atlas = Image.new('RGBA', (cw * 3, ch * len(spec['rows'])))
        frames, heights = [], []
        for index, (cell, rowmap, c, rect, anchor) in enumerate(cells):
            row = index // 3
            left, top = c * cw, row * ch
            atlas.paste(cell, (left + 32, top + 32))  # unmodified RGBA, not alpha-composited
            ax, ay, height = anchor
            ax += 32
            ay += 32
            heights.append(height)
            identifier = rowmap.get('startId', row * 3) + c
            frames.append({'id': f'{spec.get("spritePrefix", name)}_{identifier:02d}',
                           'rect': [left, atlas.height - top - ch, cw, ch],
                           'pivot': [round(ax / cw, 8), round((ch - ay) / ch, 8)],
                           'durationMs': spec.get('durations', [125, 100, 125])[c],
                           'action': spec.get('action', 'Walk'), 'form': spec['form'],
                           'direction': rowmap['direction'], 'frameIndex': c,
                           'sourcePath': refs[row]['path'],
                           'sourceRectTopLeft': [rect[0], rect[1], cell.width, cell.height],
                           'sourceSupportAnchor': [rect[0] + ax - 32, rect[1] + ay - 32],
                           'atlasSupportAnchor': [left + ax, top + ay]})
        ppu = round(float(np.median(heights)) / 1.95, 6)
        image_path = DEST / (name + '.png')
        atlas.save(image_path)
        page = {'name': name, 'image': image_path.name, 'imageSha256': sha(image_path),
                'sources': refs, 'nativeSize': list(atlas.size), 'pixelsPerUnit': ppu,
                'rows': len(spec['rows']), 'cols': 3, 'form': spec['form'],
                'action': spec.get('action', 'Walk'), 'weapon': spec['weapon'],
                'wholeFigures': True, 'status': 'visual-reviewed', 'frames': frames,
                'phaseSynchronized': spec.get('action') in ('FistAttack', 'MotoToSaws'),
                'pixelPolicy': 'Whole source cells packed 1:1; original RGBA unchanged; 32px extra padding.'}
        (DEST / (name + '.json')).write_text(json.dumps(page, indent=2) + '\n', encoding='utf-8')
        for row, rowmap in enumerate(spec['rows']):
            sequence = []
            for i in range(row * 3, row * 3 + 3):
                frame = frames[i]
                full = atlas.crop((i % 3 * cw, row * ch, (i % 3 + 1) * cw, (row + 1) * ch))
                pw, ph = max(800, cw + 160), max(700, ch + 160)
                canvas = Image.new('RGBA', (pw, ph), (40, 45, 50, 255))
                ax, ay = frame['atlasSupportAnchor']
                px, py = ax - i % 3 * cw, ay - row * ch
                canvas.alpha_composite(full, (round(pw / 2 - px), round(ph - 100 - py)))
                sequence.append(canvas.convert('RGB'))
            sequence[0].save(review / f'{name}-{rowmap["direction"]}.gif', save_all=True,
                             append_images=sequence[1:], duration=spec.get('durations', [125, 100, 125]), loop=0)
            contact = Image.new('RGB', (pw * 3, ph), (40, 45, 50))
            for i, frame_image in enumerate(sequence):
                contact.paste(frame_image, (i * pw, 0))
            contact.save(review / f'{name}-{rowmap["direction"]}-contact.png')
        pages.append(page)
        receipts.append({'name': name, 'frames': len(frames), 'ppu': ppu, 'sources': refs,
                         'imageSha256': sha(image_path), 'imported': False, 'runtimeVerified': False})
    names = {p['name'] for p in pages}
    catalog['pages'] = pages + [p for p in catalog['pages'] if p['name'] not in names]
    catalog_file.write_text(json.dumps(catalog, indent=2) + '\n', encoding='utf-8')
    (SOURCE / 'integration-receipt.json').write_text(json.dumps({'pages': receipts}, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'pages': len(pages), 'frames': sum(len(p['frames']) for p in pages), 'imported': False}))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--extract-overlap', type=Path)
    parser.add_argument('--rows', type=int, default=2)
    parser.add_argument('--columns', type=int, default=3)
    args = parser.parse_args()
    if args.extract_overlap:
        print(extract_overlapping_whole_figures(args.extract_overlap, args.rows, args.columns))
    else:
        main()
