#!/usr/bin/env python3
"""
Unity Metadata and Dependency Auditor for DARKLIGHT: REQUIEM.
Audits:
- Missing .meta files for assets and directories in Assets/
- Duplicate GUIDs across all .meta files
- Case-insensitive filename collisions
- Broken YAML GUID references in scenes and prefabs
Outputs structured JSON and human-readable summary.
"""

import os
import sys
import json
import re
from collections import defaultdict

GUID_PATTERN = re.compile(r'guid:\s*([0-9a-fA-F]{32})')

def audit_metadata(project_root="."):
    assets_dir = os.path.join(project_root, "Assets")
    results = {
        "summary": {"errors": 0, "warnings": 0, "info": 0},
        "missing_meta": [],
        "duplicate_guids": [],
        "case_collisions": [],
        "broken_yaml_references": [],
        "total_assets_checked": 0,
        "total_metas_checked": 0
    }

    def record_issue(level, category, message, path=""):
        counter={"ERROR":"errors","WARNING":"warnings","INFO":"info"}[level]
        results["summary"][counter]+=1
        results.setdefault("issues",[]).append(dict(level=level,category=category,message=message,path=path))

    if not os.path.isdir(assets_dir):
        record_issue("ERROR","MISSING_ASSETS","Assets directory does not exist")

    # 1. Check for missing .meta files and collect all GUIDs
    guid_to_path = {}
    duplicate_guids = defaultdict(list)
    case_map = defaultdict(list)
    total_assets = 0
    total_metas = 0

    for root, dirs, files in os.walk(assets_dir):
        # Case collisions in dirs
        for d in dirs:
            norm_d = d.lower()
            case_map[(root.lower(), norm_d)].append(os.path.join(root, d))
            meta_path = os.path.join(root, d + ".meta")
            if not os.path.exists(meta_path):
                rel = os.path.relpath(os.path.join(root, d), project_root).replace('\\', '/')
                results["missing_meta"].append(rel)
                record_issue("ERROR", "MISSING_META", f"Missing directory .meta file: {rel}")

        for f in files:
            p = os.path.join(root, f)
            rel = os.path.relpath(p, project_root).replace('\\', '/')
            if f.endswith('.meta'):
                total_metas += 1
                # Read guid
                try:
                    with open(p, 'r', encoding='utf-8', errors='ignore') as mf:
                        for line in mf:
                            if line.startswith('guid:'):
                                g = line.split(':')[1].strip()
                                if g in guid_to_path:
                                    duplicate_guids[g].append(rel)
                                    if guid_to_path[g] not in duplicate_guids[g]:
                                        duplicate_guids[g].insert(0, guid_to_path[g])
                                else:
                                    guid_to_path[g] = rel
                                break
                except Exception as e:
                    record_issue("ERROR", "CORRUPT_META", f"Could not read meta: {e}", rel)
            else:
                total_assets += 1
                norm_f = f.lower()
                case_map[(root.lower(), norm_f)].append(rel)
                meta_path = p + ".meta"
                if not os.path.exists(meta_path):
                    results["missing_meta"].append(rel)
                    record_issue("ERROR", "MISSING_META", f"Missing asset .meta file: {rel}")

    # Check case collisions
    for (r, name), paths in case_map.items():
        if len(paths) > 1 and len(set(paths)) > 1:
            results["case_collisions"].append(paths)
            record_issue("ERROR", "CASE_COLLISION", f"Case collision between: {paths}")

    # Record duplicate GUIDs
    for g, paths in duplicate_guids.items():
        results["duplicate_guids"].append({"guid": g, "files": paths})
        record_issue("ERROR", "DUPLICATE_GUID", f"Duplicate GUID {g} found in {paths}")

    # 2. Check broken YAML references in Scenes and Prefabs
    yaml_files = []
    for root, _, files in os.walk(assets_dir):
        for f in files:
            if f.endswith('.unity') or f.endswith('.prefab'):
                yaml_files.append(os.path.join(root, f))

    broken_refs_count = 0
    # Standard Unity builtin package GUIDs that are not in local Assets
    builtin_guid_prefixes = set()

    for yf in yaml_files:
        rel = os.path.relpath(yf, project_root).replace('\\', '/')
        try:
            with open(yf, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            matches = set(GUID_PATTERN.findall(content))
            for g in matches:
                if g not in guid_to_path and not g.startswith("0000000000000000"):
                    # Note: could be Unity builtin, record as warning if unresolvable
                    results["broken_yaml_references"].append({
                        "file": rel,
                        "guid": g
                    })
                    broken_refs_count += 1
                    record_issue("WARNING","UNRESOLVED_YAML_GUID","Reference is not in local Assets; may belong to a package",rel)
        except Exception as e:
            record_issue("WARNING", "YAML_READ_ERROR", f"Could not parse YAML {rel}: {e}")

    results["total_assets_checked"] = total_assets
    results["total_metas_checked"] = total_metas
    results["broken_yaml_references_count"] = broken_refs_count

    out_dir = os.path.join(project_root, "outputs", "validation")
    os.makedirs(out_dir, exist_ok=True)
    out_file = os.path.join(out_dir, "metadata_audit.json")
    with open(out_file, 'w', encoding='utf-8') as f:
        json.dump(results, f, indent=2)

    return results

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    res = audit_metadata(root)
    s = res["summary"]
    print(f"=== Metadata Audit Summary ===")
    print(f"Total Assets: {res['total_assets_checked']}, Metas: {res['total_metas_checked']}")
    print(f"Missing Metas: {len(res['missing_meta'])}")
    print(f"Duplicate GUIDs: {len(res['duplicate_guids'])}")
    print(f"Case Collisions: {len(res['case_collisions'])}")
    print(f"Broken YAML Refs: {res['broken_yaml_references_count']}")
    print(f"Output written to outputs/validation/metadata_audit.json")
    sys.exit(1 if s["errors"] else 0)
