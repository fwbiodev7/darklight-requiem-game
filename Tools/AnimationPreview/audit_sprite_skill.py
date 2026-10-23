"""Adapt existing whole-figure metadata to the skill; audit without changing art/game."""
from collections import defaultdict
import hashlib
import json
from pathlib import Path
import sys

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
sys.path.insert(0, str(Path(__file__).parent))
import sprite_pipeline as sp
from whole_figure_components import identify

OUT = ROOT / 'outputs/revisao-sprites-20261020'
PREVIEW = ROOT / 'outputs/fidelidade-original-20261018'


def reference(skin, form):
    name = skin + 'IdleLivingCardinal20261017'
    if form == 'normal':
        return ROOT / 'Assets/Darklight/Resources/Darklight/TodayCharacterAnimations' / (name + '.png')
    suffix = {'roxa': 'Violet', 'vermelha': 'Crimson', 'branca': 'Absolute'}[form]
    return ROOT / 'Assets/Darklight/Resources/Darklight/TransformedYoung' / (name + '_' + suffix + '.png')


def relative(path, parent):
    import os
    return Path(os.path.relpath(path, parent)).as_posix()


def canonical(page, source, target, prefix='', candidate_frames=None):
    frames = []
    original = page['frames']
    for i, old in enumerate(original):
        f = candidate_frames[i] if candidate_frames else old
        x0, y0, x1, y1 = f['rect']
        ax, ay = f.get('previewPivot', f['anchor'])
        w, h = x1 - x0, y1 - y0
        frames.append(dict(id=f'{prefix}f{i:02}', rect=[x0, y0, w, h],
                           pivot=[(ax - x0) / w, 1 - (ay - y0) / h],
                           durationMs=page.get('durationsMs', [100] * len(original))[i],
                           action=page['kind'], direction='S', skin=page['skin'], form=page['form']))
    clips = [dict(name=page['kind'] + '_' + direction, frames=[frames[i]['id'] for i in indexes],
                  loop=page['kind'] == 'walk', allowStatic=len(indexes) == 1)
             for direction, indexes in page['directions'].items()]
    ref = reference(page['skin'], page['form'])
    with Image.open(source) as im:
        size = list(im.size)
    # One scale per source, inherited from the actual consumer; no anatomical stretching.
    d = dict(schemaVersion=1, image=relative(source, target.parent), imageSha256=sp.digest(source),
             nativeSize=size, pixelsPerUnit=1 / page['uniformRenderScale'], status='candidate',
             frames=frames, clips=clips, requiredClips=[c['name'] for c in clips],
             quality=dict(alphaThreshold=128, maxHeightRatio=1.8),
             baseline=dict(image=relative(ref, target.parent), sha256=sp.digest(ref)),
             notes=['PPU uses preview coordinate units; adapt to Unity world PPU before import.',
                    'Other directions are missing, not certified by this source-specific audit.',
                    'Whole-frame rectangles only; no character parts assembled.'])
    sp.write(target, d)
    return d


def main():
    OUT.mkdir(exist_ok=True)
    metadata = OUT / 'metadata'
    report_dir = OUT / 'bank-audit'
    metadata.mkdir(exist_ok=True)
    report_dir.mkdir(exist_ok=True)
    manifest = sp.read(PREVIEW / 'pacote-v6/manifesto.json')
    by_sheet = defaultdict(list)
    for page in manifest['pages']:
        by_sheet[page['frames'][0]['sheet']].append(page)
    summary, source_failures = [], []
    for selected in manifest['sheets']:
        source = PREVIEW / selected['file']
        if sp.digest(source) != selected['sha256']:
            source_failures.append(selected['file'])
        page = by_sheet[selected['file']][0]
        target = metadata / (source.stem + '.json')
        if not target.exists():
            canonical(page, source, target)
        report_file = report_dir / (source.stem + '.json')
        if report_file.exists():
            report = sp.read(report_file)
            if report['sourceSha256'] != sp.digest(source):
                raise ValueError('Source changed since this audit: ' + str(source))
        else:
            _, _, report = sp.inspect(target)
            sp.write(report_file, report)
        summary.append(dict(file=selected['file'], frames=len(report['frames']),
                            errors=report['errors'], warningCount=len(report['warnings']),
                            otherDirectionsVerified=False, visualApproval=False))
    candidate = OUT / 'Requiem-adagas-roxa-candidata-v1.png'
    page = next(p for p in manifest['pages'] if p['skin'] == 'Requiem' and p['weapon'] == 'adagas'
                and p['form'] == 'roxa' and p['kind'] == 'attack')
    candidate_issues = []
    frames = None
    with Image.open(candidate) as im:
        try:
            frames, _ = identify(im.convert('RGBA'), rows=6, columns=4)
        except ValueError as exc:
            candidate_issues.append(str(exc))
    if frames:
        candidate_manifest = OUT / 'candidata-v1.json'
        if not candidate_manifest.exists():
            canonical(page, candidate, candidate_manifest, candidate_frames=frames)
    result = dict(date='2026-10-20', sourceSheets=len(summary),
                  frames=sum(s['frames'] for s in summary), sourceHashFailures=source_failures,
                  structureErrors=sum(len(s['errors']) for s in summary),
                  warnings=sum(s['warningCount'] for s in summary), sheets=summary,
                  candidateCompleteFigures=len(frames) if frames else None,
                  candidateIssues=candidate_issues, candidateImported=False,
                  visualApproval=False, unityRuntimeVerified=False,
                  limitations=['Bounds and hashes do not certify anatomy or clean under-eye skin.',
                               'Source art has many partially transparent pixels and smooth shading.',
                               'Existing source ownership maps remain authoritative for overlapping effects.',
                               'New candidate anchors estimated from intact silhouettes; review before integration.'])
    sp.write(OUT / 'AUDITORIA-BANCO.json', result)
    print(json.dumps({k: v for k, v in result.items() if k not in ('sheets', 'limitations')}))
    return 0 if not source_failures and not result['structureErrors'] and not candidate_issues else 2


if __name__ == '__main__':
    raise SystemExit(main())
