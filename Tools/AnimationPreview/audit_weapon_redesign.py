"""Inventory additive whole-figure redesign coverage without changing sprites or legacy catalogs."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
RESOURCE = ROOT / "Assets/Darklight/Resources/Darklight/WeaponRedesign20261023"
CORE_ACTIONS = ("Idle", "Walk", "Attack", "Charging", "Dodge", "Parry", "Hurt", "Dead")
DIRECTIONS = ("S", "SW", "W", "NW", "N", "NE", "E", "SE")


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def legacy_inventory() -> dict:
    text = (ROOT / "Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset").read_text(encoding="utf-8")
    pages = []
    for block in re.split(r"(?m)^  - name: ", text)[1:]:
        def field(name: str, default: str = "") -> str:
            match = re.search(rf"(?m)^    {name}:[ \t]*(.*)$", block)
            return match.group(1).strip() if match else default
        weapon = int(field("weapon", "-1"))
        if weapon not in (0, 1):
            continue
        encoded = re.match(r"[0-9a-fA-F]+", field("directions"))
        packed = bytes.fromhex(encoded.group()) if encoded else b""
        directions = [int.from_bytes(packed[i:i + 4], "little") for i in range(0, len(packed), 4)]
        pages.append({"name": block.splitlines()[0], "skin": int(field("skin", "-1")), "weapon": weapon,
                      "actionBank": field("kind"), "attackKey": field("attackKey"), "directions": directions,
                      "phases": int(field("phases", "0")),
                      "spriteNames": re.findall(r"(?m)^    - (Today_.+)$", block)})
    ruin = json.loads((ROOT / "Assets/Darklight/Resources/Darklight/RuinFists/catalog.json").read_text(encoding="utf-8"))
    return {"katanaAndDaggersPages": pages, "ruinFistsConfigurationCount": len(ruin["pages"]),
            "ruinFistsPhysicalSheetCount": len({p["resourcePath"] for p in ruin["pages"]}),
            "ruinFistsPaletteVariants": sum(bool(p.get("paletteVariant")) for p in ruin["pages"])}


def audit(date: str = "2026-10-23") -> dict:
    path = RESOURCE / "catalog.json"
    catalog = json.loads(path.read_text(encoding="utf-8")) if path.exists() else {"pages": [], "complete": False}
    pages = catalog.get("pages", [])
    errors, coverage, files = [], [], {}
    keys = set()
    for page in pages:
        key = (page.get("skin"), page.get("weapon"), page.get("form"), page.get("direction"), page.get("action"), page.get("attackKey") or "")
        if key in keys:
            errors.append(f"Duplicate exact context: {key}")
        keys.add(key)
        if page.get("status") not in ("visual-reviewed", "imported", "runtime-verified") or not page.get("wholeFigures"):
            errors.append(f"Unreviewed/non-whole page: {page.get('id')}")
        resource = page.get("resourcePath", "")
        if not resource.startswith("Darklight/WeaponRedesign20261023/") or ".." in resource:
            errors.append(f"Resource outside additive bank: {resource}")
            continue
        image = ROOT / "Assets/Darklight/Resources" / f"{resource}.png"
        sheet_path = image.with_suffix(".json")
        source = ROOT / page.get("sourcePath", "")
        for file, expected in ((image, page.get("imageSha256")), (source, page.get("sourceSha256"))):
            if not file.is_file():
                errors.append(f"Missing preserved source/image: {file.relative_to(ROOT)}")
            elif digest(file).lower() != str(expected).lower():
                errors.append(f"Hash mismatch: {file.relative_to(ROOT)}")
        if not sheet_path.exists():
            errors.append(f"Missing native sheet manifest: {sheet_path.relative_to(ROOT)}")
            continue
        sheet = json.loads(sheet_path.read_text(encoding="utf-8"))
        for field in ("imageSha256", "sourcePath", "sourceSha256", "nativeSize", "sourceSize", "pixelsPerUnit"):
            if page.get(field) != sheet.get(field):
                errors.append(f"Page/sheet mismatch {page.get('id')}: {field}")
        names = {frame["id"] for frame in sheet.get("frames", [])}
        if any(name not in names for name in page.get("spriteNames", [])):
            errors.append(f"Missing named whole figure: {page.get('id')}")
        mapped = bool(page.get("sourceSpriteNames"))
        coverage.append({"id": page["id"], "skin": page["skin"], "weapon": page["weapon"], "form": page["form"],
                         "direction": page["direction"], "directionName": DIRECTIONS[page["direction"]], "action": page["action"],
                         "attackKey": page.get("attackKey") or "", "frames": len(page.get("spriteNames", [])),
                         "mode": "exact-existing-phase-mapping" if mapped else "explicit-duration-clip",
                         "sourceSpriteNames": page.get("sourceSpriteNames", []), "status": page.get("status"),
                         "staticClip": len(page.get("spriteNames", [])) == 1})
        files[resource] = {"frames": len(sheet.get("frames", [])), "nativeSize": sheet.get("nativeSize"),
                           "sourceSize": sheet.get("sourceSize"), "imageSha256": page.get("imageSha256")}
    present = {key[:5] for key in keys}
    missing = [{"skin": skin, "weapon": weapon, "form": form, "direction": direction, "directionName": DIRECTIONS[direction], "action": action}
               for skin in range(2) for weapon in range(3) for form in range(4) for direction in range(8) for action in CORE_ACTIONS
               if (skin, weapon, form, direction, action) not in present]
    expected = 2 * 3 * 4 * 8 * len(CORE_ACTIONS)
    icon_path = RESOURCE / "EquipmentIcons.json"
    icon_names = {f"Redesign_Icon_Weapon{weapon}_Form{form}" for weapon in range(3) for form in range(4)}
    icons = {"manifestExists": icon_path.exists(), "expected": 12, "present": 0, "missing": sorted(icon_names)}
    if icon_path.exists():
        icon_sheet = json.loads(icon_path.read_text(encoding="utf-8"))
        actual = {frame["id"] for frame in icon_sheet.get("frames", [])}
        icons.update({"present": len(actual & icon_names), "missing": sorted(icon_names - actual),
                      "nativeSize": icon_sheet.get("nativeSize"), "sourceSize": icon_sheet.get("sourceSize"), "status": icon_sheet.get("status")})
        if actual - icon_names:
            errors.append("Equipment icon IDs outside exact weapon/form contract")
        for file, expected_hash in ((RESOURCE / "EquipmentIcons.png", icon_sheet.get("imageSha256")),
                                    (ROOT / icon_sheet.get("sourcePath", ""), icon_sheet.get("sourceSha256"))):
            if not file.is_file() or digest(file).lower() != str(expected_hash).lower():
                errors.append(f"Missing/changed dedicated icon source: {file.relative_to(ROOT)}")
    referenced = {(page.get("resourcePath"), name) for page in pages for name in page.get("spriteNames", [])}
    physical = sum(sheet["frames"] for sheet in files.values())
    explicit_holds = sum(len(page.get("spriteNames", [])) - len(set(page.get("spriteNames", [])))
                         for page in pages if page.get("sourceSpriteNames"))
    dagger_idle = []
    for form in range(4):
        directions = []
        for direction in range(8):
            clips = [page for page in pages if (page.get("skin"), page.get("weapon"), page.get("form"),
                     page.get("direction"), page.get("action")) == (0, 1, form, direction, "Idle")]
            directions.append({"direction": direction, "directionName": DIRECTIONS[direction],
                               "present": bool(clips), "uniqueFigures": len({name for page in clips for name in page.get("spriteNames", [])}),
                               "pageIds": [page["id"] for page in clips]})
        dagger_idle.append({"skin": 0, "weapon": 1, "form": form, "directions": directions,
                            "directionsPresent": sum(item["present"] for item in directions)})
    return {"schemaVersion": 1, "date": date, "catalogExists": path.exists(), "complete": False,
            "summary": {"sheets": len(files), "wholeFigures": sum(s["frames"] for s in files.values()), "exactContextClips": len(coverage),
                        "uniqueRuntimeReferencedFigures": len(referenced), "unreferencedPhysicalFigures": physical - len(referenced),
                        "explicitLegacyPhaseHolds": explicit_holds,
                        "coreContextsWithAnyNewClip": expected - len(missing), "coreContextsRequired": expected,
                        "coreContextsMissingNewArt": len(missing), "structuralErrors": len(errors),
                        "clipsByWeapon": dict(Counter(str(p["weapon"]) for p in coverage))},
            "limits": ["A context with a clip is not proof that all attack phases, living variants or transitions have new art.",
                       "Timed clips preserve explicit durations; combat is replaced only by exact existing sprite-name mappings.",
                       "Missing new contexts continue through existing approved same-context consumers; they are pending redesign.",
                       "Technical inventory does not certify anatomy, pixel art, shader output or gameplay; those need visual/runtime review.",
                       "This audit does not change sprites, palettes, Unity GUIDs, saves, combat or music."],
            "errors": errors, "coverage": coverage, "requiemDaggerIdle": dagger_idle,
            "missingCoreContexts": missing, "sheets": files, "equipmentIcons": icons, "legacy": legacy_inventory()}


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--out", default="outputs/redesign-armas-20261023/coverage.json")
    parser.add_argument("--date", default="2026-10-23", help="Date of this audit; does not relabel import/runtime review status")
    args = parser.parse_args()
    report = audit(args.date)
    output = ROOT / args.out
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({**report["summary"], "report": output.relative_to(ROOT).as_posix()}, ensure_ascii=False))
    return 2 if report["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
