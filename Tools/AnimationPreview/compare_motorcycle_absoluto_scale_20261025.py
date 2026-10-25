"""Technical preview: intact figures rendered at a shared world PPU, never fit-to-cell."""
import json
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
RES = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT = ROOT / 'outputs/redesign-armas-20261025/moto-absoluto'
TARGET_PPU = 100


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    canvas = Image.new('RGBA', (900, 800), (36, 39, 46, 255))
    draw = ImageDraw.Draw(canvas)
    evidence = []
    for column, (form, title) in enumerate(((0, 'NORMAL'), (3, 'ABSOLUTO'))):
        manifest = json.loads((RES / f'RockMotoForm{form}Ride8SlamS20261025V2.json').read_text(encoding='utf-8'))
        atlas = Image.open(RES / manifest['image']).convert('RGBA')
        draw.text((column * 450 + 15, 12), f'{title} | 100 pixels/world unit', fill='white')
        for row, index in enumerate((0, 9)):
            frame = manifest['frames'][index]
            x, y, width, height = frame['rect']
            # Crop only an entire authored figure/cell. No anatomical pieces.
            figure = atlas.crop((x, y, x + width, y + height))
            scale = TARGET_PPU / manifest['pixelsPerUnit']
            shown = figure.resize((round(width * scale), round(height * scale)), Image.Resampling.NEAREST)
            anchor_x = frame['pivot'][0] * shown.width
            anchor_y = (1 - frame['pivot'][1]) * shown.height
            ground = 370 + row * 380
            destination = (round(column * 450 + 225 - anchor_x), round(ground - anchor_y))
            draw.line((column * 450 + 10, ground, column * 450 + 440, ground), fill=(89, 95, 104, 255))
            canvas.alpha_composite(shown, destination)
            draw.text((column * 450 + 15, 36 + row * 380), 'S mounted' if index == 0 else 'S overhead lift', fill='white')
            evidence.append(dict(form=form, sourceFrame=index, assetPixelsPerUnit=manifest['pixelsPerUnit'],
                renderPixelsPerUnit=TARGET_PPU, fitToCell=False, nearestNeighborScale=scale,
                worldAnatomicalHeight=1.98, manualMeasurementTolerancePixels=6,
                note='Whole-figure technical preview only. Assets/source bytes unchanged.'))
    canvas.save(OUT / 'scale-compare-normal-absoluto.png')
    (OUT / 'scale-compare-normal-absoluto.json').write_text(json.dumps(evidence, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(preview=str((OUT / 'scale-compare-normal-absoluto.png').relative_to(ROOT)), figures=4, targetPixelsPerWorldUnit=TARGET_PPU)))


if __name__ == '__main__':
    main()
