"""Package individually reviewed Lucius Walk sheets; never resample source art.

Review records are explicit, human/agent visual decisions, not automatic approvals.
Only pages marked visual-reviewed and importReady are copied into Resources.
This script does not modify the shared catalog or execute Unity.
"""
import hashlib
import json
import shutil
import argparse
import base64
import io
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'ArtSources/LuciusWalk20261110'
OUT = ROOT / 'outputs/lucius-20261110/walk'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
DIRS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
PHASES = ['contact-A', 'passing-A', 'contact-B', 'passing-B']
REFERENCE = ROOT / 'ArtSources/LuciusNormal20261109/selected/LuciusNormalIdle8Final-source.png'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    encoded = (json.dumps(value, ensure_ascii=False, indent=2) + '\n').encode()
    if not path.exists() or path.read_bytes() != encoded:
        path.write_bytes(encoded)


def preview(record, image):
    folder = OUT / 'previews' / record['name']
    folder.mkdir(parents=True, exist_ok=True)
    for direction in record['directions']:
        selected = [f for f in record['frames'] if f['direction'] == direction]
        selected.sort(key=lambda f: f['index'])
        assert len(selected) == 4
        canvases = []
        for frame in selected:
            x, y, w, h = frame['rect']
            sprite = image.crop((x, y, x + w, y + h))
            pivot_x, pivot_y = frame['footPivotTopLeft']
            canvas = Image.new('RGBA', (640, 640), (40, 45, 54, 255))
            draw = ImageDraw.Draw(canvas)
            draw.line((0, 550, 640, 550), fill=(120, 130, 140), width=1)
            canvas.alpha_composite(sprite, (320 - pivot_x, 550 - pivot_y))
            draw.text((12, 12), direction + ' ' + PHASES[frame['index']], fill='white')
            canvases.append(canvas.convert('RGB'))
        canvases[0].save(folder / (direction + '.gif'), save_all=True,
                         append_images=canvases[1:], duration=record['durationMs'], loop=0,
                         disposal=2)
        contact = Image.new('RGB', (640 * 4, 640), (40, 45, 54))
        for index, canvas in enumerate(canvases):
            contact.paste(canvas, (index * 640, 0))
        contact.save(folder / (direction + '-contact.png'))
        encoded = []
        for canvas in canvases:
            buffer = io.BytesIO()
            canvas.save(buffer, format='PNG')
            encoded.append('data:image/png;base64,' + base64.b64encode(buffer.getvalue()).decode())
        page = '''<!doctype html><meta charset="utf-8"><title>Lucius Walk preview</title>
<style>body{background:#171b22;color:#eee;font:16px system-ui}img{width:640px;height:640px;image-rendering:pixelated}button,input{margin:8px}</style>
<p>Whole figures, fixed support axis. Preview only; no Unity/runtime certification.</p>
<button id="play">Pause</button><button id="back">Previous</button><button id="next">Next</button>
<label>Speed <input id="speed" type="range" min="0.25" max="2" step="0.25" value="1"></label><span id="phase"></span><br><img id="pose">
<script>const frames=FRAMES,durations=DURATIONS,names=NAMES;let current=0,playing=true,last=performance.now();
function show(){document.getElementById('pose').src=frames[current];document.getElementById('phase').textContent=names[current];}
document.getElementById('play').onclick=()=>{playing=!playing;document.getElementById('play').textContent=playing?'Pause':'Play';last=performance.now();};
for(const [id,step] of [['back',-1],['next',1]])document.getElementById(id).onclick=()=>{playing=false;document.getElementById('play').textContent='Play';current=(current+step+frames.length)%frames.length;show();};
function tick(now){if(playing&&now-last>=durations[current]/Number(document.getElementById('speed').value)){current=(current+1)%frames.length;last=now;show();}requestAnimationFrame(tick);}show();requestAnimationFrame(tick);</script>'''
        page = page.replace('FRAMES', json.dumps(encoded)).replace('DURATIONS', json.dumps(record['durationMs'])).replace('NAMES', json.dumps(PHASES))
        (folder / (direction + '.html')).write_text(page, encoding='utf-8')


