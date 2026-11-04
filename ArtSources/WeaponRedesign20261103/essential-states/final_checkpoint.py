"""Compact, reproducible delivery checkpoint; never touches Assets, Runtime or Git."""
import hashlib,json
from datetime import datetime,timezone
from pathlib import Path
from PIL import Image
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
OUT=ROOT/'outputs/retomada-20261103/essential-states'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
catalog=json.loads((OUT/'catalog-fragment.json').read_text(encoding='utf-8'))
names=list(dict.fromkeys(p['resourcePath'].rsplit('/',1)[-1] for p in catalog['pages']))
artifacts=[]
rgba_proofs=[]
for name in names:
    descriptor=OUT/'staging'/(name+'.json')
    sheet=json.loads(descriptor.read_text(encoding='utf-8'))
    image=descriptor.with_suffix('.png')
    source=ROOT/sheet['sourcePath']
    assert sha(image)==sheet['imageSha256'] and sha(source)==sheet['sourceSha256']
    atlas=Image.open(image).convert('RGBA')
    for f in sheet['frames']:
        if 'originalSourcePath' not in f:continue
        raw=Image.open(ROOT/f['originalSourcePath']).convert('RGBA')
        x,y,w,h=f['originalSourceRect'];crop=raw.crop((x,y,x+w,y+h))
        ax,ay=f['originalSourceAnchor'];rx,ry,rw,rh=f['rect']
        dx=rx+round(f['pivot'][0]*rw)-(ax-x)
        dy=ry+round((1-f['pivot'][1])*rh)-(ay-y)
        match=atlas.crop((dx,dy,dx+w,dy+h)).tobytes()==crop.tobytes()
        assert match,f['id']
        rgba_proofs.append({'frame':f['id'],'source':f['originalSourcePath'],
                           'sourceRect':f['originalSourceRect'],'rgbaEqual':True})
    artifacts.append({'name':name,'status':sheet['status'],'figures':len(sheet['frames']),
      'imageSha256':sha(image),'descriptorSha256':sha(descriptor),
      'sourcePath':sheet['sourcePath'],'sourceSha256':sha(source),
      'nativeSize':sheet['nativeSize'],'sourceSize':sheet['sourceSize'],
      'pixelsPerUnit':sheet['pixelsPerUnit'],'childImported':False,'runtimeVerified':False})
raw=[]
for p in sorted(HERE.glob('*-source.png')):
    raw.append({'path':p.relative_to(ROOT).as_posix(),'sha256':sha(p),'size':list(Image.open(p).size)})
for weapon in [0,2,3]:
    for action in ['Parry','Hurt','Dead']:
        selected=[p for p in catalog['pages'] if p['weapon']==weapon and p['action']==action]
        assert sorted(p['direction'] for p in selected)==list(range(8))
        assert all(p['loop'] is False and all(type(ms) is int and ms>0 for ms in p['durationMs']) for p in selected)
write(OUT/'whole-figure-rgba-proofs.json',rgba_proofs)
write(OUT/'delivery-hashes.json',{'packets':artifacts,'preservedSources':raw,
      'catalogSha256':sha(OUT/'catalog-fragment.json'),
      'motoCatalogSha256':sha(OUT/'catalog-fragment-moto.json')})
checkpoint={'timestampUtc':datetime.now(timezone.utc).isoformat(),
 'scope':'Fabius Requiem Normal, Katana/Punhos/Moto on foot, Parry/Hurt/Dead, eight directions each',
 'selectedFigures':sum(a['figures'] for a in artifacts),'reviewedPages':len(catalog['pages']),
 'reviewedSheets':len(artifacts),'producedNativeFiguresIncludingSuperseded':176,
 'preservedSourcePngsIncludingTwoDerivedAtlases':len(raw),'inFlight':0,
 'status':'visual-reviewed; staged; contract checked; not Unity-imported/tested by child',
 'sourcePixelsResampled':False,'wholeFigureRGBAProofs':len(rgba_proofs),
 'childAssetsModified':False,'childRuntimeModified':False,'childGitExecuted':False,'childUnityExecuted':False,
 'integrationOwnership':'Coordinator; coordinator reported Katana24 copied, later copied states not independently audited here.',
 'approvals':{'HurtExpressionStyle':'User explicitly approved expressive charming Hurt faces for all states/equipment.',
              'MotoParry':'User explicitly approved current Moto defense appearance; native PNG unchanged afterward.'},
 'timings':{'Parry':[260],'Hurt':[60,120],'HurtRuntimeShortenedStateMs':120,
            'Dead':[180,220,1000],'DeadFinalFrameClampedIndefinitely':True},
 'exclusions':['ReqKatanaNormalParry8Dirs20261103V1 and V2 superseded, V2 staging marked superseded.',
               'Original MotoDeadA NW rest has blades cut at canvas bottom; excluded from delivered sprites.',
               'Repair generation row labels differed from drawn direction; only genuine NW row2 two complete figures selected; other six figures unused.'],
 'guardContact':'Eight page.guardContact points normalized Unity bottom-left within each sprite rect. N is explicitly projected/occluded behind torso; seven visible junction annotations. Final orange guide matches metadata.',
 'pending':['Coordinator integration/import of current fragments as needed, preserving already copied Katana.',
            'Unity/runtime and transition/contact/VFX tests explicitly deferred by user.',
            'Actual animated playback, in-engine scale/ground contact and density remain unverified; finite GIFs and static consecutive complete poses reviewed separately.',
            'Palette extraction/full skill audit was not run in reserve closure; no claim of automated artistic certification.'],
 'nextExact':'Use catalog-fragment-moto.json for only Moto; current Moto death names are S_SW_W20261103V1Selected, NW20261103V2Selected, N_NE_E_SE20261103V1. Full fragment72pages in same directory. Read delivery-hashes.json and whole-figure-rgba-proofs.json before further import.',
 'failures':'No image-generation failure or quota failure. One preview write hit sandbox PermissionError and was rerun successfully with scoped filesystem escalation; no source lost.'}
write(OUT/'CHECKPOINT-20261103-essential-states.json',checkpoint)
print(json.dumps({'figures':checkpoint['selectedFigures'],'pages':checkpoint['reviewedPages'],
 'sheets':checkpoint['reviewedSheets'],'sourcePngs':len(raw),'rgbaProofs':len(rgba_proofs),
 'catalogSha256':sha(OUT/'catalog-fragment.json'),'motoCatalogSha256':sha(OUT/'catalog-fragment-moto.json')}))
