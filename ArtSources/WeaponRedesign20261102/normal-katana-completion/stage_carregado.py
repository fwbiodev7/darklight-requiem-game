from pathlib import Path
import sys,json,statistics,hashlib,copy,subprocess
from collections import Counter
from PIL import Image
import numpy as np
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion';O=R/'outputs/retomada-20261102/normal-katana-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'));import prepare_weapon_redesign as pipe
pipe.ART=S;pipe.RES=O/'staged';pipe.OUT=O/'qa';D=[150,140,115,115]
specs=[
 ('S_SW','V2',[0,1],[[190,466],[576,466],[994,470],[1390,466],[181,942],[578,944],[997,954],[1395,954]],[(3,142,467),(7,614,955)]),
 ('W_NW','V2',[2,3],[[191,478],[580,478],[998,476],[1401,485],[192,951],[579,944],[995,955],[1399,955]],[(3,139,486),(7,610,956)]),
 ('N_NE','V3',[4,5],[[176,463],[537,462],[910,464],[1318,464],[196,947],[558,946],[934,949],[1327,950]],[(3,111,465),(7,596,951)]),
 ('E_SE','V1',[6,7],[[211,431],[679,435],[1111,435],[1577,435],[216,858],[685,861],[1113,861],[1556,861]],[(3,84,436),(7,518,862)])]
def write(p,d):pipe.write(p,d)
def compact(m,name):
 original=Image.open(pipe.RES/(name+'.png')).convert('RGBA');blocks=[]
 for i,f in enumerate(m['frames']):
  x,y,w,h=f['rect'];im=original.crop((x,y,x+w,y+h));a=np.array(im);yy,xx=np.where(np.any(a!=0,axis=2));b=(max(0,int(xx.min())-2),max(0,int(yy.min())-2),min(w,int(xx.max())+3),min(h,int(yy.max())+3));blocks.append((i,im,b,im.crop(b)))
 opts=[]
 for width in [1024,1280,1536,1792,2048,2304,2560,3072,3584,4096]:
  x=y=rh=0;pos={};valid=True
  for i,im,b,t in sorted(blocks,key=lambda v:v[3].height,reverse=True):
   if t.width>width:valid=False;break
   if x+t.width+4>width:x=0;y+=rh+4;rh=0
   pos[i]=(x,y);x+=t.width+4;rh=max(rh,t.height)
  if valid and y+rh<4096:opts.append((width*(y+rh),width,y+rh,pos))
 _,w,h,pos=min(opts,key=lambda v:v[0]);atlas=Image.new('RGBA',(w,h));proof=[]
 for i,im,b,t in blocks:
  x,y=pos[i];atlas.paste(t,(x,y));f=m['frames'][i];oldw,oldh=im.size;pv=f['pivot'];f.update(rect=[x,y,*t.size],pivot=[(pv[0]*oldw-b[0])/t.width,1-((1-pv[1])*oldh-b[1])/t.height]);f['sourceOffsetInFrame']=[f['sourceOffsetInFrame'][0]-b[0],f['sourceOffsetInFrame'][1]-b[1]]
  check=Image.new('RGBA',im.size);check.paste(atlas.crop((x,y,x+t.width,y+t.height)),(b[0],b[1]));assert check.tobytes()==im.tobytes();assert all(0<=z<=1 for z in f['pivot']);proof.append({'frameId':f['id'],'exactRgba':True,'originalRgbaSha256':hashlib.sha256(im.tobytes()).hexdigest(),'trimRgba0000':list(b)})
 atlas.save(pipe.RES/(name+'.png'));m.update(imageSha256=pipe.sha(pipe.RES/(name+'.png')),nativeSize=list(atlas.size),packing='compact variable whole-frame rectangles; RGBA0000-only trims',compactionProof=proof)
 palette=Counter(tuple(z) for z in atlas.getdata() if z[3]);write(O/'qa'/name/'palette.json',{'imageSha256':m['imageSha256'],'uniqueRgbaCount':len(palette),'top256':[{'rgba':list(c),'count':n} for c,n in palette.most_common(256)]})
 return m
