"""Bounded Pesado staging; complete figures only; no Assets or shared writes."""
from pathlib import Path
import hashlib,json,statistics,sys
from PIL import Image
ROOT=Path(__file__).resolve().parents[3]
sys.path.insert(0,str(ROOT/'Tools/AnimationPreview'))
import prepare_weapon_redesign as pipe
from whole_figure_components import identify
ART=ROOT/'ArtSources/WeaponRedesign20261102/katana-pesado'
OUT=ROOT/'outputs/retomada-20261102/katana-pesado'
pipe.ART=ART
pipe.RES=OUT/'staged'
pipe.OUT=OUT/'qa'
DURS=[100,110,100,100]
PHASES=['anticipation','active-impact','follow-through','recovery']
SPECS=[
dict(name='ReqKatanaNormalPesadoS_SW_W20261102V1',source='ReqKatanaNormalPesadoS_SW_W20261102V1-source.png',
 directions=[0,1,2],groups=[[0,1,2,3],[4,5,6,7],[8,9,10,11]],
 supports=[[220,340],[607,340],[977,340],[1370,340],[192,675],[601,662],[985,668],[1378,675],[201,989],[615,978],[1007,989],[1396,994]],
 body=[(0,55,340),(3,54,340),(4,381,675),(7,380,675),(8,700,989),(11,699,994)]),
dict(name='ReqKatanaNormalPesadoNW_NE20261102V1',source='ReqKatanaNormalPesadoNW_N_NE20261102V1-source.png',
 directions=[3,5],groups=[[0,1,2,3],[8,9,10,11]],
 supports=[[151,332],[580,334],[980,334],[1329,337],[163,986],[583,988],[969,992],[1376,992]],
 body=[(0,66,332),(3,65,337),(8,721,986),(11,722,992)],
 excluded='All four complete N figures excluded: active pose repeats overhead Leve3 motif rather than distinct transverse heavy slash. No regeneration after criterion failure.'),
]
def write(p,x): pipe.write(p,x)
def main():
    refs=[
    'ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleS_SW_W20261025-source.png',
    'ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleNW_N_NE20261025-source.png',
    'ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdleE_SE20261025-source.png',
    'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqKatanaNormalLeve3S_SW_W20261028Selected.png',
    'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqKatanaNormalLeve3NW_N_NE20261101V1.png',
    'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqKatanaNormalLeve3E_SE20261028Selected.png',
    'ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png',
    'Assets/Darklight/Runtime/Character/DarklightWeaponRedesignBank.cs',
    'Assets/Darklight/Data/Attacks/Pesado.asset']
    write(ART/'references.json',dict(references=[dict(path=p,sha256=pipe.sha(ROOT/p)) for p in refs],
    identity='Fabius Requiem, Normal only; approved Katana idle and Leve3 current catalog; natural compact brown hair, dark red eyes, original skin/coat/blade; anatomical right-hand grip.',
    worldVisualScale=1.325,sourceResolution='Native generated raster. Indexed pixel grid is not verified; no resampling.',
    consumer=dict(runtimeKey='Pesado',startupMs=100,activeMs=110,recoveryMs=200,startupFrames=1,activeFrames=1,recoveryFrames=2,durationMs=DURS,
    sourceSpriteNames=[],note='TryResolveAttack selects phase counts, ignores durationMs for this path; explicit counts reproduce these timings. No damage contact or hitbox changed.')))
    jobs=[];pages_all=[];results=[]
    for spec in SPECS:
        indices=sum(spec['groups'],[])
        ppu=statistics.median(sole-hair for _,hair,sole in spec['body'])/1.98
        supports={str(i):dict(anchor=a,method='Manual projected midpoint of grounded boot support in intact source; no anatomy alteration.') for i,a in zip(indices,spec['supports'])}
        rows=[dict(sourceRow=g[0]//4,sourceIndices=g,direction=d,action='Attack',attackKey='Pesado',loop=False,durationMs=DURS,startupFrames=1,activeFrames=1)
              for d,g in zip(spec['directions'],spec['groups'])]
        job=dict(name=spec['name'],source=str(ART/spec['source']),preservedSourceName=spec['source'],
                 skin=0,weapon=0,form=0,columns=4,sourceRows=3,minimumArea=10000,rows=rows,supportAnchors=supports,
                 excludedReason=spec.get('excluded',''),startupFrames=1,activeFrames=1,durationMs=DURS)
        manifest,pages=pipe.prepare(job)
        manifest.update(status='candidate',visualReviewed=False,imported=False,runtimeVerified=False,
                        pixelsPerUnit=ppu,worldVisualScale=1.325,attackKey='Pesado',startupFrames=1,activeFrames=1,
                        durationMs=DURS,stagingOnly=True,gameplayContactVerified=False,
                        ppuProvenance=dict(method='Manual hair-to-sole on intact standing/anticipation figures; raised swords excluded; one PPU per sheet.',
                        targetBodyHeightUnits=1.98,measurements=[dict(sourceFrameIndex=i,hairY=h,soleY=s,bodyHeight=s-h) for i,h,s in spec['body']],
                        limitation='No Unity or gameplay test. Crouching does not get scaled.'),
                        reviewFindings=['New lateral/diagonal blade travel with low/outside windup and grounded weight transfer; original identity/outfit retained.',
                        'Full figures only; no mirror, rotation, warp, component transplant or recoloring.',
                        'Declared follow-through is visual recovery; live contact remains solely 100-210ms in existing gameplay.'])
        manifest['notes']=[n for n in manifest['notes'] if 'legacy contact timing' not in n]
        manifest['notes'].append('New Pesado timing follows explicit startupFrames=1, activeFrames=1; sourceSpriteNames stays empty. Native generated raster, not a verified indexed pixel grid.')
        for i,f in enumerate(manifest['frames']):
            f.update(durationMs=DURS[i%4],phase=PHASES[i%4],attackKey='Pesado',gameplayPhase=['startup','active','recovery','recovery'][i%4])
        for p in pages:
            p.update(status='candidate',visualReviewed=False,stagingOnly=True,imported=False,runtimeVerified=False,
                     pixelsPerUnit=ppu,startupFrames=1,activeFrames=1,durationMs=DURS,sourceSpriteNames=[])
        write(pipe.RES/(spec['name']+'.json'),manifest)
        write(pipe.RES/(spec['name']+'.pages.json'),dict(schemaVersion=1,stagingOnly=True,pages=pages))
        qa=pipe.qa(spec['name'])
        results.append(dict(name=spec['name'],sourceSha256=manifest['sourceSha256'],imageSha256=manifest['imageSha256'],
                            frames=len(manifest['frames']),directions=[pipe.DIRECTIONS[d] for d in spec['directions']],
                            exactRgba=manifest['sourceRgbaExactReconstruction'],pixelsPerUnit=ppu,qa=qa))
        jobs.append(job);pages_all.extend(pages)
    write(ART/'import-jobs-pesado.json',dict(schemaVersion=1,stagingOnly=True,jobs=jobs))
    write(OUT/'pages-fragment.json',dict(schemaVersion=1,status='candidate',stagingOnly=True,pages=pages_all))
    summary=dict(date='2026-11-02',status='candidate',generatedSheets=2,generatedWholeFigures=24,preparedWholeFigures=20,
                 visuallyInspectedFigures=24,visualReviewed=False,imported=False,runtimeVerified=False,stagingOnly=True,
                 preparedDirections=['S','SW','W','NW','NE'],pendingDirections=['N','E','SE'],
                 failedCriterion=dict(direction='N',source='ReqKatanaNormalPesadoNW_N_NE20261102V1-source.png',
                 sourceFrameIndices=[4,5,6,7],reason='Active pose repeats overhead Leve3 motif; heavy-transverse differentiation failed. Whole N clip excluded.'),
                 callsUsed=2,callsMaximum=3,newGenerationStopped=True,
                 pending=['Final PNG/GIF/contact visual check before visual-reviewed status.','N requires newly drawn complete clip; E/SE not generated after stop criterion.','Unity import, in-game phase and transition checks not executed.'],
                 durationMs=DURS,startupFrames=1,activeFrames=1,recoveryFrames=2,totalMs=sum(DURS),results=results,
                 limitation='Partial Pesado delivery only. Does not complete Fabius, other actions, transformations, import or runtime.')
    write(OUT/'checkpoint.json',summary)
    print(json.dumps(dict(prepared=20,generated=24,clips=5,errors=sum(r['qa']['errors'] for r in results),
                         warnings=sum(r['qa']['warnings'] for r in results),status='candidate')))
if __name__=='__main__':main()

