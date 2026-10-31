"""Launch the delivered Player with isolated saves; record actual storage preservation."""
import hashlib
import json
import os
import argparse
import csv
import io
import uuid
from pathlib import Path
import subprocess
import time
import winreg
from player_qa_preferences import DELETE, restoration_plan

ROOT = Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser()
parser.add_argument("--auto-run",action="store_true")
parser.add_argument("--output",type=Path)
parser.add_argument("--executable",type=Path)
args=parser.parse_args()
OUT = args.output or ROOT / "outputs/retomada-20261027" / ("standalone-auto-"+time.strftime("%H%M%S") if args.auto_run else "standalone")
OUT.mkdir(parents=True, exist_ok=True)
REG = r"Software\Fabio\DARKLIGHT REQUIEM"
SAVE = Path(os.environ["USERPROFILE"]) / "AppData/LocalLow/Fabio/DARKLIGHT REQUIEM"


def preferences():
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REG) as key:
            values = {}
            for i in range(winreg.QueryInfoKey(key)[1]):
                name, value, kind = winreg.EnumValue(key, i)
                values[name] = (value, kind)
            return values
    except FileNotFoundError:
        return {}


def saves():
    return {str(p.relative_to(SAVE)): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in SAVE.glob("*.json*") if p.is_file()} if SAVE.exists() else {}


def player_pids():
    result=subprocess.run(["tasklist","/FI","IMAGENAME eq DarklightRequiem.exe","/FO","CSV","/NH"],capture_output=True,text=True,check=True)
    return {int(row[1]) for row in csv.reader(io.StringIO(result.stdout))
            if len(row)>1 and row[0].lower()=="darklightrequiem.exe" and row[1].isdigit()}


if player_pids():
    raise RuntimeError("A DARKLIGHT Player is already open; preserve the human session")
before_saves, before_prefs = saves(), preferences()
private=ROOT/"work/qa-storage"/uuid.uuid4().hex
private.mkdir(parents=True)
snapshot={k:{"kind":kind,"value":value.hex() if isinstance(value,bytes) else value,"hex":isinstance(value,bytes)} for k,(value,kind) in before_prefs.items()}
(private/"preferences-before.json").write_text(json.dumps(snapshot),encoding="utf-8")
exe = args.executable or ROOT / "outputs/retomada-20261027/Windows/DarklightRequiem.exe"
command = [str(exe), "-darklightSilent", "-screen-fullscreen", "0", "-screen-width", "1280",
           "-screen-height", "720", "-logFile", str(OUT / "player.log"),
           "--darklight-isolated-session", str(OUT / "saves"),
           "--requiem-hud-performance-capture", str(OUT)]
startup=None
if args.auto_run:
    (OUT/"run.request").write_text("Explicit automatic Player validation; no UI input automation",encoding="utf-8")
    startup=subprocess.STARTUPINFO()
    startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow=0
process = subprocess.Popen(command, cwd=exe.parent,startupinfo=startup)
(OUT / "launch.json").write_text(json.dumps({"pid": process.pid, "started": time.time(),
    "realSaveHashesBefore": before_saves, "preferenceValueCountBefore": len(before_prefs),
    "isolatedSaves": True, "silent": True}, indent=2), encoding="utf-8")
print(json.dumps({"pid": process.pid, "output": str(OUT)}), flush=True)
timed_out=False;concurrent_player=False
try:
    deadline=time.monotonic()+600
    while process.poll() is None:
        concurrent_player |= bool(player_pids()-{process.pid})
        if time.monotonic()>deadline:raise subprocess.TimeoutExpired(command,600)
        time.sleep(2)
    code=process.returncode
except subprocess.TimeoutExpired:
    timed_out=True
    process.terminate()
    code = process.wait(timeout=30)
after_prefs, after_saves = preferences(), saves()
changed = sorted(k for k in set(before_prefs) | set(after_prefs) if before_prefs.get(k) != after_prefs.get(k))
current=preferences()
operations,skipped=restoration_plan(before_prefs,after_prefs,current,concurrent_player or bool(player_pids()))
if operations:
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, REG) as key:
        for name,original in operations.items():
            if original is not DELETE:
                value, kind = original
                winreg.SetValueEx(key, name, 0, kind, value)
            else:
                winreg.DeleteValue(key, name)
result = {"exitCode": code, "standaloneExecuted": True,
          "timedOut":timed_out,"automatic":args.auto_run,"launchMode":"hidden, no window activation" if args.auto_run else "visible",
          "realSavesUnchanged": before_saves == after_saves,
          "preferencesUnchangedBeforeRestoration": not changed,
          "changedPreferenceNames": changed,
          "skippedConcurrentOrHumanChanges":skipped,"parallelPlayerDetected":concurrent_player,
          "preferencesPreservedAfterRestoration": before_prefs == preferences(),
          "physicalControllerTested": False, "fullHumanCampaignTested": False}
(OUT / "storage-preservation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(result), flush=True)
raise SystemExit(0 if code==0 and not timed_out and result["realSavesUnchanged"] and result["preferencesPreservedAfterRestoration"] else 2)
