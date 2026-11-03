"""Retry the validated build with low CPU priority and a system memory/disk guard."""
import ctypes
from ctypes import wintypes
import json,os,runpy,shutil,subprocess,time
import xml.etree.ElementTree as ET
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

def guarded_execute(name,args,timeout=900,xml=None,isolated_ram_reserve_gib=None):
    assert name in ('build-headless','import-reviewed','integration-play')
    reserve=2.5
    if isolated_ram_reserve_gib is not None:
        # User authorized using additional RAM on Nov 3; keep this exception
        # confined to isolated tests, never a campaign run, import or build.
        index=args.index('-testFilter') if '-testFilter' in args else -1
        if (name!='integration-play' or index<0 or index+1>=len(args) or
                args[index+1]!='Darklight.Tests.DarklightNovemberIsolatedGameplayTests.November' or
                not 1.5<=isolated_ram_reserve_gib<=2.5):
            raise ValueError('RAM exception is restricted to isolated November tests with at least 1.5 GiB reserve')
        reserve=isolated_ram_reserve_gib
    row=dict(stage='build-guarded' if name=='build-headless' else name,started=gate.now(),completed=False)
    gate.state['stages'].append(row);gate.save()
    log=gate.WORK/('build-guarded.log' if name=='build-headless' else name+'.log')
    command=[str(gate.UNITY),'-batchmode','-darklightSilent','-job-worker-count','1',
             '-projectPath',str(ROOT),'-logFile',str(log),*args]
    # Scope the supported Bee concurrency control to this child process only.
    # Unity release notes UUM-90426: avoids concurrent compiler jobs exhausting commit.
    child_env=os.environ.copy();child_env['BEE_BUILD_THREADS']='1'
    state=dict(started=gate.now(),priority='BelowNormal',jobWorkers=1,beeBuildThreads=1,
               minimumAvailableRAMBytes=int(reserve*1024**3),minimumAvailableCommitBytes=3*1024**3,
               minimumFreeDiskBytes=1024**3,minimumObservedAvailableRAMBytes=memory().available)
    if isolated_ram_reserve_gib is not None:
        state['ramReserveAuthorization']='User 2026-11-03: pode utilizar quanto de ram precisar; isolated tests only'
    reason=None
    with (gate.WORK/'guarded-stdout.log').open('wb') as stream:
        process=subprocess.Popen(command,cwd=ROOT,stdout=stream,stderr=subprocess.STDOUT,
                                 creationflags=subprocess.BELOW_NORMAL_PRIORITY_CLASS,env=child_env)
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
    row['completed']=code==0 and reason is None
    if row['completed'] and xml is not None:
        result=ET.parse(xml).getroot();cases=list(result.iter('test-case'))
        row.update(cases=len(cases),passed=sum(c.get('result')=='Passed' for c in cases))
        row['completed']=result.get('result')=='Passed' and bool(cases) and row['passed']==len(cases)
        if not row['completed']:reason='Focused integration test failed';row['stopReason']=reason
    gate.save()
    print(json.dumps(row),flush=True)
    if not row['completed']:raise RuntimeError(reason or 'Unity operation failed; inspect '+str(log))

if __name__=='__main__':
    gate.execute=guarded_execute
    runpy.run_path(str(Path(__file__).with_name('retry_windows_build_20261101.py')),run_name='__main__')
