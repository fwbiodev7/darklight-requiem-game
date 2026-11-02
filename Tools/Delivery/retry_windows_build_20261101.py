"""Retry the same validated Windows build without an Editor graphics device."""
from pathlib import Path
from datetime import datetime
import hashlib,json,zipfile
import finalize_20261027 as gate

ROOT=Path(__file__).resolve().parents[2]
gate.OUT=ROOT/'outputs/retomada-20261101'
gate.WORK=ROOT/'work/retomada-20261101'/('build-headless-'+datetime.now().strftime('%H%M%S'))
gate.WORK.mkdir(parents=True,exist_ok=True)
gate.state=json.loads((gate.OUT/'release-progress.json').read_text())
stages={s['stage']:s for s in gate.state['stages'] if s.get('completed')}
assert stages['edit']['passed']==144 and stages['play']['passed']==30
assert not (ROOT/'Temp/UnityLockfile').exists(),'Preserve the active Unity session'
gate.state.pop('error',None)
edit=max(gate.OUT.glob('final-edit-*.xml'),key=lambda p:p.stat().st_mtime)
play=max(gate.OUT.glob('final-play-*.xml'),key=lambda p:p.stat().st_mtime)
try:
    gate.execute('build-headless',['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightWeaponRedesignBuild.Build',
        '-darklightRedesignEditResults',str(edit),'-darklightRedesignPlayResults',str(play),'-darklightReleaseOutput',str(gate.OUT)],timeout=900)
    receipt=json.loads((gate.OUT/'build-result.json').read_text())
    assert receipt['result']=='Succeeded' and (gate.OUT/'Windows/DarklightRequiem.exe').is_file()
    gate.state['buildSucceeded']=True;gate.save()
    archive=gate.OUT/('DarklightRequiem-Windows-20261101-'+datetime.now().strftime('%H%M%S')+'.zip')
    with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=3) as bundle:
        for p in sorted((gate.OUT/'Windows').rglob('*')):
            if p.is_file() and p.suffix.lower()!='.pdb' and not any('DoNotShip' in part for part in p.parts):
                bundle.write(p,Path('DarklightRequiem')/p.relative_to(gate.OUT/'Windows'))
    with archive.open('rb') as stream:digest=hashlib.file_digest(stream,'sha256').hexdigest()
    gate.state.update(zip=str(archive),zipBytes=archive.stat().st_size,zipSha256=digest,finished=gate.now());gate.save()
    print(json.dumps(dict(buildSucceeded=True,zip=str(archive))),flush=True)
except Exception as error:
    gate.state['error']=str(error);gate.save();raise
