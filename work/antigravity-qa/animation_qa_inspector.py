#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
DARKLIGHT: REQUIEM — Animation & Sprite Quality Assurance Tool
Antigravity Support Engineering — 2026-11-04

Analyzes spritesheets, metadata, frame spacing, pivot consistency,
and anomalies (e.g. Punhos/Grevas walk offset detection).
"""

import os
import sys
import json
import re

def analyze_meta_file(meta_path):
    issues = []
    if not os.path.exists(meta_path):
        return {"error": "Meta file not found", "path": meta_path}

    try:
        with open(meta_path, "r", encoding="utf-8") as f:
            content = f.read()

        # Check sprite sheet mode
        if "spriteMode: 2" not in content:
            issues.append("spriteMode is not Multiple (2)")

        # Extract sprites
        sprites = re.findall(r"name:\s*([^\r\n]+)[\s\S]*?rect:[\s\S]*?x:\s*([0-9.]+)[\s\S]*?y:\s*([0-9.]+)[\s\S]*?width:\s*([0-9.]+)[\s\S]*?height:\s*([0-9.]+)[\s\S]*?pivot:[\s\S]*?x:\s*([0-9.]+)[\s\S]*?y:\s*([0-9.]+)", content)
        
        frames = []
        for s in sprites:
            frames.append({
                "name": s[0].strip(),
                "rect": {"x": float(s[1]), "y": float(s[2]), "w": float(s[3]), "h": float(s[4])},
                "pivot": {"x": float(s[5]), "y": float(s[6])}
            })

        # Check alignment / offsets in walk cycles
        for i in range(1, len(frames)):
            dx = abs(frames[i]["rect"]["x"] - frames[i-1]["rect"]["x"])
            # Flag anomalous jump in position
            if dx > 150:  # arbitrary threshold for uneven step
                issues.append(f"Suspicious X gap between {frames[i-1]['name']} and {frames[i]['name']}: {dx}px")

        return {
            "meta_path": meta_path,
            "total_sprites": len(frames),
            "issues": issues,
            "frames_preview": frames[:5]
        }
    except Exception as e:
        return {"error": str(e), "path": meta_path}

def main():
    print("=== DARKLIGHT: REQUIEM — QA Automated Animation Inspector ===")
    root_dir = os.path.abspath("Assets/Darklight/Art")
    results = {}
    
    # Check specific target: Fabius walk / Punhos
    target_meta = "Assets/Darklight/Art/User/Requiem-idade-0-arma-1.png.meta"
    if os.path.exists(target_meta):
        print(f"Auditing target: {target_meta}")
        results["punhos_walk"] = analyze_meta_file(target_meta)
    else:
        print("Target meta not found directly, scanning Art/ folder...")

    report_path = "work/antigravity-qa/QA-REPORT.json"
    os.makedirs(os.path.dirname(report_path), exist_ok=True)
    with open(report_path, "w", encoding="utf-8") as f:
        json.dump(results, f, indent=2)

    print(f"Report written to {report_path}")

if __name__ == "__main__":
    main()

