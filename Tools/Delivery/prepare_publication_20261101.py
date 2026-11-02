"""Prepare only this session's reviewed delivery in the existing publication clone.

Never stages, commits, pushes, merges or copies rejected candidate directories.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
DEST = ROOT / 'work/publicacao-20261021'
OUT = ROOT / 'outputs/retomada-20261101'
BANK = Path('Assets/Darklight/Resources/Darklight/WeaponRedesign20261023')
BRANCH = 'integracao-completa-20261026'
REMOTE = 'https://github.com/fwbiodev7/darklight-requiem-game.git'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def encoded(value):
    return (json.dumps(value, ensure_ascii=False, indent=2) + '\n').encode('utf-8')


def git(*args):
    return subprocess.check_output(['git', '-C', str(DEST), *args], text=True, encoding='utf-8').strip()


def context(page):
    return tuple(page[key] for key in ('skin', 'weapon', 'form', 'direction', 'action')) + (page.get('attackKey') or '',)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--copy', action='store_true')
    args = parser.parse_args()
    if git('branch', '--show-current') != BRANCH or git('remote', 'get-url', 'origin') != REMOTE:
        raise RuntimeError('Preserve the established publication branch/remote')
    if git('diff', '--cached', '--name-only'):
        raise RuntimeError('Preserve the existing index; it must be empty before preparation')
    catalog = read(DEST / BANK / 'catalog.json')
    current = read(ROOT / BANK / 'catalog.json')
    active_names={Path(p['resourcePath']).name for p in current['pages']}
    deliveries=[d for d in read(OUT / 'art-integration.json')['deliveries']
                if d['name'] in active_names and not d.get('publicationExcluded')]
    files = {}
    for delivery in deliveries:
        if args.copy and not (delivery.get('unityImported') and delivery.get('runtimeVerified')):
            raise RuntimeError('Do not publish an unverified delivery: ' + delivery['name'])
        name = delivery['name']
        sheet = read(ROOT / BANK / (name + '.json'))
        source = Path(sheet['sourcePath'])
        if (not (ROOT / source).resolve().is_relative_to(ROOT / 'ArtSources') or
                any(word in source.as_posix().lower() for word in ('reference', 'rejected'))):
            raise ValueError('Private/rejected source cannot be published')
        for relative, digest in ((source, sheet['sourceSha256']),
                                 (BANK / (name + '.png'), sheet['imageSha256'])):
            data = (ROOT / relative).read_bytes()
            if hashlib.sha256(data).hexdigest() != digest:
                raise ValueError('Source/image hash changed: ' + relative.as_posix())
            files[relative] = data
        # Reuse the established byte-exact selected-atlas provenance workflow.
        # The rejected figures in a mixed original remain local.
        replacement = None
        if sheet.get('excludedSourceFrameIndices'):
            sys.path.insert(0, str(ROOT / 'Tools/AnimationPreview'))
            from prepare_continuation_publication_20261024 import rebase_sheet
            replacement = rebase_sheet(name, sheet)
            del files[source]
            files[Path(replacement['sourcePath'])] = files[BANK / (name + '.png')]
        for suffix in ('.json', '.json.meta', '.png.meta'):
            relative = BANK / (name + suffix)
            if not (ROOT / relative).is_file():
                raise ValueError('Missing imported descriptor/meta: ' + relative.as_posix())
            files[relative] = (ROOT / relative).read_bytes()
        if replacement:
            files[BANK / (name + '.json')] = encoded(replacement)
        pages = [copy.deepcopy(p) for p in current['pages'] if Path(p['resourcePath']).name == name]
        if len(pages) != delivery['pages']:
            raise ValueError('Unexpected context count: ' + name)
        if replacement:
            for page in pages:
                for field in ('sourcePath', 'sourceSha256', 'sourceSize'):
                    page['originalGenerated' + field[0].upper() + field[1:]] = page[field]
                    page[field] = copy.deepcopy(replacement[field])
        keys = {context(page) for page in pages}
        catalog['pages'] = [p for p in catalog['pages'] if context(p) not in keys] + pages
    catalog['complete'] = False
    keys = [context(p) for p in catalog['pages']]
    if len(keys) != len(set(keys)):
        raise ValueError('Duplicate publication context')
    files[BANK / 'catalog.json'] = encoded(catalog)
    explicit = [
        'Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs',
        'Assets/Darklight/Runtime/Character/DarklightCharacterView.cs',
        'Assets/Darklight/Runtime/World/DarklightActionParticles.cs',
        'Assets/Darklight/Runtime/World/DarklightActionParticles.cs.meta',
        'Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs',
        'Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs.meta',
        'Assets/Darklight/Runtime/Campaign/DarklightRegionAtmosphere.cs',
        'Assets/Darklight/Runtime/Campaign/DarklightRegionExploration.cs',
        'Assets/Darklight/Runtime/Campaign/DarklightCampaignSettings.cs',
        'Assets/Darklight/Runtime/Campaign/DarklightRegionAtmosphere.cs.meta',
        'Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader',
        'Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader.meta',
        'Assets/Darklight/Editor/DarklightWeaponRedesignBuild.cs',
        'Assets/Darklight/Editor/DarklightWeaponRedesignBuild.cs.meta',
        'Assets/Darklight/Editor/DarklightScope26Delivery.cs',
        'Assets/Darklight/Tests/PlayMode/DarklightAtmosphereGameplayTests.cs',
        'Assets/Darklight/Tests/PlayMode/DarklightAtmosphereGameplayTests.cs.meta',
        'Assets/Darklight/Tests/PlayMode/DarklightActionParticleGameplayTests.cs',
        'Assets/Darklight/Tests/PlayMode/DarklightActionParticleGameplayTests.cs.meta',
        'Assets/Darklight/Tests/EditMode/DarklightWeaponRedesignTests.cs',
        'Assets/Darklight/Tests/PlayMode/DarklightWeaponRedesignGameplayTests.cs',
        'Tools/AnimationPreview/prepare_weapon_redesign.py',
        'Assets/Darklight/Tests/PlayMode/DarklightRecoveredAnimationGameplayTests.cs',
        'Tools/Delivery/integrate_reviewed_20261031.py', 'Tools/Delivery/run_release_20261031.py',
        'Tools/Delivery/verify_preservation_20261101.py', 'Tools/Delivery/prepare_publication_20261101.py',
        'Tools/Delivery/prepare_action_particles_20261101.py',
        'Tools/Delivery/check_atmosphere_20261101.py',
        'Tools/Delivery/retry_windows_build_20261101.py',
        'Tools/Delivery/run_guarded_build_20261101.py',
        'Tools/Delivery/prepare_selective_katana_repair_20261101.py',
        'Tools/Delivery/prepare_proportion_audit_20261101.py',
        'Tools/Delivery/finalize_proportion_audit_20261101.py',
        'Docs/Darklight/RETOMADA-20261101-PEDIDO.md', 'Docs/Darklight/FECHAMENTO-20261101.md',
        'Docs/Darklight/GOTHIC-ECLIPSE-PLANO-20261101.md',
        'Docs/Darklight/GOTHIC-ECLIPSE-PEDIDO-20261101.md']
    explicit += [p.relative_to(ROOT).as_posix() for p in (ROOT / 'Assets/Darklight/Resources/Darklight/ActionParticles20261101').glob('*') if p.is_file()]
    explicit += ['Assets/Darklight/Resources/Darklight/ActionParticles20261101.meta',
                 'ArtSources/ActionParticles20261101/ActionParticlesV1-source.png',
                 'outputs/retomada-20261101/particles/receipt.json']
    # Only the selected imported V2 descriptors determine eligible source art.
    # Never enumerate the candidate/staged/source directories for publication.
    for folder, stem in (('AtmosphereTextures20261101', 'AtmosphereTextures'),
                         ('FastWeaponParticles20261101', 'FastWeaponDodge')):
        resource = Path('Assets/Darklight/Resources/Darklight') / folder
        descriptor = read(ROOT / resource / (stem + '.json'))
        if descriptor.get('status') != 'visual-reviewed':
            raise ValueError('Unreviewed atmosphere/particle descriptor: ' + stem)
        source = Path(descriptor['sourcePath'])
        if (not (ROOT / source).resolve().is_relative_to((ROOT / 'ArtSources').resolve()) or
                any(word in source.as_posix().lower() for word in
                    ('reference', 'rejected', 'superseded', 'candidate'))):
            raise ValueError('Private/rejected atmosphere/particle source cannot be published')
        for relative, digest in ((source, descriptor['sourceSha256']),
                                 (resource / (stem + '.png'), descriptor['imageSha256'])):
            data = (ROOT / relative).read_bytes()
            if hashlib.sha256(data).hexdigest() != digest:
                raise ValueError('Atmosphere/particle source/image hash changed: ' + relative.as_posix())
            files[relative] = data
        explicit.append(resource.as_posix() + '.meta')
        explicit += [(resource / (stem + suffix)).as_posix()
                     for suffix in ('.json', '.json.meta', '.png.meta')]
    explicit += ['outputs/retomada-20261101/' + relative for relative in (
        'katana-walk/CHECKPOINT.md', 'moto-walk/REPORT.md', 'moto-walk/checkpoint.json',
        'moto-walk-edit/REPORT.md', 'punhos-walk/REPORT.md', 'katana-dodge/CHECKPOINT.md',
        'katana-dodge/source-hashes.json', 'moto-dodge/REPORT.md', 'moto-dodge/checkpoint.json',
        'punhos-dodge/REPORT.md', 'punhos-dodge/checkpoint.json')]
    explicit += [p.relative_to(ROOT).as_posix() for p in (OUT/'proportion-audit').glob('*')
                 if p.is_file() and p.suffix in ('.md','.json')]
    explicit += ['outputs/retomada-20261101/selective-repair/integration/rgba-preservation-proof.json']
    for report_folder in ('atmosphere', 'fast-weapon-particles'):
        explicit += [p.relative_to(ROOT).as_posix() for p in (OUT/report_folder).glob('*')
                     if p.is_file() and p.suffix in ('.md','.json','.xml')]
    explicit += [p.relative_to(ROOT).as_posix() for p in OUT.glob('*') if p.is_file() and
                 p.suffix in ('.md', '.xml', '.json') and not p.name.startswith(('catalog-before', 'publication-', 'final-git'))]
    explicit += [p.relative_to(ROOT).as_posix() for p in (OUT / 'standalone').glob('*.json')
                 if p.name in ('result.json', 'capture-summary.json')]
    for relative in explicit:
        path = ROOT / relative
        if path.is_file():
            files[Path(relative)] = path.read_bytes()
    for relative, data in files.items():
        target = (DEST / relative).resolve()
        if not target.is_relative_to(DEST.resolve()):
            raise ValueError('Publication path escaped clone')
        if args.copy:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(data)
    ordered = sorted(p.as_posix() for p in files)
    (OUT / 'publication-pathspec.nul').write_bytes(b''.join(p.encode('utf-8') + b'\0' for p in ordered))
    result = {'branch': BRANCH, 'initialHead': git('rev-parse', 'HEAD'), 'copied': args.copy,
              'scopePaths': ordered, 'sha256': {p.as_posix(): hashlib.sha256(data).hexdigest() for p, data in files.items()},
              'existingPublicationProvenancePreserved': True, 'sourcesRejectedOrPrivateIncluded': False,
              'buildsSavesCachesIncluded': False, 'staged': False, 'pushed': False}
    (OUT / 'publication-preparation.json').write_bytes(encoded(result))
    print(json.dumps({'paths': len(ordered), 'copied': args.copy, 'branch': BRANCH}))


if __name__ == '__main__':
    main()
