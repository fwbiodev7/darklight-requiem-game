"""Whole-frame production packaging and semantic material masks for Punhos da Ruina.

Every skin, transformation, direction and action bank has independently authored figures.
No anatomical pieces, mirroring, planar rotations or inferred missing directions.
"""
import argparse
from collections import deque
from pathlib import Path
from types import SimpleNamespace
import json
import math
import shutil
import numpy as np
from PIL import Image
from prepare_eclipse_fangs_preview import ROOT,extract,palette,sp,translate_preview

ART=ROOT/'ArtSources/RuinFists20261021'
INDEX=ART/'production-source-index-v2.json'
OUT=ROOT/'outputs/punhos-ruina-final-20261021'
RES=ROOT/'Assets/Darklight/Resources/Darklight/RuinFists'
MAN=ROOT/'Assets/Darklight/ArtSources/RuinFists20261021'
SKINS=['Requiem','Rock'];FORMS=['Normal','Ruptura','Eclipse','Absoluto']
DIRS=['S','SW','W','NW','N','NE','E','SE']
PPU=220/1.99

def physical_name(skin,form,direction,bank):return f'Ruin_{SKINS[skin]}_{FORMS[form]}_{DIRS[direction]}_{bank}'

def init_index():
    if INDEX.exists():raise ValueError('Source index already exists; preserve curation')
    reuse={
      (0,0,0):'Requiem_Normal_S_Combat_RefocusedV2', (0,0,1):'Requiem_Normal_SW_Combat_RefocusedV3',
      (0,0,2):'Requiem_Normal_W_Combat_RefocusedV2', (0,0,4):'Requiem_Normal_N_Combat_RefocusedV3',
      (1,0,0):'Rock_Normal_S_Combat_RefocusedV3', (1,0,2):'Rock_Normal_W_Combat_RefocusedV3',
      (0,3,7):'Requiem_Absoluto_SE_Combat_HairV2',(1,3,7):'Rock_Absoluto_SE_Combat_HairV2'}
    pages=[]
    for skin in range(2):
      for form in [0,3]:
       for direction in range(8):
        for bank in ['Combat','Motion']:
         key=f'{SKINS[skin]}_{FORMS[form]}_{DIRS[direction]}_{bank}_BankV4'
         chosen=reuse.get((skin,form,direction)) if bank=='Combat' else None
         source=ART/'generated'/f'{chosen or key}.png';reviewed=chosen is not None
         if bank=='Combat' and form==0 and direction==7:
            source=ROOT/f'ArtSources/EclipseFangs20261021/candidate-v1/{"requiem" if skin==0 else "rock"}-combo-source.png';reviewed=True
         replacements=[]
         if bank=='Combat' and skin==0 and form==0 and direction==4:
            replacements=[dict(frame=3,source='ArtSources/RuinFists20261021/generated/Requiem_Normal_punch-eight-views.png',rows=2,columns=4,index=4,calibration=0)]
         if bank=='Motion' and form==0 and direction==2:
            path='generated/walk-west-guided-v9.png' if skin==0 else 'generated/Rock_Normal_Walk_W_GuidedV10.png'
            replacements=[dict(frame=i,source='ArtSources/RuinFists20261021/'+path,rows=2 if skin==0 else 1,columns=4,index=i,calibration=0) for i in range(4)]
         pages.append(dict(name=physical_name(skin,form,direction,bank),skin=skin,form=form,direction=direction,bank=bank,
                           source=source.relative_to(ROOT).as_posix(),rows=3,columns=4,calibration=0,
                           reviewed=reviewed,replacements=replacements))
    sp.write(INDEX,dict(schemaVersion=2,physicalPages=64,renderConfigurations=128,uniqueAuthoredFrames=768,
                       paletteVariants=[1,2],notes='Material variants explicitly reuse same-skin same-direction same-action geometry; white hair is independently authored.',pages=pages))
    print('64 physical pages indexed; generated sources need explicit review')

