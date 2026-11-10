"""Merge reviewed incremental action manifests without touching preserved Idle pages."""
import hashlib
import json
import argparse
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
FOLDER=ROOT/'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
DIRS=['S','SW','W','NW','N','NE','E','SE']
KEYS=['SwordHorizontal','SwordVertical','SwordThrust']


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--walk',default='outputs/lucius-20261110/walk/walk-pages.json')
    parser.add_argument('--attacks',action='append')
    parser.add_argument('--without-walk',action='store_true')
    args=parser.parse_args()
    catalog=json.loads((FOLDER/'catalog.json').read_text())
    idle=[page for page in catalog['pages'] if page['action']=='Idle']
    additions=[]
    paths=[] if args.without_walk else [args.walk]
    paths+=args.attacks or ['outputs/lucius-20261110/attacks/pages-horizontal-v3.json']
    for path in paths:
        data=json.loads((ROOT/path).read_text())
        for page in data['pages']:
            assert page['status']=='visual-reviewed' and page['wholeFigures']
            assert page['action'] in ('Walk','Attack')
            image=ROOT/'Assets/Darklight/Resources'/Path(page['resourcePath']+'.png')
            assert hashlib.sha256(image.read_bytes()).hexdigest()==page['imageSha256']
            additions.append(page)
    contexts=[(p['action'],p['direction'],p.get('attackKey','')) for p in additions]
    assert len(contexts)==len(set(contexts))
    catalog['pages']=idle+additions
    catalog['complete']=False  # Other directions/actions remain explicitly missing.
    (FOLDER/'catalog.json').write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    coverage=dict(upstream='dd03b4f',idleDirections=8,
        walkDirections=[DIRS[p['direction']] for p in additions if p['action']=='Walk'],
        attacks={key:[DIRS[p['direction']] for p in additions if p.get('attackKey')==key] for key in KEYS},
        missingWalk=[d for i,d in enumerate(DIRS) if ('Walk',i,'') not in contexts],
        missingAttacks={key:[d for i,d in enumerate(DIRS) if ('Attack',i,key) not in contexts] for key in KEYS},
        integratedActionPages=len(additions),animatedFigures=sum(len(p['spriteNames']) for p in additions),
        complete=False,emotionRefinement='Horizontal V3: closed mouth, slight angry eyebrows, dramatic posture; user approved 2026-11-10',
        imported=False,runtimeVerified=False)
    out=ROOT/'outputs/lucius-20261110/coverage.json'
    out.write_text(json.dumps(coverage,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(coverage,ensure_ascii=False))


if __name__=='__main__':
    main()
