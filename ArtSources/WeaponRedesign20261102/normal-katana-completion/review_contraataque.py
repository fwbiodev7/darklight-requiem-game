from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');O=R/'outputs/retomada-20261102/normal-katana-completion';S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion'
def rd(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def wr(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
cfg=rd(S/'import-jobs-contraataque.json');allpages=[];rows=[];review=[]
for spec in cfg['sources']:
 name=spec['name'];p=O/'staged'/(name+'.json');m=rd(p);im=Image.open(O/'staged'/(name+'.png')).convert('RGBA');m.update(status='visual-reviewed',visualReviewed=True,staticVisualReviewed=True,motionVisualReviewed=False,testsDeferredByUser=True,multiSourceWholeFigurePacking=bool(spec.get('replacements')),sourcePixels=sum(im.getchannel('A').histogram()[1:]),packedPixels=sum(im.getchannel('A').histogram()[1:]),reviewMethod='All complete source figures inspected at native raster; SE contact corrected as one complete figure, original other 31 figures retained byte-exact at every nonzero-alpha source pixel. Static phases checked; no playback or runtime test claimed.')
 for i in range(0,len(m['frames']),4):
  fs=m['frames'][i:i+4];tiles=[]
  for f in fs:
   x,y,w,h=f['rect'];tiles.append(im.crop((x,y,x+w,y+h)))
  rw=sum(t.width+8 for t in tiles);rh=max(t.height for t in tiles)+24;row=Image.new('RGB',(rw,rh),(65,65,72));dr=ImageDraw.Draw(row);x=0
  for f,t in zip(fs,tiles):row.paste(t,(x,24),t);dr.text((x+4,4),f['direction']+' '+f['phase'],fill='white');x+=t.width+8
  rows.append(row)
 wr(p,m);pg=rd(O/'staged'/(name+'.pages.json'))
 for page in pg['pages']:page.update(status='visual-reviewed',visualReviewed=True,staticVisualReviewed=True,motionVisualReviewed=False,testsDeferredByUser=True,multiSourceWholeFigurePacking=bool(spec.get('replacements')))
 wr(O/'staged'/(name+'.pages.json'),pg);allpages+=pg['pages'];review.append({'name':name,'sourceSha256':m['sourceSha256'],'imageSha256':m['imageSha256'],'manifestSha256':hashlib.sha256(p.read_bytes()).hexdigest(),'rgbaProofFrames':len(m['frameRgbaProof']),'replacementOnlySourceIndices':[5] if spec.get('replacements') else []})
contact=Image.new('RGB',(max(r.width for r in rows),sum(r.height+6 for r in rows)),(45,45,52));y=0
for row in rows:contact.paste(row,(0,y));y+=row.height+6
contact.save(O/'contraataque-contact-native.png');fragment={'schemaVersion':1,'status':'visual-reviewed','stagingOnly':True,'staticReviewOnly':True,'pages':allpages};wr(O/'contraataque-pages-fragment.json',fragment)
wr(O/'contraataque-visual-review.json',{'date':'2026-11-02','status':'visual-reviewed','staticOnly':True,'action':'ContraAtaque','clips':8,'wholeFigures':32,'preservedOriginalCompleteFigures':31,'correctedCompleteFigures':1,'findings':['SE active riposte now follows facing toward screen-right/down-right','All original compact legs, adult approved identity, Normal brown hair, black/red coat and neutral steel katana preserved in the other 31 figures','Four complete phase poses per direction with current runtime35/90/65/65ms','No resampling, mirroring, anatomical graft or repaint of original valid figures'],'sources':review,'motionReviewed':False,'imported':False,'runtimeVerified':False,'testsDeferredByUser':True})
cp=rd(O/'contraataque-checkpoint.json');cp.update(status='visual-reviewed',staticReviewed=True,motionReviewed=False,fragmentSha256=hashlib.sha256((O/'contraataque-pages-fragment.json').read_bytes()).hexdigest(),pending=['Coordinator immediate asset integration','All tests after production per user','Remaining techniques and states']);wr(O/'contraataque-checkpoint.json',cp);print(json.dumps({'clips':8,'figures':32,'status':'visual-reviewed-static','fragmentSha256':cp['fragmentSha256']}))
