"""Extend the established publication allowlist with the reviewed transition and fresh receipts."""
import hashlib
import json
from pathlib import Path
import sys
import prepare_scope_publication_20261026 as prior

ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'work/publicacao-20261021'
OUT=ROOT/'outputs/retomada-20261031'


def main():
    prior.main()
    report=json.loads((prior.OUT/'publication-preparation.json').read_text())
    hashes=json.loads((prior.OUT/'publication-hashes.json').read_text())['sha256']
    scope=set(report['scopePaths']);extra={}
    copy='--copy' in sys.argv
    name='ReqMotoNormalJoinSeparateS20261028V1'
    bank=prior.base.BANK
    sheet=prior.base.read(ROOT/bank/(name+'.json'))
    replacement=prior.base.rebase_sheet(name,sheet)
    source=replacement['sourcePath']
    extra[source]=(ROOT/bank/sheet['image']).read_bytes()
    for suffix in ('.png','.png.meta','.json.meta'):
        path=bank/(name+suffix)
        if (ROOT/path).is_file():extra[path.as_posix()]=(ROOT/path).read_bytes()
    extra[(bank/(name+'.json')).as_posix()]=prior.base.encoded(replacement)
    for path in OUT.glob('*'):
        if (path.is_file() and path.suffix in ('.json','.xml') and path.stat().st_size<2_000_000
                and not path.name.startswith(('catalog-before','publication-'))):
            extra[path.relative_to(ROOT).as_posix()]=path.read_bytes()
    for relative in ('standalone/result.json','standalone/capture-summary.json'):
        path=OUT/relative
        if path.is_file():extra[path.relative_to(ROOT).as_posix()]=path.read_bytes()
    for path in (ROOT/'Tools/Delivery').glob('*.py'):
        extra[path.relative_to(ROOT).as_posix()]=path.read_bytes()
    self=Path(__file__);extra[self.relative_to(ROOT).as_posix()]=self.read_bytes()
    for relative,data in extra.items():
        target=DEST/relative
        if not target.resolve().is_relative_to(DEST.resolve()) or prior.private_reference(relative):
            raise ValueError('Unsafe publication path: '+relative)
        if copy:
            target.parent.mkdir(parents=True,exist_ok=True)
            if not target.exists() or target.read_bytes()!=data:target.write_bytes(data)
        hashes[relative]=hashlib.sha256(data).hexdigest();scope.add(relative)
    ordered=sorted(scope)
    forbidden=[p for p in ordered if p.lower().endswith(('.zip','.exe','.mp3','.mp3.meta','.pyc'))
        or '/saves/' in p.lower() or '/windows/' in p.lower() or '__pycache__' in p.lower()]
    if forbidden:raise ValueError('Forbidden paths in allowlist: '+str(forbidden[:5]))
    OUT.mkdir(parents=True,exist_ok=True)
    (OUT/'publication-pathspec.nul').write_bytes(b''.join(p.encode('utf-8')+b'\0' for p in ordered))
    result={'date':'2026-10-31','branch':report['branch'],'initialHead':report['head'],'copied':copy,
        'scopePaths':ordered,'scopeCount':len(ordered),'buildsIncluded':False,'savesIncluded':False,
        'rejectedSourceSheetsIncluded':False,'transitionSourceRebasedToSelectedAtlas':True}
    (OUT/'publication-preparation.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    (OUT/'publication-hashes.json').write_text(json.dumps({'sha256':hashes},indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in result.items() if k!='scopePaths'}))


if __name__=='__main__':main()
