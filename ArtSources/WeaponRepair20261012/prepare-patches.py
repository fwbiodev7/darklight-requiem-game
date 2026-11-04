from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json
root=Path(__file__).parent
helpers={}
exec((root.parent.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],helpers)
asset=root.parent/'Assets/Darklight/Resources/Darklight/AnimationCatalog/RequiemKatanaLightStage4.png'
doc=json.loads(asset.with_name(asset.stem+'Rects.json').read_text(encoding='utf-8-sig'))
im=Image.open(asset).convert('RGBA')
frames=[]; cw,ch=384,256
boards=[Image.new('RGBA',(cw*4,ch*4)) for _ in range(2)]
for i,f in enumerate(doc['frames']):
    x,y,w,h=f['rect']; tile=im.crop((x,y,x+w,y+h)); a=np.asarray(tile)
    red=(a[:,:,0]>140)&(a[:,:,1]<75)&(a[:,:,2]<75)&(a[:,:,3]>180)
    labels,n,_=helpers['components_for'](red); sizes=np.bincount(labels.ravel()); sizes[0]=0
    k=sizes.argmax(); yy,xx=np.nonzero(labels==k)
    assert len(xx)>30,(i,len(xx))
    # Technical crop includes the hilt/hand context and room for a slightly longer blade.
    px0,py0=max(-28,int(xx.min())-46),max(-28,int(yy.min())-42)
    px1,py1=min(w+28,int(xx.max())+47),min(h+28,int(yy.max())+43)
    pw,ph=px1-px0,py1-py0;assert pw<=cw-16 and ph<=ch-16,(i,pw,ph)
    patch=tile.crop((px0,py0,px1,py1)); j=i%16
    bx=j%4*cw+(cw-pw)//2; by=j//4*ch+(ch-ph)//2
    boards[i//16].alpha_composite(patch,(bx,by))
    frames.append(dict(index=i,atlasRect=[x+px0,y+py0,pw,ph],boardRect=[bx,by,pw,ph],cellRect=[j%4*cw,j//4*ch,cw,ch],bladeBox=[int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)]))
for n,b in enumerate(boards):b.save(root/('patches-'+('south' if n==0 else 'north')+'-input.png'))
(root/'patch-layout.json').write_text(json.dumps(dict(asset=str(asset),cell=[cw,ch],size=list(boards[0].size),frames=frames),indent=2),encoding='utf-8')
print(json.dumps(dict(frames=len(frames),boardSize=boards[0].size,maxPatchWidth=max(f['atlasRect'][2] for f in frames),maxPatchHeight=max(f['atlasRect'][3] for f in frames))))
