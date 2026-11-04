from pathlib import Path
import sys, json, hashlib
from PIL import Image
R=Path.cwd(); A=R/'ArtSources/WeaponRedesign20261103/moto-walk-close'; O=R/'outputs/retomada-20261103/moto-walk-close'
sys.dont_write_bytecode=True
sys.path.insert(0,str(R/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'reference-extracts';p.OUT=O/'reference-extracts'
allfig=[];prov=[]
for d,old in [('W','ReqFistsNormalWalkW20261102V9Complete'),('E','ReqFistsNormalWalkE20261102V11Complete')]:
 s=R/'ArtSources/WeaponRedesign20261102/normal-fists-completion'/(old+'-source.png')
 m,_=p.prepare(dict(name='Reference'+d,source=str(s),preservedSourceName=old+'-reference.png',skin=0,weapon=2,form=0,columns=2,sourceRows=2,minimumArea=5000,rows=[dict(sourceRow=0,sourceIndices=[0,1,2,3],direction=['S','SW','W','NW','N','NE','E','SE'].index(d),action='Walk',durationMs=[120]*4,loop=True)]))
 im=Image.open(p.RES/('Reference'+d+'.png')).convert('RGBA')
 for f in m['frames']:
  x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));q=q.crop(q.getbbox());allfig.append(q)
  prov.append(dict(direction=d,sourcePath=s.relative_to(R).as_posix(),sourceSha256=p.sha(s),sourceFrameIndex=f['sourceFrameIndex'],sourceRect=f['sourceRect'],rgbaHash=hashlib.sha256(q.tobytes()).hexdigest()))
cw=max(q.width for q in allfig)+45;ch=max(q.height for q in allfig)+45
board=Image.new('RGBA',(cw*4,ch*2))
for i,q in enumerate(allfig):board.paste(q,((i%4)*cw+(cw-q.width)//2,(i//4)*ch+ch-q.height-20))
board.save(A/'W-E-whole-pose-reference.png')
p.write(A/'W-E-whole-pose-reference-provenance.json',dict(wholeFiguresOnly=True,anatomicalAssembly=False,resampled=False,figures=prov))
print(json.dumps(dict(path=str(A/'W-E-whole-pose-reference.png'),size=board.size)))
