"""Integrate reviewed intact Lucius sheets; no image pixel edits or Unity execution.
Gutter discovery ignores alpha<=96 specks only for measurement; PNGs are byte copies.
"""
import json, hashlib, shutil, statistics
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[2]
SESSION=ROOT/'ArtSources/Session20261114'
DIRS=['S','SW','W','NW','N','NE','E','SE']
INDEX=json.loads((SESSION/'zip-recovery-validation.json').read_text(encoding='utf-8'))['files']
# Source inventory index, appearance, semantic sheet ID, explicit reviewed clips.
SPECS=[
(1,'Normal','WalkNS',[('Walk',0,list(range(6)),None),('Walk',4,list(range(6,12)),None)]),
(19,'Normal','WalkDiagonalsS',[('Walk',7,list(range(6)),None),('Walk',1,list(range(6,12)),None)]),
(9,'Normal','WalkEW',[('Walk',6,list(range(6)),None),('Walk',2,list(range(6,12)),None)]),
(14,'Arconte','WalkNS',[('Walk',0,list(range(6)),None),('Walk',4,list(range(6,12)),None)]),
(8,'Arconte','WalkDiagonalsS',[('Walk',7,list(range(6)),None),('Walk',1,list(range(6,12)),None)]),
(12,'Arconte','WalkEW',[('Walk',6,list(range(6)),None),('Walk',2,list(range(6,12)),None)]),
(15,'Arconte','WalkDiagonalsN',[('Walk',5,list(range(6)),None),('Walk',3,list(range(6,11)),None)]),
(20,'Arconte','AttacksFront',[('Attack',0,[0,1,2],'SwordVertical'),('Attack',7,[3,4,5],'SwordHorizontal'),('Attack',6,[6,7,8],'SwordVertical')]),
(22,'Arconte','AttacksBack',[('Attack',4,[0,1,2],'SwordVertical'),('Attack',3,[3,4,5],'SwordHorizontal'),('Attack',2,[6,7,8],'SwordVertical'),('Attack',1,[9,10,11],'SwordThrust')]),
(16,'Arconte','ParryFront',[('Parry',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([0,7,6,5])]),
(13,'Arconte','ParryBack',[('Parry',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([4,3,2,1])]),
(18,'Arconte','HurtFront',[('Hurt',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([0,7,6,5])]),
(17,'Arconte','HurtBack',[('Hurt',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([4,3,2,1])]),
(7,'Arconte','DeadFront',[('Dead',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([0,7,6,5])]),
(23,'Arconte','DeadBack',[('Dead',d,list(range(r*3,r*3+3)),None) for r,d in enumerate([4,3,2,1])]),
]
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p,v):p.write_text(json.dumps(v,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
def cut_axis(alpha,count,vertical):
 length=alpha.height if vertical else alpha.width
 def score(pos):
  strip=alpha.crop((0,pos,alpha.width,pos+1) if vertical else (pos,0,pos+1,alpha.height))
  vals=list(strip.get_flattened_data());return (sum(v>96 for v in vals),sum(vals),abs(pos-length*k/count))
 cuts=[0]
 for k in range(1,count):
  lo=int(length*(k/count-.10));hi=int(length*(k/count+.10))
  cuts.append(min(range(max(1,lo),min(length-1,hi)),key=score))
 return cuts+[length]
def measure(im,rect):
 x,y,w,h=rect;c=im.crop((x,y,x+w,y+h));pix=c.load()
 # Boots are opaque neutral dark leather. Green aura and bright blade are excluded.
 dark=[];hair=[]
 for yy in range(h):
  for xx in range(w):
   r,g,b,a=pix[xx,yy]
   if a>220 and 9<max(r,g,b)<135 and g<r*1.3+5 and yy>h*.55:dark.append((xx,yy))
   if a>220 and r>125 and b>125 and abs(r-b)<70 and g<r*1.15 and yy<h*.60:hair.append((xx,yy))
 assert dark and hair,rect
 counts={}
 for xx,yy in dark:counts[yy]=counts.get(yy,0)+1
 foot=max(yy for yy,n in counts.items() if n>=3)+1
 sole=[xx for xx,yy in dark if yy>=foot-14]
 # Midpoint of boot support, manually reviewed in the full-figure anchor contacts.
 px=(min(sole)+max(sole)+1)/2
 top=min(yy for xx,yy in hair)
 torso=[xx for xx,yy in dark if top+(foot-top)*.45<=yy<=top+(foot-top)*.65]
 if torso: px=statistics.median(torso)
 return (x+px,y+foot),foot-top

def main():
 cats={};report={'status':'visual-reviewed','imported':False,'runtimeVerified':False,'sheets':[],'limitations':['Normal new Walk covers S,N,SE,SW,E,W only; NE/NW preserved from previous catalog.','Normal source 21 repeats E/W and is retained raw, not relabelled NE/NW.','Arconte Walk NW frame 11 turns toward front: omitted, leaving five genuine NW frames.','New green/gold attack, guard, hurt and death sources are Arconte only.','Attack coverage is technique/direction selective, not a complete three-technique eight-direction matrix.','Source20 last attack row has overlapping opaque VFX at the last separator; frames9-11 and the NE thrust replacement are omitted. Existing NE thrust stays intact.','Measurements preserve native source pixels; Unity import and runtime/transition checks are separate.']}
 preview=SESSION/'review/lucius';preview.mkdir(exist_ok=True)
 for appearance in ['Normal','Arconte']:
  folder=ROOT/('Assets/Darklight/Resources/Darklight/Lucius'+('Normal20261109' if appearance=='Normal' else 'Arconte20261111'))
  cat=json.loads((folder/'catalog.json').read_text(encoding='utf-8'));cats[appearance]=(folder,cat)
  assert len([p for p in cat['pages'] if p['action']=='Idle'])==8
 for idx,appearance,label,clips in SPECS:
  folder,cat=cats[appearance];src=ROOT/INDEX[idx]['localPath'];assert digest(src)==INDEX[idx]['sha256']
  im=Image.open(src).convert('RGBA');alpha=im.getchannel('A');ys=cut_axis(alpha,4,True);rects=[]
  for y0,y1 in zip(ys,ys[1:]):
   xs=cut_axis(alpha.crop((0,y0,im.width,y1)),3,False)
   rects.extend([(x0,y0,x1-x0,y1-y0) for x0,x1 in zip(xs,xs[1:])])
  anchors=[];heights=[]
  for r in rects:
   a,h=measure(im,r);anchors.append(a);heights.append(h)
  # Canonical Normal idle measures 410px hair-to-boot at 214.970901 PPU.
  # Arconte body keeps the same physical stature; aura/raised blades are excluded.
  reference_indices=([0] if label.startswith('Attacks') else [0,2,9,11] if label.startswith('Parry') else [0,3,6,9] if label.startswith(('Dead','Hurt')) else list(range(12)))
  selected=[heights[i] for i in reference_indices]
  if label.startswith('Dead'):
   # Collapse travels within the original figure, not by recentering the fallen torso.
   for row in range(4):
    base=row*3; relative_x=anchors[base][0]-rects[base][0]
    for col in (1,2):
     i=base+col;anchors[i]=(rects[i][0]+relative_x,anchors[i][1])
  body_height=statistics.median(selected);ppu=round(body_height/(410/214.97090106361628),6)
  name='Lucius'+appearance+label+'Session20261114';png=folder/(name+'.png');shutil.copyfile(src,png)
  frames=[]
  for i,((x,y,w,h),(ax,ay)) in enumerate(zip(rects,anchors)):
   frames.append({'id':f'Redesign_{name}_{i:02d}','rect':[x,y,w,h],'pivot':[round((ax-x)/w,7),round(1-(ay-y)/h,7)],'bodyAnchorPx':[ax,ay]})
  meta={'schemaVersion':1,'image':png.name,'imageSha256':digest(png),'sourcePath':src.relative_to(ROOT).as_posix(),'sourceSha256':digest(src),'sourceSize':list(im.size),'nativeSize':list(im.size),'pixelsPerUnit':ppu,'wholeFigures':True,'status':'visual-reviewed','frames':frames[:9] if idx==20 else frames[:11] if idx==15 else frames,'calibration':{'canonicalBodyWorldHeight':410/214.97090106361628,'measuredBodyPixels':body_height,'referenceFrameIndices':reference_indices,'method':'Hair-to-boot neutral/reference body measurement excluding green aura and blade; opaque leather foot-support anchor. Full-figure review contact records every anchor.'}}
  save(png.with_suffix('.json'),meta)
  added=[]
  for action,direction,indices,key in clips:
   page={k:meta[k] for k in ['imageSha256','sourceSha256','sourcePath','sourceSize','nativeSize','pixelsPerUnit','wholeFigures','status']}
   page.update(id=f'{name}_{action}_{DIRS[direction]}',action=action,direction=direction,resourcePath='Darklight/'+folder.name+'/'+name,spriteNames=[frames[i]['id'] for i in indices],durationMs=([95]*len(indices) if action=='Walk' else [110,90,180] if action=='Attack' else [120,100,180]),loop=action=='Walk')
   if action=='Attack':page.update(attackKey=key,startupFrames=1,activeFrames=1)
   matches=[n for n,p in enumerate(cat['pages']) if (p['action'],p['direction'],p.get('attackKey'))==(action,direction,key)]
   assert len(matches)<=1
   if matches:cat['pages'][matches[0]]=page
   else:cat['pages'].append(page)
   added.append(page['id'])
   # Animated inspection keeps each figure intact, aligned by its foot pivot.
   canvases=[]
   for i in indices:
    r=rects[i];a=anchors[i];c=im.crop((r[0],r[1],r[0]+r[2],r[1]+r[3]));canvas=Image.new('RGBA',(1000,800),(35,37,41,255));canvas.alpha_composite(c,(round(500-(a[0]-r[0])),round(650-(a[1]-r[1]))));canvases.append(canvas.resize((500,400),Image.Resampling.NEAREST).convert('RGB'))
   canvases[0].save(preview/(page['id']+'.gif'),save_all=True,append_images=canvases[1:],duration=page['durationMs'],loop=0)
  contact=im.copy();draw=ImageDraw.Draw(contact)
  for i,(r,a) in enumerate(zip(rects,anchors)):
   x,y,w,h=r;draw.rectangle((x,y,x+w-1,y+h-1),outline='cyan',width=2);draw.line((a[0]-12,a[1],a[0]+12,a[1]),fill='red',width=2);draw.line((a[0],a[1]-12,a[0],a[1]+8),fill='red',width=2);draw.text((x+5,y+5),str(i)+(' OMITTED' if idx==20 and i>=9 or idx==15 and i==11 else ''),fill='white')
  contact.save(preview/(name+'-anchors.png'))
  report['sheets'].append({'sourceIndex':idx,'source':meta['sourcePath'],'manifest':png.with_suffix('.json').relative_to(ROOT).as_posix(),'frames':len(meta['frames']),'clips':added,'bodyHeightPixels':body_height,'pixelsPerUnit':ppu})
 for appearance,(folder,cat) in cats.items():
  contexts=[(p['action'],p['direction'],p.get('attackKey')) for p in cat['pages']];assert len(contexts)==len(set(contexts));assert sum(p['action']=='Idle' for p in cat['pages'])==8
  cat['complete']=False;save(folder/'catalog.json',cat)
 previous=SESSION/'lucius-integration.json'
 if previous.exists():
  old=json.loads(previous.read_text(encoding='utf-8'))
  if 'baselineRecovery' in old:report['baselineRecovery']=old['baselineRecovery']
 report['validation']={'sheets':len(report['sheets']),'activeManifestFrames':sum(s['frames'] for s in report['sheets']),'newClips':sum(len(s['clips']) for s in report['sheets']),'byteIdenticalSources':True,'nonoverlappingRects':True,'unityImported':False,'runtimeVerified':False}
 save(previous,report)
 print(json.dumps({'sheets':len(report['sheets']),'clips':sum(len(s['clips']) for s in report['sheets']),'status':'visual-reviewed','imported':False}))
if __name__=='__main__':main()
