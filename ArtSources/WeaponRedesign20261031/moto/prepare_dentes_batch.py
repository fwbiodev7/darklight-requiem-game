"""Pack complete authored two-saw attack figures in isolated staging."""
from pathlib import Path
import json,sys,argparse
ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261031/moto'
STAGE=OUT/'staging'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART;pipeline.OUT=OUT/'qa';pipeline.RES=STAGE
DIRS=['S','SW','W','NW','N','NE','E','SE']
TIMES=[150,150,280,400]

def main():
    parser=argparse.ArgumentParser();parser.add_argument('config');args=parser.parse_args()
    cfg=json.loads(Path(args.config).read_text(encoding='utf-8'));name=cfg['name']
    source=ART/cfg['source'];status='visual-reviewed' if cfg.get('visualReviewed',False) else 'candidate'
    job=dict(name=name,source=str(source),preservedSourceName=source.name,skin=0,weapon=3,form=0,
        columns=4,sourceRows=len(cfg['directions']),minimumArea=5000,
        rows=[dict(sourceRow=i,direction=DIRS.index(d),action='Attack',attackKey='Dentes Cruzados',loop=False) for i,d in enumerate(cfg['directions'])],
        supportAnchors={str(i):dict(anchor=a,method='Inspected whole-figure two-boot ground midpoint. No drawing edit.') for i,a in enumerate(cfg['anchors'])})
    manifest,pages=pipeline.prepare(job)
    guide=cfg['calibration'];ppu=guide['pixelsPerUnit']
    for i,frame in enumerate(manifest['frames']):
        frame.update(durationMs=TIMES[i%4],attackKey='Dentes Cruzados',
            phase=['preparation','coil','crossed_contact','recovery'][i%4])
    clips=[dict(name='DentesCruzados_'+d,frames=[f['id'] for f in manifest['frames'][i*4:i*4+4]],
        attackKey='Dentes Cruzados',loop=False,allowStatic=False) for i,d in enumerate(cfg['directions'])]
    manifest.update(status=status,pixelsPerUnit=ppu,pixelsPerUnitCalibration=guide,worldVisualScale=1.325,
        clips=clips,requiredClips=[c['name'] for c in clips],
        motorcycleDualSawsAttack=dict(skin=0,weapon=3,form=0,attackKey='Dentes Cruzados',onFoot=True,mounted=False,
            directionFrames={d:c['frames'] for d,c in zip(cfg['directions'],clips)},durationMs=TIMES,
            startupFrames=2,activeFrames=1,events=[],phaseTiming=dict(startupMs=300,activeMs=280,recoveryMs=400)),
        review=dict(wholeSourceSeen=True,contactSeen=cfg.get('visualReviewed',False),
            nearest2xWholeFiguresSeen=cfg.get('visualReviewed',False),decodedGifPhasesSeen=cfg.get('visualReviewed',False),
            humanApproval=False,livePlaybackObserved=False,unityImported=False,runtimeVerified=False),
        notes=['Dedicated Dentes Cruzados complete figures, on foot with exactly two hand-held wheel-saws.',
            'No anatomy assembly, mirroring, rotation, recoloring, drawing rescale or alpha deletion.',
            'One sheet PPU from standing crown-to-sole anatomy only. PPU does not repair proportions.',
            'Native generated raster; indexed pixel grid unverified. Unity and runtime pending.'])
    for page in pages:
        page.update(status=status,pixelsPerUnit=ppu,durationMs=TIMES,startupFrames=2,activeFrames=1)
    pipeline.write(STAGE/(name+'.json'),manifest)
    pipeline.write(OUT/(name+'-pages.json'),dict(schemaVersion=1,complete=False,pages=pages))
    pipeline.write(ART/(name+'-job.json'),job)
    qa=pipeline.qa(name)
    receipt=dict(name=name,figures=len(manifest['frames']),clips=len(clips),directions=cfg['directions'],
        attackKey='Dentes Cruzados',status=status,png=(STAGE/(name+'.png')).relative_to(ROOT).as_posix(),
        json=(STAGE/(name+'.json')).relative_to(ROOT).as_posix(),pages=(OUT/(name+'-pages.json')).relative_to(ROOT).as_posix(),
        imageSha256=manifest['imageSha256'],sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],
        nativeSize=manifest['nativeSize'],sourceSize=manifest['sourceSize'],pixelsPerUnit=ppu,
        worldVisualScale=1.325,durationMs=TIMES,startupFrames=2,activeFrames=1,qa=qa,
        imported=False,runtimeVerified=False,stagingOnly=True)
    pipeline.write(OUT/(name+'-receipt.json'),receipt);print(json.dumps(receipt))

if __name__=='__main__':main()
