"""Technical playback of WHOLE generated frames; never redraws or assembles limbs.

Sources are kept byte-for-byte. Crops identify complete cells only. One uniform
scale applies to a whole sheet; no limb or per-frame stretching is performed.
"""
import base64
import hashlib
import json
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs' / 'prototipos-armas-20261017'
SHEETS = [
    ('adagas-requiem', 'Adagas', 'Requiem', 4, 16),
    ('adagas-rock', 'Adagas', 'Rock', 4, 16),
    ('katana-requiem', 'Katana', 'Requiem', 4, 16),
    ('katana-rock', 'Katana', 'Rock', 4, 16),
    ('corrente-requiem', 'Corrente e foice', 'Requiem', 4, 16),
    ('corrente-rock', 'Corrente e foice', 'Rock', 4, 16),
    ('cavaleiro-rock', 'Cavaleiro do Abismo', 'Rock', 6, 24),
    ('suporte-requiem', 'Defesa e reações', 'Requiem', 8, 32),
    ('suporte-rock', 'Defesa e reações', 'Rock', 8, 32),
]


def cuts(projection, count):
    """Choose real transparent gutters near nominal row/column boundaries."""
    length = len(projection)
    result = [0]
    for boundary in range(1, count):
        nominal = length * boundary / count
        radius = int(length / count * .16)
        candidates = range(max(result[-1] + 1, round(nominal) - radius),
                           min(length - 1, round(nominal) + radius) + 1)
        # Low-occupancy gaps take priority; distance breaks ties deterministically.
        best = min(candidates, key=lambda i: (sum(projection[max(0, i - 2):i + 3]), abs(i - nominal)))
        result.append(best)
    return result + [length]


def projection(im):
    mask = im.getchannel('A').point(lambda a: 255 if a >= 180 else 0)
    w, h = im.size
    horizontal = [mask.crop((0, y, w, y + 1)).histogram()[255] for y in range(h)]
    vertical = [mask.crop((x, 0, x + 1, h)).histogram()[255] for x in range(w)]
    return horizontal, vertical


def complete_frames(im, rows, count):
    w, h = im.size
    horizontal, _ = projection(im)
    yy = cuts(horizontal, rows)
    frames = []
    for row in range(rows):
        band = im.crop((0, yy[row], w, yy[row + 1]))
        _, vertical = projection(band)
        xx = cuts(vertical, 4)
        for col in range(4):
            rect = (xx[col], yy[row], xx[col + 1], yy[row + 1])
            cell = im.crop(rect)
            alpha = cell.getchannel('A').point(lambda a: 255 if a >= 180 else 0)
            box = alpha.getbbox()
            if box is None:
                raise ValueError(f'Empty full frame {row * 4 + col}')
            # The neutral frame establishes sheet scale; source pixels are never
            # resized independently by character pose or by a body-part boundary.
            frames.append({'index': len(frames), 'rect': list(rect),
                           'opaqueBoundsInCell': list(box),
                           'anchor': [(col + .5) * w / 4, rect[1] + box[3]],
                           'touchesCellEdge': box[0] == 0 or box[1] == 0 or
                           box[2] == cell.width or box[3] == cell.height})
    assert len(frames) == count
    return frames


def clips_for(key):
    if key.startswith('suporte-'):
        return [{'name': name, 'frames': list(range(offset, offset + 8)),
                 'loop': offset != 24, 'frameMs': 95}
                for name, offset in [('Defesa / parry', 0), ('Esquiva', 8),
                                     ('Dano / recuperação', 16), ('Derrota', 24)]]
    return [{'name': 'Combo e recuperação' if not key.startswith('cavaleiro') else
             'Serras, montagem, pilotagem e desmontagem',
             'frames': list(range(24 if key.startswith('cavaleiro') else 16)),
             'loop': True, 'frameMs': 100 if key.startswith('cavaleiro') else 85}]


