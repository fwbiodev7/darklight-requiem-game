"""Read-only scope audit: preserve the source catalogs and distinguish reusable art from candidates."""
from __future__ import annotations

import hashlib
import json
from collections import Counter
from pathlib import Path

from audit_weapon_redesign import CORE_ACTIONS, DIRECTIONS, audit

ROOT = Path(__file__).resolve().parents[2]
BANK = ROOT / "Assets/Darklight/Resources/Darklight/WeaponRedesign20261023"
OUT = ROOT / "outputs/retomada-20261026"
WEAPONS = {0: "Katana", 2: "Punhos da Ruína"}
FORMS = {0: "Normal", 1: "Ruptura", 2: "Eclipse Absoluto"}
MOTO_STATES = ("IdleMounted", "Ride", "Mount", "Dismount", "Serra Infernal", "Giro Devastador",
               "Esmagamento Vertical", "Roda Destruidora", "Cavalgada Brutal", "Execução Aérea Motorizada")


def read(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def sha(path: Path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    raw = audit("2026-10-26")
    pages = read(BANK / "catalog.json")["pages"]
    # Old Eclipse has the same numeric slot as the new combined form, but its
    # name/slot does not make its old art a reviewed combined-form design.
    reusable = [p for p in pages if p["skin"] == 0 and p["weapon"] in WEAPONS and p["form"] in (0, 1)]
    fusion_candidates = [p for p in pages if p["skin"] == 0 and p["weapon"] in WEAPONS and p["form"] in (2, 3)]
    legacy = [p for p in pages if p not in reusable and p not in fusion_candidates]
    matrix = []
    for form, label in FORMS.items():
        for weapon, weapon_name in WEAPONS.items():
            for direction, direction_name in enumerate(DIRECTIONS):
                for action in CORE_ACTIONS:
                    exact = [p for p in reusable if (p["form"], p["weapon"], p["direction"], p["action"]) ==
                             (form, weapon, direction, action)]
                    candidates = [p["id"] for p in fusion_candidates if p["weapon"] == weapon and
                                  p["direction"] == direction and p["action"] == action] if form == 2 else []
                    matrix.append(dict(skin="Réquiem", weapon=weapon_name, weaponId=weapon, form=label,
                                       formId=form, direction=direction_name, action=action,
                                       status="reviewed-source-available" if exact else "replacement-pending",
                                       pageIds=[p["id"] for p in exact], candidatePageIds=candidates,
                                       attackKeys=sorted({p.get("attackKey", "") for p in exact}),
                                       distinctFigures=len({(p["resourcePath"], n) for p in exact for n in p["spriteNames"]}),
                                       allTechniquesComplete=False, currentRuntimeVerified=False))
    moto = []
    moto_sources = []
    for path in sorted(BANK.glob("*Moto*.json")):
        sheet = read(path)
        entry = sheet.get("motorcycle") or sheet.get("motorcycleIdle", {})
        image = path.with_suffix(".png")
        source = ROOT / sheet["sourcePath"]
        source_kind = "legacy-rock-rider-candidate" if entry.get("skin") == 1 else "reviewed-requiem-rider-source"
        moto_sources.append(dict(path=path.relative_to(ROOT).as_posix(), skinId=entry.get("skin"),
                                 oldFormId=entry.get("form"), figures=len(sheet.get("frames", [])),
                                 rideViews=len(entry.get("ride", [])), slamSFrames=len(entry.get("slam", [])),
                                 idleDirections=[c["direction"] for c in entry.get("clips", [])
                                                 if len(c.get("frames", [])) == 4 and len(c.get("durationMs", [])) == 4],
                                 status=source_kind, descriptorStatus=sheet.get("status"),
                                 imageHashValid=image.is_file() and sha(image) == sheet.get("imageSha256"),
                                 sourceHashValid=source.is_file() and sha(source) == sheet.get("sourceSha256"),
                                 reusableAsRequiemWithoutReview=False, unityImported=False, runtimeVerified=False))
    for form, label in FORMS.items():
        for direction in DIRECTIONS:
            for state in MOTO_STATES:
                sources = [s for s in moto_sources if s["skinId"] == 0 and s["oldFormId"] == form and
                           s["descriptorStatus"] in ("visual-reviewed", "imported", "runtime-verified") and
                           s["imageHashValid"] and s["sourceHashValid"] and
                           ((state == "IdleMounted" and DIRECTIONS.index(direction) in s["idleDirections"]) or
                            (state == "Ride" and s["rideViews"] == 8) or
                            (state == "Esmagamento Vertical" and direction == "S" and s["slamSFrames"] == 4))]
                moto.append(dict(skin="Réquiem", form=label, formId=form, direction=direction, action=state,
                                 status="reviewed-source-available" if sources else "requiem-rider-art-pending",
                                 sourceManifests=[s["path"] for s in sources], legacyCandidatesAvailable=True,
                                 staticView=state == "Ride" and bool(sources),
                                 completeClipAvailable=bool(sources) and state in ("Esmagamento Vertical", "IdleMounted"),
                                 currentRuntimeVerified=False))
    def figures(group):
        return len({(p["resourcePath"], n) for p in group for n in p.get("spriteNames", [])})
    covered = sum(bool(row["pageIds"]) for row in matrix)
    summary = dict(catalogClipsPreserved=len(pages), catalogFiguresPreserved=raw["summary"]["wholeFigures"],
                   exactReusablePrimaryClips=len(reusable), distinctReusablePrimaryFigures=figures(reusable),
                   reusablePrimarySheets=len({p["resourcePath"] for p in reusable}),
                   primaryCoreContextsRequired=len(matrix), primaryCoreContextsWithAnyReviewedSource=covered,
                   primaryCoreContextsMissingReviewedSource=len(matrix)-covered,
                   fusionCandidateClips=len(fusion_candidates), fusionCandidateFigures=figures(fusion_candidates),
                   legacyClipsOutsideActiveScope=len(legacy), legacyReferencedFigures=figures(legacy),
                   motorcycleLegacyFigures=sum(s["figures"] for s in moto_sources if s["skinId"] == 1),
                   motorcycleRequiemSourceFigures=sum(s["figures"] for s in moto_sources if s["skinId"] == 0),
                   motorcycleRequiredContextChecklist=len(moto),
                   motorcycleRequiemContextsWithReviewedSource=sum(bool(r["sourceManifests"]) for r in moto),
                   motorcycleRequiemReviewedAnimatedClips=sum(r["completeClipAvailable"] for r in moto),
                   motorcycleRequiemReviewedStaticViews=sum(r["staticView"] for r in moto),
                   structuralErrors=len(raw["errors"]),
                   reusableClipsByForm=dict(Counter(FORMS[p["form"]] for p in reusable)),
                   reusableClipsByWeapon=dict(Counter(WEAPONS[p["weapon"]] for p in reusable)))
    report = dict(schemaVersion=1, date="2026-10-26", complete=False,
                  authority="Docs/Darklight/ESCOPO-CANONICO-20261026.md", summary=summary,
                  catalogPath=(BANK/"catalog.json").relative_to(ROOT).as_posix(), catalogSha256=sha(BANK/"catalog.json"),
                  preservedPreviousSummary=raw["summary"], errors=raw["errors"],
                  eligiblePageIds=[p["id"] for p in reusable],
                  fusionCandidatePageIds=[p["id"] for p in fusion_candidates],
                  legacyPageIds=[p["id"] for p in legacy], primaryMatrix=matrix,
                  motorcycleMatrix=moto, motorcycleSources=moto_sources,
                  limits=["Available reviewed sources are not proof of current import or runtime verification.",
                          "A core Attack context containing any clip does not cover every combo, phase, heavy or aerial technique.",
                          "Old Eclipse/Absoluto clips are candidates only; the new fused appearance needs whole-figure review.",
                          "Rock motorcycle sources retain the old rider; unlocking the mechanic does not approve a Requiem rider.",
                          "The motorcycle checklist is separate from the eight-action primary matrix; denominators cannot be added as a completion percentage.",
                          "This audit neither changes pixels/catalogs nor certifies anatomy, scale, animation consistency or gameplay."])
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT/"coverage.json").write_text(json.dumps(report, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    (OUT/"legacy-manifest.json").write_text(json.dumps(dict(date="2026-10-26", destructiveChanges=False,
        catalogSha256=report["catalogSha256"], legacyPageIds=report["legacyPageIds"],
        fusionCandidatePageIds=report["fusionCandidatePageIds"],
        motorcycleSources=[s for s in moto_sources if s["skinId"] == 1]),
        ensure_ascii=False, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=False))
    return 2 if raw["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
