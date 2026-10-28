"""Slice complete equipment icon sets; the generated source PNG remains byte-identical."""
from pathlib import Path
import json
import numpy as np
from PIL import Image
from prepare_weapon_redesign import ROOT, ART, RES, OUT, preserved_copy, sha, write, qa


def main():
    generated = Path('C:/Users/Usuario/.codex/generated_images/01a16a65-852e-7ed1-a865-2fb76d9bf2e7/exec-6c6bfc70-3420-4919-9b3f-5337d0ca08f1.png')
    source = ART / 'EquipmentIcons-source.png'
    destination = RES / 'EquipmentIcons.png'
    preserved_copy(generated, source)
    preserved_copy(source, destination)
    image = Image.open(source).convert('RGBA')
    if image.size != (1448, 1086):
        raise ValueError('Inspect actual complete set positions before changing source layout')
    alpha = np.array(image.getchannel('A'))
    xs, ys = [0, 362, 724, 1086, 1448], [0, 358, 643, 1086]
    # Full icon-set cells were reviewed. Do not split the pair of blades or
    # the four-piece gauntlet/greave set into separately cropped elements.
    for x in xs[1:-1]:
        if np.max(alpha[:, x-1:x+1]) >= 32:
            raise ValueError('An icon set crosses a proposed column boundary')
    for y in ys[1:-1]:
        if np.max(alpha[y-1:y+1]) >= 32:
            raise ValueError('An icon set crosses a proposed row boundary')
    frames, clips = [], []
    for weapon in range(3):
        for form in range(4):
            name = f'Redesign_Icon_Weapon{weapon}_Form{form}'
            rect = [xs[form], ys[weapon], xs[form+1]-xs[form], ys[weapon+1]-ys[weapon]]
            frames.append(dict(id=name, rect=rect, pivot=[.5, .5], durationMs=100,
                               action='icon', direction='inventory',
                               skin='equipment', form=['Normal','Ruptura','Eclipse','Absoluto'][form],
                               completeSet=True, weapon=weapon, formIndex=form))
            clips.append(dict(name=name, frames=[name], loop=False, allowStatic=True))
    data = dict(schemaVersion=1, image=destination.name, imageSha256=sha(destination),
                sourcePath=source.relative_to(ROOT).as_posix(), sourceSha256=sha(source),
                sourceSize=list(image.size), nativeSize=list(image.size), pixelsPerUnit=100,
                status='visual-reviewed', wholeFigures=True, sourcePngByteIdentical=True,
                frames=frames, clips=clips, requiredClips=[clip['name'] for clip in clips],
                quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=3),
                notes=['Twelve complete icon sets. No cutting a gauntlet, greave or blade from its pair/set.',
                       'Figure pages do not receive Icon actions. Bank.Icon selects exact weapon/form with fallback.',
                       'Row boundaries are inspected content gaps, not a guessed equal 3-row grid.',
                       'Continuous RGBA palette; no global quantization or background removal.'])
    write(RES / 'EquipmentIcons.json',data)
    result=qa('EquipmentIcons')
    write(OUT / 'equipment-icons-qa.json',dict(**result,frames=12,pngByteIdentical=True,unity=False))
    print(json.dumps(result))


if __name__ == '__main__':
    main()
