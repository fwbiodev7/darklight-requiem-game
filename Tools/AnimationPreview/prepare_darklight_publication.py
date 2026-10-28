"""Copy only DARKLIGHT deliverables into the isolated, verified publication clone.

Never stages/deletes files, touches saves, or publishes a website. Art candidates
outside the reviewed source index are deliberately excluded.
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess

ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'work/publicacao-20261021'
REMOTE='https://github.com/fwbiodev7/darklight-requiem-game.git'

def digest(path):return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--precopy',action='store_true');args=parser.parse_args()
    remote=subprocess.check_output(['git','-C',str(DEST),'remote','get-url','origin'],text=True).strip()
    if remote!=REMOTE:raise ValueError('Unexpected publication remote: '+remote)
    candidates=set()
    for directory in ['Assets/Darklight','Tools/AnimationPreview','Tools/TokenEconomy','Tools/Skills','Docs/Darklight','Packages','ProjectSettings']:
        candidates.update(p for p in (ROOT/directory).rglob('*') if p.is_file())
    art=ROOT/'ArtSources'
    candidates.update(p for p in art.rglob('*') if p.is_file() and 'RuinFists20261021' not in p.parts)
    index_path=art/'RuinFists20261021/production-source-index-v2.json'
    index=json.loads(index_path.read_text(encoding='utf-8'))
    candidates.add(index_path)
    for page in index['pages']:
        candidates.add(ROOT/page['source'])
        job=art/'RuinFists20261021/jobs'/(Path(page['source']).stem+'.json')
        if job.exists():candidates.add(job)
        for replacement in page.get('replacements',[]):candidates.add(ROOT/replacement['source'])
    for directory in ['references']:
        candidates.update(p for p in (art/'RuinFists20261021'/directory).rglob('*') if p.is_file())
    candidates.update(p for p in (art/'RuinFists20261021').glob('*') if p.is_file())
    copied=[];unchanged=0;skipped=[]
    for source in sorted(candidates):
        rel=source.relative_to(ROOT)
        if any(part in ['__pycache__','RejectedHeadComposition20261012','node_modules','.git'] for part in rel.parts) or source.suffix in ['.pyc','.log','.bak']:
            skipped.append(rel.as_posix());continue
        if 'marching-rejected' in source.name:
            skipped.append(rel.as_posix());continue
        if not source.exists():
            if args.precopy:skipped.append(rel.as_posix());continue
            raise FileNotFoundError(source)
        target=DEST/rel
        if not target.resolve().is_relative_to(DEST.resolve()):raise ValueError('Destination escaped publication clone')
        if target.exists() and digest(target)==digest(source):unchanged+=1;continue
        target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target);copied.append(rel.as_posix())
    report=dict(remote=remote,copied=len(copied),unchanged=unchanged,excluded=len(skipped),deleted=0,paths=copied,exclusions=skipped)
    out=ROOT/'outputs/preview-darklight-20261022/reports/publication-copy.json';out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k not in ['paths','exclusions']}))

if __name__=='__main__':main()
