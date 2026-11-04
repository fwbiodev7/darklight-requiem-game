"""Recover reviewed complete-figure sheets into isolated staging, never Assets."""
from pathlib import Path
import json, shutil, hashlib, sys

ROOT=Path(__file__).resolve().parents[3]
ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261031/moto'
STAGE=OUT/'staging'
OLD=ROOT/'outputs/retomada-20261028/moto'
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline
pipeline.ART=ART;pipeline.OUT=OUT/'qa';pipeline.RES=STAGE

def main():
    STAGE.mkdir(parents=True,exist_ok=True)
    names=['ReqMotoNormalJoinSeparateS20261028V1','ReqMotoNormalDentesSW_W20261028V1']
    deliveries=[]
    for name in names:
        shutil.copy2(OLD/'staging'/(name+'.png'),STAGE/(name+'.png'))
        manifest=json.loads((OLD/'staging'/(name+'.json')).read_text(encoding='utf-8'))
        manifest['status']='visual-reviewed'
        previous=manifest.get('review',{})
        manifest['review']=dict(previous,reviewDate='2026-10-31',wholeSourceSeen=True,contactSeen=True,
            nativeAtlasSeen=True,nearest2xWholeFiguresSeen=True,decodedGifPhasesSeen=True,
            livePlaybackObserved=False,humanApproval=False,unityImported=False,runtimeVerified=False,
            recoveredFrom='outputs/retomada-20261028/moto/staging/'+name+'.json',
            identityAuthority='Approved Requiem Katana SW/W and canonical two-saw source27')
        manifest['review']['limitations']=[
            'Native generated raster, indexed pixel grid unverified. Palette/partial-alpha warnings retained.',
            'Offline GIF phase/timing review, not live playback. Unity and runtime verification pending.']
        if 'Dentes' in name:
            manifest['review']['checklist']=dict(adultIdentityComparedToKatana=True,compactBrownHair=True,
                neutralNormal=True,longSleeves=True,twoArmsTwoHands=True,exactlyTwoWheelSaws=True,
                fullFigures=True,distinctPreparationCrossedContactRecovery=True,
                directionsObserved=['SW','W'],anatomyNotRepairedByPpu=True)
        pipeline.write(STAGE/(name+'.json'),manifest)
        pages=[]
        if 'Dentes' in name:
            fragment=json.loads((OLD/(name+'-pages.json')).read_text(encoding='utf-8'))
            for page in fragment['pages']:page['status']='visual-reviewed'
            pipeline.write(OUT/(name+'-pages.json'),fragment)
            pages=fragment['pages']
        qa=pipeline.qa(name)
        deliveries.append(dict(name=name,png=(STAGE/(name+'.png')).relative_to(ROOT).as_posix(),
            json=(STAGE/(name+'.json')).relative_to(ROOT).as_posix(),figures=len(manifest['frames']),
            clips=len(manifest['clips']),imageSha256=manifest['imageSha256'],sourcePath=manifest['sourcePath'],
            sourceSha256=manifest['sourceSha256'],status='visual-reviewed',qa=qa,pages=pages,
            imported=False,runtimeVerified=False,consumer='DarklightMotorcycleRedesignBank' if 'Join' in name else 'DarklightWeaponRedesignBank exact timed Attack pages weapon3'))
    pipeline.write(OUT/'recovered-import-job.json',dict(schemaVersion=1,date='2026-10-31',
        stagingOnly=True,assetsModified=False,unityExecuted=False,catalogMergePerformed=False,
        importer='Darklight.Editor.DarklightWeaponRedesignImporter.ImportStandalone',deliveries=deliveries))
    print(json.dumps(dict(recovered=[dict(name=d['name'],figures=d['figures'],qa=d['qa']) for d in deliveries])))

if __name__=='__main__':main()
