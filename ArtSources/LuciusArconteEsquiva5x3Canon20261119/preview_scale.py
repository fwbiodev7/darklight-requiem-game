from pathlib import Path
from PIL import Image,ImageDraw
import json
p=Path(__file__).resolve().parent
root=p.parents[1]
paths=[root/'Assets/Darklight/Resources/Darklight/LuciusArconte20261111/LuciusArconteDodgeCanonSSE20261119V1.json',p/'LuciusArconteDodge5PhaseNW20261119V1.json',p/'LuciusArconteDodge5PhaseNE20261119V1.json']
canvas=Image.new('RGBA',(1330,380),(42,42,45,255));draw=ImageDraw.Draw(canvas);index=0
for path,indices in zip(paths,[[0],[0,2,4],[0,2,4]]):
    desc=json.loads(path.read_text());image=Image.open(path.parent/desc['image']).convert('RGBA');factor=64*1.325/desc['pixelsPerUnit']
    for i in indices:
        f=desc['frames'][i];x,y,w,h=f['rect'];a=f['bodyAnchorPx']
        tile=image.crop((x,y,x+w,y+h));tile=tile.resize((round(w*factor),round(h*factor)),Image.Resampling.NEAREST)
        target=(index*190+105-round((a[0]-x)*factor),300-round((a[1]-y)*factor))
        canvas.alpha_composite(tile,target);draw.text((index*190+3,330),f['direction']+' frame'+str(i),fill='white');index+=1
canvas.save(p/'review-game-scale.png')
