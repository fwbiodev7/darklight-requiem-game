"""Measure intact nine-figure source; never edits source pixels or anatomy."""
import json,sys,hashlib,statistics
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[2]
SESSION=Path(__file__).resolve().parent
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
from integrate_session_lucius_20261114 import cut_axis,measure

def main():
    source=SESSION/'source-v1.png'
    im=Image.open(source).convert('RGBA');alpha=im.getchannel('A')
    ys=cut_axis(alpha,3,True);frames=[]
    contact=im.copy();draw=ImageDraw.Draw(contact)
    for row,(y0,y1) in enumerate(zip(ys,ys[1:])):
        xs=cut_axis(alpha.crop((0,y0,im.width,y1)),3,False)
        for col,(x0,x1) in enumerate(zip(xs,xs[1:])):
            rect=[x0,y0,x1-x0,y1-y0]
            anchor,height=measure(im,rect)
            bbox=alpha.crop((x0,y0,x1,y1)).getbbox()
            f={'index':row*3+col,'direction':['NW','N','NE'][row], 'phase':['anticipation','contact','recovery'][col], 'rect':rect,'anchorAuto':list(anchor),'bodyHeightAuto':height,'alphaBoundsLocal':bbox}
            frames.append(f)
            draw.rectangle((x0,y0,x1-1,y1-1),outline='cyan',width=2)
            draw.line((anchor[0]-10,anchor[1],anchor[0]+10,anchor[1]),fill='red',width=2)
            draw.text((x0+5,y0+5),str(f['index']),fill='white')
    data={'sourceSize':list(im.size),'mode':im.mode,'alphaRange':alpha.getextrema(),'sourceSha256':hashlib.sha256(source.read_bytes()).hexdigest(),'frames':frames}
    (SESSION/'source-measurements.json').write_text(json.dumps(data,indent=2)+'\n',encoding='utf-8')
    contact.save(SESSION/'source-anchors.png')
    print(json.dumps(data))
if __name__=='__main__':main()
