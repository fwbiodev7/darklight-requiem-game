"""Pack twelve complete Presas1 Ruptura figures; coordinator owns shared catalog."""
import json
from pathlib import Path
import prepare_weapon_redesign as pipeline

ROOT=Path(__file__).resolve().parents[2]
CANDIDATES=ROOT/'ArtSources/WeaponRedesign20261025/candidates'
STEM='ReqDaggersRupturaPresas1S_SW_W20261025V4'
ANCHORS=[[202,355],[591,356],[954,355],[1335,356],
         [201,684],[604,674],[968,678],[1370,678],
         [201,995],[591,983],[998,990],[1350,985]]
# S recovery in the approved Normal source: crown70 -> sole357 =287px.
# Same comparable S recovery in the corrective Ruptura source:46 ->356 =310px.
# Use one sheet-wide PPU preserving canonical world anatomy, not blade/FX height.
NORMAL_PPU=143.93939393939394
PPU=310/(287/NORMAL_PPU)

def main():
    normal=json.loads((ROOT/'ArtSources/WeaponRedesign20261024/candidates/import-jobs-presas-v2-20261025.json').read_text(encoding='utf-8'))['jobs'][0]
    job=dict(name=STEM,source=str(CANDIDATES/(STEM+'-source.png')),skin=0,weapon=1,form=1,
             columns=4,sourceRows=3,minimumArea=15000,rows=normal['rows'],
             supportAnchors={str(i):dict(anchor=anchor,method='Inspected projected midpoint between both boot supports; baseline at lower boot sole, not raised blade bounds.') for i,anchor in enumerate(ANCHORS)})
    pipeline.write(CANDIDATES/'import-jobs-presas-ruptura-20261025.json',dict(jobs=[job]))
    manifest,pages=pipeline.prepare(job)
    # A single anatomical scale per sheet, excluding the overhead dagger.
    manifest['pixelsPerUnit']=PPU
    for page in pages:page['pixelsPerUnit']=PPU
    for i,frame in enumerate(manifest['frames']):
        frame['durationMs']=[30,45,38,38][i%4]
        frame['phase']=['anticipation','contact','follow-through','recovery'][i%4]
    manifest['calibrationGuide']=dict(method='Comparable S recovery crown-to-sole, weapon/coat/FX excluded. One PPU for the whole sheet, no per-frame deformation.',referencePixels=287,referencePixelsPerUnit=NORMAL_PPU,referenceWorldBodyHeight=287/NORMAL_PPU,sourcePixels=310,referenceCrownY=70,referenceSoleY=357,sourceCrownY=46,sourceSoleY=356,sourceFrameIndex=3,pixelsPerUnit=PPU,uncertaintyPx=6,unityCameraCheckPending=True)
    manifest['timingNote']='Exact unchanged sourceSpriteNames retain runtime timing; preview rounds29.75ms startup to30ms.'
    references=[ROOT/'ArtSources/WeaponRedesign20261024/candidates/ReqDaggersNormalLight1S_SW_W20261024v2-source.png',ROOT/'ArtSources/WeaponRedesign20261023/references/equipment-four-forms.png',ROOT/'ArtSources/WeaponRedesign20261025/references/fabius-transformations-motorcycle.png']
    manifest['provenance']=dict(generator='built-in image_gen',promptFile=(CANDIDATES/(STEM+'-prompt.txt')).relative_to(ROOT).as_posix(),references=[dict(path=p.relative_to(ROOT).as_posix(),sha256=pipeline.sha(p)) for p in references])
    manifest['review']=dict(sourceContactSeen=True,packedContactSeen=False,motionPreviewSeen=False,visualApproval=False,hairColorPatternUserApproved=True,hairColorApprovalScope='Saturated crimson streak color only; increased hair volume was explicitly rejected. V4 redraw restores short canonical Normal cut, no long nape.',unityImported=False,unityRuntimeVerified=False,findings='V4 corrects entire twelve affected figures using whole Normal source for short haircut/silhouette/body. No dangling nape; saturated red colors stay within short locks. Two fanged crimson/silver daggers held individually. Dark-red eyes and clean skin retained. No source tinting, mirroring, anatomical assembly or resizing. Generated corrective source is not pixel-identical to V2/V3; exact RGBA preservation applies final V4 source to atlas.',scope='Requiem/Ruptura/Presas1/S,SW,W only. Other52 produced attack/idle figures are HELD candidates for hair-volume review and are not integrated.')
    pipeline.write(pipeline.RES/(STEM+'.json'),manifest)
    pipeline.write(CANDIDATES/(STEM+'-manifest.json'),manifest)
    fragment=ROOT/'outputs/redesign-armas-20261025/presas-ruptura-pages.json'
    pipeline.write(fragment,dict(complete=False,pages=pages))
    qa=pipeline.qa(STEM)
    pipeline.write(ROOT/'outputs/redesign-armas-20261025/presas-ruptura-preparation.json',dict(name=STEM,frames=12,pages=3,pixelsPerUnit=PPU,nativeSize=manifest['nativeSize'],exactRgba=manifest['sourceRgbaExactReconstruction'],qa=qa,unityImported=False,runtimeVerified=False,audio=False))
    print(json.dumps(dict(frames=12,pages=3,pixelsPerUnit=PPU,qa=qa,fragment=fragment.relative_to(ROOT).as_posix())))

if __name__=='__main__':main()
