"""Prepare the reviewed whole-figure Presas1 batch without mutating shared catalog."""
import json
from pathlib import Path

import prepare_weapon_redesign as pipeline

ROOT = Path(__file__).resolve().parents[2]
CANDIDATES = ROOT / 'ArtSources/WeaponRedesign20261024/candidates'
NAME = 'ReqDaggersNormalNeutralPresas1S_SW_W20261025V2'


def main():
    reviewed = json.loads((CANDIDATES / 'ReqDaggersNormalLight1S_SW_W20261024v2-manifest.json').read_text(encoding='utf-8'))
    assert reviewed['wholeFigures'] and reviewed['attackKey'] == 'Presas1'
    source = CANDIDATES / reviewed['image']
    assert pipeline.sha(source) == reviewed['imageSha256']
    job = dict(name=NAME, source=str(source), skin=0, weapon=1, form=0,
               columns=4, sourceRows=3, minimumArea=15000,
               supportAnchors={str(i): dict(anchor=frame['anchorSourcePx'],
                   method='Reviewed complete-figure foot-support anchor from source manifest; no anatomy edits.')
                   for i, frame in enumerate(reviewed['frames'])},
               rows=[dict(sourceRow=i, direction=i, action='Attack', attackKey='Presas1',
                          sourceSpriteNames=clip['sourceSpriteNames'])
                     for i, clip in enumerate(reviewed['clips'])])
    pipeline.write(CANDIDATES / 'import-jobs-presas-v2-20261025.json', dict(jobs=[job]))
    manifest, pages = pipeline.prepare(job)
    # Preview follows the existing fractional startup and exact legacy phase names.
    for i, frame in enumerate(manifest['frames']):
        frame['durationMs'] = reviewed['frames'][i]['durationMs']
        frame['phase'] = reviewed['frames'][i]['phase']
    manifest['timingNote'] = 'Preview rounds 29.75ms startup to30ms; runtime retains legacy sourceSpriteNames and gameplay timings.'
    manifest['review'] = dict(sourceContactSeen=True, packedContactSeen=False,
                            motionPreviewSeen=False, unityImported=False, unityRuntimeVerified=False,
                            scope='Requiem/Normal/Presas1/S,SW,W; all other skins/forms/directions/actions pending.')
    pipeline.write(pipeline.RES / (NAME + '.json'), manifest)
    fragment = ROOT / 'outputs/redesign-armas-20261025/presas-pages.json'
    pipeline.write(fragment, dict(complete=False, pages=pages))
    report = pipeline.qa(NAME)
    pipeline.write(ROOT / 'outputs/redesign-armas-20261025/presas-preparation.json',
                   dict(name=NAME, frames=len(manifest['frames']), clips=len(pages),
                        imageSha256=manifest['imageSha256'], sourceSha256=manifest['sourceSha256'],
                        nativeSize=manifest['nativeSize'], pixelsPerUnit=manifest['pixelsPerUnit'],
                        exactRgba=manifest['sourceRgbaExactReconstruction'], qa=report,
                        unityImported=False, runtimeVerified=False, audio=False))
    print(json.dumps(dict(frames=len(manifest['frames']), pages=len(pages),
                          pixelsPerUnit=manifest['pixelsPerUnit'], qa=report,
                          fragment=fragment.relative_to(ROOT).as_posix())))


if __name__ == '__main__':
    main()
