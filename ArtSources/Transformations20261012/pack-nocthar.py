"""Technical alpha extraction, padding and foot-pivot atlas assembly of generated poses."""
from pathlib import Path
from PIL import Image
import numpy as np,json,hashlib
stage=Path(__file__).parent
source=Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3/exec-e7ada9e4-c10e-4a89-a694-2044e6558474.png')
a=np.asarray(Image.open(source).convert('RGBA')).copy()
helpers={};exec((stage.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],helpers)
cells=[];audit=[]
labels,n,boxes=helpers['components_for'](a[:,:,3]>=166)
sizes=np.bincount(labels.ravel());sizes[0]=0
feet=[(185,443),(550,443),(983,442),(1346,443),(186,926),(540,923),(831,913),(1344,929)]
for i in range(8):
 x0=(i%4)*384;y0=(i//4)*512
 counts=np.bincount(labels[y0:y0+512,x0:x0+384].ravel(),minlength=n+1);counts[0]=0
 main=int(counts.argmax());mask=labels==main
 for j in range(1,n+1):
  if 24<=sizes[j]<1000 and counts[j]==sizes[j]:mask|=labels==j
 yy,xx=np.nonzero(mask);assert len(xx)>1000
 l,t,r,b=int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)
 assert l>0 and r<1536 and t>0 and b<1024,(i,l,t,r,b)
 c=a[t:b,l:r].copy();c[:,:,3]=np.where(mask[t:b,l:r],c[:,:,3],0);crop=Image.fromarray(c)
 fx,fy=feet[i];anchor=(fx-l,fy-t)
 assert 0<=anchor[0]<=crop.width and 0<=anchor[1]<=crop.height+3,(i,anchor,crop.size)
 cells.append((crop,anchor));audit.append({'frame':i,'sourceRect':[l,t,r-l,b-t],'foot':[fx,fy],'opaquePixels':len(xx)})
cw=((max(c.width for c,_ in cells)+48+15)//16)*16;ch=((max(c.height for c,_ in cells)+48+15)//16)*16
out=Image.new('RGBA',(cw*4,ch*2));frames=[]
for i,(crop,anchor) in enumerate(cells):
 x=(i%4)*cw+24;y=(i//4)*ch+24;out.alpha_composite(crop,(x,y))
 frames.append({'name':f'NoctharFinal_{i}','rect':[x,y,crop.width,crop.height],'pivot':[anchor[0]/crop.width,1-anchor[1]/crop.height]})
out.save(stage/'NoctharFinal.png')
(stage/'NoctharFinalRects.json').write_text(json.dumps({'pixelsPerUnit':cells[0][0].height/2,'frames':frames},indent=2))
(stage/'nocthar-art-audit.json').write_text(json.dumps({'source':str(source),'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'atlasSize':out.size,'padding':24,'frames':audit},indent=2))
print(json.dumps({'frames':8,'atlasSize':out.size,'alphaExtractionThreshold':166,'padding':24}))
