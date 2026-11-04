#!/usr/bin/env python3
"""Prepare a portable, read-only-derived Fabius Normal essentials staging pack.

Requires Python 3.10+ and Pillow. Reads --root and writes only --out; --out must
be outside the project root. Never invokes Unity, Git, or project writers.
"""
from __future__ import annotations

import argparse
import collections
import hashlib
import json
import math
import shutil
import sys
from pathlib import Path
from typing import Any

from PIL import Image, ImageChops


DELIVERY = Path("ArtSources/ReviewedDeliveries/FabiusNormal20261103")
STAGING = DELIVERY / "staging"
MANIFEST = DELIVERY / "manifest.json"
PAGES = DELIVERY / "PAGES.json"
CATALOG = Path("Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json")
BANK_CS = Path("Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs")
IMPORTER_CS = Path("Assets/Darklight/Editor/DarklightWeaponRedesignImporter.cs")
ESSENTIAL_ACTIONS = {"Parry", "Hurt", "Dead"}
ACTION_KEYS = ("skin", "weapon", "form", "direction", "action", "attackKey")
EXPECTED_OVERLAP_SHEETS = {
    "ReqFistsNormalHurtN_NE_E_SE20261103V1.png",
    "ReqKatanaNormalHurtN_NE_E_SE20261103V1.png",
    "ReqMotoNormalHurtN_NE_E_SE20261103V1.png",
}
TECH_SOURCE_DIR = DELIVERY / "staging" / "technical-source"


