"""Encode actual Unity capture frames at their recorded timestamps; no game delay added."""
from pathlib import Path
from PIL import Image
import json

ROOT = Path(__file__).resolve().parents[3]
OUTPUT = ROOT / 'outputs/loading-mascots-20261117'


def main():
    receipts = []
    for name in ('phase1-phase2-cat', 'phase1-phase2-mini'):
        folder = OUTPUT / name
        times = json.loads((folder / 'timestamps.json').read_text())
        frames = [Image.open(folder / f'frame-{i:04}.png').convert('RGB') for i in range(len(times))]
        # Shared palette avoids palette flicker; spatially nearest sampling preserves pixels.
        samples = Image.new('RGB', (320, 180 * len(frames)))
        for i, frame in enumerate(frames):
            samples.paste(frame.resize((320, 180), Image.Resampling.NEAREST), (0, i * 180))
        palette = samples.quantize(colors=256, method=Image.Quantize.MEDIANCUT, dither=Image.Dither.NONE)
        encoded = [frame.quantize(palette=palette, dither=Image.Dither.NONE) for frame in frames]
        durations = [max(20, round((times[i + 1] - times[i]) * 100) * 10) for i in range(len(times) - 1)] + [140]
        target = OUTPUT / (name + '.gif')
        encoded[0].save(target, save_all=True, append_images=encoded[1:], duration=durations, loop=0, optimize=False)
        receipts.append(dict(path=str(target.relative_to(ROOT)), frames=len(frames),
                             durationMs=sum(durations), size=list(frames[0].size), bytes=target.stat().st_size,
                             evidence='Real Unity PlayMode captures; original timestamps, GIF centisecond rounding'))
    (OUTPUT / 'gif-receipt.json').write_text(json.dumps(receipts, indent=2) + '\n')
    print(json.dumps(receipts))


if __name__ == '__main__':
    main()
