"""Prepare reviewed Presas/Normal additions in isolated staging; no Unity catalog writes."""
from pathlib import Path
import argparse
import json
import statistics

import prepare_weapon_redesign as pipeline

ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / 'ArtSources/WeaponRedesign20261025Retomada1350/presas-normal'
OUT = ROOT / 'outputs/redesign-armas-20261025-retomada1350/presas-normal'
CONFIG = ART / 'preparation-specs.json'
DIRS = ['S','SW','W','NW','N','NE','E','SE']


def main():
    global ART, OUT, CONFIG
    parser = argparse.ArgumentParser()
    parser.add_argument('--name', help='Prepare only one configured source.')
    parser.add_argument('--skin', type=int, choices=(0,1), default=0)
    args = parser.parse_args()
    if args.skin==1:
        ART, OUT = ART/'rock-normal', OUT/'rock-normal'
        CONFIG = ART/'preparation-specs.json'
    specs = json.loads(CONFIG.read_text(encoding='utf-8'))['sheets']
    pipeline.ART, pipeline.RES, pipeline.OUT = ART, OUT, OUT / 'qa-reviewed'
    summaries, pages, jobs = [], [], []
    for spec in specs:
        if args.name and spec['name'] != args.name:
            continue
        action = spec['action']
        groups = spec.get('groups', [list(range(i*4,i*4+4)) for i in range(len(spec['directions']))])
        rows=[]
        for direction, group in zip(spec['directions'], groups):
            row = dict(sourceRow=group[0]//4, sourceIndices=group, direction=direction,
                       action=action, loop=action=='Idle')
            if action=='Idle' and len(group)==3:
                row.update(phaseIndices=[0,1,2,0], durationMs=[350,250,180])
            if action=='Attack':
                bank = 'Cardinal' if direction%2==0 else 'Diagonal'
                offset = direction//2*4
                identity = 'Rock' if args.skin==1 else 'Requiem'
                row.update(attackKey='Presas1', sourceSpriteNames=[f'Today_{identity}AttackDaggersYoung{bank}_{offset+i}' for i in range(4)])
            rows.append(row)
        job = dict(name=spec['name'], source=str(ART/(spec['name']+'-source.png')),
                   skin=args.skin, weapon=1, form=0, columns=4,
                   sourceRows=spec.get('sourceRows',len(rows)), minimumArea=10000,
                   rows=rows, excludedReason=spec.get('excludedReason',''), supportAnchors={str(i):dict(anchor=anchor,
                   method='Visual inspection of whole source: projected midpoint between boot supports at lower sole baseline.')
                   for i,anchor in enumerate(spec['anchors'])})
        manifest, sheet_pages = pipeline.prepare(job)
        measures=[dict(sourceFrameIndex=i,headY=head,soleY=sole,anatomyHeight=sole-head)
                  for i,head,sole in spec['measurements']]
        ppu = statistics.median(m['anatomyHeight'] for m in measures)/1.98
        manifest.update(pixelsPerUnit=ppu,stagingOnly=True,imported=False,runtimeVerified=False,
            visualReviewed=True,visualApproval=False,
            ppuProvenance=dict(pipelineInferred=manifest['pixelsPerUnit'],candidateAnatomical=ppu,
                measurements=measures,targetHeightUnits=1.98,
                method='Manually identified compact canonical head-to-sole height; raised daggers do not count. One PPU per sheet; no raster scaling.',
                limitation='Projected posture and hair crown imply uncertainty; world-scale/camera and transitions require coordinator review.'),
            references=[dict(path='ArtSources/WeaponRedesign20261024/candidates/ReqDaggersNormalLight1S_SW_W20261024v2-source.png',
                sha256='b918d0e89a14e1e6bf45597a075e7cddce2d8c96890c6ffe393033b72b4d83ff')],
            generation=dict(tool='integrated image_gen',promptPath=(ART/spec.get('promptFile',spec['name']+'-prompt.txt')).relative_to(ROOT).as_posix()),
            review=dict(sourceSeen=True,packedContactSeen=spec.get('packedContactSeen',False),
                motionPreviewSeen=False,findings=spec['findings'],audioStarted=False))
        if action=='Attack':
            for i,frame in enumerate(manifest['frames']):
                frame.update(durationMs=[30,45,38,38][i%4],phase=['anticipation','contact','follow-through','recovery'][i%4])
            manifest['timingNote']='Preview rounds29.75ms startup to30ms. Runtime phase selection uses existing source names and gameplay timings.'
        else:
            starts=[sum(len(group) for group in groups[:i]) for i in range(len(rows))]
            manifest['stoppedPoseFrameIds']=[manifest['frames'][i]['id'] for i in starts]
            manifest['stoppedPosePolicy']='First neutral authored frame is stopped pose; it is not an additional figure. Four distinct phases are neutral/inhale/blink/exhale.'
            for clip,group in zip(manifest['clips'],groups):
                if len(group)==3:
                    clip['frames'].append(clip['frames'][0])
                    clip['intentionalReturnHold']='Return to already authored neutral; three unique whole figures, not four new figures. Missing fourth unique pose remains pending.'
                    manifest['stoppedPosePolicy']='First neutral is stopped pose. Three-frame clips intentionally return to existing neutral0; this is not a new fourth figure. Other declared clips have four authored phases.'
        for page in sheet_pages:
            page.update(pixelsPerUnit=ppu,stagingOnly=True,imported=False,runtimeVerified=False,
                        visualReviewed=True)
        pipeline.write(OUT/(spec['name']+'.json'),manifest)
        pipeline.write(OUT/(spec['name']+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=sheet_pages))
        qa=pipeline.qa(spec['name'])
        summaries.append(dict(name=spec['name'],action=action,directions=[DIRS[d] for d in spec['directions']],
            frames=len(manifest['frames']),imageSha256=manifest['imageSha256'],sourceSha256=manifest['sourceSha256'],
            pixelsPerUnit=ppu,exactRgba=manifest['sourceRgbaExactReconstruction'],qa=qa,
            sourceSeen=True,packedContactSeen=spec.get('packedContactSeen',False),motionPreviewSeen=False,
            imported=False,runtimeVerified=False))
        pages.extend(sheet_pages)
        jobs.append(job)
    # Aggregate from the individually saved fragments, including already prepared sheets.
    all_pages=[]
    for spec in specs:
        fragment=OUT/(spec['name']+'.pages.json')
        if fragment.exists():
            all_pages.extend(json.loads(fragment.read_text(encoding='utf-8'))['pages'])
    fragment_name='presas-rock-normal-pages.json' if args.skin==1 else 'presas-normal-pages.json'
    pipeline.write(OUT/fragment_name,dict(schemaVersion=1,complete=False,stagingOnly=True,pages=all_pages))
    pipeline.write(OUT/'preparation-summary.json',dict(sheets=summaries,stagingOnly=True,imported=False,runtimeVerified=False,audio=False))
    pipeline.write(ART/'import-jobs.json',dict(jobs=jobs))
    print(json.dumps(dict(preparedSheets=len(summaries),preparedFrames=sum(s['frames'] for s in summaries),
        aggregatePages=len(all_pages),errors=sum(s['qa']['errors'] for s in summaries),warnings=sum(s['qa']['warnings'] for s in summaries))))


if __name__=='__main__':
    main()
