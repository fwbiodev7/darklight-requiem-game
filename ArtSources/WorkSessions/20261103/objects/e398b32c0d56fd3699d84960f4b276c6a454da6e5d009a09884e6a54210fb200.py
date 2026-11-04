from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib
ROOT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba');OUT=ROOT/'outputs/fechamento-fabius-20261103/moto-qa/short-legs-v4';QA=OUT.parent
NAME='ReqMotoNormalWalkW_EShortLegs20261103V4';source=OUT/(NAME+'-source.png');raw=Image.open(source).convert('RGBA')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(n,d):(OUT/n).write_text(json.dumps(d,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
xs=[0,443,887,1330,1774];ys=[0,444,887];atlas=Image.new('RGBA',(1920,1040));frames=[];cells=[];phases=['contact_A','passing_A_low','contact_B_opposite','passing_B_low']
for i in range(8):
    r,c=divmod(i,4);x,y=xs[c],ys[r];w,h=xs[c+1]-x,ys[r+1]-y;cell=raw.crop((x,y,x+w,y+h));ox,oy=19,[49,52][r];frame=Image.new('RGBA',(480,520));frame.paste(cell,(ox,oy));cells.append(frame);atlas.paste(frame,(c*480,r*520))
    assert frame.crop((ox,oy,ox+w,oy+h)).tobytes()==cell.tobytes()
    frames.append(dict(id=f'Redesign_{NAME}_{i:02}',rect=[c*480,r*520,480,520],pivot=[.5,1-420/520],durationMs=120,action='walk',direction=['W','E'][r],skin='Requiem',form='Normal',phase=phases[c],sourceFrameIndex=i,sourceRect=[x,y,w,h],sourceOffsetInFrame=[ox,oy],sourceScale=1,pixelsPerUnit=186.36363636363637,sourceAnchor=[x+221,y+[371,368][r]],supportGuide=dict(method='Manually inspected whole-figure soles; shared ground per direction, integer frame registration only',uncertaintyPixels=3)))
atlas_path=OUT/(NAME+'.png');atlas.save(atlas_path)
metadata=dict(schemaVersion=1,image=atlas_path.name,imageSha256=sha(atlas_path),nativeSize=list(atlas.size),sourceSize=list(raw.size),sourcePath=f'ArtSources/WeaponRedesign20261103/moto-walk-short-legs/{source.name}',sourceSha256=sha(source),preservedSourceFile=source.name,pixelsPerUnit=186.36363636363637,framePixelsPerUnit=[186.36363636363637]*8,status='candidate',wholeFigures=True,presentation='OnFoot',motorcycleContext='onFoot-DualSaws',worldVisualScale=1.325,frames=frames,clips=[dict(name='walk_'+di,frames=[f['id'] for f in frames[r*4:r*4+4]],loop=True,allowStatic=False) for r,di in enumerate(['W','E'])],requiredClips=['walk_W','walk_E'],quality=dict(alphaThreshold=0,maxColors=256,maxHeightRatio=1.12),sourceScale=1,sourceRgbaExactReconstruction=True,anatomyEdits=False,anatomyRedrawn=True,logicalResolution='Native generated raster; indexed logical grid unverified',integrationBlocked=True,integrationBlockReason='Candidate: coordinator must review new short-leg proportions and temporal W/E cycle, then stage exact source, catalog and slices; Unity runtime pending. No dependence on home package or old52 sheets.',review=dict(wholeSourceSeen=True,staticContactSeen=True,continuousMotionViewed=False,humanApproved=False,imported=False,runtimeVerified=False),pixelsPerUnitCalibration=dict(method='Retain previous uniform native density for transparent before/after comparison. PPU was not used to shorten legs. All8 full figures redrawn; no resampling; common worldVisualScale1.325.',provisional=True),notes=['Direct user feedback supersedes previous proportion judgment. Old source preserved; this is full-figure authored redraw.','SourceScale1; no anatomy montage, scaling, compression, deformation, mirroring, recolor or alpha cleanup.','ArtSources sourcePath is proposed; not installed. Import blocked until actual coordinator review.'])
write(NAME+'.json',metadata)
for theme,bgcolor,fg in [('light','#eef0f4','#121722'),('dark','#121722','#eef0f4')]:
    contact=Image.new('RGB',(1920,1104),bgcolor);dr=ImageDraw.Draw(contact)
    for i,cell in enumerate(cells):
        r,c=divmod(i,4);x,y=c*480,r*552;contact.paste(cell,(x,y),cell);dr.line((x+5,y+420,x+474,y+420),fill='#8192a9');dr.text((x+7,y+526),f'{["W","E"][r]} {i%4+1} {phases[i%4]} | native1x',fill=fg)
    contact.save(OUT/f'contact-native-{theme}.png')
for r,di in enumerate(['W','E']):
    seq=cells[r*4:r*4+4];seq[0].save(OUT/f'walk_{di}.apng',format='PNG',save_all=True,append_images=seq[1:],duration=[120]*4,loop=0,disposal=0,blend=0)
for i in [0,2,4,6]:
    r,c=divmod(i,4);x,y=xs[c],ys[r];fr=raw.crop((x,y,xs[c+1],ys[r+1]));bg=Image.new('RGBA',(fr.width+40,fr.height+25),'#e1e5ea');bg.alpha_composite(fr,(35,20));dr=ImageDraw.Draw(bg)
    for yy in range(0,fr.height,20):dr.line((25,yy+20,fr.width+35,yy+20),fill='#96a4b7');dr.text((0,yy+14),str(yy),fill='#111111')
    bg.convert('RGB').save(OUT/f'whole-frame-{i:02}-ruler.png')
# Common-world QA only: resample whole complete figures nearest for display at a fixed world density; never modify source assets.
old=Image.open(ROOT/'outputs/continuacao-escola-20261103/moto-walk/ReqMotoNormalWalkW_E20261103SchoolV2.png').convert('RGBA');oldW=old.crop((0,0,480,484));oldE=old.crop((0,484,480,968))
recs=[('Old W SUPERSEDED',oldW,186.36363636363637,420),('New W V4',cells[0],186.36363636363637,420),('Katana W upright',Image.open(QA/'Katana-W-upright-manual-whole.png').convert('RGBA'),175,370),('Katana E walk',Image.open(QA/'Katana-E-walking-A.png').convert('RGBA'),173.23232323232324,367),('Old E SUPERSEDED',oldE,186.36363636363637,420),('New E V4',cells[4],186.36363636363637,420)]
comparison=Image.new('RGB',(1800,1360),'#e3e7eb');dr=ImageDraw.Draw(comparison)
for i,(title,img,ppu,pivot_y) in enumerate(recs):
    scale=160*1.325/ppu;dim=(round(img.width*scale),round(img.height*scale));disp=img.resize(dim,Image.Resampling.NEAREST);x=i%3*600+12;y=i//3*680+590-round(pivot_y*scale);comparison.paste(disp,(x,y),disp);dr.line((i%3*600+5,i//3*680+590,i%3*600+595,i//3*680+590),fill='#8394ac');dr.text((i%3*600+10,i//3*680+628),f'{title} | PPU {ppu:.3f} WVS1.325',fill='#111111')
comparison.save(OUT/'common-world-comparison.png')
write('packing-proof.json',dict(sourceSha256=sha(source),atlasSha256=sha(atlas_path),framesExact=8,noResamplingOfAssets=True,wholeFigureOnly=True,comparisonOnly=dict(worldPixelsPerUnit=160,worldVisualScale=1.325,method='Nearest whole-frame display with existing legitimate PPU; exact output dimensions rounded for PNG visualization only')))
write('supersession.json',dict(directUserFeedback='a moto walk esta com as pernas maiores doq a katana deixe todos do mesmo tamanho',supersedes=['ReqMotoNormalWalkW_E20261103SchoolV2','PARECER-MOTO-W-E.md prior acceptance','metadata-proposal.json prior upper-body-only assessment','handoff.json previous READY_FOR_COORDINATOR'],preservesOldSources=True,attemptsThisCorrection=2,attempt1='V3: mostly reduced stride, inadequate vertical leg shortening',attempt2='V4: visibly shorter thigh/shin/boot anatomy, coordinator review required'))
print(json.dumps(dict(source=str(source),atlas=str(atlas_path),comparison=str(OUT/'common-world-comparison.png'),status='candidate',frames=8)))
