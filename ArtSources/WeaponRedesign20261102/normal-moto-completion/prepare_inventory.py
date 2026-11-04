from pathlib import Path
import json,hashlib,re
from PIL import Image
ROOT=Path.cwd(); ART=ROOT/'ArtSources/WeaponRedesign20261102/normal-moto-completion'; OUT=ROOT/'outputs/retomada-20261102/normal-moto-completion'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
res=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'; cat=json.loads((res/'catalog.json').read_text(encoding='utf-8-sig'))
pages=[p for p in cat['pages'] if p['skin']==0 and p['weapon']==3 and p['form']==0]
refs=['ReqKatanaNormalNeutralIdleS_SW_W20261025','ReqFistsNormalWalkS20261101V2Selected','ReqMotoNormalDualSaws8AttackS20261027V1','ReqMotoNormalJoinSeparateS20261028V1','RequiemMotoForm0Ride8SlamS20261027V3','RequiemMotoForm0IdleSSWW20261027MatureV1','RequiemMotoForm0IdleNW_N_NE20261027MatureV1','RequiemMotoForm0IdleE_SE20261027MatureV1']
refs+=sorted({p['resourcePath'].split('/')[-1] for p in pages});references=[]
for name in dict.fromkeys(refs):
 p=res/(name+'.png'); j=res/(name+'.json');m=json.loads(j.read_text(encoding='utf-8-sig'));im=Image.open(p)
 references.append(dict(name=name,path=p.relative_to(ROOT).as_posix(),sha256=sha(p),size=list(im.size),jsonSha256=sha(j),sourcePath=m.get('sourcePath'),sourceSha256=m.get('sourceSha256'),pixelsPerUnit=m.get('pixelsPerUnit'),frames=len(m['frames']),status=m.get('status'),inspectedWhole=name in refs[:4]))
tech=[]
for filename in ['DarklightAbyssRider.cs','DarklightAbyssExpansion.cs']:
 p=ROOT/'Assets/Darklight/Runtime/Campaign'/filename;s=p.read_text(encoding='utf-8-sig')
 for match in re.finditer(r'Make\("([^"]+)",StrikeKind\.(\w+),([.\d]+)f,([.\d]+)f,([.\d]+)f',s):
  name,kind,st,ac,rc=match.groups();mounted=name in ['Investida Demoniaca','Derrapagem Sangrenta','Investida Final','Cavalgada Brutal'];hybrid=name in ['Esmagamento do Abismo','Esmagamento Vertical']
  tech.append(dict(attackKey=name,kind=kind,startupMs=round(float(st)*1000),activeMs=round(float(ac)*1000),recoveryMs=round(float(rc)*1000),context='mounted' if mounted else 'onFoot-or-mounted-slam' if hybrid else 'onFoot-DualSaws',dedicatedEightDirections=name=='Dentes Cruzados',pending=name!='Dentes Cruzados',runtimeFile=p.relative_to(ROOT).as_posix()))
write(OUT/'inventory-pre-gate.json',dict(date='2026-11-02',gate='awaiting GATE15 LIBERADO',generationStarted=False,complete=False,worldVisualScale=1.325,exclusiveOwnership=[ART.relative_to(ROOT).as_posix(),OUT.relative_to(ROOT).as_posix()],catalogSha256=sha(res/'catalog.json'),catalogPages=pages,references=references,techniques=tech,pendingTechniques=sum(t['pending'] for t in tech),walkDirections=[],protected=['Dentes Cruzados 8 directions','Dodge 8 directions','Mounted Idle 8 directions','Join/Separate S','Approved mounted finish/slam S'],clockContract=dict(attacks='CombatModel.StateTime sampled against startup/active/recovery; AttackClockSpeed=1 for Dashing/Slamming/Sustaining',locomotion='CombatModel.Clock or passed livingTime; direction S SW W NW N NE E SE',transitions='Dashing startup assembles; recovery separates; no gameplay timing/events changes',mountedPresentation='Active && (Dashing || Slamming && SlamFromMounted)',warning='Catalog attack pages are not used while MountedPresentation is true; coordinator must wire any future mounted technique pages without changing gameplay events.'),states=['candidate','visual-reviewed','imported','runtime-verified'],assetsModified=False,runtimeModified=False,unityExecuted=False,gitModified=False))
print(json.dumps(dict(references=len(references),catalogPages=len(pages),techniques=len(tech),pendingTechniques=sum(t['pending'] for t in tech),output=str(OUT/'inventory-pre-gate.json'),sha256=sha(OUT/'inventory-pre-gate.json'))))
