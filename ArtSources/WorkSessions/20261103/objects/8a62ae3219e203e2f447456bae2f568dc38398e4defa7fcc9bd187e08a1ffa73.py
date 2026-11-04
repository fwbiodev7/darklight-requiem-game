"""Preserve newer home assets/GUIDs and close the historical session archive."""
from pathlib import Path
import subprocess,json,hashlib
B=Path(__file__).resolve().parents[1];R=B/'outputs/darklight-requiem-game';A=R/'ArtSources/WorkSessions/20261103'
LOCAL='3b4eeb42bc0f59ac35c6130a6dd3db330fd456a5';BASE='852e612cd38d12845c2bc0134e9c1af4fb610d26';REMOTE='origin/integracao-completa-20261026'
def git(*args):return subprocess.check_output(['git','-C',str(R),*args])
def names(*args):return set(git(*args).decode().splitlines())
assert (R/'.git/MERGE_HEAD').exists(),'Expected active merge'
ours=names('diff','--no-renames','--name-only',BASE,LOCAL);theirs=names('diff','--no-renames','--name-only',BASE,REMOTE)
overlap=ours&theirs;conflicts=names('diff','--name-only','--diff-filter=U')
assert conflicts<=overlap,sorted(conflicts-overlap)
assert all(p.startswith('Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/') for p in overlap)
keep=sorted(overlap|{'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json'})
subprocess.run(['git','-C',str(R),'restore','--source='+REMOTE,'--staged','--worktree','--',*keep],check=True)
# All historical image references become self-contained, even if home assets changed.
m=json.loads((A/'manifest.json').read_text(encoding='utf-8'));closed=0
for row in m['files']:
    if '/WorkSessions/20261103/objects/' in row['storedPath']:continue
    data=git('show',LOCAL+':'+row['storedPath']);assert hashlib.sha256(data).hexdigest()==row['sha256']
    target=A/'objects'/(row['sha256']+Path(row['storedPath']).suffix.lower())
    if not target.exists():target.write_bytes(data)
    row['storedPath']=target.relative_to(R).as_posix();row['encoding']='identity';closed+=1
(A/'manifest.json').write_text(json.dumps(m,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
assert not names('diff','--name-only','--diff-filter=U')
receipt={'localPreservationCommit':LOCAL,'newerHomeCommit':git('rev-parse',REMOTE).decode().strip(),'retainedRemotePaths':keep,'resolvedConflicts':len(conflicts),'archiveReferencesMadeSelfContained':closed,'policy':'Preserve newer home assets, GUIDs and catalog. Older school material remains in historical commit/session archive, never used to overwrite final home sources.','unityExecutedDuringPublication':False}
(R/'Docs/Darklight/PUBLICACAO-PRESERVACAO-20261104.json').write_text(json.dumps(receipt,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'remotePathsRetained':len(keep),'conflictsResolved':len(conflicts),'archiveReferencesMadeSelfContained':closed}))
