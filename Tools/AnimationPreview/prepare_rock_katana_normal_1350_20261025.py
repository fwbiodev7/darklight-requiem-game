"""Prepare only Rock/Normal idle fragments; never edit shared Unity catalogs."""
from pathlib import Path
import json
import statistics
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

ART = ROOT/'ArtSources/WeaponRedesign20261025Retomada1350/katana-normal'
OUT = ROOT/'outputs/redesign-armas-20261025-retomada1350/katana-normal'
pipeline.ART, pipeline.RES, pipeline.OUT = ART, OUT, OUT/'qa'
SPECS = [
    dict(name='RockKatanaNormalIdleS_SW_W1350', source='RockKatanaNormalIdleS_SW_W-v1-source.png', directions=[0,1,2],
         crown=[26,26,29,27,375,371,376,375,690,690,692,693],
         soles=[340,339,341,341,685,688,688,688,1010,1010,1010,1010],
         anchors=[[231,338],[610,338],[987,340],[1367,340],[226,676],[605,678],[989,680],[1375,680],[233,1002],[620,1002],[1006,1002],[1381,1003]]),
    dict(name='RockKatanaNormalIdleNW_N_NE1350', source='RockKatanaNormalIdleNW_N_NE-v1-source.png', directions=[3,4,5],
         crown=[23,21,23,22,365,363,366,365,690,690,691,691],
         soles=[334,334,334,333,670,670,670,670,1003,1003,1004,1003],
         anchors=[[163,332],[556,332],[947,332],[1331,331],[178,670],[570,670],[961,670],[1350,670],[181,997],[573,997],[963,998],[1358,997]]),
    dict(name='RockKatanaNormalIdleE_SE1350', source='RockKatanaNormalIdleE_SE-v1-source.png', directions=[6,7],
         crown=[61,61,62,61,546,543,544,543], soles=[486,486,486,487,968,968,968,968],
         anchors=[[193,480],[577,480],[959,480],[1345,481],[206,958],[590,958],[975,958],[1363,958]]),
]


def main():
    pages, results, jobs = [], [], []
    for spec in SPECS:
        n = len(spec['directions'])
        rows = [dict(sourceRow=i, sourceIndices=list(range(i*4,i*4+4)), direction=d, action='Idle', loop=True)
                for i,d in enumerate(spec['directions'])]
        supports = {str(i):dict(anchor=a,method='Manual projected boot-support midpoint in intact source; world-scale preview checked, Unity pending.')
                    for i,a in enumerate(spec['anchors'])}
        job = dict(name=spec['name'],source=str(ART/spec['source']),skin=1,weapon=0,form=0,columns=4,
                   sourceRows=n,minimumArea=10000,rows=rows,supportAnchors=supports)
        manifest, fragment = pipeline.prepare(job)
        measurement = [dict(sourceFrameIndex=i,headY=head,soleY=sole,anatomyHeight=sole-head)
                       for i,(head,sole) in enumerate(zip(spec['crown'],spec['soles']))]
        ppu = statistics.median(m['anatomyHeight'] for m in measurement[:4])/1.98
        manifest.update(status='visual-reviewed',visualReviewed=True,imported=False,runtimeVerified=False,
                        pixelsPerUnit=ppu,originalSourcePath=(ART/spec['source']).relative_to(ROOT).as_posix(),
                        ppuProvenance=dict(candidateAnatomical=ppu,targetHeightUnits=1.98,measurements=measurement,
                                           method='Crown to boot sole in relaxed comparable stance; excludes lowered katana.',
                                           uncertaintyPixels=[3,8],limitation='Projected stance and clothing differ by view; no anatomy rescaled.'),
                        stoppedPoseFrameIds=[manifest['frames'][i*4]['id'] for i in range(n)],
                        stoppedPosePolicy='Frame0 is the static stop; four separate authored phases make each idle cycle.',
                        rearAnatomyReview='NW/N/NE head/nuca/neck/shoulders/torso/hips/feet inspected together; no frontal face over rear torso.')
        for page in fragment:
            page.update(pixelsPerUnit=ppu,status='visual-reviewed',stagingOnly=True,
                        imported=False,runtimeVerified=False,visualReviewed=True)
        pipeline.write(OUT/(spec['name']+'.json'),manifest)
        pipeline.write(OUT/(spec['name']+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=fragment))
        qa=pipeline.qa(spec['name'])
        results.append(dict(name=spec['name'],wholeFigures=len(manifest['frames']),directions=spec['directions'],
                            sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],
                            sourcePath=manifest['sourcePath'],pixelsPerUnit=ppu,
                            sourceRgbaExactReconstruction=manifest['sourceRgbaExactReconstruction'],qa=qa))
        pages+=fragment
        jobs.append(job)
    pipeline.write(OUT/'rock-katana-normal-idle-jobs.json',dict(schemaVersion=1,stagingOnly=True,jobs=jobs))
    pipeline.write(OUT/'rock-katana-normal-idle-pages.json',dict(schemaVersion=1,stagingOnly=True,pages=pages))
    summary=dict(schemaVersion=1,date='2026-10-25',session='13h50',skin=1,weapon=0,form=0,
                 wholeFigures=sum(r['wholeFigures'] for r in results),pages=len(pages),status='visual-reviewed',
                 imported=False,runtimeVerified=False,results=results,
                 missing='Rock attacks and other actions/forms, Walk SW/W correction remain pending; Requiem is not counted as Rock.')
    pipeline.write(OUT/'rock-katana-normal-idle-summary.json',summary)
    print(json.dumps(dict(wholeFigures=summary['wholeFigures'],pages=len(pages),
                          qaErrors=sum(r['qa']['errors'] for r in results),qaWarnings=sum(r['qa']['warnings'] for r in results))))


if __name__=='__main__':
    main()
