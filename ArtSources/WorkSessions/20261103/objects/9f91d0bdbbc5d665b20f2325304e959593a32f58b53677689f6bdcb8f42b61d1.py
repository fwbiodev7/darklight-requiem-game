#!/usr/bin/env python3
"""Build a portable staging-only pack for archived Katana/Fists Normal walks.

Python 3.10+ and Pillow are required. Hashes only the six selected source PNGs;
never scans the full repo/Assets tree, calls Unity/Git, or writes into --root.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import math
import shutil
import sys
from pathlib import Path
from typing import Any

from PIL import Image

DELIVERY = Path("ArtSources/ReviewedDeliveries/FabiusNormal20261103")
PAGES = DELIVERY / "PAGES.json"
CATALOG = Path("Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json")
BANK_CS = Path("Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs")
IMPORTER_CS = Path("Assets/Darklight/Editor/DarklightWeaponRedesignImporter.cs")
FIELDS = ("skin", "weapon", "form", "direction", "action", "attackKey")


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def read_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def context(page: dict[str, Any]) -> tuple[Any, ...]:
    return tuple(page.get(k) for k in FIELDS)


def intersects(a: list[int], b: list[int]) -> bool:
    return a[0] < b[0] + b[2] and a[0] + a[2] > b[0] and a[1] < b[1] + b[3] and a[1] + a[3] > b[1]


def static_sheet_errors(image_path: Path, data: dict[str, Any], source_path: Path) -> list[str]:
    errors: list[str] = []
    if data.get("schemaVersion") != 1 or not data.get("wholeFigures"):
        errors.append("schemaVersion/wholeFigures")
    if data.get("status") not in {"visual-reviewed", "imported", "runtime-verified"}:
        errors.append("status")
    if data.get("image") != image_path.name:
        errors.append("image basename")
    if len(data.get("imageSha256", "")) != 64 or len(data.get("sourceSha256", "")) != 64:
        errors.append("SHA format")
    if not source_path.is_file():
        errors.append("technical sourcePath missing")
    if data.get("sourcePath", "").startswith("ArtSources/") is False or ".." in data.get("sourcePath", ""):
        errors.append("sourcePath contract")
    if data.get("sourceSize") != list(Image.open(source_path).size):
        errors.append("sourceSize does not match technical source PNG")
    native = list(Image.open(image_path).size)
    if data.get("nativeSize") != native:
        errors.append("nativeSize does not match atlas PNG")
    if not data.get("frames"):
        errors.append("frames empty")
        return errors
    seen: set[str] = set()
    rects: list[list[int]] = []
    for frame in data["frames"]:
        fid = frame.get("id", "")
        rect, pivot = frame.get("rect"), frame.get("pivot")
        if not fid.startswith("Redesign_") or fid in seen:
            errors.append(f"frame id invalid/duplicate: {fid}")
        seen.add(fid)
        if not isinstance(rect, list) or len(rect) != 4 or any(not isinstance(v, int) for v in rect):
            errors.append(f"rect malformed: {fid}")
            continue
        x, y, w, h = rect
        if x < 0 or y < 0 or w < 1 or h < 1 or x + w > native[0] or y + h > native[1]:
            errors.append(f"rect out of bounds: {fid}")
        if any(intersects(rect, prior) for prior in rects):
            errors.append(f"rect overlap: {fid}")
        rects.append(rect)
        if not isinstance(pivot, list) or len(pivot) != 2 or any(not isinstance(v, (int, float)) or not math.isfinite(v) or not 0 <= v <= 1 for v in pivot):
            errors.append(f"pivot malformed: {fid}")
    if not isinstance(data.get("pixelsPerUnit"), (int, float)) or not math.isfinite(data["pixelsPerUnit"]) or data["pixelsPerUnit"] <= 0:
        errors.append("pixelsPerUnit")
    return errors


def static_page_errors(page: dict[str, Any], descriptor: dict[str, Any]) -> list[str]:
    e: list[str] = []
    if page.get("skin") not in range(2) or page.get("form") not in range(4) or page.get("weapon") not in range(4) or page.get("direction") not in range(8):
        e.append("skin/weapon/form/direction range")
    if page.get("skin") != 0 or page.get("form") != 0 or page.get("weapon") not in (0, 2) or page.get("action") != "Walk":
        e.append("outside Fabius Normal Katana/Fists Walk scope")
    if page.get("status") not in {"visual-reviewed", "imported", "runtime-verified"} or not page.get("wholeFigures"):
        e.append("status/wholeFigures")
    if not str(page.get("resourcePath", "")).startswith("Darklight/WeaponRedesign20261023/") or ".." in str(page.get("resourcePath", "")):
        e.append("resourcePath")
    if not str(page.get("sourcePath", "")).startswith("ArtSources/") or ".." in str(page.get("sourcePath", "")):
        e.append("sourcePath")
    for field in ("imageSha256", "sourceSha256"):
        value = str(page.get(field, ""))
        if len(value) != 64 or any(c not in "0123456789abcdefABCDEF" for c in value):
            e.append(field)
    for field in ("nativeSize", "sourceSize"):
        value = page.get(field)
        if not isinstance(value, list) or len(value) != 2 or any(not isinstance(v, int) or v <= 0 for v in value):
            e.append(field)
    if not isinstance(page.get("pixelsPerUnit"), (int, float)) or not math.isfinite(page["pixelsPerUnit"]) or page["pixelsPerUnit"] <= 0:
        e.append("pixelsPerUnit")
    names, durations = page.get("spriteNames", []), page.get("durationMs", [])
    if not names or len(names) != len(durations) or any(not str(n).startswith("Redesign_") for n in names) or any(not isinstance(d, int) or d <= 0 for d in durations):
        e.append("spriteNames/durationMs")
    densities = page.get("framePixelsPerUnit", [])
    if densities and (len(densities) != len(names) or any(not isinstance(v, (int, float)) or not math.isfinite(v) or v <= 0 for v in densities)):
        e.append("framePixelsPerUnit")
    if "loop" in descriptor and page.get("loop") != descriptor.get("loop"):
        e.append("loop mismatch")
    if page.get("pixelsPerUnit") != descriptor.get("pixelsPerUnit"):
        e.append("pixelsPerUnit mismatch")
    source_names = page.get("sourceSpriteNames") or []
    if source_names and (len(source_names) != len(names) or len(set(source_names)) != len(source_names) or any(not n for n in source_names)):
        e.append("sourceSpriteNames")
    if page.get("attackKey"):
        e.append("Walk attackKey")
    if page.get("imageSha256") != descriptor.get("imageSha256"):
        e.append("image hash mismatch")
    if any(name not in {x.get("id") for x in descriptor["frames"]} for name in names):
        e.append("spriteNames missing from descriptor")
    return e


def prepare(root: Path, out: Path) -> dict[str, Any]:
    root, out = root.resolve(), out.resolve()
    if not root.is_dir():
        raise ValueError("--root must be a project directory")
    try:
        out.relative_to(root)
    except ValueError:
        pass
    else:
        raise ValueError("--out must be outside project root; project is read-only")
    pages_path, catalog_path = root / PAGES, root / CATALOG
    if not pages_path.is_file() or not catalog_path.is_file():
        raise ValueError("PAGES.json or current catalog missing")
    pages_doc, catalog = read_json(pages_path), read_json(catalog_path)
    if not (root / BANK_CS).is_file() or not (root / IMPORTER_CS).is_file():
        raise ValueError("runtime page bank/importer source missing for static contract check")
    bank_text = (root / BANK_CS).read_text(encoding="utf-8", errors="replace")
    importer_text = (root / IMPORTER_CS).read_text(encoding="utf-8", errors="replace")
    if 'action=="Walk"' not in bank_text or "other.Overlaps(rect)" not in importer_text:
        raise ValueError("C# ValidPage/VerifySheet shape changed; update static checker before preparing package")
    walks = [p for p in pages_doc.get("pages", []) if p.get("action") == "Walk" and p.get("skin") == 0 and p.get("form") == 0 and p.get("weapon") in (0, 2)]
    if len(walks) != 9:
        raise ValueError(f"expected 9 archived Katana/Fists walks, found {len(walks)}")
    exact_contexts = {context(p) for p in catalog.get("pages", [])}
    matches = [p.get("id") for p in walks if context(p) in exact_contexts]
    if matches:
        raise ValueError(f"staged walks unexpectedly exist in current catalog: {matches}")

    source_dir = root / DELIVERY / "staging"
    output_atlases = out / "staging"
    technical_root = out / DELIVERY / "staging" / "technical-source"
    output_atlases.mkdir(parents=True, exist_ok=True)
    technical_root.mkdir(parents=True, exist_ok=True)
    sheets: dict[str, dict[str, Any]] = {}
    for page in walks:
        name = page["resourcePath"].rsplit("/", 1)[1] + ".png"
        if name in sheets:
            continue
        png = source_dir / name
        descriptor_path = png.with_suffix(".json")
        if not png.is_file() or not descriptor_path.is_file():
            raise ValueError(f"archive PNG/descriptor missing for {name}")
        png_bytes = png.read_bytes()
        source_sha = sha(png_bytes)  # the only content hash computed for this sheet
        descriptor = read_json(descriptor_path)
        if descriptor.get("imageSha256", "").lower() != source_sha:
            raise ValueError(f"imageSha256 mismatch in {name}")
        if descriptor.get("nativeSize") != list(Image.open(png).size):
            raise ValueError(f"nativeSize mismatch in {name}")
        source_size_old = descriptor.get("sourceSize")
        original_source_path = str(descriptor.get("sourcePath", ""))
        original_source_sha = str(descriptor.get("sourceSha256", ""))
        original_source_path_exists = bool(original_source_path and (root / original_source_path).is_file())
        directions = sorted(p["direction"] for p in walks if p["resourcePath"].rsplit("/", 1)[1] + ".png" == name)
        technical_name = f"Walk_w{page['weapon']}_d{'-'.join(map(str, directions))}.png"
        technical_rel = (DELIVERY / "staging" / "technical-source" / technical_name).as_posix()
        image_copy = output_atlases / name
        image_copy.parent.mkdir(parents=True, exist_ok=True)
        image_copy.write_bytes(png_bytes)
        technical_copy = out / technical_rel
        technical_copy.parent.mkdir(parents=True, exist_ok=True)
        technical_copy.write_bytes(png_bytes)
        if image_copy.read_bytes() != png_bytes or technical_copy.read_bytes() != png_bytes:
            raise ValueError(f"byte copy mismatch for {name}")

        derived = json.loads(json.dumps(descriptor))
        derived["image"] = name
        derived["imageSha256"] = source_sha
        derived["sourcePath"] = technical_rel
        derived["sourceSha256"] = source_sha
        derived["sourceSize"] = list(Image.open(technical_copy).size)
        derived["provenance"] = {
            **derived.get("provenance", {}),
            "originalAuthoredSource": {
                "path": original_source_path,
                "sha256": original_source_sha,
                "size": source_size_old,
                "rawSourceAvailable": False,
                "rawSourcePathAvailable": original_source_path_exists,
                "rawSourceContentExactMatchInReviewedAtlas": original_source_sha.lower() == source_sha,
                "note": "Original authored-source provenance. Technical source is the immutable reviewed atlas copy; no raw-source recovery is claimed.",
            },
            "derivedFromReviewedAtlas": {"path": (DELIVERY / "staging" / name).as_posix(), "sha256": source_sha, "size": derived["nativeSize"]},
        }
        errors = static_sheet_errors(image_copy, derived, technical_copy)
        if errors:
            raise ValueError(f"static VerifySheet contract failed for {name}: {errors}")
        if any(intersects(a["rect"], b["rect"]) for i, a in enumerate(derived["frames"]) for b in derived["frames"][:i]):
            # No overlaps expected in this archive subset. Stop clearly rather than silently repacking.
            raise ValueError(f"overlap requires explicitly reviewed mechanical repack: {name}")
        sidecar = output_atlases / (Path(name).stem + ".json")
        write_json(sidecar, derived)
        sheets[name] = {"png": png, "png_bytes": png_bytes, "hash": source_sha, "descriptor": derived,
                        "descriptor_path": sidecar, "technical_path": technical_rel,
                        "historical_source_path": original_source_path, "historical_source_sha256": original_source_sha,
                        "historical_source_size": source_size_old, "historical_raw_path_exists": original_source_path_exists}

    sheet_names = set(sheets)
    page_out = []
    context_rows = []
    preserve_fields = ("id", "skin", "weapon", "form", "direction", "action", "attackKey", "spriteNames", "sourceSpriteNames", "durationMs", "framePixelsPerUnit", "pixelsPerUnit", "startupFrames", "activeFrames", "loop", "wholeFigures")
    for original in walks:
        name = original["resourcePath"].rsplit("/", 1)[1] + ".png"
        info = sheets[name]
        page = json.loads(json.dumps(original))
        page["sourcePath"] = info["technical_path"]
        page["sourceSha256"] = info["hash"]
        page["sourceSize"] = info["descriptor"]["sourceSize"]
        page["nativeSize"] = info["descriptor"]["nativeSize"]
        page["imageSha256"] = info["hash"]
        page["provenance"] = {"originalAuthoredSource": info["descriptor"]["provenance"]["originalAuthoredSource"],
                              "derivedFromReviewedAtlas": info["descriptor"]["provenance"]["derivedFromReviewedAtlas"]}
        if any(page.get(k) != original.get(k) for k in preserve_fields):
            raise ValueError(f"Walk ID/timing/PPU/status changed: {original['id']}")
        if not (out / page["sourcePath"]).is_file():
            raise ValueError(f"PAGES technical sourcePath missing from portable package: {page['sourcePath']}")
        errors = static_page_errors(page, info["descriptor"])
        if errors:
            raise ValueError(f"static ValidPage contract failed for {page['id']}: {errors}")
        page_out.append(page)
        context_rows.append({"id": page["id"], "weapon": page["weapon"], "direction": page["direction"], "action": page["action"],
                            "exact_context_in_current_catalog": False, "atlas": name})
    frame_count = sum(len(page.get("spriteNames", [])) for page in page_out)
    if len(sheets) != 6 or len(page_out) != 9 or frame_count != 36:
        raise ValueError(f"unexpected package totals: {len(sheets)} sheets/{len(page_out)} pages/{frame_count} frames")
    all_atlas_frame_ids = {frame["id"] for item in sheets.values() for frame in item["descriptor"]["frames"]}
    pages_frame_ids = {name for page in page_out for name in page["spriteNames"]}
    if all_atlas_frame_ids != pages_frame_ids:
        raise ValueError("PAGES/frame identity coverage mismatch")

    pages_out_doc = {k: v for k, v in pages_doc.items() if k != "pages"}
    pages_out_doc.update({"complete": False, "pages": page_out,
                          "scopeNote": "Katana and Fists Normal Walks only; reviewed archived source pixels; staging-only, not imported/runtime approved."})
    write_json(out / "PAGES.json", pages_out_doc)
    readme = """# Portable reviewed Walks: Katana + Fists\n\nSix reviewed archive PNGs, nine Normal clips, 36 frames. This bundle contains Katana directions NE/E/SE and Fists directions N/NW/E/NE/SE/W. Moto Walk is explicitly excluded: the lateral V3 is approved and the six remaining directions are under another agent's correction.\n\nAll six archived atlas PNGs are copied byte-for-byte into `staging/` and as technical source copies under `ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/technical-source/`. Descriptors and PAGES bind to those technical copies. The old authored-source path/hash/dimensions remain in `provenance.originalAuthoredSource` with `rawSourceAvailable:false`; there is no claim that an absent raw generator source was recovered. Four source hashes equal the reviewed atlas hash; two differ.\n\nRectangles are in bounds, non-overlapping, and frame crops reproduce exactly because the atlas bytes are unchanged. Sprite IDs, pivots, PPU/frame PPU, durations, loop, and all other page timing fields are preserved. The static Unity `VerifySheet`/`ValidPage` shape checks passed. `PAGES.complete` remains false; no Unity, Assets write, runtime test, or gameplay approval was performed.\n\nRerun from anywhere with Python 3.10+ and Pillow 10+: `python prepare_reviewed_walks.py --root <checkout> --out .`. Output must be outside the checkout. The routine hashes only the six selected atlas source PNGs and writes only this package.\n"""
    (out / "README.md").write_text(readme, encoding="utf-8")
    copy_script = out / "prepare_reviewed_walks.py"
    here = Path(__file__).resolve()
    if here != copy_script.resolve():
        shutil.copy2(here, copy_script)

    geometry = []
    file_map = []
    for name, info in sorted(sheets.items()):
        desc = info["descriptor"]
        src = Image.open(info["png"]).convert("RGBA")
        out_im = Image.open(output_atlases / name).convert("RGBA")
        frame_items = []
        for frame in desc["frames"]:
            x, y, w, h = frame["rect"]
            a = src.crop((x, y, x + w, y + h)).tobytes()
            b = out_im.crop((x, y, x + w, y + h)).tobytes()
            if a != b:
                raise ValueError(f"RGBA frame reconstruction mismatch {name}/{frame['id']}")
            frame_items.append({"id": frame["id"], "rect": frame["rect"], "pivot": frame["pivot"], "rgba_exact": True})
        geometry.append({"sheet": name, "source_sha256": info["hash"], "atlas_copy_byte_exact": True,
                         "technical_source_copy_byte_exact": True, "nativeSize": desc["nativeSize"],
                         "sourceSize": desc["sourceSize"], "frame_count": len(frame_items), "frames": frame_items,
                         "old_source": {"path": info["historical_source_path"], "sha256": info["historical_source_sha256"],
                                        "size": info["historical_source_size"], "rawSourceAvailable": False,
                                        "rawSourcePathAvailable": info["historical_raw_path_exists"],
                                        "rawSourceContentExactMatchInReviewedAtlas": info["historical_source_sha256"].lower() == info["hash"]}})
        file_map.append({"atlas": f"staging/{name}", "descriptor": f"staging/{Path(name).stem}.json",
                         "technicalSource": info["technical_path"], "sourceSha256": info["hash"],
                         "copyByteExact": True})
    audit = {
        "status": "READY_FOR_COORDINATOR for this Katana/Fists Walk staging pack only",
        "scope": {"skin": 0, "form": 0, "weapons": [0, 2], "actions": ["Walk"], "moto_walk_included": False},
        "catalogComparison": {"currentCatalogPages": len(catalog.get("pages", [])), "complete": catalog.get("complete"),
                              "walk_contexts": 9, "exact_context_matches": 0, "contexts": context_rows},
        "totals": {"sheets": 6, "clips": 9, "frames": 36},
        "sha256_policy": "computed only the six selected archived atlas PNG hashes; descriptor/PAGES/catalog/full Assets hashes were not recomputed",
        "static_contracts": {"VerifySheet": "passed for six sheets", "ValidPage": "passed for nine Walk pages", "rects_in_bounds": True, "rects_nonoverlapping": True},
        "rgba_proof": {"frames_checked": 36, "frames_exact": 36, "operation": "read-only byte comparison of each original/output RGBA crop"},
        "source_files": geometry,
        "project_mutations": {"Assets": False, "ArtSources": False, "Git": False, "Unity": False},
        "runtimeApproved": False,
    }
    write_json(out / "geometry-audit.json", audit)
    bundle = {"purpose": "portable staging-only reviewed Fabius Normal Katana/Fists Walks", "complete": False,
              "totals": {"sheets": 6, "clips": 9, "frames": 36}, "sheets": file_map,
              "runtimeApproved": False, "gameAssetsPublished": False}
    write_json(out / "manifest.json", bundle)
    return {"out": str(out), "sheets": 6, "clips": 9, "frames": 36, "source_png_hashes_computed": 6,
            "context_matches_in_catalog": 0, "frames_rgba_exact": 36, "overlaps": 0,
            "static_contracts_passed": True, "runtimeApproved": False}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", required=True, type=Path)
    parser.add_argument("--out", required=True, type=Path)
    args = parser.parse_args()
    try:
        result = prepare(args.root, args.out)
    except Exception as exc:
        print(f"ERROR: {type(exc).__name__}: {exc}", file=sys.stderr)
        return 1
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
