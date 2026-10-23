"""Consolidate actual current Unity results, build and preservation evidence."""
from pathlib import Path
import json,hashlib,xml.etree.ElementTree as ET,shutil
ROOT=Path(__file__).resolve().parents[2];PACK=ROOT/'outputs/fidelidade-original-20261018/pacote-v6'
files=json.loads((PACK/'QA-PUBLISH-RUNTIME.json').read_text())['changedFiles']
source_hashes={p:hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in files}
baseline=json.loads((ROOT/'outputs/plano-implementacao-20261017/morte-preservada-sha256.json').read_text())
mismatches=[p for p,h in baseline.items() if hashlib.sha256((ROOT/p).read_bytes()).hexdigest()!=h]
assert len(baseline)==8 and not mismatches
def cases(name):return list(ET.parse(PACK/name).iter('test-case'))
first=cases('QA-UNITY-EDITMODE-20261019.xml');retry=cases('QA-UNITY-PALETTE-20261019.xml')
final={c.get('fullname'):c.get('result') for c in first+retry}
assert len(final)==10 and all(v=='Passed' for v in final.values())
play=cases('QA-UNITY-GAMEPLAY-20261019.xml')+cases('QA-UNITY-HUD-20261019.xml')
assert len(play)==7 and all(c.get('result')=='Passed' for c in play)
native=json.loads((ROOT/'work/native-runtime-20261019/resultado-executavel.json').read_text());assert native['success']
build=json.loads((ROOT/'outputs/revisao-runtime-20261019/build-result.json').read_text());assert build['success']
perf=json.loads((ROOT/'outputs/validacao-pendencias-20261019/combate-real-editor.json').read_text())
report=dict(date='2026-10-19',runtimeSourceSha256=source_hashes,
    preservedPreexistingRuntimeSha256={'Assets/Darklight/Runtime/World/DarklightAtlas.cs':hashlib.sha256((ROOT/'Assets/Darklight/Runtime/World/DarklightAtlas.cs').read_bytes()).hexdigest()},
    editMode=dict(uniquePassed=10,failed=0,initialRunPassed=9,initialRunFailed=1,correctedFixtureRetryPassed=1,
        fixtureCorrection='Palette test now creates a child trail object, matching runtime; initial fixture incorrectly combined MeshRenderer/SpriteRenderer on one GameObject.'),
    playMode=dict(passed=7,failed=0,skipped=0,cases=[c.get('fullname') for c in play],isolatedStorage=True),
    nativeExecutable=native,build=build,combatEditor=perf,previewArtImported=False,audioStarted=False,audioAuditioned=False,physicalControllerTested=False,
    protectedFileCount=8,protectedMismatches=mismatches,
    limits=['Editor performance is not standalone FPS certification.','Native check exercises Cavaleiro in four forms, not every game chapter.','Candidate preview art remains external.','Initial native -batchmode run stalled before checks; normal hidden noaudio run completed successfully.'])
(PACK/'QA-RUNTIME.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
shutil.copy2(ROOT/'work/native-runtime-20261019/resultado-executavel.json',ROOT/'outputs/revisao-runtime-20261019/native-check.json')
print(json.dumps(dict(editModePassed=10,playModePassed=7,nativePassed=native['success'],protectedFiles=8,buildVersion=build['version'])))