allpages=[];summaries=[];jobs=[]
for pair,v,dirs,anchors,body in specs:
 name='ReqKatanaNormalCarregado'+pair+'20261102'+v;source=S/(name+'-source.png');ppu=statistics.median(z-y for _,y,z in body)/1.98
 job={'name':name,'source':str(source),'preservedSourceName':source.name,'skin':0,'weapon':0,'form':0,'columns':4,'sourceRows':2,'minimumArea':10000,'rows':[{'sourceRow':row,'sourceIndices':list(range(row*4,row*4+4)),'direction':d,'action':'Attack','attackKey':'Carregado','loop':False,'durationMs':D,'startupFrames':1,'activeFrames':1} for row,d in enumerate(dirs)],'supportAnchors':{str(i):{'anchor':a,'method':'Manual midpoint of projected complete-figure boot support; original drawing unchanged'} for i,a in enumerate(anchors)}}
 try:m,pages=pipe.prepare(job)
 except Exception as e:
  write(O/'stage-failure.json',{'name':name,'error':str(e),'status':'blocked-candidate','noAssetsTouched':True});raise
 m.update(status='candidate',visualReviewed=False,imported=False,runtimeVerified=False,worldVisualScale=1.325,pixelsPerUnit=ppu,startupFrames=1,activeFrames=1,durationMs=D,attackKey='Carregado',stagingOnly=True,gameplayContactVerified=False,proportionCorrection='Whole figures redrawn with approved compact thighs/shins/boots; no scaling or anatomical assembly',ppuProvenance={'purpose':'Native raster density only; proportions reviewed before calibration','targetBodyHeightUnits':1.98,'measurements':[{'frame':i,'hairY':y,'soleY':z,'bodyHeight':z-y} for i,y,z in body]})
 for i,f in enumerate(m['frames']):f.update(durationMs=D[i%4],phase=['anticipation','active-contact','follow-through','recovery'][i%4],gameplayPhase=['startup','active','recovery','recovery'][i%4],attackKey='Carregado')
 m=compact(m,name);write(pipe.RES/(name+'.json'),m)
 for p in pages:p.update(status='candidate',visualReviewed=False,imported=False,runtimeVerified=False,stagingOnly=True,pixelsPerUnit=ppu,nativeSize=m['nativeSize'],imageSha256=m['imageSha256'],startupFrames=1,activeFrames=1,durationMs=D,sourceSpriteNames=[])
 write(pipe.RES/(name+'.pages.json'),{'schemaVersion':1,'stagingOnly':True,'pages':pages});allpages.extend(pages);jobs.append(job)
 checks={}
 for mode in ['audit','preview']:
  out=O/'qa'/name/mode
  result=subprocess.run([sys.executable,str(pipe.PIPE),mode,'--manifest',str(pipe.RES/(name+'.json')),'--out',str(out)],capture_output=True,text=True,encoding='utf-8',errors='replace');(O/'qa'/name/(mode+'.log')).write_text(result.stdout+result.stderr,encoding='utf-8');checks[mode]=result.returncode
  if result.returncode:raise RuntimeError(name+' '+mode+' failed; inspect saved log')
 report=json.loads((O/'qa'/name/'audit/report.json').read_text());summaries.append({'name':name,'figures':8,'directions':dirs,'sourceSha256':m['sourceSha256'],'imageSha256':m['imageSha256'],'atlas':m['nativeSize'],'pixelsPerUnit':ppu,'auditErrors':report['errors'],'auditWarningCount':len(report['warnings']),'checks':checks})
write(O/'carregado-pages-fragment.json',{'schemaVersion':1,'status':'candidate','stagingOnly':True,'pages':allpages})
write(S/'import-jobs-carregado.json',{'schemaVersion':1,'stagingOnly':True,'jobs':jobs})
write(O/'carregado-checkpoint.json',{'date':'2026-11-02','action':'Carregado','status':'candidate','producedFiguresIncludingRejected':64,'selectedFigures':32,'clips':8,'directions':['S','SW','W','NW','N','NE','E','SE'],'staticVisualReview':'pending contact/native/motion inspection after corrected source selection','imported':False,'runtimeVerified':False,'sourcePixelResampling':False,'approvedSourcesChanged':0,'allPhasesMs':D,'summaries':summaries})
print(json.dumps({'sheets':4,'wholeFigures':32,'clips':len(allpages),'status':'candidate','auditErrors':sum(len(s['auditErrors']) for s in summaries),'warnings':sum(s['auditWarningCount'] for s in summaries)}))
