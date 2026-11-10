"""Run affected isolated checks through the existing resource guard.

Defaults to EditMode; --mode PlayMode uses the isolated silent fixture explicitly
authorized by the user on Nov 9. No campaign, build or audio playback. Each run
has separate receipts and leaves historical evidence untouched.
"""
import argparse
import json
import subprocess
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "Tools/Delivery"))
import run_guarded_build_20261101 as guard


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--mode',choices=['EditMode','PlayMode'],default='EditMode')
    parser.add_argument('--filter')
    parser.add_argument('--headless-play',action='store_true',help='No graphics; exclude camera/render tests explicitly')
    parser.add_argument('--standard-ram-guard',action='store_true',help='Use normal import/build memory reserves')
    parser.add_argument('--import-lucius-idle',action='store_true',help='Import only the reviewed initial Lucius sheet before its EditMode tests')
    args=parser.parse_args()
    lucius_filters=set(args.filter.split(';')) if args.filter else set()
    if args.import_lucius_idle and (not args.standard_ram_guard or args.mode!='EditMode' or
            'Darklight.Tests.DarklightLuciusSpriteBankTests' not in lucius_filters or
            not lucius_filters<={'Darklight.Tests.DarklightLuciusSpriteBankTests','Darklight.Tests.DarklightLuciusCombatProfileTests'}):
        raise ValueError('Lucius initial import requires its isolated EditMode filter and standard guards')
    if args.mode=='PlayMode' and args.headless_play and (not args.filter or
            any(part.endswith('.November') or 'GuardMesh' in part for part in args.filter.split(';'))):
        raise ValueError('Headless Play requires an explicit filter excluding camera/render tests')
    if (ROOT / "Temp/UnityLockfile").exists():
        raise RuntimeError("Project already locked by Unity")
    running = subprocess.run(
        ["tasklist", "/FI", "IMAGENAME eq Unity.exe", "/FO", "CSV", "/NH"],
        capture_output=True, text=True, check=True,
    )
    if '"Unity.exe"' in running.stdout:
        raise RuntimeError("Another Unity session is active")
    ram_floor=2.5 if args.standard_ram_guard else 1
    commit_floor=3 if args.standard_ram_guard else 2
    if guard.memory().available < int(ram_floor*1024**3):
        raise RuntimeError("Insufficient physical reserve before launch")
    if guard.memory().pageAvailable < commit_floor*1024**3:
        raise RuntimeError("Insufficient commit reserve before launch")
    stamp=guard.gate.now().replace(":", "").replace(".", "-")
    out = ROOT / "outputs/retomada-20261109/resume" / (args.mode.lower()+"-"+stamp)
    work = ROOT / "work/retomada-20261109" / out.name
    out.mkdir(parents=True, exist_ok=True)
    work.mkdir(parents=True, exist_ok=True)
    gate = guard.gate
    gate.OUT, gate.WORK = out, work
    gate.state = dict(stages=[], scope="isolated Fabius tests", playMode=args.mode=='PlayMode', audio=False,
        authorization="User 2026-11-09: Autorizar testes isolados e mudos",campaignSceneValidated=False)
    guard.MONITOR = out / "editmode-resource-monitor.json"
    result = out / "tests.xml"
    filters = args.filter or ("Darklight.Tests.DarklightNovemberIsolatedGameplayTests.November" if args.mode=='PlayMode' else ";".join([
        "Darklight.Tests.DarklightCombatTests",
        "Darklight.Tests.DarklightWeaponTechniqueTests",
        "Darklight.Tests.DarklightFabiusRecoveryTests",
        "Darklight.Tests.DarklightMotorcycleRedesignTests",
    ]))
    gate.state.update(testFilter=filters,testPlatform=args.mode,graphicsRequested=args.mode=='PlayMode' and not args.headless_play)
    if args.import_lucius_idle:gate.state.update(scope='Lucius first static kit only',character='Lucius',campaignSceneValidated=False)
    try:
        if args.import_lucius_idle:
            gate.WORK=work/'import';gate.WORK.mkdir(exist_ok=True)
            guard.MONITOR=out/'lucius-import-resource-monitor.json'
            guard.guarded_execute('import-reviewed',['-nographics','-quit','-executeMethod',
                'Darklight.Editor.DarklightLuciusDelivery.ImportInitialIdle'],timeout=600)
            gate.WORK=work/'edit';gate.WORK.mkdir(exist_ok=True)
            guard.MONITOR=out/'editmode-resource-monitor.json'
        guard.guarded_execute("integration-play" if args.mode=='PlayMode' else "import-reviewed", [
            "-force-d3d11" if args.mode=='PlayMode' and not args.headless_play else "-nographics", "-runTests", "-testPlatform", args.mode,
            "-testFilter", filters, "-testResults", str(result),
        ], timeout=600, xml=result,fabius_test_reserve_gib=None if args.standard_ram_guard else 1)
    finally:
        if result.is_file():
            tree=ET.parse(result).getroot();cases=list(tree.iter('test-case'))
            gate.state['testResult']=dict(xml=str(result),result=tree.get('result'),cases=len(cases),
                passed=sum(c.get('result')=='Passed' for c in cases),
                skipped=sum(c.get('result')=='Skipped' for c in cases),
                inconclusive=sum(c.get('result')=='Inconclusive' for c in cases),
                failed=[dict(name=c.get('fullname'),message=c.findtext('failure/message'))
                    for c in cases if c.get('result')=='Failed'])
        (out / "receipt.json").write_text(
            json.dumps(gate.state, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
