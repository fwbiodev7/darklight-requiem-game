"""Pack reviewed intact Requiem rider figures; Unity import is a separate coordinator step."""
import argparse
import json
from pathlib import Path

import prepare_weapon_redesign as base
from prepare_punhos_moto_20261025 import motorcycle_job

OUT = base.ROOT / "outputs/retomada-20261026/moto"
NAME = "RequiemMotoForm0Ride8SlamS20261026V1"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--guides", type=Path, required=True,
                        help="Coordinator-reviewed whole-source supportAnchors, pixelsPerUnit and anatomical calibration")
    args = parser.parse_args()
    guides = json.loads(args.guides.read_text(encoding="utf-8"))
    if len(guides.get("supportAnchors", [])) != 12 or guides.get("pixelsPerUnit", 0) <= 0:
        raise ValueError("Twelve reviewed supports and one positive anatomical PPU are required")
    if not guides.get("wholeSourceReviewed"):
        raise ValueError("Coordinator whole-source review must be recorded before packaging")
    job = motorcycle_job(args.source, form=0, guides=guides)
    job.update(name=NAME, skin=0, preservedSourceName=NAME+"-source.png")
    base.OUT = OUT
    manifest, _ = base.prepare(job)
    manifest["pixelsPerUnit"] = job["calibratedPixelsPerUnit"]
    manifest["pixelsPerUnitCalibration"] = guides
    for frame, duration in zip(manifest["frames"], [160]*8+[232,348,160,580]):
        frame["durationMs"] = duration
    directions = ["S", "SW", "W", "NW", "N", "NE", "E", "SE"]
    manifest["clips"] = [dict(name="ride_"+direction, frames=[manifest["frames"][i]["id"]],
                              loop=False, allowStatic=True) for i, direction in enumerate(directions)] + [
        dict(name="slam_S", frames=[f["id"] for f in manifest["frames"][8:]], loop=False, allowStatic=False)]
    manifest["requiredClips"] = [clip["name"] for clip in manifest["clips"]]
    manifest["motorcycle"] = dict(form=0, skin=0, ride=[f["id"] for f in manifest["frames"][:8]],
                                  slam=[f["id"] for f in manifest["frames"][8:]], slamStartup=.58,
                                  slamActive=.16, slamRecovery=.58, staticRideViews=True, otherActionsCoverage=False)
    manifest["notes"] = ["Complete rider and motorcycle figures retained together; no anatomy assembly or resizing.",
                         "Eight static advancing ride views and four Slam S phases only; no mounted idle cycle.",
                         "All selected nonzero-alpha source RGBA retained exactly; zero-alpha source RGB remains in original.",
                         "Point/uncompressed import required; Unity import and runtime verification are separate."]
    base.write(base.RES / (NAME+".json"), manifest)
    checks = base.qa(NAME)
    receipt = dict(name=NAME, frames=len(manifest["frames"]), sourcePath=manifest["sourcePath"],
                   sourceSha256=manifest["sourceSha256"], imageSha256=manifest["imageSha256"],
                   manifestPath=(base.RES/(NAME+".json")).relative_to(base.ROOT).as_posix(),
                   pixelsPerUnit=manifest["pixelsPerUnit"], exactSourceRgba=manifest["sourceRgbaExactReconstruction"],
                   sourceReviewed=True, packaged=True, unityImported=False, runtimeVerified=False,
                   idleCycleComplete=False, otherActionsComplete=False, qa=checks)
    base.write(OUT/"preparation-jobs.json", dict(jobs=[job]))
    base.write(OUT/"prepared.json", receipt)
    print(json.dumps(receipt, ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
