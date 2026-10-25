"""Prepare a reviewed publication copy, never stage/commit/push or delete files.

Mixed generated sheets remain local. Their publication sources are byte-exact
copies of the approved runtime atlases; only source provenance is rebased in the
publication manifests. Runtime images, IDs, pivots, timings and mappings stay exact.
Requires the project's NumPy/Pillow environment for the independent RGBA proof.
"""
from pathlib import Path
import argparse
import copy
import hashlib
import json
import subprocess
import numpy as np
from PIL import Image
from whole_figure_components import identify

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'work/publicacao-20261021'
OUT = ROOT / 'work/session-20261024/publication'
REMOTE = 'https://github.com/fwbiodev7/darklight-requiem-game.git'
BANK = Path('Assets/Darklight/Resources/Darklight/WeaponRedesign20261023')
ART = Path('ArtSources/WeaponRedesign20261023')
TODAY_ART = Path('ArtSources/WeaponRedesign20261024/candidates')
# Exact approved whole-sheet exceptions; never include the candidate directory wholesale.
TODAY_SELECTED_NAMES = (
    'ReqDaggersNormalLight1S_SW_W20261024v2-source.png',
    'ReqDaggersNormalLight1S_SW_W20261024v2-repair-generated.png',
    'ReqDaggersNormalLight1S_SW_W20261024v2-manifest.json',
    'ReqDaggersNormalLight1S_SW_W20261024v2-repair-prompt.txt',
    'ReqKatanaNormalLight1_S_SW_W-v1-source.png',
    'ReqKatanaNormalLight1_S_SW_W-v1-manifest.json',
    'ReqKatanaNormalLight1_S_SW_W-v2-source.png',
    'ReqKatanaNormalLight1_S_SW_W-v2-manifest.json',
    'ReqKatanaNormalWalkS-v5-source.png',
    'ReqKatanaNormalNeutralWalkS20261024V5-source.png',
    'import-jobs-katana-walkS-v5.json',
    'ReqFistsNormalDireto_S_SW_W-v2-source.png',
    'ReqFistsNormalDireto_S_SW_W-v2-job.json',
    'ReqFistsNormalDireto_S_SW_W-v2-review.json',
    'ReqFistsNormalDireto_S_SW_W-v2-prompt.txt',
    'RockMotorcycleNormalRedesign_Ride8_SlamS-v2-source.png',
    'RockMotorcycleNormalRedesign_Ride8_SlamS-v2-job.json',
)
TODAY_IMPORTED_ORIGINALS = {
    'ReqKatanaNormalNeutralLight1S_Wv1',
    'ReqKatanaNormalNeutralLight1SWv2',
}


