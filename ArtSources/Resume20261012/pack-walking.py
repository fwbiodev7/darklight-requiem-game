from pathlib import Path
import json, math, shutil
import numpy as np
from PIL import Image

here=Path(__file__).resolve().parent
ns={};exec((here.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],ns)
source=Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')
inputs=[(source/'exec-f25ffb29-9225-40be-aecb-a959c3d5420f.png',32,4,8),(source/'exec-1f339b44-35bf-4ee5-ab07-dd4db2a3f317.png',4,2,2)]
sources=here/'art-sources';sources.mkdir(exist_ok=True)
groups=[]
for file,expected,rows,cols in inputs:
    shutil.copy2(file,sources/file.name)
    pix=np.asarray(Image.open(file).convert('RGBA'));labels,n,boxes=ns['components_for'](pix[:,:,3]>=166)
    sizes=np.bincount(labels.ravel());main=[k for k in range(1,n+1) if sizes[k]>1000]
    assert len(main)==expected,(file.name,len(main))
    assert max(pix[[0,-1],:,3].max(),pix[:,[0,-1],3].max())<=12
    main.sort(key=lambda k:(boxes[k-1][0].start+boxes[k-1][0].stop)/2)
    order=[]
    for row in range(rows):order+=sorted(main[row*cols:(row+1)*cols],key=lambda k:boxes[k-1][1].start)
    owners={k:k for k in main}
    for k in range(1,n+1):
        if k in owners or sizes[k]<8:continue
        yy,xx=boxes[k-1];cx=(xx.start+xx.stop)/2;cy=(yy.start+yy.stop)/2
        def distance(j):
            y,x=boxes[j-1];return max(x.start-cx,0,cx-x.stop)**2+max(y.start-cy,0,cy-y.stop)**2
        near=min(main,key=distance)
        if distance(near)<1600:owners[k]=near
    crops=[]
    for k in order:
        mask=np.isin(labels,[j for j,o in owners.items() if o==k]);ys,xs=np.nonzero(mask)
        x0,y0,x1,y1=int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)
        owned=mask[y0:y1,x0:x1];dil=owned.copy();pad=np.pad(owned,1)
        for dy in (-1,0,1):
            for dx in (-1,0,1):dil|=pad[1+dy:1+dy+owned.shape[0],1+dx:1+dx+owned.shape[1]]
        rgba=pix[y0:y1,x0:x1].copy();rgba[~dil,3]=0;by,bx=boxes[k-1]
        _,feet=np.nonzero(labels[by.stop-max(2,(by.stop-by.start)//10):by.stop,bx.start:bx.stop]==k)
        anchor=bx.start+(feet.min()+feet.max())/2
        crops.append(dict(image=Image.fromarray(rgba),anchor=anchor-x0,foot=y1-by.stop,height=by.stop-by.start,source=str(sources/file.name),sourceRect=[x0,y0,x1-x0,y1-y0]))
    groups.append(crops)
# The replacement NW source was generated at a larger resolution. One fixed nearest-neighbor
# factor for the entire four-frame sequence preserves its gait and pose-height changes.
ratio=float(np.median([c['height'] for c in groups[0]]))/float(np.median([c['height'] for c in groups[1]]))
for c in groups[1]:
    im=c['image'];c['image']=im.resize((round(im.width*ratio),round(im.height*ratio)),Image.Resampling.NEAREST)
    for k in ('anchor','foot','height'):c[k]*=ratio
crops=groups[0][:12]+groups[1]+groups[0][16:]
assert len(crops)==32
cw=math.ceil((2*max(max(c['anchor'],c['image'].width-c['anchor']) for c in crops)+48)/16)*16
ch=math.ceil((max(c['image'].height for c in crops)+48)/16)*16
atlas=Image.new('RGBA',(8*cw,4*ch));assert max(atlas.size)<=4096
name='RequiemWalkKatanaStage4';frames=[]
for i,c in enumerate(crops):
    im=c['image'];x=i%8*cw+round(cw/2-c['anchor']);y=i//8*ch+ch-24-im.height
    assert x-i%8*cw>=24 and (i%8+1)*cw-x-im.width>=24
    atlas.alpha_composite(im,(x,y))
    frames.append(dict(name=name+'_'+str(i),rect=[x,y,im.width,im.height],pivot=[c['anchor']/im.width,c['foot']/im.height],source=c['source'],sourceRect=c['sourceRect'],direction=['S','SW','W','NW','N','NE','E','SE'][i//4],step=i%4))
out=here/'Assets/Darklight/Resources/Darklight/Walking';out.mkdir(parents=True,exist_ok=True)
atlas.save(out/(name+'.png'))
doc=dict(pixelsPerUnit=float(np.median([c['height'] for c in crops]))/2,frames=frames,padding=24,columns=8,rows=4,sourcePreserved=True,fps=10,visualReview='NW replaced with separately reviewed source; other seven directions retained; runtime review pending',replacementScale=ratio)
(out/(name+'Rects.json')).write_text(json.dumps(doc,indent=2),encoding='utf-8')
(here/'walking-report.json').write_text(json.dumps(dict(name=name,frames=32,size=atlas.size,ppu=doc['pixelsPerUnit'],padding=24,nwReplacementScale=ratio,runtimeEnabled=False),indent=2))
print(json.dumps(dict(frames=32,size=atlas.size,ppu=doc['pixelsPerUnit'],padding=24)))
