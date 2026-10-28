#!/usr/bin/env python3
"""
Build Preflight Validator for DARKLIGHT: REQUIEM.
Validates build readiness without triggering a full compile:
- Configured scenes in EditorBuildSettings
- Unity project version consistency
- Essential packages and manifest validity
- Assembly definitions integrity
Publication privacy and Unity execution are separate checks.
"""

import os
import sys
import json
import re
from pathlib import Path

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

    definitions={};guids={}
    for folder in ("Assets","Packages","Library/PackageCache"):
        for path in Path(project_root,folder).rglob("*.asmdef"):
            try:
                definition=json.loads(path.read_text(encoding="utf-8-sig"));name=definition["name"]
                if name in definitions:record_issue("FAIL",f"Duplicate assembly name: {name}")
                definitions[name]=(path,definition)
                meta=Path(str(path)+".meta")
                if meta.is_file():
                    match=re.search(r"(?m)^guid:\s*(\w+)",meta.read_text(encoding="utf-8-sig"))
                    if match:guids[match[1]]=name
            except (ValueError,KeyError,OSError) as error:record_issue("FAIL",f"Invalid assembly definition {path}: {error}")
    unresolved=[]
    for name,(path,definition) in definitions.items():
        if not path.is_relative_to(Path(project_root,"Assets")):continue
        for reference in definition.get("references",[]):
            resolved=guids.get(reference[5:]) if reference.startswith("GUID:") else reference
            if resolved not in definitions:unresolved.append(dict(assembly=name,reference=reference))
    results["checks"]["project_assembly_count"]=sum(p.is_relative_to(Path(project_root,"Assets")) for p,_ in definitions.values())
    results["checks"]["unresolved_assembly_references"]=unresolved
    for entry in unresolved:record_issue("FAIL",f"Unresolved assembly reference: {entry}")
    settings=Path(project_root,"ProjectSettings/EditorBuildSettings.asset");enabled=[]
    if settings.is_file():
        enabled=re.findall(r"- enabled:\s*1\s+path:\s*([^\r\n]+)",settings.read_text(encoding="utf-8-sig"))
        for scene in enabled:
            if not Path(project_root,scene.strip()).is_file():record_issue("FAIL",f"Enabled scene is missing: {scene}")
    else:record_issue("FAIL","Missing EditorBuildSettings.asset")
    results["checks"]["enabled_scenes"]=enabled
    if "Assets/Darklight/Scenes/DarklightCampaign.unity" not in enabled:record_issue("FAIL","The campaign scene is not enabled in build settings")
    results["limits"]=["Does not compile or execute Unity", "Does not certify gameplay or artistic approval", "Publication privacy and save/build exclusions require the separate reviewed allowlist"]
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
