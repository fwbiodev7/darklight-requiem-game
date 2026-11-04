from pathlib import Path
import hashlib,json,shutil
B=Path(__file__).resolve().parents[1];out=B/'outputs/fechamento-fabius-20261103/moto-qa/parry-anatomy-correction';out.mkdir(exist_ok=True)
gen=Path('C:/Users/Usuario/.codex/generated_images/01a1a117-ff4b-72b3-9c2a-e2735c75b8ff')
items=[('attempt1-REJECTED-childlike.png','exec-d44f53c7-bb92-4e95-ae44-16ac96e6cd57.png','HUMAN_REJECTED: body too short, dwarf/childlike. Never integrate.'),('attempt2-katana-defense-base.png','exec-3f9934f7-8fb5-4a85-850f-d89ff27afe54.png','CANDIDATE: direct Katana defense anatomy base. Await autonomous visual review, not human-approved.')]
records=[]
for name,raw,status in items:
    target=out/name
    if not target.exists():shutil.copy2(gen/raw,target)
    records.append({'file':name,'generatedSource':str(gen/raw),'sha256':hashlib.sha256(target.read_bytes()).hexdigest(),'status':status,'appliedInAssets':False,'unityImported':False})
(out/'attempts.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
print(json.dumps(records))
