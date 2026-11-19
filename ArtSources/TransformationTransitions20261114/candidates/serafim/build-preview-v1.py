from PIL import Image, ImageDraw
from pathlib import Path
import hashlib,json,math
root=Path(__file__).resolve().parent
out=root.parent.parent/'previews'/'serafim'
out.mkdir(parents=True,exist_ok=True)
source=root/'serafim-transition-source-v1.png'
im=Image.open(source).convert('RGBA')
sha=lambda p:hashlib.sha256(Path(p).read_bytes()).hexdigest()
xs=[0,443,887,1330,1774]; ys=[0,443,887]
body_x=[225,229,224,238,224,241,244,233]
feet_y=[373,373,361,342,344,345,380,383]
durations=[350,200,1000,700,450,800,800,300]
names=['concentracao','flexao','ascensao','recepcao','manifestacao_asas','consolidacao','descida','postura_final']
canvas=620; anchor=[310,510]; lift_px=64
raw=[]; frames=[]; source_rects=[]
atlas=Image.new('RGBA',(canvas*4,canvas*2))
elapsed=0
def lift(p):
    def smooth(v): return v*v*(3-2*v)
    if p<.12:return 0
    if p<.34:return smooth((p-.12)/.22)
    if p<.76:return 1
    return 1-smooth((p-.76)/.24)
for i in range(8):
    row,col=divmod(i,4)
    box=(xs[col],ys[row],xs[col+1],ys[row+1])
    crop=im.crop(box)
    dx=anchor[0]-body_x[i]; dy=anchor[1]-feet_y[i]
    cell=Image.new('RGBA',(canvas,canvas))
    cell.paste(crop,(dx,dy))
    assert cell.crop((dx,dy,dx+crop.width,dy+crop.height)).tobytes()==crop.tobytes()
    raw.append(cell)
    mid=(elapsed+durations[i]/2)/4600
    shift=round(lift_px*lift(mid))
    posed=Image.new('RGBA',(canvas,canvas))
    posed.paste(cell,(0,-shift))
    assert not cell.getchannel('A').crop((0,0,canvas,shift)).getbbox()
    atlas.paste(posed,(col*canvas,row*canvas))
    src=[box[0],box[1],box[2]-box[0],box[3]-box[1]]
    source_rects.append(src)
    frames.append(dict(id=f'serafim_enter_SE_{i:02d}',rect=[col*canvas,row*canvas,canvas,canvas],
       pivot=[.5,1-anchor[1]/canvas],durationMs=durations[i],action='transformation_enter',direction='SE',
       skin='Requiem',form='NormalToSerafimCarmesim',phase=names[i],sourceRect=src,
       sourceBodyAnchor=[body_x[i],feet_y[i]],sourceToCellTranslation=[dx,dy],
       presentationMidpointLiftPixels=shift))
    elapsed+=durations[i]
atlas_path=root/'serafim-transition-atlas-v1.png';atlas.save(atlas_path)
refs=[]
for f in ['serafim-transition.png','ruptura-transition.png']:
    p=root.parent.parent/'references'/f
    refs.append(dict(path=str(p.relative_to(root.parent.parent)),sha256=sha(p)))
approved=root.parents[3]/'Assets'/'Darklight'/'Resources'/'Darklight'/'WeaponRedesign20261023'/'ReqKatanaNormalNeutralIdleS_SW_W20261025.png'
# File provenance only; reference art is untouched.
manifest=dict(schemaVersion=1,image=atlas_path.name,imageSha256=sha(atlas_path),nativeSize=list(atlas.size),
 sourceSize=list(im.size),logicalPixelResolution='not established; source is generated raster',
 pixelsPerUnit=157.57575757575756,ppuStatus='preview placeholder inherited from Normal; NOT calibrated for import',
 status='candidate',visualReview='full figures and light/dark contacts inspected; not user-approved',
 visualApproval=False,wholeFigures=True,sourceImage=source.name,sourceSha256=sha(source),
 references=refs,frames=frames,
 clips=[dict(name='NormalToSerafimCarmesim',frames=[f['id']for f in frames],loop=False,allowStatic=True)],
 quality=dict(alphaThreshold=0,maxColors=1000000,maxHeightRatio=1.3),
 consumer=dict(path='Assets/Darklight/Runtime/Campaign/DarklightCampaignTransformations.cs',
 durationMs=4600,releaseAtProgress=.52,liftWorldUnits=.62,
 presentationPath='Assets/Darklight/Runtime/Character/DarklightTransformationPresentation.cs',
 envelope={'start':.12,'full':.34,'descent':.76,'land':1}),
 preview=dict(durationMs=4600,sourcePoseCount=8,interpolatedPoseCount=0,
 presentationTranslationOnly=True,liftPixels=lift_px,
 sourcePixelsPreserved=True,rectOrigin='top-left',
 noRuntimeIntegration=True),
 limitations=['Source has subpixel alpha and many colors; native pixel grid is not established.',
 'Eight key poses use timed holds. Smooth aerial travel is presentation translation, not newly drawn animation.',
 'No Unity import, runtime, user approval or gameplay event validation.'])
(root/'serafim-transition-v1.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
# Compact four-column contact sheets with all pixels composited for visual review.
for label,color in [('light','#d8d8d8'),('dark','#181c23')]:
    contact=Image.new('RGB',(canvas*4,(canvas+30)*2),color)
    d=ImageDraw.Draw(contact)
    for i in range(8):
        r,c=divmod(i,4)
        posed=atlas.crop((c*canvas,r*canvas,(c+1)*canvas,(r+1)*canvas))
        contact.paste(posed,(c*canvas,r*(canvas+30)),posed)
        d.text((c*canvas+18,r*(canvas+30)+canvas+4),f'{i+1} {names[i]} / {durations[i]}ms',fill='white'if label=='dark'else'black')
    contact.save(out/f'contact-{label}-v1.png')
# 30 fps GIF: only original eight complete poses plus explicit presentation travel.
gifframes=[]; gifdur=[]; sampled=[]
for n in range(138):
    t=n*4600/138
    idx=0; acc=durations[0]
    while t>=acc and idx<7:
        idx+=1;acc+=durations[idx]
    y=-round(lift_px*lift(t/4600))
    composed=Image.new('RGBA',(canvas,canvas),'#181c23')
    composed.alpha_composite(raw[idx],(0,y))
    gifframes.append(composed.convert('RGB'))
    end=round((n+1)*4600/138/10)*10;start=round(n*4600/138/10)*10
    gifdur.append(end-start)
    sampled.append(dict(tMs=round(t),pose=idx,presentationOffsetY=y))
gifframes[0].save(out/'serafim-transition-v1.gif',save_all=True,append_images=gifframes[1:],
                 duration=gifdur,loop=0,disposal=2,optimize=False)
(out/'preview-timeline-v1.json').write_text(json.dumps(dict(sourcePoseCount=8,renderedSamples=138,
 durationMs=sum(gifdur),sampleInterpretation='Eight source poses plus integer presentation translations; no new anatomical frames',
 samples=sampled),indent=2),encoding='utf-8')
# Export whole raw cells only, never body parts.
raw_dir=root/'whole-frames-v1';raw_dir.mkdir(exist_ok=True)
for i,cell in enumerate(raw):cell.save(raw_dir/f'{i+1:02d}-{names[i]}.png')
print(json.dumps(dict(sourceSha256=sha(source),atlasSha256=sha(atlas_path),frames=8,durationMs=sum(gifdur),
 originalPixelsPreserved=True,atlasSize=atlas.size,preview=str(out/'serafim-transition-v1.gif'))))

