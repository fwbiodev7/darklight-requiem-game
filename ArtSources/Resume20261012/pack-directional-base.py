from pathlib import Path
import json,math
import numpy as np
from PIL import Image
s=Path(__file__).parent;n={};exec((s.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],n)
b=Path(r'C:/Users/Usuario/.codex/generated_images/01a12b16-c088-7f73-a2eb-6ff2fde39e75')
files=['exec-6270dc37-21e3-4f00-b98a-63e6f50adca7.png','exec-3b800528-8b5f-4e85-bf8b-dc0128035c8d.png']
crops=[];pivots=[];heights=[]
for file in files:
    pix=np.asarray(Image.open(b/file).convert('RGBA'));labels,count,boxes=n['components_for'](pix[:,:,3]>=166);sizes=np.bincount(labels.ravel());main=[k for k in range(1,count+1) if sizes[k]>1000]
    assert len(main)==16 and not np.any(pix[[0,-1],:,3]>12) and not np.any(pix[:,[0,-1],3]>12)
    main.sort(key=lambda k:(boxes[k-1][0].start+boxes[k-1][0].stop)/2);order=[]
    for row in range(4):order+=sorted(main[row*4:row*4+4],key=lambda k:boxes[k-1][1].start)
    owners={k:k for k in main}
    for k in range(1,count+1):
        if k in owners or sizes[k]<8:continue
        yy,xx=boxes[k-1];cx=(xx.start+xx.stop)/2;cy=(yy.start+yy.stop)/2
        def distance(j):
            y,x=boxes[j-1];return max(x.start-cx,0,cx-x.stop)**2+max(y.start-cy,0,cy-y.stop)**2
        near=min(main,key=distance)
        if distance(near)<1600:owners[k]=near
    for i,k in enumerate(order):
        mask=np.isin(labels,[j for j,o in owners.items() if o==k]);ys,xs=np.nonzero(mask);x0,y0,x1,y1=int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)
        own=mask[y0:y1,x0:x1];dil=own.copy();padded=np.pad(own,1)
        for dy in (-1,0,1):
            for dx in (-1,0,1):dil|=padded[1+dy:1+dy+own.shape[0],1+dx:1+dx+own.shape[1]]
        rgba=pix[y0:y1,x0:x1].copy();rgba[~dil,3]=0;by,bx=boxes[k-1]
        _,feet=np.nonzero(labels[by.stop-max(2,(by.stop-by.start)//10):by.stop,bx.start:bx.stop]==k);anchor=bx.start+(feet.min()+feet.max())/2
        crops.append(Image.fromarray(rgba));pivots.append((anchor-x0,y1-by.stop))
        if i%4==3:heights.append(by.stop-by.start)
cw=math.ceil((2*max(max(x,c.width-x) for c,(x,_) in zip(crops,pivots))+48)/16)*16;ch=math.ceil((max(c.height for c in crops)+48)/16)*16
atlas=Image.new('RGBA',(4*cw,8*ch));name='RockKatanaLightStage4';ppu=float(np.median(heights))/2;frames=[]
assert max(atlas.size)<=4096
for i,(crop,(anchor,foot)) in enumerate(zip(crops,pivots)):
    x=i%4*cw+int(round(cw/2-anchor));y=i//4*ch+ch-24-crop.height
    assert x-i%4*cw>=24 and (i%4+1)*cw-x-crop.width>=24
    atlas.alpha_composite(crop,(x,y));frames.append(dict(name=name+'_'+str(i),rect=[x,y,crop.width,crop.height],pivot=[anchor/crop.width,foot/crop.height]))
out=s/'Assets/Darklight/Resources/Darklight/AnimationCatalog';atlas.save(out/(name+'.png'))
(out/(name+'Rects.json')).write_text(json.dumps(dict(pixelsPerUnit=ppu,frames=frames,padding=24,sources=[str(b/f) for f in files],height=atlas.height),indent=2))
(s/'rock-light-direction-report.json').write_text(json.dumps(dict(name=name,count=32,directions=['S','SW','W','NW','N','NE','E','SE'],phaseCount=4,padding=24,ppu=ppu,sources=files,visualDirectionReview='accepted; swing torso rotation remains within facing viewpoint',runtimeReview='pending'),indent=2))
print(json.dumps(dict(count=32,ppu=ppu,atlas=atlas.size,sources=files)))
