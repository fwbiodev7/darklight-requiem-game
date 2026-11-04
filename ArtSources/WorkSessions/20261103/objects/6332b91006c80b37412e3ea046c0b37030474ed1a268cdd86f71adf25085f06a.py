"""Whole-figure previews at one world scale; never edits source assets."""
from pathlib import Path
import json
from PIL import Image,ImageDraw,ImageFont
B=Path(__file__).resolve().parents[1];bank=B/'outputs/darklight-requiem-game/Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
out=B/'outputs/fechamento-fabius-20261103/animation-review-normal'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
pages=read(bank/'catalog.json')['pages'];font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',13)
cols=[('Idle',0),('Walk',0),('Parry',0),('Hurt',0),('Hurt',1),('Dead',0),('Attack',0)]
im=Image.new('RGB',(1360,1520),(34,39,45));dr=ImageDraw.Draw(im)
dr.text((8,5),'Fabius Normal: mesmo registro/escala do mundo; nao normalizado pela altura do bbox. NAO e gameplay.',font=font,fill='white')
for row,(weapon,direction) in enumerate((w,d) for d in (0,2) for w in (0,2,3)):
    y=50+row*240;dr.text((4,y),f'{ {0:"Katana",2:"Punhos",3:"Moto"}[weapon]} {"S" if direction==0 else "W"}',font=font,fill='white')
    for col,(action,fi) in enumerate(cols):
        x=100+col*180;baseline=y+190;dr.line((x,baseline,x+178,baseline),fill='#6c8b9d')
        candidates=[p for p in pages if p['skin']==0 and p['form']==0 and p['weapon']==weapon and p['direction']==direction and p['action']==action]
        if not candidates:
            dr.text((x,y+200),f'{action}: separado/ausente',font=font,fill='#d8bca0');continue
        p=candidates[0];d=read(bank/(p['resourcePath'].split('/')[-1]+'.json'));f=next(f for f in d['frames'] if f['id']==p['spriteNames'][fi])
        a,b,w,h=f['rect'];source=Image.open(bank/d['image']).convert('RGBA').crop((a,b,a+w,b+h));bbox=source.getchannel('A').point(lambda a:255 if a>16 else 0).getbbox()
        if not bbox:continue
        ppu=(p.get('framePixelsPerUnit') or [p['pixelsPerUnit']]*len(p['spriteNames']))[fi];ratio=72*1.325/ppu
        cut=source.crop(bbox);cut=cut.resize((round(cut.width*ratio),round(cut.height*ratio)),Image.Resampling.NEAREST)
        pivotx=(f['pivot'][0]*w-bbox[0])*ratio;pivoty=((1-f['pivot'][1])*h-bbox[1])*ratio
        im.paste(cut,(round(x+80-pivotx),round(baseline-pivoty)),cut)
        dr.text((x,y+200),f'{action} {fi} | PPU {ppu:.1f}',font=font,fill='white')
        dr.text((x,y+216),'pixels fonte intactos',font=font,fill='#afbdc6')
im.save(out/'cross-action-S-W.png')
print(out/'cross-action-S-W.png')
