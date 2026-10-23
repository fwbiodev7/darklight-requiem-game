"""Measure reference colors and emit renderer metadata; never edit PNG pixels."""
from pathlib import Path
import json,hashlib,numpy as np
from PIL import Image
from prepare_complexion import median
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/fidelidade-original-20261018';PACK=OUT/'pacote-v6'
target=median(OUT/'referencia-original.png',requiem_reference=True)
selection=json.loads((PACK/'fontes.json').read_text(encoding='utf-8'))
entries=[]
for asset in selection['assets']:
    file='pacote-v6/'+asset['file']
    path=OUT/file;actual=median(path);shift=target-actual
    if np.max(np.abs(shift))>40.8:raise ValueError('Excessive shift requires source review: '+str(path))
    rgba=np.array(Image.open(path).convert('RGBA'));rgb=rgba[:,:,:3].astype(float)
    r,g,b=rgb[:,:,0],rgb[:,:,1],rgb[:,:,2]
    skin=(rgba[:,:,3]>=166)&(r>200)&(g>75)&(g<205)&(b>35)&(b<165)&(r>g*1.25)&(g>b*1.2)
    corrected=np.clip(rgb[skin]+shift,0,255)
    after=np.median(corrected,axis=0)
    assert np.max(np.abs(after-target))<=1
    entries.append(dict(file=file,sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
        sourceMedian=actual.tolist(),shiftRgb=shift.tolist(),correctedMedian=after.tolist(),
        warmSkinPixels=int(skin.sum())))
data=dict(revision=1,date='2026-10-19',referenceFile='referencia-original.png',
    referenceMedian=target.tolist(),referenceSkin='upper original Requiem only',
    scope='Both skins, all four forms, every selected source. Renderer metadata only; PNG pixels and anatomy unchanged.',
    predicate='alpha>=166,r>200,75<g<205,35<b<165,r>g*1.25,g>b*1.2',
    maximumChannelShiftBytes=40.8,entries=entries)
(PACK/'paleta-pele.json').write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(sources=len(entries),target=target.tolist(),maximumAppliedShiftBytes=max(max(abs(v) for v in e['shiftRgb']) for e in entries),allCorrectedMediansMatch=True,sourcePixelEdits=0)))
