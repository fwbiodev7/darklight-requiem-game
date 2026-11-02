"""Record/verify byte preservation of existing Fabius art, maps and runtime.

This proves unchanged bytes, not visual fidelity of newly authored figures.
"""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/retomada-20261101'
BASELINE = OUT / 'preservation-before.json'
BANK = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--record', action='store_true')
    parser.add_argument('--allow-state-timing-support', action='store_true',
                        help='Permit only the documented native state timing consumer change')
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    if args.record:
        if BASELINE.exists():
            raise RuntimeError('Preserve the original baseline; do not overwrite it')
        paths = set(BANK.glob('*.png'))
        for descriptor in BANK.glob('*.json'):
            data = json.loads(descriptor.read_text(encoding='utf-8-sig'))
            if data.get('sourcePath'):
                source = (ROOT / data['sourcePath']).resolve()
                if not source.is_relative_to(ROOT / 'ArtSources'):
                    raise ValueError('Source escaped ArtSources')
                paths.add(source)
        for folder in ('Assets/Darklight/Resources/Darklight/Maps',
                       'Assets/Darklight/Runtime', 'Assets/Darklight/Scenes'):
            paths.update(p for p in (ROOT / folder).rglob('*')
                         if p.is_file() and p.suffix != '.meta')
        paths.add(ROOT / 'ProjectSettings/ProjectVersion.txt')
        hashes = {p.relative_to(ROOT).as_posix(): sha(p) for p in sorted(paths)}
        BASELINE.write_text(json.dumps({'sha256': hashes}, indent=2) + '\n', encoding='utf-8')
        print(json.dumps({'recorded': len(hashes), 'baseline': str(BASELINE)}))
        return
    hashes = json.loads(BASELINE.read_text(encoding='utf-8'))['sha256']
    changed = [name for name, digest in hashes.items()
               if not (ROOT / name).is_file() or sha(ROOT / name) != digest]
    allowed = {'Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs',
               'Assets/Darklight/Runtime/Character/DarklightCharacterView.cs',
               'Assets/Darklight/Runtime/Campaign/DarklightRegionAtmosphere.cs',
               'Assets/Darklight/Runtime/Campaign/DarklightRegionExploration.cs',
               'Assets/Darklight/Runtime/Campaign/DarklightCampaignSettings.cs'} if args.allow_state_timing_support else set()
    intentional = {name: {'before': hashes[name], 'after': sha(ROOT / name)}
                   for name in changed if name in allowed and (ROOT / name).is_file()}
    unexpected = [name for name in changed if name not in intentional]
    result = {'checked': len(hashes), 'unchanged': len(hashes) - len(changed),
              'changedOrMissing': unexpected, 'documentedStateTimingChange': intentional,
              'passed': not unexpected,
              'newArtworkVisualFidelityCertifiedByHashes': False}
    (OUT / 'preservation-after.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))
    if unexpected:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
