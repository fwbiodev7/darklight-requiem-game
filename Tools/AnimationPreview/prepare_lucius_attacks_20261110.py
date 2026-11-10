"""Prepare reviewed complete horizontal figures; preserve native RGBA source pixels.

No Unity/catalog/runtime operations. Vertical stays candidate, not packaged.
"""
import hashlib,json,shutil
from pathlib import Path
from PIL import Image,ImageDraw

ROOT=Path(__file__).resolve().parents[2]
SRC=ROOT/'ArtSources/LuciusAttacks20261110'
OUT=ROOT/'outputs/lucius-20261110/attacks'
DEST=ROOT/'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
DIRS=['S','SW','W','NW','N','NE','E','SE']
TIMES=[140,120,220]
PPU=310/(302/(520/3.3))
SOLE_X=[[168,166,158],[156,163,158],[158,199,167],[134,207,159],
        [179,177,128],[190,183,134],[197,166,110],[192,168,124]]
SHEETS=[
 ('LuciusSwordHorizontalSouthWestV1','SwordHorizontal-S-SW-W-NW-candidate-v1.png',[0,1,2,3],
  [[(0,0,315,355),(340,0,390,355),(730,0,356,355)],
   [(0,355,315,355),(340,355,390,355),(730,355,356,355)],
   [(0,710,310,360),(310,710,405,360),(715,710,371,360)],
   [(0,1070,290,378),(290,1070,420,378),(710,1070,376,378)]]),
 ('LuciusSwordHorizontalNorthEastV1','SwordHorizontal-N-NE-E-SE-candidate-v1.png',[4,5,6,7],
  [[(0,0,330,375),(330,0,400,375),(730,0,356,375)],
   [(0,375,330,345),(330,375,400,345),(730,375,356,345)],
   [(0,720,320,340),(320,720,410,340),(730,720,356,340)],
   [(0,1060,320,388),(320,1060,400,388),(720,1060,366,388)]])]

def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def save(path,value):
 path.parent.mkdir(parents=True,exist_ok=True)
 path.write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def mask_bbox(im): return im.getchannel('A').point(lambda a:255 if a>=16 else 0).getbbox()