def git(*args):
    return subprocess.check_output(['git', '-C', str(DEST), *args], text=True, encoding='utf-8').strip()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def encoded(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def excluded(rel):
    name = rel.as_posix().lower()
    markers = ('rejected', 'draftcatalog', 'candidate', '__pycache__', 'audiosreview', 'audioreview20261017')
    return (any(marker in name for marker in markers)
            or rel.suffix.lower() in ('.pyc', '.log', '.bak', '.mp3') or name.endswith('.mp3.meta'))


def verify_selected_original(name, sheet, job):
    """Recompute source-pixel ownership, then prove every selected RGBA value."""
    original = ROOT / sheet['sourcePath']
    atlas_path = ROOT / BANK / sheet['image']
    if sha(original) != sheet['sourceSha256'] or sha(atlas_path) != sheet['imageSha256']:
        raise ValueError('Changed source or atlas: ' + name)
    image = Image.open(original).convert('RGBA')
    rgba = np.array(image)
    columns = job.get('columns', 4)
    rows = job.get('sourceRows', len(job.get('rows', job.get('directions', []))))
    figures, owner = identify(image, rows=rows, columns=columns,
                              minimum_area=job.get('minimumArea', 15000), alpha_threshold=128)
    yy, xx = np.where((rgba[:, :, 3] > 0) & (owner == 0))
    best = np.full(xx.shape, np.inf)
    assigned = np.zeros(xx.shape, dtype=np.uint8)
    for index, figure in enumerate(figures):
        x0, y0, x1, y1 = figure['grid']
        dx = np.maximum(np.maximum(x0 - xx, xx - x1), 0)
        dy = np.maximum(np.maximum(y0 - yy, yy - y1), 0)
        score = dx * dx + dy * dy + ((xx - (x0 + x1) / 2) ** 2 + (yy - (y0 + y1) / 2) ** 2) * .00001
        closer = score < best
        assigned[closer] = index + 1
        best[closer] = score[closer]
    owner[yy, xx] = assigned
    atlas = np.array(Image.open(atlas_path).convert('RGBA'))
    expected_atlas = np.zeros_like(atlas)
    selected = []
    for frame in sheet['frames']:
        index = frame['sourceFrameIndex']; selected.append(index)
        sx, sy, sw, sh = frame['sourceRect']
        ax, ay, aw, ah = frame['rect']
        dx, dy = frame['sourceOffsetInFrame']
        pixels = rgba[sy:sy + sh, sx:sx + sw].copy()
        pixels[owner[sy:sy + sh, sx:sx + sw] != index + 1] = 0
        expected_atlas[ay + dy:ay + dy + sh, ax + dx:ax + dx + sw] = pixels
    if not np.array_equal(atlas, expected_atlas):
        mismatch = int(np.count_nonzero(np.any(atlas != expected_atlas, axis=2)))
        raise ValueError(f'Original selected figure reconstruction failed: {name}: {mismatch} pixels')
    selected_pixels = int(np.count_nonzero((rgba[:, :, 3] > 0) & np.isin(owner, np.array(selected) + 1)))
    atlas_pixels = int(np.count_nonzero(atlas[:, :, 3]))
    if selected_pixels != atlas_pixels:
        raise ValueError('Selected nonzero-alpha pixel loss: ' + name)
    return dict(selectedFrames=len(selected), sourceFigureCount=len(figures), selectedPixels=selected_pixels,
                packedPixels=atlas_pixels, rgbaExact=True, runtimeImageSha256=sha(atlas_path))


def rebase_sheet(name, sheet):
    result = copy.deepcopy(sheet)
    for field in ('sourcePath', 'sourceSha256', 'sourceSize', 'selectedSourceFrameIndices',
                  'excludedSourceFrameIndices', 'excludedSourceReason', 'originalSourcePixels', 'alphaReview'):
        if field in result:
            result['originalGenerated' + field[0].upper() + field[1:]] = copy.deepcopy(result[field])
    result['sourcePath'] = (ART / 'publication-selected' / (name + '-source-only-approved.png')).as_posix()
    result['sourceSha256'] = result['imageSha256']
    result['sourceSize'] = result['nativeSize'][:]
    result['publicationSourceMethod'] = 'Byte-exact copy of approved whole-figure atlas; original mixed sheet excluded'
    for index, frame in enumerate(result['frames']):
        for field in ('sourceFrameIndex', 'sourceRect', 'sourceAnchor', 'sourceOffsetInFrame'):
            if field in frame:
                frame['originalGenerated' + field[0].upper() + field[1:]] = copy.deepcopy(frame[field])
        x, y, width, height = frame['rect']; px, py = frame['pivot']
        frame.update(sourceFrameIndex=index, sourceRect=frame['rect'][:],
                     sourceAnchor=[round(x + px * width, 6), round(y + (1 - py) * height, 6)],
                     sourceOffsetInFrame=[0, 0], sourceScale=1)
    result['selectedSourceFrameIndices'] = list(range(len(result['frames'])))
    result['excludedSourceFrameIndices'] = []
    result['excludedSourceReason'] = 'Publication source includes only the selected complete figures; external original provenance retained'
    result['originalSourcePixels'] = result['sourcePixels']
    result['alphaReview'] = dict(publicationSourceByteIdenticalToAtlas=True, backgroundRemoved=False,
                                everySelectedNonzeroAlphaPixelRetained=True)
    return result


def prepare(copy_files):
    if git('remote', 'get-url', 'origin') != REMOTE:
        raise ValueError('Unexpected destination remote')
    if git('diff', '--cached', '--name-only'):
        raise ValueError('Publication index must be empty before preparation')
    tracked = set(git('ls-files').splitlines())
    old_report = OUT / 'publication-scope.json'
    prior_scope = set(read(old_report).get('scopePaths', [])) if old_report.exists() else set()
    candidates, excluded_paths = set(), set()
    today_selected = {TODAY_ART / name for name in TODAY_SELECTED_NAMES if (ROOT / TODAY_ART / name).is_file()}
    candidates.update(today_selected)
    folders = ['Assets/Darklight', 'Docs/Darklight', 'Tools/AnimationPreview', 'Tools/TokenEconomy',
               'Tools/Skills/pixel-art-sprite-sheet-master', 'Tools/Audio', 'Packages', 'ProjectSettings',
               'ArtSources/Multiverse20261023', 'ArtSources/PlayerEyeReference20261020',
               'ArtSources/PlayerStyleReference20261020', 'ArtSources/AbsoluteHairReference20261021',
               'ArtSources/CombatHUD20261022']
    for folder in folders:
        candidates.update(p.relative_to(ROOT) for p in (ROOT / folder).rglob('*') if p.is_file())
    catalog = read(ROOT / BANK / 'catalog.json')
    jobs = {}
    for path in sorted((ROOT / ART).glob('import-jobs*.json')):
        jobs.update({job['name']: job for job in read(path).get('jobs', [])})
    sheets = {Path(page['resourcePath']).name for page in catalog['pages']}
    rewritten, derived, proof, mixed_sources = {}, {}, [], set()
    for name in sorted(sheets):
        path = BANK / (name + '.json'); sheet = read(ROOT / path)
        if sheet.get('excludedSourceFrameIndices') and name not in TODAY_IMPORTED_ORIGINALS:
            proof_row = verify_selected_original(name, sheet, jobs[name])
            replacement = rebase_sheet(name, sheet)
            rewritten[path] = encoded(replacement)
            derived[Path(replacement['sourcePath'])] = ROOT / BANK / sheet['image']
            mixed_sources.add(sheet['sourcePath'])
            proof.append(dict(sheet=name, originalSource=sheet['sourcePath'], originalSha256=sheet['sourceSha256'],
                              excludedOriginalFrames=sheet['excludedSourceFrameIndices'],
                              approvedPublicationSource=replacement['sourcePath'], **proof_row))
            for page in catalog['pages']:
                if Path(page['resourcePath']).name == name:
                    for field in ('sourcePath', 'sourceSha256', 'sourceSize'):
                        page['originalGenerated' + field[0].upper() + field[1:]] = copy.deepcopy(page[field])
                        page[field] = copy.deepcopy(replacement[field])
        else:
            source = Path(sheet['sourcePath'])
            if sha(ROOT / source) != sheet['sourceSha256']:
                raise ValueError('Changed approved full source: ' + source.as_posix())
            candidates.add(source)
    rewritten[BANK / 'catalog.json'] = encoded(catalog)
    rewritten[ART / 'publication-selected/PROVENANCE.json'] = encoded(dict(
        schemaVersion=1, date='2026-10-24',
        method='Approved source PNG bytes equal runtime atlas bytes; selected original RGBA ownership independently reconstructed',
        originalMixedSourcesPublished=False, runtimeImagesModified=False, sheets=proof))
    icons = read(ROOT / BANK / 'EquipmentIcons.json')
    candidates.add(Path(icons['sourcePath']))
    for path in (ROOT / ART).rglob('*'):
        if not path.is_file():
            continue
        rel = path.relative_to(ROOT)
        if path.suffix.lower() != '.png' or 'references' in rel.parts:
            candidates.add(rel)
        elif rel not in candidates:
            excluded_paths.add(rel.as_posix())
    candidates.add(Path('ArtSources/AudioDelivery20261023/manifest.json'))
    # Preserve provenance for external MP3 without copying any MP3 source.
    delivery = read(ROOT / 'ArtSources/AudioDelivery20261023/manifest.json')
    external_audio = dict(date='2026-10-24', originalsPublished=False, bossMusicPublished=False,
                         sources=[{key: row[key] for key in ('id', 'original', 'sha256', 'seconds', 'status') if key in row}
                                  for row in delivery['sources']])
    rewritten[Path('AudioSources/Delivery20261023-EXTERNAL-SOURCES.json')] = encoded(external_audio)
    # Legacy fists input selection follows the imported 64 physical pages only.
    fists_index = Path('ArtSources/RuinFists20261021/production-source-index-v2.json')
    proposed = read(ROOT / fists_index)
    fists = read(ROOT / 'Assets/Darklight/Resources/Darklight/RuinFists/catalog.json')
    physical = {Path(page['resourcePath']).name for page in fists['pages'] if not page['paletteVariant']}
    imported = [page for page in proposed['pages'] if page['name'] in physical]
    if len(physical) != 64 or len(imported) != 64:
        raise ValueError('Unexpected legacy fist source selection')
    candidates.add(fists_index)
    for page in imported:
        candidates.add(Path(page['source']))
        job = Path('ArtSources/RuinFists20261021/jobs') / (Path(page['source']).stem + '.json')
        if (ROOT / job).exists(): candidates.add(job)
        for item in page.get('replacements', []): candidates.add(Path(item['source']))
    candidates.update(p.relative_to(ROOT) for p in (ROOT / 'ArtSources/RuinFists20261021/references').rglob('*') if p.is_file())
    for folder in ('outputs/expansao-20261023', 'outputs/redesign-armas-20261023', 'outputs/continuacao-20261024'):
        for name in ('coverage.json', 'redesign-asset-qa.json', 'equipment-icons-qa.json', 'PROGRESS.json',
                     'build-result.json', 'build-smoke.json', 'final-validation.json', 'edit-tests.xml', 'play-tests.xml'):
            rel = Path(folder) / name
            if (ROOT / rel).is_file(): candidates.add(rel)
    scope, changed, overrides, conflicts, hashes = [], [], [], [], {}
    candidates.update(rewritten); candidates.update(derived)
    for rel in sorted(candidates, key=lambda p: p.as_posix()):
        text = rel.as_posix()
        if (excluded(rel) and rel not in today_selected) or text in mixed_sources:
            excluded_paths.add(text); continue
        target = DEST / rel
        if not target.resolve().is_relative_to(DEST.resolve()):
            raise ValueError('Target escaped publication clone')
        if rel in rewritten:
            data = rewritten[rel]
        elif rel in derived:
            data = derived[rel].read_bytes()
        else:
            source = ROOT / rel
            if not source.is_file(): raise FileNotFoundError(source)
            data = source.read_bytes()
        digest = hashlib.sha256(data).hexdigest()
        hashes[text] = digest; scope.append(text)
        if rel in rewritten or rel in derived: overrides.append(text)
        if not target.is_file() or sha(target) != digest:
            if target.exists() and text not in tracked and text not in prior_scope:
                conflicts.append(text); continue
            changed.append(text)
            if copy_files:
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(data)
    if conflicts:
        raise ValueError('Existing unrelated untracked files differ; preserved: ' + ', '.join(conflicts[:12]))
    if git('diff', '--cached', '--name-only'):
        raise ValueError('Index changed during preparation')
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / 'publication-pathspec.nul').write_bytes(b''.join(p.encode('utf-8') + b'\0' for p in scope))
    report = dict(remote=REMOTE, branch=git('branch', '--show-current'), head=git('rev-parse', 'HEAD'),
                  copied=copy_files, scopeCount=len(scope), changedCount=len(changed), excludedCount=len(excluded_paths),
                  selectedNewPaths=sum(path not in tracked for path in scope),
                  mixedSourcesExcluded=len(mixed_sources), canonicalApprovedSources=len(derived), staged=0, deleted=0,
                  scopePaths=scope, changedPaths=changed, excludedPaths=sorted(excluded_paths),
                  deliberateDerivedPaths=overrides, publishedSha256=hashes, selectedSourceProof=proof,
                  runtimeImagesChanged=False, originalSourcesModified=False, originalMp3Published=False,
                  note='Only publication source metadata is rebased. Eight mixed originals remain local; selected figure PNGs are exact approved atlas bytes. No staging/commit/push/deletion performed.')
    (OUT / 'publication-scope.json').write_bytes(encoded(report))
    if copy_files:
        for rel in scope:
            if sha(DEST / rel) != hashes[rel]:
                raise ValueError('Publication bytes changed during copy: ' + rel)
    print(json.dumps({key: report[key] for key in ('branch', 'head', 'copied', 'scopeCount', 'changedCount',
        'excludedCount', 'mixedSourcesExcluded', 'canonicalApprovedSources', 'staged', 'deleted')}, ensure_ascii=False))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--copy', action='store_true', help='Copy into isolated publication clone; never stage')
    prepare(parser.parse_args().copy)
