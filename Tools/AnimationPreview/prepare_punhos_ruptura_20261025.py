"""Pack reviewed whole Punhos/Ruptura figures; never change shared catalog."""
import argparse
import json
from pathlib import Path
import prepare_weapon_redesign as pipeline

ROOT = pipeline.ROOT
OUT = ROOT / 'outputs/redesign-armas-20261025/punhos-ruptura'


def main():
    p = argparse.ArgumentParser()
    p.add_argument('--job', type=Path, required=True)
    a = p.parse_args()
    job = json.loads(a.job.read_text(encoding='utf-8'))
    manifest, pages = pipeline.prepare(job)
    guide = job['calibrationGuide']
    manifest['pixelsPerUnit'] = guide['pixelsPerUnit']
    manifest['calibrationGuide'] = guide
    for page in pages:
        page['pixelsPerUnit'] = manifest['pixelsPerUnit']
    for frame in manifest['frames']:
        if frame['action'] == 'attack':
            index = int(frame['phase'].split('_')[-1])
            frame['durationMs'] = job.get('attackPreviewDurationsMs', [75,65,130])[index]
            frame['phase'] = ['anticipation','contact','recovery'][index]
    manifest['review'] = dict(wholeSourceSeen=True, packedContactSeen=False,
        motionSeen=False, unityImported=False, gameplayVerified=False,
        visualApproval=False, findings=job['visualFindings'])
    manifest['provenance'] = job['provenance']
    pipeline.write(pipeline.RES / (job['name'] + '.json'), manifest)
    qa = pipeline.qa(job['name'])
    report = dict(name=job['name'], frames=len(manifest['frames']), pages=len(pages),
        pixelsPerUnit=manifest['pixelsPerUnit'], sourceSha256=manifest['sourceSha256'],
        sourceRgbaExact=manifest['sourceRgbaExactReconstruction'], qa=qa,
        unityImported=False, gameplayVerified=False, motionSeen=False)
    fragment = OUT / (job['name'] + '-pages.json')
    pipeline.write(fragment, dict(schemaVersion=1, complete=False, pages=pages))
    pipeline.write(OUT / (job['name'] + '-preparation.json'), report)
    print(json.dumps(dict(**report, fragment=fragment.relative_to(ROOT).as_posix())))


if __name__ == '__main__':
    main()
