import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
UNITY = Path(r"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe")

def run(task_name, unity_args, timeout=600):
    work = ROOT / "work"
    work.mkdir(parents=True, exist_ok=True)
    log = work / f"{task_name}.log"
    cmd = [
        str(UNITY),
        "-batchmode",
        "-projectPath", str(ROOT),
        "-logFile", str(log),
        *unity_args
    ]
    print(f"Starting {task_name}...")
    start_time = time.time()
    proc = subprocess.Popen(cmd, cwd=ROOT, stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
    try:
        proc.wait(timeout=timeout)
    except subprocess.TimeoutExpired:
        print(f"Task {task_name} timed out after {timeout}s! Killing process {proc.pid}...")
        proc.kill()
        proc.wait()
        return 1
    
    elapsed = time.time() - start_time
    code = proc.returncode
    print(f"Task {task_name} finished in {elapsed:.1f}s with code {code}.")
    if log.exists():
        lines = log.read_text(encoding="utf-8", errors="ignore").splitlines()
        print(f"Log tail ({len(lines)} total lines):")
        for line in lines[-25:]:
            print("  ", line)
    return code

if __name__ == "__main__":
    task = sys.argv[1] if len(sys.argv) > 1 else "import-animations"
    args = sys.argv[2:] if len(sys.argv) > 2 else ["-quit", "-executeMethod", "Darklight.Editor.DarklightLuciusDelivery.ImportNormalAnimations"]
    code = run(task, args)
    sys.exit(code)

