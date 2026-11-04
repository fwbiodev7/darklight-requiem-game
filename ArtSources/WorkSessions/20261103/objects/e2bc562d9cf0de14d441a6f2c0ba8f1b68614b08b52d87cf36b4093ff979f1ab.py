from pathlib import Path
from PIL import Image,ImageDraw
import json
ROOT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba');REPO=ROOT/'outputs/darklight-requiem-game';RES=REPO/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023';OUT=ROOT/'outputs/fechamento-fabius-20261103/moto-qa/katana-walk-n-repair';OUT.mkdir(exist_ok=True)
names=['ReqKatanaNormalWalkNW_N20261027Selected','ReqKatanaNormalNeutralIdleNW_N_NE20261025']
for n in names:
    d=json.loads((RES/(n+'.json')).read_text(encoding='utf-8-sig'));im=Image.open(RES/d['image']).convert('RGBA')
    for i in ([6,7,8,9,10,11] if 'Selected' in n else [4]):
        f=d['frames'][i];x,y,w,h=f['rect'];fr=im.crop((x,y,x+w,y+h));name=('walk-'+str(i-6) if 'Selected' in n else 'idle-N');fr.save(OUT/(name+'-whole.png'));bg=Image.new('RGBA',(w+40,h+25),'#dfe4e9');bg.alpha_composite(fr,(35,20));dr=ImageDraw.Draw(bg)
        for yy in range(0,h,20):dr.line((27,yy+20,w+35,yy+20),fill='#8b9fb8');dr.text((0,yy+14),str(yy),fill='#111111')
        bg.convert('RGB').save(OUT/(name+'-ruler.png'))
