"""Read-only integrity check of complete source figures and technical owner labels."""
from pathlib import Path
import json,hashlib,numpy as np
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/fidelidade-original-20261018';PACK=OUT/'pacote-v6'
sha=lambda data:hashlib.sha256(data).hexdigest()
raw=(PACK/'manifesto.json').read_bytes();manifest=json.loads(raw)
plan=json.loads((PACK/'fontes.json').read_text(encoding='utf-8'))
cal={a['file']:a['calibration'] for a in plan['assets']}
loaded={};components=0;selected=set();checked=set()
for sheet in manifest['sheets']:
    path=OUT/sheet['file'];assert sha(path.read_bytes())==sheet['sha256']
    im=Image.open(path).convert('RGBA');pixels=np.array(im)
    owners=np.array(Image.open(OUT/sheet['ownerFile']).convert('L'))
    assert list(im.size)==sheet['size'] and owners.shape==pixels.shape[:2]
    assert [int(pixels[:,:,3].min()),int(pixels[:,:,3].max())]==sheet['alphaRange']
    assert pixels[:,:,3].min()==0
    ids=np.unique(owners);assert ids[0]==0
    components+=len(ids)-1
    loaded[sheet['file']]=(pixels,owners)
for page in manifest['pages']:
    active=set(i for indices in page['directions'].values() for i in indices)
    for variant in page.get('variants',{}).values():active.update(variant['indices'])
    for idx,frame in enumerate(page['frames']):
        pixels,owners=loaded[frame['sheet']];owned=owners==frame['ownerId']
        key=(frame['sheet'],frame['ownerId'])
        if key not in checked:
            assert owned.any() and sha(pixels[owned].tobytes())==frame['pixelHash']
            yy,xx=np.where(owned);x0,y0,x1,y1=frame['rect']
            assert x0<=xx.min() and y0<=yy.min() and x1>xx.max() and y1>yy.max()
            checked.add(key)
        if idx in active:selected.add(key)
        c=cal[Path(frame['sheet']).name]
        assert abs(frame['renderScale']-c['targetBodyHeightPx']/c['bodyHeightPx'])<1e-10
report=dict(date='2026-10-19',manifestSha256=sha(raw),sourcePngs=len(loaded),sourceComponents=components,
    checkedWholeFigureHashes=len(checked),selectedUniqueWholeFigures=len(selected),pages=len(manifest['pages']),
    checks=['Source SHA256 and dimensions','real alpha0 background','owner dimensions and identities','whole source figure pixel hashes','complete figure bounds','one manual anatomical scale per source'],
    errors=0,pixelEdits=0,visualApproval=False,productionImported=False,
    limits=['No artistic approval, anatomy or perceived-motion certification.','Low-alpha pixels outside technical ownership are not character cutouts.','Other directions remain disabled.'])
(PACK/'QA-FONTES-20261019.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report))
