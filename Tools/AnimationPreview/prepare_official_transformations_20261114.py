"""Preserve approved RGB sheets; extract connected black matte without repainting figures."""
from pathlib import Path
import hashlib
import json
import shutil

import numpy as np
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[2]
SOURCES=ROOT/'ArtSources/OfficialTransformations20261114'
DEST=ROOT/'Assets/Darklight/Resources/Darklight/OfficialTransitions20261114'
SPEC=[
    ('FabiusRupturaOfficial20261114','Transformação de Fabius_ Ruptura.png',1.95,[100,150,150,200,200,150,150,150],'red'),
    ('FabiusSerafimOfficial20261114','Transformação do Serafim Carmesim.png',1.95,[350,450,550,1050,850,600,450,300],'red'),
    ('LuciusArconteOfficial20261114','Transformação de Lucius Arconte em Aura Verde intensificada.png',1.90,[350,400,550,900,800,600,350,250],'green'),
]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def gutter(mask,count,vertical):
    projection=mask.sum(axis=1 if vertical else 0)
    size=len(projection);cuts=[0]
    for i in range(1,count):
        center=round(size*i/count);radius=round(size/count*.12)
        options=range(center-radius,center+radius+1)
        cuts.append(min(options,key=lambda p:(int(projection[p-1:p+2].sum()),abs(p-center))))
    return cuts+[size]


