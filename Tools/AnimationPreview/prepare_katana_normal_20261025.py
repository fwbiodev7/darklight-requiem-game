"""Stage reviewed whole figures; never edit the shared Unity catalog."""
from pathlib import Path
import hashlib
import json
import statistics
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

ART = ROOT / 'ArtSources/WeaponRedesign20261025/candidates'
OUT = ROOT / 'outputs/redesign-armas-20261025/katana-normal'
pipeline.ART = ART
pipeline.RES = OUT
pipeline.OUT = OUT / 'qa'
TIMINGS = [55, 75, 70, 70]
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']

# Hair-to-sole measurements inspect the intact source. Raised swords/trails are
# excluded; full source remains preserved. PPU still needs Unity camera review.
SPECS = [
    dict(name='ReqKatanaNormalNeutralLeve1NW_N20261025', source='ReqKatanaNormalLight1NW_N_NE-v3-source.png',
         sourceRows=3, directions=[3, 4], groups=[[0,1,2,3],[4,5,6,7]], action='Attack',
         measurements=[(0,53,325),(1,58,342),(2,67,341),(3,58,342),(4,383,660),(5,383,659),(6,384,660),(7,384,660)],
         anchors=[[205,309],[560,325],[963,324],[1358,326],[169,660],[573,659],[966,660],[1362,660]],
         excluded='Entire NE row excluded: anticipation swaps weapon hand. Corrected NE source is staged separately.'),
    dict(name='ReqKatanaNormalNeutralLeve1NE20261025', source='ReqKatanaNormalLight1NE-v4-source.png',
         sourceRows=1, directions=[5], groups=[[0,1,2,3]], action='Attack',
         measurements=[(0,221,626),(1,226,623),(2,237,626),(3,223,626)],
         anchors=[[255,606],[758,604],[1320,605],[1852,605]]),
    dict(name='ReqKatanaNormalNeutralLeve1E_SE20261025', source='ReqKatanaNormalLight1E_SE-v1-source.png',
         sourceRows=2, directions=[6, 7], groups=[[0,1,2,3],[4,5,6,7]], action='Attack',
         measurements=[(0,96,426),(1,103,428),(2,116,429),(3,96,429),(4,507,847),(5,507,848),(6,524,841),(7,506,847)],
         anchors=[[234,426],[662,425],[1113,429],[1586,429],[243,839],[678,831],[1132,832],[1584,840]]),
    dict(name='ReqKatanaNormalNeutralIdleS_SW_W20261025', source='ReqKatanaNormalIdleS_SW_W-v1-source.png',
         sourceRows=3, directions=[0,1,2], groups=[[0,1,2,3],[4,5,6,7],[8,9,10,11]], action='Idle',
         measurements=[(0,34,336),(1,36,337),(2,37,337),(3,36,337),(4,357,669),(5,356,669),(6,356,669),(7,356,669),(8,680,994),(9,679,991),(10,682,994),(11,681,994)],
         anchors=[[246,336],[607,337],[979,337],[1350,337],[229,656],[601,656],[975,656],[1360,656],[236,984],[605,982],[982,984],[1368,984]]),
    dict(name='ReqKatanaNormalNeutralIdleNW_N_NE20261025', source='ReqKatanaNormalIdleNW_N_NE-v1-source.png',
         sourceRows=3, directions=[3,4,5], groups=[[0,1,2,3],[4,5,6,7],[8,9,10,11]], action='Idle',
         measurements=[(0,30,338),(1,28,337),(2,28,338),(3,28,338),(4,365,668),(5,365,668),(6,365,668),(7,365,668),(8,691,997),(9,691,997),(10,692,997),(11,692,997)],
         anchors=[[180,330],[556,330],[923,330],[1306,330],[189,668],[560,668],[935,668],[1324,668],[200,988],[575,988],[949,988],[1328,988]]),
    dict(name='ReqKatanaNormalNeutralIdleE_SE20261025', source='ReqKatanaNormalIdleE_SE-v1-source.png',
         sourceRows=2, directions=[6,7], groups=[[0,1,2,3],[4,5,6,7]], action='Idle',
         measurements=[(0,48,437),(1,48,439),(2,49,439),(3,49,439),(4,476,856),(5,474,856),(6,479,856),(7,478,856)],
         anchors=[[244,434],[686,435],[1126,436],[1577,436],[242,849],[680,849],[1133,849],[1570,849]]),
]


