"""Verify production PNG provenance and the protected death/reversion baseline."""
from pathlib import Path
import hashlib,json,re
ROOT=Path(__file__).resolve().parents[2]
out=ROOT/'outputs/finalizacao-20261018'
baseline=(ROOT/'outputs/plano-implementacao-20261017/morte-preservada-sha256.json').read_text(encoding='utf-8-sig')
allowed=set(re.findall(r'[0-9a-fA-F]{64}',baseline.lower()))
protected=list((ROOT/'outputs/transformacoes-animacoes-20261017').glob('morte-*.png'))+[ROOT/'Assets/Darklight/Runtime/Character'/n for n in ['DarklightDeathReversion.cs','DarklightOriginalVampireView.cs']]
assert len(protected)==8
rows=[]
for p in protected:
    digest=hashlib.sha256(p.read_bytes()).hexdigest()
    assert digest in allowed,'Protected source changed: '+str(p)
    rows.append(dict(file=p.relative_to(ROOT).as_posix(),sha256=digest,unchanged=True))
source_rows=[]
review=json.loads((ROOT/'outputs/revisao-consistencia-20261017/manifesto-revisao.json').read_text())['sheets']
for page in review:
    name=page['name'];directory='AbyssPrototype' if 'AttackSix' in name else 'TransformedYoung' if name.endswith(('_Crimson','_Violet','_Absolute')) else 'TodayCharacterAnimations'
    path=ROOT/'Assets/Darklight/Resources/Darklight'/directory/(name+'.png')
    assert hashlib.sha256(path.read_bytes()).hexdigest()==page['sha256'],name
    source_rows.append(name)
for page in json.loads((out/'direcionais-manifesto.json').read_text())['pages']:
    path=ROOT/'Assets/Darklight/Resources/Darklight/AbyssPrototype'/(page['name']+'.png')
    assert hashlib.sha256(path.read_bytes()).hexdigest()==page['sourceSha256'],page['name']
    source_rows.append(page['name'])
assert not list((ROOT/'Assets').rglob('WhiteRequiemDaggers20261018*')),'Unapproved additional study entered production'
report=dict(success=True,protectedFiles=rows,productionSourcesVerified=len(source_rows),sourcePNGsUnchanged=True,additionalDaggerStudyExcluded=True)
(out/'auditoria-fontes.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:report[k] for k in ['success','productionSourcesVerified','sourcePNGsUnchanged','additionalDaggerStudyExcluded']}))
