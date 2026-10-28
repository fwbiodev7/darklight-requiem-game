"""Pack new whole-rider idle figures in a separate descriptor; never modify the weapon catalog."""
import json
from pathlib import Path

import prepare_weapon_redesign as base

NAME = "RequiemMotoForm0IdleSSWW20261026V1"
SOURCE = base.ROOT / "ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormalIdle-S-SW-W-v1-source.png"
OUT = base.ROOT / "outputs/retomada-20261026/moto-idle"


def main():
    supports = [[190,359],[569,359],[964,359],[1342,359],
                [197,678],[581,678],[966,679],[1349,677],
                [194,974],[581,975],[966,977],[1345,973]]
    # Comparable seated rider stature in the whole sources, not motorcycle/FX
    # bounds: approved source hair~33/boot~272; new source hair~48/boot~297.
    # One PPU for the entire sheet. The in-world scale remains a review gate.
    ppu = (252/1.98)*(249/239)
    guide = dict(method="Comparable seated rider anatomy in whole sources; bike bounds excluded",
                 referenceSource="ArtSources/WeaponRedesign20261026/sources/RequiemMotoNormal-v2-source.png",
                 referencePixelsPerUnit=252/1.98, referenceRiderHairY=33, referenceRiderBootY=272,
                 newRiderHairY=48, newRiderBootY=297, referenceAnatomyPixels=239,
                 newAnatomyPixels=249, measurementUncertaintyPixels=6, pixelsPerUnit=ppu,
                 visualScaleCheckPending=True, wholeSourceReviewed=True, humanApproval=False)
    durations = [350,250,180,350]
    job = dict(name=NAME, source=str(SOURCE), preservedSourceName=NAME+"-source.png",
               skin=0, weapon=3, form=0, columns=4, sourceRows=3, minimumArea=15000,
               rows=[dict(sourceRow=i,direction=i,action="Idle",durationMs=durations,loop=True) for i in range(3)],
               supportAnchors={str(i):dict(anchor=anchor,method="Whole-source inspected tire ground support")
                               for i,anchor in enumerate(supports)})
    base.OUT = OUT
    manifest, pages = base.prepare(job)
    manifest["pixelsPerUnit"] = ppu
    manifest["pixelsPerUnitCalibration"] = guide
    clips = []
    for direction in range(3):
        clips.append(dict(direction=direction, frames=[f["id"] for f in manifest["frames"][direction*4:direction*4+4]],
                          durationMs=durations,loop=True))
    manifest["motorcycleIdle"] = dict(skin=0,form=0,clips=clips,otherDirectionsComplete=False,
                                       movingLocomotionComplete=False)
    manifest["notes"] = ["Twelve new complete rider and motorcycle drawings, three directions and four genuine idle phases.",
                         "Neutral, inhalation, closed-eye blink/exhalation and return; wheel/boot support registration only.",
                         "No anatomical montage, resizing, palette or alpha removal; all selected source RGBA preserved.",
                         "Five directions, mounted transitions and moving locomotion remain incomplete.",
                         "Separate motorcycle idle descriptor; no insertion into the three-weapon shared catalog.",
                         "Unity import, in-world scale and runtime verification are separate gates."]
    base.write(base.RES/(NAME+".json"),manifest)
    checks = base.qa(NAME)
    receipt = dict(name=NAME,sourcePath=manifest["sourcePath"],sourceSha256=manifest["sourceSha256"],
                   imageSha256=manifest["imageSha256"],manifestPath=(base.RES/(NAME+".json")).relative_to(base.ROOT).as_posix(),
                   frames=len(manifest["frames"]),directions=[0,1,2],pixelsPerUnit=ppu,
                   exactSourceRgba=manifest["sourceRgbaExactReconstruction"],qa=checks,
                   generated=True,sourceReviewed=True,packaged=True,unityImported=False,runtimeVerified=False,
                   allEightDirectionsComplete=False,consumerImplemented=False)
    base.write(OUT/"prepared.json",receipt)
    base.write(OUT/"preparation-jobs.json",dict(jobs=[job]))
    base.write(OUT/"scale-guides.json",guide)
    print(json.dumps(receipt,ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
