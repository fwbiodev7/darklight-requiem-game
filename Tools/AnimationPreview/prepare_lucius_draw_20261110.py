"""Package reviewed whole-figure draw sheets; Assets writes require --integrate.

No catalog/runtime/Unity edits. Original RGBA pixels and native size are preserved.
"""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'ArtSources/LuciusDraw20261110'
OUT = ROOT / 'outputs/lucius-20261110/draw'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
DIRS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
PHASES = ['reach-hilt', 'grasp-hilt', 'extract-blade', 'low-guard']
DURATIONS = [180, 160, 200, 220]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def previews(record, image):
    folder = OUT / 'previews' / record['name']
    folder.mkdir(parents=True, exist_ok=True)
    for direction in record['directions']:
        frames = sorted([f for f in record['frames'] if f['direction'] == direction], key=lambda f: f['index'])
        canvases = []
        for f in frames:
            x, y, w, h = f['rect']
            sprite = image.crop((x, y, x+w, y+h))
            px, py = f['footPivotTopLeft']
            canvas = Image.new('RGBA', (512, 600), (30, 35, 45, 255))
            draw = ImageDraw.Draw(canvas)
            draw.line((0, 540, 512, 540), fill=(100, 110, 120))
            canvas.alpha_composite(sprite, (256-px, 540-py))
            draw.text((12, 12), direction + ' ' + PHASES[f['index']], fill='white')
            canvas.save(folder / (direction + '-' + str(f['index']) + '.png'))
            canvases.append(canvas.convert('RGB'))
        contact = Image.new('RGB', (2048, 600), (30, 35, 45))
        for i, canvas in enumerate(canvases):
            contact.paste(canvas, (512*i, 0))
        contact.save(folder / (direction + '-contact.png'))
        canvases[0].save(folder / (direction + '.gif'), save_all=True, append_images=canvases[1:],
                        duration=DURATIONS, disposal=2)  # Single authored draw, no infinite loop.
        final = canvases[-1]
        final.save(folder / (direction + '-IdleArmed.png'))


def main(integrate=False):
    review = json.loads((ART / 'review.json').read_text(encoding='utf-8'))
    for reference in review['references']:
        assert sha(ROOT / reference['sourcePath']) == reference['sha256']
    pages, audit = [], []
    for record in review['sheets']:
        source = ROOT / record['sourcePath']
        assert sha(source) == record['sourceSha256']
        image = Image.open(source)
        assert image.mode == 'RGBA' and list(image.size) == record['nativeSize']
        assert image.getchannel('A').getextrema()[0] == 0
        frames, hashes = [], []
        assert len(record['frames']) == 4 * len(record['directions'])
        for frame in record['frames']:
            x, y, w, h = frame['rect']
            px, py = frame['footPivotTopLeft']
            assert x >= 0 and y >= 0 and w > 0 and h > 0 and x+w <= image.width and y+h <= image.height
            assert 0 <= px <= w and 0 <= py <= h
            pixel_hash = hashlib.sha256(image.crop((x, y, x+w, y+h)).tobytes()).hexdigest()
            hashes.append(pixel_hash)
            frames.append(dict(id='Redesign_' + record['name'] + '_' + frame['direction'] + '_' + str(frame['index']).zfill(2),
                action='DrawSword', direction=frame['direction'], index=frame['index'], phase=PHASES[frame['index']],
                rect=frame['rect'], pivot=[px/w, (h-py)/h], footPivotTopLeft=[px, py],
                durationMs=DURATIONS[frame['index']], wholeFramePixelSha256=pixel_hash))
        assert len(set(hashes)) == len(hashes)
        previews(record, image)
        descriptor = dict(schemaVersion=1, image=record['name']+'.png', imageSha256=sha(source),
            sourcePath=record['sourcePath'], sourceSha256=sha(source), sourceSize=list(image.size),
            nativeSize=list(image.size), pixelsPerUnit=record['pixelsPerUnit'], wholeFigures=True,
            status=record['status'], allowStatic=True,
            pivotMethod='Reviewed sole anchors on complete figures; integer alignment, no resampling or pixel edits', frames=frames)
        save(OUT / 'descriptors' / (record['name']+'.json'), descriptor)
        for direction in record['directions']:
            sequence = sorted([f for f in frames if f['direction'] == direction], key=lambda f:f['index'])
            assert [f['index'] for f in sequence] == [0, 1, 2, 3]
            base = dict(direction=DIRS.index(direction), resourcePath='Darklight/LuciusNormal20261109/'+record['name'],
                imageSha256=sha(source), sourceSha256=sha(source), sourcePath=record['sourcePath'],
                nativeSize=list(image.size), pixelsPerUnit=record['pixelsPerUnit'], loop=False,
                wholeFigures=True, status=record['status'])
            pages.append(dict(base, id=record['name']+'_DrawSword_'+direction, action='DrawSword',
                              spriteNames=[f['id'] for f in sequence], durationMs=DURATIONS))
            pages.append(dict(base, id=record['name']+'_IdleArmed_'+direction, action='IdleArmed',
                              spriteNames=[sequence[-1]['id']], durationMs=[220], allowStatic=True))
        if integrate:
            assert record['status'] == 'visual-reviewed' and record['importReady']
            DEST.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, DEST / (record['name']+'.png'))
            save(DEST / (record['name']+'.json'), descriptor)
        audit.append(dict(name=record['name'], sourceSha256=sha(source), frames=len(frames),
            uniqueWholeFrames=len(set(hashes)), sourcePixelsUnchanged=True, status=record['status'],
            copiedToAssets=integrate, unityImported=False, runtimeVerified=False))
    manifest = dict(schemaVersion=1, characterId='Lucius', form='Normal', weapon='Sword', complete=False,
        pages=pages, pendingDirections=[d for d in DIRS if d not in {f['direction'] for r in review['sheets'] for f in r['frames']}],
        sharedCatalogEdited=False, unityImported=False, runtimeVerified=False,
        note='Page fragment only. DrawSword/IdleArmed consumer support and transitions require coordinator integration.')
    save(OUT / 'draw-pages.json', manifest)
    save(OUT / 'package-report.json', dict(sheets=audit, readyDirections=len(pages)//2, readyFrames=len(pages)*2,
        copiedToAssets=integrate, unityImported=False, runtimeVerified=False, sharedCatalogEdited=False))
    save(OUT / 'source-inventory.json', review)
    if integrate:
        save(DEST / 'LuciusDrawPages20261110.json', manifest)
    print(json.dumps(dict(readyDirections=len(pages)//2, readyFrames=len(pages)*2, copiedToAssets=integrate,
        unityImported=False, runtimeVerified=False)))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--integrate', action='store_true', help='Explicit coordinator-authorized copy of reviewed pages only')
    main(parser.parse_args().integrate)
