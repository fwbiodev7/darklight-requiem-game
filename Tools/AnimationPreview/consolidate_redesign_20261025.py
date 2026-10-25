"""Merge explicitly reviewed fragments; keep missing art and gameplay status explicit."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from prepare_weapon_redesign import ROOT, RES, write

OUT = ROOT / 'outputs/redesign-armas-20261025'


def context(page):
    return tuple(page.get(key, '') for key in ('skin', 'weapon', 'form', 'direction', 'action', 'attackKey'))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--fragment', type=Path, action='append', default=[])
    parser.add_argument('--staging-folder', type=Path)
    args = parser.parse_args()
    catalog_path = RES / 'catalog.json'
    before = catalog_path.read_bytes()
    catalog = json.loads(before.decode('utf-8'))
    old_key = 'Direto da Ru\u00c3\u00adna'
    repaired = 0
    for page in catalog['pages']:
        if page['weapon'] == 2 and page.get('attackKey') == old_key:
            page['attackKey'] = 'Direto da Ru\u00edna'
            repaired += 1
    incoming = {}
    for fragment_path in args.fragment:
        fragment = json.loads(fragment_path.read_text(encoding='utf-8'))
        for page in fragment['pages']:
            key = context(page)
            if key in incoming and incoming[key] != page:
                raise ValueError('Conflicting reviewed fragments: ' + repr(key))
            sheet_path = ROOT / 'Assets/Darklight/Resources' / (page['resourcePath'] + '.json')
            if args.staging_folder and not sheet_path.exists():
                for extension in ('.json', '.png'):
                    source = args.staging_folder / (sheet_path.stem + extension)
                    destination = sheet_path.with_suffix(extension)
                    if not source.is_file() or destination.exists():
                        raise ValueError('Staged atlas missing or destination collision: ' + str(destination))
                    shutil.copy2(source, destination)
            sheet = json.loads(sheet_path.read_text(encoding='utf-8'))
            if sheet['status'] != 'visual-reviewed' or not sheet['wholeFigures']:
                raise ValueError('Unreviewed source: ' + str(sheet_path))
            for field in ('imageSha256', 'sourceSha256', 'sourcePath', 'pixelsPerUnit'):
                if page[field] != sheet[field]:
                    raise ValueError('Page/manifest mismatch: ' + page['id'] + '/' + field)
            incoming[key] = page
    backup = OUT / 'catalog-before-20261025.json'
    if not backup.exists():
        backup.parent.mkdir(parents=True, exist_ok=True)
        backup.write_bytes(before)
    catalog['pages'] = [page for page in catalog['pages'] if context(page) not in incoming] + list(incoming.values())
    catalog['complete'] = False
    write(catalog_path, catalog)
    directions = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']
    matrix = []
    for skin in range(2):
        for weapon in range(3):
            for form in range(4):
                entries = []
                for direction, label in enumerate(directions):
                    pages = [p for p in catalog['pages'] if (p['skin'], p['weapon'], p['form'], p['direction']) == (skin, weapon, form, direction)]
                    idle = next((p for p in pages if p['action'] == 'Idle'), None)
                    entries.append(dict(direction=label,
                        staticPoseSourceAvailable=idle is not None,
                        authoredIdleCycleAvailable=bool(idle and len(set(idle['spriteNames'])) > 1),
                        newAttackKeys=[p['attackKey'] for p in pages if p['action'] == 'Attack'],
                        otherNewActions=[p['action'] for p in pages if p['action'] not in ('Attack', 'Idle')],
                        directionVisualApproval=False, gameplayVerified=False))
                matrix.append(dict(skin=skin, weapon=weapon, form=form, directions=entries))
    write(OUT / 'eight-direction-idle-matrix.json', dict(date='2026-10-25', complete=False,
        meaning='Source/catalog availability only; eight names never certify visual orientations or gameplay.', contexts=matrix))
    references = []
    for path in sorted((ROOT / 'ArtSources/WeaponRedesign20261025/references').glob('*.png')):
        references.append(dict(path=path.relative_to(ROOT).as_posix(), sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    write(OUT / 'reference-contract.json', dict(date='2026-10-25', references=references,
        contract='Docs/Darklight/CONTRATO-VISUAL-20261025.md'))
    receipt = dict(fragments=[str(p) for p in args.fragment], addedOrReplacedContexts=len(incoming),
        repairedExactFistAttackKeys=repaired, catalogPages=len(catalog['pages']), complete=False,
        imported=False, gameplayVerified=False, audioPlayed=False)
    write(OUT / 'consolidation.json', receipt)
    print(json.dumps(receipt, ensure_ascii=False))


if __name__ == '__main__':
    main()
