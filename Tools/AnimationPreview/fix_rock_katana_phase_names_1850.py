"""Repair Rock's directional phase mappings without changing raster artwork."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/redesign-armas-20261025-retomada1850'
STAGING = ROOT / 'outputs/redesign-armas-20261025-retomada1350/katana-normal'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def names(direction):
    return [f'Today_RockAttackKatanaYoung_{direction * 4 + phase}' for phase in range(4)]


def main():
    today = (ROOT / 'Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset').read_text(encoding='utf-8')
    imported = set(re.findall(r'- (Today_RockAttackKatanaYoung_\d+)\s*', today))
    assert len(imported) == 32
    paths = [ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json',
             ROOT / 'ArtSources/WeaponRedesign20261023/import-jobs-katana-rock-1350.json']
    paths += list(STAGING.glob('*pages.json')) + list(STAGING.glob('*jobs.json'))
    changed, repaired = [], 0
    for path in sorted(set(paths)):
        before = path.read_bytes()
        data = json.loads(before.decode('utf-8-sig'))
        count = 0
        for page in data.get('pages', []):
            if (page.get('skin'), page.get('weapon'), page.get('form'), page.get('action'), page.get('attackKey')) != (1, 0, 0, 'Attack', 'Leve1'):
                continue
            expected = names(page['direction'])
            assert set(expected).issubset(imported)
            if page['sourceSpriteNames'] != expected:
                assert len(page['sourceSpriteNames']) == 4
                page['sourceSpriteNames'] = expected
                count += 1
        for job in data.get('jobs', []):
            if (job.get('skin'), job.get('weapon'), job.get('form')) != (1, 0, 0):
                continue
            for row in job.get('rows', []):
                if row.get('action') == 'Attack' and row.get('attackKey') == 'Leve1' and row.get('sourceSpriteNames') != names(row['direction']):
                    row['sourceSpriteNames'] = names(row['direction'])
                    count += 1
        if not count:
            continue
        relative = path.relative_to(ROOT)
        backup = ROOT / 'work/retomada1850/mapping-before' / relative
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            backup.write_bytes(before)
        after = (json.dumps(data, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
        path.write_bytes(after)
        changed.append(dict(path=relative.as_posix(), mappings=count, beforeSha256=digest(before), afterSha256=digest(after)))
        if path.name == 'catalog.json':
            repaired = count
    OUT.mkdir(parents=True, exist_ok=True)
    report = dict(repairedCatalogPages=repaired, changedFiles=changed, actualSourceNames=sorted(imported),
        rasterModified=False, timingsModified=False, gameplayVerified=False)
    (OUT / 'rock-katana-mapping-fix.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(dict(repairedCatalogPages=repaired, changedFiles=len(changed), rasterModified=False)))


if __name__ == '__main__':
    main()
