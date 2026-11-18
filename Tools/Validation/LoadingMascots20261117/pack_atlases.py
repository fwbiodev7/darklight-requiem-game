"""Technical whole-frame packing only; preserves generated RGBA pixels, no anatomy edits."""
from pathlib import Path
from PIL import Image
import hashlib
import json

ROOT = Path(__file__).resolve().parents[3]
SOURCES = ROOT / 'ArtSources/LoadingMascots20261117/selected'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/Loading/Mascots'


def main():
    rows = []
    for name in ('gato-ruptura', 'mini-fabius'):
        source = SOURCES / (name + '-source.png')
        image = Image.open(source).convert('RGBA')
        width, height = image.size
        side = max((width + 3) // 4, (height + 1) // 2)
        atlas = Image.new('RGBA', (side * 4, side * 2))
        bounds = []
        for frame in range(8):
            col, row = frame % 4, frame // 4
            box = (round(col * width / 4), round(row * height / 2),
                   round((col + 1) * width / 4), round((row + 1) * height / 2))
            cell = image.crop(box)
            assert cell.getchannel('A').getbbox(), (name, frame)
            atlas.paste(cell, (col * side, row * side))
            bounds.append(box)
        output = DEST / (name + '.png')
        atlas.save(output)
        rows.append(dict(asset=str(output.relative_to(ROOT)), source=str(source.relative_to(ROOT)),
                         sourceSHA256=hashlib.sha256(source.read_bytes()).hexdigest(),
                         assetSHA256=hashlib.sha256(output.read_bytes()).hexdigest(),
                         sourceSize=[width, height], atlasSize=list(atlas.size), frames=bounds,
                         operation='Whole equal-grid frame cells padded to square; RGBA pixels unchanged'))
    Path(__file__).with_name('packing.json').write_text(json.dumps(rows, indent=2) + '\n')
    print(json.dumps({'atlases': len(rows), 'frames': 16, 'size': [side * 4, side * 2]}))


if __name__ == '__main__':
    main()
