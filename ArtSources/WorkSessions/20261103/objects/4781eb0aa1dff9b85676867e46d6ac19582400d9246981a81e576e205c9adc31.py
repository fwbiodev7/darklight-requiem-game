"""Append an explicitly reviewed, self-contained packet; never overwrite existing art."""
import argparse,hashlib,json,shutil,subprocess
from pathlib import Path
BASE=Path(__file__).resolve().parents[1]
ROOT=BASE/'outputs/darklight-requiem-game'
OUT=BASE/'outputs/fechamento-fabius-20261103'
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def context(p):return tuple(p.get(k,'') or '' for k in ('skin','weapon','form','direction','action','attackKey'))
def main():
    p=argparse.ArgumentParser();p.add_argument('--packet',type=Path,required=True);p.add_argument('--tag',required=True);a=p.parse_args()
    packet=a.packet.resolve();assert packet.is_relative_to(OUT.resolve())
    control=read(OUT/'CONTROLE-LOCAL.json');assert control['integrationOwner']=='Codex /root' and not control.get('activeUnityPid')
    assert not (ROOT/'Temp/UnityLockfile').exists(),'Verify/remove only the stale owned lock before project writes'
    pages=read(packet/'PAGES.json')['pages'];names=sorted({x['resourcePath'].split('/')[-1] for x in pages})
    catalog=read(BANK/'catalog.json');old_context={context(x) for x in catalog['pages']}
    assert not any(context(x) in old_context for x in pages),'Existing context needs explicit reconciliation'
    assert all(x['status']=='visual-reviewed' and x['wholeFigures'] for x in pages)
    receipts=OUT/'applied-increments'/a.tag;assert not receipts.exists(),'Do not overwrite an applied increment'
    sources=[];assets=[]
    for name in names:
        image=packet/'staging'/(name+'.png');descriptor=packet/'staging'/(name+'.json');d=read(descriptor)
        assert d['status']=='visual-reviewed' and d['wholeFigures'] and sha(image)==d['imageSha256']
        source=packet/d['sourcePath'];target=(ROOT/d['sourcePath']).resolve()
        assert target.is_relative_to((ROOT/'ArtSources').resolve()) and sha(source)==d['sourceSha256']
        assert not target.exists() or sha(target)==sha(source)
        sources.append((source,target))
        for ext in ('.png','.json'):
            target_asset=BANK/(name+ext);assert not target_asset.exists(),'Never overwrite existing drawing/descriptor'
            assets.append(target_asset.relative_to(ROOT).as_posix())
    receipts.mkdir(parents=True)
    shutil.copy2(BANK/'catalog.json',receipts/'catalog-before.json')
    for source,target in sources:target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,target)
    control['allowedAssetTargets']=sorted(set(control['allowedAssetTargets']+assets))
    control['lastAppliedIncrement']=a.tag
    (OUT/'CONTROLE-LOCAL.json').write_text(json.dumps(control,indent=2,ensure_ascii=False),encoding='utf-8')
    command=['python',str(ROOT/'Tools/Delivery/integrate_reviewed_20261031.py'),'--staged',str(packet/'staging'),'--pages',str(packet/'PAGES.json'),'--names',*names,'--output',str(ROOT/'outputs/fechamento-fabius-20261103'/a.tag)]
    subprocess.run(command,check=True)
    result={'sheets':len(names),'clips':len(pages),'frames':sum(len(read(packet/'staging'/(n+'.json'))['frames']) for n in names),'applied':True,'unityImported':False,'runtimeVerified':False,'assets':assets}
    (receipts/'receipt.json').write_text(json.dumps(result,indent=2),encoding='utf-8');print(json.dumps(result))
if __name__=='__main__':main()
