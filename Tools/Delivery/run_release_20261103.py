"""Release 2026-11-03: Full Fabius Requiem Normal build with proven sequential execution and automated validation."""
from datetime import datetime, timezone
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
import zipfile

import finalize_20261027 as release

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/retomada-20261103'
WORK = ROOT / 'work/retomada-20261103' / ('release-' + datetime.now().strftime('%H%M%S'))
release.OUT, release.WORK = OUT, WORK

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--build-only', action='store_true')
    args = parser.parse_args()

    OUT.mkdir(parents=True, exist_ok=True); WORK.mkdir(parents=True, exist_ok=True)
    if (ROOT / 'Temp/UnityLockfile').exists():
        raise RuntimeError('Unity project already locked; do not launch another instance')
    
    stamp = datetime.now().strftime('%H%M%S')

    if args.build_only:
        edit = max(OUT.glob('final-edit-*.xml'), key=lambda p: p.stat().st_mtime)
        played = max(OUT.glob('final-play-*.xml'), key=lambda p: p.stat().st_mtime)
        print(f'Reusing fresh validated test XMLs:\n  Edit: {edit.name}\n  Play: {played.name}', flush=True)
    else:
        selection = json.loads((ROOT / 'work/retomada-20261026/final-gate-selection.json').read_text())
        edits = selection['editClasses'] + [
            'Darklight.Tests.DarklightSaveContractTests', 'Darklight.Tests.DarklightTestOutputTests',
            'Darklight.Tests.DarklightCombatBufferRegressionTests', 'Darklight.Tests.DarklightCombatTests',
            'Darklight.Tests.DarklightTechnicalCombatTests']
        play = selection['playFilters'] + [
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
        
        edit = OUT / ('final-edit-' + stamp + '.xml')
        played = OUT / ('final-play-' + stamp + '.xml')

        print('[1/4] Running EditMode test gate...', flush=True)
        release.execute('edit', ['-runTests', '-testPlatform', 'EditMode', '-testFilter', ';'.join(edits), '-testResults', str(edit)], xml=edit)

        print('[2/4] Running PlayMode test gate...', flush=True)
        release.execute('play', ['-runTests', '-testPlatform', 'PlayMode', '-testFilter', ';'.join(play), '-testResults', str(played)], xml=played)

    free = shutil.disk_usage(ROOT).free
    release.state['freeBytesBeforeBuild'] = free; release.save()
    if free < 3 * 1024**3:
        raise RuntimeError('Less than 3 GiB free before Windows build')

    print('[3/4] Running Windows standalone build...', flush=True)
    release.execute('build-headless', [
        '-nographics', '-quit', '-executeMethod', 'Darklight.Editor.DarklightWeaponRedesignBuild.Build',
        '-darklightRedesignEditResults', str(edit), '-darklightRedesignPlayResults', str(played),
        '-darklightReleaseOutput', str(OUT)], timeout=900)

    receipt = json.loads((OUT / 'build-result.json').read_text())
    exe = OUT / 'Windows/DarklightRequiem.exe'
    if receipt.get('result') != 'Succeeded' or not exe.is_file():
        raise RuntimeError('No successful new build/executable created')
    release.state['buildSucceeded'] = True; release.save()

    print('[4/4] Creating Windows release ZIP archive...', flush=True)
    archive = OUT / ('DarklightRequiem-Windows-20261103-' + stamp + '.zip')
    with zipfile.ZipFile(archive, 'x', zipfile.ZIP_DEFLATED, compresslevel=3) as bundle:
        for path in sorted((OUT / 'Windows').rglob('*')):
            if path.is_file() and path.suffix.lower() != '.pdb' and not any('DoNotShip' in part for part in path.parts):
                bundle.write(path, Path('DarklightRequiem') / path.relative_to(OUT / 'Windows'))
    with archive.open('rb') as stream:
        digest = hashlib.file_digest(stream, 'sha256').hexdigest()
    release.state.update(zip=str(archive), zipBytes=archive.stat().st_size, zipSha256=digest, finished=release.now())
    release.save()
    print(json.dumps({'buildSucceeded': True, 'zip': str(archive), 'sha256': digest}), flush=True)

if __name__ == '__main__':
    try:
        main()
    except Exception as error:
        release.state['error'] = str(error); release.save()
        print(str(error), file=sys.stderr); raise SystemExit(1)
