from pathlib import Path
import hashlib,json,shutil,subprocess
from PIL import Image
BASE=Path(__file__).resolve().parents[1]
OUT=BASE/'outputs/fechamento-fabius-20261103'
GEN=Path('C:/Users/Usuario/.codex/generated_images/01a1a117-ff4b-72b3-9c2a-e2735c75b8ff')
TOOL=BASE/'outputs/darklight-requiem-game/Tools/Skills/pixel-art-sprite-sheet-master/scripts/sprite_pipeline.py'
jobs=[('exec-ed712504-496b-4cfb-857b-799ca213ab07.png','MotoWalkS_SW_SE-proportion-candidate',['S','SW','SE']),
      ('exec-6831a425-86cc-4163-8378-8df62f55d664.png','MotoWalkNW_N_NE-proportion-candidate',['NW','N','NE'])]
for source,name,directions in jobs:
    folder=OUT/'moto-six-directions'/name;folder.mkdir(parents=True,exist_ok=True)
    dest=folder/(name+'.png');shutil.copy2(GEN/source,dest)
    w,h=Image.open(dest).size;frames=[];clips=[]
    for row,direction in enumerate(directions):
        ids=[]
        for col in range(4):
            i=row*4+col;x=col*w//4;y=row*h//3;cw=(col+1)*w//4-x;ch=(row+1)*h//3-y
            fid=f'Redesign_{name}_{i:02d}';ids.append(fid)
            frames.append(dict(id=fid,rect=[x,y,cw,ch],pivot=[.5,.1],durationMs=120,action='walk',direction=direction,skin='Requiem',form='Normal',phase=['contact_A_proposed','passing_A_proposed','contact_B_proposed','passing_B_proposed'][col]))
        clips.append(dict(name='walk_'+direction,frames=ids,loop=True,allowStatic=False))
    descriptor=dict(schemaVersion=1,image=dest.name,imageSha256=hashlib.sha256(dest.read_bytes()).hexdigest(),nativeSize=[w,h],sourceSize=[w,h],pixelsPerUnit=160,status='candidate',wholeFigures=True,frames=frames,clips=clips,requiredClips=[x['name'] for x in clips],quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=1.3),provisionalRegistration=True,integrationEligible=False,humanApproved=False,imported=False,runtimeVerified=False,notes='Whole-figure anatomical correction candidate. Pivots/PPU provisional; gait and final anatomical equivalence require review. Never replaces approved lateral V3.')
    meta=folder/(name+'.json');meta.write_text(json.dumps(descriptor,indent=2),encoding='utf-8')
    results=[]
    with (folder/'commands.log').open('w',encoding='utf-8') as log:
        for command,args in [('audit',['--manifest',str(meta),'--out',str(folder/'audit')]),('preview',['--manifest',str(meta),'--out',str(folder/'preview'),'--zoom','1']),('palette',['--image',str(dest),'--out',str(folder/'palette.json')]),('unity-json',['--manifest',str(meta),'--out',str(folder/'unity-slices-INSPECTION-ONLY.json')])]:
            p=subprocess.run(['python',str(TOOL),command,*args],stdout=log,stderr=subprocess.STDOUT);results.append(dict(command=command,exitCode=p.returncode))
    (folder/'command-results.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
    print(name,w,h,results)
fists=OUT/'punhos-sw/attempt-04-opposite-pair.png'
shutil.copy2(GEN/'exec-d826aa80-cd72-4004-b0a0-9122dd32a66e.png',fists)
(OUT/'punhos-sw/attempt-04-result.json').write_text(json.dumps({'state':'rejected-for-integration','image':fists.name,'sha256':hashlib.sha256(fists.read_bytes()).hexdigest(),'reason':'The generator switched back to the old leading leg despite the opposite-support whole figure reference. Not a completed walk.','wholeFigures':True,'imported':False,'runtimeVerified':False},indent=2),encoding='utf-8')