def main():
    for folder in [SOURCES/'raw',SOURCES/'review',DEST]:folder.mkdir(parents=True,exist_ok=True)
    records=[]
    for name,filename,world_height,durations,palette in SPEC:
        original=Path('C:/Users/Usuario/Downloads')/filename
        preserved=SOURCES/'raw'/(name+'-source.png')
        if preserved.exists() and preserved.read_bytes()!=original.read_bytes():raise ValueError('Preserved source differs')
        if not preserved.exists():shutil.copyfile(original,preserved)
        rgb=Image.open(preserved).convert('RGB');pixels=np.asarray(rgb)
        # Only near-black pixels connected to the outside can become transparent.
        eligible=Image.fromarray(np.where(pixels.max(axis=2)<=2,255,0).astype('uint8')).copy()
        if eligible.getpixel((0,0))!=255:raise ValueError('No safe black border seed')
        ImageDraw.floodfill(eligible,(0,0),127,thresh=0)
        exterior=np.asarray(eligible)==127
        if exterior.sum()<pixels.shape[0]*pixels.shape[1]*.1:raise ValueError('Exterior mask was not extracted')
        # Recover the black matte's soft exterior fringe without recoloring RGB.
        # Enclosed dark garment/face pixels remain opaque; only the border-connected
        # low-luminance band is attenuated. This avoids opaque black glow halos.
        soft=Image.fromarray(np.where(pixels.max(axis=2)<=24,255,0).astype('uint8')).copy()
        ImageDraw.floodfill(soft,(0,0),127,thresh=0)
        soft_exterior=np.asarray(soft)==127
        opacity=np.where(soft_exterior,np.clip(pixels.max(axis=2)/24*255,0,255),255).astype('uint8')
        opacity[exterior]=0
        rgba=np.dstack([pixels,opacity])
        alpha=rgba[:,:,3];ys=gutter(alpha>0,2,True)
        frames=[];first_height=None;overlay=Image.fromarray(rgba);draw=ImageDraw.Draw(overlay)
        for row in range(2):
            top,bottom=ys[row:row+2];xs=gutter(alpha[top:bottom]>0,4,False)
            for col in range(4):
                index=row*4+col;left,right=xs[col:col+2];cell=rgba[top:bottom,left:right]
                h,w=cell.shape[:2];r,g,b=[cell[:,:,i].astype('int16') for i in range(3)]
                opaque=cell[:,:,3]>0
                # The reference's floor ring is the ground plane, NOT aerial boot soles.
                energy=((r>145)&(r>g*1.6)&(r>b*1.3)) if palette=='red' else ((g>135)&(g>r*1.3)&(g>b*1.25))
                energy&=opaque
                low=energy.copy();low[:int(h*.70)]=False
                counts=low.sum(axis=1)
                if index>0 and counts.max()>12:
                    peak=int(counts.argmax())
                    band=low[max(0,peak-7):min(h,peak+8)]
                    ey,ex=np.where(band)
                    ground_y=peak
                    anchor_x=float((np.quantile(ex,.02)+np.quantile(ex,.98))/2)
                else:
                    dark=opaque&(r<195)&(g<190)&(b<200)&((np.maximum(np.maximum(r,g),b)-np.minimum(np.minimum(r,g),b))<65)
                    center=dark[:,int(w*.3):int(w*.72)]
                    possible=np.where((center.sum(axis=1)>8)&(np.arange(h)>h*.50))[0]
                    if not len(possible):raise ValueError('No intact standing support')
                    ground_y=int(possible.max())
                    by,bx=np.where(dark[max(0,ground_y-30):ground_y+1])
                    anchor_x=float(np.median(bx))
                # Whole-sheet visual calibration: the floor plane remains fixed
                # while the approved aerial poses rise above it. Bright stars at
                # the bottom of a ring must not pull the root down per frame.
                ground_planes={'FabiusRupturaOfficial20261114':(379,814),
                               'FabiusSerafimOfficial20261114':(380,825),
                               'LuciusArconteOfficial20261114':(388,849)}
                local_centers={'FabiusRupturaOfficial20261114':228,
                               'FabiusSerafimOfficial20261114':223,
                               'LuciusArconteOfficial20261114':226}
                ground_y=ground_planes[name][row]-top
                anchor_x=local_centers[name]
                if index==0:
                    body=opaque&(r<225)&(g<215)&(b<230)
                    core=body[:,max(0,int(anchor_x-w*.18)):min(w,int(anchor_x+w*.18))]
                    rows=np.where(core.sum(axis=1)>5)[0]
                    first_height=ground_y-int(rows.min())
                frame={'id':f'Redesign_{name}_{index:02d}','rect':[left,top,right-left,bottom-top],
                       'pivot':[round(anchor_x/w,8),round((h-ground_y)/h,8)],'durationMs':durations[index],
                       'sourceGroundAnchor':[round(left+anchor_x,2),top+ground_y],
                       'groundAnchorMethod':'Floor energy-ring plane; standing sole/shadow for first frame. Baked aerial pose retained.',
                       'temporalIndex':index}
                frames.append(frame)
                draw.rectangle((left,top,right-1,bottom-1),outline=(0,180,240,255),width=1)
                draw.line((left,top+ground_y,right,top+ground_y),fill=(80,255,80,255),width=1)
                draw.ellipse((left+anchor_x-3,top+ground_y-3,left+anchor_x+3,top+ground_y+3),fill='yellow')
                draw.text((left+5,top+5),str(index),fill='white')
        ppu=round(first_height/world_height,6)
        image=Image.fromarray(rgba)
        png=DEST/(name+'.png');image.save(png)
        manifest={'schemaVersion':1,'image':png.name,'imageSha256':digest(png),
                  'sourcePath':preserved.relative_to(ROOT).as_posix(),'sourceSha256':digest(preserved),
                  'sourceSize':list(rgb.size),'nativeSize':list(rgb.size),'pixelsPerUnit':ppu,
                  'wholeFigures':True,'status':'visual-reviewed','frames':frames,
                  'rectOrigin':'top-left','temporalSequence':True,'directionCoverage':'one cinematic view, not eight directions',
                  'backgroundExtraction':'Exterior-connected max(R,G,B)<=2 removed; connected black-matte fringe <=24 receives proportional alpha; RGB unchanged and enclosed blacks retained.',
                  'bakedLevitation':True,'bakedAura':True,'durationMs':sum(durations)}
        (DEST/(name+'.json')).write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
        (SOURCES/'review'/(name+'-anchors.png')).write_bytes(b'')
        overlay.save(SOURCES/'review'/(name+'-anchors.png'))
        check=Image.new('RGBA',image.size,(50,53,58,255));check.alpha_composite(image)
        check.convert('RGB').save(SOURCES/'review'/(name+'-neutral.png'))
        records.append({'name':name,'sourceSha256':digest(preserved),'imageSha256':digest(png),
                        'frames':8,'pixelsPerUnit':ppu,'durationMs':sum(durations),'backgroundPixelsRemoved':int(exterior.sum()),
                        'foregroundRgbExact':bool(np.array_equal(rgba[:,:,:3],pixels)),
                        'imported':False,'runtimeVerified':False})
    (SOURCES/'manifest.json').write_text(json.dumps({'approval':'User official sheets 2026-11-14; supersedes all candidate transitions',
        'order':['Ruptura','Serafim Carmesim','Lucius Arconte'],'sheets':records},indent=2)+'\n',encoding='utf-8')
    print(json.dumps(records))


if __name__=='__main__':main()
