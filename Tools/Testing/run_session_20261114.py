"""Run one silent Unity stage with the existing standard resource guards."""
import argparse
import json
from pathlib import Path
import sys
import xml.etree.ElementTree as ET
import os
import subprocess

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'Tools/Delivery'))
import run_guarded_build_20261101 as guard


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('stage',choices=['import','walk-import','official-import','EditMode','PlayMode','capture','build'])
    parser.add_argument('--filter')
    parser.add_argument('--build-output',default='outputs/build-windows-gothic-20261114/Windows')
    parser.add_argument('--clean-player-cache',action='store_true',help='Rebuild failed player data without clearing imported source assets.')
    parser.add_argument('--uncompressed-player',action='store_true',help='Use standard standalone data files instead of the failing compressed archive writer.')
    parser.add_argument('--rebuild-failed-output',action='store_true',help='Retry only a task-local output whose existing receipt records build errors.')
    parser.add_argument('--finalize-staged-player',action='store_true',help='Finalize scripts/binaries after the current player data has already been staged.')
    parser.add_argument('--unrestricted-memory',action='store_true',help='User-authorized task-local execution without the RAM/commit stop threshold.')
    args=parser.parse_args()
    if (ROOT/'Temp/UnityLockfile').exists(): raise RuntimeError('Unity project already open or stale lock requires verification')
    memory=guard.memory()
    if not args.unrestricted_memory and (memory.available<2.5*1024**3 or memory.pageAvailable<3*1024**3):
        raise RuntimeError('Insufficient standard memory reserve')
    stamp=guard.gate.now().replace(':','').replace('.','-')
    output=ROOT/'work/session20261114'/f'{args.stage}-{stamp}'
    output.mkdir(parents=True,exist_ok=True)
    gate=guard.gate
    gate.OUT=gate.WORK=output
    gate.state={'stages':[],'scope':'Session14 reviewed integration and candidate transitions','audio':False}
    guard.MONITOR=output/'resource-monitor.json'
    xml=output/'tests.xml'
    if args.stage=='build':
        destination=(ROOT/args.build_output).resolve()
        if (destination/'DarklightRequiem.exe').exists():
            previous=destination/'build-result.json'
            allowed_root=(ROOT/'outputs').resolve()
            if not args.rebuild_failed_output or allowed_root not in destination.parents or not previous.is_file():
                raise RuntimeError('Existing build preserved; select a fresh destination before rebuilding')
            previous_result=json.loads(previous.read_text(encoding='utf-8-sig'))
            if previous_result.get('errors',0)<=0:
                raise RuntimeError('Successful output preserved; failed-output retry requires an error receipt')
        command=['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightLuciusTestBuild.Build',
                 '-darklightLuciusOutput',str(destination)]
        if args.clean_player_cache: command.append('-darklightCleanBuildCache')
        if args.uncompressed_player: command.append('-darklightUncompressedPlayer')
        if args.finalize_staged_player: command.append('-darklightFinalizeStagedPlayer')
    elif args.stage=='walk-import':
        command=['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightFabiusFormsImporter.ImportWalkRepair']
    elif args.stage=='official-import':
        command=['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightSession14Delivery.ImportOfficialTransformations']
    elif args.stage=='import':
        command=['-nographics','-quit','-executeMethod','Darklight.Editor.DarklightSession14Delivery.ImportSources']
    elif args.stage=='capture':
        command=['-force-d3d11','-executeMethod','Darklight.Editor.DarklightTransitionPreviewDelivery.Capture']
    else:
        if not args.filter: raise ValueError('Explicit test filter required')
        command=['-force-d3d11' if args.stage=='PlayMode' else '-nographics','-runTests','-testPlatform',args.stage,'-testFilter',args.filter,'-testResults',str(xml)]
    try:
        if args.unrestricted_memory:
            gate.state['memoryAuthorization']='User 2026-11-14: trabalhe com toda memoria q precisar sem protecao; task-local RAM/commit threshold disabled.'
            environment=os.environ.copy();environment['BEE_BUILD_THREADS']='1'
            full=[str(gate.UNITY),'-batchmode','-darklightSilent','-job-worker-count','1',
                  '-projectPath',str(ROOT),'-logFile',str(output/'unity.log'),*command]
            with (output/'stdout.log').open('wb') as log:
                result=subprocess.run(full,cwd=ROOT,env=environment,stdout=log,stderr=subprocess.STDOUT,
                                      creationflags=subprocess.BELOW_NORMAL_PRIORITY_CLASS,timeout=1200)
            gate.state['exitCode']=result.returncode
            if result.returncode: raise RuntimeError(f'Unity exit {result.returncode}; inspect {output}')
        else:
            guard.guarded_execute('integration-play' if args.stage=='PlayMode' else 'import-reviewed',command,
                                  timeout=900,xml=xml if args.stage in ('EditMode','PlayMode') else None)
        if args.stage=='build':
            report=json.loads((destination/'build-result.json').read_text(encoding='utf-8-sig'))
            gate.state['buildResult']=report
            if report.get('result')!='Succeeded' or report.get('errors',1)!=0:
                raise RuntimeError('Build receipt contains errors; preserve as candidate, not delivery')
            if args.uncompressed_player:
                data=destination/'DarklightRequiem_Data'
                for filename in ('globalgamemanagers','resources.assets','level0'):
                    item=data/filename
                    if not item.is_file() or item.stat().st_size==0:
                        raise RuntimeError('Uncompressed player data missing or unfinished: '+filename)
            else:
                archive=destination/'DarklightRequiem_Data/data.unity3d'
                if not archive.is_file() or archive.stat().st_size==0 or archive.with_suffix('.unity3d.tmp').exists():
                    raise RuntimeError('Compressed player archive missing or unfinished; not a complete delivery')
    finally:
        if xml.exists():
            tree=ET.parse(xml).getroot()
            gate.state['testResult']={'result':tree.get('result'),'passed':tree.get('passed'),'failed':tree.get('failed'),
                                     'failures':[{'name':t.get('fullname'),'message':t.findtext('failure/message')}
                                                 for t in tree.iter('test-case') if t.get('result')=='Failed']}
        (output/'receipt.json').write_text(json.dumps(gate.state,indent=2)+'\n',encoding='utf-8')
        print(json.dumps({'output':str(output),'tests':gate.state.get('testResult')},ensure_ascii=True))


if __name__=='__main__':main()
