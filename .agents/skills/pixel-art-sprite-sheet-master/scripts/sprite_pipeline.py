"""Whole-frame sprite metadata, inspection, preview and packing. Never edits anatomy."""
import argparse
import base64
from collections import Counter
import hashlib
import html
import json
import math
from pathlib import Path
import re
import sys

from PIL import Image, ImageDraw


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def write(path, data):
    path = Path(path)
    if path.exists():
        raise ValueError(f'Refusing overwrite: {path}')
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def directory(path):
    if path.exists() and (not path.is_dir() or any(path.iterdir())):
        raise ValueError(f'Output must be new or empty: {path}')
    path.mkdir(parents=True, exist_ok=True)
    return path


def integer(value):
    return type(value) is int


def number(value):
    return type(value) in (int, float) and math.isfinite(value)


def inspect(manifest, unity_meta=None):
    d = read(manifest)
    if not isinstance(d, dict):
        raise ValueError('Manifest must be an object')
    errors, warnings, rows = [], [], []
    if d.get('schemaVersion') != 1:
        errors.append('Unsupported schemaVersion')
    source = (manifest.parent / d['image']).resolve()
    with Image.open(source) as raw:
        mode, fmt = raw.mode, raw.format
        im = raw.convert('RGBA')
    if fmt != 'PNG':
        errors.append('Delivery image must be PNG')
    if mode not in ('RGBA', 'LA', 'P'):
        warnings.append('Source has no alpha channel; verify isolation requirements')
    if d.get('imageSha256') != digest(source):
        errors.append('imageSha256 missing or mismatched')
    if d.get('nativeSize') != list(im.size):
        errors.append('nativeSize differs from actual PNG dimensions')
    if not number(d.get('pixelsPerUnit')) or d['pixelsPerUnit'] <= 0:
        errors.append('pixelsPerUnit must be positive and finite')
    q = d.get('quality', {})
    if not isinstance(q, dict):
        raise ValueError('quality must be an object')
    if q.get('requireTransparency', True) and im.getchannel('A').getextrema()[0] != 0:
        errors.append('No fully transparent background pixels')
    threshold = q.get('alphaThreshold', 0)
    if not integer(threshold) or not 0 <= threshold <= 254:
        errors.append('alphaThreshold must be integer 0..254')
        threshold = 0
    max_colors = q.get('maxColors')
    if max_colors is not None and (not integer(max_colors) or max_colors <= 0):
        errors.append('maxColors must be positive integer')
        max_colors = None
    height_ratio = q.get('maxHeightRatio', 1.3)
    if not number(height_ratio) or height_ratio < 1:
        errors.append('maxHeightRatio must be finite and >= 1')
        height_ratio = 1.3
    frames = d.get('frames', [])
    if not isinstance(frames, list) or not frames:
        errors.append('frames must be a nonempty array')
        frames = []
    ids, by_id, valid = set(), {}, []
    for f in frames:
        if not isinstance(f, dict):
            errors.append('Each frame must be an object')
            continue
        fid = f.get('id')
        if not isinstance(fid, str) or not fid or fid in ids:
            errors.append(f'Duplicate/invalid frame id: {fid}')
            continue
        ids.add(fid)
        rect = f.get('rect', [])
        if (not isinstance(rect, list) or len(rect) != 4 or
                not all(integer(v) for v in rect)):
            errors.append(f'{fid}: rect must contain four integers')
            continue
        x, y, w, h = rect
        if min(x, y) < 0 or min(w, h) <= 0 or x + w > im.width or y + h > im.height:
            errors.append(f'{fid}: rect outside image or nonpositive size')
            continue
        pivot = f.get('pivot', [])
        if (not isinstance(pivot, list) or len(pivot) != 2 or
                not all(number(v) and 0 <= v <= 1 for v in pivot)):
            errors.append(f'{fid}: pivot must be finite normalized bottom-left xy')
        if not number(f.get('durationMs')) or f['durationMs'] <= 0:
            errors.append(f'{fid}: durationMs must be positive and finite')
        crop = im.crop((x, y, x + w, y + h))
        alpha = crop.getchannel('A')
        mask = alpha.point(lambda v: 255 if v > threshold else 0)
        box = mask.getbbox()
        colors = Counter(pixel for pixel in crop.getdata() if pixel[3] > threshold)
        if not box:
            errors.append(f'{fid}: empty frame')
        else:
            if box[0] == 0 or box[1] == 0 or box[2] == w or box[3] == h:
                warnings.append(f'{fid}: visible pixels touch frame edge; inspect clipping')
            if max_colors and len(colors) > max_colors:
                warnings.append(f'{fid}: {len(colors)} colors exceed declared {max_colors}')
        partial = sum(count for (r, g, b, a), count in colors.items() if a < 255)
        row = dict(id=fid, rect=rect, bbox=list(box) if box else None,
                   visiblePixels=sum(colors.values()), colors=len(colors), partialAlphaPixels=partial,
                   pixelSha256=hashlib.sha256(crop.tobytes()).hexdigest())
        rows.append(row)
        by_id[fid] = row
        valid.append((fid, rect))
    overlaps = []
    for i, (aid, (ax, ay, aw, ah)) in enumerate(valid):
        for bid, (bx, by, bw, bh) in valid[i + 1:]:
            if ax < bx + bw and bx < ax + aw and ay < by + bh and by < ay + ah:
                overlaps.append([aid, bid])
    if overlaps:
        warnings.append(f'{len(overlaps)} overlapping frame rectangles; inspect irregular sheet ownership')
    clips = d.get('clips', [])
    if not isinstance(clips, list) or not clips:
        errors.append('clips must be a nonempty array')
        clips = []
    names = set()
    clip_rows = []
    for clip in clips:
        if not isinstance(clip, dict):
            errors.append('Each clip must be an object')
            continue
        name = clip.get('name')
        if not isinstance(name, str) or not name or name in names:
            errors.append(f'Duplicate/invalid clip name: {name}')
            continue
        names.add(name)
        if type(clip.get('loop', False)) is not bool or type(clip.get('allowStatic', False)) is not bool:
            errors.append(f'{name}: loop and allowStatic must be booleans')
        sequence = clip.get('frames', [])
        if not isinstance(sequence, list) or not sequence:
            errors.append(f'{name}: empty/invalid sequence')
            continue
        if any(not isinstance(fid, str) or fid not in by_id for fid in sequence):
            errors.append(f'{name}: references unknown/invalid frames')
            continue
        unique = len({by_id[fid]['pixelSha256'] for fid in sequence})
        if unique == 1 and not clip.get('allowStatic', False):
            errors.append(f'{name}: no distinct animation poses; declare intentional allowStatic hold or replace')
        elif unique < len(sequence):
            warnings.append(f'{name}: repeated poses; inspect intentional holds and loop')
        heights = [by_id[fid]['bbox'][3] - by_id[fid]['bbox'][1]
                   for fid in sequence if by_id[fid]['bbox']]
        if heights and max(heights) / min(heights) > height_ratio:
            warnings.append(f'{name}: apparent height variation; inspect anatomy versus intentional pose/effects')
        clip_rows.append(dict(name=name, frames=len(sequence), distinctPoses=unique))
    for name in d.get('requiredClips', []):
        if name not in names:
            errors.append(f'Required clip missing: {name}')
    baseline = d.get('baseline')
    if baseline:
        bp = manifest.parent / baseline['image']
        if not bp.is_file() or digest(bp) != baseline.get('sha256'):
            errors.append('Approved baseline missing or SHA256 changed')
    if rows and sum(r['partialAlphaPixels'] for r in rows) > sum(r['visiblePixels'] for r in rows) * 0.1:
        warnings.append('More than 10% of visible samples have partial alpha; inspect halos/native pixel rendering')
    if unity_meta:
        text = unity_meta.read_text(encoding='utf-8-sig')
        for key, expected in [('filterMode', '0'), ('enableMipMap', '0'),
                              ('textureCompression', '0'), ('nPOTScale', '0'), ('textureType', '8')]:
            vals = re.findall(r'^\s*' + key + r':\s*(\S+)', text, re.M)
            if not vals or any(v != expected for v in vals):
                warnings.append(f'Unity {key}: expected {expected}, observed {vals}; inspect overrides')
        ppus = re.findall(r'^\s*spritePixelsToUnits:\s*(\S+)', text, re.M)
        if not ppus or any(float(v) != d.get('pixelsPerUnit') for v in ppus):
            warnings.append('Unity PPU missing/differs from manifest')
        warnings.append('Unity text inspection only: import, platform overrides and runtime not verified')
    return d, im, dict(ok=not errors, source=str(source), sourceSha256=digest(source),
                      errors=errors, warnings=warnings, frames=rows, clips=clip_rows,
                      overlaps=overlaps, visualApproval=False, runtimeVerified=False)


