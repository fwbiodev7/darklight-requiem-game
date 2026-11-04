"""Whole-frame candidate metadata; does not edit the source or Unity assets."""
from pathlib import Path
from hashlib import sha256
import json
from PIL import Image

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
STEM = 'ReqFistsNormalDireto_S_SW_W-v1'
source = HERE / (STEM + '-source.png')
image = Image.open(source).convert('RGBA')
job_path = HERE / (STEM + '-job.json')
job = json.loads(job_path.read_text(encoding='utf-8'))
xs, ys = [0, 384, 768, 1200, 1536], [0, 363, 681, 1024]
directions = ['S', 'SW', 'W']
phases = ['anticipation', 'contact', 'follow-through', 'recovery']
timings = [75, 65, 65, 65]
consumer_suffixes = ['02', '03', None, '11']
# Proposed ground anchors, inspected against complete feet and body; not imported.
anchors = [(185,347),(570,352),(976,349),(1351,349),
           (175,669),(570,662),(976,666),(1351,669),
           (185,996),(570,986),(976,982),(1351,997)]
frames, clips, checks = [], [], []
for row, direction in enumerate(directions):
    ids = []
    for col, phase in enumerate(phases):
        x,y,w,h = xs[col],ys[row],xs[col+1]-xs[col],ys[row+1]-ys[row]
        index = row*4+col
        fid = f'{STEM}_{direction}_{col:02d}'
        ax,ay = anchors[index]
        suffix = consumer_suffixes[col]
        crop = image.crop((x,y,x+w,y+h))
        visible = crop.getchannel('A').point(lambda a: 255 if a>32 else 0)
        bbox = visible.getbbox()
        source_name = f'Ruin_Requiem_Normal_{direction}_Combat_{suffix}' if suffix else None
        frames.append(dict(id=fid,rect=[x,y,w,h],pivot=[(ax-x)/w,(y+h-ay)/h],
                           durationMs=timings[col],action='Attack:light1',attackKey='Direto da Ruína',
                           direction=direction,skin='Requiem',form='Normal',phase=phase,
                           sourceName=source_name,consumerDurationMs=130 if col==3 else timings[col],
                           groundAnchorSource=[ax,ay],wholeFigure=True))
        checks.append(dict(id=fid,visibleBBoxAlphaGT32=bbox,
                           alphaBBox=crop.getchannel('A').getbbox(),
                           rgbaSha256=sha256(crop.tobytes()).hexdigest(),
                           fullFigureInspected=True,dominantHand='right'))
        ids.append(fid)
    clips.append(dict(name=f'DiretoRuina_{direction}_fourPhasePreview',frames=ids,loop=False,allowStatic=False))
manifest = dict(schemaVersion=1,image=source.name,imageSha256=sha256(source.read_bytes()).hexdigest(),
                nativeSize=list(image.size),sourceSize=list(image.size),logicalNativeSize=None,
                pixelsPerUnit=175.25252,pixelsPerUnitStatus='front-consumer-reference-only-not-imported',
                pivotStatus='proposed-ground-anchors-not-imported',status='visual-reviewed',
                visualApproval=False,wholeFigures=True,frames=frames,clips=clips,
                requiredClips=[c['name'] for c in clips],references=job['references'],
                quality=dict(alphaThreshold=32,maxColors=200000,maxHeightRatio=1.3),
                consumerContract=dict(source='Assets/Darklight/Runtime/Character/DarklightRuinFistsBank.cs',
                                      indices=[2,3,11],timingsMs=[75,65,130],
                                      sourcePhaseIndices=[0,1,3],extraPhaseIndex=2,
                                      note='Four drawings; existing bank consumes 3 names. Follow-through remains an unmapped additional source. Runtime timings/physics/events are unchanged.'))
(HERE/(STEM+'-manifest.json')).write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
review=dict(status='visual-reviewed',visualApproval=False,frames=checks,
            sourceSha256=manifest['imageSha256'],sourceResolution=list(image.size),logicalResolutionUnknown=True,
            referencesInspectedWhole=True,identityAuthority=job['identityAuthority'],
            visualFindings=['12 complete figures inspected at source size on dark and light backgrounds.',
                            'S frontal, SW diagonal down-left, W left-facing. Same dark-brown hair, youthful face and coat across rows.',
                            'Neutral steel/graphite clawed gauntlets and greaves; no transformed fissures or sword.',
                            'Distinct anticipation, right-hand contact, follow-through and recovery; counterweight legs and coat change.',
                            'Small red shoulder crystals and burgundy coat lining belong to approved identity; no red hair tips.'],
            limitations=['Alpha is generated, with very faint residual pixels (including alpha 1) outside silhouettes. Source alpha preserved unchanged; visible-bound checks use alpha >32.',
                         'Sprite source contains many colors and partial alpha; this is not a certified low-resolution palette-limited sprite.',
                         'Pivots and PPU are proposed; Unity import, transition, contact-event and runtime verification are pending.',
                         'Existing bank has 3 phase names. Follow-through is supplied additionally, not falsely mapped to another action.',
                         'Animated files generated; perceived playback not yet inspected in a live viewer.'],
            unityImported=False,runtimeVerified=False,audioStarted=False)
(HERE/(STEM+'-review.json')).write_text(json.dumps(review,ensure_ascii=False,indent=2),encoding='utf-8')
job.update(status='generated-and-visual-reviewed',source=source.name,sourceSha256=manifest['imageSha256'],
           generatedImageOrigin='C:/Users/Usuario/.codex/generated_images/01a17020-9993-7ca0-a7fb-2b2703d71fc1/exec-bbaebc88-5636-4b36-a6e5-8c801327c22b.png',
           sourceAlphaPreserved=True,wholeFigureRects=[f['rect'] for f in frames])
job_path.write_text(json.dumps(job,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(dict(frames=len(frames),clips=len(clips),hash=manifest['imageSha256'],output=source.name)))
