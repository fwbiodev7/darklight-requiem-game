"""Focused import, representative-map captures and fast-particle validation."""
from pathlib import Path
import sys
import finalize_20261027 as gate

ROOT=Path(__file__).resolve().parents[2]
gate.OUT=ROOT/'outputs/retomada-20261101/atmosphere'
gate.WORK=ROOT/'work/retomada-20261101/atmosphere-focus'


def main():
    gate.OUT.mkdir(parents=True,exist_ok=True);gate.WORK.mkdir(parents=True,exist_ok=True)
    if (ROOT/'Temp/UnityLockfile').exists():raise RuntimeError('Preserve the active Unity session')
    gate.execute('import',['-quit','-executeMethod','Darklight.Editor.DarklightScope26Delivery.Import'])
    names=['PixelAtmospherePreservesGeometryAndCharacterAndCapturesGameplay','WeaponDodgeParticlesAreDistinctAndExpireQuickly']
    gate.execute('play',['-runTests','-testPlatform','PlayMode','-testFilter',';'.join('Darklight.Tests.DarklightWeaponRedesignGameplayTests.'+n for n in names),
                        '-testResults',str(gate.OUT/'focused-play.xml')],xml=gate.OUT/'focused-play.xml')


if __name__=='__main__':
    try:main()
    except Exception as error:
        gate.state['error']=str(error);gate.save();print(str(error),file=sys.stderr);raise SystemExit(1)
