"""Pack approved intact figures without mutating the shared weapon catalog.

The coordinator merges catalog-fragment.json and runs Unity imports separately.
No generation, anatomy edit, resizing, palette conversion or gameplay change.
"""
import argparse
import json
from pathlib import Path

from prepare_weapon_redesign import ROOT, RES, OUT, prepare, qa, write, sha

CANDIDATES = ROOT / 'ArtSources/WeaponRedesign20261024/candidates'
DESTINATION = ROOT / 'outputs/redesign-armas-20261025/punhos-moto'
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']


def fists_job():
    reviewed = json.loads((CANDIDATES / 'ReqFistsNormalDireto_S_SW_W-v2-job.json').read_text(encoding='utf-8'))
    rows = []
    for direction in range(3):
        rows.append(dict(sourceRow=direction, direction=direction, action='Attack',
                         attackKey='Direto da Ru\u00edna',
                         sourceSpriteNames=[f'Ruin_Requiem_Normal_{DIRECTIONS[direction]}_Combat_{phase:02}'
                                            for phase in (2, 3, 11)]))
    return dict(name='ReqFistsNormalDiretoSSWW20261025V2',
                source=str(CANDIDATES / reviewed['source']), skin=0, weapon=2, form=0,
                columns=3, sourceRows=3, rows=rows, minimumArea=15000,
                supportAnchors={str(frame['index']): dict(anchor=frame['proposedGroundAnchorSource'],
                    method='Whole-source inspected boot support; existing reviewed integer anchors.')
                                for frame in reviewed['frames']})


def motorcycle_job(source=None, form=0, guides=None):
    # Ground positions follow whole-bike tire support or standing boot support.
    # All frames retain a single PPU based on the full standing rider in frame 9:
    # hair y=742 to sole y=989 (247px) / canonical 1.98 world units. Overhead
    # machinery is deliberately excluded from anatomical scale calibration.
    anchors = [[183,329],[554,328],[989,316],[1388,336],
               [179,651],[550,647],[977,639],[1374,668],
               [195,991],[586,989],[988,992],[1378,991]]
    rows = [dict(sourceRow=i//4, sourceIndices=[i], direction=i, action='Idle',
                 durationMs=[160], loop=False) for i in range(8)]
    rows.append(dict(sourceRow=2, sourceIndices=[8,9,10,11], direction=0,
                     action='Attack', attackKey='Esmagamento Vertical'))
    if form and guides is None:
        raise ValueError('New forms need their own inspected support anchors and anatomical PPU (--motorcycle-guides).')
    if guides:
        anchors=guides['supportAnchors']
        if len(anchors)!=12: raise ValueError('Expected twelve complete-figure support anchors')
    job=dict(name=f'RockMotoForm{form}Ride8SlamS20261025V2',
                source=str(source or CANDIDATES / 'RockMotorcycleNormalRedesign_Ride8_SlamS-v2-source.png'),
                skin=1, weapon=3, form=form, columns=4, sourceRows=3,
                rows=rows, minimumArea=15000,
                supportAnchors={str(i):dict(anchor=anchor,
                    method='Whole-source inspected tire or boot ground support, never weapon bounds.')
                                for i,anchor in enumerate(anchors)})
    job['calibratedPixelsPerUnit']=guides['pixelsPerUnit'] if guides else 247/1.98
    if source:
        job['preservedSourceName']=f'RockMotoForm{form}-'+Path(source).name
    job['calibrationGuide']=guides or dict(method='Standing canonical rider anatomy; overhead bike excluded',
        sourceFrameIndex=9,hairY=742,soleY=989,worldBodyHeight=1.98,visualScaleCheckPending=True)
    return job


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--only', choices=['fists','motorcycle','all'], default='all')
    parser.add_argument('--motorcycle-source', type=Path)
    parser.add_argument('--motorcycle-form', type=int, choices=range(4), default=0)
    parser.add_argument('--motorcycle-guides',type=Path,
                        help='JSON with twelve supportAnchors [x,y] and anatomical pixelsPerUnit for a new form')
    args = parser.parse_args()
    jobs = []
    if args.only in ('fists','all'): jobs.append(fists_job())
    guides=json.loads(args.motorcycle_guides.read_text(encoding='utf-8')) if args.motorcycle_guides else None
    if args.only in ('motorcycle','all'): jobs.append(motorcycle_job(args.motorcycle_source,args.motorcycle_form,guides))
    pages, summary = [], []
    for job in jobs:
        manifest, fragment = prepare(job)
        if job['weapon']==3:
            manifest['pixelsPerUnit'] = job['calibratedPixelsPerUnit']
            manifest['pixelsPerUnitCalibration'] = job['calibrationGuide']
            for frame, duration in zip(manifest['frames'],[160]*8+[232,348,160,580]):
                frame['durationMs']=duration
            manifest['clips'] = [dict(name='ride_'+DIRECTIONS[i],frames=[manifest['frames'][i]['id']],
                loop=False,allowStatic=True) for i in range(8)] + [dict(name='slam_S',
                frames=[frame['id'] for frame in manifest['frames'][8:]],loop=False,allowStatic=False)]
            manifest['requiredClips']=[clip['name'] for clip in manifest['clips']]
            for page in fragment: page['pixelsPerUnit']=manifest['pixelsPerUnit']
            # This standalone descriptor is loaded by the motorcycle consumer,
            # not inserted into the three-weapon catalog.
            manifest['motorcycle'] = dict(form=job['form'],skin=1,
                ride=[frame['id'] for frame in manifest['frames'][:8]],
                slam=[frame['id'] for frame in manifest['frames'][8:]],
                slamStartup=0.58,slamActive=0.16,slamRecovery=0.58,
                staticRideViews=True,otherActionsCoverage=False)
        else:
            for frame,duration in zip(manifest['frames'],[75,65,130]*3): frame['durationMs']=duration
            pages.extend(fragment)
        write(RES / (job['name']+'.json'),manifest)
        checks=qa(job['name'])
        summary.append(dict(name=job['name'],frames=len(manifest['frames']),
            sourceSha256=manifest['sourceSha256'],pixelsPerUnit=manifest['pixelsPerUnit'],
            exactSourceRgba=manifest['sourceRgbaExactReconstruction'],qa=checks,
            sourcePath=manifest['sourcePath'],manifestPath=str((RES / (job['name']+'.json')).relative_to(ROOT)),
            unityImported=False,runtimeVerified=False))
    if pages: write(DESTINATION / 'catalog-fragment.json',dict(schemaVersion=1,pages=pages))
    prior_path=DESTINATION / 'prepared.json'
    prior=json.loads(prior_path.read_text(encoding='utf-8'))['sheets'] if prior_path.exists() else []
    replaced={sheet['name'] for sheet in summary}
    write(prior_path,dict(sheets=[sheet for sheet in prior if sheet['name'] not in replaced]+summary,complete=False,audioStarted=False))
    job_path=DESTINATION / 'preparation-jobs.json'
    prior_jobs=json.loads(job_path.read_text(encoding='utf-8'))['jobs'] if job_path.exists() else []
    write(job_path,dict(jobs=[job for job in prior_jobs if job['name'] not in replaced]+jobs))
    print(json.dumps(dict(sheets=len(summary),frames=sum(s['frames'] for s in summary),
        errors=sum(s['qa']['errors'] for s in summary),warnings=sum(s['qa']['warnings'] for s in summary))))


if __name__=='__main__': main()
