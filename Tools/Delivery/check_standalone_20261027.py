"""Launch the delivered Player with isolated saves; record actual storage preservation."""
import hashlib
import json
import os
import argparse
from pathlib import Path
import subprocess
import time
import winreg

ROOT = Path(__file__).resolve().parents[2]
parser=argparse.ArgumentParser()
parser.add_argument("--auto-run",action="store_true")
args=parser.parse_args()
OUT = ROOT / "outputs/retomada-20261027" / ("standalone-auto-"+time.strftime("%H%M%S") if args.auto_run else "standalone")
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


before_saves, before_prefs = saves(), preferences()
exe = ROOT / "outputs/retomada-20261027/Windows/DarklightRequiem.exe"
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
timed_out=False
try:
    code = process.wait(timeout=600)
except subprocess.TimeoutExpired:
    timed_out=True
    process.terminate()
    code = process.wait(timeout=30)
after_prefs, after_saves = preferences(), saves()
changed = sorted(k for k in set(before_prefs) | set(after_prefs) if before_prefs.get(k) != after_prefs.get(k))
if changed:
    with winreg.CreateKey(winreg.HKEY_CURRENT_USER, REG) as key:
        for name in changed:
            if not (name.startswith('unity.player_session') or name.startswith('unity_connect.') and 'session' in name):
                continue  # Never overwrite a human preference changed in parallel.
            if name in before_prefs:
                value, kind = before_prefs[name]
                winreg.SetValueEx(key, name, 0, kind, value)
            else:
                winreg.DeleteValue(key, name)
result = {"exitCode": code, "standaloneExecuted": True,
          "timedOut":timed_out,"automatic":args.auto_run,"launchMode":"hidden, no window activation" if args.auto_run else "visible",
          "realSavesUnchanged": before_saves == after_saves,
          "preferencesUnchangedBeforeRestoration": not changed,
          "changedPreferenceNames": changed,
          "preferencesPreservedAfterRestoration": before_prefs == preferences(),
          "physicalControllerTested": False, "fullHumanCampaignTested": False}
(OUT / "storage-preservation.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(result), flush=True)
