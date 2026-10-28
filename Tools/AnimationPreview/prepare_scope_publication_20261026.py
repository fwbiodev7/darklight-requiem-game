"""Prepare the reviewed isolated copy; never stage, commit, push, merge or delete.

Run --copy only after the coordinator finishes the Antigravity integration.
The 25/10 helper supplies the checked 24/10 preparation/rebasing implementation;
its old main() is deliberately not called because its extras include references.
"""
import argparse
import hashlib
import json
from pathlib import Path

import prepare_checkpoint_publication_20261025 as previous

base = previous.base
ROOT, DEST = base.ROOT, base.DEST
OUT = ROOT / "outputs/retomada-20261027"
NAMES = ("RequiemMotoForm0Ride8SlamS20261027V3", "RequiemMotoForm0IdleSSWW20261027MatureV1",
         "ReqMotoNormalDualSaws8AttackS20261027V1",
         "RequiemMotoForm0IdleNW_N_NE20261027MatureV1", "RequiemMotoForm0IdleE_SE20261027MatureV1",
         "ReqFistsNormalChuteSSWWNW20261026Selected", "ReqKatanaNormalWalkSW_W20261026ApprovedV1")


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def private_reference(relative):
    value = relative.lower()
    return ((value.startswith("artsources/") or "/artsources/" in value) and
            any(marker in value for marker in ("/references/", "reference", "lucius", "useradaptation")) and
            value.endswith((".png", ".jpg", ".jpeg", ".webp", ".png.meta", ".jpg.meta", ".jpeg.meta", ".webp.meta")))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--copy", action="store_true")
    args = parser.parse_args()
    catalog = base.read(ROOT/base.BANK/"catalog.json")
    active = {Path(p["resourcePath"]).name for p in catalog["pages"]} | {"catalog", "EquipmentIcons", *NAMES}
    active.update(f"RockMotoForm{f}Ride8SlamS20261025V2" for f in range(4))
    permitted_sources = set()
    mixed_original_sources = set()
    rebased_descriptors = {}
    derived_sources = {}
    for name in active - {"catalog"}:
        path = ROOT/base.BANK/(name+".json")
        if not path.is_file():
            continue
        sheet = base.read(path)
        if not sheet.get("excludedSourceFrameIndices"):
            permitted_sources.add(sheet["sourcePath"])
    # These newly selected moto descriptors are not in catalog.json yet, so the
    # continuation helper's catalog walk cannot rebase their excluded originals.
    # Use its byte-exact approved-atlas rebasing rule without modifying active assets.
    for name in NAMES:
        descriptor_path = ROOT/base.BANK/(name+".json")
        if not descriptor_path.is_file():
            continue
        sheet = base.read(descriptor_path)
        if not sheet.get("excludedSourceFrameIndices"):
            continue
        mixed_original_sources.add(sheet["sourcePath"])
        mixed_original_sources.update(frame["sourcePath"] for frame in sheet.get("frames", [])
                                      if frame.get("sourcePath"))
        mixed_original_sources.update(item["path"] for item in sheet.get("additionalSources", [])
                                      if item.get("path"))
        rebased = base.rebase_sheet(name, sheet)
        for frame in rebased.get("frames", []):
            for field in ("sourcePath", "sourceSha256", "sourceSize"):
                if field in frame:
                    frame["originalGenerated"+field[0].upper()+field[1:]] = frame[field]
            frame["sourcePath"] = rebased["sourcePath"]
            frame["sourceSha256"] = rebased["sourceSha256"]
            frame["sourceSize"] = rebased["sourceSize"][:]
        if rebased.get("additionalSources"):
            rebased["originalGeneratedAdditionalSources"] = rebased["additionalSources"]
            rebased["additionalSources"] = []
        rel = (base.BANK/(name+".json")).as_posix()
        rebased_descriptors[rel] = base.encoded(rebased)
        source_rel = rebased["sourcePath"]
        derived_sources[source_rel] = (ROOT/base.BANK/sheet["image"]).read_bytes()
        permitted_sources.add(source_rel)
    extras = set(permitted_sources)
    effect_bank=ROOT/"Assets/Darklight/Resources/Darklight/CombatContacts20261027"
    for path in effect_bank.glob("*"):
        if path.is_file(): extras.add(path.relative_to(ROOT).as_posix())
    effect_sheet=base.read(effect_bank/"ContactBursts.json")
    extras.add(effect_sheet["sourcePath"])
    for folder in ("Tools/Testing", "Tools/Validation", "Tools/Preflight", "Tools/Handoff", "Tools/Delivery", ".codex"):
        extras.update(p.relative_to(ROOT).as_posix() for p in (ROOT/folder).rglob("*")
                      if p.is_file() and p.suffix.lower() in (".py", ".toml") and "__pycache__" not in p.parts)
    extras.update(("README.md", "AGENTS.md"))
    for name in NAMES:
        extras.update((base.BANK/(name+ext)).as_posix() for ext in (".png", ".json", ".png.meta", ".json.meta"))
    selected_art = (
        "sources/RequiemMotoNormal-v2-source.png", "sources/RequiemMotoNormalIdle-S-SW-W-v1-source.png",
        "requiem-motorcycle-normal-guides.json", "requiem-motorcycle-idle-generation.json",
        "punhos-normal/ReqFistsNormalChuteSSWWNW20261026Selected-source.png",
        "punhos-normal/ReqFistsNormalChuteSSWWNW20261026Selected-job.json", "punhos-normal/prepare_chute_selected.py",
        "katana-normal/ReqKatanaNormalWalkSW_W-v1-source.png", "katana-normal/contract.json",
        "katana-normal/review.json", "katana-normal/import-jobs-approved-v1.json",
    )
    extras.update("ArtSources/WeaponRedesign20261026/"+p for p in selected_art)
    extras.update(("README.md", "Docs/Darklight/Planejamento/GDD.md", "Docs/Darklight/Planejamento/LDD.md"))
    extras.update(p.relative_to(ROOT).as_posix() for p in (ROOT/"Docs/Darklight").glob("*20261026*.md"))
    for path in OUT.rglob("*"):
        if not path.is_file():
            continue
        parts = [p.lower() for p in path.relative_to(OUT).parts]
        if (path.suffix.lower() in (".json", ".xml") and path.stat().st_size < 2_000_000
                and not any(p.startswith("windows") or p in ("saves", "cache", "__pycache__", "preview", "qa-history") for p in parts)
                and path.name.lower() not in ("save.json", "palette.json", "publication-preparation.json", "publication-hashes.json")):
            extras.add(path.relative_to(ROOT).as_posix())
    for report_root in (ROOT/"outputs/retomada-20261027",):
        extras.update(p.relative_to(ROOT).as_posix() for p in report_root.glob("*.json") if p.stat().st_size<2000000 and not p.name.startswith("catalog-before"))
        extras.update(p.relative_to(ROOT).as_posix() for p in report_root.glob("*.xml"))
    extras.add("Tools/AnimationPreview/prepare_scope_publication_20261026.py")
    old_excluded = base.excluded

    def excluded(rel):
        value = rel.as_posix(); lower = value.lower()
        if value in mixed_original_sources:
            return True
        if rel.parent == base.BANK and Path(rel.name.removesuffix(".meta")).stem not in active:
            return True
        if value.startswith("ArtSources/WeaponRedesign20261026/"):
            return value not in extras
        if (lower.startswith("artsources/") or "/artsources/" in lower) and (
                "/references/" in lower or "reference" in lower or "lucius" in lower or "useradaptation" in lower):
            return True
        return False if value in permitted_sources else old_excluded(rel)

    base.excluded = excluded
    base.prepare(args.copy)
    report = base.read(base.OUT/"publication-scope.json")
    scope = set(report["scopePaths"]); hashes = dict(report["publishedSha256"]); added = []
    for relative in sorted(extras-scope):
        if private_reference(relative):
            raise ValueError("Private reference cannot be an extra: "+relative)
        source = ROOT/relative
        if not source.is_file():
            continue
        target = DEST/relative
        if not target.resolve().is_relative_to(DEST.resolve()):
            raise ValueError("Publication path escaped clone")
        digest = sha(source)
        if args.copy:
            target.parent.mkdir(parents=True, exist_ok=True)
            if target.is_file() and sha(target) != digest:
                backup = ROOT/"work/retomada-20261026/publication-before-extras"/relative
                backup.parent.mkdir(parents=True, exist_ok=True)
                if not backup.exists():
                    backup.write_bytes(target.read_bytes())
            target.write_bytes(source.read_bytes())
        scope.add(relative); hashes[relative] = digest; added.append(relative)
    # Reapply source-only provenance after the helper-base copies the active PNG/JSON.
    # The atlas bytes are the only published source for a descriptor with excluded frames.
    for relative, data in {**derived_sources, **rebased_descriptors}.items():
        digest = hashlib.sha256(data).hexdigest()
        target = DEST/relative
        if not target.resolve().is_relative_to(DEST.resolve()):
            raise ValueError("Publication provenance path escaped clone: "+relative)
        if args.copy and (not target.is_file() or sha(target) != digest):
            target.parent.mkdir(parents=True, exist_ok=True)
            if target.exists() and relative not in report["scopePaths"]:
                backup = ROOT/"work/retomada-20261027/publication-before-provenance"/relative
                backup.parent.mkdir(parents=True, exist_ok=True)
                if not backup.exists():
                    backup.write_bytes(target.read_bytes())
            target.write_bytes(data)
        scope.add(relative); hashes[relative] = digest
        if relative not in report["scopePaths"] or relative not in added:
            added.append(relative)
    forbidden = [p for p in scope if p.lower().endswith((".mp3", ".mp3.meta", ".zip", ".exe", ".pyc")) or
                 "__pycache__" in p.lower() or "/windows/" in p.lower() or private_reference(p) or "ReqKatanaNormalWalkSW_W-v2-source.png" in p]
    if forbidden:
        raise ValueError("Forbidden publication paths: "+", ".join(forbidden[:8]))
    if args.copy:
        mismatch = [p for p in sorted(scope) if not (DEST/p).is_file() or sha(DEST/p) != hashes[p]]
        if mismatch:
            raise ValueError("Copied hash mismatches: "+", ".join(mismatch[:8]))
    ordered = sorted(scope); pathspec = OUT/"publication-pathspec.nul"
    OUT.mkdir(parents=True, exist_ok=True)
    pathspec.write_bytes(b"".join(p.encode("utf-8")+b"\0" for p in ordered))
    result = dict(date="2026-10-26", destination=str(DEST), branch=base.git("branch", "--show-current"),
        head=base.git("rev-parse", "HEAD"), copied=args.copy, scopeCount=len(ordered), extraCount=len(added),
        pathspec=pathspec.relative_to(ROOT).as_posix(), sourceCatalogSha256=sha(ROOT/base.BANK/"catalog.json"),
        staged=False, committed=False, pushed=False, buildFilesIncluded=False, savesIncluded=False,
        originalMp3Included=False, privateReferencesIncluded=False, rejectedSourcesIncluded=False,
        copiedHashesVerified=args.copy, scopePaths=ordered, extras=added)
    (OUT/"publication-preparation.json").write_text(json.dumps(result,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    (OUT/"publication-hashes.json").write_text(json.dumps(dict(date="2026-10-26",sha256=hashes),ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    if args.copy:
        # Keep the publication's audit records truthful for the completed copy.
        for name in ("publication-preparation.json", "publication-hashes.json"):
            source = OUT/name
            target = DEST/"outputs/retomada-20261027"/name
            if not target.resolve().is_relative_to(DEST.resolve()):
                raise ValueError("Publication report path escaped clone: "+name)
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(source.read_bytes())
    print(json.dumps({k:v for k,v in result.items() if k not in ("scopePaths", "extras")},ensure_ascii=False))


if __name__ == "__main__":
    main()
