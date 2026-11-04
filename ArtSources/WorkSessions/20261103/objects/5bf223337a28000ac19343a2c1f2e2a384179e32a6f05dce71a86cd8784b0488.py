import base64, hashlib, io, json, shutil, statistics
from pathlib import Path
from PIL import Image, ImageDraw

ROOT=Path('C:/Users/Usuario/Documents/Codex/2026-11-03/ba')
OUT=ROOT/'outputs/fechamento-fabius-20261103/punhos-sw/cycle-v2'
PROJECT=ROOT/'outputs/darklight-requiem-game'
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def savejson(name,obj): (OUT/name).write_text(json.dumps(obj,indent=2,ensure_ascii=False),encoding='utf-8')
name='ReqFistsNormalWalkSW20261103CycleV2'
katana_ppu=162.37373737373738
# Manual landmarks from intact figure grids; floor/bbox heights are excluded from fit.
katana=dict(crownY=25,chinY=134,beltY=185,neckY=145,headWidth=138,pelvisY=229)
small=dict(crownY=44,chinY=230,beltY=322,neckY=247,headWidth=245,pelvisY=400)
large=dict(crownY=80,chinY=454,beltY=646,neckY=493,headWidth=500,pelvisY=806)
def ratios(a,b):
 return dict(headHeight=(a['chinY']-a['crownY'])/(b['chinY']-b['crownY']),crownToBelt=(a['beltY']-a['crownY'])/(b['beltY']-b['crownY']),neckToBelt=(a['beltY']-a['neckY'])/(b['beltY']-b['neckY']),headWidth=a['headWidth']/b['headWidth'],beltToPelvis=(a['pelvisY']-a['beltY'])/(b['pelvisY']-b['beltY']))
fit=ratios(small,katana);density=katana_ppu*statistics.median(fit.values())
largefit=ratios(large,small);large_density=density*statistics.median(largefit.values())
ppus=[density,density,density,large_density]
anchors=[(292,593),(246,590),(252,600),(515,1186)]
images=[Image.open(OUT/f'frame-{i:02}-native.png').convert('RGBA') for i in range(4)]
sources=[OUT.parent/'attempt-03-cycle.png']*3+[Path('C:/Users/Usuario/.codex/generated_images/01a1a132-4a58-72c0-a0e5-2be92b40fa74/exec-87d00719-eca4-4fd7-9bac-ccd213eced35.png')]
source_rects=[[0,0,599,656],[599,0,600,656],[599,656,600,656],[0,0,images[3].width,images[3].height]]
phases=['contact_A','passing_A','contact_B','passing_B']
atlas=Image.new('RGBA',(sum(im.width for im in images),max(im.height for im in images)),(0,0,0,0))
frames=[];x=0
for i,im in enumerate(images):
 atlas.paste(im,(x,0))
 ax,ay=anchors[i]
 frames.append(dict(id=f'Redesign_{name}_{i:02}',rect=[x,0,im.width,im.height],pivot=[ax/im.width,1-ay/im.height],durationMs=100,action='walk',direction='SW',skin='Requiem',form='Normal',phase=phases[i],pixelsPerUnit=ppus[i],sourceScale=1,sourcePath=str(sources[i]),sourceSha256=sha(sources[i]),sourceRect=source_rects[i],sourceFrameIndex=[0,1,3,0][i],sourceAnchor=list(anchors[i]),nativeFigurePath=f'frame-{i:02}-native.png',nativeFigureSha256=sha(OUT/f'frame-{i:02}-native.png')))
 x+=im.width
atlaspath=OUT/f'{name}.png';atlas.save(atlaspath)
proof=[]
for i,f in enumerate(frames):
 x,y,w,h=f['rect'];src=Image.open(sources[i]).convert('RGBA');sx,sy,sw,sh=source_rects[i]
 exact=atlas.crop((x,y,x+w,y+h)).tobytes()==src.crop((sx,sy,sx+sw,sy+sh)).tobytes()
 proof.append(dict(frame=f['id'],rgbaExact=exact,sourceSha256=f['sourceSha256'],sourceRect=f['sourceRect'],nativeSize=[w,h]))
 assert exact
