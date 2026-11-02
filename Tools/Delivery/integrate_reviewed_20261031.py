"""Integrate explicitly reviewed whole-figure sheets; never generate, stage Git or run Unity."""
import argparse
import hashlib
import json
from pathlib import Path
import shutil

ROOT = Path(__file__).resolve().parents[2]
BANK = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT = ROOT / 'outputs/retomada-20261031'


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def context(page):
    return tuple(page[k] for k in ('skin', 'weapon', 'form', 'direction', 'action')) + (page.get('attackKey') or '',)


def main():
    global OUT
    parser = argparse.ArgumentParser()
    parser.add_argument('--staged', type=Path, required=True)
    parser.add_argument('--pages', type=Path)
    parser.add_argument('--names', nargs='+', required=True)
    parser.add_argument('--output', type=Path, default=OUT,
                        help='Session receipts folder inside this project outputs')
    args = parser.parse_args()
    OUT = args.output.resolve()
    if not OUT.is_relative_to((ROOT / 'outputs').resolve()):
        raise ValueError('Integration receipts must stay inside project outputs')
    fragment = read(args.pages) if args.pages else {'pages': []}
    pages = fragment if isinstance(fragment, list) else fragment['pages']
    catalog_path = BANK / 'catalog.json'
    catalog = read(catalog_path)
    deliveries = []
    prepared = []
    for name in args.names:
        descriptor = args.staged / (name + '.json')
        image = args.staged / (name + '.png')
        sheet = read(descriptor)
        if (not sheet.get('wholeFigures') or sheet.get('status') != 'visual-reviewed'
                or sheet.get('image') != image.name or sha(image) != sheet['imageSha256']):
            raise ValueError('Sheet is not an intact reviewed whole-figure delivery: ' + name)
        source = (ROOT / sheet['sourcePath']).resolve()
        if not source.is_relative_to((ROOT / 'ArtSources').resolve()) or sha(source) != sheet['sourceSha256']:
            raise ValueError('Preserved source missing or changed: ' + name)
        selected = [p for p in pages if p['resourcePath'].rsplit('/', 1)[-1] == name]
        for page in selected:
            for key in ('imageSha256', 'sourcePath', 'sourceSha256', 'nativeSize', 'sourceSize', 'pixelsPerUnit'):
                if page[key] != sheet[key]:
                    raise ValueError('Catalog/sheet disagree: ' + name + '/' + key)
            if page.get('status') != 'visual-reviewed' or not page.get('wholeFigures'):
                raise ValueError('Unreviewed catalog page: ' + page['id'])
            if page['action'] == 'Attack' and not page.get('sourceSpriteNames'):
                # These selected four-frame cuts use two preparations, one contact, one recovery.
                if page['attackKey'] not in ('Dentes Cruzados', 'Leve3') or len(page['spriteNames']) != 4:
                    raise ValueError('No explicit combat phase contract: ' + page['id'])
                page['startupFrames'], page['activeFrames'] = 2, 1
                if not page.get('durationMs'):
                    page['durationMs'] = [40, 40, 100, 180]
        if not selected and not sheet.get('motorcycleTransition'):
            raise ValueError('No selected consumer for reviewed sheet: ' + name)
        for suffix in ('.png', '.json'):
            target = BANK / (name + suffix)
            origin = args.staged / target.name
            if target.exists() and sha(target) != sha(origin):
                if suffix != '.json':
                    raise ValueError('Do not overwrite existing artwork: ' + str(target))
                prior = read(target)
                for key in ('imageSha256', 'sourcePath', 'sourceSha256', 'nativeSize', 'sourceSize', 'pixelsPerUnit', 'frames'):
                    if prior.get(key) != sheet.get(key):
                        raise ValueError('An existing drawing/import contract differs: ' + str(target) + '/' + key)
                backup = OUT / 'before-reviewed-metadata' / target.name
                backup.parent.mkdir(parents=True, exist_ok=True)
                if not backup.exists():
                    shutil.copy2(target, backup)
            prepared.append((origin, target))
        contexts = {context(p) for p in selected}
        catalog['pages'] = [p for p in catalog['pages'] if context(p) not in contexts] + selected
        deliveries.append({'name': name, 'figures': len(sheet['frames']), 'pages': len(selected),
                           'imageSha256': sheet['imageSha256'], 'sourcePath': sheet['sourcePath'],
                           'sourceSha256': sheet['sourceSha256'], 'status': 'visual-reviewed',
                           'copiedToAssets': True, 'unityImported': False, 'runtimeVerified': False})
    keys = [context(p) for p in catalog['pages']]
    if len(keys) != len(set(keys)):
        raise ValueError('Duplicate catalog context')
    OUT.mkdir(parents=True, exist_ok=True)
    snapshot = OUT / 'catalog-before-integration.json'
    if not snapshot.exists():
        shutil.copy2(catalog_path, snapshot)
    for origin, target in prepared:
        shutil.copy2(origin, target)
    catalog['complete'] = False
    catalog_path.write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    receipt = OUT / 'art-integration.json'
    existing = read(receipt) if receipt.exists() else {'deliveries': [], 'redesignComplete': False}
    names = set(args.names)
    existing['deliveries'] = [d for d in existing['deliveries'] if d['name'] not in names] + deliveries
    receipt.write_text(json.dumps(existing, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'sheets': len(deliveries), 'figures': sum(d['figures'] for d in deliveries),
                      'catalogPages': sum(d['pages'] for d in deliveries), 'unityImported': False}))


if __name__ == '__main__':
    main()
