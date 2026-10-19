"""Read Unity text catalogs without loading the engine or touching source assets."""
import csv,itertools,json,re,struct
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/plano-implementacao-20261017'
RES=ROOT/'Assets/Darklight/Resources'
def scalar(block,name,default=''):
    match=re.search(r'^    '+name+r': (.*)$',block,re.M)
    return match.group(1).strip() if match else default
def main():
    text=(RES/'Darklight/TodayCharacterCatalog.asset').read_text(encoding='utf-8')
    pages=[];coverage={}
    for block in re.split(r'^  - name: ',text,flags=re.M)[1:]:
        packed=re.match(r'[0-9a-fA-F]*',scalar(block,'directions')).group()
        data=bytes.fromhex(packed);directions=list(struct.unpack('<'+'i'*(len(data)//4),data))
        page=dict(name=block.splitlines()[0],kind=scalar(block,'kind'),skin=int(scalar(block,'skin')),
                  weapon=int(scalar(block,'weapon')),phases=int(scalar(block,'phases')),directions=directions,
                  resourcePath=scalar(block,'resourcePath'),paletteResourcePath=scalar(block,'paletteResourcePath'),
                  spriteNames=len(re.findall(r'^    - Today_',block,re.M)))
        page['textureFileExists']=(RES/(page['resourcePath']+'.png')).is_file()
        page['paletteFileExists']=(RES/(page['paletteResourcePath']+'.png')).is_file()
        for direction in directions:coverage.setdefault((page['skin'],page['weapon'],direction,page['kind']),[]).append(page['name'])
        pages.append(page)
    transformed={}
    text=(RES/'Darklight/TransformedCatalog.asset').read_text(encoding='utf-8')
    for block in re.split(r'^  - baseName: ',text,flags=re.M)[1:]:
        transformed[(block.splitlines()[0],int(scalar(block,'form')))]=True
    rows=[]
    for skin,weapon,form,direction,kind in itertools.product(range(2),range(3),range(4),range(8),
        ['Idle','Walk','Attack','Combat','GuardDodge','IdleAlert','IdleImpatient','IdleInspect','IdleFatigued','IdleWounded','StartMove','Brake','Turn']):
        names=coverage.get((skin,weapon,direction,kind),[])
        matching=names if form==0 else [n for n in names if (n,form) in transformed]
        rows.append(dict(skin=['Requiem','Rock'][skin],weapon=['Katana','Adagas','Corrente/foice'][weapon],form=form,
            direction=direction,kind=kind,catalogPages=';'.join(matching),
            state='described-not-engine-verified' if matching else 'no-direct-descriptor; inspect existing fallback'))
    with (OUT/'cobertura-catalogo.csv').open('w',encoding='utf-8-sig',newline='') as f:
        writer=csv.DictWriter(f,fieldnames=rows[0].keys());writer.writeheader();writer.writerows(rows)
    summary=dict(normalPages=len(pages),normalSpriteNames=sum(p['spriteNames'] for p in pages),
        missingTextureFiles=[p['name'] for p in pages if not p['textureFileExists']],
        missingPaletteFiles=[p['name'] for p in pages if not p['paletteFileExists']],
        transformedDescriptors=len(transformed),coverageRows=len(rows),engineVerified=False,
        notes=['Catalog coverage only; files present do not prove correct import or gameplay.',
               'Cavaleiro uses its own bank and is deliberately excluded from the three-weapon matrix.',
               'No-direct-descriptor is not automatically a broken animation: existing fallback may cover it.',
               'New one-view PNG studies under outputs are not counted as imported production coverage.'],pages=pages)
    (OUT/'inventario-catalogos.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({k:v for k,v in summary.items() if k not in ['notes','pages']},ensure_ascii=False))
if __name__=='__main__':main()
