from pathlib import Path
from PIL import Image, ImageDraw
from collections import Counter
import hashlib, json

ROOT = Path(__file__).resolve().parents[2]
SRC = Path(__file__).resolve().parent
OUT = ROOT / 'outputs/retomada-20261101/atmosphere/textures'
STAGE = OUT / 'staged'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p, data): p.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
source = SRC / 'AtmosphereTextures-pixel-v2-source.png'
target = STAGE / 'AtmosphereTextures.png'
im = Image.open(source).convert('RGBA')
assert im.size == (1536,1024)
assert sha(source) == sha(target), 'Atlas must remain source-identical'
names = [('fog_0','fog_ground','low thin ground mist'),('fog_1','fog_bank','layered dense fogbank'),('fog_2','fog_veil','wispy drifting veil'),('moon_0','moon_broad','broad diagonal cold moonbeam'),('moon_1','moon_rays','narrow angled moon rays'),('moon_2','moon_haze','pale blue luminous haze')]
frames, checks = [], []
contact = Image.new('RGB',(1584,1100),(15,23,29))
draw = ImageDraw.Draw(contact)
for i,(name,kind,description) in enumerate(names):
    x,y,w,h = [(0,0,520,502),(520,0,522,502),(1042,0,494,502),(0,502,520,522),(520,502,522,522),(1042,502,494,522)][i]
    cell = im.crop((x,y,x+w,y+h))
    a=cell.getchannel('A')
    cell_hist=a.histogram()
    edges=[a.crop(r) for r in [(0,0,w,1),(0,h-1,w,h),(0,0,1,h),(w-1,0,w,h)]]
    edge_values=[v for edge in edges for v in edge.get_flattened_data()]
    threshold32=a.point(lambda v:255 if v>=32 else 0).getbbox()
    frames.append({'id':'Redesign_Atmosphere_'+name,'name':name,'rect':[x,y,w,h],'x':x,'y':y,'width':w,'height':h,'pivot':[0.5,0.5],'durationMs':1,'action':'atmosphere','direction':'none','kind':kind,'description':description,'wholeFigure':True})
    ah=a.histogram()
    checks.append({'name':name,'rect':[x,y,w,h],'alphaRange':list(a.getextrema()),'transparentPixels':ah[0],'partialAlphaPixels':sum(ah[1:255]),'opaquePixels':ah[255],'boundaryAlphaMax':max(edge_values),'boundaryAlphaMean':round(sum(edge_values)/len(edge_values),3),'boundaryEffectiveAlphaAtOpacity012':round(max(edge_values)/255*.12,6),'contentBoundsAtAlpha32':threshold32,'contentAtAlpha32InsideCell':threshold32 is not None and threshold32[0]>0 and threshold32[1]>0 and threshold32[2]<w and threshold32[3]<h})
    panel=Image.new('RGBA',(w,h),(15,23,29,255))
    panel.alpha_composite(cell)
    cy=i//3*550
    cx=i%3*528
    contact.paste(panel.convert('RGB'),(cx,cy))
    draw.text((cx+12,cy+528),name+' | '+description,fill=(183,206,216))
refs=[]
for n in ['moonlit-forest-reference.png','gothic-ruins-reference.png']:
    p=ROOT/'ArtSources/AtmosphereReference20261101'/n
    with Image.open(p) as r: size=list(r.size)
    refs.append({'path':str(p.relative_to(ROOT)).replace('\\','/'),'sha256':sha(p),'size':size,'role':'full style reference; scenery not copied'})
