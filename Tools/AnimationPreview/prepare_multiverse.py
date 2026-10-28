"""Pack intact generated figures; never redraw, recolor or assemble anatomy."""
from pathlib import Path
import hashlib,json,shutil,sys
import numpy as np
from PIL import Image
from whole_figure_components import identify

ROOT=Path(__file__).resolve().parents[2]
SOURCES=ROOT/'ArtSources/Multiverse20261023'
DEST=ROOT/'Assets/Darklight/Resources/Darklight/Multiverse'
OUT=ROOT/'outputs/expansao-20261023'
GEN=Path('C:/Users/Usuario/.codex/generated_images/01a16a65-852e-7ed1-a865-2fb76d9bf2e7')
FILES={'MossDetonator':'exec-1c9b67c6-67ef-4b1b-be4a-e07982b45c62.png',
       'NeonRevenant':'exec-1f97c17b-0527-4119-9aab-2f878e5a860c.png',
       'AshRonin':'exec-ccecf83a-d825-4252-a90b-7102ee5b7056.png',
       'Relics':'exec-6d9f23a7-c11e-40e9-bbea-9683aaac4ef2.png',
       'MotorcycleSouth':'exec-cdffba86-3bbd-4346-9307-5087aa6dbde4.png',
       'Maelis':'exec-0d1c1215-0756-4adb-bfc6-29fb7a14ab76.png'}
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def pack(name,filename):
    source=SOURCES/filename
    if not source.exists():shutil.copy2(GEN/filename,source)
    im=Image.open(source).convert('RGBA');rgba=np.array(im)
    props=name=='Relics';motor=name=='MotorcycleSouth';npc=name=='Maelis'
    rows=3 if props or motor else 2;count=rows*4
    figures,owner=identify(im,rows=rows,columns=4,minimum_area=9000 if name=='Relics' else 15000,alpha_threshold=128)
    # Extend each intact silhouette's ownership through its original soft alpha.
    # This retains every nontransparent RGBA source pixel, including detached FX.
    missing=(rgba[:,:,3]>0)&(owner==0)
    yy,xx=np.where(missing);best=np.full(xx.shape,np.inf);assigned=np.zeros(xx.shape,dtype=np.uint8)
    for i,f in enumerate(figures):
        x0,y0,x1,y1=f['grid'];dx=np.maximum(np.maximum(x0-xx,xx-x1),0);dy=np.maximum(np.maximum(y0-yy,yy-y1),0)
        distance=dx*dx+dy*dy+((xx-(x0+x1)/2)**2+(yy-(y0+y1)/2)**2)*.00001
        closer=distance<best;best[closer]=distance[closer];assigned[closer]=i+1
    owner[yy,xx]=assigned
    frames=[];extents=[]
    for i,f in enumerate(figures):
        yy,xx=np.where(owner==i+1);x0,y0,x1,y1=int(xx.min()),int(yy.min()),int(xx.max()+1),int(yy.max()+1)
        anchor=[round(f['anchor'][0]),f['anchor'][1]]
        if props:anchor=[(f['grid'][0]+f['grid'][2])//2,f['grid'][3]]
        extents.append((anchor[0]-x0,anchor[1]-y0,x1-anchor[0],y1-anchor[1]))
        frames.append((x0,y0,x1,y1,anchor))
    left,top,right,bottom=[max(x[k] for x in extents)+8 for k in range(4)]
    cw,ch=left+right,top+bottom;atlas=Image.new('RGBA',(cw*4,ch*rows));meta=[];contact=Image.new('RGBA',(cw*4,ch*rows),(40,39,45,255))
    conserved=0
    for i,(x0,y0,x1,y1,anchor) in enumerate(frames):
        pixels=rgba[y0:y1,x0:x1].copy();mask=owner[y0:y1,x0:x1]==i+1;pixels[~mask]=0
        frame=Image.fromarray(pixels);cx=(i%4)*cw;cy=(i//4)*ch;dx=left-(anchor[0]-x0);dy=top-(anchor[1]-y0)
        atlas.paste(frame,(cx+dx,cy+dy));conserved+=int(np.count_nonzero(pixels[:,:,3]))
        meta.append(dict(id=f'{name}_{i:02}',rect=[cx,cy,cw,ch],pivot=[left/cw,bottom/ch],durationMs=160,
                         sourceRect=[x0,y0,x1-x0,y1-y0],sourceAnchor=anchor,
                         action='idle' if npc else ('anticipation' if i%2==0 else 'contact') if motor else 'prop' if props else ['idle','walk','walk','hurt','windup','contact','recovery','defeat'][i],
                         direction=['S','SW','W','NW','N','NE','E','SE'][i] if npc else 'S',skin=name,form='normal'))
    path=DEST/(name+'.png');atlas.save(path)
    contact.alpha_composite(atlas);contact.convert('RGB').save(OUT/(name+'-contact.png'))
    clips=[dict(name='props',frames=[name+f'_{i:02}' for i in range(12)],loop=False,allowStatic=False)] if props else [dict(name='walk_S',frames=[name+'_01',name+'_02'],loop=True,allowStatic=False),
        dict(name='attack_S',frames=[name+'_04',name+'_05',name+'_06'],loop=False,allowStatic=False),
        dict(name='hurt_S',frames=[name+'_03'],loop=False,allowStatic=True),dict(name='defeat_S',frames=[name+'_07'],loop=False,allowStatic=True)]
    if motor:clips=[dict(name=move,frames=[name+f'_{i*2:02}',name+f'_{i*2+1:02}'],loop=False,allowStatic=False) for i,move in enumerate(['infernal_saw_S','devastating_spin_S','vertical_slam_S','destroyer_wheel_S','brutal_ride_S','motor_execution_S'])]
    if npc:clips=[dict(name='idle_'+d,frames=[name+f'_{i:02}'],loop=True,allowStatic=True) for i,d in enumerate(['S','SW','W','NW','N','NE','E','SE'])]
    manifest=dict(schemaVersion=1,image=path.name,imageSha256=sha(path),nativeSize=list(atlas.size),sourceSize=list(im.size),
                  logicalResolution='unverified generated pixel clusters; no quantization',pixelsPerUnit=figures[0]['bodyHeight']/(2.2 if name=='MossDetonator' else 2.5),
                  status='visual-reviewed',frames=meta,clips=clips,requiredClips=[c['name'] for c in clips],
                  source=str(source.relative_to(ROOT)),sourceSha256=sha(source),
                  sourcePixels=int(np.count_nonzero(rgba[:,:,3])),packedPixels=conserved,
                  quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=3),
                  warnings=['Continuous RGBA palette and soft alpha; native indexed pixel art unverified.',
                            'Eight static directional views, not animated locomotion.' if npc else 'South-facing authored animation only; no claim of eight-view coverage.'])
    if conserved!=manifest['sourcePixels']:raise ValueError('Lost source pixels')
    if props:manifest['pixelsPerUnit']=160
    if motor:manifest['pixelsPerUnit']=263/1.99
    references=['Assets/Darklight/Art/Campaign/Enemies.png'] if not motor else ['Assets/Darklight/Resources/Darklight/AbyssPrototype/Join.png','Assets/Darklight/Resources/Darklight/AbyssPrototype/Form0Slam.png','ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png']
    manifest['references']=[dict(path=r,sha256=sha(ROOT/r)) for r in references];manifest['generator']='built-in image_gen'
    (DEST/(name+'.json')).write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    print(name,dict(frames=count,size=atlas.size,sourcePixels=conserved,sha256=sha(path)))
if __name__=='__main__':
    for d in (SOURCES,DEST,OUT):d.mkdir(parents=True,exist_ok=True)
    for name,filename in FILES.items():pack(name,filename)
