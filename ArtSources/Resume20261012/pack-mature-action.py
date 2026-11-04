from pathlib import Path
import argparse,json,math,shutil
import numpy as np
from PIL import Image

here=Path(__file__).resolve().parent
helpers={}
exec((here.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],helpers)
generated=Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')

def crops(file,expected):
    pix=np.asarray(Image.open(generated/file).convert('RGBA'))
    labels,count,boxes=helpers['components_for'](pix[:,:,3]>=166)
    sizes=np.bincount(labels.ravel())
    main=[k for k in range(1,count+1) if sizes[k]>1000]
    assert len(main)==expected,(file,len(main),expected)
    assert not np.any(pix[[0,-1],:,3]>12) and not np.any(pix[:,[0,-1],3]>12),file
    main.sort(key=lambda k:(boxes[k-1][0].start+boxes[k-1][0].stop)/2)
    columns=2 if expected==4 else 4
    order=[]
    for row in range(expected//columns):order+=sorted(main[row*columns:(row+1)*columns],key=lambda k:boxes[k-1][1].start)
    owners={k:k for k in main}
    for k in range(1,count+1):
        if k in owners or sizes[k]<8:continue
        yy,xx=boxes[k-1];cx=(xx.start+xx.stop)/2;cy=(yy.start+yy.stop)/2
        def distance(j):
            y,x=boxes[j-1]
            return max(x.start-cx,0,cx-x.stop)**2+max(y.start-cy,0,cy-y.stop)**2
        near=min(main,key=distance)
        if distance(near)<1600:owners[k]=near
    result=[]
    for k in order:
        mask=np.isin(labels,[j for j,o in owners.items() if o==k]);ys,xs=np.nonzero(mask)
        x0,y0,x1,y1=int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)
        own=mask[y0:y1,x0:x1];dil=own.copy();padded=np.pad(own,1)
        for dy in (-1,0,1):
            for dx in (-1,0,1):dil|=padded[1+dy:1+dy+own.shape[0],1+dx:1+dx+own.shape[1]]
        rgba=pix[y0:y1,x0:x1].copy();rgba[~dil,3]=0
        by,bx=boxes[k-1]
        _,feet=np.nonzero(labels[by.stop-max(2,(by.stop-by.start)//10):by.stop,bx.start:bx.stop]==k)
        anchor=bx.start+(feet.min()+feet.max())/2
        result.append(dict(image=Image.fromarray(rgba),anchor=anchor-x0,foot=y1-by.stop,height=by.stop-by.start,source=file,sourceRect=[x0,y0,x1-x0,y1-y0],sourceScale=1.0))
    return result

def scale_group(group,standing_height):
    ratio=standing_height/float(np.median([c['height'] for c in group[3::4]]))
    for c in group:
        im=c['image'];c['image']=im.resize((round(im.width*ratio),round(im.height*ratio)),Image.Resampling.NEAREST)
        for key in ('anchor','foot','height'):c[key]*=ratio
        c['sourceScale']=ratio
    return group

args=argparse.ArgumentParser()
args.add_argument('--name',required=True);args.add_argument('--south',required=True);args.add_argument('--north',required=True);args.add_argument('--nw');args.add_argument('--north-phase-order',default='0,1,2,3')
a=args.parse_args()
south=crops(a.south,16);north=crops(a.north,16)
height=float(np.median([c['height'] for c in south[:12][3::4]]))
north=scale_group(north,height)
phase_order=[int(n) for n in a.north_phase_order.split(',')];assert sorted(phase_order)==[0,1,2,3]
for i,c in enumerate(north):c['authoredPhase']=i%4
north=[north[row*4+phase] for row in range(4) for phase in phase_order]
if a.nw:south=south[:12]+scale_group(crops(a.nw,4),height)
all_crops=south+north
cw=math.ceil((2*max(max(c['anchor'],c['image'].width-c['anchor']) for c in all_crops)+48)/16)*16
ch=math.ceil((max(c['image'].height for c in all_crops)+48)/16)*16
atlas=Image.new('RGBA',(4*cw,8*ch));assert max(atlas.size)<=4096
frames=[]
for i,c in enumerate(all_crops):
    im=c['image'];x=i%4*cw+round(cw/2-c['anchor']);y=i//4*ch+ch-24-im.height
    assert x-i%4*cw>=24 and (i%4+1)*cw-x-im.width>=24
    atlas.alpha_composite(im,(x,y))
    frames.append(dict(name=a.name+'_'+str(i),rect=[x,y,im.width,im.height],pivot=[c['anchor']/im.width,c['foot']/im.height],source=c['source'],sourceRect=c['sourceRect'],sourceScale=c['sourceScale'],direction=['S','SW','W','NW','N','NE','E','SE'][i//4],phase=i%4,authoredPhase=c.get('authoredPhase',i%4)))
out=here/'Assets/Darklight/Resources/Darklight/AnimationCatalog';out.mkdir(parents=True,exist_ok=True)
atlas.save(out/(a.name+'.png'))
doc=dict(pixelsPerUnit=height/2,frames=frames,padding=24,sources=[str(generated/f) for f in [a.south,a.north,a.nw] if f],visualReview='directions and distinct gesture reviewed before packing; NW replacement where specified',runtimeEnabled=False)
(out/(a.name+'Rects.json')).write_text(json.dumps(doc,indent=2),encoding='utf-8')
for file in [a.south,a.north,a.nw]:
    if file:shutil.copy2(generated/file,here/'art-sources'/file)
print(json.dumps(dict(name=a.name,frames=32,ppu=doc['pixelsPerUnit'],size=atlas.size,northScale=north[0]['sourceScale'],nwScale=south[12]['sourceScale'])))
