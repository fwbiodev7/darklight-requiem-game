"""Trim only RGBA0000 and shelf-pack complete frame rectangles; prove exact reconstruction."""
from pathlib import Path
import json,hashlib,copy,shutil,sys
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[3]
ART=ROOT/'ArtSources/WeaponRedesign20261102/katana-pesado'
OUT=ROOT/'outputs/retomada-20261102/katana-pesado'
FINAL=OUT/'staged'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipe
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
name='ReqKatanaNormalPesadoNW_N_NE20261102V2'
m=read(FINAL/(name+'.json'))
original_layout=read(OUT/'v2/whole-frame-replacement/sheet.json')
for current, original in zip(m['frames'],original_layout['frames']): current.update(rect=original['rect'],pivot=original['pivot'])
im=Image.open(OUT/'v2/whole-frame-replacement/sheet.png').convert('RGBA')
frames=[]
for i,f in enumerate(m['frames']):
 x,y,w,h=f['rect'];original=im.crop((x,y,x+w,y+h));a=np.asarray(original);ys,xs=np.where(np.any(a!=0,axis=2))
 box=[max(0,int(xs.min())-2),max(0,int(ys.min())-2),min(w,int(xs.max()+3)),min(h,int(ys.max()+3))]
 trimmed=original.crop(box)
 frames.append(dict(index=i,box=box,original=original,trimmed=trimmed,hash=hashlib.sha256(original.tobytes()).hexdigest(),pivot=f['pivot']))
def layout(width):
 positions={};x=y=rowh=0
 for z in sorted(frames,key=lambda z:z['trimmed'].height,reverse=True):
  w,h=z['trimmed'].size
  if w>width:return None
  if x+w+4>width:x=0;y+=rowh+4;rowh=0
  positions[z['index']]=(x,y);x+=w+4;rowh=max(rowh,h)
 height=y+rowh
 if height>4096:return None
 return width*height,width,height,positions
options=[p for w in [1792,1920,2048,2304,2560,3072,3584,4096] if (p:=layout(w))]
area,width,height,pos=min(options,key=lambda p:p[0])
assert width<=4096 and height<=4096 and width*height*4<20_000_000,(width,height)
atlas=Image.new('RGBA',(width,height));evidence=[]
for z in frames:
 i=z['index'];x,y=pos[i];trim=z['trimmed'];atlas.paste(trim,(x,y));oldw,oldh=z['original'].size;left,top,_,_=z['box']
 f=m['frames'][i]
 f['rect']=[x,y,*trim.size]
 f['pivot']=[(z['pivot'][0]*oldw-left)/trim.width,1-((1-z['pivot'][1])*oldh-top)/trim.height]
 assert all(0<=p<=1 for p in f['pivot'])
 restored=Image.new('RGBA',(oldw,oldh));restored.paste(atlas.crop((x,y,x+trim.width,y+trim.height)),(left,top))
 assert restored.tobytes()==z['original'].tobytes()
 evidence.append(dict(frameId=f['id'],index=i,beforeCanvas=[oldw,oldh],trimRgba0000=box if False else z['box'],
 finalRect=f['rect'],reconstructedRgbaSha256=z['hash'],exactReconstruction=True))
atlas.save(FINAL/(name+'.png'))
m.update(imageSha256=pipe.sha(FINAL/(name+'.png')),nativeSize=list(atlas.size),sourceSize=list(atlas.size),
 sourceSha256=pipe.sha(FINAL/(name+'.png')),rectanglePacking='variable-rectangles-shelves',
 compaction=dict(method='Only RGBA0000 border pixels trimmed; offsets applied to pivots, no scaling/recoloring/alpha loss.',
  width=width,height=height,rawBytes=width*height*4,exactWholeFrameReconstruction=True))
pipe.write(FINAL/(name+'.json'),m)
source=ART/(name+'-source.png')
shutil.copy2(FINAL/(name+'.png'),source)
pages=read(FINAL/(name+'.pages.json'))
for p in pages['pages']:p.update(imageSha256=m['imageSha256'],sourceSha256=m['sourceSha256'],nativeSize=list(atlas.size),sourceSize=list(atlas.size))
pipe.write(FINAL/(name+'.pages.json'),pages)
fragment=read(OUT/'pages-fragment-v2.json')
bydir={p['direction']:p for p in pages['pages']}
fragment['pages']=[bydir.get(p['direction'],p) for p in fragment['pages']]
pipe.write(OUT/'pages-fragment-v2.json',fragment)
proof=read(OUT/'preservation-proof-v2.json')
proof.update(finalPacking='RGBA0000-only trim, variable rectangles/shelves, exact full-frame reconstruction.',compactAtlas=list(atlas.size),
 rawBytes=width*height*4,sourceAfterCompactionSha256=pipe.sha(source),compactionFrameProofs=evidence)
pipe.write(OUT/'preservation-proof-v2.json',proof)
jobs=read(ART/'import-jobs-pesado-v2.json')
j=jobs['jobs'][1]
j.update(packing='manifest-authoritative-variable-rectangles',manifestPath=(FINAL/(name+'.json')).relative_to(ROOT).as_posix(),
 note='Source is already packed. Use authoritative manifest frame rectangles/pivots/per-frame PPU; do not identify a uniform grid.')
j.pop('columns',None);j.pop('sourceRows',None);j.pop('supportAnchors',None)
pipe.write(ART/'import-jobs-pesado-v2.json',jobs)
pipe.RES=FINAL;pipe.OUT=OUT/'qa-v2-compact'
qa=pipe.qa(name)
cp=read(OUT/'checkpoint-v2.json')
cp.update(rearCompaction=dict(atlas=list(atlas.size),rawBytes=width*height*4,exactWholeFrameReconstruction=True),compactRearQa=qa)
pipe.write(OUT/'checkpoint-v2.json',cp)
print(json.dumps(dict(atlas=list(atlas.size),rawBytes=width*height*4,exactFrames=len(evidence),qa=qa)))


