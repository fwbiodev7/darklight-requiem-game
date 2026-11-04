"""One owned Unity process, original memory floors, no build or gameplay run."""
import ctypes
import argparse
from ctypes import wintypes
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import shutil
import subprocess
import time

BASE=Path(__file__).resolve().parents[1]
ROOT=BASE/'outputs/darklight-requiem-game'
OUT=BASE/'outputs/fechamento-fabius-20261103/unity'
UNITY=Path('C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe')
class Memory(ctypes.Structure):
    _fields_=[('length',wintypes.DWORD),('load',wintypes.DWORD)]+[(k,ctypes.c_ulonglong) for k in ('total','available','pageTotal','pageAvailable','virtualTotal','virtualAvailable','extended')]
def memory():
    m=Memory();m.length=ctypes.sizeof(m)
    if not ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(m)):raise OSError('Memory measurement failed')
    return {'availableRAMBytes':m.available,'availableCommitBytes':m.pageAvailable,'freeDiskBytes':shutil.disk_usage(ROOT).free}
def now():return datetime.now(timezone.utc).isoformat()
def main():
    global OUT
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,default=OUT)
    parser.add_argument('--request',type=Path)
    parser.add_argument('--timeout',type=int,default=900)
    args=parser.parse_args();OUT=args.out.resolve()
    request=(args.request or OUT/'request.json').resolve()
    if not OUT.is_relative_to((BASE/'outputs/fechamento-fabius-20261103/unity').resolve()):raise ValueError('Unexpected receipt directory')
    if (OUT/'resource-monitor.json').exists():raise ValueError('Refuse to overwrite an earlier run')
    OUT.mkdir(parents=True,exist_ok=True)
    if (ROOT/'Temp/UnityLockfile').exists():raise RuntimeError('Unity project already locked')
    control=json.loads((BASE/'outputs/fechamento-fabius-20261103/CONTROLE-LOCAL.json').read_text(encoding='utf-8'))
    if control['integrationOwner']!='Codex /root' or control['state']!='INTEGRATING_ESSENTIALS_LOCAL':raise RuntimeError('Coordinator ownership gate not released for this run')
    selected=json.loads(request.read_text(encoding='utf-8'))
    allowed={p.replace('\\','/') for p in control['allowedAssetTargets']}
    if not selected.get('paths') or any(p.replace('\\','/') not in allowed for p in selected['paths']):raise ValueError('Request outside approved target allowlist')
    if Path(selected['receipt']).resolve()!=OUT/'import-receipt.json':raise ValueError('Request receipt must match this run')
    state={'started':now(),'completed':False,'runtimeVerified':False,'floors':{'availableRAMBytes':int(2.5*1024**3),'availableCommitBytes':3*1024**3,'freeDiskBytes':1024**3},'samples':[]}
    first=memory();state['samples'].append(first)
    if any(first[k]<v for k,v in state['floors'].items()):raise RuntimeError('Insufficient resources before starting Unity')
    command=[str(UNITY),'-batchmode','-nographics','-darklightSilent','-job-worker-count','1','-projectPath',str(ROOT),'-logFile',str(OUT/'import.log'),'-quit','-executeMethod','Darklight.Editor.DarklightFabiusBoundedImport.Run','-darklightFabiusRequest',str(request)]
    env=dict(os.environ,BEE_BUILD_THREADS='1')
    reason=None
    with (OUT/'stdout.log').open('wb') as log:
        proc=subprocess.Popen(command,cwd=ROOT,env=env,stdout=log,stderr=subprocess.STDOUT,creationflags=subprocess.BELOW_NORMAL_PRIORITY_CLASS|subprocess.CREATE_NO_WINDOW)
        state.update(pid=proc.pid,command=command)
        deadline=time.monotonic()+args.timeout;last=0
        while proc.poll() is None:
            row=memory();state['latest']=row
            if time.monotonic()-last>3:
                state['samples'].append(row);last=time.monotonic()
                (OUT/'resource-monitor.json').write_text(json.dumps(state,indent=2),encoding='utf-8')
            for key,floor in state['floors'].items():
                if row[key]<floor:reason='Stopped before resource exhaustion: '+key;break
            if time.monotonic()>deadline:reason='Unity import exceeded bounded time limit'
            if reason:
                subprocess.run(['taskkill','/PID',str(proc.pid),'/T','/F'],stdout=log,stderr=log)
                proc.wait(timeout=30);break
            time.sleep(.5)
        state.update(ended=now(),exitCode=proc.returncode,stopReason=reason)
    receipt=OUT/'import-receipt.json'
    state['completed']=not reason and proc.returncode==0 and receipt.exists() and json.loads(receipt.read_text()).get('completed') is True
    (OUT/'resource-monitor.json').write_text(json.dumps(state,indent=2),encoding='utf-8')
    print(json.dumps({k:state[k] for k in ('completed','exitCode','stopReason','runtimeVerified')}))
    return 0 if state['completed'] else 1
if __name__=='__main__':raise SystemExit(main())