manifest=dict(schemaVersion=1,image=atlaspath.name,imageSha256=sha(atlaspath),nativeSize=list(atlas.size),sourceSize=list(atlas.size),pixelsPerUnit=density,framePixelsPerUnit=ppus,status='visual-reviewed',reviewScope='static complete figures and support sequence; temporal coordinator review pending',wholeFigures=True,anatomyEdits=False,sourceScale=1,sourceRgbaExactReconstruction=True,visualApproval=False,imported=False,runtimeVerified=False,integrationEligible=False,coordinatorTemporalReviewPending=True,worldVisualScale=1.325,frames=frames,clips=[dict(name='walk_SW',frames=[f['id'] for f in frames],loop=True,allowStatic=False)],requiredClips=['walk_SW'],quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=2.3),notes=['Native raster atlas, no anatomical resizing/mirroring/deformation.','Per-frame density follows robust head/torso/pelvis landmark fit, not bounding-box or floor normalization.','Sources remain unchanged; nearest scale transforms occur only in clearly named world simulation previews.'])
savejson(f'{name}.json',manifest)
page=dict(id=f'{name}_Walk_SW',skin=0,weapon=2,form=0,direction=1,action='Walk',attackKey='',resourcePath='Darklight/WeaponRedesign20261023/'+name,imageSha256=sha(atlaspath),sourcePath='ArtSources/ReviewedDeliveries/FabiusNormal20261103/staging/'+name+'-source.png',sourceSha256=sha(atlaspath),sourceSize=list(atlas.size),nativeSize=list(atlas.size),pixelsPerUnit=density,framePixelsPerUnit=ppus,status='visual-reviewed',wholeFigures=True,loop=True,spriteNames=[f['id'] for f in frames],sourceSpriteNames=[],durationMs=[100]*4,humanApproved=False,imported=False,runtimeVerified=False)
savejson('proposed-page-NOT-INTEGRATED.json',page)
savejson('preservation-proof.json',dict(allFrameRgbaExact=all(p['rgbaExact'] for p in proof),frames=proof,unchangedSourceHashes={str(p):sha(p) for p in set(sources)}))
savejson('density-and-landmarks.json',dict(method='Median independent head height, head width, crown-to-belt, neck-to-belt, belt-to-pelvis; no bounding-box/floor normalization',referencePpu=katana_ppu,reference=katana,small=small,large=large,smallToKatanaRatios=fit,largeToSmallRatios=largefit,proposedFramePixelsPerUnit=ppus,uncertaintyRelative=0.06,landmarkUncertaintyPixels=dict(reference=4,small=7,large=14),projectedGroundAnchors=anchors,groundAnchorUncertaintyPixels=[5,5,5,10],worldVisualScale=1.325,limits='Single scalar density cannot change relative limb lengths or anatomy. Ratio disagreement remains visible in overlays and is not corrected by PPU.'))
# All following resizing is PREVIEW-ONLY nearest simulation, never atlas/source mutation.
ppm=katana_ppu;visualscale=1.325;tile=640;ground=570;origin=320
def render(im,ppu,anchor):
 scale=ppm*visualscale/ppu
 size=(round(im.width*scale),round(im.height*scale));scaled=im.resize(size,Image.Resampling.NEAREST)
 canv=Image.new('RGBA',(tile,tile),(38,42,48,255));xx=origin-round(anchor[0]*scale);yy=ground-round(anchor[1]*scale)
 canv.alpha_composite(scaled,(xx,yy));return canv,scale,(xx,yy)
world=[]
for i,im in enumerate(images):
 canv,s,pos=render(im,ppus[i],anchors[i]);d=ImageDraw.Draw(canv);d.line((0,ground,tile,ground),fill=(70,200,220),width=1);d.line((origin,0,origin,tile),fill=(90,100,110),width=1);d.text((10,10),f'{i:02} {phases[i]} 100ms | PPU {ppus[i]:.2f} +/-6%',fill='white');d.text((10,28),'SIMULACAO NEAREST - escala de mundo 1.325 - nao altera asset',fill=(230,200,100));canv.save(OUT/f'simulacao-nearest-world-{i:02}.png');world.append(canv)
