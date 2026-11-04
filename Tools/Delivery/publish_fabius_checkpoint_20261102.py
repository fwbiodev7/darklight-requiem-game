"""Publish only an explicit documentation checkpoint; never runtime, artwork or game assets."""
from datetime import datetime,timezone
import hashlib,json,shutil,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'work/publicacao-20261021'
OUT=ROOT/'outputs/retomada-20261102'
BRANCH='integracao-completa-20261026'
REMOTE='https://github.com/fwbiodev7/darklight-requiem-game.git'
FILES=[
 'Docs/Darklight/FABIUS-PRODUCAO-20261102-TARDE.md',
 'Docs/Darklight/ESCOPO-ENXUTO-FABIUS-20261102.md',
 'Docs/Darklight/PENDENCIAS-ANIMACOES-20261102.md',
 'Docs/Darklight/FECHAMENTO-20261102.md',
 'Docs/Darklight/RETOMADA-20261102.md',
 'outputs/retomada-20261102/animation-progress-current.json',
 'outputs/retomada-20261102/continuation-state.json',
 'outputs/retomada-20261102/implementation-receipt.json',
 'outputs/retomada-20261102/art-integration.json',
 'outputs/retomada-20261102/afternoon-preservation.json',
]
def git(*args):
    r=subprocess.run(['git','-C',str(DEST),*args],capture_output=True,text=True,encoding='utf-8',errors='replace')
    if r.returncode:raise RuntimeError('git '+args[0]+': '+r.stderr[-1200:])
    return r.stdout.strip()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def preserved_update(source,target):
    if not target.is_file():return False
    if sha(source)==sha(target):return True
    if source.suffix=='.md':
        a=source.read_text(encoding='utf-8-sig');b=target.read_text(encoding='utf-8-sig')
        return a.endswith(b) or a.startswith(b)
    a=json.loads(source.read_text(encoding='utf-8-sig'));b=json.loads(target.read_text(encoding='utf-8-sig'))
    a.pop('updatedUtc',None);b.pop('updatedUtc',None)
    return a==b
def main():
    if git('branch','--show-current')!=BRANCH or git('remote','get-url','origin')!=REMOTE:
        raise RuntimeError('Wrong publication branch/remote')
    if git('diff','--cached','--name-only'):raise RuntimeError('Preserve existing index')
    before=git('rev-parse','HEAD')
    for name in FILES:
        p=Path(name)
        if p.suffix not in ('.md','.json') or p.parts[0] not in ('Docs','outputs'):
            raise RuntimeError('Documentation-only allowlist violation')
        if not (ROOT/p).is_file():raise RuntimeError('Missing checkpoint source: '+name)
        if git('diff','HEAD','--',name) and not preserved_update(ROOT/p,DEST/p):
            raise RuntimeError('Preserve unrelated changes: '+name)
        if (DEST/p).is_file() and not git('ls-files','--',name) and not preserved_update(ROOT/p,DEST/p):
            raise RuntimeError('Preserve unrelated untracked file: '+name)
    for name in FILES:
        target=DEST/name;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/name,target)
        if sha(target)!=sha(ROOT/name):raise RuntimeError('Copy hash mismatch')
    selection=OUT/'checkpoint-publication-paths.nul'
    selection.write_bytes(b''.join(name.encode()+b'\0' for name in FILES))
    git('add','--force','--pathspec-from-file='+str(selection),'--pathspec-file-nul')
    staged=git('diff','--cached','--name-only').splitlines()
    if not set(staged)<=set(FILES):raise RuntimeError('Unexpected staging')
    git('diff','--cached','--check')
    if staged:
        message=OUT/'checkpoint-git-message.txt'
        message.write_text('docs: checkpoint Fabius production and reduced animation scope\n\n'
            'Record 89 locally imported clips and explicit shared animation families.\n'
            'Runtime/art publication and final tests remain pending; no game assets included.\n',encoding='utf-8')
        git('commit','--file',str(message))
    head=git('rev-parse','HEAD')
    receipt={'updatedUtc':datetime.now(timezone.utc).isoformat(),'branch':BRANCH,'remote':REMOTE,
        'previousHead':before,'commit':head,'scope':'documentation-checkpoint-only','files':staged,
        'runtimeAndArtPublished':False,'runtimePublicationGateBypassed':False,'pushed':False}
    final=OUT/'final-git-receipt.json'
    if final.exists():shutil.copy2(final,OUT/'final-git-receipt-before-afternoon.json')
    try:
        git('push','origin','HEAD:refs/heads/'+BRANCH)
        remote=git('ls-remote','origin','refs/heads/'+BRANCH).split()[0]
        if remote!=head:raise RuntimeError('Remote head differs after push')
        receipt.update(pushed=True,remoteHead=remote)
    except Exception as error:
        receipt['error']=str(error);raise
    finally:final.write_text(json.dumps(receipt,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:receipt[k] for k in ('commit','pushed','scope','runtimeAndArtPublished')}))
if __name__=='__main__':main()