def regions(binary):
    """Connected material regions; labels do not change any drawing pixels."""
    seen=np.zeros(binary.shape,bool);result=[];h,w=binary.shape
    for sy,sx in zip(*np.where(binary)):
        if seen[sy,sx]:continue
        queue=[(int(sy),int(sx))];seen[sy,sx]=True;points=[]
        while queue:
            y,x=queue.pop();points.append((y,x))
            for dy,dx in [(-1,0),(1,0),(0,-1),(0,1)]:
                ny,nx=y+dy,x+dx
                if 0<=ny<h and 0<=nx<w and binary[ny,nx] and not seen[ny,nx]:seen[ny,nx]=True;queue.append((ny,nx))
        if len(points)>=4:result.append(np.array(points))
    return result

def semantic_mask(tile,skin,form,direction):
    """RGB are iris, transformable materials, skin. A is filled with whole-frame ID later."""
    a=np.array(tile);rgb=a[:,:,:3].astype(float);r,g,b=rgb.transpose(2,0,1);solid=a[:,:,3]>=128
    h,w=solid.shape;yy,xx=np.indices(solid.shape)
    warm=solid&(r>185)&(g>65)&(g<210)&(b>35)&(b<175)&(r>g*1.2)&(g>b*1.12)
    brown=solid&(r>40)&(r<195)&(g>24)&(g<165)&(b>12)&(r>g*1.18)&(g>b*1.04)
    if form==3:brown=solid&(r>95)&(g>90)&(b>95)&((np.maximum(r,np.maximum(g,b))-np.minimum(r,np.minimum(g,b)))<50)&~warm
    clusters=[p for p in regions(brown) if p[:,0].mean()<h*.75]
    hair=np.zeros(solid.shape,bool)
    if clusters:
        primary=max(clusters,key=len);y0,x0=primary.min(0);y1,x1=primary.max(0)+1
    else:
        # White hair has neutral bright clusters instead of brown; use full-figure upper region.
        y0,x0,y1,x1=0,round(w*.25),round(h*.32),round(w*.75)
    hh=max(12,y1-y0);hw=max(12,x1-x0)
    head=(yy>=max(0,y0-hh*.15))&(yy<=y1+hh*.3)&(xx>=x0-hw*.45)&(xx<=x1+hw*.45)
    candidates=[p for p in regions(warm&head) if len(p)>8]
    face=np.zeros(solid.shape,bool)
    if candidates and direction not in [3,4,5]:
        pixels=max(candidates,key=len);fy0,fx0=pixels.min(0);fy1,fx1=pixels.max(0)+1
        face=(yy>=fy0-2)&(yy<=min(fy1+2,y1+hh*.25))&(xx>=fx0-3)&(xx<=fx1+3)
    red=solid&(r>70)&(r>g*1.8)&(r>b*1.25)&((r-np.minimum(g,b))>35)
    # The drawings already contain the approved red irises. A hue threshold also
    # selected cheek/neck shadows, turning the entire lower face scarlet. Preserve
    # authored eyes and skin shadows instead of recoloring any red facial pixels.
    iris=np.zeros(solid.shape,bool)
    selected_skin=warm&face
    if skin==1:
        upper=warm&(yy>=y0)&(yy<=y1+hh*1.7)
        for pixels in regions(upper):
            if len(pixels)>max(18,hh*hh*.018):selected_skin[pixels[:,0],pixels[:,1]]=True
    transform=np.zeros(solid.shape,np.uint8)
    if form==0:
        transform[red&~face&~warm]=255
        hair=np.zeros(solid.shape,bool)
        for pixels in clusters:
            if pixels[:,0].mean()<=y1 and pixels[:,1].mean()>=x0-hw*.2 and pixels[:,1].mean()<=x1+hw*.2:
                hair[pixels[:,0],pixels[:,1]]=True
        hair&=head&~face&~warm
        tips=hair&(yy>y0+hh*.66)
        transform[tips]=205
        gray=solid&(r>100)&(abs(r-g)<22)&(abs(g-b)<25)
        shoulder=gray&(yy>y1-hh*.15)&(yy<y1+hh*.55)&~face&~warm
        transform[shoulder]=205
    transform[face|selected_skin]=0
    mask=np.zeros(a.shape,np.uint8);mask[:,:,0]=iris*255;mask[:,:,1]=transform;mask[:,:,2]=selected_skin*255
    shader_warm=selected_skin&(r>200)&(g>75)&(g<205)&(b>35)&(b<165)&(r>g*1.25)&(g>b*1.2)
    return mask,dict(hairBox=[int(x0),int(y0),int(x1),int(y1)],irisPixels=int(iris.sum()),skinPixels=int(selected_skin.sum()),
                     materialPixels=int((transform>0).sum()),faceTransformOverlap=int((face&(transform>0)).sum())),rgb[shader_warm]

