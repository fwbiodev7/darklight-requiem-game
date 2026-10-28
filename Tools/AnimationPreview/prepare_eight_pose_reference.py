"""Contact of eight COMPLETE existing poses as a generation reference, no body-part edits."""
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/padronizacao-artes-20261020'
meta=ROOT/'outputs/olhos-todas-formas-20261020/metadata/Darklight__TransformedYoung__RequiemCombatSupportDaggersYoung_Violet.json'
d=json.loads(meta.read_text());source=Path(d['image']);raw=Image.open(source).convert('RGBA')
owner=np.array(Image.open(source.with_name(source.stem+'_Owner.png')).convert('RGBA'))[:,:,3]
cell,ground=320,288
contact=Image.new('RGBA',(cell*4,cell*2))
for i,f in enumerate(d['frames'][:8]):
    x,y,w,h=f['rect'];a=np.array(raw)[y:y+h,x:x+w].copy();a[owner[y:y+h,x:x+w]!=i+1,3]=0
    tile=Image.fromarray(a)
    # Exact source scale, align its existing full-figure pivot. No missing pixels.
    dx=round(cell*.5-f['pivot'][0]*w);dy=round(ground-(1-f['pivot'][1])*h)
    if dx<0 or dy<0 or dx+w>cell or dy+h>cell:raise ValueError('Needs a larger shared canvas')
    contact.paste(tile,(i%4*cell+dx,i//4*cell+dy))
contact.save(OUT/'adagas-8-poses-referencia-inteira.png')
print(json.dumps(dict(source=d['image'],frames=8,sourceUnchanged=True,headBodyWeaponsTogether=True)))
