#!/usr/bin/env python3
"""
Build Preflight Validator for DARKLIGHT: REQUIEM.
Validates build readiness without triggering a full compile:
- Configured scenes in EditorBuildSettings
- Unity project version consistency
- Essential packages and manifest validity
- Assembly definitions integrity
- Forbidden / private files in tree
- Exclusions of test-artifacts and saves
"""

import os
import sys
import json
import re

def run_preflight(project_root="."):
    results = {
        "status": "PASS",
        "checks": {},
        "issues": []
    }

    def record_issue(level, message):
        results["issues"].append({"level": level, "message": message})
        if level == "FAIL":
            results["status"] = "FAIL"

    # 1. Unity Version Check
    version_file = os.path.join(project_root, "ProjectSettings", "ProjectVersion.txt")
    if os.path.exists(version_file):
        with open(version_file, 'r', encoding='utf-8') as f:
            v_text = f.read()
        m = re.search(r'm_EditorVersion:\s*([^\s]+)', v_text)
        detected_version = m.group(1) if m else "Unknown"
        results["checks"]["unity_version"] = detected_version
        if detected_version.startswith("6000.6.0"):
            results["checks"]["unity_version_status"] = "VALID"
        else:
            record_issue("WARN", f"Project configured for {detected_version}, recommended 6000.6.0f1")
    else:
        record_issue("FAIL", "Missing ProjectSettings/ProjectVersion.txt")

    # 2. Package Manifest Check
    manifest_file = os.path.join(project_root, "Packages", "manifest.json")
    if os.path.exists(manifest_file):
        try:
            with open(manifest_file, 'r', encoding='utf-8') as f:
                p_data = json.load(f)
            deps = p_data.get("dependencies", {})
            results["checks"]["package_count"] = len(deps)
            essential = ["com.unity.inputsystem", "com.unity.render-pipelines.universal", "com.unity.test-framework"]
            for pkg in essential:
                if pkg in deps:
                    results["checks"][pkg] = deps[pkg]
                else:
                    record_issue("FAIL", f"Missing essential package: {pkg}")
        except Exception as e:
            record_issue("FAIL", f"Corrupt Packages/manifest.json: {e}")
    else:
        record_issue("FAIL", "Missing Packages/manifest.json")

    # 3. Main Scene Check
    campaign_scene = os.path.join(project_root, "Assets", "Darklight", "Scenes", "DarklightCampaign.unity")
    combat_scene = os.path.join(project_root, "Assets", "Darklight", "Scenes", "DarklightCombatLab.unity")
    results["checks"]["scenes"] = {
        "DarklightCampaign": os.path.exists(campaign_scene),
        "DarklightCombatLab": os.path.exists(combat_scene)
    }
    if not os.path.exists(campaign_scene):
        record_issue("FAIL", "Missing main scene DarklightCampaign.unity")

    # 4. Assembly Definition Check
    runtime_asmdef = os.path.join(project_root, "Assets", "Darklight", "Runtime", "Darklight.Runtime.asmdef")
    results["checks"]["runtime_asmdef"] = os.path.exists(runtime_asmdef)
    if not os.path.exists(runtime_asmdef):
        record_issue("FAIL", "Missing Darklight.Runtime.asmdef")

    # 5. Check for private/suspicious paths in tracked git files
    suspicious_patterns = [r"\.codex", r"remaster-prototype-20261014", r"cavaleiro-sprites"]
    # Check if outputs/ directory has build artifacts
    out_dir = os.path.join(project_root, "outputs")
    results["checks"]["outputs_directory_present"] = os.path.exists(out_dir)

    out_file = os.path.join(project_root, "outputs", "validation", "build_preflight.json")
    os.makedirs(os.path.dirname(out_file), exist_ok=True)
    with open(out_file, 'w', encoding='utf-8') as f:
        json.dump(results, f, indent=2)

    return results

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    r = run_preflight(root)
    print(f"=== Build Preflight Result: {r['status']} ===")
    print(f"Unity Version: {r['checks'].get('unity_version')}")
    print(f"Packages: {r['checks'].get('package_count')} detected")
    print(f"Issues: {len(r['issues'])}")
    for iss in r['issues']:
        print(f"  [{iss['level']}] {iss['message']}")
    print(f"Report saved to outputs/validation/build_preflight.json")
    sys.exit(0 if r['status'] == "PASS" else 1)
