"""Run only affected EditMode checks through the existing resource guard.

No PlayMode, scene launch, build or audio. Historical receipts are untouched.
"""
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "Tools/Delivery"))
import run_guarded_build_20261101 as guard


def main():
    if (ROOT / "Temp/UnityLockfile").exists():
        raise RuntimeError("Project already locked by Unity")
    running = subprocess.run(
        ["tasklist", "/FI", "IMAGENAME eq Unity.exe", "/FO", "CSV", "/NH"],
        capture_output=True, text=True, check=True,
    )
    if '"Unity.exe"' in running.stdout:
        raise RuntimeError("Another Unity session is active")
    if guard.memory().available < int(2.5 * 1024**3):
        raise RuntimeError("Less than 2.5 GiB physical reserve before launch")
    if guard.memory().pageAvailable < 3 * 1024**3:
        raise RuntimeError("Less than 3 GiB commit reserve before launch")
    out = ROOT / "outputs/retomada-20261109"
    work = ROOT / "work/retomada-20261109/editmode"
    out.mkdir(parents=True, exist_ok=True)
    work.mkdir(parents=True, exist_ok=True)
    gate = guard.gate
    gate.OUT, gate.WORK = out, work
    gate.state = dict(stages=[], scope="affected EditMode only", playMode=False, audio=False)
    guard.MONITOR = out / "editmode-resource-monitor.json"
    result = out / ("focused-edit-" + gate.now().replace(":", "").replace(".", "-") + ".xml")
    filters = ";".join([
        "Darklight.Tests.DarklightCombatTests",
        "Darklight.Tests.DarklightWeaponRedesignTests",
        "Darklight.Tests.DarklightWeaponTechniqueTests",
    ])
    try:
        guard.guarded_execute("import-reviewed", [
            "-nographics", "-runTests", "-testPlatform", "EditMode",
            "-testFilter", filters, "-testResults", str(result),
        ], timeout=600, xml=result)
    finally:
        (out / "editmode-receipt.json").write_text(
            json.dumps(gate.state, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
