"""Reference-color metadata only; never rewrites source images or cuts anatomy."""
from pathlib import Path
import json
import numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
MAX_SHIFT=.16  # Requiem-only reference is paler than the previous pooled Rock/Requiem sample.

def median(path,requiem_reference=False):
    a=np.array(Image.open(path).convert('RGBA'));c=a[:,:,:3].astype(float)
    if requiem_reference:
        # Measure the original upper Requiem figure in memory only. No cropped
        # artwork is created or supplied to generation, and source pixels stay intact.
        a=a[:a.shape[0]//2];c=c[:c.shape[0]//2]
    r,g,b=c[:,:,0],c[:,:,1],c[:,:,2]
    mask=(a[:,:,3]>=166)&(r>200)&(g>75)&(g<205)&(b>35)&(b<165)&(r>g*1.25)&(g>b*1.2)
    if int(mask.sum())<100:raise ValueError('Too few warm color candidates: '+str(path))
    return np.median(c[mask],axis=0)

def main():
    folder=ROOT/'outputs/implementacao-fase2-20261017'
    config=json.loads((folder/'PLANO-IDLE-FINAL.json').read_text(encoding='utf-8'))
    reference=Path(config['canonicalSkinReference'])
    if not reference.is_file():reference=folder/'design-original-referencia.png'
    target=median(reference,requiem_reference=True)
    manifest=json.loads((ROOT/'Assets/Darklight/ArtSources/LivingIdle20261017/manifest.json').read_text())
    detailed=ROOT/'Assets/Darklight/ArtSources/DetailedCombat20261017/manifest.json'
    if detailed.is_file():
        extra=json.loads(detailed.read_text(encoding='utf-8'))
        for section in ['pages','forms']:manifest[section].extend(extra[section])
    consistent=ROOT/'Assets/Darklight/ArtSources/ConsistentWalk20261017/manifest.json'
    if consistent.is_file():
        extra=json.loads(consistent.read_text(encoding='utf-8'))
        for section in ['pages','forms']:manifest[section].extend(extra[section])
    entries=[]
    for section,directory in [('pages','TodayCharacterAnimations'),('forms','TransformedYoung')]:
        for page in manifest[section]:
            path=ROOT/'Assets/Darklight/Resources/Darklight'/directory/(page['name']+'.png')
            actual=median(path);shift=(target-actual)/255
            if max(abs(shift))>MAX_SHIFT:raise ValueError('Needs whole-source regeneration: '+str(path))
            entries.append(dict(texture=page['name'],shift=shift.tolist(),sourceMedian=actual.tolist()))
    abyss=ROOT/'Assets/Darklight/ArtSources/ConsistentAbyss20261017/manifest.json'
    if abyss.is_file():
        for page in json.loads(abyss.read_text())['pages']:
            actual=median(ROOT/'Assets/Darklight/Resources/Darklight/AbyssPrototype'/(page['name']+'.png'));shift=(target-actual)/255
            if max(abs(shift))>MAX_SHIFT:raise ValueError('Needs whole-source regeneration: '+page['name'])
            entries.append(dict(texture=page['name'],shift=shift.tolist(),sourceMedian=actual.tolist()))
    data=dict(reference=config['canonicalSkinReference'],referenceMedian=target.tolist(),scope='new whole idle/motion/individual-strike sprites only; death and legacy excluded',entries=entries)
    directional=ROOT/'Assets/Darklight/ArtSources/AbyssDirectional20261018/manifest.json'
    if directional.exists():
        names=[p['name'] for p in json.loads(directional.read_text())['pages']]+['ActionF0'+v for v in ['W','NW','N']]
        for name in names:
            actual=median(ROOT/'Assets/Darklight/Resources/Darklight/AbyssPrototype'/(name+'.png'));shift=(target-actual)/255
            if max(abs(shift))>MAX_SHIFT:raise ValueError('Needs whole-source regeneration: '+name)
            entries.append(dict(texture=name,shift=shift.tolist(),sourceMedian=actual.tolist()))
        data['scope']='whole idle/motion/walk/strike/saw/directional sources; protected death unchanged'
    for path in [ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json',folder/'paleta-pele-original.json']:
        path.write_text(json.dumps(data,indent=2),encoding='utf-8')
    print(json.dumps(dict(textures=len(entries),referenceMedian=target.tolist())))
if __name__=='__main__':main()
