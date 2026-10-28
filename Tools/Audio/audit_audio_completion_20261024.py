"""Verify preserved delivery and create a neutral timeline for later listening.

No playback, extraction, source edits or semantic segment selection. Timeline
windows cover the four long sources uniformly and never enter Unity Assets.
"""
from pathlib import Path
import argparse
import hashlib
import json
import struct
import wave

ROOT = Path(__file__).resolve().parents[2]
DEFERRED = {'WM02', 'WM06', 'PW02', 'PW09'}


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def audit(output, allow_external_originals=False):
    delivery = json.loads((ROOT / 'ArtSources/AudioDelivery20261023/manifest.json').read_text(encoding='utf-8'))
    audio = ROOT / 'Assets/Darklight/Resources/Darklight/Audio'
    report = dict(date='2026-10-24', playbackPerformed=False, semanticSelectionPerformed=False,
                  allowExternalOriginals=allow_external_originals, externalOriginalsNotVerified=[],
                  originals=[], prepared=[], longSourceListeningTimeline=[], issues=[])
    total_memory = 0
    for source in delivery['sources']:
        ident = source['id']
        original = ROOT / source['original']
        if not original.is_file() and allow_external_originals:
            report['originals'].append(dict(id=ident, originalPreserved=None, status='external_not_verified'))
            report['externalOriginalsNotVerified'].append(ident)
            preserved = None
        else:
            preserved = original.is_file() and sha(original) == source['sha256']
            report['originals'].append(dict(id=ident, originalPreserved=preserved))
        if preserved is False:
            report['issues'].append('Original hash mismatch or missing: ' + ident)
        if ident in DEFERRED:
            seconds = source['seconds']
            windows = []
            start = 0.0
            while start < seconds:
                end = min(seconds, start + 2.0)
                windows.append(dict(startSeconds=round(start, 6), endSeconds=round(end, 6)))
                start = end
            integrated = any(audio.rglob('dl-user-' + ident.lower() + '*'))
            report['longSourceListeningTimeline'].append(dict(
                id=ident, source=source['original'], seconds=seconds, sha256=source['sha256'],
                method='Uniform contiguous 2-second windows; no content or amplitude selection',
                heard=False, runtimeIntegrated=integrated, windows=windows))
            if integrated:
                report['issues'].append('Unreviewed long delivery unexpectedly present in runtime: ' + ident)
            continue
        asset = ROOT / source['asset']
        intact = asset.is_file() and sha(asset) == source['assetSha256']
        if not intact:
            report['issues'].append('Prepared asset hash mismatch or missing: ' + ident)
            continue
        with wave.open(str(asset), 'rb') as wav:
            channels, rate, width, frames = wav.getnchannels(), wav.getframerate(), wav.getsampwidth(), wav.getnframes()
            peak = max(abs(value[0]) for value in struct.iter_unpack('<h', wav.readframes(frames))) / 32768
        total_memory += frames * channels * 4
        bounded = channels == 1 and width == 2 and rate in (24000, 32000) and .2 <= frames / rate <= 6.6 and .01 <= peak <= .901
        report['prepared'].append(dict(id=ident, preserved=True, seconds=round(frames / rate, 6),
                                       channels=channels, sampleRate=rate, peak=round(peak, 6), bounded=bounded))
        if not bounded:
            report['issues'].append('Prepared cue outside verified technical bounds: ' + ident)
    report.update(originalCount=len(report['originals']), preparedCount=len(report['prepared']),
                  deferredCount=len(report['longSourceListeningTimeline']), decodedPcmBytes=total_memory)
    for actual, expected, label in ((report['originalCount'], 23, 'originals'),
                                    (report['preparedCount'], 19, 'prepared cues'),
                                    (report['deferredCount'], 4, 'deferred long sources')):
        if actual != expected:
            report['issues'].append(f'Expected {expected} {label}, found {actual}')
    if total_memory >= 5_000_000:
        report['issues'].append('Delivery exceeds its 5 MB decoded PCM budget')
    music = list((audio.parent / 'Music').glob('*.mp3'))
    report['musicFileCount'] = len(music)
    if len(music) != 5:
        report['issues'].append('Expected only the five existing supplied music files')
    commercial = [p.name for p in (ROOT / 'Assets').rglob('*.mp3')
                  if any(name in p.name.lower() for name in ('going under', 'going_under', 'afterlife', 'see u in hell', 'see_u_in_hell'))]
    report['commercialBossMusicInAssets'] = commercial
    if commercial:
        report['issues'].append('Deferred commercial boss music found in Assets')
    report['success'] = not report['issues']
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({key: report[key] for key in ('success', 'originalCount', 'preparedCount', 'deferredCount',
        'decodedPcmBytes', 'musicFileCount', 'playbackPerformed', 'semanticSelectionPerformed', 'issues')}, ensure_ascii=False))
    return 0 if report['success'] else 1


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, default=ROOT / 'work/session-20261024/audio-completion-audit.json')
    parser.add_argument('--allow-external-originals', action='store_true',
                        help='Public source copy: report absent original MP3 as external/unverified, never as preserved')
    args = parser.parse_args()
    raise SystemExit(audit(args.output, args.allow_external_originals))
