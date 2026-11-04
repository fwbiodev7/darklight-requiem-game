"""Read-only source audit and technical whole-sheet inspection previews."""
from pathlib import Path
from PIL import Image
import hashlib
import json

ROOT = Path(__file__).resolve().parent
RECTS = {
    1: ([0, 354, 746, 1177, 1536], [0, 365, 679, 1024]),
    2: ([0, 359, 830, 1211, 1536], [0, 359, 683, 1024]),
}
data = []
ANCHORS = {
    1: [[(194,353),(588,354),(950,345),(1356,351)],[(190,670),(590,663),(950,664),(1354,671)],[(180,981),(602,963),(975,967),(1350,971)]],
    2: [[(178,344),(620,351),(1004,352),(1385,351)],[(175,674),(627,662),(1018,670),(1380,667)],[(185,983),(646,974),(1014,976),(1396,977)]],
}
for version, (xs, ys) in RECTS.items():
    source = ROOT / f'ReqKatanaNormalLight1_S_SW_W-v{version}-source.png'
    im = Image.open(source).convert('RGBA')
    alpha = im.getchannel('A')
    hist = alpha.histogram()
    frames = []
    for row, direction in enumerate(['S', 'SW', 'W']):
        for col, (phase, duration) in enumerate(zip(['anticipation', 'contact', 'follow-through', 'recovery'], [55, 75, 70, 70])):
            rect = [xs[col], ys[row], xs[col+1]-xs[col], ys[row+1]-ys[row]]
            a = alpha.crop((xs[col], ys[row], xs[col+1], ys[row+1]))
            strong = a.point(lambda p: 255 if p >= 200 else 0)
            bbox = strong.getbbox()
            frames.append({'row': row, 'column': col, 'direction': direction, 'directionId': row, 'phase': phase, 'durationMs': duration, 'sourceRect': rect, 'strongAlphaBoundsLocal': bbox})
    for bg, color in [('light', (238, 238, 238, 255)), ('dark', (35, 35, 35, 255))]:
        canvas = Image.new('RGBA', im.size, color)
        canvas.alpha_composite(im)
        canvas.convert('RGB').save(ROOT / f'ReqKatanaNormalLight1_S_SW_W-v{version}-inspection-{bg}.png')
    sha = hashlib.sha256(source.read_bytes()).hexdigest()
    data.append({'version': version, 'source': source.name, 'sha256': sha, 'sourceSize': list(im.size), 'alphaExtrema': alpha.getextrema(), 'transparentPixels': hist[0], 'partialAlphaPixels': sum(hist[1:255]), 'opaquePixels': hist[255], 'frames': frames})
    manifest_frames = []
    for f in frames:
        x,y,w,h = f['sourceRect']
        ax,ay = ANCHORS[version][f['row']][f['column']]
        manifest_frames.append({'id': f'v{version}_{f["direction"]}_{f["column"]:02}', 'rect': f['sourceRect'], 'pivot': [(ax-x)/w, 1-(ay-y)/h], 'durationMs': f['durationMs'], 'action': 'attackLight1', 'direction': f['direction'], 'skin': 'Requiem', 'form': 'Normal', 'phase': f['phase']})
    manifest = {'schemaVersion': 1, 'image': source.name, 'imageSha256': sha, 'nativeSize': list(im.size), 'sourceSize': list(im.size), 'logicalNativeSize': None, 'pixelsPerUnit': 174.24242424242425, 'pixelsPerUnitStatus': 'reference-consumer-value-not-imported', 'pivotStatus': 'proposed-for-inspection-only', 'status': 'candidate', 'wholeFigures': True, 'visualApproval': False, 'frames': manifest_frames, 'clips': [{'name': f'attackLight1_{d}', 'frames': [f['id'] for f in manifest_frames if f['direction']==d], 'loop': False, 'allowStatic': False} for d in ['S','SW','W']], 'requiredClips': ['attackLight1_S','attackLight1_SW','attackLight1_W'], 'quality': {'alphaThreshold': 200, 'maxHeightRatio': 1.5}}
    (ROOT / f'ReqKatanaNormalLight1_S_SW_W-v{version}-manifest.json').write_text(json.dumps(manifest, indent=2), encoding='utf-8')
(ROOT / 'ReqKatanaNormalLight1-source-audit.json').write_text(json.dumps(data, indent=2), encoding='utf-8')
print(json.dumps([{'version': d['version'], 'frames': [{'direction': f['direction'], 'phase': f['phase'], 'bounds': f['strongAlphaBoundsLocal'], 'rect': f['sourceRect']} for f in d['frames']]} for d in data]))
