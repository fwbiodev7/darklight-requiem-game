"""Read-only package preflight: hashes and git apply --check; never applies patches."""
from datetime import datetime,timezone
from pathlib import Path
import hashlib,json,subprocess
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261102'
PUB=ROOT/'work/publicacao-20261021'
NAMES=['antigravity-gothic-eclipse','antigravity-lucius-normal','antigravity-combat-2']
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def within(path,boundary):
    p=path.resolve()
    if not p.is_relative_to(boundary.resolve()):raise ValueError('Package path escapes allowed boundary')
    return p
def git(cwd,*args):
    r=subprocess.run(['git','-C',str(cwd),*args],capture_output=True,text=True,encoding='utf-8',errors='replace')
    return {'exitCode':r.returncode,'passed':r.returncode==0,'output':(r.stderr or r.stdout).strip()[:2200]}
def main():
    result={'receivedUtc':datetime.now(timezone.utc).isoformat(),'patchesApplied':False,'assetsModified':False,
        'unityExecuted':False,'technicalReviewCompleted':False,'publicationHead':git(PUB,'rev-parse','HEAD')['output'],
        'order':['Fabius Normal finalization and validation','Gothic Eclipse','Lucius Normal','Transformations','Combat AI2.0'],
        'packages':[]}
    for name in NAMES:
        folder=ROOT/'work'/name
        manifest=read(folder/'manifest.json')
        row={'folder':folder.relative_to(ROOT).as_posix(),'manifestSha256':sha(folder/'manifest.json'),
            'declaredBaseCommit':manifest.get('baseCommit'),'hashChecks':[],'baseChecks':[],
            'documentsPresent':[],'documentsMissing':[],'documentSnapshots':{},'technicalReview':'deferred-until-priority-stage',
            'status':'received-not-approved','candidateCodeNotCompiled':True}
        for doc in ['REPORT.md','VALIDATION.md','REUSE-MAP.md']:
            if (folder/doc).is_file():
                row['documentsPresent'].append(doc);row['documentSnapshots'][doc]=sha(folder/doc)
            else:row['documentsMissing'].append(doc)
        for item in manifest.get('files',[]):
            candidate=within(ROOT/item['source'],folder)
            target=within(ROOT/item['target'],ROOT/'Assets')
            actual=sha(candidate) if candidate.is_file() else None
            row['hashChecks'].append({'file':item['source'],'expected':item['candidateSha256'],
                'actual':actual,'passed':actual is not None and actual.lower()==item['candidateSha256'].lower()})
            base=item.get('baseSha256')
            current=sha(target) if target.is_file() else None
            row['baseChecks'].append({'target':item['target'],'expectedBase':base,'current':current,
                'matches':not target.exists() if base=='NEW-FILE' else current is not None and current.lower()==(base or '').lower()})
        for group in ['candidates','documentation']:
            for item in manifest.get(group,[]):
                p=within(folder/item['path'],folder)
                actual=sha(p) if p.is_file() else None
                row['hashChecks'].append({'file':p.relative_to(ROOT).as_posix(),'expected':item['sha256'],
                    'actual':actual,'passed':actual is not None and actual.lower()==item['sha256'].lower(),
                    'bytesMatch':p.is_file() and p.stat().st_size==item['bytes']})
        if manifest.get('canonicalReference'):
            ref=within(ROOT/manifest['canonicalReference'],ROOT/'ArtSources')
            actual=sha(ref) if ref.is_file() else None
            row['hashChecks'].append({'file':manifest['canonicalReference'],'expected':manifest['canonicalReferenceSha256'],
                'actual':actual,'passed':actual is not None and actual.lower()==manifest['canonicalReferenceSha256'].lower()})
        patch=folder/'PATCH.diff'
        row['patchSha256']=sha(patch)
        row['applyCheckCurrentWorkspace']=git(ROOT,'apply','--check','--',str(patch))
        row['applyCheckPublicationWorktree']=git(PUB,'apply','--check','--',str(patch))
        row['declaredHashesPassed']=all(c['passed'] and c.get('bytesMatch',True) for c in row['hashChecks'])
        if not row['declaredHashesPassed'] or not row['applyCheckCurrentWorkspace']['passed']:
            row['status']='received-preflight-failed-not-approved'
        result['packages'].append(row)
    receipt=OUT/'antigravity-receipt-preflight.json'
    receipt.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    state_path=OUT/'continuation-state.json';state=read(state_path)
    state.update(antigravityPackagesReceipt=receipt.relative_to(ROOT).as_posix(),
        antigravityPackagesApplied=False,antigravityPackageReviewOrder=result['order'])
    state_path.write_text(json.dumps(state,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'packages':[{'folder':r['folder'],'hashChecks':len(r['hashChecks']),
        'hashesPassed':r['declaredHashesPassed'],'baseMatches':[b['matches'] for b in r['baseChecks']],
        'patchCheckWorkspace':r['applyCheckCurrentWorkspace']['passed'],
        'patchCheckPublication':r['applyCheckPublicationWorktree']['passed'],
        'checkError':r['applyCheckCurrentWorkspace']['output'][:210],
        'missingDocuments':r['documentsMissing']} for r in result['packages']],
        'patchesApplied':False,'unityExecuted':False}))
if __name__=='__main__':main()
