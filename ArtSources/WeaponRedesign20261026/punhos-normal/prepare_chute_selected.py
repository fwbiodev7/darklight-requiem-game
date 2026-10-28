"""Technical packing of complete figures; preserve eleven, replace one whole figure."""
from pathlib import Path
import hashlib
import json
import sys
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
sys.path.insert(0, str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipeline

SOURCE = Path(__file__).resolve().parent
OUT = ROOT/'outputs/retomada-20261026/punhos-normal'
STAGE = OUT/'source-extraction'
pipeline.ART = SOURCE
pipeline.RES = STAGE
pipeline.OUT = OUT
DIRS = ['S','SW','W','NW']
NAME = 'ReqFistsNormalChuteSSWWNW20261026Selected'

def job(name, source):
    return dict(name=name,source=str(source),skin=0,weapon=2,form=0,columns=3,sourceRows=4,minimumArea=15000,
        rows=[dict(sourceRow=d,direction=d,action='Attack',attackKey='Chute Quebra-Osso',
                   sourceSpriteNames=[f'Ruin_Requiem_Normal_{DIRS[d]}_Combat_{p:02}' for p in (4,5,11)]) for d in range(4)])

def digest(image):
    box=image.getchannel('A').getbbox()
    return hashlib.sha256(image.crop(box).tobytes()).hexdigest(), list(image.crop(box).size)

def write(path,value):
    path.parent.mkdir(parents=True,exist_ok=True)
    path.write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')

def main():
    originals=[SOURCE/'ReqFistsNormalChuteSSWWNW20261026V1-source.png',
               SOURCE/'ReqFistsNormalChuteSSWWNW20261026V2-repair-source.png']
    manifests=[]
    for number,source in enumerate(originals):
        manifest,_=pipeline.prepare(job(NAME+'Extraction'+str(number),source))
        manifests.append(manifest)
    images=[Image.open(STAGE/m['image']).convert('RGBA') for m in manifests]
    figures=[]
    extents=[]
    for index in range(12):
        selected=1 if index==1 else 0
        frame=manifests[selected]['frames'][index]
        x,y,w,h=frame['rect']
        figure=images[selected].crop((x,y,x+w,y+h))
        anchor=(round(frame['pivot'][0]*w),round((1-frame['pivot'][1])*h))
        figures.append((figure,anchor,selected,frame))
        extents.append((anchor[0],anchor[1],w-anchor[0],h-anchor[1]))
    left,top,right,bottom=[max(row[i] for row in extents)+16 for i in range(4)]
    cw,ch=left+right,top+bottom
    selected_source=Image.new('RGBA',(cw*3,ch*4))
    proofs=[]
    for index,(figure,anchor,selected,frame) in enumerate(figures):
        offset=(index%3*cw+left-anchor[0],index//3*ch+top-anchor[1])
        selected_source.paste(figure,offset)
        before,size=digest(figure)
        check=selected_source.crop((offset[0],offset[1],offset[0]+figure.width,offset[1]+figure.height))
        after,_=digest(check)
        if before!=after:raise ValueError('Whole-figure pixel proof failed '+str(index))
        proofs.append(dict(index=index,originalSource=originals[selected].relative_to(ROOT).as_posix(),
                           originalSourceSha256=pipeline.sha(originals[selected]),sourceFrameIndex=index,
                           completeFigurePixelSha256=before,tightCanvas=size,pixelsUnchanged=True,
                           entireFigureReplacement=(index==1)))
    clean_source=SOURCE/(NAME+'-source.png')
    selected_source.save(clean_source)
    final_job=job(NAME,clean_source)
    final_job['provenance']=dict(wholeFiguresOnly=True,preservedOriginalFigures=11,repairedWholeFigures=1,frames=proofs)
    final_job['visualFindings']=['S contact repaired as a complete figure to kick toward the viewer.',
        'SW/W/NW have distinct front-left/profile/back-left anatomy; NW hides face.',
        'Normal dark brown canonical hair; neutral steel gauntlets/greaves; no transformation aura.',
        'Partial coverage only: second light, four directions; no new forms.']
    final_job['calibrationGuide']=dict(pixelsPerUnit=327/1.98,
        method='Frontal supported whole-character anatomy (hair y31 to sole y358=327px), excluding sideways kick reach; one PPU for all twelve figures.',
        canonicalReference='ArtSources/WeaponRedesign20261023/ReqFistsNormalDiretoSSWW20261025V2-source.png',
        canonicalWorldBodyHeight=1.98,worldVisualScale=1.325,individualResizing=False)
    pipeline.RES=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
    manifest,pages=pipeline.prepare(final_job)
    ppu=final_job['calibrationGuide']['pixelsPerUnit']
    manifest['pixelsPerUnit']=ppu
    manifest['pixelsPerUnitCalibration']=final_job['calibrationGuide']
    manifest['provenance']=final_job['provenance']
    manifest['status']='candidate'
    manifest['review']=dict(wholeSourceSeen=True,packedContactSeen=False,motionSeen=False,
        humanApproved=False,unityImported=False,gameplayVerified=False,findings=final_job['visualFindings'])
    for index,frame in enumerate(manifest['frames']):
        frame['durationMs']=[75,85,160][index%3]
        frame['phase']=['anticipation','contact','recovery'][index%3]
    for page in pages:
        page['pixelsPerUnit']=ppu;page['status']='candidate'
    pipeline.write(pipeline.RES/(NAME+'.json'),manifest)
    write(SOURCE/(NAME+'-job.json'),final_job)
    write(OUT/'catalog-fragment.json',dict(schemaVersion=1,complete=False,pages=pages))
    write(OUT/'whole-figure-preservation-proof.json',dict(wholeFiguresOnly=True,preserved11=True,repairedWholeFigureIndex=1,
        anatomicalAssembly=False,individualResizing=False,proofs=proofs))
    qa=pipeline.qa(NAME)
    write(OUT/'prepared.json',dict(name=NAME,frames=12,clips=4,directions=DIRS,attackKey='Chute Quebra-Osso',
        sourcePhases=[4,5,11],durationsMs=[75,85,160],pixelsPerUnit=ppu,qa=qa,wholeFigurePixelProof=True,
        visualReviewed=False,humanApproved=False,unityImported=False,runtimeVerified=False))
    print(json.dumps(dict(name=NAME,frames=12,clips=4,pixelsPerUnit=ppu,qa=qa)))

if __name__=='__main__':main()
