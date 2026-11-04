from pathlib import Path
import json,hashlib
from PIL import Image,ImageDraw
p=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba/outputs/continuacao-escola-20261103/moto-walk')
repo=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba/work/darklight-requiem-game')
refs=['ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/ReqMotoNormalWalkSW_SE20261103ClosedV2.png','ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/ReqMotoNormalWalkS20261103ClosedV1.png','ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/ReqKatanaNormalCarregadoW_NW20261102V2.png','ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/ReqKatanaNormalWalkNE_E_SE20261102WeaponRepairSelectedCompact.png','ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png']
(p/'reference-hashes.json').write_text(json.dumps([dict(path=r,sha256=hashlib.sha256((repo/r).read_bytes()).hexdigest(),wholeImageInspected=True) for r in refs],indent=2))
pal=json.loads((p/'palette-final.json').read_text());sw=Image.new('RGB',(512,512),'white');dr=ImageDraw.Draw(sw)
for i,e in enumerate(pal['entries'][:256]):
    x,y=i%16*32,i//16*32;dr.rectangle((x,y,x+31,y+31),fill=tuple(e['rgba'][:3]))
sw.save(p/'palette-top256.png')
metrics={}
for f in ['walk_W.apng','walk_E.apng']:
    im=Image.open(p/f);dur=[]
    for i in range(im.n_frames): im.seek(i);dur.append(im.info.get('duration'))
    metrics[f]=dict(nFrames=im.n_frames,canvas=list(im.size),durationMs=dur,loop=im.info.get('loop'),decoded=True,continuousMotionViewed=False)
(p/'apng-validation.json').write_text(json.dumps(metrics,indent=2))
print(json.dumps(metrics))
