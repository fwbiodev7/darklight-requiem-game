"""Pack intact Heavy uppercut figures; shared catalog and Unity are coordinator-owned."""
import argparse
import json
import statistics

from PIL import Image
import prepare_weapon_redesign as pipeline
from whole_figure_components import identify

ROOT = pipeline.ROOT
ART = ROOT / 'ArtSources/WeaponRedesign20261027/punhos-heavy'
OUT = ROOT / 'outputs/retomada-20261027/punhos-heavy'
RES = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
TIMINGS = [140, 120, 280]
ATTACK = 'Ascensão da Ruína'
FIRST = 'ReqFistsNormalHeavySSWWNW20261027V1'
SECOND = 'ReqFistsNormalHeavyNNEESE20261027V1'


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--second',action='store_true')
    args = parser.parse_args()
    name = SECOND if args.second else FIRST
    directions = list(range(4,8)) if args.second else list(range(4))
    source = ART / (name+'-source.png')
    image = Image.open(source).convert('RGBA')
    figures, _ = identify(image,rows=4,columns=3,minimum_area=10000,alpha_threshold=128)
    assert len(figures)==12 and not any(f['touchesExtractionEdge'] for f in figures)
    if args.second:
        guide=json.loads((ART/(name+'-guides.json')).read_text(encoding='utf-8'))
    else:
        guide=dict(supportAnchors=[[219,368],[567,369],[925,369],
                                  [224,721],[574,719],[928,727],
                                  [220,1073],[565,1073],[936,1074],
                                  [215,1417],[568,1419],[934,1404]],
                   anatomyMeasurements=[dict(sourceFrameIndex=i,hairY=y,soleY=figures[i]['grid'][3])
                                        for i,y in [(1,82),(4,440),(7,774),(10,1111)]])
        pipeline.write(ART/(name+'-guides.json'),guide)
    heights=[m['soleY']-m['hairY'] for m in guide['anatomyMeasurements']]
    ppu=statistics.median(heights)/1.98
    calibration=dict(method='Manual whole-source hair-to-grounded-sole anatomy of upright contact poses; raised gauntlet excluded.',
                     measurements=guide['anatomyMeasurements'],heights=heights,bodyHeightUnits=1.98,
                     worldVisualScale=1.325,measurementUncertaintyPixels=8,individualResizing=False,
                     visualScaleCheckPending=True)
    rows=[dict(sourceRow=row,direction=d,action='Attack',attackKey=ATTACK,loop=False,
               sourceSpriteNames=[f'Ruin_Requiem_Normal_{pipeline.DIRECTIONS[d]}_Combat_{phase:02}' for phase in [7,8,11]])
          for row,d in enumerate(directions)]
    job=dict(name=name,source=str(source),preservedSourceName=source.name,skin=0,weapon=2,form=0,
             columns=3,sourceRows=4,minimumArea=10000,rows=rows,
             supportAnchors={str(i):dict(anchor=anchor,method='Whole-body stance center over ground plane; manual full-figure review; no arm/weapon anchoring.')
                             for i,anchor in enumerate(guide['supportAnchors'])})
    pipeline.ART,pipeline.RES,pipeline.OUT=ART,RES,OUT
    manifest,pages=pipeline.prepare(job)
    manifest.update(pixelsPerUnit=ppu,status='visual-reviewed',attackKey=ATTACK,
                    pixelsPerUnitCalibration=calibration,agentReviewed=True,coordinatorReviewed=False,
                    humanApproved=False,unityImported=False,runtimeVerified=False)
    manifest['quality']['maxHeightRatio']=1.5
    for i,frame in enumerate(manifest['frames']):
        frame.update(durationMs=TIMINGS[i%3],phase=['anticipation','contact','recovery'][i%3],attackKey=ATTACK)
    manifest['notes'] += ['Authored Heavy uppercut: loaded low fist -> rising strike with hip/shoulder torque -> guarding recovery.',
                          'Normal neutral silver equipment on both hands and boots. No transformation colors, generated VFX or katana.',
                          'Combat timings .14 startup/.12 active/.28 recovery; exact legacy source slots7/8/11.',
                          'Manual anatomy excludes the raised gauntlet; source rasters and every selected nonzero RGBA pixel preserved.',
                          'Reviewed by art agent; coordinator handles catalog activation and Unity/runtime verification.']
    for page in pages:page.update(pixelsPerUnit=ppu,status='visual-reviewed')
    pipeline.write(RES/(name+'.json'),manifest)
    pipeline.write(ART/(name+'-job.json'),job)
    pipeline.write(OUT/(name+'-catalog-fragment.json'),dict(schemaVersion=1,complete=False,pages=pages))
    qa=pipeline.qa(name)
    # APNG preserves the gameplay140/120/280ms contract without GIF timing approximation.
    atlas=Image.open(RES/(name+'.png')).convert('RGBA')
    preview=OUT/name/'preview'
    for row,d in enumerate(directions):
        animation=[]
        for frame in manifest['frames'][row*3:row*3+3]:
            x,y,w,h=frame['rect'];canvas=Image.new('RGBA',(768,768))
            canvas.alpha_composite(atlas.crop((x,y,x+w,y+h)),(384-round(frame['pivot'][0]*w),650-round((1-frame['pivot'][1])*h)))
            animation.append(canvas)
        animation[0].save(preview/('heavy-'+pipeline.DIRECTIONS[d]+'-exact540ms.png'),save_all=True,append_images=animation[1:],
                          duration=TIMINGS,loop=0,disposal=1,blend=0)
    references=['Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/'+n+'.png'
                for n in ['ReqFistsNormalDiretoSSWW20261025V2','ReqFistsNormalChuteSSWWNW20261026Selected','ReqKatanaNormalNeutralIdleS_SW_W20261025']]
    if args.second:
        references=['ArtSources/WeaponRedesign20261027/punhos-heavy/'+FIRST+'-source.png',
                    'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalChuteNE_E_SE20261027Selected.png',
                    'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalDiretoN_NE20261025V1.png',
                    'ArtSources/WeaponRedesign20261027/katana-normal/ReqKatanaNormalWalkNW_N20261027Selected-source.png']
    receipt=dict(name=name,status='visual-reviewed',attackKey=ATTACK,directions=[pipeline.DIRECTIONS[d] for d in directions],
                 figures=12,clips=4,timingsMs=TIMINGS,totalMs=540,sourcePath=manifest['sourcePath'],sourceSha256=manifest['sourceSha256'],
                 imageSha256=manifest['imageSha256'],nativeSize=manifest['nativeSize'],sourceSize=manifest['sourceSize'],
                 pixelsPerUnit=ppu,exactWholeFigureRgba=manifest['sourceRgbaExactReconstruction'],sourcePixels=manifest['sourcePixels'],
                 rasterCleanup=False,anatomicalAssembly=False,generationCalls=1,agentReviewed=True,sourceWholeSheetSeen=True,
                 packedContactSeen=False,animationPlaybackSeen=False,humanApproved=False,unityImported=False,runtimeVerified=False,
                 qa=qa,references=[dict(path=p,sha256=pipeline.sha(ROOT/p)) for p in references])
    pipeline.write(OUT/(name+'-prepared.json'),receipt)
    print(json.dumps(receipt,ensure_ascii=False))


if __name__=='__main__':main()
