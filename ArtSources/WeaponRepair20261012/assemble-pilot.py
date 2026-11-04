"""Technical masked assembly of imagegen weapon patches; no drawn/recolored art."""
from pathlib import Path
from PIL import Image,ImageDraw
import numpy as np,json,hashlib,shutil
root=Path(__file__).parent
layout=json.loads((root/'patch-layout.json').read_text())
asset=Path(layout['asset']); manifest=asset.with_name(asset.stem+'Rects.json')
original=np.asarray(Image.open(asset).convert('RGBA')).copy(); result=original.copy()
doc=json.loads(manifest.read_text(encoding='utf-8-sig'))
sources=['exec-cd745346-60fe-4f0b-a655-231e236f6966.png','exec-e5792583-25ae-4eb4-9b35-53061492af10.png']
generated=Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')
patches=[np.asarray(Image.open(generated/f).convert('RGBA')).copy() for f in sources]
inputs=[np.asarray(Image.open(root/f'patches-{d}-input.png').convert('RGBA')) for d in ['south','north']]
helpers={};exec((root.parent.parent/'sprite_audit.py').read_text(encoding='utf-8').split('ROOT =')[0],helpers)
assert all(p.shape==inputs[0].shape for p in patches)

def dilate(a,r):
    h,w=a.shape;p=np.pad(a,r);out=a.copy()
    for y in range(2*r+1):
        for x in range(2*r+1):out|=p[y:y+h,x:x+w]
    return out

def blade_points(a):
    red=(a[:,:,0]>140)&(a[:,:,1]<75)&(a[:,:,2]<75)&(a[:,:,3]>166)
    labels,n,_=helpers['components_for'](red);sizes=np.bincount(labels.ravel());sizes[0]=0
    yy,xx=np.nonzero(labels==sizes.argmax());assert len(xx)>25
    return np.column_stack((xx,yy))

def hull_mask(points,size):
    pts=sorted(set(map(tuple,points)))
    def cross(a,b,c):return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
    lower=[];upper=[]
    for p in pts:
        while len(lower)>=2 and cross(lower[-2],lower[-1],p)<=0:lower.pop()
        lower.append(p)
    for p in pts[::-1]:
        while len(upper)>=2 and cross(upper[-2],upper[-1],p)<=0:upper.pop()
        upper.append(p)
    im=Image.new('1',size);ImageDraw.Draw(im).polygon(lower[:-1]+upper[:-1],fill=1)
    return np.asarray(im,dtype=bool)

changes=[]; all_selection=np.zeros(original.shape[:2],bool)
for f in layout['frames']:
    i=f['index']; bx,by,pw,ph=f['boardRect']; x,y,_,_=f['atlasRect']
    old=inputs[i//16][by:by+ph,bx:bx+pw].copy(); new=patches[i//16][by:by+ph,bx:bx+pw].copy()
    op=blade_points(old);np_=blade_points(new)
    selection=dilate(hull_mask(op,(pw,ph))|hull_mask(np_,(pw,ph)),7)
    # Extend the assembly selection to the guard nearest the original hand.
    center=op.mean(axis=0);_,_,vh=np.linalg.svd(op-center,full_matrices=False)
    projection=(op-center)@vh[0];ends=[op[projection.argmin()],op[projection.argmax()]]
    yy,xx=np.indices((ph,pw))
    scores=[]
    for ex,ey in ends:
        near=(xx-ex)**2+(yy-ey)**2<24**2
        scores.append(int(((old[:,:,3]>166)&near).sum()))
    hx,hy=ends[int(np.argmax(scores))]
    selection|=(xx-hx)**2+(yy-hy)**2<17**2
    # Original warm skin/hair pixels remain exact even where a guard overlaps them.
    r,g,b=old[:,:,0].astype(float),old[:,:,1].astype(float),old[:,:,2].astype(float)
    protected=(old[:,:,3]>0)&(g>30)&(r>g*1.12)&(g>b*1.06)
    # Saturated red sword pixels are not skin: exclude the original blade contour.
    protected&=~dilate(hull_mask(op,(pw,ph)),2)
    selection&=~protected
    # Extract actual authored fragment coverage, excluding low-alpha preview halos.
    coverage=dilate(new[:,:,3]>=166,1)
    new[~coverage]=0
    assembled=old.copy();assembled[selection]=new[selection]
    assert np.array_equal(assembled[protected],old[protected])
    assert np.array_equal(assembled[~selection],old[~selection])
    result[y:y+ph,x:x+pw]=assembled
    all_selection[y:y+ph,x:x+pw]|=selection
    changed=np.any(assembled!=old,axis=2)
    changes.append(dict(index=i,patchRect=f['atlasRect'],guardAnchor=[int(hx),int(hy)],changedPixels=int(changed.sum()),protectedSkinHairPixels=int(protected.sum())))
    Image.fromarray(assembled).save(root/f'patch-{i:02}-assembled.png')

assert np.array_equal(result[~all_selection],original[~all_selection])
out=root/'pilot';out.mkdir(exist_ok=True)
shutil.copy2(asset,out/'RequiemKatanaLightStage4-before.png')
shutil.copy2(manifest,out/'RequiemKatanaLightStage4-beforeRects.json')
# A tip needs six extra pixels of clearance. Pad cells rather than resize artwork.
packed=np.zeros((2944,2304,4),dtype=np.uint8)
for i in range(32):
    cx,cy=i%4*560,i//4*368;dx=i%4*576+8
    packed[cy:cy+368,dx:dx+560]=result[cy:cy+368,cx:cx+560]
Image.fromarray(packed).save(out/asset.name)
# Keep each foot's world anchor while widening rectangles for repaired tips/guards.
for i,f in enumerate(doc['frames']):
    x,y,w,h=f['rect'];px,py=f['pivot'];anchorX=x+px*w;anchorY=y+(1-py)*h
    cx,cy=i%4*560,i//4*368
    cell=result[cy:cy+368,cx:cx+560];yy,xx=np.nonzero(cell[:,:,3]>0)
    nx,ny=cx+int(xx.min()),cy+int(yy.min());nw,nh=int(xx.max()-xx.min()+1),int(yy.max()-yy.min()+1)
    shifted=nx+i%4*16+8
    f['rect']=[shifted,ny,nw,nh];f['pivot']=[(anchorX-nx)/nw,1-(anchorY-ny)/nh]
    assert shifted-i%4*576>=24 and (i%4+1)*576-(shifted+nw)>=24 and ny-cy>=12
doc['weaponRepair']=dict(reference='WeaponsCanonicalUser.png',sourcePatches=sources,method='imagegen local weapon retouch + masked technical atlas assembly',preserved='all pixels outside weapon/guard assembly selections; original skin/hair within selections',approval='user 12/10: faz isso entao',frames=changes)
(out/manifest.name).write_text(json.dumps(doc,indent=2),encoding='utf-8')
report=dict(frames=32,changedPixels=int(np.any(result!=original,axis=2).sum()),unchangedOutsideSelections=True,skinHairProtected=True,pixelsPerUnit=doc['pixelsPerUnit'],frameNamesPreserved=True,originalSha256=hashlib.sha256(asset.read_bytes()).hexdigest(),patchedSha256=hashlib.sha256((out/asset.name).read_bytes()).hexdigest(),framesDetails=changes)
(root/'pilot-assembly-report.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='framesDetails'}))
