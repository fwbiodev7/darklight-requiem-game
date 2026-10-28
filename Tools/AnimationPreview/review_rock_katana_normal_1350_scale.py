"""Compare canonical Requiem and Rock idle at actual world PPU; display only."""
from pathlib import Path
import json
import sys
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import review_katana_normal_scale_20261025 as review

OUT=ROOT/'outputs/redesign-armas-20261025-retomada1350/katana-normal'
review.STAGE=OUT
CELL,ROW,DISPLAY_PPU=360,390,112


def main(action='Idle'):
    live=json.loads((review.RESOURCE/'catalog.json').read_text(encoding='utf-8'))['pages']
    req={p['direction']:p for p in live if p['skin']==0 and p['weapon']==0 and p['form']==0 and p['action']==action
         and (action=='Idle' or p.get('attackKey')=='Leve1')}
    kind='idle' if action=='Idle' else 'leve1'
    prefix='rock-requiem-world-scale-review' if action=='Idle' else 'rock-requiem-leve1-world-scale-review'
    rock={p['direction']:p for p in json.loads((OUT/('rock-katana-normal-'+kind+'-pages.json')).read_text(encoding='utf-8'))['pages']}
    canvas=Image.new('RGBA',(CELL*8,ROW*2+80),(24,30,37,255))
    draw=ImageDraw.Draw(canvas)
    phase=0 if action=='Idle' else 3
    draw.text((15,10),f'KATANA / NORMAL / {action} - same world display scale (112 px/u), same phase{phase}, actual descriptor PPU',font=review.FONT,fill='white')
    draw.text((15,34),'Top: integrated Requiem. Bottom: new Rock. Clothes only change. Full figures and source pixels preserved.',font=review.SMALL,fill='#c8d1df')
    entries=[]
    for row,(skin,pages) in enumerate([('Requiem',req),('Rock',rock)]):
        for d in range(8):
            page=pages[d]
            manifest,frame,shown,ppu,body=review.selected(page)
            cx=d*CELL+CELL//2
            ground=80+row*ROW+ROW-36
            x=cx-round(frame['pivot'][0]*shown.width)
            y=ground-round((1-frame['pivot'][1])*shown.height)
            canvas.alpha_composite(shown,(x,y))
            for h,color in [(0,'#657888'),(1,'#35424d'),(1.98,'#35424d')]:
                yy=ground-round(h*DISPLAY_PPU)
                draw.line((d*CELL+6,yy,(d+1)*CELL-6,yy),fill=color,width=1)
            draw.line((cx-4,ground,cx+4,ground),fill='#4ed0ce',width=2)
            draw.text((d*CELL+10,80+row*ROW+8),f'{skin} {review.DIRS[d]} {action} / phase{phase}',font=review.FONT,fill='white')
            value='unknown' if body is None else f'{body:.3f}u'
            draw.text((d*CELL+10,80+row*ROW+32),f'PPU {ppu:.3f} | crown-sole {value}',font=review.SMALL,fill='#c8d1df')
            entries.append(dict(skin=skin,direction=review.DIRS[d],frameId=frame['id'],pixelsPerUnit=ppu,
                                estimatedCrownSoleWorld=body,sourcePath=manifest['sourcePath'],
                                sourcePixelsEdited=False,displayPixelsPerWorldUnit=DISPLAY_PPU))
    canvas.convert('RGB').save(OUT/(prefix+'.png'))
    (OUT/(prefix+'.json')).write_text(json.dumps(dict(schemaVersion=1,date='2026-10-25',session='13h50',action=action,
        comparison='same world scale by actual PPU; no per-frame fit-to-cell',displayPixelsPerWorldUnit=DISPLAY_PPU,
        sourcePixelsEdited=False,entries=entries,
        uncertainty='Crown/sole approximately +/-3-8 source pixels; internal body landmarks +/-10-18 pixels. Perspective, posture and clothing obscure precise segment lengths.'),indent=2),encoding='utf-8')
    print(json.dumps(dict(comparedWholeFigures=len(entries),panel=prefix+'.png')))


if __name__=='__main__':
    main()
    main('Attack')
