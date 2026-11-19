from PIL import Image, ImageDraw
from pathlib import Path
import json,hashlib,uuid
root=Path(__file__).resolve().parent
stem='NeroFraturadoCombatS20261119'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
v1=root/'sources'/f'{stem}-v1.png'
v2=root/'sources'/f'{stem}-stepB-correction-v2.png'
a=Image.open(v1).convert('RGBA');b=Image.open(v2).convert('RGBA')
assert a.size==b.size==(1448,1086)
rects=[(0,0,362,543),(362,0,724,543),(724,0,1086,543),(1086,0,1448,543),(0,543,362,1086),(362,543,686,1086),(686,543,1086,1086),(1086,543,1448,1086)]
grounds=[(207,480),(563,484),(935,484),(1275,478),(190,1020),(535,1020),(885,1020),(1280,1020)]
labels=['Idle','StepA','StepB','Guard','Windup','LateAnticipation','ContactFollowthrough','Recovery']
durations=[300,180,180,300,432,168,250,220]
canvas=Image.new('RGBA',(2048,1280))
frames=[];proof=[]
for i,(r,g,label,ms) in enumerate(zip(rects,grounds,labels,durations)):
    source=b if i==2 else a
    crop=source.crop(r)
    offset=(256-(g[0]-r[0]),560-(g[1]-r[1]))
    assert offset[0]>=0 and offset[1]>=0 and offset[0]+crop.width<=512 and offset[1]+crop.height<=640
    cx=(i%4)*512;cy=(i//4)*640
    canvas.paste(crop,(cx+offset[0],cy+offset[1]))
    recovered=canvas.crop((cx+offset[0],cy+offset[1],cx+offset[0]+crop.width,cy+offset[1]+crop.height))
    exact=recovered.tobytes()==crop.tobytes();assert exact
    name=f'{stem}_{i:02d}'
    frames.append(dict(id=name,name=name,rect=[cx,cy,512,640],pivot=[.5,.125],durationMs=ms,action=label,direction='S',form='Fraturado',skin='Nero',source='sources/'+(v2.name if i==2 else v1.name),sourceRectTopLeft=[r[0],r[1],r[2]-r[0],r[3]-r[1]],sourceGroundTopLeft=list(g),packOffset=list(offset),bodyHeightPx=425 if i==0 else None))
    proof.append(dict(id=name,sourceUnalteredRGBA=exact,corrected=i==2,pixelsSha256=hashlib.sha256(crop.tobytes()).hexdigest()))
image_path=root/'staging'/f'{stem}.png'
canvas.save(image_path)
ppu=425*1.325/2.5394134611477823
manifest=dict(schemaVersion=1,image=image_path.name,imageSha256=sha(image_path),nativeSize=list(canvas.size),sourceSize=[1448,1086],pixelsPerUnit=ppu,status='visual-reviewed',frames=frames,clips=[dict(name='Idle_S',frames=[frames[0]['id']],loop=True,allowStatic=True),dict(name='Walk_S',frames=[frames[1]['id'],frames[2]['id']],loop=True,allowStatic=False),dict(name='Guard_S',frames=[frames[3]['id']],loop=True,allowStatic=True),dict(name='Attack_S',frames=[f['id'] for f in frames[4:]],loop=False,allowStatic=False)],requiredClips=['Idle_S','Walk_S','Guard_S','Attack_S'],quality=dict(alphaThreshold=7,maxHeightRatio=1.4),visualScale=1.325,worldBodyHeight=2.5394134611477823,calibration=dict(method='Manual hair-top to sole idle body, excludes sword and ribbons; one global PPU. Whole source RGBA crops translated at integer offsets only, never scaled or retouched.',idleHairTopSourceY=55,idleGroundSourceY=480,bodyHeightPx=425,groundInCellTopLeft=[256,560]),baseOrderedNames=[f['id']for f in frames[:4]],attackOrderedNames=[f['id']for f in frames[4:]],consumer=dict(runtimeKind='KaelFractured',baseSlots=['idle','right-leg-forward step','left-leg-forward step','guard'],attackSlots=['initial windup','late anticipation','decisive contact/followthrough','recovery'],attackTimingSeconds=[.60,.25,.22],windupSplit=[.72,.28],walkMode='distance driven at .36 world units per step; 180ms preview is illustrative only',events='Existing runtime triggers damage at start of attack slot2, not authored by PNG/JSON.',rootInstruction='Keep approved OfficialCast/Nero as runtime base[0]; atlas00 is only transition/reference candidate.'),states=dict(produced=True,visualReviewed=True,staged=True,copiedToAssets=False,unityImported=False,runtimeVerified=False),limitations=['Projected blade length varies by pose, noticeably shorter in down-left idle/steps than across-body guard/contact. No recolor or clipping; exact rigid blade length across poses is NOT certified.','Two anticipation poses intentionally differ subtly; no added VFX/warning arcs.','Generator has low alpha1..7 specks; retained, not threshold-erased. Audit uses alphaThreshold7 for visible-geometry warnings.','Only a frontal pilot with slight combat turn, not eight-direction coverage.','Still contact review completed; movement preview generated, no Unity/runtime test.'])
(root/'staging'/f'{stem}.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
(root/'whole-frame-provenance.json').write_text(json.dumps(dict(source1Sha256=sha(v1),source2Sha256=sha(v2),unchangedOriginalFrameCount=7,correctedWholeFrameIndices=[2],proof=proof),indent=2),encoding='utf8')
guid=uuid.uuid4().hex
parts=[f'fileFormatVersion: 2\nguid: {guid}\nTextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {{}}\n  serializedVersion: 13\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n    sRGBTexture: 1\n  isReadable: 0\n  streamingMipmaps: 0\n  textureFormat: 1\n  maxTextureSize: 2048\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 0\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  compressionQuality: 100\n  spriteMode: 2\n  spriteExtrude: 1\n  spriteMeshType: 1\n  alignment: 9\n  spritePivot: {{x: 0.5, y: 0.125}}\n  spritePixelsToUnits: {ppu}\n  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}\n  spriteGenerateFallbackPhysicsShape: 0\n  alphaUsage: 1\n  alphaIsTransparency: 1\n  textureType: 8\n  textureShape: 1\n  platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 2048\n    resizeAlgorithm: 0\n    textureFormat: -1\n    textureCompression: 0\n    compressionQuality: 100\n    crunchedCompression: 0\n    allowsAlphaSplitting: 0\n    overridden: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites:\n']
for i,f in enumerate(frames):
    x,y,w,h=f['rect'];sid=uuid.uuid4().hex
    parts.append(f"    - serializedVersion: 2\n      name: {f['id']}\n      rect:\n        serializedVersion: 2\n        x: {x}\n        y: {1280-y-h}\n        width: {w}\n        height: {h}\n      alignment: 9\n      pivot: {{x: 0.5, y: 0.125}}\n      border: {{x: 0, y: 0, z: 0, w: 0}}\n      customData:\n      outline: []\n      physicsShape: []\n      tessellationDetail: 0\n      bones: []\n      spriteID: {sid}\n      internalID: {21300000+i}\n      vertices: []\n      indices:\n      edges: []\n      weights: []\n")
parts.append('    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: '+uuid.uuid4().hex+'\n    internalID: 0\n    vertices: []\n    indices:\n    edges: []\n    weights: []\n    secondaryTextures: []\n    nameFileIdTable:\n')
for i,f in enumerate(frames): parts.append(f"      {f['id']}: {21300000+i}\n")
parts.append('  userData: staging-only-nero-fraturado-combat-body-calibrated\n  assetBundleName:\n  assetBundleVariant:\n')
(root/'staging'/f'{stem}.png.meta').write_text(''.join(parts),encoding='utf8')
sources=[]
for p in [Path('Docs/Darklight/References/CampanhaOficial20261119/Nero.png'),Path('Assets/Darklight/Resources/Darklight/OfficialCast/Nero.png')]:sources.append(dict(path=p.as_posix(),sha256=sha(p)))
(root/'references.json').write_text(json.dumps(sources,indent=2),encoding='utf8')
# Compact display only, uniformly reduced nearest neighbor; authoritative raster unchanged.
contact=Image.new('RGBA',(1024,700),(37,38,46,255))
for i,f in enumerate(frames):
    r=f['rect'];tile=canvas.crop((r[0],r[1],r[0]+r[2],r[1]+r[3])).resize((256,320),Image.Resampling.NEAREST)
    contact.alpha_composite(tile,((i%4)*256,(i//4)*350))
draw=ImageDraw.Draw(contact)
for i,label in enumerate(labels):draw.text(((i%4)*256+8,(i//4)*350+322),f'{i:02d} {label}',fill=(240,240,240,255))
contact.convert('RGB').save(root/'previews'/'contact-compact.png')
print(json.dumps(dict(atlasSize=list(canvas.size),ppu=ppu,guid=guid,pngSha256=sha(image_path),sourcePixelsPreserved=all(p['sourceUnalteredRGBA']for p in proof))))

