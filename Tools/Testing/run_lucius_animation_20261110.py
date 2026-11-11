"""Import reviewed Lucius pages and run affected, silent Unity checks once."""
import argparse
import json
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Delivery'))
import run_guarded_build_20261101 as guard


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--import-reviewed', action='store_true')
    parser.add_argument('--mode', choices=['EditMode', 'PlayMode'], default='EditMode')
    parser.add_argument('--filter', required=True)
    args = parser.parse_args()
    if (ROOT / 'Temp/UnityLockfile').exists():
        raise RuntimeError('Unity project already open')
    running = subprocess.run(['tasklist', '/FI', 'IMAGENAME eq Unity.exe', '/FO', 'CSV', '/NH'],
                             capture_output=True, text=True, check=True)
    if '"Unity.exe"' in running.stdout:
        raise RuntimeError('Another Unity session is active')
    memory = guard.memory()
    if memory.available < 2.5 * 1024**3 or memory.pageAvailable < 3 * 1024**3:
        raise RuntimeError('Insufficient standard memory reserve before launch')
    stamp = guard.gate.now().replace(':', '').replace('.', '-')
    out = ROOT / 'outputs/lucius-20261110' / (args.mode.lower() + '-' + stamp)
    work = ROOT / 'work/lucius-20261110' / out.name
    out.mkdir(parents=True, exist_ok=True)
    work.mkdir(parents=True, exist_ok=True)
    gate = guard.gate
    gate.OUT, gate.WORK = out, work
    gate.state = dict(stages=[], scope='Lucius animation and stable sprite roots',
                      testPlatform=args.mode, testFilter=args.filter, audio=False,
                      campaignSceneValidated=False)
    guard.MONITOR = out / 'resource-monitor.json'
    result = out / 'tests.xml'
    try:
        if args.import_reviewed:
            guard.guarded_execute('import-reviewed', ['-nographics', '-quit', '-executeMethod',
                'Darklight.Editor.DarklightLuciusDelivery.ImportNormalAnimations'], timeout=600)
        guard.guarded_execute('integration-play' if args.mode == 'PlayMode' else 'import-reviewed',
            ['-nographics', '-runTests', '-testPlatform', args.mode, '-testFilter', args.filter,
             '-testResults', str(result)], timeout=600, xml=result)
    finally:
        if result.exists():
            tree = ET.parse(result).getroot()
            cases = list(tree.iter('test-case'))
            gate.state['testResult'] = dict(result=tree.get('result'), cases=len(cases),
                passed=sum(c.get('result') == 'Passed' for c in cases),
                failed=[dict(name=c.get('fullname'), message=c.findtext('failure/message'))
                        for c in cases if c.get('result') == 'Failed'])
        (out / 'receipt.json').write_text(json.dumps(gate.state, indent=2) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
