"""Technical whole-frame packing only; never edits, resizes or paints source pixels."""
from pathlib import Path
from PIL import Image
import hashlib, json, uuid

OUT = Path(__file__).resolve().parent
ROOT = OUT.parents[1]
SOURCE = OUT / 'MatriarcaRubraCombat-source-v1.png'
SOURCE_SIZE = (1448,1086)
CELL_SOURCE = (362,543)
CELL = (384,608)
ANCHOR = (192,520)
# Manually inspected boot-ground midpoint per intact source cell, top-left origin.
BOOTS = [(183,467),(188,477),(190,478),(187,471),(180,486),(183,485),(180,485),(184,478)]
NAMES = ['MatriarcaRubraCombat_BaseIdle_S_00','MatriarcaRubraCombat_BaseStepA_S_01',
         'MatriarcaRubraCombat_BaseStepB_S_02','MatriarcaRubraCombat_BaseGuard_S_03',
         'MatriarcaRubraCombat_AttackPreparation_S_00','MatriarcaRubraCombat_AttackContact_S_01',
         'MatriarcaRubraCombat_AttackFollowthrough_S_02','MatriarcaRubraCombat_AttackRecovery_S_03']
DURATIONS = [300,180,180,300,600,125,125,220]
PPU = round(351 * 1.325 / 2.74257,12)
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def write(path, data): path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

