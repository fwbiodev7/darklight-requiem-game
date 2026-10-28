"""Propose local eye palette regions on complete sprites; originals are read-only.

Proposals require visual review. Geometry/alpha/other materials are never rewritten.
"""
import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import sys

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]


def components(mask):
    parents, runs, previous = [], [], []
    def find(i):
        while parents[i] != i:
            parents[i] = parents[parents[i]]
            i = parents[i]
        return i
    for y, line in enumerate(mask):
        changes = np.diff(np.concatenate(([False], line, [False])).astype(np.int8))
        current = []
        for start, end in zip(np.where(changes == 1)[0], np.where(changes == -1)[0]):
            x0, x1 = int(start), int(end)
            i = len(parents)
            parents.append(i)
            current.append((x0, x1, i))
            runs.append((x0, y, x1, i))
            for p0, p1, pi in previous:
                if p0 > x1:
                    break
                if p1 >= x0:
                    parents[find(pi)] = find(i)
        previous = current
    result = {}
    labels = np.zeros(mask.shape, np.int32)
    for x0, y, x1, i in runs:
        key = find(i) + 1
        labels[y, x0:x1] = key
        b = result.setdefault(key, [x0, y, x1, y + 1, 0])
        b[0], b[1], b[2], b[3] = min(b[0], x0), min(b[1], y), max(b[2], x1), max(b[3], y + 1)
        b[4] += x1 - x0
    return result, labels


