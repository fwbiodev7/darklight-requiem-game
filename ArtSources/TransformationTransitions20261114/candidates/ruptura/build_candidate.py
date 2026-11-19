"""Candidate-only whole-figure atlas preservation and measured preview manifest."""
from pathlib import Path
from PIL import Image
import hashlib, json

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3]
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
v2 = HERE / 'RequiemNormalToRuptura-v2-source.png'
v3 = HERE / 'RequiemNormalToRuptura-v4-form-source.png'
base, fire = Image.open(v2).convert('RGBA'), Image.open(v3).convert('RGBA')
assert base.size == fire.size == (1774, 887)
xs, ys = [0, 443, 887, 1330, 1774], [0, 443, 887]
rects = [[xs[n%4], ys[n//4], xs[n%4+1]-xs[n%4], ys[n//4+1]-ys[n//4]] for n in range(8)]
# Preserve approved unaffected complete cells, never anatomical components.
out = fire.copy()
for n in range(3):
    x,y,w,h = rects[n]
    out.paste(base.crop((x,y,x+w,y+h)), (x,y))
candidate = HERE / 'RequiemNormalToRuptura-v5-form-candidate.png'
out.save(candidate)
for n in range(3):
    x,y,w,h = rects[n]
    assert out.crop((x,y,x+w,y+h)).tobytes() == base.crop((x,y,x+w,y+h)).tobytes()
names = ['normal','concentration','gather','first_flames','short_hover','release','descent','ruptura_settle']
durations = [120,160,150,150,70,140,140,320]
assert sum(durations) == 1250 and sum(durations[:5]) == 650
anchor_x = [253,697,1139,1578,250,693,1138,1578]
ground_y = [399,399,393,392,844,844,844,844]
frames = []
for n, (x,y,w,h) in enumerate(rects):
    crop=out.crop((x,y,x+w,y+h))
    b=crop.getchannel('A').point(lambda a: 255 if a >= 16 else 0).getbbox()
    frames.append(dict(id=f'ruptura_enter_{n:02d}',phase=names[n],rect=[x,y,w,h],
        pivot=[(anchor_x[n]-x)/w,1-(ground_y[n]-y)/h],durationMs=durations[n],
        action='TransformationEnter',direction='SE',skin='Requiem',form='NormalToRuptura',
        source=v2.name if n<3 else v3.name,sourceCellIndex=n,
        significantAlphaBoundsLocal=list(b),presentationAnchor='manually observed body center / support plane; hover retained from actual drawing'))
sources=[dict(image=p.name,sha256=sha(p)) for p in [v2,v3,HERE/'RequiemNormalToRuptura-v1-source.png',HERE/'ruptura-fire-user-reference.png']]
references=[dict(path=str(p.relative_to(ROOT)),sha256=sha(p)) for p in [ROOT/'ArtSources/TransformationTransitions20261114/references/ruptura-transition.png',HERE/'ruptura-fire-user-reference.png',ROOT/'ArtSources/WeaponRedesign20261031/katana/references/ReqKatanaNormalIdleSE-whole-frame.png']]
d=dict(schemaVersion=1,image=candidate.name,imageSha256=sha(candidate),nativeSize=list(out.size),
    logicalPixelResolution=None,pixelsPerUnit=157.57576,pixelsPerUnitStatus='reference-only value from approved katana idle meta, not calibrated or integrated',status='candidate',visualReviewed=True,
    imported=False,runtimeVerified=False,frames=frames,
    clips=[dict(name='NormalToRuptura',frames=[f['id'] for f in frames],loop=False,allowStatic=False)],
    requiredClips=['NormalToRuptura'],quality=dict(alphaThreshold=16,maxColors=1000000,maxHeightRatio=1.3),
    sources=sources,references=references,provenance=dict(tool='integrated image_gen',originalSourcesPreserved=True,
        preservation='Frames 0..2 preserve v2 RGBA byte-for-byte as complete cells. Frames 3..7 are complete v4 transformed figures. No anatomy montage, recoloring, alpha edits, rescaling, rotation, mirroring or artificial tween/bobbing.'),
    consumer=dict(path='Assets/Darklight/Runtime/Campaign/DarklightCampaignTransformations.cs',
        durationMs=1250,releaseProgress=0.52,releaseMs=650,
        binding='No Ruptura transformation sprite entry in current TryTransformationPose; candidate only, not integrated.'),
    review=dict(findings=['8 complete figures; blade same hand aimed down-right throughout.',
        'Visible external red-orange flames develop in frame3 and persist frames4..7. Ruptura has strong crimson fringe streaks, red collar/seams/cruciform embroidery, and red katana edge.',
        'First 3 unaffected Normal/concentration figures preserved exactly.',
        'Short hover is drawn in complete frames4..5, no fabricated motion.',
        'Body/face and warm skin remain readable; no smoke wall or wings.'],
        limitations=['Low-alpha generator residuals exist in transparent gutters; sources preserved unchanged.',
            'Most character pixels alpha near253; visually checked on neutral and dark backgrounds, no global alpha threshold applied.',
            '20% transparent safety margin is not met on every side of the source atlas; no figure/katana/flame is visibly cut.',
            'One fixed SE/front view only; native logical pixel density and Unity import PPU not certified.',
            'GIF palette is a presentation derivative; RGBA PNG is authoritative.']))
(HERE/'RequiemNormalToRuptura-v5-form-candidate.json').write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(image=candidate.name,sha256=sha(candidate),frames=8,durationMs=sum(durations),releaseMs=650,preservedFrames=[0,1,2])))
