"""Pack the later human-approved V1 intact; no generation or shared-catalog edit."""
import json
import statistics

import prepare_weapon_redesign as pipeline
from whole_figure_components import identify
from PIL import Image

ROOT = pipeline.ROOT
NAME = "ReqKatanaNormalWalkSW_W20261026ApprovedV1"
SOURCE = ROOT/"ArtSources/WeaponRedesign20261026/katana-normal/ReqKatanaNormalWalkSW_W-v1-source.png"
OUT = ROOT/"outputs/retomada-20261026/katana-normal"
EXPECTED = "233f498a8e69f2ed2b2fe3b06baf9b069a4df82a069bb930b616e6ec2eed00e5"


def main():
    if pipeline.sha(SOURCE) != EXPECTED:
        raise ValueError("Approved source SHA differs; do not substitute V2 or regenerate")
    protected = ROOT/"Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json"
    before = pipeline.sha(protected)
    figures, _ = identify(Image.open(SOURCE),rows=4,columns=3,minimum_area=10000,alpha_threshold=128)
    if len(figures) != 12 or any(f["touchesExtractionEdge"] for f in figures):
        raise ValueError("Expected twelve complete uncut approved figures")
    # The whole-figure bounds start at hair and end at boot soles in this source;
    # the lowered katana never defines either vertical anatomical endpoint.
    measurements = [dict(sourceFrameIndex=f["index"],hairY=f["grid"][1],soleY=f["grid"][3],
                         anatomyHeight=f["grid"][3]-f["grid"][1]) for f in figures]
    ppu = statistics.median(m["anatomyHeight"] for m in measurements)/1.98
    supports = [[232,359],[570,363],[935,359],[238,703],[574,703],[934,703],
                [280,1040],[588,1043],[960,1043],[268,1389],[590,1384],[960,1389]]
    durations = [65,65,70,65,65,70]
    rows = [dict(sourceRow=0,sourceIndices=list(range(6)),direction=1,action="Walk",durationMs=durations,loop=True),
            dict(sourceRow=2,sourceIndices=list(range(6,12)),direction=2,action="Walk",durationMs=durations,loop=True)]
    job = dict(name=NAME,source=str(SOURCE),preservedSourceName=NAME+"-source.png",skin=0,weapon=0,form=0,
               columns=3,sourceRows=4,rows=rows,minimumArea=10000,
               supportAnchors={str(i):dict(anchor=p,method="Whole-source projected boot-support midpoint; no anatomical crop")
                               for i,p in enumerate(supports)})
    pipeline.OUT = OUT
    manifest,pages = pipeline.prepare(job)
    manifest.update(pixelsPerUnit=ppu,humanApprovedSource=True,approvedSourceVersion="V1",imported=False,runtimeVerified=False,
                    pixelsPerUnitCalibration=dict(method="Whole-source hair-to-sole anatomy, excludes lowered blade",measurements=measurements,
                                                   bodyHeightUnits=1.98,worldVisualScale=1.325,measurementUncertaintyPixels=4,
                                                   visualScaleCheckPending=True))
    manifest["notes"] += ["Explicit later human visual approval selects V1 despite the prior technical gait concern; V2 stays rejected.",
                           "Six phases per direction, 400 ms loop. No image editing, resizing, montage or regeneration.",
                           "Staging fragment only; shared catalog and runtime were not changed."]
    for page in pages:
        page.update(pixelsPerUnit=ppu,humanApprovedSource=True,imported=False,runtimeVerified=False)
    pipeline.write(pipeline.RES/(NAME+".json"),manifest)
    pipeline.write(OUT/"catalog-fragment.json",dict(schemaVersion=1,complete=False,stagingOnly=True,pages=pages))
    pipeline.write(ROOT/"ArtSources/WeaponRedesign20261026/katana-normal/import-jobs-approved-v1.json",dict(jobs=[job]))
    checks = pipeline.qa(NAME)
    after = pipeline.sha(protected)
    if before != after:
        raise RuntimeError("Shared catalog changed during staging; coordinator must inspect concurrent work")
    receipt = dict(name=NAME,sourcePath=manifest["sourcePath"],sourceSha256=manifest["sourceSha256"],
                   imageSha256=manifest["imageSha256"],manifestPath=(pipeline.RES/(NAME+".json")).relative_to(ROOT).as_posix(),
                   frames=12,clips=2,directions=["SW","W"],phasesPerDirection=6,loopMs=400,durationMs=durations,
                   pixelsPerUnit=ppu,humanApprovedSource=True,sourceRgbaExactReconstruction=manifest["sourceRgbaExactReconstruction"],
                   qa=checks,packaged=True,unityImported=False,runtimeVerified=False,sharedCatalogUnchanged=before==after,
                   catalogSha256=after,rejectedV2Published=False)
    pipeline.write(OUT/"prepared.json",receipt)
    print(json.dumps(receipt,ensure_ascii=False))
    return 2 if checks["errors"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
