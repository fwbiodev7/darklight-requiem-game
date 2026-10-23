"""Prepare an explicit DARKLIGHT-only publication scope; preserve unrelated changes.

Never deletes files, rewrites history, pushes, or touches saves. Staging is opt-in.
"""
from pathlib import Path
import argparse,hashlib,json,shutil,subprocess
ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'work/publicacao-20261021'
REMOTE='https://github.com/fwbiodev7/darklight-requiem-game.git'
OUT=ROOT/'outputs/expansao-20261023'
def git(*args):return subprocess.check_output(['git','-C',str(DEST),*args],text=True,encoding='utf-8').strip()
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def excluded(rel):
    text=rel.as_posix().lower()
    return any(s in text for s in ['rejectedheadcomposition','draftcatalog','marching-rejected','__pycache__','audiosreview','audioreview20261017']) or rel.suffix in ['.pyc','.log','.bak']
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--stage',action='store_true');parser.add_argument('--resume-staging',action='store_true');args=parser.parse_args()
    if git('remote','get-url','origin')!=REMOTE:raise ValueError('Unexpected destination remote')
    if args.stage and git('diff','--cached','--name-only'):
        previous=OUT/'publication-pathspec.nul'
        allowed={p.decode('utf-8') for p in previous.read_bytes().split(b'\0') if p} if previous.exists() else set()
        staged=set(git('diff','--cached','--name-only').splitlines())
        if not args.resume_staging or not staged.issubset(allowed):raise ValueError('Existing staged changes must be preserved')
    candidates=set()
    directories=['Assets/Darklight','Docs/Darklight','Tools/AnimationPreview','Tools/TokenEconomy','Tools/Skills/pixel-art-sprite-sheet-master','Packages','ProjectSettings',
                 'ArtSources/Multiverse20261023','ArtSources/PlayerEyeReference20261020','ArtSources/PlayerStyleReference20261020','ArtSources/AbsoluteHairReference20261021','ArtSources/CombatHUD20261022']
    for folder in directories:candidates.update(p for p in (ROOT/folder).rglob('*') if p.is_file())
    index=ROOT/'ArtSources/RuinFists20261021/production-source-index-v2.json';candidates.add(index)
    proposed=json.loads(index.read_text(encoding='utf-8'))
    catalog=json.loads((ROOT/'Assets/Darklight/Resources/Darklight/RuinFists/catalog.json').read_text(encoding='utf-8-sig'))
    physical={Path(p['resourcePath']).name for p in catalog['pages'] if not p['paletteVariant']}
    imported=[p for p in proposed['pages'] if p['name'] in physical]
    if len(imported)!=64 or len(physical)!=64:raise ValueError('Unexpected imported physical source inventory')
    actual=dict(schemaVersion=1,status='imported-not-runtime-verified',physicalPages=len(imported),renderConfigurations=len(catalog['pages']),
                proposedIndependentBanks=proposed['physicalPages'],note='The historical v2 index is a 128-physical-bank plan. This inventory describes the 64 physical banks actually imported, with same-direction material variants.',pages=imported)
    actual_path=ROOT/'ArtSources/RuinFists20261021/production-imported-20261023.json';actual_path.write_text(json.dumps(actual,indent=2),encoding='utf-8');candidates.add(actual_path)
    for page in imported:
        candidates.add(ROOT/page['source'])
        job=ROOT/'ArtSources/RuinFists20261021/jobs'/(Path(page['source']).stem+'.json')
        if job.exists():candidates.add(job)
        for replacement in page.get('replacements',[]):candidates.add(ROOT/replacement['source'])
    for p in (ROOT/'ArtSources/RuinFists20261021/references').rglob('*'):
        if p.is_file():candidates.add(p)
    # Evidence is bounded; do not upload the large RGBA histograms or local logs.
    for name in ['asset-qa.json','PROGRESS.json','bridge-edit-review.txt','bridge-play-review.txt','air-play-trace.txt',
                 'play-tests.xml','edit-tests.xml','render-tests.xml','build-result.json','build-smoke.json']:
        if (OUT/name).exists():candidates.add(OUT/name)
    for folder in ['unity-editmode','unity-play']:
        candidates.update(p for p in (OUT/folder).rglob('*') if p.is_file())
    copies=[];scope=[];omitted=[]
    for source in sorted(candidates):
        rel=source.relative_to(ROOT)
        if excluded(rel):omitted.append(rel.as_posix());continue
        if not source.is_file():raise FileNotFoundError(source)
        target=DEST/rel
        if not target.resolve().is_relative_to(DEST.resolve()):raise ValueError('Path outside intended clone')
        # Old raw texture drafts are preserved locally but are not delivery inputs.
        if source.suffix.lower() in ['.ogg','.wav','.mp3']:
            continue
        if not target.exists() or sha(target)!=sha(source):
            target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target);copies.append(rel.as_posix())
        scope.append(rel.as_posix())
    # Only existing selected paths can be staged: prior deletions stay untouched.
    scope=sorted(set(scope));pathspec=OUT/'publication-pathspec.nul';pathspec.write_bytes(b''.join(p.encode('utf-8')+b'\0' for p in scope))
    if args.stage:
        result=subprocess.run(['git','-C',str(DEST),'add','--force','--pathspec-from-file='+str(pathspec),'--pathspec-file-nul'],capture_output=True,text=True,encoding='utf-8')
        if result.returncode:print(result.stderr[-2000:]);result.check_returncode()
    staged=git('diff','--cached','--name-only').splitlines() if args.stage else []
    if any(p not in scope for p in staged):raise ValueError('Staged file escaped explicit scope')
    if git('diff','--cached','--diff-filter=D','--name-only'):raise ValueError('Refusing to stage deletions')
    # Every staged working file must match the validated checkout byte-for-byte.
    for p in staged:
        if sha(ROOT/p)!=sha(DEST/p):raise ValueError('Published file differs from validated source: '+p)
    report=dict(remote=REMOTE,branch=git('branch','--show-current'),copied=len(copies),scope=len(scope),staged=len(staged),excluded=len(omitted),deletions=0,
                copies=copies,stagedPaths=staged,validation='Staged bytes equal the tested checkout; raw audio, rejected art, saves, logs and other projects excluded.')
    (OUT/'publication-scope.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k not in ['copies','stagedPaths']}))
if __name__=='__main__':main()