descriptor={'schemaVersion':1,'image':'AtmosphereTextures.png','imageSha256':sha(target),'nativeSize':list(im.size),'sourceSize':list(im.size),'source':str(source.relative_to(ROOT)).replace('\\','/'),'sourceSha256':sha(source),'origin':'top-left','rectConvention':'x,y,width,height; Unity reader must invert y','pixelsPerUnit':256,'status':'visual-reviewed','wholeFigures':True,'staticTextures':True,'frames':frames,'clips':[],'references':refs,'generation':{'mode':'integrated imagegen','calls':1,'prompt':'ArtSources/AtmosphereTextures20261101/prompt.txt','promptSha256':sha(SRC/'prompt.txt')},'integrationContract':{'suggestedFilter':'Bilinear','mipmaps':False,'compression':'uncompressed RGBA','drawBelowActors':True,'maxRendererOpacity':0.12,'timing':'static texture; consumer controls slow drift, no frame animation','noSceneOrGameplayChanges':True},'review':{'visual':'Six distinct complete soft atmospheric motifs, cool desaturated blue-gray; no copied scenery, text, moon disk, or opaque white patches. Diffuse smooth style intentionally differs from crisp combat particles.','alpha':'Genuine generated alpha preserved without edits. Low alpha residue at cell borders is measured in QA, not threshold-erased.','runtimeVerified':False,'imported':False}}
descriptor['generation'].update({'calls':2,'version':'pixel-v2','prompt':'ArtSources/AtmosphereTextures20261101/prompt-pixel-v2.txt','promptSha256':sha(SRC/'prompt-pixel-v2.txt')})
descriptor['integrationContract']['suggestedFilter']='Point'
descriptor['review']['visual']='Pixel-art V2 preserves all six V1 effect motifs in cold blue-gray and pale moonlight. Deliberate square clusters, stepped edges, restrained dithering, no scenery or added effects. Native source preserved without resizing or recoloring.'
descriptor['review']['alpha']='Real generated alpha preserved untouched; actual effect boundaries measured rather than assuming regular cells. See per-frame alpha edges in QA.'
save(STAGE/'AtmosphereTextures.json',descriptor)
contact.save(OUT/'AtmosphereTextures-contact.png')
hist=im.getchannel('A').histogram()
report={'schemaVersion':1,'status':'visual-reviewed-staging','imageSha256':sha(target),'sourceByteIdentical':sha(target)==sha(source),'imageSize':list(im.size),'frames':checks,'alphaRange':list(im.getchannel('A').getextrema()),'transparentPixels':hist[0],'partialAlphaPixels':sum(hist[1:255]),'opaquePixels':hist[255],'wholeMotifsAtAlpha32':all(c['contentAtAlpha32InsideCell'] for c in checks),'maxEffectiveCellEdgeAtOpacity012':max(c['boundaryEffectiveAlphaAtOpacity012'] for c in checks),'pixelEdits':False,'imported':False,'runtimeVerified':False,'limitations':['Very faint partial-alpha residue remains in gutters/cell borders; maximum 30/255 alpha becomes 0.014118 at renderer opacity 0.12. Preserve pixels; verify final sprite boundaries in runtime.','No seamless tiling claim. Six static whole textures; no animation phase claim.','No Unity import, build, runtime or human playtest performed by this art task.']}
report['version']='pixel-v2'
report['limitations'][0]='Partial-alpha residue remains at actual cell boundaries; see measured per-frame maxima and effective opacity. Source pixels preserved; Point filtering required.'
report['genericSpriteAudit']='V1 generic audit required nonempty animation clips and flagged partial alpha. Static textures intentionally use clips:[]; V2 independently checks actual whole-motif bounds, hashes and alpha instead.'
assert report['wholeMotifsAtAlpha32'], 'Meaningful effect pixels must be inside each measured cell'
assert len(set(f['id'] for f in frames))==6
save(OUT/'AtmosphereTextures-qa.json',report)
visible=Counter(px for px in im.get_flattened_data() if px[3]>=32)
save(OUT/'AtmosphereTextures-palette.json',{'schemaVersion':1,'description':'Smooth diffuse asset: observed exact RGBA samples, not a restricted pixel-art palette. No quantization or recoloring applied.','topExactVisibleRgba':[{'rgba':list(k),'pixels':v} for k,v in visible.most_common(24)],'distinctVisibleRgba':len(visible),'referenceGuides':['#537580','#9ABCC8','#C7E0E8'],'material':'cold blue-gray fog and pale-blue moonlight; partial alpha is intentional'})
palette_path=OUT/'AtmosphereTextures-palette.json'
palette_info=json.loads(palette_path.read_text(encoding='utf-8'))
palette_info['description']='Pixel-art V2 exact observed RGBA samples; generated deliberate square clusters with partial-alpha transitions. No post-generation quantization, resizing or recoloring.'
save(palette_path,palette_info)
receipt={'status':'visual-reviewed-staging','version':'pixel-v2','stagedPng':str(target.relative_to(ROOT)).replace('\\','/'),'stagedJson':str((STAGE/'AtmosphereTextures.json').relative_to(ROOT)).replace('\\','/'),'pngSha256':sha(target),'jsonSha256':sha(STAGE/'AtmosphereTextures.json'),'rawSourceSha256':sha(source),'count':6,'nativeSize':list(im.size),'contact':str((OUT/'AtmosphereTextures-contact.png').relative_to(ROOT)).replace('\\','/'),'qa':str((OUT/'AtmosphereTextures-qa.json').relative_to(ROOT)).replace('\\','/'),'states':{'produced':True,'visualReviewed':True,'imported':False,'runtimeVerified':False},'pending':'Parent coordinator import and runtime verification of V2 with Point filter, renderer opacity <=0.12.'}
save(OUT/'receipt.json',receipt)
print(json.dumps({'count':6,'pngSha256':sha(target),'jsonSha256':sha(STAGE/'AtmosphereTextures.json'),'wholeMotifsAtAlpha32':report['wholeMotifsAtAlpha32'],'maxEffectiveEdge':report['maxEffectiveCellEdgeAtOpacity012'],'states':receipt['states']}))
