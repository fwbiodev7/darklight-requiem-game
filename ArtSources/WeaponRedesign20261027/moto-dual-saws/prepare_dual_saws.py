"""Pack twelve new whole adult Requiem figures; never changes shared consumers."""
from pathlib import Path
import json
import sys
import importlib.util
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261027/moto-dual-saws'
NAME='ReqMotoNormalDualSaws8AttackS20261027V1'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART
pipeline.OUT=OUT/'qa'
DIRECTIONS=['S','SW','W','NW','N','NE','E','SE']

def main():
    guide=json.loads((ART/'guides.json').read_text(encoding='utf-8'))
    rows=[dict(sourceRow=i//4,sourceIndices=[i],direction=i,action='Idle',durationMs=[160],loop=False) for i in range(8)]
    rows.append(dict(sourceRow=2,sourceIndices=[8,9,10,11],direction=0,action='Attack',attackKey='Dual Saws - Front Alternating Cut'))
    job=dict(name=NAME,source=str(ART/(NAME+'-source.png')),preservedSourceName=NAME+'-source.png',
       skin=0,weapon=3,form=0,columns=4,sourceRows=3,rows=rows,minimumArea=15000,
       supportAnchors={str(i):dict(anchor=anchor,method='Whole-source inspected ground under pelvis/planted boots; saw bounds excluded.')
                       for i,anchor in enumerate(guide['supportAnchors'])})
    if guide.get('replacements'):
        # Reuse the established whole-frame packer. It preserves every selected
        # nonzero-alpha RGBA pixel and never assembles anatomical components.
        helper_path=ROOT/'ArtSources/WeaponRedesign20261027/moto-idle-mature/prepare_mature_idles.py'
        spec=importlib.util.spec_from_file_location('whole_moto_frames',helper_path)
        helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
        pipeline.ART=ART;pipeline.OUT=OUT/'qa'
        helper.OUT=OUT;resource_bank=pipeline.RES;helper.RES=resource_bank
        mapping={int(k):v for k,v in guide['replacements'].items()}
        def part(part_name,source,indices,anchors,threshold=128):
            pjob=dict(job)
            pjob.update(name=part_name,source=str(ART/source),preservedSourceName=source,
               rows=[dict(sourceRow=i//4,sourceIndices=[i],direction=i if i<8 else 0,
                          action='Idle',durationMs=[160],loop=False) for i in indices],
               supportAnchors={str(i):dict(anchor=a,method='Whole-source inspected boot support.') for i,a in enumerate(anchors)})
            pipeline.RES=OUT/'staging'
            original_identifier=pipeline.identify
            def identify_whole(image,**kwargs):
                kwargs['alpha_threshold']=threshold
                return original_identifier(image,**kwargs)
            pipeline.identify=identify_whole
            try:result,_=pipeline.prepare(pjob)
            finally:pipeline.identify=original_identifier
            pipeline.write(ART/(part_name+'-job.json'),pjob)
            return result
        original=part(NAME+'-preserved',NAME+'-source.png',[i for i in range(12) if i not in mapping],guide['supportAnchors'])
        correction=part(NAME+'-correction',guide['repairSource'],list(mapping.values()),guide['repairSupportAnchors'])
        helper.RES=OUT/'staging'
        intermediate=helper.merge_intact(NAME+'-before-clothing',original,correction,mapping)
        clothing=guide.get('clothingRepair')
        if clothing:
            last_map={int(k):v for k,v in clothing['map'].items()}
            fixed_clothing=part(NAME+'-clothing',clothing['source'],list(last_map.values()),clothing['anchors'],224)
            helper.RES=resource_bank;pipeline.RES=resource_bank
            manifest=helper.merge_intact(NAME,intermediate,fixed_clothing,last_map)
            manifest['additionalSources']=[dict(path=correction['sourcePath'],sha256=correction['sourceSha256']),
                                           dict(path=fixed_clothing['sourcePath'],sha256=fixed_clothing['sourceSha256'])]
            proof=[]
            for i,frame in enumerate(manifest['frames']):
                provenance=fixed_clothing if i in last_map else correction if i in mapping else original
                source_index=last_map[i] if i in last_map else mapping.get(i,i)
                source_frame=next(f for f in provenance['frames'] if f['sourceFrameIndex']==source_index)
                frame['sourcePath']=provenance['sourcePath'];frame['sourceSha256']=provenance['sourceSha256']
                frame['sourceRect']=source_frame['sourceRect'];frame['sourceFrameIndex']=source_index
                proof.append(dict(index=i,sourcePath=frame['sourcePath'],sourceSha256=frame['sourceSha256'],
                  sourceFrameIndex=source_index,sourceRect=frame['sourceRect'],exactSelectedNonzeroAlphaRgba=True,
                  wholeFrame=True,preservedOriginal=i not in mapping and i not in last_map,
                  repaired=i in mapping or i in last_map))
            manifest['wholeFrameRepairProof']=proof
            pipeline.write(OUT/'preservation-whole-frames.json',proof)
        else:
            helper.RES=resource_bank;pipeline.RES=resource_bank
            manifest=helper.merge_intact(NAME,original,correction,mapping)
        job['wholeFigureReplacements']=dict(source=guide['repairSource'],sourceFrameMap=guide['replacements'],
                                           untouchedOriginalFrames=[i for i in range(12) if i not in mapping])
    else:
        manifest,_=pipeline.prepare(job)
    manifest['pixelsPerUnit']=guide['pixelsPerUnit']
    manifest['pixelsPerUnitCalibration']=guide
    manifest['worldVisualScale']=1.325
    for i,frame in enumerate(manifest['frames']):
        frame['direction']=DIRECTIONS[i] if i<8 else 'S'
        frame['durationMs']=160 if i<8 else [120,100,100,180][i-8]
        frame['phase']='static_ready' if i<8 else ['anticipation','first_cut','alternate_follow_through','recovery'][i-8]
        frame['action']='ready' if i<8 else 'attack'
    clips=[dict(name='ready_'+d,frames=[manifest['frames'][i]['id']],loop=False,allowStatic=True) for i,d in enumerate(DIRECTIONS)]
    clips.append(dict(name='attack_S',frames=[f['id'] for f in manifest['frames'][8:]],loop=False,allowStatic=False,
                      durationMs=[120,100,100,180],timingProposed=True))
    manifest['clips']=clips
    manifest['requiredClips']=[c['name'] for c in clips]
    manifest['motorcycleDualSaws']=dict(schemaVersion=1,skin=0,form=0,onFoot=True,dualSaws=True,
       ready=[f['id'] for f in manifest['frames'][:8]],directions=DIRECTIONS,staticReadyViews=True,
       attackSouth=[f['id'] for f in manifest['frames'][8:]],attackPhases=['anticipation','first_cut','alternate_follow_through','recovery'],
       attackDurationsMs=[120,100,100,180],attackTimingProposed=True,gameplayEventsDefined=False,
       wheelSawModules=2,legacyMechanismReusedAsReference=True,vehicleSeparationRepresented=True,
       assembledVehicleActionsComplete=False,allAttackFamiliesComplete=False)
    manifest['review']=dict(wholeSourceSeen=True,contactSeen=False,decodedGifPhasesSeen=False,humanApproval=False,
       identityAuthority='Approved Katana Normal; legacy Rock used only for wheel-saw mechanism',
       unityImported=False,runtimeVerified=False)
    manifest['notes']=['Eight actual standing ready directions and one four-phase south attack with two separate wheel-saw modules.',
       'Static ready views are not idle or movement animation cycles.',
       'All four attack phases are on foot; both saws remain hand-held. Normal hardware is neutral black/silver.',
       'Existing Join/Attack/ActionF0 whole sheets define mechanical separation, not the old Rock identity or transformation palette.',
       'No anatomical montage, resizing, mirror, raster deformation, recoloring or global alpha removal; selected nonzero-alpha RGBA preserved exactly.',
       'Source anatomy sets PPU, not bike/saw bounds; WorldVisualScale remains 1.325.',
       'Attack timing is a visual preview proposal. Runtime hit events/profile timing remain undefined until coordinator integration.',
       'No shared catalog, Runtime, Unity or Git edits by this art worker; mounted choreography and other attacks remain incomplete.']
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    pipeline.write(ART/(NAME+'-job.json'),job)
    checks=pipeline.qa(NAME)
    gif=OUT/'qa'/NAME/'preview/clip-08.gif'
    im=Image.open(gif);phases=[];timings=[]
    for i in range(im.n_frames):
        im.seek(i);phases.append(im.convert('RGBA').copy());timings.append(im.info.get('duration'))
    changes=[int(np.count_nonzero(np.any(np.array(phases[i])!=np.array(phases[i+1]),axis=2))) for i in range(len(phases)-1)]
    if im.n_frames!=4 or timings!=[120,100,100,180] or not all(changes):raise ValueError('Attack preview integrity failed')
    strip=Image.new('RGBA',(phases[0].width*4,phases[0].height),(40,45,53,255))
    for i,phase in enumerate(phases):strip.paste(phase,(i*phase.width,0),phase)
    strip.save(OUT/'attack-gif-phase-strip.png')
    pipeline.write(OUT/'gif-audit.json',dict(gif=gif.relative_to(ROOT).as_posix(),frames=im.n_frames,
       durationMs=timings,previewMs=sum(timings),changedPixels=changes,allPhasesDistinct=all(changes),
       livePlaybackObserved=False,timingProposed=True))
    refs=json.loads((ART/'preservation-before.json').read_text(encoding='utf-8-sig'))
    preservation=[dict(path=p['path'],sha256=p['sha256'],unchanged=pipeline.sha(ROOT/p['path'])==p['sha256']) for p in refs]
    if not all(p['unchanged'] for p in preservation):raise ValueError('Identity/mechanism source changed')
    pipeline.write(OUT/'preservation-after.json',preservation)
    receipt=dict(name=NAME,resourcePath='Darklight/WeaponRedesign20261023/'+NAME,
        figures=12,staticDualSawReadyDirections=8,frontAttackPhases=4,sourcePath=manifest['sourcePath'],
        sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],sourceSize=manifest['sourceSize'],
        nativeSize=manifest['nativeSize'],pixelsPerUnit=guide['pixelsPerUnit'],worldVisualScale=1.325,
        sourceRgbaExact=manifest['sourceRgbaExactReconstruction'],qa=checks,
        generated=True,sourceReviewed=True,packaged=True,humanApproval=False,
        unityImported=False,runtimeVerified=False,legacySourcesUnchanged=len(preservation),
        sharedCatalogModified=False,allAttackFamiliesComplete=False)
    pipeline.write(OUT/'prepared.json',receipt)
    print(json.dumps(receipt))

if __name__=='__main__':main()
