"""Replace ONE complete figure, prove eleven unchanged, and stage the north clip."""
from pathlib import Path
import json
import sys
from PIL import Image

ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
sys.path.insert(0,str(ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts'))
import prepare_weapon_redesign as pipeline
import replace_frames

ART=Path(__file__).resolve().parent
OUT=ROOT/'outputs/retomada-20261027/punhos-normal/north-repair'
NAME='ReqFistsNormalChuteN20261027Selected'
SOURCES=[ART/'ReqFistsNormalChuteN_NE_E_SE20261027V1-source.png',ART/'ReqFistsNormalChuteN20261027V2-repair-source.png']
pipeline.ART=ART
pipeline.RES=OUT/'extraction-staging'
pipeline.OUT=OUT/'qa'

def rows(all_directions=True):
    return [dict(sourceRow=i,direction=i+4,action='Attack',attackKey='Chute Quebra-Osso',
        sourceSpriteNames=[f'Ruin_Requiem_Normal_{["N","NE","E","SE"][i]}_Combat_{phase:02}' for phase in (4,5,11)])
        for i in range(4 if all_directions else 1)]

def job(name,source,all_directions=True):
    return dict(name=name,source=str(source),skin=0,weapon=2,form=0,columns=3,sourceRows=4 if all_directions else 1,
                minimumArea=15000,rows=rows(all_directions))

def main():
    manifests=[]
    for index,source in enumerate(SOURCES):
        manifest,_=pipeline.prepare(job(NAME+'Original'+str(index),source))
        manifests.append(manifest)
    atlas=[Image.open(pipeline.RES/m['image']).convert('RGBA') for m in manifests]
    first,repair=[m['frames'][1] for m in manifests]
    x,y,w,h=repair['rect'];repair_canvas=atlas[1].crop((x,y,x+w,y+h))
    bounds=repair_canvas.getchannel('A').getbbox()
    whole=repair_canvas.crop(bounds)
    repair_anchor=(round(repair['pivot'][0]*w)-bounds[0],round((1-repair['pivot'][1])*h)-bounds[1])
    _,_,width,height=first['rect']
    anchor=(round(first['pivot'][0]*width),round((1-first['pivot'][1])*height))
    offset=(anchor[0]-repair_anchor[0],anchor[1]-repair_anchor[1])
    if min(offset)<0 or offset[0]+whole.width>width or offset[1]+whole.height>height:
        raise ValueError('Complete repaired north figure does not fit original frame; do not resize or cut anatomy')
    replacement=Image.new('RGBA',(width,height));replacement.paste(whole,offset)
    replacement_path=OUT/'whole-north-contact.png';replacement_path.parent.mkdir(parents=True,exist_ok=True)
    replacement.save(replacement_path)
    plan=OUT/'whole-figure-replacement-plan.json'
    pipeline.write(plan,dict(wholeFramesConfirmed=True,replacements=[dict(frameId=first['id'],image=replacement_path.name)]))
    proof=replace_frames.replace(pipeline.RES/(manifests[0]['image'].removesuffix('.png')+'.json'),plan,OUT/'fixed-twelve')
    fixed=ART/(NAME+'TwelveWholeFigures-source.png')
    fixed.write_bytes((OUT/'fixed-twelve/sheet.png').read_bytes())
    extract=job(NAME+'NorthExtraction',fixed)
    extract['rows']=rows(False)
    extract['excludedReason']='Other eleven sprites are proved unchanged; this descriptor extracts only north, not duplicate NE/E/SE clips.'
    extracted,_=pipeline.prepare(extract)
    clean_source=ART/(NAME+'-source.png')
    clean_source.write_bytes((pipeline.RES/extracted['image']).read_bytes())
    final_job=job(NAME,clean_source,False)
    ppu=158.58585858585857
    final_job['calibrationGuide']=dict(pixelsPerUnit=ppu,worldVisualScale=1.325,targetBodyWorldHeight=1.98,
        method='Same single anatomical PPU as the nine NE/E/SE figures from this generated sheet; no individual resizing.',
        nativeNorthHairToSole=[306,310,306],poseVariation='Foreshortened kick/coat can occlude the far leg; use the shared body scale.')
    final_job['provenance']=dict(wholeFigureReplacementOnly=True,repairedOriginalFrameIndex=1,preservedOriginalFigures=11,
        sources=[dict(path=p.relative_to(ROOT).as_posix(),sha256=pipeline.sha(p)) for p in SOURCES],
        proof=(OUT/'fixed-twelve/replacement-proof.json').relative_to(ROOT).as_posix(),anatomicalAssembly=False)
    pipeline.RES=OUT/'payload'
    manifest,pages=pipeline.prepare(final_job)
    manifest.update(pixelsPerUnit=ppu,status='visual-reviewed',pixelsPerUnitCalibration=final_job['calibrationGuide'],provenance=final_job['provenance'])
    for index,frame in enumerate(manifest['frames']):
        frame['durationMs']=[75,85,160][index];frame['phase']=['anticipation','contact','recovery'][index]
    manifest['review']=dict(wholeSourceSeen=True,packedContactSeen=False,motionSeen=False,humanApproved=False,
        technicalCoordinatorAccepted=True,unityImported=False,runtimeVerified=False,
        limitation='Far kicking leg is strongly foreshortened and mostly hidden by torso/coat; contact readability is limited in this north view.',
        finding='North back/nape/spine coherent; incorrect westward extension removed. No further generation attempted.')
    for page in pages:page.update(pixelsPerUnit=ppu,status='visual-reviewed')
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    pipeline.write(ART/(NAME+'-job.json'),final_job)
    pipeline.write(OUT/'catalog-fragment.json',dict(schemaVersion=1,complete=False,pages=pages))
    qa=pipeline.qa(NAME)
    pipeline.write(OUT/'prepared.json',dict(name=NAME,selectedFigures=3,clips=1,directions=['N'],sourcePhases=[4,5,11],
        durationsMs=[75,85,160],pixelsPerUnit=ppu,worldVisualScale=1.325,preservedOriginalFigures=11,wholeFrameReplacementProof=proof,
        qa=qa,humanApproved=False,unityImported=False,runtimeVerified=False,contactReadabilityLimitedByOcclusion=True))
    print(json.dumps(dict(name=NAME,figures=3,clips=1,preserved=11,pixelsPerUnit=ppu,qa=qa)))

if __name__=='__main__':main()
