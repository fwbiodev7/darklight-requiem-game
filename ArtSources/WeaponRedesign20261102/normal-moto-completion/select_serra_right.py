from pathlib import Path
import sys,json,hashlib
from PIL import Image
sys.dont_write_bytecode=True
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261102/normal-moto-completion';O=R/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'));import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'preparation-extracts';p.OUT=p.RES;n='ReqMotoNormalSerraEntradaNE_E_SE20261102V1'
info=json.loads((O/(n+'-components.json')).read_text());anchors=[[188,359],[552,359],[913,358],[1275,359],[189,711],[552,710],[913,711],[1275,711],[188,1056],[552,1056],[913,1058]]
guides={str(i):dict(anchor=a,method='Native ground midpoint of two planted boots',uncertaintyPixels=10) for i,a in enumerate(anchors)}
m,_=p.prepare(dict(name=n+'GoodWholeExtract',source=str(A/(n+'-source.png')),preservedSourceName=n+'-source.png',skin=0,weapon=3,form=0,columns=4,sourceRows=3,minimumArea=5000,supportAnchors=guides,rows=[dict(sourceRow=0,sourceIndices=list(range(11)),direction=5,action='Attack',attackKey='Serra de Entrada')]))
im=Image.open(p.RES/(n+'GoodWholeExtract.png')).convert('RGBA');selected=[];provenance=[]
for j in list(range(11))+[8]:
 f=m['frames'][j];x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));b=q.getbbox();q=q.crop(b);pivot=[round(f['pivot'][0]*w-b[0]),round((1-f['pivot'][1])*h-b[1])];selected.append((q,pivot));provenance.append(dict(sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceFrameIndex=f['sourceFrameIndex'],sourceRect=f['sourceRect'],sourceAnchor=f['sourceAnchor'],figureRGBAHash=hashlib.sha256(q.tobytes()).hexdigest(),intentionalGuardReuse=j==8 and len(provenance)==11))
l=max(v[1][0] for v in selected)+16;t=max(v[1][1] for v in selected)+16;r=max(v[0].width-v[1][0] for v in selected)+16;b=max(v[0].height-v[1][1] for v in selected)+16;cw,ch=l+r,t+b;board=Image.new('RGBA',(cw*4,ch*3));support={}
for i,(q,a) in enumerate(selected):
 x=i%4*cw+l-a[0];y=i//4*ch+t-a[1];board.paste(q,(x,y));assert board.crop((x,y,x+q.width,y+q.height)).tobytes()==q.tobytes();support[str(i)]=dict(anchor=[i%4*cw+l,i//4*ch+t],method='Retained measured native support',uncertaintyPixels=10)
name='ReqMotoNormalSerraEntradaNE_E_SE20261102Selected';board.save(A/(name+'-source.png'));(A/(name+'-whole-provenance.json')).write_text(json.dumps(dict(figures=provenance,uniqueFigures=11,authoredFrames=12,excludedClippedFigure=11,recoveryUsesOriginalSourceIndex=8,rgbaExact=True),ensure_ascii=False,indent=2))
cfg=dict(name=name,source=name+'-source.png',directions=['NE','E','SE'],columns=4,action='Attack',attackKey='Serra de Entrada',presentation='OnFoot',durationMs=[125,125,240,370],phaseNames=['early_windup','late_windup','active_cut','recovery'],startupFrames=2,activeFrames=1,runtimeTimingsMs=dict(startup=250,active=240,recovery=370),bodyMeasurements=[dict(sourceFrameIndex=3,bodyHeightPixels=325)],framePixelsPerUnit=[325/1.98]*4+[314/1.98]*8,supportAnchors=support,sourceFigureProvenance=provenance,visualReviewed=False,wholeSourceSeen=True,nearestWholeFiguresSeen=False,staticContactSeen=False)
(A/'config-SerraEntrada-NE-E-SE-selected.json').write_text(json.dumps(cfg,ensure_ascii=False,indent=2),encoding='utf-8')
print('11 whole figures/12 phases preserved; SE recovery is explicit whole guard reuse.')

