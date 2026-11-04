"""Preserve today's external handoff losslessly; candidates stay outside Assets.

Run from the original workspace. --verify checks the portable archive without it.
Repeated payloads reference a repository file or one SHA256 object. Large text is
gzip-compressed; original bytes, paths and hashes are recoverable from manifest.
"""
from pathlib import Path
import argparse,gzip,hashlib,json,re,subprocess

ROOT=Path(__file__).resolve().parents[2]
ARCHIVE=ROOT/'ArtSources/WorkSessions/20261103'
WORKSPACE=ROOT.parents[1]
SECRET=re.compile(rb'(?<![A-Za-z0-9])(?:gh[pousr]_[A-Za-z0-9]{30,}|sk-(?:proj-)?[A-Za-z0-9_-]{32,}|Authorization:\s*Bearer\s+[A-Za-z0-9._-]{24,})')

def digest(data):return hashlib.sha256(data).hexdigest()
def git(*args):return subprocess.check_output(['git','-C',str(ROOT),*args])
def verify(index=False):
    m=json.loads((ARCHIVE/'manifest.json').read_text(encoding='utf-8'))
    blobs={}
    if index:
        for entry in git('ls-files','--stage','-z').split(b'\0'):
            if entry:
                meta,path=entry.split(b'\t',1);blobs[path.decode('utf-8')]=meta.split()[1].decode()
        proc=subprocess.Popen(['git','-C',str(ROOT),'cat-file','--batch'],stdin=subprocess.PIPE,stdout=subprocess.PIPE)
    for row in m['files']:
        p=ROOT/row['storedPath']
        if index:
            proc.stdin.write((blobs[row['storedPath']]+'\n').encode());proc.stdin.flush()
            header=proc.stdout.readline().split();assert header[1]==b'blob'
            data=proc.stdout.read(int(header[2]));assert proc.stdout.read(1)==b'\n'
        else:data=p.read_bytes()
        if row['encoding']=='gzip':data=gzip.decompress(data)
        assert len(data)==row['originalBytes'] and digest(data)==row['sha256'],row['originalPath']
    if index:proc.stdin.close();assert proc.wait()==0
    return {'restorableFiles':len(m['files']),'verified':True,'originalMiB':round(sum(x['originalBytes'] for x in m['files'])/1048576,2),'uniqueObjectMiB':round(sum(p.stat().st_size for p in (ARCHIVE/'objects').iterdir())/1048576,2)}

def build():
    assert not (ARCHIVE/'manifest.json').exists(),'Existing archive: verify instead of overwrite'
    assert not (ROOT/'Temp/UnityLockfile').exists(),'Check active Unity/lock before snapshot'
    paths=[]
    for folder in (WORKSPACE/'outputs/fechamento-fabius-20261103',WORKSPACE/'outputs/gothic-eclipse-20261103',ROOT/'outputs/fechamento-fabius-20261103'):
        paths.extend(p for p in folder.rglob('*') if p.is_file() and '__pycache__' not in p.parts)
    work=WORKSPACE/'work'
    for p in work.rglob('*'):
        if not p.is_file() or any(x in p.relative_to(work).parts for x in ('clone-incompleto','darklight-requiem-game','__pycache__','.git')):continue
        # Keep art/integration tools and previews. Git transport artifacts are reproducible.
        if len(p.relative_to(work).parts)==1 and p.suffix not in ('.py','.json'):continue
        if p.name in ('tree-api.json',):continue
        paths.append(p)
    # Reuse exact source/asset payloads already present in this repository.
    existing={}
    candidates=[ROOT/p.decode('utf-8') for p in git('ls-files','-z').split(b'\0') if p]
    candidates.extend(p for parent in (ROOT/'Assets/Darklight',ROOT/'ArtSources/ReviewedDeliveries',ROOT/'ArtSources/WeaponRedesign20261103') for p in parent.rglob('*') if p.is_file())
    for p in candidates:
        if p.is_file() and not p.is_relative_to(ARCHIVE) and p.suffix.lower() in ('.png','.jpg','.jpeg','.gif','.apng'):existing.setdefault(digest(p.read_bytes()),p.relative_to(ROOT).as_posix())
    (ARCHIVE/'objects').mkdir(parents=True,exist_ok=True)
    records=[]
    for p in sorted(set(paths)):
        data=p.read_bytes();sha=digest(data)
        if p.suffix.lower() in ('.txt','.log','.json','.py','.md','.html') and SECRET.search(data):raise ValueError('Potential credential: '+str(p))
        if sha in existing:
            stored=existing[sha];encoding='identity'
        else:
            packed=gzip.compress(data,compresslevel=9,mtime=0) if p.suffix.lower() in ('.json','.html','.log','.txt','.csv','.md','.py') and len(data)>262144 else data
            encoding='gzip' if packed is not data else 'identity'
            suffix='.gz' if encoding=='gzip' else p.suffix.lower() or '.bin'
            stored=(ARCHIVE/'objects'/(sha+suffix)).relative_to(ROOT).as_posix()
            (ROOT/stored).write_bytes(packed);existing[sha]=stored
        # A reused gzip object must retain its decoding contract.
        if stored.endswith('.gz') and '/WorkSessions/20261103/objects/' in stored:encoding='gzip'
        records.append({'originalPath':p.relative_to(WORKSPACE).as_posix(),'sha256':sha,'originalBytes':len(data),'storedPath':stored,'encoding':encoding})
    manifest={'sessionDate':'2026-11-03','baseCommit':git('rev-parse','HEAD').decode().strip(),'publication':'Session preservation; candidates/rejected art are not gameplay assets or approvals.','excluded':'Git transport/cache clones, __pycache__, reproducible git object lists and transport logs; all remain locally. No user saves, music, builds or unrelated projects.','files':records}
    (ARCHIVE/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    return verify()

if __name__=='__main__':
    a=argparse.ArgumentParser(description=__doc__);a.add_argument('--verify',action='store_true');a.add_argument('--verify-index',action='store_true');args=a.parse_args()
    print(json.dumps(verify(index=args.verify_index) if args.verify or args.verify_index else build()))
