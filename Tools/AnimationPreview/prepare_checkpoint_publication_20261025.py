"""Extend the reviewed isolated publication allowlist; never stage or push."""
import argparse
import hashlib
import json
from pathlib import Path
import prepare_continuation_publication_20261024 as base

ROOT, DEST = base.ROOT, base.DEST
OUT = ROOT / 'outputs/redesign-armas-20261025'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--copy', action='store_true')
    args = parser.parse_args()
    catalog = base.read(ROOT / base.BANK / 'catalog.json')
    active = {Path(p['resourcePath']).name for p in catalog['pages']} | {'catalog', 'EquipmentIcons'}
    active.update(f'RockMotoForm{f}Ride8SlamS20261025V2' for f in range(4))
    permitted = set()
    for name in active - {'catalog'}:
        sheet = base.read(ROOT / base.BANK / (name + '.json'))
        if not sheet.get('excludedSourceFrameIndices'):
            permitted.add(sheet['sourcePath'])
    original_excluded = base.excluded
    def exclude(rel):
        value = rel.as_posix()
        if rel.parent == base.BANK:
            filename = rel.name.removesuffix('.meta')
            if Path(filename).stem not in active:
                return True
        return False if value in permitted else original_excluded(rel)
    base.excluded = exclude
    base.prepare(args.copy)
    report = base.read(base.OUT / 'publication-scope.json')
    scope = set(report['scopePaths'])
    extras = set(permitted)
    extras.update(p.relative_to(ROOT).as_posix() for p in (ROOT / 'ArtSources/WeaponRedesign20261025/references').glob('*.png'))
    extras.update('outputs/redesign-armas-20261025-retomada1350/' + name for name in (
        'coverage.json', 'final-validation.json', 'focused-edit-tests.xml', 'integration-preparation.json'))
    current = ROOT / 'outputs/redesign-armas-20261025-retomada1850'
    extras.update(p.relative_to(ROOT).as_posix() for p in current.glob('*.json'))
    extras.update(p.relative_to(ROOT).as_posix() for p in current.glob('*.xml'))
    extras.update(p.relative_to(ROOT).as_posix() for p in (current / 'audio').glob('*.json'))
    extras.update(p.relative_to(ROOT).as_posix() for p in (current / 'gameplay').glob('*.json'))
    extras.update('outputs/redesign-armas-20261025/' + name for name in (
        'coverage.json', 'reference-contract.json', 'consolidation.json', 'eight-direction-idle-matrix.json',
        'final-import.json', 'focused-edit-tests.xml'))
    for relative in sorted(extras - scope):
        source = ROOT / relative
        if not source.is_file():
            continue
        target = DEST / relative
        if not target.resolve().is_relative_to(DEST.resolve()):
            raise ValueError('Publication path escaped clone')
        if args.copy:
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(source.read_bytes())
        scope.add(relative)
    ordered = sorted(scope)
    pathspec = OUT / 'publication-pathspec.nul'
    pathspec.write_bytes(b''.join(p.encode('utf-8') + b'\0' for p in ordered))
    summary = dict(branch=base.git('branch', '--show-current'), head=base.git('rev-parse', 'HEAD'),
        copied=args.copy, scopeCount=len(ordered), extraPaths=len(scope - set(report['scopePaths'])),
        pathspec=pathspec.relative_to(ROOT).as_posix(), staged=False, committed=False, pushed=False,
        rejectedRasterPublished=False, scopePaths=ordered)
    (OUT / 'publication-preparation.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps({k:v for k,v in summary.items() if k!='scopePaths'}))


if __name__ == '__main__':
    main()
