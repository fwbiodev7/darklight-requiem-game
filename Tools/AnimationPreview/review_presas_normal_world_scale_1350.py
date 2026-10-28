"""Compare complete figures using actual PPU and gameplay WorldVisualScale."""
import json
from pathlib import Path
import re

from PIL import Image,ImageDraw,ImageFont

ROOT=Path(__file__).resolve().parents[2]
STAGE=ROOT/'outputs/redesign-armas-20261025-retomada1350/presas-normal'
LIVE=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
DIRS=['S','SW','W','NW','N','NE','E','SE']
CELL,ROW,DISPLAY_PPU=400,480,112


def main():
    code=(ROOT/'Assets/Darklight/Runtime/Character/DarklightCharacterView.cs').read_text(encoding='utf-8')
    scale=float(re.search(r'WorldVisualScale\s*=\s*([0-9.]+)f',code).group(1))
    pages=json.loads((LIVE/'catalog.json').read_text(encoding='utf-8'))['pages']
    pages+=json.loads((STAGE/'presas-normal-pages.json').read_text(encoding='utf-8'))['pages']
    chosen={(p['direction'],p['action']):p for p in pages if p['skin']==0 and p['weapon']==1 and p['form']==0 and
            (p['action']=='Idle' or p.get('attackKey')=='Presas1')}
    panel=Image.new('RGBA',(CELL*8,ROW*2+95),(25,31,39,255))
    draw=ImageDraw.Draw(panel)
    font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',16)
    draw.text((12,12),f'PRESAS / REQUIEM / NORMAL | same112display pixels/world unit | actual WorldVisualScale={scale}',font=font,fill='white')
    draw.text((12,38),'Whole figures, actual PPU, nearest-neighbor display. Attack recovery above / stopped Idle0 below. No per-cell autofit.',font=font,fill='#bdc9da')
    records=[]
    for row,action in enumerate(['Attack','Idle']):
        for direction in range(8):
            page=chosen[(direction,action)]
            name=page['resourcePath'].split('/')[-1]
            folder=STAGE if (STAGE/(name+'.json')).exists() else LIVE
            manifest=json.loads((folder/(name+'.json')).read_text(encoding='utf-8'))
            frame_id=page['spriteNames'][3 if action=='Attack' else 0]
            frame=next(f for f in manifest['frames'] if f['id']==frame_id)
            image=Image.open(folder/manifest['image']).convert('RGBA')
            x,y,w,h=frame['rect']
            complete=image.crop((x,y,x+w,y+h))
            factor=DISPLAY_PPU*scale/page['pixelsPerUnit']
            shown=complete.resize((round(w*factor),round(h*factor)),Image.Resampling.NEAREST)
            ground=95+ROW*(row+1)-35
            cx=direction*CELL+CELL//2
            panel.alpha_composite(shown,(cx-round(frame['pivot'][0]*shown.width),ground-round((1-frame['pivot'][1])*shown.height)))
            for height in [0,1,1.98]:
                yy=ground-round(height*scale*DISPLAY_PPU)
                draw.line((direction*CELL+10,yy,(direction+1)*CELL-10,yy),fill='#495c6c' if height==0 else '#30404e')
            draw.text((direction*CELL+10,95+row*ROW+8),DIRS[direction]+' '+action+f" PPU{page['pixelsPerUnit']:.3f}",font=font,fill='white')
            records.append(dict(direction=DIRS[direction],action=action,id=frame_id,pixelsPerUnit=page['pixelsPerUnit'],
                worldVisualScale=scale,displayPixelsPerWorldUnit=DISPLAY_PPU,wholeFigureDisplayFactor=factor,
                sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256']))
    panel.convert('RGB').save(STAGE/'world-scale-review.png')
    (STAGE/'world-scale-review.json').write_text(json.dumps(dict(records=records,perCellAutofit=False,
        sourceArtEdited=False,worldVisualScale=scale,displayPixelsPerWorldUnit=DISPLAY_PPU),indent=2),encoding='utf-8')
    print(json.dumps(dict(figures=len(records),worldVisualScale=scale,panel='world-scale-review.png')))


if __name__=='__main__':
    main()
