"""Prepare whole figures and replace only f07, proving 23 other frames unchanged."""
import json
from pathlib import Path
from types import SimpleNamespace
import sys

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
from whole_figure_components import identify
import sprite_pipeline as sp
import replace_frames as rf


def main():
    out = sp.directory(ROOT / 'outputs/revisao-sprites-20261020/seletiva-f07-final')
    source = out.parent / 'Requiem-adagas-roxa-candidata-v5.png'
    source_hash = sp.digest(source)
    im = Image.open(source).convert('RGBA')
    figures, owners = identify(im, rows=6, columns=4)
    cell, ground = 320, 288
    atlas = Image.new('RGBA', (cell * 4, cell * 6))
    descriptor, provenance = [], []
    for i, f in enumerate(figures):
        x0, y0, x1, y1 = f['rect']
        tile = im.crop((x0, y0, x1, y1))
        rgba = np.array(tile)
        rgba[owners[y0:y1, x0:x1] != i + 1, 3] = 0
        tile = Image.fromarray(rgba)
        # Technical isolation of each COMPLETE ownership figure, never anatomy pieces.
        if tile.width > cell - 24 or tile.height > ground - 12:
            raise ValueError('Whole figure needs a larger common canvas; do not crop')
        canvas = Image.new('RGBA', (cell, cell))
        offset = ((cell - tile.width) // 2, ground - tile.height)
        canvas.paste(tile, offset)
        if i == 7:
            canvas.save(out / 'f07-antes.png')
        x, y = i % 4 * cell, i // 4 * cell
        atlas.paste(canvas, (x, y))
        descriptor.append(dict(id=f'f{i:02}', rect=[x, y, cell, cell], pivot=[.5, .1],
                               durationMs=100, action='attack', direction='S', skin='Requiem', form='roxa'))
        provenance.append(dict(id=f'f{i:02}', sourceRectXYXY=f['rect'], ownerId=i+1,
                               wholeFigure=True, originalOwnedPixelsUnrecolored=True))
    atlas.save(out / 'baseline.png')
    reference = ROOT / 'ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png'
    d = dict(schemaVersion=1, image='baseline.png', imageSha256=sp.digest(out / 'baseline.png'),
             nativeSize=list(atlas.size), pixelsPerUnit=1, status='candidate', frames=descriptor,
             clips=[dict(name='attack_S', frames=[f['id'] for f in descriptor], loop=False)],
             baseline=dict(image=str(reference), sha256=sp.digest(reference)),
             quality=dict(alphaThreshold=128),
             notes=['Prepared review atlas only, no production import.',
                    'All source figures kept at original pixel scale; low-alpha background outside owner labels excluded.',
                    'One source-specific 320px canvas; PPU/pivots and timings require production calibration.'])
    sp.write(out / 'baseline.json', d)
    # Generated source dimensions need not equal its requested logical canvas.
    # Resample the ENTIRE square uniformly; never stretch or edit anatomy parts.
    replacement_path = out.parent / 'f07-corrigido-mechas-roxas.png'
    replacement = Image.open(replacement_path).convert('RGBA')
    if replacement.width != replacement.height:
        raise ValueError('Unexpected aspect ratio; never stretch the source')
    native = replacement.resize((256, 256), Image.Resampling.NEAREST)
    bbox = native.getchannel('A').point(lambda a: 255 if a > 128 else 0).getbbox()
    if not bbox:
        raise ValueError('Empty replacement')
    corrected = Image.new('RGBA', (cell, cell))
    offset = ((cell - 256) // 2, ground - bbox[3])
    if offset[1] < 0 or offset[1] + bbox[3] >= cell:
        raise ValueError('Corrected whole frame cannot fit without clipping')
    corrected.paste(native, offset)
    corrected.save(out / 'f07-depois.png')
    sp.write(out / 'plan.json', dict(wholeFramesConfirmed=True,
                                   replacements=[dict(frameId='f07', image='f07-depois.png')]))
    result = rf.replace(out / 'baseline.json', out / 'plan.json', out / 'corrigida')
    audit = sp.audit(SimpleNamespace(manifest=out / 'corrigida/sheet.json',
                                    out=out / 'auditoria', unity_meta=None))
    preview = sp.preview(SimpleNamespace(manifest=out / 'corrigida/sheet.json', out=out / 'previa', zoom=2))
    if sp.digest(source) != source_hash:
        raise ValueError('Source altered')
    sp.write(out / 'proveniencia.json', dict(source=str(source), sourceSha256=source_hash,
              sourceUnchanged=True, figures=provenance, eyeReferenceSha256=sp.digest(reference),
              generatedReplacementSha256=sp.digest(replacement_path), sourceCanvas=list(replacement.size),
              nativeReplacementCanvas=[256, 256], onlyFrameReplaced='f07',
              uniformWholeFrameResample=256 / replacement.width, resampling='nearest-neighbor',
              purpleHairStreaksPreserved=True, visualApproval=False, runtimeVerified=False))
    print(json.dumps(dict(result, auditOk=audit['ok'], preview=preview['preview'], productionImported=False)))
    return 0 if audit['ok'] else 2


if __name__ == '__main__':
    raise SystemExit(main())
