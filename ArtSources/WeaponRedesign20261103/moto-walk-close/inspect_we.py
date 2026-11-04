from pathlib import Path
import sys,json
from PIL import Image
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261103/moto-walk-close';O=R/'outputs/retomada-20261103/moto-walk-close'
sys.path.insert(0,str(R/'Tools/AnimationPreview'));from whole_figure_components import identify
for tag in ['ReqMotoNormalWalkWE20261103V1','ReqMotoNormalWalkWE20261103V2']:
 im=Image.open(A/(tag+'-source.png')).convert('RGBA');fs,_=identify(im,rows=2,columns=4,minimum_area=5000)
 (O/(tag+'-components.json')).write_text(json.dumps(fs,indent=2),encoding='utf-8')
 for i,f in enumerate(fs):
  x0,y0,x1,y1=f['rect'];q=im.crop((x0,y0,x1,y1));bg=Image.new('RGBA',q.size,(170,170,170,255));bg.alpha_composite(q);bg.resize((q.width*2,q.height*2),Image.Resampling.NEAREST).save(O/(tag+f'-whole{i:02}-2x.png'))
 print(json.dumps(dict(name=tag,size=im.size,heights=[f['bodyHeight'] for f in fs],anchors=[f['anchor'] for f in fs])))