def main(preview_only=False, stage_only=False):
    review_path = ART / 'review.json'
    review = json.loads(review_path.read_text(encoding='utf-8'))
    assert review['referenceSha256'] == sha(REFERENCE), 'Identity reference changed'
    OUT.mkdir(parents=True, exist_ok=True)
    destination = OUT / 'stagedAssets' if stage_only else DEST
    pages, results = [], []
    for record in review['sheets']:
        source = ROOT / record['sourcePath']
        assert sha(source) == record['sourceSha256'], 'Source changed since review'
        image = Image.open(source)
        assert image.mode == 'RGBA' and image.getchannel('A').getextrema()[0] == 0
        assert list(image.size) == record['nativeSize']
        for origin in record.get('wholeFrameSources', []):
            original = ROOT / origin['sourcePath']
            assert sha(original) == origin['sourceSha256']
            x, y, w, h = origin['sourceRect']
            original_pixels = Image.open(original).crop((x, y, x+w, y+h)).tobytes()
            target_x, target_y, target_w, target_h = origin['selectedRect']
            selected_pixels = image.crop((target_x, target_y, target_x+target_w, target_y+target_h)).tobytes()
            assert (w, h) == (target_w, target_h) and original_pixels == selected_pixels, 'Whole-frame pixels changed'
        hashes = []
        for frame in record['frames']:
            x, y, w, h = frame['rect']
            assert min(x, y) >= 0 and w > 0 and h > 0
            assert x + w <= image.width and y + h <= image.height
            hashes.append(hashlib.sha256(image.crop((x, y, x+w, y+h)).tobytes()).hexdigest())
        assert len(set(hashes)) == len(hashes), 'Duplicate complete frame'
        preview(record, image)
        result = dict(name=record['name'], sourcePath=record['sourcePath'],
                      sourceSha256=sha(source), frameCount=len(hashes),
                      uniqueWholeFrameHashes=len(set(hashes)), status=record['status'],
                      sourcePixelsUnchanged=True, unityImported=False, runtimeVerified=False)
        results.append(result)
        if preview_only:
            continue
        if record['status'] != 'visual-reviewed' or not record.get('importReady'):
            continue
        assert record['wholeFigures'] and record['realAlternatingGait']
        assert record['nativeScaleConsistentWithReference']
        assert len(record['frames']) == 4 * len(record['directions'])
        assert 1 <= len(record['directions']) <= 2
        density = record['pixelsPerUnit']
        assert density > 0 and all(120 <= ms <= 150 for ms in record['durationMs'])
        destination.mkdir(parents=True, exist_ok=True)
        name = record['name']
        assert name.startswith('LuciusWalk')
        target = destination / (name + '.png')
        if not target.exists() or sha(target) != sha(source):
            shutil.copy2(source, target)
        frames = []
        for frame in record['frames']:
            x, y, w, h = frame['rect']
            px, py = frame['footPivotTopLeft']
            assert 0 <= px <= w and 0 <= py <= h
            frames.append(dict(id='Redesign_' + name + '_' + frame['direction'] + '_' + str(frame['index']).zfill(2),
                action='Walk', direction=frame['direction'], index=frame['index'],
                phase=PHASES[frame['index']], rect=frame['rect'],
                pivot=[px/w, (h-py)/h], durationMs=record['durationMs'][frame['index']]))
        save(destination / (name + '.json'), dict(schemaVersion=1, image=target.name,
            imageSha256=sha(target), sourcePath=record['sourcePath'], sourceSha256=sha(source),
            sourceSize=list(image.size), nativeSize=list(image.size), pixelsPerUnit=density,
            wholeFigures=True, status='visual-reviewed', allowStatic=False,
            pivotMethod='reviewed sole/support anchors on whole figures, integer coordinates; source pixels unchanged',
            frames=frames))
        for direction in record['directions']:
            selected = sorted([f for f in frames if f['direction'] == direction], key=lambda f:f['index'])
            assert [f['index'] for f in selected] == [0, 1, 2, 3]
            pages.append(dict(id=name + '_Walk_' + direction, action='Walk',
                direction=DIRS.index(direction), resourcePath='Darklight/LuciusNormal20261109/' + name,
                imageSha256=sha(target), sourceSha256=sha(source), sourcePath=record['sourcePath'],
                nativeSize=list(image.size), pixelsPerUnit=density,
                spriteNames=[f['id'] for f in selected], durationMs=record['durationMs'],
                loop=True, wholeFigures=True, status='visual-reviewed'))
        result['assetImage'] = target.relative_to(ROOT).as_posix()
    if preview_only:
        save(OUT / 'candidate-preview-report.json', dict(sheets=results,
             assetsEdited=False, unityImported=False, runtimeVerified=False))
        print(json.dumps(dict(previewSheets=len(results), assetsEdited=False)))
        return
    manifest = dict(schemaVersion=1, characterId='Lucius', form='Normal', weapon='Sword',
        complete=False, pages=pages, sharedCatalogEdited=False, unityImported=False,
        runtimeVerified=False, referenceSha256=sha(REFERENCE),
        note='Append reviewed pages to the existing catalog through the coordinator; this is a page fragment, not a complete catalog.')
    save(OUT / 'walk-pages.json', manifest)
    if pages:
        save(destination / 'LuciusWalkPages20261110.json', manifest)
    save(OUT / 'package-report.json', dict(sheets=results, readyDirections=len(pages),
        readyFrames=len(pages)*4, unityImported=False, runtimeVerified=False,
        sharedCatalogEdited=False, publicationVerified=False, stagedOnly=stage_only))
    print(json.dumps(dict(readyDirections=len(pages), readyFrames=len(pages)*4,
        reviewedSheets=sum(r['status']=='visual-reviewed' for r in results),
        unityImported=False, runtimeVerified=False)))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    group = parser.add_mutually_exclusive_group()
    group.add_argument('--preview-only', action='store_true', help='Write previews/report only; never touch Assets or page fragments')
    group.add_argument('--stage-only', action='store_true', help='Prepare ready files in outputs/stagedAssets; never touch Assets')
    arguments = parser.parse_args()
    main(arguments.preview_only, arguments.stage_only)