def checked(path):
    d, im, report = inspect(path)
    if report['errors']:
        raise ValueError('; '.join(report['errors'][:8]))
    return d, im, report


def crop_frame(im, f):
    x, y, w, h = f['rect']
    return im.crop((x, y, x + w, y + h))


def common_canvas(d):
    # Round the complete-frame anchor to integer pixels; never move body parts.
    anchors = {f['id']: (round(f['pivot'][0] * f['rect'][2]),
                          round((1 - f['pivot'][1]) * f['rect'][3])) for f in d['frames']}
    left = max(a[0] for a in anchors.values())
    top = max(a[1] for a in anchors.values())
    right = max(f['rect'][2] - anchors[f['id']][0] for f in d['frames'])
    bottom = max(f['rect'][3] - anchors[f['id']][1] for f in d['frames'])
    return (left + right, top + bottom), (left, top), anchors


def aligned(d, im):
    size, target, anchors = common_canvas(d)
    output = {}
    for f in d['frames']:
        canvas = Image.new('RGBA', size)
        ax, ay = anchors[f['id']]
        canvas.paste(crop_frame(im, f), (target[0] - ax, target[1] - ay))
        output[f['id']] = canvas
    return output


def grid(a):
    with Image.open(a.image) as src:
        size = src.size
    w, h = a.cell
    if min(w, h, a.columns, a.count) <= 0:
        raise ValueError('Cell, columns and count must be positive')
    if a.columns * w > size[0] or math.ceil(a.count / a.columns) * h > size[1]:
        raise ValueError('Requested grid does not fit source')
    frames = [dict(id=f'frame_{i:03}', rect=[i % a.columns * w, i // a.columns * h, w, h],
                   pivot=[0.5, 0], durationMs=a.duration, action='unspecified', direction='unspecified',
                   skin='default', form='normal') for i in range(a.count)]
    image_path = Path(__import__('os').path.relpath(a.image.resolve(), a.out.parent.resolve())).as_posix()
    write(a.out, dict(schemaVersion=1, image=image_path, imageSha256=digest(a.image),
                     nativeSize=list(size), pixelsPerUnit=a.ppu, status='candidate', frames=frames,
                     clips=[dict(name='sequence', frames=[f['id'] for f in frames], loop=True,
                                 allowStatic=a.count == 1)]))
    return dict(ok=True, frames=len(frames), manifest=str(a.out))


def from_aseprite(a):
    raw = read(a.input)
    frames = raw['frames']
    if isinstance(frames, dict):
        frames = [dict(v, filename=k) for k, v in frames.items()]
    result = []
    for i, f in enumerate(frames):
        if f.get('rotated') or f.get('trimmed'):
            raise ValueError('Re-export without trim/rotation to preserve complete original frames')
        r = f['frame']
        result.append(dict(id=f.get('filename', f'frame_{i:03}'),
                           rect=[r[k] for k in ('x', 'y', 'w', 'h')], pivot=[0.5, 0],
                           durationMs=f['duration'], action='unspecified', direction='unspecified'))
    clips = []
    for tag in raw.get('meta', {}).get('frameTags', []):
        start, end = tag['from'], tag['to']
        if not (integer(start) and integer(end) and 0 <= start <= end < len(result)):
            raise ValueError('Tag bounds invalid')
        indexes = list(range(start, end + 1))
        direction = tag.get('direction', 'forward')
        if direction in ('reverse', 'pingpong_reverse'):
            indexes.reverse()
        elif direction not in ('forward', 'pingpong'):
            raise ValueError('Unsupported tag direction: ' + direction)
        if direction.startswith('pingpong'):
            indexes += indexes[-2:0:-1]
        clips.append(dict(name=tag['name'], frames=[result[i]['id'] for i in indexes],
                          loop=True, allowStatic=len(indexes) == 1))
    if not clips:
        clips = [dict(name='sequence', frames=[f['id'] for f in result], loop=True,
                      allowStatic=len(result) == 1)]
    image = a.input.parent / raw['meta']['image']
    with Image.open(image) as im:
        size = im.size
    relative = Path(__import__('os').path.relpath(image.resolve(), a.out.parent.resolve())).as_posix()
    write(a.out, dict(schemaVersion=1, image=relative, imageSha256=digest(image), nativeSize=list(size),
                     pixelsPerUnit=a.ppu, status='candidate', frames=result, clips=clips))
    return dict(ok=True, frames=len(result), clips=len(clips))


def palette(a):
    with Image.open(a.image) as im:
        counts = Counter(px for px in im.convert('RGBA').getdata() if px[3] > 0)
    write(a.out, dict(source=str(a.image), sha256=digest(a.image), colors=len(counts),
                     entries=[dict(rgba=list(px), count=n) for px, n in counts.most_common()]))
    return dict(ok=True, colors=len(counts), palette=str(a.out))


def audit(a):
    _, _, report = inspect(a.manifest, a.unity_meta)
    directory(a.out)
    write(a.out / 'report.json', report)
    lines = ['# Sprite sheet technical report', '', f"Technical result: {'PASS' if report['ok'] else 'FAIL'}",
             f"Frames: {len(report['frames'])}; clips: {len(report['clips'])}",
             f"Source SHA-256: {report['sourceSha256']}", '', '## Errors', '']
    lines += ['- ' + x for x in report['errors']] or ['None.']
    lines += ['', '## Review warnings', '']
    lines += ['- ' + x for x in report['warnings']] or ['None.']
    lines += ['', '## Limits', '', 'Visual identity, anatomy, perceived motion, user approval and Unity runtime are not certified.',
              'Inspect contact sheet and animated preview; record findings in the delivery report.']
    (a.out / 'REPORT.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
    return dict(ok=report['ok'], frames=len(report['frames']), errors=len(report['errors']),
                warnings=len(report['warnings']), report=str(a.out / 'report.json'))


def preview(a):
    d, im, _ = checked(a.manifest)
    if not 1 <= a.zoom <= 8:
        raise ValueError('zoom must be integer 1..8')
    directory(a.out)
    cells = aligned(d, im)
    cw, ch = next(iter(cells.values())).size
    cols = min(6, len(cells))
    contact = Image.new('RGB', (cols * cw, math.ceil(len(cells) / cols) * (ch + 18)), '#30343a')
    draw = ImageDraw.Draw(contact)
    for i, (fid, frame) in enumerate(cells.items()):
        x, y = i % cols * cw, i // cols * (ch + 18)
        contact.paste(frame, (x, y), frame.getchannel('A'))
        draw.text((x + 2, y + ch + 2), fid, fill='white')
    contact.save(a.out / 'contact.png')
    lookup = {f['id']: f for f in d['frames']}
    gif_paths = []
    for i, clip in enumerate(d['clips']):
        images, durations = [], []
        for fid in clip['frames']:
            bg = Image.new('RGB', (cw, ch), '#30343a')
            frame = cells[fid]
            bg.paste(frame, (0, 0), frame.getchannel('A'))
            images.append(bg.resize((cw * a.zoom, ch * a.zoom), Image.Resampling.NEAREST))
            durations.append(max(10, round(lookup[fid]['durationMs'] / 10) * 10))
        name = f'clip-{i:02}.gif'
        options = dict(save_all=True, append_images=images[1:], duration=durations, disposal=2)
        if clip.get('loop', False):
            options['loop'] = 0
        images[0].save(a.out / name, **options)
        gif_paths.append(name)
    # Embed source; the HTML remains usable when moved without a local server.
    source = (a.manifest.parent / d['image']).read_bytes()
    payload = json.dumps(d, ensure_ascii=False).replace('<', '\\u003c')
    encoded = base64.b64encode(source).decode('ascii')
    page = '''<!doctype html><meta charset="utf-8"><title>Sprite animation review</title>
<style>body{font:16px system-ui;background:#181c23;color:#eee;padding:20px}canvas{image-rendering:pixelated;border:1px solid #888;max-width:100%}button,select,input{margin:6px}a{color:#9cf}</style>
<h1>Sprite animation review</h1><p>Candidate preview. Inspect identity, feet, weapons and loop. No audio.</p>
<select id="clip"></select><button id="play">Pause</button><button id="step">Next frame</button>
<label>Frame <select id="frame"></select></label>
<label>Speed <input id="speed" type="range" min="0.25" max="2" step="0.25" value="1"></label>
<label>Zoom <select id="zoom"><option>1</option><option>2</option><option selected>3</option><option>4</option></select></label>
<label>Background <input id="bg" type="color" value="#30343a"></label><p id="info"></p><canvas id="view"></canvas>
<p><a href="contact.png">Contact sheet</a></p><script>
const d=PAYLOAD, image=new Image(), byId=Object.fromEntries(d.frames.map(f=>[f.id,f]));
const clipEl=document.getElementById('clip'), play=document.getElementById('play'), step=document.getElementById('step'),
speed=document.getElementById('speed'),zoom=document.getElementById('zoom'),bg=document.getElementById('bg'),info=document.getElementById('info'),
frameEl=document.getElementById('frame'),
c=document.getElementById('view'),ctx=c.getContext('2d');
const anchors=d.frames.map(f=>[Math.round(f.pivot[0]*f.rect[2]),Math.round((1-f.pivot[1])*f.rect[3])]);
const anchorLeft=Math.max(...anchors.map(a=>a[0])),anchorTop=Math.max(...anchors.map(a=>a[1]));
c.width=anchorLeft+Math.max(...d.frames.map((f,i)=>f.rect[2]-anchors[i][0]));
c.height=anchorTop+Math.max(...d.frames.map((f,i)=>f.rect[3]-anchors[i][1]));
d.clips.forEach((v,i)=>{const o=document.createElement('option');o.value=i;o.textContent=v.name;clipEl.append(o)});
let index=0,running=true,last=0,elapsed=0;
function frameOptions(){frameEl.replaceChildren();d.clips[Number(clipEl.value)].frames.forEach((id,i)=>{const o=document.createElement('option');o.value=i;o.textContent=(i+1)+' - '+id;frameEl.append(o)})}frameOptions();
function draw(){if(!image.complete||!image.naturalWidth)return;const clip=d.clips[Number(clipEl.value)],f=byId[clip.frames[index]],r=f.rect;
ctx.fillStyle=bg.value;ctx.fillRect(0,0,c.width,c.height);ctx.imageSmoothingEnabled=false;
ctx.drawImage(image,...r,anchorLeft-Math.round(f.pivot[0]*r[2]),anchorTop-Math.round((1-f.pivot[1])*r[3]),r[2],r[3]);
c.style.width=c.width*Number(zoom.value)+'px';c.style.height=c.height*Number(zoom.value)+'px';
info.textContent=clip.name+' | '+(index+1)+'/'+clip.frames.length+' | '+f.id+' | '+f.durationMs+' ms';frameEl.value=index;}
function next(){const clip=d.clips[Number(clipEl.value)];if(index+1<clip.frames.length)index++;else if(clip.loop)index=0;else{running=false;play.textContent='Play'}draw()}
play.onclick=()=>{running=!running;if(running&&index===d.clips[Number(clipEl.value)].frames.length-1)index=0;play.textContent=running?'Pause':'Play';elapsed=0;draw()};
step.onclick=()=>{running=false;play.textContent='Play';next()};clipEl.onchange=()=>{index=0;elapsed=0;frameOptions();draw()};zoom.onchange=draw;bg.oninput=draw;
frameEl.onchange=()=>{index=Number(frameEl.value);running=false;play.textContent='Play';elapsed=0;draw()};
function tick(now){if(last&&running&&image.complete){elapsed+=Math.min(now-last,250)*Number(speed.value);let guard=0;
while(running&&elapsed>=byId[d.clips[Number(clipEl.value)].frames[index]].durationMs&&guard++<100){elapsed-=byId[d.clips[Number(clipEl.value)].frames[index]].durationMs;next()}}last=now;requestAnimationFrame(tick)}
image.onload=()=>{draw();requestAnimationFrame(tick)};image.src='data:image/png;base64,ENCODED';
</script>'''.replace('PAYLOAD', payload).replace('ENCODED', encoded)
    (a.out / 'preview.html').write_text(page, encoding='utf-8')
    write(a.out / 'preview.json', dict(sourceSha256=digest(a.manifest.parent / d['image']),
                                     gifs=gif_paths, gifQuantized=True, visuallyReviewed=False))
    return dict(ok=True, clips=len(gif_paths), preview=str(a.out / 'preview.html'))


def pack(a):
    d, im, _ = checked(a.manifest)
    if a.columns <= 0:
        raise ValueError('columns must be positive')
    directory(a.out)
    cells = aligned(d, im)
    (w, h), anchor, _ = common_canvas(d)
    count = len(d['frames'])
    columns = min(a.columns, count)
    atlas = Image.new('RGBA', (columns * w, math.ceil(count / columns) * h))
    frames = []
    for i, f in enumerate(d['frames']):
        x, y = i % columns * w, i // columns * h
        atlas.paste(cells[f['id']], (x, y))
        frames.append(dict(f, rect=[x, y, w, h], pivot=[anchor[0] / w, 1 - anchor[1] / h],
                           provenance=dict(sourceSha256=d['imageSha256'], rect=f['rect'], pivot=f['pivot'])))
    atlas.save(a.out / 'sheet.png')
    d = dict(d, image='sheet.png', imageSha256=digest(a.out / 'sheet.png'), nativeSize=list(atlas.size),
             frames=frames, status='candidate')
    if 'baseline' in d:
        d['baseline'] = dict(d['baseline'], image=str((a.manifest.parent / d['baseline']['image']).resolve()))
    write(a.out / 'sheet.json', d)
    return dict(ok=True, frames=count, sheet=str(a.out / 'sheet.png'))


def unity_json(a):
    d, im, _ = checked(a.manifest)
    frames = []
    for f in d['frames']:
        x, y, w, h = f['rect']
        frames.append(dict(f, rect=[x, im.height - y - h, w, h]))
    relative = Path(__import__('os').path.relpath((a.manifest.parent / d['image']).resolve(), a.out.parent.resolve())).as_posix()
    write(a.out, dict(schemaVersion=1, coordinateSystem='bottom-left',
                     sourceSha256=d['imageSha256'], image=relative, imageSize=list(im.size),
                     pixelsPerUnit=d['pixelsPerUnit'], frames=frames, clips=d['clips'],
                     automaticUnityImport=False))
    return dict(ok=True, frames=len(frames), metadata=str(a.out))


def main():
    p = argparse.ArgumentParser(description=__doc__)
    sub = p.add_subparsers(dest='command', required=True)
    for name in ['grid', 'from-aseprite', 'palette', 'audit', 'preview', 'pack', 'unity-json']:
        s = sub.add_parser(name)
        s.add_argument('--out', type=Path, required=True)
        if name in ('grid', 'palette'):
            s.add_argument('--image', type=Path, required=True)
        elif name == 'from-aseprite':
            s.add_argument('--input', type=Path, required=True)
        else:
            s.add_argument('--manifest', type=Path, required=True)
        if name in ('grid', 'from-aseprite'):
            s.add_argument('--ppu', type=float, default=32)
        if name == 'grid':
            s.add_argument('--cell', type=int, nargs=2, required=True)
            s.add_argument('--columns', type=int, required=True)
            s.add_argument('--count', type=int, required=True)
            s.add_argument('--duration', type=int, default=100)
        elif name == 'audit':
            s.add_argument('--unity-meta', type=Path)
        elif name == 'preview':
            s.add_argument('--zoom', type=int, default=3)
        elif name == 'pack':
            s.add_argument('--columns', type=int, default=6)
    a = p.parse_args()
    try:
        result = globals()[a.command.replace('-', '_')](a)
    except (ValueError, OSError, KeyError, TypeError, IndexError) as exc:
        print(json.dumps(dict(ok=False, error=str(exc)), ensure_ascii=False))
        return 2
    print(json.dumps(result, ensure_ascii=False))
    return 0 if result.get('ok') else 2


if __name__ == '__main__':
    raise SystemExit(main())
