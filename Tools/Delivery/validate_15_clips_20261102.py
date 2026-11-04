"""Import only the seven reviewed sheets, then run new focused gameplay tests. No build."""
from pathlib import Path
from datetime import datetime
import json,shutil,argparse
import finalize_20261027 as gate
import run_guarded_build_20261101 as guarded
ROOT=Path(__file__).resolve().parents[2]
gate.OUT=ROOT/'outputs/retomada-20261102'
gate.WORK=ROOT/'work/retomada-20261102'/('integration-'+datetime.now().strftime('%H%M%S'))
gate.WORK.mkdir(parents=True,exist_ok=True)
parser=argparse.ArgumentParser()
parser.add_argument('--skip-import',action='store_true')
parser.add_argument('--headless-tests',action='store_true')
parser.add_argument('--force-d3d11',action='store_true')
parser.add_argument('--isolated',action='store_true')
parser.add_argument('--isolated-ram-reserve-gib',type=float,help='Explicit Nov 3 authorization; isolated tests only, minimum 1.5 GiB')
args=parser.parse_args()
if args.isolated_ram_reserve_gib is not None and (not args.isolated or not 1.5<=args.isolated_ram_reserve_gib<=2.5):
    parser.error('RAM override requires --isolated and reserve 1.5..2.5 GiB')
gate.state=json.loads((gate.OUT/'release-progress.json').read_text()) if args.skip_import else dict(stages=[],buildSucceeded=False,buildAttempted=False,importAndFocusedTestsOnly=True)
gate.state.pop('error',None)
guarded.MONITOR=gate.OUT/'integration-resource-monitor.json'
assert not (ROOT/'Temp/UnityLockfile').exists(),'Preserve active Unity session'
try:
    if args.skip_import:
        assert any(s.get('stage')=='import-reviewed' and s.get('completed') for s in gate.state['stages'])
        assert json.loads((gate.OUT/'unity-import-receipt.json').read_text())['unityImported']
    else:
        guarded.guarded_execute('import-reviewed',['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightNovemberIntegrationDelivery.Import'])
        shutil.copyfile(guarded.MONITOR,gate.OUT/'import-resource-monitor.json')
    result=gate.OUT/('integration-play-'+datetime.now().strftime('%H%M%S')+'.xml')
    guarded.guarded_execute('integration-play',(['-nographics'] if args.headless_tests else [])+(['-force-d3d11'] if args.force_d3d11 else [])+['-runTests','-testPlatform','PlayMode','-testFilter',
        ('Darklight.Tests.DarklightNovemberIsolatedGameplayTests.November' if args.isolated else 'Darklight.Tests.DarklightWeaponRedesignGameplayTests.November'), '-testResults',str(result)],xml=result,isolated_ram_reserve_gib=args.isolated_ram_reserve_gib)
    shutil.copyfile(guarded.MONITOR,gate.OUT/'play-resource-monitor.json')
    gate.state.update(runtimeVerified=True,isolatedRuntime=args.isolated,campaignSceneValidated=not args.isolated,graphicsValidated=False,finished=gate.now());gate.save()
    print(json.dumps(dict(imported=True,runtimeVerified=True,xml=str(result))),flush=True)
except Exception as error:
    gate.state['error']=str(error);gate.save();raise
