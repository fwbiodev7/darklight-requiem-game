from pathlib import Path
import json,sys,statistics,hashlib,copy
from PIL import Image
import numpy as np
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main'); S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion'; O=R/'outputs/retomada-20261102/normal-katana-completion'
sys.path.insert(0,str(R/'Tools/AnimationPreview'));import prepare_weapon_redesign as pipe
pipe.ART=S;pipe.RES=O/'staged';pipe.OUT=O/'qa'
def save(p,d):pipe.write(p,d)
def prepare(spec,key,durations,active):
 name=spec['name']; rows=len(spec['directions']);n=len(durations)
 job={'name':name,'source':str(S/(name+'-source.png')),'preservedSourceName':name+'-source.png','skin':0,'weapon':0,'form':0,'columns':n,'sourceRows':rows,'minimumArea':10000,'rows':[{'sourceRow':i,'sourceIndices':list(range(i*n,i*n+n)),'direction':d,'action':'Attack','attackKey':key,'loop':False,'durationMs':durations,'startupFrames':1,'activeFrames':active} for i,d in enumerate(spec['directions'])]}
 m,pages=pipe.prepare(job)
 atlas=Image.open(pipe.RES/(name+'.png')).convert('RGBA'); blocks=[]
 for f in m['frames']:
  x,y,w,h=f['rect'];im=atlas.crop((x,y,x+w,y+h));f['frameSourcePath']=m['sourcePath'];f['frameSourceSha256']=m['sourceSha256'];blocks.append((im,copy.deepcopy(f)))
 for rep in spec.get('replacements',[]):
  rm,_=prepare({'name':rep['name'],'directions':spec['directions']},key,durations,active)
  ri=Image.open(pipe.RES/(rep['name']+'.png')).convert('RGBA')
  for index in rep['indices']:
   rf=copy.deepcopy(rm['frames'][index]);x,y,w,h=rf['rect'];im=ri.crop((x,y,x+w,y+h));rf['id']=m['frames'][index]['id'];rf['replacesOnlyCompleteFigure']=True;blocks[index]=(im,rf)
   m['frames'][index]=rf
 packed=[]
 for i,(im,f) in enumerate(blocks):
  a=np.array(im);yy,xx=np.where(a[:,:,3]>0);b=(max(0,int(xx.min())-2),max(0,int(yy.min())-2),min(im.width,int(xx.max())+3),min(im.height,int(yy.max())+3));tile=im.crop(b);oldpv=f['pivot'];pivotpx=[oldpv[0]*im.width-b[0],(1-oldpv[1])*im.height-b[1]];packed.append((i,tile,b,pivotpx,im,f))
 opts=[]
 for width in [1024,1280,1536,1792,2048,2304,2560,3072]:
  x=y=rh=0;pos={}
  for i,t,b,p,im,f in sorted(packed,key=lambda t:t[1].height,reverse=True):
   if x+t.width+4>width:x=0;y+=rh+4;rh=0
   pos[i]=(x,y);x+=t.width+4;rh=max(rh,t.height)
  if all(t.width<=width for _,t,*_ in packed):opts.append((width*(y+rh),width,y+rh,pos))
 _,w,h,pos=min(opts,key=lambda t:t[0]);out=Image.new('RGBA',(w,h));proof=[]
 for i,t,b,p,im,f in packed:
  x,y=pos[i];out.paste(t,(x,y));f['rect']=[x,y,*t.size];f['pivot']=[p[0]/t.width,1-p[1]/t.height];f['sourceOffsetInFrame']=[f['sourceOffsetInFrame'][0]-b[0],f['sourceOffsetInFrame'][1]-b[1]]
  check=Image.new('RGBA',im.size);check.paste(t,(b[0],b[1]));a=np.array(im);q=np.array(check);assert np.array_equal(a[a[:,:,3]>0],q[a[:,:,3]>0]);assert np.count_nonzero(a[:,:,3])==np.count_nonzero(q[:,:,3]);assert all(0<=z<=1 for z in f['pivot']);m['frames'][i]=f
  proof.append({'frameId':f['id'],'everyNonzeroAlphaRgbaPreserved':True,'sourcePath':f['frameSourcePath'],'sourceSha256':f['frameSourceSha256']})
 ppu=spec.get('bodyHeightPx',statistics.median((b[1].height for b in packed if b[0]%n==n-1)))/1.98
 out.save(pipe.RES/(name+'.png'))
 m.update(imageSha256=pipe.sha(pipe.RES/(name+'.png')),nativeSize=list(out.size),pixelsPerUnit=ppu,status='candidate',visualReviewed=False,staticVisualReviewed=False,motionVisualReviewed=False,imported=False,runtimeVerified=False,worldVisualScale=1.325,stagingOnly=True,attackKey=key,startupFrames=1,activeFrames=active,durationMs=durations,packing='Whole intact figure rectangles; no resampling or anatomical assembly',frameRgbaProof=proof,ppuProvenance=spec.get('ppuProvenance','Median complete recovery-pose raster height / 1.98; not an anatomy correction'))
 for i,f in enumerate(m['frames']):f.update(durationMs=durations[i%n],attackKey=key,phase=('anticipation' if i%n==0 else 'active-contact' if i%n<=active else 'follow-through' if i%n<n-1 else 'recovery'))
 for p in pages:p.update(status='candidate',visualReviewed=False,staticVisualReviewed=False,motionVisualReviewed=False,imported=False,runtimeVerified=False,stagingOnly=True,nativeSize=m['nativeSize'],imageSha256=m['imageSha256'],pixelsPerUnit=ppu,startupFrames=1,activeFrames=active,durationMs=durations,sourceSpriteNames=[])
 save(pipe.RES/(name+'.json'),m);save(pipe.RES/(name+'.pages.json'),{'schemaVersion':1,'stagingOnly':True,'pages':pages});return m,pages
if __name__=='__main__':
 cfg=json.loads((S/sys.argv[1]).read_text(encoding='utf-8-sig'));pages=[];summary=[]
 for spec in cfg['sources']:
  m,p=prepare(spec,cfg['attackKey'],cfg['durationMs'],cfg.get('activeFrames',1));pages+=p;summary.append({'name':spec['name'],'frames':len(m['frames']),'sourceSha256':m['sourceSha256'],'imageSha256':m['imageSha256'],'pixelsPerUnit':m['pixelsPerUnit']})
 slug=cfg['slug'];save(O/(slug+'-pages-fragment.json'),{'schemaVersion':1,'status':'candidate','stagingOnly':True,'pages':pages});save(O/(slug+'-checkpoint.json'),{'date':'2026-11-02','action':cfg['attackKey'],'status':'candidate','figures':sum(x['frames'] for x in summary),'clips':len(pages),'staticReviewed':False,'motionReviewed':False,'imported':False,'runtimeVerified':False,'testsDeferredByUser':True,'sources':summary});print(json.dumps({'action':cfg['attackKey'],'clips':len(pages),'figures':sum(x['frames'] for x in summary),'status':'candidate','testsRun':False}))