def main():
    protected = [ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json',
                 ROOT / 'Tools/AnimationPreview/prepare_weapon_redesign.py']
    before = {p.relative_to(ROOT).as_posix(): pipeline.sha(p) for p in protected}
    results, all_pages, jobs = [], [], []
    for spec in SPECS:
        action = spec['action']
        indices = sum(spec['groups'], [])
        supports = {str(i): dict(anchor=anchor, method='Manual projected foot-support midpoint in intact full source; Unity calibration pending.')
                    for i, anchor in zip(indices, spec['anchors'])}
        rows = []
        for direction, group in zip(spec['directions'], spec['groups']):
            row = dict(sourceRow=group[0] // 4, sourceIndices=group, direction=direction,
                       action=action, loop=action == 'Idle')
            if action == 'Attack':
                bank = 'Cardinal' if direction % 2 == 0 else 'Diagonal'
                offset = (direction // 2) * 4
                row.update(attackKey='Leve1', sourceSpriteNames=[f'Today_RequiemAttackKatanaYoung{bank}_{offset+i}' for i in range(4)])
            rows.append(row)
        job = dict(name=spec['name'], source=str(ART/spec['source']), skin=0, weapon=0, form=0,
                   columns=4, sourceRows=spec['sourceRows'], minimumArea=10000,
                   rows=rows, supportAnchors=supports, excludedReason=spec.get('excluded', ''))
        manifest, pages = pipeline.prepare(job)
        measurements = [dict(sourceFrameIndex=i, headY=y, soleY=sole, anatomyHeight=sole-y) for i,y,sole in spec['measurements']]
        inferred = manifest['pixelsPerUnit']
        ppu = statistics.median(x['anatomyHeight'] for x in measurements) / 1.98
        manifest.update(status='visual-reviewed', visualReviewed=True, imported=False, runtimeVerified=False,
                        pixelsPerUnit=ppu, originalSourcePath=(ART/spec['source']).relative_to(ROOT).as_posix(),
                        ppuProvenance=dict(pipelineInferred=inferred, candidateAnatomical=ppu, targetHeightUnits=1.98,
                                           measurements=measurements, method='Manual hair-to-sole, excludes raised blade and effects.',
                                           limitation='Pose and source scale differ; Unity camera and transitions still require review.'))
        if action == 'Attack':
            for i, frame in enumerate(manifest['frames']):
                frame['durationMs'] = TIMINGS[i % 4]
            manifest['previewTimingsMs'] = TIMINGS
        else:
            manifest['stoppedPoseFrameIds'] = [manifest['frames'][i*4]['id'] for i in range(len(rows))]
            manifest['stoppedPosePolicy'] = 'Neutral first frame is a static stopped pose; the four separately authored phases form the idle.'
        manifest['notes'] += ['Staging only; no shared Unity catalog or gameplay timings modified.',
                              'Direction review includes silhouette, nuca, profile and right-hand grip; source sheet positions are not direction evidence alone.']
        pipeline.write(OUT/(spec['name']+'.json'), manifest)
        for page in pages:
            page.update(status='visual-reviewed', pixelsPerUnit=ppu, stagingOnly=True,
                        imported=False, runtimeVerified=False, visualReviewed=True)
        pipeline.write(OUT/(spec['name']+'.pages.json'), dict(schemaVersion=1, stagingOnly=True, pages=pages))
        qa = pipeline.qa(spec['name'])
        results.append(dict(name=spec['name'], action=action, frames=len(manifest['frames']),
                            directions=[DIRECTIONS[d] for d in spec['directions']],
                            sourceSha256=manifest['sourceSha256'], imageSha256=manifest['imageSha256'],
                            sourcePixels=manifest['sourcePixels'], exactRgba=manifest['sourceRgbaExactReconstruction'],
                            atlasSize=manifest['nativeSize'], pixelsPerUnit=ppu, qa=qa))
        all_pages += pages
        jobs.append(job)
    after = {p.relative_to(ROOT).as_posix(): pipeline.sha(p) for p in protected}
    if before != after:
        raise RuntimeError('Shared protected files changed during staging; review concurrent mutation before merging.')
    pipeline.write(OUT/'katana-normal-jobs.json', dict(schemaVersion=1, stagingOnly=True, jobs=jobs))
    pipeline.write(OUT/'katana-normal-pages.json', dict(schemaVersion=1, stagingOnly=True, pages=all_pages))
    summary = dict(schemaVersion=1, date='2026-10-25', status='visual-reviewed', visualReviewed=True,
                   imported=False, runtimeVerified=False, protectedFilesUnchanged=True, protectedSha256=before,
                   preparedWholeFigures=sum(r['frames'] for r in results), pages=len(all_pages), results=results,
                   walkStatus='Four generated sheets rejected for repeated leading leg; no Walk replacement staged.',
                   scope='Requiem/Normal only. Rock and other attacks/actions/forms remain pending.')
    pipeline.write(OUT/'staging-summary.json', summary)
    print(json.dumps(dict(frames=summary['preparedWholeFigures'], pages=len(all_pages),
                          qaErrors=sum(r['qa']['errors'] for r in results), qaWarnings=sum(r['qa']['warnings'] for r in results),
                          protectedFilesUnchanged=True), ensure_ascii=False))


if __name__ == '__main__':
    main()