def render_whole(im, entry, frame):
    # Crop includes the complete character and equipment together.
    rect = frame['rect']
    whole = im.crop(rect)
    scale = entry['previewScale']
    whole = whole.resize((round(whole.width * scale), round(whole.height * scale)),
                         Image.Resampling.NEAREST)
    canvas = Image.new('RGBA', (420, 420), (23, 23, 31, 255))
    x = round(210 + (rect[0] - frame['anchor'][0]) * scale)
    y = round(370 + (rect[1] - frame['anchor'][1]) * scale)
    canvas.alpha_composite(whole, (x, y))
    return canvas.convert('RGB')


def main():
    entries = []
    for key, weapon, skin, rows, count in SHEETS:
        path = OUT / (key + '.png')
        im = Image.open(path).convert('RGBA')
        frames = complete_frames(im, rows, count)
        neutral = frames[0]['opaqueBoundsInCell']
        neutral_height = neutral[3] - neutral[1]
        entry = {'id': key, 'weapon': weapon, 'skin': skin, 'file': path.name,
                 'width': im.width, 'height': im.height, 'frameCount': count,
                 'columns': 4, 'rows': rows, 'origin': 'top-left',
                 'previewScale': min(1.6, 285 / neutral_height),
                 'frames': frames, 'clips': clips_for(key),
                 'alphaRange': list(im.getchannel('A').getextrema()),
                 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                 'productionReady': False}
        for i, clip in enumerate(entry['clips']):
            filename = key + (f'-{i + 1}' if len(entry['clips']) > 1 else '') + '.gif'
            playback = [render_whole(im, entry, frames[j]) for j in clip['frames']]
            durations = [clip['frameMs']] * len(playback)
            durations[0] += 120
            durations[-1] += 280 if clip['loop'] else 1200
            options = {'loop': 0} if clip['loop'] else {}
            playback[0].save(OUT / filename, save_all=True, append_images=playback[1:],
                             duration=durations, disposal=2, optimize=False, **options)
            with Image.open(OUT / filename) as checked:
                if checked.n_frames != len(clip['frames']):
                    raise ValueError(f'GIF frame count mismatch: {filename}')
            clip['gif'] = filename
        # Embedded image makes the local review self contained, with no requests.
        entry['imageData'] = 'data:image/png;base64,' + base64.b64encode(path.read_bytes()).decode()
        entries.append(entry)
    manifest = {'date': '2026-10-17', 'kind': 'visual-approval-prototypes',
                'integrationVerified': False, 'directionCoverage': 'one-view prototype',
                'normalFormOnly': True, 'sheets': [{k: v for k, v in e.items() if k != 'imageData'} for e in entries],
                'notes': ['Whole-frame playback only; source artwork is unchanged.',
                          'One fixed scale per sheet, never independent body-part scaling.',
                          'Preview pivots are provisional and have not been validated in Unity.',
                          'Cell-edge contacts require review before production import.',
                          'No claims of gameplay balance, performance, eight-direction or transformed-form completion.']}
    (OUT / 'manifesto-preview.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
    original = 'data:image/png;base64,' + base64.b64encode((OUT / 'design-original.png').read_bytes()).decode()
    template = (Path(__file__).parent / 'review-template.html').read_text(encoding='utf-8')
    html = template.replace('__SHEETS__', json.dumps(entries, ensure_ascii=False)).replace('__ORIGINAL__', original)
    (OUT / 'previa-animada.html').write_text(html, encoding='utf-8')
    print(json.dumps({'sheets': len(entries), 'wholeFrames': sum(e['frameCount'] for e in entries),
                      'clips': sum(len(e['clips']) for e in entries),
                      'gifFrameCountsVerified': True,
                      'edgeContactsForReview': sum(f['touchesCellEdge'] for e in entries for f in e['frames']),
                      'html': str(OUT / 'previa-animada.html')}, ensure_ascii=False))


if __name__ == '__main__':
    main()
