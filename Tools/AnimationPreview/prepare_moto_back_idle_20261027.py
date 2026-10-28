"""Pack twelve new intact rear-idle riders. No runtime, Unity, shared catalog or Git changes."""
import json

import prepare_weapon_redesign as pipeline

ROOT = pipeline.ROOT
ART = ROOT/"ArtSources/WeaponRedesign20261027/moto-normal"
OUT = ROOT/"outputs/retomada-20261027/moto-normal"
NAME = "RequiemMotoForm0IdleNW_N_NE20261027V1"
SOURCE = ART/"RequiemMotoNormalIdleNW_N_NE-v1-source.png"
REFERENCE = ROOT/"Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/RequiemMotoForm0Ride8SlamS20261026V1.json"


def main():
    preserved = [ROOT/"ArtSources/WeaponRedesign20261026/sources"/p for p in (
        "RequiemMotoNormal-v2-source.png", "RequiemMotoNormalIdle-S-SW-W-v1-source.png")]
    preserved += [pipeline.RES/(p+".png") for p in (
        "RequiemMotoForm0Ride8SlamS20261026V1", "RequiemMotoForm0IdleSSWW20261026V1")]
    before = {p.relative_to(ROOT).as_posix():pipeline.sha(p) for p in preserved}
    reference = json.loads(REFERENCE.read_text(encoding="utf-8"))
    # No standing rider exists in this rear-idle sheet. Reuse the inspected
    # standing-rider PPU of the eight-view reference after whole-source comparison
    # of matching native head/nape/torso scale, not vehicle bounding-box height.
    ppu = reference["pixelsPerUnit"]
    guide = dict(method="Matched native seated rider anatomy against the whole eight-view reference; reused its standing-body PPU",
        referenceManifest=REFERENCE.relative_to(ROOT).as_posix(), referencePixelsPerUnit=ppu,
        originalStandingCalibration=reference.get("pixelsPerUnitCalibration"),
        comparison="Rear head/nape, shoulders, seated torso and boots remain at the reference native scale",
        directStandingBodyInNewSource=False, measurementUncertaintyPercent=4,
        worldVisualScale=1.325, visualScaleCheckPending=True, humanApproval=False)
    supports = [[205,342],[580,341],[964,342],[1342,341],
                [181,662],[578,662],[955,662],[1348,662],
                [194,980],[578,980],[962,980],[1346,980]]
    durations = [350,250,180,350]
    directions = [3,4,5]
    job = dict(name=NAME,source=str(SOURCE),preservedSourceName=SOURCE.name,skin=0,weapon=3,form=0,
        columns=4,sourceRows=3,minimumArea=15000,
        rows=[dict(sourceRow=i,direction=d,action="Idle",durationMs=durations,loop=True) for i,d in enumerate(directions)],
        supportAnchors={str(i):dict(anchor=p,method="Whole-source tire-contact support, excludes protruding metallic blade tips")
                        for i,p in enumerate(supports)})
    pipeline.ART = ART
    pipeline.OUT = OUT
    manifest,_ = pipeline.prepare(job)
    inferred = manifest["pixelsPerUnit"]
    manifest.update(pixelsPerUnit=ppu,pixelsPerUnitCalibration=guide,humanApprovedSource=False,
                    coordinatorReviewPending=True,imported=False,runtimeVerified=False)
    clips = []
    for row,direction in enumerate(directions):
        frames = manifest["frames"][row*4:row*4+4]
        for frame,phase in zip(frames,["neutral","inhale","exhale","return"]):
            frame["phase"] = phase
        clips.append(dict(direction=direction,frames=[f["id"] for f in frames],durationMs=durations,loop=True))
    manifest["motorcycleIdle"] = dict(skin=0,form=0,clips=clips,otherDirectionsComplete=False,
        movingLocomotionComplete=False,fullBackFaceHidden=True,eyeMotionOnlyWhenGeometricallyVisible=True)
    manifest["notes"] = ["Twelve newly drawn whole rider-and-bike figures; NW/N/NE, four idle phases each.",
        "N shows nape and rear coat only, no face. Rear-quarter head and torso face the same direction.",
        "Breathing, subtle seated weight and cloth settling; no eye blink claimed where no eye is visible.",
        "No body-part montage, image scaling, palette conversion or alpha removal; selected source RGBA stays exact.",
        "Tire contacts are integer supports; metallic wheel blades do not set ground height or body PPU.",
        "Whole-vehicle inferred PPU was discarded in favor of the reference anatomical PPU: "+str(inferred),
        "Separate motorcycle idle descriptor; no shared catalog, runtime code or Unity import was changed.",
        "Coordinator review, in-world scale, actual playback and runtime verification remain separate states."]
    pipeline.write(pipeline.RES/(NAME+".json"),manifest)
    checks = pipeline.qa(NAME)
    after = {p.relative_to(ROOT).as_posix():pipeline.sha(p) for p in preserved}
    if before != after:
        raise RuntimeError("A previously prepared motorcycle source/atlas changed; coordinator must inspect")
    receipt = dict(date="2026-10-27",name=NAME,sourcePath=manifest["sourcePath"],sourceSha256=manifest["sourceSha256"],
        imageSha256=manifest["imageSha256"],manifestPath=(pipeline.RES/(NAME+".json")).relative_to(ROOT).as_posix(),
        nativeSize=manifest["nativeSize"],sourceSize=manifest["sourceSize"],frames=12,clips=3,
        directions=["NW","N","NE"],durationMs=durations,pixelsPerUnit=ppu,
        sourceRgbaExactReconstruction=manifest["sourceRgbaExactReconstruction"],qa=checks,
        generated=True,sourceVisuallyReviewedTechnical=True,packaged=True,coordinatorReviewPending=True,
        humanApproved=False,unityImported=False,runtimeVerified=False,consumerImplemented=False,
        existing24FiguresPreserved=True,preservedSha256=after,sharedCatalogEdited=False,runtimeCodeEdited=False)
    pipeline.write(OUT/"prepared.json",receipt)
    pipeline.write(OUT/"scale-guides.json",guide)
    pipeline.write(ART/"import-jobs.json",dict(jobs=[job]))
    print(json.dumps(receipt,ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
