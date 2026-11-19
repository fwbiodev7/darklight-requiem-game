"""Read user atlas literally. Whole-figure extraction only; no pixel edits."""
from pathlib import Path
from PIL import Image
import hashlib,json

HERE=Path(__file__).resolve().parent
SRC=HERE/'source-user-reference-v6.png'
PREVIEW=HERE.parents[1]/'previews/ruptura/v6-complete-frames'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
im=Image.open(SRC)
assert im.mode=='RGBA' and im.size==(2172,724)
assert sha(SRC)=='fbef65a4a9875c55d244775d81c28d790716db2036619462b201427c45896898'
PREVIEW.mkdir(parents=True,exist_ok=True)
durations=[120,170,170,190,170,150,130,150]
assert sum(durations)==1250 and sum(durations[:4])==650
names=['source_neutral','concentration','energy_gather','open_arms_ascent','peak_hover','consolidation','descent','final_ruptura']
ax=[286,826,1366,1916,285,832,1368,1917]
ay=[314,314,315,319,685,685,685,685]
frames=[]
for n in range(8):
    x=n%4*543;y=0 if n<4 else 350;w=543;h=350 if n<4 else 374
    crop=im.crop((x,y,x+w,y+h))
    path=PREVIEW/f'ruptura_v6_{n:02d}.png';crop.save(path)
    assert Image.open(path).tobytes()==crop.tobytes()
    significant=crop.getchannel('A').point(lambda a:255 if a>=16 else 0).getbbox()
    frames.append(dict(id=f'ruptura_v6_{n:02d}',phase=names[n],rect=[x,y,w,h],
        pivot=[(ax[n]-x)/w,1-(ay[n]-y)/h],durationMs=durations[n],
        action='TransformationEnter',direction='source_front_three_quarter',skin='Requiem',form='source_NormalToRuptura',
        completeFigureExtract=str(path.relative_to(HERE.parents[1])),extractSha256=sha(path),
        significantAlphaBoundsLocal=list(significant),pixelsIdenticalToSourceRectangle=True))
d=dict(schemaVersion=1,image=SRC.name,imageSha256=sha(SRC),nativeSize=list(im.size),
    logicalPixelResolution=None,pixelsPerUnit=157.57576,
    pixelsPerUnitStatus='reference-only approved katana meta; user sheet not calibrated/imported',
    status='candidate-user-literal',visualReviewed=True,userApproved=False,imported=False,runtimeVerified=False,
    frames=frames,clips=[dict(name='LiteralNormalToRupturaV6',frames=[f['id'] for f in frames],loop=False,allowStatic=False)],
    requiredClips=['LiteralNormalToRupturaV6'],quality=dict(alphaThreshold=16,maxColors=1000000,maxHeightRatio=1.3),
    source=dict(original='C:/Users/Usuario/Downloads/Planilha de Sprites_ Ascensão de Ruptura.png',
        sha256=sha(SRC),copiedByteForByte=True,tool='user-provided PNG; no image generation performed'),
    finalAuthority=dict(image='ruptura-fire-user-reference.png',sha256=sha(HERE/'ruptura-fire-user-reference.png'),
        use='inspected complete final-form reference; no pixels transplanted'),
    supersedes='RequiemNormalToRuptura-v5-form-candidate.png',
    consumer=dict(path='Assets/Darklight/Runtime/Campaign/DarklightCampaignTransformations.cs',
        durationMs=1250,releaseProgress=0.52,releaseMs=650,proposedReleaseFrameIndex=4,
        binding='candidate only; no current Ruptura sprite entry integration'),
    preservation=dict(sourceFileBytesUnchanged=True,allEightWholeRectanglesPixelIdentical=True,
        noAnatomyMontage=True,noAlphaNormalization=True,noScaling=True,noPaletteEdit=True,
        noMirroring=True,noRotation=True,noTween=True,noArtificialBobbing=True,
        rowBoundary='Observed transparent separation y350; mathematical half y362 would clip low-alpha onset of bottom-row second aura.'),
    review=dict(findings=['Literal eight complete figures from user sheet, no artistic reinterpretation.',
        'Larger drawn levitation retained from source and ground-plane anchors used without body movement synthesis.',
        '8 distinct frame crops and exact original source PNG preserved.'],
        limitations=['Source neutral figure already has red clothing details.',
            'Katana is absent in source open-handed frames3..5 and returns in frame6; preserved literally, no correction requested.',
            'Source has partial alpha and low-alpha gutter residues; no alpha modifications.',
            'Fixed source view only; native logical pixel grid and Unity PPU not certified.',
            '1.25s preview follows current consumer; HTML supports slower inspection. GIF is palette-quantized only.',
            'Awaiting user review; no Assets/Runtime/catalog/Git/Unity changes.']))
(HERE/'source-user-reference-v6.json').write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
active=dict(candidate='source-user-reference-v6.png',manifest='source-user-reference-v6.json',
    status='candidate-not-approved-not-integrated',sha256=sha(SRC),supersedes=d['supersedes'])
(HERE/'ACTIVE-CANDIDATE.json').write_text(json.dumps(active,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(sha256=sha(SRC),frames=8,sourceSize=list(im.size),durationMs=sum(durations),releaseMs=650,pixelsPreserved=True)))
