"""Register twelve new whole-figure back-view attacks; leave shared catalog alone."""
import json
from pathlib import Path

import prepare_weapon_redesign as pipeline

ROOT = Path(__file__).resolve().parents[2]
CANDIDATES = ROOT / 'ArtSources/WeaponRedesign20261024/candidates'
STEM = 'ReqDaggersNormalPresas1NW_N_NE20261025v1'
NAME = 'ReqDaggersNormalNeutralPresas1NW_N_NE20261025V1'
ANCHORS = [[195,353],[622,347],[980,346],[1357,348],
           [200,673],[599,668],[963,669],[1362,673],
           [185,990],[568,968],[965,989],[1369,971]]
PHASES = ['anticipation','contact','follow-through','recovery']


def main():
    source = CANDIDATES / (STEM + '-source.png')
    rows = [dict(sourceRow=0, direction=3, action='Attack', attackKey='Presas1',
                 sourceSpriteNames=['Today_RequiemAttackDaggersYoungDiagonal_'+str(i) for i in range(4,8)]),
            dict(sourceRow=1, direction=4, action='Attack', attackKey='Presas1',
                 sourceSpriteNames=['Today_RequiemAttackDaggersYoungCardinal_'+str(i) for i in range(8,12)]),
            dict(sourceRow=2, direction=5, action='Attack', attackKey='Presas1',
                 sourceSpriteNames=['Today_RequiemAttackDaggersYoungDiagonal_'+str(i) for i in range(8,12)])]
    job = dict(name=NAME, source=str(source), skin=0, weapon=1, form=0,
               columns=4, sourceRows=3, minimumArea=15000, rows=rows,
               supportAnchors={str(i):dict(anchor=anchor,
                   method='Projected midpoint between complete boot supports; lower sole baseline, visual source inspection.')
                   for i,anchor in enumerate(ANCHORS)})
    pipeline.write(CANDIDATES / 'import-jobs-presas-missing-views-20261025.json', dict(jobs=[job]))
    manifest,pages = pipeline.prepare(job)
    for i,frame in enumerate(manifest['frames']):
        frame['durationMs'] = [30,45,38,38][i%4]
        frame['phase'] = PHASES[i%4]
    manifest['timingNote'] = 'Runtime uses exact legacy phase names; preview rounds 29.75ms startup to30ms.'
    manifest['provenance'] = dict(generator='integrated image_gen',
         promptFile=(CANDIDATES / (STEM+'-prompt.txt')).relative_to(ROOT).as_posix(),
         reference='ArtSources/WeaponRedesign20261024/candidates/ReqDaggersNormalLight1S_SW_W20261024v2-source.png',
         referenceSha256='b918d0e89a14e1e6bf45597a075e7cddce2d8c96890c6ffe393033b72b4d83ff',
         generatedFile='C:/Users/Usuario/.codex/generated_images/01a172e6-5797-7ca2-9fb4-809363b6444e/exec-dd22f0bf-1844-4fac-b1db-4f01f6373e08.png')
    manifest['review'] = dict(sourceContactSeen=True, packedContactSeen=False,
         motionPreviewSeen=False, visualApproval=False, unityImported=False, unityRuntimeVerified=False,
         findings='12 complete paired-dagger figures; neutral steel, dark-brown hair and black/wine costume. Back views NW/N/NE have no front-facing eyes; shoulder rotation follows attack. No transformation energy.',
         scope='Requiem/Normal/Presas1/NW,N,NE; directions E/SE and remaining skins/forms/actions pending.')
    pipeline.write(pipeline.RES / (NAME+'.json'), manifest)
    pipeline.write(CANDIDATES / (STEM+'-manifest.json'), manifest)
    fragment = ROOT/'outputs/redesign-armas-20261025/presas-missing-views-pages.json'
    pipeline.write(fragment,dict(complete=False,pages=pages))
    report=pipeline.qa(NAME)
    pipeline.write(ROOT/'outputs/redesign-armas-20261025/presas-missing-views-preparation.json',
         dict(name=NAME,frames=12,clips=3,nativeSize=manifest['nativeSize'],pixelsPerUnit=manifest['pixelsPerUnit'],
              exactRgba=True,qa=report,unityImported=False,runtimeVerified=False,audio=False))
    print(json.dumps(dict(frames=12,pages=3,pixelsPerUnit=manifest['pixelsPerUnit'],qa=report,
                         fragment=fragment.relative_to(ROOT).as_posix())))


if __name__=='__main__':
    main()
