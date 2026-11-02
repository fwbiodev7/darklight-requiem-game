"""Register twelve complete generated effects without changing source pixels."""
import hashlib
import json
from pathlib import Path
import shutil
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'ArtSources/ActionParticles20261101/ActionParticlesV1-source.png'
BANK=ROOT/'Assets/Darklight/Resources/Darklight/ActionParticles20261101'
OUT=ROOT/'outputs/retomada-20261101/particles'


def main():
    image=Image.open(SOURCE).convert('RGBA')
    if image.size!=(1536,1024):raise ValueError('Inspect new source dimensions before using these complete effect rectangles')
    BANK.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
    target=BANK/'ActionParticles.png'
    if target.exists() and target.read_bytes()!=SOURCE.read_bytes():raise ValueError('Do not overwrite existing effects')
    shutil.copy2(SOURCE,target)
    digest=hashlib.sha256(SOURCE.read_bytes()).hexdigest()
    frames=[];clips=[]
    names=('Dodge','Defense','Damage','Death')
    times=((30,70,70),(25,50,75),(20,50,65),(50,90,130))
    for row,name in enumerate(names):
        clip=[]
        for phase,ms in enumerate(times[row]):
            identity=f'Redesign_ActionParticle_{name}_{phase:02}'
            rect=[phase*512,row*256,512,256]
            frames.append(dict(id=identity,rect=rect,pivot=[.5,.5],durationMs=ms,
                               action=name,direction='Effect',contentSize=[512,256],sourceRect=rect))
            clip.append(identity)
        clips.append(dict(name=name,frames=clip,loop=False))
    sheet=dict(schemaVersion=1,image=target.name,imageSha256=digest,sourcePath=SOURCE.relative_to(ROOT).as_posix(),
               sourceSha256=digest,sourceSize=list(image.size),nativeSize=list(image.size),pixelsPerUnit=256,
               status='visual-reviewed',wholeFigures=True,frames=frames,clips=clips,
               review='Twelve complete neutral effects; restrained separate overlay, no character recoloring. Soft alpha/glow retained, not quantized or cleaned.',
               candidate=False,unityImported=False,runtimeVerified=False)
    (BANK/'ActionParticles.json').write_text(json.dumps(sheet,indent=2)+'\n',encoding='utf-8')
    palette=image.getcolors(image.width*image.height)
    colors=sorted(palette,reverse=True)[:64]
    (OUT/'palette.json').write_text(json.dumps({'mostFrequentRgba':colors,'uniqueRgba':len(palette)},indent=2)+'\n')
    previews=[]
    for row,name in enumerate(names):
        selected=[image.crop((phase*512,row*256,(phase+1)*512,(row+1)*256)) for phase in range(3)]
        path=OUT/(name+'.gif');selected[0].save(path,save_all=True,append_images=selected[1:],duration=list(times[row]),loop=0,disposal=2)
        previews.append(path.name)
    (OUT/'receipt.json').write_text(json.dumps({'figures':12,'clips':4,'sourceSha256':digest,
        'nativeRgbaPixelsUnchanged':True,'status':'visual-reviewed','unityImported':False,'runtimeVerified':False,
        'previews':previews,'softAlphaPreserved':True,'humanContinuousPlaybackObserved':False},indent=2)+'\n')
    print(json.dumps({'effects':4,'frames':12,'pixelsChanged':False,'sha256':digest}))


if __name__=='__main__':main()
