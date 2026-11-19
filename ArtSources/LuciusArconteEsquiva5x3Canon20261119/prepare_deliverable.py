"""Technical extraction of intact accepted rows only. No Assets or catalog writes."""
import hashlib, importlib.util, json, statistics
from pathlib import Path
from PIL import Image

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def save(path, data): path.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
source=HERE/'source-v1.png'
im=Image.open(source).convert('RGBA')
assert im.size==(1536,1024)
refs=[ROOT/'Docs/Darklight/References/LuciusVisual20261119/Lucius-Arconte.png', ROOT/'ArtSources/ArconteDodgeCanonPilot20261119/source.png']
refdata=[{'path':p.relative_to(ROOT).as_posix(),'sha256':sha(p),'sourceSize':list(Image.open(p).size)} for p in refs]
active=ROOT/'Assets/Darklight/Resources/Darklight/LuciusArconte20261111/LuciusArconteDodgeCanonSSE20261119V1.json'
reference=json.loads(active.read_text(encoding='utf-8'))
world_height=reference['calibration']['worldHeight']
xs=[0,307,614,922,1229,1536]
times=[35,35,65,45,50]
phases=['anticipation','push-compression','low-travel','braking-contact','guard-recovery']
pages=[]; proofs=[]
spec=importlib.util.spec_from_file_location('meta_helper', ROOT/'Tools/AnimationPreview/prepare_fists_combat_stance_20261118.py')
helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
for direction,direction_index,y0,y1,heights,anchors in [
    ('NW',3,341,682,[265,264],[(129,630),(432,629),(740,629),(1049,629),(1357,630)]),
    ('NE',5,682,1024,[277,279],[(155,969),(464,969),(776,967),(1084,965),(1390,970)])
]:
    name=f'LuciusArconteDodge5Phase{direction}20261119V1'
    out=HERE/(name+'.png'); row=im.crop((0,y0,1536,y1));row.save(out)
    assert row.tobytes()==im.crop((0,y0,1536,y1)).tobytes()
    ppu=float(format(statistics.median(heights)/world_height,'.12g'))
    frames=[]; names=[]
    for i in range(5):
        x0,x1=xs[i:i+2]; anchor=anchors[i]; fid=f'Redesign_{name}_{i:02d}'; names.append(fid)
        rect=[x0,0,x1-x0,y1-y0]
        cut=row.crop((x0,0,x1,y1-y0)); bbox=cut.getchannel('A').point(lambda v:255 if v>15 else 0).getbbox()
        assert bbox and bbox[0]>0 and bbox[1]>0 and bbox[2]<x1-x0 and bbox[3]<y1-y0
        frame={'id':fid,'rect':rect,'pivot':[(anchor[0]-x0)/(x1-x0),(y1-anchor[1])/(y1-y0)],'bodyAnchorPx':[anchor[0],anchor[1]-y0],'durationMs':times[i],'action':'Dodge','direction':direction,'phase':phases[i],'skin':'Lucius','form':'Arconte'}
        frames.append(frame)
        proofs.append({'id':fid,'sourceRect':[x0,y0,x1-x0,y1-y0],'wholeFigure':True,'rgbaSha256':hashlib.sha256(cut.tobytes()).hexdigest(),'alpha15Bounds':list(bbox),'sourcePixelEquality':True})
    data={'schemaVersion':1,'image':out.name,'imageSha256':sha(out),'sourcePath':source.relative_to(ROOT).as_posix(),'sourceSha256':sha(source),'sourceSize':list(im.size),'nativeSize':list(row.size),'pixelsPerUnit':ppu,'status':'visual-reviewed','visualApproval':False,'wholeFigures':True,'frames':frames,'clips':[{'name':'Dodge_'+direction,'frames':names,'loop':False,'allowStatic':False}],'requiredClips':['Dodge_'+direction],'quality':{'alphaThreshold':15,'maxColors':65536,'maxHeightRatio':1.4},'calibration':{'method':'manually inspected neutral whole-body hair-to-boot heights, sword excluded; same physical body height as active S/SE','neutralBodyMeasurements':heights,'neutralBodyHeightPx':statistics.median(heights),'referenceDescriptor':active.relative_to(ROOT).as_posix(),'referenceDescriptorSha256':sha(active),'referencePPU':reference['pixelsPerUnit'],'worldHeight':world_height,'visualScale':1.325,'worldHeightAfterVisualScale':world_height*1.325,'pixelsResampled':False,'pivotMethod':'manual projected body/support anchor in complete source figures'},'references':refdata,'warnings':['Generated RGBA has partial alpha; original values preserved. Logical pixel grid not certified.','Temporal preview prepared; Unity import, renderer appearance, runtime transitions and actual motion remain unverified.'],'unityImported':False,'runtimeVerified':False}
    save(out.with_suffix('.json'),data)
    meta=Path(str(out)+'.meta');meta.write_bytes(helper.texture_meta(meta,row.size,ppu,frames))
    jm=Path(str(out.with_suffix('.json'))+'.meta');jm.write_bytes(helper.text_meta(jm))
    pages.append({'id':name+'_Dodge_'+direction,'action':'Dodge','attackKey':'','direction':direction_index,'resourcePath':'Darklight/LuciusArconte20261111/'+name,'imageSha256':sha(out),'sourceSha256':sha(source),'sourcePath':source.relative_to(ROOT).as_posix(),'nativeSize':list(row.size),'sourceSize':list(im.size),'pixelsPerUnit':ppu,'spriteNames':names,'durationMs':times,'loop':False,'wholeFigures':True,'status':'visual-reviewed','directionalFallback':False,'sourceDirection':direction_index})
save(HERE/'catalog-snippet.json',{'schemaVersion':1,'characterId':'Lucius','form':'Arconte','weapon':'Sword','complete':False,'note':'Coordinator-only staging: 10 intact accepted NW/NE figures; no catalog changed. SW rejected for wrong-facing row.','pages':pages})
save(HERE/'whole-frame-provenance.json',{'calls':1,'generatedBy':'built-in image_gen','sourceSha256':sha(source),'sourceSize':list(im.size),'references':refdata,'acceptedFigures':10,'acceptedDirections':['NW','NE'],'rejectedRow':{'requestedDirection':'SW','actualDirection':'SE','sourceRect':[0,0,1536,341],'reason':'front row looks right; no integration or mirrored repair permitted'},'pixelsResampled':False,'anatomicalComposition':False,'wholeFrameProofs':proofs,'produced':True,'visualReviewed':True,'unityImported':False,'runtimeVerified':False,'catalogIntegrated':False,'published':False})
print(json.dumps({'acceptedFrames':10,'pages':[{'direction':p['direction'],'ppu':p['pixelsPerUnit'],'sha256':p['imageSha256']} for p in pages],'worldBodyHeight':world_height,'visualScale':1.325,'durationMs':sum(times),'sourceSha256':sha(source)}))