def digest_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def digest_file(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        for block in iter(lambda: f.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest()


def load_json(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8-sig"))


def dump_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def relpath(root: Path, path: Path) -> str:
    return path.relative_to(root).as_posix()


def ctx(page: dict[str, Any]) -> tuple[Any, ...]:
    return tuple(page.get(k) for k in ACTION_KEYS)


def rect_overlap(a: list[int], b: list[int]) -> bool:
    return a[0] < b[0] + b[2] and a[0] + a[2] > b[0] and a[1] < b[1] + b[3] and a[1] + a[3] > b[1]


def validate_sheet(path: Path, data: dict[str, Any]) -> list[str]:
    errors: list[str] = []
    if data.get("schemaVersion") != 1 or not data.get("wholeFigures"):
        errors.append("schemaVersion/wholeFigures inválido")
    if data.get("status") not in {"visual-reviewed", "imported", "runtime-verified"}:
        errors.append("status não aceito pelo importer")
    if data.get("image") != path.name:
        errors.append("image não corresponde ao basename da PNG")
    for key in ("imageSha256", "sourceSha256"):
        if len(str(data.get(key, ""))) != 64 or any(c not in "0123456789abcdefABCDEF" for c in str(data.get(key, ""))):
            errors.append(f"{key} inválido")
    size = data.get("nativeSize")
    if not isinstance(size, list) or len(size) != 2 or any(not isinstance(x, int) or x <= 0 for x in size):
        errors.append("nativeSize inválido")
        return errors
    if not isinstance(data.get("sourceSize"), list) or len(data["sourceSize"]) != 2 or any(not isinstance(x, int) or x <= 0 for x in data["sourceSize"]):
        errors.append("sourceSize inválido")
    if not isinstance(data.get("pixelsPerUnit"), (int, float)) or not math.isfinite(data["pixelsPerUnit"]) or data["pixelsPerUnit"] <= 0:
        errors.append("pixelsPerUnit inválido")
    if not isinstance(data.get("frames"), list) or not data["frames"]:
        errors.append("frames ausentes")
        return errors
    ids: set[str] = set()
    rects: list[list[int]] = []
    for frame in data["frames"]:
        fid = str(frame.get("id", ""))
        rect = frame.get("rect")
        pivot = frame.get("pivot")
        if not fid.startswith("Redesign_") or fid in ids:
            errors.append(f"id frame inválido/duplicado: {fid}")
        ids.add(fid)
        if not isinstance(rect, list) or len(rect) != 4 or any(not isinstance(v, int) for v in rect):
            errors.append(f"rect inválido: {fid}")
            continue
        x, y, w, h = rect
        if x < 0 or y < 0 or w < 1 or h < 1 or x + w > size[0] or y + h > size[1]:
            errors.append(f"rect fora dos limites: {fid}")
        if any(rect_overlap(rect, other) for other in rects):
            errors.append(f"rect sobreposto: {fid}")
        rects.append(rect)
        if not isinstance(pivot, list) or len(pivot) != 2 or any(not isinstance(v, (int, float)) or not math.isfinite(v) or not 0 <= v <= 1 for v in pivot):
            errors.append(f"pivot inválido: {fid}")
    return errors


def validate_page_static(page: dict[str, Any]) -> list[str]:
    """Mirror the runtime ValidPage checks for this TimedStateAction subset."""
    errors: list[str] = []
    if not page.get("id") or page.get("skin") not in range(2) or page.get("weapon") not in range(4):
        errors.append("id/skin/weapon inválidos")
    if page.get("form") not in range(4) or page.get("direction") not in range(8):
        errors.append("form/direction inválidos")
    if page.get("action") not in {"Parry", "Hurt", "Dead"}:
        errors.append("ação fora do subconjunto TimedStateAction")
    if page.get("status") not in {"visual-reviewed", "imported", "runtime-verified"} or not page.get("wholeFigures"):
        errors.append("status/wholeFigures inválidos")
    if not str(page.get("resourcePath", "")).startswith("Darklight/WeaponRedesign20261023/") or ".." in str(page.get("resourcePath", "")) or str(page.get("resourcePath", "")).endswith(".png"):
        errors.append("resourcePath inválido para o banco")
    if not str(page.get("sourcePath", "")).startswith("ArtSources/") or ".." in str(page.get("sourcePath", "")):
        errors.append("sourcePath inválido para o importer")
    for field in ("imageSha256", "sourceSha256"):
        v = str(page.get(field, ""))
        if len(v) != 64 or any(c not in "0123456789abcdefABCDEF" for c in v):
            errors.append(f"{field} inválido")
    for field in ("nativeSize", "sourceSize"):
        v = page.get(field)
        if not isinstance(v, list) or len(v) != 2 or any(not isinstance(n, int) or n <= 0 for n in v):
            errors.append(f"{field} inválido")
    if not isinstance(page.get("pixelsPerUnit"), (int, float)) or not math.isfinite(page["pixelsPerUnit"]) or page["pixelsPerUnit"] <= 0:
        errors.append("pixelsPerUnit inválido")
    names = page.get("spriteNames")
    durations = page.get("durationMs")
    if not isinstance(names, list) or not names or any(not str(n).startswith("Redesign_") for n in names):
        errors.append("spriteNames inválidos")
    if not isinstance(durations, list) or len(durations) != len(names or []) or any(not isinstance(n, int) or n <= 0 for n in durations):
        errors.append("durationMs não corresponde aos frames")
    densities = page.get("framePixelsPerUnit")
    if densities and (len(densities) != len(names or []) or any(not isinstance(n, (int, float)) or not math.isfinite(n) or n <= 0 for n in densities)):
        errors.append("framePixelsPerUnit inválido")
    if page.get("attackKey"):
        errors.append("TimedStateAction não pode carregar attackKey")
    if page.get("loop"):
        errors.append("TimedStateAction não pode fazer loop")
    return errors


def repack_rows(original: Image.Image, descriptor: dict[str, Any], gutter: int = 2, columns: int = 4) -> tuple[Image.Image, list[list[int]], dict[str, Any]]:
    """Keep original canvas unchanged and duplicate exact full frame crops below it."""
    frames = descriptor["frames"]
    rows = [frames[i:i + columns] for i in range(0, len(frames), columns)]
    row_widths = [sum(f["rect"][2] for f in row) + gutter * (len(row) + 1) for row in rows]
    row_heights = [max(f["rect"][3] for f in row) + gutter for row in rows]
    width = max(original.width, max(row_widths, default=original.width))
    height = original.height + gutter + sum(row_heights) + gutter
    atlas = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    # Preserve every source RGBA pixel in the untouched original-canvas region.
    atlas.paste(original, (0, 0))
    placements: list[list[int] | None] = [None] * len(frames)
    y = original.height + gutter
    index = 0
    for row, row_h in zip(rows, row_heights):
        x = gutter
        for frame in row:
            rx, ry, rw, rh = frame["rect"]
            crop = original.crop((rx, ry, rx + rw, ry + rh))
            atlas.paste(crop, (x, y))  # no alpha mask/compositing: RGBA bytes are copied verbatim
            placements[index] = [x, y, rw, rh]
            x += rw + gutter
            index += 1
        y += row_h + gutter
    assert all(p is not None for p in placements)
    proof = {"original_region_xywh": [0, 0, original.width, original.height], "copy_rows": len(rows), "columns_max": columns, "gutter_px": gutter}
    return atlas, [p for p in placements if p is not None], proof


def run(root: Path, out: Path) -> dict[str, Any]:
    root = root.resolve()
    out = out.resolve()
    if not root.is_dir():
        raise ValueError(f"project root inexistente: {root}")
    try:
        out.relative_to(root)
    except ValueError:
        pass
    else:
        raise ValueError("--out deve ficar fora do project root; a rotina nunca escreve no projeto")
    out.mkdir(parents=True, exist_ok=True)

    source_manifest_path = root / MANIFEST
    source_pages_path = root / PAGES
    source_catalog_path = root / CATALOG
    bank_cs_path = root / BANK_CS
    importer_cs_path = root / IMPORTER_CS
    for p in (source_manifest_path, source_pages_path, source_catalog_path, bank_cs_path, importer_cs_path):
        if not p.is_file():
            raise ValueError(f"arquivo requerido ausente: {p.relative_to(root).as_posix()}")
    bank_text = bank_cs_path.read_text(encoding="utf-8", errors="replace")
    importer_text = importer_cs_path.read_text(encoding="utf-8", errors="replace")
    if "static bool ValidPage" not in bank_text or "other.Overlaps(rect)" not in importer_text:
        raise ValueError("contrato C# mudou; revisão estática precisa ser atualizada antes de preparar pacote")

    manifest = load_json(source_manifest_path)
    pages_doc = load_json(source_pages_path)
    original_pages = pages_doc.get("pages", [])
    all_png_rows = {Path(row["path"]).name: row for row in manifest.get("files", []) if row.get("path", "").lower().endswith(".png")}
    essential_pages = [p for p in original_pages if "/essential-states/" in str(p.get("sourcePath", ""))]
    if pages_doc.get("complete") is not False or len(essential_pages) != 72 or any(p.get("action") not in ESSENTIAL_ACTIONS for p in essential_pages):
        raise ValueError(f"subconjunto essencial inesperado: complete={pages_doc.get('complete')}, clips={len(essential_pages)}")
    if {Path(p["sourcePath"]).name.replace("-source.png", ".png") for p in essential_pages} - set(all_png_rows):
        raise ValueError("PAGES essential sheet sem correspondência de PNG no manifesto")

    # Snapshot protected input hashes before writing any portable output.
    sheet_source_infos: list[dict[str, Any]] = []
    selected_png_names: set[str] = set()
    for page in essential_pages:
        desc_name = Path(page["sourcePath"]).name.replace("-source.png", ".png")
        selected_png_names.add(desc_name)
    if len(selected_png_names) != 16:
        raise ValueError(f"esperava 16 folhas essenciais, encontrei {len(selected_png_names)}")
    protected_paths = [source_manifest_path, source_pages_path, source_catalog_path, bank_cs_path, importer_cs_path]
    input_by_name: dict[str, dict[str, Any]] = {}
    for name in sorted(selected_png_names):
        row = all_png_rows.get(name)
        if not row:
            raise ValueError(f"PNG essencial fora do manifesto: {name}")
        image_path = root / row["path"]
        descriptor_path = image_path.with_suffix(".json")
        if not image_path.is_file() or not descriptor_path.is_file():
            raise ValueError(f"PNG/descriptor ausente: {row['path']}")
        original_hash = digest_file(image_path)
        if original_hash != row.get("sha256"):
            raise ValueError(f"PNG hash não confere com manifesto: {row['path']}")
        descriptor = load_json(descriptor_path)
        if descriptor.get("imageSha256", "").lower() != original_hash or descriptor.get("sourceSha256", "").lower() != original_hash:
            raise ValueError(f"essencial não possui sourceSha=imageSha original, interrupção segura: {name}")
        original_size = list(Image.open(image_path).size)
        if descriptor.get("nativeSize") != original_size or descriptor.get("sourceSize") != original_size:
            raise ValueError(f"tamanho nativo/source da folha essencial diverge: {name}")
        raw_path = str(descriptor.get("sourcePath", ""))
        raw_abs = root / raw_path
        raw_path_exists = raw_abs.is_file()
        # The original path is absent, but an exact sourceSha == reviewed imageSha
        # means the same bytes survive in the immutable reviewed atlas.
        raw_match_content = original_hash == str(descriptor.get("sourceSha256", "")).lower()
        # Keep the portable Windows path under MAX_PATH; the directory itself
        # identifies the file as the technical source copy.
        technical_source_path = (TECH_SOURCE_DIR / image_path.name).as_posix()
        sheet_source_infos.append({
            "name": name, "image_path": row["path"], "descriptor_path": row["path"].removesuffix(".png") + ".json",
            "image_abs": image_path, "descriptor_abs": descriptor_path, "descriptor": descriptor,
            "old_image_sha256": original_hash, "old_size": original_size,
            "historical_source_path": raw_path, "historical_source_sha256": descriptor["sourceSha256"],
            "historical_source_size": descriptor.get("sourceSize"), "historical_source_path_exists": raw_path_exists,
            "historical_raw_content_exact_path_match": bool(raw_match_content),
            "technical_source_path": technical_source_path,
        })
        protected_paths.extend((image_path, descriptor_path))
    before_hashes = {relpath(root, p): digest_file(p) for p in protected_paths}

    overlaps_by_sheet: dict[str, list[tuple[int, int, list[int], list[int]]]] = {}
    for info in sheet_source_infos:
        d = info["descriptor"]
        rects = []
        overlaps = []
        for i, frame in enumerate(d["frames"]):
            r = frame["rect"]
            if any(rect_overlap(r, prior) for prior in rects):
                for j, other in enumerate(d["frames"][:i]):
                    if rect_overlap(r, other["rect"]):
                        overlaps.append((j, i, other["rect"], r))
            rects.append(r)
        if overlaps:
            overlaps_by_sheet[info["name"]] = overlaps
    if set(overlaps_by_sheet) != EXPECTED_OVERLAP_SHEETS:
        raise ValueError(f"overlap set mudou: {sorted(overlaps_by_sheet)}")

    staging_out = out / "staging"
    technical_root = out / TECH_SOURCE_DIR
    staging_out.mkdir(parents=True, exist_ok=True)
    technical_root.mkdir(parents=True, exist_ok=True)
    descriptor_outputs: dict[str, dict[str, Any]] = {}
    atlas_outputs: dict[str, Path] = {}
    frame_proofs: list[dict[str, Any]] = []
    repack_proofs: list[dict[str, Any]] = []

    for info in sheet_source_infos:
        name = info["name"]
        original_path: Path = info["image_abs"]
        original_bytes = original_path.read_bytes()
        original_image = Image.open(original_path).convert("RGBA")
        desc = json.loads(json.dumps(info["descriptor"]))
        technical_path = out / info["technical_source_path"]
        technical_path.parent.mkdir(parents=True, exist_ok=True)
        technical_path.write_bytes(original_bytes)
        if digest_file(technical_path) != info["old_image_sha256"]:
            raise ValueError(f"source technical copy hash mismatch: {name}")

        atlas_out = staging_out / name
        old_native = info["old_size"]
        if name in overlaps_by_sheet:
            atlas, placements, pack = repack_rows(original_image, desc, gutter=2, columns=4)
            old_frames = desc["frames"]
            old_rects = [list(f["rect"]) for f in old_frames]
            for frame_index, (frame, old_rect, new_rect) in enumerate(zip(old_frames, old_rects, placements)):
                frame["sourceRect"] = old_rect[:]
                frame["sourceFrameIndex"] = frame_index
                frame["rect"] = new_rect
            desc["nativeSize"] = list(atlas.size)
            desc["imageSha256"] = ""
            atlas.save(atlas_out, format="PNG", optimize=False)
            out_image = Image.open(atlas_out).convert("RGBA")
            if out_image.crop((0, 0, original_image.width, original_image.height)).tobytes() != original_image.tobytes():
                raise ValueError(f"repack original canvas pixels changed: {name}")
            repack_proofs.append({"sheet": name, "original_nativeSize": old_native, "derived_nativeSize": list(atlas.size), "layout": pack, "frames_repacked": len(old_frames), "gutter_px": 2, "original_canvas_rgba_exact_in_derived_atlas": True})
        else:
            atlas_out.write_bytes(original_bytes)
            out_image = Image.open(atlas_out).convert("RGBA")
            if out_image.size != original_image.size or out_image.tobytes() != original_image.tobytes():
                raise ValueError(f"byte-copy atlas pixel content changed: {name}")
        new_sha = digest_file(atlas_out)
        desc["image"] = name
        desc["imageSha256"] = new_sha
        desc["sourcePath"] = info["technical_source_path"]
        desc["sourceSha256"] = info["old_image_sha256"]
        desc["sourceSize"] = old_native[:]
        desc["nativeSize"] = list(out_image.size)
        desc["provenance"] = {
            **desc.get("provenance", {}),
            "originalAuthoredSource": {
                "path": info["historical_source_path"],
                "sha256": info["historical_source_sha256"],
                "size": info["historical_source_size"],
                "rawSourceAvailable": False,
                "rawSourcePathAvailable": info["historical_source_path_exists"],
                "rawSourceContentExactMatchInReviewedAtlas": info["historical_raw_content_exact_path_match"],
                "note": "Historical authored-source provenance; technical source is the immutable reviewed atlas copy. No raw source recovery is claimed.",
            },
            "derivedFromReviewedAtlas": {"path": info["image_path"], "sha256": info["old_image_sha256"], "size": old_native},
        }
        errors = validate_sheet(atlas_out, desc)
        if errors:
            raise ValueError(f"static importer sheet contract failed for {name}: {errors}")
        if digest_file(technical_path) != desc["sourceSha256"] or Image.open(technical_path).size != tuple(desc["sourceSize"]):
            raise ValueError(f"static source hash/dimension check failed for {name}")
        for frame in desc["frames"]:
            x, y, w, h = frame["rect"]
            if name not in overlaps_by_sheet:
                source_crop = original_image.crop((x, y, x + w, y + h))
            else:
                sx, sy, sw, sh = frame["sourceRect"]
                source_crop = original_image.crop((sx, sy, sx + sw, sy + sh))
            target_crop = out_image.crop((x, y, x + w, y + h))
            if source_crop.size != target_crop.size or source_crop.tobytes() != target_crop.tobytes():
                raise ValueError(f"per-frame RGBA reconstruction mismatch: {name}/{frame['id']}")
            frame_proofs.append({"sheet": name, "frameId": frame["id"], "sourceFrameIndex": frame.get("sourceFrameIndex"), "sourceRect": frame.get("sourceRect", frame["rect"]), "derivedRect": frame["rect"], "source_rgba_sha256": digest_bytes(source_crop.tobytes()), "derived_rgba_sha256": digest_bytes(target_crop.tobytes()), "rgba_exact": True, "pivot_unchanged": True})
        desc_path = staging_out / Path(name).with_suffix(".json")
        dump_json(desc_path, desc)
        descriptor_outputs[name] = desc
        atlas_outputs[name] = atlas_out

    # Build an essential-only PAGES file. Preserve guardContact and all clip timing/identity fields.
    old_pages_by_sheet = collections.defaultdict(list)
    name_to_info = {i["name"]: i for i in sheet_source_infos}
    pages_out = []
    guard_fields = ("guardContact", "guardContactSource", "guardContactVisibility")
    preservation_fields = ("id", "skin", "weapon", "form", "direction", "action", "attackKey", "spriteNames", "durationMs", "framePixelsPerUnit", "pixelsPerUnit", "startupFrames", "activeFrames", "loop", "wholeFigures", "guardContact", "guardContactSource", "guardContactVisibility")
    for old_page in essential_pages:
        name = Path(old_page["sourcePath"]).name.replace("-source.png", ".png")
        info = name_to_info[name]
        new_page = json.loads(json.dumps(old_page))
        d = descriptor_outputs[name]
        new_page["sourcePath"] = info["technical_source_path"]
        new_page["sourceSha256"] = info["old_image_sha256"]
        new_page["sourceSize"] = info["old_size"][:]
        new_page["nativeSize"] = d["nativeSize"][:]
        new_page["imageSha256"] = d["imageSha256"]
        # Keep existing resourcePath; no file is written into Assets/Resources.
        new_page["provenance"] = {
            "originalAuthoredSource": d["provenance"]["originalAuthoredSource"],
            "derivedFromReviewedAtlas": d["provenance"]["derivedFromReviewedAtlas"],
        }
        for k in preservation_fields:
            if new_page.get(k) != old_page.get(k):
                raise ValueError(f"essential PAGES field unexpectedly changed: {k}/{old_page.get('id')}")
        for k in guard_fields:
            if new_page.get(k) != old_page.get(k):
                raise ValueError(f"guard contact changed: {old_page.get('id')}/{k}")
        errors = validate_page_static(new_page)
        if errors:
            raise ValueError(f"static C# ValidPage contract failed for {new_page.get('id')}: {errors}")
        pages_out.append(new_page)
    if len({p["id"] for p in pages_out}) != 72 or len({ctx(p) for p in pages_out}) != 72:
        raise ValueError("duplicate context/id in essential package")

    pages_out_doc = {**{k: v for k, v in pages_doc.items() if k != "pages"}, "pages": pages_out, "complete": False,
                     "scopeNote": "Portable Fabius Normal essential-only staging. Static pixel-review status preserved; no runtime, Unity import, or gameplay approval claimed."}
    dump_json(out / "PAGES.json", pages_out_doc)

    # Re-hash protected project inputs; this routine must not mutate any of them.
    after_hashes = {relpath(root, p): digest_file(p) for p in protected_paths}
    if before_hashes != after_hashes:
        changed = sorted(k for k in before_hashes if before_hashes[k] != after_hashes.get(k))
        raise ValueError(f"protected project input SHA changed during preparation: {changed}")

    # Validate all emitted clip pages against static ValidPage mirror and prove moto guard metadata unchanged.
    expected_frames = sum(len(p["spriteNames"]) for p in pages_out)
    if expected_frames != 144:
        raise ValueError(f"essential clip frame count changed unexpectedly: {expected_frames}")
    source_frame_ids = {f["id"] for d in descriptor_outputs.values() for f in d["frames"]}
    page_frame_ids = {n for p in pages_out for n in p["spriteNames"]}
    if source_frame_ids != page_frame_ids:
        raise ValueError(f"PAGES/descriptors frame identity mismatch: desc-only={sorted(source_frame_ids-page_frame_ids)[:5]}, pages-only={sorted(page_frame_ids-source_frame_ids)[:5]}")

    runtime_editor_text = "\n".join(
        p.read_text(encoding="utf-8", errors="replace")
        for subtree in (root / "Assets/Darklight/Runtime", root / "Assets/Darklight/Editor")
        if subtree.exists() for p in subtree.rglob("*.cs")
    )
    moto_guard_pages = [p for p in pages_out if p.get("weapon") == 3 and p.get("action") == "Parry"]

    README = """# Portable Fabius Normal essentials staging\n\nThis package contains only 16 Parry/Hurt/Dead atlases (72 clips, 144 frames), their descriptors, and exact technical-source PNG copies. It excludes all Walk sheets, including Moto Walk after direct rejection of the long-legged version.\n\nRerun from any working directory with Python 3.10+ and Pillow 10+: `python prepare_portable_essentials.py --root <project-checkout> --out .`. The script refuses an output directory inside the project root. It reads the checkout and writes only this external output package.\n\nThe 13 atlases without overlaps are copied byte-for-byte. Three Hurt atlases are packing-only derivatives: the full original canvas is retained at the top-left; exact full-frame RGBA crops are copied below it into disjoint rectangles with 2px gutters. Sprite frame IDs, crop dimensions, pivots, PPU, frame PPU, durations, timing, loop, and guardContact metadata are unchanged. sourceRect/sourceFrameIndex records each pre-repack crop. No pixel was removed, resized, resampled, recolored, or composited.\n\n`ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/technical-source/` contains byte-exact copies of the reviewed native atlas PNGs. The new descriptors bind sourcePath/sourceSha256/sourceSize to those technical copies. `provenance.originalAuthoredSource` preserves the historical authored-source path/hash/size and marks `rawSourceAvailable:false`; exact reviewed-atlas hash matches are reported separately. This does not claim recovery of a distinct generator source or approve runtime use.\n\nStatus remains static visual review of original pixels. Packing proof is separate. Package PAGES remains `complete:false`; no Unity importer, Assets write, runtime test, gameplay test, or new art was performed.\n"""
    (out / "README.md").write_text(README, encoding="utf-8")

    # Ship the generator so this package is reproducible after it leaves the chat workspace.
    generator = Path(__file__).resolve()
    generator_copy = out / "prepare_portable_essentials.py"
    if generator != generator_copy.resolve():
        shutil.copy2(generator, generator_copy)

    package_files = []
    for p in sorted(x for x in out.rglob("*") if x.is_file() and x.name not in {"manifest.json", "geometry-proof.json"}):
        package_files.append({"path": p.relative_to(out).as_posix(), "sha256": digest_file(p), "bytes": p.stat().st_size})
    geometry_proof = {
        "schemaVersion": 1,
        "scope": "16 essential Fabius Normal sheets only; no Walk, no Unity, no project writes",
        "sourcePages": {"clips": len(essential_pages), "frameInstances": expected_frames, "complete": False},
        "sheets": [{"name": i["name"], "originalSHA256": i["old_image_sha256"], "sourceTechnicalSHA256": digest_file(out / i["technical_source_path"]), "sourceSize": descriptor_outputs[i["name"]]["sourceSize"], "sourceSha_equals_originalImageSha": descriptor_outputs[i["name"]]["sourceSha256"] == i["old_image_sha256"], "derivedSHA256": digest_file(atlas_outputs[i["name"]]), "originalNativeSize": i["old_size"], "derivedNativeSize": descriptor_outputs[i["name"]]["nativeSize"], "repacked": i["name"] in EXPECTED_OVERLAP_SHEETS} for i in sheet_source_infos],
        "repack": repack_proofs,
        "frames": frame_proofs,
        "rgba_reconstruction": {"frames_checked": len(frame_proofs), "frames_exact": sum(x["rgba_exact"] for x in frame_proofs), "pixel_ops": "byte-exact crop/paste in RGBA; no alpha mask or interpolation"},
        "static_validation": {"sheet_contract_mirror": "DarklightWeaponRedesignImporter.VerifySheet validation fields: passed", "page_contract_mirror": "DarklightWeaponRedesignBank.ValidPage TimedStateAction subset: passed", "unique_page_ids_and_contexts": True, "guardContact_fields_preserved": True, "no_overlapping_output_rectangles": True},
        "consumer_status": {"moto_parry_pages_with_guardContact": sum("guardContact" in p for p in moto_guard_pages), "moto_parry_pages_total": len(moto_guard_pages), "guardContact_consumer_found_in_Runtime_or_Editor_CSharp": "guardContact" in runtime_editor_text or "GuardContact" in runtime_editor_text, "dedicated_DarklightFormController_class_found": "class DarklightFormController" in runtime_editor_text or "class FormController" in runtime_editor_text, "campaign_form_logic_found": "TryTransformPlayer()" in (root / "Assets/Darklight/Runtime/Campaign/DarklightCampaignTransformations.cs").read_text(encoding="utf-8", errors="replace"), "note": "Static symbol scan only; absence of a dedicated FormController does not mean the existing DarklightCampaign partial form logic is absent."},
        "protected_project_inputs": [{"path": k, "sha256_before": before_hashes[k], "sha256_after": after_hashes[k], "unchanged": before_hashes[k] == after_hashes[k]} for k in sorted(before_hashes)],
        "source_sha_history": [{"sheet": i["name"], "originalAuthoredSource": {"path": i["historical_source_path"], "sha256": i["historical_source_sha256"], "size": i["historical_source_size"], "rawSourceAvailable": False, "rawSourcePathAvailable": i["historical_source_path_exists"], "rawSourceContentExactMatchInReviewedAtlas": i["historical_raw_content_exact_path_match"]}} for i in sheet_source_infos],
        "runtimeApproved": False,
        "gameAssetsPublished": False,
        "handoffStatus": "READY_FOR_COORDINATOR for this portable 16-sheet/72-clip essentials package only",
    }
    dump_json(out / "geometry-proof.json", geometry_proof)
    # Hash all package files except the manifest itself; include the proof/README/PAGES in inventory.
    files = []
    for p in sorted(x for x in out.rglob("*") if x.is_file() and x.name != "manifest.json"):
        files.append({"path": p.relative_to(out).as_posix(), "sha256": digest_file(p), "bytes": p.stat().st_size})
    package_manifest = {"purpose": "portable derived technical staging; not a Unity/runtime release", "schemaVersion": 1,
                        "totals": {"sheets": 16, "clips": len(pages_out), "frames": expected_frames},
                        "files": files, "complete": False, "runtimeApproved": False, "gameAssetsPublished": False}
    dump_json(out / "manifest.json", package_manifest)
    return {
        "output": str(out), "sheets": 16, "clips": len(pages_out), "frames": expected_frames,
        "source_matches_preserved": len(sheet_source_infos), "repacked_sheets": len(repack_proofs),
        "frame_rgba_exact": len(frame_proofs), "protected_inputs_unchanged": all(x == after_hashes[k] for k, x in before_hashes.items()),
        "static_valid_page_pass": True, "guardContact_preserved": True, "runtimeApproved": False,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", required=True, type=Path, help="project checkout root (read-only)")
    parser.add_argument("--out", required=True, type=Path, help="portable package directory; must be outside --root")
    args = parser.parse_args()
    try:
        result = run(args.root, args.out)
    except Exception as exc:
        print(f"ERROR: {type(exc).__name__}: {exc}", file=sys.stderr)
        return 1
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
