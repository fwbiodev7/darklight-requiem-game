"""Technical whole-frame registration only; never transplant anatomical elements."""
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
ART=ROOT/'ArtSources/WeaponRedesign20261025Retomada1350/presas-normal'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    old=ART/'ReqDaggersNormalIdleE_SE1350V1-source.png'
    repair=ART/'ReqDaggersNormalIdleE_SE1350V2-repair-generated.png'
    target=ART/'ReqDaggersNormalIdleE_SE1350V2-source.png'
    original=Image.open(old).convert('RGBA')
    generated=Image.open(repair).convert('RGBA')
    if original.size!=(1536,1024) or generated.size!=original.size:
        raise ValueError('Whole-figure repair requires original native canvas; do not resize anatomy.')
    rect=(1180,512,1536,1024)
    consolidated=original.copy()
    consolidated.paste(generated.crop(rect),rect[:2])
    a,b=np.array(original),np.array(consolidated)
    protected=np.ones(a.shape[:2],bool)
    protected[rect[1]:rect[3],rect[0]:rect[2]]=False
    if not np.array_equal(a[protected],b[protected]):
        raise ValueError('Changed pixels outside the complete defective frame.')
    consolidated.save(target)
    back=ART/'ReqDaggersNormalIdleNW_N_NE1350V1-source.png'
    back_target=ART/'ReqDaggersNormalIdleNW_N1350V1-source.png'
    source=Image.open(back).convert('RGBA')
    if source.size!=(1536,1024):
        raise ValueError('Unexpected source; inspect whole figures before registration.')
    # Retain the eight complete good NW/N figures; remove only entire rejected NE row.
    source.crop((0,0,1536,674)).save(back_target)
    if not np.array_equal(np.array(Image.open(back_target)),np.array(source)[:674]):
        raise ValueError('Changed retained NW/N source pixels.')
    proof=dict(schemaVersion=1,anatomicalAssembly=False,resizing=False,
        daggerTipRepair=dict(originalSource=old.name,originalSha256=sha(old),
            integratedImageGenRepair=repair.name,repairSha256=sha(repair),
            wholeFrameRect=[1180,512,356,512],result=target.name,resultSha256=sha(target),
            allPixelsOutsideWholeFrameUnchanged=True,sevenGoodFiguresPreserved=True),
        backViews=dict(originalSource=back.name,originalSha256=sha(back),
            result=back_target.name,resultSha256=sha(back_target),wholeFigureCrop=[0,0,1536,674],
            eightGoodFiguresPreserved=True,excluded='Entire NE row, four figures: chain on wrong hip. Replaced by new full-figure NEV3 source.'))
    (ART/'whole-frame-repair-proof.json').write_text(json.dumps(proof,indent=2),encoding='utf-8')
    print(json.dumps(dict(sevenGoodFiguresExact=True,eightGoodBackViewsExact=True,anatomicalAssembly=False)))


if __name__=='__main__':
    main()
