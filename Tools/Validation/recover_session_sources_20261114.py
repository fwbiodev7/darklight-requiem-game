"""Validate and preserve original ZIP bytes without importing Unity assets."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import unicodedata
import zipfile
from contextlib import ExitStack

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
SESSION = ROOT / 'ArtSources/Session20261114'


def normalized(name):
    return unicodedata.normalize('NFC', name).replace(':', '_').casefold()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive', type=Path)
    parser.add_argument('--extra', type=Path, action='append', default=[])
    args = parser.parse_args()
    inventory = json.loads((SESSION / 'source-inventory.json').read_text(encoding='utf-8'))['files']
    preservation = json.loads((SESSION / 'preservation-result.json').read_text(encoding='utf-8'))['files']
    allowed = {f['library_file_id'] for f in preservation if f['folder'].endswith('/01-produzidas-e-aprovadas')}
    records, found = [], set()
    raw = SESSION / 'raw'
    raw.mkdir(exist_ok=True)
    with ExitStack() as stack:
        archive = stack.enter_context(zipfile.ZipFile(args.archive))
        entries = [(entry, None) for entry in archive.infolist()]
        entries.extend((zipfile.ZipInfo(path.name), path) for path in args.extra)
        for entry, extra in entries:
            if entry.is_dir():
                continue
            if extra:
                entry.file_size = extra.stat().st_size
            names = {normalized(Path(entry.filename).name)}
            try:
                names.add(normalized(Path(entry.filename).name.encode('cp437').decode('cp850')))
            except UnicodeError:
                pass
            matches = [f for f in inventory if f['sizeBytes'] == entry.file_size and normalized(f['filename']) in names]
            if len(matches) != 1:
                raise ValueError(f'Unrecognized or ambiguous entry: {entry.filename!r}')
            source = matches[0]
            identifier = source['library_file_id']
            if identifier not in allowed or identifier in found:
                raise ValueError(f'Superseded or duplicate entry: {identifier}')
            data = extra.read_bytes() if extra else archive.read(entry)  # Verifies ZIP CRC.
            if not data.startswith(b'\x89PNG\r\n\x1a\n'):
                raise ValueError(f'Not a PNG: {identifier}')
            with Image.open(io.BytesIO(data)) as image:
                if image.format != 'PNG':
                    raise ValueError(identifier)
                image.verify()
            with Image.open(io.BytesIO(data)) as image:
                image.load()
                dimensions, mode = image.size, image.mode
                alpha_range = image.convert('RGBA').getchannel('A').getextrema()
            destination = raw / (identifier + '.png')
            if destination.exists():
                if destination.read_bytes() != data:
                    raise ValueError(f'Existing source differs: {destination}')
            else:
                with destination.open('xb') as output:
                    output.write(data)
            found.add(identifier)
            records.append({**source, 'archiveEntry': entry.filename,
                            'localPath': destination.relative_to(ROOT).as_posix(),
                            'sha256': hashlib.sha256(data).hexdigest(),
                            'dimensions': dimensions, 'mode': mode, 'alphaRange': alpha_range,
                            'status': 'bytes-validated-visual-review-pending'})
    missing = [f for f in inventory if f['library_file_id'] in allowed - found]
    with args.archive.open('rb') as archive_file:
        archive_sha = hashlib.file_digest(archive_file, 'sha256').hexdigest()
    report = {'archive': args.archive.name, 'extraFiles': [p.name for p in args.extra],
              'archiveSha256': archive_sha,
              'expected': len(allowed), 'recovered': len(records), 'supersededExtracted': 0,
              'validation': 'ZIP CRC, PNG signature, Pillow verify and full decode, inventory name and byte size',
              'hashLimitation': 'Inventory has no original SHA-256; these hashes establish the received baseline.',
              'files': records, 'missing': missing, 'unityImported': False}
    (SESSION / 'zip-recovery-validation.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'expected': len(allowed), 'validPngs': len(records), 'superseded': 0,
                      'missing': [f['filename'] for f in missing]}, ensure_ascii=True))
    return 2 if missing else 0


if __name__ == '__main__':
    raise SystemExit(main())
