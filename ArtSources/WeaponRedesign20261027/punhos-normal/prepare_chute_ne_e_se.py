"""Pack reviewed COMPLETE figures only; never mutate the shared catalog or Unity assets."""
from pathlib import Path
import hashlib
import json
import statistics
import sys
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261027/punhos-normal'
NAME='ReqFistsNormalChuteNE_E_SE20261027Selected'
RAW=ART/'ReqFistsNormalChuteN_NE_E_SE20261027V1-source.png'
DIRS=['NE','E','SE']
pipeline.ART=ART
pipeline.RES=OUT/'extraction-staging'
pipeline.OUT=OUT/'qa'

def rows(offset=0):
    return [dict(sourceRow=i+offset,direction=i+5,action='Attack',attackKey='Chute Quebra-Osso',
        sourceSpriteNames=[f'Ruin_Requiem_Normal_{DIRS[i]}_Combat_{phase:02}' for phase in (4,5,11)]) for i in range(3)]

def main():
    initial=dict(name=NAME+'Extraction',source=str(RAW),skin=0,weapon=2,form=0,columns=3,sourceRows=4,
        minimumArea=15000,rows=rows(1),excludedReason='North row held: contact kicks sideways west, not north. No incomplete N clip registered.')
    original,_=pipeline.prepare(initial)
    source=ART/(NAME+'-source.png')
    source.write_bytes((pipeline.RES/original['image']).read_bytes())
    ppu=statistics.median([313,315,305,301,320,322])/1.98
    job=dict(name=NAME,source=str(source),skin=0,weapon=2,form=0,columns=3,sourceRows=3,minimumArea=15000,rows=rows(),
        calibrationGuide=dict(pixelsPerUnit=ppu,targetBodyWorldHeight=1.98,worldVisualScale=1.325,
          method='Hair-to-grounded-sole measurements of six chamber/recovery poses, excluding raised kicking leg extent; one PPU for the whole sheet.',
          originalSourceFrameIndices=[3,5,6,8,9,11],anatomyHeights=[313,315,305,301,320,322],individualResizing=False),
        provenance=dict(wholeFiguresOnly=True,originalSource=RAW.relative_to(ROOT).as_posix(),originalSourceSha256=pipeline.sha(RAW),
          selectedOriginalFrameIndices=list(range(3,12)),retainedButUnusedNorthIndices=[0,1,2],rawSourceFrameDescriptors=original['frames'],
          rawSelectedRgbaPreserved=True,anatomicalAssembly=False,generatedWith='built-in imagegen',generationCalls=1))
    pipeline.RES=OUT/'payload'
    manifest,pages=pipeline.prepare(job)
    manifest.update(pixelsPerUnit=ppu,status='candidate',pixelsPerUnitCalibration=job['calibrationGuide'],provenance=job['provenance'])
    for i,frame in enumerate(manifest['frames']):
        frame['durationMs']=[75,85,160][i%3];frame['phase']=['anticipation','contact','recovery'][i%3]
    for page in pages:page.update(pixelsPerUnit=ppu,status='candidate')
    manifest['review']=dict(wholeSourceSeen=True,packedContactSeen=False,motionSeen=False,humanApproved=False,
        unityImported=False,runtimeVerified=False,findings=['North contact is held; NE/E/SE are selected as nine complete figures.',
        'NE back-right anatomy hides face; E profile/right and SE front-right preserve identity and neutral equipment.',
        'No existing source, sprite or clip has been replaced.'])
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    pipeline.write(ART/(NAME+'-job.json'),job)
    pipeline.write(OUT/'catalog-fragment.json',dict(schemaVersion=1,complete=False,pages=pages))
    qa=pipeline.qa(NAME)
    pipeline.write(OUT/'prepared.json',dict(name=NAME,producedFigures=12,selectedFigures=9,clips=3,directions=DIRS,
        northClipIntegrated=False,northHeldReason='Contact direction incorrect; repair the entire original index1 after coordinator review.',
        sourcePhases=[4,5,11],durationsMs=[75,85,160],pixelsPerUnit=ppu,worldVisualScale=1.325,
        generatedCalls=1,qa=qa,existingApprovedFiguresUnchanged=True,humanApproved=False,unityImported=False,runtimeVerified=False))
    print(json.dumps(dict(name=NAME,produced=12,selected=9,clips=3,pixelsPerUnit=ppu,qa=qa,northHeld=True)))

if __name__=='__main__':main()
