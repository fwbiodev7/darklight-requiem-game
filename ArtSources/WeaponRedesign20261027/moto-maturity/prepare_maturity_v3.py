"""Pack twelve complete reviewed V3 figures; leave V2 and all catalogs untouched."""
from pathlib import Path
import json
import sys

ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261027/moto-maturity'
NAME='RequiemMotoForm0Ride8SlamS20261027V3'
SOURCE=ART/'RequiemMotoNormalMaturity20261027V3-source.png'
pipeline.ART=ART
pipeline.OUT=OUT/'qa'
DIRS=['S','SW','W','NW','N','NE','E','SE']

def main():
    anchors=[[166,333],[551,335],[987,329],[1387,329],
             [173,656],[550,647],[972,647],[1380,669],
             [185,1004],[570,1003],[974,991],[1320,992]]
    guide=dict(date='2026-10-27',wholeSourceReviewed=True,technicalCoordinatorAccepted=True,humanApproval=False,
        sourceFrameIndex=9,hairY=738,soleY=1003,bodyHeightSourcePixels=265,measurementUncertaintyPixels=4,
        worldBodyHeight=1.98,worldVisualScale=1.325,pixelsPerUnit=265/1.98,
        method='Whole standing adult rider in overhead-lift figure: crown-to-boot sole; overhead bike excluded. One PPU for all twelve; no individual resizing.',
        primaryIdentityReferences=['ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png',
            'ArtSources/WeaponRedesign20261026/katana-normal/ReqKatanaNormalWalkSW_W-v1-source.png'],
        supportAnchors=anchors,visualScaleCheckPending=True,
        limits=['Eight static riding directions and four south slam phases; no mounted idle, true movement cycle or all-technique coverage.',
                'Native indexed pixel grid unverified; partial alpha/palette warnings remain visible.',
                'Identity/maturity accepted technically by coordinator, not new human approval.'])
    rows=[dict(sourceRow=i//4,sourceIndices=[i],direction=i,action='Idle',durationMs=[160],loop=False) for i in range(8)]
    rows.append(dict(sourceRow=2,sourceIndices=[8,9,10,11],direction=0,action='Attack',attackKey='Esmagamento Vertical'))
    job=dict(name=NAME,source=str(SOURCE),preservedSourceName=SOURCE.name,skin=0,weapon=3,form=0,
        columns=4,sourceRows=3,rows=rows,minimumArea=15000,calibrationGuide=guide,
        supportAnchors={str(i):dict(anchor=a,method='Whole-frame inspected tire or boot ground support; pivot excludes overhead machinery and sharp blade extremities.') for i,a in enumerate(anchors)})
    manifest,_=pipeline.prepare(job)
    manifest['pixelsPerUnit']=guide['pixelsPerUnit'];manifest['pixelsPerUnitCalibration']=guide
    manifest['status']='visual-reviewed'
    for frame,duration in zip(manifest['frames'],[160]*8+[232,348,160,580]):frame['durationMs']=duration
    manifest['clips']=[dict(name='ride_'+DIRS[i],frames=[manifest['frames'][i]['id']],loop=False,allowStatic=True) for i in range(8)]+[
        dict(name='slam_S',frames=[f['id'] for f in manifest['frames'][8:]],loop=False,allowStatic=False)]
    manifest['requiredClips']=[c['name'] for c in manifest['clips']]
    manifest['motorcycle']=dict(form=0,skin=0,ride=[f['id'] for f in manifest['frames'][:8]],
        slam=[f['id'] for f in manifest['frames'][8:]],slamStartup=.58,slamActive=.16,slamRecovery=.58,
        staticRideViews=True,otherActionsCoverage=False)
    manifest['review']=dict(wholeSourceSeen=True,packedContactSeen=False,motionSeen=False,technicalCoordinatorAccepted=True,
        humanApproval=False,unityImported=False,runtimeVerified=False,original26Preserved=True,
        newIdleIntegrated=False,identityAuthority='Approved Katana Normal, not the earlier juvenile rider',
        limitation='Tight right source margin on recovery; all opaque silhouettes complete, all selected nonzero alpha retained.')
    manifest['notes']=['Whole figures only, no anatomy assembly, no scaling/recoloring or alpha removal.',
        'Eight STATIC mounted advancing views; these are not idle or locomotion cycles.',
        'Only the four south Esmagamento Vertical phases are newly authored as an action sequence.',
        'Long Requiem sleeves/coat and sober Katana identity retained; motorcycle hardware is the V2 design.',
        'All earlier sources and assets preserved. Runtime consumer switch/import/testing belong to coordinator.']
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    pipeline.write(ART/(NAME+'-job.json'),job)
    qa=pipeline.qa(NAME)
    report=dict(name=NAME,resourcePath='Darklight/WeaponRedesign20261023/'+NAME,figures=12,
        rideStaticDirections=8,slamSouthFrames=4,pixelsPerUnit=guide['pixelsPerUnit'],worldVisualScale=1.325,
        sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],
        sourceRgbaExact=manifest['sourceRgbaExactReconstruction'],qa=qa,technicalCoordinatorAccepted=True,
        humanApproval=False,original26Preserved=True,unityImported=False,runtimeVerified=False)
    pipeline.write(OUT/'prepared.json',report)
    print(json.dumps(report))

if __name__=='__main__':main()
