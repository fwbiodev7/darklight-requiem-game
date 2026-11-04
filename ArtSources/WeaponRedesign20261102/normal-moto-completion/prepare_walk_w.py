from pathlib import Path
import sys,json,hashlib
from collections import Counter
from PIL import Image,ImageDraw
sys.dont_write_bytecode=True
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261102/normal-moto-completion';O=R/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'preparation-extracts';p.OUT=O/'preparation-extracts'
def sha(q):return hashlib.sha256(q.read_bytes()).hexdigest()
specs=[('ReqMotoNormalWalkW20261102V1',[0,1]),('ReqMotoNormalWalkWPoseV8Primary20261102V4',[0]),('ReqMotoNormalWalkE_W20261102V3FistsEdit',[7])]
selected=[];provenance=[]
for n,idx in specs:
 m,_=p.prepare(dict(name=n+'WWholeExtract',source=str(A/(n+'-source.png')),preservedSourceName=n+'-source.png',skin=0,weapon=3,form=0,columns=4,sourceRows=2,minimumArea=5000,rows=[dict(sourceRow=0,sourceIndices=idx,direction=2,action='Walk',loop=True,durationMs=[120]*len(idx))]))
 im=Image.open(p.RES/(n+'WWholeExtract.png')).convert('RGBA')
 for f in m['frames']:
  x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));box=q.getbbox();q=q.crop(box);pivot=[f['pivot'][0]*w-box[0],(1-f['pivot'][1])*h-box[1]]
  selected.append((q,pivot))
  provenance.append(dict(sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],sourceFrameIndex=f['sourceFrameIndex'],sourceRect=f['sourceRect'],sourceAnchor=f['sourceAnchor'],figureRGBAHash=hashlib.sha256(q.tobytes()).hexdigest(),visiblePixels=sum(a>0 for a in q.getchannel('A').getdata())))
left=max(round(v[1][0]) for v in selected)+16;top=max(round(v[1][1]) for v in selected)+16
right=max(v[0].width-round(v[1][0]) for v in selected)+16;bottom=max(v[0].height-round(v[1][1]) for v in selected)+16
cw,ch=left+right,top+bottom;board=Image.new('RGBA',(cw*2,ch*2));anchors={}
for i,(q,pivot) in enumerate(selected):
 dx=(i%2)*cw+left-round(pivot[0]);dy=(i//2)*ch+top-round(pivot[1]);board.paste(q,(dx,dy))
 assert board.crop((dx,dy,dx+q.width,dy+q.height)).tobytes()==q.tobytes()
 anchors[str(i)]=dict(anchor=[(i%2)*cw+left,(i//2)*ch+top],method='Original whole-figure planted support anchor retained; source figures not resized',uncertaintyPixels=6)
n='ReqMotoNormalWalkW20261102V4Complete';src=A/(n+'-source.png');board.save(src)
p.write(A/(n+'-whole-figure-provenance.json'),dict(parts=provenance,rgbaExact=True,partAssembly=False,fullFiguresOnly=True,sourceResampled=False))
p.RES=O/'staging';p.OUT=O
m,pages=p.prepare(dict(name=n,source=str(src),preservedSourceName=src.name,skin=0,weapon=3,form=0,columns=2,sourceRows=2,minimumArea=5000,supportAnchors=anchors,rows=[dict(sourceRow=0,sourceIndices=[0,1,2,3],direction=2,action='Walk',loop=True,durationMs=[120]*4)]))
bodyHeights=[381,381,390,387]
ppu=[v/1.98 for v in bodyHeights]
m.update(status='candidate',presentation='OnFoot',motorcycleContext='onFoot-DualSaws',worldVisualScale=1.325,pixelsPerUnit=ppu[0],framePixelsPerUnit=ppu,pixelsPerUnitCalibration=dict(method='Per-source crown-to-sole body density; no equipment extents, no resampling',bodyHeightUnits=1.98,bodyHeightPixels=bodyHeights,uncertaintyPixels=5),sourceFigureProvenance=provenance,review=dict(wholeSourceSeen=True,nearestWholeFiguresSeen=True,staticContactSeen=True,continuousMotionViewed=False,imported=False,runtimeVerified=False,humanApproved=False),phases=['contact_A','passing_A_low','contact_B_opposite','passing_B_low'])
for i,f in enumerate(m['frames']):f.update(phase=m['phases'][i],pixelsPerUnit=ppu[i],origin=provenance[i])
for page in pages:page.update(status='candidate',presentation='OnFoot',pixelsPerUnit=ppu[0],framePixelsPerUnit=ppu,durationMs=[120]*4,loop=True)
p.write(p.RES/(n+'.json'),m);p.write(O/(n+'-pages.json'),dict(schemaVersion=1,complete=False,pages=pages))
im=Image.open(p.RES/(n+'.png')).convert('RGBA');cols=Counter(c for c in im.getdata() if c[3]>0)
p.write(O/(n+'-palette.json'),dict(imageSha256=sha(p.RES/(n+'.png')),uniqueColorCount=len(cols),paletteQuantized=False,top256=[dict(rgba=list(c),count=v) for c,v in cols.most_common(256)]))
# Preview only: never written back to source/atlas.
for density,label in [(1,'native'),(1.325*120,'worldscale')]:
 figs=[]
 for i,(q,pivot) in enumerate(selected):
  s=1 if density==1 else density/ppu[i];qq=q if s==1 else q.resize((round(q.width*s),round(q.height*s)),Image.Resampling.NEAREST)
  figs.append(qq)
 cellw=max(q.width for q in figs)+24;cellh=max(q.height for q in figs)+42
 contact=Image.new('RGBA',(cellw*4,cellh),(155,155,155,255));d=ImageDraw.Draw(contact)
 for i,q in enumerate(figs):
  contact.alpha_composite(q,(i*cellw+(cellw-q.width)//2,cellh-q.height-8));d.text((i*cellw+8,6),m['phases'][i],fill='black')
 contact.save(O/(n+'-'+label+'-contact.png'))
receipt=dict(name=n,status='candidate',completeActionEightDirections=False,directions=['W'],figures=4,clips=1,phaseDurationMs=[120]*4,loop=True,presentation='OnFoot',sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],nativeSize=m['nativeSize'],imageSha256=m['imageSha256'],manifestSha256=sha(p.RES/(n+'.json')),pagesSha256=sha(O/(n+'-pages.json')),framePixelsPerUnit=ppu,rgbaExact=True,automatedTestsRun=False,continuousMotionViewed=False,imported=False,runtimeVerified=False,pendingWalkDirections=['S','SW','NW','N','NE','E','SE'])
p.write(O/(n+'-receipt.json'),receipt)
print(json.dumps(receipt))

