from pathlib import Path
from PIL import Image
import json
ROOT=Path.cwd();RES=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023';OUT=ROOT/'ArtSources/WeaponRedesign20261102/normal-moto-completion'
p=RES/'ReqFistsNormalWalkNE_SE20261102FeetFixedV3.json';m=json.loads(p.read_text());im=Image.open(p.with_suffix('.png')).convert('RGBA');fig=[]
for f in m['frames']:
 if f['direction']!='SE':continue
 x,y,w,h=f['rect'];whole=im.crop((x,y,x+w,y+h));b=whole.getbbox();fig.append(whole.crop(b))
w=max(v.width for v in fig)+20;h=max(v.height for v in fig)+20;board=Image.new('RGBA',(w*len(fig),h))
for i,v in enumerate(fig):board.alpha_composite(v,(i*w+(w-v.width)//2,h-v.height-10))
p=OUT/'approved-fists-SE-whole-figures-reference.png';board.save(p);print(json.dumps(dict(path=str(p),size=board.size,wholeFigures=len(fig),drawingResized=False)))

