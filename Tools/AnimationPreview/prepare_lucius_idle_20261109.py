"""Package eight reviewed whole Lucius views without changing their pixels."""
import hashlib,json,shutil
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSources/LuciusNormal20261109/selected/LuciusNormalIdle8Final-source.png'
REVIEW=ROOT/'outputs/retomada-20261109/resume/lucius-final-blend-review.json'
DEST=ROOT/'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
NAME='LuciusNormalIdle8V1'
DIRS=['S','SW','W','NW','N','NE','E','SE']
PIVOTS=[(235,466),(205,466),(183,466),(154,466),(215,468),(188,469),(174,470),(204,473)]

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def save(path,value):
    content=(json.dumps(value,ensure_ascii=False,indent=2)+'\n').encode()
    if not path.exists() or path.read_bytes()!=content:path.write_bytes(content)

def main():
    review=json.loads(REVIEW.read_text(encoding='utf-8'))
    assert review['states']['importReady'], 'Visual review not ready for import'
    assert review['sourceSha256Reviewed']==sha(SOURCE), 'Reviewed source changed'
    image=Image.open(SOURCE)
    assert image.mode=='RGBA' and image.size==(1536,1024)
    assert image.getchannel('A').getextrema()[0]==0
    DEST.mkdir(parents=True,exist_ok=True)
    target=DEST/(NAME+'.png')
    if not target.exists() or sha(target)!=sha(SOURCE):shutil.copy2(SOURCE,target)
    density=review['calibration']['pixelsPerUnit']
    pivots=[frame['footPivotTopLeft'] for frame in sorted(review['frames'],key=lambda f:f['index'])]
    frames=[];pages=[]
    for index,direction in enumerate(DIRS):
        sprite='Redesign_'+NAME+'_'+str(index).zfill(2)
        frames.append(dict(id=sprite,action='Idle',direction=direction,index=0,
            rect=[index%4*384,index//4*512,384,512],
            pivot=[pivots[index][0]/384,(512-pivots[index][1])/512],durationMs=350))
        pages.append(dict(id=NAME+'_Idle_'+direction,action='Idle',direction=index,
            resourcePath='Darklight/LuciusNormal20261109/'+NAME,imageSha256=sha(target),
            sourceSha256=sha(SOURCE),sourcePath=SOURCE.relative_to(ROOT).as_posix(),
            nativeSize=list(image.size),pixelsPerUnit=density,spriteNames=[sprite],
            durationMs=[350],loop=True,wholeFigures=True,status='visual-reviewed'))
    save(DEST/(NAME+'.json'),dict(schemaVersion=1,image=target.name,imageSha256=sha(target),
        sourcePath=SOURCE.relative_to(ROOT).as_posix(),sourceSha256=sha(SOURCE),
        sourceSize=list(image.size),nativeSize=list(image.size),pixelsPerUnit=density,
        wholeFigures=True,status='visual-reviewed',allowStatic=True,
        pivotMethod='sole anchors reviewed on complete figures; no anatomy resampling',frames=frames))
    save(DEST/'catalog.json',dict(schemaVersion=1,characterId='Lucius',form='Normal',weapon='Sword',
        complete=False,pages=pages))
    save(ROOT/'outputs/retomada-20261109/resume/lucius-idle-package.json',dict(
        source=SOURCE.relative_to(ROOT).as_posix(),sourceSha256=sha(SOURCE),
        imageSha256=sha(target),sheet=target.relative_to(ROOT).as_posix(),
        staticViews=8,animatedCycles=0,pixelsPerUnit=density,bodyHeightPixels=review['calibration']['mainBodySHeightPx'],
        targetUnscaledBodyHeight=302/(520/3.3),visualScale=1.325,sourcePixelsUnchanged=True,
        visualReviewed=True,unityImported=False,runtimeVerified=False,campaignIntegrated=False))
    print(json.dumps(dict(staticViews=8,sourcePixelsUnchanged=True,pixelsPerUnit=density)))

if __name__=='__main__':main()
