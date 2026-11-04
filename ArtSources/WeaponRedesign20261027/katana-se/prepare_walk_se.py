"""Pack the eight authored whole-character SE walk phases; no shared integration edits."""
from pathlib import Path
import json
import sys
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261027/katana-se'
NAME='ReqKatanaNormalWalkSE20261027V1'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART
pipeline.OUT=OUT
PHASES=['contact_L','down_L','passing_L','up_L','contact_R','down_R','passing_R','up_R']

def main():
    guides=json.loads((ART/'guides.json').read_text(encoding='utf-8'))
    job=dict(name=NAME,source=str(ART/(NAME+'-source.png')),preservedSourceName=NAME+'-source.png',
             skin=0,weapon=1,form=0,columns=4,sourceRows=2,minimumArea=15000,
             rows=[dict(sourceRow=0,sourceIndices=list(range(8)),direction=7,action='Walk',
                        durationMs=[50]*8,loop=True)],
             supportAnchors={str(i):dict(anchor=anchor,method='Whole-source inspected pelvis projection at planted boot ground plane; raised boot and blade excluded.')
                             for i,anchor in enumerate(guides['supportAnchors'])})
    manifest,pages=pipeline.prepare(job)
    manifest['pixelsPerUnit']=guides['pixelsPerUnit']
    manifest['anatomicalScaleGuide']=guides
    manifest['worldVisualScale']=1.325
    for frame,phase in zip(manifest['frames'],PHASES):
        frame['phase']=phase
        frame['durationMs']=50
    manifest['clips']=[dict(name='walk_SE',frames=[f['id'] for f in manifest['frames']],
                            loop=True,allowStatic=False,durationMs=[50]*8)]
    manifest['requiredClips']=['walk_SE']
    manifest['review']=dict(wholeSourceSeen=True,contactSeen=True,completePhaseStripSeen=True,
                            humanApproval=False,unityImported=False,runtimeVerified=False)
    manifest['notes']=['Eight genuinely authored full-character southeast walk phases; 400ms loop.',
       'First and fifth contacts reverse leading/trailing legs; passing phases alternate raised boot.',
       'Correct right-hand katana carry follows the approved southeast idle, not a mirror of southwest.',
       'Approved Katana SW/W and NW/N define adult identity, outfit, canonical short haircut and gait quality.',
       'Every selected nonzero-alpha source RGBA pixel preserved; no anatomy montage, rotation, mirroring, recoloring or resizing.',
       'One body-calibrated PPU; blade bounds excluded. WorldVisualScale remains 1.325.',
       'Unity import, dynamic contact and transitions belong to coordinator; no shared catalog edits.']
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    for page in pages:
        page['pixelsPerUnit']=manifest['pixelsPerUnit']
        page['durationMs']=[50]*8
    pipeline.write(OUT/'catalog-fragment.json',dict(schemaVersion=1,complete=False,pages=pages))
    pipeline.write(ART/(NAME+'-job.json'),job)
    checks=pipeline.qa(NAME)
    gif=OUT/NAME/'preview/clip-00.gif'
    im=Image.open(gif)
    gif_frames=[];timings=[]
    for i in range(im.n_frames):
        im.seek(i);gif_frames.append(im.convert('RGBA').copy());timings.append(im.info.get('duration'))
    distinct=[]
    for i in range(len(gif_frames)):
        a=np.array(gif_frames[i]);b=np.array(gif_frames[(i+1)%len(gif_frames)])
        distinct.append(int(np.count_nonzero(np.any(a!=b,axis=2))))
    if im.n_frames!=8 or timings!=[50]*8 or not all(distinct):raise ValueError('GIF phase/timing audit failed')
    strip=Image.new('RGBA',(gif_frames[0].width*4,gif_frames[0].height*2),(40,45,53,255))
    for i,frame in enumerate(gif_frames):strip.paste(frame,((i%4)*frame.width,(i//4)*frame.height),frame)
    strip.save(OUT/'decoded-gif-phase-strip.png')
    before=json.loads((ART/'preservation-before.json').read_text(encoding='utf-8-sig'))
    preservation=[dict(path=p['path'],before=p['sha256'],after=pipeline.sha(ROOT/p['path']),
                       unchanged=pipeline.sha(ROOT/p['path'])==p['sha256']) for p in before]
    pipeline.write(OUT/'preservation-after.json',preservation)
    pipeline.write(OUT/'gif-audit.json',dict(gif=gif.relative_to(ROOT).as_posix(),frames=im.n_frames,
                    durationMs=timings,loopMs=sum(timings),changedPixelsBetweenPhases=distinct,
                    allPhasesDistinct=all(distinct),livePlaybackObserved=False))
    receipt=dict(name=NAME,resourcePath='Darklight/WeaponRedesign20261023/'+NAME,
         frames=8,direction='SE',durationMs=[50]*8,loopMs=400,
         sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],
         imageSha256=manifest['imageSha256'],nativeSize=manifest['nativeSize'],sourceSize=manifest['sourceSize'],
         pixelsPerUnit=manifest['pixelsPerUnit'],worldVisualScale=1.325,
         rgbaExact=manifest['sourceRgbaExactReconstruction'],wholeFigures=True,
         qa=checks,generated=True,visualReviewed=True,packaged=True,humanApproval=False,
         unityImported=False,runtimeVerified=False,sharedCatalogModified=False,
         referenceFilesPreserved=sum(p['unchanged'] for p in preservation))
    pipeline.write(OUT/'prepared.json',receipt)
    print(json.dumps(receipt))

if __name__=='__main__':main()
