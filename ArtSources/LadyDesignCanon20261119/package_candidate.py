"""Technical packaging only: whole-figure crop, nearest comparison, metadata."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json, shutil, uuid

root = Path(__file__).resolve().parent
source = Path('C:/Users/Usuario/Downloads/Lyra Gótica com Pistola e Grimório.png')
fabius = Path('C:/Users/Usuario/Documents/Codex/2026-10-10/f/work/lyra-piloto-20261119/Fabius-Normal-S.png')
generated = Path('C:/Users/Usuario/.codex/generated_images/01a1f5bf-9577-74d0-b04b-68cb07b1200b/exec-cc858598-5376-4653-b7e8-55d963007883.png')
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
for name, p in [('Lady-Reference-Exact.png',source),('Lady-Idle-S-raw.png',generated),('Fabius-Normal-S-reference.png',fabius)]:
    shutil.copy2(p, root/name)
raw = Image.open(generated).convert('RGBA')
visible = raw.getchannel('A').point(lambda v:255 if v>=8 else 0).getbbox()
crop = (visible[0]-16,visible[1]-16,visible[2]+16,visible[3]+16)
candidate = raw.crop(crop)
out = root/'Lady-Idle-S-candidate-v1.png'
candidate.save(out)
assert candidate.tobytes() == raw.crop(crop).tobytes()
height = visible[3]-visible[1]
ppu = height/302*157.5757576
pivot = [(610-crop[0])/candidate.width,(crop[3]-1140)/candidate.height]
guid = uuid.uuid5(uuid.NAMESPACE_URL,'darklight:LadyDesignCanon20261119:IdleS:candidate-v1').hex
meta = f'''fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  streamingMipmaps: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 9
  spritePivot: {{x: {pivot[0]}, y: {pivot[1]}}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  textureType: 8
  textureShape: 1
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 100
    crunchedCompression: 0
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {uuid.uuid5(uuid.NAMESPACE_URL,guid).hex}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  userData: Lady staging candidate; not imported or runtime verified
  assetBundleName:
  assetBundleVariant:
'''
out.with_suffix('.png.meta').write_text(meta,encoding='utf-8')
manifest = {'schemaVersion':1,'image':out.name,'imageSha256':sha(out),'nativeSize':list(candidate.size),'pixelsPerUnit':ppu,'status':'visual-reviewed-candidate','frames':[{'id':'Lady_Idle_S_00','rect':[0,0,*candidate.size],'pivot':pivot,'durationMs':1000,'action':'idle','direction':'S','skin':'Lady','form':'normal'}],'clips':[{'name':'Lady_Idle_S','frames':['Lady_Idle_S_00'],'loop':True,'allowStatic':True}],'requiredClips':['Lady_Idle_S'],'quality':{'alphaThreshold':8,'maxColors':candidate.width*candidate.height,'maxHeightRatio':1.3},'provenance':{'referenceImage':'Lady-Reference-Exact.png','referenceSha256':sha(source),'rawImage':'Lady-Idle-S-raw.png','rawSha256':sha(generated),'tool':'integrated imagegen','operation':'background extraction; generated pixels differ from source; technical whole-figure crop only afterward','rawCropTopLeftXYXY':list(crop),'cropPixelsUnmodified':True},'calibration':{'bodyVisibleAlphaThreshold':8,'bodyHeightPx':height,'fabiusBodyHeightPx':302,'fabiusPPU':157.5757576,'visualScale':1.325,'bodyWorldHeight':height/ppu*1.325,'supportRawXY':[610,1140],'rawSize':list(raw.size),'rawVisibleBounds':list(visible),'previewBodyTargetPx':302,'note':'PPU calibrates whole figure uniformly; no anatomical resizing. Legacy AddProp rescales by entire canvas and must be handled separately.'},'states':{'produced':True,'visualReviewed':True,'integrated':False,'imported':False,'runtimeVerified':False,'published':False},'limits':['single front-facing idle prototype, no movement or directional coverage','alpha 1..7 faint generated residue remains; visible bounds evaluated at alpha>=8','not exact source pixels: integrated background extraction repainted fine detail; exact original preserved','not Unity-imported or tested; no Assets/runtime/catalog edits']}
(root/'Lady-Idle-S-candidate-v1.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')

# Whole sprites uniformly scaled for QA, never anatomy slices or transplanting.
figures = [('Fabius Normal',Image.open(fabius),[53,26,386,333],302),('Lady candidata',candidate,[16,16,candidate.width-16,candidate.height-16],height)]
preview = Image.new('RGBA',(720,380),(43,45,52,255))
d = ImageDraw.Draw(preview)
d.line((12,344,708,344),fill=(150,150,155),width=1)
for idx,(label,im,rect,bodyheight) in enumerate(figures):
    complete = im.crop(tuple(rect))
    scaled = complete.resize((round(complete.width*302/bodyheight),round(complete.height*302/bodyheight)),Image.Resampling.NEAREST)
    x=idx*360+(360-scaled.width)//2
    preview.alpha_composite(scaled,(x,344-scaled.height))
    d.text((idx*360+24,14),label,fill='white')
    d.text((idx*360+24,360),'corpo: 302 px equivalentes',fill=(215,215,220))
preview.convert('RGB').save(root/'Lady-Fabius-proportions-1x.png')
preview.resize((1440,760),Image.Resampling.NEAREST).convert('RGB').save(root/'Lady-Fabius-proportions-2x.png')
print(json.dumps({'candidate':str(out),'size':candidate.size,'visibleHeight':height,'ppu':ppu,'worldBodyHeight':height/ppu*1.325,'pivot':pivot,'sha256':sha(out),'guid':guid,'raw':sha(generated),'cropPixelsUnmodified':True}))
