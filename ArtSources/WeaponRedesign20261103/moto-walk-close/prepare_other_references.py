from pathlib import Path
import json,sys,hashlib
from PIL import Image
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261103/moto-walk-close';B=R/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
plans={'NW_N_NE':[('NW','ReqFistsNormalWalkNW20261102NaturalV2Selected'),('N','ReqFistsNormalWalkN20261102NaturalV1'),('NE','ReqFistsNormalWalkNE_SE20261102FeetFixedV3')],'S_SW_SE':[('S','ReqFistsNormalWalkS20261101V2Selected'),('SW','ReqKatanaNormalWalkSW_W20261026ApprovedV1'),('SE','ReqFistsNormalWalkNE_SE20261102FeetFixedV3')]}
for tag,specs in plans.items():
 figs=[];prov=[]
 for d,n in specs:
  j=B/(n+'.json');m=json.loads(j.read_text(encoding='utf-8-sig'));im=Image.open(j.with_suffix('.png')).convert('RGBA');fs=[f for f in m['frames'] if f['direction']==d]
  if len(fs)==6: fs=[fs[k] for k in [0,1,3,4]]
  assert len(fs)==4,(n,d,len(fs))
  for f in fs:
   x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));q=q.crop(q.getbbox());figs.append(q);prov.append(dict(direction=d,manifest=j.relative_to(R).as_posix(),manifestSha256=hashlib.sha256(j.read_bytes()).hexdigest(),frame=f['id'],framePixelsPerUnit=f.get('pixelsPerUnit',m['pixelsPerUnit']),rgbaHash=hashlib.sha256(q.tobytes()).hexdigest()))
 cw=max(q.width for q in figs)+50;ch=max(q.height for q in figs)+45;board=Image.new('RGBA',(cw*4,ch*3))
 for i,q in enumerate(figs):board.paste(q,((i%4)*cw+(cw-q.width)//2,(i//4)*ch+ch-q.height-20))
 dest=A/(tag+'-whole-pose-reference.png');board.save(dest);dest.with_suffix('.json').write_text(json.dumps(dict(wholeFiguresOnly=True,resampled=False,figures=prov),indent=2),encoding='utf-8');print(tag,board.size)

