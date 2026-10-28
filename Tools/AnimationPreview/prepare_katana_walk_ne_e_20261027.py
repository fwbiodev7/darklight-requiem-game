"""Preserve and package the NE/E whole-figure candidate; never change the active catalog."""
import argparse
import json
import statistics

from PIL import Image, ImageSequence
import prepare_weapon_redesign as pipeline
from prepare_katana_walk_nw_n_20261027 import head_axis
from whole_figure_components import identify

ROOT = pipeline.ROOT
ART = ROOT / "ArtSources/WeaponRedesign20261027/katana-normal"
OUT = ROOT / "outputs/retomada-20261027/katana-normal/ne-e-candidate"
NAME = "ReqKatanaNormalWalkNE_E20261027CandidateV1"
TIMINGS = [65, 65, 70, 65, 65, 70]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--coordinator-reviewed", action="store_true")
    args = parser.parse_args()
    status = "visual-reviewed" if args.coordinator_reviewed else "candidate"
    source = ART / "ReqKatanaNormalWalkNE_E-v1-source.png"
    image = Image.open(source).convert("RGBA")
    figures, owners = identify(image, rows=4, columns=3, minimum_area=10000, alpha_threshold=128)
    if len(figures) != 12 or any(f["touchesExtractionEdge"] for f in figures):
        raise ValueError("Expected twelve complete uncut NE/E figures")
    body_height = statistics.median(f["bodyHeight"] for f in figures)
    ppu = body_height / 1.98
    ref_back = ROOT / "ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png"
    ref_east = ROOT / "ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleE_SE20261025-source.png"
    reference_offsets = {}
    for direction, path, rows, index, support_x in [(5, ref_back, 3, 8, 200), (6, ref_east, 2, 0, 244)]:
        ref = Image.open(path).convert("RGBA")
        ref_figures, ref_owner = identify(ref, rows=rows, columns=4, minimum_area=10000, alpha_threshold=128)
        reference_offsets[direction] = (support_x - head_axis(ref, ref_owner, ref_figures[index])) * body_height / ref_figures[index]["bodyHeight"]
    supports, measurements = {}, []
    for figure in figures:
        index = figure["index"]
        direction = 5 if index < 6 else 6
        anchor = [round(head_axis(image, owners, figure) + reference_offsets[direction]), figure["grid"][3]]
        supports[str(index)] = dict(anchor=anchor, method="Canonical whole-figure head-axis to support offset; complete source inspected; lower boot sole")
        measurements.append(dict(sourceFrameIndex=index, hairY=figure["grid"][1], soleY=figure["grid"][3], anatomyHeight=figure["bodyHeight"]))
    job = dict(name=NAME, source=str(source), skin=0, weapon=0, form=0, columns=3, sourceRows=4,
               minimumArea=10000, supportAnchors=supports,
               rows=[dict(sourceRow=0, sourceIndices=list(range(6)), direction=5, action="Walk", loop=True, durationMs=TIMINGS),
                     dict(sourceRow=2, sourceIndices=list(range(6, 12)), direction=6, action="Walk", loop=True, durationMs=TIMINGS)])
    catalog = ROOT / "Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json"
    before = pipeline.sha(catalog)
    pipeline.ART = ART
    pipeline.RES = OUT / "staged-candidate"
    pipeline.OUT = OUT
    manifest, pages = pipeline.prepare(job)
    manifest.update(status=status, pixelsPerUnit=ppu, humanApproved=False, coordinatorReviewed=args.coordinator_reviewed,
                    unityImported=False, runtimeVerified=False,
                    pixelsPerUnitCalibration=dict(method="One median intact hair-to-boot anatomical PPU; blade does not define the height",
                                                   measurements=measurements, bodyHeightUnits=1.98, worldVisualScale=1.325,
                                                   measurementUncertaintyPixels=6, visualScaleCheckPending=True))
    manifest["notes"] += ["NE/E whole figures and orientation are consistent; inversion of support/leading legs is not clearly established after one repair attempt.",
                           "V2 repair is preserved separately but not selected; source V1 is retained intact without any pixel edit.",
                           "Not active until coordinator reviews the full sheet; this is not a claim of complete validated walking."]
    for page in pages:
        page.update(status=status, pixelsPerUnit=ppu, humanApproved=False, coordinatorReviewed=args.coordinator_reviewed,
                    unityImported=False, runtimeVerified=False)
    pipeline.write(pipeline.RES / (NAME + ".json"), manifest)
    pipeline.write(ART / "import-jobs-ne-e.json", dict(jobs=[job], coordinatorReviewed=args.coordinator_reviewed))
    fragment_name = "catalog-fragment.json" if args.coordinator_reviewed else "candidate-catalog-fragment.json"
    pipeline.write(OUT / fragment_name, dict(schemaVersion=1, complete=False, stagingOnly=True, status=status, pages=pages))
    checks = pipeline.qa(NAME)
    preview = OUT / NAME / "preview"
    for index, direction in enumerate(("NE", "E")):
        with Image.open(preview / f"clip-0{index}.gif") as gif:
            frames = [frame.copy() for frame in ImageSequence.Iterator(gif)]
        frames[0].save(preview / f"walk-{direction}-400ms.gif", save_all=True, append_images=frames[1:],
                       duration=[60, 70, 70, 60, 70, 70], loop=0, disposal=2)
    contract = json.loads((ART / "contract-ne-e.json").read_text(encoding="utf-8"))
    preserved = all(pipeline.sha(ROOT / entry["path"]) == entry["sha256"] for entry in contract["protectedWalkSources"])
    receipt = dict(name=NAME, status=status, figures=12, clips=2, directions=["NE", "E"], phasesPerDirection=6,
                   loopMs=400, pixelsPerUnit=ppu, worldVisualScale=1.325, nativeSize=manifest["nativeSize"],
                   sourcePath=manifest["sourcePath"], sourceSha256=manifest["sourceSha256"], imageSha256=manifest["imageSha256"],
                   selectedSource="V1", repairV2Selected=False, sourceRgbaExactReconstruction=manifest["sourceRgbaExactReconstruction"],
                   anatomyEdits=False, priorWalkSourcesUnchanged=preserved, sharedCatalogBeforeSha256=before,
                   sharedCatalogAfterSha256=pipeline.sha(catalog), sharedCatalogWrittenByThisScript=False,
                   qa=checks, coordinatorReviewed=args.coordinator_reviewed, humanApproved=False,
                   unityImported=False, runtimeVerified=False, remaining="Leading/support leg inversion and movement need full-sheet review; SE not generated")
    pipeline.write(OUT / "prepared.json", receipt)
    contract.update(status=status, candidateName=NAME, priorWalkSourcesUnchanged=preserved, coordinatorReviewed=args.coordinator_reviewed,
                    unityImported=False, runtimeVerified=False, gaitReviewPending=not args.coordinator_reviewed)
    pipeline.write(ART / "contract-ne-e.json", contract)
    print(json.dumps(receipt, ensure_ascii=False))
    return 2 if checks["errors"] or not preserved else 0


if __name__ == "__main__":
    raise SystemExit(main())