def analyze(rgba):
    h, w = rgba.shape[:2]
    c = rgba[:, :, :3].astype(np.float32)
    r, g, b = c[:, :, 0], c[:, :, 1], c[:, :, 2]
    opaque = rgba[:, :, 3] >= 128
    skin = opaque & (r > 150) & (g > 70) & (g < 220) & (b > 30) & (b < 185) & (r > g + 25) & (g > b + 10)
    # Search only the upper portion of the whole pose, not a global red-color filter.
    skin[int(h * .65):] = False
    groups, labels = components(skin)
    possible = [(key, box) for key, box in groups.items() if box[4] >= max(8, w*h*.0003)
                and box[3]-box[1] >= 3 and box[2]-box[0] >= 3]
    if not possible:
        return dict(status='no-visible-face-proposal', iris=[], cleanup=[], confidence=0)
    def score(item):
        _, (x0, y0, x1, y1, area) = item
        aspect = (x1-x0)/max(1, y1-y0)
        return area / (1 + y0/h*2) / (1 + max(0, aspect-1.5))
    key, box = max(possible, key=score)
    x0, y0, x1, y1, area = box
    fw, fh = x1-x0, y1-y0
    if fw < 5 or fh < 5:
        return dict(status='face-too-small-for-auto-proposal', face=box[:4], iris=[], cleanup=[], confidence=0)
    # White sclera/highlights anchor the eye line. Jewelry outside the face is excluded.
    yy, xx = np.indices((h, w))
    face_upper = (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y0 + fh*.64)
    white = opaque & face_upper & (r > 205) & (g > 195) & (b > 175) & (np.maximum.reduce([r,g,b])-np.minimum.reduce([r,g,b]) < 45)
    whites, _ = components(white)
    # Prefer small white eye features; broad skin/hair highlights are not sclera.
    candidates = [(k, v) for k, v in whites.items() if 1 <= v[4] <= max(12, fw*fw*.06)
                  and v[2]-v[0] <= fw*.28 and v[3]-v[1] <= max(3, fh*.22)]
    red = opaque & (r > 65) & (r > g*1.5) & (r > b*1.25) & (g < 110) & (b < 130)
    violet_blue = opaque & (b > 65) & (b > g*1.05) & (b > r*1.05) & (np.minimum.reduce([r,g,b]) < 225)
    neutral_iris = opaque & (np.maximum.reduce([r,g,b]) > 65) & (np.maximum.reduce([r,g,b]) < 210) & (np.maximum.reduce([r,g,b])-np.minimum.reduce([r,g,b]) < 28)
    eye_color = red | violet_blue | neutral_iris
    eyes = []
    for _, wb in candidates:
        wx, wy = (wb[0]+wb[2]-1)/2, (wb[1]+wb[3]-1)/2
        radius = max(2, int(round(fw*.18)))
        nearby = eye_color & (abs(xx-wx) <= radius) & (abs(yy-wy) <= max(2, fw*.12)) & face_upper
        ry, rx = np.where(nearby)
        if len(rx) < 2:
            continue
        eyes.append(dict(x=float(np.median(rx)), y=float(wy), white=wb[:4]))
    # Merge the multiple white features belonging to one eye, retain both eyes when visible.
    merged = []
    for eye in sorted(eyes, key=lambda a:a['x']):
        near = next((e for e in merged if abs(e['x']-eye['x']) < fw*.23 and abs(e['y']-eye['y']) < fw*.12), None)
        if near is None:
            merged.append(eye)
    if len(merged) > 2:
        merged = sorted(merged, key=lambda e:e['y'])[:2]
    if merged:
        anchor_y = float(np.median([e['y'] for e in merged]))
        eye_band = eye_color & face_upper & (abs(yy-anchor_y) <= max(2,fw*.12))
        red_groups, _ = components(eye_band)
        for rb in red_groups.values():
            if rb[4] < 2:
                continue
            ex, ey = (rb[0]+rb[2]-1)/2, (rb[1]+rb[3]-1)/2
            if any(abs(e['x']-ex) < fw*.23 for e in merged):
                continue
            if len(merged) < 2 and any(fw*.25 < abs(e['x']-ex) < fw*.80 for e in merged):
                merged.append(dict(x=float(ex),y=anchor_y,white=None,pairedProposal=True))
    iris, cleanup = [], []
    for eye in merged:
        ex, ey = eye['x'], eye['y']
        half_w = max(2, fw*.12)
        top, bottom = ey-max(1,fw*.07), ey+max(1,fw*.10)
        region = eye_color & face_upper & (abs(xx-ex) <= half_w) & (yy >= top) & (yy <= bottom)
        iy, ix = np.where(region)
        iris.extend([[int(x),int(y)] for x,y in zip(ix,iy)])
        # Only anomalously red existing pixels just under this eye are candidates.
        face_full = (xx >= x0) & (xx < x1) & (yy >= y0) & (yy < y1)
        under = red & face_full & (abs(xx-ex) <= half_w*1.12) & (yy > bottom) & (yy <= bottom+max(1,fw*.10))
        sy, sx = np.where(under)
        for px, py in zip(sx, sy):
            local = skin & (abs(xx-px) <= max(2,fw*.14)) & (yy > py) & (yy < py+max(3,fw*.20))
            samples = rgba[local, :3]
            if len(samples) >= 3:
                tone = np.median(samples, axis=0).astype(int).tolist()
                cleanup.append([int(px),int(py),*tone])
    confidence = .8 if len(merged)==2 else .55 if len(merged)==1 else 0
    return dict(status='proposal-needs-visual-review' if iris else 'no-iris-proposal', face=box[:4],
                eyes=merged, iris=iris, cleanup=cleanup, confidence=confidence)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--manifests', type=Path, required=True)
    p.add_argument('--out', type=Path, required=True)
    p.add_argument('--limit', type=int, default=0)
    p.add_argument('--no-contact', action='store_true')
    a = p.parse_args()
    if a.out.exists() and any(a.out.iterdir()):
        p.error('Use a new output folder')
    a.out.mkdir(parents=True, exist_ok=True)
    manifests = sorted(a.manifests.glob('*.json'))
    if a.limit:
        manifests = manifests[:a.limit]
    summary, counts = [], Counter()
    for manifest in manifests:
        d = json.loads(manifest.read_text(encoding='utf-8-sig'))
        source = (manifest.parent / d['image']).resolve()
        raw = Image.open(source).convert('RGBA')
        pixels = np.array(raw)
        rows, tiles = [], []
        for f in d['frames']:
            x, y, w, h = f['rect']
            if f.get('expectedBack') or f.get('expectedClosed'):
                region=dict(status='expected-back-or-closed',iris=[],cleanup=[],confidence=1)
            else:
                region = analyze(pixels[y:y+h, x:x+w])
            rows.append(dict(id=f['id'], rect=f['rect'], direction=f.get('direction'),
                             expectedClosed=f.get('expectedClosed',False),expectedBack=f.get('expectedBack',False),**region))
            counts[region['status']] += 1
            if a.no_contact:
                continue
            # Diagnostic contact shows whole poses, never cropped eye/head inserts.
            tile = raw.crop((x,y,x+w,y+h))
            draw = ImageDraw.Draw(tile)
            if 'face' in region:
                draw.rectangle(tuple(region['face']), outline='#00d5ff', width=1)
            for px, py in region['iris']:
                draw.point((px,py), fill='#00ff00')
            for px, py, *_ in region['cleanup']:
                draw.point((px,py), fill='#ffff00')
            if max(tile.size) > 360:
                scale = 360/max(tile.size)
                tile = tile.resize((round(tile.width*scale),round(tile.height*scale)), Image.Resampling.NEAREST)
            tiles.append((f['id'],tile))
        if tiles:
            cw, ch = max(t.width for _,t in tiles), max(t.height for _,t in tiles)+20
            contact = Image.new('RGBA', (cw*4, ch*((len(tiles)+3)//4)), '#30343a')
            draw = ImageDraw.Draw(contact)
            for i,(fid,tile) in enumerate(tiles):
                dx,dy=(i%4)*cw,(i//4)*ch
                contact.alpha_composite(tile,(dx,dy))
                draw.text((dx+2,dy+ch-17),fid,fill='white')
            contact.convert('RGB').save(a.out/(manifest.stem+'-annotations.png'))
        report=dict(source=str(source),sourceSha256=sha(source),frames=rows,
                    visualReviewed=False,productionApplied=False,
                    legend='cyan=face proposal; green=iris; yellow=under-eye cleanup candidate')
        (a.out/(manifest.stem+'.json')).write_text(json.dumps(report,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
        summary.append(dict(sheet=manifest.stem,frames=len(rows),irisPixels=sum(len(r['iris']) for r in rows),
                            cleanupPixels=sum(len(r['cleanup']) for r in rows)))
    report=dict(sheets=summary,states=dict(counts),visualReviewed=False,productionApplied=False)
    (a.out/'summary.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(dict(sheets=len(summary),states=dict(counts),productionApplied=False)))


if __name__ == '__main__':
    main()
