"""Inspection artifacts only: never modify production sprite pixels."""
from pathlib import Path
import json,sys
from PIL import Image,ImageSequence
ROOT=Path(__file__).resolve().parents[3];OUT=ROOT/'outputs/retomada-20261031/moto'
name=sys.argv[1];m=json.loads((OUT/'staging'/(name+'.json')).read_text());im=Image.open(OUT/'staging'/m['image']).convert('RGBA')
for i in range(2,len(m['frames']),4):
    x,y,w,h=m['frames'][i]['rect'];piece=im.crop((x,y,x+w,y+h));bg=Image.new('RGBA',piece.size,(215,215,215,255));bg.alpha_composite(piece)
    bg.resize((w*2,h*2),Image.Resampling.NEAREST).save(OUT/(name+'-whole-contact-%02d-2x.png'%i))
audit=[]
for ci,clip in enumerate(m['clips']):
    path=OUT/'qa'/name/'preview'/('clip-%02d.gif'%ci)
    image=Image.open(path);frames=[];times=[]
    for frame in ImageSequence.Iterator(image):
        frames.append(frame.convert('RGBA'));times.append(frame.info.get('duration',0))
    strip=Image.new('RGBA',(sum(f.width for f in frames),max(f.height for f in frames)),(215,215,215,255));offset=0
    for frame in frames:strip.alpha_composite(frame,(offset,0));offset+=frame.width
    strip.save(OUT/(name+'-'+clip['name']+'-gif-phase-strip.png'))
    audit.append(dict(clip=clip['name'],frames=len(frames),durationMs=times,totalMs=sum(times),decoded=True,livePlaybackObserved=False))
(OUT/(name+'-gif-audit.json')).write_text(json.dumps(audit,indent=2))
print(json.dumps(audit))
