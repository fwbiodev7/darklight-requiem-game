"""Replace selected WHOLE frames; prove every pixel outside their rects is unchanged."""
import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageChops, ImageDraw
import sprite_pipeline as sp


def overlap(a, b):
    x, y, w, h = a
    bx, by, bw, bh = b
    return x < bx + bw and bx < x + w and y < by + bh and by < y + h


def replace(manifest, plan_path, out):
    d, source, _ = sp.checked(manifest)
    plan = sp.read(plan_path)
    entries = plan['replacements']
    if not isinstance(entries, list) or not entries:
        raise ValueError('Select at least one complete frame')
    lookup = {f['id']: f for f in d['frames']}
    selected = [r['frameId'] for r in entries]
    if len(set(selected)) != len(selected) or any(fid not in lookup for fid in selected):
        raise ValueError('Replacement IDs must be unique and exist in the source')
    for fid in selected:
        for other in d['frames']:
            if other['id'] != fid and overlap(lookup[fid]['rect'], other['rect']):
                raise ValueError(f'{fid} overlaps {other["id"]}; use existing ownership integration, not rectangular replacement')
    if plan.get('wholeFramesConfirmed') is not True:
        raise ValueError('Review whole-frame boundaries and set wholeFramesConfirmed=true; never replace anatomy parts')
    prepared, evidence = [], []
    for entry in entries:
        f = lookup[entry['frameId']]
        x, y, w, h = f['rect']
        path = plan_path.parent / entry['image']
        with Image.open(path) as raw:
            replacement = raw.convert('RGBA')
        rw, rh = replacement.size
        if rw % w or rh % h or rw // w != rh // h or rw < w:
            raise ValueError(f'{f["id"]}: require matching canvas or uniform integer multiple; no stretching')
        if (rw, rh) != (w, h):
            replacement = replacement.resize((w, h), Image.Resampling.NEAREST)
        box = replacement.getchannel('A').point(lambda a: 255 if a > 128 else 0).getbbox()
        if not box or min(box[0], box[1]) == 0 or box[2] == w or box[3] == h:
            raise ValueError(f'{f["id"]}: empty or touches canvas edge; inspect complete body and both weapons')
        prepared.append((f, replacement))
        evidence.append(dict(frameId=f['id'], replacementSha256=sp.digest(path),
                             sourceCanvas=[rw, rh], nativeCanvas=[w, h], rect=f['rect']))
    changed = source.copy()
    selected_mask = Image.new('L', source.size, 0)
    draw = ImageDraw.Draw(selected_mask)
    for f, replacement in prepared:
        x, y, w, h = f['rect']
        changed.paste(replacement, (x, y))
        draw.rectangle((x, y, x + w - 1, y + h - 1), fill=255)
    # Compare all four channels explicitly: RGBA getbbox alone can hide RGB-only differences.
    diff = ImageChops.difference(source, changed)
    outside = ImageChops.invert(selected_mask)
    if any(ImageChops.multiply(channel, outside).getbbox() for channel in diff.split()):
        raise ValueError('Unselected source pixels changed')
    unchanged = []
    for f in d['frames']:
        if f['id'] not in selected:
            before = sp.crop_frame(source, f).tobytes()
            after = sp.crop_frame(changed, f).tobytes()
            if before != after:
                raise ValueError('Unselected frame changed: ' + f['id'])
            unchanged.append(dict(id=f['id'], pixelSha256=hashlib.sha256(before).hexdigest()))
    sp.directory(out)
    changed.save(out / 'sheet.png')
    d = dict(d, image='sheet.png', imageSha256=sp.digest(out / 'sheet.png'), status='candidate')
    if d.get('baseline'):
        d['baseline'] = dict(d['baseline'], image=str((manifest.parent / d['baseline']['image']).resolve()))
    sp.write(out / 'sheet.json', d)
    report = dict(ok=True, wholeFramesOnly=True, sourceSha256=sp.digest(manifest.parent / sp.read(manifest)['image']),
                  selectedFrames=selected, replacements=evidence, outsidePixelsUnchanged=True,
                  unchangedFrames=unchanged, visualApproval=False, runtimeVerified=False)
    sp.write(out / 'replacement-proof.json', report)
    return dict(ok=True, replaced=len(selected), untouchedFrames=len(unchanged),
                outsidePixelsUnchanged=True, report=str(out / 'replacement-proof.json'))


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--manifest', type=Path, required=True)
    p.add_argument('--plan', type=Path, required=True)
    p.add_argument('--out', type=Path, required=True)
    a = p.parse_args()
    try:
        result = replace(a.manifest, a.plan, a.out)
    except (OSError, ValueError, TypeError, KeyError) as exc:
        print(json.dumps(dict(ok=False, error=str(exc)), ensure_ascii=False))
        return 2
    print(json.dumps(result, ensure_ascii=False))
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
