"""Promote inspected metadata only. Source/image/rect/pivot/timing stay unchanged."""
from pathlib import Path
import json,sys,hashlib
ROOT=Path(__file__).resolve().parents[3];OUT=ROOT/'outputs/retomada-20261031/moto';STAGE=OUT/'staging'
name=sys.argv[1]
def read(path):return json.loads(path.read_text(encoding='utf-8'))
def write(path,value):path.write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
path=STAGE/(name+'.json');m=read(path);before=sha(path)
assert sha(STAGE/m['image'])==m['imageSha256']
assert sha(ROOT/m['sourcePath'])==m['sourceSha256']
m['status']='visual-reviewed'
m['review'].update(reviewDate='2026-10-31',wholeSourceSeen=True,contactSeen=True,
    nativeAtlasSeen=True,nearest2xWholeFiguresSeen=True,decodedGifPhasesSeen=True,
    humanApproval=False,livePlaybackObserved=False,unityImported=False,runtimeVerified=False,
    checklist=dict(adultIdentityComparedToKatana=True,compactBrownHair=True,neutralNormal=True,
        longSleeves=True,exactlyTwoArmsTwoHands=True,exactlyTwoWheelSaws=True,wholeFigures=True,
        realDirectionalViews=True,independentlyDrawnNotMirrored=True,
        sourceRgbaPreserved=True,anatomyNotRepairedByPpu=True),
    limitations=['Native generated raster, indexed pixel grid unverified. Palette and partial-alpha warnings retained.',
        'Offline decoded GIF phase/timing inspection, not live animation playback.',
        'Unity import and runtime remain coordinator-owned and pending.'])
write(path,m)
pages=OUT/(name+'-pages.json');p=read(pages)
for page in p['pages']:page.update(status='visual-reviewed',startupFrames=2,activeFrames=1)
write(pages,p)
rpath=OUT/(name+'-receipt.json');r=read(rpath);r.update(status='visual-reviewed',visualReviewed=True,
    reviewManifestSha256=sha(path),reviewMetadataOnly=True,qaManifestBeforeReviewSha256=before,
    qaAppliesToUnchangedImageRectsPivotsAndTiming=True)
write(rpath,r)
print(json.dumps(dict(name=name,status=r['status'],imageSha256=m['imageSha256'],qa=r['qa'],pages=str(pages.relative_to(ROOT)))))
