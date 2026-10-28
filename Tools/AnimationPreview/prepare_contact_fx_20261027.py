"""Package the generated contact VFX unchanged, independently of weapon catalogs."""
import argparse
import json
import shutil

import numpy as np
from PIL import Image, ImageSequence
import prepare_weapon_redesign as pipeline

ROOT = pipeline.ROOT
ART = ROOT / "ArtSources/CombatContacts20261027/candidate"
OUT = ROOT / "outputs/retomada-20261027/contact-fx"
NAME = "ContactBursts"
TIMINGS = [25, 40, 45, 55]
ANCHORS = [[223, 225], [585, 225], [945, 225], [1329, 225],
           [225, 521], [585, 521], [945, 521], [1329, 521],
           [223, 875], [585, 875], [945, 875], [1329, 875]]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--coordinator-reviewed", action="store_true")
    args = parser.parse_args()
    source = ART / "ContactBursts-v1-source.png"
    image = Image.open(source).convert("RGBA")
    rgba = np.array(image)
    width, height = image.size
    staged = OUT / "staged"
    staged.mkdir(parents=True, exist_ok=True)
    target = staged / (NAME + ".png")
    shutil.copy2(source, target)
    if pipeline.sha(source) != pipeline.sha(target):
        raise ValueError("Contact source PNG changed while copying")
    frames, metrics = [], []
    for row in range(3):
        for column in range(4):
            index = row * 4 + column
            x0, x1 = round(column * width / 4), round((column + 1) * width / 4)
            y0, y1 = round(row * height / 3), round((row + 1) * height / 3)
            part = rgba[y0:y1, x0:x1]
            alpha = part[:, :, 3]
            if not np.any(alpha >= 32):
                raise ValueError(f"Empty visible contact motif {index}")
            edge = np.concatenate((alpha[0], alpha[-1], alpha[:, 0], alpha[:, -1]))
            if int(edge.max()) >= 32:
                raise ValueError(f"Visible motif crosses cell boundary: {index}")
            pivot = [(ANCHORS[index][0] - x0) / (x1 - x0), 1 - (ANCHORS[index][1] - y0) / (y1 - y0)]
            frames.append(dict(id=f"Redesign_Contact_{index:02}", rect=[x0, y0, x1 - x0, y1 - y0], pivot=pivot,
                               durationMs=TIMINGS[column], action=["Punch", "Katana", "Heavy"][row],
                               direction="S", phase=["birth", "peak", "fragments", "dissipation"][column],
                               sourceFrameIndex=index, sourceRect=[x0, y0, x1 - x0, y1 - y0],
                               sourceAnchor=ANCHORS[index], sourceOffsetInFrame=[0, 0], sourceScale=1))
            metrics.append(dict(frame=index, rect=frames[-1]["rect"], visibleAtAlpha32=int((alpha >= 32).sum()),
                                nonzeroAlpha=int((alpha > 0).sum()), edgeAlphaMax=int(edge.max()),
                                colorCountVisible=len(np.unique(part[alpha >= 32], axis=0))))
    clips = [dict(name=name, frames=[f"Redesign_Contact_{row * 4 + i:02}" for i in range(4)], loop=False, allowStatic=False)
             for row, name in enumerate(("Punch", "Katana", "Heavy"))]
    status = "visual-reviewed" if args.coordinator_reviewed else "candidate"
    manifest = dict(schemaVersion=1, image=target.name, imageSha256=pipeline.sha(target), nativeSize=[width, height],
                    sourceSize=[width, height], sourcePath=source.relative_to(ROOT).as_posix(), sourceSha256=pipeline.sha(source),
                    pixelsPerUnit=512, status=status, wholeFigures=True, wholeEffects=True, containsCharacters=False,
                    assetType="contact-vfx", columns=4, rows=3, rectangleOrigin="top-left", frames=frames, clips=clips,
                    requiredClips=[c["name"] for c in clips], quality=dict(alphaThreshold=8, maxColors=256, maxHeightRatio=32),
                    sourceRgbaExactReconstruction=True, sourceScale=1, sourceEdited=False, anatomyEdits=False,
                    coordinatorReviewed=args.coordinator_reviewed, humanApproved=False, unityImported=False, runtimeVerified=False,
                    logicalResolution="Native generated raster; exact indexed pixel grid unverified",
                    notes=["Separate complete effects only; no characters, body parts, weapons or scene tiles.",
                           "The original RGBA PNG is copied byte-for-byte. No alpha threshold, palette edit or background removal was applied to the asset.",
                           "Generated motifs contain soft-alpha halos and more colors than the requested limited palette; coordinator review required.",
                           "Native 4x3 cells retain all detached particles. Maximum boundary alpha is recorded separately.",
                           "PPU512 is a proposed compact effect scale: full cell width0.75world units; verify size/occlusion at actual contact in Unity.",
                           "Pivot is a manually estimated emission origin of the complete effect; uncertainty roughly6native pixels.",
                           "Point filter/no mipmaps/uncompressed import; 25/40/45/55ms=165ms per clip; non-looping.",
                           "No weapon catalog, Runtime, Unity import or Git changes in this preparation."])
    pipeline.write(staged / (NAME + ".json"), manifest)
    pipeline.RES, pipeline.OUT = staged, OUT
    checks = pipeline.qa(NAME)
    preview = OUT / NAME / "preview"
    # GIF stores10ms ticks; preserve nearest cumulative timing (total160ms) in a labeled preview.
    for index, name in enumerate(("Punch", "Katana", "Heavy")):
        with Image.open(preview / f"clip-0{index}.gif") as gif:
            animation = [frame.copy() for frame in ImageSequence.Iterator(gif)]
        animation[0].save(preview / f"{name}-timed-preview.gif", save_all=True, append_images=animation[1:],
                          duration=[20, 40, 50, 50], loop=0, disposal=2)
    receipt = dict(name=NAME, status=status, motifs=12, clips=3, framesPerClip=4, durationMs=TIMINGS, clipDurationMs=165,
                   rectangleOrigin="top-left", sourcePath=manifest["sourcePath"], sourceSha256=manifest["sourceSha256"],
                   imageSha256=manifest["imageSha256"], nativeSize=[width, height], pixelsPerUnit=512,
                   rgbaCopyExact=True, originalPngPreserved=True, fieldsMeasured=metrics,
                   softAlphaPixels=int(((rgba[:, :, 3] > 0) & (rgba[:, :, 3] < 255)).sum()),
                   qa=checks, coordinatorReviewed=args.coordinator_reviewed, humanApproved=False,
                   unityImported=False, runtimeVerified=False, weaponCatalogChanged=False,
                   sourceGenerationCalls=1, runtimePoolingAndCinematicsImplemented=False)
    pipeline.write(OUT / "prepared.json", receipt)
    contract = json.loads((ART / "contract.json").read_text(encoding="utf-8"))
    contract.update(status=status, sourcePath=manifest["sourcePath"], sourceSha256=manifest["sourceSha256"],
                    durationMs=TIMINGS, clipDurationMs=165, coordinatorReviewed=args.coordinator_reviewed,
                    knownLimits=["Soft-alpha glow generated despite the no-bloom request", "Palette exceeds requested12-16colors"],
                    unityImported=False, runtimeVerified=False)
    pipeline.write(ART / "contract.json", contract)
    print(json.dumps({k: v for k, v in receipt.items() if k != "fieldsMeasured"}, ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
