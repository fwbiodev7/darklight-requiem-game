"""Pack complete generated figures only; never update the shared Unity catalog."""
import json
import statistics

import numpy as np
from PIL import Image
import prepare_weapon_redesign as pipeline
from whole_figure_components import identify

ROOT = pipeline.ROOT
ART = ROOT / "ArtSources/WeaponRedesign20261027/katana-normal"
OUT = ROOT / "outputs/retomada-20261027/katana-normal"
NAME = "ReqKatanaNormalWalkNW_N20261027Selected"
DURATIONS = [65, 65, 70, 65, 65, 70]


def head_axis(image, owner, figure):
    """Measure an intact figure; this never extracts or moves an anatomical part."""
    y0 = figure["grid"][1]
    y1 = y0 + round(figure["bodyHeight"] * .22)
    _, xs = np.where(owner[y0:y1] == figure["index"] + 1)
    return float(np.mean(xs))


def main():
    protected = ROOT / "Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json"
    before = pipeline.sha(protected)
    originals = [ART / f"ReqKatanaNormalWalkNW_N-v{v}-source.png" for v in (1, 2)]
    images = [Image.open(p).convert("RGBA") for p in originals]
    detected = [identify(image, rows=4, columns=3, minimum_area=10000, alpha_threshold=128) for image in images]
    selected = [(0, i) for i in range(9)] + [(1, i) for i in range(9, 12)]
    heights = [detected[s][0][i]["bodyHeight"] for s, i in selected]
    body_height = statistics.median(heights)
    ppu = body_height / 1.98
    reference = ROOT / "ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png"
    ref_image = Image.open(reference).convert("RGBA")
    ref_figures, ref_owner = identify(ref_image, rows=3, columns=4, minimum_area=10000, alpha_threshold=128)
    # Existing manually reviewed support anchors from prepare_katana_normal_20261025.py.
    ref_offsets = {3: (180 - head_axis(ref_image, ref_owner, ref_figures[0])) * body_height / ref_figures[0]["bodyHeight"],
                   4: (189 - head_axis(ref_image, ref_owner, ref_figures[4])) * body_height / ref_figures[4]["bodyHeight"]}
    measurements, supports = [], {}
    for source, index in selected:
        figure = detected[source][0][index]
        direction = 3 if index < 6 else 4
        axis = head_axis(images[source], detected[source][1], figure)
        anchor = [round(axis + ref_offsets[direction]), figure["grid"][3]]
        supports[(source, index)] = anchor
        measurements.append(dict(originalSourcePath=originals[source].relative_to(ROOT).as_posix(),
                                 originalSourceFrameIndex=index, hairY=figure["grid"][1], soleY=figure["grid"][3],
                                 anatomyHeight=figure["bodyHeight"], supportAnchor=anchor,
                                 method="Canonical whole-figure head-axis to ground-support offset, measured hair-to-sole; no image alteration"))
    pipeline.ART = ART
    pipeline.RES = OUT / "packing-inputs"
    temporary = []
    jobs = []
    for source, indices in ((0, list(range(9))), (1, list(range(9, 12)))):
        groups = [(3, list(range(6))), (4, list(range(6, 9)))] if source == 0 else [(4, indices)]
        rows = [dict(sourceRow=group[0] // 3, sourceIndices=group, direction=direction, action="Walk",
                     durationMs=DURATIONS[:len(group)], loop=True) for direction, group in groups]
        job = dict(name=NAME + ("Original9" if source == 0 else "Repaired3"), source=str(originals[source]),
                   preservedSourceName=originals[source].name, skin=0, weapon=0, form=0,
                   columns=3, sourceRows=4, minimumArea=10000, rows=rows,
                   supportAnchors={str(i): dict(anchor=supports[(source, i)], method=measurements[0]["method"]) for i in indices},
                   excludedReason="Only complete selected figures retained; temporary packing input, never an active clip")
        manifest, _ = pipeline.prepare(job)
        manifest["status"] = "packing-input-only"
        pipeline.write(pipeline.RES / (job["name"] + ".json"), manifest)
        temporary.append((manifest, Image.open(pipeline.RES / manifest["image"]).convert("RGBA")))
        jobs.append(job)
    cw = max(m["frames"][0]["rect"][2] for m, _ in temporary)
    ch = max(m["frames"][0]["rect"][3] for m, _ in temporary)
    combined = Image.new("RGBA", (cw * 3, ch * 4))
    final_supports, provenance = {}, []
    frame_index = 0
    for source, (manifest, atlas) in enumerate(temporary):
        for frame in manifest["frames"]:
            x, y, width, height = frame["rect"]
            # Technical atlas packing of WHOLE frames; no anatomical pieces or scaling.
            intact = atlas.crop((x, y, x + width, y + height))
            dx = frame_index % 3 * cw + (cw - width) // 2
            dy = frame_index // 3 * ch
            combined.paste(intact, (dx, dy))
            copied = combined.crop((dx, dy, dx + width, dy + height))
            if not np.array_equal(np.array(copied), np.array(intact)):
                raise ValueError("A complete figure changed RGBA while assembling the technical atlas")
            final_supports[str(frame_index)] = dict(anchor=[round(dx + frame["pivot"][0] * width),
                                                          round(dy + (1 - frame["pivot"][1]) * height)],
                                                       method=measurements[0]["method"])
            provenance.append(dict(finalFrameIndex=frame_index,
                                   originalSourcePath=originals[source].relative_to(ROOT).as_posix(),
                                   originalSourceSha256=pipeline.sha(originals[source]),
                                   originalSourceFrameIndex=frame["sourceFrameIndex"],
                                   originalSourceRect=frame["sourceRect"], wholeFigureRgbaPreserved=True))
            frame_index += 1
    combined_source = ART / (NAME + "-source.png")
    combined.save(combined_source)
    pipeline.RES = OUT / "staged"
    pipeline.OUT = OUT
    final_job = dict(name=NAME, source=str(combined_source), preservedSourceName=combined_source.name,
                     skin=0, weapon=0, form=0, columns=3, sourceRows=4, minimumArea=10000,
                     rows=[dict(sourceRow=0, sourceIndices=list(range(6)), direction=3, action="Walk", durationMs=DURATIONS, loop=True),
                           dict(sourceRow=2, sourceIndices=list(range(6, 12)), direction=4, action="Walk", durationMs=DURATIONS, loop=True)],
                     supportAnchors=final_supports)
    manifest, pages = pipeline.prepare(final_job)
    expected_pixels = sum(m["sourcePixels"] for m, _ in temporary)
    if manifest["sourcePixels"] != expected_pixels:
        raise ValueError("Selected source RGBA pixel count changed")
    calibration = dict(method="Median intact hair-to-sole anatomy, one PPU per atlas; no per-frame resizing",
                       measurements=measurements, bodyHeightUnits=1.98, worldVisualScale=1.325,
                       measurementUncertaintyPixels=6, visualScaleCheckPending=True)
    manifest.update(pixelsPerUnit=ppu, pixelsPerUnitCalibration=calibration, originalFrameProvenance=provenance,
                    humanApproved=False, imported=False, runtimeVerified=False)
    manifest["notes"] += ["Nine complete V1 figures preserved; only three full N poses selected from V2 repair.",
                           "New NW/N six-phase walking; existing human-approved SW/W V1 and S were not edited.",
                           "400 ms each direction; clipping, direction and gait reviewed; Unity transitions/ground contact remain pending."]
    for page in pages:
        page.update(pixelsPerUnit=ppu, humanApproved=False, imported=False, runtimeVerified=False)
    pipeline.write(pipeline.RES / (NAME + ".json"), manifest)
    pipeline.write(ART / "import-jobs.json", dict(jobs=[final_job], wholeFigureSourcePreparationJobs=jobs))
    pipeline.write(OUT / "catalog-fragment.json", dict(schemaVersion=1, complete=False, stagingOnly=True, pages=pages))
    checks = pipeline.qa(NAME)
    after = pipeline.sha(protected)
    receipt = dict(name=NAME, frames=12, clips=2, directions=["NW", "N"], phasesPerDirection=6,
                   durationMs=DURATIONS, loopMs=400, pixelsPerUnit=ppu, worldVisualScale=1.325,
                   nativeSize=manifest["nativeSize"], sourceSize=manifest["sourceSize"],
                   sourcePath=manifest["sourcePath"], sourceSha256=manifest["sourceSha256"], imageSha256=manifest["imageSha256"],
                   rgbaExact=manifest["sourceRgbaExactReconstruction"], originalWholeFiguresRgbaExact=True,
                   originalFiguresPreserved=9, completeFiguresRepaired=3, anatomyEdits=False,
                   packaged=True, visualReviewed=True, humanApproved=False, unityImported=False, runtimeVerified=False,
                   sharedCatalogBeforeSha256=before, sharedCatalogAfterSha256=after, sharedCatalogUnchanged=before == after,
                   qa=checks, originalFrameProvenance=provenance)
    pipeline.write(OUT / "prepared.json", receipt)
    print(json.dumps({k: v for k, v in receipt.items() if k != "originalFrameProvenance"}, ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
