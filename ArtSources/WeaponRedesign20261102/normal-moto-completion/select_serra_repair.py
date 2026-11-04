from pathlib import Path
import sys,json,hashlib
from PIL import Image
sys.dont_write_bytecode=True
R=Path.cwd();A=R/'ArtSources/WeaponRedesign20261102/normal-moto-completion';O=R/'outputs/retomada-20261102/normal-moto-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'));import prepare_weapon_redesign as p
p.ART=A;p.RES=O/'preparation-extracts';p.OUT=p.RES
specs=[('ReqMotoNormalSerraEntradaS_SW_W20261102V1',[8,9],[[175,1053],[568,1053]]),('ReqMotoNormalSerraEntradaW_NW_N20261102V2',list(range(2,12)),[[901,349],[1283,347],[170,704],[530,702],[902,694],[1285,708],[185,1052],[529,1052],[905,1052],[1278,1055]])]
selected=[];prov=[]
for n,idx,anchors in specs:
 guides={str(j):dict(anchor=anchors[k],method='Measured midpoint of two grounded boots on native whole source',uncertaintyPixels=8) for k,j in enumerate(idx)}
 m,_=p.prepare(dict(name=n+'SelectedExtract',source=str(A/(n+'-source.png')),preservedSourceName=n+'-source.png',skin=0,weapon=3,form=0,columns=4,sourceRows=3,minimumArea=5000,supportAnchors=guides,rows=[dict(sourceRow=0,sourceIndices=idx,direction=2,action='Attack',attackKey='Serra de Entrada')]))
 im=Image.open(p.RES/(n+'SelectedExtract.png')).convert('RGBA')
 for f in m['frames']:
  x,y,w,h=f['rect'];q=im.crop((x,y,x+w,y+h));b=q.getbbox();q=q.crop(b);pivot=[round(f['pivot'][0]*w-b[0]),round((1-f['pivot'][1])*h-b[1])]
  selected.append((q,pivot));prov.append(dict(sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],sourceSize=m['sourceSize'],sourceFrameIndex=f['sourceFrameIndex'],sourceRect=f['sourceRect'],sourceAnchor=f['sourceAnchor'],figureRGBAHash=hashlib.sha256(q.tobytes()).hexdigest()))
l=max(v[1][0] for v in selected)+16;t=max(v[1][1] for v in selected)+16;r=max(v[0].width-v[1][0] for v in selected)+16;b=max(v[0].height-v[1][1] for v in selected)+16;cw,ch=l+r,t+b
board=Image.new('RGBA',(cw*4,ch*3));guides={}
for i,(q,anchor) in enumerate(selected):
 x=(i%4)*cw+l-anchor[0];y=(i//4)*ch+t-anchor[1];board.paste(q,(x,y));assert board.crop((x,y,x+q.width,y+q.height)).tobytes()==q.tobytes()
 guides[str(i)]=dict(anchor=[i%4*cw+l,i//4*ch+t],method='Retained measured source support midpoint',uncertaintyPixels=8)
n='ReqMotoNormalSerraEntradaW_NW_N20261102Selected';board.save(A/(n+'-source.png'))
(A/(n+'-whole-provenance.json')).write_text(json.dumps(dict(figures=prov,rgbaExact=True,fullFiguresOnly=True,noSourceResampling=True,retainedOriginalWFrames=[8,9],repairedOnlyOriginalWFrames=[10,11]),ensure_ascii=False,indent=2),encoding='utf-8')
cfg=dict(name=n,source=n+'-source.png',sourceRows=3,directions=['W','NW','N'],columns=4,action='Attack',attackKey='Serra de Entrada',presentation='OnFoot',durationMs=[125,125,240,370],phaseNames=['early_windup','late_windup','active_cut','recovery'],startupFrames=2,activeFrames=1,runtimeTimingsMs=dict(startup=250,active=240,recovery=370),bodyMeasurements=[dict(source='original W standing',bodyHeightPixels=309)],framePixelsPerUnit=[309/1.98]*2+[302/1.98]*2+[301/1.98]*4+[306/1.98]*4,supportAnchors=guides,visualReviewed=False,wholeSourceSeen=True,nearestWholeFiguresSeen=False,staticContactSeen=False,sourceFigureProvenance=prov)
(A/'config-SerraEntrada-W-NW-N.json').write_text(json.dumps(cfg,ensure_ascii=False,indent=2),encoding='utf-8')
q=A/'prepare_action.py';s=q.read_text();s=s.replace("for i,f in enumerate(m['frames']):\n  k=i%cfg.get('columns',4);f['durationMs']=cfg['durationMs'][k];f['phase']=cfg['phaseNames'][k]","if cfg.get('framePixelsPerUnit'):m['framePixelsPerUnit']=cfg['framePixelsPerUnit']\n if cfg.get('sourceFigureProvenance'):m['sourceFigureProvenance']=cfg['sourceFigureProvenance']\n for i,f in enumerate(m['frames']):\n  k=i%cfg.get('columns',4);f['durationMs']=cfg['durationMs'][k];f['phase']=cfg['phaseNames'][k]\n  if cfg.get('framePixelsPerUnit'):f['pixelsPerUnit']=cfg['framePixelsPerUnit'][i]")
s=s.replace("for page in pages:\n  page.update", "for pageIndex,page in enumerate(pages):\n  if cfg.get('framePixelsPerUnit'):page['framePixelsPerUnit']=cfg['framePixelsPerUnit'][pageIndex*cfg.get('columns',4):(pageIndex+1)*cfg.get('columns',4)]\n  page.update")
q.write_text(s,encoding='utf-8');print('12 selected full figures consolidated exactly; original W startup pair retained.')