source = Image.open(SOURCE).convert('RGBA')
assert source.size == SOURCE_SIZE
stage = Image.new('RGBA',(CELL[0]*4,CELL[1]*2))
frames=[]
proof=[]
for i,name in enumerate(NAMES):
    sx,sy=(i%4)*362,(i//4)*543
    whole=source.crop((sx,sy,sx+362,sy+543))
    ox,oy=ANCHOR[0]-BOOTS[i][0],ANCHOR[1]-BOOTS[i][1]
    tx,ty=(i%4)*CELL[0],(i//4)*CELL[1]
    stage.paste(whole,(tx+ox,ty+oy)) # Exact RGBA copy, not alpha blend.
    recovered=stage.crop((tx+ox,ty+oy,tx+ox+362,ty+oy+543))
    assert recovered.tobytes()==whole.tobytes()
    proof.append({'frame':name,'wholeSourceRGBAEqual':True,'sourceRect':[sx,sy,362,543],
                  'packOffset':[ox,oy],'sourceBootAnchor':list(BOOTS[i])})
    frames.append({'id':name,'plannedSpriteName':name,'rect':[tx,ty,*CELL],
                   'pivot':[.5,(CELL[1]-ANCHOR[1])/CELL[1]],'durationMs':DURATIONS[i],
                   'action':['idle','stepA','stepB','guard','preparation','contact','followthrough','recovery'][i],
                   'direction':'S-front-slight-angle','skin':'MatriarcaRubra','form':'Normal',
                   'sourceRect':[sx,sy,362,543], 'integrate': i!=0})
image=OUT/'MatriarcaRubraCombat-staging-v1.png'
stage.save(image)
manifest={'schemaVersion':1,'image':image.name,'imageSha256':sha(image),'nativeSize':list(stage.size),
 'sourceNativeSize':list(source.size),'pixelsPerUnit':PPU,'worldVisualScale':1.325,
 'targetBodyWorldHeight':2.74257,'calibrationBodyPixels':351,
 'status':'visual-reviewed-candidate','imported':False,'runtimeVerified':False,'userApproved':False,
 'originalIdlePreserved':True,'integrationNote':'Use approved OfficialCast idle as base[0]; generated BaseIdle is inspection/reference only. All new frames use localScale 1.325.',
 'references':[{'path':'Assets/Darklight/Resources/Darklight/OfficialCast/MatriarcaRubra.png',
                'sha256':sha(ROOT/'Assets/Darklight/Resources/Darklight/OfficialCast/MatriarcaRubra.png')},
               {'path':'Docs/Darklight/References/CampanhaOficial20261119/Matriarca-Rubra.png',
                'sha256':sha(ROOT/'Docs/Darklight/References/CampanhaOficial20261119/Matriarca-Rubra.png')}],
 'source':{'image':SOURCE.name,'sha256':sha(SOURCE),'generationCalls':1,'correctionCalls':0,
           'resampling':'none','anatomicalAssembly':False},
 'frames':frames,'clips':[{'name':'base_inspection','frames':NAMES[:4],'loop':False,'allowStatic':False},
                        {'name':'walk','frames':NAMES[1:3],'loop':True,'allowStatic':False},
                        {'name':'attack','frames':NAMES[4:],'loop':True,'allowStatic':False}],
 'requiredClips':['walk','attack'],
 'quality':{'alphaThreshold':8,'requireTransparency':True,'maxHeightRatio':1.3},
 'timingEvidence':{'consumer':'Assets/Darklight/Runtime/Campaign/DarklightEnemyAttacks.cs',
   'windupMs':600,'windupNote':'multiplied by actual difficulty windup; follow-up can reduce preparation',
   'contactMs':250,'contactFrameMs':[125,125],'recoveryMs':220,
   'locomotionNote':'Show(1/2) alternates by traveled distance, .28 world units per step for Matriarch; preview 180ms is illustrative.'},
 'colorCritiques':['Silver/lavender hair, warm pale skin, red eyes, ruby dress and gold trim match the approved material identities.',
  'No large slash/ground VFX; blade and halo retain intrinsic luminous red.',
  'Source has 256 alpha values and faint disconnected red emission in gutters; all pixels preserved without thresholding.',
  'Generated small-scale detail is more compact than the large idle; game-scale continuity still requires coordinator inspection in engine.'],
 'pending':['Unity import and runtime visual review','Confirm intro idle-to-combat transition at actual camera size',
            'Consumer has automatic flipX; opposite-view asymmetry remains existing runtime limitation, not separately authored coverage.']}
write(OUT/'MatriarcaRubraCombat-staging-v1.json',manifest)
write(OUT/'whole-frame-pixel-proof.json',{'sourceSha256':sha(SOURCE),'stageSha256':sha(image),'frames':proof})

# Self-contained Unity multiple-sprite TextureImporter staging metadata.
guid=uuid.uuid5(uuid.NAMESPACE_URL,'darklight/matriarca-combat-20261119/v1').hex
ids=[21300000+i*2 for i in range(8)]
lines=['fileFormatVersion: 2',f'guid: {guid}','TextureImporter:','  internalIDToNameTable:']
for name,fileid in zip(NAMES,ids):
    lines += ['  - first:',f'      213: {fileid}',f'    second: {name}']
lines += ['  externalObjects: {}','  serializedVersion: 13','  mipmaps:','    mipMapMode: 0',
 '    enableMipMap: 0','    sRGBTexture: 1','  isReadable: 0','  streamingMipmaps: 0',
 '  textureFormat: 1','  maxTextureSize: 2048','  textureSettings:','    serializedVersion: 2',
 '    filterMode: 0','    aniso: 1','    mipBias: 0','    wrapU: 1','    wrapV: 1','    wrapW: 1',
 '  nPOTScale: 0','  compressionQuality: 100','  spriteMode: 2','  spriteExtrude: 1',
 '  spriteMeshType: 1','  alignment: 9','  spritePivot: {x: 0.5, y: 0.144736842105263}',
 f'  spritePixelsToUnits: {PPU:.12f}','  spriteBorder: {x: 0, y: 0, z: 0, w: 0}',
 '  spriteGenerateFallbackPhysicsShape: 0','  alphaUsage: 1','  alphaIsTransparency: 1',
 '  textureType: 8','  textureShape: 1','  platformSettings:','  - serializedVersion: 4',
 '    buildTarget: DefaultTexturePlatform','    maxTextureSize: 2048','    resizeAlgorithm: 0',
 '    textureFormat: -1','    textureCompression: 0','    compressionQuality: 100',
 '    crunchedCompression: 0','    allowsAlphaSplitting: 0','    overridden: 0',
 '  spriteSheet:','    serializedVersion: 2','    sprites:']
for i,(name,fileid) in enumerate(zip(NAMES,ids)):
    x=(i%4)*CELL[0]; y=(1-i//4)*CELL[1]
    sid=uuid.uuid5(uuid.NAMESPACE_URL,'darklight/'+name).hex
    lines += ['    - serializedVersion: 2',f'      name: {name}','      rect:','        serializedVersion: 2',
       f'        x: {x}',f'        y: {y}',f'        width: {CELL[0]}',f'        height: {CELL[1]}',
       '      alignment: 9','      pivot: {x: 0.5, y: 0.144736842105263}',
       '      border: {x: 0, y: 0, z: 0, w: 0}','      outline: []','      physicsShape: []',
       '      tessellationDetail: 0','      bones: []',f'      spriteID: {sid}',f'      internalID: {fileid}',
       '      vertices: []','      indices:','      edges: []','      weights: []']
lines += ['    outline: []','    physicsShape: []','    bones: []',f'    spriteID: {guid}',
 '    internalID: 0','    vertices: []','    indices:','    edges: []','    weights: []',
 '    secondaryTextures: []','    nameFileIdTable:']
for name,fileid in zip(NAMES,ids):lines.append(f'      {name}: {fileid}')
lines += ['  userData: staging-only-visual-reviewed-not-imported-body351px-scale1.325',
 '  assetBundleName:','  assetBundleVariant:']
(OUT/(image.name+'.meta')).write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(json.dumps({'stage':str(image.relative_to(ROOT)),'size':stage.size,'ppu':PPU,'sha256':sha(image),'wholeFramesPreserved':len(proof)}))
