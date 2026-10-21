"""Prepare complete source figures and complexion comparisons; no anatomy assembly."""
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/padronizacao-artes-20261020'
META=ROOT/'outputs/olhos-todas-formas-20261020/metadata'


def complexion(a,shift):
    result=a.copy()
    c=a[:,:,:3].astype(float)
    r,g,b=c[:,:,0],c[:,:,1],c[:,:,2]
    selected=(a[:,:,3]>=166)&(r>200)&(g>75)&(g<205)&(b>35)&(b<165)&(r>g*1.25)&(g>b*1.2)
    result[selected,:3]=np.clip(np.rint(c[selected]+np.array(shift)*255),0,255).astype(np.uint8)
    return result


def main():
    OUT.mkdir(exist_ok=True)
    colors=json.loads((ROOT/'outputs/olhos-todas-formas-20261020/CanonicalComplexion-complete.json').read_text())
    shifts={r['texture']:r['shift'] for r in colors['entries']}
    tokens=['RequiemAttackKatanaYoungCardinal_Violet','RequiemAttackKatanaYoungCardinal',
            'RockWalkKatanaYoung_Crimson','RockCombatSupportKatanaYoung_Violet',
            'RequiemIdleLivingCardinal20261017_Absolute','RockIdleLivingDiagonal20261017_Violet',
            'RequiemCombatSupportDaggersYoung_Violet']
    tiles,records=[],[]
    for token in tokens:
        matches=[p for p in META.glob('*.json') if json.loads(p.read_text())['image'].endswith('/'+token+'.png') or Path(json.loads(p.read_text())['image']).stem==token]
        if len(matches)!=1:
            raise ValueError((token,len(matches)))
        d=json.loads(matches[0].read_text())
        source=Path(d['image']); raw=Image.open(source).convert('RGBA')
        for index in ([0,1,2,3] if token.endswith('Violet') and ('AttackKatana' in token or 'SupportDaggers' in token) else [0]):
            f=d['frames'][index];x,y,w,h=f['rect']
            tile=raw.crop((x,y,x+w,y+h))
            owner=source.with_name(source.stem+'_Owner.png')
            if owner.exists():
                mask=np.array(Image.open(owner).convert('RGBA'))[y:y+h,x:x+w,3]
                a=np.array(tile); a[mask!=index+1,3]=0;tile=Image.fromarray(a)
            dest=OUT/(token+'-f'+str(index)+'.png')
            if not dest.exists():tile.save(dest)
            target=Image.fromarray(complexion(np.array(tile),shifts[token]))
            tiles.append((token+' f'+str(index),tile,target))
            records.append(dict(image=dest.name,source=str(source),sourceSha256=d['imageSha256'],
                                frame=f,ownershipApplied=owner.exists(),wholeFigure=True))
    cellw,cellh=660,400
    contact=Image.new('RGBA',(cellw*2,cellh*((len(tiles)+1)//2)), '#252932')
    draw=ImageDraw.Draw(contact)
    for i,(label,before,after) in enumerate(tiles):
        dx,dy=i%2*cellw,i//2*cellh
        draw.text((dx+4,dy+4),label,fill='white')
        for j,im in enumerate([before,after]):
            scale=min(1.0,300/im.width,350/im.height)
            im=im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST)
            contact.alpha_composite(im,(dx+j*330+(330-im.width)//2,dy+30))
            draw.text((dx+j*330+8,dy+cellh-22),'ORIGINAL' if j==0 else 'PELE REQUIEM',fill='white')
    contact.convert('RGB').save(OUT/'comparacao-pele.png')
    (OUT/'referencias-quadros-inteiros.json').write_text(json.dumps(records,indent=2))
    print(json.dumps(dict(fullReferences=len(records),contact=str(OUT/'comparacao-pele.png'))))


if __name__=='__main__':main()
