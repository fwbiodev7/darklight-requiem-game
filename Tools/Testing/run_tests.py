#!/usr/bin/env python3
"""
Unity Test Runner for DARKLIGHT: REQUIEM.
Executes EditMode and PlayMode tests in batchmode with Unity 6000.6.0f1.
Features:
- Unity installation detection (6000.6.0f1)
- Project path validation
- Concurrent execution guard (UnityLockfile detection)
- Output log capturing and XML test results parsing
- Clear distinction between Unity launch failure vs test assertion failure
"""

import os
import sys
import subprocess
import argparse
import xml.etree.ElementTree as ET
import time
import uuid

UNITY_DEFAULT_PATH = r"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe"

def find_unity_binary():
    if os.path.exists(UNITY_DEFAULT_PATH):
        return UNITY_DEFAULT_PATH
    # Check PATH or other standard locations
    custom = os.environ.get("UNITY_PATH")
    if custom and os.path.exists(custom):
        return custom
    return None

def check_concurrent_lock(project_path):
    lockfile = os.path.join(project_path, "Temp", "UnityLockfile")
    return os.path.exists(lockfile)

def run_tests(mode="EditMode", test_filter=None, project_path=".", timeout_sec=300, nographics=False):
    project_abs = os.path.abspath(project_path)
    unity_bin = find_unity_binary()

    result = {
        "mode": mode,
        "unity_found": bool(unity_bin),
        "unity_path": unity_bin,
        "project_path": project_abs,
        "exit_code": -1,
        "status": "UNKNOWN",
        "passed": 0,
        "failed": 0,
        "inconclusive": 0,
        "skipped": 0,
        "log_path": None,
        "xml_path": None
    }

    if not unity_bin:
        result["status"] = "BLOCKED_UNITY_NOT_FOUND"
        print(f"[BLOCKED] Unity 6000.6.0f1 executable not found at expected path: {UNITY_DEFAULT_PATH}")
        return result

    if not os.path.isfile(os.path.join(project_abs,"ProjectSettings","ProjectVersion.txt")) or timeout_sec<=0:
        result["status"]="INVALID_PROJECT_OR_TIMEOUT"
        return result

    if check_concurrent_lock(project_abs):
        result["status"] = "BLOCKED_CONCURRENT_INSTANCE"
        print(f"[BLOCKED] Project lockfile detected at {project_abs}/Temp/UnityLockfile. A Unity instance is already running!")
        return result

    out_dir = os.path.join(project_abs, "outputs", "test-artifacts", "runner",time.strftime("%Y%m%dT%H%M%S")+"-"+uuid.uuid4().hex)
    os.makedirs(out_dir)
    log_file = os.path.join(out_dir, f"unity_{mode.lower()}.log")
    xml_file = os.path.join(out_dir, f"results_{mode.lower()}.xml")

    result["log_path"] = log_file
    result["xml_path"] = xml_file

    cmd = [
        unity_bin,
        "-batchmode",
        "-darklightSilent",
        "-projectPath", project_abs,
        "-runTests",
        "-testPlatform", mode,
        "-testResults", xml_file,
        "-logFile", log_file
    ]
    if nographics:
        cmd.append("-nographics")

    if test_filter:
        cmd.extend(["-testFilter", test_filter])

    print(f"Executing {mode} tests with Unity {os.path.basename(os.path.dirname(os.path.dirname(unity_bin)))}...")
    print(f"Log: {log_file}")
    print(f"Results: {xml_file}")

    try:
        began_ns=time.time_ns()
        proc = subprocess.run(cmd, timeout=timeout_sec)
        result["exit_code"] = proc.returncode

        # Parse NUnit XML results
        if os.path.exists(xml_file):
            try:
                tree = ET.parse(xml_file)
                root = tree.getroot()
                cases=list(root.iter("test-case"))
                result["executed"]=sum(c.get("result") in ("Passed","Failed") for c in cases)
                result["passed"] = int(root.attrib.get("passed", 0))
                result["failed"] = int(root.attrib.get("failed", 0))
                result["inconclusive"] = int(root.attrib.get("inconclusive", 0))
                result["skipped"] = int(root.attrib.get("skipped", 0))

                if proc.returncode!=0:
                    result["status"]="UNITY_EXIT_FAILURE"
                elif root.tag != "test-run":
                    result["status"]="XML_SCHEMA_ERROR"
                # The path is new for this run; allow filesystem timestamp granularity.
                elif os.stat(xml_file).st_mtime_ns<began_ns-2_000_000_000:
                    result["status"]="STALE_XML"
                elif not result["executed"]:
                    result["status"]="NO_EXECUTED_TESTS"
                elif result["failed"] > 0 or any(c.get("result")=="Failed" for c in cases):
                    result["status"]="TESTS_FAILED"
                elif result["passed"] != sum(c.get("result")=="Passed" for c in cases) or result["failed"] != sum(c.get("result")=="Failed" for c in cases):
                    result["status"]="XML_INCONSISTENT"
                elif root.get("result") not in ("Passed","Success"):
                    result["status"] = "TESTS_FAILED"
                elif any(c.get("result")!="Passed" for c in cases):
                    result["status"]="TESTS_INCOMPLETE"
                else:
                    result["status"] = "PASSED"
            except Exception as e:
                result["status"] = "XML_PARSE_ERROR"
                result["error"]=str(e)
        else:
            if proc.returncode == 0:
                result["status"] = "COMPLETED_WITHOUT_XML"
            else:
                result["status"] = "LAUNCH_OR_CRASH_FAILURE"

    except subprocess.TimeoutExpired:
        result["status"] = "TIMEOUT"
        result["exit_code"] = -2
        print(f"[TIMEOUT] Tests exceeded {timeout_sec} seconds threshold.")
    except OSError as error:
        result["status"]="LAUNCH_OR_CRASH_FAILURE"
        result["error"]=str(error)

    return result

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="DARKLIGHT: REQUIEM Unity Test Runner")
    parser.add_argument("--mode", choices=["EditMode", "PlayMode"], default="EditMode", help="Test platform mode")
    parser.add_argument("--filter", default=None, help="NUnit test filter pattern")
    parser.add_argument("--project", default=".", help="Project path")
    parser.add_argument("--timeout", type=int, default=180, help="Timeout in seconds")
    parser.add_argument("--nographics",action="store_true",help="Opt in only for tests that do not need rendering/audio")
    args = parser.parse_args()

    res = run_tests(args.mode, args.filter, args.project, args.timeout,args.nographics)
    print(f"\n=== Run Summary ===")
    print(f"Status: {res['status']}")
    print(f"Exit Code: {res['exit_code']}")
    print(f"Passed: {res['passed']}, Failed: {res['failed']}, Skipped: {res['skipped']}")
    sys.exit(0 if res['status'] == "PASSED" else 1)
