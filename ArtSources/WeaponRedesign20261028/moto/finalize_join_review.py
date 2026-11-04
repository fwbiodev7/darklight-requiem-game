from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw, ImageSequence

ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261028/moto'
NAME='ReqMotoNormalJoinSeparateS20261028V1'
STAGE=OUT/'staging'
QA=OUT/'qa'/NAME

def write(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()

manifest=json.loads((STAGE/(NAME+'.json')).read_text())
im=Image.open(STAGE/(NAME+'.png')).convert('RGBA')
gif_evidence=[]
for i,c in enumerate(manifest['clips']):
    gif=Image.open(QA/'preview'/('clip-%02d.gif'%i))
    decoded=[f.convert('RGBA') for f in ImageSequence.Iterator(gif)]
    durations=[]
    gif.seek(0)
    for f in ImageSequence.Iterator(gif):durations.append(f.info.get('duration'))
    strip=Image.new('RGBA',(decoded[0].width*len(decoded),decoded[0].height),(48,52,58,255))
    for k,f in enumerate(decoded):strip.paste(f,(k*f.width,0))
    strip.save(OUT/(c['name']+'-gif-phase-strip.png'))
    gif_evidence.append(dict(clip=c['name'],frames=len(decoded),durationMs=durations,
        totalMs=sum(durations),decoded=True,livePlaybackObserved=False))
for i in [0,3,6,7]:
    f=manifest['frames'][i];x,y,w,h=f['rect'];tile=im.crop((x,y,x+w,y+h))
    bg=Image.new('RGBA',tile.size,(236,236,236,255));bg.alpha_composite(tile)
    bg.resize((w*2,h*2),Image.Resampling.NEAREST).save(OUT/('whole-figure-%02d-2x-light.png'%i))
refs=json.loads((ART/'references.json').read_text(encoding='utf-8-sig'))
preservation=[dict(path=r['path'],sha256=r['sha256'],unchanged=sha(ROOT/r['path'])==r['sha256']) for r in refs]
if not all(r['unchanged'] for r in preservation):raise ValueError('Reference changed: inspect coordinator ownership')
write(OUT/'preservation-after.json',preservation)
write(OUT/'gif-audit.json',gif_evidence)
review=dict(date='2026-10-28',wholeSourceSeen=True,packedContactSeen=True,nativeAtlasSeen=True,
    nearest2xWholeFiguresSeen=True,decodedGifPhaseStripsSeen=True,livePlaybackObserved=False,
    browserAttempt=dict(result='blocked',reason='file protocol not permitted by browser URL security policy',workaroundAttempted=False),
    wholeFigures=True,preservedOriginalFrames=[0,1,2,4,5,8,9,10,11],repairedWholeFrames=[3,6,7],
    repairedReasons={'3':'SW changed to S front straddle; same head/body native scale.',
        '6':'Third central tire removed by replacing whole figure with exactly two hinged modules.',
        '7':'Adjacent rejected figure left a low-alpha tire trace in extraction; clean whole figure selected from repair2.'},
    checklist=dict(adultIdentityComparedToKatana=True,compactHair=True,longSleeves=True,neutralNormal=True,
        twoArmsTwoHands=True,twoWheelSawModules=True,frontSouthCamera=True,anatomyNotRepairedByPpu=True),
    limitations=['Fast six-phase assembly / seven-frame separation sharing mounted endpoint, S only; hinged side-by-side modules to front/back folded chassis needs runtime timing review.',
        'Native indexed grid unverified; palette and alpha warnings retained, no global cleaning.',
        'Live continuous animation preview blocked; decoded GIF phases/timing review does not certify runtime.'],
    humanApproval=False,imported=False,runtimeVerified=False)
write(OUT/'visual-review.json',review)
print(json.dumps(dict(gifs=gif_evidence,preservedReferences=len(preservation),imageSha256=manifest['imageSha256'])))
