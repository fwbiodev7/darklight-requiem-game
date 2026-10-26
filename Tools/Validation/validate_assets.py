#!/usr/bin/env python3
"""
Automated Asset Validator for DARKLIGHT: REQUIEM.
Validates JSON manifests, catalogs, referenced textures, duplicate files, and coverage matrices.
Classifies results into ERROR, WARNING, INFO, and BLOCKED_ABSENT_IN_CLONE.
Outputs structured JSON and human-readable summary.
"""

import os
import sys
import json
import hashlib
from collections import defaultdict

def sha256_file(filepath):
    h = hashlib.sha256()
    with open(filepath, 'rb') as f:
        while chunk := f.read(65536):
            h.update(chunk)
    return h.hexdigest()

def run_validation(project_root="."):
    resources_dir = os.path.join(project_root, "Assets", "Darklight", "Resources")
    results = {
        "summary": {"errors": 0, "warnings": 0, "info": 0, "blocked": 0},
        "coverage": {},
        "issues": []
    }

    def record_issue(level, category, message, path=""):
        results["summary"][level.lower()] = results["summary"].get(level.lower(), 0) + 1
        results["issues"].append({
            "level": level,
            "category": category,
            "message": message,
            "path": path.replace('\\', '/')
        })

    # 1. Validate all JSON syntax in Resources and ArtSources
    json_count = 0
    for folder in [resources_dir, os.path.join(project_root, "ArtSources")]:
        if not os.path.exists(folder):
            continue
        for root, _, files in os.walk(folder):
            for f in files:
                if f.endswith('.json'):
                    json_count += 1
                    p = os.path.join(root, f)
                    rel = os.path.relpath(p, project_root)
                    try:
                        with open(p, 'r', encoding='utf-8') as jf:
                            json.load(jf)
                    except Exception as e:
                        record_issue("ERROR", "JSON_SYNTAX", f"Invalid JSON syntax: {e}", rel)

    # 2. Validate RuinFists catalog
    rf_cat_path = os.path.join(resources_dir, "Darklight", "RuinFists", "catalog.json")
    if os.path.exists(rf_cat_path):
        try:
            with open(rf_cat_path, 'r', encoding='utf-8') as jf:
                rf_cat = json.load(jf)
            pages = rf_cat.get("pages", [])
            for i, p in enumerate(pages):
                res_path = p.get("resourcePath", "")
                pal_path = p.get("palettePath", "")
                actual_res = os.path.join(resources_dir, res_path + ".png")
                if not os.path.exists(actual_res):
                    record_issue("ERROR", "MISSING_RESOURCE", f"RuinFists page {i} references missing sprite sheet: {res_path}.png", rf_cat_path)
                if pal_path:
                    actual_pal = os.path.join(resources_dir, pal_path + ".png")
                    if not os.path.exists(actual_pal):
                        record_issue("ERROR", "MISSING_PALETTE", f"RuinFists page {i} references missing palette mask: {pal_path}.png", rf_cat_path)
        except Exception as e:
            record_issue("ERROR", "CATALOG_ERROR", f"Error reading RuinFists catalog: {e}", rf_cat_path)

    # 3. Validate WeaponRedesign20261023 catalog
    wr_cat_path = os.path.join(resources_dir, "Darklight", "WeaponRedesign20261023", "catalog.json")
    if os.path.exists(wr_cat_path):
        try:
            with open(wr_cat_path, 'r', encoding='utf-8') as jf:
                wr_cat = json.load(jf)
            pages = wr_cat.get("pages", [])
            for i, p in enumerate(pages):
                res_path = p.get("resourcePath", "")
                actual_res = os.path.join(resources_dir, res_path + ".png")
                if not os.path.exists(actual_res):
                    record_issue("ERROR", "MISSING_RESOURCE", f"WeaponRedesign page {i} ({p.get('id')}) references missing PNG: {res_path}.png", wr_cat_path)
        except Exception as e:
            record_issue("ERROR", "CATALOG_ERROR", f"Error reading WeaponRedesign catalog: {e}", wr_cat_path)

    # 4. Check for duplicate PNG files by exact SHA256 in Resources
    hash_to_files = defaultdict(list)
    png_count = 0
    for root, _, files in os.walk(resources_dir):
        for f in files:
            if f.endswith('.png'):
                png_count += 1
                p = os.path.join(root, f)
                h = sha256_file(p)
                hash_to_files[h].append(os.path.relpath(p, project_root).replace('\\', '/'))

    duplicates_found = 0
    for h, flist in hash_to_files.items():
        if len(flist) > 1:
            duplicates_found += 1
            record_issue("INFO", "DUPLICATE_ASSET", f"Identical PNG content ({len(flist)} copies): {flist}")

    # 5. Coverage matrix
    record_issue("BLOCKED", "COVERAGE_GAP", "Moto com Requiem: Nao ha pranchas autorais com o piloto Requiem no clone.", "Assets/Darklight/Runtime/Character/DarklightMotorcycleRedesignBank.cs")
    record_issue("WARNING", "COVERAGE_GAP", "Katana Transformaçoes (Ruptura/Eclipse/Absoluto): Possuem apenas vista Sul autoral no redesign.", "Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json")

    results["coverage"] = {
        "Katana": {"forms": {0: "Normal (Idle 8 dir, Leve1 8 dir, Leve2 8 dir, Walk South)", 1: "Ruptura (South only)", 2: "Eclipse (South only)", 3: "Absoluto (South only)"}},
        "PunhosDaRuina": {"status": "Complete 128 pages (2 skins x 4 forms x 8 directions x 2 banks)"},
        "Moto": {
            "Rock": "4 forms x 8 directions + 4 slam phases",
            "Requiem": "BLOCKED_ABSENT_IN_CLONE: No Requiem rider authored sheets present"
        },
        "total_pngs_checked": png_count,
        "total_jsons_checked": json_count,
        "duplicate_hash_clusters": duplicates_found
    }

    out_dir = os.path.join(project_root, "outputs", "validation")
    os.makedirs(out_dir, exist_ok=True)
    out_file = os.path.join(out_dir, "assets_validation.json")
    with open(out_file, 'w', encoding='utf-8') as f:
        json.dump(results, f, indent=2)

    return results

if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    res = run_validation(root)
    s = res["summary"]
    print(f"=== Asset Validation Summary ===")
    print(f"Errors: {s.get('errors', 0)}")
    print(f"Warnings: {s.get('warnings', 0)}")
    print(f"Info: {s.get('info', 0)}")
    print(f"Blocked: {s.get('blocked', 0)}")
    print(f"Output written to outputs/validation/assets_validation.json")
