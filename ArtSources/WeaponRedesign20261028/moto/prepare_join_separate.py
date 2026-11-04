"""Staging only: register and merge complete Requiem/machine figures without altering RGBA."""
from pathlib import Path
import importlib.util
import json
import sys

ROOT = Path(__file__).resolve().parents[3]
ART = Path(__file__).resolve().parent
OUT = ROOT/'outputs/retomada-20261028/moto'
STAGE = OUT/'staging'
NAME = 'ReqMotoNormalJoinSeparateS20261028V1'
sys.path.insert(0, str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

def part(name, source, rows, indices, anchors):
    pipeline.ART = ART
    pipeline.OUT = OUT/'qa'
    pipeline.RES = STAGE
    job = dict(name=name, source=str(ART/source), preservedSourceName=source,
        skin=0, weapon=3, form=0, columns=4, sourceRows=rows, minimumArea=9000,
        rows=[dict(sourceRow=i//4,sourceIndices=[i],direction=0,action='Transition',loop=False) for i in indices],
        supportAnchors={str(i):dict(anchor=a,method='Whole-source inspected boot/tire ground support, no anatomy edits.') for i,a in enumerate(anchors)})
    original_identifier = pipeline.identify
    def complete_silhouettes(image, **kwargs):
        kwargs['alpha_threshold']=224
        return original_identifier(image,**kwargs)
    pipeline.identify=complete_silhouettes
    try: manifest,_=pipeline.prepare(job)
    finally: pipeline.identify=original_identifier
    pipeline.write(ART/(name+'-job.json'),job)
    return manifest

def main():
    cfg=json.loads((ART/'join-config.json').read_text(encoding='utf-8'))
    mapping={int(k):v for k,v in cfg['repairMap'].items()}
    original=part(NAME+'-preserved',NAME+'-source.png',3,[i for i in range(12) if i not in mapping],cfg['anchors'])
    fixed=part(NAME+'-repair',cfg['repairSource'],cfg['repairRows'],list(mapping.values()),cfg['repairAnchors'])
    helper_path=ROOT/'ArtSources/WeaponRedesign20261027/moto-idle-mature/prepare_mature_idles.py'
    spec=importlib.util.spec_from_file_location('intact_moto_frames',helper_path)
    helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
    helper.OUT=OUT;helper.RES=STAGE
    manifest=helper.merge_intact(NAME,original,fixed,mapping)
    pipeline.ART=ART;pipeline.OUT=OUT/'qa';pipeline.RES=STAGE
    guide=cfg['calibration']
    manifest.update(pixelsPerUnit=guide['pixelsPerUnit'],pixelsPerUnitCalibration=guide,
        worldVisualScale=1.325,status='visual-reviewed',wholeFigures=True)
    join_phases=['two_separate_hand_saws','lower_and_approach','folding_bridge_engaged','straddle_unfolded_chassis','lower_to_saddle','mounted_ready']
    separate_phases=['release_and_rise','opening_bridge','folding_spine','pull_modules_apart','lower_hand_saws','on_foot_ready']
    timings=[140,120,140,160,120,220,140,140,140,120,120,220]
    for i,f in enumerate(manifest['frames']):
        f.update(action='assemble' if i<6 else 'separate',direction='S',durationMs=timings[i],
            phase=(join_phases+separate_phases)[i])
    manifest['clips']=[dict(name='assemble_S',frames=[f['id'] for f in manifest['frames'][:6]],loop=False,allowStatic=False),
        dict(name='separate_S',frames=[f['id'] for f in manifest['frames'][5:]],loop=False,allowStatic=False)]
    manifest['requiredClips']=[c['name'] for c in manifest['clips']]
    manifest['motorcycleTransition']=dict(skin=0,weapon=3,form=0,direction=0,
        assemble=manifest['clips'][0]['frames'],separate=manifest['clips'][1]['frames'],
        onFootAndMounted=True,dualSawModules=2,timingStatus='visual-proposal-no-gameplay-events',
        events=[],otherDirectionsComplete=False)
    manifest['review']=dict(wholeSourceSeen=True,contactSeen=True,nativeAtlasSeen=True,
        nearest2xWholeFiguresSeen=True,decodedGifPhasesSeen=True,motionSeen=False,livePlaybackObserved=False,
        playbackLimit='Local file URL blocked by browser security policy; no workaround attempted. Offline GIF phases/timing inspected.',humanApproval=False,
        unityImported=False,runtimeVerified=False,identityAuthority='Approved Requiem Katana and dual-saw source27')
    manifest['notes']=['Assembly has six phases; separation shares the mounted endpoint then six new phases (seven clip frames). Twelve distinct whole figures S only.',
        'Exactly two wheel-saw modules fold into one motorcycle; no independent third wheel.',
        'Correct original figures preserved pixel for pixel; defective figures replaced complete.',
        'All selected nonzero-alpha source RGBA preserved; no anatomical composition, rescale, recolor, mirroring or alpha deletion.',
        'PPU calibrates comparable standing anatomy only; it does not repair anatomy.',
        'Native generated raster; indexed pixel grid unverified. Unity/runtime require coordinator.']
    pipeline.write(STAGE/(NAME+'.json'),manifest)
    qa=pipeline.qa(NAME)
    pipeline.write(OUT/'prepared.json',dict(name=NAME,figures=12,clips=2,qa=qa,
        sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],
        pixelsPerUnit=guide['pixelsPerUnit'],worldVisualScale=1.325,visualReviewed=True,
        imported=False,runtimeVerified=False,stagingOnly=True))
    print(json.dumps(dict(name=NAME,qa=qa,imageSha256=manifest['imageSha256'],size=manifest['nativeSize'])))

if __name__=='__main__':main()
