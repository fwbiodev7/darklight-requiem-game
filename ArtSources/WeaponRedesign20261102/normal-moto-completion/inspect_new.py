from pathlib import Path
import sys,json,hashlib
from PIL import Image
ROOT=Path.cwd();ART=ROOT/'ArtSources/WeaponRedesign20261102/normal-moto-completion';OUT=ROOT/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'));from whole_figure_components import identify
names=['ReqMotoNormalWalkWPoseV8Primary20261102V4','ReqMotoNormalWalkNW_N_NE20261102V1','ReqMotoNormalWalkE_SE_SW20261102V1']
for name in names:
 p=ART/(name+'-source.png');im=Image.open(p).convert('RGBA');rows=3 if 'NW_N_NE' in name or 'E_SE_SW' in name else 2;fig,own=identify(im,rows=rows,columns=4,minimum_area=5000);details=[]
 for i,f in enumerate(fig):
  x0,y0,x1,y1=f['rect'];whole=im.crop((x0,y0,x1,y1));bg=Image.new('RGBA',whole.size,(190,190,190,255));bg.alpha_composite(whole);bg.resize((bg.width*2,bg.height*2),Image.Resampling.NEAREST).save(OUT/(name+f'-whole{i:02}-2x.png'));details.append(dict(index=i,rect=[x0,y0,x1-x0,y1-y0],height=f['bodyHeight'],anchor=f['anchor'],size=im.size))
 (OUT/(name+'-components.json')).write_text(json.dumps(details,indent=2),encoding='utf-8');print(json.dumps(dict(name=name,figures=len(fig),size=im.size,heights=[f['bodyHeight'] for f in fig],sha256=hashlib.sha256(p.read_bytes()).hexdigest())))
