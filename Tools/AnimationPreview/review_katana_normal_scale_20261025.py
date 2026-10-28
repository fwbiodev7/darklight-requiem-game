"""Compare whole frames at one world-space scale; does not edit sprite pixels."""
from pathlib import Path
import json
import math
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
STAGE = ROOT/'outputs/redesign-armas-20261025/katana-normal'
RESOURCE = ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
DIRS = ['S','SW','W','NW','N','NE','E','SE']
WORLD_PPU = 112
CELL = 360
ROW = 400
FONT = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 16)
SMALL = ImageFont.truetype('C:/Windows/Fonts/arial.ttf', 14)


def load_page(page):
    name = page['resourcePath'].split('/')[-1]
    folder = STAGE if (STAGE/(name+'.json')).exists() else RESOURCE
    manifest = json.loads((folder/(name+'.json')).read_text(encoding='utf-8'))
    return folder, manifest


def selected(page):
    folder, manifest = load_page(page)
    index = 3 if page['action']=='Attack' else 0
    frame_id = page['spriteNames'][index]
    frame = next(f for f in manifest['frames'] if f['id']==frame_id)
    image = Image.open(folder/manifest['image']).convert('RGBA')
    x,y,w,h = frame['rect']
    crop = image.crop((x,y,x+w,y+h))
    ppu = page['pixelsPerUnit']
    shown = crop.resize((round(w*WORLD_PPU/ppu),round(h*WORLD_PPU/ppu)),Image.Resampling.NEAREST)
    measurements = manifest.get('ppuProvenance',{}).get('measurements',[])
    measurement = next((m for m in measurements if m['sourceFrameIndex']==frame['sourceFrameIndex']),None)
    if measurement is None and page['action']=='Attack':
        # Approved complete source measurements recorded on 24/10 staging.
        old = {
            0: dict(headY=88, soleY=355),
            1: dict(headY=421, soleY=669),
            2: dict(headY=730, soleY=973),
        }.get(page['direction'])
        if old:
            measurement = dict(anatomyHeight=old['soleY']-old['headY'])
    body = None if not measurement else measurement['anatomyHeight']/ppu
    return manifest,frame,shown,ppu,body


def main():
    live = json.loads((RESOURCE/'catalog.json').read_text(encoding='utf-8'))['pages']
    fresh = json.loads((STAGE/'katana-normal-pages.json').read_text(encoding='utf-8'))['pages']
    pages = {(p['direction'],p['action']):p for p in live if p['skin']==0 and p['weapon']==0 and p['form']==0 and
             (p['action']=='Idle' or p.get('attackKey')=='Leve1')}
    pages.update({(p['direction'],p['action']):p for p in fresh})
    canvas = Image.new('RGBA',(CELL*8,ROW*2+95),(26,31,39,255))
    draw = ImageDraw.Draw(canvas)
    draw.text((15,12),'KATANA / REQUIEM / NORMAL - comparable whole frames at 112 display pixels per WORLD UNIT',font=FONT,fill='white')
    draw.text((15,36),'Attack: recovered guard (phase 3). Idle: neutral stopped pose (phase 0). Nearest-neighbor display; source pixels untouched.',font=SMALL,fill='#bec8d6')
    entries=[]
    for row,action in enumerate(['Attack','Idle']):
        for direction in range(8):
            page=pages[(direction,action)]
            manifest,frame,shown,ppu,body=selected(page)
            cx=direction*CELL+CELL//2
            ground=95+row*ROW+ROW-42
            px=cx-round(frame['pivot'][0]*shown.width)
            py=ground-round((1-frame['pivot'][1])*shown.height)
            canvas.alpha_composite(shown,(px,py))
            draw.line((direction*CELL+8,ground,(direction+1)*CELL-8,ground),fill='#657787',width=1)
            for height in [1,1.98]:
                gy=ground-round(height*WORLD_PPU)
                draw.line((direction*CELL+8,gy,(direction+1)*CELL-8,gy),fill='#35424f',width=1)
            draw.line((cx-4,ground,cx+4,ground),fill='#49d8d0',width=2)
            draw.line((cx,ground-4,cx,ground+4),fill='#49d8d0',width=2)
            label=DIRS[direction]+' '+('Leve1 guard' if action=='Attack' else 'stopped / Idle0')
            draw.text((direction*CELL+10,95+row*ROW+8),label,font=FONT,fill='white')
            b='unknown' if body is None else f'{body:.3f}u'
            draw.text((direction*CELL+10,95+row*ROW+32),f'PPU {ppu:.3f} | hair-to-sole {b}',font=SMALL,fill='#bec8d6')
            entries.append(dict(direction=DIRS[direction],action=action,frameId=frame['id'],sourceFrameIndex=frame['sourceFrameIndex'],
                                pixelsPerUnit=ppu,anatomicalWorldHeight=body,displayPixelsPerWorldUnit=WORLD_PPU,
                                worldCanvas=[shown.width/WORLD_PPU,shown.height/WORLD_PPU],sourcePath=manifest['sourcePath'],
                                limitation='Manual hair-to-sole measurement; pose/perspective and projected sole anchor create uncertainty. No anatomy resized.'))
    canvas.convert('RGB').save(STAGE/'world-scale-review.png')
    (STAGE/'world-scale-review.json').write_text(json.dumps(dict(schemaVersion=1,date='2026-10-25',
        sourcePixelsEdited=False,worldDisplayPixelsPerUnit=WORLD_PPU,figureScaling='Display only, whole sprite, by actual catalog/staging PPU.',
        benchmark='Approved Normal Requiem Leve1 S/SW/W existing complete figures.',entries=entries),indent=2),encoding='utf-8')
    print(json.dumps(dict(panel='outputs/redesign-armas-20261025/katana-normal/world-scale-review.png',figures=len(entries))))


if __name__=='__main__':
    main()
