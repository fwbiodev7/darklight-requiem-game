"""Retry the validated build with low CPU priority and a system memory/disk guard."""
import ctypes
from ctypes import wintypes
import json,runpy,shutil,subprocess,time
from pathlib import Path
import finalize_20261027 as gate

ROOT=Path(__file__).resolve().parents[2]
MONITOR=ROOT/'outputs/retomada-20261101/build-resource-monitor.json'

class Memory(ctypes.Structure):
    _fields_=[('length',wintypes.DWORD),('load',wintypes.DWORD)]+[
        (key,ctypes.c_ulonglong) for key in ('total','available','pageTotal','pageAvailable','virtualTotal','virtualAvailable','extended')]

def memory():
    value=Memory();value.length=ctypes.sizeof(value)
    if not ctypes.windll.kernel32.GlobalMemoryStatusEx(ctypes.byref(value)):
        raise OSError('Unable to read memory pressure')
    return value

def guarded_execute(name,args,timeout=900,xml=None):
    assert name=='build-headless' and xml is None
    row=dict(stage='build-guarded',started=gate.now(),completed=False)
    gate.state['stages'].append(row);gate.save()
    log=gate.WORK/'build-guarded.log'
    command=[str(gate.UNITY),'-batchmode','-darklightSilent','-job-worker-count','2',
             '-projectPath',str(ROOT),'-logFile',str(log),*args]
    state=dict(started=gate.now(),priority='BelowNormal',jobWorkers=2,
               minimumAvailableRAMBytes=int(2.5*1024**3),minimumAvailableCommitBytes=3*1024**3,
               minimumFreeDiskBytes=1024**3,minimumObservedAvailableRAMBytes=memory().available)
    reason=None
    with (gate.WORK/'guarded-stdout.log').open('wb') as stream:
        process=subprocess.Popen(command,cwd=ROOT,stdout=stream,stderr=subprocess.STDOUT,
                                 creationflags=subprocess.BELOW_NORMAL_PRIORITY_CLASS)
        state['unityPid']=process.pid
        deadline=time.monotonic()+timeout;last_saved=0
        while process.poll() is None:
            value=memory();free=shutil.disk_usage(ROOT).free
            state.update(availableRAMBytes=value.available,availableCommitBytes=value.pageAvailable,freeDiskBytes=free)
            state['minimumObservedAvailableRAMBytes']=min(state['minimumObservedAvailableRAMBytes'],value.available)
            if value.available<state['minimumAvailableRAMBytes']:reason='Stopped before system RAM exhaustion'
            elif value.pageAvailable<state['minimumAvailableCommitBytes']:reason='Stopped before system commit exhaustion'
            elif free<state['minimumFreeDiskBytes']:reason='Stopped before disk exhaustion'
            elif time.monotonic()>deadline:reason='Build exceeded time limit'
            if reason:
                subprocess.run(['taskkill','/PID',str(process.pid),'/T','/F'],stdout=stream,stderr=stream)
                process.wait(timeout=30);break
            if time.monotonic()-last_saved>5:
                MONITOR.write_text(json.dumps(state,indent=2)+'\n');last_saved=time.monotonic()
            time.sleep(.5)
        code=process.returncode
    row.update(exitCode=code,ended=gate.now(),log=str(log),stopReason=reason)
    state.update(exitCode=code,ended=gate.now(),stopReason=reason)
    MONITOR.write_text(json.dumps(state,indent=2)+'\n')
    row['completed']=code==0 and reason is None;gate.save()
    print(json.dumps(row),flush=True)
    if not row['completed']:raise RuntimeError(reason or 'Native build failed; inspect '+str(log))

gate.execute=guarded_execute
runpy.run_path(str(Path(__file__).with_name('retry_windows_build_20261101.py')),run_name='__main__')