contact=Image.new('RGBA',(tile*2,tile*2),(38,42,48,255))
for i,canv in enumerate(world):contact.paste(canv,((i%2)*tile,(i//2)*tile))
contact.save(OUT/'simulacao-nearest-world-contact.png')
world[0].save(OUT/'simulacao-nearest-world-loop.gif',save_all=True,append_images=world[1:],duration=100,loop=0,disposal=2)
refim=Image.open(OUT/'reference-katana-sw00-whole.png').convert('RGBA')
# Reference pivot is its packed ground-projection anchor (225,346).
refworld,_,_=render(refim,katana_ppu,(225,346))
refworld.save(OUT/'simulacao-nearest-reference-katana.png')
compare=Image.new('RGBA',(tile*3,tile),(38,42,48,255))
compare.paste(refworld,(0,0));compare.paste(world[0],(tile,0));compare.paste(world[3],(tile*2,0))
ImageDraw.Draw(compare).text((10,10),'KATANA SW canonical reference',fill='white');compare.save(OUT/'simulacao-nearest-proportion-comparison.png')
overlay=Image.new('RGBA',(tile*2,tile),(38,42,48,255))
for col,i in enumerate([0,3]):
 dest=Image.new('RGBA',(tile,tile),(38,42,48,255))
 # Tint entire silhouettes, never anatomical parts.
 for im,ppu,anchor,color in [(refim,katana_ppu,(225,346),(235,80,220)),(images[i],ppus[i],anchors[i],(60,220,220))]:
  sc=ppm*visualscale/ppu;scaled=im.resize((round(im.width*sc),round(im.height*sc)),Image.Resampling.NEAREST);mask=scaled.getchannel('A').point(lambda a:round(a*.38));tint=Image.new('RGBA',scaled.size,color+(0,));tint.putalpha(mask);dest.alpha_composite(tint,(origin-round(anchor[0]*sc),ground-round(anchor[1]*sc)))
 d=ImageDraw.Draw(dest);d.line((0,ground,tile,ground),fill='white');d.text((10,10),f'Katana magenta / Fists {i:02} cyan | simulation only',fill='white');overlay.paste(dest,(col*tile,0))
overlay.save(OUT/'simulacao-nearest-proportion-overlay.png')
urls=[]
for im in world:
 b=io.BytesIO();im.save(b,format='PNG');urls.append('data:image/png;base64,'+base64.b64encode(b.getvalue()).decode())
html='''<!doctype html><meta charset="utf-8"><title>SW cycle v1 - simulaÃ§Ã£o nearest</title><style>body{background:#181b20;color:#eee;font:16px sans-serif}img{width:640px;height:640px;image-rendering:pixelated}button,input{margin:8px}</style><h2>SimulaÃ§Ã£o nearest em escala do mundo â€” nÃ£o altera asset</h2><p>Quatro fases100ms; densidade por quadro. PPU proposto Â±6%. RevisÃ£o temporal da coordenaÃ§Ã£o pendente.</p><button id="play">Pausar</button><button id="step">PrÃ³ximo quadro</button><label>Velocidade <input id="speed" type="range" min="0.25" max="2" step="0.25" value="1"></label><span id="label"></span><br><img id="view"><script>const frames=FRAMES;const phases=PHASES;let n=0,playing=true,t=performance.now();const im=document.querySelector('#view');function show(){im.src=frames[n];document.querySelector('#label').textContent=n+' '+phases[n];}document.querySelector('#play').onclick=()=>{playing=!playing;document.querySelector('#play').textContent=playing?'Pausar':'Reproduzir';};document.querySelector('#step').onclick=()=>{playing=false;n=(n+1)%4;show();};function tick(now){if(playing&&now-t>=100/Number(document.querySelector('#speed').value)){n=(n+1)%4;t=now;show();}requestAnimationFrame(tick);}show();requestAnimationFrame(tick);</script>'''.replace('FRAMES',json.dumps(urls)).replace('PHASES',json.dumps(phases))
(OUT/'simulacao-nearest-world-preview.html').write_text(html,encoding='utf-8')
savejson('handoff.json',dict(state='READY_FOR_COORDINATOR',candidateCycleComplete=True,visualReview='static support alternation A/A/B/B with separate low passing-B figure',continuousMotionReviewPending=True,integrationEligible=False,imported=False,runtimeVerified=False,atlas=atlaspath.name,manifest=f'{name}.json',proposedPage='proposed-page-NOT-INTEGRATED.json',preview='simulacao-nearest-world-preview.html',proportionComparison='simulacao-nearest-proportion-comparison.png',overlay='simulacao-nearest-proportion-overlay.png',framePixelsPerUnit=ppus,uncertaintyRelative=.06,loopMs=400,wholeFrameRgbaExact=True,requiredCoordinatorActions=['Review temporal motion and proportional consistency in world-scale preview','Confirm pivots/densities within reported uncertainty','Copy source atlas to proposed ArtSources path and validate actual hashes/resources/GUIDs before integration']))
print(json.dumps(dict(atlas=list(atlas.size),framePpu=ppus,rgbaExact=True,state='READY_FOR_COORDINATOR')))
