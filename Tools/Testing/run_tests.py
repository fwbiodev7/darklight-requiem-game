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

def run_tests(mode="EditMode", test_filter=None, project_path=".", timeout_sec=300):
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

    if check_concurrent_lock(project_abs):
        result["status"] = "BLOCKED_CONCURRENT_INSTANCE"
        print(f"[BLOCKED] Project lockfile detected at {project_abs}/Temp/UnityLockfile. A Unity instance is already running!")
        return result

    out_dir = os.path.join(project_abs, "outputs", "test-artifacts", "runner")
    os.makedirs(out_dir, exist_ok=True)
    log_file = os.path.join(out_dir, f"unity_{mode.lower()}.log")
    xml_file = os.path.join(out_dir, f"results_{mode.lower()}.xml")

    result["log_path"] = log_file
    result["xml_path"] = xml_file

    cmd = [
        unity_bin,
        "-batchmode",
        "-nographics",
        "-projectPath", project_abs,
        "-runTests",
        "-testPlatform", mode,
        "-testResults", xml_file,
        "-logFile", log_file
    ]

    if test_filter:
        cmd.extend(["-testFilter", test_filter])

    print(f"Executing {mode} tests with Unity {os.path.basename(os.path.dirname(os.path.dirname(unity_bin)))}...")
    print(f"Log: {log_file}")
    print(f"Results: {xml_file}")

    try:
        proc = subprocess.run(cmd, timeout=timeout_sec)
        result["exit_code"] = proc.returncode

        # Parse NUnit XML results
        if os.path.exists(xml_file):
            try:
                tree = ET.parse(xml_file)
                root = tree.getroot()
                result["passed"] = int(root.attrib.get("passed", 0))
                result["failed"] = int(root.attrib.get("failed", 0))
                result["inconclusive"] = int(root.attrib.get("inconclusive", 0))
                result["skipped"] = int(root.attrib.get("skipped", 0))

                if result["failed"] > 0:
                    result["status"] = "TESTS_FAILED"
                else:
                    result["status"] = "PASSED"
            except Exception as e:
                result["status"] = "XML_PARSE_ERROR"
        else:
            if proc.returncode == 0:
                result["status"] = "COMPLETED_WITHOUT_XML"
            else:
                result["status"] = "LAUNCH_OR_CRASH_FAILURE"

    except subprocess.TimeoutExpired:
        result["status"] = "TIMEOUT"
        result["exit_code"] = -2
        print(f"[TIMEOUT] Tests exceeded {timeout_sec} seconds threshold.")

    return result

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="DARKLIGHT: REQUIEM Unity Test Runner")
    parser.add_argument("--mode", choices=["EditMode", "PlayMode"], default="EditMode", help="Test platform mode")
    parser.add_argument("--filter", default=None, help="NUnit test filter pattern")
    parser.add_argument("--project", default=".", help="Project path")
    parser.add_argument("--timeout", type=int, default=180, help="Timeout in seconds")
    args = parser.parse_args()

    res = run_tests(args.mode, args.filter, args.project, args.timeout)
    print(f"\n=== Run Summary ===")
    print(f"Status: {res['status']}")
    print(f"Exit Code: {res['exit_code']}")
    print(f"Passed: {res['passed']}, Failed: {res['failed']}, Skipped: {res['skipped']}")
    sys.exit(0 if res['status'] == "PASSED" or res['failed'] == 0 and res['exit_code'] == 0 else 1)
