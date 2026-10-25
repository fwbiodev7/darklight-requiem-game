"""Measure existing PCM clips and static references; never infer listening from file data."""
import hashlib
import json
import math
import re
import wave
from pathlib import Path
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/redesign-armas-20261025-retomada1850/audio'


def db(value):
    return round(20 * math.log10(value), 2) if value > 0 else None


def main():
    delivery = json.loads((ROOT / 'ArtSources/AudioDelivery20261023/manifest.json').read_text(encoding='utf-8'))
    source_ids = {row.get('asset'): row['id'] for row in delivery['sources'] if row.get('asset')}
    literals = {}
    for path in (ROOT / 'Assets/Darklight/Runtime').rglob('*.cs'):
        for literal in set(re.findall(r'"([^"\r\n]+)"', path.read_text(encoding='utf-8-sig'))):
            if len(literal) >= 4 and ('-' in literal or literal in source_ids.values()):
                literals.setdefault(literal, []).append(path.relative_to(ROOT).as_posix())
    clips, failures = [], []
    for path in sorted((ROOT / 'Assets/Darklight/Resources/Darklight/Audio').rglob('*.wav')):
        try:
            with wave.open(str(path), 'rb') as wav:
                channels, rate, width, frames = wav.getnchannels(), wav.getframerate(), wav.getsampwidth(), wav.getnframes()
                raw = wav.readframes(frames)
            if width == 1:
                values = (np.frombuffer(raw, dtype=np.uint8).astype(np.float64) - 128) / 128
            elif width == 2:
                values = np.frombuffer(raw, dtype='<i2').astype(np.float64) / 32768
            elif width == 3:
                packed = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3).astype(np.int32)
                integers = packed[:, 0] | packed[:, 1] << 8 | packed[:, 2] << 16
                values = ((integers ^ 0x800000) - 0x800000).astype(np.float64) / 8388608
            elif width == 4:
                values = np.frombuffer(raw, dtype='<i4').astype(np.float64) / 2147483648
            else:
                raise ValueError('Unsupported PCM width: ' + str(width))
            peak = float(np.max(np.abs(values), initial=0))
            rms = float(np.sqrt(np.mean(values * values))) if values.size else 0
            amplitude = np.max(np.abs(values.reshape(-1, channels)), axis=1)
            active = np.flatnonzero(amplitude > .001)
            relative = path.relative_to(ROOT).as_posix()
            keys = [key for key in literals if path.stem.startswith(key) or key == source_ids.get(relative)]
            clips.append(dict(path=relative, id=source_ids.get(relative), sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                seconds=round(frames / rate, 5), channels=channels, sampleRate=rate, bits=width * 8,
                peakDbfs=db(peak), rmsDbfs=db(rms), clippedSampleFraction=float(np.mean(np.abs(values) >= .9999)),
                leadingSilenceMs=round(int(active[0]) / rate * 1000, 2) if active.size else None,
                trailingSilenceMs=round((frames - 1 - int(active[-1])) / rate * 1000, 2) if active.size else None,
                staticReferenceSites=sorted({site for key in keys for site in literals[key]}),
                playbackVerified=False, listeningVerified=False))
        except (wave.Error, ValueError) as error:
            failures.append(dict(path=path.relative_to(ROOT).as_posix(), error=str(error)))
    duplicate_groups = {}
    for clip in clips:
        duplicate_groups.setdefault(clip['sha256'], []).append(clip['path'])
    warnings = [dict(path=c['path'], reason='Digital clipping samples; inspect content and mix before changing it')
        for c in clips if c['clippedSampleFraction'] > .001]
    report = dict(date='2026-10-25', scope='Existing PCM and static routing evidence only', clips=clips,
        failures=failures, warnings=warnings, identicalFileGroups=[paths for paths in duplicate_groups.values() if len(paths) > 1],
        deliveredPrepared=len([c for c in clips if c['id']]), deferredIds=['WM02', 'WM06', 'PW02', 'PW09'],
        staticReferencesDoNotProvePlayback=True, subjectiveListeningPerformed=False, originalsModified=False,
        physicalOutputOrSynchronizationCertified=False)
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / 'pcm-and-routing-audit.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
    print(json.dumps(dict(clips=len(clips), deliveredPrepared=report['deliveredPrepared'],
        identicalFileGroups=len(report['identicalFileGroups']), warnings=len(warnings), failures=len(failures), listeningVerified=False)))


if __name__ == '__main__':
    main()
