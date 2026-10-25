"""Audit supplied SFX and prepare short, bounded Unity cues without playback.

Original MP3 files are preserved verbatim. Long ambiguous deliveries remain
outside Assets until an audible review identifies their intended event/segment.
"""
from pathlib import Path
import argparse
import hashlib
import json
import subprocess
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[2]
SOURCES = {
    'PW11': 'ability ready cooldown error cue.mp3',
    'PW10': 'headphones equip power activation.mp3',
    'PW09': 'radiant energy transformation.mp3',
    'PW08': 'void shadow transformation.mp3',
    'PW07': 'violent dark transformation burst.mp3',
    'PW06': 'dark magic sword judgment impact.mp3',
    'PW05': 'blood rage power activation.mp3',
    'PW04': 'dark magic pull vortex.mp3',
    'PW03': 'vampire blood drain dark magic.mp3',
    'PW02': 'shadow teleport dash.mp3',
    'PC06': 'leather armor cloth movement.mp3',
    'PC05': 'potion bottle drink heal.mp3',
    'PC04': 'exhausted breathing heartbeat.mp3',
    'PC03': 'body fall death effort foley.mp3',
    'PC02': 'male pain grunt exertion.mp3',
    'PC01': 'dodge cloth movement whoosh.mp3',
    'WM08': 'weapon equip swap foley.mp3',
    'WM06': 'uppercut hammer fist ground slam.mp3',
    'WM05': 'martial arts kick spin whoosh.mp3',
    'WM04': 'armored gauntlet punch impact.mp3',
    'WM03': 'dual dagger draw slash.mp3',
    'WM02': 'katana slash heavy charged swing.mp3',
    'WM01': 'katana unsheathe sheathe.mp3',
}
DEFER = {'PW09', 'PW02', 'WM06', 'WM02'}


def run(args):
    p = subprocess.run([str(x) for x in args], capture_output=True, check=True)
    return p.stdout


def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()


def prepare(downloads, ffmpeg, ffprobe):
    archive = ROOT / 'ArtSources/AudioDelivery20261023'
    assets = ROOT / 'Assets/Darklight/Resources/Darklight/Audio/Delivery20261023'
    archive.mkdir(parents=True, exist_ok=True)
    assets.mkdir(parents=True, exist_ok=True)
    report = dict(schema=1, date='2026-10-23', playbackPerformed=False,
                  commercialMusicImported=False, sources=[], issues=[])
    hashes = {}
    for ident, name in SOURCES.items():
        source = downloads / name
        row = dict(id=ident, source=str(source), file=name, exists=source.is_file())
        report['sources'].append(row)
        if not source.is_file():
            report['issues'].append('Missing: ' + name)
            continue
        target = archive / name
        if target.exists() and sha(target) != sha(source):
            raise RuntimeError('Refusing to overwrite different archived original: ' + name)
        if not target.exists():
            target.write_bytes(source.read_bytes())
        row.update(sha256=sha(source), bytes=source.stat().st_size,
                   original=target.relative_to(ROOT).as_posix())
        probe = json.loads(run([ffprobe, '-v', 'error', '-select_streams', 'a:0',
                               '-show_entries', 'stream=codec_name,sample_rate,channels',
                               '-show_entries', 'format=duration', '-of', 'json', source]))
        stream = probe['streams'][0]
        rate = min(32000, int(stream['sample_rate']))
        pcm = np.frombuffer(run([ffmpeg, '-v', 'error', '-i', source, '-map', '0:a:0',
                                '-ac', '1', '-ar', rate, '-f', 'f32le', 'pipe:1']), dtype='<f4').copy()
        block = max(1, rate // 100)
        rms = np.array([np.sqrt(np.mean(pcm[i:i+block].astype(float)**2))
                        for i in range(0, len(pcm), block)])
        audible = np.flatnonzero(rms >= 10**(-50/20))
        row.update(codec=stream['codec_name'], sampleRate=int(stream['sample_rate']),
                   channels=int(stream['channels']), seconds=round(len(pcm)/rate, 6),
                   monoPeak=float(np.max(np.abs(pcm))),
                   monoRms=float(np.sqrt(np.mean(pcm.astype(float)**2))),
                   silentBlocksPercent=round(float(np.mean(rms < 10**(-50/20)))*100, 3),
                   clippingSamples=int(np.count_nonzero(np.abs(pcm) >= .9999)))
        if row['sha256'] in hashes:
            row['identicalTo'] = hashes[row['sha256']]
        else:
            hashes[row['sha256']] = ident
        if not len(audible):
            row.update(status='received_not_integrated', reason='No signal above -50 dB RMS')
            report['issues'].append('Silent: ' + name)
            continue
        row['leadingSilenceSeconds'] = round(int(audible[0])*block/rate, 4)
        row['trailingSilenceSeconds'] = round(max(0, len(pcm)-(int(audible[-1])+1)*block)/rate, 4)
        if ident in DEFER:
            row.update(status='received_not_integrated', reason='Long source: select and audibly validate a short event before import')
            continue
        start = max(0, int(audible[0])*block-int(rate*.02))
        end = min(len(pcm), (int(audible[-1])+1)*block+int(rate*.08))
        cue = pcm[start:end].copy()
        peak = float(np.max(np.abs(cue)))
        gain = min(1., .9/peak)
        cue *= gain
        fade_in, fade_out = min(int(rate*.005), len(cue)), min(int(rate*.04), len(cue))
        cue[:fade_in] *= np.linspace(0, 1, fade_in)
        cue[-fade_out:] *= np.linspace(1, 0, fade_out)
        output = assets / ('dl-user-' + ident.lower() + '.wav')
        with wave.open(str(output), 'wb') as wav:
            wav.setnchannels(1)
            wav.setsampwidth(2)
            wav.setframerate(rate)
            wav.writeframes(np.rint(np.clip(cue, -1, 1)*32767).astype('<i2').tobytes())
        row.update(status='prepared_for_event_integration', resource='dl-user-'+ident.lower(),
                   asset=output.relative_to(ROOT).as_posix(), assetSha256=sha(output),
                   cueSeconds=round(len(cue)/rate, 6), cueRate=rate,
                   sourceStartSeconds=round(start/rate, 6), sourceEndSeconds=round(end/rate, 6),
                   appliedGain=round(gain, 6), fadeInSeconds=.005, fadeOutSeconds=.04,
                   audibleValidation=False)
    report['sourceCount'] = len(report['sources'])
    report['preparedCount'] = sum(s.get('status') == 'prepared_for_event_integration' for s in report['sources'])
    report['deferredCount'] = sum(s.get('status') == 'received_not_integrated' for s in report['sources'])
    report['decodedPcmBudgetBytes'] = sum(int(s.get('cueSeconds', 0)*s.get('cueRate', 0))*4 for s in report['sources'])
    (archive/'manifest.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps({k: report[k] for k in ['sourceCount','preparedCount','deferredCount','decodedPcmBudgetBytes','playbackPerformed','issues']}))
    for s in report['sources']:
        print(s['id'], s.get('seconds'), s.get('cueSeconds'), s.get('monoPeak'), s.get('identicalTo', ''), s.get('status'))


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('--downloads', type=Path, default=Path.home()/'Downloads')
    p.add_argument('--ffmpeg', type=Path, required=True)
    p.add_argument('--ffprobe', type=Path, required=True)
    args = p.parse_args()
    prepare(args.downloads, args.ffmpeg, args.ffprobe)
