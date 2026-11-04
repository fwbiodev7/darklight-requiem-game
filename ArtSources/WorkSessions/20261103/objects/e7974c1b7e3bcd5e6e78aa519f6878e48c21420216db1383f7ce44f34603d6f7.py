"""Apply reviewed four-file increment with exact source checks and backups."""
import hashlib,json,shutil,subprocess
from datetime import datetime,timezone
from pathlib import Path
base=Path(__file__).resolve().parents[1]
stage=base/'outputs/gothic-eclipse-20261103/start'
manifest=json.loads((stage/'manifest.json').read_text(encoding='utf-8-sig'))
root=Path(manifest['source_root']).resolve()
def git(*args):return subprocess.check_output(['git','-C',str(root),*args],text=True).strip()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
assert Path(git('rev-parse','--show-toplevel')).resolve()==root
assert git('branch','--show-current')=='integracao-completa-20261026'
receipt=stage/'application-receipt.json'
assert not receipt.exists(), 'Increment already applied; inspect receipt'
for f in manifest['files']:
    target=root/f['path'];source=stage/'files'/f['path']
    assert sha(source)==f['staged_sha256'],f['path']
    assert (sha(target) if target.exists() else None)==f['source_sha256'],f['path']
result={'timestampUtc':datetime.now(timezone.utc).isoformat(),'branch':git('branch','--show-current'),'head':git('rev-parse','HEAD'),'state':'APPLIED_TO_PROJECT','unityCompiled':False,'runtimeVerified':False,'files':[]}
for f in manifest['files']:
    target=root/f['path'];source=stage/'files'/f['path'];meta=Path(str(target)+'.meta')
    mh=sha(meta) if meta.exists() else None
    if target.exists():
        backup=stage/'before'/f['path'];backup.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(target,backup)
    target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
    assert sha(target)==f['staged_sha256']
    assert (sha(meta) if meta.exists() else None)==mh
    result['files'].append(dict(f,existingMetaSha256=mh))
receipt.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
controlpath=base/'outputs/fechamento-fabius-20261103/CONTROLE-LOCAL.json'
c=json.loads(controlpath.read_text(encoding='utf-8-sig'))
c['gothicEclipse']={'state':'FIRST_INCREMENT_APPLIED_TO_PROJECT','files':4,'receipt':str(receipt),'unityCompiled':False,'runtimeVerified':False,'authorization':'Recent human request: finish animation feedback and start Gothic Eclipse now; build and gameplay screenshots after graphical evolution is complete.'}
c['appliedInAssets'].update(sheets=23,clips=83,frames=188)
c['latestHumanDirection']='Audit visual proportions across Fabius actions; preserve approved art; begin Gothic Eclipse. Future order: Gothic integrated build/gameplay screenshots, then Lucius.'
c['allowedAssetTargets']=sorted(set(c['allowedAssetTargets']+[f['path'] for f in manifest['files']]))
controlpath.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'state':result['state'],'files':len(result['files']),'receipt':str(receipt),'runtimeVerified':False}))
