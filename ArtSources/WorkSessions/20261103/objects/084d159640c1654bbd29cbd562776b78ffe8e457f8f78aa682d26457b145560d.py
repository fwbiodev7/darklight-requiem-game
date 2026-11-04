from pathlib import Path
from PIL import Image, ImageDraw
import hashlib,json

OUT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba/outputs/continuacao-escola-20261103/moto-walk')
NAME='ReqMotoNormalWalkW_E20261103SchoolV2'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d): p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sources=[OUT/'ReqMotoNormalWalkW_E20261103V1-source.png',OUT/'ReqMotoNormalWalkW_E20261103V2-source.png']
images=[Image.open(p).convert('RGBA') for p in sources]
assert all(i.size==(1774,887) for i in images)
xs=[0,443,887,1330,1774];ys=[0,444,887]
composite=Image.new('RGBA',(1774,887));atlas=Image.new('RGBA',(1920,968))
frames=[];proof=[];cells=[];phases=['contact_A','passing_A_low','contact_B_opposite','passing_B_low']
for i in range(8):
    row,col=divmod(i,4); x,y=xs[col],ys[row];w,h=xs[col+1]-x,ys[row+1]-y
    source_index=0 if col<2 else 1
    cell=images[source_index].crop((x,y,x+w,y+h))
    composite.paste(cell,(x,y))
    # Shared inferred pelvis projection, constant common sole ground. Integer full-figure offset only.
    local_anchor=(221,400)
    offset=(19,20)
    atlas.paste(cell,(col*480+offset[0],row*484+offset[1]))
    canvas=Image.new('RGBA',(480,484));canvas.paste(cell,offset);cells.append(canvas)
    restored=atlas.crop((col*480+19,row*484+20,col*480+19+w,row*484+20+h))
    assert restored.tobytes()==cell.tobytes()
    frame_id=f'Redesign_{NAME}_{i:02}'
    frames.append(dict(id=frame_id,rect=[col*480,row*484,480,484],pivot=[240/480,1-420/484],durationMs=120,action='walk',direction=['W','E'][row],skin='Requiem',form='Normal',phase=phases[col],sourceFrameIndex=i,sourceRect=[x,y,w,h],sourceOffsetInFrame=list(offset),sourceScale=1,pixelsPerUnit=186.36363636363637,provenance=dict(source=sources[source_index].name,sourceSha256=sha(sources[source_index]),rect=[x,y,w,h]),supportGuide=dict(anchor=[x+221,y+400],method='Manual common sole ground and inferred pelvis projection; fixed camera; integer whole-frame registration',uncertaintyPixels=4)))
    proof.append(dict(frameId=frame_id,exactRgbaReconstruction=True,source=sources[source_index].name,pixelSha256=hashlib.sha256(cell.tobytes()).hexdigest(),preservedFromV1=col<2))
composite_path=OUT/(NAME+'-source.png');composite.save(composite_path)
atlas_path=OUT/(NAME+'.png');atlas.save(atlas_path)
meta=dict(schemaVersion=1,image=atlas_path.name,imageSha256=sha(atlas_path),nativeSize=list(atlas.size),sourceSize=list(composite.size),sourcePath=f'ArtSources/WeaponRedesign20261103/moto-walk-school/{composite_path.name}',sourceSha256=sha(composite_path),preservedSourceFile=composite_path.name,pixelsPerUnit=186.36363636363637,status='candidate',wholeFigures=True,frames=frames,clips=[dict(name='walk_'+d,frames=[f['id'] for f in frames[r*4:r*4+4]],loop=True,allowStatic=False) for r,d in enumerate(['W','E'])],requiredClips=['walk_W','walk_E'],quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=1.12),presentation='OnFoot',worldVisualScale=1.325,motorcycleContext='onFoot-DualSaws',anatomyEdits=False,sourceScale=1,sourceRgbaExactReconstruction=True,integrationBlocked=True,integrationBlockReason='School artifact only; home package reconciliation, PPU/world-scale calibration and actual Unity preview pending. ArtSources sourcePath is proposed and has not been installed.',logicalResolution='Native generated RGBA raster; deliberate indexed pixel grid unverified',pixelsPerUnitCalibration=dict(method='Provisional whole standing crown-to-sole against reviewed S 388px at 195.9595959596PPU; new side body 369px ->186.363636. One PPU all8, no resampling. Latest home package must resolve final anatomical scale.',reference='ReqMotoNormalWalkS20261103ClosedV1',referenceBodyHeight=388,sourceBodyHeight=369,uncertaintyPixels=5),review=dict(wholeSourceSeen=True,staticContactSeen=True,continuousMotionViewed=False,humanApproved=False,imported=False,runtimeVerified=False),notes=['Two original generations preserved. First two columns use V1 exact RGBA, final two columns V2 full figures. No body-part assembly, mirroring, recoloring, scaling or deformation.','Status candidate is retained until temporal inspection and home-package reconciliation; no automated artistic approval.','Importer-compatible schema/rects/pivots, but candidate status and missing proposed ArtSources path intentionally prevent direct Unity import.'])
write(OUT/(NAME+'.json'),meta)
write(OUT/'source-preservation-proof.json',dict(frames=proof,allExact=True,sourceFiles=[dict(path=p.name,sha256=sha(p),size=list(images[i].size)) for i,p in enumerate(sources)]))
contact=Image.new('RGB',(1920,1016),'#30343a');draw=ImageDraw.Draw(contact)
for i,cell in enumerate(cells):
    row,col=divmod(i,4);x,y=col*480,row*508
    contact.paste(cell,(x,y),cell);draw.line((x+10,y+420,x+470,y+420),fill='#7c8b9c');draw.line((x+240,y+410,x+240,y+430),fill='#ffd74e')
    draw.text((x+8,y+486),f'{["W","E"][row]} {i%4+1} {phases[i%4]}',fill='white')
contact.save(OUT/'contact-sheet-native.png');contact.resize((3840,2032),Image.Resampling.NEAREST).save(OUT/'contact-sheet-nearest2x.png')
for row,d in enumerate(['W','E']):
    seq=cells[row*4:row*4+4]
    seq[0].save(OUT/f'walk_{d}.apng',format='PNG',save_all=True,append_images=seq[1:],duration=[120]*4,loop=0,disposal=0,blend=0)
    for n in range(4):
        a=seq[n].copy();b=seq[(n+1)%4].copy();a.putalpha(a.getchannel('A').point(lambda v:v//2));b.putalpha(b.getchannel('A').point(lambda v:v//2))
        bg=Image.new('RGBA',a.size,'#30343a');bg=Image.alpha_composite(Image.alpha_composite(bg,a),b)
        bg.save(OUT/f'overlay_{d}_{n+1}-{(n+1)%4+1}.png')
print(json.dumps(dict(atlas=str(atlas_path),frames=8,clips=2,exactFrames=8,ppu=meta['pixelsPerUnit'])))
