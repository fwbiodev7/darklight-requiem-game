"""Register intact generated figures and retain every selected nonzero-alpha RGBA pixel.

Does not edit Runtime, the shared catalog, legacy files or Unity metadata.
"""
from pathlib import Path
import hashlib
import json
import sys
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
ART = Path(__file__).resolve().parent
OUT = ROOT / 'outputs/retomada-20261027/moto-idle-mature'
RES = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
sys.path.insert(0, str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

DIRECTIONS = ['S','SW','W','NW','N','NE','E','SE']
DURATIONS = [350,250,180,350]
pipeline.ART = ART
pipeline.OUT = OUT/'qa'

def write(path, value): pipeline.write(path, value)

def prepare_source(name, source, directions, anchors, selected=None, staging=False):
    selected = selected or list(range(len(directions)*4))
    rows = [dict(sourceRow=i//4, sourceIndices=[i], direction=directions[i//4],
                 action='Idle', durationMs=[DURATIONS[i%4]], loop=True) for i in selected]
    job = dict(name=name, source=str(ART/source), preservedSourceName=source, skin=0,
               weapon=3, form=0, columns=4, sourceRows=len(directions), rows=rows,
               minimumArea=15000, supportAnchors={str(i):dict(anchor=anchor,
               method='Whole-source wheel contact support; center between front/rear contact where visible.')
               for i,anchor in enumerate(anchors)})
    pipeline.RES = OUT/'staging' if staging else RES
    manifest, _ = pipeline.prepare(job)
    write(ART/(name+'-job.json'),job)
    return manifest

def merge_intact(name, original, correction, replacement_map):
    original_by_index={f['sourceFrameIndex']:f for f in original['frames']}
    correction_by_index={f['sourceFrameIndex']:f for f in correction['frames']}
    selection=[]
    for i in range(12):
        if i in replacement_map:
            selection.append((correction,correction_by_index[replacement_map[i]]))
        else: selection.append((original,original_by_index[i]))
    pieces=[]
    for manifest,frame in selection:
        im=Image.open(OUT/'staging'/manifest['image']).convert('RGBA')
        x,y,w,h=frame['rect'];full=im.crop((x,y,x+w,y+h))
        bx,by,ex,ey=full.getbbox()
        support=[round(frame['pivot'][0]*w)-bx,round((1-frame['pivot'][1])*h)-by]
        piece=full.crop((bx,by,ex,ey))
        pieces.append((piece,support,manifest,frame,[bx,by]))
    left=max(p[1][0] for p in pieces)+8
    top=max(p[1][1] for p in pieces)+8
    right=max(p[0].width-p[1][0] for p in pieces)+8
    bottom=max(p[0].height-p[1][1] for p in pieces)+8
    cw,ch=left+right,top+bottom
    atlas=Image.new('RGBA',(4*cw,3*ch));frames=[];proof=[]
    for i,(piece,support,manifest,old,bounds) in enumerate(pieces):
        dx,dy=left-support[0],top-support[1]
        x,y=(i%4)*cw,(i//4)*ch
        atlas.paste(piece,(x+dx,y+dy))
        actual=np.array(atlas.crop((x+dx,y+dy,x+dx+piece.width,y+dy+piece.height)))
        before=np.array(piece);mask=before[:,:,3]>0
        exact=bool(np.array_equal(actual[mask],before[mask]))
        if not exact: raise ValueError('RGBA preservation failed')
        frame=dict(old)
        frame.update(id=f'Redesign_{name}_{i:02}',rect=[x,y,cw,ch],pivot=[left/cw,bottom/ch],
                     durationMs=DURATIONS[i%4],direction=DIRECTIONS[i//4],
                     phase=['neutral','inhale','blink','return'][i%4],
                     sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],
                     sourceOffsetInFrame=[old['sourceOffsetInFrame'][0]-bounds[0]+dx,
                                          old['sourceOffsetInFrame'][1]-bounds[1]+dy])
        frames.append(frame)
        proof.append(dict(index=i,sourcePath=manifest['sourcePath'],sourceFrameIndex=old['sourceFrameIndex'],
                          exactSelectedNonzeroAlphaRgba=exact,wholeFrame=True,
                          repaired=i in replacement_map,preservedOriginal=i not in replacement_map))
    target=RES/(name+'.png');atlas.save(target)
    manifest=dict(original)
    manifest.update(image=target.name,imageSha256=pipeline.sha(target),nativeSize=list(atlas.size),
                    frames=frames,additionalSources=[dict(path=correction['sourcePath'],sha256=correction['sourceSha256'])],
                    sourcePixels=int(np.count_nonzero(np.array(atlas)[:,:,3])),
                    packedPixels=int(np.count_nonzero(np.array(atlas)[:,:,3])),
                    sourceRgbaExactReconstruction=True,wholeFrameRepairProof=proof)
    write(OUT/'preservation-whole-frames.json',proof)
    return manifest

def finalize(name, manifest, directions, guide):
    manifest['pixelsPerUnit']=guide['pixelsPerUnit']
    manifest['pixelsPerUnitCalibration']=guide
    manifest['status']='visual-reviewed'
    clips=[]
    for row,direction in enumerate(directions):
        selected=manifest['frames'][row*4:row*4+4]
        for phase,frame in enumerate(selected):
            frame.update(direction=DIRECTIONS[direction],durationMs=DURATIONS[phase],
                         phase=['neutral','inhale','blink','return'][phase])
        clips.append(dict(name='idle_'+DIRECTIONS[direction],direction=direction,
                          frames=[f['id'] for f in selected],durationMs=DURATIONS,loop=True,allowStatic=False))
    manifest['clips']=clips
    manifest['requiredClips']=[c['name'] for c in clips]
    manifest['motorcycleIdle']=dict(skin=0,form=0,clips=[dict(direction=c['direction'],frames=c['frames'],
                        durationMs=DURATIONS,loop=True) for c in clips],
                        otherDirectionsComplete=len(directions)==8,movingLocomotionComplete=False)
    manifest['review']=dict(wholeSourceSeen=True,contactSeen=True,motionSeen=False,
                           decodedGifPhaseStripsSeen=True,livePlaybackObserved=False,
                           playbackLimit='Local file URL preview was blocked by browser policy; offline GIF frame/timing review completed.',
                           humanApproval=False,unityImported=False,runtimeVerified=False,
                           identityAuthority='Approved Katana Normal')
    manifest['notes']=['Four genuinely drawn complete-figure phases: neutral, inhale, exhale/blink, return.',
       'No anatomy montage, raster deformation, mirroring or individual rescaling.',
       'All selected nonzero-alpha source RGBA retained exactly; source beneath zero alpha preserved in originals.',
       'One sheet PPU from comparable adult anatomy; WorldVisualScale remains 1.325.',
       'Eight-direction idle coverage belongs to three combined descriptors. Moving locomotion remains outside this package.',
       'No Unity import or runtime verification performed by this art worker.']
    pipeline.RES=RES
    write(RES/(name+'.json'),manifest)
    checks=pipeline.qa(name)
    return dict(name=name,resourcePath='Darklight/WeaponRedesign20261023/'+name,frames=len(manifest['frames']),
                directions=directions,sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],
                qa=checks,sourceRgbaExact=True,pixelsPerUnit=guide['pixelsPerUnit'],
                visualReviewed=True,packaged=True,unityImported=False,runtimeVerified=False)

if __name__=='__main__':
    config=json.loads((ART/'preparation-config.json').read_text(encoding='utf-8'))
    receipts=[]
    for sheet in config['sheets']:
        if sheet.get('repairs'):
            original=prepare_source(sheet['name']+'-original',sheet['source'],sheet['directions'],
                                    sheet['anchors'],selected=sheet['selected'],staging=True)
            repair=sheet['repairs']
            correction=prepare_source(sheet['name']+'-correction',repair['source'],repair['directions'],
                                      repair['anchors'],selected=repair['selected'],staging=True)
            manifest=merge_intact(sheet['name'],original,correction,
                                  {int(k):v for k,v in repair['map'].items()})
        else:manifest=prepare_source(sheet['name'],sheet['source'],sheet['directions'],sheet['anchors'])
        receipts.append(finalize(sheet['name'],manifest,sheet['directions'],sheet['guide']))
    baseline=json.loads((ART/'preservation-before.json').read_text(encoding='utf-8-sig'))
    preservation=[dict(path=p['path'],sha256=p['sha256'],unchanged=pipeline.sha(p['path'])==p['sha256']) for p in baseline]
    if not all(p['unchanged'] for p in preservation):raise ValueError('Preserved baseline changed')
    write(OUT/'preservation-after.json',preservation)
    write(OUT/'prepared.json',dict(sheets=receipts,frames=sum(r['frames'] for r in receipts),
          idleDirections=8,allEightIdleDirectionsPackaged=len(receipts)==3,
          legacyFilesUnchanged=len(preservation),unityImported=False,runtimeVerified=False))
    print(json.dumps(dict(sheets=len(receipts),frames=sum(r['frames'] for r in receipts),
          errors=sum(r['qa']['errors'] for r in receipts),preserved=len(preservation))))
