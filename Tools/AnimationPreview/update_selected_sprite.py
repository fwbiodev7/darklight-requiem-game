"""Update one complete sprite against an existing baseline; preserve every other frame."""
import argparse
import json
from pathlib import Path
from types import SimpleNamespace
import sys

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
import sprite_pipeline as sp
import replace_frames as rf


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--baseline', type=Path, required=True)
    p.add_argument('--replacement', type=Path, required=True)
    p.add_argument('--frame', required=True)
    p.add_argument('--logical-size', type=int, required=True)
    p.add_argument('--ground', type=int, required=True)
    p.add_argument('--out', type=Path, required=True)
    a = p.parse_args()
    d, _, _ = sp.checked(a.baseline)
    f = next(f for f in d['frames'] if f['id'] == a.frame)
    w, h = f['rect'][2:]
    source = Image.open(a.replacement).convert('RGBA')
    if source.width != source.height or a.logical_size > min(w, h):
        raise ValueError('Use a square full-frame source that fits without stretching')
    tile = source.resize((a.logical_size, a.logical_size), Image.Resampling.NEAREST)
    box = tile.getchannel('A').point(lambda value: 255 if value > 128 else 0).getbbox()
    if not box:
        raise ValueError('Replacement is empty')
    x, y = (w - a.logical_size) // 2, a.ground - box[3]
    if y < 0 or x + box[0] <= 0 or x + box[2] >= w or y + box[3] >= h:
        raise ValueError('Whole sprite would clip; do not crop')
    canvas = Image.new('RGBA', (w, h))
    canvas.paste(tile, (x, y))
    sp.directory(a.out)
    canvas.save(a.out / 'replacement.png')
    sp.write(a.out / 'plan.json', dict(wholeFramesConfirmed=True,
                                      replacements=[dict(frameId=a.frame, image='replacement.png')]))
    result = rf.replace(a.baseline, a.out / 'plan.json', a.out / 'atlas')
    report = sp.audit(SimpleNamespace(manifest=a.out / 'atlas/sheet.json', out=a.out / 'audit', unity_meta=None))
    preview = sp.preview(SimpleNamespace(manifest=a.out / 'atlas/sheet.json', out=a.out / 'preview-final', zoom=2))
    sp.write(a.out / 'normalization.json', dict(sourceSha256=sp.digest(a.replacement),
             sourceSize=list(source.size), logicalSize=[a.logical_size, a.logical_size],
             uniformScale=a.logical_size/source.width, resampling='nearest-neighbor',
             wholeFrameOnly=True, placement=[x, y], ground=a.ground,
             productionImported=False, visualApproval=False))
    print(json.dumps(dict(result, auditOk=report['ok'], preview=preview['preview'])))
    return 0 if report['ok'] else 2


if __name__ == '__main__':
    raise SystemExit(main())