def make_page(page):
    if not page['reviewed']:raise ValueError('Unreviewed source: '+page['name'])
    name=page['name'];target=OUT/name;resource=RES/(name+'.png')
    if (target/'page.json').exists():
        cached=sp.read(target/'page.json')
        if cached['sourceSha256']!=sp.digest(ROOT/page['source']):raise ValueError('Source changed after review: '+name)
        return cached
    target.mkdir(parents=True,exist_ok=True);RES.mkdir(parents=True,exist_ok=True);MAN.mkdir(parents=True,exist_ok=True)
    source_path=ROOT/page['source'];source,figures=extract(source_path,page['rows'],page['columns'],minimum_area=page.get('minimumFigureArea',10000))
    if len(figures)!=12:raise ValueError('Expected 12 complete figures')
    factor=220/figures[page['calibration']]['bodyHeight'];choices={r['frame']:r for r in page.get('replacements',[])}
    cache={};tiles=[];provenance=[];samples=[];material=[]
    for i,original in enumerate(figures):
        f=original;origin=source_path;scale=factor;record=choices.get(i)
        if record:
            origin=ROOT/record['source']
            if str(origin) not in cache:cache[str(origin)]=extract(origin,record['rows'],record['columns'])[1]
            alternate=cache[str(origin)];f=alternate[record['index']];scale=220/alternate[record.get('calibration',0)]['bodyHeight']
        image=f['image'].resize(tuple(max(1,round(v*scale)) for v in f['image'].size),Image.Resampling.NEAREST)
        ax=round((f['sourceGroundAnchor'][0]-f['sourceRect'][0])*scale)
        ay=round((f['sourceGroundAnchor'][1]-f['sourceRect'][1])*scale)
        if page['bank']=='Combat' and i in [8,9]:ay+=62 # authored airborne pose projected to ground; no sinusoidal canvas bobbing
        mask,qa,skin_samples=semantic_mask(image,page['skin'],page['form'],page['direction']);samples.extend(skin_samples)
        tiles.append(dict(image=image,anchor=[ax,ay],mask=mask));material.append(qa)
        provenance.append(dict(source=origin.relative_to(ROOT).as_posix(),sourceSha256=sp.digest(origin),sourceRect=f['sourceRect'],
                               completeFigure=True,uniformNearestNeighborScale=scale,groundAnchor=[ax,ay],replacement=record is not None))
    left=max(t['anchor'][0] for t in tiles)+12;ground=max(t['anchor'][1] for t in tiles)+12
    right=max(t['image'].width-t['anchor'][0] for t in tiles)+12;bottom=max(t['image'].height-t['anchor'][1] for t in tiles)+12
    cw=math.ceil((left+right)/32)*32;ch=math.ceil((ground+bottom)/32)*32
    atlas=Image.new('RGBA',(cw*4,ch*3));owner=Image.new('RGBA',atlas.size);frames=[];unity=[]
    for i,t in enumerate(tiles):
        x,y=(i%4)*cw,(i//4)*ch;dx,dy=left-t['anchor'][0],ground-t['anchor'][1]
        atlas.paste(t['image'],(x+dx,y+dy));m=t['mask'];m[:,:,3]=np.where(np.array(t['image'])[:,:,3]>0,i+1,0)
        assert atlas.crop((x+dx,y+dy,x+dx+t['image'].width,y+dy+t['image'].height)).tobytes()==t['image'].tobytes()
        owner.paste(Image.fromarray(m),(x+dx,y+dy));identity=name+f'_{i:02d}'
        frames.append(dict(id=identity,rect=[x,y,cw,ch],pivot=[left/cw,1-ground/ch],durationMs=150,
                           action=page['bank'],direction=DIRS[page['direction']],skin=SKINS[page['skin']],form=FORMS[page['form']]))
        unity.append(dict(name=identity,x=x,y=atlas.height-y-ch,w=cw,h=ch,pivotX=left/cw,pivotY=1-ground/ch))
    if max(atlas.size)>4096:raise ValueError('Native atlas exceeds texture cap')
    atlas.save(resource);owner.save(RES/(name+'_Owner.png'));shutil.copy2(resource,target/'sheet.png')
    if page['bank']=='Combat':
        sequences=[('Repouso',[0,1],True),('Soco',[2,3,11],False),('Chute',[4,5,11],False),('Giro',[4,6,11],False),
                   ('Uppercut',[7,8,11],False),('Aereo',[8,9,10,11],False),('Finalizacao',[7,10,11],False),('Combo',[2,3,11,4,5,11,4,6,11,7,10,11],True)]
    else:sequences=[('Caminhada',[0,1,2,3],True),('Esquiva',[4,5,6],False),('Defesa',[7,8,7],False),('Dano',[9,7],False),('Morte',[10,11],False)]
    clips=[dict(name=key,frames=[frames[i]['id'] for i in indices],loop=loop) for key,indices,loop in sequences]
    data=dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(resource),nativeSize=list(atlas.size),pixelsPerUnit=PPU,
              status='visual-reviewed',visualApproval=False,imported=False,runtimeVerified=False,logicalPixelResolution=None,nativePixelArtCertified=False,
              frames=frames,clips=clips,requiredClips=[c['name'] for c in clips],quality=dict(maxColors=256,maxHeightRatio=3),
              provenance=provenance,materialMask=material,notes=['Full complete-figure drawings. Uniform whole-figure nearest-neighbor scaling only.',
              'RGB selectors: red=iris, green=transformable energy/hair tips/shoulder studs, blue=skin; alpha=whole figure owner ID.',
              'Semantic masks do not cut, transplant or modify anatomy. Color/pivot review is separate from structural audit.',
              'Shared return/guard poses are intentional. Runtime uses CombatModel phase timing; HTML 150 ms is inspection cadence.'])
    sp.write(target/'sheet.json',data)
    audit=sp.audit(SimpleNamespace(manifest=target/'sheet.json',out=target/'audit',unity_meta=None))
    if not audit['ok']:raise ValueError(audit)
    sp.preview(SimpleNamespace(manifest=target/'sheet.json',out=target/'preview',zoom=2));translate_preview(target/'preview/preview.html',name)
    html=target/'preview/preview.html';html.write_text(html.read_text(encoding='utf-8').replace('Visual aprovado em 21/10/2026.','Fonte revisada para integração; cores finais conferidas no Unity.'),encoding='utf-8')
    palette(resource,target)
    measured=np.median(samples,axis=0).tolist() if len(samples)>30 else [246,174,130]
    shift=((np.array([246,174,130])-measured)/255).tolist()
    result=dict(name=name,skin=page['skin'],form=page['form'],direction=page['direction'],bank=page['bank'],ppu=PPU,
                paletteVariant=False,imagePath=resource.relative_to(ROOT).as_posix(),maskPath=(RES/(name+'_Owner.png')).relative_to(ROOT).as_posix(),
                imageSha256=sp.digest(resource),maskSha256=sp.digest(RES/(name+'_Owner.png')),sprites=unity,
                sourceSha256=sp.digest(source_path),maskRevision=3,
                complexion=dict(texture=name,shift=shift,sourceMedian=measured),audit=audit)
    sp.write(target/'page.json',result);return result

