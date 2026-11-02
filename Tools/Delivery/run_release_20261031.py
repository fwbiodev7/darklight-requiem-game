"""Reuse the proven sequential gates, with fresh dated output and additional regression coverage."""
from datetime import datetime
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys
import zipfile

import finalize_20261027 as release

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261031'
WORK=ROOT/'work/retomada-20261031'/('release-'+datetime.now().strftime('%H%M%S'))
release.OUT,release.WORK=OUT,WORK


def main():
    global OUT, WORK
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--session-date',default='20261031')
    parser.add_argument('--skip-import',action='store_true')
    parser.add_argument('--tests-only',action='store_true')
    args=parser.parse_args()
    if not re.fullmatch(r'\d{8}',args.session_date):
        raise ValueError('Session date must use YYYYMMDD')
    datetime.strptime(args.session_date,'%Y%m%d')
    OUT=ROOT/('outputs/retomada-'+args.session_date)
    WORK=ROOT/('work/retomada-'+args.session_date)/('release-'+datetime.now().strftime('%H%M%S'))
    release.OUT,release.WORK=OUT,WORK
    OUT.mkdir(parents=True,exist_ok=True);WORK.mkdir(parents=True,exist_ok=True)
    if (ROOT/'Temp/UnityLockfile').exists():
        raise RuntimeError('Unity project already locked; do not launch another instance')
    selection=json.loads((ROOT/'work/retomada-20261026/final-gate-selection.json').read_text())
    edits=selection['editClasses']+['Darklight.Tests.DarklightSaveContractTests','Darklight.Tests.DarklightTestOutputTests',
        'Darklight.Tests.DarklightCombatBufferRegressionTests','Darklight.Tests.DarklightCombatTests',
        'Darklight.Tests.DarklightTechnicalCombatTests']
    play=selection['playFilters']+[
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.EveryActiveWeaponStateExcludesRetiredCharacterFrames',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.ReviewedKatanaWalkLoopsReachAllDeclaredFramesAndReturnToIdle',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.ConfirmedContactEmphasizesWholePoseButWhiffsDoNot',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.RecoveredMovesVisitEveryAuthoredPhaseAndPreserveContactTiming',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.ActionParticlesEmitOnceForRealStatesAndRespectLimits',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.CatalogDodgeClipsRunThroughRealCombatClockAndReturnToIdle',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.ReviewedFistsSouthWalkLoopsAllFramesAndSettlesToIdle',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.PixelAtmospherePreservesGeometryAndCharacterAndCapturesGameplay',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.WeaponDodgeParticlesAreDistinctAndExpireQuickly',
        'Darklight.Tests.DarklightWeaponRedesignGameplayTests.RecoveredMotorcycleTransitionUsesWholeStartupAndRecoveryFigures']
    stamp=datetime.now().strftime('%H%M%S')
    edit=OUT/('final-edit-'+stamp+'.xml');played=OUT/('final-play-'+stamp+'.xml')
    if '--skip-import' in sys.argv:
        previous=json.loads((OUT/'release-progress.json').read_text(encoding='utf-8'))
        imported=next((s for s in previous['stages'] if s['stage']=='import' and s['completed']),None)
        if imported is None:raise RuntimeError('No successful import to reuse')
        ended=datetime.fromisoformat(imported['ended']).timestamp()
        for folder in ('WeaponRedesign20261023','CombatContacts20261027'):
            for path in (ROOT/'Assets/Darklight/Resources/Darklight'/folder).rglob('*'):
                if path.is_file() and path.suffix!='.meta' and path.stat().st_mtime>ended+1:
                    raise RuntimeError('Resource changed after import: '+str(path))
        release.state['stages'].append(imported);release.save()
    else:
        release.execute('import',['-quit','-executeMethod','Darklight.Editor.DarklightScope26Delivery.Import'])
    release.execute('edit',['-runTests','-testPlatform','EditMode','-testFilter',';'.join(edits),'-testResults',str(edit)],xml=edit)
    release.execute('play',['-runTests','-testPlatform','PlayMode','-testFilter',';'.join(play),'-testResults',str(played)],xml=played)
    if '--tests-only' in sys.argv:
        return
    free=shutil.disk_usage(ROOT).free
    release.state['freeBytesBeforeBuild']=free;release.save()
    if free<4*1024**3:
        raise RuntimeError('Less than 4 GiB free before Windows build; preserve sources and previous builds')
    release.execute('build',['-quit','-executeMethod','Darklight.Editor.DarklightWeaponRedesignBuild.Build',
        '-darklightRedesignEditResults',str(edit),'-darklightRedesignPlayResults',str(played),
        '-darklightReleaseOutput',str(OUT)],timeout=900)
    receipt=json.loads((OUT/'build-result.json').read_text())
    if receipt.get('result')!='Succeeded' or not (OUT/'Windows/DarklightRequiem.exe').is_file():
        raise RuntimeError('No successful new build/executable')
    release.state['buildSucceeded']=True;release.save()
    build_bytes=sum(path.stat().st_size for path in (OUT/'Windows').rglob('*') if path.is_file())
    if shutil.disk_usage(ROOT).free<build_bytes+512*1024**2:
        raise RuntimeError('Build succeeded but insufficient disk space to safely create ZIP')
    archive=OUT/('DarklightRequiem-Windows-'+args.session_date+'-'+stamp+'.zip')
    with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=3) as bundle:
        for path in sorted((OUT/'Windows').rglob('*')):
            if path.is_file() and path.suffix.lower()!='.pdb' and not any('DoNotShip' in part for part in path.parts):
                bundle.write(path,Path('DarklightRequiem')/path.relative_to(OUT/'Windows'))
    with archive.open('rb') as stream:
        digest=hashlib.file_digest(stream,'sha256').hexdigest()
    release.state.update(zip=str(archive),zipBytes=archive.stat().st_size,zipSha256=digest,finished=release.now())
    release.save();print(json.dumps({'buildSucceeded':True,'zip':str(archive),'sha256':digest}),flush=True)


if __name__=='__main__':
    try:
        main()
    except Exception as error:
        release.state['error']=str(error);release.save()
        print(str(error),file=sys.stderr);raise SystemExit(1)
