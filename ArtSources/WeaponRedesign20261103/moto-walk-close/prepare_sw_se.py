from pathlib import Path
import sys,json,hashlib
from PIL import Image
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261103/moto-walk-close';O=R/'outputs/retomada-20261103/moto-walk-close';sys.path.insert(0,str(R/'Tools/AnimationPreview'));import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'reference-extracts';p.OUT=O/'reference-extracts';qs=[];ps=[]
s=R/'ArtSources/WeaponRedesign20261102/normal-fists-completion/ReqFistsNormalWalkSW20261102V13Complete-source.png'
m,_=p.prepare(dict(name='ReferenceSW',source=str(s),preservedSourceName='ReqFistsNormalWalkSW20261102V13Complete-reference.png',skin=0,weapon=2,form=0,columns=2,sourceRows=2,minimumArea=5000,rows=[dict(sourceRow=0,sourceIndices=[0,1,2,3],direction=1,action='Walk',durationMs=[120]*4,loop=True)]))
for j,d in [(p.RES/'ReferenceSW.json','SW'),(R/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalWalkNE_SE20261102FeetFixedV3.json','SE')]:
 m=json.loads(j.read_text(encoding='utf-8-sig'));im=Image.open(j.with_suffix('.png')).convert('RGBA')
 for f in m['frames']:
  if f['direction']!=d:continue
  x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));q=q.crop(q.getbbox());qs.append(q);ps.append(dict(direction=d,manifest=j.relative_to(R).as_posix(),frame=f['id'],figureRGBAHash=hashlib.sha256(q.tobytes()).hexdigest()))
assert len(qs)==8
cw=max(q.width for q in qs)+40;ch=max(q.height for q in qs)+40;board=Image.new('RGBA',(cw*4,ch*2))
for i,q in enumerate(qs):board.paste(q,((i%4)*cw+(cw-q.width)//2,(i//4)*ch+ch-q.height-20))
board.save(A/'SW-SE-whole-pose-reference.png');p.write(A/'SW-SE-whole-pose-reference.json',dict(wholeFiguresOnly=True,resampled=False,figures=ps));print(json.dumps(dict(size=board.size,figures=len(qs))))