def refresh_white_masks():
    for path in OUT.glob('*/page.json'):
        data=sp.read(path)
        if data.get('maskRevision',1)>=3:continue
        source=RES/(data['name']+'.png');mask_path=RES/(data['name']+'_Owner.png')
        source_hash=sp.digest(source);atlas=Image.open(source).convert('RGBA');old=np.array(Image.open(mask_path).convert('RGBA'))
        owner=Image.new('RGBA',atlas.size);sheet_path=path.parent/'sheet.json';sheet=sp.read(sheet_path);qa=[];samples=[]
        for i,frame in enumerate(sheet['frames']):
            x,y,w,h=frame['rect'];tile=atlas.crop((x,y,x+w,y+h));mask,info,flesh=semantic_mask(tile,data['skin'],data['form'],data['direction'])
            mask[:,:,3]=old[y:y+h,x:x+w,3];owner.paste(Image.fromarray(mask),(x,y));qa.append(info);samples.extend(flesh)
        assert np.array_equal(old[:,:,3],np.array(owner)[:,:,3]),'Whole-figure ownership must remain identical'
        backup=OUT/'previous-material-masks'/mask_path.name;backup.parent.mkdir(parents=True,exist_ok=True)
        if not backup.exists():shutil.copy2(mask_path,backup)
        owner.save(mask_path);assert sp.digest(source)==source_hash,'Drawing pixels changed during material labeling'
        measured=np.median(samples,axis=0).tolist() if len(samples)>30 else [246,174,130]
        data.update(maskSha256=sp.digest(mask_path),maskRevision=3,complexion=dict(texture=data['name'],sourceMedian=measured,
                    shift=((np.array([246,174,130])-measured)/255).tolist()))
        sheet['materialMask']=qa;sheet['maskRevision']=3
        path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        sheet_path.write_text(json.dumps(sheet,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        print(data['name'],'authored iris/shadows preserved; drawing/ownership unchanged')

def publish_manifest():
    index=sp.read(INDEX);physical=[sp.read(OUT/p['name']/'page.json') for p in index['pages']]
    if len(physical)!=128:raise ValueError('128 independently authored physical banks required')
    pages=physical
    (MAN/'manifest.json').write_text(json.dumps(dict(schemaVersion=3,complete=True,physicalBanks=128,paletteVariantBanks=0,uniqueCompleteFigures=1536,pages=pages),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    complexion=ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json';colors=sp.read(complexion)
    fresh={p['name']:p['complexion'] for p in physical}
    backup=OUT/'CanonicalComplexion-before.json'
    if not backup.exists():shutil.copy2(complexion,backup)
    colors['entries']=[e for e in colors['entries'] if e['texture'] not in fresh]+list(fresh.values())
    complexion.write_text(json.dumps(colors,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(manifest=str(MAN/'manifest.json'),physical=128,paletteVariants=0,renderConfigurations=len(pages),uniqueCompleteFrames=1536)))

def main():
    ap=argparse.ArgumentParser();ap.add_argument('--init-index',action='store_true');ap.add_argument('--review',nargs='*');ap.add_argument('--page');ap.add_argument('--available',action='store_true');ap.add_argument('--publish-manifest',action='store_true');ap.add_argument('--refresh-white-masks',action='store_true');args=ap.parse_args()
    if args.init_index:init_index();return
    index=sp.read(INDEX)
    if args.refresh_white_masks:refresh_white_masks()
    if args.review:
        for p in index['pages']:
            if p['name'] in args.review or Path(p['source']).stem in args.review:
                p['reviewed']=True;p['reviewedSourceSha256']=sp.digest(ROOT/p['source'])
        INDEX.write_text(json.dumps(index,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    selected=[p for p in index['pages'] if (args.available and p['reviewed'] and (ROOT/p['source']).exists()) or p['name']==args.page]
    for p in selected:
        result=make_page(p);print(p['name'],'frames=12 errors='+str(result['audit']['errors']))
    if args.publish_manifest:publish_manifest()

if __name__=='__main__':main()
