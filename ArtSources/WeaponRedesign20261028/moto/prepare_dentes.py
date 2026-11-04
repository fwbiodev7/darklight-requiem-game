"""Prepare only complete authored two-saw figures and a detached weapon3 page fragment."""
from pathlib import Path
import json
import sys
import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261028/moto'
STAGE=OUT/'staging'
NAME='ReqMotoNormalDentesSW_W20261028V1'
TIMES=[150,150,280,400]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART;pipeline.OUT=OUT/'qa';pipeline.RES=STAGE

def prepare_part(part,indices):
    job=dict(name=NAME+'-part'+str(part['id']),source=str(ART/part['source']),preservedSourceName=part['source'],
        skin=0,weapon=3,form=0,columns=4,sourceRows=2,minimumArea=9000,
        rows=[dict(sourceRow=i//4,sourceIndices=[i],direction=1+i//4,action='Attack',attackKey='Dentes Cruzados',loop=False) for i in indices],
        supportAnchors={str(i):dict(anchor=a,method='Inspected whole-figure boot-ground support; excludes saw tips.') for i,a in enumerate(part['anchors'])})
    normal=pipeline.identify
    def identify(image,**kwargs):
        kwargs['alpha_threshold']=part.get('alphaThreshold',128)
        figures,owner=normal(image,**kwargs)
        if not part.get('nearestSilhouetteOwnership'):return figures,owner
        # A box can overlap a neighboring weapon without the silhouettes touching.
        # Label complete connected silhouettes first, then propagate ownership by
        # actual pixel distance. Never modify, discard or recolor source RGBA.
        alpha=np.array(image.getchannel('A'));seeds=np.zeros(owner.shape,dtype=np.uint8)
        for i,figure in enumerate(figures):
            x0,y0,x1,y1=figure['grid'];mask=((owner[y0:y1,x0:x1]==i+1)&(alpha[y0:y1,x0:x1]>=kwargs['alpha_threshold']))
            yy,xx=np.where(mask);rank=np.argsort((xx-(x1-x0)/2)**2+(yy-(y1-y0)/2)**2)
            main=None
            for k in rank[:32]:
                region=Image.fromarray((mask*255).astype(np.uint8)).copy()
                ImageDraw.floodfill(region,(int(xx[k]),int(yy[k])),128,border=0)
                candidate=np.array(region)==128
                if np.count_nonzero(candidate)>9000:main=candidate;break
            if main is None:raise ValueError('Could not seed one complete connected figure')
            seeds[y0:y1,x0:x1][main]=i+1
        nonzero=alpha>0
        for distance in range(max(image.size)):
            if np.all(seeds[nonzero]>0):break
            padded=np.pad(seeds,1);neighbors=np.zeros_like(seeds)
            for dy in range(3):
                for dx in range(3):
                    if dx==1 and dy==1:continue
                    neighbors=np.maximum(neighbors,padded[dy:dy+image.height,dx:dx+image.width])
            empty=seeds==0;seeds[empty]=neighbors[empty]
        else:raise ValueError('Could not assign every selected source RGBA pixel')
        return figures,seeds
    pipeline.identify=identify
    try:manifest,_=pipeline.prepare(job)
    finally:pipeline.identify=normal
    pipeline.write(ART/(job['name']+'-job.json'),job)
    return manifest

def main():
    cfg=json.loads((ART/'dentes-config.json').read_text())
    sources={p['id']:p for p in cfg['sources']}
    parts={key:prepare_part(part,sorted({v['index'] for v in cfg['selection'] if v['source']==key})) for key,part in sources.items()}
    pieces=[]
    for selection in cfg['selection']:
        m=parts[selection['source']];f=next(f for f in m['frames'] if f['sourceFrameIndex']==selection['index'])
        image=Image.open(STAGE/m['image']).convert('RGBA');x,y,w,h=f['rect'];full=image.crop((x,y,x+w,y+h))
        bx,by,ex,ey=full.getbbox();piece=full.crop((bx,by,ex,ey))
        support=[round(f['pivot'][0]*w)-bx,round((1-f['pivot'][1])*h)-by]
        pieces.append((piece,support,m,f,[bx,by],selection))
    left=max(p[1][0] for p in pieces)+8;top=max(p[1][1] for p in pieces)+8
    right=max(p[0].width-p[1][0] for p in pieces)+8;bottom=max(p[0].height-p[1][1] for p in pieces)+8
    cw,ch=left+right,top+bottom
    atlas=Image.new('RGBA',(cw*4,ch*2));frames=[];proof=[]
    for i,(piece,support,m,old,bounds,selection) in enumerate(pieces):
        dx,dy=left-support[0],top-support[1];x,y=(i%4)*cw,(i//4)*ch
        atlas.paste(piece,(x+dx,y+dy));before=np.array(piece);mask=before[:,:,3]>0
        actual=np.array(atlas.crop((x+dx,y+dy,x+dx+piece.width,y+dy+piece.height)))
        if not np.array_equal(before[mask],actual[mask]):raise ValueError('Changed selected whole-figure RGBA')
        f=dict(old);f.update(id='Redesign_'+NAME+'_%02d'%i,rect=[x,y,cw,ch],pivot=[left/cw,bottom/ch],
            durationMs=TIMES[i%4],direction=['SW','W'][i//4],action='attack',attackKey='Dentes Cruzados',
            phase=['preparation','coil','crossed_contact','recovery'][i%4],sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],
            sourceOffsetInFrame=[old['sourceOffsetInFrame'][0]-bounds[0]+dx,old['sourceOffsetInFrame'][1]-bounds[1]+dy])
        frames.append(f);proof.append(dict(index=i,sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],
            sourceFrameIndex=old['sourceFrameIndex'],sourceRect=old['sourceRect'],wholeFrame=True,
            exactSelectedNonzeroAlphaRgba=True,preservedOriginal=selection['source']==0))
    target=STAGE/(NAME+'.png');atlas.save(target)
    manifest=dict(parts[cfg['selection'][0]['source']]);status='visual-reviewed' if cfg.get('visualReviewed') else 'candidate'
    clips=[dict(name='DentesCruzados_'+direction,frames=[f['id'] for f in frames[row*4:row*4+4]],
        attackKey='Dentes Cruzados',loop=False,allowStatic=False) for row,direction in enumerate(['SW','W'])]
    manifest.update(image=target.name,imageSha256=pipeline.sha(target),nativeSize=list(atlas.size),
        frames=frames,clips=clips,requiredClips=[c['name'] for c in clips],status=status,
        pixelsPerUnit=cfg['calibration']['pixelsPerUnit'],pixelsPerUnitCalibration=cfg['calibration'],worldVisualScale=1.325,
        sourcePixels=int(np.count_nonzero(np.array(atlas)[:,:,3])),packedPixels=int(np.count_nonzero(np.array(atlas)[:,:,3])),
        sourceRgbaExactReconstruction=True,wholeFrameRepairProof=proof,
        additionalSources=[dict(path=m['sourcePath'],sha256=m['sourceSha256'],sourceSize=m['sourceSize']) for m in parts.values()],
        motorcycleDualSawsAttack=dict(skin=0,weapon=3,form=0,attackKey='Dentes Cruzados',onFoot=True,mounted=False,
            directionFrames={d:c['frames'] for d,c in zip(['SW','W'],clips)},durationMs=TIMES,events=[],
            phaseTiming=dict(startupMs=300,activeMs=280,recoveryMs=400)),
        review=dict(wholeSourceSeen=True,contactSeen=cfg.get('visualReviewed',False),livePlaybackObserved=False,
            decodedGifPhasesSeen=cfg.get('decodedGifPhasesSeen',False),humanApproval=False,unityImported=False,runtimeVerified=False),
        notes=['Dentes Cruzados authored complete figures, SW/W only. Exactly two hand-held wheel-saw modules, on foot.',
            'Timed Attack page durationMs [150,150,280,400] from coordinator real startup300/active280/recovery400ms; no legacy aliases.',
            'No anatomy assembly, mirroring, per-frame resizing, recoloring or global alpha deletion.',
            'Native generated raster; indexed pixel grid unverified. Unity import/gameplay events remain coordinator-owned.'])
    pipeline.write(STAGE/(NAME+'.json'),manifest);pipeline.write(OUT/(NAME+'-preservation-whole-frames.json'),proof)
    pages=[]
    for row,direction in enumerate([1,2]):
        pages.append(dict(id=NAME+'_Attack_'+['SW','W'][row],skin=0,weapon=3,form=0,direction=direction,
            action='Attack',attackKey='Dentes Cruzados',resourcePath='Darklight/WeaponRedesign20261023/'+NAME,
            imageSha256=manifest['imageSha256'],sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],
            sourceSize=manifest['sourceSize'],nativeSize=manifest['nativeSize'],pixelsPerUnit=manifest['pixelsPerUnit'],
            status=status,wholeFigures=True,loop=False,spriteNames=clips[row]['frames'],sourceSpriteNames=[],durationMs=TIMES))
    pipeline.write(OUT/(NAME+'-pages.json'),dict(schemaVersion=1,complete=False,pages=pages))
    qa=pipeline.qa(NAME)
    receipt=dict(name=NAME,figures=8,pages=2,attackKey='Dentes Cruzados',directions=['SW','W'],durationMs=TIMES,
        imageSha256=manifest['imageSha256'],sourceSha256=manifest['sourceSha256'],nativeSize=manifest['nativeSize'],
        pixelsPerUnit=manifest['pixelsPerUnit'],qa=qa,status=status,stagingOnly=True,imported=False,runtimeVerified=False)
    pipeline.write(OUT/'dentes-prepared.json',receipt);print(json.dumps(receipt))

if __name__=='__main__':main()
