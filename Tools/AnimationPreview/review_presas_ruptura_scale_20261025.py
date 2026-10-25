"""Read-only whole-figure world-scale contact, never alter game sprites."""
import json
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[2]
RES=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT=ROOT/'outputs/redesign-armas-20261025/presas-ruptura-scale'
OUT.mkdir(parents=True,exist_ok=True)
WORLD_PPU=128
canvas=Image.new('RGB',(2000,1080),'#24242b')
draw=ImageDraw.Draw(canvas)
sets=[('ReqDaggersNormalNeutralPresas1S_SW_W20261025V2',[3,7,11],0,'Normal approved reference'),('ReqDaggersRupturaPresas1S_SW_W20261025V4',[3,7,11],1,'Ruptura V4 short-hair corrective')]
for stem,indices,row,label in sets:
    m=json.loads((RES/(stem+'.json')).read_text(encoding='utf-8'))
    im=Image.open(RES/m['image']).convert('RGBA')
    for col,index in enumerate(indices):
        f=m['frames'][index];x,y,w,h=f['rect'];whole=im.crop((x,y,x+w,y+h))
        k=WORLD_PPU/m['pixelsPerUnit'];size=(round(w*k),round(h*k))
        whole=whole.resize(size,Image.Resampling.NEAREST)
        ax=f['pivot'][0]*size[0];ay=(1-f['pivot'][1])*size[1]
        px=col*500+250-round(ax);py=row*540+460-round(ay)
        canvas.paste(whole,(px,py),whole)
        draw.text((col*500+20,row*540+20),f'{label}: {f["direction"]} | PPU {m["pixelsPerUnit"]:.4f}',fill='white')
        draw.line((col*500+30,row*540+460,col*500+470,row*540+460),fill='#697986')
m=json.loads((RES/'ReqKatanaNormalNeutralLight1S_Wv1.json').read_text(encoding='utf-8'))
im=Image.open(RES/m['image']).convert('RGBA')
for row,index in enumerate((3,7)):
    f=m['frames'][index];x,y,w,h=f['rect'];whole=im.crop((x,y,x+w,y+h));k=WORLD_PPU/m['pixelsPerUnit']
    size=(round(w*k),round(h*k));whole=whole.resize(size,Image.Resampling.NEAREST)
    ax=f['pivot'][0]*size[0];ay=(1-f['pivot'][1])*size[1]
    canvas.paste(whole,(1750-round(ax),row*540+460-round(ay)),whole)
    draw.text((1520,row*540+20),f'Canonical katana: {f["direction"]} | PPU {m["pixelsPerUnit"]:.4f}',fill='white')
    draw.line((1530,row*540+460,1970,row*540+460),fill='#697986')
canvas.save(OUT/'world-scale-contact.png')
print(json.dumps(dict(contact=str(OUT/'world-scale-contact.png'),worldPixelsPerUnit=WORLD_PPU,autoFit=False,anatomyChanges=False)))
