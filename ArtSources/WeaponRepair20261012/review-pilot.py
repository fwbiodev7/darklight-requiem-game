from pathlib import Path
from PIL import Image,ImageDraw
import json,math
root=Path(__file__).parent
before=root/'pilot/RequiemKatanaLightStage4-before.png';after=root/'pilot/RequiemKatanaLightStage4.png'
docs=[json.loads(p.with_name(p.stem+'Rects.json').read_text(encoding='utf-8-sig')) for p in [before,after]]
images=[Image.open(p).convert('RGBA') for p in [before,after]]
ratio=48/float(docs[0]['pixelsPerUnit'])*1.325
cw,ch=224,184
canvas=Image.new('RGB',(8*cw,8*ch),(43,45,49));draw=ImageDraw.Draw(canvas)
def put(target,dest,which,i):
    f=docs[which]['frames'][i];x,y,w,h=f['rect'];px,py=f['pivot']
    tile=images[which].crop((x,y,x+w,y+h));tile=tile.resize((round(w*ratio),round(h*ratio)),Image.Resampling.NEAREST)
    left=dest[0]+112-round(tile.width*px);top=dest[1]+171-round(tile.height*(1-py))
    target.paste(tile,(left,top),tile)
for i in range(32):
    for which in range(2):
        col=i%4*2+which;dest=(col*cw,i//4*ch);put(canvas,dest,which,i)
        draw.text((dest[0]+8,dest[1]+4),f'{i} '+['S','SW','W','NW','N','NE','E','SE'][i//4]+(' ANTES' if which==0 else 'DEPOIS'),fill=(220,220,220))
canvas.save(root/'pilot-before-after-game-scale.png')
anim=[]
for phase in range(4):
    frame=Image.new('RGB',(8*cw,2*ch),(43,45,49));draw=ImageDraw.Draw(frame)
    for direction in range(8):
        for which in range(2):
            dest=((direction%4*2+which)*cw,direction//4*ch);put(frame,dest,which,direction*4+phase)
            draw.text((dest[0]+6,dest[1]+4),['S','SW','W','NW','N','NE','E','SE'][direction]+(' ANTES' if which==0 else 'DEPOIS'),fill=(220,220,220))
    anim.append(frame)
anim[0].save(root/'pilot-before-after.gif',save_all=True,append_images=anim[1:],duration=[220,200,200,200],loop=0,disposal=2)
print('Technical game-scale review: 32 pairs + eight-direction animation, not a gameplay capture')
