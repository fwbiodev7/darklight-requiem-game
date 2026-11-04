from pathlib import Path
import json,sys,hashlib
from PIL import Image,ImageDraw
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');O=R/'outputs/retomada-20261102/normal-katana-completion';S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion'
def rd(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def wr(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
cfg=rd(S/sys.argv[1]);slug=cfg['slug'];commit='--reviewed' in sys.argv;allpages=[];rows=[];review=[]
for spec in cfg['sources']:
 name=spec['name'];p=O/'staged'/(name+'.json');m=rd(p);im=Image.open(O/'staged'/(name+'.png')).convert('RGBA');n=len(cfg['durationMs'])
 for i in range(0,len(m['frames']),n):
  fs=m['frames'][i:i+n];tiles=[]
  for f in fs:
   x,y,w,h=f['rect'];tiles.append(im.crop((x,y,x+w,y+h)))
  row=Image.new('RGB',(sum(t.width+8 for t in tiles),max(t.height for t in tiles)+24),(65,65,72));dr=ImageDraw.Draw(row);x=0
  for f,t in zip(fs,tiles):row.paste(t,(x,24),t);dr.text((x+4,4),f['direction']+' '+f['phase'],fill='white');x+=t.width+8
  rows.append(row)
 if commit:
  m.update(status='visual-reviewed',visualReviewed=True,staticVisualReviewed=True,motionVisualReviewed=False,testsDeferredByUser=True,reviewMethod='All complete authored source figures and packed native contact inspected statically; fullbody phases, blade grip, real facing and approved compact proportions checked. No playback/runtime test claimed.')
  wr(p,m)
 pg=rd(O/'staged'/(name+'.pages.json'))
 if commit:
  for page in pg['pages']:page.update(status='visual-reviewed',visualReviewed=True,staticVisualReviewed=True,motionVisualReviewed=False,testsDeferredByUser=True)
  wr(O/'staged'/(name+'.pages.json'),pg)
 allpages+=pg['pages'];review.append({'name':name,'sourceSha256':m['sourceSha256'],'imageSha256':m['imageSha256'],'manifestSha256':hashlib.sha256(p.read_bytes()).hexdigest(),'rgbaProofFrames':len(m['frameRgbaProof'])})
contact=Image.new('RGB',(max(r.width for r in rows),sum(r.height+6 for r in rows)),(45,45,52));y=0
for row in rows:contact.paste(row,(0,y));y+=row.height+6
contact.save(O/(slug+'-contact-native.png'))
if commit:
 fragment={'schemaVersion':1,'status':'visual-reviewed','stagingOnly':True,'staticReviewOnly':True,'pages':allpages};wr(O/(slug+'-pages-fragment.json'),fragment);fhash=hashlib.sha256((O/(slug+'-pages-fragment.json')).read_bytes()).hexdigest()
 wr(O/(slug+'-visual-review.json'),{'date':'2026-11-02','status':'visual-reviewed','staticOnly':True,'action':cfg['attackKey'],'clips':len(allpages),'wholeFigures':sum(v['rgbaProofFrames'] for v in review),'findings':cfg.get('reviewFindings',[]),'sources':review,'motionReviewed':False,'imported':False,'runtimeVerified':False,'testsDeferredByUser':True})
 cp=rd(O/(slug+'-checkpoint.json'));cp.update(status='visual-reviewed',staticReviewed=True,motionReviewed=False,fragmentSha256=fhash,pending=['Coordinator immediate asset integration','All tests after production per user','Remaining techniques and states']);wr(O/(slug+'-checkpoint.json'),cp);print(json.dumps({'action':cfg['attackKey'],'clips':len(allpages),'status':'visual-reviewed-static','fragmentSha256':fhash}))
else:print(json.dumps({'contact':str(O/(slug+'-contact-native.png')),'status':'candidate-visual-inspection-pending'}))