def foot_anchor(im):
 box=mask_bbox(im); left,top,right,bottom=box
 # Only the visible contact-band of both boots defines root; not sword/coat bbox.
 band=im.getchannel('A').crop((0,bottom-9,im.width,bottom)).point(lambda a:255 if a>=128 else 0)
 foot=band.getbbox()
 return ((foot[0]+foot[2])//2,bottom),box
def main():
 OUT.mkdir(parents=True,exist_ok=True);DEST.mkdir(parents=True,exist_ok=True)
 pages=[];review=[]
 for name,filename,directions,rows in SHEETS:
  source=SRC/filename;im=Image.open(source);target=DEST/(name+'.png')
  assert im.mode=='RGBA' and im.size==(1086,1448)
  shutil.copy2(source,target)
  frames=[];clips=[]
  for direction,rectangles in zip(directions,rows):
   crops=[];anchors=[];boxes=[];ids=[]
   for index,rect in enumerate(rectangles):
    x,y,w,h=rect;crop=im.crop((x,y,x+w,y+h));anchor,box=foot_anchor(crop)
    # Full-figure review corrected asymmetric-height soles: do not anchor one boot.
    anchor=(SOLE_X[direction][index],anchor[1])
    frameid=f'Redesign_{name}_{DIRS[direction]}_{index:02d}';ids.append(frameid)
    frames.append(dict(id=frameid,action='Attack',attackKey='SwordHorizontal',direction=DIRS[direction],index=index,
       rect=list(rect),pivot=[anchor[0]/w,(h-anchor[1])/h],durationMs=TIMES[index],phase=['startup','active','recovery'][index]))
    crops.append(crop);anchors.append(anchor);boxes.append(box)
   # Same native canvas and ground origin for inspection; no scaling/repainting.
   canvases=[];origin=(440,430);canvas_size=(880,480)
   for crop,anchor in zip(crops,anchors):
    canvas=Image.new('RGBA',canvas_size);canvas.alpha_composite(crop,(origin[0]-anchor[0],origin[1]-anchor[1]));canvases.append(canvas)
   contact=Image.new('RGB',(880*3,480),(58,58,65));draw=ImageDraw.Draw(contact)
   for index,canvas in enumerate(canvases):
    base=Image.new('RGBA',canvas_size,(58,58,65,255));base.alpha_composite(canvas)
    contact.paste(base.convert('RGB'),(880*index,0));draw.line((880*index,430,880*(index+1),430),fill=(90,105,95))
    draw.text((880*index+16,450),f'{DIRS[direction]} {index} '+['startup','active','recovery'][index],fill='white')
   contact.save(OUT/f'SwordHorizontal-{DIRS[direction]}-contact-native.png')
   gif=[]
   for canvas in canvases:
    bg=Image.new('RGBA',canvas_size,(58,58,65,255));bg.alpha_composite(canvas);gif.append(bg.convert('RGB'))
   gif[0].save(OUT/f'SwordHorizontal-{DIRS[direction]}.gif',save_all=True,append_images=gif[1:],duration=TIMES,loop=0,disposal=2)
   pages.append(dict(id=f'{name}_Attack_{DIRS[direction]}',action='Attack',attackKey='SwordHorizontal',direction=direction,
     resourcePath='Darklight/LuciusNormal20261109/'+name,imageSha256=sha(target),sourceSha256=sha(source),
     sourcePath=source.relative_to(ROOT).as_posix(),nativeSize=list(im.size),pixelsPerUnit=PPU,
     spriteNames=ids,durationMs=TIMES,startupFrames=1,activeFrames=1,loop=False,wholeFigures=True,status='visual-reviewed'))
   clips.append(dict(name=f'{name}_{DIRS[direction]}',frames=ids,loop=False,allowStatic=False))
   review.append(dict(direction=DIRS[direction],source=source.relative_to(ROOT).as_posix(),sourceSha256=sha(source),
    rectangles=[list(r) for r in rectangles],solePivotsLocal=anchors,visibleBoundsAlpha16=boxes,
    completeFigures=True,distinctPhases=True,noRootTravel=True,pivotBasis='center of visible boot contacts in last 9 opaque rows; reviewed against boots',
    pixelsResampled=False,phaseDurationsProvisional=True,stabilityReview='boot-contact frame-step review; common ground; stance flex only, no root travel',
    emotionRevision='pending stronger anger/drama requested after generation'))
  save(DEST/(name+'.json'),dict(schemaVersion=1,image=target.name,imageSha256=sha(target),sourcePath=source.relative_to(ROOT).as_posix(),
    sourceSha256=sha(source),sourceSize=list(im.size),nativeSize=list(im.size),pixelsPerUnit=PPU,wholeFigures=True,
    status='visual-reviewed',allowStatic=False,pivotMethod='complete-frame boot sole anchors, no anatomy edits',frames=frames,clips=clips,
    quality=dict(alphaThreshold=15),alphaReview='native RGBA retained; alpha1-4 magenta residue invisible over gray; threshold15 only affects audit, never source pixels'))
 save(OUT/'pages-horizontal.json',dict(schemaVersion=1,characterId='Lucius',form='Normal',weapon='Sword',complete=False,pages=pages))
 save(OUT/'horizontal-review.json',dict(schemaVersion=1,status='visual-reviewed',produced=True,reviewed=True,unityImported=False,runtimeVerified=False,
    sourcePixelsUnchanged=True,wholeFigures=True,bodyCalibration=dict(mainBodySHeightPx=310,targetPreScaleBodyWorld=302/(520/3.3),pixelsPerUnit=PPU,worldVisualScale=1.325,
    basis='hair-to-sole frontal body approx y25..335, excludes raised blade; common PPU for both horizontal sources; no per-frame scaling'),frames=review))
 print(json.dumps(dict(pages=len(pages),frames=len(pages)*3,pixelsPerUnit=PPU,unityImported=False,runtimeVerified=False)))

def main_v3(technique='SwordHorizontal',write_assets=True):
 import runpy
 component_fn=runpy.run_path(str(OUT/'inspect_components.py'))['components']
 selected=SRC/'selected';selected.mkdir(exist_ok=True)
 specifications=[('LuciusSwordHorizontalSouthWestV3','SwordHorizontal-S-SW-W-NW-candidate-v3.png',[0,1,2,3],450,312),
  ('LuciusSwordHorizontalNorthEastBackV3','SwordHorizontal-N-NE-E-SE-candidate-v2.png',[4,5],450,312),
  ('LuciusSwordHorizontalEastSouthEastV3','SwordHorizontal-E-SE-candidate-v3.png',[6,7],550,416)]
 if technique=='SwordVertical':
  specifications=[('LuciusSwordVerticalSouthWestV3','SwordVertical-S-SW-W-NW-overhead-repair-v3.png',[0,1,2,3],500,260),
   ('LuciusSwordVerticalNorthEastV3','SwordVertical-N-NE-E-SE-candidate-v3.png',[4,5,6,7],500,260)]
 short=technique.replace('Sword','').lower()
 destination=DEST if write_assets else OUT/('prepared-'+short+'-resources')
 destination.mkdir(parents=True,exist_ok=True)
 pages=[];provenance=[]
 for name,filename,directions,ch,body in specifications:
  source=SRC/filename;original=Image.open(source);groups=component_fn(original)
  groups.sort(key=lambda g:(round((g['bbox'][1]+g['bbox'][3])/2/(original.height/(4 if len(groups)==12 else 2))),g['bbox'][0]))
  # Sort explicitly by row, then x; varying sword heights cannot reorder directions.
  groups=sorted(groups,key=lambda g:(int((g['bbox'][1]+g['bbox'][3])/2//(original.height/(4 if len(groups)==12 else 2))),g['bbox'][0]))
  if technique=='SwordVertical' and directions==[0,1,2,3]:
   older=SRC/'SwordVertical-S-SW-W-NW-candidate-v3.png';oldgroups=component_fn(Image.open(older))
   oldgroups=[g for g in oldgroups if g['bbox'][0]>300]
   oldgroups.sort(key=lambda g:(int((g['bbox'][1]+g['bbox'][3])/2//(1448/4)),g['bbox'][0]))
   assert len(oldgroups)==8
   for row in range(4):
    for col in (1,2):
     g=oldgroups[row*2+col-1];g['originSource']=older;groups[row*3+col]=g
  if directions==[4,5]:groups=groups[:6]
  assert len(groups)==len(directions)*3
  cw=600;atlas=Image.new('RGBA',(cw*3,ch*len(directions)));frames=[];clips=[];ppu=body/(302/(520/3.3))
  for row,direction in enumerate(directions):
   ids=[];canvases=[];facts=[]
   for index,g in enumerate(groups[row*3:row*3+3]):
    frame_source=g.get('originSource',source);frame_original=Image.open(frame_source)
    mask=Image.new('L',frame_original.size);px=mask.load()
    for p in g['pixels']:px[p%frame_original.width,p//frame_original.width]=255
    # Technical extraction of a complete connected figure; never anatomy parts.
    from PIL import ImageChops,ImageFilter
    mask=mask.filter(ImageFilter.MaxFilter(7));whole=frame_original.copy()
    whole.putalpha(ImageChops.multiply(frame_original.getchannel('A'),mask))
    b=whole.getbbox();crop=whole.crop(b);assert crop.width<cw-10 and crop.height<ch-10
    # Opaque boot contacts, excluding thin diagonal blade/coat fringes.
    opaque=crop.getchannel('A').point(lambda a:255 if a>=128 else 0);v=opaque.getbbox();bottom=v[3]
    if technique=='SwordVertical':
     # Explicit boots: downward blade may finish below the feet and is not root.
     if directions==[0,1,2,3]:
      foot_y=([377,721,1071,1437] if index==0 else [375,720,1071,1437])[row]
     else:foot_y=[[417,417,417],[834,834,834],[1237,1237,1237],[1664,1665,1664]][row][index]
     bottom=foot_y-b[1]
    vals=[];p=opaque.load()
    for xx in range(crop.width):
     if sum(p[xx,yy]>0 for yy in range(max(0,bottom-60),bottom))>=20:vals.append(xx)
    assert vals;anchor=((min(vals)+max(vals))//2,bottom)
    xoff=(cw-crop.width)//2;yoff=ch-crop.height-12;atlas.alpha_composite(crop,(index*cw+xoff,row*ch+yoff))
    fid=f'Redesign_{name}_{DIRS[direction]}_{index:02d}';ids.append(fid)
    frames.append(dict(id=fid,action='Attack',attackKey=technique,direction=DIRS[direction],index=index,
      rect=[index*cw,row*ch,cw,ch],pivot=[(xoff+anchor[0])/cw,(ch-yoff-anchor[1])/ch],durationMs=TIMES[index],phase=['startup','active','recovery'][index]))
    canvas=Image.new('RGBA',(900,600));canvas.alpha_composite(crop,(450-anchor[0],550-anchor[1]));canvases.append(canvas)
    record=dict(direction=DIRS[direction],index=index,originalSource=frame_source.relative_to(ROOT).as_posix(),originalSourceSha256=sha(frame_source),
     originalWholeFigureRect=[b[0],b[1],b[2]-b[0],b[3]-b[1]],selectedRect=[index*cw,row*ch,cw,ch],
     originalSolePivot=[b[0]+anchor[0],b[1]+anchor[1]],pixelsResampled=False,
     extraction='complete connected figure alpha>=16, 3px fringe; native RGBA unchanged inside mask; no anatomy assembly',
     expression='closed mouth, subtle angled eyebrows' if direction not in (4,5) else 'face hidden; complete figure preserved',
     opaqueFigurePixelsPreserved=True)
    facts.append(record);provenance.append(record)
   contact=Image.new('RGB',(2700,600),(58,58,65))
   for index,canvas in enumerate(canvases):
    bg=Image.new('RGBA',canvas.size,(58,58,65,255));bg.alpha_composite(canvas);contact.paste(bg.convert('RGB'),(900*index,0))
   contact.save(OUT/f'{technique}V3-{DIRS[direction]}-contact.png')
   gifs=[]
   for canvas in canvases:
    bg=Image.new('RGBA',canvas.size,(58,58,65,255));bg.alpha_composite(canvas);gifs.append(bg.convert('RGB'))
   gifs[0].save(OUT/f'{technique}V3-{DIRS[direction]}.gif',save_all=True,append_images=gifs[1:],duration=TIMES,loop=0,disposal=2)
   clips.append(dict(name=f'{name}_{DIRS[direction]}',frames=ids,loop=False,allowStatic=False))
   pages.append(dict(id=f'{name}_Attack_{DIRS[direction]}',action='Attack',attackKey=technique,direction=direction,
     resourcePath='Darklight/LuciusNormal20261109/'+name,nativeSize=list(atlas.size),pixelsPerUnit=ppu,spriteNames=ids,
     durationMs=TIMES,startupFrames=1,activeFrames=1,loop=False,wholeFigures=True,status='visual-reviewed'))
  final=selected/(name+'-source.png');atlas.save(final);target=destination/(name+'.png');shutil.copy2(final,target)
  for page in pages:
   if page['resourcePath'].endswith('/'+name):page.update(imageSha256=sha(target),sourceSha256=sha(final),sourcePath=final.relative_to(ROOT).as_posix())
  save(destination/(name+'.json'),dict(schemaVersion=1,image=target.name,imageSha256=sha(target),sourcePath=final.relative_to(ROOT).as_posix(),sourceSha256=sha(final),
   sourceSize=list(atlas.size),nativeSize=list(atlas.size),pixelsPerUnit=ppu,status='visual-reviewed',wholeFigures=True,allowStatic=False,frames=frames,clips=clips,
   quality=dict(alphaThreshold=15),pivotMethod='opaque boot contact band, full figure extraction, no root travel or anatomy resampling'))
 save(OUT/f'pages-{short}-v3.json',dict(schemaVersion=1,characterId='Lucius',form='Normal',weapon='Sword',complete=False,pages=pages))
 save(OUT/f'{short}-v3-provenance.json',dict(schemaVersion=1,frames=provenance,wholeFigures=True,pixelsResampled=False,unityImported=False,runtimeVerified=False,
   supersedes='V3 closed mouth and slight eyebrow anger, bodily drama retained; horizontal N/NE full figures preserved from V2'))
 print(json.dumps(dict(pages=len(pages),frames=len(provenance),manifest=f'outputs/lucius-20261110/attacks/pages-{short}-v3.json',unityImported=False)))

if __name__=='__main__':
 import sys
 if '--vertical-v3' in sys.argv:main_v3('SwordVertical',False)
 elif '--v3' in sys.argv:main_v3()
 else:main()
