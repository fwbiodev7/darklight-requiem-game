from pathlib import Path
from collections import Counter
from PIL import Image
import json,hashlib,py_compile
ROOT=Path.cwd();ART=ROOT/'ArtSources/WeaponRedesign20261102/normal-moto-completion';OUT=ROOT/'outputs/retomada-20261102/normal-moto-completion'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
p=OUT/'inventory-pre-gate.json';m=json.loads(p.read_text(encoding='utf-8'))
seen={'ReqKatanaNormalNeutralIdleS_SW_W20261025','ReqFistsNormalWalkS20261101V2Selected','ReqMotoNormalDualSaws8AttackS20261027V1','ReqMotoNormalJoinSeparateS20261028V1','ReqMotoNormalDentesNW_N_NE20261031V1','ReqMotoNormalDentesS_E_SE20261031V1','RequiemMotoForm0Ride8SlamS20261027V3'}
for ref in m['references']:ref['inspectedWhole']=ref['name'] in seen
p2=ROOT/'ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png';m['eyeReference']=dict(path=p2.relative_to(ROOT).as_posix(),sha256=sha(p2),inspectedWhole=True,authority='Eyes only; brown Normal hair preserved; no transformed red hair transferred')
m['knownPreviousGaitIssues']='MotoWalk01 V1 rejected all; V2 has opposite contacts4/5/11 but no complete cycle. Old KatanaSW/W not suitable motion authority per existing moto-walk-edit report; no new audit.'
write(p,m)
ref=m['references'][2];png=ROOT/ref['path'];colors=Counter(c for c in Image.open(png).convert('RGBA').getdata() if c[3]>0)
ordered=sorted(colors.items());write(OUT/'reference-palette.json',dict(reference=ref['path'],imageSha256=sha(png),uniqueColorCount=len(colors),fullColorTableSha256=hashlib.sha256(json.dumps(ordered,separators=(',',':')).encode()).hexdigest(),top256=[dict(rgba=list(c),count=n) for c,n in colors.most_common(256)],paletteQuantized=False,semanticMaterials=dict(skin='Approved warm peach [246,174,130] base with shades',eyes='Dark red #C40814/#5A0000, clean surrounding skin',hair='Natural compact brown',coat='Black with burgundy lining',saws='Neutral black/silver, exactly two')))
py_compile.compile(str(ART/'prepare_batch.py'),cfile=str(ART/'prepare_batch.pyc'),doraise=True)
files=[ART/'prepare_inventory.py',ART/'prepare_batch.py',p,OUT/'reference-palette.json']
checkpoint=dict(date='2026-11-02',state='prepared-awaiting-gate15',complete=False,generationStarted=False,generationInFlight=False,figuresProduced=0,clipsReviewed=0,imported=False,runtimeVerified=False,assetsModified=False,runtimeModified=False,unityExecuted=False,gitModified=False,helperValidation='prepare_batch.py --help exit0; py_compile success; not applied to new generated art',files=[dict(path=f.relative_to(ROOT).as_posix(),sha256=sha(f)) for f in files],nextExact='After explicit GATE15 LIBERADO: generate8–12 complete figures per call for onFoot DualSaws Walk; use FistsWalkS gait + adult Katana identity + directional saw equipment. Finish8directions before next action. Persist source PNG intact, inspect each full figure/stride, prepare candidate metadata+native body PPU+compact<=4096 atlas+QA/previews. Preserve approved Dentes/Dodge/Idle8 and Join/FinishS.')
write(OUT/'checkpoint-preparation.json',checkpoint)
print(json.dumps(dict(checkpoint=str((OUT/'checkpoint-preparation.json').relative_to(ROOT)),sha256=sha(OUT/'checkpoint-preparation.json'),files=checkpoint['files'],uniqueReferenceColors=len(colors))))
