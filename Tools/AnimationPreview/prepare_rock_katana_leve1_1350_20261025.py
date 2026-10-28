"""Stage Rock Leve1; selectively register five repaired WHOLE figures only."""
from pathlib import Path
import hashlib
import json
import statistics
import sys
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as p

ART=ROOT/'ArtSources/WeaponRedesign20261025Retomada1350/katana-normal'
OUT=ROOT/'outputs/redesign-armas-20261025-retomada1350/katana-normal'
p.ART,p.RES,p.OUT=ART,OUT,OUT/'qa'
TIMINGS=[55,75,70,70]


def rows(directions):
    result=[]
    for row,d in enumerate(directions):
        bank='Cardinal' if d%2==0 else 'Diagonal'
        offset=(d//2)*4
        result.append(dict(sourceRow=row,sourceIndices=list(range(row*4,row*4+4)),direction=d,action='Attack',
                           attackKey='Leve1',loop=False,sourceSpriteNames=[f'Today_RockAttackKatanaYoung{bank}_{offset+i}' for i in range(4)]))
    return result


def job(name,source,directions,anchors):
    return dict(name=name,source=str(source),skin=1,weapon=0,form=0,columns=4,sourceRows=len(directions),minimumArea=10000,
                rows=rows(directions),supportAnchors={str(i):dict(anchor=a,method='Manual projected boot support in intact whole figure.') for i,a in enumerate(anchors)})


def mixed_back():
    # Each registration retains every nonzero RGBA source pixel. Selection is by
    # complete frame, never by head/face/limb/weapon. The seven good v1 figures stay exact.
    anchors=[[205,315],[560,321],[955,326],[1361,324],[190,661],[570,661],[961,662],[1338,663],
             [168,985],[537,975],[926,976],[1331,977]]
    sources=['RockKatanaNormalLeve1NW_N_NE-v1-source.png','RockKatanaNormalLeve1NW_N_NE-v2-repair-source.png']
    heads=[[47,53,63,56,382,385,387,387,704,698,707,699],
           [47,52,62,55,382,383,385,383,706,695,701,698]]
    soles=[[328,338,340,341,663,665,665,665,991,989,991,991],
           [328,338,341,340,664,666,667,667,1000,990,992,989]]
    raw=OUT/'preselection'
    oldres=p.RES;p.RES=raw
    regs=[]
    for i,source in enumerate(sources):
        m,_=p.prepare(job('RockKatanaBackRaw'+str(i),ART/source,[3,4,5],anchors))
        regs.append((m,Image.open(raw/m['image']).convert('RGBA')))
    p.RES=oldres
    chosen=[1 if i in [0,1,2,3,8] else 0 for i in range(12)]
    cw=max(f['rect'][2] for m,_ in regs for f in m['frames'])+16
    ch=max(f['rect'][3] for m,_ in regs for f in m['frames'])+16
    canvas=Image.new('RGBA',(cw*4,ch*3))
    supports,measures,provenance={},{},[]
    for i,which in enumerate(chosen):
        m,atlas=regs[which];f=m['frames'][i]
        x,y,w,h=f['rect'];frame=atlas.crop((x,y,x+w,y+h))
        dx,dy=(i%4)*cw+8,(i//4)*ch+8
        canvas.paste(frame,(dx,dy))
        assert np.array_equal(np.array(canvas.crop((dx,dy,dx+w,dy+h))),np.array(frame))
        ox,oy=f['sourceOffsetInFrame'];sx,sy,_,_=f['sourceRect']
        supports[str(i)]=dict(anchor=[dx+ox+anchors[i][0]-sx,dy+oy+anchors[i][1]-sy],
                                 method='Exact coordinate transfer of intact original/repaired full-figure support; no anatomical changes.')
        measures[i]=(dy+oy+heads[which][i]-sy,dy+oy+soles[which][i]-sy)
        provenance.append(dict(frameIndex=i,sourcePath=(ART/sources[which]).relative_to(ROOT).as_posix(),
                               sourceSha256=p.sha(ART/sources[which]),sourceFrameIndex=i,
                               sourceRegistrationFrame=f['rect'],wholeFrameRect=[dx,dy,w,h],
                               selectedCompleteFramePixelSha256=hashlib.sha256(frame.tobytes()).hexdigest(),
                               rgbaExact=True,repaired=bool(which)))
    source=ART/'RockKatanaNormalLeve1NW_N_NE-selected-whole-figures-source.png'
    canvas.save(source)
    record=dict(schemaVersion=1,method='Whole-figure technical source registration only; no anatomical assembly, resizing, mirroring, recoloring or warping.',
                repairedWholeFigureIndices=[0,1,2,3,8],preservedOriginalWholeFigureIndices=[4,5,6,7,9,10,11],
                everyChosenFrameRgbaExact=True,sourcePath=source.relative_to(ROOT).as_posix(),sourceSha256=p.sha(source),frames=provenance)
    p.write(OUT/'rock-leve1-back-selective-provenance.json',record)
    j=job('RockKatanaNormalLeve1NW_N_NE1350',source,[3,4,5],[[0,0]]*12)
    j['supportAnchors']=supports
    return j,measures,record


def main():
    back,bmeasures,provenance=mixed_back()
    specs=[
        (job('RockKatanaNormalLeve1S_SW_W1350',ART/'RockKatanaNormalLeve1S_SW_W-v1-source.png',[0,1,2],
             [[186,336],[601,336],[963,337],[1349,337],[190,660],[603,659],[981,656],[1380,659],
              [186,991],[596,974],[1005,979],[1394,979]]),
         {i:(h,s) for i,(h,s) in enumerate(zip([62,56,82,56,404,392,412,400,727,718,733,718],
                                               [338,338,338,339,670,664,660,664,1002,982,986,986]))}),
        (back,bmeasures),
        (job('RockKatanaNormalLeve1E_SE1350',ART/'RockKatanaNormalLeve1E_SE-v1-source.png',[6,7],
             [[208,420],[645,420],[1111,421],[1572,420],[223,847],[664,844],[1109,841],[1575,849]]),
         {i:(h,s) for i,(h,s) in enumerate(zip([87,88,94,83,501,494,500,498],[423,423,423,423,857,854,849,854]))}),
    ]
    pages,jobs,results=[],[],[]
    for j,measures in specs:
        m,fragment=p.prepare(j)
        measurement=[dict(sourceFrameIndex=i,headY=h,soleY=s,anatomyHeight=s-h) for i,(h,s) in measures.items()]
        ppu=statistics.median(x['anatomyHeight'] for x in measurement[:4])/1.98
        m.update(status='visual-reviewed',visualReviewed=True,imported=False,runtimeVerified=False,pixelsPerUnit=ppu,
                 originalSourcePath=Path(j['source']).relative_to(ROOT).as_posix(),
                 ppuProvenance=dict(candidateAnatomical=ppu,targetHeightUnits=1.98,measurements=measurement,
                                    method='Manual crown-to-sole, excludes raised blade/trail; one PPU per source sheet.',
                                    uncertaintyPixels=[5,12],limitation='Crouch and projected stance change apparent body height.'),
                 previewTimingsMs=TIMINGS,
                 rearAnatomyReview='Head/nuca/neck/shoulders/torso/hips/feet reviewed as one whole figure; no owl-like frontal head above rear torso.')
        if j['name']==back['name']:
            m['wholeFigureSelectionProvenance']='rock-leve1-back-selective-provenance.json'
            m['preservedOriginalCompleteFrames']=7
        for i,f in enumerate(m['frames']):f['durationMs']=TIMINGS[i%4]
        for page in fragment:page.update(status='visual-reviewed',pixelsPerUnit=ppu,stagingOnly=True,visualReviewed=True,imported=False,runtimeVerified=False)
        p.write(OUT/(j['name']+'.json'),m)
        p.write(OUT/(j['name']+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=fragment))
        qa=p.qa(j['name'])
        results.append(dict(name=j['name'],wholeFigures=len(m['frames']),sourcePath=m['sourcePath'],sourceSha256=m['sourceSha256'],
                            imageSha256=m['imageSha256'],pixelsPerUnit=ppu,sourceRgbaExactReconstruction=m['sourceRgbaExactReconstruction'],qa=qa))
        pages+=fragment;jobs.append(j)
    idlejobs=json.loads((OUT/'rock-katana-normal-idle-jobs.json').read_text(encoding='utf-8'))['jobs']
    p.write(OUT/'rock-katana-normal-leve1-pages.json',dict(schemaVersion=1,stagingOnly=True,pages=pages))
    p.write(OUT/'rock-katana-normal-leve1-jobs.json',dict(schemaVersion=1,stagingOnly=True,jobs=jobs))
    p.write(ROOT/'ArtSources/WeaponRedesign20261023/import-jobs-katana-rock-1350.json',dict(schemaVersion=1,jobs=idlejobs+jobs))
    summary=dict(schemaVersion=1,date='2026-10-25',session='13h50',skin=1,weapon=0,form=0,status='visual-reviewed',
                 wholeFigures=32,pages=len(pages),imported=False,runtimeVerified=False,results=results,
                 preservedOriginalWholeFigures=7,repairedWholeFigures=5,
                 limits='Only Leve1. Other attacks/combos/actions/forms remain pending. No shared catalog/Unity/Play/audio modified.')
    p.write(OUT/'rock-katana-normal-leve1-summary.json',summary)
    print(json.dumps(dict(wholeFigures=32,pages=len(pages),qaErrors=sum(r['qa']['errors'] for r in results),
                          qaWarnings=sum(r['qa']['warnings'] for r in results),preservedOriginalWholeFigures=7,repairedWholeFigures=5)))


if __name__=='__main__':main()
