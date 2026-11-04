"""Apply only the checked essentials and the user-selected lateral V3 to this isolated clone."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess

BASE=Path(__file__).resolve().parents[1]
ROOT=BASE/'outputs/darklight-requiem-game'
OUT=BASE/'outputs/fechamento-fabius-20261103'
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
PACKET=OUT/'source-audit/portable-essentials'
MOTO=OUT/'moto-qa/approved-v3'
CONTROL=OUT/'CONTROLE-LOCAL.json'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def context(p):return tuple(p.get(k,'') or '' for k in ('skin','weapon','form','direction','action','attackKey'))
def main():
    control=read(CONTROL)
    if control['integrationOwner']!='Codex /root':raise ValueError('Unexpected owner')
    if ROOT.resolve()!=Path(control['projectRoot']).resolve():raise ValueError('Unexpected project root')
    if subprocess.check_output(['git','-C',str(ROOT),'rev-parse','HEAD']).decode().strip()!=control['baseCommit']:raise ValueError('Base moved')
    # Capture before any project mutation; never overwrite this initial evidence.
    before=OUT/'integration-before.json'
    if before.exists():raise ValueError('Initial integration already attempted; inspect receipt before retry')
    targets=[];sources=[];batches=[]
    for staged,pages,tag in [(PACKET/'staging',PACKET/'PAGES.json','essentials'),(MOTO,MOTO/'PAGES-2clips.json','moto-v3')]:
        page_data=read(pages)['pages'];names=sorted({p['resourcePath'].split('/')[-1] for p in page_data})
        for name in names:
            desc=read(staged/(name+'.json'))
            if desc['status']!='visual-reviewed' or not desc['wholeFigures']:raise ValueError('Not reviewed: '+name)
            if sha(staged/desc['image'])!=desc['imageSha256']:raise ValueError('Image changed: '+name)
            source=PACKET/desc['sourcePath'] if tag=='essentials' else MOTO/'V3-source.png'
            if sha(source)!=desc['sourceSha256']:raise ValueError('Source changed: '+name)
            if tag=='moto-v3' and (not desc['humanApproved'] or desc['sourceSha256']!='5de9d53e5e1a6976a7824d6bdc6c12f324757c81547c2b1e5584e92ea684e01e'):raise ValueError('Wrong approved version')
            sources.append((source,ROOT/desc['sourcePath']))
            for ext in ('.png','.json'):
                dest=BANK/(name+ext)
                if dest.exists():raise ValueError('Refuse to overwrite existing asset: '+str(dest))
                targets.append(str(dest.relative_to(ROOT)))
        batches.append((staged,pages,names,tag))
    merged=read(BANK/'catalog.json')
    new_pages=read(PACKET/'PAGES.json')['pages']+read(MOTO/'PAGES-2clips.json')['pages']
    new_context={context(p) for p in new_pages}
    collisions=[p['id'] for p in merged['pages'] if context(p) in new_context]
    if collisions:raise ValueError('Existing contexts require explicit review: '+repr(collisions))
    protected={str(p.relative_to(ROOT)):sha(p) for p in (ROOT/'Assets').rglob('*') if p.is_file() and p.suffix.lower() in ('.png','.meta')}
    approved={str(p.relative_to(ROOT)):sha(p) for p in (ROOT/'ArtSources/ReviewedDeliveries/FabiusNormal20261103').rglob('*') if p.is_file()}
    before.write_text(json.dumps({'protectedAssets':protected,'archivedDelivery':approved,'catalogSha256':sha(BANK/'catalog.json'),'plannedAssets':targets,'newPages':len(new_pages),'collisions':collisions},indent=2),encoding='utf-8')
    editor_target=ROOT/'Assets/Darklight/Editor/DarklightFabiusBoundedImport.cs'
    if editor_target.exists():raise ValueError('Bounded importer unexpectedly exists')
    control.update(state='INTEGRATING_ESSENTIALS_LOCAL',allowedAssetTargets=targets+[str((BANK/'catalog.json').relative_to(ROOT)),str(editor_target.relative_to(ROOT))],allowedNewSources=sorted({str(d.relative_to(ROOT)) for _,d in sources}),metaPolicy='New targets may receive new Unity metas; existing metas/pixels must remain unchanged')
    CONTROL.write_text(json.dumps(control,indent=2,ensure_ascii=False),encoding='utf-8')
    for source,dest in sources:
        if dest.exists() and sha(dest)!=sha(source):raise ValueError('Refuse source overwrite: '+str(dest))
        dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,dest)
    shutil.copy2(OUT/'integration-tools/DarklightFabiusBoundedImport.cs',editor_target)
    for staged,pages,names,tag in batches:
        command=['python',str(ROOT/'Tools/Delivery/integrate_reviewed_20261031.py'),'--staged',str(staged),'--pages',str(pages),'--names',*names,'--output',str(ROOT/('outputs/fechamento-fabius-20261103/'+tag))]
        subprocess.run(command,check=True)
    request={'paths':[p for p in targets if p.endswith('.png')],'receipt':str(OUT/'unity/import-receipt.json')}
    (OUT/'unity').mkdir(exist_ok=True)
    (OUT/'unity/request.json').write_text(json.dumps(request,indent=2),encoding='utf-8')
    changed=[p for p,v in protected.items() if sha(ROOT/p)!=v]
    archived_changed=[p for p,v in approved.items() if sha(ROOT/p)!=v]
    if changed or archived_changed:raise ValueError('Protected content changed before Unity')
    print(json.dumps({'sheets':len(request['paths']),'clips':len(new_pages),'protectedAssetsUnchanged':len(protected),'archivedFilesUnchanged':len(approved)}))
if __name__=='__main__':main()
