"""Package nine reviewed whole figures. No catalog/runtime/Unity changes."""
import hashlib
import json
import runpy
import shutil
import uuid
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
ART = Path(__file__).resolve().parent
OUT = ART / 'prepared'
OUT.mkdir(exist_ok=True)
NAME = 'LuciusSwordThrustSWN20261119V1'
SOURCE = ART / (NAME + '-source.png')
TARGET = OUT / (NAME + '.png')
shutil.copy2(SOURCE, TARGET)
im = Image.open(SOURCE).convert('RGBA')
assert im.size == (1254, 1254)
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
save = lambda p, d: p.write_text(json.dumps(d, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
rows = [(0, 410, [0, 410, 835, 1254]), (410, 795, [0, 360, 830, 1254]), (795, 1254, [0, 410, 835, 1254])]
directions = [('S', 0), ('W', 2), ('N', 4)]
times = [200, 180, 260]
ppu = float(format(305 * 214.9709011 / 410, '.12g'))
frames, clips, pages, facts = [], [], [], []
contact = Image.new('RGB', (1500, 1410), (42, 46, 55))
for row, ((top, bottom, xs), (direction, direction_id)) in enumerate(zip(rows, directions)):
    ids, canvases = [], []
    for phase in range(3):
        x, y, w, h = xs[phase], top, xs[phase+1] - xs[phase], bottom - top
        crop = im.crop((x, y, x+w, y+h))
        alpha = crop.getchannel('A')
        bbox = alpha.point(lambda a: 255 if a >= 16 else 0).getbbox()
        assert bbox and bbox[0] > 0 and bbox[1] > 0 and bbox[2] < w and bbox[3] < h
        sole_band = alpha.crop((0, bbox[3]-30, w, bbox[3])).point(lambda a: 255 if a >= 128 else 0).getbbox()
        anchor = [(sole_band[0]+sole_band[2])//2, bbox[3]]
        fid = f'Redesign_{NAME}_{direction}_{phase:02d}'
        ids.append(fid)
        frames.append(dict(id=fid, action='Attack', attackKey='SwordThrust', direction=direction,
                           index=phase, phase=['startup','active','recovery'][phase],
                           rect=[x,y,w,h], pivot=[anchor[0]/w,(h-anchor[1])/h], durationMs=times[phase]))
        facts.append(dict(id=fid, rect=[x,y,w,h], visibleBoundsAlpha16=list(bbox),
                          groundLocal=anchor, rgbaSha256=hashlib.sha256(crop.tobytes()).hexdigest()))
        canvas = Image.new('RGBA', (500, 470), (42,46,55,255))
        draw = ImageDraw.Draw(canvas)
        draw.line((0,430,500,430), fill=(90,120,110))
        canvas.alpha_composite(crop, (250-anchor[0],430-anchor[1]))
        draw.text((12,12), f'{direction} {phase} {times[phase]}ms', fill='white')
        canvases.append(canvas.convert('RGB'))
        contact.paste(canvases[-1], (phase*500,row*470))
    canvases[0].save(OUT / f'{NAME}-{direction}.gif',save_all=True,append_images=canvases[1:],duration=times,loop=0,disposal=2)
    clips.append(dict(name=f'{NAME}_{direction}', frames=ids, loop=False, allowStatic=False))
    pages.append(dict(id=f'{NAME}_Attack_{direction}', action='Attack', attackKey='SwordThrust', direction=direction_id,
                      resourcePath='Darklight/LuciusNormal20261109/'+NAME, imageSha256=sha(TARGET),
                      sourceSha256=sha(SOURCE), sourcePath=SOURCE.relative_to(ROOT).as_posix(), nativeSize=list(im.size),
                      pixelsPerUnit=ppu, spriteNames=ids, durationMs=times, startupFrames=1, activeFrames=1,
                      wholeFigures=True,loop=False,status='visual-reviewed'))
contact.save(OUT / (NAME+'-contact.png'))
manifest = dict(schemaVersion=1, image=TARGET.name, imageSha256=sha(TARGET), sourcePath=SOURCE.relative_to(ROOT).as_posix(),
                sourceSha256=sha(SOURCE), sourceSize=list(im.size),nativeSize=list(im.size),pixelsPerUnit=ppu,
                status='visual-reviewed',wholeFigures=True,frames=frames,clips=clips,quality=dict(alphaThreshold=15),
                bodyHeightPixels=305, scaleReference='Lucius Normal S head-to-sole 410px at 214.9709011 PPU',
                pivotMethod='Reviewed floor projection, center of boot sole contacts in bottom 30 opaque rows',
                timing='Three phases aggregate previous 100+100 / 90+90 / 120+140 ms; total 640ms; no runtime timing changes')
save(OUT / (NAME+'.json'), manifest)
save(OUT / 'pages-thrust-swn.json', dict(schemaVersion=1,characterId='Lucius',form='Normal',weapon='Sword',complete=False,pages=pages))
helper = runpy.run_path(str(ROOT/'Tools/AnimationPreview/prepare_fists_combat_stance_20261118.py'))
namespace = helper['texture_meta'].__globals__
namespace['identity'] = lambda key: uuid.uuid5(uuid.NAMESPACE_URL,'darklight/lucius-normal-thrust-20261119/'+key).hex
dest = ROOT/'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
(OUT/(NAME+'.png.meta')).write_bytes(helper['texture_meta'](dest/(NAME+'.png.meta'),im.size,ppu,frames))
(OUT/(NAME+'.json.meta')).write_bytes(helper['text_meta'](dest/(NAME+'.json.meta')))
refs = [ROOT/'Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png',
        dest/'LuciusNormalIdle8V1.png',dest/'LuciusSwordThrustV1.png']
save(OUT/'review.json',dict(status='visual-reviewed',produced=True,reviewed=True,copiedToAssets=False,unityImported=False,runtimeVerified=False,
     sourcePath=SOURCE.relative_to(ROOT).as_posix(),sourceSha256=sha(SOURCE),sourcePixelsUnchanged=sha(SOURCE)==sha(TARGET),
     pixelsResampled=False,wholeFigures=True,frames=facts,pixelsPerUnit=ppu,
     references=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)) for p in refs],
     reviewedFindings=['S front-facing foreshortened thrust; W true left profile straight blade; N back-facing with face occluded',
      'All nine heads, hands, legs, boots and full swords inside recorded rectangles; no assembled anatomy',
      'Common sheet PPU derives from 305px body, excluding blade and impact effect',
      'Native partial alpha fringe retained; no global recoloring or alpha erasure'],
     pending=['Remaining SW/NW/NE/E/SE thrust directions', 'Unity import and gameplay evaluation remain unexecuted']))
print(json.dumps(dict(frames=len(frames),pages=len(pages),pixelsPerUnit=ppu,imageSha256=sha(TARGET),sourcePixelsUnchanged=sha(SOURCE)==sha(TARGET))))
