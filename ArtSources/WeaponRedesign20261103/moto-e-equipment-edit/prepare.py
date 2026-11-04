from PIL import Image
from pathlib import Path
import json,hashlib
root=Path('.'); srcdir=root/'ArtSources/WeaponRedesign20261103/moto-e-equipment-edit'; out=root/'outputs/retomada-20261103/moto-e-equipment-edit'; out.mkdir(exist_ok=True,parents=True)
name='ReqMotoNormalWalkE20261103EquipmentEditV1'
sp=srcdir/(name+'-source.png'); im=Image.open(sp).convert('RGBA'); sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
atlas=Image.new('RGBA',(1860,423)); frames=[]; proof=[]; anchors=[(190,382),(196,382),(300,377),(218,379)]; phases=['contact_A','passing_A','contact_B','passing_B']; ppu=342/1.98
for i,(ax,ay) in enumerate(anchors):
 rect=[465*i,0,465,423]; tile=im.crop((rect[0],0,rect[0]+465,423)); atlas.paste(tile,(465*i,0))
 a=tile.getchannel('A'); bound=a.point(lambda v:255 if v>8 else 0).getbbox(); fid=f'Redesign_{name}_{i:02}'
 frames.append(dict(id=fid,rect=rect,pivot=[ax/465,1-ay/423],durationMs=100,action='walk',direction='E',skin='Requiem',form='Normal',phase=phases[i],sourceFrameIndex=i,sourceRect=rect,sourceAnchor=[465*i+ax,ay],sourcePackingAnchor=[465*i+ax,ay],sourceScale=1,visibleAlphaBoundsInFrame=list(bound),supportGuide=dict(anchor=[465*i+ax,ay],method='Manual vertical projection of pelvis to grounded-sole line; no anatomy/pixels changed.'),legReview=['Near leg leads; far leg trails; both contact low.','Near leg planted; far leg passes low behind.','Near leg trails; far leg leads; contact opposite half.','Far leg planted; near leg passes low behind.'][i]))
 proof.append(dict(frameId=fid,sourceRect=rect,rgbaSha256=hashlib.sha256(tile.tobytes()).hexdigest(),exactRGBA=True,resampled=False,mirrored=False,wholeFigure=True))
atlaspath=out/(name+'.png');atlas.save(atlaspath)
sourcePath=sp.as_posix(); sourceHash=sha(sp); atlasHash=sha(atlaspath)
refs=[]
for p in [root/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalWalkE20261102V11Complete.png',root/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqMotoNormalParry8Dirs20261103V1.png',srcdir/'edit-target-eight-whole-poses.png',srcdir/'equipment-only-whole-E-reference.png']:
 refs.append(dict(path=p.as_posix(),sha256=sha(p)))
d=dict(schemaVersion=1,image=atlaspath.name,imageSha256=atlasHash,nativeSize=list(atlas.size),sourceSize=list(im.size),pixelsPerUnit=ppu,framePixelsPerUnit=[ppu]*4,status='candidate',wholeFigures=True,sourcePath=sourcePath,sourceSha256=sourceHash,worldVisualScale=1.325,frames=frames,clips=[dict(name='walk_E',frames=[f['id'] for f in frames],loop=True,allowStatic=False)],requiredClips=['walk_E'],quality=dict(alphaThreshold=8,maxColors=200000,maxHeightRatio=1.1),logicalResolution='Native generated raster; logical pixel grid unverified',anatomyEdits=False,sourceScale=1,notes=['All selected figures are whole source cells; RGBA unchanged including low-alpha source dust.','Four source target poses retained by integrated image edit; zero resampling, mirror or anatomical assembly.','Uniform PPU calibrated to median native crown-to-ground body height342px / canonical1.98; WorldVisualScale1.325 preserved.','Second saw partially occluded by body/coat during arm-back phases, handle remains hand-held.','Not imported or runtime-verified.'])
(out/(name+'.json')).write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
pages=dict(schemaVersion=1,complete=True,status='candidate',integrationBlocked=True,pages=[dict(id=name+'_Walk_E',skin=0,weapon=3,form=0,direction=6,action='Walk',attackKey='',resourcePath='Darklight/WeaponRedesign20261023/'+name,imageSha256=atlasHash,sourcePath=sourcePath,sourceSha256=sourceHash,sourceSize=list(im.size),nativeSize=list(atlas.size),pixelsPerUnit=ppu,framePixelsPerUnit=[ppu]*4,status='candidate',wholeFigures=True,loop=True,spriteNames=[f['id'] for f in frames],sourceSpriteNames=[],durationMs=[100]*4,integrationBlocked=True,presentation='OnFoot')])
(out/(name+'-pages.json')).write_text(json.dumps(pages,ensure_ascii=False,indent=2),encoding='utf-8')
proofdoc=dict(sourcePath=sourcePath,sourceSha256=sourceHash,atlasSha256=atlasHash,wholeFramesOnly=True,selectedSourceIndexes=[0,1,2,3],unselectedSourceIndexes=[4,5,6,7],frames=proof,references=refs)
for i,f in enumerate(frames):
 x,y,w,h=f['rect']; tile=atlas.crop((x,y,x+w,y+h)); proof[i]['exactRGBA']=tile.tobytes()==im.crop((x,y,x+w,y+h)).tobytes()
(out/'whole-frame-rgba-proof.json').write_text(json.dumps(proofdoc,ensure_ascii=False,indent=2),encoding='utf-8')
(srcdir/'generation-job.json').write_text(json.dumps(dict(tool='image_gen.imagegen',mode='precise-object-edit',transparent_background=True,outputPath=sourcePath,sourceSha256=sourceHash,editTarget=refs[2],equipmentReference=refs[3]),indent=2),encoding='utf-8')
print(json.dumps(dict(sourceSha256=sourceHash,atlasSha256=atlasHash,frames=len(frames),ppu=ppu,allExact=all(f['exactRGBA'] for f in proof))))
